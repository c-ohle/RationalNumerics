

global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;

global using Float32 = System.Numerics.Generic.Float<Test.SizeType32>;
global using Float64 = System.Numerics.Generic.Float<Test.SizeType64>;
global using Float80 = System.Numerics.Generic.Float<Test.SizeType80>;
global using Float128 = System.Numerics.Generic.Float<Test.SizeType128>;
global using Float256 = System.Numerics.Generic.Float<Test.SizeType256>;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Numerics.Generic;
using System.Runtime.InteropServices;

namespace Test
{
  [StructLayout(LayoutKind.Sequential, Size = 04)] readonly struct SizeType32 { }
  [StructLayout(LayoutKind.Sequential, Size = 08)] readonly struct SizeType64 { }
  [StructLayout(LayoutKind.Sequential, Size = 10)] readonly struct SizeType80 { }
  [StructLayout(LayoutKind.Sequential, Size = 16)] readonly struct SizeType128 { }
  [StructLayout(LayoutKind.Sequential, Size = 32)] readonly struct SizeType256 { }

#if false
  [StructLayout(LayoutKind.Sequential, Size = 8), Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float64 : IFloatType<Float64>
  {
    public override string ToString() => ToString(null);
    public readonly string ToString(string? format, IFormatProvider? provider = default)
    {
      return ((Float<Float64>)this).ToString(format, provider);
    }
    public readonly bool TryFormat(Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
      return ((Float<Float64>)this).TryFormat(dest, out charsWritten, format, provider);
    }
    public readonly override int GetHashCode() => ((Float<Float64>)this).GetHashCode();
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => ((Float<Float64>)this).Equals(obj);
    public readonly bool Equals(Float64 other) => ((Float<Float64>)this).Equals(other);
    public static Float64 Parse(ReadOnlySpan<char> value, IFormatProvider? provider = null)
    {
      TryParse(value, provider, out var r); return r;
    }
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out Float64 result)
    {
      var ok = Float<Float64>.TryParse(value, provider, out var r); result = r; return ok;
    }
    public static bool TryParse(ReadOnlySpan<char> value, NumberStyles style, IFormatProvider? provider, out Float64 result)
    {
      var ok = Float<Float64>.TryParse(value, style, provider, out var r); result = r; return ok;
    }

    public static implicit operator Float64(int value) => (Float<Float64>)value;
    public static implicit operator Float64(float value) => (Float<Float64>)value;
    public static implicit operator Float64(double value) => (Float<Float64>)value;
    public static explicit operator Float64(BigRational value) => (Float<Float64>)value;
    public static implicit operator BigRational(Float64 value) => (Float<Float64>)value;

    public static Float64 operator +(Float64 a) => a;
    public static Float64 operator -(Float64 a) => -(Float<Float64>)a;
    public static Float64 operator +(Float64 a, Float64 b) => (Float<Float64>)a + (Float<Float64>)b;
    public static Float64 operator -(Float64 a, Float64 b) => (Float<Float64>)a - (Float<Float64>)b;
    public static Float64 operator *(Float64 a, Float64 b) => (Float<Float64>)a * (Float<Float64>)b;
    public static Float64 operator /(Float64 a, Float64 b) => (Float<Float64>)a / (Float<Float64>)b;
    public static Float64 operator %(Float64 a, Float64 b) => (Float<Float64>)a % (Float<Float64>)b;
    public static Float64 operator ++(Float64 a) => (Float<Float64>)a + 1;
    public static Float64 operator --(Float64 a) => (Float<Float64>)a - 1;
    public static bool operator ==(Float64 a, Float64 b) => (Float<Float64>)a == (Float<Float64>)b;
    public static bool operator !=(Float64 a, Float64 b) => (Float<Float64>)a != (Float<Float64>)b;
    public static bool operator <=(Float64 a, Float64 b) => (Float<Float64>)a <= (Float<Float64>)b;
    public static bool operator >=(Float64 a, Float64 b) => (Float<Float64>)a >= (Float<Float64>)b;
    public static bool operator <(Float64 a, Float64 b) => (Float<Float64>)a < (Float<Float64>)b;
    public static bool operator >(Float64 a, Float64 b) => (Float<Float64>)a > (Float<Float64>)b;

    public static Float64 E => Float<Float64>.E;
    public static Float64 Pi => Float<Float64>.Pi;
    public static Float64 Tau => Float<Float64>.Tau;
    public static Float64 MinValue => Float<Float64>.MinValue;
    public static Float64 MaxValue => Float<Float64>.MaxValue;
    public static Float64 NaN => Float<Float64>.NaN;
    public static Float64 Epsilon => Float<Float64>.Epsilon;
    public static Float64 NegativeInfinity => Float<Float64>.NegativeInfinity;
    public static Float64 NegativeZero => Float<Float64>.NegativeZero;
    public static Float64 PositiveInfinity => Float<Float64>.PositiveInfinity;

    public static bool IsEvenInteger(Float64 a) => Float<Float64>.IsEvenInteger(a);
    public static bool IsInteger(Float64 a) => Float<Float64>.IsInteger(a);
    public static bool IsOddInteger(Float64 a) => Float<Float64>.IsOddInteger(a);
    public static bool IsPositive(Float64 a) => Float<Float64>.IsPositive(a);
    public static bool IsRealNumber(Float64 a) => Float<Float64>.IsRealNumber(a);
    public static bool IsFinite(Float64 a) => Float<Float64>.IsFinite(a);
    public static bool IsInfinity(Float64 a) => Float<Float64>.IsInfinity(a);
    public static bool IsNaN(Float64 a) => Float<Float64>.IsNaN(a);
    public static bool IsNegative(Float64 a) => Float<Float64>.IsNegative(a);
    public static bool IsNegativeInfinity(Float64 a) => Float<Float64>.IsNegativeInfinity(a);
    public static bool IsNormal(Float64 a) => Float<Float64>.IsNormal(a);
    public static bool IsPositiveInfinity(Float64 a) => Float<Float64>.IsPositiveInfinity(a);
    public static bool IsSubnormal(Float64 a) => Float<Float64>.IsSubnormal(a);
    public static bool IsPow2(Float64 a) => Float<Float64>.IsPow2(a);

    public static Float64 Abs(Float64 a) => Float<Float64>.Abs(a);
    public static Float64 Sqrt(Float64 a) => Float<Float64>.Sqrt(a);
    public static int Sign(Float64 a) => Float<Float64>.Sign(a);
    public static Float64 Truncate(Float64 a) => Float<Float64>.Truncate(a);
    public static Float64 Min(Float64 a, Float64 b) => Float<Float64>.Min(a, b);
    public static Float64 Max(Float64 a, Float64 b) => Float<Float64>.Max(a, b);
    public static Float64 Round(Float64 a, int digits = 0, MidpointRounding mode = MidpointRounding.ToEven) => Float<Float64>.Round(a, digits, mode);
    public static Float64 Floor(Float64 a) => Float<Float64>.Floor(a);
    public static Float64 Ceiling(Float64 a) => Float<Float64>.Ceiling(a);
    public static Float64 Pow(Float64 x, Float64 y) => Float<Float64>.Pow(x, y);
    public static Float64 MaxMagnitude(Float64 x, Float64 y) => Float<Float64>.MaxMagnitude(x, y);
    public static Float64 MaxMagnitudeNumber(Float64 x, Float64 y) => Float<Float64>.MaxMagnitudeNumber(x, y);
    public static Float64 MinMagnitude(Float64 x, Float64 y) => Float<Float64>.MinMagnitude(x, y);
    public static Float64 MinMagnitudeNumber(Float64 x, Float64 y) => Float<Float64>.MinMagnitudeNumber(x, y);
    public static Float64 ScaleB(Float64 x, int n) => Float<Float64>.ScaleB(x, n);
    public static Float64 Exp10(Float64 x) => Float<Float64>.Exp10(x);
    public static int ILogB(Float64 x) => Float<Float64>.ILogB(x);
    public static Float64 Atan2Pi(Float64 y, Float64 x) => Float<Float64>.Atan2Pi(y, x);
    public static Float64 Exp2(Float64 x) => Float<Float64>.Exp2(x);
    public static Float64 BitIncrement(Float64 x) => Float<Float64>.BitIncrement(x);
    public static Float64 BitDecrement(Float64 x) => Float<Float64>.BitDecrement(x);
    public static Float64 FusedMultiplyAdd(Float64 a, Float64 b, Float64 c) => Float<Float64>.FusedMultiplyAdd(a, b, c);
    public static Float64 Ieee754Remainder(Float64 a, Float64 b) => Float<Float64>.Ieee754Remainder(a, b);
    public static Float64 Pow2(Float64 x) => Float<Float64>.Pow2(x);
    public static Float64 Cbrt(Float64 x) => Float<Float64>.Cbrt(x);
    public static Float64 RootN(Float64 x, int n) => Float<Float64>.RootN(x, n);
    public static Float64 Hypot(Float64 x, Float64 y) => Float<Float64>.Hypot(x, y);
    public static Float64 Log2(Float64 x) => Float<Float64>.Log2(x);
    public static Float64 Log10(Float64 x) => Float<Float64>.Log10(x);
    public static Float64 Log(Float64 x) => Float<Float64>.Log(x);
    public static Float64 Log(Float64 x, Float64 newBase) => Float<Float64>.Log(x, newBase);
    public static Float64 Exp(Float64 x) => Float<Float64>.Exp(x);
    public static Float64 Sin(Float64 x) => Float<Float64>.Sin(x);
    public static Float64 Cos(Float64 x) => Float<Float64>.Cos(x);
    public static Float64 Tan(Float64 x) => Float<Float64>.Tan(x);
    public static Float64 Asin(Float64 x) => Float<Float64>.Asin(x);
    public static Float64 Acos(Float64 x) => Float<Float64>.Acos(x);
    public static Float64 Atan(Float64 x) => Float<Float64>.Atan(x);
    public static Float64 Atan2(Float64 y, Float64 x) => Float<Float64>.Atan2(y, x);
    public static Float64 Asinh(Float64 x) => Float<Float64>.Asinh(x);
    public static Float64 Acosh(Float64 x) => Float<Float64>.Acosh(x);
    public static Float64 Atanh(Float64 x) => Float<Float64>.Atanh(x);
    public static Float64 Sinh(Float64 x) => Float<Float64>.Asinh(x);
    public static Float64 Cosh(Float64 x) => Float<Float64>.Acosh(x);
    public static Float64 Tanh(Float64 x) => Float<Float64>.Tanh(x);
    public static Float64 SinPi(Float64 x) => Float<Float64>.SinPi(x);
    public static Float64 TanPi(Float64 x) => Float<Float64>.TanPi(x);
    public static Float64 AcosPi(Float64 x) => Float<Float64>.AcosPi(x);
    public static Float64 AsinPi(Float64 x) => Float<Float64>.AsinPi(x);
    public static Float64 AtanPi(Float64 x) => Float<Float64>.AtanPi(x);
    public static Float64 CosPi(Float64 x) => Float<Float64>.CosPi(x);

  }

  [StructLayout(LayoutKind.Sequential, Size = 10), Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float80 : IFloatType<Float80>
  {
    public override string ToString() => ToString(null);
    public readonly string ToString(string? format, IFormatProvider? provider = default)
    {
      return ((Float<Float80>)this).ToString(format, provider);
    }
    public readonly bool TryFormat(Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
      return ((Float<Float80>)this).TryFormat(dest, out charsWritten, format, provider);
    }
    public readonly override int GetHashCode() => ((Float<Float80>)this).GetHashCode();
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => ((Float<Float80>)this).Equals(obj);
    public readonly bool Equals(Float80 other) => ((Float<Float80>)this).Equals(other);
    public static Float80 Parse(ReadOnlySpan<char> value, IFormatProvider? provider = null)
    {
      TryParse(value, provider, out var r); return r;
    }
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out Float80 result)
    {
      var ok = Float<Float80>.TryParse(value, provider, out var r); result = r; return ok;
    }
    public static bool TryParse(ReadOnlySpan<char> value, NumberStyles style, IFormatProvider? provider, out Float80 result)
    {
      var ok = Float<Float80>.TryParse(value, style, provider, out var r); result = r; return ok;
    }

    public static implicit operator Float80(int value) => (Float<Float80>)value;
    public static implicit operator Float80(float value) => (Float<Float80>)value;
    public static implicit operator Float80(double value) => (Float<Float80>)value;
    public static explicit operator Float80(BigRational value) => (Float<Float80>)value;
    public static implicit operator BigRational(Float80 value) => (Float<Float80>)value;

    public static Float80 operator +(Float80 a) => a;
    public static Float80 operator -(Float80 a) => -(Float<Float80>)a;
    public static Float80 operator +(Float80 a, Float80 b) => (Float<Float80>)a + (Float<Float80>)b;
    public static Float80 operator -(Float80 a, Float80 b) => (Float<Float80>)a - (Float<Float80>)b;
    public static Float80 operator *(Float80 a, Float80 b) => (Float<Float80>)a * (Float<Float80>)b;
    public static Float80 operator /(Float80 a, Float80 b) => (Float<Float80>)a / (Float<Float80>)b;
    public static Float80 operator %(Float80 a, Float80 b) => (Float<Float80>)a % (Float<Float80>)b;
    public static Float80 operator ++(Float80 a) => (Float<Float80>)a + 1;
    public static Float80 operator --(Float80 a) => (Float<Float80>)a - 1;
    public static bool operator ==(Float80 a, Float80 b) => (Float<Float80>)a == (Float<Float80>)b;
    public static bool operator !=(Float80 a, Float80 b) => (Float<Float80>)a != (Float<Float80>)b;
    public static bool operator <=(Float80 a, Float80 b) => (Float<Float80>)a <= (Float<Float80>)b;
    public static bool operator >=(Float80 a, Float80 b) => (Float<Float80>)a >= (Float<Float80>)b;
    public static bool operator <(Float80 a, Float80 b) => (Float<Float80>)a < (Float<Float80>)b;
    public static bool operator >(Float80 a, Float80 b) => (Float<Float80>)a > (Float<Float80>)b;

    public static Float80 E => Float<Float80>.E;
    public static Float80 Pi => Float<Float80>.Pi;
    public static Float80 Tau => Float<Float80>.Tau;
    public static Float80 MinValue => Float<Float80>.MinValue;
    public static Float80 MaxValue => Float<Float80>.MaxValue;
    public static Float80 NaN => Float<Float80>.NaN;
    public static Float80 Epsilon => Float<Float80>.Epsilon;
    public static Float80 NegativeInfinity => Float<Float80>.NegativeInfinity;
    public static Float80 NegativeZero => Float<Float80>.NegativeZero;
    public static Float80 PositiveInfinity => Float<Float80>.PositiveInfinity;

    public static bool IsEvenInteger(Float80 a) => Float<Float80>.IsEvenInteger(a);
    public static bool IsInteger(Float80 a) => Float<Float80>.IsInteger(a);
    public static bool IsOddInteger(Float80 a) => Float<Float80>.IsOddInteger(a);
    public static bool IsPositive(Float80 a) => Float<Float80>.IsPositive(a);
    public static bool IsRealNumber(Float80 a) => Float<Float80>.IsRealNumber(a);
    public static bool IsFinite(Float80 a) => Float<Float80>.IsFinite(a);
    public static bool IsInfinity(Float80 a) => Float<Float80>.IsInfinity(a);
    public static bool IsNaN(Float80 a) => Float<Float80>.IsNaN(a);
    public static bool IsNegative(Float80 a) => Float<Float80>.IsNegative(a);
    public static bool IsNegativeInfinity(Float80 a) => Float<Float80>.IsNegativeInfinity(a);
    public static bool IsNormal(Float80 a) => Float<Float80>.IsNormal(a);
    public static bool IsPositiveInfinity(Float80 a) => Float<Float80>.IsPositiveInfinity(a);
    public static bool IsSubnormal(Float80 a) => Float<Float80>.IsSubnormal(a);
    public static bool IsPow2(Float80 a) => Float<Float80>.IsPow2(a);

    public static Float80 Abs(Float80 a) => Float<Float80>.Abs(a);
    public static Float80 Sqrt(Float80 a) => Float<Float80>.Sqrt(a);
    public static int Sign(Float80 a) => Float<Float80>.Sign(a);
    public static Float80 Truncate(Float80 a) => Float<Float80>.Truncate(a);
    public static Float80 Min(Float80 a, Float80 b) => Float<Float80>.Min(a, b);
    public static Float80 Max(Float80 a, Float80 b) => Float<Float80>.Max(a, b);
    public static Float80 Round(Float80 a, int digits = 0, MidpointRounding mode = MidpointRounding.ToEven) => Float<Float80>.Round(a, digits, mode);
    public static Float80 Floor(Float80 a) => Float<Float80>.Floor(a);
    public static Float80 Ceiling(Float80 a) => Float<Float80>.Ceiling(a);
    public static Float80 Pow(Float80 x, Float80 y) => Float<Float80>.Pow(x, y);
    public static Float80 MaxMagnitude(Float80 x, Float80 y) => Float<Float80>.MaxMagnitude(x, y);
    public static Float80 MaxMagnitudeNumber(Float80 x, Float80 y) => Float<Float80>.MaxMagnitudeNumber(x, y);
    public static Float80 MinMagnitude(Float80 x, Float80 y) => Float<Float80>.MinMagnitude(x, y);
    public static Float80 MinMagnitudeNumber(Float80 x, Float80 y) => Float<Float80>.MinMagnitudeNumber(x, y);
    public static Float80 ScaleB(Float80 x, int n) => Float<Float80>.ScaleB(x, n);
    public static Float80 Exp10(Float80 x) => Float<Float80>.Exp10(x);
    public static int ILogB(Float80 x) => Float<Float80>.ILogB(x);
    public static Float80 Atan2Pi(Float80 y, Float80 x) => Float<Float80>.Atan2Pi(y, x);
    public static Float80 Exp2(Float80 x) => Float<Float80>.Exp2(x);
    public static Float80 BitIncrement(Float80 x) => Float<Float80>.BitIncrement(x);
    public static Float80 BitDecrement(Float80 x) => Float<Float80>.BitDecrement(x);
    public static Float80 FusedMultiplyAdd(Float80 a, Float80 b, Float80 c) => Float<Float80>.FusedMultiplyAdd(a, b, c);
    public static Float80 Ieee754Remainder(Float80 a, Float80 b) => Float<Float80>.Ieee754Remainder(a, b);
    public static Float80 Pow2(Float80 x) => Float<Float80>.Pow2(x);
    public static Float80 Cbrt(Float80 x) => Float<Float80>.Cbrt(x);
    public static Float80 RootN(Float80 x, int n) => Float<Float80>.RootN(x, n);
    public static Float80 Hypot(Float80 x, Float80 y) => Float<Float80>.Hypot(x, y);
    public static Float80 Log2(Float80 x) => Float<Float80>.Log2(x);
    public static Float80 Log10(Float80 x) => Float<Float80>.Log10(x);
    public static Float80 Log(Float80 x) => Float<Float80>.Log(x);
    public static Float80 Log(Float80 x, Float80 newBase) => Float<Float80>.Log(x, newBase);
    public static Float80 Exp(Float80 x) => Float<Float80>.Exp(x);
    public static Float80 Sin(Float80 x) => Float<Float80>.Sin(x);
    public static Float80 Cos(Float80 x) => Float<Float80>.Cos(x);
    public static Float80 Tan(Float80 x) => Float<Float80>.Tan(x);
    public static Float80 Asin(Float80 x) => Float<Float80>.Asin(x);
    public static Float80 Acos(Float80 x) => Float<Float80>.Acos(x);
    public static Float80 Atan(Float80 x) => Float<Float80>.Atan(x);
    public static Float80 Atan2(Float80 y, Float80 x) => Float<Float80>.Atan2(y, x);
    public static Float80 Asinh(Float80 x) => Float<Float80>.Asinh(x);
    public static Float80 Acosh(Float80 x) => Float<Float80>.Acosh(x);
    public static Float80 Atanh(Float80 x) => Float<Float80>.Atanh(x);
    public static Float80 Sinh(Float80 x) => Float<Float80>.Asinh(x);
    public static Float80 Cosh(Float80 x) => Float<Float80>.Acosh(x);
    public static Float80 Tanh(Float80 x) => Float<Float80>.Tanh(x);
    public static Float80 SinPi(Float80 x) => Float<Float80>.SinPi(x);
    public static Float80 TanPi(Float80 x) => Float<Float80>.TanPi(x);
    public static Float80 AcosPi(Float80 x) => Float<Float80>.AcosPi(x);
    public static Float80 AsinPi(Float80 x) => Float<Float80>.AsinPi(x);
    public static Float80 AtanPi(Float80 x) => Float<Float80>.AtanPi(x);
    public static Float80 CosPi(Float80 x) => Float<Float80>.CosPi(x);
  }

  [StructLayout(LayoutKind.Sequential, Size = 16), Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float128 : IFloatType<Float128>
  {
    public override string ToString() => ToString(null);
    public readonly string ToString(string? format, IFormatProvider? provider = default)
    {
      return ((Float<Float128>)this).ToString(format, provider);
    }
    public readonly bool TryFormat(Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
      return ((Float<Float128>)this).TryFormat(dest, out charsWritten, format, provider);
    }
    public readonly override int GetHashCode() => ((Float<Float128>)this).GetHashCode();
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => ((Float<Float128>)this).Equals(obj);
    public readonly bool Equals(Float128 other) => ((Float<Float128>)this).Equals(other);
    public static Float128 Parse(ReadOnlySpan<char> value, IFormatProvider? provider = null)
    {
      TryParse(value, provider, out var r); return r;
    }
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out Float128 result)
    {
      var ok = Float<Float128>.TryParse(value, provider, out var r); result = r; return ok;
    }
    public static bool TryParse(ReadOnlySpan<char> value, NumberStyles style, IFormatProvider? provider, out Float128 result)
    {
      var ok = Float<Float128>.TryParse(value, style, provider, out var r); result = r; return ok;
    }

    public static implicit operator Float128(int value) => (Float<Float128>)value;
    public static implicit operator Float128(float value) => (Float<Float128>)value;
    public static implicit operator Float128(double value) => (Float<Float128>)value;
    public static implicit operator Float128(Float80 value) => Float<Float128>.Cast<Float<Float80>>((Float<Float80>)value);
    public static implicit operator Float128(decimal value) => (Float<Float128>)value;
    public static explicit operator Float128(BigRational value) => (Float<Float128>)value;

    public static explicit operator float(Float128 value) => (float)(Float<Float128>)value;
    public static explicit operator double(Float128 value) => (double)(Float<Float128>)value;
    public static explicit operator decimal(Float128 value) => (decimal)(Float<Float128>)value;
    public static implicit operator BigRational(Float128 value) => (Float<Float128>)value;

    public static Float128 operator +(Float128 a) => a;
    public static Float128 operator -(Float128 a) => -(Float<Float128>)a;
    public static Float128 operator +(Float128 a, Float128 b) => (Float<Float128>)a + (Float<Float128>)b;
    public static Float128 operator -(Float128 a, Float128 b) => (Float<Float128>)a - (Float<Float128>)b;
    public static Float128 operator *(Float128 a, Float128 b) => (Float<Float128>)a * (Float<Float128>)b;
    public static Float128 operator /(Float128 a, Float128 b) => (Float<Float128>)a / (Float<Float128>)b;
    public static Float128 operator %(Float128 a, Float128 b) => (Float<Float128>)a % (Float<Float128>)b;
    public static Float128 operator ++(Float128 a) => (Float<Float128>)a + 1;
    public static Float128 operator --(Float128 a) => (Float<Float128>)a - 1;
    public static bool operator ==(Float128 a, Float128 b) => (Float<Float128>)a == (Float<Float128>)b;
    public static bool operator !=(Float128 a, Float128 b) => (Float<Float128>)a != (Float<Float128>)b;
    public static bool operator <=(Float128 a, Float128 b) => (Float<Float128>)a <= (Float<Float128>)b;
    public static bool operator >=(Float128 a, Float128 b) => (Float<Float128>)a >= (Float<Float128>)b;
    public static bool operator <(Float128 a, Float128 b) => (Float<Float128>)a < (Float<Float128>)b;
    public static bool operator >(Float128 a, Float128 b) => (Float<Float128>)a > (Float<Float128>)b;

    public static Float128 E => Float<Float128>.E;
    public static Float128 Pi => Float<Float128>.Pi;
    public static Float128 Tau => Float<Float128>.Tau;
    public static Float128 MinValue => Float<Float128>.MinValue;
    public static Float128 MaxValue => Float<Float128>.MaxValue;
    public static Float128 NaN => Float<Float128>.NaN;
    public static Float128 Epsilon => Float<Float128>.Epsilon;
    public static Float128 NegativeInfinity => Float<Float128>.NegativeInfinity;
    public static Float128 NegativeZero => Float<Float128>.NegativeZero;
    public static Float128 PositiveInfinity => Float<Float128>.PositiveInfinity;

    public static bool IsEvenInteger(Float128 a) => Float<Float128>.IsEvenInteger(a);
    public static bool IsInteger(Float128 a) => Float<Float128>.IsInteger(a);
    public static bool IsOddInteger(Float128 a) => Float<Float128>.IsOddInteger(a);
    public static bool IsPositive(Float128 a) => Float<Float128>.IsPositive(a);
    public static bool IsRealNumber(Float128 a) => Float<Float128>.IsRealNumber(a);
    public static bool IsFinite(Float128 a) => Float<Float128>.IsFinite(a);
    public static bool IsInfinity(Float128 a) => Float<Float128>.IsInfinity(a);
    public static bool IsNaN(Float128 a) => Float<Float128>.IsNaN(a);
    public static bool IsNegative(Float128 a) => Float<Float128>.IsNegative(a);
    public static bool IsNegativeInfinity(Float128 a) => Float<Float128>.IsNegativeInfinity(a);
    public static bool IsNormal(Float128 a) => Float<Float128>.IsNormal(a);
    public static bool IsPositiveInfinity(Float128 a) => Float<Float128>.IsPositiveInfinity(a);
    public static bool IsSubnormal(Float128 a) => Float<Float128>.IsSubnormal(a);
    public static bool IsPow2(Float128 a) => Float<Float128>.IsPow2(a);

    public static Float128 Abs(Float128 a) => Float<Float128>.Abs(a);
    public static Float128 Sqrt(Float128 a) => Float<Float128>.Sqrt(a);
    public static int Sign(Float128 a) => Float<Float128>.Sign(a);
    public static Float128 Truncate(Float128 a) => Float<Float128>.Truncate(a);
    public static Float128 Min(Float128 a, Float128 b) => Float<Float128>.Min(a, b);
    public static Float128 Max(Float128 a, Float128 b) => Float<Float128>.Max(a, b);
    public static Float128 Round(Float128 a, int digits = 0, MidpointRounding mode = MidpointRounding.ToEven) => Float<Float128>.Round(a, digits, mode);
    public static Float128 Floor(Float128 a) => Float<Float128>.Floor(a);
    public static Float128 Ceiling(Float128 a) => Float<Float128>.Ceiling(a);
    public static Float128 Pow(Float128 x, Float128 y) => Float<Float128>.Pow(x, y);
    public static Float128 MaxMagnitude(Float128 x, Float128 y) => Float<Float128>.MaxMagnitude(x, y);
    public static Float128 MaxMagnitudeNumber(Float128 x, Float128 y) => Float<Float128>.MaxMagnitudeNumber(x, y);
    public static Float128 MinMagnitude(Float128 x, Float128 y) => Float<Float128>.MinMagnitude(x, y);
    public static Float128 MinMagnitudeNumber(Float128 x, Float128 y) => Float<Float128>.MinMagnitudeNumber(x, y);
    public static Float128 ScaleB(Float128 x, int n) => Float<Float128>.ScaleB(x, n);
    public static Float128 Exp10(Float128 x) => Float<Float128>.Exp10(x);
    public static int ILogB(Float128 x) => Float<Float128>.ILogB(x);
    public static Float128 Atan2Pi(Float128 y, Float128 x) => Float<Float128>.Atan2Pi(y, x);
    public static Float128 Exp2(Float128 x) => Float<Float128>.Exp2(x);
    public static Float128 BitIncrement(Float128 x) => Float<Float128>.BitIncrement(x);
    public static Float128 BitDecrement(Float128 x) => Float<Float128>.BitDecrement(x);
    public static Float128 FusedMultiplyAdd(Float128 a, Float128 b, Float128 c) => Float<Float128>.FusedMultiplyAdd(a, b, c);
    public static Float128 Ieee754Remainder(Float128 a, Float128 b) => Float<Float128>.Ieee754Remainder(a, b);
    public static Float128 Pow2(Float128 x) => Float<Float128>.Pow2(x);
    public static Float128 Cbrt(Float128 x) => Float<Float128>.Cbrt(x);
    public static Float128 RootN(Float128 x, int n) => Float<Float128>.RootN(x, n);
    public static Float128 Hypot(Float128 x, Float128 y) => Float<Float128>.Hypot(x, y);
    public static Float128 Log2(Float128 x) => Float<Float128>.Log2(x);
    public static Float128 Log10(Float128 x) => Float<Float128>.Log10(x);
    public static Float128 Log(Float128 x) => Float<Float128>.Log(x);
    public static Float128 Log(Float128 x, Float128 newBase) => Float<Float128>.Log(x, newBase);
    public static Float128 Exp(Float128 x) => Float<Float128>.Exp(x);
    public static Float128 Sin(Float128 x) => Float<Float128>.Sin(x);
    public static Float128 Cos(Float128 x) => Float<Float128>.Cos(x);
    public static Float128 Tan(Float128 x) => Float<Float128>.Tan(x);
    public static Float128 Asin(Float128 x) => Float<Float128>.Asin(x);
    public static Float128 Acos(Float128 x) => Float<Float128>.Acos(x);
    public static Float128 Atan(Float128 x) => Float<Float128>.Atan(x);
    public static Float128 Atan2(Float128 y, Float128 x) => Float<Float128>.Atan2(y, x);
    public static Float128 Asinh(Float128 x) => Float<Float128>.Asinh(x);
    public static Float128 Acosh(Float128 x) => Float<Float128>.Acosh(x);
    public static Float128 Atanh(Float128 x) => Float<Float128>.Atanh(x);
    public static Float128 Sinh(Float128 x) => Float<Float128>.Asinh(x);
    public static Float128 Cosh(Float128 x) => Float<Float128>.Acosh(x);
    public static Float128 Tanh(Float128 x) => Float<Float128>.Tanh(x);
    public static Float128 SinPi(Float128 x) => Float<Float128>.SinPi(x);
    public static Float128 TanPi(Float128 x) => Float<Float128>.TanPi(x);
    public static Float128 AcosPi(Float128 x) => Float<Float128>.AcosPi(x);
    public static Float128 AsinPi(Float128 x) => Float<Float128>.AsinPi(x);
    public static Float128 AtanPi(Float128 x) => Float<Float128>.AtanPi(x);
    public static Float128 CosPi(Float128 x) => Float<Float128>.CosPi(x);
  }
#endif
}
