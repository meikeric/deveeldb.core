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
using System.Threading.Tasks;

using Deveel.Data.Sql;

namespace Deveel.Data.Sql.Tables {
	public sealed class Field {
		private readonly Row row;
		private readonly int column;

		public Field(Row row, int column) {
			this.row = row;
			this.column = column;
		}

		public SqlType ColumnType => row.Table.TableInfo.Columns[column].ColumnType;

		public string ColumnName => row.Table.TableInfo.Columns[column].ColumnName;

		public SqlObject GetValue() {
			return row.GetValue(column);
		}

		public Task<SqlObject> GetValueAsync() {
			return row.GetValueAsync(column);
		}
	}
}