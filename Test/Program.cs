global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;

namespace Test
{
  internal unsafe static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); // test();
      Application.Run(new MainFrame());
    }

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
  }
}