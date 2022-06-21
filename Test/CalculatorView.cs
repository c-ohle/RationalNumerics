using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
  public partial class CalculatorView : UserControl
  {
    public CalculatorView()
    {
      InitializeComponent();
    }
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e); if (DesignMode) return;
      textBox1.Text = "0"; label1.Text = ""; digits = 32;
      numericUpDownDigits.Value = digits;
      sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];
    }
    protected override CreateParams CreateParams
    {
      get { var t = base.CreateParams; t.ExStyle |= 0x02000000; return t; } // WS_EX_COMPOSITED
    }
    List<string> list = new(); int state, kl, digits, exp; char sep; Font? font; bool deg;

    void button_Click(object sender, EventArgs e)
    {
      var s = textBox1.Text; var st = (string)((Button)sender).Tag; var c = st[0];
      switch (c)
      {
        case >= '0' and <= '9':
          if (state == 2 || state == 3) { s = "0"; list.Clear(); state = kl = exp = 0; }
          if (state != 0 || s == "0") s = "";
          if (exp != 0)
          {
            if (s.Length == exp + 2 && s[exp + 1] == '0') s = s.Substring(0, exp + 1);
          }
          s += c; state = 0;
          break;
        case '.':
        case '\'':
          if (state == 2 || state == 3) { list.Clear(); state = kl = exp = 0; s = "0"; }
          if (exp != 0) return;
          if (!s.Contains(sep)) { s += sep; break; }
          if (c == '\'' && !s.Contains('\'')) { s += '\''; break; }
          return;
        case '±':
          if (state == 2 || state == 3) { list.Clear(); state = kl = 0; }
          if (state != 0 || s == "0") return;
          if (exp != 0) { s = s.Substring(0, exp) + (s[exp] == '+' ? '-' : '+') + s.Substring(exp + 1); break; }
          s = s[0] == '-' ? s.Substring(1) : '-' + s;
          break;
        case '(':
          if (state == 3) { s = "0"; list.Clear(); state = kl = exp = 0; }
          if (state == 0 && s != "0") { list.Add(s); state = 2; }
          if (state == 2) list.Add("*");
          list.Add("("); kl++; state = 1;
          break;
        case ')':
          if (kl == 0) return; //if (list.Count != 0 && list[list.Count - 1] == "(") return;
          if (state == 1) return; // (
          if (state == 0) list.Add(s);
          list.Add(c.ToString()); state = 2; kl--;
          break;
        case '+' or '-' or '*' or '/' or '%' or '^' or '=':
          if (state == 3) { if (c == '=') return; list.Clear(); state = kl = 0; }
          if (state == 0) list.Add(s);
          if (state == 1)
          {
            var t = list.Count - 1; if (list[t] == "(") return;
            if (c == '=')
            {
              //var u = t; for (; u >= 0 && list[u][0])); u--) ; 
            }
            list.RemoveAt(t);
          }
          if (c == '=') for (; kl != 0; kl--, list.Add(")")) ;
          list.Add(c.ToString()); state = c == '=' ? 3 : 1;
          break;
        case '«':
          if (state != 0) return;
          s = s.Substring(0, s.Length - 1);
          if (exp != 0) { if (s.Length == exp + 1) s += '0'; break; }
          if (s.Length == 0 || s.Length == 1 && s[0] == '-') s = "0";
          break;
        case 'E':
          if (state != 0 || exp != 0) return;
          exp = s.Length + 1; s += "E+0";
          break;
        case 'C':
          if (state == 0 && s != "0") { s = "0"; exp = 0; break; }
          s = "0"; list.Clear(); state = kl = exp = 0;
          break;
        case 'R': deg = !deg; btn_rad.Text = deg ? "Deg" : "Rad"; break;
        case 'π':
        case '℮':
          if (state == 3) { s = "0"; list.Clear(); state = kl = exp = 0; }
          if (state == 0 && s != "0") { list.Add(s); state = 2; }
          if (state == 2) list.Add("*");
          list.Add(c.ToString()); state = 2;
          break;
        case '?': return;
        default:
          if (st == "inv") st = "1 /";
          if (state == 3) { list.Clear(); state = kl = exp = 0; }
          if (state == 0) { list.Add(st); list.Add("("); list.Add(s); list.Add(")"); state = 2; break; }
          if (state == 2) // )
          {
            int k = list.Count - 1;
            for (int l = 0; k != 0; k--)
              if (list[k] == ")") l++;
              else if (list[k] == "(") l--;
              else if (l == 0) { if (char.IsLetter(list[k][0])) continue; k++; break; }
            list.Insert(k, st); if (list[k + 1] != "(") { list.Insert(k + 1, "("); list.Add(")"); }
            state = 2; break;
          }
          return;
      }
      label1.Text = string.Join(' ', list);
      textBox1.Text = parse() ?? s;
      if (state == 0) { textBox1.Select(textBox1.TextLength, 0); textBox1.ScrollToCaret(); }
      if (c == '=') AddHistory();
    }

    void AddHistory()
    {
      var page = (CalculatorPage)Parent; //var main = (MainFrame)Parent.Parent.Parent;
      var panel = page.panel_hist;
      var c1 = Color.FromArgb(unchecked((int)0xfff8f8f8));// SystemColors.Control;
      var c2 = SystemColors.Control;
      var t1 = new TextBox() { Text = label1.Text, Dock = DockStyle.Top, TextAlign = HorizontalAlignment.Right, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = c1, ForeColor = Color.Gray };
      var t2 = new TextBox() { Text = textBox1.Text, Dock = DockStyle.Top, TextAlign = HorizontalAlignment.Right, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = c1, Font = new Font(Font.FontFamily, 13, FontStyle.Regular), Padding = new Padding(5), };
      var t3 = new Panel() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10), BackColor = c1, };
      var e1 = (EventHandler)((p, e) => t1.BackColor = t2.BackColor = t3.BackColor = c1);
      var e2 = (EventHandler)((p, e) => t1.BackColor = t2.BackColor = t3.BackColor = c2);
      t1.MouseEnter += e2; t1.MouseLeave += e1;
      t2.MouseEnter += e2; t2.MouseLeave += e1;
      t3.MouseEnter += e2; t3.MouseLeave += e1;
      t3.Controls.Add(t2);
      t3.Controls.Add(t1);
      panel.Controls.Add(t3); t3.SendToBack(); panel.AutoScrollPosition = default;
      t3.DoubleClick += (p, e) =>
      {
        list.Clear(); state = kl = exp = 0;
        label1.Text = ""; textBox1.Text = t2.Text;
      };
    }

    string? parse()
    {
      if (state == 0) return null;
      if (kl != 0) return null;
      try
      {
        var f = button_rat.Text; var d = f == "d";
        if (!d && digits > 1000) Cursor.Current = Cursors.WaitCursor;
        var a = 0; var b = state == 2 ? list.Count - 1 : list.Count - 2;
        var r = d ? rnd(parsed(a, b)) : parse(a, b);
        return r.ToString((f == "ℚ" ? "L" : "S") + (digits + 2));
        double rnd(double t) { if (digits <= 15) t = Math.Round(t, digits); return t; }
      }
      catch (Exception) { return "NaN"; }
      finally { if (digits > 1000) Cursor.Current = Cursors.Default; }
    }
    rat parse(int a, int b)
    {
      var h = list[a];
      if (a == b)
        switch (h)
        {
          case "π": return rat.Pi(digits);
          case "℮": return rat.Exp(1, digits);
          default: return rat.Parse(h);
        }
      for (int l = 0; l < 3; l++)
        for (int k = 0, i = b; i >= a; i--)
        {
          var s = list[i]; if (s.Length != 1) continue; var c = s[0];
          if (c == ')') { k++; continue; }
          if (c == '(') { k--; continue; }
          if (k != 0) continue;
          if (l == 0)
          {
            if (c == '+') return parse(a, i - 1) + parse(i + 1, b);
            if (c == '-') return parse(a, i - 1) - parse(i + 1, b);
          }
          else if (l == 1)
          {
            if (c == '*') return parse(a, i - 1) * parse(i + 1, b);
            if (c == '/') return parse(a, i - 1) / parse(i + 1, b);
            if (c == '%') return parse(a, i - 1) % parse(i + 1, b);
          }
          else
          {
            if (c == '^') return rat.Pow(parse(a, i - 1), parse(i + 1, b));
          }
        }
      if (h == "(") return parse(a + 1, b - 1);
      var t = parse(a + 1, b);
      switch (h)
      {
        case "abs": t = rat.Abs(t); break;
        case "1 /": t = 1 / t; break;
        case "sqrt": t = rat.Sqrt(t, digits); break;
        case "sqr": t = t * t; break;
        case "ln": t = rat.Log(t, digits); break;
        case "log":
          // Math.Log2((double)t) / Math.Log2(10);            
          t = rat.Log2(t, digits) / rat.Log2(10, digits);
          //t = rat.Log(t, digits) / rat.Log(10, digits);
          break;
        case "fact": t = fact(t); break;
        case "sin": if (deg) t = t * (rat.Pi(digits) / 180); t = rat.Sin(t, digits); break;
        case "cos": if (deg) t = t * (rat.Pi(digits) / 180); t = rat.Cos(t, digits); break;
        case "tan": if (deg) t = t * (rat.Pi(digits) / 180); t = rat.Tan(t, digits); break;
        case "cot": if (deg) t = t * (rat.Pi(digits) / 180); t = rat.Cos(t, digits) / rat.Sin(t, digits); break;
        case "asin": t = rat.Asin(t, digits); if (deg) t = t * (180 / rat.Pi(digits)); break;
        case "atan": t = rat.Atan(t, digits); if (deg) t = t * (180 / rat.Pi(digits)); break;
        default: break;
      }
      return t;
    }
    double parsed(int a, int b)
    {
      var h = list[a];
      if (a == b)
        switch (h)
        {
          case "π": return Math.PI;
          case "℮": return Math.E;
          default: return (double)rat.Parse(h);
        }
      for (int l = 0; l < 3; l++)
        for (int k = 0, i = b; i >= a; i--)
        {
          var s = list[i]; if (s.Length != 1) continue; var c = s[0];
          if (c == ')') { k++; continue; }
          if (c == '(') { k--; continue; }
          if (k != 0) continue;
          if (l == 0)
          {
            if (c == '+') return parsed(a, i - 1) + parsed(i + 1, b);
            if (c == '-') return parsed(a, i - 1) - parsed(i + 1, b);
          }
          else if (l == 1)
          {
            if (c == '*') return parsed(a, i - 1) * parsed(i + 1, b);
            if (c == '/') return parsed(a, i - 1) / parsed(i + 1, b);
            if (c == '%') return parsed(a, i - 1) % parsed(i + 1, b);
          }
          else
          {
            if (c == '^') return Math.Pow(parsed(a, i - 1), parsed(i + 1, b));
          }
        }
      if (h == "(") return parsed(a + 1, b - 1);
      var t = parsed(a + 1, b);
      switch (h)
      {
        case "abs": t = Math.Abs(t); break;
        case "1 /": t = 1 / t; break;
        case "sqrt": t = Math.Sqrt(t); break;
        case "sqr": t = t * t; break;
        case "ln": t = Math.Log(t); break;
        case "log": t = Math.Log2((double)t) / Math.Log2(10); break;
        case "fact": t = factd(t); break;
        case "sin": if (deg) t = t * (Math.PI / 180); t = Math.Sin(t); break;
        case "cos": if (deg) t = t * (Math.PI / 180); t = Math.Cos(t); break;
        case "tan": if (deg) t = t * (Math.PI / 180); t = Math.Tan(t); break;
        case "cot": if (deg) t = t * (Math.PI / 180); t = 1 / Math.Tan(t); break;
        case "asin": t = Math.Asin(t); if (deg) t = t * (180 / Math.PI); break;
        case "atan": t = Math.Atan(t); if (deg) t = t * (180 / Math.PI); break;
      }
      return t;
    }

    #region gamma
    rat fact(rat z, int digits = 32)
    {
      if (rat.IsInt(z))
      {
        if (z < 0) throw new ArgumentException();
        return MathR.Factorial((int)z);
      }
      var r = gamma(z + 1, 30, 55); //todo: formula for z, digits => a, d 
      r = rat.Round(r, digits); return r;
    }
    rat gamma(rat z, int a, int d)
    {
      var kk = gamma_kk;
      if (kk == null || kk.Length != a || gamma_d != d)
      {
        kk = gamma_kk = calc(a, gamma_d = d);
        static rat[] calc(int a, int digits)
        {
          var kk = new rat[a]; rat fac = 1;
          kk[0] = rat.Sqrt(rat.Pi(digits) * 2, digits);
          for (int k = 1; k < a; k++) { kk[k] = fac; fac *= -k; }
          Parallel.For(1, a, k => kk[k] = rat.Exp(a - k, digits) * rat.Pow(a - k, k - 0.5, digits) / kk[k]);
          return kk;
        }
        //static rat[] calc(int a, int digits)
        //{
        //  var kk = new rat[a]; rat fac = 1;
        //  for (int k = 1; k < a; fac *= -k, k++)
        //    kk[k] = rat.Exp(a - k, digits) * rat.Pow(a - k, k - 0.5, digits) / fac;
        //  return kk;
        //}
      }
      var s = kk[0]; for (int k = 1; k < a; k++) s += kk[k] / (z + k);
      s *= rat.Exp(-(z + a), d) * rat.Pow(z + a, z + 0.5, d);
      return s / z;
    }
    rat[]? gamma_kk; int gamma_d;

    static double factd(double d)
    {
      return gammad(d + 1);
    }
    static double gammad(double z)
    {
      if (z < 0.5) return Math.PI / (Math.Sin(Math.PI * z) * gammad(1 - z));
      z -= 1;
      double x = gamma_dd[0];
      for (var i = 1; i < 7 + 2; i++) x += gamma_dd[i] / (z + i);
      double t = z + 7 + 0.5;
      return Math.Sqrt(2 * Math.PI) * (Math.Pow(t, z + 0.5)) * Math.Exp(-t) * x;
    }
    static double[] gamma_dd = { 0.99999999999980993, 676.5203681218851, -1259.1392167224028, 771.32342877765313, -176.61502916214059, 12.507343278686905, -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 };
    #endregion

    #region handler 
    void numericUpDownRound_ValueChanged(object sender, EventArgs e)
    {
      digits = (int)numericUpDownDigits.Value;
      numericUpDownDigits.ForeColor = digits > 1000 ? Color.Red : Color.Black;
      numericUpDownDigits.Update();
      var s = parse(); if (s != null) { textBox1.Text = s; Debug.Assert(s.Length == textBox1.Text.Length); }
    }
    void numericUpDownRound_KeyDown(object sender, KeyEventArgs e)
    {
      e.SuppressKeyPress = e.KeyCode == Keys.Enter;
    }
    bool checkresize;
    void textBox1_TextChanged(object sender, EventArgs e)
    {
      checkresize = false;
      if (font != null)
      {
        textBox1.Font = font;
        textBox1.TextAlign = HorizontalAlignment.Right;
        textBox1.ScrollBars = ScrollBars.None;
      }
      var s = textBox1.Text;
      var i = textBox1.GetLineFromCharIndex(s.Length - 1);
      if (i != 0)
      {
        font ??= textBox1.Font;
        textBox1.Font = new Font(font.FontFamily, font.SizeInPoints * 0.7f);
        i = textBox1.GetLineFromCharIndex(s.Length - 1);
        if (i != 0)
        {
          textBox1.TextAlign = HorizontalAlignment.Left;
          textBox1.ScrollBars = ScrollBars.Horizontal;
        }
      }
      checkresize = true;
    }
    void textBox1_Resize(object sender, EventArgs e)
    {
      if (checkresize) textBox1_TextChanged(sender, e);
    }
    private void contextMenuEdit_Opening(object sender, CancelEventArgs e)
    {
      mi_paste.Enabled = Clipboard.ContainsText();
    }
    void button_rat_Click(object sender, EventArgs e)
    {
      var s = button_rat.Text; button_rat.Text = s == "ℚ'" ? "ℚ" : s == "ℚ" ? "d" : "ℚ'";
      numericUpDownRound_ValueChanged(sender, e);
    }
    private void oncopy(object sender, EventArgs e)
    {
      ProcessDialogKey(Keys.Control | Keys.C);
    }
    private void onpaste(object sender, EventArgs e)
    {
      ProcessDialogKey(Keys.Control | Keys.V);
    }
    protected override bool ProcessDialogKey(Keys k)
    {
      if ((k & Keys.Control) != 0 && !numericUpDownDigits.Focused)
      {
        switch ((Keys)((int)k & 0xff))
        {
          case Keys.C: Clipboard.SetText(textBox1.Text); break;
          case Keys.V:
            if (!Clipboard.ContainsText()) break;
            try
            {
              var s = Clipboard.GetText();
              var t = rat.Parse(s);
              s = t.ToString("S" + (digits + 2)); //if (state != 0 || list.Count != 0) break;
              //{ list.Clear(); state = kl = exp = 0; label1.Text = ""; }                 
              if (state == 2 || state == 3) { list.Clear(); state = kl = exp = 0; }
              //if (exp != 0) { if (s.Length == exp + 2 && s[exp + 1] == '0') s = s.Substring(0, exp + 1); }
              state = 0;
              textBox1.Text = s; textBox1.Select(textBox1.TextLength, 0); textBox1.ScrollToCaret();
              return true;
            }
            catch { }
            break;
        }
      }
      return base.ProcessDialogKey(k);
    }
    protected override bool ProcessDialogChar(char charCode)
    {
      var btn = Controls.OfType<Button>().FirstOrDefault(p => p.Tag is string s && s[0] == charCode);
      if (btn != null) { btn.Focus(); btn.PerformClick(); return true; }
      return base.ProcessDialogChar(charCode);
    }
    #endregion
  }
}
