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
      tabControl.SelectedIndex = 2;
    }
#if DEBUG
    internal static readonly bool debug = true;
#else
    internal static readonly bool debug = System.Diagnostics.Debugger.IsAttached;
#endif
  }
}
