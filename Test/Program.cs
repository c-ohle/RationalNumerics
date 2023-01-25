
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
      ApplicationConfiguration.Initialize(); test();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }

    static void test1()
    {
      double a, b; BigRat c;
      var aa = new double[] { 0, 123, -123, 123.456, -123.456, 1.1, 1.8, -1.1, -1.8, 0.1, -0.1 };
      for (int i = 0; i < aa.Length; i++)
      {
        a = aa[i];
        b = double.Truncate(a); c = BigRat.Truncate(a); if (b != c) { }
        b = double.Floor(a); c = BigRat.Floor(a); if (b != c) { }
        b = double.Ceiling(a); c = BigRat.Ceiling(a); if (b != c) { }
        b = double.Round(a); c = BigRat.Round(a); if (b != c) { }
      }
    }

    static void test()
    {
      test1();

      var a = (rat)double.Pi; string s;
      var b = (BigRat)double.Pi; b = -double.E;

      b = BigRat.Parse("123.456'789"); s = b.ToString(null, null);
      b = BigRat.Parse("-123.456'789"); s = b.ToString(null, null);
      b = 9870123; s = b.ToString(null, null);
      b = (BigRat)1 / 3; s = b.ToString(null, null);
      b /= 1000; s = b.ToString(null, null);
      b *= 10; s = b.ToString(null, null);
      b *= 10; s = b.ToString(null, null);
      b *= 10; s = b.ToString(null, null);
      b *= 10; s = b.ToString(null, null);
      b *= 10; s = b.ToString(null, null);
      b *= 10; s = b.ToString(null, null);
      b *= 10; s = b.ToString(null, null);
      b *= 10; s = b.ToString(null, null);
      b = BigRat.Pi(200); s = b.ToString(null, null);
      b = BigRat.Pi(2000); s = b.ToString(null, null);
      b = 0; s = b.ToString(null, null);
      b = 123000; s = b.ToString(null, null);
      b = 12300; s = b.ToString(null, null);
      b = 1230; s = b.ToString(null, null);
      b = 123; s = b.ToString(null, null);
      b = 12.3; s = b.ToString(null, null);
      b = 1.23; s = b.ToString(null, null);
      b = 0.123; s = b.ToString(null, null);
      b = 0.0123; s = b.ToString(null, null);
      b = 0.00123; s = b.ToString(null, null);
      b = 0.000123; s = b.ToString(null, null);
      b = 0.0000123; s = b.ToString(null, null);
      b = 0.00000123; s = b.ToString(null, null);
      b = 0.000000123; s = b.ToString(null, null);
      b = 0.0000000123; s = b.ToString(null, null);
      b = 0.00000000123; s = b.ToString(null, null);
      b = 0.000000000123; s = b.ToString(null, null);
      b = 0.0000000000123; s = b.ToString(null, null);
      b = 0.00000000000123; s = b.ToString(null, null);

      b = 1e10; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = -1e-10; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = 1e20; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = -1e-20; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = 1e-20; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = 1e200; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = -1e-200; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = 1e300; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = -1e-300; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = 1e55; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);
      b = -1e-55; s = b.ToString(null, null); b = BigRat.Normalize(b); s = b.ToString(null, null);

      b = 9.99123e20; s = b.ToString(null, null);
      b = -9.99123e-20; s = b.ToString(null, null);


      a = rat.Sqrt(123.456);
      b = BigRat.Sqrt(123.456, 10);
      b = BigRat.Sqrt(123.456, 32);
      b = BigRat.Sqrt(123.456, 20);
      b = BigRat.Sqrt(123.456, 1000);

      a = rat.Sqrt(2);
      b = BigRat.Sqrt(2, 20);
      b = BigRat.Sqrt(2, 1000);
      b = BigRat.Sqrt(2, 1);
      b = BigRat.Sqrt(2, 2);
      b = BigRat.Sqrt(2, 3);
      b = BigRat.Sqrt(2, 4);
      b = BigRat.Sqrt(2, 10);
      b = BigRat.Sqrt(2, 20);

      a = rat.Sqrt(10);     
      a = rat.Parse("3.1622776601683793319988935444327185337195551393252168268575048527");
      b = BigRat.Sqrt(10, 20);
      b = BigRat.Sqrt(10, 30);

      a = rat.Sqrt(1e20);
      b = BigRat.Sqrt(1e20, 20);

      a = rat.Sqrt(1e-20);
      b = BigRat.Sqrt(1e-20, 20);
      
      b = BigRat.Sqrt(9, 20);
      b = BigRat.Sqrt(4, 20); b = BigRat.Normalize(b);

      b = BigRat.Sqrt(7 * 7, 20);

      var c = BigRat.Parse("3.14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327886593615338182");
      
      a = rat.Pi(100);
      b = BigRat.Pi(10);
      b = BigRat.Pi(20);
      b = BigRat.Pi(32);
      b = BigRat.Pi(100);
      b = BigRat.Pi(1000); a = rat.Pi(1000);

      b = 1 / b; b = -b; b = -b; b = 1 / b;

      b = BigRat.Normalize(b);

      b = BigRat.Round(b, 5);

      b = BigRat.Parse("123.456");
      b = BigRat.Parse("0.00120034");
      b = BigRat.Parse("123.456'789");
      b = BigRat.Parse("0.'3");
      b = BigRat.Parse("-123.456'789");
      b = BigRat.Parse("-3.141e-20");
      b = BigRat.Parse("3.141E+20");
      b = BigRat.Parse("-12.34'5678E+123");

      b = BigRat.Pi(32); b++; b--;

      b = BigRat.Pi(20);
      b = BigRat.Round(b, 7);

      b = (BigRat)double.Pi;
      b = BigRat.Normalize(b);

      b = (BigRat)700000000 / 800000000;
      b = BigRat.Normalize(b);

      b = BigRat.Pi(100);

      var rnd = new Random(0);
      for (int i = 0; i < 1000; i++)
      {
        var x1 = (0.5 - rnd.NextDouble()) * 1e10;
        var x2 = (0.5 - rnd.NextDouble()) * 1e10;
        var i1 = x1.CompareTo(x2);
        var i2 = ((BigRat)x1).CompareTo((BigRat)x2);
        if (i1 != i2) { }
      }

      b = (BigRat)1 / 3 + 10;
      a = (rat)1 / 3 + 10;
      b = b * b; a = a * a;
      b = b * b; a = a * a;

      if ((BigRat)2 / 3 == (BigRat)4 / 6) { }
      if ((BigRat)2 / 3 == (BigRat)4 / 5) { }

    }

  }

}