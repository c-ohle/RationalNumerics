
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
    [Serializable, SkipLocalsInit] 
    public unsafe readonly struct Float<T> where T : unmanaged
    {
      public override readonly string ToString()
      {
        return ((BigRational)this).ToString(sizeof(T) == 2 ? "L4" : sizeof(T) == 4 ? "L7" : sizeof(T) == 8 ? "L15" : sizeof(T) == 16 ? "L34" : "L71");
      }

      public static implicit operator Float<T>(int value)
      {
        var cpu = main_cpu; cpu.push(value);
        Float<T> c; ctor(cpu, 0, (uint*)&c); return c;
      }
      public static implicit operator Float<T>(double value)
      {
        if (sizeof(T) == sizeof(double)) return *(Float<T>*)&value;
        var cpu = main_cpu; var e = Float<double>.exp((uint*)&value);
        Float<double>.push(cpu, (uint*)&value);
        Float<T> c; ctor(cpu, e, (uint*)&c); return c;
      }
      public static implicit operator BigRational(Float<T> value)
      {
        var cpu = main_cpu; var p = (uint*)&value.p;
        push(cpu, p); cpu.pow(2, exp(p)); cpu.mul(); return cpu.popr();
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
                                       
      public static Float<T> operator +(Float<T> a) => a;
      public static Float<T> operator -(Float<T> a)
      {
        ushort* u = (ushort*)&a, h = &u[(sizeof(T) >> 1) - 1];
        if (*h != 0) *h ^= 0x8000; return a;
      }
      public static Float<T> operator +(Float<T> a, Float<T> b)
      {
        uint* u = (uint*)&a, v = (uint*)&b;
        Float<T> c; add(u, v, (uint*)&c); return c;
      }
      public static Float<T> operator -(Float<T> a, Float<T> b)
      {
        ushort* u = (ushort*)&a, v = (ushort*)&b, h = &v[(sizeof(T) >> 1) - 1];
        Float<T> c; *h ^= 0x8000; add((uint*)u, (uint*)v, (uint*)&c); return c;
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

      private readonly T p;
      private static int exp(uint* p)
      {
        return unchecked((int)((p[sizeof(T) != 2 ? (sizeof(T) >> 2) - 1 : 0] >> esh) & msk)) - bia;
      }
      private static void push(CPU cpu, uint* p)
      {
        var l = sizeof(T) != 2 ? (sizeof(T) >> 2) - 1 : 0;
        if ((sizeof(T) != 2 ? p[l] : (p[l] & 0xffff)) == 0) { cpu.push(); return; }
        var t = stackalloc uint[l + 4]; var x = 1u << (mbi - 1);
        t[0] = (sizeof(T) != 2 ? p[l] & 0x80000000 : (p[l] & 0x8000) << 16) | unchecked((uint)l + 1);
        t[l + 1] = (p[l] & (x - 1)) | x; t[l + 2] = t[l + 3] = 1;
        for (int i = 0; i < l; i++) t[i + 1] = p[i];
        cpu.push(new ReadOnlySpan<uint>(t, l + 4));
      }
      private static void ctor(CPU cpu, int e, uint* p)
      {
        var s = unchecked((int)cpu.msb()); if (s == 0) { cpu.pop(); *(Float<T>*)p = default; return; }
        var d = s - mbi; cpu.shr(d); cpu.get(cpu.mark() - 1, out ReadOnlySpan<uint> sp);
        var l = sizeof(T) != 2 ? (sizeof(T) >> 2) - 1 : 0; var bi = unchecked((int)mbi - 1);
        var f = (unchecked((uint)((e + d) + bia)) << esh) | (sp[l + 1] & ((1u << bi) - 1));
        if (sizeof(T) != 2) p[l] = (sp[0] & 0x80000000) | f;
        else ((ushort*)p)[0] = unchecked((ushort)(((sp[0] & 0x80000000) >> 16) | f));
        for (int i = 0; i < l; i++) p[i] = sp[i + 1];
        cpu.pop();
      }
      private static void add(uint* u, uint* v, uint* c)
      {
        int ae = exp(u), be = exp(v), e, l = ae < be ? 1 : 0;
        var cpu = main_cpu; push(cpu, u); push(cpu, v);
        cpu.shr(l == 0 ? (e = ae) - be : (e = be) - ae, l);
        cpu.add(); ctor(cpu, e, c);
      }
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private static readonly int mbi, bia, esh, msk;
      static Float()
      {
        switch (sizeof(T))
        {
          case 02: mbi = 011; esh = 10; msk = 0x01f; bia = 0025; break;
          case 04: mbi = 024; esh = 23; msk = 0x0ff; bia = 0150; break;
          case 08: mbi = 053; esh = 20; msk = 0x7ff; bia = 1075; break;
          //case 10: mbi = 113; esh = 20; msk = 0x7ff; bia = 1075; break; //todo: 80 bit extended 
          case 16: mbi = 113; esh = 20; msk = 0x7ff; bia = 1075; break; //todo: check
          case 32: mbi = 237; esh = 20; msk = 0x7ff; bia = 1075; break; //todo: check
          default: throw new NotSupportedException($"{typeof(T)} {sizeof(T) << 3}"); //todo: intermediate sizes
        }
      }
    }

  }
}
