global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;
global using Float16 = System.Numerics.BigRational.Float<System.UInt16>;
global using Float32 = System.Numerics.BigRational.Float<System.UInt32>;

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
      Float64 a, b; double d, e;

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

      test_Float64();
      test_Float32();
      test_Float80();
      test_Float16();
      test_Float96();
      test_Float128();
      test_Float256();
      test_str();
      test_big();

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
