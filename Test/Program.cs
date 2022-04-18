global using System.Diagnostics;
global using System.Numerics.Rational;
global using rat = System.Numerics.Rational.NewRational;
global using old = Test.OldRational;
using System.Buffers;
using System.Numerics;

namespace Test
{
  internal static class Program
  {    
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize();
      Application.Run(new Form { Text = "Under construction", Width = 640, Height = 480 });
    }
  }
}