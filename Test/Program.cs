global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;
using System.Runtime.InteropServices;

namespace Test
{
  internal unsafe static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); //test();
      Application.Run(new MainFrame());
    }

#if false
#if NET7_0
    static void test()
    {
      rat a, b, c; double d; bool o; decimal m;

      var x = (Int128)123.456;
      x = (Int128)2;
      x = (Int128)4;

      a = (Int128)(-1);        /**/ b = (Int128)a; Debug.Assert(a == b);
      a = (Int128)(+1);        /**/ b = (Int128)a; Debug.Assert(a == b);

      a = (Int128)(-123);        /**/ b = (Int128)a; Debug.Assert(a == b);
      a = (Int128)(+123);        /**/ b = (Int128)a; Debug.Assert(a == b);
      a = -(Int128)0xffffffffffffffff << 8; /**/ b = (Int128)a; Debug.Assert(a == b);
      a = +(Int128)0xffffffffffffffff << 8; /**/ b = (Int128)a; Debug.Assert(a == b);
      a = Int128.MaxValue; /**/ b = (Int128)a; Debug.Assert(a == b);
      a = Int128.MinValue; /**/ b = (Int128)a; Debug.Assert(a == b);

      a = UInt128.MaxValue;     /**/ b = (UInt128)a; Debug.Assert(a == b);
      a = UInt128.MinValue;     /**/ b = (UInt128)a; Debug.Assert(a == b);
      a = Math.PI * +1000;      /**/ b = (Int128)a; Debug.Assert(a != b);
      a = Math.PI * -1000;      /**/ b = (Int128)a; Debug.Assert(a != b);

      var u = (UInt128)(-123);
      u = (UInt128)(double)(-123); //0
      u = (UInt128)(long)(-123);
      //u = (UInt128)(decimal)(-123); //exception
      a = (UInt128)(-123);

      a = (byte)+123;       /**/ b = (byte)a; Debug.Assert(a == b);
      a = (sbyte)-123;      /**/ b = (sbyte)a; Debug.Assert(a == b);
      a = (ushort)+123;     /**/ b = (ushort)a; Debug.Assert(a == b);
      m = 'c';
      d = 'c';
      a = 'c';              /**/ b = (char)a; Debug.Assert(a == b);
      a = (short)-123;      /**/ b = (short)a; Debug.Assert(a == b);
      a = (uint)+123;       /**/ b = (uint)a; Debug.Assert(a == b);
      a = (int)-123;        /**/ b = (int)a; Debug.Assert(a == b);
      a = (ulong)+123;      /**/ b = (ulong)a; Debug.Assert(a == b);
      a = (long)-123;       /**/ b = (long)a; Debug.Assert(a == b);
      a = (nuint)(+123);    /**/ b = (nuint)a; Debug.Assert(a == b);
      a = (nint)(-123);     /**/ b = (nint)a; Debug.Assert(a == b);
      a = (UInt128)123;     /**/ b = (UInt128)a; Debug.Assert(a == b);
      a = (Int128)(-123);   /**/ b = (Int128)a; Debug.Assert(a == b);
      a = (Half)(+123.4);   /**/ b = (Half)a; Debug.Assert(a == b);
      a = (Half)(-123.4);   /**/ b = (Half)a; Debug.Assert(a == b);
      a = (NFloat)123.4;    /**/ b = (NFloat)a; Debug.Assert(a == b);
      a = (NFloat)(-123.4); /**/ b = (NFloat)a; Debug.Assert(a == b);

      a = Math.PI;    /**/ a++; a--;
      d = (double)a;  /**/ d++; d--;

      o = d.Equals(null);     /**/ o = a.Equals(null);
      c = d.CompareTo(null);  /**/ c = a.CompareTo(null);

      m = (decimal)d;

      d = double.MaxMagnitude(-2, 1.5);        /**/ c = rat.MaxMagnitude(-2, 1.5);
      d = double.MaxMagnitudeNumber(-2, +1.5); /**/ c = rat.MaxMagnitudeNumber(-2, +1.5);
      d = double.MaxMagnitudeNumber(-2, -1.5); /**/ c = rat.MaxMagnitudeNumber(-2, -1.5);
      d = double.MaxMagnitudeNumber(+2, +1.5); /**/ c = rat.MaxMagnitudeNumber(+2, +1.5);
      d = double.MaxMagnitudeNumber(+2, -1.5); /**/ c = rat.MaxMagnitudeNumber(+2, -1.5);
      d = double.MinMagnitude(-2, 1.5);        /**/ c = rat.MinMagnitude(-2, 1.5);
      d = double.MinMagnitudeNumber(-2, +1.5); /**/ c = rat.MinMagnitudeNumber(-2, +1.5);
      d = double.MinMagnitudeNumber(-2, -1.5); /**/ c = rat.MinMagnitudeNumber(-2, -1.5);
      d = double.MinMagnitudeNumber(+2, +1.5); /**/ c = rat.MinMagnitudeNumber(+2, +1.5);
      d = double.MinMagnitudeNumber(+2, -1.5); /**/ c = rat.MinMagnitudeNumber(+2, -1.5);


      o = double.IsInteger(1);  /**/ o = rat.IsInteger(1);
      o = double.IsInteger(-1); /**/ o = rat.IsInteger(-1);
      o = double.IsInteger(0);  /**/ o = rat.IsInteger(0);

      o = double.IsEvenInteger(1);  /**/ o = rat.IsEvenInteger(1);
      o = double.IsEvenInteger(-1); /**/ o = rat.IsEvenInteger(-1);
      o = double.IsEvenInteger(0);  /**/ o = rat.IsEvenInteger(0);
      o = double.IsEvenInteger(2);  /**/ o = rat.IsEvenInteger(2);
      o = double.IsEvenInteger(-2); /**/ o = rat.IsEvenInteger(-2);

      o = double.IsOddInteger(1);   /**/ o = rat.IsOddInteger(1);
      o = double.IsOddInteger(-1);  /**/ o = rat.IsOddInteger(-1);
      o = double.IsOddInteger(0);   /**/ o = rat.IsOddInteger(0);
      o = double.IsOddInteger(2);   /**/ o = rat.IsOddInteger(2);
      o = double.IsOddInteger(-2);  /**/ o = rat.IsOddInteger(-2);

      o = ((object)123m).Equals((object)(123m));
      o = ((object)123m).Equals(null);
      o = ((object)0m).Equals(null);
      a = (123m).CompareTo((object)(123m));
      a = (123m).CompareTo((object)(-123m));
      a = (123m).CompareTo(null);
      a = (-123m).CompareTo(null);
  }
#endif

    static void test()
    {
      rat a, b, c;
      a = +(rat)1 / 3; c = MathR.GetNumerator(a, out b);
      a = -(rat)1 / 3; c = MathR.GetNumerator(a, out b);
      a = 0; c = MathR.GetNumerator(a, out b);

      a = MathR.Sin(1);
      MathR.DefaultDigits = 10;
      b = MathR.Sin(1);
      MathR.DefaultDigits = 100;
      b = MathR.Sin(1);
      MathR.DefaultDigits = 10;
    }
    
    private static AsyncLocal<rat.CPU>? _async_cpu;
    public static rat.CPU async_cpu
    {
      get
      {
        var p = _async_cpu ??= new AsyncLocal<rat.CPU>();
        return p.Value ?? (p.Value = new rat.CPU());
      }
    }
    static void test()
    {
      rat a, b, c, d;
      d = 100 / 13;
      b = MathR.IDiv(+100, +13);
      d = -100 / +13;
      b = MathR.IDiv(-100, +13);
      d = -100 / -13;
      b = MathR.IDiv(-100, -13);
      d = +100 / -13;
      b = MathR.IDiv(+100, -13);
 
      //var apu = async_cpu;
      //
      //var t1 = Environment.TickCount;
      //for (int i = 0; i < 10_000_000; i++) { var t = rat.task_cpu; }
      //var t2 = Environment.TickCount; // t2 - t1 -> 78 ms
      //for (int i = 0; i < 10_000_000; i++) { var t = async_cpu; }
      //var t3 = Environment.TickCount; // t3 - t2 -> 156 ms
    }
    static void test1()
    {
      var cpu = rat.task_cpu;

      var a = new BigRational[1000];
      Parallel.For(0, 1000, i => a[i] = MathR.Sin(i * 0.123m, digits: 100));

      var b = new BigRational[1000];
      Parallel.For(0, 1000, i =>
      {
        var cpu = rat.task_cpu;
        cpu.push(i);
        cpu.push(0.123m);
        cpu.mul();
        cpu.sin(80, cos: false); //80: ILog10(digits);  
        cpu.rnd(24); //digits
        b[i] = cpu.popr();
      });

      var mycpu = new BigRational.CPU();
      var task = Task.Run(() =>
      {
        mycpu.push(1);
        mycpu.push(2);
        mycpu.push(3);
      });
      task.Wait();
      var x = mycpu.popr();
      var y = mycpu.popr();
      var z = mycpu.popr();

      mycpu.get(0, out ReadOnlySpan<uint> sp);
      cpu.push(sp);
      var d = cpu.popd();

      static IEnumerable<BigRational> getpows10forever()
      {
        var mycpu = new rat.CPU();
        mycpu.push(1);
        for (int i = 0; ; i++)
        {
          if (i != 0) { mycpu.push(10); mycpu.mul(); }
          mycpu.get(0, out rat r); yield return r;
        }
      }
      var c = getpows10forever().Take(100).ToArray();

    }

    static void test()
    {
      VectorR a, b, c; string s; rat x, y;

      x = MathRE.IntegerDivide(+10, 3);
      x = MathRE.IntegerDivide(-10, 3);
      y = MathRE.IntegerModula(10, 3);
      y = MathRE.IntegerModula(-10, 3);

      //"2.7182818284590452353602874713526624977572470936999595749669676277"
      var e = MathR.Exp(1);
      s = MathRE.GetContinuedFraction(e);
      e = (rat)"2.7182818284590452353602874713526624977572470936999595749669676277";
      s = e.ToString("L100");
      s = MathRE.GetContinuedFraction(e);
      y = MathRE.ParseContinuedFraction(s);
      if (y != e) { }

      s = string.Concat(s.Select(c => c == ';' || c == ',' ? ' ' : c));
      c = VectorR.Parse(s);

      x = (rat)"0.00000000000007686868786";
      s = MathRE.GetContinuedFraction(x);
      y = MathRE.ParseContinuedFraction(s);
      
      c = VectorR.Parse(string.Concat(s.Select(c => c == ';' || c == ',' ? ' ' : c)));

      a = VectorR.Create(1, -2, MathR.Sin(Math.PI / 4), Math.PI);
      var h = a.GetHashCode();
      b = a * 2; if (a == b) { }
      b = a * 0.5f; if (a == b) { }
      b = b * 2; if (a == b) { }

      s = b.ToString();
      a = VectorR.Parse(s);

      a = VectorR.Create(1, 2787868.89979997, 909008809098.090890809m);
      b = a * a;
      a = VectorR.Create(-1, 33, -7.5, MathF.PI);
      b = VectorR.Create(88, -33, -7.8, -MathF.PI * 2);
      c = VectorR.Max(a, b);
      c = VectorR.Min(a, b);

      var cpu = rat.task_cpu;
      cpu.push(Math.PI); cpu.push(0); cpu.push(Math.Tau); cpu.push(Math.E);
      a = VectorR.Create(cpu, 4); cpu.pop(4);
      for (int i = 0; i < a.Length; i++) { var t = a[i]; }
    }
#endif
#if false
    static void test()
    {
      rat b; Vector2R a; string s;
      a = Vector2R.SinCosR(Math.PI + 1); s = a.ToString();
      a = Vector2R.SinCosR(Math.PI); s = a.ToString();
      a = Vector2R.SinCosR(Math.PI / 2); s = a.ToString();
      b = Vector2R.LengthR(a); var c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI / 4); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI / 4, 20); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI / 4, 35); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI / 4, 52); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(-Math.PI / 2); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(+Math.PI / 2); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(2 * Math.PI); s = a.ToString();
      b = Vector2R.LengthR(a);
    }
#endif
  }
}