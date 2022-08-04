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
      var path = Path.GetFullPath("templ\\benchmarks2.htm");
      wb = new WebBrowser
      {
        Dock = DockStyle.Fill,
        AllowNavigation = false,
        WebBrowserShortcutsEnabled = false,
        IsWebBrowserContextMenuEnabled = false,
        ScriptErrorsSuppressed = true,
        ScrollBarsEnabled = false,
        //AllowWebBrowserDrop = false,
        Url = new Uri(path)
      };
      wb.DocumentCompleted += wb_ready;
      panel_webview.Controls.Add(wb);
    }

    void textBox1_Leave(object sender, EventArgs e)
    {
      //try { rat.CPU.klim = (uint)int.Parse(textBox1.Text); }
      //catch { }
      //textBox1.Text = rat.CPU.klim.ToString();
    }

    WebBrowser? wb; XElement? esvg;//HtmlElement? p5; 
    void wb_ready(object? sender, WebBrowserDocumentCompletedEventArgs? e)
    {
      var doc = wb.Document; var body = doc.Body;
      var t0 = RuntimeInformation.FrameworkDescription;
      var t2 = RuntimeInformation.OSDescription;
      var t3 = RuntimeInformation.OSArchitecture;
      var t4 = RuntimeInformation.ProcessArchitecture;
      var t5 = RuntimeInformation.RuntimeIdentifier;
      doc.GetElementById("0").InnerText = $"{t2} {t3} Runtime: {t0} {t4}";

      var a = typeof(BigRational).Assembly; var name = a.GetName();
      var t1 = a.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false).FirstOrDefault() as System.Runtime.Versioning.TargetFrameworkAttribute;
      doc.GetElementById("1").InnerText = $"{name.Version.ToString()} Runtime: {a.ImageRuntimeVersion} {t1?.FrameworkName}";
      //doc.GetElementById("2").InnerText = $"{a.ImageRuntimeVersion} Framework: ({t1?.FrameworkName}";
      using (var key = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0"))//\\CentralProcessor\\0\\ProcessorNameString"))
        doc.GetElementById("3").InnerText = $"{key.GetValue("ProcessorNameString")} ({key.GetValue("~MHz")} MHz) × {Environment.ProcessorCount}";
      if (!MainFrame.debug) doc.GetElementById("4").OuterHtml = "";//.Style = "visibility: collapse";
      esvg = XElement.Parse(doc.GetElementById("svg").InnerHtml);
    }

    (float[] times, int count)[]? passes;

    void runtest(HtmlElement svg, Action<int, Func<bool>> test, int ex)
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

      update_svg(svg, ex);

      var a1 = passes[0].times.Take(255);//.Average();
      var a2 = passes[1].times.Take(255);//.Average();
      var av = MathF.Round(a1.Average() * 100 / a2.Average());
      var mi = Math.Min(a1.Max(), a2.Max());
      var ma = Math.Max(a1.Max(), a2.Max());

      var info = svg.NextSibling;
      var s = info.InnerHtml;
      var x = s.LastIndexOf(':'); if (x > 0) s = s.Substring(0, x + 1);
      s += $" {av} % (min {mi} ms; max {ma} ms)";
      info.InnerHtml = s;
      s = svg.Parent.Style;
      if (s.Contains("none")) svg.Parent.Style = s.Replace("none", "block"); // "display: block; margin-left: 16px";
    }

    void button_run_Click(object sender, EventArgs e)
    {
      if (esvg == null) return; //if (task != null) return;

      Cursor = Cursors.WaitCursor;
      var test = _test_mul(out var ex); var tag = wb.Document.GetElementById("5"); var old = tag.FirstChild;
      runtest(tag, test, ex);
      if (old == null) tag.ScrollIntoView(false);
      for (int i = 0; i < 3; i++) { Thread.Sleep(0); Application.DoEvents(); }

      test = _test_div(out ex); tag = wb.Document.GetElementById("6"); old = tag.FirstChild;
      runtest(tag, test, ex);
      if (old == null) tag.ScrollIntoView(false);
      for (int i = 0; i < 3; i++) { Thread.Sleep(0); Application.DoEvents(); }

      test = _test_gcd(out ex); tag = wb.Document.GetElementById("7");
      runtest(tag, test, ex);
      if (old == null) tag.ScrollIntoView(false);
      for (int i = 0; i < 3; i++) { Thread.Sleep(0); Application.DoEvents(); }

      test = _test_sqr(out ex); tag = wb.Document.GetElementById("8");
      runtest(tag, test, ex);
      if (old == null) tag.ScrollIntoView(false);
      for (int i = 0; i < 3; i++) { Thread.Sleep(0); Application.DoEvents(); }

      test = _test_bigint_biginteger_1(out ex); tag = wb.Document.GetElementById("9");
      runtest(tag, test, ex);
      if (old == null) tag.ScrollIntoView(false);
      for (int i = 0; i < 3; i++) { Thread.Sleep(0); Application.DoEvents(); }

      Cursor = Cursors.Default;
      return;
    }

    void update_svg(HtmlElement? psvg, int ex)
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
      var mx = ex; var nx = 10;
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

    static Action<int, Func<bool>> _test_mul(out int e)
    {
      var rr = new BigRational[256]; var ii = new BigInteger[256];
      var cr = new BigRational[256]; var ci = new BigInteger[256];
      var ra = new Random(13);
      for (int i = 0; i < 256; i++) ii[i] = (BigInteger)(rr[i] = random_rat(ra, i << 4)); //E+4080
      e = rat.ILog10(rr[255]);
      return (pass, f) =>
      {
        if (pass == 0) for (int i = 1; f() && i < 256; i++) cr[i] = rr[i - 1] * rr[i];
        if (pass == 1) for (int i = 1; f() && i < 256; i++) ci[i] = ii[i - 1] * ii[i];
        if (pass == 8) Debug.Assert(cr.Zip(ci).All(p => p.First == p.Second));
      };
    }
    static Action<int, Func<bool>> _test_sqr(out int e)
    {
      var rr = new BigRational[256]; var ii = new BigInteger[256];
      var cr = new BigRational[256]; var ci = new BigInteger[256];
      var ra = new Random(13);
      for (int i = 0; i < 256; i++) ii[i] = (BigInteger)(rr[i] = random_rat(ra, i << 4)); //E+4080
      e = rat.ILog10(rr[255]);
      return (pass, f) =>
      {
        if (pass == 0) for (int i = 1; f() && i < 256; i++) cr[i] = rr[i] * rr[i];
        if (pass == 1) for (int i = 1; f() && i < 256; i++) ci[i] = ii[i] * ii[i];
        if (pass == 8) Debug.Assert(cr.Zip(ci).All(p => p.First == p.Second));
      };
    }
    static Action<int, Func<bool>> _test_div(out int e)
    {
      var rr = new BigRational[256]; var ii = new BigInteger[256];
      var cr = new BigRational[256]; var ci = new BigInteger[256];
      var ra = new Random(13);
      for (int i = 0; i < 256; i++) ii[i] = (BigInteger)(rr[i] = random_rat(ra, i << 6));
      e = rat.ILog10(rr[255]);
      return (pass, f) =>
      {
        if (pass == 0) for (int i = 1; f() && i < 256; i++) cr[i] = BigRational.IDiv(rr[i], rr[i - 1]);
        if (pass == 1) for (int i = 1; f() && i < 256; i++) ci[i] = ii[i] / (ii[i - 1]);
        if (pass == 8) Debug.Assert(cr.Zip(ci).All(p => p.First == p.Second));
      };
    }
    static Action<int, Func<bool>> _test_gcd(out int e)
    {
      var rr = new BigRational[256]; var ii = new BigInteger[256];
      var cr = new BigRational[256]; var ci = new BigInteger[256];
      var ra = new Random(13); //E+2040
      for (int i = 0; i < 256; i++) ii[i] = (BigInteger)(rr[i] = random_rat(ra, i << 3));
      e = rat.ILog10(rr[255]);
      return (pass, f) =>
      {
        if (pass == 0) for (int i = 1; f() && i < 256; i++) cr[i] = BigRational.GreatestCommonDivisor(rr[i - 1], rr[i]);
        if (pass == 1) for (int i = 1; f() && i < 256; i++) ci[i] = BigInteger.GreatestCommonDivisor(ii[i - 1], ii[i]);
        if (pass == 8) Debug.Assert(cr.Zip(ci).All(p => p.First == p.Second));
      };
    }

    static Action<int, Func<bool>> _test_bigint_biginteger_1(out int e)
    {
      var rr = new BigInt[256]; var ii = new BigInteger[256];
      var cr = new BigInt[256]; var ci = new BigInteger[256];
      var ra = new Random(13); //E+2040
      for (int i = 0; i < 256; i++) ii[i] = (BigInteger)(rr[i] = (BigInt)random_rat(ra, i << 3));
      e = rat.ILog10(rr[255]);
      return (pass, f) =>
      {
        // (a * b + a * b - a + a * a) / a; // a * a -> sqr() 
        if (pass == 0) for (int i = 1; f() && i < 256; i++) cr[i] = 0 | (rr[i] * rr[i - 1] + rr[i] * rr[i - 1] - rr[i] + rr[i] * rr[i]) / rr[i];
        if (pass == 1) for (int i = 1; f() && i < 256; i++) ci[i] = (ii[i] * ii[i - 1] + ii[i] * ii[i - 1] - ii[i] + ii[i] * ii[i]) / ii[i];
        if (pass == 8) Debug.Assert(cr.Zip(ci).All(p => p.First == p.Second)); // ii[1+(i>>2)]
      };
    }


    static BigRational random_rat(Random rnd, int digits) //todo: make more precise
    {
      var cpu = rat.task_cpu;
      cpu.pow(10, digits);
      var x = cpu.msb();
      for (uint i = 0; i < x; i += 32)
      {
        var v = rnd.Next(); if (i + 32 >= x) v = (int)((long)v >> ((int)(i + 32) - (int)x + 1));
        cpu.push(v); cpu.shl(i); cpu.xor();
      }
      var r = cpu.popr(); return r;
    }

    void button_save_Click(object sender, EventArgs e)
    {
      var dlg = new SaveFileDialog() { FileName = "BigRational-Benchmarks.html", Filter = "HTML files|*.html;*.htm", DefaultExt = "htm" };
      if (dlg.ShowDialog(this) != DialogResult.OK) return;
      var s = wb.Document.GetElementsByTagName("html")[0].OuterHtml;
      try { File.WriteAllText(dlg.FileName, s); }
      catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

  }
}
