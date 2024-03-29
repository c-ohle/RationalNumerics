﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace NewNumeric
{
  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public unsafe readonly struct BigRat :
#if NET7_0_OR_GREATER
    INumber<BigRat>, ISignedNumber<BigRat>, IPowerFunctions<BigRat>, IRootFunctions<BigRat>,
    IExponentialFunctions<BigRat>, ILogarithmicFunctions<BigRat>, ISpanParsable<BigRat>,
    ITrigonometricFunctions<BigRat>, IHyperbolicFunctions<BigRat>,
#endif
    IEquatable<BigRat>, IComparable<BigRat>, IComparable, IFormattable, ISpanFormattable
  {
    public static implicit operator BigRat(byte value)
    {
      return (uint)value;
    }
    public static implicit operator BigRat(sbyte value)
    {
      return (int)value;
    }
    public static implicit operator BigRat(char value)
    {
      return (uint)value;
    }
    public static implicit operator BigRat(ushort value)
    {
      return (uint)value;
    }
    public static implicit operator BigRat(short value)
    {
      return (int)value;
    }
    public static implicit operator BigRat(int value)
    {
      return new BigRat(value != 0 ? new uint[4] {
        unchecked((uint)value & 0x80000000) | 1, uabs(value), 1, 1 } : null);
    }
    public static implicit operator BigRat(uint value)
    {
      return new BigRat(value != 0 ? new uint[4] { 1, value, 1, 1 } : null);
    }
    public static implicit operator BigRat(long value)
    {
      var t = value >> 63; var p = (BigRat)unchecked((ulong)((value ^ t) - t));
      if (p.p != null) p.p[0] |= unchecked((uint)(value >> 32)) & 0x80000000; return p;
    }
    public static implicit operator BigRat(ulong value)
    {
      return (value >> 32) == 0 ? (uint)value :
        new BigRat(new uint[] { 2, (uint)value, (uint)(value >> 32), 1, 1 });
    }
    public static implicit operator BigRat(nint value)
    {
      return (long)value;
    }
    public static implicit operator BigRat(nuint value)
    {
      return (ulong)value;
    }
    public static implicit operator BigRat(Half value)
    {
      return (double)value;
    }
    public static implicit operator BigRat(float value)
    {
      return (double)value;
    }
    public static implicit operator BigRat(double value)
    {
      if (value == 0) return default; // if (value == (float)value) { }
      var e = unchecked((int)((*(ulong*)&value >> 52) & 0x7FF) - 1022); //Debug.Assert(!double.IsFinite(v) == (e == 0x401));
      if (e == 0x401) throw new OverflowException(nameof(value)); // NaN, +/-Inifinity 
      int p = 14 - ((e * 19728) >> 16), t = p; if (p > 308) p = 308; // v < 1E-294
      var d = Math.Abs(value) * Math.Pow(10, p); //Debug.Assert(double.IsNormal(d));
      t = t - p; if (d < 1e14) { t++; if (e == -1022) t += 14 - (int)Math.Log10(*(ulong*)&value & ~(1ul << 63)); }
      if (t != 0) { d *= Math.Pow(10, t); p += t; }
      var m = (ulong)Math.Round(d); // var a = m / Pow10(p); if (value < 0) a.p![0] |= 0x80000000; return a;
      var u = stackalloc uint[5 + ((e = abs(p)) >> 3)];
      u[0] = (m >> 32) != 0 ? 2u : 1u; *(ulong*)(u + 1) = m;
      var v = u + u[0] + 1; pow10(v, unchecked((uint)e));
      if (p < 0) { u[0] = mul((uint*)&m, u[0], v + 1, v[0], u + 1); *(ulong*)(u + u[0] + 1) = 0x100000001; }
      var n = u[0]; u[0] |= ((uint*)&value)[1] & 0x80000000;
      return new BigRat(u, 2 + n + u[n + 1]);
    }
    public static implicit operator BigRat(decimal value)
    {
      var b = (uint*)&value; uint f = b[0], e = (f >> 16) & 0xff; //0..28 
      uint* p = stackalloc uint[9];
      *(ulong*)&p[1] = *(ulong*)&b[2]; p[3] = b[1]; p[0] = p[3] != 0 ? 3u : p[2] != 0 ? 2u : 1;
      if (*(ulong*)p == 1) { *(ulong*)(p + 2) = 0x100000001; }
      else
      {
        var s = p + (p[0] + 1);
        if (e == 0) { *(ulong*)s = 0x100000001; }
        else
        {
          var t = (ulong*)(s + 1); var h = e < 19 ? e : 19;
          t[0] = (ulong)Math.Pow(10, h);
          t[1] = h == e ? 0 : Math.BigMul(t[0], (ulong)Math.Pow(10, e - h), out t[0]);
          s[0] = s[3] != 0 ? 3u : s[2] != 0 ? 2u : 1;
        }
        p[0] |= f & 0x80000000;
      }
      return new BigRat(p, 2 + (f = p[0] & 0x3fffffff) + p[f + 1]);
    }
    public static implicit operator BigRat(BigInteger value)
    {
      var si = value.Sign; if (si == 0) return default;
      if (si < 0) value = -value; int n = value.GetByteCount(true), c = ((n - 1) >> 2) + 1;
      uint* p = stackalloc uint[c + 3];
      p[c] = 0; var ok = value.TryWriteBytes(new Span<byte>(p + 1, n), out _, true);
      if (p[c] == 0) c--; Debug.Assert(ok && p[c] != 0); //todo: report BigInteger bug p[c] == 0
      p[0] = unchecked((uint)c) | ((uint)si & 0x80000000); *(ulong*)(p + c + 1) = 0x100000001;
      return new BigRat(p, unchecked((uint)c) + 3);
    }

    public static explicit operator byte(BigRat value)
    {
      return (byte)(double)value;
    }
    public static explicit operator sbyte(BigRat value)
    {
      return (sbyte)(double)value;
    }
    public static explicit operator char(BigRat value)
    {
      return (char)(double)value;
    }
    public static explicit operator short(BigRat value)
    {
      return (short)(double)value;
    }
    public static explicit operator ushort(BigRat value)
    {
      return (ushort)(double)value;
    }
    public static explicit operator int(BigRat value)
    {
      return (int)(double)value; //todo opt.
    }
    public static explicit operator uint(BigRat value)
    {
      return (uint)(double)value;
    }
    public static explicit operator long(BigRat value)
    {
      long t; toi(value, (uint*)&t, 2); return t;
    }
    public static explicit operator ulong(BigRat value)
    {
      ulong t; toi(value, (uint*)&t, 2 | 0x10000); return t;
    }
    public static explicit operator nint(BigRat value)
    {
      return (nint)(long)value;
    }
    public static explicit operator nuint(BigRat value)
    {
      return (nuint)(ulong)value;
    }
    public static explicit operator Half(BigRat value)
    {
      return (Half)(double)value;
    }
    public static explicit operator float(BigRat value)
    {
      return (float)(double)value;
    }
    public static explicit operator double(BigRat value)
    {
      if (value.p == null) return 0;
      fixed (uint* a = value.p)
      {
        var na = a[0] & 0x3fffffff; var b = a + na + 1; var nb = b[0];
        var ca = BitOperations.LeadingZeroCount(a[na]);
        var cb = BitOperations.LeadingZeroCount(b[nb]);
        var va = ((ulong)a[na] << 32 + ca) | (na < 2 ? 0 : ((ulong)a[na - 1] << ca) | (na < 3 ? 0 : (ulong)a[na - 2] >> 32 - ca));
        var vb = ((ulong)b[nb] << 32 + cb) | (nb < 2 ? 0 : ((ulong)b[nb - 1] << cb) | (nb < 3 ? 0 : (ulong)b[nb - 2] >> 32 - cb));
        var e = ((na << 5) - ca) - ((nb << 5) - cb); Debug.Assert(vb != 0);
        var r = (double)(va >> 11) / (vb >> 11);
        if (e < -1021) { if (e >= -1074) *(ulong*)&r = (ulong)(r * (1L << ((int)e + 1074))) + (((*(ulong*)&r >> 52) & 1) ^ 1); else r = 0; }
        else if (e > +1023) r = double.PositiveInfinity;
        else { var p = (0x3ff + e) << 52; r *= *(double*)&p; }
        ((uint*)&r)[1] |= a[0] & 0x80000000; return r;
      }
    }
    public static explicit operator decimal(BigRat value)
    {
      return tom(value, false);
    }
    public static explicit operator BigInteger(BigRat value)
    {
      if (value.p == null) return default;
      var s = new ReadOnlySpan<uint>(Truncate(value).p);
      var a = MemoryMarshal.AsBytes(s.Slice(1, unchecked((int)(s[0] & 0x3fffffff))));
      var r = new BigInteger(a, true, false); if (value < 0) r = -r; return r;
    }

    public static explicit operator checked byte(BigRat value)
    {
      return checked((byte)(double)value);
    }
    public static explicit operator checked sbyte(BigRat value)
    {
      return checked((sbyte)(double)value);
    }
    public static explicit operator checked char(BigRat value)
    {
      return checked((char)(double)value);
    }
    public static explicit operator checked short(BigRat value)
    {
      return checked((short)(double)value);
    }
    public static explicit operator checked ushort(BigRat value)
    {
      return checked((ushort)(double)value);
    }
    public static explicit operator checked int(BigRat value)
    {
      return checked((int)(double)value);
    }
    public static explicit operator checked uint(BigRat value)
    {
      return checked((uint)(double)value);
    }
    public static explicit operator checked long(BigRat value)
    {
      long t; toi(value, (uint*)&t, 2 | 0x80000); return t;
    }
    public static explicit operator checked ulong(BigRat value)
    {
      ulong t; toi(value, (uint*)&t, 2 | 0x10000 | 0x80000); return t;
    }
    public static explicit operator checked nint(BigRat value)
    {
      nint t; toi(value, (uint*)&t, ((uint)sizeof(nint) >> 2) | 0x80000); return t;
    }
    public static explicit operator checked nuint(BigRat value)
    {
      nuint t; toi(value, (uint*)&t, ((uint)sizeof(nuint) >> 2) | 0x10000 | 0x80000); return t;
    }

    public static BigRat operator +(BigRat a)
    {
      return a;
    }
    public static BigRat operator -(BigRat a)
    {
      if (a.p == null) return default;
      fixed (uint* p = a.p) a = new BigRat(p, unchecked((uint)a.p.Length));
      a.p![0] ^= 0x80000000; return a;
    }
    public static BigRat operator +(BigRat a, BigRat b)
    {
      if (a.p == null) return b;
      if (b.p == null) return a;
      var w = stackalloc uint[(a.p.Length + b.p.Length) * 3];
      fixed (uint* u = a.p, v = b.p) return add(u, v, w, 0);
    }
    public static BigRat operator -(BigRat a, BigRat b)
    {
      if (a.p == null) return -b;
      if (b.p == null) return a;
      var w = stackalloc uint[(a.p.Length + b.p.Length) * 3];
      fixed (uint* u = a.p, v = b.p) return add(u, v, w, 0x80000000);
    }
    public static BigRat operator *(BigRat a, BigRat b)
    {
      if (a.p == null || b.p == null) return default;
      uint* w = stackalloc uint[a.p.Length + b.p.Length];
      fixed (uint* u = a.p, v = b.p) return mul(u, v, w);
    }
    public static BigRat operator /(BigRat a, BigRat b)
    {
      if (b.p == null) throw new DivideByZeroException();
      if (a.p == null) return default;
      uint* w = stackalloc uint[a.p.Length + b.p.Length];
      fixed (uint* u = a.p, v = b.p) return div(u, v, w);
    }
    public static BigRat operator %(BigRat a, BigRat b)
    {
      return a - Truncate(a / b) * b;
    }
    public static BigRat operator +(BigRat a, int b)
    {
      if (a.p == null) return b;
      if (b == 0) return a;
      var v = stackalloc uint[4 + (a.p.Length + 4) * 3]; copy(v, b);
      fixed (uint* u = a.p) return add(u, v, v + 4, 0);
    }
    public static BigRat operator -(BigRat a, int b)
    {
      return a + -b;
    }
    public static BigRat operator *(BigRat a, int b)
    {
      if (a.p == null || b == 0) return default;
      var v = stackalloc uint[a.p.Length + 8]; copy(v, b);
      fixed (uint* u = a.p) return mul(u, v, v + 4);
    }
    public static BigRat operator /(BigRat a, int b)
    {
      if (b == 0) throw new DivideByZeroException();
      if (a.p == null) return default;
      var v = stackalloc uint[a.p.Length + 8]; copy(v, b);
      fixed (uint* u = a.p) return div(u, v, v + 4);
    }
    public static BigRat operator %(BigRat a, int b)
    {
      return a - Truncate(a / b) * b;
    }
    public static BigRat operator +(int a, BigRat b)
    {
      if (a == 0) return b;
      if (b.p == null) return a;
      var u = stackalloc uint[4 + (4 + b.p.Length) * 3]; copy(u, a);
      fixed (uint* v = b.p) return add(u, v, u + 4, 0);
    }
    public static BigRat operator -(int a, BigRat b)
    {
      if (a == 0) return -b;
      if (b.p == null) return a;
      var u = stackalloc uint[4 + (4 + b.p.Length) * 3]; copy(u, a);
      fixed (uint* v = b.p) return add(u, v, u + 4, 0x80000000);
    }
    public static BigRat operator *(int a, BigRat b)
    {
      if (a == 0 || b.p == null) return default;
      var u = stackalloc uint[8 + b.p.Length]; copy(u, a);
      fixed (uint* v = b.p) return mul(u, v, u + 4);
    }
    public static BigRat operator /(int a, BigRat b)
    {
      if (b.p == null) throw new DivideByZeroException();
      if (a == 0) return default;
      var u = stackalloc uint[8 + b.p.Length];
      fixed (uint* v = b.p)
      {
        if (a == 1) // fast inv 1 / x
        {
          var n = unchecked((uint)b.p.Length);
          copy(u, v, n); inv(u); return new BigRat(u, n);
        }
        copy(u, a); return div(u, v, u + 4);
      }
    }
    public static BigRat operator ++(BigRat a)
    {
      return a + 1;
    }
    public static BigRat operator --(BigRat a)
    {
      return a - 1;
    }

    public static bool operator ==(BigRat a, BigRat b)
    {
      return a.CompareTo(b) == 0;
    }
    public static bool operator !=(BigRat a, BigRat b)
    {
      return a.CompareTo(b) != 0;
    }
    public static bool operator <=(BigRat a, BigRat b)
    {
      return a.CompareTo(b) <= 0;
    }
    public static bool operator >=(BigRat a, BigRat b)
    {
      return a.CompareTo(b) >= 0;
    }
    public static bool operator <(BigRat a, BigRat b)
    {
      return a.CompareTo(b) < 0;
    }
    public static bool operator >(BigRat a, BigRat b)
    {
      return a.CompareTo(b) > 0;
    }
    public static bool operator ==(BigRat a, int b)
    {
      return a.CompareTo(b) == 0;
    }
    public static bool operator !=(BigRat a, int b)
    {
      return a.CompareTo(b) != 0;
    }
    public static bool operator <=(BigRat a, int b)
    {
      return a.CompareTo(b) <= 0;
    }
    public static bool operator >=(BigRat a, int b)
    {
      return a.CompareTo(b) >= 0;
    }
    public static bool operator <(BigRat a, int b)
    {
      return a.CompareTo(b) < 0;
    }
    public static bool operator >(BigRat a, int b)
    {
      return a.CompareTo(b) > 0;
    }
    public static bool operator ==(int a, BigRat b)
    {
      return b.CompareTo(a) == 0;
    }
    public static bool operator !=(int a, BigRat b)
    {
      return b.CompareTo(a) != 0;
    }
    public static bool operator <=(int a, BigRat b)
    {
      return b.CompareTo(a) >= 0;
    }
    public static bool operator >=(int a, BigRat b)
    {
      return b.CompareTo(a) <= 0;
    }
    public static bool operator <(int a, BigRat b)
    {
      return b.CompareTo(a) > 0;
    }
    public static bool operator >(int a, BigRat b)
    {
      return b.CompareTo(a) < 0;
    }

    public static int Sign(BigRat value)
    {
      return value.p == null ? 0 : (value.p[0] & 0x80000000) != 0 ? -1 : +1;
    }
    public static int ILogB(BigRat value)
    {
      if (value.p == null) return -int.MaxValue; // int.MinValue; //-2147483648; //infinity
      fixed (uint* p = value.p)
      {
        var h = p[0]; var a = h & 0x3fffffff; var q = p + a + 1; var b = q[0];
        var u = (a << 5) - unchecked((uint)BitOperations.LeadingZeroCount(p[a]));
        var v = (b << 5) - unchecked((uint)BitOperations.LeadingZeroCount(q[b]));
        return unchecked((int)u - (int)v);
      }
    }
    public static int ILog10(BigRat value)
    {
      if (value.p == null) return 0;
      fixed (uint* p = value.p)
      {
        var np = p[0] & 0x3fffffff; var nq = p[np + 1]; var q = p + np + 1; // est log(2) / log(10)
        var mp = ((((long)np << 5) - BitOperations.LeadingZeroCount(p[np])) * 1292913986) >> 32;
        var mq = ((((long)nq << 5) - BitOperations.LeadingZeroCount(q[nq])) * 1292913986) >> 32;
        var ex = unchecked((int)(mp - mq)); var ne = 5 + (abs(ex) >> 3);
        uint* u = stackalloc uint[(ne + value.p.Length) << 1], w = u + ne, z;
        for (; ; )
        {
          pow10(u, uabs(ex));
          if (ex < 0) { w[0] = mul(p + 1, np, u + 1, u[0], w + 1); mod(w, q, u); }
          else { w[0] = mul(q + 1, nq, u + 1, u[0], w + 1); copy(z = w + w[0] + 1, p, np + 1); z[0] &= 0x3fffffff; mod(z, w, u); }
          Debug.Assert(u[0] == 1);
          if (u[1] == 00) { ex--; continue; }
          if (u[1] >= 10) { ex++; continue; }
          return ex;
        }
      }
    }
    public static BigRat Abs(BigRat a)
    {
      return Sign(a) >= 0 ? a : -a;
    }
    public static BigRat Min(BigRat x, BigRat y)
    {
      return x.CompareTo(y) <= 0 ? x : y;
    }
    public static BigRat Max(BigRat x, BigRat y)
    {
      return x.CompareTo(y) >= 0 ? x : y;
    }
    public static BigRat MaxMagnitude(BigRat x, BigRat y)
    {
      return Abs(x).CompareTo(Abs(y)) >= 0 ? x : y; //todo: acmp
    }
    public static BigRat MinMagnitude(BigRat x, BigRat y)
    {
      return Abs(x).CompareTo(Abs(y)) <= 0 ? x : y; //todo: acmp
    }
    public static BigRat MaxMagnitudeNumber(BigRat x, BigRat y)
    {
      return MaxMagnitude(x, y); // IEEE 754:2019 does not propagate NaN inputs back 
    }
    public static BigRat MinMagnitudeNumber(BigRat x, BigRat y)
    {
      return MinMagnitude(x, y); // IEEE 754:2019 does not propagate NaN inputs back 
    }
    public static BigRat CopySign(BigRat value, BigRat sign)
    {
      int a, b; return (a = Sign(value)) != 0 && (b = Sign(sign) < 0 ? -1 : +1) != 0 && a != b ? -value : value;
    }
    public static BigRat Clamp(BigRat value, BigRat min, BigRat max)
    {
      if (min > max) throw new ArgumentException($"{nameof(min)} {nameof(max)}");
      return value < min ? min : value > max ? max : value;
    }
    public static BigRat Truncate(BigRat value)
    {
      return mod(value, 0);
    }
    public static BigRat Floor(BigRat value)
    {
      return mod(value, 1);
    }
    public static BigRat Ceiling(BigRat value)
    {
      return mod(value, 2);
    }
    public static BigRat Round(BigRat value)
    {
      return mod(value, 3);
    }
    public static BigRat Round(BigRat value, int digits)
    {
      //todo: inline see BigRat(double value)
      // if (value.p == null) return value; if(digits == 0) return Truncate(value);
      var e = Pow10(digits); value = Round(value * e) / e; return value;
    }
    public static BigRat Round(BigRat value, MidpointRounding mode)
    {
      return Round(value, 0, mode);
    }
    public static BigRat Round(BigRat value, int digits, MidpointRounding mode)
    {
      var p = Pow10(digits); value *= p;
      switch (mode)
      {
        case MidpointRounding.ToEven: value = Round(value); break;
        case MidpointRounding.ToZero: value = Truncate(value); break;
        case MidpointRounding.ToNegativeInfinity: value = Floor(value); break;
        case MidpointRounding.ToPositiveInfinity: value = Ceiling(value); break;
        case MidpointRounding.AwayFromZero:
          var u = Truncate(value); var v = value - u;
          if (Abs(v) >= new BigRat(1, 2)) u += Sign(v); value = u; // >= 0.5
          break;
      }
      value /= p; return value;
    }
    public static BigRat RoundB(BigRat value, int bits)
    {
      if (value.p == null || bits <= 0) return default;
      fixed (uint* p = value.p)
      {
        var h = p[0]; var a = h & 0x3fffffff; var q = p + a + 1; var b = q[0];
        var u = (a << 5) - unchecked((uint)BitOperations.LeadingZeroCount(p[a]));
        var v = (b << 5) - unchecked((uint)BitOperations.LeadingZeroCount(q[b]));
        if (u > v) u = v; if (u <= bits) return value;
        var w = stackalloc uint[value.p.Length]; copy(w, p, unchecked((uint)value.p.Length));
        var t = (int)(u - bits); shr(w, t); shr(q = w + a + 1, t);
        if (w[0] != a) copy(w + w[0] + 1, q, q[0] + 1);
        var l = 2 + w[0] + w[w[0] + 1]; w[0] |= h & 0x80000000;
        return new BigRat(w, l);
      }
    }
    public static BigRat Pow(BigRat x, int y)
    {
      var r = (BigRat)1u;
      for (var e = uabs(y); ; e >>= 1) { if ((e & 1) != 0) r *= x; if (e <= 1) break; x *= x; }
      if (y < 0) fixed (uint* p = r.p) inv(p); // if (y < 0) r = 1 / r;
      return r;
    }
    public static BigRat Pow(BigRat x, BigRat y, int digits)
    {
      var p = base2(abs(digits)); x = exp(y * log(x, p), p);
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Pow(BigRat x, BigRat y)
    {
      return Pow(x, y, 032);
    }
    public static BigRat Pow2(int y)
    {
      var z = abs(y); uint a = (unchecked((uint)z) >> 5) + 1, b = 1u << (z & 31);
      var p = new uint[a + 3]; if (y >= 0) { p[0] = a; p[a] = b; p[a + 1] = p[a + 2] = 1; }
      else { p[2] = a; p[a + 2] = b; p[0] = p[1] = 1; }
      return new BigRat(p);
    }
    public static BigRat Pow10(int y)
    {
      uint e = uabs(y); uint* u = stackalloc uint[5 + unchecked((int)(e >> 3))], v;
      pow10(v = y >= 0 ? u : u + 2, e); *(ulong*)(u + (y >= 0 ? u[0] + 1 : 0)) = 0x100000001;
      return new BigRat(u, 3 + v[0]);
    }
    public static BigRat Sqrt(BigRat x)
    {
      return Sqrt(x, 032);
    }
    public static BigRat Sqrt(BigRat x, int digits)
    {
      if (x <= 0) { if (x == 0) return default; throw new ArgumentException(nameof(x)); }
      x = sqrt(x, base2(abs(digits))); return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Cbrt(BigRat x)
    {
      return Cbrt(x, 032);
    }
    public static BigRat Cbrt(BigRat x, int digits)
    {
      return Pow(x, new BigRat(1, 3), digits);
    }
    public static BigRat RootN(BigRat x, int n)
    {
      return RootN(x, n, 032);
    }
    public static BigRat RootN(BigRat x, int n, int digits)
    {
      return Pow(x, new BigRat(1, n), digits);
    }
    public static BigRat Hypot(BigRat x, BigRat y)
    {
      return Hypot(x, y, 032);
    }
    public static BigRat Hypot(BigRat x, BigRat y, int digits)
    {
      return Sqrt(x * x + y * y, digits);
    }
    public static BigRat Exp(BigRat x)
    {
      return Exp(x, 032);
    }
    public static BigRat Exp(BigRat x, int digits)
    {
      x = exp(x, base2(abs(digits)));
      return digits >= 0 ? Round(x, digits - ILog10(x)) : x;
    }
    public static BigRat Exp10(BigRat x)
    {
      return Exp10(x, 032);
    }
    public static BigRat Exp10(BigRat x, int digits)
    {
      return Pow(10, x, digits); //todo: test
    }
    public static BigRat Exp2(BigRat x)
    {
      return Exp2(x, 032);
    }
    public static BigRat Exp2(BigRat x, int digits)
    {
      return Pow(2, x, digits); //todo: test
    }
    public static BigRat Log(BigRat x)
    {
      return Log(x, 032);
    }
    public static BigRat Log(BigRat x, int digits)
    {
      if (x <= 0) throw new ArgumentException(nameof(x));
      x = log(x, base2(abs(digits))); return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Log(BigRat x, BigRat newBase)
    {
      return Log(x, newBase, 032);
    }
    public static BigRat Log(BigRat x, BigRat newBase, int digits)
    {
      if (newBase < 0 || newBase == 1 || newBase == 0 && x != 1) throw new ArgumentException(nameof(newBase));
      if (x < 0 || x == 0 && (newBase > 1 || newBase > 0 && newBase < 1)) throw new ArgumentException(nameof(x));
      if (newBase == 0 && x == 1) return default;
      var p = base2(abs(digits)); x = log(x, p) / log(newBase, p);
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Log2(BigRat x)
    {
      return Log2(x, 032);
    }
    public static BigRat Log2(BigRat x, int digits)
    {
      if (x <= 0) throw new ArgumentException(nameof(x));
      x = log2(x, base2(abs(digits)));
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Log10(BigRat x)
    {
      return Log10(x, 032);
    }
    public static BigRat Log10(BigRat x, int digits)
    {
      if (x <= 0) throw new ArgumentException(nameof(x));
      var b = base2(abs(digits)); x = log2(x, b) / log2(10, b); //todo: test 
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Pi()
    {
      return Pi(032);
    }
    public static BigRat Pi(int digits)
    {
      var x = pi(base2(abs(digits)));
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Tau()
    {
      return Tau(032);
    }
    public static BigRat Tau(int digits)
    {
      var x = pi(base2(abs(digits))) * 2;
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat E()
    {
      return E(32);
    }
    public static BigRat E(int digits)
    {
      var x = exp(1, base2(abs(digits)));
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Sin(BigRat x)
    {
      return Sin(x, 032);
    }
    public static BigRat Sin(BigRat x, int digits)
    {
      x = sin(x, base2(abs(digits)), false);
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Cos(BigRat x)
    {
      return Cos(x, 032);
    }
    public static BigRat Cos(BigRat x, int digits)
    {
      x = sin(x, base2(abs(digits)), true);
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Tan(BigRat x)
    {
      return Tan(x, 032);
    }
    public static BigRat Tan(BigRat x, int digits)
    {
      var d = -abs(digits); x = Sin(x, d) / Cos(x, d); //todo: test alg 
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Asin(BigRat x)
    {
      return Asin(x, 032);
    }
    public static BigRat Asin(BigRat x, int digits)
    {
      var t = base2(abs(digits)); var a = 1 - x * x;
      if (a > 0) x = atan(x / sqrt(a, t), t);
      else if (a == 0) { var b = pi(t) / 2; x = x < 0 ? -b : b; }
      else throw new ArgumentException(nameof(x));
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Acos(BigRat x)
    {
      return Acos(x, 032);
    }
    public static BigRat Acos(BigRat x, int digits)
    {
      var t = base2(abs(digits)); var a = 1 - x; var b = 1 + x;
      if (a < 0 || b < 0) throw new ArgumentException(nameof(x));
      if (b > 0) x = 2 * atan(sqrt(a / b, t), t); else x = pi(t);
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Atan(BigRat x)
    {
      return Atan(x, 032);
    }
    public static BigRat Atan(BigRat x, int digits)
    {
      x = atan(x, base2(abs(digits)));
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Atan2(BigRat y, BigRat x)
    {
      return Atan2(y, x, 032);
    }
    public static BigRat Atan2(BigRat y, BigRat x, int digits)
    {
      x = atan2(y, x, base2(abs(digits)));
      return digits >= 0 ? Round(x, digits) : x;
      static BigRat atan2(BigRat y, BigRat x, int bits) //todo: inline
      {
        if (x > 0) return 2 * atan(y / (sqrt(x * x + y * y, bits) + x), bits);
        else if (y != 0) return 2 * atan((sqrt(x * x + y * y, bits) - x) / y, bits);
        else if (x < 0 && y == 0) return pi(bits);
        else throw new ArgumentException(); //x == y == 0
      }
    }
    public static BigRat Acosh(BigRat x)
    {
      return Acosh(x, 032);
    }
    public static BigRat Acosh(BigRat x, int digits)
    {
      var d = -abs(digits); x = Log(x + Sqrt(x * x - 1, d), d); //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Asinh(BigRat x)
    {
      return Asinh(x, 032);
    }
    public static BigRat Asinh(BigRat x, int digits)
    {
      var d = -abs(digits); x = Log(x + Sqrt(x * x + 1, d), d); //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Atanh(BigRat x)
    {
      return Atanh(x, 032);
    }
    public static BigRat Atanh(BigRat x, int digits)
    {
      var d = -abs(digits); x = Log((1 + x) / (1 - x), d) / 2; //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Cosh(BigRat x)
    {
      return Cosh(x, 032);
    }
    public static BigRat Cosh(BigRat x, int digits)
    {
      var d = -abs(digits); x = (Exp(x, d) + Exp(-x, d)) / 2; //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Sinh(BigRat x, int digits)
    {
      var d = -abs(digits); x = (Exp(x, d) - Exp(-x, d)) / 2; //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat Sinh(BigRat x)
    {
      return Sinh(x, 032);
    }
    public static BigRat Tanh(BigRat x)
    {
      return Tanh(x, 032);
    }
    public static BigRat Tanh(BigRat x, int digits)
    {
      var d = -abs(digits); x = 1 - 2 / (Exp(x * 2, d) + 1); //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat AcosPi(BigRat x)
    {
      return AcosPi(x, 032);
    }
    public static BigRat AcosPi(BigRat x, int digits)
    {
      var d = -abs(digits); x = Acos(x, d) / Pi(d); //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat AsinPi(BigRat x)
    {
      return AsinPi(x, 032);
    }
    public static BigRat AsinPi(BigRat x, int digits)
    {
      var d = -abs(digits); x = Asin(x, d) / Pi(d); //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat AtanPi(BigRat x)
    {
      return AtanPi(x, 032);
    }
    public static BigRat AtanPi(BigRat x, int digits)
    {
      var d = -abs(digits); x = Atan(x, d) / Pi(d); //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat CosPi(BigRat x)
    {
      return CosPi(x, 032);
    }
    public static BigRat CosPi(BigRat x, int digits)
    {
      var d = -abs(digits); x = Cos(x * Pi(d), d); //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat SinPi(BigRat x)
    {
      return SinPi(x, 032);
    }
    public static BigRat SinPi(BigRat x, int digits)
    {
      var d = -abs(digits); x = Sin(x * Pi(d), d); //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static BigRat TanPi(BigRat x)
    {
      return TanPi(x, 032);
    }
    public static BigRat TanPi(BigRat x, int digits)
    {
      var d = -abs(digits); x = Tan(x * Pi(d), d); //todo: test
      return digits >= 0 ? Round(x, digits) : x;
    }
    public static (BigRat Sin, BigRat Cos) SinCos(BigRat x)
    {
      return SinCos(x, 032);
    }
    public static (BigRat Sin, BigRat Cos) SinCos(BigRat x, int digits)
    {
      return (Sin(x, digits), Cos(x, digits));
    }
    public static (BigRat SinPi, BigRat CosPi) SinCosPi(BigRat x)
    {
      return SinCosPi(x, 032);
    }
    public static (BigRat SinPi, BigRat CosPi) SinCosPi(BigRat x, int digits)
    {
      return (SinPi(x, digits), CosPi(x, digits));
    }

    public static bool IsZero(BigRat value)
    {
      return value.p == null;
    }
    public static bool IsNegative(BigRat value)
    {
      return value.p != null && (value.p[0] & 0x80000000) != 0;
    }
    public static bool IsPositive(BigRat value)
    {
      return value.p == null || (value.p[0] & 0x80000000) == 0;
    }
    public static bool IsInteger(BigRat value)
    {
      // return Truncate(value) == value;
      return mod(value, 4).p != null;
    }
    public static bool IsEvenInteger(BigRat value)
    {
      //return value.p == null || IsInteger(value) && (Normalize(value).p![1] & 1) == 0;
      if (value.p == null) return true;
      var a = mod(value, 5); return a.p != null ? (a.p[1] & 1) == 0 : false;
    }
    public static bool IsOddInteger(BigRat value)
    {
      //return value.p != null && IsInteger(value) && (Normalize(value).p![1] & 1) == 1;
      var a = mod(value, 5); return a.p != null ? (a.p[1] & 1) != 0 : false;
    }
    public static bool IsNormalized(BigRat value)
    {
      return value.Normalize().p == value.p;
    }

    public BigRat Normalize()
    {
      if (this.p == null) return this;
      fixed (uint* p = this.p)
      {
        var d = p + (p[0] & 0x3fffffff) + 1; if (*(ulong*)d == 0x100000001) return this;
        var l = this.p.Length; var s = stackalloc uint[l * 3]; uint x;
        copy(s, p, unchecked((uint)l)); s[0] &= 0x3fffffff;
        var e = gcd(s, s + s[0] + 1); if (*(ulong*)e == 0x100000001) return this;
        var t = s + l; copy(t, p, unchecked((uint)l)); t[0] &= 0x3fffffff;
        var r = t + l; d = t + t[0] + 1; mod(t, e, r); mod(d, e, r + r[0] + 1);
        x = r[0]; r[0] |= p[0] & 0x80000000;
        return new BigRat(r, 2 + x + r[x + 1]);
      }
    }
    public BigRat Numerator()
    {
      if (this.p == null) return default;
      var s = new ReadOnlySpan<uint>(this.p);
      var n = (int)(s[0] & 0x3fffffff); var a = new uint[n + 3];
      s.Slice(0, n + 1).CopyTo(a); a[n + 1] = a[n + 2] = 1;
      return new BigRat(a);
    }
    public BigRat Denominator()
    {
      if (this.p == null) return 1u;
      var s = new ReadOnlySpan<uint>(this.p);
      int n = (int)(s[0] & 0x3fffffff), m = (int)s[n + 1]; var a = new uint[m + 3];
      new ReadOnlySpan<uint>(this.p).Slice(n + 1, m + 1).CopyTo(a); a[m + 1] = a[m + 2] = 1;
      return new BigRat(a);
    }

    public override int GetHashCode()
    {
      if (p == null) return 0;
      fixed (uint* p = this.p)
      {
        uint h = 0, n = unchecked((uint)this.p.Length); // n = p[(n = p[0] & 0x3fffffff) + 1] + n + 2;
        for (uint i = 0; i < n; i++) h = ((h << 7) | (h >> 25)) ^ p[i];
        return unchecked((int)h);
      }
    }
    public override bool Equals(object? value)
    {
      return value is BigRat a ? Equals(a) : false;
    }
    public bool Equals(BigRat value)
    {
      if (((ReadOnlySpan<uint>)this.p).SequenceEqual(((ReadOnlySpan<uint>)value.p))) return true; //todo: perf test
      return CompareTo(value) == 0;
    }
    public int CompareTo(BigRat value)
    {
      fixed (uint* u = this.p, v = value.p) return cmp(u, v);
    }
    public int CompareTo(int value)
    {
      if (value == 0) return Sign(this);
      var v = stackalloc uint[4]; copy(v, value);
      fixed (uint* u = this.p) return cmp(u, v);
    }
    public int CompareTo(object? value)
    {
      if (value == null) return 1;
      if (value is BigRat d) return CompareTo(d);
      throw new ArgumentException(nameof(value));
    }

    public bool TryFormat(Span<char> s, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
      var a = this; char f = 'Q', g = f; int d = 32; var info = NumberFormatInfo.GetInstance(provider);
      if (format.Length != 0)
      {
        f = (char)((g = format[0]) & ~0x20); var l = format.Length; if (l > 10) throw new FormatException(nameof(format));
        if (l > 1) for (int j = 1, y = d = 0; j < l; d = d * 10 + (y = format[j++] - '0')) if ((uint)y > 9) throw new FormatException(nameof(format));
        switch (f)
        {
          case 'Q': if (d == 0 && a.p != null) goto fmq; break;
          case 'E': if (l == 1) d = 6; break;
          case 'G': if (d == 0) d = 32; break;
          case 'F': case 'N': if (l == 1) d = info.NumberDecimalDigits; g = f; f = 'F'; break;
          case 'R': f = 'Q'; if (d == 0 && a.p != null) goto fmq; break;
          //todo: case 'P': case 'C':  
          default: throw new FormatException(nameof(format));
        }
      }
      int e = ILog10(a), x = e + 1, t = 16 + d + (f == 'F' && e > 0 ? e * 3 / 2 : 0);
      if (t > s.Length) goto err;
      if (f != 'Q') a = Round(a, f == 'G' ? d - (e + 1) : f == 'E' ? d - e : d);
      if (a.p == null) { if (f != 'E' && f != 'F') { s[0] = '0'; charsWritten = 1; return true; } }
      else if (e != 0) a = a / BigRat.Pow10(e);
      int n = tos(s.Slice(0, f == 'F' ? Math.Max(d + e, 0) + 2 : d + 1), a, f == 'Q', out var r);
      if (n == d + 1) { if (f != 'E' && f != 'F') n--; if (f == 'Q') { if ((g & ~0x20) == 'R') goto fmq; s[n++] = '…'; } }
      else if (f == 'Q' && abs(x) < d) { /*e = 0;*/ } //todo: check
      if (r != -1) { s.Slice(r, n - r).CopyTo(s.Slice(r + 1)); s[r] = '\''; n++; if (r == 0) { s.Slice(0, n).CopyTo(s.Slice(1)); s[0] = '0'; n++; e++; x++; } }
      if (f == 'F') { e = 0; if (a.p == null) x = 1; }
      if ((e <= -5 || e >= 17) || (f == 'G' && x > d) || f == 'E' || (r != -1 && e >= r)) x = 1; else e = 0;
      if (x <= 0) { t = 1 - x; s.Slice(0, n).CopyTo(s.Slice(t)); s.Slice(0, t).Fill('0'); n += t; x += t; }
      if (x > n) { s.Slice(n, x - n).Fill('0'); n = x; }
      if (f == 'E' && (t = d - n + 1) > 0) { s.Slice(n, t).Fill('0'); n += t; }
      if (f == 'F' && (t = d - (n - x)) > 0) { s.Slice(n, t).Fill('0'); n += t; }
      if (x > 0 && x < n) { s.Slice(x, n++ - x).CopyTo(s.Slice(x + 1)); s[x] = info.NumberDecimalSeparator[0]; }
      if (g == 'N')
      {
        var l = info.NumberGroupSizes[0]; var c = info.NumberGroupSeparator[0];
        for (t = (x < n ? x : n); (t -= l) > 0;) { s.Slice(t, n - t).CopyTo(s.Slice(t + 1)); s[t] = c; n++; }
      }
      if (this < 0) { s.Slice(0, n).CopyTo(s.Slice(1)); s[0] = '-'; n++; }
      if (e != 0 || f == 'E')
      {
        s[n++] = (char)('E' | (g & 0x20)); s[n++] = e >= 0 ? '+' : '-';
        n += tos(s.Slice(n), unchecked((uint)(((e ^ (e >>= 31)) - e))), f == 'E' ? 3 : 2);
      }
      charsWritten = n; return true; fmq:
      a = this.Numerator(); var b = this.Denominator(); //todo: "123/1E+100", "123/1" -> "123" ...
      e = ILog10(a); x = ILog10(b); if ((t = e + x + 4) > s.Length) goto err;
      n = 0; if (this < 0) s[n++] = '-';
      r = tos(s.Slice(n), a / Pow10(e), false, out _); s.Slice(n + r, e + 1 - r).Fill('0'); n += e + 1; if (b == 1) goto ifm; s[n++] = '/';
      r = tos(s.Slice(n), b / Pow10(x), false, out _); s.Slice(n + r, x + 1 - r).Fill('0'); n += x + 1; ifm:
      charsWritten = n; return true; err:
      new Span<char>(&t, 2).TryCopyTo(s); charsWritten = 0; return false;
    }
    public string ToString(string? format, IFormatProvider? provider)
    {
      if (format != default && format.Length == 0) provider = NumberFormatInfo.InvariantInfo; // "" : dbg
      Span<char> s = stackalloc char[1024];
      if (TryFormat(s, out var charsWritten, format, provider)) return s.Slice(0, charsWritten).ToString();
      int need; fixed (char* p = s) need = *(int*)p; char* pa = null;
      try
      {
        char* p; if (need < (250000 >> 1)) { var t = stackalloc char[need]; p = t; }
        else p = pa = (char*)alloc(need << 1);
        var ok = TryFormat(s = new Span<char>(p, need), out charsWritten, format, provider); Debug.Assert(ok);
        return s.Slice(0, charsWritten).ToString();
      }
      finally { free(pa); }
    }
    public string ToString(string? format)
    {
      return ToString(format, default(IFormatProvider));
    }
    public string ToString(IFormatProvider? provider)
    {
      return ToString(default(string), provider);
    }
    public override string ToString()
    {
      return ToString(default(string), default(IFormatProvider));
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out BigRat result)
    {
      if ((style & NumberStyles.AllowHexSpecifier) != 0) goto err;
      if ((style & NumberStyles.AllowLeadingWhite) != 0) s = s.TrimStart(" \u0009\u000A\u000B\u000C\u000D");
      if ((style & NumberStyles.AllowTrailingWhite) != 0) s = s.TrimEnd(" \u0009\u000A\u000B\u000C\u000D");
      if (s.Length == 0) goto err;
      var h = provider != null ? NumberFormatInfo.GetInstance(provider).NumberDecimalSeparator[0] : default;
      BigRat a = 0, e = 1, d = 0, p = 0; char c;
      for (int i = s.Length - 1; i >= 0; i--)
        switch (c = s[i])
        {
          case >= '0' and <= '9': a += e * (c - '0'); e *= 10; continue;
          case '.': case ',': if (h == default || h == c) d = e; if ((style & NumberStyles.AllowDecimalPoint) == 0) goto err; continue;
          case '\'': a *= e / (e - 1); continue;
          case '-': a = -a; if ((style & NumberStyles.AllowLeadingSign) == 0) goto err; continue;
          case '+' or '_' or '…': continue;
          case 'e' or 'E': p = Pow10((int)a); a = 0; e = 1; if ((style & NumberStyles.AllowExponent) == 0) goto err; continue;
          case '/': if (!TryParse(s.Slice(0, i), style, provider, out d)) goto err; result = d / a; return true;
          default: goto err;
        }
      if (d != 0) a /= d; if (p != 0) a *= p;
      result = a; return true; err: result = default; return false;
    }
    public static bool TryParse([NotNullWhen(true)] string? s, out BigRat result)
    {
      return TryParse(s.AsSpan(), NumberStyles.Float, null, out result);
    }
    public static bool TryParse(ReadOnlySpan<char> s, out BigRat result)
    {
      return TryParse(s, NumberStyles.Float, null, out result);
    }
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out BigRat result)
    {
      return TryParse(s.AsSpan(), style, provider, out result);
    }
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BigRat result)
    {
      return TryParse(s, NumberStyles.Float, provider, out result);
    }
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BigRat result)
    {
      return TryParse(s.AsSpan(), NumberStyles.Float, provider, out result);
    }

    public static BigRat Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands, IFormatProvider? provider = null)
    {
      if (s == null) throw new ArgumentNullException(nameof(s));
      if ((style & NumberStyles.AllowHexSpecifier) != 0) throw new ArgumentException(nameof(style));
      if (!TryParse(s, style, provider, out var result)) throw new ArgumentException(); return result;
    }
    public static BigRat Parse(string s)
    {
      return Parse(s.AsSpan(), NumberStyles.Float | NumberStyles.AllowThousands, null);
    }
    public static BigRat Parse(string s, NumberStyles style)
    {
      return Parse(s.AsSpan(), style, null);
    }
    public static BigRat Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
      return Parse(s, NumberStyles.Float, provider);
    }
    public static BigRat Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
      return Parse(s.AsSpan(), style, provider);
    }
    public static BigRat Parse(string s, IFormatProvider? provider)
    {
      return Parse(s.AsSpan(), NumberStyles.Float, provider);
    }

    // fast bit precise conversion
    public BigRat(double value)
    {
      if (value == 0) return; int h = ((int*)&value)[1], e = ((h >> 20) & 0x7FF) - 1075;
      if (e == 0x3cc) throw new OverflowException(nameof(value)); // NaN 
      var p = stackalloc uint[64]; Debug.Assert((abs(e) >> 5) + 8 <= 41);
      p[0] = 2; p[1] = *(uint*)&value; p[2] = (unchecked((uint)h) & 0x000FFFFF) | 0x100000;
      if (e == -1075) { if ((p[2] &= 0xfffff) == 0 && p[p[0] = 1] == 0) { *(ulong*)(p + 2) = 0x100000001; goto m1; } e++; } // denormalized
      if (e > 0) shl(p, e); var q = p + (p[0] + 1); *(ulong*)q = 0x100000001; if (e < 0) shl(q, -e); m1:
      var n = p[0]; p[0] |= unchecked((uint)h) & 0x80000000; this.p = new BigRat(p, 2 + n + p[n + 1]).p;
    }

    // fast bitlevel access, binary serialization etc.
    public ReadOnlySpan<uint> AsSpan()
    {
      return p;
    }
    public BigRat(ReadOnlySpan<uint> span)
    {
      uint n = unchecked((uint)span.Length), u, v, l; if (n == 0) return; // zero
      fixed (uint* p = span)
      {
        if ((u = p[0] & 0x7fffffff) == 0 || u + 3 > n || u != 1 && p[u] == 0) throw new ArgumentException($"{nameof(span)} {"numerator"}");
        if ((v = p[u + 1]) == 0 || (l = u + v + 2) > n || p[u + v + 1] == 0) throw new ArgumentException($"{nameof(span)} {"denominator"}");
        this.p = p[u] != 0 ? span.Slice(0, unchecked((int)l)).ToArray() : null;
      }
    }

    readonly uint[]? p;

    BigRat(uint[]? p)
    {
      this.p = p;
    }
    BigRat(uint* p, uint n)
    {
      //this.p = new uint[n]; for (uint i = 0; i < n; i++) this.p[i] = p[i]; return;
      if (p[1] == 0 && p[(p[0] & 0x3fffffff) + 2] == 0) // zero rem 
      {
        uint d = p[0] & 0x3fffffff, c = 1; var v = p + d + 1; var e = v[0];
        for (; p[c + 1] == 0 && v[c + 1] == 0; c++) ;
        copy(p + 1, p + 1 + c, d -= c); p[0] = (p[0] & 0x80000000) | d;
        copy(v - c + 1, v + c + 1, v[-c] = e - c); n -= c << 1;
      }
      fixed (uint* t = this.p = new uint[n]) copy(t, p, n);
    }
    BigRat(int n, int d)
    {
      fixed (uint* t = this.p = new uint[4])
      {
        p[0] = 1 | unchecked(((uint)n ^ (uint)d) & 0x80000000);
        p[1] = uabs(n); p[2] = 1; p[3] = uabs(d);
      }
    }

    static BigRat mul(uint* u, uint* v, uint* w)
    {
      if (u == v) return sqr(u, w);
      uint nu = u[0] & 0x3fffffff, nv = v[0] & 0x3fffffff;
      uint na = mul(u + 1, nu, v + 1, nv, w + 1);
      uint nb = mul(u + nu + 2, u[nu + 1], v + nv + 2, v[nv + 1], w + 2 + na);
      w[0] = (u[0] ^ v[0]) & 0x80000000 | na; w[na + 1] = nb;
      return new BigRat(w, na + nb + 2);
    }
    static BigRat sqr(uint* u, uint* w)
    {
      uint nu = u[0] & 0x3fffffff;
      uint na = sqr(u + 1, nu, w + 1);
      uint nb = sqr(u + nu + 2, u[nu + 1], w + 2 + na);
      w[0] = na; w[na + 1] = nb;
      return new BigRat(w, na + nb + 2);
    }
    static BigRat div(uint* u, uint* v, uint* w)
    {
      uint nu = u[0] & 0x3fffffff, nv = v[0] & 0x3fffffff;
      uint na = mul(u + 1, nu, v + nv + 2, v[nv + 1], w + 1);
      uint nb = mul(u + nu + 2, u[nu + 1], v + 1, nv, w + 2 + na);
      w[0] = (u[0] ^ v[0]) & 0x80000000 | na; w[na + 1] = nb;
      return new BigRat(w, na + nb + 2);
    }
    static BigRat add(uint* u, uint* v, uint* w, uint f)
    {
      uint nu = u[0] & 0x3fffffff, nv = v[0] & 0x3fffffff;
      uint* wa = w + 1, wb, vv, uu;
      uint na = mul(u + 1, nu, vv = v + nv + 2, v[nv + 1], wa);
      uint nb = mul(uu = u + nu + 2, u[nu + 1], v + 1, nv, wb = wa + na), si = 0;
      if (((u[0] ^ v[0] ^ f) & 0x80000000) != 0)
      {
        si = na == nb ? 0 : na > nb ? 1 : 0x80000000;
        if (si == 0) for (uint i = na; i != 0;) if (wa[--i] != wb[i]) { si = wa[i] > wb[i] ? 1 : 0x80000000; break; }
        if (si == 0) return default;
      }
      na = si == 0 ? add(wa, na, wb, nb, wa) : si == 1 ? sub(wa, na, wb, nb, wa) : sub(wb, nb, wa, na, wa);
      nb = mul(uu, u[nu + 1], vv, v[nv + 1], w + 2 + na);
      w[0] = ((u[0] ^ si) & 0x80000000) | na; w[na + 1] = nb;
      return new BigRat(w, na + nb + 2);
    }
    static BigRat mod(BigRat a, int f)
    {
      if (a.p == null) return a;
      fixed (uint* p = a.p)
      {
        var n = p[0] & 0x3fffffff; if (*(ulong*)(p + n + 1) == 0x100000001) return a;
        var u = stackalloc uint[a.p.Length << 1]; var v = u + a.p.Length; // n + 1;
        copy(u, p, n + 1); u[0] = n; mod(u, p + n + 1, v); var inc = false;
        switch (f)
        {
          case 1: inc = (p[0] & 0x80000000) != 0 && *(ulong*)u != 1; break; // Floor 
          case 2: inc = (p[0] & 0x80000000) == 0 && *(ulong*)u != 1; break; // Ceiling
          case 3: // Round
            {
              var t = u + n + 1; copy(t, p + n + 1, p[n + 1] + 1);
              shr(t, 1); var x = cms(u, t); if (x == 0 && (v[1] & 1) != 0) x = 1; inc = x > 0;
            }
            break;
          case 4: return *(ulong*)u != 1 ? default : a; // IsInteger // case 6: v = u; break;
          case 5: if (*(ulong*)u != 1) return default; break; // return int's only IsEvenInt, IsOddInt
        }
        if (inc) { var w = 1u; v[0] = add(v + 1, v[0], &w, 1, v + 1); }
        if (*(ulong*)v == 1) return default;
        var l = v[0]; v[0] |= p[0] & 0x80000000; *(ulong*)(v + l + 1) = 0x100000001;
        return new BigRat(v, l + 3);
      }
    }

    static BigRat log2(BigRat x, int bits)
    {
      var r = bits + 16; x = RoundB(x, r); Debug.Assert(x > 0);
      var a = ILogB(x); if (a != 0) x /= Pow2(a);
      var e = x.CompareTo(1); if (e == 0) return a; if (e < 0) { x *= 2; a--; }
      var b = default(BigRat); Debug.Assert(x > 1 && x < 2);
      for (int i = 1; i <= r; i++)
      {
        x = x * x; x = RoundB(x, r); if (x <= 2) continue;
        x = x / 2; var p = Pow2(-i); b += p; b = RoundB(b, r);
      }
      return a + b;
    }
    static BigRat log(BigRat x, int bits)
    {
      return log2(x, bits) / log2(exp(1, bits), bits); //todo: const log2(E)
    }
    static BigRat exp(BigRat x, int bits)
    {
      var s = x < 0; if (s) x = -x; if (bits < 16) bits = 16;
      int p = ILogB(x) + 1; if (p > 0) x = x / Pow2(p); else p = 0; // 0..0.5
      BigRat a = x + 1, b = x, c = 1, d; int i = 2, e = -bits - base2(p) - 16, r = bits + 32;
      for (; ; i++) { b *= x; c *= i; a += d = b / c; if (ILogB(d) < e) break; a = RoundB(a, r); }
      for (i = 0; i < p; i++) a = RoundB(a * a, r);
      if (s) a = 1 / a; return a;
    }
    static BigRat pi(int bits)
    {
      Debug.Assert(bits >= 0);
      bits += 16; BigRat c = 0, a = 1; //, t1 = -32, t2 = 1, t3 = 256, t4 = 64, t5 = 4, a = 1;
      for (int n = 0, l = (((bits - 1) >> 5) + 2) << 5; ; n++)
      {
        int t = 10 * n, s = n << 2; //var b = t1 / (s + 1) - t2 / (s + 3) + t3 / (t + 1) - t4 / (t + 3) - t5 / (t + 5) - t5 / (t + 7) + t2 / (t + 9);
        var b = new BigRat(-32, s + 1) - new BigRat(1, s + 3) + new BigRat(256, t + 1) - new BigRat(64, t + 3) - new BigRat(4, t + 5) - new BigRat(4, t + 7) + new BigRat(1, t + 9);
        var d = a * b; var i = -ILogB(d); if (i > bits) break;
        c += d; c = RoundB(c, l); a /= -1024;
      }
      c /= 64; return c;
    }
    static BigRat sin(BigRat x, int bits, bool cos)
    {
      int c = bits + 32, e = ILogB(x);
      var pih = pi(c + Math.Max(0, e)); pih /= 2; if (cos) x += pih;
      var u = Truncate(x / pih); var s = u.p != null ? unchecked((int)u.p[1]) : 0; if (u < 0) s = -s;
      if (s != 0) { x -= u * pih; if ((s & 1) != 0) x = pih - x; if ((s & 2) != 0) x = -x; }
      if (x.p == null) return default;
      //if ((e = ILogB(u = x < 0 ? pih + x : pih - x)) < -27) { if (e < -bits) return Sign(x); c += 64; } // < 1e-8 near 90°
      BigRat f = 6, a = x, b = x * x, d; b = RoundB(b, c);
      for (int i = 1, k = 4; ; i++, k += 2)
      {
        a *= b; a = RoundB(a, c); d = a / f; var t = -ILogB(d); if (t > c) break;
        if ((i & 1) != 1) x += d; else x -= d; f *= k * (k + 1); x = RoundB(x, c);
      }
      return x;
    }
    static BigRat sqrt(BigRat x, int bits)
    {
      Debug.Assert(x >= 0 && bits >= 0); if (x.p == null) return default;
      var a = est(x); var e = abs(ILog10(x)); bits += 16; if (e > bits) bits = e;
      for (int i = 0; ; i++)
      {
        var f = a * a - x; var d = -ILogB(f); if (d > bits) break;
        a = RoundB(a, bits); a = (x / a + a) / 2;
      }
      return a;
      static BigRat est(BigRat x)
      {
        var u = stackalloc uint[x.p!.Length]; fixed (uint* t = x.p) copy(u, t, unchecked((uint)x.p.Length));
        uint nu = u[0], nv = u[nu + 1]; var v = u + nu + 1;
        var mu = (nu << 5) - unchecked((uint)BitOperations.LeadingZeroCount(u[nu]));
        var mv = (nv << 5) - unchecked((uint)BitOperations.LeadingZeroCount(v[nv]));
        if (mu > 1) shr(u, unchecked((int)(mu >> 1))); var un = u[0];
        if (mv > 1) shr(v, unchecked((int)(mv >> 1))); var vn = v[0];
        if (un != nu) copy(u + un + 1, v, vn + 1); return new BigRat(u, 2 + un + vn);
      }
    }
    static BigRat atan(BigRat _x, int bits)
    {
      var x = _x; var s = Sign(x); if (s == 0) return default; // atan(0) = 0
      if (s < 0) x = -x; // atan(-x) = -atan(x)
      var td = x > 1; if (td) x = 1 / x; // atan(1/x) = pi/2 - atan(x)
      var sh = 0; bits += 32; // atan(x) = atan(c) + atan((x - c) / (1 + c * x)) -> c == (x - c) / (1 + c * x) -> c <= 0.1
      for (var d = new BigRat(1, 10); x > d;) { var y = 1 / x; x = sqrt(y * y + 1, bits) - y; x = RoundB(x, bits); sh++; }
      BigRat zsqr = x * x, zsqr1 = zsqr + 1, a = 1, b = 1;
      for (int n = 2; ; n += 2) // n < 20
      {
        b *= n * zsqr / ((n + 1) * zsqr1);
        a += b; a = RoundB(a, bits); var l = -ILogB(b); if (l > bits) break;
      }
      x = (x * a) / zsqr1; if (sh != 0) x *= Pow2(sh); a = b = x;//rem
      if (td) x = pi(bits) / 2 - x;
      if (s < 0) x.p![0] ^= 0x80000000;
      return x;
    }

    static int tos(Span<char> s, BigRat v, bool reps, out int rep)
    {
      rep = -1; if (v.p == null) { s[0] = '0'; return 1; }
      uint nv = unchecked((uint)v.p.Length), np = v.p[0] & 0x3fffffff, ten = 10, nr = 0; int ex = 0;
      if (reps) { nr = v.p[np + 1] + 1; var uex = (ulong)nr * (uint)s.Length; if (uex > (500000 >> 2)) uex = 0; ex = unchecked((int)uex); }
      var need = checked(v.p.Length + 8 + ex); uint* pa = default;
      try
      {
        uint* p; if (need < (500000 >> 2)) { var t = stackalloc uint[need]; p = t; }
        else p = pa = (uint*)alloc(need << 2);
        uint* d = p + np + 1 + 4;
        fixed (uint* t = v.p) { copy(p, t, np + 1); copy(d, t + np + 1, t[np + 1] + 1); p[0] &= 0x3fffffff; }
        uint* w = p + nv + 4, r = w + 4;
        for (int i = 0; i < s.Length; i++)
        {
          mod(p, d, w); var c = (char)('0' + w[1]); // Debug.Assert(c >= '0' && c <= '9');
          if (ex != 0 && c != '0')
          {
            for (int t = 0; t < i; t++)
            {
              if (s[t] != c || cms(p, r + t * nr) != 0) continue;
              for (; t != 0 && s[i - 1] == '0' && s[t - 1] == '0'; i--, t--) ;
              rep = t; return i;
            }
            copy(r + i * nr, p, p[0] + 1); Debug.Assert(p[0] < nr);
          }
          s[i] = c; if (*(ulong*)p == 1) return i + 1;
          p[0] = mul(p + 1, p[0], &ten, 1, p + 1);
        }
      }
      finally { free(pa); }
      return s.Length;
    }
    static int tos(Span<char> s, uint u, int m)
    {
      int i = 0; for (; u != 0 || i < m; u /= 10u) s[i++] = unchecked((char)('0' + u % 10));
      s.Slice(0, i).Reverse(); return i;
    }
    static void toi(BigRat x, uint* d, uint f)
    {
      uint i, c, l = f & 0xffff; if (x.p == null) { for (i = 0; i < l; i++) d[i] = 0; return; }
      fixed (uint* p = Truncate(x).p)
      {
        var n = p[0] & 0x3fffffff; var s = (p[0] & 0x80000000) != 0;
        if ((f & 0x10000) != 0) // unsigned
        {
          if (s || n > l)
          {
            if ((f & 0x80000) != 0) throw new OverflowException();
            if ((f & 0x20000) != 0) { c = s ? 0 : 0xffffffff; for (i = 0; i < l; i++) d[i] = c; return; }
            if (!s) { for (i = 0; i < l; i++) d[i] = 0; return; }
            c = n < l ? n : l; for (i = 0; i < c; d[i] = p[++i]) ; neg(d, c);
            for (; i < l; d[i++] = 0xffffffff) ; return;
          }
        }
        else
        {
          i = 0; if (s && n == l && p[n] == 0x80000000) for (i = 1; i < n && p[i] == 0; i++) ; //if (i == n) { } //MinValue
          if (n > l || i != n && n == l && (p[n] & 0x80000000) != 0)
          {
            if ((f & 0x80000) != 0) throw new OverflowException();
            c = (f & 0x20000) != 0 && !s ? 0xffffffff : 0;
            for (i = 0; i < l; i++) d[i] = c; d[l - 1] ^= 0x80000000; return;
          }
        }
        for (i = 0; i < n; d[i] = p[++i]) ; for (; i < l; d[i++] = 0) ;
        if (s) neg(d, l); static void neg(uint* d, uint l) { for (uint i = 0, c = 1; i < l; i++) { var t = (ulong)(d[i] ^ 0xffffffff) + c; d[i] = (uint)t; c = (uint)(t >> 32); } }
      }
    }
    static decimal tom(BigRat x, bool sat)
    {
      if (x.p == null) return default;
      fixed (uint* p = x.p)
      {
        var u = p; uint nu = u[0] & 0x3ffffff, nv = u[nu + 1]; var v = u + nu + 1;
        var mu = unchecked((int)nu << 5) - BitOperations.LeadingZeroCount(u[nu]);
        var mv = unchecked((int)nv << 5) - BitOperations.LeadingZeroCount(v[nv]);
        var sh = Math.Max(mu - 96, mv - 96);
        if (sh > 0)
        {
          var t = stackalloc uint[x.p.Length]; copy(t, u, unchecked((uint)x.p.Length));
          v = (u = t) + nu + 1; shr(u, sh); shr(v, sh); nu = u[0]; nv = v[0];
          if (*(ulong*)u == 1) return default;
          if (*(ulong*)v == 1)
          {
            if (sat) return (p[0] & 0x80000000) != 0 ? decimal.MinValue : decimal.MaxValue;
            throw new ArgumentException();
          }
        }
        var du = new decimal(((int*)u)[1], nu >= 2 ? ((int*)u)[2] : 0, nu >= 3 ? ((int*)u)[3] : 0, (x.p[0] & 0x80000000) != 0, 0);
        var dv = new decimal(((int*)v)[1], nv >= 2 ? ((int*)v)[2] : 0, nv >= 3 ? ((int*)v)[3] : 0, false, 0);
        return du / dv;
      }
    }
    static void pow10(uint* p, uint e)
    {
      uint n = e / 19, r = e - n * 19; ulong a = 1, b = 10;
      for (; ; r >>= 1) { if ((r & 1) != 0) a *= b; if (r <= 1) break; b *= b; }
      *(ulong*)(p + 1) = a; p[0] = p[2] != 0 ? 2u : 1u; if (n == 0) return;
      var l = 4 + (int)(e >> 3); uint* v = stackalloc uint[l << 1], w = v + l, t;
      v[0] = 2; *(ulong*)(v + 1) = 10000000000000000000;
      for (; ; n >>= 1)
      {
        if ((n & 1) != 0) { copy(w, p, p[0] + 1); p[0] = mul(v + 1, v[0], w + 1, w[0], p + 1); } // p *= v;
        if (n <= 1) break; w[0] = sqr(v + 1, v[0], w + 1); t = v; v = w; w = t; // v *= v; 
      }
    }

    static int cms(uint* a, uint* b)
    {
      uint na = a[0], nb = b[0]; if (na != nb) return na > nb ? +1 : -1;
      for (var i = na + 1; --i != 0;) if (a[i] != b[i]) return a[i] > b[i] ? +1 : -1;
      return 0;
    }
    static int cmp(uint* a, uint* b)
    {
      if (a == b) return 0;
      var su = a == null ? 0 : (a[0] & 0x80000000) != 0 ? -1 : +1;
      var sv = b == null ? 0 : (b[0] & 0x80000000) != 0 ? -1 : +1;
      if (su != sv) return su > sv ? +1 : -1;
      uint nu = a[0] & 0x3fffffff, du = a[nu + 1], nv = b[0] & 0x3fffffff, dv = b[nv + 1];
      uint* w = stackalloc uint[unchecked((int)(nu + du + nv + dv + 8))], p;
      uint cu = mul(a + 1, nu, b + nv + 2, dv, w);
      uint cv = mul(b + 1, nv, a + nu + 2, du, p = w + cu);
      if (cu != cv) return cu > cv ? su : -su;
      for (uint i = cu; i != 0;) if (w[--i] != p[i]) return w[i] > p[i] ? su : -su;
      return 0;
    }
    static uint mul(uint* a, uint na, uint* b, uint nb, uint* r)
    {
      //todo: enable optis
      //if ((na < nb ? na : nb) >= 32 && na + nb < 25000) { kmu(a, na, b, nb, r); if (r[(na = na + nb) - 1] == 0) na--; return na; }
      //if (na < nb) { var u = na; na = nb; nb = u; var v = a; a = b; b = v; }
      //if (na == 1) { *(ulong*)r = (ulong)a[0] * b[0]; goto ret; }
      //if (na == 2 && Bmi2.X64.IsSupported) { *(ulong*)(r + 2) = Bmi2.X64.MultiplyNoFlags(*(ulong*)a, nb == 2 ? *(ulong*)b: b[0], (ulong*)r); goto ret; } 
      ulong c = 0, d;
      for (uint k = 0; k < na; k++, c = d >> 32)
        r[k] = unchecked((uint)(d = c + (ulong)a[k] * b[0]));
      r[na] = (uint)c;
      for (uint i = 1, k; i < nb; i++)
      {
        for (k = 0, c = 0; k < na; k++, c = d >> 32)
          r[i + k] = unchecked((uint)(d = r[i + k] + c + (ulong)a[k] * b[i]));
        r[i + na] = (uint)c;
      } //ret:
      if (r[(na = na + nb) - 1] == 0) na--; return na;
    }
    static uint sqr(uint* a, uint n, uint* r)
    {
      if (n <= 2)
      {
        if (n < 2) { *(ulong*)r = (ulong)a[0] * a[0]; goto m1; }
        if (Bmi2.X64.IsSupported) { ((ulong*)r)[1] = Bmi2.X64.MultiplyNoFlags(*(ulong*)a, *(ulong*)a, ((ulong*)r)); goto m1; }
        var t1 = (ulong)a[0] * a[0];
        var t2 = (ulong)a[0] * a[1];
        var t3 = (ulong)a[1] * a[1];
        var t4 = t2 + (t1 >> 32);
        var t5 = t2 + (uint)t4;
        ((ulong*)r)[0] = t5 << 32 | (uint)t1;
        ((ulong*)r)[1] = t3 + (t4 >> 32) + (t5 >> 32); goto m1;
      }
      for (uint i = 0; i < n; i++)
      {
        ulong c = 0, e, f;
        for (uint j = 0; j < i; j++, c = (f + (e >> 1)) >> 31)
          r[i + j] = unchecked((uint)((e = r[i + j] + c) + ((f = (ulong)a[j] * a[i]) << 1)));
        *(ulong*)(r + (i << 1)) = (ulong)a[i] * a[i] + c;
      }
    m1: if (r[(n = n << 1) - 1] == 0) n--; Debug.Assert(r[n - 1] != 0); return n;
    }
    static uint add(uint* a, uint na, uint* b, uint nb, uint* r)
    {
      if (na < nb) { var p = a; a = b; b = p; var t = na; na = nb; nb = t; }
      uint i = 0; ulong c = 0, d;
      for (; i < nb; i++, c = d >> 32) r[i] = unchecked((uint)(d = (a[i] + c) + b[i]));
      for (; i < na; i++, c = d >> 32) r[i] = unchecked((uint)(d = a[i] + c));
      return (r[i] = unchecked((uint)c)) != 0 ? i + 1 : i;
    }
    static uint sub(uint* a, uint na, uint* b, uint nb, uint* r)
    {
      uint i = 0; var c = 0L; Debug.Assert(na >= nb);
      for (; i < nb; i++) { var u = a[i] + c - b[i]; r[i] = unchecked((uint)u); c = u >> 32; }
      for (; i < na; i++) { var u = a[i] + c; /*  */ r[i] = unchecked((uint)u); c = u >> 32; }
      for (; i > 1 && r[i - 1] == 0; i--) ; Debug.Assert(c == 0); return i;
    }
    static void mod(uint* a, uint* b, uint* r)
    {
      uint na = a[0], nb = b[0] & 0x3fffffff; if (r != null) *(ulong*)r = 1;
      if (na < nb) return;
      if (na == 1)
      {
        if (b[1] == 0) return;
        uint q = a[1] / b[1], m = a[1] - q * b[1];
        a[a[0] = 1] = m; if (r != null) r[1] = q; return;
      }
      if (na == 2)
      {
        ulong x = *(ulong*)(a + 1), y = nb != 1 ? *(ulong*)(b + 1) : b[1];
        ulong q = x / y, m = x - (q * y);
        *(ulong*)(a + 1) = m; a[0] = a[2] != 0 ? 2u : 1u; if (r == null) return;
        *(ulong*)(r + 1) = q; r[0] = r[2] != 0 ? 2u : 1u; return;
      }
      if (nb == 1)
      {
        ulong x = 0; uint y = b[1];
        for (uint i = na; i != 0; i--)
        {
          ulong q = (x = (x << 32) | a[i]) / y, m = x - (q * y);
          if (r != null) r[i] = unchecked((uint)q); x = m;
        }
        a[0] = 1; a[1] = unchecked((uint)x); if (r == null) return;
        for (; na > 1 && r[na] == 0; na--) ; r[0] = na; return;
      }
      uint dh = b[nb], dl = nb > 1 ? b[nb - 1] : 0;
      int sh = BitOperations.LeadingZeroCount(dh), sb = 32 - sh;
      if (sh > 0)
      {
        uint db = nb > 2 ? b[nb - 2] : 0;
        dh = (dh << sh) | (dl >> sb);
        dl = (dl << sh) | (db >> sb);
      }
      for (uint i = na, d; i >= nb; i--)
      {
        uint n = i - nb, t = i < na ? a[i + 1] : 0;
        ulong vh = ((ulong)t << 32) | a[i];
        uint vl = i > 1 ? a[i - 1] : 0;
        if (sh > 0)
        {
          uint va = i > 2 ? a[i - 2] : 0;
          vh = (vh << sh) | (vl >> sb);
          vl = (vl << sh) | (va >> sb);
        }
        ulong di = vh / dh; if (di > 0xffffffff) di = 0xffffffff;
        for (; ; di--)
        {
          ulong th = dh * di, tl = dl * di; th += tl >> 32;
          if (th < vh) break; if (th > vh) continue;
          if ((tl & 0xffffffff) > vl) continue; break;
        }
        if (di != 0)
        {
          ulong c = 0; var p = a + n;
          for (uint k = 1; k <= nb; k++)
          {
            c += b[k] * di; d = unchecked((uint)c); c >>= 32;
            if (p[k] < d) c++; p[k] = p[k] - d; // ryujit
          }
          if (unchecked((uint)c) != t)
          {
            c = 0; p = a + n; di--; ulong g;
            for (uint k = 1; k <= nb; k++, c = g >> 32) p[k] = unchecked((uint)(g = (p[k] + c) + b[k]));
          }
        }
        if (di != 0 && r != null)
        {
          var x = n + 1; for (var j = r[0] + 1; j < x; j++) r[j] = 0;
          if (r[0] < x) r[0] = x; r[x] = unchecked((uint)di);
        }
        if (i < na) a[i + 1] = 0;
      }
      for (; a[0] > 1 && a[a[0]] == 0; a[0]--) ; //todo: nb == 1 case, test and rem
    }
    static void inv(uint* p)
    {
      uint n = (p[0] & 0x3fffffff) + 1, m = p[n] + 1, t, a, b;
      p[n] |= p[0] & 0x80000000; p[0] &= 0x3fffffff;
      if (n == m) { for (a = 0; a < n; a++) { t = p[a]; p[a] = p[b = a + n]; p[b] = t; } }
      else
      {
        uint c = n - 1, d = m + c;
        for (a = 0, b = c; a < b; a++, b--) { t = p[a]; p[a] = p[b]; p[b] = t; }
        for (a = n, b = d; a < b; a++, b--) { t = p[a]; p[a] = p[b]; p[b] = t; }
        for (a = 0, b = d; a < b; a++, b--) { t = p[a]; p[a] = p[b]; p[b] = t; }
      }
    }
    static void shr(uint* p, int c)
    {
      int s = c & 31, r = 32 - s; uint n = p[0] & 0x3fffffff, i = 0, k = 1 + unchecked((uint)c >> 5), l = k <= n ? p[k++] >> s : 0;
      while (k <= n) { var t = p[k++]; p[++i] = l | unchecked((uint)((ulong)t << r)); l = t >> s; }
      if (l != 0 || i == 0) p[++i] = l; p[0] = i;
    }
    static void shl(uint* p, int c)
    {
      int s = c & 31, r = 32 - s; uint d = unchecked((uint)(c >> 5)), n = p[0] & 0x3fffffff; p[0] = p[n + 1] = 0;
      for (uint i = n + 1; i > 0; i--) p[i + d] = (p[i] << s) | unchecked((uint)((ulong)p[i - 1] >> r));
      for (uint i = 1; i <= d; i++) p[i] = 0;
      n += d; p[0] = p[n + 1] != 0 ? n + 1 : n;
    }
    static uint* gcd(uint* u, uint* v)
    {
      if (cms(u, v) < 0) { var t = u; u = v; v = t; }
      while (v[0] > 2)
      {
        uint e = u[0] - v[0];
        if (e <= 2)
        {
          ulong xh = u[u[0]], xm = u[u[0] - 1], xl = u[u[0] - 2];
          ulong yh = e == 0 ? v[v[0]] : 0, ym = e <= 1 ? v[v[0] - (1 - e)] : 0, yl = v[v[0] - (2 - e)];
          int z = BitOperations.LeadingZeroCount(unchecked((uint)xh));
          ulong x = ((xh << 32 + z) | (xm << z) | (xl >> 32 - z)) >> 1;
          ulong y = ((yh << 32 + z) | (ym << z) | (yl >> 32 - z)) >> 1; Debug.Assert(x >= y);
          uint a = 1, b = 0, c = 0, d = 1, k = 0;
          while (y != 0)
          {
            ulong q, r, s, t;
            q = x / y; if (q > 0xffffffff) break;
            r = a + q * c; if (r > 0x7fffffff) break;
            s = b + q * d; if (s > 0x7fffffff) break;
            t = x - q * y; if (t < s || t + r > y - c) break;
            a = unchecked((uint)r);
            b = unchecked((uint)s); k++;
            x = t; if (x == b) break;
            q = y / x; if (q > 0xffffffff) break;
            r = d + q * b; if (r > 0x7fffffff) break;
            s = c + q * a; if (s > 0x7fffffff) break;
            t = y - q * x; if (t < s || t + r > x - b) break;
            d = unchecked((uint)r);
            c = unchecked((uint)s); k++;
            y = t; if (y == c) break;
          }
          if (b != 0)
          {
            long cx = 0, cy = 0;
            for (uint i = 1, l = v[0]; i <= l; i++)
            {
              long ui = u[i], vi = v[i];
              long dx = a * ui - b * vi + cx; u[i] = unchecked((uint)dx); cx = dx >> 32;
              long dy = d * vi - c * ui + cy; v[i] = unchecked((uint)dy); cy = dy >> 32;
            }
            u[0] = v[0];
            for (; u[0] > 1 && u[u[0]] == 0; u[0]--) ;
            for (; v[0] > 1 && v[v[0]] == 0; v[0]--) ;
            if ((k & 1) != 0) { var t = u; u = v; v = t; }
            continue;
          }
        }
        mod(u, v, null); var p = u; u = v; v = p;
      }
      if (*(ulong*)v != 1)
      {
        if (u[0] > 2) { mod(u, v, null); var t = u; u = v; v = t; if (*(ulong*)v == 1) goto m1; }
        ulong x = u[0] != 1 ? *(ulong*)(u + 1) : u[1];
        ulong y = v[0] != 1 ? *(ulong*)(v + 1) : v[1];
        while (y != 0 && (x | y) > 0xffffffff) { var t = x % y; x = y; y = t; }
        while (y != 0) { var t = unchecked((uint)x) % unchecked((uint)y); x = y; y = t; }
        *(ulong*)(u + 1) = x; u[0] = u[2] != 0 ? 2u : 1u; m1:;
      }
      return u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int abs(int x)
    {
      return (x ^ (x >> 31)) - (x >> 31);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint uabs(int x)
    {
      return unchecked((uint)abs(x));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int base2(int digits)
    {
      //return (int)Math.Ceiling(digits * 3.321928094887362);
      return unchecked((int)(((long)digits * 14267572527) >> 32)) + 1;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void copy(uint* d, uint* s, uint n)
    {
      //for (uint i = 0; i < n; i++) d[i] = s[i];
      Unsafe.CopyBlock(d, s, n << 2);
    }
    static void copy(uint* d, int v)
    {
      d[0] = unchecked((uint)v & 0x80000000) | 1;
      d[1] = uabs(v); ((ulong*)d)[1] = 0x100000001;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void* alloc(int n)
    {
      return Marshal.AllocCoTaskMem(n).ToPointer();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void free(void* p)
    {
      if (p != null) Marshal.FreeCoTaskMem((IntPtr)p);
    }

#if NET7_0_OR_GREATER 
    public static implicit operator BigRat(Int128 value)
    {
      var p = (uint*)&value; var s = (p[3] & 0x80000000) != 0; if (s) value = -value;
      var a = (BigRat)(*(UInt128*)p); if (s) a.p![0] |= 0x80000000; return a;
    }
    public static implicit operator BigRat(UInt128 value)
    {
      var p = (ulong*)&value; return p[1] == 0 ? p[0] : p[0] + p[1] * Pow2(64); //todo: opt
    }
    public static explicit operator Int128(BigRat value)
    {
      Int128 t; toi(value, (uint*)&t, 4); return t;
    }
    public static explicit operator UInt128(BigRat value)
    {
      UInt128 t; toi(value, (uint*)&t, 4 | 0x10000); return t;
    }
    public static explicit operator checked Int128(BigRat value)
    {
      Int128 t; toi(value, (uint*)&t, 4 | 0x80000); return t;
    }
    public static explicit operator checked UInt128(BigRat value)
    {
      UInt128 t; toi(value, (uint*)&t, 4 | 0x10000 | 0x80000); return t;
    }

    public static bool IsNaN(BigRat value)
    {
      return false;
    }
    public static bool IsNormal(BigRat value)
    {
      return value.p != null;
    }
    public static bool IsFinite(BigRat value)
    {
      return true;
    }
    public static bool IsInfinity(BigRat value)
    {
      return false;
    }
    public static bool IsNegativeInfinity(BigRat value)
    {
      return false;
    }
    public static bool IsPositiveInfinity(BigRat value)
    {
      return false;
    }
    public static bool IsRealNumber(BigRat value)
    {
      return true;
    }
    public static bool IsSubnormal(BigRat value)
    {
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigRat CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      if (TryConvertFrom(value, out var a) || TOther.TryConvertToChecked(value, out a)) return a;
      throw new NotSupportedException();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigRat CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      if (TryConvertFrom(value, out var a) || TOther.TryConvertToSaturating(value, out a)) return a;
      throw new NotSupportedException();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigRat CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      if (TryConvertFrom(value, out var a) || TOther.TryConvertToTruncating(value, out a)) return a;
      throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<BigRat>.TryConvertFromChecked<TOther>(TOther value, out BigRat result)
    {
      return TryConvertFrom(value, out result);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<BigRat>.TryConvertFromSaturating<TOther>(TOther value, out BigRat result)
    {
      return TryConvertFrom(value, out result);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<BigRat>.TryConvertFromTruncating<TOther>(TOther value, out BigRat result)
    {
      return TryConvertFrom(value, out result);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<BigRat>.TryConvertToSaturating<T>(BigRat value, out T result)
    {
      return TryConvertTo<T>(value, out result);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<BigRat>.TryConvertToTruncating<T>(BigRat value, out T result)
    {
      return TryConvertTo<T>(value, out result);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<BigRat>.TryConvertToChecked<T>(BigRat value, out T result)
    {
      if (typeof(T) == typeof(BigRat)) { result = (T)(object)(BigRat)value; return true; }
      else if (typeof(T) == typeof(double)) { result = (T)(object)checked((double)value); return true; }
      else if (typeof(T) == typeof(long)) { result = (T)(object)checked((long)value); return true; }
      else if (typeof(T) == typeof(ulong)) { result = (T)(object)checked((long)value); return true; }
      else if (typeof(T) == typeof(nint)) { result = (T)(object)checked((nint)value); return true; }
      else if (typeof(T) == typeof(nuint)) { result = (T)(object)checked((nuint)value); return true; }
      else if (typeof(T) == typeof(decimal)) { result = (T)(object)checked((decimal)value); return true; }
      else if (typeof(T) == typeof(Int128)) { result = (T)(object)checked((Int128)value); return true; }
      else if (typeof(T) == typeof(UInt128)) { result = (T)(object)checked((UInt128)value); return true; }
      else if (typeof(T) == typeof(BigInteger)) { result = (T)(object)checked((BigInteger)value); return true; }
      else return T.TryConvertFromChecked((double)value, out result!);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool TryConvertTo<T>(BigRat value, out T result) where T : INumberBase<T>
    {
      if (typeof(T) == typeof(BigRat)) { result = (T)(object)value; return true; }
      else if (typeof(T) == typeof(double)) { result = (T)(object)(double)value; return true; }
      else if (typeof(T) == typeof(long)) { long t; toi(value, (uint*)&t, 2 | 0x20000); result = (T)(object)t; return true; }
      else if (typeof(T) == typeof(ulong)) { ulong t; toi(value, (uint*)&t, 2 | 0x20000 | 0x10000); result = (T)(object)t; return true; }
      else if (typeof(T) == typeof(nint)) { nint t; toi(value, (uint*)&t, ((uint)sizeof(nint) >> 2) | 0x20000); result = (T)(object)t; return true; }
      else if (typeof(T) == typeof(nuint)) { nuint t; toi(value, (uint*)&t, ((uint)sizeof(nuint) >> 2) | 0x20000 | 0x10000); result = (T)(object)t; return true; }
      else if (typeof(T) == typeof(Int128)) { Int128 t; toi(value, (uint*)&t, 4 | 0x20000); result = (T)(object)t; return true; }
      else if (typeof(T) == typeof(UInt128)) { UInt128 t; toi(value, (uint*)&t, 4 | 0x20000 | 0x10000); result = (T)(object)t; return true; }
      else if (typeof(T) == typeof(decimal)) { result = (T)(object)tom(value, true); return true; }
      else if (typeof(T) == typeof(BigInteger)) { result = (T)(object)(BigInteger)value; return true; }
      else return T.TryConvertFromSaturating((double)value, out result!);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool TryConvertFrom<T>(T value, out BigRat result) where T : INumberBase<T>
    {
      if (typeof(T) == typeof(BigRat)) { result = (BigRat)(object)value; return true; }
      else if (typeof(T) == typeof(double)) { result = (BigRat)(double)(object)value; return true; }
      else if (typeof(T) == typeof(float)) { result = (BigRat)(float)(object)value; return true; }
      else if (typeof(T) == typeof(Half)) { result = (BigRat)(Half)(object)value; return true; }
      else if (typeof(T) == typeof(byte)) { result = (BigRat)(byte)(object)value; return true; }
      else if (typeof(T) == typeof(sbyte)) { result = (BigRat)(sbyte)(object)value; return true; }
      else if (typeof(T) == typeof(char)) { result = (BigRat)(char)(object)value; return true; }
      else if (typeof(T) == typeof(ushort)) { result = (BigRat)(ushort)(object)value; return true; }
      else if (typeof(T) == typeof(short)) { result = (BigRat)(short)(object)value; return true; }
      else if (typeof(T) == typeof(int)) { result = (BigRat)(int)(object)value; return true; }
      else if (typeof(T) == typeof(uint)) { result = (BigRat)(uint)(object)value; return true; }
      else if (typeof(T) == typeof(long)) { result = (BigRat)(long)(object)value; return true; }
      else if (typeof(T) == typeof(ulong)) { result = (BigRat)(ulong)(object)value; return true; }
      else if (typeof(T) == typeof(nint)) { result = (BigRat)(nint)(object)value; return true; }
      else if (typeof(T) == typeof(nuint)) { result = (BigRat)(nuint)(object)value; return true; }
      else if (typeof(T) == typeof(decimal)) { result = (BigRat)(decimal)(object)value; return true; }
      else if (typeof(T) == typeof(Int128)) { result = (BigRat)(Int128)(object)value; return true; }
      else if (typeof(T) == typeof(UInt128)) { result = (BigRat)(UInt128)(object)value; return true; }
      else if (typeof(T) == typeof(BigInteger)) { result = (BigRat)(BigInteger)(object)value; return true; }
      else { result = default; return false; }
    }

    static bool INumberBase<BigRat>.IsCanonical(BigRat value)
    {
      return true;
    }
    static bool INumberBase<BigRat>.IsComplexNumber(BigRat value)
    {
      return false;
    }
    static bool INumberBase<BigRat>.IsImaginaryNumber(BigRat value)
    {
      return false;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static int INumberBase<BigRat>.Radix
    {
      get { return 1; }
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static BigRat INumberBase<BigRat>.Zero
    {
      get { return default; }
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static BigRat INumberBase<BigRat>.One
    {
      get { return 1u; }
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static BigRat IAdditiveIdentity<BigRat, BigRat>.AdditiveIdentity
    {
      get { return default; }
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static BigRat IMultiplicativeIdentity<BigRat, BigRat>.MultiplicativeIdentity
    {
      get { return 1u; }
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static BigRat ISignedNumber<BigRat>.NegativeOne
    {
      get { return -1; }
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static BigRat IFloatingPointConstants<BigRat>.E
    {
      get { return Exp(1, 032); }
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static BigRat IFloatingPointConstants<BigRat>.Pi
    {
      get { return Pi(032); }
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    static BigRat IFloatingPointConstants<BigRat>.Tau
    {
      get { return Tau(032); }
    }
#endif
  }
}
