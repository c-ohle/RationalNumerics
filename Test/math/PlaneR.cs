﻿
namespace System.Numerics.Rational
{
  /// <summary>
  /// A Plane class based on NewRational.<br/>
  /// <i>This is just a non optimal example implementation!</i>/>
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct PlaneR : IEquatable<PlaneR>, IFormattable, ISpanFormattable
  {
    public readonly Vector3R Normal; public readonly rat Dist;
    public PlaneR(rat x, rat y, rat z, rat d)
    {
      Normal = new Vector3R(x, y, z); Dist = d;
    }
    public PlaneR(Vector3R n, rat d)
    {
      Normal = n; Dist = d;
    }
    public override string ToString()
    {
      return $"{Normal}; {Dist}";
    }
    public readonly string ToString(string? format, IFormatProvider? provider = null)
    {
      return $"{Normal.ToString(format, provider)}; {Dist.ToString(format, provider)}";
    }
    public bool TryFormat(Span<char> sw, out int nw, ReadOnlySpan<char> fmt, IFormatProvider? fp)
    {
      int n; nw = 0;
      Normal.TryFormat(sw.Slice(nw), out n, fmt, fp); nw += n; sw[nw++] = ' ';
      Dist.TryFormat(sw.Slice(nw), out n, fmt, fp); nw += n;
      return true;
    }
    public static PlaneR Parse(ref ReadOnlySpan<char> s)
    {
      return new PlaneR(
        rat.Parse(s.token()),
        rat.Parse(s.token()),
        rat.Parse(s.token()),
        rat.Parse(s.token()));
    }
    public readonly void WriteToBytes(ref Span<byte> ws)
    {
      Normal.WriteToBytes(ref ws); Dist.WriteToBytes(ref ws);
    }
    public static PlaneR ReadFromBytes(ref ReadOnlySpan<byte> rs)
    {
      return new PlaneR(Vector3R.ReadFromBytes(ref rs), rat.ReadFromBytes(ref rs));
    }
    public override int GetHashCode()
    {
      return Normal.GetHashCode() ^ Dist.GetHashCode();
    }
    public override bool Equals(object? p)
    {
      return p is PlaneR b && Equals(b);
    }
    public readonly bool Equals(PlaneR b)
    {
      return Dist.Equals(b.Dist) && Normal.Equals(b.Normal);
    }
    public static explicit operator Plane(in PlaneR v)
    {
      return new Plane((Vector3)v.Normal, (float)v.Dist);
    }
    public static implicit operator PlaneR(Plane v)
    {
      return new PlaneR(v.Normal, v.D);
    }
    public static PlaneR operator -(in PlaneR a) => new PlaneR(-a.Normal, -a.Dist);
    public static PlaneR FromVertices(in Vector3R a, in Vector3R b, in Vector3R c)
    {
      //var n = Vector3R.Cross(b - a, c - a);
      //n = Vector3R.Normalize(n);
      //var d = -Vector3R.Dot(n, a);
      //return new PlaneR(n, d);
      var cpu = rat.task_cpu;
      cpu.cross(a, b, c); // n = cross(b - a, c - a)
      cpu.norm3(); // n = norm(n)
      cpu.dot(a); cpu.neg(); // d = -dot(n, a)
      //fetch
      cpu.swp(0, 3); cpu.swp(1, 2);
      var p = new PlaneR(cpu.pop_rat(), cpu.pop_rat(), cpu.pop_rat(), cpu.pop_rat());
      cpu.pop(6); return p;
    }
    public static rat DotCoord(in PlaneR a, in Vector3R b)
    {
      //return Vector3R.Dot(a.N, b) + a.D;
      var cpu = rat.task_cpu; cpu.dot(a.Normal, b); cpu.add(a.Dist);
      return cpu.pop_rat();
    }
    public static int DotCoordSign(in PlaneR a, in Vector3R b)
    {
      //return Vector3R.Dot(a.N, b) + a.D;
      var cpu = rat.task_cpu; cpu.dot(a.Normal, b); cpu.add(a.Dist);
      var s = cpu.sign(); cpu.pop(); return s;
    }
    public static Vector3R Intersect(in PlaneR e, in Vector3R a, in Vector3R b)
    {
      //var u = Vector3R.Dot(e.Normal, a);
      //var v = Vector3R.Dot(e.Normal, b);
      //var w = (u + e.Dist) / (u - v);
      //return a + (b - a) * w;
      var cpu = rat.task_cpu; var m = cpu.mark();
      cpu.dot(e.Normal, a);
      cpu.dot(e.Normal, b);
      //w = (u + e.D) / (u - v)
      cpu.dup(1); cpu.add(e.Dist); cpu.sub(m + 0, m + 1); cpu.div();
      //a + (b - a) * w
      cpu.sub(b.Z, a.Z); cpu.mul(0, 1); cpu.add(a.Z);
      cpu.sub(b.Y, a.Y); cpu.mul(0, 2); cpu.add(a.Y);
      cpu.sub(b.X, a.X); cpu.mul(0, 3); cpu.add(a.X);
      var p = new Vector3R(cpu.pop_rat(), cpu.pop_rat(), cpu.pop_rat());
      cpu.pop(3); return p;
    }
    public static PlaneR Transform(in PlaneR a, in Matrix4x3R b)
    {
      //var m = ~b; rat x = a.Normal.X, y = a.Normal.Y, z = a.Normal.Z, w = a.Dist;
      //return new PlaneR(
      //  x * m.M11 + y * m.M12 + z * m.M13,
      //  x * m.M21 + y * m.M22 + z * m.M23,
      //  x * m.M31 + y * m.M32 + z * m.M33,
      //  x * m.M41 + y * m.M42 + z * m.M43 + w);
      var cpu = rat.task_cpu; var m = !b;
      cpu.dot(a.Normal.X, m.M41, a.Normal.Y, m.M42, a.Normal.Z, m.M43, a.Dist);
      cpu.dot(a.Normal.X, m.M31, a.Normal.Y, m.M32, a.Normal.Z, m.M33);
      cpu.dot(a.Normal.X, m.M21, a.Normal.Y, m.M22, a.Normal.Z, m.M23);
      cpu.dot(a.Normal.X, m.M11, a.Normal.Y, m.M12, a.Normal.Z, m.M13);
      return new PlaneR(cpu.pop_rat(), cpu.pop_rat(), cpu.pop_rat(), cpu.pop_rat());
    }
  }
}