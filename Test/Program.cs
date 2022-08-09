global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;


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

    static void test3()
    {
      var a = (BigFloat)Math.PI; var ss = a.ToString();
      var b = (BigFloat)2;
      var c = a * b;
      a = -(BigFloat)Math.PI;
      a = (BigFloat)(-Math.PI);
      b = -2;

      a = BigFloat.Sin(Math.PI / 4);

      var aa = 0 | (BigInt)7 * 8 + 1000;
      a = 0 | (BigFloat)7 * 8 + 1000;

      var r = BigRational.Pi(100); ss = r.ToString("L60"); var nd = BigRational.NumDen(r);
      a = (BigFloat)r;
      r = (BigRational)1 / 3;
      a = (BigFloat)r;

      c = c * 0.5;

      b = a + 1;
      c = b - 1;

      b = 1;
      c = b + 2;
      c = c * 100;

      var bi = BigRational.Integer.GreatestCommonDivisor(8, 12);
      bi = BigRational.Integer.GreatestCommonDivisor(56, 7);
      BigRational br = BigRational.Integer.GreatestCommonDivisor(56, 7);
      BigInteger bbi = (BigInteger)bi;

      //BigRational.Integer.Lsb
      a = 1; a = a / 3; b = a; if (b == a) { }
      b = 100; b = b / 3; if (b != a) { }
      c = 0.001; c = c / 3;

      c = b;
      if (c < b) { } if (c <= b) { } if (c > b) { } if (c >= b) { }
      b += 0.000001;
      if (c < b) { }
      if (c <= b) { }
      if (c > b) { }
      if (c >= b) { }

      c = 1; c /= 100; c = c / 3;
      c = 1; c /= 10000; c = c / 3;
      c = 1; c /= 2; c = c / 3;

      ss = BigRational.Pi(100).ToString("L100");
      a = BigFloat.Pi();
      c = Math.PI;
      c = c / 2;
      c = c * 2;

      a = Math.PI;
      b = a + Math.PI;
      c = b - Math.PI;

      a = Math.PI;
      b = a + 100000;
      c = b - 100000;

      a = Math.PI;
      b = a + 0.00001;
      c = b - 0.00001;
    }
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
      test3();
      test2();

      //BigInt.test();


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
