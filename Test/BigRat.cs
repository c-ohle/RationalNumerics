
#pragma warning disable CS8618, CS8765, CS8601, CS8625

using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Test
{
  [SkipLocalsInit, DebuggerDisplay("{ToString(),nq}")]
  public unsafe readonly struct BigRat : IEquatable<BigRat>, IComparable<BigRat>, IFormattable, ISpanFormattable
  {
    public override string ToString()
    {
      if (p == null) return "0"; p[0] |= 0x40000000;
      var cpu = rat.task_cpu; cpu.push(new ReadOnlySpan<uint>(p)); p[0] ^= 0x40000000;
      return cpu.popr().ToString();
    }
    public override int GetHashCode()
    {
      if (p == null) return 0;
      fixed (uint* p = this.p)
      {
        uint h = 0, n; n = p[(n = p[0] & 0x3fffffff) + 1] + n + 2;
        for (uint i = 0; i < n; i++) h = ((h << 7) | (h >> 25)) ^ p[i];
        return unchecked((int)h);
      }
    }
    public override bool Equals(object obj)
    {
      return obj is BigRat a ? Equals(a) : false;
    }
    public bool Equals(BigRat value)
    {
      return CompareTo(value) == 0;
    }
    public int CompareTo(BigRat value)
    {
      fixed (uint* u = this.p, v = value.p) return cmp(u, v);
    }
    public int CompareTo(int value)
    {
      if (value == 0) return Sign(this);
      var v = stackalloc uint[4]; mov(v, value);
      fixed (uint* u = this.p) return cmp(u, v);
    }

    public BigRat(int value)
    {
      p = value != 0 ? new uint[4] {
        unchecked((uint)value & 0x80000000) | 1,
        unchecked((uint)((value ^ (value >>= 31)) - value)), 1, 1 } : null;
    }
    public BigRat(uint value)
    {
      p = value != 0 ? new uint[4] { 1, value, 1, 1 } : null;
    }
    public BigRat(ulong value)
    {
      p = (value >> 32) != 0 ?
        new uint[] { 2, (uint)value, (uint)(value >> 32), 1, 1 } : value != 0 ?
        new uint[] { 1, (uint)value, 1, 1 } : null;
    }
    public BigRat(double value)
    {
      if (value == 0) return;
      var e = unchecked((int)((*(ulong*)&value >> 52) & 0x7FF) - 1022); //Debug.Assert(!double.IsFinite(v) == (e == 0x401));
      if (e == 0x401) { throw new ArgumentException(); } // NaN 
      int p = 14 - ((e * 19728) >> 16), t = p; if (p > 308) p = 308; // v < 1E-294
      var d = double.Abs(value) * double.Pow(10, p); // Debug.Assert(double.IsNormal(d));
      if (t != p) { d *= double.Pow(10, t = t - p); p += t; if (d < 1e14) { d *= 10; p++; } }
      var m = (ulong)double.Round(d);
      this.p = (m / Pow10(p)).p; if (value < 0) this.p[0] |= 0x80000000;
    }

    public static implicit operator BigRat(int value) => new BigRat(value);
    public static implicit operator BigRat(ulong value) => new BigRat(value);
    public static implicit operator BigRat(double value) => new BigRat(value);

    public static explicit operator int(BigRat value)
    {
      if (value.p == null) return 0;
      fixed (uint* p = value.p)
      {
        uint n = p[0] & 0x3fffffff, v = p[1];
        if (*(ulong*)(p + n + 1) != 0x100000001) return int.MinValue;
        if (n != 1 || (v & 0x80000000) != 0) return int.MinValue;
        return (p[0] & 0x80000000) == 0 ? (int)v : -(int)v;
      }
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
        if (vb == 0) return double.NaN;
        var e = ((na << 5) - ca) - ((nb << 5) - cb);
        if (e < -1021) return double.NegativeInfinity;
        if (e > +1023) return double.PositiveInfinity;
        var r = (double)(va >> 11) / (vb >> 11);
        var x = (0x3ff + e) << 52; r *= *(double*)&x; //fast r *= Math.Pow(2, e);
        return (a[0] & 0x80000000) != 0 ? -r : r;
      }
    }
    public static explicit operator BigInteger(BigRat value)
    {
      if (value.p == null) return default;
      var s = new ReadOnlySpan<uint>(Truncate(value).p);
      var a = System.Runtime.InteropServices.MemoryMarshal.Cast<uint, byte>(s.Slice(1, unchecked((int)(s[0] & 0x3fffffff))));
      var r = new BigInteger(a, true, false); if (value < 0) r = -r; return r;
    }

    public static BigRat operator +(BigRat a)
    {
      return a;
    }
    public static BigRat operator -(BigRat a)
    {
      if (a.p == null) return default;
      fixed (uint* p = a.p) { a = new BigRat(p, unchecked((uint)a.p.Length)); }
      a.p[0] ^= 0x80000000; return a;
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
      var v = stackalloc uint[4 + (a.p.Length + 4) * 3]; mov(v, b);
      fixed (uint* u = a.p) return add(u, v, v + 4, 0);
    }
    public static BigRat operator -(BigRat a, int b)
    {
      return a + -b;
    }
    public static BigRat operator *(BigRat a, int b)
    {
      if (a.p == null || b == 0) return default;
      var v = stackalloc uint[a.p.Length + 8]; mov(v, b);
      fixed (uint* u = a.p) return mul(u, v, v + 4);
    }
    public static BigRat operator /(BigRat a, int b)
    {
      if (b == 0) throw new DivideByZeroException();
      if (a.p == null) return default;
      var v = stackalloc uint[a.p.Length + 8]; mov(v, b);
      fixed (uint* u = a.p) return div(u, v, v + 4);
    }
    public static BigRat operator +(int a, BigRat b)
    {
      if (a == 0) return b;
      if (b.p == null) return a;
      var u = stackalloc uint[4 + (4 + b.p.Length) * 3]; mov(u, a);
      fixed (uint* v = b.p) return add(u, v, u + 4, 0);
    }
    public static BigRat operator -(int a, BigRat b)
    {
      if (a == 0) return -b;
      if (b.p == null) return a;
      var u = stackalloc uint[4 + (4 + b.p.Length) * 3]; mov(u, a);
      fixed (uint* v = b.p) return add(u, v, u + 4, 0x80000000);
    }
    public static BigRat operator *(int a, BigRat b)
    {
      if (a == 0 || b.p == null) return default;
      var u = stackalloc uint[8 + b.p.Length]; mov(u, a);
      fixed (uint* v = b.p) return mul(u, v, u + 4);
    }
    public static BigRat operator /(int a, BigRat b)
    {
      if (b.p == null) throw new DivideByZeroException();
      if (a == 0) return default;
      var u = stackalloc uint[8 + b.p.Length]; mov(u, a);
      fixed (uint* v = b.p) return div(u, v, u + 4);
    }
    public static BigRat operator ++(BigRat a)
    {
      return a + 1;
    }
    public static BigRat operator --(BigRat a)
    {
      return a - 1;
    }

    public static bool operator ==(BigRat a, BigRat b) => a.CompareTo(b) == 0;
    public static bool operator !=(BigRat a, BigRat b) => a.CompareTo(b) != 0;
    public static bool operator <=(BigRat a, BigRat b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BigRat a, BigRat b) => a.CompareTo(b) >= 0;
    public static bool operator <(BigRat a, BigRat b) => a.CompareTo(b) < 0;
    public static bool operator >(BigRat a, BigRat b) => a.CompareTo(b) > 0;

    public static bool operator ==(BigRat a, int b) => a.CompareTo(b) == 0;
    public static bool operator !=(BigRat a, int b) => a.CompareTo(b) != 0;
    public static bool operator <=(BigRat a, int b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BigRat a, int b) => a.CompareTo(b) >= 0;
    public static bool operator <(BigRat a, int b) => a.CompareTo(b) < 0;
    public static bool operator >(BigRat a, int b) => a.CompareTo(b) > 0;

    public static BigRat Abs(BigRat a)
    {
      return Sign(a) >= 0 ? a : -a;
    }
    public static BigRat Min(BigRat a, BigRat b)
    {
      return a.CompareTo(b) <= 0 ? a : b;
    }
    public static BigRat Max(BigRat a, BigRat b)
    {
      return a.CompareTo(b) >= 0 ? a : b;
    }
    public static BigRat Normalize(BigRat a)
    {
      if (a.p == null) return a;
      fixed (uint* p = a.p)
      {
        var d = p + (p[0] & 0x3fffffff) + 1; if (*(ulong*)d == 0x100000001) return a;
        var l = a.p.Length; var s = stackalloc uint[l * 3]; uint x;
        for (int i = 0; i < l; i++) s[i] = p[i]; s[0] &= 0x3fffffff;
        var e = gcd(s, s + s[0] + 1); if (*(ulong*)e == 0x100000001) return a;
        var t = s + l; for (int i = 0; i < l; i++) t[i] = p[i]; t[0] &= 0x3fffffff;
        var r = t + l; d = t + t[0] + 1; mod(t, e, r); mod(d, e, r + r[0] + 1); r[0] |= p[0] & 0x80000000;
        return new BigRat(r, r[(x = (r[0] & 0x3fffffff)) + 1] + x + 2);
      }
    }
    public static BigRat Truncate(BigRat value) => mod(value, 0);
    public static BigRat Floor(BigRat value) => mod(value, 1);
    public static BigRat Ceiling(BigRat value) => mod(value, 2);
    public static BigRat Round(BigRat value) => mod(value, 3);
    public static BigRat Round(BigRat value, int digits)
    {
      var e = Pow10(digits);
      return Round(value * e) / e;
    }
    public static BigRat Limit(BigRat value, uint digits)
    {
      if (value.p == null) return value;
      fixed (uint* p = value.p)
      {
        uint na = value.p[0] & 0x3fffffff, nb = p[na + 1];
        uint nc = na < nb ? na : nb; if (nc <= digits) return value;
        uint d = nc - digits, nu = na - d, nv = nb - d;
        var w = new uint[2 + nu + nv];
        fixed (uint* v = w)
        {
          v[0] = nu | (p[0] & 0x80000000); v[nu + 1] = nv;
          for (uint i = 1; i <= nu; i++) v[i] = p[i + d];
          for (uint i = 1; i <= nv;) v[nu + ++i] = p[na + i + d];
        }
        return new BigRat(w);
      }
    }
    public static BigRat Pow(BigRat x, int y)
    {
      BigRat r = 1u;
      for (var e = unchecked((uint)(y >= 0 ? y : -y)); ; e >>= 1)
      {
        if ((e & 1) != 0) r *= x;
        if (e <= 1) break; x *= x;
      }
      if (y < 0) r = 1 / r;
      return r;
    }
    public static BigRat Pow10(int y)
    {
      return Pow(10, y);
    }
    public static BigRat Sqrt(BigRat x, int digits)
    {
      if (x <= 0) { if (x == 0) return default; throw new ArgumentOutOfRangeException(); }
      BigRat e = Pow10(digits), a = est(x), f; int g;
      for (var t = ILog10(x) - 1; ;)
      {
        a = (x / a + a) / 2;
        f = a * a - x; if (f == 0) break; // return a; // break; Normalize(a);
        g = t - ILog10(f); if (g > digits) break;
        a = Limit(a, e.p[0] + 1);
      }
      a = Round(a * e) / e; //if ((e = Truncate(a)) == a) { };
      return a;
      static BigRat est(BigRat x)
      {
        var u = stackalloc uint[x.p.Length]; fixed (uint* t = x.p) mov(u, t, (uint)x.p.Length);
        uint nu = u[0], nv = u[nu + 1]; var v = u + nu + 1;
        var mu = (nu << 5) - unchecked((uint)BitOperations.LeadingZeroCount(u[nu]));
        var mv = (nv << 5) - unchecked((uint)BitOperations.LeadingZeroCount(v[nv]));
        if (mu > 1) shr(u, unchecked((int)(mu >> 1))); var un = u[0];
        if (mv > 1) shr(v, unchecked((int)(mv >> 1))); var vn = v[0];
        if (un != nu) mov(u + un + 1, v, vn + 1); return new BigRat(u, 2 + un + vn);
      }
    }
    //public static int ILog2(BigRat value)
    //{
    //  if (value.p == null) return -int.MaxValue; //infinity
    //  fixed (uint* p = value.p)
    //  {
    //    var h = p[0]; var a = h & 0x3fffffff; var q = p + a + 1; var b = q[0];
    //    var u = (a << 5) - unchecked((uint)BitOperations.LeadingZeroCount(p[a]));
    //    var v = (b << 5) - unchecked((uint)BitOperations.LeadingZeroCount(q[b]));
    //    return unchecked((int)u - (int)v);
    //  }
    //}
    public static int ILog10(BigRat value)
    {
      if (value.p == null) return 0; // -int.MaxValue; //-infinity
      fixed (uint* p = value.p)
      {
        double a = flog10(p), b = flog10(p + (p[0] & 0x3fffffff) + 1), c = a - b;
        int e = (int)c; if (c < 0)
        {
          var aa = GetNumerator(value);
          var bb = GetDenominator(value);
          var cc = (BigInteger)aa;
          var dd = (BigInteger)bb;
          var xx = BigInteger.Log10(cc);
          var yy = BigInteger.Log10(dd);

          e--;
        }
        return e;
      }
      static double flog10(uint* p)
      {
        uint n = p[0] & 0x3fffffff; if (n == 1) return double.Log10(p[1]);
        var c = BitOperations.LeadingZeroCount(p[n]); Debug.Assert(c != 32);
        var b = ((long)n << 5) - unchecked((uint)c);
        ulong h = p[n], m = p[n - 1], l = n > 2 ? p[n - 2] : 0;
        ulong x = (h << 32 + c) | (m << c) | (l >> 32 - c);
        var r = double.Log10(x) + (b - 64) * 0.3010299956639811952; return r;
      }
    }
    public static int Sign(BigRat value)
    {
      return value.p == null ? 0 : (value.p[0] & 0x80000000) != 0 ? -1 : +1;
    }
    public static BigRat Pi(int digits)
    {
      BigRat c = 0, t1 = -32, t2 = 1, t3 = 256, t4 = 64, t5 = 4, a = 1, e = Pow10(digits);
      for (int n = 0, l = 1 + digits / 3; ;)
      {
        int t = 10 * n, s = n << 2;
        var b = t1 / (s + 1) - t2 / (s + 3) + t3 / (t + 1) - t4 / (t + 3) - t5 / (t + 5) - t5 / (t + 7) + t2 / (t + 9);
        c += a * b; // var d = c / 64;
        if (++n >= l) break; a /= -1024;
        c = Limit(c, e.p[0] + 1);
      }
      c /= t4; c = Round(c * e) / e; return c;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
      Span<char> ss = stackalloc char[1024];
      if (!TryFormat(ss, out var charsWritten, format, formatProvider)) { }
      ss = ss.Slice(0, charsWritten); return ss.ToString();
    }
    public bool TryFormat(Span<char> s, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
      //s.Fill(' '); //todo: rem
      if (this.p == null) { s[0] = '0'; charsWritten = 1; return true; }
      int e = BigRat.ILog10(this); var a = this / BigRat.Pow10(e);
      if (a <= -10 || a >= 10) { a /= 10; e++; }
      Debug.Assert(a >= 0 ? (a >= 1 && a < 10) : (a <= -1 && a > -10));
      var ss = s.Slice(0, 30); var i = tos(ss, a, true, out var r);
      if (r != -1) { s.Slice(r, i - r).CopyTo(s.Slice(r + 1)); s[r] = '\''; i++; }
      if (r == 0) { s.Slice(0, i).CopyTo(s.Slice(1)); s[0] = '0'; i++; e++; }
      if (i == ss.Length && r == -1) s[i++] = '…';
      var x = e + 1;
      if ((r != -1 && e >= r) || (e <= -5 || e >= 17)) x = 1; else e = 0;
      if (x <= 0) { var t = 1 - x; s.Slice(0, i).CopyTo(s.Slice(t)); s.Slice(0, t).Fill('0'); i += t; x += t; }
      if (x > i) { s.Slice(i, x - i).Fill('0'); i = x; }
      if (x > 0 && x < i) { s.Slice(x, i - x).CopyTo(s.Slice(x + 1)); s[x] = '.'; i++; }
      if (this < 0) { s.Slice(0, i).CopyTo(s.Slice(1)); s[0] = '-'; i++; }
      if (e != 0) { s[i++] = 'E'; s[i++] = e > 0 ? '+' : '-'; i += tos(s.Slice(i), unchecked((uint)(((e ^ (e >>= 31)) - e))), 2); }//f == 'E' ? 3 : 
      charsWritten = i; return true;
    }
    public static BigRat Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands, IFormatProvider? provider = null)
    {
      BigRat a = 0, e = 1, d = 0, p = 0; char c;
      for (int i = s.Length - 1; i >= 0; i--)
        switch (c = s[i])
        {
          case >= '0' and <= '9': a += e * (c - '0'); e *= 10; break;
          case '.' or ',': d = e; break;
          case '\'': a *= e / (e - 1); break;
          case '-': a = -a; break;
          case '+': break;
          case 'e' or 'E': p = Pow10((int)a); a = 0; e = 1; break;
        }
      if (d != 0) a /= d; if (p != 0) a *= p;
      return a;
    }

    public static bool IsInteger(BigRat value)
    {
      return Truncate(value) == value;
    }
    public static BigRat GetNumerator(BigRat value)
    {
      if (value.p == null) return default;
      var s = new ReadOnlySpan<uint>(value.p);
      var n = (int)(s[0] & 0x3fffffff); var a = new uint[n + 3];
      s.Slice(0, n + 1).CopyTo(a); a[n + 1] = a[n + 2] = 1; return new BigRat(a);
    }
    public static BigRat GetDenominator(BigRat value)
    {
      if (value.p == null) return 1;
      var s = new ReadOnlySpan<uint>(value.p);
      int n = (int)(s[0] & 0x3fffffff), m = (int)s[n + 1]; var a = new uint[m + 3];
      new ReadOnlySpan<uint>(value.p).Slice(n + 1, m + 1).CopyTo(a); a[m + 1] = a[m + 2] = 1;
      return new BigRat(a);
    }

    readonly uint[] p;

    BigRat(uint[] p) { this.p = p; }
    BigRat(uint* p, uint n) { this.p = new uint[n]; for (uint i = 0; i < n; i++) this.p[i] = p[i]; }

    static BigRat mul(uint* u, uint* v, uint* w)
    {
      uint nu = u[0] & 0x3fffffff, nv = v[0] & 0x3fffffff;
      uint na = mul(u + 1, nu, v + 1, nv, w + 1);
      uint nb = mul(u + nu + 2, u[nu + 1], v + nv + 2, v[nv + 1], w + 2 + na);
      w[0] = (u[0] ^ v[0]) & 0x80000000 | na; w[na + 1] = nb;
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
        mov(u, p, n + 1); u[0] &= 0x3fffffff; mod(u, p + n + 1, v); var inc = false;
        switch (f)
        {
          case 1: inc = (p[0] & 0x80000000) != 0 && *(ulong*)u != 1; break; // Floor 
          case 2: inc = (p[0] & 0x80000000) == 0 && *(ulong*)u != 1; break; // Ceiling
          case 3: // Round
            {
              var t = u + n + 1; mov(t, p + n + 1, p[n + 1] + 1); //for (uint i = 0; i <= p[n + 1]; i++) t[i] = p[n + 1 + i];
              shr(t, 1); var x = cms(u, t); if (x == 0 && (v[1] & 1) != 0) x = 1;
              inc = x > 0;
            }
            break;
            //case 8: // IsInt
            //  if (*(ulong*)u != 1) return default;
            //  break;
        }
        if (inc) { var w = 1u; v[0] = add(v + 1, v[0], &w, 1, v + 1); }
        if (*(ulong*)v == 1) return default;
        var l = v[0]; v[0] |= p[0] & 0x80000000; *(ulong*)(v + l + 1) = 0x100000001;
        return new BigRat(v, l + 3);
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
    static void mov(uint* a, uint* b, uint n)
    {
      for (uint i = 0; i < n; i++) a[i] = b[i];
    }
    static void mov(uint* a, int b)
    {
      a[0] = unchecked((uint)b & 0x80000000) | 1;
      a[1] = unchecked((uint)((b ^ (b >>= 31)) - b));
      ((ulong*)a)[1] = 0x100000001;
    }
    static uint mul(uint* a, uint na, uint* b, uint nb, uint* r)
    {
      //if ((na < nb ? na : nb) >= 32 && na + nb < 25000) { kmu(a, na, b, nb, r); if (r[(na = na + nb) - 1] == 0) na--; return na; }
      ulong c = 0, d;
      for (uint k = 0; k < na; k++, c = d >> 32)
        r[k] = unchecked((uint)(d = c + (ulong)a[k] * b[0]));
      r[na] = (uint)c;
      for (uint i = 1, k; i < nb; i++)
      {
        for (k = 0, c = 0; k < na; k++, c = d >> 32)
          r[i + k] = unchecked((uint)(d = r[i + k] + c + (ulong)a[k] * b[i]));
        r[i + na] = (uint)c;
      }
      if (r[(na = na + nb) - 1] == 0) na--;
      return na;
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
        if (b[1] == 0) return; // keep NaN
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
    static void shr(uint* p, int c)
    {
      int s = c & 31, r = 32 - s; uint n = p[0] & 0x3fffffff, i = 0, k = 1 + unchecked((uint)c >> 5), l = k <= n ? p[k++] >> s : 0;
      while (k <= n) { var t = p[k++]; p[++i] = l | unchecked((uint)((ulong)t << r)); l = t >> s; }
      if (l != 0 || i == 0) p[++i] = l; p[0] = i;
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

    static int tos(Span<char> s, BigRat v, bool reps, out int rep)
    {
      uint nv = unchecked((uint)v.p.Length), np = v.p[0] & 0x3fffffff, ten = 10, nr = 0; int ex = 0;
      if (reps) { nr = v.p[np + 1] + 1; ex = unchecked((int)nr) * s.Length; if (ex > 100000) { ex = 0; } }
      uint* p = stackalloc uint[v.p.Length + 8 + ex], d = p + np + 1 + 4;
      fixed (uint* t = v.p) { mov(p, t, np + 1); mov(d, t + np + 1, t[np + 1] + 1); p[0] &= 0x3fffffff; }
      uint* w = p + nv + 4, r = w + 4; rep = -1;
      for (int i = 0; i < s.Length; i++)
      {
        mod(p, d, w); var c = (char)('0' + w[1]);
        if (ex != 0 && c != '0')
        {
          for (int t = 0; t < i; t++)
          {
            if (s[t] != c || cms(p, r + t * nr) != 0) continue;
            for (; t != 0 && s[i - 1] == '0' && s[t - 1] == '0'; i--, t--) ;
            rep = t; return i;
          }
          mov(r + i * nr, p, p[0] + 1); Debug.Assert(p[0] < nr);
        }
        s[i] = c; if (*(ulong*)p == 1) return i + 1;
        p[0] = mul(p + 1, p[0], &ten, 1, p + 1);
      }
      return s.Length;
    }
    static int tos(Span<char> s, uint u, int m)
    {
      int i = 0; for (; u != 0 || i < m; u /= 10u) s[i++] = unchecked((char)('0' + u % 10));
      s.Slice(0, i).Reverse(); return i;
    }
  }
}