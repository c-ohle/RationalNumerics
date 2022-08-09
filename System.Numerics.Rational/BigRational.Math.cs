
namespace System.Numerics
{
  unsafe partial struct BigRational
  {
    /// <summary>
    /// Gets a <see cref="int"/> number that indicates the sign 
    /// (negative, positive, or zero) of a <see cref="BigRational"/> number.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number.</param>
    /// <returns>
    /// A <see cref="int"/> that indicates the sign of the <see cref="BigRational"/> number, as
    /// shown in the following table.<br/>
    /// Number – Description<br/>
    /// -1 – The value of the object is negative.<br/>
    ///  0 – The value of the object is 0 (zero).<br/>
    /// +1 – The value of the object is positive.<br/>
    /// </returns>
    public static int Sign(BigRational a)
    {
      return a.p == null ? 0 :
        (a.p[0] & 0x80000000) != 0 ? -1 :
        (a.p[0] & 0x3fffffff) == 1 && a.p[1] == 0 ? 0 : +1; //debug view 
    }
    /// <summary>
    /// Returns a value indicating whether the specified number is an integer. 
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number.</param>
    /// <returns><c>true</c> if <paramref name="a" /> is an integer; otherwise, <c>false</c>.</returns>
    public static bool IsInteger(BigRational a)
    {
      if (a.p == null) return true; //since BigRational is always normalized:
      fixed (uint* p = a.p) return *(ulong*)(p + ((p[0] & 0x3fffffff) + 1)) == 0x100000001;
      // or: return a % 1 == 0; or: var cpu = main_cpu; cpu.push(a); var b = cpu.isi(); cpu.pop(); return b;
    }
    /// <summary>
    /// Determines if the value is NaN. 
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number.</param>
    /// <returns><c>true</c> if <paramref name="a" /> is NaN; otherwise, <c>false</c>.</returns>
    public static bool IsNaN(BigRational a)
    {
      if (a.p == null) return false;
      fixed (uint* p = a.p) return *(ulong*)(p + ((p[0] & 0x3fffffff) + 1)) == 0x100000000;
    }
    /// <summary>
    /// Gets the absolute value of a <see cref="BigRational"/> number.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number as value.</param>
    /// <returns>The absolute value of the <see cref="BigRational"/> number.</returns>
    public static BigRational Abs(BigRational a)
    {
      return Sign(a) >= 0 ? a : -a;
    }
    /// <summary>
    /// Returns the smaller of two <see cref="BigRational"/> numbers.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>The a or b parameter, whichever is smaller.</returns>
    public static BigRational Min(BigRational a, BigRational b)
    {
      return a.CompareTo(b) <= 0 ? a : b;
    }
    /// <summary>
    /// Returns the larger of two <see cref="BigRational"/> numbers.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>The a or b parameter, whichever is larger.</returns>
    public static BigRational Max(BigRational a, BigRational b)
    {
      return a.CompareTo(b) >= 0 ? a : b;
    }
    /// <summary>
    /// Calculates the integral part of the <see cref="BigRational"/> number.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number as value to truncate.</param>
    /// <returns>
    /// The integral part of the <see cref="BigRational"/> number.<br/> 
    /// This is the number that remains after any fractional digits have been discarded.
    /// </returns>
    public static BigRational Truncate(BigRational a)
    {
      var cpu = main_cpu; cpu.push(a);
      cpu.rnd(0, 0); return cpu.popr();
    }
    /// <summary>
    /// Rounds a specified <see cref="BigRational"/> number to the closest integer toward negative infinity.
    /// </summary>
    /// The <see cref="BigRational"/> number to round.
    /// <returns>
    /// If <paramref name="a"/> has a fractional part, the next whole number toward negative
    /// infinity that is less than <paramref name="a"/>.<br/>
    /// or if <paramref name="a"/> doesn't have a fractional part, <paramref name="a"/> is returned unchanged.<br/>
    /// </returns>
    public static BigRational Floor(BigRational a)
    {
      var cpu = main_cpu; cpu.push(a);
      cpu.rnd(0, cpu.sign() >= 0 ? 0 : 4); return cpu.popr();
    }
    /// <summary>
    /// Returns the smallest integral value that is greater than or equal to the specified number.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number.</param>
    /// <returns>
    /// The smallest integral value that is greater than or equal to the <paramref name="a"/> parameter.
    /// </returns>
    public static BigRational Ceiling(BigRational a)
    {
      var cpu = main_cpu; cpu.push(a);
      cpu.rnd(0, cpu.sign() < 0 ? 0 : 4); return cpu.popr();
    }
    /// <summary>
    /// Calculates the factorial of <paramref name="a"/>.
    /// </summary>
    /// <param name="a">A positive number.</param>
    /// <returns>Returns the factorial of <paramref name="a"/>. NaN if <paramref name="a"/> less zero.</returns>
    public static BigRational Factorial(int a)
    {
      if (a < 0) return double.NaN; //NET 7 req. //throw new ArgumentException();
      var cpu = main_cpu; cpu.fac((uint)a);
      return cpu.popr();
    }
    /// <summary>
    /// Rounds a <see cref="BigRational"/> number to the nearest integral value
    /// and rounds midpoint values to the nearest even number.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number to be rounded.</param>
    /// <returns></returns>
    public static BigRational Round(BigRational a)
    {
      var cpu = main_cpu; cpu.push(a);
      cpu.rnd(0, 1); return cpu.popr();
    }
    /// <summary>
    /// Rounds a <see cref="BigRational"/> number to a specified number of fractional
    /// digits, and rounds midpoint values to the nearest even number. 
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number to be rounded.</param>
    /// <param name="digits">The number of fractional digits in the return value.</param>
    /// <returns>The <see cref="BigRational"/> number nearest to value that contains a number of fractional digits equal to digits.</returns>
    public static BigRational Round(BigRational a, int digits)
    {
      //var e = Pow10(digits); return Round(a * e) / e;
      var cpu = main_cpu; cpu.push(a);
      cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Rounds a <see cref="BigRational"/> number to a specified number of fractional digits 
    /// using the specified rounding convention.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number to be rounded.</param>
    /// <param name="digits">The number of decimal places in the return value.</param>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    /// <returns>
    /// The <see cref="BigRational"/> number with decimals fractional digits that the value is rounded to.<br/> 
    /// If the number has fewer fractional digits than decimals, the number is returned unchanged.
    /// </returns>
    public static BigRational Round(BigRational a, int digits, MidpointRounding mode)
    {
      int f = 1;
      switch (mode)
      {
        case MidpointRounding.ToZero: f = 0; break;
        case MidpointRounding.ToPositiveInfinity: if (Sign(a) < 0) f = 0; else f = 4; break;
        case MidpointRounding.ToNegativeInfinity: if (Sign(a) > 0) f = 0; else f = 4; break;
      }
      var cpu = main_cpu; cpu.push(a);
      cpu.rnd(digits, f); return cpu.popr();
    }
    /// <summary>
    /// Returns a specified number raised to the specified power.
    /// </summary>
    /// <param name="a">A <see cref="int"/> number to be raised to a power.</param>
    /// <param name="b">A <see cref="int"/> number that specifies a power.</param>
    /// <returns>The <see cref="BigRational"/> number a raised to the power b.</returns>
    public static BigRational Pow(int a, int b)
    {
      var cpu = main_cpu; cpu.pow(a, b); return cpu.popr();
    }
    /// <summary>
    /// Returns a specified number raised to the specified power.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> number to be raised to a power.</param>
    /// <param name="b">A <see cref="int"/> number that specifies a power.</param>
    /// <returns>The <see cref="BigRational"/> number a raised to the power b.</returns>
    public static BigRational Pow(BigRational a, int b)
    {
      //todo: opt. if(IsInteger(a) && IsPowerOfTwo(a))...
      var cpu = main_cpu; cpu.push(a); cpu.pow(b); return cpu.popr();
    }
    /// <summary>
    /// Returns a specified number raised to the specified power.<br/>
    /// For fractional exponents, the result is rounded to the specified number of decimal places.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="x">A <see cref="BigRational"/> number to be raised to a power.</param>
    /// <param name="y">A <see cref="BigRational"/> number that specifies a power.</param>
    /// <param name="digits"> The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>
    /// The <see cref="BigRational"/> number <paramref name="x"/> raised to the power <paramref name="y"/>.<br/>
    /// NaN if <paramref name="x"/> is less zero and <paramref name="y"/> is fractional.
    /// </returns>
    public static BigRational Pow(BigRational x, BigRational y, int digits)
    {
      //return Exp(y * Log(x, digits), digits);
      var s = Sign(x); if (s == 0) return default;
      if (s < 0)
      {
        if (IsInteger(y)) return Round(Pow(x, (int)y, digits), digits); //todo: inline, cases
        return double.NaN; //NET 7 req. //throw new ArgumentException(nameof(x));
      }
      var cpu = main_cpu; var c = prec(digits);
      cpu.push(x); cpu.log(c);
      cpu.push(y); cpu.mul(); cpu.exp(c);
      cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Returns a specified number raised to the specified power.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="x">A <see cref="BigRational"/> number to be raised to a power.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <returns>The <see cref="BigRational"/> number a raised to the power b.</returns>
    public static BigRational Pow2(BigRational x, int digits)
    {
      //todo: pow2 alg + cpu since log is fast but exp is slow
      return Exp(x * Log(2, digits), digits); //todo: opt. cpu
    }
    /// <summary>
    /// Returns the square root of a specified number.
    /// </summary>
    /// <remarks>
    /// For fractional roots, the result is rounded to the specified number of decimal places.
    /// </remarks>
    /// <param name="a">The number whose square root is to be found.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>
    /// Zero or positive – The positive square root of <paramref name="a"/>.<br/>
    /// NaN if <paramref name="a"/> is less zero.
    /// </returns>
    public static BigRational Sqrt(BigRational a, int digits)
    {
      if (Sign(a) < 0) return double.NaN; //NET 7 req. //throw new ArgumentException(nameof(a));
      var cpu = main_cpu; var c = prec(digits);
      cpu.push(a); cpu.sqrt(c); cpu.rnd(digits);
      return cpu.popr();
    }
    /// <summary>Computes the cube-root of a value.</summary>
    /// <param name="x">The value whose cube-root is to be computed.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>The cube-root of <paramref name="x" />.</returns>
    public static BigRational Cbrt(BigRational x, int digits)
    {
      return Pow(x, (BigRational)1 / 3, digits); //todo: opt. cpu
    }
    /// <summary>Computes the n-th root of a value.</summary>
    /// <param name="x">The value whose <paramref name="n" />-th root is to be computed.</param>
    /// <param name="n">The degree of the root to be computed.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>The <paramref name="n" />-th root of <paramref name="x" />.</returns>
    public static BigRational RootN(BigRational x, int n, int digits)
    {
      return Pow(x, (BigRational)1 / n, digits); //todo: opt. cpu
    }
    /// <summary>Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled triangle.</summary>
    /// <param name="x">The value to square and add to <paramref name="y" />.</param>
    /// <param name="y">The value to square and add to <paramref name="x" />.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>The square root of <paramref name="x" />-squared plus <paramref name="y" />-squared.</returns>
    public static BigRational Hypot(BigRational x, BigRational y, int digits)
    {
      //return Sqrt(x * x + y * y);
      var cpu = main_cpu; var c = prec(digits);
      cpu.push(x); cpu.sqr(); cpu.push(y); cpu.sqr(); cpu.add(); //todo: lim x^2, y^2 and check
      cpu.sqrt(c); cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Returns the base 2 logarithm of a specified number.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="x">The number whose logarithm is to be found.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <returns>
    /// The base 2 logarithm of <paramref name="x"/>.<br/>
    /// NaN if <paramref name="x"/> is less or equal zero.
    /// </returns>
    public static BigRational Log2(BigRational x, int digits)
    {
      if (Sign(x) <= 0) return double.NaN; //NET 7 req. //throw new ArgumentException(nameof(x));
      var cpu = main_cpu; var c = prec(digits);
      cpu.push(x); cpu.log2(c);
      cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Gets the integer base 2 logarithm of a <see cref="BigRational"/> number.
    /// </summary>
    /// <param name="x">A <see cref="BigRational"/> number as value.</param>
    /// <returns>The integer base 2 logarithm of the <see cref="BigRational"/> number.</returns>
    public static int ILog2(BigRational x)
    {
      if (x.p == null) return 0;
      if (IsInteger(x)) //todo: opt. 
      {
        var n = unchecked((int)(x.p[0] & 0x3fffffff));
        return (n << 5) - BitOperations.LeadingZeroCount(x.p[n]) - 1;
      }
      return (int)Truncate(Log2(x, 0)); //todo: opt. 
    }
    /// <summary>
    /// Returns the base 10 logarithm of a specified number.
    /// </summary>
    /// <param name="x">The number whose logarithm is to be found.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <returns>The base 10 logarithm of <paramref name="x"/>.</returns>
    /// <exception cref="ArgumentException">For <paramref name="x"/> is less or equal zero.</exception>
    public static BigRational Log10(BigRational x, int digits)
    {
      return Round(Log2(x, digits) / Log2(10, digits), digits); //todo: inline
    }
    /// <summary>
    /// Gets the integer base 10 logarithm of a <see cref="BigRational"/> number.
    /// </summary>
    /// <remarks>
    /// The integer base 10 logarithm is identical with the exponent in the scientific notation of the number.<br/> 
    /// eg. <b>3</b> for 1000 (1E+<b>03</b>) or <b>-3</b> for 0.00123 (1.23E-<b>03</b>)
    /// </remarks>
    /// <param name="a">A <see cref="BigRational"/> number as value.</param>
    /// <returns>The integer base 10 logarithm of the <see cref="BigRational"/> number.</returns>
    public static int ILog10(BigRational a)
    {
      var cpu = main_cpu; cpu.push(a);
      cpu.tos(default, out _, out var e, out _, false); return e;
    }
    /// <summary>
    /// Returns the natural (base e) logarithm of a specified number.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="x">The number whose logarithm is to be found.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>
    /// The natural logarithm of <paramref name="x"/>; that is, <c>ln <paramref name="x"/></c>, or <c>log e <paramref name="x"/></c>.<br/>
    /// NaN if <paramref name="x"/> is less or equal zero.
    /// </returns>
    public static BigRational Log(BigRational x, int digits)
    {
      if (Sign(x) <= 0) return double.NaN; //NET 7 req. //throw new ArgumentException(nameof(x));
      var cpu = main_cpu; var c = prec(digits);
      cpu.push(x); cpu.log(c);
      cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>Computes the logarithm of a value in the specified base.</summary>
    /// <param name="x">The value whose logarithm is to be computed.</param>
    /// <param name="newBase">The base in which the logarithm is to be computed.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns><c>log<sub><paramref name="newBase" /></sub>(<paramref name="x" />)</c></returns>
    public static BigRational Log(BigRational x, BigRational newBase, int digits) //todo: <--> Log(x, digits)
    {
      return Round(Log(x, digits) / Log(newBase, digits), digits); //todo: opt. cpu
    }
    /// <summary>
    /// Returns e raised to the specified power.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="x">A number specifying a power.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <returns>
    /// The number e raised to the power <paramref name="x"/>.
    /// </returns>
    public static BigRational Exp(BigRational x, int digits)
    {
      var cpu = main_cpu; var c = prec(digits);
      cpu.push(x); cpu.exp(c);
      cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Calculates π rounded to the specified number of decimal digits.<br/>
    /// </summary>
    /// <remarks>
    /// Represents the ratio of the circumference of a circle to its diameter, specified by the constant, π.
    /// </remarks>
    /// <param name="digits">
    /// The number of decimal digits to calculate.
    /// </param>
    /// <returns>π rounded to the specified number of decimal digits.</returns>
    public static BigRational Pi(int digits)
    {
      var cpu = main_cpu; var c = prec(digits);
      cpu.pi(c); cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Calculates π rounded to <see cref="MaxDigits"/>.
    /// </summary>
    /// <remarks>
    /// Represents the ratio of the circumference of a circle to its diameter, specified by the constant, π.
    /// </remarks>
    /// <returns>π rounded to <see cref="MaxDigits"/>.</returns>
    public static BigRational Pi()
    {
      return Pi(MaxDigits); //todo: opt. cpu
    }
    /// <summary>
    /// Calculates the number of radians in one turn, specified by the constant, τ rounded to the specified number of decimal digits..
    /// </summary>
    /// <param name="digits">
    /// The number of decimal digits to calculate.
    /// </param>
    /// <returns>τ rounded to the specified number of decimal digits.</returns>
    public static BigRational Tau(int digits)
    {
      var cpu = main_cpu; var c = prec(digits);
      cpu.pi(c); cpu.push(2u); cpu.mul(); cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Calculates π rounded to <see cref="MaxDigits"/>.<br/>
    /// Calculates the number of radians in one turn, specified by the constant, τ rounded to the specified number of decimal digits..
    /// </summary>
    /// <returns>τ rounded to <see cref="MaxDigits"/>.</returns>
    public static BigRational Tau()
    {
      return Tau(MaxDigits); //todo: opt. cpu
    }
    /// <summary>
    /// Returns the sine of the specified angle.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="x">An angle, measured in radians.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <returns>The sine of <paramref name="x"/>.</returns>
    public static BigRational Sin(BigRational x, int digits)
    {
      var cpu = main_cpu; var c = prec(digits);
      cpu.push(x); cpu.sin(c, false);
      cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Returns the cosine of the specified angle.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="x">An angle, measured in radians.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <returns>The cosine of <paramref name="x"/>.</returns>
    public static BigRational Cos(BigRational x, int digits)
    {
      var cpu = main_cpu; var c = prec(digits);
      cpu.push(x); cpu.sin(c, true);
      cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Returns the tangent of the specified angle.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="x">An angle, measured in radians.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <returns>The tangent of <paramref name="x"/>.</returns>
    public static BigRational Tan(BigRational x, int digits)
    {
      return Sin(x, digits) / Cos(x, digits); //todo: inline
    }
    /// <summary>
    /// Returns the angle whose sine is the specified number.
    /// </summary>
    /// <param name="x">A number representing a sine, where d must be greater than or equal to -1, but less than or equal to 1.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <returns>
    /// An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2. 
    /// -or- NaN if <paramref name="x"/> &lt; -1 or <paramref name="x"/> &gt; 1.
    /// </returns>
    public static BigRational Asin(BigRational x, int digits)
    {
      return Atan(x / Sqrt(1 - x * x, digits), digits); //todo: inline
    }
    /// <summary>
    /// Returns the angle whose cosine is the specified number.
    /// </summary>
    /// <param name="x">A number representing a cosine, where d must be greater than or equal to -1, but less than or equal to 1.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <returns>
    /// An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2. 
    /// -or- NaN if <paramref name="x"/> &lt; -1 or <paramref name="x"/> &gt; 1.
    /// </returns>
    public static BigRational Acos(BigRational x, int digits)
    {
      return Atan(Sqrt(1 - x * x, digits) / x, digits); //todo: inline
    }
    /// <summary>
    /// Returns the angle whose tangent is the specified number.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="x">A number representing a tangent.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.</returns>
    public static BigRational Atan(BigRational x, int digits)
    {
      var cpu = main_cpu; var c = prec(digits);
      cpu.push(x); cpu.atan(c);
      cpu.rnd(digits); return cpu.popr();
    }
    /// <summary>
    /// Returns the angle whose tangent is the quotient of two specified numbers.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/>and the accuracy of the last digits has not yet been ensured!
    /// </remarks>
    /// <param name="y">The y coordinate of a point.</param>
    /// <param name="x">The x coordinate of a point.</param>
    /// <param name="digits">
    /// The maximum number of fractional decimal digits in the return value.<br/>
    /// </param>
    /// <returns>
    /// An angle, θ, measured in radians, such that -π ≤ θ ≤ π, and tan(θ) = y / x, where
    /// (x, y) is a point in the Cartesian plane.<br/>Observe the following:<br/>
    /// - For (x, y) in quadrant 1, 0 &lt; θ &lt; π/2.<br/> 
    /// - For (x, y) in quadrant 2, π/2 &lt; θ ≤ π.<br/> 
    /// - For (x, y) in quadrant 3, -π &lt; θ &lt; -π/2.<br/> 
    /// - For (x, y) in quadrant 4, -π/2 &lt; θ &lt; 0.<br/> 
    /// For points on the boundaries of the quadrants, the return value is the following:<br/>
    /// - If y is 0 and x is not negative, θ = 0.<br/> 
    /// - If y is 0 and x is negative, θ = π.<br/>
    /// - If y is positive and x is 0, θ = π/2<br/>
    /// - If y is negative and x is 0, θ = -π/2.<br/>
    /// - If y is 0 and x is 0, θ = 0.<br/>
    /// </returns>
    public static BigRational Atan2(BigRational y, BigRational x, int digits)
    {
      if (x > 0)
        return 2 * Atan(y / (Sqrt(x * x + y * y, digits) + x), digits);
      else if (y != 0)
        return 2 * Atan((Sqrt(x * x + y * y, digits) - x) / y, digits);
      else if (x < 0 && y == 0)
        return Pi(digits);
      return default(BigRational) / 0; //NaN
    }

    /// <summary>Computes the hyperbolic arc-sine of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic arc-sine is to be computed.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>The hyperbolic arc-sine of <paramref name="x" />.</returns>
    public static BigRational Asinh(BigRational x, int digits)
    {
      return Log(x + Sqrt(x * x + 1, digits), digits); //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic arc-cosine of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic arc-cosine is to be computed.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>The hyperbolic arc-cosine of <paramref name="x" />.</returns>
    public static BigRational Acosh(BigRational x, int digits)
    {
      return Log(x + Sqrt(x * x - 1, digits), digits); //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic arc-tangent of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic arc-tangent is to be computed.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>The hyperbolic arc-tangent of <paramref name="x" />.</returns>
    public static BigRational Atanh(BigRational x, int digits)
    {
      return Log((1 + x) / (1 - x), digits) / 2; //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic sine of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic sine is to be computed.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>The hyperbolic sine of <paramref name="x" />.</returns>
    public static BigRational Sinh(BigRational x, int digits)
    {
      return (Exp(x, digits) - Exp(-x, digits)) / 2; //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic cosine of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic cosine is to be computed.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>The hyperbolic cosine of <paramref name="x" />.</returns>
    public static BigRational Cosh(BigRational x, int digits)
    {
      return (Exp(x, digits) + Exp(-x, digits)) / 2; //todo: opt. cpu
    }
    /// <summary>Computes the hyperbolic tangent of a value.</summary>
    /// <param name="x">The value, in radians, whose hyperbolic tangent is to be computed.</param>
    /// <param name="digits">The maximum number of fractional decimal digits in the return value.</param>
    /// <returns>The hyperbolic tangent of <paramref name="x" />.</returns>
    public static BigRational Tanh(BigRational x, int digits)
    {
      return 1 - 2 / (Exp(x * 2, digits) + 1); //todo: opt. cpu
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
    public static BigRational GreatestCommonDivisor(BigRational a, BigRational b) //todo: -> BigInt
    {
      var cpu = main_cpu; cpu.push(a); cpu.push(b);
      cpu.gcd(); return cpu.popr();
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
    public static BigRational LeastCommonMultiple(BigRational a, BigRational b) //todo: -> BigInt
    {
      //|a * b| / gcd(a, b) == |a / gcd(a, b) * b| 
      var cpu = main_cpu; cpu.push(a); cpu.push(b);
      cpu.dup(); cpu.dup(2); cpu.gcd(); cpu.div(); cpu.mul(); cpu.abs();
      return cpu.popr();
    }
    /// <summary>
    /// Performes an integer division <paramref name="a"/> / <paramref name="b"/> 
    /// </summary>
    /// <remarks>
    /// For integer values <paramref name="a"/> and <paramref name="b"/>, the result equals a <see cref="BigInteger.Divide(BigInteger, BigInteger)"/> division.<br/>
    /// This in contrast to <paramref name="a"/> / <paramref name="b"/>, where a corresponding fraction results.
    /// </remarks>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (devisor)</param>
    /// <returns>A <see cref="BigRational "/> integer value. NaN when divided by zero.</returns>
    public static BigRational IDiv(BigRational a, BigRational b)
    {
      //var cpu = main_cpu; cpu.div(a, b); cpu.mod(); cpu.swp(); cpu.pop(); return cpu.popr();
      var cpu = main_cpu; cpu.push(a); cpu.push(b); cpu.idiv(); return cpu.popr();
    }
    /// <summary>
    /// Performes an integer modulo operation <paramref name="a"/> % <paramref name="b"/> what is the remainder that results from a division.
    /// </summary>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (divisor)</param>
    /// <remarks>
    /// For integer values <paramref name="a"/> and <paramref name="b"/>, the result equals a <see cref="BigInteger"/> modulo (%) operation.<br/>
    /// This in contrast to <paramref name="a"/> % <paramref name="b"/>, where for <see cref="BigInteger"/> a corresponding fraction results.
    /// </remarks>
    /// <returns>A <see cref="BigRational "/> integer value. NaN when divided by zero.</returns>
    public static BigRational IMod(BigRational a, BigRational b)
    {
      //var cpu = main_cpu; cpu.push(a); cpu.push(b); cpu.div(b, b); cpu.mod(); cpu.pop(); return cpu.popr();
      var cpu = main_cpu; cpu.push(a); cpu.push(b); cpu.imod(); return cpu.popr();
    }
    /// <summary>
    /// Calculates the quotient of two <see cref="BigRational"/> signed values and also returns the remainder in an output parameter.
    /// </summary>
    /// <remarks>
    /// This function is for compatibility to <see cref="Math.DivRem(int, int, out int)"/> like functions.<br/>
    /// For integer values <paramref name="a"/> and <paramref name="b"/>, the result equals a <see cref="BigInteger.DivRem(BigInteger, BigInteger, out BigInteger)"/> operation..<br/>
    /// </remarks>
    /// <param name="a">The dividend.</param>
    /// <param name="b">The divisor.</param>
    /// <param name="r">The remainder.</param>
    /// <returns>The quotient of the specified numbers. NaN when divided by zero.</returns>
    public static BigRational DivRem(BigRational a, BigRational b, out BigRational r)
    {
      if (BigRational.Sign(b) == 0) return r = double.NaN; 
      var cpu = main_cpu; cpu.div(a, b); cpu.mod(0);
      cpu.swp(); r = cpu.popr(); return cpu.popr();
    }
    /// <summary>
    /// Produces the quotient and the remainder of two signed <see cref="BigRational"/> numbers.
    /// </summary>
    /// This function is for compatibility to <see cref="Math.DivRem(int, int)"/> like functions.<br/>
    /// For integer values <paramref name="a"/> and <paramref name="b"/>, the result equals a <see cref="BigInteger.DivRem(BigInteger, BigInteger, out BigInteger)"/> operation..<br/>
    /// <param name="a">The dividend.</param>
    /// <param name="b">The divisor.</param>
    /// <returns>The quotient and the remainder of the specified numbers as integer values.</returns>
    public static (BigRational Quotient, BigRational Remainder) DivRem(BigRational a, BigRational b)
    {
      var d = DivRem(a, b, out var r); return (d, r);
    }
    /// <summary>
    /// Returns the numerator and the denominator of the specified number.
    /// </summary>
    /// <param name="a">A <see cref="BigRational"/> integer number</param>
    /// <param name="den">returns the denominator of <paramref name="a"/> always positive integer.</param>
    /// <returns>Returns the numerator of <paramref name="a"/>.</returns>
    public static BigRational NumDen(BigRational a, out BigRational den)
    {
      var cpu = main_cpu; cpu.push(a);
      cpu.mod(8); var s = cpu.sign();
      if (s < 0) cpu.neg(); den = cpu.popr();
      if (s < 0) cpu.neg(); return cpu.popr();
    }

    /// <summary>
    /// Gets or sets the default maximum number of decimal digits computed by functions with irrational results.<br/> 
    /// Applies to power, root, exponential, logarithmic, trigonometric and hyperbolic function versions without explicit digits parameter.
    /// </summary>
    /// <remarks>
    /// The initial value is 30 in difference to <see cref="double"/> with a fixed precision of around 15 decimal digits.<br/>
    /// This is a thread static property.
    /// </remarks>  
    /// <value>Maximum number of decimal digits.</value>
    public static int MaxDigits
    {
      get => main_cpu.maxdigits;
      set => main_cpu.maxdigits = value;
    }
    static uint prec(int digits) => (uint)Math.Ceiling(digits * 3.321928094887362); // * ((Math.Log(2) + Math.Log(5)) / Math.Log(2))
  }
}
