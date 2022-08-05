global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;
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

#if NET7_0

    static void test()
    {
      BigInteger a, aa; BigInt b, bb; BigRational u, v; double d;

      testx(0x2342424234324234, true);
      testx(0x234242423432, true);
      testx(0, false);
      testx(-0x234242423432, false);
      testx(-1, false);
      testx(-0x234242423432, false);
      testx(-0x734242423432, false);
      testx(-0x834242423432, false);
      testx(-0xffffffff, false);
      testx(-0x9fffffff, false);

      testx(0x2342424234324234, true, true);
      testx(0x234242423432, true, true);
      testx(0, false, true);
      testx(-0x234242423432, false, true);
      testx(-1, false, true);
      testx(-0x234242423432, false, true);
      testx(-0x734242423432, false, true);
      testx(-0x834242423432, false, true);
      testx(-0xffffffff, false, true);
      testx(-0x9fffffff, false, true);

      static void testx(long v, bool isUnsigned = false, bool isBigEndian = false)
      {
        BigInteger a = v; BigInt b = v;
        var nt = a.GetByteCount(isUnsigned);
        var tt = new byte[nt]; a.TryWriteBytes(tt, out var nwa, isUnsigned, isBigEndian);

        var ns = b.GetByteCount(isUnsigned);
        var ss = new byte[ns]; b.TryWriteBytes(ss, out var nwb, isUnsigned, isBigEndian);
        Debug.Assert(tt.SequenceEqual(ss));

        var c = new BigInt(ss, isUnsigned, isBigEndian);
        if (c != b) { }
      }

      for (int i = -100; i < +100; i++)
      {
        a = BigInteger.PopCount(i);
        b = long.PopCount(i);
        b = Int128.PopCount(i);
        b = int.PopCount(i);
        b = BigInteger.PopCount(i); bb = b;
        b = BigInt.PopCount(i); if (bb != b) { }

        var k = (i > 0 ? 2L : -2L) * uint.MaxValue + (long)i;
        b = long.PopCount(k);
        b = Int128.PopCount(k); bb = b;
        b = BigInteger.PopCount(k); bb = b;
        b = BigInt.PopCount(k); if (bb != b) { }

      }

      for (int i = -100; i < +100; i++)
      {
        if (i == 0) continue;
        a = BigInteger.TrailingZeroCount(i);
        b = int.TrailingZeroCount(i);
        b = long.TrailingZeroCount(i);
        b = Int128.TrailingZeroCount(i); bb = b;
        b = BigInt.TrailingZeroCount(i); if (bb != b) { }

        var k = (i > 0 ? 2L : -2L) * uint.MaxValue + (long)i;
        a = BigInteger.TrailingZeroCount(k);
        d = long.TrailingZeroCount(k);
        b = Int128.TrailingZeroCount(k); bb = b;
        b = BigInt.TrailingZeroCount(k); if (bb != b) { }
      }

      u = 27;
      if (u.Equals(27)) { }
      if (u.Equals(-27)) { }
      u = Math.PI;
      if (u.Equals(27)) { }


      a = BigInteger.Log2(1);
      b = BigInt.Log2(1); b = -1; u = float.NaN; u = -2; u = 3;
      u = BigRational.Log2(1);
      d = double.Log2(1);
      b = Int128.Log2(1);

      a = BigInteger.Log2(2);
      b = BigInt.Log2(2);
      u = BigRational.Log2(2);
      d = double.Log2(2);
      b = Int128.Log2(2);

      a = BigInteger.Log2(0);
      b = BigInt.Log2(0);
      u = BigRational.Log2(0);
      d = double.Log2(0);
      b = Int128.Log2(0);

      a = BigInteger.Parse("ffff", NumberStyles.HexNumber);
      b = BigInt.Parse("ffff", NumberStyles.HexNumber);

      a = BigInteger.Parse("7ffff", NumberStyles.HexNumber);
      b = BigInt.Parse("7ffff", NumberStyles.HexNumber);

      aa = BigInteger.Log2(a);
      bb = BigInt.Log2(b);
      for (int i = 0; i < 100; i++)
      {
        a = i; b = i;
        aa = BigInteger.Log2(a);
        bb = BigInt.Log2(b); Debug.Assert(aa == bb);
        Debug.Assert(a.IsPowerOfTwo == b.IsPowerOfTwo);
      }

      for (long i = int.MaxValue + 100L; i < int.MaxValue + 1000L; i++)
      {
        a = i; b = i;
        aa = BigInteger.Log2(a);
        bb = BigInt.Log2(b); Debug.Assert(aa == bb);
        Debug.Assert(a.IsPowerOfTwo == b.IsPowerOfTwo);
      }


      a = BigInteger.Parse("8ff00", NumberStyles.HexNumber);
      b = BigInt.Parse("8ff00", NumberStyles.HexNumber);

      for (int i = -20; i <= +20; i++)
      {
        a = i; var sa = a.ToString("X8");
        b = i; var sb = b.ToString("X8");
        Debug.Assert(sa == sb);
        a = BigInteger.Parse(sb, NumberStyles.HexNumber);
        b = BigInt.Parse(sb, NumberStyles.HexNumber);
        Debug.Assert(a == b); //sb = b.ToString();

        //if (i >= 0)
        {
          aa = BigInteger.PopCount(a);
          bb = BigInt.PopCount(b); Debug.Assert(aa == bb); // Debug.WriteLine($"{b} {aa} {bb}");
          aa = BigInteger.TrailingZeroCount(a);
          bb = BigInt.TrailingZeroCount(b); Debug.Assert(i == 0 || aa == bb);
        }

        //var c = BigInteger.TrailingZeroCount(0);
        //Debug.WriteLine(c);

      }

      for (int i = -10020; i <= +120; i++)
      {
        a = i; var sa = a.ToString("x");
        b = i; var sb = b.ToString("x");
        Debug.Assert(sa == sb);
        a = BigInteger.Parse(sa, NumberStyles.HexNumber);
        b = BigInt.Parse(sb, NumberStyles.HexNumber);
        Debug.Assert(a == b); //sb = b.ToString();                      
      }

      for (int i = -10020; i <= +120; i++)
      {
        a = i; var sa = a.ToString("X3");
        b = i; var sb = b.ToString("X3");
        Debug.Assert(sa == sb);
        a = BigInteger.Parse(sa, NumberStyles.HexNumber);
        b = BigInt.Parse(sb, NumberStyles.HexNumber);
        Debug.Assert(a == b); //sb = b.ToString();
      }

      for (long i = uint.MaxValue - 1000L; i < uint.MaxValue + 1000L; i++)
      {
        a = i; var sa = a.ToString("X8");
        b = i; var sb = b.ToString("X8");
        Debug.Assert(sa == sb);
        a = BigInteger.Parse(sa, NumberStyles.HexNumber);
        b = BigInt.Parse(sb, NumberStyles.HexNumber);
        Debug.Assert(a == b); //sb = b.ToString();
        aa = BigInteger.Log2(a);
        bb = BigInt.Log2(b); Debug.Assert(aa == bb);
        Debug.Assert(a.IsPowerOfTwo == b.IsPowerOfTwo);
        if (i >= 0)
        {
          aa = BigInteger.PopCount(a);
          bb = BigInt.PopCount(b); Debug.Assert(aa == bb);
          aa = BigInteger.TrailingZeroCount(a);
          bb = BigInt.TrailingZeroCount(b); Debug.Assert(aa == bb);
        }
      }
      for (long i = uint.MaxValue - 1000L; i < uint.MaxValue + 1000L; i++)
      {
        a = -i; var sa = a.ToString("X8");
        b = -i; var sb = b.ToString("X8");
        Debug.Assert(sa == sb);
        a = BigInteger.Parse(sa, NumberStyles.HexNumber);
        b = BigInt.Parse(sb, NumberStyles.HexNumber);
        Debug.Assert(a == b); //sb = b.ToString();
        //if (i >= 0)
        {
          aa = BigInteger.PopCount(a);
          bb = BigInt.PopCount(b); //Debug.Assert(aa == bb);
          aa = BigInteger.TrailingZeroCount(a);
          bb = BigInt.TrailingZeroCount(b); Debug.Assert(aa == bb);
        }
      }
    }
#endif
  }
}

