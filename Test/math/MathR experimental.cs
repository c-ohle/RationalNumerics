
namespace System.Numerics.Rational
{
  /// <summary>
  /// MathR experimental
  /// </summary>
  public static class MathRE
  {
    /// <summary>
    /// PI calculation based on Bellard's formula.<br/>
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/Bellard%27s_formula<br/>
    /// <i>Just to create big numbers with lots of controllable digits.</i>
    /// </remarks>
    /// <param name="digits">The number of decimal digits to calculate.</param>
    public static BigRational Pi(int digits)
    {
      var cpu = BigRational.task_cpu; cpu.push();
      for (int n = 0, c = 1 + digits / 3; n < c; n++)
      {
        int a = n << 2, b = 10 * n;
        cpu.pow(-1, n); cpu.pow(2, b); cpu.div();
        cpu.push(-32);  /**/ cpu.push(a + 1); cpu.div();
        cpu.push(-1);   /**/ cpu.push(a + 3); cpu.div(); cpu.add(); //cpu.norm();
        cpu.push(256u); /**/ cpu.push(b + 1); cpu.div(); cpu.add(); //cpu.norm();
        cpu.push(-64);  /**/ cpu.push(b + 3); cpu.div(); cpu.add(); //cpu.norm();
        cpu.push(-4);   /**/ cpu.push(b + 5); cpu.div(); cpu.add(); //cpu.norm();
        cpu.push(-4);   /**/ cpu.push(b + 7); cpu.div(); cpu.add(); //cpu.norm();
        cpu.push(1u);   /**/ cpu.push(b + 9); cpu.div(); cpu.add(); //cpu.norm();
        cpu.mul(); cpu.add();
        if ((n & 0x3) == 0x3)
          cpu.norm();
      }
      cpu.push(64); cpu.div(); cpu.rnd(digits);
      return cpu.popr();
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
      s = s.Trim().Trim("[]").Trim();
      var cpu = BigRational.task_cpu; cpu.push();
      for (; s.Length != 0;)
      {
        var x = s.LastIndexOfAny(",;");
        var d = x != -1 ? s.Slice(x + 1).Trim() : s; s = s.Slice(0, x != -1 ? x : 0);
        if (cpu.sign() != 0) cpu.inv();
        //or tor: cpu.push(0); for (int i = 0; i < d.Length; i++) { cpu.push(10u); cpu.mul(); cpu.push(d[i] - '0'); cpu.add(); }      
        cpu.tor(d); cpu.add();
      }
      return cpu.popr();
    }

  }
}
