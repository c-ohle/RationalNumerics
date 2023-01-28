
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
    public Float(Half value) => this = value;

    public static implicit operator Float<T>(int value)
    {
      var cpu = main_cpu; cpu.push(value);
      Float<T> a; cpu.fpop<Float<T>>(&a, 0); return a;
    }
    public static implicit operator Float<T>(long value)
    {
      var cpu = main_cpu; cpu.push(value);
      Float<T> a; cpu.fpop<Float<T>>(&a, 0); return a;
    }
    public static implicit operator Float<T>(Half value)
    {
      Float<T> a; main_cpu.fcast<Float<T>, Half>(&a, &value); return a;
    }
    public static implicit operator Float<T>(float value)
    {
      //if(sizeof(T) == 4) return *(Float<T>*)&value;
      Float<T> a; main_cpu.fcast<Float<T>, float>(&a, &value); return a;
    }
    public static implicit operator Float<T>(double value)
    {
      Float<T> a; main_cpu.fcast<Float<T>, double>(&a, &value); return a;
    }
    public static implicit operator Float<T>(decimal value)
    {
      var cpu = main_cpu; cpu.push(value);
      Float<T> x; cpu.fpop<Float<T>>(&x); return x;
    }
    public static explicit operator Float<T>(BigRational value)
    {
      var cpu = main_cpu; cpu.push(value);
      Float<T> x; cpu.fpop<Float<T>>(&x); return x;
    }

    public static explicit operator int(Float<T> value)
    {
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&value);
      int a; cpu.ipop(&a, sizeof(int)); return a;
    }
    public static explicit operator long(Float<T> value)
    {
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&value);
      long a; cpu.ipop(&a, sizeof(long)); return a;
    }
    public static explicit operator Half(Float<T> value)
    {
      Half c; main_cpu.fcast<Half, Float<T>>(&c, &value); return c;
    }
    public static explicit operator float(Float<T> value)
    {
      float c; main_cpu.fcast<float, Float<T>>(&c, &value); return c;
      //var cpu = main_cpu; var e = cpu.__fpush<Float<T>>(&value);
      //float c; cpu.__fpop<float>(&c, e); return c;
    }
    public static explicit operator double(Float<T> value)
    {
      double c; main_cpu.fcast<double, Float<T>>(&c, &value); return c;
      //var cpu = main_cpu; var e = cpu.__fpush<Float<T>>(&value);
      //double c; cpu.__fpop<double>(&c, e); return c;
    }
    public static explicit operator decimal(Float<T> value)
    {
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&value);
      return cpu.popm();
    }
    public static implicit operator BigRational(Float<T> value)
    {
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&value); return cpu.popr();
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
      int u = cpu.fpush<Float<T>>(&left);
      int v = cpu.fpush<Float<T>>(&right.p), l = u < v ? 1 : 0, e;
      cpu.shr(l == 0 ? (e = u) - v : (e = v) - u, l);
      cpu.add(); cpu.fpop<Float<T>>(&left, e); return left;
    }
    public static Float<T> operator -(Float<T> left, Float<T> right)
    {
      var cpu = main_cpu;
      int u = cpu.fpush<Float<T>>(&left);
      int v = cpu.fpush<Float<T>>(&right), l = u < v ? 1 : 0, e;
      cpu.shr(l == 0 ? (e = u) - v : (e = v) - u, l);
      cpu.sub(); cpu.fpop<Float<T>>(&left, e); return left;
    }
    public static Float<T> operator *(Float<T> left, Float<T> right)
    {
      var cpu = main_cpu;
      int u = cpu.fpush<Float<T>>(&left);
      int v = cpu.fpush<Float<T>>(&right); cpu.mul();
      cpu.fpop<Float<T>>(&left, u + v); return left;
    }
    public static Float<T> operator /(Float<T> left, Float<T> right)
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; var bi = (desc >> 16) + 2;
      var u = cpu.fpush<Float<T>>(&left); cpu.shl(bi);
      var v = cpu.fpush<Float<T>>(&right); cpu.idiv();
      cpu.fpop<Float<T>>(&left, u - v - bi); return left;
    }
    public static Float<T> operator %(Float<T> left, Float<T> right)
    {
      return left - Truncate(left / right) * right; //todo: opt. cpu
    }

    public static bool operator ==(Float<T> left, Float<T> right) => CPU.fequ<Float<T>>(&left, &right);
    public static bool operator !=(Float<T> left, Float<T> right) => !CPU.fequ<Float<T>>(&left, &right);
    public static bool operator >=(Float<T> left, Float<T> right) => CPU.fcmp<Float<T>>(&left, &right) >= 0;
    public static bool operator <=(Float<T> left, Float<T> right) => CPU.fcmp<Float<T>>(&left, &right) <= 0;
    public static bool operator >(Float<T> left, Float<T> right) => CPU.fcmp<Float<T>>(&left, &right) > 0;
    public static bool operator <(Float<T> left, Float<T> right) => CPU.fcmp<Float<T>>(&left, &right) < 0;

    public static int BitCount => sizeof(Float<T>) << 3;
    public static int MaxDigits
    {
      get
      {
        var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
        return unchecked((int)(((desc >> 16) + 1) * 0.30103f) + 1);
      }
    }
    public static Float<T> MinValue
    {
      get { return -MaxValue; }
    }
    public static Float<T> MaxValue
    {
      get
      {
        var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
        int size = desc & 0xffff, bc = desc >> 16, ec = (size << 3) - bc;
        Float<T> a; new Span<uint>(&a, size >> 2).Fill(0xffffffff);
        *(uint*)(((byte*)&a) + (size - 4)) = 0x7fffffff ^ (1u << (32 - ec)); return a;
      }
    }
    public static Float<T> Epsilon
    {
      get { var a = default(Float<T>); *(uint*)&a |= 1; return a; }
    }
    public static Float<T> NaN
    {
      get { Float<T> a; CPU.fnan<Float<T>>(&a, 0); return a; }
    }
    public static Float<T> NegativeInfinity
    {
      get { Float<T> a; CPU.fnan<Float<T>>(&a, 1); return a; }
    }
    public static Float<T> PositiveInfinity
    {
      get { Float<T> a; CPU.fnan<Float<T>>(&a, 2); return a; }
    }
    public static Float<T> NegativeZero
    {
      get
      {
        var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
        Float<T> a = default; int size = desc & 0xffff; (((byte*)&a)[size - 1]) = 0x80; return a;
      }
    }

    public static Float<T> E
    {
      get
      {
        var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
        var cpu = main_cpu; cpu.push(1u); cpu.exp(unchecked((uint)(desc >> 16) + 2));
        Float<T> x; cpu.fpop<Float<T>>(&x); return x;
      }
    }
    public static Float<T> Pi
    {
      get
      {
        var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
        var cpu = main_cpu; cpu.pi(unchecked((uint)(desc >> 16) + 2));
        Float<T> x; cpu.fpop<Float<T>>(&x); return x;
      }
    }
    public static Float<T> Tau
    {
      get
      {
        var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
        var cpu = main_cpu; cpu.pi(unchecked((uint)(desc >> 16) + 2)); cpu.shl(1);
        Float<T> x; cpu.fpop<Float<T>>(&x); return x;
      }
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
      var e = CPU.ftest<Float<T>>(&value); return e == 0;
    }
    public static bool IsNaN(Float<T> value)
    {
      var e = CPU.ftest<Float<T>>(&value); return e == 1;
    }
    public static bool IsRealNumber(Float<T> value)
    {
      var e = CPU.ftest<Float<T>>(&value); return e != 1;
    }
    public static bool IsInfinity(Float<T> value)
    {
      var e = CPU.ftest<Float<T>>(&value); return e == 2 || e == 3;
    }
    public static bool IsNegativeInfinity(Float<T> value)
    {
      var e = CPU.ftest<Float<T>>(&value); return e == 2;
    }
    public static bool IsPositiveInfinity(Float<T> value)
    {
      var e = CPU.ftest<Float<T>>(&value); return e == 3;
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
      if (value == default) return false;
      var e = CPU.ftest<Float<T>>(&value); return e == 0;
    }
    public static bool IsSubnormal(Float<T> value)
    {
      return false; //todo: check
    }
    public static bool IsPow2(Float<T> value)
    {
      var cpu = main_cpu; cpu.fpush<Float<T>>(&value);
      var r = cpu.sign() > 0 && cpu.ipt(); cpu.pop(); return r;
    }

    public static Float<T> Abs(Float<T> value)
    {
      //return value < 0 ? -value : value;
      *(uint*)(((byte*)&value) + (sizeof(Float<T>) - 4)) &= 0x7fffffff; return value;
    }
    public static Float<T> Min(Float<T> x, Float<T> y)
    {
      return CPU.fcmp<Float<T>>(&x, &y) <= 0 ? x : y;
    }
    public static Float<T> Max(Float<T> x, Float<T> y)
    {
      return CPU.fcmp<Float<T>>(&x, &y) >= 0 ? x : y;
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
      var cpu = main_cpu; var e = cpu.fpush<Float<T>>(&a);
      cpu.shl(e); cpu.fpop<Float<T>>(&a, 0); return a;
    }
    public static Float<T> Ceiling(Float<T> x)
    {
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x); //cpu.norm();
      cpu.rnd(0, cpu.sign() < 0 ? 0 : 4); cpu.fpop<Float<T>>(&x); return x;
    }
    public static Float<T> Floor(Float<T> x)
    {
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);//cpu.norm();
      cpu.rnd(0, cpu.sign() >= 0 ? 0 : 4); cpu.fpop<Float<T>>(&x); return x;
    }
    public static Float<T> Round(Float<T> x) => Round(x, 0, MidpointRounding.ToEven);
    public static Float<T> Round(Float<T> x, int digits) => Round(x, digits, MidpointRounding.ToEven);
    public static Float<T> Round(Float<T> x, MidpointRounding mode) => Round(x, 0, mode);
    public static Float<T> Round(Float<T> x, int digits, MidpointRounding mode)
    {
      //if (mode == MidpointRounding.ToPositiveInfinity || mode == MidpointRounding.ToNegativeInfinity) { } //todo: sign,...
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x); //cpu.norm();
      cpu.rnd(digits, mode == MidpointRounding.ToEven ? 2 : mode == MidpointRounding.ToZero ? 0 : 1);
      cpu.fpop<Float<T>>(&x); return x;
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
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);
      cpu.sqrt(unchecked((uint)(desc >> 16) + 2)); cpu.fpop<Float<T>>(&x); return x;
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
      //cpu.sqrt(unchecked((uint)(desc >> 16) + 2)); cpu.__fpop<Float<T>>(&x); return x;
    }
    public static Float<T> ScaleB(Float<T> x, int n)
    {
      var cpu = main_cpu; var e = cpu.fpush<Float<T>>(&x); // x * 2^n
      cpu.fpop<Float<T>>(&x, e + n); return x;
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
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);
      cpu.exp(unchecked((uint)(desc >> 16) + 2)); cpu.fpop<Float<T>>(&x); return x;
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
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);
      cpu.log(unchecked((uint)(desc >> 16) + 2)); cpu.fpop<Float<T>>(&x); return x;
    }
    public static Float<T> Log2(Float<T> x)
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);
      cpu.log2(unchecked((uint)(desc >> 16) + 2)); cpu.fpop<Float<T>>(&x); return x;
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
      var cpu = main_cpu; var e = cpu.fpush<Float<T>>(&x); var m = cpu.msb();
      cpu.pop(); return m != 0 ? e + unchecked((int)m) - 1 : CPU.ftest<Float<T>>(&x) != 1 ? int.MinValue : int.MaxValue; //nan: MaxValue
    }
    public static Float<T> FusedMultiplyAdd(Float<T> left, Float<T> right, Float<T> addend)
    {
      return left * right + addend; //todo: opt. cpu
    }
    public static Float<T> BitIncrement(Float<T> x)
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var c = desc & 0xffff; var s = ((byte*)&x)[sizeof(T) - 1] & 0x80;
      if (s == 0) CPU.inc(&x, c); else CPU.dec(&x, c); return x;
      //var cpu = main_cpu; cpu.ipush(&x, desc & 0xffff);
      //var s = cpu.sign(); cpu.push(s == 0 ? 1 : s); cpu.add();
      //cpu.ipop(&x, desc & 0xffff); return x;
    }
    public static Float<T> BitDecrement(Float<T> x)
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var c = desc & 0xffff; var s = ((byte*)&x)[c - 1] & 0x80;
      if (s == 0 && x == default) { ((byte*)&x)[c - 1] = 0x80; ((byte*)&x)[0] = 1; return x; }
      if (s == 0) CPU.dec(&x, c); else CPU.inc(&x, c); return x;
      //var cpu = main_cpu; cpu.ipush(&x, desc & 0xffff);
      //var s = cpu.sign(); cpu.push(s == 0 ? -1 : s); cpu.sub();
      //cpu.ipop(&x, desc & 0xffff); return s == 0 ? -x : x;
    }

    public static Float<T> Sin(Float<T> x)
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);
      cpu.sin(unchecked((uint)(desc >> 16) + 2), false); cpu.fpop<Float<T>>(&x); return x;
    }
    public static Float<T> Cos(Float<T> x)
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);
      cpu.sin(unchecked((uint)(desc >> 16) + 2), true); cpu.fpop<Float<T>>(&x); return x;
    }
    public static Float<T> Atan(Float<T> x)
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);
      cpu.atan(unchecked((uint)(desc >> 16) + 2)); cpu.fpop<Float<T>>(&x); return x;
    }
    #region todo cpu impl after test's
    public static Float<T> Acos(Float<T> x)
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);
      cpu.acos(unchecked((uint)(desc >> 16) + 2));
      cpu.fpop<Float<T>>(&x); return x;
    }
    public static Float<T> Asin(Float<T> x)
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      var cpu = main_cpu; cpu.fpushr<Float<T>>(&x);
      cpu.asin(unchecked((uint)(desc >> 16) + 2));
      cpu.fpop<Float<T>>(&x); return x;
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
      var a = this; return CPU.fequ<Float<T>>(&a, &b);
    }
    public readonly bool Equals(Float<T> other) { var t = this; return CPU.fequ<Float<T>>(&t, &other); }
    public readonly int CompareTo(object? obj) => obj == null ? 1 : p is Float<T> b ? this.CompareTo(b) : throw new ArgumentException();
    public readonly int CompareTo(Float<T> other) { var a = this; return CPU.fcmp<Float<T>>(&a, &other); }

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
        if (d) dig = stoi(format.Slice(1)); //int.Parse(format.Slice(1));
        if (f == 'E') { rnd = dig; if (rnd == 0 && !d) rnd = 6; dig = rnd + 1; }
        if (f == 'F') { rnd = dig; if (rnd == 0 && !d) rnd = info.NumberDecimalDigits; dig = 0; }
      }

      if (dig == 0) { dig = MaxDigits; if (dbg) dig += 0;/* dig++ */ } //dbg like float G9, double G17         
      if (dest.Length < dig + 16) { dig += 16; goto ex; }
      var cpu = main_cpu; var val = this;
      var nan = CPU.ftest<Float<T>>(&val);
      if (nan != 0)
      {
        var s = nan == 1 ? info.NaNSymbol : nan == 2 ? info.NegativeInfinitySymbol : info.PositiveInfinitySymbol;
        s.CopyTo(dest); charsWritten = s.Length; return true;
      }
      int e = cpu.fpush<Float<T>>(&val), es = 0;
      if (cpu.sign() != 0)
      {
        //var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
        //if (!dbg)
        //{
        Debug.Assert(dbg || dig >= 0);
        if (abs(e) >= 128)
        {
          var expo = (int)(CPU.fexpo<Float<T>>(&val) * 0.30103f);
          cpu.pop(); val *= Pow10(-(es = expo - dig));
          e = cpu.fpush<Float<T>>(&val); Debug.Assert(abs(e) < 128);
        }
        //}
        //else
        //{
        //  var ep = (int)((e + (desc >> 16)) * 0.30103f); //var d1 = ep < 0 ? -ep : ep; var d2 = dig < 0 ? -dig : dig; var dd = d1 - d2;
        //  var d1 = __abs(ep); var d2 = __abs(dig); var dd = d1 - d2;
        //  if (dd > 10) //todo: opt. F? check
        //  {
        //    if (ep == 0) { }
        //    else if (ep > 0)
        //    {
        //      cpu.pop(); val *= Pow10(-(es = dd - 3));
        //      e = cpu.fpush<Float<T>>(&val);
        //    }
        //    else if (ep < 0)
        //    {
        //      cpu.pop(); val *= Pow10(-(es = -(dd - 3)));
        //      e = cpu.fpush<Float<T>>(&val);
        //    }
        //    else { }
        //  }
        //}
        cpu.pow(2, e); cpu.mul();
      }

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
      Float<T> x; cpu.fpop<Float<T>>(&x); return x;
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Float<T> result)
    {
      var info = provider != null ? NumberFormatInfo.GetInstance(provider) : null;
      var cpu = main_cpu; cpu.tor(s, 10, info != null ? info.NumberDecimalSeparator[0] : default);
      Float<T> x; cpu.fpop<Float<T>>(&x); result = x; return true;
    }
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Float<T> result) => TryParse(s.AsSpan(), style, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Float<T> result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);
    public static bool TryParse(string? s, IFormatProvider? provider, out Float<T> result) => TryParse(s.AsSpan(), NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);
    public static bool TryParse(ReadOnlySpan<char> s, out Float<T> result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, out Float<T> result) => TryParse(s.AsSpan(), NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

    #region private
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly T p;
    //[DebuggerBrowsable(DebuggerBrowsableState.Never)] private static int desc = CPU.fdesc(sizeof(T));
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
#if NET7_0_OR_GREATER
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

#if NET7_0_OR_GREATER
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
    int IFloatingPoint<Float<T>>.GetExponentByteCount()
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      return (((desc >> 16) + 1) >> 5) + 1; //sizeof(sbyte), sizeof(short), 4 ???
    }
    int IFloatingPoint<Float<T>>.GetSignificandBitLength()
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      return (desc >> 16) + 1; // 53
    }
    int IFloatingPoint<Float<T>>.GetSignificandByteCount()
    {
      var desc = CPU.tdesc<Float<T>>.desc & 0x0fffffff;
      return desc & 0xffff;
    }
    int IFloatingPoint<Float<T>>.GetExponentShortestBitLength()
    {
      var t = this; var e = CPU.fexpo<Float<T>>(&t);
      return e >= 0 ? 32 - int.LeadingZeroCount(e) : 33 - int.LeadingZeroCount(~e);
    }
    bool IFloatingPoint<Float<T>>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      var c = sizeof(T); c = c < 8 ? 1 : c < 16 ? 2 : 4; //todo: check 16
      if (destination.Length < c) { bytesWritten = 0; return false; }
      var t = this; var e = CPU.fexpo<Float<T>>(&t); var s = new Span<byte>(&e, c);
      s.CopyTo(destination); bytesWritten = c; return true;
    }
    bool IFloatingPoint<Float<T>>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten)
    {
      var c = sizeof(T); c = c < 8 ? 1 : c < 16 ? 2 : 4; //todo: check 16
      if (destination.Length < c) { bytesWritten = 0; return false; }
      var t = this; var e = CPU.fexpo<Float<T>>(&t); var s = new Span<byte>(&e, c); s.Reverse();
      s.CopyTo(destination); bytesWritten = c; return true;
    }
    bool IFloatingPoint<Float<T>>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten)
    {
      var c = sizeof(T); if (destination.Length < c) { bytesWritten = 0; return false; }
      var d = destination.Slice(0, bytesWritten = c); if (this == default) { d.Clear(); return true; }
      var cpu = main_cpu; var t = this; cpu.fpush<Float<T>>(&t);
      MemoryMarshal.Cast<uint, byte>(cpu.gets(cpu.mark() - 1)).Slice(4, c).CopyTo(d); cpu.pop();
      return true;
    }
    bool IFloatingPoint<Float<T>>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten)
    {
      var c = sizeof(T); if (destination.Length < c) { bytesWritten = 0; return false; }
      var d = destination.Slice(0, bytesWritten = c); if (this == default) { d.Clear(); return true; }
      var cpu = main_cpu; var t = this; cpu.fpush<Float<T>>(&t);
      MemoryMarshal.Cast<uint, byte>(cpu.gets(cpu.mark() - 1)).Slice(4, c).CopyTo(d); cpu.pop();
      d.Reverse(); return true;
    }
  }
#endif

}

