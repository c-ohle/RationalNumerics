global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;
using System.Globalization;

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

#if NET7_0

    static void test2()
    {
      BigRational a, b, c, d;
      a = 3; b = 4; c = 5;

      d = a * a + a * b - c / a;
      d = 0 | a * a + a * b - c / a;
      d = a * a + a * b - c / a | 0;
      d = 100 | a * a + a * b - c / a;
      d = a * a + a * b - c / a | 100;
      d = 1 | a * a + a * b - c / a;
      d = a * a + a * b - c / a | 1;

      BigInt x, y, z, w;
      x = 3; y = 4; z = 5;
      w = x * x + x * y - z / x;
      w = 0 | x * x + x * y - z / x;
      w = 0xff | x * x + x * y - z / x;
         
    }

    static void test()
    {
      test2();

      BigInt.test();

      BigInt b, bb = 88;

      b = (BigInt)100 * 10 + bb * 0x9999999999999;
      b = 0 | (BigInt)100 * 10 + bb * 0x9999999999999;

      var t = bb * 100 == b - 10 - 237790060325163126 + 8800;
      t = (0 | bb * 100) == (0 | b - 10 - 237790060325163126 + 8800);
      t = (0 | bb * 100) == (0 | b - 10 - 237790060325163126 + 8800);

      var xx = fastcalc(bb, b);
      xx = 0 | fastcalc(bb, b);
      static BigInt fastcalc(BigInt a, BigInt b)
      {
        var t = a * 100 == b - 10 - 237790060325163126 + 8800;
        return a * b + b * a;
      }
    }

#endif
  }
}

