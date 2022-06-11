﻿#pragma warning disable CS8601, CS8600, CS8602, CS8603, CS8604
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows.Forms.Design;
using System.Xml.Linq;
using Node = Test.DX11ModelCtrl.Models.Node;

namespace Test
{
  public unsafe class DX11ModelCtrl : DX11Ctrl
  {
    internal Models.Settings? settings;
    [Browsable(false)]
    public Models.Scene? Scene
    {
      get => scene;
      set
      {
        if (scene != null)
        {
          scene.root = null; undos = null; undoi = 0; selection.Clear();
          camera = null; light = null; Invalidate();
        }
        scene = value; if (scene != null) scene.root = this;
      }
    }
    internal List<string>? Infos;
    Models.Scene? scene;
    Models.Camera? camera; Models.Light? light;
    List<Models.Geometry>? transp;
    List<Node>? selection;
    List<Action>? undos; int undoi;
    Action<DC>? RenderClient;
    int flags = 0x01 | 0x02 | 0x04 | 0x100; //0x01:SelectBox, 0x02:Select Pivot, 0x04:Wireframe, 0x08:Normals, 0x100:Shadows 

    protected override void OnLoad(EventArgs e)
    {
      Initialize(4L << 32);
      BkColor = (uint)BackColor.ToArgb();
      selection = new();
      settings = new();
      transp = new();
    }
    void link(Models.Base node)
    {
      if (camera == null) camera = node as Models.Camera;
      if (light == null) light = node as Models.Light;
      var a = node.VisibleNodes;
      if (a != null) for (int i = 0; i < a.Length; i++) { var p = a[i]; p.Parent = node; link(p); }
      if (node is Models.Geometry geo) geo.checkbuild(0);
    }
    void render(DC dc, int was, Node[] nodes)
    {
      var tm = dc.Transform;
      for (int i = 0; i < nodes.Length; i++)
      {
        var node = nodes[i];
        dc.Transform = node.Transform * tm;
        var pp = node.VisibleNodes; if (pp != null) render(dc, was, pp);
        var geo = node as Models.Geometry; if (geo == null) continue;
        if (was == 0) dc.Select(node);
        if (was == 2) { dc.DrawMesh(geo.vb, geo.ib); continue; }
        var rr = geo.ranges;
        for (int k = 0, a = 0, w = was, c; k < rr.Length; k++, a += c)
        {
          var mat = rr[k].material; c = rr[k].count;
          if (mat.Diffuse >> 24 != 0xff) { if (w == 0) { w = 1; transp.Add(geo); } continue; }
          dc.Color = mat.Diffuse;
          if (mat.Texture != null)
          {
            dc.Texture = mat.Texture; dc.PixelShader = PixelShader.Texture;
            dc.VertexShader = VertexShader.Tex; dc.Textrans = mat.Transform;
          }
          else { dc.PixelShader = PixelShader.Color3D; dc.VertexShader = VertexShader.Lighting; }
          dc.DrawMesh(geo.vb, geo.ib, a, c);
        }
      }
      dc.Transform = tm;
    }
    protected override void OnRender(DC dc)
    {
      var size = dc.Viewport;
      if (scene != null)
      {
        camera = null; light = null; link(scene);
        if (camera != null)
        {
          dc.Projection = !camera.GetTransform() *
            Matrix4x4.CreatePerspectiveFieldOfView(camera.Fov * (MathF.PI / 180), size.X / size.Y, camera.Near, camera.Far);
          var lightdir = light != null ? light.GetTransform()[2] : default;// Vector3.Normalize(new Vector3(+1, -0.5f, -2));
          var shadows = (flags & 0x100) != 0;
          dc.Light = shadows ? lightdir * 0.3f : lightdir;
          dc.Ambient = scene.ambient;
          dc.State = State.Default3D;
          dc.Transform = Matrix4x4.Identity;
          render(dc, 0, scene.Nodes);
          dc.Select();
          if (shadows && !dc.IsPicking)
          {
            var t4 = dc.State; dc.Light = lightdir; dc.LightZero = -5; //todo: calc
            dc.State = State.Shadows3D;
            render(dc, 2, scene.Nodes);
            dc.State = t4;
            dc.Ambient = 0;
            dc.Light = lightdir * 0.7f;
            dc.BlendState = BlendState.AlphaAdd;
            render(dc, 1, scene.Nodes);
            dc.Clear(CLEAR.STENCIL);
            dc.State = t4;
            dc.Light = lightdir;
            dc.Ambient = scene.ambient;
          }
          if (selection.Count != 0 || RenderClient != null)
          {
            dc.PixelShader = PixelShader.Color;
            dc.DepthStencil = DepthStencil.ZWrite;
            dc.Rasterizer = Rasterizer.CullNone;
            if ((flags & 4) != 0 && !dc.IsPicking)
            {
              dc.Color = 0x40000000; var t1 = dc.State;
              dc.BlendState = BlendState.Alpha;
              dc.Rasterizer = Rasterizer.Wireframe;
              for (int i = 0; i < selection.Count; i++) wire(dc, selection[i]);
              static void wire(DC dc, Node p)
              {
                if (p is Models.Geometry geo) { geo.checkbuild(0); dc.Transform = p.GetTransform(); dc.DrawMesh(geo.vb, geo.ib); }
                var a = p.VisibleNodes; if (a != null) for (int i = 0; i < a.Length; i++) wire(dc, a[i]);
              }
              dc.State = t1;
            }
            if ((flags & (1 | 2)) != 0)
            {
              for (int i = 0; i < selection.Count; i++)
              {
                var node = selection[i]; var box = node.GetBox();
                if (box.Min.X == float.MaxValue) box = default;
                dc.Transform = node.GetTransform();
                dc.Select(node);
                if ((flags & 1) != 0) { dc.Color = 0xffffffff; dc.DrawBox(box); }
                if ((flags & 2) != 0)
                {
                  var px = new Vector3(box.Max.X + 0.25f, 0, 0);
                  var py = new Vector3(0, box.Max.Y + 0.25f, 0);
                  var pz = new Vector3(0, 0, box.Max.Z + 0.25f); float l = 0.12f, r = 0.02f;
                  dc.Color = 0xffff0000; dc.DrawLine(default, px); dc.DrawArrow(px, Vector3.UnitX * l, r);
                  dc.Color = 0xff00ff00; dc.DrawLine(default, py); dc.DrawArrow(py, Vector3.UnitY * l, r);
                  dc.Color = 0xff0000ff; dc.DrawLine(default, pz); dc.DrawArrow(pz, Vector3.UnitZ * l, r);
                }
              }
              dc.Select();
            }
            if (!dc.IsPicking) RenderClient?.Invoke(dc);
            var main = selection.Count == 1 ? selection[0] as Models.Geometry : null;
            if (main != null) main.Render(dc, main);
          }
          if (transp.Count != 0)
          {
            for (int i = 0; i < transp.Count; i++)
            {
              var geo = transp[i];
              dc.Transform = geo.GetTransform(scene);
              dc.State = State.Default3D;
              dc.BlendState = BlendState.Alpha; dc.Select(geo);
              var rr = geo.ranges;
              for (int k = 0, a = 0, c; k < rr.Length; k++, a += c)
              {
                var mat = rr[k].material; c = rr[k].count;
                if (mat.Diffuse >> 24 == 0xff) continue;
                dc.Color = mat.Diffuse;
                if (mat.Texture != null)
                {
                  dc.Texture = mat.Texture; dc.PixelShader = PixelShader.Texture;
                  dc.VertexShader = VertexShader.Tex; dc.Textrans = mat.Transform;
                }
                else { dc.PixelShader = PixelShader.Color3D; dc.VertexShader = VertexShader.Lighting; }
                dc.DrawMesh(geo.vb, geo.ib, a, c);
              }
            }
            dc.Select(); transp.Clear();
          }
        }
      }
      if (Infos != null)
      {
        dc.Projection = Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, -1000, +1000);
        dc.Transform = Matrix4x4.Identity; dc.State = State.Default2D;
        dc.Color = 0xff000000; var font = dc.Font; var y = 8f;
        for (int i = 0; i < Infos.Count; i++, y += dc.Font.Height)
          dc.DrawText(8, y + dc.Font.Ascent, Infos[i]);
      }
    }
    protected override int OnMouse(int id, PC pc)
    {
      switch (id)
      {
        case 0x0200: //WM_MOUSEMOVE
          //{ if (pc.Hover is UI.Frame frame) { frame.OnMouse(id, pc); break; } }
          Cursor =
            (pc.Id & 0x10000000) != 0 ? Cursors.Cross :
            (pc.Id & 0x20000000) != 0 ? Cursors.UpArrow : Cursors.Arrow;
          break;
        case 0x0201: //WM_LBUTTONDOWN
          {
            //{ if (pc.Hover is UI.Frame frame) { frame.OnMouse(id, pc); break; } }
            var main = pc.Hover as Node; var keys = ModifierKeys;
            var layer = selection.Count != 0 ? selection[0].Parent : scene;
            if (main != null) for (; !IsSelect(main) && main.Parent is Node t && t != layer; main = t) ;
            if (main == null || main.Fixed)
            {
              pc.SetTool(
                keys == Keys.Control ? tool_cam_move(pc) :
                keys == Keys.Shift ? tool_cam_vert(pc) :
                keys == Keys.Alt ? tool_cam_decl(pc) :
                keys == (Keys.Shift | Keys.Control) ? tool_cam_incl(pc) :
                tool_cam_select(pc));
            }
            else
            {
              if (main is Models.Geometry geo)
              {
                var p = geo.GetTool(pc, main);
                if (p != null) { pc.SetTool(p); break; }
              }
              pc.SetTool(
                keys == Keys.Control ? tool_obj_drag(pc, main) :
                keys == Keys.Shift ? tool_obj_vert(pc, main) :
                keys == Keys.Alt ? tool_obj_rot(pc, main) :
                keys == (Keys.Control | Keys.Shift) ? tool_obj_rot_axis(pc, main, 0) :
                keys == (Keys.Control | Keys.Alt) ? tool_obj_rot_axis(pc, main, 1) :
                keys == (Keys.Control | Keys.Shift | Keys.Alt) ? tool_obj_rot_axis(pc, main, 2) :
                tool_obj_move(pc, main));
            }
          }
          break;
        case 0x020A: wheel(pc); return 1; //WM_MOUSEWHEEL
        case 0x0233: //WM_DROPFILES
          try { pc.SetTool(tool_drop(pc)); }
          catch (Exception e) { Debug.WriteLine(e.Message); }
          return 1;
      }
      return 0;
    }

    public int OnCmd(int id, object? test)
    {
      switch (id)
      {
        case 2000: return OnUndo(test);
        case 2001: return OnRedo(test);
        case 2002: return OnCut(test);
        case 2003: return OnCopy(test);
        case 2004: return OnPaste(test);
        case 2005: return OnDelete(test);
        case 2006: return OnGroup(test);
        case 2007: return OnUngroup(test);
        //case 2008: return OnProperties(test);
        case 2009: return OnSelectAll(test);
        case 2050: return OnIntersect(id, test); // Union
        case 2051: return OnIntersect(id, test); // Difference
        case 2052: return OnIntersect(id, test); // Intersection
        case 2053: return OnIntersect(id, test); // Halfspace"
        case 2054: return OnCheckMash(test);
        case 2055: return OnConvert(test);
        case 2100: //Select Box
        case 2101: //Select Pivot
        case 2102: //Select Wireframe
        case 2103: //Select Normals
        case 2108: //Shadows
          return OnFlags(test, id);
        case 3015: return base.OnDriver(test);
        case 3016: return base.OnSamples(test);
      }
      return 0;
    }
    int OnRedo(object? test)
    {
      if (undos == null || undoi >= undos.Count) return 0;
      if (test == null) { undos[undoi++](); Invalidate(); Focus(); }
      return 1;
    }
    int OnUndo(object? test)
    {
      if (undos == null || undoi == 0) return 0;
      if (test == null) { undos[--undoi](); Invalidate(); Focus(); }
      return 1;
    }
    int OnCut(object? test)
    {
      if (OnDelete(this) == 0) return 0;
      if (test != null) return 1;
      OnCopy(null); return OnDelete(null);
    }
    int OnCopy(object? test)
    {
      if (selection.Count == 0) return 0;
      if (test != null) return 1;
      var xml = Models.Save(new Models.Scene { Unit = scene.Unit, Nodes = selection.ToArray() });
      Clipboard.SetData("x³", xml.ToString());
      return 1;
    }
    int OnPaste(object? test)
    {
      if (Clipboard.ContainsData("x³"))
      {
        if (test != null || this.scene.Nodes == null) return 1;
        var xml = XElement.Parse((string)Clipboard.GetData("x³"));
        var p = Models.Load(xml);
        Execute(undonodes(this.scene, this.scene.Nodes.Concat(p.Nodes).ToArray()), undosel(p.Nodes));
        return 0;
      }
      return 0;
    }
    int OnDelete(object? test)
    {
      if (selection.Count == 0 || scene.Nodes == null) return 0;
      if (test != null) return 1;
      Execute(undosel(null), undonodes(selection[0].Parent, scene.Nodes.Except(selection).ToArray()));
      return 1;
    }
    int OnSelectAll(object? test)
    {
      if (test != null) return 1;
      selection.Clear(); selection.AddRange(scene.Nodes.Where(p => !p.Fixed && isgeo(p))); Invalidate();
      static bool isgeo(Node p)
      {
        if (p is Models.Geometry) return true;
        if (p.Nodes != null) for (int i = 0; i < p.Nodes.Length; i++) if (isgeo(p.Nodes[i])) return true;
        return false;
      }
      return 0;
    }
    int OnGroup(object? test)
    {
      if (selection.Count == 0) return 0;
      if (test != null) return 1;
      var box = (Min: new Vector3(float.MaxValue), Max: new Vector3(-float.MaxValue));
      foreach (var p in selection) p.GetBox(ref box, p.Transform);
      if (box.Min.X == float.MaxValue) box = default;
      var g = new Node { Nodes = selection.ToArray() }; var pm = (box.Min + box.Max) * 0.5f; pm.Z = box.Min.Z;
      var m = Matrix4x3.CreateTranslation(pm); g.Transform = m; m = !m;
      var nodes = this.scene.Nodes.Except(selection).Concat(Enumerable.Repeat(g, 1)).ToArray();
      Execute(undonodes(this.scene, nodes), selection.Select(p => undotrans(p, p.Transform * m)), undosel(g));
      return 1;
    }
    int OnUngroup(object? test)
    {
      var groups = selection.Where(p => p.Nodes != null);
      if (!groups.Any()) return 0;
      if (test != null) return 1; //var scene = groups.First().Parent;
      var childs = groups.SelectMany(p => p.Nodes ?? Array.Empty<Node>()).ToArray();
      var nodes = this.scene.Nodes.Except(groups).Concat(childs).ToArray();
      Execute(undonodes(this.scene, nodes),
        childs.Select(p => undotrans(p, p.Transform * ((Node)p.Parent).Transform)), undosel(childs));
      return 1;
    }
    int OnFlags(object? test, int id)
    {
      var f = 1 << (id - 2100);
      if (test != null)
      {
        if (id < 2108 && selection.Count == 0 && test is ToolStripButton) return 0;
        return (flags & f) != 0 ? 3 : 1;
      }
      flags ^= f; //Application.UserAppDataRegistry.SetValue("fl", flags);
      Invalidate(); return 0;
    }
    int OnCheckMash(object? test)
    {
      if (selection.Count != 1 || selection[0] is not Models.Geometry geo) return 0;
      if (test != null) return 1;
      check(geo); return 0;
      static void check(Models.Geometry geo)
      {
        Cursor.Current = Cursors.WaitCursor;
        var ss = (string)null;
        var pp = geo.vertices.Select(p => (Vector3R)p).ToArray();
        var ii = geo.indices;
        var t1 = ii.Count(i => i >= pp.Length);
        if (t1 != 0) ss += $"\n{t1} {"Invalid Indices"}";
        var t2 = pp.Length - pp.Distinct().Count();
        if (t2 != 0) ss += $"\n{t2} {"Duplicate Vertices"}";
        var t3 = pp.Length - ii.Distinct().Count();
        if (t3 != 0) ss += $"\n{t3} {"Unused Vertices"}";
        var ee = Enumerable.Range(0, ii.Length / 3).
          Where(i => Math.Max(Math.Max(ii[i * 3], ii[i * 3 + 1]), ii[i * 3 + 2]) < pp.Length).
          Select(i => (k: i *= 3, e: PlaneR.FromVertices(pp[ii[i]], pp[ii[i + 1]], pp[ii[i + 2]]))).
          GroupBy(p => p.e, p => p.k).Select(p => (e: p.Key, kk: p.ToArray())).ToArray();
        var t4 = ee.FirstOrDefault(e => e.e.Normal == default);
        if (t4.kk != null) ss += $"\n{t4.kk.Length} {"Empty Polygones"}";
        var eb = Enumerable.Range(0, ii.Length).
          Select(i => (a: ii[i], b: ii[i % 3 != 2 ? i + 1 : i - 2])).ToHashSet();//.ToLookup(p => p);// ToArray();
        var t5 = eb.Count(p => !eb.Contains((p.b, p.a)));
        if (t5 != 0) ss += $"\n{t5} {"Open Edges"}";
        var t6 = ss != null ? 0 : ee.Sum(e =>
        {
          var tt = e.kk.SelectMany(k => Enumerable.Range(k, 3)).
            Select(i => (a: ii[i], b: ii[i % 3 != 2 ? i + 1 : i - 2])).ToArray();
          return tt.Count(p => !tt.Contains((p.b, p.a)));
        });
        Cursor.Current = Cursors.Default;
        MessageBox.Show(ss != null ? "Mesh Errors:\n" + ss :
            $"Mesh ok\n" +
            $"\n{pp.Length} {"Points"}" +
            $"\n{ii.Length / 3} {"Polygones"}" +
            $"\n{ee.Length} {"Planes"}" +
            $"\n{t6 / 2} {"Edges"}" +
            $"\n{geo.ranges.Length} {"Ranges"}" +
            $"\n{geo.ranges.Select(p => p.material).Distinct().Count()} {"Materials"}" +
            $"\n{geo.ranges.Select(p => p.material.Texture).OfType<object>().Distinct().Count()} {"Textures"}",
            Application.ProductName, MessageBoxButtons.OK,
            ss != null ? MessageBoxIcon.Error : MessageBoxIcon.Information);
      }
    }
    int OnIntersect(int id, object? test)
    {
      //2050: Union 2051: Difference 2052: Intersection 2053: Halfspace"
      if (selection.Count != 2) return 0;
      var a = selection[0] as Models.Geometry; if (a == null) return 0;
      var b = selection[1] as Models.Geometry; if (b == null) return 0;
      if (test != null) return 1;
      Cursor.Current = Cursors.WaitCursor;
      var m = (Matrix4x3R)b.Transform * !(Matrix4x3R)a.Transform;
      var mb = PolyhedronR.GetInstance();
      mb.SetMesh(0, a.GetVertices(), a.indices);
      if (id == 2053) //Halfspace
      {
        var plane = PlaneR.Transform(new PlaneR(0, 0, -1, 0), m);
        if (mb.Cut(plane))
        {
          var ras = Models.BoolGeometry.remap(mb.Indices, mb.Mapping, a, b);
          var c = new Models.MeshGeometry
          {
            Name = a.Name,
            Transform = a.Transform,
            rpts = mb.Vertices.ToArray(),
            indices = mb.Indices.Select(p => (ushort)p).ToArray(),
            ranges = ras
          };
          var xx = a.Parent.Nodes.Select(p => p == a ? c : p).ToArray();
          Execute(undosel(a), undonodes(a.Parent, xx), undosel(c));
        }
      }
      else
      {
        mb.SetMesh(1, b.GetVertices(), b.indices);
        mb.Transform(1, m); //var t1 = Environment.TickCount;
        mb.Boolean((PolyhedronR.Mode)(id - 2050)); //var t2 = Environment.TickCount; Application.OpenForms[0].Text = $"{t2 - t1} ms";
        var ras = Models.BoolGeometry.remap(mb.Indices, mb.Mapping, a, b);
        var c = new Models.MeshGeometry
        {
          Name = a.Name,
          Transform = a.Transform,
          rpts = mb.Vertices.ToArray(),
          indices = mb.Indices.Select(p => (ushort)p).ToArray(),
          ranges = ras
        };
        var xx = a.Parent.Nodes.Select(p => p == a ? c : p == b ? null : p).OfType<Node>().ToArray();
        Execute(undosel(a), undonodes(a.Parent, xx), undosel(c));
      }
      Cursor.Current = Cursors.Arrow;
      return 0;
    }
    int OnConvert(object? test)
    {
      if (selection.Count != 1) return 0;
      var oldnode = selection[0]; var tn = oldnode.GetType(); var cc = oldnode.Nodes ?? Array.Empty<Node>();
      if (test is ToolStripMenuItem item)
      {
        var nodetypes = typeof(Models).GetNestedTypes().Where(t => canconv(oldnode, t));
        var items = item.DropDownItems;
        foreach (var p in nodetypes) items.Add(new ToolStripMenuItem(p.Name) { Tag = p, Checked = p == tn });
        return 0;
      }
      if (test is not Type t || t == tn) return 0;
      var newnode = (Node)Activator.CreateInstance(t);
      newnode.InitNew(oldnode);
      Execute(
        undonodes(oldnode.Parent, oldnode.Parent.Nodes.Select(p => p == oldnode ? newnode : p).ToArray()),
        undosel(newnode));
      static bool canconv(Node from, Type to)
      {
        if (to.IsAbstract || !(to == typeof(Node) || to.IsSubclassOf(typeof(Node)))) return false;
        if (to == typeof(Models.MeshGeometry)) { if (from is not Models.Geometry) return false; }
        else if (to == typeof(Models.BoolGeometry))
        {
          var cc = from.Nodes; if (cc == null || cc.Length != 2) return false;
          if (cc[0] is not Models.Geometry && cc[1] is not Models.Geometry) return false;
        }
        return true;
      }
      return 0;
    }

    bool IsSelect(Node node) => selection.Contains(node);
    public void Select(object? p, bool toggle = false)
    {
      if (p == null) { if (selection.Count != 0) { selection.Clear(); Invalidate(); } return; }
      if (p is Node node)
      {
        if (!toggle)
        {
          if (selection.Count == 1 && selection[0] == node) return;
          selection.Clear(); selection.Add(node);
        }
        else
        {
          if (selection.Contains(node)) selection.Remove(node);
          else
          {
            if (selection.Count != 0 && selection[0].Parent != node.Parent) selection.Clear();
            selection.Add(node);
          }
        }
        Invalidate(); return;
      }
      if (p is IEnumerable e)
      {
        if (e.OfType<Node>().SequenceEqual(selection)) return;
        selection.Clear(); selection.AddRange(e.OfType<Node>());
        Invalidate(); return;
      }
    }
    public void AddUndo(Action? p)
    {
      if (p == null) return;
      if (undos == null) undos = new List<Action>();
      undos.RemoveRange(undoi, undos.Count - undoi);
      undos.Add(p); undoi = undos.Count;
    }
    public void Execute(Action a)
    {
      if (a == null) return;
      a(); AddUndo(a); Invalidate();
    }
    public void Execute(params object[] a)
    {
      var b = undo(a); if (b == null) return;
      b(); AddUndo(b); Invalidate();
    }
    [Browsable(false)]
    public bool IsModified
    {
      get => undoi != 0;
      set { undos = null; undoi = 0; }
    }
    Action undosel(params Node[]? a)
    {
      return () => { var t = selection.Count != 0 ? selection.ToArray() : null; Select(a); a = t; };
    }
    Action undonodes(Models.Base p, Node[]? nodes)
    {
      return () => { var t = p.Nodes; p.Nodes = nodes; nodes = t; };
    }
    static Action? undotrans(Node p, Matrix4x3 m)
    {
      if (p.Transform == m) return null;
      return () => { var t = p.Transform; p.Transform = m; m = t; };
    }
    static Action? undo(object? p)
    {
      if (p is Action ac) return ac;
      if (p is Action<int, Matrix4x3> em) return () => em(0, default);
      if (p is IEnumerable<Action> en) return undo(en.ToArray());
      return null;
    }
    static Action? undo(params object[] a)
    {
      var b = a.SelectMany(p => p is IEnumerable<Action> e ? e : Enumerable.Repeat(undo(p), 1)).OfType<Action>().ToArray();
      if (b.Length == 0) return null;
      if (b.Length == 1) return b[0];
      return () => { for (int i = 0; i < b.Length; i++) b[i](); Array.Reverse(b); };
    }

    static int lastwheel;
    void wheel(PC pc)
    {
      if (pc.Hover is not Node) return;
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var m = camera.Transform;
      var v = wp - m.Translation;
      var l = v.Length(); var f = pc.Primitive / 120f;
      camera.Transform = m * Matrix4x3.CreateTranslation(v * (l * f * 0.02f)); pc.Invalidate();
      var t = Environment.TickCount; if (t - lastwheel > 500) { lastwheel = t; AddUndo(undotrans(camera, m)); }
    }
    Action<int> tool_cam_move(PC pc)
    {
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var wm = Matrix4x3.CreateTranslation(wp);
      pc.SetPlane(wm);
      var p1 = pc.Pick(); var m = camera.Transform;
      return id =>
      {
        if (id == 0)
        {
          var dp = p1 - pc.Pick();
          camera.Transform = m * Matrix4x3.CreateTranslation(dp.X, dp.Y, 0);
          pc.Invalidate();
        }
        if (id == 1) AddUndo(undotrans(camera, m));// undo(main, om));
      };
    }
    Action<int> tool_cam_vert(PC pc)
    {
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var wm = Matrix4x3.CreateTranslation(wp);
      var cm = camera.Transform;
      *(Vector3*)&wm.M31 = Vector3.Normalize(cm.Translation - wp);
      *(Vector3*)&wm.M21 = new Vector3(0, 0, 1);
      *(Vector3*)&wm.M11 = Vector3.Cross(*(Vector3*)&wm.M21, *(Vector3*)&wm.M31);
      pc.SetPlane(wm);
      var p1 = pc.Pick();
      return id =>
      {
        if (id == 0)
        {
          var dz = p1.Y - pc.Pick().Y;
          camera.Transform = cm * Matrix4x3.CreateTranslation(0, 0, dz); pc.Invalidate();
        }
        if (id == 1) AddUndo(undotrans(camera, cm));
      };
    }
    Action<int> tool_cam_decl(PC pc)
    {
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var cm = camera.Transform;
      var rot = cm.Translation;
      pc.SetPlane(Matrix4x3.CreateTranslation(rot.X, rot.Y, wp.Z));
      var p1 = pc.Pick(); var p2 = p1; var a1 = MathF.Atan2(p1.Y, p1.X);
      return id =>
      {
        if (id == 0)
        {
          var p2 = pc.Pick(); var a2 = MathF.Atan2(p2.Y, p2.X);
          camera.Transform = cm *
            Matrix4x3.CreateTranslation(-rot) *
            Matrix4x3.CreateRotationZ(a1 - a2) *
            Matrix4x3.CreateTranslation(rot);
          pc.Invalidate();
        }
        if (id == 1) AddUndo(undotrans(camera, cm));
      };
    }
    Action<int> tool_cam_incl(PC pc)
    {
      var cm = camera.Transform; var om = camera;
      var mp = cm * pc.Plane; mp.M41 = mp.M31; mp.M42 = mp.M32; mp.M44 = mp.M34; pc.Plane = mp;
      var a1 = MathF.Atan(pc.Pick().Y);
      return id =>
      {
        if (id == 0)
        {
          camera.Transform = Matrix4x3.CreateRotationX(MathF.Atan(pc.Pick().Y) - a1) * cm;
          pc.Invalidate();
        }
        if (id == 1) AddUndo(undotrans(camera, cm));
      };
    }
    Action<int> tool_cam_select(PC pc)
    {
      ////setplane(dc, 0); wp?
      var wp = Vector3.Transform(pc.Point, pc.Transform); //Debug.WriteLine(wp.Z);
      var ma = Matrix4x4.CreateTranslation(0, 0, wp.Z); pc.SetPlane(ma);
      var p1 = pc.Pick(); var p2 = p1;
      //var ca = pc.ViewPort.Camera;
      RenderClient += draw; return tool;
      void tool(int id)
      {
        if (id == 0) { p2 = pc.Pick(); pc.Invalidate(); }
        if (id == 1)
        {
          RenderClient -= draw; pc.Invalidate();
          if (p1 == p2) { Select(null); return; }
          var rect = (Min: Vector2.Min(p1, p2), Max: Vector2.Max(p1, p2));
          var list = new List<Node>();
          for (int i = 0; i < scene.Nodes.Length; i++)
          {
            var node = scene.Nodes[i]; if (node.Fixed) continue;
            if (intersect(node, rect)) list.Add(node);
            var box = node.GetBox(); if (!Intersect(box, rect)) continue;
            static bool intersect(Node node, (Vector2 Min, Vector2 Max) rect)
            {
              var a = node.Nodes; if (a != null) for (int i = 0; i < a.Length; i++) if (intersect(a[i], rect)) return true;
              var g = node as Models.Geometry; if (g == null) return false;
              var m = node.GetTransform(); var pp = g.vertices; var ii = g.indices;
              for (int i = 0; i < ii.Length; i += 3)
              {
                var p1 = Vector3.Transform(pp[ii[i + 0]], m);
                var p2 = Vector3.Transform(pp[ii[i + 1]], m);
                var p3 = Vector3.Transform(pp[ii[i + 2]], m);
                if (Intersect((*(Vector2*)&p1, *(Vector2*)&p2, *(Vector2*)&p3), rect)) return true;
              }
              return false;
            }
          }
          Select(list);
        }
      }
      void draw(DC dc)
      {
        dc.Transform = ma; var dp = p2 - p1;
        dc.Color = 0x808080ff; dc.FillRect(p1.X, p1.Y, dp.X, dp.Y);
        dc.Color = 0xff8080ff; dc.DrawRect(p1.X, p1.Y, dp.X, dp.Y);
      }
    }

    static Action<int, Matrix4x3> move(Node node)
    {
      var m = node.Transform;
      return (id, v) =>
      {
        if (id == 0) { v = node.Transform; node.Transform = m; m = v; node.Parent?.Invalidate(); }
        else
        {
          var t = m * v;
          if (v.M41 != 0) t.M41 = MathF.Round(t.M41, 6);
          if (v.M42 != 0) t.M42 = MathF.Round(t.M42, 6);
          if (v.M43 != 0) t.M43 = MathF.Round(t.M43, 6);
          node.Transform = t; node.Parent?.Invalidate();
        }
      };
    }
    static Action<int, Matrix4x3> move(IEnumerable<Node> nodes)
    {
      var a = nodes.Select(p => move(p)).ToArray();
      return a.Length == 1 ? a[0] : (id, m) => { for (int i = 0; i < a.Length; i++) a[i](id, m); };
    }
    Action<int> tool_obj_move(PC pc, Node main)
    {
      var ws = IsSelect(main); if (!ws) Select(main);
      var rp = main.Location;
      var mo = default(Action<int, Matrix4x3>);
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var wm = Matrix4x3.CreateTranslation(wp);
      if (main.Parent is Node pn) { var t = pn.GetTransform(); t.M41 = t.M42 = t.M43 = 0; wm = t * wm; }
      //if (main.Parent is Node pn) wm = Matrix4x3.CreateTranslation(Vector3.Transform(wp, !pn.GetTransform())) * pn.GetTransform();
      //var xm = main.Parent is Node pn ? pn.GetTransform() : Matrix4x3.Identity;
      pc.SetPlane(wm); Vector2 p1 = pc.Pick(), p2 = p1;
      //RenderClient += drawplane; Invalidate();
      return id =>
      {
        if (id == 0)
        {
          var dp = (p2 = pc.Pick()) - p1; //Debug.WriteLine($"{dp}");
          if (mo == null && p1 == p2) return;
          var r = settings.Raster;
          if (r != 0)
          {
            dp.X = MathF.Round((rp.X + dp.X) / r) * r - rp.X;
            dp.Y = MathF.Round((rp.Y + dp.Y) / r) * r - rp.Y;
          }
          if (dp == default) return; //Debug.WriteLine($"{dp}");
          if (mo == null) mo = move(selection);
          mo(1, Matrix4x3.CreateTranslation(new Vector3(dp, 0)));
          //mo(1, xm * Matrix4x3.CreateTranslation(new Vector3(dp, 0)) * !xm);
        }
        if (id == 1)
        {
          //RenderClient -= drawplane; Invalidate();
          if (mo == null)
          {
            if (ws)
            {
              //if (selection.Count == 1 && pc.Hover != main)
              //{
              //  var t = pc.Hover as Node; for (; t != null && t.Parent != main; t = t.Parent as Node) ;
              //  if (t != null && !t.Fixed) main = t;
              //}
              Select(main);
            }
          }
          else if (main.Location != rp) AddUndo(undo(mo));
        }
      };
      //void drawplane(DC dc)
      //{
      //  Matrix4x4.Invert(dc.Projection, out var ip);
      //  dc.Transform = pc.Plane * ip;
      //  dc.Color = 0x80ffffff; dc.FillRect(0, 0, 3, 3);
      //  dc.Color = 0x80ff0000; dc.DrawLine(default, new Vector3(3, 0, 0));
      //  dc.Color = 0x8000ff00; dc.DrawLine(default, new Vector3(0, 3, 0));
      //}
    }
    Action<int> tool_obj_vert(PC pc, Node main)
    {
      if (!IsSelect(main)) Select(main);
      var box = (Min: new Vector3(float.MaxValue), Max: new Vector3(-float.MaxValue));
      foreach (var p in selection) p.GetBox(ref box, p.Transform);
      var mo = default(Action<int, Matrix4x3>);
      var rp = main.Location;
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var wm = Matrix4x3.CreateTranslation(wp);
      var cm = camera.GetTransform();
      var up = new Vector3(0, 0, 1);
      if (main.Parent is Node pn) up = Vector3.Normalize(pn.GetTransform()[2]);
      wm[0] = Vector3.Cross(wm[1] = up, wm[2] = Vector3.Normalize(cm.Translation - wp));
      pc.SetPlane(wm);
      var p1 = pc.Pick(); float dz = 0, bz = box.Min.Z;
      //RenderClient += drawplane; Invalidate();
      return id =>
      {
        if (id == 0)
        {
          var lastz = bz + dz; dz = pc.Pick().Y - p1.Y;
          if (mo == null && dz == 0) return;
          var r = settings.Raster;
          if (r != 0) dz = MathF.Round((rp.Z + dz) / r) * r - rp.Z;
          if (lastz >= 0 && bz + dz < 0 && bz + dz > -0.25f) dz = -bz;
          if (dz == 0) return;
          if (mo == null) mo = move(selection);
          mo(1, Matrix4x3.CreateTranslation(new Vector3(0, 0, dz))); //pc.Invalidate();
        }
        if (id == 1)
        {
          //RenderClient -= drawplane; Invalidate();
          if (main.Location != rp) AddUndo(undo(mo));
        }
      };
      //void drawplane(DC dc)
      //{
      //  Matrix4x4.Invert(dc.Projection, out var ip);
      //  dc.Transform = pc.Plane * ip;
      //  dc.Color = 0x80ffffff; dc.FillRect(0, 0, 3, 3);
      //  dc.Color = 0x80ff0000; dc.DrawLine(default, new Vector3(3, 0, 0));
      //  dc.Color = 0x8000ff00; dc.DrawLine(default, new Vector3(0, 3, 0));
      //}
    }
    Action<int> tool_obj_rot(PC pc, Node main)
    {
      var ws = IsSelect(main); if (!ws) Select(main);
      var mo = default(Action<int, Matrix4x3>);
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var mw = main.Transform;// GetTransform();
      var mp = mw.Translation;
      var vm = Math.Abs(mw.M13) > 0.8f ? new Vector2(mw.M21, mw.M22) : new Vector2(mw.M11, mw.M12);
      var w0 = MathF.Atan2(vm.Y, vm.X);
      var ag = angelgrid((MathF.PI / 180) * settings.Angelgrid);
      var me = Matrix4x3.CreateTranslation(mp.X, mp.Y, wp.Z);
      if (main.Parent is Node pn) { me = pn.GetTransform(); me = Matrix4x3.CreateTranslation(mp.X, mp.Y, Vector3.Transform(wp, !me).Z) * me; }
      pc.SetPlane(me);
      var p1 = pc.Pick(); float a1 = MathF.Atan2(p1.Y, p1.X), rw = 0;
      //RenderClient += drawplane; Invalidate();
      return id =>
      {
        if (id == 0)
        {
          var p2 = pc.Pick(); var a2 = MathF.Atan2(p2.Y, p2.X);
          rw = ag(w0 + a2 - a1) - w0;
          if (mo == null) { if (rw == 0) return; mo = move(selection); }
          mo(1, Matrix4x3.CreateTranslation(-mp) * Matrix4x3.CreateRotationZ(rw) * mp);
        }
        if (id == 1)
        {
          //RenderClient -= drawplane; Invalidate();
          if (mo == null)
          {
            if (ws && selection.Count == 1 && pc.Hover != main)
            {
              var t = pc.Hover as Node; for (; t != null && t.Parent != main; t = t.Parent as Node) ;
              if (t != null && !t.Fixed) Select(t);
            }
          }
          else if (rw != 0) AddUndo(undo(mo));
        }
      };
      //void drawplane(DC dc)
      //{
      //  Matrix4x4.Invert(dc.Projection, out var ip);
      //  dc.Transform = pc.Plane * ip;
      //  dc.Color = 0x80ffffff; dc.FillRect(0, 0, 3, 3);
      //  dc.Color = 0x80ff0000; dc.DrawLine(default, new Vector3(3, 0, 0));
      //  dc.Color = 0x8000ff00; dc.DrawLine(default, new Vector3(0, 3, 0));
      //}
    }
    Action<int> tool_obj_rot_axis(PC pc, Node main, int axis)
    {
      if (!IsSelect(main)) Select(main);
      var mo = default(Action<int, Matrix4x3>);
      var mm = main.Transform;// GetTransform();
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var op = Vector3.Transform(wp, !main.GetTransform());
      var mr = (
        axis == 0 ? Matrix4x3.CreateTranslation(0, 0, op.X) * Matrix4x3.CreateRotationY(+MathF.PI / 2) :
        axis == 1 ? Matrix4x3.CreateTranslation(0, 0, op.Y) * Matrix4x3.CreateRotationX(-MathF.PI / 2) :
        Matrix4x3.CreateTranslation(0, 0, op.Z)) * mm;
      var w0 = Math.Abs(mr.M33) > 0.9f ? MathF.Atan2(mr.M12, mr.M22) : MathF.Atan2(mr.M13, mr.M23);
      var ag = angelgrid((MathF.PI / 180) * settings.Angelgrid);
      var me = Matrix4x3.CreateRotationZ(-w0) * mr;
      if (main.Parent is Node pn) { var t = pn.GetTransform(); me = me * t; }
      pc.SetPlane(me);
      var p1 = pc.Pick(); float a1 = MathF.Atan2(p1.Y, p1.X), rw = 0;
      //RenderClient += drawplane; Invalidate();
      return id =>
      {
        if (id == 0)
        {
          var p2 = pc.Pick(); var a2 = MathF.Atan2(p2.Y, p2.X);
          rw = ag(w0 + a2 - a1) - w0;
          if (mo == null) { if (rw == 0) return; mo = move(selection); }
          mo(1, !mm * (
            axis == 0 ? Matrix4x3.CreateRotationX(rw) :
            axis == 1 ? Matrix4x3.CreateRotationY(rw) :
            Matrix4x3.CreateRotationZ(rw)) * mm); //pc.Invalidate();
        }
        if (id == 1)
        {
          //RenderClient -= drawplane; Invalidate();
          if (rw != 0) AddUndo(undo(mo));
        }
      };
      //void drawplane(DC dc)
      //{
      //  Matrix4x4.Invert(dc.Projection, out var ip);
      //  dc.Transform = pc.Plane * ip;
      //  dc.Color = 0x80ffffff; dc.FillRect(0, 0, 3, 3);
      //  dc.Color = 0x80ff0000; dc.DrawLine(default, new Vector3(3, 0, 0));
      //  dc.Color = 0x8000ff00; dc.DrawLine(default, new Vector3(0, 3, 0));
      //}
    }
    Action<int> tool_obj_drag(PC pc, Node main)
    {
      var ws = IsSelect(main); if (!ws) Select(main, true);
      var pt = Vector3.Transform(pc.Point, ((Node)pc.Hover).GetTransform(main.Parent));
      //var pt = Vector3.Transform(pc.Point, pc.Transform);
      var p1 = Cursor.Position;
      return id =>
      {
        if (id == 0)
        {
          var p2 = Cursor.Position;
          if (new Vector2(p2.X - p1.X, p2.Y - p1.Y).LengthSquared() < 10) return;
          if (!ws) Select(main); ws = false; if (!AllowDrop) return;
          var name = main.Name; if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) name = main.GetType().Name;
          var path = Path.Combine(Path.GetTempPath(), name + ".x³");
          var xml = Models.Save(new Models.Scene { Unit = scene.Unit, Nodes = selection.ToArray() });
          var ppt = pt; xml.SetAttributeValue("pt", Models.format(new Span<float>(&ppt, 3)));
          var data = new DataObject();
          data.SetFileDropList(new System.Collections.Specialized.StringCollection { path });
          try { xml.Save(path); DoDragDrop(data, DragDropEffects.Copy); }
          catch (Exception e) { Debug.WriteLine(e.Message); }
          finally { File.Delete(path); }
        }
        if (id == 1 && ws) { Select(main, true); }
      };
    }
    Action<int>? tool_drop(PC pc)
    {
      var data = pc.View.Tag as DataObject; if (data == null || this.scene.Nodes == null) return null;
      var list = data.GetFileDropList(); if (list == null || list.Count != 1) return null;
      var path = list[0]; if (path == null) return null;
      var ext = Path.GetExtension(path).ToLower(); if (ext != ".x³") return null;
      var xml = XElement.Load(path);
      var pt = default(Vector3); var s = (string)xml.Attribute("pt");
      if (s != null) Models.parse(s.AsSpan().Trim(), new Span<float>(&pt, 3));
      var nodes = Models.Load(xml).Nodes; var rp = nodes[nodes.Length - 1].Location;
      var mover = move(nodes); var oldnodes = this.scene.Nodes;
      this.scene.Nodes = this.scene.Nodes.Concat(nodes).ToArray();
      pc.SetPlane(Matrix4x3.CreateTranslation(pt)); var plane = pc.Plane;
      return id =>
      {
        if (id == 0)
        {
          pc.Plane = plane; var dp = pc.Pick();
          var r = settings.Raster;
          if (r != 0)
          {
            dp.X = MathF.Round((rp.X + dp.X) / r) * r - rp.X;
            dp.Y = MathF.Round((rp.Y + dp.Y) / r) * r - rp.Y;
          }
          mover(1, Matrix4x3.CreateTranslation(dp.X, dp.Y, 0)); //pc.Invalidate();
        }
        else if (id == 1)
        {
          var sel = undosel(nodes); sel(); pc.Invalidate();
          AddUndo(undo(sel, undonodes(this.scene, oldnodes)));
        }
        else if (id == 2) { this.scene.Nodes = oldnodes; pc.Invalidate(); }
      };
    }

    static Func<float, float> angelgrid(float grad) //(MathF.PI / 180) 1°
    {
      var seg1 = 0.0f; var hang = 0; var count = 0; var len = MathF.PI / 4;
      return val =>
      {
        var seg2 = MathF.Floor(val / len); var len2 = len * 0.33f;
        if (0 == count++) seg1 = seg2;
        if (seg2 == seg1) { hang = 0; if (grad != 0) val = MathF.Round(val / grad) * grad; return val; }
        if (Math.Abs(seg2 * len - val) < len2) { if (hang != 1) { hang = 1; /*snd*/} return seg2 * len; }
        var d = seg1 * len - val; d = d % MathF.Tau;
        if (Math.Abs(d) < len2) { val = seg1 * len; if (hang != 2) { hang = 2; /*snd*/} } else { seg1 = seg2; hang = 0; }
        return val;
      };
    }
    static bool Intersect((Vector3 Min, Vector3 Max) box, (Vector2 Min, Vector2 Max) rect)
    {
      if (box.Max.X < rect.Min.X) return false;
      if (box.Max.Y < rect.Min.Y) return false;
      if (box.Min.X > rect.Max.X) return false;
      if (box.Min.Y > rect.Max.Y) return false;
      return true;
    }
    static bool Intersect((Vector2 a, Vector2 b, Vector2 c) t, (Vector2 Min, Vector2 Max) box)
    {
      var min = Vector2.Min(Vector2.Min(t.a, t.b), t.c); if (min.X > box.Max.X || min.Y > box.Max.Y) return false;
      var max = Vector2.Max(Vector2.Max(t.a, t.b), t.c); if (max.X < box.Min.X || max.Y < box.Min.Y) return false;
      if (test(t.a, t.b, box)) return true;
      if (test(t.b, t.c, box)) return true;
      if (test(t.c, t.a, box)) return true;
      if (PtInPoly(t, box.Min)) return true;
      static bool test(Vector2 p1, Vector2 p2, (Vector2 Min, Vector2 Max) box)
      {
        var min = Vector2.Min(p1, p2); if (min.X > box.Max.X || min.Y > box.Max.Y) return false;
        var max = Vector2.Max(p1, p2); if (max.X < box.Min.X || max.Y < box.Min.Y) return false;
        if (min.Y == max.Y) return true;
        var v = p2 - p1; var f = v.X / v.Y;
        var x2 = p1.X + (MathF.Min(max.Y, box.Max.Y) - p1.Y) * f;
        var x1 = p1.X + (MathF.Max(min.Y, box.Min.Y) - p1.Y) * f;
        if (x1 < box.Min.X && x2 < box.Min.X) return false;
        if (x1 > box.Max.X && x2 > box.Max.X) return false;
        return true;
      }
      return false;
    }
    static bool PtInPoly((Vector2 a, Vector2 b, Vector2 c) t, Vector2 p)
    {
      var a = t.b - t.a; var aa = Vector2.Dot(a, a);
      var b = t.c - t.a; var ab = Vector2.Dot(a, b); var bb = Vector2.Dot(b, b);
      var d = (aa * bb - ab * ab); if (d == 0) return false; d = 1 / d;
      var c = p - t.a; var ac = Vector2.Dot(a, c); var bc = Vector2.Dot(b, c);
      var u = (bb * ac - ab * bc) * d; if (u < 0) return false;
      var v = (aa * bc - ab * ac) * d; if (v < 0 || u + v > 1) return false;
      return true;
    }

    public static class Models
    {
      static readonly XNamespace ns = "file://C:/Users/cohle/Desktop/Mini3d";
      public static XElement Save(Base node)
      {
        var e = new XElement(ns + node.GetType().Name);
        node.Serialize(e, true);
        return e;
      }
      public static Base Load(XElement e)
      {
        var node = (Base)Activator.CreateInstance(typeof(Models).GetNestedType(e.Name.LocalName));
        node.Serialize(e, false);
        return node;
      }
      internal static string format(ReadOnlySpan<float> sp)
      {
        return DX11Ctrl.ToString(sp); //, "G9");
      }
      internal static string format(ReadOnlySpan<ushort> sp)
      {
        return DX11Ctrl.ToString(sp);
      }
      internal static void parse(ReadOnlySpan<char> s, Span<float> a)
      {
        var fmt = NumberFormatInfo.InvariantInfo;
        for (int i = 0, n = a.Length; i < n; i++) a[i] = float.Parse(token(ref s), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, fmt);
      }
      internal static void parse(ReadOnlySpan<char> s, Span<ushort> a)
      {
        var fmt = NumberFormatInfo.InvariantInfo;
        for (int i = 0, n = a.Length; i < n; i++) a[i] = ushort.Parse(token(ref s), NumberStyles.Integer, fmt);
      }
      static ReadOnlySpan<char> token(ref ReadOnlySpan<char> a)
      {
        int i = 0; for (; i < a.Length && !(char.IsWhiteSpace(a[i]) || a[i] == ';'); i++) ;
        var w = a.Slice(0, i); a = a.Slice(i < a.Length ? i + 1 : i).TrimStart(); return w;
      }
      static int tokencount(ReadOnlySpan<char> a)
      {
        var n = 0; for (var t = a; t.Length != 0; token(ref t), n++) ; return n;
      }
      static Vector2 sincos(float a)
      {
        var c = MathF.Cos(a);
        var s = MathF.Sin(a);
        return new Vector2(c, s);
      }
      internal static bool propref;

      const string _General = "\t\tGeneral";
      const string _Transform = "\tTransform";

      public class Settings
      {
        float raster = 0.001f, angelgrid = 0.01f;
        [Category("Tools")]
        public float Raster
        {
          get => raster;
          set { raster = value; }
        }
        [Category("Tools")]
        public float Angelgrid
        {
          get => angelgrid;
          set { angelgrid = value; }
        }
      }

      public abstract class Base
      {
        internal protected int flags; //0x01: Fixed 0x04:buildok 0x08:vbok 0x10:merge
        string? name;
        public Node[]? Nodes;
        public Base? Parent;
        public string? Name
        {
          get => name;
          set
          {
            if (value != null && (value = value.Trim()).Length == 0) value = null;
            name = value;
          }
        }
        internal protected virtual unsafe void Serialize(XElement e, bool storing)
        {
          if (storing)
          {
            if (name != null) e.SetAttributeValue("name", name);
            if (Nodes != null)
              for (int i = 0; i < Nodes.Length; i++)
              {
                var p = Nodes[i]; var c = new XElement(ns + p.GetType().Name);
                e.Add(c); p.Serialize(c, true);
              }
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("name")) != null) Name = a.Value;
            var n = 0;
            for (var p = e.FirstNode; p != null; p = p.NextNode)
            {
              if (p is not XElement pe) continue;
              var t = typeof(Models).GetNestedType(pe.Name.LocalName);
              if (t == null || !t.IsSubclassOf(typeof(Base))) continue;
              pe.AddAnnotation(t); n++;
            }
            if (n != 0)
            {
              Nodes = new Node[n]; n = 0;
              for (var p = e.FirstNode; p != null; p = p.NextNode)
              {
                if (p is not XElement pe) continue;
                var t = pe.Annotation<Type>(); if (t == null) continue;
                (Nodes[n++] = (Node)Activator.CreateInstance(t)).Serialize(pe, false);
              }
            }
          }
        }
        internal protected virtual void Invalidate() => Parent?.Invalidate();
        internal protected Node[]? VisibleNodes
        {
          get => Nodes != null && this is not BoolGeometry ? Nodes : default;
          //get => (flags & 0x02) == 0;
          //set => flags = (flags & ~0x02) | (value ? 0 : 0x02);
        }
        internal protected virtual object? GetService(Type t)
        {
          return Parent != null ? Parent.GetService(t) : null;
        }
      }

      public class Scene : Base
      {
        internal DX11Ctrl? root;
        public enum Units { Meter = 1, Centimeter = 2, Millimeter = 3, Micron = 4, Foot = 5, Inch = 6 }
        Units unit; internal uint ambient;
        [Category(_General)]
        public new string? Name { get => base.Name; set => base.Name = value; }
        [Category("Scene")]
        public Units Unit
        {
          get => unit;
          set => unit = value;
        }
        [Category("Scene")]
        public Color Ambient
        {
          get => Color.FromArgb(unchecked((int)ambient));
          set { ambient = unchecked((uint)value.ToArgb()); }
        }
        protected internal override void Serialize(XElement e, bool storing)
        {
          if (storing)
          {
            if (unit != 0) e.SetAttributeValue("unit", unit);
            if (ambient != 0) e.SetAttributeValue("ambient", ambient.ToString("X8"));
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("unit")) != null) unit = Enum.Parse<Units>(a.Value);
            if ((a = e.Attribute("ambient")) != null) ambient = uint.Parse(a.Value, NumberStyles.HexNumber);
          }
          base.Serialize(e, storing);
        }
        protected internal override void Invalidate() { root?.Invalidate(); }
        protected internal override object? GetService(Type t)
        {
          if (t == typeof(Scene)) return this;
          if (t == typeof(DX11ModelCtrl)) return root;
          return base.GetService(t);
        }
      }

      public class Node : Base
      {
        [Category(_General)]
        public new string? Name { get => base.Name; set => base.Name = value; }
#if false
      [Category(_General), TypeConverter(typeof(TTC))]
      public Type Type
      {
        get => GetType();
        set { }
      }
      class TTC : TypeConverter
      {
        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
        {
          return value is Type t ? t.Name : String.Empty;
        }
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => true;
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
          if(value is not string s) return null;
          return typeof(ObjectModel).GetNestedType(s);
        }
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;
        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
          var a = typeof(ObjectModel).GetNestedTypes().Where(p => !p.IsAbstract && p.IsAssignableTo(typeof(Node))).ToArray();
          return new StandardValuesCollection(a);
        }
      }
#endif
        [Category(_General)]
        public bool Fixed
        {
          get => (flags & 0x01) != 0;
          set => flags = (flags & ~0x01) | (value ? 0x01 : 0);
        }
        [Category(_Transform)]
        public Vector3 Location
        {
          get => Transform.Translation;
          set { Transform.Translation = value; Parent?.Invalidate(); }
        }
        [Category(_Transform)]
        public Vector3 Rotation
        {
          get => Transform.Rotation;
          set { Transform.Rotation = value; Parent?.Invalidate(); }
        }
        [Category(_Transform)]
        public Vector3 Scaling
        {
          get => Transform.Scaling;
          set { Transform.Scaling = value; Parent?.Invalidate(); }
        }
        public Matrix4x3 Transform;
        public Matrix4x3 GetTransform(Base? root = null)
        {
          if (root == this) return Matrix4x3.Identity;
          if (root == Parent || Parent is not Node p) return Transform;
          return Transform * p.GetTransform(root);
        }
        public void GetBox(ref (Vector3 Min, Vector3 Max) box, in Matrix4x3? pm = default)
        {
          var a = VisibleNodes;
          if (a != null)
            for (int i = 0; i < a.Length; i++)
            {
              var p = Nodes[i]; p.GetBox(ref box, pm.HasValue ? p.Transform * pm.Value : p.Transform);
            }
          if (this is not Models.Geometry geo) return;
          var pp = geo.vertices; if (pp == null) return;
          for (int i = 0; i < pp.Length; i++)
          {
            var p = pp[i]; if (pm.HasValue) p = Vector3.Transform(p, pm.Value);
            box.Min = Vector3.Min(box.Min, p);
            box.Max = Vector3.Max(box.Max, p);
          }
        }
        public (Vector3 Min, Vector3 Max) GetBox(in Matrix4x3? pm = default)
        {
          var box = (Min: new Vector3(float.MaxValue), Max: new Vector3(-float.MaxValue));
          GetBox(ref box, pm); return box;
        }
        protected internal override unsafe void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            var m = Transform;
            if (!m.IsIdentity) e.SetAttributeValue("transform", format(new ReadOnlySpan<float>(&m, 12)));
            if (Fixed) e.SetAttributeValue("fixed", true);
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("transform")) != null)
            {
              var m = default(Matrix4x3);
              parse(a.Value.AsSpan().Trim(), new Span<float>(&m, 12)); Transform = m;
            }
            else Transform = Matrix4x3.Identity;
            if ((a = e.Attribute("fixed")) != null) Fixed = (bool)a;
          }
        }
        internal protected virtual void InitNew(Node? from)
        {
          if (from != null) { Name = from.Name; Transform = from.Transform; Nodes = from.Nodes; }
          else Transform = Matrix4x3.Identity;
        }
      }

      public class Camera : Node
      {
        [Category("Camera")]
        public float Fov { get; set; }
        [Category("Camera")]
        public float Near { get; set; }
        [Category("Camera")]
        public float Far { get; set; }
        protected internal override void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            e.SetAttributeValue("fov", Fov);
            e.SetAttributeValue("near", Near);
            e.SetAttributeValue("far", Far);
          }
          else
          {
            XAttribute p;
            if ((p = e.Attribute("fov")) != null) Fov = (float)p; else Fov = 50;
            if ((p = e.Attribute("near")) != null) Near = (float)p; else Near = 0.1f;
            if ((p = e.Attribute("far")) != null) Far = (float)p; else Far = 1000;
          }
        }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from);
          Fov = 50; Near = 0.1f; Far = 1000;
        }
      }

      public class Light : Node
      {
        protected internal override void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
        }
      }

      public class Material : IEquatable<Material>
      {
        public uint Diffuse;
        public DX11Ctrl.Texture? Texture;
        public unsafe Matrix4x3 Transform
        {
          get
          {
            if (trans == null) return Matrix4x3.Identity;
            fixed (float* p = trans) return *(Matrix4x3*)p;
          }
          set
          {
            if (value.IsIdentity) { trans = null; return; }
            fixed (float* p = trans ??= new float[12]) *(Matrix4x3*)p = value;
          }
        }
        float[]? trans;
        public override int GetHashCode()
        {
          return HashCode.Combine(Diffuse, Texture, Transform);
        }
        public bool Equals(Material? b)
        {
          return Diffuse == b.Diffuse && Texture == b.Texture && Transform == b.Transform;
        }
        internal Material Clone(in Matrix4x3 m)
        {
          var p = new Material { Diffuse = Diffuse, Texture = Texture };
          if (p.Texture != null) p.Transform = m * Transform; return p;
        }
      }

      [TypeConverter(typeof(GTC))]
      public abstract class Geometry : Node
      {
        int current;
        [Category("Material"), TypeConverter(typeof(MTC)), DefaultValue(0)]
        public int Current
        {
          get => current < ranges.Length ? current : 0;
          set { current = value; propref = true; }
        }
        [Category("Material")]
        public Color ColorDiffuse
        {
          get => Color.FromArgb(unchecked((int)ranges[Current].material.Diffuse));
          set { ranges[Current].material.Diffuse = unchecked((uint)value.ToArgb()); }
        }
        [Category("Material")]
        public string? Texture
        {
          get { var m = ranges[Current].material; return m.Texture != null ? m.Texture.Url : null; }
          set
          {
            if (value != null && (value = value.Trim()).Length == 0) value = null;
            var m = ranges[Current].material.Texture = value != null ? DX11Ctrl.GetTexture(value) : null; propref = true;
          }
        }
        [Category("Material")]
        public Vector3 TextureScaling
        {
          get => ranges[Current].material.Transform.Scaling;
          set
          {
            var m = ranges[Current].material.Transform; m.Scaling = value;
            ranges[Current].material.Transform = m;
          }
        }
        [Category("Material")]
        public Vector3 TextureRotation
        {
          get => ranges[Current].material.Transform.Rotation;
          set
          {
            var m = ranges[0].material.Transform; m.Rotation = value;
            ranges[0].material.Transform = m;
          }
        }
        [Category("Material")]
        public Vector2 TextureOffset
        {
          get { var p = ranges[Current].material.Transform.Translation; return new Vector2(p.X, p.Y); }
          set
          {
            var m = ranges[Current].material.Transform; m.Translation = new Vector3(value, 0);
            ranges[Current].material.Transform = m;
          }
        }

        internal protected Vector3[]? vertices;
        internal protected ushort[]? indices;
        internal (int count, Material material)[]? ranges;
        internal DX11Ctrl.VertexBuffer? vb;
        internal DX11Ctrl.IndexBuffer? ib;
        internal unsafe void checkbuild(int skip)
        {
          skip |= flags;
          if ((skip & 0x04) == 0) { flags |= 0x04; Build(); }
          if ((skip & 0x08) == 0)
          {
            flags |= 0x08;
            fixed (Vector3* pp = vertices)
            fixed (ushort* ii = indices)
              DX11Ctrl.UpdateMesh(pp, vertices.Length, ii, indices.Length, 0.3f, ref ib, ref vb);
          }
        }
        protected internal override void Invalidate()
        {
          flags &= ~(0x04 | 0x08); base.Invalidate();
        }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from);
          var mat = from is Geometry geo ? geo.ranges[0].material : new Material { Diffuse = 0xffeeeeee };
          ranges = new (int count, Material material)[] { new(0, mat) };
        }
        protected internal override unsafe void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            if (ranges.Length == 1) save(e, ranges[0].material);
            else
            {
              var ea = new XElement(ns + "Materials"); e.AddFirst(ea);
              for (int k = 0; k < ranges.Length; k++)
              {
                var r = ranges[k]; var em = new XElement(ns + "Material"); ea.Add(em);
                if (r.count != 0) em.SetAttributeValue("count", r.count);
                save(em, r.material);
              }
            }
            static void save(XElement e, Material m)
            {
              e.SetAttributeValue("color", m.Diffuse.ToString("X8")); if (m.Texture == null) return;
              e.SetAttributeValue("texture", m.Texture.Url); var t = m.Transform; if (t.IsIdentity) return;
              e.SetAttributeValue("texture-trans", format(new ReadOnlySpan<float>(&t, 12)));
            }
          }
          else
          {
            var ea = e.Element(ns + "Materials");
            if (ea == null)
            {
              ranges = new (int, Material)[] { new(0, load(e)) };
            }
            else
            {
              ranges = new (int, Material)[ea.Elements().Count()]; int nr = 0;
              for (var t = ea.FirstNode; t != null; t = t.NextNode)
              {
                if (t is not XElement em) continue;
                ref var r = ref ranges[nr++]; r.material = load(em); XAttribute a;
                if ((a = em.Attribute("count")) != null) r.count = (int)a;
              }
            }
          }
          static Material load(XElement e)
          {
            var ma = new Material(); XAttribute a;
            if ((a = e.Attribute("color")) != null) ma.Diffuse = uint.Parse(a.Value, NumberStyles.HexNumber);
            if ((a = e.Attribute("texture")) != null) ma.Texture = DX11Ctrl.GetTexture(a.Value);
            if ((a = e.Attribute("texture-trans")) != null)
            {
              var m = default(Matrix4x3);
              parse(a.Value.AsSpan().Trim(), new Span<float>(&m, 12)); ma.Transform = m;
            }
            return ma;
          }
        }
        protected internal virtual void Build()
        {
          vertices = Array.Empty<Vector3>();
          indices = Array.Empty<ushort>();
        }
        protected internal virtual void Render(DX11Ctrl.DC dc, Node main) { }
        protected internal virtual Action<int>? GetTool(DX11Ctrl.PC pc, Node main)
        {
          return null;
        }
        protected internal virtual Array GetVertices() => vertices;
        protected void invert()
        {
          for (int i = 0; i < indices.Length; i += 3) { var t = indices[i]; indices[i] = indices[i + 1]; indices[i + 1] = t; }
        }
        protected void extruse(TesselatorR tess, float dz)
        {
          var vp = tess.VerticesVector3; var np = vp.Length;
          var vi = tess.Indices; var ni = vi.Length; var bz = dz != 0;
          var ll = bz ? tess.Outline : default;
          var cc = bz ? tess.OutlineCounts : default;
          Array.Resize(ref vertices, bz ? np * 2 : np);
          Array.Resize(ref indices, ni * 2 + ll.Length * 6);
          for (int i = 0; i < np; i++) vertices[i] = vp[i];
          if (bz) for (int i = 0; i < np; i++) { var p = vertices[i]; p.Z += dz; vertices[np + i] = p; }
          for (int i = 0; i < ni; i++) indices[i] = (ushort)vi[i];
          for (int i = 0, k = dz != 0 ? np : 0; i < ni; i += 3)
          {
            indices[ni + i + 0] = (ushort)(k + indices[i + 0]);
            indices[ni + i + 1] = (ushort)(k + indices[i + 2]);
            indices[ni + i + 2] = (ushort)(k + indices[i + 1]);
          }
          for (int i = 0, k = 0, t = ni << 1; i < cc.Length; k += cc[i++])
            for (int j = 0, n = cc[i]; j < n; j++, t += 6)
            {
              int a = ll[k + j], b = ll[k + (j + 1) % n];
              indices[t + 0] = indices[t + 5] = (ushort)(b);
              indices[t + 1] = (ushort)(a);
              indices[t + 2] = indices[t + 3] = (ushort)(a + np);
              indices[t + 4] = (ushort)(b + np); //t += 6;
            }
          if (dz < 0) invert();
        }
        class GTC : TypeConverter
        {
          public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;
          public override PropertyDescriptorCollection? GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
          {
            var geo = (Geometry)context.Instance;
            var pp = TypeDescriptor.GetProperties(geo);
            if (geo is BoolGeometry bg && bg.Source != BoolGeometry.MaterialSource.Own)
            {
              pp = new PropertyDescriptorCollection(pp.OfType<PropertyDescriptor>().
                Where(p => p.ComponentType != typeof(Geometry)).ToArray());
            }
            else if (geo.ranges[geo.Current].material.Texture == null)
              pp = new PropertyDescriptorCollection(pp.OfType<PropertyDescriptor>().
                Where(p => p.Name == "Texture" || !p.Name.StartsWith("Texture")).ToArray());
            return pp;
          }
        }
        class MTC : Int32Converter
        {
          public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;
          public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;
          public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
          {
            var n = ((Geometry)context.Instance).ranges.Length;
            return new StandardValuesCollection(Enumerable.Range(0, n).ToArray());
          }
        }
      }
      public class MeshGeometry : Geometry
      {
        [Category("Geometry")]
        public int VertexCount
        {
          get => rpts != null ? rpts.Length : vertices.Length;
        }
        [Category("Geometry")]
        public int IndexCount
        {
          get => indices.Length;
        }
        internal Vector3R[]? rpts;
        protected internal override void Build()
        {
          if (rpts != null)
          {
            var n = rpts.Length; Array.Resize(ref vertices, n);
            for (int i = 0; i < n; i++) vertices[i] = (Vector3)rpts[i];
          }
        }
        protected internal override unsafe void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            string s;
            if (rpts != null)
            {
              s = DX11Ctrl.ToString(new ReadOnlySpan<Vector3R>(rpts), "R");
              e.SetAttributeValue("vertices-rat", s);
            }
            else
            {
              fixed (Vector3* p = vertices) s = format(new ReadOnlySpan<float>(p, vertices.Length * 3));
              e.SetAttributeValue("vertices", s);
            }
            fixed (ushort* p = indices) s = format(new ReadOnlySpan<ushort>(p, indices.Length));
            e.SetAttributeValue("indices", s);
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("vertices-rat")) != null)
            {
              var sp = a.Value.AsSpan().Trim(); rpts = new Vector3R[tokencount(sp) / 3];
              for (int i = 0; i < rpts.Length; i++) rpts[i] = Vector3R.Parse(ref sp);
            }
            else if ((a = e.Attribute("vertices")) != null)
            {
              var sp = a.Value.AsSpan().Trim(); vertices = new Vector3[tokencount(sp) / 3];
              fixed (Vector3* p = vertices) parse(sp, new Span<float>(p, vertices.Length * 3));
            }
            if ((a = e.Attribute("indices")) != null)
            {
              var sp = a.Value.AsSpan().Trim(); indices = new ushort[tokencount(sp)];
              fixed (ushort* p = indices) parse(sp, new Span<ushort>(p, indices.Length));
            }
          }
        }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from);
          if (from is Geometry geo)
          {
            vertices = (Vector3[])geo.vertices.Clone();
            indices = (ushort[])geo.indices;
            ranges = ((int count, Material material)[])geo.ranges.Clone();
          }
          //else throw new ArgumentException();
        }
        protected internal override Array GetVertices() => rpts ?? (Array)vertices;
      }

      public class BoolGeometry : Geometry
      {
        PolyhedronR.Mode mode; Vector3R[]? rpts; MaterialSource source;
        (int count, Material material)[]? myranges;
        [Category("Geometry")]
        public PolyhedronR.Mode Operation
        {
          get => mode;
          set { mode = value; Invalidate(); }
        }
        public enum MaterialSource { Own, First, Second, Merge }
        [Category("Material")]
        public MaterialSource Source
        {
          get => source;
          set { source = value; ranges = myranges; propref = true; Invalidate(); }
        }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from); myranges = ranges;
        }
        protected internal override void Serialize(XElement e, bool storing)
        {
          { var t = ranges; ranges = myranges; myranges = t; }
          base.Serialize(e, storing);
          { var t = ranges; ranges = myranges; myranges = t; }
          if (storing)
          {
            e.SetAttributeValue("op", mode);
            if (source != 0) e.SetAttributeValue("mat", source);
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("op")) != null) PolyhedronR.Mode.TryParse(a.Value, out mode);
            if ((a = e.Attribute("mat")) != null) MaterialSource.TryParse(a.Value, out source);
          }
        }
        protected internal override Array GetVertices() => rpts ?? (Array)vertices;
        protected internal override void Build()
        {
          var n = Nodes != null ? Nodes.Length : 0;
          for (int i = 0; i < n; i++) Nodes[i].Parent = this;
          if (n >= 2 && Nodes[0] is Geometry a && Nodes[1] is Geometry b)
          {
            a.checkbuild(0x08);
            b.checkbuild(0x08);
            var mb = PolyhedronR.GetInstance();
            mb.SetMesh(0, a.GetVertices(), a.indices); mb.Transform(0, a.Transform);
            mb.SetMesh(1, b.GetVertices(), b.indices); mb.Transform(1, b.Transform);
            mb.Boolean(mode, source == MaterialSource.Merge ? 0 : 2);
            switch (source)
            {
              case MaterialSource.Merge: ranges = remap(mb.Indices, mb.Mapping, a, b); break;
              case MaterialSource.First: ranges = new (int count, Material material)[] { new(0, a.ranges[0].material) }; break;
              case MaterialSource.Second: ranges = new (int count, Material material)[] { new(0, b.ranges[0].material) }; break;
              default: ranges = myranges; break;
            }
            rpts = mb.Vertices.ToArray();
            vertices = mb.Vertices.Select(p => (Vector3)p).ToArray();
            indices = mb.Indices.Select(p => (ushort)p).ToArray();
            return;
          }
          rpts = null; ranges = myranges;
          vertices = Array.Empty<Vector3>();
          indices = Array.Empty<ushort>();
        }
        internal static (int, Material)[] remap(List<int> ii, List<int> map, Models.Geometry a, Models.Geometry b)
        {
          if (ii.Count == 0) return a.ranges.Take(1).ToArray();
          var t1 = a.ranges.Select((p, i) => (n: p.count != 0 ? p.count : a.indices.Length, m: p.material)).ToList();
          var n1 = t1.Count; var n2 = a.indices.Length / 3;
          var tm = a.Transform * !b.Transform;
          t1.AddRange(b.ranges.Select((p, i) => (n: p.count != 0 ? p.count : b.indices.Length, m: p.material.Clone(tm))));
          var tt = t1.SelectMany((p, i) => Enumerable.Repeat(i, p.n / 3)).ToArray();
          for (int i = 0, t; i < map.Count; i++) map[i] = tt[(((t = map[i]) & 1) * n2) + (t >> 1) / 3];
          var t2 = map.Zip(ii.Chunk(3)).GroupBy(p => t1[p.First].m, p => p.Second).ToArray();
          ii.Clear(); ii.AddRange(t2.SelectMany(p => p.SelectMany(p => p)));
          return t2.Select(p => (p.Count() * 3, p.Key)).ToArray();
        }
      }

      public class BoxGeometry : Geometry
      {
        internal Vector3 p1, p2;
        [Category("Geometry")]
        public Vector3 Min
        {
          get => p1;
          set { p1 = value; Invalidate(); }
        }
        [Category("Geometry")]
        public Vector3 Max
        {
          get => p2;
          set { p2 = value; Invalidate(); }
        }
        internal protected override void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            if (p1.X != 0) e.SetAttributeValue("x1", p1.X);
            if (p1.Y != 0) e.SetAttributeValue("y1", p1.Y);
            if (p1.Z != 0) e.SetAttributeValue("z1", p1.Z);
            if (p2.X != 0) e.SetAttributeValue("x2", p2.X);
            if (p2.Y != 0) e.SetAttributeValue("y2", p2.Y);
            if (p2.Z != 0) e.SetAttributeValue("z2", p2.Z);
          }
          else
          {
            XAttribute p;
            if ((p = e.Attribute("x1")) != null) p1.X = (float)p;
            if ((p = e.Attribute("y1")) != null) p1.Y = (float)p;
            if ((p = e.Attribute("z1")) != null) p1.Z = (float)p;
            if ((p = e.Attribute("x2")) != null) p2.X = (float)p;
            if ((p = e.Attribute("y2")) != null) p2.Y = (float)p;
            if ((p = e.Attribute("z2")) != null) p2.Z = (float)p;
          }
        }
        protected internal override void Build()
        {
          Array.Resize(ref vertices, 8);
          Array.Resize(ref indices, 36);
          vertices[0] = new Vector3(p1.X, p1.Y, p1.Z);
          vertices[1] = new Vector3(p2.X, p1.Y, p1.Z);
          vertices[2] = new Vector3(p2.X, p2.Y, p1.Z);
          vertices[3] = new Vector3(p1.X, p2.Y, p1.Z);
          vertices[4] = new Vector3(p1.X, p1.Y, p2.Z);
          vertices[5] = new Vector3(p2.X, p1.Y, p2.Z);
          vertices[6] = new Vector3(p2.X, p2.Y, p2.Z);
          vertices[7] = new Vector3(p1.X, p2.Y, p2.Z);
          indices[00] = 0; indices[01] = 2; indices[02] = 1; indices[03] = 2; indices[04] = 0; indices[05] = 3;
          indices[06] = 4; indices[07] = 5; indices[08] = 6; indices[09] = 6; indices[10] = 7; indices[11] = 4;
          indices[12] = 0; indices[13] = 1; indices[14] = 5; indices[15] = 5; indices[16] = 4; indices[17] = 0;
          indices[18] = 1; indices[19] = 2; indices[20] = 6; indices[21] = 6; indices[22] = 5; indices[23] = 1;
          indices[24] = 2; indices[25] = 3; indices[26] = 7; indices[27] = 7; indices[28] = 6; indices[29] = 2;
          indices[30] = 3; indices[31] = 0; indices[32] = 4; indices[33] = 4; indices[34] = 7; indices[35] = 3;
          if (p1.X < p2.X ^ p1.Y < p2.Y ^ p1.Z < p2.Z) invert();
        }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from); p2 = new Vector3(1);
          if (from != null)
          {
            var box = from.GetBox();
            if (box.Min.X != float.MaxValue) { Nodes = null; p1 = box.Min; p2 = box.Max; }
          }
        }
      }
      public class SphereGeometry : Geometry
      {
        float radius; public int segx, segy;
        [Category("Geometry")]
        public float Radius { get => radius; set { radius = value; Invalidate(); } }
        [Category("Geometry")]
        public int SegX { get => segx; set { segx = value; Invalidate(); } }
        [Category("Geometry")]
        public int SegY { get => segy; set { segy = value; Invalidate(); } }
        protected internal override void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            if (segx != 0) e.SetAttributeValue("segx", segx);
            if (segy != 0) e.SetAttributeValue("segy", segy);
            if (radius != 0) e.SetAttributeValue("radius", radius);
          }
          else
          {
            XAttribute p;
            if ((p = e.Attribute("segx")) != null) segx = (int)p;
            if ((p = e.Attribute("segy")) != null) segy = (int)p;
            if ((p = e.Attribute("radius")) != null) radius = (float)p;
          }
        }
        protected internal override void Build()
        {
          var dt = (360 * (MathF.PI / 180)) / segx;
          var dp = (180 * (MathF.PI / 180)) / (segy + 1);
          Array.Resize(ref vertices, segx * segy + 2); int np = 0;
          vertices[np++] = new Vector3(0, radius, 0);
          for (int pi = 0; pi < segy; pi++)
          {
            var sc = sincos((pi + 1) * dp);
            for (int ti = 0; ti < segx; ti++)
            {
              var vc = sincos(ti * dt);
              vertices[np++] = new Vector3(vc.Y * sc.Y, sc.X, vc.X * sc.Y) * radius;
            }
          }
          vertices[np++] = new Vector3(0, -radius, 0);
          Array.Resize(ref indices, segx * (segy - 1) * 6 + segx * 6); int ni = 0;
          for (int x = 0, u = segx - 1; x < segx; u = x++)
          {
            indices[ni++] = (ushort)0;
            indices[ni++] = (ushort)(1 + x);
            indices[ni++] = (ushort)(1 + u);
          }
          for (int t = 0; t < segy - 1; t++)
            for (int y = 1 + t * segx, v = y + segx,
              u = segx - 1, x = 0; x < segx; u = x++)
            {
              indices[ni++] = (ushort)(y + u);
              indices[ni++] = (ushort)(y + x);
              indices[ni++] = (ushort)(v + u);
              indices[ni++] = (ushort)(y + x);
              indices[ni++] = (ushort)(v + x);
              indices[ni++] = (ushort)(v + u);
            }
          for (int x = 0, u = segx - 1, t = np - 1 - segx; x < segx; u = x++)
          {
            indices[ni++] = (ushort)(np - 1);
            indices[ni++] = (ushort)(t + u);
            indices[ni++] = (ushort)(t + x);
          }
        }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from);
          segx = 32; segy = 16; radius = 1;
          if (from == null) return;
          var box = from.GetBox();
          if (box.Min.X != float.MaxValue) { Nodes = null; radius = (box.Max - box.Min).Length() * 0.5f; }
        }
      }
      public class CylinderGeometry : Geometry
      {
        float radius, height; int seg;
        [Category("Geometry")]
        public float Radius { get => radius; set { radius = value; Invalidate(); } }
        [Category("Geometry")]
        public float Height { get => height; set { height = value; Invalidate(); } }
        [Category("Geometry")]
        public int Seg { get => seg; set { seg = value; Invalidate(); } }
        internal protected override void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            if (seg != 0) e.SetAttributeValue("seg", seg);
            if (radius != 0) e.SetAttributeValue("radius", radius);
            if (height != 0) e.SetAttributeValue("height", height);
          }
          else
          {
            XAttribute p;
            if ((p = e.Attribute("seg")) != null) seg = (int)p;
            if ((p = e.Attribute("radius")) != null) radius = (float)p;
            if ((p = e.Attribute("height")) != null) height = (float)p;
          }
        }
        protected internal override void Build()
        {
          Array.Resize(ref vertices, seg * 2);
          var f = (2 * MathF.PI) / seg; var h = new Vector3(0, 0, height);
          for (int i = 0; i < seg; i++) vertices[seg + i] = (vertices[i] = new Vector3(sincos(i * f) * radius, 0)) + h;
          int s = (seg - 2) * 3, t = height != 0 ? seg * 6 : 0;
          Array.Resize(ref indices, s * 2 + t);
          for (int i = 1, k = 0, j = s + t; i < seg - 1; i++)
          {
            indices[k++] = 0; indices[k++] = (ushort)i; indices[k++] = (ushort)(i + 1);
            indices[j++] = (ushort)seg; indices[j++] = (ushort)(seg + i + 1); indices[j++] = (ushort)(seg + i);
          }
          if (t == 0) return;
          for (int i = 0, k = s, j = seg - 1; i < seg; j = i++, k += 6)
          {
            indices[k + 0] = indices[k + 5] = (ushort)i; indices[k + 1] = (ushort)j;
            indices[k + 2] = indices[k + 3] = (ushort)(j + seg); indices[k + 4] = (ushort)(i + seg);
          }
        }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from);
          seg = 32; radius = height = 1;
          if (from == null) return;
          var box = from.GetBox();
          if (box.Min.X != float.MaxValue) { Nodes = null; height = box.Max.Z; radius = (box.Max - box.Min).Length() * 0.5f; }
        }
      }

      [EditorAttribute(typeof(Editor), typeof(ComponentEditor))]
      public abstract class Poly2DGeometry : Geometry
      {
        protected internal Vector2[]? points;
        protected internal ushort[]? counts;
        class Editor : WindowsFormsComponentEditor
        {
          public override bool EditComponent(ITypeDescriptorContext context, object component, IWin32Window owner)
          {
            var grid = (PropertyGrid)owner;
            var toolstrip = (ToolStrip)grid.Controls[3];
            var btn = (ToolStripButton)toolstrip.Items[4];
            if (btn.Checked) return false;
            btn.Checked = true; toolstrip.Cursor = Cursors.Default;
            var page = new PolyEditCtrl { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Tag = component };
            var site = grid.Controls[2].Controls; site.Add(page); page.BringToFront(); page.Focus();
            var info = grid.Controls[0]; info.Controls.Add(page.infogrid); page.infogrid.BringToFront();
            grid.PropertySort = PropertySort.NoSort;
            grid.SelectedObjectsChanged += rem;
            grid.PropertySortChanged += rem;
            void rem(object? p, EventArgs e)
            {
              grid.SelectedObjectsChanged -= rem;
              grid.PropertySortChanged -= rem;
              btn.Checked = false; page.Dispose(); page.infogrid.Dispose();
            }
            return false;
          }
        }
        class PolyEditCtrl : UserControl
        {
          internal PropertyGrid? infogrid;
          public PolyEditCtrl() { DoubleBuffered = true; }
          Vector2 pos, sca; Pen? pen;
          Poly2DGeometry? geo; Action<int>? tool; int isel;
          DX11ModelCtrl RootCtrl => (DX11ModelCtrl)geo.GetService(typeof(DX11ModelCtrl));
          static Vector2 conv(Point p) => new Vector2(p.X, p.Y);
          static Vector2 conv(Size p) => new Vector2(p.Width, p.Height);
          Vector2 curpos => conv(PointToClient(Cursor.Position));
          Vector2 trans1(Vector2 p) => pos + p * sca;
          Vector2 trans2(Vector2 p) => (p - pos) / sca;
          Vector2 raster(Vector2 p) => new Vector2(MathF.Round(p.X, 3), MathF.Round(p.Y, 3));
          static Action undo(Poly2DGeometry geo, int i, Vector2 p)
          {
            return () => { var t = geo.points[i]; geo.points[i] = p; p = t; geo.Invalidate(); };
          }
          static Action undo(Poly2DGeometry geo, Vector2[] p)
          {
            return () => { var t = geo.points; geo.points = p; p = t; geo.Invalidate(); };
          }
          static Action undo(Poly2DGeometry geo, Vector2[] p, ushort[] c)
          {
            if (c == geo.counts) return undo(geo, p);
            return () =>
            {
              var t = geo.points; geo.points = p; p = t;
              var s = geo.counts; geo.counts = c; c = s; geo.Invalidate();
            };
          }
          static int ipoly(IReadOnlyList<ushort>? counts, int i)
          {
            if (counts == null) return -1; int t = 0;
            for (int x = 0, y; t < counts.Count && (y = (x + counts[t])) <= i; x = y, t++) ; return t;
          }
          protected override void OnLoad(EventArgs e)
          {
            base.OnLoad(e); geo = (Poly2DGeometry)Tag;
            infogrid = new PropertyGrid { Dock = DockStyle.Fill };
            var cc = infogrid.Controls; cc[0].Visible = cc[3].Visible = false;
            infogrid.PropertySort = PropertySort.NoSort;
            infogrid.SelectedObject = new Info { view = this };
            var si = conv(Parent.Size); pos = si * 0.5f; sca = new Vector2(300, -300);
            if (geo.points.Length != 0)
            {
              isel = 0x10000000; pos = default;
              var pp = geo.points; Vector2 min = new Vector2(0/*float.MaxValue*/), max = -min;
              for (int i = 0; i < pp.Length; i++) { var p = trans1(pp[i]); min = Vector2.Min(min, p); max = Vector2.Max(max, p); }
              var ts = (si * 0.5f) / (max - min); var f = Math.Min(ts.X, ts.Y);
              pos = si * 0.5f - (min + max) * 0.5f * f; sca *= f;
            }
          }
          protected override void OnPaint(PaintEventArgs e)
          {
            var g = e.Graphics; if (pen == null) pen = new Pen(Color.Black, 2);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawLine(Pens.Red, pos.X - 50, pos.Y, pos.X + 1000, pos.Y);
            g.DrawLine(Pens.Green, pos.X, pos.Y + 50, pos.X, pos.Y - 1000);

            for (int k = 0, nk = geo.counts != null ? geo.counts.Length : 1, a = 0, n; k < nk; k++, a += n)
            {
              n = geo.counts != null ? geo.counts[k] : geo.points.Length;
              for (int i = 0; i < n; i++)
              {
                var p1 = trans1(geo.points[a + i]);
                var p2 = trans1(geo.points[a + (i + 1) % n]);
                g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
              }
            }
            for (int i = 0, n = geo.points.Length; i < n; i++)
            {
              var p1 = trans1(geo.points[i]);
              g.FillEllipse((i | 0x10000000) == isel ? Brushes.Red : Brushes.White, p1.X - 4, p1.Y - 4, 8, 8);
              g.DrawEllipse(Pens.Black, p1.X - 4, p1.Y - 4, 8, 8);
            }
            if (!infogrid.ContainsFocus) infogrid.Controls[2].Refresh();
          }
          int pick(Point e)
          {
            var pt = conv(e);
            for (int i = 0, n = geo.points.Length; i < n; i++)
            {
              var p1 = trans1(geo.points[i]);
              if ((p1 - pt).LengthSquared() < 8 * 8) return 0x10000000 | i;
            }
            return 0;
          }
          Action<int> tool_pt(int wo)
          {
            isel = wo; Invalidate(); wo &= 0x0fffffff;
            var p1 = trans2(curpos); var o = geo.points[wo];
            var ud = default(Action); var vd = ud;
            if (ModifierKeys == Keys.Control)
            {
              var pp = geo.points.Take(wo + 1).Concat(geo.points.Skip(wo)).ToArray();
              var cc = geo.counts != null ? geo.counts.ToArray() : null; if (cc != null) cc[ipoly(cc, wo)]++;
              ud = undo(geo, pp, cc);
            }
            return id =>
            {
              if (id == 0)
              {
                if (ud != null && vd == null) (vd = ud)();
                var p2 = trans2(curpos);
                var p = o + (p2 - p1); if (geo is RotationGeometry) p.X = Math.Max(0, p.X);
                p = raster(p); if (p == geo.points[wo]) return;
                geo.points[wo] = p; geo.Invalidate(); this.Invalidate();
              }
              if (id == 1)
              {
                if (geo.points[wo] != o) RootCtrl.AddUndo(vd ?? undo(geo, wo, o));
                else vd?.Invoke();
              }
            };
          }
          Action<int> tool_new()
          {
            var p1 = raster(trans2(curpos)); var p2 = p1; var p3 = p1;
            var np = geo.points.Length;
            var pp = geo.points.Concat(Enumerable.Repeat(p1, 4)).ToArray();
            var cc = (geo.counts != null ? geo.counts : new ushort[] { (ushort)np }).Concat(Enumerable.Repeat((ushort)4, 1)).ToArray();
            var ud = undo(geo, pp, cc); var vd = default(Action); Cursor = Cursors.Cross;
            return id =>
            {
              if (id == 0)
              {
                p2 = raster(trans2(curpos)); if (p3 == p2) return; p3 = p2;
                if (vd == null) (vd = ud)();
                geo.points[np + 1].X = p2.X;
                geo.points[np + 2] = p2;
                geo.points[np + 3].Y = p2.Y; geo.Invalidate(); Invalidate();
              }
              if (id == 1)
              {
                Cursor = Cursors.Default; if (vd == null) return;
                if (p1 != p2) RootCtrl.AddUndo(vd); else { vd(); Invalidate(); }
              }
            };
          }
          Action<int> tool_nav()
          {
            var p1 = curpos; var o = pos;
            return id =>
            {
              if (id == 0)
              {
                var p2 = curpos;
                pos = o + (p2 - p1); Invalidate();
              }
            };
          }
          protected override void OnMouseDown(MouseEventArgs e)
          {
            if (e.Button == MouseButtons.Left)
            {
              var wo = pick(e.Location); Focus();
              if ((wo & 0x10000000) != 0) tool = tool_pt(wo);
              else if (ModifierKeys == Keys.Control) tool = tool_new();
              else tool = tool_nav();
              if (tool != null) Capture = true;
            }
            base.OnMouseDown(e);
          }
          protected override void OnMouseMove(MouseEventArgs e)
          {
            if (tool != null) { tool(0); Update(); return; }
            var wo = pick(e.Location);
            Cursor = (wo & 0x10000000) != 0 ? Cursors.Cross : Cursors.Default;
          }
          protected override void OnMouseUp(MouseEventArgs e)
          {
            if (tool != null) { tool(1); tool = null; Capture = false; return; }
            base.OnMouseUp(e);
          }
          protected override void OnMouseLeave(EventArgs e)
          {
            if (tool != null) { tool(1); tool = null; Capture = false; return; }
            base.OnMouseLeave(e);
          }
          protected override void OnMouseWheel(MouseEventArgs e)
          {
            var p1 = conv(e.Location);  //if (!ClientRectangle.Contains(p1)) return;
            var s = sca.X * (1 + e.Delta * (0.1f * 1f / 120));
            var p2 = trans2(p1);
            sca = new Vector2(s, -s);
            var p3 = trans1(p2);
            pos += p1 - p3; Invalidate();
          }
          protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
          {
            if (keyData == Keys.Delete)
            {
              if ((isel & 0x10000000) != 0)
              {
                var i = isel & 0x0fffffff; if (i >= geo.points.Length) return true;
                var pp = geo.points.ToList(); pp.RemoveAt(i);
                var cc = geo.counts != null ? geo.counts.ToList() : null;
                if (cc != null) { var t = ipoly(cc, i); if (--cc[t] == 0) cc.RemoveAt(t); if (cc.Count <= 1) cc = null; }
                RootCtrl.Execute(undo(geo, pp.ToArray(), cc != null ? cc.ToArray() : null));
                if (i != 0 && i == geo.points.Length) isel--; Invalidate();
              }
              return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
          }
          class Info
          {
            internal PolyEditCtrl? view;
            int ipt() => (view.isel & 0x10000000) != 0 && (view.isel & 0x0fffffff) < view.geo.points.Length ? view.isel & 0x0fffffff : -1;
            public float X
            {
              get { var i = ipt(); return i != -1 ? view.geo.points[i].X : 0; }
              set
              {
                var i = ipt(); if (i == -1) return;
                var p = view.geo.points[i]; p.X = value == 0 ? 0 : value;
                view.RootCtrl.Execute(undo(view.geo, i, p)); view.Invalidate();
              }
            }
            public float Y
            {
              get { var i = ipt(); return i != -1 ? view.geo.points[i].Y : 0; }
              set
              {
                var i = ipt(); if (i == -1) return;
                var p = view.geo.points[i]; p.Y = value == 0 ? 0 : value;
                view.RootCtrl.Execute(undo(view.geo, i, p)); view.Invalidate();
              }
            }
          }
        }
      }

      public class ExtrusionGeometry : Poly2DGeometry
      {
        float height; Winding winding;
        [Category("Geometry")]
        public float Height { get => height; set { height = value; Invalidate(); } }
        [Category("Geometry")]
        public Winding Winding { get => winding; set { winding = value; Invalidate(); } }
        internal protected override unsafe void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            string s; fixed (Vector2* p = points) s = format(new ReadOnlySpan<float>(p, points.Length << 1));
            e.SetAttributeValue("points", s);
            fixed (ushort* p = counts) s = format(new ReadOnlySpan<ushort>(p, counts.Length));
            e.SetAttributeValue("counts", s);
            if (height != 0) e.SetAttributeValue("height", height);
            if (winding != 0) e.SetAttributeValue("winding", winding);
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("points")) != null)
            {
              var sp = a.Value.AsSpan().Trim(); points = new Vector2[tokencount(sp) >> 1];
              fixed (Vector2* p = points) parse(sp, new Span<float>(p, points.Length << 1));
            }
            if ((a = e.Attribute("counts")) != null)
            {
              var sp = a.Value.AsSpan().Trim(); counts = new ushort[tokencount(sp)];
              fixed (ushort* p = counts) parse(sp, new Span<ushort>(p, counts.Length));
            }
            if ((a = e.Attribute("height")) != null) height = (float)a;
            if ((a = e.Attribute("winding")) != null) Winding.TryParse(a.Value, out winding);
          }
        }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from);
          Vector2 min = default, max = new Vector2(1);
          if (from != null)
          {
            var box = from.GetBox();
            if (box.Min.X != float.MaxValue)
            {
              Nodes = null;
              min = new(box.Min.X, box.Min.Y);
              max = new(box.Max.X, box.Max.Y);
              height = box.Max.Z;
            }
          }
          //counts = new ushort[] { 4 };
          points = new Vector2[] { min, new(max.X, min.Y), max, new(min.X, max.Y), };
        }
        protected internal override void Build()
        {
          var tess = TesselatorR.GetInstance();
          tess.Winding = winding;// Winding.Positive;
          tess.Options = TesselatorR.Option.Fill | TesselatorR.Option.Delaunay | TesselatorR.Option.OutlinePrecise | TesselatorR.Option.Trim;
          tess.BeginPolygon();
          for (int i = 0, k = 0, c = counts != null ? counts.Length : 1, j, n; i < c; k += n, i++)
          {
            tess.BeginContour();
            for (j = 0, n = counts != null ? counts[i] : points.Length; j < n; j++)
              tess.AddVertex(points[k + j]);
            tess.EndContour();
          }
          tess.EndPolygon();
          extruse(tess, height);
        }
        protected internal override void Render(DX11Ctrl.DC dc, Node main)
        {
          dc.Transform = main.GetTransform();
          for (int i = 0; i < points.Length; i++) dc.SetPoint(i, new Vector3(points[i], height));
          dc.Select(main, 0x20000000);
          dc.Color = 0xff000000;
          if (counts == null) dc.DrawPolygon(0, points.Length);
          else for (int i = 0, k = 0; i < counts.Length; k += counts[i++]) dc.DrawPolygon(k, counts[i]);
          dc.Select(main, 0x10000000);
          dc.Color = 0xffffffff;
          dc.DrawPoints(points.Length, 7);
          dc.Select();
        }
        protected internal override Action<int>? GetTool(DX11Ctrl.PC pc, Node main)
        {
          if ((pc.Id & 0x10000000) != 0) return tool(pc, main);
          return base.GetTool(pc, main);
          Action<int> tool(DX11Ctrl.PC pc, Node main)
          {
            var i = pc.Primitive; var o = points[i];
            pc.SetPlane(Matrix4x3.CreateTranslation(0, 0, height) * main.GetTransform());
            var p1 = pc.Pick(); var p2 = p1;
            return id =>
            {
              if (id == 0) { p2 = pc.Pick(); points[i] = o + (p2 - p1); Invalidate(); }
              if (id == 1 && p1 != p2) (pc.View as DX11ModelCtrl)?.AddUndo(() => { var t = points[i]; points[i] = o; o = t; Invalidate(); });
            };
          }
        }
      }

      public class RotationGeometry : Poly2DGeometry
      {
        int seg;
        [Category("Geometry")]
        public int Seg { get => seg; set { seg = value; Invalidate(); } }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from);
          seg = 20;
          var n = 20; var f = (MathF.Tau / n); var r = 0.2f; var v = new Vector2(0.5f, 0);
          Array.Resize(ref points, n);
          for (int i = 0; i < n; i++) points[i] = sincos(i * f) * r + v;
        }
        internal protected override unsafe void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            string s; fixed (Vector2* p = points) s = format(new ReadOnlySpan<float>(p, points.Length << 1));
            e.SetAttributeValue("points", s);
            if (counts != null)
            {
              fixed (ushort* p = counts) s = format(new ReadOnlySpan<ushort>(p, counts.Length));
              e.SetAttributeValue("counts", s);
            }
            e.SetAttributeValue("seg", seg);
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("points")) != null)
            {
              var sp = a.Value.AsSpan().Trim(); points = new Vector2[tokencount(sp) >> 1];
              fixed (Vector2* p = points) parse(sp, new Span<float>(p, points.Length << 1));
            }
            if ((a = e.Attribute("counts")) != null)
            {
              var sp = a.Value.AsSpan().Trim(); counts = new ushort[tokencount(sp)];
              fixed (ushort* p = counts) parse(sp, new Span<ushort>(p, counts.Length));
            }
            if ((a = e.Attribute("seg")) != null) seg = (int)a;
          }
        }
        class Builder
        {
          [ThreadStatic] static WeakReference? wr; Builder() { }
          public static Builder GetInstance()
          {
            if (wr == null || wr.Target is not Builder p)
              wr = new WeakReference(p = new Builder());
            return p;
          }
          public void Extruse(Vector2[] pp, ushort[]? cc, int nz)
          {
            var np = pp.Length; var nc = cc != null ? cc.Length : 1;
            this.pp.Clear(); this.pp.EnsureCapacity(np * nz);
            ii.Clear(); ii.EnsureCapacity(np * nz * 6);
            for (int l = 0, k1 = 0; l < nz; l++, k1 += np)
            {
              var k2 = (l + 1) % nz * np;
              for (int i = 0; i < np; i++) this.pp.Add(new Vector3(pp[i], l));
              for (int i = 0, a = 0, b, j; i < nc; i++, a += b)
                for (j = 0, b = cc != null ? cc[i] : np; j < b; j++)
                {
                  var t = (j + 1) % b;
                  ii.Add((ushort)(k1 + a + j));
                  ii.Add((ushort)(k2 + a + j));
                  ii.Add((ushort)(k1 + a + t));
                  ii.Add((ushort)(k1 + a + t));
                  ii.Add((ushort)(k2 + a + j));
                  ii.Add((ushort)(k2 + a + t));
                }
            }
          }
          public void Wash()
          {
            dict.Clear(); dict.EnsureCapacity(pp.Count);
            kk.Clear(); kk.EnsureCapacity(ii.Count);
            for (int i = 0; i < ii.Count; i += 3)
            {
              var p1 = pp[ii[i + 0]]; var p2 = pp[ii[i + 1]]; var p3 = pp[ii[i + 2]];
              if (Vector3.Cross(p2 - p1, p3 - p1) == default) continue;
              kk.Add(addp(p1)); kk.Add(addp(p2)); kk.Add(addp(p3));
            }
            ii.Clear(); for (int i = 0; i < kk.Count; i++) ii.Add((ushort)kk[i]);
            pp.Clear(); for (var e = dict.GetEnumerator(); e.MoveNext();) pp.Add(e.Current.Key);
          }
          public void CopyTo(ref Vector3[] vv, ref ushort[] ii)
          {
            Array.Resize(ref vv, this.pp.Count); this.pp.CopyTo(vv);
            Array.Resize(ref ii, this.ii.Count); this.ii.CopyTo(ii);
          }
          internal readonly List<ushort> ii = new();
          internal readonly List<Vector3> pp = new();
          internal readonly List<int> kk = new();
          internal readonly Dictionary<Vector3, int> dict = new();
          int addp(Vector3 p)
          {
            if (!dict.TryGetValue(p, out var x)) dict.Add(p, x = dict.Count); return x;
          }
        }

        protected internal override void Build()
        {
          var bld = Builder.GetInstance();
          bld.Extruse(points, counts, seg);
          var ft = MathF.Tau / seg;
          for (int i = 0; i < bld.pp.Count; i++)
          {
            var p = bld.pp[i]; var s = sincos(p.Z * ft);
            bld.pp[i] = new Vector3(s.X * p.X, p.Y, s.Y * p.X);
          }
          bld.Wash();
          bld.CopyTo(ref vertices, ref indices);

          //static void torus(ref Vector3[] vv, ref ushort[] ii, Vector2[] pp, ushort[]? cc, int nz)
          //{
          //  var np = pp.Length; var nc = cc != null ? cc.Length : 1;
          //  Array.Resize(ref vv, np * nz);
          //  Array.Resize(ref ii, np * nz * 6);
          //  for (int l = 0, k1 = 0, ci = 0; l < nz; l++, k1 += np)
          //  {
          //    var k2 = (l + 1) % nz * np;
          //    for (int i = 0; i < np; i++) vv[k1 + i] = new Vector3(pp[i], l);
          //    for (int i = 0, a = 0, b, j; i < nc; i++, a += b)
          //      for (j = 0, b = cc != null ? cc[i] : np; j < b; j++, ci += 6)
          //      {
          //        var t = (j + 1) % b;
          //        ii[ci + 0] = (ushort)(k1 + a + j);
          //        ii[ci + 1] = ii[ci + 4] = (ushort)(k2 + a + j);
          //        ii[ci + 2] = ii[ci + 3] = (ushort)(k1 + a + t);
          //        ii[ci + 5] = (ushort)(k2 + a + t);
          //      }
          //  }
          //} 
          //torus(ref vertices, ref indices, points, counts, seg);
          //
          //var ft = MathF.Tau / seg;
          //for (int i = 0; i < vertices.Length; i++)
          //{
          //  var p = vertices[i]; var s = sincos(p.Z * ft);
          //  vertices[i] = new Vector3(s.X * p.X, p.Y, s.Y * p.X);
          //}
          //
          //for (int i = 0; i < indices.Length; i += 3)
          //{
          //  var p1 = vertices[indices[i + 0]]; var p2 = vertices[indices[i + 1]]; var p3 = vertices[indices[i + 2]];
          //  if (Vector3.Cross(p2 - p1, p3 - p1) == default) continue;
          //  //kk.Add(addp(p1)); kk.Add(addp(p2)); kk.Add(addp(p3));
          //}


          //if (points.Any(p => p.X == 0))
          //{
          //  int iw = 0; var dict = new Dictionary<Vector3, int>(vertices.Length);
          //  for (int i = 0; i < indices.Length; i++)
          //  {
          //    var p = vertices[indices[i]];
          //    if (!dict.TryGetValue(p, out var x)) dict.Add(p, x = dict.Count);
          //    indices[iw++] = (ushort)x;
          //    if ((iw % 3 == 0) && (
          //        indices[iw - 3] == indices[iw - 2] ||
          //        indices[iw - 2] == indices[iw - 1] ||
          //        indices[iw - 1] == indices[iw - 3])) iw -= 3;
          //  }
          //  Array.Resize(ref indices, iw);
          //  Array.Resize(ref vertices, dict.Count);
          //  dict.Keys.CopyTo(vertices, 0);
          //}

        }
        protected internal override void Render(DX11Ctrl.DC dc, Node main)
        {
          dc.Transform = main.GetTransform();
          for (int i = 0; i < points.Length; i++) dc.SetPoint(i, new Vector3(points[i], 0));
          dc.Select(main, 0x20000000);
          dc.Color = 0xff000000;
          if (counts == null) dc.DrawPolygon(0, points.Length);
          else for (int i = 0, k = 0; i < counts.Length; k += counts[i++]) dc.DrawPolygon(k, counts[i]);
          dc.Select(main, 0x10000000);
          dc.Color = 0xffffffff;
          dc.DrawPoints(points.Length, 7);
          dc.Select();
        }
        protected internal override Action<int>? GetTool(DX11Ctrl.PC pc, Node main)
        {
          if ((pc.Id & 0x10000000) != 0) return tool(pc, main);
          return base.GetTool(pc, main);
          Action<int> tool(DX11Ctrl.PC pc, Node main)
          {
            var i = pc.Primitive; var o = points[i];
            pc.SetPlane(Matrix4x3.CreateTranslation(0, 0, 0) * main.GetTransform());
            var p1 = pc.Pick(); var p2 = p1;
            return id =>
            {
              if (id == 0)
              {
                p2 = pc.Pick(); var p = o + (p2 - p1); p.X = Math.Max(0, p.X);
                if (p != points[i]) { points[i] = p; Invalidate(); }
              }
              if (id == 1 && o != points[i]) ((DX11ModelCtrl)pc.View).AddUndo(() => { var t = points[i]; points[i] = o; o = t; Invalidate(); });
            };
          }
        }
      }

      public class TextGeometry : Geometry
      {
        string text = string.Empty; System.Drawing.Font? font;
        float height = 1, depth; int flat = 8;
        [Category("Geometry")]
        public string Text { get => text; set { text = value; Invalidate(); } }
        [Category("Geometry")]
        public System.Drawing.Font Font
        {
          get => font ??= new System.Drawing.Font("Arial", 72);
          set
          {
            if (value.SizeInPoints != 72) value = new System.Drawing.Font(value.Name, 72, value.Style);
            font = value; Invalidate();
          }
        }
        [Category("Geometry")]
        public float Height { get => height; set { height = value; Invalidate(); } }
        [Category("Geometry")]
        public float Depth { get => depth; set { depth = value; Invalidate(); } }
        [Category("Geometry")]
        public int Flat
        {
          get => flat;
          set { flat = value; Invalidate(); }
        }
        protected internal override void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            e.SetAttributeValue("text", text);
            if (font != null)
            {
              if (font.Name != "Arial") e.SetAttributeValue("font", font.Name);
              if (font.Style != 0) e.SetAttributeValue("font-style", font.Style);
            }
            if (flat != 8) e.SetAttributeValue("flat", flat);
            if (height != 0) e.SetAttributeValue("height", height);
            if (depth != 0) e.SetAttributeValue("depth", depth);
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("text")) != null) text = a.Value;
            var fn = "Arial"; var fs = default(FontStyle);
            if ((a = e.Attribute("font")) != null) fn = a.Value;
            if ((a = e.Attribute("font-style")) != null) fs = Enum.Parse<FontStyle>(a.Value, true);
            font = new System.Drawing.Font(fn, 72, fs);
            if ((a = e.Attribute("flat")) != null) Flat = (int)a;
            if ((a = e.Attribute("height")) != null) height = (float)a;
            if ((a = e.Attribute("depth")) != null) depth = (float)a;
          }
        }
        protected internal override unsafe void Build()
        {
          var tess = TesselatorR.GetInstance();
          tess.Winding = Winding.EvenOdd;
          tess.Options = TesselatorR.Option.Fill | TesselatorR.Option.Delaunay | TesselatorR.Option.Trim |
            (depth != 0 ? TesselatorR.Option.OutlinePrecise : 0);
          tess.BeginPolygon();
          float xx = 0, sc = (1f / 0x10000 * 0.0113f) * height;
          fixed (char* p = text) DX11Ctrl.GlyphContour(p, text.Length, Font, Flat, (id, x, y) =>
          {
            switch (id)
            {
              case 0: tess.BeginContour(); xx = (float)x * 0x10000; break;
              case 1: tess.AddVertex(new Vector2(xx + x, y) * sc); break;
              case 2: tess.EndContour(); break;
            }
          });
          tess.EndPolygon();
          extruse(tess, depth);
        }
        protected internal override void InitNew(Node? from)
        {
          base.InitNew(from);
          if (from == null) return;
          var box = from.GetBox();
          if (box.Min.X != float.MaxValue) { text = "X"; var s = box.Max - box.Min; depth = s.Y; height = s.Z; }
        }
      }

    }
  }

}