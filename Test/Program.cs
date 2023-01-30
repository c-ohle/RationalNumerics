
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
      ApplicationConfiguration.Initialize(); //test_bigrat();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }

    static void test_bigrat()
    {
      test_sin();
      test_pi();
      test_log();
      test_exp(); //4.8
      test_rounds();
      test_tos();
      test_conv();
      return;
      static void test_sin()
      {
        //test(int.MaxValue);
        test(0.2); test(-0.2); test(Math.PI / 4 + 0.2); test(Math.PI / 2 + 0.2);
        test(Math.PI / 2 + 0.2); test(Math.PI * 3 / 4 + 0.2); test(-(Math.PI * 3 / 4 + 0.2));
        test(Math.PI + 0.2); test(Math.PI * 5 / 4 + 0.2);
        test(Math.PI * 3 / 3 + 0.2); test(Math.PI * 7 / 4 + 0.2);
        test(1); test(2); test(3); test(0);
        test(short.MaxValue);
        test(int.MaxValue);
        test(-short.MaxValue);
        test(-int.MaxValue);
        test(1.0 / short.MaxValue);
        test(1.0 / int.MaxValue);

        test(1.0e+10);
        test(-1.0e+10);
        test(1.0e-10);
        test(-1.0e-10);

        test(1.0e+100);
        test(-1.0e+100);
        test(1.567e-100); //1.0! 1.1
        test(-1.567e-100);
        test(BigRat.Parse("1e1000"));
        return;

        static void test(BigRat z)
        {
          double x; BigRat a, b, c, d, e;
          int l = 100 + Math.Max(0, BigRat.ILog10(z)); c = BigRat.Pi(l) * 2; c = z % c;
          x = Math.Cos((double)c);
          e = BigRat.Cos(z, 300); Debug.Assert(e.ToString("G14") == x.ToString("G14"));          
          x = Math.Sin((double)c);
          a = BigRat.Sin(z, 300); Debug.Assert(a.ToString("G14") == x.ToString("G14"));         
          for (int i = 0; i < 200; i++)
          {
            b = BigRat.Round(a, i); c = BigRat.Sin(z, i); Debug.Assert((d = b - c) == 0);
            b = BigRat.Round(e, i); c = BigRat.Cos(z, i); Debug.Assert((d = b - c) == 0);
          }
        }
      }
      static void test_log()
      {
        Debug.Assert(BigRat.Log(1, 20) == 0);
        test(0.5); test(2); test(1.5); //todo:...
        static void test(double z)
        {
          var a = Math.Log(z); BigRat b, c, d, x = z; //rat r;
          b = BigRat.Log(x, 200); if (double.IsFinite(a)) Debug.Assert(a.ToString("G15") == b.ToString("G15"));
          for (int i = 4; i < 150; i++)
          {
            c = BigRat.Log(x, i);
            d = BigRat.Round(b, i);// Debug.Assert(c == d);
          }
        }

        double a; BigRat b; rat r; //, c, d, z; string s;
        a = Math.Pow(2, 0.5);
        b = BigRat.Pow(2, 0.5, 20);
        a = Math.Pow(2, 12.34);
        b = BigRat.Pow(2, 12.34, 20);

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
      static void test_pi()
      {
        BigRat a, b, c, d = 0, e = 0, mi = 100, ma = 0;
        //var s = "3.14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196442881097566593344612847564823378678316527120190914564856692346034861045432664821339360726024914127372458700660631558817488152092096282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179310511854807446237996274956735188575272489122793818301194912983367336244065664308602139494639522473719070217986094370277053921717629317675238467481846766940513200056812714526356082778577134275778960917363717872146844090122495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837297804995105973173281609631859502445945534690830264252230825334468503526193118817101000313783875288658753320838142061717766914730359825349042875546873115956286388235378759375195778185778053217122680661300192787661119590921642019893809525720106548586327886593615338182796823030195203530185296899577362259941389124972177528347913151557485724245415069595082953311686172785588907509838175463746493931925506040092770167113900984882401285836160356370766010471018194295559619894676783744944825537977472684710404753464620804668425906949129331367702898915210475216205696602405803815019351125338243003558764024749647326391419927260426992279678235478163600934172164121992458631503028618297455570674983850549458858692699569092721079750930295532116534498720275596023648066549911988183479775356636980742654252786255181841757467289097777279380008164706001614524919217321721477235014144197356854816136115735255213347574184946843852332390739414333454776241686251898356948556209921922218427255025425688767179049460165346680498862723279178608578438382796797668145410095388378636095068006422512520511739298489608412848862694560424196528502221066118630674427862203919494504712371378696095636437191728746776465757396241389086583264599581339047802759009946576407895126946839835259570982582262052248940772671947826848260147699090264013639443745530506820349625245174939965143142980919065925093722169646151570985838741059788595977297549893016175392846813826868386894277415599185592524595395943104997252468084598727364469584865383673622262609912460805124388439045124413654976278079771569143599770012961608944169486855584840635342207222582848864815845602850601684273945226746767889525213852254995466672782398645659611635488623057745649803559363456817432411251507606947945109659609402522887971089314566913686722874894056010150330861792868092087476091782493858900971490967598526136554978189312978482168299894872265880485756401427047755513237964145152374623436454285844479526586782105114135473573952311342716610213596953623144295248493718711014576540359027993440374200731057853906219838744780847848968332144571386875194350643021845319104848100537061468067491927819119793995206141966342875444064374512371819217999839101591956181467514269123974894090718649423196156794520809514655022523160388193014209376213785595663893778708303906979207734672218256259966150142150306803844773454920260541466592520149744285073251866600213243408819071048633173464965145390579626856100550810665879699816357473638405257145910289706414011097120628043903975951567715770042033786993600723055876317635942187312514712053292819182618612586732157919841484882916447060957527069572209175671167229109816909152801735067127485832228718352093539657251210835791513698820914442100675103346711031412671113699086585163983150197016515116851714376576183515565088490998985998238734552833163550764791853589322618548963213293308985706420467525907091548141654985946163718027098199430992448895757128289059232332609729971208443357326548938239119325974636673058360414281388303203824903758985243744170291327656180937734440307074692112019130203303801976211011004492932151608424448596376698389522868478312355265821314495768572624334418930396864262434107732269780280731891544110104468232527162010526522721116603966655730925471105578537634668206531098965269186205647693125705863566201855810072936065987648611791045334885034611365768675324944166803962657978771855608455296";
        var s = "3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117067982148086513282306647093844609550582231725359408128481117450284102701938521105559644622948954930381964428810975665933446128475648233786783165271201909145648566923460348610454326648213393607260249141273724587006606315588174881520920962829254091";
        var n = s.Length;
        a = BigRat.Parse(s);
        for (int i = 0; i < s.Length - 20; i++)
        {
          b = BigRat.Round(a, i);
          c = BigRat.Pi(i); Debug.Assert(b == c);
          //c = BigRat.pi(BigRat.prec(i));             
          //d = -BigRat.ILog10(c - a); e = d - i; 
          //mi = BigRat.Min(mi, e);
          //ma = BigRat.Max(ma, e);            
          //c = BigRat.Round(c, i);
          //Debug.Assert((d = b - c) == 0);
          //d = BigRat.Pi(i); Debug.Assert(d == c);
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
      static void test_conv()
      {
#if NET7_0_OR_GREATER
        BigRat b, c; bool d;
        b = 0;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == int.IsEvenInteger((int)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == int.IsOddInteger((int)b));
        b = -5;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == int.IsEvenInteger((int)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == int.IsOddInteger((int)b));
        b = 5;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == int.IsEvenInteger((int)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == int.IsOddInteger((int)b));
        b *= 128;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == int.IsEvenInteger((int)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == int.IsOddInteger((int)b));
        b /= 128;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == int.IsEvenInteger((int)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == int.IsOddInteger((int)b));
        b++;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == int.IsEvenInteger((int)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == int.IsOddInteger((int)b));
        b = 1.5;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == double.IsEvenInteger((double)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == double.IsOddInteger((double)b));
        b = -1.5;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == double.IsEvenInteger((double)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == double.IsOddInteger((double)b));
        b = Int128.MaxValue;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == Int128.IsEvenInteger((Int128)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == Int128.IsOddInteger((Int128)b));
        b = ulong.MaxValue; b += 123;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == Int128.IsEvenInteger((Int128)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == Int128.IsOddInteger((Int128)b));
        b /= 2923820290820; b *= 2923820290820;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == Int128.IsEvenInteger((Int128)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == Int128.IsOddInteger((Int128)b));
        b = -b;
        Debug.Assert((d = BigRat.IsEvenInteger(b)) == Int128.IsEvenInteger((Int128)b));
        Debug.Assert((d = BigRat.IsOddInteger(b)) == Int128.IsOddInteger((Int128)b));
        b = BigRat.Normalize(b);
        b = BigRat.Normalize(b);

        b = (Int128)0; c = (Int128)b; Debug.Assert(b == c);
        b = (Int128)1234; c = (Int128)b; Debug.Assert(b == c);
        b = (Int128)(-1234); c = (Int128)b; Debug.Assert(b == c);
        b = (Int128)(long.MinValue); c = (Int128)b; Debug.Assert(b == c);
        b = Int128.MaxValue; c = (Int128)b; Debug.Assert(b == c);
        b = Int128.MinValue; c = (Int128)b; Debug.Assert(b == c);
        b = (UInt128)0; c = (UInt128)b; Debug.Assert(b == c);
        b = (UInt128)1234; c = (UInt128)b; Debug.Assert(b == c);
        b = UInt128.MaxValue; c = (UInt128)b; Debug.Assert(b == c);
        //b = UInt128.MaxValue; c = (Int128)b; // exception                                                           
        //b = UInt128.MaxValue; b = -b; c = (Int128)b; // exception
        //b = UInt128.MaxValue; b++; c = (Int128)b; // exception
        //b = UInt128.MaxValue; b++; b = -b; c = (Int128)b;
#endif
      }
    }

  }

}