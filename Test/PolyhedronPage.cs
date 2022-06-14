using System.ComponentModel;
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
      //modelView.Infos.Add("@2");
    }
    int OnCommand(int id, object? test)
    {
      try
      {
        var x = this.modelView.OnCmd(id, test);
        if (x != 0) return x;
        switch (id)
        {
          case 2008: return OnProperties(test);
          case 2009: return OnToolbox(test);
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
      if (propsView.Visible && toolbox != null && toolbox.Visible) { showtb(false); return 1; }
      propsView.Visible ^= true; if (propsView.Visible && toolbox != null && toolbox.Visible) showtb(false);
      return 1;
    }
    int OnToolbox(object? test)
    {
      if (test != null) return 1;
      if (toolbox == null) propsView.Controls.Add(toolbox = new WebBrowser()
      {
        Visible = false,
        Dock = DockStyle.Fill,
        AllowNavigation = true,
        ScriptErrorsSuppressed = true,
        WebBrowserShortcutsEnabled = false,
        IsWebBrowserContextMenuEnabled = false,
        //ScrollBarsEnabled = false,
        Url = new Uri("https://c-ohle.github.io/RationalNumerics/web/cat.htm"),
        //Url = new Uri("file://C:/Users/cohle/Desktop/RationalNumericsDoc/web/cat.htm"),
      });
      if (propsView.Visible && toolbox != null && !toolbox.Visible) { showtb(true); return 1; }
      propsView.Visible ^= true; if (propsView.Visible && toolbox != null && !toolbox.Visible) showtb(true);
      return 1;
    }
    int OnStoryBoard(object? test)
    {
      if (test != null) return 1;
      panelStory.Visible ^= true;
      return 1;
    }
    WebBrowser? toolbox;
    void showtb(bool on)
    {
      var a = propsView.Controls;
      for (int i = 0; i < a.Count; i++) { var p = a[i]; p.Visible = p is WebBrowser == on; }
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
                Diffuse = (uint)Color.LightGray.ToArgb(), /*Texture = tex1*/ }) },
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

    }

    int xx;
    void panelStory_MouseEnter(object sender, EventArgs e)
    {
      var p = (Control)sender; p.Cursor = p.Dock == DockStyle.Bottom ? Cursors.SizeNS : Cursors.SizeWE;
    }
    void panelStory_MouseLeave(object sender, EventArgs e)
    {
      var p = (Control)sender; p.Cursor = Cursors.Default;
    }
    void panelStory_MouseDown(object sender, MouseEventArgs e)
    {
      var p = (Control)sender; p.Capture = true;
      xx = p.Dock == DockStyle.Bottom ? p.Height + Cursor.Position.Y : p.Width + Cursor.Position.X;
    }
    void panelStory_MouseMove(object sender, MouseEventArgs e)
    {
      var p = (Control)sender;
      if (p.Capture)
      {
        if (p.Dock == DockStyle.Bottom)
          p.Height = Math.Max(p.Padding.Top, Math.Min(p.Parent.ClientSize.Height, xx - Cursor.Position.Y));
        else
          p.Width = Math.Max(p.Padding.Left, Math.Min(p.Parent.ClientSize.Width, xx - Cursor.Position.X));
        p.Parent.Update(); return;
      }
    }
    void panelStory_MouseUp(object sender, MouseEventArgs e)
    {
      var p = (Control)sender; p.Capture = false;
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
