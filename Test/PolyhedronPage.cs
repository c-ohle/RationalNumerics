#pragma warning disable CS8602
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Globalization;
using System.Xml.Linq;
using Group = Test.DX11ModelCtrl.Group;
using Models = Test.DX11ModelCtrl.Models;

namespace Test
{
  public partial class PolyhedronPage : UserControl
  {
    public PolyhedronPage()
    {
      InitializeComponent();
    }
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      MenuItem.CmdRoot = OnCommand;
      //this.path = Path.GetFullPath("templ\\testcase2.xxd");
      var path = Path.GetFullPath("templ\\testcase2.xxd");
      modelView.Scene = (Models.Scene)Models.Load(XElement.Load(path));
      propsView.Target = modelView;
      //modelView.Infos.Add("Hello World!");
      panelStory.VisibleChanged += (_, _) =>
      {
        if (panelStory.Visible)
        {
          if (timeLineView == null)
          {
            timeLineView = new TimeLineView(modelView) { Dock = DockStyle.Fill };
            panelStory.Controls.Add(timeLineView); timeLineView.BringToFront();
          }
          modelView.Animations += maintick;
        }
        else
        {
          modelView.Animations -= maintick;
        }
      };
      btn_run_Click(null, null);
    }
    TimeLineView? timeLineView; string? path; int tv, showtime;

    int OnCommand(int id, object? test)
    {
      try
      {
        var x = modelView.OnCmd(id, test);
        if (x != 0) return x;
        switch (id)
        {
          case 1000: return OnNew(test);
          case 1001: return OnOpen(test);
          case 1002: return OnSave(test, false); // Save
          case 1003: return OnSave(test, true); // SaveAs          
          case 2008: return OnProperties(test);
          //case 2009: return OnToolbox(test);
          case 2010: return OnStoryBoard(test);
          case 2054: return OnCheckMash(test);
        }
      }
      catch (Exception e)
      {
        if (test == null) MessageBox.Show(e.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        else Debug.WriteLine(e.Message);
      }
      return 0;
    }
    void Open(string path)
    {
      var doc = XElement.Load(path);
      var scene = (Models.Scene)Models.Load(doc); //var last = view.Scene != null; if (last && !AskSave()) return; 
      //timeLineView?.Refresh();
      modelView.Scene = scene; this.path = path; // UpdateTitle(); if (last && path != null) mru(path, path);
      if (timeLineView != null) { timeLineView.adjust(); timeLineView.Invalidate(); }
    }
    int OnNew(object? test)
    {
      if (test != null) return 1;
      timeLineView?.Refresh(); this.path = null;
      modelView.Scene = new Models.Scene
      {
        Unit = Models.Scene.Units.Meter,
        Ambient = Color.FromArgb(0x00404040),
        Shadows = true,
        nodes = new Group[] {
        new Models.Camera {
          Name = "Camera", Fov = 30, Near = 0.1f, Far = 1000,
          Transform = !(Matrix4x3)Matrix4x4.CreateLookAt(new Vector3(0, -5, 5), new Vector3(), new Vector3(0, 0, 1)),
        },
        new Models.Light {
          Name = "Light",
          Transform = !(Matrix4x3)Matrix4x4.CreateLookAt(new Vector3(), new Vector3(-1, -1.5f, 3), new Vector3(0, 0, 1)),
        },
        new Models.BoxGeometry {
          Name = "Ground", Fixed = true,
          Transform = Matrix4x3.CreateTranslation(0, 0, 0),
          p1 = new Vector3(-10, -10, -0.1f), p2 = new Vector3(+10, +10, 0),
          ranges = new (int, Models.Material)[] { (0, new Models.Material {
            Diffuse = (uint)Color.LightGray.ToArgb(),
            //Texture = DX11ModelCtrl.GetTexture("https://c-ohle.github.io/RationalNumerics/web/tex/millis.png"),
          }) },
        } }
      };
      return 1;
    }
    int OnOpen(object? test)
    {
      if (test != null) return 1;
      var dlg = new OpenFileDialog() { Filter = "xxd files|*.xxd;*.xxd.png" };
      if (dlg.ShowDialog(this) != DialogResult.OK) return 1;
      Open(dlg.FileName); return 1;
    }
    int OnSave(object? test, bool saveas)
    {
      var scene = modelView.Scene; if (scene == null) return 0;
      if (test != null) return 1;
      var path = this.path;
      if (saveas || path == null)
      {
        var dlg = new SaveFileDialog() { Filter = "xxd files|*.xxd;*.xxd.png", DefaultExt = "xxd", FileName = path };
        if (dlg.ShowDialog(this) != DialogResult.OK) return 1; path = dlg.FileName;
      }
      if (path == null) return 0; Cursor.Current = Cursors.WaitCursor;
      try { var e = Models.Save(scene); e.Save(path); modelView.IsModified = false; this.path = path; Thread.Sleep(100); }
      finally { Cursor.Current = Cursors.Default; }
      return 1;
    }
    int OnProperties(object? test)
    {
      if (test != null) return 1;
      if (propsView.Visible) { if (!propsView.btnprops.Checked) { propsView.btnprops.PerformClick(); return 1; } }
      else
      {
        var c = propsView.IsHandleCreated; propsView.Visible = true;
        if (!c) propsView.btnstory.Click += (_, _) => panelStory.Visible = (propsView.btnstory.Checked ^= true);
      }
      return 1;
    }
    int OnStoryBoard(object? test)
    {
      if (test != null) return 1;
      var x = propsView.IsHandleCreated; if (!x) OnProperties(null);
      propsView.btnstory.PerformClick(); if (!x) OnProperties(null); return 1;
    }
    int OnCheckMash(object? test)
    {
      if (modelView.selection.Count != 1 || modelView.selection[0] is not Models.Geometry geo) return 0;
      if (test != null) return 1;
      if (!propsView.Visible || !propsView.btnprops.Checked) OnProperties(null);
      modelView.extrasel = new(modelView, new MeshInfo(geo)); modelView.Invalidate();
      return 1;
    }
    class MeshInfo
    {
      internal MeshInfo(Models.Geometry geo) => this.geo = geo; readonly Models.Geometry geo;
      public override string ToString() => "Mesh Info";
      [Category("\t\tGeneral")]
      public string Name => geo.Name;
      [Category("\t\tGeneral")]//, Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
      public string Status { get => "ok"; } //set { } }
      [Category("Mesh")]
      public int Vertices => geo.vertices.Length;
      [Category("Mesh")]
      public int Polygones => geo.indices.Length / 3;
      [Category("Mesh")]
      public int Edges
      {
        get { calc(); return edges; }
      }
      [Category("Mesh")]
      public int Euler => Vertices - Edges + Polygones;
      [Category("Polyhedron")]
      public int Planes
      {
        get { calc(); return ee.Length; }
      }
      [Category("Polyhedron")]
      public int Edges_
      {
        get { calc(); return hedges; }
      }
      Vector3R[]? pp; (PlaneR e, int[] kk)[]? ee; int edges, hedges;
      void calc()
      {
        if (this.pp != null) return;
        var xp = geo.GetVertices();
        this.pp = xp as Vector3R[] ?? ((Vector3[])xp).Select(p => (Vector3R)p).ToArray();
        var ii = geo.indices;
        this.ee = Enumerable.Range(0, ii.Length / 3).
          Where(i => Math.Max(Math.Max(ii[i * 3], ii[i * 3 + 1]), ii[i * 3 + 2]) < pp.Length).
          Select(i => (k: i *= 3, e: PlaneR.FromVertices(pp[ii[i]], pp[ii[i + 1]], pp[ii[i + 2]]))).
          GroupBy(p => p.e, p => p.k).Select(p => (e: p.Key, kk: p.ToArray())).ToArray();
        this.hedges = this.ee.Sum(e =>
        {
          var tt = e.kk.SelectMany(k => Enumerable.Range(k, 3)).Select(i => (a: ii[i], b: ii[i % 3 != 2 ? i + 1 : i - 2])).ToArray();
          return tt.Count(p => !tt.Contains((p.b, p.a)));
        }) >> 1;
        var eb = Enumerable.Range(0, ii.Length).Select(i => (a: ii[i], b: ii[i % 3 != 2 ? i + 1 : i - 2])).ToHashSet();
        this.edges = eb.Count(p => p.a < p.b && eb.Contains((p.b, p.a)));
      }
    }

    void btn_close_Click(object sender, EventArgs e)
    {
      OnStoryBoard(null);
    }
    void panel_MouseDown(object sender, MouseEventArgs e)
    {
      var p = (Control)sender; p.Capture = true;
      tv = p.Dock == DockStyle.Bottom ? p.Height + Cursor.Position.Y : p.Width + Cursor.Position.X;
      p.Cursor = p.Dock == DockStyle.Bottom ? Cursors.SizeNS : Cursors.SizeWE;
    }
    void panel_MouseMove(object sender, MouseEventArgs e)
    {
      var p = (Control)sender;
      if (!p.Capture) Cursor.Current = p.Dock == DockStyle.Bottom ? Cursors.SizeNS : Cursors.SizeWE;
      else
      {
        if (p.Dock == DockStyle.Bottom)
          p.Height = Math.Max(p.Padding.Top, Math.Min(p.Parent.ClientSize.Height, tv - Cursor.Position.Y));
        else
          p.Width = Math.Max(p.Padding.Left, Math.Min(p.Parent.ClientSize.Width, tv - Cursor.Position.X));
        p.Parent.Update();
      }
    }
    void panel_MouseUp(object sender, MouseEventArgs e)
    {
      var p = (Control)sender; p.Capture = false;
      p.Cursor = Cursors.Default;
    }
    void panel_MouseLeave(object sender, EventArgs e)
    {
      ((Control)sender).Cursor = Cursors.Default;
    }

    void btn_run_Click(object? sender, EventArgs? e)
    {
      //if (sender == null) return;
      var aniset = modelView.Scene.aniset; if (aniset == null) return;
      //if (modelView.RunningAnimation != null) return;
      modelView.RunningAnimation = null;
      aniset.ani(0); modelView.RunningAnimation = aniset;
    }

    void btn_back_Click(object sender, EventArgs e) { timeLineView.ani(0); timeLineView.scrtime(); }
    void btn_back__Click(object sender, EventArgs e) { }
    void btn_forw__Click(object sender, EventArgs e)
    {
      var scene = timeLineView.view.Scene; if (scene == null) return;
      var t1 = DX11ModelCtrl.Models.Save(scene);
      var t2 = t1.ToString(); //File.WriteAllText(@"C:\Users\cohle\Desktop\rec.xml", t2);
      var t3 = DX11ModelCtrl.Models.Load(XElement.Parse(t2.ToString()));
      var t4 = DX11ModelCtrl.Models.Save(t3);
    }
    void btn_forw_Click(object sender, EventArgs e) { timeLineView.ani(timeLineView.aniset.getendtime()); timeLineView.scrtime(); }
    void btn_record_Click(object sender, EventArgs e) => timeLineView.record();

    void btn_play_Click(object sender, EventArgs e)
    {
      var aniset = timeLineView.aniset;
      modelView.RunningAnimation = !btn_play.Checked ? aniset : null;
    }
    void maintick()
    {
      var t = timeLineView.view.RunningAnimation; //if (t != timeLineView.aniset) return;
      if (t == timeLineView.aniset && showtime != t.time)
      {
        showtime = t.time; timeLineView.Invalidate(); timeLineView.scrtime();
      }
      if (btn_play.Checked != (modelView.RunningAnimation == timeLineView.aniset))
        btn_play.Text = (btn_play.Checked ^= true) ? "" : "";
      btn_record.Enabled = timeLineView.recundo() != null;
    }

    class TimeLineView : UserControl
    {
      internal TimeLineView(DX11ModelCtrl view)
      {
        DoubleBuffered = true; AutoScroll = true;
        this.view = view; this.aniset = adjust();
      }
      internal readonly DX11ModelCtrl view; DX11ModelCtrl.Undo? lastu;
      internal DX11ModelCtrl.AniSet aniset;
      const int leftofs = 3, dyline = 17;
      float xscale = 0.1f; int wo, st, pcx, ctrl;
      readonly List<int> selection = new();
      internal DX11ModelCtrl.AniSet adjust()
      {
        if (this.aniset == null || this.aniset != view.Scene.aniset)
        {
          this.aniset = view.Scene.aniset ??= new(); this.lastu = null;
          selection.Clear();
        }
        var aniset = this.aniset; if (aniset.maxtime == 0) aniset.maxtime = aniset.getendtime();
        AutoScrollMinSize = new Size(64 + (int)(aniset.maxtime * xscale), 16 + aniset.lines.Count * 16);
        if (aniset.time > aniset.maxtime) { aniset.time = aniset.maxtime; Invalidate(); }
        return this.aniset;
      }
      internal void ani(int t)
      {
        var aniset = this.aniset; if (aniset.time == t) return;
        if (aniset.ani(t & ~0x40000000)) view.Invalidate(); Invalidate();
      }
      protected override void OnPaint(PaintEventArgs e)
      {
        var g = e.Graphics; var s = ClientSize; var o = AutoScrollPosition;
        g.TranslateTransform(leftofs + o.X, o.Y); var aniset = this.aniset; var list = aniset.lines;
        var xe = (int)(aniset.maxtime * xscale); g.DrawLine(Pens.LightGray, xe, 0, xe, s.Height - o.Y);
        for (int i = 0, y = 0; i < list.Count; i++, y += dyline)
        {
          var l = list[i]; var tt = l.times; var se = selection.Contains(0x20000000 | (i << 16));
          if (se) g.FillRectangle(SystemBrushes.GradientInactiveCaption, 0, y, s.Width - o.X, dyline - 1);
          for (int k = 0; k < tt.Count; k += 2)
          {
            var t = tt[k]; var dt = tt[k + 1]; se = selection.Contains(0x40000000 | (i << 16) | k);
            var u = (int)(t * xscale); var v = (int)((t + dt) * xscale) - u;
            var r = new Rectangle(u, y, v, dyline - 1);
            if (se) g.FillRectangle(SystemBrushes.GradientInactiveCaption, r);
            //if (l.disp(1, 1) is string ic)
            //{
            //  //TextRenderer.DrawText(g, "*", Font, r, Color.Gray, 
            //  //  TextFormatFlags.HorizontalCenter| TextFormatFlags.VerticalCenter| 
            //  //  TextFormatFlags.PreserveGraphicsTranslateTransform);
            //  g.DrawString(ic, Font, Brushes.Gray, r.X +3, r.Y+3);// new RectangleF(r.X, r.Y, r.Width, r.Height));
            //}
            g.DrawRectangle(Pens.Black, r);
          }
          g.DrawLine(Pens.LightGray, 0, y + dyline, s.Width - o.X, y + dyline);
        }
        var x = (int)(aniset.time * xscale); g.DrawLine(Pens.Red, x, 0, x, s.Height - o.Y);
      }

      internal DX11ModelCtrl.Undo? recundo()
      {
        var p = view.undoi != 0 && view.undoi == view.undos.Count ? view.undos[view.undoi - 1] : null;
        return p != lastu ? p : null;
      }
      internal void record()
      {
        var undo = recundo(); if (undo == null) return;
        var test = undo.record(aniset, -1); //if (test == default) return;
        var times = test.line?.times; int dt = 250, ddt, xxx = 0, split = -1;
        var t = aniset.time;
        if (times != null && t != aniset.maxtime)
        {
          if (t >= dt) t -= dt;
          for (int i = 0; i < times.Count; i += 2)
          {
            var t1 = times[i]; if (t1 >= t + dt) continue;
            var t2 = t1 + times[i + 1]; if (t2 <= t) continue;
            if (i == times.Count - 2 && t2 <= aniset.time) { t = aniset.time; break; } // add last
            if ((ddt = aniset.time - t1) < 10 || (dt = t2 - aniset.time) < 10) return;
            split = i; t = aniset.time; xxx = times[split + 1]; times[split + 1] = ddt; break;
          }
        }
        var w = undo.record(aniset, t); if (w.line == null) { times[split + 1] = xxx; return; }
        if (split != -1) { w.line.disp(5, ((split >> 1) + 1)); times[split + 3] = dt; dt = 0; }
        if (t + dt != aniset.time) { aniset.maxtime = 0; aniset.ani(t + dt); }
        adjust(); select(w.wo); lastu = undo; scrvis(w.wo);
      }

      Point p2s(Point p)
      {
        var o = AutoScrollPosition;
        return new Point((int)(p.X * xscale) + (o.X + leftofs), p.Y + o.Y);
      }
      Point s2p(Point p)
      {
        var o = AutoScrollPosition;
        return new Point((int)((p.X - (o.X + leftofs)) / xscale), p.Y - (o.Y));
      }
      Rectangle r2s(int w)
      {
        var y = (w >> 16) & 0x1fff; var x = w & 0xfffe;
        var times = aniset.lines[y].times;
        var s = p2s(new Point(times[x], y * dyline));
        return new Rectangle(s.X, s.Y, (int)(times[x + 1] * xscale), dyline);
      }
      void select(int wo)
      {
        if (wo == 1) return; wo &= ~1;
        if ((ctrl & 2) != 0 && wo != 0) { if ((ctrl & 1) == 0) selection.Add(wo); else selection.Remove(wo); }
        else
        {
          var c = selection.Count; if ((c == 0 && wo == 0) || (c == 1 && selection[0] == wo)) return;
          selection.Clear(); if (wo != 0) selection.Add(wo);
        }
        Invalidate();
        if (selection.Count > 1)
        {
          var a = selection.Select(w => (w & 0x40000000) != 0 ? new AniRec(this, w) : null).OfType<object>().ToArray();
          if (a.Length > 0) view.extrasel = (this, a);
          else view.extrasel = default;
          view.Invalidate(); return;
        }
        var w = selection.Count == 1 ? selection[0] : 0;
        if ((w & 0x40000000) != 0) view.extrasel = (this, new AniRec(this, selection[0]));
        //else if ((w & 0x20000000) != 0) view.extrasel = (this, new AniLin(this, selection[0]));
        //else view.extrasel = (this, new AniSet(this));
        else view.extrasel = default;
        view.Invalidate();
      }
      void scrvis(int wo)
      {
        if (wo == 0) wo = selection[0];
        var r = r2s(wo); r.Inflate(32, 16); // r.Width += 32; r.Height += 16;
        var o = AutoScrollPosition; var q = new Point(-o.X, -o.Y); var rs = ClientRectangle;
        if ((wo & 0x40000000) == 0) r.X = r.Width = 0;
        if (r.X < 0) q.X += r.X; else if (r.Right > rs.Right) q.X += r.Right - rs.Right;
        if (r.Y < 0) q.Y += r.Y; else if (r.Bottom > rs.Bottom) q.Y += r.Bottom - rs.Bottom;
        if (o.X != -q.X || o.Y != -q.Y) AutoScrollPosition = q;
      }
      internal void scrtime()
      {
        var a = AutoScrollPosition;
        int x = (int)(aniset.time * xscale) - leftofs + a.X;
        int r = ClientSize.Width - (this.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0);
        if (x < 0 || x > r)
        {
          var b = a.X; if (x < 0) a.X -= x; else a.X -= (x - r);
          AutoScrollPosition = new Point(-a.X, -a.Y); pcx += AutoScrollPosition.X - b;
        }
      }

      protected override void OnMouseDown(MouseEventArgs e)
      {
        Focus(); if (wo == 0) select(wo);
        if (!((wo == 1) || (wo & 0x60000000) != 0)) return;
        pcx = e.X; st = -1;
        ctrl = (selection.Contains(wo & ~1) ? 1 : 0) | ((ModifierKeys == Keys.Control) ? 2 : 0);
        if ((ctrl & 2) == 0) select(wo); Capture = true;
      }
      protected override void OnMouseMove(MouseEventArgs e)
      {
        var aniset = this.aniset;
        if (!Capture)
        {
          var o = AutoScrollPosition; var x = o.X + leftofs + (int)(aniset.time * xscale);
          wo = Math.Abs(e.X - x) < 4 ? 1 : 0;
          if (wo == 0)
          {
            var i = (e.Y - o.Y) / dyline; var tx = (e.X - leftofs - o.X) / xscale;
            if (i >= 0 && i < aniset.lines.Count)
            {
              var a = aniset.lines[i].times; wo = 0x20000000 | (i << 16);
              for (int k = a.Count - 2; k >= 0; k -= 2)
              {
                var t = a[k]; var dt = a[k + 1]; var w = 0x40000000 | (i << 16) | k;
                if (Math.Abs(tx - (t + dt)) * xscale < 4) { wo = w | 1; break; }
                if (tx >= t && tx <= t + dt) { wo = w; break; }
              }
            }
          }
          this.Cursor = wo == 1 ? Cursors.VSplit : (wo & 0x40000001) == 0x40000001 ? Cursors.SizeWE : Cursors.Default; return;
        }
        if (wo == 0) return; var fdt = (int)((e.X - pcx) / xscale);
        if (wo == 1) //time
        {
          if (st == -1) st = aniset.time;
          var u = Math.Max(0, Math.Min(aniset.maxtime, st + fdt)); if (u == aniset.time) return;
          ani(u); scrtime(); //Target.Invalidate(); //props          
        }
        else if ((wo & 0x40000000) != 0)
        {
          int x = wo & 0xffff, y = (wo >> 16) & 0x1fff;
          var tt = aniset.lines[y].times;
          if (st == -1) { st = tt[x]; if ((ctrl & 2) != 0) { ctrl ^= 2; select(wo); } }
          var u = st + fdt;
          if ((wo & 1) == 0)
          {
            u = Math.Max(u, x == 0 ? 0 : tt[x - 2] + tt[x - 1]);
            if (x + 2 < tt.Count) u = Math.Min(u, tt[x + 2] - tt[x + 1]);
          }
          else
          {
            u = Math.Max(20, u);
            if (x + 1 < tt.Count) u = Math.Min(u, tt[x + 1] - tt[x - 1]);
          }
          if (tt[x] == u) return; // if (et != aniset.maxtime) aniset.maxtime = et;
          tt[x] = u; aniset.maxtime = aniset.getendtime();
          ani(aniset.time | 0x40000000); view.Invalidate(); //props
        }
      }
      protected override void OnMouseUp(MouseEventArgs e)
      {
        Capture = false; if ((wo & 0x60000000) == 0) return;
        if ((ctrl & 2) != 0) select(wo); adjust();
      }
      protected override void OnMouseWheel(MouseEventArgs e) //todo:
      {
        var p = e.Location; var v = s2p(p);
        var d = Math.Max(0.01f, Math.Min(10, xscale * (1 + e.Delta * (0.1f / 120))));
        if (xscale == d) return; xscale = d; Invalidate(); adjust();
        var o = AutoScrollPosition; var s = p2s(v);
        AutoScrollPosition = new Point(-(o.X - (s.X - p.X)), -(o.Y - (s.Y - p.Y)));
      }
      protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
      {
        if (selection.Count != 1) return;
        var wo = selection[0]; int y = (wo >> 16) & 0x1fff, x = wo & 0xffff;
        var lines = this.aniset.lines; var line = lines[y];
        if ((e.KeyCode == Keys.Left || e.KeyCode == Keys.Right) && (wo & 0x40000000) != 0)
        {
          var u = e.KeyCode == Keys.Left ? x - 2 : x + 2;
          if (u >= 0 && u < line.times.Count)
          {
            select(0x40000000 | (y << 16) | u); scrvis(0); return;
          }
        }
        if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) && (wo & 0x20000000) != 0)
        {
          var v = e.KeyCode == Keys.Up ? y - 1 : y + 1;
          if (v >= 0 && v < lines.Count)
          {
            if (e.Shift)
            {
              var t = lines[y]; lines[y] = lines[v]; lines[v] = t;
              this.ani(aniset.time | 0x40000000);
            }
            select(0x20000000 | (v << 16)); scrvis(0); return;
          }
        }
        if (e.KeyCode == Keys.Delete && (wo & 0x40000000) != 0)
        {
          select(0);
          if (line.times.Count > 2)
          {
            line.disp(8, x);
            select(0x40000000 | (y << 16) | Math.Min(x, line.times.Count - 2)); scrvis(0);
          }
          else lines.RemoveAt(y);
          aniset.maxtime = 0; adjust();
          ani(aniset.time | 0x40000000);
        }
      }

      public class AniSet
      {
        public override string ToString() => "Animation Set";
        internal AniSet(TimeLineView p) => this.view = p;
        readonly TimeLineView view;
        [Category("Set")]
        public string? Name
        {
          get => view.aniset.name ?? String.Empty;
          set => view.aniset.name = value != null && (value = value.Trim()).Length != 0 ? value : null;
        }
        [Category("Set")]
        public int Time { get => view.aniset.time; set { } }
        [Category("Set")]
        public int MaxTime => view.aniset.maxtime;
      }
      public class AniLin
      {
        public override string ToString() => "Animation Line";
        internal AniLin(TimeLineView p, int wo)
        {
          this.view = p;
          this.line = p.aniset.lines[(wo >> 16) & 0x1fff];
        }
        readonly TimeLineView view; readonly DX11ModelCtrl.AniLine line;
        [Category("Line")]
        public string Target
        {
          get { var t = (DX11ModelCtrl.Node?)line.disp(0); return t.name ?? t.GetType().Name; }
        }
        [Category("Line")]
        public string? Type
        {
          get => line.GetType().Name;
        }
        [Category("Line")]
        public string? Prop
        {
          get => line.disp(1) as string;
        }
      }
      public class AniRec
      {
        public override string ToString() => "Animation Record";
        internal AniRec(TimeLineView p, int wo)
        {
          this.view = p; this.line = p.aniset.lines[(wo >> 16) & 0x1fff];
          this.x = wo & 0xffff;
        }
        readonly TimeLineView view; readonly DX11ModelCtrl.AniLine line; int x;
        [Category("\tSet")]
        public string? Name
        {
          get => view.aniset.name ?? String.Empty;
          set => view.aniset.name = value != null && (value = value.Trim()).Length != 0 ? value : null;
        }
        [Category("\tSet")]
        public int AniTime
        {
          get => view.aniset.time;
          set { }
        }
        [Category("\tSet")]
        public int MaxTime => view.aniset.maxtime;
        [Category("Line")]
        public string Target
        {
          get { var t = (DX11ModelCtrl.Node?)line.disp(0); return t.name ?? t.GetType().Name; }
        }
        [Category("Line")]
        public string? Type
        {
          get => line.GetType().Name;
        }
        [Category("Line")]
        public string? Prop
        {
          get => line.disp(1) as string;
        }
        [Category("Record")]
        public int Time
        {
          get { return x < line.times.Count ? line.times[x] : -1; }
          set { }
        }
        [Category("Record")]
        public int Delta
        {
          get { return x < line.times.Count ? line.times[x | 1] : -1; }
          set { }
        }
        [Category("Record"), DefaultValue(0f)]
        public float Gamma
        {
          get { return x < line.times.Count && line.disp(9, x >> 1) is float g ? g : 0; }
          set { line.disp(9, x >> 1, value); }
        }
        //[Category("Record"), DefaultValue(false)]
        //public bool LongWay
        //{
        //  get { return x < line.times.Count ? line.getgamma(1, x >> 1) != 0 : false; }
        //  set { line.setgamma(1, x >> 1, value ? 1 : 0); }
        //}
      }
    }
  }

  unsafe struct Quat //todo: sort in
  {
    public Vector3 t; Quaternion q; public Vector3 s; // keep order!
    public static implicit operator Quat(Matrix4x3 m) //todo: SSE impl 
    {
      Matrix4x4.Decompose(m, out var s, out var q, out var t);
      s.X = MathF.Round(s.X, 6); s.Y = MathF.Round(s.Y, 6); s.Z = MathF.Round(s.Z, 6);
      return new Quat { q = q, s = s, t = t };
    }
    public static implicit operator Matrix4x3(in Quat q) //todo: SSE impl 
    {
      return (Matrix4x3)( //Matrix4x4.CreateScale(q.s)) *
        Matrix4x4.CreateFromQuaternion(q.q) *
        Matrix4x4.CreateTranslation(q.t));
    }
    public static Matrix4x3 Slerp(in Quat a, in Quat b, float f) //todo: SSE impl 
    {
      return (Matrix4x3)( //Matrix4x4.CreateScale(Vector3.Lerp(a.s, b.s, f)) *
        Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(a.q, b.q, f)) *
        Matrix4x4.CreateTranslation(Vector3.Lerp(a.t, b.t, f))
      );
    }
    public int Format(Span<char> w, NumberFormatInfo f)
    {
      var m = this; var a = w.Length;
      var n = m.s != Vector3.One ? 10 : m.q != Quaternion.Identity ? 7 : 3;
      for (int i = 0; i < n; i++)
      {
        if (i != 0) { w[0] = ' '; w = w.Slice(1); }
        ((float*)&m)[i].TryFormat(w, out var c, default, f); w = w.Slice(c);
      }
      return a - w.Length;
    }
    public static Quat Parse(ReadOnlySpan<char> sp, NumberFormatInfo fi)
    {
      var m = default(Quat);
      int n = SpanTools.param_count(sp);
      for (int i = 0; i < n; i++)
      {
        var s = SpanTools.param_slice(ref sp);
        var f = float.Parse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, fi);
        ((float*)&m)[i] = f;
      }
      if (n < 10) { m.s = Vector3.One; if (n < 7) m.q = Quaternion.Identity; }
      return m;
    }
    /*
      internal static bool alt;
      static Quaternion SlerpN(Quaternion a, Quaternion b, float t)
      {
        var d = Quaternion.Dot(a, b);
        if (d < 0.0f) { d = -d; b = -b; }
        var at = MathF.Acos(d);
        var st = MathF.Sin(at);
        var af = MathF.Sin((1.0f - t) * at) / st;
        var bf = MathF.Sin(t * at) / st;
        return a * af + b * bf;
      }
      static Quaternion SlerpF(Quaternion a, Quaternion b, float t)
      {
        var d = Quaternion.Dot(a, b);
        if (d > 0.0f) { d = -d; b = -b; }
        var at = MathF.Acos(d);
        var st = MathF.Sin(at);
        var af = MathF.Sin((1.0f - t) * at) / st;
        var bf = MathF.Sin(t * at) / st;
        return a * af + b * bf;
      }
      */
  }

  public class MenuItem : ToolStripMenuItem
  {
    public MenuItem() { }
    public MenuItem(string text, params ToolStripItem[] items)
    {
      Text = text; DropDownItems.AddRange(items);
    }
    public MenuItem(int id, string text, Keys keys = Keys.None)
    {
      this.Id = id; Text = text; ShortcutKeys = keys;
    }
    public int Id { get; set; }
    protected override void OnDropDownShow(EventArgs e)
    {
      base.OnDropDownShow(e);
      Update(DropDownItems);
    }
    public static void Update(ToolStripItemCollection items)
    {
      if (CmdRoot == null) return;
      for (int i = 0; i < items.Count; i++)
      {
        var mi = items[i] as MenuItem; if (mi == null || mi.Id == 0) continue;
        var hr = CmdRoot(mi.Id, mi); mi.Enabled = (hr & 1) != 0; mi.Checked = (hr & 2) != 0;
        if (!mi.HasDropDownItems) continue; mi.Visible = false;
        foreach (var e in mi.DropDownItems.OfType<ToolStripMenuItem>())
          items.Insert(++i, new MenuItem(mi.Id, e.Text) { Tag = e.Tag, Checked = e.Checked });
        mi.DropDownItems.Clear();
      }
    }
    protected override void OnDropDownClosed(EventArgs e)
    {
      base.OnDropDownClosed(e);
      onclose(DropDownItems);
    }
    protected override void OnClick(EventArgs e)
    {
      base.OnClick(e);
      if (CmdRoot == null) return;
      if (Id != 0) CmdRoot(Id, Tag);
    }
    internal static Func<int, object, int>? CmdRoot;
    internal static void onclose(ToolStripItemCollection items)
    {
      if (CmdRoot == null) return;
      for (int i = 0, k = -1; i < items.Count; i++)
      {
        var p = items[i]; p.Enabled = true;
        if (p.Tag != null) { items.RemoveAt(i--); if (k != i) items[k = i].Visible = true; }
      }
    }
  }
  public class ContextMenu : ContextMenuStrip
  {
    public ContextMenu() { }
    public ContextMenu(IContainer container) : base(container) { }
    protected override void OnOpening(CancelEventArgs e) { MenuItem.Update(Items); }
    protected override void OnClosed(ToolStripDropDownClosedEventArgs? e)
    {
      MenuItem.onclose(Items);
    }
  }
  [DesignerCategory("Code")]
  public class DX11PropsCtrl : DX11ModelCtrl.PropsCtrl
  {
  }
}
