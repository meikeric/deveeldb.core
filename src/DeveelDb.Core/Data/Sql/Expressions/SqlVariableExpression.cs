﻿// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;

using Deveel.Data.Configuration;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlVariableExpression : SqlExpression {
		internal SqlVariableExpression(string variableName)
			: base(SqlExpressionType.Variable) {
			if (String.IsNullOrWhiteSpace(variableName))
				throw new ArgumentNullException(nameof(variableName));
			if (!Variables.Variable.IsValidName(variableName))
				throw new ArgumentException($"The variable name '{variableName}' is invalid.");

			VariableName = variableName;
		}

		public string VariableName { get; }

		public override bool CanReduce => true;

		public override bool IsReference => true;

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.AppendFormat(":{0}", VariableName);
		}

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitVariable(this);
		}

		public override SqlExpression Reduce(IContext context) {
			if (context == null)
				throw new SqlExpressionException("A context is required to reduce a variable expression");

			var ignoreCase = context.GetValue("ignoreCase", false);
			var variable = context.ResolveVariable(VariableName, ignoreCase);

			if (variable == null)
				return Constant(SqlObject.Unknown);

			return variable.Evaluate(context);
		}

		public override SqlType GetSqlType(IContext context) {
			if (context == null)
				throw new SqlExpressionException("A context is required to reduce a variable expression");

			var ignoreCase = context.GetValue("ignoreCase", false);
			var variable = context.ResolveVariable(VariableName, ignoreCase);

			if (variable == null)
				throw new SqlExpressionException();

			return variable.Type;
		}
	}
}