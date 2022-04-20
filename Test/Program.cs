global using System.Diagnostics;
global using System.Numerics.Rational;
global using rat = System.Numerics.Rational.NewRational;
global using old = Test.BigRational;
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
      var form = new Form { Text = "Under construction", Width = 400, Height = 400 };
      //form.Controls.Add(new MandelbrotView { Dock = DockStyle.Fill });
      Application.Run(form);
    }
                                
    static void test()
    {
    }
  }

  class MandelbrotView : Control
  {
    Bitmap? bmp;
    static uint col(int r, int g, int b) { return 0xff000000 | (uint)((r << 24) | (g << 8) | b); }
    static uint[] colors = {
      col(66, 30, 15),
      col(25, 7, 26),
      col(9, 1, 47),
      col(4, 4, 73),
      col(0, 7, 100),
      col(12, 44, 138),
      col(24, 82, 177),
      col(57, 125, 209),
      col(134, 181, 229),
      col(211, 236, 248),
      col(241, 233, 191),
      col(248, 201, 95),
      col(255, 170, 0),
      col(204, 128, 0),
      col(153, 87, 0),
      col(106, 52, 3),
    };

    rat xmin = -2.0, xmax = +0.7;
    rat ymin = -1.2, ymax = +1.2;
    rat qmax = 4;
    int imax = 16;

    protected override unsafe void OnPaint(PaintEventArgs e)
    {
      var size = ClientSize; //size = new Size(300, 300);
      if (bmp == null || bmp.Size != size)
      {
        if (bmp != null) bmp.Dispose();
        bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppRgb);
      }

      var lb = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
      var pp = (uint*)lb.Scan0.ToPointer();

      var t1 = Environment.TickCount;
      int dx = bmp.Width, dy = bmp.Height;
      var dp = lb.Stride >> 2;
      var fx = (xmax - xmin) / dx;
      var fy = (ymax - ymin) / dy;
#if true
      var cpu = rat.task_cpu; var m1 = cpu.mark();
      cpu.push(ymin); cpu.push(fy); //y, fy, 
      cpu.push(xmin); cpu.push(fx); //x, fx
      var m = cpu.mark();
      for (int py = 0; py < dy; py++, pp += dp)
      {
        for (int px = 0; px < dx; px++)
        {
          //var a = x; var b = y;
          cpu.dup(m1 + 2); cpu.dup(m1 + 0); 
          int i = 0;
          for (; i < imax; i++)
          {
            // var u = a * a - b * b + x;
            cpu.mul(m + 0, m + 0); cpu.mul(m + 1, m + 1); cpu.sub(); cpu.add(m1 + 2);
            cpu.lim(32); //cpu.rnd(8);
            // var v = 2 * a * b + y;
            cpu.mul(m + 0, m + 1); cpu.shl(1); cpu.add(m1 + 0);
            cpu.lim(32); //cpu.rnd(8);
            // if (u * u + v * v > 4) break;
            cpu.mul(m + 2, m + 2); cpu.mul(m + 3, m + 2); cpu.add();
            if (cpu.cmp(qmax) > 0) { cpu.pop(3); break; }
            // a = u; b = v;
            cpu.swp(m + 0, m + 2); cpu.swp(m + 1, m + 3); cpu.pop(3);
          }
          cpu.pop(2);
          pp[px] = i < imax ? colors[i] : 0;
          cpu.add(1, 0); cpu.norm(1); // x += fx;
        }
        cpu.add(3, 2); cpu.norm(3); // y += fy;
        cpu.push(xmin); cpu.swp(m1 + 2, m1 + 4); cpu.pop(); // x = xmin
      }
      cpu.pop(4);
#else
      for (int py = 0; py < dy; py++, pp += dp)
      {
        var y = ymin + py * fy;
        for (int px = 0; px < dx; px++)
        {
          var x = xmin + px * fx;
          int i = 0; var a = x; var b = y;
          for (; i < imax; i++)
          {
            var u = a * a - b * b + x;
            var v = 2 * a * b + y;
            u = rat.Round(u, 8);
            v = rat.Round(v, 8);
            a = u; b = v;
            if (a * a + b * b > qmax) break;
          }
          pp[px] = i < imax ? colors[i] : 0;
        }
      }
#endif
      var t2 = Environment.TickCount;

      bmp.UnlockBits(lb);
      e.Graphics.DrawImage(bmp, 0, 0);
      e.Graphics.DrawString($"{t2 - t1} ms", SystemFonts.DefaultFont, Brushes.White, 4, 4);
    }
    protected override void OnSizeChanged(EventArgs e)
    {
      Invalidate();
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
      Invalidate();
    }
  }

}