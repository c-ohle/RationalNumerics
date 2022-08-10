global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;
global using float16 = System.Numerics.BigRational.Float<System.Int16>;
global using float32 = System.Numerics.BigRational.Float<System.Int32>;
global using float64 = System.Numerics.BigRational.Float<System.Int64>;
global using float128 = System.Numerics.BigRational.Float<(System.Int64, System.Int64)>; //System.Int128 for NET7

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

    static void test()
    {
      float128 a, b, c;
      a = Math.PI;
      b = (float128)BigRational.Pi(38);
      a = b;
      b = a * a;
      c = b / a;
      c = a + a;
      b = c - a;
      a = a + 100000;
      a = a - 100000;
      a = a - 100000;
      a = a + 100000;
    }
  }

}
