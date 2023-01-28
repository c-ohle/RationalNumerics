
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static System.Numerics.BigRational; //todo: remove

#pragma warning disable CS1591 //todo: xml comments

namespace System.Numerics.Generic
{
  /// <summary>
  /// under construction
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  //public
  unsafe readonly partial struct Decimal<T> : IComparable<Decimal<T>>, IComparable, IEquatable<Decimal<T>>, IFormattable, ISpanFormattable where T : unmanaged
  {
    public static implicit operator Decimal<T>(int value) => default;

    public static Decimal<T> operator +(Decimal<T> value) => value;
    public static Decimal<T> operator -(Decimal<T> value)
    {
      *(byte*)&value.p = 0; return default;
    }
    public static Decimal<T> operator +(Decimal<T> left, Decimal<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static Decimal<T> operator -(Decimal<T> left, Decimal<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static Decimal<T> operator *(Decimal<T> left, Decimal<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static Decimal<T> operator /(Decimal<T> left, Decimal<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static Decimal<T> operator %(Decimal<T> left, Decimal<T> right)
    {
      return default;
    }

    #region private
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly T p;
    #endregion

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
      dest[0] = '0'; charsWritten = 1; return true;
    }
    public int CompareTo(object? obj) => throw new NotImplementedException();
    public int CompareTo(Decimal<T> other) => throw new NotImplementedException();
    public bool Equals(Decimal<T> other) => throw new NotImplementedException();
  }

#if NET7_0_OR_GREATER
  //public 
  unsafe readonly partial struct Decimal<T> : IFloatingPoint<Decimal<T>>, IMinMaxValue<Decimal<T>>
  {
    static Decimal<T> IFloatingPointConstants<Decimal<T>>.E => throw new NotImplementedException();
    static Decimal<T> IFloatingPointConstants<Decimal<T>>.Pi => throw new NotImplementedException();
    static Decimal<T> IFloatingPointConstants<Decimal<T>>.Tau => throw new NotImplementedException();
    static Decimal<T> ISignedNumber<Decimal<T>>.NegativeOne => throw new NotImplementedException();
    static Decimal<T> INumberBase<Decimal<T>>.One => throw new NotImplementedException();
    static int INumberBase<Decimal<T>>.Radix => throw new NotImplementedException();
    static Decimal<T> INumberBase<Decimal<T>>.Zero => throw new NotImplementedException();
    static Decimal<T> IAdditiveIdentity<Decimal<T>, Decimal<T>>.AdditiveIdentity => throw new NotImplementedException();
    static Decimal<T> IMultiplicativeIdentity<Decimal<T>, Decimal<T>>.MultiplicativeIdentity => throw new NotImplementedException();
    static Decimal<T> IMinMaxValue<Decimal<T>>.MaxValue => throw new NotImplementedException();
    static Decimal<T> IMinMaxValue<Decimal<T>>.MinValue => throw new NotImplementedException();
    static Decimal<T> INumberBase<Decimal<T>>.Abs(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsCanonical(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsComplexNumber(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsEvenInteger(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsFinite(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsImaginaryNumber(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsInfinity(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsInteger(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsNaN(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsNegative(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsNegativeInfinity(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsNormal(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsOddInteger(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsPositive(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsPositiveInfinity(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsRealNumber(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsSubnormal(Decimal<T> value) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.IsZero(Decimal<T> value) => throw new NotImplementedException();
    static Decimal<T> INumberBase<Decimal<T>>.MaxMagnitude(Decimal<T> x, Decimal<T> y) => throw new NotImplementedException();
    static Decimal<T> INumberBase<Decimal<T>>.MaxMagnitudeNumber(Decimal<T> x, Decimal<T> y) => throw new NotImplementedException();
    static Decimal<T> INumberBase<Decimal<T>>.MinMagnitude(Decimal<T> x, Decimal<T> y) => throw new NotImplementedException();
    static Decimal<T> INumberBase<Decimal<T>>.MinMagnitudeNumber(Decimal<T> x, Decimal<T> y) => throw new NotImplementedException();
    static Decimal<T> INumberBase<Decimal<T>>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static Decimal<T> INumberBase<Decimal<T>>.Parse(string s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static Decimal<T> ISpanParsable<Decimal<T>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => throw new NotImplementedException();
    static Decimal<T> IParsable<Decimal<T>>.Parse(string s, IFormatProvider? provider) => throw new NotImplementedException();
    static Decimal<T> IFloatingPoint<Decimal<T>>.Round(Decimal<T> x, int digits, MidpointRounding mode) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.TryConvertFromChecked<TOther>(TOther value, out Decimal<T> result) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.TryConvertFromSaturating<TOther>(TOther value, out Decimal<T> result) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.TryConvertFromTruncating<TOther>(TOther value, out Decimal<T> result) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.TryConvertToChecked<TOther>(Decimal<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.TryConvertToSaturating<TOther>(Decimal<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.TryConvertToTruncating<TOther>(Decimal<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Decimal<T> result) => throw new NotImplementedException();
    static bool INumberBase<Decimal<T>>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Decimal<T> result) => throw new NotImplementedException();
    static bool ISpanParsable<Decimal<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Decimal<T> result) => throw new NotImplementedException();
    static bool IParsable<Decimal<T>>.TryParse(string? s, IFormatProvider? provider, out Decimal<T> result) => throw new NotImplementedException();
    // int IComparable.CompareTo(object? obj) => throw new NotImplementedException();
    // int IComparable<Decimal<T>>.CompareTo(Decimal<T> other) => throw new NotImplementedException();
    // bool IEquatable<Decimal<T>>.Equals(Decimal<T> other) => throw new NotImplementedException();
    int IFloatingPoint<Decimal<T>>.GetExponentByteCount() => throw new NotImplementedException();
    int IFloatingPoint<Decimal<T>>.GetExponentShortestBitLength() => throw new NotImplementedException();
    int IFloatingPoint<Decimal<T>>.GetSignificandBitLength() => throw new NotImplementedException();
    int IFloatingPoint<Decimal<T>>.GetSignificandByteCount() => throw new NotImplementedException();
    // string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => throw new NotImplementedException();
    // bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => throw new NotImplementedException();
    bool IFloatingPoint<Decimal<T>>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IFloatingPoint<Decimal<T>>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IFloatingPoint<Decimal<T>>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IFloatingPoint<Decimal<T>>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    static Decimal<T> IUnaryPlusOperators<Decimal<T>, Decimal<T>>.operator +(Decimal<T> value) => throw new NotImplementedException();
    static Decimal<T> IAdditionOperators<Decimal<T>, Decimal<T>, Decimal<T>>.operator +(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static Decimal<T> IUnaryNegationOperators<Decimal<T>, Decimal<T>>.operator -(Decimal<T> value) => throw new NotImplementedException();
    static Decimal<T> ISubtractionOperators<Decimal<T>, Decimal<T>, Decimal<T>>.operator -(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static Decimal<T> IIncrementOperators<Decimal<T>>.operator ++(Decimal<T> value) => throw new NotImplementedException();
    static Decimal<T> IDecrementOperators<Decimal<T>>.operator --(Decimal<T> value) => throw new NotImplementedException();
    static Decimal<T> IMultiplyOperators<Decimal<T>, Decimal<T>, Decimal<T>>.operator *(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static Decimal<T> IDivisionOperators<Decimal<T>, Decimal<T>, Decimal<T>>.operator /(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static Decimal<T> IModulusOperators<Decimal<T>, Decimal<T>, Decimal<T>>.operator %(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static bool IEqualityOperators<Decimal<T>, Decimal<T>, bool>.operator ==(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static bool IEqualityOperators<Decimal<T>, Decimal<T>, bool>.operator !=(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<Decimal<T>, Decimal<T>, bool>.operator <(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<Decimal<T>, Decimal<T>, bool>.operator >(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<Decimal<T>, Decimal<T>, bool>.operator <=(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<Decimal<T>, Decimal<T>, bool>.operator >=(Decimal<T> left, Decimal<T> right) => throw new NotImplementedException();
  }
#endif

}
