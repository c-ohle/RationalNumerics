global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;

namespace Test
{
  internal static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); test();
      //Application.EnableVisualStyles();
      //Application.SetCompatibleTextRenderingDefault(false);
      //Application.SetHighDpiMode(HighDpiMode.SystemAware);
      Application.Run(new MainFrame());
    }
    
    static void test() 
    {
      //var a = rat.Parse("1.4142135623730950488016887242097");
      //var b = rat.Pow(2, (rat)1 / 2, 30);
    }
  }
}