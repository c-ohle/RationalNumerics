#pragma warning disable CS8602
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;
using Models = Test.DX11ModelCtrl.Models;

namespace Test
{
  public partial class PolyhedronPage : UserControl
  {
    Models.Scene demo1()
    {
      return new Models.Scene
      {
        Unit = Models.Scene.Units.Meter,
        Ambient = Color.FromArgb(0x00404040),
        Shadows = true,
        Nodes = new Models.Node[] {
            new Models.Camera {
              Name = "Camera1", Fov = 30, Near = 0.1f, Far = 1000,
              Transform = !(Matrix4x3)Matrix4x4.CreateLookAt(
                //new Vector3(0, -7, 2), new Vector3(), new Vector3(0, 0, 1)),
                new Vector3(0, -5, 5), new Vector3(), new Vector3(0, 0, 1)),
            },
            new Models.Light {
              Name = "Light1",
              Transform = !(Matrix4x3)Matrix4x4.CreateLookAt(
                new Vector3(), new Vector3(-1, -1.5f, 3), new Vector3(0, 0, 1)),
            },
            new Models.BoxGeometry {
              Name = "Ground", Fixed = true,
              Transform = Matrix4x3.CreateTranslation(0, 0, 0),
              p1 = new Vector3(-10, -10, -0.1f), p2 = new Vector3(+10, +10, 0),
              ranges = new (int, Models.Material)[] { (0, new Models.Material {
                Diffuse = (uint)Color.LightGray.ToArgb(),
                Texture = DX11ModelCtrl.GetTexture("https://c-ohle.github.io/RationalNumerics/web/tex/millis.png"),
              }) },
            },
            //new Models.BoxGeometry {
            //  Name="Box1",
            //  Transform = Matrix4x3.CreateTranslation(0, 0, 0),
            //  p2 = new Vector3(1, 1, 1),
            //  ranges = new (int, Models.Material)[] { (0, new Models.Material {
            //    Diffuse = (uint)Color.Gold.ToArgb(), /*Texture = tex2*/ } ) },
            //},
            extdemo(),
          }
      };
      static Models.ExtrusionGeometry extdemo()
      {
        var pp = new List<Vector2>(); var cc = new List<ushort>();
        TesselatorPage.demo1((pp, cc));
        for (int i = 0; i < pp.Count; i++) pp[i] = (pp[i] - new Vector2(350, 200)) * (-1.0f / 100);
        var model = new Models.ExtrusionGeometry
        {
          Name = "Extrusion1",
          Transform = Matrix4x3.Identity,
          points = pp.ToArray(),
          counts = cc.ToArray(),
          Height = 0.2f,
          ranges = new (int, Models.Material)[] { (0, new Models.Material {
          Diffuse = (uint)Color.Gold.ToArgb(), }) }
        };
        return model;
      }
    }

    public PolyhedronPage()
    {
      InitializeComponent();
    }
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      MenuItem.CmdRoot = OnCommand;
      modelView.Scene = demo1();
      //modelView.Infos.Add("Hello World!"); 
      panelStory.VisibleChanged += (_, _) =>
      {
        if (timeLineView != null) return;
        timeLineView = new TimeLineView { Dock = DockStyle.Fill };
        panelStory.Controls.Add(timeLineView); timeLineView.BringToFront();
        timeLineView.TimeChange += timechange;
      };
    }
    TimeLineView? timeLineView; string? path;

    int OnCommand(int id, object? test)
    {
      try
      {
        var x = this.modelView.OnCmd(id, test);
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
      timeLineView?.Clear();
      modelView.Scene = scene; this.path = path; // UpdateTitle(); if (last && path != null) mru(path, path);
    }
    int OnNew(object? test)
    {
      if (test != null) return 1;
      timeLineView?.Clear(); this.path = null;
      modelView.Scene = new Models.Scene
      {
        Unit = Models.Scene.Units.Meter,
        Ambient = Color.FromArgb(0x00404040),
        Shadows = true,
        Nodes = new Models.Node[] {
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
      var s = default(string); // path;
      if (saveas || s == null)
      {
        var dlg = new SaveFileDialog() { Filter = "xxd files|*.xxd;*.xxd.png", DefaultExt = "xxd", FileName = s };
        if (dlg.ShowDialog(this) != DialogResult.OK) return 1; s = dlg.FileName;
      }
      if (s == null) return 0;
      var e = Models.Save(scene); e.Save(s); // if (path != s) { path = s; UpdateTitle(); mru(path, path); }
      modelView.IsModified = false; return 1;
    }
    int OnProperties(object? test)
    {
      if (test != null) return 1;
      if (propsView.Visible && !propsView.btnprops.Checked) { propsView.btnprops.PerformClick(); return 1; }
      var t = propsView.IsHandleCreated;
      propsView.Visible ^= true;
      if (!t) propsView.btnstory.Click += (_, _) => panelStory.Visible = (propsView.btnstory.Checked ^= true);
      return 1;
    }
    int OnStoryBoard(object? test)
    {
      if (test != null) return 1;
      var x = propsView.IsHandleCreated; if (!x) OnProperties(null);
      propsView.btnstory.PerformClick(); if (!x) OnProperties(null); return 1;
    }

    int tv;
    void panelStory_MouseDown(object sender, MouseEventArgs e)
    {
      var p = (Control)sender; p.Capture = true;
      tv = p.Dock == DockStyle.Bottom ? p.Height + Cursor.Position.Y : p.Width + Cursor.Position.X;
      p.Cursor = p.Dock == DockStyle.Bottom ? Cursors.SizeNS : Cursors.SizeWE;
    }
    void panelStory_MouseMove(object sender, MouseEventArgs e)
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
    void panelStory_MouseUp(object sender, MouseEventArgs e)
    {
      var p = (Control)sender; p.Capture = false;
      p.Cursor = Cursors.Default;
    }
    void panelStory_MouseLeave(object sender, EventArgs e)
    {
      ((Control)sender).Cursor = Cursors.Default;
    }

    void btn_run_Click(object sender, EventArgs e)
    {
      var scene = modelView.Scene; if (scene == null) return;
      var cam = scene.Find("Camera1") as Models.Camera; if (cam == null) return;
      var obj = scene.Find("Extrusion1") as Models.Node; if (obj == null) return;

      //var m1 = cam.Transform;
      //var m2 = Matrix4x3.CreateTranslation(0, 0, -2) * cam.Transform;
      var m1 = Matrix4x3.Parse("1 0 -0 0 0.7071068 0.7071068 0 -0.7071068 0.7071068 0 -5.0000005 5.0000005", CultureInfo.InvariantCulture);
      var m2 = Matrix4x3.Parse("0.7429419 0.66935587 0 -0.5006389 0.55567694 0.66376495 0.44429496 -0.4931388 0.74794126 2.0379539 -2.396401 3.3004177", CultureInfo.InvariantCulture);

      modelView.Animations +=
        animate(modelView, obj,
          Matrix4x3.Identity,
          Matrix4x3.CreateRotationX(MathF.PI / 2) * Matrix4x3.CreateTranslation(0, 0, 2),
        animate(modelView, obj,
          Matrix4x3.CreateRotationX(MathF.PI / 2) * Matrix4x3.CreateTranslation(0, 0, 2),
          Matrix4x3.CreateRotationY(MathF.PI) * Matrix4x3.CreateRotationX(MathF.PI / 2) * Matrix4x3.CreateTranslation(0, 0, 2),
        animate(modelView, obj,
          Matrix4x3.CreateRotationY(MathF.PI) * Matrix4x3.CreateRotationX(MathF.PI / 2) * Matrix4x3.CreateTranslation(0, 0, 2),
          Matrix4x3.CreateRotationX(MathF.PI / 2) * Matrix4x3.CreateTranslation(0, 0, 2))
        //animate(modelView, cam, m1, m2))
        ));

      static Action animate(DX11Ctrl view, Models.Node node, Matrix4x3 m1, Matrix4x3 m2, Action? next = null)
      {
        Matrix4x4.Decompose(m1, out _, out var q1, out var t1);
        Matrix4x4.Decompose(m2, out _, out var q2, out var t2);
        int count = 0; return zoom;
        void zoom()
        {
          var f = MathF.Min(1, sigmoid(++count / 20f, 4));
          var m = (Matrix4x3)(
            Matrix4x4.CreateFromQuaternion(Quaternion.Lerp(q1, q2, f)) *
            Matrix4x4.CreateTranslation(Vector3.Lerp(t1, t2, f)));
          node.Transform = m; view.Invalidate();
          if (f == 1) { view.Animations -= zoom; view.Animations += next; }
        }
      }

      static float sigmoid(float t, float gamma) => ((1 / MathF.Atan(gamma)) * MathF.Atan(gamma * (2 * t - 1)) + 1) * 0.5f;

    }

    void btn_back_Click(object sender, EventArgs e) { } //0
    void btn_back__Click(object sender, EventArgs e) { }//this.anix - 1);
    void btn_forw__Click(object sender, EventArgs e) { }//this.anix + 1);
    void btn_forw_Click(object sender, EventArgs e) { }//this.anis.Length);
    void btn_record_Click(object sender, EventArgs e)
    {
      if (btn_record.Checked ^= true) { modelView.UndoChanged += undochanged; }
      else { modelView.UndoChanged -= undochanged; }
    }
    void btn_play_Click(object sender, EventArgs e)
    {
      if (btn_play.Checked ^= true) { modelView.Animations += animate; }
      else { modelView.Animations -= animate; }
    }
    void btn_stop_Click(object sender, EventArgs e)
    {
      //if (!btn_stop.Enabled) return;
      //if (btn_play.Checked)
      //{
      //  modelView.Animations -= animate;
      //
      //  //btn_play.Checked = false;
      //  //btn_stop.Enabled = false;
      //  //btn_record.Enabled = true;
      //  //btn_back.Enabled = btn_back_.Enabled = false;// this.anix != 0;
      //  //btn_forw.Enabled = btn_forw_.Enabled = false;// this.anix < this.anis.Length;
      //}
      //else if (btn_record.Checked)
      //{
      //  btn_record.Checked = false; modelView.UndoChanged -= undochanged;
      //  btn_stop.Enabled = false;
      //  //btn_stop.Enabled = btn_stop.Visible = !(btn_play.Visible = true);
      //  //if (modelView.undos == null || modelView.undoi <= undoab) return;
      //  //listView1.Items.Add("*");
      //  btn_back.Enabled = btn_back_.Enabled = true;
      //
      //  //this.anis = modelView.undos.Skip(undoab).ToArray(); this.anix = this.anis.Length;
      //  //anilines = new List<DX11ModelCtrl.AniLine>();
      //  //for (int i = 0; i < this.anis.Length; i++)
      //  //{
      //  //  var tm = getmaxt(anilines);
      //  //  this.anis[i].link(anilines, tm + 100);// i * 1000);
      //  //}
      //  return;
      //  //this.records.Save("C:\\Users\\cohle\\Desktop\\rec.xml");
      //}
    }
    void btn_clear_Click(object sender, EventArgs e)
    {
      if (btn_record.Checked || btn_play.Checked) return;
      timeLineView.Clear();
    }
    void btn_save_Click(object sender, EventArgs _)
    {
      var scene = modelView.Scene;
      scene.anilines = timeLineView.lines.ToList();
      var a = Models.Save(scene);
      var b = XElement.Parse(a.ToString()); 
      var c = Models.Load(b);               
    }
    void animate()
    {
      var time = timeLineView.time; var endtime = timeLineView.endtime;
      if (time >= endtime) { btn_play.PerformClick(); return; }
      timeLineView.time = time = Math.Min(time + 30, endtime); timeLineView.Invalidate();
      var a = timeLineView.lines; var inf = false;
      for (int i = 0; i < a.Count; i++) inf |= a[i].lerp(time);
      if (inf) modelView.Invalidate();
    }

    void undochanged()
    {
      if (modelView.undoi == 0) return;
      var undo = modelView.undos[modelView.undoi - 1];
      timeLineView.Add(undo);
    }
    void timechange()
    {
      var inf = false; var a = timeLineView.lines; var t = timeLineView.time;
      for (int i = 0; i < a.Count; i++) inf |= a[i].lerp(t);
      if (inf) modelView.Invalidate();
    }

    class TimeLineView : UserControl
    {
      internal TimeLineView() { DoubleBuffered = true; AutoScroll = true; }
      internal readonly List<DX11ModelCtrl.AniLine> lines = new();
      internal int time, endtime; float xscale = 0.1f; const int leftofs = 8, dyline = 17;
      readonly List<int> selection = new();
      internal Action? TimeChange;
      internal void Clear()
      {
        lines.Clear(); selection.Clear(); time = endtime = 0;
        AutoScrollMinSize = default; Invalidate();
      }
      internal void Add(DX11ModelCtrl.Undo undo)
      {
        undo.link(lines, getmaxt(lines)); time = endtime = getmaxt(lines); selection.Clear();
        adjust(); var o = AutoScrollPosition; var q = AutoScrollMinSize;
        o.X = q.Width; AutoScrollPosition = o; Invalidate();
      }
      void adjust()
      {
        AutoScrollMinSize = new Size(64 + (int)(endtime * xscale), 16 + lines.Count * 16);
        if (time > endtime) { time = endtime; Invalidate(); }
      }
      static int getmaxt(List<DX11ModelCtrl.AniLine> a)
      {
        var max = 0; for (int i = 0; i < a.Count; i++) { var t = a[i].times; max = Math.Max(max, t[t.Count - 2] + t[t.Count - 1]); }
        return max;
      }
      protected override void OnPaint(PaintEventArgs e)
      {
        var g = e.Graphics; var s = ClientSize; var o = AutoScrollPosition;
        g.TranslateTransform(leftofs + o.X, o.Y);
        var xe = (int)(endtime * xscale); g.DrawLine(Pens.LightGray, xe, 0, xe, s.Height - o.Y);
        for (int i = 0, y = 0; i < lines.Count; i++, y += dyline)
        {
          var l = lines[i]; var tt = l.times;
          for (int k = 0; k < tt.Count; k += 2)
          {
            var t = tt[k]; var dt = tt[k + 1]; var se = selection.Contains((i << 16) | k);
            if (se) g.FillRectangle(SystemBrushes.GradientInactiveCaption, t * xscale, y, dt * xscale, dyline - 1);
            g.DrawRectangle(Pens.Black, t * xscale, y, dt * xscale, dyline - 1);
          }
          g.DrawLine(Pens.LightGray, 0, y + dyline, s.Width - o.X, y + dyline);
        }
        var x = (int)(time * xscale); g.DrawLine(Pens.Red, x, 0, x, s.Height - o.Y);
      }
      int wo, st, pcx;
      void select(int xy)
      {
        if (selection.Count == 1 && selection[0] == xy) return;
        selection.Clear(); selection.Add(xy); Invalidate();
      }
      protected override void OnMouseMove(MouseEventArgs e)
      {
        if (!Capture)
        {
          var o = AutoScrollPosition; var x = o.X + leftofs + (int)(time * xscale);
          wo = Math.Abs(e.X - x) < 4 ? 1 : 0;
          if (wo == 0)
          {
            var i = (e.Y - o.Y) / dyline; var tx = (e.X - leftofs - o.X) / xscale;
            if (i >= 0 && i < lines.Count)
            {
              var a = lines[i].times;
              for (int k = a.Count - 2; k >= 0; k -= 2)
              {
                var t = a[k]; var dt = a[k + 1]; var w = 0x40000000 | k | (i << 16);
                if (Math.Abs(tx - (t + dt)) * xscale < 4) { wo = w | 1; break; }
                if (tx >= t && tx <= t + dt) { wo = w; break; }
              }
            }
          }
          this.Cursor = wo == 1 ? Cursors.VSplit : (wo & 0x40000001) == 0x40000001 ? Cursors.SizeWE : Cursors.Default; return;
        }
        if (wo == 0) return; var fdt = (e.X - pcx) / xscale;
        if (wo == 1)
        {
          if (st == -1) st = time;
          var u = Math.Max(0, Math.Min(endtime, (int)(st + fdt)));
          if (time == u) return; time = u; Invalidate(); Update(); TimeChange?.Invoke();
        }
        else //if ((wo & 0x40000000) != 0)
        {
          int x = wo & 0xffff, y = (wo >> 16) & 0x3fff; var times = lines[y].times;
          if (st == -1) { st = times[x]; }
          var u = (int)(st + fdt);
          if ((wo & 1) == 0)
          {
            u = Math.Max(0, u);
          }
          else
          {
            u = Math.Max(20, u);
          }
          if (times[x] == u) return;
          var et = getmaxt(lines); if (et != endtime) endtime = et;
          times[x] = u; Invalidate(); Update(); TimeChange?.Invoke();
        }
      }
      protected override void OnMouseDown(MouseEventArgs e)
      {
        Focus(); if (wo == 0) return;
        if ((wo & 0x40000001) == 0x40000000) select(wo & 0x3fffffff);
        pcx = e.X; st = -1; Capture = true;
      }
      protected override void OnMouseUp(MouseEventArgs e)
      {
        Capture = false; if ((wo & 0x40000000) != 0) adjust();
      }
      protected override void OnMouseWheel(MouseEventArgs e)
      {
      }
      protected override void OnKeyDown(KeyEventArgs e)
      {
        if (e.KeyCode == Keys.Delete)
        {
          e.Handled = true;
        }
        base.OnKeyDown(e);
      }
    }
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
  public class DX11PropsCtrl : DX11ModelCtrl.PropsCtrl
  {
  }
}
