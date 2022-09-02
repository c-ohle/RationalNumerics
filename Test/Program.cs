
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Test
{
  internal unsafe static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); // test();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }

#if NET6_0
    static void test()
    {
    }
#endif

#if NET7_0
    
    static void test()
    {
      //System.Numerics.Generic.UInt<uint> a = 1; a = a * a;
      //System.Numerics.Generic.Decimal<uint> b = 1; b = b * b;
 
      test_s(Math.PI); test_s(Math.PI * 1000); test_s(Math.PI * 0.00001);
      test_s(12345678); test_s(-12345678); test_s(1234000000);
      cast_ilogs(); //test_rath();

      rat r; float a; Float32 b; double c; Float64 d, x; Float80 e; Float128 f; Float256 g; string s1, s2;

      r = 1.5M; var dec = (decimal)r; Debug.Assert(r == dec); r = 0; dec = (decimal)r; Debug.Assert(r == dec);
      r = 1e32; s1 = r.ToString(); r = float.NaN; s1 = r.ToString(); r = 1e-8;
      d = r; s2 = d.ToString("F");
      d = 123.4567; s2 = d.ToString("F"); s2 = d.ToString("F10"); s2 = d.ToString("E"); s2 = d.ToString("E10");
      r = d; s1 = r.ToString("F"); s1 = r.ToString("F10"); s1 = r.ToString("E"); s1 = r.ToString("E10");

      r = 0; d = r; r = 7; d = r; r = -7; d = r;
      c = Math.PI; d = c; r = rat.Pi(20); d = r; d = Float64.Pi; //16 dig
      c = Math.Sqrt(2); d = c; r = rat.Sqrt(2, 20); d = r; d = Float64.Sqrt(2);

      a = MathF.PI; b = a; r = rat.Pi(20); b = r; b = Float32.Pi;
      a = MathF.Sqrt(2); b = a;

      r = rat.Sqrt(2, 20); b = r; b = Float32.Sqrt(2);

      x = 7; x = 7.7; r = (rat)x;

      d = Float64.Parse("123.456");

      a = MathF.PI; b = a; Debug.Assert(*(uint*)&a == *(uint*)&b);
      c = Math.PI; d = c; Debug.Assert(*(ulong*)&c == *(ulong*)&d);
      c = a; d = c; Debug.Assert(*(ulong*)&c == *(ulong*)&d);

      c = Math.PI; a = (float)c; b = (Float32)c; Debug.Assert(*(uint*)&a == *(uint*)&b);

      cast_test(1); cast_test(0.000001); cast_test(100000);
      static void cast_test(double f)
      {
        var rnd = new Random(13);
        for (int i = 0; i < 10000; i++)
        {
          var d = (0.5 - rnd.NextDouble()) * f;
          var a = (float)d;
          var b = (Float32)d;
          Debug.Assert(*(uint*)&a == *(uint*)&b);
        }
      }

      a = MathF.PI; c = a; d = a; Debug.Assert(*(ulong*)&c == *(ulong*)&d);

      d = 7.7; d = 12345.6789; d = 0.00012345; d = 1234567.0;
      d = 7.7f; d = 12345.6789f; d = 0.00012345f; d = 1234567.0f;
      e = 7.7f; e = 12345.6789f; e = 0.00012345f; e = 1234567.0f;


      a = MathF.PI; a = a * a; a = -10000 * a;
      b = Float32.Pi; b = b * b; b = -10000 * b;
      d = Float64.Pi; d = d * d; d = -10000 * d;

      b = Float32.MaxValue;
      d = Float64.MaxValue; d = Float64.Cast(b);
      d = Float64.MaxValue; d = Float64.Cast(d);
      d = Float64.MaxValue; b = Float32.Cast(d);
      d = Float64.MinValue; b = Float32.Cast(d);

      a = (float)double.MaxValue;
      a = (float)double.MinValue;
      c = +1e-300; a = (float)c; b = c;
      c = -1e-300; a = (float)c; b = c;

      d = 123; a = Float64.Sign(d); d = -123; a = Float64.Sign(d); d -= d; a = Float64.Sign(d);
      e = 123; a = Float80.Sign(e); e = -123; a = Float80.Sign(e); e -= e; a = Float80.Sign(e);
      a = MathF.PI; b = MathF.PI; c = MathF.PI; d = MathF.PI; e = MathF.PI; f = MathF.PI; g = MathF.PI;
      a = (float)Math.PI; b = Math.PI; c = Math.PI; d = Math.PI; e = Math.PI; f = Math.PI; g = Math.PI;
      a = MathF.PI; b = Float32.Pi; c = Math.PI; d = Float64.Pi; e = Float80.Pi; f = Float128.Pi; g = Float256.Pi;
      e = a; e = c;

      f = c; f = Float128.Cast(d);
      f = new Float128(1.5);

      a = MathF.Sqrt(2); b = Float32.Sqrt(2); c = Math.Sqrt(2); d = Float64.Sqrt(2);
      e = Float80.Sqrt(2); f = Float128.Sqrt(2); g = Float256.Sqrt(2);

      a = MathF.Sin(2); b = Float32.Sin(2); c = Math.Sin(2); d = Float64.Sin(2);
      e = Float80.Sin(2); f = Float128.Sin(2); g = Float256.Sin(2);

      a = MathF.Cos(2); b = Float32.Cos(2); c = Math.Cos(2); d = Float64.Cos(2);
      e = Float80.Cos(2); f = Float128.Cos(2); g = Float256.Cos(2);

      a = MathF.Pow(2, 0.5f); b = Float32.Pow(2, 0.5f); c = Math.Pow(2, 0.5f); d = Float64.Pow(2, 0.5f);
      e = Float80.Pow(2, 0.5f); f = Float128.Pow(2, 0.5f); g = Float256.Pow(2, 0.5f);

      compare(c, d);
      compare(a, b);
      static void compare<A, B>(A a, B b)
        where A : IBinaryFloatingPointIeee754<A>, IMinMaxValue<A>
        where B : IBinaryFloatingPointIeee754<B>, IMinMaxValue<B>
      {
        var c = a * a;
        var d = b * b;
      }

      test80();
      test128();
      test32();
      test64();
    }

#if false
    static void test_rath()
    {
      rat a; string b, c;

      a = rat.Parse("123.456'789000"); b = a.ToString2("G"); c = a.ToString2();

      a = rat.Parse("0.'3"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+20"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E-20"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E-9"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E-7"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E-6"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E-5"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E-4"); b = a.ToString(); c = a.ToString2(); //Debug.Assert(b == c);
      a = rat.Parse("0.'3E-3"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E-2"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E-1"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E-0"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E+1"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E+2"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E+3"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E+4"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("0.'3E+5"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);

      a = rat.Parse("123.456'789E-6"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E-5"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E-4"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E-3"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E-2"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E-1"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E-0"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+0"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+1"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+2"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+3"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+4"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+5"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+6"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+7"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      a = rat.Parse("123.456'789E+8"); b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);

      var rnd = new Random(13);
      for (int i = 0; i < 10000; i++)
      {
        var d = (0.5 - rnd.NextDouble());
        a = d; a *= a; b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
        a *= a; b = a.ToString(); c = a.ToString2(); Debug.Assert(b == c);
      }
    }
#endif
    static void cast_ilogs()
    {
      {
        var a = BigInteger.Log10(-3); var b = BigInt.Log10(-3); Debug.Assert(a.ToString() == b.ToString());
        a = BigInteger.Log10(0); b = BigInt.Log10(0); Debug.Assert(a.ToString() == b.ToString());
      }
      for (int i = 1; i < 100; i++)
      {
        var a = BigInteger.Pow(10, i); var b = BigInt.Pow(10, i);
        Debug.Assert(a == b); Debug.Assert(a.ToString() == b.ToString());
      }
      var rnd = new Random(13);
      for (int i = 1; i < 1000; i++)
      {
        var t1 = rnd.NextDouble(); var e = (int)(t1 * 100);
        var t2 = BigInt.Pow(2, e);
        var t3 = (BigInteger)t2;
        var t4 = BigInteger.Pow(2, e); Debug.Assert(t4 == t2);
        var a = BigInteger.Log10(t3);
        var b = BigInt.Log10(t2); //var c = BigRational.Log10(t2, 17);
        Debug.Assert(Math.Abs(a - b) < 0.000001);
        Debug.Assert(t2.ToString() == t4.ToString());
        Debug.Assert((-t2).ToString() == (-t4).ToString());
      }
    }
    static void test_s(double a)
    {
      Float64 b = a; float c = (float)a; Float32 d = c; string s1, s2;
      Debug.Assert(*(ulong*)&a == *(ulong*)&b);
      Debug.Assert(*(uint*)&c == *(uint*)&d);
      if (a > 0.001)
      {
        s1 = a.ToString(); s2 = b.ToString(); Debug.Assert(s1 == s2);
        s1 = a.ToString("G"); s2 = b.ToString("G"); Debug.Assert(s1 == s2);
        s1 = a.ToString("G0"); s2 = b.ToString("G0"); Debug.Assert(s1 == s2);
      }
      s1 = a.ToString("G1"); s2 = b.ToString("G1"); Debug.Assert(s1 == s2);
      s1 = a.ToString("G2"); s2 = b.ToString("G2"); Debug.Assert(s1 == s2);
      s1 = a.ToString("G3"); s2 = b.ToString("G3"); Debug.Assert(s1 == s2);
      s1 = a.ToString("G4"); s2 = b.ToString("G4"); Debug.Assert(s1 == s2);
      s1 = a.ToString("G5"); s2 = b.ToString("G5"); Debug.Assert(s1 == s2);
      s1 = a.ToString("G14"); s2 = b.ToString("G14"); Debug.Assert(s1 == s2);
      s1 = a.ToString("G15"); s2 = b.ToString("G15"); Debug.Assert(s1 == s2);
      s1 = a.ToString("G16"); s2 = b.ToString("G16"); Debug.Assert(s1 == s2);
      s1 = a.ToString("G17", NumberFormatInfo.InvariantInfo); s2 = b.ToString("", null); //d:G17

      s1 = c.ToString(); s2 = d.ToString(); Debug.Assert(s1 == s2);
      s1 = c.ToString("G"); s2 = d.ToString("G"); Debug.Assert(s1 == s2);
      s1 = c.ToString("G0"); s2 = d.ToString("G0"); Debug.Assert(s1 == s2);
      s1 = a.ToString("G2"); s2 = d.ToString("G2");
      s1 = a.ToString("G3"); s2 = d.ToString("G3");
      s1 = a.ToString("G4"); s2 = d.ToString("G4");
      s1 = a.ToString("G5"); s2 = d.ToString("G5");
      s1 = c.ToString("G9", NumberFormatInfo.InvariantInfo); s2 = d.ToString("", null); //f:G9
    }
    static void test64()
    {
      Float64 a; double d; bool x, y; string s; // double.CreateChecked

      d = double.MaxValue; a = Float64.MaxValue;

      a = Float64.NaN; s = a.ToString();
      a = Float64.NegativeInfinity; s = a.ToString();
      a = Float64.PositiveInfinity; s = a.ToString();

      d = double.NaN; a = Float64.NaN; x = double.IsNaN(d); y = Float64.IsNaN(a);
      d = double.Epsilon; a = Float64.Epsilon; a.ToString();
      d = double.NegativeInfinity; a = Float64.NegativeInfinity; x = double.IsNaN(d); y = Float64.IsNaN(a);
      //y = !Float64.IsFinite(a);
      d = double.PositiveInfinity; a = Float64.PositiveInfinity; x = double.IsNaN(d); y = Float64.IsNaN(a);
      d = double.NegativeZero; a = Float64.NegativeZero; x = double.IsNaN(d); y = Float64.IsNaN(a);
      *(ulong*)&d |= 0x8000000000000000;
      d = Math.PI; a = *(Float64*)&d;
      //float.NegativeZero
      d = double.Pi; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);

      d = double.Epsilon; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);
      d = double.NaN; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);


      d = double.NegativeInfinity; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);
      d = double.PositiveInfinity; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);
      d = double.NegativeZero; a = *(Float64*)&d; x = double.IsFinite(d); y = Float64.IsFinite(a);

      d = double.NegativeZero; a = Float64.NegativeZero;
      x = double.IsFinite(d); y = Float64.IsFinite(a);
      x = double.IsInfinity(d); y = Float64.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float64.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float64.IsNegativeInfinity(a);

      d = double.NegativeInfinity; a = Float64.NegativeInfinity;
      x = double.IsFinite(d); y = Float64.IsFinite(a);
      x = double.IsInfinity(d); y = Float64.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float64.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float64.IsNegativeInfinity(a);

      d = double.NaN; a = Float64.NaN;
      x = double.IsNaN(d); y = Float64.IsNaN(a);
      x = double.IsFinite(d); y = Float64.IsFinite(a);
      x = double.IsInfinity(d); y = Float64.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float64.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float64.IsNegativeInfinity(a);

    }
    static void test32()
    {
      Float32 a; float d; bool x, y; string s; // float.CreateChecked

      d = float.MaxValue; a = Float32.MaxValue;

      a = Float32.NaN; s = a.ToString();
      a = Float32.NegativeInfinity; s = a.ToString();
      a = Float32.PositiveInfinity; s = a.ToString();

      d = float.NaN; a = Float32.NaN; x = float.IsNaN(d); y = Float32.IsNaN(a);
      d = float.Epsilon; a = Float32.Epsilon; a.ToString();
      d = float.NegativeInfinity; a = Float32.NegativeInfinity; x = float.IsNaN(d); y = Float32.IsNaN(a);
      //y = !Float32.IsFinite(a);
      d = float.PositiveInfinity; a = Float32.PositiveInfinity; x = float.IsNaN(d); y = Float32.IsNaN(a);
      d = float.NegativeZero; a = Float32.NegativeZero; x = float.IsNaN(d); y = Float32.IsNaN(a);
      d = MathF.PI; a = *(Float32*)&d;
      //float.NegativeZero
      d = float.Pi; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);

      d = float.Epsilon; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);
      d = float.NaN; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);


      d = float.NegativeInfinity; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);
      d = float.PositiveInfinity; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);
      d = float.NegativeZero; a = *(Float32*)&d; x = float.IsFinite(d); y = Float32.IsFinite(a);

      d = float.NegativeZero; a = Float32.NegativeZero;
      x = float.IsFinite(d); y = Float32.IsFinite(a);
      x = float.IsInfinity(d); y = Float32.IsInfinity(a);
      x = float.IsPositiveInfinity(d); y = Float32.IsPositiveInfinity(a);
      x = float.IsNegativeInfinity(d); y = Float32.IsNegativeInfinity(a);

      d = float.NegativeInfinity; a = Float32.NegativeInfinity;
      x = float.IsFinite(d); y = Float32.IsFinite(a);
      x = float.IsInfinity(d); y = Float32.IsInfinity(a);
      x = float.IsPositiveInfinity(d); y = Float32.IsPositiveInfinity(a);
      x = float.IsNegativeInfinity(d); y = Float32.IsNegativeInfinity(a);

      d = float.NaN; a = Float32.NaN;
      x = float.IsNaN(d); y = Float32.IsNaN(a);
      x = float.IsFinite(d); y = Float32.IsFinite(a);
      x = float.IsInfinity(d); y = Float32.IsInfinity(a);
      x = float.IsPositiveInfinity(d); y = Float32.IsPositiveInfinity(a);
      x = float.IsNegativeInfinity(d); y = Float32.IsNegativeInfinity(a);
    }
    static void test80()
    {
      Float80 a; double d; bool x, y; string s; // double.CreateChecked

      d = double.MaxValue; a = Float80.MaxValue;
      a = 1234;
      a = Math.PI;
      a = a + a; a = a * a;

      a = Float80.NaN; s = a.ToString();
      a = Float80.NegativeInfinity; s = a.ToString();
      a = Float80.PositiveInfinity; s = a.ToString();

      d = double.NegativeZero; a = Float80.NegativeZero;
      x = double.IsFinite(d); y = Float80.IsFinite(a);
      x = double.IsInfinity(d); y = Float80.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float80.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float80.IsNegativeInfinity(a);

      d = double.NegativeInfinity; a = Float80.NegativeInfinity;
      x = double.IsFinite(d); y = Float80.IsFinite(a);
      x = double.IsInfinity(d); y = Float80.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float80.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float80.IsNegativeInfinity(a);

      d = double.NaN; a = Float80.NaN;
      x = double.IsNaN(d); y = Float80.IsNaN(a);
      x = double.IsFinite(d); y = Float80.IsFinite(a);
      x = double.IsInfinity(d); y = Float80.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float80.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float80.IsNegativeInfinity(a);

    }
    static void test128()
    {
      Float128 a; double d; bool x, y; string s; // double.CreateChecked

      d = double.MaxValue; a = Float128.MaxValue;

      a = Float128.NaN; s = a.ToString();
      a = Float128.NegativeInfinity; s = a.ToString();
      a = Float128.PositiveInfinity; s = a.ToString();

      d = double.NaN; a = Float128.NaN; x = double.IsNaN(d); y = Float128.IsNaN(a);
      d = double.Epsilon; a = Float128.Epsilon; a.ToString();
      d = double.NegativeInfinity; a = Float128.NegativeInfinity; x = double.IsNaN(d); y = Float128.IsNaN(a);
      //y = !Float128.IsFinite(a);
      d = double.PositiveInfinity; a = Float128.PositiveInfinity; x = double.IsNaN(d); y = Float128.IsNaN(a);
      d = double.NegativeZero; a = Float128.NegativeZero; x = double.IsNaN(d); y = Float128.IsNaN(a);
      d = double.Pi; a = Float128.Pi; x = double.IsFinite(d); y = Float128.IsFinite(a);

      d = double.Epsilon; a = *(Float128*)&d; x = double.IsFinite(d); y = Float128.IsFinite(a);
      d = double.NaN; a = Float128.NaN; x = double.IsFinite(d); y = Float128.IsFinite(a);


      d = double.NegativeInfinity; a = Float128.NegativeInfinity; x = double.IsFinite(d); y = Float128.IsFinite(a);
      d = double.PositiveInfinity; a = Float128.PositiveInfinity; x = double.IsFinite(d); y = Float128.IsFinite(a);
      d = double.NegativeZero; a = Float128.NegativeZero; x = double.IsFinite(d); y = Float128.IsFinite(a);

      d = double.NegativeZero; a = Float128.NegativeZero;
      x = double.IsFinite(d); y = Float128.IsFinite(a);
      x = double.IsInfinity(d); y = Float128.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float128.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float128.IsNegativeInfinity(a);

      d = double.NegativeInfinity; a = Float128.NegativeInfinity;
      x = double.IsFinite(d); y = Float128.IsFinite(a);
      x = double.IsInfinity(d); y = Float128.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float128.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float128.IsNegativeInfinity(a);

      d = double.NaN; a = Float128.NaN;
      x = double.IsNaN(d); y = Float128.IsNaN(a);
      x = double.IsFinite(d); y = Float128.IsFinite(a);
      x = double.IsInfinity(d); y = Float128.IsInfinity(a);
      x = double.IsPositiveInfinity(d); y = Float128.IsPositiveInfinity(a);
      x = double.IsNegativeInfinity(d); y = Float128.IsNegativeInfinity(a);

    }

    readonly struct Number64
    {
      public override string ToString() => p.ToString();
      public string ToString(string? format, IFormatProvider? formatProvider = null) => p.ToString(format, formatProvider);

      public static implicit operator Number64(int value) => new Number64(value);
      public static implicit operator Number64(double value) => new Number64(value);

      public static explicit operator int(Number64 value) => (int)value.p;
      public static implicit operator BigRational(Number64 value) => (BigRational)value.p;

      private Number64(System.Numerics.Generic.Float<Test.SizeType32> value) => this.p = value;
      readonly System.Numerics.Generic.Float<Test.SizeType32> p;
    }

#endif

  }
}