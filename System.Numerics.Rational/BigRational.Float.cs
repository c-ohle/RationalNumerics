
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
    /// <c>BigRational.Float&lt;Matrix4x4&gt;</c> defines a Octuple precision type.<br/>
    /// </remarks>
    /// <typeparam name="T">Any unmanaged value structure type to store the number bits.</typeparam>
    [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(),nq}")]
    public unsafe readonly partial struct Float<T> where T : unmanaged
    {
      public override readonly string ToString() => ToString(null, null);
      public readonly string ToString(string? format, IFormatProvider? provider = default)
      {
        var dig = (int)MathF.Round(MathF.Ceiling(mbi * 0.30103f)); if (sizeof(T) == 2) dig = 5;
        var cpu = main_cpu; var p = this.p; var e = push(cpu, (uint*)&p); //if (dig < e) dig = e;
        cpu.pow(2, e); cpu.mul(); var r = new BigRational(cpu.p[cpu.i - 1]);
        var ss = r.ToString("Ö" + dig); cpu.pop(); return ss;
      }
      public override readonly int GetHashCode()
      {
        return p.GetHashCode();
      }
      public override readonly bool Equals([NotNullWhen(true)] object? obj)
      {
        return obj is Float<T> t ? this == t : false;
      }
      public static int Compare(Float<T> a, Float<T> b)
      {
        return Compare((uint*)&a, (uint*)&b);
      }

      public static explicit operator Float<T>(int value)
      {
        var cpu = main_cpu; cpu.push(value);
        Float<T> c; pop(cpu, 0, (uint*)&c); return c;
      }
      public static explicit operator Float<T>(Half value)
      {
        //if (sizeof(T) == 2) return *(Float<T>*)&value; //todo: enable  after  tests
        var cpu = main_cpu; var e = Float<UInt16>.push(cpu, (uint*)&value);
        Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static explicit operator Float<T>(float value)
      {
        //if (sizeof(T) == 4) return *(Float<T>*)&value; //todo: enable  after  tests
        var cpu = main_cpu; var e = Float<UInt32>.push(cpu, (uint*)&value);
        Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static explicit operator Float<T>(double value)
      {
        //if (sizeof(T) == 8) return *(Float<T>*)&value; //todo: enable  after  tests
        var cpu = main_cpu; var e = Float<UInt64>.push(cpu, (uint*)&value);
        Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static explicit operator Float<T>(BigRational value)
      {
        var cpu = main_cpu; cpu.push(value); cpu.mod(8); var m = mbi;
        int s = cpu.sign(); if (cpu.sign() < 0) { cpu.neg(); cpu.neg(1); }
        int j, k, u = m - unchecked(j = (int)cpu.msb());
        cpu.shl(u); cpu.swp(); int v = (m << 1) - unchecked(k = (int)cpu.msb());
        cpu.shl(v); cpu.swp(); cpu.idiv();
        Float<T> c; pop(cpu, k - j - m, (uint*)&c); return c;
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
      public static implicit operator BigRational(Float<T> value)
      {
        var cpu = main_cpu; var e = push(cpu, (uint*)&value.p);
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
        Float<B> c; Float<B>.pop(cpu, e, (uint*)&c); return c;
      }

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
      //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
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
        int d = s - mbi, l;
        if (d > 0) { var c = cpu.bt(d - 1); cpu.shr(d); if (c) cpu.inc(); } //x87 rnd
        else if (d != 0) cpu.shl(-d);
        var sp = cpu.gets(cpu.mark() - 1);
        p[l = lui] = (sp[0] & 0x80000000) | (unchecked((uint)((e + d) + bia)) << ((31 - sbi))) | (sp[l + 1] & ((1u << (mbi - 1)) - 1u));
        if ((sizeof(T) & 2) != 0) if (l != 0) p[l] >>= 16; else p[l] = (p[l] & 0xffff) | p[l] >> 16;
        for (int i = 0; i < l; i++) p[i] = sp[i + 1];
        cpu.pop();
      }
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
      private static readonly int sbi, mbi, bia, lui; //todo: inline
      static Float()
      {
        int size = sizeof(T); if ((size & 1) != 0) throw new NotSupportedException(nameof(T));
        switch (size) { case 2: sbi = 5; break; case 4: sbi = 8; break; case 8: sbi = 11; break; case <= 16: sbi = 15; break; default: sbi = BitOperations.Log2(unchecked((uint)size)) * 3 + 4; break; }
        mbi = (size << 3) - sbi; bia = (1 << (sbi - 1)) + mbi - 2; lui = (size >> 2) - 1 + ((size & 2) >> 1); //check();
        Debug.WriteLine($"Float{size << 3} significand bits {mbi} exponent bits {((sizeof(T) << 3) - mbi) - 1} decimal digits {mbi * MathF.Log10(2):0.##}");
      }
      #endregion

      internal static int Bits => sizeof(T) << 3;
      internal static int BiasedExponentShift => mbi - 1; // dbl 52
      internal static int ExponentBits => (sizeof(T) << 3) - mbi; // dbl 11
      internal static int MaxBiasedExponent => (1 << sbi) - 1; // dbl 0x07FF;
      internal static int ExponentBias => (1 << (ExponentBits - 1)) - 1; // dbl 1023
      internal static int MinExponent => 1 - ExponentBias; // dbl -1022;
      internal static int MaxExponent => ExponentBias; // dbl +1023;
      internal static float DecimalDigits => mbi * MathF.Log10(2);

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

    // func's spec
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
      public static int Sign(Float<T> a) => default;
      /// <summary>
      /// Returns a value indicating whether the specified number is an integer. 
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number.</param>
      /// <returns><c>true</c> if <paramref name="a" /> is an integer; otherwise, <c>false</c>.</returns>
      public static bool IsInteger(Float<T> a) => default;
      /// <summary>
      /// Determines if the value is NaN. 
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number.</param>
      /// <returns><c>true</c> if <paramref name="a" /> is NaN; otherwise, <c>false</c>.</returns>
      public static bool IsNaN(Float<T> a) => default;
      /// <summary>
      /// Gets the absolute value of a <see cref="Float{T}"/> number.
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number as value.</param>
      /// <returns>The absolute value of the <see cref="Float{T}"/> number.</returns>
      public static Float<T> Abs(Float<T> a) => default;
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
      public static Float<T> Floor(Float<T> a) => default;
      /// <summary>
      /// Returns the smallest integral value that is greater than or equal to the specified number.
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number.</param>
      /// <returns>
      /// The smallest integral value that is greater than or equal to the <paramref name="a"/> parameter.
      /// </returns>
      public static Float<T> Ceiling(Float<T> a) => default;
      /// <summary>
      /// Rounds a <see cref="Float{T}"/> number to the nearest integral value
      /// and rounds midpoint values to the nearest even number.
      /// </summary>
      /// <param name="a">A <see cref="Float{T}"/> number to be rounded.</param>
      /// <returns></returns>
      public static Float<T> Round(Float<T> a) => default;
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
      public static Float<T> Round(Float<T> a, int digits, MidpointRounding mode) => default;
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
      public static Float<T> Pow(Float<T> x, Float<T> y) => default;
      /// <summary>
      /// Returns a specified number raised to the specified power.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">A <see cref="Float{T}"/> number to be raised to a power.</param>
      /// <returns>The <see cref="Float{T}"/> number a raised to the power b.</returns>
      public static Float<T> Pow2(Float<T> x) => default;
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
      public static Float<T> Sqrt(Float<T> a) => default;
      /// <summary>Computes the cube-root of a value.</summary>
      /// <param name="x">The value whose cube-root is to be computed.</param>
      /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
      /// <returns>The cube-root of <paramref name="x" />.</returns>
      public static Float<T> Cbrt(Float<T> x, int digits) => default;
      /// <summary>Computes the n-th root of a value.</summary>
      /// <param name="x">The value whose <paramref name="n" />-th root is to be computed.</param>
      /// <param name="n">The degree of the root to be computed.</param>
      /// <returns>The <paramref name="n" />-th root of <paramref name="x" />.</returns>
      public static Float<T> RootN(Float<T> x, int n) => default;
      /// <summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
      /// <param name="x">The value to square and add to <paramref name="y" />.</param>
      /// <param name="y">The value to square and add to <paramref name="x" />.</param>
      /// <returns>The square root of <paramref name="x" />-squared plus <paramref name="y" />-squared.</returns>
      public static Float<T> Hypot(Float<T> x, Float<T> y) => default;
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
      public static Float<T> Log2(Float<T> x) => default;
      /// <summary>
      /// Returns the base 10 logarithm of a specified number.
      /// </summary>
      /// <param name="x">The number whose logarithm is to be found.</param>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <returns>The base 10 logarithm of <paramref name="x"/>.</returns>
      /// <exception cref="ArgumentException">For <paramref name="x"/> is less or equal zero.</exception>
      public static Float<T> Log10(Float<T> x)
      {
        return Round(Log2(x) / Log2((Float<T>)10)); //todo: inline
      }
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
      public static Float<T> Log(Float<T> x) => default;
      /// <summary>Computes the logarithm of a value in the specified base.</summary>
      /// <param name="x">The value whose logarithm is to be computed.</param>
      /// <param name="newBase">The base in which the logarithm is to be computed.</param>
      /// <returns><c>log<sub><paramref name="newBase" /></sub>(<paramref name="x" />)</c></returns>
      public static Float<T> Log(Float<T> x, Float<T> newBase)
      {
        return Log(x) / Log(newBase); //todo: opt. cpu
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
      public static Float<T> Exp(Float<T> x) => default;
      /// <summary>
      /// Returns the sine of the specified angle.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">An angle, measured in radians.</param>
      /// <returns>The sine of <paramref name="x"/>.</returns>
      public static Float<T> Sin(Float<T> x) => default;
      /// <summary>
      /// Returns the cosine of the specified angle.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">An angle, measured in radians.</param>
      /// <returns>The cosine of <paramref name="x"/>.</returns>
      public static Float<T> Cos(Float<T> x) => default;
      /// <summary>
      /// Returns the tangent of the specified angle.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">An angle, measured in radians.</param>
      /// <returns>The tangent of <paramref name="x"/>.</returns>
      public static Float<T> Tan(Float<T> x)
      {
        return Sin(x) / Cos(x); //todo: inline
      }
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
      public static Float<T> Asin(Float<T> x)
      {
        return Atan(x / Sqrt((Float<T>)1 - x * x)); //todo: inline
      }
      /// <summary>
      /// Returns the angle whose cosine is the specified number.
      /// </summary>
      /// <param name="x">A number representing a cosine, where d must be greater than or equal to -1, but less than or equal to 1.</param>
      /// <returns>
      /// An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2. 
      /// -or- NaN if <paramref name="x"/> &lt; -1 or <paramref name="x"/> &gt; 1.
      /// </returns>
      public static Float<T> Acos(Float<T> x)
      {
        return Atan(Sqrt((Float<T>)1 - x * x) / x); //todo: inline
      }
      /// <summary>
      /// Returns the angle whose tangent is the specified number.
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="x">A number representing a tangent.</param>
      /// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.</returns>
      public static Float<T> Atan(Float<T> x) => default;
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
      public static Float<T> Atan2(Float<T> y, Float<T> x) => default;

      /// <summary>Computes the hyperbolic arc-sine of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic arc-sine is to be computed.</param>
      /// <returns>The hyperbolic arc-sine of <paramref name="x" />.</returns>
      public static Float<T> Asinh(Float<T> x)
      {
        return Log(x + Sqrt(x * x + (Float<T>)1)); //todo: opt. cpu
      }
      /// <summary>Computes the hyperbolic arc-cosine of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic arc-cosine is to be computed.</param>
      /// <returns>The hyperbolic arc-cosine of <paramref name="x" />.</returns>
      public static Float<T> Acosh(Float<T> x)
      {
        return Log(x + Sqrt(x * x - (Float<T>)1)); //todo: opt. cpu
      }
      /// <summary>Computes the hyperbolic arc-tangent of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic arc-tangent is to be computed.</param>
      /// <returns>The hyperbolic arc-tangent of <paramref name="x" />.</returns>
      public static Float<T> Atanh(Float<T> x)
      {
        return Log(((Float<T>)1 + x) / ((Float<T>)1 - x)) / (Float<T>)2; //todo: opt. cpu
      }
      /// <summary>Computes the hyperbolic sine of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic sine is to be computed.</param>
      /// <returns>The hyperbolic sine of <paramref name="x" />.</returns>
      public static Float<T> Sinh(Float<T> x)
      {
        return (Exp(x) - Exp(-x)) / (Float<T>)2; //todo: opt. cpu
      }
      /// <summary>Computes the hyperbolic cosine of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic cosine is to be computed.</param>
      /// <returns>The hyperbolic cosine of <paramref name="x" />.</returns>
      public static Float<T> Cosh(Float<T> x)
      {
        return (Exp(x) + Exp(-x)) / (Float<T>)2; //todo: opt. cpu
      }
      /// <summary>Computes the hyperbolic tangent of a value.</summary>
      /// <param name="x">The value, in radians, whose hyperbolic tangent is to be computed.</param>
      /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
      /// <returns>The hyperbolic tangent of <paramref name="x" />.</returns>
      public static Float<T> Tanh(Float<T> x, int digits)
      {
        return (Float<T>)1 - (Float<T>)2 / (Exp(x * (Float<T>)2) + (Float<T>)1); //todo: opt. cpu
      }
    }
  }

}
