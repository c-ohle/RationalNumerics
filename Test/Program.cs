
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Test
{
  internal unsafe static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); test();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }

//#if NET7_0
    static void test()
    {
      var a = __float16.Pi;
      var b = __float32.Pi;
      var c = __float64.Pi;
      var d = __float80.Pi;
      var e = __float96.Pi;
      var f = __float128.Pi;
      var g = __float256.Pi;
    }
//#endif
  }

}