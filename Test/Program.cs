
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    static void test()
    {
      test1();
      test2();

      static void test1()
      {
        double a, b; BigRat c;
        var aa = new double[] { 0, 123, -123, 123.456, -123.456, 1.1, 1.8, -1.1, -1.8, 0.1, -0.1 };
        for (int i = 0; i < aa.Length; i++)
        {
          a = aa[i];
          b = Math.Truncate(a); c = BigRat.Truncate(a); Debug.Assert(b == c);
          b = Math.Floor(a); c = BigRat.Floor(a); Debug.Assert(b == c);
          b = Math.Ceiling(a); c = BigRat.Ceiling(a); Debug.Assert(b == c);
          b = Math.Round(a); c = BigRat.Round(a); Debug.Assert(b == c);
        }
      }
      static void test2()
      {
        double a; BigRat b; string sa, sb;
        a = Math.PI;
        b = BigRat.Pi(1000);
        for (int k = 16; k >= 1; k--)  //17
        {
          var fmt = ((k & 1) != 0 ? "G" : "g") + k;
          for (int i = -10; i <= 20; i++)
          {
            var p = Math.Pow(10, i); if ((i & 1) != 0) p = -p;
            sa = (a * p).ToString(fmt);
            sb = (b * p).ToString(fmt); Debug.Assert(sa == sb);
            for (int j = 5; j < 8; j++) //
            {
              sa = (Math.Round(a, j) * p).ToString(fmt);
              sb = (BigRat.Round(b, j) * p).ToString(fmt); Debug.Assert(sa == sb);
            }
          }
        }
      }
    }

  }

}