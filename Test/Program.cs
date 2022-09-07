
using System.Diagnostics;
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
      ApplicationConfiguration.Initialize(); // test();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }

#if NET6_0
    static void test()
    {
      object p;
      //p = __uint128.CreateTruncating("asdad");
      p = __uint128.CreateTruncating(64ul);
      p = __uint128.CreateTruncating((__uint32)32);
      p = __uint128.CreateTruncating((rat)32);
      p = __uint128.CreateTruncating((byte)1);
      p = __uint128.CreateTruncating((decimal)1234567890);
      p = __uint128.CreateTruncating(123.45f);
      p = __float80.CreateTruncating(123.45);
      p = __float80.CreateTruncating((decimal)123456.123);
      p = __float80.CreateSaturating(Half.MaxValue);
      p = rat.CreateChecked(MathF.PI);
      p = rat.CreateChecked(Math.PI);
      p = rat.CreateChecked(Float32.Pi);
      p = rat.CreateChecked(Float64.Pi);
      p = rat.CreateChecked(Float128.Pi);
      p = rat.CreateChecked(Float256.Pi);
      p = rat.CreateChecked(Float512.Pi);
      p = __int128.CreateSaturating(uint.MaxValue);
      p = __int128.CreateSaturating(__int64.MaxValue);
      p = __int128.CreateSaturating(long.MaxValue);
      p = __int128.CreateSaturating(long.MinValue);
      p = __int128.CreateSaturating(ulong.MaxValue);
      p = __int64.CreateSaturating(__int128.MaxValue);
      p = __int64.CreateSaturating(__int128.MinValue);
      p = __int64.CreateSaturating((BigInteger)125555555555555);
      p = __uint128.CreateTruncating((BigInteger)125555555555555);
      p = __int64.CreateSaturating((BigInt)125555555555555);
      p = __uint128.CreateTruncating((BigInt)125555555555555);
      p = __int64.CreateSaturating(nint.MinValue);
      p = __uint128.CreateTruncating(nuint.MaxValue);
      p = BigInt.CreateTruncating(879899);
    }
#endif

#if NET7_0
    static void test_conv()
    {
      object p;

      p = Float128.CreateTruncating(Half.Pi);
      p = Float128.CreateTruncating(float.Pi);
      p = Float128.CreateTruncating(double.Pi);
      p = Float128.CreateTruncating(Float32.Pi);
      p = Float128.CreateTruncating(Float64.Pi);
      p = Float128.CreateTruncating(Float80.Pi);
      p = Float128.CreateTruncating(Float96.Pi);
      p = Float128.CreateTruncating(Float128.Pi);
      p = Float128.CreateTruncating(Float256.Pi);
      p = Float128.CreateTruncating(Float512.Pi);
      p = Float128.CreateTruncating(NFloat.Pi);
     
      p = Float128.CreateTruncating(Half.MaxValue);
      p = Float128.CreateTruncating(float.MaxValue);
      p = Float128.CreateTruncating(double.MaxValue);
      p = Float128.CreateTruncating(Float80.MaxValue);
      p = Float128.CreateTruncating(Float96.MaxValue);
      p = Float128.CreateTruncating(UInt128.MaxValue);
      p = Float128.CreateTruncating(Int128.MaxValue);
      p = Float128.CreateTruncating(Int128.MinValue);

      var x1 = Half.MaxValue; var s = Half.MaxValue.ToString();
      var x2 = (double)Half.MaxValue;
      p = __uint128.CreateTruncating(64ul);
      p = __uint128.CreateTruncating((__uint32)32);
      p = __uint128.CreateTruncating((rat)32);
      p = __uint128.CreateTruncating((byte)1);
      p = __uint128.CreateTruncating((decimal)1234567890);
      p = __uint128.CreateTruncating(123.45f);
      p = __float80.CreateTruncating(123.45);
      p = __float80.CreateTruncating((decimal)123456.123);
      p = __float80.CreateSaturating(Half.MaxValue);
      
      p = rat.CreateChecked(MathF.PI);
      p = rat.CreateChecked(Math.PI);
      p = rat.CreateChecked(Float32.Pi);
      p = rat.CreateChecked(Float64.Pi);
      p = rat.CreateChecked(Float128.Pi);
      p = rat.CreateChecked(Float256.Pi);
      p = rat.CreateChecked(Float512.Pi);

      p = __int128.CreateSaturating(uint.MaxValue);
      p = __int128.CreateSaturating(__int64.MaxValue);
      p = __int128.CreateSaturating(long.MaxValue);
      p = __int128.CreateSaturating(long.MinValue);
      p = __int128.CreateSaturating(ulong.MaxValue);
      p = __int64.CreateSaturating(__int128.MaxValue);
      p = __int64.CreateSaturating(__int128.MinValue);
      p = __int64.CreateSaturating((BigInteger)125555555555555);
      p = __uint128.CreateTruncating((BigInteger)125555555555555);
      p = __int64.CreateSaturating((BigInt)125555555555555);
      p = __uint128.CreateTruncating((BigInt)125555555555555);
      p = __int64.CreateSaturating(nint.MinValue);
      p = __uint128.CreateTruncating(nuint.MaxValue);
      p = BigInt.CreateTruncating(879899);

      p = double.CreateTruncating(1.5f);
      p = BigInteger.CreateTruncating(1.5f);
      p = Half.CreateTruncating(1.5f);

      p = __uint128.CreateTruncating(64ul);
      p = __uint128.CreateTruncating((__uint32)32);
      p = __uint128.CreateTruncating((rat)32);
      p = __uint128.CreateTruncating((byte)1);
      p = __uint128.CreateTruncating((decimal)1234567890);
      p = __uint128.CreateTruncating(123.45f);
     
      p = float.CreateTruncating((__uint128)p);
      p = float.CreateTruncating((__int128)6868768786);
      p = double.CreateTruncating((Float80)123.456);
      p = ulong.CreateTruncating((Float80)123.456);
      p = UInt128.CreateTruncating((Float128)123.456);
      p = Int128.CreateTruncating((Float32)123.456);
      p = decimal.CreateTruncating((Float32)123.456);

      //var t1 = Half.MaxValue.ToString();
      //var t2 = Half.Parse(t1); if(t2 == Half.MaxValue) { }

      p = Float128.CreateTruncating(1.5f);
      p = __float80.CreateSaturating(1.5f);
      var e = __uint128.CreateTruncating((__uint32)32);
      var a = UInt128.CreateTruncating(123);
      var b = __uint128.CreateTruncating(123);
      var c = __uint128.CreateTruncating(a);
      var d = __uint64.CreateTruncating(12345);
      b = __uint128.CreateTruncating(d);
      c = __uint128.CreateTruncating((__uint512)12345);
    }
    static void test()
    {
      test_conv();
      test_uint2();
      test_uint();
      test_int();

      __float80 f1; f1 = 123.456; f1 = f1 * 1000;
      var xx = __float80.Pi;
      compareeq(f1, f1);

      //gamma();

      test_s(Math.PI); test_s(Math.PI * 1000); test_s(Math.PI * 0.00001);
      test_s(12345678); test_s(-12345678); test_s(1234000000);
      cast_ilogs();

      rat r; float a; Float32 b; double c; Float64 d, x; Float80 e; Float128 f; Float256 g; string s1, s2;

      e = 1.2346; var ee = e + 0.0000001; if (e < ee) { }

      for (int i = 1; i < 13; i++) r = (rat)i / 13;
      for (int i = 1; i < 19; i++) r = (rat)i / 19;
      for (int i = 1; i < 7; i++) r = (rat)i / 7;

      var rnd = new Random(13);
      for (int i = 0; i < 1000; i++)
      {
        var t1 = (0.5 - rnd.NextDouble()) * 100;
        var t2 = (0.5 - rnd.NextDouble()) * 100; if ((i % 50) == 0) if ((i % 100) == 0) t1 = t2 = 0; else t1 = t2;
        var x1 = t1.CompareTo(t2);
        var x2 = (*(Float64*)&t1).CompareTo((*(Float64*)&t2));
        Debug.Assert(x1 == x2);

        Debug.Assert((t1 < t2) == (*(Float64*)&t1 < *(Float64*)&t2));
        Debug.Assert((t1 > t2) == (*(Float64*)&t1 > *(Float64*)&t2));
        Debug.Assert((t1 <= t2) == (*(Float64*)&t1 <= *(Float64*)&t2));
        Debug.Assert((t1 >= t2) == (*(Float64*)&t1 >= *(Float64*)&t2));
        Debug.Assert((t1 == t2) == (*(Float64*)&t1 == *(Float64*)&t2));
        Debug.Assert((t1 != t2) == (*(Float64*)&t1 != *(Float64*)&t2));
        compareeq(t1, (*(Float64*)&t1));
      }
      for (int i = 0; i < 1000; i++)
      {
        var t1 = (float)((0.5 - rnd.NextDouble()) * 100);
        var t2 = (float)((0.5 - rnd.NextDouble()) * 100); if ((i % 50) == 0) if ((i % 100) == 0) t1 = t2 = 0; else t1 = t2;
        var x1 = t1.CompareTo(t2);
        var x2 = (*(Float32*)&t1).CompareTo((*(Float32*)&t2));
        Debug.Assert(x1 == x2);
        compareeq(t1, (*(Float32*)&t1));
      }
      static void compareeq<A, B>(A a, B b)
        where A : IBinaryFloatingPointIeee754<A>, IMinMaxValue<A>
        where B : IBinaryFloatingPointIeee754<B>, IMinMaxValue<B>
      {
        Debug.Assert(A.Radix == B.Radix); int c, d;
        c = a.GetSignificandBitLength(); d = b.GetSignificandBitLength(); Debug.Assert(c == d);
        c = a.GetSignificandByteCount(); d = b.GetSignificandByteCount(); Debug.Assert(c == d);
        c = a.GetExponentByteCount(); d = b.GetExponentByteCount(); Debug.Assert(c == d);
        c = a.GetExponentShortestBitLength(); d = b.GetExponentShortestBitLength(); Debug.Assert(c == d);
        //var x = A.AllBitsSet; var y = B.AllBitsSet; Debug.Assert(x. == y);
      }

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
    static void test_uint<A, B>(A a, B b)
      where A : IBinaryInteger<A>, IUnsignedNumber<A>, IMinMaxValue<A>
      where B : IBinaryInteger<B>, IUnsignedNumber<B>, IMinMaxValue<B>
    {
      var s = a.ToString(); var t = b.ToString(); Debug.Assert(s == t);
      s = a.ToString("X", null);
      var e = A.Parse(s, NumberStyles.HexNumber, null); Debug.Assert(e == a);
      t = b.ToString("X", null);
      var x = B.Parse(s, NumberStyles.HexNumber, null);
      var c = a + a;
      var d = b + b; Debug.Assert(c.ToString("X", null) == d.ToString("X", null));
      var t1 = A.PopCount(a); var t2 = B.PopCount(b); Debug.Assert(t1.ToString() == t2.ToString());
      var i1 = a.GetShortestBitLength(); var i2 = b.GetShortestBitLength(); Debug.Assert(i1 == i2);
    }
    static void test_uint()
    {
      __uint128 a, b, c; uint d; UInt128 e; string s;// rat r;

      e = 0x1234abcd; a = 0x1234abcd; 
      for (int i = 0; i < 200; i++)
      {
        e = UInt128.RotateRight(e, 1);
        a = __uint128.RotateRight(a, 1); Debug.Assert(e == a);
      }
      e = 0x1234abcd; a = 0x1234abcd;
      for (int i = 0; i < 200; i++)
      {
        e = UInt128.RotateLeft(e, 1);
        a = __uint128.RotateLeft(a, 1); Debug.Assert(e == a);
      }

      e = UInt128.RotateLeft(0, 27);
      a = __uint128.RotateLeft(0, 27); Debug.Assert(e == a);

      e = UInt128.RotateLeft(10, -27);
      a = __uint128.RotateLeft(10, -27); Debug.Assert(e == a);

      e = 0x1234; e = UInt128.RotateRight(e, 10);
      a = 0x1234; a = __uint128.RotateRight(a, 10); Debug.Assert(e == a);
      e = 0xff00ff001234ff; e = UInt128.RotateRight(e, 10);
      a = 0xff00ff001234ff; a = __uint128.RotateRight(a, 10); Debug.Assert(e == a);
      e = UInt128.RotateLeft(e, 10);
      a = __uint128.RotateLeft(a, 10); Debug.Assert(e == a);

      e = 3252426434636; var aa = UInt128.DivRem(e, 234242);
      a = 3252426434636; var bb = __uint128.DivRem(a, 234242); Debug.Assert(aa.Quotient == bb.Quotient && aa.Remainder == bb.Remainder);
       
      e = 10; aa = UInt128.DivRem(e, 3);
      a = 10; bb = __uint128.DivRem(a, 3); Debug.Assert(aa.Quotient == bb.Quotient && aa.Remainder == bb.Remainder);

      Debug.Assert(aa == bb);

      e = (UInt128)1232424232423456.79m;
      a = __uint128.CreateChecked(e);
      a = (__uint128)1232424232423456.79m;
      var x1 = UInt128.LeadingZeroCount(e);
      var x2 = __uint128.LeadingZeroCount(a);
      test_uint(e, a);
      e <<= 19; a <<= 19; Debug.Assert(e == a);
      e >>= 50; a >>= 50; Debug.Assert(e == a);
      e |= 0xfffff0000000000;
      a |= 0xfffff0000000000; Debug.Assert(e == a);
      e ^= 0x00034A345000000;
      a ^= 0x00034A345000000; Debug.Assert(e == a);
      e &= 0x03ffffff5234000;
      a &= 0x03ffffff5234000;
      e = ~e;
      a = ~a; Debug.Assert(e == a);
      e ^= e; a ^= a; Debug.Assert(e == a);

      
      a = 1000; b = 88; d = (uint)b; c = a * b + 1;
      a++; a--; a = UInt64.MaxValue; a = UInt64.MinValue; a = UInt128.MaxValue;
      a = 100; b = 200; if (a > b) { }
      a = (UInt128.MaxValue / UInt64.MaxValue);
      a = (UInt128)(Int128.MaxValue / Int64.MaxValue);
      a = UInt128.MaxValue; b = UInt64.MaxValue; c = a / b;//18446744073709551617

      a = 100; b = 100; if (a > b) { }
      if (a <= b) { }
      if (a > b) { }
      a = 0; b = 100; if (a > b) { }
      if (a < b) { }

      e = (UInt128)(-1); a = (__uint128)(-1);
      e = (UInt128)a;
      s = e.ToString("X"); s = a.ToString("X");
      e = UInt64.MaxValue; s = e.ToString("X");
      a = UInt64.MaxValue; s = a.ToString("X");
      a = UInt128.MaxValue; s = a.ToString("X");
      e = (UInt128)decimal.MaxValue; s = e.ToString("X"); a = e;
      a = (UInt128)decimal.MaxValue; s = a.ToString("X");
      s = a.ToString("X4"); a = 0x00ffffff; s = a.ToString("X4"); s = a.ToString("X8");
      e = (UInt128)a; s = e.ToString("X8");
      var b1 = UInt128.IsPow2(e); var b2 = __uint128.IsPow2(e); Debug.Assert(b1 == b2);
      a = 0; e = (UInt128)a; b1 = UInt128.IsPow2(e); b2 = __uint128.IsPow2(e); Debug.Assert(b1 == b2);
      a = 12345; e = (UInt128)a; c = UInt128.Log2(e); b = __uint128.Log2(e);

      a = (UInt128)decimal.MaxValue / 2; e = (UInt128)a; c = UInt128.Log2(e); b = __uint128.Log2(a);
      a = 0; e = (UInt128)a; c = UInt128.Log2(e); b = __uint128.Log2(a);

    }
    static void test_uint2()
    {
      UInt128 a; __uint128 b; string s, t;
      for (uint i = 0; i < 0x2ff; i++)
      {
        a = i; s = a.ToString("X");
        b = i; t = b.ToString("X"); Debug.Assert(s == t);
      }
      for (uint i = 0; i < 0x2ff; i++)
      {
        a = i; s = a.ToString("X2");
        b = i; t = b.ToString("X2"); Debug.Assert(s == t);
      }
      for (uint i = 0; i < 0x2ff; i++)
      {
        a = i; s = a.ToString("X4");
        b = i; t = b.ToString("X4"); Debug.Assert(s == t);
      }
      for (uint i = 0; i < 100; i++)
      {
        var k = i * 0x678979877834535ul;
        a = k; s = a.ToString("X8");
        b = k; t = b.ToString("X8"); Debug.Assert(s == t);
      }
    }
    static void test_int()
    {
      __int128 a, b, c; int d; Int128 e;

      e = 0x23424242;
      a = 0x23424242;
      e = -e; a = -a;
      e = 0xe3424242;
      a = 0xe3424242; // todo: ToString("x")
      
      Debug.Assert(Int128.Log2(4) == __int128.Log2(4));
      Debug.Assert(Int128.Log2(0) == __int128.Log2(0));
      Debug.Assert(Int128.Log2(897897978978997997) == __int128.Log2(897897978978997997));
 
      Debug.Assert(Int128.IsPow2(4) == __int128.IsPow2(4));
      Debug.Assert(Int128.IsPow2(-4) == __int128.IsPow2(-4));
      Debug.Assert(Int128.IsPow2(-89797997) == __int128.IsPow2(-89797997));
      Debug.Assert(Int128.IsPow2(897897978978997997)== __int128.IsPow2(897897978978997997));
      Debug.Assert(Int128.IsPow2(-897897978978997997) == __int128.IsPow2(-897897978978997997));

      e = 3252426434636; var s = Int128.DivRem(e, 234242);
      a = 3252426434636; var t = __int128.DivRem(a, 234242); Debug.Assert(s.Quotient == t.Quotient && s.Remainder == t.Remainder);
       
      e = -3252426434636; s = Int128.DivRem(e, 234242);
      a = -3252426434636; t = __int128.DivRem(a, 234242); Debug.Assert(s.Quotient == t.Quotient && s.Remainder == t.Remainder);

      s = Int128.DivRem(100, 3); t = __int128.DivRem(100, 3); Debug.Assert(s.Quotient == t.Quotient && s.Remainder == t.Remainder);
      s = Int128.DivRem(-100, 3); t = __int128.DivRem(-100, 3); Debug.Assert(s.Quotient == t.Quotient && s.Remainder == t.Remainder);
      s = Int128.DivRem(100, -3); t = __int128.DivRem(100, -3); Debug.Assert(s.Quotient == t.Quotient && s.Remainder == t.Remainder);
      s = Int128.DivRem(-100, -3); t = __int128.DivRem(-100, -3); Debug.Assert(s.Quotient == t.Quotient && s.Remainder == t.Remainder);
      s = Int128.DivRem(0, 3); t = __int128.DivRem(0, 3); Debug.Assert(s.Quotient == t.Quotient && s.Remainder == t.Remainder);
      s = Int128.DivRem(0, -3); t = __int128.DivRem(0, -3); Debug.Assert(s.Quotient == t.Quotient && s.Remainder == t.Remainder);
       
      a = 1234; b = -1234; d = (int)b;
      b = 0; d = (int)b; a = long.MaxValue; b = long.MinValue;
      a = Int128.MaxValue; b = Int128.MinValue;
      a = 1000; b = 7; c = -a; c = -b; c = a + b; c = a - b; c = a * b; c = a / b;
      a = -1000; b = -7; c = -a; c = -b; c = a + b; c = a - b; c = a * b; c = a / b;

      a = 1234; b = -1234; if (a > b) { }
      if (a <= b) { }
      a = 12345; b = 1234; if (a > b) { }
      if (a <= b) { }
      a = 123; b = 123; if (a > b) { }
      if (a <= b) { }
      a = -123; b = -123; if (a > b) { }
      if (a <= b) { }
      a = -1234; b = -123; if (a > b) { }
      if (a <= b) { }
      a = 0; b = -123; if (a > b) { }
      a = 0; b = +123; if (a > b) { }
      b = 0; a = -123; if (a > b) { }
      b = 0; a = +123; if (a > b) { }

      Debug.Assert(Int128.IsPow2(-4) == __int128.IsPow2(-4));
      Debug.Assert(Int128.IsPow2(0) == __int128.IsPow2(0));
      Debug.Assert(Int128.IsPow2(1) == __int128.IsPow2(1));
      Debug.Assert(Int128.IsPow2(2) == __int128.IsPow2(2));
      Debug.Assert(Int128.IsPow2(3) == __int128.IsPow2(3));
      Debug.Assert(Int128.IsPow2(4) == __int128.IsPow2(4));
      Debug.Assert(Int128.IsPow2(5) == __int128.IsPow2(5));
    }
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
    static void gamma()
    {
      rat z = (rat)3 / 2;
      int n = 1500;

      Float128 f = 1; for (int i = 2; i <= n; i++) { f *= i; }
      var p = rat.Factorial(n);

      f = Float128.Pow(n, z);
      p = rat.Pow(n, z);

      var num = rat.Factorial(n) * rat.Pow(n, z);
      var den = z; for (int i = 1; i <= n; i++) den = den * (z + i);
      var g = num / den;
      //0.8862269254527580136490837416705725913987747280611935641069038949
    }

#endif

  }
}