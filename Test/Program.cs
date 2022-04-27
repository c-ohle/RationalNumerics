global using System.Diagnostics;
global using System.Numerics.Rational;
global using rat = System.Numerics.Rational.NewRational;
//global using old = Test.BigRational;
using System.Buffers;
using System.Drawing.Imaging;
using System.Numerics;

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
    }
  }
}