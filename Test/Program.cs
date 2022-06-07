global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;

namespace Test
{
  internal static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); test();
      Application.Run(new MainFrame());
    }

    static void test()
    {
      //double a; rat b;
      //
      //b = rat.Sqrt(rat.Pow(10, 5000), 10000); var ss = b.ToString("L10000");
      //b = rat.Sqrt(rat.Pow(1.23456m, -5000), 10000); ss = b.ToString("L10000");
      //
      //a = Math.Sqrt(1e-55); b = rat.Sqrt(1e-55, 1000); //var ss = b.ToString("L1000");
      //a = Math.Sqrt(1e-55); b = rat.Sqrt(1e-55, 32); //var ss = b.ToString("L1000");
      //
      //a = Math.Sqrt(1e-50); b = rat.Sqrt(1e-50, 100); //var ss = b.ToString("L1000");
      //a = Math.Sqrt(1e+50); b = rat.Sqrt(1e+50, 100);
      //a = Math.Sqrt(2); b = rat.Sqrt(2, 20);
      //a = Math.Sqrt(1); b = rat.Sqrt(1, 20);
      //a = Math.Sqrt(0); b = rat.Sqrt(0, 20);
      //
      //a = Math.Sqrt(1e+300); b = rat.Sqrt(1e+300, 20);
      //a = Math.Sqrt(1e-300); b = rat.Sqrt(1e-300, 20);
    }
  }
}