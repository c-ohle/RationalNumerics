using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics.Generic;
using System.Runtime.InteropServices;

#pragma warning disable CS1591 //todo: xml comments

namespace System.Numerics
{
  /// <summary>
  /// Represents a 128-bit quadruple-precision floating-point number. <b>(under construction)</b>
  /// </summary>
  /// <remarks>
  /// Also known as C/C++ (GCC) <c>__float128</c><br/> 
  /// The data format and properties as defined in <seealso href="https://en.wikipedia.org/wiki/IEEE_754">IEEE 754</seealso>.<br/>
  /// </remarks>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float128 : IFloat<Float128>, IComparable<Float128>, IEquatable<Float128>, IComparable, ISpanFormattable    
  {
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float128 b) => Float<UInt128>.Equals(this.p, b.p);
    public readonly int CompareTo(Float128 b) => Float<UInt128>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float128 b ? Float<UInt128>.Compare(this.p, b.p) : throw new ArgumentException();

    public static implicit operator Float128(int value) => new Float128((Float<UInt128>)value);
    public static implicit operator Float128(Half value) => new Float128((Float<UInt128>)value);
    public static implicit operator Float128(float value) => new Float128((Float<UInt128>)value);
    public static implicit operator Float128(double value) => new Float128((Float<UInt128>)value);
    public static implicit operator Float128(decimal value) => new Float128((Float<UInt128>)value);
    public static explicit operator Float128(BigRational value) => new Float128((Float<UInt128>)value);

    public static explicit operator int(Float128 value) => (int)value.p;
    public static explicit operator Half(Float128 value) => (Half)value.p;
    public static explicit operator float(Float128 value) => (float)value.p;
    public static explicit operator double(Float128 value) => (double)value.p;
    public static explicit operator decimal(Float128 value) => (decimal)value.p;
    public static implicit operator BigRational(Float128 value) => (BigRational)value.p;

    public static Float128 operator ++(Float128 a) => new Float128(a.p + +1);
    public static Float128 operator --(Float128 a) => new Float128(a.p + -1);
    public static Float128 operator +(Float128 a, Float128 b) => new Float128(a.p + b.p);
    public static Float128 operator -(Float128 a, Float128 b) => new Float128(a.p - b.p);
    public static Float128 operator *(Float128 a, Float128 b) => new Float128(a.p * b.p);
    public static Float128 operator /(Float128 a, Float128 b) => new Float128(a.p / b.p);
    public static Float128 operator %(Float128 a, Float128 b) => new Float128(a.p % b.p);
    
    public static bool operator ==(Float128 a, Float128 b) => a.p == b.p;
    public static bool operator !=(Float128 a, Float128 b) => a.p != b.p;
    public static bool operator <=(Float128 a, Float128 b) => a.p <= b.p;
    public static bool operator >=(Float128 a, Float128 b) => a.p >= b.p;
    public static bool operator <(Float128 a, Float128 b) => a.p < b.p;
    public static bool operator >(Float128 a, Float128 b) => a.p > b.p;
     
    public static Float128 Pi => new Float128(Float<UInt128>.Pi);
    public static Float128 Tau => new Float128(Float<UInt128>.Tau);
    public static Float128 E => new Float128(Float<UInt128>.E);
    public static Float128 MinValue => new Float128(Float<UInt128>.MinValue);
    public static Float128 MaxValue => new Float128(Float<UInt128>.MaxValue);
    public static Float128 Epsilon => new Float128(Float<UInt128>.Epsilon);
    public static Float128 NaN => new Float128(Float<UInt128>.NaN);
    public static Float128 NegativeInfinity => new Float128(Float<UInt128>.NegativeInfinity);
    public static Float128 PositiveInfinity => new Float128(Float<UInt128>.PositiveInfinity);
    public static Float128 NegativeZero => new Float128(Float<UInt128>.NegativeZero);

    public static Float128 Abs(Float128 a) => new Float128(Float<UInt128>.Abs(a.p));
    public static Float128 Acos(Float128 a) => new Float128(Float<UInt128>.Acos(a.p));
    public static Float128 Acosh(Float128 a) => new Float128(Float<UInt128>.Acosh(a.p));
    public static Float128 AcosPi(Float128 a) => new Float128(Float<UInt128>.AcosPi(a.p));
    public static Float128 Asin(Float128 a) => new Float128(Float<UInt128>.Asin(a.p));
    public static Float128 Asinh(Float128 a) => new Float128(Float<UInt128>.Asinh(a.p));
    public static Float128 AsinPi(Float128 a) => new Float128(Float<UInt128>.AsinPi(a.p));
    public static Float128 Atan(Float128 a) => new Float128(Float<UInt128>.Atan(a.p));
    public static Float128 Atanh(Float128 a) => new Float128(Float<UInt128>.Atanh(a.p));
    public static Float128 AtanPi(Float128 a) => new Float128(Float<UInt128>.AtanPi(a.p));
    public static Float128 Atan2Pi(Float128 a, Float128 b) => new Float128(Float<UInt128>.Atan2Pi(a.p, b.p));
    public static Float128 BitDecrement(Float128 a) => new Float128(Float<UInt128>.BitDecrement(a.p));
    public static Float128 BitIncrement(Float128 a) => new Float128(Float<UInt128>.BitIncrement(a.p));
    public static Float128 Cbrt(Float128 a) => new Float128(Float<UInt128>.Cbrt(a.p));
    public static Float128 Ceiling(Float128 a) => new Float128(Float<UInt128>.Ceiling(a.p));
    public static Float128 Clamp(Float128 a, Float128 min, Float128 max) => new Float128(Float<UInt128>.Clamp(a.p, min.p, max.p));
    public static Float128 CopySign(Float128 a, Float128 sign) => new Float128(Float<UInt128>.CopySign(a.p, sign.p));
    public static Float128 Cos(Float128 a) => new Float128(Float<UInt128>.Cos(a.p));
    public static Float128 Cosh(Float128 a) => new Float128(Float<UInt128>.Cosh(a.p));
    public static Float128 CosPi(Float128 a) => new Float128(Float<UInt128>.CosPi(a.p));
    public static Float128 Exp(Float128 a) => new Float128(Float<UInt128>.Exp(a.p));
    public static Float128 Exp10(Float128 a) => new Float128(Float<UInt128>.Exp10(a.p));
    public static Float128 Exp10M1(Float128 a) => new Float128(Float<UInt128>.Exp10M1(a.p));
    public static Float128 Exp2(Float128 a) => new Float128(Float<UInt128>.Exp2(a.p));
    public static Float128 Exp2M1(Float128 a) => new Float128(Float<UInt128>.Exp2M1(a.p));
    public static Float128 ExpM1(Float128 a) => new Float128(Float<UInt128>.ExpM1(a.p));
    public static Float128 Floor(Float128 a) => new Float128(Float<UInt128>.Floor(a.p));
    public static Float128 FusedMultiplyAdd(Float128 a, Float128 b, Float128 c) => new Float128(Float<UInt128>.FusedMultiplyAdd(a.p, b.p, c.p));
    public static Float128 Hypot(Float128 a, Float128 b) => new Float128(Float<UInt128>.Hypot(a.p, b.p));
    public static Float128 Ieee754Remainder(Float128 a, Float128 b) => new Float128(Float<UInt128>.Ieee754Remainder(a.p, b.p));
    public static int ILogB(Float128 a) => Float<UInt128>.ILogB(a.p);
    public static bool IsEvenInteger(Float128 a) => Float<UInt128>.IsEvenInteger(a.p);
    public static bool IsFinite(Float128 a) => Float<UInt128>.IsFinite(a.p);
    public static bool IsInfinity(Float128 a) => Float<UInt128>.IsInfinity(a.p);
    public static bool IsInteger(Float128 a) => Float<UInt128>.IsInteger(a.p);
    public static bool IsNaN(Float128 a) => Float<UInt128>.IsNaN(a.p);
    public static bool IsNegative(Float128 a) => Float<UInt128>.IsNegative(a.p);
    public static bool IsNegativeInfinity(Float128 a) => Float<UInt128>.IsNegativeInfinity(a.p);
    public static bool IsNormal(Float128 a) => Float<UInt128>.IsNormal(a.p);
    public static bool IsOddInteger(Float128 a) => Float<UInt128>.IsOddInteger(a.p);
    public static bool IsPositiveInfinity(Float128 a) => Float<UInt128>.IsPositiveInfinity(a.p);
    public static bool IsPow2(Float128 a) => Float<UInt128>.IsPow2(a.p);
    public static bool IsRealNumber(Float128 a) => Float<UInt128>.IsRealNumber(a.p);
    public static bool IsSubnormal(Float128 a) => Float<UInt128>.IsSubnormal(a.p);
    public static Float128 Log(Float128 a) => new Float128(Float<UInt128>.Log(a.p));
    public static Float128 Log10(Float128 a) => new Float128(Float<UInt128>.Log10(a.p));
    public static Float128 LogP1(Float128 a) => new Float128(Float<UInt128>.LogP1(a.p));
    public static Float128 Log2(Float128 a) => new Float128(Float<UInt128>.Log2(a.p));
    public static Float128 Log2P1(Float128 a) => new Float128(Float<UInt128>.Log2P1(a.p));
    public static Float128 Max(Float128 a, Float128 b) => new Float128(Float<UInt128>.Max(a.p, b.p));
    public static Float128 MaxMagnitude(Float128 a, Float128 b) => new Float128(Float<UInt128>.MaxMagnitude(a.p, b.p));
    public static Float128 MaxMagnitudeNumber(Float128 a, Float128 b) => new Float128(Float<UInt128>.MaxMagnitudeNumber(a.p, b.p));
    public static Float128 MaxNumber(Float128 a, Float128 b) => new Float128(Float<UInt128>.MaxNumber(a.p, b.p));
    public static Float128 Min(Float128 a, Float128 b) => new Float128(Float<UInt128>.Min(a.p, b.p));
    public static Float128 MinMagnitude(Float128 a, Float128 b) => new Float128(Float<UInt128>.MinMagnitude(a.p, b.p));
    public static Float128 MinMagnitudeNumber(Float128 a, Float128 b) => new Float128(Float<UInt128>.MinMagnitudeNumber(a.p, b.p));
    public static Float128 MinNumber(Float128 a, Float128 b) => new Float128(Float<UInt128>.MinNumber(a.p, b.p));
    public static Float128 Pow(Float128 a, Float128 b) => new Float128(Float<UInt128>.Pow(a.p, b.p));
    public static Float128 ReciprocalEstimate(Float128 a) => new Float128(Float<UInt128>.ReciprocalEstimate(a.p));
    public static Float128 ReciprocalSqrtEstimate(Float128 a) => new Float128(Float<UInt128>.ReciprocalSqrtEstimate(a.p));
    public static Float128 RootN(Float128 a, int n) => new Float128(Float<UInt128>.RootN(a.p, n));
    public static Float128 Round(Float128 a) => new Float128(Float<UInt128>.Round(a.p));
    public static Float128 Round(Float128 a, int digits) => new Float128(Float<UInt128>.Round(a.p, digits));
    public static Float128 Round(Float128 a, int digits, MidpointRounding mode ) => new Float128(Float<UInt128>.Round(a.p, digits, mode));
    public static Float128 ScaleB(Float128 a, int n) => new Float128(Float<UInt128>.ScaleB(a.p, n));
    public static int Sign(Float128 a) => Float<UInt128>.Sign(a.p);
    public static (Float128 Sin, Float128 Cos) SinCos(Float128 a) { var t=Float<UInt128>.SinCos(a.p); return (new Float128(t.Sin), new Float128(t.Cos)); }
    public static (Float128 SinPi, Float128 CosPi) SinCosPi(Float128 a) { var t = Float<UInt128>.SinCosPi(a.p); return (new Float128(t.SinPi), new Float128(t.CosPi)); }
    public static Float128 Sinh(Float128 a) => new Float128(Float<UInt128>.Sinh(a.p));
    public static Float128 SinPi(Float128 a) => new Float128(Float<UInt128>.SinPi(a.p));
    public static Float128 Sqrt(Float128 a) => new Float128(Float<UInt128>.Sqrt(a.p));
    public static Float128 Tan(Float128 a) => new Float128(Float<UInt128>.Tan(a.p));
    public static Float128 Tanh(Float128 a) => new Float128(Float<UInt128>.Tanh(a.p));
    public static Float128 TanPi(Float128 a) => new Float128(Float<UInt128>.TanPi(a.p));
    public static Float128 Truncate(Float128 a) => new Float128(Float<UInt128>.Truncate(a.p));

    public static Float128 Cast<T>(Float<T> a) where T : unmanaged => new Float128(Float<T>.Cast<UInt128>(a));
    public static Float<T> Cast<T>(Float128 a) where T : unmanaged => Float<UInt128>.Cast<T>(a.p);
    
    Float128(Float<UInt128> p) => this.p = p;
    private readonly Float<UInt128> p;
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    private readonly struct UInt128 { } //NET6 compat
  }

  /// <summary>
  /// Represents a 256-bit octuple-precision floating-point number. <b>(under construction)</b>
  /// </summary>
  /// <remarks>
  /// The data format and properties as defined in <seealso href="https://en.wikipedia.org/wiki/IEEE_754">IEEE 754</seealso>.
  /// </remarks>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float256 : IFloat<Float256>, IComparable<Float256>, IEquatable<Float256>, IComparable, ISpanFormattable
  {
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float256 b) => Float<UInt256>.Equals(this.p, b.p);
    public readonly int CompareTo(Float256 b) => Float<UInt256>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float256 b ? Float<UInt256>.Compare(this.p, b.p) : throw new ArgumentException();

    public static implicit operator Float256(int value) => new Float256((Float<UInt256>)value);
    public static implicit operator Float256(Half value) => new Float256((Float<UInt256>)value);
    public static implicit operator Float256(float value) => new Float256((Float<UInt256>)value);
    public static implicit operator Float256(double value) => new Float256((Float<UInt256>)value);
    public static implicit operator Float256(decimal value) => new Float256((Float<UInt256>)value);
    public static implicit operator Float256(Float128 value) => new Float256(Float128.Cast<UInt256>(value));
    public static explicit operator Float256(BigRational value) => new Float256((Float<UInt256>)value);

    public static explicit operator int(Float256 value) => (int)value.p;
    public static explicit operator Half(Float256 value) => (Half)value.p;
    public static explicit operator float(Float256 value) => (float)value.p;
    public static explicit operator double(Float256 value) => (double)value.p;
    public static explicit operator decimal(Float256 value) => (decimal)value.p;
    public static explicit operator Float128(Float256 value) => Float128.Cast(value.p);
    public static implicit operator BigRational(Float256 value) => (BigRational)value.p;

    public static Float256 operator ++(Float256 a) => new Float256(a.p + +1);
    public static Float256 operator --(Float256 a) => new Float256(a.p + -1);
    public static Float256 operator +(Float256 a, Float256 b) => new Float256(a.p + b.p);
    public static Float256 operator -(Float256 a, Float256 b) => new Float256(a.p - b.p);
    public static Float256 operator *(Float256 a, Float256 b) => new Float256(a.p * b.p);
    public static Float256 operator /(Float256 a, Float256 b) => new Float256(a.p / b.p);
    public static Float256 operator %(Float256 a, Float256 b) => new Float256(a.p % b.p);

    public static bool operator ==(Float256 a, Float256 b) => a.p == b.p;
    public static bool operator !=(Float256 a, Float256 b) => a.p != b.p;
    public static bool operator <=(Float256 a, Float256 b) => a.p <= b.p;
    public static bool operator >=(Float256 a, Float256 b) => a.p >= b.p;
    public static bool operator <(Float256 a, Float256 b) => a.p < b.p;
    public static bool operator >(Float256 a, Float256 b) => a.p > b.p;

    public static Float256 Pi => new Float256(Float<UInt256>.Pi);
    public static Float256 Tau => new Float256(Float<UInt256>.Tau);
    public static Float256 E => new Float256(Float<UInt256>.E);
    public static Float256 MinValue => new Float256(Float<UInt256>.MinValue);
    public static Float256 MaxValue => new Float256(Float<UInt256>.MaxValue);
    public static Float256 Epsilon => new Float256(Float<UInt256>.Epsilon);
    public static Float256 NaN => new Float256(Float<UInt256>.NaN);
    public static Float256 NegativeInfinity => new Float256(Float<UInt256>.NegativeInfinity);
    public static Float256 PositiveInfinity => new Float256(Float<UInt256>.PositiveInfinity);
    public static Float256 NegativeZero => new Float256(Float<UInt256>.NegativeZero);

    public static Float256 Abs(Float256 a) => new Float256(Float<UInt256>.Abs(a.p));
    public static Float256 Acos(Float256 a) => new Float256(Float<UInt256>.Acos(a.p));
    public static Float256 Acosh(Float256 a) => new Float256(Float<UInt256>.Acosh(a.p));
    public static Float256 AcosPi(Float256 a) => new Float256(Float<UInt256>.AcosPi(a.p));
    public static Float256 Asin(Float256 a) => new Float256(Float<UInt256>.Asin(a.p));
    public static Float256 Asinh(Float256 a) => new Float256(Float<UInt256>.Asinh(a.p));
    public static Float256 AsinPi(Float256 a) => new Float256(Float<UInt256>.AsinPi(a.p));
    public static Float256 Atan(Float256 a) => new Float256(Float<UInt256>.Atan(a.p));
    public static Float256 Atanh(Float256 a) => new Float256(Float<UInt256>.Atanh(a.p));
    public static Float256 AtanPi(Float256 a) => new Float256(Float<UInt256>.AtanPi(a.p));
    public static Float256 Atan2Pi(Float256 a, Float256 b) => new Float256(Float<UInt256>.Atan2Pi(a.p,b.p));
    public static Float256 BitDecrement(Float256 a) => new Float256(Float<UInt256>.BitDecrement(a.p));
    public static Float256 BitIncrement(Float256 a) => new Float256(Float<UInt256>.BitIncrement(a.p));
    public static Float256 Cbrt(Float256 a) => new Float256(Float<UInt256>.Cbrt(a.p));
    public static Float256 Ceiling(Float256 a) => new Float256(Float<UInt256>.Ceiling(a.p));
    public static Float256 Clamp(Float256 a, Float256 min, Float256 max) => new Float256(Float<UInt256>.Clamp(a.p, min.p, max.p));
    public static Float256 CopySign(Float256 a, Float256 sign) => new Float256(Float<UInt256>.CopySign(a.p, sign.p));
    public static Float256 Cos(Float256 a) => new Float256(Float<UInt256>.Cos(a.p));
    public static Float256 Cosh(Float256 a) => new Float256(Float<UInt256>.Cosh(a.p));
    public static Float256 CosPi(Float256 a) => new Float256(Float<UInt256>.CosPi(a.p));
    public static Float256 Exp(Float256 a) => new Float256(Float<UInt256>.Exp(a.p));
    public static Float256 Exp10(Float256 a) => new Float256(Float<UInt256>.Exp10(a.p));
    public static Float256 Exp10M1(Float256 a) => new Float256(Float<UInt256>.Exp10M1(a.p));
    public static Float256 Exp2(Float256 a) => new Float256(Float<UInt256>.Exp2(a.p));
    public static Float256 Exp2M1(Float256 a) => new Float256(Float<UInt256>.Exp2M1(a.p));
    public static Float256 ExpM1(Float256 a) => new Float256(Float<UInt256>.ExpM1(a.p));
    public static Float256 Floor(Float256 a) => new Float256(Float<UInt256>.Floor(a.p));
    public static Float256 FusedMultiplyAdd(Float256 a, Float256 b, Float256 c) => new Float256(Float<UInt256>.FusedMultiplyAdd(a.p, b.p, c.p));
    public static Float256 Hypot(Float256 a, Float256 b) => new Float256(Float<UInt256>.Hypot(a.p, b.p));
    public static Float256 Ieee754Remainder(Float256 a, Float256 b) => new Float256(Float<UInt256>.Ieee754Remainder(a.p, b.p));
    public static int ILogB(Float256 a) => Float<UInt256>.ILogB(a.p);
    public static bool IsEvenInteger(Float256 a) => Float<UInt256>.IsEvenInteger(a.p);
    public static bool IsFinite(Float256 a) => Float<UInt256>.IsFinite(a.p);
    public static bool IsInfinity(Float256 a) => Float<UInt256>.IsInfinity(a.p);
    public static bool IsInteger(Float256 a) => Float<UInt256>.IsInteger(a.p);
    public static bool IsNaN(Float256 a) => Float<UInt256>.IsNaN(a.p);
    public static bool IsNegative(Float256 a) => Float<UInt256>.IsNegative(a.p);
    public static bool IsNegativeInfinity(Float256 a) => Float<UInt256>.IsNegativeInfinity(a.p);
    public static bool IsNormal(Float256 a) => Float<UInt256>.IsNormal(a.p);
    public static bool IsOddInteger(Float256 a) => Float<UInt256>.IsOddInteger(a.p);
    public static bool IsPositiveInfinity(Float256 a) => Float<UInt256>.IsPositiveInfinity(a.p);
    public static bool IsPow2(Float256 a) => Float<UInt256>.IsPow2(a.p);
    public static bool IsRealNumber(Float256 a) => Float<UInt256>.IsRealNumber(a.p);
    public static bool IsSubnormal(Float256 a) => Float<UInt256>.IsSubnormal(a.p);
    public static Float256 Log(Float256 a) => new Float256(Float<UInt256>.Log(a.p));
    public static Float256 Log10(Float256 a) => new Float256(Float<UInt256>.Log10(a.p));
    public static Float256 LogP1(Float256 a) => new Float256(Float<UInt256>.LogP1(a.p));
    public static Float256 Log2(Float256 a) => new Float256(Float<UInt256>.Log2(a.p));
    public static Float256 Log2P1(Float256 a) => new Float256(Float<UInt256>.Log2P1(a.p));
    public static Float256 Max(Float256 a, Float256 b) => new Float256(Float<UInt256>.Max(a.p, b.p));
    public static Float256 MaxMagnitude(Float256 a, Float256 b) => new Float256(Float<UInt256>.MaxMagnitude(a.p, b.p));
    public static Float256 MaxMagnitudeNumber(Float256 a, Float256 b) => new Float256(Float<UInt256>.MaxMagnitudeNumber(a.p, b.p));
    public static Float256 MaxNumber(Float256 a, Float256 b) => new Float256(Float<UInt256>.MaxNumber(a.p, b.p));
    public static Float256 Min(Float256 a, Float256 b) => new Float256(Float<UInt256>.Min(a.p, b.p));
    public static Float256 MinMagnitude(Float256 a, Float256 b) => new Float256(Float<UInt256>.MinMagnitude(a.p, b.p));
    public static Float256 MinMagnitudeNumber(Float256 a, Float256 b) => new Float256(Float<UInt256>.MinMagnitudeNumber(a.p, b.p));
    public static Float256 MinNumber(Float256 a, Float256 b) => new Float256(Float<UInt256>.MinNumber(a.p, b.p));
    public static Float256 Pow(Float256 a, Float256 b) => new Float256(Float<UInt256>.Pow(a.p, b.p));
    public static Float256 ReciprocalEstimate(Float256 a) => new Float256(Float<UInt256>.ReciprocalEstimate(a.p));
    public static Float256 ReciprocalSqrtEstimate(Float256 a) => new Float256(Float<UInt256>.ReciprocalSqrtEstimate(a.p));
    public static Float256 RootN(Float256 a, int n) => new Float256(Float<UInt256>.RootN(a.p, n));
    public static Float256 Round(Float256 a) => new Float256(Float<UInt256>.Round(a.p));
    public static Float256 Round(Float256 a, int digits) => new Float256(Float<UInt256>.Round(a.p, digits));
    public static Float256 Round(Float256 a, int digits, MidpointRounding mode) => new Float256(Float<UInt256>.Round(a.p, digits, mode));
    public static Float256 ScaleB(Float256 a, int n) => new Float256(Float<UInt256>.ScaleB(a.p, n));
    public static int Sign(Float256 a) => Float<UInt256>.Sign(a.p);
    public static (Float256 Sin, Float256 Cos) SinCos(Float256 a) { var t = Float<UInt256>.SinCos(a.p); return (new Float256(t.Sin), new Float256(t.Cos)); }
    public static (Float256 SinPi, Float256 CosPi) SinCosPi(Float256 a) { var t = Float<UInt256>.SinCosPi(a.p); return (new Float256(t.SinPi), new Float256(t.CosPi)); }
    public static Float256 Sinh(Float256 a) => new Float256(Float<UInt256>.Sinh(a.p));
    public static Float256 SinPi(Float256 a) => new Float256(Float<UInt256>.SinPi(a.p));
    public static Float256 Sqrt(Float256 a) => new Float256(Float<UInt256>.Sqrt(a.p));
    public static Float256 Tan(Float256 a) => new Float256(Float<UInt256>.Tan(a.p));
    public static Float256 Tanh(Float256 a) => new Float256(Float<UInt256>.Tanh(a.p));
    public static Float256 TanPi(Float256 a) => new Float256(Float<UInt256>.TanPi(a.p));
    public static Float256 Truncate(Float256 a) => new Float256(Float<UInt256>.Truncate(a.p));

    public static Float256 Cast<T>(Float<T> a) where T : unmanaged => new Float256(Float<T>.Cast<UInt256>(a));
    public static Float<T> Cast<T>(Float256 a) where T : unmanaged => Float<UInt256>.Cast<T>(a.p);

    private readonly Float<UInt256> p;
    Float256(Float<UInt256> p) => this.p = p;
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    private readonly struct UInt256 { }
  }

  /// <summary>
  /// Represents a 80-bit extended-precision floating-point number. <b>(under construction)</b>
  /// </summary>
  /// <remarks>
  /// Also known as C/C++ <c>long double</c>, GCC <c>__float80</c>, Pascal (Delphi) <c>Extended</c>, D <c>Real</c> BASIC <c>EXT</c> or <c>EXTENDED</c>.<br/>   
  /// The data format and properties as defined for the most common 
  /// <seealso href="https://en.wikipedia.org/wiki/Extended_precision#x86_extended_precision_format">x86 extended precision format</seealso>.<br/>
  /// </remarks>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float80 : IFloat<Float80>, IComparable<Float80>, IEquatable<Float80>, IComparable, ISpanFormattable
  {
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float80 b) => Float<UInt80>.Equals(this.p, b.p);
    public readonly int CompareTo(Float80 b) => Float<UInt80>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float80 b ? Float<UInt80>.Compare(this.p, b.p) : throw new ArgumentException();

    public static implicit operator int(Float80 value) => (int)value.p;
    public static implicit operator Half(Float80 value) => (Half)value.p;
    public static implicit operator float(Float80 value) => (float)value.p;
    public static implicit operator double(Float80 value) => (double)value.p;
    public static explicit operator decimal(Float80 value) => (decimal)(BigRational)value; //todo: inline     
    public static implicit operator Float128(Float80 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float80 value) => Float256.Cast(value.p);
    public static implicit operator BigRational(Float80 value) => (BigRational)value.p;

    public static implicit operator Float80(int value) => new Float80((Float<UInt80>)value);
    public static implicit operator Float80(Half value) => new Float80((Float<UInt80>)value);
    public static implicit operator Float80(float value) => new Float80((Float<UInt80>)value);
    public static implicit operator Float80(double value) => new Float80((Float<UInt80>)value);
    public static explicit operator Float80(Float128 value) => new Float80(Float128.Cast<UInt80>(value));
    public static explicit operator Float80(Float256 value) => new Float80(Float256.Cast<UInt80>(value));
    public static explicit operator Float80(BigRational value) => new Float80((Float<UInt80>)value);

    public static Float80 operator ++(Float80 a) => new Float80(a.p + +1);
    public static Float80 operator --(Float80 a) => new Float80(a.p + -1);
    public static Float80 operator +(Float80 a, Float80 b) => new Float80(a.p + b.p);
    public static Float80 operator -(Float80 a, Float80 b) => new Float80(a.p - b.p);
    public static Float80 operator *(Float80 a, Float80 b) => new Float80(a.p * b.p);
    public static Float80 operator /(Float80 a, Float80 b) => new Float80(a.p / b.p);
    public static Float80 operator %(Float80 a, Float80 b) => new Float80(a.p % b.p);

    public static bool operator ==(Float80 a, Float80 b) => a.p == b.p;
    public static bool operator !=(Float80 a, Float80 b) => a.p != b.p;
    public static bool operator <=(Float80 a, Float80 b) => a.p <= b.p;
    public static bool operator >=(Float80 a, Float80 b) => a.p >= b.p;
    public static bool operator <(Float80 a, Float80 b) => a.p < b.p;
    public static bool operator >(Float80 a, Float80 b) => a.p > b.p;

    public static Float80 Pi => new Float80(Float<UInt80>.Pi);
    public static Float80 Tau => new Float80(Float<UInt80>.Tau);
    public static Float80 E => new Float80(Float<UInt80>.E);
    public static Float80 MinValue => new Float80(Float<UInt80>.MinValue);
    public static Float80 MaxValue => new Float80(Float<UInt80>.MaxValue);
    public static Float80 Epsilon => new Float80(Float<UInt80>.Epsilon);
    public static Float80 NaN => new Float80(Float<UInt80>.NaN);
    public static Float80 NegativeInfinity => new Float80(Float<UInt80>.NegativeInfinity);
    public static Float80 PositiveInfinity => new Float80(Float<UInt80>.PositiveInfinity);
    public static Float80 NegativeZero => new Float80(Float<UInt80>.NegativeZero);

    public static Float80 Abs(Float80 a) => new Float80(Float<UInt80>.Abs(a.p));
    public static Float80 Acos(Float80 a) => new Float80(Float<UInt80>.Acos(a.p));
    public static Float80 Acosh(Float80 a) => new Float80(Float<UInt80>.Acosh(a.p));
    public static Float80 AcosPi(Float80 a) => new Float80(Float<UInt80>.AcosPi(a.p));
    public static Float80 Asin(Float80 a) => new Float80(Float<UInt80>.Asin(a.p));
    public static Float80 Asinh(Float80 a) => new Float80(Float<UInt80>.Asinh(a.p));
    public static Float80 AsinPi(Float80 a) => new Float80(Float<UInt80>.AsinPi(a.p));
    public static Float80 Atan(Float80 a) => new Float80(Float<UInt80>.Atan(a.p));
    public static Float80 Atanh(Float80 a) => new Float80(Float<UInt80>.Atanh(a.p));
    public static Float80 AtanPi(Float80 a) => new Float80(Float<UInt80>.AtanPi(a.p));
    public static Float80 Atan2Pi(Float80 a, Float80 b) => new Float80(Float<UInt80>.Atan2Pi(a.p,b.p));
    public static Float80 BitDecrement(Float80 a) => new Float80(Float<UInt80>.BitDecrement(a.p));
    public static Float80 BitIncrement(Float80 a) => new Float80(Float<UInt80>.BitIncrement(a.p));
    public static Float80 Cbrt(Float80 a) => new Float80(Float<UInt80>.Cbrt(a.p));
    public static Float80 Ceiling(Float80 a) => new Float80(Float<UInt80>.Ceiling(a.p));
    public static Float80 Clamp(Float80 a, Float80 min, Float80 max) => new Float80(Float<UInt80>.Clamp(a.p, min.p, max.p));
    public static Float80 CopySign(Float80 a, Float80 sign) => new Float80(Float<UInt80>.CopySign(a.p, sign.p));
    public static Float80 Cos(Float80 a) => new Float80(Float<UInt80>.Cos(a.p));
    public static Float80 Cosh(Float80 a) => new Float80(Float<UInt80>.Cosh(a.p));
    public static Float80 CosPi(Float80 a) => new Float80(Float<UInt80>.CosPi(a.p));
    public static Float80 Exp(Float80 a) => new Float80(Float<UInt80>.Exp(a.p));
    public static Float80 Exp10(Float80 a) => new Float80(Float<UInt80>.Exp10(a.p));
    public static Float80 Exp10M1(Float80 a) => new Float80(Float<UInt80>.Exp10M1(a.p));
    public static Float80 Exp2(Float80 a) => new Float80(Float<UInt80>.Exp2(a.p));
    public static Float80 Exp2M1(Float80 a) => new Float80(Float<UInt80>.Exp2M1(a.p));
    public static Float80 ExpM1(Float80 a) => new Float80(Float<UInt80>.ExpM1(a.p));
    public static Float80 Floor(Float80 a) => new Float80(Float<UInt80>.Floor(a.p));
    public static Float80 FusedMultiplyAdd(Float80 a, Float80 b, Float80 c) => new Float80(Float<UInt80>.FusedMultiplyAdd(a.p, b.p, c.p));
    public static Float80 Hypot(Float80 a, Float80 b) => new Float80(Float<UInt80>.Hypot(a.p, b.p));
    public static Float80 Ieee754Remainder(Float80 a, Float80 b) => new Float80(Float<UInt80>.Ieee754Remainder(a.p, b.p));
    public static int ILogB(Float80 a) => Float<UInt80>.ILogB(a.p);
    public static bool IsEvenInteger(Float80 a) => Float<UInt80>.IsEvenInteger(a.p);
    public static bool IsFinite(Float80 a) => Float<UInt80>.IsFinite(a.p);
    public static bool IsInfinity(Float80 a) => Float<UInt80>.IsInfinity(a.p);
    public static bool IsInteger(Float80 a) => Float<UInt80>.IsInteger(a.p);
    public static bool IsNaN(Float80 a) => Float<UInt80>.IsNaN(a.p);
    public static bool IsNegative(Float80 a) => Float<UInt80>.IsNegative(a.p);
    public static bool IsNegativeInfinity(Float80 a) => Float<UInt80>.IsNegativeInfinity(a.p);
    public static bool IsNormal(Float80 a) => Float<UInt80>.IsNormal(a.p);
    public static bool IsOddInteger(Float80 a) => Float<UInt80>.IsOddInteger(a.p);
    public static bool IsPositiveInfinity(Float80 a) => Float<UInt80>.IsPositiveInfinity(a.p);
    public static bool IsPow2(Float80 a) => Float<UInt80>.IsPow2(a.p);
    public static bool IsRealNumber(Float80 a) => Float<UInt80>.IsRealNumber(a.p);
    public static bool IsSubnormal(Float80 a) => Float<UInt80>.IsSubnormal(a.p);
    public static Float80 Log(Float80 a) => new Float80(Float<UInt80>.Log(a.p));
    public static Float80 Log10(Float80 a) => new Float80(Float<UInt80>.Log10(a.p));
    public static Float80 LogP1(Float80 a) => new Float80(Float<UInt80>.LogP1(a.p));
    public static Float80 Log2(Float80 a) => new Float80(Float<UInt80>.Log2(a.p));
    public static Float80 Log2P1(Float80 a) => new Float80(Float<UInt80>.Log2P1(a.p));
    public static Float80 Max(Float80 a, Float80 b) => new Float80(Float<UInt80>.Max(a.p, b.p));
    public static Float80 MaxMagnitude(Float80 a, Float80 b) => new Float80(Float<UInt80>.MaxMagnitude(a.p, b.p));
    public static Float80 MaxMagnitudeNumber(Float80 a, Float80 b) => new Float80(Float<UInt80>.MaxMagnitudeNumber(a.p, b.p));
    public static Float80 MaxNumber(Float80 a, Float80 b) => new Float80(Float<UInt80>.MaxNumber(a.p, b.p));
    public static Float80 Min(Float80 a, Float80 b) => new Float80(Float<UInt80>.Min(a.p, b.p));
    public static Float80 MinMagnitude(Float80 a, Float80 b) => new Float80(Float<UInt80>.MinMagnitude(a.p, b.p));
    public static Float80 MinMagnitudeNumber(Float80 a, Float80 b) => new Float80(Float<UInt80>.MinMagnitudeNumber(a.p, b.p));
    public static Float80 MinNumber(Float80 a, Float80 b) => new Float80(Float<UInt80>.MinNumber(a.p, b.p));
    public static Float80 Pow(Float80 a, Float80 b) => new Float80(Float<UInt80>.Pow(a.p, b.p));
    public static Float80 ReciprocalEstimate(Float80 a) => new Float80(Float<UInt80>.ReciprocalEstimate(a.p));
    public static Float80 ReciprocalSqrtEstimate(Float80 a) => new Float80(Float<UInt80>.ReciprocalSqrtEstimate(a.p));
    public static Float80 RootN(Float80 a, int n) => new Float80(Float<UInt80>.RootN(a.p, n));
    public static Float80 Round(Float80 a) => new Float80(Float<UInt80>.Round(a.p));
    public static Float80 Round(Float80 a, int digits) => new Float80(Float<UInt80>.Round(a.p, digits));
    public static Float80 Round(Float80 a, int digits, MidpointRounding mode) => new Float80(Float<UInt80>.Round(a.p, digits, mode));
    public static Float80 ScaleB(Float80 a, int n) => new Float80(Float<UInt80>.ScaleB(a.p, n));
    public static int Sign(Float80 a) => Float<UInt80>.Sign(a.p);
    public static (Float80 Sin, Float80 Cos) SinCos(Float80 a) { var t = Float<UInt80>.SinCos(a.p); return (new Float80(t.Sin), new Float80(t.Cos)); }
    public static (Float80 SinPi, Float80 CosPi) SinCosPi(Float80 a) { var t = Float<UInt80>.SinCosPi(a.p); return (new Float80(t.SinPi), new Float80(t.CosPi)); }
    public static Float80 Sinh(Float80 a) => new Float80(Float<UInt80>.Sinh(a.p));
    public static Float80 SinPi(Float80 a) => new Float80(Float<UInt80>.SinPi(a.p));
    public static Float80 Sqrt(Float80 a) => new Float80(Float<UInt80>.Sqrt(a.p));
    public static Float80 Tan(Float80 a) => new Float80(Float<UInt80>.Tan(a.p));
    public static Float80 Tanh(Float80 a) => new Float80(Float<UInt80>.Tanh(a.p));
    public static Float80 TanPi(Float80 a) => new Float80(Float<UInt80>.TanPi(a.p));
    public static Float80 Truncate(Float80 a) => new Float80(Float<UInt80>.Truncate(a.p));

    public static Float80 Cast<T>(Float<T> a) where T : unmanaged => new Float80(Float<T>.Cast<UInt80>(a));
    public static Float<T> Cast<T>(Float80 a) where T : unmanaged => Float<UInt80>.Cast<T>(a.p);

    Float80(Float<UInt80> p) => this.p = p;
    private readonly Float<UInt80> p;
    [StructLayout(LayoutKind.Sequential, Size = 10)] // Pack = 2 
    private readonly struct UInt80 { } // readonly UInt16 upper; readonly UInt64 lower; }
  }
}
