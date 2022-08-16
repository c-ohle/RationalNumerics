
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Numerics;

namespace System.Numerics.Rational
{
  /// <summary>
  /// It's just a double for test
  /// </summary>
  public readonly struct Float64
  {
    public readonly override string ToString() => p.ToString();

    public static implicit operator int(Float64 value) => (int)value.p;
    public static implicit operator Half(Float64 value) => (Half)value.p;
    public static implicit operator float(Float64 value) => (float)value.p;
    public static implicit operator double(Float64 value) => (double)value.p;
    public static implicit operator Float128(Float64 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float64 value) => Float256.Cast(value.p);
    public static implicit operator BigRational(Float64 value) => (BigRational)value.p;

    public static implicit operator Float64(int value) => new Float64((BigRational.Float<UInt64>)value);
    public static implicit operator Float64(Half value) => new Float64((BigRational.Float<UInt64>)value);
    public static implicit operator Float64(float value) => new Float64((BigRational.Float<UInt64>)value);
    public static implicit operator Float64(double value) => new Float64((BigRational.Float<UInt64>)value);
    public static explicit operator Float64(Float128 value) => new Float64(Float128.Cast<UInt64>(value));
    public static explicit operator Float64(Float256 value) => new Float64(Float256.Cast<UInt64>(value));
    public static explicit operator Float64(BigRational value) => new Float64((BigRational.Float<UInt64>)value);

    public static Float64 operator +(Float64 a, Float64 b) => new Float64(a.p + b.p);
    public static Float64 operator -(Float64 a, Float64 b) => new Float64(a.p - b.p);
    public static Float64 operator *(Float64 a, Float64 b) => new Float64(a.p * b.p);
    public static Float64 operator /(Float64 a, Float64 b) => new Float64(a.p / b.p);
    public static Float64 operator %(Float64 a, Float64 b) => new Float64(a.p % b.p);

    public static Float64 Truncate(Float64 x) => new Float64(BigRational.Float<UInt64>.Truncate(x.p));

    public static Float64 Cast<T>(BigRational.Float<T> a) where T : unmanaged => new Float64(BigRational.Float<T>.Cast<UInt64>(a));
    public static BigRational.Float<T> Cast<T>(Float64 a) where T : unmanaged => BigRational.Float<UInt64>.Cast<T>(a.p);

    public static Float64 MinValue => new Float64(BigRational.Float<UInt64>.MinValue);
    public static Float64 MaxValue => new Float64(BigRational.Float<UInt64>.MaxValue);

    private Float64(BigRational.Float<UInt64> p) => this.p = p;
    private readonly BigRational.Float<UInt64> p;
  }

  /// <summary>
  /// It's just for test for external new floating types from BigRational.Float
  /// </summary>
  public readonly struct Float96 : IComparable<Float96>, IEquatable<Float96>
  {
    public readonly override string ToString() => p.ToString();
    public override readonly int GetHashCode() => p.GetHashCode();
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public readonly bool Equals(Float96 b) => BigRational.Float<UInt96>.Equals(this.p, b.p);
    public readonly int CompareTo(Float96 b) => BigRational.Float<UInt96>.Compare(this.p, b.p);

    public static implicit operator int(Float96 value) => (int)value.p;
    public static implicit operator Half(Float96 value) => (Half)value.p;
    public static implicit operator float(Float96 value) => (float)value.p;
    public static implicit operator double(Float96 value) => (double)value.p;
    public static implicit operator Float64(Float96 value) => Float64.Cast(value.p);
    public static implicit operator Float128(Float96 value) => Float128.Cast(value.p);
    public static implicit operator Float256(Float96 value) => Float256.Cast(value.p);

    public static implicit operator Float96(int value) => new Float96((BigRational.Float<UInt96>)value);
    public static implicit operator Float96(Half value) => new Float96((BigRational.Float<UInt96>)value);
    public static implicit operator Float96(float value) => new Float96((BigRational.Float<UInt96>)value);
    public static implicit operator Float96(double value) => new Float96((BigRational.Float<UInt96>)value);
    public static implicit operator Float96(Float64 value) => new Float96(Float64.Cast<UInt96>(value));
    public static explicit operator Float96(Float128 value) => new Float96(Float128.Cast<UInt96>(value));
    public static explicit operator Float96(Float256 value) => new Float96(Float256.Cast<UInt96>(value));
    public static explicit operator Float96(BigRational value) => new Float96((BigRational.Float<UInt96>)value);

    public static Float96 operator ++(Float96 a) => new Float96(a.p + +1);
    public static Float96 operator --(Float96 a) => new Float96(a.p + -1);
    public static Float96 operator +(Float96 a, Float96 b) => new Float96(a.p + b.p);
    public static Float96 operator -(Float96 a, Float96 b) => new Float96(a.p - b.p);
    public static Float96 operator *(Float96 a, Float96 b) => new Float96(a.p * b.p);
    public static Float96 operator /(Float96 a, Float96 b) => new Float96(a.p / b.p);
    public static Float96 operator %(Float96 a, Float96 b) => new Float96(a.p % b.p);

    public static Float96 Truncate(Float96 a) => new Float96(BigRational.Float<UInt96>.Truncate(a.p));

    public static Float96 Cast<T>(BigRational.Float<T> a) where T : unmanaged => new Float96(BigRational.Float<T>.Cast<UInt96>(a));
    public static BigRational.Float<T> Cast<T>(Float96 a) where T : unmanaged => BigRational.Float<UInt96>.Cast<T>(a.p);

    public static Float96 MinValue => new Float96(BigRational.Float<UInt96>.MinValue);
    public static Float96 MaxValue => new Float96(BigRational.Float<UInt96>.MaxValue);

    Float96(BigRational.Float<UInt96> p) => this.p = p;
    private readonly BigRational.Float<UInt96> p;
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
    private readonly struct UInt96 { readonly UInt32 high, low; }
  }

}
