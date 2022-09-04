
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static System.Numerics.BigRational; //todo: remove

#pragma warning disable CS1591 //todo: xml comments

namespace System.Numerics.Generic
{

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public unsafe readonly partial struct UInt<T> : IComparable<UInt<T>>, IComparable, IEquatable<UInt<T>>, IFormattable, ISpanFormattable where T : unmanaged
  {
    public static implicit operator UInt<T>(uint value)
    {
      var cpu = main_cpu; cpu.push(value);
      var v = default(UInt<T>); cpu.ipop(&v, 0x100 | unchecked((uint)sizeof(T) >> 2)); return v;
    }
    public static implicit operator UInt<T>(ulong value)
    {
      var cpu = main_cpu; cpu.push(value);
      var v = default(UInt<T>); cpu.ipop(&v, 0x100 | unchecked((uint)sizeof(T) >> 2)); return v;
    }

    public static explicit operator uint(UInt<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value.p, 0x100 | (sizeof(T) >> 2));
      var a = default(uint); cpu.ipop(&a, 0x0101); return a;
    }
    public static explicit operator ulong(UInt<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, 0x100 | (sizeof(T) >> 2));
      var a = default(ulong); cpu.ipop(&a, 0x0102); return a;
    }

    public static UInt<T> operator +(UInt<T> value) => value;
    public static UInt<T> operator -(UInt<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, 0x100 | (sizeof(T) >> 2));
      cpu.neg(); value = default; cpu.ipop(&value, 0x100 | unchecked((uint)sizeof(T) >> 2)); return value;
    }
    public static UInt<T> operator ++(UInt<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, 0x0104);
      cpu.inc(); value = default; cpu.ipop(&value, 0x100 | unchecked((uint)sizeof(T) >> 2)); return value;
    }
    public static UInt<T> operator --(UInt<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, 0x100 | (sizeof(T) >> 2));
      cpu.dec(); value = default; cpu.ipop(&value, 0x100 | unchecked((uint)sizeof(T) >> 2)); return value;
    }
    public static UInt<T> operator +(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, 0x100 | (sizeof(T) >> 2)); cpu.ipush(&right, 0x100 | (sizeof(T) >> 2));
      cpu.add(); left = default; cpu.ipop(&left, 0x100 | unchecked((uint)sizeof(T) >> 2)); return left;
    }
    public static UInt<T> operator -(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, 0x100 | (sizeof(T) >> 2)); cpu.ipush(&right, 0x100 | (sizeof(T) >> 2));
      cpu.sub(); left = default; cpu.ipop(&left, 0x100 | unchecked((uint)sizeof(T) >> 2)); return left;
    }
    public static UInt<T> operator *(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, 0x100 | (sizeof(T) >> 2)); cpu.ipush(&right, 0x100 | (sizeof(T) >> 2));
      cpu.mul(); left = default; cpu.ipop(&left, 0x100 | unchecked((uint)sizeof(T) >> 2)); return left;
    }
    public static UInt<T> operator /(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, 0x100 | (sizeof(T) >> 2)); cpu.ipush(&right, 0x100 | (sizeof(T) >> 2));
      cpu.idiv(); left = default; cpu.ipop(&left, 0x100 | unchecked((uint)sizeof(T) >> 2)); return left;
    }
    public static UInt<T> operator %(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, 0x100 | (sizeof(T) >> 2)); cpu.ipush(&right, 0x100 | (sizeof(T) >> 2));
      cpu.imod(); left = default; cpu.ipop(&left, 0x100 | unchecked((uint)sizeof(T) >> 2)); return left;
    }

    public static bool operator ==(UInt<T> left, UInt<T> right)
    {
      return CPU.icmp(&left, &right, 0x100 | (sizeof(T) >> 2)) == 0;
    }
    public static bool operator !=(UInt<T> left, UInt<T> right)
    {
      return CPU.icmp(&left, &right, 0x100 | (sizeof(T) >> 2)) != 0;
    }
    public static bool operator <=(UInt<T> left, UInt<T> right)
    {
      return CPU.icmp(&left, &right, 0x100 | (sizeof(T) >> 2)) <= 0;
    }
    public static bool operator >=(UInt<T> left, UInt<T> right)
    {
      return CPU.icmp(&left, &right, 0x100 | (sizeof(T) >> 2)) >= 0;
    }
    public static bool operator <(UInt<T> left, UInt<T> right)
    {
      return CPU.icmp(&left, &right, 0x100 | (sizeof(T) >> 2)) < 0;
    }
    public static bool operator >(UInt<T> left, UInt<T> right)
    {
      return CPU.icmp(&left, &right, 0x100 | (sizeof(T) >> 2)) > 0;
    }

    public static UInt<T> MaxValue
    {
      get { UInt<T> a; new Span<uint>(&a, sizeof(T) >> 2).Fill(0xffffffff); return a; }
    }
    public static UInt<T> MinValue => default;

    public static UInt<T> Min(UInt<T> x, UInt<T> y)
    {
      return CPU.icmp(&x, &y, 0x100 | (sizeof(T) >> 2)) <= 0 ? x : y;
    }
    public static UInt<T> Max(UInt<T> x, UInt<T> y)
    {
      return CPU.icmp(&x, &y, 0x100 | (sizeof(T) >> 2)) >= 0 ? x : y;
    }
    public static int LeadingZeroCount(UInt<T> x)
    {
      for (int i = (sizeof(T) >> 2) - 1; i >= 0; i--)
        if (((uint*)&x)[i] != 0)
          return (i << 3) + BitOperations.LeadingZeroCount(((uint*)&x)[i]);
      return sizeof(T) << 3;
    }
    public static int TrailingZeroCount(UInt<T> x)
    {
      for (int i = 0, n = sizeof(T) >> 2; i < n; i++)
        if (((uint*)&x)[i] != 0)
          return (i << 3) + BitOperations.TrailingZeroCount(((uint*)&x)[i]);
      return sizeof(T) << 3;
    }

    #region private
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly T p;
    #endregion

    public readonly override string ToString() => ToString(null, null);
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
      Span<char> sp = stackalloc char[100 + 32];
      if (!TryFormat(sp, out var ns, format, formatProvider))
      {
        int n; sp.Slice(0, 2).CopyTo(new Span<char>(&n, 2)); sp = stackalloc char[n];
        TryFormat(sp, out ns, format, formatProvider); Debug.Assert(ns != 0);
      }
      return sp.Slice(0, ns).ToString();
    }
    public bool TryFormat(Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
      var fmt = 'D'; int dig = 0;
      if (format.Length != 0)
      {
        var fc = (fmt = format[0]) & ~0x20; var d = format.Length > 1;
        if (d) dig = stoi(format.Slice(1));
      }
      var cpu = main_cpu; //cpu.push(88);
      var t = this; cpu.ipush(&t, 0x100 | (sizeof(T) >> 2));
      var n = tos(dest, cpu, fmt, dig, 0, 0, 0);
      if (n > 0) { charsWritten = n; return true; }
      if (dest.Length >= 2) { n = -n; new Span<char>(&n, 2).CopyTo(dest); }
      charsWritten = 0; return false;
    }
    public int CompareTo(object? obj) { return obj == null ? 1 : p is Int<T> b ? this.CompareTo(b) : throw new ArgumentException(); }
    public int CompareTo(UInt<T> other) { var a = this; return CPU.icmp(&a, &other, 0x100 | (sizeof(T) >> 2)); }
    public bool Equals(UInt<T> other) { var t = this; return CPU.icmp(&t, &other, 0x100 | (sizeof(T) >> 2)) == 0; }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      if (obj is not UInt<T> b) return false;
      var a = this; return CPU.icmp(&a, &b, 0x100 | (sizeof(T) >> 2)) == 0;
    }
    public override int GetHashCode()
    {
      var a = this; return CPU.hash(&a, sizeof(T));
    }
  }

#if NET7_0
  public unsafe readonly partial struct UInt<T> : IBinaryInteger<UInt<T>>, IMinMaxValue<UInt<T>>, IUnsignedNumber<UInt<T>>
  {
    public static implicit operator UInt<T>(UInt128 value)
    {
      var cpu = main_cpu; cpu.push(value);
      var v = default(UInt<T>); cpu.ipop(&v, 0x100 | unchecked((uint)sizeof(T) >> 2)); return v;
    }
    public static explicit operator UInt128(UInt<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, 0x100 | (sizeof(T) >> 2));
      var a = default(UInt128); cpu.ipop(&a, 0x0004); return a;
    }

    static int INumberBase<UInt<T>>.Radix => 2;
    static UInt<T> IBinaryNumber<UInt<T>>.AllBitsSet => MaxValue;
    static UInt<T> INumberBase<UInt<T>>.One => 1;
    static UInt<T> INumberBase<UInt<T>>.Zero => default;
    static UInt<T> IAdditiveIdentity<UInt<T>, UInt<T>>.AdditiveIdentity => default;
    static UInt<T> IMultiplicativeIdentity<UInt<T>, UInt<T>>.MultiplicativeIdentity => 1;
    static bool INumberBase<UInt<T>>.IsCanonical(UInt<T> value) => true;
    static bool INumberBase<UInt<T>>.IsComplexNumber(UInt<T> value) => false;
    static bool INumberBase<UInt<T>>.IsImaginaryNumber(UInt<T> value) => false;
    static bool INumberBase<UInt<T>>.IsFinite(UInt<T> value) => true;
    static bool INumberBase<UInt<T>>.IsInteger(UInt<T> value) => true;
    static bool INumberBase<UInt<T>>.IsRealNumber(UInt<T> value) => true;
    static bool INumberBase<UInt<T>>.IsSubnormal(UInt<T> value) => false;
    static bool INumberBase<UInt<T>>.IsInfinity(UInt<T> value) => false;
    static bool INumberBase<UInt<T>>.IsNaN(UInt<T> value) => false;
    static bool INumberBase<UInt<T>>.IsPositiveInfinity(UInt<T> value) => false;
    static bool INumberBase<UInt<T>>.IsNegative(UInt<T> value) => false;
    static bool INumberBase<UInt<T>>.IsNegativeInfinity(UInt<T> value) => false;
    static bool INumberBase<UInt<T>>.IsZero(UInt<T> value) => value == default;
    static bool INumberBase<UInt<T>>.IsPositive(UInt<T> value) => true;
    static bool INumberBase<UInt<T>>.IsNormal(UInt<T> value) => value != 0;
    static bool INumberBase<UInt<T>>.IsEvenInteger(UInt<T> value) => (((uint*)&value)[0] & 1) == 0;
    static bool INumberBase<UInt<T>>.IsOddInteger(UInt<T> value) => (((uint*)&value)[0] & 1) != 0;
    static UInt<T> INumberBase<UInt<T>>.Abs(UInt<T> value) => value;
    static UInt<T> INumberBase<UInt<T>>.MaxMagnitude(UInt<T> x, UInt<T> y) => Max(x, y);
    static UInt<T> INumberBase<UInt<T>>.MaxMagnitudeNumber(UInt<T> x, UInt<T> y) => Max(x, y);
    static UInt<T> INumberBase<UInt<T>>.MinMagnitude(UInt<T> x, UInt<T> y) => Min(x, y);
    static UInt<T> INumberBase<UInt<T>>.MinMagnitudeNumber(UInt<T> x, UInt<T> y) => Min(x, y);
    int IBinaryInteger<UInt<T>>.GetByteCount() => sizeof(T);
    int IBinaryInteger<UInt<T>>.GetShortestBitLength() => unchecked((sizeof(T) << 3) - (int)LeadingZeroCount(this));

    #region todo
    static bool IBinaryNumber<UInt<T>>.IsPow2(UInt<T> value) => throw new NotImplementedException();
    static UInt<T> IBinaryNumber<UInt<T>>.Log2(UInt<T> value) => throw new NotImplementedException();
    static UInt<T> IBitwiseOperators<UInt<T>, UInt<T>, UInt<T>>.operator ~(UInt<T> value) => throw new NotImplementedException();
    static UInt<T> IBitwiseOperators<UInt<T>, UInt<T>, UInt<T>>.operator &(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static UInt<T> IBitwiseOperators<UInt<T>, UInt<T>, UInt<T>>.operator |(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static UInt<T> IBitwiseOperators<UInt<T>, UInt<T>, UInt<T>>.operator ^(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static UInt<T> IShiftOperators<UInt<T>, int, UInt<T>>.operator <<(UInt<T> value, int shiftAmount) => throw new NotImplementedException();
    static UInt<T> IShiftOperators<UInt<T>, int, UInt<T>>.operator >>(UInt<T> value, int shiftAmount) => throw new NotImplementedException();
    static UInt<T> IShiftOperators<UInt<T>, int, UInt<T>>.operator >>>(UInt<T> value, int shiftAmount) => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.Parse(string s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static UInt<T> ISpanParsable<UInt<T>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => throw new NotImplementedException();
    static UInt<T> IParsable<UInt<T>>.Parse(string s, IFormatProvider? provider) => throw new NotImplementedException();
    static UInt<T> IBinaryInteger<UInt<T>>.PopCount(UInt<T> value) => throw new NotImplementedException();
    static UInt<T> IBinaryInteger<UInt<T>>.TrailingZeroCount(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertFromChecked<TOther>(TOther value, out UInt<T> result) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertFromSaturating<TOther>(TOther value, out UInt<T> result) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertFromTruncating<TOther>(TOther value, out UInt<T> result) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertToChecked<TOther>(UInt<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertToSaturating<TOther>(UInt<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertToTruncating<TOther>(UInt<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out UInt<T> result) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UInt<T> result) => throw new NotImplementedException();
    static bool ISpanParsable<UInt<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out UInt<T> result) => throw new NotImplementedException();
    static bool IParsable<UInt<T>>.TryParse(string? s, IFormatProvider? provider, out UInt<T> result) => throw new NotImplementedException();
    static bool IBinaryInteger<UInt<T>>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt<T> value) => throw new NotImplementedException();
    static bool IBinaryInteger<UInt<T>>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt<T> value) => throw new NotImplementedException();
    bool IBinaryInteger<UInt<T>>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IBinaryInteger<UInt<T>>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    #endregion

    // static UInt<T> IIncrementOperators<UInt<T>>.operator ++(UInt<T> value) => throw new NotImplementedException();
    // static UInt<T> IDecrementOperators<UInt<T>>.operator --(UInt<T> value) => throw new NotImplementedException();
    // static UInt<T> IMultiplyOperators<UInt<T>, UInt<T>, UInt<T>>.operator *(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static UInt<T> IDivisionOperators<UInt<T>, UInt<T>, UInt<T>>.operator /(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static UInt<T> IModulusOperators<UInt<T>, UInt<T>, UInt<T>>.operator %(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static UInt<T> IUnaryPlusOperators<UInt<T>, UInt<T>>.operator +(UInt<T> value) => throw new NotImplementedException();
    // static UInt<T> IAdditionOperators<UInt<T>, UInt<T>, UInt<T>>.operator +(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static UInt<T> IUnaryNegationOperators<UInt<T>, UInt<T>>.operator -(UInt<T> value) => throw new NotImplementedException();
    // static UInt<T> ISubtractionOperators<UInt<T>, UInt<T>, UInt<T>>.operator -(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static bool IEqualityOperators<UInt<T>, UInt<T>, bool>.operator ==(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static bool IEqualityOperators<UInt<T>, UInt<T>, bool>.operator !=(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<UInt<T>, UInt<T>, bool>.operator <(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<UInt<T>, UInt<T>, bool>.operator >(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<UInt<T>, UInt<T>, bool>.operator <=(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<UInt<T>, UInt<T>, bool>.operator >=(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static UInt<T> IMinMaxValue<UInt<T>>.MaxValue => throw new NotImplementedException();
    // static UInt<T> IMinMaxValue<UInt<T>>.MinValue => default;
    // int IComparable.CompareTo(object? obj) => throw new NotImplementedException();
    // int IComparable<UInt<T>>.CompareTo(UInt<T> other) => throw new NotImplementedException();
    // bool IEquatable<UInt<T>>.Equals(UInt<T> other) => throw new NotImplementedException();
    // string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => throw new NotImplementedException();
    // bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => throw new NotImplementedException();
  }
#endif

}

