﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.CSharp.Resolver {
	[TestFixture]
	public class MethodTests : ResolverTestBase {
		[Test]
		public void ParameterIdentityInNormalMethod()
		{
			string code = @"using System;
class TestClass {
	$int F(int i, int j) {
		return i + j;
	}$
}";
			
			var prep = PrepareResolver(code);
			var md = (MethodDeclaration)prep.Item2;

			var resolver = prep.Item1;
			var method = (IMethod)((MemberResolveResult)resolver.Resolve(md)).Member;
			IVariable i1 = method.Parameters.Single(p => p.Name == "i");
			IVariable j1 = method.Parameters.Single(p => p.Name == "j");

			var returnExpr = (BinaryOperatorExpression)md.Body.Children.OfType<ReturnStatement>().Single().Expression;
			var returnRR = (OperatorResolveResult)resolver.Resolve(returnExpr);

			IVariable i2 = ((LocalResolveResult)returnRR.Operands[0]).Variable;
			IVariable j2 = ((LocalResolveResult)returnRR.Operands[1]).Variable;

			Assert.IsTrue(ReferenceEquals(i1, i2));
			Assert.IsTrue(ReferenceEquals(i1, i2));
		}

		[Test]
		public void ParameterIdentityInPropertySetter()
		{
			string code = @"using System;
class TestClass {
	int myField;
	$int Prop {
		get { return myField; }
		set { myField = value; }
	}$
}";
			
			var prep = PrepareResolver(code);
			var pd = (PropertyDeclaration)prep.Item2;

			var resolver = prep.Item1;
			var property = (IProperty)((MemberResolveResult)resolver.Resolve(pd)).Member;
			IVariable value1 = property.Setter.Parameters.Single(p => p.Name == "value");

			var assignExpr = (AssignmentExpression)pd.Setter.Body.Children.OfType<ExpressionStatement>().Single().Expression;
			var assignRR = (OperatorResolveResult)resolver.Resolve(assignExpr);

			IVariable value2 = ((LocalResolveResult)assignRR.Operands[1]).Variable;

			Assert.IsTrue(ReferenceEquals(value1, value2));
		}

		[Test]
		public void ParameterIdentityInIndexerGetter()
		{
			string code = @"using System;
class TestClass {
	int[,] myField;
	$int this[int i, int j] {
		get { return myField[i, j]; }
		set { myField[i, j] = value; }
	}$
}";
			
			var prep = PrepareResolver(code);
			var id = (IndexerDeclaration)prep.Item2;

			var resolver = prep.Item1;
			var property = (IProperty)((MemberResolveResult)resolver.Resolve(id)).Member;
			IVariable i1 = property.Getter.Parameters.Single(p => p.Name == "i");
			IVariable j1 = property.Getter.Parameters.Single(p => p.Name == "j");

			var accessExpr = (IndexerExpression)id.Getter.Body.Children.OfType<ReturnStatement>().Single().Expression;
			var accessRR = (ArrayAccessResolveResult)resolver.Resolve(accessExpr);
			IVariable i2 = ((LocalResolveResult)accessRR.Indexes[0]).Variable;
			IVariable j2 = ((LocalResolveResult)accessRR.Indexes[1]).Variable;

			Assert.IsTrue(ReferenceEquals(i1, i2));
			Assert.IsTrue(ReferenceEquals(j1, j2));
		}

		[Test]
		public void ParameterIdentityInIndexerSetter()
		{
			string code = @"using System;
class TestClass {
	int[,] myField;
	$int this[int i, int j] {
		get { return myField[i, j]; }
		set { myField[i, j] = value; }
	}$
}";
			
			var prep = PrepareResolver(code);
			var id = (IndexerDeclaration)prep.Item2;

			var resolver = prep.Item1;
			var property = (IProperty)((MemberResolveResult)resolver.Resolve(id)).Member;
			IVariable i1 = property.Setter.Parameters.Single(p => p.Name == "i");
			IVariable j1 = property.Setter.Parameters.Single(p => p.Name == "j");
			IVariable value1 = property.Setter.Parameters.Single(p => p.Name == "value");

			var assignExpr = (AssignmentExpression)id.Setter.Body.Children.OfType<ExpressionStatement>().Single().Expression;
			var assignRR = (OperatorResolveResult)resolver.Resolve(assignExpr);
			var accessRR = (ArrayAccessResolveResult)assignRR.Operands[0];
			IVariable i2 = ((LocalResolveResult)accessRR.Indexes[0]).Variable;
			IVariable j2 = ((LocalResolveResult)accessRR.Indexes[1]).Variable;
			IVariable value2 = ((LocalResolveResult)assignRR.Operands[1]).Variable;

			Assert.IsTrue(ReferenceEquals(i1, i2));
			Assert.IsTrue(ReferenceEquals(j1, j2));
			Assert.IsTrue(ReferenceEquals(value1, value2));
		}

		[Test]
		public void ParameterIdentityInEventAdder()
		{
			string code = @"using System;
class TestClass {
	EventHandler myField;
	$event EventHandler Evt {
		add { myField += value; }
		remove { myField -= value; }
	}$
}";
			
			var prep = PrepareResolver(code);
			var ed = (CustomEventDeclaration)prep.Item2;

			var resolver = prep.Item1;
			var evt = (IEvent)((MemberResolveResult)resolver.Resolve(ed)).Member;
			IVariable value1 = evt.AddAccessor.Parameters.Single(p => p.Name == "value");

			var assignExpr = (AssignmentExpression)ed.AddAccessor.Body.Children.OfType<ExpressionStatement>().Single().Expression;
			var assignRR = (OperatorResolveResult)resolver.Resolve(assignExpr);

			IVariable value2 = ((LocalResolveResult)assignRR.Operands[1]).Variable;

			Assert.IsTrue(ReferenceEquals(value1, value2));
		}

		[Test]
		public void ParameterIdentityInEventRemover()
		{
			string code = @"using System;
class TestClass {
	EventHandler myField;
	$event EventHandler Evt {
		add { myField += value; }
		remove { myField -= value; }
	}$
}";
			
			var prep = PrepareResolver(code);
			var ed = (CustomEventDeclaration)prep.Item2;

			var resolver = prep.Item1;
			var evt = (IEvent)((MemberResolveResult)resolver.Resolve(ed)).Member;
			IVariable value1 = evt.RemoveAccessor.Parameters.Single(p => p.Name == "value");

			var assignExpr = (AssignmentExpression)ed.RemoveAccessor.Body.Children.OfType<ExpressionStatement>().Single().Expression;
			var assignRR = (OperatorResolveResult)resolver.Resolve(assignExpr);

			IVariable value2 = ((LocalResolveResult)assignRR.Operands[1]).Variable;

			Assert.IsTrue(ReferenceEquals(value1, value2));
		}
	}
}
