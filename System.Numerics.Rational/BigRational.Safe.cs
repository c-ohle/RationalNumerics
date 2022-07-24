
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace System.Numerics
{
  unsafe partial struct BigRational
  {
    /// <summary>
    /// Represents a thread-safe instance of a shared <see cref="CPU"/> for rational and arbitrary arithmetic.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: As a <c>ref</c> type, it <b>cannot</b> be used in asynchronous methods, asynchronous lambda expressions.<br/>
    /// query expressions, iterator blocks or inside non-static nested functions.<br/>
    /// For such cases, however, it is safe to use a local new created instance of a <see cref="CPU"/> object.
    /// </remarks>
    [DebuggerTypeProxy(typeof(DebugView)), DebuggerDisplay("Count = {p.i}")]
    public readonly ref struct SafeCPU
    {
      internal readonly CPU p; internal SafeCPU(CPU p) => this.p = p; //internal for DebugView only
      /// <summary>
      /// Returns a temporary absolute index of the current stack top
      /// from which subsequent instructions can index stack entries.
      /// </summary>
      /// <remarks>
      /// The absolute indices are encoded as <see cref="uint"/>, which is different from the relative indices, 
      /// which are encoded as <see cref="int"/>.<br/>
      /// Within functions it is often more easy to work with absolute indices as they do not change frequently.
      /// </remarks>
      /// <example><code>
      /// var m = cpu.mark();
      /// cpu.push(x); // at m + 0
      /// cpu.push(y); // at m + 1
      /// cpu.push(z); // at m + 2
      /// cpu.add(m + 0, m + 1); // x + y
      /// cpu.pop(4);
      /// </code></example>
      /// <returns>A <see cref="uint"/> value that marks the current stack top.</returns>
      public uint mark() => p.mark();
      /// <summary>
      /// Pushes a 0 (zero) value onto the stack.
      /// </summary>
      public void push() => p.push();
      /// <summary>
      /// Pushes the supplied <see cref="BigRational"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(BigRational v) => p.push(v);
      /// <summary>
      /// Pushes the supplied <see cref="int"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(int v) => p.push(v);
      /// <summary>
      /// Pushes n copies of the supplied <see cref="int"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      /// <param name="n">The number of copies to push.</param>
      public void push(int v, int n) => p.push(v, n);
      /// <summary>
      /// Pushes the supplied <see cref="uint"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(uint v) => p.push(v);
      /// <summary>
      /// Pushes the supplied <see cref="long"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(long v) => p.push(v);
      /// <summary>
      /// Pushes the supplied <see cref="ulong"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(ulong v) => p.push(v);
      /// <summary>
      /// Pushes the supplied <see cref="float"/> value onto the stack.
      /// </summary>
      /// <remarks>
      /// Rounds the value implicit to 7 digits as the <see cref="float"/> format has
      /// only 7 digits of precision.<br/> 
      /// see: <see cref="BigRational(float)"/>
      /// </remarks>
      /// <param name="v">The value to push.</param>
      public void push(float v) => p.push(v);
      /// <summary>
      /// Pushes the supplied <see cref="double"/> value onto the stack.
      /// </summary>
      /// <remarks>
      /// Rounds the value implicit to 15 digits as the <see cref="double"/> format has
      /// only 15 digits of precision.<br/> 
      /// see: <see cref="BigRational.BigRational(double)"/>
      /// </remarks>
      /// <param name="v">The value to push.</param>
      public void push(double v) => p.push(v);
      /// <summary>
      /// Pushes the supplied <see cref="double"/> value onto the stack
      /// using a bit-exact conversion without rounding.
      /// </summary>                                    
      /// <remarks>
      /// see: <see cref="BigRational.BigRational(double)"/>
      /// </remarks>
      /// <param name="v">The value to push.</param>
      /// <param name="exact">Should be true.</param>
      /// <exception cref="ArgumentOutOfRangeException">Value id NaN.</exception>
      public void push(double v, bool exact) => p.push(v, exact);
      /// <summary>
      /// Pushes the supplied <see cref="decimal"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(decimal v) => p.push(v);
      /// <summary>
      /// Pushes the supplied <see cref="BigInteger"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(BigInteger v) => p.push(v);
      /// <summary>
      /// Pushes the supplied value onto the stack.<br/>
      /// </summary>
      /// <remarks>
      /// <b>Note</b>: The Span must contain valid data based on the specification:<br/>
      /// <seealso href="https://c-ohle.github.io/RationalNumerics/#data-layout"/><br/>
      /// (exception: it does not have to be a normalized fraction.)<br/>
      /// For performance reasons, there are no validation checks at this layer.<br/><br/> 
      /// Together with <see cref="get(uint, out ReadOnlySpan{uint})"/> the operation represent a fast low level interface for direct access in form of the internal data representation.<br/> 
      /// This is intended to allow:<br/>
      /// 1. custom algorithms working on bitlevel<br/>
      /// 2. custom binary serialization<br/>
      /// 3. custom types like vectors with own storage managemend.<br/>
      /// </remarks>
      /// <param name="v"></param>
      public void push(ReadOnlySpan<uint> v) => p.push(v);
      /// <summary>
      /// Converts the value at absolute position <paramref name="i"/> on stack to a 
      /// always normalized <see cref="BigRational"/> number and returns it.
      /// </summary>
      /// <remarks>
      /// This function is helpful to read out vectors and matrices efficently.<br/>
      /// For absolute positions see: <see cref="mark"/>
      /// </remarks>
      /// <param name="i">Absolute index of the value to get.</param>
      /// <param name="v">Returns the value.</param>
      public void get(uint i, out BigRational v) => p.get(i, out v);
      /// <summary>
      /// Converts the value at absolute position <paramref name="i"/> on stack to a 
      /// <see cref="double"/> value and returns it.
      /// </summary>
      /// <remarks>
      /// This function is helpful to read out vectors and matrices efficently.<br/>
      /// For absolute positions see: <see cref="mark"/>
      /// </remarks>
      /// <param name="i">Absolute index of the value to get.</param>
      /// <param name="v">Returns the value.</param>
      public void get(uint i, out double v) => p.get(i, out v);
      /// <summary>
      /// Converts the value at absolute position <paramref name="i"/> on stack to a 
      /// <see cref="float"/> value and returns it.
      /// </summary>
      /// <remarks>
      /// This function is helpful to read out vectors and matrices efficently.<br/>
      /// For absolute positions see: <see cref="mark"/>
      /// </remarks>
      /// <param name="i">Absolute index of the value to get.</param>
      /// <param name="v">Returns the value.</param>
      public void get(uint i, out float v) => p.get(i, out v);
      /// <summary>
      /// Exposes the internal data representation of the value at absolute position i on the stack.<br/>
      /// </summary>
      /// <remarks>
      /// Apply <see cref="norm(int)"/> beforehand to ensure the fraction is normalized in its binary form, if required.<br/>
      /// <b>Note</b>: The returned data is only valid solong the stack entry is not changed by a subsequent cpu operation.<br/><br/>
      /// Together with <see cref="push(ReadOnlySpan{uint})"/> the operation represent a fast low level interface for direct access in form of the internal data representation.<br/> 
      /// This is intended to allow:<br/>
      /// 1. custom algorithms working on bitlevel<br/>
      /// 2. custom binary serialization<br/>
      /// 3. custom types like vectors with own storage managemend.<br/><br/>
      /// For absolute positions see: <see cref="mark"/>
      /// </remarks>
      /// <param name="i">Absolute index of the value to get.</param>
      /// <param name="v">Returns the value.</param>
      public void get(uint i, out ReadOnlySpan<uint> v) => p.get(i, out v);
      /// <summary>
      /// Removes the value currently on top of the stack, 
      /// convert and returns it as always normalized <see cref="BigRational"/> number.<br/>
      /// </summary>
      /// <returns>A always normalized <see cref="BigRational"/> number.</returns>
      public BigRational popr() => p.popr();
      /// <summary>
      /// Removes the value currently on top of the stack, 
      /// convert and returns it as <see cref="double"/> value.<br/>
      /// </summary>
      /// <remarks>
      /// <see cref="double.PositiveInfinity"/> or <see cref="double.NegativeInfinity"/> 
      /// is returned in case of out of range as the <see cref="double"/> value range is limited. 
      /// </remarks>
      /// <returns>A <see cref="double"/> value.</returns>
      public double popd() => p.popd();
      /// <summary>
      /// Removes the value currently on top of the stack, 
      /// convert the numerator and returns it as <see cref="int"/> value.<br/>
      /// </summary>
      /// <remarks>
      /// Only the first 32-bit of the numerator are interpreted and retuned as <see cref="int"/>.<br/>
      /// It conforms to the convention of integer casts like the cast from <see cref="long"/> to <see cref="int"/>.<br/>
      /// The function is intended for the fastest possible access to integer results in the range of <see cref="int"/>.<br/>
      /// </remarks>
      /// <returns>A <see cref="int"/> value.</returns>
      public int popi() => p.popi();
      /// <summary>
      /// Removes the value currently on top of the stack.
      /// </summary>
      public void pop() => p.pop();
      /// <summary>
      /// Removes n values currently on top of the stack.
      /// </summary>
      /// <param name="n">Number of values to pop.</param>
      public void pop(int n) => p.pop(n);
      /// <summary>
      /// Swaps the first two values on top of the stack.
      /// </summary>
      public void swp() => p.swp();
      /// <summary>
      /// Swaps the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void swp(int a = 1, int b = 0) => p.swp(a, b);
      /// <summary>
      /// Swaps the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void swp(uint a, uint b) => p.swp(a, b);
      /// <summary>
      /// Swaps the values at index <paramref name="a"/> as absolute index with the value on top of the stack.
      /// </summary>
      /// <param name="a">Absolute index a stack entry.</param>
      public void swp(uint a) => p.swp(a);
      /// <summary>
      /// Duplicates the value at index <paramref name="a"/> relative to the top of the stack 
      /// and pushes it as copy on top of the stack.
      /// </summary>
      /// <remarks>
      /// Since dup-operations always require memory copies, 
      /// it is more efficient to use swap when possible.
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Relative index of a stack entry.</param>
      public void dup(int a = 0) => p.dup(a);
      /// <summary>
      /// Duplicates the value at index <paramref name="a"/> as absolute index in the stack 
      /// and pushes it as copy on top of the stack.
      /// </summary>
      /// <remarks>
      /// Since dup-operations always require memory copies, 
      /// it is more efficient to use <see cref="swp()"/> when possible.
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void dup(uint a) => p.dup(a);
      /// <summary>
      /// Negates the value at index <paramref name="a"/> relative to the top of the stack.<br/>
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      public void neg(int a = 0) => p.neg(a);
      /// <summary>
      /// Negates the value at index <paramref name="a"/> as absolute index in the stack.<br/>
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void neg(uint a) => p.neg(a);
      /// <summary>
      /// Convert the value at index <paramref name="a"/> relative to the top of the stack to it's absolute value.<br/>
      /// Default index 0 addresses the value on top of the stack.
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      public void abs(int a = 0) => p.abs(a);
      /// <summary>
      /// Adds the first two values on top of the stack 
      /// and replaces them with the result.
      /// </summary>
      public void add() => p.add();
      /// <summary>
      /// Adds the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack
      /// and replaces the value at index a with the result.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void add(int a, int b) => p.add(a, b);
      /// <summary>
      /// Adds the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack
      /// and pushes the result on top of the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void add(uint a, uint b) => p.add(a, b);
      /// <summary>
      /// Adds the value at index a as absolute index in the stack
      /// and the value on top of the stack.<br/>
      /// Replaces the value on top of the stack with the result.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void add(uint a) => p.add(a);
      /// <summary>
      /// Adds value <paramref name="a"/> and the value on top of the stack.<br/>
      /// Replaces the value on top of the stack with the result.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> value.</param>
      public void add(BigRational a) => p.add(a);
      /// <summary>
      /// Adds the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      public void add(BigRational a, BigRational b) => p.add(a, b);
      /// <summary>
      /// Subtract the first two values on top of the stack 
      /// and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// a - b where b is the value on top of the stack. 
      /// </remarks>
      public void sub() => p.sub();
      /// <summary>
      /// Subtracts the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack
      /// and replaces the value at index a with the result.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void sub(int a, int b) => p.sub(a, b);
      /// <summary>
      /// Subtracts the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack
      /// and pushes the result on top of the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void sub(uint a, uint b) => p.sub(a, b);
      /// <summary>
      /// Subtracts the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      public void sub(BigRational a, BigRational b) => p.sub(a, b);
      /// <summary>
      /// Multiplies the first two values on top of the stack 
      /// and replaces them with the result.
      /// </summary>
      public void mul() => p.mul();
      /// <summary>
      /// Multiplies the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack
      /// and replaces the value at index a with the result.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void mul(int a, int b) => p.mul(a, b);
      /// <summary>
      /// Multiplies the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack
      /// and pushes the result on top of the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void mul(uint a, uint b) => p.mul(a, b);
      /// <summary>
      /// Multiplies the value at index a as absolute index in the stack
      /// and the value on top of the stack.<br/>
      /// Replaces the value on top of the stack with the result.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void mul(uint a) => p.mul(a);
      /// <summary>
      /// Multiplies value <paramref name="a"/> and the value on top of the stack.<br/>
      /// Replaces the value on top of the stack with the result.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> value.</param>
      public void mul(BigRational a) => p.mul(a);
      /// <summary>
      /// Multiplies the value <paramref name="a"/> and the value at b as absolute index in the stack
      /// and pushes the result on the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">Absolute index of a stack entry as second value.</param>
      public void mul(BigRational a, uint b) => p.mul(a, b);
      /// <summary>
      /// Multiplies the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      public void mul(BigRational a, BigRational b) => p.mul(a, b);
      /// <summary>
      /// Divides the first two values on top of the stack 
      /// and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// Divides a / b where b is the value on top of the stack.
      /// </remarks>
      public void div() => p.div();
      /// <summary>
      /// Divides the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack
      /// and replaces the value at index a with the result.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void div(int a, int b) => p.div(a, b);
      /// <summary>
      /// Divides the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack
      /// and pushes the result on top of the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void div(uint a, uint b) => p.div(a, b);
      /// <summary>
      /// Divides the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      public void div(BigRational a, BigRational b) => p.div(a, b);
      /// <summary>
      /// Divides the value <paramref name="a"/> and the value at index <paramref name="b"/> relative to the top of the stack.<br/>
      /// Pushes the result on top of the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A relative index of a stack entry.</param>
      public void div(BigRational a, int b) => p.div(a, b);
      /// <summary>
      /// Squares the value at index <paramref name="a"/> relative to the top of the stack.<br/>
      /// Default index 0 squares the value on top of the stack.
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      public void sqr(int a = 0) => p.sqr(a);
      /// <summary>
      /// Squares the value at index <paramref name="a"/> as absolute index in the stack.<br/>
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void sqr(uint a) => p.sqr(a);
      /// <summary>
      /// Replaces the value at index <paramref name="i"/> relative to the top of the stack<br/>
      /// with it's multiplicative inverse,
      /// also called the reciprocal. <c>x = 1 / x;</c> 
      /// </summary>
      /// <remarks>
      /// It's a fast operation and should be used for <c>1 / x</c> instead of a equivalent <see cref="div()"/> operations.<br/>
      /// Inversion of rational 0 values (0 / 1 => 1 / 0) results in NaN values which can be ignored for all bit-level operations.
      /// </remarks>
      /// <param name="i">Relative index of a stack entry.</param>
      public void inv(int i = 0) => p.inv(i);
      /// <summary>
      /// Selects the value at index i relative to the top of the stack.<br/> 
      /// Shifts the numerator value to the left (in zeros) by the specified number of bits.
      /// </summary>
      /// <remarks>
      /// Shifts the numerator value only, which is a fast alternative to multiplies by powers of two.<br/>
      /// To shift the denominator is possible by calling <see cref="inv"/> before and after.
      /// </remarks>
      /// <param name="c">The number of bits to shift.</param>
      /// <param name="i">A relative index of a stack entry.</param>
      public void shl(uint c, int i = 0) => p.shl(c, i);
      /// <summary>
      /// Selects the value at index i relative to the top of the stack.<br/> 
      /// Shifts the numerator value to the right (in zeros) by the specified number of bits.
      /// </summary>
      /// <remarks>
      /// Shifts the numerator value only, which is a fast alternative to divisions by powers of two.<br/>
      /// Manipulating denominators is possible by calling <see cref="inv"/> beforehand.<br/>
      /// Inversion of rational 0 values (0 / 1 => 1 / 0) results in NaN values which can be ignored for all bit-level operations.
      /// </remarks>
      /// <param name="c">The number of bits to shift.</param>
      /// <param name="i">A relative index of a stack entry.</param>
      public void shr(uint c, int i = 0) => p.shr(c, i);
      /// <summary>
      /// Bitwise logical AND of the numerators<br/>
      /// of the first two values on top of the stack and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// Manipulating denominators is possible by calling <see cref="inv"/> beforehand.<br/>
      /// Inversion of rational 0 values (0 / 1 => 1 / 0) results in NaN values which can be ignored for all bit-level operations.
      /// </remarks>
      public void and() => p.and();
      /// <summary>
      /// Bitwise logical OR of the numerators<br/>
      /// of the first two values on top of the stack and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// Manipulating denominators is possible by calling <see cref="inv"/> beforehand.<br/>
      /// Inversion of rational 0 values (0 / 1 => 1 / 0) results in NaN values which can be ignored for all bit-level operations.
      /// </remarks>
      public void or() => p.or();
      /// <summary>
      /// Bitwise logical XOR of the numerators<br/>
      /// of the first two values on top of the stack and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// Manipulating denominators is possible by calling <see cref="inv"/> beforehand.<br/>
      /// Inversion of rational 0 values (0 / 1 => 1 / 0) results in NaN values which can be ignored for all bit-level operations.
      /// </remarks>
      public void xor() => p.xor();
      /// <summary>
      /// Pops the value at the top of the stack.<br/> 
      /// Divides the numerator by the denominator and 
      /// pushes the remainder and the quotient as integer values on the stack.
      /// </summary>
      /// <remarks>
      /// 0 quotient truncated towards zero.<br/>
      /// 1 quotient rounded away from zero.<br/>
      /// 2 quotient rounded to even.<br/>
      /// 4 quotient ceiling.<br/>
      /// 8 skip division, numerator and denominator remaining on stack.<br/>
      /// </remarks>
      /// <param name="f">A <see cref="int"/>flags see remarks.</param>
      public void mod(int f = 0) => p.mod(f);
      /// <summary>
      /// Rounds the value at the top of the stack 
      /// to the specified number of fractional decimal digits.<br/>
      /// </summary>
      /// <remarks>
      /// Parameter <paramref name="f"/> midpoint rounding see: <see cref="mod(int)"/><br/>
      /// 0 quotient truncated towards zero.<br/>
      /// 1 rounded away from zero. (default)<br/>
      /// 2 rounded to even.<br/>
      /// 4 ceiling.<br/>
      /// </remarks>
      /// <param name="c">Specifies the number of decimal digits to round to.</param>
      /// <param name="f">Midpoint rounding see: <see cref="mod(int)"/>.</param>
      public void rnd(int c, int f = 1) => p.rnd(c, f);
      /// <summary>
      /// Pushes the specified number x raised to the specified power y to the stack.
      /// </summary>
      /// <param name="x">A <see cref="int"/> value to be raised to a power.</param>
      /// <param name="y">A <see cref="int"/> value that specifies a power.</param>
      public void pow(int x, int y) => p.pow(x, y);
      /// <summary>
      /// Replaces the value on top of the stack 
      /// with the power of this number to the specified power y.
      /// </summary>
      /// <param name="y">A <see cref="int"/> value that specifies a power.</param>
      public void pow(int y) => p.pow(y);
      /// <summary>
      /// Pushes the factorial of the specified number c on the stack.
      /// </summary>
      /// <param name="c">The <see cref="int"/> value from which the factorial is calculated.</param>
      public void fac(uint c) => p.fac(c);
      /// <summary>
      /// Returns the MSB difference of numerator and denominator for the value at the top of the stack.<br/>
      /// <c>msb(numerator) - msb(denominator)</c><br/> 
      /// Mainly intended for fast break criterias in iterations as alternative to relatively slow comparisons.<br/>
      /// For example, 1e-24 corresponds to a BDI value of -80 since msb(1e+24) == 80
      /// </summary>
      /// <remarks>
      /// The result equals to an <c>ILog2</c> with a maximum difference of 1 since fractional bits are not tested. 
      /// </remarks>
      /// <returns>An <see cref="int"/> value that represents the MSB difference.</returns>
      public int bdi() => p.bdi();
      /// <summary>
      /// Limitates the binary digits of the value at the top of the stack to the specified count <paramref name="c"/>.<br/>
      /// If the number of digits in numerator or denominator exceeds this count the bits will shifted right by the difference.
      /// </summary>
      /// <remarks>
      /// This is a fast binary rounding operation that limits precision to increase the performance.<br/>
      /// Mainly intended for iterations where the successive numeric operations internally produce much more precision than is finally needed.<br/>
      /// Example: <c>lim(msb(1E+24))</c> to limit the precision to about 24 decimal digits.
      /// </remarks>
      /// <param name="c">A positive number as bit count.</param>
      /// <param name="i">A relative index of a stack entry.</param>
      public void lim(uint c, int i = 0) => p.lim(c, i);
      /// <summary>
      /// Gets a number that indicates the sign of the value at index i relative to the top of the stack.<br/>
      /// Default index 0 returns the sign of the value on top of the stack.
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      /// <returns>
      /// -1 – value is less than zero.<br/>
      /// 0 – value is equal to zero.<br/>
      /// 1 – value is greater than zero.<br/>
      /// </returns>
      public int sign(int a = 0) => p.sign(a);
      /// <summary>
      /// Compares the <u>absolute</u> values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack.<br/>
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      /// <returns>
      /// -1 – value at a is less than value at b.<br/>
      /// 0 – value at a is equal to value at b.<br/>
      /// 1 – value at a is greater than value at b.<br/>
      /// </returns>
      public int cmpa(int a = 0, int b = 1) => p.cmpa(a, b);
      /// <summary>
      /// Compares the values at index <paramref name="a"/> relative to the top of the stack 
      /// with the <see cref="int"/> value <paramref name="b"/>.<br/>
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">An <see cref="int"/> value to compare to.</param>
      /// <returns>
      /// -1 – value at a is less than value b.<br/>
      /// 0 – value at a is equal to value b.<br/>
      /// 1 – value at a is greater than value b.<br/>
      /// </returns>
      public int cmpi(int a, int b) => p.cmpi(a, b);
      /// <summary>
      /// Compares the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack.<br/>
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      /// <returns>
      /// -1 – value at a is less than value at b.<br/>
      /// 0 – value at a is equal to value at b.<br/>
      /// 1 – value at a is greater than value at b.<br/>
      /// </returns>
      public int cmp(int a = 0, int b = 1) => p.cmp(a, b);
      /// <summary>
      /// Compares the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      /// <returns>
      /// -1 – value at a is less than value at b.<br/>
      /// 0 – value at a is equal to value at b.<br/>
      /// 1 – value at a is greater than value at b.<br/>
      /// </returns>
      public int cmp(uint a, uint b) => p.cmp(a, b);
      /// <summary>
      /// Compares the value on top of the stack with the value at b as absolute index in the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="b">Absolute index of a stack entry.</param>
      /// <returns>
      /// -1 – value on top of the stack is less than value at b.<br/>
      /// 0 – value on top of the stack is equal to value at b.<br/>
      /// 1 – value on top of the stack is greater than value at b.<br/>
      /// </returns>
      public int cmp(uint b) => p.cmp(b);
      /// <summary>
      /// Compares the <see cref="BigRational"/> value <paramref name="a"/> with the <see cref="BigRational"/> value b.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      /// <returns>
      /// -1 – a is less than b.<br/>
      /// 0 – a is equal b.<br/>
      /// 1 – a is greater than b.<br/>
      /// </returns>
      public int cmp(BigRational a, BigRational b) => p.cmp(a, b);
      /// <summary>
      /// Compares the value on top of the stack a with the <see cref="BigRational"/> value b.
      /// </summary>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      /// <returns>
      /// -1 – the value on top of the stack is less than b.<br/>
      /// 0 – the value on top of the stack is equal b.<br/>
      /// 1 – the value on top of the stack is greater than b.<br/>
      /// </returns>
      public int cmp(BigRational b) => p.cmp(b);
      /// <summary>
      /// Compares the <see cref="BigRational"/> value <paramref name="a"/> with the <see cref="BigRational"/> value b for equality.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> value as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> value as second value.</param>
      /// <returns>true if the values are equal, otherwise false.</returns>
      public bool equ(BigRational a, BigRational b) => p.equ(a, b);
      /// <summary>
      /// Compares the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack for equality.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      /// <returns>true if the values are equal, otherwise false.</returns>
      public bool equ(uint a, uint b) => p.equ(a, b);
      /// <summary>
      /// Compares the value at index a as absolute index in the stack with the <see cref="BigRational"/> value b for equality.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">A <see cref="BigRational"/> value as second value.</param>
      /// <returns>true if the values are equal, otherwise false.</returns>
      public bool equ(uint a, BigRational b) => p.equ(a, b);
      /// <summary>
      /// Normalize the value at the specified index relative to the top of the stack.
      /// </summary>
      /// <remarks>
      /// Normalization works automatically, 
      /// on demand or for the final results returning as <see cref="BigRational"/>.<br/>
      /// In the case of complex calculations on stack, 
      /// it makes sense to normalize intermediate results manually,
      /// as this can speed up further calculations.<br/>
      /// However, normalization is a time critical operation, 
      /// and too many normalizations can slow down everything.
      /// </remarks>
      /// <param name="i">Relative index of the stack entry to normalize.</param>
      public void norm(int i = 0) => p.norm(i);
      /// <summary>
      /// Calculates a hash value for the value at b as absolute index in the stack.
      /// </summary>
      /// <remarks>
      /// The function is helpful for comparsions and to build dictionarys without creation of <see cref="BigRational"/> objects.<br/>
      /// To get meaningful hash values the function forces normalizations what can be time critical.<br/>
      /// The hash value returned is identical with the hash value returned from <see cref="BigRational.GetHashCode"/> for same values.<br/>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="i">Absolute index of a stack entry.</param>
      /// <returns>A <see cref="uint"/> value as hash value.</returns>
      public uint hash(uint i) => p.hash(i);
      /// <summary>
      /// Returns the MSB (most significant bit) of the numerator of the value on top of the stack.
      /// </summary>
      /// <remarks>
      /// Uses <i>MSB 1 bit numbering</i>, so 0 gives 0, 1 gives 1, 4 gives 2.<br/>
      /// To get the MSB of the denominator is possible by calling <see cref="inv"/> before and after.
      /// </remarks>
      /// <returns>A <see cref="uint"/> value as MSB.</returns>
      public uint msb() => p.msb();
      /// <summary>
      /// Returns the LSB (least significant bit) of the numerator of the value on top of the stack.
      /// </summary>
      /// <remarks>
      /// Uses <i>LSB 1 bit numbering</i>, so 0 gives 0, 1 gives 1, 4 gives 2.<br/>
      /// To get the LSB of the denominator is possible by calling <see cref="inv"/> before and after.
      /// </remarks>
      /// <returns>A <see cref="uint"/> value as MSB.</returns>
      public uint lsb() => p.lsb();
      /// <summary>
      /// Returns whether the value on top of the stack is an integer.
      /// </summary>
      /// <returns>true if the value is integer; false otherwise.</returns>
      public bool isi() => p.isi();
      /// <summary>
      /// Finds the greatest common divisor (GCD) of the numerators<br/>
      /// of the first two values on top of the stack and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// This operation makes only sense for integer values since the denominators are ignored.
      /// </remarks>
      public void gcd() => p.gcd();
      /// <summary>
      /// Converts the value on top of the stack to decimal digits.
      /// </summary>
      /// <remarks>
      /// Can be used for custom string builders, repetition checks etc.
      /// </remarks>
      /// <param name="sp">Span that preserves the decimal digits.</param>
      /// <param name="ns">The number of characters that were written in sp.</param>
      /// <param name="exp">Decimal point position.</param>
      /// <param name="rep">Index of repetition, otherwise -1.</param>
      /// <param name="reps">Check for repetitions.</param>
      public void tos(Span<char> sp, out int ns, out int exp, out int rep, bool reps) => p.tos(sp, out ns, out exp, out rep, reps);
      /// <summary>
      /// Converts a string to a rational number and pushes the result on the stack.
      /// </summary>
      /// <remarks>
      /// Support of the rational specific formats like "1.234'567e-1000" or "1/3"<br/>
      /// <b>Note</b>: There is no check for invalid chars at this layer, 
      /// non-number specific chars, spaces etc. are simply ignored.<br/>
      /// </remarks>
      /// <param name="sp">A Span that preserves the digits.</param>
      /// <param name="bas">The number base, 1 to 10 or 16.<br/>(For decimal a value of 10 is recommanded.)</param>
      /// <param name="sep">A specific (decimal) seperator; otherwise default: '.' or ','.</param>
      public void tor(ReadOnlySpan<char> sp, int bas = 10, char sep = default) => p.tor(sp, bas, sep);
      /// <summary>
      /// Replaces the value on top of the stack with it's square root.<b/>
      /// Negative values result in NaN.
      /// </summary>
      /// <remarks>
      /// The desired precision is controlled by the parameter <paramref name="c"/> 
      /// where <paramref name="c"/> represents a break criteria of the internal iteration.<br/>
      /// For a desired precesission of decimal digits <paramref name="c"/> can be calculated as:<br/> 
      /// <c>msb(pow(10, digits))</c> or<br/> 
      /// <c>(uint)Math.Ceiling(digits * 3.321928094887362) // * (log(2) + log(5)) / log(2))</c>.<br/> 
      /// The result however has to be rounded explicitely to get an exact decimal representation.
      /// </remarks>
      /// <param name="c">The desired precision.</param>
      public void sqrt(uint c) => p.sqrt(c);
      /// <summary>
      /// Replaces the value on top of the stack with it's base 2 logarithm.<b/>
      /// Non-positive values result in NaN.
      /// </summary>
      /// <remarks>
      /// The desired precision is controlled by the parameter <paramref name="c"/> 
      /// where <paramref name="c"/> represents a break criteria of the internal iteration.<br/>
      /// For a desired precesission of decimal digits <paramref name="c"/> can be calculated as:<br/> 
      /// <c>msb(pow(10, digits))</c> or<br/> 
      /// <c>(uint)Math.Ceiling(digits * 3.321928094887362) // * (log(2) + log(5)) / log(2))</c>.<br/> 
      /// The result however has to be rounded explicitely to get an exact decimal representation.
      /// </remarks>
      /// <param name="c">The desired precision.</param>
      public void log2(uint c) => p.log2(c);
      /// <summary>
      /// Replaces the value on top of the stack with it's natural (base e) logarithm.<b/>
      /// Non-positive values result in NaN.
      /// </summary>
      /// <remarks>
      /// The desired precision is controlled by the parameter <paramref name="c"/> 
      /// where <paramref name="c"/> represents a break criteria of the internal iteration.<br/>
      /// For a desired precesission of decimal digits <paramref name="c"/> can be calculated as:<br/> 
      /// <c>msb(pow(10, digits))</c> or<br/> 
      /// <c>(uint)Math.Ceiling(digits * 3.321928094887362) // * (log(2) + log(5)) / log(2))</c>.<br/> 
      /// The result however has to be rounded explicitely to get an exact decimal representation.
      /// </remarks>
      /// <param name="c">The desired precision.</param>
      public void log(uint c) => p.log(c);
      /// <summary>
      /// Replaces the value on top of the stack with e raised to the power of that value.
      /// </summary>
      /// <remarks>
      /// The desired precision is controlled by the parameter <paramref name="c"/> 
      /// where <paramref name="c"/> represents a break criteria of the internal iteration.<br/>
      /// For a desired precesission of decimal digits <paramref name="c"/> can be calculated as:<br/> 
      /// <c>msb(pow(10, digits))</c> or<br/> 
      /// <c>(uint)Math.Ceiling(digits * 3.321928094887362) // * (log(2) + log(5)) / log(2))</c>.<br/> 
      /// The result however has to be rounded explicitely to get an exact decimal representation.
      /// </remarks>
      /// <param name="c">The desired precision.</param>
      public void exp(uint c) => p.exp(c);
      /// <summary>
      /// Calculates <c>π</c> (PI) to the desired precision and push it to the stack.<br/>
      /// Represents the ratio of the circumference of a circle to its diameter.
      /// </summary>
      /// <remarks>
      /// The desired precision is controlled by the parameter <paramref name="c"/> 
      /// where <paramref name="c"/> represents a break criteria of the internal iteration.<br/>
      /// For a desired precesission of decimal digits <paramref name="c"/> can be calculated as:<br/> 
      /// <c>msb(pow(10, digits))</c> or<br/> 
      /// <c>(uint)Math.Ceiling(digits * 3.321928094887362) // * (log(2) + log(5)) / log(2))</c>.<br/> 
      /// The result however has to be rounded explicitely to get an exact decimal representation.
      /// </remarks>
      /// <param name="c">The desired precision.</param>
      public void pi(uint c) => p.pi(c);
      /// <summary>
      /// Replaces the value on top of the stack with the sine or cosine of that value.<br/>
      /// The value interpreted as an angle is measured in radians.
      /// </summary>
      /// <remarks>
      /// The desired precision is controlled by the parameter <paramref name="c"/> 
      /// where <paramref name="c"/> represents a break criteria of the internal iteration.<br/>
      /// For a desired precesission of decimal digits <paramref name="c"/> can be calculated as:<br/> 
      /// <c>msb(pow(10, digits))</c> or<br/> 
      /// <c>(uint)Math.Ceiling(digits * 3.321928094887362) // * (log(2) + log(5)) / log(2))</c>.<br/> 
      /// The result however has to be rounded explicitely to get an exact decimal representation.
      /// </remarks>
      /// <param name="c">The desired precision.</param>
      /// <param name="cos">If true, the cosine is calculated, otherwise the sine.</param>
      public void sin(uint c, bool cos) => p.sin(c, cos);
      /// <summary>
      /// Replaces the value on top of the stack with the atan of that value.<br/>
      /// The value interpreted as an angle is measured in radians.
      /// </summary>
      /// <remarks>
      /// The desired precision is controlled by the parameter <paramref name="c"/> 
      /// where <paramref name="c"/> represents a break criteria of the internal iteration.<br/>
      /// For a desired precesission of decimal digits <paramref name="c"/> can be calculated as:<br/> 
      /// <c>msb(pow(10, digits))</c> or<br/> 
      /// <c>(uint)Math.Ceiling(digits * 3.321928094887362) // * (log(2) + log(5)) / log(2))</c>.<br/> 
      /// The result however has to be rounded explicitely to get an exact decimal representation.<br/> 
      /// <b>Note</b>: In the current version, the function has not yet been finally optimized for performance<br/> 
      /// and the accuracy of the last digits has not yet been ensured!
      /// </remarks>
      /// <param name="c">The desired precision.</param>
      public void atan(uint c) => p.atan(c);
      /// <summary>
      /// Frees the current thread static instance of the <see cref="CPU"/> and associated buffers.<br/>
      /// A new <see cref="CPU"/> is then automatically created for subsequent calculations.
      /// </summary>
      /// <remarks>
      /// After calculating with large numbers, the <see cref="CPU"/> naturally keeps large buffers that can be safely released in this way.<br/>
      /// More precisely, the operation does not destroy anything. The current CPU instance can continue to be used. But the GC can collect them later when there are no more references to them.
      /// </remarks>
      public void free() => p.free();
    }

    /// <summary>
    /// Safe thread static instance of a <see cref="CPU"/> for general use.
    /// </summary>
    /// <remarks>
    /// Recomandation: add to Watch for Debug 
    /// </remarks>
    public static SafeCPU task_cpu => new SafeCPU(main_cpu);
  }
}
