
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
      UInt<T> v; cpu.upop(&v, sizeof(T)); return v;
    }
    public static implicit operator UInt<T>(ulong value)
    {
      var cpu = main_cpu; cpu.push(value);
      UInt<T> v; cpu.upop(&v, sizeof(T)); return v;
    }
    public static explicit operator UInt<T>(int value)
    {
      var cpu = main_cpu; cpu.push(value);
      UInt<T> v; cpu.upop(&v, sizeof(T)); return v;
    }
    public static explicit operator UInt<T>(long value)
    {
      var cpu = main_cpu; cpu.push(value);
      UInt<T> v; cpu.upop(&v, sizeof(T)); return v;
    }
    public static explicit operator UInt<T>(decimal value)
    {
      var cpu = main_cpu; cpu.push(value);
      UInt<T> v; cpu.upop(&v, sizeof(T)); return v;
    }
    
    public static explicit operator uint(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value.p, sizeof(T));
      uint a; cpu.upop(&a, sizeof(uint)); return a;
    }
    public static explicit operator ulong(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      ulong a; cpu.upop(&a, sizeof(ulong)); return a;
    }
    public static explicit operator int(UInt<T> value)
    {
      return (int)(uint)value;
    }
    public static explicit operator long(UInt<T> value)
    {
      return (long)(ulong)value;
    }
    public static implicit operator BigRational(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      return cpu.popr();
    }

    public static UInt<T> operator +(UInt<T> value) => value;
    public static UInt<T> operator ++(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      cpu.inc(); cpu.upop(&value, sizeof(T)); return value;
    }
    public static UInt<T> operator --(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      cpu.dec(); cpu.upop(&value, sizeof(T)); return value;
    }
    public static UInt<T> operator +(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.add(); cpu.upop(&left, sizeof(T)); return left;
    }
    public static UInt<T> operator -(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.sub(); cpu.upop(&left, sizeof(T)); return left;
    }
    public static UInt<T> operator *(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.mul(); cpu.upop(&left, sizeof(T)); return left;
    }
    public static UInt<T> operator /(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.idiv(); cpu.upop(&left, sizeof(T)); return left;
    }
    public static UInt<T> operator %(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.imod(); cpu.upop(&left, sizeof(T)); return left;
    }

    public static UInt<T> operator <<(UInt<T> value, int shiftAmount)
    {
      var n = sizeof(T) << 3; shiftAmount &= n - 1;
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      var t = cpu.msb(); if (t + shiftAmount > n) { cpu.pow(2, n - shiftAmount); cpu.dec(); cpu.and(); } //cut high bits       
      cpu.shl(shiftAmount); cpu.upop(&value, sizeof(T)); return value;
    }
    public static UInt<T> operator >>(UInt<T> value, int shiftAmount)
    {
      var n = sizeof(T) << 3; shiftAmount &= n - 1;
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      cpu.shr(shiftAmount); cpu.upop(&value, sizeof(T)); return value;
    }
    public static UInt<T> operator ~(UInt<T> value)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 2), i = 0; i < n; i++)
        ((uint*)&value)[i] = ~((uint*)&value)[i];
      return value;
    }
    public static UInt<T> operator &(UInt<T> left, UInt<T> right)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 2), i = 0; i < n; i++)
        ((uint*)&left)[i] &= ((uint*)&right)[i];
      return left;
      //var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      //cpu.and(); cpu.upop(&left, sizeof(T)); return left;
    }
    public static UInt<T> operator |(UInt<T> left, UInt<T> right)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 2), i = 0; i < n; i++)
        ((uint*)&left)[i] |= ((uint*)&right)[i];
      return left;
      //var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      //cpu.or(); cpu.upop(&left, sizeof(T)); return left;
    }
    public static UInt<T> operator ^(UInt<T> left, UInt<T> right)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 2), i = 0; i < n; i++)
        ((uint*)&left)[i] ^= ((uint*)&right)[i];
      return left;
      //var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      //cpu.xor(); cpu.upop(&left, sizeof(T)); return left;
    }

    public static bool operator ==(UInt<T> left, UInt<T> right)
    {
      return CPU.ucmp(&left, &right, sizeof(T)) == 0;
    }
    public static bool operator !=(UInt<T> left, UInt<T> right)
    {
      return CPU.ucmp(&left, &right, sizeof(T)) != 0;
    }
    public static bool operator <=(UInt<T> left, UInt<T> right)
    {
      return CPU.ucmp(&left, &right, sizeof(T)) <= 0;
    }
    public static bool operator >=(UInt<T> left, UInt<T> right)
    {
      return CPU.ucmp(&left, &right, sizeof(T)) >= 0;
    }
    public static bool operator <(UInt<T> left, UInt<T> right)
    {
      return CPU.ucmp(&left, &right, sizeof(T)) < 0;
    }
    public static bool operator >(UInt<T> left, UInt<T> right)
    {
      return CPU.ucmp(&left, &right, sizeof(T)) > 0;
    }

    public static UInt<T> MaxValue
    {
      get { UInt<T> a; new Span<uint>(&a, sizeof(T) >> 2).Fill(0xffffffff); return a; }
    }
    public static UInt<T> MinValue => default;
    public static UInt<T> One => 1;
    public static UInt<T> Zero => default;

    public static UInt<T> Min(UInt<T> x, UInt<T> y)
    {
      return CPU.ucmp(&x, &y, sizeof(T)) <= 0 ? x : y;
    }
    public static UInt<T> Max(UInt<T> x, UInt<T> y)
    {
      return CPU.ucmp(&x, &y, sizeof(T)) >= 0 ? x : y;
    }
    public static UInt<T> Clamp(UInt<T> value, UInt<T> min, UInt<T> max)
    {
      Debug.Assert(min <= max);
      return CPU.ucmp(&value, &min, sizeof(T)) < 0 ? min : CPU.ucmp(&value, &max, sizeof(T)) > 0 ? max : value;
    }
    public static (UInt<T> Quotient, UInt<T> Remainder) DivRem(UInt<T> left, UInt<T> right)
    {
      //var q = left / right; return (q, left - (q * right));
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.divrem(); cpu.upop(&left, sizeof(T)); cpu.upop(&right, sizeof(T)); return (left, right);
    }
    public static UInt<T> RotateLeft(UInt<T> value, int rotateAmount)
    {
      return (value << rotateAmount) | (value >> ((sizeof(T) << 3) - rotateAmount));
    }
    public static UInt<T> RotateRight(UInt<T> value, int rotateAmount)
    {
      return (value >> rotateAmount) | (value << ((sizeof(T) << 3) - rotateAmount));
    }

    public static int Sign(UInt<T> value) => value == default ? 0 : 1;
    public static bool IsEvenInteger(UInt<T> value) => (((uint*)&value)[0] & 1) == 0;
    public static bool IsOddInteger(UInt<T> value) => (((uint*)&value)[0] & 1) != 0;
    public static bool IsPow2(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      var x = cpu.ipt(); cpu.pop(); return x;
    }
    public static int LeadingZeroCount(UInt<T> x)
    {
      for (int n = (sizeof(T) >> 2) - 1, i = n; i >= 0; i--)
        if (((uint*)&x)[i] != 0)
          return ((n - i) << 5) + BitOperations.LeadingZeroCount(((uint*)&x)[i]);
      return sizeof(T) << 3;
    }
    public static int TrailingZeroCount(UInt<T> x)
    {
      for (int i = 0, n = sizeof(T) >> 2; i < n; i++)
        if (((uint*)&x)[i] != 0)
          return (i << 5) + BitOperations.TrailingZeroCount(((uint*)&x)[i]);
      return sizeof(T) << 3;
    }
    public static uint Log2(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      var x = cpu.msb(); cpu.pop(); return x == 0 ? x : x - 1;
    }
    public static uint PopCount(UInt<T> value)
    {
      uint c = 0; for (int n = sizeof(T) >> 2, i = 0; i < n; i++)
        c += (uint)BitOperations.PopCount(((uint*)&value)[i]);
      return c;
    }
     
    public int CompareTo(object? obj) { return obj == null ? 1 : p is Int<T> b ? this.CompareTo(b) : throw new ArgumentException(); }
    public int CompareTo(UInt<T> other)
    {
      var a = this; return CPU.ucmp(&a, &other, sizeof(T));
    }
    public bool Equals(UInt<T> other)
    {
      var t = this; return CPU.ucmp(&t, &other, sizeof(T)) == 0;
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      if (obj is not UInt<T> b) return false;
      var a = this; return CPU.ucmp(&a, &b, sizeof(T)) == 0;
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
      var t = this; cpu.upush(&t, sizeof(T));
      var n = tos(dest, cpu, fmt, dig, 0, 0, 0x01);
      if (n > 0) { charsWritten = n; return true; }
      if (dest.Length >= 2) { n = -n; new Span<char>(&n, 2).CopyTo(dest); }
      charsWritten = 0; return false;
    }
    
    public static UInt<T> Parse(string s) => Parse(s.AsSpan(), NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
    public static UInt<T> Parse(string s, NumberStyles style) => Parse(s.AsSpan(), style, NumberFormatInfo.CurrentInfo);
    public static UInt<T> Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), NumberStyles.Integer, provider);
    public static UInt<T> Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s.AsSpan(), style, provider);
    public static UInt<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);
    public static UInt<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider = null)
    {
      var cpu = main_cpu; cpu.tor(s = s.Trim(), (style & NumberStyles.AllowHexSpecifier) != 0 ? 16 : 10);
      UInt<T> v; cpu.upop(&v, sizeof(T)); return v;
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out UInt<T> result)
    {
      var cpu = main_cpu; cpu.tor(s = s.Trim(), (style & NumberStyles.AllowHexSpecifier) != 0 ? 16 : 10);
      UInt<T> v; cpu.upop(&v, sizeof(T)); result = v; return true;
    }
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UInt<T> result) => TryParse(s.AsSpan(), style, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out UInt<T> result) => TryParse(s, NumberStyles.Integer, provider, out result);
    public static bool TryParse(string? s, IFormatProvider? provider, out UInt<T> result) => TryParse(s.AsSpan(), NumberStyles.Integer, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, out UInt<T> result) => TryParse(s, NumberStyles.Integer, null, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, out UInt<T> result) => TryParse(s.AsSpan(), NumberStyles.Integer, null, out result);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly T p;

#if NET6_0
    public static UInt<T> CreateTruncating<TOther>(TOther value) where TOther : struct 
    {
      UInt<T> a; if (main_cpu.cast(value, out a, 0)) return a;
      throw new NotSupportedException();
    }
    public static UInt<T> CreateSaturating<TOther>(TOther value) where TOther : struct
    {
      UInt<T> a; if (main_cpu.cast(value, out a, 1)) return a;
      throw new NotSupportedException();
    }
    public static UInt<T> CreateChecked<TOther>(TOther value) where TOther : struct
    {
      UInt<T> a; if (main_cpu.cast(value, out a, 2)) return a;
      throw new NotSupportedException();
    }
#endif
#if NET7_0
    public static UInt<T> CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      UInt<T> a; if (main_cpu.cast(value, out a, 0) || TOther.TryConvertToTruncating(value, out a)) return a;
      throw new NotSupportedException();
    }
    public static UInt<T> CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      UInt<T> a; if (main_cpu.cast(value, out a, 1) || TOther.TryConvertToSaturating(value, out a)) return a;
      throw new NotSupportedException();
    }
    public static UInt<T> CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      UInt<T> a; if (main_cpu.cast(value, out a, 2) || TOther.TryConvertToChecked(value, out a)) return a;
      throw new NotSupportedException();
    }
#endif
  }

#if NET7_0
  public unsafe readonly partial struct UInt<T> : IBinaryInteger<UInt<T>>, IMinMaxValue<UInt<T>>, IUnsignedNumber<UInt<T>>
  {
    public static implicit operator UInt<T>(UInt128 value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(UInt128));
      UInt<T> v; cpu.upop(&v, sizeof(T)); return v;
    }
    public static explicit operator UInt128(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      UInt128 a; cpu.upop(&a, sizeof(UInt128)); return a;
    }

    public static UInt<T> operator >>>(UInt<T> value, int shiftAmount) => value >> shiftAmount;

    static int INumberBase<UInt<T>>.Radix => 2;
    static UInt<T> IBinaryNumber<UInt<T>>.AllBitsSet => MaxValue;
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
    static UInt<T> INumberBase<UInt<T>>.Abs(UInt<T> value) => value;
    static UInt<T> INumberBase<UInt<T>>.MaxMagnitude(UInt<T> x, UInt<T> y) => Max(x, y);
    static UInt<T> INumberBase<UInt<T>>.MaxMagnitudeNumber(UInt<T> x, UInt<T> y) => Max(x, y);
    static UInt<T> INumberBase<UInt<T>>.MinMagnitude(UInt<T> x, UInt<T> y) => Min(x, y);
    static UInt<T> INumberBase<UInt<T>>.MinMagnitudeNumber(UInt<T> x, UInt<T> y) => Min(x, y);
    static UInt<T> IBinaryNumber<UInt<T>>.Log2(UInt<T> value) => Log2(value);
    static UInt<T> IBinaryInteger<UInt<T>>.PopCount(UInt<T> value) => PopCount(value);
    static UInt<T> IUnaryNegationOperators<UInt<T>, UInt<T>>.operator -(UInt<T> value)
    {
      CPU.ineg(&value, sizeof(T)); return value; //todo: check
    }
    static UInt<T> IUnaryNegationOperators<UInt<T>, UInt<T>>.operator checked -(UInt<T> value)
    {
      if (!CPU.ineg(&value, sizeof(T))) throw new OverflowException(); return value; //todo: check
    }
    static UInt<T> IBinaryInteger<UInt<T>>.TrailingZeroCount(UInt<T> value) => (UInt<T>)TrailingZeroCount(value);
    int IBinaryInteger<UInt<T>>.GetShortestBitLength() => (sizeof(T) << 3) - LeadingZeroCount(this);
    int IBinaryInteger<UInt<T>>.GetByteCount() => sizeof(T);
    bool IBinaryInteger<UInt<T>>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      if (destination.Length < sizeof(UInt<T>)) { bytesWritten = 0; return false; }
      var t = this; new ReadOnlySpan<byte>(&t, sizeof(UInt<T>)).CopyTo(destination);
      bytesWritten = sizeof(UInt<T>); return true;
    }
    bool IBinaryInteger<UInt<T>>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
    {
      if (destination.Length < sizeof(UInt<T>)) { bytesWritten = 0; return false; }
      var t = this; new ReadOnlySpan<byte>(&t, sizeof(UInt<T>)).CopyTo(destination);
      destination.Slice(0, sizeof(UInt<T>)).Reverse(); bytesWritten = sizeof(UInt<T>); return true;
    }
    static bool IBinaryInteger<UInt<T>>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt<T> value)
    {
      if (source.Length != sizeof(UInt<T>) || !isUnsigned) { value = default; return false; }
      UInt<T> t; var s = new Span<byte>(&t, sizeof(UInt<T>)); source.CopyTo(s); value = t; return true;
    }
    static bool IBinaryInteger<UInt<T>>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt<T> value)
    {
      if (source.Length != sizeof(UInt<T>) || !isUnsigned) { value = default; return false; }
      UInt<T> t; var s = new Span<byte>(&t, sizeof(UInt<T>)); source.CopyTo(s); s.Reverse(); value = t; return true;
    }

    static bool INumberBase<UInt<T>>.TryConvertFromTruncating<TOther>(TOther value, out UInt<T> result) => main_cpu.cast(value, out result, 0);
    static bool INumberBase<UInt<T>>.TryConvertFromSaturating<TOther>(TOther value, out UInt<T> result) => main_cpu.cast(value, out result, 1);
    static bool INumberBase<UInt<T>>.TryConvertFromChecked<TOther>(TOther value, out UInt<T> result) => main_cpu.cast(value, out result, 2);
    static bool INumberBase<UInt<T>>.TryConvertToTruncating<TOther>(UInt<T> value, out TOther result) where TOther : default => main_cpu.cast(value, out result, 0);
    static bool INumberBase<UInt<T>>.TryConvertToSaturating<TOther>(UInt<T> value, out TOther result) where TOther : default => main_cpu.cast(value, out result, 1);
    static bool INumberBase<UInt<T>>.TryConvertToChecked<TOther>(UInt<T> value, out TOther result) where TOther : default => main_cpu.cast(value, out result, 2);
  }
#endif

}

