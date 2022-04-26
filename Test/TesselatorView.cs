using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
  public partial class TesselatorView : UserControl
  {
    public TesselatorView()
    {
      DoubleBuffered = true;
      DrawPolygons = true;
      Winding = Winding.EvenOdd; Delaunay = true;
      DrawSurface = DrawOutlines = DrawMesh = true;
    }
    public bool DrawPolygons { get; set; }
    public bool DrawPoints { get; set; }
    public Winding Winding { get; set; }
    public bool Delaunay { get; set; }
    public bool DrawSurface { get; set; }
    public bool DrawOutlines { get; set; }
    public bool DrawMesh { get; set; }

    public readonly List<Vector2> Points = new();
    public readonly List<int> Counts = new();

    TesselatorR? tess;
    PointF[]? pointsf; Pen? pen;

    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e)
    {
      var g = e.Graphics; g.Clear(Color.White);
      g.SmoothingMode = SmoothingMode.AntiAlias;
      pen ??= new Pen(Color.Black, 1);
      tess ??= new TesselatorR();
      tess.Winding = Winding;
      tess.Options = (tess.Options & ~TesselatorR.Option.Delaunay) | (Delaunay ? TesselatorR.Option.Delaunay : 0);
      tess.BeginPolygon();
      for (int i = 0, a = 0; i < Counts.Count; a += Counts[i++])
      {
        tess.BeginContour();
        for (int k = 0, n = Counts[i]; k < n; k++)
          tess.AddVertex(Points[a + k]);
        tess.EndContour();
      }
      var t1 = Stopwatch.GetTimestamp();// Environment.TickCount;
      tess.EndPolygon();
      var t2 = Stopwatch.GetTimestamp();

      var indices = tess.Indices;
      var vertices = tess.VerticesVector3;
      if (DrawSurface)
      {
        pointsf ??= new PointF[3];
        for (int i = 0; i < indices.Length; i += 3)
        {
          var p1 = vertices[indices[i + 0]]; pointsf[0] = new PointF(p1.X, p1.Y);
          var p2 = vertices[indices[i + 1]]; pointsf[1] = new PointF(p2.X, p2.Y);
          var p3 = vertices[indices[i + 2]]; pointsf[2] = new PointF(p3.X, p3.Y);
          g.FillPolygon(Brushes.Gold, pointsf);
        }
      }
      if (DrawMesh)
      {
        pen.Width = 1f; pen.Color = DrawSurface ? Color.Goldenrod : Color.LightGray;
        for (int i = 0; i < indices.Length; i += 3)
        {
          var p1 = vertices[indices[i + 0]];
          var p2 = vertices[indices[i + 1]];
          var p3 = vertices[indices[i + 2]];
          g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
          g.DrawLine(pen, p2.X, p2.Y, p3.X, p3.Y);
          g.DrawLine(pen, p3.X, p3.Y, p1.X, p1.Y);
        }
      }
      if (DrawPolygons)
      {
        pen.Width = 1; pen.Color = Color.Gray;
        for (int i = 0, a = 0; i < Counts.Count; a += Counts[i++])
          for (int k = 0, n = Counts[i]; k < n; k++)
          {
            var p1 = Points[a + k];
            var p2 = Points[a + (k + 1) % n];
            g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
          }
      }
      if (DrawOutlines)
      {
        var outline = tess.Outline;
        var counts = tess.OutlineCounts;
        pen.Width = 2; pen.Color = Color.Black;
        for (int i = 0, a = 0; i < counts.Length; a += counts[i++])
          for (int k = 0, n = counts[i]; k < n; k++)
          {
            var p1 = vertices[outline[a + k]];
            var p2 = vertices[outline[a + (k + 1) % n]];
            g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
          }
      }
      if (DrawPoints)
      {
        for (int i = 0; i < Points.Count; i++)
        {
          var p = Points[i];
          var r = new RectangleF(p.X - 3, p.Y - 3, 6, 6);
          g.FillEllipse(Brushes.White, r);
          g.DrawEllipse(Pens.Black, r);

        }

      }

      var font = this.Font; var y = 4;
      g.DrawString($"{vertices.Length} vertices {indices.Length/3} polygones", font, Brushes.Black, 4, y); y += font.Height;
      g.DrawString($"{(t2 - t1) * 1000 / Stopwatch.Frequency} ms", font, Brushes.Black, 4, y); y += font.Height;
    }
  }
}
