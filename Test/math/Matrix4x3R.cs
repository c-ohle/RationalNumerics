
namespace System.Numerics.Rational
{
  /// <summary>
  /// A Matrix4x3 class based on NewRational.<br/>
  /// <i>This is just a non optimal example implementation!</i>/>
  /// </summary>
  [Serializable]
  public struct Matrix4x3R : IEquatable<Matrix4x3R>
  {
    public rat M11, M12, M13;
    public rat M21, M22, M23;
    public rat M31, M32, M33;
    public rat M41, M42, M43;
    public readonly void WriteToBytes(ref Span<byte> ws)
    {
      M11.WriteToBytes(ref ws); M12.WriteToBytes(ref ws); M12.WriteToBytes(ref ws);
      M21.WriteToBytes(ref ws); M22.WriteToBytes(ref ws); M22.WriteToBytes(ref ws);
      M31.WriteToBytes(ref ws); M32.WriteToBytes(ref ws); M32.WriteToBytes(ref ws);
      M41.WriteToBytes(ref ws); M42.WriteToBytes(ref ws); M42.WriteToBytes(ref ws);
    }
    public static Matrix4x3R ReadFromBytes(ref ReadOnlySpan<byte> rs)
    {
      var m = new Matrix4x3R();
      m.M11 = rat.ReadFromBytes(ref rs); m.M12 = rat.ReadFromBytes(ref rs); m.M12 = rat.ReadFromBytes(ref rs);
      m.M21 = rat.ReadFromBytes(ref rs); m.M22 = rat.ReadFromBytes(ref rs); m.M22 = rat.ReadFromBytes(ref rs);
      m.M31 = rat.ReadFromBytes(ref rs); m.M32 = rat.ReadFromBytes(ref rs); m.M32 = rat.ReadFromBytes(ref rs);
      m.M41 = rat.ReadFromBytes(ref rs); m.M42 = rat.ReadFromBytes(ref rs); m.M42 = rat.ReadFromBytes(ref rs);
      return m;
    }
    public override int GetHashCode()
    {
      return
        +HashCode.Combine(M11, M12, M13, M21, M22, M23) ^
        ~HashCode.Combine(M31, M32, M33, M41, M42, M43);
    }
    public override bool Equals(object? p)
    {
      return p is Matrix4x3R b && Equals(b);
    }
    public readonly bool Equals(Matrix4x3R b)
    {
      return
        M11.Equals(b.M11) && M12.Equals(b.M12) && M13.Equals(b.M13) &&
        M21.Equals(b.M21) && M22.Equals(b.M22) && M23.Equals(b.M23) &&
        M31.Equals(b.M31) && M32.Equals(b.M32) && M33.Equals(b.M33) &&
        M41.Equals(b.M41) && M42.Equals(b.M42) && M43.Equals(b.M43);
    }
    public static explicit operator Matrix4x4(in Matrix4x3R m)
    {
      return new Matrix4x4(
          (float)m.M11, (float)m.M12, (float)m.M13, 0,
          (float)m.M21, (float)m.M22, (float)m.M23, 0,
          (float)m.M31, (float)m.M32, (float)m.M33, 0,
          (float)m.M41, (float)m.M42, (float)m.M43, 1);
    }
    public static explicit operator Matrix4x3R(in Matrix4x4 m)
    {
      return new Matrix4x3R
      {
        M11 = m.M11,
        M12 = m.M12,
        M13 = m.M13,
        M21 = m.M21,
        M22 = m.M22,
        M23 = m.M23,
        M31 = m.M31,
        M32 = m.M32,
        M33 = m.M33,
        M41 = m.M41,
        M42 = m.M42,
        M43 = m.M43
      };
    }
    public static bool operator ==(in Matrix4x3R a, in Matrix4x3R b)
    {
      return
        a.M11 == b.M11 && a.M21 == b.M21 && a.M31 == b.M31 && a.M41 == b.M41 &&
        a.M12 == b.M12 && a.M22 == b.M22 && a.M32 == b.M32 && a.M42 == b.M42 &&
        a.M13 == b.M13 && a.M23 == b.M23 && a.M33 == b.M33 && a.M43 == b.M43;
    }
    public static bool operator !=(in Matrix4x3R a, in Matrix4x3R b)
    {
      return !(a == b);
    }
    public static Matrix4x3R operator *(in Matrix4x3R a, in Matrix4x3R b)
    {
      //Matrix4x3R m;
      //m.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31;
      //m.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32;
      //m.M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33;
      //m.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31;
      //m.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32;
      //m.M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33;
      //m.M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31;
      //m.M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32;
      //m.M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33;
      //m.M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + b.M41;
      //m.M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + b.M42;
      //m.M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + b.M43;
      //return m;
      var cpu = rat.task_cpu; Matrix4x3R c;
      cpu.dot(a.M11, b.M11, a.M12, b.M21, a.M13, b.M31); c.M11 = cpu.pop_rat();
      cpu.dot(a.M11, b.M12, a.M12, b.M22, a.M13, b.M32); c.M12 = cpu.pop_rat();
      cpu.dot(a.M11, b.M13, a.M12, b.M23, a.M13, b.M33); c.M13 = cpu.pop_rat();
      cpu.dot(a.M21, b.M11, a.M22, b.M21, a.M23, b.M31); c.M21 = cpu.pop_rat();
      cpu.dot(a.M21, b.M12, a.M22, b.M22, a.M23, b.M32); c.M22 = cpu.pop_rat();
      cpu.dot(a.M21, b.M13, a.M22, b.M23, a.M23, b.M33); c.M23 = cpu.pop_rat();
      cpu.dot(a.M31, b.M11, a.M32, b.M21, a.M33, b.M31); c.M31 = cpu.pop_rat();
      cpu.dot(a.M31, b.M12, a.M32, b.M22, a.M33, b.M32); c.M32 = cpu.pop_rat();
      cpu.dot(a.M31, b.M13, a.M32, b.M23, a.M33, b.M33); c.M33 = cpu.pop_rat();
      cpu.dot(a.M41, b.M11, a.M42, b.M21, a.M43, b.M31, b.M41); c.M41 = cpu.pop_rat();
      cpu.dot(a.M41, b.M12, a.M42, b.M22, a.M43, b.M32, b.M42); c.M42 = cpu.pop_rat();
      cpu.dot(a.M41, b.M13, a.M42, b.M23, a.M43, b.M33, b.M43); c.M43 = cpu.pop_rat();
      return c;
    }
    public static Matrix4x3R operator !(in Matrix4x3R a)
    {
#if false
      rat x = a.M11, b = a.M12, c = a.M13;
      rat e = a.M21, f = a.M22, g = a.M23;
      rat i = a.M31, j = a.M32, k = a.M33;
      rat m = a.M41, n = a.M42, o = a.M43;
      rat jo_kn = j * o - k * n;
      rat io_km = i * o - k * m;
      rat in_jm = i * n - j * m;
      rat a11 = +(f * k - g * j);
      rat a12 = -(e * k - g * i);
      rat a13 = +(e * j - f * i);
      rat a14 = -(e * jo_kn - f * io_km + g * in_jm);
      rat det = x * a11 + b * a12 + c * a13;
      Matrix4x3R r;
      r.M11 = a11 / det;
      r.M21 = a12 / det;
      r.M31 = a13 / det;
      r.M41 = a14 / det;
      r.M12 = -(b * k - c * j) / det;
      r.M22 = +(x * k - c * i) / det;
      r.M32 = -(x * j - b * i) / det;
      r.M42 = +(x * jo_kn - b * io_km + c * in_jm) / det;
      rat fo_gn = f * o - g * n;
      rat eo_gm = e * o - g * m;
      rat en_fm = e * n - f * m;
      r.M13 = +(b * g - c * f) / det;
      r.M23 = -(x * g - c * e) / det;
      r.M33 = +(x * f - b * e) / det;
      r.M43 = -(x * fo_gn - b * eo_gm + c * en_fm) / det;
      return r;
#else
      var cpu = rat.task_cpu; var m = cpu.mark();
      cpu.cross(a.M31, a.M42, a.M32, a.M41);
      cpu.cross(a.M31, a.M43, a.M33, a.M41);
      cpu.cross(a.M32, a.M43, a.M33, a.M42);
      cpu.cross(a.M22, a.M33, a.M23, a.M32);
      cpu.cross(a.M21, a.M33, a.M23, a.M31);
      cpu.cross(a.M21, a.M32, a.M22, a.M31);
      cpu.crdot(a.M21, m + 02, a.M22, m + 01, a.M23, m + 00);
      cpu.crdot(a.M11, m + 03, a.M12, m + 04, a.M13, m + 05);
      //if (cpu.sign() == 0) { } // det
      cpu.cross(a.M11, a.M22, a.M12, a.M21);
      cpu.cross(a.M11, a.M23, a.M13, a.M21);
      cpu.cross(a.M12, a.M23, a.M13, a.M22);
      cpu.cross(a.M12, a.M33, a.M13, a.M32);
      cpu.cross(a.M11, a.M33, a.M13, a.M31);
      cpu.cross(a.M11, a.M32, a.M12, a.M31);
      cpu.crdot(a.M11, m + 02, a.M12, m + 01, a.M13, m + 00);
      cpu.crdot(a.M41, m + 10, a.M42, m + 09, a.M43, m + 08);
      Matrix4x3R b;
      cpu.div(m + 03, m + 7); b.M11 = cpu.pop_rat(); cpu.div(m + 11, m + 7); cpu.neg(); b.M12 = cpu.pop_rat();
      cpu.div(m + 10, m + 7); b.M13 = cpu.pop_rat(); cpu.div(m + 04, m + 7); cpu.neg(); b.M21 = cpu.pop_rat();
      cpu.div(m + 12, m + 7); b.M22 = cpu.pop_rat(); cpu.div(m + 09, m + 7); cpu.neg(); b.M23 = cpu.pop_rat();
      cpu.div(m + 05, m + 7); b.M31 = cpu.pop_rat(); cpu.div(m + 13, m + 7); cpu.neg(); b.M32 = cpu.pop_rat();
      cpu.div(m + 08, m + 7); b.M33 = cpu.pop_rat(); cpu.div(m + 06, m + 7); cpu.neg(); b.M41 = cpu.pop_rat();
      cpu.div(m + 14, m + 7); b.M42 = cpu.pop_rat(); cpu.div(m + 15, m + 7); cpu.neg(); b.M43 = cpu.pop_rat();
      cpu.pop(16); return b;
#endif
    }
    public static Matrix4x3R Inverse(in Matrix4x3R a)
    {
      return !a;
    }
  }
}
