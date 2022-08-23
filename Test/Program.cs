
global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;
global using Float16 = System.Numerics.Generic.Float<System.UInt16>;
global using Float32 = System.Numerics.Generic.Float<System.UInt32>;

using System.Diagnostics;
using System.Numerics;
using System.Globalization;
using System.Numerics.Generic;
using System.Numerics.Rational;

namespace Test
{
  internal unsafe static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); //test();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }
  }
}
