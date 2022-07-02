#pragma warning disable CS8625,CS8604,CS8625,CS8603,CS8602,CS8600,CS8620,CS8605,CS8601,CS8618
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Test
{
  /// <summary>
  /// Helpful small extension for System.Linq.Expressions
  /// </summary>
  public static class ExpressionsCompiler
  {
    /// <summary>
    /// Compiles C# synatx scripts to System.Linq.Expression tree to generate runtime class extensions.
    /// </summary>
    /// <param name="code">C# code to compile.</param>
    /// <param name="this">Type of a class to extend; default null for static.</param>
    /// <param name="usings">default usings.</param>
    /// <param name="dbg">generate debug infos.</param>
    /// <returns>Factory object that to create instances.</returns>
    public static Expression<Creator> Compile(string code, Type @this = null, string[]? usings = null, Debugger? dbg = null)
    {
      //return new Compiler().Compile(@this, code, dbg);
      if (Compiler.wr?.Target is not Compiler p)
        Compiler.wr = new WeakReference(p = new Compiler());
      return p.Compile(@this ?? typeof(object), code, usings, dbg);
    }
    public delegate object[] Creator(object @this);
    public delegate object Exchange(string name, object value);
    public delegate void Accessor(Exchange func);
    public delegate Expression Debugger(int i, int n, object p);
    unsafe class Compiler : List<ParameterExpression>
    {
      [ThreadStatic] internal static WeakReference? wr;
      ParameterExpression @this; int npub, nfix, nusings;
      Span epo; Debugger dbg; internal char* code;
      List<object> usings = new List<object>();
      List<Expression> list = new List<Expression>();
      Assembly[] assemblys; Type[] extensions;
      internal Expression<Creator> Compile(Type @this, string code, string[]? usings, Debugger dbg)
      {
        if (this.code != null) { this.Clear(); this.usings.Clear(); list.Clear(); npub = nfix = nusings = 0; }
        var n = code.Length; var mem = Marshal.AllocCoTaskMem((n + 1) << 1);
        var s = this.code = (char*)mem.ToPointer();
        try
        {
          fixed (char* p = code) //Native.memcpy(s, p, (n + 1) << 1); 
            for (int i = 0; i <= n; i++) s[i] = p[i];
          #region remove comments
          for (int i = 0, l = n - 1; i < l; i++)
          {
            var c = s[i]; if (c <= 32) { s[i] = ' '; continue; }
            if (c == '"' || c == '\'') { for (++i; i < n && s[i] != c; i++) if (s[i] == '\\') i++; continue; }
            if (c != '/') continue;
            if (s[i + 1] == '/') { for (; i < n && s[i] != '\n'; i++) s[i] = ' '; continue; }
            if (s[i + 1] == '*') { var t = i; for (; i < l && !(s[i] == '/' && s[i - 1] == '*'); i++) ; for (; t <= i; t++) s[t] = ' '; continue; }
          }
          #endregion
          Span a; a.s = s; a.n = n; a.trim(); this.epo = a;
          this.Add(Expression.Parameter(typeof(object), "this"));
          this.@this = Expression.Variable(@this, "this");
          this.dbg = dbg;
          if (usings != null) { this.usings.AddRange(usings); this.nusings = this.usings.Count; }
          this.npub = 1; this.Parse(a, null, null, null, 0x08 | 0x04);
          var t1 = this.Parse(a, null, null, null, 0x01);
          var t2 = Expression.Lambda<Creator>(t1, ".ctor", this.Take(1));
          return t2;
        }
        catch (System.Exception ex)
        {
          ex.Data.Add("pos", (unchecked((int)(this.epo.s - s)), this.epo.n)); throw;
        }
        finally { Marshal.FreeCoTaskMem(mem); }
      }
      Expression Parse(Span span, LabelTarget @return, LabelTarget @break, LabelTarget @continue, int flags)
      {
        var list = this.list; int stackab = (flags & 1) != 0 ? 1 : this.Count, listab = list.Count, ifunc = 0; var ep = this.epo;
        if ((flags & 0x01) != 0)
        {
          if (dbg != null) // general access to locals and for debug
          {
            var ex = this.Skip(npub).Where(p => /*p.Name[0] != '_' &&*/ !p.Type.IsSubclassOf(typeof(Delegate)));
            if (ex.Any())
            {
              var t1 = Expression.Parameter(typeof(Exchange));
              var t2 = typeof(Compiler).GetMethod(nameof(Compiler.disp), BindingFlags.Static | BindingFlags.NonPublic);
              var t3 = ex.Select(p => Expression.Call(null, t2.MakeGenericMethod(p.Type), t1, Expression.Constant(p.Name), p));
              var t4 = Expression.Lambda<Accessor>(Expression.Block(t3), "#", Enumerable.Repeat(t1, 1));
              var t5 = Expression.Variable(typeof(Accessor), t4.Name);
              list.Add(Expression.Assign(t5, t4)); this.Insert(1, t5); npub++;
            }
          }
          this.Add(this.@this); list.Add(Expression.Assign(this.@this, Expression.Convert(this[0], this.@this.Type)));
          this.nfix = this.Count;
        }
        if (dbg != null && (flags & 0x20) != 0) { var s = span; for (s.n = 1; *s.s != '{'; s.s--) ; brk(s); }
        for (var c = span; c.n != 0;)
        {
          var a = c.block(); if (a.n == 0) continue;
          if (a.s[0] == '{') { if ((flags & 0x08) != 0) continue; a.trim(1, 1); list.Add(Parse(a, @return, @break, @continue, 0x20)); continue; }
          var t = a; var n = t.next(); this.epo = a;
          if (n.equals("using"))
          {
            if ((flags & 0x08) == 0) continue;
            this.usings.Add(t.ToString().Replace(" ", string.Empty));
            this.nusings = this.usings.Count; continue;
          }
          if (n.equals("if"))
          {
            if ((flags & 0x08) != 0) continue;
            n = t.next(); n.trim(1, 1);
            a = c; var l = c; for (; a.n != 0;) { var e = a.next(); if (!e.equals("else")) break; a.block(); l = a; }
            if (l.s != c.s) { a = c; c = l; a.next(); a.n = (int)(l.s - a.s); a.trim(); } else a.n = 0; brk(n);
            var t1 = Parse(n, typeof(bool)); var t2 = Parse(t, @return, @break, @continue, 0);
            list.Add(a.n != 0 ?
              Expression.IfThenElse(t1, t2, Parse(a, @return, @break, @continue, 0)) :
              Expression.IfThen(t1, t2)); continue;
          }
          if (n.equals("for"))
          {
            if ((flags & 0x08) != 0) continue;
            n = t.next(); n.trim(1, 1); var br = Expression.Label("break"); var co = Expression.Label("continue");
            a = n.next(';'); var t1 = this.Count; var t2 = list.Count; Parse(a, null, null, null, 0x04);
            a = n.next(';'); var t3 = a.n != 0 ? Parse(a, typeof(bool)) : null;
            var t4 = list.Count; var t5 = this.Count;
            Parse(t, @return, br, co, 0x04); list.Add(Expression.Label(co));
            for (; n.n != 0;) { var e = n.next(','); Parse(e, null, null, null, 0x04); }
            var t6 = (Expression)Expression.Block(this.Skip(t5), list.Skip(t4)); this.RemoveRange(t5, this.Count - t5); list.RemoveRange(t4, list.Count - t4);
            if (t3 != null && dbg != null) t3 = brk(a, t3);
            if (t3 != null) t6 = Expression.IfThenElse(t3, t6, Expression.Break(br));
            list.Add(Expression.Loop(t6, br)); t6 = Expression.Block(this.Skip(t1), list.Skip(t2));
            this.RemoveRange(t1, this.Count - t1); list.RemoveRange(t2, list.Count - t2);
            list.Add(t6); continue;
          }
          if (n.equals("while"))
          {
            if ((flags & 0x08) != 0) continue;
            n = t.next(); n.trim(1, 1); var br = Expression.Label("break"); var co = Expression.Label("continue");
            var t3 = Parse(n, typeof(bool)); if (dbg != null) t3 = brk(n, t3); var t4 = list.Count; var t5 = this.Count;
            Parse(t, @return, br, co, 0x04); list.Add(Expression.Label(co));
            var t6 = (Expression)Expression.Block(this.Skip(t5), list.Skip(t4)); this.RemoveRange(t5, this.Count - t5); list.RemoveRange(t4, list.Count - t4);
            list.Add(Expression.Loop(Expression.IfThenElse(t3, t6, Expression.Break(br)), br));
            continue;
          }
          if (n.equals("switch"))
          {
            if ((flags & 0x08) != 0) continue;
            n = t.next(); n.trim(1, 1); brk(n); var t1 = Parse(n, null);
            n = t.next(); n.trim(1, 1); var t2 = this.usings.Count; var t5 = (Expression)null;
            for (var ab = list.Count; n.n != 0;)
            {
              for (; n.n != 0;)
              {
                t = n; a = t.next(); if (a.equals("default")) { n = t; n.next(); break; }
                if (!a.equals("case")) break; n = t; list.Add(Convert(Parse(n.next(':'), t1.Type), t1.Type));
              }
              for (t = n; n.n != 0;)
              {
                a = n.block(); if (a.equals("break")) { t.n = (int)(a.s - t.s); t.trim(); break; }
                var s = a; s = s.next(); if (s.equals("return") || s.equals("continue")) { t.n = (int)(a.s - t.s) + a.n; t.trim(); break; }
              }
              var br = Expression.Label(); var t4 = Parse(t, @return, br, @continue, 0x10);
              if (list.Count != ab) this.usings.Add(Expression.SwitchCase(t4, list.Skip(ab))); else t5 = t4;
              list.RemoveRange(ab, list.Count - ab);
            }
            list.Add(Expression.Switch(t1, t5, this.usings.Skip(t2).Cast<SwitchCase>().ToArray()));
            this.usings.RemoveRange(t2, this.usings.Count - t2); continue;
          }
          if (n.equals("return"))
          {
            if ((flags & 0x08) != 0) continue;
            if (@return == null) error(n);
            if (@return.Type != typeof(void))
            {
              var t0 = @return.Type != typeof(Span) ? @return.Type : null;
              var t1 = Parse(t, t0); if (t0 == null) @return.GetType().GetField("_type", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(@return, t0 = t1.Type);
              t1 = Convert(t1, t0); if (dbg != null) { n.n = (int)(t.s - n.s) + t.n; t1 = brk(n, t1); }
              list.Add(Expression.Return(@return, t1)); continue;
            }
            if (t.n != 0) error(n, "invalid type"); brk(n); list.Add(Expression.Return(@return)); continue;
          }
          if (n.equals("break")) { if ((flags & 0x08) != 0) continue; brk(n); list.Add(Expression.Break(@break)); continue; }
          if (n.equals("continue")) { if ((flags & 0x08) != 0) continue; brk(n); list.Add(Expression.Break(@continue)); continue; }
          if (n.equals("new")) { if ((flags & 0x08) != 0) continue; list.Add(Parse(a, null)); continue; }
          var @public = false; if (n.equals("public")) { a = t; @public = true; }
          n = a; t = n.gettype(); if (t.n == 0) { if ((flags & 0x08) != 0) continue; brk(a); list.Add(Parse(a, null)); continue; }
          a = n; n = a.next();
          var type = !t.equals("var") ? GetType(t) : null;
          if (a.n != 0 && a.s[0] == '(')
          {
            int i = 0; if ((flags & 0x01) != 0) for (i = 1; !n.equals(this[i].Name); i++) ;
            var s = n.ToString(); var istack = i; if (i == 0) { istack = @public ? this.npub++ : this.Count; this.Insert(istack, null); }
            t = a.next(); t.trim(1, 1); a.trim(1, 1); var ab = this.Count;
            for (; t.n != 0;) { n = t.next(','); var v = n.gettype(); check(n, ab); this.Add(Expression.Parameter(GetType(v), n.ToString())); if (dbg != null && (flags & 0x08) == 0) map(n, this[this.Count - 1]); }
            var t0 = i != 0 ? this[istack].Type : type != typeof(void) ? Expression.GetFuncType(this.Skip(ab).Select(p => p.Type).Append(type).ToArray()) : Expression.GetActionType(this.Skip(ab).Select(p => p.Type).ToArray());
            var t1 = i != 0 ? this[istack] : Expression.Variable(t0, s); this[istack] = t1;
            if ((flags & 0x08) != 0) { this.RemoveRange(ab, this.Count - ab); continue; }
            var t2 = Expression.Lambda(t0, Parse(a, Expression.Label(type, "return"), null, null, 0x02 | 0x20), s, this.Skip(ab));
            this.RemoveRange(ab, this.Count - ab); list.Insert(++ifunc, Expression.Assign(t1, t2)); continue;
          }
          for (; n.n != 0; n = a.next())
          {
            var v = a.next(','); var b = v.next('='); if (!((flags & 0x01) != 0 && type != null)) check(n, stackab);
            if ((flags & 0x08) != 0) { if (type != null) { this.Add(Expression.Variable(type, n.ToString())); if (dbg != null) map(n, this[this.Count - 1]); } continue; }
            var r = type == null || v.n != 0 ? Parse(v, type) : null;
            int i = 0; if ((flags & 0x01) != 0 && type != null) for (i = 1; !n.equals(this[i].Name); i++) ;
            var e = i != 0 ? this[i] : Expression.Variable(type ?? r.Type, n.ToString());
            //if (map != null) __map(n, 0x04, e.Type);
            if (r != null)
            {
              if (dbg != null) { var u = n; u.n = (int)(v.s - n.s) + v.n; brk(u); }
              list.Add(Expression.Assign(e, Convert(r, e.Type)));
            }
            if (i == 0)
            {
              Debug.Assert((flags & 0x01) == 0);
              //if (dbg != null && (flags & 0x01) != 0) this.Insert(this.nfix++, e); else //todo: check
              this.Add(e); map(n, e);
            }
          }
        }
        this.epo = ep;
        if ((flags & 0x04) != 0) return null;
        if (dbg != null && (flags & 0x20) != 0) { var s = span; for (s = span, s.s += s.n, s.n = 1; *s.s != '}'; s.s++) ; brk(s); }
        if ((flags & 0x02) != 0) list.Add(@return.Type != typeof(void) ? Expression.Label(@return, Expression.Default(@return.Type)) : Expression.Label(@return));
        if ((flags & 0x01) != 0) { list.Add(Expression.NewArrayInit(typeof(object), this.Take(this.npub))); }
        if ((flags & 0x10) != 0) list.Add(Expression.Label(@break));
        var block = this.Count != stackab || list.Count - listab > 1 ? Expression.Block(this.Skip(stackab), list.Skip(listab)) : list.Count - listab == 1 ? list[listab] : Expression.Empty();
        list.RemoveRange(listab, list.Count - listab); this.RemoveRange(stackab, this.Count - stackab);
        return block;
      }
      Expression Parse(Span span, Type wt)
      {
        if (span.n == 0) error(span);
        Span a = span, b = a, c, d; int op = 0, i1 = 0, i2 = 0;
        for (c = a; c.n != 0; i2++)
        {
          var t = c.next(); var o = t.opcode();
          if (o == 0) { i1 = 0; continue; }
          i1++; if (op >> 4 > o >> 4) continue;
          if (o >> 4 == 0x03 && (i2 == 0 || i1 != 1)) { i2--; continue; }
          if ((o == 0xd0 || o == 0xc0) && op == o) continue; // =, ?
          op = o; a.n = (int)(t.s - span.s); a.trim(); b = c;
        }
        if (op != 0)
        {
          if (op == 0xdf) // => 
          {
            if (wt == null) return Expression.Constant(span);
            var me = wt.GetMethod("Invoke"); if (me == null) error(span, "unknown type");
            var pp = me.GetParameters(); if (a.s[0] == '(') a.trim(1, 1); var ab = this.Count;
            int i = 0; for (; a.n != 0 && i < pp.Length; i++) { var n = a.next(','); this.Add(Expression.Parameter(pp[i].ParameterType, n.ToString())); }
            if (a.n != 0 || i != pp.Length) error(span, "invalid param count");
            var g = me.ReturnType.ContainsGenericParameters; Expression r;
            if (b.s[0] == '{') { b.trim(1, 1); r = Parse(b, Expression.Label(g ? typeof(Span) : me.ReturnType, "return"), null, null, 0x02 | 0x20); }
            else r = Parse(b, g ? null : wt);
            if (g) { if (r.Type == typeof(Span)) error(b, "missing return"); var gg = wt.GetGenericArguments(); gg[gg.Length - 1] = r.Type; wt = wt.GetGenericTypeDefinition().MakeGenericType(gg); }
            r = Expression.Lambda(wt, r, this.Skip(ab)); this.RemoveRange(ab, this.Count - ab); return r;
          }
          var ea = Parse(a, null);
          switch (op)
          {
            case 0x54: return Expression.TypeIs(ea, GetType(b));
            case 0x55: return Expression.TypeAs(ea, GetType(b));
            case 0xc0: a = b.next(':'); return Expression.Condition(ea, ea = Parse(a, wt), Parse(b, wt ?? ea.Type));
          }
          var eb = Parse(b, ea.Type);
          if (op == 0x30 && (ea.Type == typeof(string) || eb.Type == typeof(string)))
          {
            if (ea.Type != typeof(string)) ea = Expression.Call(ea, ea.Type.GetMethod("ToString", Type.EmptyTypes));
            if (eb.Type != typeof(string)) eb = Expression.Call(eb, eb.Type.GetMethod("ToString", Type.EmptyTypes));
            return Expression.Call(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }), ea, eb);
          }
          var sp = this.epo; this.epo = span; MethodInfo mo = null;
          switch (op)
          {
            case 0x20: case 0xd1: mo = refop("op_Multiply", ea, eb); break;
            case 0x21: case 0xd2: mo = refop("op_Division", ea, eb); break;
            case 0x22: case 0xd3: mo = refop("op_Modulus", ea, eb); break;
            case 0x30: case 0xd4: mo = refop("op_Addition", ea, eb); break;
            case 0x31: case 0xd5: mo = refop("op_Subtraction", ea, eb); break;
            case 0x70: case 0xd6: mo = refop("op_BitwiseAnd", ea, eb); break;
            case 0x80: case 0xd7: mo = refop("op_ExclusiveOr", ea, eb); break;
            case 0x90: case 0xd8: mo = refop("op_BitwiseOr", ea, eb); break;
            case 0x40: case 0xd9: mo = refop("op_LeftShift", ea, eb); break;
            case 0x41: case 0xda: mo = refop("op_RightShift", ea, eb); break;
            case 0x50: mo = refop("op_LessThan", ea, eb); break;
            case 0x51: mo = refop("op_GreaterThan", ea, eb); break;
            case 0x52: mo = refop("op_LessThanOrEqual", ea, eb); break;
            case 0x53: mo = refop("op_GreaterThanOrEqual", ea, eb); break;
            case 0x60: mo = refop("op_Equality", ea, eb); break;
            case 0x61: mo = refop("op_Inequality", ea, eb); break;
          }
          if (mo != null)
          {
            var pp = mo.GetParameters();
            //if (pp[1].ParameterType.IsByRef && eb is MemberExpression me && me.Member.DeclaringType.IsValueType) 
            //{
            //  var t = Expression.Variable(eb.Type, string.Empty); this.Add(t); eb = Expression.Assign(t, eb);
            //}
            //if ((pp[1].Attributes & ParameterAttributes.In) != 0) { }
            ea = Convert(ea, pp[0].ParameterType);
            eb = Convert(eb, pp[1].ParameterType);
          }
          else if (ea.Type != eb.Type)
          {
            if (Convertible(eb.Type, ea.Type) != null) eb = Convert(eb, ea.Type);
            else if (Convertible(ea.Type, eb.Type) != null) ea = Convert(ea, eb.Type);
            else if ((op & 0xf0) == 0x60 && (ea.Type.IsEnum || eb.Type.IsEnum))
            {
              if (0.Equals((ea as ConstantExpression)?.Value)) ea = Expression.Convert(ea, eb.Type);
              if (0.Equals((eb as ConstantExpression)?.Value)) eb = Expression.Convert(eb, ea.Type);
            }
          }

          wt = op >= 0x70 && op <= 0x90 && ea.Type.IsEnum ? ea.Type : null; // | & ^
          if (wt != null)
          {
            var x = wt.GetEnumUnderlyingType();
            if (ea.Type != x) ea = Expression.Convert(ea, x);
            if (eb.Type != x) eb = Expression.Convert(eb, x);
          }
          switch (op)
          {
            case 0x20: eb = Expression.Multiply(ea, eb, mo); break;
            case 0x21: eb = Expression.Divide(ea, eb, mo); break;
            case 0x22: eb = Expression.Modulo(ea, eb, mo); break;
            case 0x30: eb = Expression.Add(ea, eb, mo); break;
            case 0x31: eb = Expression.Subtract(ea, eb, mo); break;
            case 0x40: eb = Expression.LeftShift(ea, eb, mo); break;
            case 0x41: eb = Expression.RightShift(ea, eb, mo); break;
            case 0x50: eb = Expression.LessThan(ea, eb, false, mo); break;
            case 0x51: eb = Expression.GreaterThan(ea, eb, false, mo); break;
            case 0x52: eb = Expression.LessThanOrEqual(ea, eb, false, mo); break;
            case 0x53: eb = Expression.GreaterThanOrEqual(ea, eb, false, mo); break;
            case 0x60: eb = Expression.Equal(ea, eb, false, mo); break;
            case 0x61: eb = Expression.NotEqual(ea, eb, false, mo); break;
            case 0x70: eb = Expression.And(ea, eb, mo); break;
            case 0x80: eb = Expression.ExclusiveOr(ea, eb, mo); break;
            case 0x90: eb = Expression.Or(ea, eb, mo); break;
            case 0xa0: eb = Expression.OrElse(ea, eb); break;
            case 0xb0: eb = Expression.AndAlso(ea, eb); break;
            case 0xc2: eb = Expression.Coalesce(ea, eb); break;
            case 0xd0: eb = Expression.Assign(ea, eb); break; // bugs in Expressions if (((MemberExpression)ea).Member.DeclaringType.IsValueType) ...
            case 0xd1: eb = Expression.Assign(ea, Expression.Multiply(ea, eb, mo)); break;//Expression.MultiplyAssign(ea, eb);
            case 0xd2: eb = Expression.Assign(ea, Expression.Divide(ea, eb, mo)); break; //Expression.DivideAssign(ea, eb);
            case 0xd3: eb = Expression.Assign(ea, Expression.Modulo(ea, eb, mo)); break;//return Expression.ModuloAssign(ea, eb);
            case 0xd4: eb = Expression.Assign(ea, Expression.Add(ea, eb, mo)); break; //return Expression.AddAssign(ea, eb);//if (((MemberExpression)ea).Member.DeclaringType.IsValueType)
            case 0xd5: eb = Expression.Assign(ea, Expression.Subtract(ea, eb, mo)); break; //return Expression.SubtractAssign(ea, eb);
            case 0xd6: eb = Expression.Assign(ea, Expression.And(ea, eb, mo)); break; //return Expression.AndAssign(ea, eb);
            case 0xd7: eb = Expression.Assign(ea, Expression.ExclusiveOr(ea, eb, mo)); break; //return Expression.ExclusiveOrAssign(ea, eb);
            case 0xd8: eb = Expression.Assign(ea, Expression.Or(ea, eb, mo)); break;//return Expression.OrAssign(ea, eb);
            case 0xd9: eb = Expression.Assign(ea, Expression.LeftShift(ea, eb)); break; //return Expression.LeftShiftAssign(ea, eb);
            case 0xda: eb = Expression.Assign(ea, Expression.RightShift(ea, eb)); break; //return Expression.RightShiftAssign(ea, eb);
          }
          if (wt != null) eb = Expression.Convert(eb, wt);
          this.epo = sp; return eb;
        }
        b = span;
        if (b.n >= 2 && b.s[b.n - 1] == b.s[b.n - 2])
        {
          if (b.s[b.n - 1] == '+') { b.trim(0, 2); return Expression.PostIncrementAssign(Parse(b, wt)); }
          if (b.s[b.n - 1] == '-') { b.trim(0, 2); return Expression.PostDecrementAssign(Parse(b, wt)); }
        }
        a = b.next();
        if (a.n == 1)
        {
          if (a.s[0] == '+') { var t = Parse(b, wt); return t; } //"op_UnaryPlus"
          if (a.s[0] == '-') { var t = Parse(b, wt); return Expression.Negate(t, refop("op_UnaryNegation", t)); }
          if (a.s[0] == '!') { var t = Parse(b, wt); return Expression.Not(t, refop("op_LogicalNot", t)); }
          if (a.s[0] == '~') { var t = Parse(b, wt); return Expression.OnesComplement(t, refop("op_OnesComplement", t)); }
        }
        if (a.n == 2 && a.s[0] == a.s[1])
        {
          if (a.s[0] == '+') return Expression.PreIncrementAssign(Parse(b, wt));
          if (a.s[0] == '-') return Expression.PreDecrementAssign(Parse(b, wt));
        }
        Expression left = null; Type type = null; bool checkthis = false, priv = false;
        if (a.s[0] == '(')
        {
          a.trim(1, 1); if (b.n != 0 && b.s[0] != '.' && b.s[0] != '[') { var t = Parse(b, wt = GetType(a)); return t.Type != wt ? Expression.Convert(t, wt) : t; }
          left = Parse(a, wt); goto eval;
        }
        if (char.IsNumber(a.s[0]) || a.s[0] == '.')
        {
          if (a.n > 1 && a.s[0] == '0' && (a.s[1] | 0x20) == 'x')
          {
            a.trim(2, 0); if (wt == typeof(uint) || (a.n == 8 && a.s[0] > '7')) { left = Expression.Constant(uint.Parse(a.ToString(), NumberStyles.HexNumber)); goto eval; }
            left = Expression.Constant(int.Parse(a.ToString(), NumberStyles.HexNumber)); goto eval;
          }
          var tc = TypeCode.Int32;
          switch (a.s[span.n - 1] | 0x20)
          {
            case 'f': a.trim(0, 1); tc = TypeCode.Single; break;
            case 'd': a.trim(0, 1); tc = TypeCode.Double; break;
            case 'm': a.trim(0, 1); tc = TypeCode.Decimal; break;
            case 'u': a.trim(0, 1); tc = TypeCode.UInt32; break;
            case 'l': a.trim(0, 1); tc = TypeCode.Int64; if ((a.s[span.n - 1] | 0x20) == 'u') { a.trim(0, 1); tc = TypeCode.UInt64; } break;
            default: for (int i = 0; i < a.n; i++) if (!char.IsNumber(a.s[i])) { tc = TypeCode.Double; break; } break;
          }
          if (wt != null) { var v = Type.GetTypeCode(wt); if (v > tc) tc = v; }
          left = Expression.Constant(System.Convert.ChangeType(a.ToString(), tc, CultureInfo.InvariantCulture));
          if (wt != null && wt.IsEnum) if (0.Equals(((ConstantExpression)left).Value)) left = Expression.Convert(left, wt);
          goto eval;
        }
        if (a.s[0] == '"') { var ss = new string(a.s, 1, a.n - 2); if (ss.IndexOf('\\') >= 0) ss = Regex.Unescape(ss); left = Expression.Constant(ss); goto eval; }
        if (a.s[0] == '\'') { if (a.n == 3) { left = Expression.Constant(a.s[1]); goto eval; } var ss = Regex.Unescape(new string(a.s, 1, a.n - 2)); if (ss.Length != 1) error(a); left = Expression.Constant(ss[0]); goto eval; }
        if (a.equals("true")) { left = Expression.Constant(true); goto eval; }
        if (a.equals("false")) { left = Expression.Constant(false); goto eval; }
        if (a.equals("null")) { left = Expression.Constant(null, wt ?? typeof(object)); goto eval; }
        if (a.equals("default")) { left = Expression.Default(wt ?? typeof(object)); goto eval; }
        if (a.equals("typeof")) { a = b.next(); a.trim(1, 1); left = Expression.Constant(GetType(a)); goto eval; }
        if (a.equals("new"))
        {
          for (a = b; b.n != 0 && b.s[0] != '(' && b.s[0] != '[' && b.s[0] != '{'; b.next()) ;
          a.n = (int)(b.s - a.s); a.trim(); var t = GetType(a); a = b.next(); var tc = a.s[0]; a.trim(1, 1);
          if (tc == '[') while (a.n == 0 && b.s[0] == '[') { t = t.MakeArrayType(); a = b.next(); a.trim(1, 1); }
          var ab
            = this.list.Count; if (tc != '{') for (; a.n != 0;) this.list.Add(Parse(a.next(','), null));
          if (tc == '[')
          {
            if (ab != this.list.Count) left = Expression.NewArrayBounds(t, this.list.Skip(ab));
            else
            {
              a = b.next(); if (a.n == 0 || a.s[0] != '{') error(a);
              for (a.trim(1, 1); a.n != 0;) this.list.Add(Convert(Parse(a.next(','), null), t));
              left = Expression.NewArrayInit(t, this.list.Skip(ab));
            }
          }
          else
          {
            if (ab == this.list.Count) left = Expression.New(t);
            else
            {
              var ct = GetMember(t, null, BindingFlags.Instance | BindingFlags.Public, ab, this.usings.Count) as ConstructorInfo;
              if (ct == null) error(span, "invalid ctor");
              left = Expression.New(ct, this.list.Skip(ab));
            }
            if (b.n != 0 && b.s[0] == '{') { a = b.next(); a.trim(1, 1); tc = '{'; }
            if (tc == '{')
            {
              var ic = left.Type.GetInterface("ICollection`1"); var ns = this.list.Count;
              if (ic != null)
              {
                for (t = ic.GetGenericArguments()[0]; a.n != 0;) this.list.Add(Convert(Parse(a.next(','), t), t));
                left = Expression.ListInit((NewExpression)left, this.list.Skip(ns));
              }
              else
              {
                var pp = this.usings; var np = pp.Count;//var list = new List<MemberAssignment>();
                for (; a.n != 0;) { var p = a.next(','); var e = Expression.PropertyOrField(left, p.next('=').ToString()); pp.Add(Expression.Bind(e.Member, Convert(Parse(p, e.Type), e.Type))); }
                left = Expression.MemberInit((NewExpression)left, pp.Skip(np).Cast<MemberAssignment>()); pp.RemoveRange(np, pp.Count - np);
              }
            }
          }
          this.list.RemoveRange(ab, this.list.Count - ab); goto eval;
        }
        for (int i = this.Count - 1; i >= 0; i--) if (a.equals(this[i].Name)) { left = this[i]; map(a, this[i]); goto eval; }
        var sa = a.ToString();
        for (d = a, c = b; c.n != 0 && (c.s[0] == '.' || c.s[0] == '·');)
        {
          if ((type = GetType(d, false)) != null) { b = c; goto eval; }
          c.next(); c.next(); d = a; d.n = (int)(c.s - d.s); d.trim();
        }
        left = this.@this; checkthis = true; eval:
        for (; b.n != 0 || checkthis; type = null, checkthis = priv = false)
        {
          if (checkthis) goto call; a = b.next();
          if (a.s[0] == '[')
          {
            a.trim(1, 1); var ab = this.list.Count; for (; a.n != 0;) this.list.Add(Parse(a.next(','), null));
            var lt = left.Type;
            if (lt.IsArray) left = Expression.ArrayAccess(left, this.list.Skip(ab));
            else left = Expression.Property(left, lt == typeof(string) ? "Chars" : "Item", this.list.Skip(ab).ToArray());
            this.list.RemoveRange(ab, this.list.Count - ab); continue;
          }
          if (a.s[0] == '(')
          {
            a.trim(1, 1); var ab = this.list.Count; for (; a.n != 0;) this.list.Add(Parse(a.next(','), null));
            var pb = left.Type.GetMethod("Invoke").GetParameters(); if (pb.Length != this.list.Count - ab) error(a, "invalid param count");
            for (int i = 0; i < pb.Length; i++) this.list[ab + i] = Convert(this.list[ab + i], pb[i].ParameterType);
            left = Expression.Invoke(left, this.list.Skip(ab)); this.list.RemoveRange(ab, this.list.Count - ab); continue;
          }
          if (a.n != 1 || !(a.s[0] == '.' || (priv = a.s[0] == '·'))) error(a);
          a = b.next(); call:
          var bf = (type != null ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public;
          var t1 = type ?? left.Type; if (priv || t1 == this.@this.Type) bf |= BindingFlags.NonPublic | BindingFlags.FlattenHierarchy; // BindingFlags.NonPublic;
          var s = a.ToString();
          if (b.n > 1 && (b.s[0] == '(' || b.s[0] == '<'))
          {
            var ng = this.usings.Count;
            if (b.s[0] == '<') { c = b.next(); c.trim(1, 1); for (; c.n != 0;) this.usings.Add(GetType(c.next(','))); }
            c = b.next(); c.trim(1, 1); var ab = this.list.Count;
            for (; c.n != 0;) { var t = c.next(','); if (t.take("ref") || t.take("out")) { } this.list.Add(Parse(t, null)); }
            var me = GetMember(t1, s, bf, ab, ng);
            if (me == null && checkthis) { left = null; }
            if (me == null && left != null)
            {
              this.list.Insert(ab, left); left = null;
              if (extensions == null)
              {
                extensions = new Type[] { typeof(Enumerable), typeof(MemoryExtensions), typeof(ParallelEnumerable) };
                //extensions = assemblys.SelectMany(p => p.GetExportedTypes()).Where(p => p.GetCustomAttribute(typeof(System.Runtime.CompilerServices.ExtensionAttribute)) != null).ToArray();
              }
              for (int i = 0; i < extensions.Length; i++) if ((me = GetMember(extensions[i], s, BindingFlags.Static | BindingFlags.Public, ab, ng)) != null) break; ;
            }
            if (me == null) { a.n = (int)(b.s - a.s); a.trim(); error(a, "unknown method" + " " + a.ToString()); }
            map(a, me);
            left = Expression.Call(left, (MethodInfo)me, this.list.Skip(ab)); this.list.RemoveRange(ab, this.list.Count - ab); this.usings.RemoveRange(ng, this.usings.Count - ng);
            if (type == typeof(Debugger) && !System.Diagnostics.Debugger.IsAttached && (me.GetCustomAttribute(typeof(ConditionalAttribute)) as ConditionalAttribute)?.ConditionString == "DEBUG") left = Expression.Empty();
            continue;
          }
          var x = t1.GetMember(s, MemberTypes.Property | MemberTypes.Field, bf);
          if (x.Length != 0) { left = Expression.MakeMemberAccess(left, x[0]); map(a, x[0]); continue; }
          if ((bf & BindingFlags.Static) != 0) { type = t1.GetNestedType(s, bf); if (type != null) goto eval; }
          left = null; break;
        }
        if (left == null || checkthis) error(a);
        return left;
      }
      MethodBase GetMember(Type type, string name, BindingFlags bf, int xt, int xg)
      {
        var mm = type.GetMember(name ?? ".ctor", name != null ? MemberTypes.Method : MemberTypes.Constructor, bf); if (mm.Length == 0) return null;
        var me = (MethodBase)null; var pp = (ParameterInfo[])null; var vt = (Type)null; var tt = this.list; int nt = tt.Count - xt, best = int.MaxValue;
        for (int i = 0; i < mm.Length; i++)
        {
          var mt = (MethodBase)mm[i];
          if (this.usings.Count > xg) if (!mt.IsGenericMethod || mt.GetGenericArguments().Length != this.usings.Count - xg) continue;
          var pt = mt.GetParameters(); var lt = pt.Length;
          var at = lt != 0 && pt[lt - 1].ParameterType.IsArray && pt[lt - 1].IsDefined(typeof(ParamArrayAttribute), false) ? pt[lt - 1].ParameterType : null;
          if (at != null) if (lt == nt && Convertible(tt[xt + lt - 1].Type, at) != null) at = null; else at = at.GetElementType();
          if (lt < nt && !(at != null && nt >= lt - 1)) continue;
          if (lt > nt && !(pt[nt].HasDefaultValue || (at != null && nt == lt - 1))) continue;
          int t1 = 0, t2 = 0, conv = 0;
          for (int t = 0; t < nt; t++)
          {
            var p1 = tt[xt + t].Type; var p2 = at != null && t >= lt - 1 ? at : pt[t].ParameterType;
            if (p1 == p2) { t1++; continue; }
            if (p2.IsPointer) break;
            if (p2.IsByRef && p2.GetElementType() == p1) { t1++; continue; }
            if (p1 == typeof(Span))
            {
              var a = p2.GetMethod("Invoke")?.GetParameters(); if (a == null) break;
              var b = (Span)((ConstantExpression)tt[xt + t]).Value; b = b.next();
              if (b.s[0] == '(') b.trim(1, 1); int c = 0; for (; b.n != 0; b.next(','), c++) ;
              if (c != a.Length) break; t1++; continue;
            }
            if (p2.ContainsGenericParameters) { t2++; continue; }
            if (p1 == typeof(object) && tt[xt + t] is ConstantExpression && ((ConstantExpression)tt[xt + t]).Value == null) { t1++; continue; } //null
            if (Convertible(p1, p2) != null) { conv += Math.Abs(Type.GetTypeCode(p1) - Type.GetTypeCode(p2)); t2++; continue; }
            break;
          }
          if (t1 + t2 < nt) continue;
          if (conv > best) continue; best = conv;
          if (at != null && me != null) continue;
          me = mt; pp = pt; vt = at; if (t1 == nt && at == null) break;
        }
        if (me == null) return null;
        if (me.IsGenericMethod)
        {
          var aa = me.GetGenericArguments();
          if (this.usings.Count > xg)
            for (int t = 0; t < aa.Length; t++) aa[t] = (Type)this.usings[xg + t];
          else
          {
            for (int i = 0, j, f = 0; i < pp.Length && f < aa.Length; i++)
            {
              var t = pp[i].ParameterType; if (!t.ContainsGenericParameters) continue;
              var s = tt[xt + i].Type;
              if (!t.IsConstructedGenericType)
              {
                if (t.HasElementType) t = t.GetElementType();
                for (j = 0; j < aa.Length && !(aa[j].Name == t.Name && aa[j].ContainsGenericParameters); j++) ;
                if (j == aa.Length) continue; aa[j] = s; f++; continue;
              }
              var a = t.GetGenericArguments();
              if (s == typeof(Span))
              {
                for (j = 0; j < aa.Length && !(aa[j].Name == a[a.Length - 1].Name && aa[j].ContainsGenericParameters); j++) ;
                if (j == aa.Length) continue; var bb = me.GetGenericArguments();
                for (int x = 0, y; x < a.Length; x++) for (y = 0; y < bb.Length; y++) if (bb[y].Name == a[x].Name && a[x].ContainsGenericParameters) { a[x] = aa[y]; break; }
                var r = Parse(((Span)((ConstantExpression)tt[xt + i]).Value), t.GetGenericTypeDefinition().MakeGenericType(a));
                tt[xt + i] = r; aa[j] = ((LambdaExpression)r).ReturnType; f++; continue;
              }
              //if (!s.IsGenericType) { s = s.GetInterface(t.Name); if (s == null) continue; }
              if (t.IsInterface && t.Name != s.Name) { s = s.GetInterface(t.Name); if (s == null) continue; }
              var v = s.GetGenericArguments(); if (v.Length != a.Length) continue;
              for (int x = 0; x < a.Length; x++) for (int y = 0; y < aa.Length; y++) if (aa[y].Name == a[x].Name && aa[y].ContainsGenericParameters) { aa[y] = v[x]; f++; break; }
            }
          }
          me = ((MethodInfo)me).MakeGenericMethod(aa); pp = me.GetParameters();
        }
        for (int i = 0, n = vt != null ? pp.Length - 1 : pp.Length; i < n; i++)
        {
          var p = pp[i]; var t = p.ParameterType;
          if (i >= nt)
          {
            if (p.DefaultValue == null) { tt.Add(Expression.Default(t.IsByRef ? t.GetElementType() : t)); continue; }
            tt.Add(Expression.Constant(p.DefaultValue, t)); continue;
          }
          var pa = tt[xt + i];
          if (pa.Type == typeof(Span)) { pa = tt[xt + i] = Parse((Span)((ConstantExpression)pa).Value, t); continue; }
          tt[xt + i] = Convert(pa, t);
        }
        if (vt != null)
        {
          xt += pp.Length - 1; for (int i = xt; i < tt.Count; i++) tt[i] = Convert(tt[i], vt);
          var e = Expression.NewArrayInit(vt, tt.Skip(xt)); tt.RemoveRange(xt, tt.Count - xt); tt.Add(e);
        }
        return me;
      }
      Type GetType(Span a, bool ex = true)
      {
        if (a.s[a.n - 1] == ']')
        {
          var p = a; for (; ; ) { var x = p; x.next(); if (x.n == 0) break; p = x; }
          a.n = (int)(p.s - a.s); a.trim(); return GetType(a, ex).MakeArrayType();
        }
        if (a.s[a.n - 1] == '>')
        {
          var p = a; for (; p.n != 0 && p.s[0] != '<'; p.next()) ; a.n = (int)(p.s - a.s); a.trim(); p.trim(1, 1);
          int n = 0; for (var z = p; z.n != 0; z.next(','), n++) ;
          var u = GetType($"{a}`{n}"); if (u == null && ex) error(a, "unknown type");
          var w = new Type[n]; for (int i = 0; p.n != 0; i++) w[i] = GetType(p.next(',')); return u.MakeGenericType(w);
        }
        static Type std(Span a)
        {
          if (a.equals("void")) return typeof(void);
          if (a.equals("bool")) return typeof(bool);
          if (a.equals("char")) return typeof(char);
          if (a.equals("sbyte")) return typeof(sbyte);
          if (a.equals("byte")) return typeof(byte);
          if (a.equals("int")) return typeof(int);
          if (a.equals("uint")) return typeof(uint);
          if (a.equals("short")) return typeof(short);
          if (a.equals("ushort")) return typeof(ushort);
          if (a.equals("long")) return typeof(long);
          if (a.equals("ulong")) return typeof(ulong);
          if (a.equals("decimal")) return typeof(decimal);
          if (a.equals("float")) return typeof(float);
          if (a.equals("double")) return typeof(double);
          if (a.equals("string")) return typeof(string);
          if (a.equals("object")) return typeof(object);
          return null;
        }
        var t = std(a) ?? GetType(a.ToString()); if (t != null) map(a, t);
        if (t == null && ex) error(a, "unknown type");
        return t;
      }
      Type GetType(string s)
      {
        if (s.Contains(' ')) s = s.Replace(" ", string.Empty);
        var ss = this.usings; var nu = this.nusings; Type t;
        for (int i = 0; i < nu; i++) { var u = (string)ss[i]; if (u.StartsWith(s) && (u.Length == s.Length || (u.Length < s.Length && s[u.Length] == '.'))) return null; }
        var a = assemblys ??= AppDomain.CurrentDomain.GetAssemblies();
        for (; ; )
        {
          var x = s.LastIndexOf('.');
          if (x != -1) for (int i = 0; i < a.Length; i++) if ((t = a[i].GetType(s)) != null) return t;
          if (x == -1) for (int i = 0; i < a.Length; i++) for (int k = 0; k < nu; k++) if ((t = a[i].GetType($"{(string)ss[k]}.{s}")) != null) return t;
          if (x == -1) return null; s = s.Substring(0, x) + '+' + s.Substring(x + 1);
        }
      }

      void check(Span s, int stackab)
      {
        if (!s.isname()) error(s);
        for (int i = stackab; i < this.Count; i++) if (s.equals(this[i].Name)) error(s, "duplicate name");
      }
      void error(Span s, string msg = "syntax")
      {
        if (s.n <= 0) s.n = 1; this.epo = s; throw new SyntaxException(msg);
      }

      static MethodInfo refop(string op, Expression a)
      {
        Type t1 = a.Type; if (t1.IsPrimitive) return null;
        var me = t1.GetMethod(op, BindingFlags.Static | BindingFlags.Public, null, new Type[] { t1.MakeByRefType() }, null); if (me != null) return me;
        return null;
      }
      static MethodInfo refop(string op, Expression a, Expression b)
      {
        Type t1 = a.Type, t2 = b.Type; if (t1 == t2 && t1.IsPrimitive) return null;
        MethodInfo match = null;
        for (int i = 0; i < 2; i++)
        {
          var mm = (i == 0 ? t1 : t2).GetMember(op, MemberTypes.Method, BindingFlags.Static | BindingFlags.Public);
          for (int k = 0; k < mm.Length; k++)
          {
            var m = (MethodInfo)mm[k]; var pp = m.GetParameters();
            if (pp[0].ParameterType == t1 && pp[1].ParameterType == t2) return m;
            if (Convertible(t1, pp[0].ParameterType) == null) continue;
            if (Convertible(t2, pp[1].ParameterType) == null) continue;
            match = m;
          }
          if (t1 == t2) break;
        }
        return match;
      }
      static Expression Convert(Expression a, Type t)
      {
        if (a.Type == t) return a;
        if (t == typeof(object) && !a.Type.IsClass) return Expression.Convert(a, t);
        if (t.IsAssignableFrom(a.Type)) return a;
        if (t.IsByRef)
        {
          t = t.GetElementType();
          //if (a.Type == t && a is MemberExpression ex && ex.Member.DeclaringType.IsValueType) //bypass Expressions bug, ref access to struct member  
          //{
          //  var v = Expression.Variable(t, string.Empty); stack.Add(v);
          //  return Expression.Assign(v, a);
          //}
          return Convert(a, t);
        }
        if (a is ConstantExpression c)
        {
          var v = c.Value; if (v == null) return Expression.Constant(v, t);
          if (t.IsPrimitive && v is IConvertible) return Expression.Constant(System.Convert.ChangeType(v, t), t);
        }
        var x = Convertible(a.Type, t); if (x == null) throw new SyntaxException("invalid conversion");
        if (x is MethodInfo me) return Expression.Convert(Convert(a, me.GetParameters()[0].ParameterType), t, me);
        return Expression.Convert(a, t);
      }
      static object Convertible(Type ta, Type tb, bool imp = true)
      {
        if (tb.IsAssignableFrom(ta)) return tb;
        int b = (int)Type.GetTypeCode(tb) - 4;
        if (b < 0)
        {
          if (tb.IsByRef) return Convertible(ta, tb.GetElementType(), imp);
          if (!imp) return null;
          if (!ta.IsValueType || !tb.IsValueType) return null; //Debug.WriteLine(ta + " " + tb);
          for (int j = 0; j < 2; j++)
          {
            var aa = (j == 0 ? tb : ta).GetMember("op_Implicit", MemberTypes.Method, BindingFlags.Static | BindingFlags.Public);
            for (int i = 0; i < aa.Length; i++)
              if (Convertible(ta, ((MethodBase)aa[i]).GetParameters()[0].ParameterType, false) != null) return aa[i];
          }
          return null;
        }
        int a = (int)Type.GetTypeCode(ta) - 4, m;
        switch (a)
        {
          case 0: m = 0b111111110000; break;
          case 1: m = 0b111010101000; break;
          case 2: m = 0b111111111001; break;
          case 3: m = 0b111010100000; break;
          case 4: m = 0b111111100000; break;
          case 5: m = 0b111010000000; break;
          case 6: m = 0b111110000000; break;
          case 7: m = 0b111000000000; break;
          case 8: m = 0b111000000000; break;
          case 9: m = 0b010000000000; break;
          default: return null;
        }
        return (m & (1 << b)) != 0 ? tb : null;
      }

      void map(Span s, object p)
      {
        if (dbg == null) return;
        dbg((int)(s.s - code), s.n, p);
      }
      Expression brk(Span s, Expression b = null)
      {
        if (dbg == null) return b;
        var a = dbg((int)(s.s - code), s.n, this.Skip(this.nfix));
        if (a == null) return b;
        if (b == null) this.list.Add(a);
        else a = Expression.Block(a, b); return a;
      }

      static void disp<T>(Exchange func, string name, ref T value)
      {
        var p = func(name, null); if (p == null) return;
        if (p == (object)name) { func(name, value); return; } // get
        if (p == (object)typeof(Type)) p = func(name, typeof(T));
        if (p is T t) { value = t; return; } // set
        if (p == (object)typeof(DBNull)) { value = default; return; } // set default       
      }

      struct Span
      {
        internal char* s; internal int n;
        public override string ToString() => new string(s, 0, n);
        internal Span next()
        {
          var a = this; a.n = token(); s += a.n; n -= a.n; trim(); return a;
        }
        internal Span next(char c)
        {
          var p = this; for (; n != 0;) { var t = next(); if (t.n == 1 && t.s[0] == c) { p.n = (int)(t.s - p.s); p.trim(); break; } }
          return p;
        }
        internal Span block()
        {
          var p = this;
          for (var e = true; n != 0;)
          {
            //if (s[0] == ')' || s[0] == '}') { n = 1; error(); }
            var t = next(); if (t.s[0] == ';') { p.n = (int)(t.s - p.s); p.trim(); break; }
            if (e && t.s[t.n - 1] == '}') { p.n = (int)(t.s + t.n - p.s); p.trim(); break; }
            if (t.n == 3 && t.equals("new")) e = false;
          }
          return p;
        }
        internal Span gettype()
        {
          for (var t = this; ;)
          {
            if (this.n == 0) throw new SyntaxException();
            var a = next(); if (n != 1 && s[0] == '.') { next(); continue; }
            if (a.isname() && (isname() || (n != 1 && (s[0] == '<' || s[0] == '['))))
            {
              //if (debugger != null && a.s != t.s) { for (int i = 0; i < keywords.Length; i++) if (a.equals(keywords[i])) goto r; }
              if (s[0] == '<') next();
              while (n != 0 && s[0] == '[') { var i = next(); i.trim(1, 1); if (i.n != 0) goto r; }
              t.n = (int)(s - t.s); t.trim(); return t;
            }
          r: this = t; t.n = 0; return t;
          }
        }
        internal bool equals(string name)
        {
          var l = name.Length; if (l != n) return false;
          for (int i = 0; i < l; i++) if (s[i] != name[i]) return false; return true;
        }
        internal int token()
        {
          if (n == 0) return 0;
          int x = 0; var c = s[x++];
          if (c == '(' || c == '{' || c == '[')
          {
            for (int k = 1; x < n;)
            {
              c = s[x++];
              if (c == '"' || c == '\'') { for (char v; x < n && (v = s[x++]) != c;) if (v == '\\') x++; continue; }
              if (c == '(' || c == '{' || c == '[') { k++; continue; }
              if (c == ')' || c == '}' || c == ']') { if (--k == 0) break; continue; }
            }
            return x;
          }
          if (c == '"' || c == '\'') { for (char v; x < n && (v = s[x++]) != c;) if (v == '\\') x++; return x; }
          if (char.IsLetter(c) || c == '_') { for (; x < n && (char.IsLetterOrDigit(c = s[x]) || c == '_'); x++) ; return x; }
          if (c == '0' && x < n && (s[x] | 0x20) == 'x') { for (++x; x < n && char.IsLetterOrDigit(c = s[x]); x++) ; return x; }
          if (char.IsNumber(c) || (c == '.' && x < n && char.IsNumber(s[x])))
          {
            for (; x < n && char.IsNumber(c = s[x]); x++) ;
            if (c == '.') for (++x; x < n && char.IsNumber(c = s[x]); x++) ;
            if ((c | 0x20) == 'e') { for (++x; x < n && char.IsNumber(c = s[x]); x++) ; if (c == '+' || c == '-') x++; for (; x < n && char.IsNumber(c = s[x]); x++) ; }
            c |= (char)0x20; if (c == 'f' || c == 'd' || c == 'm') x++;
            return x;
          }
          if (c == ',' || c == ';' || c == '.') return x;
          if (c == '<')
          {
            for (int k = 1, y = x; y < n;)
            {
              var z = s[y++]; if (z == '<') { if (s[y - 2] == z) break; k++; continue; }
              if (z == '>') { if (--k == 0) return y; continue; }
              if (!(z <= ' ' || z == '.' || z == ',' || z == '_' || char.IsLetterOrDigit(z))) break;
            }
          }
          if (x == n) return x;
          if (c == '+' || c == '-' || c == '=') { if (s[x] == c) return x + 1; if (c != '=' && s[x] != '=') return x; }
          for (; x < n && (c = s[x]) > ' ' && c != '(' && c != '.' && c != '_' && c != '"' && c != '\'' && !char.IsLetterOrDigit(c); x++) ;//for (; x < e && "/=+-*%&|^!?:<>~".IndexOf(code[x]) >= 0; x++) ;
          return x;
        }
        internal int opcode()
        {
          switch (n)
          {
            case 1: switch (s[0]) { case '+': return 0x30; case '-': return 0x31; case '*': return 0x20; case '/': return 0x21; case '%': return 0x22; case '=': return 0xd0; case '?': return 0xc0; case '|': return 0x90; case '^': return 0x80; case '&': return 0x70; case '<': return 0x50; case '>': return 0x51; } break;
            case 2:
              if (s[1] == '=') { switch (s[0]) { case '*': return 0xd1; case '/': return 0xd2; case '%': return 0xd3; case '+': return 0xd4; case '-': return 0xd5; case '&': return 0xd6; case '^': return 0xd7; case '|': return 0xd8; case '=': return 0x60; case '!': return 0x61; case '<': return 0x52; case '>': return 0x53; } break; }
              if (s[1] == 's') { switch (s[0]) { case 'i': return 0x54; case 'a': return 0x55; } break; }
              if (s[0] == s[1]) { switch (s[0]) { case '<': return 0x40; case '>': return 0x41; case '&': return 0xb0; case '?': return 0xc2; case '|': return 0xa0; } break; }
              if (s[0] == '=' && s[1] == '>') return 0xdf;
              break;
            case 3: if (s[2] == '=' && s[0] == s[1]) switch (s[0]) { case '<': return 0xd9; case '>': return 0xda; } break;
          }
          return 0;
        }
        internal void trim()
        {
          for (; n != 0 && *s <= 32; s++, n--) ;
          for (; n != 0 && s[n - 1] <= 32; n--) ;
        }
        internal void trim(int a, int b = 0)
        {
          s += a; n -= a + b; if (n < 0) throw new SyntaxException(); trim();
        }
        internal bool isname()
        {
          return n != 0 && (char.IsLetter(s[0]) || s[0] == '_');
        }
        internal bool take(string v)
        {
          var l = v.Length; if (l > n) return false;
          for (int i = 0; i < l; i++) if (s[i] != v[i]) return false; s += l; n -= l; trim(); return true;
        }
      }
      class SyntaxException : Exception { internal SyntaxException(string s = "Synatx") : base(s) { } }
    }
  }
}
