using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

#if NET7_0 // new number types, interfaces and extensions that will probably introduced in .NET7

#pragma warning disable CS1591 //todo: xml comment

namespace System.Numerics
{
  unsafe partial struct BigRational :
    INumber<BigRational>, ISignedNumber<BigRational>, //IConvertible, //todo: check IConvertible, does it makes much sens for non system types?
    IPowerFunctions<BigRational>, IRootFunctions<BigRational>, IExponentialFunctions<BigRational>,
    ILogarithmicFunctions<BigRational>, ITrigonometricFunctions<BigRational>, IHyperbolicFunctions<BigRational>
  {
    public static implicit operator BigRational(byte value)
    {
      return (uint)value;
    }
    public static implicit operator BigRational(sbyte value)
    {
      return (int)value;
    }
    public static implicit operator BigRational(ushort value)
    {
      return (uint)value;
    }
    public static implicit operator BigRational(char value)
    {
      return (uint)value;
    }
    public static implicit operator BigRational(short value)
    {
      return (int)value;
    }
    public static implicit operator BigRational(nint value)
    {
      return (long)value;
    }
    public static implicit operator BigRational(nuint value)
    {
      return (ulong)value;
    }
    public static implicit operator BigRational(Half value)
    {
      return (float)value;
    }
    public static implicit operator BigRational(Int128 value)
    {
      var p = (ulong*)&value; var s = (p[1] >> 63) != 0; if (s) value = -value;
      var cpu = task_cpu; cpu.push(p[0]); if (p[1] != 0) { cpu.push(p[1]); cpu.shl(64); cpu.or(); }
      if (s) cpu.neg(); return cpu.popr();
    }
    public static implicit operator BigRational(UInt128 value)
    {
      var cpu = task_cpu; var p = (ulong*)&value;
      cpu.push(p[0]); if (p[1] != 0) { cpu.push(p[1]); cpu.shl(64); cpu.or(); }
      return cpu.popr();
    }
    public static implicit operator BigRational(NFloat value)
    {
      // return nint.Size == 4 : (float)Math.Round(value.Value, 6) : value.Value; //todo: digits check for cases 5, 6, 7, regions ?
      return value.Value;
    }

    public static explicit operator byte(BigRational value)
    {
      return (byte)(uint)value;
    }
    public static explicit operator sbyte(BigRational value)
    {
      return (sbyte)(int)value;
    }
    public static explicit operator short(BigRational value)
    {
      return (short)(int)value;
    }
    public static explicit operator char(BigRational value)
    {
      return (char)(uint)value;
    }
    public static explicit operator Half(BigRational value)
    {
      return (Half)(double)value;
    }
    public static explicit operator nint(BigRational value)
    {
      return (nint)(long)value;
    }
    public static explicit operator nuint(BigRational value)
    {
      return (nuint)(ulong)value;
    }
    public static explicit operator Int128(BigRational value)
    {
      var a = default(Int128); if (value.p == null || IsNaN(value)) return a; //NaN like double
      var cpu = task_cpu; cpu.push(value); var s = cpu.sign();
      if (!cpu.isi()) { cpu.mod(); cpu.swp(); cpu.pop(); } // trunc
      var b = cpu.msb(); if (b > 127) { cpu.pop(); return s < 0 ? Int128.MinValue : Int128.MaxValue; } // like double
      cpu.get(cpu.mark() - 1, out ReadOnlySpan<uint> sp);
      sp.Slice(1, sp.Length - 3).CopyTo(new Span<uint>(&a, 4));
      cpu.pop(); if (s < 0) a = -a; return a;
    }
    public static explicit operator UInt128(BigRational value)
    {
      var a = default(UInt128); if (Sign(value) <= 0) return a; //NaN incl. like double
      var cpu = task_cpu; cpu.push(value);
      if (!cpu.isi()) { cpu.mod(); cpu.swp(); cpu.pop(); } // trunc
      var b = cpu.msb(); if (b > 128) { cpu.pop(); return UInt128.MaxValue; } // like double
      cpu.get(cpu.mark() - 1, out ReadOnlySpan<uint> sp);
      sp.Slice(1, sp.Length - 3).CopyTo(new Span<uint>(&a, 4));
      cpu.pop(); return a;
    }
    public static explicit operator NFloat(BigRational value)
    {
      return nint.Size == 4 ? new NFloat((float)value) : new NFloat((double)value);
    }

    public static int Radix => 1; //todo: check, Radix for rational?
    public static BigRational One => 1u;
    public static BigRational Zero => 0;
    public static BigRational NegativeOne => -1;
    public static BigRational AdditiveIdentity => 0;
    public static BigRational MultiplicativeIdentity => 1u;
    public static bool IsZero(BigRational value) => value.p == null;
    public static bool IsNegative(BigRational value) => Sign(value) < 0;
    public static bool IsPositive(BigRational value) => Sign(value) > 0;
    public static bool IsEvenInteger(BigRational value)
    {
      return value.p == null || IsInteger(value) && (value.p[1] & 1) == 0;
    }
    public static bool IsOddInteger(BigRational value)
    {
      return value.p != null && IsInteger(value) && (value.p[1] & 1) == 1;
    }
    public static bool IsCanonical(BigRational value) => true;
    public static bool IsComplexNumber(BigRational value) => true;
    public static bool IsFinite(BigRational value) => !IsNaN(value);
    public static bool IsImaginaryNumber(BigRational value) => false;
    public static bool IsInfinity(BigRational value) => false;
    public static bool IsNegativeInfinity(BigRational value) => false;
    public static bool IsPositiveInfinity(BigRational value) => false;
    public static bool IsRealNumber(BigRational value) => true;
    public static bool IsNormal(BigRational value) => true;
    public static bool IsSubnormal(BigRational value) => false;

    static int cmpa(BigRational x, BigRational y)
    {
      var cpu = task_cpu; cpu.push(x); cpu.push(y);
      var i = cpu.cmpa(); cpu.pop(2); return i;
    }
    public static BigRational MaxMagnitude(BigRational x, BigRational y)
    {
      return IsNaN(x) ? x : IsNaN(y) ? y : cmpa(x, y) <= 0 ? x : y;
    }
    public static BigRational MaxMagnitudeNumber(BigRational x, BigRational y)
    {
      return IsNaN(x) ? y : IsNaN(x) ? x : cmpa(x, y) <= 0 ? x : y;
    }
    public static BigRational MinMagnitude(BigRational x, BigRational y)
    {
      return IsNaN(x) ? x : IsNaN(y) ? y : cmpa(x, y) >= 0 ? x : y;
    }
    public static BigRational MinMagnitudeNumber(BigRational x, BigRational y)
    {
      return IsNaN(x) ? y : IsNaN(x) ? x : cmpa(x, y) >= 0 ? x : y;
    }

    public static BigRational Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
      var f = style & NumberStyles.HexNumber; if (f != 0) throw new ArgumentException($"{nameof(s)} {f}"); //todo: hex parse 
      var r = Parse(s, provider); if (IsNaN(r)) throw new ArgumentException(nameof(s)); return r;
    }
    public static BigRational Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
      return Parse(s.AsSpan(), style, provider);
    }
    public static BigRational Parse(string s, IFormatProvider? provider)
    {
      var r = Parse(s.AsSpan(), provider);
      if (IsNaN(r)) throw new ArgumentException(nameof(s)); return r;
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out BigRational result)
    {
      var f = style & NumberStyles.HexNumber; if (f != 0) { result = default; return false; } //todo: hex parse 
      return !IsNaN(result = Parse(s, provider));
    }
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out BigRational result)
    {
      return !IsNaN(result = Parse(s.AsSpan(), provider));
    }
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BigRational result)
    {
      return !IsNaN(result = Parse(s, provider));
    }
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BigRational result)
    {
      if (s == null) { result = default; return false; }
      return !IsNaN(result = Parse(s, provider));
    }

    //INumberBase //todo: check !!!
    static bool INumberBase<BigRational>.TryConvertFromChecked<TOther>(TOther value, out BigRational result) //where TOther : INumberBase<TOther>
    {
      return TryConvertFrom<TOther>(value, out result);
    }
    static bool INumberBase<BigRational>.TryConvertFromSaturating<TOther>(TOther value, out BigRational result) //where TOther : INumberBase<TOther>
    {
      return TryConvertFrom<TOther>(value, out result);
    }
    static bool INumberBase<BigRational>.TryConvertFromTruncating<TOther>(TOther value, out BigRational result) //where TOther : INumberBase<TOther>
    {
      return TryConvertFrom<TOther>(value, out result);
    }
    static bool TryConvertFrom<TOther>(TOther value, out BigRational result) where TOther : INumberBase<TOther>
    {
      if (typeof(TOther) == typeof(Half)) { result = (Half)(object)value; return true; }
      if (typeof(TOther) == typeof(short)) { result = (short)(object)value; return true; }
      if (typeof(TOther) == typeof(int)) { result = (int)(object)value; return true; }
      if (typeof(TOther) == typeof(long)) { result = (long)(object)value; return true; }
      if (typeof(TOther) == typeof(Int128)) { result = (Int128)(object)value; return true; }
      if (typeof(TOther) == typeof(nint)) { result = (nint)(object)value; return true; }
      if (typeof(TOther) == typeof(sbyte)) { result = (sbyte)(object)value; return true; }
      if (typeof(TOther) == typeof(float)) { result = (float)(object)value; return true; }
      result = default; return false;
    }
    static bool INumberBase<BigRational>.TryConvertToChecked<TOther>(BigRational value, [NotNullWhen(true)] out TOther? result) where TOther : default
    {
      if (typeof(TOther) == typeof(byte)) { result = (TOther)(object)checked((byte)value); return true; }
      if (typeof(TOther) == typeof(char)) { result = (TOther)(object)checked((char)value); return true; }
      if (typeof(TOther) == typeof(decimal)) { result = (TOther)(object)checked((decimal)value); return true; }
      if (typeof(TOther) == typeof(ushort)) { result = (TOther)(object)checked((ushort)value); return true; }
      if (typeof(TOther) == typeof(uint)) { result = (TOther)(object)checked((uint)value); return true; }
      if (typeof(TOther) == typeof(ulong)) { result = (TOther)(object)checked((ulong)value); return true; }
      if (typeof(TOther) == typeof(UInt128)) { result = (TOther)(object)checked((UInt128)value); return true; }
      if (typeof(TOther) == typeof(nuint)) { result = (TOther)(object)checked((nuint)value); return true; }
      result = default!; return false;
    }
    static bool INumberBase<BigRational>.TryConvertToSaturating<TOther>(BigRational value, [NotNullWhen(true)] out TOther? result) where TOther : default
    {
      return TryConvertTo<TOther>(value, out result);
    }
    static bool INumberBase<BigRational>.TryConvertToTruncating<TOther>(BigRational value, [NotNullWhen(true)] out TOther? result) where TOther : default
    {
      return TryConvertTo<TOther>(value, out result);
    }
    static bool TryConvertTo<TOther>(BigRational value, [NotNullWhen(true)] out TOther result) where TOther : INumberBase<TOther>
    {
      if (typeof(TOther) == typeof(byte))
      {
        byte x = (value >= byte.MaxValue) ? byte.MaxValue : (value <= byte.MinValue) ? byte.MinValue : (byte)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(char))
      {
        char x = (value >= char.MaxValue) ? char.MaxValue : (value <= char.MinValue) ? char.MinValue : (char)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(decimal))
      {
        decimal x = (value >= +79228162514264337593543950336.0) ? decimal.MaxValue :
                               (value <= -79228162514264337593543950336.0) ? decimal.MinValue :
                               IsNaN(value) ? 0.0m : (decimal)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(ushort))
      {
        ushort x = (value >= ushort.MaxValue) ? ushort.MaxValue : (value <= ushort.MinValue) ? ushort.MinValue : (ushort)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(uint))
      {
        uint x = (value >= uint.MaxValue) ? uint.MaxValue : (value <= uint.MinValue) ? uint.MinValue : (uint)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(ulong))
      {
        ulong x = (value >= ulong.MaxValue) ? ulong.MaxValue : (value <= ulong.MinValue) ? ulong.MinValue : IsNaN(value) ? 0 : (ulong)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(UInt128))
      {
        UInt128 x = (value >= 340282366920938463463374607431768211455.0) ? UInt128.MaxValue : (value <= 0.0) ? UInt128.MinValue : (UInt128)value;
        result = (TOther)(object)x; return true;
      }
      if (typeof(TOther) == typeof(nuint))
      {
#if TARGET_64BIT
        nuint actualResult = (value >= ulong.MaxValue) ? unchecked((nuint)ulong.MaxValue) :
                             (value <= ulong.MinValue) ? unchecked((nuint)ulong.MinValue) : (nuint)value;
        result = (TOther)(object)actualResult;
        return true;
#else
        nuint actualResult = (value >= uint.MaxValue) ? uint.MaxValue :
                             (value <= uint.MinValue) ? uint.MinValue : (nuint)value;
        result = (TOther)(object)actualResult;
        return true;
#endif
      }
      result = default!; return false;
    }
    static BigRational INumberBase<BigRational>.CreateChecked<TOther>(TOther value) //where TOther : INumberBase<TOther>
    {
      if (typeof(TOther) == typeof(BigRational)) return (BigRational)(object)value;
      if (!TryConvertFrom<TOther>(value, out BigRational r) && !TOther.TryConvertToChecked(value, out r) )
        throw new NotSupportedException(typeof(TOther).Name);
      return r;
    }
    static BigRational INumberBase<BigRational>.CreateSaturating<TOther>(TOther value) // where TOther : INumberBase<TOther>
    {
      TryConvertFrom<TOther>(value, out var r); return r;
    }
    static BigRational INumberBase<BigRational>.CreateTruncating<TOther>(TOther value) //where TOther : INumberBase<TOther>
    {
      TryConvertFrom<TOther>(value, out var r); return r;
    }

    //INumber
    public static BigRational Clamp(BigRational value, BigRational min, BigRational max)
    {
      if (min > max) throw new ArgumentException($"{nameof(min)} {nameof(max)}");
      if (value < min) return min;
      if (value > max) return max;
      return value;
    }
    public static BigRational CopySign(BigRational value, BigRational sign)
    {
      int a, b; return (a = Sign(value)) != 0 && (b = IsNegative(sign) ? -1 : +1) != 0 && a != b ? -value : value;
    }

    //IPowerFunctions
    public static BigRational Pow(BigRational x, BigRational y)
    {
      return Pow(x, y, DefaultDigits); //todo: opt. cpu
    }

    //IRootFunctions
    public static BigRational Sqrt(BigRational x)
    {
      return Sqrt(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Cbrt(BigRational x)
    {
      return Pow(x, (BigRational)1 / 3); //todo: opt. cpu
    }
    public static BigRational Hypot(BigRational x, BigRational y)
    {
      //return Sqrt(x * x + y * y);
      var cpu = task_cpu; var d = DefaultDigits; var c = prec(d);
      cpu.push(x); cpu.sqr(); cpu.push(y); cpu.sqr(); cpu.add(); //todo: lim x^2, y^2 and check
      cpu.sqrt(c); cpu.rnd(d); return cpu.popr();
    }
    public static BigRational Root(BigRational x, int n)
    {
      return Pow(x, (BigRational)1 / n); //todo: opt. cpu
    }

    //IExponentialFunctions
    public static BigRational Exp(BigRational x)
    {
      return Exp(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Exp2(BigRational x)
    {
      return Pow(2, x, DefaultDigits); //todo: impl
    }
    public static BigRational Exp10(BigRational x)
    {
      return Pow(10, x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational ExpM1(BigRational x)
    {
      return Exp(x, DefaultDigits) - 1; //todo: opt. cpu
    }
    public static BigRational Exp2M1(BigRational x)
    {
      return Pow(2, x, DefaultDigits) - 1; //todo: opt. cpu
    }
    public static BigRational Exp10M1(BigRational x)
    {
      return Exp10(x) - 1; //todo: opt. cpu
    }

    //ILogarithmicFunctions
    public static BigRational Log(BigRational x)
    {
      return Log(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Log(BigRational x, BigRational newBase)
    {
      return double.Log((double)x, (double)newBase); //todo: impl
    }
    public static BigRational Log2(BigRational x)
    {
      return Log2(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Log10(BigRational x)
    {
      return Log10(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational LogP1(BigRational x)
    {
      return Log(x + 1); //todo: opt. cpu
    }
    public static BigRational Log10P1(BigRational x)
    {
      return Log10(x + 1); //todo: opt. cpu
    }
    public static BigRational Log2P1(BigRational x)
    {
      return Log2(x + 1); //todo: opt. cpu
    }

    //for double compat
    public static BigRational ILogB(BigRational x)
    {
      return (int)Log2(x); //todo: ILog2 alg
    }

    //ITrigonometricFunctions
    public static BigRational Sin(BigRational x)
    {
      return Sin(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Cos(BigRational x)
    {
      return Cos(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Tan(BigRational x)
    {
      return Tan(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Asin(BigRational x)
    {
      return Asin(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Acos(BigRational x)
    {
      return Acos(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Atan(BigRational x)
    {
      return Atan(x, DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Atan2(BigRational y, BigRational x)
    {
      return Atan2(y, x, DefaultDigits); //todo: opt. cpu
    }
    public static (BigRational Sin, BigRational Cos) SinCos(BigRational x)
    {
      return (Sin(x, DefaultDigits), Cos(x, DefaultDigits)); //todo: opt. cpu
    }
    public static BigRational AcosPi(BigRational x)
    {
      return Acos(x, DefaultDigits) / Pi(DefaultDigits); //todo: opt. cpu
    }
    public static BigRational AsinPi(BigRational x)
    {
      return Asin(x, DefaultDigits) / Pi(DefaultDigits); //todo: opt. cpu
    }
    public static BigRational Atan2Pi(BigRational y, BigRational x)
    {
      return Atan2(y, x, DefaultDigits) / Pi(DefaultDigits); //todo: opt. cpu
    }
    public static BigRational AtanPi(BigRational x)
    {
      return Atan(x, DefaultDigits) / Pi(DefaultDigits); //todo: opt. cpu
    }
    public static BigRational CosPi(BigRational x)
    {
      return Cos(x * Pi(DefaultDigits), DefaultDigits); //todo: opt. cpu
    }
    public static BigRational SinPi(BigRational x)
    {
      return Sin(x * Pi(DefaultDigits), DefaultDigits); //todo: opt. cpu
    }
    public static BigRational TanPi(BigRational x)
    {
      return Tan(x * Pi(DefaultDigits), DefaultDigits); //todo: opt. cpu
    }

    //IHyperbolicFunctions
    public static BigRational Asinh(BigRational x)
    {
      return Log(x + Sqrt(x * x + 1)); //todo: opt. cpu
    }
    public static BigRational Acosh(BigRational x)
    {
      return Log(x + Sqrt(x * x - 1)); //todo: opt. cpu
    }
    public static BigRational Atanh(BigRational x)
    {
      return Log((1 + x) / (1 - x)) / 2; //todo: opt. cpu
    }
    public static BigRational Sinh(BigRational x)
    {
      return (Exp(x) - Exp(-x)) / 2; //todo: opt. cpu
    }
    public static BigRational Cosh(BigRational x)
    {
      return (Exp(x) + Exp(-x)) / 2; //todo: opt. cpu
    }
    public static BigRational Tanh(BigRational x)
    {
      return 1 - 2 / (Exp(x * 2) + 1); //todo: opt. cpu
    }
  }
}

#endif

