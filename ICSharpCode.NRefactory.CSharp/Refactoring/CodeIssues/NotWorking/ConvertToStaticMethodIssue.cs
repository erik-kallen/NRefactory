//
// ConvertToStaticMethodIssue.cs
//
// Author:
//       Ciprian Khlud <ciprian.mustiata@yahoo.com>
//
// Copyright (c) 2013 Ciprian Khlud
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System.Collections.Generic;
using ICSharpCode.NRefactory.Refactoring;
using ICSharpCode.NRefactory.Semantics;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.CSharp.Refactoring.ExtractMethod;

namespace ICSharpCode.NRefactory.CSharp.Refactoring
{
	[IssueDescription("Make this method static",
	                  Description = "This method doesn't use any instance members so it can be made static",
	                  Severity = Severity.Hint,
	                  IssueMarker = IssueMarker.Underline)]
    public class ConvertToStaticMethodIssue : ICodeIssueProvider
	{
		public IEnumerable<CodeIssue> GetIssues(BaseRefactoringContext context)
		{
			return new GatherVisitor(context).GetIssues();
		}

		private class GatherVisitor : GatherVisitorBase<ConvertToStaticMethodIssue>
		{
			public GatherVisitor(BaseRefactoringContext context)
                : base(context)
			{
			}

			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
			{
				var rr = ctx.Resolve(typeDeclaration);
				if (rr.Type.GetNonInterfaceBaseTypes().Any(t => t.Name == "MarshalByRefObject" && t.Namespace == "System"))
					return; // ignore MarshalByRefObject, as only instance method calls get marshaled
				
				base.VisitTypeDeclaration(typeDeclaration);
			}
			
			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
			{
				var context = ctx;
				if (methodDeclaration.HasModifier(Modifiers.Static) ||
				    methodDeclaration.HasModifier(Modifiers.Virtual) ||
				    methodDeclaration.HasModifier(Modifiers.Override) ||
				    methodDeclaration.HasModifier(Modifiers.New) ||
				    methodDeclaration.Attributes.Any())
					return;

				var body = methodDeclaration.Body;
				// skip empty methods
				if (!body.Statements.Any())
					return;

				if (body.Statements.Count == 1) {
					if (body.Statements.First () is ThrowStatement)
						return;
				}
					
				var resolved = context.Resolve(methodDeclaration) as MemberResolveResult;
				if (resolved == null)
					return;

				var isImplementingInterface = resolved.Member.ImplementedInterfaceMembers.Any();
				if (isImplementingInterface)
					return;

				if (StaticVisitor.UsesNotStaticMember(context, body))
					return;
				
				AddIssue(methodDeclaration.NameToken.StartLocation, methodDeclaration.NameToken.EndLocation,
				                     context.TranslateString(string.Format("Make '{0}' static", methodDeclaration.Name)),
				                     script => ExecuteScriptToFixStaticMethodIssue(script, context, methodDeclaration));
			}

			static void ExecuteScriptToFixStaticMethodIssue(Script script,
			                                                BaseRefactoringContext context,
			                                                                 AstNode methodDeclaration)
			{
				var clonedDeclaration = (MethodDeclaration)methodDeclaration.Clone();
				clonedDeclaration.Modifiers |= Modifiers.Static;
				script.Replace(methodDeclaration, clonedDeclaration);
				var rr = context.Resolve(methodDeclaration) as MemberResolveResult;
				//var method = (IMethod)rr.Member;
				//method.ImplementedInterfaceMembers.Any(m => methodGroupResolveResult.Methods.Contains((IMethod)m));

				/*script.DoGlobalOperationOn(rr.Member,
				                                       (fctx, fscript, fnode) => {
					DoStaticMethodGlobalOperation(fnode, fctx, rr, fscript); });*/
			}

			static void DoStaticMethodGlobalOperation(AstNode fnode, RefactoringContext fctx, MemberResolveResult rr,
			                                                           Script fscript)
			{
				if (fnode is MemberReferenceExpression) {
					var memberReference = new MemberReferenceExpression(
						new TypeReferenceExpression(fctx.CreateShortType(rr.Member.DeclaringType)),
						rr.Member.Name
					);
					fscript.Replace(fnode, memberReference);
				} else {
					var invoke = fnode as InvocationExpression;
					if (invoke == null)
						return;
					if ((invoke.Target is MemberReferenceExpression))
						return;
					var memberReference = new MemberReferenceExpression(
						new TypeReferenceExpression(fctx.CreateShortType(rr.Member.DeclaringType)),
						rr.Member.Name
					);
					fscript.Replace(invoke.Target, memberReference);
				}
			}
		}
	}
}

