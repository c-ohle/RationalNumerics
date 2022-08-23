
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

using static System.Numerics.BigRational;

#pragma warning disable CS1591 //xml comments

namespace System.Numerics.Generic
{
  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public unsafe readonly struct Float<T> : IComparable<Float<T>>, IEquatable<Float<T>>, IComparable, ISpanFormattable, IFloat<Float<T>> where T : unmanaged
  {
    public readonly override string ToString() => ToString(default);
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

      cpu.pow(2, e); cpu.mul();
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
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Float<T> result)
    {
      return TryParse(s, provider, out result);//todo: style checks
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
      //return Float<UInt16>.Cast<T>(*(Float<UInt16>*)&value);
      var cpu = main_cpu; var e = Float<UInt16>.push(cpu, (uint*)&value);
      Float<T> c; pop(cpu, e, (uint*)&c); return c;
    }
    public static explicit operator Float<T>(float value)
    {
      //if (sizeof(T) == 4) return *(Float<T>*)&value; //todo: enable  after  tests
      //return Float<UInt32>.Cast<T>(*(Float<UInt32>*)&value);
      var cpu = main_cpu; var e = Float<UInt32>.push(cpu, (uint*)&value);
      Float<T> c; pop(cpu, e, (uint*)&c);
      //if (Float<T>.Digits > (e = Float<UInt32>.Digits)) c = Float<T>.Round(c, e); 
      return c; //return c;
    }
    public static explicit operator Float<T>(double value)
    {
      //if (sizeof(T) == 8) return *(Float<T>*)&value; //todo: enable  after  tests
      //return Float<UInt64>.Cast<T>(*(Float<UInt64>*)&value);
      var cpu = main_cpu; var e = Float<UInt64>.push(cpu, (uint*)&value);
      Float<T> c; pop(cpu, e, (uint*)&c);
      //if (Float<T>.Digits > (e = Float<UInt64>.Digits)) c = Float<T>.Round(c, e); 
      return c;
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

    #region public static 
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

    public static Float<T> SinPi(Float<T> x) => (Float<T>)BigRational.SinPi((BigRational)x, Float<T>.Digits);
    public static Float<T> TanPi(Float<T> x) => (Float<T>)BigRational.TanPi((BigRational)x, Float<T>.Digits);
    public static Float<T> AcosPi(Float<T> x) => (Float<T>)BigRational.AcosPi((BigRational)x, Float<T>.Digits);
    public static Float<T> AsinPi(Float<T> x) => (Float<T>)BigRational.AsinPi((BigRational)x, Float<T>.Digits);
    public static Float<T> AtanPi(Float<T> x) => (Float<T>)BigRational.AtanPi((BigRational)x, Float<T>.Digits);
    public static Float<T> CosPi(Float<T> x) => (Float<T>)BigRational.CosPi((BigRational)x, Float<T>.Digits);
    #endregion
    #region todo impl.
    public static Float<T> Clamp(Float<T> value, Float<T> min, Float<T> max) => throw new NotImplementedException();
    public static Float<T> CopySign(Float<T> value, Float<T> sign) => throw new NotImplementedException();
    public static Float<T> Epsilon => -1;//throw new NotImplementedException();
    public static Float<T> NaN => -1;//throw new NotImplementedException();
    public static Float<T> NegativeInfinity => -1;//throw new NotImplementedException();
    public static Float<T> NegativeZero => -1;//throw new NotImplementedException();
    public static Float<T> PositiveInfinity => -1;//throw new NotImplementedException();
    internal static Float<T> AllBitsSet => -1;//throw new NotImplementedException();    
    public static Float<T> Atan2Pi(Float<T> y, Float<T> x) => throw new NotImplementedException();
    public static Float<T> BitDecrement(Float<T> x) => throw new NotImplementedException();
    public static Float<T> BitIncrement(Float<T> x) => throw new NotImplementedException();
    public static Float<T> Exp10(Float<T> x) => throw new NotImplementedException();
    public static Float<T> Exp2(Float<T> x) => throw new NotImplementedException();
    public static Float<T> Exp10M1(Float<T> x) => throw new NotImplementedException();
    public static Float<T> Exp2M1(Float<T> x) => throw new NotImplementedException();
    public static Float<T> ExpM1(Float<T> x) => throw new NotImplementedException();
    public static Float<T> FusedMultiplyAdd(Float<T> left, Float<T> right, Float<T> addend) => throw new NotImplementedException();
    public static Float<T> Ieee754Remainder(Float<T> left, Float<T> right) => throw new NotImplementedException();
    public static Float<T> ScaleB(Float<T> x, int n) => throw new NotImplementedException();
    public static Float<T> LogP1(Float<T> x) => throw new NotImplementedException();
    public static Float<T> Log2P1(Float<T> x) => throw new NotImplementedException();
    public static int ILogB(Float<T> x) => throw new NotImplementedException();
    public static bool IsPow2(Float<T> x) => throw new NotImplementedException();
    public static (Float<T> Sin, Float<T> Cos) SinCos(Float<T> x) => throw new NotImplementedException();
    public static (Float<T> SinPi, Float<T> CosPi) SinCosPi(Float<T> x) => throw new NotImplementedException();
    public static Float<T> ReciprocalEstimate(Float<T> x) => throw new NotImplementedException();
    public static Float<T> ReciprocalSqrtEstimate(Float<T> x) => throw new NotImplementedException();
    public static Float<T> MaxNumber(Float<T> x, Float<T> y) => throw new NotImplementedException();
    public static Float<T> MinNumber(Float<T> x, Float<T> y) => throw new NotImplementedException();
    public static Float<T> MaxMagnitude(Float<T> x, Float<T> y) => throw new NotImplementedException();
    public static Float<T> MaxMagnitudeNumber(Float<T> x, Float<T> y) => throw new NotImplementedException();
    public static Float<T> MinMagnitude(Float<T> x, Float<T> y) => throw new NotImplementedException();
    public static Float<T> MinMagnitudeNumber(Float<T> x, Float<T> y) => throw new NotImplementedException();
    internal static Float<T> bcmp(Float<T> a) => throw new NotImplementedException();
    internal static Float<T> band(Float<T> a, Float<T> b) => throw new NotImplementedException();
    internal static Float<T> bor(Float<T> a, Float<T> b) => throw new NotImplementedException();
    internal static Float<T> bxor(Float<T> a, Float<T> b) => throw new NotImplementedException();
    internal int GetExponentByteCount() => throw new NotImplementedException();
    internal int GetExponentShortestBitLength() => throw new NotImplementedException();
    internal int GetSignificandBitLength() => throw new NotImplementedException();
    internal int GetSignificandByteCount() => throw new NotImplementedException();
    internal bool TryWrite(int fl, Span<byte> sp, out int bw) => throw new NotImplementedException();
    internal static bool TryConvertFrom<TOther>(TOther value, int fl, out T result) => throw new NotImplementedException();
    internal static bool TryConvertTo<TOther>(T value, int fl, out TOther result) => throw new NotImplementedException();
    internal static bool IsZero(Float<T> a) => throw new NotImplementedException();
    public static bool IsEvenInteger(Float<T> a) => throw new NotImplementedException();
    public static bool IsInfinity(Float<T> a) => throw new NotImplementedException();
    public static bool IsNegative(Float<T> a) => throw new NotImplementedException();
    public static bool IsNegativeInfinity(Float<T> a) => throw new NotImplementedException();
    public static bool IsNormal(Float<T> a) => throw new NotImplementedException();
    public static bool IsOddInteger(Float<T> a) => throw new NotImplementedException();
    internal static bool IsPositive(Float<T> a) => throw new NotImplementedException();
    public static bool IsPositiveInfinity(Float<T> a) => throw new NotImplementedException();
    public static bool IsRealNumber(Float<T> a) => throw new NotImplementedException();
    public static bool IsSubnormal(Float<T> a) => throw new NotImplementedException();
    #endregion

    #region private
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
    private static readonly int sbi, mbi, bia, lui; //todo: inline, for NET 7 type desc
    static Float()
    {
      int size = sizeof(T); if ((size & 1) != 0) throw new NotSupportedException(nameof(T));
      switch (size) { case 2: sbi = 5; break; case 4: sbi = 8; break; case 8: sbi = 11; break; case <= 16: sbi = 15; break; default: sbi = BitOperations.Log2(unchecked((uint)size)) * 3 + 4; break; }
      mbi = (size << 3) - sbi; bia = (1 << (sbi - 1)) + mbi - 2; lui = (size >> 2) - 1 + ((size & 2) >> 1);
      //check();
      //Debug.WriteLine($"Float{size << 3} significand bits {mbi} exponent bits {((sizeof(T) << 3) - mbi) - 1} decimal digits {mbi * MathF.Log10(2):0.##}");
    }
    internal static int Bits => sizeof(T) << 3;
    internal static int BiasedExponentShift => mbi - 1; // 52
    internal static int ExponentBits => (sizeof(T) << 3) - mbi; // 11
    internal static int MaxBiasedExponent => (1 << sbi) - 1; // 0x07FF;
    internal static int ExponentBias => (1 << (ExponentBits - 1)) - 1; // 1023
    internal static int MinExponent => 1 - ExponentBias; // -1022;
    internal static int MaxExponent => ExponentBias; // +1023;
    internal static int Digits => sizeof(T) != 2 ? (int)((mbi) * 0.30102999566398114) : 5;
    #endregion
  }

#if false
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float32 : IFloat<Float32>
  {
    public override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);

    public static implicit operator Float32(int value) => (Float<UInt32>)value;
    public static implicit operator Float32(Half value) => (Float<UInt32>)value;
    public static implicit operator Float32(float value) => (Float<UInt32>)value;
    public static explicit operator Float32(double value) => (Float<UInt32>)value;
    public static explicit operator Float32(decimal value) => (Float<UInt32>)value;
    public static explicit operator Float32(BigRational value) => (Float<UInt32>)value;
    public static implicit operator Float32(Float<UInt32> p) => new Float32(p);

    public static explicit operator int(Float32 value) => (int)value.p;
    public static explicit operator Half(Float32 value) => (Half)value.p;
    public static explicit operator float(Float32 value) => (float)value.p;
    public static implicit operator double(Float32 value) => (double)value.p;
    public static explicit operator decimal(Float32 value) => (decimal)value.p;
    public static implicit operator BigRational(Float32 value) => (BigRational)value.p;

    public static Float32 operator +(Float32 a) => +a.p;
    public static Float32 operator -(Float32 a) => -a.p;
    public static Float32 operator ++(Float32 a) => a.p + 1;
    public static Float32 operator --(Float32 a) => a.p - 1;
    public static Float32 operator +(Float32 a, Float32 b) => a.p + b.p;
    public static Float32 operator -(Float32 a, Float32 b) => a.p - b.p;
    public static Float32 operator *(Float32 a, Float32 b) => a.p * b.p;
    public static Float32 operator /(Float32 a, Float32 b) => a.p / b.p;
    public static Float32 operator %(Float32 a, Float32 b) => a.p % b.p;

    public static Float32 Pi => Float<UInt32>.Pi;
    public static Float32 Tau => Float<UInt32>.Tau;
    public static Float32 E => Float<UInt32>.E;
    public static Float32 MinValue => Float<UInt32>.MinValue;
    public static Float32 MaxValue => Float<UInt32>.MaxValue;

    private Float32(Float<UInt32> p) => this.p = p;
    readonly Float<UInt32> p;
  }
#endif
}

