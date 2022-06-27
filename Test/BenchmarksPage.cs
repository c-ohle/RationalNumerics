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
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
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
    }
    WebBrowser? wb;
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

    }
    void button_add_Click(object sender, EventArgs e)
    {
      //var doc = wb.Document;
      //var t1 = doc.GetElementById("5");
      //var t2 = XElement.Parse(t1.InnerHtml);
      //var t3 = t2.Descendants().First(p => (string?)p.Attribute("id") == "10");
      //t3.SetAttributeValue("points", "0,0 300,300");
      //t1.InnerHtml = t2.ToString();
      return;
    }
    void button_add2_Click(object sender, EventArgs e)
    {
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
    }
  }
}
