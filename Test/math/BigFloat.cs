using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms.Design.Behavior;
using Test;

namespace System.Numerics.Rational
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
  public readonly partial struct BigFloat : IComparable, IComparable<BigFloat>, IEquatable<BigFloat>, IFormattable
  {
    public override readonly string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? formatProvider) => p.ToString(format, formatProvider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(BigFloat other) => p.Equals(other.p);
    public readonly int CompareTo(BigFloat other) => p.CompareTo(other.p);
    public readonly int CompareTo(object? obj) => obj == null ? 1 : obj is BigFloat v ? CompareTo(v) : throw new ArgumentException();

    private readonly BigRational p;
    private BigFloat(BigRational p) => this.p = p;
    private BigFloat(BigRational.SafeCPU cpu) => p = cpu.popr();
  }
#if NET7_0
#endif
}
