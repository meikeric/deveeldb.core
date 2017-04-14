﻿using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Methods {
	public sealed class MethodContext : Context, IVariableResolver {
		private Dictionary<string, SqlExpression> output;
		private Dictionary<string, SqlExpression> namedArgs;

		internal MethodContext(IContext context, SqlMethod method, Invoke invoke)
			: base(context, $"Method({method.MethodInfo.MethodName})") {
			Invoke = invoke;
			Method = method;

			namedArgs = BuildArguments(method.MethodInfo, invoke);

			ResultValue = SqlExpression.Constant(SqlObject.Null);
			output = new Dictionary<string, SqlExpression>();

			ContextScope.RegisterInstance<IVariableResolver>(this);
		}

		public SqlMethod Method { get; }

		public Invoke Invoke { get; private set; }

		internal SqlExpression ResultValue { get; private set; }

		internal bool HasResult { get; private set; }

		public SqlExpression Argument(string argName) {
			SqlExpression value;
			if (!namedArgs.TryGetValue(argName, out value)) {
				throw new InvalidOperationException();
			}

			return value;
		}

		public SqlExpression Argument(int offset) {
			if (offset >= Invoke.Arguments.Count)
				throw new ArgumentOutOfRangeException();

			return Invoke.Arguments[offset].Value;
		}

		public SqlObject Value(string argName) {
			var exp = Argument(argName);
			var value = exp.Reduce(this);

			if (!(value is SqlConstantExpression))
				throw new InvalidOperationException($"The argument {argName} of the invoke does not resolve to any constant value");

			return ((SqlConstantExpression) value).Value;
		}

		public SqlObject Value(int offset) {
			var exp = Argument(offset);
			var value = exp.Reduce(this);

			if (!(value is SqlConstantExpression))
				throw new InvalidOperationException($"The argument at offset {offset} of the invoke does not resolve to any constant value");

			return ((SqlConstantExpression)value).Value;
		}

		private static Dictionary<string, SqlExpression> BuildArguments(SqlMethodInfo methodInfo, Invoke invoke) {
			var result = new Dictionary<string, SqlExpression>();

			if (invoke.IsNamed) {
				var invokeArgs = invoke.Arguments.ToDictionary(x => x.ParameterName, y => y.Value);
				var methodParams = methodInfo.Parameters.ToDictionary(x => x.Name, y => y);

				foreach (var invokeArg in invokeArgs) {
					SqlMethodParameterInfo paramInfo;
					if (!methodParams.TryGetValue(invokeArg.Key, out paramInfo))
						throw new InvalidOperationException(
							$"Invoke argument {invokeArg.Key} does not correspond to any parameter of the method");

					result[invokeArg.Key] = invokeArg.Value;
				}

				foreach (var methodParam in methodParams) {
					if (!result.ContainsKey(methodParam.Key)) {
						var paramInfo = methodParam.Value;
						if (!paramInfo.HasDefaultValue)
							throw new InvalidOperationException(
								$"The invoke to {methodInfo.MethodName} has no value for parameter {paramInfo.Name} and the parameter has no default value");

						result[methodParam.Key] = paramInfo.DefaultValue;
					}
				}
			} else {
				if (methodInfo.Parameters.Count != invoke.Arguments.Count)
					throw new NotSupportedException($"Invoke arguments mismatch the number of parameters of {methodInfo.MethodName}");

				for (int i = 0; i < methodInfo.Parameters.Count; i++) {
					var parmInfo = methodInfo.Parameters[i];
					result[parmInfo.Name] = invoke.Arguments[i].Value;
				}
			}

			return result;
		}

		Variable IVariableResolver.ResolveVariable(string name, bool ignoreCase) {
			SqlMethodParameterInfo paramInfo;
			if (!Method.MethodInfo.TryGetParameter(name, ignoreCase, out paramInfo))
				return null;

			SqlExpression value;
			if (!namedArgs.TryGetValue(name, out value)) {
				value = SqlExpression.Constant(SqlObject.Null);
			}

			return new Variable(name, paramInfo.ParameterType, true, value);
		}

		internal SqlMethodResult CreateResult() {
			return new SqlMethodResult(ResultValue, HasResult, output);
		}

		public void SetOutput(string parameterName, SqlExpression value) {
			if (String.IsNullOrWhiteSpace(parameterName))
				throw new ArgumentNullException(nameof(parameterName));

			if (!Method.IsProcedure)
				throw new InvalidOperationException($"The method {Method.MethodInfo.MethodName} is not a Procedure");

			SqlMethodParameterInfo parameter;
			if (!Method.MethodInfo.Parameters.ToDictionary(x => x.Name, y => y).TryGetValue(parameterName, out parameter))
				throw new ArgumentException($"The method {Method.MethodInfo.MethodName} contains no parameter {parameterName}");

			if (!parameter.IsOutput)
				throw new ArgumentException($"The parameter {parameter.Name} is not an OUTPUT parameter");

			output[parameterName] = value;
		}

		public void SetResult(SqlObject value) {
			if (!Method.IsFunction)
				throw new InvalidOperationException();

			if (value.IsNull) {
				var functionInfo = (SqlFunctionInfo) Method.MethodInfo;
				value = SqlObject.NullOf(functionInfo.ReturnType);
			}

			SetResult(SqlExpression.Constant(value));
		}

		public void SetResult(SqlExpression value) {
			if (!Method.IsFunction)
				throw new InvalidOperationException($"Trying to set the return type to the method {Method.MethodInfo.MethodName} that is not a function.");

			var functionInfo = (SqlFunctionInfo)Method.MethodInfo;

			if (value.ExpressionType == SqlExpressionType.Constant) {
				var exp = (SqlConstantExpression) value;
				if (exp.Value.IsNull)
					value = SqlExpression.Constant(SqlObject.NullOf(functionInfo.ReturnType));
				if (exp.Value.IsUnknown || exp.Value.IsNull) {
					ResultValue = value;
					HasResult = true;
					return;
				}
			}

			var valueType = value.GetSqlType(this);
			if (!valueType.IsComparable(functionInfo.ReturnType))
				throw new InvalidOperationException($"The result type {valueType} of the expression is not compatible " +
				                                    $"with the return type {functionInfo.ReturnType} of the function {Method.MethodInfo.MethodName}");

			ResultValue = value;
			HasResult = true;
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (output != null)
					output.Clear();
			}

			output = null;
			ResultValue = null;
			base.Dispose(disposing);
		}
	}
}