using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Classes for testing BigRat in practice, these are not optimal implementations

namespace NewNumeric
{
  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct BigRatSet : IEnumerable<BigRat>, IEnumerable, IEquatable<BigRatSet>, IFormattable
  {
    public int Length
    {
      get => p != null ? unchecked((int)p[0]) : 0;
    }
    public BigRat this[int i]
    {
      get
      {
        if (this.p == null || (uint)i >= this.p[0]) throw new ArgumentOutOfRangeException(nameof(i));
        var k = unchecked((int)this.p[1 + i]);
        return k != 0 ? new BigRat(this.p.AsSpan().Slice(k)) : default;
      }
    }

    public BigRatSet(IEnumerable<BigRat> values)
    {
      var tg = values.Select((p, i) => (p, i)).GroupBy(p => p.p, p => p.i).ToArray();
      var ti = tg.Select((p, i) => p.Select(t => (t, i))).SelectMany(p => p).OrderBy(p => p.t).Select(p => p.i).ToArray();
      var tv = tg.Select(p => p.Key).ToArray(); Debug.Assert(ti.Select(i => tv[i]).SequenceEqual(values));
      var tn = tv.Sum(p => p.AsSpan().Length);
      var bb = new uint[1 + ti.Length + tn];
      for (int i = 0, y = 1 + ti.Length; i < tv.Length; i++) { bb[1 + ti.Length + i] = (uint)y; y += tv[i].AsSpan().Length; }
      bb[0] = (uint)ti.Length; for (int i = 0; i < ti.Length; i++) { bb[i + 1] = tv[ti[i]].AsSpan().Length != 0 ? bb[1 + ti.Length + ti[i]] : 0; }
      for (int i = 0, y = 1 + ti.Length; i < tv.Length; i++) { var sp = tv[i].AsSpan(); sp.CopyTo(bb.AsSpan().Slice(y)); y += sp.Length; }
      this.p = bb;
    }
    public BigRatSet(params BigRat[] values)
    {
      this.p = new BigRatSet(values.AsEnumerable<BigRat>()).p;
    }

    public override int GetHashCode()
    {
      var hash = new HashCode(); hash.AddBytes(MemoryMarshal.Cast<uint, byte>(this.p));
      return hash.ToHashCode();
    }
    public bool Equals(BigRatSet other)
    {
      return this.p == other.p || this.p != null && other.p != null && this.p.AsSpan().SequenceEqual(other.p.AsSpan());
      //if (this.p == other.p) return true;
      //if (this.p == null || other.p == null || this.Length != other.Length) return false;
      //for (int i = 0, n = this.Length; i < n; i++) if (this[i] != other[i]) return false;
      //return true;
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is BigRatSet v && this.Equals(v);
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
      if (format != default && format.Length == 0) { format = "Q8"; provider = NumberFormatInfo.InvariantInfo; } // "" : dbg
      return $"[{string.Join("; ", this.Select(p => p.ToString(format, provider)))}]";
    }
    public string ToString(string? format)
    {
      return ToString(format, default(IFormatProvider));
    }
    public string ToString(IFormatProvider? provider)
    {
      return ToString(default(string), provider);
    }
    public override string ToString()
    {
      return ToString(null, null);
    }

    public IEnumerator<BigRat> GetEnumerator()
    {
      for (int i = 0, n = Length; i < n; i++) yield return this[i];
    }

    // fast bitlevel access, binary serialization etc.
    public ReadOnlySpan<uint> AsSpan()
    {
      return p;
    }
    public BigRatSet(ReadOnlySpan<uint> span)
    {
      if (span.Length == 0) return; //empty
      uint c = span[0], l = 0; for (int i = 1; i <= c; l = Math.Max(l, span[i]), i++) ;
      if (l == 0) l = 1 + c; else { l += (span[(int)l] & 0x3fffffff) + 1; l += span[(int)l] + 1; }
      this.p = span.Slice(0, (int)l).ToArray();
    }

    readonly uint[]? p;

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct BigRatVector2 : IEquatable<BigRatVector2>, IFormattable
  {
    public BigRat X
    {
      get => p != null ? p[0] : default;
    }
    public BigRat Y
    {
      get => p != null ? p[1] : default;
    }

    public static BigRatVector2 Zero
    {
      get => default;
    }
    public static BigRatVector2 One
    {
      get => new BigRatVector2(BigRatVector3.RatOne);
    }
    public static BigRatVector2 UnitX
    {
      get => new BigRatVector2(BigRatVector3.RatOne, 0);
    }
    public static BigRatVector2 UnitY
    {
      get => new BigRatVector2(0, BigRatVector3.RatOne);
    }

    public BigRat this[int i]
    {
      get => p != null ? p[i] : default;
    }

    public static implicit operator BigRatVector2(Vector2 value)
    {
      return new BigRatVector2(value.X, value.Y);
    }
    public static explicit operator Vector2(BigRatVector2 value)
    {
      return new Vector2((float)value.X, (float)value.Y);
    }

    public static BigRatVector2 operator +(BigRatVector2 a, BigRatVector2 b)
    {
      return new BigRatVector2(a.X + b.X, a.Y + b.Y);
    }
    public static BigRatVector2 operator -(BigRatVector2 a, BigRatVector2 b)
    {
      return new BigRatVector2(a.X - b.X, a.Y - b.Y);
    }
    public static BigRatVector2 operator *(BigRatVector2 a, BigRat b)
    {
      return new BigRatVector2(a.X * b, a.Y * b);
    }
    public static BigRatVector2 operator *(BigRatVector2 a, BigRatVector2 b)
    {
      return new BigRatVector2(a.X * b.X, a.Y * b.Y);
    }
    public static BigRatVector2 operator /(BigRatVector2 a, BigRat b)
    {
      return new BigRatVector2(a.X / b, a.Y / b);
    }
    public static BigRatVector2 operator +(BigRatVector2 a)
    {
      return a;
    }
    public static BigRatVector2 operator -(BigRatVector2 a)
    {
      return new BigRatVector2(-a.X, -a.Y);
    }

    public static bool operator ==(BigRatVector2 a, BigRatVector2 b)
    {
      return a.X == b.X && a.Y == b.Y;
    }
    public static bool operator !=(BigRatVector2 a, BigRatVector2 b)
    {
      return a.X != b.X || a.Y != b.Y;
    }

    public static BigRatVector2 Abs(BigRatVector2 a)
    {
      return new BigRatVector2(BigRat.Abs(a.X), BigRat.Abs(a.Y));
    }
    public static BigRatVector2 Min(BigRatVector2 a, BigRatVector2 b)
    {
      return new BigRatVector2(BigRat.Min(a.X, b.X), BigRat.Min(a.Y, b.Y));
    }
    public static BigRatVector2 Max(BigRatVector2 a, BigRatVector2 b)
    {
      return new BigRatVector2(BigRat.Max(a.X, b.X), BigRat.Max(a.Y, b.Y));
    }
    public static BigRatVector2 Clamp(BigRatVector2 value, BigRatVector2 min, BigRatVector2 max)
    {
      return Min(Max(value, min), max);
    }
    public static BigRat Dot(BigRatVector2 a, BigRatVector2 b)
    {
      return (a.X * b.X + a.Y * b.Y).Normalize();
    }
    public static BigRat Cross(BigRatVector2 a, BigRatVector2 b)
    {
      return (a.X * b.Y - a.Y * b.X).Normalize();
    }
    public static BigRatVector2 Lerp(BigRatVector2 a, BigRatVector2 b, BigRat amount)
    {
      return (a * (1 - amount) + b * amount).Normalize();
    }
    public static BigRat LengthSq(BigRatVector2 a)
    {
      return Dot(a, a);
    }
    public static BigRat Length(BigRatVector2 a, int digits)
    {
      return BigRat.Sqrt(LengthSq(a), digits);
    }
    public static BigRatVector2 Normalize(BigRatVector2 a)
    {
      var b = Abs(a); var l = BigRat.Max(b.X, b.Y); //if (l == 0) return a;
      return a / l;
    }
    public static BigRatVector2 Normalize(BigRatVector2 a, int digits)
    {
      var l = a.X * a.X + a.Y * a.Y; //if (l == 0) return a;
      l = BigRat.Sqrt(l, digits); var b = a / l; b = b.Normalize(); return b;
    }
    public static BigRatVector2 Round(BigRatVector2 a, int digits)
    {
      return new BigRatVector2(BigRat.Round(a.X, digits), BigRat.Round(a.Y, digits));
    }
    public static BigRatVector2 Transform(BigRatVector2 position, BigRatMatrix4x3 matrix)
    {
      return new BigRatVector2(
          (position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41).Normalize(),
          (position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42).Normalize());
    }
    public static BigRatVector2 TransformNormal(BigRatVector2 normal, BigRatMatrix4x3 matrix)
    {
      return new BigRatVector2(
          (normal.X * matrix.M11 + normal.Y * matrix.M21).Normalize(),
          (normal.X * matrix.M12 + normal.Y * matrix.M22).Normalize());
    }

    public BigRatVector2(BigRat xy)
    {
      p = new BigRat[] { xy, xy };
    }
    public BigRatVector2(BigRat x, BigRat y)
    {
      p = new BigRat[] { x, y };
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
      if (format != default && format.Length == 0) { format = "Q8"; provider = NumberFormatInfo.InvariantInfo; } // "" : dbg
      return $"[{X.ToString(format, provider)}; {Y.ToString(format, provider)}]";
    }
    public string ToString(string? format)
    {
      return ToString(format, default(IFormatProvider));
    }
    public string ToString(IFormatProvider? provider)
    {
      return ToString(default(string), provider);
    }
    public override string ToString()
    {
      return ToString(null, null);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(X, Y);
    }
    public bool Equals(BigRatVector2 other)
    {
      return X == other.X && Y == other.Y;
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is BigRatVector2 t && this.Equals(t);
    }

    public BigRatVector2 Normalize()
    {
      return new BigRatVector2(X.Normalize(), Y.Normalize());
    }

    readonly BigRat[]? p;
  }

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct BigRatVector3 : IEquatable<BigRatVector3>, IFormattable
  {
    public BigRat X
    {
      get => p != null ? p[0] : default;
    }
    public BigRat Y
    {
      get => p != null ? p[1] : default;
    }
    public BigRat Z
    {
      get => p != null ? p[2] : default;
    }

    public static BigRatVector3 Zero
    {
      get => default;
    }
    public static BigRatVector3 One
    {
      get => new BigRatVector3(BigRatVector3.RatOne, BigRatVector3.RatOne, BigRatVector3.RatOne);
    }
    public static BigRatVector3 UnitX
    {
      get => new BigRatVector3(BigRatVector3.RatOne, 0, 0);
    }
    public static BigRatVector3 UnitY
    {
      get => new BigRatVector3(0, BigRatVector3.RatOne, 0);
    }
    public static BigRatVector3 UnitZ
    {
      get => new BigRatVector3(0, 0, BigRatVector3.RatOne);
    }

    public BigRat this[int i]
    {
      get => p != null ? p[i] : default;
    }

    public static implicit operator BigRatVector3(Vector3 value)
    {
      return new BigRatVector3(value.X, value.Y, value.Z);
    }
    public static explicit operator Vector3(BigRatVector3 value)
    {
      return new Vector3((float)value.X, (float)value.Y, (float)value.Z);
    }

    public static BigRatVector3 operator +(BigRatVector3 a, BigRatVector3 b)
    {
      return new BigRatVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    public static BigRatVector3 operator -(BigRatVector3 a, BigRatVector3 b)
    {
      return new BigRatVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    public static BigRatVector3 operator *(BigRatVector3 a, BigRat b)
    {
      return new BigRatVector3(a.X * b, a.Y * b, a.Z * b);
    }
    public static BigRatVector3 operator *(BigRatVector3 a, BigRatVector3 b)
    {
      return new BigRatVector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }
    public static BigRatVector3 operator /(BigRatVector3 a, BigRat b)
    {
      return new BigRatVector3(a.X / b, a.Y / b, a.Z / b);
    }
    public static BigRatVector3 operator +(BigRatVector3 a)
    {
      return a;
    }
    public static BigRatVector3 operator -(BigRatVector3 a)
    {
      return new BigRatVector3(-a.X, -a.Y, -a.Z);
    }

    public static bool operator ==(BigRatVector3 a, BigRatVector3 b)
    {
      return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    }
    public static bool operator !=(BigRatVector3 a, BigRatVector3 b)
    {
      return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
    }

    public static BigRatVector3 Abs(BigRatVector3 a)
    {
      return new BigRatVector3(BigRat.Abs(a.X), BigRat.Abs(a.Y), BigRat.Abs(a.Z));
    }
    public static BigRatVector3 Min(BigRatVector3 a, BigRatVector3 b)
    {
      return new BigRatVector3(BigRat.Min(a.X, b.X), BigRat.Min(a.Y, b.Y), BigRat.Min(a.Z, b.Z));
    }
    public static BigRatVector3 Max(BigRatVector3 a, BigRatVector3 b)
    {
      return new BigRatVector3(BigRat.Max(a.X, b.X), BigRat.Max(a.Y, b.Y), BigRat.Max(a.Z, b.Z));
    }
    public static BigRatVector3 Clamp(BigRatVector3 value, BigRatVector3 min, BigRatVector3 max)
    {
      return Min(Max(value, min), max);
    }
    public static BigRat Dot(BigRatVector3 a, BigRatVector3 b)
    {
      return (a.X * b.X + a.Y * b.Y + a.Z * b.Z).Normalize();
    }
    public static BigRatVector3 Cross(BigRatVector3 a, BigRatVector3 b)
    {
      return new BigRatVector3(
        (a.Y * b.Z - a.Z * b.Y).Normalize(),
        (a.Z * b.X - a.X * b.Z).Normalize(),
        (a.X * b.Y - a.Y * b.X).Normalize());
    }
    public static BigRatVector3 Lerp(BigRatVector3 a, BigRatVector3 b, BigRat amount)
    {
      return (a * (1 - amount) + b * amount).Normalize();
    }
    public static BigRat LengthSq(BigRatVector3 a)
    {
      return Dot(a, a);
    }
    public static BigRat Length(BigRatVector3 a, int digits)
    {
      return BigRat.Sqrt(Dot(a, a), digits);
    }
    public static BigRatVector3 Normalize(BigRatVector3 a)
    {
      var b = Abs(a); var c = BigRat.Max(BigRat.Max(b.X, b.Y), b.Z); return a / c;
    }
    public static BigRatVector3 Normalize(BigRatVector3 a, int digits)
    {
      var l = BigRat.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z, digits);
      var b = a / l; b = b.Normalize(); return b;
    }
    public static BigRatVector3 Round(BigRatVector3 a, int digits)
    {
      return new BigRatVector3(
        BigRat.Round(a.X, digits),
        BigRat.Round(a.Y, digits),
        BigRat.Round(a.Z, digits));
    }
    public static BigRatVector3 Transform(BigRatVector3 position, BigRatMatrix4x3 matrix)
    {
      return new BigRatVector3(
          (position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41).Normalize(),
          (position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42).Normalize(),
          (position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43).Normalize());
    }
    public static BigRatVector3 TransformNormal(BigRatVector3 normal, BigRatMatrix4x3 matrix)
    {
      return new BigRatVector3(
          (normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31).Normalize(),
          (normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32).Normalize(),
          (normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33).Normalize());
    }

    public BigRatVector3(BigRat xyz)
    {
      p = new BigRat[] { xyz, xyz, xyz };
    }
    public BigRatVector3(BigRat x, BigRat y, BigRat z)
    {
      p = new BigRat[] { x, y, z };
    }
    public BigRatVector3(BigRatVector2 xy, BigRat z)
    {
      p = new BigRat[] { xy.X, xy.Y, z };
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
      if (format != default && format.Length == 0) { format = "Q8"; provider = NumberFormatInfo.InvariantInfo; } // "" : dbg
      return $"[{X.ToString(format, provider)}; {Y.ToString(format, provider)}; {Z.ToString(format, provider)}]";
    }
    public string ToString(string? format)
    {
      return ToString(format, default(IFormatProvider));
    }
    public string ToString(IFormatProvider? provider)
    {
      return ToString(default(string), provider);
    }
    public override string ToString()
    {
      return ToString(null, null);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(X, Y, Z);
    }
    public bool Equals(BigRatVector3 other)
    {
      return X == other.X && Y == other.Y && Z == other.Z;
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is BigRatVector3 t && this.Equals(t);
    }

    public BigRatVector3 Normalize()
    {
      return new BigRatVector3(X.Normalize(), Y.Normalize(), Z.Normalize());
    }

    readonly BigRat[]? p;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static readonly BigRat RatOne = 1u;
  }

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct BigRatPlane : IEquatable<BigRatPlane>, IFormattable
  {
    public BigRat X
    {
      get => p != null ? p[0] : default;
    }
    public BigRat Y
    {
      get => p != null ? p[1] : default;
    }
    public BigRat Z
    {
      get => p != null ? p[2] : default;
    }
    public BigRat D
    {
      get => p != null ? p[3] : default;
    }

    public BigRat this[int i]
    {
      get => p != null ? p[i] : default;
    }

    public static BigRatPlane operator +(BigRatPlane a)
    {
      return a;
    }
    public static BigRatPlane operator -(BigRatPlane a)
    {
      return new BigRatPlane(-a.X, -a.Y, -a.Z, -a.D);
    }

    public static bool operator ==(BigRatPlane a, BigRatPlane b)
    {
      return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.D == b.D;
    }
    public static bool operator !=(BigRatPlane a, BigRatPlane b)
    {
      return a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.D != b.D;
    }

    public static implicit operator BigRatPlane(Plane value)
    {
      var n = value.Normal; return new BigRatPlane(n.X, n.Y, n.Z, value.D);
    }
    public static explicit operator Plane(BigRatPlane value)
    {
      return new Plane((float)value.X, (float)value.Y, (float)value.Z, (float)value.D);
    }

    public BigRatPlane(BigRat x, BigRat y, BigRat z, BigRat w)
    {
      p = new BigRat[] { x, y, z, w };
    }
    public BigRatPlane(BigRatVector3 normal, BigRat dist)
    {
      this.p = new BigRat[] { normal.X, normal.Y, normal.Z, dist };
    }

    public static BigRatPlane CreateFromVertices(BigRatVector3 point1, BigRatVector3 point2, BigRatVector3 point3)
    {
      var n = BigRatVector3.Normalize(BigRatVector3.Cross(point2 - point1, point3 - point1));
      var d = BigRatVector3.Dot(n = n.Normalize(), point1).Normalize();
      return new BigRatPlane(n, -d);
    }
    public static BigRat DotCoordinate(BigRatPlane plane, BigRatVector3 value)
    {
      return plane.X * value.X + plane.Y * value.Y + plane.Z * value.Z + plane.D;
    }
    public static BigRat DotNormal(BigRatPlane plane, BigRatVector3 value)
    {
      return plane.X * value.X + plane.Y * value.Y + plane.Z * value.Z;
    }
    public static BigRatPlane Normalize(BigRatPlane plane)
    {
      var c = BigRat.Max(BigRat.Max(BigRat.Abs(plane.X), BigRat.Abs(plane.Y)), BigRat.Abs(plane.Z));
      return new BigRatPlane(plane.X / c, plane.Y / c, plane.Z / c, plane.D / c);
    }
    public static BigRatPlane Normalize(BigRatPlane plane, int digits)
    {
      var l = BigRat.Sqrt(plane.X * plane.X + plane.Y * plane.Y + plane.Z * plane.Z, digits);
      var b = new BigRatPlane(
        (plane.X / l).Normalize(), (plane.Y / l).Normalize(),
        (plane.Z / l).Normalize(), (plane.D / l).Normalize()); return b;
    }
    public static BigRatPlane Transform(BigRatPlane plane, BigRatMatrix4x3 matrix)
    {
      BigRat x = plane.X, y = plane.Y, z = plane.Z, w = plane.D; var m = BigRatMatrix4x3.Invert(matrix);
      return new BigRatPlane(
        (x * m.M11 + y * m.M12 + z * m.M13).Normalize(),
        (x * m.M21 + y * m.M22 + z * m.M23).Normalize(),
        (x * m.M31 + y * m.M32 + z * m.M33).Normalize(),
        (x * m.M41 + y * m.M42 + z * m.M43 + w).Normalize());
    }
    public static BigRatVector3 LineIntersection(BigRatPlane plane, BigRatVector3 point1, BigRatVector3 point2)
    {
      var n = new BigRatVector3(plane.X, plane.Y, plane.Z);
      var u = BigRatVector3.Dot(n, point1);
      var v = BigRatVector3.Dot(n, point2);
      var w = (u + plane.D) / (u - v);
      return (point1 + (point2 - point1) * w).Normalize();
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
      if (format != default && format.Length == 0) { format = "Q8"; provider = NumberFormatInfo.InvariantInfo; } // "" : dbg
      return $"[{X.ToString(format, provider)}; {Y.ToString(format, provider)}; {Z.ToString(format, provider)}; {D.ToString(format, provider)}]";
    }
    public string ToString(string? format)
    {
      return ToString(format, default(IFormatProvider));
    }
    public string ToString(IFormatProvider? provider)
    {
      return ToString(default(string), provider);
    }
    public override string ToString()
    {
      return ToString(null, null);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(X, Y, Z, D);
    }
    public bool Equals(BigRatPlane other)
    {
      return X == other.X && Y == other.Y && Z == other.Z && D == other.D;
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is BigRatPlane t && this.Equals(t);
    }

    public BigRatPlane Normalize()
    {
      return new BigRatPlane(X.Normalize(), Y.Normalize(), Z.Normalize(), D.Normalize());
    }

    readonly BigRat[]? p;
  }

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct BigRatMatrix4x3 : IEquatable<BigRatMatrix4x3>, IFormattable
  {
    public BigRat M11
    {
      get => p != null ? p[0] : default;
    }
    public BigRat M12
    {
      get => p != null ? p[1] : default;
    }
    public BigRat M13
    {
      get => p != null ? p[2] : default;
    }
    public BigRat M21
    {
      get => p != null ? p[3] : default;
    }
    public BigRat M22
    {
      get => p != null ? p[4] : default;
    }
    public BigRat M23
    {
      get => p != null ? p[5] : default;
    }
    public BigRat M31
    {
      get => p != null ? p[6] : default;
    }
    public BigRat M32
    {
      get => p != null ? p[7] : default;
    }
    public BigRat M33
    {
      get => p != null ? p[8] : default;
    }
    public BigRat M41
    {
      get => p != null ? p[9] : default;
    }
    public BigRat M42
    {
      get => p != null ? p[10] : default;
    }
    public BigRat M43
    {
      get => p != null ? p[11] : default;
    }

    public static BigRatMatrix4x3 Identity
    {
      get => CreateScale(BigRatVector3.RatOne);
    }

    public BigRat this[int i]
    {
      get => p != null ? p[i] : default;
    }

    public static explicit operator BigRatMatrix4x3(Matrix4x4 value)
    {
      return new BigRatMatrix4x3(
        value.M11, value.M12, value.M13,
        value.M21, value.M22, value.M23,
        value.M31, value.M32, value.M33,
        value.M41, value.M42, value.M43);
    }
    public static explicit operator Matrix4x4(BigRatMatrix4x3 value)
    {
      return new Matrix4x4(
        (float)value.M11, (float)value.M12, (float)value.M13, 0,
        (float)value.M21, (float)value.M22, (float)value.M23, 0,
        (float)value.M31, (float)value.M32, (float)value.M33, 0,
        (float)value.M41, (float)value.M42, (float)value.M43, 1);
    }

    public static BigRatMatrix4x3 CreateScale(BigRat sx, BigRat sy, BigRat sz)
    {
      var t = new BigRat[12]; t[0] = sx; t[4] = sy; t[8] = sz;
      return new BigRatMatrix4x3(t);
    }
    public static BigRatMatrix4x3 CreateScale(BigRat s)
    {
      var t = new BigRat[12]; t[0] = t[4] = t[8] = s;
      return new BigRatMatrix4x3(t);
    }
    public static BigRatMatrix4x3 CreateTranslation(BigRat x, BigRat y, BigRat z)
    {
      var m = CreateScale(1); m.p![9] = x; m.p[10] = y; m.p[11] = z; return m;
    }
    public static BigRatMatrix4x3 CreateTranslation(BigRatVector3 position)
    {
      return CreateTranslation(position.X, position.Y, position.Z);
    }
    public static BigRatMatrix4x3 CreateRotationX(BigRat radians, int digits)
    {
      var c = BigRat.Cos(radians, digits);
      var s = BigRat.Sin(radians, digits);
      var t = new BigRat[12]; t[8] = t[4] = c; t[5] = s; t[7] = -s; t[0] = BigRatVector3.RatOne;
      return new BigRatMatrix4x3(t);
    }
    public static BigRatMatrix4x3 CreateRotationY(BigRat radians, int digits)
    {
      var c = BigRat.Cos(radians, digits);
      var s = BigRat.Sin(radians, digits);
      var t = new BigRat[12]; t[0] = t[8] = c; t[2] = -s; t[6] = s; t[4] = BigRatVector3.RatOne;
      return new BigRatMatrix4x3(t);
    }
    public static BigRatMatrix4x3 CreateRotationZ(BigRat radians, int digits)
    {
      var c = BigRat.Cos(radians, digits);
      var s = BigRat.Sin(radians, digits);
      var t = new BigRat[12]; t[0] = t[4] = c; t[1] = s; t[3] = -s; t[8] = BigRatVector3.RatOne;
      return new BigRatMatrix4x3(t);
    }
    public static BigRatMatrix4x3 Invert(BigRatMatrix4x3 a)
    {
      BigRat x = a.M11, b = a.M12, c = a.M13, e = a.M21, f = a.M22, g = a.M23;
      BigRat i = a.M31, j = a.M32, k = a.M33, m = a.M41, n = a.M42, o = a.M43;
      BigRat jo_kn = j * o - k * n;
      BigRat io_km = i * o - k * m;
      BigRat in_jm = i * n - j * m;
      BigRat a11 = +(f * k - g * j);
      BigRat a12 = -(e * k - g * i);
      BigRat a13 = +(e * j - f * i);
      BigRat a14 = -(e * jo_kn - f * io_km + g * in_jm);
      BigRat det = x * a11 + b * a12 + c * a13; if (det == default) throw new ArgumentException(nameof(a));
      BigRat m11 = a11 / det;
      BigRat m21 = a12 / det;
      BigRat m31 = a13 / det;
      BigRat m41 = a14 / det;
      BigRat m12 = -(b * k - c * j) / det;
      BigRat m22 = +(x * k - c * i) / det;
      BigRat m32 = -(x * j - b * i) / det;
      BigRat m42 = +(x * jo_kn - b * io_km + c * in_jm) / det;
      BigRat fo_gn = f * o - g * n;
      BigRat eo_gm = e * o - g * m;
      BigRat en_fm = e * n - f * m;
      BigRat m13 = +(b * g - c * f) / det;
      BigRat m23 = -(x * g - c * e) / det;
      BigRat m33 = +(x * f - b * e) / det;
      BigRat m43 = -(x * fo_gn - b * eo_gm + c * en_fm) / det;
      return new BigRatMatrix4x3(
        m11.Normalize(), m12.Normalize(), m13.Normalize(), m21.Normalize(),
        m22.Normalize(), m23.Normalize(), m31.Normalize(), m32.Normalize(),
        m33.Normalize(), m41.Normalize(), m42.Normalize(), m43.Normalize());
    }

    public static BigRatMatrix4x3 operator *(BigRatMatrix4x3 a, BigRatMatrix4x3 b)
    {
      return new BigRatMatrix4x3(
        (a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31).Normalize(),
        (a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32).Normalize(),
        (a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33).Normalize(),
        (a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31).Normalize(),
        (a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32).Normalize(),
        (a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33).Normalize(),
        (a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31).Normalize(),
        (a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32).Normalize(),
        (a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33).Normalize(),
        (a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + b.M41).Normalize(),
        (a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + b.M42).Normalize(),
        (a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + b.M43).Normalize());
    }
    public static BigRatMatrix4x3 operator !(BigRatMatrix4x3 a)
    {
      return Invert(a);
    }

    public static bool operator ==(BigRatMatrix4x3 a, BigRatMatrix4x3 b)
    {
      return a.Equals(b);
    }
    public static bool operator !=(BigRatMatrix4x3 a, BigRatMatrix4x3 b)
    {
      return !a.Equals(b);
    }

    public BigRatMatrix4x3(BigRat m11, BigRat m12, BigRat m13, BigRat m21, BigRat m22, BigRat m23, BigRat m31, BigRat m32, BigRat m33, BigRat m41, BigRat m42, BigRat m43)
    {
      this.p = new BigRat[] { m11, m12, m13, m21, m22, m23, m31, m32, m33, m41, m42, m43 };
    }

    public readonly string ToString(string? format, IFormatProvider? provider)
    {
      if (format != default && format.Length == 0) { format = "Q8"; provider = NumberFormatInfo.InvariantInfo; } // "" : dbg
      var t = this; return $"[{string.Join("; ", Enumerable.Range(0, 12).Select(i => t[i].ToString(format, provider)))}]";
      //var a = new BigRatVector3(M11, M12, M13).ToString(format, provider);
      //var b = new BigRatVector3(M21, M22, M23).ToString(format, provider);
      //var c = new BigRatVector3(M31, M32, M33).ToString(format, provider);
      //var d = new BigRatVector3(M41, M42, M43).ToString(format, provider); return $"[{a} {b} {c} {d}]";
    }
    public string ToString(string? format)
    {
      return ToString(format, default(IFormatProvider));
    }
    public string ToString(IFormatProvider? provider)
    {
      return ToString(default(string), provider);
    }
    public override string ToString()
    {
      return ToString(null, null);
    }

    public BigRatMatrix4x3 Normalize()
    {
      return p != null ? new BigRatMatrix4x3(p.Select(v => v.Normalize()).ToArray()) : this;
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(
        HashCode.Combine(M11, M12, M13, M21, M22, M23),
        HashCode.Combine(M31, M32, M33, M31, M42, M43));
    }
    public bool Equals(BigRatMatrix4x3 other)
    {

      if (this.p == other.p) return true;
      if (this.p == null || other.p == null) return false;
      return this.p.SequenceEqual(other.p);
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is BigRatMatrix4x3 t && this.Equals(t);
    }

    readonly BigRat[]? p;

    BigRatMatrix4x3(BigRat[] p)
    {
      Debug.Assert(p.Length == 12); this.p = p;
    }
  }

}