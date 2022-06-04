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
  public partial class CalculatorPage : UserControl
  {
    public CalculatorPage()
    {
      InitializeComponent();
      //this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
    }
    protected override CreateParams CreateParams
    {
      get { var t = base.CreateParams; t.ExStyle |= 0x02000000; return t; } // WS_EX_COMPOSITED
    }
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e); if (DesignMode) return;
      textBox1.Text = "0"; label1.Text = ""; digits = 32;
      numericUpDownDigits.Value = digits;
      sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];
    }

    List<string> list = new(); int state, kl, digits; char sep; Font? font;

    protected override bool ProcessDialogChar(char charCode)
    {
      var btn = Controls.OfType<Button>().FirstOrDefault(p => p.Tag is string s && s[0] == charCode);
      if (btn != null) { btn.Focus(); btn.PerformClick(); return true; }
      return base.ProcessDialogChar(charCode);
    }

    BigRational parse(int a, int b)
    {
      for (int l = 0; l < 2; l++)
        for (int k = 0, i = b; i >= a; i--)
        {
          var s = list[i]; if (s.Length != 1) continue; var c = s[0];
          if (c == ')') { k++; continue; }
          if (c == '(') { k--; continue; }
          if (k != 0) continue;
          if (c == '+' && l == 0) return parse(a, i - 1) + parse(i + 1, b);
          if (c == '-' && l == 0) return parse(a, i - 1) - parse(i + 1, b);
          if (c == '*' && l == 1) return parse(a, i - 1) * parse(i + 1, b);
          if (c == '/' && l == 1) return parse(a, i - 1) / parse(i + 1, b);
        }
      switch (list[a])
      {
        case "(": return parse(a + 1, b - 1);
        case "√": { var t = parse(a + 1, b); t = BigRational.Sqrt(t, digits); return t; }
        case "sqr": { var t = parse(a + 1, b); t = t * t; return t; }
        case "ln": { var t = parse(a + 1, b); t = BigRational.Log(t, digits); return t; }
        case "log":
          {
            var t = parse(a + 1, b); //var x = Math.Log2((double)t) / Math.Log2(10);            
            t = BigRational.Log2(t, digits) / BigRational.Log2(10, digits);
            //t = BigRational.Log(t, digits) / BigRational.Log(10, digits);
            return t;
          }
        case "fact": { var t = parse(a + 1, b); t = factorial(t); return t; }
        case "sin": { var t = parse(a + 1, b); t = Math.Sin((double)t); return t; }
        case "cos": { var t = parse(a + 1, b); t = Math.Cos((double)t); return t; }
        case "π": return MathR.PI(digits);
        case "℮": return BigRational.Exp(1, digits);
        default: return BigRational.Parse(list[a]);
      }
    }

    void button_Click(object sender, EventArgs e)
    {
      var s = textBox1.Text;
      var c = ((string)((Button)sender).Tag)[0];
      switch (c)
      {
        case >= '0' and <= '9':
          if (state == 2 || state == 3) { s = "0"; list.Clear(); state = kl = 0; }
          if (state != 0 || s == "0") s = "";
          s += c; state = 0;
          break;
        case '.':
          if (state == 2 || state == 3) { list.Clear(); state = kl = 0; s = "0"; }
          if (!s.Contains(sep)) { s += sep; break; }
          if (!s.Contains('\'')) { s += '\''; break; }
          return;
        case '±':
          if (state == 2 || state == 3) { list.Clear(); state = kl = 0; }
          if (state != 0 || s == "0") return;
          s = s[0] == '-' ? s.Substring(1) : '-' + s;
          break;
        case '(':
          if (state == 3) { s = "0"; list.Clear(); state = kl = 0; }
          if (state == 0 && s != "0") { list.Add(s); state = 2; }
          if (state == 2) list.Add("*");
          list.Add("("); kl++; state = 1;
          break;
        case ')':
          if (kl == 0) return;
          if (state == 0) list.Add(s);
          list.Add(c.ToString()); state = 2; kl--;
          break;
        case '+' or '-' or '*' or '/' or '=':
          if (state == 3) { if (c == '=') return; list.Clear(); state = kl = 0; }
          if (state == 0) list.Add(s);
          if (state == 1) { var t = list.Count - 1; if (list[t] == "(") return; list.RemoveAt(t); }
          if (c == '=') for (; kl != 0; kl--, list.Add(")")) ;
          list.Add(c.ToString()); state = c == '=' ? 3 : 1;
          break;
        case '«':
          if (state != 0) return;
          s = s.Substring(0, s.Length - 1); if (s.Length == 0 || s.Length == 1 && s[0] == '-') s = "0";
          break;
        case 'C':
          if (state == 0 && s != "0") { s = "0"; break; }
          s = "0"; list.Clear(); state = kl = 0;
          break;
        case 'π':
        case '℮':
          if (state == 3) { s = "0"; list.Clear(); state = kl = 0; }
          if (state == 0 && s != "0") { list.Add(s); state = 2; }
          if (state == 2) list.Add("*");
          list.Add(c.ToString()); state = 2;
          break;
        case '!':
        case '√':
        case '²':
        case 's':
        case 'c':
        case 'l':
        case 'L':
          {
            var f =
              c == '!' ? "fact" : c == '√' ? "√" :
              c == '²' ? "sqr" : c == 'l' ? "ln" : c == 'L' ? "log" :
              c == 's' ? "sin" : "cos";
            if (state == 3) { list.Clear(); state = kl = 0; state = 0; }
            if (state == 0) { list.Add(f); list.Add("("); list.Add(s); list.Add(")"); state = 2; break; }
            if (state == 2)
            {
              int k = list.Count - 1; for (int l = 0; k != 0; k--) if (list[k] == ")") l++; else if (list[k] == "(") l--; else if (l == 0) break;
              list.Insert(k, f); if (list[k + 1] != "(") { list.Insert(k + 1, "("); list.Add(")"); }
              state = 2; break;
            }
          }
          return;
      }
      label1.Text = string.Join(' ', list);
      textBox1.Text = calcs() ?? s;
      if (state == 0) { textBox1.Select(textBox1.TextLength, 0); textBox1.ScrollToCaret(); }
    }
    string? calcs()
    {
      if (state != 0 && kl == 0 && (state == 2 || list.Count >= 3))
      {
        try
        {
          var r = parse(0, state == 2 ? list.Count - 1 : list.Count - 2);
          return r.ToString("S" + (digits + 2));
        }
        catch (Exception) { return "NaN"; }
      }
      return null;
    }

    #region gamma
    BigRational factorial(BigRational z, int digits = 32)
    {
      if (rat.IsInt(z))
      {
        if (z < 0) throw new ArgumentException();
        return MathR.Factorial((int)z);
      }
      var r = gamma(z + 1, 30, 55); //todo: formula for z, digits => a, d 
      r = BigRational.Round(r, digits); return r;
    }
    BigRational gamma(BigRational z, int a, int d)
    {
      var kk = gamma_kk;
      if (kk == null || kk.Length != a || gamma_d != d)
      {
        kk = gamma_kk = calc(a, gamma_d = d);
        static BigRational[] calc(int a, int digits)
        {
          var kk = new BigRational[a]; BigRational fac = 1;
          kk[0] = BigRational.Sqrt(MathR.PI(digits) * 2, digits);
          for (int k = 1; k < a; k++) { kk[k] = fac; fac *= -k; }
          Parallel.For(1, a, k => kk[k] = BigRational.Exp(a - k, digits) * BigRational.Pow(a - k, k - 0.5, digits) / kk[k]);
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
      s *= BigRational.Exp(-(z + a), d) * BigRational.Pow(z + a, z + 0.5, d);
      return s / z;
    }
    BigRational[]? gamma_kk; int gamma_d;
    #endregion

    #region handler 
    private void numericUpDownRound_ValueChanged(object sender, EventArgs e)
    {
      digits = (int)numericUpDownDigits.Value;
      var s = calcs(); if (s == null) return;
      textBox1.Text = s; // tbadjust(s);
    }
    void numericUpDownRound_KeyDown(object sender, KeyEventArgs e)
    {
      e.SuppressKeyPress = e.KeyCode == Keys.Enter;
    }
    bool checkresize;
    private void textBox1_TextChanged(object sender, EventArgs e)
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
    private void textBox1_Resize(object sender, EventArgs e)
    {
      if (!checkresize) return;
      textBox1_TextChanged(sender, e);
    }
    #endregion
  }
}
