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
#if DEBUG
      var dbg = true;
#else
      var dbg = System.Diagnostics.Debugger.IsAttached;
#endif
      labelDebug.Visible = dbg;

      tabControl.SelectedIndex = 3;
    }
  }
}
