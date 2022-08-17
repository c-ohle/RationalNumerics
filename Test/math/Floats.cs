
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Globalization;

namespace System.Numerics.Rational
{
  /// <summary>
  /// It's just a double - as Float template test
  /// </summary>
  public readonly partial struct Float64 : IComparable<Float64>, IEquatable<Float64>, IComparable, ISpanFormattable
  {
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public readonly bool Equals(Float64 b) => BigRational.Float<UInt64>.Equals(this.p, b.p);
    public readonly int CompareTo(Float64 b) => BigRational.Float<UInt64>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float64 b ? BigRational.Float<UInt64>.Compare(this.p, b.p) : throw new ArgumentException();

    public static implicit operator int(Float64 value) => (int)value.p;
    public static implicit operator Half(Float64 value) => (Half)value.p;
    public static implicit operator float(Float64 value) => (float)value.p;
    public static implicit operator double(Float64 value) => (double)value.p;
    public static implicit operator Float128(Float64 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float64 value) => Float256.Cast(value.p);
    public static implicit operator BigRational(Float64 value) => (BigRational)value.p;

    public static implicit operator Float64(int value) => new Float64((BigRational.Float<UInt64>)value);
    public static implicit operator Float64(Half value) => new Float64((BigRational.Float<UInt64>)value);
    public static implicit operator Float64(float value) => new Float64((BigRational.Float<UInt64>)value);
    public static implicit operator Float64(double value) => new Float64((BigRational.Float<UInt64>)value);
    public static explicit operator Float64(Float128 value) => new Float64(Float128.Cast<UInt64>(value));
    public static explicit operator Float64(Float256 value) => new Float64(Float256.Cast<UInt64>(value));
    public static explicit operator Float64(BigRational value) => new Float64((BigRational.Float<UInt64>)value);

    public static Float64 operator +(Float64 a, Float64 b) => new Float64(a.p + b.p);
    public static Float64 operator -(Float64 a, Float64 b) => new Float64(a.p - b.p);
    public static Float64 operator *(Float64 a, Float64 b) => new Float64(a.p * b.p);
    public static Float64 operator /(Float64 a, Float64 b) => new Float64(a.p / b.p);
    public static Float64 operator %(Float64 a, Float64 b) => new Float64(a.p % b.p);

    public static Float64 Truncate(Float64 x) => new Float64(BigRational.Float<UInt64>.Truncate(x.p));

    public static Float64 Cast<T>(BigRational.Float<T> a) where T : unmanaged => new Float64(BigRational.Float<T>.Cast<UInt64>(a));
    public static BigRational.Float<T> Cast<T>(Float64 a) where T : unmanaged => BigRational.Float<UInt64>.Cast<T>(a.p);

    public static Float64 MinValue => new Float64(BigRational.Float<UInt64>.MinValue);
    public static Float64 MaxValue => new Float64(BigRational.Float<UInt64>.MaxValue);

    private Float64(BigRational.Float<UInt64> p) => this.p = p;
    private readonly BigRational.Float<UInt64> p;
  }

  /// <summary>
  /// It's just for test for external new floating types from BigRational.Float
  /// </summary>
  public readonly struct Float96 : IComparable<Float96>, IEquatable<Float96>
  {
    public readonly override string ToString() => p.ToString();
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float96 b) => BigRational.Float<UInt96>.Equals(this.p, b.p);
    public readonly int CompareTo(Float96 b) => BigRational.Float<UInt96>.Compare(this.p, b.p);

    public static implicit operator int(Float96 value) => (int)value.p;
    public static implicit operator Half(Float96 value) => (Half)value.p;
    public static implicit operator float(Float96 value) => (float)value.p;
    public static implicit operator double(Float96 value) => (double)value.p;
    public static implicit operator Float64(Float96 value) => Float64.Cast(value.p);
    public static implicit operator Float128(Float96 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float96 value) => Float256.Cast(value.p);

    public static implicit operator Float96(int value) => new Float96((BigRational.Float<UInt96>)value);
    public static implicit operator Float96(Half value) => new Float96((BigRational.Float<UInt96>)value);
    public static implicit operator Float96(float value) => new Float96((BigRational.Float<UInt96>)value);
    public static implicit operator Float96(double value) => new Float96((BigRational.Float<UInt96>)value);
    public static implicit operator Float96(Float64 value) => new Float96(Float64.Cast<UInt96>(value));
    public static explicit operator Float96(Float128 value) => new Float96(Float128.Cast<UInt96>(value));
    public static explicit operator Float96(Float256 value) => new Float96(Float256.Cast<UInt96>(value));
    public static explicit operator Float96(BigRational value) => new Float96((BigRational.Float<UInt96>)value);

    public static Float96 operator ++(Float96 a) => new Float96(a.p + +1);
    public static Float96 operator --(Float96 a) => new Float96(a.p + -1);
    public static Float96 operator +(Float96 a, Float96 b) => new Float96(a.p + b.p);
    public static Float96 operator -(Float96 a, Float96 b) => new Float96(a.p - b.p);
    public static Float96 operator *(Float96 a, Float96 b) => new Float96(a.p * b.p);
    public static Float96 operator /(Float96 a, Float96 b) => new Float96(a.p / b.p);
    public static Float96 operator %(Float96 a, Float96 b) => new Float96(a.p % b.p);

    public static Float96 Truncate(Float96 a) => new Float96(BigRational.Float<UInt96>.Truncate(a.p));

    public static Float96 Cast<T>(BigRational.Float<T> a) where T : unmanaged => new Float96(BigRational.Float<T>.Cast<UInt96>(a));
    public static BigRational.Float<T> Cast<T>(Float96 a) where T : unmanaged => BigRational.Float<UInt96>.Cast<T>(a.p);

    public static Float96 MinValue => new Float96(BigRational.Float<UInt96>.MinValue);
    public static Float96 MaxValue => new Float96(BigRational.Float<UInt96>.MaxValue);

    Float96(BigRational.Float<UInt96> p) => this.p = p;
    private readonly BigRational.Float<UInt96> p;
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
    private readonly struct UInt96 { readonly UInt32 high, low; }
  }

#if false //NET7_0
  partial struct Float64 : IBinaryFloatingPointIeee754<Float64>, IMinMaxValue<Float64>
  {
    static Float64 IBinaryNumber<Float64>.AllBitsSet => throw new NotImplementedException();
    static Float64 IFloatingPointIeee754<Float64>.Epsilon => throw new NotImplementedException();
    static Float64 IFloatingPointIeee754<Float64>.NaN => throw new NotImplementedException();
    static Float64 IFloatingPointIeee754<Float64>.NegativeInfinity => throw new NotImplementedException();
    static Float64 IFloatingPointIeee754<Float64>.NegativeZero => throw new NotImplementedException();
    static Float64 IFloatingPointIeee754<Float64>.PositiveInfinity => throw new NotImplementedException();
    static Float64 ISignedNumber<Float64>.NegativeOne => throw new NotImplementedException();
    static Float64 IFloatingPointConstants<Float64>.E => throw new NotImplementedException();
    static Float64 IFloatingPointConstants<Float64>.Pi => throw new NotImplementedException();
    static Float64 IFloatingPointConstants<Float64>.Tau => throw new NotImplementedException();
    static Float64 INumberBase<Float64>.One => throw new NotImplementedException();
    static int INumberBase<Float64>.Radix => throw new NotImplementedException();
    static Float64 INumberBase<Float64>.Zero => throw new NotImplementedException();
    static Float64 IAdditiveIdentity<Float64, Float64>.AdditiveIdentity => throw new NotImplementedException();
    static Float64 IMultiplicativeIdentity<Float64, Float64>.MultiplicativeIdentity => throw new NotImplementedException();
    static Float64 INumberBase<Float64>.Abs(Float64 value)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.Acos(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IHyperbolicFunctions<Float64>.Acosh(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.AcosPi(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.Asin(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IHyperbolicFunctions<Float64>.Asinh(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.AsinPi(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.Atan(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IFloatingPointIeee754<Float64>.Atan2(Float64 y, Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IFloatingPointIeee754<Float64>.Atan2Pi(Float64 y, Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IHyperbolicFunctions<Float64>.Atanh(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.AtanPi(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IFloatingPointIeee754<Float64>.BitDecrement(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IFloatingPointIeee754<Float64>.BitIncrement(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IRootFunctions<Float64>.Cbrt(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.Cos(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IHyperbolicFunctions<Float64>.Cosh(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.CosPi(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IExponentialFunctions<Float64>.Exp(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IExponentialFunctions<Float64>.Exp10(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IExponentialFunctions<Float64>.Exp2(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IFloatingPointIeee754<Float64>.FusedMultiplyAdd(Float64 left, Float64 right, Float64 addend)
    {
      throw new NotImplementedException();
    }
    static Float64 IRootFunctions<Float64>.Hypot(Float64 x, Float64 y)
    {
      throw new NotImplementedException();
    }
    static Float64 IFloatingPointIeee754<Float64>.Ieee754Remainder(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static int IFloatingPointIeee754<Float64>.ILogB(Float64 x)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsCanonical(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsComplexNumber(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsEvenInteger(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsFinite(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsImaginaryNumber(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsInfinity(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsInteger(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsNaN(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsNegative(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsNegativeInfinity(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsNormal(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsOddInteger(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsPositive(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsPositiveInfinity(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool IBinaryNumber<Float64>.IsPow2(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsRealNumber(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsSubnormal(Float64 value)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.IsZero(Float64 value)
    {
      throw new NotImplementedException();
    }
    static Float64 ILogarithmicFunctions<Float64>.Log(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ILogarithmicFunctions<Float64>.Log(Float64 x, Float64 newBase)
    {
      throw new NotImplementedException();
    }
    static Float64 ILogarithmicFunctions<Float64>.Log10(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IBinaryNumber<Float64>.Log2(Float64 value)
    {
      throw new NotImplementedException();
    }
    static Float64 ILogarithmicFunctions<Float64>.Log2(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 INumberBase<Float64>.MaxMagnitude(Float64 x, Float64 y)
    {
      throw new NotImplementedException();
    }
    static Float64 INumberBase<Float64>.MaxMagnitudeNumber(Float64 x, Float64 y)
    {
      throw new NotImplementedException();
    }
    static Float64 INumberBase<Float64>.MinMagnitude(Float64 x, Float64 y)
    {
      throw new NotImplementedException();
    }
    static Float64 INumberBase<Float64>.MinMagnitudeNumber(Float64 x, Float64 y)
    {
      throw new NotImplementedException();
    }
    static Float64 INumberBase<Float64>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
      throw new NotImplementedException();
    }
    static Float64 INumberBase<Float64>.Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
      throw new NotImplementedException();
    }
    static Float64 ISpanParsable<Float64>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
      throw new NotImplementedException();
    }
    static Float64 IParsable<Float64>.Parse(string s, IFormatProvider? provider)
    {
      throw new NotImplementedException();
    }
    static Float64 IPowerFunctions<Float64>.Pow(Float64 x, Float64 y)
    {
      throw new NotImplementedException();
    }
    static Float64 IRootFunctions<Float64>.RootN(Float64 x, int n)
    {
      throw new NotImplementedException();
    }
    static Float64 IFloatingPoint<Float64>.Round(Float64 x, int digits, MidpointRounding mode)
    {
      throw new NotImplementedException();
    }
    static Float64 IFloatingPointIeee754<Float64>.ScaleB(Float64 x, int n)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.Sin(Float64 x)
    {
      throw new NotImplementedException();
    }
    static (Float64 Sin, Float64 Cos) ITrigonometricFunctions<Float64>.SinCos(Float64 x)
    {
      throw new NotImplementedException();
    }
    static (Float64 SinPi, Float64 CosPi) ITrigonometricFunctions<Float64>.SinCosPi(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IHyperbolicFunctions<Float64>.Sinh(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.SinPi(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IRootFunctions<Float64>.Sqrt(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.Tan(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IHyperbolicFunctions<Float64>.Tanh(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 ITrigonometricFunctions<Float64>.TanPi(Float64 x)
    {
      throw new NotImplementedException();
    }
    static Float64 IFloatingPoint<Float64>.Truncate(Float64 x)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.TryConvertFromChecked<TOther>(TOther value, out Float64 result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.TryConvertFromSaturating<TOther>(TOther value, out Float64 result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.TryConvertFromTruncating<TOther>(TOther value, out Float64 result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.TryConvertToChecked<TOther>(Float64 value, out TOther result) where TOther : default
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.TryConvertToSaturating<TOther>(Float64 value, out TOther result) where TOther : default
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.TryConvertToTruncating<TOther>(Float64 value, out TOther result) where TOther : default
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Float64 result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<Float64>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Float64 result)
    {
      throw new NotImplementedException();
    }
    static bool ISpanParsable<Float64>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Float64 result)
    {
      throw new NotImplementedException();
    }
    static bool IParsable<Float64>.TryParse(string? s, IFormatProvider? provider, out Float64 result)
    {
      throw new NotImplementedException();
    }
    int IComparable.CompareTo(object? obj)
    {
      throw new NotImplementedException();
    }
    int IComparable<Float64>.CompareTo(Float64 other)
    {
      throw new NotImplementedException();
    }
    bool IEquatable<Float64>.Equals(Float64 other)
    {
      throw new NotImplementedException();
    }
    int IFloatingPoint<Float64>.GetExponentByteCount()
    {
      throw new NotImplementedException();
    }
    int IFloatingPoint<Float64>.GetExponentShortestBitLength()
    {
      throw new NotImplementedException();
    }
    int IFloatingPoint<Float64>.GetSignificandBitLength()
    {
      throw new NotImplementedException();
    }
    int IFloatingPoint<Float64>.GetSignificandByteCount()
    {
      throw new NotImplementedException();
    }
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
    {
      throw new NotImplementedException();
    }
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
      throw new NotImplementedException();
    }
    bool IFloatingPoint<Float64>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten)
    {
      throw new NotImplementedException();
    }
    bool IFloatingPoint<Float64>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      throw new NotImplementedException();
    }
    bool IFloatingPoint<Float64>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten)
    {
      throw new NotImplementedException();
    }
    bool IFloatingPoint<Float64>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      throw new NotImplementedException();
    }
    static Float64 IUnaryPlusOperators<Float64, Float64>.operator +(Float64 value)
    {
      throw new NotImplementedException();
    }
    static Float64 IAdditionOperators<Float64, Float64, Float64>.operator +(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static Float64 IUnaryNegationOperators<Float64, Float64>.operator -(Float64 value)
    {
      throw new NotImplementedException();
    }
    static Float64 ISubtractionOperators<Float64, Float64, Float64>.operator -(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static Float64 IBitwiseOperators<Float64, Float64, Float64>.operator ~(Float64 value)
    {
      throw new NotImplementedException();
    }
    static Float64 IIncrementOperators<Float64>.operator ++(Float64 value)
    {
      throw new NotImplementedException();
    }
    static Float64 IDecrementOperators<Float64>.operator --(Float64 value)
    {
      throw new NotImplementedException();
    }
    static Float64 IMultiplyOperators<Float64, Float64, Float64>.operator *(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static Float64 IDivisionOperators<Float64, Float64, Float64>.operator /(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static Float64 IModulusOperators<Float64, Float64, Float64>.operator %(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static Float64 IBitwiseOperators<Float64, Float64, Float64>.operator &(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static Float64 IBitwiseOperators<Float64, Float64, Float64>.operator |(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static Float64 IBitwiseOperators<Float64, Float64, Float64>.operator ^(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static bool IEqualityOperators<Float64, Float64, bool>.operator ==(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static bool IEqualityOperators<Float64, Float64, bool>.operator !=(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static bool IComparisonOperators<Float64, Float64, bool>.operator <(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static bool IComparisonOperators<Float64, Float64, bool>.operator >(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static bool IComparisonOperators<Float64, Float64, bool>.operator <=(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
    static bool IComparisonOperators<Float64, Float64, bool>.operator >=(Float64 left, Float64 right)
    {
      throw new NotImplementedException();
    }
  }
#endif

}
