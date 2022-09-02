
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
    public static implicit operator UInt<T>(int value) => default;

    public static UInt<T> operator +(UInt<T> value) => value;
    public static UInt<T> operator -(UInt<T> value)
    {
      *(byte*)&value.p = 0; return default;
    }
    public static UInt<T> operator +(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static UInt<T> operator -(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static UInt<T> operator *(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static UInt<T> operator /(UInt<T> left, UInt<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static UInt<T> operator %(UInt<T> left, UInt<T> right)
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
    public int CompareTo(UInt<T> other) => throw new NotImplementedException();
    public bool Equals(UInt<T> other) => throw new NotImplementedException();
  }

#if NET7_0
  public unsafe readonly partial struct UInt<T> : IBinaryInteger<UInt<T>>, IMinMaxValue<UInt<T>>, IUnsignedNumber<UInt<T>>
  {
    static UInt<T> IBinaryNumber<UInt<T>>.AllBitsSet => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.One => throw new NotImplementedException();
    static int INumberBase<UInt<T>>.Radix => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.Zero => throw new NotImplementedException();
    static UInt<T> IAdditiveIdentity<UInt<T>, UInt<T>>.AdditiveIdentity => throw new NotImplementedException();
    static UInt<T> IMultiplicativeIdentity<UInt<T>, UInt<T>>.MultiplicativeIdentity => throw new NotImplementedException();
    static UInt<T> IMinMaxValue<UInt<T>>.MaxValue => throw new NotImplementedException();
    static UInt<T> IMinMaxValue<UInt<T>>.MinValue => throw new NotImplementedException();

    static UInt<T> INumberBase<UInt<T>>.Abs(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsCanonical(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsComplexNumber(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsEvenInteger(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsFinite(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsImaginaryNumber(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsInfinity(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsInteger(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsNaN(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsNegative(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsNegativeInfinity(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsNormal(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsOddInteger(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsPositive(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsPositiveInfinity(UInt<T> value) => throw new NotImplementedException();
    static bool IBinaryNumber<UInt<T>>.IsPow2(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsRealNumber(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsSubnormal(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.IsZero(UInt<T> value) => throw new NotImplementedException();
    static UInt<T> IBinaryNumber<UInt<T>>.Log2(UInt<T> value) => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.MaxMagnitude(UInt<T> x, UInt<T> y) => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.MaxMagnitudeNumber(UInt<T> x, UInt<T> y) => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.MinMagnitude(UInt<T> x, UInt<T> y) => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.MinMagnitudeNumber(UInt<T> x, UInt<T> y) => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static UInt<T> INumberBase<UInt<T>>.Parse(string s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static UInt<T> ISpanParsable<UInt<T>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => throw new NotImplementedException();
    static UInt<T> IParsable<UInt<T>>.Parse(string s, IFormatProvider? provider) => throw new NotImplementedException();
    static UInt<T> IBinaryInteger<UInt<T>>.PopCount(UInt<T> value) => throw new NotImplementedException();
    static UInt<T> IBinaryInteger<UInt<T>>.TrailingZeroCount(UInt<T> value) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertFromChecked<TOther>(TOther value, out UInt<T> result) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertFromSaturating<TOther>(TOther value, out UInt<T> result) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertFromTruncating<TOther>(TOther value, out UInt<T> result) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertToChecked<TOther>(UInt<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertToSaturating<TOther>(UInt<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryConvertToTruncating<TOther>(UInt<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out UInt<T> result) => throw new NotImplementedException();
    static bool INumberBase<UInt<T>>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out UInt<T> result) => throw new NotImplementedException();
    static bool ISpanParsable<UInt<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out UInt<T> result) => throw new NotImplementedException();
    static bool IParsable<UInt<T>>.TryParse(string? s, IFormatProvider? provider, out UInt<T> result) => throw new NotImplementedException();
    static bool IBinaryInteger<UInt<T>>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt<T> value) => throw new NotImplementedException();
    static bool IBinaryInteger<UInt<T>>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out UInt<T> value) => throw new NotImplementedException();
    int IComparable.CompareTo(object? obj) => throw new NotImplementedException();
    int IComparable<UInt<T>>.CompareTo(UInt<T> other) => throw new NotImplementedException();
    bool IEquatable<UInt<T>>.Equals(UInt<T> other) => throw new NotImplementedException();
    int IBinaryInteger<UInt<T>>.GetByteCount() => throw new NotImplementedException();
    int IBinaryInteger<UInt<T>>.GetShortestBitLength() => throw new NotImplementedException();
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => throw new NotImplementedException();
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => throw new NotImplementedException();
    bool IBinaryInteger<UInt<T>>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IBinaryInteger<UInt<T>>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    // static UInt<T> IUnaryPlusOperators<UInt<T>, UInt<T>>.operator +(UInt<T> value) => throw new NotImplementedException();
    // static UInt<T> IAdditionOperators<UInt<T>, UInt<T>, UInt<T>>.operator +(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static UInt<T> IUnaryNegationOperators<UInt<T>, UInt<T>>.operator -(UInt<T> value) => throw new NotImplementedException();
    // static UInt<T> ISubtractionOperators<UInt<T>, UInt<T>, UInt<T>>.operator -(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static UInt<T> IBitwiseOperators<UInt<T>, UInt<T>, UInt<T>>.operator ~(UInt<T> value) => throw new NotImplementedException();
    static UInt<T> IIncrementOperators<UInt<T>>.operator ++(UInt<T> value) => throw new NotImplementedException();
    static UInt<T> IDecrementOperators<UInt<T>>.operator --(UInt<T> value) => throw new NotImplementedException();
    // static UInt<T> IMultiplyOperators<UInt<T>, UInt<T>, UInt<T>>.operator *(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static UInt<T> IDivisionOperators<UInt<T>, UInt<T>, UInt<T>>.operator /(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    // static UInt<T> IModulusOperators<UInt<T>, UInt<T>, UInt<T>>.operator %(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static UInt<T> IBitwiseOperators<UInt<T>, UInt<T>, UInt<T>>.operator &(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static UInt<T> IBitwiseOperators<UInt<T>, UInt<T>, UInt<T>>.operator |(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static UInt<T> IBitwiseOperators<UInt<T>, UInt<T>, UInt<T>>.operator ^(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static UInt<T> IShiftOperators<UInt<T>, int, UInt<T>>.operator <<(UInt<T> value, int shiftAmount) => throw new NotImplementedException();
    static UInt<T> IShiftOperators<UInt<T>, int, UInt<T>>.operator >>(UInt<T> value, int shiftAmount) => throw new NotImplementedException();
    static bool IEqualityOperators<UInt<T>, UInt<T>, bool>.operator ==(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static bool IEqualityOperators<UInt<T>, UInt<T>, bool>.operator !=(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<UInt<T>, UInt<T>, bool>.operator <(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<UInt<T>, UInt<T>, bool>.operator >(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<UInt<T>, UInt<T>, bool>.operator <=(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<UInt<T>, UInt<T>, bool>.operator >=(UInt<T> left, UInt<T> right) => throw new NotImplementedException();
    static UInt<T> IShiftOperators<UInt<T>, int, UInt<T>>.operator >>>(UInt<T> value, int shiftAmount) => throw new NotImplementedException();
  }
#endif

}

