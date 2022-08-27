
global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;

using System.Diagnostics;
using System.Numerics;
using System.Globalization;
using System.Numerics.Generic;
using System.Numerics.Rational;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

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

    [StructLayout(LayoutKind.Sequential, Size = 4)]
    readonly struct FloatType32 : IFloatType<FloatType32> { }
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    readonly struct FloatType256 : IFloatType<FloatType256> {  }

    static void test()
    {
      var a = Float<FloatType32>.Pi;
      var b = Math.PI; 
      var c = Float80.Pi;
      var d = Float128.Pi;       
      var e = Float<FloatType256>.Pi;

      d = Float128.Round(d, 4);
      e = Float<FloatType256>.Round(e, 4);
      d = b;
    }
  }
}