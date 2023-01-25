using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    /// <summary></summary>
    public override string ToString() { return ToString(null, null); }
    /// <summary></summary>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
      //if (format != default && format.Length == 0) formatProvider = NumberFormatInfo.InvariantInfo;
      Span<char> sp = stackalloc char[032 + (16 + 32)]; //32 dbg
      if (!TryFormat(sp, out var ns, format, formatProvider))
      {
        int n; sp.Slice(0, 2).CopyTo(new Span<char>(&n, 2)); sp = stackalloc char[n + 32];
        TryFormat(sp, out ns, format, formatProvider); Debug.Assert(ns != 0);
      }
      return sp.Slice(0, ns).ToString();
    }
    /// <summary></summary>
    public bool TryFormat(Span<char> dest, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
      var dbg = provider == null && format.Length == 0 && format != default;
      var fmt = 'H'; int dig = 0, rnd = 0; var info = dbg ? NumberFormatInfo.InvariantInfo : NumberFormatInfo.GetInstance(provider);
      if (format.Length != 0)
      {
        var f = (fmt = format[0]) & ~0x20; var d = format.Length > 1;
        if (d) dig = stoi(format.Slice(1)); //int.Parse(format.Slice(1));//, NumberFormatInfo.InvariantInfo);
        if (f == 'E') { rnd = dig; if (rnd == 0 && !d) rnd = 6; dig = rnd + 1; }
        if (f == 'F') { rnd = dig; if (rnd == 0 && !d) rnd = info.NumberDecimalDigits; dig = 0; }
      }
      if (dig == 0) dig = 032; // dec digits;
      if (dest.Length < dig + 16) { dig += 16; goto ex; }
      var cpu = main_cpu; uint u, v; // debug safety to allow core debug
      if (dbg && this.p != null && (((u = this.p[0] & 0x3fffffff) == 0 || (u + 3 > this.p.Length)) || (((v = this.p[u + 1]) == 0)) || (u + v + 2 > this.p.Length)))
        goto nan;

      cpu.push(this); if (cpu.msd() == 0) { cpu.pop(); goto nan; }
      var n = tos(dest, cpu, fmt, dig, rnd, 0, info.NumberDecimalSeparator[0] == ',' ? 0x04 : 0);
      if (n < 0) { dig = -n; goto ex; }
      if (dbg && this.p != null && (this.p[0] & 0x40000000) != 0 && dest.Length - n > 22) // debug nd info ₀ ₀  
      {
        dest[n++] = ' '; n += utos(dest.Slice(n), u = (this.p[0] & 0x3fffffff), '₀', 0);
        dest[n++] = ' '; n += utos(dest.Slice(n), this.p[u + 1], '₀', 0);
      }
      charsWritten = n; return true; ex:
      charsWritten = 0; if (dest.Length >= 2) new Span<char>(&dig, 2).CopyTo(dest); return false; nan:
      var s = info.NaNSymbol.AsSpan(); s.CopyTo(dest); charsWritten = s.Length; return true;
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
      var cpu = main_cpu; cpu.push(value, true); this.p = cpu.popr().p;
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
      var cpu = main_cpu; cpu.push(value); int r; cpu.ipop(&r, sizeof(int)); return r;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="uint"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="uint"/>.</returns>
    public static explicit operator uint(BigRational value)
    {
      var cpu = main_cpu; cpu.push(value); uint r; cpu.upop(&r, sizeof(uint)); return r;
      //var a = default(uint); main_cpu.toi(value, (uint*)&a, 0x0101); return a;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="long"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="long"/>.</returns>
    public static explicit operator long(BigRational value)
    {
      var cpu = main_cpu; cpu.push(value); long r; cpu.ipop(&r, sizeof(long)); return r;
      //var a = default(long); main_cpu.toi(value, (uint*)&a, 0x0002); return a;
    }
    /// <summary>
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="ulong"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="ulong"/>.</returns>
    public static explicit operator ulong(BigRational value)
    {
      var cpu = main_cpu; cpu.push(value); ulong r; cpu.upop(&r, sizeof(ulong)); return r;
      //var a = default(ulong); main_cpu.toi(value, (uint*)&a, 0x0102); return a;
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
    /// Defines an explicit conversion of a <see cref="BigRational"/> number to a <see cref="float"/> value.
    /// </summary>
    /// <param name="value">The value to convert to a <see cref="float"/>.</param>
    /// <returns>The value of the current instance, converted to an <see cref="float"/>.</returns>
    public static explicit operator float(BigRational value)
    {
      return (float)(double)value; //todo: fast float convert
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
      var cpu = main_cpu; cpu.push(value); return cpu.popm();
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
      var s = cpu.gets(cpu.mark() - 1);
      var a = MemoryMarshal.Cast<uint, byte>(s.Slice(1, unchecked((int)(s[0] & 0x3fffffff))));
      var r = new BigInteger(a, true, false); if (cpu.sign() < 0) r = -r; cpu.pop(); return r;
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
      var t = cpu.sp; cpu.sp = null; var r = cpu.getr(unchecked((uint)(i - 1))); //fetch
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
      return main_cpu.equ(a, b); //return a.Equals(b);
    }
    /// <summary>
    /// Returns a value that indicates whether two <see cref="BigRational"/> numbers have different values.
    /// </summary>
    /// <param name="a">The first value to compare.</param>
    /// <param name="b">The second value to compare.</param>
    /// <returns>true if left and right are not equal; otherwise, false.</returns>
    public static bool operator !=(BigRational a, BigRational b)
    {
      return !main_cpu.equ(a, b); //return !a.Equals(b);
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
      return main_cpu.cmp(a, b) <= 0; //return a.CompareTo(b) <= 0;
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
      return main_cpu.cmp(a, b) >= 0; // return a.CompareTo(b) >= 0;
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
      return main_cpu.cmp(a, b) < 0; // return a.CompareTo(b) < 0;
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
      return main_cpu.cmp(a, b) > 0; // return a.CompareTo(b) > 0;
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
      internal int i; internal uint[][] p; //todo: private
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
        fixed (uint* p = rent(4))
        {
          p[0] = (unchecked((uint)v) & 0x80000000) | 1;
          p[1] = unchecked((uint)BigRational.abs(v));
          *(ulong*)(p + 2) = 0x100000001;
        }
        //push(unchecked((uint)__abs(v))); if (v < 0) this.p[this.i - 1][0] |= unchecked((uint)v) & 0x80000000;
      }
      /// <summary>
      /// Pushes n copies of the supplied <see cref="int"/> value onto the stack.
      /// </summary>
      /// <param name="v">The value to push.</param>
      /// <param name="n">The number of copies to push.</param>
      public void push(int v, int n)
      {
        if (n <= 0) return;
        push(v); for (int i = 1; i < n; i++) dup(unchecked((uint)(this.i - 1)));
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
        push(unchecked((ulong)((v ^ (v >> 63)) - (v >> 63))));
        this.p[this.i - 1][0] |= unchecked((uint)(v >> 32)) & 0x80000000; //neg()
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
        fixed (uint* p = rent((unchecked((uint)BigRational.abs(e)) >> 5) + 8))
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
          p[0] = unchecked((uint)c) | ((uint)si & 0x80000000);
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
      /// Together with <see cref="gets(uint)"/> the operation represent a fast low level interface for direct access in form of the internal data representation.<br/> 
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
      public BigRational getr(uint i)
      {
        fixed (uint* p = this.p[i])
        {
          if (isz(p)) return default;
          if ((p[0] & 0x40000000) != 0) norm(p);
          var n = len(p);
          if (n == 4 && p[1] == p[3])
          {
            Debug.Assert(p[1] <= 1); // 11, 00
            return cachx(p[3] + (p[0] >> 31), p, n); //NaN, 1, -1            
          }
          var a = new uint[n]; fixed (uint* s = a) copy(s, p, n);
          return new BigRational(a);
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
      public double getd(uint i)
      {
        return (double)new BigRational(p[i]);
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
      public float getf(uint i)
      {
        return (float)(double)new BigRational(p[i]); // (float)getd(i);
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
      public ReadOnlySpan<uint> gets(uint i)
      {
        var p = this.p[i]; fixed (uint* u = p) return new ReadOnlySpan<uint>(p).Slice(0, unchecked((int)len(u)));
      }

      /// <summary>
      /// Removes the value currently on top of the stack, 
      /// convert and returns it as always normalized <see cref="BigRational"/> number.<br/>
      /// </summary>
      /// <returns>A always normalized <see cref="BigRational"/> number.</returns>
      public BigRational popr()
      {
        if (sp != null) { long d = (long)sp - (long)&d; if (d < 1024) return new BigRational(this.p[this.i - 1]); }
        var r = getr(unchecked((uint)(i - 1))); pop(); return r;
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
        var t = getd(unchecked((uint)(i - 1))); pop(); return t;
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
        var t = geti(unchecked((uint)(i - 1))); pop(); return t;
        //fixed (uint* u = p[i - 1]) { var i = unchecked((int)u[1]); if ((u[0] & 0x80000000) != 0) i = -i; pop(); return i; }
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
          var h = v[0]; v[0] &= 0x3fffffff; if (*(ulong*)u == 1) { swp(1, 2); pop(2); return; }
          div(v, u, w); *(ulong*)(w + w[0] + 1) = 0x100000001;
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
      public void shl(int c, int i = 0)
      {
        if (c <= 0) { if (c != 0) shr(-c, i); return; }
        fixed (uint* u = p[this.i - 1 - i])
        {
          if (*(ulong*)u == 1) return;
          fixed (uint* v = rent(len(u) + (unchecked((uint)c) >> 5) + 1))
          {
            var n = (u[0] & 0x3fffffff) + 1;
            copy(v, u, n); shl(v, c);
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
      public void shr(int c, int i = 0)
      {
        if (c <= 0) { if (c != 0) shl(-c, i); return; }
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
            else { shr(t, 1); var x = cms(u, t); f = x == 0 && f == 2 && (v[1] & 1) != 0 ? 1 : x; }
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
        uint e = unchecked((uint)BigRational.abs(y)), z; //todo: pow cache
        if (x == 10) { push(unchecked((ulong)Math.Pow(x, z = e < 19 ? e : 19))); e -= z; } //todo: opt. 1, 0, -1, -2, -4,...
        else if ((x & (x - 1)) == 0 && x >= 0) // 2, 4, 8
        {
          if (x > 1) { push(unchecked((uint)BitOperations.TrailingZeroCount(x))); shl(unchecked((int)e)); }
          else push(unchecked((uint)x)); e = 0;
        }
        else push(1u);
        if (e != 0)
        {
          push(x);
          for (; ; e >>= 1) { if ((e & 1) != 0) { mul(1, 0); if (e == 1) break; } sqr(); }
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
        push(1u); swp(); //todo: opt. ipt()
        for (var e = unchecked((uint)BigRational.abs(y)); ; e >>= 1)
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
        var v = stackalloc uint[] { unchecked((uint)b & 0x8000000) | 1, unchecked((uint)BigRational.abs(b)), 0, 1 };
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
        swp(0, 2); pop(2); return true;
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
          else rr = (uint*)(mem = Marshal.AllocCoTaskMem(need << 2));//todo: rem
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
        var e = sign(); if (e <= 0) { if (e < 0) { pop(); pnan(); } return; }
        e = bdi(); e = BigRational.abs(e); if (e > c) c = unchecked((uint)e);
        uint m = mark(), s; //get(m - 1, out double d); // best possible initial est: 
        //if (double.IsNormal(d)) push(Math.Sqrt(d));   // todo: enable afte checks, best possible start value
        //else
        {
          dup(); if ((s = msb()) > 1) shr(unchecked((int)s) >> 1); // est
          inv(); if ((s = msb()) > 1) shr(unchecked((int)s) >> 1); inv();
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
        if (a != 0) { push(1u); shl(a > 0 ? a : -a); if (a > 0) div(); else mul(); }
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
          push(1u); shl(unchecked((int)i)); inv(); // var p = Pow(2, -i); //var b = bdi(); if (i == c) { }
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
          push(1u); shl(unchecked((int)b)); inv(); if ((n & 1) != 0) neg(); //pow(-1, unchecked((int)n)); pow(2, unchecked((int)b)); div();
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
        var s = geti(m + 1); //dup(); var s = popi(); // seg                            
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
        var s = sign(); if (s == 0 || isnan()) return; // atan(0) = 0        
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
        if (sh != 0) shl(sh); // z *= pow(2, sh); 
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
      // /// <summary>
      // /// Converts an integer value on top of the stack to its two's complement and vice versa.
      // /// </summary>
      // /// <remarks>
      // /// Negative signed integer values are converted to it's unsigned variant.<br/>
      // /// Positive unsigned integers are converted to it's signed negative variant.<br/>
      // /// Parameter <paramref name="c"/> defines a integer type size in bytes.
      // /// </remarks>
      // /// <param name="c">Integer type size in bytes.</param>
      internal void toc(uint c) //todo: remove, replace with ipush ipop
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
          if (*(ulong*)ud == 1 || *(ulong*)vd == 1) { *(ulong*)w = *(ulong*)(w + 2) = 1; return; } //nan
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
          mul(u + 0, inv ? v + s : v, w); if (*(ulong*)w == 1 && *(ulong*)(u + t) != 1) { *(ulong*)(w + 2) = inv && *(ulong*)(v + s) == 1 ? 1ul : 0x100000001; return; } // 0 but keep NaN
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
      bool isnan()
      {
        fixed (uint* p = this.p[this.i - 1])
          return *(ulong*)(p + ((p[0] & 0x3fffffff) + 1)) == 1;// 0x100000000;
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
          switch (f)
          {
            case 0: *(ulong*)r = 1; return;
            case 1: copy(r, a, na + 1); r[0] = na; return;
              //case 2: //todo: opt. shl(1)
              //case 3: //todo: opt. + +
              //case 4: //todo: opt. shl(2)
              //...
          }
        }
        if (na == 1) *(ulong*)(r + 1) = (ulong)a[1] * b[1]; //todo: jt
        else
        if (na == 2 && Bmi2.X64.IsSupported)
        {
          *(ulong*)(r + 3) = Bmi2.X64.MultiplyNoFlags(*(ulong*)(a + 1), nb == 2 ? *(ulong*)(b + 1) : b[1], (ulong*)(r + 1));
        } //todo: banchmarks nb in low range 
        else if (na >= 00_32 && na + nb < 25000) kmu(a + 1, na, b + 1, nb, r + 1); //stack <~ 10 * (na + nb)
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
            f = b[k + 1]; c = 0; var s = r + k; //if (f == 0) { s[na + 1] = 0; continue; }
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
        else if (n >= 0_040 && n < 50000) kmu(a, n, v); //stack <~ 5 * n -> 250k
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
      static void shr(uint* p, int c)
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
      static bool isint(uint* p) //or nan
      {
        return *(ulong*)(p + ((p[0] & 0x3fffffff) + 1)) <= 0x100000001; //==
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      static int sig(uint* p)
      {
        return (p[0] & 0x80000000) != 0 ? -1 : isz(p) ? 0 : +1;
      }
      static void kmu(uint* a, uint na, uint* b, uint nb, uint* r)
      {
        if (nb < 00_20)
        {
          ulong c = 0, d;
          for (uint k = 0; k < na; k++, c = d >> 32)
            r[k] = unchecked((uint)(d = c + (ulong)a[k] * b[0]));
          r[na] = (uint)c;
          for (uint i = 1, k; i < nb; i++)
          {
            for (k = 0, c = 0; k < na; k++, c = d >> 32)
              r[i + k] = unchecked((uint)(d = r[i + k] + c + (ulong)a[k] * b[i]));
            r[i + na] = (uint)c;
          }
          return;
        }
        uint n = nb >> 1, m = n << 1;
        kmu(a, n, b, n, r);//, m);
        kmu(a + n, na - n, b + n, nb - n, r + m);//, nr - m);
        uint r1 = na - n + 1, r2 = nb - n + 1, r3 = r1 + r2, r4 = r1 + r2 + r3;
        uint* t1 = stackalloc uint[(int)r4], t2 = t1 + r1, t3 = t2 + r2;
        add(a + n, na - n, a, n, t1);
        add(b + n, nb - n, b, n, t2);
        kmu(t1, r1, t2, r2, t3);//, r3);
        sub(r + m, (na + nb) - m, r, m, t3, r3);
        add(r + n, (na + nb) - n, t3, r3);
      }
      static void kmu(uint* a, uint na, uint* r)
      {
        if (na < 0_032)
        {
          ulong u, v, c; r[0] = unchecked((uint)(u = (ulong)a[0] * a[0])); r[1] = (uint)(u >> 32);
          for (uint i = 1, k; i < na; i++)
          {
            for (k = 0, c = 0; k < i; k++, c = (v + (u >> 1)) >> 31)
              r[i + k] = unchecked((uint)((u = r[i + k] + c) + ((v = (ulong)a[k] * a[i]) << 1)));
            r[i + i] = unchecked((uint)(u = (ulong)a[i] * a[i] + c)); r[i + i + 1] = (uint)(u >> 32);
          }
          return;
        }
        uint n = na >> 1, m = n << 1;
        kmu(a, n, r);
        kmu(a + n, na - n, r + m);
        uint r1 = na - n + 1, r2 = r1 + r1;
        uint* t1 = stackalloc uint[(int)(r1 + r2)], t2 = t1 + r1;
        add(a + n, na - n, a, n, t1);
        kmu(t1, r1, t2);
        sub(r + m, na + na - m, r, m, t2, r2);
        add(r + n, na + na - n, t2, r2);
      }
      static void sub(uint* a, uint na, uint* b, uint nb, uint* r, uint nr)
      {
        uint i = 0; long c = 0, d;
        for (; i < nb; i++, c = d >> 32) r[i] = unchecked((uint)(d = (r[i] + c) - a[i] - b[i]));
        for (; i < na; i++, c = d >> 32) r[i] = unchecked((uint)(d = (r[i] + c) - a[i]));
        for (; c != 0 && i < nr; i++, c = d >> 32) r[i] = unchecked((uint)(d = r[i] + c));
      }
      static void add(uint* a, uint na, uint* b, uint nb, uint* r)
      {
        uint i = 0; ulong c = 0, d; //Debug.Assert(na >= nb && nr == na + 1);
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
      internal void asin(uint c)
      {
        dup(); sqr(); push(1u); var t = cmp(); // Atan(x / Sqrt(1 - x * x));
        if (t <= 0) { var s = sign(2); pop(3); if (t == 0) { pi(c); shr(1); if (s < 0) neg(); } else pnan(); return; }
        swp(); sub(); sqrt(c); div(); atan(c);
      }
      internal void acos(uint c)
      {
        dup(); push(1u); swp(); sub(); // 2 * Atan(Sqrt((1 - x) / (1 + x)))
        swp(); push(1u); add(); if (sign() == 0) { pop(2); pi(c); return; }
        div(); if (sign() < 0) { pop(); pnan(); return; }
        sqrt(c); atan(c); shl(1);
      }
      internal int geti(uint i)
      {
        fixed (uint* u = this.p[i])
        {
          var v = unchecked((int)u[1]);
          return (u[0] & 0x80000000) != 0 ? -v : v;
        }
      }
      internal uint msd()
      {
        fixed (uint* p = this.p[this.i - 1])
        {
          uint i1 = p[0] & 0x3fffffff, i2 = p[i1 + 1];
          return (i2 << 5) - unchecked((uint)BitOperations.LeadingZeroCount(p[i1 + 1 + i2]));
        }
      }
      internal double flog10(uint f)
      {
        fixed (uint* u = this.p[this.i - 1])
        {
          uint n = u[0] & 0x3fffffff; var p = u; if ((f & 1) != 0) { p = p + n + 1; n = p[0]; }
          if (n == 1) return Math.Log10(p[1]);
          var c = BitOperations.LeadingZeroCount(p[n]); Debug.Assert(c != 32);
          var b = ((long)n << 5) - unchecked((uint)c);
          ulong h = p[n], m = p[n - 1], l = n > 2 ? p[n - 2] : 0;
          ulong x = (h << 32 + c) | (m << c) | (l >> 32 - c);
          var r = Math.Log10(x) + (b - 64) * 0.3010299956639811952; return r;
        }
      }
      internal int ilog10()
      {
        Debug.Assert(sign() != 0 && !isnan());
        //var x = (int)Math.Floor(flog10(0) - flog10(1));
        double a = flog10(0), b = flog10(1), c = a - b;
        int e = (int)c; if (c < 0) e--;
        //Debug.Assert(e == x);
        return e;
      }
      internal bool bt(int c, int i = 0) //BT Bit Test, BTS and Set, BTR and Reset, BTC and Complement
      {
        if (c < 0) return false;
        fixed (uint* p = this.p[this.i - 1 - i])
        {
          if ((c >> 5) >= (p[0] & 0x3fffffff)) return false;
          return (p[1 + (c >> 5)] & (1u << (c & 31))) != 0;
        }
        //return (this.p[this.i - 1 - i][1 + (c >> 5)] & (1 << (c & 31))) != 0;
      }
      internal static int hash(void* p, int n)
      {
        uint h = 0, c = unchecked((uint)n) >> 2;
        for (uint i = 0; i < c; i++) h = ((uint*)p)[i] ^ ((h << 7) | (h >> 25));
        if ((n & 2) != 0) h ^= ((uint*)p)[c] & 0xffff;
        return unchecked((int)h);
      }
      internal static void inc(void* p, int n) //todo: apply for inc()
      {
        for (uint c = unchecked((uint)n >> 2), i = 0; i < c; i++)
          if (++((uint*)p)[i] != 0) return; // false;
        if ((n & 2) != 0) if (++((ushort*)p)[unchecked((uint)n >> 1) - 1] != 0) return; // false; return true;
      }
      internal static void dec(void* p, int n) //todo: apply for dec()
      {
        for (uint c = unchecked((uint)n >> 2), i = 0; i < c; i++)
          if (((uint*)p)[i]-- != 0) return; // false;
        if ((n & 2) != 0) if (((ushort*)p)[unchecked((uint)n >> 1) - 1]-- != 0) return; // false; return true;
      }
      //for INumber to avoid another ThreadLocal static root - CPU doesn't need it
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal int maxdigits = 30; //INumber default limitation for irrational funcs 
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal void* sp; //debug security for visualizer and cross thread access       
      #region int support
      internal void divrem() //todo: divrem public?
      {
        fixed (uint* u = p[this.i - 1])
        fixed (uint* v = p[this.i - 2])
        fixed (uint* w = rent(len(u))) // nu - nv + 1
        {
          var h = v[0]; v[0] &= 0x3fffffff; div(v, u, w);
          *(ulong*)(v + v[0] + 1) = 0x100000001; v[0] |= h & 0x80000000;
          *(ulong*)(w + w[0] + 1) = 0x100000001; if (((h ^ u[0]) & 0x80000000) != 0 && *(ulong*)w != 1) w[0] |= 0x80000000;
        }
        swp(1); pop();
      }
      internal static int ucmp(void* a, void* b, int size)
      {
        Debug.Assert(size >= 4 && (size & 3) == 0);
        for (uint i = unchecked((uint)size >> 2); i-- != 0;)
          if (((uint*)a)[i] != ((uint*)b)[i])
            return (((uint*)a)[i] > ((uint*)b)[i]) ? +1 : -1;
        return 0;
      }
      internal void upush(void* p, int size)
      {
        if (size < 4) { push(size == 1 ? (uint)*(byte*)p : (uint)*(ushort*)p); return; }
        uint n = unchecked((uint)size >> 2), i; Debug.Assert((size & 3) == 0); // for now
        fixed (uint* v = rent(n + 3))
        {
          copy(v + 1, (uint*)p, n); for (i = n; i > 1 && v[i] == 0; i--) ;
          v[0] = i; *(ulong*)(v + i + 1) = 0x100000001;
        }
      }
      internal void upop(void* p, int size)
      {
        var f = (size & 0xffff) >> 2;
        if (f == 0)
        {
          int t; ipop(&t, 0x10000004); var m = size >> 28;
          if ((size & 3) == 1) *(byte*)p = m == 0 ? unchecked((byte)t) : m == 1 ? unchecked((byte)(t < 0 ? 0 : t > 0xff ? 0xff : t)) : checked((byte)t);
          else *(ushort*)p = m == 0 ? unchecked((ushort)t) : m == 1 ? unchecked((ushort)(t < 0 ? 0 : t > 0xffff ? 0xffff : t)) : checked((ushort)t);
          return;
        }
        var v = (uint*)p; uint n = unchecked((uint)f); Debug.Assert((size & 3) == 0);
        var s = sign(); if (s == 0) { pop(); copy(v, n); return; } // zero
        rnd(0, 0); uint b = msb(), c = n << 5;
        if ((b > c || s < 0) && (f = size >> 28) != 0)
        {
          pop(); if (f == 2) throw new OverflowException();
          for (b = s > 0 ? 0xffffffff : 0, c = 0; c < n; c++) v[c] = b; return;
        }
        fixed (uint* t = this.p[this.i - 1])
        {
          copy(v, t + 1, n < (b = t[0] & 0x3fffffff) ? n : b);
          if (n > b) copy(v + b, n - b);
        }
        pop(); if (s < 0) ineg(p, size & 0xffff);
      }
      internal static bool ineg(void* p, int size)
      {
        Debug.Assert(size >= 4 && (size & 3) == 0);
        var u = (uint*)p; uint n = unchecked((uint)size) >> 2;
        for (uint i = 0, c = 1; i < n; i++)
        {
          var x = unchecked((ulong)(u[i] ^ 0xffffffff) + c);
          u[i] = unchecked((uint)x); c = unchecked((uint)(x >> 32));
        }
        return true;
      }
      internal static int icmp(void* a, void* b, int size)
      {
        Debug.Assert(size >= 4 && (size & 3) == 0 && (size & 0x100) == 0);
        uint n = unchecked((uint)size >> 2) - 1, i;
        for (i = n; ((uint*)a)[i] == ((uint*)b)[i]; i--) if (i == 0) return 0;
        var s = (((uint*)a)[i] > ((uint*)b)[i]) ? +1 : -1;
        if (i == n && (((((uint*)a)[i] ^ ((uint*)b)[i])) & 0x80000000) != 0) return -s;
        return s;
        //var sa = (f & 0x100) == 0 ? ((uint*)a)[n - 1] >> 31 : 0;
        //var sb = (f & 0x100) == 0 ? ((uint*)b)[n - 1] >> 31 : 0;
        //if (sa != sb) return sa != 0 ? -1 : +1;
        //for (uint i = n; i-- != 0;)
        //  if (((uint*)a)[i] != ((uint*)b)[i])
        //    return (((uint*)a)[i] > ((uint*)b)[i]) ? +1 : -1;
        //return 0;
      }
      internal void ipush(void* p, int size)
      {
        if (size < 4) { push(size == 1 ? (int)*(sbyte*)p : (int)*(short*)p); return; }
        var f = size >> 2; Debug.Assert((size & 3) == 0); // for now
        uint n = unchecked((uint)f), s = ((uint*)p)[n - 1] & 0x80000000;
        fixed (uint* v = rent(n + 3))
        {
          copy(v + 1, (uint*)p, n);
          if ((s & 0x80000000) != 0) ineg(v + 1, size);
          var i = n; for (; i > 1 && v[i] == 0; i--) ;
          v[0] = i | s; *(ulong*)(v + i + 1) = 0x100000001;
        }
      }
      internal void ipop(void* p, int size)
      {
        var f = (size & 0xffff) >> 2;
        if (f == 0)
        {
          int m = size >> 28, t; ipop(&t, 0x10000004);
          if ((size & 3) == 1) *(sbyte*)p = m == 0 ? unchecked((sbyte)t) : m == 1 ? unchecked((sbyte)(t < sbyte.MinValue ? sbyte.MinValue : t > sbyte.MaxValue ? sbyte.MaxValue : t)) : checked((sbyte)t);
          else *(short*)p = m == 0 ? unchecked((short)t) : m == 1 ? unchecked((short)(t < short.MinValue ? short.MinValue : t > short.MaxValue ? short.MaxValue : t)) : checked((short)t);
          return;
        }
        var v = (uint*)p; uint n = unchecked((uint)f); Debug.Assert((size & 3) == 0);
        var s = sign(); if (s == 0) { pop(); copy(v, n); return; }
        rnd(0, 0); uint b = msb(), c = n << 5;
        if ((b > c || b == c && s > 0) && (f = size >> 28) != 0)
        {
          pop(); if (f == 2) throw new OverflowException();
          for (b = s > 0 ? 0xffffffff : 0, n--, c = 0; c < n; c++) v[c] = b;
          v[n] = s > 0 ? 0x7fffffff : 0x80000000; return;
        }
        fixed (uint* t = this.p[this.i - 1])
        {
          copy(v, t + 1, n < (b = t[0] & 0x3fffffff) ? n : b);
          if (n > b) copy(v + b, n - b);
        }
        pop(); if (s < 0) ineg(p, size & 0xffff);
      }
      #endregion
      #region fp support (not final optimized)
      internal static int fdesc(int size)
      {
        int sbi; Debug.Assert((size & 1) == 0); //for now 
        switch (size)
        {
          case 2: sbi = 5; break;
          case 4: sbi = 8; break;
          case 8: sbi = 11; break;
          case <= 16: sbi = 15; break;
          default: sbi = BitOperations.Log2(unchecked((uint)size)) * 3 + 4; break;
        }
        return size | ((((size << 3) - sbi) - 1) << 16);
      }
      internal static bool fequ<T>(void* a, void* b) //todo: uequ
      {
        int desc = tdesc<T>.desc & 0x0fffffff;
        uint n = unchecked((uint)desc & 0xffff), c, i = 0;
        uint* u = (uint*)a, v = (uint*)b;
        for (c = n >> 2; i < c; i++) if (u[i] != v[i]) return false;
        if (c << 2 < n) if (*(ushort*)&u[c] != *(ushort*)&v[c]) return false;
        return true;
      }
      internal static int fcmp<T>(void* a, void* b)
      {
        int desc = tdesc<T>.desc & 0x0fffffff, size = desc & 0xffff; //todo: nan cmp
        var ha = *(uint*)(((byte*)a) + (size - 4));
        var hb = *(uint*)(((byte*)b) + (size - 4));
        var sa = ha == 0 ? 0 : (ha & 0x80000000) != 0 ? -1 : +1;
        var sb = hb == 0 ? 0 : (hb & 0x80000000) != 0 ? -1 : +1;
        if (sa != sb) return Math.Sign(sa - sb); if (sa == 0) return 0;
        int bc = desc >> 16, ec = (size << 3) - bc, bi = ((1 << (ec - 2)) + bc) - 1;
        var ea = unchecked((int)((ha & 0x7fffffff) >> (32 - ec))) - bi;
        var eb = unchecked((int)((hb & 0x7fffffff) >> (32 - ec))) - bi;
        if (ea != eb) { return Math.Sign(ea - eb) * sa; }
        int i = (size >> 2) - 1;
        if ((size & 2) != 0) Debug.Assert((ha >> 16) == (hb >> 16));
        for (; i >= 0; i--) if (((uint*)a)[i] != ((uint*)b)[i])
          {
            var g = ((uint*)a)[i] > ((uint*)b)[i] ? +1 : -1; return g * sa;
          }
        return 0;
      }
      internal static int fexpo<T>(void* p)
      {
        var desc = tdesc<T>.desc & 0x0fffffff;
        int size = desc & 0xffff, bc = desc >> 16, ec = (size << 3) - bc;
        var h = *(uint*)(((byte*)p) + (size - 4));
        var eb = (0xffffffff >> (32 - ec)) >> 2; // exponent bias 1023 127
        var be = (h & 0x7fffffff) >> (32 - ec); // biased exponent
        return unchecked((int)be - (int)eb); // exponent
      }
      internal static void fnan<T>(void* p, int f)
      {
        var desc = tdesc<T>.desc & 0x0fffffff;
        int size = desc & 0xffff, bc = desc >> 16, ec = (size << 3) - bc;
        var pin = ~(0xffffffff >> ec); Debug.Assert(pin == ~((1u << (32 - ec)) - 1));
        if (f == 0) pin |= 1u << (31 - ec); //NaN
        else if (f == 2) pin ^= 0x80000000; //+Infinity
        copy((uint*)p, unchecked((uint)(size >> 2))); *(uint*)(((byte*)p) + (size - 4)) = pin;
      }
      internal static int ftest<T>(void* p)
      {
        var desc = tdesc<T>.desc & 0x0fffffff;
        int size = desc & 0xffff, bc = desc >> 16, ec = (size << 3) - bc; //if (size == 2) { }
        var h = *(uint*)(((byte*)p) + (size - 4));
        var pin = 0x7fffffff & ~(0xffffffff >> ec); Debug.Assert(pin == ~(((1u << (32 - ec)) - 1) | 0x80000000));
        if ((h & pin) == pin)
        {
          if (h == pin) return 3; //PositiveInfinity
          else if (h == (pin | 0x80000000)) return 2; //NegativeInfinity
          else if (h == ((pin >> 1) | 0xc0000000)) return 1; //NaN
          else return 1; // ??? AllBitsSet
        }
        //if ((h & 0x80000000) != 0)
        //{
        //  var bi = ((1 << (ec - 2)) + bc) - 1;
        //  var e = unchecked((int)((h & 0x7fffffff) >> (32 - ec))) - bi;
        //  if (bi + e <= 0) return 4; // -0.0
        //}
        return 0;
      }
      internal int fpush<T>(void* p)
      {
        var desc = tdesc<T>.desc & 0x0fffffff;
        int size = desc & 0xffff, bc = desc >> 16, ec = (size << 3) - bc, bi = ((1 << (ec - 2)) + bc) - 1;
        var h = *(uint*)(((byte*)p) + (size - 4));
        var e = unchecked((int)((h & 0x7fffffff) >> (32 - ec))) - bi;
        if (bi + e <= 0) { push(); return e; } // 0, -0.0
        if (bi + e >= (1 << (ec - 1)) - 1) { pnan(); return e; } // +/- Infinity or NaN
        var l = unchecked((uint)size >> 2);
        if ((size & 2) != 0 && l++ == 0) h = (h & 0x80000000) | (h >> 16);
        fixed (uint* u = rent(l + 3))
        {
          u[0] = l | (h & 0x80000000); copy(u + 1, (uint*)p, l - 1);
          u[l] = (1u << bc) | (h & ((1u << bc) - 1)); *(ulong*)&u[l + 1] = 0x100000001;
        }
        return e;
      }
      internal void fpop<T>(void* p, int e, bool add = false)
      {
        fixed (uint* u = this.p[this.i - 1])
        {
          var desc = tdesc<T>.desc & 0x0fffffff;
          int size = desc & 0xffff, bc = desc >> 16, ec = (size << 3) - bc, bi = ((1 << (ec - 2)) + bc) - 1;
          var n = u[0] & 0x3fffffff; var msb = (n << 5) - unchecked((uint)BitOperations.LeadingZeroCount(u[n]));
          var s = u[0] & 0x80000000; var d = unchecked((int)msb - (bc + 1)); e += d;
          var l = unchecked((uint)((size >> 2) + ((size >> 1) & 1)));
          if (bi + e <= 0 || msb == 0) // 0, -0.0
          {
            if (*(ulong*)(u + (n + 1)) == 1) { pop(); fnan<T>(p, 0); return; } //NaN 
            pop(); copy(((uint*)p), l); if (s != 0) *(uint*)(((byte*)p) + (size - 4)) |= s; return; //-0.0
          }
          if (bi + e >= (1 << (ec - 1)) - 1) { pop(); fnan<T>(p, s != 0 ? 1 : 2); return; } // +/- Infinity
          if (d == 0)
          {
            if (add) { }
          }
          else if (d > 0)
          {
            if (add) { }
            var t = d - 1; var c = (u[1 + (t >> 5)] & (1u << (t & 31))) != 0; //var u1 = u[1 + (t >> 5)];
            shr(u, d);
            if (c)
            {
              if (add) { }
              else
                u[1]++;
              //if (add) { if ((u[1] & 1) == 1) u[1]++; } else u[1]++;
            }
          }
          else if (d < 0)
          {
            if (add) { }
            if (size >= this.p[this.i - 1].Length << 2) //todo: finale check this very rare case //Debug.Assert(n + ((uint)(-d) >> 5) + 1 <= this.p[this.i - 1].Length);
              fixed (uint* t = rent(l)) { copy(t, u, n + 1); fpop<T>(p, e - d); pop(); return; }
            shl(u, -d);
          }

          var ps = (uint*)(((byte*)u) + size);
          *ps = s | (unchecked((uint)(bi + e)) << (32 - ec)) | (*ps & ~(1u << (32 - ec)));
          copy((uint*)p, u + 1, l); pop();
        }
      }
      internal void fpop<T>(void* p)
      {
        var b = msd(); if (b <= 1) { fpop<T>(p, 0); return; } // 0, int, nan
        var a = msb(); if (a == 0) { } // 0, nan
        var desc = tdesc<T>.desc & 0x0fffffff;
        var c = unchecked((int)a - (int)b);
        var d = (desc >> 16) + 1; // 52 + 1
        var e = d - c; if (e <= 0) { } //todo: opt. handle? Debug.Assert(e > 0);
        shl(e); mod(0); swp(); pop();
        fpop<T>(p, -e);
      }
      internal void fpushr<T>(void* p)
      {
        var e = fpush<T>(p); if (sign() == 0) return; if (isnan()) return;
        pow(2, e); mul();
      }
      internal void fcast<A, B>(void* d, void* s)
      {
        var x = ftest<B>(s); if (x != 0) { fnan<A>(d, x - 1); return; }
        var e = fpush<B>(s); fpop<A>(d, e);
      }
      #endregion
      #region dec support
      internal decimal popm()
      {
        var e = 0; // dup(); tos(default, out _, out var ee, out _, false);
        var a = msb(); if (a == 0) goto ex; // 0
        var b = msd(); if (b == 1) { if (a > 96) goto ex; }
        else
        {
          double u = flog10(0), v = flog10(1), w = u - v; e = (int)w; if (w < 0) e--;
          if (e < -28 || e > 28) goto ex; e = 28 - (e < 0 ? 0 : e);
          pow(10, e); mul(); rnd(0); b = msb(); if (b > 96) { push(10u); idiv(); e--; }
        }
        fixed (uint* u = this.p[this.i - 1])
        {
          var v = (int*)u; var n = v[0] & 0x3fffffff; Debug.Assert(n <= 3);
          var d = new decimal(v[1], n >= 2 ? v[2] : 0, n >= 3 ? v[3] : 0, (u[0] & 0x80000000) != 0, unchecked((byte)e));
          pop(); return d;
        }
      ex: pop(); return default;
      }
      #endregion
      #region number typecasts // can highly optimized - copy cast's by desc / f checks etc.
      internal static class tdesc<T>
      {
        internal static readonly int desc;
        static tdesc()
        {
          var t = typeof(T); desc = Unsafe.SizeOf<T>(); //todo: opt. t.IsPrimitive, ...
          if (t == typeof(int) || t == typeof(long) || t == typeof(sbyte) || t == typeof(short) || t == typeof(nint)) return;
          if (t == typeof(uint) || t == typeof(ulong) || t == typeof(byte) || t == typeof(char) || t == typeof(ushort) || t == typeof(nuint)) { desc |= (1 << 28); return; }
          if (t == typeof(float) | t == typeof(double) || t == typeof(Half) || t == typeof(NFloat)) { desc = fdesc(desc) | (2 << 28); return; }
          if (t == typeof(decimal)) { desc |= (3 << 28); return; }
          if (t == typeof(BigRational)) { desc |= (4 << 28); return; }
          if (t == typeof(BigRational.Integer)) { desc |= (5 << 28); return; }
          if (t == typeof(BigInteger)) { desc |= (6 << 28); return; }
          if (t.IsGenericType)
          {
            var g = t.GetGenericTypeDefinition();
            if (g == typeof(Generic.Int<>)) return;
            if (g == typeof(Generic.UInt<>)) { desc |= (1 << 28); return; }
            if (g == typeof(Generic.Float<>)) { desc = fdesc(desc) | (2 << 28); return; }
            if (g == typeof(Generic.Decimal<>)) { desc |= (3 << 28); return; }
          }
#if NET7_0
          if (t == typeof(Int128)) return;
          if (t == typeof(UInt128)) { desc |= (1 << 28); return; }
#endif
          desc = 0;
        }
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      internal bool cast<A, B>(A a, out B b, int f)
      {
        int ta = tdesc<A>.desc; var u = Unsafe.AsPointer(ref a); Unsafe.SkipInit(out b);
        var tb = tdesc<B>.desc; var v = Unsafe.AsPointer(ref b); //if (ta == tb) { }
        switch (ta >> 28)
        {
          case 0: ipush(u, ta & 0x0000ffff); break;
          case 1: upush(u, ta & 0x0000ffff); break;
          case 2:
            if ((tb >> 28) == 2) { fcast<B, A>(v, u); return true; }
            fpushr<A>(u); break;
          case 3: push(*(decimal*)u); break; //todo: mpush
          case 4: push(Unsafe.AsRef<BigRational>(u)); break;
          case 5: push(Unsafe.AsRef<BigRational.Integer>(u)); break;
          case 6: push(Unsafe.AsRef<BigInteger>(u)); break;
          default: return false;
        }
        switch (tb >> 28)
        {
          case 0: ipop(v, (tb & 0x0000ffff) | (f << 28)); return true;
          case 1: upop(v, (tb & 0x0000ffff) | (f << 28)); return true;
          case 2: fpop<B>(v); return true;
          case 3: Unsafe.AsRef<decimal>(v) = popm(); return true; //todo: mpop
          case 4: Unsafe.AsRef<BigRational>(v) = popr(); return true;
          case 5: Unsafe.AsRef<BigRational.Integer>(v) = (BigRational.Integer)popr(); return true;
          case 6: Unsafe.AsRef<BigInteger>(v) = (BigInteger)popr(); return true;
          default: pop(); return false;
        }
      }
      #endregion 
      #endregion experimental
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
      //new ReadOnlySpan<uint>(s, unchecked((int)n)).CopyTo(new Span<uint>(d, unchecked((int)n))); return; // 10 % slower
      //if (n > 16) { new ReadOnlySpan<uint>(s, unchecked((int)n)).CopyTo(new Span<uint>(d, unchecked((int)n))); return; } // 5% slower 
      uint i = 0, c;
      for (c = n & ~3u; i < c; i += 4) *(decimal*)&((byte*)d)[i << 2] = *(decimal*)&((byte*)s)[i << 2]; // RyuJIT vmovdqu
      for (c = n & ~1u; i < c; i += 2) *(ulong*)&((byte*)d)[i << 2] = *(ulong*)&((byte*)s)[i << 2];
      if (i != n) d[i] = s[i]; //if ?
    }
    [ThreadStatic, DebuggerBrowsable(DebuggerBrowsableState.Never)] //debug visualizer security
    static CPU? cpu;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //debug visualizer security
    internal static CPU main_cpu => cpu ??= new CPU();
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
    #region experimental 
    internal static int tos(Span<char> sp, CPU cpu, char fmt, int dig, int rnd, int es, int fl)
    {
      var sig = cpu.sign(); var f = fmt & ~0x20; Debug.Assert(sp.Length >= dig + 16); // -.E+2147483647 14
      if (f == 'X')
      {
        int z = unchecked((int)cpu.msb()), xx = (fl & 0x01) == 0 ? (z >> 2) + 1 : z == 0 ? 1 : (z + 3) >> 2;
        if (sig < 0 && (z & 3) == 0 && cpu.ipt()) xx--; //80
        var n = dig > xx ? dig : xx; if (sp.Length < n) { cpu.pop(); return -n; }
        if (sig < 0) cpu.toc(4); var pp = cpu.gets(cpu.mark() - 1);
        pp = pp.Slice(1, unchecked((int)(pp[0] & 0x7fffffff)));
        for (int i = 0, k, o = fmt == 'X' ? 'A' - 10 : 'a' - 10; i < n; i++)
        {
          var d = (k = i >> 3) < pp.Length ? (pp[k] >> ((i & 7) << 2)) & 0xf : sig < 0 ? 0xf : 0u;
          sp[n - i - 1] = (char)(d < 10 ? '0' + d : o + d);
        }
        cpu.pop(); return n;
      }
      var e = sig != 0 ? cpu.ilog10() : 0;
#if DEBUG
      // { cpu.dup(); cpu.tos(default, out _, out var ee, out _, false); Debug.Assert(e == ee); } //todo: remove 
#endif
      if (f == 'E') cpu.rnd(rnd - e);
      else if (f == 'F')
      {
        var h = abs(e) + rnd + 16; if (h > sp.Length) { cpu.pop(); return -h; }
        var d = e + rnd + 1; if (d > dig) dig = d;
        cpu.rnd(rnd);
      }
      else if (f == 'D') { var h = e + 16; if (h > sp.Length) { cpu.pop(); return -h; } dig = e + 1; }
      else if (f == 'H') { }
      else { var h = dig - e - 1; cpu.rnd(h); } // G3 double bug in NET ?
      var tp = sp.Slice(0, dig);
      cpu.tos(tp, out var ns, out e, out var r, f == 'H');
      if (f == 'H')
      {
        if (r != -1) { sp.Slice(r, ns - r).CopyTo(sp.Slice(r + 1)); sp[r] = '\''; ns++; }
        if (r == 0) { sp.Slice(0, ns).CopyTo(sp.Slice(1)); sp[0] = '0'; ns++; e++; } //r++; 
      }
      else { tp = tp.Slice(0, ns).TrimEnd('0'); ns = tp.Length; e += es; }
      int x = e + 1, l = 0;
      if (f == 'E') { x = 1; if (ns < dig) l = dig; }
      else if (f == 'F') { if (ns < x + rnd) l = x + rnd; e = 0; }
      else if (f == 'D') { x = e + 1; e = 0; }
      else if (f == 'H')
      {
        if ((r != -1 && e >= r) || (e <= -5 || e >= 17)) x = 1; else e = 0;
        if (ns == dig && r == -1) sp[ns++] = '…';
      }
      else if (e <= -5 || e >= 17) x = 1;
      else { var h = dig - e - 1; if (h < 0) x = 1; else e = 0; } // 'G'
      if (l != 0) { sp.Slice(ns, l - ns).Fill('0'); ns = l; } //todo: opt. in
      if (x >= ns) { sp.Slice(ns, x - ns).Fill('0'); ns = x; }
      else
      {
        if (x <= 0) { sp.Slice(0, ns).CopyTo(sp.Slice(x = 1 - x, ns)); sp.Slice(0, x).Fill('0'); ns += x; x = 1; }
        sp.Slice(x, ns - x).CopyTo(sp.Slice(x + 1, ns - x)); sp[x] = (fl & 0x04) != 0 ? ',' : '.'; ns++;
      }
      if (sig == -1) { sp.Slice(0, ns).CopyTo(sp.Slice(1, ns)); sp[0] = '-'; ns++; }
      if (e != 0 || f == 'E')
      {
        sp[ns++] = (fmt & 0x20) == 0 ? 'E' : 'e'; sp[ns++] = e > 0 ? '+' : '-';
        ns += utos(sp.Slice(ns), unchecked((uint)(e > 0 ? e : -e)), '0', f == 'E' ? 3 : 2);
        //(e > 0 ? e : -e).TryFormat(sp.Slice(ns), out var z, f == 'E' ? "000" : "00", NumberFormatInfo.InvariantInfo); ns += z;
      }
      return ns;
    }
    internal static int utos(Span<char> s, uint u, char z, int m) // avoid uint.Parse as they use ArrayPool<>.Shared which keeps crashing the debug visualizer
    {
      int i = 0; for (; u != 0 || i < m; u /= 10u) s[i++] = unchecked((char)(z + u % 10));
      s.Slice(0, i).Reverse(); return i;
    }
    internal static int stoi(ReadOnlySpan<char> s) // avoid int.ToString as they use ArrayPool<>.Shared which keeps crashing the debug visualizer
    {
      int v = 0; for (int i = 0, n = s.Length; i < n; i++) { var x = s[i] - '0'; if (x < 0 || x > 9) break; v = v * 10 + x; }
      return v;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int abs(int x)
    {
      var a = (x ^ (x >> 31)) - (x >> 31); //var b = x < 0 ? -x : x; Debug.Assert(a == b);
      return a;
    }
    #endregion
  }
}
