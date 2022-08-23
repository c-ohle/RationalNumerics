using System.Numerics;
using System.Numerics.Rational;

namespace Test
{
  public partial class TesselatorPage : UserControl
  {
    public TesselatorPage()
    {
      InitializeComponent();
    }
    protected override void OnLoad(EventArgs e)
    {
      update(); setdemo1();
    }
    bool init;
    void update()
    {
      checkBoxPolygons.Checked = tesselatorView1.DrawPolygons;
      checkBoxPoints.Checked = tesselatorView1.DrawPoints;
      checkBoxDelaunay.Checked = tesselatorView1.Delaunay;
      comboBoxWinding.SelectedIndex = (int)tesselatorView1.Winding;
      checkBoxDrawSurface.Checked = tesselatorView1.DrawSurface;
      checkBoxDrawMesh.Checked = tesselatorView1.DrawMesh;
      checkBoxDrawOutlines.Checked = tesselatorView1.DrawOutlines;
      init = true;
    }
    private void comboBoxWinding_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!init) return;
      tesselatorView1.Winding = (Winding)comboBoxWinding.SelectedIndex;
      tesselatorView1.Invalidate();
    }
    void checkBoxDraw_CheckedChanged(object sender, EventArgs e)
    {
      if (!init) return;
      tesselatorView1.DrawPolygons = checkBoxPolygons.Checked;
      tesselatorView1.DrawPoints = checkBoxPoints.Checked;
      tesselatorView1.Delaunay = checkBoxDelaunay.Checked;
      tesselatorView1.DrawSurface = checkBoxDrawSurface.Checked;
      tesselatorView1.DrawMesh = checkBoxDrawMesh.Checked;
      tesselatorView1.DrawOutlines = checkBoxDrawOutlines.Checked;
      tesselatorView1.Invalidate();
    }
    
    static void add_ellipse((List<Vector2> points, List<ushort> counts) a, Vector2 midpoint, Vector2 radii, int segs)
    {
      var f = (2 * MathF.PI) / segs; var c = a.points.Count;
      for (int i = 0; i < segs; i++) a.points.Add(
        midpoint + new Vector2(MathF.Cos(i * f), MathF.Sin(i * f)) * radii);
      a.counts.Add((ushort)(a.points.Count - c));
    }
    static void demo1((List<Vector2> points, List<ushort> counts) a)
    {
      add_ellipse(a, new Vector2(300, 200), new Vector2(100), 100);
      add_ellipse(a, new Vector2(400, 200), new Vector2(100), 100);
      add_ellipse(a, new Vector2(350, 300), new Vector2(50), 100);
      add_ellipse(a, new Vector2(500, 200), new Vector2(50, -50), 100);
      add_ellipse(a, new Vector2(200, 200), new Vector2(50, -50), 100);
      add_ellipse(a, new Vector2(300 - 10, 200), new Vector2(20, 20), 16);
      add_ellipse(a, new Vector2(400 + 10, 200), new Vector2(20, 20), 16);
    }
    void setdemo1()
    {
      var v = tesselatorView1;
      v.Points.Clear(); v.Counts.Clear(); 
      demo1((v.Points, v.Counts));
    }
    void buttonReset_Click(object sender, EventArgs e)
    {
      setdemo1();
      tesselatorView1.Reset(); 
      tesselatorView1.Invalidate();
      init = false; update(); init = true;
    }
  }
}
