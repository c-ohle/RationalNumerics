using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Numerics.Rational
{
  /// <summary>
  /// Represents an arbitrarily large rational number.
  /// </summary>
  [Serializable, SkipLocalsInit, DebuggerDisplay("{ToString(\"\"),nq}")]
  public unsafe readonly partial struct NewRational : IComparable<NewRational>, IEquatable<NewRational>, IFormattable, ISpanFormattable
  {
    private readonly uint[] p;
    /// <summary>
    /// Converts the numeric value of the current <see cref="NewRational"/> instance to its equivalent string representation.
    /// </summary>
    /// <remarks>
    /// Behavior like <see cref="double.ToString()"/> 
    /// with decimal notation in the range E-04 to E+16<br/>
    /// Special: Maximum number of digits is by default 32.<br/>
    /// Special: Repetions marked with apostrophe (') like 1/3 as "0.'3" instead of "0.333333…"<br/>
    /// Special: Overflow marked with trailing dots (…) like: "1.234567…"<br/>
    /// </remarks>
    /// <returns>The string representation of the current <see cref="NewRational"/> value.</returns>
    public override readonly string ToString()
    {
      Span<char> sp = stackalloc char[64];
      TryFormat(sp, out var ns); return sp.Slice(0, ns).ToString();
    }
    /// <summary>
    /// Formats this <see cref="NewRational"/> instance to
    /// its string representation by using the specified format and culture-specific
    /// format information.
    /// </summary>
    /// <remarks>
    /// Supported numeric standard formats: C, D, E, F, G, N, P, R like "E10".<br/>
    /// Custom formats supported, currently in the range +/- E-308 .. E+308 like "0.#####"<br/>
    /// Standard formats F, E with unlimited many digits like: "F1000" for 1000 decimal digits.<br/>
    /// Special R: behavior like <see cref="double.ToString(string?)"/> with fallback to format Q to ensure the round-trip to an identical number.<br/>
    /// Additional: Format Q for Fraction notation returns "1/3" instead of "0.333333…"<br/>
    /// Additional: Format L for Large, like F without trailing zeros: L5 returns 1.5 instead of 1.50000…<br/>
    /// </remarks>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of the current <see cref="NewRational"/> value as
    /// specified by the format and provider parameters.</returns>
    public readonly string ToString(string? format, IFormatProvider? provider = default)
    {
      provider ??= format?.Length == 0 ? NumberFormatInfo.InvariantInfo : NumberFormatInfo.CurrentInfo; //DebuggerDisplay("", null);
      Span<char> sp = stackalloc char[100]; char[]? a = null;
      for (int ns, na = 1024; ; na <<= 1)
      {
        try { if (TryFormat(sp, out ns, format, provider)) return sp.Slice(0, ns).ToString(); }
        finally { if (a != null) ArrayPool<char>.Shared.Return(a); }
        sp = a = ArrayPool<char>.Shared.Rent(Math.Max(ns, na));
      }
    }
    /// <summary>
    /// Formats this <see cref="NewRational"/> instance into a span of characters.
    /// </summary>
    /// <remarks>
    /// Supported formats see: <see cref="NewRational.ToString(string?, IFormatProvider?)"/>.
    /// </remarks>
    /// <param name="destination">The span of characters into which this instance will be written.</param>
    /// <param name="charsWritten">When the method returns, contains the length of the span in number of characters.</param>
    /// <param name="format">A read-only span of characters that specifies the format for the formatting operation.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information about value.</param>
    /// <returns>true if the formatting operation succeeds; false otherwise.</returns>
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
      int digs = 32, round = -1, emin = -4, emax = +16; var fc = '\0';
      var info = NumberFormatInfo.GetInstance(provider);
      if (format.Length != 0)
      {
        if (char.IsLetter(format[0]) && (format.Length == 1 || (char.IsNumber(format[1]) && int.TryParse(format.Slice(1), out round))))
          switch (fc = char.ToUpper(format[0]))
          {
            case 'Q': fc = 'R'; digs = 0; round = -1; goto def; // fractions "1/3"
            case 'R': if (round != -1) digs = round; round = -1; goto def;
            case 'F':
            case 'L': // like F without trailing zeros
              if (round == -1) round = info.NumberDecimalDigits;
              digs = Math.Max(0, ILog10(this)) + 1 + round;
              if (digs > destination.Length && destination.Length == 100) { charsWritten = digs; return false; } //hint for ToString(,)
              emin = -(emax = int.MaxValue); goto def;
            case 'E':
              if (round == -1) round = 6; digs = 1 + round; //if (round <= 10) break; 
              if (digs + 32 > destination.Length && destination.Length == 100) { charsWritten = digs + 32; return false; } //hint for ToString(,)
              emax = -(emin = int.MaxValue); goto def;
          }
        var e = ILog10(this);
        if (e <= 28) return ((decimal)this).TryFormat(destination, out charsWritten, format, provider);
        if (e <= 308) return ((double)this).TryFormat(destination, out charsWritten, format, provider);
      }
    def:
      var pb = digs <= 0x8000 ? null : ArrayPool<char>.Shared.Rent(digs);
      var ts = stackalloc char[pb != null ? 0 : digs];
      var ss = pb != null ? pb.AsSpan().Slice(0, digs) : new Span<char>(ts, digs);
      //var ss = new char[digs].AsSpan(); var pb = (char[])null;
      var cpu = task_cpu; cpu.push(this);
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
          apc(ref ws, '0', 1);
          aps(ref ws, info.NumberDecimalSeparator);
          apc(ref ws, '0', -exp - 1);
          if (rep == -1) { aps(ref ws, ss); if (fc == 'F') apc(ref ws, '0', round - (ns - exp - 1)); }
          else { aps(ref ws, ss.Slice(0, rep)); apc(ref ws, '\'', 1); aps(ref ws, ss.Slice(rep)); }
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
        apc(ref ws, fc == 'E' ? format[0] : 'E', 1);
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
      static void fra(ref Span<char> ws, NewRational.CPU cpu)
      {
        var s = cpu.sign(); int x = 0; cpu.mod(8);
        for (int i = 2; ;)
        {
          if (x + 3 > ws.Length) { cpu.pop(i); ws = default; return; }
          cpu.push(10); cpu.div(); cpu.mod(); cpu.swp();
          ws[x++] = (char)('0' + cpu.pop_int());
          if (cpu.sign() != 0) continue;
          cpu.pop(); if (--i == 0) break; ws[x++] = '/';
        }
        if (s < 0) ws[x++] = '-'; ws.Slice(0, x).Reverse(); ws = ws.Slice(x);
      }
    }
    /// <summary>
    /// Converts the representation of a number, contained in the specified read-only 
    /// span of characters to its <see cref="NewRational"/> equivalent.
    /// </summary>
    /// <remarks>
    /// Like <see cref="double.Parse(string)"/> but with unlimited number of digits.<br/>
    /// Additional: Converts Fraction notation like "1/3" or "-123/456"<br/> 
    /// Additional: Converts Repetions like "0.'3", see: <see cref="NewRational.ToString(string?, IFormatProvider?)"/>.<br/>
    /// Additional: Accepts prefix "0x" for hexadecimal- and "0b" for binary number representations. 
    /// Ignores spaces, underscores (_) leading dots (…)
    /// </remarks>
    /// <param name="value">A read-only span of characters that contains the number to convert.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about value.</param>
    /// <returns>A value that is equivalent to the number specified in the value parameter.</returns>
    public static NewRational Parse(ReadOnlySpan<char> value, IFormatProvider? provider = null)
    {
      var info = provider != null ? NumberFormatInfo.GetInstance(provider) : null;
      int a = 0, ba = 10; for (; a < value.Length - 1;)
      {
        var c = value[a]; if (c <= ' ') { a++; continue; }
        if (c != '0') break; c = (char)(value[a + 1] | 0x20);
        if (c == 'x') { ba = 16; a++; break; }
        if (c == 'b') { ba = 02; a++; break; }
        break;
      }
      var cpu = task_cpu;
      cpu.push(); cpu.push(); cpu.push(); cpu.push(1); // rat p = 0, a = 0, b = 0, e = 1;
      for (int i = value.Length - 1; i >= a; i--)
      {
        var c = value[i];
        if (c >= '0' && c <= '9' || (ba == 16 && (c | 0x20) >= 'a' && (c | 0x20) <= 'f'))
        {
          if (c > '9') c = (char)((c | 0x20) + ('9' + 1 - 'a'));
          if (c != '0') { cpu.push(c - '0'); cpu.mul(0, 1); cpu.add(3, 0); cpu.pop(); } // a += (c - '0') * e;
          if (i > a) { cpu.push(ba); cpu.mul(); }
          continue; // e *= ba;
        }
        if (c == '.' || c == ',')
        {
          if (info != null && info.NumberGroupSeparator[0] == c) continue;
          cpu.dup(); cpu.swp(2); cpu.pop(); continue; // b = e; 
        }
        if (c == '+') continue;
        if (c == '-') { cpu.neg(2); continue; } // a = -a;
        if ((c | 0x20) == 'e')
        {
          cpu.pop(2); cpu.pow(10, cpu.pop_int()); cpu.swp(); // p = pow10(a); 
          cpu.push(); cpu.push(1); continue; // b = 0; e = 1;
        }
        if (c == '\'')
        {
          cpu.dup(); cpu.dup(); cpu.push(1); cpu.sub(); // a *= e / (e - 1);
          cpu.div(); cpu.mul(3, 0); cpu.pop(); continue;
        }
        if (c == '/') // 1 / 3    
        {
          //if (cpu.sig(1) != 0) throw new FormatException();
          cpu.swp(1, 2); cpu.pop(); cpu.push(1); continue;
        }
        continue;
      }
      cpu.pop(); if (cpu.sign() != 0) cpu.div(); else cpu.pop(); // if (b.sign != 0) a /= b;
      cpu.swp(); if (cpu.sign() != 0) cpu.mul(); else cpu.pop(); // if (p.sign != 0) a *= p; 
      return cpu.pop_rat();
    }
    /// <summary>
    /// Copies the value of this <see cref="NewRational"/> as little-endian twos-complement bytes.<br/>
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
    /// Initializes a new instance of the <see cref="NewRational"/> structure using
    /// the values in a read-only span of bytes.
    /// </summary>
    /// <remarks>
    /// Reverse function of <see cref="NewRational.WriteToBytes"/> 
    /// </remarks>
    /// <param name="value">A read-only span of bytes representing the <see cref="NewRational"/> number.</param>
    /// <returns>A <see cref="NewRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    /// <exception cref="FormatException">Signals an invalid format.</exception>
    public static NewRational ReadFromBytes(ref ReadOnlySpan<byte> value)
    {
      if (value.Length < 4) throw new FormatException();
      fixed (byte* s = &value.GetPinnableReference())
      {
        if (*(uint*)s == 0) { value = value.Slice(4); return default; }
        if (value.Length < 16) throw new FormatException();
        var n = len((uint*)s); var p = new uint[n];
        fixed (uint* d = p) copy(d, (uint*)s, n);
        value = value.Slice((int)n << 2); return new NewRational(p);
      }
    }
    /// <summary>
    /// Returns the hash code for the current <see cref="NewRational"/> number.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override readonly int GetHashCode()
    {
      if (this.p == null) return 0x00280081;
      uint h = 0; Debug.Assert((p[0] & F.Norm) == 0);
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
    /// true if the obj argument is a <see cref="NewRational"/> number, 
    /// and its value is equal to the value of the current <see cref="NewRational"/> instance; 
    /// otherwise, false.
    /// </returns>
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is NewRational r && Equals(r);
    }
    /// <summary>
    /// Returns a value that indicates whether the current instance and a specified <see cref="NewRational"/> number have the same value.
    /// </summary>
    /// <param name="b">The object to compare.</param>
    /// <returns>true if this <see cref="NewRational"/> number and other have the same value; otherwise, false.</returns>
    public readonly bool Equals(NewRational b)
    {
      return task_cpu.equ(this, b);
    }
    /// <summary>
    /// Compares this instance to a second <see cref="NewRational"/> and returns an
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
    public readonly int CompareTo(NewRational b)
    {
      return task_cpu.cmp(this, b);
    }
    /// <summary>
    /// Compares this instance to a second <see cref = "System.Int32" /> and returns an
    /// integer that indicates whether the value of this instance is less than, equal
    /// to, or greater than the value of the specified object.
    /// </summary>
    /// <remarks>
    /// Fast shortcut for the most common comparisons like: <c>x == 0; x &lt;= 1; x != -1;</c> etc.
    /// </remarks>
    /// <param name="b"><see cref="int"/> value to compare.</param>
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
      var cpu = task_cpu; cpu.push(b); var s = -cpu.cmp(this); cpu.pop(); return s;
    }

    /// <summary>
    /// Defines an explicit bit-exact conversion of a <see cref="float"/> value to a <see cref="NewRational"/> value.
    /// </summary>
    /// <remarks>
    /// This as alternative to the explicit <see cref="float"/> conversion.<br/>
    /// <c>var x = new <see cref="NewRational"/>(0.1f);</c><br/>
    /// results in <c>0.100000001490116119384765625</c> as bit-exact interpretation.<br/>
    /// where the more common explicit conversion:<br/>
    /// <c>var x = (<see cref="NewRational"/>)0.1f;</c><br/>
    /// results in <c>0.1</c> as this function implicitly rounds to the precision of significant bits of <see cref="float"/>.
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public NewRational(float value)
    {
      var cpu = task_cpu; cpu.push(value, true); p = cpu.pop_rat().p;
    }
    /// <summary>
    /// Defines an explicit bit-exact conversion of a <see cref="double"/> value to a <see cref="NewRational"/> value.
    /// </summary>
    /// <remarks>
    /// This as alternative to the explicit <see cref="double"/> conversion.<br/>
    /// <c>var x = new <see cref="NewRational"/>(0.1);</c><br/>
    /// results in <c>0.10000000000000000555111512312578…</c> as bit-exact interpretation.<br/>
    /// where the more common explicit conversion:<br/>
    /// <c>var x = (<see cref="NewRational"/>)0.1;</c><br/>
    /// results in <c>0.1</c> as this function implicitly rounds to the precision of significant bits of <see cref="double"/>.
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public NewRational(double v)
    {
      var cpu = task_cpu; cpu.push(v, true); p = cpu.pop_rat().p;
    }

    /// <summary>
    /// Defines an implicit conversion of a <see cref="int"/> object to a <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator NewRational(int value)
    {
      if (value == 0) return default;
      var cpu = task_cpu; cpu.push(value); return cpu.pop_rat();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="uint"/> object to a <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator NewRational(uint value)
    {
      if (value == 0) return default;
      var cpu = task_cpu; cpu.push(value); return cpu.pop_rat();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="long"/> object to a <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator NewRational(long value)
    {
      if (value == 0) return default;
      var cpu = task_cpu; cpu.push(value); return cpu.pop_rat();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="ulong"/> object to a <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator NewRational(ulong value)
    {
      if (value == 0) return default;
      var cpu = task_cpu; cpu.push(value); return cpu.pop_rat();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="float"/> object to a <see cref="NewRational"/> value.
    /// </summary>
    /// <remarks>
    /// This function implicitly rounds to the precision of significant bits of <see cref="float"/>.<br/>
    /// This rounding is identical to the <see cref="decimal"/> rounding of <see cref="float"/> values during conversion.
    /// For a bit-exact conversion see: <see cref="NewRational(float)"/>
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is rounded to the number specified in the value parameter.</returns>
    public static implicit operator NewRational(System.Single value)
    {
      if (value == 0) return default;
      var cpu = task_cpu; cpu.push(value); return cpu.pop_rat();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="double"/> object to a <see cref="NewRational"/> value.
    /// </summary>
    /// <remarks>
    /// This function implicitly rounds to the precision of significant bits of <see cref="double"/>.<br/>
    /// This rounding is identical to the <see cref="decimal"/> rounding of <see cref="double"/> values during conversion.
    /// For a bit-exact conversion see: <see cref="NewRational(System.Double)"/>
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is rounded to the number specified in the value parameter.</returns>
    public static implicit operator NewRational(double value)
    {
      if (value == 0) return default;
      var cpu = task_cpu; cpu.push(value); return cpu.pop_rat();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="decimal"/> object to a <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator NewRational(decimal value)
    {
      var cpu = task_cpu; cpu.push(value); return cpu.pop_rat();
    }
    /// <summary>
    /// Defines an implicit conversion of a <see cref="BigInteger"/> object to a <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="NewRational"/>.</param>
    /// <returns>A <see cref="NewRational"/> number that is equivalent to the number specified in the value parameter.</returns>
    public static implicit operator NewRational(BigInteger value)
    {
      var cpu = task_cpu; cpu.push(value); return cpu.pop_rat();
    }

    /// <summary>
    /// Defines an explicit conversion of a <see cref="NewRational"/> number to a <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="int"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="int"/>.</returns>
    public static explicit operator int(NewRational value)
    {
      return (int)(double)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="NewRational"/> number to a <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="long"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="long"/>.</returns>
    public static explicit operator long(NewRational value)
    {
      return (long)(decimal)value;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="NewRational"/> number to a <see cref="float"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="float"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="float"/>.</returns>
    public static explicit operator float(NewRational value)
    {
      return (float)(double)value; //todo: fast float convert
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="NewRational"/> number to a <see cref="double"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="double"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="double"/>.</returns>
    public static explicit operator double(NewRational value)
    {
      if (value.p == null) return 0;
      fixed (uint* a = value.p)
      {
        var na = a[0] & F.Mask; var b = a + (na + 1); var nb = b[0];
        var ca = BitOperations.LeadingZeroCount(a[na]);
        var cb = BitOperations.LeadingZeroCount(b[nb]);
        var va = ((ulong)a[na] << 32 + ca) | (na < 2 ? 0 : ((ulong)a[na - 1] << ca) | (na < 3 ? 0 : (ulong)a[na - 2] >> 32 - ca));
        var vb = ((ulong)b[nb] << 32 + cb) | (nb < 2 ? 0 : ((ulong)b[nb - 1] << cb) | (nb < 3 ? 0 : (ulong)b[nb - 2] >> 32 - cb));
        if (vb == 0) return double.NaN;
        var e = ((na << 5) - ca) - ((nb << 5) - cb);
        if (e < -1021) return double.NegativeInfinity;
        if (e > +1023) return double.PositiveInfinity;
        var r = (double)(va >> 11) / (vb >> 11);
        var x = (0x3ff + e) << 52; r *= *(double*)&x; // r *= Math.Pow(2, e);
        if ((a[0] & F.Sign) != 0) r = -r; return r;
      }
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="NewRational"/> number to a <see cref="decimal"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="decimal"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="decimal"/>.</returns>
    public static explicit operator decimal(NewRational value)
    {
      if (value.p == null) return default;
      fixed (uint* p = value.p)
      {
        var d = dec(p, out var s) / dec(p + (p[0] & F.Mask) + 1, out var t);
        if (s != t) d *= new decimal(Math.Pow(2, s - t)); return d;
      }
      static decimal dec(uint* p, out int e)
      {
        var n = p[0] & F.Mask; var c = n < 4 ? n : 4;
        var t = stackalloc uint[5] { c, 0, 0, 0, 0 }; copy(t + 1, p + 1 + (n - c), c);
        e = (int)(n << 5) - BitOperations.LeadingZeroCount(p[n]) - 96;
        if (e <= 0) e = 0; else CPU.shr(t, e - ((int)(n - c) << 5));
        return new decimal((int)t[1], (int)t[2], (int)t[3], (p[0] & F.Sign) != 0, 0);
      }
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="NewRational"/> number to a <see cref="BigInteger"/> value.
    /// </summary>
    /// <remarks>
    /// The result is truncated to integer by default.<br/>
    /// Consider rounding the value before using one of the <see cref="NewRational.Round"/> methods.
    /// </remarks>
    /// <param name="value">The value to convert to a <see cref="BigInteger"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="BigInteger"/>.</returns>
    public static explicit operator BigInteger(NewRational value)
    {
      if (value.p == null) return default;
      fixed (uint* p = value.p)
      {
        var n = p[0] & F.Mask; var b = p + (n + 1);
        var r = new BigInteger(new ReadOnlySpan<byte>((byte*)(p + 1), (int)n << 2), true);
        if (*(ulong*)b != 0x100000001) r /= new BigInteger(new ReadOnlySpan<byte>(b + 1, (int)b[0] << 2), true);
        return (p[0] & F.Sign) == 0 ? r : -r;
      }
    }

    /// <summary>
    /// Returns the value of the <see cref="NewRational"/> operand. (The sign of the operand is unchanged.)
    /// </summary>
    /// <param name="a">An <see cref="NewRational"/> value.</param>
    /// <returns>The value of the value operand.</returns>
    public static NewRational operator +(NewRational a)
    {
      return a;
    }
    /// <summary>
    /// Negates a specified <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="a">The value to negate.</param>
    /// <returns>The result of the value parameter multiplied by negative one (-1).</returns>
    public static NewRational operator -(NewRational a)
    {
      var cpu = task_cpu; cpu.push(a); cpu.neg(); return cpu.pop_rat();
    }
    /// <summary>
    /// Adds the values of two specified <see cref="NewRational"/> numbers.
    /// </summary>
    /// <param name="a">The first value to add.</param>
    /// <param name="b">The second value to add.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static NewRational operator +(NewRational a, NewRational b)
    {
      var cpu = task_cpu; cpu.add(a, b); return cpu.pop_rat();
    }
    /// <summary>
    /// Subtracts a <see cref="NewRational"/> value from another <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="a">The value to subtract from (the minuend).</param>
    /// <param name="b">The value to subtract (the subtrahend).</param>
    /// <returns>The result of subtracting b from a.</returns>
    public static NewRational operator -(NewRational a, NewRational b)
    {
      var cpu = task_cpu; cpu.sub(a, b); return cpu.pop_rat();
    }
    /// <summary>
    /// Multiplies two specified <see cref="NewRational"/> values.
    /// </summary>
    /// <param name="a">The first value to multiply.</param>
    /// <param name="b">The second value to multiply.</param>
    /// <returns>The product of left and right.</returns>
    public static NewRational operator *(NewRational a, NewRational b)
    {
      var cpu = task_cpu; cpu.mul(a, b); return cpu.pop_rat();
    }
    /// <summary>
    /// Divides a specified <see cref="NewRational"/> value by another specified <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (devisor)</param>
    /// <returns>The result of the division.</returns>
    /// <exception cref="DivideByZeroException">divisor is 0 (zero).</exception>
    public static NewRational operator /(NewRational a, NewRational b)
    {
      if (b.p == null) throw new DivideByZeroException(nameof(b));
      var cpu = task_cpu; cpu.div(a, b); return cpu.pop_rat();
    }
    /// <summary>
    /// Returns the remainder that results from division with two specified <see cref="NewRational"/> values.
    /// </summary>
    /// <remarks>
    /// For fractions, the same rules apply to modulus as to floating point types such as <see cref="double"/><br/>
    /// where <c>a % b</c> is defined as: <c>a - Truncate(a / b) * b</c>
    /// </remarks>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (divisor)</param>
    /// <returns>The remainder that results from the division.</returns>
    public static NewRational operator %(NewRational a, NewRational b)
    {
      //return a - Truncate(a / b) * b;
      if (b.p == null) throw new DivideByZeroException(nameof(b));
      var cpu = task_cpu; //todo: % optimization for integers
      cpu.div(a, b); cpu.mod(); cpu.swp(); cpu.pop();
      cpu.mul(b); cpu.neg(); cpu.add(a);
      return cpu.pop_rat();
    }

    /// <summary>
    /// Returns a value that indicates whether the values of two <see cref="NewRational"/> numbers are equal.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if the left and right parameters have the same value; otherwise, false.</returns>
    public static bool operator ==(NewRational a, NewRational b)
    {
      return a.Equals(b);
    }
    /// <summary>
    /// Returns a value that indicates whether two <see cref="NewRational"/> numbers have different values.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left and right are not equal; otherwise, false.</returns>
    public static bool operator !=(NewRational a, NewRational b)
    {
      return !a.Equals(b);
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value is
    /// less than or equal to another <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is less than or equal to right; otherwise, false.</returns>
    public static bool operator <=(NewRational a, NewRational b)
    {
      return a.CompareTo(b) <= 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value is
    /// greater than or equal to another <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is greater than or equal to right; otherwise, false.</returns>
    public static bool operator >=(NewRational a, NewRational b)
    {
      return a.CompareTo(b) >= 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value is
    /// less than another <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is less than right; otherwise, false.</returns>
    public static bool operator <(NewRational a, NewRational b)
    {
      return a.CompareTo(b) < 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value is
    /// greater than another <see cref="NewRational"/> value.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is greater than right; otherwise, false.</returns>
    public static bool operator >(NewRational a, NewRational b)
    {
      return a.CompareTo(b) > 0;
    }

    #region integer operator
    /// <summary>
    /// Adds the values of a specified <see cref="NewRational"/> and a <see cref="long"/> number.
    /// </summary>
    /// <remarks>
    /// Fast addition of common cases like: <c>x + 1</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to add.</param>
    /// <param name="b">The second value to add.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static NewRational operator +(NewRational a, long b)
    {
      var cpu = task_cpu; cpu.push(b); cpu.add(a); return cpu.pop_rat();
    }
    /// <summary>
    /// Subtracts the values of a specified <see cref="NewRational"/> from another <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// Fast subtraction of common cases like: <c>x - 1</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The value to subtract from (the minuend).</param>
    /// <param name="b">The value to subtract (the subtrahend).</param>
    /// <returns>The result of subtracting b from a.</returns>
    public static NewRational operator -(NewRational a, long b)
    {
      return a + -b; // var cpu = task_cpu; cpu.push(-b); cpu.add(a); return cpu.pop_rat();
    }
    /// <summary>
    /// Multiplies the values of a specified <see cref="NewRational"/> and a <see cref="long"/> number.
    /// </summary>
    /// <remarks>
    /// Fast multiplication of common cases like: <c>x * 2</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to multiply.</param>
    /// <param name="b">The second value to multiply.</param>
    /// <returns>The product of left and right.</returns>
    public static NewRational operator *(NewRational a, long b)
    {
      var cpu = task_cpu; cpu.push(b); cpu.mul(a); return cpu.pop_rat();
    }
    /// <summary>
    /// Divides the values of a specified <see cref="NewRational"/> and a <see cref="long"/> number.
    /// </summary>
    /// <remarks>
    /// Fast divison of common cases like: <c>x / 2</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (devisor)</param>
    /// <returns>The result of the division.</returns>
    /// <exception cref="DivideByZeroException">divisor is 0 (zero).</exception>
    public static NewRational operator /(NewRational a, long b)
    {
      if (b == 0) throw new DivideByZeroException(nameof(b));
      var cpu = task_cpu; cpu.push(b); cpu.div(a, 0); cpu.swp(); cpu.pop(); return cpu.pop_rat();
    }
    /// <summary>
    /// Adds the values of a specified <see cref="long"/> and a <see cref="NewRational"/> number.
    /// </summary>
    /// <remarks>
    /// Fast addition of common cases like: <c>1 + x</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to add.</param>
    /// <param name="b">The second value to add.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static NewRational operator +(long a, NewRational b)
    {
      return b + a; // var cpu = task_cpu; cpu.push(a); cpu.add(b); return cpu.pop_rat();
    }
    /// <summary>
    /// Subtracts the values of a specified <see cref="long"/> from another <see cref="NewRational"/> value.
    /// </summary>
    /// <remarks>
    /// Fast subtraction of common cases like: <c>1 - x</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The value to subtract from (the minuend).</param>
    /// <param name="b">The value to subtract (the subtrahend).</param>
    /// <returns>The result of subtracting b from a.</returns>
    public static NewRational operator -(long a, NewRational b)
    {
      var cpu = task_cpu; cpu.push(a); cpu.push(b); cpu.sub(); return cpu.pop_rat();
    }
    /// <summary>
    /// Multiplies the values of a specified <see cref="long"/> and a <see cref="NewRational"/> number.
    /// </summary>
    /// <remarks>
    /// Fast multiplication of common cases like: <c>2 * x</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to multiply.</param>
    /// <param name="b">The second value to multiply.</param>
    /// <returns>The product of left and right.</returns>
    public static NewRational operator *(long a, NewRational b)
    {
      return b * a; //var cpu = task_cpu; cpu.push(a); cpu.mul(b); return cpu.pop_rat();
    }
    /// <summary>
    /// Divides the values of a specified <see cref="long"/> and a <see cref="NewRational"/> number.
    /// </summary>
    /// <remarks>
    /// Fast divison of common cases like: <c>2 / x</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The value to be divided. (dividend)</param>
    /// <param name="b">The value to divide by. (devisor)</param>
    /// <returns>The result of the division.</returns>
    /// <exception cref="DivideByZeroException">divisor is 0 (zero).</exception>
    public static NewRational operator /(long a, NewRational b)
    {
      if (b.p == null) throw new DivideByZeroException(nameof(b));
      var cpu = task_cpu; cpu.push(a); cpu.push(b); cpu.div(); return cpu.pop_rat();
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value and
    /// an <see cref="long"/> value are equal.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x == 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems. 
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if the left and right parameters have the same value; otherwise, false.</returns>
    public static bool operator ==(NewRational a, long b)
    {
      return a.CompareTo(b) == 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value and
    /// an <see cref="long"/> value are not equal.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x != 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left and right are not equal; otherwise, false.</returns>
    public static bool operator !=(NewRational a, long b)
    {
      return a.CompareTo(b) != 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value is
    /// less than or equal to an <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x &lt;= 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is less than or equal to right; otherwise, false.</returns>
    public static bool operator <=(NewRational a, long b)
    {
      return a.CompareTo(b) <= 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value is
    /// greater than or equal to an <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x &gt;= 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is greater than or equal to right; otherwise, false.</returns>
    public static bool operator >=(NewRational a, long b)
    {
      return a.CompareTo(b) >= 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value is
    /// less than an <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x &lt; 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is less than right; otherwise, false.</returns>
    public static bool operator <(NewRational a, long b)
    {
      return a.CompareTo(b) < 0;
    }
    /// <summary>
    /// Returns a value that indicates whether a <see cref="NewRational"/> value is
    /// greater than an <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// For fast comparisons of common cases like: <c>x &gt; 0</c>.<br/>
    /// Note that all smaller signed and unsigned integer types are automatically mapped to this operator, which is efficient on 64-bit systems.
    /// </remarks>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left is greater than right; otherwise, false.</returns>
    public static bool operator >(NewRational a, long b)
    {
      return a.CompareTo(b) > 0;
    }
    #endregion

    /// <summary>
    /// Gets a <see cref="int"/> number that indicates the sign 
    /// (negative, positive, or zero) of a <see cref="NewRational"/> number.
    /// </summary>
    /// <param name="a">A <see cref="NewRational"/> number.</param>
    /// <returns>
    /// A <see cref="int"/> that indicates the sign of the <see cref="NewRational"/> number, as
    /// shown in the following table.<br/>
    /// Number – Description<br/>
    /// -1 – The value of the object is negative.<br/>
    ///  0 – The value of the object is 0 (zero).<br/>
    /// +1 – The value of the object is positive.<br/>
    /// </returns>
    public static int Sign(NewRational a)
    {
      return a.p == null ? 0 :
        (a.p[0] & F.Sign) != 0 ? -1 :
        (a.p[0] & F.Mask) == 1 && a.p[1] == 0 ? 0 : +1; //debug view 
    }
    /// <summary>
    /// Gets the integer base 10 logarithm of a <see cref="NewRational"/> number.
    /// </summary>
    /// <remarks>
    /// The integer base 10 logarithm is identical with the exponent in the scientific notation of the number.<br/> 
    /// eg. <b>3</b> for 1000 (1E+<b>03</b>) or <b>-3</b> for 0.00123 (1.23E-<b>03</b>)
    /// </remarks>
    /// <param name="a">A <see cref="NewRational"/> number as value.</param>
    /// <returns>The integer base 10 logarithm of the <see cref="NewRational"/> number.</returns>
    public static int ILog10(NewRational a)
    {
      var cpu = task_cpu; cpu.push(a);
      cpu.tos(default, out _, out var e, out _, false); return e;
    }
    /// <summary>
    /// Gets the absolute value of a <see cref="NewRational"/> number.
    /// </summary>
    /// <param name="a">A <see cref="NewRational"/> number as value.</param>
    /// <returns>The absolute value of the <see cref="NewRational"/> number.</returns>
    public static NewRational Abs(NewRational a)
    {
      return Sign(a) >= 0 ? a : -a;
    }
    /// <summary>
    /// Returns the larger of two <see cref="NewRational"/> numbers.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>The a or b parameter, whichever is larger.</returns>
    public static NewRational Min(NewRational a, NewRational b)
    {
      return a.CompareTo(b) <= 0 ? a : b;
    }
    /// <summary>
    /// Returns the smaller of two <see cref="NewRational"/> numbers.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>The a or b parameter, whichever is smaller.</returns>
    public static NewRational Max(NewRational a, NewRational b)
    {
      return a.CompareTo(b) >= 0 ? a : b;
    }
    /// <summary>
    /// Calculates the integral part of the <see cref="NewRational"/> number.
    /// </summary>
    /// <param name="a">A <see cref="NewRational"/> number as value to truncate.</param>
    /// <returns>
    /// The integral part of the <see cref="NewRational"/> number.<br/> 
    /// This is the number that remains after any fractional digits have been discarded.
    /// </returns>
    public static NewRational Truncate(NewRational a)
    {
      var cpu = task_cpu; cpu.push(a);
      cpu.rnd(0, 0); return cpu.pop_rat();
    }
    /// <summary>
    /// Rounds a specified <see cref="NewRational"/> number to the closest integer toward negative infinity.
    /// </summary>
    /// The <see cref="NewRational"/> number to round.
    /// <returns>
    /// If <paramref name="a"/> has a fractional part, the next whole number toward negative
    /// infinity that is less than <paramref name="a"/>.<br/>
    /// or if <paramref name="a"/> doesn't have a fractional part, <paramref name="a"/> is returned unchanged.<br/>
    /// </returns>
    public static NewRational Floor(NewRational a)
    {
      var cpu = task_cpu; cpu.push(a);
      cpu.rnd(0, cpu.sign() >= 0 ? 0 : 4); return cpu.pop_rat();
    }
    /// <summary>
    /// Returns the smallest integral value that is greater than or equal to the specified number.
    /// </summary>
    /// <param name="a">A <see cref="NewRational"/> number.</param>
    /// <returns>
    /// The smallest integral value that is greater than or equal to the <paramref name="a"/> parameter.
    /// </returns>
    public static NewRational Ceiling(NewRational a)
    {
      var cpu = task_cpu; cpu.push(a);
      cpu.rnd(0, cpu.sign() < 0 ? 0 : 4); return cpu.pop_rat();
    }
    /// <summary>
    /// Rounds a <see cref="NewRational"/> number to the nearest integral value
    /// and rounds midpoint values to the nearest even number.
    /// </summary>
    /// <param name="a">A <see cref="NewRational"/> number to be rounded.</param>
    /// <returns></returns>
    public static NewRational Round(NewRational a)
    {
      var cpu = task_cpu; cpu.push(a);
      cpu.rnd(0, 1); return cpu.pop_rat();
    }
    /// <summary>
    /// Rounds a <see cref="NewRational"/> number to a specified number of fractional
    /// digits, and rounds midpoint values to the nearest even number. 
    /// </summary>
    /// <param name="a">A <see cref="NewRational"/> number to be rounded.</param>
    /// <param name="digits">The number of fractional digits in the return value.</param>
    /// <returns>The <see cref="NewRational"/> number nearest to value that contains a number of fractional digits equal to digits.</returns>
    public static NewRational Round(NewRational a, int digits)
    {
      //var e = Pow10(digits); return Round(a * e) / e;
      var cpu = task_cpu; cpu.push(a);
      cpu.rnd(digits); return cpu.pop_rat();
    }
    /// <summary>
    /// Rounds a <see cref="NewRational"/> number to a specified number of fractional digits 
    /// using the specified rounding convention.
    /// </summary>
    /// <param name="a">A <see cref="NewRational"/> number to be rounded.</param>
    /// <param name="digits">The number of decimal places in the return value.</param>
    /// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
    /// <returns>
    /// The <see cref="NewRational"/> number with decimals fractional digits that the value is rounded to.<br/> 
    /// If the number has fewer fractional digits than decimals, the number is returned unchanged.
    /// </returns>
    public static NewRational Round(NewRational a, int digits, MidpointRounding mode)
    {
      int f = 1;
      switch (mode)
      {
        case MidpointRounding.ToZero: f = 0; break;
        case MidpointRounding.ToPositiveInfinity: if (Sign(a) < 0) f = 0; else f = 4; break;
        case MidpointRounding.ToNegativeInfinity: if (Sign(a) > 0) f = 0; else f = 4; break;
      }
      var cpu = task_cpu; cpu.push(a);
      cpu.rnd(digits, f); return cpu.pop_rat();
    }
    /// <summary>
    /// Returns a specified number raised to the specified power.
    /// </summary>
    /// <param name="a">A <see cref="int"/> number to be raised to a power.</param>
    /// <param name="b">A <see cref="int"/> number that specifies a power.</param>
    /// <returns>The <see cref="NewRational"/> number a raised to the power b.</returns>
    public static NewRational Pow(int a, int b)
    {
      var cpu = task_cpu; cpu.pow(a, b); return cpu.pop_rat();
    }
    /// <summary>
    /// Returns a specified number raised to the specified power.
    /// </summary>
    /// <param name="a">A <see cref="NewRational "/> number to be raised to a power.</param>
    /// <param name="b">A <see cref="int"/> number that specifies a power.</param>
    /// <returns>The <see cref="NewRational"/> number a raised to the power b.</returns>
    public static NewRational Pow(NewRational a, int b)
    {
      var cpu = task_cpu; cpu.push(a); cpu.pow(b); return cpu.pop_rat();
    }

    /// <summary>
    /// Represents a stack machine for rational arithmetics.
    /// </summary>
    public sealed partial class CPU
    {
      int i; uint[][] p;
      /// <summary>
      /// Initializes a new instance of a <see cref="CPU"/> class that
      /// has the specified initial stack capacity.
      /// </summary>
      /// <param name="capacity">The initial stack capacity that must be greater than 0 (zero).</param>
      public CPU(uint capacity = 32)
      {
        p = new uint[capacity][]; Debug.Assert(capacity > 0);
      }
      /// <summary>
      /// Returns a temporary absolute index of the current stack top
      /// from which subsequent commands can index stack entries.
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
      /// Pushes the supplied <see cref="NewRational"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(NewRational v)
      {
        if (v.p == null) { push(); return; }
        fixed (uint* a = v.p)
        {
          var n = len(a);
          fixed (uint* b = rent(n)) copy(b, a, n);
        }
      }
      /// <summary>
      /// Pushes the supplied <see cref="int"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(int v)
      {
        fixed (uint* p = rent(4))
        {
          p[0] = (v < 0 ? F.Sign : 0) | 1;
          p[1] = unchecked((uint)(v < 0 ? -v : v)); *(ulong*)(p + 2) = 0x100000001;
        }
      }
      /// <summary>
      /// Pushes n copies of the supplied <see cref="int"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      /// <param name="n">The number of copies to push.</param>
      public void push(int v, int n)
      {
        for (int i = 0; i < n; i++) push(v);  //todo: optimize
      }
      /// <summary>
      /// Pushes the supplied <see cref="uint"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(uint v)
      {
        fixed (uint* p = rent(4))
        {
          p[0] = 1; p[1] = v;
          *(ulong*)(p + 2) = 0x100000001;
        }
      }
      /// <summary>
      /// Pushes the supplied <see cref="long"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(long v)
      {
        var u = unchecked((int)v); if (u == v) { push(u); return; }
        fixed (uint* p = rent(5))
        {
          p[0] = (v < 0 ? F.Sign : 0) | 2;
          *(ulong*)(p + 1) = unchecked((ulong)(v < 0 ? -v : v));
          *(ulong*)(p + 3) = 0x100000001;
        }
      }
      /// <summary>
      /// Pushes the supplied <see cref="ulong"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(ulong v)
      {
        var u = unchecked((uint)v); if (u == v) { push(u); return; }
        fixed (uint* p = rent(5))
        {
          p[0] = 2;
          *(ulong*)(p + 1) = v;
          *(ulong*)(p + 3) = 0x100000001;
        }
      }
      /// <summary>
      /// Pushes the supplied <see cref="float"/> value onto the stack.
      /// </summary>
      /// <remarks>
      /// Rounds the value implicit to 7 digits as the <see cref="float"/> format has
      /// only 7 digits of precision.<br/> 
      /// see: <see cref="NewRational(float)"/>
      /// </remarks>
      /// <param name="v">The value to push.</param>
      public void push(float v)
      {
        if (v == 0) { push(); return; }
        var e = unchecked((byte)(*(uint*)&v >> 23) - 126); //Debug.Assert(!float.IsFinite(v) == (e == 0x81));
        if (e == 0x81) { nan(); return; }
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
      /// see: <see cref="NewRational.NewRational(double)"/>
      /// </remarks>
      /// <param name="v">The value to push.</param>
      public void push(double v)
      {
        if (v == 0) { push(); return; }
        var e = unchecked((int)((*(ulong*)&v >> 52) & 0x7FF) - 1022); //Debug.Assert(!double.IsFinite(v) == (e == 0x401));
        if (e == 0x401) { nan(); return; } //NaN 
        var p = 14 - ((e * 19728) >> 16); //-14..43
        var d = Math.Abs(v) * Math.Pow(10, p);
        if (d < 1e14) { d *= 10; p++; }
        var m = (ulong)Math.Round(d);
        push(m); pow(10, p); div(); if (v < 0) neg(); //norm();        
      }
      /// <summary>
      /// Pushes the supplied <see cref="double"/> value onto the stack
      /// using a bit-exact conversion without rounding.
      /// </summary>                                    
      /// <remarks>
      /// see: <see cref="NewRational.NewRational(double)"/>
      /// </remarks>
      /// <param name="v">The value to push.</param>
      /// <param name="exact">Should be true.</param>
      /// <exception cref="ArgumentOutOfRangeException">Value id NaN.</exception>
      public void push(double v, bool exact)
      {
        if (v == 0) { push(); return; }
        int h = ((int*)&v)[1], e = ((h >> 20) & 0x7FF) - 1075; // Debug.Assert(!double.IsFinite(v) == (e == 0x3cc));
        if (e == 0x3cc) { nan(); return; } //NaN 
        fixed (uint* p = rent((unchecked((uint)(e < 0 ? -e : e)) >> 5) + 8))
        {
          p[0] = 2; p[1] = *(uint*)&v; p[2] = (unchecked((uint)h) & 0x000FFFFF) | 0x100000;
          if (e == -1075) { if ((p[2] &= 0xfffff) == 0) if (p[p[0] = 1] == 0) { *(ulong*)(p + 2) = 0x100000001; return; } e++; } // denormalized
          if (e > 0) shl(p, e); var q = p + (p[0] + 1);
          *(ulong*)q = 0x100000001; if (e < 0) shl(q, -e);
          p[0] |= (unchecked((uint)h) & F.Sign) | F.Norm;
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
            s[0] = s[3] != 0 ? 3u : s[2] != 0 ? 2u : 1; p[0] |= F.Norm;
          }
          p[0] |= f & F.Sign;
        }
      }
      /// <summary>
      /// Pushes the supplied <see cref="BigInteger"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      public void push(BigInteger v)
      {
        var si = v.Sign; if (si == 0) { push(); return; }
        if (si < 0) v = -v; var n = v.GetByteCount(true); var c = ((n - 1) >> 2) + 1;
        fixed (uint* p = rent(unchecked((uint)(c + 3))))
        {
          p[c] = 0; v.TryWriteBytes(new Span<byte>(p + 1, n), out _, true); Debug.Assert(p[c] != 0);
          p[0] = unchecked((uint)c) | (si < 0 ? F.Sign : 0);
          *(ulong*)(p + c + 1) = 0x100000001;
        }
      }

      /// <summary>
      /// Converts the value at absolute position i on stack to a 
      /// always normalized <see cref="NewRational"/> number and returns it.
      /// </summary>
      /// <remarks>
      /// This function is helpful to read out vectors and matrices efficently.<br/>
      /// For absolute positions see: <see cref="mark"/>
      /// </remarks>
      /// <param name="i">Absolute index of the value to get.</param>
      /// <param name="v">Returns the value.</param>
      public void get(uint i, out NewRational v)
      {
        fixed (uint* p = this.p[i])
        {
          if (isz(p)) { v = default; return; }
          if ((p[0] & F.Norm) != 0) norm(p);
          var n = len(p); var a = new uint[n];
          fixed (uint* s = a) copy(s, p, n);
          v = new NewRational(a);
        }
      }
      /// <summary>
      /// Converts the value at absolute position i on stack to a 
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
        v = (double)new NewRational(p[i]);
      }
      /// <summary>
      /// Converts the value at absolute position i on stack to a 
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
        v = (float)new NewRational(p[i]);
      }

      /// <summary>
      /// Removes the value currently on top of the stack, 
      /// convert and returns it as always normalized <see cref="NewRational"/> number.<br/>
      /// </summary>
      /// <returns>A always normalized <see cref="NewRational"/> number.</returns>
      public NewRational pop_rat()
      {
        get(unchecked((uint)(i - 1)), out NewRational t); pop(); return t;
      }
      /// <summary>
      /// Removes the value currently on top of the stack, 
      /// convert and returns it as <see cref="double"/> value.<br/>
      /// </summary>
      /// <remarks>
      /// <see cref="double.PositiveInfinity"/> or <see cref="double.NegativeInfinity"/> 
      /// is returned in case of out of range as the <see cref="double"/> precision is limited. 
      /// </remarks>
      /// <returns>A <see cref="double"/> value.</returns>
      public double pop_dbl()
      {
        get(unchecked((uint)(i - 1)), out double t); pop(); return t;
      }
      /// <summary>
      /// Removes the value currently on top of the stack, 
      /// convert the numerator and returns it as <see cref="int"/> value.<br/>
      /// </summary>
      /// <remarks>
      /// Only the first 32-bit of the numerator are interpreted and retuned as integer.<br/>
      /// The function is intended for the fastest possible access to results that are always in the range of <see cref="int"/>.<br/>
      /// For example: <c>x % 10</c> results.<br/>
      /// For the case, rounding, range checks etc. should be done in previous calls.<br/>
      /// Or simple convert from double: <c>var i = (int)<see cref="pop_dbl()"/></c>  
      /// </remarks>
      /// <returns>A <see cref="int"/> value.</returns>
      public int pop_int()
      {
        fixed (uint* u = p[i - 1])
        {
          var i = unchecked((int)u[1]);
          if ((u[0] & F.Sign) != 0) i = -i;
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
      /// it is more efficient to use <see cref="swp"/> when possible.
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      public void dup(uint a)
      {
        fixed (uint* u = this.p[a])
        {
          var n = len(u);
          fixed (uint* v = rent(n)) copy(v, u, n);
        }
      }
      /// <summary>
      /// Negates the value at index <paramref name="a"/> relative to the top of the stack.<br/>
      /// Default index 0 negates the value on top of the stack.
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      public void neg(int a = 0)
      {
        neg(unchecked((uint)(this.i - 1 - a))); // fixed (uint* p = this.p[this.i - 1 - i]) if (!isz(p)) p[0] ^= F.Sign;
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
        fixed (uint* p = this.p[a]) if (!isz(p)) p[0] ^= F.Sign;
      }
      /// <summary>
      /// Convert the value at index <paramref name="a"/> relative to the top of the stack to it's absolute value.<br/>
      /// Default index 0 addresses the value on top of the stack.
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      public void abs(int a = 0)
      {
        fixed (uint* p = this.p[this.i - 1 - a]) p[0] &= ~F.Sign;
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
      /// <param name="a">A <see cref="NewRational"/> value.</param>
      public void add(NewRational a)
      {
        if (a.p == null) return;
        fixed (uint* u = p[i - 1], v = a.p) add(u, v, false);
        swp(); pop();
      }
      /// <summary>
      /// Adds the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="NewRational"/> as first value.</param>
      /// <param name="b">A <see cref="NewRational"/> as second value.</param>
      public void add(NewRational a, NewRational b)
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
      /// <param name="a">A <see cref="NewRational"/> as first value.</param>
      /// <param name="b">A <see cref="NewRational"/> as second value.</param>
      public void sub(NewRational a, NewRational b)
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
        fixed (uint* u = p[i - 2], v = p[i - 1]) mul(u, v, false); swp(0, 2); pop(2);
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
      /// <param name="a">A <see cref="NewRational"/> value.</param>
      public void mul(NewRational a)
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
      /// <param name="a">A <see cref="NewRational"/> as first value.</param>
      /// <param name="b">Absolute index of a stack entry as second value.</param>
      public void mul(NewRational a, uint b)
      {
        if (a.p == null) { push(); return; }
        fixed (uint* u = a.p, v = p[b]) mul(u, v, false);
      }
      /// <summary>
      /// Multiplies the values <paramref name="a"/> and <paramref name="b"/> and pushes the result on the stack.
      /// </summary>
      /// <param name="a">A <see cref="NewRational"/> as first value.</param>
      /// <param name="b">A <see cref="NewRational"/> as second value.</param>
      public void mul(NewRational a, NewRational b)
      {
        if (a.p == null) { push(); return; }
        if (b.p == null) { push(); return; }
        fixed (uint* u = a.p, v = b.p) mul(u, v, false);
      }
      /// <summary>
      /// Divides the first two values on top of the stack 
      /// and replaces them with the result.
      /// </summary>
      /// <remarks>
      /// a / b where b is the value on top of the stack 
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
      /// <param name="a">A <see cref="NewRational"/> as first value.</param>
      /// <param name="b">A <see cref="NewRational"/> as second value.</param>
      public void div(NewRational a, NewRational b)
      {
        if (b.p == null) { nan(); return; }
        if (a.p == null) { push(); return; }
        fixed (uint* u = a.p, v = b.p) mul(u, v, true);
      }
      /// <summary>
      /// Divides the value <paramref name="a"/> and the value at index <paramref name="b"/> relative to the top of the stack.<br/>
      /// Pushes the result on top of the stack.
      /// </summary>
      /// <param name="a">A <see cref="NewRational"/> as first value.</param>
      /// <param name="b">A relative index of a stack entry.</param>
      public void div(NewRational a, int b)
      {
        if (a.p == null) { push(); return; }
        fixed (uint* u = a.p, v = p[this.i - 1 - b]) mul(u, v, true);
      }
      /// <summary>
      /// Squares the value at index <paramref name="a"/> relative to the top of the stack.<br/>
      /// Default index 0 squares the value on top of the stack.
      /// </summary>
      /// <param name="a">Relative index of a stack entry.</param>
      public void sqr(int a = 0)
      {
        //mul(a, a); return;
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
        //mul(a, a); return;
        fixed (uint* u = this.p[a])
        fixed (uint* w = rent(len(u) << 1))
        {
          sqr(u, w); if (*(ulong*)w == 1) { *(ulong*)(w + 2) = 0x100000001; return; }
          sqr(u + ((u[0] & F.Mask) + 1), w + (w[0] + 1)); w[0] |= F.Norm;
        }
      }
      /// <summary>
      /// Replaces the value on top of the stack with it's multiplicative inverse,
      /// also called the reciprocal. <c>x = 1 / x;</c> 
      /// </summary>
      /// <remarks>
      /// It's a fast operation and should be used for 1 / x instead of a equivalent <see cref="div"/> operations.
      /// </remarks>
      public void inv()
      {
        //push(1); swp(); div();
        fixed (uint* u = p[i - 1])
        fixed (uint* v = rent(len(u)))
        {
          uint n = (u[0] & F.Mask) + 1, m = u[n] + 1;
          copy(v, u + n, m); v[0] |= u[0] & (F.Sign | F.Norm);
          copy(v + m, u, n); v[m] &= F.Mask;
          swp(); pop();
        }
      }
      /// <summary>
      /// Selects the value at index i relative to the top of the stack.<br/> 
      /// Shifts the numerator value to the left (in zeros) by the specified number of bits.
      /// </summary>
      /// <remarks>
      /// Shifts the numerator value only, which is a fast alternative to multiplies by powers of two.<br/>
      /// To shift the denominator is possible by calling <see cref="inv()"/> before and after.
      /// </remarks>
      /// <param name="c">The number of bits to shift.</param>
      /// <param name="i">A relative index of a stack entry.</param>
      public void shl(int c, int i = 0)
      {
        if (c <= 0) { Debug.Assert(c == 0); return; }
        fixed (uint* u = p[this.i - 1 - i])
        {
          if (*(ulong*)u == 1) return;
          fixed (uint* v = rent(len(u) + (unchecked((uint)c) >> 5) + 1)) //todo: optimize see shr
          {
            var n = (u[0] & F.Mask) + 1;
            copy(v, u, n); shl(v, c);
            copy(v + v[0] + 1, u + n, u[n] + 1);
            v[0] |= (u[0] & F.Sign) | F.Norm; swp(i + 1); pop();
          }
        }
      }
      /// <summary>
      /// Selects the value at index i relative to the top of the stack.<br/> 
      /// Shifts the numerator value to the right (in zeros) by the specified number of bits.
      /// </summary>
      /// <remarks>
      /// Shifts the numerator value only, which is a fast alternative to divisions by powers of two.<br/>
      /// To shift the denominator is possible by calling <see cref="inv()"/> before and after.
      /// </remarks>
      /// <param name="c">The number of bits to shift.</param>
      /// <param name="i">A relative index of a stack entry.</param>
      public void shr(int c, int i = 0)
      {
        if (c <= 0) { Debug.Assert(c == 0); return; }
        fixed (uint* p = this.p[this.i - 1 - i])
        {
          var h = p[0]; var a = h & F.Mask;
          shr(p, c); if (*(ulong*)p == 1) { *(ulong*)(p + 2) = 0x100000001; return; }
          if (p[0] != a) copy(p + p[0] + 1, p + a + 1, p[a + 1] + 1);
          p[0] |= (h & F.Sign) | F.Norm;
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
          var h = u[0]; u[0] &= F.Mask; var t = u + u[0] + 1;
          if (f != 8) div(u, t, v); else copy(v, t, t[0] + 1);
          if ((f & (1 | 2 | 4)) != 0)
          {
            if (f == 4) f = *(ulong*)u != 1 ? 1 : 0;
            else { shr(t, 1); var x = cms(u, t); f = f == 1 ? x : x + 1; }
            if (f > 0) { var w = 0x100000001; add(v, (uint*)&w, v); }
          }
          *(ulong*)(u + u[0] + 1) = *(ulong*)(v + v[0] + 1) = 0x100000001;
          if ((h & F.Sign) != 0 && *(ulong*)v != 1) v[0] |= F.Sign;
        }
      }
      /// <summary>
      /// Rounds the value at the top of the stack 
      /// to the specified number of fractional decimal digits.<br/>
      /// </summary>
      /// <param name="c">Specifies the number of decimal digits to round to.</param>
      /// <param name="f">Midpoint rounding see: <see cref="mod(int)"/>.</param>
      public void rnd(int c, int f = 1)
      {
        if (c >= 0) fixed (uint* p = this.p[this.i - 1]) if (isint(p)) return;
        if (c == 0) { mod(f); swp(); pop(); }
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
        else push(1);
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
        push(1); swp();
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
        push(1); for (uint i = 2; i <= c; i++) { push(i); mul(); }
      }
      /// <summary>
      /// Limitates the binary digits of the value at the top of the stack to the specified count <paramref name="c"/>.<br/>
      /// If the number of digits in numerator or denominator exceeds this count the bits will shifted right by the difference.
      /// </summary>
      /// <remarks>
      /// This is a fast binary rounding operation that limits precision to increase the performance.<br/>
      /// Mainly intended for iterations where the successive numeric operations internally produce much more precision than is finally needed.
      /// </remarks>
      /// <param name="c">A positive number as bit count.</param>
      public void lim(uint c)
      {
        Debug.Assert(c > 0);
        fixed (uint* p = this.p[this.i - 1])
        {
          if (isz(p)) return;
          var h = p[0]; var a = h & F.Mask; var q = p + a + 1; var b = q[0];
          var u = (a << 5) - unchecked((uint)BitOperations.LeadingZeroCount(p[a]));
          var v = (b << 5) - unchecked((uint)BitOperations.LeadingZeroCount(q[b]));
          if (u > v) u = v; if (u <= c) return;
          var t = (int)(u - c); shr(p, t); shr(q, t);
          if (p[0] != a) copy(p + p[0] + 1, q, q[0] + 1);
          p[0] |= (h & F.Sign) | F.Norm;
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
          var x = u[0]; u[0] &= ~F.Sign;
          var y = v[0]; v[0] &= ~F.Sign;
          var c = cmp(u, v); u[0] = x; v[0] = y; return c;
        }
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
      /// Compares the <see cref="NewRational"/> value <paramref name="a"/> with the <see cref="NewRational"/> value b.
      /// </summary>
      /// <param name="a">A <see cref="NewRational"/> as first value.</param>
      /// <param name="b">A <see cref="NewRational"/> as second value.</param>
      /// <returns>
      /// -1 – a is less than b.<br/>
      /// 0 – a is equal b.<br/>
      /// 1 – a is greater than b.<br/>
      /// </returns>
      public int cmp(NewRational a, NewRational b)
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
      /// Compares the value on top of the stack a with the <see cref="NewRational"/> value b.
      /// </summary>
      /// <param name="b">A <see cref="NewRational"/> as second value.</param>
      /// <returns>
      /// -1 – the value on top of the stack is less than b.<br/>
      /// 0 – the value on top of the stack is equal b.<br/>
      /// 1 – the value on top of the stack is greater than b.<br/>
      /// </returns>
      public int cmp(NewRational b)
      {
        if (b.p == null) return sign();
        fixed (uint* u = p[this.i - 1], v = b.p) return cmp(u, v);
      }
      /// <summary>
      /// Compares the <see cref="NewRational"/> value <paramref name="a"/> with the <see cref="NewRational"/> value b for equality.
      /// </summary>
      /// <param name="a">A <see cref="NewRational"/> value as first value.</param>
      /// <param name="b">A <see cref="NewRational"/> value as second value.</param>
      /// <returns>true if the values are equal, otherwise false.</returns>
      public bool equ(NewRational a, NewRational b)
      {
        if (a.p == b.p) return true;
        fixed (uint* u = a.p, v = b.p)
        {
          if (u == null) return isz(v);
          if (v == null) return isz(u);
          if (((u[0] | v[0]) & F.Norm) == 0)
          {
            var n = len(u); // for (uint i = 0; i < n; i++) if (u[i] != v[i]) return false;
            for (uint i = 0, c = n >> 1; i < c; i++)
              if (((ulong*)u)[i] != ((ulong*)v)[i])
                return false;
            if ((n & 1) != 0) return u[n - 1] == v[n - 1];
            return true;
          }
          return cmp(u, v) == 0;
        }
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
        return equ(new NewRational(p[a]), new NewRational(p[b]));
      }
      /// <summary>
      /// Compares the value at index a as absolute index in the stack with the <see cref="NewRational"/> value b for equality.
      /// </summary>
      /// <remarks>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="a">Absolute index of a stack entry.</param>
      /// <param name="b">A <see cref="NewRational"/> value as second value.</param>
      /// <returns>true if the values are equal, otherwise false.</returns>
      public bool equ(uint a, NewRational b)
      {
        return equ(new NewRational(p[a]), b);
      }
      /// <summary>
      /// Normalize the value at the specified index relative to the top of the stack.
      /// </summary>
      /// <remarks>
      /// Normalization works automatically, 
      /// on demand or for the final results returning as <see cref="NewRational"/>.<br/>
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
          if ((u[0] & F.Norm) != 0) norm(u);
      }
      /// <summary>
      /// Calculates a hash value for the value at b as absolute index in the stack.
      /// </summary>
      /// <remarks>
      /// The function is helpful for comparsions and to build dictionarys without creaton of <see cref="NewRational"/> objects.<br/>
      /// To get meaningful hash values the function forces normalizations what can be time critical.<br/>
      /// The hash value returned is identical with the hash value returned from <see cref="NewRational.GetHashCode"/> for same values.<br/>
      /// see: <see cref="mark()"/> for absolute indices. 
      /// </remarks>
      /// <param name="b">Absolute index of a stack entry.</param>
      /// <returns>A <see cref="uint"/> value as hash value.</returns>
      public uint hash(uint i)
      {
        var p = this.p[i]; if ((p[0] & F.Norm) != 0) fixed (uint* u = p) norm(u);
        return unchecked((uint)new NewRational(p).GetHashCode());
      }
      /// <summary>
      /// Returns the MSB (most significant bit) of the numerator of the value on top of the stack.
      /// </summary>
      /// <remarks>
      /// To get the MSB of the denominator is possible by calling <see cref="inv()"/> before and after.
      /// </remarks>
      /// <returns>A <see cref="uint"/> value as MSB.</returns>
      public uint msb()
      {
        fixed (uint* p = this.p[this.i - 1])
        {
          var n = p[0] & F.Mask;
          return (n << 5) - (uint)BitOperations.LeadingZeroCount(p[n]);
        }
      }
      /// <summary>
      /// Returns the LSB (least significant bit) of the numerator of the value on top of the stack.
      /// </summary>
      /// <remarks>
      /// To get the LSB of the denominator is possible by calling <see cref="inv()"/> before and after.
      /// </remarks>
      /// <returns>A <see cref="uint"/> value as MSB.</returns>
      public uint lsb()
      {
        fixed (uint* p = this.p[this.i - 1])
        {
          for (uint t = 1, n = p[0] & F.Mask; t <= n; t++)
            if (p[t] != 0)
              return ((t - 1) << 5) + (uint)BitOperations.TrailingZeroCount(p[t]) + 1;
          return 0;
        }
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
          u[0] &= F.Mask; v[0] &= F.Mask;
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
        var i1 = p[0] & F.Mask; if (i1 == 0 || i1 + 3 > p.Length) { pop(); return; } // NaN
        var i2 = p[i1 + 1]; if (i2 == 0 || i1 + i2 + 2 > p.Length) { pop(); return; } // NaN
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
          pop(2); push(10); if (d == 0) { mul(); exp--; } else { div(); exp++; }
        }
        int nr = 0; uint* rr = null; IntPtr mem = default;
        if (reps)
        {
          nr = unchecked((int)(this.p[this.i - 3][0] & F.Mask)) + 1;
          var need = sp.Length * nr;
          if (need <= 0x8000) { var t = stackalloc uint[need]; rr = t; }
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
              tt[0] &= F.Mask; Debug.Assert(tt[0] < nr);
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
          push(10); mul(); dup(); mod();
        }
        if (mem != default) Marshal.FreeCoTaskMem(mem);
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
      void nan()
      {
        fixed (uint* p = rent(4)) *(ulong*)p = *(ulong*)(p + 2) = 1;
      }
      void add(uint* u, uint* v, bool neg)
      {
        var l = len(u) + len(v);
        fixed (uint* w = rent(l * 3))
        {
          var sa = u[0] & F.Sign;
          var sb = v[0] & F.Sign; if (neg) sb ^= F.Sign;
          var ud = u + ((u[0] & F.Mask) + 1);
          var vd = v + ((v[0] & F.Mask) + 1);
          uint* s = w + (l << 1), t;
          mul(u, vd, s);
          mul(v, ud, t = s + (s[0] + 1));
          if (sa == sb) add(s, t, w);
          else
          {
            var si = cms(s, t); if (si == 0) { *(ulong*)w = 1; *(ulong*)(w + 2) = 0x100000001; return; }
            if (si < 0) { var h = s; s = t; t = h; sa ^= F.Sign; }
            sub(s, t, w);
          }
          if (*(ulong*)w == 1) { *(ulong*)(w + 2) = 0x100000001; return; }
          mul(ud, vd, w + (w[0] + 1)); w[0] |= sa | F.Norm;
        }
      }
      void mul(uint* u, uint* v, bool inv)
      {
        fixed (uint* w = rent(len(u) + len(v)))
        {
          var s = (v[0] & F.Mask) + 1;
          var t = (u[0] & F.Mask) + 1;
          mul(u + 0, inv ? v + s : v, w); if (*(ulong*)w == 1) { *(ulong*)(w + 2) = 0x100000001; return; }
          mul(u + t, inv ? v : v + s, w + (w[0] + 1));
          w[0] |= ((u[0] ^ v[0]) & F.Sign) | F.Norm;
        }
      }
      int cmp(uint* a, uint* b)
      {
        int sa = sig(a), sb = sig(b);
        if (sa != sb) return sa > sb ? +1 : -1; if (sa == 0) return 0;
        uint na, la = (na = a[0] & F.Mask) + a[na + 1];
        uint nb, lb = (nb = b[0] & F.Mask) + b[nb + 1];
        fixed (uint* u = rent(la + lb + 4))
        {
          mul(a, b + (nb + 1), u); var v = u + (u[0] + 1);
          mul(b, a + (na + 1), v); sa *= cms(u, v);
          pop(); return sa;
        }
      }
      void norm(uint* p)
      {
        Debug.Assert((p[0] & F.Norm) != 0);
        var d = p + ((p[0] & F.Mask) + 1);
        if (*(ulong*)d == 0x100000001) { p[0] &= ~F.Norm; return; }
        if (*(ulong*)d == 1) { *(ulong*)p = *(ulong*)(p + 2) = 1; return; } //NaN
        var l = len(p);
        fixed (uint* s = rent(l << 1))
        {
          copy(s, p, l); s[0] &= F.Mask;
          var e = gcd(s, s + (s[0] + 1));
          if (*(ulong*)e == 0x100000001) { p[0] &= ~F.Norm; pop(); return; }
          var t = s + l; var h = p[0];
          copy(t, p, l); t[0] &= F.Mask;
          d = t + (t[0] + 1); div(t, e, p); div(d, e, p + (p[0] + 1));
          p[0] |= (h & F.Sign); pop();
        }
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
        uint na = a[0] & F.Mask, nb = b[0] & F.Mask;
        if (na < nb) { var u = na; na = nb; nb = u; var v = a; a = b; b = v; }
        uint f = b[1];
        if (nb == 1)
        {
          if (f == 0) { *(ulong*)r = 1; return; }
          if (f == 1) { r[0] = na; for (uint i = 1; i <= na; i++) r[i] = a[i]; return; }
        }
        //if (nb >= klim) //todo: benchmarks
        //{
        //  var n = na + nb; for (uint i = 1; i <= n; i++) r[i] = 0;
        //  kmul(a + 1, na, b + 1, nb, r + 1, n);
        //  r[0] = n; while (r[r[0]] == 0) r[0]--;
        //  return;
        //}
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
        if (r[r[0] = na + nb] == 0 && r[0] > 1) { r[0]--; Debug.Assert(!(r[r[0]] == 0 && r[0] > 1)); }
      }
      static void sqr(uint* a, uint* r)
      {
        uint n = *a++ & F.Mask; var v = r + 1;
        if (n == 1)
        {
          *(ulong*)v = (ulong)a[0] * a[0];
          r[0] = r[2] != 0 ? 2u : 1u; return;
        }
        for (uint i = 0; i < n; i++)
        {
          ulong c = 0;
          for (uint j = 0; j < i; j++)
          {
            ulong e = v[i + j] + c, f = (ulong)a[j] * a[i];
            v[i + j] = unchecked((uint)(e + (f << 1)));
            c = (f + (e >> 1)) >> 31;
          }
          *(ulong*)(v + (i << 1)) = (ulong)a[i] * a[i] + c;
        }
        if (r[r[0] = n << 1] == 0 && r[0] > 1) { r[0]--; Debug.Assert(!(r[r[0]] == 0 && r[0] > 1)); }
      }
      static void div(uint* a, uint* b, uint* r)
      {
        uint na = a[0], nb = b[0] & F.Mask; if (r != null) *(ulong*)r = 1;
        if (na < nb) return;
        if (na == 1)
        {
          uint q = a[1] / b[1], m = a[1] - (q * b[1]);
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
        for (uint i = na; i >= nb; i--)
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
          ulong di = vh / dh;
          if (di > 0xFFFFFFFF) di = 0xFFFFFFFF;
          for (; ; )
          {
            ulong th = dh * di;
            ulong tl = dl * di;
            th = th + (tl >> 32);
            tl = tl & 0xFFFFFFFF;
            if (th < vh) break; if (th > vh) { di--; continue; }
            if (tl < vl) break; if (tl > vl) { di--; continue; }
            break;
          }
          if (di > 0)
          {
            ulong c = 0; var p = a + n;
            for (int k = 1; k <= nb; k++)
            {
              c += b[k] * di; uint d = unchecked((uint)c);
              c = c >> 32; if (p[k] < d) ++c;
              p[k] = unchecked(p[k] - d);
            }
            if (unchecked((uint)c) != t)
            {
              c = 0; p = a + n; di--;
              for (int k = 1; k <= nb; k++)
              {
                ulong d = (p[k] + c) + b[k];
                p[k] = unchecked((uint)d); c = d >> 32;
              }
            }
          }
          if (r != null && di != 0)
          {
            var x = n + 1; for (var j = r[0] + 1; j < x; j++) r[j] = 0;
            if (r[0] < x) r[0] = x; r[x] = unchecked((uint)di);
          }
          if (i < na) a[i + 1] = 0;
        }
        for (; a[0] > 1 && a[a[0]] == 0; a[0]--) ;
      }
      static void shl(uint* p, int c)
      {
        var s = c & 31; uint d = (uint)c >> 5, n = p[0] & F.Mask; p[0] = p[n + 1] = 0;
        for (int i = (int)n + 1; i > 0; i--) p[i + d] = (p[i] << s) | unchecked((uint)((ulong)p[i - 1] >> (32 - s)));
        for (int i = 1; i <= d; i++) p[i] = 0;
        n += d; p[0] = p[n + 1] != 0 ? n + 1 : n;
      }
      internal static void shr(uint* p, int c)
      {
        int s = c & 31, r = 32 - s;
        uint n = p[0] & F.Mask, i = 0, k = 1 + unchecked((uint)c >> 5), l = k <= n ? p[k++] >> s : 0;
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
        Debug.Assert((*(ulong*)p & ~(ulong)(F.Norm | F.Sign)) != 1 || *(ulong*)p == 1);
        return *(ulong*)p == 1; //return (*(ulong*)p & ~(ulong)F.Norm) == 1;
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      static bool isint(uint* p)
      {
        return *(ulong*)(p + ((p[0] & F.Mask) + 1)) == 0x100000001;
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      static int sig(uint* p)
      {
        return (p[0] & F.Sign) != 0 ? -1 : isz(p) ? 0 : +1;
      }
    }

    /// <summary>
    /// Thread static instance of a <see cref="CPU"/> for general use.
    /// </summary>
    public static CPU task_cpu
    {
      get { return _cpu ??= new CPU(); }
    }

    #region private 
    [ThreadStatic] private static CPU? _cpu;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    NewRational(uint[] p) { this.p = p; }
    struct F { internal const uint Mask = 0x3fffffff, Sign = 0x80000000, Norm = 0x40000000; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint len(uint* p)
    {
      var n = p[0] & F.Mask;
      return n + p[n + 1] + 2;
    }
    static void copy(uint* d, uint* s, uint n)
    {
      for (uint i = 0, c = n >> 1; i < c; i++) ((ulong*)d)[i] = ((ulong*)s)[i];
      if ((n & 1) != 0) d[n - 1] = s[n - 1];
    }
    #endregion
    #region debug support
    [DebuggerTypeProxy(typeof(DebugView)), DebuggerDisplay("Count = {i}")]
    partial class CPU
    {
      sealed class DebugView
      {
        CPU p; public DebugView(CPU p) { this.p = p; }
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public NewRational[] Items
        {
          get
          {
            var a = new NewRational[p.i];
            for (int i = 0; i < p.i; i++)
              a[i] = new NewRational(p.p[p.i - 1 - i]);
            return a;
          }
        }
      }
      #region experimental
#if false
      public void cut(int c, int i = 0)
      {
        Debug.Assert(c >= 0);
        fixed (uint* p = this.p[this.i - 1 - i])
        {
          var h = p[0]; var n = h & F.Mask;
          var x = unchecked(((uint)c >> 5) + 1); if (x > n) return;
          p[x] &= unchecked((uint)((1 << (c & 31)) - 1)); for (; x > 1 && p[x] == 0; x--) ;
          if (n != x) { copy(p + (x + 1), p + (x + 1) + (n - x), p[n + 1] + (x + 1)); p[0] = x; }
          if (*(ulong*)p == 1) { *(ulong*)(p + 2) = 0x100000001; return; }
          p[0] |= (h & F.Sign) | F.Norm;
        }
      }
      const uint klim = 16;
      static void kmul(uint* a, uint na, uint* b, uint nb, uint* r, uint nr)
      {
        Debug.Assert(na >= nb && nr == na + nb);
        if (nb < klim)
        {
          for (uint i = 0; i < nb; i++)
          {
            ulong c = 0, d;
            for (uint k = 0; k < na; k++, c = d >> 32) r[i + k] = unchecked((uint)(d = r[i + k] + c + (ulong)a[k] * b[i]));
            r[i + na] = (uint)c;
          }
          return;
        }
        uint n = nb >> 1, m = n << 1;
        kmul(a, n, b, n, r, m); // p1 = al * bl
        kmul(a + n, na - n, b + n, nb - n, r + m, nr - m); // p2 = ah * bh
        uint r1 = na - n + 1, r2 = nb - n + 1, r3 = r1 + r2, r4 = r1 + r2 + r3;
        uint* t1 = stackalloc uint[(int)r4], t2 = t1 + r1, t3 = t2 + r2; for (uint i = 0; i < r4; i++) t1[i] = 0;
        add(a + n, na - n, a, n, t1, r1);
        add(b + n, nb - n, b, n, t2, r2);
        kmul(t1, r1, t2, r2, t3, r3); // p3 = (ah + al) * (bh + bl)
        sub(r + m, nr - m, r, m, t3, r3);
        ada(r + n, nr - n, t3, r3); // (p2 << m) + ((p3 - (p1 + p2)) << n) + p1
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
        static void ada(uint* a, uint na, uint* b, uint nb)
        {
          uint i = 0; ulong c = 0L, d; Debug.Assert(na >= nb);
          for (; i < nb; i++, c = d >> 32) a[i] = unchecked((uint)(d = (a[i] + c) + b[i]));
          for (; c != 0 && i < na; i++, c = d >> 32) a[i] = unchecked((uint)(d = a[i] + c));
        }
      }
#endif
      #endregion
    }
    #endregion
  }
}