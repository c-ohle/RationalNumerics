using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using static System.Numerics.BigRational;

namespace System.Numerics.Rational
{
  /// <summary>
  /// Represents an arbitrarily large signed integer. (under construction)
  /// </summary>
  /// <remarks>
  /// This implementation using BigRational.SafeCPU as calculation core is intended for testing and benchmarks to BigInteger in real scenarios.<br/>
  /// Therefore implemented with identical function and interface set inclusive wrong behavior and illogics of the current INumber implementation.<br/>
  /// <i>boost operator public just to test how much performance would be possible.</i><br/>
  /// </remarks>
  [Serializable]
  public readonly partial struct BigInt : IComparable, IComparable<BigInt>, IEquatable<BigInt>, IFormattable, ISpanFormattable
  {
    public override readonly string ToString() => p.ToString("L0"); //public override readonly string ToString() => ((BigInteger)p).ToString();
    public readonly unsafe string ToString(string? format, IFormatProvider? provider = default)
    {
      var f = format != null ? format.AsSpan().Trim() : default;
      if (f.Length != 0 && (f[0] | 0x20) == 'x') //int hex style
      {
        var l = f.Length > 1 ? unchecked((int)uint.Parse(f.Slice(1))) : 0;
        var n = hex(default, this.p, l, f[0]); var s = new string('0', n);
        fixed (char* w = s) hex(new Span<char>(w, n), this.p, l, f[0]); return s;
      }
      return p.ToString(format, provider);
    }
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
      var f = format.Trim();
      if (f.Length != 0 && (f[0] | 0x20) == 'x') //int hex style
      {
        var l = f.Length > 1 ? unchecked((int)uint.Parse(f.Slice(1))) : 0;
        var n = hex(destination, this.p, l, f[0]);
        return (charsWritten = n <= destination.Length ? n : 0) != 0;
      }
      return p.TryFormat(destination, out charsWritten, format, provider);
    }
    public static BigInt Parse(string value) => Parse(value, NumberStyles.Integer);
    public static BigInt Parse(string value, NumberStyles style) => Parse(value.AsSpan(), style, NumberFormatInfo.CurrentInfo);
    public static BigInt Parse(string value, IFormatProvider? provider) => Parse(value, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
    public static BigInt Parse(string value, NumberStyles style, IFormatProvider? provider) => Parse(value.AsSpan(), style, provider);
    public static BigInt Parse(ReadOnlySpan<char> value, IFormatProvider? provider) => Parse(value, NumberStyles.Integer, provider);
    public static BigInt Parse(ReadOnlySpan<char> value, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
      if (style == NumberStyles.HexNumber)
      {
        var cpu = rat.task_cpu; cpu.tor(value = value.Trim(), 16);
        if (value[0] > '7') //int hex style
        {
          var t = cpu.msb(); cpu.push(1u); cpu.shl(t); //todo: opt. inc, dec, cpu.pow(2,f(t)); 
          cpu.push(1u); cpu.sub(); cpu.xor(); cpu.push(1u); cpu.add(); cpu.neg();
        }
        return new BigInt(cpu);
      }
      var p = BigRational.Parse(value, style, provider);
      if (!BigRational.IsInteger(p)) throw new ArgumentException(nameof(value));
      return new BigInt(p);
    }
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(BigInt other) => p.Equals(other.p);
    public readonly bool Equals(long other) => p.Equals(other);
    public readonly int CompareTo(BigInt other) => p.CompareTo(other.p);
    public readonly int CompareTo(long other) => p.CompareTo(other);
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
      if (value.Length == 1) { Debug.Assert(value[0] == 0); return; }
      this = new BigInteger(value, isUnsigned, isBigEndian); //todo: impl.
    }

    public byte[] ToByteArray() => ToByteArray(false, false);
    public byte[] ToByteArray(bool isUnsigned = false, bool isBigEndian = false)
    {
      var a = new byte[GetByteCount(isUnsigned)]; TryWriteBytes(a, out var _, isUnsigned, isBigEndian); return a;
    }
    public readonly int GetByteCount(bool isUnsigned = false) { TryWriteBytes(default, out int n, isUnsigned); return n; }
    public readonly bool TryWriteBytes(Span<byte> destination, out int bytesWritten, bool isUnsigned = false, bool isBigEndian = false)
    {
      var u = destination; var w = u != default;
      var s = (ReadOnlySpan<uint>)p; if (s.Length == 0) { if (w) { if (u.Length == 0) goto m1; u[0] = 0; } bytesWritten = 1; return true; }
      var n = unchecked((int)(s[0] & 0x3fffffff)); var m = (s[0] & 0x80000000) != 0; s = s.Slice(1, n);
      if (m && isUnsigned) throw new OverflowException(nameof(isUnsigned));
      var b = MemoryMarshal.Cast<uint, byte>(s); b = b.TrimEnd((byte)0); var l = b.Length;
      if (!m) { if (w) { if (u.Length < b.Length) goto m1; b.CopyTo(u); } }
      else
      {
        if ((b[l - 1] & 0x80) != 0) l++;
        if (!w) { bytesWritten = l; return true; }
        if (u.Length < l) goto m1; int i = 0;
        for (; i < l;) { u[i] = (byte)(~b[i] + 1); if (u[i++] != 0) break; }
        for (; i < l; i++) u[i] = (byte)(i < b.Length ? ~b[i] : 0xff);
      }
      if (w && isBigEndian) u.Slice(0, l).Reverse();
      bytesWritten = l; return true; m1: bytesWritten = 0; return true;
    }

    public readonly int Sign => BigRational.Sign(p);
    //some non-optimal implementations as test for the span access operator:
    public readonly bool IsZero { get { var s = (ReadOnlySpan<uint>)p; return s.Length < 2 || s[1] == 0; } }
    public readonly bool IsOne { get { var s = (ReadOnlySpan<uint>)p; return s.Length > 1 && s[1] == 1; } }
    public readonly bool IsEven { get { var s = (ReadOnlySpan<uint>)p; return s.Length < 2 || (s[1] & 1) == 0; } }
    public readonly bool IsPowerOfTwo
    {
      get
      {
        if (BigRational.Sign(p) <= 0) return false;
        var s = (ReadOnlySpan<uint>)p; var n = unchecked((int)(s[0] & 0x3fffffff));
        return BitOperations.IsPow2(s[n]) && s.Slice(1, n - 1).TrimStart(0u).Length == 0;
      }
    }

    public static implicit operator BigInt(int value) => new BigInt((BigRational)value);
    public static implicit operator BigInt(uint value) => new BigInt((BigRational)value);
    public static implicit operator BigInt(long value) => new BigInt((BigRational)value);
    public static implicit operator BigInt(BigInteger value) => new BigInt((BigRational)value);
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

    public static bool operator <(BigInt a, long b) => a.CompareTo(b) < 0;
    public static bool operator <=(BigInt a, long b) => a.CompareTo(b) <= 0;
    public static bool operator >(BigInt a, long b) => a.CompareTo(b) > 0;
    public static bool operator >=(BigInt a, long b) => a.CompareTo(b) >= 0;
    public static bool operator ==(BigInt a, long b) => a.Equals(b);
    public static bool operator !=(BigInt a, long b) => !a.Equals(b);

    public static BigInt Zero => default;
    public static BigInt One => new BigInt((BigRational)1u);
    public static BigInt MinusOne => new BigInt((BigRational)(-1));

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

    #region private
    private readonly BigRational p;
    private BigInt(BigRational p) => this.p = p;
    private BigInt(BigRational.SafeCPU cpu) => p = cpu.popr();
    private static unsafe int hex(Span<char> s, BigRational v, int l, int a)
    {
      var p = (ReadOnlySpan<uint>)v;
      if (p.Length == 0) { ulong t = 1; p = new ReadOnlySpan<uint>(&t, 2); } // zero
      var n = unchecked((int)(p[0] & 0x3fffffff)); var m = (p[0] & 0x80000000) != 0;
      var z = (n << 5) - BitOperations.LeadingZeroCount(p[n]);
      var x = (z >> 2) + 1;
      if (m && (z & 3) == 0 && BitOperations.IsPow2(p[n]) && p.Slice(2, n - 1).TrimStart(0u).Length == 0) x--; //80
      if (l < x) l = x; if (s.Length < l) return l;
      var c = 1u; a = a == 'X' ? 'A' - 10 : 'a' - 10;
      for (int i = l - 1; i >= 0; i--)
      {
        int t = l - i - 1, k = 1 + (t >> 3);
        var d = (k <= n ? p[k] >> ((t & 7) << 2) : 0) & 0xf;
        if (m) if ((d = (~d & 0xf) + c) > 0xf) { d = 0; c = 1; } else c = 0;
        s[i] = (char)(d < 10 ? '0' + d : a + d);
      }
      return l;
    }
    #endregion

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
    public static explicit operator BigInt(float value) => new BigInt((float)value);
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

    static int INumberBase<BigInt>.Radix => 1; //NET7 BigInteger nonsense Radix = 2
    static BigInt IAdditiveIdentity<BigInt, BigInt>.AdditiveIdentity => Zero;
    static BigInt IMultiplicativeIdentity<BigInt, BigInt>.MultiplicativeIdentity => One;
    static BigInt ISignedNumber<BigInt>.NegativeOne => MinusOne;
    static BigInt IBinaryNumber<BigInt>.AllBitsSet => MinusOne; //NET7 BigInteger nonsense

    public static BigInt PopCount(BigInt value)
    {
      var s = (ReadOnlySpan<uint>)value.p; if (s.Length == 0) return default;
      int i = 1, n = unchecked((int)(s[0] & 0x3fffffff)); var c = 0ul;
      if ((s[0] & 0x80000000) == 0)
      {
        for (; i <= n; i++) c += unchecked((uint)BitOperations.PopCount(s[i]));
      }
      else //for < 0 emulate BigInteger 32^n - NET7 nonsense
      {
        for (uint t; ;) { c += unchecked((uint)uint.PopCount(t = ~s[i] + 1)); if (++i > n || t != 0) break; }
        for (; i <= n;) { c += unchecked((uint)uint.PopCount(~s[i++])); }
      }
      return c;
    }
    public static BigInt TrailingZeroCount(BigInt value)
    {
      var s = (ReadOnlySpan<uint>)value.p; if (s.Length == 0) return MinusOne; //NET7 BigInteger nonsense - returns 32 "undefined" 
      var n = unchecked((int)(s[0] & 0x3fffffff)); var c = 0ul;
      for (int i = 1; i <= n; i++)
      {
        if (s[i] == 0) c += 32;
        else { c += unchecked((uint)BitOperations.TrailingZeroCount(s[i])); break; }
      }
      return c;
    }

    int IBinaryInteger<BigInt>.GetByteCount() => GetByteCount();
    int IBinaryInteger<BigInt>.GetShortestBitLength() => GetByteCount();
    bool IBinaryInteger<BigInt>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
      => TryWriteBytes(destination, out bytesWritten, false, true);
    bool IBinaryInteger<BigInt>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
      => TryWriteBytes(destination, out bytesWritten, false, false);

    public static bool IsPow2(BigInt value) => value.IsPowerOfTwo;
    public static BigInt Log2(BigInt value) //todo: check general Log2(0), ILog2(0) behavior
    {
      if (value.Sign < 0) throw new ArgumentOutOfRangeException(nameof(value));
      return BigRational.ILog2(value.p);
    }

    public static bool IsEvenInteger(BigInt value) => value.IsEven;
    public static bool IsNegative(BigInt value) => value.Sign < 0;
    public static bool IsOddInteger(BigInt value) => value.IsEven;
    public static bool IsPositive(BigInt value) => value.Sign > 0;
    static bool INumberBase<BigInt>.IsCanonical(BigInt value) => true;
    static bool INumberBase<BigInt>.IsComplexNumber(BigInt value) => false;
    static bool INumberBase<BigInt>.IsZero(BigInt value) => value.IsZero;
    static bool INumberBase<BigInt>.IsFinite(BigInt value) => false;
    static bool INumberBase<BigInt>.IsImaginaryNumber(BigInt value) => false;
    static bool INumberBase<BigInt>.IsInfinity(BigInt value) => false;
    static bool INumberBase<BigInt>.IsInteger(BigInt value) => true;
    static bool INumberBase<BigInt>.IsNaN(BigInt value) => false;
    static bool INumberBase<BigInt>.IsNegativeInfinity(BigInt value) => false;
    static bool INumberBase<BigInt>.IsNormal(BigInt value) => value.Sign != 0; //NET7 BigInteger nonsense
    static bool INumberBase<BigInt>.IsPositiveInfinity(BigInt value) => false;
    static bool INumberBase<BigInt>.IsRealNumber(BigInt value) => true;
    static bool INumberBase<BigInt>.IsSubnormal(BigInt value) => false;
    static BigInt INumberBase<BigInt>.MaxMagnitudeNumber(BigInt x, BigInt y) => new BigInt(BigRational.MaxMagnitudeNumber(x.p, y.p));
    static BigInt INumberBase<BigInt>.MinMagnitudeNumber(BigInt x, BigInt y) => new BigInt(BigRational.MinMagnitudeNumber(x.p, y.p));
    public static BigInt MaxMagnitude(BigInt x, BigInt y) => new BigInt(BigRational.MaxMagnitude(x.p, y.p));
    public static BigInt MinMagnitude(BigInt x, BigInt y) => new BigInt(BigRational.MinMagnitude(x.p, y.p));

    static int INumber<BigInt>.Sign(BigInt value) => Sign(value);
    static BigInt INumber<BigInt>.MaxNumber(BigInt x, BigInt y) => Max(x, y);
    static BigInt INumber<BigInt>.MinNumber(BigInt x, BigInt y) => Min(x, y);

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out BigInt result) => TryParse(s.AsSpan(), style, provider, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BigInt result) => TryParse(s.AsSpan(), NumberStyles.Integer, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BigInt result) => TryParse(s, NumberStyles.Integer, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out BigInt result)
    {
      if (style == NumberStyles.HexNumber) { result = Parse(s, style, provider); return true; }
      if (!BigRational.TryParse(s, style, provider, out var r) || !BigRational.IsInteger(r)) { result = default; return false; }
      result = new BigInt(r); return true;
    }

    static bool INumberBase<BigInt>.TryConvertFromChecked<T>(T value, out BigInt result)
    {
      static bool f<R>(T v, out R result) where R : INumberBase<R> => R.TryConvertFromChecked(v, out result!);
      if (!f<BigRational>(value, out var r)) { result = default; return false; }
      result = new BigInt(BigRational.Truncate(r)); return true; //todo: check if spec is available     
    }
    static bool INumberBase<BigInt>.TryConvertFromSaturating<T>(T value, out BigInt result)
    {
      static bool f<R>(T v, out R result) where R : INumberBase<R> => R.TryConvertFromSaturating(v, out result!);
      if (!f<BigRational>(value, out var r)) { result = default; return false; }
      result = new BigInt(BigRational.Truncate(r)); return true; //todo: check if spec is available
    }
    static bool INumberBase<BigInt>.TryConvertFromTruncating<T>(T value, out BigInt result)
    {
      static bool f<R>(T v, out R result) where R : INumberBase<R> => R.TryConvertFromTruncating(v, out result!);
      if (!f<BigRational>(value, out var r)) { result = default; return false; }
      result = new BigInt(BigRational.Truncate(r)); return true; //todo: check if spec is available
    }
    static bool INumberBase<BigInt>.TryConvertToChecked<T>(BigInt value, [NotNullWhen(true)] out T result)
    {
      static bool f<R>(R v, out T result) where R : INumberBase<R> => R.TryConvertToChecked(v, out result!);
      return f<BigRational>(value.p, out result); //ok
    }
    static bool INumberBase<BigInt>.TryConvertToSaturating<T>(BigInt value, [NotNullWhen(true)] out T result)
    {
      static bool f<R>(R v, out T result) where R : INumberBase<R> => R.TryConvertToSaturating(v, out result!);
      return f<BigRational>(value.p, out result); //ok
    }
    static bool INumberBase<BigInt>.TryConvertToTruncating<T>(BigInt value, [NotNullWhen(true)] out T result)
    {
      static bool f<R>(R v, out T result) where R : INumberBase<R> => R.TryConvertToTruncating(v, out result!);
      return f<BigRational>(value.p, out result); //ok
    }
  }
#endif
}
