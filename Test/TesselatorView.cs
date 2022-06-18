using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
      DoubleBuffered = true; //Reset();
    }
    public bool DrawPolygons { get; set; }
    public bool DrawPoints { get; set; }
    public Winding Winding { get; set; }
    public bool Delaunay { get; set; }
    public bool DrawSurface { get; set; }
    public bool DrawOutlines { get; set; }
    public bool DrawMesh { get; set; }
    public readonly List<Vector2> Points = new();
    public readonly List<ushort> Counts = new();
    public void Reset()
    {
      Winding = Winding.EvenOdd;
      DrawPolygons = true; DrawPoints = false;
      Winding = Winding.EvenOdd; Delaunay = true;
      DrawSurface = DrawOutlines = DrawMesh = true;
      mx = 0; my = 0; ms = DeviceDpi * (1f / 120);
      prad = 4 * ms; Invalidate();
    }

    TesselatorR? tess;
    PointF[]? pointsf; Pen? pen;
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      Reset();
    }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e)
    {
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
      var t1 = Stopwatch.GetTimestamp();
      tess.EndPolygon();
      var t2 = Stopwatch.GetTimestamp();
      var indices = tess.Indices;
      var vertices = tess.VerticesVector3;

      var g = e.Graphics; g.Clear(Color.White);
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.TranslateTransform(mx, my);
      g.ScaleTransform(ms, ms);
      pen ??= new Pen(Color.Black, 1);
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
        pen.Width = 1f / ms; pen.Color = DrawSurface ? Color.Goldenrod : Color.LightGray;
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
        pen.Width = 1f / ms; pen.Color = Color.Gray;
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
        pen.Width = 2 / ms; pen.Color = Color.Black;
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
        pen.Width = 1 / ms; pen.Color = Color.Black; var f = prad / ms;
        for (int i = 0; i < Points.Count; i++)
        {
          var p = Points[i];
          var r = new RectangleF(p.X - f, p.Y - f, 2 * f, 2 * f);
          g.FillEllipse(Brushes.White, r);
          g.DrawEllipse(pen, r);
        }
      }
      g.ResetTransform();
      var font = this.Font; var y = 4;
      g.DrawString($"{vertices.Length} vertices {indices.Length / 3} polygones", font, Brushes.Black, 4, y); y += font.Height;
      g.DrawString($"{(t2 - t1) * 1000 / Stopwatch.Frequency} ms", font, Brushes.Black, 4, y); y += font.Height;
    }

    float mx, my, ms, prad;// = 4;
    Action<int>? tool;
    Action<int> tool_move()
    {
      var ox = mx; var oy = my;
      var p1 = Cursor.Position;
      return id =>
      {
        if (id == 0)
        {
          var p2 = Cursor.Position;
          mx = ox + (p2.X - p1.X);
          my = oy + (p2.Y - p1.Y);
          Invalidate(); Update();
        }
      };
    }
    Action<int> tool_movepoint(int i)
    {
      var po = Points[i];
      var p1 = s2p(Cursor.Position);
      return id =>
      {
        if (id == 0)
        {
          var p2 = s2p(Cursor.Position);
          Points[i] = po + p2 - p1;
          Invalidate(); Update();
        }
      };
    }
    Action<int> tool_movepoly(int x)
    {
      int i = 0, a = 0; for (; i < x; a += Counts[i++]) ;
      var pp = Points.Skip(a).Take(Counts[i]).ToArray();
      var po = Points[i];
      var p1 = s2p(Cursor.Position);
      return id =>
      {
        if (id == 0)
        {
          var p2 = s2p(Cursor.Position); var dp = p2 - p1;
          for (int k = 0; k < pp.Length; k++) Points[a + k] = pp[k] + dp;
          Invalidate(); Update();
        }
      };
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        Capture = true; var i = pick(e.Location); tool =
          (i & 0x10000000) != 0 ? tool_movepoint(i & 0x0fffffff) :
          (i & 0x20000000) != 0 ? tool_movepoly(i & 0x0fffffff) :
          tool_move();
      }
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
      if (tool != null) tool(0);
      else
      {
        var i = pick(e.Location); Cursor =
          (i & 0x10000000) != 0 ? Cursors.Cross :
          (i & 0x20000000) != 0 ? Cursors.UpArrow : Cursors.Default;
      }
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
      if (tool != null) { tool(1); tool = null; Capture = false; }
    }
    protected override void OnLeave(EventArgs e)
    {
      if (tool != null) { tool(2); tool = null; Capture = false; }
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
      var p = e.Location; var v = s2p(p);
      var d = 1 + e.Delta * (0.1f / 120);
      ms *= d; var s = p2s(v);
      mx -= s.X - p.X;
      my -= s.Y - p.Y;
      Invalidate(); Update();
    }

    Vector2 p2s(Vector2 p)
    {
      return new Vector2(p.X * ms + mx, p.Y * ms + my);
    }
    Vector2 s2p(Point p)
    {
      return new Vector2((p.X - mx) / ms, (p.Y - my) / ms);
    }
    unsafe int pick(Point s)
    {
      var p = new Vector2(s.X, s.Y);
      if (DrawPoints)
      {
        var r = prad * prad;
        for (int i = 0; i < Points.Count; i++)
          if ((p2s(Points[i]) - p).LengthSquared() < r) return 0x10000000 | i;
      }

      if (DrawOutlines && tess != null && pen != null)
      {
        var vertices = tess.VerticesVector3;
        var outline = tess.Outline;
        var counts = tess.OutlineCounts;
        uint pixel = 0;
        using (var pbm = new Bitmap(1, 1, 4, PixelFormat.Format32bppRgb, new IntPtr(&pixel)))
        using (var g = Graphics.FromImage(pbm))
        {
          g.TranslateTransform(mx - s.X, my - s.Y);
          g.ScaleTransform(ms, ms);
          pen.Width = 5 / ms; pen.Color = Color.White;
          for (int i = 0, a = 0; i < counts.Length; a += counts[i++])
            for (int k = 0, n = counts[i]; k < n; k++)
            {
              var p1 = vertices[outline[a + k]];
              var p2 = vertices[outline[a + (k + 1) % n]];
              g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
            }
          if (pixel != 0)
          {
            pixel = 0;
            for (int i = 0, a = 0; i < Counts.Count; a += Counts[i++])
            {
              for (int k = 0, n = Counts[i]; k < n; k++)
              {
                var p1 = Points[a + k];
                var p2 = Points[a + (k + 1) % n];
                g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
              }
              if (pixel != 0) return i | 0x20000000;
            }
          }
        }
      }
      return 0;
    }
  }
}
