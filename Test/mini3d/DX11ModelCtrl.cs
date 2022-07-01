#pragma warning disable CS8601, CS8600, CS8602, CS8603, CS8604
using System.Buffers;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing.Design;
using System.Globalization;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms.Design;
using System.Xml.Linq;
using System.Xml.Serialization;
using Group = Test.DX11ModelCtrl.Group;

namespace Test
{
  [DesignerCategory("Code")]
  public unsafe partial class DX11ModelCtrl : DX11Ctrl
  {
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
    public List<string> Infos
    {
      get => infos ??= new();
    }
    public Settings settings { get => _settings ??= new Settings(this); }
    Models.Scene? scene; Settings? _settings;
    Models.Camera? camera; Models.Light? light;
    List<Models.Geometry>? transp;
    internal List<Group>? selection; internal (Control view, object? p) extrasel;
    internal List<Undo>? undos; internal int undoi;  //todo: private
    Action<DC>? RenderClient; List<string>? infos; //string? adapter;
    int flags = 0x01 | 0x02 | 0x04 | 0x10; // 0x01:SelectBox, 0x02:Select Pivot, 0x04:Wireframe, 0x08:Normals, 0x10:Clients, 0x20:fps
    protected override void OnLoad(EventArgs e)
    {
      Initialize(4L << 32);
      BkColor = (uint)BackColor.ToArgb();
      selection = new(); transp = new();
    }
    void link(Node node)
    {
      if (camera == null) camera = node as Models.Camera;
      if (light == null) light = node as Models.Light;
      var a = node.VisibleNodes;
      if (a != null) for (int i = 0; i < a.Length; i++) { var p = a[i]; p.parent = node; link(p); }
      if (node is Models.Geometry geo) geo.checkbuild(0);
    }
    void render(DC dc, int was, Group[] nodes)
    {
      var tm = dc.Transform;
      for (int i = 0; i < nodes.Length; i++)
      {
        var node = nodes[i]; if ((node.flags & 0x02) != 0) continue;
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
          var lightdir = light != null ? light.GetTransform()[2] : default;
          var shadows = scene.Shadows;
          dc.Light = shadows ? lightdir * 0.3f : lightdir;
          dc.Ambient = scene.ambient;
          dc.State = State.Default3D;
          dc.Transform = Matrix4x4.Identity;
          render(dc, 0, scene.nodes);
          dc.Select();
          if (shadows && !dc.IsPicking)
          {
            var t4 = dc.State; dc.Light = lightdir; dc.LightZero = -5; //todo: calc
            dc.State = State.Shadows3D;
            render(dc, 2, scene.nodes);
            dc.State = t4;
            dc.Ambient = 0;
            dc.Light = lightdir * 0.7f;
            dc.BlendState = BlendState.AlphaAdd;
            render(dc, 1, scene.nodes);
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
            if ((flags & 0x04) != 0 && !dc.IsPicking)
            {
              dc.Color = 0x40000000; var t1 = dc.State;
              dc.BlendState = BlendState.Alpha;
              dc.Rasterizer = Rasterizer.Wireframe;
              for (int i = 0; i < selection.Count; i++)
              {
                var node = selection[i]; if ((node.flags & 0x02) != 0) continue;
                wire(dc, node);
              }
              static void wire(DC dc, Group p)
              {
                if (p is Models.Geometry geo) { geo.checkbuild(0); dc.Transform = p.GetTransform(); dc.DrawMesh(geo.vb, geo.ib); }
                var a = p.VisibleNodes; if (a != null) for (int i = 0; i < a.Length; i++) wire(dc, a[i]);
              }
              dc.State = t1;
            }
            if ((flags & (0x01 | 0x02)) != 0)
            {
              for (int i = 0; i < selection.Count; i++)
              {
                var node = selection[i]; if ((node.flags & 0x02) != 0) continue;
                var box = node.GetBox(); if (box.Min.X == float.MaxValue) box = default;
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
            if ((flags & 0x10) != 0 && selection.Count == 1)
              if (selection[0] is Models.Geometry g && g.Visible)
                g.Render(dc, g);
          }
          if (transp.Count != 0)
          {
            for (int i = 0; i < transp.Count; i++)
            {
              var geo = transp[i]; if ((geo.flags & 0x02) != 0) continue;
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
      if ((flags & 0x20) != 0 || (infos != null && infos.Count != 0) && !dc.IsPicking)
      {
        dc.Projection = Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, -1000, +1000);
        dc.Transform = Matrix4x4.Identity; dc.State = State.Text2D; // State.Default2D;
        dc.Color = 0xff000000; var font = dc.Font; float y = 8f, dy = font.Height;
        if ((flags & 0x20) != 0)
        {
          if (TimerTicks <= 1) { TimerTicks = 1; }// Invalidate(); }
          else test(dc, font, 8, y + font.Ascent, (int)(Stopwatch.Frequency / TimerTicks));
          y += dy;
          unsafe static void test(DC dc, Font font, float x, float y, int v)
          {
            var ss = stackalloc char[100]; var sp = new Span<char>(ss, 100);
            v.TryFormat(sp, out var ns); " fps".CopyTo(sp.Slice(ns)); ns += 4;
            font.Draw(dc, x, y, ss, ns);
          }
        }
        for (int i = 0, n = infos != null ? infos.Count : 0; i < n; i++, y += dy)
          dc.DrawText(8, y + font.Ascent, Infos[i]);
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
            var main = pc.Hover as Group; var keys = ModifierKeys;
            var layer = selection.Count != 0 ? selection[0].parent : scene;
            if (main != null) for (; !IsSelect(main) && main.parent is Group t && t != layer; main = t) ;
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
        case 0x0007: //WM_SETFOCUS
        case 0x0008: //WM_KILLFOCUS
          if (extrasel.p != null) Invalidate();
          break;
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
        case 2050: return OnIntersect(id, test); // Union
        case 2051: return OnIntersect(id, test); // Difference
        case 2052: return OnIntersect(id, test); // Intersection
        case 2053: return OnIntersect(id, test); // Halfspace"
        //case 2054: return OnCheckMash(test);
        //case 2055: return OnConvert(test);
        case 2056: return OnCenter(test);
        case 2057: return OnSelectAll(test);
          //case 2100: //Select Box
          //case 2101: //Select Pivot
          //case 2102: //Select Wireframe
          //case 2103: //Select Normals
          //case 2104:
          //  return OnFlags(test, id);
          //case 3015: return base.OnDriver(test);
          //case 3016: return base.OnSamples(test);
      }
      return 0;
    }
    int OnRedo(object? test)
    {
      if (undos == null || undoi >= undos.Count) return 0;
      if (test == null) { undos[undoi++].exec(); Invalidate(); Focus(); /*UndoChanged?.Invoke();*/ }
      return 1;
    }
    int OnUndo(object? test)
    {
      if (undos == null || undoi == 0) return 0;
      if (test == null) { undos[--undoi].exec(); Invalidate(); Focus(); /*UndoChanged?.Invoke();*/ }
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
      var xml = Models.Save(new Models.Scene { Unit = scene.Unit, nodes = selection.ToArray() });
      Clipboard.SetText(xml.ToString()); return 1;
    }
    int OnPaste(object? test)
    {
      if (!Clipboard.ContainsText()) return 0;
      var s = Clipboard.GetText();
      if (s[0] != '<' || !s.Contains(Models.ns.NamespaceName)) return 0;
      if (test != null) return 1;
      var xml = XElement.Parse(s); var scene = Models.Load(xml); //todo: units
      Execute(new UndoNodes(this.scene, this.scene.nodes.Concat(scene.nodes).ToArray()), undosel(scene.nodes));
      return 1;
    }
    int OnDelete(object? test)
    {
      if (selection.Count == 0 || scene.nodes == null) return 0;
      if (test != null) return 1;
      var layer = selection[0].parent;
      Execute(undosel(null), new UndoNodes(layer, layer.nodes.Except(selection).ToArray()));
      return 1;
    }
    int OnSelectAll(object? test)
    {
      if (test != null) return 1;
      selection.Clear(); selection.AddRange(scene.nodes.Where(p => !p.Fixed && isgeo(p))); Invalidate();
      static bool isgeo(Group p)
      {
        if (p is Models.Geometry) return true;
        if (p.nodes != null) for (int i = 0; i < p.nodes.Length; i++) if (isgeo(p.nodes[i])) return true;
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
      var a = selection.ToArray();
      var g = settings.GroupType == Settings.GroupM.BoolGeometry &&
        a.Length == 2 && a[0] is Models.Geometry ga && a[1] is Models.Geometry ?
        new Models.BoolGeometry() { nodes = a, ranges = new (int count, Models.Material material)[] { (0, ga.ranges[0].material) } } :
        new Group { nodes = a };
      var pm = (box.Min + box.Max) * 0.5f; pm.Z = box.Min.Z;
      var m = Matrix4x3.CreateTranslation(pm); g.Transform = m; m = !m;
      var nodes = this.scene.nodes.Except(selection).Concat(Enumerable.Repeat(g, 1)).ToArray();
      Execute(new UndoNodes(this.scene, nodes),
        selection.Select(p => UndoTrans.get(p, p.Transform * m)), undosel(g));
      return 1;
    }
    int OnUngroup(object? test)
    {
      var groups = selection.Where(p => p.nodes != null);
      if (!groups.Any()) return 0;
      if (test != null) return 1; //var scene = groups.First().Parent;
      var childs = groups.SelectMany(p => p.nodes ?? Array.Empty<Group>()).ToArray();
      var nodes = this.scene.nodes.Except(groups).Concat(childs).ToArray();
      Execute(new UndoNodes(this.scene, nodes),
        childs.Select(p => UndoTrans.get(p, p.Transform * ((Group)p.parent).Transform)), undosel(childs));
      return 1;
    }
    //int OnFlags(object? test, int id)
    //{
    //  var f = 1 << (id - 2100);
    //  if (test != null)
    //  {
    //    if (id < 2108 && selection.Count == 0 && test is ToolStripButton) return 0;
    //    return (flags & f) != 0 ? 3 : 1;
    //  }
    //  flags ^= f; //Application.UserAppDataRegistry.SetValue("fl", flags);
    //  Invalidate(); return 0;
    //}
    int OnCheckMash(object? test)
    {
      if (selection.Count != 1 || selection[0] is not Models.Geometry geo) return 0;
      if (test != null) return 1;
      check(geo); return 1;
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
          var xx = a.parent.nodes.Select(p => p == a ? c : p).ToArray();
          Execute(undosel(a), new UndoNodes(a.parent, xx), undosel(c));
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
        var xx = a.parent.nodes.Select(p => p == a ? c : p == b ? null : p).OfType<Group>().ToArray();
        Execute(undosel(a), new UndoNodes(a.parent, xx), undosel(c));
      }
      Cursor.Current = Cursors.Arrow;
      return 0;
    }
    int OnCenter(object? test)
    {
      if (selection.Count == 0) return 0;
      if (test != null) return 1;
      var cm = camera.GetTransform(scene); var size = ClientSize;
      var pr = Matrix4x4.CreatePerspectiveFieldOfView(camera.Fov * (MathF.PI / 180), (float)size.Width / size.Height, camera.Near, camera.Far);
      cm = Center(pr, cm, selection).m; Execute(UndoTrans.get(camera, cm)); return 1;
    }

    bool IsSelect(Group node) => selection.Contains(node);
    public void Select(object? p, bool toggle = false)
    {
      if (extrasel.view == this) { extrasel = default; Invalidate(); } //props
      if (p == null) { if (selection.Count != 0) { selection.Clear(); Invalidate(); } return; }
      if (p is Group node)
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
            if (selection.Count != 0 && selection[0].parent != node.parent) selection.Clear();
            selection.Add(node);
          }
        }
        Invalidate(); return;
      }
      if (p is IEnumerable e)
      {
        if (e.OfType<Group>().SequenceEqual(selection)) return;
        selection.Clear(); selection.AddRange(e.OfType<Group>());
        Invalidate(); return;
      }
    }
    public void AddUndo(Undo? p)
    {
      if (p == null) return;
      if (undos == null) undos = new List<Undo>();
      undos.RemoveRange(undoi, undos.Count - undoi);
      undos.Add(p); undoi = undos.Count; //UndoChanged?.Invoke();
    }
    public void Execute(Undo a)
    {
      if (a == null) return;
      a.exec(); AddUndo(a); Invalidate();
    }
    public void Execute(params object[] a) //Ani, IEnumerable<Ani>, null
    {
      var b = Undo.join(a); if (b == null) return;
      b.exec(); AddUndo(b); Invalidate();
    }
    [Browsable(false)]
    public bool IsModified
    {
      get => undoi != 0;
      set { undos = null; undoi = 0; }
    }
    Undo undosel(params Group[]? a)
    {
      return new UndoSel(this, a); //return new AniEm(() => { var t = selection.Count != 0 ? selection.ToArray() : null; Select(a); a = t; });
    }

    static int lastwheel;
    void wheel(PC pc)
    {
      if (pc.Hover is not Group) return;
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var m = camera.Transform;
      var v = wp - m.Translation;
      var l = v.Length(); var f = pc.Primitive / 120f;
      camera.Transform = m * Matrix4x3.CreateTranslation(v * (l * f * 0.02f)); pc.Invalidate();
      var t = Environment.TickCount; if (t - lastwheel > 500) { lastwheel = t; AddUndo(UndoTrans.get(camera, m)); }
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
        if (id == 1) AddUndo(UndoTrans.get(camera, m));
      };
    }
    Action<int> tool_cam_vert(PC pc)
    {
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var wm = Matrix4x3.CreateTranslation(wp);
      var cm = camera.Transform;
      wm[0] = Vector3.Cross(wm[1] = new Vector3(0, 0, 1), wm[2] = Vector3.Normalize(cm.Translation - wp));
      pc.SetPlane(wm);
      var p1 = pc.Pick();
      return id =>
      {
        if (id == 0)
        {
          var dz = p1.Y - pc.Pick().Y;
          camera.Transform = cm * Matrix4x3.CreateTranslation(0, 0, dz); pc.Invalidate();
        }
        if (id == 1) AddUndo(UndoTrans.get(camera, cm));
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
        if (id == 1) AddUndo(UndoTrans.get(camera, cm));
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
        if (id == 1) AddUndo(UndoTrans.get(camera, cm));
      };
    }
    Action<int> tool_cam_select(PC pc)
    {
      ////setplane(dc, 0); wp?
      var wp = Vector3.Transform(pc.Point, pc.Transform); //Debug.WriteLine(wp.Z);
      var ma = Matrix4x4.CreateTranslation(0, 0, wp.Z + 0.0005f); pc.SetPlane(ma);
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
          var list = new List<Group>();
          for (int i = 0; i < scene.nodes.Length; i++)
          {
            var node = scene.nodes[i]; if (node.Fixed) continue;
            if (intersect(node, rect)) list.Add(node);
            var box = node.GetBox(); if (!Intersect(box, rect)) continue;
            static bool intersect(Group node, (Vector2 Min, Vector2 Max) rect)
            {
              var a = node.nodes; if (a != null) for (int i = 0; i < a.Length; i++) if (intersect(a[i], rect)) return true;
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

    static Func<int, Matrix4x3, object> move(Group node)
    {
      var m = node.Transform;
      return (id, v) =>
      {
        if (id == 0) { v = node.Transform; node.Transform = m; m = v; node.parent?.Invalidate(); }
        else if (id == 1)
        {
          var t = m * v;
          if (v.M41 != 0) t.M41 = MathF.Round(t.M41, 6);
          if (v.M42 != 0) t.M42 = MathF.Round(t.M42, 6);
          if (v.M43 != 0) t.M43 = MathF.Round(t.M43, 6);
          node.Transform = t; node.parent?.Invalidate();
        }
        else if (id == 7) return UndoTrans.get(node, m);
        return null;
      };
    }
    static Func<int, Matrix4x3, object> move(IEnumerable<Group> nodes)
    {
      var a = nodes.Select(p => move(p)).ToArray();
      return a.Length == 1 ? a[0] : (id, m) =>
      {
        if (id == 7) return new UndoSet(a.Select(p => p(7, m)).OfType<UndoTrans>().ToArray());
        for (int i = 0; i < a.Length; i++) a[i](id, m); return null;
      };
    }
    Action<int> tool_obj_move(PC pc, Group main)
    {
      var ws = IsSelect(main); if (!ws) Select(main);
      var rp = main.Location;
      var mo = default(Func<int, Matrix4x3, object>); //var ani = default(Ani);
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var wm = Matrix4x3.CreateTranslation(wp);
      if (main.parent is Group pn) { var t = pn.GetTransform(); t.M41 = t.M42 = t.M43 = 0; wm = t * wm; }
      //if (main.Parent is Node pn) wm = Matrix4x3.CreateTranslation(Vector3.Transform(wp, !pn.GetTransform())) * pn.GetTransform();
      //var xm = main.Parent is Node pn ? pn.GetTransform() : Matrix4x3.Identity;
      pc.SetPlane(wm); Vector2 p1 = pc.Pick(), p2 = p1; //RenderClient += drawplane; Invalidate();
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
          else if (main.Location != rp) AddUndo(mo(7, default) as Undo);
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
    Action<int> tool_obj_vert(PC pc, Group main)
    {
      if (!IsSelect(main)) Select(main);
      var box = (Min: new Vector3(float.MaxValue), Max: new Vector3(-float.MaxValue));
      foreach (var p in selection) p.GetBox(ref box, p.Transform);
      var mo = default(Func<int, Matrix4x3, object>);
      var rp = main.Location;
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var wm = Matrix4x3.CreateTranslation(wp);
      var cm = camera.GetTransform();
      var up = new Vector3(0, 0, 1);
      if (main.parent is Group pn) up = Vector3.Normalize(pn.GetTransform()[2]);
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
          if (main.Location != rp) AddUndo(mo(7, default) as Undo);
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
    Action<int> tool_obj_rot(PC pc, Group main)
    {
      var ws = IsSelect(main); if (!ws) Select(main);
      var mo = default(Func<int, Matrix4x3, object>);
      var wp = Vector3.Transform(pc.Point, pc.Transform);
      var mw = main.Transform;// GetTransform();
      var mp = mw.Translation;
      var vm = Math.Abs(mw.M13) > 0.8f ? new Vector2(mw.M21, mw.M22) : new Vector2(mw.M11, mw.M12);
      var w0 = MathF.Atan2(vm.Y, vm.X);
      var ag = angelgrid((MathF.PI / 180) * settings.Angelgrid);
      var me = Matrix4x3.CreateTranslation(mp.X, mp.Y, wp.Z);
      if (main.parent is Group pn) { me = pn.GetTransform(); me = Matrix4x3.CreateTranslation(mp.X, mp.Y, Vector3.Transform(wp, !me).Z) * me; }
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
              var t = pc.Hover as Group; for (; t != null && t.parent != main; t = t.parent as Group) ;
              if (t != null && !t.Fixed) Select(t);
            }
          }
          else if (rw != 0) AddUndo(mo(7, default) as Undo);
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
    Action<int> tool_obj_rot_axis(PC pc, Group main, int axis)
    {
      if (!IsSelect(main)) Select(main);
      var mo = default(Func<int, Matrix4x3, object>);
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
      if (main.parent is Group pn) { var t = pn.GetTransform(); me = me * t; }
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
          if (rw != 0) AddUndo(mo(7, default) as Undo);
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
    Action<int> tool_obj_drag(PC pc, Group main)
    {
      var ws = IsSelect(main); if (!ws) Select(main, true);
      var pt = Vector3.Transform(pc.Point, ((Group)pc.Hover).GetTransform(main.parent));
      var p1 = Cursor.Position;
      return id =>
      {
        if (id == 0)
        {
          var p2 = Cursor.Position;
          if (new Vector2(p2.X - p1.X, p2.Y - p1.Y).LengthSquared() < 10) return;
          if (!ws) Select(main); ws = false; if (!AllowDrop) return;
          var png = settings.FileFormat == Settings.XFormat.xxzpng;
          var name = main.Name; if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) name = main.GetType().Name;
          var path = Path.Combine(Path.GetTempPath(), name + ".xxz" + (png ? ".png" : null));
          var xml = Models.Save(new Models.Scene { Unit = scene.Unit, nodes = selection.ToArray() });
          var ppt = pt; xml.SetAttributeValue("pt", Models.format(new Span<float>(&ppt, 3)));
          var data = new DataObject();
          data.SetFileDropList(new System.Collections.Specialized.StringCollection { path });
          try
          {
            if (png)
            {
              using (var bmp = preview(settings.PreviewsSize, 20))
              using (var str = new FileStream(path, FileMode.Create))
              {
                bmp.Save(str, System.Drawing.Imaging.ImageFormat.Png); var x = str.Position;
                using (var zstr = new ZLibStream(str, CompressionLevel.Optimal, true)) xml.Save(zstr);
                str.Write(BitConverter.GetBytes(unchecked((int)x)));
              }
            }
            else xml.Save(path); DoDragDrop(data, DragDropEffects.Copy);
          }
          catch (Exception e) { Debug.WriteLine(e.Message); }
          finally { File.Delete(path); }
        }
        if (id == 1 && ws) { Select(main, true); }
      };
    }
    Action<int>? tool_drop(PC pc)
    {
      static Group[] load(DataObject data, out Vector3 pt)
      {
        var list = data.GetFileDropList(); pt = default;
        if (list != null && list.Count == 1)
        {
          var path = list[0];
          if (path.EndsWith(".xxz", true, null)) return loadx(XElement.Load(path), ref pt);
          if (path.EndsWith(".png", true, null) && path.Contains(".xxz")) using (var str = new FileStream(path, FileMode.Open)) return loadxpng(str, ref pt);
          //else return null;
        }
        if (data.GetDataPresent("UniformResourceLocatorW"))
        {
          var rst = data.GetData("UniformResourceLocatorW") as MemoryStream; if (rst == null) return null;
          var txt = System.Text.Encoding.Unicode.GetString(rst.ToArray());
          var par = txt.Split('\0'); if (par.Length == 0) return null;
          var uri = new Uri(par[0]); var path = uri.LocalPath;
          if (!path.EndsWith(".png", true, null)) return null;
          if (path.EndsWith(".png", true, null) && path.Contains(".xxz"))
          {
            if (uri.IsFile) using (var str = new FileStream(uri.LocalPath, FileMode.Open)) return loadxpng(str, ref pt);
            var client = GetHttpClient();
            using (var task = client.GetByteArrayAsync(uri))
            {
              task.Wait();
              if (task.Status == TaskStatus.RanToCompletion)
                using (var str = new MemoryStream(task.Result)) return loadxpng(str, ref pt);
            }
          }
          var tex = GetTexture(uri.AbsoluteUri); var ts = tex.Size; var os = ts / 100;
          var box = new Models.BoxGeometry { Transform = Matrix4x3.Identity, Max = new Vector3(os, 0.01f) };
          box.ranges = new (int, Models.Material)[] { (0, new Models.Material {
            Diffuse = 0xffffffff, Texture = tex,
            Transform = Matrix4x3.CreateScale(new Vector3(1 / os.X, -1 / os.Y, 1)) } )};
          return new Group[] { box };
        }
        return null;
        static Group[] loadxpng(Stream str, ref Vector3 pt)
        {
          str.Seek(-4, SeekOrigin.End); var c = new byte[4]; str.Read(c); var x = BitConverter.ToInt32(c);
          str.Seek(x, SeekOrigin.Begin);
          using (var zstr = new ZLibStream(str, CompressionMode.Decompress))
            return loadx(XElement.Load(zstr), ref pt);
        }
        static Group[] loadx(XElement xml, ref Vector3 pt)
        {
          var s = (string)xml.Attribute("pt");
          if (s != null) pt = SpanTools.readv3(s, null);
          return Models.Load(xml).nodes;
        }
      }


      var data = pc.View.Tag as DataObject; if (data == null || this.scene.nodes == null) return null;
      var nodes = load(data, out var pt); if (nodes == null) return null;
      var rp = nodes[nodes.Length - 1].Location;
      var mover = move(nodes); var oldnodes = this.scene.nodes;
      this.scene.nodes = this.scene.nodes.Concat(nodes).ToArray();
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
          var sel = undosel(nodes); sel.exec(); pc.Invalidate();
          AddUndo(Undo.join(sel, new UndoNodes(this.scene, oldnodes))); Focus();
        }
        else if (id == 2) { this.scene.nodes = oldnodes; pc.Invalidate(); }
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
    static (Matrix4x3 m, Vector2 nf) Center(in Matrix4x4 proj, in Matrix4x3 cm, IList<Group> nodes)
    {
      var u = new Vector2(proj.M11, proj.M22);
      var c = (min: new Vector3(+float.MaxValue), max: new Vector3(-float.MaxValue), new Vector2(-1) / u);
      recu(ref c, nodes, !cm);
      var v = (c.max - c.min) * 0.5f; var z = MathF.Max(v.Y * u.Y, v.X * u.X);
      return (Matrix4x3.CreateTranslation(c.min.X + v.X, c.min.Y + v.Y, z) * cm, new Vector2(c.min.Z - z, c.max.Z - z));
      static void recu(ref (Vector3 min, Vector3 max, Vector2 s) c, IList<Group> nodes, in Matrix4x3 pm)
      {
        for (int i = 0, n = nodes.Count; i < n; i++)
        {
          var node = nodes[i]; var m = node.Transform * pm;
          if (node.nodes != null) recu(ref c, node.nodes, m);
          if (node is not Models.Geometry g) continue;
          var a = g.vertices; if (a == null) continue;
          for (int t = 0; t < a.Length; t++)
          {
            var p = Matrix4x3.Transform(a[t], m);
            var q = new Vector3(c.s * p.Z, 0);
            c.min = Vector3.Min(c.min, p + q);
            c.max = Vector3.Max(c.max, p - q);
          }
        }
      }
    }
    Bitmap preview(Size size, float fov)
    {
      return Print(size.Width, size.Height, 4, 0, dc =>
      {
        var cm = camera.Transform; // GetTransform(scene); 
        var pr = Matrix4x4.CreatePerspectiveFieldOfView(fov * (MathF.PI / 180), (float)size.Width / size.Height, camera.Near, camera.Far);
        var tm = Center(pr, cm, selection).m;
        var a = scene.nodes; var b = ArrayPool<int>.Shared.Rent(a.Length);
        for (int i = 0; i < a.Length; i++) { b[i] = a[i].flags; a[i].flags |= 0x02; }
        for (int i = 0; i < selection.Count; i++) selection[i].flags &= ~0x02;
        var t3 = this.flags; this.flags &= ~(0x01 | 0x02 | 0x04 | 0x08 | 0x10 | 0x20);
        camera.Transform = tm; var t1 = camera.Fov; camera.Fov = fov; // var t4 = RenderClient; RenderClient = null;
        var t2 = infos; infos = null; OnRender(dc);
        camera.Transform = cm; camera.Fov = t1; infos = t2; this.flags = t3; // RenderClient = t4;
        for (int i = 0; i < a.Length; i++) { a[i].flags = (a[i].flags & ~0x02) | (b[i] & 0x02); }
        ArrayPool<int>.Shared.Return(b);
      });
    }

    #region undo
    public abstract class Undo
    {
      internal abstract void exec();
      internal static Undo? join(params object[] a) // Ani, IEnumerable<Ani>, null
      {
        static Undo? join(object[] p)
        {
          var a = p.Select(p =>
            p is Undo a ? a :
            p is IEnumerable<Undo> e ? join(e.ToArray()) :
            null).OfType<Undo>().ToArray();
          if (a.Length == 0) return null;
          if (a.Length == 1) return a[0];
          return new UndoSet(a);
        }
        return join(a);
      }
      internal virtual (AniLine? line, int wo) record(AniSet set, int time) => default;
      protected (AniLine? line, int wo) getline(AniSet set, int time, Type type, Node target, object? prop)
      {
        var lines = set.lines; AniLine t = null;
        for (int i = 0, x = 0; i < lines.Count; i++)
          if ((t = lines[i]).GetType() == type && t.disp(0, 0) == target && t.disp(1, 0) == prop)
          {
            if (time != -1)
            {
              for (; x < t.times.Count && t.times[x] + t.times[x + 1] <= time; x += 2) ;
              t.times.Insert(x, time); t.times.Insert(x + 1, 250);
              if (t.gammas != null && (x >> 1) < t.gammas.Count) t.gammas.Insert(x >> 1, 0);
            }
            return (t, 0x40000000 | (i << 16) | x); // (t.times.Count - 2));
          }
        if (time == -1) return default;
        t = (AniLine)Activator.CreateInstance(type);
        t.times.Add(time); t.times.Add(250); lines.Add(t);
        return (t, 0x40000000 | ((lines.Count - 1) << 16));
      }
    }
    public abstract class AniLine
    {
      //0:tag 1:prop 2:save 3:load 5:cap x 8:del x 9:gamma x
      internal virtual object? disp(int id, int wp = 0, object? lp = null)
      {
        switch (id)
        {
          case 2: //save
            {
              var e = new XElement(Models.o2s(this));
              var f = (Func<Node, string?>)lp;
              e.SetAttributeValue("pid", f((Node)disp(0, 0))); return e;
            }
          case 3: //load
            {
              var e = (XElement)lp; var f = e.Annotation<Func<string, Node?>>();
              var a = e.Attribute("pid"); return a != null ? f(a.Value) : null;
            }
          case 8: //del time
            {
              var x = wp >> 1; Debug.Assert(times.Count > 2);
              times.RemoveRange(x << 1, 2);
              if (gammas != null)
              {
                if (x < gammas.Count) gammas.RemoveAt(x);
                for (int t; gammas.Count != 0 && gammas[t = gammas.Count - 1] == 0; gammas.RemoveAt(t)) ;
                if (gammas.Count == 0) gammas = null;
              }
            }
            break;
          case 9: //get/set gamma
            {
              var x = wp;
              if (lp == null) return gammas != null && x < gammas.Count ? gammas[x] : 0;
              var g = (float)lp;
              if (x >= (gammas ??= new()).Count) gammas.AddRange(Enumerable.Repeat(0f, x + 1 - gammas.Count));
              gammas[x] = g;
              while (gammas[gammas.Count - 1] == 0) gammas.RemoveAt(gammas.Count - 1);
              if (gammas.Count == 0) gammas = null;
            }
            break;
        }
        return null;
      }
      internal readonly List<int> times = new(2); internal List<float>? gammas;

      internal bool ani(int t1, int t2)
      {
        var inf = false;
        for (int k, t, dt, n = times.Count, i = 0; i < n; i += 2)
        {
          if (t1 <= t2)
          {
            if ((t = times[k = i]) > t2) break;
            if (t + (dt = times[i + 1]) < t1) continue;
          }
          else
          {
            if ((t = times[k = n - i - 2]) > t1) continue;
            if (t + (dt = times[k + 1]) < t2) break;
          }
          float f = (float)(t2 - t) / dt, g; k >>= 1;
          if (f <= 0) f = 0;
          else if (f >= 1) f = 1;
          else
          {
            if (gammas != null && k < gammas.Count && (g = gammas[k]) != 0)
              f = ((1 / MathF.Atan(g)) * MathF.Atan(g * (2 * f - 1)) + 1) * 0.5f;
          }
          inf |= lerp(k, f);
        }
        return inf;
      }
      internal abstract bool lerp(int x, float f);
      protected internal virtual void serial(XElement e, Node? load)
      {
        var fi = NumberFormatInfo.InvariantInfo;
        if (load == null)
        {
          var s = stackspan<char>(); var w = s;
          var n = times.Count;
          for (int i = 0; i < n; i++)
          {
            if (i != 0) { w[0] = ' '; w = w.Slice(1); }
            times[i].TryFormat(w, out var c, default, fi); w = w.Slice(c);
          }
          var ts = s.Slice(0, s.Length - w.Length); if (n == 0) n = 2;
          if (ts.Length != 0) e.SetAttributeValue("times", ts.ToString()); w = s; 
          for (int i = 0, l = (n >> 1) + 1; i < l; i++)
          {
            if (i != 0) { w[0] = ';'; w[1] = ' '; w = w.Slice(2); }
            w = w.Slice(write(i, w, fi));
          }
          ts = s.Slice(0, s.Length - w.Length);
          e.SetAttributeValue("line", ts.ToString());
          if (gammas != null)  //check: check zeros
          {
            w = s;
            for (int i = 0; i < gammas.Count; i++)
            {
              if (i != 0) { w[0] = ' '; w = w.Slice(1); }
              gammas[i].TryFormat(w, out var c, default, fi); w = w.Slice(c);
            }
            ts = s.Slice(0, s.Length - w.Length);
            e.SetAttributeValue("gammas", ts.ToString());
          }
        }
        else
        {
          var a = e.Attribute("times");
          var s = a != null ? a.Value.AsSpan().Trim() : default;
          while (s.Length != 0) { var t = Models.token(ref s); times.Add(int.Parse(t)); }
          if ((a = e.Attribute("line")) != null) s = a.Value.AsSpan().Trim(); else return; //aninodes
          while (token(ref s, ';', out var b)) read(b, fi);
          if ((a = e.Attribute("gammas")) != null)
          {
            gammas = new(); s = a.Value.AsSpan().Trim();
            while (s.Length != 0) { var t = Models.token(ref s); gammas.Add(float.Parse(t, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, fi)); }
          }
          static bool token(ref ReadOnlySpan<char> a, char c, out ReadOnlySpan<char> b)
          {
            if (a.Length == 0) { b = a; return false; }
            var x = a.IndexOf(';'); if (x == -1) { b = a; a = default; return true; }
            b = a.Slice(0, x).TrimEnd(); a = a.Slice(x + 1).TrimStart(); return true;
          }
        }
      }
      protected abstract int write(int i, Span<char> s, NumberFormatInfo f);
      protected abstract void read(ReadOnlySpan<char> s, NumberFormatInfo f);
    }
    public class AniSet
    {
      internal readonly List<AniLine> lines = new();
      internal string? name;
      internal int time, maxtime; int lt;
      internal int getendtime()
      {
        var max = 0;
        for (int i = 0; i < lines.Count; i++)
        {
          var t = lines[i].times;
          max = Math.Max(max, t[t.Count - 2] + t[t.Count - 1]);
        }
        return max;
      }
      internal bool ani(int t)
      {
        lt = time; time = t; var inf = false; //todo: ref
        for (int n = lines.Count, i = 0; i < n; i++)
          inf |= lines[i].ani(lt, time);
        return inf;
      }
      internal object? disp(int id, object? v)
      {
        switch (id)
        {
          case 2:
            {
              var e = new XElement(Models.ns + "aniset"); var getid = (Func<Node, string?>)v;
              if (name != null) e.SetAttributeValue("name", name);
              if (time != 0) e.SetAttributeValue("time", time);
              for (int i = 0; i < lines.Count; i++) e.Add((XElement)lines[i].disp(id, 0, getid));
              return e;
            }
          case 3:
            {
              var e = (XElement)v; var f = e.Annotation<Func<string, Node?>>(); var a = default(XAttribute);
              if ((a = e.Attribute("name")) != null) name = a.Value; //todo: trim
              if ((a = e.Attribute("time")) != null) time = (int)a;
              foreach (var ani in e.Elements())
              {
                var type = Models.s2t(ani.Name.LocalName); if (type == null) continue;
                var line = (AniLine)Activator.CreateInstance(type);
                ani.AddAnnotation(f); line.disp(3, 0, ani); lines.Add(line);
              }
              return this;
            }
          default: return null;
        }
      }
    }
    sealed class UndoTrans : Undo
    {
      readonly Group p; Matrix4x3 m;
      internal UndoTrans(Group p, in Matrix4x3 m) { this.p = p; this.m = m; }
      internal override void exec() { var t = p.Transform; p.Transform = m; m = t; }
      internal static UndoTrans? get(Group p, in Matrix4x3 m) => p.Transform != m ? new UndoTrans(p, m) : null;
      internal override (AniLine? line, int wo) record(AniSet set, int time)
      {
        var c = getline(set, time, typeof(Line), this.p, null);
        if (time == -1) return c; var line = (Line)c.line;
        if (line.p == null) { line.p = this.p; line.list.Add(m); }
        line.list.Insert(((c.wo & 0xffff) >> 1) + 1, p.Transform); return c;
      }
      internal class Line : AniLine
      {
        internal override object? disp(int id, int wp, object? v)
        {
          switch (id)
          {
            case 0: return p;
            case 2: { var e = (XElement)base.disp(id, wp, v); base.serial(e, null); return e; }
            case 3: serial((XElement)v, p = (Group)base.disp(id, wp, v)); return this;
            case 5: { var x = wp; list[x + 1] = list[x]; list[x] = p.Transform; } break; //splitt trans
            case 8: list.RemoveAt(wp == 0 ? 0 : (wp >> 1) + 1); break; //del trans
          }
          return base.disp(id, wp, v);
        }
        internal Group? p; internal readonly List<Quat> list = new(2);
        protected internal override void serial(XElement e, Node? load)
        {
          if (load != null) p = (Group)load;
          base.serial(e, load);
        }
        protected override int write(int i, Span<char> w, NumberFormatInfo f)
        {
          return list[i].Format(w, f);
        }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f)
        {
          list.Add(Quat.Parse(s, f));
        }
        internal override bool lerp(int x, float f)
        {
          var m =
            f <= 0 ? (Matrix4x3)list[x + 0] :
            f >= 1 ? (Matrix4x3)list[x + 1] :
            Quat.Slerp(list[x + 0], list[x + 1], f);
          if (m == p.Transform) return false;
          p.Transform = m; return true;
        }
      }
    }
    sealed class UndoProp : Undo
    {
      Node p; string s; object v;
      internal UndoProp(Node p, string s, object v) { this.p = p; this.s = s; this.v = v; }
      internal override void exec()
      {
        var pd = p.GetType().GetProperty(s); var t = pd.GetValue(p); pd.SetValue(p, v); v = t;
      }
      static Type? t2t(Type pt)
      {
        if (pt == typeof(Color)) return typeof(ColorLine);
        if (pt == typeof(float)) return typeof(FloatLine);
        if (pt == typeof(Vector2)) return typeof(Vector2Line);
        if (pt == typeof(Vector3)) return typeof(Vector3Line);
        if (pt == typeof(bool)) return typeof(BoolLine);
        if (pt == typeof(string)) return typeof(StringLine);
        if (pt == typeof(int) || pt.IsEnum) return typeof(IntLine);
        return null;
      }
      internal override (AniLine? line, int wo) record(AniSet set, int time)
      {
        var pd = this.p.GetType().GetProperty(s);
        var lt = t2t(pd.PropertyType); if (lt == null) return default;
        var c = getline(set, time, lt, this.p, s); if (time == -1) return c;
        c.line.disp(c.wo, 0, this); return c;
      }
      static List<Type> tc = new();
      static void prechache(Type t) //init internal caches, speed and keep props order
      {
        if (t == typeof(Node) || tc.Contains(t)) return;
        prechache(t.BaseType); var a = t.GetProperties(); tc.Add(t); //Debug.WriteLine("prechache " + t);
      }
      internal abstract class PropLine<T> : AniLine
      {
        internal override object? disp(int id, int wp, object? v = null)
        {
          switch (id)
          {
            case 0: return p;
            case 1: return acc.get.Method.Name;// wp == 0 ? acc.get.Method.Name : wp == 1 ? "⍾" : default;
            case 2: { var e = (XElement)base.disp(id, wp, v); serial(e, null); return e; }
            case 3: serial((XElement)v, p = (Node)base.disp(id, 0, v)); return this;
            case 5: { var x = wp; list[x + 1] = list[x]; list[x] = acc.get(p); } break;
            case 8: list.RemoveAt(wp == 0 ? 0 : (wp >> 1) + 1); break; //del prop
          }
          if ((id & 0x40000000) != 0 && v is UndoProp a)
          {
            if (p == null) { p = a.p; setacc(a.s); list.Add((T)a.v); }
            list.Insert(((id & 0xffff) >> 1) + 1, acc.get(p)); return p;
          }
          return base.disp(id, wp, v);
        }
        Node? p; PropAcc<T>? acc;
        internal readonly List<T> list = new(2);
        protected abstract bool equals(T a, T b);
        protected abstract T lerp(T a, T b, float f);
        internal override bool lerp(int x, float f)
        {
          var a = acc.get(p); var b = lerp(list[x + 0], list[x + 1], f);
          if (equals(a, b)) return false;
          acc.set(p, b); return true;
        }
        protected internal override void serial(XElement e, Node? load)
        {
          if (load == null) e.SetAttributeValue("prop", ((string)this.disp(1, 0)).ToLower());
          else { this.p = load; var a = e.Attribute("prop"); setacc(a.Value); }
          base.serial(e, load);
        }
        void setacc(string s)
        {
          var t = this.p.GetType(); prechache(t);
          var p = t.GetProperty(s, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
          if (p.DeclaringType != p.ReflectedType) p = p.DeclaringType.GetProperty(p.Name);
          acc = GetPropAcc<T>(p);
        }
      }
      internal sealed class ColorLine : PropLine<Color>
      {
        protected override bool equals(Color a, Color b) => a == b;
        protected override Color lerp(Color a, Color b, float f)
        {
          if (f <= 0) return a; if (f >= 1) return b;
          //var v = Vector4.Lerp(new Vector4(a.A, a.R, a.G, a.B), new Vector4(b.A, b.R, b.G, b.B), f);
          //return Color.FromArgb((int)v.X, (int)v.Y, (int)v.Z, (int)v.W);
          var v1 = new Vector3(a.R, a.G, a.B); var u1 = new Vector2(v1.Length(), a.A);
          var v2 = new Vector3(b.R, b.G, b.B); var u2 = new Vector2(v2.Length(), b.A);
          var q1 = Quaternion.Normalize(new Quaternion(v1, 1));
          var q2 = Quaternion.Normalize(new Quaternion(v2, 1));
          var q3 = Quaternion.Slerp(q1, q2, f);
          var v3 = Vector2.Lerp(u1, u2, f);
          var v4 = Vector4.Min(new Vector4(new Vector3(q3.X, q3.Y, q3.Z) * v3.X, v3.Y), new Vector4(255));
          return Color.FromArgb((int)v4.W, (int)v4.X, (int)v4.Y, (int)v4.Z);
        }
        protected override int write(int i, Span<char> w, NumberFormatInfo f)
        {
          ((uint)list[i].ToArgb()).TryFormat(w, out var c, "X8"); return c;
        }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f)
        {
          list.Add(Color.FromArgb(unchecked((int)uint.Parse(s, NumberStyles.HexNumber))));
        }
      }
      internal sealed class FloatLine : PropLine<float>
      {
        protected override bool equals(float a, float b) => a == b;
        protected override float lerp(float a, float b, float f) => a + (b - a) * f;
        protected override int write(int i, Span<char> w, NumberFormatInfo f)
        {
          list[i].TryFormat(w, out var c, default, f); return c;
        }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f)
        {
          list.Add(float.Parse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, f));
        }
      }
      internal sealed class Vector2Line : PropLine<Vector2>
      {
        protected override bool equals(Vector2 a, Vector2 b) => a == b;
        protected override Vector2 lerp(Vector2 a, Vector2 b, float f) => Vector2.Lerp(a, b, f);
        protected override int write(int i, Span<char> w, NumberFormatInfo f)
        {
          return SpanTools.write(list[i], w, f);
        }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f)
        {
          list.Add(SpanTools.readv2(s, f));
        }
      }
      internal sealed class Vector3Line : PropLine<Vector3>
      {
        protected override bool equals(Vector3 a, Vector3 b) => a == b;
        protected override Vector3 lerp(Vector3 a, Vector3 b, float f) => Vector3.Lerp(a, b, f);
        protected override int write(int i, Span<char> w, NumberFormatInfo f)
        {
          return SpanTools.write(list[i], w, f);
        }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f)
        {
          list.Add(SpanTools.readv3(s, f));
        }
      }
      internal sealed class BoolLine : PropLine<bool>
      {
        protected override bool equals(bool a, bool b) => a == b;
        protected override bool lerp(bool a, bool b, float f) => f <= 0 ? a : b;
        protected override int write(int i, Span<char> w, NumberFormatInfo f)
        {
          w[0] = list[i] ? '1' : '0'; return 1;
        }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f)
        {
          list.Add(s[0] != '0');
        }
      }
      internal sealed class StringLine : PropLine<string>
      {
        protected override bool equals(string a, string b) => a == b;
        protected override string lerp(string a, string b, float f) => f <= 0 ? a : b;
        protected override int write(int i, Span<char> w, NumberFormatInfo f)
        {
          //var s = list[i].Replace(';', '\t');
          var s = System.Xml.XmlConvert.EncodeName(list[i]);
          s.TryCopyTo(w); return s.Length;
        }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f)
        {
          //var a = s.ToString(); a=a.Replace('\t', ';'); list.Add(a);
          list.Add(System.Xml.XmlConvert.DecodeName(s.ToString()));
        }
      }
      internal sealed class IntLine : PropLine<int>
      {
        protected override bool equals(int a, int b) => a == b;
        protected override int lerp(int a, int b, float f) => f <= 0 ? a : b;
        protected override int write(int i, Span<char> w, NumberFormatInfo f)
        {
          list[i].TryFormat(w, out var c); return c;
        }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f)
        {
          list.Add(int.Parse(s));
        }
      }
    }
    sealed class UndoNodes : Undo
    {
      Node p; Group[]? b;
      internal UndoNodes(Node p, Group[]? b)
      {
        this.p = p; this.b = b;
      }
      internal override void exec()
      {
        if (b != null) for (int i = 0, n = b.Length; i < n; i++) b[i].parent = p;
        var t = p.nodes; p.nodes = b; b = t;
      }
      internal override (AniLine? line, int wo) record(AniSet set, int time)
      {
        var c = getline(set, time, typeof(Line), this.p, null);
        if (time == -1) return c; var line = (Line)c.line;
        if (line.p == null) { line.p = this.p; line.list.Add(this.b); }
        line.list.Insert(((c.wo & 0xffff) >> 1) + 1, this.p.nodes); return c;
      }
      internal class Line : AniLine
      {
        internal Node? p; internal readonly List<Group[]> list = new(2);
        internal override object? disp(int id, int wp, object? v)
        {
          switch (id)
          {
            case 0: return p;
            case 2: //save
              {
                var e = (XElement)base.disp(id, 0, v); serial(e, null); //todo: skip line output  line="; " 
                var f = (Func<Node, string?>)v;
                e.SetAttributeValue("line", string.Join("; ", list.Select(y => string.Join(' ', y.Select(x => f(x))))));
                return e;
              }
            case 3: //load
              {
                var e = (XElement)v; var f = e.Annotation<Func<string, Node?>>();
                serial((XElement)v, p = (Node)base.disp(id, 0, v)); //todo: skip line read
                list.AddRange(((string)e.Attribute("line")).Split(';').
                  Select(y => y.Split(' ', StringSplitOptions.RemoveEmptyEntries).
                  Select(x => f(x)).OfType<Group>().ToArray()));
                return this;
              }
            case 5: { var x = wp; list[x + 1] = list[x]; list[x] = p.nodes; } break;
            case 8: list.RemoveAt(wp >> 1); break; //del nodes
          }
          return base.disp(id, wp, v);
        }
        protected override int write(int i, Span<char> w, NumberFormatInfo f)
        {
          return 0;
        }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f)
        {
          //list.Add(Quat.Parse(s, f));
        }
        internal override bool lerp(int x, float f)
        {
          var a = f <= 0 ? list[x + 0] : list[x + 1];
          if (p.nodes == a) return false; //var t = p.nodes;
          //Debug.Assert(!p.nodes.SequenceEqual(a)); //if (Enumerable.SequenceEqual(p.nodes, a)) { return false; }          
          var v = (DX11ModelCtrl)p.GetService(typeof(DX11ModelCtrl));
          foreach (var t in p.nodes.Except(a)) if (v.IsSelect(t)) v.Select(t, true);
          foreach (var t in a.Except(p.nodes)) if (!v.IsSelect(t)) v.Select(t, true);
          p.nodes = a;
          return true;
        }
      }
    }
    sealed class UndoSet : Undo
    {
      internal UndoSet(Undo[] a) => this.a = a; readonly Undo[] a;
      internal override void exec()
      {
        for (int i = 0; i < a.Length; i++) a[i].exec(); Array.Reverse(a);
      }
      internal override (AniLine? line, int wo) record(AniSet set, int time)
      {
        var recu = set.name == string.Empty;
        var coll = recu ? set : new AniSet() { name = string.Empty };
        for (int i = 0; i < a.Length; i++) a[i].record(coll, 0);
        if (recu) return default;
        var scene = default(Models.Scene);
        coll.name = null;
        for (int i = 0; i < coll.lines.Count; i++)
        {
          var b = coll.lines[i]; //b.times[1] = 0; //
          b.times.Clear(); b.times.TrimExcess();
          if (scene == null && b.disp(0) is Node x)
            scene = x.GetService(typeof(Models.Scene)) as Models.Scene;
        }
        var c = getline(set, time, typeof(Line), scene, null);
        if (time == -1) return c; var line = (Line)c.line;
        if (line.p == null) { line.p = scene; line.list.Add(coll); }
        else line.list.Insert(((c.wo & 0xffff) >> 1), coll); return c;
        //line.list.Insert(((c.wo & 0xffff) >> 1) + 1, coll); return c;
      }
      internal class Line : AniLine
      {
        internal Models.Scene? p; internal readonly List<AniSet> list = new(2);
        internal override object? disp(int id, int wp, object? v)
        {
          switch (id)
          {
            case 0: return p;
            case 2: //save
              {
                var e = (XElement)base.disp(id, 0, v); //pid
                base.serial(e, null); e.SetAttributeValue("line", null); //times, line
                var f = (Func<Node, string?>)v;
                for (int i = 0; i < list.Count; i++) e.Add(list[i].disp(2, f) as XElement);
                return e;
              }
            case 3: //load
              {
                p = (Models.Scene)base.disp(id, 0, v);
                var e = (XElement)v; base.serial(e, p); //times
                var f = e.Annotation<Func<string, Node?>>();
                foreach (var c in e.Elements())
                {
                  var a = new AniSet(); c.AddAnnotation(f); a.disp(3, c);
                  list.Add(a);
                }
                return this;
              }
            case 5:
              {
                //{ var x = (int)(lid >> 32); list[x + 1] = list[x]; list[x] = p.Transform; }
                throw new Exception("todo"); //todo: catch before   
              }
            case 8: list.RemoveAt(wp >> 1); break; //del set
          }
          return base.disp(id, wp, v);
        }
        protected internal override void serial(XElement e, Node? load) { base.serial(e, load); }
        protected override int write(int i, Span<char> w, NumberFormatInfo f) { return 0; }
        protected override void read(ReadOnlySpan<char> s, NumberFormatInfo f) { }
        internal override bool lerp(int x, float f)
        {
          f = f <= 0 ? 0 : 1; //if (trig == f) return false; trig = f;
          var aniset = list[x]; var inf = false;
          for (int i = 0; i < aniset.lines.Count; i++)
          {
            var p = aniset.lines[i]; inf |= p.lerp(0, f);
          }
          return inf;
        }
      }
    }
    sealed class UndoSel : Undo
    {
      DX11ModelCtrl p; Group[]? a;
      internal UndoSel(DX11ModelCtrl p, Group[]? a) { this.p = p; this.a = a; }
      internal override void exec()
      {
        var t = p.selection.Count != 0 ? p.selection.ToArray() : null; p.Select(a); a = t;
      }
    }
    sealed class UndoAction : Undo
    {
      internal UndoAction(Action p) => this.p = p; Action p;
      internal override void exec() => p();
    }
    #endregion

    AniSet? running;
    void animate()
    {
      if (running.ani(Math.Min(running.time + 30, running.maxtime))) Invalidate();
      if (running.time == running.maxtime) RunningAnimation = null;
    }
    internal AniSet? RunningAnimation
    {
      get => running;
      set
      {
        if (value == running) return;
        if (running != null) { Animations -= animate; running = null; }
        if (value == null) return;
        if (value.maxtime == 0) value.maxtime = value.getendtime();
        running = value; Animations += animate;
      }
    }

    public class Settings
    {
      internal Settings(DX11ModelCtrl p) { this.p = p; }
      readonly DX11ModelCtrl p; float raster = 0.001f, angelgrid = 0.01f;
      [Category("\t\t\tSelection"), DisplayName("Bounding Box"), DefaultValue(true)]
      public bool sel_box
      {
        get => (p.flags & 0x01) != 0; set { p.flags = (p.flags & ~0x01) | (value ? 0x01 : 0); p.Invaliated(); }
      }
      [Category("\t\t\tSelection"), DisplayName("Pivot Coords"), DefaultValue(true)]
      public bool sel_pivot
      {
        get => (p.flags & 0x02) != 0; set { p.flags = (p.flags & ~0x02) | (value ? 0x02 : 0); p.Invaliated(); }
      }
      [Category("\t\t\tSelection"), DisplayName("Wireframe"), DefaultValue(false)]
      public bool sel_wire
      {
        get => (p.flags & 0x04) != 0; set { p.flags = (p.flags & ~0x04) | (value ? 0x04 : 0); p.Invaliated(); }
      }
      [Category("\t\t\tSelection"), DisplayName("Normals"), DefaultValue(false)]
      public bool sel_normals
      {
        get => (p.flags & 0x08) != 0; set { p.flags = (p.flags & ~0x08) | (value ? 0x08 : 0); p.Invaliated(); }
      }
      [Category("\t\t\tSelection"), DisplayName("Tool Points"), DefaultValue(true)]
      public bool sel_points
      {
        get => (p.flags & 0x10) != 0; set { p.flags = (p.flags & ~0x10) | (value ? 0x10 : 0); p.Invaliated(); }
      }

      [Category("\t\tModel Tools"), DefaultValue(0.001f)]
      public float Raster
      {
        get => raster;
        set { raster = value; }
      }
      [Category("\t\tModel Tools"), DefaultValue(0.01f)]
      public float Angelgrid
      {
        get => angelgrid;
        set { angelgrid = value; }
      }

      [Category("\tDrag Tool"), DefaultValue(XFormat.xxz)]
      public XFormat FileFormat { get; set; }
      [Category("\tDrag Tool"), DefaultValue(typeof(Size), "64, 64")]
      public Size PreviewsSize { get; set; } = new Size(64, 64);

      [Category("\tGroup Command"), DefaultValue(GroupM.Group)]
      public GroupM GroupType { get; set; }
      [Category("\tGroup Command"), DefaultValue(GroupC.CenterXY)]
      public GroupC GroupCenter { get; set; }

      [Category("Display Driver"), TypeConverter(typeof(DrvConv))]
      public uint Driver
      {
        get => unchecked((uint)p.OnDriver(null));
        set => p.OnDriver(value);
      }
      [Category("Display Driver"), TypeConverter(typeof(DrvConv))]
      public int Multisamples
      {
        get => p.OnSamples(null);
        set => p.OnSamples(value);
      }
      [Category("Display Driver"), DisplayName("Show Fps"), DefaultValue(false)]
      public bool show_fps
      {
        get => (p.flags & 0x20) != 0; set { p.flags = (p.flags & ~0x20) | (value ? 0x20 : 0); p.Invaliated(); }
      }

      [Category("Registry"), DefaultValue(false)]
      public bool SaveSettings { get; set; }

      public enum GroupM { Group, BoolGeometry }
      public enum GroupC { CenterXY }
      public enum XFormat { xxz, xxzpng }
      class DrvConv : TypeConverter
      {
        ToolStripMenuItem? m1, m2; int drv;
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => true;
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => true;
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
          var m = value is uint ? m1 : m2;
          if (m == null)
          {
            var view = ((Settings)context.Instance).p; m = new ToolStripMenuItem();
            if (value is uint) view.OnDriver(m1 = m); else view.OnSamples(m2 = m);
          }
          return m.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(p => value.Equals(p.Tag))?.Text;
        }
        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
          var x = ((Settings)context.Instance).p.OnDriver(null);
          if (x != drv) { drv = x; m2 = null; ConvertTo(context, null, 0, GetType()); }
          return new StandardValuesCollection(
            (context.PropertyDescriptor.PropertyType == typeof(uint) ? m1 : m2)?.
            DropDownItems.OfType<ToolStripMenuItem>().Select(p => p.Tag).Distinct().ToArray());
        }
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
          return (context.PropertyDescriptor.PropertyType == typeof(uint) ? m1 : m2).
            DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(p => p.Text.Equals(value))?.Tag;
        }
      }
    }
  }

  //Models
  public unsafe partial class DX11ModelCtrl
  {
    public abstract class Node
    {
      internal int flags; // 0x01: Fixed 0x02:!visible 0x04:buildok 0x08:vbok 0x10:merge 0x20:shadows
      internal string? name;
      internal Group[]? nodes;
      internal Node? parent;
      internal protected virtual unsafe void Serialize(XElement e, bool storing)
      {
        e.AddAnnotation(this);
        if (storing)
        {
          if (name != null) e.SetAttributeValue("name", name);
          if (nodes != null)
            for (int i = 0; i < nodes.Length; i++)
            {
              var p = nodes[i]; var c = new XElement(Models.ns + Models.o2s(p));
              e.Add(c); p.Serialize(c, true);
            }
        }
        else
        {
          XAttribute a;
          if ((a = e.Attribute("name")) != null) name = a.Value;
          for (int i = 0, n = 0; i < 2; i++)
          {
            if (i == 1) { if (n == 0) break; nodes = new Group[n]; n = 0; }
            for (var p = e.FirstNode; p != null; p = p.NextNode)
            {
              if (!(p is XElement pe && Models.s2t(pe.Name.LocalName) is Type t && t.IsSubclassOf(typeof(Node)))) continue;
              if (i == 0) { n++; continue; }
              (nodes[n++] = (Group)Activator.CreateInstance(t)).Serialize(pe, false);
            }
          }
        }
      }
      internal protected virtual void Invalidate() => parent?.Invalidate();
      internal protected Group[]? VisibleNodes
      {
        get => nodes != null && this is not Models.BoolGeometry ? nodes : default;
      }
      internal protected virtual object? GetService(Type t)
      {
        return parent != null ? parent.GetService(t) : null;
      }
      public Node? Find(string name)
      {
        if (nodes != null)
          for (int i = 0; i < nodes.Length; i++)
            if (nodes[i].Name == name)
              return nodes[i];
        return null;
      }
    }

    public class Group : Node
    {
      [Category("\t\tGeneral")]
      public string Name
      {
        get => name ?? String.Empty;
        set => name = value != null && (value = value.Trim()).Length != 0 ? value : null;
      }
      [Category("\t\tGeneral"), DefaultValue(false)]
      public bool Fixed
      {
        get => (flags & 0x01) != 0;
        set => flags = (flags & ~0x01) | (value ? 0x01 : 0);
      }
      [Category("\t\tGeneral"), DefaultValue(true)]
      public bool Visible
      {
        get => (flags & 0x02) == 0;
        set => flags = (flags & ~0x02) | (value ? 0 : 0x02);
      }
      [Category("\tTransform"), TypeConverter(typeof(Models.VectorConverter))]
      public Vector3 Location
      {
        get => Transform.Translation;
        set { Transform.Translation = value; parent?.Invalidate(); }
      }
      [Category("\tTransform"), TypeConverter(typeof(Models.VectorConverter))]
      public Vector3 Rotation
      {
        get => Transform.Rotation;
        set { Transform.Rotation = value; parent?.Invalidate(); }
      }
      [Category("\tTransform"), TypeConverter(typeof(Models.VectorConverter))]
      public Vector3 Scaling
      {
        get => Transform.Scaling;
        set { Transform.Scaling = value; parent?.Invalidate(); }
      }
      public Matrix4x3 Transform;
      public Matrix4x3 GetTransform(Node? root = null)
      {
        if (root == this) return Matrix4x3.Identity;
        if (root == parent || parent is not Group p) return Transform;
        return Transform * p.GetTransform(root);
      }
      public void GetBox(ref (Vector3 Min, Vector3 Max) box, in Matrix4x3? pm = default)
      {
        var a = VisibleNodes;
        if (a != null)
          for (int i = 0; i < a.Length; i++)
          {
            var p = nodes[i]; p.GetBox(ref box, pm.HasValue ? p.Transform * pm.Value : p.Transform);
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
          if (!m.IsIdentity) e.SetAttributeValue("transform", Models.format(new ReadOnlySpan<float>(&m, 12)));
          if (Fixed) e.SetAttributeValue("fixed", true);
          if (!Visible) e.SetAttributeValue("visible", false);
        }
        else
        {
          XAttribute a;
          if ((a = e.Attribute("transform")) != null)
          {
            var m = default(Matrix4x3);
            Models.parse(a.Value.AsSpan().Trim(), new Span<float>(&m, 12)); Transform = m;
          }
          else Transform = Matrix4x3.Identity;
          if ((a = e.Attribute("fixed")) != null) Fixed = (bool)a;
          if ((a = e.Attribute("visible")) != null) Visible = (bool)a;
        }
      }
    }

    public static class Models
    {
      public class Scene : Node
      {
        internal DX11Ctrl? root;
        internal AniSet? aniset;
        public enum Units { Meter = 1, Centimeter = 2, Millimeter = 3, Micron = 4, Foot = 5, Inch = 6 }
        Units unit; internal uint ambient;
        [Category("\t\tGeneral")]
        public string Name
        {
          get => name ?? String.Empty;
          set => name = value != null && (value = value.Trim()).Length != 0 ? value : null;
        }
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
        [Category("Scene")]
        public bool Shadows
        {
          get => (flags & 0x20) != 0;
          set { flags = (flags & ~0x20) | (value ? 0x20 : 0); }
        }
        protected internal override void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            if (unit != 0) e.SetAttributeValue("unit", unit);
            if (ambient != 0) e.SetAttributeValue("ambient", ambient.ToString("X8"));
            if (Shadows) e.SetAttributeValue("shadows", true);
            if (aniset != null && aniset.lines.Count != 0)
            {
              var res = default(XElement);
              int id = 0; Func<Node, string?> f = null; f = p =>
              {
                var t1 = e.DescendantsAndSelf().FirstOrDefault(x => x.Annotation<Node>() == p);
                if (t1 == null)
                {
                  t1 = res?.Elements().FirstOrDefault(x => x.Annotation<Node>() == p);
                  if (t1 == null)
                  {
                    var t = p.nodes; p.nodes = null;
                    (res ??= new("res")).Add(t1 = Save(p)); p.nodes = t;
                    if (t != null) t1.SetAttributeValue("cc", string.Join(" ", t.Select(x => f(x))));
                  }
                }
                var t2 = t1.Attribute("id");
                if (t2 == null)
                {
                  t1.Add(t2 = new XAttribute("id", ++id));
                  for (XAttribute t; (t = t1.FirstAttribute) != t2; t.Remove(), t1.Add(t)) ;
                }
                return t2.Value;
              };
              e.Add((XElement)aniset.disp(2, f));
              if (res != null) e.Add(res);
            }
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("unit")) != null) unit = Enum.Parse<Units>(a.Value);
            if ((a = e.Attribute("ambient")) != null) ambient = uint.Parse(a.Value, NumberStyles.HexNumber);
            if ((a = e.Attribute("shadows")) != null) Shadows = (bool)a;
            var aniset = e.Element(ns + "aniset");
            if (aniset != null)
            {
              var res = default(XElement);
              Func<string, Node?> f = null; f = pid =>
              {
                var p = e.DescendantsAndSelf().
                  FirstOrDefault(a => (string?)a.Attribute("id") == pid)?.
                  Annotation(typeof(Node)) as Node;
                if (p != null) return p;
                var x = (res ??= e.Element(ns + "res")).Elements().
                  First(a => (string?)a.Attribute("id") == pid);
                if ((p = x.Annotation(typeof(Node)) as Node) == null)
                {
                  p = Load(x); var a = x.Attribute("cc");
                  if (a != null) p.nodes = a.Value.
                    Split(' ', StringSplitOptions.RemoveEmptyEntries).
                    Select(x => f(x)).OfType<Group>().ToArray();
                }
                return p;
              };
              aniset.AddAnnotation(f); (this.aniset = new()).disp(3, aniset);
            }
          }
        }
        protected internal override void Invalidate() { root?.Invalidate(); }
        protected internal override object? GetService(Type t)
        {
          if (t == typeof(Scene)) return this;
          if (t == typeof(DX11ModelCtrl)) return root;
          return base.GetService(t);
        }
      }

      public class Camera : Group
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
      }

      public class Light : Group
      {
        internal uint color = 0xffffffff;
        protected internal override void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            if (color != 0xffffffff) e.SetAttributeValue("color", color.ToString("X8"));
          }
          else
          {
            XAttribute a;
            if ((a = e.Attribute("color")) != null) color = uint.Parse(a.Value, NumberStyles.HexNumber);
          }
        }
        [Category("Light")]
        public Color Color
        {
          get => Color.FromArgb(unchecked((int)color));
          set { color = unchecked((uint)value.ToArgb()); }
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
      public abstract class Geometry : Group
      {
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
        [Category("Material"), TypeConverter(typeof(VectorConverter))]
        public Vector3 TextureScaling
        {
          get => ranges[Current].material.Transform.Scaling;
          set
          {
            var m = ranges[Current].material.Transform; m.Scaling = value;
            ranges[Current].material.Transform = m;
          }
        }
        [Category("Material"), TypeConverter(typeof(VectorConverter))]
        public Vector3 TextureRotation
        {
          get => ranges[Current].material.Transform.Rotation;
          set
          {
            var m = ranges[0].material.Transform; m.Rotation = value;
            ranges[0].material.Transform = m;
          }
        }
        [Category("Material"), TypeConverter(typeof(VectorConverter))]
        public Vector2 TextureOffset
        {
          get { var p = ranges[Current].material.Transform.Translation; return new Vector2(p.X, p.Y); }
          set
          {
            var m = ranges[Current].material.Transform; m.Translation = new Vector3(value, 0);
            ranges[Current].material.Transform = m;
          }
        }
        [Category("Geometry")]
        public MeshInfo Mesh => MeshInfo.get(this);

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class MeshInfo //todo: remove
        {
          static MeshInfo? cache; readonly Geometry p; MeshInfo(Geometry p) => this.p = p;
          internal static MeshInfo get(Geometry p) => cache?.p == p ? cache : cache = new MeshInfo(p);
          public override string ToString() { return "ok"; } // "errors"
          public int MeshVertices => p.vertices.Length;
          public int MeshPolygones => p.indices.Length / 3;
          public int MeshEdges
          {
            get { calc(); return edges; }
          }
          public int MeshEuler => MeshVertices - MeshEdges + MeshPolygones;
          public int PolyhedronPlanes
          {
            get { calc(); return ee.Length; }
          }
          public int PolyhedronEdges
          {
            get { calc(); return hedges; }
          }
          //public int PolyhedronEuler => MeshVertices - MeshEdges + MeshPolygones;
          public int Errors => 0;
          Vector3R[]? pp; (PlaneR e, int[] kk)[]? ee; int hash, edges, hedges;
          void calc()
          {
            var hash = p.vertices.Length ^ p.indices.Length;
            if (hash != this.hash) { this.hash = hash; this.pp = null; }
            if (this.pp != null) return; //Console.Beep(4000, 100);
            var ii = p.indices; var xp = p.GetVertices();
            this.pp = xp as Vector3R[] ?? ((Vector3[])xp).Select(p => (Vector3R)p).ToArray();
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

        internal protected Vector3[]? vertices;
        internal protected ushort[]? indices;
        internal (int count, Material material)[]? ranges; int current;
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
        protected internal override unsafe void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing)
          {
            if (ranges.Length == 1) save(e, ranges[0].material);
            else
            {
              var ea = new XElement(ns + "materials"); e.AddFirst(ea);
              for (int k = 0; k < ranges.Length; k++)
              {
                var r = ranges[k]; var em = new XElement(ns + "material"); ea.Add(em);
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
            var ea = e.Element(ns + "materials");
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
        protected internal virtual void Render(DX11Ctrl.DC dc, Group main) { }
        protected internal virtual Action<int>? GetTool(DX11Ctrl.PC pc, Group main)
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

      public sealed class ScriptGeometry : Geometry
      {
        [Category("Geometry"), Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string? Code
        {
          get => code;
          set
          {
            if (value == this.code) return;
            if (string.IsNullOrEmpty(value)) { this.code = value; this.inst = null; return; }
            var expr = ExpressionsCompiler.Compile(value, GetType(), usings);
            var ctor = expr.Compile();
            var inst = ctor(this);
            this.code = value; this.inst = inst; flags &= ~0x04;
          }
        }
        string? code; object[]? inst;
        static readonly string[] usings = { "System", "System.Collections.Generic", "System.Numerics" };
        internal protected override void Serialize(XElement e, bool storing)
        {
          base.Serialize(e, storing);
          if (storing) { if (code != null) e.SetAttributeValue("code", code); }
          else { XAttribute p; if ((p = e.Attribute("code")) != null) if (string.IsNullOrEmpty(code = p.Value)) code = null; }
        }
        protected internal override void Build()
        {
          var pp = new List<Vector3>();
          var ii = new List<int>();
          if (code != null)
            try
            {
              if (this.inst == null)
              {
                var expr = ExpressionsCompiler.Compile(code, GetType(), usings);
                var ctor = expr.Compile();
                var inst = ctor(this); this.inst = inst;
              }
              var func = inst.OfType<Action<List<Vector3>, List<int>>>().FirstOrDefault();
              if (func != null) func(pp, ii);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
          vertices = pp.ToArray();
          indices = ii.Select(p => (ushort)p).ToArray();
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
          var n = nodes != null ? nodes.Length : 0;
          for (int i = 0; i < n; i++) nodes[i].parent = this; if (myranges == null) myranges = ranges;
          if (n >= 2 && nodes[0] is Geometry a && nodes[1] is Geometry b)
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
        [Category("Geometry"), TypeConverter(typeof(VectorConverter))]
        public Vector3 Min
        {
          get => p1;
          set { p1 = value; Invalidate(); }
        }
        [Category("Geometry"), TypeConverter(typeof(VectorConverter))]
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
            if (btn.Checked) return false; ((PropsCtrl)grid.Parent).btnprops.PerformClick();
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
                if (geo.points[wo] != o) RootCtrl.AddUndo(new UndoAction(vd ?? undo(geo, wo, o)));
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
                if (p1 != p2) RootCtrl.AddUndo(new UndoAction(vd)); else { vd(); Invalidate(); }
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
        protected internal override void Render(DX11Ctrl.DC dc, Group main)
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
        protected internal override Action<int>? GetTool(DX11Ctrl.PC pc, Group main)
        {
          if ((pc.Id & 0x10000000) != 0) return tool(pc, main);
          return base.GetTool(pc, main);
          Action<int> tool(DX11Ctrl.PC pc, Group main)
          {
            var i = pc.Primitive; var o = points[i];
            pc.SetPlane(Matrix4x3.CreateTranslation(0, 0, height) * main.GetTransform());
            var p1 = pc.Pick(); var p2 = p1;
            return id =>
            {
              if (id == 0) { p2 = pc.Pick(); points[i] = o + (p2 - p1); Invalidate(); }
              if (id == 1 && p1 != p2) (pc.View as DX11ModelCtrl)?.AddUndo(new UndoAction(() => { var t = points[i]; points[i] = o; o = t; Invalidate(); }));
            };
          }
        }
      }

      public class RotationGeometry : Poly2DGeometry
      {
        int seg;
        [Category("Geometry")]
        public int Seg { get => seg; set { seg = value; Invalidate(); } }
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
        protected internal override void Render(DX11Ctrl.DC dc, Group main)
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
        protected internal override Action<int>? GetTool(DX11Ctrl.PC pc, Group main)
        {
          if ((pc.Id & 0x10000000) != 0) return tool(pc, main);
          return base.GetTool(pc, main);
          Action<int> tool(DX11Ctrl.PC pc, Group main)
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
              if (id == 1 && o != points[i]) ((DX11ModelCtrl)pc.View).AddUndo(new UndoAction(() => { var t = points[i]; points[i] = o; o = t; Invalidate(); }));
            };
          }
        }
      }

      public class TextGeometry : Geometry
      {
        string text = string.Empty; System.Drawing.Font? font;
        float height = 1, depth; int flat = 8;
        [Category("Geometry"), Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
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
          var o = new Vector2(); float sc = (0.0113f / 0x10000) * height;
          fixed (char* p = text) DX11Ctrl.GlyphContour(p, text.Length, Font, Flat, (id, x, y) =>
          {
            switch (id)
            {
              case 0: tess.BeginContour(); o.X = (float)x * 0x10000; break;
              case 1: tess.AddVertex((o + new Vector2(x, y)) * sc); break;
              case 2: tess.EndContour(); break;
              case 3: if (x == '\n') o.Y -= font.GetHeight(96) * 0x10000; break;
            }
          });
          tess.EndPolygon();
          extruse(tess, depth);
        }
      }

      #region xml
      public static readonly XNamespace ns = XNamespace.None; //todo: 
      internal static Type? s2t(string s)
      {
        switch (s)
        {
          case "g": case "Node": return typeof(Group);
          case "box": case "BoxGeometry": return typeof(BoxGeometry);
          case "polyext": case "ExtrusionGeometry": return typeof(ExtrusionGeometry);
          case "polyrot": case "RotationGeometry": return typeof(RotationGeometry);
          case "mesh": case "MeshGeometry": return typeof(MeshGeometry);
          case "bool": case "BoolGeometry": return typeof(BoolGeometry);
          case "text": case "TextGeometry": return typeof(TextGeometry);
          case "cylinder": case "CylinderGeometry": return typeof(CylinderGeometry);
          case "sphere": case "SphereGeometry": return typeof(SphereGeometry);
          case "light": case "Light": return typeof(Light);
          case "camera": case "Camera": return typeof(Camera);
          case "scene": case "Scene": return typeof(Scene);
          case "script": return typeof(ScriptGeometry);
          case "at": return typeof(UndoTrans.Line);
          case "ac": return typeof(UndoProp.ColorLine);
          case "af": return typeof(UndoProp.FloatLine);
          case "a2": return typeof(UndoProp.Vector2Line);
          case "a3": return typeof(UndoProp.Vector3Line);
          case "ab": return typeof(UndoProp.BoolLine);
          case "ai": return typeof(UndoProp.IntLine);
          case "as": return typeof(UndoProp.StringLine);
          case "aa": return typeof(UndoSet.Line);
          case "an": return typeof(UndoNodes.Line);
          default: return null;
        }
      }
      internal static object? s2o(string s)
      {
        return Activator.CreateInstance(s2t(s));
      }
      internal static string o2s(object p)
      {
        //return p.GetType().Name; //var a = (XmlTypeAttribute)Attribute.GetCustomAttribute(p.GetType(), typeof(XmlTypeAttribute));
        var t = p.GetType();
        if (t == typeof(Group)) return "g";
        if (t == typeof(BoxGeometry)) return "box";
        if (t == typeof(ExtrusionGeometry)) return "polyext";
        if (t == typeof(RotationGeometry)) return "polyrot";
        if (t == typeof(MeshGeometry)) return "mesh";
        if (t == typeof(BoolGeometry)) return "bool";
        if (t == typeof(TextGeometry)) return "text";
        if (t == typeof(CylinderGeometry)) return "cylinder";
        if (t == typeof(SphereGeometry)) return "sphere";
        if (t == typeof(Light)) return "light";
        if (t == typeof(Camera)) return "camera";
        if (t == typeof(Scene)) return "scene";
        if (t == typeof(ScriptGeometry)) return "script";
        if (t == typeof(UndoTrans.Line)) return "at";
        if (t == typeof(UndoProp.ColorLine)) return "ac";
        if (t == typeof(UndoProp.FloatLine)) return "af";
        if (t == typeof(UndoProp.Vector2Line)) return "a2";
        if (t == typeof(UndoProp.Vector3Line)) return "a3";
        if (t == typeof(UndoProp.BoolLine)) return "ab";
        if (t == typeof(UndoProp.IntLine)) return "ai";
        if (t == typeof(UndoProp.StringLine)) return "as";
        if (t == typeof(UndoSet.Line)) return "aa";
        if (t == typeof(UndoNodes.Line)) return "an";
        throw new Exception();
      }

      public static XElement Save(Node node)
      {
        var e = new XElement(ns + o2s(node));
        node.Serialize(e, true); return e;
      }
      public static Node Load(XElement e)
      {
        var node = (Node)s2o(e.Name.LocalName); // (Base)Activator.CreateInstance(typeof(Models).GetNestedType(e.Name.LocalName));
        node.Serialize(e, false); return node;
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
      internal static ReadOnlySpan<char> token(ref ReadOnlySpan<char> a)
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
      #endregion
      #region converter
      internal class VectorConverter : TypeConverter
      {
        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
        {
          if (value is Vector2 a) return $"{a.X}; {a.Y}";
          if (value is Vector3 b) return $"{b.X}; {b.Y}; {b.Z}";
          if (value is Vector4 c) return $"{c.X}; {c.Y}; {c.Z}; {c.W}";
          //if (value is rat d) return d.ToString();
          return null;
        }
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType) => true;
        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
          var a = ((string)value).Split(';');
          var b = stackalloc float[a.Length];
          for (int i = 0; i < a.Length; i++) b[i] = float.Parse(a[i]);
          if (a.Length == 2) return new Vector2(b[0], b[1]);
          if (a.Length == 3) return new Vector3(b[0], b[1], b[2]);
          if (a.Length == 4) return new Vector4(b[0], b[1], b[2], b[3]);
          return base.ConvertFrom(context, culture, value);
        }
      }
      #endregion
    }
  }

  static unsafe class SpanTools
  {
    internal static Vector3 readv3(ReadOnlySpan<char> s, NumberFormatInfo? fi)
    {
      Vector3 v; DX11ModelCtrl.Models.parse(s.Trim(), new Span<float>(&v, 3)); return v;
    }
    internal static Vector2 readv2(ReadOnlySpan<char> s, NumberFormatInfo? fi)
    {
      Vector2 v; DX11ModelCtrl.Models.parse(s.Trim(), new Span<float>(&v, 2)); return v;
    }
    internal static int write(Vector2 p, Span<char> w, NumberFormatInfo f)
    {
      p.X.TryFormat(w, out var a, default, f); w[a] = ' '; w = w.Slice(a + 1);
      p.Y.TryFormat(w, out var b, default, f); return a + b + 1;
    }
    internal static int write(Vector3 p, Span<char> w, NumberFormatInfo f)
    {
      p.X.TryFormat(w, out var a, default, f); w[a] = ' '; w = w.Slice(a + 1);
      p.Y.TryFormat(w, out var b, default, f); w[b] = ' '; w = w.Slice(b + 1);
      p.Z.TryFormat(w, out var c, default, f); return a + b + c + 2;
    }
    internal static ReadOnlySpan<char> param_slice(ref ReadOnlySpan<char> sp)
    {
      Debug.Assert(sp == sp.Trim());
      int i = 0; for (; i < sp.Length && !(char.IsWhiteSpace(sp[i]) || sp[i] == ';'); i++) ;
      var w = sp.Slice(0, i); sp = sp.Slice(i < sp.Length ? i + 1 : i).TrimStart(); return w;
    }
    internal static int param_count(ReadOnlySpan<char> sp)
    {
      Debug.Assert(sp == sp.Trim());
      var n = 0; for (; sp.Length != 0; param_slice(ref sp), n++) ; return n;
    }
  }

  //Props
  public unsafe partial class DX11ModelCtrl
  {
    public abstract class PropsCtrl : UserControl
    {
      public DX11ModelCtrl? Target; // { get; set; }
      protected override void OnLoad(EventArgs e)
      {
        base.OnLoad(e);
        combo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, DrawMode = DrawMode.OwnerDrawFixed };
        combo.DrawItem += (p, e) =>
        {
          e.DrawBackground(); var i = e.Index; if (i < 0) return;
          var g = e.Graphics; var o = combo.Items[e.Index]; var r = e.Bounds;
          var node = o as Node; var font = Font;
          if ((e.State & DrawItemState.ComboBoxEdit) == 0 && node != null)
          {
            for (var t = node.parent; t != null; t = t.parent) r.X += 16;
            if (node.nodes != null)
            {
              var ss = Target.selection; var s = default(Node);
              if (ss.Count != 0) //for (int x = 0; x < ss.Count && ms == null; x++)
                for (s = ss[ss.Count - 1]; s != null && s != node; s = s.parent) ;
              TextRenderer.DrawText(g, s != null ? "" : "", font, // ˃ ˅
                new Rectangle(r.X - 15, r.Y - 2, r.Width, r.Height), e.ForeColor, TextFormatFlags.Left);
            }
          }
          var sn = node != null ? node.name : o.ToString();
          if (sn != null)
          {
            TextRenderer.DrawText(g, sn, bold ??= new System.Drawing.Font(font, FontStyle.Bold), r, e.ForeColor, TextFormatFlags.Left);
            r.X += TextRenderer.MeasureText(sn, bold).Width;
          }
          if (node != null) TextRenderer.DrawText(g, o.GetType().Name, font, r, e.ForeColor, TextFormatFlags.Left);
          e.DrawFocusRectangle();
        };
        combo.SelectedIndexChanged += (_, e) =>
        {
          if (!btnprops.Checked) return;
          if (!combo.Focused || comboupdate) return;
          var p = combo.SelectedItem;
          fillcombo(p);
          Target.Select(p as Group); update = true;
        };
        grid = new PropertyGrid { Dock = DockStyle.Fill, TabIndex = 1 };
        grid.PropertySort = PropertySort.Categorized;
        grid.PropertySortChanged += (p, e) =>
        {
          if (grid.PropertySort == PropertySort.CategorizedAlphabetical)
            grid.PropertySort = PropertySort.Categorized; //NoSort
        };
        grid.PropertyValueChanged += (_, e) =>
        {
          var d = e.ChangedItem.PropertyDescriptor; //if (d.PropertyType == typeof(Type)) return;
          var o = e.ChangedItem.GetType().GetProperty("Instance").GetValue(e.ChangedItem);
          var s = d.Name; var v = e.OldValue;
          Target.Invalidate(); if (o is not Node ba) return;
          if (lastpd == d && d.PropertyType == typeof(int)) return; lastpd = d; // index selectors
          var t = o.GetType().GetProperty(s).GetValue(o); if (object.Equals(v, t)) return;
          Target.AddUndo(new UndoProp(ba, s, v)); grid.Update();
        };
        var a = Controls; a.Add(grid); a.Add(combo);
        var cc = grid.Controls; view = cc[2]; info = cc[0];

        var f = new System.Drawing.Font("Segoe UI Symbol", Font.Size); var al = ContentAlignment.TopCenter;
        btnprops = new ToolStripButton() { Text = "", ToolTipText = "Properties", Font = f, TextAlign = al, AccessibleRole = AccessibleRole.RadioButton, DisplayStyle = ToolStripItemDisplayStyle.Text, Checked = true };
        btnsetting = new ToolStripButton() { Text = "", ToolTipText = "Settings", Font = f, TextAlign = al, AccessibleRole = AccessibleRole.RadioButton, DisplayStyle = ToolStripItemDisplayStyle.Text };
        //btnedit = new ToolStripButton() { Text = "", ToolTipText = "Edit", Enabled = false, Font = f, TextAlign = al,AccessibleRole = AccessibleRole.RadioButton, DisplayStyle = ToolStripItemDisplayStyle.Text };
        btntoolbox = new ToolStripButton() { Text = "⚒", ToolTipText = "Toolbox", Font = f, TextAlign = al, AccessibleRole = AccessibleRole.RadioButton, DisplayStyle = ToolStripItemDisplayStyle.Text };
        btnstory = new ToolStripButton() { Text = "", ToolTipText = "Storyboard", Font = f, TextAlign = al, AccessibleRole = AccessibleRole.CheckButton, DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnclose = new ToolStripButton() { Text = "⨯", ToolTipText = "Close", Font = f, TextAlign = al, Alignment = ToolStripItemAlignment.Right };

        btnclose.Click += (p, e) => { this.Visible = false; };
        btnprops.Click += (p, e) =>
        {
          if (btnprops.Checked) return; btnprops.Checked = true;
          btnsetting.Checked = btntoolbox.Checked = false; showtoolbox(false);
          update = true; combo.Items.Clear(); //grid.Focus();
          //var cm = ContextMenuStrip; if (cm != null) cm.Enabled = true;
        };
        btnsetting.Click += (p, e) =>
        {
          if (btnsetting.Checked) return; btnsetting.Checked = true;
          btnprops.Checked = btntoolbox.Checked = false; showtoolbox(false);
          grid.SelectedObject = Target.settings;
          combo.Items.Clear(); combo.Items.Add("Settings"); combo.SelectedIndex = 0;
          //var cm = ContextMenuStrip; if (cm != null) cm.Enabled = false;
        };
        btntoolbox.Click += (p, e) =>
        {
          btntoolbox.Checked = true;
          btnprops.Checked = btnsetting.Checked = false;
          combo.Items.Clear(); combo.Items.Add("Toolbox"); combo.SelectedIndex = 0;
          showtoolbox(true);
        };
        var ts = (ToolStrip)cc[3]; var it = ts.Items; ts.RenderMode = ToolStripRenderMode.Professional;
        it.Add(new ToolStripSeparator());
        it.Add(btnprops); it.Add(btnsetting); /*it.Add(btnedit);*/
        it.Add(btntoolbox); it.Add(btnstory); it.Add(btnclose);
      }
      protected override void OnVisibleChanged(EventArgs e)
      {
        base.OnVisibleChanged(e); if (Target == null || grid == null) return;
        if (Visible)
        {
          Target.Animations += onidle; Target.Invaliated += oninv; update = true;
        }
        else
        {
          Target.Animations -= onidle; Target.Invaliated -= oninv;
          grid.SelectedObject = null; combo.Items.Clear();
        }
        void oninv() => update = true;
      }
      PropertyGrid? grid; ComboBox? combo; System.Drawing.Font? bold;
      Control? view, info; internal ToolStripButton? btnprops, btnsetting, btntoolbox, btnstory;
      bool comboupdate, update; object? lastpd; WebBrowser? wb;
      void onidle()
      {
        if (!update) return; update = false;
        if (combo.DroppedDown || !btnprops.Checked) return;
        var list = Target.selection; var ext = Target.extrasel;
        var csel = combo.SelectedItem;
        if (ext.p != null && grid.ContainsFocus && csel is not Node) return;
        var disp =
          ext.p != null && ext.view.ContainsFocus ? ext.p :
          (list.Count != 0 ? (object)list[list.Count - 1] :
           csel != null && csel is not Node ? csel : Target.Scene);
        if (csel != disp && !combo.ContainsFocus) fillcombo(csel = disp);
        if (grid.SelectedObject != csel)
        {
          grid.SelectedObject = csel;
          Models.propref = false; lastpd = null;
          if (grid.PropertySort == PropertySort.NoSort)
            grid.PropertySort = PropertySort.Categorized;
        }
        else
        {
          if (Models.propref) { Models.propref = false; grid.Refresh(); return; }
          combo.Invalidate(); //if (ContainsFocus) { update = true; return; }
          grid.Invalidate(); grid.Refresh();
        }
      }
      void fillcombo(object disp)
      {
        var items = combo.Items; comboupdate = true;
        items.Clear(); var node = disp as Node;
        if (node == null) items.Add(disp); // extrasel
        else
        {
          items.Add(Target.Scene);
          if (node != null)
          {
            recu(items, node);
            static void recu(ComboBox.ObjectCollection items, Node node)
            {
              if (node.parent != null) recu(items, node.parent);
              var a = node.nodes; if (a == null) return;
              for (int i = 0, x = items.IndexOf(node); i < a.Length; i++)
                items.Insert(++x, a[i]);
            }
          }
        }
        combo.SelectedItem = disp; combo.Update(); comboupdate = false;
      }
      void showtoolbox(bool on)
      {
        if (on == (wb != null && wb.Visible)) return;
        if (on)
        {
          if (wb == null)
            view.Controls.Add(wb = new WebBrowser()
            {
              Visible = false,
              Dock = DockStyle.Fill,
              AllowNavigation = false,
              ScriptErrorsSuppressed = true,
              WebBrowserShortcutsEnabled = false,
              IsWebBrowserContextMenuEnabled = false,
              //ScrollBarsEnabled = false,
              Url = new Uri("https://c-ohle.github.io/RationalNumerics/web/cat/index.htm"),
              //Url = new Uri("file://C:/Users/cohle/Desktop/RationalNumericsDoc/web/cat/index.htm"),
            });
          info.Visible = false;
          wb.Visible = true;
          wb.BringToFront(); wb.Focus();
        }
        else
        {
          wb.Visible = false; info.Visible = true;
        }
        grid.Height++; grid.Height--;
      }
    }
  }

}
