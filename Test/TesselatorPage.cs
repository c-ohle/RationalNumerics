using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
  public partial class TesselatorPage : UserControl
  {
    public TesselatorPage()
    {
      InitializeComponent();
    }
    bool init;
    protected override void OnLoad(EventArgs e)
    {
      update();
      addface();
    }
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

    void add_ellipse(Vector2 midpoint, Vector2 radii, int segs)
    {
      var v = tesselatorView1;
      var f = (2 * MathF.PI) / segs; var c = v.Points.Count;
      for (int i = 0; i < segs; i++) v.Points.Add(
        midpoint + new Vector2(MathF.Cos(i * f), MathF.Sin(i * f)) * radii);
      v.Counts.Add(v.Points.Count - c);
    }
    void addface()
    {
      var v = tesselatorView1;
      v.Points.Clear(); v.Counts.Clear();
      add_ellipse(new Vector2(300, 200), new Vector2(100), 100);
      add_ellipse(new Vector2(400, 200), new Vector2(100), 100);
      add_ellipse(new Vector2(350, 300), new Vector2(50), 100);
      add_ellipse(new Vector2(500, 200), new Vector2(50, -50), 100);
      add_ellipse(new Vector2(200, 200), new Vector2(50, -50), 100);
      add_ellipse(new Vector2(300 - 10, 200), new Vector2(20, 20), 16);
      add_ellipse(new Vector2(400 + 10, 200), new Vector2(20, 20), 16);
    }
         
  }
}
