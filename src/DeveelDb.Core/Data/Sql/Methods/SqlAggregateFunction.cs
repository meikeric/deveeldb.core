﻿using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Services;

namespace Deveel.Data.Sql.Methods {
	public abstract class SqlAggregateFunction : SqlFunctionBase {
		public SqlAggregateFunction(SqlFunctionInfo functionInfo)
			: base(functionInfo) {
		}

		public override FunctionType FunctionType => FunctionType.Aggregate;

		protected virtual Task InitializeAsync(InitializeContext context) {
			return Task.CompletedTask;
		}

		protected abstract Task IterateAsync(IterateContext context);

		protected virtual Task MergeAsync(MergeContext context) {
			return Task.CompletedTask;
		}

		protected override async Task ExecuteContextAsync(MethodContext context) {
			var groupResolver = (context as IContext).Scope.Resolve<IGroupResolver>();
			if (groupResolver == null)
				throw new NotSupportedException($"Aggregate function {MethodInfo.MethodName} requires a group resolver in context");

			if (groupResolver.Size == 0) {
				context.SetResult(SqlObject.NullOf(MethodInfo.ReturnType));
				return;
			}

			var input = context.Argument(0);

			using (var seed = new InitializeContext(context, input)) {
				await InitializeAsync(seed);

				if (seed.Result != null)
					input = seed.Result;

				if (!seed.Iterate) {
					context.SetResult(input);
					return;
				}
			}


			SqlObject output;

			if (input is SqlReferenceExpression) {
				var reference = (SqlReferenceExpression)input;
				output = await AccumulateReference(context, reference.ReferenceName, groupResolver);
			} else {
				output = await AccumulateValues(context, input, groupResolver);
			}

			using (var aggregate = new MergeContext(context, output)) {
				await MergeAsync(aggregate);

				if (aggregate.Output != null)
					output = aggregate.Output;
			}

			context.SetResult(output);
		}

		private async Task<SqlObject> AccumulateValues(MethodContext context, SqlExpression input, IGroupResolver groupResolver) {
			SqlObject result = null;

			for (long i = 0; i < groupResolver.Size; i++) {
				SqlObject value;

				using (var reduce = context.Create("reduce")) {
					var resolver = groupResolver.GetResolver(i);
					reduce.RegisterInstance<IReferenceResolver>(resolver);

					var reduced = input.Reduce(reduce);
					if (reduced.ExpressionType != SqlExpressionType.Constant)
						throw new InvalidOperationException();

					value = ((SqlConstantExpression)reduced).Value;
				}

				using (var accumulate = new IterateContext(context, result, value)) {
					await IterateAsync(accumulate);

					if (accumulate.Result == null)
						throw new InvalidOperationException();

					result = accumulate.Result;
				}
			}

			return result;
		}

		private async Task<SqlObject> AccumulateReference(MethodContext context, ObjectName refName, IGroupResolver groupResolver) {
			SqlObject result = null;

			for (long i = 0; i < groupResolver.Size; i++) {
				var rowValue = groupResolver.ResolveReference(refName, i);
				var current = rowValue;

				using (var accumulate = new IterateContext(context, result, current)) {

					await IterateAsync(accumulate);

					if (accumulate.Result == null)
						throw new InvalidOperationException();

					result = accumulate.Result;
				}
			}

			return result;
		}
	}
}