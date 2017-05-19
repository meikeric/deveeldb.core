﻿using System;
using System.Threading.Tasks;

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