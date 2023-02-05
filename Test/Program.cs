﻿
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Test
{
  internal unsafe static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); // bigrat_tests();
      Debug.Assert(rat.task_cpu.mark() == 0);
      Application.Run(new MainFrame());
      Debug.Assert(rat.task_cpu.mark() == 0);
    }

    [Conditional("DEBUG")]
    static void bigrat_tests()
    {
      double d; BigRat r;

      d = 12345.67; r = BigRat.CreatePrecise(d); Debug.Assert(d == r);
      d = 0.5; r = BigRat.CreatePrecise(d); Debug.Assert(d == r);
      d = 0.1; r = BigRat.CreatePrecise(d); Debug.Assert(d == r);
      d = double.MaxValue; r = BigRat.CreatePrecise(d); Debug.Assert(d == r);
      d = double.MinValue; r = BigRat.CreatePrecise(d); Debug.Assert(d == r);

      test_atan2();
      test_asin();
      test_log();
      test_log2();
      test_exp();
      test_sin();
      test_atan();
      test_pow();
      test_sqrt();
      test_pi();
      test_rounds();
      test_tostring();
      test_conv();
      return;

      static void test_atan2()
      {
        double x, y; BigRat a, b, c, d, u, v, w, q;
        var rnd = new Random(1);
        for (int i = 0; i < 20; i++)
        {
          var f = rnd.NextDouble() * 10 - 5; var g = rnd.NextDouble() * 10 - 5;
          x = Math.Atan2(f, g); y = Math.Round(x, 10); u = f; q = g;
          a = BigRat.Atan2(u, q, 200); w = BigRat.Round(a, 10); Debug.Assert(y == w);
          for (int e = 0; e < 20; e++)
          {
            b = BigRat.Atan2(u, q, -e); c = e != 0 ? BigRat.Round(b, e) : b;
            d = BigRat.Round(b, -BigRat.ILog10(a - b)); v = BigRat.Round(a, e); Debug.Assert(v == c);
            //var sp = (ReadOnlySpan<uint>)b; var t = (BigRat)sp; Debug.Assert(t == b);
          }
        }
      }
      static void test_asin()
      {
        double x, y; BigRat a, b, c, d, u, v, w;
        //a = BigRat.Pi(32); a = BigRat.Asin(0.5); a = BigRat.Acos(0.5); a--;
        x = Math.Asin(+0); a = BigRat.Asin(+0); Debug.Assert(a == 0); w = BigRat.Pi(32);
        x = Math.Asin(+1); a = BigRat.Asin(+1); c = a - w / 2; Debug.Assert(c == 0);
        x = Math.Asin(-1); a = BigRat.Asin(-1); c = a + w / 2; Debug.Assert(c == 0);
        x = Math.Acos(+0); a = BigRat.Acos(+0); c = a - w / 2; Debug.Assert(c == 0);
        x = Math.Acos(+1); a = BigRat.Acos(+1); Debug.Assert(a == 0);
        x = Math.Acos(-1); a = BigRat.Acos(-1); c = a - w; Debug.Assert(c == 0);
        //x = Math.Asin(+1.1); a = BigRat.Asin(+1.1);
        //x = Math.Asin(-1.1); a = BigRat.Asin(-1.1);
        //x = Math.Acos(+1.1); a = BigRat.Acos(+1.1);
        //x = Math.Acos(-1.1); a = BigRat.Acos(-1.1);
        var rnd = new Random(1);
        for (int i = 0; i < 20; i++)
        {
          var f = rnd.NextDouble() * 2 - 1; x = Math.Asin(f); y = Math.Round(x, 10); u = f;
          a = BigRat.Asin(u, 200); w = BigRat.Round(a, 10); Debug.Assert(y == w);
          for (int e = 0; e < 20; e++)
          {
            b = BigRat.Asin(u, -e); c = e != 0 ? BigRat.Round(b, e) : b;
            d = BigRat.Round(b, -BigRat.ILog10(a - b)); v = BigRat.Round(a, e); Debug.Assert(v == c);
          }
        }
        rnd = new Random(1);
        for (int i = 0; i < 20; i++)
        {
          var f = rnd.NextDouble() * 2 - 1; x = Math.Acos(f); y = Math.Round(x, 10); u = f;
          a = BigRat.Acos(u, 200); w = BigRat.Round(a, 10); Debug.Assert(y == w);
          for (int e = 0; e < 20; e++)
          {
            b = BigRat.Acos(u, -e); c = e != 0 ? BigRat.Round(b, e) : b;
            d = BigRat.Round(b, -BigRat.ILog10(a - b)); v = BigRat.Round(a, e); Debug.Assert(v == c);
          }
        }
      }
      static void test_log()
      {
        double x, y; BigRat a, b, c, d, u, v, w;
        var rnd = new Random(1);
        for (int i = 0; i < 200; i++)
        {
          var f = rnd.NextDouble() * 10; x = Math.Log(f); y = Math.Round(x, 10); u = f;
          a = BigRat.Log(u, 200); w = BigRat.Round(a, 10); Debug.Assert(y == w);
          for (int e = 0; e < 100; e++)
          {
            b = BigRat.Log(u, -e); c = BigRat.Round(b, e);
            d = BigRat.Round(b, -BigRat.ILog10(a - b)); v = BigRat.Round(a, e); Debug.Assert(v == c);
          }
        }
        //log(x, base)
        //x = Math.Log(123, 2); try { a = BigRat.Log(123, 2, 32); } catch (Exception e) { }
        //x = Math.Log(-123, 2); try { a = BigRat.Log(-123, 2, 32); } catch (Exception e) { }
        //x = Math.Log(3, 0.5); try { a = BigRat.Log(3, 0.5, 32); } catch (Exception e) { }
        //x = Math.Log(3, 1); try { a = BigRat.Log(3, 1, 32); } catch (Exception e) { }
        //x = Math.Log(0, 1); try { a = BigRat.Log(0, 1, 32); } catch (Exception e) { }
        //x = Math.Log(0, 1); try { a = BigRat.Log(0, 1, 32); } catch (Exception e) { }
        //x = Math.Log(0, 1.1); try { a = BigRat.Log(0, 1.1, 32); } catch (Exception e) { }
        //x = Math.Log(1, 0); a = BigRat.Log(1, 0, 32);
      }
      static void test_log2()
      {
        double x, y; BigRat a, b, c, d, u, v, w;
        a = BigRat.Log2(0.25, 32); Debug.Assert(a == -2);
        a = BigRat.Log2(0.5, 32); Debug.Assert(a == -1);
        a = BigRat.Log2(1, 32); Debug.Assert(a == 0);
        a = BigRat.Log2(2, 32); Debug.Assert(a == 1);
        a = BigRat.Log2(4, 32); Debug.Assert(a == 2);
        a = BigRat.Log2(8, 32); Debug.Assert(a == 3);
        a = BigRat.Log2(1ul << 63, 32); Debug.Assert(a == 63);
        var rnd = new Random(1);
        for (int i = 0; i < 200; i++)
        {
          var f = rnd.NextDouble() * 100; x = Math.Log2(f); y = Math.Round(x, 10); u = f;
          a = BigRat.Log2(u, 200); w = BigRat.Round(a, 10); Debug.Assert(y == w);
          for (int e = 0; e < 100; e++)
          {
            b = BigRat.Log2(u, -e); c = BigRat.Round(b, e);
            d = BigRat.Round(b, -BigRat.ILog10(a - b)); v = BigRat.Round(a, e); Debug.Assert(v == c);
          }
        }
      }
      static void test_exp()
      {
        double x, y; BigRat a, b, c, d, u, v, w; x = Math.Exp(1); string s;

        a = BigRat.Exp(+100, 10); b = BigRat.Exp(+100, 2); c = BigRat.Exp(+100, 1); d = BigRat.Exp(+100, 0);
        a = BigRat.Exp(-100, 10); b = BigRat.Exp(-100, 2); c = BigRat.Exp(-100, 1); d = BigRat.Exp(-100, 0);
        a = BigRat.Exp(0.01, 10); b = BigRat.Exp(0.01, 2); c = BigRat.Exp(0.01, 1); d = BigRat.Exp(0.01, 0);

        //s = " 2.7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274274663919320030599218174135966290435729003342952605956307381323286279434907632338298807531952510190115738341879307021540891499348841675092447614606680822648001684774118537423454424371075390777449920695517027618386062613313845830007520449338265602976067371132007093287091274437470472306969772093101416928368190255151086574637721112523897844250569536967707854499699679468644549059879316368892300987931277361782154249992295763514822082698951936680331825288693984964651058209392398294887933203625094431173012381970684161403970198376793206832823764648042953118023287825098194558153017567173613320698112509961818815930416903515988885193458072738667385894228792284998920868058257492796104841984443634632449684875602336248270419786232090021609902353043699418491463140934317381436405462531520961836908887070167683964243781405927145635490613031072085103837505101157477041718986106873969655212671546889570350354021234078498193343210681701210056278802351930332247450158539047304199577770935036604169973297250886876966403555707162268447162560798826517871341951246652010305921236677194325278675398558944896970964097545918569563802363701621120477427228364896134225164450781824423529486363721417402388934412479635743702637552944483379980161254922785092577825620926226483262779333865664816277251640191059004916449982893150566047258027786318641551956532442586982946959308019152987211725563475463964479101459040905862984967912874068705048958586717479854667757573205681288459205413340539220001137863009455606881667400169842055804033637953764520304024322566135278369511778838638744396625322498506549958862342818997077332761717839280349465014345588970719425863987727547109629537415211151368350627526023264847287039207643100595841166120545297030236472549296669381151373227536450988890313602057248176585118063036442812314965507047510254465011727211555194866850800368532281831521960037356252794495158284188294787610852639813955990067376482922443752871846245780361929819713991475644882626039033814418232 ";
        s = "2.7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274274663919320030599218174";
        a = BigRat.Parse(s); u = 1;
        for (int e = 0; e < s.Length - 5; e++)
        {
          b = BigRat.Exp(u, -e); c = e != 0 ? round(b, e) : b;
          d = BigRat.Round(b, -BigRat.ILog10(a - b)); v = round(a, e); Debug.Assert(v == c);
        }
        var rnd = new Random(1);
        for (int i = 0; i < 200; i++)
        {
          var f = rnd.NextDouble() * 10 - 5; x = Math.Exp(f); y = Math.Round(x, 10);
          a = BigRat.Exp(u = f, 200); w = BigRat.Round(a, 10); Debug.Assert(y == w);
          for (int e = 0; e < 100; e++)
          {
            b = BigRat.Exp(u, -e); c = e != 0 ? round(b, e) : b;
            d = BigRat.Round(b, -BigRat.ILog10(a - b)); v = round(a, e); Debug.Assert(v == c);
          }
        }
        for (int i = 0; i < 200; i++)
        {
          var f = rnd.NextDouble() * 10 - 5; x = Math.Exp(f); y = Math.Round(x, 10);
          a = BigRat.Exp(u = f, 200); w = BigRat.Round(a, 10); Debug.Assert(y == w);
          for (int e = 0; e < 100; e++) { b = BigRat.Exp(u, e); v = round(a, e); Debug.Assert(v == b); }
        }
        static BigRat round(BigRat a, int digits) { return BigRat.Round(a, digits - BigRat.ILog10(a)); }
      }
      static void test_sin()
      {
        double x, y; BigRat a, b, c, d, u, v, w;
        Debug.Assert((a = BigRat.Sin(0)) == 0);
        Debug.Assert((a = BigRat.Cos(0)) == 1);
        Debug.Assert((a = BigRat.Sin(BigRat.Pi(35) / 2)) == 1);
        Debug.Assert((a = BigRat.Cos(BigRat.Pi(35) / 2)) == 0);
        Debug.Assert((a = BigRat.Sin(BigRat.Pi(35) * 3 / 2)) == -1);
        Debug.Assert((a = BigRat.Cos(BigRat.Pi(35) * 3 / 2)) == 0);
        Debug.Assert((a = BigRat.Sin(BigRat.Pi(35))) == 0);
        Debug.Assert((a = BigRat.Cos(BigRat.Pi(35))) == -1);

        var rnd = new Random(1);
        for (int i = 0; i < 200; i++)
        {
          var f = rnd.NextDouble() * 10 - 5; x = Math.Sin(f); y = Math.Round(x, 10); u = f;
          a = BigRat.Sin(u, 200); w = BigRat.Round(a, 10); Debug.Assert(y == w);
          for (int e = 0; e < 100; e++)
          {
            b = BigRat.Sin(u, -e); c = e != 0 ? BigRat.Round(b, e) : b;
            d = BigRat.Round(b, -BigRat.ILog10(a - b)); v = BigRat.Round(a, e); Debug.Assert(v == c);
          }
        }

        test(0.2); test(-0.2); test(Math.PI / 4 + 0.2); test(Math.PI / 2 + 0.2);
        test(Math.PI / 2 + 0.2); test(Math.PI * 3 / 4 + 0.2); test(-(Math.PI * 3 / 4 + 0.2));
        test(Math.PI + 0.2); test(Math.PI * 5 / 4 + 0.2);
        test(Math.PI * 3 / 3 + 0.2); test(Math.PI * 7 / 4 + 0.2);
        test(+1.0e+10); test(+1.0e-10); test(+1.0e+100); test(+1.1e-100);
        test(-1.0e+100); test(-1.0e+10); test(-1.0e-10); test(-1.1e-100); //1.0! 1.1        
        test(BigRat.Pi(300) / 2);
        test(BigRat.Pi(300));
        test(2 * BigRat.Pi(300)); //test(BigRat.Parse("1e1000"));//todo: check speed
        return;
        static void test(BigRat z)
        {
          double x; BigRat a, b, c, d, e, pi, pi2, pih, fs, fc;
          int l = 300 + Math.Max(0, BigRat.ILog10(z)); pi = BigRat.Pi(l); pi2 = pi * 2; pih = pi / 2;
          fs = z % pi2; fc = (z + pih) % pi2 - pih;// (z + pih) - pih;          
          e = BigRat.Cos(z, 300); x = Math.Cos((double)fc); d = BigRat.Round(e, 14); x = Math.Round(x, 14); Debug.Assert(d == x);
          a = BigRat.Sin(z, 300); x = Math.Sin((double)fs); d = BigRat.Round(a, 14); x = Math.Round(x, 14); Debug.Assert(d == x);
          for (int i = 0; i < 200; i++)
          {
            b = BigRat.Round(a, i); c = BigRat.Sin(z, i); Debug.Assert((d = b - c) == 0);
            b = BigRat.Round(e, i); c = BigRat.Cos(z, i); Debug.Assert((d = b - c) == 0);
          }
        }
      }
      static void test_atan()
      {
        double x, y; BigRat a, b, c, d; //rat r; r = rat.Atan(0.52, 100);
        x = Math.Atan(0.52);
        a = BigRat.Parse("0.4795192919925961654230970412954361042093499740232560651067716031446394831965044433555948919651396337087634489781292287670121725920728885377833098357039294474757192434955907042403553972520719769582350382379814137575982177110523829516399740643675863411530954031906787563635085891746166705989289808799461991459810915958132154246802728877428129585283360748934926994613977022962102736087713299014925908481911977361875121887371083787751537486610167271955015110958567335000054186483277518501311406975078877255129012619325846");
        for (int i = 1; i < 80; i++)
        {
          b = BigRat.Atan(0.52, i);
          c = BigRat.Round(b, i);
          d = BigRat.Round(b, -BigRat.ILog10(a - b));
        }

        var rnd = new Random(1);
        for (int i = 0; i < 200; i++)
        {
          var v = rnd.NextDouble() * 10 - 5; x = Math.Atan(v); y = Math.Round(x, 10);
          a = BigRat.Atan(v, 100); var o = BigRat.Round(a, 10); Debug.Assert(y == o);
          for (int e = 0; e < 50; e++)
          {
            b = BigRat.Atan(v, e); c = BigRat.Round(b, e);
            d = BigRat.Round(b, -BigRat.ILog10(a - b));
            var t = BigRat.Round(a, e); Debug.Assert(t == c);
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
      static void test_sqrt()
      {
        double x, y; BigRat a, b, c;
        a = BigRat.Sqrt(0); Debug.Assert(a == 0);
        a = BigRat.Sqrt(4); Debug.Assert(a == 2);
        a = BigRat.Sqrt(9); Debug.Assert(a == 3);
        a = BigRat.Sqrt(16); Debug.Assert(a == 4);
        var rnd = new Random(1);
        for (int i = 0; i < 200; i++)
        {
          var v = rnd.NextDouble() * 1000; x = Math.Sqrt(v); y = Math.Round(x, 10);
          a = BigRat.Sqrt(v, 400); var o = BigRat.Round(a, 10); Debug.Assert(y == o);
          for (int d = 0; d < 200; d++)
          {
            b = BigRat.Sqrt(v, d);
            c = BigRat.Round(a, d); Debug.Assert(b == c);
          }
        }
      }
      static void test_rounds()
      {
        double t, x; BigRat b, a;
        var aa = new double[] {
          0, 123, -123, 123.456, -123.456, 1.1, 1.8, -1.1, -1.8, 0.1, -0.1,
          -14.876543,-14.876553,-14.876453,14.876553,
        };
        for (int i = 0; i < aa.Length; i++)
        {
          t = aa[i]; a = t;
          x = Math.Truncate(t); b = BigRat.Truncate(a); Debug.Assert(x == b);
          x = Math.Floor(t); b = BigRat.Floor(a); Debug.Assert(x == b);
          x = Math.Ceiling(t); b = BigRat.Ceiling(a); Debug.Assert(x == b);
          x = Math.Round(t); b = BigRat.Round(a); Debug.Assert(x == b);
          x = Math.Round(t, MidpointRounding.AwayFromZero);
          b = BigRat.Round(a, MidpointRounding.AwayFromZero); Debug.Assert(x == b);
          for (int d = 0; d < 10; d++)
          {
            x = Math.Round(t, d);
            b = BigRat.Round(a, d); Debug.Assert(x == b);
            x = Math.Round(t, d, MidpointRounding.AwayFromZero);
            b = BigRat.Round(a, d, MidpointRounding.AwayFromZero); Debug.Assert(x == b);
          }
        }
      }
      static void test_tostring()
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
        Debug.Assert(BigRat.Parse("-0") == 0);

        b = Int64.MaxValue;
        sb = b.ToString("R0"); Debug.Assert(BigRat.Parse("-0") == 0);
        sb = (b / 2).ToString("R0");
        sb = b.ToString("Q0");
        sb = (b / 2).ToString("Q0");
      }
      static void test_pow()
      {
        BigRat a;
        for (int i = 0; i <= 1000; i++)
        {
          a = BigRat.Pow10(+i); Debug.Assert(a == BigRat.Pow(10, +i));
          a = BigRat.Pow10(-i); Debug.Assert(a == BigRat.Pow(10, -i));
          a = BigRat.Pow2(i); Debug.Assert(a == BigRat.Pow(2, i));
        }
      }
      static void test_conv()
      {
#if NET7_0_OR_GREATER
        test_inumber();
        static void test_inumber()
        {
          double d; float f; BigRat r; short s, ss; long l, ll; ulong ul, ull; decimal c; //ushort us; 

          r = +123456.78; c = (decimal)r; Debug.Assert(c == r);
          c = decimal.CreateTruncating(r); Debug.Assert(c == r);
          c = decimal.CreateSaturating(r); Debug.Assert(c == r);
          c = decimal.CreateChecked(r); Debug.Assert(c == r);
          c = decimal.CreateChecked((double)r); Debug.Assert(c == r);

          r = +123456.78e50;
          c = decimal.CreateTruncating((double)r); Debug.Assert(c == decimal.MaxValue);
          c = decimal.CreateTruncating(r); Debug.Assert(c == decimal.MaxValue);
          c = decimal.CreateSaturating(r); Debug.Assert(c == decimal.MaxValue);
          c = decimal.CreateSaturating(-r); Debug.Assert(c == decimal.MinValue);
          //try { c = decimal.CreateChecked(r); Debug.Assert(false); } catch { }

          r = +123456.78; d = (double)r; Debug.Assert(r == d);
          r = -123456.78; d = (double)r; Debug.Assert(r == d);
          r = float.MaxValue; d = (double)r; Debug.Assert(r == d);
          r = float.MaxValue; f = (float)r; Debug.Assert(f == r);

          r = float.MaxValue; r *= 1.0001; f = (float)r; Debug.Assert(f == float.PositiveInfinity && f == (float)(double)r);
          f = float.CreateTruncating(r); Debug.Assert(f == float.CreateTruncating((double)r));
          f = float.CreateSaturating(r); Debug.Assert(f == float.CreateSaturating((double)r));
          f = float.CreateChecked(r); Debug.Assert(f == float.CreateChecked((double)r));
          f = checked((float)r); Debug.Assert(f == checked((float)(double)r));

          r = float.MinValue; r *= 1.0001; f = (float)r; Debug.Assert(f == float.NegativeInfinity && f == (float)(double)r);
          f = float.CreateTruncating(r); Debug.Assert(f == float.CreateTruncating((double)r));
          f = float.CreateSaturating(r); Debug.Assert(f == float.CreateSaturating((double)r));
          f = float.CreateChecked(r); Debug.Assert(f == float.CreateChecked((double)r));

          //
          r = +123456; l = (long)r; Debug.Assert(r == l);
          r = -123456; l = (long)r; Debug.Assert(r == l);
          r = long.MaxValue; l = (long)r; Debug.Assert(r == l);
          r = long.MinValue; l = (long)r; Debug.Assert(r == l);
          r = long.MaxValue; r++; l = (long)r; Debug.Assert(l == long.MinValue);
          r = long.MinValue; r--; l = (long)r; Debug.Assert(l == long.MinValue);
          r = long.MinValue; l = long.CreateChecked(r); Debug.Assert(r == l);
          r = long.MaxValue; l = long.CreateChecked(r); Debug.Assert(r == l);

          r = +123456; ul = (ulong)r; Debug.Assert(ul == r);
          r = -123456; ul = (ulong)r; Debug.Assert(ul == (ulong)(Int128)r);
          r = ulong.MaxValue; ul = (ulong)r; Debug.Assert(ul == r);
          r = ulong.MaxValue; r++; ul = (ulong)r; Debug.Assert(ul == unchecked((ulong)((double)ulong.MaxValue + (double)ulong.MaxValue)));

          r = BigRat.Parse("+1e1000"); d = double.MaxValue;
          s = (short)r; Debug.Assert(s == (ss = (short)d)); // 0
          s = short.CreateTruncating(r); Debug.Assert(s == (ss = short.CreateTruncating(d))); //32767
          s = short.CreateSaturating(r); Debug.Assert(s == (ss = short.CreateSaturating(d)));
          //try { s = short.CreateChecked(r); Debug.Assert(false); } catch { }

          r = BigRat.Parse("-1e1000"); d = double.MinValue;
          s = (short)r; Debug.Assert(s == (ss = (short)d));
          s = short.CreateTruncating(r); Debug.Assert(s == (ss = short.CreateTruncating(d)));
          s = short.CreateSaturating(r); Debug.Assert(s == (ss = short.CreateSaturating(d)));
          //try { s = short.CreateChecked(r); Debug.Assert(false); } catch { }

          r = BigRat.Parse("+1e1000"); d = double.MaxValue;
          l = (long)r; Debug.Assert(l == (ll = (long)d));
          l = long.CreateTruncating(r); Debug.Assert(l == (ll = long.CreateTruncating(d)));
          l = long.CreateSaturating(r); Debug.Assert(l == (ll = long.CreateSaturating(d)));
          //try { l = long.CreateChecked(r); Debug.Assert(false); } catch { }

          r = BigRat.Parse("-1e1000"); d = double.MinValue;
          l = (long)r; Debug.Assert(l == (ll = (long)d));
          l = long.CreateTruncating(r); Debug.Assert(l == (ll = long.CreateTruncating(d)));
          l = long.CreateSaturating(r); Debug.Assert(l == (ll = long.CreateSaturating(d)));
          //try { l = long.CreateChecked(r); Debug.Assert(false); } catch { }

          r = BigRat.Parse("+1e1000"); d = double.MaxValue;
          ul = (ulong)r; Debug.Assert(ul == (ull = (ulong)d));
          ul = ulong.CreateTruncating(r); Debug.Assert(ul == (ull = ulong.CreateTruncating(d)));
          ul = ulong.CreateSaturating(r); Debug.Assert(ul == (ull = ulong.CreateSaturating(d)));
          //try { ul = ulong.CreateChecked(r); Debug.Assert(false); } catch { }

          r = BigRat.Parse("-1e1000"); d = double.MinValue;
          ul = (ulong)r; Debug.Assert(ul == unchecked((ulong)(double.MinValue)));
          ul = ulong.CreateTruncating(r); Debug.Assert(ul == (ull = ulong.CreateTruncating(d)));
          ul = ulong.CreateSaturating(r); Debug.Assert(ul == (ull = ulong.CreateSaturating(d)));
          //try { ul = ulong.CreateChecked(r); Debug.Assert(false); } catch { }
        }

        Debug.Assert((BigRat)double.NegativeZero == 0);
        Debug.Assert((BigRat)float.NegativeZero == 0);
        Debug.Assert((BigRat)Half.NegativeZero == 0);
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