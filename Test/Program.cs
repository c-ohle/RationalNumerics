﻿global using System.Diagnostics;
global using System.Numerics;
global using System.Numerics.Rational;
global using rat = System.Numerics.BigRational;

namespace Test
{
  internal unsafe static class Program
  {
    [STAThread]
    static void Main()
    {
      ApplicationConfiguration.Initialize(); //test();
      Application.Run(new MainFrame());
    }

#if true
    static void test()
    {
      VectorR a, b, c; string s;

      a = VectorR.Create(1, -2, MathR.Sin(Math.PI / 4), Math.PI);
      var h = a.GetHashCode();
      b = a * 2; if (a == b) { }
      b = a * 0.5f; if (a == b) { }
      b = b * 2; if (a == b) { }

      s = b.ToString();
      a = VectorR.Parse(s);

      a = VectorR.Create(1, 2787868.89979997, 909008809098.090890809m);
      b = a * a;
      a = VectorR.Create(-1, 33, -7.5, MathF.PI);
      b = VectorR.Create(88, -33, -7.8, -MathF.PI * 2);
      c = VectorR.Max(a, b);
      c = VectorR.Min(a, b);

      var cpu = rat.task_cpu;
      cpu.push(Math.PI); cpu.push(0); cpu.push(Math.Tau); cpu.push(Math.E);
      a = VectorR.Create(cpu, 4); cpu.pop(4);
      for (int i = 0; i < a.Length; i++) { var t = a[i]; }
    }
#endif
#if false
    static void test()
    {
      rat b; Vector2R a; string s;
      a = Vector2R.SinCosR(Math.PI + 1); s = a.ToString();
      a = Vector2R.SinCosR(Math.PI); s = a.ToString();
      a = Vector2R.SinCosR(Math.PI / 2); s = a.ToString();
      b = Vector2R.LengthR(a); var c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI / 4); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI / 4, 20); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI / 4, 35); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI / 4, 52); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(Math.PI); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(-Math.PI / 2); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(+Math.PI / 2); s = a.ToString();
      b = Vector2R.LengthR(a); c = MathR.Atan2(a.Y, a.X) * (180 / Math.PI);
      a = Vector2R.SinCosR(2 * Math.PI); s = a.ToString();
      b = Vector2R.LengthR(a);
    }
#endif
  }
}