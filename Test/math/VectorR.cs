

using System.Diagnostics.CodeAnalysis;
using Test;

namespace System.Numerics.Rational
{
  /// <summary>
  /// A general imutable arbitrary one dimensional vector class based on <see cref="BigRational.CPU"/>.<br/>
  /// <i>This is just a non-optimal example implementation for testing!</i>
  /// </summary>
  [Serializable, DebuggerDisplay("{ToString(\"\"),nq}")]
  public readonly struct VectorR : IEnumerable<BigRational>, IEquatable<VectorR> //, IFormattable, ISpanFormattable, IReadOnlyList<BigRational> 
  {
    public override string ToString()
    {
      return ToString(null);
    }
    public readonly string ToString(string? format, IFormatProvider? provider = null)
    {
      return '[' + string.Join(' ', this.Select(p => p.ToString(format, provider))) + ']';
    }
    public static VectorR Parse(ReadOnlySpan<char> sp)
    {
      sp = sp.Trim().Trim("[]").Trim(); //trim for param_..., tor don't need it
      int n = SpanTools.param_count(sp); //todo: if(n > anylim) -> chunks
      var cpu = rat.task_cpu; 
      for (int i = 0; i < n; i++) cpu.tor(SpanTools.param_slice(ref sp), 10, default);
      var r = Create(cpu, n); cpu.pop(n); return r;
    }
    public unsafe override int GetHashCode()
    {
      if (p == null) return 0;
      fixed (uint* p = this.p)
      { //todo: change, own hash is definitely faster and does not ignore bits
        var h = new HashCode(); h.AddBytes(new ReadOnlySpan<byte>(p, this.p.Length << 2));
        return h.ToHashCode();
      }
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      return obj is VectorR t && this.Equals(t);
    }
    public bool Equals(VectorR b)
    {
      if (p == b.p) return true;
      if (p == null || p.Length != b.p.Length) return false;
      for (int i = 0; i < p.Length; i++) if (p[i] != b.p[i]) return false;
      return true;
    }
    public int Length
    {
      get => p != null ? unchecked((int)p[0]) : 0;
    }
    public BigRational this[int i]
    {
      get { var cpu = rat.task_cpu; cpu.push(getat(i)); return cpu.popr(); }
    }
    public static bool operator ==(VectorR a, VectorR b)
    {
      return a.Equals(b);
    }
    public static bool operator !=(VectorR a, VectorR b)
    {
      return !a.Equals(b);
    }
    public static VectorR operator +(VectorR a) => a;
    public static VectorR operator -(VectorR a)
    {
      var cpu = rat.task_cpu; var n = a.Length;
      for (int i = 0; i < n; i++) { cpu.push(a.getat(i)); cpu.neg(); }
      var r = Create(cpu, n); cpu.pop(n); return r;
    }
    public static VectorR operator *(VectorR a, BigRational b)
    {
      if (b == 1) return a;
      var cpu = rat.task_cpu; var n = a.Length;
      for (int i = 0; i < n; i++) { cpu.push(a.getat(i)); cpu.mul(b); }
      var r = Create(cpu, n); cpu.pop(n); return r;
    }
    public static VectorR operator *(VectorR a, VectorR b)
    {
      var cpu = rat.task_cpu; var n = Math.Min(a.Length, b.Length); //one possible interpretation
      for (int i = 0; i < n; i++) { cpu.push(a.getat(i)); cpu.push(b.getat(i)); cpu.mul(); }
      var r = Create(cpu, n); cpu.pop(n); return r;
    }
    //operators, and so on...
    public static unsafe VectorR Create(params BigRational[] values)
    {
      var n = values.Length; if (n == 0) return default;
      //var cpu = rat.task_cpu; for (int i = 0; i < n; i++) cpu.push(values[i]);
      //var r = Create(cpu, n); cpu.pop(n); return r;
      int a = 0; for (int i = 0; i < n; i++) a += Math.Max(4, ((ReadOnlySpan<uint>)values[i]).Length);
      var p = new uint[n + a]; var w = new Span<uint>(p);
      for (int i = 0, b = n; i < n; i++)
      {
        p[i] = unchecked((uint)(i == 0 ? n : b));
        var s = (ReadOnlySpan<uint>)values[i];
        if (s.Length == 0) { var t = (1u, 0u, 1u, 1u); s = new ReadOnlySpan<uint>(&t.Item1, 4); }
        s.CopyTo(w.Slice(b)); b += s.Length;
      }
      return new VectorR(p);
    }
    //public static unsafe VectorR Create(IEnumerable<BigRational> values)
    //{
    //  int n = 0, a = 0;
    //  foreach (var z in values) { a += Math.Max(4, ((ReadOnlySpan<uint>)z).Length); n++; }
    //  if (n == 0) return default;
    //  var p = new uint[n + a]; int i = 0, b = n; var w = new Span<uint>(p);
    //  foreach (var z in values) //for (int i = 0, b = n; i < n; i++)
    //  {
    //    p[i] = unchecked((uint)(i == 0 ? n : b)); i++;
    //    var s = (ReadOnlySpan<uint>)z;
    //    if (s.Length == 0) { var t = (1u, 0u, 1u, 1u); s = new ReadOnlySpan<uint>(&t.Item1, 4); }
    //    s.CopyTo(w.Slice(b)); b += s.Length;
    //  }
    //  return new VectorR(p);
    //}
    public static VectorR Create(BigRational.CPU cpu, int count)
    {
      uint c = checked((uint)count), m = cpu.mark() - c; int a = 0;
      for (uint i = 0; i < count; i++)
      {
        cpu.norm(unchecked((int)(count - i - 1)));
        cpu.get(m + i, out ReadOnlySpan<uint> s); a += s.Length;
      }
      var p = new uint[count + a]; var w = new Span<uint>(p);
      for (uint i = 0, b = c; i < count; i++)
      {
        p[i] = i == 0 ? c : b; cpu.get(m + i, out ReadOnlySpan<uint> s);
        s.CopyTo(w.Slice(unchecked((int)b))); b += unchecked((uint)s.Length);
      }
      return new VectorR(p);
    }
    public static VectorR Min(VectorR a, VectorR b)
    {
      return minmax(a, b, -1);
    }
    public static VectorR Max(VectorR a, VectorR b)
    {
      return minmax(a, b, +1);
    }
    public IEnumerator<BigRational> GetEnumerator()
    {
      for (int i = 0, n = Length; i < n; i++) yield return this[i];
    }
    Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #region private
    readonly uint[] p; // own storage: count, index[0 .. count - 1], rat[0 .. count - 1]
    VectorR(uint[] p) => this.p = p;
    ReadOnlySpan<uint> getat(int i)
    {
      return new ReadOnlySpan<uint>(p).Slice((int)p[i], (int)((i < p[0] - 1 ? p[i + 1] : p.Length) - p[i]));
    }
    static VectorR minmax(VectorR a, VectorR b, int sig)
    {
      int na, nb, n = Math.Max(na = a.Length, nb = b.Length);
      var cpu = rat.task_cpu;
      for (int i = 0; i < n; i++)
      {
        cpu.push((i < na ? a : b).getat(i));
        cpu.push((i < nb ? b : a).getat(i));
        if (cpu.cmp() == sig) cpu.swp(); cpu.pop();
      }
      var r = Create(cpu, n); cpu.pop(n); return r;
    }
    #endregion
  }
}
