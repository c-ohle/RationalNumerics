
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
  public unsafe readonly partial struct Int<T> : IComparable<Int<T>>, IComparable, IEquatable<Int<T>>, IFormattable, ISpanFormattable where T : unmanaged
  {
    public static implicit operator Int<T>(int value)
    {
      var cpu = main_cpu; cpu.push(value);
      var v = default(Int<T>); cpu.ipop(&v, unchecked((uint)sizeof(T) >> 2)); return v;
    }
    public static implicit operator Int<T>(long value)
    {
      var cpu = main_cpu; cpu.push(value);
      var v = default(Int<T>); cpu.ipop(&v, unchecked((uint)sizeof(T) >> 2)); return v;
    }

    public static explicit operator int(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value.p, sizeof(T) >> 2);
      var a = default(int); cpu.ipop(&a, 0x0001); return a;
    }
    public static explicit operator long(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T) >> 2);
      var a = default(long); cpu.ipop(&a, 0x0002); return a;
    }

    public static Int<T> operator +(Int<T> value) => value;
    public static Int<T> operator -(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T) >> 2);
      cpu.neg(); value = default; cpu.ipop(&value, unchecked((uint)sizeof(T) >> 2)); return value;
    }
    public static Int<T> operator ++(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T) >> 2);
      cpu.inc(); value = default; cpu.ipop(&value, unchecked((uint)sizeof(T) >> 2)); return value;
    }
    public static Int<T> operator --(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T) >> 2);
      cpu.dec(); value = default; cpu.ipop(&value, unchecked((uint)sizeof(T) >> 2)); return value;
    }
    public static Int<T> operator +(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T) >> 2); cpu.ipush(&right, sizeof(T) >> 2);
      cpu.add(); left = default; cpu.ipop(&left, unchecked((uint)sizeof(T) >> 2)); return left;
    }
    public static Int<T> operator -(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T) >> 2); cpu.ipush(&right, sizeof(T) >> 2);
      cpu.sub(); left = default; cpu.ipop(&left, unchecked((uint)sizeof(T) >> 2)); return left;
    }
    public static Int<T> operator *(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T) >> 2); cpu.ipush(&right, sizeof(T) >> 2);
      cpu.mul(); left = default; cpu.ipop(&left, unchecked((uint)sizeof(T) >> 2)); return left;
    }
    public static Int<T> operator /(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T) >> 2); cpu.ipush(&right, sizeof(T) >> 2);
      cpu.idiv(); left = default; cpu.ipop(&left, unchecked((uint)sizeof(T) >> 2)); return left;
    }
    public static Int<T> operator %(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T) >> 2); cpu.ipush(&right, sizeof(T) >> 2);
      cpu.imod(); left = default; cpu.ipop(&left, unchecked((uint)sizeof(T) >> 2)); return left;
    }

    public static bool operator ==(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T) >> 2) == 0;
    }
    public static bool operator !=(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T) >> 2) != 0;
    }
    public static bool operator <=(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T) >> 2) <= 0;
    }
    public static bool operator >=(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T) >> 2) >= 0;
    }
    public static bool operator <(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T) >> 2) < 0;
    }
    public static bool operator >(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T) >> 2) > 0;
    }

    public static Int<T> MaxValue
    {
      get { Int<T> v; new Span<uint>(&v, sizeof(T) >> 2).Fill(0xffffffff); ((uint*)&v)[(sizeof(T) >> 2) - 1] ^= 0x80000000; return v; }
    }
    public static Int<T> MinValue
    {
      get { var v = default(Int<T>); ((uint*)&v)[(sizeof(T) >> 2) - 1] = 0x80000000; return v; }
    }

    public static Int<T> Abs(Int<T> value)
    {
      var s = (((uint*)&value)[(sizeof(T) >> 2) - 1] & 0x80000000) != 0;
      return s ? -value : value;
    }
    public static Int<T> Min(Int<T> x, Int<T> y)
    {
      return CPU.icmp(&x, &y, (sizeof(T) >> 2)) <= 0 ? x : y;
    }
    public static Int<T> Max(Int<T> x, Int<T> y)
    {
      return CPU.icmp(&x, &y, (sizeof(T) >> 2)) >= 0 ? x : y;
    }
    public static bool IsPositive(Int<T> value)
    {
      return (((uint*)&value)[(sizeof(T) >> 2) - 1] & 0x80000000) == 0;
    }
    public static bool IsNegative(Int<T> value)
    {
      return (((uint*)&value)[(sizeof(T) >> 2) - 1] & 0x80000000) != 0;
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
      var t = this; cpu.ipush(&t, sizeof(T) >> 2);
      var n = tos(dest, cpu, fmt, dig, 0, 0, 0);
      if (n > 0) { charsWritten = n; return true; }
      if (dest.Length >= 2) { n = -n; new Span<char>(&n, 2).CopyTo(dest); }
      charsWritten = 0; return false;
    }
    public int CompareTo(object? obj) { return obj == null ? 1 : p is Int<T> b ? this.CompareTo(b) : throw new ArgumentException(); }
    public int CompareTo(Int<T> other) { var a = this; return CPU.icmp(&a, &other, sizeof(T) >> 2); }
    public bool Equals(Int<T> other) { var t = this; return CPU.icmp(&t, &other, sizeof(T) >> 2) == 0; }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      if (obj is not Int<T> b) return false;
      var a = this; return CPU.icmp(&a, &b, sizeof(T) >> 2) == 0;
    }
    public override int GetHashCode()
    {
      var a = this; return CPU.hash(&a, sizeof(T));
    }
  }

#if NET7_0
  public unsafe readonly partial struct Int<T> : IBinaryInteger<Int<T>>, IMinMaxValue<Int<T>>, ISignedNumber<Int<T>>
  {
    public static implicit operator Int<T>(Int128 value)
    {
      var cpu = main_cpu; cpu.ipush(&value, 0x0004);
      var v = default(Int<T>); cpu.ipop(&v, unchecked((uint)sizeof(T) >> 2)); return v;
    }
    public static explicit operator Int128(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T) >> 2);
      var a = default(Int128); cpu.ipop(&a, 0x0004); return a;
    }

    static int INumberBase<Int<T>>.Radix => 2;
    static Int<T> IBinaryNumber<Int<T>>.AllBitsSet => -1;
    static Int<T> INumberBase<Int<T>>.One => 1;
    static Int<T> INumberBase<Int<T>>.Zero => default;
    static Int<T> IAdditiveIdentity<Int<T>, Int<T>>.AdditiveIdentity => default;
    static Int<T> IMultiplicativeIdentity<Int<T>, Int<T>>.MultiplicativeIdentity => 1;
    static Int<T> ISignedNumber<Int<T>>.NegativeOne => -1;
    static bool INumberBase<Int<T>>.IsCanonical(Int<T> value) => true;
    static bool INumberBase<Int<T>>.IsComplexNumber(Int<T> value) => false;
    static bool INumberBase<Int<T>>.IsFinite(Int<T> value) => false;
    static bool INumberBase<Int<T>>.IsRealNumber(Int<T> value) => true;
    static bool INumberBase<Int<T>>.IsImaginaryNumber(Int<T> value) => false;
    static bool INumberBase<Int<T>>.IsInfinity(Int<T> value) => false;
    static bool INumberBase<Int<T>>.IsInteger(Int<T> value) => true;
    static bool INumberBase<Int<T>>.IsPositiveInfinity(Int<T> value) => false;
    static bool INumberBase<Int<T>>.IsSubnormal(Int<T> value) => false;
    static bool INumberBase<Int<T>>.IsNaN(Int<T> value) => false;
    static bool INumberBase<Int<T>>.IsNegativeInfinity(Int<T> value) => true;
    static bool INumberBase<Int<T>>.IsNormal(Int<T> value) => value != default;
    static bool INumberBase<Int<T>>.IsOddInteger(Int<T> value) => (((uint*)&value)[0] & 1) != 0;
    static bool INumberBase<Int<T>>.IsEvenInteger(Int<T> value) => (((uint*)&value)[0] & 1) == 0;     
    static bool INumberBase<Int<T>>.IsZero(Int<T> value) => value == default;
    int IBinaryInteger<Int<T>>.GetByteCount() => sizeof(T);
    
    #region todo
    int IBinaryInteger<Int<T>>.GetShortestBitLength() => throw new NotImplementedException();
    static bool IBinaryNumber<Int<T>>.IsPow2(Int<T> value) => throw new NotImplementedException();
    static Int<T> IBinaryNumber<Int<T>>.Log2(Int<T> value) => throw new NotImplementedException();    
    static Int<T> INumberBase<Int<T>>.MaxMagnitude(Int<T> x, Int<T> y) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.MaxMagnitudeNumber(Int<T> x, Int<T> y) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.MinMagnitude(Int<T> x, Int<T> y) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.MinMagnitudeNumber(Int<T> x, Int<T> y) => throw new NotImplementedException();    
    static Int<T> INumberBase<Int<T>>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.Parse(string s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static Int<T> ISpanParsable<Int<T>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => throw new NotImplementedException();
    static Int<T> IParsable<Int<T>>.Parse(string s, IFormatProvider? provider) => throw new NotImplementedException();
    static Int<T> IBinaryInteger<Int<T>>.PopCount(Int<T> value) => throw new NotImplementedException();
    static Int<T> IBinaryInteger<Int<T>>.TrailingZeroCount(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertFromChecked<TOther>(TOther value, out Int<T> result) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertFromSaturating<TOther>(TOther value, out Int<T> result) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertFromTruncating<TOther>(TOther value, out Int<T> result) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertToChecked<TOther>(Int<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertToSaturating<TOther>(Int<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertToTruncating<TOther>(Int<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Int<T> result) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Int<T> result) => throw new NotImplementedException();
    static bool ISpanParsable<Int<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Int<T> result) => throw new NotImplementedException();
    static bool IParsable<Int<T>>.TryParse(string? s, IFormatProvider? provider, out Int<T> result) => throw new NotImplementedException();
    static bool IBinaryInteger<Int<T>>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Int<T> value) => throw new NotImplementedException();
    static bool IBinaryInteger<Int<T>>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Int<T> value) => throw new NotImplementedException();
    bool IBinaryInteger<Int<T>>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IBinaryInteger<Int<T>>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    static Int<T> IBitwiseOperators<Int<T>, Int<T>, Int<T>>.operator ~(Int<T> value) => throw new NotImplementedException();
    static Int<T> IBitwiseOperators<Int<T>, Int<T>, Int<T>>.operator &(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static Int<T> IBitwiseOperators<Int<T>, Int<T>, Int<T>>.operator |(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static Int<T> IBitwiseOperators<Int<T>, Int<T>, Int<T>>.operator ^(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static Int<T> IShiftOperators<Int<T>, int, Int<T>>.operator <<(Int<T> value, int shiftAmount) => throw new NotImplementedException();
    static Int<T> IShiftOperators<Int<T>, int, Int<T>>.operator >>(Int<T> value, int shiftAmount) => throw new NotImplementedException();
    static Int<T> IShiftOperators<Int<T>, int, Int<T>>.operator >>>(Int<T> value, int shiftAmount) => throw new NotImplementedException();
    #endregion

    // static bool IEqualityOperators<Int<T>, Int<T>, bool>.operator ==(Int<T> left, Int<T> right) => throw new NotImplementedException();
    // static bool IEqualityOperators<Int<T>, Int<T>, bool>.operator !=(Int<T> left, Int<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<Int<T>, Int<T>, bool>.operator <(Int<T> left, Int<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<Int<T>, Int<T>, bool>.operator >(Int<T> left, Int<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<Int<T>, Int<T>, bool>.operator <=(Int<T> left, Int<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<Int<T>, Int<T>, bool>.operator >=(Int<T> left, Int<T> right) => throw new NotImplementedException();
    // static bool INumberBase<Int<T>>.IsPositive(Int<T> value) => throw new NotImplementedException();
    // static bool INumberBase<Int<T>>.IsNegative(Int<T> value) => throw new NotImplementedException();
    // static Int<T> INumberBase<Int<T>>.Abs(Int<T> value) => throw new NotImplementedException();
    // static Int<T> IMinMaxValue<Int<T>>.MaxValue => throw new NotImplementedException();
    // static Int<T> IMinMaxValue<Int<T>>.MinValue => throw new NotImplementedException();
    // static Int<T> IUnaryPlusOperators<Int<T>, Int<T>>.operator +(Int<T> value)
    // static Int<T> IAdditionOperators<Int<T>, Int<T>, Int<T>>.operator +(Int<T> left, Int<T> right)
    // static Int<T> IUnaryNegationOperators<Int<T>, Int<T>>.operator -(Int<T> value)
    // static Int<T> ISubtractionOperators<Int<T>, Int<T>, Int<T>>.operator -(Int<T> left, Int<T> right)
    // static Int<T> IIncrementOperators<Int<T>>.operator ++(Int<T> value) => throw new NotImplementedException();
    // static Int<T> IDecrementOperators<Int<T>>.operator --(Int<T> value) => throw new NotImplementedException();
    // static Int<T> IMultiplyOperators<Int<T>, Int<T>, Int<T>>.operator *(Int<T> left, Int<T> right)
    // static Int<T> IDivisionOperators<Int<T>, Int<T>, Int<T>>.operator /(Int<T> left, Int<T> right)
    // static Int<T> IModulusOperators<Int<T>, Int<T>, Int<T>>.operator %(Int<T> left, Int<T> right)
  }
#endif

}

