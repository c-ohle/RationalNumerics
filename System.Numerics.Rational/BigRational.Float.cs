
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static System.Numerics.BigRational;

#pragma warning disable CS1591  //xml comments
//Float<T> support assembly size 85.504 Bytes -> 88.064 Bytes

namespace System.Numerics
{
  unsafe partial struct BigRational
  {
    /// <summary>
    /// Represents a sizeof&lt;<typeparamref name="T"/>&gt;-precision-floating floating-point number. (under construction)
    /// </summary>
    /// <remarks>
    /// Template for <seealso href="https://en.wikipedia.org/wiki/IEEE_754">IEEE 754</seealso> floating-point numbers.<br/>
    /// Using struct <typeparamref name="T"/> as data storage and the <see cref="CPU"/> as calculation core.<br/>
    /// The sizeof(<typeparamref name="T"/>) defines the type type and precision. For example:<br/>
    /// <c>BigRational.Float&lt;Int64&gt;</c> defines a Double precision type.<br/>
    /// <c>BigRational.Float&lt;Int128&gt;</c> defines a Quadruple precision type.<br/>
    /// <c>BigRational.Float&lt;Int256&gt;</c> defines a Octuple precision type.<br/>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(),nq}")]
    public unsafe readonly struct Float<T> where T : unmanaged
    {
      public override readonly string ToString() //todo: inline
      {
        return ((BigRational)this).ToString(
          sizeof(T) == 2 ? "L4" :
          sizeof(T) == 4 ? "L7" :
          sizeof(T) == 8 ? "L15" :
          sizeof(T) == 12 ? "L25" :
          sizeof(T) == 16 ? "L34" : "L71");
      }
      public override readonly int GetHashCode()
      {
        return p.GetHashCode();
      }
      public override readonly bool Equals([NotNullWhen(true)] object? obj)
      {
        return obj is Float<T> t ? this == t : false;
      }

      public static implicit operator Float<T>(int value)
      {
        //return (Float<T>)(double)value;
        var cpu = main_cpu; cpu.push(value);
        Float<T> c; pop(cpu, 0, (uint*)&c); return c;
      }
      public static implicit operator Float<T>(Half value)
      {
        //if (sizeof(T) == 2) return *(Float<T>*)&value;
        var cpu = main_cpu; var e = Float<Half>.push(cpu, (uint*)&value);
        Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static implicit operator Float<T>(float value)
      {
        //if (sizeof(T) == 4) return *(Float<T>*)&value;
        var cpu = main_cpu; var e = Float<float>.push(cpu, (uint*)&value);
        Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static implicit operator Float<T>(double value)
      {
        //if (sizeof(T) == 8) return *(Float<T>*)&value;
        var cpu = main_cpu; var e = Float<double>.push(cpu, (uint*)&value);
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
        //if (sizeof(T) == 8) return *(double*)&value;
        var cpu = main_cpu; var e = push(cpu, (uint*)&value);
        Float<double> c; Float<double>.pop(cpu, e, (uint*)&c); return *(double*)&c;
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
        return a + 1; //todo: opt. inline
      }
      public static Float<T> operator --(Float<T> a)
      {
        return a - 1; //todo: opt. inline
      }
      public static Float<T> operator +(Float<T> a, Float<T> b)
      {
        var cpu = main_cpu; int ae = push(cpu, (uint*)&a), be = push(cpu, (uint*)&b), l = ae < be ? 1 : 0, e;
        cpu.shr(l == 0 ? (e = ae) - be : (e = be) - ae, l); cpu.add();
        Float<T> c; pop(cpu, e, (uint*)&c); return c;
      }
      public static Float<T> operator -(Float<T> a, Float<T> b)
      {
        var cpu = main_cpu; int ae = push(cpu, (uint*)&a), be = push(cpu, (uint*)&b), l = ae < be ? 1 : 0, e;
        cpu.shr(l == 0 ? (e = ae) - be : (e = be) - ae, l); cpu.sub();
        Float<T> c; pop(cpu, e, (uint*)&c); return c;

        //uint* u = (uint*)&a, v = (uint*)&b;
        //if ((sizeof(T) & 2) == 0) v[lui] ^= 0x80000000; else v[lui] ^= 0x8000;
        //Float<T> c; add(u, v, (uint*)&c); return c;
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

      public static Float<T> Truncate(Float<T> a)
      {
        var cpu = main_cpu; var e = push(cpu, (uint*)&a); cpu.shl(e);
        Float<T> c; pop(cpu, 0, (uint*)&c); return c;
      }
      public static Float<B> Cast<B>(T a) where B : unmanaged
      {
        var cpu = main_cpu; var e = push(cpu, (uint*)&a);
        Float<B> c; Float<B>.pop(cpu, e, (uint*)&c); return c;
      }

      public static bool Equals(Float<T> a, Float<T> b)
      {
        return Equals((uint*)&a, (uint*)&b);
      }
      public static int Compare(Float<T> a, Float<T> b)
      {
        return Compare((uint*)&a, (uint*)&b);
      }

      #region private
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private readonly T p;
      private static int push(CPU cpu, uint* p)
      {
        var l = lui; //if (((sizeof(T) & 2) == 0 ? p[l] : (p[l] & 0xffff)) == 0) { cpu.push(); return; }
        if ((sizeof(T) & 2) != 0) if (l != 0) p[l] <<= 16; else p[0] = (p[0] & 0x03FFu) | ((p[0] & ~0x03FFu) << 16);
        if (p[l] == 0) { cpu.push(); return 0; }
        var t = stackalloc uint[l + 4]; var x = 1u << (mbi - 1);
        t[0] = (p[l] & 0x80000000) | unchecked((uint)l + 1);
        t[l + 1] = (p[l] & (x - 1)) | x; t[l + 2] = t[l + 3] = 1;
        for (int i = 0; i < l; i++) t[i + 1] = p[i];
        cpu.push(new ReadOnlySpan<uint>(t, l + 4));
        return unchecked((int)((p[l] >> esh) & msk)) - bia;
      }
      private static void pop(CPU cpu, int e, uint* p)
      {
        int s = unchecked((int)cpu.msb()); if (s == 0) { cpu.pop(); *(Float<T>*)p = default; return; }
        int d = s - mbi, l; cpu.shr(d); cpu.get(cpu.mark() - 1, out ReadOnlySpan<uint> sp);
        p[l = lui] = (sp[0] & 0x80000000) | (unchecked((uint)((e + d) + bia)) << esh) | (sp[l + 1] & ((1u << (mbi - 1)) - 1u));
        if ((sizeof(T) & 2) != 0) if (l != 0) p[l] >>= 16; else p[l] = (p[l] & 0xffff) | p[l] >> 16;
        for (int i = 0; i < l; i++) p[i] = sp[i + 1]; cpu.pop();
      }
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private static readonly int sbi, mbi, bia, esh, msk, lui;
      static Float()
      {
        if ((sizeof(T) & 1) != 0) throw new NotSupportedException(nameof(T));
        sbi = BitOperations.Log2(unchecked((uint)sizeof(T)));
        switch (sizeof(T))
        {
          case 02: Debug.Assert(sbi == 1); sbi = 05; break; // Half 16 bit        1
          case 04: Debug.Assert(sbi == 2); sbi = 08; break; // Single 32 bit      2
          case 08: Debug.Assert(sbi == 3); sbi = 11; break; // Double 64 bit      3
          case 10: Debug.Assert(sbi == 3); sbi = 15; break; // Extended 80 bit    3
          case 12: Debug.Assert(sbi == 3); sbi = 15; break; // Extended 96 bit    3
          case 16: Debug.Assert(sbi == 4); sbi = 15; break; // Quadruple 128 bit  4
          case 32: Debug.Assert(sbi == 5); sbi = 19; break; // Octuple 256        5
          default: throw new NotSupportedException($"{typeof(T)} {sizeof(T) << 3}"); //todo: f() intermediate sizes
        }
        msk = (1 << sbi) - 1;
        esh = 32 - sbi - 1;
        mbi = (sizeof(T) << 3) - sbi;
        bia = (1 << (sbi - 1)) + mbi - 2;
        lui = (sizeof(T) >> 2) - 1;
        if ((sizeof(T) & 2) != 0) lui++;

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
        if(sa != sb) return Math.Sign(sa - sb); if (sa == 0) return 0;
        var cpu = main_cpu; int ea = push(cpu, a), eb = push(cpu, b);
        if (ea != eb) { cpu.pop(2); return ea > eb ? sa : -sa; }
        ea = cpu.cmp(); cpu.pop(2); return ea * sa;
      }
      #endregion
      //private static int exp(uint* p) => return unchecked((int)((p[lui] >> esh) & msk)) - bia;
      //internal static int BiasedExponentShift => mbi - 1; // dbl 52
      //internal static int MinBiasedExponent => 0;
      //internal static int MaxBiasedExponent => msk; // dbl 0x07FF;
      //internal static int ExponentBias => (1 << (sbi - 1)) - 1; // dbl 1023
      //internal static int MinExponent => 1 - ExponentBias; // dbl -1022;
      //internal static int MaxExponent => ExponentBias; // dbl +1023;
      //internal static int Bits => sizeof(T) << 3;
      //public static Float<T> MinValue { get { return 0; } }
      //public static Float<T> MaxValue { get { return 0; } }

    }

  }
}
