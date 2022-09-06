
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
    public static explicit operator UInt<T>(int value)
    {
      //return (uint)value; //checked value < 0 exc
      var cpu = main_cpu; cpu.push(value);
      UInt<T> v; cpu.ipop(&v, 0x0300 | (sizeof(T) >> 2)); return v;
    }
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
    public static explicit operator UInt<T>(decimal value)
    {
      var cpu = main_cpu; cpu.push(value);
      UInt<T> v; cpu.upop(&v, sizeof(T)); return v;
    }

    public static explicit operator int(UInt<T> value)
    {
      return (int)(uint)value;
    }
    public static explicit operator uint(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value.p, sizeof(T));
      var a = default(uint); cpu.ipop(&a, 0x0101); return a;
    }
    public static explicit operator ulong(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      var a = default(ulong); cpu.ipop(&a, 0x0102); return a;
    }

    public static UInt<T> operator +(UInt<T> value) => value;
    public static UInt<T> operator -(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      cpu.neg(); cpu.upop(&value, sizeof(T)); return value;
    }
    public static UInt<T> operator ++(UInt<T> value)
    {
      var cpu = main_cpu; cpu.upush(&value, 4);
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
      //Debug.Assert(shiftAmount >= 0); //todo: -shiftAmount 
      var n = sizeof(T) << 3; shiftAmount &= n - 1;
      //if (shiftAmount > n) shiftAmount %= n; if (shiftAmount == 0) return value;
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      var t = cpu.msb(); if (t + shiftAmount > n) { cpu.pow(2, n - shiftAmount); cpu.dec(); cpu.and(); } //cut high bits       
      cpu.shl(shiftAmount); cpu.upop(&value, sizeof(T));
      return value;
    }
    public static UInt<T> operator >>(UInt<T> value, int shiftAmount)
    {
      var n = sizeof(T) << 3; shiftAmount &= n - 1;
      //Debug.Assert(shiftAmount >= 0); //todo: -shiftAmount
      var cpu = main_cpu; cpu.upush(&value, sizeof(T));
      cpu.shr(shiftAmount); cpu.upop(&value, sizeof(T)); return value;
    }
    public static UInt<T> operator ~(UInt<T> value)
    {
      for (int n = sizeof(T) >> 2, i = 0; i < n; i++) ((uint*)&value)[i] = ~((uint*)&value)[i];
      return value;
    }
    public static UInt<T> operator &(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.and(); cpu.upop(&left, sizeof(T)); return left;
    }
    public static UInt<T> operator |(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.or(); cpu.upop(&left, sizeof(T)); return left;
    }
    public static UInt<T> operator ^(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.xor(); cpu.upop(&left, sizeof(T)); return left;
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
    public static UInt<T> One => 1;
    public static UInt<T> Zero => default;

    public static UInt<T> Min(UInt<T> x, UInt<T> y)
    {
      return CPU.icmp(&x, &y, 0x100 | (sizeof(T) >> 2)) <= 0 ? x : y;
    }
    public static UInt<T> Max(UInt<T> x, UInt<T> y)
    {
      return CPU.icmp(&x, &y, 0x100 | (sizeof(T) >> 2)) >= 0 ? x : y;
    }
    public static UInt<T> Clamp(UInt<T> value, UInt<T> min, UInt<T> max)
    {
      Debug.Assert(min <= max);
      return value < min ? min : value > max ? max : value;
    }
    public static (UInt<T> Quotient, UInt<T> Remainder) DivRem(UInt<T> left, UInt<T> right)
    {
      //var q = left / right; return (q, left - (q * right));
      var cpu = main_cpu; cpu.upush(&left, sizeof(T)); cpu.upush(&right, sizeof(T));
      cpu.idr(); cpu.upop(&left, sizeof(T)); cpu.upop(&right, sizeof(T)); return (left, right);
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
      return 0x7fffffff;
    }
    public static int TrailingZeroCount(UInt<T> x)
    {
      for (int i = 0, n = sizeof(T) >> 2; i < n; i++)
        if (((uint*)&x)[i] != 0)
          return (i << 3) + BitOperations.TrailingZeroCount(((uint*)&x)[i]);
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
      var t = this; cpu.upush(&t, sizeof(T));
      var n = tos(dest, cpu, fmt, dig, 0, 0, 0x01);
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

    public static UInt<T> Parse(string s) => Parse(s.AsSpan(), NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
    public static UInt<T> Parse(string s, NumberStyles style) => Parse(s.AsSpan(), style, NumberFormatInfo.CurrentInfo);
    public static UInt<T> Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), NumberStyles.Integer, provider);
    public static UInt<T> Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s.AsSpan(), style, provider);
    public static UInt<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, NumberStyles.Integer, provider);
    public static UInt<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider = null)
    {
      var cpu = main_cpu; cpu.tor(s = s.Trim(), (style & NumberStyles.AllowHexSpecifier) != 0 ? 16 : 10);
      UInt<T> v; cpu.ipop(&v, 0x0300 | (sizeof(T) >> 2)); return v;
    }
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out UInt<T> result)
    {
      var cpu = main_cpu; cpu.tor(s = s.Trim(), (style & NumberStyles.AllowHexSpecifier) != 0 ? 16 : 10);
      UInt<T> v; cpu.ipop(&v, 0x0300 | (sizeof(T) >> 2)); result = v; return true;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly T p;
  }

#if NET7_0
  public unsafe readonly partial struct UInt<T> : IBinaryInteger<UInt<T>>, IMinMaxValue<UInt<T>>, IUnsignedNumber<UInt<T>>
  {
    public static UInt<T> CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      UInt<T> a; if (TryConvertFromChecked(value, out a) || TOther.TryConvertToChecked(value, out a)) return a;
      throw new NotSupportedException();
    }
    public static UInt<T> CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      UInt<T> a; if (TryConvertFromSaturating(value, out a) || TOther.TryConvertToSaturating(value, out a)) return a;
      throw new NotSupportedException();
    }
    public static UInt<T> CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      UInt<T> a; if (TryConvertFromTruncating(value, out a) || TOther.TryConvertToTruncating(value, out a)) return a;
      throw new NotSupportedException();
    }

    static bool TryConvertFromTruncating<TOther>(TOther value, out UInt<T> result) //where TOther : INumberBase<TOther>
    {
      var type = typeof(TOther);
      if(type.IsPrimitive)
      {
        var tc = Type.GetTypeCode(type);
        switch (tc) { case TypeCode.Int32: break; }
      }
      else if (type.IsGenericType)
      {
        var p = Unsafe.AsPointer(ref value); 
        var n = Unsafe.SizeOf<TOther>();
        var t = type.GetGenericTypeDefinition();
        var cpu = main_cpu;
        if (t == typeof(UInt<>)) 
        {
          cpu.upush(p, n); UInt<T> v; cpu.upop(&v, sizeof(T)); result = v; return true; 
        }
        if (t == typeof(Int<>)) { result = default; return false; }
        if (t == typeof(Decimal<>)) { result = default; return false; }
        if (t == typeof(Float<>)) { result = default; return false; }
      }

      { if (value is UInt<T> a) { result = a; return true; } }
      { if (value is int a) { result = (UInt<T>)a; return true; } }
      { if (value is uint a) { result = (UInt<T>)a; return true; } }
      { if (value is ulong a) { result = (UInt<T>)a; return true; } }
      { if (value is UInt128 a) { result = (UInt<T>)a; return true; } }
      
      result = default; return false;
    }
    static bool TryConvertToTruncating<TOther>(UInt<T> value, out TOther result) //where TOther : default
    {
      if (typeof(TOther) == typeof(int))
      {
        int actualResult = (int)value;
        result = (TOther)(object)actualResult;
        return true;
      }

      result = default!; return false;
    }
    static bool TryConvertFromChecked<TOther>(TOther value, out UInt<T> result) //where TOther : INumberBase<TOther>
    {
      return TryConvertFromTruncating<TOther>(value, out result);
    }
    static bool TryConvertFromSaturating<TOther>(TOther value, out UInt<T> result) //where TOther : INumberBase<TOther>
    {
      return TryConvertFromTruncating<TOther>(value, out result);
    }
    static bool TryConvertToChecked<TOther>(UInt<T> value, out TOther result) //where TOther : default
    {
      return TryConvertToTruncating(value, out result);
    }
    static bool TryConvertToSaturating<TOther>(UInt<T> value, out TOther result) //where TOther : default
    {
      return TryConvertToTruncating(value, out result);
    }

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
    static UInt<T> IBinaryInteger<UInt<T>>.TrailingZeroCount(UInt<T> value) => (UInt<T>)TrailingZeroCount(value);
    int IBinaryInteger<UInt<T>>.GetShortestBitLength() => (sizeof(T) << 3) - (int)LeadingZeroCount(this);
    int IBinaryInteger<UInt<T>>.GetByteCount() => sizeof(T);
    static bool INumberBase<UInt<T>>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UInt<T> result) => TryParse(s.AsSpan(), style, provider, out result);
    static bool ISpanParsable<UInt<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out UInt<T> result) => TryParse(s, NumberStyles.Integer, provider, out result);
    static bool IParsable<UInt<T>>.TryParse(string? s, IFormatProvider? provider, out UInt<T> result) => TryParse(s.AsSpan(), NumberStyles.Integer, provider, out result);
    static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out UInt<T> result) => TryParse(s, NumberStyles.Integer, provider, out result);
    bool IBinaryInteger<UInt<T>>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
    {
      if (destination.Length < sizeof(UInt<T>)) { bytesWritten = 0; return false; }
      var t = this; new ReadOnlySpan<byte>(&t, sizeof(UInt<T>)).CopyTo(destination);
      bytesWritten = sizeof(UInt<T>); return true;
    }
    bool IBinaryInteger<UInt<T>>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      if (destination.Length < sizeof(UInt<T>)) { bytesWritten = 0; return false; }
      var t = this; new ReadOnlySpan<byte>(&t, sizeof(UInt<T>)).CopyTo(destination);
      destination.Slice(0, sizeof(UInt<T>)).Reverse();
      bytesWritten = sizeof(UInt<T>); return true;
    }
    static bool IBinaryInteger<UInt<T>>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt<T> value)
    {
      if (source.Length != sizeof(UInt<T>) || !isUnsigned) { value = default; return false; }
      UInt<T> t; var s = new Span<byte>(&t, sizeof(UInt<T>)); source.CopyTo(s); value = t; return true;
    }
    static bool IBinaryInteger<UInt<T>>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt<T> value)
    {
      if (source.Length != sizeof(UInt<T>) || !isUnsigned) { value = default; return false; }
      UInt<T> t; var s = new Span<byte>(&t, sizeof(UInt<T>)); source.CopyTo(s); s.Reverse(); value = t; return true;
    }
    static bool INumberBase<UInt<T>>.TryConvertFromTruncating<TOther>(TOther value, out UInt<T> result) => TryConvertFromTruncating<TOther>(value, out result);
    static bool INumberBase<UInt<T>>.TryConvertFromChecked<TOther>(TOther value, out UInt<T> result) => TryConvertFromChecked<TOther>(value, out result);
    static bool INumberBase<UInt<T>>.TryConvertFromSaturating<TOther>(TOther value, out UInt<T> result) => TryConvertFromSaturating<TOther>(value, out result);
    static bool INumberBase<UInt<T>>.TryConvertToChecked<TOther>(UInt<T> value, out TOther result) where TOther : default => TryConvertToChecked<TOther>(value, out result);
    static bool INumberBase<UInt<T>>.TryConvertToSaturating<TOther>(UInt<T> value, out TOther result) where TOther : default => TryConvertToSaturating<TOther>(value, out result);
    static bool INumberBase<UInt<T>>.TryConvertToTruncating<TOther>(UInt<T> value, out TOther result) where TOther : default => TryConvertToTruncating<TOther>(value, out result);
  }
#endif

}

