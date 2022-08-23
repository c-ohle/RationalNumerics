
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Numerics.Rational
{
  /// <summary>
  /// A Complex number class based on <see cref="BigRational"/>.<br/>
  /// <i>This is just a non-optimal example implementation for testing!</i>
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct ComplexR : IEquatable<ComplexR>, IFormattable //, ISpanFormattable
  {
    public readonly BigRational Real, Imaginary;
    public ComplexR(BigRational real, BigRational imaginary)
    {
      Real = real; Imaginary = imaginary;
    }
    public readonly override string ToString()
    {
      return $"{Real} + {Imaginary}i";
    }
    public readonly string ToString(string? format, IFormatProvider? provider = null)
    {
      return $"{Real.ToString(format, provider)} + {Imaginary.ToString(format, provider)}i";
    }
    public override int GetHashCode()
    {
      return HashCode.Combine(Real, Imaginary); 
    }
    public readonly bool Equals(ComplexR other)
    {
      return Real.Equals(other.Real) && Imaginary.Equals(other.Imaginary);
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is ComplexR t && Equals(t);
    }
    public static implicit operator ComplexR(Complex a)
    {
      return new ComplexR(a.Real, a.Imaginary);
    }
    public static explicit operator Complex(ComplexR a)
    {
      return new Complex((float)a.Real, (float)a.Imaginary);
    }
    public static bool operator ==(ComplexR a, ComplexR b)
    {
      return a.Equals(b);
    }
    public static bool operator !=(ComplexR a, ComplexR b)
    {
      return !a.Equals(b);
    }
    public static ComplexR operator +(ComplexR a, ComplexR b)
    {
      return new ComplexR(a.Real + b.Real, a.Imaginary + b.Imaginary);
    }
    public static ComplexR operator -(ComplexR a, ComplexR b)
    {
      return new ComplexR(a.Real - b.Real, a.Imaginary - b.Imaginary);
    }
    public static ComplexR operator *(ComplexR a, ComplexR b)
    {
      //todo: optimize
      return new ComplexR(
         (a.Real * b.Real) - (a.Imaginary * b.Imaginary),
         (a.Imaginary * b.Real) + (a.Real * b.Imaginary));
    }
    public static ComplexR operator /(ComplexR a, ComplexR b)
    {
      //todo: optimize
      if (rat.Abs(b.Imaginary) < rat.Abs(b.Real))
      {
        var doc = b.Imaginary / b.Real;
        return new ComplexR(
          (a.Real + a.Imaginary * doc) / (b.Real + b.Imaginary * doc),
          (a.Imaginary - a.Real * doc) / (b.Real + b.Imaginary * doc));
      }
      else
      {
        var cod = b.Real / b.Imaginary;
        return new ComplexR(
          (a.Imaginary + a.Real * cod) / (b.Imaginary + b.Real * cod),
          (-a.Real + a.Imaginary * cod) / (b.Imaginary + b.Real * cod));
      }
    }
  }
}
