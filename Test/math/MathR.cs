
namespace System.Numerics.Rational
{
  /// <summary>
  /// Implementation of some common mathematical functions for <see cref="BigRational"/>.<br/>
  /// <i>These are just non-optimal example implementations for testing!</i>
  /// </summary>
  public static class MathR
  {
    /// <summary>
    /// Returns the numerator of the specified number.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number</param>
    /// <returns>Returns the numerator of <paramref name="a"/>.</returns>
    public static BigRational Numerator(BigRational a)
    {
      var cpu = BigRational.task_cpu; cpu.push(a);
      cpu.mod(8); var s = cpu.sign(); cpu.pop();
      if (s < 0) cpu.neg(); return cpu.pop_rat();
    }
    /// <summary>
    /// Returns the denominator of the specified number.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number</param>
    /// <returns>Returns the denominator of <paramref name="a"/>.</returns>
    public static BigRational Denominator(BigRational a)
    {
      var cpu = BigRational.task_cpu; cpu.push(a);
      cpu.mod(8); var s = cpu.sign(); cpu.swp(); cpu.pop();
      if (s < 0) cpu.neg(); return cpu.pop_rat();
    }
    /// <summary>
    /// Calculates the factorial of <paramref name="a"/>.
    /// </summary>
    /// <param name="a">A positive number.</param>
    /// <returns>Returns the factorial of <paramref name="a"/>.</returns>
    public static BigRational Factorial(int a)
    {
      if (a < 0) throw new ArgumentException();
      var cpu = BigRational.task_cpu; cpu.fac((uint)a);
      return cpu.pop_rat();
    }
    /// <summary>
    /// PI calculation based on Bellard's formula.<br/>
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/Bellard%27s_formula<br/>
    /// <i>Just to create big numbers with lots of controllable digits.</i>
    /// </remarks>
    /// <param name="digits">The number of decimal digits to calculate.</param>
    public static BigRational PI(int digits)
    {
      var cpu = BigRational.task_cpu; cpu.push();
      for (int n = 0, c = 1 + digits / 3; n < c; n++)
      {
        int a = n << 2, b = 10 * n;
        cpu.pow(-1, n); cpu.pow(2, b); cpu.div();
        cpu.push(-32);  /**/ cpu.push(a + 1); cpu.div();
        cpu.push(-1);   /**/ cpu.push(a + 3); cpu.div(); cpu.add();
        cpu.push(256u); /**/ cpu.push(b + 1); cpu.div(); cpu.add();
        cpu.push(-64);  /**/ cpu.push(b + 3); cpu.div(); cpu.add();
        cpu.push(-4);   /**/ cpu.push(b + 5); cpu.div(); cpu.add();
        cpu.push(-4);   /**/ cpu.push(b + 7); cpu.div(); cpu.add();
        cpu.push(1u);   /**/ cpu.push(b + 9); cpu.div(); cpu.add();
        cpu.mul(); cpu.add(); if ((n & 0x3) == 0x3) cpu.norm();
      }
      cpu.push(64); cpu.div(); cpu.rnd(digits);
      return cpu.pop_rat();
    }
    /// <summary>
    /// Converts a <see cref="BigRational"/> number to a continued fraction<br/>
    /// to the common string format: "[1;2,3,4,5]"
    /// </summary>
    /// <param name="a">The number to convert.</param>
    /// <returns>A <see cref="string"/> formatted as continued fraction.</returns>
    public static string GetContinuedFraction(BigRational a)
    {
      var wr = new System.Buffers.ArrayBufferWriter<char>(256);
      wr.GetSpan(1)[0] = '['; wr.Advance(1);
      var cpu = BigRational.task_cpu; cpu.push(a);
      for (int i = 0, e, ns; ; i++)
      {
        cpu.dup(); cpu.mod(0); cpu.swp(); cpu.pop();
        if (i != 0) { wr.GetSpan(1)[0] = i == 1 ? ';' : ','; wr.Advance(1); }
        for (int c = 1; ; c <<= 1)
        {
          var ws = wr.GetSpan(c); cpu.dup(); cpu.tos(ws, out ns, out e, out _, false);
          if (ns < ws.Length) { wr.Advance(ns); break; }
        }
        ns = e + 1 - ns; if (ns > 0) { wr.GetSpan(ns).Fill('0'); wr.Advance(ns); }
        cpu.sub(); if (cpu.sign() == 0) { cpu.pop(); break; }
        cpu.inv();
      }
      wr.GetSpan(1)[0] = ']'; wr.Advance(1);
      return wr.WrittenSpan.ToString();
    }
    /// <summary>
    /// Parses and calculate a rational number from a continued fraction<br/>
    /// of the common string format: "[1;2,3,4,5]"
    /// </summary>
    /// <param name="s">The value to parse.</param>
    /// <returns>A <see cref="BigRational"/> number.</returns>
    public static BigRational ParseContinuedFraction(ReadOnlySpan<char> s)
    {
      var cpu = BigRational.task_cpu; cpu.push();
      for (; s.Length != 0;)
      {
        var x = s.LastIndexOfAny(",;");
        var d = x != -1 ? s.Slice(x + 1).Trim() : s; s = s.Slice(0, x != -1 ? x : 0);
        if (cpu.sign() != 0) cpu.inv();
        cpu.push(0);
        for (int i = 0; i < d.Length; i++)
        {
          cpu.push(10); cpu.mul();
          cpu.push(d[i] - '0'); cpu.add();
        }
        cpu.add();
      }
      return cpu.pop_rat();
    }
    /// <summary>
    /// Finds the greatest common divisor (GCD) of two <see cref="BigRational"/> integer values.
    /// </summary>
    /// <remarks>
    /// This operation makes only sense for integer values.
    /// </remarks>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The greatest common divisor of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static BigRational GreatestCommonDivisor(BigRational a, BigRational b)
    {
      var cpu = BigRational.task_cpu; cpu.push(a); cpu.push(b);
      cpu.gcd(); return cpu.pop_rat();
    }
    /// <summary>
    /// Finds the least common multiple (LCM) of two <see cref="BigRational"/> integer values.
    /// </summary>
    /// <remarks>
    /// This operation makes only sense for integer values.
    /// </remarks>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The least common multiple of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static BigRational LeastCommonMultiple(BigRational a, BigRational b)
    {
      //|a * b| / gcd(a, b) == |a / gcd(a, b) * b|
      var cpu = BigRational.task_cpu; cpu.push(a); cpu.push(b);
      cpu.dup(); cpu.dup(2); cpu.gcd(); cpu.div(); cpu.mul(); cpu.abs();
      return cpu.pop_rat();
    }
  }
}
