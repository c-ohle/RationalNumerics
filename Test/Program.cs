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

#if false //NET7_0

    static void test()
    {
      BigInteger a, aa; BigInt b, bb; BigRational u; int j, jj; long l; Int128 I; UInt128 U; double d;

      var rnd = new Random(13); l = 0;
      for (int i = 0; i < 100000; i++)
      {
        int k = rnd.Next(), s = 100 - rnd.Next(200);
        a = k; aa = a >> s;
        b = k; bb = b >> s; Debug.Assert(aa == bb);
        a = -k; aa = a >> s; //j = -k; jj = j >> s;
        b = -k; bb = b >> s; Debug.Assert(aa == bb);
        a = k; aa = a >>> s;
        b = k; bb = b >>> s; Debug.Assert(aa == bb);
        a = -k; aa = a >>> s; //j = -k; jj = j >>> s;
        b = -k; bb = b >>> s; Debug.Assert(aa == bb);
      }
      rnd = new Random(13); l = 0;
      for (int i = 0; i < 100000; i++)
      {
        long k = (long)rnd.Next() | ((long)rnd.Next() << 32); int s = 100 - rnd.Next(200);
        a = k; aa = a >> s;
        b = k; bb = b >> s; Debug.Assert(aa == bb);
        a = -k; aa = a >> s;  
        b = -k; bb = b >> s; Debug.Assert(aa == bb);
        a = k; aa = a >>> s;
        b = k; bb = b >>> s; Debug.Assert(aa == bb);
        a = -k; aa = a >>> s; 
        b = -k; bb = b >>> s; //Debug.Assert(aa == bb);
      }

      var cpu = rat.task_cpu; string s1, s2;

      I = -123;
      U = (uint)I; cpu.push(I); cpu.toc(4); cpu.toc(4); b = (BigInt)cpu.popr(); Debug.Assert(b == I);
      U = (ulong)I; cpu.push(I); cpu.toc(8); cpu.toc(8); b = (BigInt)cpu.popr(); Debug.Assert(b == I);
      U = (ushort)I; cpu.push(I); cpu.toc(2); cpu.toc(2); b = (BigInt)cpu.popr(); Debug.Assert(b == I);

      U = (byte)I; cpu.push(I); cpu.toc(1); b = (BigInt)cpu.popr();  Debug.Assert(b==U);
      U = (ushort)I; cpu.push(I); cpu.toc(2); b = (BigInt)cpu.popr(); Debug.Assert(b == U);
      U = (uint)I; cpu.push(I); cpu.toc(4); b = (BigInt)cpu.popr(); Debug.Assert(b == U);
      U = (ulong)I; cpu.push(I); cpu.toc(8); b = (BigInt)cpu.popr(); Debug.Assert(b == U);
      U = (UInt128)I; cpu.push(I); cpu.toc(16); b = (BigInt)cpu.popr(); Debug.Assert(b == U);

      if(false)
      for (I = -123; I > -260; I--)
      {
        U = (byte)I; cpu.push(I); cpu.toc(1); b = (BigInt)cpu.popr();
        s1 = ((byte)I).ToString("X");
        s2 = b.ToString("X");
        s1 = ((BigInteger)(byte)I).ToString("X"); Debug.Assert(s1 == s2);

        U = (ushort)I; cpu.push(I); cpu.toc(2); b = (BigInt)cpu.popr();
        s1 = ((ushort)I).ToString("X");
        s2 = b.ToString("X");
        s1 = ((BigInteger)(ushort)I).ToString("X"); Debug.Assert(s1 == s2);

      }


      j = -123; l = j; var ui = (uint)j; j = (int)ui; var ul = (ulong)l; l = (long)ul; I = j; U = (UInt128)I;
      cpu.push(l); cpu.toc(4); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(8); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(16); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(1); b = (BigInt)cpu.popr(); ui = (byte)j;
      cpu.push(l); cpu.toc(2); b = (BigInt)cpu.popr(); ui = (ushort)j;

      j = -1; l = j; ui = (uint)j; j = (int)ui; ul = (ulong)l; l = (long)ul; I = l; U = (UInt128)I;
      cpu.push(l); cpu.toc(4); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(8); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(16); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(1); b = (BigInt)cpu.popr(); ui = (byte)j;
      cpu.push(l); cpu.toc(2); b = (BigInt)cpu.popr(); ui = (ushort)j;

      j = unchecked((int)0xE0012345); l = j; ui = (uint)j; j = (int)ui; ul = (ulong)l; l = (long)ul; I = l; U = (UInt128)I;
      cpu.push(l); cpu.toc(4); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(8); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(16); b = (BigInt)cpu.popr();

      j = int.MinValue; l = j; ui = (uint)j; j = (int)ui; ul = (ulong)l; l = (long)ul; I = l; U = (UInt128)I;
      cpu.push(j); cpu.toc(4); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(8); b = (BigInt)cpu.popr();
      cpu.push(l); cpu.toc(16); b = (BigInt)cpu.popr();

      j = +12345; jj = j >> 3;
      a = +12345; aa = a >> 3; Debug.Assert(aa == jj);
      b = +12345; bb = b >> 3; Debug.Assert(bb == jj);
      j = -12345; jj = j >> 3;
      a = -12345; aa = a >> 3; Debug.Assert(aa == jj);
      b = -12345; bb = b >> 3; Debug.Assert(bb == jj); //todo: cpu.toc();   

      j = +12345; jj = j >> 1;
      a = +12345; aa = a >> 1; Debug.Assert(aa == jj);
      b = +12345; bb = b >> 1; Debug.Assert(bb == jj);
      j = -12345; jj = j >> 1;
      a = -12345; aa = a >> 1; Debug.Assert(aa == jj);
      b = -12345; bb = b >> 1; Debug.Assert(bb == jj); 


      j = +12345; jj = j >> 16;
      a = +12345; aa = a >> 16; Debug.Assert(aa == jj);
      b = +12345; bb = b >> 16; Debug.Assert(bb == jj);
      j = -12345; jj = j >> 16;
      a = -12345; aa = a >> 16; Debug.Assert(aa == jj);
      b = -12345; bb = b >> 16; Debug.Assert(bb == jj);

      bb = b >> 0;

      j = +12345; jj = j >>> 3;
      a = +12345; aa = a >>> 3; Debug.Assert(aa == jj);
      b = +12345; bb = b >>> 3; Debug.Assert(bb == jj);
      j = -12345; jj = j >>> 3;
      a = -12345; aa = a >>> 3; Debug.Assert(aa == jj);
      b = -12345; bb = b >>> 3; Debug.Assert(bb == jj); //todo: cpu.toc();

      bb = 88;

      b = (BigInt)100 * 10 + bb * 0x9999999999999;
      b = 0 | (BigInt)100 * 10 + bb * 0x9999999999999;

      var t = bb * 100 == b - 10 - 237790060325163126 + 8800;       //{8800} b - 10 = {237790060325163126}
      t = (0 | bb * 100) == (0 | b - 10 - 237790060325163126 + 8800);       //{8800} b - 10 = {237790060325163126}

      t = (0 | bb * 100) == (0 | b - 10 - 237790060325163126 + 8800);       //{8800} b - 10 = {237790060325163126}

      var xx = fastcalc(bb, b);
      xx = 0 | fastcalc(bb, b);

      static BigInt fastcalc(BigInt a, BigInt b)
      {
        var t = a * 100 == b - 10 - 237790060325163126 + 8800;
        return a * b + b * a;
      }

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

        //var c = new BigInt(ss, isUnsigned, isBigEndian); if (c != b) { }
      }

      for (int i = -100; i < +100; i++)
      {
        a = BigInteger.PopCount(i);
        b = long.PopCount(i);
        b = Int128.PopCount(i);
        b = int.PopCount(i);
        b = BigInteger.PopCount(i); bb = b;
        b = BigInt.PopCount(i); Debug.Assert(bb == b);

        var k = (i > 0 ? 2L : -2L) * uint.MaxValue + (long)i;
        b = long.PopCount(k);
        b = Int128.PopCount(k); bb = b;
        b = BigInteger.PopCount(k); bb = b;
        b = BigInt.PopCount(k); Debug.Assert(bb == b);

      }

      for (int i = -100; i < +100; i++)
      {
        if (i == 0) continue;
        a = BigInteger.TrailingZeroCount(i);
        b = int.TrailingZeroCount(i);
        b = long.TrailingZeroCount(i);
        b = Int128.TrailingZeroCount(i); bb = b;
        b = BigInt.TrailingZeroCount(i); Debug.Assert(bb == b);

        var k = (i > 0 ? 2L : -2L) * uint.MaxValue + (long)i;
        a = BigInteger.TrailingZeroCount(k);
        d = long.TrailingZeroCount(k);
        b = Int128.TrailingZeroCount(k); bb = b;
        b = BigInt.TrailingZeroCount(k); Debug.Assert(bb == b);
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

