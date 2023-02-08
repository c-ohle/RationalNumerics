using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NewNumeric
{
  // Float128 type, under construction
  [Serializable, SkipLocalsInit, StructLayout(LayoutKind.Sequential), DebuggerDisplay("{ToString(\"\"),nq}")]
  public unsafe readonly struct Quadruple
  {
    public static implicit operator Quadruple(double value)
    {
      return default;
    }
    public static explicit operator Quadruple(BigRat value)
    {
      return default;
    }

    public static explicit operator double(Quadruple value)
    {
      return default;
    }
    public static explicit operator BigRat(Quadruple value)
    {
      return default;
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
      return ((BigRat)this).ToString(format, provider);
    }
    public string ToString(string? format)
    {
      return ToString(format, default(IFormatProvider));
    }
    public string ToString(IFormatProvider? provider)
    {
      return ToString(default(string), provider);
    }
    public override string ToString()
    {
      return ToString(default(string), default(IFormatProvider));
    }

    readonly ulong h, l;
  }

}
