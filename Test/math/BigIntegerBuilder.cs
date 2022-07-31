using System.Diagnostics.CodeAnalysis;
using Test;
using static System.Numerics.BigRational;
using static Test.DX11ModelCtrl.Models.Scene;

#if NET7_0

namespace System.Numerics.Rational
{
  /// <summary>
  /// A type 1 BigIntegerBuilder
  /// </summary>
  /// <remarks>
  /// Properties:<br/>
  /// * Definitly the most simple, flexible, memory efficient and performant solution.<br/> 
  /// * Function list not complete, only to show the system, can easily extend to mutch more than BigInteger currently can.<br/> 
  /// * For .NET7 and numbers with little bit more than 32 bit - about 20% faster than using the internal BigInteger.Calculator<br/>
  /// * For all functions: IndexOutOfRangeException if called without enough params on stack<br/>
  /// Negatives:<br/>
  /// * The user need to understand the stack concept.<br/>
  /// Security:<br/>
  /// * This builder has a private instance of a BigRational.CPU - no static refs are in use.<br/>
  /// * It represents simply a class that the GC can collect if no more referenced.<br/>
  /// * One instance can be used in several threads, user managed using lock(builder) {} etc.
  /// </remarks>
  public class BigIntegerBuilder1
  {
    // inital capacity 8 - for integer calculations more than enough
    readonly BigRational.CPU cpu = new BigRational.CPU(8);

    // for debug only: not mutch effort, simply show stack as list
    public override string ToString()
    {
      return string.Join("; ", Enumerable.Range(0, (int)cpu.mark()).Reverse().
        Select(i => { cpu.get((uint)i, out BigInteger r); return r.ToString(); }));
    }

    // after exceptions or for the case someone lose control 
    public void Clear() { cpu.pop((int)cpu.mark()); }

    public void Push(BigInteger value) { cpu.push(value); }

    // for integer types > Int32 explicitly, since the implicite BigInteger casts would include allocs
    public void Push(int value) { cpu.push(value); } // but then it needs int32 too - to avoid ambigous ...
    public void Push(uint value) { cpu.push(value); }
    public void Push(long value) { cpu.push(value); }
    public void Push(ulong value) { cpu.push(value); }
    public void Push(Int128 value) { cpu.push(value); }
    public void Push(UInt128 value) { cpu.push(value); }
    // and so on...

    // binary ops     
    public void Add() { cpu.add(); }
    public void Subtract() { cpu.sub(); }
    public void Multiply() { cpu.mul(); }
    public void Divide()
    {
      if (cpu.sign() == 0) throw new DivideByZeroException(); //BigInteger behavior 
      cpu.div(); cpu.rnd(0, 0); // trunc
    }
    // and so on...

    // unary ops
    public void Negate() { cpu.neg(); }
    public void Pow(int exponent) { cpu.pow(exponent); }
    public void Sqr() { cpu.sqr(); }
    public void ShiftLeft(int count) { cpu.shl(checked((uint)count)); }
    public void ShiftRight(int count) { cpu.shr(checked((uint)count)); }
    // and so on...

    // ops necessary to support vector calculations or complex ones using temps etc. 
    public void Swap(int a, int b) { cpu.swp(a, b); }
    public void Duplicate() { cpu.dup(); }

    // fetch, the one and only final alloc (if the cpu was already in use)
    public BigInteger PopResult()
    {
      cpu.get(cpu.mark() - 1, out BigInteger r);
      cpu.pop(); return r;
    }
  }

  /// <summary>
  /// A type 2 BigIntegerBuilder
  /// </summary>
  /// <remarks>
  /// * System, Security, all like for BigIntegerBuilder1<br/> 
  /// * Just to show the system, assuming BigIntegerBuilder2 params always the same, div 0 not handled etc.<br/>
  /// Positives:<br/>
  /// * Syntax similiar to standard notation: a * b + c * d -> Add(Multiply(a, b), Multiply(c, d));<br/>
  /// * Syntax very restricted but safe and user dont need to understand the stack concept behind.<br/>
  /// Negatives:<br/>
  /// * To get the same memory efficency like for BigIntegerBuilder1 it would need thousands of function parameter versions.<br/>
  /// * More complex calculations, vectors or with temps nearly impossible.<br/>
  /// </remarks>
  public class BigIntegerBuilder2
  {
    // inital capacity 8 - for integer calculations more than enough
    readonly BigRational.CPU cpu = new BigRational.CPU(8);

    // for debug: nothing to show at runtime because of the syntax restictions; otherwise always empty
    public override string? ToString() => base.ToString();

    // binary ops
    public BigIntegerBuilder2 Add(BigInteger a, BigInteger b) { cpu.push(a); cpu.push(b); cpu.add(); return this; }
    public BigIntegerBuilder2 Add(BigIntegerBuilder2 a, BigInteger b) { cpu.push(b); cpu.add(); return this; }
    public BigIntegerBuilder2 Add(BigInteger a, BigIntegerBuilder2 b) { cpu.push(a); cpu.add(); return this; }
    public BigIntegerBuilder2 Add(BigIntegerBuilder2 a, BigIntegerBuilder2 b) { cpu.add(); return this; }
    public BigIntegerBuilder2 Substract(BigInteger a, BigInteger b) { cpu.push(a); cpu.push(b); cpu.sub(); return this; }
    public BigIntegerBuilder2 Substract(BigIntegerBuilder2 a, BigInteger b) { cpu.push(b); cpu.sub(); return this; }
    public BigIntegerBuilder2 Substract(BigInteger a, BigIntegerBuilder2 b) { cpu.push(a); cpu.swp(); cpu.sub(); return this; }
    public BigIntegerBuilder2 Substract(BigIntegerBuilder2 a, BigIntegerBuilder2 b) { cpu.sub(); return this; }
    public BigIntegerBuilder2 Multiply(BigInteger a, BigInteger b) { cpu.push(a); cpu.push(b); cpu.mul(); return this; }
    public BigIntegerBuilder2 Multiply(BigIntegerBuilder2 a, BigInteger b) { cpu.push(b); cpu.mul(); return this; }
    public BigIntegerBuilder2 Multiply(BigInteger a, BigIntegerBuilder2 b) { cpu.push(a); cpu.mul(); return this; }
    public BigIntegerBuilder2 Multiply(BigIntegerBuilder2 a, BigIntegerBuilder2 b) { cpu.mul(); return this; }
    public BigIntegerBuilder2 Divide(BigInteger a, BigInteger b) { cpu.push(a); cpu.push(b); cpu.div(); cpu.rnd(0, 0); return this; }
    public BigIntegerBuilder2 Divide(BigIntegerBuilder2 a, BigInteger b) { cpu.push(b); cpu.div(); cpu.rnd(0, 0); return this; }
    public BigIntegerBuilder2 Divide(BigInteger a, BigIntegerBuilder2 b) { cpu.push(a); cpu.swp(); cpu.div(); cpu.rnd(0, 0); return this; }
    public BigIntegerBuilder2 Divide(BigIntegerBuilder2 a, BigIntegerBuilder2 b) { cpu.div(); cpu.rnd(0, 0); return this; }
    // and so on...

    // unary ops
    public BigIntegerBuilder2 Negate(BigInteger a) { cpu.push(a); cpu.neg(); return this; }
    public BigIntegerBuilder2 Negate(BigIntegerBuilder2 a) { cpu.neg(); return this; }
    public BigIntegerBuilder2 Pow(BigInteger a, int exponent) { cpu.push(a); cpu.pow(exponent); return this; }
    public BigIntegerBuilder2 Pow(BigIntegerBuilder2 a, int exponent) { cpu.pow(exponent); return this; }
    // and so on...

    // fetch, the one and only final alloc (if the cpu was already in use)
    public BigInteger ToBigInteger()
    {
      cpu.get(cpu.mark() - 1, out BigInteger r); cpu.pop(); return r;
    }

    // eg. after exceptions
    public void Clear() { cpu.pop((int)cpu.mark()); }

  }

  /// <summary>
  /// A type 3 BigIntegerBuilder
  /// </summary>
  /// <remarks>
  /// Properties:<br/>
  /// * System, Security, all like for BigIntegerBuilder1 and BigIntegerBuilder2<br/> 
  /// * Based on a fast Expressions compilation it allows hig-optimized specialized typesafe function sets for exactly the cases that needed.<br/>
  /// * BigIntegerBuilder3 is small, the ExpressionsCompiler currently 4k in-assembly size provides much more functionality than needed.<br/>
  /// * Without debug support full templates and Linq support - it could be small private BigIntegerBuilder3.ExpressionsCompiler version.<br/>
  /// * Or, to support INumber, with less effort could be extended to support static interfaces with correct inlining.<br/>
  /// * Builder librarys could be load from any sources, online etc.<br/>
  /// Negatives:<br/>
  /// * Users would need to write simple oldstyle C# code. This can be done in C# as draft but needs knowladge.<br/>
  /// * Debug only possible with a small VS extension I wrote, edit and continue currently not supported.
  /// </remarks>
  public class BigIntegerBuilder3
  {
    readonly object[] funcs;
    public BigIntegerBuilder3(string script)
    {
      var t1 = BigInteger.One; var t2 = BigRational.One; // to speed up compilation, ensure assemblys are loaded 
      var expr = ExpressionsCompiler.Compile(script);
      var ctor = expr.Compile();
      funcs = ctor(null); // simple static, single instance, could be compiled as BigIntegerBuilder3 extension with much more possebilities
    }
    public T GetFunc<T>(string name) where T : MulticastDelegate
    {
      //this as slow but readable accessor, should be for(...) loop and/or with dictionary for bigger libs
      return funcs.OfType<T>().First(f => f.Method.Name == name);
    }
  }

#if false //new one in BigInt.cs using SafeCPU as builder ()
  /// <summary>
  /// A type 4 BigIntegerBuilder
  /// </summary>
  /// <remarks>
  /// <see cref="BigInt"/> represents an arbitrarily large signed integer like <see cref="BigInteger"/>.<br/>
  /// It is only for testing the cooperation with <see cref="BigInt.Builder"/> and is not fully implemented.<br/>
  /// However, could easily extend to the <see cref="BigInteger"/> functionality with identical interface and much better performance.<br/>
  /// <see cref="BigInt.Builder"/> Properties:<br/>
  /// * As ref struct the builder shares the same properties like <see cref="BigRational.Builder"/> or <see cref="BigRational.SafeCPU"/>.<br/>
  /// * With implicit conversions from and to <see cref="BigInt"/> the default behavior is neutral.<br/>
  /// * However, applied in standard numeric expressions, the operators activate the stack-machine internal builder functionality in a secure way.<br/>
  /// </remarks>
  public readonly struct BigInt
  {
    public override string ToString() => ((BigInteger)p).ToString(); //todo
    public override int GetHashCode() => p.GetHashCode();
    public override bool Equals([NotNullWhen(true)] object? obj) => p.Equals(obj);
    public bool Equals(BigInt other) => p.Equals(other.p);
    public int CompareTo(BigInt other) => p.CompareTo(other.p);

    public BigInt(int value) => p = value;
    public BigInt(uint value) => p = value;
    public BigInt(long value) => p = value;
    public BigInt(ulong value) => p = value;
    public BigInt(float value) { var cpu = rat.task_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
    public BigInt(double value) { var cpu = rat.task_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
    public BigInt(decimal value) { var cpu = rat.task_cpu; cpu.push(value); cpu.rnd(0, 0); p = cpu.popr(); }
    //public BigInt(byte[] value) 
    //public BigInt(ReadOnlySpan<byte> value, bool isUnsigned = false, bool isBigEndian = false)

    public static implicit operator BigInt(long value) => new BigInt(value);
    public static implicit operator BigInt(Int128 value) => new BigInt(value);
    public static implicit operator BigInt(BigInteger value) => new BigInt(value);
    public static implicit operator BigRational(BigInt value) => value.p;
    public static explicit operator BigInt(BigRational value) => new BigInt(BigRational.Truncate(value));
    public static explicit operator BigInteger(BigInt value) => (BigInteger)value.p;

    public static BigInt operator +(BigInt a) => new BigInt(+a.p);
    public static BigInt operator -(BigInt a) => new BigInt(-a.p);
    public static BigInt operator +(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.add(a.p, b.p); return new BigInt(cpu); }
    public static BigInt operator -(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.sub(a.p, b.p); return new BigInt(cpu); }
    public static BigInt operator *(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.mul(a.p, b.p); return new BigInt(cpu); }
    public static BigInt operator /(BigInt a, BigInt b)
    {
      var cpu = rat.task_cpu;
      cpu.push(a.p); var t1 = cpu.msb();
      cpu.push(b.p); var t2 = cpu.msb();
      if (t2 > t1) { cpu.pop(2); cpu.push(); return new BigInt(cpu); }
      cpu.idiv(); return new BigInt(cpu);
    }
    public static BigInt operator <<(BigInt a, int b) { var cpu = rat.task_cpu; var c = checked((uint)b); cpu.push(a.p); cpu.shl(c); return new BigInt(cpu); }
    public static BigInt operator >>(BigInt a, int b) { var cpu = rat.task_cpu; var c = checked((uint)b); cpu.push(a.p); cpu.shr(c); return new BigInt(cpu); }
    public static BigInt operator &(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.and(); return new BigInt(cpu); }
    public static BigInt operator ^(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.xor(); return new BigInt(cpu); }
    //public static BigInt operator |(BigInt a, BigInt b) { var cpu = rat.task_cpu; cpu.push(a.p); cpu.push(b.p); cpu.or(); return new BigInt(cpu); }

    public static bool operator <(BigInt a, BigInt b) => a.CompareTo(b) < 0;
    public static bool operator <=(BigInt a, BigInt b) => a.CompareTo(b) <= 0;
    public static bool operator >(BigInt a, BigInt b) => a.CompareTo(b) > 0;
    public static bool operator >=(BigInt a, BigInt b) => a.CompareTo(b) >= 0;
    public static bool operator ==(BigInt a, BigInt b) => a.Equals(b);
    public static bool operator !=(BigInt a, BigInt b) => !a.Equals(b);

    public static BigInt Pow(BigInt value, int exponent) => new BigInt(BigRational.Pow(value.p, exponent));
    public static BigInt Negate(BigInt a) => new BigInt(-a.p);

    readonly BigRational p; BigInt(BigRational p) => this.p = p;
    BigInt(BigRational.SafeCPU cpu) => p = cpu.popr();

  #region boost operator 
    /// <summary>
    /// Performs a bitwise Or operation on two <see cref="BigInt"/> values.
    /// </summary>
    /// <param name="a">Left value.</param>
    /// <param name="b">Right value.</param>
    /// <returns>The result of the bitwise Or operation.</returns>
    public static BigInt operator |(BigInt? a, BigInt b) => new BigInt((BigRational?)a.GetValueOrDefault().p | b.p);
    public static implicit operator BigInt?(int value) => new BigInt(((BigRational?)value).GetValueOrDefault());
  #endregion

  #region ref struct builder
    //public readonly ref struct Builder
    //{
    //  readonly BigRational.Builder p;
    //  public override string ToString() => p.ToString();
    //
    //  public static implicit operator Builder(BigInt v) { return new Builder(v.p); }
    //
    //  public static Builder operator +(Builder a, Builder b) => new Builder(a.p + b.p);
    //  public static Builder operator -(Builder a, Builder b) => new Builder(a.p - b.p);
    //  public static Builder operator *(Builder a, Builder b) => new Builder(a.p * b.p);
    //  public static Builder operator /(Builder a, Builder b) => new Builder(a.p / b.p);
    //
    //  public static Builder operator +(Builder a, BigInt b) => new Builder(a.p + b.p);
    //  public static Builder operator -(Builder a, BigInt b) => new Builder(a.p - b.p);
    //  public static Builder operator *(Builder a, BigInt b) => new Builder(a.p * b.p);
    //  public static Builder operator /(Builder a, BigInt b) => new Builder(a.p / b.p); 
    //
    //  Builder(rat p) => this.p = p;
    //  Builder(BigRational.Builder p) => this.p = p;
    //  public static implicit operator BigInt(Builder v) => new BigInt((BigRational)v.p);
    //}
  #endregion
  }
#endif

  public static class TestBigIntegerBuilder
  {
    public static void TestBuilder()
    {
      TestType1();
      TestType2();
      TestType3();
      TestType4();
    }

    public static void TestType1()
    {
      BigInteger a = 10, b = 20, c = 30, d = 40, x;

      var builder = new BigIntegerBuilder1();

      builder.Push(BigInteger.One);
      builder.Push(BigInteger.MinusOne);
      builder.Add();
      x = builder.PopResult();

      // the case: x = a * b + c * d;
      builder.Push(a);
      builder.Push(b);
      builder.Multiply(); // a * b
      builder.Push(c);
      builder.Push(d);
      builder.Multiply(); // c * d
      builder.Add();      // (a * b) + (c * d) 
      x = builder.PopResult();

      builder.Push(Int128.MaxValue);
      builder.Push(-1000);
      builder.Multiply();
      x = builder.PopResult();

      builder.Push(1000);
      builder.Push(123);
      builder.Divide();
      x = builder.PopResult();

      builder.Push(10); // 10 ^ 100
      builder.Pow(100);
      x = builder.PopResult();

      // // div 0 test -> exception
      // builder.Push(10);
      // builder.Push(0);
      // try { builder.Divide(); result = builder.PopResult(); }
      // catch (Exception e) { Debug.WriteLine(e.Message); builder.Clear(); }

      builder.Clear();
    }

    public static void TestType2()
    {
      var builder = new BigIntegerBuilder2();
      BigInteger a = 10, b = 20, c = 30, d = 40, x;

      // x = a * b + c * d; 
      x = builder.Add(builder.Multiply(a, b), builder.Multiply(c, d)).ToBigInteger();

      // x = a * b + c; 
      x = builder.Add(builder.Multiply(a, b), c).ToBigInteger();

    }

    public static void TestType3()
    {
      // some simple examples only:
      var builder = new BigIntegerBuilder3("""
        using System;
        using System.Numerics;
        
        BigRational.CPU cpu = new BigRational.CPU((uint)8); // 8 enough for integer calculations
        
        public BigInteger Add(BigInteger a, int b) 
        {
          cpu.push(a); cpu.push(b); 
          return pop_result();                            
        }

        // a * b + c * d
        public BigInteger MulAddMul(BigInteger a, BigInteger b, BigInteger c, BigInteger d) 
        {
          cpu.push(a); cpu.push(b); cpu.mul(); 
          cpu.push(c); cpu.push(d); cpu.mul(); cpu.add(); 
          return pop_result();                            
        }

        public BigInteger Int128BigMul(Int128 a, Int128 b) 
        {
          cpu.push(a); cpu.push(b); cpu.mul(); 
          return pop_result();                            
        }
          
        BigInteger pop_result() // helper
        {
          BigInteger r; cpu.get(cpu.mark() - 1, out r); cpu.pop(); return r;
        }
        """);

      BigInteger a = 10, b = 20, c = 30, d = 40, x;

      // x = a * b + c * d; 
      var MulAddMul = builder.GetFunc<Func<BigInteger, BigInteger, BigInteger, BigInteger, BigInteger>>("MulAddMul");
      x = MulAddMul(a, b, c, d);

      var Int128BigMul = builder.GetFunc<Func<Int128, Int128, BigInteger>>("Int128BigMul");
      x = Int128BigMul(Int128.MaxValue, Int128.MaxValue);
    }

    public static void TestType4()
    {
      BigInt a = 10, b = 20, c = 30, d = 40, x;

      var xx = (BigInteger)a; xx *= xx;

      x = a * b + c * d; 
      x = 0 | a * b + c * d;
          
      a = Int128.MaxValue; b = Int128.MinValue; c = long.MaxValue; d = long.MinValue;
      x = a * b + c * d;
      x = 0 | a * b + c * d;

      a = 10; b = 20; c = 32; d = 40;

      x = a * b + c * d + BigInt.Pow(d, 100) + 1234;
      x = 0 | a * b + c * d + BigInt.Pow(d, 100) + 1234;

    }
  }
}

#endif