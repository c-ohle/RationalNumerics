
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static System.Numerics.BigRational; //todo: remove

#pragma warning disable CS1591 //todo: xml comments

namespace System.Numerics.Generic
{

  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public unsafe readonly partial struct Float<T> : IComparable<Float<T>>, IComparable, IEquatable<Float<T>>, IFormattable, ISpanFormattable where T : unmanaged
  {
    public Float(int value) => this = value;
    public Float(long value) => this = value;
    public Float(float value) => this = value;
    public Float(double value) => this = value;
    public Float(decimal value) => this = value;
    public Float(Half value) => this = (double)value;

    public static implicit operator Float<T>(int value)
    {
      var cpu = main_cpu; cpu.push(value);
      Float<T> a; cpu.fpop(&a, 0, desc); return a;
    }
    public static implicit operator Float<T>(long value)
    {
      var cpu = main_cpu; cpu.push(value);
      Float<T> a; cpu.fpop(&a, 0, desc); return a;
    }
    public static implicit operator Float<T>(Half value)
    {
      return new Float<T>(value);
    }
    public static implicit operator Float<T>(float value)
    {
      //if(sizeof(T) == 4) return *(Float<T>*)&value;
      var cpu = main_cpu; //cpu.push(value); return pop(cpu);
      var e = cpu.fpush(&value, sizeof(float) | (23 << 16));
      Float<T> a; cpu.fpop(&a, e, desc); return a;
    }
    public static implicit operator Float<T>(double value)
    {
      //if(sizeof(T) == 8) return *(Float<T>*)&value;
      var cpu = main_cpu; //cpu.push(value); return pop(cpu);
      var e = cpu.fpush(&value, sizeof(double) | (52 << 16));
      Float<T> a; cpu.fpop(&a, e, desc); return a;
    }
    public static implicit operator Float<T>(decimal value)
    {
      var cpu = main_cpu; cpu.push(value); Float<T> x; cpu.fpop(&x, desc); return x;
    }
    public static implicit operator Float<T>(BigRational value)
    {
      var cpu = main_cpu; cpu.push(value); Float<T> x; cpu.fpop(&x, desc); return x;
    }
   
    public static explicit operator int(Float<T> value)
    {
      var cpu = main_cpu;
      var e = cpu.fpush(&value, desc); cpu.pow(2, e); cpu.mul();
      var a = default(int); cpu.ipop(&a, 0x0001); return a;
    }
    public static explicit operator long(Float<T> value)
    {
      var cpu = main_cpu;
      var e = cpu.fpush(&value, desc); cpu.pow(2, e); cpu.mul();
      var a = default(long); cpu.ipop(&a, 0x0002); return a;
    }
    public static explicit operator Half(Float<T> value)
    { 
      return (Half)(double)value; 
    }    
    public static explicit operator float(Float<T> value)
    {
      //todo: direct cast, now just for test
      var cpu = main_cpu; var e = cpu.fpush(&value, desc);
      float c; cpu.fpop(&c, e, sizeof(float) | (23 << 16)); return c;
    }
    public static explicit operator double(Float<T> value)
    {
      //todo: direct cast, now just for test
      var cpu = main_cpu; var e = cpu.fpush(&value, desc);
      double c; cpu.fpop(&c, e, sizeof(double) | (52 << 16)); return c;
    }
    public static explicit operator decimal(Float<T> value)
    {
      var cpu = main_cpu; var e = cpu.fpush(&value, desc);
      cpu.pow(2, e); cpu.mul(); return cpu.popm();
    }
    public static implicit operator BigRational(Float<T> value)
    {
      var cpu = main_cpu; var e = cpu.fpush(&value, desc);
      cpu.pow(2, e); cpu.mul(); return cpu.popr(); //todo: e > mbi, see tos
    }

    public static Float<T> operator +(Float<T> value) => value;
    public static Float<T> operator -(Float<T> value)
    {
      var cpu = main_cpu; int u = cpu.fpush(&value, desc);
      cpu.neg(); cpu.fpop(&value, u, desc); return value;
    }
    public static Float<T> operator +(Float<T> left, Float<T> right)
    {
      var cpu = main_cpu;
      int u = cpu.fpush(&left, desc), v = cpu.fpush(&right.p, desc), l = u < v ? 1 : 0, e;
      cpu.shr(l == 0 ? (e = u) - v : (e = v) - u, l); cpu.add();
      cpu.fpop(&left, e, desc); return left;
    }
    public static Float<T> operator -(Float<T> left, Float<T> right)
    {
      var cpu = main_cpu; int u = cpu.fpush(&left, desc), v = cpu.fpush(&right, desc), l = u < v ? 1 : 0, e;
      cpu.shr(l == 0 ? (e = u) - v : (e = v) - u, l); cpu.sub();
      cpu.fpop(&left, e, desc); return left;
    }
    public static Float<T> operator *(Float<T> left, Float<T> right)
    {
      var cpu = main_cpu;
      int u = cpu.fpush(&left, desc);
      int v = cpu.fpush(&right, desc); cpu.mul();
      cpu.fpop(&left, u + v, desc); return left;
    }
    public static Float<T> operator /(Float<T> left, Float<T> right)
    {
      var cpu = main_cpu; var mbi = desc >> 16;
      var u = cpu.fpush(&left, desc); cpu.shl(mbi);
      var v = cpu.fpush(&right, desc); cpu.idiv();
      var w = u - v - mbi; if (v > 0x7ffffff0) w = 0x7ffffff1; else if (u > 0x7ffffff0) w = 0x7ffffff3; //todo: rem. or check inf rules
      cpu.fpop(&left, w, desc); return left;
    }
    public static Float<T> operator %(Float<T> left, Float<T> right)
    {
      return left - Truncate(left / right) * right; //todo: opt. inline
    }

    public static bool operator ==(Float<T> left, Float<T> right) => CPU.fequ(&left, &right, desc);
    public static bool operator !=(Float<T> left, Float<T> right) => !CPU.fequ(&left, &right, desc);
    public static bool operator >=(Float<T> left, Float<T> right) => CPU.fcmp(&left, &right, desc) >= 0;
    public static bool operator <=(Float<T> left, Float<T> right) => CPU.fcmp(&left, &right, desc) <= 0;
    public static bool operator >(Float<T> left, Float<T> right) => CPU.fcmp(&left, &right, desc) > 0;
    public static bool operator <(Float<T> left, Float<T> right) => CPU.fcmp(&left, &right, desc) < 0;

    public static int Bits => sizeof(Float<T>) << 3;
    public static int MaxDigits
    {
      get { return unchecked((int)(((desc >> 16) + 1) * 0.30103f) + 1); }
    }
    public static Float<T> MinValue
    {
      get { return -MaxValue; }
    }
    public static Float<T> MaxValue
    {
      get //0x7fefffffffffffff
      {
        Float<T> a; new Span<uint>(&a, sizeof(Float<T>) >> 2).Fill(0xffffffff);
        *(uint*)(((byte*)&a) + (sizeof(Float<T>) - 4)) = 0x7fffffff ^ (1u << (desc >> 16)); return a;
      }
    }
    public static Float<T> Epsilon
    {
      get { var a = default(Float<T>); *(uint*)&a |= 1; return a; }
    }
    public static Float<T> NaN
    {
      get { var cpu = main_cpu; cpu.push(); Float<T> a; cpu.fpop(&a, 0x7ffffff3, desc); return a; }
    }
    public static Float<T> NegativeInfinity
    {
      get { var cpu = main_cpu; cpu.push(); Float<T> a; cpu.fpop(&a, 0x7ffffff2, desc); return a; }
    }
    public static Float<T> PositiveInfinity
    {
      get { var cpu = main_cpu; cpu.push(); Float<T> a; cpu.fpop(&a, 0x7ffffff1, desc); return a; }
    }
    public static Float<T> NegativeZero => default; //todo: cpu 

    public static Float<T> E
    {
      get { var cpu = main_cpu; cpu.push(1u); cpu.exp(unchecked((uint)(desc >> 16) + 2)); Float<T> x; cpu.fpop(&x, desc); return x; }
    }
    public static Float<T> Pi
    {
      get { var cpu = main_cpu; cpu.pi(unchecked((uint)(desc >> 16) + 2)); Float<T> x; cpu.fpop(&x, desc); return x; }
    }
    public static Float<T> Tau
    {
      get { var cpu = main_cpu; cpu.pi(unchecked((uint)(desc >> 16) + 2)); cpu.shl(1); Float<T> x; cpu.fpop(&x, desc); return x; }
    }

    public static int Sign(Float<T> value)
    {
      var h = *(uint*)(((byte*)&value) + (sizeof(Float<T>) - 4));
      return h == 0 ? 0 : (h & 0x80000000) != 0 ? -1 : +1;
    }
    public static bool IsPositive(Float<T> value)
    {
      var h = *(uint*)(((byte*)&value) + (sizeof(Float<T>) - 4));
      return (h & 0x80000000) == 0;
    }
    public static bool IsNegative(Float<T> value)
    {
      var h = *(uint*)(((byte*)&value) + (sizeof(Float<T>) - 4));
      return (h & 0x80000000) != 0;
    }
    public static bool IsInteger(Float<T> value)
    {
      return IsFinite(value) && value == Truncate(value); //todo: inline
    }
    public static bool IsFinite(Float<T> value)
    {
      var e = CPU.ftest(&value, desc); return e < 0x7ffffff0;
    }
    public static bool IsNaN(Float<T> value)
    {
      var e = CPU.ftest(&value, desc); return e == 0x7ffffff3;
    }
    public static bool IsRealNumber(Float<T> value)
    {
      var e = CPU.ftest(&value, desc); return e != 0x7ffffff3;
    }
    public static bool IsInfinity(Float<T> value)
    {
      var e = CPU.ftest(&value, desc); return e == 0x7ffffff1 || e == 0x7ffffff2;
    }
    public static bool IsNegativeInfinity(Float<T> value)
    {
      var e = CPU.ftest(&value, desc); return e == 0x7ffffff1;
    }
    public static bool IsPositiveInfinity(Float<T> value)
    {
      var e = CPU.ftest(&value, desc); return e == 0x7ffffff2;
    }

    public static Float<T> Abs(Float<T> value)
    {
      return value < 0 ? -value : value;
    }
    public static Float<T> Truncate(Float<T> a)
    {
      var cpu = main_cpu; var e = cpu.fpush(&a, desc);
      cpu.shl(e); cpu.fpop(&a, 0, desc); return a;
    }
    public static Float<T> Round(Float<T> x, int digits = 0, MidpointRounding mode = MidpointRounding.ToEven)
    {
      var p = Pow10(digits); var a = Truncate(x * p) / p; return a;
    }
    public static Float<T> Pow10(int y)
    {
      Float<T> x = 1, z = 10; uint e = unchecked((uint)(y >= 0 ? y : -y));
      for (; ; e >>= 1)
      {
        if ((e & 1) != 0) x *= z;
        if (e <= 1) break; z *= z; //todo: sqr
      }
      if (y < 0) x = 1 / x;
      return x;
    }
    public static Float<T> Sqrt(Float<T> x)
    {
      var cpu = main_cpu; cpu.push(x);
      cpu.sqrt(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Sin(Float<T> x)
    {
      var cpu = main_cpu; cpu.push(x);
      cpu.sin(unchecked((uint)(desc >> 16) + 2), false); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Cos(Float<T> x)
    {
      var cpu = main_cpu; cpu.push(x);
      cpu.sin(unchecked((uint)(desc >> 16) + 2), true); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Atan(Float<T> x)
    {
      var cpu = main_cpu; cpu.push(x);
      cpu.atan(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Exp(Float<T> x)
    {
      var cpu = main_cpu; cpu.push(x);
      cpu.exp(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Exp2(Float<T> x)
    {
      return Pow(2, x);
    }
    public static Float<T> Exp10(Float<T> x)
    {
      return Pow(10, x);
    }
    public static Float<T> Log(Float<T> x)
    {
      var cpu = main_cpu; cpu.push(x);
      cpu.log(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Log2(Float<T> x)
    {
      var cpu = main_cpu; cpu.push(x);
      cpu.log2(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Pow(Float<T> x, Float<T> y)
    {
      var s = Sign(x); if (s == 0) return default;
      if (s < 0)
      {
        if (IsInteger(y)) return Round(Pow(x, (int)y));
        return Float<T>.NaN;
      }
      return Exp(Log(x) * y);
      // var cpu = main_cpu; var c = prec(digits);
      // cpu.push(x); cpu.log(c);
      // cpu.push(y); cpu.mul(); cpu.exp(c);
      // cpu.rnd(digits); return cpu.popr();
    }
    public static Float<T> Cast<TOther>(Float<TOther> value) where TOther : unmanaged
    {
      var cpu = main_cpu; var e = cpu.fpush(&value, Float<TOther>.desc);
      Float<T> b; cpu.fpop(&b, e, desc); return b;
    }

    #region private
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly T p;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static int desc;
    static Float()
    {
      int size = sizeof(T), sbi; if ((size & 1) != 0) throw new NotSupportedException(nameof(T));
      switch (size)
      {
        case 2: sbi = 5; break;
        case 4: sbi = 8; break;
        case 8: sbi = 11; break;
        case <= 16: sbi = 15; break;
        default: sbi = BitOperations.Log2(unchecked((uint)size)) * 3 + 4; break;
      }
      desc = sizeof(T) | ((((size << 3) - sbi) - 1) << 16);
    }
    #endregion

    public readonly override string ToString() => ToString(null, null);
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
      Span<char> sp = stackalloc char[MaxDigits + 32]; // (1 + 16)];
      if (!TryFormat(sp, out var ns, format, formatProvider))
      {
        int n; sp.Slice(0, 2).CopyTo(new Span<char>(&n, 2)); sp = stackalloc char[n];
        TryFormat(sp, out ns, format, formatProvider); Debug.Assert(ns != 0);
      }
      return sp.Slice(0, ns).ToString();
    }
    public bool TryFormat(Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
      var dbg = provider == null && format.Length == 0 && format != default;
      var fmt = 'G'; int dig = 0, rnd = 0; var info = dbg ? NumberFormatInfo.InvariantInfo : NumberFormatInfo.GetInstance(provider);
      if (format.Length != 0)
      {
        var f = (fmt = format[0]) & ~0x20; var d = format.Length > 1;
        if (d) dig = stoi(format.Slice(1)); //int.Parse(format.Slice(1));//, NumberFormatInfo.InvariantInfo);
        if (f == 'E') { rnd = dig; if (rnd == 0 && !d) rnd = 6; dig = rnd + 1; }
        if (f == 'F') { rnd = dig; if (rnd == 0 && !d) rnd = info.NumberDecimalDigits; dig = 0; }
      }
      // dig = (int)Math.Ceiling((((desc >> 16) + 1) * 0.30103f));
      if (dig == 0) { dig = MaxDigits; if (dbg) dig++; } //dbg like float G9, double G17         
      if (dest.Length < dig + 16) { dig += 16; goto ex; }
      var cpu = main_cpu; var value = this; var es = 0;
      var e = cpu.fpush(&value, desc);
      if (e >= 0x7ffffff0)
      {
        var s = ((e & 7) == 1 ? info.NegativeInfinitySymbol : (e & 7) == 2 ? info.PositiveInfinitySymbol : info.NaNSymbol).AsSpan();
        cpu.pop(); s.CopyTo(dest); charsWritten = s.Length; return true;
      }
      var ep = (int)((e + (desc >> 16)) * 0.30103f); // maxd unchecked((int)(((desc >> 16) + 1) * 0.30103f) + 1);
      var d1 = ep < 0 ? -ep : ep; var d2 = dig < 0 ? -dig : dig; var dd = d1 - d2;
      if (dd > 10) //todo: opt. F? check
      {
        if (dig <= 0) { }
        else if (ep > 0)
        {
          cpu.pop(); value *= Pow10(-(es = dd - 3));
          e = cpu.fpush(&value, desc);
        }
        else if (ep < 0)
        {
          cpu.pop(); value *= Pow10(-(es = 3 - dd));
          e = cpu.fpush(&value, desc);
        }
        else { }
      }
      cpu.pow(2, e); cpu.mul();
      var n = tos(dest, cpu, fmt, dig, rnd, es, info.NumberDecimalSeparator[0] == ',' ? 0x04 : 0);
      if (n < 0) { dig = -n; goto ex; }
      charsWritten = n; return true; ex:
      charsWritten = 0; if (dest.Length >= 2) new Span<char>(&dig, 2).CopyTo(dest); return false;
    }
    public static Float<T> Parse(string s)
    {
      return Parse(s.AsSpan(), null);
    }
    public static Float<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
      var info = provider != null ? NumberFormatInfo.GetInstance(provider) : null;
      var cpu = main_cpu; cpu.tor(s, 10, info != null ? info.NumberDecimalSeparator[0] : default);
      Float<T> x; cpu.fpop(&x, desc); return x;
    }
    public readonly override int GetHashCode()
    {
      var a = this; return CPU.hash(&a, sizeof(T));      
      //var p = (uint*)&a; uint n = unchecked((uint)sizeof(T)), h = 0;
      //for (uint i = 0, c = n >> 2; i < c; i++) h = p[i] ^ ((h << 7) | (h >> 25));
      //if ((n & 2) != 0) h ^= p[n >> 2] & 0xffff;
      //return unchecked((int)h);
    }
    public readonly override bool Equals([NotNullWhen(true)] object? obj)
    {
      if (obj is not Float<T> b) return false;
      var a = this; return CPU.fequ(&a, &b, desc);
    }
    public readonly bool Equals(Float<T> other) { var t = this; return CPU.fequ(&t, &other, desc); }
    public readonly int CompareTo(object? obj) => obj == null ? 1 : p is Float<T> b ? this.CompareTo(b) : throw new ArgumentException();
    public readonly int CompareTo(Float<T> other) { var a = this; return CPU.fcmp(&a, &other, desc); }
  }

#if NET7_0
  public unsafe readonly partial struct Float<T> : IBinaryFloatingPointIeee754<Float<T>>, IMinMaxValue<Float<T>>
  {
    static int INumberBase<Float<T>>.Radix => 2;
    static Float<T> INumberBase<Float<T>>.One => 1;
    static Float<T> ISignedNumber<Float<T>>.NegativeOne => -1;
    static Float<T> INumberBase<Float<T>>.Zero => default;
    static Float<T> IAdditiveIdentity<Float<T>, Float<T>>.AdditiveIdentity => default;
    static Float<T> IMultiplicativeIdentity<Float<T>, Float<T>>.MultiplicativeIdentity => 1;
    static Float<T> IBinaryNumber<Float<T>>.AllBitsSet { get { Float<T> a; new Span<ushort>(&a, sizeof(Float<T>) >> 1).Fill(0xffff); return a; } }
    static bool INumberBase<Float<T>>.IsCanonical(Float<T> value) => true;
    static bool INumberBase<Float<T>>.IsComplexNumber(Float<T> value) => false;
    static bool INumberBase<Float<T>>.IsImaginaryNumber(Float<T> value) => false;
    static bool INumberBase<Float<T>>.IsZero(Float<T> value) => value == default; //value == 0
    int IFloatingPoint<Float<T>>.GetExponentByteCount() => (((desc >> 16) + 1) >> 5) + 1; //sizeof(sbyte), sizeof(short), 4;
    int IFloatingPoint<Float<T>>.GetSignificandBitLength() => (desc >> 16) + 1; // 53
    int IFloatingPoint<Float<T>>.GetSignificandByteCount() => sizeof(T); // ??? like float and double // (((desc >> 16) + 1) >> 3) + 1; // 2
    int IFloatingPoint<Float<T>>.GetExponentShortestBitLength()
    {
      int size = desc & 0xffff, bc = desc >> 16, ec = (size << 3) - bc; //, bi = ((1 << (ec - 2)) + bc) - 1; //2 52 12 1075
      var t = this; var h = *(uint*)(((byte*)&t) + (size - 4));
      var ExponentBias = (0xffffffff >> (32 - ec)) >> 2; //1023 127
      var BiasedExponent = (h & 0x7fffffff) >> (32 - ec);
      var Exponent = unchecked((int)BiasedExponent - (int)ExponentBias);
      var bl = Exponent >= 0 ? 32 - int.LeadingZeroCount(Exponent) : 33 - int.LeadingZeroCount((int)(~Exponent));
      return bl;
    }

    #region todo
    static Float<T> IDecrementOperators<Float<T>>.operator --(Float<T> value) => throw new NotImplementedException();
    static Float<T> IIncrementOperators<Float<T>>.operator ++(Float<T> value) => throw new NotImplementedException();
    static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator &(Float<T> left, Float<T> right) => throw new NotImplementedException();
    static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator |(Float<T> left, Float<T> right) => throw new NotImplementedException();
    static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator ^(Float<T> left, Float<T> right) => throw new NotImplementedException();
    static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator ~(Float<T> value) => throw new NotImplementedException();
    static Float<T> IBinaryNumber<Float<T>>.Log2(Float<T> value) => throw new NotImplementedException();
    static Float<T> IFloatingPointIeee754<Float<T>>.Atan2(Float<T> y, Float<T> x) => throw new NotImplementedException();
    static Float<T> IFloatingPointIeee754<Float<T>>.Atan2Pi(Float<T> y, Float<T> x) => throw new NotImplementedException();
    static Float<T> IFloatingPointIeee754<Float<T>>.BitDecrement(Float<T> x) => throw new NotImplementedException();
    static Float<T> IFloatingPointIeee754<Float<T>>.BitIncrement(Float<T> x) => throw new NotImplementedException();
    static Float<T> IFloatingPointIeee754<Float<T>>.FusedMultiplyAdd(Float<T> left, Float<T> right, Float<T> addend) => throw new NotImplementedException();
    static Float<T> IFloatingPointIeee754<Float<T>>.Ieee754Remainder(Float<T> left, Float<T> right) => throw new NotImplementedException();
    static int IFloatingPointIeee754<Float<T>>.ILogB(Float<T> x) => throw new NotImplementedException();
    static Float<T> IFloatingPointIeee754<Float<T>>.ScaleB(Float<T> x, int n) => throw new NotImplementedException();
    bool IFloatingPoint<Float<T>>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IFloatingPoint<Float<T>>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IFloatingPoint<Float<T>>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    bool IFloatingPoint<Float<T>>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    static Float<T> IHyperbolicFunctions<Float<T>>.Acosh(Float<T> x) => throw new NotImplementedException();
    static Float<T> IHyperbolicFunctions<Float<T>>.Asinh(Float<T> x) => throw new NotImplementedException();
    static Float<T> IHyperbolicFunctions<Float<T>>.Atanh(Float<T> x) => throw new NotImplementedException();
    static Float<T> IHyperbolicFunctions<Float<T>>.Cosh(Float<T> x) => throw new NotImplementedException();
    static Float<T> IHyperbolicFunctions<Float<T>>.Sinh(Float<T> x) => throw new NotImplementedException();
    static Float<T> IHyperbolicFunctions<Float<T>>.Tanh(Float<T> x) => throw new NotImplementedException();
    static Float<T> ILogarithmicFunctions<Float<T>>.Log(Float<T> x, Float<T> newBase) => throw new NotImplementedException();
    static Float<T> ILogarithmicFunctions<Float<T>>.Log10(Float<T> x) => throw new NotImplementedException();
    static Float<T> IRootFunctions<Float<T>>.Cbrt(Float<T> x) => throw new NotImplementedException();
    static Float<T> IRootFunctions<Float<T>>.Hypot(Float<T> x, Float<T> y) => throw new NotImplementedException();
    static Float<T> IRootFunctions<Float<T>>.RootN(Float<T> x, int n) => throw new NotImplementedException();
    static Float<T> ITrigonometricFunctions<Float<T>>.Acos(Float<T> x) => throw new NotImplementedException();
    static Float<T> ITrigonometricFunctions<Float<T>>.AcosPi(Float<T> x) => throw new NotImplementedException();
    static Float<T> ITrigonometricFunctions<Float<T>>.Asin(Float<T> x) => throw new NotImplementedException();
    static Float<T> ITrigonometricFunctions<Float<T>>.AsinPi(Float<T> x) => throw new NotImplementedException();
    static Float<T> ITrigonometricFunctions<Float<T>>.AtanPi(Float<T> x) => throw new NotImplementedException();
    static Float<T> ITrigonometricFunctions<Float<T>>.CosPi(Float<T> x) => throw new NotImplementedException();
    static (Float<T> Sin, Float<T> Cos) ITrigonometricFunctions<Float<T>>.SinCos(Float<T> x) => throw new NotImplementedException();
    static (Float<T> SinPi, Float<T> CosPi) ITrigonometricFunctions<Float<T>>.SinCosPi(Float<T> x) => throw new NotImplementedException();
    static Float<T> ITrigonometricFunctions<Float<T>>.SinPi(Float<T> x) => throw new NotImplementedException();
    static Float<T> ITrigonometricFunctions<Float<T>>.Tan(Float<T> x) => throw new NotImplementedException();
    static Float<T> ITrigonometricFunctions<Float<T>>.TanPi(Float<T> x) => throw new NotImplementedException();
    static Float<T> INumberBase<Float<T>>.MaxMagnitude(Float<T> x, Float<T> y) => throw new NotImplementedException();
    static Float<T> INumberBase<Float<T>>.MaxMagnitudeNumber(Float<T> x, Float<T> y) => throw new NotImplementedException();
    static Float<T> INumberBase<Float<T>>.MinMagnitude(Float<T> x, Float<T> y) => throw new NotImplementedException();
    static Float<T> INumberBase<Float<T>>.MinMagnitudeNumber(Float<T> x, Float<T> y) => throw new NotImplementedException();
    static Float<T> INumberBase<Float<T>>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static Float<T> INumberBase<Float<T>>.Parse(string s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.TryConvertFromChecked<TOther>(TOther value, out Float<T> result) => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.TryConvertFromSaturating<TOther>(TOther value, out Float<T> result) => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.TryConvertFromTruncating<TOther>(TOther value, out Float<T> result) => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.TryConvertToChecked<TOther>(Float<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.TryConvertToSaturating<TOther>(Float<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.TryConvertToTruncating<TOther>(Float<T> value, out TOther result) where TOther : default => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Float<T> result) => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Float<T> result) => throw new NotImplementedException();
    static bool ISpanParsable<Float<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Float<T> result) => throw new NotImplementedException();
    static Float<T> IParsable<Float<T>>.Parse(string s, IFormatProvider? provider) => throw new NotImplementedException();
    static bool IParsable<Float<T>>.TryParse(string? s, IFormatProvider? provider, out Float<T> result) => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.IsEvenInteger(Float<T> value) => throw new NotImplementedException(); //IsInteger(value) && (Abs(value % 2) == 0);
    static bool INumberBase<Float<T>>.IsOddInteger(Float<T> value) => throw new NotImplementedException(); //IsInteger(value) && (Abs(value % 2) == 1);
    static bool INumberBase<Float<T>>.IsNegativeInfinity(Float<T> value) => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.IsNormal(Float<T> value) => throw new NotImplementedException();
    static bool INumberBase<Float<T>>.IsSubnormal(Float<T> value) => throw new NotImplementedException();
    static bool IBinaryNumber<Float<T>>.IsPow2(Float<T> value) => throw new NotImplementedException();
    #endregion

    // static bool INumberBase<Float<T>>.IsNaN(Float<T> value) => throw new NotImplementedException();//value == value; 
    // static bool INumberBase<Float<T>>.IsRealNumber(Float<T> value) => throw new NotImplementedException();//value == value;
    // static bool INumberBase<Float<T>>.IsFinite(Float<T> value) => throw new NotImplementedException();
    // static bool INumberBase<Float<T>>.IsInfinity(Float<T> value) => throw new NotImplementedException();
    // static bool INumberBase<Float<T>>.IsNegativeInfinity(Float<T> value) => throw new NotImplementedException();
    // static bool INumberBase<Float<T>>.IsPositiveInfinity(Float<T> value) => throw new NotImplementedException();
    // static bool INumberBase<Float<T>>.IsInteger(Float<T> value) => throw new NotImplementedException();
    // static bool INumberBase<Float<T>>.IsPositive(Float<T> value) => throw new NotImplementedException(); //BitConverter.DoubleToInt64Bits(value) >= 0;
    // static bool INumberBase<Float<T>>.IsNegative(Float<T> value) => throw new NotImplementedException();
    // static Float<T> IFloatingPointIeee754<Float<T>>.Epsilon => throw new NotImplementedException();
    // static Float<T> IFloatingPointIeee754<Float<T>>.NaN => throw new NotImplementedException();
    // static Float<T> IFloatingPointIeee754<Float<T>>.NegativeInfinity => throw new NotImplementedException();
    // static Float<T> IFloatingPointIeee754<Float<T>>.NegativeZero => throw new NotImplementedException();
    // static Float<T> IFloatingPointIeee754<Float<T>>.PositiveInfinity => throw new NotImplementedException();
    // static Float<T> IFloatingPointConstants<Float<T>>.E => throw new NotImplementedException();
    // static Float<T> IFloatingPointConstants<Float<T>>.Pi => throw new NotImplementedException();
    // static Float<T> IFloatingPointConstants<Float<T>>.Tau => throw new NotImplementedException();
    // bool IEquatable<Float<T>>.Equals(Float<T> other) => throw new NotImplementedException();
    // bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => throw new NotImplementedException();
    // string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => throw new NotImplementedException();
    // static Float<T> ISpanParsable<Float<T>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => throw new NotImplementedException();
    // static Float<T> ITrigonometricFunctions<Float<T>>.Atan(Float<T> x) => throw new NotImplementedException();
    // static Float<T> ITrigonometricFunctions<Float<T>>.Sin(Float<T> x) => throw new NotImplementedException();
    // static Float<T> INumberBase<Float<T>>.Abs(Float<T> value) => throw new NotImplementedException();
    // static Float<T> IPowerFunctions<Float<T>>.Pow(Float<T> x, Float<T> y) => throw new NotImplementedException();
    // static Float<T> IRootFunctions<Float<T>>.Sqrt(Float<T> x) => throw new NotImplementedException();
    // static Float<T> ITrigonometricFunctions<Float<T>>.Cos(Float<T> x) => throw new NotImplementedException();
    // static Float<T> ILogarithmicFunctions<Float<T>>.Log(Float<T> x) => throw new NotImplementedException();
    // static Float<T> ILogarithmicFunctions<Float<T>>.Log2(Float<T> x) => throw new NotImplementedException();
    // int IComparable.CompareTo(object? obj) => throw new NotImplementedException();
    // int IComparable<Float<T>>.CompareTo(Float<T> other) => throw new NotImplementedException();
    // static Float<T> IExponentialFunctions<Float<T>>.Exp(Float<T> x) => throw new NotImplementedException();
    // static Float<T> IExponentialFunctions<Float<T>>.Exp10(Float<T> x) => throw new NotImplementedException();
    // static Float<T> IExponentialFunctions<Float<T>>.Exp2(Float<T> x) => throw new NotImplementedException();
    // static Float<T> IFloatingPoint<Float<T>>.Round(Float<T> x, int digits, MidpointRounding mode) => throw new NotImplementedException();
    // static Float<T> IFloatingPoint<Float<T>>.Truncate(Float<T> x) => throw new NotImplementedException();
    // static Float<T> IAdditionOperators<Float<T>, Float<T>, Float<T>>.operator +(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static Float<T> ISubtractionOperators<Float<T>, Float<T>, Float<T>>.operator -(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static Float<T> IMultiplyOperators<Float<T>, Float<T>, Float<T>>.operator *(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static Float<T> IDivisionOperators<Float<T>, Float<T>, Float<T>>.operator /(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static Float<T> IModulusOperators<Float<T>, Float<T>, Float<T>>.operator %(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static bool IEqualityOperators<Float<T>, Float<T>, bool>.operator ==(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static bool IEqualityOperators<Float<T>, Float<T>, bool>.operator !=(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<Float<T>, Float<T>, bool>.operator <=(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<Float<T>, Float<T>, bool>.operator >=(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<Float<T>, Float<T>, bool>.operator >(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static bool IComparisonOperators<Float<T>, Float<T>, bool>.operator <(Float<T> left, Float<T> right) => throw new NotImplementedException();
    // static Float<T> IUnaryNegationOperators<Float<T>, Float<T>>.operator -(Float<T> value) => throw new NotImplementedException();
    // static Float<T> IUnaryPlusOperators<Float<T>, Float<T>>.operator +(Float<T> value) => throw new NotImplementedException();

  }
#endif

}

