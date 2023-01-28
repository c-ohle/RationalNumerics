
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Test
{
  internal unsafe static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); //test();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }

    static void test()
    {
      test_log();
      test_exp(); //4.8
      test_rounds();
      test_tos();
      static void test_log()
      {
        double a; BigRat b; rat r; //, c, d, z; string s;

        r = rat.Log2(1.234, 20);
        b = BigRat.Log2(1.234, 20);// BigRat.Normalize(1.234), 20);
        r = rat.Log2(Math.E, 20);
        b = BigRat.Log2(Math.E, 20);

        a = Math.Log(1.234);
        r = rat.Log(1.234, 20);
        b = BigRat.Log(1.234, 20);
        b = BigRat.Log(1.234, 500);
        var s = b.ToString("Q1000");
        //0.21026092548319607136082943601527476998663058511279952672610238831073031555349357511057494302632237237330031845848044124246550057062593110062548373496664892064094190787271590207840143550597519300643847685017807312653528239040163649841602031594149790436352051125536486811504352777856803787667477787672280193777016870566736501271702897533470750414519643876916995004759095858391002479556171146857789765457662493308574590100659465114950016333030550031421612643834686312425877379845181240384200203389926002196400295742956569673318525362673493792713956854402325159454534796274807613490827052286966473879978811023635119017984412193569903357628109766912273071186675999374962308300370651872311481878489457601336261759885496186194965297509530177389517756613080376920444275468234008328303942380145313460000897712485653170563406801131031436122142091815979374896332334213666273620966723997881289454922849027321285484826153366455411859013315801930617126946207975997341946444367357110636374846023930261735383653262280690447364023597482563503322957261338...
        //0,21026092548319607136082943601527476998663058511279952672610238831073031555349357511057494302632237237330031845848044124246550057062593110062548373496664892064094190787271590207840143550597519300643847685017807312653528239040163649841602031594149790436352051125536486811504352777856803787667477787672280193777016870566736501271702897533470750414519643876916995004759095858391002479556171146857789765457662493308574590100659465114950016333030550031421612643834686312425877379845181240384200203389926002
        //0,21026092548319607136082943601527476998663058511279952672610238831073031555349357511057494302632237237330031845848044124246550057062593110062548373496664892064094190787271590207840143550597519300643847685017807312653528239040163649841602031594149790436352051125536486811504352777856803787667477787672280193777016870566736501271702897533470750414519643876916995004759095858391002479556171146857789765457662493308574590100659465114950016333030550031421612643834686312425877379845181240384200203389926002
      }
      static void test_exp()
      {
        test(1); test(100); test(1000); test(0.01);
        test(0.00000123); test(-9.8765432); test(-123.456789);
        test(-0.0123456); test(-0.000001234567);
        static void test(BigRat z)
        {
          //z = BigRat.Normalize(z);
          var a = Math.Exp((double)z); BigRat b, c, d; //rat r;
          b = BigRat.Exp(z, 500); if (double.IsFinite(a)) Debug.Assert(a.ToString("G15") == b.ToString("G15"));
          for (int i = 0; i < 300; i++)
          {
            c = BigRat.Exp(z, i);
            d = BigRat.Round(b, i - BigRat.ILog10(b) - 1); Debug.Assert(c == d);
          }
        }
      }
      static void test_rounds()
      {
        double a, b; BigRat c;
        var aa = new double[] { 0, 123, -123, 123.456, -123.456, 1.1, 1.8, -1.1, -1.8, 0.1, -0.1 };
        for (int i = 0; i < aa.Length; i++)
        {
          a = aa[i];
          b = Math.Truncate(a); c = BigRat.Truncate(a); Debug.Assert(b == c);
          b = Math.Floor(a); c = BigRat.Floor(a); Debug.Assert(b == c);
          b = Math.Ceiling(a); c = BigRat.Ceiling(a); Debug.Assert(b == c);
          b = Math.Round(a); c = BigRat.Round(a); Debug.Assert(b == c);
        }
      }
      static void test_tos()
      {
        double a; BigRat b; string sa, sb;
        a = Math.PI;
        b = BigRat.Pi(1000);
        for (int k = 16; k >= 1; k--)  //17
        {
          var fmt = ((k & 1) != 0 ? "G" : "g") + k;
          for (int i = -10; i <= 20; i++)
          {
            var p = Math.Pow(10, i); if ((i & 1) != 0) p = -p;
            sa = (a * p).ToString(fmt);
            sb = (b * p).ToString(fmt); Debug.Assert(sa == sb);
            for (int j = 5; j < 8; j++) //
            {
              sa = (Math.Round(a, j) * p).ToString(fmt);
              sb = (BigRat.Round(b, j) * p).ToString(fmt); Debug.Assert(sa == sb);
            }
          }
        }

        sa = (0.0).ToString(); sb = ((BigRat)0).ToString(); Debug.Assert(sa == sb);
        sa = (0.0).ToString("G10"); sb = ((BigRat)0).ToString("G10"); Debug.Assert(sa == sb);

        sa = a.ToString("E");
        sb = b.ToString("E"); Debug.Assert(sa == sb);
        sa = a.ToString("E0");
        sb = b.ToString("E0"); Debug.Assert(sa == sb);
        sa = a.ToString("E13");
        sb = b.ToString("E13"); Debug.Assert(sa == sb);
        sa = (a * 100).ToString("E13");
        sb = (b * 100).ToString("E13"); Debug.Assert(sa == sb);
        sa = (a / 100).ToString("E13");
        sb = (b / 100).ToString("E13"); Debug.Assert(sa == sb);
        sa = (a * 1000000).ToString("E4");
        sb = (b * 1000000).ToString("E4"); Debug.Assert(sa == sb);
        sa = (0.0).ToString("E");
        sb = ((BigRat)0).ToString("E"); Debug.Assert(sa == sb);
        sa = (1.234).ToString("E8");
        sb = ((BigRat)1.234).ToString("E8"); Debug.Assert(sa == sb);
        sa = (1.234).ToString("E5");
        sb = ((BigRat)1.234).ToString("E5"); Debug.Assert(sa == sb);
        sa = (1234).ToString("E4");
        sb = ((BigRat)1234).ToString("E4"); Debug.Assert(sa == sb);
        sa = (1.234).ToString("E3");
        sb = ((BigRat)1.234).ToString("E3"); Debug.Assert(sa == sb);
        sa = (1234e18).ToString("E4");
        sb = ((BigRat)1234e18).ToString("E4"); Debug.Assert(sa == sb);

        sa = a.ToString("F");
        sb = b.ToString("F"); Debug.Assert(sa == sb);
        sa = a.ToString("F0");
        sb = b.ToString("F0"); Debug.Assert(sa == sb);
        sa = (a * 0).ToString("F5");
        sb = (b * 0).ToString("F5"); Debug.Assert(sa == sb);

        sa = (a * 100).ToString("F8");
        sb = (b * 100).ToString("F8"); Debug.Assert(sa == sb);
        sa = (a / 100).ToString("F8");
        sb = (b / 100).ToString("F8"); Debug.Assert(sa == sb);
        sa = (a * 1e10).ToString("F8");
        sb = (b * 1e10).ToString("F8"); //Debug.Assert(sa == sb);
        sa = (a * 1e20).ToString("F8");
        sb = (b * 1e20).ToString("F8"); //Debug.Assert(sa == sb);

        sa = (a * 1e100).ToString("F8");
        sb = (b * 1e100).ToString("F8"); //Debug.Assert(sa == sb);
        sa = (a * 1e200).ToString("F8");
        sb = (b * 1e200).ToString("F8"); //Debug.Assert(sa == sb);
        sa = (a / 1e100).ToString("F8");
        sb = (b / 1e100).ToString("F8"); Debug.Assert(sa == sb);
        sa = (a / 1e200).ToString("F8");
        sb = (b / 1e200).ToString("F8"); Debug.Assert(sa == sb);

        sa = a.ToString("N");
        sb = b.ToString("N"); Debug.Assert(sa == sb);
        sa = a.ToString("N10");
        sb = b.ToString("N10"); Debug.Assert(sa == sb);
        sa = (a * 100).ToString("N10");
        sb = (b * 100).ToString("N10"); Debug.Assert(sa == sb);
        sa = (a / 100).ToString("N10");
        sb = (b / 100).ToString("N10"); Debug.Assert(sa == sb);

        var info = CultureInfo.GetCultureInfo("en-US");
        sa = a.ToString("N10", info);
        sb = b.ToString("N10", info); Debug.Assert(sa == sb);
        sa = (a * 1000000000).ToString("N5", info);
        sb = (b * 1000000000).ToString("N5", info); Debug.Assert(sa == sb);
        sa = (a / 1000000000).ToString("N10", info);
        sb = (b / 1000000000).ToString("N10", info); Debug.Assert(sa == sb);

        info = CultureInfo.InvariantCulture;
        sa = (a * 1000000000).ToString("N5", info);
        sb = (b * 1000000000).ToString("N5", info); Debug.Assert(sa == sb);
        info = CultureInfo.GetCultureInfo("de-DE");
        sa = (a * 1000000000).ToString("N5", info);
        sb = (b * 1000000000).ToString("N5", info); Debug.Assert(sa == sb);
        info = CultureInfo.GetCultureInfo("sv-SE");
        sa = (a * 1000000000).ToString("N5", info);
        sb = (b * 1000000000).ToString("N5", info); Debug.Assert(sa == sb);

        sa = (a * 1e100).ToString("N8");
        sb = (b * 1e100).ToString("N8"); //Debug.Assert(sa == sb);

        sa = (Math.Round(a, 4)).ToString("N8");
        sb = (BigRat.Round(b, 4)).ToString("N8"); Debug.Assert(sa == sb);
        sa = (Math.Round(a, 4) * 1000).ToString("N8");
        sb = (BigRat.Round(b, 4) * 1000).ToString("N8"); Debug.Assert(sa == sb);

        sb = (BigRat.Round(b, 20)).ToString("R"); var t = BigRat.Parse(sb); Debug.Assert(BigRat.Round(b, 20) == t);
        sb = (BigRat.Round(b, 20)).ToString("R0"); t = BigRat.Parse(sb); Debug.Assert(BigRat.Round(b, 20) == t);
        sb = (BigRat.Round(b, 20)).ToString("R10"); t = BigRat.Parse(sb); Debug.Assert(BigRat.Round(b, 20) == t);
        sb = ((BigRat)0).ToString("R0"); t = BigRat.Parse(sb); Debug.Assert(((BigRat)0) == t);
        sb = ((BigRat)0).ToString("R1"); t = BigRat.Parse(sb); Debug.Assert(((BigRat)0) == t);
        sb = (BigRat.Round(b, 20)).ToString("Q10");
        sb = (BigRat.Round(b, 20)).ToString("Q32");
        sb = (BigRat.Round(b, 120)).ToString("Q32");
        sb = (BigRat.Round(b, 20)).ToString("R");
        sb = ((BigRat)0).ToString("R0"); t = BigRat.Parse(sb); Debug.Assert(((BigRat)0) == t);

        sb = ((BigRat)1613 / 72048).ToString("Q1000"); //t = BigRat.Parse(sb); Debug.Assert(((BigRat)0) == t);
        t = BigRat.Parse(sb); Debug.Assert((BigRat)1613 / 72048 == t);

      }
    }

  }

}