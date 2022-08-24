
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics.Generic;
using System.Runtime.InteropServices;

namespace System.Numerics.Rational
{
  /// <summary>
  /// It's just a double - as test use Float template from external
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float64 : IFloat<Float64>, IComparable<Float64>, IEquatable<Float64>, IComparable, ISpanFormattable
  {    
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public readonly bool Equals(Float64 b) => Float<UInt64>.Equals(this.p, b.p);
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly int CompareTo(Float64 b) => Float<UInt64>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float64 b ? Float<UInt64>.Compare(this.p, b.p) : throw new ArgumentException();
    public readonly override int GetHashCode() => p.GetHashCode();
    public static Float64 Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null) => new Float64(Float<UInt64>.Parse(s, provider));
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Float64 result) { var t = Float<UInt64>.TryParse(s, provider, out var r); result = new Float64(r); return t; }

    public static implicit operator int(Float64 value) => (int)value.p;
    public static implicit operator Half(Float64 value) => (Half)value.p;
    public static implicit operator float(Float64 value) => (float)value.p;
    public static implicit operator double(Float64 value) => (double)value.p;
    public static implicit operator decimal(Float64 value) => (decimal)value.p;
    public static implicit operator Float128(Float64 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float64 value) => Float256.Cast(value.p);
    public static implicit operator BigRational(Float64 value) => (BigRational)value.p;

    public static implicit operator Float64(int value) => new Float64((Float<UInt64>)value);
    public static implicit operator Float64(Half value) => new Float64((Float<UInt64>)value);
    public static implicit operator Float64(float value) => new Float64((Float<UInt64>)value);
    public static implicit operator Float64(double value) => new Float64((Float<UInt64>)value);
    public static implicit operator Float64(decimal value) => new Float64((Float<UInt64>)value);
    public static explicit operator Float64(Float128 value) => new Float64(Float128.Cast<UInt64>(value));
    public static explicit operator Float64(Float256 value) => new Float64(Float256.Cast<UInt64>(value));
    public static explicit operator Float64(BigRational value) => new Float64((Float<UInt64>)value);

    public static Float64 operator ++(Float64 a) => new Float64(a.p + +1);
    public static Float64 operator --(Float64 a) => new Float64(a.p + -1);
    public static Float64 operator +(Float64 a, Float64 b) => new Float64(a.p + b.p);
    public static Float64 operator -(Float64 a, Float64 b) => new Float64(a.p - b.p);
    public static Float64 operator *(Float64 a, Float64 b) => new Float64(a.p * b.p);
    public static Float64 operator /(Float64 a, Float64 b) => new Float64(a.p / b.p);
    public static Float64 operator %(Float64 a, Float64 b) => new Float64(a.p % b.p);

    public static bool operator ==(Float64 a, Float64 b) => a.p == b.p;
    public static bool operator !=(Float64 a, Float64 b) => a.p != b.p;
    public static bool operator <=(Float64 a, Float64 b) => a.p <= b.p;
    public static bool operator >=(Float64 a, Float64 b) => a.p >= b.p;
    public static bool operator <(Float64 a, Float64 b) => a.p < b.p;
    public static bool operator >(Float64 a, Float64 b) => a.p > b.p;

    public static Float64 Pi => new Float64(Float<UInt64>.Pi);
    public static Float64 Tau => new Float64(Float<UInt64>.Tau);
    public static Float64 E => new Float64(Float<UInt64>.E);
    public static Float64 MinValue => new Float64(Float<UInt64>.MinValue);
    public static Float64 MaxValue => new Float64(Float<UInt64>.MaxValue);
    public static Float64 Epsilon => new Float64(Float<UInt64>.Epsilon);
    public static Float64 NaN => new Float64(Float<UInt64>.NaN);
    public static Float64 NegativeInfinity => new Float64(Float<UInt64>.NegativeInfinity);
    public static Float64 PositiveInfinity => new Float64(Float<UInt64>.PositiveInfinity);
    public static Float64 NegativeZero => new Float64(Float<UInt64>.NegativeZero);

    public static Float64 Abs(Float64 a) => new Float64(Float<UInt64>.Abs(a.p));
    public static Float64 Acos(Float64 a) => new Float64(Float<UInt64>.Acos(a.p));
    public static Float64 Acosh(Float64 a) => new Float64(Float<UInt64>.Acosh(a.p));
    public static Float64 AcosPi(Float64 a) => new Float64(Float<UInt64>.AcosPi(a.p));
    public static Float64 Asin(Float64 a) => new Float64(Float<UInt64>.Asin(a.p));
    public static Float64 Asinh(Float64 a) => new Float64(Float<UInt64>.Asinh(a.p));
    public static Float64 AsinPi(Float64 a) => new Float64(Float<UInt64>.AsinPi(a.p));
    public static Float64 Atan(Float64 a) => new Float64(Float<UInt64>.Atan(a.p));
    public static Float64 Atanh(Float64 a) => new Float64(Float<UInt64>.Atanh(a.p));
    public static Float64 AtanPi(Float64 a) => new Float64(Float<UInt64>.AtanPi(a.p));
    public static Float64 Atan2Pi(Float64 a, Float64 b) => new Float64(Float<UInt64>.Atan2Pi(a.p, b.p));
    public static Float64 BitDecrement(Float64 a) => new Float64(Float<UInt64>.BitDecrement(a.p));
    public static Float64 BitIncrement(Float64 a) => new Float64(Float<UInt64>.BitIncrement(a.p));
    public static Float64 Cbrt(Float64 a) => new Float64(Float<UInt64>.Cbrt(a.p));
    public static Float64 Ceiling(Float64 a) => new Float64(Float<UInt64>.Ceiling(a.p));
    public static Float64 Clamp(Float64 a, Float64 min, Float64 max) => new Float64(Float<UInt64>.Clamp(a.p, min.p, max.p));
    public static Float64 CopySign(Float64 a, Float64 sign) => new Float64(Float<UInt64>.CopySign(a.p, sign.p));
    public static Float64 Cos(Float64 a) => new Float64(Float<UInt64>.Cos(a.p));
    public static Float64 Cosh(Float64 a) => new Float64(Float<UInt64>.Cosh(a.p));
    public static Float64 CosPi(Float64 a) => new Float64(Float<UInt64>.CosPi(a.p));
    public static Float64 Exp(Float64 a) => new Float64(Float<UInt64>.Exp(a.p));
    public static Float64 Exp10(Float64 a) => new Float64(Float<UInt64>.Exp10(a.p));
    public static Float64 Exp10M1(Float64 a) => new Float64(Float<UInt64>.Exp10M1(a.p));
    public static Float64 Exp2(Float64 a) => new Float64(Float<UInt64>.Exp2(a.p));
    public static Float64 Exp2M1(Float64 a) => new Float64(Float<UInt64>.Exp2M1(a.p));
    public static Float64 ExpM1(Float64 a) => new Float64(Float<UInt64>.ExpM1(a.p));
    public static Float64 Floor(Float64 a) => new Float64(Float<UInt64>.Floor(a.p));
    public static Float64 FusedMultiplyAdd(Float64 a, Float64 b, Float64 c) => new Float64(Float<UInt64>.FusedMultiplyAdd(a.p, b.p, c.p));
    public static Float64 Hypot(Float64 a, Float64 b) => new Float64(Float<UInt64>.Hypot(a.p, b.p));
    public static Float64 Ieee754Remainder(Float64 a, Float64 b) => new Float64(Float<UInt64>.Ieee754Remainder(a.p, b.p));
    public static int ILogB(Float64 a) => Float<UInt64>.ILogB(a.p);
    public static bool IsEvenInteger(Float64 a) => Float<UInt64>.IsEvenInteger(a.p);
    public static bool IsFinite(Float64 a) => Float<UInt64>.IsFinite(a.p);
    public static bool IsInfinity(Float64 a) => Float<UInt64>.IsInfinity(a.p);
    public static bool IsInteger(Float64 a) => Float<UInt64>.IsInteger(a.p);
    public static bool IsNaN(Float64 a) => Float<UInt64>.IsNaN(a.p);
    public static bool IsNegative(Float64 a) => Float<UInt64>.IsNegative(a.p);
    public static bool IsNegativeInfinity(Float64 a) => Float<UInt64>.IsNegativeInfinity(a.p);
    public static bool IsNormal(Float64 a) => Float<UInt64>.IsNormal(a.p);
    public static bool IsOddInteger(Float64 a) => Float<UInt64>.IsOddInteger(a.p);
    public static bool IsPositiveInfinity(Float64 a) => Float<UInt64>.IsPositiveInfinity(a.p);
    public static bool IsPow2(Float64 a) => Float<UInt64>.IsPow2(a.p);
    public static bool IsRealNumber(Float64 a) => Float<UInt64>.IsRealNumber(a.p);
    public static bool IsSubnormal(Float64 a) => Float<UInt64>.IsSubnormal(a.p);
    public static Float64 Log(Float64 a) => new Float64(Float<UInt64>.Log(a.p));
    public static Float64 Log10(Float64 a) => new Float64(Float<UInt64>.Log10(a.p));
    public static Float64 LogP1(Float64 a) => new Float64(Float<UInt64>.LogP1(a.p));
    public static Float64 Log2(Float64 a) => new Float64(Float<UInt64>.Log2(a.p));
    public static Float64 Log2P1(Float64 a) => new Float64(Float<UInt64>.Log2P1(a.p));
    public static Float64 Max(Float64 a, Float64 b) => new Float64(Float<UInt64>.Max(a.p, b.p));
    public static Float64 MaxMagnitude(Float64 a, Float64 b) => new Float64(Float<UInt64>.MaxMagnitude(a.p, b.p));
    public static Float64 MaxMagnitudeNumber(Float64 a, Float64 b) => new Float64(Float<UInt64>.MaxMagnitudeNumber(a.p, b.p));
    public static Float64 MaxNumber(Float64 a, Float64 b) => new Float64(Float<UInt64>.MaxNumber(a.p, b.p));
    public static Float64 Min(Float64 a, Float64 b) => new Float64(Float<UInt64>.Min(a.p, b.p));
    public static Float64 MinMagnitude(Float64 a, Float64 b) => new Float64(Float<UInt64>.MinMagnitude(a.p, b.p));
    public static Float64 MinMagnitudeNumber(Float64 a, Float64 b) => new Float64(Float<UInt64>.MinMagnitudeNumber(a.p, b.p));
    public static Float64 MinNumber(Float64 a, Float64 b) => new Float64(Float<UInt64>.MinNumber(a.p, b.p));
    public static Float64 Pow(Float64 a, Float64 b) => new Float64(Float<UInt64>.Pow(a.p, b.p));
    public static Float64 ReciprocalEstimate(Float64 a) => new Float64(Float<UInt64>.ReciprocalEstimate(a.p));
    public static Float64 ReciprocalSqrtEstimate(Float64 a) => new Float64(Float<UInt64>.ReciprocalSqrtEstimate(a.p));
    public static Float64 RootN(Float64 a, int n) => new Float64(Float<UInt64>.RootN(a.p, n));
    public static Float64 Round(Float64 a) => new Float64(Float<UInt64>.Round(a.p));
    public static Float64 Round(Float64 a, int digits) => new Float64(Float<UInt64>.Round(a.p, digits));
    public static Float64 Round(Float64 a, int digits, MidpointRounding mode) => new Float64(Float<UInt64>.Round(a.p, digits, mode));
    public static Float64 ScaleB(Float64 a, int n) => new Float64(Float<UInt64>.ScaleB(a.p, n));
    public static int Sign(Float64 a) => Float<UInt64>.Sign(a.p);
    public static (Float64 Sin, Float64 Cos) SinCos(Float64 a) { var t = Float<UInt64>.SinCos(a.p); return (new Float64(t.Sin), new Float64(t.Cos)); }
    public static (Float64 SinPi, Float64 CosPi) SinCosPi(Float64 a) { var t = Float<UInt64>.SinCosPi(a.p); return (new Float64(t.SinPi), new Float64(t.CosPi)); }
    public static Float64 Sinh(Float64 a) => new Float64(Float<UInt64>.Sinh(a.p));
    public static Float64 SinPi(Float64 a) => new Float64(Float<UInt64>.SinPi(a.p));
    public static Float64 Sqrt(Float64 a) => new Float64(Float<UInt64>.Sqrt(a.p));
    public static Float64 Tan(Float64 a) => new Float64(Float<UInt64>.Tan(a.p));
    public static Float64 Tanh(Float64 a) => new Float64(Float<UInt64>.Tanh(a.p));
    public static Float64 TanPi(Float64 a) => new Float64(Float<UInt64>.TanPi(a.p));
    public static Float64 Truncate(Float64 a) => new Float64(Float<UInt64>.Truncate(a.p));

    public static Float64 Cast<T>(Float<T> a) where T : unmanaged => new Float64(Float<T>.Cast<UInt64>(a));
    public static Float<T> Cast<T>(Float64 a) where T : unmanaged => Float<UInt64>.Cast<T>(a.p);

    private Float64(Float<UInt64> p) => this.p = p;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Float<UInt64> p;
  }

  /// <summary>
  /// It's just for test for external new floating types from Float
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float96 : IFloat<Float96>, IComparable<Float96>, IEquatable<Float96>, IComparable, ISpanFormattable
  {
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float96 b) => Float<UInt96>.Equals(this.p, b.p);
    public readonly int CompareTo(Float96 b) => Float<UInt96>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float96 b ? Float<UInt96>.Compare(this.p, b.p) : throw new ArgumentException();

    public static implicit operator int(Float96 value) => (int)value.p;
    public static implicit operator Half(Float96 value) => (Half)value.p;
    public static implicit operator float(Float96 value) => (float)value.p;
    public static implicit operator double(Float96 value) => (double)value.p;
    public static implicit operator decimal(Float96 value) => (decimal)value.p;
    public static implicit operator Float64(Float96 value) => Float64.Cast(value.p);
    public static implicit operator Float128(Float96 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float96 value) => Float256.Cast(value.p);

    public static implicit operator Float96(int value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(Half value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(float value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(double value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(decimal value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(Float64 value) => new Float96(Float64.Cast<UInt96>(value));
    public static explicit operator Float96(Float128 value) => new Float96(Float128.Cast<UInt96>(value));
    public static explicit operator Float96(Float256 value) => new Float96(Float256.Cast<UInt96>(value));
    public static explicit operator Float96(BigRational value) => new Float96((Float<UInt96>)value);
     
    public static Float96 operator ++(Float96 a) => new Float96(a.p + +1);
    public static Float96 operator --(Float96 a) => new Float96(a.p + -1);
    public static Float96 operator +(Float96 a, Float96 b) => new Float96(a.p + b.p);
    public static Float96 operator -(Float96 a, Float96 b) => new Float96(a.p - b.p);
    public static Float96 operator *(Float96 a, Float96 b) => new Float96(a.p * b.p);
    public static Float96 operator /(Float96 a, Float96 b) => new Float96(a.p / b.p);
    public static Float96 operator %(Float96 a, Float96 b) => new Float96(a.p % b.p);

    public static bool operator ==(Float96 a, Float96 b) => a.p == b.p;
    public static bool operator !=(Float96 a, Float96 b) => a.p != b.p;
    public static bool operator <=(Float96 a, Float96 b) => a.p <= b.p;
    public static bool operator >=(Float96 a, Float96 b) => a.p >= b.p;
    public static bool operator <(Float96 a, Float96 b) => a.p < b.p;
    public static bool operator >(Float96 a, Float96 b) => a.p > b.p;

    public static Float96 Pi => new Float96(Float<UInt96>.Pi);
    public static Float96 Tau => new Float96(Float<UInt96>.Tau);
    public static Float96 E => new Float96(Float<UInt96>.E);
    public static Float96 MinValue => new Float96(Float<UInt96>.MinValue);
    public static Float96 MaxValue => new Float96(Float<UInt96>.MaxValue);
    public static Float96 Epsilon => new Float96(Float<UInt96>.Epsilon);
    public static Float96 NaN => new Float96(Float<UInt96>.NaN);
    public static Float96 NegativeInfinity => new Float96(Float<UInt96>.NegativeInfinity);
    public static Float96 PositiveInfinity => new Float96(Float<UInt96>.PositiveInfinity);
    public static Float96 NegativeZero => new Float96(Float<UInt96>.NegativeZero);

    public static Float96 Abs(Float96 a) => new Float96(Float<UInt96>.Abs(a.p));
    public static Float96 Acos(Float96 a) => new Float96(Float<UInt96>.Acos(a.p));
    public static Float96 Acosh(Float96 a) => new Float96(Float<UInt96>.Acosh(a.p));
    public static Float96 AcosPi(Float96 a) => new Float96(Float<UInt96>.AcosPi(a.p));
    public static Float96 Asin(Float96 a) => new Float96(Float<UInt96>.Asin(a.p));
    public static Float96 Asinh(Float96 a) => new Float96(Float<UInt96>.Asinh(a.p));
    public static Float96 AsinPi(Float96 a) => new Float96(Float<UInt96>.AsinPi(a.p));
    public static Float96 Atan(Float96 a) => new Float96(Float<UInt96>.Atan(a.p));
    public static Float96 Atanh(Float96 a) => new Float96(Float<UInt96>.Atanh(a.p));
    public static Float96 AtanPi(Float96 a) => new Float96(Float<UInt96>.AtanPi(a.p));
    public static Float96 Atan2Pi(Float96 a, Float96 b) => new Float96(Float<UInt96>.Atan2Pi(a.p, b.p));
    public static Float96 BitDecrement(Float96 a) => new Float96(Float<UInt96>.BitDecrement(a.p));
    public static Float96 BitIncrement(Float96 a) => new Float96(Float<UInt96>.BitIncrement(a.p));
    public static Float96 Cbrt(Float96 a) => new Float96(Float<UInt96>.Cbrt(a.p));
    public static Float96 Ceiling(Float96 a) => new Float96(Float<UInt96>.Ceiling(a.p));
    public static Float96 Clamp(Float96 a, Float96 min, Float96 max) => new Float96(Float<UInt96>.Clamp(a.p, min.p, max.p));
    public static Float96 CopySign(Float96 a, Float96 sign) => new Float96(Float<UInt96>.CopySign(a.p, sign.p));
    public static Float96 Cos(Float96 a) => new Float96(Float<UInt96>.Cos(a.p));
    public static Float96 Cosh(Float96 a) => new Float96(Float<UInt96>.Cosh(a.p));
    public static Float96 CosPi(Float96 a) => new Float96(Float<UInt96>.CosPi(a.p));
    public static Float96 Exp(Float96 a) => new Float96(Float<UInt96>.Exp(a.p));
    public static Float96 Exp10(Float96 a) => new Float96(Float<UInt96>.Exp10(a.p));
    public static Float96 Exp10M1(Float96 a) => new Float96(Float<UInt96>.Exp10M1(a.p));
    public static Float96 Exp2(Float96 a) => new Float96(Float<UInt96>.Exp2(a.p));
    public static Float96 Exp2M1(Float96 a) => new Float96(Float<UInt96>.Exp2M1(a.p));
    public static Float96 ExpM1(Float96 a) => new Float96(Float<UInt96>.ExpM1(a.p));
    public static Float96 Floor(Float96 a) => new Float96(Float<UInt96>.Floor(a.p));
    public static Float96 FusedMultiplyAdd(Float96 a, Float96 b, Float96 c) => new Float96(Float<UInt96>.FusedMultiplyAdd(a.p, b.p, c.p));
    public static Float96 Hypot(Float96 a, Float96 b) => new Float96(Float<UInt96>.Hypot(a.p, b.p));
    public static Float96 Ieee754Remainder(Float96 a, Float96 b) => new Float96(Float<UInt96>.Ieee754Remainder(a.p, b.p));
    public static int ILogB(Float96 a) => Float<UInt96>.ILogB(a.p);
    public static bool IsEvenInteger(Float96 a) => Float<UInt96>.IsEvenInteger(a.p);
    public static bool IsFinite(Float96 a) => Float<UInt96>.IsFinite(a.p);
    public static bool IsInfinity(Float96 a) => Float<UInt96>.IsInfinity(a.p);
    public static bool IsInteger(Float96 a) => Float<UInt96>.IsInteger(a.p);
    public static bool IsNaN(Float96 a) => Float<UInt96>.IsNaN(a.p);
    public static bool IsNegative(Float96 a) => Float<UInt96>.IsNegative(a.p);
    public static bool IsNegativeInfinity(Float96 a) => Float<UInt96>.IsNegativeInfinity(a.p);
    public static bool IsNormal(Float96 a) => Float<UInt96>.IsNormal(a.p);
    public static bool IsOddInteger(Float96 a) => Float<UInt96>.IsOddInteger(a.p);
    public static bool IsPositiveInfinity(Float96 a) => Float<UInt96>.IsPositiveInfinity(a.p);
    public static bool IsPow2(Float96 a) => Float<UInt96>.IsPow2(a.p);
    public static bool IsRealNumber(Float96 a) => Float<UInt96>.IsRealNumber(a.p);
    public static bool IsSubnormal(Float96 a) => Float<UInt96>.IsSubnormal(a.p);
    public static Float96 Log(Float96 a) => new Float96(Float<UInt96>.Log(a.p));
    public static Float96 Log10(Float96 a) => new Float96(Float<UInt96>.Log10(a.p));
    public static Float96 LogP1(Float96 a) => new Float96(Float<UInt96>.LogP1(a.p));
    public static Float96 Log2(Float96 a) => new Float96(Float<UInt96>.Log2(a.p));
    public static Float96 Log2P1(Float96 a) => new Float96(Float<UInt96>.Log2P1(a.p));
    public static Float96 Max(Float96 a, Float96 b) => new Float96(Float<UInt96>.Max(a.p, b.p));
    public static Float96 MaxMagnitude(Float96 a, Float96 b) => new Float96(Float<UInt96>.MaxMagnitude(a.p, b.p));
    public static Float96 MaxMagnitudeNumber(Float96 a, Float96 b) => new Float96(Float<UInt96>.MaxMagnitudeNumber(a.p, b.p));
    public static Float96 MaxNumber(Float96 a, Float96 b) => new Float96(Float<UInt96>.MaxNumber(a.p, b.p));
    public static Float96 Min(Float96 a, Float96 b) => new Float96(Float<UInt96>.Min(a.p, b.p));
    public static Float96 MinMagnitude(Float96 a, Float96 b) => new Float96(Float<UInt96>.MinMagnitude(a.p, b.p));
    public static Float96 MinMagnitudeNumber(Float96 a, Float96 b) => new Float96(Float<UInt96>.MinMagnitudeNumber(a.p, b.p));
    public static Float96 MinNumber(Float96 a, Float96 b) => new Float96(Float<UInt96>.MinNumber(a.p, b.p));
    public static Float96 Pow(Float96 a, Float96 b) => new Float96(Float<UInt96>.Pow(a.p, b.p));
    public static Float96 ReciprocalEstimate(Float96 a) => new Float96(Float<UInt96>.ReciprocalEstimate(a.p));
    public static Float96 ReciprocalSqrtEstimate(Float96 a) => new Float96(Float<UInt96>.ReciprocalSqrtEstimate(a.p));
    public static Float96 RootN(Float96 a, int n) => new Float96(Float<UInt96>.RootN(a.p, n));
    public static Float96 Round(Float96 a) => new Float96(Float<UInt96>.Round(a.p));
    public static Float96 Round(Float96 a, int digits) => new Float96(Float<UInt96>.Round(a.p, digits));
    public static Float96 Round(Float96 a, int digits, MidpointRounding mode) => new Float96(Float<UInt96>.Round(a.p, digits, mode));
    public static Float96 ScaleB(Float96 a, int n) => new Float96(Float<UInt96>.ScaleB(a.p, n));
    public static int Sign(Float96 a) => Float<UInt96>.Sign(a.p);
    public static (Float96 Sin, Float96 Cos) SinCos(Float96 a) { var t = Float<UInt96>.SinCos(a.p); return (new Float96(t.Sin), new Float96(t.Cos)); }
    public static (Float96 SinPi, Float96 CosPi) SinCosPi(Float96 a) { var t = Float<UInt96>.SinCosPi(a.p); return (new Float96(t.SinPi), new Float96(t.CosPi)); }
    public static Float96 Sinh(Float96 a) => new Float96(Float<UInt96>.Sinh(a.p));
    public static Float96 SinPi(Float96 a) => new Float96(Float<UInt96>.SinPi(a.p));
    public static Float96 Sqrt(Float96 a) => new Float96(Float<UInt96>.Sqrt(a.p));
    public static Float96 Tan(Float96 a) => new Float96(Float<UInt96>.Tan(a.p));
    public static Float96 Tanh(Float96 a) => new Float96(Float<UInt96>.Tanh(a.p));
    public static Float96 TanPi(Float96 a) => new Float96(Float<UInt96>.TanPi(a.p));
    public static Float96 Truncate(Float96 a) => new Float96(Float<UInt96>.Truncate(a.p));

    public static Float96 Cast<T>(Float<T> a) where T : unmanaged => new Float96(Float<T>.Cast<UInt96>(a));
    public static Float<T> Cast<T>(Float96 a) where T : unmanaged => Float<UInt96>.Cast<T>(a.p);

    Float96(Float<UInt96> p) => this.p = p;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Float<UInt96> p;
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
    private readonly struct UInt96 { readonly UInt32 high, mid, low; }
  }

}
