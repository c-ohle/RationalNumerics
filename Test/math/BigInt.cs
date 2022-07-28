using System.Diagnostics.CodeAnalysis;
using Test;

namespace System.Numerics.Rational
{
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
#if NET7_0
    public static implicit operator BigInt(Int128 value) => new BigInt(value);
#endif
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
  }
}
