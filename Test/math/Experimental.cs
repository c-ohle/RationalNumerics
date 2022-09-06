
using System.Numerics;
using System.Reflection;

namespace Test
{
  /// <summary>
  /// MathR experimental
  /// </summary>
  public static class Experimental
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

#if false
    //var code = Experimental.TypeGen( "__float80", typeof(System.Numerics.Generic.Float<Test.SizeType80>));
    public static string TypeGen(string name, Type type)
    {
      var ss = $"public readonly struct {name}";
      ss += $" : {"IComparable"}<{name}>, {"IEquatable"}<{name}>, {"IComparable"}, {"IFormattable"}, {"ISpanFormattable"}";
      ss += "\r\n"; ss += "{"; ss += "\r\n";
      var mm = type.GetMembers(); var tag = trans(type, false);
      foreach (var m in mm.OfType<ConstructorInfo>())
      {
        var pa = m.GetParameters();
        var v = string.Join(", ", pa.Select(p => para(p)));
        var s = string.Join(", ", pa.Select(p => parb(p)));
        s = $"{"public"} {name}({v}) => new {name}(new {tag}({s}))";
        ss += $"  {s};\r\n";
      }
      ss += "\r\n";
      for (int i = 0; i < 2; i++)
      {
        foreach (var m in mm.OfType<MethodInfo>().Where(p => p.IsSpecialName && p.Name.EndsWith("it") &&
          (p.ReturnType == type) == (i == 0)))
        {
          var op = m.Name; op = op == "op_Implicit" ? "implicit" : "explicit";
          var t = m.ReturnType; var pa = m.GetParameters().First();
          var s = pa.Name; if (t == type) s = $"{"new"} {name}(({tag}){s})";
          else s = $"({trans(t, false)}){s}.p";
          s = $"{trans(t, true)}({para(pa)}) => {s}";
          s = $"public static {op} {"operator"} {s}";
          ss += $"  {s};\r\n";
        }
        ss += "\r\n";
      }
      for (int i = 0; i < 2; i++)
      {
        foreach (var m in mm.OfType<MethodInfo>().Where(p => p.IsSpecialName && (i == 0) == (p.ReturnType == type)))
        {
          var op = m.Name; switch (op)
          {
            case "op_UnaryPlus": op = "+"; break;
            case "op_UnaryNegation": op = "-"; break;
            case "op_Addition": op = "+"; break;
            case "op_Subtraction": op = "-"; break;
            case "op_Multiply": op = "*"; break;
            case "op_Division": op = "/"; break;
            case "op_Modulus": op = "%"; break;
            case "op_Equality": op = "=="; break;
            case "op_Inequality": op = "!="; break;
            case "op_GreaterThanOrEqual": op = ">="; break;
            case "op_LessThanOrEqual": op = "<="; break;
            case "op_GreaterThan": op = ">"; break;
            case "op_LessThan": op = "<"; break;
            default:
              if (op.StartsWith("get")) continue;
              if (op.EndsWith("it")) continue;
              //if (!op.StartsWith("op")) continue; 
              break;
          }
          var pp = m.GetParameters(); var pb = pp.Select(p => p.ParameterType == type ? p.Name + ".p" : p.Name).ToArray();
          var s = pp.Length == 1 ? $"{op}{pb[0]}" : $"{pb[0]} {op} {pb[1]}";
          if (m.ReturnType == type) s = $"{"new"} {name}({s})";
          s = $"public static {trans(m.ReturnType, true)} {"operator"} {op}({string.Join(", ", pp.Select(p => para(p)))}) => {s}";
          ss += $"  {s};\r\n";
        }
        ss += "\r\n";
      }
      foreach (var m in mm.OfType<PropertyInfo>())
      {
        var s = $"{tag}.{m.Name}"; if (m.PropertyType == type) s = $"new {name}({s})";
        s = $"{"public"} {"static"} {trans(m.PropertyType, true)} {m.Name} => {s}";
        ss += $"  {s};\r\n";
      }
      ss += "\r\n";
      foreach (var m in mm.OfType<MethodInfo>().Where(p => p.IsStatic && !p.IsSpecialName))
      {
        if (m.IsGenericMethod) continue; //todo: needs?
        var pa = m.GetParameters(); if (pa.Any(p => p.ParameterType != type && p.ParameterType.IsGenericType)) continue;
        var u = string.Join(", ", pa.Select(p => para(p))); var t = m.ReturnType;
        var s = string.Join(", ", pa.Select(p => p.ParameterType == type ? p.Name + ".p" : p.Name));
        s = $"{tag}.{m.Name}({s})"; if (t == type) s = $"{"new"} {name}({s})";
        s = $"public static {trans(t, true)} {m.Name}({u}) => {s}"; ss += $"  {s};\r\n";
      }
      ss += "\r\n";
      foreach (var m in mm.OfType<MethodInfo>().Where(p => !p.IsStatic))
      {
        var t = m.ReturnType; if (t == typeof(Type)) continue; var pa = m.GetParameters();
        var v = string.Join(", ", pa.Select(p => para(p)));
        var s = string.Join(", ", pa.Select(p => parb(p)));
        s = $"{trans(t, true)} {m.Name}({v}) => p.{m.Name}({s})";
        if (m.IsVirtual && !m.IsFinal) s = $"{"override"} {s}";
        s = $"{"public"} {s}"; ss += $"  {s};\r\n";
      }
      ss += "\r\n";
      {
        var s = $"{name}({tag} p) => this.p = p"; ss += $"  {s};\r\n";
        s = $"private readonly {tag} p"; ss += $"  {s};\r\n";
      }
      ss += "}"; return ss;

      string para(ParameterInfo p)
      {
        var t = p.ParameterType; if (t.IsByRef) t = t.GetElementType()!;
        var s = trans(t, true);

        if (!t.IsValueType && (p.IsOptional || t.IsInterface || t == typeof(object) || p.Name == "format")) s += '?';

        //if (!t.IsValueType && (p.IsOptional || p.Name == "obj" || p.CustomAttributes.Any(x => x.AttributeType.Name== "NullableAttribute"))) s += '?'; 

        s = $"{s} {p.Name}"; if (p.IsOut) s = $"{"out"} {s}";
        if (p.HasDefaultValue)
        {
          var v = p.DefaultValue?.ToString(); if (t.IsEnum) v = $"{t.FullName}.{v}";
          s = $"{s} = {(v != null ? v : "null")}";
        }
        return s;
      }
      string parb(ParameterInfo p)
      {
        var s = p.Name!; if (p.IsOut) s = $"{"out"} {s}";
        if (p.ParameterType == type) s = $"{s} {".p"}"; return s;
      }
      string trans(Type t, bool b)
      {
        if (t == type && b) return name;
        if (t.IsGenericType)
        {
          var s = string.Join(", ", t.GetGenericArguments().Select(p => trans(p, false)));
          s = $"{t.Name.Split('`')[0]}<{s}>"; if (char.IsUpper(s[0])) s = $"{t.Namespace}.{s}";
          return s;
        }
        if (t.IsEnum) return $"{t.Namespace}.{t.Name}";
        switch (Type.GetTypeCode(t))
        {
          case TypeCode.String: return "string";
          case TypeCode.Boolean: return "bool";
          case TypeCode.Char: return "char";
          case TypeCode.Byte: return "byte";
          case TypeCode.Int16: return "short";
          case TypeCode.Int32: return "int";
          case TypeCode.Int64: return "long";
          case TypeCode.UInt16: return "ushort";
          case TypeCode.UInt32: return "uint";
          case TypeCode.UInt64: return "ulong";
          case TypeCode.Single: return "float";
          case TypeCode.Double: return "double";
          case TypeCode.Decimal: return "decimal";
        }
        return $"{t.Namespace}.{t.Name}"; //if (t.IsEnum || t.IsInterface || t.Namespace != "System") return $"{t.Namespace}.{t.Name}";
      }
    }
#endif
  }
}
