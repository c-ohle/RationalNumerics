using System.Diagnostics;
using Rational = System.Numerics.Rational.NewRational;
using rat = System.Numerics.Rational.NewRational;
using old = Test.OldRational;
using Microsoft.Win32;

namespace Test
{

  internal static class Program
  {
    [STAThread]
    static void Main()
    {
      var x = (rat)123.456789m;
      var y = (old)x;
      x = x * x * x * x + 12345678;
      y = y * y * y * y + 12345678;
      Debug.Assert(x == y);

      x = MathF.PI;
      x = Math.PI;


      ApplicationConfiguration.Initialize();
      Application.Run(new Form { Text = "Under construction", Width = 640, Height = 480 });
    }
  }
}