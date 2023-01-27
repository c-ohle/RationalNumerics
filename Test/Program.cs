
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
      double a; BigRat b, c, d, z;

      //for (int i = 0; i < 500; i++)
      //{
      //  b = BigRat.Pow10(i); var x = BigRat.GetMsbNum(b);
      //  Debug.WriteLine($"{i} {x} na:{x / 32 + 1} {(int)(i * 3.326f) / 32 + 1} {(double)i / x}");
      //}

      z = 1; a = Math.Exp((double)z); b = BigRat.Exp(z, 20);
      z = 2; a = Math.Exp((double)z); b = BigRat.Exp(z, 20);
      z = -2; a = Math.Exp((double)z); b = BigRat.Exp(z, 20);

      b = BigRat.Exp(z, 500);
      var xx = rat.Exp(234.567, 500);
      b = BigRat.Exp(1, 1000);
      xx = rat.Exp(1, 1000); //452
      var s1 = b.ToString("Q2000");
      var s2 = xx.ToString("H2000");

      //2,718281828459045235360287471352662497757247093699959574966967627724076630353547594571382178525166427427466391932003059921817413596629043572900334295260595630738132328627943490763233829880753195251019011573834187930702154089149934884167509244761460668082264800168477411853742345442437107539077744992069551702761838606261331384583000752044933826560297606737113200709328709127443747047230696977209310141692836819025515108657463772111252389784425056953696770785449969967946864454905987931636889230098793127736178215424999229576351482208269895193668033182528869398496465105820939239829488793320362509443117301238197068416140397019837679320683282376464804295311802328782509819455815301756717361332069811250996181881593041690351598888519345807273866738589422879228499892086805825749279610484198444363463244968487560233624827041978623209002160990235304369941849146314093431738143640546253152096183690888707016768396424378140592714563549061303107208510383750510115747704171898610687396965521267154688957035035
      //2,7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274274663919320030599218174135966290435729003342952605956307381323286279434907632338298807531952510190115738341879307021540891499348841675092447614606680822648001684774118537423454424371075390777449920695517027618386062613313845830007520449338265602976067371132007093287091274437470472306969772093101416928368190255151086574637721112523897844250569536967707854499699679468644549059879316368892300987931277361782154249992295763514822082698951936680331825288693984964651058209392398294887933203625094431173012381970684161403970198376793206832823764648042953118023287825098194558153017567173613320698112509961818815930416903515988885193458072738667385894228792284998920868058257492796104841984443634632449684875602336248270419786232090021609902353043699418491463140934317381436405462531520961836908887070167683964243781405927145635490613031072085103837505101157477041718986106873969655212671546889570350354
      //2,718281828459045235360287471352662497757247093699959574966967627724076630353547594571382178525166427427466391932003059921817413596629043572900334295260595630738132328627943490763233829880753195251019011573834187930702154089149934884167509244761460668082264800168477411853742345442437107539077744992069551702761838606261331384583000752044933826560297606737113200709328709127443747047230696977209310141692836819025515108657463772111252389784425056953696770785449969967946864454905987931636889230098793127736178215424999229576351482208269895193668033182528869398496465105820939239829488793320362509443117301238197068416140397019837679320683282376464804295311802328782509819455815301756717361332069811250996181881593041690351598888519345807273866738589422879228499892086805825749279610484198444363463244968487560233624827041978623209002160990235304369941849146314093431738143640546253152096183690888707016768396424378140592714563549061303107208510383750510115747704171898610687396965521267154688957035035

      //var xx = rat.Exp(234.567,20); //689
      //7.4328220699218859465214335182136441353726751272263092713840... × 10^101
      z = 234.567; b = BigRat.Exp(z, 500); a = Math.Exp((double)z);
         
      z = 123.456; a = Math.Exp((double)z); b = BigRat.Exp(z, 400);
      for (int i = 0; i < 200; i++)
      {
        c = BigRat.Exp(z, i);
        d = BigRat.Round(b, i - BigRat.ILog10(b) - 1);
        Debug.Assert(c == d);
      }

      z = 1; a = Math.Exp((double)z); b = BigRat.Exp(z, 400);
      for (int i = 0; i < 200; i++)
      {
        c = BigRat.Exp(z, i);
        d = BigRat.Round(b, i - BigRat.ILog10(b) - 1);
        Debug.Assert(c == d);
      }

      z = 0.01; a = Math.Exp((double)z); b = BigRat.Exp(z, 400);
      for (int i = 0; i < 200; i++)
      {
        c = BigRat.Exp(z, i);
        d = BigRat.Round(b, i - BigRat.ILog10(b) - 1);
        Debug.Assert(c == d);
      }

      z = -0.01; a = Math.Exp((double)z); b = BigRat.Exp(z, 400);
      for (int i = 0; i < 200; i++)
      {
        c = BigRat.Exp(z, i);
        d = BigRat.Round(b, i - BigRat.ILog10(b) - 1);
        Debug.Assert(c == d);
      }

      z = -333.333333; a = Math.Exp((double)z); b = BigRat.Exp(z, 400);
      for (int i = 0; i < 200; i++)
      {
        c = BigRat.Exp(z, i);
        d = BigRat.Round(b, i - BigRat.ILog10(b) - 1);
        Debug.Assert(c == d);
      }

      a = Math.Exp(0);
      b = BigRat.Exp(0, 100);

      test1();
      test2();

      static void test1()
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
      static void test2()
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