using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NewNumeric
{
  // Classes for testing BigRat in practice, these are not optimal implementations

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct BigRatVector : IEnumerable<BigRat>, IEnumerable, IEquatable<BigRatVector>
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

    public BigRatVector(IEnumerable<BigRat> values)
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
    public BigRatVector(params BigRat[] values)
    {
      this.p = new BigRatVector(values.AsEnumerable<BigRat>()).p;
    }

    public override int GetHashCode()
    {
      var hash = new HashCode(); hash.AddBytes(MemoryMarshal.Cast<uint, byte>(this.p));
      return hash.ToHashCode();
    }
    public bool Equals(BigRatVector other)
    {
      return this.p == other.p || this.p != null && other.p != null &&
        this.p.AsSpan().SequenceEqual(other.p.AsSpan());
      //if (this.p == other.p) return true;
      //if (this.p == null || other.p == null || this.Length != other.Length) return false;
      //for (int i = 0, n = this.Length; i < n; i++) if (this[i] != other[i]) return false;
      //return true;
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is BigRatVector v && this.Equals(v);
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
      if (format == string.Empty && provider == null) provider = NumberFormatInfo.InvariantInfo;
      return "[" + string.Join("; ", this.Select(p => p.ToString(format, provider))) + ']';
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

    readonly uint[]? p;

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct BigRatVector2 : IEquatable<BigRatVector2>
  {
    public BigRat X
    {
      get => p != null ? p[0] : default;
    }
    public BigRat Y
    {
      get => p != null ? p[1] : default;
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

    public static BigRatVector2 operator +(BigRatVector2 a) => a;
    public static BigRatVector2 operator -(BigRatVector2 a) => new BigRatVector2(-a.X, -a.Y);
    public static BigRatVector2 operator +(BigRatVector2 a, BigRatVector2 b)
    {
      return new BigRatVector2(a.X + b.X, a.Y + b.Y);
    }
    public static BigRatVector2 operator -(BigRatVector2 a, BigRatVector2 b)
    {
      return new BigRatVector2(a.Y - b.X, a.Y - b.Y);
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
    public static BigRat Dot(BigRatVector2 a, BigRatVector2 b)
    {
      return a.X * b.X + a.Y * b.Y;
    }
    public static BigRat Cross(BigRatVector2 a, BigRatVector2 b)
    {
      return a.X * b.Y - a.Y * b.X;
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
      var b = Abs(a); var c = BigRat.Max(b.X, b.Y); return a / c;
    }
    public static BigRatVector2 Round(BigRatVector2 a, int digits)
    {
      return new BigRatVector2(BigRat.Round(a.X, digits), BigRat.Round(a.Y, digits));
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
      if (format == string.Empty && provider == null) provider = NumberFormatInfo.InvariantInfo;
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

    public BigRatVector2 Simplify()
    {
      return new BigRatVector2(X.Normalize(), Y.Normalize());
    }

    readonly BigRat[]? p;
  }

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct BigRatVector3 : IEquatable<BigRatVector3>
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

    public static BigRatVector3 operator +(BigRatVector3 a) => a;
    public static BigRatVector3 operator -(BigRatVector3 a) => new BigRatVector3(-a.X, -a.Y, -a.Z);
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
    public static BigRat Dot(BigRatVector3 a, BigRatVector3 b)
    {
      return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    public static BigRatVector3 Cross(BigRatVector3 a, BigRatVector3 b)
    {
      return new BigRatVector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
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
    public static BigRatVector3 Round(BigRatVector3 a, int digits)
    {
      return new BigRatVector3(BigRat.Round(a.X, digits), BigRat.Round(a.Y, digits), BigRat.Round(a.Z, digits));
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
      if (format == string.Empty && provider == null) provider = NumberFormatInfo.InvariantInfo;
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
      return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
    }
    public bool Equals(BigRatVector3 other)
    {
      return X == other.X && Y == other.Y && Z == other.Z;
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is BigRatVector3 t && this.Equals(t);
    }

    public BigRatVector3 Simplify()
    {
      return new BigRatVector3(X.Normalize(), Y.Normalize(), Z.Normalize());
    }

    readonly BigRat[]? p;
  }

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct BigRatMatrix4x3 //: IEquatable<BigRatVector3>
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
      get => CreateScale(1);
    }

    public static BigRatMatrix4x3 CreateScale(BigRat s)
    {
      return new BigRatMatrix4x3(s, 0, 0, 0, s, 0, 0, 0, s, 0, 0, 0 );
    }
    public static BigRatMatrix4x3 CreateTranslation(BigRat x, BigRat y, BigRat z)
    {
      var m = CreateScale(1); m.p![9] = z; m.p![10] = z; m.p![11] = z; return m;
    }
    public static BigRatMatrix4x3 CreateTranslation(BigRatVector3 position)
    {                               
      return CreateTranslation(position.X, position.Y, position.Z);
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

    public BigRatMatrix4x3(BigRat m11, BigRat m12, BigRat m13, BigRat m21, BigRat m22, BigRat m23, BigRat m31, BigRat m32, BigRat m33, BigRat m41, BigRat m42, BigRat m43)
    {
      this.p = new BigRat[] { m11, m12, m13, m21, m22, m23, m31, m32, m33, m41, m42, m43 };
    }

    readonly BigRat[]? p;
  }

}