using System.Collections;
using System.Numerics;
#pragma warning disable CS8602

namespace Test
{
  public enum Winding { EvenOdd = 0, NonZero = 1, Positive = 2, Negative = 3, AbsGeqTwo = 4, AbsGeqThree = 5 }

  /// <summary>
  /// Tesselator based on <see cref="rat"/>.<br/>
  /// <i>This is just a non-optimal example implementation for testing!</i>
  /// </summary>
  public class TesselatorR
  {
    public Winding Winding = Winding.EvenOdd;
    public Option Options = Option.Fill | Option.Delaunay | Option.OutlinePrecise | Option.Trim;
    public void SetNormal(in Vector3R v)
    {
      var i = Vector3R.LongAxis(v);
      var s = rat.Sign(i == 0 ? v.X : i == 1 ? v.Y : v.Z);
      Options = (Options & (Option)0xffff) | (Option)((int)Option.NormX << i) | (Option)(s & (int)Option.NormNeg);
    }
    public void BeginPolygon()
    {
      //Debug.Assert(state != 1 && state != 2);
      if (state == 1 || state == 2) { state = 0; throw new Exception(); }
      state = 1; np = 0; cpu.pop(unchecked((int)cpu.mark()));
    }
    public void BeginContour()
    {
      if (state != 1) { state = 0; throw new Exception(); }
      state = 2; fi = np;
    }
    public void AddVertex(rat x, rat y, rat z = default)
    {
      cpu.push(x); cpu.push(y); cpu.push(z); enter();
      //if (state != 2) { state = 0; throw new Exception(); }
      //if (np == pp.Length) resize(0);
      //if (np != fi) pp[np - 1].next = np; pp[np++].next = fi;
      //cpu.push(x); cpu.push(y); cpu.push(z); cpu.push(0, 5);
      //if(shv == 0 && np != 0) { var t = cpu.mark(); if (cpu.equ(t - 7, t - 15)) shv = 1;  }
    }
    public void AddVertex(float x, float y, float z = 0)
    {
      cpu.push(x); cpu.norm();
      cpu.push(y); cpu.norm();
      cpu.push(z); cpu.norm(); enter();
      //var m = cpu.mark(); AddVertex(default(rat), default(rat), default(rat));
      //cpu.push(x); cpu.norm(); cpu.swp(m + 0); cpu.pop();
      //cpu.push(y); cpu.norm(); cpu.swp(m + 1); cpu.pop();
      //cpu.push(z); cpu.norm(); cpu.swp(m + 2); cpu.pop();
    }
    public void AddVertex(Vector2 p)
    {
      AddVertex(p.X, p.Y, 0);
    }
    public void AddVertex(in Vector3R p)
    {
      AddVertex(p.X, p.Y, p.Z);
    }
    public void AddVertex(int x, int y)
    {
      cpu.push(x); cpu.push(y); cpu.push(); enter();
    }
    public void EndContour()
    {
      if (state != 2) { state = 0; throw new Exception(); }
      state = 1;
    }
    public void EndPolygon()
    {
      if (state != 1) { state = 0; throw new Exception(); }
      var pro = project(0);
      ns = nl = nc = this.ni = 0; int ab = 0, np = this.np, shv = 1; trim:
      if (shv != 0)
      {
        var t = cpu.mark(); cpu.push(+123);
        for (int i = 0; i < this.np; i++)
        {
          var p = (uint)(i * 8); // p.y = p.y + p.x / kill;
          if (shv == 1) { cpu.dup(p + 1); cpu.swp(p + 7); cpu.pop(); } //save org y
          cpu.div(p + 0, t); cpu.add(p + 1); cpu.norm();
          cpu.swp(p + 1); cpu.pop();
        }
        cpu.pop();
      }
      cdict(hash); int ni = 0;
      for (int i = 0, d; i < np; i++)
      {
        var a = (uint)(i << 3); ref var u = ref pp[i];
        var b = (uint)(u.next * 8);
        //var dx = b.x - a.x; var dy = b.y - a.y;
        //if ((d = rat.Sign(dy)) == 0) { /*m.end();*/ if (rat.Sign(dx) == 0) continue; shv++; goto trim; }
        //a.a = dx / dy; a.f = a.x - a.y * a.a; a.x2 = d > 0 ? a.x : b.x;
        cpu.sub(b + 0, a + 0); cpu.sub(b + 1, a + 1);
        if ((d = cpu.sign()) == 0) { var t = cpu.sign(1); cpu.pop(2); if (t == 0) continue; shv++; goto trim; }
        cpu.div(); cpu.norm(); cpu.swp(a + 3); cpu.pop();
        cpu.mul(a + 1, a + 3); cpu.neg();
        cpu.add(a + 0); cpu.norm(); cpu.swp(a + 4); cpu.pop();
        cpu.dup(d > 0 ? a + 0 : b + 0); //a.x2 = d > 0 ? a.x : b.x;
        cpu.swp(a + 6); cpu.pop();
        kk[ni++] = (i, d > 0 ? i : u.next); u.line = -1; add(a + 0, a + 1, i);
      }
      sort(cmpy, kk, ni); if (this.ni == -1) { this.ni = 0; shv++; goto trim; }
      uint y1, y2 = default, tmp = 0; int active = 0, nfl = 0;
      for (int l = 0; l < ni;)
      {
        for (y1 = (uint)(kk[l].b * 8 + 1); ;)
        {
          kk[active++] = kk[l];
          if (++l == ni || !cpu.equ(y2 = (uint)(kk[l].b * 8 + 1), y1)) break;
        }
        for (var y3 = y2; ; y1 = y2, y2 = y3)
        {
          for (int t = active, j; t-- != 0;)
          {
            var next = pp[j = kk[t].a].next;
            if (next < 0)
            {
              ref var w = ref pp[j]; //a.x1 = a.x2;
              cpu.dup((uint)(j * 8 + 6)); cpu.swp((uint)(j * 8 + 5)); cpu.pop();
              if (w.line != -1) { pp[nfl++].fl = j | (w.line << 16); w.line = -1; }
              active--; for (int s = t; s < active; s++) kk[s] = kk[s + 1]; goto next;
            }
            var y = (uint)((j != kk[t].b ? j : next) * 8 + 1);
            if (l != ni) { if (cpu.cmp(y, y2) < 0) y2 = y; continue; }
            if (cpu.cmp(y, y3) > 0) y3 = y; next:
            if (t == 0 && l == ni) { y2 = y3; t = active; l++; }
          }
          ////////////
          for (int t = 0; t < active; t++)
          {
            var p = (uint)(kk[t].a * 8); //p.x1 = p.x2; p.x2 = p.f + y2 * p.a;
            cpu.swp(p + 5, p + 6);
            cpu.mul(y2, p + 3); cpu.add(p + 4);
            cpu.swp(p + 6); cpu.pop();
          }
          sort(cmpx, kk, active);
          int h = active, nc = 0; tmp ^= 1;
          for (int i = 1; i < active; i++)
          {
            var a = (uint)(kk[i - 1].a * 8); var b = (uint)(kk[i].a * 8);
            if (cpu.cmp(a + 6, b + 6) <= 0) continue;
            //var t = (b.f - a.f) / (a.a - b.a); y2 = (h == 0 ? rat.Min(y2, t) : t); h = 0;
            cpu.sub(b + 4, a + 4); cpu.sub(a + 3, b + 3); cpu.div();
            if (h != 0 || cpu.cmp(y2) < 0) { cpu.norm(); cpu.swp(y2 = (uint)(tmp * 8 + 7)); ab = 2; }
            cpu.pop(); h = 0;
          }
          for (; h < active; h++)
          {
            var p = (uint)(kk[h].a * 8); //p.x2 = p.f + y2 * p.a
            cpu.mul(y2, p + 3); cpu.add(p + 4); cpu.swp(p + 6); cpu.pop();
          }
          for (int i = 0, k = 0, dir = 0, t; i < active; i++)
          {
            var b = (uint)((t = kk[i].a) * 8); ref var v = ref pp[t]; var d = t != kk[i].b;
            if (cpu.equ(d ? b + 1 : (uint)(v.next * 8 + 1), y2)) v.next = -1 - v.next;
            var old = dir; dir += d ? +1 : -1;
            switch (this.Winding)
            {
              case Winding.EvenOdd:
                if ((old & 1) == 0 && (dir & 1) == 1) { k = i; continue; }
                if ((old & 1) == 1 && (dir & 1) == 0) break;
                goto skip;
              case Winding.Positive:
                if (dir == +1 && old == 0) { k = i; continue; }
                if (old == +1 && dir == 0) break;
                goto skip;
              case Winding.Negative:
                if (dir == -1 && old == 0) { k = i; continue; }
                if (old == -1 && dir == 0) break;
                goto skip;
              case Winding.NonZero:
                if (old == 0) { k = i; continue; }
                if (dir == 0) break;
                goto skip;
              case Winding.AbsGeqTwo:
                if (Math.Abs(old) == 1 && Math.Abs(dir) == 2) { k = i; continue; }
                if (Math.Abs(old) == 2 && Math.Abs(dir) == 1) break;
                goto skip;
              case Winding.AbsGeqThree:
                if (old == 2 && dir == 3) { k = i; continue; }
                if (old == 3 && dir == 2) break;
                goto skip;
            }
            var a = (uint)(kk[k].a * 8); ref var u = ref pp[kk[k].a];
            if (cpu.equ(a + 5, b + 5) && cpu.equ(a + 6, b + 6))
            {
              if (u.line != -1) { pp[nfl++].fl = (kk[k].a) | (u.line << 16); u.line = -1; }
              if (v.line != -1) goto skip;
              continue;
            }
            if (nc != 0)
            {
              var ic = pp[nc - 1].ic; var c = (uint)(ic * 8); ref var w = ref pp[ic];
              if (cpu.equ(c + 5, a + 5) && cpu.equ(c + 6, a + 6)) //xor
              {
                if (u.line != -1) { pp[nfl++].fl = (kk[k].a) | (u.line << 16); u.line = -1; }
                if (w.line != -1) { pp[nfl++].fl = pp[nc - 1].ic | (w.line << 16); w.line = -1; }
                nc--; goto m1;
              }
            }
            pp[nc++].ic = kk[k].a; m1:
            pp[nc++].ic = t;
            continue; skip: if (v.line != -1) { pp[nfl++].fl = t | (v.line << 16); v.line = -1; }
          }
          for (int i = 0, j; i < nc; i++)
          {
            if (this.np + 4 >= pp.Length) resize(0);
            var b = (uint)((j = pp[i].ic) * 8); ref var v = ref pp[j];
            bool f1 = false, f2 = false;
            for (int k = i - 1; k <= i + 1; k += 2)
            {
              if ((uint)k >= (uint)nc) continue;
              var c = (uint)(pp[k].ic * 8);
              if (!f1 && cpu.equ(b + 5, c + 5)) f1 = true;
              if (!f2 && cpu.equ(b + 6, c + 6)) f2 = true;
            }
            if (!f1)
            {
              if (v.line == -1)
                for (int t = 0; t < nfl; t++)
                {
                  var c = (uint)((pp[t].fl & 0xffff) * 8);
                  if (!cpu.equ(b + 4, c + 4) || !cpu.equ(b + 3, c + 3)) continue;
                  v.line = pp[t].fl >> 16; pp[t].fl = pp[i].ic | (v.line << 16);
                  for (nfl--; t < nfl; t++) pp[t].fl = pp[t + 1].fl; break;
                }
              if (v.line != -1)
              {
                if ((i & 1) != 0)
                {
                  if (ii[v.line].b == -1) { if (f2) { ii[v.line].b = add(b + 6, y2, -1 - j); v.line = -1; } continue; }
                  ii[v.line].a = add(b + 5, y1, -1 - j);
                }
                else
                {
                  if (ii[v.line].a == -1) { if (f2) { ii[v.line].a = add(b + 6, y2, -1 - j); v.line = -1; } continue; }
                  ii[v.line].b = add(b + 5, y1, -1 - j);
                }
              }
            }
            var k1 = add(b + 5, y1, -1 - j);// b = ref pp[j];
            if (f1 && v.line != -1) { if (ii[v.line].a == -1) ii[v.line].a = k1; else ii[v.line].b = k1; }
            var k2 = f2 ? add(b + 6, y2, -1 - j) : -1;
            v.line = f2 ? -1 : this.ni; if (this.ni == ii.Length) Array.Resize(ref ii, this.ni << 1);
            ii[this.ni++] = (i & 1) != 0 ? (k1, k2) : (k2, k1);
          }
          for (int i = 0, j; i < nfl; i++)
          {
            var c = (uint)((j = pp[i].fl & 0xffff) * 8);
            var line = pp[i].fl >> 16; var t = add(c + 5, y1, -1 - j);
            if (ii[line].a == -1) ii[line].a = t; else ii[line].b = t;
          }
          nfl = 0;
          ////////////
          if (cpu.equ(y2, y3)) break;
        }
      }
      for (int i = 0, j; i < active; i++)
      {
        var c = (uint)((j = kk[i].a) * 8); ref var w = ref pp[j]; if (w.line == -1) continue;
        var t = add(c + 6, y2, -1 - j); if (ii[w.line].a == -1) ii[w.line].a = t; else ii[w.line].b = t;
      }
      if ((Options & Option.Fill) != 0) fill();
      if (shv != 0)
      {
        // var f = shv == 1 ? kill : kill / shv;
        var t = cpu.mark(); cpu.push(-123); if (shv != 1) { cpu.push(shv); cpu.div(); }
        for (int i = 0; i < this.np; i++)
        {
          var p = (uint)(i * 8); //p.y = p.y - p.x / f; 
          if (i >= ab && i < np) { cpu.swp(p + 1, p + 7); continue; }
          cpu.div(p + 0, t); cpu.add(p + 1); cpu.norm();
          cpu.swp(p + 1); cpu.pop();
        }
        cpu.pop();
      }
      if ((Options & (Option.Outline | Option.OutlinePrecise)) != 0) outline();
      if ((Options & (Option.Fill | Option.Delaunay)) == (Option.Fill | Option.Delaunay)) optimize();
      if (pro != 0) project(pro << 2);
      if ((Options & Option.Trim) != 0) trim(); state = 3;
    }
    #region output  
    public ReadOnlyArray<Vector3R> Vertices
    {
      get
      {
        if (state < 3) return default;
        if ((state & 0x20) == 0)
        {
          state |= 0x20;
          if (rp == null || rp.Length < np) rp = new Vector3R[np]; //ensure(ref rp, Math.Max(4, this.np));
          for (int i = 0; i < np; i++)
          {
            cpu.get((uint)(i * 8 + 0), out rat x);
            cpu.get((uint)(i * 8 + 1), out rat y);
            cpu.get((uint)(i * 8 + 2), out rat z);
            rp[i] = new Vector3R(x, y, z);
          }
        }
        return new ReadOnlyArray<Vector3R>(rp, this.np);
      }
    }
    public ReadOnlyArray<Vector3> VerticesVector3
    {
      get
      {
        if (state < 3) return default;
        if ((state & 0x10) == 0)
        {
          state |= 0x10; //Debug.Assert(pp.Length >= np);
          if (fp == null || fp.Length < np) fp = new Vector3[np];
          for (int i = 0; i < np; i++)
          {
            cpu.get((uint)(i * 8 + 0), out float x);
            cpu.get((uint)(i * 8 + 1), out float y);
            cpu.get((uint)(i * 8 + 2), out float z);
            fp[i] = new Vector3(x, y, z);
          }
        }
        return new ReadOnlyArray<Vector3>(fp, this.np);
      }
    }
    public ReadOnlyArray<int> Indices
    {
      get => new ReadOnlyArray<int>(ss, state >= 3 ? ns : 0);
    }
    public ReadOnlyArray<int> Outline
    {
      get => new ReadOnlyArray<int>(ll, state >= 3 ? nl : 0);
    }
    public ReadOnlyArray<int> OutlineCounts
    {
      get => new ReadOnlyArray<int>(lc, state >= 3 ? nc : 0);
    }
    #endregion
    [Flags]
    public enum Option
    {
      Fill = 0x0100, Delaunay = 0x0200,
      Outline = 0x1000, OutlinePrecise = 0x2000, Trim = 0x8000,
      NormX = 0x10000, NormY = 0x20000, NormZ = 0x40000, NormNeg = 0x80000
    }
    const int hash = 199; //1103
    int state, ns, nl, nc, fi; int[]? ss, ll, lc;
    int np; (int next, int ic, int line, int fl)[] pp;
    int mi; int[] dict;
    int ni; (int a, int b)[] ii, kk;
    Vector3R[]? rp; Vector3[]? fp;
    readonly rat.CPU cpu;
    readonly Comparison<(int a, int b)> cmpy, cmpx, cmpya;
    readonly Comparison<int> cmpab;
    public TesselatorR()
    {
      const int n = 32;
      cpu = new rat.CPU(n << 3);
      pp = new (int, int, int, int)[n];
      kk = new (int, int)[n << 1];
      ss = new int[n << 1];
      ii = new (int, int)[n];
      dict = new int[hash + n];
      cmpy = (a, b) =>
      {
        if (a.b == b.b) return 0;
        var t = unchecked(cpu.cmp((uint)(a.b * 8 + 1), (uint)(b.b * 8 + 1)));
        if (t == 0)
        {
          var x = unchecked(cpu.cmp((uint)(a.b * 8 + 0), (uint)(b.b * 8 + 0)));
          if (x != 0) this.ni = -1; //trim
        }
        return t;
      };
      cmpx = (a, b) =>
      {
        int t = unchecked(cpu.cmp((uint)(a.a * 8 + 5), (uint)(b.a * 8 + 5))); //x1
        return t != 0 ? t : unchecked(cpu.cmp((uint)(a.a * 8 + 6), (uint)(b.a * 8 + 6))); //x2
      };
      cmpya = (a, b) =>
      {
        var i = unchecked(cpu.cmp((uint)(a.a * 8 + 1), (uint)(b.a * 8 + 1)));
        if (i == 0) i = a.a.CompareTo(b.a); return i;
      };
      cmpab = (a, b) => unchecked(cpu.cmp(
        (uint)((pp[a].fl < 0 ? ii[a].a : ii[a].b) * 8 + 1),
        (uint)((pp[b].fl < 0 ? ii[b].a : ii[b].b) * 8 + 1)));
    }
    void enter()
    {
      cpu.push(0, 5); if (state != 2) { state = 0; throw new Exception(); }
      //if (shv == 0 && np != 0) { var t = cpu.mark(); if (cpu.equ(t - 7, t - 15)) shv = 1; }
      if (np == pp.Length) resize(0);
      if (np != fi) pp[np - 1].next = np; pp[np++].next = fi;
    }
    void outline()
    {
      if (ll == null || ni > ll.Length) Array.Resize(ref ll, ii.Length);
      if (np + (ni << 1) > dict.Length) Array.Resize(ref dict, Math.Max(pp.Length, np) + (ii.Length << 1));
      cdict(np);
      for (int i = 0, k, j = np; i < ni; i++)
      {
        dict[j] = i; dict[j + 1] = dict[k = ii[i].a]; dict[k] = j; j += 2;
      }
      for (int i = 0, t, l = 0; i < np; i++)
      {
        if ((t = dict[i]) == 0) continue;
        for (var ab = nl; ;)
        {
          var u = ii[dict[t]]; //for (var j = t; j != 0; j = dict[j + 1]) { var xx = ii[dict[j]]; }
          if ((Options & Option.OutlinePrecise) != 0)
          {
            if (dict[t + 1] != 0) //branches
            {
              pp[u.a].next = 1 << 20;
              if (nl != ab)
              {
                for (var j = dict[t + 1]; j != 0; j = dict[j + 1])
                {
                  var v = ii[dict[j]];
                  var d = ccw(ll[nl - 1], u.a, u.b) -
                          ccw(ll[nl - 1], u.a, v.b);
                  if (d > 0) continue;
                  if (d == 0 && ccw(u.a, u.b, v.b) <= 0) continue;
                  var q = dict[t]; dict[t] = dict[j]; dict[j] = q; u = v;
                }
              }
            }
            if (nl != ab)
            {
              if (pp[u.a].next == (1 << 20) && ccw(ll[nl - 1], u.a, u.b) == 0) goto skip;
              if (u.b == i && pp[ll[ab]].next == (1 << 20) && ccw(u.a, ll[ab], ll[ab + 1]) == 0)
              {
                nl--; for (int j = ab; j < nl; j++) ll[j] = ll[j + 1];
              }
            }
          }
          ll[nl++] = u.a; skip: dict[u.a] = dict[t + 1];
          if (u.b != i) { t = dict[u.b]; continue; }
          if (lc == null || lc.Length == nc) Array.Resize(ref lc, Math.Max(4, nc << 1));
          lc[nc++] = nl - l; l = nl; i--; break;
        }
      }
    }
    void fill()
    {
      sort(cmpya, ii, ni);
      if (ni > pp.Length) resize(ii.Length);
      for (int i = 0; i < ni; i++)
      {
        pp[i].ic = 0;
        pp[i].fl = cpu.cmp((uint)(ii[i].a * 8 + 1), (uint)(ii[i].b * 8 + 1));
      }
      if (ni > ss.Length) ss = new int[ii.Length];
      for (int i = 0; i < ni; i++) ss[i] = i;
      sort(cmpab, ss, ni); // y-max, create trapezoidal map on dict in O(n)
      cdict(mi = ni); (int k, int d1, int d2) l1 = default, l2 = default;
      for (int i = 0, lp = -1, active = ni; i < ni; i++)
      {
        var ip = ii[i].a; if (ip == lp) continue; lp = ip;
        var pt = (uint)(ip * 8); l1.k = l2.k = -1;
        for (int j = 0, k; j < active; j++)
        {
          var p = ii[k = ss[j]]; if (p.a == ip || p.b == ip) continue;
          var x = ((int)cpu.mark() >> 3) - 1;
          if (k > x) cpu.push(0, (k - x) * 8);
          var c = (uint)(k * 8); ref var w = ref pp[k];
          var a = (uint)(p.a * 8); var b = (uint)(p.b * 8);
          //var y1 = w.fl < 0 ? a.y : b.y; if (cpu.cmp(y1, pt.y) >= 0) break;
          //var y2 = w.fl < 0 ? b.y : a.y; if (cpu.cmp(y2, pt.y) <= 0) { active--; for (var t = j--; t < active; t++) ss[t] = ss[t + 1]; continue; }
          if (cpu.cmp(w.fl < 0 ? a + 1 : b + 1, pt + 1) >= 0) break;
          if (cpu.cmp(w.fl < 0 ? b + 1 : a + 1, pt + 1) <= 0) { active--; for (var t = j--; t < active; t++) ss[t] = ss[t + 1]; continue; }
          if (w.ic == 0)
          {
            w.ic = 1;
            //c.a = (b.x - a.x) / (b.y - a.y);
            //c.f = a.x - a.y * c.a;
            cpu.sub(b + 0, a + 0); cpu.sub(b + 1, a + 1);
            cpu.div(); cpu.norm(); cpu.swp(c + 3); cpu.pop();
            cpu.mul(a + 1, c + 3); cpu.neg();
            cpu.add(a + 0); cpu.norm(); cpu.swp(c + 4); cpu.pop();
          }
          //var x = 0 | c.f + pt.y * c.a;
          //var s = cpu.cmp(pt.x, x);
          //if (s < 0 && (l1.k == -1 || cpu.cmp(x, l1.x) < 0)) l1 = (k, x, c.fl, s);
          //if (s > 0 && (l2.k == -1 || cpu.cmp(x, l2.x) > 0)) l2 = (k, x, c.fl, s);
          cpu.mul(pt + 1, c + 3);
          cpu.add(c + 4); var s = -cpu.cmp(pt + 0); //tmp x1, x2
          if (s < 0 && (l1.k == -1 || cpu.cmp(5u) < 0)) { cpu.swp(5u); l1 = (k, w.fl, s); } //else
          if (s > 0 && (l2.k == -1 || cpu.cmp(6u) > 0)) { cpu.swp(6u); l2 = (k, w.fl, s); }
          cpu.pop();
        }
        for (int l = 0; l < 2; l++)
        {
          var p = l == 1 ? l2 : l1; if (p.k == -1 || p.d1 * p.d2 != 1) continue;
          if (mi + 2 > dict.Length) Array.Resize(ref dict, dict.Length << 1);
          var h = dict[p.k]; var n = 0;
          if (p.d1 > 0) { n = h; h = mi; }
          else { if (h != 0) dict[(h >> 16) + 1] = mi; else h = mi; h = (h & 0xffff) | (mi << 16); }
          dict[p.k] = h; dict[mi++] = ip; dict[mi++] = n;
        }
      }
      //tess monotones on ss in O(n): 
      for (int i = 0, t; i < ni; i++)
        for (; (t = dict[i] & 0xffff) != 0;)
          for (int t1 = ii[i].a, t2, t3, n, l = -1;/* t != 0*/; t1 = t2, l = t, t = n)
          {
            t2 = dict[t]; t3 = (n = dict[t + 1]) != 0 ? dict[n] : ii[i].b;
            if (ccw(t1, t2, t3) != -1) continue; //possible: fans after sequence of left turns  
            if (ns + 3 >= ss.Length) Array.Resize(ref ss, ss.Length << 1);
            ss[ns++] = t1; ss[ns++] = t3; ss[ns++] = t2;
            dict[l == -1 ? i : l + 1] = n; break;
          }
    }
    int project(int m)
    {
      if (m == 0 && (m = (((int)Options >> 15) & 0x12) | (((int)Options >> 17) & 1)) == 0) return m;
      for (uint i = 0; i < np; i++)
      {
        var k = i * 8;
        if ((m & 0x40) != 0) cpu.neg(k + 1); // p.y = -p.y;
        switch (m & 0xf)
        {
          case 1: case 8: cpu.swp(k + 0, k + 1); cpu.swp(k + 0, k + 2); break; // new vector3(p.z, p.x, p.y);
          case 2: case 4: cpu.swp(k + 0, k + 1); cpu.swp(k + 1, k + 2); break; // new vector3(p.y, p.z, p.x);
        }
        if ((m & 0x10) != 0) cpu.neg(k + 1); // p.y = -p.y;
      }
      return m;
    }
    void resize(int c)
    {
      Array.Resize(ref pp, Math.Max(c, pp.Length << 1));
      Array.Resize(ref kk, Math.Max(kk.Length, pp.Length << 1));
      Array.Resize(ref dict, Math.Max(dict.Length, hash + pp.Length));
    }
    void cdict(int n)
    {
      for (int i = 0; i < n; i++) dict[i] = 0;
    }
    int add(uint x, uint y, int v)
    {
      int h = unchecked((int)((cpu.hash(x) + cpu.hash(y) * 13) % hash)), i = dict[h] - 1;
      for (uint t; i != -1; i = dict[hash + i] - 1)
        if (cpu.equ(t = (uint)(i * 8), x) && cpu.equ(t + 1, y)) return i;
      if ((i = v) < 0)
      {
        if (np == pp.Length) resize(0); //todo: check necessary? possible?
        var a = (uint)((-v - 1) * 8); var n = pp[-v - 1].next;
        var b = (uint)((n < 0 ? -n - 1 : n) * 8);
        //pp[i = np++].pt = new vector3(x, y, a.z + (y - a.y) * (b.z - a.z) / (b.y - a.y));
        var k = (uint)((i = np++) * 8); Debug.Assert(k == cpu.mark());
        cpu.dup(x);
        cpu.dup(y);
        if (cpu.equ(a + 2, b + 2)) cpu.dup(a + 2); //c.z = a.z;
        else
        {
          //c.z = a.z + (y - a.y) * (b.z - a.z) / (b.y - a.y);
          cpu.sub(b + 2, a + 2);
          cpu.sub(b + 1, a + 1); cpu.div();
          cpu.sub(k + 1, a + 1); cpu.mul();
          cpu.add(a + 2); cpu.norm(); //norm ?
        }
        cpu.push(0, 5);
      }
      dict[hash + i] = dict[h]; dict[h] = i + 1; return i;
    }
    int ccw(int a, int b, int c)
    {
      var u = (uint)(a * 8); var v = (uint)(b * 8); var w = (uint)(c * 8);
      //return rat.Sign((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x));
      cpu.sub(v + 0, u + 0); cpu.sub(v + 1, u + 1);
      cpu.sub(w + 0, u + 0); cpu.sub(w + 1, u + 1);
      var s = cpu.sign(3) * cpu.sign(0) - cpu.sign(2) * cpu.sign(1);
      if (s != 0) { cpu.pop(4); return Math.Sign(s); }
      cpu.mul(3, 0); cpu.mul(2, 1); s = cpu.cmp(3, 2); cpu.pop(4);
      return s;
    }
    unsafe void optimize()
    {
      static int mod(int i, int k) { var r = i % 3; return i - r + (r + k) % 3; }
      if (ns > ii.Length) Array.Resize(ref ii, ss.Length);
      if (hash + ns > dict.Length) dict = new int[hash + ss.Length];
      if (ns > kk.Length) kk = new (int, int)[ss.Length]; //kk[].a adjancies
      for (int i = 0; i < ns; i++) kk[i] = (-1, 0); cdict(hash);
      for (int i = 0; i < ns; i++)
      {
        var l = (a: ss[i], b: ss[mod(i, 1)]);
        int h = unchecked((int)((uint)l.GetHashCode() % hash)), t = dict[h] - 1;
        for (; t != -1 && (ii[t].a != l.a || ii[t].b != l.b); t = dict[hash + t] - 1) ;
        if (t != -1) { kk[kk[i].a = t].a = i; continue; }
        h = unchecked((int)((uint)(ii[i] = (l.b, l.a)).GetHashCode() % hash));
        dict[hash + i] = dict[h]; dict[h] = i + 1;
      }
      Debug.Assert(pp.Length >= np);
      fixed (void* tp = pp)
      {
        var dd = (Vector2*)tp;
        for (int i = 0; i < np; i++)
        {
          cpu.get((uint)(i * 8 + 0), out dd[i].X);
          cpu.get((uint)(i * 8 + 1), out dd[i].Y);
        }
        for (int i = 0, s = 0, t; i < ns; i++)
        {
          if (kk[i].b == 1) continue; var k = kk[i].a; if (k == -1) continue;
          int u1, u2, v2, i1 = ss[i], i2 = ss[u1 = mod(i, 1)], i3 = ss[u2 = mod(i, 2)], k3 = ss[v2 = mod(k, 2)];
          if (circum(dd, i1, i2, i3, k3)) { kk[i].b = kk[k].b = 1; continue; } //ok
          ss[i] = k3; ss[k] = i3; int j = i, v1 = mod(k, 1);
          if ((t = kk[u1].a) != -1) { kk[t].b = 0; if (t < j) j = t; }
          if ((t = kk[v1].a) != -1) { kk[t].b = 0; if (t < j) j = t; }
          if ((t = kk[i].a = kk[v2].a) != -1) { kk[t].a = i; kk[t].b = 0; if (t < j) j = t; }
          if ((t = kk[k].a = kk[u2].a) != -1) { kk[t].a = k; kk[t].b = 0; if (t < j) j = t; }
          kk[kk[u2].a = v2].a = u2; if (j < i) i = j - 1; if (s++ == ns << 2) break; //reps
        }
        static bool circum(Vector2* dd, int i1, int i2, int i3, int i4)
        {
          var a = dd[i1]; var b = dd[i2];
          var c = dd[i3]; var d = dd[i4];
          var ab = (a + b) * 0.5f;
          var bc = (b + c) * 0.5f;
          var vc = ab - bc;
          var va = a - b; va = new Vector2(va.Y, -va.X); // new Vector2(a.Y - b.Y, b.X - a.X);
          var vb = b - c; vb = new Vector2(vb.Y, -vb.X); // new Vector2(b.Y - c.Y, c.X - b.X);
          var f = (vb.X * vc.Y - vb.Y * vc.X) / (va.X * vb.Y - va.Y * vb.X);
          var p = ab + va * f;
          var r = Vector2.Dot(a = p - a, a);
          var l = Vector2.Dot(d = p - d, d);
          return l >= r;
        }
      }
    }
    void trim()
    {
      cdict(this.np); var np = 0;
      for (int i = 0; i < ns; i++) dict[ss[i]] = 1;
      for (int i = 0; i < nl; i++) dict[ll[i]] = 1;
      for (int i = 0; i < this.np; i++)
      {
        if (dict[i] == 0) continue;
        if (np != i)
        {
          var a = (uint)(i * 8); var b = (uint)(np * 8);
          cpu.swp(a + 0, b + 0);
          cpu.swp(a + 1, b + 1);
          cpu.swp(a + 2, b + 2);
        }
        dict[i] = np++;
      }
      if (this.np == np) return; this.np = np;
      for (int i = 0; i < ns; i++) ss[i] = dict[ss[i]];
      for (int i = 0; i < nl; i++) ll[i] = dict[ll[i]];
    }
    static unsafe void sort<T>(Comparison<T> c, T[] a, int n) where T : unmanaged
    {
      if (n < 2) return;
      fixed (T* p = a) intro(c, p, n, (BitOperations.Log2(unchecked((uint)n)) + 1) << 1);
      static void intro(Comparison<T> c, T* p, int n, int lim)
      {
        for (int l = n; l > 1;)
        {
          if (l <= 16)
          {
            if (l == 2) { cwp(c, p, 0, 1); return; }
            if (l == 3) { cwp(c, p, 0, 1); cwp(c, p, 0, 2); cwp(c, p, 1, 2); return; }
            isort(c, p, n); return;
          }
          if (lim == 0) { hsort(c, p, n); return; }
          var x = part(c, p, l); intro(c, p + (x + 1), l - (x + 1), --lim); l = x;
        }
      }
      static int part(Comparison<T> c, T* p, int n)
      {
        int h = n - 1, m = h >> 1;
        cwp(c, p, 0, m); cwp(c, p, 0, h); cwp(c, p, m, h);
        var x = p[m]; swp(p, m, h - 1);
        int l = 0, r = h - 1;
        for (; l < r;)
        {
          while (c(p[++l], x) < 0) ;
          while (c(x, p[--r]) < 0) ;
          if (l >= r) break; swp(p, l, r);
        }
        if (l != h - 1) swp(p, l, h - 1);
        return l;
      }
      static void isort(Comparison<T> c, T* p, int n)
      {
        for (int i = 0, j; i < n - 1; i++)
        {
          var t = p[i + 1];
          for (j = i; j >= 0 && c(t, p[j]) < 0; p[j + 1] = p[j], j--) ;
          p[j + 1] = t;
        }
      }
      static void hsort(Comparison<T> cmp, T* p, int n)
      {
        for (int i = n >> 1; i >= 1; i--) down(cmp, p, i, n);
        for (int i = n; i > 1; i--) { swp(p, 0, i - 1); down(cmp, p, 1, i - 1); }
        static void down(Comparison<T> c, T* p, int i, int n)
        {
          var d = p[i - 1];
          while (i <= n >> 1)
          {
            int t = i << 1;
            if (t < n && c(p[t - 1], p[t]) < 0) t++;
            if (!(c(d, p[t - 1]) < 0)) break;
            p[i - 1] = p[t - 1]; i = t;
          }
          p[i - 1] = d;
        }
      }
      static void cwp(Comparison<T> c, T* p, int i, int j)
      {
        if (c(p[i], p[j]) > 0) { var t = p[i]; p[i] = p[j]; p[j] = t; }
      }
      static void swp(T* p, int i, int j)
      {
        var t = p[i]; p[i] = p[j]; p[j] = t;
      }
    }

    [DebuggerTypeProxy(typeof(DebugView<>)), DebuggerDisplay("{typeof(T).Name,nq}[{n}]")]
    public struct ReadOnlyArray<T> : IEnumerable<T>
    {
      T[]? a; int n;
      public ReadOnlyArray(T[]? a, int n)
      {
        this.a = a; this.n = n;
      }
      public int Length => n;
      public T this[int i] => a[i];
      public ReadOnlySpan<T> AsSpan() => a.AsSpan().Slice(0, n);
      public IEnumerator<T> GetEnumerator()
      {
        return (a != null ? a.Take(n) : Enumerable.Empty<T>()).GetEnumerator();
      }
      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
    }
    sealed class DebugView<T>
    {
      IEnumerable<T> p;
      public DebugView(IEnumerable<T> p) => this.p = p;
      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public T[] Items => p.ToArray();
    }
  }

}
