using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
  /// <summary>
  /// Intersections and boolean operations on arbitrary polyhedral meshes based on <see cref="rat"/>.<br/>
  /// <i>This is just a non-optimal example implementation for testing!</i>
  /// </summary>
  public static class PolyhedronR
  {
    public enum Mode { Union, Difference, Intersection }
    /// <summary>
    /// Calculates the union, difference or intersection of a polyhedron A and a polyhedron B, both defined by a list of points and indices.<br/>
    /// Returns the result in the lists of polyhedron A.
    /// </summary>
    /// <remarks>
    /// <i>Only works for real polyhedra without openings or self-intersections.</i>
    /// </remarks>
    /// <param name="mode">Specifies the boolean operation.</param>
    /// <param name="app">Points of polyhedron A.</param>
    /// <param name="aii">Indices of polyhedron A.</param>
    /// <param name="bpp">Points of polyhedron B.</param>
    /// <param name="bii">Indices of polyhedron B.</param>
    /// <param name="map">
    /// If not null, returns the source polygon indices of the corresponding result polygons.<br/>
    /// This list can be used to restore material ranges etc.
    /// </param>
    public static void Boolean(Mode mode, List<Vector3R> app, List<int> aii, List<Vector3R> bpp, List<int> bii, List<int>? map = null)
    {
      if (mode == Mode.Union) Union(app, aii, bpp, bii, map);
      else if (mode == Mode.Intersection) Intersect(app, aii, bpp, bii, map);
      else Difference(app, aii, bpp, bii, map);
    }
    /// <summary>
    /// Calculates the difference between a polyhedron A defined by a list of points and indices,<br/>
    /// and the half-space B defined by a <see cref="PlaneR"/> <paramref name="plane"/>.<br/>
    /// Returns the result in the lists of polyhedron A.
    /// </summary>
    /// <remarks>
    /// <i>Only works for real polyhedra without openings or self-intersections.</i>
    /// </remarks>
    /// <param name="pp">Points of polyhedron A.</param>
    /// <param name="ii">Indices of polyhedron A.</param>
    /// <param name="plane">A plane that defines the half-space.</param>
    /// <param name="bii">Indices of polyhedron B.</param>
    /// <param name="map">
    /// If not null, returns the source polygon indices of the corresponding result polygons.<br/>
    /// This list can be used to restore material ranges etc.
    /// </param>
    /// <returns>True if there is an intersection, false otherwise.</returns>
    public static bool Cut(List<Vector3R> pp, List<int> ii, PlaneR plane, List<int>? map = null)
    {
      var ff = Pool.RentArray<int>(pp.Count); var fa = getff(ff, pp, plane);
      if ((fa & 4) == 0) { Pool.Return(ff); return false; }
      if ((fa & 1) == 0) { Pool.Return(ff); pp.Clear(); ii.Clear(); return true; }
      int np = pp.Count, ni = ii.Count;

      var tess = Pool.Rent<TesselatorR>();
      tess.Winding = Winding.NonZero;
      tess.Options = TesselatorR.Option.Fill | TesselatorR.Option.Delaunay;

      var iii = Pool.Rent<List<int>>(); iii.Clear(); iii.EnsureCapacity(ni);
      var ppp = Pool.Rent<Dictionary<Vector3R, int>>(); ppp.Clear(); ppp.EnsureCapacity(np);
      var abi = Pool.Rent<Dictionary<(int a, int b), int>>();
      var abs = Pool.Rent<Dictionary<int, int>>();

      var ee = getplanes(pp, ii);
      for (int i = 0; i < ee.Length; i++)
      {
        var e = ee[i]; int f = 0, cii = e.ii.Length, nii = iii.Count;
        for (int t = 0; t < cii; t++) f |= ff[e.ii[t]];
        if (f == 2) continue; //todo: check, on plain?        
        if ((f & 4) == 0) { add(ppp, iii, pp, e.ii); goto m1; }
        if ((f & 1) == 0) continue;
        var nx = pp.Count;
        cutface(pp, ff, e.ii, plane, abs, abi); //if(abs.Count == 0) { }
        tess.SetNormal(e.plane.Normal);
        tess.BeginPolygon();
        tessel(tess, pp, abs); pp.RemoveRange(nx, pp.Count - nx);
        tess.EndPolygon();
        add(ppp, iii, tess); m1: if (map != null) map.AddRange(Enumerable.Repeat(xmap(ii, e.ii) << 1, (iii.Count - nii) / 3));
      }

      pp.Clear(); pp.AddRange(ppp.Keys); np = pp.Count;
      ii.Clear(); ii.AddRange(iii); ni = ii.Count;

      if (ff.Length < np) { Pool.Return(ff); ff = Pool.RentArray<int>(np); }
      for (int i = 0; i < np; i++) ff[i] = PlaneR.DotCoordSign(plane, pp[i]);

      abs.Clear();
      for (int i = 0, k; i < ni; i++)
        if (ff[ii[i]] == 0 && ff[ii[k = mod(i)]] == 0)
          add(abs, ii[k], ii[i]);

      tess.SetNormal(plane.Normal); //tess.Options ^= TesselatorR.Option.NormNeg;
      tess.BeginPolygon();
      tessel(tess, pp, abs);
      tess.EndPolygon();
      iii.Clear(); var t2 = ppp.Count;
      add(ppp, iii, tess); Debug.Assert(t2 == ppp.Count); //for (int i = 0; i < iii.Count; i += 3) { var t = iii[i]; iii[i] = iii[i + 1]; iii[i + 1] = t; }
      ii.AddRange(iii);

      if (map != null) { map.AddRange(Enumerable.Repeat(1, iii.Count / 3)); Debug.Assert(map.Count * 3 == ii.Count); }

      ppp.Clear(); Pool.Return(ppp); Pool.Return(iii);
      Pool.Return(abi); Pool.Return(abs); Pool.Return(ff);
      Pool.Return(tess);
      return true;
    }

    public static bool Difference(List<Vector3R> app, List<int> aii, List<Vector3R> bpp, List<int> bii, List<int>? map = null)
    {
      var abox = getbox(app);
      var bbox = getbox(bpp);
      if (!touch(abox, bbox))//!intersect(abox, bbox))
      {
        if (map != null) for (int i = 0; i < aii.Count; i += 3) map.Add(i << 1);
        return false;
      }
      var aee = getplanes(app, aii);
      var bee = getplanes(bpp, bii);

      var iii = Pool.Rent<List<int>>(); iii.Clear(); iii.EnsureCapacity(aii.Count);
      var ppp = Pool.Rent<Dictionary<Vector3R, int>>(); ppp.Clear(); ppp.EnsureCapacity(app.Count);
      var abs = Pool.Rent<Dictionary<int, int>>();
      var abi = Pool.Rent<Dictionary<(int a, int b), int>>();
      var ff = Pool.RentArray<int>(Math.Max(app.Count, bpp.Count));

      var tess = Pool.Rent<TesselatorR>();
      tess.Options = TesselatorR.Option.Fill | TesselatorR.Option.Delaunay;

      tess.Winding = Winding.Positive;
      for (int i = 0; i < aee.Length; i++)
      {
        var e = aee[i]; var box = getbox(app, e.ii); var nii = iii.Count;
        if (!touch(bbox, box)) { add(ppp, iii, app, e.ii); goto m1; }
        var f = getff(ff, bpp, e.plane);
        int q = (f & 2) != 0 ? qplane(bee, e.plane) : -1;
        if (f < 5 && q == -1) { add(ppp, iii, app, e.ii); goto m1; }
        var nx = bpp.Count;
        if (f >= 5) cutmesh(bpp, ff, bii, e.plane, abs, abi);
        if (q != -1) xor2(abs, bee[q].ii);
        if (abs.Count == 0) { add(ppp, iii, app, e.ii); goto m1; }
        tess.SetNormal(e.plane.Normal);
        tess.BeginPolygon();
        tessel(tess, bpp, abs);
        bpp.RemoveRange(nx, bpp.Count - nx);
        abs.Clear();
        xor1(abs, e.ii);
        tessel(tess, app, abs);
        tess.EndPolygon();
        add(ppp, iii, tess); m1: if (map != null) map.AddRange(Enumerable.Repeat((xmap(aii, e.ii) << 1) | 0, (iii.Count - nii) / 3));
      }
      tess.Winding = Winding.AbsGeqTwo;
      for (int i = 0; i < bee.Length; i++)
      {
        var e = bee[i]; var box = getbox(bpp, e.ii); if (!touch(abox, box)) continue;
        var f = getff(ff, app, e.plane); if (f < 5) continue;
        var nx = app.Count; var nii = iii.Count;
        cutmesh(app, ff, aii, e.plane, abs, abi); if (abs.Count == 0) continue;
        tess.SetNormal(e.plane.Normal); tess.Options ^= TesselatorR.Option.NormNeg;
        tess.BeginPolygon();
        tessel(tess, app, abs); app.RemoveRange(nx, app.Count - nx);
        abs.Clear();
        xor2(abs, e.ii);
        tessel(tess, bpp, abs);
        tess.EndPolygon();
        add(ppp, iii, tess);
        if (map != null) map.AddRange(Enumerable.Repeat((xmap(bii, e.ii) << 1) | 1, (iii.Count - nii) / 3));
      }

      app.Clear(); app.AddRange(ppp.Keys);
      aii.Clear(); aii.AddRange(iii);
      join(app, aii, abs, map);

      ppp.Clear(); Pool.Return(ppp); Pool.Return(iii);
      Pool.Return(ff); Pool.Return(abs); Pool.Return(abi);
      Pool.Return(tess);
      return true;
    }
    public static bool Intersect(List<Vector3R> app, List<int> aii, List<Vector3R> bpp, List<int> bii, List<int>? map = null)
    {
      var abox = getbox(app);
      var bbox = getbox(bpp);
      if (!touch(abox, bbox))//!intersect(abox, bbox)) 
      {
        app.Clear(); aii.Clear();
        return false;
      }
      var aee = getplanes(app, aii);
      var bee = getplanes(bpp, bii);

      var iii = Pool.Rent<List<int>>(); iii.Clear(); iii.EnsureCapacity(aii.Count);
      var ppp = Pool.Rent<Dictionary<Vector3R, int>>(); ppp.Clear(); ppp.EnsureCapacity(app.Count);
      var abs = Pool.Rent<Dictionary<int, int>>();
      var abi = Pool.Rent<Dictionary<(int a, int b), int>>();
      var ff = Pool.RentArray<int>(Math.Max(app.Count, bpp.Count));

      var tess = Pool.Rent<TesselatorR>();
      tess.Options = TesselatorR.Option.Fill | TesselatorR.Option.Delaunay;
      tess.Winding = Winding.AbsGeqTwo;
      for (int i = 0; i < aee.Length; i++)
      {
        var e = aee[i]; var box = getbox(app, e.ii);
        if (!touch(bbox, box)) continue;
        var f = getff(ff, bpp, e.plane);
        int q = (f & 2) != 0 ? qplane(bee, e.plane) : -1;
        if (f < 5 && q == -1) continue;
        var nx = bpp.Count; var nii = iii.Count;
        if (f >= 5) cutmesh(bpp, ff, bii, e.plane, abs, abi);
        if (q != -1) xor2(abs, bee[q].ii);
        if (abs.Count == 0) { Debug.Assert(bpp.Count == nx); continue; }
        tess.SetNormal(e.plane.Normal);
        tess.BeginPolygon();
        tessel(tess, bpp, abs); bpp.RemoveRange(nx, bpp.Count - nx);
        abs.Clear();
        xor2(abs, e.ii);
        tessel(tess, app, abs);
        tess.EndPolygon();
        add(ppp, iii, tess); if (map != null) map.AddRange(Enumerable.Repeat((xmap(aii, e.ii) << 1) | 0, (iii.Count - nii) / 3));
      }
      for (int i = 0; i < bee.Length; i++)
      {
        var e = bee[i]; var box = getbox(bpp, e.ii);
        if (!touch(abox, box)) continue;
        var f = getff(ff, app, e.plane); if (f < 5) continue;
        var nx = app.Count; var nii = iii.Count;
        cutmesh(app, ff, aii, e.plane, abs, abi); if (abs.Count == 0) continue;
        tess.SetNormal(e.plane.Normal);
        tess.BeginPolygon();
        tessel(tess, app, abs); app.RemoveRange(nx, app.Count - nx);
        abs.Clear();
        xor2(abs, e.ii);
        tessel(tess, bpp, abs);
        tess.EndPolygon();
        add(ppp, iii, tess); if (map != null) map.AddRange(Enumerable.Repeat((xmap(bii, e.ii) << 1) | 1, (iii.Count - nii) / 3));
      }

      app.Clear(); app.AddRange(ppp.Keys);
      aii.Clear(); aii.AddRange(iii);
      join(app, aii, abs, map);

      ppp.Clear(); Pool.Return(ppp); Pool.Return(iii);
      Pool.Return(ff); Pool.Return(abs); Pool.Return(abi);
      Pool.Return(tess);
      return true;
    }
    public static bool Union(List<Vector3R> app, List<int> aii, List<Vector3R> bpp, List<int> bii, List<int>? map = null)
    {
      var abox = getbox(app);
      var bbox = getbox(bpp); //if (!touch(abox, bbox)) { }
      var aee = getplanes(app, aii);
      var bee = getplanes(bpp, bii);

      var iii = Pool.Rent<List<int>>(); iii.Clear(); iii.EnsureCapacity(aii.Count);
      var ppp = Pool.Rent<Dictionary<Vector3R, int>>(); ppp.Clear(); ppp.EnsureCapacity(app.Count);
      var abs = Pool.Rent<Dictionary<int, int>>();
      var abi = Pool.Rent<Dictionary<(int a, int b), int>>();
      var ff = Pool.RentArray<int>(Math.Max(app.Count, bpp.Count));

      var tess = Pool.Rent<TesselatorR>();
      tess.Options = TesselatorR.Option.Fill | TesselatorR.Option.Delaunay;
      tess.Winding = Winding.Positive;
      for (int i = 0; i < aee.Length; i++)
      {
        var e = aee[i]; var box = getbox(app, e.ii); var nii = iii.Count;
        if (!touch(bbox, box)) { add(ppp, iii, app, e.ii); goto m1; }
        var f = getff(ff, bpp, e.plane);
        int q = -1;// (f & 2) != 0 ? qplane(bee, e.plane) : -1;
        if (f < 5 && q == -1) { add(ppp, iii, app, e.ii); goto m1; }
        var nx = bpp.Count;
        if (f >= 5) cutmesh(bpp, ff, bii, e.plane, abs, abi);
        //if (q != -1) xor1(abs, bee[q].ii);
        if (q != -1) xor2(abs, bee[q].ii);
        if (abs.Count == 0) { }
        tess.SetNormal(e.plane.Normal);
        tess.BeginPolygon();
        tessel(tess, bpp, abs); bpp.RemoveRange(nx, bpp.Count - nx); abs.Clear();
        xor1(abs, e.ii);
        tessel(tess, app, abs);
        tess.EndPolygon();
        add(ppp, iii, tess); m1: if (map != null) map.AddRange(Enumerable.Repeat((xmap(aii, e.ii) << 1) | 0, (iii.Count - nii) / 3));
      }
      for (int i = 0; i < bee.Length; i++)
      {
        var e = bee[i]; if (e.ii == null) continue; var box = getbox(bpp, e.ii); var nii = iii.Count;
        if (!touch(abox, box)) { add(ppp, iii, bpp, e.ii); goto m1; }
        var f = getff(ff, app, e.plane);
        int q = (f & 2) != 0 ? qplane(aee, e.plane) : -1;
        if (f < 5 && q == -1) { add(ppp, iii, bpp, e.ii); goto m1; }
        var nx = app.Count;
        if (f >= 5) cutmesh(app, ff, aii, e.plane, abs, abi);
        if (q != -1) xor2(abs, aee[q].ii);
        if (abs.Count == 0) { }
        tess.SetNormal(e.plane.Normal);
        tess.BeginPolygon();
        tessel(tess, app, abs); app.RemoveRange(nx, app.Count - nx);
        abs.Clear();
        xor1(abs, e.ii);
        tessel(tess, bpp, abs);
        tess.EndPolygon();
        add(ppp, iii, tess); m1: if (map != null) map.AddRange(Enumerable.Repeat((xmap(bii, e.ii) << 1) | 1, (iii.Count - nii) / 3));
      }
      app.Clear(); app.AddRange(ppp.Keys);
      aii.Clear(); aii.AddRange(iii);
      join(app, aii, abs, map);

      ppp.Clear(); Pool.Return(ppp); Pool.Return(iii);
      Pool.Return(ff); Pool.Return(abs); Pool.Return(abi);
      Pool.Return(tess);
      return true;
    }

    const string err = "Invalid input polyhedron";
    static (PlaneR plane, int[] ii)[] getplanes(List<Vector3R> pp, List<int> ii)
    {
      var nk = ii.Count / 3;
      var ee = Pool.RentArray<PlaneR>(nk);
      Parallel.For(0, nk, k => ee[k] = PlaneR.FromVertices(pp[ii[k * 3]], pp[ii[k * 3 + 1]], pp[ii[k * 3 + 2]]));
      var ed = Pool.Rent<Dictionary<PlaneR, int>>(); ed.Clear();
      var kk = Pool.Rent<List<int>>(); kk.Clear(); kk.EnsureCapacity(nk);
      add2(kk, ed, ee, nk);
      var re = group(ed, kk, nk, ii);
      Pool.Return(ee); Pool.Return(ed); Pool.Return(kk);
      return re;
    }
    static (PlaneR plane, int[] ii)[] group(Dictionary<PlaneR, int> ed, List<int> kk, int nk, List<int> ii)
    {
      var ne = ed.Count;
      var re = new (PlaneR plane, int[] ii)[ne];
      var en = ed.GetEnumerator();
      for (int i = 0; en.MoveNext(); re[i++].plane = en.Current.Key) ;
      int nl = 0; var ll = Pool.RentArray<int>(ne + (nk << 1));
      for (int e = 0; e < ne; e++) ll[nl++] = -1;
      for (int k = nk - 1; k >= 0; k--) { var e = kk[k]; ll[nl++] = k; ll[nl++] = ll[e]; ll[e] = nl - 2; }
      for (int e = 0; e < ne; e++)
      {
        int c = 0; for (int t = ll[e]; t != -1; t = ll[t + 1]) c++;
        var tt = new int[c * 3];
        for (int t = ll[e], j = 0, x; t != -1; t = ll[t + 1])
        {
          tt[j++] = ii[x = ll[t] * 3];
          tt[j++] = ii[x + 1];
          tt[j++] = ii[x + 2];
        }
        re[e].ii = tt;
      }
      Pool.Return(ll);
      return re;
    }
    static int add1<T>(Dictionary<T, int> dict, T value) where T : notnull
    {
      if (!dict.TryGetValue(value, out var x)) dict.Add(value, x = dict.Count);
      return x;
    }
    static void add2<T>(List<int> ii, Dictionary<T, int> dict, IList<T> values, int count) where T : notnull
    {
      for (int i = 0; i < count; i++) ii.Add(add1(dict, values[i]));
    }

    static int getff(int[] ff, List<Vector3R> pp, in PlaneR plane)
    {
      int np = pp.Count, fa = 0;
      for (int i = 0; i < np; i++)
        fa |= ff[i] = 1 << (1 - PlaneR.DotCoordSign(plane, pp[i]));
      return fa;
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
    static (Vector3R min, Vector3R max) getbox(List<Vector3R> pp, List<(int a, int b)> ii)
    {
      var p = pp[ii[0].a]; var box = (min: p, max: p);
      for (int i = 1; i < ii.Count; i++)
      {
        p = pp[ii[i].a];
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

    static void add(Dictionary<int, int> abs, int a, int b)
    {
      while (abs.ContainsKey(a)) a += 0x01000000;
      abs.Add(a, b); Debug.Assert(a >= 0);
    }
    static bool rem(Dictionary<int, int> abs, int a, int b)
    {
      for (int t; abs.TryGetValue(a, out var x); a += 0x01000000)
      {
        if (x != b) continue; abs.Remove(a);
        for (; abs.TryGetValue(t = a + 0x01000000, out x); abs.Remove(t), abs.Add(a, x), a = t) ;
        return true;
      }
      return false;
    }
    static bool rem(Dictionary<int, int> abs, int a, out int b)
    {
      if (!abs.TryGetValue(a, out b)) return false; abs.Remove(a);
      for (int t; abs.TryGetValue(t = a + 0x01000000, out var x); abs.Remove(t), abs.Add(a, x), a = t) ;
      return true;
    }
    static void xor(Dictionary<int, int> abs, int a, int b)
    {
      if (!rem(abs, b, a)) add(abs, a, b);
    }
    static void xor1(Dictionary<int, int> abs, int[] ii)
    {
      for (int t = 0; t < ii.Length; t++) xor(abs, ii[t], ii[mod(t)]);
    }
    static void xor2(Dictionary<int, int> abs, int[] ii)
    {
      for (int t = 0; t < ii.Length; t++) xor(abs, ii[mod(t)], ii[t]);
    }

    static int mod(int i) => i % 3 != 2 ? i + 1 : i - 2;
    static void add(Dictionary<Vector3R, int> ppp, List<int> iii, TesselatorR tess)
    {
      var ii = tess.Indices; var pp = tess.Vertices;
      for (int t = 0, n = ii.Length; t < n; t++) iii.Add(add(ppp, pp[ii[t]]));
    }
    static int add(Dictionary<Vector3R, int> dict, in Vector3R p)
    {
      if (!dict.TryGetValue(p, out var x)) dict.Add(p, x = dict.Count);
      return x;
    }
    static void add(Dictionary<Vector3R, int> ppp, List<int> iii, List<Vector3R> pp, IList<int> ii)
    {
      for (int t = 0, n = ii.Count; t < n; t++) iii.Add(add(ppp, pp[ii[t]]));
    }
    static unsafe void cutface(List<Vector3R> pp, int[] ff, int[] ii, in PlaneR plane,
      Dictionary<int, int> abs, Dictionary<(int a, int b), int> abi)
    {
      abs.Clear(); abi.Clear(); decimal m; var ss = (int*)&m;
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
          abi.Add(ab, ss[ns++] = pp.Count);
          pp.Add(PlaneR.Intersect(plane, pp[i1], pp[i2]));
        }
        if (ns == 0) continue; Debug.Assert(ns == 3 || ns == 4);
        for (int j = ns - 1, k = 0; k < ns; j = k++) xor(abs, ss[j], ss[k]);
      }
    }
    static unsafe void cutmesh(List<Vector3R> pp, int[] ff, IList<int> ii, in PlaneR plane,
      Dictionary<int, int> abs, Dictionary<(int a, int b), int> abi)
    {
      abs.Clear(); abi.Clear();
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
            abi.Add(ab, x = pp.Count);
            pp.Add(PlaneR.Intersect(plane, pp[i1], pp[i2]));
          }
          if (f1 == 1) x1 = x; else x2 = x;
        }
        if (x1 == -1 || x2 == -1) continue;
        add(abs, x1, x2);
      }
    }
    static void tessel(TesselatorR tess, List<Vector3R> pp, Dictionary<int, int> abs)
    {
      while (abs.Count != 0)
      {
        var e = abs.GetEnumerator(); e.MoveNext();
        var t = e.Current;
        var s = t.Key & 0x00ffffff;
        var a = t.Value; rem(abs, s, a);
        tess.BeginContour();
        tess.AddVertex(pp[s]);
        for (; ; )
        {
          tess.AddVertex(pp[a]);
          if (!rem(abs, a, out var b)) throw new Exception(err);
          if (b == s) break; a = b;
        }
        tess.EndContour();
      }
    }
    static void join(List<Vector3R> pp, List<int> ii, Dictionary<int, int> abs, List<int>? map)
    {
      if (abs.Count != 0) abs.Clear();
      for (int i = 0; i < ii.Count; i++) xor(abs, ii[i], ii[mod(i)]);
      if (abs.Count == 0) return;
      for (var e = abs.GetEnumerator(); e.MoveNext();)
      {
        if (!abs.TryGetValue(e.Current.Value, out var c)) continue;
        int a = e.Current.Key & 0x00ffffff, b = e.Current.Value;
        if (Vector3R.PtInline(pp[a], pp[b], pp[c]) != 3) continue;
        xor(abs, b, a); xor(abs, a, c); xor(abs, c, b);
        int u = 0, v; for (; ii[u] != a || ii[v = mod(u)] != b; u++) ;
        ii[v] = c; ii.Add(ii[mod(v)]); ii.Add(c); ii.Add(b);
        if (map != null) map.Add(map[u / 3]);
        if (abs.Count == 0) return; e = abs.GetEnumerator();
      }
      throw new Exception(err);
    }
    static int xmap(List<int> ii, int[] tt)
    {
      int t; for (t = 0; t < ii.Count && !(ii[t] == tt[0] && ii[t + 1] == tt[1]); t += 3) ;
      Debug.Assert(t < ii.Count); return t;
    }
    static int qplane((PlaneR plane, int[] ii)[] bee, in PlaneR plane)
    {
      for (int t = 0; t < bee.Length; t++) if (plane.Equals(bee[t].plane)) return t;
      return -1;
    }

    public static class Pool
    {
      static GCHandle[] pool = new GCHandle[4]; static int count;
      public static T Rent<T>() where T : new()
      {
        //lock (pool)
        for (int i = 0; i < count; i++)
          if (pool[i].Target is T t)
          {
            pool[i].Target = null; return t;
          }
        return new T();
      }
      public static T[] RentArray<T>(int n)
      {
        //lock (pool)
        for (int i = 0; i < count; i++)
          if (pool[i].Target is T[] a && a.Length >= n)
          {
            pool[i].Target = null; return a;
          }
        return new T[n];
      }
      public static void Return(object p)
      {
        //lock (pool)
        {
          for (int i = 0; i < count; i++)
            if (pool[i].Target == null) { pool[i].Target = p; return; }
          if (pool.Length == count) Array.Resize(ref pool, count << 1);
          pool[count++] = GCHandle.Alloc(p, GCHandleType.Weak);
        }
      }
    }
  }
}
