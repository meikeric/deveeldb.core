﻿using System;
using System.Linq;

using Xunit;

namespace Deveel.Data.Sql.Tables {
	public class TemporaryTableTests {
		[Fact]
		public static void CreateAndAddRows() {
			var tableInfo = new TableInfo(new ObjectName("table1"));
			tableInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			tableInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.VarChar(22)));

			var table = new TemporaryTable(tableInfo);
			table.AddRow(new SqlObject[] {
				SqlObject.Integer(22),
				SqlObject.String(new SqlString("test")), 
			});

			Assert.Equal(1, table.RowCount);

			var value1 = table.GetValue(0, 0);
			var value2 = table.GetValue(0, 1);

			Assert.Equal(SqlObject.Integer(22), value1);
			Assert.Equal(SqlObject.String(new SqlString("test")), value2);
		}

		[Fact]
		public static void EnuemrateRows() {
			var tableInfo = new TableInfo(new ObjectName("table1"));
			tableInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			tableInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.VarChar(22)));

			var table = new TemporaryTable(tableInfo);
			table.AddRow(new SqlObject[] {
				SqlObject.Integer(22),
				SqlObject.String(new SqlString("test")),
			});
			table.AddRow(new [] {
				SqlObject.Integer(15002933),
				SqlObject.String(new SqlString("test2")), 
			});

			Assert.Equal(2, table.Count());

			var row1 = table.ElementAt(0);
			var row2 = table.ElementAt(1);

			Assert.Equal(SqlObject.Integer(22), row1["a"]);
			Assert.Equal(SqlObject.String(new SqlString("test")), row1["b"]);
			Assert.Equal(SqlObject.Integer(15002933), row2["a"]);
		}
	}
}