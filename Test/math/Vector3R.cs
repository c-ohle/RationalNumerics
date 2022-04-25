
namespace System.Numerics.Rational
{
  /// <summary>
  /// A Vector3 class based on <see cref="rat"/>.<br/>
  /// <i>This is just a non-optimal example implementation for testing!</i>
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Vector3R : IEquatable<Vector3R>, IFormattable, ISpanFormattable
  {
    public readonly rat X, Y, Z;
    public Vector3R(rat x, rat y, rat z)
    {
      X = x; Y = y; Z = z;
    }
    public override string ToString()
    {
      return $"{X}; {Y}; {Z}";
    }
    public readonly string ToString(string? format, IFormatProvider? provider = null)
    {
      return $"{X.ToString(format, provider)}; {Y.ToString(format, provider)}; {Z.ToString(format, provider)}";
    }
    public readonly bool TryFormat(Span<char> sw, out int nw, ReadOnlySpan<char> fmt, IFormatProvider? fp)
    {
      int n; nw = 0;
      X.TryFormat(sw.Slice(nw), out n, fmt, fp); nw += n; sw[nw++] = ' ';
      Y.TryFormat(sw.Slice(nw), out n, fmt, fp); nw += n; sw[nw++] = ' ';
      Z.TryFormat(sw.Slice(nw), out n, fmt, fp); nw += n;
      return true;
    }
    public static Vector3R Parse(ref ReadOnlySpan<char> s)
    {
      return new Vector3R(rat.Parse(s.token()), rat.Parse(s.token()), rat.Parse(s.token()));
    }
    public readonly void WriteToBytes(ref Span<byte> ws)
    {
      X.WriteToBytes(ref ws); X.WriteToBytes(ref ws); Z.WriteToBytes(ref ws);
    }
    public static Vector3R ReadFromBytes(ref ReadOnlySpan<byte> rs)
    {
      return new Vector3R(rat.ReadFromBytes(ref rs), rat.ReadFromBytes(ref rs), rat.ReadFromBytes(ref rs));
    }
    public override int GetHashCode()
    {
      return HashCode.Combine(X, Y, Z);
    }
    public override bool Equals(object? p)
    {
      return p is Vector3R b && Equals(b);
    }
    public readonly bool Equals(Vector3R b)
    {
      return X.Equals(b.X) && Y.Equals(b.Y) && Z.Equals(b.Z);
    }
    public static explicit operator Vector3(in Vector3R v)
    {
      return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
    }
    public static implicit operator Vector3R(Vector3 v)
    {
      return new Vector3R(v.X, v.Y, v.Z);
    }
    public static bool operator ==(Vector3R a, Vector3R b)
    {
      return a.Equals(b);
    }
    public static bool operator !=(Vector3R a, Vector3R b)
    {
      return !a.Equals(b);
    }
    public static Vector3R operator +(in Vector3R a) => a;
    public static Vector3R operator -(in Vector3R a) => new Vector3R(-a.X, -a.Y, -a.Z);
    public static Vector3R operator +(in Vector3R a, in Vector3R b)
    {
      return new Vector3R(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    public static Vector3R operator -(in Vector3R a, in Vector3R b)
    {
      return new Vector3R(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    public static Vector3R operator *(in Vector3R a, rat b)
    {
      return new Vector3R(a.X * b, a.Y * b, a.Z * b);
    }
    public static Vector3R operator *(in Vector3R a, in Vector3R b)
    {
      return new Vector3R(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }
    public static Vector3R operator ^(in Vector3R a, in Vector3R b) => Cross(a, b);
    public static rat LengthSq(in Vector3R a)
    {
      var cpu = rat.task_cpu; cpu.dot(a, a); return cpu.pop_rat();
    }
    public static double Length(in Vector3R a)
    {
      var cpu = rat.task_cpu; cpu.dot(a, a); return Math.Sqrt(cpu.pop_dbl());
    }
    public static rat Dot(in Vector3R a, in Vector3R b)
    {
      //return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
      var cpu = rat.task_cpu; cpu.dot(a, b); return cpu.pop_rat();
    }
    public static Vector3R Cross(in Vector3R a, in Vector3R b)
    {
      //return new VectorR3(
      //  a.Y * b.Z - a.Z * b.Y,
      //  a.Z * b.X - a.X * b.Z,
      //  a.X * b.Y - a.Y * b.X);
      var cpu = rat.task_cpu;
      cpu.mul(a.X, b.Y); cpu.mul(a.Y, b.X); cpu.sub();
      cpu.mul(a.Z, b.X); cpu.mul(a.X, b.Z); cpu.sub();
      cpu.mul(a.Y, b.Z); cpu.mul(a.Z, b.Y); cpu.sub();
      return new Vector3R(cpu.pop_rat(), cpu.pop_rat(), cpu.pop_rat());
    }
    public static Vector3R Normalize(in Vector3R v)
    {
      //int i = LongAxis(v); var l = i == 0 ? v.X : i == 1 ? v.Y : v.Z;
      //var s = NewRational.Sign(l); if (s < 0) l = -l;
      //return new Vector3R(i == 0 ? s : v.X / l, i == 1 ? s : v.Y / l, i == 2 ? s : v.Z / l);      
      var cpu = rat.task_cpu; cpu.push(v.Z); cpu.push(v.Y); cpu.push(v.X);
      cpu.norm3(); return new Vector3R(cpu.pop_rat(), cpu.pop_rat(), cpu.pop_rat());
    }
    public static int LongAxis(in Vector3R a)
    {
      //var l = rat.Abs(a.X) > rat.Abs(a.Y) ? 0 : 1;
      //if (rat.Abs(a.Z) > rat.Abs(l == 0 ? a.X : a.Y)) l = 2; return l;
      var cpu = rat.task_cpu;
      cpu.push(a.Z); cpu.push(a.Y); cpu.push(a.X);
      int l; if (cpu.cmpa(2, l = cpu.cmpa(1, 0) > 0 ? 1 : 0) > 0) l = 2;
      cpu.pop(3); return l;
    }
    public static Vector3R Min(in Vector3R a, in Vector3R b)
    {
      return new Vector3R(
        rat.Min(a.X, b.X),
        rat.Min(a.Y, b.Y),
        rat.Min(a.Z, b.Z));
    }
    public static Vector3R Max(in Vector3R a, in Vector3R b)
    {
      return new Vector3R(
        rat.Max(a.X, b.X),
        rat.Max(a.Y, b.Y),
        rat.Max(a.Z, b.Z));
    }
    public static Vector3R Transform(in Vector3R a, in Matrix4x3R b)
    {
      //return new Vector3R(
      //  a.X * b.M11 + a.Y * b.M21 + a.Z * b.M31 + b.M41,
      //  a.X * b.M12 + a.Y * b.M22 + a.Z * b.M32 + b.M42,
      //  a.X * b.M13 + a.Y * b.M23 + a.Z * b.M33 + b.M43);
      var cpu = rat.task_cpu;
      cpu.dot(a.X, b.M13, a.Y, b.M23, a.Z, b.M33, b.M43);
      cpu.dot(a.X, b.M12, a.Y, b.M22, a.Z, b.M32, b.M42);
      cpu.dot(a.X, b.M11, a.Y, b.M21, a.Z, b.M31, b.M41);
      return new Vector3R(cpu.pop_rat(), cpu.pop_rat(), cpu.pop_rat());
    }
    public static int PtInline(in Vector3R a, in Vector3R b, in Vector3R c)
    {
      //var u = b - a; var v = c - a;
      //var t = Cross(u, v); if (t != default) return 0;
      //var f = u.X != 0 ? v.X / u.X : u.Y != 0 ? v.Y / u.Y : v.Z / u.Z;
      //var s = f.CompareTo(0); if (s <= 0) return s + 2; //1, 2
      //s = f.CompareTo(1); return s + 4;      
      var cpu = rat.task_cpu; cpu.cross(a, b, c);
      var s = cpu.sign(0) | cpu.sign(1) | cpu.sign(2);
      if (s != 0) { cpu.pop(9); return 0; }
      cpu.pop(3); var l = cpu.sign(0) != 0 ? 0 : cpu.sign(1) != 0 ? 1 : 2;
      cpu.div(l, l + 3);
      cpu.push(0); s = cpu.cmp(l + 1, 0); if (s <= 0) { cpu.pop(7); return s + 2; } // 1, 2
      cpu.push(1); s = cpu.cmp(l + 2, 0); cpu.pop(8); return s + 4; // 3, 4, 5    
    }
  }
}
