using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#pragma warning disable CS1591 //todo: xml comments

namespace System.Numerics
{
  /// <summary>
  /// Represents a 128-bit quadruple-precision floating-point number. <b>(under construction)</b>
  /// </summary>
  /// <remarks>
  /// Also known as C/C++ (GCC) <c>__float128</c><br/> 
  /// The data format and properties as defined in <seealso href="https://en.wikipedia.org/wiki/IEEE_754">IEEE 754</seealso>.<br/>
  /// </remarks>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float128 : IComparable<Float128>, IEquatable<Float128>, IComparable, ISpanFormattable
#if NET7_0
    , BigRational.Float<Float128.UInt128>.IFloat<Float128>
#endif  
  {
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float128 b) => BigRational.Float<UInt128>.Equals(this.p, b.p);
    public readonly int CompareTo(Float128 b) => BigRational.Float<UInt128>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float128 b ? BigRational.Float<UInt128>.Compare(this.p, b.p) : throw new ArgumentException();

    public static implicit operator Float128(int value) => new Float128((BigRational.Float<UInt128>)value);
    public static implicit operator Float128(Half value) => new Float128((BigRational.Float<UInt128>)value);
    public static implicit operator Float128(float value) => new Float128((BigRational.Float<UInt128>)value);
    public static implicit operator Float128(double value) => new Float128((BigRational.Float<UInt128>)value);
    public static implicit operator Float128(decimal value) => new Float128((BigRational.Float<UInt128>)value);
    public static explicit operator Float128(BigRational value) => new Float128((BigRational.Float<UInt128>)value);

    public static explicit operator int(Float128 value) => (int)value.p;
    public static explicit operator Half(Float128 value) => (Half)value.p;
    public static explicit operator float(Float128 value) => (float)value.p;
    public static explicit operator double(Float128 value) => (double)value.p;
    public static explicit operator decimal(Float128 value) => (decimal)value.p;
    public static implicit operator BigRational(Float128 value) => (BigRational)value.p;

    public static Float128 operator ++(Float128 a) => new Float128(a.p + +1);
    public static Float128 operator --(Float128 a) => new Float128(a.p + -1);
    public static Float128 operator +(Float128 a, Float128 b) => new Float128(a.p + b.p);
    public static Float128 operator -(Float128 a, Float128 b) => new Float128(a.p - b.p);
    public static Float128 operator *(Float128 a, Float128 b) => new Float128(a.p * b.p);
    public static Float128 operator /(Float128 a, Float128 b) => new Float128(a.p / b.p);
    public static Float128 operator %(Float128 a, Float128 b) => new Float128(a.p % b.p);
    public static bool operator ==(Float128 a, Float128 b) => a.p == b.p;
    public static bool operator !=(Float128 a, Float128 b) => a.p != b.p;
    public static bool operator <=(Float128 a, Float128 b) => a.p <= b.p;
    public static bool operator >=(Float128 a, Float128 b) => a.p >= b.p;
    public static bool operator <(Float128 a, Float128 b) => a.p < b.p;
    public static bool operator >(Float128 a, Float128 b) => a.p > b.p;

    public static Float128 Round(Float128 a, int digits) => new Float128(BigRational.Float<UInt128>.Round(a.p, digits));
    public static Float128 Truncate(Float128 a) => new Float128(BigRational.Float<UInt128>.Truncate(a.p));

    public static Float128 Pi => new Float128(BigRational.Float<UInt128>.Pi);
    public static Float128 Tau => new Float128(BigRational.Float<UInt128>.Tau);
    public static Float128 E => new Float128(BigRational.Float<UInt128>.E);
    public static Float128 MinValue => new Float128(BigRational.Float<UInt128>.MinValue);
    public static Float128 MaxValue => new Float128(BigRational.Float<UInt128>.MaxValue);

    public static Float128 Cast<T>(BigRational.Float<T> a) where T : unmanaged => new Float128(BigRational.Float<T>.Cast<UInt128>(a));
    public static BigRational.Float<T> Cast<T>(Float128 a) where T : unmanaged => BigRational.Float<UInt128>.Cast<T>(a.p);
    Float128(BigRational.Float<UInt128> p) => this.p = p;
    private readonly BigRational.Float<UInt128> p;
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    private readonly struct UInt128 { } // for NET 6 compat
  }

  /// <summary>
  /// Represents a 256-bit octuple-precision floating-point number. <b>(under construction)</b>
  /// </summary>
  /// <remarks>
  /// The data format and properties as defined in <seealso href="https://en.wikipedia.org/wiki/IEEE_754">IEEE 754</seealso>.
  /// </remarks>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float256 : IComparable<Float256>, IEquatable<Float256>, IComparable, ISpanFormattable
#if NET7_0
    , BigRational.Float<Float256.UInt256>.IFloat<Float256>
#endif
  {
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float256 b) => BigRational.Float<UInt256>.Equals(this.p, b.p);
    public readonly int CompareTo(Float256 b) => BigRational.Float<UInt256>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float256 b ? BigRational.Float<UInt256>.Compare(this.p, b.p) : throw new ArgumentException();

    public static implicit operator Float256(int value) => new Float256((BigRational.Float<UInt256>)value);
    public static implicit operator Float256(Half value) => new Float256((BigRational.Float<UInt256>)value);
    public static implicit operator Float256(float value) => new Float256((BigRational.Float<UInt256>)value);
    public static implicit operator Float256(double value) => new Float256((BigRational.Float<UInt256>)value);
    public static implicit operator Float256(decimal value) => new Float256((BigRational.Float<UInt256>)value);
    public static implicit operator Float256(Float128 value) => new Float256(Float128.Cast<UInt256>(value));
    public static explicit operator Float256(BigRational value) => new Float256((BigRational.Float<UInt256>)value);

    public static explicit operator int(Float256 value) => (int)value.p;
    public static explicit operator Half(Float256 value) => (Half)value.p;
    public static explicit operator float(Float256 value) => (float)value.p;
    public static explicit operator double(Float256 value) => (double)value.p;
    public static explicit operator decimal(Float256 value) => (decimal)value.p;
    public static explicit operator Float128(Float256 value) => Float128.Cast(value.p);
    public static implicit operator BigRational(Float256 value) => (BigRational)value.p;

    public static Float256 operator ++(Float256 a) => new Float256(a.p + +1);
    public static Float256 operator --(Float256 a) => new Float256(a.p + -1);
    public static Float256 operator +(Float256 a, Float256 b) => new Float256(a.p + b.p);
    public static Float256 operator -(Float256 a, Float256 b) => new Float256(a.p - b.p);
    public static Float256 operator *(Float256 a, Float256 b) => new Float256(a.p * b.p);
    public static Float256 operator /(Float256 a, Float256 b) => new Float256(a.p / b.p);
    public static Float256 operator %(Float256 a, Float256 b) => new Float256(a.p % b.p);
    public static bool operator ==(Float256 a, Float256 b) => a.p == b.p;
    public static bool operator !=(Float256 a, Float256 b) => a.p != b.p;
    public static bool operator <=(Float256 a, Float256 b) => a.p <= b.p;
    public static bool operator >=(Float256 a, Float256 b) => a.p >= b.p;
    public static bool operator <(Float256 a, Float256 b) => a.p < b.p;
    public static bool operator >(Float256 a, Float256 b) => a.p > b.p;

    public static Float256 Round(Float256 a, int digits) => new Float256(BigRational.Float<UInt256>.Round(a.p, digits));
    public static Float256 Truncate(Float256 a) => new Float256(BigRational.Float<UInt256>.Truncate(a.p));
    public static Float256 Cast<T>(BigRational.Float<T> a) where T : unmanaged => new Float256(BigRational.Float<T>.Cast<UInt256>(a));
    public static BigRational.Float<T> Cast<T>(Float256 a) where T : unmanaged => BigRational.Float<UInt256>.Cast<T>(a.p);

    public static Float256 Pi => new Float256(BigRational.Float<UInt256>.Pi);
    public static Float256 Tau => new Float256(BigRational.Float<UInt256>.Pi);
    public static Float256 E => new Float256(BigRational.Float<UInt256>.E);
    public static Float256 MinValue => new Float256(BigRational.Float<UInt256>.MinValue);
    public static Float256 MaxValue => new Float256(BigRational.Float<UInt256>.MaxValue);

    private readonly BigRational.Float<UInt256> p;
    Float256(BigRational.Float<UInt256> p) => this.p = p;
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    private readonly struct UInt256 { }
  }

  /// <summary>
  /// Represents a 80-bit extended-precision floating-point number. <b>(under construction)</b>
  /// </summary>
  /// <remarks>
  /// Also known as C/C++ <c>long double</c>, GCC <c>__float80</c>, Pascal (Delphi) <c>Extended</c>, D <c>Real</c> BASIC <c>EXT</c> or <c>EXTENDED</c>.<br/>   
  /// The data format and properties as defined for the most common 
  /// <seealso href="https://en.wikipedia.org/wiki/Extended_precision#x86_extended_precision_format">x86 extended precision format</seealso>.<br/>
  /// </remarks>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float80 : IComparable<Float80>, IEquatable<Float80>, IComparable, ISpanFormattable
#if NET7_0
    , BigRational.Float<Float80.UInt80>.IFloat<Float80>
#endif
  {
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float80 b) => BigRational.Float<UInt80>.Equals(this.p, b.p);
    public readonly int CompareTo(Float80 b) => BigRational.Float<UInt80>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float80 b ? BigRational.Float<UInt80>.Compare(this.p, b.p) : throw new ArgumentException();

    public static implicit operator int(Float80 value) => (int)value.p;
    public static implicit operator Half(Float80 value) => (Half)value.p;
    public static implicit operator float(Float80 value) => (float)value.p;
    public static implicit operator double(Float80 value) => (double)value.p;
    public static explicit operator decimal(Float80 value) => (decimal)(BigRational)value; //todo: inline     
    public static implicit operator Float128(Float80 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float80 value) => Float256.Cast(value.p);
    public static implicit operator BigRational(Float80 value) => (BigRational)value.p;

    public static implicit operator Float80(int value) => new Float80((BigRational.Float<UInt80>)value);
    public static implicit operator Float80(Half value) => new Float80((BigRational.Float<UInt80>)value);
    public static implicit operator Float80(float value) => new Float80((BigRational.Float<UInt80>)value);
    public static implicit operator Float80(double value) => new Float80((BigRational.Float<UInt80>)value);
    public static explicit operator Float80(Float128 value) => new Float80(Float128.Cast<UInt80>(value));
    public static explicit operator Float80(Float256 value) => new Float80(Float256.Cast<UInt80>(value));
    public static explicit operator Float80(BigRational value) => new Float80((BigRational.Float<UInt80>)value);

    public static Float80 operator ++(Float80 a) => new Float80(a.p + +1);
    public static Float80 operator --(Float80 a) => new Float80(a.p + -1);
    public static Float80 operator +(Float80 a, Float80 b) => new Float80(a.p + b.p);
    public static Float80 operator -(Float80 a, Float80 b) => new Float80(a.p - b.p);
    public static Float80 operator *(Float80 a, Float80 b) => new Float80(a.p * b.p);
    public static Float80 operator /(Float80 a, Float80 b) => new Float80(a.p / b.p);
    public static Float80 operator %(Float80 a, Float80 b) => new Float80(a.p % b.p);

    public static Float80 Round(Float80 a, int digits) => new Float80(BigRational.Float<UInt80>.Round(a.p, digits));
    public static Float80 Truncate(Float80 a) => new Float80(BigRational.Float<UInt80>.Truncate(a.p));
    public static Float80 Cast<T>(BigRational.Float<T> a) where T : unmanaged => new Float80(BigRational.Float<T>.Cast<UInt80>(a));
    public static BigRational.Float<T> Cast<T>(Float80 a) where T : unmanaged => BigRational.Float<UInt80>.Cast<T>(a.p);

    public static Float80 Pi => new Float80(BigRational.Float<UInt80>.Pi);
    public static Float80 Tau => new Float80(BigRational.Float<UInt80>.Pi);
    public static Float80 E => new Float80(BigRational.Float<UInt80>.E);
    public static Float80 MinValue => new Float80(BigRational.Float<UInt80>.MinValue);
    public static Float80 MaxValue => new Float80(BigRational.Float<UInt80>.MaxValue);

    Float80(BigRational.Float<UInt80> p) => this.p = p;
    private readonly BigRational.Float<UInt80> p;
    [StructLayout(LayoutKind.Sequential, Size = 10)] // Pack = 2 
    private readonly struct UInt80 { } // readonly UInt16 upper; readonly UInt64 lower; }
  }
}
