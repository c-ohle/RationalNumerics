global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;
global using Float16 = System.Numerics.BigRational.Float<System.UInt16>;
global using Float32 = System.Numerics.BigRational.Float<System.UInt32>;
using System.Globalization;

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

#if false //NET7_0
    static void test()
    {
      //var a = float.Pi;
      //var b = double.Pi;
      var c = Float80.Pi;
      var d = Float96.Pi;
      var e = Float128.Pi;
      var f = BigRational.Float<(UInt128, UInt64)>.Pi; // Float192
      //var g = Float256.Pi;
      //var h = BigRational.Float<(UInt128, UInt128, UInt128)>.Pi; // Float384
      //var i = BigRational.Float<(UInt128, UInt128, UInt128, UInt128)>.Pi; // Float512
      
      test_strxx();
      static void test_strxx()
      {
        Float64 a = Float64.Pi, b = 3;
        b = Float64.Parse("1.23456");
        b = Float64.Parse("-1.23456");
        var s = b.ToString("", null);
        s = b.ToString();
        var d = double.Parse("-1.23456", NumberFormatInfo.InvariantInfo);
        d = double.Parse("-1.23456", NumberFormatInfo.InvariantInfo);

        b = 3;
        xxx(a, b);
        xxx((Float96)Math.PI, (Float96)3);

        xxx(Math.PI, 3); xxx(Math.PI, 0); xxx(0, Math.PI);

        static void xxx<T>(T a, T b) where T : IBinaryFloatingPointIeee754<T>
        {
          var c = a * b;
          c = a + b;
          c = a - b;
          c = a * b;
          c = a / b;
          c = a % b;
          c = T.Truncate(a);
          c = T.Abs(+a);
          c = T.Abs(-a);
          c = T.Sqrt(a);
          c = T.Sin(a);
          var x = T.Radix;
          var y = T.IsCanonical(a);
          b = T.One;
          b = T.Zero;
          b = T.NegativeOne;
          //x = b.GetExponentShortestBitLength();
          var s = b.ToString("G", null);
          b++; b--;
          c = T.Parse(s, null);
          //c = a & b;
          //c = a ^ b;
          //c = a | b;
        }
      }
      test_str69();
      test_str64();
      test_Float64();
      test_Float32();
      test_Float80();
      test_Float16();
      test_Float96();
      test_Float128();
      test_Float256();
      test_str();
      test_big();

      static void test_str69()
      {
        Float96 a, b; double d, e; string s, t;

        d = 1054.32179;
        t = d.ToString("F"); a = d; s = a.ToString("F");
        t = d.ToString("F1"); a = d; s = a.ToString("F1");
        t = d.ToString("F2"); a = d; s = a.ToString("F2");
        t = d.ToString("F3"); a = d; s = a.ToString("F3");
        t = d.ToString("F15"); a = d; s = a.ToString("F15");

        d = 123000000;
        t = d.ToString("F15"); a = d; s = a.ToString("F15");
        d = 0.000000000000000000123;
        t = d.ToString("F15"); a = d; s = a.ToString("F15");

        s = a.ToString("F30");
        s = a.ToString("F40");
        s = a.ToString("F80");

        d = -195489100.8377;
        t = d.ToString("F"); a = d; s = a.ToString("F");
        t = d.ToString("F1"); a = d; s = a.ToString("F1");
        t = d.ToString("F2"); a = d; s = a.ToString("F2");
        t = d.ToString("F3"); a = d; s = a.ToString("F3");
        t = d.ToString("f0"); a = d; s = a.ToString("f0");

        d = 1230.0012300123;
        t = d.ToString("F"); a = d; s = a.ToString("F");
        t = d.ToString("f0"); a = d; s = a.ToString("f0");
        t = d.ToString("F1"); a = d; s = a.ToString("F1");
        t = d.ToString("F2"); a = d; s = a.ToString("F2");
        t = d.ToString("F3"); a = d; s = a.ToString("F3");
        t = d.ToString("F4"); a = d; s = a.ToString("F4");
        t = d.ToString("F5"); a = d; s = a.ToString("F5");
        t = d.ToString("F6"); a = d; s = a.ToString("F6");
        t = d.ToString("F7"); a = d; s = a.ToString("F7");

        d = 1054.32179;
        t = d.ToString("e3"); a = d; s = a.ToString("e3");
        t = d.ToString("e2"); a = d; s = a.ToString("e2");
        t = d.ToString("e1"); a = d; s = a.ToString("e1");
        t = d.ToString("E"); a = d; s = a.ToString("E");
        t = d.ToString("E10"); a = d; s = a.ToString("E10");

        d = -1.954891e+008;
        t = d.ToString("e0"); a = d; s = a.ToString("e0");
        t = d.ToString("e1"); a = d; s = a.ToString("e1");
        t = d.ToString("e2"); a = d; s = a.ToString("e2");
        t = d.ToString("e3"); a = d; s = a.ToString("e3");

        d = 1234567.89;
        t = d.ToString("E0"); a = d; s = a.ToString("E0");
        t = d.ToString("E1"); a = d; s = a.ToString("E1");
        t = d.ToString("E2"); a = d; s = a.ToString("E2");
        t = d.ToString("E3"); a = d; s = a.ToString("E3");
        d = -0.00001020304;
        t = d.ToString("E"); a = d; s = a.ToString("E");
        t = d.ToString("E1"); a = d; s = a.ToString("E1");
        t = d.ToString("E2"); a = d; s = a.ToString("E2");
        t = d.ToString("E3"); a = d; s = a.ToString("E3");
        t = d.ToString("E4"); a = d; s = a.ToString("E4");
        t = d.ToString("E5"); a = d; s = a.ToString("E5");


        Span<char> ss = new char[100];
        a = Math.PI;
        a.TryFormat(ss, out var ns, "G", null); s = ss.Slice(0, ns).ToString();
        a.TryFormat(ss, out ns, "G4", null); s = ss.Slice(0, ns).ToString();
        a = Math.PI * 1e50;
        a.TryFormat(ss, out ns, default, null); s = ss.Slice(0, ns).ToString();
        a.TryFormat(ss, out ns, "g7", null); s = ss.Slice(0, ns).ToString();

        for (int i = -20; i <= +20; i++)
        {
          d = Math.PI * Math.Pow(10, i);
          t = d.ToString(); a = d; s = a.ToString("G");
        }
        d = Math.PI; t = d.ToString(); a = d; s = a.ToString("G");
        d = Math.PI; t = d.ToString("G0"); a = d; s = a.ToString("G0");
        d = Math.PI; t = d.ToString("G1"); a = d; s = a.ToString("G1");
        d = Math.PI; t = d.ToString("G2"); a = d; s = a.ToString("G2");
        d = Math.PI; t = d.ToString("G3"); a = d; s = a.ToString("G3");
        d = -Math.PI; t = d.ToString(); a = d; s = a.ToString("G");
        d = -Math.PI; t = d.ToString("G3"); a = d; s = a.ToString("G3");

        d = 123.456; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1054.32179; t = d.ToString(); a = d; s = a.ToString("G");
        d = -195489100.8377; t = d.ToString(); a = d; s = a.ToString("G");
        d = -195489100; t = d.ToString(); a = d; s = a.ToString("G");
        d = -0.00123; t = d.ToString(); a = d; s = a.ToString("G");
        d = -0.123; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1234; t = d.ToString(); a = d; s = a.ToString("G");
        d = 0; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1; t = d.ToString(); a = d; s = a.ToString("G");
        d = -1; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1.0437E+21; t = d.ToString(); a = d; s = a.ToString("G");
        d = -1.0573E-05; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1.01234567891E+21; t = d.ToString(); a = d; s = a.ToString("G"); //check

        d = 1054.32179;
        t = d.ToString("E"); a = d; s = a.ToString("E");
        t = d.ToString("E1"); a = d; s = a.ToString("E1");
        t = d.ToString("e2"); a = d; s = a.ToString("e2");
        t = d.ToString("E3"); a = d; s = a.ToString("E3");
        d = -195489100.8377;
        t = d.ToString("e"); a = d; s = a.ToString("e");
        t = d.ToString("E1"); a = d; s = a.ToString("E1");
        t = d.ToString("E2"); a = d; s = a.ToString("E2");
        t = d.ToString("E3"); a = d; s = a.ToString("E3");

        a = Float64.MinValue; Debug.Assert(a == double.MinValue);
        a = Float64.MaxValue; Debug.Assert(a == double.MaxValue);

        a = (double)Float32.MinValue; Debug.Assert(a == float.MinValue);
        a = (double)Float32.MaxValue; Debug.Assert(a == float.MaxValue);

        var z1 = Float16.MaxValue; Debug.Assert((double)z1 == +65504);
        var z2 = Float16.MinValue; Debug.Assert((double)z2 == -65504);

        var x2 = Float80.MaxValue;
        var x1 = Float80.MinValue;

        var t1 = Float128.MaxValue; t1 = Float128.MinValue;
        var t2 = Float256.MaxValue; t2 = Float256.MinValue;

        a = 2; d = 2; Debug.Assert((double)a == d);
        a *= 2; d *= 2; Debug.Assert((double)a == d);
        a *= 2; d *= 2; Debug.Assert((double)a == d);
        a /= 2; d /= 2; Debug.Assert((double)a == d);
        var rnd = new Random(13);
        for (int i = 0; i < 10000; i++)
        {
          d = -rnd.NextDouble() * 1e-20; a = d; Debug.Assert((double)a == d);
          a *= 2; d *= 2; Debug.Assert((double)a == d);
          a /= 2; d /= 2; Debug.Assert((double)a == d);
          b = a; e = d;
          a = b * b; d = e * e; Debug.Assert((double)a == d);
        }
        a = Math.PI; d = Math.PI; Debug.Assert((double)a == d);
        b = a * a; e = d * d; Debug.Assert((double)b == e);
        b = b / a; e = e / d; Debug.Assert((double)b == e);

      }
      static void test_str64()
      {
        Float64 a, b; double d, e; string s, t;

        d = 1054.32179;
        t = d.ToString("F"); a = d; s = a.ToString("F");
        t = d.ToString("F1"); a = d; s = a.ToString("F1");
        t = d.ToString("F2"); a = d; s = a.ToString("F2");
        t = d.ToString("F3"); a = d; s = a.ToString("F3");
        t = d.ToString("F15"); a = d; s = a.ToString("F15");

        d = 123000000;
        t = d.ToString("F15"); a = d; s = a.ToString("F15");
        d = 0.000000000000000000123;
        t = d.ToString("F15"); a = d; s = a.ToString("F15");

        s = a.ToString("F30");
        s = a.ToString("F40");
        s = a.ToString("F80");

        d = -195489100.8377;
        t = d.ToString("F"); a = d; s = a.ToString("F");
        t = d.ToString("F1"); a = d; s = a.ToString("F1");
        t = d.ToString("F2"); a = d; s = a.ToString("F2");
        t = d.ToString("F3"); a = d; s = a.ToString("F3");
        t = d.ToString("f0"); a = d; s = a.ToString("f0");

        d = 1230.0012300123;
        t = d.ToString("F"); a = d; s = a.ToString("F");
        t = d.ToString("f0"); a = d; s = a.ToString("f0");
        t = d.ToString("F1"); a = d; s = a.ToString("F1");
        t = d.ToString("F2"); a = d; s = a.ToString("F2");
        t = d.ToString("F3"); a = d; s = a.ToString("F3");
        t = d.ToString("F4"); a = d; s = a.ToString("F4");
        t = d.ToString("F5"); a = d; s = a.ToString("F5");
        t = d.ToString("F6"); a = d; s = a.ToString("F6");
        t = d.ToString("F7"); a = d; s = a.ToString("F7");

        d = 1054.32179;
        t = d.ToString("e3"); a = d; s = a.ToString("e3");
        t = d.ToString("e2"); a = d; s = a.ToString("e2");
        t = d.ToString("e1"); a = d; s = a.ToString("e1");
        t = d.ToString("E"); a = d; s = a.ToString("E");
        t = d.ToString("E10"); a = d; s = a.ToString("E10");

        d = -1.954891e+008;
        t = d.ToString("e0"); a = d; s = a.ToString("e0");
        t = d.ToString("e1"); a = d; s = a.ToString("e1");
        t = d.ToString("e2"); a = d; s = a.ToString("e2");
        t = d.ToString("e3"); a = d; s = a.ToString("e3");

        d = 1234567.89;
        t = d.ToString("E0"); a = d; s = a.ToString("E0");
        t = d.ToString("E1"); a = d; s = a.ToString("E1");
        t = d.ToString("E2"); a = d; s = a.ToString("E2");
        t = d.ToString("E3"); a = d; s = a.ToString("E3");
        d = -0.00001020304;
        t = d.ToString("E"); a = d; s = a.ToString("E");
        t = d.ToString("E1"); a = d; s = a.ToString("E1");
        t = d.ToString("E2"); a = d; s = a.ToString("E2");
        t = d.ToString("E3"); a = d; s = a.ToString("E3");
        t = d.ToString("E4"); a = d; s = a.ToString("E4");
        t = d.ToString("E5"); a = d; s = a.ToString("E5");


        Span<char> ss = new char[100];
        a = Math.PI;
        a.TryFormat(ss, out var ns, "G", null); s = ss.Slice(0, ns).ToString();
        a.TryFormat(ss, out ns, "G4", null); s = ss.Slice(0, ns).ToString();
        a = Math.PI * 1e50;
        a.TryFormat(ss, out ns, default, null); s = ss.Slice(0, ns).ToString();
        a.TryFormat(ss, out ns, "g7", null); s = ss.Slice(0, ns).ToString();

        for (int i = -20; i <= +20; i++)
        {
          d = Math.PI * Math.Pow(10, i);
          t = d.ToString(); a = d; s = a.ToString("G");
        }
        d = Math.PI; t = d.ToString(); a = d; s = a.ToString("G");
        d = Math.PI; t = d.ToString("G0"); a = d; s = a.ToString("G0");
        d = Math.PI; t = d.ToString("G1"); a = d; s = a.ToString("G1");
        d = Math.PI; t = d.ToString("G2"); a = d; s = a.ToString("G2");
        d = Math.PI; t = d.ToString("G3"); a = d; s = a.ToString("G3");
        d = -Math.PI; t = d.ToString(); a = d; s = a.ToString("G");
        d = -Math.PI; t = d.ToString("G3"); a = d; s = a.ToString("G3");

        d = 123.456; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1054.32179; t = d.ToString(); a = d; s = a.ToString("G");
        d = -195489100.8377; t = d.ToString(); a = d; s = a.ToString("G");
        d = -195489100; t = d.ToString(); a = d; s = a.ToString("G");
        d = -0.00123; t = d.ToString(); a = d; s = a.ToString("G");
        d = -0.123; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1234; t = d.ToString(); a = d; s = a.ToString("G");
        d = 0; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1; t = d.ToString(); a = d; s = a.ToString("G");
        d = -1; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1.0437E+21; t = d.ToString(); a = d; s = a.ToString("G");
        d = -1.0573E-05; t = d.ToString(); a = d; s = a.ToString("G");
        d = 1.01234567891E+21; t = d.ToString(); a = d; s = a.ToString("G"); //check

        d = 1054.32179;
        t = d.ToString("E"); a = d; s = a.ToString("E");
        t = d.ToString("E1"); a = d; s = a.ToString("E1");
        t = d.ToString("e2"); a = d; s = a.ToString("e2");
        t = d.ToString("E3"); a = d; s = a.ToString("E3");
        d = -195489100.8377;
        t = d.ToString("e"); a = d; s = a.ToString("e");
        t = d.ToString("E1"); a = d; s = a.ToString("E1");
        t = d.ToString("E2"); a = d; s = a.ToString("E2");
        t = d.ToString("E3"); a = d; s = a.ToString("E3");

        a = Float64.MinValue; Debug.Assert(a == double.MinValue);
        a = Float64.MaxValue; Debug.Assert(a == double.MaxValue);

        a = (double)Float32.MinValue; Debug.Assert(a == float.MinValue);
        a = (double)Float32.MaxValue; Debug.Assert(a == float.MaxValue);

        var z1 = Float16.MaxValue; Debug.Assert((double)z1 == +65504);
        var z2 = Float16.MinValue; Debug.Assert((double)z2 == -65504);

        var x2 = Float80.MaxValue;
        var x1 = Float80.MinValue;

        var t1 = Float128.MaxValue; t1 = Float128.MinValue;
        var t2 = Float256.MaxValue; t2 = Float256.MinValue;

        a = 2; d = 2; Debug.Assert((double)a == d && *(double*)&a == *(double*)&d);
        a *= 2; d *= 2; Debug.Assert((double)a == d && *(double*)&a == *(double*)&d);
        a *= 2; d *= 2; Debug.Assert((double)a == d && *(double*)&a == *(double*)&d);
        a /= 2; d /= 2; Debug.Assert((double)a == d && *(double*)&a == *(double*)&d);
        var rnd = new Random(13);
        for (int i = 0; i < 10000; i++)
        {
          d = -rnd.NextDouble() * 1e-20; a = d; Debug.Assert(*(double*)&a == *(double*)&d);
          a *= 2; d *= 2; Debug.Assert(*(double*)&a == *(double*)&d);
          a /= 2; d /= 2; Debug.Assert(*(double*)&a == *(double*)&d);
          b = a; e = d;
          a = b * b; d = e * e; Debug.Assert(*(double*)&a == *(double*)&d);
        }
        a = Math.PI; d = Math.PI; Debug.Assert((double)a == d);
        b = a * a; e = d * d; Debug.Assert((double)b == e);
        b = b / a; e = e / d; Debug.Assert((double)b == e);

      }
      static void test_str()
      {
        Float16 a; Float32 b; Float64 c; Float80 d; Float128 e; Float256 f; double g; string s;
        f = (Float256)rat.Pi(1000);
        e = (Float128)f;
        d = (Float80)e;
        c = (Float64)e;
        b = Float64.Cast<UInt32>(c);
        a = Float64.Cast<UInt16>(c);

        for (int i = -32; i < +32; i++)
        {
          g = Math.PI * Math.Pow(10, i);
          c = g; s = c.ToString();
        }
      }
      static void test_Float64()
      {
        Float64 a, b; double d, e;
        a = Math.PI; d = Math.PI; Debug.Assert((double)a == d && *(double*)&a == *(double*)&d);
        a = MathF.PI; d = MathF.PI; Debug.Assert((double)a == d && *(double*)&a == *(double*)&d);
        a = Math.PI; Debug.Assert((double)a != d);
        a = 2; d = 2; Debug.Assert((double)a == d && *(double*)&a == *(double*)&d);
        a *= 2; d *= 2; Debug.Assert((double)a == d && *(double*)&a == *(double*)&d);
        a /= 2; d /= 2; Debug.Assert((double)a == d && *(double*)&a == *(double*)&d);
        a++; d++; Debug.Assert((double)a == d);
        a--; d--; Debug.Assert((double)a == d);
        for (int i = -5; i <= +3; i++)
        {
          a += i; d += i; Debug.Assert((double)a == d);
          a -= i; d -= i; Debug.Assert((double)a == d);
        }
        a = Math.PI; d = Math.PI;
        b = Float64.Truncate(a); e = Math.Truncate(d); Debug.Assert((double)b == e);
        b = a * a; e = d * d; Debug.Assert((double)b == e);
        var t1 = *(ulong*)&b;
        var t2 = *(ulong*)&e;
        b = b / a; e = e / d; Debug.Assert((double)b == e);
      }
      static void test_Float16()
      {
#if NET7_0
        Float16 a, b; Half d, e;
        a = (Float16)(double)Half.Pi; d = (Half)(double)Half.Pi; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
        a = (Float16)Half.Pi; d = (Half)Half.Pi; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
        a = (Float16)Math.PI; Debug.Assert((Half)a == d);
        a = (Float16)2; d = (Half)2; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
        a *= (Float16)2; d *= (Half)2; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
        a /= (Float16)2; d /= (Half)2; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
        a++; d++; Debug.Assert((Half)a == d);
        a--; d--; Debug.Assert((Half)a == d);
        for (int i = -5; i <= +3; i++)
        {
          a += (Float16)i; d += (Half)i; Debug.Assert((Half)a == d);
          a -= (Float16)i; d -= (Half)i; Debug.Assert((Half)a == d);
        }
        a = (Float16)Half.Pi; d = Half.Pi;
        b = Float16.Truncate(a); e = Half.Truncate(d); Debug.Assert((Half)b == e);
        b = a * a; e = d * d; //Debug.Assert((Half)b == e);
        b = b / a; e = e / d; //Debug.Assert((Half)b == e);
#endif
      }
      static void test_Float32()
      {
        Float32 a, b; float d, e;
        a = (Float32)MathF.PI; d = MathF.PI; Debug.Assert((float)a == d && *(float*)&a == *(float*)&d);
        a = (Float32)Math.PI; Debug.Assert((float)a == d);
        a = (Float32)2; d = 2; Debug.Assert((float)a == d && *(float*)&a == *(float*)&d);
        a *= (Float32)2; d *= 2; Debug.Assert((float)a == d && *(float*)&a == *(float*)&d);
        a /= (Float32)2; d /= 2; Debug.Assert((float)a == d && *(float*)&a == *(float*)&d);
        a++; d++; Debug.Assert((float)a == d);
        a--; d--; Debug.Assert((float)a == d);
        for (int i = -5; i <= +3; i++)
        {
          a += (Float32)i; d += i; Debug.Assert((float)a == d);
          a -= (Float32)i; d -= i; Debug.Assert((float)a == d);
        }
        a = (Float32)MathF.PI; d = MathF.PI;
        b = Float32.Truncate(a); e = MathF.Truncate(d); Debug.Assert((float)b == e);
        b = a * a; e = d * d; //Debug.Assert((float)b == e);
        b = b / a; e = e / d; //Debug.Assert((float)b == e);
      }
      static void test_Float80()
      {
        Float80 a, b; Float128 d, e;
        a = Math.PI; d = Math.PI; Debug.Assert(a == d);
        a = 2; d = 2; Debug.Assert(a == d);
        a *= 2; d *= 2; Debug.Assert(a == d);
        a /= 2; d /= 2; Debug.Assert(a == d);
        a++; d++; Debug.Assert(a == d);
        a--; d--; Debug.Assert(a == d);
        for (int i = -5; i <= +3; i++)
        {
          a += i; d += i; Debug.Assert(a == d);
          a -= i; d -= i; Debug.Assert(a == d);
        }
        a = Math.PI; d = Math.PI; Debug.Assert(a == d);
        b = Float80.Truncate(a); e = Float128.Truncate(d); Debug.Assert(b == e);
      }
      static void test_Float96()
      {
        Float96 a, b; Float128 d, e;
        a = Math.PI; d = Math.PI; Debug.Assert(a == d);
        a = 2; d = 2; Debug.Assert(a == d);
        a *= 2; d *= 2; Debug.Assert(a == d);
        a /= 2; d /= 2; Debug.Assert(a == d);
        a++; d++; Debug.Assert(a == d);
        a--; d--; Debug.Assert(a == d);
        for (int i = -5; i <= +3; i++)
        {
          a += i; d += i; Debug.Assert(a == d);
          a -= i; d -= i; Debug.Assert(a == d);
        }
        a = Math.PI; d = Math.PI; Debug.Assert(a == d);
        b = Float96.Truncate(a); e = Float128.Truncate(d); Debug.Assert(b == e);
      }
      static void test_Float128()
      {
        Float128 a, b; rat d, e;
        a = Math.PI; d = new rat(Math.PI); Debug.Assert(a == d);
        a = 2; d = 2; Debug.Assert(a == d);
        a *= 2; d *= 2; Debug.Assert(a == d);
        a /= 2; d /= 2; Debug.Assert(a == d);
        a++; d++; Debug.Assert(a == d);
        a--; d--; Debug.Assert(a == d);
        for (int i = -5; i <= +3; i++)
        {
          a += i; d += i; Debug.Assert(a == d);
          a -= i; d -= i; Debug.Assert(a == d);
        }
        a = Math.PI; d = new rat(Math.PI); Debug.Assert(a == d);
        b = Float128.Truncate(a); e = rat.Truncate(d); Debug.Assert(b == e);
      }
      static void test_Float256()
      {
        Float256 a, b; rat d, e;
        a = Math.PI; d = new rat(Math.PI); Debug.Assert(a == d);
        a = 2; d = 2; Debug.Assert(a == d);
        a *= 2; d *= 2; Debug.Assert(a == d);
        a /= 2; d /= 2; Debug.Assert(a == d);
        a++; d++; Debug.Assert(a == d);
        a--; d--; Debug.Assert(a == d);
        for (int i = -5; i <= +3; i++)
        {
          a += i; d += i; Debug.Assert(a == d);
          a -= i; d -= i; Debug.Assert(a == d);
        }
        a = Math.PI; d = new rat(Math.PI); Debug.Assert(a == d);
        b = Float256.Truncate(a); e = rat.Truncate(d); Debug.Assert(b == e);
      }
      static void test_big()
      {
#if NET7_0
        var a = (Float256)rat.Pi(80);
        var b = Float256.Cast<(UInt128, UInt128, UInt64)>(a);
        var c = Float256.Cast<(UInt128, UInt128, UInt128)>(a);
        var d = Float256.Cast<(UInt128, UInt128, UInt128, UInt128)>(a); // Float512
        var e = d + d; e = e - d;
        var f = Float256.Cast<(UInt128, UInt128, UInt128, UInt128, UInt128, UInt128, UInt128, UInt128)>(a); // Float1024      
        f = f + f; f = f - f;
        var g = Float256.Cast<(UInt128, UInt128, UInt128, UInt128, UInt128, UInt128, UInt128, UInt128,
          UInt128, UInt128, UInt128, UInt128, UInt128, UInt128, UInt128, UInt128)>(a); // Float2048
        g = g + g; g = g * g; g = g * g; g = g * g; g = g * g; g = g * g; g = g / g;
#endif
      }
    }
#endif
  }
}
