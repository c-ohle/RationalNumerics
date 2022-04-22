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
      var form = new Form { Text = "Under construction", Width = 400, Height = 400 };
      form.Controls.Add(new MandelbrotView { Dock = DockStyle.Fill });
      Application.Run(form);
    }

    static void test()
    {      
    }
  }

  unsafe class MandelbrotView : Control
  {
    internal MandelbrotView()
    {
      ToolStripMenuItem t2, t1, t0;
      (this.ContextMenuStrip = new ContextMenuStrip()).Items.AddRange(new ToolStripItem[] {
        t2 = new ToolStripMenuItem("NewRational Driver", null, (p,e) => { driver = 2; update(); }),
        t1 = new ToolStripMenuItem("BigInteger Driver", null, (p,e) => { driver = 1; update();}),
        t0 = new ToolStripMenuItem("Double Driver", null, (p,e) => { driver = 0; update();}),
        new ToolStripSeparator(),
        new ToolStripMenuItem("Reset", null, (p,e) => { stop();
          mx = -0.7; my = 0; scale = 1.2f; imax=32; update(); })
      });
      this.ContextMenuStrip.Opening += (p, e) =>
      {
        t0.Checked = driver == 0;
        t1.Checked = driver == 1;
        t2.Checked = driver == 2;
      };
    }

    Bitmap? bmp; uint* scan; int dx, dy, stride, driver = 2;
    Task? task; bool cancel; long t1, t2;
    System.Windows.Forms.Timer? timer; Action<int>? tool;

    rat mx = -0.7, my = 0, scale = 1.2f;
    rat xmin, xmax, ymin, ymax, qmax = 4; int imax = 32;

    void mandel_dbl()
    {
      calcmap(); if (map == null) return;
      var scalx = scale * dx / dy;
      this.xmin = mx - scalx; this.xmax = mx + scalx;
      this.ymin = my - scale; this.ymax = my + scale;

      var xmin = (double)this.xmin; var xmax = (double)this.xmax;
      var ymin = (double)this.ymin; var ymax = (double)this.ymax; var qmax = (double)this.qmax;
      var fx = (xmax - xmin) / dx;
      var fy = (ymax - ymin) / dy;
      t1 = Environment.TickCount;
      //for (int py = 0; py < dy; py++)
      Parallel.For(0, dy, (py, po) =>
      {
        if (cancel) { po.Break(); return; }
        var p = scan + py * (stride >> 2);
        var y = ymin + py * fy;
        for (int px = 0; px < dx; px++)
        {
          var x = xmin + px * fx;
          int i = 0; var a = x; var b = y;
          for (; i < imax; i++)
          {
            var u = a * a - b * b + x;
            var v = 2 * a * b + y;
            if (u * u + v * v > qmax) break;
            a = u; b = v;
          }
          p[px] = i < imax ? map[i] : 0;
        }
      }
      );
      t2 = Environment.TickCount;
    }
    void mandel_big()
    {
      calcmap(); if (map == null) return;
      var scalx = scale * dx / dy;
      this.xmin = mx - scalx; this.xmax = mx + scalx;
      this.ymin = my - scale; this.ymax = my + scale;

      var xmin = (BigRational)this.xmin; var xmax = (BigRational)this.xmax;
      var ymin = (BigRational)this.ymin; var ymax = (BigRational)this.ymax; var qmax = (BigRational)this.qmax;
      var fx = (xmax - xmin) / dx;
      var fy = (ymax - ymin) / dy;
      t1 = Environment.TickCount;
      //for (int py = 0; py < dy; py++)
      Parallel.For(0, dy, (py, po) =>
      {
        if (cancel) { po.Break(); return; }
        var p = scan + py * (stride >> 2);
        var y = ymin + py * fy;
        for (int px = 0; px < dx; px++)
        {
          var x = xmin + px * fx;
          int i = 0; var a = x; var b = y;
          for (; i < imax; i++)
          {
            var u = a * a - b * b + x; u = BigRational.Round(u, 8);
            var v = 2 * a * b + y; v = BigRational.Round(v, 8);
            if (u * u + v * v > qmax) break;
            a = u; b = v;
          }
          p[px] = i < imax ? map[i] : 0;
        }
      }
      );
      t2 = Environment.TickCount;
    }
    void mandel_rat()
    {
      calcmap(); if (map == null) return;
      var scalx = scale * dx / dy;
      xmin = mx - scalx; xmax = mx + scalx;
      ymin = my - scale; ymax = my + scale;

      var fx = (xmax - xmin) / dx;
      var fy = (ymax - ymin) / dy;
      t1 = Environment.TickCount;
      //for (int py = 0; py < dy; py++)
      Parallel.For(0, dy, (py, po) =>
      {
        if (cancel) { po.Break(); return; }
        var p = scan + py * (stride >> 2);
        var cpu = rat.task_cpu; var m = cpu.mark();
        //var y = ymin + py * fy;
        cpu.push(ymin); cpu.push(fy); cpu.push(py); cpu.mul(); cpu.add(); cpu.norm();
        cpu.push(xmin); //var x = xmin;
        for (int px = 0; px < dx; px++)
        {
          int i = 0; //var a = x; var b = y;
          cpu.dup(m + 1); // var a = x;
          cpu.dup(m + 0); // var b = y;
          for (; i < imax; i++)
          {
            // var u = a * a - b * b + x;
            cpu.sqr(m + 2); cpu.sqr(m + 3); cpu.sub(); cpu.add(m + 1); cpu.lim(64);
            // var v = 2 * a * b + y;
            cpu.mul(m + 2, m + 3); cpu.shl(1); cpu.add(m + 0); cpu.lim(64);
            // if (u * u + v * v > 4) break;
            cpu.sqr(m + 4); cpu.sqr(m + 5); cpu.add();
            if (cpu.cmp(qmax) > 0) { cpu.pop(3); break; }
            // a = u; b = v;
            cpu.swp(m + 2, m + 4); cpu.swp(m + 3, m + 5); cpu.pop(3);
          }
          cpu.pop(2); // a, b
          p[px] = i < imax ? map[i] : 0;
          cpu.add(fx); cpu.norm(); //x += fx;
        }
        cpu.pop(2);
      }
      );
      t2 = Environment.TickCount;
    }

    void start()
    {
      stop(); var size = ClientSize;
      if (bmp == null || bmp.Size != size)
      {
        if (bmp != null) bmp.Dispose();
        bmp = new Bitmap(dx = size.Width, dy = size.Height, PixelFormat.Format32bppRgb);
        var lb = bmp.LockBits(new Rectangle(0, 0, dx, dy), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
        scan = (uint*)lb.Scan0.ToPointer(); stride = lb.Stride; bmp.UnlockBits(lb);
      }
      task = new Task(driver == 0 ? mandel_dbl : driver == 1 ? mandel_big : mandel_rat);
      cancel = false; task.Start(); timer?.Start();
    }
    void stop()
    {
      if (task != null) { timer?.Stop(); cancel = true; task.Wait(); task.Dispose(); task = null; }
    }
    void update()
    {
      start(); Invalidate(); //task?.Wait(); Update();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
      timer = new() { Interval = 100 }; timer.Tick += Timer_Tick;
    }
    protected override void OnHandleDestroyed(EventArgs e)
    {
      if (task != null) { cancel = true; task.Wait(); task.Dispose(); task = null; }
      if (timer != null) { timer.Tick -= Timer_Tick; timer.Dispose(); timer = null; }
      if (bmp != null) { bmp.Dispose(); bmp = null; }
    }
    protected override void OnPaintBackground(PaintEventArgs pevent) { }
    protected override void OnPaint(PaintEventArgs e)
    {
      if (bmp == null || bmp.Size != ClientSize) { start(); }
      if (bmp != null) e.Graphics.DrawImage(bmp, 0, 0);
      if (task != null && task.IsCompleted)
        e.Graphics.DrawString($"{imax} iterations {t2 - t1} ms", SystemFonts.DefaultFont, Brushes.White, 4, 4);
    }
    void Timer_Tick(object? sender, EventArgs e)
    {
      Invalidate(); //Update();
      if (task != null && task.IsCompleted) { timer?.Stop(); return; }
    }
    protected override void OnSizeChanged(EventArgs e)
    {
      Invalidate(); //Update();
    }

    Action<int> tool_move()
    {
      var p1 = Cursor.Position; var po = p1;
      var x1 = this.mx; var y1 = this.my;
      return id =>
      {
        if (id == 0)
        {
          var p2 = Cursor.Position;
          this.mx = x1 - (2 * (p2.X - p1.X)) * scale / dy;
          this.my = y1 - (2 * (p2.Y - p1.Y)) * scale / dy;

          stop();
          if (bmp != null)
            using (var gr = Graphics.FromImage(bmp))
            {
              var sx = p2.X - po.X; var sy = p2.Y - po.Y;
              gr.DrawImage(bmp, sx, sy);
              if (sx > 0) gr.FillRectangle(Brushes.Black, 0, 0, sx, dy);
              if (sx < 0) gr.FillRectangle(Brushes.Black, dx + sx, 0, -sx, dy);
              if (sy > 0) gr.FillRectangle(Brushes.Black, 0, 0, dx, sy);
              if (sy < 0) gr.FillRectangle(Brushes.Black, 0, dy + sy, dx, -sy);
              po = p2;
            }
          update();
        }
      };
    }
    Action<int> tool_imax()
    {
      var p1 = Cursor.Position; var x1 = imax;
      return id =>
      {
        if (id == 0)
        {
          var p2 = Cursor.Position;
          var i = Math.Max(16, Math.Min(1000, x1 + p1.Y - p2.Y));
          if (imax != i) { stop(); imax = i; update(); }
        }
      };
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        Capture = true; var keys = ModifierKeys;
        tool =
          keys == Keys.Shift ? tool_imax() :
          tool_move();
      }
      //start();
      base.OnMouseDown(e);
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
      if (tool != null) { tool(0); }
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
      if (tool != null) { tool(1); tool = null; Capture = false; }
      base.OnMouseUp(e);
    }
    protected override void OnMouseLeave(EventArgs e)
    {
      if (tool != null) { tool(2); tool = null; Capture = false; }
      base.OnMouseLeave(e);
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
      var p = e.Location;
      var d = 1 - e.Delta * (1f / 120) * 0.1f;

      var x1 = (dx - p.X * 2) * scale / dy;
      var y1 = (dy - p.Y * 2) * scale / dy;

      scale *= d;

      var x2 = (dx - p.X * 2) * scale / dy;
      var y2 = (dy - p.Y * 2) * scale / dy;

      mx += x2 - x1;
      my += y2 - y1;
      update();
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);
      if (e.KeyCode == Keys.Space) update();
    }
    static uint col(int r, int g, int b) { return 0xff000000 | (uint)((r << 16) | (g << 8) | b); }
    static uint[] colors = { col(66, 30, 15), col(25, 7, 26), col(9, 1, 47), col(4, 4, 73), col(0, 7, 100), col(12, 44, 138), col(24, 82, 177), col(57, 125, 209), col(134, 181, 229), col(211, 236, 248), col(241, 233, 191), col(248, 201, 95), col(255, 170, 0), col(204, 128, 0), col(153, 87, 0), col(106, 52, 3) };
    uint[]? map = null;
    void calcmap()
    {
      if (map != null && map.Length == imax) return;
      map = new uint[imax];
      for (uint i = 0; i < imax; i++)
      {
        var z = i * (15 * 0x100) / (uint)imax;
        var y = z >> 8; if (y + 1 >= 16) break; // Debug.Assert(y + 1 < 16);
        var f1 = z & 0xff; var f2 = 0x100 - f1;
        var b = ((colors[y] & 0xff) * f2 + (colors[y + 1] & 0xff) * f1) >> 8;
        var g = (((colors[y] >> 8) & 0xff) * f2 + ((colors[y + 1] >> 8) & 0xff) * f1) >> 8;
        var r = (((colors[y] >> 16) & 0xff) * f2 + ((colors[y + 1] >> 16) & 0xff) * f1) >> 8;
        map[i] = 0xff000000 | (r << 16) | (g << 8) | b;
      }
    }
  }
}