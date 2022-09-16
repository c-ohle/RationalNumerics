
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
      var cpu = main_cpu; cpu.push(value);
      Float<T> x; cpu.fpop(&x, desc); return x;
    }

    public static explicit operator int(Float<T> value)
    {
      var cpu = main_cpu;
      var e = cpu.fpush(&value, desc); cpu.pow(2, e); cpu.mul();
      int a; cpu.ipop(&a, sizeof(int)); return a;
    }
    public static explicit operator long(Float<T> value)
    {
      var cpu = main_cpu;
      var e = cpu.fpush(&value, desc); cpu.pow(2, e); cpu.mul();
      long a; cpu.ipop(&a, sizeof(long)); return a;
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
      if (value != default) ((byte*)&value)[sizeof(Float<T>) - 1] ^= 0x80; return value;
      //var cpu = main_cpu; int u = cpu.fpush(&value, desc);
      //cpu.neg(); cpu.fpop(&value, u, desc); return value;
    }
    public static Float<T> operator ++(Float<T> value)
    {
      return value + 1;
    }
    public static Float<T> operator --(Float<T> value)
    {
      return value - 1;
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
      return left - Truncate(left / right) * right; //todo: opt. cpu
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
      if ((((byte*)&value)[sizeof(T) - 1] & 0x80) != 0) return -1;
      return value == default ? 0 : 1;
      //var h = *(uint*)(((byte*)&value) + (sizeof(Float<T>) - 4));
      //return h == 0 ? 0 : (h & 0x80000000) != 0 ? -1 : +1;
    }
    public static bool IsPositive(Float<T> value)
    {
      return (((byte*)&value)[sizeof(T) - 1] & 0x80) == 0;
      //var h = *(uint*)(((byte*)&value) + (sizeof(Float<T>) - 4));
      //return (h & 0x80000000) == 0;
    }
    public static bool IsNegative(Float<T> value)
    {
      return (((byte*)&value)[sizeof(T) - 1] & 0x80) != 0;
      //var h = *(uint*)(((byte*)&value) + (sizeof(Float<T>) - 4));
      //return (h & 0x80000000) != 0;
    }
    public static bool IsInteger(Float<T> value)
    {
      return IsFinite(value) && value == Truncate(value); //todo: cpu
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
    public static bool IsEvenInteger(Float<T> value)
    {
      return IsInteger(value) && Abs(value % 2) == 0;
    }
    public static bool IsOddInteger(Float<T> value)
    {
      return IsInteger(value) && Abs(value % 2) == 1;
    }
    public static bool IsNormal(Float<T> value)
    {
      return value != default && CPU.ftest(&value, desc) < 0x7ffffff0;
    }
    public static bool IsSubnormal(Float<T> value)
    {
      return value != default && CPU.ftest(&value, desc) >= 0x7ffffff0;
    }
    public static bool IsPow2(Float<T> value)
    {
      var cpu = main_cpu; var e = cpu.fpush(&value, desc);
      var r = e < 0x7ffffff0 && cpu.sign() > 0 && cpu.ipt(); cpu.pop(); return r;
    }

    public static Float<T> Abs(Float<T> value)
    {
      //return value < 0 ? -value : value;
      *(uint*)(((byte*)&value) + (sizeof(Float<T>) - 4)) &= 0x7fffffff; return value;
    }
    public static Float<T> Min(Float<T> x, Float<T> y)
    {
      return CPU.fcmp(&x, &y, desc) <= 0 ? x : y;
    }
    public static Float<T> Max(Float<T> x, Float<T> y)
    {
      return CPU.fcmp(&x, &y, desc) >= 0 ? x : y;
    }
    public static Float<T> MinMagnitude(Float<T> x, Float<T> y)
    { //todo: opt. cpu  
      var ax = Abs(x); var ay = Abs(y); if (ax < ay || IsNaN(ax)) return x;
      if (ax == ay) return IsNegative(x) ? x : y; return y;
    }
    public static Float<T> MaxMagnitude(Float<T> x, Float<T> y)
    { //todo: opt. cpu  
      var ax = Abs(x); var ay = Abs(y); if (ax > ay || IsNaN(ax)) return x;
      if (ax == ay) return IsNegative(x) ? y : x; return y;
    }
    public static Float<T> MinMagnitudeNumber(Float<T> x, Float<T> y)
    { //todo: opt. cpu  
      var ax = Abs(x); var ay = Abs(y); if (ax < ay || IsNaN(ay)) return x;
      if (ax == ay) return IsNegative(x) ? x : y; return y;
    }
    public static Float<T> MaxMagnitudeNumber(Float<T> x, Float<T> y)
    { //todo: opt. cpu  
      var ax = Abs(x); var ay = Abs(y);
      if (ax > ay || IsNaN(ay)) return x;
      if (ax == ay) return IsNegative(x) ? y : x;
      return y;
    }
    public static Float<T> MaxNumber(Float<T> x, Float<T> y)
    { //todo: opt. cpu  
      if (x != y) { if (!IsNaN(y)) return y < x ? x : y; return x; }
      return IsNegative(y) ? x : y;
    }
    public static Float<T> MinNumber(Float<T> x, Float<T> y)
    { //todo: opt. cpu  
      if (x != y) { if (!IsNaN(y)) return x < y ? x : y; return x; }
      return IsNegative(x) ? x : y;
    }
    public static Float<T> Truncate(Float<T> a)
    {
      var cpu = main_cpu; var e = cpu.fpush(&a, desc);
      cpu.shl(e); cpu.fpop(&a, 0, desc); return a;
    }
    public static Float<T> Ceiling(Float<T> x)
    {
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul(); //cpu.norm();
      cpu.rnd(0, cpu.sign() < 0 ? 0 : 4); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Floor(Float<T> x)
    {
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul(); //cpu.norm();
      cpu.rnd(0, cpu.sign() >= 0 ? 0 : 4); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Round(Float<T> x) => Round(x, 0, MidpointRounding.ToEven);
    public static Float<T> Round(Float<T> x, int digits) => Round(x, digits, MidpointRounding.ToEven);
    public static Float<T> Round(Float<T> x, MidpointRounding mode) => Round(x, 0, mode);
    public static Float<T> Round(Float<T> x, int digits, MidpointRounding mode)
    {
      //if (mode == MidpointRounding.ToPositiveInfinity || mode == MidpointRounding.ToNegativeInfinity) { } //todo: sign,...
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul(); //cpu.norm();
      cpu.rnd(digits, mode == MidpointRounding.ToEven ? 2 : mode == MidpointRounding.ToZero ? 0 : 1);
      cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Pow(Float<T> x, Float<T> y)
    {
      var s = Sign(x); if (s == 0) return default; //todo: opt. cpu
      if (s < 0) { if (IsInteger(y)) return Round(Pow(x, (int)y)); return Float<T>.NaN; }
      return Exp(Log(x) * y);
      // var cpu = main_cpu; var c = prec(digits);
      // cpu.push(x); cpu.log(c);
      // cpu.push(y); cpu.mul(); cpu.exp(c);
      // cpu.rnd(digits); return cpu.popr();
    }
    public static Float<T> Pow10(int y) //todo: opt. cpu, for now as test
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
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul();
      cpu.sqrt(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Cbrt(Float<T> x)
    {
      return Pow(x, (Float<T>)1 / 3); //todo: opt
    }
    public static Float<T> RootN(Float<T> x, int n)
    {
      return Pow(x, (Float<T>)1 / n); //todo: opt
    }
    public static Float<T> Hypot(Float<T> x, Float<T> y)
    {
      return Sqrt(x * x + y * y); //todo: opt
      //var cpu = main_cpu;
      //cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul(); cpu.sqr();
      //cpu.pow(2, cpu.fpush(&y, desc)); cpu.mul(); cpu.sqr(); cpu.add();
      //cpu.sqrt(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> ScaleB(Float<T> x, int n)
    {
      var cpu = main_cpu; var e = cpu.fpush(&x, desc); // x * 2^n
      cpu.fpop(&x, e + n, desc); return x;
    }
    public static Float<T> Ieee754Remainder(Float<T> x, Float<T> y)
    {
      var a = x % y; if (IsNaN(a)) return NaN; //todo: opt. cpu
      if (a == default && IsNegative(x)) return NegativeZero;
      var b = a - Abs(y) * Sign(x);
      if (Abs(b) == Abs(a)) { y = Round(x = x / y); return Abs(y) > Abs(x) ? b : a; }
      return Abs(b) < Abs(a) ? b : a;
    }
    public static Float<T> Exp(Float<T> x)
    {
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul();
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
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul();
      cpu.log(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Log2(Float<T> x)
    {
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul();
      cpu.log2(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Log10(Float<T> x)
    {
      return Log2(x) / Log2(10);
    }
    public static Float<T> Log(Float<T> x, Float<T> newBase)
    {
      return Log(x) / Log(newBase);
    }
    public static int ILogB(Float<T> x)
    {
      var cpu = main_cpu; var e = cpu.fpush(&x, desc); var m = cpu.msb();
      cpu.pop(); return m != 0 ? e + unchecked((int)m) - 1 : int.MinValue; //nan: MaxValue
    }
    public static Float<T> FusedMultiplyAdd(Float<T> left, Float<T> right, Float<T> addend)
    {
      return left * right + addend; //todo: opt. cpu
    }
    public static Float<T> BitIncrement(Float<T> x)
    {
      var c = desc & 0xffff; var s = ((byte*)&x)[sizeof(T) - 1] & 0x80;
      if (s == 0) CPU.inc(&x, c); else CPU.dec(&x, c); return x;
      //var cpu = main_cpu; cpu.ipush(&x, desc & 0xffff);
      //var s = cpu.sign(); cpu.push(s == 0 ? 1 : s); cpu.add();
      //cpu.ipop(&x, desc & 0xffff); return x;
    }
    public static Float<T> BitDecrement(Float<T> x)
    {
      var c = desc & 0xffff; var s = ((byte*)&x)[c - 1] & 0x80;
      if (s == 0 && x == default) { ((byte*)&x)[c - 1] = 0x80; ((byte*)&x)[0] = 1; return x; }
      if (s == 0) CPU.dec(&x, c); else CPU.inc(&x, c); return x;
      //var cpu = main_cpu; cpu.ipush(&x, desc & 0xffff);
      //var s = cpu.sign(); cpu.push(s == 0 ? -1 : s); cpu.sub();
      //cpu.ipop(&x, desc & 0xffff); return s == 0 ? -x : x;
    }

    public static Float<T> Sin(Float<T> x)
    {
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul();
      cpu.sin(unchecked((uint)(desc >> 16) + 2), false); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Cos(Float<T> x)
    {
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul();
      cpu.sin(unchecked((uint)(desc >> 16) + 2), true); cpu.fpop(&x, desc); return x;
    }
    public static Float<T> Atan(Float<T> x)
    {
      var cpu = main_cpu; cpu.pow(2, cpu.fpush(&x, desc)); cpu.mul();
      cpu.atan(unchecked((uint)(desc >> 16) + 2)); cpu.fpop(&x, desc); return x;
    }
    #region todo cpu impl
    public static Float<T> Acos(Float<T> x)
    {
      return Atan(Sqrt(1 - x * x) / x); //todo: opt. cpu
    }
    public static Float<T> Asin(Float<T> x)
    {
      return Atan(x / Sqrt(1 - x * x)); //todo: opt. cpu
    }
    public static Float<T> AcosPi(Float<T> x)
    {
      return Acos(x) / Pi;
    }
    public static Float<T> AsinPi(Float<T> x)
    {
      return Asin(x) / Pi; //todo: opt. cpu
    }
    public static Float<T> AtanPi(Float<T> x) 
    {
      return Atan(x) / Pi; //todo: opt. cpu
    }
    public static Float<T> CosPi(Float<T> x) 
    {
      return Cos(x * Pi); //todo: opt. cpu
    }
    public static Float<T> SinPi(Float<T> x) 
    {
      return Sin(x * Pi); //todo: opt. cpu
    }
    public static Float<T> Tan(Float<T> x) 
    {
      return Sin(x) / Cos(x); //todo: inline
    }
    public static Float<T> TanPi(Float<T> x) 
    {
      return Tan(x * Pi); //todo: opt. cpu
    }
    public static (Float<T> Sin, Float<T> Cos) SinCos(Float<T> x) 
    {
      return (Sin(x), Cos(x)); //todo: opt. cpu
    }
    public static (Float<T> SinPi, Float<T> CosPi) SinCosPi(Float<T> x) 
    {
      x *= Pi; return (Sin(x), Cos(x)); //todo: opt. cpu
    }
    public static Float<T> Atan2(Float<T> y, Float<T> x) 
    {
      if (x > 0) return 2 * Atan(y / (Sqrt(x * x + y * y) + x));
      else if (y != 0) return 2 * Atan((Sqrt(x * x + y * y) - x) / y);
      else if (x < 0 && y == 0) return Pi;
      else return NaN;
    }
    public static Float<T> Atan2Pi(Float<T> y, Float<T> x) 
    {
      return Atan2(y, x) / Pi; //todo: opt. cpu
    }
    public static Float<T> Acosh(Float<T> x) 
    {
      return Log(x + Sqrt(x * x - 1)); //todo: opt. cpu
    }
    public static Float<T> Asinh(Float<T> x) 
    {
      return Log(x + Sqrt(x * x + 1)); //todo: opt. cpu
    }
    public static Float<T> Atanh(Float<T> x) 
    {
      return Log((1 + x) / (1 - x)) / 2; //todo: opt. cpu
    }
    public static Float<T> Cosh(Float<T> x) 
    {
      return (Exp(x) + Exp(-x)) / 2; //todo: opt. cpu
    }
    public static Float<T> Sinh(Float<T> x) 
    {
      return (Exp(x) - Exp(-x)) / 2; //todo: opt. cpu
    }
    public static Float<T> Tanh(Float<T> x) 
    {
      return 1 - 2 / (Exp(x * 2) + 1); //todo: opt. cpu
    }
    #endregion

    public readonly override int GetHashCode()
    {
      var a = this; return CPU.hash(&a, sizeof(T));
    }
    public readonly override bool Equals([NotNullWhen(true)] object? obj)
    {
      if (obj is not Float<T> b) return false;
      var a = this; return CPU.fequ(&a, &b, desc);
    }
    public readonly bool Equals(Float<T> other) { var t = this; return CPU.fequ(&t, &other, desc); }
    public readonly int CompareTo(object? obj) => obj == null ? 1 : p is Float<T> b ? this.CompareTo(b) : throw new ArgumentException();
    public readonly int CompareTo(Float<T> other) { var a = this; return CPU.fcmp(&a, &other, desc); }

    public readonly override string ToString() => ToString(null, null);
    public string ToString(string? format) => ToString(format, null);
    public string ToString(IFormatProvider? formatProvider) => ToString(null, formatProvider);
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

    public static Float<T> Parse(string s) => Parse(s.AsSpan(), NumberStyles.Float | NumberStyles.AllowThousands, null);
    public static Float<T> Parse(string s, NumberStyles style) => Parse(s.AsSpan(), style, null);
    public static Float<T> Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), NumberStyles.Float | NumberStyles.AllowThousands, null);
    public static Float<T> Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s.AsSpan(), style, provider);
    public static Float<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
    public static Float<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
      var info = provider != null ? NumberFormatInfo.GetInstance(provider) : null;
      var cpu = main_cpu; cpu.tor(s, 10, info != null ? info.NumberDecimalSeparator[0] : default);
      Float<T> x; cpu.fpop(&x, desc); return x;
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Float<T> result)
    {
      var info = provider != null ? NumberFormatInfo.GetInstance(provider) : null;
      var cpu = main_cpu; cpu.tor(s, 10, info != null ? info.NumberDecimalSeparator[0] : default);
      Float<T> x; cpu.fpop(&x, desc); result = x; return true;
    }
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Float<T> result) => TryParse(s.AsSpan(), style, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Float<T> result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);
    public static bool TryParse(string? s, IFormatProvider? provider, out Float<T> result) => TryParse(s.AsSpan(), NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, out Float<T> result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, out Float<T> result) => TryParse(s.AsSpan(), NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

    #region private
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly T p;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static int desc = CPU.fdesc(sizeof(T));
    #endregion

#if NET6_0
    public static Float<T> CreateTruncating<TOther>(TOther value) where TOther : struct
    {
      Float<T> a; if (main_cpu.cast(value, out a, 0)) return a;
      throw new NotSupportedException();
    }
    public static Float<T> CreateSaturating<TOther>(TOther value) where TOther : struct
    {
      Float<T> a; if (main_cpu.cast(value, out a, 1)) return a;
      throw new NotSupportedException();
    }
    public static Float<T> CreateChecked<TOther>(TOther value) where TOther : struct
    {
      Float<T> a; if (main_cpu.cast(value, out a, 2)) return a;
      throw new NotSupportedException();
    }
#endif
#if NET7_0
    public static Float<T> CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      Float<T> a; if (main_cpu.cast(value, out a, 0) || TOther.TryConvertToTruncating(value, out a)) return a;
      throw new NotSupportedException();
    }
    public static Float<T> CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      Float<T> a; if (main_cpu.cast(value, out a, 1) || TOther.TryConvertToSaturating(value, out a)) return a;
      throw new NotSupportedException();
    }
    public static Float<T> CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
      Float<T> a; if (main_cpu.cast(value, out a, 2) || TOther.TryConvertToChecked(value, out a)) return a;
      throw new NotSupportedException();
    }
#endif
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
    static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator &(Float<T> left, Float<T> right)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 1), i = 0; i < n; i++)
        ((ushort*)&left)[i] &= ((ushort*)&right)[i];
      return left;
    }
    static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator |(Float<T> left, Float<T> right)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 1), i = 0; i < n; i++)
        ((ushort*)&left)[i] |= ((ushort*)&right)[i];
      return left;
    }
    static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator ^(Float<T> left, Float<T> right)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 1), i = 0; i < n; i++)
        ((ushort*)&left)[i] ^= ((ushort*)&right)[i];
      return left;
    }
    static Float<T> IBitwiseOperators<Float<T>, Float<T>, Float<T>>.operator ~(Float<T> value)
    {
      for (uint n = unchecked((uint)sizeof(T) >> 1), i = 0; i < n; i++)
        ((ushort*)&value)[i] = (ushort)(~((ushort*)&value)[i]);
      return value;
    }
    static bool INumberBase<Float<T>>.TryConvertFromTruncating<TOther>(TOther value, out Float<T> result) => main_cpu.cast(value, out result, 0);
    static bool INumberBase<Float<T>>.TryConvertFromSaturating<TOther>(TOther value, out Float<T> result) => main_cpu.cast(value, out result, 1);
    static bool INumberBase<Float<T>>.TryConvertFromChecked<TOther>(TOther value, out Float<T> result) => main_cpu.cast(value, out result, 2);
    static bool INumberBase<Float<T>>.TryConvertToTruncating<TOther>(Float<T> value, out TOther result) where TOther : default => main_cpu.cast(value, out result, 0);
    static bool INumberBase<Float<T>>.TryConvertToSaturating<TOther>(Float<T> value, out TOther result) where TOther : default => main_cpu.cast(value, out result, 1);
    static bool INumberBase<Float<T>>.TryConvertToChecked<TOther>(Float<T> value, out TOther result) where TOther : default => main_cpu.cast(value, out result, 2);
    int IFloatingPoint<Float<T>>.GetExponentByteCount() => (((desc >> 16) + 1) >> 5) + 1; //sizeof(sbyte), sizeof(short), 4;
    int IFloatingPoint<Float<T>>.GetSignificandBitLength() => (desc >> 16) + 1; // 53
    int IFloatingPoint<Float<T>>.GetSignificandByteCount() => sizeof(T);
    int IFloatingPoint<Float<T>>.GetExponentShortestBitLength()
    {
      var t = this; var e = CPU.fexpo(&t, desc);
      return e >= 0 ? 32 - int.LeadingZeroCount(e) : 33 - int.LeadingZeroCount(~e);
    }
    bool IFloatingPoint<Float<T>>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      var c = sizeof(T); c = c < 8 ? 1 : c < 16 ? 2 : 4; //todo: check 16
      if (destination.Length < c) { bytesWritten = 0; return false; }
      var t = this; var e = CPU.fexpo(&t, desc); var s = new Span<byte>(&e, c);
      s.CopyTo(destination); bytesWritten = c; return true;
    }
    bool IFloatingPoint<Float<T>>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten)
    {
      var c = sizeof(T); c = c < 8 ? 1 : c < 16 ? 2 : 4; //todo: check 16
      if (destination.Length < c) { bytesWritten = 0; return false; }
      var t = this; var e = CPU.fexpo(&t, desc); var s = new Span<byte>(&e, c); s.Reverse();
      s.CopyTo(destination); bytesWritten = c; return true;
    }
    bool IFloatingPoint<Float<T>>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      var c = sizeof(T); if (destination.Length < c) { bytesWritten = 0; return false; }
      var d = destination.Slice(0, bytesWritten = c); if (this == default) { d.Clear(); return true; }
      var cpu = main_cpu; var t = this; cpu.fpush(&t, desc);
      MemoryMarshal.Cast<uint, byte>(cpu.gets(cpu.mark() - 1)).Slice(4, c).CopyTo(d); cpu.pop();
      return true;
    }
    bool IFloatingPoint<Float<T>>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten)
    {
      var c = sizeof(T); if (destination.Length < c) { bytesWritten = 0; return false; }
      var d = destination.Slice(0, bytesWritten = c); if (this == default) { d.Clear(); return true; }
      var cpu = main_cpu; var t = this; cpu.fpush(&t, desc);
      MemoryMarshal.Cast<uint, byte>(cpu.gets(cpu.mark() - 1)).Slice(4, c).CopyTo(d); cpu.pop();
      d.Reverse(); return true;
    }

  }
#endif

}

