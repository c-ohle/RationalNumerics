
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
  public unsafe readonly partial struct Int<T> : IComparable<Int<T>>, IComparable, IEquatable<Int<T>>, IFormattable, ISpanFormattable where T : unmanaged
  {
    public static implicit operator Int<T>(int value) => default;

    public static Int<T> operator +(Int<T> value) => value;
    public static Int<T> operator -(Int<T> value)
    {
      *(byte*)&value.p = 0; return default;
    }
    public static Int<T> operator +(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static Int<T> operator -(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static Int<T> operator *(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static Int<T> operator /(Int<T> left, Int<T> right)
    {
      var cpu = main_cpu; return default;
    }
    public static Int<T> operator %(Int<T> left, Int<T> right)
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
    public int CompareTo(Int<T> other) => throw new NotImplementedException();
    public bool Equals(Int<T> other) => throw new NotImplementedException();
  }

#if NET7_0
  public unsafe readonly partial struct Int<T> : IBinaryInteger<Int<T>>, IMinMaxValue<Int<T>>, ISignedNumber<Int<T>>
  {
    static Int<T> IBinaryNumber<Int<T>>.AllBitsSet => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.One => throw new NotImplementedException();
    static int INumberBase<Int<T>>.Radix => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.Zero => throw new NotImplementedException();
    static Int<T> IAdditiveIdentity<Int<T>, Int<T>>.AdditiveIdentity => throw new NotImplementedException();
    static Int<T> IMultiplicativeIdentity<Int<T>, Int<T>>.MultiplicativeIdentity => throw new NotImplementedException();
    static Int<T> IMinMaxValue<Int<T>>.MaxValue => throw new NotImplementedException();
    static Int<T> IMinMaxValue<Int<T>>.MinValue => throw new NotImplementedException();
    static Int<T> ISignedNumber<Int<T>>.NegativeOne => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.Abs(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsCanonical(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsComplexNumber(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsEvenInteger(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsFinite(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsImaginaryNumber(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsInfinity(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsInteger(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsNaN(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsNegative(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsNegativeInfinity(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsNormal(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsOddInteger(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsPositive(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsPositiveInfinity(Int<T> value) => throw new NotImplementedException();
    static bool IBinaryNumber<Int<T>>.IsPow2(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsRealNumber(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsSubnormal(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.IsZero(Int<T> value) => throw new NotImplementedException();
    static Int<T> IBinaryNumber<Int<T>>.Log2(Int<T> value) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.MaxMagnitude(Int<T> x, Int<T> y) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.MaxMagnitudeNumber(Int<T> x, Int<T> y) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.MinMagnitude(Int<T> x, Int<T> y) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.MinMagnitudeNumber(Int<T> x, Int<T> y) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static Int<T> INumberBase<Int<T>>.Parse(string s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static Int<T> ISpanParsable<Int<T>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => throw new NotImplementedException();
    static Int<T> IParsable<Int<T>>.Parse(string s, IFormatProvider? provider) => throw new NotImplementedException();
    static Int<T> IBinaryInteger<Int<T>>.PopCount(Int<T> value) => throw new NotImplementedException();
    static Int<T> IBinaryInteger<Int<T>>.TrailingZeroCount(Int<T> value) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertFromChecked<TOther>(TOther value, out Int<T> result) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertFromSaturating<TOther>(TOther value, out Int<T> result) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertFromTruncating<TOther>(TOther value, out Int<T> result) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertToChecked<TOther>(Int<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertToSaturating<TOther>(Int<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryConvertToTruncating<TOther>(Int<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Int<T> result) => throw new NotImplementedException();
    static bool INumberBase<Int<T>>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Int<T> result) => throw new NotImplementedException();
    static bool ISpanParsable<Int<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Int<T> result) => throw new NotImplementedException();
    static bool IParsable<Int<T>>.TryParse(string? s, IFormatProvider? provider, out Int<T> result) => throw new NotImplementedException();
    static bool IBinaryInteger<Int<T>>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Int<T> value) => throw new NotImplementedException();
    static bool IBinaryInteger<Int<T>>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Int<T> value) => throw new NotImplementedException();
    int IBinaryInteger<Int<T>>.GetByteCount() => throw new NotImplementedException();
    int IBinaryInteger<Int<T>>.GetShortestBitLength() => throw new NotImplementedException();
    bool IBinaryInteger<Int<T>>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IBinaryInteger<Int<T>>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    // static Int<T> IUnaryPlusOperators<Int<T>, Int<T>>.operator +(Int<T> value)
    // static Int<T> IAdditionOperators<Int<T>, Int<T>, Int<T>>.operator +(Int<T> left, Int<T> right)
    // static Int<T> IUnaryNegationOperators<Int<T>, Int<T>>.operator -(Int<T> value)
    // static Int<T> ISubtractionOperators<Int<T>, Int<T>, Int<T>>.operator -(Int<T> left, Int<T> right)
    static Int<T> IBitwiseOperators<Int<T>, Int<T>, Int<T>>.operator ~(Int<T> value) => throw new NotImplementedException();
    static Int<T> IIncrementOperators<Int<T>>.operator ++(Int<T> value) => throw new NotImplementedException();
    static Int<T> IDecrementOperators<Int<T>>.operator --(Int<T> value) => throw new NotImplementedException();
    // static Int<T> IMultiplyOperators<Int<T>, Int<T>, Int<T>>.operator *(Int<T> left, Int<T> right)
    // static Int<T> IDivisionOperators<Int<T>, Int<T>, Int<T>>.operator /(Int<T> left, Int<T> right)
    // static Int<T> IModulusOperators<Int<T>, Int<T>, Int<T>>.operator %(Int<T> left, Int<T> right)
    static Int<T> IBitwiseOperators<Int<T>, Int<T>, Int<T>>.operator &(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static Int<T> IBitwiseOperators<Int<T>, Int<T>, Int<T>>.operator |(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static Int<T> IBitwiseOperators<Int<T>, Int<T>, Int<T>>.operator ^(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static Int<T> IShiftOperators<Int<T>, int, Int<T>>.operator <<(Int<T> value, int shiftAmount) => throw new NotImplementedException();
    static Int<T> IShiftOperators<Int<T>, int, Int<T>>.operator >>(Int<T> value, int shiftAmount) => throw new NotImplementedException();
    static bool IEqualityOperators<Int<T>, Int<T>, bool>.operator ==(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static bool IEqualityOperators<Int<T>, Int<T>, bool>.operator !=(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<Int<T>, Int<T>, bool>.operator <(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<Int<T>, Int<T>, bool>.operator >(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<Int<T>, Int<T>, bool>.operator <=(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static bool IComparisonOperators<Int<T>, Int<T>, bool>.operator >=(Int<T> left, Int<T> right) => throw new NotImplementedException();
    static Int<T> IShiftOperators<Int<T>, int, Int<T>>.operator >>>(Int<T> value, int shiftAmount) => throw new NotImplementedException();
  }
#endif

}

