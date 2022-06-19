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

    static unsafe void test()
    {
      var cpu = rat.task_cpu;
    }
  }
}