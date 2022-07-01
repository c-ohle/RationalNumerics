using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
  public partial class MainFrame : Form
  {
    public MainFrame()
    {
      InitializeComponent();
      labelDebug.Visible = debug;
      //tabControl.SelectedIndex = 2;
    }
#if DEBUG
    internal static readonly bool debug = true;
#else
    internal static readonly bool debug = System.Diagnostics.Debugger.IsAttached;
#endif
  }

  public class GCSpyControl : Label
  {
    [Browsable(false)]
    public new string? Text { get; set; }
#if false
    protected override void OnHandleCreated(EventArgs e)
    {
      base.OnHandleCreated(e);
      Application.Idle += Application_Idle;
    }
    long icoll, icollab, last, icount, count, loops;
    void Application_Idle(object? sender, EventArgs e)
    {
      var c = GC.GetTotalMemory(false);
      if (last != c)
      {
        var p = GC.GetGCMemoryInfo();
        if (icoll != p.Index)
        {
          icoll = p.Index; if (icollab == 0) icollab = icoll; else loops += icoll - icollab;
          last = c;
        }
        else
        {
          count += c - last; last = c;
        }
        base.Text = $"{count / 1024}kb {loops}gc";
      }
  }
    protected override void OnClick(EventArgs e)
    {
      base.OnClick(e);
      icoll = icollab = last = count = loops = 0;
    }
#endif
  }

}
