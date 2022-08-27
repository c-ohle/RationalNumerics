
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static System.Numerics.BigRational; //todo: remove

#pragma warning disable CS1591 //todo: xml comments

namespace System.Numerics.Generic
{
  [SkipLocalsInit]
  static unsafe class FCPU<T> where T : unmanaged, IFloatType<T>
  {
    internal static T cast(BigRational a)
    {
      var cpu = main_cpu; cpu.push(a); return pop(cpu);
    }
    internal static T cast(int a)
    {
      var cpu = main_cpu; cpu.push(a); return pop(cpu, 0);
    }
    internal static T cast(float a)
    {
      //if (sizeof(T) == 4) return *(T*)&value; //todo: enable  after  tests
      //var cpu = main_cpu; cpu.push(a);
      //T c; pop(cpu, 0, (uint*)&c); return c;
      var cpu = main_cpu;
      var e = FCPU<Float32>.push(cpu, (Float32*)&a); return pop(cpu, e);
    }
    internal static T cast(double a)
    {
      //if (sizeof(T) == 4) return *(T*)&value; //todo: enable  after  tests
      //var cpu = main_cpu; cpu.push(a);
      //T c; pop(cpu, 0, (uint*)&c); return c;
      var cpu = main_cpu; var e = FCPU<Float64>.push(cpu, (Float64*)&a);
      return pop(cpu, e);
    }
    internal static T cast(decimal a)
    {
      var cpu = main_cpu; cpu.push(a); return pop(cpu);
    }
    internal static T cast<U>(U* a) where U : unmanaged, IFloatType<U>
    {
      var cpu = main_cpu; var e = FCPU<U>.push(cpu, a);
      //  int b, t;
      //  if ((b = FloatCPU<U>.mbi) > (t = mbi))
      //  {
      //    //var x1 = (int)Math.Log(b, 2); //34
      //    //var x2 = (int)Math.Log(t, 2); //15
      //    //e -= x1 - x2;
      //  }
      return pop(cpu, e);
    }
    internal static BigRational getr(T* a)
    {
      var cpu = main_cpu; var e = push(cpu, a); //todo: e > mbi
      cpu.pow(2, e); cpu.mul(); return cpu.popr();
    }
    internal static float getf(T* a)
    {
      var cpu = main_cpu; var e = push(cpu, a);
      var c = FCPU<Float32>.pop(cpu, e); return *(float*)&c;
    }
    internal static double getd(T* a)
    {
      var cpu = main_cpu; var e = push(cpu, a);
      var c = FCPU<Float64>.pop(cpu, e); return *(double*)&c;
    }
    internal static decimal getm(T* a)
    {
      var r = getr(a); return (decimal)r; //todo: inline
    }
    internal static T add(T* a, T* b)
    {
      var cpu = main_cpu; int ae = push(cpu, a), be = push(cpu, b), l = ae < be ? 1 : 0, e;
      cpu.shr(l == 0 ? (e = ae) - be : (e = be) - ae, l); cpu.add(); return pop(cpu, e);
    }
    internal static T sub(T* a, T* b)
    {
      var cpu = main_cpu; int ae = push(cpu, a), be = push(cpu, b), l = ae < be ? 1 : 0, e;
      cpu.shr(l == 0 ? (e = ae) - be : (e = be) - ae, l); cpu.sub();
      return pop(cpu, e);
    }
    internal static T mul(T* a, T* b)
    {
      var cpu = main_cpu;
      int eu = push(cpu, a), ev = push(cpu, b); cpu.mul();
      return pop(cpu, eu + ev);
    }
    internal static T div(T* a, T* b)
    {
      var cpu = main_cpu;
      var eu = push(cpu, a); cpu.shl(mbi);
      var ev = push(cpu, b); cpu.idiv();
      return pop(cpu, eu - ev - mbi);
    }
    internal static T mod(T* a, T* b)
    {
      //return a - Truncate(a / b) * b; //todo: opt. inline
      var t = div(a, b); t = trunc(&t);
      t = mul(&t, b); t = sub(a, &t); return t;
      //return a[0] - Float<T>.Truncate(a[0] / b[0]) * b[0];
    }
    internal static T rnd(T* a, int digits, MidpointRounding mode)
    {
      if (digits == 0) return trunc(a);
      var cpu = main_cpu; var b = pow10(cpu, digits);
      var c = mul(a, &b); //var c = a[0] * b;       
      var e = push(cpu, &c); cpu.shr(-e);
      c = pop(cpu, 0); c = div(&c, &b); return c;
      //a *= b; var e = push(cpu, (uint*)&a); cpu.shr(-e);
      //pop(cpu, 0, (uint*)&a); a /= b; return a; // (TFloat<T>)BigRational.Round((BigRational)a, digits, mode);
    }
    internal static T trunc(T* a)
    {
      var cpu = main_cpu; var e = push(cpu, a);
      cpu.shl(e); return pop(cpu, 0);
    }

    internal static T sqr(T* a) { return mul(a, a); } //todo: sqr
    internal static T inc(T* a) { var b = cast(1); return add(a, &b); }
    internal static T dec(T* a) { var b = cast(1); return sub(a, &b); }
    internal static bool equ(T* a, T* b)
    {
      //if (a == b) return true; //todo: check sort's
      uint n = unchecked((uint)sizeof(T)), i = 0, c;
      uint* u = (uint*)a, v = (uint*)b;
      for (c = n >> 2; i < c; i++) if (u[i] != v[i]) return false;
      if (c << 2 < n) if (*(ushort*)&u[c] != *(ushort*)&v[c]) return false;
      return true;
    }
    internal static int cmp(T* a, T* b)
    {
      uint* u = (uint*)a, v = (uint*)b; // if (u == v) return 0; //todo: check sort's
      var ha = (sizeof(T) & 2) == 0 ? u[lui] : (u[lui] & 0xffff) << 16;
      var hb = (sizeof(T) & 2) == 0 ? v[lui] : (v[lui] & 0xffff) << 16;
      var sa = ha == 0 ? 0 : (ha & 0x80000000) != 0 ? -1 : +1;
      var sb = hb == 0 ? 0 : (hb & 0x80000000) != 0 ? -1 : +1;
      if (sa != sb) return Math.Sign(sa - sb); if (sa == 0) return 0;
      var cpu = main_cpu; int ea = push(cpu, a), eb = push(cpu, b);
      if (ea != eb) { cpu.pop(2); return ea > eb ? sa : -sa; }
      ea = cpu.cmp(); cpu.pop(2); return ea * sa;
    }

    internal static int sign(T* a)
    {
      var h = (sizeof(T) & 2) == 0 ? ((uint*)a)[lui] : (((uint*)a)[lui] & 0xffff) << 16;
      return h == 0 ? 0 : (h & 0x80000000) != 0 ? -1 : +1;
    }
    internal static T neg(T* a)
    {
      if (sign(a) <= 0) return *a;
      T b = *a; uint* u = (uint*)&b; if ((sizeof(T) & 2) == 0) u[lui] ^= 0x80000000; else u[lui] ^= 0x8000; return b;
    }
    internal static T abs(T* a)
    {
      return sign(a) >= 0 ? *a : neg(a);
    }

    internal static T sqrt(T* a) => cast(BigRational.Sqrt(getr(a), Digits)); //todo: impl.
    internal static T e() => cast(BigRational.Exp(1, Digits)); //todo: impl.
    internal static T pi() => cast(BigRational.Pi(Digits)); //todo: impl.
    internal static T tau() => cast(BigRational.Tau(Digits)); //todo: impl.
    internal static T max()
    {
      T u; new Span<byte>(&u, sizeof(T)).Fill(0xff); //todo: rem
      ref var p = ref ((uint*)&u)[lui]; p = 0x7fffffffu ^ (1u << ((mbi - 1) & 31));
      if ((sizeof(T) & 2) != 0) p = sizeof(T) != 2 ? p >> 16 : 0x7bff; return u;
    }
    internal static T min() { var t = max(); neg(&t); return t; }
    internal static T sin(T* a) => cast(BigRational.Sin(getr(a), Digits));

    internal static int push(CPU cpu, T* v)
    {
      var p = (uint*)v; var l = lui; if ((sizeof(T) & 2) != 0) if (l != 0) p[l] <<= 16; else p[0] = (p[0] & 0x03FFu) | ((p[0] & ~0x03FFu) << 16);
      if (p[l] == 0) { cpu.push(); return 0; }
      var t = stackalloc uint[l + 4]; var x = 1u << (mbi - 1);
      t[0] = (p[l] & 0x80000000) | unchecked((uint)l + 1);
      t[l + 1] = (p[l] & (x - 1)) | x; t[l + 2] = t[l + 3] = 1;
      for (int i = 0; i < l; i++) t[i + 1] = p[i];
      cpu.push(new ReadOnlySpan<uint>(t, l + 4));
      return unchecked((int)((p[l] >> (31 - sbi)) & ((1 << sbi) - 1))) - bia;
    }
    internal static T pop(CPU cpu, int e)
    {
      int s = unchecked((int)cpu.msb()); if (s == 0) { cpu.pop(); return default; }
      int d = s - mbi, l; //todo: the point inf, nan starts
      if (d > 0) { var c = cpu.bt(d - 1); cpu.shr(d); if (c) cpu.inc(); } //X64 rnd 
      else if (d != 0) cpu.shl(-d);
      var sp = cpu.gets(cpu.mark() - 1); T v; var p = (uint*)&v;
      p[l = lui] = (sp[0] & 0x80000000) | (unchecked((uint)((e + d) + bia)) << (31 - sbi)) | (sp[l + 1] & ((1u << (mbi - 1)) - 1u));
      if ((sizeof(T) & 2) != 0) if (l != 0) p[l] >>= 16; else p[l] = (p[l] & 0xffff) | p[l] >> 16;
      for (int i = 0; i < l; i++) p[i] = sp[i + 1]; cpu.pop(); return v;
    }
    internal static T pop(CPU cpu)
    {
      var s = cpu.sign(); if (s == 0) { cpu.pop(); return default; }
      cpu.mod(8); T a = pop(cpu, 0), b = pop(cpu, 0); a = div(&b, &a); return a; //todo: div inline
    }
    internal static T pow10(CPU cpu, int y)
    {
      T x = cast(1), z = cast(10); uint e = unchecked((uint)(y >= 0 ? y : -y));
      for (; ; e >>= 1)
      {
        if ((e & 1) != 0) x = mul(&x, &z);
        if (e <= 1) break; z = sqr(&z); //todo: sqr
      }
      if (y < 0) { z = cast(1); x = div(&z, &x); } // x = 1 / x; //static float inv(float x) { uint* i = (uint*)&x; *i = 0x7F000000 - *i; return x; }
      return x;
    }

    internal static T band(T* a, T* b) { return default; }
    internal static T bor(T* a, T* b) { return default; }
    internal static T bxor(T* a, T* b) { return default; }
    internal static T binv(T* a) { return default; }  
    internal static T nan() => default;
    internal static T eps() => default;
    internal static T neginf() => default;
    internal static T negzero() => default;
    internal static T posinf() => default;
    internal static T allbs() => default;
    internal static int btc(int i)
    {
      switch (i)
      {
        case 0: break; // int IFloatingPoint<T>.GetExponentByteCount() => FCPU<T>.btc(0);
        case 1: break; // int IFloatingPoint<T>.GetExponentShortestBitLength() => FCPU<T>.btc(1);
        case 2: break; // int IFloatingPoint<T>.GetSignificandBitLength() => FCPU<T>.btc(2);
        case 3: break; // int IFloatingPoint<T>.GetSignificandByteCount() => FCPU<T>.btc(3);
      }
      return 0;
    }

    internal static bool isevi(T* a) => default;
    internal static bool isint(T* a) => default; //IsFinite(a) && a == Truncate(a);
    internal static bool isoddint(T* a) => default;
    internal static bool ispos(T* a) => default;
    internal static bool isrel(T* a) => default;
    internal static bool isfin(T* a) => default;
    internal static bool isinfin(T* a) => default;
    internal static bool isnan(T* a) => default;
    internal static bool isneg(T* a) => default;
    internal static bool isneginf(T* a) => default;
    internal static bool isnorm(T* a) => default;
    internal static bool isposinf(T* a) => default;
    internal static bool issubnorm(T* a) => default;
    internal static bool ispow2(T* a) => default;

    internal static string ToString(T value, string? format, IFormatProvider? provider = default)
    {
      if (format != default && format.Length == 0) provider = NumberFormatInfo.InvariantInfo;
      Span<char> sp = stackalloc char[Digits + 16];
      if (!TryFormat(value, sp, out var ns, format, provider))
      {
        int n; sp.Slice(0, 2).CopyTo(new Span<char>(&n, 2)); sp = stackalloc char[n]; //todo: bigbuf
        TryFormat(value, sp, out ns, format, provider); Debug.Assert(ns != 0);
      }
      return sp.Slice(0, ns).ToString();
    }
    internal static bool TryFormat(T value, Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
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
      var cpu = main_cpu; var es = 0;
      var e = push(cpu, &value);

      var ep = (int)((e + mbi) * 0.30102999566398114); //Math.Log(2) / Math.Log(10) Math.Log10(2);
      var d1 = Math.Abs(ep); var d2 = Math.Abs(dig); var dd = d1 - d2;
      if (dd > 10)// && provider == null) //todo: opt. F? check
      {
        if (dig <= 0) { }
        else if (ep > 0)
        {
          cpu.pop(); var p = pow10(cpu, 3 - dd); p = mul(&value, &p); //todo: inline //var p = value * pow10(cpu, 3 - dd);
          e = push(cpu, &p); es = dd - 3;
        }
        else if (ep < 0)
        {
          cpu.pop(); var p = pow10(cpu, dd - 3); p = mul(&value, &p); //todo: inline //var p = value * pow10(cpu, dd - 3); //todo: opt. bias 
          e = push(cpu, &p); es = 3 - dd;
        }
        else { }
      }

      cpu.pow(2, e); cpu.mul();
      var n = tos(dest, cpu, fmt, dig, rnd, es, info.NumberDecimalSeparator[0] == ',' ? 0x04 : 0);
      if (n < 0) { dig = -n; goto ex; }
      charsWritten = n; return true; ex:
      charsWritten = 0; if (dest.Length >= 2) new Span<char>(&dig, 2).CopyTo(dest); return false;
    }
    internal static int GetHashCode(T value)
    {
      var p = (uint*)&value; uint n = unchecked((uint)sizeof(T)), h = 0;
      for (uint i = 0, c = n >> 2; i < c; i++) h = p[i] ^ ((h << 7) | (h >> 25));
      if ((n & 2) != 0) h ^= p[n >> 2] & 0xffff;
      return unchecked((int)h);
    }
    internal static bool Equals(T value, object? obj)
    {
      return false;
    }
    internal static bool TryWrite(T value, int fl, Span<byte> sp, out int bw) { bw = 0; return false; }
    internal static bool TryConvertFrom<TOther>(TOther value, int fl, out T result) { result = default; return false; }
    internal static bool TryConvertTo<TOther>(Float<T> p, int fl, out TOther result) { result = default!; return false; }

    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly int sbi, mbi, bia, lui; //todo: inline, for NET 7 type desc
    static FCPU()
    {
      int size = sizeof(T); if ((size & 1) != 0) throw new NotSupportedException(nameof(T));
      switch (size) { case 2: sbi = 5; break; case 4: sbi = 8; break; case 8: sbi = 11; break; case <= 16: sbi = 15; break; default: sbi = BitOperations.Log2(unchecked((uint)size)) * 3 + 4; break; }
      mbi = (size << 3) - sbi; bia = (1 << (sbi - 1)) + mbi - 2; lui = (size >> 2) - 1 + ((size & 2) >> 1);
      Debug.WriteLine($"  FloatCPU<{typeof(T)}> {sizeof(T) << 3} bit");
      //Debug.WriteLine($"  significand bits {mbi} exponent bits {((sizeof(T) << 3) - mbi) - 1} decimal digits {mbi * MathF.Log10(2):0.##} (int){Digits}");
    }
    //internal static int Bits => sizeof(T) << 3;
    //internal static int BiasedExponentShift => mbi - 1; // 52
    //internal static int ExponentBits => (sizeof(T) << 3) - mbi; // 11
    //internal static int MaxBiasedExponent => (1 << sbi) - 1; // 0x07FF;
    //internal static int ExponentBias => (1 << (ExponentBits - 1)) - 1; // 1023
    //internal static int MinExponent => 1 - ExponentBias; // -1022;
    //internal static int MaxExponent => ExponentBias; // +1023;
    internal static int Digits => sizeof(T) != 2 ? (int)((mbi) * 0.30102999566398114) : 4;
  }

#if NET7_0

  public unsafe interface IFloatType<T> : IBinaryFloatingPointIeee754<T>, IMinMaxValue<T> where T : unmanaged, IFloatType<T>
  {
    static T IUnaryPlusOperators<T, T>.operator +(T value) => value;
    static T IUnaryNegationOperators<T, T>.operator -(T value) => FCPU<T>.neg(&value);
    static T IAdditionOperators<T, T, T>.operator +(T a, T b) => FCPU<T>.add(&a, &b);
    static T ISubtractionOperators<T, T, T>.operator -(T a, T b) => FCPU<T>.sub(&a, &b);
    static T IMultiplyOperators<T, T, T>.operator *(T a, T b) => FCPU<T>.mul(&a, &b);
    static T IDivisionOperators<T, T, T>.operator /(T a, T b) => FCPU<T>.div(&a, &b);
    static T IModulusOperators<T, T, T>.operator %(T a, T b) => FCPU<T>.mod(&a, &b);
    static T IIncrementOperators<T>.operator ++(T value) => FCPU<T>.inc(&value);
    static T IDecrementOperators<T>.operator --(T value) => FCPU<T>.dec(&value);
    static bool IEqualityOperators<T, T, bool>.operator ==(T a, T b) => FCPU<T>.equ(&a, &b);
    static bool IEqualityOperators<T, T, bool>.operator !=(T a, T b) => !FCPU<T>.equ(&a, &b);
    static bool IComparisonOperators<T, T, bool>.operator <(T a, T b) => FCPU<T>.cmp(&a, &b) < 0;
    static bool IComparisonOperators<T, T, bool>.operator >(T a, T b) => FCPU<T>.cmp(&a, &b) > 0;
    static bool IComparisonOperators<T, T, bool>.operator <=(T a, T b) => FCPU<T>.cmp(&a, &b) <= 0;
    static bool IComparisonOperators<T, T, bool>.operator >=(T a, T b) => FCPU<T>.cmp(&a, &b) >= 0;
    static T IBitwiseOperators<T, T, T>.operator &(T a, T b) => FCPU<T>.band(&a, &b);
    static T IBitwiseOperators<T, T, T>.operator |(T a, T b) => FCPU<T>.bor(&a, &b);
    static T IBitwiseOperators<T, T, T>.operator ^(T a, T b) => FCPU<T>.bxor(&a, &b);
    static T IBitwiseOperators<T, T, T>.operator ~(T a) => FCPU<T>.binv(&a);

    static T IFloatingPointConstants<T>.E => FCPU<T>.e();
    static T IFloatingPointConstants<T>.Pi => FCPU<T>.pi();
    static T IFloatingPointConstants<T>.Tau => FCPU<T>.tau();
    static T IMinMaxValue<T>.MinValue => FCPU<T>.min();
    static T IMinMaxValue<T>.MaxValue => FCPU<T>.max();
    static T INumberBase<T>.Abs(T value) => FCPU<T>.abs(&value);
    static T IRootFunctions<T>.Sqrt(T x) => FCPU<T>.sqrt(&x);

    static int INumberBase<T>.Radix => 2;
    static T ISignedNumber<T>.NegativeOne => FCPU<T>.cast(-1);
    static T INumberBase<T>.One => FCPU<T>.cast(1);
    static T INumberBase<T>.Zero => default;
    static T IAdditiveIdentity<T, T>.AdditiveIdentity => default;
    static T IMultiplicativeIdentity<T, T>.MultiplicativeIdentity => FCPU<T>.cast(1);
    static T IFloatingPointIeee754<T>.NaN => FCPU<T>.nan();
    static T IFloatingPointIeee754<T>.Epsilon => FCPU<T>.eps();
    static T IFloatingPointIeee754<T>.NegativeInfinity => FCPU<T>.neginf();
    static T IFloatingPointIeee754<T>.NegativeZero => FCPU<T>.negzero();
    static T IFloatingPointIeee754<T>.PositiveInfinity => FCPU<T>.posinf();

    static T IBinaryNumber<T>.AllBitsSet => FCPU<T>.allbs();
    static bool INumberBase<T>.IsEvenInteger(T value) => FCPU<T>.isevi(&value);
    static bool INumberBase<T>.IsFinite(T value) => FCPU<T>.isfin(&value);
    static bool INumberBase<T>.IsInfinity(T value) => FCPU<T>.isinfin(&value);
    static bool INumberBase<T>.IsInteger(T value) => FCPU<T>.isint(&value);
    static bool INumberBase<T>.IsNaN(T value) => FCPU<T>.isnan(&value);
    static bool INumberBase<T>.IsNegative(T value) => FCPU<T>.isneg(&value);
    static bool INumberBase<T>.IsNegativeInfinity(T value) => FCPU<T>.isneginf(&value);
    static bool INumberBase<T>.IsNormal(T value) => FCPU<T>.isnorm(&value);
    static bool INumberBase<T>.IsOddInteger(T value) => FCPU<T>.isoddint(&value);
    static bool INumberBase<T>.IsPositive(T value) => FCPU<T>.ispos(&value);
    static bool INumberBase<T>.IsPositiveInfinity(T value) => FCPU<T>.isposinf(&value);
    static bool IBinaryNumber<T>.IsPow2(T value) => FCPU<T>.ispow2(&value);
    static bool INumberBase<T>.IsRealNumber(T value) => FCPU<T>.isrel(&value);
    static bool INumberBase<T>.IsSubnormal(T value) => FCPU<T>.issubnorm(&value);
    static bool INumberBase<T>.IsZero(T value) => value == default;
    static bool INumberBase<T>.IsCanonical(T value) => true;
    static bool INumberBase<T>.IsComplexNumber(T value) => false;
    static bool INumberBase<T>.IsImaginaryNumber(T value) => false;

    static T INumberBase<T>.MaxMagnitude(T x, T y) => Float<T>.MaxMagnitude(x, y);
    static T INumberBase<T>.MaxMagnitudeNumber(T x, T y) => Float<T>.MaxMagnitudeNumber(x, y);
    static T INumberBase<T>.MinMagnitude(T x, T y) => Float<T>.MinMagnitude(x, y);
    static T INumberBase<T>.MinMagnitudeNumber(T x, T y) => Float<T>.MinMagnitudeNumber(x, y);

    static T IPowerFunctions<T>.Pow(T x, T y) => Float<T>.Pow(x, y);

    static T IRootFunctions<T>.Hypot(T x, T y) => Float<T>.Hypot(x, y);
    static T IRootFunctions<T>.Cbrt(T x) => Float<T>.Cbrt(x);
    static T IRootFunctions<T>.RootN(T x, int n) => Float<T>.RootN(x, n);

    static T ILogarithmicFunctions<T>.Log(T x) => Float<T>.Log(x);
    static T ILogarithmicFunctions<T>.Log(T x, T newBase) => Float<T>.Log(x, newBase);
    static T ILogarithmicFunctions<T>.Log10(T x) => Float<T>.Log10(x);
    static T ILogarithmicFunctions<T>.Log2(T x) => Float<T>.Log2(x);
    static T IBinaryNumber<T>.Log2(T x) => Float<T>.Log2(x);

    static T IExponentialFunctions<T>.Exp(T x) => Float<T>.Exp(x);
    static T IExponentialFunctions<T>.Exp10(T x) => Float<T>.Exp10(x);
    static T IExponentialFunctions<T>.Exp2(T x) => Float<T>.Exp2(x);

    static T ITrigonometricFunctions<T>.Sin(T x) => FCPU<T>.sin(&x);
    static T ITrigonometricFunctions<T>.Acos(T x) => Float<T>.Acos(x);
    static T ITrigonometricFunctions<T>.Asin(T x) => Float<T>.Asin(x);
    static T ITrigonometricFunctions<T>.AcosPi(T x) => Float<T>.AcosPi(x);
    static T ITrigonometricFunctions<T>.AsinPi(T x) => Float<T>.AsinPi(x);
    static T ITrigonometricFunctions<T>.Atan(T x) => Float<T>.Atan(x);
    static T ITrigonometricFunctions<T>.AtanPi(T x) => Float<T>.AtanPi(x);
    static T ITrigonometricFunctions<T>.Cos(T x) => Float<T>.Cos(x);
    static T ITrigonometricFunctions<T>.CosPi(T x) => Float<T>.CosPi(x);
    static T ITrigonometricFunctions<T>.SinPi(T x) => Float<T>.SinPi(x);
    static T ITrigonometricFunctions<T>.Tan(T x) => Float<T>.Tan(x);
    static T ITrigonometricFunctions<T>.TanPi(T x) => Float<T>.TanPi(x);
    static (T Sin, T Cos) ITrigonometricFunctions<T>.SinCos(T x) { var t = Float<T>.SinCos(x); return (t.Sin, t.Cos); }
    static (T SinPi, T CosPi) ITrigonometricFunctions<T>.SinCosPi(T x) { var t = Float<T>.SinCosPi(x); return (t.SinPi, t.CosPi); }

    static T IHyperbolicFunctions<T>.Acosh(T x) => Float<T>.Acosh(x);
    static T IHyperbolicFunctions<T>.Atanh(T x) => Float<T>.Atanh(x);
    static T IHyperbolicFunctions<T>.Cosh(T x) => Float<T>.Cosh(x);
    static T IHyperbolicFunctions<T>.Asinh(T x) => Float<T>.Asinh(x);
    static T IHyperbolicFunctions<T>.Sinh(T x) => Float<T>.Sinh(x);
    static T IHyperbolicFunctions<T>.Tanh(T x) => Float<T>.Tanh(x);

    static T IFloatingPointIeee754<T>.Atan2(T y, T x) => Float<T>.Atan2(y, x);
    static T IFloatingPointIeee754<T>.ScaleB(T x, int n) => Float<T>.ScaleB(x, n);
    static int IFloatingPointIeee754<T>.ILogB(T x) => Float<T>.ILogB(x);
    static T IFloatingPointIeee754<T>.Atan2Pi(T y, T x) => Float<T>.Atan2Pi(y, x);
    static T IFloatingPointIeee754<T>.FusedMultiplyAdd(T a, T b, T c) => Float<T>.FusedMultiplyAdd(a, b, c);
    static T IFloatingPointIeee754<T>.Ieee754Remainder(T a, T b) => Float<T>.Ieee754Remainder(a, b);
    static T IFloatingPointIeee754<T>.BitDecrement(T x) => Float<T>.BitDecrement(x);
    static T IFloatingPointIeee754<T>.BitIncrement(T x) => Float<T>.BitIncrement(x);

    static T IFloatingPoint<T>.Round(T x, int digits, MidpointRounding mode) => Float<T>.Round(x, digits, mode);
    int IFloatingPoint<T>.GetExponentByteCount() => FCPU<T>.btc(0);
    int IFloatingPoint<T>.GetExponentShortestBitLength() => FCPU<T>.btc(1);
    int IFloatingPoint<T>.GetSignificandBitLength() => FCPU<T>.btc(2);
    int IFloatingPoint<T>.GetSignificandByteCount() => FCPU<T>.btc(3);

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ((Float<T>)(T)this).ToString(format, formatProvider);
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => ((Float<T>)(T)this).TryFormat(destination, out charsWritten, format, provider);

    int IComparable.CompareTo(object? obj) => ((Float<T>)(T)this).CompareTo(obj);
    int IComparable<T>.CompareTo(T other) => ((Float<T>)(T)this).CompareTo(other);
    bool IEquatable<T>.Equals(T other) => ((Float<T>)(T)this).Equals(other);

    static T IParsable<T>.Parse(string value, IFormatProvider? provider) { var t = Float<T>.Parse(value, provider); return t; }
    static bool IParsable<T>.TryParse(string? value, IFormatProvider? provider, out T result) { var t = Float<T>.TryParse(value, provider, out var r); result = r; return t; }
    static T ISpanParsable<T>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) { var t = Float<T>.TryParse(s, provider, out var r); return r; }
    static bool ISpanParsable<T>.TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out T result) { var t = Float<T>.TryParse(value, provider, out var r); result = r; return t; }
    static T INumberBase<T>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) { var t = Float<T>.TryParse(s, style, provider, out var r); return r; }
    static T INumberBase<T>.Parse(string s, NumberStyles style, IFormatProvider? provider) { var t = Float<T>.TryParse(s, style, provider, out var r); return r; }
    static bool INumberBase<T>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out T result) { var t = Float<T>.TryParse(s, style, provider, out var r); result = r; return t; }
    static bool INumberBase<T>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out T result) { var t = Float<T>.TryParse(s, style, provider, out var r); result = r; return t; }

    static bool INumberBase<T>.TryConvertFromChecked<TOther>(TOther value, out T result) => FCPU<T>.TryConvertFrom<TOther>(value, 0, out result);
    static bool INumberBase<T>.TryConvertFromSaturating<TOther>(TOther value, out T result) => FCPU<T>.TryConvertFrom<TOther>(value, 1, out result);
    static bool INumberBase<T>.TryConvertFromTruncating<TOther>(TOther value, out T result) => FCPU<T>.TryConvertFrom<TOther>(value, 2, out result);
    static bool INumberBase<T>.TryConvertToChecked<TOther>(T value, out TOther result) where TOther : default => FCPU<T>.TryConvertTo<TOther>(value, 0, out result);
    static bool INumberBase<T>.TryConvertToSaturating<TOther>(T value, out TOther result) where TOther : default => FCPU<T>.TryConvertTo<TOther>(value, 1, out result);
    static bool INumberBase<T>.TryConvertToTruncating<TOther>(T value, out TOther result) where TOther : default => FCPU<T>.TryConvertTo<TOther>(value, 2, out result);

    bool IFloatingPoint<T>.TryWriteExponentBigEndian(Span<byte> sp, out int bw) => FCPU<T>.TryWrite((T)this, 0, sp, out bw);
    bool IFloatingPoint<T>.TryWriteExponentLittleEndian(Span<byte> sp, out int bw) => FCPU<T>.TryWrite((T)this, 1, sp, out bw);
    bool IFloatingPoint<T>.TryWriteSignificandBigEndian(Span<byte> sp, out int bw) => FCPU<T>.TryWrite((T)this, 2, sp, out bw);
    bool IFloatingPoint<T>.TryWriteSignificandLittleEndian(Span<byte> sp, out int bw) => FCPU<T>.TryWrite((T)this, 3, sp, out bw);

  }
#else //todo: compat: IComparable<T>, IEquatable<T>, ISpanFormattable 
  public interface IFloatType<T> where T : unmanaged
  {
  }
#endif

  //todo: remove, currently only needed for conversions -> cpu cap
  [StructLayout(LayoutKind.Sequential, Size = 4)]
  struct Float32 : IFloatType<Float32> { }
  [StructLayout(LayoutKind.Sequential, Size = 8)]
  struct Float64 : IFloatType<Float64> { }

  [DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly unsafe struct Float<T> : IFloatType<Float<T>> where T : unmanaged, IFloatType<T>
  {
    public readonly override string ToString() => ToString(default);
    public readonly string ToString(string? format, IFormatProvider? provider = default) => FCPU<Float<T>>.ToString(this, format, provider);
    public readonly bool TryFormat(Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => FCPU<Float<T>>.TryFormat(this, dest, out charsWritten, format, provider);
    public readonly override int GetHashCode() => FCPU<Float<T>>.GetHashCode(this);
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => FCPU<Float<T>>.Equals(this, obj);
    public readonly int CompareTo(Float<T> b) { var a = this; return FCPU<Float<T>>.cmp(&a, &b); }
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float<T> b ? this.CompareTo(b) : throw new ArgumentException();
    public readonly bool Equals(Float<T> other) { var a = this; return FCPU<Float<T>>.equ(&a, &other); }
    public static Float<T> Parse(ReadOnlySpan<char> value, IFormatProvider? provider = null) { TryParse(value, provider, out var r); return r; }
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out Float<T> result)
    {
      //todo: rnd to digits in str
      var info = provider != null ? NumberFormatInfo.GetInstance(provider) : null;
      var cpu = main_cpu; cpu.tor(value, 10, info != null ? info.NumberDecimalSeparator[0] : default);
      result = FCPU<Float<T>>.pop(cpu); return true;
    }
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Float<T> result)
    {
      return TryParse(s, provider, out result); //todo: style checks
    }
    public static Float<T> Cast<B>(Float<B> b) where B : unmanaged, IFloatType<B> => FCPU<Float<T>>.cast(&b);

    public static implicit operator Float<T>(int value) => FCPU<Float<T>>.cast(value);
    public static implicit operator Float<T>(float value) => FCPU<Float<T>>.cast(value);
    public static implicit operator Float<T>(double value) => FCPU<Float<T>>.cast(value);
    public static implicit operator Float<T>(decimal value) => FCPU<Float<T>>.cast(value);
    public static implicit operator Float<T>(T value) => *(Float<T>*)&value;
    public static explicit operator Float<T>(BigRational value) => FCPU<Float<T>>.cast(value);

    public static implicit operator T(Float<T> value) => *(T*)&value;
    public static explicit operator float(Float<T> value) => FCPU<Float<T>>.getf(&value);
    public static explicit operator double(Float<T> value) => FCPU<Float<T>>.getd(&value);
    public static explicit operator decimal(Float<T> value) => FCPU<Float<T>>.getm(&value);
    public static implicit operator BigRational(Float<T> value) => FCPU<Float<T>>.getr(&value);

    public static Float<T> operator +(Float<T> a) => a;
    public static Float<T> operator -(Float<T> a) => FCPU<Float<T>>.neg(&a);
    public static Float<T> operator +(Float<T> a, Float<T> b) => FCPU<Float<T>>.add(&a, &b);
    public static Float<T> operator -(Float<T> a, Float<T> b) => FCPU<Float<T>>.sub(&a, &b);
    public static Float<T> operator *(Float<T> a, Float<T> b) => FCPU<Float<T>>.mul(&a, &b);
    public static Float<T> operator /(Float<T> a, Float<T> b) => FCPU<Float<T>>.div(&a, &b);
    public static Float<T> operator %(Float<T> a, Float<T> b) => FCPU<Float<T>>.mod(&a, &b);

    public static Float<T> operator ++(Float<T> a) => FCPU<Float<T>>.inc(&a);
    public static Float<T> operator --(Float<T> a) => FCPU<Float<T>>.dec(&a);

    public static bool operator ==(Float<T> a, Float<T> b) => FCPU<Float<T>>.equ(&a, &b);
    public static bool operator !=(Float<T> a, Float<T> b) => !FCPU<Float<T>>.equ(&a, &b);
    public static bool operator <=(Float<T> a, Float<T> b) => FCPU<Float<T>>.cmp(&a, &b) <= 0;
    public static bool operator >=(Float<T> a, Float<T> b) => FCPU<Float<T>>.cmp(&a, &b) >= 0;
    public static bool operator <(Float<T> a, Float<T> b) => FCPU<Float<T>>.cmp(&a, &b) < 0;
    public static bool operator >(Float<T> a, Float<T> b) => FCPU<Float<T>>.cmp(&a, &b) > 0;

    public static Float<T> Abs(Float<T> a) => FCPU<Float<T>>.abs(&a);
    public static Float<T> Sqrt(Float<T> a) => FCPU<Float<T>>.sqrt(&a);
    public static int Sign(Float<T> a) => FCPU<Float<T>>.sign(&a);
    public static Float<T> Truncate(Float<T> a) => FCPU<Float<T>>.trunc(&a);
    public static Float<T> Min(Float<T> a, Float<T> b) => FCPU<Float<T>>.cmp(&a, &b) <= 0 ? a : b;
    public static Float<T> Max(Float<T> a, Float<T> b) => FCPU<Float<T>>.cmp(&a, &b) >= 0 ? a : b;
    public static Float<T> Round(Float<T> a, int digits = 0, MidpointRounding mode = MidpointRounding.ToEven) => FCPU<Float<T>>.rnd(&a, digits, mode);

    public static Float<T> MaxMagnitude(T x, T y) => default;
    public static Float<T> MaxMagnitudeNumber(T x, T y) => default;
    public static Float<T> MinMagnitude(T x, T y) => default;
    public static Float<T> MinMagnitudeNumber(T x, T y) => default;
    public static Float<T> ScaleB(T x, int n) { return default; }
    public static Float<T> Exp10(T x) => default;
    public static int ILogB(T x) => default;
    public static Float<T> Atan2Pi(T y, T x) => default;
    public static Float<T> Exp2(T x) => default;
    public static Float<T> BitIncrement(T x) => default;
    public static Float<T> BitDecrement(T x) => default;
    public static Float<T> FusedMultiplyAdd(T a, T b, T c) => default;
    public static Float<T> Ieee754Remainder(T a, T b) => default;

    public static Float<T> Floor(Float<T> a) => (Float<T>)BigRational.Floor((BigRational)a);
    public static Float<T> Ceiling(Float<T> a) => (Float<T>)BigRational.Ceiling((BigRational)a);
    public static Float<T> Pow2(Float<T> x) => (Float<T>)BigRational.Pow2((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Cbrt(Float<T> x) => (Float<T>)BigRational.Cbrt((BigRational)x, FCPU<T>.Digits);
    public static Float<T> RootN(Float<T> x, int n) => (Float<T>)BigRational.RootN((BigRational)x, n, FCPU<T>.Digits);
    public static Float<T> Hypot(Float<T> x, Float<T> y) => (Float<T>)BigRational.Hypot((BigRational)x, (BigRational)y, FCPU<T>.Digits);
    public static Float<T> Log2(Float<T> x) => (Float<T>)BigRational.Log2((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Log10(Float<T> x) => (Float<T>)BigRational.Log10((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Log(Float<T> x) => (Float<T>)BigRational.Log((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Log(Float<T> x, Float<T> newBase) => (Float<T>)BigRational.Log((BigRational)x, (BigRational)newBase, FCPU<T>.Digits);
    public static Float<T> Exp(Float<T> x) => (Float<T>)BigRational.Exp((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Sin(Float<T> x) => (Float<T>)BigRational.Sin((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Cos(Float<T> x) => (Float<T>)BigRational.Cos((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Tan(Float<T> x) => (Float<T>)BigRational.Tan((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Asin(Float<T> x) => (Float<T>)BigRational.Asin((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Acos(Float<T> x) => (Float<T>)BigRational.Acos((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Atan(Float<T> x) => (Float<T>)BigRational.Atan((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Atan2(Float<T> y, Float<T> x) => (Float<T>)BigRational.Atan2((BigRational)y, (BigRational)x, FCPU<T>.Digits);
    public static Float<T> Asinh(Float<T> x) => (Float<T>)BigRational.Asinh((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Acosh(Float<T> x) => (Float<T>)BigRational.Acosh((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Atanh(Float<T> x) => (Float<T>)BigRational.Atanh((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Sinh(Float<T> x) => (Float<T>)BigRational.Asinh((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Cosh(Float<T> x) => (Float<T>)BigRational.Acosh((BigRational)x, FCPU<T>.Digits);
    public static Float<T> Tanh(Float<T> x) => (Float<T>)BigRational.Tanh((BigRational)x, FCPU<T>.Digits);
    public static Float<T> SinPi(Float<T> x) => (Float<T>)BigRational.SinPi((BigRational)x, FCPU<T>.Digits);
    public static Float<T> TanPi(Float<T> x) => (Float<T>)BigRational.TanPi((BigRational)x, FCPU<T>.Digits);
    public static Float<T> AcosPi(Float<T> x) => (Float<T>)BigRational.AcosPi((BigRational)x, FCPU<T>.Digits);
    public static Float<T> AsinPi(Float<T> x) => (Float<T>)BigRational.AsinPi((BigRational)x, FCPU<T>.Digits);
    public static Float<T> AtanPi(Float<T> x) => (Float<T>)BigRational.AtanPi((BigRational)x, FCPU<T>.Digits);
    public static Float<T> CosPi(Float<T> x) => (Float<T>)BigRational.CosPi((BigRational)x, FCPU<T>.Digits);
    public static (T Sin, T Cos) SinCos(T x) => (Sin(x), Cos(x));
    public static (T SinPi, T CosPi) SinCosPi(T x) => (SinPi(x), CosPi(x));
    public static Float<T> Pow(Float<T> x, Float<T> y) => (Float<T>)BigRational.Pow((BigRational)x, (BigRational)y, FCPU<T>.Digits);

    public static Float<T> E => FCPU<Float<T>>.e();
    public static Float<T> Pi => FCPU<Float<T>>.pi();
    public static Float<T> Tau => FCPU<Float<T>>.tau();
    public static Float<T> MinValue => FCPU<Float<T>>.min();
    public static Float<T> MaxValue => FCPU<Float<T>>.max();
    public static Float<T> NaN => FCPU<Float<T>>.nan();
    public static Float<T> Epsilon => FCPU<Float<T>>.eps();
    public static Float<T> NegativeInfinity => FCPU<Float<T>>.neginf();
    public static Float<T> NegativeZero => FCPU<Float<T>>.neginf();
    public static Float<T> PositiveInfinity => FCPU<Float<T>>.posinf();
    public static bool IsEvenInteger(Float<T> a) => FCPU<Float<T>>.isevi(&a);
    public static bool IsInteger(Float<T> a) => FCPU<Float<T>>.isint(&a);
    public static bool IsOddInteger(Float<T> a) => FCPU<Float<T>>.isoddint(&a);
    public static bool IsPositive(Float<T> a) => FCPU<Float<T>>.ispos(&a);
    public static bool IsRealNumber(Float<T> a) => FCPU<Float<T>>.isrel(&a);
    public static bool IsFinite(Float<T> a) => FCPU<Float<T>>.isfin(&a);
    public static bool IsInfinity(Float<T> a) => FCPU<Float<T>>.isinfin(&a);
    public static bool IsNaN(Float<T> a) => FCPU<Float<T>>.isnan(&a);
    public static bool IsNegative(Float<T> a) => FCPU<Float<T>>.isneg(&a);
    public static bool IsNegativeInfinity(Float<T> a) => FCPU<Float<T>>.isneginf(&a);
    public static bool IsNormal(Float<T> a) => FCPU<Float<T>>.isnorm(&a);
    public static bool IsPositiveInfinity(Float<T> a) => FCPU<Float<T>>.isposinf(&a);
    public static bool IsSubnormal(Float<T> a) => FCPU<Float<T>>.issubnorm(&a);
    public static bool IsPow2(Float<T> a) => FCPU<Float<T>>.ispow2(&a);
#pragma warning disable CS0169 //never used
    private readonly T p;
#pragma warning restore CS0169
  }
}

