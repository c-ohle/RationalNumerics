using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace System.Numerics
{
  /// <summary>
  /// Represents an arbitrarily large rational number.
  /// </summary>                                                    
  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public unsafe readonly partial struct BigRational : IComparable<BigRational>, IComparable, IEquatable<BigRational>, IFormattable, ISpanFormattable
  {
    /// <summary>
    /// Converts the numeric value of the current <see cref="BigRational"/> instance to its equivalent string representation.
    /// </summary>
    /// <remarks>
    /// Behavior like <see cref="double.ToString()"/> 
    /// with decimal notation in the range E-04 to E+16<br/>
    /// Special: Maximum number of digits is by default 32.<br/>
    /// Special: Repetions marked with apostrophe (') like 1/3 as "0.'3" instead of "0.333333…"<br/>
    /// Special: Overflow marked with trailing dots (…) like: "1.234567…"<br/>
    /// </remarks>
    /// <returns>The string representation of the current <see cref="BigRational"/> value.</returns>
    public override readonly string ToString()
    {
      Span<char> sp = stackalloc char[64];
      TryFormat(sp, out var ns); return sp.Slice(0, ns).ToString();
    }
    /// <summary>
    /// Formats this <see cref="BigRational"/> instance to
    /// its string representation by using the specified format and culture-specific
    /// format information.
    /// </summary>
    /// <remarks>
    /// Supported numeric standard formats: C, D, E, F, G, N, P, R like "E10".<br/>
    /// Custom formats supported, currently in the range +/- E-308 .. E+308 like "0.#####"<br/>
    /// Standard formats F, E with unlimited many digits like: "F1000" for 1000 decimal digits.<br/>
    /// Special R: behavior like <see cref="double.ToString(string?)"/> with fallback to format Q to ensure the round-trip to an identical number.<br/>
    /// Additional: Format S for Standard (no rounding, repetion checks, overflow sign) with specific maximum overall digits count.<br/>
    /// Additional: Format Q for Fraction notation returns "1/3" instead of "0.333333…"<br/>
    /// Additional: Format L for Large, like F without trailing zeros: L5 returns 1.5 instead of 1.50000…<br/>
    /// </remarks>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of the current <see cref="BigRational"/> value as
    /// specified by the format and provider parameters.</returns>
    public readonly string ToString(string? format, IFormatProvider? provider = default)
    {
      var dbg = format != default && format.Length == 0; // DebuggerDisplay("", null);
      if (dbg && isnan(this.p)) return NumberFormatInfo.InvariantInfo.NaNSymbol; // extra safety at debug, allows deep stack debug
      //if (dbg) { var cpu = task_cpu; var p = cpu.sp; if (p != null) { cpu.sp = null; var t = ToString(format, provider); cpu.sp = p; return t + " [" + ((long)p - (long)&p).ToString() + "]"; } }
      provider ??= dbg ? NumberFormatInfo.InvariantInfo : NumberFormatInfo.CurrentInfo;
      Span<char> sp = stackalloc char[100]; char[]? a = null; string? s = null;
      for (int ns, na = 1024; ;)
      {
        if (TryFormat(sp, out ns, format, provider)) s = sp.Slice(0, ns).ToString();
        if (a != null) ArrayPool<char>.Shared.Return(a);
        if (s != null) break; sp = a = ArrayPool<char>.Shared.Rent(na = Math.Max(ns, na << 1));
      }
      #region debug ext
      if (dbg && p != null && (p[0] & 0x40000000) != 0) // debug nd info ₀ ₀  
      {
        var x = p[0] & 0x3fffffff; var i = s.Length; s += ' ' + x.ToString() + ' ' + p[x + 1].ToString();
        fixed (char* p = s) for (int n = s.Length; i < n; i++) { var c = s[i]; if (c != ' ') p[i] = (char)('₀' + (c - '0')); }
      }
      static bool isnan(uint[] p) // only for extra safety at debug, to debug into the core
      {
        if (p == null) return false;
        var u = p[0] & 0x3fffffff; if (u == 0 || u + 3 > p.Length) return true;
        var v = p[u + 1]; if (v == 0 || u + v + 2 > p.Length) return true;
        if (u != 1 && p[u] == 0) return true;
        if (p[u + v + 1] == 0) return true;
        return false;
      }
      #endregion
      return s;
    }
    /// <summary>
    /// Formats this <see cref="BigRational"/> instance into a span of characters.
    /// </summary>
    /// <remarks>
    /// Supported formats see: <see cref="BigRational.ToString(string?, IFormatProvider?)"/>.
    /// </remarks>
    /// <param name="destination">The span of characters into which this instance will be written.</param>
    /// <param name="charsWritten">When the method returns, contains the length of the span in number of characters.</param>
    /// <param name="format">A read-only span of characters that specifies the format for the formatting operation.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information about value.</param>
    /// <returns>true if the formatting operation succeeds; false otherwise.</returns>
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
      int digs = 32, round = -1, emin = -4, emax = +16; char fc = '\0', tc = fc;
      //if (format != default && format.Length == 0) digs = DefaultDigits; //todo: check makes sense for debug?
      var info = NumberFormatInfo.GetInstance(provider);
      if (format.Length != 0)
      {
        if (char.IsLetter(tc = format[0]) && (format.Length == 1 || (char.IsNumber(format[1]) && int.TryParse(format.Slice(1), out round))))
          switch (fc = char.ToUpper(tc))
          {
            case 'Q': fc = 'R'; digs = 0; round = -1; goto def; // fraction form "1/3"
            case 'R':
            case 'S':
              if (round == -1) goto def; digs = round; round = -1;
              if (digs > destination.Length && destination.Length == 100) { charsWritten = digs; return false; } //hint for ToString(,)
              goto def;
            case 'F':
            case 'L': // like F without trailing zeros
              if (round == -1) round = info.NumberDecimalDigits;
              digs = Math.Max(0, ILog10(this)) + 1 + round;
              if (digs > destination.Length && destination.Length == 100) { charsWritten = digs; return false; } //hint for ToString(,)
              emin = -(emax = int.MaxValue); goto def;
            case 'E':
              if (round == -1) round = 6; digs = 1 + round;
              if (digs + 32 > destination.Length && destination.Length == 100) { charsWritten = digs + 32; return false; } //hint for ToString(,)
              emax = -(emin = int.MaxValue); goto def;
              //case 'X': throw new FormatException(nameof(format));
          }
        var e = ILog10(this);
        if (e <= 28) return ((decimal)this).TryFormat(destination, out charsWritten, format, provider);
        if (e <= 308) return ((double)this).TryFormat(destination, out charsWritten, format, provider);
      }
    def:
      var pb = digs <= 0x8000 ? null : ArrayPool<char>.Shared.Rent(digs);
      var ts = stackalloc char[pb != null ? 0 : digs];
      var ss = pb != null ? pb.AsSpan().Slice(0, digs) : new Span<char>(ts, digs);
      var cpu = main_cpu; cpu.push(this);
      if (round >= 0) cpu.rnd(fc == 'E' ? Math.Max(0, round - ILog10(this)) : round);
      cpu.tos(ss, out var ns, out var exp, out var rep, round == -1);
      var ofl = ns == ss.Length && round == -1;
      ss = ss.Slice(0, ns); var ws = destination;
      if (ns == 0 && digs != 0) { aps(ref ws, info.NaNSymbol); goto ret; }
      if (ofl && fc == 'R') { cpu.push(this); fra(ref ws, cpu); goto ret; }
      if (Sign(this) < 0) aps(ref ws, info.NegativeSign);
      if (emax > emin)
      {
        if (!ofl && rep == -1 && exp + 1 >= ns && exp <= emax) // 0, 1, 1234, 1234000
        {
          aps(ref ws, ss.Slice(0, ns)); apc(ref ws, '0', exp - ns + 1);
          if (fc == 'F' && round > 0) { aps(ref ws, info.NumberDecimalSeparator); apc(ref ws, '0', round); }
          goto ret;
        }
        if (exp >= 0 && exp <= emax && exp + 1 < ns && (rep == -1 || rep >= exp + 1)) // 123.456, 1.2'34
        {
          aps(ref ws, ss.Slice(0, exp + 1));
          aps(ref ws, info.NumberDecimalSeparator);
          if (rep == -1) { aps(ref ws, ss.Slice(exp + 1)); if (fc == 'F') apc(ref ws, '0', round - (ns - exp - 1)); }
          else { aps(ref ws, ss.Slice(exp + 1, rep - exp - 1)); apc(ref ws, '\'', 1); aps(ref ws, ss.Slice(rep)); }
          if (ofl) apc(ref ws, '…', 1); goto ret;
        }
        if (exp < 0 && exp >= emin) // 0.00123, 0.0012'3
        {
          int u = -exp - 1, v = 0; if (rep != -1) while (u - v > 0 && ss[ss.Length - 1 - v] == '0') v++; //prime reciprocals, 1/17 0,0'58..70
          apc(ref ws, '0', 1); aps(ref ws, info.NumberDecimalSeparator); apc(ref ws, '0', u - v);
          if (rep == -1) { aps(ref ws, ss); if (fc == 'F') apc(ref ws, '0', round - (ns - exp - 1)); }
          else { aps(ref ws, ss.Slice(0, rep)); apc(ref ws, '\'', 1); apc(ref ws, '0', v); aps(ref ws, ss.Slice(rep, ss.Length - rep - v)); }
          if (ofl) apc(ref ws, '…', 1); goto ret;
        }
      }
      if (rep == 0)
      {
        apc(ref ws, '0', 1); aps(ref ws, info.NumberDecimalSeparator);
        apc(ref ws, '\'', 1); aps(ref ws, ss); exp++;
      }
      else
      {
        aps(ref ws, ss.Slice(0, 1));
        if (ns > 1 || fc == 'E')
        {
          aps(ref ws, info.NumberDecimalSeparator);
          if (rep == -1) aps(ref ws, ss.Slice(1));
          else { aps(ref ws, ss.Slice(1, rep - 1)); apc(ref ws, '\'', 1); aps(ref ws, ss.Slice(rep)); }
        }
      }
      if (fc == 'E') apc(ref ws, '0', digs - ss.Length);
      if (ofl) apc(ref ws, '…', 1);
      if (exp != 0 || fc == 'E')
      {
        apc(ref ws, fc == tc ? 'E' : 'e', 1);
        aps(ref ws, exp >= 0 ? info.PositiveSign : info.NegativeSign);
        if (Math.Abs(exp).TryFormat(ws, out var nw, fc == 'E' ? "000" : "00")) ws = ws.Slice(nw); else ws = default;
      }
    ret:
      if (pb != null) ArrayPool<char>.Shared.Return(pb);
      if (ws == default) { charsWritten = 0; return false; }
      charsWritten = destination.Length - ws.Length; return true;
      static void aps(ref Span<char> ws, ReadOnlySpan<char> s)
      {
        if (s.TryCopyTo(ws)) ws = ws.Slice(s.Length); else ws = default;
      }
      static void apc(ref Span<char> ws, char c, int n)
      {
        if (n <= 0) return; if (n > ws.Length) { ws = default; return; }
        ws.Slice(0, n).Fill(c); ws = ws.Slice(n);
      }
      static void fra(ref Span<char> ws, CPU cpu)
      {
        var s = cpu.sign(); int x = 0; cpu.mod(8);
        for (int i = 2; ;)
        {
          if (x + 3 > ws.Length) { cpu.pop(i); ws = default; return; }
          cpu.push(10u); cpu.div(); cpu.mod(); cpu.swp();
          ws[x++] = (char)('0' + cpu.popi());
          if (cpu.sign() != 0) continue;
          cpu.pop(); if (--i == 0) break; ws[x++] = '/';
        }
        if (s < 0) ws[x++] = '-'; ws.Slice(0, x).Reverse(); ws = ws.Slice(x);
      }
    }
    /// <summary>
    /// Converts the representation of a number, contained in the specified read-only 
    /// span of characters to its <see cref="BigRational"/> equivalent.
    /// </summary>
    /// <remarks>
    /// Like <see cref="double.Parse(string)"/> but with unlimited number of digits.<br/>
    /// Additional: Converts Fraction notation like "1/3" or "-123/456"<br/> 
    /// Additional: Converts Repetions like "0.'3", see: <see cref="BigRational.ToString(string?, IFormatProvider?)"/>.<br/>
    /// Ignores spaces, underscores (_) leading dots (…)
    /// </remarks>
    /// <param name="value">A read-only span of characters that contains the number to convert.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about value.</param>
    /// <returns>A value that is equivalent to the number specified in the value parameter.</returns>
    public static BigRational Parse(ReadOnlySpan<char> value, IFormatProvider? provider = null)
    {
      var info = provider != null ? NumberFormatInfo.GetInstance(provider) : null;
      var cpu = main_cpu; cpu.tor(value, 10, info != null ? info.NumberDecimalSeparator[0] : default);
      return cpu.popr();
    }
    /// <summary>Parses a string into a value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="s">The string to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="s" /> is <c>null</c>.</exception>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException"><paramref name="s" /> is not representable by result.</exception>
    public static BigRational Parse(string s, NumberStyles style, IFormatProvider? provider = null)
    {
      return Parse(s.AsSpan(), style, provider);
    }
    /// <summary>Parses a span of characters into a value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.</exception>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException"><paramref name="s" /> is not representable by result.</exception>
    public static BigRational Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
      if ((style & NumberStyles.AllowHexSpecifier) != 0) throw new ArgumentException(nameof(style));
      return Parse(s, provider);
    }
    /// <summary>Parses a string into a value.</summary>
    /// <remarks>Part of the new NET 7 number type system.</remarks>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s" /> is <c>null</c>.</exception>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException"><paramref name="s" /> is not representable by result.</exception>
    public static BigRational Parse(string s, IFormatProvider? provider)
    {
      var r = Parse(s.AsSpan(), provider);
      if (IsNaN(r)) throw new ArgumentException(nameof(s)); return r;
    }
    /// <summary>
    /// Copies the value of this <see cref="BigRational"/> as little-endian twos-complement bytes.<br/>
    /// If the value is zero, outputs one uint whose element is 0x00000000.
    /// </summary>
    /// <remarks>
    /// The destination Span will get the offset of the number of bytes written.<br/>
    /// In case of undersized destination, as signal the span is reset to default.<br/>
    /// This allows efficient code in serialization sequences.
    /// </remarks>
    /// <example><code>
    /// X.WriteToBytes(ref dest);<br/>Y.WriteToBytes(ref dest);<br/>Z.WriteToBytes(ref dest);
    /// if(dest == default) return false; //undersized destination
    /// </code></example>
    /// <param name="destination">The destination span to which the resulting bytes should be written.</param>
    /// <returns>The number of bytes written or required in case of an empty or undersized destination.</returns>
    public readonly int WriteToBytes(ref Span<byte> destination)
    {
      fixed (uint* s = this.p)
      {
        var n = s != null ? this.p.Length << 2 : 4;
        if (!new ReadOnlySpan<byte>(s != null ? s : &s, n).TryCopyTo(destination)) destination = default;
        else destination = destination.Slice(n); return n;
      }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="BigRational"/> structure using
    /// the values in a read-only span of bytes.
    /// </summary>
    /// <remarks>
    /// Reverse function of <see cref="BigRational.WriteToBytes"/> 
    /// </remarks>
    /// <param name="value">A read-only span of bytes representing the <see cref="BigRational"/> number.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    /// <exception cref="FormatException">Signals an invalid format.</exception>
    public static BigRational ReadFromBytes(ref ReadOnlySpan<byte> value)
    {
      if (value.Length < 4) throw new FormatException(nameof(value));
      fixed (byte* s = &value.GetPinnableReference())
      {
        if (*(uint*)s == 0) { value = value.Slice(4); return default; }
        if (value.Length < 16) throw new FormatException(nameof(value));
        var n = len((uint*)s); var p = new uint[n];
        fixed (uint* d = p) copy(d, (uint*)s, n);
        value = value.Slice((int)n << 2); return new BigRational(p);
      }
    }
    /// <summary>
    /// Returns the hash code for the current <see cref="BigRational"/> number.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override readonly int GetHashCode()
    {
      if (this.p == null) return 0x00280081;
      uint h = 0; Debug.Assert((p[0] & 0x40000000) == 0);
      fixed (uint* p = this.p)
        for (uint i = 0, n = len(p); i < n; i++)
          h = ((h << 7) | (h >> 25)) ^ p[i]; //todo: check BitOperations.RotateLeft faster than shifts?
      return unchecked((int)h);
    }
    /// <summary>
    /// Returns a value that indicates whether the current instance and a specified object have the same value.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>
    /// true if the obj argument is a <see cref="BigRational"/> number, 
    /// and its value is equal to the value of the current <see cref="BigRational"/> instance; 
    /// otherwise, false.
    /// </returns>
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is BigRational r && Equals(r);
    }
    /// <summary>
    /// Returns a value that indicates whether the current instance and a specified <see cref="BigRational"/> number have the same value.
    /// </summary>
    /// <param name="b">The object to compare.</param>
    /// <returns>true if this <see cref="BigRational"/> number and other have the same value; otherwise, false.</returns>
    public readonly bool Equals(BigRational b)
    {
      return main_cpu.equ(this, b);
    }
    /// <summary>
    /// Returns a value that indicates whether the current instance and a specified <see cref="long"/> number have the same value.
    /// </summary>
    /// <param name="b">The object to compare.</param>
    /// <returns>true if this <see cref="long"/> number and other have the same value; otherwise, false.</returns>
    public readonly bool Equals(long b)
    {
      //return CompareTo(b) == 0; //if (p == null) return b == 0; //todo: check cc?, benchmarks
      var cpu = main_cpu; cpu.push(b); var s = cpu.equ(cpu.mark() - 1, this); cpu.pop(); return s;
    }
    /// <summary>
    /// Compares this object to another object, returning an instance of System.Relation.<br/>
    /// Null is considered less than any instance.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>
    /// A signed integer value that indicates the relationship of this instance to other,
    /// as shown in the following table.<br/>
    /// Return value – Description<br/>
    /// Less than zero – The current instance is less than other.<br/>
    /// Zero – The current instance equals other.<br/>
    /// Greater than zero – The current instance is greater than other.<br/>
    /// </returns>
    /// <exception cref="ArgumentException">If obj is not null and not of type <see cref="BigRational"/>.</exception>
    public readonly int CompareTo(object? obj)
    {
      if (obj == null) return 1;
      if (obj is not BigRational v) throw new ArgumentException(nameof(obj));
      return this.CompareTo(v);
    }
    /// <summary>
    /// Compares this instance to a second <see cref="BigRational"/> and returns an
    /// integer that indicates whether the value of this instance is less than, equal
    /// to, or greater than the value of the specified object.
    /// </summary>
    /// <param name="b">The object to compare.</param>
    /// <returns>
    /// A signed integer value that indicates the relationship of this instance to other,
    /// as shown in the following table.<br/>
    /// Return value – Description<br/>
    /// Less than zero – The current instance is less than other.<br/>
    /// Zero – The current instance equals other.<br/>
    /// Greater than zero – The current instance is greater than other.<br/>
    /// </returns>
    public readonly int CompareTo(BigRational b)
    {
      return main_cpu.cmp(this, b);
    }
    /// <summary>
    /// Compares this instance to a second <see cref = "long" /> and returns an
    /// integer that indicates whether the value of this instance is less than, equal
    /// to, or greater than the value of the specified object.
    /// </summary>
    /// <remarks>
    /// Fast shortcut for the most common comparisons like: <c>x == 0; x &lt;= 1; x != -1;</c> etc.
    /// </remarks>
    /// <param name="b"><see cref="long"/> value to compare.</param>
    /// <returns>
    /// A signed integer value that indicates the relationship of this instance to other,
    /// as shown in the following table.<br/>
    /// Return value – Description<br/>
    /// Less than zero – The current instance is less than other.<br/>
    /// Zero – The current instance equals other.<br/>
    /// Greater than zero – The current instance is greater than other.<br/>
    /// </returns>
    public readonly int CompareTo(long b)
    {
      if (b == 0) return Sign(this);
      if (p == null) return -Math.Sign(b);
      var cpu = main_cpu; cpu.push(b); var s = cpu.cmp(this); cpu.pop(); return -s;
    }

    /// <summary>
    /// Defines an explicit bit-exact conversion of a <see cref="float"/> value to a <see cref="BigRational"/> value.
    /// </summary>
    /// <remarks>
    /// This as alternative to the explicit <see cref="float"/> conversion.<br/>
    /// <c>var x = new <see cref="BigRational"/>(0.1f);</c><br/>
    /// results in <c>0.100000001490116119384765625</c> as bit-exact interpretation.<br/>
    /// where the more common explicit conversion:<br/>
    /// <c>var x = (<see cref="BigRational"/>)0.1f;</c><br/>
    /// results in <c>0.1</c> as this function implicitly rounds to the precision of significant bits of <see cref="float"/>.
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public BigRational(float value)
    {
      var cpu = main_cpu; cpu.push(value, true); p = cpu.popr().p;
    }
    /// <summary>
    /// Defines an explicit bit-exact conversion of a <see cref="double"/> value to a <see cref="BigRational"/> value.
    /// </summary>
    /// <remarks>
    /// This as alternative to the explicit <see cref="double"/> conversion.<br/>
    /// <c>var x = new <see cref="BigRational"/>(0.1);</c><br/>
    /// results in <c>0.10000000000000000555111512312578…</c> as bit-exact interpretation.<br/>
    /// where the more common explicit conversion:<br/>
    /// <c>var x = (<see cref="BigRational"/>)0.1;</c><br/>
    /// results in <c>0.1</c> as this function implicitly rounds to the precision of significant bits of <see cref="double"/>.
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public BigRational(double value)
    {
      var cpu = main_cpu; cpu.push(value, true); p = cpu.popr().p;
    }

    /// <summary>
    /// Defines an implicit conversion of a <see cref="int"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(int value)
    {
      if (value == 0) return default;
      var cpu = main_cpu; cpu.push(value); return cpu.popr();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="uint"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(uint value)
    {
      if (value == 0) return default;
      var cpu = main_cpu; cpu.push(value); return cpu.popr();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="long"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(long value)
    {
      if (value == 0) return default;
      var cpu = main_cpu; cpu.push(value); return cpu.popr();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="ulong"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(ulong value)
    {
      if (value == 0) return default;
      var cpu = main_cpu; cpu.push(value); return cpu.popr();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="float"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <remarks>
    /// This function implicitly rounds to the precision of significant bits of <see cref="float"/>.<br/>
    /// This rounding is identical to the <see cref="decimal"/> rounding of <see cref="float"/> values during conversion.
    /// For a bit-exact conversion see: <see cref="BigRational(float)"/>
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is rounded to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(float value)
    {
      if (value == 0) return default;
      var cpu = main_cpu; cpu.push(value); return cpu.popr();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="double"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <remarks>
    /// This function implicitly rounds to the precision of significant bits of <see cref="double"/>.<br/>
    /// This rounding is identical to the <see cref="decimal"/> rounding of <see cref="double"/> values during conversion.
    /// For a bit-exact conversion see: <see cref="BigRational(System.Double)"/>
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is rounded to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(double value)
    {
      if (value == 0) return default;
      var cpu = main_cpu; cpu.push(value); return cpu.popr();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="decimal"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(decimal value)
    {
      var cpu = main_cpu; cpu.push(value); return cpu.popr();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="BigInteger"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational(BigInteger value)
    {
      var cpu = main_cpu; cpu.push(value); return cpu.popr();
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="string"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <remarks>
    /// Shortcut for <c>Parse</c> allows simple syntax like:<br/>
    /// <c>var x = (<see cref="BigRational"/>)"1.23'456e+1000";</c>
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static explicit operator BigRational(string value)
    {
      return Parse(value);
    }

    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="byte"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="byte"/>.</returns>
    public static explicit operator byte(BigRational value)
    {
      return (byte)(uint)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="sbyte"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="sbyte"/>.</returns>
    public static explicit operator sbyte(BigRational value)
    {
      return (sbyte)(int)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="short"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="short"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="short"/>.</returns>
    public static explicit operator short(BigRational value)
    {
      return (short)(int)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="ushort"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="ushort"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="ushort"/>.</returns>
    public static explicit operator ushort(BigRational value)
    {
      return (ushort)(uint)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="char"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="char"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="char"/>.</returns>
    public static explicit operator char(BigRational value)
    {
      return (char)(uint)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="int"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="int"/>.</returns>
    public static explicit operator int(BigRational value)
    {
      var a = default(int); main_cpu.toi(value, (uint*)&a, 0x0001); return a;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="uint"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="uint"/>.</returns>
    public static explicit operator uint(BigRational value)
    {
      var a = default(uint); main_cpu.toi(value, (uint*)&a, 0x0101); return a;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="long"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="long"/>.</returns>
    public static explicit operator long(BigRational value)
    {
      var a = default(long); main_cpu.toi(value, (uint*)&a, 0x0002); return a;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="ulong"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="ulong"/>.</returns>
    public static explicit operator ulong(BigRational value)
    {
      var a = default(ulong); main_cpu.toi(value, (uint*)&a, 0x0102); return a;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="float"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="float"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="float"/>.</returns>
    public static explicit operator float(BigRational value)
    {
      return (float)(double)value; //todo: fast float convert
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="IntPtr"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="IntPtr"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="IntPtr"/>.</returns>
    public static explicit operator nint(BigRational value)
    {
      return (nint)(long)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="UIntPtr"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="UIntPtr"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="UIntPtr"/>.</returns>
    public static explicit operator nuint(BigRational value)
    {
      return (nuint)(ulong)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="Half"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="Half"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="Half"/>.</returns>
    public static explicit operator Half(BigRational value)
    {
      return (Half)(double)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="double"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="double"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="double"/>.</returns>
    public static explicit operator double(BigRational value)
    {
      if (value.p == null) return 0;
      fixed (uint* a = value.p)
      {
        var na = a[0] & 0x3fffffff; var b = a + (na + 1); var nb = b[0];  //todo: opt. check performance using lim(64)
        var ca = BitOperations.LeadingZeroCount(a[na]);
        var cb = BitOperations.LeadingZeroCount(b[nb]);
        var va = ((ulong)a[na] << 32 + ca) | (na < 2 ? 0 : ((ulong)a[na - 1] << ca) | (na < 3 ? 0 : (ulong)a[na - 2] >> 32 - ca));
        var vb = ((ulong)b[nb] << 32 + cb) | (nb < 2 ? 0 : ((ulong)b[nb - 1] << cb) | (nb < 3 ? 0 : (ulong)b[nb - 2] >> 32 - cb));
        if (vb == 0) return double.NaN;
        var e = ((na << 5) - ca) - ((nb << 5) - cb);
        if (e < -1021) return double.NegativeInfinity;
        if (e > +1023) return double.PositiveInfinity; //todo: opt. extended over ctor
        var r = (double)(va >> 11) / (vb >> 11);
        var x = (0x3ff + e) << 52; r *= *(double*)&x; //fast r *= Math.Pow(2, e);
        if ((a[0] & 0x80000000) != 0) r = -r; return r;
      }
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="decimal"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="decimal"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="decimal"/>.</returns>
    public static explicit operator decimal(BigRational value)
    {
      if (value.p == null) return default;
      fixed (uint* p = value.p)
      {
        var d = dec(p, out var s) / dec(p + (p[0] & 0x3fffffff) + 1, out var t);
        if (s != t) d *= new decimal(Math.Pow(2, s - t)); return d;
      }
      static decimal dec(uint* p, out int e)
      {
        var n = p[0] & 0x3fffffff; var c = n < 4 ? n : 4;
        var t = stackalloc uint[5] { c, 0, 0, 0, 0 }; copy(t + 1, p + 1 + (n - c), c);
        e = (int)(n << 5) - BitOperations.LeadingZeroCount(p[n]) - 96;
        if (e <= 0) e = 0; else CPU.shr(t, e - ((int)(n - c) << 5));
        return new decimal((int)t[1], (int)t[2], (int)t[3], (p[0] & 0x80000000) != 0, 0);
      }
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="BigInteger"/> value.
    /// </summary>
    /// <remarks>
    /// The result is truncated to integer by default.<br/>
    /// Consider rounding the value before using <see cref="Round(BigRational)"/>, <see cref="Round(BigRational, int)"/>  or <see cref="Round(BigRational, int, MidpointRounding)"/> methods.
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="BigInteger"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="BigInteger"/>.</returns>
    public static explicit operator BigInteger(BigRational value)
    {
      if (value.p == null) return default;
      var cpu = main_cpu; cpu.push(value); cpu.rnd(0, 0); // trunc
      cpu.get(cpu.mark() - 1, out BigInteger r); cpu.pop(); return r;
    }
    /// <summary>
    /// Defines an explicit access to the internal data representation of a <see cref="BigRational"/> number.
    /// </summary>
    /// <param name="value">A <see cref="BigRational"/> number.</param>
    public static explicit operator ReadOnlySpan<uint>(BigRational value)
    {
      return value.p;
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="int"/> object to a <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="BigRational"/>.</param>
    /// <returns>A <see cref="BigRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator BigRational?(int value)
    {
      var cpu = main_cpu; if (value != 0) { cpu.push(value); return cpu.popr(); }
      if (cpu.sp == null) cpu.sp = &value; // debug visualizer security
      return cpu.i != 0 ? new BigRational(cpu.p[cpu.i - 1]) : default;
    }

    /// <summary>
    /// Returns the value of the <see cref="BigRational"/> operand. (The sign of the operand is unchanged.)
    /// </summary>
    /// <param name="a">An <see cref="BigRational"/> value.</param>
    /// <returns>The value of the value operand.</returns>
    public static BigRational operator +(BigRational a)
    {
      return a;
    }
    /// <summary>
    /// Negates a specified <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="a">The value to negate.</param>
    /// <returns>The result of the value parameter multiplied by negative one (-1).</returns>
    public static BigRational operator -(BigRational a)
    {
      var cpu = main_cpu; cpu.push(a); cpu.neg(); return cpu.popr();
    }
    /// <summary>
    /// Increments a <see cref="BigRational"/> value by one.
    /// </summary>
    /// <param name="value">The value to increment.</param>
    /// <returns>The result of incrementing <paramref name="value" />.</returns>
    public static BigRational operator ++(BigRational value)
    {
      var cpu = main_cpu; cpu.push(1u); cpu.add(value); return cpu.popr();
    }
    /// <summary>
    /// Decrements a <see cref="BigRational"/> value by one.
    /// </summary>
    /// <param name="value">The value to decrement.</param>
    /// <returns>The result of decrementing <paramref name="value" />.</returns>
    public static BigRational operator --(BigRational value)
    {
      var cpu = main_cpu; cpu.push(value); cpu.push(1u); cpu.sub(); return cpu.popr();
    }
    /// <summary>
    /// Adds the values of two specified <see cref="BigRational"/> numbers.
    /// </summary>
    /// <param name="a">The first value to add.</param>
    /// <param name="b">The second value to add.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static BigRational operator +(BigRational a, BigRational b)
    {
      var cpu = main_cpu; cpu.add(a, b); return cpu.popr();
    }
    /// <summary>
    /// Subtracts a <see cref="BigRational"/> value from another <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="a">The value to subtract from (the minuend).</param>
    /// <param name="b">The value to subtract (the subtrahend).</param>
    /// <returns>The result of subtracting b from a.</returns>
    public static BigRational operator -(BigRational a, BigRational b)
    {
      var cpu = main_cpu; cpu.sub(a, b); return cpu.popr();
    }
    /// <summary>
    /// Multiplies two specified <see cref="BigRational"/> values.
    /// </summary>
    /// <param name="a">The first value to multiply.</param>
    /// <param name="b">The second value to multiply.</param>
    /// <returns>The product of left and right.</returns>
    public static BigRational operator *(BigRational a, BigRational b)
    {
      var cpu = main_cpu;
      if (a.p == b.p) { cpu.push(a); cpu.sqr(); } else cpu.mul(a, b);
      return cpu.popr();
    }
    /// <summary>
    /// Divides a specified <see cref="BigRational"/> value by another specified <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (devisor)</param>
    /// <returns>The result of the division. NaN when divided by zero.</returns>
    public static BigRational operator /(BigRational a, BigRational b)
    {
      if (b.p == null) return double.NaN; //NET 7 req. //throw new DivideByZeroException(nameof(b));
      var cpu = main_cpu; cpu.div(a, b); return cpu.popr();
    }
    /// <summary>
    /// Returns the remainder that results from division with two specified <see cref="BigRational"/> values.
    /// </summary>
    /// <remarks>
    /// For fractions, the same rules apply to modulus as to floating point types such as <see cref="double"/><br/>
    /// where <c>a % b</c> is defined as: <c>a - Truncate(a / b) * b</c>
    /// </remarks>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (divisor)</param>
    public static BigRational operator %(BigRational a, BigRational b)
    {
      //return a - Truncate(a / b) * b;
      if (b.p == null) return double.NaN; //NET 7 req. //throw new DivideByZeroException(nameof(b));
      var cpu = main_cpu; //todo: % optimization for integers
      cpu.div(a, b); cpu.mod(); cpu.swp(); cpu.pop();
      cpu.mul(b); cpu.neg(); cpu.add(a); return cpu.popr();
    }
    /// <summary>
    /// Performs a bitwise Or operation on two <see cref="BigRational"/> values.
    /// </summary>
    /// <remarks>
    /// The special case Zero Or before a target assignment activates an internal optimization.<br/>
    /// Significant better performance without normalization and allocs for intermediate results.<br/>
    /// Example:<br/><br/>
    /// <c>var x = a * b + c * d + e * f; // standard notation.</c><br/>
    /// <c>var y = 0 | a * b + c * d + e * f; // 5x faster for this example!</c><br/><br/>
    /// <i>For C# there is currently no better way to achieve such performance with standard notation<br/>since the compiler does not support assign-operators.</i>
    /// </remarks>
    /// <returns>The result of the bitwise Or operation.</returns>
    public static BigRational operator |(BigRational? a, BigRational b)
    {
      var cpu = main_cpu; var p = a.GetValueOrDefault();
      if (p.p != null) { cpu.push(p); cpu.push(b); cpu.or(); return cpu.popr(); }
      var i = cpu.i; if (i == 0) return b;
      //todo: a == 0 && k out of index is the perfect RT check, but also safe to distinguish - boost or op
      var k = 0; if (p.p != null) { for (; k < i && cpu.p[k] != p.p; k++) ; k++; } //frame support //todo: check opt. test reverse?
      var t = cpu.sp; cpu.sp = null; cpu.get(unchecked((uint)(i - 1)), out BigRational r); //fetch
      if (k != 0) cpu.sp = t; cpu.pop(i - k); return r;
    }

    /// <summary>
    /// Returns a value that indicates whether the values of two <see cref="BigRational"/> numbers are equal.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if the left and right parameters have the same value; otherwise, false.</returns>
    public static bool operator ==(BigRational a, BigRational b)
    {
      return a.Equals(b);
    }
    /// <summary>
    /// Returns a value that indicates whether two <see cref="BigRational"/> numbers have different values.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left and right are not equal; otherwise, false.</returns>
    public static bool operator !=(BigRational a, BigRational b)
    {
      return !a.Equals(b);
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value is
    /// less than or equal to another <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is less than or equal to right; otherwise, false.</returns>
    public static bool operator <=(BigRational a, BigRational b)
    {
      return a.CompareTo(b) <= 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value is
    /// greater than or equal to another <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is greater than or equal to right; otherwise, false.</returns>
    public static bool operator >=(BigRational a, BigRational b)
    {
      return a.CompareTo(b) >= 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value is
    /// less than another <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is less than right; otherwise, false.</returns>
    public static bool operator <(BigRational a, BigRational b)
    {
      return a.CompareTo(b) < 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value is
    /// greater than another <see cref="BigRational"/> value.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is greater than right; otherwise, false.</returns>
    public static bool operator >(BigRational a, BigRational b)
    {
      return a.CompareTo(b) > 0;
    }

    #region integer operator
    /// <summary>
    /// Adds the values of a specified <see cref="BigRational"/> and a <see cref="long"/> number.
    /// </summary>
    /// <remarks>
    /// Fast addition of common cases like: <c>x + 1</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to add.</param>
    /// <param name="b">The second value to add.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static BigRational operator +(BigRational a, long b)
    {
      var cpu = main_cpu; cpu.push(b); cpu.add(a); return cpu.popr();
    }
    /// <summary>
    /// Subtracts the values of a specified <see cref="BigRational"/> from another <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// Fast subtraction of common cases like: <c>x - 1</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The value to subtract from (the minuend).</param>
    /// <param name="b">The value to subtract (the subtrahend).</param>
    /// <returns>The result of subtracting b from a.</returns>
    public static BigRational operator -(BigRational a, long b)
    {
      return a + -b; // var cpu = main_cpu; cpu.push(-b); cpu.add(a); return cpu.pop_rat();
    }
    /// <summary>
    /// Multiplies the values of a specified <see cref="BigRational"/> and a <see cref="long"/> number.
    /// </summary>
    /// <remarks>
    /// Fast multiplication of common cases like: <c>x * 2</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to multiply.</param>
    /// <param name="b">The second value to multiply.</param>
    /// <returns>The product of left and right.</returns>
    public static BigRational operator *(BigRational a, long b)
    {
      var cpu = main_cpu; cpu.push(b); cpu.mul(a); return cpu.popr();
    }
    /// <summary>
    /// Divides the values of a specified <see cref="BigRational"/> and a <see cref="long"/> number.
    /// </summary>
    /// <remarks>
    /// Fast divison of common cases like: <c>x / 2</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (devisor)</param>
    /// <returns>The result of the division. NaN when divided by zero.</returns>
    public static BigRational operator /(BigRational a, long b)
    {
      if (b == 0) return double.NaN; //NET 7 req. //throw new DivideByZeroException(nameof(b));
      var cpu = main_cpu; cpu.push(b); cpu.div(a, 0); cpu.swp(); cpu.pop(); return cpu.popr();
    }
    /// <summary>
    /// Adds the values of a specified <see cref="long"/> and a <see cref="BigRational"/> number.
    /// </summary>
    /// <remarks>
    /// Fast addition of common cases like: <c>1 + x</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to add.</param>
    /// <param name="b">The second value to add.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static BigRational operator +(long a, BigRational b)
    {
      return b + a; // var cpu = main_cpu; cpu.push(a); cpu.add(b); return cpu.pop_rat();
    }
    /// <summary>
    /// Subtracts the values of a specified <see cref="long"/> from another <see cref="BigRational"/> value.
    /// </summary>
    /// <remarks>
    /// Fast subtraction of common cases like: <c>1 - x</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The value to subtract from (the minuend).</param>
    /// <param name="b">The value to subtract (the subtrahend).</param>
    /// <returns>The result of subtracting b from a.</returns>
    public static BigRational operator -(long a, BigRational b)
    {
      var cpu = main_cpu; cpu.push(a); cpu.push(b); cpu.sub(); return cpu.popr();
    }
    /// <summary>
    /// Multiplies the values of a specified <see cref="long"/> and a <see cref="BigRational"/> number.
    /// </summary>
    /// <remarks>
    /// Fast multiplication of common cases like: <c>2 * x</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to multiply.</param>
    /// <param name="b">The second value to multiply.</param>
    /// <returns>The product of left and right.</returns>
    public static BigRational operator *(long a, BigRational b)
    {
      return b * a; //var cpu = main_cpu; cpu.push(a); cpu.mul(b); return cpu.pop_rat();
    }
    /// <summary>
    /// Divides the values of a specified <see cref="long"/> and a <see cref="BigRational"/> number.
    /// </summary>
    /// <remarks>
    /// Fast divison of common cases like: <c>2 / x</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (devisor)</param>
    /// <returns>The result of the division. NaN when divided by zero.</returns>
    public static BigRational operator /(long a, BigRational b)
    {
      if (b.p == null) return double.NaN; //NET 7 req. //throw new DivideByZeroException(nameof(b));
      var cpu = main_cpu; cpu.push(a); cpu.push(b); cpu.div(); return cpu.popr();
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value and
    /// an <see cref="long"/> value are equal.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x == 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if the left and right parameters have the same value; otherwise, false.</returns>
    public static bool operator ==(BigRational a, long b)
    {
      return a.Equals(b);
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value and
    /// an <see cref="long"/> value are not equal.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x != 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left and right are not equal; otherwise, false.</returns>
    public static bool operator !=(BigRational a, long b)
    {
      return !a.Equals(b);
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value is
    /// less than or equal to an <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x &lt;= 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is less than or equal to right; otherwise, false.</returns>
    public static bool operator <=(BigRational a, long b)
    {
      return a.CompareTo(b) <= 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value is
    /// greater than or equal to an <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x &gt;= 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is greater than or equal to right; otherwise, false.</returns>
    public static bool operator >=(BigRational a, long b)
    {
      return a.CompareTo(b) >= 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value is
    /// less than an <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x &lt; 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is less than right; otherwise, false.</returns>
    public static bool operator <(BigRational a, long b)
    {
      return a.CompareTo(b) < 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="BigRational"/> value is
    /// greater than an <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x &gt; 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is greater than right; otherwise, false.</returns>
    public static bool operator >(BigRational a, long b)
    {
      return a.CompareTo(b) > 0;
    }
    #endregion

    /// <summary>
    /// Represents a virtual stack machine for rational and arbitrary arithmetic.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: In difference to a <see cref="SafeCPU"/> a <see cref="CPU"/> instance can also be used in:<br/> 
    /// in asynchronous methods, asynchronous lambda expressions, query expressions, iterator blocks and inside non-static nested functions.<br/>
    /// </remarks>
    [DebuggerTypeProxy(typeof(DebugView)), DebuggerDisplay("Count = {i}")]
    public sealed class CPU
    {
      internal int i; internal uint[][] p;
      /// <summary>
      /// Initializes a new instance of a <see cref="CPU"/> class that
      /// has the specified initial stack capacity.
      /// </summary>
      /// <param name="capacity">The initial stack capacity that must be greater than 0 (zero).</param>
      public CPU(uint capacity = 32)
      {
        p = new uint[capacity][];
      }
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
      public uint mark()
      {
        return unchecked((uint)i);
      }
      /// <summary>
      /// Pushes a 0 (zero) value onto the stack.
      /// </summary>
      public void push()
      {
        fixed (uint* p = rent(4))
        {
          *(ulong*)p = 1; *(ulong*)(p + 2) = 0x100000001;
        }
      }
      /// <summary>
      /// Pushes the supplied <see cref="BigRational"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(BigRational v)
      {
        if (v.p == null) { push(); return; }
        uint n; fixed (uint* a = v.p, b = rent(n = len(a))) copy(b, a, n);
      }
      /// <summary>
      /// Pushes the supplied <see cref="int"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(int v)
      {
        //todo: benchmarks
        push(unchecked((uint)(v < 0 ? -v : v))); if (v < 0) this.p[this.i - 1][0] |= 0x80000000; //neg()
        //fixed (uint* p = rent(4))
        //{
        //  p[0] = unchecked((uint)v & 0x80000000) | 1;
        //  p[1] = unchecked((uint)(v < 0 ? -v : v)); *(ulong*)(p + 2) = 0x100000001;
        //}
      }
      /// <summary>
      /// Pushes n copies of the supplied <see cref="int"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      /// <param name="n">The number of copies to push.</param>
      public void push(int v, int n)
      {
        //for (int i = 0; i < n; i++) push(v);
        if (n <= 0) return; push(v); for (int i = 1; i < n; i++) dup(unchecked((uint)(this.i - 1))); //todo: benchmark
      }
      /// <summary>
      /// Pushes the supplied <see cref="uint"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(uint v)
      {
        fixed (uint* p = rent(4)) { p[0] = 1; p[1] = v; *(ulong*)(p + 2) = 0x100000001; }
      }
      /// <summary>
      /// Pushes the supplied <see cref="long"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(long v)
      {
        //todo: benchmarks recu inline
        push(unchecked((ulong)(v < 0 ? -v : v))); if (v < 0) this.p[this.i - 1][0] |= 0x80000000; //neg()
        //var t = unchecked((uint)v); if (t == v) { push(t); return; } // mcc
        //var s = v < 0; var u = unchecked((ulong)(s ? -v : v)); var n = unchecked((uint)u) == u ? 1u : 2u;
        //fixed (uint* p = rent(5))
        //{
        //  p[0] = (s ? 0x80000000 : 0) | n;
        //  *(ulong*)(p + 1) = u; *(ulong*)(p + (n + 1)) = 0x100000001;
        //}
      }
      /// <summary>
      /// Pushes the supplied <see cref="ulong"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(ulong v)
      {
        var u = unchecked((uint)v); if (u == v) { push(u); return; }
        fixed (uint* p = rent(5)) { p[0] = 2; *(ulong*)(p + 1) = v; *(ulong*)(p + 3) = 0x100000001; }
      }
      /// <summary>
      /// Pushes the supplied <see cref="float"/> value onto the stack.
      /// </summary>
      /// <remarks>
      /// Rounds the value implicit to 7 digits as the <see cref="float"/> format has
      /// only 7 digits of precision.<br/> 
      /// see: <see cref="BigRational(float)"/>
      /// </remarks>
      /// <param name="v">The value to push.</param>
      public void push(float v)
      {
        if (v == 0) { push(); return; }
        var e = unchecked((byte)(*(uint*)&v >> 23) - 126); //Debug.Assert(!float.IsFinite(v) == (e == 0x81));
        if (e == 0x81) { pnan(); return; }
        var p = 6 - ((e * 19728) >> 16);
        var d = MathF.Abs(v) * Math.Pow(10, p); //-32..44
        if (d < 1e6) { d *= 10; p++; }
        var m = (uint)Math.Round(d);
        push(m); pow(10, p); div(); if (v < 0) neg();
      }
      /// <summary>
      /// Pushes the supplied <see cref="double"/> value onto the stack.
      /// </summary>
      /// <remarks>
      /// Rounds the value implicit to 15 digits as the <see cref="double"/> format has
      /// only 15 digits of precision.<br/> 
      /// see: <see cref="BigRational.BigRational(double)"/>
      /// </remarks>
      /// <param name="v">The value to push.</param>
      public void push(double v)
      {
        if (v == 0) { push(); return; }
        var e = unchecked((int)((*(ulong*)&v >> 52) & 0x7FF) - 1022); //Debug.Assert(!double.IsFinite(v) == (e == 0x401));
        if (e == 0x401) { pnan(); return; } // NaN 
        int p = 14 - ((e * 19728) >> 16), t = p; if (p > 308) p = 308; // v < 1E-294
        var d = Math.Abs(v) * Math.Pow(10, p); // Debug.Assert(double.IsNormal(d));
        if (t != p) { d *= Math.Pow(10, t = t - p); p += t; if (d < 1e14) { d *= 10; p++; } }
        var m = (ulong)Math.Round(d);
        push(m); pow(10, p); div(); if (v < 0) neg(); // norm();        
      }
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
      public void push(double v, bool exact)
      {
        if (v == 0) { push(); return; }
        int h = ((int*)&v)[1], e = ((h >> 20) & 0x7FF) - 1075; // Debug.Assert(!double.IsFinite(v) == (e == 0x3cc));
        if (e == 0x3cc) { pnan(); return; } // NaN 
        fixed (uint* p = rent((unchecked((uint)(e < 0 ? -e : e)) >> 5) + 8))
        {
          p[0] = 2; p[1] = *(uint*)&v; p[2] = (unchecked((uint)h) & 0x000FFFFF) | 0x100000;
          if (e == -1075) { if ((p[2] &= 0xfffff) == 0) if (p[p[0] = 1] == 0) { *(ulong*)(p + 2) = 0x100000001; return; } e++; } // denormalized
          if (e > 0) shl(p, e); var q = p + (p[0] + 1);
          *(ulong*)q = 0x100000001; if (e < 0) shl(q, -e);
          p[0] |= (unchecked((uint)h) & 0x80000000) | 0x40000000;
        }
      }
      /// <summary>
      /// Pushes the supplied <see cref="decimal"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(decimal v)
      {
        var b = (uint*)&v; uint f = b[0], e = (f >> 16) & 0xff; //0..28 
        fixed (uint* p = rent(9))
        {
          *(ulong*)&p[1] = *(ulong*)&b[2]; p[3] = b[1]; p[0] = p[3] != 0 ? 3u : p[2] != 0 ? 2u : 1;
          if (*(ulong*)p == 1) { *(ulong*)(p + 2) = 0x100000001; return; }
          var s = p + (p[0] + 1);
          if (e == 0) { *(ulong*)s = 0x100000001; }
          else
          {
            var t = (ulong*)(s + 1); var h = e < 19 ? e : 19;
            t[0] = (ulong)Math.Pow(10, h);
            t[1] = h == e ? 0 : Math.BigMul(t[0], (ulong)Math.Pow(10, e - h), out t[0]);
            s[0] = s[3] != 0 ? 3u : s[2] != 0 ? 2u : 1; p[0] |= 0x40000000;
          }
          p[0] |= f & 0x80000000;
        }
      }
      /// <summary>
      /// Pushes the supplied <see cref="BigInteger"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(BigInteger v)
      {
        var si = v.Sign; if (si == 0) { push(); return; }
        if (si < 0) v = -v; int n = v.GetByteCount(true), c = ((n - 1) >> 2) + 1;
        fixed (uint* p = rent(unchecked((uint)(c + 3))))
        {
          p[c] = 0; var ok = v.TryWriteBytes(new Span<byte>(p + 1, n), out _, true);
          if (p[c] == 0) c--; Debug.Assert(ok && p[c] != 0); //todo: report BigInteger bug p[c] == 0
          p[0] = unchecked((uint)c) | (si < 0 ? 0x80000000 : 0);
          *(ulong*)(p + c + 1) = 0x100000001;
        }
      }
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
      public void push(ReadOnlySpan<uint> v)
      {
        fixed (uint* a = v)
        {
          var n = (uint)v.Length;
          fixed (uint* b = rent(n)) copy(b, a, n);
        }
      }
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
      public void get(uint i, out BigRational v)
      {
        fixed (uint* p = this.p[i])
        {
          if (isz(p)) { v = default; return; }
          if ((p[0] & 0x40000000) != 0) norm(p);
          var n = len(p);
          if (n == 4 && p[1] == p[3])
          {
            Debug.Assert(p[1] <= 1); // 11, 00
            v = cachx(p[3] + (p[0] >> 31), p, n); return; //NaN, 1, -1            
          }
          var a = new uint[n]; fixed (uint* s = a) copy(s, p, n);
          v = new BigRational(a);
        }
      }
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
      public void get(uint i, out double v)
      {
        v = (double)new BigRational(p[i]);
      }
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
      public void get(uint i, out float v)
      {
        v = (float)new BigRational(p[i]);
      }
      /// <summary>
      /// Converts the numerator of the value at absolute position <paramref name="i"/> on stack to a 
      /// <see cref="BigInteger"/> number and returns it.
      /// </summary>
      /// <remarks>
      /// In the case of fractional values, it is advisable to round them beforehand using the desired rounding mode.
      /// </remarks>
      /// <param name="i">Absolute index of the value to get.</param>
      /// <param name="v">Returns the value.</param>
      public void get(uint i, out BigInteger v)
      {
        fixed (uint* p = this.p[i])
        {
          v = new BigInteger(new ReadOnlySpan<byte>((byte*)(p + 1), unchecked((int)(p[0] & 0x3fffffff) << 2)), true, false);
          if (sig(p) < 0) v = -v; //fast BigInteger
        }
      }
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
      public void get(uint i, out ReadOnlySpan<uint> v)
      {
        var p = this.p[i];
        fixed (uint* u = p) v = new ReadOnlySpan<uint>(p).Slice(0, unchecked((int)len(u)));
      }

      /// <summary>
      /// Removes the value currently on top of the stack, 
      /// convert and returns it as always normalized <see cref="BigRational"/> number.<br/>
      /// </summary>
      /// <returns>A always normalized <see cref="BigRational"/> number.</returns>
      public BigRational popr()
      {
        if (sp != null) { long d = (long)sp - (long)&d; if (d < 1024) return new BigRational(p[i - 1]); }
        get(unchecked((uint)(i - 1)), out BigRational r); pop(); return r;
      }
      /// <summary>
      /// Removes the value currently on top of the stack, 
      /// convert and returns it as <see cref="double"/> value.<br/>
      /// </summary>
      /// <remarks>
      /// <see cref="double.PositiveInfinity"/> or <see cref="double.NegativeInfinity"/> 
      /// is returned in case of out of range as the <see cref="double"/> value range is limited. 
      /// </remarks>
      /// <returns>A <see cref="double"/> value.</returns>
      public double popd()
      {
        get(unchecked((uint)(i - 1)), out double t); pop(); return t;
      }
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
      public int popi()
      {
        fixed (uint* u = p[i - 1])
        {
          var i = unchecked((int)u[1]);
          if ((u[0] & 0x80000000) != 0) i = -i;
          pop(); return i;
        }
      }
      /// <summary>
      /// Removes the value currently on top of the stack.
      /// </summary>
      public void pop()
      {
        i -= 1;
      }
      /// <summary>
      /// Removes n values currently on top of the stack.
      /// </summary>
      /// <param name="n">Number of values to pop.</param>
      public void pop(int n)
      {
        i -= n;
      }
      /// <summary>
      /// Swaps the first two values on top of the stack.
      /// </summary>
      public void swp()
      {
        var t = p[i - 1]; p[i - 1] = p[i - 2]; p[i - 2] = t;
      }
      /// <summary>
      /// Swaps the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void swp(int a = 1, int b = 0)
      {
        var t = p[a = this.i - 1 - a]; p[a] = p[b = this.i - 1 - b]; p[b] = t;
        //swp(unchecked((uint)(this.i - 1 - i)), unchecked((uint)(this.i - 1 - k)));
      }
      /// <summary>
      /// Swaps the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void swp(uint a, uint b)
      {
        var t = p[a]; p[a] = p[b]; p[b] = t;
      }
      /// <summary>
      /// Swaps the values at index <paramref name="a"/> as absolute index with the value on top of the stack.
      /// </summary>
      /// <param name="a">Absolute index a stack entry.</param>
      public void swp(uint a)
      {
        var t = p[a]; p[a] = p[i - 1]; p[i - 1] = t;
      }
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
      public void dup(int a = 0)
      {
        dup(unchecked((uint)(this.i - 1 - a)));
      }
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
      public void dup(uint a)
      {
        uint n; fixed (uint* u = this.p[a], v = rent(n = len(u))) copy(v, u, n);
        //fixed (uint* u = this.p[a]) { var n = len(u); fixed (uint* v = rent(n)) copy(v, u, n); }
      }
      /// <summary>
      /// Negates the value at index <paramref name="a"/> relative to the top of the stack.<br/>
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      public void neg(int a = 0)
      {
        neg(unchecked((uint)(this.i - 1 - a))); // fixed (uint* p = this.p[this.i - 1 - i]) if (!isz(p)) p[0] ^= 0x80000000;
      }
      /// <summary>
      /// Negates the value at index <paramref name="a"/> as absolute index in the stack.<br/>
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void neg(uint a)
      {
        fixed (uint* p = this.p[a]) if (!isz(p)) p[0] ^= 0x80000000;
      }
      /// <summary>
      /// Convert the value at index <paramref name="a"/> relative to the top of the stack to it's absolute value.<br/>
      /// Default index 0 addresses the value on top of the stack.
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      public void abs(int a = 0)
      {
        fixed (uint* p = this.p[this.i - 1 - a]) p[0] &= ~0x80000000;
      }
      /// <summary>
      /// Adds the first two values on top of the stack 
      /// and replaces them with the result.
      /// </summary>
      public void add()
      {
        fixed (uint* u = p[i - 2], v = p[i - 1]) add(u, v, false); swp(0, 2); pop(2);
      }
      /// <summary>
      /// Adds the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack
      /// and replaces the value at index a with the result.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void add(int a, int b)
      {
        fixed (uint* u = p[this.i - 1 - a])
        fixed (uint* v = p[this.i - 1 - b]) add(u, v, false);
        swp(a + 1); pop();
      }
      /// <summary>
      /// Adds the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack
      /// and pushes the result on top of the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void add(uint a, uint b)
      {
        fixed (uint* u = p[a])
        fixed (uint* v = p[b]) add(u, v, false);
      }
      /// <summary>
      /// Adds the value at index a as absolute index in the stack
      /// and the value on top of the stack.<br/>
      /// Replaces the value on top of the stack with the result.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void add(uint a)
      {
        fixed (uint* u = p[i - 1], v = p[a]) add(u, v, false);
        swp(); pop();
      }
      /// <summary>
      /// Adds value <paramref name="a"/> and the value on top of the stack.<br/>
      /// Replaces the value on top of the stack with the result.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> value.</param>
      public void add(BigRational a)
      {
        if (a.p == null) return;
        fixed (uint* u = p[i - 1], v = a.p) add(u, v, false);
        swp(); pop();
      }
      /// <summary>
      /// Adds the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      public void add(BigRational a, BigRational b)
      {
        if (a.p == null) { push(b); return; }
        if (b.p == null) { push(a); return; }
        fixed (uint* u = a.p, v = b.p) add(u, v, false);
      }
      /// <summary>
      /// Subtract the first two values on top of the stack 
      /// and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// a - b where b is the value on top of the stack. 
      /// </remarks>
      public void sub()
      {
        fixed (uint* u = p[i - 2], v = p[i - 1]) add(u, v, true); swp(0, 2); pop(2);
      }
      /// <summary>
      /// Subtracts the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack
      /// and replaces the value at index a with the result.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void sub(int a, int b)
      {
        fixed (uint* u = p[this.i - 1 - a])
        fixed (uint* v = p[this.i - 1 - b]) add(u, v, true);
        swp(a + 1); pop();
      }
      /// <summary>
      /// Subtracts the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack
      /// and pushes the result on top of the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void sub(uint a, uint b)
      {
        fixed (uint* u = p[a])
        fixed (uint* v = p[b]) add(u, v, true);
      }
      /// <summary>
      /// Subtracts the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      public void sub(BigRational a, BigRational b)
      {
        if (a.p == null) { push(b); neg(); return; }
        if (b.p == null) { push(a); return; }
        fixed (uint* u = a.p, v = b.p) add(u, v, true);
      }
      /// <summary>
      /// Multiplies the first two values on top of the stack 
      /// and replaces them with the result.
      /// </summary>
      public void mul()
      {
        fixed (uint* u = p[i - 2], v = p[i - 1]) mul(u, v, false);
        swp(0, 2); pop(2);
      }
      /// <summary>
      /// Multiplies the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack
      /// and replaces the value at index a with the result.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void mul(int a, int b)
      {
        fixed (uint* u = p[this.i - 1 - a])
        fixed (uint* v = p[this.i - 1 - b]) mul(u, v, false);
        swp(a + 1); pop();
      }
      /// <summary>
      /// Multiplies the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack
      /// and pushes the result on top of the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void mul(uint a, uint b)
      {
        fixed (uint* u = p[a])
        fixed (uint* v = p[b]) mul(u, v, false);
      }
      /// <summary>
      /// Multiplies the value at index a as absolute index in the stack
      /// and the value on top of the stack.<br/>
      /// Replaces the value on top of the stack with the result.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void mul(uint a)
      {
        fixed (uint* u = p[i - 1], v = p[a]) mul(u, v, false);
        swp(); pop();
      }
      /// <summary>
      /// Multiplies value <paramref name="a"/> and the value on top of the stack.<br/>
      /// Replaces the value on top of the stack with the result.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> value.</param>
      public void mul(BigRational a)
      {
        if (a.p == null) push();
        else fixed (uint* u = p[i - 1], v = a.p) mul(u, v, false);
        swp(); pop();
      }
      /// <summary>
      /// Multiplies the value <paramref name="a"/> and the value at b as absolute index in the stack
      /// and pushes the result on the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">Absolute index of a stack entry as second value.</param>
      public void mul(BigRational a, uint b)
      {
        if (a.p == null) { push(); return; }
        fixed (uint* u = a.p, v = p[b]) mul(u, v, false);
      }
      /// <summary>
      /// Multiplies the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      public void mul(BigRational a, BigRational b)
      {
        if (a.p == null) { push(); return; }
        if (b.p == null) { push(); return; }
        if (a.p == b.p) { push(a); sqr(); return; }
        fixed (uint* u = a.p, v = b.p) mul(u, v, false);
      }
      /// <summary>
      /// Divides the first two values on top of the stack 
      /// and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// Divides a / b where b is the value on top of the stack.
      /// </remarks>
      public void div()
      {
        fixed (uint* u = p[i - 2], v = p[i - 1]) mul(u, v, true); swp(0, 2); pop(2);
      }
      /// <summary>
      /// Divides the values at index <paramref name="a"/> and <paramref name="b"/> relative to the top of the stack
      /// and replaces the value at index a with the result.
      /// </summary>
      /// <param name="a">Relative index of the first stack entry.</param>
      /// <param name="b">Relative index of the second stack entry.</param>
      public void div(int a, int b)
      {
        fixed (uint* u = p[this.i - 1 - a])
        fixed (uint* v = p[this.i - 1 - b]) mul(u, v, true);
        swp(a + 1); pop();
      }
      /// <summary>
      /// Divides the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack
      /// and pushes the result on top of the stack.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      public void div(uint a, uint b)
      {
        fixed (uint* u = p[a], v = p[b]) mul(u, v, true);
      }
      /// <summary>
      /// Divides the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      public void div(BigRational a, BigRational b)
      {
        if (b.p == null) { pnan(); return; }
        if (a.p == null) { push(); return; }
        fixed (uint* u = a.p, v = b.p) mul(u, v, true);
      }
      /// <summary>
      /// Divides the value <paramref name="a"/> and the value at index <paramref name="b"/> relative to the top of the stack.<br/>
      /// Pushes the result on top of the stack.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> as first value.</param>
      /// <param name="b">A relative index of a stack entry.</param>
      public void div(BigRational a, int b)
      {
        if (a.p == null) { push(); return; }
        fixed (uint* u = a.p, v = p[this.i - 1 - b]) mul(u, v, true);
      }
      /// <summary>
      /// Performs an integer division of the numerators of the first two values on top of the stack<br/> 
      /// and replaces them with the integral result.
      /// </summary>
      /// <remarks>
      /// Divides a / b where b is the value on top of the stack.<br/>
      /// This is a integer division with always non-farctional integer results.<br/>
      /// When calculating with integers and integer results are required,<br/>
      /// this operation is faster than dividing and then rounding to integer result.
      /// </remarks>
      public void idiv()
      {
        //uint nu = u[0] & 0x3fffffff, nv = v[0] & 0x3fffffff; if (nu < nv || (nu == nv && BitOperations.LeadingZeroCount(u[nu]) < BitOperations.LeadingZeroCount(v[nv]))) { pop(2); push(); return; }
        fixed (uint* u = p[this.i - 1])
        fixed (uint* v = p[this.i - 2])
        fixed (uint* w = rent(len(u))) // nu - nv + 1
        {
          var h = v[0]; v[0] &= 0x3fffffff; div(v, u, w);
          *(ulong*)(w + w[0] + 1) = 0x100000001;
          if (((h ^ u[0]) & 0x80000000) != 0 && *(ulong*)w != 1) w[0] |= 0x80000000;
        }
        swp(2); pop(2);
      }
      /// <summary>
      /// Performs an integer division of the numerators of the first two values on top of the stack<br/> 
      /// and replaces them with the integral result.
      /// </summary>
      /// <remarks>
      /// Divides a / b where b is the value on top of the stack.<br/>
      /// This is a integer division with always non-farctional integer results.<br/>
      /// When calculating with integers and integer results are required,<br/>
      /// this operation is faster than dividing and then rounding to integer result.<br/>
      /// </remarks>
      public void imod()
      {
        fixed (uint* u = p[this.i - 1])
        fixed (uint* v = p[this.i - 2])
        {
          var h = v[0]; v[0] &= 0x3fffffff; div(v, u, null);
          *(ulong*)(v + v[0] + 1) = 0x100000001;
          v[0] |= h & 0x80000000;
        }
        pop();
      }
      /// <summary>
      /// Squares the value at index <paramref name="a"/> relative to the top of the stack.<br/>
      /// Default index 0 squares the value on top of the stack.
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      public void sqr(int a = 0)
      {
        sqr(unchecked((uint)(this.i - 1 - a)));
        swp(a + 1); pop();
      }
      /// <summary>
      /// Squares the value at index <paramref name="a"/> as absolute index in the stack.<br/>
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void sqr(uint a)
      {
        fixed (uint* u = this.p[a])
        fixed (uint* w = rent(len(u) << 1))
        {
          sqr(u, w); if (*(ulong*)w == 1) { *(ulong*)(w + 2) = 0x100000001; return; }
          sqr(u + ((u[0] & 0x3fffffff) + 1), w + (w[0] + 1)); w[0] |= 0x40000000;
        }
      }
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
      public void inv(int i = 0)
      {
        fixed (uint* p = this.p[this.i - 1 - i])
        {
          uint n = (p[0] & 0x3fffffff) + 1, m = p[n] + 1, t, a, b;
          p[n] |= p[0] & (0x80000000 | 0x40000000); p[0] &= 0x3fffffff;
          if (n == m)
          {
            for (a = 0; a < n; a++) { t = p[a]; p[a] = p[b = a + n]; p[b] = t; }
          }
          else
          {
            uint c = n - 1, d = m + c;
            for (a = 0, b = c; a < b; a++, b--) { t = p[a]; p[a] = p[b]; p[b] = t; }
            for (a = n, b = d; a < b; a++, b--) { t = p[a]; p[a] = p[b]; p[b] = t; }
            for (a = 0, b = d; a < b; a++, b--) { t = p[a]; p[a] = p[b]; p[b] = t; }
          }
        }
      }
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
      public void shl(uint c, int i = 0)
      {
        if (c == 0) return;
        fixed (uint* u = p[this.i - 1 - i])
        {
          if (*(ulong*)u == 1) return;
          fixed (uint* v = rent(len(u) + (c >> 5) + 1))
          {
            var n = (u[0] & 0x3fffffff) + 1;
            copy(v, u, n); shl(v, unchecked((int)c));
            copy(v + v[0] + 1, u + n, u[n] + 1);
            v[0] |= (u[0] & 0x80000000) | 0x40000000; swp(i + 1); pop();
          }
        }
      }
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
      public void shr(uint c, int i = 0)
      {
        if (c == 0) return;
        fixed (uint* p = this.p[this.i - 1 - i])
        {
          var h = p[0]; var a = h & 0x3fffffff;
          shr(p, unchecked((int)c)); if (*(ulong*)p == 1) { *(ulong*)(p + 2) = 0x100000001; return; }
          if (p[0] != a) copy(p + p[0] + 1, p + a + 1, p[a + 1] + 1);
          p[0] |= (h & 0x80000000) | 0x40000000;
        }
      }
      /// <summary>
      /// Bitwise logical AND of the numerators<br/>
      /// of the first two values on top of the stack and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// Manipulating denominators is possible by calling <see cref="inv"/> beforehand.<br/>
      /// Inversion of rational 0 values (0 / 1 => 1 / 0) results in NaN values which can be ignored for all bit-level operations.
      /// </remarks>
      public void and()
      {
        fixed (uint* u = this.p[this.i - 1])
        fixed (uint* v = this.p[this.i - 2])
        {
          uint nu = u[0] & 0x3fffffff, nv = v[0] & 0x3fffffff, n = nu < nv ? nu : nv, l = 1;
          for (uint i = 1; i <= n; i++) if ((v[i] &= u[i]) != 0) l = i;
          if (l != nv) { copy(v + l + 1, v + nv + 1, v[nv + 1] + 1); v[0] = (v[0] & 0xc0000000) | l; }
          pop();
        }
      }
      /// <summary>
      /// Bitwise logical OR of the numerators<br/>
      /// of the first two values on top of the stack and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// Manipulating denominators is possible by calling <see cref="inv"/> beforehand.<br/>
      /// Inversion of rational 0 values (0 / 1 => 1 / 0) results in NaN values which can be ignored for all bit-level operations.
      /// </remarks>
      public void or()
      {
        fixed (uint* u = this.p[this.i - 1])
        fixed (uint* v = this.p[this.i - 2])
        {
          uint nu = u[0] & 0x3fffffff, nv = v[0] & 0x3fffffff;
          if (nv < nu) { swp(); or(); return; }
          for (uint i = 1; i <= nu; i++) v[i] |= u[i]; pop();
        }
      }
      /// <summary>
      /// Bitwise logical XOR of the numerators<br/>
      /// of the first two values on top of the stack and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// Manipulating denominators is possible by calling <see cref="inv"/> beforehand.<br/>
      /// Inversion of rational 0 values (0 / 1 => 1 / 0) results in NaN values which can be ignored for all bit-level operations.
      /// </remarks>
      public void xor()
      {
        fixed (uint* u = this.p[this.i - 1])
        fixed (uint* v = this.p[this.i - 2])
        {
          uint nu = u[0] & 0x3fffffff, nv = v[0] & 0x3fffffff;
          if (nv < nu) { swp(); xor(); return; }
          uint l = 1; for (uint i = 1; i <= nu; i++) if ((v[i] ^= u[i]) != 0) l = i;
          if (nu == nv && l != nv) { copy(v + l + 1, v + nv + 1, v[nv + 1] + 1); v[0] = (v[0] & 0xc0000000) | l; }
          pop();
        }
      }
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
      public void mod(int f = 0)
      {
        fixed (uint* u = p[this.i - 1])
        fixed (uint* v = rent(len(u) + 1))
        {
          var h = u[0]; u[0] &= 0x3fffffff; var t = u + u[0] + 1;
          if (*(ulong*)t == 1) { pop(); pnan(); return; } //keep NaN
          if (f != 8) div(u, t, v); else copy(v, t, t[0] + 1);
          if ((f & (1 | 2 | 4)) != 0)
          {
            if (f == 4) f = *(ulong*)u != 1 ? 1 : 0;
            else { shr(t, 1); var x = cms(u, t); f = f == 1 ? x : x + 1; }
            if (f > 0) { var w = 0x100000001; add(v, (uint*)&w, v); }
          }
          *(ulong*)(u + u[0] + 1) = *(ulong*)(v + v[0] + 1) = 0x100000001;
          if ((h & 0x80000000) != 0 && *(ulong*)v != 1) v[0] |= 0x80000000;
        }
      }
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
      public void rnd(int c, int f = 1)
      {
        if (c >= 0) fixed (uint* p = this.p[this.i - 1]) if (isint(p)) return;
        if (c == 0) { mod(f); swp(); pop(); } //trunc, cail, ... 
        else { pow(10, c); swp(); mul(0, 1); mod(f); swp(); pop(); swp(); div(); }
      }
      /// <summary>
      /// Pushes the specified number x raised to the specified power y to the stack.
      /// </summary>
      /// <param name="x">A <see cref="int"/> value to be raised to a power.</param>
      /// <param name="y">A <see cref="int"/> value that specifies a power.</param>
      public void pow(int x, int y)
      {
        uint e = unchecked((uint)(y < 0 ? -y : y)), z;
        if (x == 10) { push(unchecked((ulong)Math.Pow(x, z = e < 19 ? e : 19))); e -= z; }
        //else if (x == 2) { push(1u); shl(e); e = 0; } //opt. possible but cases?
        else push(1u);
        if (e != 0)
        {
          push(x);
          for (; ; e >>= 1)
          {
            if ((e & 1) != 0) mul(1, 0);
            if (e == 1) break; sqr();// mul(0, 0);
          }
          pop();
        }
        if (y < 0) inv();
      }
      /// <summary>
      /// Replaces the value on top of the stack 
      /// with the power of this number to the specified power y.
      /// </summary>
      /// <param name="y">A <see cref="int"/> value that specifies a power.</param>
      public void pow(int y)
      {
        push(1u); swp();
        for (var e = unchecked((uint)(y < 0 ? -y : y)); ; e >>= 1)
        {
          if ((e & 1) != 0) mul(1, 0);
          if (e <= 1) break; sqr(); // mul(0, 0);
        }
        pop(); if (y < 0) inv();
      }
      /// <summary>
      /// Pushes the factorial of the specified number c on the stack.
      /// </summary>
      /// <param name="c">The <see cref="int"/> value from which the factorial is calculated.</param>
      public void fac(uint c)
      {
        push(1u); for (uint i = 2; i <= c; i++) { push(i); mul(); }
      }
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
      public int bdi()
      {
        fixed (uint* p = this.p[this.i - 1])
        {
          if (isz(p)) return -int.MaxValue; //infinity
          var h = p[0]; var a = h & 0x3fffffff; var q = p + a + 1; var b = q[0];
          var u = (a << 5) - unchecked((uint)BitOperations.LeadingZeroCount(p[a]));
          var v = (b << 5) - unchecked((uint)BitOperations.LeadingZeroCount(q[b]));
          return unchecked((int)u - (int)v);
        }
      }
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
      public void lim(uint c, int i = 0)
      {
        Debug.Assert(c > 0);
        fixed (uint* p = this.p[this.i - 1 - i])
        {
          if (isz(p)) return;
          var h = p[0]; var a = h & 0x3fffffff; var q = p + a + 1; var b = q[0];
          var u = (a << 5) - unchecked((uint)BitOperations.LeadingZeroCount(p[a]));
          var v = (b << 5) - unchecked((uint)BitOperations.LeadingZeroCount(q[b]));
          if (u > v) u = v; if (u <= c) return;
          var t = (int)(u - c); shr(p, t); shr(q, t);
          if (p[0] != a) copy(p + p[0] + 1, q, q[0] + 1);
          p[0] |= (h & 0x80000000) | 0x40000000;
        }
      }
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
      public int sign(int a = 0)
      {
        fixed (uint* u = p[this.i - 1 - a]) return sig(u);
      }
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
      public int cmpa(int a = 0, int b = 1)
      {
        fixed (uint* u = p[this.i - 1 - a])
        fixed (uint* v = p[this.i - 1 - b])
        {
          var x = u[0]; u[0] &= ~0x80000000;
          var y = v[0]; v[0] &= ~0x80000000;
          var c = cmp(u, v); u[0] = x; v[0] = y; return c;
        }
      }
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
      public int cmpi(int a, int b)
      {
        //push(b); a = cmp(a + 1, 0); pop(); return a;
        var v = stackalloc uint[] { unchecked((uint)b & 0x8000000) | 1, unchecked((uint)(b < 0 ? -b : b)), 0, 1 };
        fixed (uint* u = p[this.i - 1 - a]) return cmp(u, v);
      }
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
      public int cmp(int a = 0, int b = 1)
      {
        fixed (uint* u = p[this.i - 1 - a])
        fixed (uint* v = p[this.i - 1 - b]) return cmp(u, v);
      }
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
      public int cmp(uint a, uint b)
      {
        if (a == b) return 0; //sort's
        fixed (uint* u = p[a])
        fixed (uint* v = p[b]) return cmp(u, v);
      }
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
      public int cmp(uint b)
      {
        return cmp((uint)(this.i - 1), b);
      }
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
      public int cmp(BigRational a, BigRational b)
      {
        if (a.p == b.p) return 0; //sort's
        fixed (uint* u = a.p, v = b.p)
        {
          if (u == null) return -sig(v);
          if (v == null) return +sig(u);
          return cmp(u, v);
        }
      }
      /// <summary>
      /// Compares the value on top of the stack a with the <see cref="BigRational"/> value b.
      /// </summary>
      /// <param name="b">A <see cref="BigRational"/> as second value.</param>
      /// <returns>
      /// -1 – the value on top of the stack is less than b.<br/>
      /// 0 – the value on top of the stack is equal b.<br/>
      /// 1 – the value on top of the stack is greater than b.<br/>
      /// </returns>
      public int cmp(BigRational b)
      {
        if (b.p == null) return sign();
        fixed (uint* u = p[this.i - 1], v = b.p) return cmp(u, v);
      }
      /// <summary>
      /// Compares the <see cref="BigRational"/> value <paramref name="a"/> with the <see cref="BigRational"/> value b for equality.
      /// </summary>
      /// <param name="a">A <see cref="BigRational"/> value as first value.</param>
      /// <param name="b">A <see cref="BigRational"/> value as second value.</param>
      /// <returns>true if the values are equal, otherwise false.</returns>
      public bool equ(BigRational a, BigRational b)
      {
        return equ(a.p, b.p);
      }
      /// <summary>
      /// Compares the values at index <paramref name="a"/> and <paramref name="b"/> as absolute indices in the stack for equality.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">Absolute index of a stack entry.</param>
      /// <returns>true if the values are equal, otherwise false.</returns>
      public bool equ(uint a, uint b)
      {
        return equ(p[a], p[b]);
      }
      /// <summary>
      /// Compares the value at index a as absolute index in the stack with the <see cref="BigRational"/> value b for equality.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">A <see cref="BigRational"/> value as second value.</param>
      /// <returns>true if the values are equal, otherwise false.</returns>
      public bool equ(uint a, BigRational b)
      {
        return equ(p[a], b.p);
      }
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
      public void norm(int i = 0)
      {
        fixed (uint* u = p[this.i - 1 - i])
          if ((u[0] & 0x40000000) != 0) norm(u);
      }
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
      public uint hash(uint i)
      {
        var p = this.p[i]; if ((p[0] & 0x40000000) != 0) fixed (uint* u = p) norm(u);
        return unchecked((uint)new BigRational(p).GetHashCode());
      }
      /// <summary>
      /// Returns the MSB (most significant bit) of the numerator of the value on top of the stack.
      /// </summary>
      /// <remarks>
      /// Uses <i>MSB 1 bit numbering</i>, so 0 gives 0, 1 gives 1, 4 gives 2.<br/>
      /// To get the MSB of the denominator is possible by calling <see cref="inv"/> before and after.
      /// </remarks>
      /// <returns>A <see cref="uint"/> value as MSB.</returns>
      public uint msb()
      {
        fixed (uint* p = this.p[this.i - 1])
        {
          var n = p[0] & 0x3fffffff;
          return (n << 5) - unchecked((uint)BitOperations.LeadingZeroCount(p[n]));
        }
      }
      /// <summary>
      /// Returns the LSB (least significant bit) of the numerator of the value on top of the stack.
      /// </summary>
      /// <remarks>
      /// Uses <i>LSB 1 bit numbering</i>, so 0 gives 0, 1 gives 1, 4 gives 2.<br/>
      /// To get the LSB of the denominator is possible by calling <see cref="inv"/> before and after.
      /// </remarks>
      /// <returns>A <see cref="uint"/> value as MSB.</returns>
      public uint lsb()
      {
        fixed (uint* p = this.p[this.i - 1])
        {
          for (uint t = 1, n = p[0] & 0x3fffffff; t <= n; t++)
            if (p[t] != 0)
              return ((t - 1) << 5) + (uint)BitOperations.TrailingZeroCount(p[t]) + 1;
          return 0;
        }
      }
      /// <summary>
      /// Returns whether the value on top of the stack is an integer.
      /// </summary>
      /// <returns>true if the value is integer; false otherwise.</returns>
      public bool isi()
      {
        fixed (uint* p = this.p[this.i - 1])
        {
          if (isint(p)) return true;
          if ((p[0] & 0x40000000) == 0) return false; //if ((p[0] & 0x40000000) != 0) norm(p);
        }
        dup(); mod(); if (sign(1) != 0) { pop(2); return false; }
        swp(0, 2); pop(2); return true; //keep normalized                   
      }
      /// <summary>
      /// Finds the greatest common divisor (GCD) of the numerators<br/>
      /// of the first two values on top of the stack and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// This operation makes only sense for integer values since the denominators are ignored.
      /// </remarks>
      public void gcd()
      {
        fixed (uint* u = this.p[this.i - 2])
        fixed (uint* v = this.p[this.i - 1])
        {
          u[0] &= 0x3fffffff; v[0] &= 0x3fffffff;
          var r = isz(u) ? v : isz(v) ? u : gcd(u, v);
          *(ulong*)(r + (r[0] + 1)) = 0x100000001;
          if (r == v) swp(); pop();
        }
      }
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
      public void tos(Span<char> sp, out int ns, out int exp, out int rep, bool reps)
      {
        var p = this.p[this.i - 1]; exp = ns = 0; rep = -1;
        var i1 = p[0] & 0x3fffffff; //if (i1 == 0 || i1 + 3 > p.Length) { pop(); return; } // NaN
        var i2 = p[i1 + 1]; //if (i2 == 0 || i1 + i2 + 2 > p.Length) { pop(); return; } // NaN
        var c1 = ((long)i1 << 5) - BitOperations.LeadingZeroCount(p[i1]);
        var c2 = ((long)i2 << 5) - BitOperations.LeadingZeroCount(p[i1 + 1 + i2]);
        if (c2 == 0) { pop(); return; } // NaN
        if (c1 == 0) { if (sp.Length != 0) { sp[0] = '0'; ns = 1; } pop(); return; }
        if (c2 == 1) reps = false;
        c1 = (c1 * 1292913986) >> 32; // est Math.Log(2) / Math.Log(10)  
        c2 = (c2 * 1292913986) >> 32;
        if ((exp = unchecked((int)(c1 - c2))) != 0) { pow(10, exp); div(); }
        for (; ; )
        {
          dup(); mod(); var d = this.p[this.i - 1][1]; if (d != 0 && d < 10) break;
          pop(2); push(10u); if (d == 0) { mul(); exp--; } else { div(); exp++; }
        }
        int nr = 0; uint* rr = null; IntPtr mem = default;
        if (reps)
        {
          nr = unchecked((int)(this.p[this.i - 3][0] & 0x3fffffff)) + 1;
          var l = (long)sp.Length * nr; var need = unchecked((int)l);
          if (l >= (int.MaxValue >> 3)) reps = false;
          else if (l <= 0x8000) { var t = stackalloc uint[need]; rr = t; }
          else rr = (uint*)(mem = Marshal.AllocCoTaskMem(need << 2));
        }
        for (int i = 0, t; ; i++)
        {
          if (i == sp.Length) { pop(3); ns = i; break; }
          var c = unchecked((char)('0' + this.p[this.i - 1][1]));
          if (reps && c != '0')
          {
            fixed (uint* tt = this.p[this.i - 2])
            {
              tt[0] &= 0x3fffffff; Debug.Assert(tt[0] < nr);
              for (t = 0; t < i && !(sp[t] == c && cms(tt, rr + t * nr) == 0); t++) ;
              if (t < i)
              {
                for (; t != 0 && sp[i - 1] == '0' && sp[t - 1] == '0'; i--, t--) ;
                rep = t; ns = i; pop(3); break;
              }
              copy(rr + i * nr, tt, tt[0] + 1);
            }
          }
          sp[i] = c; swp(); pop();
          sub(); if (sign() == 0) { pop(); ns = i + 1; break; }
          push(10u); mul(); dup(); mod();
        }
        if (mem != default) Marshal.FreeCoTaskMem(mem);
      }
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
      public void tor(ReadOnlySpan<char> sp, int bas = 10, char sep = default)
      {
        push(); push(); push(); push(1u); // rat p = 0, a = 0, b = 0, e = 1;
        for (int i = sp.Length - 1; i >= 0; i--)
        {
          var c = sp[i];
          if (c >= '0' && c <= '9' || (bas == 16 && (c | 0x20) >= 'a' && (c | 0x20) <= 'f'))
          {
            if (c > '9') c = (char)((c | 0x20) + ('9' + 1 - 'a'));
            if (c != '0') { push(unchecked((uint)(c - '0'))); mul(0, 1); add(3, 0); pop(); } // a += (c - '0') * e;
            if (i > 0) { push(bas); mul(); }
            continue; // e *= ba;
          }
          if (c == '.' || c == ',')
          {
            if (sep != default && sep != c) continue;
            dup(); swp(2); pop(); continue; // b = e; 
          }
          if (c == '+') continue;
          if (c == '-') { neg(2); continue; } // a = -a;
          if ((c | 0x20) == 'e')
          {
            pop(2); pow(10, popi()); swp(); // p = pow10(a); 
            push(); push(1u); continue; // b = 0; e = 1;
          }
          if (c == '\'')
          {
            dup(); dup(); push(1u); sub(); // a *= e / (e - 1);
            div(); mul(3, 0); pop(); continue;
          }
          if (c == '/') // 1 / 3    
          {
            swp(1, 2); pop(); push(1u); continue;
          }
          continue;
        }
        pop(); if (sign() != 0) div(); else pop(); // if (b.sign != 0) a /= b;
        swp(); if (sign() != 0) mul(); else pop(); // if (p.sign != 0) a *= p; 
      }
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
      public void sqrt(uint c)
      {
        var e = bdi(); if (e == -int.MaxValue) return; // 0
        e = Math.Abs(e); if (e > c) c = unchecked((uint)e);
        uint m = mark(), s; //get(m - 1, out double d); // best possible initial est: 
        //if (double.IsNormal(d)) push(Math.Sqrt(d));   // todo: enable afte checks, best possible start value
        //else
        {
          dup(); if ((s = msb()) > 1) shr(s >> 1); // est
          inv(); if ((s = msb()) > 1) shr(s >> 1); inv();
        }
        //uint i = 0;
        for (; ; )
        {
          dup(); sqr(); sub(0, 2); var t = -bdi();
          pop(); if (t > c) break; //i++;
          lim(c); div(m - 1, m); add(); push(2u); div();
        }
        swp(); pop(); //Debug.WriteLine("it " + i);
      }
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
      public void log2(uint c)
      {
        if (sign() <= 0) { pop(); pnan(); return; } // NaN
        lim(c + 32); // todo: check, lim x?
        var a = bdi(); // var c = _shl(1, a); x = x / c;
        if (a != 0) { push(1u); shl(unchecked((uint)(a > 0 ? a : -a))); if (a > 0) div(); else mul(); }
        var e = cmpi(0, 1); // push(1u); var e = cmp(1, 0); pop();
        if (e == 0) { pop(); push(a); return; } //if (x == 1) return a;
        if (e < 0) { shl(1); a--; } // adjust bdi //todo: lim x ?
        Debug.Assert(cmpi(0, 1) > 0 && cmpi(0, 2) < 0); // if (x <= 1 || x >= 2) { } //todo: remove
        push(); // b 
        for (uint i = 1; i <= c; i++)
        {
          sqr(1); lim(c, 1); // x = x * x; x = lim(x, c);
          if (cmpi(1, 2) <= 0) continue; // if (x < 2) continue;        
          //push(2u); div(2, 0); pop(); // x = x / 2; //todo: shl den       
          swp(); inv(); shl(1); inv(); swp(); // x = x / 2; //todo: shl den
          push(1u); shl(i); inv(); // var p = Pow(2, -i); //var b = bdi(); if (i == c) { }
          add(); lim(c); // b += p;
        }
        swp(); pop(); if (a != 0) { push(a); add(); } // return a + b;
      }
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
      public void log(uint c)
      {
        log2(c); push(1u); exp(c); log2(c); div(); // exponentially faster for rat
        // todo: check again https://en.wikipedia.org/wiki/Natural_logarithm#High_precision
        // if (sign() <= 0) { pop(); pnan(); return; }
        // push(); swp(); // r
        // var t = bdi(); c += (uint)Math.Abs(t) + 16;
        // push(1u); sub(); dup(); push(2u); add(); div(); // p = (x - 1) / (x + 1)
        // dup(); sqr(); // f = p * p
        // for (uint i = 0, m = mark(); ; i++)
        // {
        //   push((i << 1) + 1); div(m - 2, m); swp(); pop();// d = p / ((n << 1) + 1)
        //   add(3, 0); // r += d        
        //   if (-bdi() >= c) break; // d < 1e-...
        //   pop(); mul(1, 0); // p *= f;
        //   lim(c, 1); lim(c, 2); // lim p, r
        // }
        // pop(3); shl(1); //r *= 2
      }
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
      public void exp(uint c) //todo: catch cases fast exp(1), ...
      {
        var s = sign(); if (s < 0) neg(); c += 16; //todo: exp NaN fix
        dup(); push(1u); add(); // r = x + 1
        dup(1); // d = x
        for (uint i = 2, m = mark(); ; i++)
        {
          push(i); div(m - 3, m); mul(2, 0); pop(2); // d *= x / i;
          add(1, 0); lim(c, 1); if (-bdi() >= c) break;
        }
        pop(); swp(); pop(); if (s < 0) inv();
      }
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
      public void pi(uint c) //todo: c = f(c), cache
      {
        push(); //alg: https://en.wikipedia.org/wiki/Bellard%27s_formula
        for (uint n = 0; ; n++)
        {
          uint a = n << 2, b = n * 10;
          push(1u); shl(b); inv(); if ((n & 1) != 0) neg(); //pow(-1, unchecked((int)n)); pow(2, unchecked((int)b)); div();
          push(-32);  /**/ push(a + 1); div();
          push(-1);   /**/ push(a + 3); div(); add();
          push(256u); /**/ push(b + 1); div(); add();
          push(-64);  /**/ push(b + 3); div(); add();
          push(-4);   /**/ push(b + 5); div(); add();
          push(-4);   /**/ push(b + 7); div(); add();
          push(1u);   /**/ push(b + 9); div(); add();
          mul(); var t = -bdi();
          add(); lim(c + 64); if (t > c + 32) break;
        }
        shr(6); //push(64); div(); //shr checked up 4159
      }
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
      public void sin(uint c, bool cos)
      {
        var e = bdi() << 2; c += 16; if (e > c) { } // bdi x
        var m = mark(); pi(e > c ? unchecked((uint)e) : c); shr(1); // push PI2
        if (cos) add(1, 0); // x += PI2 
        div(m - 1, m); mod(); swp(); pop(); // push divmod(x, PI2)
        var s = this.p[this.i - 1][1]; // dup(); var s = pop_int(); // seg
        mul(0, 1); neg(); add(2, 0); pop(); // x - trunc(x / PI2) * PI2
        if ((s & 1) != 0) { swp(); sub(); } else pop(); // x = PI2 - x, pop PI2
        if ((s & 2) != 0) neg(); lim(c); // x = -x, lim x
        push(6u); dup(1); dup(); sqr(); lim(c); swp(); // 3!, x^2, x
        for (uint i = 1, k = 4; ; i++, k += 2)
        {
          mul(1, 0); lim(c, 1); dup(1); div(0, 3); // x +/-= x^n / n!, n = 3, 5, 7, ...
          var b = -bdi(); if (b > c) break; lim(c);
          if ((i & 1) != 0) neg(); add(4, 0); pop(); lim(c, 3); // x += 
          push(k * (k + 1)); mul(3, 0); pop(); mul(1, 0); // n! *=, x^n *=
        }
        pop(4);
      }
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
      public void atan(uint c)
      {
        var s = sign(); if (s == 0) return; // atan(0) = 0        
        if (s < 0) neg(); // atan(-x) = -atan(x)        
        var td = cmpi(0, 1) > 0; if (td) inv(); // atan(1/x) = pi/2 - atan(x)
        int sh = 0; // atan(x) = atan(c) + atan((x - c) / (1 + c * x)) -> c == (x - c) / (1 + c * x) -> c <= 0.1 
        push(1u); push(10u); div(); // push 0.1
        while (cmp(1, 0) > 0) // z > 0.1
        {
          dup(1); inv(); dup(); sqr(); push(1u); add(); // y = 1 / z; y = y * y; y += 1;
          sqrt(c); sub(0, 1); swp(0, 3); pop(2); // z = sqrt(y) - z
          lim(c, 1); sh++;
        }
        pop(); // euler taylor:
        dup(); sqr(); lim(c); // zsqr = z * z
        dup(); push(1u); add(); // zsqr1 = zsqr + 1
        push(1u); dup(); // a = 1, m = 1
        for (int n = 1; n < 20; n++) //todo: 20 -> f(c) !!!
        {
          dup(3); push(n << 1); mul(); // (2 * k) * zsqr
          dup(3); push((n << 1) + 1); mul(); // (2 * k + 1) * zsqr1
          div(); mul(); // m *= (2 * n) * zsqr / ((2 * n + 1) * zsqr1)
          add(1, 0); lim(c, 1); // a += m
        }
        mul(1, 4); div(1, 2); //z *= z / zsqr1;
        swp(1, 4); pop(4);
        if (sh != 0) shl(unchecked((uint)sh)); // z *= pow(2, sh); 
        if (td) { neg(); pi(c); shr(1); add(); } // z = pi / 2 - z;
        if (s < 0) neg();
      }
      /// <summary>
      /// Frees the current thread static instance of the <see cref="CPU"/> and associated buffers.<br/>
      /// A new <see cref="CPU"/> is then automatically created for subsequent calculations.
      /// </summary>
      /// <remarks>
      /// After calculating with large numbers, the <see cref="CPU"/> naturally keeps large buffers that can be safely released in this way.<br/>
      /// More precisely, the operation does not destroy anything. The current CPU instance can continue to be used. But the GC can collect them later when there are no more references to them.
      /// </remarks>
      public void free()
      {
        cpu = null;
      }

      /// <summary>
      /// Increments the value on top of the stack.
      /// </summary>
      public void inc() { push(1u); add(); } //todo: opt.
      /// <summary>
      /// Decrements the value on top of the stack.
      /// </summary>
      public void dec() { push(1u); sub(); } //todo: opt.
      /// <summary>
      /// Converts an integer value on top of the stack to its two's complement and vice versa.
      /// </summary>
      /// <remarks>
      /// Negative signed integer values are converted to it's unsigned variant.<br/>
      /// Positive unsigned integers are converted to it' signed variant.<br/>
      /// Parameter <paramref name="c"/> defines a integer type size in bytes.
      /// </remarks>
      /// <param name="c">Integer type size in bytes.</param>
      public void toc(uint c)
      {
        fixed (uint* u = this.p[this.i - 1])
        {
          var nu = u[0] & 0x3fffffff; if (nu == 0) return;
          var si = (u[0] & 0x80000000) != 0; var cc = si ? c >> 2 : 0;
          fixed (uint* v = rent(nu + (cc + 3)))
          {
            if (si)
            {
              uint nv = nu, i = 1; if (nv < cc) nv = cc;
              for (; i <= nu;) { v[i] = ~u[i] + 1; if (v[i++] != 0) break; }
              for (; i <= nu; i++) v[i] = ~u[i];
              for (; i <= nv; i++) v[i] = 0xffffffff;
              v[0] = i - 1; *(ulong*)(v + i) = 0x100000001; //if ((c & 3) != 0) { }
            }
            else
            {
              uint nv = nu, i = 1, l = 1, e = 1;
              var xx = BitOperations.LeadingZeroCount(u[nu]);
              if (xx != 0) u[nu] |= 0xffffffff << (32 - xx);
              for (; i <= nu; i++) { if ((v[i] = ~u[i] + e) == 0) e = 1; else { e = 0; l = i; } }
              v[0] = 0x80000000 | l; *(ulong*)(v + (l + 1)) = 0x100000001;
            }
          }
        }
        swp(); pop();
      }
      /// <summary>
      /// Evaluates whether the numerator of the value on top of the stack is a power of two.
      /// </summary>
      /// <returns>true if the specified value is a power of two; false otherwise.</returns>
      public bool ipt()
      {
        fixed (uint* p = this.p[this.i - 1])
        {
          uint n = p[0] & 0x3fffffff, v = p[n]; if ((v & (v - 1)) != 0 || v == 0) return false;
          for (uint i = 1; i < n; i++) if (p[i] != 0) return false;
        }
        return true;
      }

      uint[] rent(uint n)
      {
        var a = p[i++];
        if (a == null || a.Length < n)
        {
          p[i - 1] = a = new uint[1 << (BitOperations.Log2(n - 1) + 1)]; //4, 8, 16, ...
          if (i == p.Length) Array.Resize(ref p, i << 1);
        }
        return a;
      }
      void add(uint* u, uint* v, bool neg)
      {
        var l = len(u) + len(v);
        fixed (uint* w = rent(l * 3))
        {
          var sa = u[0] & 0x80000000;
          var sb = v[0] & 0x80000000; if (neg) sb ^= 0x80000000;
          var ud = u + ((u[0] & 0x3fffffff) + 1);
          var vd = v + ((v[0] & 0x3fffffff) + 1);
          uint* s = w + (l << 1), t;
          mul(u, vd, s);
          mul(v, ud, t = s + (s[0] + 1));
          if (sa == sb) add(s, t, w);
          else
          {
            var si = cms(s, t); if (si == 0) { *(ulong*)w = 1; *(ulong*)(w + 2) = 0x100000001; return; }
            if (si < 0) { var h = s; s = t; t = h; sa ^= 0x80000000; }
            sub(s, t, w);
          }
          if (*(ulong*)w == 1) { *(ulong*)(w + 2) = 0x100000001; return; }
          mul(ud, vd, w + (w[0] + 1)); w[0] |= sa | 0x40000000;
        }
      }
      void mul(uint* u, uint* v, bool inv)
      {
        fixed (uint* w = rent(len(u) + len(v)))
        {
          var s = (v[0] & 0x3fffffff) + 1;
          var t = (u[0] & 0x3fffffff) + 1;
          mul(u + 0, inv ? v + s : v, w); if (*(ulong*)w == 1 && *(ulong*)(u + t) != 1) { *(ulong*)(w + 2) = 0x100000001; return; } // 0 but keep NaN
          mul(u + t, inv ? v : v + s, w + (w[0] + 1));
          w[0] |= ((u[0] ^ v[0]) & 0x80000000) | 0x40000000;
        }
      }
      bool equ(uint[] a, uint[] b)
      {
        if (a == b) return true; //sort's
        fixed (uint* u = a, v = b)
        {
          if (u == null) return isz(v);
          if (v == null) return isz(u);
          if (((u[0] | v[0]) & 0x40000000) == 0)
          {
            var n = len(u); // for (uint i = 0; i < n; i++) if (u[i] != v[i]) return false;
            for (uint i = 0, c = n >> 1; i < c; i++) //todo: opt
              if (((ulong*)u)[i] != ((ulong*)v)[i])
                return false;
            if ((n & 1) != 0) return u[n - 1] == v[n - 1];
            return true;
          }
          return cmp(u, v) == 0;
        }
      }
      int cmp(uint* a, uint* b)
      {
        int sa = sig(a), sb = sig(b);
        if (sa != sb) return sa > sb ? +1 : -1; if (sa == 0) return 0;
        uint na, la = (na = a[0] & 0x3fffffff) + a[na + 1];
        uint nb, lb = (nb = b[0] & 0x3fffffff) + b[nb + 1];
        fixed (uint* u = rent(la + lb + 4))
        {
          mul(a, b + (nb + 1), u); var v = u + (u[0] + 1);
          mul(b, a + (na + 1), v); sa *= cms(u, v);
          pop(); return sa;
        }
      }
      void norm(uint* p)
      {
        Debug.Assert((p[0] & 0x40000000) != 0);
        var d = p + ((p[0] & 0x3fffffff) + 1);
        if (*(ulong*)d == 0x100000001) { p[0] &= ~0x40000000u; return; }
        if (*(ulong*)d == 1) { *(ulong*)p = *(ulong*)(p + 2) = 1; return; } //NaN
        var l = len(p);
        fixed (uint* s = rent(l << 1))
        {
          copy(s, p, l); s[0] &= 0x3fffffff;
          var e = gcd(s, s + (s[0] + 1));
          if (*(ulong*)e == 0x100000001) { p[0] &= ~0x40000000u; pop(); return; }
          var t = s + l; var h = p[0];
          copy(t, p, l); t[0] &= 0x3fffffff;
          d = t + (t[0] + 1); div(t, e, p); div(d, e, p + (p[0] + 1));
          p[0] |= (h & 0x80000000); pop();
        }
      }
      void pnan()
      {
        fixed (uint* p = rent(4)) *(ulong*)p = *(ulong*)(p + 2) = 1;
      }

      static void add(uint* a, uint* b, uint* r)
      {
        if (a[0] < b[0]) { var t = a; a = b; b = t; }
        uint i = 1, na = a[0], nb = b[0]; var c = 0ul;
        for (; i <= nb; i++) { var u = (ulong)a[i] + b[i] + c; r[i] = unchecked((uint)u); c = u >> 32; }
        for (; i <= na; i++) { var u = (ulong)a[i] + c; /*  */ r[i] = unchecked((uint)u); c = u >> 32; }
        if (c != 0) r[++na] = unchecked((uint)c); r[0] = na;
      }
      static void sub(uint* a, uint* b, uint* r)
      {
        uint i = 1, na = a[0], nb = b[0]; var c = 0L; Debug.Assert(na >= nb);
        for (; i <= nb; i++) { var u = a[i] + c - b[i]; r[i] = unchecked((uint)u); c = u >> 32; }
        for (; i <= na; i++) { var u = a[i] + c; /*  */ r[i] = unchecked((uint)u); c = u >> 32; }
        for (i--; i > 1 && r[i] == 0; i--) ; r[0] = i; Debug.Assert(c == 0);
      }
      static void mul(uint* a, uint* b, uint* r)
      {
        uint na = a[0] & 0x3fffffff, nb = b[0] & 0x3fffffff;
        if (na < nb) { var u = na; na = nb; nb = u; var v = a; a = b; b = v; }
        uint f = b[1];
        if (nb == 1)
        {
          switch (f) //jump table
          {
            case 0: *(ulong*)r = 1; return;
            case 1: copy(r, a, na + 1); r[0] = na; return;
              //case 2: //todo: opt. shl(1)
              //case 3: //todo: opt. + +
              //case 4: //todo: opt. shl(2)
              //...
          }
          //if (f == 0) { *(ulong*)r = 1; return; }
          //if (f == 1) { copy(r, a, na + 1); r[0] = na; /* r[0] = na; for (uint i = 1; i <= na; i++) r[i] = a[i]; */ return; }
        }
        if (na == 1) *(ulong*)(r + 1) = (ulong)a[1] * b[1]; //todo: jt
        else
        if (na == 2 && Bmi2.X64.IsSupported)
        {
          *(ulong*)(r + 3) = Bmi2.X64.MultiplyNoFlags(*(ulong*)(a + 1), nb == 2 ? *(ulong*)(b + 1) : b[1], (ulong*)(r + 1));
        }
        else if (na >= 00_32) //nb <= na
        {
          var n = na + nb; copy(r + 1, n); kmu(a + 1, na, b + 1, nb, r + 1, n);
        }
        else
        {
          ulong c = 0;
          for (uint i = 1; i <= na; i++)
          {
            var d = (ulong)a[i] * f + c;
            r[i] = unchecked((uint)d); c = d >> 32;
          }
          r[na + 1] = unchecked((uint)c);
          for (uint k = 1; k < nb; k++)
          {
            f = b[k + 1]; c = 0; var s = r + k;
            //if (f == 0) { s[na + 1] = 0; continue; }
            for (uint i = 1; i <= na; i++)
            {
              var d = (ulong)a[i] * f + c + s[i];
              s[i] = unchecked((uint)d); c = d >> 32;
            }
            s[na + 1] = unchecked((uint)c);
          }
        }
        if (r[r[0] = na + nb] == 0 && r[0] > 1) { r[0]--; Debug.Assert(!(r[r[0]] == 0 && r[0] > 1)); }
      }
      static void sqr(uint* a, uint* r) //todo: opt kmu for sqr
      {
        uint n = *a++ & 0x3fffffff; var v = r + 1;
        if (n == 1)
        {
          *(ulong*)v = (ulong)a[0] * a[0];
          r[0] = r[2] != 0 ? 2u : 1u; return;
        }
        if (n == 2)
        {
          if (Bmi2.X64.IsSupported)
          {
            ((ulong*)v)[1] = Bmi2.X64.MultiplyNoFlags(*(ulong*)a, *(ulong*)a, ((ulong*)v));
          }
          else
          {
            var t1 = (ulong)a[0] * a[0];
            var t2 = (ulong)a[0] * a[1];
            var t3 = (ulong)a[1] * a[1];
            var t4 = t2 + (t1 >> 32);
            var t5 = t2 + (uint)t4;
            ((ulong*)v)[0] = t5 << 32 | (uint)t1;
            ((ulong*)v)[1] = t3 + (t4 >> 32) + (t5 >> 32);
          }
        }
        else if (n >= 0_040) { copy(v, n + n); kmu(a, n, v, n + n); }
        else
        {
          for (uint i = 0; i < n; i++)
          {
            ulong c = 0, e, f;
            for (uint j = 0; j < i; j++, c = (f + (e >> 1)) >> 31)
              v[i + j] = unchecked((uint)((e = v[i + j] + c) + ((f = (ulong)a[j] * a[i]) << 1)));
            *(ulong*)(v + (i << 1)) = (ulong)a[i] * a[i] + c;
          }
        }
        if (r[r[0] = n << 1] == 0 && r[0] > 1) { r[0]--; Debug.Assert(!(r[r[0]] == 0 && r[0] > 1)); }
      }
      static void div(uint* a, uint* b, uint* r)
      {
        uint na = a[0], nb = b[0] & 0x3fffffff; if (r != null) *(ulong*)r = 1;
        if (na < nb) return;
        if (na == 1)
        {
          if (b[1] == 0) return; // keep NaN
          uint q = a[1] / b[1], m = a[1] - q * b[1];
          a[a[0] = 1] = m; if (r != null) r[1] = q; return;
        }
        if (na == 2)
        {
          ulong x = *(ulong*)(a + 1), y = nb != 1 ? *(ulong*)(b + 1) : b[1];
          ulong q = x / y, m = x - (q * y);
          *(ulong*)(a + 1) = m; a[0] = a[2] != 0 ? 2u : 1u; if (r == null) return;
          *(ulong*)(r + 1) = q; r[0] = r[2] != 0 ? 2u : 1u; return;
        }
        if (nb == 1)
        {
          ulong x = 0; uint y = b[1];
          for (uint i = na; i != 0; i--)
          {
            ulong q = (x = (x << 32) | a[i]) / y, m = x - (q * y);
            if (r != null) r[i] = unchecked((uint)q); x = m;
          }
          a[0] = 1; a[1] = unchecked((uint)x); if (r == null) return;
          for (; na > 1 && r[na] == 0; na--) ; r[0] = na; return;
        }
        uint dh = b[nb], dl = nb > 1 ? b[nb - 1] : 0;
        int sh = BitOperations.LeadingZeroCount(dh), sb = 32 - sh;
        if (sh > 0)
        {
          uint db = nb > 2 ? b[nb - 2] : 0;
          dh = (dh << sh) | (dl >> sb);
          dl = (dl << sh) | (db >> sb);
        }
        for (uint i = na, d; i >= nb; i--)
        {
          uint n = i - nb, t = i < na ? a[i + 1] : 0;
          ulong vh = ((ulong)t << 32) | a[i];
          uint vl = i > 1 ? a[i - 1] : 0;
          if (sh > 0)
          {
            uint va = i > 2 ? a[i - 2] : 0;
            vh = (vh << sh) | (vl >> sb);
            vl = (vl << sh) | (va >> sb);
          }
          ulong di = vh / dh; if (di > 0xffffffff) di = 0xffffffff;
          for (; ; di--)
          {
            ulong th = dh * di, tl = dl * di; th += tl >> 32;
            if (th < vh) break; if (th > vh) continue;
            if ((tl & 0xffffffff) > vl) continue; break;
          }
          if (di != 0)
          {
            ulong c = 0; var p = a + n;
            for (uint k = 1; k <= nb; k++)
            {
              c += b[k] * di; d = unchecked((uint)c); c >>= 32;
              if (p[k] < d) c++; p[k] = p[k] - d; // ryujit
            }
            if (unchecked((uint)c) != t)
            {
              c = 0; p = a + n; di--; ulong g;
              for (uint k = 1; k <= nb; k++, c = g >> 32) p[k] = unchecked((uint)(g = (p[k] + c) + b[k]));
            }
          }
          if (di != 0 && r != null)
          {
            var x = n + 1; for (var j = r[0] + 1; j < x; j++) r[j] = 0;
            if (r[0] < x) r[0] = x; r[x] = unchecked((uint)di);
          }
          if (i < na) a[i + 1] = 0;
        }
        for (; a[0] > 1 && a[a[0]] == 0; a[0]--) ; //todo: nb == 1 case, test and rem
      }
      static void shl(uint* p, int c)
      {
        int s = c & 31, r = 32 - s; uint d = unchecked((uint)(c >> 5)), n = p[0] & 0x3fffffff; p[0] = p[n + 1] = 0;
        for (uint i = n + 1; i > 0; i--) p[i + d] = (p[i] << s) | unchecked((uint)((ulong)p[i - 1] >> r));
        for (uint i = 1; i <= d; i++) p[i] = 0;
        n += d; p[0] = p[n + 1] != 0 ? n + 1 : n;
      }
      internal static void shr(uint* p, int c)
      {
        int s = c & 31, r = 32 - s; uint n = p[0] & 0x3fffffff, i = 0, k = 1 + unchecked((uint)c >> 5), l = k <= n ? p[k++] >> s : 0;
        while (k <= n) { var t = p[k++]; p[++i] = l | unchecked((uint)((ulong)t << r)); l = t >> s; }
        if (l != 0 || i == 0) p[++i] = l; p[0] = i;
      }
      static uint* gcd(uint* u, uint* v)
      {
        var su = clz(u); if (su != 0) shr(u, su);
        var sv = clz(v); if (sv != 0) shr(v, sv); if (su > sv) su = sv;
        if (cms(u, v) < 0) { var t = u; u = v; v = t; }
        while (v[0] > 2)
        {
          uint e = u[0] - v[0];
          if (e <= 2)
          {
            ulong xh = u[u[0]], xm = u[u[0] - 1], xl = u[u[0] - 2];
            ulong yh = e == 0 ? v[v[0]] : 0, ym = e <= 1 ? v[v[0] - (1 - e)] : 0, yl = v[v[0] - (2 - e)];
            int z = BitOperations.LeadingZeroCount(unchecked((uint)xh));
            ulong x = ((xh << 32 + z) | (xm << z) | (xl >> 32 - z)) >> 1;
            ulong y = ((yh << 32 + z) | (ym << z) | (yl >> 32 - z)) >> 1; Debug.Assert(x >= y);
            uint a = 1, b = 0, c = 0, d = 1, k = 0;
            while (y != 0)
            {
              ulong q, r, s, t;
              q = x / y; if (q > 0xffffffff) break;
              r = a + q * c; if (r > 0x7fffffff) break;
              s = b + q * d; if (s > 0x7fffffff) break;
              t = x - q * y; if (t < s || t + r > y - c) break;
              a = unchecked((uint)r);
              b = unchecked((uint)s); k++;
              x = t; if (x == b) break;
              q = y / x; if (q > 0xffffffff) break;
              r = d + q * b; if (r > 0x7fffffff) break;
              s = c + q * a; if (s > 0x7fffffff) break;
              t = y - q * x; if (t < s || t + r > x - b) break;
              d = unchecked((uint)r);
              c = unchecked((uint)s); k++;
              y = t; if (y == c) break;
            }
            if (b != 0)
            {
              long cx = 0, cy = 0;
              for (uint i = 1, l = v[0]; i <= l; i++)
              {
                long ui = u[i], vi = v[i];
                long dx = a * ui - b * vi + cx; u[i] = unchecked((uint)dx); cx = dx >> 32;
                long dy = d * vi - c * ui + cy; v[i] = unchecked((uint)dy); cy = dy >> 32;
              }
              u[0] = v[0];
              for (; u[0] > 1 && u[u[0]] == 0; u[0]--) ;
              for (; v[0] > 1 && v[v[0]] == 0; v[0]--) ;
              if ((k & 1) != 0) { var t = u; u = v; v = t; }
              continue;
            }
          }
          div(u, v, null); var p = u; u = v; v = p;
        }
        if (*(ulong*)v != 1)
        {
          if (u[0] > 2) { div(u, v, null); var t = u; u = v; v = t; if (*(ulong*)v == 1) goto m1; }
          ulong x = u[0] != 1 ? *(ulong*)(u + 1) : u[1];
          ulong y = v[0] != 1 ? *(ulong*)(v + 1) : v[1];
          while (y != 0 && (x | y) > 0xffffffff) { var t = x % y; x = y; y = t; }
          while (y != 0) { var t = unchecked((uint)x) % unchecked((uint)y); x = y; y = t; }
          *(ulong*)(u + 1) = x; u[0] = u[2] != 0 ? 2u : 1u; m1:;
        }
        if (su != 0) shl(u, su);
        return u;
      }
      static int clz(uint* p)
      {
        Debug.Assert(!isz(p));
        for (int i = 1; ; i++)
        {
          var x = BitOperations.TrailingZeroCount(p[i]);
          if (x != 32) return ((i - 1) << 5) + x;
        }
      }
      static int cms(uint* a, uint* b)
      {
        uint na = a[0], nb = b[0];
        if (na != nb) return na > nb ? +1 : -1;
        for (var i = na + 1; --i != 0;)
          if (a[i] != b[i]) return a[i] > b[i] ? +1 : -1;
        return 0;
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      static bool isz(uint* p)
      {
        //Debug.Assert((*(ulong*)p & ~(ulong)(0x40000000 | 0x80000000)) != 1 || *(ulong*)p == 1);
        return ((ulong*)p)[0] == 1 && ((ulong*)p)[1] != 1; // && !NaN 
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      static bool isint(uint* p)
      {
        return *(ulong*)(p + ((p[0] & 0x3fffffff) + 1)) == 0x100000001;
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      static int sig(uint* p)
      {
        return (p[0] & 0x80000000) != 0 ? -1 : isz(p) ? 0 : +1;
      }
      static void kmu(uint* a, uint na, uint* b, uint nb, uint* r, uint nr) //todo: opt, non recursive on stack
      {
        Debug.Assert(na >= nb && nr == na + nb);
        if (nb < 00_20)
        {
          for (uint i = 0; i < nb; i++) //todo: opt after checks, the 0 row trick
          {
            ulong c = 0, d;
            for (uint k = 0; k < na; k++, c = d >> 32)  //todo: opt
            {
              //ref uint h = ref r[i + k]; h = unchecked((uint)(d = h + c + (ulong)a[k] * b[i]));
              r[i + k] = unchecked((uint)(d = r[i + k] + c + (ulong)a[k] * b[i]));
            }
            r[i + na] = (uint)c;
          }
          return;
        }
        uint n = nb >> 1, m = n << 1;
        kmu(a, n, b, n, r, m); // p1 = al * bl
        kmu(a + n, na - n, b + n, nb - n, r + m, nr - m); // p2 = ah * bh
        uint r1 = na - n + 1, r2 = nb - n + 1, r3 = r1 + r2, r4 = r1 + r2 + r3;
        uint* t1 = stackalloc uint[(int)r4], t2 = t1 + r1, t3 = t2 + r2; //todo: opt, use cpu.stack  
        copy(t1, r4);
        add(a + n, na - n, a, n, t1, r1);
        add(b + n, nb - n, b, n, t2, r2);
        kmu(t1, r1, t2, r2, t3, r3); // p3 = (ah + al) * (bh + bl)
        sub(r + m, nr - m, r, m, t3, r3);
        add(r + n, nr - n, t3, r3); // (p2 << m) + ((p3 - (p1 + p2)) << n) + p1        
      }
      static void kmu(uint* a, uint na, uint* r, uint nr)
      {
        if (na < 0_032)
        {
          for (uint i = 0; i < na; i++) //todo: opt after checks, the 0 row trick
          {
            ulong c = 0, u, v;
            for (uint k = 0; k < i; k++, c = (v + (u >> 1)) >> 31) //todo: opt
            {
              //ref uint h = ref r[i + k]; h = unchecked((uint)((u = h + c) + ((v = (ulong)a[k] * a[i]) << 1)));
              r[i + k] = unchecked((uint)((u = r[i + k] + c) + ((v = (ulong)a[k] * a[i]) << 1)));
            }
            r[i + i] = unchecked((uint)(u = (ulong)a[i] * a[i] + c)); r[i + i + 1] = (uint)(u >> 32);
          }
          return;
        }
        uint n = na >> 1, m = n << 1;
        kmu(a, n, r, m);
        kmu(a + n, na - n, r + m, nr - m);
        uint r1 = na - n + 1, r2 = r1 + r1;
        uint* t1 = stackalloc uint[(int)(r1 + r2)], t2 = t1 + r1; copy(t1, r1 + r2); //todo: cpu stack
        add(a + n, na - n, a, n, t1, r1);
        kmu(t1, r1, t2, r2);
        sub(r + m, nr - m, r, m, t2, r2);
        add(r + n, (nr - n), t2, r2);
      }
      static void sub(uint* a, uint na, uint* b, uint nb, uint* r, uint nr)
      {
        uint i = 0; long c = 0, d;
        for (; i < nb; i++, c = d >> 32) r[i] = unchecked((uint)(d = (r[i] + c) - a[i] - b[i]));
        for (; i < na; i++, c = d >> 32) r[i] = unchecked((uint)(d = (r[i] + c) - a[i]));
        for (; c != 0 && i < nr; i++, c = d >> 32) r[i] = unchecked((uint)(d = r[i] + c));
      }
      static void add(uint* a, uint na, uint* b, uint nb, uint* r, uint nr)
      {
        uint i = 0; ulong c = 0, d; Debug.Assert(na >= nb && nr == na + 1);
        for (; i < nb; i++, c = d >> 32) r[i] = unchecked((uint)(d = (a[i] + c) + b[i]));
        for (; i < na; i++, c = d >> 32) r[i] = unchecked((uint)(d = a[i] + c));
        r[i] = unchecked((uint)c);
      }
      static void add(uint* a, uint na, uint* b, uint nb)
      {
        uint i = 0; ulong c = 0L, d; Debug.Assert(na >= nb);
        for (; i < nb; i++, c = d >> 32) a[i] = unchecked((uint)(d = (a[i] + c) + b[i]));
        for (; c != 0 && i < na; i++, c = d >> 32) a[i] = unchecked((uint)(d = a[i] + c));
      }
      #region experimental
      internal void toi(BigRational v, uint* p, uint f) //int type conversions, Int32, Unt32,..., Int128, UInt128, 256, 512 ... 
      {
        if (v.p == null) return; uint n = f & 0xff;
        push(v); var s = sign();
        if ((f & 0x0100) != 0 && s <= 0) //uint <= 0 -> 0 incl. NaN 
        {
          pop(); if ((f & 0x1000) != 0 && s < 0 || IsNaN(v)) throw new OverflowException(); //todo: message text range       
          return;
        }
        rnd(0, 0); uint b = msb(), c = n << 5;
        if (b >= c)
        {
          if ((f & 0x0100) == 0) //si
          {
            if (((f & 0x1000) == 0) || !(b == c && s < 0 && ipt()))
            {
              pop(); if ((f & 0x1000) != 0) throw new OverflowException(); //todo: message text range
              n--; //todo: opt. small types after tests
              if (s > 0) { for (c = 0; c < n; c++) p[c] = 0xffffffff; p[n] = 0x7fffffff; } // MinValue
              else p[n] = 0x80000000; // MinValue
              return;
            }
          }
          else //ui
          {
            if (b > c)
            {
              pop(); if ((f & 0x1000) != 0) throw new OverflowException(); //todo: message text range
              for (c = 0; c < n; c++) p[c] = 0xffffffff; //MaxValue
              return;
            }
          }
        }
        fixed (uint* t = this.p[this.i - 1])
        {
          var u = t[0] & 0x3fffffff;
          if (u == 1) *p = t[1]; //most common case 
          else copy(p, t + 1, n < u ? n : u);
        }
        pop(); if (s > 0) return;
        for (b = 0, c = 1; b < n; b++) //todo: opt. small std types after tests
        {
          var x = unchecked((ulong)(p[b] ^ 0xffffffff) + c);
          p[b] = unchecked((uint)x); c = unchecked((uint)(x >> 32));
        }
      }
      //INumber only, to avoid another ThreadLocal static root - the CPU doesn't need it
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal int maxdigits = 30; //INumber default limitation for irrational funcs 
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal void* sp; //debug security for visualizer and cross thread access 
      #endregion
    }
    #region private 
    readonly uint[] p;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    BigRational(uint[] p) => this.p = p;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint len(uint* p)
    {
      var n = p[0] & 0x3fffffff;
      return n + p[n + 1] + 2;
    }
    static void copy(uint* d, uint n)
    {
      uint i = 0, c;
      for (c = n & ~3u; i < c; i += 4) *(decimal*)&((byte*)d)[i << 2] = default; // X64 vxorps, vmovdqu
      for (c = n & ~1u; i < c; i += 2) *(ulong*)&((byte*)d)[i << 2] = default;
      if (i != n) d[i] = 0; //if ?
    }
    static void copy(uint* d, uint* s, uint n)
    {
      //new ReadOnlySpan<uint>(s, unchecked((int)n)).CopyTo(new Span<uint>(d, unchecked((int)n))); return; // 10% slower 
      //if (n > 16) { new ReadOnlySpan<uint>(s, unchecked((int)n)).CopyTo(new Span<uint>(d, unchecked((int)n))); return; } // 5% slower 
      uint i = 0, c;
      for (c = n & ~3u; i < c; i += 4) *(decimal*)&((byte*)d)[i << 2] = *(decimal*)&((byte*)s)[i << 2]; // RyuJIT vmovdqu
      for (c = n & ~1u; i < c; i += 2) *(ulong*)&((byte*)d)[i << 2] = *(ulong*)&((byte*)s)[i << 2];
      if (i != n) d[i] = s[i]; //if ?
    }
    [ThreadStatic, DebuggerBrowsable(DebuggerBrowsableState.Never)] //debug visualizer security
    static CPU? cpu;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //debug visualizer security
    static CPU main_cpu => cpu ??= new CPU();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //debug visualizer security
    static uint[][]? cache;
    static BigRational cachx(uint x, uint* s, uint n)
    {
      var a = cache ??= new uint[4][]; //worst case twice in threads doesn't matter
      var b = a[x]; if (b == null) fixed (uint* d = b = a[x] = new uint[n]) copy(d, s, n);
      return new BigRational(b);
      //var p = cache != null ? cache[x] : null;
      //if (p == null) lock (string.Empty) fixed (uint* d = (cache ??= new uint[4][])[x] = p = new uint[n]) copy(d, s, n);
      //return new BigRational(p);
    }
    #endregion
    #region debug support
    sealed class DebugView
    {
      readonly CPU p;
      public DebugView(CPU p) => this.p = p;
      public DebugView(SafeCPU p) => this.p = p.p;
      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public BigRational[] Items
      {
        get
        {
          var a = new BigRational[p.i];
          for (int i = 0; i < p.i; i++)
            a[i] = new BigRational(p.p[p.i - 1 - i]);
          return a;
        }
      }
    }
    #endregion
  }

}
