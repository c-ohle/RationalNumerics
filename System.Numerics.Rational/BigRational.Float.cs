
using System.Diagnostics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591  //xml comments
//85.504 Bytes -> 88.064 Bytes

namespace System.Numerics
{
  unsafe partial struct BigRational
  {
    /// <summary>
    /// Represents a sizeof(<typeparamref name="T"/>)-precision-floating floating-point number. (under construction)
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
      public override readonly string ToString()
      {
        return ((BigRational)this).ToString(
          sizeof(T) == 2 ? "L4" :
          sizeof(T) == 4 ? "L7" :
          sizeof(T) == 8 ? "L15" :
          sizeof(T) == 12 ? "L25" :
          sizeof(T) == 16 ? "L34" : "L71");
      }

      public static implicit operator Float<T>(int value)
      {
        var cpu = main_cpu; cpu.push(value);
        Float<T> c; ctor(cpu, 0, (uint*)&c); return c;
      }
      public static implicit operator Float<T>(double value)
      {
        if (sizeof(T) == 8) return *(Float<T>*)&value;
        var cpu = main_cpu; Float<double>.push(cpu, (uint*)&value); 
        var e = Float<double>.exp((uint*)&value);
        Float<T> c; ctor(cpu, e, (uint*)&c); return c;
      }
      public static explicit operator Float<T>(BigRational value)
      {
        var cpu = main_cpu; cpu.push(value); cpu.mod(8); var m = mbi;
        int s = cpu.sign(); if (cpu.sign() < 0) { cpu.neg(); cpu.neg(1); }
        int j, k, u = m - unchecked(j = (int)cpu.msb());
        cpu.shl(u); cpu.swp(); int v = (m << 1) - unchecked(k = (int)cpu.msb());
        cpu.shl(v); cpu.swp(); cpu.idiv();
        Float<T> c; ctor(cpu, k - j - m, (uint*)&c); return c;
      }

      public static explicit operator int(Float<T> value)
      {
        return (int)(double)value;
      }
      public static explicit operator double(Float<T> value)
      {
        if (sizeof(T) == 8) return *(double*)&value;
        var cpu = main_cpu; push(cpu, (uint*)&value); var e = exp((uint*)&value);
        Float<double> c; Float<double>.ctor(cpu, e, (uint*)&c); return *(double*)&c;
      }
      public static implicit operator BigRational(Float<T> value)
      {
        var cpu = main_cpu; var p = (uint*)&value.p;
        push(cpu, p); cpu.pow(2, exp(p)); cpu.mul(); return cpu.popr();
      }

      public static Float<T> operator +(Float<T> a) => a;
      public static Float<T> operator -(Float<T> a)
      {
        uint* u = (uint*)&a;
        if ((sizeof(T) & 2) == 0) u[lui] ^= 0x80000000; else u[lui] ^= 0x8000;
        //if (u[lui] != 0) u[lui] ^= 0x80000000; 
        return a;
      }
      public static Float<T> operator +(Float<T> a, Float<T> b)
      {
        uint* u = (uint*)&a, v = (uint*)&b;
        Float<T> c; add(u, v, (uint*)&c); return c;
      }
      public static Float<T> operator -(Float<T> a, Float<T> b)
      {
        uint* u = (uint*)&a, v = (uint*)&b;
        if ((sizeof(T) & 2) == 0) v[lui] ^= 0x80000000; else v[lui] ^= 0x8000;
        Float<T> c; add(u, v, (uint*)&c); return c;
      }
      public static Float<T> operator *(Float<T> a, Float<T> b)
      {
        var cpu = main_cpu; uint* u = (uint*)&a, v = (uint*)&b;
        push(cpu, u); push(cpu, v); cpu.mul();
        Float<T> c; ctor(cpu, exp(u) + exp(v), (uint*)&c); return c;
      }
      public static Float<T> operator /(Float<T> a, Float<T> b)
      {
        var cpu = main_cpu; uint* u = (uint*)&a, v = (uint*)&b; var m = mbi;
        push(cpu, u); cpu.shl(m); push(cpu, v); cpu.idiv();
        Float<T> c; ctor(cpu, exp(u) - exp(v) - m, (uint*)&c); return c;
      }
      public static Float<T> operator %(Float<T> a, Float<T> b)
      {
        return a - Truncate(a / b) * b;
      }

      public static Float<T> Truncate(Float<T> a)
      {
        var cpu = main_cpu; uint* u = (uint*)&a;
        push(cpu, u); var e = exp(u); cpu.shr(-e);
        Float<T> c; ctor(cpu, 0, (uint*)&c); return c;
      }

      //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private readonly T p;
      private static int exp(uint* p)
      {
        return unchecked((int)((p[lui] >> esh) & msk)) - bia;
      }
      private static void push(CPU cpu, uint* p)
      {
        var l = lui; if (((sizeof(T) & 2) == 0 ? p[l] : (p[l] & 0xffff)) == 0) { cpu.push(); return; }
        var t = stackalloc uint[l + 4]; var x = 1u << (mbi - 1);
        if ((sizeof(T) & 2) != 0) if (l != 0) p[l] <<= 16; else p[0] = (p[0] & 0x03FFu) | ((p[0] & ~0x03FFu) << 16);
        t[0] = (p[l] & 0x80000000) | unchecked((uint)l + 1);
        t[l + 1] = (p[l] & (x - 1)) | x; t[l + 2] = t[l + 3] = 1;
        for (int i = 0; i < l; i++) t[i + 1] = p[i];
        cpu.push(new ReadOnlySpan<uint>(t, l + 4)); //return unchecked((int)((p[lui] >> esh) & msk)) - bia;
      }
      private static void ctor(CPU cpu, int e, uint* p)
      {
        var s = unchecked((int)cpu.msb()); if (s == 0) { cpu.pop(); *(Float<T>*)p = default; return; }
        var d = s - mbi; cpu.shr(d); cpu.get(cpu.mark() - 1, out ReadOnlySpan<uint> sp);
        var l = lui; var bi = unchecked((int)mbi - 1);
        var f = (unchecked((uint)((e + d) + bia)) << esh) | (sp[l + 1] & ((1u << bi) - 1));
        p[l] = (sp[0] & 0x80000000) | f;
        if ((sizeof(T) & 2) != 0) { if (l != 0) p[l] >>= 16; else p[l] = (p[l] & 0xffff) | p[l] >> 16; }
        for (int i = 0; i < l; i++) p[i] = sp[i + 1];
        cpu.pop();
      }
      private static void add(uint* u, uint* v, uint* c)
      {
        var cpu = main_cpu; push(cpu, u); push(cpu, v);
        int ae = exp(u), be = exp(v), e, l = ae < be ? 1 : 0;
        cpu.shr(l == 0 ? (e = ae) - be : (e = be) - ae, l);
        cpu.add(); ctor(cpu, e, c);
      }
      //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private static readonly int sbi, mbi, bia, esh, msk, lui;
      static Float()
      {
        switch (sizeof(T))
        {
          case 02: sbi = 05; break; // Half 16 bit
          case 04: sbi = 08; break; // Single 32 bit
          case 08: sbi = 11; break; // Double 64 bit
          case 10: sbi = 15; break; // Extended 80 bit
          case 12: sbi = 15; break; // Extended 96 bit
          case 16: sbi = 15; break; // Quadruple 128 bit
          case 32: sbi = 19; break; // Octuple 256
          default: throw new NotSupportedException($"{typeof(T)} {sizeof(T) << 3}"); //todo: f() intermediate sizes
        }
        msk = (1 << sbi) - 1;
        esh = 32 - sbi - 1; // (sizeof(T) != 2 ? 32 : 16) - sbi - 1;
        mbi = (sizeof(T) << 3) - sbi;
        bia = (1 << (sbi - 1)) + mbi - 2;
        lui = (sizeof(T) >> 2) - 1;
        if ((sizeof(T) & 2) != 0) { lui++; }
        if ((sizeof(T)) == 2)
        {

        }
      }

      internal static int BiasedExponentShift => mbi - 1; // dbl 52
      internal static int MinBiasedExponent => 0;
      internal static int MaxBiasedExponent => msk; // dbl 0x07FF;
      internal static int ExponentBias => (1 << (sbi - 1)) - 1; // dbl 1023
      internal static int MinExponent => 1 - ExponentBias; // dbl -1022;
      internal static int MaxExponent => ExponentBias; // dbl +1023;
      internal static int Bits => sizeof(T) << 3;

      public static Float<T> MinValue { get { return 0; } }
      public static Float<T> MaxValue { get { return 0; } }

    }

  }
}
