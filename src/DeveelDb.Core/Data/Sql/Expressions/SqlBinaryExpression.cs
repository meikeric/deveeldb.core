﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlBinaryExpression : SqlExpression {
		internal SqlBinaryExpression(SqlExpressionType expressionType, SqlExpression left, SqlExpression right)
			: base(expressionType) {
			if (left == null)
				throw new ArgumentNullException(nameof(left));
			if (right == null)
				throw new ArgumentNullException(nameof(right));

			Left = left;
			Right = right;
		}

		public SqlExpression Left { get; }

		public SqlExpression Right { get; }

		public override bool CanReduce => true;

		private SqlExpression[] ReduceSides(IContext context) {
			var info = new List<BinaryEvaluateInfo> {
				new BinaryEvaluateInfo {Expression = Left, Offset = 0},
				new BinaryEvaluateInfo {Expression = Right, Offset = 1}
			}.OrderByDescending(x => x.Precedence);

			foreach (var evaluateInfo in info) {
				evaluateInfo.Expression = evaluateInfo.Expression.Reduce(context);
			}

			return info.OrderBy(x => x.Offset)
				.Select(x => x.Expression)
				.ToArray();
		}

		public override SqlExpression Reduce(IContext context) {
			var sides = ReduceSides(context);

			var left = sides[0];
			var right = sides[1];

			if (left.ExpressionType != SqlExpressionType.Constant)
				throw new SqlExpressionException("The reduced left side of a binary expression is not constant");
			if (right.ExpressionType != SqlExpressionType.Constant)
				throw new SqlExpressionException("The reduced right side of a binary expression is not constant.");

			var value1 = ((SqlConstantExpression)Left).Value;
			var value2 = ((SqlConstantExpression)Right).Value;

			var result = ReduceBinary(value1, value2);

			return Constant(result);
		}

		private SqlObject ReduceBinary(SqlObject left, SqlObject right) {
			switch (ExpressionType) {
				case SqlExpressionType.Add:
					return left.Add(right);
				case SqlExpressionType.Subtract:
					return left.Subtract(right);
				case SqlExpressionType.Multiply:
					return left.Multiply(right);
				case SqlExpressionType.Divide:
					return left.Divide(right);
				case SqlExpressionType.Modulo:
					return left.Modulo(right);
				case SqlExpressionType.GreaterThan:
					return left.GreaterThan(right);
				case SqlExpressionType.GreaterThanOrEqual:
					return left.GreaterThanOrEqual(right);
				case SqlExpressionType.LessThan:
					return left.LessThan(right);
				case SqlExpressionType.LessThanOrEqual:
					return left.LessOrEqualThan(right);
				case SqlExpressionType.Equal:
					return left.Equal(right);
				case SqlExpressionType.NotEqual:
					return left.NotEqual(right);
				case SqlExpressionType.Is:
					return left.Is(right);
				case SqlExpressionType.IsNot:
					return left.IsNot(right);
				case SqlExpressionType.And:
					return left.And(right);
				case SqlExpressionType.Or:
					return left.Or(right);
				case SqlExpressionType.XOr:
					return left.XOr(right);
				// TODO: ANY and ALL
				default:
					throw new SqlExpressionException($"The type {ExpressionType} is not a binary expression or is not supported.");
			}
		}

		#region BinaryEvaluateInfo

		class BinaryEvaluateInfo {
			public SqlExpression Expression { get; set; }
			public int Offset { get; set; }

			public int Precedence => Expression.Precedence;
		}

		#endregion
	}
}