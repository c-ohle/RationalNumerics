
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

    static void test()
    {
      float a; Float32 b; double c; Float64 d; Float80 e; Float128 f; Float256 g;

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

    }

#if NET7_0
    static void compare<A, B>(A a, B b)
      where A : IBinaryFloatingPointIeee754<A>, IMinMaxValue<A>
      where B : IBinaryFloatingPointIeee754<B>, IMinMaxValue<B>
    {
      var c = a * a;
      var d = b * b;
    }
#endif
  }
}