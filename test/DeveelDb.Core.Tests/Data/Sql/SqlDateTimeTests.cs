﻿using System;
using System.Globalization;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlDateTimeTests {
		[Theory]
		[InlineData(2012, 12, 01, 07, 16, 22, 556, 2, 0)]
		public static void FromFullForm(int year,
			int month,
			int day,
			int hour,
			int minute,
			int second,
			int millis,
			int offsetHour,
			int offsetMinute) {
			var offset = new SqlDayToSecond(offsetHour, offsetMinute, 0);
			var date = new SqlDateTime(year, month, day, hour, minute, second, millis, offset);

			Assert.Equal(year, date.Year);
			Assert.Equal(month, date.Month);
			Assert.Equal(day, date.Day);
			Assert.Equal(hour, date.Hour);
			Assert.Equal(minute, date.Minute);
			Assert.Equal(second, date.Second);
			Assert.Equal(millis, date.Millisecond);
			Assert.Equal(offset, date.Offset);
		}

		[Theory]
		[InlineData(2012, 12, 01, 07, 16, 22, 556)]
		public static void FromMediumForm(int year, int month, int day, int hour, int minute, int second, int millis) {
			var date = new SqlDateTime(year, month, day, hour, minute, second, millis);

			Assert.Equal(year, date.Year);
			Assert.Equal(month, date.Month);
			Assert.Equal(day, date.Day);
			Assert.Equal(hour, date.Hour);
			Assert.Equal(minute, date.Minute);
			Assert.Equal(second, date.Second);
			Assert.Equal(millis, date.Millisecond);
			Assert.False(date.Offset.IsNull);
			Assert.Equal(0, date.Offset.Hours);
			Assert.Equal(0, date.Offset.Minutes);
		}

		[Theory]
		[InlineData(2012, 12, 01, 07, 16, 22, 556, 8, 0)]
		public static void FullFormToBytes(int year,
			int month,
			int day,
			int hour,
			int minute,
			int second,
			int millis,
			int offsetHour,
			int offsetMinute) {
			var offset = new SqlDayToSecond(offsetHour, offsetMinute, 0);
			var date = new SqlDateTime(year, month, day, hour, minute, second, millis, offset);

			var bytes = date.ToByteArray(true);

			Assert.NotNull(bytes);
			Assert.Equal(13, bytes.Length);

			var back = new SqlDateTime(bytes);

			Assert.Equal(date, back);
		}

		[Theory]
		[InlineData(2012, 12, 01, 07, 16, 22, 556)]
		public static void MediumFormToBytes(int year, int month, int day, int hour, int minute, int second, int millis) {
			var date = new SqlDateTime(year, month, day, hour, minute, second, millis);

			var bytes = date.ToByteArray();

			Assert.NotNull(bytes);
			Assert.Equal(11, bytes.Length);

			var back = new SqlDateTime(bytes);

			Assert.Equal(date, back);
		}

		[Theory]
		[InlineData("2014-04-11T02:19:13.334 +02:30", 2014, 04, 11, 02, 19, 13, 334, 02, 30, true)]
		public static void TryParseFull(string s,
			int year,
			int month,
			int day,
			int hour,
			int minute,
			int second,
			int millis,
			int offsetHour,
			int offsetMinute,
			bool expected) {
			SqlDateTime date;
			Assert.Equal(expected, SqlDateTime.TryParse(s, out date));

			Assert.False(date.IsNull);
			Assert.Equal(year, date.Year);
			Assert.Equal(month, date.Month);
			Assert.Equal(day, date.Day);
			Assert.Equal(hour, date.Hour);
			Assert.Equal(minute, date.Minute);
			Assert.Equal(second, date.Second);
			Assert.Equal(millis, date.Millisecond);
			Assert.Equal(offsetHour, date.Offset.Hours);
			Assert.Equal(offsetMinute, date.Offset.Minutes);
		}

		[Theory]
		[InlineData("2014-04-11T02:19:13.334 +02:30", 2014, 04, 11, 02, 19, 13, 334, 02, 0, "CET", true)]
		public static void TryParseFullWithTimeZone(string s,
			int year,
			int month,
			int day,
			int hour,
			int minute,
			int second,
			int millis,
			int offsetHour,
			int offsetMinute,
			string timeZone,
			bool expected) {
			SqlDateTime date;
			Assert.Equal(expected, SqlDateTime.TryParseTimeStamp(s, timeZone, out date));

			Assert.False(date.IsNull);
			Assert.Equal(year, date.Year);
			Assert.Equal(month, date.Month);
			Assert.Equal(day, date.Day);
			Assert.Equal(hour, date.Hour);
			Assert.Equal(minute, date.Minute);
			Assert.Equal(second, date.Second);
			Assert.Equal(millis, date.Millisecond);
			Assert.Equal(offsetHour, date.Offset.Hours);
			Assert.Equal(offsetMinute, date.Offset.Minutes);
		}


		[Theory]
		[InlineData("2011-02-15", "2012-03-22", false)]
		[InlineData(null, "2011-11-02T22:01:00", null)]
		[InlineData(null, null, null)]
		public static void Operator_Equal(string d1, string d2, bool? expected) {
			BinaryOp((x, y) => x == y, d1, d2, expected);
		}

		[Theory]
		[InlineData("2001-04-06T22:11:04.556", "2001-04-06", true)]
		[InlineData("22:01:11", null, null)]
		[InlineData(null, null, null)]
		public static void Operator_NotEqual(string d1, string d2, bool? expected) {
			BinaryOp((x, y) => x != y, d1, d2, expected);
		}

		[Theory]
		[InlineData("2010-02-10", "2011-04-11", false)]
		[InlineData("04:11:02.345", null, null)]
		public static void Operator_Greater(string d1, string d2, bool? expected) {
			BinaryOp((x, y) => x > y, d1, d2, expected);
		}

		[Theory]
		[InlineData("2004-08-19", "2004-08-19", true)]
		[InlineData("2013-02-05", "2012-02-05", true)]
		[InlineData("2010-01-03", null, null)]
		[InlineData(null, null, null)]
		public static void Operator_GreaterOrEqual(string d1, string d2, bool? expected) {
			BinaryOp((x, y) => x >= y, d1, d2, expected);
		}


		[Theory]
		[InlineData("2004-08-19", "2004-08-19", true)]
		[InlineData("2010-02-24", "2012-02-25", true)]
		[InlineData("20:30:22.019", null, null)]
		public static void Operator_SmallerOrEqual(string d1, string d2, bool? expected) {
			BinaryOp((x, y) => x <= y, d1, d2, expected);
		}


		[Theory]
		[InlineData("1999-02-01T22:11:03.011", "1999-02-01T22:11:03.012", true)]
		[InlineData("13:47:02", "13:45:21", false)]
		[InlineData("1980-04-06T03:01:20", "1981-08-27T05:05:22", true)]
		[InlineData(null, null, null)]
		[InlineData("15:16:09", null, null)]
		public static void Operator_Smaller(string d1, string d2, bool? expected) {
			BinaryOp((x, y) => x < y, d1, d2, expected);
		}

		[Theory]
		[InlineData("2016-11-29", "10.20:00:03.445", "2016-12-09T20:00:03.445")]
		public static void Operator_Add(string d1, string d2, string expected) {
			BinaryOp((x, y) => x + y, d1, d2, expected);
		}

		[Theory]
		[InlineData("0001-02-10T00:00:01", "2.23:12:02", "0001-02-07T00:47:59")]
		public static void Operator_Subtract(string d1, string d2, string expected) {
			BinaryOp((x, y) => x - y, d1, d2, expected);
		}

		[Theory]
		[InlineData("2013-12-01T09:11:25.893", 20, "2012-04-01T09:11:25.893")]
		public static void Operator_SubtractMonths(string d, int months, string expected) {
			BinaryOp((x, y) => x - y, d, months, expected);
		}

		[Theory]
		[InlineData("2005-04-04", 5, "2005-09-04")]
		public static void Operator_AddMonths(string d, int months, string expected) {
			BinaryOp((x, y) => x + y, d, months, expected);
		}

		private static void BinaryOp(Func<SqlDateTime, SqlDateTime, SqlBoolean> op, string s1, string s2, bool? expected) {
			var date1 = String.IsNullOrEmpty(s1) ? SqlDateTime.Null : SqlDateTime.Parse(s1);
			var date2 = String.IsNullOrEmpty(s2) ? SqlDateTime.Null : SqlDateTime.Parse(s2);

			var result = op(date1, date2);
			var expectedResult = (SqlBoolean) expected;

			Assert.Equal(expectedResult, result);
		}

		private static void BinaryOp(Func<SqlDateTime, SqlDayToSecond, SqlDateTime> op,
			string s1,
			string s2,
			string expected) {
			var date = String.IsNullOrEmpty(s1) ? SqlDateTime.Null : SqlDateTime.Parse(s1);
			var date2 = String.IsNullOrEmpty(s2) ? SqlDayToSecond.Null : SqlDayToSecond.Parse(s2);

			var result = op(date, date2);
			var expectedResult = SqlDateTime.Parse(expected);

			Assert.Equal(expectedResult, result);
		}

		private static void BinaryOp(Func<SqlDateTime, SqlYearToMonth, SqlDateTime> op,
			string s,
			int? months,
			string expected) {
			var date = String.IsNullOrEmpty(s) ? SqlDateTime.Null : SqlDateTime.Parse(s);
			var ytm = months == null ? SqlYearToMonth.Null : new SqlYearToMonth(months.Value);

			var result = op(date, ytm);
			var expectedResult = SqlDateTime.Parse(expected);

			Assert.Equal(expectedResult, result);
		}


		[Theory]
		[InlineData("2016-02-04", DayOfWeek.Monday, "2016-02-08")]
		[InlineData("1991-06-02", DayOfWeek.Tuesday, "1991-06-04")]
		public static void GetNextDateForDay(string s, DayOfWeek dayOfWeek, string expected) {
			var date = SqlDateTime.Parse(s);
			var nextDate = date.GetNextDateForDay(dayOfWeek);

			Assert.False(nextDate.IsNull);
			Assert.Equal(expected, nextDate.ToDateString());
		}

		[Theory]
		[InlineData("2017-02-01T02:12:17.012", "02:12:17.012 +00:00")]
		public static void ToTimeString(string s, string expected) {
			var date = SqlDateTime.Parse(s);
			var result = date.ToTimeString();

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("2018-01-12", "2018-01-12T00:00:00.000 +00:00")]
		public static void ToTimeStampString(string s, string expected) {
			var date = SqlDateTime.Parse(s);
			var result = date.ToTimeStampString();

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("12:25:01", "CET", "13:25:01 +01:00")]
		public static void AtTimeZone(string s, string timeZone, string expected) {
			var date = SqlDateTime.Parse(s);
			var result = date.AtTimeZone(timeZone);

			var expectedResult = SqlDateTime.Parse(expected);
			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData("2011-10-23T23:22:10 +02:00", "2011-10-23T21:22:10.000 +00:00")]
		[InlineData(null, null)]
		public static void ToUtc(string s, string expected) {
			var date = String.IsNullOrEmpty(expected) ? SqlDateTime.Null : SqlDateTime.Parse(s);
			var utc = date.ToUtc();

			var expectedResult = String.IsNullOrEmpty(expected) ? SqlDateTime.Null : SqlDateTime.Parse(expected);
			Assert.Equal(expectedResult, utc);
		}

		[Fact]
		public static void Convert_ToDateTime() {
			var date = new SqlDateTime(2018, 02, 04, 14, 02, 00, 00);
			var result = Convert.ChangeType(date, typeof(DateTime), CultureInfo.InvariantCulture);

			Assert.NotNull(result);
			Assert.IsType<DateTime>(result);

			var d = (DateTime) result;
			Assert.Equal(2018, d.Year);
			Assert.Equal(02, d.Month);
		}

		[Fact]
		public static void Convert_ToDateTimeOffset() {
			var date = new SqlDateTime(2018, 02, 04, 14, 02, 00, 00);
			var result = Convert.ChangeType(date, typeof(DateTimeOffset), CultureInfo.InvariantCulture);

			Assert.NotNull(result);
			Assert.IsType<DateTimeOffset>(result);

			var d = (DateTimeOffset)result;
			Assert.Equal(2018, d.Year);
			Assert.Equal(02, d.Month);
		}

		[Fact]
		public static void Convert_ToBinaryArray() {
			var date = new SqlDateTime(2018, 02, 04, 14, 02, 00, 00);
			var result = Convert.ChangeType(date, typeof(byte[]), CultureInfo.InvariantCulture);

			Assert.NotNull(result);
			Assert.IsType<byte[]>(result);

			var bytes = (byte[]) result;
			Assert.Equal(11, bytes.Length);
		}

		[Fact]
		public static void Convert_ToInt64() {
			var date = new SqlDateTime(2018, 02, 04, 14, 02, 00, 00);
			var result = Convert.ChangeType(date, typeof(long), CultureInfo.InvariantCulture);

			Assert.NotNull(result);
			Assert.IsType<long>(result);

			Assert.Equal(date.Ticks, (long) result);

			var back = new SqlDateTime((long) result);

			Assert.Equal(date.Year, back.Year);
			Assert.Equal(date.Month, back.Month);
		}


		[Fact]
		public static void InvalidConvert_Int32() {
			InvalidConvertTo(typeof(int));
		}

		[Fact]
		public static void InvalidConvert_Int16() {
			InvalidConvertTo(typeof(short));
		}

		[Fact]
		public static void InvalidConvert_Byte() {
			InvalidConvertTo(typeof(byte));
		}

		[Fact]
		public static void InvalidConvert_SByte() {
			InvalidConvertTo(typeof(sbyte));
		}

		[Fact]
		public static void InvalidConvert_UInt32() {
			InvalidConvertTo(typeof(uint));
		}

		[Fact]
		public static void InvalidConvert_Char() {
			InvalidConvertTo(typeof(char));
		}

		[Fact]
		public static void InvalidConvert_UInt16() {
			InvalidConvertTo(typeof(ushort));
		}

		[Fact]
		public static void InvalidConvert_Decimal() {
			InvalidConvertTo(typeof(decimal));
		}

		[Fact]
		public static void InvalidConvert_Boolean() {
			InvalidConvertTo(typeof(bool));
		}

		[Fact]
		public static void InvalidConvert_Single() {
			InvalidConvertTo(typeof(float));
		}

		[Fact]
		public static void InvalidConvert_Double() {
			InvalidConvertTo(typeof(double));
		}


		private static void InvalidConvertTo(Type type) {
			var date = new SqlDateTime(2001, 12, 01);
			Assert.Throws<InvalidCastException>(() => Convert.ChangeType(date, type));
		}
	}
}