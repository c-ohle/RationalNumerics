using System.ComponentModel;
using System.Drawing.Imaging;
using System.Numerics;
using System.Numerics.Rational;
using System.Runtime.InteropServices;

namespace Test
{
  [DesignerCategory("Code")]
  public unsafe class MandelbrotView : UserControl
  {
    public MandelbrotView()
    {
      //ToolStripMenuItem t2, t1, t0;
      //(this.ContextMenuStrip = new ContextMenuStrip()).Items.AddRange(new ToolStripItem[] {
      //  t2 = new ToolStripMenuItem("NewRational Driver", null, (p,e) => { driver = 2; update(); }),
      //  t1 = new ToolStripMenuItem("BigInteger Driver", null, (p,e) => { driver = 1; update();}),
      //  t0 = new ToolStripMenuItem("Double Driver", null, (p,e) => { driver = 0; update();}),
      //  new ToolStripSeparator(),
      //  new ToolStripMenuItem("Reset", null, (p,e) => { stop();
      //    mx = -0.7; my = 0; scale = 1.2f; imax=32; update(); })
      //});
      //this.ContextMenuStrip.Opening += (p, e) =>
      //{
      //  t0.Checked = driver == 0;
      //  t1.Checked = driver == 1;
      //  t2.Checked = driver == 2;
      //};
    }
    public bool Manual
    {
      get => manual;
      set { if (manual == value) return; stop(); manual = value; delaystart(); }
    }
    public enum MandelDriver { BigRat = 3, BigRational = 2, BigInteger = 1, Double = 0 }
    public MandelDriver Driver
    {
      get => (MandelDriver)driver;
      set { if (value == Driver) return; stop(); driver = (int)value; delaystart(); }
    }
    public int Iterations
    {
      get => imax;
      set
      {
        //value = Math.Max(4, Math.Min(1000, value));
        if (imax == value) return; stop(); imax = value; delaystart();
      }
    }
    public int Lim
    {
      get => lim;
      set
      {
        value = Math.Max(4, Math.Min(1000, value)); if (value == lim) return;
        stop(); lim = value; delaystart();
      }
    }
    public BigRational Scaling
    {
      get => scale;
      set
      {
        if (value == scale) return;
        stop(); scale = value; delaystart();
      }
    }
    public BigRational CenterX
    {
      get => mx;
      set
      {
        if (value == mx) return;
        stop(); mx = value; delaystart();
      }
    }
    public BigRational CenterY
    {
      get => my;
      set
      {
        if (value == my) return;
        stop(); my = value; delaystart();
      }
    }
    public long RenderTime
    {
      get => t2 - t1;
    }
    public EventHandler? PropChanged { get; set; }
    public EventHandler? StateChanged { get; set; }
    public void Start()
    {
      if (task != null) return;
      restart = true; start();
    }
    public void Stop()
    {
      stop();
    }
    public void Reset()
    {
      stop(); mx = -0.7; my = 0; scale = 1.2; imax = 32; lim = 64;
      delaystart();
    }
    public void Clear()
    {
      stop(); if (bmp != null) { bmp.Dispose(); bmp = null; Invalidate(); }
      t1 = t2 = 0; StateChanged?.Invoke(this, EventArgs.Empty);
    }

    Bitmap? bmp; uint* scan; int dx, dy, stride, driver = 2;
    Task? task; bool cancel, dostart, restart, manual; long t1, t2;
    System.Windows.Forms.Timer timer = new() { Interval = 100 };
    BigRational mx = -0.7, my = 0, scale = 1.2; int imax = 32, lim = 64;     
    void mandel_double()
    {
      calcmap(); if (map == null) return;
      var x1 = (double)(mx - scale * dx / dy);
      var y1 = (double)(my - scale);
      var fi = (double)(2 * scale / dy);
      t1 = t2 = Environment.TickCount;
      //for (int py = 0; py < dy; py++)
      Parallel.For(0, dy, (py, po) =>
      {
        if (cancel) { po.Break(); return; }
        var p = scan + py * (stride >> 2);
        var y = y1 + py * fi;
        for (int px = 0; px < dx && !cancel; px++)
        {
          var x = x1 + px * fi;
          int i = 0; var a = x; var b = y;
          for (; i < imax; i++)
          {
            var u = a * a - b * b + x;
            var v = 2 * a * b + y;
            if (u * u + v * v > 4) break;
            a = u; b = v;
          }
          p[px] = i < imax ? map[i] : 0;
        }
      }
      );
      t2 = Environment.TickCount;
    }
    void mandel_BigInteger()
    {
      calcmap(); if (map == null) return;
      var x1 = (OldRational)(mx - scale * dx / dy);
      var y1 = (OldRational)(my - scale);
      var fi = (OldRational)(2 * scale / dy); var qmax = (OldRational)4;
      t1 = t2 = Environment.TickCount; var digits = Math.Max(8, 10 * lim / 64); // actually about 15 * lim / 64
      try // as long as the BigInteger bugs in NET7 not fixed
      {
        //for (int py = 0; py < dy; py++)
        Parallel.For(0, dy, (py, po) =>
        {
          if (cancel) { po.Break(); return; }
          var p = scan + py * (stride >> 2);
          var y = y1 + py * fi;
          for (int px = 0; px < dx && !cancel; px++)
          {
            var x = x1 + px * fi;
            int i = 0; var a = x; var b = y;
            for (; i < imax; i++)
            {
              var u = a * a - b * b + x; u = OldRational.Round(u, digits);
              var v = 2 * a * b + y; v = OldRational.Round(v, digits);
              if (u * u + v * v > qmax) break;
              a = u; b = v;
            }
            p[px] = i < imax ? map[i] : 0;
          }
        });
      }
      catch (Exception ex)
      {
        Invoke(() =>
        {
          this.Controls.Add(new TextBox { Text = ex.InnerException?.ToString() ?? ex.Message, Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical });
          this.Enabled = true;
        });
      }
      t2 = Environment.TickCount;

    }
    void mandel_BigRat()
    {
      calcmap(); if (map == null) return;
      static BigRat conv(BigRational value) => (BigRat)(ReadOnlySpan<uint>)value;
      var x1 = conv(mx - scale * dx / dy);
      var y1 = conv(my - scale); var lim = this.lim;
      var fi = conv(2 * scale / dy); var qmax = (BigRat)4;
      t1 = t2 = Environment.TickCount; //var digits = Math.Max(8, 10 * lim / 64); // actually about 15 * lim / 64
      Parallel.For(0, dy, (py, po) =>
      {
        if (cancel) { po.Break(); return; }
        var p = scan + py * (stride >> 2);
        var y = BigRat.Normalize(y1 + py * fi);
        for (int px = 0; px < dx && !cancel; px++)
        {
          var x = BigRat.Normalize(x1 + px * fi);
          int i = 0; var a = x; var b = y;
          for (; i < imax; i++)
          {
            var u = BigRat.RoundB(a * a - b * b + x, lim);
            var v = BigRat.RoundB(a * b * 2 + y, lim);
            if (u * u + v * v > qmax) break; a = u; b = v;
          }
          p[px] = i < imax ? map[i] : 0;
        }
      });
      t2 = Environment.TickCount;
    }
    void mandel_BigRational()
    {
      calcmap(); if (map == null) return;
      var x1 = mx - scale * dx / dy;
      var y1 = my - scale;
      var fi = 2 * scale / dy; var lim = (uint)this.lim;
      t1 = t2 = Environment.TickCount;
      //for (int py = 0; py < dy; py++)
      Parallel.For(0, dy, (py, po) =>
      {
        if (cancel) { po.Break(); return; }
        var p = scan + py * (stride >> 2);
        var cpu = BigRational.task_cpu; var m = cpu.mark();
        //var y = y1 + py * f1;
        cpu.push(y1); cpu.push(fi); cpu.push(py); cpu.mul(); cpu.add(); cpu.norm();
        cpu.push(x1); //var x = xmin;
        for (int px = 0; px < dx && !cancel; px++)
        {
          //if (cancel) { po.Break(); break; }
          int i = 0; //var a = x; var b = y;
          cpu.dup(m + 1); //var a = x;
          cpu.dup(m + 0); //var b = y;
          for (; i < imax; i++)
          {
            // var u = a * a - b * b + x;
            cpu.sqr(m + 2); cpu.sqr(m + 3); cpu.sub(); cpu.add(m + 1); cpu.lim(lim);
            // var v = 2 * a * b + y;
            cpu.mul(m + 2, m + 3); cpu.shl(1); cpu.add(m + 0); cpu.lim(lim);
            // if (u * u + v * v > 4) break;
            cpu.sqr(m + 4); cpu.sqr(m + 5); cpu.add();
            //if (cpu.bdi() >= 2) { cpu.pop(3); break; }
            //if (cpu.bdi() > 1)
            if (cpu.cmpi(0, 4) > 0) { cpu.pop(3); break; }
            // a = u; b = v;
            cpu.swp(m + 2, m + 4); cpu.swp(m + 3, m + 5); cpu.pop(3);
          }
          cpu.pop(2); // a, b
          p[px] = i < imax ? map[i] : 0;
          cpu.add(fi); cpu.norm(); //x += fi;
        }
        cpu.pop(2);
      }
      );
      t2 = Environment.TickCount;
    }

    void start()
    {
      stop(); if (manual && !restart) return; restart = false;
      var size = ClientSize;
      if (bmp == null || bmp.Size != size)
      {
        bmp?.Dispose();
        bmp = new Bitmap(dx = size.Width, dy = size.Height, PixelFormat.Format32bppRgb);
        var lb = bmp.LockBits(new Rectangle(0, 0, dx, dy), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
        scan = (uint*)lb.Scan0.ToPointer(); stride = lb.Stride; bmp.UnlockBits(lb);
      }
      t1 = t2 = 0;
      task = new Task(driver == 0 ? mandel_double : driver == 1 ? mandel_BigInteger : driver == 2 ? mandel_BigRational : mandel_BigRat);
      cancel = dostart = false; task.Start(); timer.Start(); gcount(true); StateChanged?.Invoke(this, EventArgs.Empty);
    }

    internal long gcnews; long gclast;
    void gcount(bool on)
    {
      //return;
      if (on) { gclast = gcnews = 0; Application.Idle += gcounter; }
      else { gcounter(null, EventArgs.Empty); Application.Idle -= gcounter; }
      void gcounter(object? sender, EventArgs e)
      {
        var c = GC.GetTotalMemory(false); //var p = GC.GetGCMemoryInfo(); p.Index...
        if (c > gclast) gcnews += c - gclast; gclast = c;
      }
    }

    void delaystart()
    {
      if (bmp == null || manual) return;
      dostart = true; timer.Start();
    }
    void stop()
    {
      timer.Stop(); if (task == null) return;
      cancel = true; task.Wait(); task.Dispose(); task = null; gcount(false);
    }
    void update()
    {
      start(); Invalidate(); //task?.Wait(); Update();
    }

    void ontimer(object? sender, EventArgs e)
    {
      if (dostart) { dostart = false; start(); return; }
      if (!Visible) { stop(); bmp?.Dispose(); bmp = null; restart = true; return; }
      Invalidate();
      if (task != null && task.IsCompleted) { timer.Stop(); task = null; gcount(false); if (t1 == t2) t2 = t1 + 1; Update(); StateChanged?.Invoke(this, EventArgs.Empty); return; }
    }
    protected override void OnHandleCreated(EventArgs e)
    {
      timer.Tick += ontimer;
    }
    protected override void OnHandleDestroyed(EventArgs e)
    {
      stop();
      if (timer != null) { timer.Tick -= ontimer; timer.Dispose(); }
      if (bmp != null) { bmp.Dispose(); bmp = null; }
    }
    protected override void OnPaintBackground(PaintEventArgs pevent) { }
    protected override void OnPaint(PaintEventArgs e)
    {
      if (bmp == null || bmp.Size != ClientSize) start();
      if (bmp != null) e.Graphics.DrawImage(bmp, 0, 0); else e.Graphics.Clear(Color.Black);
    }
    protected override void OnSizeChanged(EventArgs e)
    {
      if (bmp == null) return;
      stop(); var p1 = s2r(new Point()); var p2 = s2r(new Point(dx, dy));
      var size = ClientSize;
      var tmp = new Bitmap(dx = size.Width, dy = size.Height, PixelFormat.Format32bppRgb);
      var s1 = r2s(p1); var s2 = r2s(p2);
      using (var g = Graphics.FromImage(tmp))
        g.DrawImage(bmp, s1.X, s1.Y, s2.X - s1.X, s2.Y - s1.Y);
      bmp.Dispose(); bmp = tmp;
      var lb = bmp.LockBits(new Rectangle(0, 0, dx, dy), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
      scan = (uint*)lb.Scan0.ToPointer(); stride = lb.Stride; bmp.UnlockBits(lb);
      Invalidate(); Update(); delaystart();
    }

    Action<int>? tool;
    Action<int> tool_move()
    {
      var p1 = Cursor.Position; var po = p1;
      var x1 = this.mx; var y1 = this.my;
      return id =>
      {
        if (id == 0)
        {
          stop();
          var p2 = Cursor.Position;
          mx = x1 - (2 * (p2.X - p1.X)) * scale / dy;
          my = y1 - (2 * (p2.Y - p1.Y)) * scale / dy;
          var l = rat.ILog10(scale);
          mx = rat.Round(mx, 4 - l);
          my = rat.Round(my, 4 - l);
          if (bmp != null)
            using (var g = Graphics.FromImage(bmp))
            {
              var sx = p2.X - po.X; var sy = p2.Y - po.Y;
              g.DrawImage(bmp, sx, sy);
              if (sx > 0) g.FillRectangle(Brushes.Black, 0, 0, sx, dy);
              if (sx < 0) g.FillRectangle(Brushes.Black, dx + sx, 0, -sx, dy);
              if (sy > 0) g.FillRectangle(Brushes.Black, 0, 0, dx, sy);
              if (sy < 0) g.FillRectangle(Brushes.Black, 0, dy + sy, dx, -sy);
              po = p2; //Invalidate(); Update();
            }
          Invalidate(); Update(); delaystart(); PropChanged?.Invoke(this, EventArgs.Empty);
        }
      };
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        Capture = true;
        tool = tool_move(); //ModifierKeys == Keys.Shift ? tool_imax() :
      }
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
      if (tool != null) tool(0); else base.OnMouseMove(e);
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
      if (tool != null) { tool(1); tool = null; Capture = false; }
      base.OnMouseUp(e);
    }
    protected override void OnMouseLeave(EventArgs e)
    {
      if (tool != null) { tool(2); tool = null; Capture = false; }
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
      stop();
      var p1 = s2r(new Point()); var p2 = s2r(new Point(dx, dy));
      var p = e.Location;
      var d = 1 - e.Delta * (0.1f / 120);
      var t = scale; scale *= d;
      var l = rat.ILog10(scale);
      scale = rat.Round(scale, 5 - l);
      t = (scale - t) / dy;
      mx += t * (dx - p.X * 2); mx = rat.Round(mx, 4 - l);
      my += t * (dy - p.Y * 2); my = rat.Round(my, 4 - l);
      if (bmp != null)
        using (var g = Graphics.FromImage(bmp))
        {
          var s1 = r2s(p1); var s2 = r2s(p2);
          g.DrawImage(bmp, s1.X, s1.Y, s2.X - s1.X, s2.Y - s1.Y);
          if (s1.X > 0) g.FillRectangle(Brushes.Black, 0, 0, s1.X, dy);
          if (s2.X < dx) g.FillRectangle(Brushes.Black, s2.X, 0, dx - s2.X, dy);
          if (s1.Y > 0) g.FillRectangle(Brushes.Black, 0, 0, dx, s1.Y);
          if (s2.Y < dy) g.FillRectangle(Brushes.Black, 0, s2.Y, dx, dy - s2.Y);
        }
      Invalidate(); Update(); PropChanged?.Invoke(this, EventArgs.Empty);
      delaystart();
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e); //if (e.KeyCode == Keys.Space) update();
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
    Vector2R s2r(Point p)
    {
      var x1 = mx + scale * (2 * p.X - dx) / dy;
      var y1 = my + scale * (2 * p.Y - dy) / dy;
      return new Vector2R(x1, y1);
    }
    Point r2s(Vector2R p)
    {
      var x = ((int)((p.X - mx) * dy / scale) + dx) / 2;
      var y = ((int)((p.Y - my) * dy / scale) + dy) / 2;
      return new Point(x, y);
    }
  }

}
