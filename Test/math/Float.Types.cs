
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Globalization;
using static System.Numerics.BigRational;

namespace System.Numerics.Rational
{
  /// <summary>
  /// It's just a double - as test use Float template from external
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float64 : IComparable<Float64>, IEquatable<Float64>, IComparable, ISpanFormattable
#if NET7_0
    , Float<UInt64>.IFloat<Float64>
#endif
  {    
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public readonly bool Equals(Float64 b) => Float<UInt64>.Equals(this.p, b.p);
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly int CompareTo(Float64 b) => Float<UInt64>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float64 b ? Float<UInt64>.Compare(this.p, b.p) : throw new ArgumentException();
    public readonly override int GetHashCode() => p.GetHashCode();
    public static Float64 Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null) => new Float64(Float<UInt64>.Parse(s, provider));
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Float64 result) { var t = Float<UInt64>.TryParse(s, provider, out var r); result = new Float64(r); return t; }

    public static implicit operator int(Float64 value) => (int)value.p;
    public static implicit operator Half(Float64 value) => (Half)value.p;
    public static implicit operator float(Float64 value) => (float)value.p;
    public static implicit operator double(Float64 value) => (double)value.p;
    public static implicit operator decimal(Float64 value) => (decimal)value.p;
    public static implicit operator Float128(Float64 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float64 value) => Float256.Cast(value.p);
    public static implicit operator BigRational(Float64 value) => (BigRational)value.p;

    public static implicit operator Float64(int value) => new Float64((Float<UInt64>)value);
    public static implicit operator Float64(Half value) => new Float64((Float<UInt64>)value);
    public static implicit operator Float64(float value) => new Float64((Float<UInt64>)value);
    public static implicit operator Float64(double value) => new Float64((Float<UInt64>)value);
    public static implicit operator Float64(decimal value) => new Float64((Float<UInt64>)value);
    public static explicit operator Float64(Float128 value) => new Float64(Float128.Cast<UInt64>(value));
    public static explicit operator Float64(Float256 value) => new Float64(Float256.Cast<UInt64>(value));
    public static explicit operator Float64(BigRational value) => new Float64((Float<UInt64>)value);

    public static Float64 operator +(Float64 a, Float64 b) => new Float64(a.p + b.p);
    public static Float64 operator -(Float64 a, Float64 b) => new Float64(a.p - b.p);
    public static Float64 operator *(Float64 a, Float64 b) => new Float64(a.p * b.p);
    public static Float64 operator /(Float64 a, Float64 b) => new Float64(a.p / b.p);
    public static Float64 operator %(Float64 a, Float64 b) => new Float64(a.p % b.p);
    public static bool operator <(Float64 a, Float64 b) => a.p < b.p;
    public static bool operator >(Float64 a, Float64 b) => a.p > b.p;
    public static bool operator <=(Float64 a, Float64 b) => a.p <= b.p;
    public static bool operator >=(Float64 a, Float64 b) => a.p >= b.p;
    public static bool operator ==(Float64 a, Float64 b) => a.p == b.p;
    public static bool operator !=(Float64 a, Float64 b) => a.p > b.p;

    public static Float64 Abs(Float64 a) => new Float64(Float<UInt64>.Abs(a.p));
    public static Float64 Sqrt(Float64 a) => new Float64(Float<UInt64>.Sqrt(a.p));
    public static Float64 Sin(Float64 a) => new Float64(Float<UInt64>.Sin(a.p));
    public static Float64 Truncate(Float64 x) => new Float64(Float<UInt64>.Truncate(x.p));
    
    public static Float64 Pi => new Float64(Float<UInt64>.Pi);
    public static Float64 Tau => new Float64(Float<UInt64>.Tau);
    public static Float64 E => new Float64(Float<UInt64>.E);
    public static Float64 MinValue => new Float64(Float<UInt64>.MinValue);
    public static Float64 MaxValue => new Float64(Float<UInt64>.MaxValue);

    public static Float64 Cast<T>(Float<T> a) where T : unmanaged => new Float64(Float<T>.Cast<UInt64>(a));
    public static Float<T> Cast<T>(Float64 a) where T : unmanaged => Float<UInt64>.Cast<T>(a.p);

    private Float64(Float<UInt64> p) => this.p = p;
    private readonly Float<UInt64> p;
  }

  /// <summary>
  /// It's just for test for external new floating types from Float
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct Float96 : IComparable<Float96>, IEquatable<Float96>, IComparable, ISpanFormattable
#if NET7_0
    , Float<Float96.UInt96>.IFloat<Float96>
#endif
  {
    public readonly override string ToString() => p.ToString();
    public readonly string ToString(string? format, IFormatProvider? provider = default) => p.ToString(format, provider);
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => p.TryFormat(destination, out charsWritten, format, provider);
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float96 b) => Float<UInt96>.Equals(this.p, b.p);
    public readonly int CompareTo(Float96 b) => Float<UInt96>.Compare(this.p, b.p);
    public readonly int CompareTo(object? p) => p == null ? 1 : p is Float96 b ? Float<UInt96>.Compare(this.p, b.p) : throw new ArgumentException();

    public static implicit operator int(Float96 value) => (int)value.p;
    public static implicit operator Half(Float96 value) => (Half)value.p;
    public static implicit operator float(Float96 value) => (float)value.p;
    public static implicit operator double(Float96 value) => (double)value.p;
    public static implicit operator decimal(Float96 value) => (decimal)value.p;
    public static implicit operator Float64(Float96 value) => Float64.Cast(value.p);
    public static implicit operator Float128(Float96 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float96 value) => Float256.Cast(value.p);

    public static implicit operator Float96(int value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(Half value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(float value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(double value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(decimal value) => new Float96((Float<UInt96>)value);
    public static implicit operator Float96(Float64 value) => new Float96(Float64.Cast<UInt96>(value));
    public static explicit operator Float96(Float128 value) => new Float96(Float128.Cast<UInt96>(value));
    public static explicit operator Float96(Float256 value) => new Float96(Float256.Cast<UInt96>(value));
    public static explicit operator Float96(BigRational value) => new Float96((Float<UInt96>)value);

    public static Float96 operator ++(Float96 a) => new Float96(a.p + +1);
    public static Float96 operator --(Float96 a) => new Float96(a.p + -1);
    public static Float96 operator +(Float96 a, Float96 b) => new Float96(a.p + b.p);
    public static Float96 operator -(Float96 a, Float96 b) => new Float96(a.p - b.p);
    public static Float96 operator *(Float96 a, Float96 b) => new Float96(a.p * b.p);
    public static Float96 operator /(Float96 a, Float96 b) => new Float96(a.p / b.p);
    public static Float96 operator %(Float96 a, Float96 b) => new Float96(a.p % b.p);

    public static Float96 Round(Float96 a, int digits) => new Float96(Float<UInt96>.Round(a.p, digits));
    public static Float96 Truncate(Float96 a) => new Float96(Float<UInt96>.Truncate(a.p));

    public static Float96 Pi => new Float96(Float<UInt96>.Pi);
    public static Float96 Tau => new Float96(Float<UInt96>.Pi);
    public static Float96 E => new Float96(Float<UInt96>.E);
    public static Float96 MinValue => new Float96(Float<UInt96>.MinValue);
    public static Float96 MaxValue => new Float96(Float<UInt96>.MaxValue);

    public static Float96 Cast<T>(Float<T> a) where T : unmanaged => new Float96(Float<T>.Cast<UInt96>(a));
    public static Float<T> Cast<T>(Float96 a) where T : unmanaged => Float<UInt96>.Cast<T>(a.p);

    Float96(Float<UInt96> p) => this.p = p;
    private readonly Float<UInt96> p;
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
    private readonly struct UInt96 { readonly UInt32 high, mid, low; }
  }

}
