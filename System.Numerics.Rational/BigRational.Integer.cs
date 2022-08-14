
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS1591  //todo: xml comments

namespace System.Numerics
{
  unsafe partial struct BigRational
  {
    /// <summary>
    /// Represents an arbitrarily large signed integer.
    /// </summary>
    /// <remarks>
    /// This type of number is compatible with .NET7 <see cref="BigInteger"/>, hence all the overhead.<br/>
    /// There are additional functions for fast and numerically exact bit-level operations e.g:<br/>
    /// <see cref="Shl"/>, <see cref="Shr"/>, <see cref="Msb(Integer)"/>, <see cref="Lsb(Integer)"/> ...<br/>
    /// The implementation is currently not yet CPU optimized.
    /// </remarks>
    [Serializable, SkipLocalsInit] //, DebuggerDisplay("{ToString(),nq}")
    public readonly partial struct Integer : IComparable, IComparable<Integer>, IEquatable<Integer>, IFormattable, ISpanFormattable
    {
      public override readonly string ToString() => p.ToString("L0"); //public override readonly string ToString() => ((BigInteger)p).ToString();
      public readonly unsafe string ToString(string? format, IFormatProvider? provider = default)
      {
        var f = format != null ? format.AsSpan().Trim() : default; var c = f.Length != 0 ? f[0] | 0x20 : 0;
        if (c == 'x') //int hex style
        {
          TryFormat(default, out var n, f, provider); var s = new string('0', n);
          fixed (char* w = s) TryFormat(new Span<char>(w, n), out n, f, provider); return s;
        }
        return p.ToString(c == 'r' ? "L0" : format, provider);
      }
      public readonly bool TryFormat(Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
      {
        var f = format.Trim(); var c = f.Length != 0 ? f[0] | 0x20 : 0;
        if (c == 'x') //int hex style
        {
          var l = f.Length > 1 ? unchecked((int)uint.Parse(f.Slice(1))) : 0;
          var cpu = main_cpu; cpu.push(this.p);
          var z = cpu.msb(); int s = cpu.sign(), x = unchecked((int)((z >> 2) + 1));
          if (s < 0 && (z & 3) == 0 && cpu.ipt()) x--; //80
          var n = l > x ? l : x; if (dest.Length < n) { cpu.pop(); charsWritten = dest == default ? n : 0; return false; }
          if (s < 0) cpu.toc(4); cpu.get(cpu.mark() - 1, out ReadOnlySpan<uint> sp);
          sp = sp.Slice(1, unchecked((int)(sp[0] & 0x7fffffff)));
          for (int i = 0, k, o = f[0] == 'X' ? 'A' - 10 : 'a' - 10; i < n; i++)
          {
            var d = (k = i >> 3) < sp.Length ? (sp[k] >> ((i & 7) << 2)) & 0xf : s < 0 ? 0xf : 0u;
            dest[n - i - 1] = (char)(d < 10 ? '0' + d : o + d);
          }
          cpu.pop(); charsWritten = n; return true;
        }
        return p.TryFormat(dest, out charsWritten, c == 'r' ? "L0" : format, provider);
      }
      public static Integer Parse(string value) => Parse(value, NumberStyles.Integer);
      public static Integer Parse(string value, NumberStyles style) => Parse(value.AsSpan(), style, NumberFormatInfo.CurrentInfo);
      public static Integer Parse(string value, IFormatProvider? provider) => Parse(value, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
      public static Integer Parse(string value, NumberStyles style, IFormatProvider? provider) => Parse(value.AsSpan(), style, provider);
      public static Integer Parse(ReadOnlySpan<char> value, IFormatProvider? provider) => Parse(value, NumberStyles.Integer, provider);
      public static Integer Parse(ReadOnlySpan<char> value, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
      {
        if (style == NumberStyles.HexNumber)
        {
          var cpu = main_cpu; cpu.tor(value = value.Trim(), 16);
          if (value[0] > '7') //int hex style
          {
            var t = cpu.msb(); cpu.push(1u); cpu.shl(unchecked((int)t)); //todo: opt. check cpu.pow(2, t); ??? 
            cpu.dec(); cpu.xor(); cpu.inc(); cpu.neg();
          }
          return new Integer(cpu);
        }
        var p = BigRational.Parse(value, style, provider);
        if (!BigRational.IsInteger(p)) throw new ArgumentException(nameof(value));
        return new Integer(p);
      }
      public override readonly int GetHashCode() => p.GetHashCode();
      public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
      public readonly bool Equals(Integer other) => p.Equals(other.p);
      public readonly bool Equals(long other) => p.Equals(other);
      public readonly int CompareTo(Integer other) => p.CompareTo(other.p);
      public readonly int CompareTo(long other) => p.CompareTo(other);
      public readonly int CompareTo(object? obj) => obj == null ? 1 : obj is Integer v ? CompareTo(v) : throw new ArgumentException();

      public Integer(int value) => p = value;
      public Integer(uint value) => p = value;
      public Integer(long value) => p = value;
      public Integer(ulong value) => p = value;
      public Integer(float value) { var cpu = main_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
      public Integer(double value) { var cpu = main_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
      public Integer(decimal value) { var cpu = main_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
      public Integer(byte[] value) : this(new ReadOnlySpan<byte>(value ?? throw new ArgumentNullException())) { }
      public Integer(ReadOnlySpan<byte> value, bool isUnsigned = false, bool isBigEndian = false)
      {
        int n = value.Length, k = isBigEndian ? n - 1 : 0, d = isBigEndian ? -1 : +1;
        var cpu = main_cpu; cpu.push(value[k]); //todo: opt.
        for (int i = 1; i < n; i++) { k += d; cpu.push(value[k]); cpu.shl(i << 3); cpu.or(); }
        if (!isUnsigned && (value[k] & 0x80) != 0) cpu.toc(4); this.p = cpu.popr();
      }

      public byte[] ToByteArray() => ToByteArray(false, false);
      public byte[] ToByteArray(bool isUnsigned = false, bool isBigEndian = false)
      {
        var a = new byte[GetByteCount(isUnsigned)]; TryWriteBytes(a, out var _, isUnsigned, isBigEndian); return a;
      }
      public readonly int GetByteCount(bool isUnsigned = false) { TryWriteBytes(default, out int n, isUnsigned); return n; }
      public readonly bool TryWriteBytes(Span<byte> dest, out int bytesWritten, bool isUnsigned = false, bool isBigEndian = false)
      {
        if (isUnsigned && BigRational.Sign(this.p) < 0) throw new OverflowException(nameof(isUnsigned));
        var cpu = main_cpu; cpu.push(this.p); var s = cpu.sign(); var z = cpu.msb();
        var n = s != 0 ? unchecked((int)(((z - 1) >> 3) + 1)) : isUnsigned ? 1 : 0; //todo: opt.
        var l = n; if (!isUnsigned && (z & 7) == 0 && !(s < 0 && cpu.ipt())) n++;
        if (dest.Length < n) { cpu.pop(); bytesWritten = dest != default ? 0 : n; return false; }
        if (s == -1) cpu.toc(4); cpu.get(cpu.mark() - 1, out ReadOnlySpan<uint> sp);
        var bb = MemoryMarshal.Cast<uint, byte>(sp.Slice(1)).Slice(0, l);
        bb.CopyTo(dest); if (l != n) dest[l] = (byte)(s < 0 ? 0xff : 0); cpu.pop();
        bytesWritten = n; if (isBigEndian) dest.Slice(0, n).Reverse(); return true;
      }

      public readonly int Sign => BigRational.Sign(p);
      //some non-optimal implementations as test for the span access operator:
      public readonly bool IsZero { get { var s = (ReadOnlySpan<uint>)p; return s.Length < 2 || s[1] == 0; } }
      public readonly bool IsOne { get { var s = (ReadOnlySpan<uint>)p; return s.Length > 1 && s[0] == 1 && s[1] == 1; } }
      public readonly bool IsEven { get { var s = (ReadOnlySpan<uint>)p; return s.Length < 2 || (s[1] & 1) == 0; } }
      public readonly bool IsPowerOfTwo
      {
        get
        {
          var cpu = main_cpu; cpu.push(p); var b = cpu.sign() > 0 && cpu.ipt(); cpu.pop(); return b;
          //if (BigRational.Sign(p) <= 0) return false;
          //var s = (ReadOnlySpan<uint>)p; var n = unchecked((int)(s[0] & 0x3fffffff));
          //return BitOperations.IsPow2(s[n]) && s.Slice(1, n - 1).TrimStart(0u).Length == 0;
        }
      }

      public static implicit operator Integer(int value) => new Integer((BigRational)value);
      public static implicit operator Integer?(int value) => new Integer(((BigRational?)value).GetValueOrDefault());
      public static implicit operator Integer(uint value) => new Integer((BigRational)value);
      public static implicit operator Integer(long value) => new Integer((BigRational)value);
      public static implicit operator Integer(BigInteger value) => new Integer((BigRational)value);
      public static implicit operator BigRational(Integer value) => value.p;
      public static explicit operator Integer(BigRational value) => new Integer(BigRational.IsInteger(value) ? value : BigRational.Truncate(value));
      public static explicit operator BigInteger(Integer value) => (BigInteger)value.p;

      public static explicit operator byte(Integer value) => (byte)value.p;
      public static explicit operator sbyte(Integer value) => (sbyte)value.p;
      public static explicit operator short(Integer value) => (short)value.p;
      public static explicit operator ushort(Integer value) => (ushort)value.p;
      public static explicit operator char(Integer value) => (char)value.p;
      public static explicit operator int(Integer value) => (int)value.p;
      public static explicit operator uint(Integer value) => (uint)value.p;
      public static explicit operator long(Integer value) => (long)value.p;
      public static explicit operator ulong(Integer value) => (ulong)value.p;
      public static explicit operator nint(Integer value) => (nint)value.p;
      public static explicit operator nuint(Integer value) => (nuint)value.p;
      public static explicit operator Half(Integer value) => (Half)value.p;
      public static explicit operator float(Integer value) => (float)value.p;
      public static explicit operator double(Integer value) => (double)value.p;
      public static explicit operator decimal(Integer value) => (decimal)value.p;

      public static Integer operator +(Integer a) => a;
      public static Integer operator -(Integer a) => new Integer(-a.p);
      public static Integer operator +(Integer a, Integer b) { var cpu = main_cpu; cpu.add(a.p, b.p); return new Integer(cpu); }
      public static Integer operator -(Integer a, Integer b) { var cpu = main_cpu; cpu.sub(a.p, b.p); return new Integer(cpu); }
      public static Integer operator *(Integer a, Integer b) { var cpu = main_cpu; cpu.mul(a.p, b.p); return new Integer(cpu); }
      public static Integer operator /(Integer a, Integer b) { var cpu = main_cpu; cpu.push(a.p); cpu.push(b.p); cpu.idiv(); return new Integer(cpu); }
      public static Integer operator %(Integer a, Integer b) { var cpu = main_cpu; cpu.push(a.p); cpu.push(b.p); cpu.imod(); return new Integer(cpu); }
      public static Integer operator <<(Integer a, int b)
      {
        if (b <= 0) return b != 0 ? a >> -b : b;
        var cpu = main_cpu; cpu.push(a.p); cpu.shl(b);
        return new Integer(cpu);
      }
      public static Integer operator >>(Integer a, int b)
      {
        if (b <= 0) return b != 0 ? a << -b : a;
        var cpu = main_cpu; cpu.push(a.p); var s = cpu.sign() == -1;
        if (s) cpu.toc(4); cpu.shr(b); if (s) cpu.toc(4);
        return new Integer(cpu);
      }
      public static Integer operator &(Integer a, Integer b) { var cpu = main_cpu; cpu.push(a.p); cpu.push(b.p); cpu.and(); return new Integer(cpu); }
      /// <summary>
      /// Performs a bitwise Or operation on two <see cref="Integer"/> values.
      /// </summary>
      /// <param name="a">Left value.</param>
      /// <param name="b">Right value.</param>
      /// <returns>The result of the bitwise Or operation.</returns>
      public static Integer operator |(Integer? a, Integer b) => new Integer((BigRational?)a.GetValueOrDefault().p | b.p);
      public static Integer operator ^(Integer a, Integer b) { var cpu = main_cpu; cpu.push(a.p); cpu.push(b.p); cpu.xor(); return new Integer(cpu); }
      public static Integer operator ~(Integer value) => -(value + One);
      public static Integer operator --(Integer value) => value - 1u;
      public static Integer operator ++(Integer value) => value + 1u;

      public static bool operator <(Integer a, Integer b) => a.CompareTo(b) < 0;
      public static bool operator <=(Integer a, Integer b) => a.CompareTo(b) <= 0;
      public static bool operator >(Integer a, Integer b) => a.CompareTo(b) > 0;
      public static bool operator >=(Integer a, Integer b) => a.CompareTo(b) >= 0;
      public static bool operator ==(Integer a, Integer b) => a.Equals(b);
      public static bool operator !=(Integer a, Integer b) => !a.Equals(b);

      public static bool operator <(Integer a, long b) => a.CompareTo(b) < 0;
      public static bool operator <=(Integer a, long b) => a.CompareTo(b) <= 0;
      public static bool operator >(Integer a, long b) => a.CompareTo(b) > 0;
      public static bool operator >=(Integer a, long b) => a.CompareTo(b) >= 0;
      public static bool operator ==(Integer a, long b) => a.Equals(b);
      public static bool operator !=(Integer a, long b) => !a.Equals(b);

      public static Integer Zero => default;
      public static Integer One => new Integer((BigRational)1u);
      public static Integer MinusOne => new Integer((BigRational)(-1));

      public static Integer Abs(Integer value) => new Integer(BigRational.Abs(value.p));
      public static Integer Max(Integer left, Integer right) => new Integer(BigRational.Max(left.p, right.p));
      public static Integer Min(Integer left, Integer right) => new Integer(BigRational.Min(left.p, right.p));
      public static Integer Add(Integer left, Integer right) => left + right;
      public static Integer Subtract(Integer left, Integer right) => left - right;
      public static Integer Multiply(Integer left, Integer right) => left * right;
      public static Integer Divide(Integer dividend, Integer divisor) => dividend / divisor;
      public static Integer Remainder(Integer dividend, Integer divisor) => dividend % divisor;
      public static Integer DivRem(Integer dividend, Integer divisor, out Integer remainder)
      {
        remainder = dividend % divisor;
        return dividend / divisor;
      }
      public static (Integer Quotient, Integer Remainder) DivRem(Integer left, Integer right) { var q = DivRem(left, right, out var r); return (q, r); }
      public static Integer Negate(Integer value) => -value;
      public static double Log(Integer value) => Log(value, Math.E);
      public static double Log(Integer value, double baseValue) => (double)BigRational.Log(value.p, baseValue, 0);
      public static double Log10(Integer value) => (double)BigRational.Log10(value.p, 0);
      /// <summary>
      /// Finds the greatest common divisor (GCD) of two <see cref="Integer"/> integer values.
      /// </summary>
      /// <param name="a">The first value.</param>
      /// <param name="b">The second value.</param>
      /// <returns>The greatest common divisor of <paramref name="a"/> and <paramref name="b"/>.</returns>
      public static Integer GreatestCommonDivisor(Integer a, Integer b)
      {
        var cpu = main_cpu; cpu.push(a); cpu.push(b);
        cpu.gcd(); return new Integer(cpu);
      }
      /// <summary>
      /// Finds the least common multiple (LCM) of two <see cref="Integer"/> integer values.
      /// </summary>
      /// <param name="a">The first value.</param>
      /// <param name="b">The second value.</param>
      /// <returns>The least common multiple of <paramref name="a"/> and <paramref name="b"/>.</returns>
      public static BigRational LeastCommonMultiple(BigRational a, BigRational b)
      {
        //|a * b| / gcd(a, b) == |a / gcd(a, b) * b| 
        var cpu = main_cpu; cpu.push(a); cpu.push(b);
        cpu.dup(); cpu.dup(2); cpu.gcd(); cpu.div(); cpu.mul(); cpu.abs();
        return cpu.popr();
      }
      public static Integer Pow(Integer value, int exponent) => new Integer(BigRational.Pow(value.p, exponent));
      public static Integer ModPow(Integer value, Integer exponent, Integer modulus) => new Integer(BigRational.Pow(value.p, exponent, 0) % modulus);

      //non-std's
      public static int Msb(Integer value) { var cpu = main_cpu; cpu.push(value.p); var x = cpu.msb(); cpu.pop(); return unchecked((int)x); }
      public static int Lsb(Integer value) { var cpu = main_cpu; cpu.push(value.p); var x = cpu.lsb(); cpu.pop(); return unchecked((int)x); }
      public static Integer Shl(Integer value, int shift)
      {
        if (shift <= 0) return shift == 0 ? value : Shr(value, -shift);
        var cpu = main_cpu; cpu.push(value.p);
        cpu.shl(shift); return new Integer(cpu);
      }
      public static Integer Shr(Integer value, int shift)
      {
        if (shift <= 0) return shift == 0 ? value : Shl(value, -shift);
        var cpu = main_cpu; cpu.push(value.p);
        cpu.shr(shift); return new Integer(cpu);
      }

      #region private
      private readonly BigRational p;
      private Integer(BigRational p) => this.p = p;
      private Integer(BigRational.CPU cpu) => p = cpu.popr();
      #endregion
    }

#if NET7_0
    public readonly partial struct Integer : ISpanFormattable, IBinaryInteger<Integer>, ISignedNumber<Integer>
    {
      public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out Integer result) => TryParse(s.AsSpan(), style, provider, out result);
      public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Integer result) => TryParse(s.AsSpan(), NumberStyles.Integer, provider, out result);
      public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Integer result) => TryParse(s, NumberStyles.Integer, provider, out result);
      public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Integer result)
      {
        if (style == NumberStyles.HexNumber) { result = Parse(s, style, provider); return true; }
        if (!BigRational.TryParse(s, style, provider, out var r) || !BigRational.IsInteger(r)) { result = default; return false; }
        result = new Integer(r); return true;
      }

      public Integer(Int128 value) => this = new Integer((BigRational)value);
      public Integer(UInt128 value) => this = new Integer((BigRational)value);

      public static bool IsEvenInteger(Integer value) => value.IsEven;
      public static bool IsNegative(Integer value) => value.Sign < 0;
      public static bool IsOddInteger(Integer value) => value.IsEven;
      public static bool IsPositive(Integer value) => value.Sign > 0;
      public static Integer PopCount(Integer value)
      {
        var s = (ReadOnlySpan<uint>)value.p; if (s.Length == 0) return default;
        int i = 1, n = unchecked((int)(s[0] & 0x3fffffff)); var c = 0ul;
        if ((s[0] & 0x80000000) == 0) { for (; i <= n; i++) c += unchecked((uint)BitOperations.PopCount(s[i])); }
        else //for < 0 emulate BigInteger 32^n - NET7 nonsense
        {
          for (uint t; ;) { c += unchecked((uint)uint.PopCount(t = ~s[i] + 1)); if (++i > n || t != 0) break; }
          for (; i <= n;) { c += unchecked((uint)uint.PopCount(~s[i++])); }
        }
        return c;
      }
      public static Integer TrailingZeroCount(Integer value)
      {
        var s = (ReadOnlySpan<uint>)value.p; if (s.Length == 0) return 32; //NET7 BigInteger nonsense 
        var n = unchecked((int)(s[0] & 0x3fffffff)); var c = 0ul;
        for (int i = 1; i <= n; i++)
        {
          if (s[i] == 0) c += 32;
          else { c += unchecked((uint)BitOperations.TrailingZeroCount(s[i])); break; }
        }
        return c;
      }
      public static Integer RotateLeft(Integer value, int c)
      {
        return BigInteger.RotateLeft((BigInteger)value, c); //todo: rot
      }
      public static Integer RotateRight(Integer value, int c)
      {
        return BigInteger.RotateRight((BigInteger)value, c); //todo: rot
      }
      public static bool IsPow2(Integer value) => value.IsPowerOfTwo;
      public static Integer Log2(Integer value) //todo: check general Log2(0), ILog2(0) behavior
      {
        if (value.Sign < 0) throw new ArgumentOutOfRangeException(nameof(value));
        return BigRational.ILog2(value.p);
      }
      public static Integer MaxMagnitude(Integer x, Integer y) => new Integer(BigRational.MaxMagnitude(x.p, y.p));
      public static Integer MinMagnitude(Integer x, Integer y) => new Integer(BigRational.MinMagnitude(x.p, y.p));

      //NET7 nonsense - emulation of the strange BigInteger behavior - !math
      public static Integer operator >>>(Integer value, int shift)
      {
        if (shift <= 0) return shift != 0 ? value << -shift : value;
        var cpu = main_cpu; cpu.push(value.p);
        if (cpu.sign() == -1) { cpu.toc(4); if (shift >= cpu.msb()) { cpu.pop(); return MinusOne; } }
        cpu.shr(shift); return new Integer(cpu);
      }

      public static implicit operator Integer(byte value) => new Integer((uint)value);
      public static implicit operator Integer(char value) => new Integer((uint)value);
      public static implicit operator Integer(short value) => new Integer((int)value);
      public static implicit operator Integer(sbyte value) => new Integer((int)value);
      public static implicit operator Integer(ushort value) => new Integer((uint)value);
      public static implicit operator Integer(ulong value) => new Integer(value);
      public static implicit operator Integer(nuint value) => new Integer(value);
      public static implicit operator Integer(nint value) => new Integer(value);
      public static implicit operator Integer(Int128 value) => new Integer(value);
      public static implicit operator Integer(UInt128 value) => new Integer(value);
      public static explicit operator Integer(Half value) => new Integer((double)value);
      public static explicit operator Integer(float value) => new Integer((float)value);
      public static explicit operator Integer(double value) => new Integer(value);
      public static explicit operator Integer(decimal value) => new Integer(value);
      public static explicit operator Integer(Complex value) => value.Imaginary == 0 ? new Integer(value.Real) : throw new OverflowException();

      public static explicit operator Int128(Integer value) => (Int128)value.p;
      public static explicit operator UInt128(Integer value) => (UInt128)value.p;
      public static explicit operator Complex(Integer value) => new Complex((double)value.p, 0);

      // INumber, IBinaryInteger<BigInt>, ISignedNumber<BigInt>
      static Integer IBitwiseOperators<Integer, Integer, Integer>.operator |(Integer left, Integer right)
      {
        var cpu = main_cpu; cpu.push(left.p); cpu.push(right.p); cpu.or(); return new Integer(cpu);
      }

      [DebuggerBrowsable(DebuggerBrowsableState.Never)] static int INumberBase<Integer>.Radix => 1; //NET7 BigInteger nonsense Radix = 2
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] static Integer IAdditiveIdentity<Integer, Integer>.AdditiveIdentity => Zero;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] static Integer IMultiplicativeIdentity<Integer, Integer>.MultiplicativeIdentity => One;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] static Integer ISignedNumber<Integer>.NegativeOne => MinusOne;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] static Integer IBinaryNumber<Integer>.AllBitsSet => MinusOne; //NET7 BigInteger nonsense

      int IBinaryInteger<Integer>.GetByteCount() => GetByteCount();
      int IBinaryInteger<Integer>.GetShortestBitLength() => GetByteCount();
      bool IBinaryInteger<Integer>.TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
        => TryWriteBytes(destination, out bytesWritten, false, true);
      bool IBinaryInteger<Integer>.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
        => TryWriteBytes(destination, out bytesWritten, false, false);
      static bool IBinaryInteger<Integer>.TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Integer value)
      {
        throw new NotImplementedException(); //todo: impl
      }
      static bool IBinaryInteger<Integer>.TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out Integer value)
      {
        throw new NotImplementedException(); //todo: impl
      }

      static bool INumberBase<Integer>.IsCanonical(Integer value) => true;
      static bool INumberBase<Integer>.IsComplexNumber(Integer value) => false;
      static bool INumberBase<Integer>.IsZero(Integer value) => value.IsZero;
      static bool INumberBase<Integer>.IsFinite(Integer value) => false;
      static bool INumberBase<Integer>.IsImaginaryNumber(Integer value) => false;
      static bool INumberBase<Integer>.IsInfinity(Integer value) => false;
      static bool INumberBase<Integer>.IsInteger(Integer value) => true;
      static bool INumberBase<Integer>.IsNaN(Integer value) => false;
      static bool INumberBase<Integer>.IsNegativeInfinity(Integer value) => false;
      static bool INumberBase<Integer>.IsNormal(Integer value) => value.Sign != 0; //NET7 BigInteger nonsense
      static bool INumberBase<Integer>.IsPositiveInfinity(Integer value) => false;
      static bool INumberBase<Integer>.IsRealNumber(Integer value) => true;
      static bool INumberBase<Integer>.IsSubnormal(Integer value) => false;
      static Integer INumberBase<Integer>.MaxMagnitudeNumber(Integer x, Integer y) => new Integer(BigRational.MaxMagnitudeNumber(x.p, y.p));
      static Integer INumberBase<Integer>.MinMagnitudeNumber(Integer x, Integer y) => new Integer(BigRational.MinMagnitudeNumber(x.p, y.p));

      static int INumber<Integer>.Sign(Integer value) => Sign(value);
      static Integer INumber<Integer>.MaxNumber(Integer x, Integer y) => Max(x, y);
      static Integer INumber<Integer>.MinNumber(Integer x, Integer y) => Min(x, y);

      static bool INumberBase<Integer>.TryConvertFromChecked<T>(T value, out Integer result)
      {
        static bool f<R>(T v, out R result) where R : INumberBase<R> => R.TryConvertFromChecked(v, out result!);
        if (!f<BigRational>(value, out var r)) { result = default; return false; }
        result = new Integer(BigRational.Truncate(r)); return true; //todo: check if spec is available     
      }
      static bool INumberBase<Integer>.TryConvertFromSaturating<T>(T value, out Integer result)
      {
        static bool f<R>(T v, out R result) where R : INumberBase<R> => R.TryConvertFromSaturating(v, out result!);
        if (!f<BigRational>(value, out var r)) { result = default; return false; }
        result = new Integer(BigRational.Truncate(r)); return true; //todo: check if spec is available
      }
      static bool INumberBase<Integer>.TryConvertFromTruncating<T>(T value, out Integer result)
      {
        static bool f<R>(T v, out R result) where R : INumberBase<R> => R.TryConvertFromTruncating(v, out result!);
        if (!f<BigRational>(value, out var r)) { result = default; return false; }
        result = new Integer(BigRational.Truncate(r)); return true; //todo: check if spec is available
      }
      static bool INumberBase<Integer>.TryConvertToChecked<T>(Integer value, [NotNullWhen(true)] out T result)
      {
        static bool f<R>(R v, out T result) where R : INumberBase<R> => R.TryConvertToChecked(v, out result!);
        return f<BigRational>(value.p, out result); //ok
      }
      static bool INumberBase<Integer>.TryConvertToSaturating<T>(Integer value, [NotNullWhen(true)] out T result)
      {
        static bool f<R>(R v, out T result) where R : INumberBase<R> => R.TryConvertToSaturating(v, out result!);
        return f<BigRational>(value.p, out result); //ok
      }
      static bool INumberBase<Integer>.TryConvertToTruncating<T>(Integer value, [NotNullWhen(true)] out T result)
      {
        static bool f<R>(R v, out T result) where R : INumberBase<R> => R.TryConvertToTruncating(v, out result!);
        return f<BigRational>(value.p, out result); //ok
      }
    }
#endif
  }
}
