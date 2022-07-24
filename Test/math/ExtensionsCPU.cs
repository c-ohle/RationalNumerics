
namespace System.Numerics.Rational
{
  /// <summary>
  /// Extensions for <see cref="BigRational.CPU"/>.
  /// </summary>
  static class ExtensionsCPU
  {
    /// <summary>
    /// a * b + c * d + e * f
    /// </summary>
    internal static void dot(this BigRational.SafeCPU cpu, BigRational a, BigRational b, BigRational c, BigRational d, BigRational e, BigRational f)
    {
      cpu.mul(a, b); cpu.mul(c, d); cpu.add(); cpu.mul(e, f); cpu.add();
    }
    /// <summary>
    /// a * b + c * d + e * f + g
    /// </summary>
    internal static void dot(this BigRational.SafeCPU cpu, BigRational a, BigRational b, BigRational c, BigRational d, BigRational e, BigRational f, BigRational g)
    {
      cpu.mul(a, b); cpu.mul(c, d); cpu.add(); cpu.mul(e, f); cpu.add(); cpu.add(g);
    }
    /// <summary>
    /// a.X * b.X + a.Y * b.Y 
    /// </summary>
    internal static void dot(this BigRational.SafeCPU cpu, in Vector2R a, in Vector2R b)
    {
      cpu.mul(a.X, b.X); cpu.mul(a.Y, b.Y); cpu.add();
    }
    /// <summary>
    /// a.X * b.X + a.Y * b.Y + a.Z * b.Z
    /// </summary>
    internal static void dot(this BigRational.SafeCPU cpu, in Vector3R a, in Vector3R b)
    {
      cpu.mul(a.X, b.X); cpu.mul(a.Y, b.Y); cpu.add(); cpu.mul(a.Z, b.Z); cpu.add();
    }
    /// <summary>
    /// a.X * b.X + a.Y * b.Y + a.Z * b.Z where a is on stack
    /// </summary>
    internal static void dot(this BigRational.SafeCPU cpu, in Vector3R b)
    {
      var m = cpu.mark() - 3;
      cpu.mul(b.X, m + 0);
      cpu.mul(b.Y, m + 1); cpu.add();
      cpu.mul(b.Z, m + 2); cpu.add();
    }
    /// <summary>
    /// box normalization, last 3 entries on stack
    /// </summary>
    internal static void norm3(this BigRational.SafeCPU cpu)
    {
      int l; if (cpu.cmpa(2, l = cpu.cmpa(1, 0) > 0 ? 1 : 0) > 0) l = 2; if (cpu.sign(l) == 0) return;
      cpu.dup(l); cpu.abs(); cpu.div(1 + (l + 1) % 3, 0); cpu.div(1 + (l + 2) % 3, 0); cpu.pop();
      cpu.push(cpu.sign(l)); cpu.swp(0, l + 1); cpu.pop();
    }
    /// <summary>
    /// a * b - c * d
    /// </summary>
    internal static void cross(this BigRational.SafeCPU cpu, BigRational a, BigRational b, BigRational c, BigRational d)
    {
      cpu.mul(a, b); cpu.mul(c, d); cpu.sub();
    }
    /// <summary>
    /// a * [b] - c * [d] + e * [f] 
    /// </summary>
    internal static void crdot(this BigRational.SafeCPU cpu, BigRational a, uint b, BigRational c, uint d, BigRational e, uint f)
    {
      cpu.mul(a, b); cpu.mul(c, d); cpu.sub(); cpu.mul(e, f); cpu.add();
    }
    /// <summary>
    /// cross(b - a, c - a)
    /// </summary>
    internal static void cross(this BigRational.SafeCPU cpu, in Vector3R a, in Vector3R b, in Vector3R c)
    {
      var m = cpu.mark();
      cpu.sub(b.X, a.X); cpu.sub(b.Y, a.Y); cpu.sub(b.Z, a.Z); // u = b - a
      cpu.sub(c.X, a.X); cpu.sub(c.Y, a.Y); cpu.sub(c.Z, a.Z); // v = c - a
      cpu.mul(m + 1, m + 5); cpu.mul(m + 2, m + 4); cpu.sub(); // u.Y * v.Z - u.Z * v.Y
      cpu.mul(m + 2, m + 3); cpu.mul(m + 0, m + 5); cpu.sub(); // u.Z * v.X - u.X * v.Z
      cpu.mul(m + 0, m + 4); cpu.mul(m + 1, m + 3); cpu.sub(); // u.X * v.Y - u.Y * v.X
    }
    /// <summary>
    /// helper for serialization
    /// </summary>
    internal static ReadOnlySpan<char> token(this ref ReadOnlySpan<char> a)
    {
      int i = 0; for (; i < a.Length && !(char.IsWhiteSpace(a[i]) || a[i] == ';'); i++) ;
      var w = a.Slice(0, i); a = a.Slice(i < a.Length ? i + 1 : i).TrimStart(); return w;
    }
  }
}
