
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
      Int<T> v; cpu.ipop(&v, sizeof(T)); return v;
    }
    public static implicit operator Int<T>(long value)
    {
      var cpu = main_cpu; cpu.push(value);
      Int<T> v; cpu.ipop(&v, sizeof(T)); return v;
    }

    public static explicit operator int(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value.p, sizeof(T));
      int a; cpu.ipop(&a, sizeof(int)); return a;
    }
    public static explicit operator long(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T));
      long a; cpu.ipop(&a, sizeof(long)); return a;
    }

    public static Int<T> operator +(Int<T> value) => value;
    public static Int<T> operator -(Int<T> value)
    {
      CPU.ineg(&value, sizeof(T)); return value;
    }
    public static Int<T> operator ++(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T));
      cpu.inc(); cpu.ipop(&value, sizeof(T)); return value;
    }
    public static Int<T> operator --(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T));
      cpu.dec(); cpu.ipop(&value, sizeof(T)); return value;
    }
    public static Int<T> operator +(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T)); cpu.ipush(&right, sizeof(T));
      cpu.add(); cpu.ipop(&left, sizeof(T)); return left;
    }
    public static Int<T> operator -(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T)); cpu.ipush(&right, sizeof(T));
      cpu.sub(); cpu.ipop(&left, sizeof(T)); return left;
    }
    public static Int<T> operator *(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T)); cpu.ipush(&right, sizeof(T));
      cpu.mul(); cpu.ipop(&left, sizeof(T)); return left;
    }
    public static Int<T> operator /(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T)); cpu.ipush(&right, sizeof(T));
      cpu.idiv(); cpu.ipop(&left, sizeof(T)); return left;
    }
    public static Int<T> operator %(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T)); cpu.ipush(&right, sizeof(T));
      cpu.imod(); cpu.ipop(&left, sizeof(T)); return left;
    }

    public static Int<T> operator ~(Int<T> value)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 2), i = 0; i < n; i++)
        ((uint*)&value)[i] = ~((uint*)&value)[i];
      return value;
    }
    public static Int<T> operator &(Int<T> left, Int<T> right)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 2), i = 0; i < n; i++)
        ((uint*)&left)[i] &= ((uint*)&right)[i];
      return left;
      //var cpu = main_cpu; cpu.ipush(&left, sizeof(T)); cpu.ipush(&right, sizeof(T));
      //cpu.and(); cpu.ipop(&left, sizeof(T)); return left;
    }
    public static Int<T> operator |(Int<T> left, Int<T> right)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 2), i = 0; i < n; i++)
        ((uint*)&left)[i] |= ((uint*)&right)[i];
      return left;
      //var cpu = main_cpu; cpu.ipush(&left, sizeof(T)); cpu.ipush(&right, sizeof(T));
      //cpu.or(); cpu.ipop(&left, sizeof(T)); return left;
    }
    public static Int<T> operator ^(Int<T> left, Int<T> right)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 2), i = 0; i < n; i++)
        ((uint*)&left)[i] ^= ((uint*)&right)[i];
      return left;
      //var cpu = main_cpu; cpu.ipush(&left, sizeof(T)); cpu.ipush(&right, sizeof(T));
      //cpu.xor(); cpu.ipop(&left, sizeof(T)); return left;
    }

    public static Int<T> operator <<(Int<T> value, int shiftAmount)
    {
      var n = sizeof(T) << 3; shiftAmount &= n - 1;
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      var t = cpu.msb(); if (t + shiftAmount > n) { cpu.pow(2, n - shiftAmount); cpu.dec(); cpu.and(); } //cut high bits       
      cpu.shl(shiftAmount); cpu.upop(&value, sizeof(T)); return value;
    }
    public static Int<T> operator >>(Int<T> value, int shiftAmount)
    {
      var n = sizeof(T) << 3; shiftAmount &= n - 1;
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T)); var s = cpu.sign();
      if (s < 0) { cpu.neg(); cpu.dec(); }
      cpu.shr(shiftAmount);
      if (s < 0) { cpu.inc(); cpu.neg(); }
      cpu.ipop(&value, sizeof(T)); return value;
    }

    public static bool operator ==(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T)) == 0;
    }
    public static bool operator !=(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T)) != 0;
    }
    public static bool operator <=(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T)) <= 0;
    }
    public static bool operator >=(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T)) >= 0;
    }
    public static bool operator <(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T)) < 0;
    }
    public static bool operator >(Int<T> left, Int<T> right)
    {
      return CPU.icmp(&left, &right, sizeof(T)) > 0;
    }

    public static Int<T> MaxValue
    {
      get { Int<T> v; new Span<uint>(&v, sizeof(T) >> 2).Fill(0xffffffff); ((uint*)&v)[(sizeof(T) >> 2) - 1] ^= 0x80000000; return v; }
    }
    public static Int<T> MinValue
    {
      get { var v = default(Int<T>); ((uint*)&v)[(sizeof(T) >> 2) - 1] = 0x80000000; return v; }
    }
    public static Int<T> One => 1;
    public static Int<T> Zero => default;
    public static Int<T> NegativeOne => -1;

    public static Int<T> Abs(Int<T> value)
    {
      var s = (((uint*)&value)[(sizeof(T) >> 2) - 1] & 0x80000000) != 0;
      return s ? -value : value;
    }
    public static Int<T> Min(Int<T> x, Int<T> y)
    {
      return CPU.icmp(&x, &y, sizeof(T)) <= 0 ? x : y;
    }
    public static Int<T> Max(Int<T> x, Int<T> y)
    {
      return CPU.icmp(&x, &y, sizeof(T)) >= 0 ? x : y;
    }
    public static Int<T> MaxMagnitude(Int<T> x, Int<T> y)
    {
      var cpu = main_cpu; cpu.ipush(&x, sizeof(T)); cpu.ipush(&y, sizeof(T));
      var s = cpu.cmpa(); cpu.pop(2); return s <= 0 ? x : y;
    }
    public static Int<T> MinMagnitude(Int<T> x, Int<T> y)
    {
      var cpu = main_cpu; cpu.ipush(&x, sizeof(T)); cpu.ipush(&y, sizeof(T));
      var s = cpu.cmpa(); cpu.pop(2); return s >= 0 ? x : y;
    }
    public static Int<T> Clamp(Int<T> value, Int<T> min, Int<T> max)
    {
      Debug.Assert(min <= max);
      return CPU.icmp(&value, &min, sizeof(T)) < 0 ? min : CPU.icmp(&value, &max, sizeof(T)) > 0 ? max : value;
    }

    public static int Sign(Int<T> value)
    {
      return (((uint*)&value)[(sizeof(T) >> 2) - 1] & 0x80000000) != 0 ? -1 : value == default ? 0 : +1;
    }
    public static bool IsPositive(Int<T> value)
    {
      return (((uint*)&value)[(sizeof(T) >> 2) - 1] & 0x80000000) == 0;
    }
    public static bool IsNegative(Int<T> value)
    {
      return (((uint*)&value)[(sizeof(T) >> 2) - 1] & 0x80000000) != 0;
    }
    public static bool IsOddInteger(Int<T> value)
    {
      return (((uint*)&value)[0] & 1) != 0;
    }
    public static bool IsEvenInteger(Int<T> value)
    {
      return (((uint*)&value)[0] & 1) == 0;
    }
    public static (Int<T> Quotient, Int<T> Remainder) DivRem(Int<T> left, Int<T> right)
    {
      //var q = left / right; return (q, left - (q * right));
      var cpu = main_cpu; cpu.ipush(&left, sizeof(T)); cpu.ipush(&right, sizeof(T));
      cpu.divrem(); cpu.ipop(&left, sizeof(T)); cpu.ipop(&right, sizeof(T)); return (left, right);
    }
    public static bool IsPow2(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T));
      var x = cpu.sign() > 0 && cpu.ipt(); cpu.pop(); return x;
    }
    public static Int<T> Log2(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T));
      var x = cpu.msb(); cpu.pop(); return x == 0 ? x : x - 1;
    }
    public static int PopCount(Int<T> value)
    {
      uint c = 0, n, i;
      for (n = unchecked((uint)sizeof(T) >> 2), i = 0; i < n; i++)
        c += (uint)BitOperations.PopCount(((uint*)&value)[i]);
      return unchecked((int)c);
    }
    public static int LeadingZeroCount(Int<T> x)
    {
      for (int n = (sizeof(T) >> 2) - 1, i = n; i >= 0; i--)
        if (((uint*)&x)[i] != 0)
          return ((n - i) << 5) + BitOperations.LeadingZeroCount(((uint*)&x)[i]);
      return sizeof(T) << 3;
    }
    public static int TrailingZeroCount(Int<T> x)
    {
      for (int i = 0, n = sizeof(T) >> 2; i < n; i++)
        if (((uint*)&x)[i] != 0)
          return (i << 5) + BitOperations.TrailingZeroCount(((uint*)&x)[i]);
      return sizeof(T) << 3;
    }
    public static Int<T> CopySign(Int<T> value, Int<T> sign)
    {
      if (Sign(value) < 0) value = -value;
      if (Sign(sign) >= 0) { if (Sign(value) < 0) throw new OverflowException(); return value; }
      return -value;
    }

    public int CompareTo(object? obj) { return obj == null ? 1 : p is Int<T> b ? this.CompareTo(b) : throw new ArgumentException(); }
    public int CompareTo(Int<T> other) { var t = this; return CPU.icmp(&t, &other, sizeof(T)); }
    public bool Equals(Int<T> other) { var t = this; return CPU.icmp(&t, &other, sizeof(T)) == 0; }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      if (obj is not Int<T> b) return false;
      var a = this; return CPU.icmp(&a, &b, sizeof(T)) == 0;
    }
    public override int GetHashCode()
    {
      var a = this; return CPU.hash(&a, sizeof(T));
    }

    public readonly override string ToString() => ToString(null, null);
    public string ToString(string? format) => ToString(format, null);
    public string ToString(IFormatProvider? formatProvider) => ToString(null, formatProvider);
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
      var t = this; cpu.ipush(&t, sizeof(T));
      var n = tos(dest, cpu, fmt, dig, 0, 0, 0);
      if (n > 0) { charsWritten = n; return true; }
      if (dest.Length >= 2) { n = -n; new Span<char>(&n, 2).CopyTo(dest); }
      charsWritten = 0; return false;
    }
     
    public static Int<T> Parse(string s) => Parse(s.AsSpan(), NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
    public static Int<T> Parse(string s, NumberStyles style) => Parse(s.AsSpan(), style, NumberFormatInfo.CurrentInfo);
    public static Int<T> Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), NumberStyles.Integer, provider);
    public static Int<T> Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s.AsSpan(), style, provider);
    public static Int<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);
    public static Int<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider = null)
    {
      var cpu = main_cpu; cpu.tor(s = s.Trim(), (style & NumberStyles.AllowHexSpecifier) != 0 ? 16 : 10);
      Int<T> v; cpu.ipop(&v, sizeof(T)); return v;
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Int<T> result)
    {
      if (s.Length == 0) { result = default; return false; }
      var cpu = main_cpu; cpu.tor(s = s.Trim(), (style & NumberStyles.AllowHexSpecifier) != 0 ? 16 : 10);
      Int<T> v; cpu.ipop(&v, sizeof(T)); result = v; return true;
    }
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Int<T> result) => TryParse(s.AsSpan(), style, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Int<T> result) => TryParse(s, NumberStyles.Integer, provider, out result);
    public static bool TryParse(string? s, IFormatProvider? provider, out Int<T> result) => TryParse(s.AsSpan(), NumberStyles.Integer, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, out Int<T> result) => TryParse(s, NumberStyles.Integer, null, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, out Int<T> result) => TryParse(s.AsSpan(), NumberStyles.Integer, null, out result);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly T p;

#if NET6_0
    public static Int<T> CreateTruncating<TOther>(TOther value) where TOther : struct
    {
      Int<T> a; if (main_cpu.cast(value, out a, 0)) return a;
      throw new NotSupportedException();
    }
    public static Int<T> CreateSaturating<TOther>(TOther value) where TOther : struct
    {
      Int<T> a; if (main_cpu.cast(value, out a, 1)) return a;
      throw new NotSupportedException();
    }
    public static Int<T> CreateChecked<TOther>(TOther value) where TOther : struct
    {
      Int<T> a; if (main_cpu.cast(value, out a, 2)) return a;
      throw new NotSupportedException();
    }

#endif
#if NET7_0
    public static Int<T> CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      Int<T> a; if (main_cpu.cast(value, out a, 0) || TOther.TryConvertToTruncating(value, out a)) return a;
      throw new NotSupportedException();
    }
    public static Int<T> CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      Int<T> a; if (main_cpu.cast(value, out a, 1) || TOther.TryConvertToSaturating(value, out a)) return a;
      throw new NotSupportedException();
    }
    public static Int<T> CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      Int<T> a; if (main_cpu.cast(value, out a, 2) || TOther.TryConvertToChecked(value, out a)) return a;
      throw new NotSupportedException();
    }
#endif  
  }

#if NET7_0
  public unsafe readonly partial struct Int<T> : IBinaryInteger<Int<T>>, IMinMaxValue<Int<T>>, ISignedNumber<Int<T>>
  {
    public static implicit operator Int<T>(Int128 value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(Int128));
      Int<T> v; cpu.ipop(&v, sizeof(T)); return v;
    }
    public static explicit operator Int128(Int<T> value)
    {
      var cpu = main_cpu; cpu.ipush(&value, sizeof(T));
      Int128 a; cpu.ipop(&a, sizeof(Int128)); return a;
    }
    public static Int<T> operator >>>(Int<T> value, int shiftAmount)
    {
      var n = sizeof(T) << 3; shiftAmount &= n - 1;
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      cpu.shr(shiftAmount); cpu.upop(&value, sizeof(T)); return value;
    }
    public static Int<T> RotateLeft(Int<T> value, int rotateAmount)
    {
      return (value << rotateAmount) | (value >>> ((sizeof(T) << 3) - rotateAmount));
    }
    public static Int<T> RotateRight(Int<T> value, int rotateAmount)
    {
      return (value >>> rotateAmount) | (value << ((sizeof(T) << 3) - rotateAmount));
    }

    static int INumberBase<Int<T>>.Radix => 2;
    static Int<T> IBinaryNumber<Int<T>>.AllBitsSet => NegativeOne;
    static Int<T> IAdditiveIdentity<Int<T>, Int<T>>.AdditiveIdentity => default;
    static Int<T> IMultiplicativeIdentity<Int<T>, Int<T>>.MultiplicativeIdentity => 1;
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
    static bool INumberBase<Int<T>>.IsZero(Int<T> value) => value == default;

    static Int<T> INumberBase<Int<T>>.MaxMagnitudeNumber(Int<T> x, Int<T> y) => MaxMagnitude(x, y);
    static Int<T> INumberBase<Int<T>>.MinMagnitudeNumber(Int<T> x, Int<T> y) => MinMagnitude(x, y);
    static Int<T> IBinaryInteger<Int<T>>.PopCount(Int<T> value) => PopCount(value);
    static Int<T> IBinaryInteger<Int<T>>.TrailingZeroCount(Int<T> value) => TrailingZeroCount(value);
    static Int<T> IBinaryInteger<Int<T>>.LeadingZeroCount(Int<T> value) => LeadingZeroCount(value);
    int IBinaryInteger<Int<T>>.GetShortestBitLength()
    {
      return this >= 0 ? //todo: opt.
       (sizeof(T) << 3) - LeadingZeroCount(this) :
       (sizeof(T) << 3) + 1 - LeadingZeroCount(~this);
    }

    int IBinaryInteger<Int<T>>.GetByteCount() => sizeof(T);
    bool IBinaryInteger<Int<T>>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      if (destination.Length < sizeof(Int<T>)) { bytesWritten = 0; return false; }
      var t = this; new ReadOnlySpan<byte>(&t, sizeof(Int<T>)).CopyTo(destination);
      bytesWritten = sizeof(Int<T>); return true;
    }
    bool IBinaryInteger<Int<T>>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
    {
      if (destination.Length < sizeof(Int<T>)) { bytesWritten = 0; return false; }
      var t = this; new ReadOnlySpan<byte>(&t, sizeof(Int<T>)).CopyTo(destination);
      destination.Slice(0, sizeof(Int<T>)).Reverse(); bytesWritten = sizeof(Int<T>); return true;
    }
    static bool IBinaryInteger<Int<T>>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Int<T> value)
    {
      if (source.Length != sizeof(Int<T>) || isUnsigned) { value = default; return false; }
      Int<T> t; var s = new Span<byte>(&t, sizeof(Int<T>)); source.CopyTo(s); value = t; return true;
    }
    static bool IBinaryInteger<Int<T>>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Int<T> value)
    {
      if (source.Length != sizeof(Int<T>) || isUnsigned) { value = default; return false; }
      Int<T> t; var s = new Span<byte>(&t, sizeof(Int<T>)); source.CopyTo(s); s.Reverse(); value = t; return true;
    }

    static bool INumberBase<Int<T>>.TryConvertFromTruncating<TOther>(TOther value, out Int<T> result) => main_cpu.cast(value, out result, 0);
    static bool INumberBase<Int<T>>.TryConvertFromSaturating<TOther>(TOther value, out Int<T> result) => main_cpu.cast(value, out result, 1);
    static bool INumberBase<Int<T>>.TryConvertFromChecked<TOther>(TOther value, out Int<T> result) => main_cpu.cast(value, out result, 2);
    static bool INumberBase<Int<T>>.TryConvertToTruncating<TOther>(Int<T> value, out TOther result) where TOther : default => main_cpu.cast(value, out result, 0);
    static bool INumberBase<Int<T>>.TryConvertToSaturating<TOther>(Int<T> value, out TOther result) where TOther : default => main_cpu.cast(value, out result, 1);
    static bool INumberBase<Int<T>>.TryConvertToChecked<TOther>(Int<T> value, out TOther result) where TOther : default => main_cpu.cast(value, out result, 2);
  }
#endif

}

