#pragma warning disable CS8602
using Microsoft.Win32;
using System.Data;
using System.Xml;
using System.Xml.Linq;

namespace Test
{
  public partial class BenchmarksPage : UserControl
  {
    public BenchmarksPage()
    {
      InitializeComponent();
    }
    WebBrowser? wb;
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      wb = new WebBrowser
      {
        Dock = DockStyle.Fill,
        AllowNavigation = false,
        //AllowWebBrowserDrop = false,
        WebBrowserShortcutsEnabled = false,
        IsWebBrowserContextMenuEnabled = false
      };
      panel_webview.Controls.Add(wb);
      wb.DocumentCompleted += Wb_DocumentCompleted;
      wb.DocumentText =
      "<!DOCTYPE html><html><head>" +
        "<meta http-equiv='X-UA-Compatible' content='IE=11'/>" +
        "<style type='text/css'>" +
          "body { font-family:Cascadia Mono; font-size:10pt }" +
          "table,th,td { border:1px solid gray; border-collapse:collapse; padding:2pt; font-size:10pt; vertical-align:top }" +
          "th { background-color:#eeeeee }" +
          "h1 { font-size:14pt }" +
        //"h2 { font-size:12pt }" +
        "</style>" +
      "</head><body></body></html>";
    }
    void Wb_DocumentCompleted(object? sender, WebBrowserDocumentCompletedEventArgs? e)
    {
      add("Environment", "h1");
      var a = typeof(BigRational).Assembly; var name = a.GetName();
      add($"{nameof(BigRational)} Version: {name.Version} ImageRuntime: {a.ImageRuntimeVersion}");
      using (var key = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0"))//\\CentralProcessor\\0\\ProcessorNameString"))
        add($"Processor: {key.GetValue("ProcessorNameString")} ({key.GetValue("~MHz")} MHz) × {Environment.ProcessorCount}");
      if (MainFrame.debug)
      {
        var s = "Application is in debug or a debugger is attached - benchmarks are not realistic!";
        add($"<span style=\"color:red\">{s}<span/>");
      }
      add("...", "h6");
    }

    void button_add_Click(object sender, EventArgs e)
    {
      var svg = diag(750, 250);
      add(svg.ToString());
    }
    void button_add2_Click(object sender, EventArgs e)
    {
      //var doc = wb.Document; var body = doc.Body;
      //var par = checkBox_append.Checked ? body.AppendChild(doc.CreateElement("p")) : body.Children.Cast<HtmlElement>().LastOrDefault();
      //if (par == null) return;
      //par.InnerHtml = "<span style=\"font-size:7pt\">Small text " + DateTime.Now + "<span/>";
      //par.ScrollIntoView(false);
    }
    void button_add3_Click(object sender, EventArgs e)
    {
    }

    void button_save_Click(object sender, EventArgs e)
    {
      var dlg = new SaveFileDialog() { FileName = "BigRational-Benchmarks.htm", Filter = "HTML files|*.htm", DefaultExt = "htm" };
      if (dlg.ShowDialog(this) != DialogResult.OK) return;
      var s = wb.Document.GetElementsByTagName("html")[0].OuterHtml;
      try { File.WriteAllText(dlg.FileName, s); }
      catch (Exception ex) { MessageBox.Show(ex.Message); }
    }
    void button_clear_Click(object sender, EventArgs e)
    {
      wb.Document.Body.InnerHtml = "";
      Wb_DocumentCompleted(null, null);
    }

    HtmlElement add(string s, string tag = "p")
    {
      var doc = wb.Document; var body = doc.Body;
      var par = body.AppendChild(doc.CreateElement(tag));
      if (s.Length != 0 && s[0] == '<') par.InnerHtml = s; else par.InnerText = s;
      par.ScrollIntoView(false); return par;
    }
    static XElement diag(int dx, int dy)
    {
      var ff1 = new float[32]; var rnd = new Random(Environment.TickCount);
      for (int i = 0; i < ff1.Length; i++) ff1[i] = (float)rnd.NextDouble();
      var ff2 = new float[32];
      for (int i = 0; i < ff2.Length; i++) ff2[i] = (float)rnd.NextDouble();

      int rx = 10, ry = 13;
      var svg = new XElement("svg", new XAttribute("width", dx), new XAttribute("height", dy));
      var defs = new XElement("defs",
        new XElement("pattern", new XAttribute("id", "smallgrid"),
          new XAttribute("width", 10),
          new XAttribute("height", 10), new XAttribute("patternUnits", "userSpaceOnUse"),
            new XElement("path", new XAttribute("d", "M 10 0 L 0 0 0 10"),
              new XAttribute("fill", "none"), new XAttribute("stroke", "gray"),
              new XAttribute("stroke-width", "0.5"))),
        new XElement("pattern", new XAttribute("id", "grid"),
          new XAttribute("width", 100),
          new XAttribute("height", 100), new XAttribute("patternUnits", "userSpaceOnUse"),
            new XElement("rect", new XAttribute("width", 100), new XAttribute("height", 100),
              new XAttribute("fill", "url(#smallgrid)")),
            new XElement("path", new XAttribute("d", "M 100 0 L 0 0 0 100"),
              new XAttribute("fill", "none"), new XAttribute("stroke", "gray"),
              new XAttribute("stroke-width", "1"))));
      svg.Add(defs);
      svg.Add(new XElement("g", new XAttribute("transform", $"translate({rx},{dy - ry}) scale(1,-1)"),
        new XElement("rect",
          new XAttribute("fill", "url(#grid)"),
          new XAttribute("width", dx - rx),
          new XAttribute("height", dy - ry))));

      // points="0 10 100 50 200 20 300 50" 
      float x0 = rx, y0 = dy - ry, fx = dx / ff1.Length, fy = dy * 0.7f;
      svg.Add(new XElement("polyline", new XAttribute("stroke", "blue"), new XAttribute("fill", "none"),
        new XAttribute("points", string.Join(" ", ff1.Select((p, i) =>
        $"{XmlConvert.ToString(x0 + i * fx)} {XmlConvert.ToString(y0 - p * fy)}")))));
      svg.Add(new XElement("polyline", new XAttribute("stroke", "red"), new XAttribute("fill", "none"),
        new XAttribute("points", string.Join(" ", ff2.Select((p, i) =>
        $"{XmlConvert.ToString(x0 + i * fx)} {XmlConvert.ToString(y0 - p * fy)}")))));

      for (int x = 0; x < dx; x += 100)
        svg.Add(new XElement("text",
          new XAttribute("transform", $"translate({(x == 0 ? x : x + rx)},{dy}) scale(0.7,0.7)"),
          (x / 100).ToString()));
      for (int y = 100; y < dy; y += 100)
        svg.Add(new XElement("text",
          new XAttribute("transform", $"translate({0},{dy - y - ry}) scale(0.7,0.7)"),
          (y / 100).ToString()));
      return svg;
    }

  }
}
