
namespace System.Numerics.Rational
{
  /// <summary>
  /// A Vector2 class based on <see cref="BigRational"/>.<br/>
  /// <i>This is just a non-optimal example implementation for testing!</i>
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Vector2R : IEquatable<Vector2R>, IFormattable, ISpanFormattable
  {
    public readonly BigRational X, Y;
    public Vector2R(BigRational x, BigRational y)
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
      return new Vector2R(BigRational.Parse(s.token()), BigRational.Parse(s.token()));
    }
    public readonly void WriteToBytes(ref Span<byte> sp)
    {
      X.WriteToBytes(ref sp); X.WriteToBytes(ref sp);
    }
    public static Vector2R ReadFromBytes(ref ReadOnlySpan<byte> rs)
    {
      return new Vector2R(BigRational.ReadFromBytes(ref rs), BigRational.ReadFromBytes(ref rs));
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
    public static Vector2R operator *(Vector2R a, BigRational b)
    {
      return new Vector2R(a.X * b, a.Y * b);
    }
    public static Vector2R operator *(Vector2R a, Vector2R b)
    {
      return new Vector2R(a.X * b.X, a.Y * b.Y);

    }
    public static BigRational Dot(Vector2R a, Vector2R b)
    {
      //return a.X * b.X + a.Y * b.Y;
      var cpu = BigRational.task_cpu; cpu.dot(a, b); return cpu.popr();
    }
    public static BigRational Cross(Vector2R a, Vector2R b)
    {
      //return a.X * b.Y - a.Y * b.X;
      var cpu = BigRational.task_cpu;
      cpu.mul(a.X, b.Y); cpu.mul(a.Y, b.X); cpu.sub();
      return cpu.popr();
    }
    public static BigRational LengthSq(Vector2R a)
    {
      var cpu = BigRational.task_cpu; cpu.dot(a, a); return cpu.popr();
    }
    public static double Length(Vector2R a)
    {
      var cpu = BigRational.task_cpu; cpu.dot(a, a); return Math.Sqrt(cpu.popd());
    }
    /// <summary>
    /// Returns the length of the vector with the desired precision of decimal digits.
    /// </summary>
    /// <param name="a">A <see cref="Vector2R"/></param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// With default value (digits = 0) the current value of <see cref="rat.DefaultDigits"/> is used.
    /// </param>
    /// <returns>A length as <see cref="BigRational"/> number.</returns>
    public static BigRational LengthR(Vector2R a, int digits = 0)
    {
      //return rat.Sqrt(a.X * a.X + a.Y * a.Y, digits);      
      if (digits == 0) digits = rat.DefaultDigits;
      var cpu = rat.task_cpu;
      cpu.pow(10, digits); var c = cpu.msb(); cpu.pop(); //todo: build in      
      cpu.push(a.X); cpu.sqr();
      cpu.push(a.Y); cpu.sqr(); cpu.add();
      cpu.sqrt(c); cpu.rnd(digits);
      return cpu.popr();
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
    /// <summary>
    /// Returns the sine and cosine of the specified angle
    /// as <see cref="Vector2R"/> with an garantized length of 1. 
    /// </summary>
    /// <remarks>
    /// For the non-rational sine-cosine pairs, the closest possible rational pair 
    /// that satisfies the condition x2 + y2 == 1 is calculated.<br/>
    /// The precision refers to the accuracy of the pair's angle, 
    /// which of course will then differ from the input.<br/> 
    /// This function is useful, for example, for transformations without length distortions.<br/>
    /// <br/><b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits is not yet guaranteed!
    /// </remarks>
    /// <param name="a">An angle, measured in radians.</param>
    /// <param name="prec">A precesission in a range 10..52.</param>
    /// <returns>
    /// A <see cref="Vector2R"/> where the sine is returned in Y and the cosine in X.
    /// </returns>
    public unsafe static Vector2R SinCosR(double a, int prec = 10) //10..52 
    { 
      var co = Math.Cos(a); if (Math.Abs(co) == 1) return new Vector2(Math.Sign(co), 0);
      var si = Math.Sin(a); if (Math.Abs(si) == 1) return new Vector2(0, Math.Sign(si));
      var dm = si / (1 - co); *(ulong*)&dm &= 0xffffffffffffffff << (52 - prec); //todo: prec, extend and spec 
      //var m1 = (BigRational)dm; var m2 = m1 * m1; var m3 = m2 + 1;
      //return new Vector2R((m2 - 1) / m3, 2 * m1 / m3); // x * x + y * y == 1
      var cpu = rat.task_cpu;
      cpu.push(dm); cpu.dup(); cpu.sqr(); cpu.dup(); cpu.push(1u); cpu.add(); // m1, m2, m3
      cpu.push(1u); cpu.sub(2, 0); cpu.pop(); cpu.div(1, 0); // (m2 - 1) / m3
      cpu.shl(1, 2); cpu.div(2, 0); cpu.pop(); // 2 * m1 / m3
      return new Vector2R(cpu.popr(), cpu.popr());
    }
  }
}
