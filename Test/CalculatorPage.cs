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
  public partial class CalculatorPage : UserControl
  {
    public CalculatorPage()
    {
      InitializeComponent();
    }
    void basket_Click(object sender, EventArgs e)
    {
      panel_hist.Controls.Clear();
    }
  }
}
