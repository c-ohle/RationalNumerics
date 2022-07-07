#pragma warning disable CS8602,CS8604
using Microsoft.Win32;
using System.Data;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml;
using System.Xml.Linq;

namespace Test
{
  public unsafe partial class BenchmarksPage : UserControl
  {
    public BenchmarksPage()
    {
      InitializeComponent();
    }
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e); if (DesignMode) return;
      //if(!Stopwatch.IsHighResolution) { } //todo:
      var path = Path.GetFullPath("templ\\benchmarks1.htm");
      wb = new WebBrowser
      {
        Dock = DockStyle.Fill,
        AllowNavigation = false,
        WebBrowserShortcutsEnabled = false,
        IsWebBrowserContextMenuEnabled = false,
        ScriptErrorsSuppressed = true,
        //AllowWebBrowserDrop = false,
        Url = new Uri(path)
      };
      wb.DocumentCompleted += wb_DocumentCompleted;
      panel_webview.Controls.Add(wb);
      //wb.DocumentText = File.ReadAllText(path);
      //textBox1.Text = rat.CPU.klim.ToString();
    }

    void textBox1_Leave(object sender, EventArgs e)
    {
      //try { rat.CPU.klim = (uint)int.Parse(textBox1.Text); }
      //catch { }
      //textBox1.Text = rat.CPU.klim.ToString();
    }

    WebBrowser? wb; HtmlElement? p5; XElement? esvg;
    void wb_DocumentCompleted(object? sender, WebBrowserDocumentCompletedEventArgs? e)
    {
      var doc = wb.Document; var body = doc.Body;
      var a = typeof(BigRational).Assembly; var name = a.GetName();
      var t1 = a.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false).FirstOrDefault() as System.Runtime.Versioning.TargetFrameworkAttribute;
      doc.GetElementById("1").InnerText = name.Version.ToString();
      doc.GetElementById("2").InnerText = a.ImageRuntimeVersion + " " + t1?.FrameworkName;
      using (var key = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0"))//\\CentralProcessor\\0\\ProcessorNameString"))
        doc.GetElementById("3").InnerText = $"{key.GetValue("ProcessorNameString")} ({key.GetValue("~MHz")} MHz) × {Environment.ProcessorCount}";
      if (!MainFrame.debug) doc.GetElementById("4").OuterHtml = "";

      esvg = XElement.Parse((p5 = doc.GetElementById("5")).InnerHtml);
    }

    //Task? task; System.Windows.Forms.Timer? timer;
    (float[] times, int count)[]? passes;

    void runtest(HtmlElement svg, HtmlElement info, Action<int, Func<bool>> test)
    {
      passes = new (float[], int)[2];
      passes[0].times = new float[256];
      passes[1].times = new float[256];
      for (int k = 0; k < 2; k++) for (int i = 0; i < 256; i++) passes[k].times[i] = float.MaxValue;
      var ft = 1000f / Stopwatch.Frequency;
      for (int i = 0; i < 40; i++)
      {
        var lt = 0L;
        test(i & 1, () =>
        {
          var t = Stopwatch.GetTimestamp();
          ref var p = ref passes[i & 1];
          if (lt == 0) p.count = 0;
          else { p.times[p.count] = MathF.Min(p.times[p.count], (t - lt) * ft); p.count++; }
          lt = Stopwatch.GetTimestamp(); return true;
        });
      }
      test(8, () => true);
      update_svg(svg);
      var a1 = passes[0].times.Take(255);//.Average();
      var a2 = passes[1].times.Take(255);//.Average();
      info.InnerHtml = $"Average ratio: {MathF.Round(a1.Average() * 100 / a2.Average())}%; BigRational Max: {a1.Max()}ms, BigInteger Max: {a2.Max()}ms";
      info.ScrollIntoView(false);
    }

    void button_run_Click(object sender, EventArgs e)
    {
      if (esvg == null) return;
      //if (task != null) return;
      runtest(
        wb.Document.GetElementById("5"),
        wb.Document.GetElementById("6"), _test_mul());
      runtest(
        wb.Document.GetElementById("7"),
        wb.Document.GetElementById("8"), _test_gcd());

      return;
#if false
      passes = new (float[], int)[2];
      passes[0].times = new float[256];
      passes[1].times = new float[256];
      for (int k = 0; k < 2; k++) for (int i = 0; i < 256; i++) passes[k].times[i] = float.MaxValue;
      var ft = 1000f / Stopwatch.Frequency;
#if true
      for (int i = 0; i < 40; i++)
      {
        var lt = 0L;
        test(i & 1, () =>
        {
          var t = Stopwatch.GetTimestamp();
          ref var p = ref passes[i & 1];
          if (lt == 0) p.count = 0;
          else { p.times[p.count] = MathF.Min(p.times[p.count], (t - lt) * ft); p.count++; }
          lt = Stopwatch.GetTimestamp(); return true;
        });
      }
      update_svg(p5);
      var a1 = passes[0].times.Take(255);//.Average();
      var a2 = passes[1].times.Take(255);//.Average();
      var p6 = wb.Document.GetElementById("6");
      p6.InnerHtml = $"Average: {MathF.Round(a1.Average() * 100 / a2.Average())}%; Bigrational Max: {a1.Max()}ms, BigInteger Max: {a2.Max()}ms";
      p6.ScrollIntoView(false);

#else
      task = new Task(() =>
      {
        for (int i = 0; i < 40; i++)
        {
          var lt = 0L;
          test(i & 1, () =>
          {
            var t = Stopwatch.GetTimestamp();
            ref var p = ref passes[i & 1];
            if (lt == 0) p.count = 0;
            else { p.times[p.count] = MathF.Min(p.times[p.count], (t - lt) * ft); p.count++; }
            lt = Stopwatch.GetTimestamp(); return true;
          });
        }
      });
      timer = new System.Windows.Forms.Timer() { Interval = 200 };
      timer.Tick += ontimer; timer.Start();
      task.Start();
      void ontimer(object? p, EventArgs? e)
      {
        update_svg(p5); if (!task.IsCompleted) return;
        timer.Tick -= ontimer; timer.Dispose(); timer = null;
        task.Dispose(); task = null;

        var a1 = passes[0].times.Skip(1).Take(248);//.Average();
        var a2 = passes[1].times.Skip(1).Take(248);//.Average();
        var p6 = wb.Document.GetElementById("6");
        p6.InnerHtml =
          $"Average: {MathF.Round(a1.Average() * 100 / a2.Average())}%; Bigrational Max: {a1.Max()}ms, BigInteger Max: {a2.Max()}ms";
        p6.ScrollIntoView(false);
      };   
#endif
#endif
    }

    void update_svg(HtmlElement? psvg)
    {
      var ns = esvg.Name.Namespace;// DX11ModelCtrl.Models.ns;
      var bk = esvg.Descendants(ns + "rect").First(p => (string?)p.Attribute("name") == "F");
      var sx = esvg.Descendants(ns + "g").First(p => (string?)p.Attribute("name") == "X"); sx.RemoveNodes();
      var sy = esvg.Descendants(ns + "g").First(p => (string?)p.Attribute("name") == "Y"); sy.RemoveNodes();
      var lb = esvg.Descendants(ns + "polyline").First(p => (string?)p.Attribute("name") == "B");
      var lr = esvg.Descendants(ns + "polyline").First(p => (string?)p.Attribute("name") == "R");
      var a1 = passes[0].times.Take(255);
      var a2 = passes[1].times.Take(255);

      int wx = 720, wy = 223;
      var mx = 2040f; var nx = 10;
      var my = Math.Max(a1.Max(), a2.Max()); //my = 0.0748f;                
      var oy = MathF.Pow(10, 1 - MathF.Ceiling(MathF.Log10(my)));
      var t1 = MathF.Ceiling(my * oy); oy = t1 / oy;
      var ny = (int)t1; ny = ny == 1 ? 5 : ny == 2 || ny == 8 ? 4 : ny == 6 || ny == 9 ? 3 : ny;

      float xx = (wx / nx) * 0.01f, yy = (wy / ny) * 0.01f;
      bk.SetAttributeValue("transform", $"scale({XmlConvert.ToString(xx)} {XmlConvert.ToString(yy)})");
      bk.SetAttributeValue("width", XmlConvert.ToString(740 / xx));
      bk.SetAttributeValue("height", XmlConvert.ToString(237 / yy));

      for (int i = 0; i <= nx; i++)
      {
        var tt = XElement.Parse("<tspan>10<tspan dy=\"-5\" font-size=\"60%\"/></tspan>");
        var ti = tt.Elements().First(); if (i == 0) tt.Value = "0";
        else ti.Value = XmlConvert.ToString(i * mx / nx);
        sx.Add(new XElement(ns + "text", new XAttribute("x", i * wx / nx), tt));
      }
      for (int i = 1; i <= ny; i++) sy.Add(new XElement(ns + "text",
        new XAttribute("y", 237 - i * wy / ny), new XText(MathF.Round(i * oy / ny, 5) + (i == ny ? " ms" : null))));

      float fx = wx / 254f, fy = wy / oy;
      lb.SetAttributeValue("points", string.Join(' ', a1.Select((p, i) => XmlConvert.ToString(i * fx) + "," + XmlConvert.ToString(p * fy))));
      lr.SetAttributeValue("points", string.Join(' ', a2.Select((p, i) => XmlConvert.ToString(i * fx) + "," + XmlConvert.ToString(p * fy))));
      psvg.InnerHtml = esvg.ToString();
    }
    //void update_svg_old(HtmlElement? psvg)
    //{
    //  var a1 = passes[0].times.Take(passes[0].count);
    //  var a2 = passes[1].times.Take(passes[1].count);
    //  var fx = 740f / 256;
    //  var fy = 237f / 0.15f; //ms
    //  var s1 = string.Join(' ', a1.Select((p, i) => XmlConvert.ToString(i * fx) + "," + XmlConvert.ToString(p * fy)));
    //  var s2 = string.Join(' ', a2.Select((p, i) => XmlConvert.ToString(i * fx) + "," + XmlConvert.ToString(p * fy)));
    //  var t1 = esvg.Descendants().First(p => (string?)p.Attribute("name") == "B");
    //  var t2 = esvg.Descendants().First(p => (string?)p.Attribute("name") == "R");
    //  t1.SetAttributeValue("points", s1);
    //  t2.SetAttributeValue("points", s2);
    //  psvg.InnerHtml = esvg.ToString(); //psvg.ScrollIntoView(false);
    //}

    static Action<int, Func<bool>> _test_gcd()
    {
      var aa = new BigRational[256]; var bb = new BigInteger[256];
      var ar = new BigRational[256]; var br = new BigInteger[256];
      var ra = new Random(13); //E+2040
      for (int i = 0; i < 256; i++) bb[i] = (BigInteger)(aa[i] = random_rat(ra, i << 3));
      return test;
      void test(int pass, Func<bool> f)
      {
        if (pass == 0)
          for (int i = 1; f() && i < 256; i++)
            ar[i] = MathRexp.GreatestCommonDivisor(aa[i - 1], aa[i]);

        if (pass == 1)
          for (int i = 1; f() && i < 256; i++)
            br[i] = BigInteger.GreatestCommonDivisor(bb[i - 1], bb[i]);

        if (pass == 8) Debug.Assert(ar.Zip(br).All(p => p.First == p.Second));
      }
    }
    static Action<int, Func<bool>> _test_mul()
    {
      var aa = new BigRational[256]; var bb = new BigInteger[256];
      var ar = new BigRational[256]; var br = new BigInteger[256];
      var ra = new Random(13); //E+4080
      for (int i = 0; i < 256; i++) bb[i] = (BigInteger)(aa[i] = random_rat(ra, i << 4));//<< 3
      //Debug.Assert(aa.Zip(bb).All(p => p.First == p.Second));
      return test;
      void test(int pass, Func<bool> f)
      {
        if (pass == 0) for (int i = 1; f() && i < 256; i++) ar[i] = aa[i - 1] * aa[i];
        if (pass == 1) for (int i = 1; f() && i < 256; i++) br[i] = bb[i - 1] * bb[i];
        if (pass == 8) Debug.Assert(ar.Zip(br).All(p => p.First == p.Second));
      }
    }

    static BigRational random_rat(Random rnd, int digits)
    {
      var cpu = rat.task_cpu;
      cpu.pow(10, digits);
      var x = cpu.msb();
      for (int i = 0; i < x; i += 32)
      {
        var v = rnd.Next(); if (i + 32 >= x) v = (int)((long)v >> ((i + 32) - (int)x + 1));
        cpu.push(v); cpu.shl(i); cpu.xor();
      }
      //var y = cpu.msb(); if(x != y) { }
      var r = cpu.popr(); return r;
    }

    void button_save_Click(object sender, EventArgs e)
    {
      var dlg = new SaveFileDialog() { FileName = "BigRational-Benchmarks.htm", Filter = "HTML files|*.htm", DefaultExt = "htm" };
      if (dlg.ShowDialog(this) != DialogResult.OK) return;
      var s = wb.Document.GetElementsByTagName("html")[0].OuterHtml;
      try { File.WriteAllText(dlg.FileName, s); }
      catch (Exception ex) { MessageBox.Show(ex.Message); }
    }
  }
}
