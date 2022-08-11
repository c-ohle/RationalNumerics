global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;

global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;
global using float16 = System.Numerics.BigRational.Float<System.Int16>;
global using float32 = System.Numerics.BigRational.Float<System.Int32>;
global using float64 = System.Numerics.BigRational.Float<System.Int64>;
global using float80 = System.Numerics.BigRational.Float<Test._base80>;
global using float96 = System.Numerics.BigRational.Float<Test._base96>;
global using float128 = System.Numerics.BigRational.Float<(System.Int64, System.Int64)>; //System.Int128 for NET7
global using float256 = System.Numerics.BigRational.Float<(System.Int64, System.Int64, System.Int64, System.Int64)>; //System.Int128 for NET7
using System.Runtime.InteropServices;

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

    static void test()
    {
      var a = (float128)1; a /= 3;
      var b = (float128)BigRational.Pi(100);
      var c = b * 100;
      c = c / 100;
      c = c + 100;
      c = c - 100;

      c = b * 1234.5678;
      c = c / 1234.5678;
      c = c + 1234.5678;
      c = c - 1234.5678;

      c = b * 128;
      c = c / 128;
      c = c + 128;
      c = c - 128;

    }
  }
  //unsafe struct _base80 { fixed ushort a[5]; }
  [StructLayout(LayoutKind.Sequential, Pack = 2, Size = 10)]
  unsafe struct _base80 { ushort h; ulong l; }
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
  unsafe struct _base96 { uint h; ulong l; }

}
