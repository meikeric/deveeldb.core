﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public abstract class SqlMethod : ISqlFormattable {
		protected SqlMethod(SqlMethodInfo methodInfo) {
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			MethodInfo = methodInfo;
		}

		public SqlMethodInfo MethodInfo { get; }

		public abstract MethodType Type { get; }

		public bool IsFunction => Type == MethodType.Function;

		public bool IsProcedure => Type == MethodType.Procedure;

		public virtual bool IsSystem => true;

		public async Task<SqlMethodResult> ExecuteAsync(IContext context, Invoke invoke) {
			using (var methodContext = new MethodContext(context, this, invoke)) {
				await ExecuteContextAsync(methodContext);

				var result = methodContext.CreateResult();

				result.Validate(this, context);

				return result;
			}
		}

		public Task<SqlMethodResult> ExecuteAsync(IContext context, params InvokeArgument[] args) {
			var invoke = new Invoke(MethodInfo.MethodName);
			foreach (var arg in args) {
				invoke.Arguments.Add(arg);
			}

			return ExecuteAsync(context, invoke);
		}

		public Task<SqlMethodResult> ExecuteAsync(IContext context, params SqlExpression[] args) {
			var invokeArgs = args == null ? new InvokeArgument[0] : args.Select(x => new InvokeArgument(x)).ToArray();
			return ExecuteAsync(context, invokeArgs);
		}

		public Task<SqlMethodResult> ExecuteAsync(IContext context, params SqlObject[] args) {
			var exps = args == null
				? new SqlExpression[0]
				: args.Select(SqlExpression.Constant).Cast<SqlExpression>().ToArray();
			return ExecuteAsync(context, exps);
		}

		protected abstract Task ExecuteContextAsync(MethodContext context);

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		protected virtual void AppendTo(SqlStringBuilder builder) {
			builder.Append(Type.ToString().ToUpperInvariant());
			builder.Append(" ");

			MethodInfo.AppendTo(builder);
		}

		public override string ToString() {
			return this.ToSqlString();
		}

		public bool Matches(IContext context, Invoke invoke) {
			return MethodInfo.Matches(context, invoke);
		}
	}
}