using System.Globalization;

#pragma warning disable CS1591 //xml comments

namespace System.Numerics.Generic
{
#if NET7_0 //_OR_GREATER

  /// <summary>
  /// Interface for <see cref="Float{T}"/> types to provide <see cref="IBinaryFloatingPointIeee754{TSelf}"/> and <see cref="INumber{TSelf}"/> support. 
  /// </summary>
  /// <remarks>
  /// <see cref="Float{T}"/> does not implement <see cref="INumber{TSelf}"/> interfaces itself, 
  /// since host types like <see cref="Float128"/> would not inherit the functionality.<br/> 
  /// But this interface allows to extend final <see cref="Float{T}"/> type constructions with <see cref="INumber{TSelf}"/> support from top.
  /// </remarks>
  /// <typeparam name="T">A unmanaged <see cref="Float{T}"/> type.</typeparam>
  public unsafe interface IFloat<T> : IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> where T : unmanaged, IFloat<T>
  {
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => Cast<T>((T)this).ToString(format, formatProvider);
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => Cast<T>((T)this).TryFormat(destination, out charsWritten, format, provider);
    int IComparable.CompareTo(object? obj) => Cast<T>((T)this).CompareTo(obj);
    int IComparable<T>.CompareTo(T other) => Cast<T>((T)this).CompareTo(other);
    bool IEquatable<T>.Equals(T other) => Cast<T>((T)this).Equals(other);
    static Float<T> Cast<U>(U u) where U : unmanaged, IFloat<U> => *(Float<T>*)&u;

    static T IParsable<T>.Parse(string value, IFormatProvider? provider) { var t = Float<T>.Parse(value, provider); return *(T*)&t; }
    static bool IParsable<T>.TryParse(string? value, IFormatProvider? provider, out T result) { var t = Float<T>.TryParse(value, provider, out var r); result = *(T*)&r; return t; }
    static T ISpanParsable<T>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) { var t = Float<T>.TryParse(s, provider, out var r); return *(T*)&r; }
    static bool ISpanParsable<T>.TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out T result) { var t = Float<T>.TryParse(value, provider, out var r); result = *(T*)&r; return t; }
    static T INumberBase<T>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) { var t = Float<T>.TryParse(s, style, provider, out var r); return *(T*)&r; }
    static T INumberBase<T>.Parse(string s, NumberStyles style, IFormatProvider? provider) { var t = Float<T>.TryParse(s, style, provider, out var r); return *(T*)&r; }
    static bool INumberBase<T>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out T result) { var t = Float<T>.TryParse(s, style, provider, out var r); result = *(T*)&r; return t; }
    static bool INumberBase<T>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out T result) { var t = Float<T>.TryParse(s, style, provider, out var r); result = *(T*)&r; return t; }

    static T IIncrementOperators<T>.operator ++(T a) { *(Float<T>*)&a = ++(*(Float<T>*)&a); return *(T*)&a; }
    static T IDecrementOperators<T>.operator --(T a) { *(Float<T>*)&a = --(*(Float<T>*)&a); return *(T*)&a; }
    static T IUnaryPlusOperators<T, T>.operator +(T a) { *(Float<T>*)&a = +(*(Float<T>*)&a); return *(T*)&a; }
    static T IUnaryNegationOperators<T, T>.operator -(T a) { *(Float<T>*)&a = -(*(Float<T>*)&a); return *(T*)&a; }
    static T IAdditionOperators<T, T, T>.operator +(T a, T b) { *(Float<T>*)&a = *(Float<T>*)&a + *(Float<T>*)&b; return *(T*)&a; }
    static T ISubtractionOperators<T, T, T>.operator -(T a, T b) { *(Float<T>*)&a = *(Float<T>*)&a - *(Float<T>*)&b; return *(T*)&a; }
    static T IMultiplyOperators<T, T, T>.operator *(T a, T b) { *(Float<T>*)&a = *(Float<T>*)&a * *(Float<T>*)&b; return *(T*)&a; }
    static T IDivisionOperators<T, T, T>.operator /(T a, T b) { *(Float<T>*)&a = *(Float<T>*)&a / *(Float<T>*)&b; return *(T*)&a; }
    static T IModulusOperators<T, T, T>.operator %(T a, T b) { *(Float<T>*)&a = *(Float<T>*)&a % *(Float<T>*)&b; return *(T*)&a; }

    static bool IEqualityOperators<T, T, bool>.operator ==(T a, T b) => *(Float<T>*)&a == *(Float<T>*)&b;
    static bool IEqualityOperators<T, T, bool>.operator !=(T a, T b) => *(Float<T>*)&a != *(Float<T>*)&b;
    static bool IComparisonOperators<T, T, bool>.operator <(T a, T b) => *(Float<T>*)&a < *(Float<T>*)&b;
    static bool IComparisonOperators<T, T, bool>.operator >(T a, T b) => *(Float<T>*)&a > *(Float<T>*)&b;
    static bool IComparisonOperators<T, T, bool>.operator <=(T a, T b) => *(Float<T>*)&a <= *(Float<T>*)&b;
    static bool IComparisonOperators<T, T, bool>.operator >=(T a, T b) => *(Float<T>*)&a >= *(Float<T>*)&b;

    static T IBitwiseOperators<T, T, T>.operator ~(T a) { *(Float<T>*)&a = Float<T>.bcmp(*(Float<T>*)&a); return *(T*)&a; }
    static T IBitwiseOperators<T, T, T>.operator &(T a, T b) { *(Float<T>*)&a = Float<T>.band(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }
    static T IBitwiseOperators<T, T, T>.operator |(T a, T b) { *(Float<T>*)&a = Float<T>.bor(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }
    static T IBitwiseOperators<T, T, T>.operator ^(T a, T b) { *(Float<T>*)&a = Float<T>.bxor(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }

    static T IFloatingPointConstants<T>.E { get { var t = Float<T>.E; return *(T*)&t; } }
    static T IFloatingPointConstants<T>.Pi { get { var t = Float<T>.Pi; return *(T*)&t; } }
    static T IFloatingPointConstants<T>.Tau { get { var t = Float<T>.Tau; return *(T*)&t; } }
    static T IMinMaxValue<T>.MaxValue { get { var t = Float<T>.MaxValue; return *(T*)&t; } }
    static T IMinMaxValue<T>.MinValue { get { var t = Float<T>.MaxValue; return *(T*)&t; } }

    static T IBinaryNumber<T>.AllBitsSet { get { var t = Float<T>.AllBitsSet; return *(T*)&t; } }
    static T IFloatingPointIeee754<T>.Epsilon { get { var t = Float<T>.Epsilon; return *(T*)&t; } }
    static T IFloatingPointIeee754<T>.NaN { get { var t = Float<T>.NaN; return *(T*)&t; } }
    static T IFloatingPointIeee754<T>.NegativeInfinity { get { var t = Float<T>.NegativeInfinity; return *(T*)&t; } }
    static T IFloatingPointIeee754<T>.NegativeZero { get { var t = Float<T>.NegativeZero; return *(T*)&t; } }
    static T IFloatingPointIeee754<T>.PositiveInfinity { get { var t = Float<T>.PositiveInfinity; return *(T*)&t; } }
    
    static T ISignedNumber<T>.NegativeOne { get { var t = (Float<T>)(sbyte)(-1); return *(T*)&t; } }
    static T IAdditiveIdentity<T, T>.AdditiveIdentity => default;
    static T IMultiplicativeIdentity<T, T>.MultiplicativeIdentity { get { var t = (Float<T>)(sbyte)(1); return *(T*)&t; } }

    static int INumberBase<T>.Radix => 2;
    static bool INumberBase<T>.IsCanonical(T a) => true;
    static bool INumberBase<T>.IsComplexNumber(T a) => false;
    static bool INumberBase<T>.IsImaginaryNumber(T a) => false;
    static T INumberBase<T>.One { get { var t = (Float<T>)(sbyte)(1); return *(T*)&t; } }
    static T INumberBase<T>.Zero => default;
    static bool INumberBase<T>.IsEvenInteger(T a) => Float<T>.IsEvenInteger(*(Float<T>*)&a);
    static bool INumberBase<T>.IsFinite(T a) => Float<T>.IsFinite(*(Float<T>*)&a);
    static bool INumberBase<T>.IsInfinity(T a) => Float<T>.IsInfinity(*(Float<T>*)&a);
    static bool INumberBase<T>.IsInteger(T a) => Float<T>.IsInteger(*(Float<T>*)&a);
    static bool INumberBase<T>.IsNaN(T a) => Float<T>.IsNaN(*(Float<T>*)&a);
    static bool INumberBase<T>.IsNegative(T a) => Float<T>.IsNegative(*(Float<T>*)&a);
    static bool INumberBase<T>.IsNegativeInfinity(T a) => Float<T>.IsNegativeInfinity(*(Float<T>*)&a);
    static bool INumberBase<T>.IsNormal(T a) => Float<T>.IsNormal(*(Float<T>*)&a);
    static bool INumberBase<T>.IsOddInteger(T a) => Float<T>.IsOddInteger(*(Float<T>*)&a);
    static bool INumberBase<T>.IsPositive(T a) => Float<T>.IsPositive(*(Float<T>*)&a);
    static bool INumberBase<T>.IsPositiveInfinity(T a) => Float<T>.IsPositiveInfinity(*(Float<T>*)&a);
    static bool INumberBase<T>.IsRealNumber(T a) => Float<T>.IsRealNumber(*(Float<T>*)&a);
    static bool INumberBase<T>.IsSubnormal(T a) => Float<T>.IsSubnormal(*(Float<T>*)&a);
    static bool INumberBase<T>.IsZero(T a) => Float<T>.IsZero(*(Float<T>*)&a);
    static T INumberBase<T>.Abs(T a) { *(Float<T>*)&a = Float<T>.Abs(*(Float<T>*)&a); return *(T*)&a; }

    static T IPowerFunctions<T>.Pow(T a, T b) { *(Float<T>*)&a = Float<T>.Pow(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }
    static T IRootFunctions<T>.RootN(T a, int n) { *(Float<T>*)&a = Float<T>.RootN(*(Float<T>*)&a, n); return *(T*)&a; }

    static T IRootFunctions<T>.Sqrt(T a) { *(Float<T>*)&a = Float<T>.Sqrt(*(Float<T>*)&a); return *(T*)&a; }
    static T IRootFunctions<T>.Cbrt(T a) { *(Float<T>*)&a = Float<T>.Cbrt(*(Float<T>*)&a); return *(T*)&a; }
    static T IRootFunctions<T>.Hypot(T a, T b) { *(Float<T>*)&a = Float<T>.Hypot(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }

    static T ILogarithmicFunctions<T>.Log(T a) { *(Float<T>*)&a = Float<T>.Log(*(Float<T>*)&a); return *(T*)&a; }
    static T ILogarithmicFunctions<T>.Log(T a, T newBase) { *(Float<T>*)&a = Float<T>.Log(*(Float<T>*)&a, *(Float<T>*)&newBase); return *(T*)&a; }
    static T ILogarithmicFunctions<T>.Log10(T a) { *(Float<T>*)&a = Float<T>.Log10(*(Float<T>*)&a); return *(T*)&a; }
    static T ILogarithmicFunctions<T>.Log2(T a) { *(Float<T>*)&a = Float<T>.Log2(*(Float<T>*)&a); return *(T*)&a; }

    static T IExponentialFunctions<T>.Exp(T a) { *(Float<T>*)&a = Float<T>.Exp(*(Float<T>*)&a); return *(T*)&a; }
    static T IExponentialFunctions<T>.Exp10(T a) { *(Float<T>*)&a = Float<T>.Exp10(*(Float<T>*)&a); return *(T*)&a; }
    static T IExponentialFunctions<T>.Exp2(T a) { *(Float<T>*)&a = Float<T>.Exp2(*(Float<T>*)&a); return *(T*)&a; }

    static T ITrigonometricFunctions<T>.Sin(T a) { *(Float<T>*)&a = Float<T>.Sin(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.Cos(T a) { *(Float<T>*)&a = Float<T>.Cos(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.Acos(T a) { *(Float<T>*)&a = Float<T>.Acos(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.AcosPi(T a) { *(Float<T>*)&a = Float<T>.AcosPi(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.Asin(T a) { *(Float<T>*)&a = Float<T>.Asin(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.AsinPi(T a) { *(Float<T>*)&a = Float<T>.AsinPi(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.Atan(T a) { *(Float<T>*)&a = Float<T>.Atan(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.AtanPi(T a) { *(Float<T>*)&a = Float<T>.AtanPi(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.CosPi(T a) { *(Float<T>*)&a = Float<T>.CosPi(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.Tan(T a) { *(Float<T>*)&a = Float<T>.Tan(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.TanPi(T a) { *(Float<T>*)&a = Float<T>.TanPi(*(Float<T>*)&a); return *(T*)&a; }
    static T ITrigonometricFunctions<T>.SinPi(T a) { *(Float<T>*)&a = Float<T>.SinPi(*(Float<T>*)&a); return *(T*)&a; }
    static (T Sin, T Cos) ITrigonometricFunctions<T>.SinCos(T a) { var t = Float<T>.SinCos(*(Float<T>*)&a); return (*(T*)&t.Sin, *(T*)&t.Cos); }
    static (T SinPi, T CosPi) ITrigonometricFunctions<T>.SinCosPi(T a) { var t = Float<T>.SinCosPi(*(Float<T>*)&a); return (*(T*)&t.SinPi, *(T*)&t.CosPi); }

    static T IHyperbolicFunctions<T>.Acosh(T a) { *(Float<T>*)&a = Float<T>.Acosh(*(Float<T>*)&a); return *(T*)&a; }
    static T IHyperbolicFunctions<T>.Asinh(T a) { *(Float<T>*)&a = Float<T>.Asinh(*(Float<T>*)&a); return *(T*)&a; }
    static T IHyperbolicFunctions<T>.Atanh(T a) { *(Float<T>*)&a = Float<T>.Atanh(*(Float<T>*)&a); return *(T*)&a; }
    static T IHyperbolicFunctions<T>.Cosh(T a) { *(Float<T>*)&a = Float<T>.Cosh(*(Float<T>*)&a); return *(T*)&a; }
    static T IHyperbolicFunctions<T>.Tanh(T a) { *(Float<T>*)&a = Float<T>.Tanh(*(Float<T>*)&a); return *(T*)&a; }
    static T IHyperbolicFunctions<T>.Sinh(T a) { *(Float<T>*)&a = Float<T>.Sinh(*(Float<T>*)&a); return *(T*)&a; }

    static T IFloatingPointIeee754<T>.Atan2(T a, T b) { *(Float<T>*)&a = Float<T>.Atan2(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }
    static T IFloatingPointIeee754<T>.Atan2Pi(T a, T b) { *(Float<T>*)&a = Float<T>.Atan2Pi(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }
    static T IFloatingPointIeee754<T>.BitDecrement(T a) { *(Float<T>*)&a = Float<T>.BitDecrement(*(Float<T>*)&a); return *(T*)&a; }
    static T IFloatingPointIeee754<T>.BitIncrement(T a) { *(Float<T>*)&a = Float<T>.BitIncrement(*(Float<T>*)&a); return *(T*)&a; }
    static T IFloatingPointIeee754<T>.FusedMultiplyAdd(T a, T b, T c) { *(Float<T>*)&a = Float<T>.FusedMultiplyAdd(*(Float<T>*)&a, *(Float<T>*)&b, *(Float<T>*)&c); return *(T*)&a; }
    static T IFloatingPointIeee754<T>.Ieee754Remainder(T a, T b) { *(Float<T>*)&a = Float<T>.Ieee754Remainder(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }
    static T IFloatingPointIeee754<T>.ScaleB(T a, int n) { *(Float<T>*)&a = Float<T>.ScaleB(*(Float<T>*)&a, n); return *(T*)&a; }
    static int IFloatingPointIeee754<T>.ILogB(T a) => Float<T>.ILogB(*(Float<T>*)&a);

    static T IBinaryNumber<T>.Log2(T a) { *(Float<T>*)&a = Float<T>.Log2(*(Float<T>*)&a); return *(T*)&a; }
    static bool IBinaryNumber<T>.IsPow2(T a) => Float<T>.IsPow2(*(Float<T>*)&a);

    static T INumberBase<T>.MaxMagnitude(T a, T b) { *(Float<T>*)&a = Float<T>.MaxMagnitude(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }
    static T INumberBase<T>.MaxMagnitudeNumber(T a, T b) { *(Float<T>*)&a = Float<T>.MaxMagnitudeNumber(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }
    static T INumberBase<T>.MinMagnitude(T a, T b) { *(Float<T>*)&a = Float<T>.MinMagnitude(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }
    static T INumberBase<T>.MinMagnitudeNumber(T a, T b) { *(Float<T>*)&a = Float<T>.MinMagnitudeNumber(*(Float<T>*)&a, *(Float<T>*)&b); return *(T*)&a; }

    static T IFloatingPoint<T>.Round(T a, int digits, MidpointRounding mode) { *(Float<T>*)&a = Float<T>.Round(*(Float<T>*)&a, digits, mode); return *(T*)&a; }
    int IFloatingPoint<T>.GetExponentByteCount() => Cast<T>((T)this).GetExponentByteCount();
    int IFloatingPoint<T>.GetExponentShortestBitLength() => Cast<T>((T)this).GetExponentShortestBitLength();
    int IFloatingPoint<T>.GetSignificandBitLength() => Cast<T>((T)this).GetSignificandBitLength();
    int IFloatingPoint<T>.GetSignificandByteCount() => Cast<T>((T)this).GetSignificandByteCount();
    bool IFloatingPoint<T>.TryWriteExponentBigEndian(Span<byte> sp, out int bw) => Cast<T>((T)this).TryWrite(0, sp, out bw);
    bool IFloatingPoint<T>.TryWriteExponentLittleEndian(Span<byte> sp, out int bw) => Cast<T>((T)this).TryWrite(1, sp, out bw);
    bool IFloatingPoint<T>.TryWriteSignificandBigEndian(Span<byte> sp, out int bw) => Cast<T>((T)this).TryWrite(2, sp, out bw);
    bool IFloatingPoint<T>.TryWriteSignificandLittleEndian(Span<byte> sp, out int bw) => Cast<T>((T)this).TryWrite(3, sp, out bw);

    static bool INumberBase<T>.TryConvertFromChecked<TOther>(TOther value, out T result) => Float<T>.TryConvertFrom<TOther>(value, 0, out result);
    static bool INumberBase<T>.TryConvertFromSaturating<TOther>(TOther value, out T result) => Float<T>.TryConvertFrom<TOther>(value, 1, out result);
    static bool INumberBase<T>.TryConvertFromTruncating<TOther>(TOther value, out T result) => Float<T>.TryConvertFrom<TOther>(value, 2, out result);
    static bool INumberBase<T>.TryConvertToChecked<TOther>(T value, out TOther result) where TOther : default => Float<T>.TryConvertTo<TOther>(value, 0, out result);
    static bool INumberBase<T>.TryConvertToSaturating<TOther>(T value, out TOther result) where TOther : default => Float<T>.TryConvertTo<TOther>(value, 1, out result);
    static bool INumberBase<T>.TryConvertToTruncating<TOther>(T value, out TOther result) where TOther : default => Float<T>.TryConvertTo<TOther>(value, 2, out result);
  }

#else // NET6_0

  /// <summary>
  /// For NET6 IFloat as place holder interface.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IFloat<T> { }

#endif 
}

