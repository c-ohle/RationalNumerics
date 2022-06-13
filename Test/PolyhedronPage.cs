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
        }
      }
      catch (Exception e)
      {
        if (test == null) MessageBox.Show(e.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        else Debug.WriteLine(e.Message);
      }
      return 0;
    }
    private int OnProperties(object? test)
    {
      if (test == null)
      {
        propsView.Visible ^= true;
        //this.splitContainer1.Panel2Collapsed = !this.splitContainer1.Panel2Collapsed;
      }
      return 1;
    }
    static Models.Scene demo1()
    {
      //var tex1 = DX11Ctrl.GetTexture("https://raw.githubusercontent.com/c-ohle/CSG-Project/master/bin/textures/wood1.png");
      //var tex2 = DX11Ctrl.GetTexture("https://raw.githubusercontent.com/c-ohle/CSG-Project/master/bin/textures/bricks1.png");
      return new Models.Scene
      {
        Unit = Models.Scene.Units.Meter,
        Ambient = Color.FromArgb(0x00404040),
        Shadows = true,
        Nodes = new Models.Node[] {
            new Models.Camera {
              Name = "Camera1", Fov = 30, Near = 0.1f, Far = 1000,
              Transform = !(Matrix4x3)Matrix4x4.CreateLookAt(new Vector3(-5, -3, 2), new Vector3(), new Vector3(0, 0, 1)),
            },
            new Models.Light {
              Name = "Light1",
              Transform = !(Matrix4x3)Matrix4x4.CreateLookAt(new Vector3(), -new Vector3(+1, -0.5f, -2), new Vector3(0, 0, 1)),
            },
            new Models.BoxGeometry {
              Name = "Ground", Fixed = true,
              Transform = Matrix4x3.CreateTranslation(0, 0, 0),
              p1 = new Vector3(-10, -10, -0.1f), p2 = new Vector3(+10, +10, 0),
              ranges = new (int, Models.Material)[] { (0, new Models.Material {
                Diffuse = (uint)Color.LightGray.ToArgb(), /*Texture = tex1*/ }) },
            },
            new Models.BoxGeometry {
              Name="Box1",
              Transform = Matrix4x3.CreateTranslation(0, 0, 0),
              p2 = new Vector3(1, 1, 1),
              ranges = new (int, Models.Material)[] { (0, new Models.Material {
                Diffuse = (uint)Color.Gold.ToArgb(), /*Texture = tex2*/ } ) },
            },
          }
      };
    }

    //WebBrowser? wb;
    private void button_toolbox_Click(object sender, EventArgs e)
    {                
      //if (wb == null)
      //{
      //  wb = new WebBrowser() { Bounds = new Rectangle(8,8,300,400) };
      //  wb.AllowNavigation = true;
      //  wb.ScriptErrorsSuppressed = true;
      //  modelView.Controls.Add(wb);
      //  //wb.Navigate("https://htmlpreview.github.io/?https://github.com/c-ohle/RationalNumerics/blob/master/Test/web/cat.htm");
      //  //wb.Navigate("https://htmlpreview.github.io/?https://github.com/c-ohle/RationalNumerics/blob/master/Test/web/cat.htm");
      //
      //  wb.Navigate("https://c-ohle.github.io/RationalNumerics/");
      //  //wb.Navigate("file://C:/Users/cohle/Desktop/RationalNumerics/Test/web/cat.htm");
      //}
      //else { wb.Dispose(); wb = null; }
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
