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

#if false
    static void test()
    {
      BigInteger a, aa; BigInt b, bb;
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
          bb = BigInt.TrailingZeroCount(b); Debug.Assert(aa == bb);
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
          bb = BigInt.PopCount(b); Debug.Assert(aa == bb);
          aa = BigInteger.TrailingZeroCount(a);
          bb = BigInt.TrailingZeroCount(b); Debug.Assert(aa == bb);
        }
      }
    }
#endif
  }
}

