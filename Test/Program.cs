global using System.Diagnostics;
global using System.Numerics.Rational;
global using rat = System.Numerics.Rational.NewRational;
//global using old = Test.BigRational;
using System.Buffers;
using System.Drawing.Imaging;
using System.Numerics;

namespace Test
{
  internal static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); test();
      Application.Run(new MainFrame());
      //var form = new Form { Text = "Under construction", Width = 400, Height = 400 };
      //form.Controls.Add(new MandelbrotView { Dock = DockStyle.Fill });
      //Application.Run(form);
    }

    static void test()
    {
    }
  }

}