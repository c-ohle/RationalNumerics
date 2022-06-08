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
      Application.Run(new MainFrame());
    }
   
    static void test()
    {
      // double a; rat b, c;
      // c = (rat)"0.7853981633974483096156608458198757210492923498437764552437361480";
      // a = Math.Atan(1); b = rat.Atan(1, 10); b = rat.Atan(1, 20); 
      // c = (rat)"0.4636476090008061162142562314612144020285370542861202638109330887";
      // a = Math.Atan(0.5); b = rat.Atan(0.5, 20); //b = Atan(0.5, 40); b = Atan(0.5, 80);
      // c = (rat)"1.10714871779409050301706546017853704007004764540143264667653920743";
      // a = Math.Atan(2); b = rat.Atan(2, 20); //b = Atan(2, 40); b = Atan(2, 80); b = Atan(2, 120);
      // 
      // c = (rat)"0.0996686524911620273784461198780205902432783225043146480155087768";
      // a = Math.Atan(0.1); b = rat.Atan(0.1, 5); b = rat.Atan(0.1, 10); b = rat.Atan(0.1, 20);
      // 
      // c = (rat)"1.5607966601082313810249815754304718935372153471431762708595328779";
      // a = Math.Atan(100); b = rat.Atan(100, 20); //b = Atan(100, 40); b = Atan(100, 80); b = Atan(100, 300);
      // 
      // a = Math.Atan(1234578910); b = rat.Atan(1234578910, 20); //b = Atan(100, 40); b = Atan(100, 80); b = Atan(100, 300);
      // 
      // a = Math.Atan(-100); b = rat.Atan(-100, 20); //b = Atan(100, 40); b = Atan(100, 80); b = Atan(100, 300);
      // 
      // a = Math.Atan(0); b = rat.Atan(0, 20); //b = Atan(100, 40); b = Atan(100, 80); b = Atan(100, 300);
    }

    
  }
}