using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Numerics.Rational;

namespace Test
{
  /// <summary>
  /// Usual implementation of a rational number class based on <see cref="System.Numerics.BigInteger"/>.<br/>
  /// </summary>
  /// <remarks>
  /// <i>This class is only intended for speed comparisons and benchmark tests with <see cref="BigRational"/>.</i>
  /// </remarks>
  [DebuggerDisplay("{ToString(\"\"),nq}")]
  public struct OldRational : IEquatable<OldRational>, IComparable<OldRational>, IFormattable
  {
    BigInteger num, den;
    public int Sign => num.Sign;
    /// <summary>
    /// Simply mapped to <see cref="BigRational.ToString"/> as it is not part of the speed comparisons.
    /// </summary>
    public override string ToString()
    {
      return ToString(null, null);
    }
    /// <summary>
    /// Simply mapped to <see cref="BigRational.ToString(string?, IFormatProvider?)"/> as it is not part of the speed comparisons.
    /// </summary>
    public string ToString(string? format, IFormatProvider? provider = default)
    {
      if (den.IsZero) return NumberFormatInfo.GetInstance(provider).NaNSymbol;
      return ((BigRational)this).ToString(format, provider);
    }
    public override int GetHashCode()
    {
      return num.GetHashCode() + den.GetHashCode() * 13;
    }
    public override bool Equals(object? b)
    {
      return b is OldRational n ? Equals(n) : false;
    }
    public int CompareTo(OldRational b)
    {
      var s1 = num.Sign;
      if (s1 != b.num.Sign) return s1 > b.num.Sign ? +1 : -1;
      if (s1 == 0) return 0;
      var den = this.den; if (den.Sign < 0) den = -den;
      if (b.den.Sign < 0) b.den = -b.den;
      var s2 = num.CompareTo(b.num) * s1;
      var s3 = den.CompareTo(b.den);
      if (s3 == 0) return +s2 * s1;
      if (s2 == 0) return -s3 * s1;
      if (s2 > 0 && s3 < 0) return +s1;
      if (s3 < 0 && s2 > 0) return -s1;
      return (num * b.den).CompareTo(b.num * den);
    }
    public bool Equals(OldRational b)
    {
      return num.Equals(b.num) && den.Equals(b.den);
    }
    public static implicit operator OldRational(int v)
    {
      return new OldRational { num = v, den = 1 };
    }
    public static implicit operator OldRational(long v)
    {
      return new OldRational { num = v, den = 1 };
    }
    public static implicit operator OldRational(ulong v)
    {
      return new OldRational { num = v, den = 1 };
    }
    public static implicit operator BigRational(OldRational v)
    {
      return (BigRational)v.num / (BigRational)v.den;
    }
    public static explicit operator OldRational(BigRational v)
    {
      var cpu = BigRational.task_cpu; cpu.push(v); cpu.mod(8);
      if (cpu.sign() < 0) { cpu.neg(0); cpu.neg(1); }
      return new OldRational { den = (BigInteger)cpu.pop_rat(), num = (BigInteger)cpu.pop_rat() };
    }
    public static OldRational operator +(OldRational a)
    {
      return a;
    }
    public static OldRational operator -(OldRational a)
    {
      a.num = -a.num; return a;
    }
    public static OldRational operator +(OldRational a, OldRational b)
    {
      a.num = a.num * b.den + a.den * b.num;
      a.den = a.den * b.den;
      a.normalize(); return a;
    }
    public static OldRational operator -(OldRational a, OldRational b)
    {
      a.num = a.num * b.den - a.den * b.num;
      a.den = a.den * b.den;
      a.normalize(); return a;
    }
    public static OldRational operator *(OldRational a, OldRational b)
    {
      a.num *= b.num;
      a.den *= b.den;
      a.normalize(); return a;
    }
    public static OldRational operator /(OldRational a, OldRational b)
    {
      if (b.num.IsZero) throw new DivideByZeroException();
      a.num *= b.den;
      a.den *= b.num;
      a.normalize(); return a;
    }
    public static bool operator ==(OldRational a, OldRational b)
    {
      return a.Equals(b);
    }
    public static bool operator !=(OldRational a, OldRational b)
    {
      return !a.Equals(b);
    }
    public static bool operator <=(OldRational a, OldRational b)
    {
      return a.CompareTo(b) <= 0;
    }
    public static bool operator >=(OldRational a, OldRational b)
    {
      return a.CompareTo(b) >= 0;
    }
    public static bool operator <(OldRational a, OldRational b)
    {
      return a.CompareTo(b) < 0;
    }
    public static bool operator >(OldRational a, OldRational b)
    {
      return a.CompareTo(b) > 0;
    }
    public static OldRational Abs(OldRational a)
    {
      return a.Sign < 0 ? -a : a;
    }
    public static OldRational Min(OldRational a, OldRational b)
    {
      return a < b ? a : b;
    }
    public static OldRational Max(OldRational a, OldRational b)
    {
      return a > b ? a : b;
    }
    public static OldRational Pow(OldRational a, int b)
    {
      OldRational result = 1;
      for (var e = unchecked((uint)(b < 0 ? -b : b)); ; e >>= 1)
      {
        if ((e & 1) != 0) result *= a;
        if (e <= 1) break; a *= a;
      }
      if (b < 0) result = 1 / result;
      return result;
    }
    public static OldRational Round(OldRational a, int digits)
    {
      var e = Pow(10, digits); var b = a * e;
      var div = BigInteger.DivRem(BigInteger.Abs(b.num), b.den, out var rem);
      if (rem > (b.den >> 1)) div += 1;
      var result = new OldRational { num = b.Sign >= 0 ? div : -div, den = 1 } / e;
      return result;
    }
    void normalize()
    {
      if (den < 0) { num = -num; den = -den; }
      var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(num), den);
      if (gcd != 1) { num /= gcd; den /= gcd; }
    }
  }
}
