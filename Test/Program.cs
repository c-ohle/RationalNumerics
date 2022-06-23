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
      ApplicationConfiguration.Initialize(); //test();
      Application.Run(new MainFrame());
    }
     
    static unsafe void test()
    {
      var cpu = rat.task_cpu; double a; rat b;  
      
      b = (rat)1 / 17; //0.0'5882352941176470
      b = (rat)1 / 19; //0.0'526315789473684210
      b = (rat)1 / 23; //0.0'4347826086956521739130
        
      a = Math.Pow(2, 0.5); b = MathR.Pow2(0.5);
      a = Math.Pow(2, 23.456); b = MathR.Pow2(23.456);

      a = Math.Atan2(+2.1, +0.5); b = MathR.Atan2(+2.1, +0.5);
      a = Math.Atan2(+2.1, -0.5); b = MathR.Atan2(+2.1, -0.5);
      a = Math.Atan2(-2.1, -0.5); b = MathR.Atan2(-2.1, -0.5);
      a = Math.Atan2(-2.1, +0.5); b = MathR.Atan2(-2.1, +0.5);

      a = Math.Atan2(0, +0.5); b = MathR.Atan2(0, +0.5);
      a = Math.Atan2(0, -0.5); b = MathR.Atan2(0, -0.5);
      a = Math.Atan2(+0.5, 0); b = MathR.Atan2(+0.5, 0);
      a = Math.Atan2(-0.5, 0); b = MathR.Atan2(-0.5, 0);
      //a = Math.Atan2(-0.0, 0); b = MathR.Atan2(-0.0, 0);

      MathR.DefaultDigits = 20;
      b = MathR.Atan2(-0.5, 0);
      MathR.DefaultDigits = 100;
      b = MathR.Atan2(-0.5, 0);
      MathR.DefaultDigits = 0;
      b = MathR.Atan2(-0.5, 0);

      b = a;
    }
  }
}