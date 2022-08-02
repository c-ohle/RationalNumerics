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
  }
}

