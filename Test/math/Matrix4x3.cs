using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Numerics
{
  /// <summary>
  /// A Matrix4x3 class based on <see cref="float"/>.<br/>
  /// <i>This is just a non-optimal example implementation for testing!</i>
  /// </summary>
  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public unsafe struct Matrix4x3 : IEquatable<Matrix4x3>, IFormattable, ISpanFormattable
  {
    //public float M11, M21, M31, M41;
    //public float M12, M22, M32, M42;
    //public float M13, M23, M33, M43;
    public float M11, M12, M13;
    public float M21, M22, M23;
    public float M31, M32, M33;
    public float M41, M42, M43;
    public override readonly string ToString()
    {
      return ToString(null, null); //"G9"
    }
    public readonly string ToString(string? format, IFormatProvider? provider = null)
    {
      provider ??= format?.Length == 0 ? NumberFormatInfo.InvariantInfo : NumberFormatInfo.CurrentInfo; //DebuggerDisplay("", null);
      Span<char> sp = stackalloc char[256]; TryFormat(sp, out var ns, format, provider);
      return sp.Slice(0, ns).ToString();
    }
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
      charsWritten = 0; var sw = destination;
      for (int i = 0, nw = 0; i < 12; i++)
      {
        if (i != 0) { if (sw.Length == 0) return false; sw[0] = ' '; sw = sw.Slice(1); }
        if (!get(this, i).TryFormat(sw, out nw, format, provider)) return false; sw = sw.Slice(nw);
      }
      charsWritten = destination.Length - sw.Length; return true;
      static float get(in Matrix4x3 m, int i)
      {
        switch (i)
        {
          case 0x0: return m.M11;
          case 0x1: return m.M12;
          case 0x2: return m.M13;
          case 0x3: return m.M21;
          case 0x4: return m.M22;
          case 0x5: return m.M23;
          case 0x6: return m.M31;
          case 0x7: return m.M32;
          case 0x8: return m.M33;
          case 0x9: return m.M41;
          case 0xa: return m.M42;
          case 0xb: return m.M43;
          default: return 0;
        }
      }
    }
    public static Matrix4x3 Parse(ReadOnlySpan<char> sp, IFormatProvider? provider = null)
    {
      Matrix4x3 m = default;
      for (int x = 0; sp.Length != 0; x++)
      {
        var i = sp.IndexOf(' '); if (i == -1) i = sp.Length;
        var s = sp.Slice(0, i); sp = sp.Slice(i).Trim();
        set(ref m, x, float.Parse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, provider));
      }
      return m;
      static void set(ref Matrix4x3 m, int i, float f)
      {
        switch (i)
        {
          case 0x0: m.M11 = f; break;
          case 0x1: m.M12 = f; break;
          case 0x2: m.M13 = f; break;
          case 0x3: m.M21 = f; break;
          case 0x4: m.M22 = f; break;
          case 0x5: m.M23 = f; break;
          case 0x6: m.M31 = f; break;
          case 0x7: m.M32 = f; break;
          case 0x8: m.M33 = f; break;
          case 0x9: m.M41 = f; break;
          case 0xa: m.M42 = f; break;
          case 0xb: m.M43 = f; break;
        }
      }
    }
    public override readonly int GetHashCode()
    {
      HashCode h = default;
      h.Add(M11); h.Add(M21); h.Add(M31); h.Add(M41);
      h.Add(M12); h.Add(M22); h.Add(M32); h.Add(M42);
      h.Add(M13); h.Add(M23); h.Add(M33); h.Add(M43);
      return h.ToHashCode();
    }
    public override readonly bool Equals(object? p)
    {
      return p is Matrix4x3 b && this == b;
    }
    public readonly bool Equals(Matrix4x3 b)
    {
      return this == b;
      //M11 == b.M11 && M21 == b.M21 && M31 == b.M31 && M41 == b.M41 &&
      //M12 == b.M12 && M22 == b.M22 && M32 == b.M32 && M42 == b.M42 &&
      //M13 == b.M13 && M23 == b.M23 && M33 == b.M33 && M43 == b.M43;
    }
    public Vector3 this[int i]
    {
      get
      {
        switch (i)
        {
          case 00: return new Vector3(M11, M12, M13);
          case 01: return new Vector3(M21, M22, M23);
          case 02: return new Vector3(M31, M32, M33);
          default: return new Vector3(M41, M42, M43);
        }
      }
      set
      {
        switch (i)
        {
          case 00: M11 = value.X; M12 = value.Y; M13 = value.Z; break;
          case 01: M21 = value.X; M22 = value.Y; M23 = value.Z; break;
          case 02: M31 = value.X; M32 = value.Y; M33 = value.Z; break;
          default: M41 = value.X; M42 = value.Y; M43 = value.Z; break;
        }
      }
    }
    public static Matrix4x3 Identity
    {
      get => new() { M11 = 1, M22 = 1, M33 = 1 };
    }
    public readonly bool IsIdentity => this == Identity;
    public Vector3 Translation
    {
      readonly get => new Vector3(M41, M42, M43);
      set { M41 = value.X; M42 = value.Y; M43 = value.Z; }
    }
    static Vector3 round(Vector3 v, int digits)
    {
      v.X = MathF.Round(v.X, digits); if (v.X == 0) v.X = 0;
      v.Y = MathF.Round(v.Y, digits); if (v.Y == 0) v.Y = 0;
      v.Z = MathF.Round(v.Z, digits); if (v.Z == 0) v.Z = 0;
      return v;
    }
    public Vector3 Scaling
    {
      readonly get
      {
        var vx = new Vector3(M11, M12, M13);
        var vy = new Vector3(M21, M22, M23);
        var vz = new Vector3(M31, M32, M33);
        var sc = new Vector3(vx.Length(), vy.Length(), vz.Length());
        sc = round(sc, 4); return sc;
      }
      set
      {
        var vx = new Vector3(M11, M12, M13);
        var vy = new Vector3(M21, M22, M23);
        var vz = new Vector3(M31, M32, M33);
        if (value.X > 0) vx *= value.X / vx.Length();
        if (value.Y > 0) vy *= value.Y / vy.Length();
        if (value.Z > 0) vz *= value.Z / vz.Length();
        M11 = vx.X; M12 = vx.Y; M13 = vx.Z;
        M21 = vy.X; M22 = vy.Y; M23 = vy.Z;
        M31 = vz.X; M32 = vz.Y; M33 = vz.Z;
      }
    }
    public Vector3 Rotation
    {
      readonly get
      {
        var vx = Vector3.Normalize(new Vector3(M11, M12, M13));
        var vy = Vector3.Normalize(new Vector3(M21, M22, M23));
        var vz = Vector3.Normalize(new Vector3(M31, M32, M33));
        var ax = +MathF.Atan2(vy.Z, vz.Z);
        var ay = -MathF.Asin(vx.Z);
        var az = vx.Z == 1 || vx.Z == -1 ? -MathF.Atan2(vy.X, vy.Y) : MathF.Atan2(vx.Y, vx.X);
        var an = new Vector3(ax, ay, az) * (180 / MathF.PI);
        an = round(an, 4); return an;
      }
      set
      {
        var an = value * (MathF.PI / 180);
        this =
          Matrix4x3.CreateScale(Scaling) *
          Matrix4x3.CreateRotationX(an.X) *
          Matrix4x3.CreateRotationY(an.Y) *
          Matrix4x3.CreateRotationZ(an.Z) *
          Matrix4x3.CreateTranslation(Translation);
      }
    }
    public static implicit operator Matrix4x4(Matrix4x3 m)
    {
      return new Matrix4x4(m.M11, m.M12, m.M13, 0, m.M21, m.M22, m.M23, 0, m.M31, m.M32, m.M33, 0, m.M41, m.M42, m.M43, 1);
    }
    public static explicit operator Matrix4x3(Matrix4x4 m)
    {
      return new Matrix4x3 { M11 = m.M11, M12 = m.M12, M13 = m.M13, M21 = m.M21, M22 = m.M22, M23 = m.M23, M31 = m.M31, M32 = m.M32, M33 = m.M33, M41 = m.M41, M42 = m.M42, M43 = m.M43 };
    }
    public static implicit operator Matrix4x3R(Matrix4x3 m)
    {
      return new Matrix4x3R { M11 = m.M11, M12 = m.M12, M13 = m.M13, M21 = m.M21, M22 = m.M22, M23 = m.M23, M31 = m.M31, M32 = m.M32, M33 = m.M33, M41 = m.M41, M42 = m.M42, M43 = m.M43 };
    }
    public static explicit operator Matrix4x3(in Matrix4x3R m)
    {
      return new Matrix4x3 { M11 = (float)m.M11, M12 = (float)m.M12, M13 = (float)m.M13, M21 = (float)m.M21, M22 = (float)m.M22, M23 = (float)m.M23, M31 = (float)m.M31, M32 = (float)m.M32, M33 = (float)m.M33, M41 = (float)m.M41, M42 = (float)m.M42, M43 = (float)m.M43 };
    }
    public static unsafe bool operator ==(Matrix4x3 a, Matrix4x3 b)
    {
      // //Sse.MoveMask(Sse.CompareNotEqual(vector1, vector2)) == 0;
      // if (Sse.IsSupported)
      //   return Sse.MoveMask(Sse.CompareNotEqual(Sse.LoadVector128(&a.M11), Sse.LoadVector128(&b.M11))) == 0 &&
      //          Sse.MoveMask(Sse.CompareNotEqual(Sse.LoadVector128(&a.M12), Sse.LoadVector128(&b.M12))) == 0 &&
      //          Sse.MoveMask(Sse.CompareNotEqual(Sse.LoadVector128(&a.M13), Sse.LoadVector128(&b.M13))) == 0;
      return
        a.M11 == b.M11 && a.M21 == b.M21 && a.M31 == b.M31 && a.M41 == b.M41 &&
        a.M12 == b.M12 && a.M22 == b.M22 && a.M32 == b.M32 && a.M42 == b.M42 &&
        a.M13 == b.M13 && a.M23 == b.M23 && a.M33 == b.M33 && a.M43 == b.M43;
    }
    public static unsafe bool operator !=(Matrix4x3 a, Matrix4x3 b)
    {
      return !(a == b);
    }
    public static Matrix4x3 operator *(Matrix4x3 a, Matrix4x3 b)
    {
      Matrix4x3 m;
      m.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31;
      m.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32;
      m.M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33;

      m.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31;
      m.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32;
      m.M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33;

      m.M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31;
      m.M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32;
      m.M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33;

      m.M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + b.M41;
      m.M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + b.M42;
      m.M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + b.M43;

      return m;
    }
    /// <summary>
    /// Inverts the specified matrix.
    /// </summary>
    /// <param name="a">The matrix to invert.</param>
    /// <returns>When this method returns, contains the inverted matrix if the operation succeeded.</returns>
    public static Matrix4x3 operator !(Matrix4x3 a)
    {
      var t1 = a.M32 * a.M43 - a.M33 * a.M42;
      var t2 = a.M31 * a.M43 - a.M33 * a.M41;
      var t3 = a.M31 * a.M42 - a.M32 * a.M41;
      var t4 = a.M22 * a.M33 - a.M23 * a.M32;
      var t5 = a.M23 * a.M31 - a.M21 * a.M33;
      var t6 = a.M21 * a.M32 - a.M22 * a.M31;
      var t8 = a.M11 * t4 + a.M12 * t5 + a.M13 * t6; t8 = 1.0f / t8;
      var t7 = a.M22 * t2 - a.M21 * t1 - a.M23 * t3;
      Matrix4x3 b;
      b.M11 = t4 * t8;
      b.M21 = t5 * t8;
      b.M31 = t6 * t8;
      b.M41 = t7 * t8;
      b.M12 = (a.M13 * a.M32 - a.M12 * a.M33) * t8;
      b.M22 = (a.M11 * a.M33 - a.M13 * a.M31) * t8;
      b.M32 = (a.M12 * a.M31 - a.M11 * a.M32) * t8;
      b.M42 = (a.M11 * t1 - a.M12 * t2 + a.M13 * t3) * t8;
      var v1 = a.M23 * a.M42 - a.M22 * a.M43;
      var v2 = a.M21 * a.M43 - a.M23 * a.M41;
      var v3 = a.M22 * a.M41 - a.M21 * a.M42;
      b.M13 = (a.M12 * a.M23 - a.M13 * a.M22) * t8;
      b.M23 = (a.M13 * a.M21 - a.M11 * a.M23) * t8;
      b.M33 = (a.M11 * a.M22 - a.M12 * a.M21) * t8;
      b.M43 = (a.M12 * v2 + a.M11 * v1 + a.M13 * v3) * t8;
      return b;
    }
    public static Matrix4x3 operator *(Matrix4x3 a, Vector3 b)
    {
      a.M41 += b.X;
      a.M42 += b.Y;
      a.M43 += b.Z; return a;
    }
    public static Matrix4x3 CreateTranslation(Vector3 p)
    {
      return new() { M11 = 1, M22 = 1, M33 = 1, M41 = p.X, M42 = p.Y, M43 = p.Z };
    }
    public static Matrix4x3 CreateTranslation(float x, float y, float z)
    {
      return CreateTranslation(new Vector3(x, y, z));
    }
    public static Matrix4x3 CreateScale(Vector3 s)
    {
      return new() { M11 = s.X, M22 = s.Y, M33 = s.Z };
    }
    public static Matrix4x3 CreateScale(float s)
    {
      return CreateScale(new Vector3(s));
    }
    public static Matrix4x3 CreateRotationX(float a)
    {
      float c = MathF.Cos(a), s = MathF.Sin(a);
      if (MathF.Abs(s) == 1) c = 0; else if (MathF.Abs(c) == 1) s = 0;
      return new() { M11 = 1, M22 = c, M23 = s, M32 = -s, M33 = c };
    }
    public static Matrix4x3 CreateRotationY(float a)
    {
      float c = MathF.Cos(a), s = MathF.Sin(a);
      if (MathF.Abs(s) == 1) c = 0; else if (MathF.Abs(c) == 1) s = 0;
      return new() { M22 = 1, M11 = c, M13 = -s, M31 = s, M33 = c };
    }
    public static Matrix4x3 CreateRotationZ(float a)
    {
      float c = MathF.Cos(a), s = MathF.Sin(a);
      if (MathF.Abs(s) == 1) c = 0; else if (MathF.Abs(c) == 1) s = 0;
      return new() { M33 = 1, M11 = c, M12 = s, M21 = -s, M22 = c };
    }
    public static Vector3 Transform(Vector3 a, in Matrix4x3 b)
    {
      return new Vector3(
        a.X * b.M11 + a.Y * b.M21 + a.Z * b.M31 + b.M41,
        a.X * b.M12 + a.Y * b.M22 + a.Z * b.M32 + b.M42,
        a.X * b.M13 + a.Y * b.M23 + a.Z * b.M33 + b.M43);
    }
  }
}
