
global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;

global using __float16 = System.Numerics.Generic.Float<Test.SizeType16>;
global using __float32 = System.Numerics.Generic.Float<Test.SizeType32>;
global using __float64 = System.Numerics.Generic.Float<Test.SizeType64>;
global using __float80 = System.Numerics.Generic.Float<Test.SizeType80>;
global using __float96 = System.Numerics.Generic.Float<Test.SizeType96>;
global using __float128 = System.Numerics.Generic.Float<Test.SizeType128>;
global using __float256 = System.Numerics.Generic.Float<Test.SizeType256>;
global using __float512 = System.Numerics.Generic.Float<Test.SizeType512>;

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
using System.Runtime.CompilerServices;

namespace Test
{
  [StructLayout(LayoutKind.Sequential, Size = 02)] readonly struct SizeType16 { }
  [StructLayout(LayoutKind.Sequential, Size = 04)] readonly struct SizeType32 { }
  [StructLayout(LayoutKind.Sequential, Size = 08)] readonly struct SizeType64 { }
  [StructLayout(LayoutKind.Sequential, Size = 10)] readonly struct SizeType80 { }
  [StructLayout(LayoutKind.Sequential, Size = 12)] readonly struct SizeType96 { }
  [StructLayout(LayoutKind.Sequential, Size = 16)] readonly struct SizeType128 { }
  [StructLayout(LayoutKind.Sequential, Size = 32)] readonly struct SizeType256 { }
  [StructLayout(LayoutKind.Sequential, Size = 64)] readonly struct SizeType512 { }

  /// <summary>
  /// Float80 as prototype
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float80 :
    IComparable<Float80>, IEquatable<Float80>, IComparable, IFormattable, ISpanFormattable
#if NET7_0
    , IBinaryFloatingPointIeee754<Float80>, IMinMaxValue<Float80>
#endif
  {
    public Float80(int value) => new Float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public Float80(long value) => new Float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public Float80(float value) => new Float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public Float80(double value) => new Float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public Float80(decimal value) => new Float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));
    public Float80(System.Half value) => new Float80(new System.Numerics.Generic.Float<Test.SizeType80>(value));

    public static implicit operator Float80(int value) => new Float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator Float80(long value) => new Float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator Float80(float value) => new Float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator Float80(double value) => new Float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator Float80(decimal value) => new Float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator Float80(System.Numerics.BigRational value) => new Float80((System.Numerics.Generic.Float<Test.SizeType80>)value);
    public static implicit operator Float80(System.Half value) => new Float80((System.Numerics.Generic.Float<Test.SizeType80>)value);

    public static explicit operator int(Float80 value) => (int)value.p;
    public static explicit operator long(Float80 value) => (long)value.p;
    public static explicit operator float(Float80 value) => (float)value.p;
    public static explicit operator double(Float80 value) => (double)value.p;
    public static explicit operator decimal(Float80 value) => (decimal)value.p;
    public static implicit operator System.Numerics.BigRational(Float80 value) => (System.Numerics.BigRational)value.p;
    public static explicit operator System.Half(Float80 value) => (System.Half)value.p;

    public static Float80 operator +(Float80 value) => new Float80(+value.p);
    public static Float80 operator -(Float80 value) => new Float80(-value.p);
    public static Float80 operator ++(Float80 value) => new Float80(value.p + 1);
    public static Float80 operator --(Float80 value) => new Float80(value.p - 1);
    public static Float80 operator +(Float80 left, Float80 right) => new Float80(left.p + right.p);
    public static Float80 operator -(Float80 left, Float80 right) => new Float80(left.p - right.p);
    public static Float80 operator *(Float80 left, Float80 right) => new Float80(left.p * right.p);
    public static Float80 operator /(Float80 left, Float80 right) => new Float80(left.p / right.p);
    public static Float80 operator %(Float80 left, Float80 right) => new Float80(left.p % right.p);

    public static bool operator ==(Float80 left, Float80 right) => left.p == right.p;
    public static bool operator !=(Float80 left, Float80 right) => left.p != right.p;
    public static bool operator >=(Float80 left, Float80 right) => left.p >= right.p;
    public static bool operator <=(Float80 left, Float80 right) => left.p <= right.p;
    public static bool operator >(Float80 left, Float80 right) => left.p > right.p;
    public static bool operator <(Float80 left, Float80 right) => left.p < right.p;

    public static int BitCount => System.Numerics.Generic.Float<Test.SizeType80>.BitCount;
    public static int MaxDigits => System.Numerics.Generic.Float<Test.SizeType80>.MaxDigits;
    public static Float80 MinValue => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.MinValue);
    public static Float80 MaxValue => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.MaxValue);
    public static Float80 Epsilon => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Epsilon);
    public static Float80 NaN => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.NaN);
    public static Float80 NegativeInfinity => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.NegativeInfinity);
    public static Float80 PositiveInfinity => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.PositiveInfinity);
    public static Float80 NegativeZero => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.NegativeZero);
    public static Float80 E => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.E);
    public static Float80 Pi => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Pi);
    public static Float80 Tau => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Tau);

    public static int Sign(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.Sign(value.p);
    public static bool IsPositive(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsPositive(value.p);
    public static bool IsNegative(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsNegative(value.p);
    public static bool IsInteger(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsInteger(value.p);
    public static bool IsFinite(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsFinite(value.p);
    public static bool IsNaN(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsNaN(value.p);
    public static bool IsRealNumber(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsRealNumber(value.p);
    public static bool IsInfinity(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsInfinity(value.p);
    public static bool IsNegativeInfinity(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsNegativeInfinity(value.p);
    public static bool IsPositiveInfinity(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsPositiveInfinity(value.p);
    public static bool IsEvenInteger(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsEvenInteger(value.p);
    public static bool IsOddInteger(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsOddInteger(value.p);
    public static bool IsNormal(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsNormal(value.p);
    public static bool IsSubnormal(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsSubnormal(value.p);
    public static bool IsPow2(Float80 value) => System.Numerics.Generic.Float<Test.SizeType80>.IsPow2(value.p);

    public static Float80 Abs(Float80 value) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Abs(value.p));
    public static Float80 Min(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Min(x.p, y.p));
    public static Float80 Max(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Max(x.p, y.p));
    public static Float80 MinMagnitude(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.MinMagnitude(x.p, y.p));
    public static Float80 MaxMagnitude(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.MaxMagnitude(x.p, y.p));
    public static Float80 MinMagnitudeNumber(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.MinMagnitudeNumber(x.p, y.p));
    public static Float80 MaxMagnitudeNumber(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.MaxMagnitudeNumber(x.p, y.p));
    public static Float80 MinNumber(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.MinNumber(x.p, y.p));
    public static Float80 MaxNumber(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.MaxNumber(x.p, y.p));
    public static Float80 Truncate(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Truncate(x.p));
    public static Float80 Ceiling(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Ceiling(x.p));
    public static Float80 Floor(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Floor(x.p));
    public static Float80 Round(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Round(x.p));
    public static Float80 Round(Float80 x, int digits) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Round(x.p, digits));
    public static Float80 Round(Float80 x, MidpointRounding mode) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Round(x.p, mode));
    public static Float80 Round(Float80 x, int digits, System.MidpointRounding mode) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Round(x.p, digits, mode));
    public static Float80 Pow(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Pow(x.p, y.p));
    public static Float80 Pow10(int y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Pow10(y));
    public static Float80 Sqrt(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Sqrt(x.p));
    public static Float80 Cbrt(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Cbrt(x.p));
    public static Float80 Hypot(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Hypot(x.p, y.p));
    public static Float80 RootN(Float80 x, int n) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.RootN(x.p, n));
    public static Float80 ScaleB(Float80 x, int n) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.ScaleB(x.p, n));
    public static Float80 Ieee754Remainder(Float80 x, Float80 y) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Ieee754Remainder(x.p, y.p));
    public static Float80 Exp(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Exp(x.p));
    public static Float80 Exp2(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Exp2(x.p));
    public static Float80 Exp10(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Exp10(x.p));
    public static Float80 Log(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Log(x.p));
    public static Float80 Log2(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Log2(x.p));
    public static Float80 Log10(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Log10(x.p));
    public static Float80 Log(Float80 x, Float80 newBase) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Log(x.p, newBase.p));
    public static int ILogB(Float80 x) => System.Numerics.Generic.Float<Test.SizeType80>.ILogB(x.p);
    public static Float80 FusedMultiplyAdd(Float80 left, Float80 right, Float80 addend) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.FusedMultiplyAdd(left.p, right.p, addend.p));
    public static Float80 BitDecrement(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.BitDecrement(x.p));
    public static Float80 BitIncrement(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.BitIncrement(x.p));

    public static Float80 Sin(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Sin(x.p));
    public static Float80 Cos(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Cos(x.p));
    public static Float80 Atan(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Atan(x.p));
    public static Float80 Acos(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Acos(x.p));
    public static Float80 AcosPi(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.AcosPi(x.p));
    public static Float80 Asin(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Asin(x.p));
    public static Float80 AsinPi(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.AsinPi(x.p));
    public static Float80 AtanPi(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.AtanPi(x.p));
    public static Float80 CosPi(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.CosPi(x.p));
    public static Float80 SinPi(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.SinPi(x.p));
    public static Float80 Tan(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Tan(x.p));
    public static Float80 TanPi(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.TanPi(x.p));
    public static Float80 Atan2(Float80 y, Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Atan2(y.p, x.p));
    public static Float80 Atan2Pi(Float80 y, Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Atan2Pi(y.p, x.p));
    public static (Float80 Sin, Float80 Cos) SinCos(Float80 x) { var v = System.Numerics.Generic.Float<Test.SizeType80>.SinCos(x.p); return (new Float80(v.Sin), new Float80(v.Cos)); }
    public static (Float80 SinPi, Float80 CosPi) SinCosPi(Float80 x) { var v = System.Numerics.Generic.Float<Test.SizeType80>.SinCosPi(x.p); return (new Float80(v.SinPi), new Float80(v.CosPi)); }

    public static Float80 Acosh(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Acosh(x.p));
    public static Float80 Asinh(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Asinh(x.p));
    public static Float80 Atanh(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Atanh(x.p));
    public static Float80 Cosh(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Cosh(x.p));
    public static Float80 Sinh(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Sinh(x.p));
    public static Float80 Tanh(Float80 x) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Tanh(x.p));

    public override int GetHashCode() => p.GetHashCode();
    public override bool Equals(System.Object? obj) => p.Equals(obj);
    public bool Equals(Float80 other) => p.Equals(other.p);
    public int CompareTo(System.Object? obj) => p.CompareTo(obj);
    public int CompareTo(Float80 other) => p.CompareTo(other.p);

    public override string ToString() => p.ToString();
    public string ToString(string? format) => p.ToString(format);
    public string ToString(IFormatProvider? formatProvider) => p.ToString(formatProvider);
    public string ToString(string? format, System.IFormatProvider? formatProvider = null) => p.ToString(format, formatProvider);
    public bool TryFormat(System.Span<char> dest, out int charsWritten, System.ReadOnlySpan<char> format, System.IFormatProvider? provider) => p.TryFormat(dest, out charsWritten, format, provider);

    public static Float80 Parse(string s) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Parse(s));
    public static Float80 Parse(string s, NumberStyles style) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Parse(s, style));
    public static Float80 Parse(string s, IFormatProvider? provider) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Parse(s, provider));
    public static Float80 Parse(string s, NumberStyles style, IFormatProvider? provider) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Parse(s, style, provider));
    public static Float80 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Parse(s, provider));
    public static Float80 Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.Parse(s, style, provider));

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Float80 result) { var r = System.Numerics.Generic.Float<Test.SizeType80>.TryParse(s, style, provider, out var v); result = new Float80(v); return r; }
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Float80 result) { var r = System.Numerics.Generic.Float<Test.SizeType80>.TryParse(s, style, provider, out var v); result = new Float80(v); return r; }
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Float80 result) { var r = System.Numerics.Generic.Float<Test.SizeType80>.TryParse(s, provider, out var v); result = new Float80(v); return r; }
    public static bool TryParse(string? s, IFormatProvider? provider, out Float80 result) { var r = System.Numerics.Generic.Float<Test.SizeType80>.TryParse(s, provider, out var v); result = new Float80(v); return r; }
    public static bool TryParse(ReadOnlySpan<char> s, out Float80 result) { var r = System.Numerics.Generic.Float<Test.SizeType80>.TryParse(s, out var v); result = new Float80(v); return r; }
    public static bool TryParse([NotNullWhen(true)] string? s, out Float80 result) { var r = System.Numerics.Generic.Float<Test.SizeType80>.TryParse(s, out var v); result = new Float80(v); return r; }

    Float80(System.Numerics.Generic.Float<Test.SizeType80> p) => this.p = p;
    private readonly System.Numerics.Generic.Float<Test.SizeType80> p;

    #region INumber
#if NET6_0
    public static Float80 CreateTruncating<TOther>(TOther value) where TOther : struct => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateTruncating(value));
    public static Float80 CreateSaturating<TOther>(TOther value) where TOther : struct => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateSaturating(value));
    public static Float80 CreateChecked<TOther>(TOther value) where TOther : struct => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateChecked(value));
#endif
#if NET7_0
    public static Float80 CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther> => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateTruncating(value));
    public static Float80 CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther> => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateSaturating(value));
    public static Float80 CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther> => new Float80(System.Numerics.Generic.Float<Test.SizeType80>.CreateChecked(value));
    #region static 
    static int INumberBase<Float80>.Radix => 2;
    static Float80 IBinaryNumber<Float80>.AllBitsSet => new Float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(0));
    static Float80 ISignedNumber<Float80>.NegativeOne => new Float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(1));
    static Float80 INumberBase<Float80>.One => new Float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(2));
    static Float80 INumberBase<Float80>.Zero => new Float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(3));
    static Float80 IAdditiveIdentity<Float80, Float80>.AdditiveIdentity => new Float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(4));
    static Float80 IMultiplicativeIdentity<Float80, Float80>.MultiplicativeIdentity => new Float80(wrap<System.Numerics.Generic.Float<Test.SizeType80>>(5));
    int IFloatingPoint<Float80>.GetExponentByteCount() => iwrap(0, this.p);
    int IFloatingPoint<Float80>.GetExponentShortestBitLength() => iwrap(1, this.p);
    int IFloatingPoint<Float80>.GetSignificandBitLength() => iwrap(2, this.p);
    int IFloatingPoint<Float80>.GetSignificandByteCount() => iwrap(3, this.p);
    static Float80 IBitwiseOperators<Float80, Float80, Float80>.operator &(Float80 left, Float80 right) => new Float80(wrap(0, left.p, right.p));
    static Float80 IBitwiseOperators<Float80, Float80, Float80>.operator |(Float80 left, Float80 right) => new Float80(wrap(1, left.p, right.p));
    static Float80 IBitwiseOperators<Float80, Float80, Float80>.operator ^(Float80 left, Float80 right) => new Float80(wrap(2, right.p));
    static Float80 IBitwiseOperators<Float80, Float80, Float80>.operator ~(Float80 value) => new Float80(wrap(0, value.p));
    static bool INumberBase<Float80>.IsCanonical(Float80 value) => iwrap(4, value.p) != 0;
    static bool INumberBase<Float80>.IsComplexNumber(Float80 value) => iwrap(5, value.p) != 0;
    static bool INumberBase<Float80>.IsEvenInteger(Float80 value) => iwrap(6, value.p) != 0;
    static bool INumberBase<Float80>.IsImaginaryNumber(Float80 value) => iwrap(7, value.p) != 0;
    static bool INumberBase<Float80>.IsNormal(Float80 value) => iwrap(8, value.p) != 0;
    static bool INumberBase<Float80>.IsOddInteger(Float80 value) => iwrap(9, value.p) != 0;
    static bool INumberBase<Float80>.IsSubnormal(Float80 value) => iwrap(10, value.p) != 0;
    static bool INumberBase<Float80>.IsZero(Float80 value) => iwrap(11, value.p) != 0;

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
    bool IFloatingPoint<Float80>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten) => wrap(0, this, destination, out bytesWritten);
    bool IFloatingPoint<Float80>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten) => wrap(1, this, destination, out bytesWritten);
    bool IFloatingPoint<Float80>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten) => wrap(2, this, destination, out bytesWritten);
    bool IFloatingPoint<Float80>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten) => wrap(3, this, destination, out bytesWritten);

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
    static bool INumberBase<Float80>.TryConvertFromChecked<TOther>(TOther value, out Float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a; var b = wrap(0, value, out a); result = new Float80(a); return b;
    }
    static bool INumberBase<Float80>.TryConvertFromSaturating<TOther>(TOther value, out Float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a; var b = wrap(1, value, out a); result = new Float80(a); return b;
    }
    static bool INumberBase<Float80>.TryConvertFromTruncating<TOther>(TOther value, out Float80 result)
    {
      System.Numerics.Generic.Float<Test.SizeType80> a; var b = wrap(2, value, out a); result = new Float80(a); return b;
    }

    static bool wrap<T, TOther>(int i, T a, out TOther? result) where T : IBinaryFloatingPointIeee754<T> where TOther : INumberBase<TOther>
    {
      switch (i)
      {
        case 0: return T.TryConvertToTruncating<TOther>(a, out result);
        case 1: return T.TryConvertToSaturating<TOther>(a, out result);
        case 2: return T.TryConvertToChecked<TOther>(a, out result);
        default: throw new Exception();
      }
    }
    static bool INumberBase<Float80>.TryConvertToTruncating<TOther>(Float80 value, out TOther result) where TOther : default => wrap(0, value.p, out result!);
    static bool INumberBase<Float80>.TryConvertToSaturating<TOther>(Float80 value, out TOther result) => wrap(1, value.p, out result!);
    static bool INumberBase<Float80>.TryConvertToChecked<TOther>(Float80 value, out TOther result) => wrap(2, value.p, out result!);

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
          //case 1: return T.BitDecrement(a);
          //case 2: return T.BitIncrement(a);
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
#endif
    #endregion
  }

}
