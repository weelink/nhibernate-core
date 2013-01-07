using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Hql.Ast;
using NHibernate.Linq.Visitors;
using System.Linq;

namespace NHibernate.Linq.Functions
{
	internal class CompareGenerator : BaseHqlGeneratorForMethod
	{
		private static readonly HashSet<MethodInfo> ActingMethods = new HashSet<MethodInfo>
			{
				ReflectionHelper.GetMethodDefinition(() => string.Compare(null, null)),
				ReflectionHelper.GetMethodDefinition<string>(s => s.CompareTo(s)),
				ReflectionHelper.GetMethodDefinition<char>(x => x.CompareTo(x)),

				ReflectionHelper.GetMethodDefinition<byte>(x => x.CompareTo(x)),
				ReflectionHelper.GetMethodDefinition<sbyte>(x => x.CompareTo(x)),
				
				ReflectionHelper.GetMethodDefinition<short>(x => x.CompareTo(x)),
				ReflectionHelper.GetMethodDefinition<ushort>(x => x.CompareTo(x)),

				ReflectionHelper.GetMethodDefinition<int>(x => x.CompareTo(x)),
				ReflectionHelper.GetMethodDefinition<uint>(x => x.CompareTo(x)),

				ReflectionHelper.GetMethodDefinition<long>(x => x.CompareTo(x)),
				ReflectionHelper.GetMethodDefinition<ulong>(x => x.CompareTo(x)),

				ReflectionHelper.GetMethodDefinition<float>(x => x.CompareTo(x)),
				ReflectionHelper.GetMethodDefinition<double>(x => x.CompareTo(x)),
				ReflectionHelper.GetMethodDefinition<decimal>(x => x.CompareTo(x)),
			};


		internal static bool IsCompareMethod(MethodInfo methodInfo)
		{
			return ActingMethods.Contains(methodInfo);
		}


		public CompareGenerator()
		{
			SupportedMethods = ActingMethods.ToArray();
		}

		public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
		{
			// Instance CompareTo() or static string.Compare()?
			Expression lhs = arguments.Count == 1 ? targetObject : arguments[0];
			Expression rhs = arguments.Count == 1 ? arguments[0] : arguments[1];

			HqlExpression lhs1 = visitor.Visit(lhs).AsExpression();
			HqlExpression rhs1 = visitor.Visit(rhs).AsExpression();
			HqlExpression lhs2 = visitor.Visit(lhs).AsExpression();
			HqlExpression rhs2 = visitor.Visit(rhs).AsExpression();

			// CASE WHEN (table.[Name] = N'Foo') THEN 0
			//      WHEN (table.[Name] > N'Foo') THEN 1
			//      ELSE -1 END

			return treeBuilder.Case(
				new[]
					{
						treeBuilder.When(treeBuilder.Equality(lhs1, rhs1), treeBuilder.Constant(0)),
						treeBuilder.When(treeBuilder.GreaterThan(lhs2, rhs2), treeBuilder.Constant(1))
					},
				treeBuilder.Constant(-1));
		}
	}
}