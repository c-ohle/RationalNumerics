
namespace System.Numerics.Rational
{
  /// <summary>
  /// A Vector2 class based on <see cref="rat"/>.<br/>
  /// <i>This is just a non-optimal example implementation for testing!</i>
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Vector2R : IEquatable<Vector2R>, IFormattable, ISpanFormattable
  {
    public readonly rat X, Y;
    public Vector2R(rat x, rat y)
    {
      X = x; Y = y;
    }
    public override string ToString()
    {
      return $"{X}; {Y}";
    }
    public readonly string ToString(string? format, IFormatProvider? provider = null)
    {
      return $"{X.ToString(format, provider)}; {Y.ToString(format, provider)}";
    }
    public readonly bool TryFormat(Span<char> sw, out int nw, ReadOnlySpan<char> fmt, IFormatProvider? fp)
    {
      int n; nw = 0;
      X.TryFormat(sw.Slice(nw), out n, fmt, fp); nw += n; sw[nw++] = ' ';
      Y.TryFormat(sw.Slice(nw), out n, fmt, fp); nw += n;
      return true;
    }
    public static Vector2R Parse(ref ReadOnlySpan<char> s)
    {
      return new Vector2R(rat.Parse(s.token()), rat.Parse(s.token()));
    }
    public readonly void WriteToBytes(ref Span<byte> sp)
    {
      X.WriteToBytes(ref sp); X.WriteToBytes(ref sp);
    }
    public static Vector2R ReadFromBytes(ref ReadOnlySpan<byte> rs)
    {
      return new Vector2R(rat.ReadFromBytes(ref rs), rat.ReadFromBytes(ref rs));
    }
    public override int GetHashCode()
    {
      return HashCode.Combine(X, Y);
    }
    public override bool Equals(object? p)
    {
      return p is Vector2R v && Equals(v);
    }
    public readonly bool Equals(Vector2R b)
    {
      return X.Equals(b.X) && Y.Equals(b.Y);
    }
    public static explicit operator Vector2(Vector2R v)
    {
      return new Vector2((float)v.X, (float)v.Y);
    }
    public static implicit operator Vector2R(Vector2 v)
    {
      return new Vector2R(v.X, v.Y);
    }
    public static bool operator ==(Vector2R a, Vector2R b)
    {
      return a.Equals(b);
    }
    public static bool operator !=(Vector2R a, Vector2R b)
    {
      return !a.Equals(b);
    }
    public static Vector2R operator +(Vector2R a) => a;
    public static Vector2R operator -(Vector2R a) => new Vector2R(-a.X, -a.Y);
    public static Vector2R operator +(Vector2R a, Vector2R b)
    {
      return new Vector2R(a.X + b.X, a.Y + b.Y);
    }
    public static Vector2R operator -(Vector2R a, Vector2R b)
    {
      return new Vector2R(a.Y - b.X, a.Y - b.Y);
    }
    public static Vector2R operator *(Vector2R a, rat b)
    {
      return new Vector2R(a.X * b, a.Y * b);
    }
    public static Vector2R operator *(Vector2R a, Vector2R b)
    {
      return new Vector2R(a.X * b.X, a.Y * b.Y);

    }
    public static rat Dot(in Vector2R a, in Vector2R b)
    {
      //return a.X * b.X + a.Y * b.Y;
      var cpu = rat.task_cpu; cpu.dot(a, b); return cpu.pop_rat();
    }
    public static rat Cross(in Vector2R a, in Vector2R b)
    {
      //return a.X * b.Y - a.Y * b.X;
      var cpu = rat.task_cpu;
      cpu.mul(a.X, b.Y); cpu.mul(a.Y, b.X); cpu.sub();
      return cpu.pop_rat();
    }
    public static rat LengthSq(in Vector2R a)
    {
      var cpu = rat.task_cpu; cpu.dot(a, a); return cpu.pop_rat();
    }
    public static double Length(in Vector2R a)
    {
      var cpu = rat.task_cpu; cpu.dot(a, a); return Math.Sqrt(cpu.pop_dbl());
    }
    public static Vector2R Min(in Vector2R a, in Vector2R b)
    {
      return new Vector2R(
        rat.Min(a.X, b.X),
        rat.Min(a.Y, b.Y));
    }
    public static Vector2R Max(in Vector2R a, in Vector2R b)
    {
      return new Vector2R(
        rat.Max(a.X, b.X),
        rat.Max(a.Y, b.Y));
    }
  }
}
