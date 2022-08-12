global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;
global using BigInt = System.Numerics.BigRational.Integer;
global using Float16 = System.Numerics.BigRational.Float<System.Int16>;
global using Float32 = System.Numerics.BigRational.Float<System.Int32>;
global using Float64 = System.Numerics.BigRational.Float<System.Int64>;

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

#if NET7_0

    static void test()
    {
      Float128 a; Float80 b; double d; rat r;
      a = 8; a = Math.PI;
      b = 8; b = Math.PI;
      d = (double)a; r = a;
      d = (double)b; r = b;
      a = b;
      b = (Float80)a;

      a = test_SimpleNumber(a, b);
      a = test_SimpleNumber(b, b);

      //a = a * a;
      var x = test_INumber(2.5, 3);
      r = test_INumber((rat)2.5, 3);

      test_Float64();
      test_Float80();
      test_Float32();
      test_Float16();
      test_Float128();
    }

    static T test_INumber<T>(T a, T b) where T : INumber<T>
    {
      var t1 = a++; var t2 = a--; Debug.Assert(a == t1); Debug.Assert(a != t2);
      if (a > b) { }
      if (a <= b) { }
      if (a == b) return a * a;
      return a * b;
    }
    static T test_SimpleNumber<T>(T a, T b) where T : ISimpleNumber<T>
    {
      var t1 = a++; var t2 = a--; Debug.Assert(a == t1); Debug.Assert(a != t2);
      if (a > b) { }
      if (a <= b) { }
      if (a == b) return a * a;
      return a * b;
    }

    static void test_Float16()
    {
      Float16 a, b; Half d, e;
      a = (double)Half.Pi; d = (Half)(double)Half.Pi; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
      a = Half.Pi; d = (Half)Half.Pi; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
      a = Math.PI; Debug.Assert((Half)a == d);
      a = 2; d = (Half)2; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
      a *= 2; d *= (Half)2; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
      a /= 2; d /= (Half)2; Debug.Assert((Half)a == d && *(Half*)&a == *(Half*)&d);
      a++; d++; Debug.Assert((Half)a == d);
      a--; d--; Debug.Assert((Half)a == d);
      for (int i = -5; i <= +3; i++)
      {
        a += i; d += (Half)i; Debug.Assert((Half)a == d);
        a -= i; d -= (Half)i; Debug.Assert((Half)a == d);
      }
      a = Half.Pi; d = Half.Pi;
      b = Float16.Truncate(a); e = Half.Truncate(d); Debug.Assert((Half)b == e);
      b = a * a; e = d * d; //Debug.Assert((Half)b == e);
      b = b / a; e = e / d; //Debug.Assert((Half)b == e);
    }
    static void test_Float32()
    {
      Float32 a, b; float d, e;
      a = MathF.PI; d = MathF.PI; Debug.Assert((float)a == d && *(float*)&a == *(float*)&d);
      a = Math.PI; Debug.Assert((float)a != d);
      a = 2; d = 2; Debug.Assert((float)a == d && *(float*)&a == *(float*)&d);
      a *= 2; d *= 2; Debug.Assert((float)a == d && *(float*)&a == *(float*)&d);
      a /= 2; d /= 2; Debug.Assert((float)a == d && *(float*)&a == *(float*)&d);
      a++; d++; Debug.Assert((float)a == d);
      a--; d--; Debug.Assert((float)a == d);
      for (int i = -5; i <= +3; i++)
      {
        a += i; d += i; Debug.Assert((float)a == d);
        a -= i; d -= i; Debug.Assert((float)a == d);
      }
      a = MathF.PI; d = MathF.PI;
      b = Float32.Truncate(a); e = MathF.Truncate(d); Debug.Assert((float)b == e);
      b = a * a; e = d * d; //Debug.Assert((float)b == e);
      b = b / a; e = e / d; //Debug.Assert((float)b == e);
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
      b = Float64.Truncate(a); e = Math.Truncate(d); Debug.Assert((float)b == e);
      b = a * a; e = d * d;// Debug.Assert((double)b == e);
      var t1 = *(ulong*)&b;
      var t2 = *(ulong*)&e;
      b = b / a; e = e / d;// Debug.Assert((double)b == e);
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
#endif
  }
}
