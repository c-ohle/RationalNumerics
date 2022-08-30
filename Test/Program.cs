
using System.Diagnostics;
using System.Numerics;

namespace Test
{
  internal unsafe static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); // test();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }

#if NET7_0

    static void test()
    {
      float a; Float32 b; double c; Float64 d; Float80 e; Float128 f; Float256 g;

      a = MathF.PI; a = a * a; a = -10000 * a;
      b = Float32.Pi; b = b * b; b = -10000 * b;
      d = Float64.Pi; d = d * d; d = -10000 * d;

      b = Float32.MaxValue;
      d = Float64.MaxValue; d = Float64.Cast(b);
      d = Float64.MaxValue; d = Float64.Cast(d);
      d = Float64.MaxValue; b = Float32.Cast(d);
      d = Float64.MinValue; b = Float32.Cast(d);

      a = (float)double.MaxValue;
      a = (float)double.MinValue;
      c = +1e-300; a = (float)c; b = c;
      c = -1e-300; a = (float)c; b = c;
       
      d = 123; a = Float64.Sign(d); d = -123; a = Float64.Sign(d); d -= d; a = Float64.Sign(d);
      e = 123; a = Float80.Sign(e); e = -123; a = Float80.Sign(e); e -= e; a = Float80.Sign(e);
      a = MathF.PI; b = MathF.PI; c = MathF.PI; d = MathF.PI; e = MathF.PI; f = MathF.PI; g = MathF.PI;
      a = (float)Math.PI; b = Math.PI; c = Math.PI; d = Math.PI; e = Math.PI; f = Math.PI; g = Math.PI;
      a = MathF.PI; b = Float32.Pi; c = Math.PI; d = Float64.Pi; e = Float80.Pi; f = Float128.Pi; g = Float256.Pi;
      e = a; e = c;

      f = c; f = Float128.Cast(d);
      f = new Float128(1.5);

      a = MathF.Sqrt(2); b = Float32.Sqrt(2); c = Math.Sqrt(2); d = Float64.Sqrt(2);
      e = Float80.Sqrt(2); f = Float128.Sqrt(2); g = Float256.Sqrt(2);

      a = MathF.Sin(2); b = Float32.Sin(2); c = Math.Sin(2); d = Float64.Sin(2);
      e = Float80.Sin(2); f = Float128.Sin(2); g = Float256.Sin(2);

      a = MathF.Cos(2); b = Float32.Cos(2); c = Math.Cos(2); d = Float64.Cos(2);
      e = Float80.Cos(2); f = Float128.Cos(2); g = Float256.Cos(2);

      a = MathF.Pow(2, 0.5f); b = Float32.Pow(2, 0.5f); c = Math.Pow(2, 0.5f); d = Float64.Pow(2, 0.5f);
      e = Float80.Pow(2, 0.5f); f = Float128.Pow(2, 0.5f); g = Float256.Pow(2, 0.5f);

      compare(c, d);
      compare(a, b);
      static void compare<A, B>(A a, B b)
        where A : IBinaryFloatingPointIeee754<A>, IMinMaxValue<A>
        where B : IBinaryFloatingPointIeee754<B>, IMinMaxValue<B>
      {
        var c = a * a;
        var d = b * b;
      }

      test80();
      test128();
      test32();
      test64();
    }

    static void test64()
    {
      Float64 a; double d; bool x, y; string s; // double.CreateChecked

      d = double.MaxValue; a = Float64.MaxValue;

      a = Float64.NaN; s = a.ToString();
      a = Float64.NegativeInfinity; s = a.ToString();
      a = Float64.PositiveInfinity; s = a.ToString();

      d = double.NaN; a = Float64.NaN; x = double.IsNaN(d); y = Float64.IsNaN(a);
      d = double.Epsilon; a = Float64.Epsilon; a.ToString();
      d = double.NegativeInfinity; a = Float64.NegativeInfinity; x = double.IsNaN(d); y = Float64.IsNaN(a);
      //y = !Float64.IsFinite(a);
      d = double.PositiveInfinity; a = Float64.PositiveInfinity; x = double.IsNaN(d); y = Float64.IsNaN(a);
      d = double.NegativeZero; a = Float64.NegativeZero; x = double.IsNaN(d); y = Float64.IsNaN(a);
      *(ulong*)&d |= 0x8000000000000000;
      d = Math.PI; a = *(Float64*)&d;
      //float.NegativeZero
      d = double.Pi; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);

      d = double.Epsilon; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);
      d = double.NaN; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);


      d = double.NegativeInfinity; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);
      d = double.PositiveInfinity; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);
      d = double.NegativeZero; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);

      d = double.NegativeZero; a = Float64.NegativeZero;
      x = double.IsFinite(d); y = Float64.IsFinite(a);
      x = double.IsInfinity(d); y = Float64.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float64.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float64.IsNegativeInfinity(a);

      d = double.NegativeInfinity; a = Float64.NegativeInfinity;
      x = double.IsFinite(d); y = Float64.IsFinite(a);
      x = double.IsInfinity(d); y = Float64.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float64.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float64.IsNegativeInfinity(a);

      d = double.NaN; a = Float64.NaN;
      x = double.IsNaN(d); y = Float64.IsNaN(a);
      x = double.IsFinite(d); y = Float64.IsFinite(a);
      x = double.IsInfinity(d); y = Float64.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float64.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float64.IsNegativeInfinity(a);

    }
    static void test32()
    {
      Float32 a; float d; bool x, y; string s; // float.CreateChecked

      d = float.MaxValue; a = Float32.MaxValue;

      a = Float32.NaN; s = a.ToString();
      a = Float32.NegativeInfinity; s = a.ToString();
      a = Float32.PositiveInfinity; s = a.ToString();

      d = float.NaN; a = Float32.NaN; x = float.IsNaN(d); y = Float32.IsNaN(a);
      d = float.Epsilon; a = Float32.Epsilon; a.ToString();
      d = float.NegativeInfinity; a = Float32.NegativeInfinity; x = float.IsNaN(d); y = Float32.IsNaN(a);
      //y = !Float32.IsFinite(a);
      d = float.PositiveInfinity; a = Float32.PositiveInfinity; x = float.IsNaN(d); y = Float32.IsNaN(a);
      d = float.NegativeZero; a = Float32.NegativeZero; x = float.IsNaN(d); y = Float32.IsNaN(a);
      d = MathF.PI; a = *(Float32*)&d;
      //float.NegativeZero
      d = float.Pi; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);

      d = float.Epsilon; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);
      d = float.NaN; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);


      d = float.NegativeInfinity; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);
      d = float.PositiveInfinity; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);
      d = float.NegativeZero; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);

      d = float.NegativeZero; a = Float32.NegativeZero;
      x = float.IsFinite(d); y = Float32.IsFinite(a);
      x = float.IsInfinity(d); y = Float32.IsInfinity(a);
      x = float.IsPositiveInfinity(d); y = Float32.IsPositiveInfinity(a);
      x = float.IsNegativeInfinity(d); y = Float32.IsNegativeInfinity(a);

      d = float.NegativeInfinity; a = Float32.NegativeInfinity;
      x = float.IsFinite(d); y = Float32.IsFinite(a);
      x = float.IsInfinity(d); y = Float32.IsInfinity(a);
      x = float.IsPositiveInfinity(d); y = Float32.IsPositiveInfinity(a);
      x = float.IsNegativeInfinity(d); y = Float32.IsNegativeInfinity(a);

      d = float.NaN; a = Float32.NaN;
      x = float.IsNaN(d); y = Float32.IsNaN(a);
      x = float.IsFinite(d); y = Float32.IsFinite(a);
      x = float.IsInfinity(d); y = Float32.IsInfinity(a);
      x = float.IsPositiveInfinity(d); y = Float32.IsPositiveInfinity(a);
      x = float.IsNegativeInfinity(d); y = Float32.IsNegativeInfinity(a);
    }
    static void test80()
    {
      Float80 a; double d; bool x, y; string s; // double.CreateChecked

      d = double.MaxValue; a = Float80.MaxValue;
      a = 1234;
      a = Math.PI;
      a = a + a; a = a * a;

      a = Float80.NaN; s = a.ToString();
      a = Float80.NegativeInfinity; s = a.ToString();
      a = Float80.PositiveInfinity; s = a.ToString();

      d = double.NegativeZero; a = Float80.NegativeZero;
      x = double.IsFinite(d); y = Float80.IsFinite(a);
      x = double.IsInfinity(d); y = Float80.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float80.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float80.IsNegativeInfinity(a);

      d = double.NegativeInfinity; a = Float80.NegativeInfinity;
      x = double.IsFinite(d); y = Float80.IsFinite(a);
      x = double.IsInfinity(d); y = Float80.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float80.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float80.IsNegativeInfinity(a);

      d = double.NaN; a = Float80.NaN;
      x = double.IsNaN(d); y = Float80.IsNaN(a);
      x = double.IsFinite(d); y = Float80.IsFinite(a);
      x = double.IsInfinity(d); y = Float80.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float80.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float80.IsNegativeInfinity(a);

    }
    static void test128()
    {
      Float128 a; double d; bool x, y; string s; // double.CreateChecked

      d = double.MaxValue; a = Float128.MaxValue;

      a = Float128.NaN; s = a.ToString();
      a = Float128.NegativeInfinity; s = a.ToString();
      a = Float128.PositiveInfinity; s = a.ToString();

      d = double.NaN; a = Float128.NaN; x = double.IsNaN(d); y = Float128.IsNaN(a);
      d = double.Epsilon; a = Float128.Epsilon; a.ToString();
      d = double.NegativeInfinity; a = Float128.NegativeInfinity; x = double.IsNaN(d); y = Float128.IsNaN(a);
      //y = !Float128.IsFinite(a);
      d = double.PositiveInfinity; a = Float128.PositiveInfinity; x = double.IsNaN(d); y = Float128.IsNaN(a);
      d = double.NegativeZero; a = Float128.NegativeZero; x = double.IsNaN(d); y = Float128.IsNaN(a);
      d = double.Pi; a = Float128.Pi; x = double.IsFinite(d); y = Float128.IsFinite(a);

      d = double.Epsilon; a = *(Float128*)&d; x = double.IsFinite(d); y = Float128.IsFinite(a);
      d = double.NaN; a = Float128.NaN; x = double.IsFinite(d); y = Float128.IsFinite(a);


      d = double.NegativeInfinity; a = Float128.NegativeInfinity; x = double.IsFinite(d); y = Float128.IsFinite(a);
      d = double.PositiveInfinity; a = Float128.PositiveInfinity; x = double.IsFinite(d); y = Float128.IsFinite(a);
      d = double.NegativeZero; a = Float128.NegativeZero; x = double.IsFinite(d); y = Float128.IsFinite(a);

      d = double.NegativeZero; a = Float128.NegativeZero;
      x = double.IsFinite(d); y = Float128.IsFinite(a);
      x = double.IsInfinity(d); y = Float128.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float128.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float128.IsNegativeInfinity(a);

      d = double.NegativeInfinity; a = Float128.NegativeInfinity;
      x = double.IsFinite(d); y = Float128.IsFinite(a);
      x = double.IsInfinity(d); y = Float128.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float128.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float128.IsNegativeInfinity(a);

      d = double.NaN; a = Float128.NaN;
      x = double.IsNaN(d); y = Float128.IsNaN(a);
      x = double.IsFinite(d); y = Float128.IsFinite(a);
      x = double.IsInfinity(d); y = Float128.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float128.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float128.IsNegativeInfinity(a);

    }
#endif

  }
}