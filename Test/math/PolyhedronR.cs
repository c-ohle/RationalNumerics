
namespace Test
{
  /// <summary>
  /// Intersections and boolean operations on arbitrary polyhedral meshes based on <see cref="rat"/>.<br/>
  /// <i>This is just a non-optimal example implementation for testing!</i>
  /// </summary>
  public sealed class PolyhedronR
  {
    public static PolyhedronR GetInstance()
    {
      if (wr == null || wr.Target is not PolyhedronR p)
        wr = new WeakReference(p = new PolyhedronR());
      return p;
    }
    public void SetMesh(int i, object pp, object ii)
    {
      var xp = i == 0 ? app : bpp; xp.Clear(); map.Clear();
      var xi = i == 0 ? aii : bii; xi.Clear();
      if (pp is IList<System.Numerics.Vector3> l2)
      {
        xp.EnsureCapacity(l2.Count);
        for (int k = 0, n = l2.Count; k < n; k++) xp.Add(l2[k]);
      }
      else if (pp is IList<Vector3R> l1) xp.AddRange(l1);
      else throw new ArgumentException(nameof(pp));
      if (ii is IList<ushort> u1)
      {
        xi.EnsureCapacity(u1.Count);
        for (int k = 0, n = u1.Count; k < n; k++) xi.Add(u1[k]);
      }
      else if (ii is IList<int> u2) xi.AddRange(u2);
      else throw new ArgumentException(nameof(ii));
    }
    public void Transform(int i, in Matrix4x3R m)
    {
      var xp = i == 0 ? app : bpp;
      for (i = 0; i < xp.Count; i++) xp[i] = Vector3R.Transform(xp[i], m);
    }
    public List<Vector3R> Vertices => app;
    public List<int> Indices => aii;
    public List<int> Mapping => map;
    public enum Mode { Union, Difference, Intersection }
    public void Boolean(Mode mode, int flags = 0)
    {
      var abox = getbox(app);
      var bbox = getbox(bpp);
      map.Clear(); if (bmap = (flags & 2) == 0) map.EnsureCapacity((aii.Count + bii.Count) >> 1);
      if (mode == Mode.Difference)
      {
        //if (!touch(abox, bbox))
        if (!((flags & 1) == 0 ? intersect(abox, bbox) : touch(abox, bbox)))
        {
          if (bmap) for (int i = 0; i < aii.Count; i += 3) map.Add(i << 1);
          return;
        }
      }
      else if (mode == Mode.Union)
      {
        //if (!touch(abox, bbox)) { }      
      }
      else //Mode.Intersection
      {
        if (!touch(abox, bbox)) { app.Clear(); aii.Clear(); return; }
      }
      var aee = getplanes(app, aii);
      var bee = getplanes(bpp, bii);
      iii.Clear(); iii.EnsureCapacity(aii.Count + bii.Count);
      ppp.Clear(); ppp.EnsureCapacity(app.Count + bpp.Count);
      if (mode == Mode.Difference)
      {
        Parallel.For(0, aee.Length, i =>
        {
          var e = aee[i];
          if (!touch(bbox, getbox(app, e.ii))) { fetch(0, e); return; }
          var t = Tess.GetInstance();
          var f = t.getff(bpp, e.plane, Math.Max(app.Count, bpp.Count));
          int q = (f & 2) != 0 ? qplane(bee, e.plane) : -1;
          int r = (f & 2) != 0 && (flags & 1) == 0 ? qplane(bee, -e.plane) : -1;
          if (f < 5 && q == -1) { fetch(0, e); return; }
          if (f >= 5) t.cutmesh(bpp, bii, e.plane);
          if (q != -1) t.xor2(bee[q].ii);
          if (r != -1) { t.xor2(bee[r].ii); bee[r] = default; }
          if (t.empty) { fetch(0, e); return; }
          t.tess.Winding = Winding.Positive;
          t.tess.SetNormal(e.plane.Normal);
          t.tess.BeginPolygon(); t.tessel(bpp); t.xor1(e.ii); t.tessel(app);
          t.tess.EndPolygon(); fetch(0, e, t.tess);
        });
        Parallel.For(0, bee.Length, i =>
        {
          var e = bee[i]; if (e.ii == null) return;
          if (!touch(abox, getbox(bpp, e.ii))) return;
          var t = Tess.GetInstance();
          var f = t.getff(app, e.plane, Math.Max(app.Count, bpp.Count)); if (f < 5) return;
          t.cutmesh(app, aii, e.plane);
          t.tess.Winding = Winding.AbsGeqTwo;
          t.tess.SetNormal(e.plane.Normal); t.tess.Options ^= TesselatorR.Option.NormNeg;
          t.tess.BeginPolygon(); t.tessel(app); t.xor2(e.ii); t.tessel(bpp);
          t.tess.EndPolygon(); fetch(1, e, t.tess);
        });
      }
      else if (mode == Mode.Union)
      {
        Parallel.For(0, aee.Length, i =>
        {
          var e = aee[i];
          if (!touch(bbox, getbox(app, e.ii))) { fetch(0, e); return; }
          var t = Tess.GetInstance();
          var f = t.getff(bpp, e.plane, Math.Max(app.Count, bpp.Count));
          int q = (f & 2) != 0 && (flags & 1) == 0 ? qplane(bee, e.plane) : -1;
          if (f < 5 && q == -1) { fetch(0, e); return; }
          if (f >= 5) t.cutmesh(bpp, bii, e.plane);
          if (q != -1) { t.xor1(bee[q].ii); bee[q] = default; }
          t.tess.Winding = Winding.Positive;
          t.tess.SetNormal(e.plane.Normal);
          t.tess.BeginPolygon(); t.tessel(bpp); t.xor1(e.ii); t.tessel(app);
          t.tess.EndPolygon(); fetch(0, e, t.tess);
        });
        Parallel.For(0, bee.Length, i =>
        {
          var e = bee[i]; if (e.ii == null) return;
          if (!touch(abox, getbox(bpp, e.ii))) { fetch(1, e); return; }
          var t = Tess.GetInstance();
          var f = t.getff(app, e.plane, Math.Max(app.Count, bpp.Count));
          int q = (f & 2) != 0 ? qplane(aee, e.plane) : -1; //if (q != -1 && (flags & 1) != 0) continue;
          if (f < 5 && q == -1) { fetch(1, e); return; }
          if (f >= 5) t.cutmesh(app, aii, e.plane);
          if (q != -1) t.xor2(aee[q].ii);
          t.tess.Winding = Winding.Positive;
          t.tess.SetNormal(e.plane.Normal);
          t.tess.BeginPolygon(); t.tessel(app); t.xor1(e.ii); t.tessel(bpp);
          t.tess.EndPolygon(); fetch(1, e, t.tess);
        });
      }
      else //Mode.Intersection
      {
        Parallel.For(0, aee.Length, i =>
        {
          var e = aee[i]; if (!touch(bbox, getbox(app, e.ii))) return;
          var t = Tess.GetInstance();
          var f = t.getff(bpp, e.plane, Math.Max(app.Count, bpp.Count));
          int q = (f & 2) != 0 ? qplane(bee, e.plane) : -1;
          if (f < 5 && q == -1) return;
          if (f >= 5) t.cutmesh(bpp, bii, e.plane);
          if (q != -1) t.xor2(bee[q].ii);
          t.tess.Winding = Winding.AbsGeqTwo;
          t.tess.SetNormal(e.plane.Normal);
          t.tess.BeginPolygon(); t.tessel(bpp); t.xor2(e.ii); t.tessel(app);
          t.tess.EndPolygon(); fetch(0, e, t.tess);
        });
        Parallel.For(0, bee.Length, i =>
        {
          var e = bee[i]; if (!touch(abox, getbox(bpp, e.ii))) return;
          var t = Tess.GetInstance();
          var f = t.getff(app, e.plane, Math.Max(app.Count, bpp.Count)); if (f < 5) return;
          t.cutmesh(app, aii, e.plane);
          t.tess.Winding = Winding.AbsGeqTwo;
          t.tess.SetNormal(e.plane.Normal);
          t.tess.BeginPolygon(); t.tessel(app); t.xor2(e.ii); t.tessel(bpp);
          t.tess.EndPolygon(); fetch(1, e, t.tess);
        });
      }
      app.Clear(); app.AddRange(ppp.Keys);
      aii.Clear(); aii.AddRange(iii);
      Tess.GetInstance().join(app, aii, bmap ? map : null);
    }
    public bool Cut(PlaneR plane, int flags = 0)
    {
      var ff = getff(app.Count); var fa = Tess.getff(ff, app, plane);
      if ((fa & 4) == 0) { return false; }
      if ((fa & 1) == 0) { app.Clear(); aii.Clear(); return true; }
      iii.Clear(); iii.EnsureCapacity(aii.Count);
      ppp.Clear(); ppp.EnsureCapacity(app.Count);
      map.Clear(); if (bmap = (flags & 2) == 0) map.EnsureCapacity(aii.Count >> 1);

      var ee = getplanes(app, aii);
      Parallel.For(0, ee.Length, i =>
      {
        var e = ee[i]; int f = 0, cii = e.ii.Length; Tess? t = null;
        for (int k = 0; k < cii; k++) f |= ff[e.ii[k]];
        if (f == 2) return; //todo: check, on plain?        
        if ((f & 4) == 0) { fetch(0, e); return; }
        if ((f & 1) == 0) return;
        t = Tess.GetInstance();
        t.cutface(app, ff, e.ii, plane); //if(t.empty) { }
        t.tess.Winding = Winding.NonZero;
        t.tess.SetNormal(e.plane.Normal);
        t.tess.BeginPolygon(); t.tessel(app);
        t.tess.EndPolygon(); fetch(0, e, t.tess);
      });
      //if (true)
      {
        var t = Tess.GetInstance();
        t.getff(app, plane = -plane, app.Count);
        t.cutmesh(app, aii, plane);
        t.tess.Winding = Winding.NonZero;
        t.tess.SetNormal(plane.Normal);
        t.tess.BeginPolygon(); t.tessel(app);
        t.tess.EndPolygon(); var n = iii.Count; add(t.tess);
        if (bmap) map.AddRange(Enumerable.Repeat(1, (iii.Count - n) / 3));
      }
      app.Clear(); app.AddRange(ppp.Keys);
      aii.Clear(); aii.AddRange(iii);
      return true;
    }

    PolyhedronR() { }
    [ThreadStatic] static WeakReference? wr;
    readonly List<Vector3R> app = new(), bpp = new();
    readonly List<int> aii = new(), bii = new(), map = new(), iii = new();
    readonly Dictionary<Vector3R, int> ppp = new();
    readonly Dictionary<int, int> abs = new();
    int[]? ff; PlaneR[]? ee; bool bmap;
    int[] getff(int n)
    {
      return ff != null && ff.Length >= n ? ff : ff = new int[n];
    }
    void add(TesselatorR tess)
    {
      var ii = tess.Indices; var pp = tess.Vertices;
      for (int t = 0, n = ii.Length; t < n; t++) iii.Add(add(pp[ii[t]]));
    }
    void add(List<Vector3R> pp, IList<int> ii)
    {
      for (int t = 0, n = ii.Count; t < n; t++) iii.Add(add(pp[ii[t]]));
    }
    int add(in Vector3R p)
    {
      if (!ppp.TryGetValue(p, out var x)) ppp.Add(p, x = ppp.Count);
      return x;
    }
    void fetch(int ab, in (PlaneR plane, int[] ii, int t) e, TesselatorR? te = null)
    {
      lock (ppp)
      {
        var n = iii.Count;
        if (te != null) add(te); else add(ab == 0 ? app : bpp, e.ii);
        if (bmap) map.AddRange(Enumerable.Repeat(((e.t * 3) << 1) | ab, (iii.Count - n) / 3));
      }
    }
    sealed class Tess
    {
      internal static Tess GetInstance()
      {
        if (wr == null || wr.Target is not Tess p)
          wr = new WeakReference(p = new Tess());
        return p;
      }
      [ThreadStatic] static WeakReference? wr;
      internal readonly TesselatorR tess = TesselatorR.GetInstance();
      int[] ff = Array.Empty<int>();
      readonly Dictionary<int, int> abs = new();
      readonly Dictionary<(int a, int b), int> abi = new();
      readonly List<Vector3R> pps = new();
      internal bool empty => abs.Count == 0;
      internal int getff(IReadOnlyList<Vector3R> pp, in PlaneR plane, int nx)
      {
        if (ff.Length < nx) ff = new int[nx];
        return getff(ff, pp, plane);
      }
      internal static int getff(int[] ff, IReadOnlyList<Vector3R> pp, in PlaneR plane)
      {
        int np = pp.Count, fa = 0;
        for (int i = 0; i < np; i++)
          fa |= ff[i] = 1 << (1 - PlaneR.DotCoordSign(plane, pp[i]));
        return fa;
      }
      internal unsafe void cutface(List<Vector3R> pp, int[] ff, int[] ii, in PlaneR plane)
      {
        abi.Clear(); pps.Clear(); decimal m; var ss = (int*)&m;
        for (int t = 0; t < ii.Length; t += 3)
        {
          int ns = 0;
          for (int j = 0, k = 2; j < 3; k = j++)
          {
            int i1, i2, f1 = ff[i1 = ii[t + k]], f2 = ff[i2 = ii[t + j]];
            if (f1 != 4) ss[ns++] = i1;
            if (f1 == f2 || ((f1 | f2) & 2) != 0) continue;
            var ab = (Math.Min(i1, i2), Math.Max(i1, i2));
            if (abi.TryGetValue(ab, out var x)) { ss[ns++] = x; continue; }
            abi.Add(ab, ss[ns++] = pp.Count + pps.Count);
            pps.Add(PlaneR.Intersect(plane, pp[i1], pp[i2]));
          }
          if (ns == 0) continue; Debug.Assert(ns == 3 || ns == 4);
          for (int j = ns - 1, k = 0; k < ns; j = k++) xor(ss[j], ss[k]);
        }
      }
      internal void cutmesh(IReadOnlyList<Vector3R> pp, IReadOnlyList<int> ii, in PlaneR plane)
      {
        abi.Clear(); pps.Clear();
        for (int t = 0; t < ii.Count; t += 3)
        {
          var fa = ff[ii[t]] | ff[ii[t + 1]] | ff[ii[t + 2]];
          if (fa < 5) continue;
          int x1 = -1, x2 = -1;
          for (int j = 0, k = 2; j < 3; k = j++)
          {
            int i1, i2, f1 = ff[i1 = ii[t + k]], f2 = ff[i2 = ii[t + j]];
            if (f1 == 2)
            {
              if (f2 == 2) { f2 = ff[ii[t + (j + 1) % 3]]; f2 = f2 == 1 ? 4 : 1; }
              if (f2 == 1) x2 = i1; else x1 = i1;
              continue;
            }
            if ((f1 | f2) != 5) continue;
            var ab = (Math.Min(i1, i2), Math.Max(i1, i2));
            if (!abi.TryGetValue(ab, out var x))
            {
              abi.Add(ab, x = pp.Count + pps.Count);
              pps.Add(PlaneR.Intersect(plane, pp[i1], pp[i2]));
            }
            if (f1 == 1) x1 = x; else x2 = x;
          }
          if (x1 == -1 || x2 == -1) continue;
          add(x1, x2);
        }
      }
      internal void tessel(IReadOnlyList<Vector3R> pp)
      {
        for (var np = pp.Count; abs.Count != 0;)
        {
          var e = abs.GetEnumerator(); e.MoveNext();
          var t = e.Current;
          var s = t.Key & 0x00ffffff;
          var a = t.Value; rem(s, a);
          tess.BeginContour();
          tess.AddVertex(s < np ? pp[s] : pps[s - np]);
          for (; ; )
          {
            tess.AddVertex(a < np ? pp[a] : pps[a - np]);
            if (!rem(a, out var b)) { abs.Clear(); throw new Exception(err); }
            if (b == s) break; a = b;
          }
          tess.EndContour();
        }
      }
      internal void xor1(int[] ii)
      {
        for (int t = 0; t < ii.Length; t++) xor(ii[t], ii[mod(t)]);
      }
      internal void xor2(int[] ii)
      {
        for (int t = 0; t < ii.Length; t++) xor(ii[mod(t)], ii[t]);
      }
      void xor(int a, int b)
      {
        if (!rem(b, a)) add(a, b);
      }
      void add(int a, int b)
      {
        while (abs.ContainsKey(a)) a += 0x01000000;
        abs.Add(a, b); Debug.Assert(a >= 0);
      }
      bool rem(int a, int b)
      {
        for (int t; abs.TryGetValue(a, out var x); a += 0x01000000)
        {
          if (x != b) continue; abs.Remove(a);
          for (; abs.TryGetValue(t = a + 0x01000000, out x); abs.Remove(t), abs.Add(a, x), a = t) ;
          return true;
        }
        return false;
      }
      bool rem(int a, out int b)
      {
        if (!abs.TryGetValue(a, out b)) return false; abs.Remove(a);
        for (int t; abs.TryGetValue(t = a + 0x01000000, out var x); abs.Remove(t), abs.Add(a, x), a = t) ;
        return true;
      }
      static int mod(int i) => i % 3 != 2 ? i + 1 : i - 2;
      internal void join(List<Vector3R> pp, List<int> ii, List<int>? map)
      {
        Debug.Assert(abs.Count == 0);
        for (int i = 0; i < ii.Count; i++) xor(ii[i], ii[mod(i)]);
        if (abs.Count == 0) return;
        for (var e = abs.GetEnumerator(); e.MoveNext();)
        {
          if (!abs.TryGetValue(e.Current.Value, out var c)) continue;
          int a = e.Current.Key & 0x00ffffff, b = e.Current.Value;
          if (Vector3R.PtInline(pp[a], pp[b], pp[c]) != 3) continue;
          xor(b, a); xor(a, c); xor(c, b);
          int u = 0, v; for (; ii[u] != a || ii[v = mod(u)] != b; u++) ;
          ii[v] = c; ii.Add(ii[mod(v)]); ii.Add(c); ii.Add(b);
          if (map != null) map.Add(map[u / 3]);
          if (abs.Count == 0) return; e = abs.GetEnumerator();
        }
        abs.Clear(); throw new Exception(err);
      }
    }
    const string err = "Invalid input polyhedron";
    (PlaneR plane, int[] ii, int t)[] getplanes(List<Vector3R> pp, List<int> ii)
    {
      var nk = ii.Count / 3;
      if (ee == null || ee.Length < nk) ee = new PlaneR[Math.Max(aii.Count, bii.Count) >> 1];
      Parallel.For(0, nk, k => { int t; ee[k] = PlaneR.FromVertices(pp[ii[t = k * 3]], pp[ii[t + 1]], pp[ii[t + 2]]); });
      return Enumerable.Range(0, nk).GroupBy(i => ee[i]).Select(p =>
      {
        var a = (IList<int>)p; var b = new int[a.Count * 3];
        for (int i = 0, j = 0, k; i < b.Length; i += 3)
        {
          b[i] = ii[k = a[j++] * 3];
          b[i + 1] = ii[k + 1];
          b[i + 2] = ii[k + 2];
        }
        return (p.Key, b, a[0]);
      }).ToArray();
    }
    static (Vector3R min, Vector3R max) getbox(List<Vector3R> pp)
    {
      var box = (min: pp[0], max: pp[0]);
      for (int n = pp.Count, i = 1; i < n; i++)
      {
        var p = pp[i];
        box.min = Vector3R.Min(box.min, p);
        box.max = Vector3R.Max(box.max, p);
      }
      return box;
    }
    static (Vector3R min, Vector3R max) getbox(List<Vector3R> pp, IList<int> ii)
    {
      var p = pp[ii[0]]; var box = (min: p, max: p);
      for (int i = 1; i < ii.Count; i++) //todo: distinct
      {
        p = pp[ii[i]];
        box.min = Vector3R.Min(box.min, p);
        box.max = Vector3R.Max(box.max, p);
      }
      return box;
    }
    static bool intersect(in (Vector3R min, Vector3R max) a, in (Vector3R min, Vector3R max) b)
    {
      if (a.max.X <= b.min.X) return false;
      if (a.max.Y <= b.min.Y) return false;
      if (a.max.Z <= b.min.Z) return false;
      if (a.min.X >= b.max.X) return false;
      if (a.min.Y >= b.max.Y) return false;
      if (a.min.Z >= b.max.Z) return false;
      return true;
    }
    static bool touch(in (Vector3R min, Vector3R max) a, in (Vector3R min, Vector3R max) b)
    {
      if (a.max.X < b.min.X) return false;
      if (a.max.Y < b.min.Y) return false;
      if (a.max.Z < b.min.Z) return false;
      if (a.min.X > b.max.X) return false;
      if (a.min.Y > b.max.Y) return false;
      if (a.min.Z > b.max.Z) return false;
      return true;
    }
    static int qplane((PlaneR plane, int[] ii, int t)[] bee, in PlaneR plane)
    {
      for (int t = 0; t < bee.Length; t++) if (plane.Equals(bee[t].plane)) return t;
      return -1;
    }
  }
}
