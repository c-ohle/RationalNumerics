
global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;

global using Float32 = System.Numerics.Generic.Float<Test.SizeType32>;
global using Float64 = System.Numerics.Generic.Float<Test.SizeType64>;
global using Float80 = System.Numerics.Generic.Float<Test.SizeType80>;
global using Float96 = System.Numerics.Generic.Float<Test.SizeType96>;
global using Float128 = System.Numerics.Generic.Float<Test.SizeType128>;
global using Float256 = System.Numerics.Generic.Float<Test.SizeType256>;
global using Float512 = System.Numerics.Generic.Float<Test.SizeType512>;

global using __int32 = System.Numerics.Generic.Int<Test.SizeType32>;
global using __int64 = System.Numerics.Generic.Int<Test.SizeType64>;
global using __int128 = System.Numerics.Generic.Int<Test.SizeType128>;
global using __int256 = System.Numerics.Generic.Int<Test.SizeType256>;
global using __int512 = System.Numerics.Generic.Int<Test.SizeType512>;

global using __uint32 = System.Numerics.Generic.UInt<Test.SizeType32>;
global using __uint64 = System.Numerics.Generic.UInt<Test.SizeType64>;
global using __uint128 = System.Numerics.Generic.UInt<Test.SizeType128>;
global using __uint256 = System.Numerics.Generic.UInt<Test.SizeType256>;
global using __uint512 = System.Numerics.Generic.UInt<Test.SizeType512>;

global using __decimal64 = System.Numerics.Generic.Decimal<Test.SizeType64>;
global using __decimal256 = System.Numerics.Generic.Decimal<Test.SizeType256>;
global using __decimal512 = System.Numerics.Generic.Decimal<Test.SizeType512>;

using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace Test
{
  [StructLayout(LayoutKind.Sequential, Size = 04)] readonly struct SizeType32 { }
  [StructLayout(LayoutKind.Sequential, Size = 08)] readonly struct SizeType64 { }
  [StructLayout(LayoutKind.Sequential, Size = 10)] readonly struct SizeType80 { }
  [StructLayout(LayoutKind.Sequential, Size = 12)] readonly struct SizeType96 { }
  [StructLayout(LayoutKind.Sequential, Size = 16)] readonly struct SizeType128 { }
  [StructLayout(LayoutKind.Sequential, Size = 32)] readonly struct SizeType256 { }
  [StructLayout(LayoutKind.Sequential, Size = 64)] readonly struct SizeType512 { }

  /// <summary>
  /// __float80 as prototype
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct __float80 :
    IComparable<__float80>, IEquatable<__float80>, IComparable, IFormattable, ISpanFormattable
#if NET7_0
    , IBinaryFloatingPointIeee754<__float80>, IMinMaxValue<__float80>
#endif
  {
    public __float80(int value) => new __float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public __float80(long value) => new __float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public __float80(float value) => new __float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public __float80(double value) => new __float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public __float80(decimal value) => new __float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public __float80(System.Half value) => new __float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));

    public static implicit operator __float80(int value) => new __float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator __float80(long value) => new __float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator __float80(float value) => new __float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator __float80(double value) => new __float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator __float80(decimal value) => new __float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator __float80(System.Numerics.BigRational value) => new __float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator __float80(System.Half value) => new __float80((System.Numerics.Generic.Float<Test.SizeType80>)value);

    public static explicit operator int(__float80 value) => (int)value.p;
    public static explicit operator long(__float80 value) => (long)value.p;
    public static explicit operator float(__float80 value) => (float)value.p;
    public static explicit operator double(__float80 value) => (double)value.p;
    public static explicit operator decimal(__float80 value) => (decimal)value.p;
    public static implicit operator System.Numerics.BigRational(__float80 value) => (System.Numerics.BigRational)value.p;
    public static explicit operator System.Half(__float80 value) => (System.Half)value.p;

    public static __float80 operator +(__float80 value) => new __float80(+value.p);
    public static __float80 operator -(__float80 value) => new __float80(-value.p);
    public static __float80 operator +(__float80 left, __float80 right) => new __float80(left.p + right.p);
    public static __float80 operator -(__float80 left, __float80 right) => new __float80(left.p - right.p);
    public static __float80 operator *(__float80 left, __float80 right) => new __float80(left.p * right.p);
    public static __float80 operator /(__float80 left, __float80 right) => new __float80(left.p / right.p);
    public static __float80 operator %(__float80 left, __float80 right) => new __float80(left.p % right.p);

    public static bool operator ==(__float80 left, __float80 right) => left.p == right.p;
    public static bool operator !=(__float80 left, __float80 right) => left.p != right.p;
    public static bool operator >=(__float80 left, __float80 right) => left.p >= right.p;
    public static bool operator <=(__float80 left, __float80 right) => left.p <= right.p;
    public static bool operator >(__float80 left, __float80 right) => left.p > right.p;
    public static bool operator <(__float80 left, __float80 right) => left.p < right.p;

    public static int Bits => System.Numerics.Generic.Float<Test.SizeType80>.Bits;
    public static int MaxDigits => System.Numerics.Generic.Float<Test.SizeType80>.MaxDigits;
    public static __float80 MinValue => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.MinValue);
    public static __float80 MaxValue => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.MaxValue);
    public static __float80 Epsilon => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Epsilon);
    public static __float80 NaN => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.NaN);
    public static __float80 NegativeInfinity => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.NegativeInfinity);
    public static __float80 PositiveInfinity => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.PositiveInfinity);
    public static __float80 NegativeZero => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.NegativeZero);
    public static __float80 E => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.E);
    public static __float80 Pi => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Pi);
    public static __float80 Tau => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Tau);

    public static int Sign(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.Sign(value.p);
    public static bool IsPositive(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsPositive(value.p);
    public static bool IsNegative(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsNegative(value.p);
    public static bool IsInteger(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsInteger(value.p);
    public static bool IsFinite(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsFinite(value.p);
    public static bool IsNaN(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsNaN(value.p);
    public static bool IsRealNumber(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsRealNumber(value.p);
    public static bool IsInfinity(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsInfinity(value.p);
    public static bool IsNegativeInfinity(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsNegativeInfinity(value.p);
    public static bool IsPositiveInfinity(__float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsPositiveInfinity(value.p);
    public static __float80 Abs(__float80 value) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Abs(value.p));
    public static __float80 Truncate(__float80 a) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Truncate(a.p));
    public static __float80 Round(__float80 x, int digits = 0, System.MidpointRounding mode = System.MidpointRounding.ToEven) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Round(x.p, digits, mode));
    public static __float80 Pow10(int y) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Pow10(y));
    public static __float80 Sqrt(__float80 x) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Sqrt(x.p));
    public static __float80 Sin(__float80 x) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Sin(x.p));
    public static __float80 Cos(__float80 x) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Cos(x.p));
    public static __float80 Atan(__float80 x) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Atan(x.p));
    public static __float80 Exp(__float80 x) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Exp(x.p));
    public static __float80 Exp2(__float80 x) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Exp2(x.p));
    public static __float80 Exp10(__float80 x) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Exp10(x.p));
    public static __float80 Log(__float80 x) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Log(x.p));
    public static __float80 Log2(__float80 x) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Log2(x.p));
    public static __float80 Pow(__float80 x, __float80 y) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Pow(x.p, y.p));
    public static __float80 Parse(string s) => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.Parse(s));

    public override string ToString() => p.ToString();
    public string ToString(string? format, System.IFormatProvider? formatProvider = null) => p.ToString(format, formatProvider);
    public bool TryFormat(System.Span<char> dest, out int charsWritten, System.ReadOnlySpan<char> format, System.IFormatProvider? provider) => p.TryFormat(dest, out charsWritten, format, provider);
    public override int GetHashCode() => p.GetHashCode();
    public override bool Equals(System.Object? obj) => p.Equals(obj);
    public bool Equals(__float80 other) => p.Equals(other.p);
    public int CompareTo(System.Object? obj) => p.CompareTo(obj);
    public int CompareTo(__float80 other) => p.CompareTo(other.p);

    __float80(System.Numerics.Generic.Float<Test.SizeType80> p) => this.p = p;
    private readonly System.Numerics.Generic.Float<Test.SizeType80> p;

#if NET6_0
    public static __float80 CreateTruncating<TOther>(TOther value) where TOther : struct => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateTruncating(value));
    public static __float80 CreateSaturating<TOther>(TOther value) where TOther : struct => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateSaturating(value));
    public static __float80 CreateChecked<TOther>(TOther value) where TOther : struct => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateChecked(value));
#endif
    #region INumber
#if NET7_0
    public static __float80 CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther> => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateTruncating(value));
    public static __float80 CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther> => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateSaturating(value));
    public static __float80 CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther> => new __float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateChecked(value));
    #region keeps static 
    static int INumberBase<__float80>.Radix => iwrap<System.Numerics.Generic.Float<Test.SizeType80>>(0);
    static __float80 IBinaryNumber<__float80>.AllBitsSet => new __float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(0));
    static __float80 ISignedNumber<__float80>.NegativeOne => new __float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(1));
    static __float80 INumberBase<__float80>.One => new __float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(2));
    static __float80 INumberBase<__float80>.Zero => new __float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(3));
    static __float80 IAdditiveIdentity<__float80, __float80>.AdditiveIdentity => new __float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(4));
    static __float80 IMultiplicativeIdentity<__float80, __float80>.MultiplicativeIdentity => new __float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(5));
    int IFloatingPoint<__float80>.GetExponentByteCount() => iwrap(0, this.p);
    int IFloatingPoint<__float80>.GetExponentShortestBitLength() => iwrap(1, this.p);
    int IFloatingPoint<__float80>.GetSignificandBitLength() => iwrap(2, this.p);
    int IFloatingPoint<__float80>.GetSignificandByteCount() => iwrap(3, this.p);
    static __float80 IBitwiseOperators<__float80, __float80, __float80>.operator &(__float80 left, __float80 right) => new __float80(wrap(0, left.p, right.p));
    static __float80 IBitwiseOperators<__float80, __float80, __float80>.operator |(__float80 left, __float80 right) => new __float80(wrap(1, left.p, right.p));
    static __float80 IBitwiseOperators<__float80, __float80, __float80>.operator ^(__float80 left, __float80 right) => new __float80(wrap(2, right.p));
    static __float80 IBitwiseOperators<__float80, __float80, __float80>.operator ~(__float80 value) => new __float80(wrap(0, value.p));
    static __float80 IFloatingPointIeee754<__float80>.BitDecrement(__float80 x) => new __float80(wrap(1, x.p));
    static __float80 IFloatingPointIeee754<__float80>.BitIncrement(__float80 x) => new __float80(wrap(2, x.p));
    static bool INumberBase<__float80>.IsCanonical(__float80 value) => iwrap(4, value.p) != 0;
    static bool INumberBase<__float80>.IsComplexNumber(__float80 value) => iwrap(5, value.p) != 0;
    static bool INumberBase<__float80>.IsEvenInteger(__float80 value) => iwrap(6, value.p) != 0;
    static bool INumberBase<__float80>.IsImaginaryNumber(__float80 value) => iwrap(7, value.p) != 0;
    static bool INumberBase<__float80>.IsNormal(__float80 value) => iwrap(8, value.p) != 0;
    static bool INumberBase<__float80>.IsOddInteger(__float80 value) => iwrap(9, value.p) != 0;
    static bool INumberBase<__float80>.IsSubnormal(__float80 value) => iwrap(10, value.p) != 0;
    static bool INumberBase<__float80>.IsZero(__float80 value) => iwrap(11, value.p) != 0;

    static bool wrap<T>(int i, T a, Span<byte> destination, out int bytesWritten) where T : IBinaryFloatingPointIeee754<T>
    {
      switch (i)
      {
        case 0: return a.TryWriteExponentBigEndian(destination, out bytesWritten);
        case 1: return a.TryWriteExponentLittleEndian(destination, out bytesWritten);
        case 2: return a.TryWriteSignificandBigEndian(destination, out bytesWritten);
        case 3: return a.TryWriteSignificandLittleEndian(destination, out bytesWritten);
        default: throw new Exception();
      }
    }
    bool IFloatingPoint<__float80>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten) => wrap(0, this, destination, out bytesWritten);
    bool IFloatingPoint<__float80>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten) => wrap(1, this, destination, out bytesWritten);
    bool IFloatingPoint<__float80>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten) => wrap(2, this, destination, out bytesWritten);
    bool IFloatingPoint<__float80>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten) => wrap(3, this, destination, out bytesWritten);

    static T wrap<T>(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) where T : IBinaryFloatingPointIeee754<T>
    {
      return T.Parse(s, style, provider);
    }
    static __float80 INumberBase<__float80>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => new __float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(s, style, provider));
    static __float80 INumberBase<__float80>.Parse(string s, NumberStyles style, IFormatProvider? provider) => new __float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(s.AsSpan(), style, provider));

    static bool wrap<T, TOther>(int i, TOther a, out T? result) where T : IBinaryFloatingPointIeee754<T> where TOther : INumberBase<TOther>
    {
      switch (i)
      {
        case 0: return T.TryConvertFromChecked<TOther>(a, out result);
        case 1: return T.TryConvertFromSaturating<TOther>(a, out result);
        case 2: return T.TryConvertFromTruncating<TOther>(a, out result);
        default: throw new Exception();
      }
    }
    static bool INumberBase<__float80>.TryConvertFromChecked<TOther>(TOther value, out __float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a; var b = wrap(0, value, out a); result = new __float80(a); return b;
    }
    static bool INumberBase<__float80>.TryConvertFromSaturating<TOther>(TOther value, out __float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a; var b = wrap(1, value, out a); result = new __float80(a); return b;
    }
    static bool INumberBase<__float80>.TryConvertFromTruncating<TOther>(TOther value, out __float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a; var b = wrap(2, value, out a); result = new __float80(a); return b;
    }

    static bool wrap<T, TOther>(int i, T a, out TOther? result) where T : IBinaryFloatingPointIeee754<T> where TOther : INumberBase<TOther>
    {
      switch (i)
      {
        case 0: return T.TryConvertToChecked<TOther>(a, out result);
        case 1: return T.TryConvertToSaturating<TOther>(a, out result);
        case 2: return T.TryConvertToTruncating<TOther>(a, out result);
        default: throw new Exception();
      }
    }
    static bool INumberBase<__float80>.TryConvertToChecked<TOther>(__float80 value, out TOther result) => wrap(0, value.p, out result!);
    static bool INumberBase<__float80>.TryConvertToSaturating<TOther>(__float80 value, out TOther result) => wrap(1, value.p, out result!);
    static bool INumberBase<__float80>.TryConvertToTruncating<TOther>(__float80 value, out TOther result) where TOther : default => wrap(2, value.p, out result!);

    static bool wrap<T>(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out T a) where T : IBinaryFloatingPointIeee754<T>
    {
      return T.TryParse(s, style, provider, out a);
    }
    static bool INumberBase<__float80>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out __float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a;
      var b = wrap(s, style, provider, out a); result = new __float80(a); return b;
    }
    static bool INumberBase<__float80>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out __float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a;
      var b = wrap(s.AsSpan(), style, provider, out a); result = new __float80(a); return b;
    }
    static __float80 ISpanParsable<__float80>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a;
      wrap(s, NumberStyles.None, provider, out a); return new __float80(a);
    }
    static bool ISpanParsable<__float80>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out __float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a;
      var b = wrap(s, NumberStyles.None, provider, out a); result = new __float80(a); return b;
    }
    static __float80 IParsable<__float80>.Parse(string s, IFormatProvider? provider)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a;
      wrap(s.AsSpan(), NumberStyles.None, provider, out a); return new __float80(a);
    }
    static bool IParsable<__float80>.TryParse(string? s, IFormatProvider? provider, out __float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a;
      var b = wrap(s.AsSpan(), NumberStyles.None, provider, out a); result = new __float80(a); return b;
    }

    static int iwrap<T>(int i) where T : IBinaryFloatingPointIeee754<T>
    {
      switch (i) { case 0: return T.Radix; }
      return 0;
    }
    static int iwrap<T>(int i, T a) where T : IBinaryFloatingPointIeee754<T>
    {
      switch (i)
      {
        case 0: return a.GetExponentByteCount();
        case 1: return a.GetExponentShortestBitLength();
        case 2: return a.GetSignificandBitLength();
        case 3: return a.GetSignificandByteCount();
        case 4: return T.IsCanonical(a) ? 1 : 0;
        case 5: return T.IsComplexNumber(a) ? 1 : 0;
        case 6: return T.IsEvenInteger(a) ? 1 : 0;
        case 7: return T.IsImaginaryNumber(a) ? 1 : 0;
        case 8: return T.IsNormal(a) ? 1 : 0;
        case 9: return T.IsOddInteger(a) ? 1 : 0;
        case 10: return T.IsSubnormal(a) ? 1 : 0;
        case 11: return T.IsZero(a) ? 1 : 0;
      }
      return 0;
    }
    static T wrap<T>(int i) where T : IBinaryFloatingPointIeee754<T>
    {
      switch (i)
      {
        case 0: return T.AllBitsSet;
        case 1: return T.NegativeOne;
        case 2: return T.One;
        case 3: return T.Zero;
        case 4: return T.AdditiveIdentity;
        case 5: return T.MultiplicativeIdentity;
      }
      return T.Zero;
    }
    static T wrap<T>(int i, T a) where T : IBinaryFloatingPointIeee754<T>
    {
      switch (i)
      {
        case 0: return ~a;
        case 1: return T.BitDecrement(a);
        case 2: return T.BitIncrement(a);
      }
      return T.Zero;
    }
    static T wrap<T>(int i, T a, T b) where T : IBinaryFloatingPointIeee754<T>
    {
      switch (i)
      {
        case 0: return a & b;
        case 1: return a | b;
        case 2: return a ^ b;
      }
      return T.Zero;
    }
    #endregion
    #region gets public
    static __float80 IDecrementOperators<__float80>.operator --(__float80 value) => throw new NotImplementedException();
    static __float80 IIncrementOperators<__float80>.operator ++(__float80 value) => throw new NotImplementedException();
    static bool IBinaryNumber<__float80>.IsPow2(__float80 value) => throw new NotImplementedException();
    static __float80 IFloatingPointIeee754<__float80>.Atan2(__float80 y, __float80 x) => throw new NotImplementedException();
    static __float80 IFloatingPointIeee754<__float80>.Atan2Pi(__float80 y, __float80 x) => throw new NotImplementedException();
    static __float80 IFloatingPointIeee754<__float80>.FusedMultiplyAdd(__float80 left, __float80 right, __float80 addend) => throw new NotImplementedException();
    static __float80 IFloatingPointIeee754<__float80>.Ieee754Remainder(__float80 left, __float80 right) => throw new NotImplementedException();
    static int IFloatingPointIeee754<__float80>.ILogB(__float80 x) => throw new NotImplementedException();
    static __float80 IFloatingPointIeee754<__float80>.ScaleB(__float80 x, int n) => throw new NotImplementedException();
    static __float80 IHyperbolicFunctions<__float80>.Acosh(__float80 x) => throw new NotImplementedException();
    static __float80 IHyperbolicFunctions<__float80>.Asinh(__float80 x) => throw new NotImplementedException();
    static __float80 IHyperbolicFunctions<__float80>.Atanh(__float80 x) => throw new NotImplementedException();
    static __float80 IHyperbolicFunctions<__float80>.Cosh(__float80 x) => throw new NotImplementedException();
    static __float80 IHyperbolicFunctions<__float80>.Sinh(__float80 x) => throw new NotImplementedException();
    static __float80 IHyperbolicFunctions<__float80>.Tanh(__float80 x) => throw new NotImplementedException();
    static __float80 ILogarithmicFunctions<__float80>.Log(__float80 x, __float80 newBase) => throw new NotImplementedException();
    static __float80 ILogarithmicFunctions<__float80>.Log10(__float80 x) => throw new NotImplementedException();
    static __float80 IRootFunctions<__float80>.Cbrt(__float80 x) => throw new NotImplementedException();
    static __float80 IRootFunctions<__float80>.Hypot(__float80 x, __float80 y) => throw new NotImplementedException();
    static __float80 IRootFunctions<__float80>.RootN(__float80 x, int n) => throw new NotImplementedException();
    static __float80 ITrigonometricFunctions<__float80>.Acos(__float80 x) => throw new NotImplementedException();
    static __float80 ITrigonometricFunctions<__float80>.AcosPi(__float80 x) => throw new NotImplementedException();
    static __float80 ITrigonometricFunctions<__float80>.Asin(__float80 x) => throw new NotImplementedException();
    static __float80 ITrigonometricFunctions<__float80>.AsinPi(__float80 x) => throw new NotImplementedException();
    static __float80 ITrigonometricFunctions<__float80>.AtanPi(__float80 x) => throw new NotImplementedException();
    static __float80 ITrigonometricFunctions<__float80>.CosPi(__float80 x) => throw new NotImplementedException();
    static (__float80 Sin, __float80 Cos) ITrigonometricFunctions<__float80>.SinCos(__float80 x) => throw new NotImplementedException();
    static (__float80 SinPi, __float80 CosPi) ITrigonometricFunctions<__float80>.SinCosPi(__float80 x) => throw new NotImplementedException();
    static __float80 ITrigonometricFunctions<__float80>.SinPi(__float80 x) => throw new NotImplementedException();
    static __float80 ITrigonometricFunctions<__float80>.Tan(__float80 x) => throw new NotImplementedException();
    static __float80 ITrigonometricFunctions<__float80>.TanPi(__float80 x) => throw new NotImplementedException();
    static __float80 INumberBase<__float80>.MaxMagnitude(__float80 x, __float80 y) => throw new NotImplementedException();
    static __float80 INumberBase<__float80>.MaxMagnitudeNumber(__float80 x, __float80 y) => throw new NotImplementedException();
    static __float80 INumberBase<__float80>.MinMagnitude(__float80 x, __float80 y) => throw new NotImplementedException();
    static __float80 INumberBase<__float80>.MinMagnitudeNumber(__float80 x, __float80 y) => throw new NotImplementedException();
    #endregion
#endif
    #endregion
  }

}
