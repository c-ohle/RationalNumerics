
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

//todo: nans, infs, upcast rnd, parse rnd, sqr 

#pragma warning disable CS1591  //xml comments

namespace System.Numerics
{
  unsafe partial struct BigRational
  {
    /// <summary>
    /// Represents a sizeof&lt;<typeparamref name="T"/>&gt;-precision-floating floating-point number. <b>(under construction)</b>
    /// </summary>
    /// <remarks>
    /// Template for <seealso href="https://en.wikipedia.org/wiki/IEEE_754">IEEE 754</seealso> floating-point numbers.<br/>
    /// Using struct <typeparamref name="T"/> as data storage and the <see cref="CPU"/> as calculation core.<br/>
    /// The typeof(<typeparamref name="T"/>) itself is meaningless.<br/> 
    /// The sizeof(<typeparamref name="T"/>) defines the type type and precision e.g.:<br/><br/>
    /// <c>BigRational.Float&lt;UInt64&gt;</c> defines a Double precision type.<br/>
    /// <c>BigRational.Float&lt;UInt128&gt;</c> defines a Quadruple precision type.<br/>
    /// <c>BigRational.Float&lt;UInt256&gt;</c> defines a Octuple precision type.<br/>
    /// </remarks>
    /// <typeparam name="T">Any unmanaged value structure type to store the number bits.</typeparam>
    [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
    public unsafe readonly partial struct Float<T> : IComparable<Float<T>>, IComparable, IEquatable<Float<T>> where T : unmanaged
    {
      public readonly override string ToString() => ToString(default, null);
      public readonly string ToString(string? format, IFormatProvider? provider = default)
      {
        if (format != default && format.Length == 0) provider = NumberFormatInfo.InvariantInfo;
        Span<char> sp = stackalloc char[Digits + 16];
        if (!TryFormat(sp, out var ns, format, provider))
        {
          int n; sp.Slice(0, 2).CopyTo(new Span<char>(&n, 2)); sp = stackalloc char[n]; //todo: bigbuf
          TryFormat(sp, out ns, format, provider); Debug.Assert(ns != 0);
        }
        return sp.Slice(0, ns).ToString();
      }
      public readonly bool TryFormat(Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
      {
        var fmt = 'G'; int dig = 0, rnd = 0; var info = NumberFormatInfo.GetInstance(provider);
        if (format.Length != 0)
        {
          var f = (fmt = format[0]) & ~0x20; var d = format.Length > 1;
          if (d) dig = int.Parse(format.Slice(1));//, NumberFormatInfo.InvariantInfo);
          if (f == 'E') { rnd = dig; if (rnd == 0 && !d) rnd = 6; dig = rnd + 1; }
          if (f == 'F') { rnd = dig; if (rnd == 0 && !d) rnd = info.NumberDecimalDigits; dig = 0; }
        }
        if (dig == 0) dig = Digits; // (int)MathF.Round(MathF.Ceiling(mbi * 0.30103f));
        if (dest.Length < dig + 16) { dig += 16; goto ex; }
        var cpu = main_cpu; var p = this.p; var es = 0;
        var e = push(cpu, (uint*)&p);

        var ep = (int)((e + mbi) * 0.30102999566398114); //Math.Log(2) / Math.Log(10) Math.Log10(2);
        var d1 = Math.Abs(ep); var d2 = Math.Abs(dig); var dd = d1 - d2;
        if (dd > 10)// && provider == null) //todo: opt. F? check
        {
          if (dig <= 0) { }
          else if (ep > 0)
          {
            cpu.pop(); *(Float<T>*)&p = this * pow10(cpu, 3 - dd);
            e = push(cpu, (uint*)&p); es = dd - 3;
          }
          else if (ep < 0)
          {
            cpu.pop(); *(Float<T>*)&p = this * pow10(cpu, dd - 3); //todo: opt. bias 
            e = push(cpu, (uint*)&p); es = 3 - dd;
          }
          else { }
        }

        cpu.pow(2, e); cpu.mul(); //(char fmt,int dig,int rnd,int ex,int fl)
        var n = tos(dest, cpu, fmt, dig, rnd, es, info.NumberDecimalSeparator[0] == ',' ? 0x04 : 0);
        if (n < 0) { dig = -n; goto ex; }
        charsWritten = n; return true; ex:
        charsWritten = 0; if (dest.Length >= 2) new Span<char>(&dig, 2).CopyTo(dest); return false;
      }
      public readonly override int GetHashCode()
      {
        return p.GetHashCode(); //todo: own impl! 
      }
      public readonly bool Equals(Float<T> other) => this == other;
      public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is Float<T> t ? this == t : false;
      public readonly int CompareTo(Float<T> b) => Compare(this, b);
      public readonly int CompareTo(object? p) => p == null ? 1 : p is Float<T> b ? Compare(this, b) : throw new ArgumentException();
      public static int Compare(Float<T> a, Float<T> b)
      {
        return Compare((uint*)&a, (uint*)&b);
      }
      public static Float<T> Parse(ReadOnlySpan<char> value, IFormatProvider? provider = null) { TryParse(value, provider, out var r); return r; }
      public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out Float<T> result)
      { //todo: rnd to digits in str
        var info = provider != null ? NumberFormatInfo.GetInstance(provider) : null;
        var cpu = main_cpu; cpu.tor(value, 10, info != null ? info.NumberDecimalSeparator[0] : default);
        result = pop(cpu); return true;
      }

      public static implicit operator Float<T>(sbyte value) => (Float<T>)(int)value; //todo: rem, after Half support is gone
      public static explicit operator Float<T>(int value)
      {
        var cpu = main_cpu; cpu.push(value);
        Float<T> c; pop(cpu, 0, (uint*)&c); return c;
      }
      //todo: check, explicit - better implicite with NaN returns ?   
      public static explicit operator Float<T>(Half value)
      {
        //if (sizeof(T) == 2) return *(Float<T>*)&value; //todo: enable  after  tests
        return Float<UInt16>.Cast<T>(*(Float<UInt16>*)&value);
        //var cpu = main_cpu; var e = Float<UInt16>.push(cpu, (uint*)&value);
        //Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static explicit operator Float<T>(float value)
      {
        //if (sizeof(T) == 4) return *(Float<T>*)&value; //todo: enable  after  tests
        return Float<UInt32>.Cast<T>(*(Float<UInt32>*)&value);
        //var cpu = main_cpu; var e = Float<UInt32>.push(cpu, (uint*)&value);
        //Float<T> c; pop(cpu, e, (uint*)&c);
        //if (Float<T>.Digits > (e = Float<UInt32>.Digits)) c = Float<T>.Round(c, e); return c; //return c;
      }
      public static explicit operator Float<T>(double value)
      {
        //if (sizeof(T) == 8) return *(Float<T>*)&value; //todo: enable  after  tests
        return Float<UInt64>.Cast<T>(*(Float<UInt64>*)&value);
        //var cpu = main_cpu; var e = Float<UInt64>.push(cpu, (uint*)&value);
        //Float<T> c; pop(cpu, e, (uint*)&c);
        //if (Float<T>.Digits > (e = Float<UInt64>.Digits)) c = Float<T>.Round(c, e); return c;
      }
      public static explicit operator Float<T>(decimal value)
      {
        var cpu = main_cpu; cpu.push(value); return pop(cpu);
      }
      public static explicit operator Float<T>(BigRational value)
      {
        var cpu = main_cpu; cpu.push(value); return pop(cpu);
      }

      public static explicit operator int(Float<T> value)
      {
        return (int)(double)value;
      }
      public static explicit operator Half(Float<T> value)
      {
        return (Half)(double)value;
      }
      public static explicit operator float(Float<T> value)
      {
        return (float)(double)value;
      }
      public static explicit operator double(Float<T> value)
      {
        var cpu = main_cpu; var e = push(cpu, (uint*)&value);
        Float<UInt64> c; Float<UInt64>.pop(cpu, e, (uint*)&c); return *(double*)&c;
      }
      public static explicit operator decimal(Float<T> value)
      {
        return (decimal)(BigRational)value; //todo: inline
      }
      public static implicit operator BigRational(Float<T> value)
      {
        var cpu = main_cpu; var e = push(cpu, (uint*)&value.p); //todo: e > mbi
        cpu.pow(2, e); cpu.mul(); return cpu.popr();
      }

      public static Float<T> operator +(Float<T> a) => a;
      public static Float<T> operator -(Float<T> a)
      {
        uint* u = (uint*)&a; if ((sizeof(T) & 2) == 0) u[lui] ^= 0x80000000; else u[lui] ^= 0x8000; return a;
      }
      public static Float<T> operator ++(Float<T> a)
      {
        return a + +1;
      }
      public static Float<T> operator --(Float<T> a)
      {
        return a + -1;
      }
      public static Float<T> operator +(Float<T> a, Float<T> b)
      {
        var cpu = main_cpu; int ae = push(cpu, (uint*)&a), be = push(cpu, (uint*)&b), l = ae < be ? 1 : 0, e;
        cpu.shr(l == 0 ? (e = ae) - be : (e = be) - ae, l); cpu.add();
        Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static Float<T> operator +(Float<T> a, sbyte b)
      {
        var cpu = main_cpu; int ae = push(cpu, (uint*)&a), l = ae < 0 ? 1 : 0, e;
        cpu.push(b); cpu.shr(l == 0 ? (e = ae) : (e = 0) - ae, l); cpu.add();
        Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static Float<T> operator -(Float<T> a, Float<T> b)
      {
        var cpu = main_cpu; int ae = push(cpu, (uint*)&a), be = push(cpu, (uint*)&b), l = ae < be ? 1 : 0, e;
        cpu.shr(l == 0 ? (e = ae) - be : (e = be) - ae, l); cpu.sub();
        Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static Float<T> operator *(Float<T> a, Float<T> b)
      {
        var cpu = main_cpu; uint* u = (uint*)&a, v = (uint*)&b;
        int eu = push(cpu, u), ev = push(cpu, v); cpu.mul();
        Float<T> c; pop(cpu, eu + ev, (uint*)&c); return c;
      }
      public static Float<T> operator /(Float<T> a, Float<T> b)
      {
        var cpu = main_cpu;
        var eu = push(cpu, (uint*)&a); cpu.shl(mbi);
        var ev = push(cpu, (uint*)&b); cpu.idiv();
        Float<T> c; pop(cpu, eu - ev - mbi, (uint*)&c); return c;
      }
      public static Float<T> operator %(Float<T> a, Float<T> b)
      {
        return a - Truncate(a / b) * b; //todo: opt. inline
      }

      public static bool operator ==(Float<T> a, Float<T> b)
      {
        return Equals((uint*)&a, (uint*)&b);
      }
      public static bool operator !=(Float<T> a, Float<T> b)
      {
        return !Equals((uint*)&a, (uint*)&b);
      }
      public static bool operator <=(Float<T> a, Float<T> b)
      {
        return Compare((uint*)&a, (uint*)&b) <= 0;
      }
      public static bool operator >=(Float<T> a, Float<T> b)
      {
        return Compare((uint*)&a, (uint*)&b) >= 0;
      }
      public static bool operator <(Float<T> a, Float<T> b)
      {
        return Compare((uint*)&a, (uint*)&b) < 0;
      }
      public static bool operator >(Float<T> a, Float<T> b)
      {
        return Compare((uint*)&a, (uint*)&b) > 0;
      }

      public static Float<B> Cast<B>(Float<T> a) where B : unmanaged
      {
        var cpu = main_cpu; var e = push(cpu, (uint*)&a);
        //  int b, t;
        //  if ((b = Float<B>.mbi) > (t = Float<T>.mbi))
        //  {
        //    //var x1 = (int)Math.Log(b, 2); //34
        //    //var x2 = (int)Math.Log(t, 2); //15
        //    //e -= x1 - x2;
        //  }
        Float<B> c; Float<B>.pop(cpu, e, (uint*)&c);
        return c;
      }

      public static Float<T> Pi => (Float<T>)BigRational.Pi(Digits);
      public static Float<T> Tau => (Float<T>)BigRational.Tau(Digits);
      public static Float<T> E => (Float<T>)BigRational.Exp(1, Digits);
      public static Float<T> MaxValue
      {
        get
        {
          Float<T> u; new Span<byte>(&u, sizeof(T)).Fill(0xff);
          ref var p = ref ((uint*)&u)[lui]; p = 0x7fffffffu ^ (1u << ((mbi - 1) & 31));
          if ((sizeof(T) & 2) != 0) p = sizeof(T) != 2 ? p >> 16 : 0x7bff;
          return u;
        }
      }
      public static Float<T> MinValue
      {
        get { return -MaxValue; }
      }

      #region private
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private readonly T p;
      private static int push(CPU cpu, uint* p)
      {
        var l = lui; if ((sizeof(T) & 2) != 0) if (l != 0) p[l] <<= 16; else p[0] = (p[0] & 0x03FFu) | ((p[0] & ~0x03FFu) << 16);
        if (p[l] == 0) { cpu.push(); return 0; }
        var t = stackalloc uint[l + 4]; var x = 1u << (mbi - 1);
        t[0] = (p[l] & 0x80000000) | unchecked((uint)l + 1);
        t[l + 1] = (p[l] & (x - 1)) | x; t[l + 2] = t[l + 3] = 1;
        for (int i = 0; i < l; i++) t[i + 1] = p[i];
        cpu.push(new ReadOnlySpan<uint>(t, l + 4));
        return unchecked((int)((p[l] >> (31 - sbi)) & ((1 << sbi) - 1))) - bia;
      }
      private static void pop(CPU cpu, int e, uint* p)
      {
        int s = unchecked((int)cpu.msb()); if (s == 0) { cpu.pop(); *(Float<T>*)p = default; return; }
        int d = s - mbi, l; //todo: the point inf, nan starts
        if (d > 0) { var c = cpu.bt(d - 1); cpu.shr(d); if (c) cpu.inc(); } //x87 rnd
        else if (d != 0) cpu.shl(-d);
        var sp = cpu.gets(cpu.mark() - 1);
        p[l = lui] = (sp[0] & 0x80000000) | (unchecked((uint)((e + d) + bia)) << (31 - sbi)) | (sp[l + 1] & ((1u << (mbi - 1)) - 1u));
        if ((sizeof(T) & 2) != 0) if (l != 0) p[l] >>= 16; else p[l] = (p[l] & 0xffff) | p[l] >> 16;
        for (int i = 0; i < l; i++) p[i] = sp[i + 1];
        cpu.pop();
      }
      private static Float<T> pop(CPU cpu)
      {
        Float<T> a, b; var s = cpu.sign(); if (s == 0) { cpu.pop(); return default; }
        cpu.mod(8); pop(cpu, 0, (uint*)&a); pop(cpu, 0, (uint*)&b); a = b / a; return a;

        //var s = cpu.sign(); if (s <= 0) { if (s == 0) return default; cpu.neg(); }
        //var w = cpu.cmpa(); if (w > 0) { cpu.swp(); }
        //int m = mbi; //if (s < 0) { cpu.neg(); cpu.neg(1); }
        //int j, k, u = m - unchecked(j = (int)cpu.msb());
        //cpu.shl(u); cpu.swp(); int v = (m << 1) - unchecked(k = (int)cpu.msb());
        //cpu.shl(v); cpu.swp(); cpu.idiv(); if (s < 0) cpu.neg();
        //Float<T> c; pop(cpu, k - j - m, (uint*)&c); return c;
      }
      private static Float<T> pow10(CPU cpu, int y)
      {
        Float<T> x = 1, z = 10; uint e = unchecked((uint)(y >= 0 ? y : -y));
        for (; ; e >>= 1)
        {
          if ((e & 1) != 0) x *= z;
          if (e <= 1) break; z = z * z; //todo: sqr
        }
        if (y < 0) x = 1 / x; //static float inv(float x) { uint* i = (uint*)&x; *i = 0x7F000000 - *i; return x; }
        return x;
      }
      //private static Float<T> sqr(CPU cpu, Float<T> a) => a * a;
      private static bool Equals(uint* a, uint* b)
      {
        if (a == b) return true; //sort's
        uint n = unchecked((uint)sizeof(T)), i = 0, c;
        for (c = n >> 2; i < c; i++) if (a[i] != b[i]) return false;
        if (c << 2 < n) if (*(ushort*)&b[c] != *(ushort*)&b[c]) return false;
        return true;
      }
      private static int Compare(uint* a, uint* b) //todo: check and opt.
      {
        if (a == b) return 0; //sort's
        var ha = (sizeof(T) & 2) == 0 ? a[lui] : (a[lui] & 0xffff) << 16;
        var hb = (sizeof(T) & 2) == 0 ? b[lui] : (b[lui] & 0xffff) << 16;
        var sa = ha == 0 ? 0 : (ha & 0x80000000) != 0 ? -1 : +1;
        var sb = hb == 0 ? 0 : (hb & 0x80000000) != 0 ? -1 : +1;
        if (sa != sb) return Math.Sign(sa - sb); if (sa == 0) return 0;
        var cpu = main_cpu; int ea = push(cpu, a), eb = push(cpu, b);
        if (ea != eb) { cpu.pop(2); return ea > eb ? sa : -sa; }
        ea = cpu.cmp(); cpu.pop(2); return ea * sa;
      }
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private static readonly int sbi, mbi, bia, lui; //todo: inline, for NET 7 type desc
      static Float()
      {
        int size = sizeof(T); if ((size & 1) != 0) throw new NotSupportedException(nameof(T));
        switch (size) { case 2: sbi = 5; break; case 4: sbi = 8; break; case 8: sbi = 11; break; case <= 16: sbi = 15; break; default: sbi = BitOperations.Log2(unchecked((uint)size)) * 3 + 4; break; }
        mbi = (size << 3) - sbi; bia = (1 << (sbi - 1)) + mbi - 2; lui = (size >> 2) - 1 + ((size & 2) >> 1); //check();
                                                                                                              //Debug.WriteLine($"Float{size << 3} significand bits {mbi} exponent bits {((sizeof(T) << 3) - mbi) - 1} decimal digits {mbi * MathF.Log10(2):0.##}");
      }
      #endregion

      internal static int Bits => sizeof(T) << 3;
      internal static int BiasedExponentShift => mbi - 1; // 52
      internal static int ExponentBits => (sizeof(T) << 3) - mbi; // 11
      internal static int MaxBiasedExponent => (1 << sbi) - 1; // 0x07FF;
      internal static int ExponentBias => (1 << (ExponentBits - 1)) - 1; // 1023
      internal static int MinExponent => 1 - ExponentBias; // -1022;
      internal static int MaxExponent => ExponentBias; // +1023;
                                                       //internal static float DecimalDigits => mbi * MathF.Log10(2);
                                                       //internal static int Digits2 => sizeof(T) != 2 ? (int)MathF.Round(MathF.Ceiling(mbi * 0.30103f)) : 5;
      internal static int Digits => sizeof(T) != 2 ? (int)((mbi) * 0.30102999566398114) : 5;
      //static void check()
      //{
      //  //if ((fltinits & (1ul << (sizeof(T) >> 1))) == 0) fltinits |= (1ul << (sizeof(T) >> 1)); else { }
      //  switch (sizeof(T))
      //  {
      //    case 02: Debug.Assert(Bits == 016 && mbi == 011 && ExponentBits == 05 && MinExponent == -000014 && MaxExponent == 000015); break;
      //    case 04: Debug.Assert(Bits == 032 && mbi == 024 && ExponentBits == 08 && MinExponent == -000126 && MaxExponent == 000127); break;
      //    case 08: Debug.Assert(Bits == 064 && mbi == 053 && ExponentBits == 11 && MinExponent == -001022 && MaxExponent == 001023); break;
      //    case 10: Debug.Assert(Bits == 080 && mbi == 065 && ExponentBits == 15 && MinExponent == -016382 && MaxExponent == 016383); break;
      //    case 12: Debug.Assert(Bits == 096 && mbi == 081 && ExponentBits == 15 && MinExponent == -016382 && MaxExponent == 016383); break;
      //    case 16: Debug.Assert(Bits == 128 && mbi == 113 && ExponentBits == 15 && MinExponent == -016382 && MaxExponent == 016383); break;
      //    case 32: Debug.Assert(Bits == 256 && mbi == 237 && ExponentBits == 19 && MinExponent == -262142 && MaxExponent == 262143); break;
      //  }
      //}
    }

    // func's spec, currently mapped many times to BigRational, slow but precise 
    public unsafe readonly partial struct Float<T>
    {
      /// <summary> <see cref="Float{T}"/>
      /// Gets a <see cref="int"/> number that indicates the sign 
      /// (negative, positive, or zero) of a <see cref="Float{T}"/> number.
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number.</param>
      /// <returns>
      /// A <see cref="int"/> that indicates the sign of the <see cref="Float{T}"/> number, as
      /// shown in the following table.<br/>
      /// Number – Description<br/>
      /// -1 – The value of the object is negative.<br/>
      ///  0 – The value of the object is 0 (zero).<br/>
      /// +1 – The value of the object is positive.<br/>
      /// </returns>
      public static int Sign(Float<T> a)
      {
        var u = (uint*)&a; var h = (sizeof(T) & 2) == 0 ? u[lui] : (u[lui] & 0xffff) << 16;
        return h == 0 ? 0 : (h & 0x80000000) != 0 ? -1 : +1;
      }
      /// <summary>
      /// Determines if the value is NaN. 
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number.</param>
      /// <returns><c>true</c> if <paramref name="a" /> is NaN; otherwise, <c>false</c>.</returns>
      public static bool IsNaN(Float<T> a) => false;
      /// <summary>
      /// Determines whether the specified value is finite (zero, subnormal, or normal).
      /// </summary>
      public static bool IsFinite(Float<T> a) => true;
      /// <summary>
      /// Returns a value indicating whether the specified number is an integer. 
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number.</param>
      /// <returns><c>true</c> if <paramref name="a" /> is an integer; otherwise, <c>false</c>.</returns>
      public static bool IsInteger(Float<T> a) => IsFinite(a) && a == Truncate(a);
      /// <summary>
      /// Gets the absolute value of a <see cref="Float{T}"/> number.
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number as value.</param>
      /// <returns>The absolute value of the <see cref="Float{T}"/> number.</returns>
      public static Float<T> Abs(Float<T> a)
      {
        return Sign(a) < 0 ? -a : a;
      }
      /// <summary>
      /// Returns the smaller of two <see cref="Float{T}"/> numbers.
      /// </summary>
      /// <param name="a">The first value to compare.</param>
      /// <param name="b">The second value to compare.</param>
      /// <returns>The a or b parameter, whichever is smaller.</returns>
      public static Float<T> Min(Float<T> a, Float<T> b)
      {
        return Compare((uint*)&a, (uint*)&b) <= 0 ? a : b;
      }
      /// <summary>
      /// Returns the larger of two <see cref="Float{T}"/> numbers.
      /// </summary>
      /// <param name="a">The first value to compare.</param>
      /// <param name="b">The second value to compare.</param>
      /// <returns>The a or b parameter, whichever is larger.</returns>
      public static Float<T> Max(Float<T> a, Float<T> b)
      {
        return Compare((uint*)&a, (uint*)&b) >= 0 ? a : b;
      }
      /// <summary>
      /// Calculates the integral part of the <see cref="Float{T}"/> number.
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number as value to truncate.</param>
      /// <returns>
      /// The integral part of the <see cref="Float{T}"/> number.<br/> 
      /// This is the number that remains after any fractional digits have been discarded.
      /// </returns>
      public static Float<T> Truncate(Float<T> a)
      {
        var cpu = main_cpu; var e = push(cpu, (uint*)&a); cpu.shl(e);
        Float<T> c; pop(cpu, 0, (uint*)&c); return c;
      }
      /// <summary>
      /// Rounds a specified <see cref="Float{T}"/> number to the closest integer toward negative infinity.
      /// </summary>
      /// The <see cref="Float{T}"/> number to round.
      /// <returns>
      /// If <paramref name="a"/> has a fractional part, the next whole number toward negative
      /// infinity that is less than <paramref name="a"/>.<br/>
      /// or if <paramref name="a"/> doesn't have a fractional part, <paramref name="a"/> is returned unchanged.<br/>
      /// </returns>
      public static Float<T> Floor(Float<T> a) => (Float<T>)BigRational.Floor((BigRational)a);
      /// <summary>
      /// Returns the smallest integral value that is greater than or equal to the specified number.
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number.</param>
      /// <returns>
      /// The smallest integral value that is greater than or equal to the <paramref name="a"/> parameter.
      /// </returns>
      public static Float<T> Ceiling(Float<T> a) => (Float<T>)BigRational.Ceiling((BigRational)a);
      // /// <summary>
      // /// Rounds a <see cref="Float{T}"/> number to the nearest integral value
      // /// and rounds midpoint values to the nearest even number.
      // /// </summary>
      // /// <param name="a">A <see cref="Float{T}"/> number to be rounded.</param>
      // /// <returns></returns>
      // public static Float<T> Round(Float<T> a)
      // { 
      //   retuurn (Float<T>)BigRational.Round((BigRational)a);
      // }
      /// <summary>
      /// Rounds a <see cref="Float{T}"/> number to a specified number of fractional digits 
      /// using the specified rounding convention.
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number to be rounded.</param>
      /// <param name="digits">The number of decimal places in the return value.</param>
      /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
      /// <returns>
      /// The <see cref="Float{T}"/> number with decimals fractional digits that the value is rounded to.<br/> 
      /// If the number has fewer fractional digits than decimals, the number is returned unchanged.
      /// </returns>
      public static Float<T> Round(Float<T> a, int digits = 0, MidpointRounding mode = MidpointRounding.ToEven)
      {
        if (digits == 0) return Truncate(a);
        var cpu = main_cpu; var b = pow10(cpu, digits);
        a *= b; var e = push(cpu, (uint*)&a); cpu.shr(-e);
        pop(cpu, 0, (uint*)&a); a /= b; return a; // (Float<T>)BigRational.Round((BigRational)a, digits, mode);
      }
      /// <summary>
      /// Returns a specified number raised to the specified power.<br/>
      /// For fractional exponents, the result is rounded to the specified number of decimal places.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">A <see cref="Float{T}"/> number to be raised to a power.</param>
      /// <param name="y">A <see cref="Float{T}"/> number that specifies a power.</param>
      /// <returns>
      /// The <see cref="Float{T}"/> number <paramref name="x"/> raised to the power <paramref name="y"/>.<br/>
      /// NaN if <paramref name="x"/> is less zero and <paramref name="y"/> is fractional.
      /// </returns>
      public static Float<T> Pow(Float<T> x, Float<T> y)
      {
        return (Float<T>)BigRational.Pow((BigRational)x, (BigRational)y, Digits);
      }
      /// <summary>
      /// Returns a specified number raised to the specified power.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">A <see cref="Float{T}"/> number to be raised to a power.</param>
      /// <returns>The <see cref="Float{T}"/> number a raised to the power b.</returns>
      public static Float<T> Pow2(Float<T> x) => (Float<T>)BigRational.Pow2((BigRational)x, Digits);
      /// <summary>
      /// Returns the square root of a specified number.
      /// </summary>
      /// <remarks>
      /// For fractional roots, the result is rounded to the specified number of decimal places.
      /// </remarks>
      /// <param name="a">The number whose square root is to be found.</param>
      /// <returns>
      /// Zero or positive – The positive square root of <paramref name="a"/>.<br/>
      /// NaN if <paramref name="a"/> is less zero.
      /// </returns>
      public static Float<T> Sqrt(Float<T> a) => (Float<T>)BigRational.Sqrt((BigRational)a, Digits);
      /// <summary>Computes the cube-root of a value.</summary>
      /// <param name="x">The value whose cube-root is to be computed.</param>
      /// <returns>The cube-root of <paramref name="x" />.</returns>
      public static Float<T> Cbrt(Float<T> x) => (Float<T>)BigRational.Cbrt((BigRational)x, Digits);
      /// <summary>Computes the n-th root of a value.</summary>
      /// <param name="x">The value whose <paramref name="n" />-th root is to be computed.</param>
      /// <param name="n">The degree of the root to be computed.</param>
      /// <returns>The <paramref name="n" />-th root of <paramref name="x" />.</returns>
      public static Float<T> RootN(Float<T> x, int n) => (Float<T>)BigRational.RootN((BigRational)x, n, Digits);
      /// <summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
      /// <param name="x">The value to square and add to <paramref name="y" />.</param>
      /// <param name="y">The value to square and add to <paramref name="x" />.</param>
      /// <returns>The square root of <paramref name="x" />-squared plus <paramref name="y" />-squared.</returns>
      public static Float<T> Hypot(Float<T> x, Float<T> y) => (Float<T>)BigRational.Hypot((BigRational)x, (BigRational)y, Digits);
      /// <summary>
      /// Returns the base 2 logarithm of a specified number.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">The number whose logarithm is to be found.</param>
      /// <returns>
      /// The base 2 logarithm of <paramref name="x"/>.<br/>
      /// NaN if <paramref name="x"/> is less or equal zero.
      /// </returns>
      public static Float<T> Log2(Float<T> x) => (Float<T>)BigRational.Log2((BigRational)x, Digits);
      /// <summary>
      /// Returns the base 10 logarithm of a specified number.
      /// </summary>
      /// <param name="x">The number whose logarithm is to be found.</param>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <returns>The base 10 logarithm of <paramref name="x"/>.</returns>
      /// <exception cref="ArgumentException">For <paramref name="x"/> is less or equal zero.</exception>
      public static Float<T> Log10(Float<T> x) => (Float<T>)BigRational.Log10((BigRational)x, Digits);
      /// <summary>
      /// Returns the natural (base e) logarithm of a specified number.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">The number whose logarithm is to be found.</param>
      /// <returns>
      /// The natural logarithm of <paramref name="x"/>; that is, <c>ln <paramref name="x"/></c>, or <c>log e <paramref name="x"/></c>.<br/>
      /// NaN if <paramref name="x"/> is less or equal zero.
      /// </returns>
      public static Float<T> Log(Float<T> x) => (Float<T>)BigRational.Log((BigRational)x, Digits);
      /// <summary>Computes the logarithm of a value in the specified base.</summary>
      /// <param name="x">The value whose logarithm is to be computed.</param>
      /// <param name="newBase">The base in which the logarithm is to be computed.</param>
      /// <returns><c>log<sub><paramref name="newBase" /></sub>(<paramref name="x" />)</c></returns>
      public static Float<T> Log(Float<T> x, Float<T> newBase)
      {
        return (Float<T>)BigRational.Log((BigRational)x, (BigRational)newBase, Digits);
      }
      /// <summary>
      /// Returns e raised to the specified power.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">A number specifying a power.</param>
      /// <returns>
      /// The number e raised to the power <paramref name="x"/>.
      /// </returns>
      public static Float<T> Exp(Float<T> x) => (Float<T>)BigRational.Exp((BigRational)x, Digits);
      /// <summary>
      /// Returns the sine of the specified angle.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">An angle, measured in radians.</param>
      /// <returns>The sine of <paramref name="x"/>.</returns>
      public static Float<T> Sin(Float<T> x) => (Float<T>)BigRational.Sin((BigRational)x, Digits);
      /// <summary>
      /// Returns the cosine of the specified angle.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">An angle, measured in radians.</param>
      /// <returns>The cosine of <paramref name="x"/>.</returns>
      public static Float<T> Cos(Float<T> x) => (Float<T>)BigRational.Cos((BigRational)x, Digits);
      /// <summary>
      /// Returns the tangent of the specified angle.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">An angle, measured in radians.</param>
      /// <returns>The tangent of <paramref name="x"/>.</returns>
      public static Float<T> Tan(Float<T> x) => (Float<T>)BigRational.Tan((BigRational)x, Digits);
      /// <summary>
      /// Returns the angle whose sine is the specified number.
      /// </summary>
      /// <param name="x">A number representing a sine, where d must be greater than or equal to -1, but less than or equal to 1.</param>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <returns>
      /// An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2. 
      /// -or- NaN if <paramref name="x"/> &lt; -1 or <paramref name="x"/> &gt; 1.
      /// </returns>
      public static Float<T> Asin(Float<T> x) => (Float<T>)BigRational.Asin((BigRational)x, Digits);
      /// <summary>
      /// Returns the angle whose cosine is the specified number.
      /// </summary>
      /// <param name="x">A number representing a cosine, where d must be greater than or equal to -1, but less than or equal to 1.</param>
      /// <returns>
      /// An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2. 
      /// -or- NaN if <paramref name="x"/> &lt; -1 or <paramref name="x"/> &gt; 1.
      /// </returns>
      public static Float<T> Acos(Float<T> x) => (Float<T>)BigRational.Acos((BigRational)x, Digits);
      /// <summary>
      /// Returns the angle whose tangent is the specified number.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">A number representing a tangent.</param>
      /// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.</returns>
      public static Float<T> Atan(Float<T> x) => (Float<T>)BigRational.Atan((BigRational)x, Digits);
      /// <summary>
      /// Returns the angle whose tangent is the quotient of two specified numbers.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="y">The y coordinate of a point.</param>
      /// <param name="x">The x coordinate of a point.</param>
      /// <returns>
      /// An angle, θ, measured in radians, such that -π ≤ θ ≤ π, and tan(θ) = y / x, where
      /// (x, y) is a point in the Cartesian plane.<br/>Observe the following:<br/>
      /// - For (x, y) in quadrant 1, 0 &lt; θ &lt; π/2.<br/> 
      /// - For (x, y) in quadrant 2, π/2 &lt; θ ≤ π.<br/> 
      /// - For (x, y) in quadrant 3, -π &lt; θ &lt; -π/2.<br/> 
      /// - For (x, y) in quadrant 4, -π/2 &lt; θ &lt; 0.<br/> 
      /// For points on the boundaries of the quadrants, the return value is the following:<br/>
      /// - If y is 0 and x is not negative, θ = 0.<br/> 
      /// - If y is 0 and x is negative, θ = π.<br/>
      /// - If y is positive and x is 0, θ = π/2<br/>
      /// - If y is negative and x is 0, θ = -π/2.<br/>
      /// - If y is 0 and x is 0, θ = 0.<br/>
      /// </returns>
      public static Float<T> Atan2(Float<T> y, Float<T> x) => (Float<T>)BigRational.Atan2((BigRational)y, (BigRational)x, Digits);

      /// <summary>Computes the hyperbolic arc-sine of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic arc-sine is to be computed.</param>
      /// <returns>The hyperbolic arc-sine of <paramref name="x" />.</returns>
      public static Float<T> Asinh(Float<T> x) => (Float<T>)BigRational.Asinh((BigRational)x, Digits);
      /// <summary>Computes the hyperbolic arc-cosine of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic arc-cosine is to be computed.</param>
      /// <returns>The hyperbolic arc-cosine of <paramref name="x" />.</returns>
      public static Float<T> Acosh(Float<T> x) => (Float<T>)BigRational.Acosh((BigRational)x, Digits);
      /// <summary>Computes the hyperbolic arc-tangent of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic arc-tangent is to be computed.</param>
      /// <returns>The hyperbolic arc-tangent of <paramref name="x" />.</returns>
      public static Float<T> Atanh(Float<T> x) => (Float<T>)BigRational.Atanh((BigRational)x, Digits);
      /// <summary>Computes the hyperbolic sine of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic sine is to be computed.</param>
      /// <returns>The hyperbolic sine of <paramref name="x" />.</returns>
      public static Float<T> Sinh(Float<T> x) => (Float<T>)BigRational.Asinh((BigRational)x, Digits);
      /// <summary>Computes the hyperbolic cosine of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic cosine is to be computed.</param>
      /// <returns>The hyperbolic cosine of <paramref name="x" />.</returns>
      public static Float<T> Cosh(Float<T> x) => (Float<T>)BigRational.Acosh((BigRational)x, Digits);
      /// <summary>Computes the hyperbolic tangent of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic tangent is to be computed.</param>
      /// <returns>The hyperbolic tangent of <paramref name="x" />.</returns>
      public static Float<T> Tanh(Float<T> x) => (Float<T>)BigRational.Tanh((BigRational)x, Digits);
    }

#if NET7_0

    public unsafe readonly partial struct Float<T> : IMinMaxValue<Float<T>>, IBinaryFloatingPointIeee754<Float<T>>
    {
      #region todo //todo: Float<T>
      static bool IsEvenInteger(Float<T> value) => IsInteger(value) && (Abs(value % 2) == 0);
      static bool IsInfinity(Float<T> value) => false;
      static bool IsNegative(Float<T> value) => false;
      static bool IsNegativeInfinity(Float<T> value) => false;
      static bool IsOddInteger(Float<T> value) => false;
      static bool IsPositive(Float<T> value) => false;
      static bool IsPositiveInfinity(Float<T> value) => false;
      static bool IsRealNumber(Float<T> value) => false;
      static bool IsSubnormal(Float<T> value) => false;
      static bool IsNormal(Float<T> value) => false;
      static bool IsZero(Float<T> value) => false;
      static bool IsPow2(Float<T> value) => false;
      static Float<T> bwand(Float<T> x, Float<T> y) => throw new NotImplementedException();
      static Float<T> bwor(Float<T> x, Float<T> y) => throw new NotImplementedException();
      static Float<T> bwxor(Float<T> x, Float<T> y) => throw new NotImplementedException();
      static Float<T> bwcmp(Float<T> x) => throw new NotImplementedException();
      static Float<T> Exp10(Float<T> x) => throw new NotImplementedException();
      static Float<T> Exp2(Float<T> x) => throw new NotImplementedException();
      static (Float<T> Sin, Float<T> Cos) SinCos(Float<T> x) => throw new NotImplementedException();
      static (Float<T> Sin, Float<T> Cos) SinCosPi(Float<T> x) => throw new NotImplementedException();
      static Float<T> MaxMagnitude(Float<T> x, Float<T> y) => throw new NotImplementedException();
      static Float<T> MaxMagnitudeNumber(Float<T> x, Float<T> y) => throw new NotImplementedException();
      static Float<T> MinMagnitude(Float<T> x, Float<T> y) => throw new NotImplementedException();
      static Float<T> MinMagnitudeNumber(Float<T> x, Float<T> y) => throw new NotImplementedException();
      static Float<T> Atan2Pi(Float<T> y, Float<T> x) => throw new NotImplementedException();
      static Float<T> BitDecrement(Float<T> x) => throw new NotImplementedException();
      static Float<T> BitIncrement(Float<T> x) => throw new NotImplementedException();
      static Float<T> FusedMultiplyAdd(Float<T> left, Float<T> right, Float<T> addend) => throw new NotImplementedException();
      static Float<T> Ieee754Remainder(Float<T> left, Float<T> right) => throw new NotImplementedException();
      static Float<T> ScaleB(Float<T> x, int n) => throw new NotImplementedException();
      static int ILogB(Float<T> x) => throw new NotImplementedException();
      static bool IBinaryNumber<Float<T>>.IsPow2(Float<T> value) => IsPow2(value);
      static Float<T> Epsilon => -1;//throw new NotImplementedException();
      static Float<T> NaN => -1;//throw new NotImplementedException();
      static Float<T> NegativeInfinity => -1;//throw new NotImplementedException();
      static Float<T> NegativeZero => -1;//throw new NotImplementedException();
      static Float<T> PositiveInfinity => -1;//throw new NotImplementedException();
      static Float<T> AllBitsSet => -1;//throw new NotImplementedException();
      readonly int GetExponentByteCount() => -1;//throw new NotImplementedException();
      readonly int GetExponentShortestBitLength() => -1;//throw new NotImplementedException();
      readonly int GetSignificandBitLength() => -1;//throw new NotImplementedException();
      readonly int GetSignificandByteCount() => -1;//throw new NotImplementedException();
      static bool TryWrite(Float<T> v, int fl, Span<byte> sp, out int bw) => throw new NotImplementedException();
      #endregion

      readonly string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(format, formatProvider);
      readonly bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => TryFormat(destination, out charsWritten, format, provider);
      public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Float<T> result)
      {
        //todo: style check
        var t = TryParse(s, provider, out result); return t;
      }
      static Float<T> ISpanParsable<Float<T>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, provider);
      static bool ISpanParsable<Float<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Float<T> result) => TryParse(s, provider, out result);
      static bool IParsable<Float<T>>.TryParse(string? s, IFormatProvider? provider, out Float<T> result) => TryParse(s, provider, out result);
      static Float<T> IParsable<Float<T>>.Parse(string s, IFormatProvider? provider) => Parse(s, provider);
      static Float<T> INumberBase<Float<T>>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) { TryParse(s, style, provider, out var r); return r; }
      static Float<T> INumberBase<Float<T>>.Parse(string s, NumberStyles style, IFormatProvider? provider) { TryParse(s, style, provider, out var r); return r; }
      static bool INumberBase<Float<T>>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Float<T> result) { var t = TryParse(s, style, provider, out result); return t; }
      static bool INumberBase<Float<T>>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Float<T> result) { var t = TryParse(s, style, provider, out result); return t; }

      static Float<T> IUnaryPlusOperators<Float<T>, Float<T>>.operator +(Float<T> value) => value;
      static Float<T> IUnaryNegationOperators<Float<T>, Float<T>>.operator -(Float<T> value) => -value;
      static Float<T> IIncrementOperators<Float<T>>.operator ++(Float<T> value) => ++value;
      static Float<T> IDecrementOperators<Float<T>>.operator --(Float<T> value) => --value;
      static Float<T> IAdditionOperators<Float<T>, Float<T>, Float<T>>.operator +(Float<T> left, Float<T> right) => left + right;
      static Float<T> ISubtractionOperators<Float<T>, Float<T>, Float<T>>.operator -(Float<T> left, Float<T> right) => left - right;
      static Float<T> IMultiplyOperators<Float<T>, Float<T>, Float<T>>.operator *(Float<T> left, Float<T> right) => left * right;
      static Float<T> IDivisionOperators<Float<T>, Float<T>, Float<T>>.operator /(Float<T> left, Float<T> right) => left / right;
      static Float<T> IModulusOperators<Float<T>, Float<T>, Float<T>>.operator %(Float<T> left, Float<T> right) => left % right;
      static bool IEqualityOperators<Float<T>, Float<T>, bool>.operator ==(Float<T> left, Float<T> right) => left == right;
      static bool IEqualityOperators<Float<T>, Float<T>, bool>.operator !=(Float<T> left, Float<T> right) => left != right;
      static bool IComparisonOperators<Float<T>, Float<T>, bool>.operator <(Float<T> left, Float<T> right) => left < right;
      static bool IComparisonOperators<Float<T>, Float<T>, bool>.operator >(Float<T> left, Float<T> right) => left > right;
      static bool IComparisonOperators<Float<T>, Float<T>, bool>.operator <=(Float<T> left, Float<T> right) => left <= right;
      static bool IComparisonOperators<Float<T>, Float<T>, bool>.operator >=(Float<T> left, Float<T> right) => left >= right;

      static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator ~(Float<T> x) => bwcmp(x);
      static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator &(Float<T> x, Float<T> y) => bwand(x, y);
      static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator |(Float<T> x, Float<T> y) => bwor(x, y);
      static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator ^(Float<T> x, Float<T> y) => bwxor(x, y);

      static Float<T> IExponentialFunctions<Float<T>>.Exp(Float<T> x) => Exp(x);
      static Float<T> IExponentialFunctions<Float<T>>.Exp10(Float<T> x) => Exp10(x);
      static Float<T> IExponentialFunctions<Float<T>>.Exp2(Float<T> x) => Exp2(x);
      static Float<T> SinPi(Float<T> x)
      {
        var t = BigRational.MaxDigits; BigRational.MaxDigits = Float<T>.Digits;
        var b = (Float<T>)BigRational.SinPi((BigRational)x); BigRational.MaxDigits = t; return b;
      }
      static Float<T> TanPi(Float<T> x)
      {
        var t = BigRational.MaxDigits; BigRational.MaxDigits = Float<T>.Digits;
        var b = (Float<T>)BigRational.TanPi((BigRational)x); BigRational.MaxDigits = t; return b;
      }
      static Float<T> AcosPi(Float<T> x)
      {
        var t = BigRational.MaxDigits; BigRational.MaxDigits = Float<T>.Digits;
        var b = (Float<T>)BigRational.AcosPi((BigRational)x); BigRational.MaxDigits = t; return b;
      }
      static Float<T> AsinPi(Float<T> x)
      {
        var t = BigRational.MaxDigits; BigRational.MaxDigits = Float<T>.Digits;
        var b = (Float<T>)BigRational.AsinPi((BigRational)x); BigRational.MaxDigits = t; return b;
      }
      static Float<T> AtanPi(Float<T> x)
      {
        var t = BigRational.MaxDigits; BigRational.MaxDigits = Float<T>.Digits;
        var b = (Float<T>)BigRational.AtanPi((BigRational)x); BigRational.MaxDigits = t; return b;
      }
      static Float<T> CosPi(Float<T> x)
      {
        var t = BigRational.MaxDigits; BigRational.MaxDigits = Float<T>.Digits;
        var b = (Float<T>)BigRational.CosPi((BigRational)x); BigRational.MaxDigits = t; return b;
      }
      static Float<T> ITrigonometricFunctions<Float<T>>.SinPi(Float<T> x) => SinPi(x);
      static Float<T> ITrigonometricFunctions<Float<T>>.TanPi(Float<T> x) => TanPi(x);
      static Float<T> ITrigonometricFunctions<Float<T>>.AcosPi(Float<T> x) => AcosPi(x);
      static Float<T> ITrigonometricFunctions<Float<T>>.AsinPi(Float<T> x) => AsinPi(x);
      static Float<T> ITrigonometricFunctions<Float<T>>.AtanPi(Float<T> x) => AtanPi(x);
      static Float<T> ITrigonometricFunctions<Float<T>>.CosPi(Float<T> x) => CosPi(x);
      static (Float<T> Sin, Float<T> Cos) ITrigonometricFunctions<Float<T>>.SinCos(Float<T> x) => SinCos(x);
      static (Float<T> SinPi, Float<T> CosPi) ITrigonometricFunctions<Float<T>>.SinCosPi(Float<T> x) => SinCosPi(x);

      static bool INumberBase<Float<T>>.IsEvenInteger(Float<T> value) => IsEvenInteger(value);
      static bool INumberBase<Float<T>>.IsFinite(Float<T> value) => IsFinite(value);
      static bool INumberBase<Float<T>>.IsInfinity(Float<T> value) => IsInfinity(value);
      static bool INumberBase<Float<T>>.IsInteger(Float<T> value) => IsInteger(value);
      static bool INumberBase<Float<T>>.IsNaN(Float<T> value) => IsNaN(value);
      static bool INumberBase<Float<T>>.IsNegative(Float<T> value) => IsNegative(value);
      static bool INumberBase<Float<T>>.IsNegativeInfinity(Float<T> value) => IsNegativeInfinity(value);
      static bool INumberBase<Float<T>>.IsOddInteger(Float<T> value) => IsOddInteger(value);
      static bool INumberBase<Float<T>>.IsPositive(Float<T> value) => IsPositive(value);
      static bool INumberBase<Float<T>>.IsPositiveInfinity(Float<T> value) => IsPositiveInfinity(value);
      static bool INumberBase<Float<T>>.IsRealNumber(Float<T> value) => IsRealNumber(value);
      static bool INumberBase<Float<T>>.IsSubnormal(Float<T> value) => IsSubnormal(value);
      static bool INumberBase<Float<T>>.IsNormal(Float<T> value) => IsNormal(value);
      static bool INumberBase<Float<T>>.IsZero(Float<T> value) => IsZero(value);

      static int INumberBase<Float<T>>.Radix => 2;
      static bool INumberBase<Float<T>>.IsCanonical(Float<T> value) => true;
      static bool INumberBase<Float<T>>.IsComplexNumber(Float<T> value) => false;
      static bool INumberBase<Float<T>>.IsImaginaryNumber(Float<T> value) => false;

      static Float<T> INumberBase<Float<T>>.One => 1;
      static Float<T> INumberBase<Float<T>>.Zero => default;
      static Float<T> ISignedNumber<Float<T>>.NegativeOne => -1;
      static Float<T> IFloatingPointIeee754<Float<T>>.Epsilon => Epsilon;
      static Float<T> IFloatingPointIeee754<Float<T>>.NaN => NaN;
      static Float<T> IFloatingPointIeee754<Float<T>>.NegativeInfinity => NegativeInfinity;
      static Float<T> IFloatingPointIeee754<Float<T>>.NegativeZero => NegativeZero;
      static Float<T> IFloatingPointIeee754<Float<T>>.PositiveInfinity => PositiveInfinity;
      static Float<T> IFloatingPointConstants<Float<T>>.E => Float<T>.E;
      static Float<T> IFloatingPointConstants<Float<T>>.Pi => Float<T>.Pi;
      static Float<T> IFloatingPointConstants<Float<T>>.Tau => Float<T>.Tau;

      static Float<T> INumberBase<Float<T>>.MaxMagnitude(Float<T> x, Float<T> y) => (Float<T>)BigRational.MaxMagnitude((BigRational)x, (BigRational)y);
      static Float<T> INumberBase<Float<T>>.MaxMagnitudeNumber(Float<T> x, Float<T> y) => (Float<T>)BigRational.MaxMagnitudeNumber((BigRational)x, (BigRational)y);
      static Float<T> INumberBase<Float<T>>.MinMagnitude(Float<T> x, Float<T> y) => (Float<T>)BigRational.MinMagnitude((BigRational)x, (BigRational)y);
      static Float<T> INumberBase<Float<T>>.MinMagnitudeNumber(Float<T> x, Float<T> y) => (Float<T>)BigRational.MinMagnitudeNumber((BigRational)x, (BigRational)y);

      static Float<T> IBinaryNumber<Float<T>>.Log2(Float<T> value) => Log2(value);
      static Float<T> IBinaryNumber<Float<T>>.AllBitsSet => AllBitsSet;
      static Float<T> IAdditiveIdentity<Float<T>, Float<T>>.AdditiveIdentity => default;
      static Float<T> IMultiplicativeIdentity<Float<T>, Float<T>>.MultiplicativeIdentity => (sbyte)1;
      static Float<T> IFloatingPointIeee754<Float<T>>.Atan2Pi(Float<T> y, Float<T> x) => Atan2Pi(y, x);
      static Float<T> IFloatingPointIeee754<Float<T>>.BitDecrement(Float<T> x) => BitDecrement(x);
      static Float<T> IFloatingPointIeee754<Float<T>>.BitIncrement(Float<T> x) => BitIncrement(x);
      static Float<T> IFloatingPointIeee754<Float<T>>.FusedMultiplyAdd(Float<T> left, Float<T> right, Float<T> addend) => FusedMultiplyAdd(left, right, addend);
      static Float<T> IFloatingPointIeee754<Float<T>>.Ieee754Remainder(Float<T> left, Float<T> right) => Ieee754Remainder(left, right);
      static int IFloatingPointIeee754<Float<T>>.ILogB(Float<T> x) => ILogB(x);
      static Float<T> IFloatingPointIeee754<Float<T>>.ScaleB(Float<T> x, int n) => ScaleB(x, n);
      static Float<T> IRootFunctions<Float<T>>.Hypot(Float<T> x, Float<T> y) => Hypot(x, y);
      readonly int IFloatingPoint<Float<T>>.GetExponentByteCount() => GetExponentByteCount();
      readonly int IFloatingPoint<Float<T>>.GetExponentShortestBitLength() => GetExponentShortestBitLength();
      readonly int IFloatingPoint<Float<T>>.GetSignificandBitLength() => GetSignificandBitLength();
      readonly int IFloatingPoint<Float<T>>.GetSignificandByteCount() => GetSignificandByteCount();

      static bool TryConvertFrom<R>(R value, uint f, out Float<T> result) where R : INumberBase<R>
      {
        if (BigRational.TryConvertFrom(value, out var r)) { result = (Float<T>)r; return false; }
        result = default; return false; //todo: opt. checks inline
      }
      static bool TryConvertTo<R>(Float<T> value, uint f, out R result) where R : INumberBase<R>
      {
        var a = (BigRational)value; //todo: opt. checks inline
        if (BigRational.TryConvertTo<R>(a, 0x1000, out var r)) { result = r; return true; }
        result = default!; return false;
      }
      static bool INumberBase<Float<T>>.TryConvertFromChecked<TOther>(TOther value, out Float<T> result) => TryConvertFrom<TOther>(value, 0x1000, out result);
      static bool INumberBase<Float<T>>.TryConvertFromSaturating<TOther>(TOther value, out Float<T> result) => TryConvertFrom<TOther>(value, 0, out result);
      static bool INumberBase<Float<T>>.TryConvertFromTruncating<TOther>(TOther value, out Float<T> result) => TryConvertFrom<TOther>(value, 0, out result);
      static bool INumberBase<Float<T>>.TryConvertToChecked<TOther>(Float<T> value, out TOther result) where TOther : default => TryConvertTo<TOther>(value, 0x1000, out result);
      static bool INumberBase<Float<T>>.TryConvertToSaturating<TOther>(Float<T> value, out TOther result) where TOther : default => TryConvertTo<TOther>(value, 0, out result);
      static bool INumberBase<Float<T>>.TryConvertToTruncating<TOther>(Float<T> value, out TOther result) where TOther : default => TryConvertTo<TOther>(value, 0, out result);

      readonly bool IFloatingPoint<Float<T>>.TryWriteExponentBigEndian(Span<byte> sp, out int bw) => TryWrite(this, 0, sp, out bw);
      readonly bool IFloatingPoint<Float<T>>.TryWriteExponentLittleEndian(Span<byte> sp, out int bw) => TryWrite(this, 1, sp, out bw);
      readonly bool IFloatingPoint<Float<T>>.TryWriteSignificandBigEndian(Span<byte> sp, out int bw) => TryWrite(this, 2, sp, out bw);
      readonly bool IFloatingPoint<Float<T>>.TryWriteSignificandLittleEndian(Span<byte> sp, out int bw) => TryWrite(this, 3, sp, out bw);

      static Float<T> cast<U>(U u) where U : unmanaged, Float<T>.IFloat<U> => *(Float<T>*)&u;
    }

    //readonly struct TestFloat32 : BigRational.Float<UInt32>.IFloat<TestFloat32> { }

    public unsafe readonly partial struct Float<T>
    {
      public interface IFloat<U> : IMinMaxValue<U>, IBinaryFloatingPointIeee754<U> where U : unmanaged, IFloat<U>
      {
        string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => Float<T>.cast<U>((U)this).ToString(format, formatProvider);
        bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) { var t = Float<T>.cast<U>((U)this); return t.TryFormat(destination, out charsWritten, format, provider); }
        int IComparable.CompareTo(object? obj) { if (obj is not U u) return 1; var t = Float<T>.cast<U>((U)this); return Float<T>.Compare((uint*)&t, (uint*)&u); }
        int IComparable<U>.CompareTo(U other) { var t = Float<T>.cast<U>((U)this); return Float<T>.Compare((uint*)&t, (uint*)&other); }
        bool IEquatable<U>.Equals(U other) { var t = Float<T>.cast<U>((U)this); return Float<T>.Equals((uint*)&t, (uint*)&other); }
        static U IParsable<U>.Parse(string value, IFormatProvider? provider) { var t = Float<T>.Parse(value, provider); return *(U*)&t; }
        static bool IParsable<U>.TryParse(string? value, IFormatProvider? provider, out U result) { var t = Float<T>.TryParse(value, provider, out var r); result = *(U*)&r; return t; }
        static U ISpanParsable<U>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) { var t = Float<T>.TryParse(s, provider, out var r); return *(U*)&r; }
        static bool ISpanParsable<U>.TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out U result) { var t = Float<T>.TryParse(value, provider, out var r); result = *(U*)&r; return t; }
        static U INumberBase<U>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        {
          var t = Float<T>.TryParse(s, style, provider, out var r); return *(U*)&r;
        }
        static U INumberBase<U>.Parse(string s, NumberStyles style, IFormatProvider? provider)
        {
          var t = Float<T>.TryParse(s, style, provider, out var r); return *(U*)&r;
        }
        static bool INumberBase<U>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out U result)
        {
          var t = Float<T>.TryParse(s, style, provider, out var r); result = *(U*)&r; return t;
        }
        static bool INumberBase<U>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out U result)
        {
          var t = Float<T>.TryParse(s, style, provider, out var r); result = *(U*)&r; return t;
        }

        static U IUnaryPlusOperators<U, U>.operator +(U value) => value;
        static U IUnaryNegationOperators<U, U>.operator -(U value) { *(Float<T>*)&value = -(*(Float<T>*)&value); return *(U*)&value; }
        static U IIncrementOperators<U>.operator ++(U x) { *(Float<T>*)&x = ++(*(Float<T>*)&x); return *(U*)&x; }
        static U IDecrementOperators<U>.operator --(U x) { *(Float<T>*)&x = --(*(Float<T>*)&x); return *(U*)&x; }
        static U IAdditionOperators<U, U, U>.operator +(U x, U y) { *(Float<T>*)&x = *(Float<T>*)&x + *(Float<T>*)&y; return *(U*)&x; }
        static U ISubtractionOperators<U, U, U>.operator -(U x, U y) { *(Float<T>*)&x = *(Float<T>*)&x - *(Float<T>*)&y; return *(U*)&x; }
        static U IMultiplyOperators<U, U, U>.operator *(U x, U y) { *(Float<T>*)&x = *(Float<T>*)&x * *(Float<T>*)&y; return *(U*)&x; }
        static U IDivisionOperators<U, U, U>.operator /(U x, U y) { *(Float<T>*)&x = *(Float<T>*)&x / *(Float<T>*)&y; return *(U*)&x; }
        static U IModulusOperators<U, U, U>.operator %(U x, U y) { *(Float<T>*)&x = *(Float<T>*)&x % *(Float<T>*)&y; return *(U*)&x; }
        static bool IEqualityOperators<U, U, bool>.operator ==(U x, U y) => *(Float<T>*)&x == *(Float<T>*)&y;
        static bool IEqualityOperators<U, U, bool>.operator !=(U x, U y) => *(Float<T>*)&x != *(Float<T>*)&y;
        static bool IComparisonOperators<U, U, bool>.operator <(U x, U y) => *(Float<T>*)&x < *(Float<T>*)&y;
        static bool IComparisonOperators<U, U, bool>.operator >(U x, U y) => *(Float<T>*)&x > *(Float<T>*)&y;
        static bool IComparisonOperators<U, U, bool>.operator <=(U x, U y) => *(Float<T>*)&x <= *(Float<T>*)&y;
        static bool IComparisonOperators<U, U, bool>.operator >=(U x, U y) => *(Float<T>*)&x >= *(Float<T>*)&y;
        static U IBitwiseOperators<U, U, U>.operator ~(U x) { *(Float<T>*)&x = Float<T>.bwcmp(*(Float<T>*)&x); return *(U*)&x; }
        static U IBitwiseOperators<U, U, U>.operator &(U x, U y) { *(Float<T>*)&x = Float<T>.bwand(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x; }
        static U IBitwiseOperators<U, U, U>.operator |(U x, U y) { *(Float<T>*)&x = Float<T>.bwor(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x; }
        static U IBitwiseOperators<U, U, U>.operator ^(U x, U y) { *(Float<T>*)&x = Float<T>.bwxor(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x; }

        static U IMinMaxValue<U>.MaxValue { get { var t = Float<T>.MaxValue; return *(U*)&t; } }
        static U IMinMaxValue<U>.MinValue { get { var t = Float<T>.MinValue; return *(U*)&t; } }
        static U IFloatingPointIeee754<U>.Epsilon { get { var t = Float<T>.Epsilon; return *(U*)&t; } }
        static U IFloatingPointIeee754<U>.NaN { get { var t = Float<T>.NaN; return *(U*)&t; } }
        static U IFloatingPointIeee754<U>.NegativeInfinity { get { var t = Float<T>.NegativeInfinity; return *(U*)&t; } }
        static U IFloatingPointIeee754<U>.NegativeZero { get { var t = Float<T>.NegativeZero; return *(U*)&t; } }
        static U IFloatingPointIeee754<U>.PositiveInfinity { get { var t = Float<T>.PositiveInfinity; return *(U*)&t; } }

        static U INumberBase<U>.MaxMagnitude(U x, U y)
        {
          *(Float<T>*)&x = Float<T>.MaxMagnitude(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x;
        }
        static U INumberBase<U>.MaxMagnitudeNumber(U x, U y)
        {
          *(Float<T>*)&x = Float<T>.MaxMagnitudeNumber(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x;
        }
        static U INumberBase<U>.MinMagnitude(U x, U y)
        {
          *(Float<T>*)&x = Float<T>.MinMagnitude(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x;
        }
        static U INumberBase<U>.MinMagnitudeNumber(U x, U y)
        {
          *(Float<T>*)&x = Float<T>.MinMagnitudeNumber(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x;
        }
        static U IFloatingPointIeee754<U>.BitDecrement(U x) { *(Float<T>*)&x = Float<T>.BitDecrement(*(Float<T>*)&x); return *(U*)&x; }
        static U IFloatingPointIeee754<U>.BitIncrement(U x) { *(Float<T>*)&x = Float<T>.BitIncrement(*(Float<T>*)&x); return *(U*)&x; }
        static U IFloatingPointIeee754<U>.Atan2(U y, U x) { *(Float<T>*)&x = Float<T>.Atan2(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x; }
        static U IFloatingPointIeee754<U>.Atan2Pi(U y, U x) { *(Float<T>*)&y = Float<T>.Atan2Pi(*(Float<T>*)&y, *(Float<T>*)&x); return *(U*)&y; }
        static U IFloatingPointIeee754<U>.ScaleB(U x, int n) { *(Float<T>*)&x = Float<T>.ScaleB(*(Float<T>*)&x, n); return *(U*)&x; }
        static int IFloatingPointIeee754<U>.ILogB(U x) => Float<T>.ILogB(*(Float<T>*)&x);
        static U IFloatingPointIeee754<U>.FusedMultiplyAdd(U x, U y, U z) { *(Float<T>*)&x = Float<T>.FusedMultiplyAdd(*(Float<T>*)&x, *(Float<T>*)&y, *(Float<T>*)&z); return *(U*)&x; }
        static U IFloatingPointIeee754<U>.Ieee754Remainder(U x, U y) { *(Float<T>*)&x = Float<T>.Ieee754Remainder(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x; }
        static U IBinaryNumber<U>.AllBitsSet { get { var t = Float<T>.AllBitsSet; return *(U*)&t; } }

        static U INumberBase<U>.One { get { var t = (Float<T>)(sbyte)1; return *(U*)&t; } }
        static U INumberBase<U>.Zero => default;
        static U ISignedNumber<U>.NegativeOne { get { var t = (Float<T>)(sbyte)(-1); return *(U*)&t; } }
        static U IFloatingPointConstants<U>.E { get { var t = Float<T>.E; return *(U*)&t; } }
        static U IFloatingPointConstants<U>.Pi { get { var t = Float<T>.Pi; return *(U*)&t; } }
        static U IFloatingPointConstants<U>.Tau { get { var t = Float<T>.Tau; return *(U*)&t; } }
        static U IAdditiveIdentity<U, U>.AdditiveIdentity => default;
        static U IMultiplicativeIdentity<U, U>.MultiplicativeIdentity { get { var t = (Float<T>)(sbyte)1; return *(U*)&t; } }

        static bool INumberBase<U>.IsEvenInteger(U value) => Float<T>.IsEvenInteger(*(Float<T>*)&value);
        static bool INumberBase<U>.IsFinite(U value) => Float<T>.IsFinite(*(Float<T>*)&value);
        static bool INumberBase<U>.IsInfinity(U value) => Float<T>.IsInfinity(*(Float<T>*)&value);
        static bool INumberBase<U>.IsInteger(U value) => Float<T>.IsInteger(*(Float<T>*)&value);
        static bool INumberBase<U>.IsNaN(U value) => Float<T>.IsNaN(*(Float<T>*)&value);
        static bool INumberBase<U>.IsNegative(U value) => Float<T>.IsNegative(*(Float<T>*)&value);
        static bool INumberBase<U>.IsNegativeInfinity(U value) => Float<T>.IsNegativeInfinity(*(Float<T>*)&value);
        static bool INumberBase<U>.IsOddInteger(U value) => Float<T>.IsOddInteger(*(Float<T>*)&value);
        static bool INumberBase<U>.IsPositive(U value) => Float<T>.IsPositive(*(Float<T>*)&value);
        static bool INumberBase<U>.IsPositiveInfinity(U value) => Float<T>.IsPositiveInfinity(*(Float<T>*)&value);
        static bool INumberBase<U>.IsRealNumber(U value) => Float<T>.IsRealNumber(*(Float<T>*)&value);
        static bool INumberBase<U>.IsSubnormal(U value) => Float<T>.IsSubnormal(*(Float<T>*)&value);
        static bool INumberBase<U>.IsNormal(U value) => Float<T>.IsNormal(*(Float<T>*)&value);
        static bool INumberBase<U>.IsZero(U value) => Float<T>.IsZero(*(Float<T>*)&value);

        static int INumberBase<U>.Radix => 2;
        static bool INumberBase<U>.IsCanonical(U value) => true;
        static bool INumberBase<U>.IsComplexNumber(U value) => false;
        static bool INumberBase<U>.IsImaginaryNumber(U value) => false;

        static U INumberBase<U>.Abs(U value)
        {
          *(Float<T>*)&value = Float<T>.Abs(*(Float<T>*)&value); return *(U*)&value;
          //var a = Float<T>.Abs(*(Float<T>*)&value); return *(U*)&a;
          //var b = *(Float<T>*)&value; /*inline*/ return *(U*)&b;
        }
        static U IRootFunctions<U>.Sqrt(U value)
        {
          *(Float<T>*)&value = Float<T>.Sqrt(*(Float<T>*)&value); return *(U*)&value;
          //var a = Float<T>.Sqrt(*(Float<T>*)&value); return *(U*)&a;
        }
        static U ITrigonometricFunctions<U>.Sin(U x) { *(Float<T>*)&x = Float<T>.Sin(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.Cos(U x) { *(Float<T>*)&x = Float<T>.Cos(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.Acos(U x) { *(Float<T>*)&x = Float<T>.Acos(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.Asin(U x) { *(Float<T>*)&x = Float<T>.Asin(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.Atan(U x) { *(Float<T>*)&x = Float<T>.Atan(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.Tan(U x) { *(Float<T>*)&x = Float<T>.Tan(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.AtanPi(U x) { *(Float<T>*)&x = Float<T>.AtanPi(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.AcosPi(U x) { *(Float<T>*)&x = Float<T>.AcosPi(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.AsinPi(U x) { *(Float<T>*)&x = Float<T>.AsinPi(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.CosPi(U x) { *(Float<T>*)&x = Float<T>.CosPi(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.SinPi(U x) { *(Float<T>*)&x = Float<T>.SinPi(*(Float<T>*)&x); return *(U*)&x; }
        static U ITrigonometricFunctions<U>.TanPi(U x) { *(Float<T>*)&x = Float<T>.TanPi(*(Float<T>*)&x); return *(U*)&x; }
        static (U Sin, U Cos) ITrigonometricFunctions<U>.SinCos(U x) { var t = Float<T>.SinCos(*(Float<T>*)&x); return (*(U*)&t.Sin, *(U*)&t.Cos); }
        static (U SinPi, U CosPi) ITrigonometricFunctions<U>.SinCosPi(U x) { var t = Float<T>.SinCosPi(*(Float<T>*)&x); return (*(U*)&t.Sin, *(U*)&t.Cos); }
        static U IHyperbolicFunctions<U>.Acosh(U x) { *(Float<T>*)&x = Float<T>.Acosh(*(Float<T>*)&x); return *(U*)&x; }
        static U IHyperbolicFunctions<U>.Asinh(U x) { *(Float<T>*)&x = Float<T>.Asinh(*(Float<T>*)&x); return *(U*)&x; }
        static U IHyperbolicFunctions<U>.Atanh(U x) { *(Float<T>*)&x = Float<T>.Atanh(*(Float<T>*)&x); return *(U*)&x; }
        static U IHyperbolicFunctions<U>.Cosh(U x) { *(Float<T>*)&x = Float<T>.Cosh(*(Float<T>*)&x); return *(U*)&x; }
        static U IHyperbolicFunctions<U>.Sinh(U x) { *(Float<T>*)&x = Float<T>.Sinh(*(Float<T>*)&x); return *(U*)&x; }
        static U IHyperbolicFunctions<U>.Tanh(U x) { *(Float<T>*)&x = Float<T>.Tanh(*(Float<T>*)&x); return *(U*)&x; }
        static U IRootFunctions<U>.Cbrt(U x) { *(Float<T>*)&x = Float<T>.Cbrt(*(Float<T>*)&x); return *(U*)&x; }
        static U IExponentialFunctions<U>.Exp(U x) { *(Float<T>*)&x = Float<T>.Exp(*(Float<T>*)&x); return *(U*)&x; }
        static U IExponentialFunctions<U>.Exp10(U x) { *(Float<T>*)&x = Float<T>.Exp10(*(Float<T>*)&x); return *(U*)&x; }
        static U IExponentialFunctions<U>.Exp2(U x) { *(Float<T>*)&x = Float<T>.Exp2(*(Float<T>*)&x); return *(U*)&x; }
        static U IRootFunctions<U>.Hypot(U x, U y) { *(Float<T>*)&x = Float<T>.Hypot(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x; }
        static bool IBinaryNumber<U>.IsPow2(U x) => Float<T>.IsPow2(*(Float<T>*)&x);
        static U ILogarithmicFunctions<U>.Log(U x) { *(Float<T>*)&x = Float<T>.Log(*(Float<T>*)&x); return *(U*)&x; }
        static U ILogarithmicFunctions<U>.Log(U x, U newBase) { *(Float<T>*)&x = Float<T>.Log(*(Float<T>*)&x, *(Float<T>*)&newBase); return *(U*)&x; }
        static U ILogarithmicFunctions<U>.Log10(U x) { *(Float<T>*)&x = Float<T>.Log10(*(Float<T>*)&x); return *(U*)&x; }
        static U IBinaryNumber<U>.Log2(U x) { *(Float<T>*)&x = Float<T>.Log2(*(Float<T>*)&x); return *(U*)&x; }
        static U ILogarithmicFunctions<U>.Log2(U x) { *(Float<T>*)&x = Float<T>.Log2(*(Float<T>*)&x); return *(U*)&x; }
        static U IPowerFunctions<U>.Pow(U x, U y) { *(Float<T>*)&x = Float<T>.Pow(*(Float<T>*)&x, *(Float<T>*)&y); return *(U*)&x; }
        static U IRootFunctions<U>.RootN(U x, int n) { *(Float<T>*)&x = Float<T>.RootN(*(Float<T>*)&x, n); return *(U*)&x; }
        static U IFloatingPoint<U>.Round(U x, int digits, MidpointRounding mode) { *(Float<T>*)&x = Float<T>.Round(*(Float<T>*)&x, digits, mode); return *(U*)&x; }

        int IFloatingPoint<U>.GetExponentByteCount() { var t = Float<T>.cast<U>((U)this); return (*(Float<T>*)&t).GetExponentByteCount(); }
        int IFloatingPoint<U>.GetExponentShortestBitLength() { var t = Float<T>.cast<U>((U)this); return (*(Float<T>*)&t).GetExponentShortestBitLength(); }
        int IFloatingPoint<U>.GetSignificandBitLength() { var t = Float<T>.cast<U>((U)this); return (*(Float<T>*)&t).GetSignificandBitLength(); }
        int IFloatingPoint<U>.GetSignificandByteCount() { var t = Float<T>.cast<U>((U)this); return (*(Float<T>*)&t).GetSignificandByteCount(); }
        bool IFloatingPoint<U>.TryWriteExponentBigEndian(Span<byte> sp, out int bw) => Float<T>.TryWrite(Float<T>.cast<U>((U)this), 0, sp, out bw);
        bool IFloatingPoint<U>.TryWriteExponentLittleEndian(Span<byte> sp, out int bw) => Float<T>.TryWrite(Float<T>.cast<U>((U)this), 1, sp, out bw);
        bool IFloatingPoint<U>.TryWriteSignificandBigEndian(Span<byte> sp, out int bw) => Float<T>.TryWrite(Float<T>.cast<U>((U)this), 2, sp, out bw);
        bool IFloatingPoint<U>.TryWriteSignificandLittleEndian(Span<byte> sp, out int bw) => Float<T>.TryWrite(Float<T>.cast<U>((U)this), 3, sp, out bw);

        static bool INumberBase<U>.TryConvertFromChecked<TOther>(TOther value, out U result) { var t = Float<T>.TryConvertFrom<TOther>(value, 0x1000, out var x); result = *(U*)&x; return t; }
        static bool INumberBase<U>.TryConvertFromSaturating<TOther>(TOther value, out U result) { var t = Float<T>.TryConvertFrom<TOther>(value, 0, out var x); result = *(U*)&x; return t; }
        static bool INumberBase<U>.TryConvertFromTruncating<TOther>(TOther value, out U result) { var t = Float<T>.TryConvertFrom<TOther>(value, 0, out var x); result = *(U*)&x; return t; }
        static bool INumberBase<U>.TryConvertToChecked<TOther>(U value, out TOther result) where TOther : default { var x = *(Float<T>*)&value; return Float<T>.TryConvertTo<TOther>(x, 0x1000, out result); }
        static bool INumberBase<U>.TryConvertToSaturating<TOther>(U value, out TOther result) where TOther : default { var x = *(Float<T>*)&value; return Float<T>.TryConvertTo<TOther>(x, 0, out result); }
        static bool INumberBase<U>.TryConvertToTruncating<TOther>(U value, out TOther result) where TOther : default { var x = *(Float<T>*)&value; return Float<T>.TryConvertTo<TOther>(x, 0, out result); }
      }
    }

#endif
  }
}
