
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
      ApplicationConfiguration.Initialize(); //test();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }

#if _NET6_0
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
#endif
#if _NET7_0
    static void test()
    {
      var t1 = double.Acos(0.5);
      var t2 = rat.Acos(0.5); t2 = rat.Acos(0.5, 30); t2 = rat.Acos(0.5, 40);
      var t3 = __float128.Acos(0.5);
      var t4 = (rat)"1.0471975511965977461542144610931 676280657231331250352736583148641026054687620696662093449417807056893273826955044274355490312815";
      
      t1 = double.Acos(-0.5);
      t2 = rat.Acos(-0.5);
      t3 = __float128.Acos(-0.5);

      t1 = double.Acos(0);
      t2 = rat.Acos(0);
      t3 = __float128.Acos(0);

      t1 = double.Acos(0.1);
      t2 = rat.Acos(0.1);
      t3 = __float128.Acos(0.1);

      t1 = double.Acos(-0.1);
      t2 = rat.Acos(-0.1);
      t3 = __float128.Acos(-0.1);

      t1 = double.Acos(1);
      t2 = rat.Acos(1);
      t3 = __float128.Acos(1);

      t1 = double.Acos(-1);
      t2 = rat.Acos(-1);
      t3 = __float128.Acos(-1);

      t1 = double.Acos(1.1);
      t2 = rat.Acos(1.1);
      t3 = __float128.Acos(1.1);

      t1 = double.Acos(-1.1);
      t2 = rat.Acos(-1.1);
      t3 = __float128.Acos(-1.1);

      t1 = double.Asin(0.5);
      t2 = rat.Asin(0.5);
      t3 = __float128.Asin(0.5);

      t1 = double.Asin(0);
      t2 = rat.Asin(0);
      t3 = __float128.Asin(0);

      t1 = double.Asin(1.1);
      t2 = rat.Asin(1.1); //Asin(1) return 0 instead of 1.570796 rad (90°)
      t3 = __float128.Asin(1.1);

      t1 = double.Asin(1);
      t2 = rat.Asin(1); //Asin(1) return 0 instead of 1.570796 rad (90°)
      t3 = __float128.Asin(1);

      t1 = double.Asin(-1);
      t2 = rat.Asin(-1); //Asin(1) return 0 instead of 1.570796 rad (90°)
      t3 = __float128.Asin(-1);

      t1 = double.Acos(0);
      t2 = rat.Acos(0); //Acos(0) return 0 instead of 1.570796 rad(90°)
      t3 = __float128.Acos(0);

      t1 = double.Acos(1);
      t2 = rat.Acos(1);
      t3 = __float128.Acos(1);


      var a = __float16.Pi;
      var b = __float32.Pi;
      var c = __float64.Pi;
      var d = __float80.Pi;
      var e = __float96.Pi;
      var f = __float128.Pi;
      var g = __float256.Pi;
      g++;
    }
#endif
  }
}