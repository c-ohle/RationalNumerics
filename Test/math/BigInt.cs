using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
  [Serializable] //, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly partial struct BigInt : IComparable, IComparable<BigInt>, IEquatable<BigInt>, IFormattable, ISpanFormattable
  {
    public override readonly string ToString() => p.ToString("L0"); //public override readonly string ToString() => ((BigInteger)p).ToString();
    public readonly string ToString(string? format, IFormatProvider? formatProvider = default) => p.ToString(format, formatProvider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public static BigInt Parse(string value) => Parse(value, NumberStyles.Integer);
    public static BigInt Parse(string value, NumberStyles style) => Parse(value.AsSpan(), style, NumberFormatInfo.CurrentInfo);
    public static BigInt Parse(string value, IFormatProvider? provider) => Parse(value, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
    public static BigInt Parse(string value, NumberStyles style, IFormatProvider? provider) => Parse(value.AsSpan(), style, provider);
    public static BigInt Parse(ReadOnlySpan<char> value, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
      var p = BigRational.Parse(value, style, provider);
      if (!BigRational.IsInteger(p)) throw new ArgumentException(nameof(value)); 
      return new BigInt(p); 
    }
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
    public BigInt(byte[] value) : this(new ReadOnlySpan<byte>(value ?? throw new ArgumentNullException())) { }
    public BigInt(ReadOnlySpan<byte> value, bool isUnsigned = false, bool isBigEndian = false)
    {
      //todo: public cpu.toi() for SafeCPU as span version what does the job much faster
      this = new BigInteger(value, isUnsigned, isBigEndian);
    }

    public static BigInt Zero => 0u;
    public static BigInt One => 1u;
    public static BigInt MinusOne => -1;

    public readonly int Sign => BigRational.Sign(p);
    //some non-optimal implementations as test for span access:
    public readonly bool IsZero { get { var s = (ReadOnlySpan<uint>)p; return s.Length < 2 || s[1] == 0; } }
    public readonly bool IsOne { get { var s = (ReadOnlySpan<uint>)p; return s.Length > 1 && s[1] == 1; } }
    public readonly bool IsEven { get { var s = (ReadOnlySpan<uint>)p; return s.Length < 2 || (s[1] & 1) == 0; } }
    public readonly bool IsPowerOfTwo
    {
      get
      {
        if (BigRational.Sign(p) <= 0) return false;
        var s = (ReadOnlySpan<uint>)p; var n = (int)(s[0] & 0x3fffffff);
        if (!BitOperations.IsPow2(s[n])) return false; for (int i = 1; i < n; i++) if (s[i] != 0) return false; return true;
        //NET7: return BitOperations.IsPow2(s[n]) && s.Slice(1, n - 1).IndexOfAnyExcept(0u) == -1;
      }
    }

    public static implicit operator BigInt(int value) => new BigInt(value);
    public static implicit operator BigInt(uint value) => new BigInt(value);
    public static implicit operator BigInt(long value) => new BigInt(value);
    public static implicit operator BigInt(BigInteger value) => new BigInt(value);
    public static implicit operator BigRational(BigInt value) => value.p;
    public static explicit operator BigInt(BigRational value) => new BigInt(BigRational.Truncate(value)); //todo: cpu
    public static explicit operator BigInteger(BigInt value) => (BigInteger)value.p;

    public static explicit operator byte(BigInt value) => (byte)value.p;
    public static explicit operator sbyte(BigInt value) => (sbyte)value.p;
    public static explicit operator short(BigInt value) => (short)value.p;
    public static explicit operator ushort(BigInt value) => (ushort)value.p;
    public static explicit operator char(BigInt value) => (char)value.p;
    public static explicit operator int(BigInt value) => (int)value.p;
    public static explicit operator uint(BigInt value) => (uint)value.p;
    public static explicit operator long(BigInt value) => (long)value.p;
    public static explicit operator ulong(BigInt value) => (ulong)value.p;
    public static explicit operator nint(BigInt value) => (nint)value.p;
    public static explicit operator nuint(BigInt value) => (nuint)value.p;
    public static explicit operator Half(BigInt value) => (Half)value.p;
    public static explicit operator float(BigInt value) => (float)value.p;
    public static explicit operator double(BigInt value) => (double)value.p;
    public static explicit operator decimal(BigInt value) => (decimal)value.p;

    public static BigInt operator +(BigInt a) => a;
    public static BigInt operator -(BigInt a) => new BigInt(-a.p);
    public static BigInt operator +(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.add(a.p, b.p); return new BigInt(cpu); }
    public static BigInt operator -(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.sub(a.p, b.p); return new BigInt(cpu); }
    public static BigInt operator *(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.mul(a.p, b.p); return new BigInt(cpu); }
    public static BigInt operator /(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.idiv(); return new BigInt(cpu); }
    public static BigInt operator %(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.imod(); return new BigInt(cpu); }
    public static BigInt operator <<(BigInt a, int b) { var cpu = rat.task_cpu; var c = checked((uint)b); cpu.push(a.p); cpu.shl(c); return new BigInt(cpu); }
    public static BigInt operator >>(BigInt a, int b) { var cpu = rat.task_cpu; var c = checked((uint)b); cpu.push(a.p); cpu.shr(c); return new BigInt(cpu); } //todo: >> it's actualy a >>>
    public static BigInt operator &(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.and(); return new BigInt(cpu); }
    public static BigInt operator ^(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.xor(); return new BigInt(cpu); }
    public static BigInt operator ~(BigInt value) => -(value + One);
    public static BigInt operator --(BigInt value) => value - 1u;
    public static BigInt operator ++(BigInt value) => value + 1u;

    public static bool operator <(BigInt a, BigInt b) => a.CompareTo(b) < 0;
    public static bool operator <=(BigInt a, BigInt b) => a.CompareTo(b) <= 0;
    public static bool operator >(BigInt a, BigInt b) => a.CompareTo(b) > 0;
    public static bool operator >=(BigInt a, BigInt b) => a.CompareTo(b) >= 0;
    public static bool operator ==(BigInt a, BigInt b) => a.Equals(b);
    public static bool operator !=(BigInt a, BigInt b) => !a.Equals(b);

    public static BigInt Abs(BigInt value) => new BigInt(BigRational.Abs(value.p));
    public static BigInt Max(BigInt left, BigInt right) => new BigInt(BigRational.Max(left.p, right.p));
    public static BigInt Min(BigInt left, BigInt right) => new BigInt(BigRational.Min(left.p, right.p));
    public static BigInt Add(BigInt left, BigInt right) => left + right;
    public static BigInt Subtract(BigInt left, BigInt right) => left - right;
    public static BigInt Multiply(BigInt left, BigInt right) => left * right;
    public static BigInt Divide(BigInt dividend, BigInt divisor) => dividend / divisor;
    public static BigInt Remainder(BigInt dividend, BigInt divisor) => dividend % divisor;
    public static BigInt DivRem(BigInt dividend, BigInt divisor, out BigInt remainder) { remainder = dividend % divisor; return dividend / divisor; }
    public static (BigInt Quotient, BigInt Remainder) DivRem(BigInt left, BigInt right) { var q = DivRem(left, right, out var r); return (q, r); }
    public static BigInt Negate(BigInt value) => -value;
    public static double Log(BigInt value) => Log(value, Math.E);
    public static double Log(BigInt value, double baseValue) => (double)BigRational.Log(value.p, baseValue, 0);
    public static double Log10(BigInt value) => (double)BigRational.Log10(value.p, 0);
    public static BigInt GreatestCommonDivisor(BigInt left, BigInt right) => new BigInt(BigRational.GreatestCommonDivisor(left.p, right.p));
    public static BigInt Pow(BigInt value, int exponent) => new BigInt(BigRational.Pow(value.p, exponent));
    public static BigInt ModPow(BigInt value, BigInt exponent, BigInt modulus) => new BigInt(BigRational.Pow(value.p, exponent, 0) % modulus);

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
    //works
    //static unsafe int hex(Span<char> s, BigRational v, int l, int a)
    //{
    //  var p = (ReadOnlySpan<uint>)v; //var p = u; if (p == null) { ulong t = 1; p = (uint*)&t; }
    //  if (p.Length == 0) { ulong t = 1; p = new ReadOnlySpan<uint>(&t, 2); }
    //  var n = unchecked((int)(p[0] & 0x3fffffff)); var m = (p[0] & 0x80000000) != 0;
    //  var x = (((n << 5) - BitOperations.LeadingZeroCount(p[n])) >> 2) + 1;
    //  if (l < x) l = x; if (s.Length < l) return l;
    //  a = a == 'X' ? 'A' - 10 : 'a' - 10; var c = 1u;
    //  for (int i = l - 1; i >= 0; i--)
    //  {
    //    int t = l - i - 1, k = 1 + (t >> 3);
    //    var d = (k <= n ? p[k] >> ((t & 7) << 2) : 0) & 0xf;
    //    if (m) if ((d = (~d & 0xf) + c) > 0xf) { d = 0; c = 1; } else c = 0;
    //    s[i] = (char)(d < 10 ? '0' + d : a + d);
    //  }
    //  return l;
    //}
  }

#if NET7_0

  public readonly partial struct BigInt : ISpanFormattable, IBinaryInteger<BigInt>, ISignedNumber<BigInt>
  {
    public BigInt(Int128 value) => this = new BigInt((BigRational)value);
    public BigInt(UInt128 value) => this = new BigInt((BigRational)value);

    public static implicit operator BigInt(byte value) => new BigInt((uint)value);
    public static implicit operator BigInt(char value) => new BigInt((uint)value);
    public static implicit operator BigInt(short value) => new BigInt((int)value);
    public static implicit operator BigInt(sbyte value) => new BigInt((int)value);
    public static implicit operator BigInt(ushort value) => new BigInt((uint)value);
    public static implicit operator BigInt(ulong value) => new BigInt(value);
    public static implicit operator BigInt(nuint value) => new BigInt(value);
    public static implicit operator BigInt(nint value) => new BigInt(value);
    public static implicit operator BigInt(Int128 value) => new BigInt(value);
    public static implicit operator BigInt(UInt128 value) => new BigInt(value);

    public static explicit operator BigInt(Half value) => new BigInt((double)value);
    public static explicit operator BigInt(float value) => new BigInt((double)value);
    public static explicit operator BigInt(double value) => new BigInt(value);
    public static explicit operator BigInt(decimal value) => new BigInt(value);
    public static explicit operator BigInt(Complex value) => value.Imaginary == 0 ? new BigInt(value.Real) : throw new OverflowException();

    public static explicit operator Int128(BigInt value) => (Int128)value.p;
    public static explicit operator UInt128(BigInt value) => (UInt128)value.p;
    public static explicit operator Complex(BigInt value) => new Complex((double)value.p, 0);

    // INumber, IBinaryInteger<BigInt>, ISignedNumber<BigInt>
    static BigInt IBitwiseOperators<BigInt, BigInt, BigInt>.operator |(BigInt left, BigInt right)
    {
      var cpu = rat.task_cpu; cpu.push(left.p); cpu.push(right.p); cpu.or(); return new BigInt(cpu);
    }
    public static BigInt operator >>>(BigInt value, int shiftAmount)
    {
      var cpu = rat.task_cpu; cpu.push(value.p); cpu.shr(checked((uint)shiftAmount)); return new BigInt(cpu);
    }

    public static int Radix => 2; //NET7 nonsense 
    public static BigInt AdditiveIdentity => Zero;
    public static BigInt MultiplicativeIdentity => One;
    public static BigInt NegativeOne => MinusOne;
    public static BigInt AllBitsSet => MinusOne;

    public int GetByteCount()
    {
      // ((BigInteger)this).GetByteCount();
      throw new NotImplementedException("under construction");
    }
    public int GetShortestBitLength()
    {
      throw new NotImplementedException("under construction");
    }
    public static BigInt PopCount(BigInt value)
    {
      throw new NotImplementedException("under construction");
    }
    public static BigInt TrailingZeroCount(BigInt value)
    {
      throw new NotImplementedException("under construction");
    }
    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
    {
      throw new NotImplementedException("under construction");
    }
    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      throw new NotImplementedException("under construction");
    }
    public static bool IsPow2(BigInt value) => value.IsPowerOfTwo;
    public static BigInt Log2(BigInt value)
    {
      throw new NotImplementedException("under construction");
    }
    public static bool IsCanonical(BigInt value) => true;
    public static bool IsComplexNumber(BigInt value) => false;
    public static bool IsEvenInteger(BigInt value) => value.IsEven;
    public static bool IsFinite(BigInt value) => false;
    public static bool IsImaginaryNumber(BigInt value) => false;
    public static bool IsInfinity(BigInt value) => false;
    public static bool IsInteger(BigInt value) => true;
    public static bool IsNaN(BigInt value) => false;
    public static bool IsNegative(BigInt value) => value.Sign < 0;
    public static bool IsNegativeInfinity(BigInt value) => false;
    public static bool IsNormal(BigInt value) => true;
    public static bool IsOddInteger(BigInt value) => value.IsEven;
    public static bool IsPositive(BigInt value) => value.Sign > 0;
    public static bool IsPositiveInfinity(BigInt value) => false;
    public static bool IsRealNumber(BigInt value) => true;
    public static bool IsSubnormal(BigInt value) => false;
    public static BigInt MaxMagnitude(BigInt x, BigInt y) => new BigInt(BigRational.MaxMagnitude(x.p, y.p));
    public static BigInt MaxMagnitudeNumber(BigInt x, BigInt y) => new BigInt(BigRational.MaxMagnitudeNumber(x.p, y.p));
    public static BigInt MinMagnitude(BigInt x, BigInt y) => new BigInt(BigRational.MinMagnitude(x.p, y.p));
    public static BigInt MinMagnitudeNumber(BigInt x, BigInt y) => new BigInt(BigRational.MinMagnitudeNumber(x.p, y.p));
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out BigInt result)
    {
      throw new NotImplementedException("under construction");
    }
    public static BigInt Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
      throw new NotImplementedException("under construction");
    }
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BigInt result) => TryParse(s.AsSpan(), provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BigInt result) => TryParse(s, NumberStyles.Integer, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out BigInt result)
    {
      throw new NotImplementedException("under construction");
    }

    static bool INumberBase<BigInt>.TryConvertFromChecked<TOther>(TOther value, out BigInt result)
    {
      throw new NotImplementedException("under construction");
    }
    static bool INumberBase<BigInt>.TryConvertFromSaturating<TOther>(TOther value, out BigInt result)
    {
      throw new NotImplementedException("under construction");
    }
    static bool INumberBase<BigInt>.TryConvertFromTruncating<TOther>(TOther value, out BigInt result)
    {
      throw new NotImplementedException("under construction");
    }
    static bool INumberBase<BigInt>.TryConvertToChecked<TOther>(BigInt value, [NotNullWhen(true)] out TOther result)
    {
      throw new NotImplementedException("under construction");
    }
    static bool INumberBase<BigInt>.TryConvertToSaturating<TOther>(BigInt value, [NotNullWhen(true)] out TOther result)
    {
      throw new NotImplementedException("under construction");
    }
    static bool INumberBase<BigInt>.TryConvertToTruncating<TOther>(BigInt value, [NotNullWhen(true)] out TOther result)
    {
      throw new NotImplementedException("under construction");
    }
    static bool INumberBase<BigInt>.IsZero(BigInt value) => value.Sign == 0;
  }

#endif
}
