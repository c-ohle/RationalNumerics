using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms.Design.Behavior;
using Test;

namespace System.Numerics.Rational
{
  /// <summary>
  /// Represents an arbitrarily large signed integer. (under construction)
  /// </summary>
  /// <remarks>
  /// This implementation using BigRational.SafeCPU as calculation core is intended for testing and benchmarks to BigInteger in real scenarios.<br/>
  /// Therefore implemented with identical function and interface set inclusive all wrong behavior and illogics of the current INumber implementation.<br/>
  /// Currently under construction.<br/>
  /// boost operator public just to test how much performance would be possible.<br/>
  /// </remarks>
  public readonly partial struct BigInt : IComparable, IComparable<BigInt>, IEquatable<BigInt>, IFormattable
  {
    public override readonly string ToString() => p.ToString("L0"); //((BigInteger)p).ToString();
    public readonly string ToString(string? format, IFormatProvider? formatProvider) => p.ToString(format, formatProvider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(BigInt other) => p.Equals(other.p);
    public readonly int CompareTo(BigInt other) => p.CompareTo(other.p);
    public readonly int CompareTo(object? obj) => obj == null ? 1 : obj is BigInt v ? CompareTo(v) : throw new ArgumentException();

    public BigInt(int value) => p = value;
    public BigInt(uint value) => p = value;
    public BigInt(long value) => p = value;
    public BigInt(ulong value) => p = value;
    public BigInt(float value) { var cpu = rat.task_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
    public BigInt(double value) { var cpu = rat.task_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
    public BigInt(decimal value) { var cpu = rat.task_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
    //public BigInt(byte[] value) // todo:
    //public BigInt(ReadOnlySpan<byte> value, bool isUnsigned = false, bool isBigEndian = false)

    public static BigInt Zero => 0;
    public static BigInt One => 1;
    public static BigInt MinusOne => -1;

    public readonly int Sign => BigRational.Sign(p);
    public readonly bool IsZero => p == 0;
    public readonly bool IsOne => p == 1;
    public readonly bool IsEven
    {
      get
      {
        return false;
      }
    }
    public readonly bool IsPowerOfTwo
    {
      get
      {
        return false;
      }
    }

    public static implicit operator BigInt(long value) => new BigInt(value);
    public static implicit operator BigInt(BigInteger value) => new BigInt(value);
    public static implicit operator BigRational(BigInt value) => value.p;
    public static explicit operator BigInt(BigRational value) => new BigInt(BigRational.Truncate(value));
    public static explicit operator BigInteger(BigInt value) => (BigInteger)value.p;

    public static BigInt operator +(BigInt a) => new BigInt(+a.p);
    public static BigInt operator -(BigInt a) => new BigInt(-a.p);
    public static BigInt operator +(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.add(a.p, b.p); return new BigInt(cpu); }
    public static BigInt operator -(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.sub(a.p, b.p); return new BigInt(cpu); }
    public static BigInt operator *(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.mul(a.p, b.p); return new BigInt(cpu); }
    public static BigInt operator /(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.idiv(); return new BigInt(cpu); }
    public static BigInt operator %(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.imod(); return new BigInt(cpu); }
    public static BigInt operator <<(BigInt a, int b) { var cpu = rat.task_cpu; var c = checked((uint)b); cpu.push(a.p); cpu.shl(c); return new BigInt(cpu); }
    public static BigInt operator >>(BigInt a, int b) { var cpu = rat.task_cpu; var c = checked((uint)b); cpu.push(a.p); cpu.shr(c); return new BigInt(cpu); }
    public static BigInt operator &(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.and(); return new BigInt(cpu); }
    public static BigInt operator ^(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.xor(); return new BigInt(cpu); }
    public static BigInt operator ~(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static BigInt operator --(BigInt value) => value - 1u;
    public static BigInt operator ++(BigInt value) => value + 1u;

    public static bool operator <(BigInt a, BigInt b) => a.CompareTo(b) < 0;
    public static bool operator <=(BigInt a, BigInt b) => a.CompareTo(b) <= 0;
    public static bool operator >(BigInt a, BigInt b) => a.CompareTo(b) > 0;
    public static bool operator >=(BigInt a, BigInt b) => a.CompareTo(b) >= 0;
    public static bool operator ==(BigInt a, BigInt b) => a.Equals(b);
    public static bool operator !=(BigInt a, BigInt b) => !a.Equals(b);

    public static BigInt Pow(BigInt value, int exponent) => new BigInt(BigRational.Pow(value.p, exponent));
    public static BigInt Negate(BigInt a) => new BigInt(-a.p);

    private readonly BigRational p;
    private BigInt(BigRational p) => this.p = p;
    private BigInt(BigRational.SafeCPU cpu) => p = cpu.popr();

    #region boost operator 
    /// <summary>
    /// Performs a bitwise Or operation on two <see cref="BigInt"/> values.
    /// </summary>
    /// <param name="a">Left value.</param>
    /// <param name="b">Right value.</param>
    /// <returns>The result of the bitwise Or operation.</returns>
    public static BigInt operator |(BigInt? a, BigInt b)
    {
      var p = a.GetValueOrDefault().p; if (p == 0) return new BigInt((BigRational?)p | b.p); //BigRational.IsZero(p)
      var cpu = rat.task_cpu; cpu.push(p); cpu.push(b.p); cpu.or(); return new BigInt(cpu);
    }
    public static implicit operator BigInt?(int value)
    {
      if (value == 0) return new BigInt(((BigRational?)value).GetValueOrDefault());
      return new BigInt(value);
    }
    #endregion
  }

#if NET7_0
  public readonly partial struct BigInt : ISpanFormattable, IBinaryInteger<BigInt>, ISignedNumber<BigInt>
  {
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
      => p.TryFormat(destination, out charsWritten, format, provider);
    public BigInt(Int128 value) { var cpu = rat.task_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
    public static implicit operator BigInt(Int128 value) => new BigInt(value);

    // INumber, IBinaryInteger<BigInt>, ISignedNumber<BigInt>
    static BigInt IBitwiseOperators<BigInt, BigInt, BigInt>.operator |(BigInt left, BigInt right)
    {
      var cpu = rat.task_cpu; cpu.push(left.p); cpu.push(right.p); cpu.or(); return new BigInt(cpu);
    }
    public static BigInt operator >>>(BigInt value, int shiftAmount)
    {
      throw new NotImplementedException();
    }

    public static int Radix => 2; //all NET7 BigInteger compatible, what nonsense 
    public static BigInt AdditiveIdentity => Zero;
    public static BigInt MultiplicativeIdentity => One;
    public static BigInt NegativeOne => MinusOne;
    public static BigInt AllBitsSet => MinusOne;

    public int GetByteCount()
    {
      throw new NotImplementedException();
    }
    public int GetShortestBitLength()
    {
      throw new NotImplementedException();
    }
    public static BigInt PopCount(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static BigInt TrailingZeroCount(BigInt value)
    {
      throw new NotImplementedException();
    }
    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
    {
      throw new NotImplementedException();
    }
    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      throw new NotImplementedException();
    }
    public static bool IsPow2(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static BigInt Log2(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static BigInt Abs(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsCanonical(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsComplexNumber(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsEvenInteger(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsFinite(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsImaginaryNumber(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsInfinity(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsInteger(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsNaN(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsNegative(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsNegativeInfinity(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsNormal(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsOddInteger(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsPositive(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsPositiveInfinity(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsRealNumber(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static bool IsSubnormal(BigInt value)
    {
      throw new NotImplementedException();
    }
    public static BigInt MaxMagnitude(BigInt x, BigInt y)
    {
      throw new NotImplementedException();
    }
    public static BigInt MaxMagnitudeNumber(BigInt x, BigInt y)
    {
      throw new NotImplementedException();
    }
    public static BigInt MinMagnitude(BigInt x, BigInt y)
    {
      throw new NotImplementedException();
    }
    public static BigInt MinMagnitudeNumber(BigInt x, BigInt y)
    {
      throw new NotImplementedException();
    }
    public static BigInt Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
      throw new NotImplementedException();
    }
    public static BigInt Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
      throw new NotImplementedException();
    }
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out BigInt result)
    {
      throw new NotImplementedException();
    }
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out BigInt result)
    {
      throw new NotImplementedException();
    }
    public static BigInt Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
      throw new NotImplementedException();
    }
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BigInt result)
    {
      throw new NotImplementedException();
    }
    public static BigInt Parse(string s, IFormatProvider? provider)
    {
      throw new NotImplementedException();
    }
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BigInt result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<BigInt>.TryConvertFromChecked<TOther>(TOther value, out BigInt result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<BigInt>.TryConvertFromSaturating<TOther>(TOther value, out BigInt result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<BigInt>.TryConvertFromTruncating<TOther>(TOther value, out BigInt result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<BigInt>.TryConvertToChecked<TOther>(BigInt value, [NotNullWhen(true)] out TOther result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<BigInt>.TryConvertToSaturating<TOther>(BigInt value, [NotNullWhen(true)] out TOther result)
    {
      throw new NotImplementedException();
    }
    static bool INumberBase<BigInt>.TryConvertToTruncating<TOther>(BigInt value, [NotNullWhen(true)] out TOther result)
    {
      throw new NotImplementedException();
    }

    static bool INumberBase<BigInt>.IsZero(BigInt value)
    {
      throw new NotImplementedException();
    }
  }
#endif
}
