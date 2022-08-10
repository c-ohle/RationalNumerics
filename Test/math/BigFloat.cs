using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;


namespace System.Numerics
{
  /// <summary>
  /// Represents an arbitrarily large floating-point number. (under construction)
  /// </summary>
  /// <remarks>
  /// Allows emulation of IEEE 754 types <seealso href="https://en.wikipedia.org/wiki/IEEE_754"/> with bit exact data representation.<br/>
  /// This implementation using BigRational.SafeCPU as calculation core is intended for<br/>
  /// testing and benchmarks to other custom floating-point number types in real scenarios.<br/>
  /// <i>boost operator public just to test how much performance would be possible.</i><br/>
  /// </remarks>
  [Serializable, SkipLocalsInit] //, DebuggerDisplay("{ToString(),nq})"
  public readonly unsafe partial struct BigFloat : IComparable, IComparable<BigFloat>, IEquatable<BigFloat> // , IFormattable
  {
    public override readonly string ToString() => ((BigRational)this).ToString("L42");
    public override readonly int GetHashCode() => p.GetHashCode() ^ e;
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is BigFloat r && Equals(r);
    }
    public readonly bool Equals(BigFloat other) => e == other.e && p.Equals(other.p);
    public readonly int CompareTo(object? obj) => obj == null ? 1 : obj is BigFloat v ? CompareTo(v) : throw new ArgumentException();
    public readonly int CompareTo(BigFloat other)
    {
      var s = this.p.Sign; var t = s - other.p.Sign; if (t != 0) return t > 0 ? 1 : -1;
      if (this.e != other.e) return e > other.e ? s : -s;
      return p.CompareTo(other.p);
    }

    public BigFloat(int v)
    {
      if (v == 0) { this = default; return; }
      this = new BigFloat(v, 0);
    }
    public BigFloat(double v)
    {
      if (v == 0) { this = default; return; }
      long m; int h = ((int*)&v)[1], e = ((h >> 20) & 0x7FF) - 1075;
      ((uint*)&m)[0] = *(uint*)&v;
      ((uint*)&m)[1] = (unchecked((uint)h) & 0x000FFFFF) | 0x100000;
      this = new BigFloat(v < 0 ? -m : m, e);
    }

    public static implicit operator BigFloat(int value) => new BigFloat(value);
    public static implicit operator BigFloat(long value) => (BigInt)value;
    public static implicit operator BigFloat(double value) => new BigFloat(value);
    public static implicit operator BigFloat(decimal value) => (BigRational)value;
    public static implicit operator BigFloat(BigInt value)
    {
      return new BigFloat(value, 0);
    }
    public static implicit operator BigFloat(BigRational value)
    {
      var num = BigRational.NumDen(value, out var den);
      return (BigFloat)(BigInt)num / (BigFloat)(BigInt)den;
    }

    public static implicit operator BigRational(BigFloat v) => v.p * BigRational.Pow(2, v.e);

    public static BigFloat operator +(BigFloat a) => a;
    public static BigFloat operator -(BigFloat a) => new BigFloat(-a.p, a.e);
    public static BigFloat operator +(BigFloat a, BigFloat b)
    {
      var u = a.p; var v = b.p; int e;
      if (a.e == b.e) e = a.e;
      else if (a.e > b.e) v = BigInt.Shr(v, (e = a.e) - b.e);
      else u = BigInt.Shr(u, (e = b.e) - a.e);
      return new BigFloat(u + v, e);
    }
    public static BigFloat operator -(BigFloat a, BigFloat b)
    {
      return a + -b;
    }
    public static BigFloat operator *(BigFloat a, BigFloat b)
    {
      return new BigFloat(a.p * b.p, a.e + b.e);
    }
    public static BigFloat operator /(BigFloat a, BigFloat b)
    {
      return new BigFloat(BigInt.Shl(a.p, mant) / b.p, a.e - b.e - mant);
    }

    public static bool operator <(BigFloat a, BigFloat b) => a.CompareTo(b) < 0;
    public static bool operator <=(BigFloat a, BigFloat b) => a.CompareTo(b) <= 0;
    public static bool operator >(BigFloat a, BigFloat b) => a.CompareTo(b) > 0;
    public static bool operator >=(BigFloat a, BigFloat b) => a.CompareTo(b) >= 0;
    public static bool operator ==(BigFloat a, BigFloat b) => a.Equals(b);
    public static bool operator !=(BigFloat a, BigFloat b) => !a.Equals(b);

    public static bool operator <(BigFloat a, long b) => a.CompareTo(b) < 0;
    public static bool operator <=(BigFloat a, long b) => a.CompareTo(b) <= 0;
    public static bool operator >(BigFloat a, long b) => a.CompareTo(b) > 0;
    public static bool operator >=(BigFloat a, long b) => a.CompareTo(b) >= 0;
    public static bool operator ==(BigFloat a, long b) => a.Equals(b);
    public static bool operator !=(BigFloat a, long b) => !a.Equals(b);

    public static BigFloat operator |(BigFloat? a, BigFloat b) => new BigFloat(a.GetValueOrDefault().p | b.p, b.e);
    public static implicit operator BigFloat?(int value) => new BigFloat(((BigInt?)value).GetValueOrDefault(), 0);

    public static BigFloat Sin(BigFloat angle) => (BigFloat)BigRational.Sin(angle, 42);
    public static BigFloat Cos(BigFloat angle) => (BigFloat)BigRational.Cos(angle, 42);
    public static BigFloat Tan(BigFloat angle) => (BigFloat)BigRational.Tan(angle, 42);
    public static BigFloat Pi() => (BigFloat)BigRational.Pi(42);

    const int mant = 128; // 53;
    private readonly BigInt p; readonly int e;
    private BigFloat(BigInt p, int e)
    {
      var s = BigInt.Msb(p);
      if (s == 0) { this = default; return; }
      if (s == mant) { this.p = p; this.e = e; return; }
      var d = s - mant;
      this.p = d >= 0 ? BigInt.Shr(p, d) : BigInt.Shl(p, -d);
      this.e = e + d; Debug.Assert(BigInt.Msb(this.p) == mant);
    }

  }

#if NET7_0
  unsafe partial struct BigFloat
  {
    //static (long m, int e) getme(double d)
    //{
    //  var p = (IFloatingPoint<double>)d; var x = p.GetSignificandBitLength();
    //  int e; p.WriteExponentLittleEndian(new Span<byte>(&e, 4));
    //  long m; p.WriteSignificandLittleEndian(new Span<byte>(&m, 8));
    //  return (m, e);
    //}
  }
#endif
}
