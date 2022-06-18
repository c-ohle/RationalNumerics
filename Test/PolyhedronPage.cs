#pragma warning disable CS8602
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;
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
      modelView.Scene = demo1();
      modelView.Infos.Add("@1"); // modelView.Adapter);
      modelView.Infos.Add("@2"); // fps
      panelStory.VisibleChanged += (_, _) =>
      {
        if (timeLineView != null) return;
        timeLineView = new TimeLineView { Dock = DockStyle.Fill };
        panelStory.Controls.Add(timeLineView); timeLineView.BringToFront();
      };
    }
    TimeLineView? timeLineView;

    int OnCommand(int id, object? test)
    {
      try
      {
        var x = this.modelView.OnCmd(id, test);
        if (x != 0) return x;
        switch (id)
        {
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
 
    void btn_record_Click(object sender, EventArgs e)
    {
      if (btn_record.Checked) return;
      btn_record.Checked = btn_stop.Enabled = true;
      btn_play.Enabled = btn_back.Enabled = btn_forw.Enabled = btn_back_.Enabled = btn_forw_.Enabled = false;      
      timeLineView.Clear(); 
      modelView.UndoChanged += undochanged;
    }
    void undochanged()
    {
      if (modelView.undoi == 0) return;
      var undo = modelView.undos[modelView.undoi - 1];
      timeLineView.Add(undo);
    }
    void gotoi(int x)
    {
      //if (this.anis == null) return;
      //x = Math.Max(0, Math.Min(this.anis.Length, x)); if (x == this.anix) return;
      //for (; this.anix < x;) this.anis[this.anix++].exec();
      //for (; this.anix > x;) this.anis[--this.anix].exec();
      //modelView.Invalidate();
      //btn_back.Enabled = btn_back_.Enabled = this.anix != 0;
      //btn_forw.Enabled = btn_forw_.Enabled = this.anix < this.anis.Length;
      //btn_play.Enabled = btn_forw.Enabled;
      //listView1.SelectedIndices.Clear();
      //if (x < listView1.Items.Count) { listView1.Items[x].Selected = true; listView1.EnsureVisible(x); }
    }
    void btn_back_Click(object sender, EventArgs e) => gotoi(0);
    void btn_back__Click(object sender, EventArgs e) => gotoi(0);//this.anix - 1);
    void btn_forw__Click(object sender, EventArgs e) => gotoi(0);//this.anix + 1);
    void btn_forw_Click(object sender, EventArgs e) => gotoi(0);//this.anis.Length);
    void btn_play_Click(object sender, EventArgs e)
    {
      if (btn_play.Checked) return;
      btn_play.Checked = true;
      btn_stop.Enabled = true;
      btn_record.Enabled = false;
      btn_back_.Enabled = btn_forw_.Enabled = false;
      btn_back.Enabled = btn_forw.Enabled = false;
      //time = 0; modelView.Animations += animate;
    }

    static int getmaxt(List<DX11ModelCtrl.AniLine> a)
    {
      var max = 0;
      for (int i = 0; i < a.Count; i++) { var t = a[i].times; max = Math.Max(max, t[t.Count - 2] + t[t.Count - 1]); }
      return max;
    }

    //List<DX11ModelCtrl.AniLine>? anilines; int time;
    void animate()
    {
      //var inf = false;
      //for (int i = 0; i < anilines.Count; i++) inf |= anilines[i].lerp(time);
      //if (inf) modelView.Invalidate(); time += 10;
    }
    void btn_stop_Click(object sender, EventArgs e)
    {
      if (!btn_stop.Enabled) return;
      if (btn_play.Checked)
      {
        modelView.Animations -= animate;

        btn_play.Checked = false;
        btn_stop.Enabled = false;
        btn_record.Enabled = true;
        btn_back.Enabled = btn_back_.Enabled = false;// this.anix != 0;
        btn_forw.Enabled = btn_forw_.Enabled = false;// this.anix < this.anis.Length;
      }
      else if (btn_record.Checked)
      {
        btn_record.Checked = false; modelView.UndoChanged -= undochanged;
        btn_stop.Enabled = false;
        //btn_stop.Enabled = btn_stop.Visible = !(btn_play.Visible = true);
        //if (modelView.undos == null || modelView.undoi <= undoab) return;
        //listView1.Items.Add("*");
        btn_back.Enabled = btn_back_.Enabled = true;

        //this.anis = modelView.undos.Skip(undoab).ToArray(); this.anix = this.anis.Length;
        //anilines = new List<DX11ModelCtrl.AniLine>();
        //for (int i = 0; i < this.anis.Length; i++)
        //{
        //  var tm = getmaxt(anilines);
        //  this.anis[i].link(anilines, tm + 100);// i * 1000);
        //}
        return;
        //this.records.Save("C:\\Users\\cohle\\Desktop\\rec.xml");
      }
    }

    class TimeLineView : UserControl
    {
      internal readonly List<DX11ModelCtrl.AniLine> anilines = new();
      internal int time, endtime; float xscale = 0.1f;
      internal TimeLineView() { DoubleBuffered = true; AutoScroll = true; }
      internal void Clear()
      {
        anilines.Clear(); time = endtime = 0;
        AutoScrollMinSize = default;
      }
      internal void Add(DX11ModelCtrl.Undo undo)
      {
        undo.link(anilines, getmaxt(anilines)); time = endtime = getmaxt(anilines);
        AutoScrollMinSize = new Size(64 + (int)(endtime * xscale), 16 + anilines.Count * 16);
        var o = AutoScrollPosition; var q = AutoScrollMinSize;
        o.X = q.Width; AutoScrollPosition = o; Invalidate();
      }
      protected override void OnPaint(PaintEventArgs e)
      {
        var g = e.Graphics; var o = AutoScrollPosition; g.TranslateTransform(o.X, o.Y);
        for (int i = 0; i < anilines.Count; i++)
        {
          var l = anilines[i]; var tt = l.times;
          for (int k = 0; k < tt.Count; k += 2)
          {
            var t = tt[k]; var dt = tt[k + 1]; endtime = Math.Max(endtime, t + dt);
            g.DrawRectangle(Pens.Black, t * xscale, i * 16, dt * xscale, 16);
          }
        }
        var x = (int)(time * xscale); g.DrawLine(Pens.Red, x, 0, x, Height);
      }

      //internal Action? TimeChange;
      int wo, st; Point pc;
      protected override void OnMouseDown(MouseEventArgs e)
      {
        if (wo == 1) { pc = e.Location; st = time; Capture = true; }
      }
      protected override void OnMouseMove(MouseEventArgs e)
      {
        if (Capture)
        {
          if (wo == 1)
          {
            var u = Math.Max(0, Math.Min(endtime, (int)(st + (e.X - pc.X) / xscale)));
            if (time == u) return;
            time = u; Invalidate(); Update(); //TimeChange?.Invoke();

            var view = (DX11ModelCtrl?)anilines[0].target.GetService(typeof(DX11ModelCtrl));
            if (view != null) 
            {
              var inf = false;
              for (int i = 0; i < anilines.Count; i++) inf |= anilines[i].lerp(time);
              if (inf) view.Invalidate();
            }
          }
          return;
        }
        var o = AutoScrollPosition;
        var x = o.X + (int)(time * xscale);
        wo = Math.Abs(e.X - x) < 4 ? 1 : 0;
        this.Cursor = wo == 1 ? Cursors.VSplit : Cursors.Default;
      }
      protected override void OnMouseUp(MouseEventArgs e)
      {
        Capture = false;
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
