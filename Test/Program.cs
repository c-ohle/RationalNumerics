global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;
global using Builder = System.Numerics.BigRational.Builder;

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
             
#if false //NET7_0
    //todo: cachex -1, pi
    //todo: nuget multipack...
    //todo: decimal checked...

    static rat gamma1(rat x)
    {
      return x * x + x * x - x;
    }
    static rat gamma2(rat x)
    {
      return (Builder)x * x + x * x - x;
    }
    static Builder gamma3(Builder x)
    {
      return x * x + x * x - x;
    }
     
    static void test()
    {
      TestBigIntegerBuilder.TestType4();
     
      TestBigIntegerBuilder.TestBuilder();

      rat a, b, c, x; double d;
                  
      b = Math.PI; c = Math.E; //b = rat.Pi(100);

      a = b * c + c / b;
      a = (Builder)b * c + c / b;
      
      a = b * c + c / b + gamma1(1.5) + 1;
      a = (Builder)b * c + c / b + gamma1(1.5) + 1;
      a = (Builder)b * c + c / b + gamma3(1.5) + 1;
      a = (Builder)b * c + c / b + gamma2(1.5) + 1;

      x = (Builder)a * b - BigRational.Truncate(c / 3);

      a = b * c + c / b + BigRational.Sinh(0.1) + 1;
      a = (Builder)b * c + c / b + BigRational.Sinh(0.1) + 1;

      var t1 = (int)b; t1 = (int)(-b); a = int.MaxValue; t1 = (int)a; a++; t1 = (int)a; a = int.MinValue; t1 = (int)a; a--; t1 = (int)a;
      var t2 = (uint)b; t2 = (uint)(-b); a = uint.MaxValue; t2 = (uint)a; a++; t2 = (uint)a; a = uint.MinValue; t2 = (uint)a;
      var t3 = (long)b; t3 = (long)(-b); a = long.MaxValue; t3 = (long)a; a++; t3 = (long)a; a = long.MinValue; t3 = (long)a; a--; t3 = (long)a;
      var t4 = (ulong)b; t4 = (ulong)(-b); a = ulong.MaxValue; t4 = (ulong)a; a++; t4 = (ulong)a; a = ulong.MinValue; t4 = (ulong)a;
      var t5 = (Int128)b; t5 = (Int128)(-b); a = Int128.MaxValue; t5 = (Int128)a; a++; t5 = (Int128)a; a = Int128.MinValue; t5 = (Int128)a; a--; t5 = (Int128)a;
      var t6 = (UInt128)b; t6 = (UInt128)(-b); a = UInt128.MaxValue; t6 = (UInt128)a; a++; t6 = (UInt128)a; a = UInt128.MinValue; t6 = (UInt128)a;

      //checked
      //{
      //  t1 = (int)b; t1 = (int)(-b); a = int.MaxValue; t1 = (int)a; a++; t1 = (int)a; a = int.MinValue; t1 = (int)a; a--; t1 = (int)a;
      //  t2 = (uint)b; t2 = (uint)(-b); a = uint.MaxValue; t2 = (uint)a; a++; t2 = (uint)a; a = uint.MinValue; t2 = (uint)a;
      //  t3 = (long)b; t3 = (long)(-b); a = long.MaxValue; t3 = (long)a; a++; t3 = (long)a; a = long.MinValue; t3 = (long)a; a--; t3 = (long)a;
      //  t4 = (ulong)b; t4 = (ulong)(-b); a = ulong.MaxValue; t4 = (ulong)a; a++; t4 = (ulong)a; a = ulong.MinValue; t4 = (ulong)a;
      //  t5 = (Int128)b; t5 = (Int128)(-b); a = Int128.MaxValue; t5 = (Int128)a; a++; t5 = (Int128)a; a = Int128.MinValue; t5 = (Int128)a; a--; t5 = (Int128)a;
      //  t6 = (UInt128)b; t6 = (UInt128)(-b); a = UInt128.MaxValue; t6 = (UInt128)a; a++; t6 = (UInt128)a; a = UInt128.MinValue; t6 = (UInt128)a;
      //}

      //INumber
      a = rat.Clamp(0.5, -1, 0.2); b = d = double.Clamp(0.5, -1, 0.2);
      a = rat.Clamp(-0.2, -1, 0.2); b = d = double.Clamp(-0.2, -1, 0.2);
      a = rat.Clamp(-4.2, -1, 0.2); b = d = double.Clamp(-4.2, -1, 0.2);
      a = rat.CopySign(-0.5, -1); b = d = double.CopySign(-0.5, -1);
      a = rat.CopySign(+0.5, -1); b = d = double.CopySign(+0.5, -1);
      a = rat.CopySign(0, -1); b = d = double.CopySign(0, -1);
      a = rat.CopySign(0, +1); b = d = double.CopySign(0, +1);
      a = rat.CopySign(-1, 0); b = d = double.CopySign(-1, 0);
      a = rat.CopySign(+1, 0); b = d = double.CopySign(+1, 0);

      //IPowerFunctions
      a = rat.Pow(0.5, 4d); b = d = double.Pow(0.5, 4d);
      a = rat.Pow(0.5, 1.23456); b = d = double.Pow(0.5, 1.23456);
      a = rat.Pow(0.5, -1.23); b = d = double.Pow(0.5, -1.23);

      //IRootFunctions
      a = rat.Sqrt(0.5); b = d = double.Sqrt(0.5);
      a = rat.Cbrt(0.5); b = d = double.Cbrt(0.5);
      a = rat.Hypot(0.5, 1.25); b = d = double.Hypot(0.5, 1.25);
      a = rat.Root(0.5, 3); b = d = double.Root(0.5, 3);
      a = rat.Root(0.5, 12); b = d = double.Root(0.5, 12);
      a = rat.Root(0.5, -12); b = d = double.Root(0.5, -12);

      //ILogarithmicFunctions
      a = rat.Log(0.5); b = d = double.Log(0.5);
      a = rat.Log2(0.5); b = d = double.Log2(0.5);
      a = rat.Log10(0.5); b = d = double.Log10(0.5);
      a = rat.Log10P1(0.5); b = d = double.Log10P1(0.5);
      a = rat.Log2P1(0.5); b = d = double.Log2P1(0.5);
      a = rat.LogP1(0.5); b = d = double.LogP1(0.5);
      a = rat.ILogB(0.5); b = d = double.ILogB(0.5);
      a = rat.ILogB(+12345.56); b = d = double.ILogB(+12345.56);

      a = rat.Log(0.5, (rat)16); b = d = double.Log(0.5, 16);

      //IExponentialFunctions
      a = rat.Exp(0.5); b = d = double.Exp(0.5);
      a = rat.Exp2(0.5); b = d = double.Exp2(0.5);
      a = rat.Exp2(-0.5); b = d = double.Exp2(-0.5);
      a = rat.Exp10(0.5); b = d = double.Exp10(0.5);
      a = rat.ExpM1(0.5); b = d = double.ExpM1(0.5);
      a = rat.Exp2M1(0.5); b = d = double.Exp2M1(0.5);
      a = rat.Exp10M1(0.5); b = d = double.Exp10M1(0.5);

      //ITrigonometricFunctions
      a = rat.Sin(0.5); b = d = double.Sin(0.5);
      a = rat.Cos(0.5); b = d = double.Cos(0.5);
      a = rat.Tan(0.5); b = d = double.Tan(0.5);
      a = rat.Asin(0.5); b = d = double.Asin(0.5);
      a = rat.Acos(0.5); b = d = double.Acos(0.5);
      a = rat.Atan(0.5); b = d = double.Atan(0.5);
      a = rat.AcosPi(0.5); b = d = double.AcosPi(0.5);
      a = rat.AsinPi(0.5); b = d = double.AsinPi(0.5);
      a = rat.Atan2Pi(0.5, 0.2); b = d = double.Atan2Pi(0.5, 0.2);
      a = rat.AtanPi(0.5); b = d = double.AtanPi(0.5);
      a = rat.CosPi(0.005); b = d = double.CosPi(0.005);
      a = rat.SinPi(0.005); b = d = double.SinPi(0.005);
      a = rat.TanPi(0.005); b = d = double.TanPi(0.005);
      a = rat.Atan2(0.5, 0.2); b = d = double.Atan2(0.5, 0.2);
      var o1 = rat.SinCos(0.5); var o2 = double.SinCos(0.5);

      //IHyperbolicFunctions
      b = d = double.Asinh(0.5); a = rat.Asinh(0.5);
      b = d = double.Acosh(2.5); a = rat.Acosh(2.5);
      b = d = double.Atanh(0.5); a = rat.Atanh(0.5);
      b = d = double.AsinPi(0.5); a = rat.AsinPi(0.5);
      b = d = double.AcosPi(0.5); a = rat.AcosPi(0.5);
      b = d = double.Sinh(0.5); a = rat.Sinh(0.5);
      b = d = double.Cosh(0.5); a = rat.Cosh(0.5);
      b = d = double.Tanh(0.5); a = rat.Tanh(0.5);

    }
#endif

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

    static BigRational ExampleCalcPiHalf()
    {
      var cpu = BigRational.task_cpu;
      cpu.push(Math.PI);
      cpu.push(2);
      cpu.div();
      var result = cpu.popr();
      return result;
    }
     
    //todo: check later
    static T Add1<T>(T a, T b) where T : INumber<T> // in release: no inline 
    {
      return a + b; 
    }
    static T Add2<T>(T a, T b) // in release: inline - and overall slightly faster ?!? 
    {
      { if (a is int u && b is int v && (u + v) is T r) return r; }
      { if (a is float u && b is float v && (u + v) is T r) return r; }
      { if (a is double u && b is double v && (u + v) is T r) return r; }
      { if (a is Int128 u && b is Int128 v && (u + v) is T r) return r; }
      { if (a is BigRational u && b is BigRational v && (u + v) is T r) return r; }
      { if (a is BigInteger u && b is BigInteger v && (u + v) is T r) return r; }
      return a;
    }

#endif
  }
}

