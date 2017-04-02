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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Deveel.Data.Sql {
	/// <summary>
	/// The most simple implementation of a SQL string with a small size
	/// </summary>
	/// <remarks>
	/// <para>
	/// Instances of this object handle strings that are not backed by large
	/// objects and can handle a fixed length of characters.
	/// </para>
	/// <para>
	/// The encoding of the string is dependent from the <see cref="SqlStringType"/> that
	/// defines an object, but the default is <see cref="UnicodeEncoding"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="ISqlString"/>
	public struct SqlString : ISqlString, IEquatable<SqlString>, IConvertible
	{
		/// <summary>
		/// The maximum length of characters a <see cref="SqlString"/> can handle.
		/// </summary>
		public const int MaxLength = Int16.MaxValue;

		/// <summary>
		/// The <c>null</c> instance of a string.
		/// </summary>
		public static readonly SqlString Null = new SqlString(null, 0, true);

		private readonly string source;

		private SqlString(char[] chars, int length, bool isNull) : this() {
			if (chars == null) {
				source = null;
			} else {
				if (length > MaxLength)
					throw new ArgumentOutOfRangeException("length");

				source = new string(chars, 0, length);
				Length = chars.Length;
			}

			IsNull = isNull || source == null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlString"/> structure with
		/// the given set of characters.
		/// </summary>
		/// <param name="chars">The chars.</param>
		public SqlString(char[] chars)
			: this(chars, chars == null ? 0 : chars.Length) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlString"/> structure.
		/// </summary>
		/// <param name="chars">The chars.</param>
		/// <param name="length">The length.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">length</exception>
		public SqlString(char[] chars, int length)
			: this(chars, length, false) {
		}

		public SqlString(string source)
			: this(source == null ? (char[]) null : source.ToCharArray()) {
		}

		public SqlString(byte[] bytes, int offset, int length)
			: this(GetChars(bytes, offset, length)) {
		}

		public SqlString(byte[] bytes)
			: this(bytes, 0, bytes == null ? 0 : bytes.Length) {
		}

		private static char[] GetChars(byte[] bytes, int offset, int length) {
			if (bytes == null)
				return null;

			return Encoding.Unicode.GetChars(bytes, offset, length);
		}

		int IComparable.CompareTo(object obj) {
			return CompareTo((ISqlString) obj);
		}

		int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
			return CompareTo((ISqlString) other);
		}

		public bool IsNull { get; private set; }

		public string Value {
			get { return source; }
		}

		public char this[long index] {
			get {
				if (index > Int32.MaxValue)
					throw new ArgumentOutOfRangeException(nameof(index));

				if (source == null)
					return '\0';
				if (index >= Length)
					throw new ArgumentOutOfRangeException(nameof(index));

				return source[(int) index];
			}
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return other is ISqlString;
		}

		public int CompareTo(ISqlString other) {
			if (other == null)
				throw new ArgumentNullException("other");

			if (IsNull && other.IsNull)
				return 0;
			if (IsNull)
				return 1;
			if (other.IsNull)
				return -1;

			if (other is SqlString) {
				var otherString = (SqlString) other;
				return String.Compare(Value, otherString.Value, StringComparison.Ordinal);
			}

			throw new NotImplementedException("Comparison with long strong not implemented yet.");
		}

		public IEnumerator<char> GetEnumerator() {
			return new StringEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public long Length { get; private set; }

		public TextReader GetInput() {
			if (IsNull)
				return TextReader.Null;

			var bytes = Encoding.Unicode.GetBytes(source);
			var stream = new MemoryStream(bytes);
			return new StreamReader(stream);
		}

		public SqlString Substring(int offset, int count) {
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (count >= Length)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > Length)
				throw new ArgumentOutOfRangeException();

			if (IsNull)
				return Null;

			return source.Substring(offset, count);
		}

		public SqlString PadRight(int length)
			=> PadRight(length, ' ');

		public SqlString PadRight(int length, char c) {
			if (length < 0)
				throw new ArgumentException();

			if (IsNull)
				return Null;

			return new SqlString(source.PadRight(length, c));
		}

		public bool Equals(SqlString other) {
			return Equals(other, false);
		}

		public bool Equals(SqlString other, bool ignoreCase) {
			if (source == null && other.source == null)
				return true;
			if (source == null)
				return false;
			if (other == null || other.IsNull)
				return false;

			if (source.Length != other.source.Length)
				return false;

			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			return source.Equals(other.source, comparison);
		}

		public override bool Equals(object obj) {
			if (obj is SqlNull && IsNull)
				return true;

			return Equals((SqlString) obj);
		}

		public override int GetHashCode() {
			if (source == null)
				return 0;

			unchecked {
				int hash = 17;

				// get hash code for all items in array
				foreach (var item in source) {
					hash = hash*23 + item.GetHashCode();
				}

				return hash;
			}
		}

		public byte[] ToByteArray() {
			if (source == null)
				return new byte[0];

			return Encoding.Unicode.GetBytes(source);
		}

		public SqlString Concat(ISqlString other) {
			if (other == null || other.IsNull)
				return this;

			if (other is SqlString) {
				var otheString = (SqlString) other;
				var length = (int) (Length + otheString.Length);
				if (length >= MaxLength)
					throw new ArgumentException("The final string will be over the maximum length");

				var sourceChars = ToCharArray();
				var otherChars = otheString.ToCharArray();
				var destChars = new char[length];

				Array.Copy(sourceChars, 0, destChars, 0, (int) Length);
				Array.Copy(otherChars, 0, destChars, (int) Length, (int) otheString.Length);
				return new SqlString(destChars, length);
			}

			var sb = new StringBuilder(Int16.MaxValue);
			using (var output = new StringWriter(sb)) {
				// First read the current stream
				using (var reader = GetInput()) {
					var buffer = new char[2048];
					int count;
					while ((count = reader.Read(buffer, 0, buffer.Length)) != 0) {
						output.Write(buffer, 0, count);
					}
				}

				// Then read the second stream
				using (var reader = other.GetInput()) {
					var buffer = new char[2048];
					int count;
					while ((count = reader.Read(buffer, 0, buffer.Length)) != 0) {
						output.Write(buffer, 0, count);
					}
				}

				output.Flush();
			}

			var outChars = new char[sb.Length];
			sb.CopyTo(0, outChars, 0, sb.Length);
			return new SqlString(outChars, outChars.Length);
		}

		#region StringEnumerator

		class StringEnumerator : IEnumerator<char> {
			private readonly SqlString sqlString;
			private int index = -1;
			private readonly int length;

			public StringEnumerator(SqlString sqlString) {
				this.sqlString = sqlString;
				length = (int) sqlString.Length;
			}

			public void Dispose() {
			}

			public bool MoveNext() {
				return ++index < length;
			}

			public void Reset() {
				index = -1;
			}

			public char Current {
				get {
					if (index >= length)
						throw new InvalidOperationException();

					return sqlString[index];
				}
			}

			object IEnumerator.Current {
				get { return Current; }
			}
		}

		#endregion

		public override string ToString() {
			return Value;
		}

		TypeCode IConvertible.GetTypeCode() {
			return TypeCode.String;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider) {
			return Convert.ToBoolean(Value, provider);
		}

		char IConvertible.ToChar(IFormatProvider provider) {
			return Convert.ToChar(Value, provider);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider) {
			return Convert.ToSByte(Value, provider);
		}

		byte IConvertible.ToByte(IFormatProvider provider) {
			return Convert.ToByte(Value, provider);
		}

		short IConvertible.ToInt16(IFormatProvider provider) {
			return Convert.ToInt16(Value, provider);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider) {
			return Convert.ToUInt16(Value, provider);
		}

		int IConvertible.ToInt32(IFormatProvider provider) {
			return Convert.ToInt32(Value, provider);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider) {
			return Convert.ToUInt32(Value, provider);
		}

		long IConvertible.ToInt64(IFormatProvider provider) {
			return Convert.ToInt64(Value, provider);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider) {
			return Convert.ToUInt64(Value, provider);
		}

		float IConvertible.ToSingle(IFormatProvider provider) {
			return Convert.ToSingle(Value, provider);
		}

		double IConvertible.ToDouble(IFormatProvider provider) {
			return Convert.ToDouble(Value, provider);
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider) {
			return Convert.ToDecimal(Value, provider);
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider) {
			return Convert.ToDateTime(Value, provider);
		}

		string IConvertible.ToString(IFormatProvider provider) {
			return Convert.ToString(Value, provider);
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider) {
			if (conversionType == typeof(char[]))
				return ToCharArray();

			if (conversionType == typeof(SqlNumber))
				return ToNumber(provider);
			if (conversionType == typeof(SqlBoolean))
				return ToBoolean();
			/*
			TODO:
			if (conversionType == typeof(SqlDateTime))
				return ToDateTime();
			*/
			if (conversionType == typeof(SqlBinary))
				return ToBinary();

			throw new InvalidCastException(String.Format("Cannot convet SQL STRING to {0}", conversionType.FullName));
		}

		private SqlBoolean ToBoolean() {
			SqlBoolean value;
			if (!SqlBoolean.TryParse(Value, out value))
				return SqlBoolean.Null; // TODO: Should we throw an exception?

			return value;
		}

		private SqlNumber ToNumber(IFormatProvider provider) {
			SqlNumber value;
			if (!SqlNumber.TryParse(Value, provider, out value))
				return SqlNumber.Null; // TODO: Shoudl we throw an exception?

			return value;
		}

		/*
		TODO:
		public SqlDateTime ToDateTime() {
			SqlDateTime value;
			if (!SqlDateTime.TryParse(Value, out value))
				return SqlDateTime.Null; // TODO: Shoudl we throw an exception?

			return value;
		}
		*/

		private SqlBinary ToBinary() {
			var bytes = ToByteArray();
			return new SqlBinary(bytes);
		}

		public char[] ToCharArray() {
			if (source == null)
				return new char[0];

			return source.ToCharArray();
		}

		#region Operators

		public static bool operator ==(SqlString s1, SqlString s2) {
			return s1.Equals(s2);
		}

		public static bool operator !=(SqlString s1, SqlString s2) {
			return !(s1 == s2);
		}

		#endregion

		#region Implicit Operators

		public static implicit operator SqlString(string s) {
			return new SqlString(s);
		}

		public static implicit operator string(SqlString s) {
			return s.Value;
		}

		#endregion
	}
}