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
    }

    protected override bool ProcessDialogChar(char charCode)
    {
      var btn = Controls.OfType<Button>().FirstOrDefault(p => p.Tag is string s && s[0] == charCode);
      if (btn != null) { btn.Focus(); btn.PerformClick(); return true; }
      return base.ProcessDialogChar(charCode);
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e); textBox1.Text = "0"; label1.Text = "";
    }
    List<string> list = new(); int state, kl;
    NewRational parse(int a, int b)
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
        case "√": { var t = parse(a + 1, b); t = MathR.Sqrt(t, 20); return t; }
        case "sqr": { var t = parse(a + 1, b); t = t * t; return t; }
        case "fact":
          {
            var t = parse(a + 1, b);
            if (t % 1 == 0) { } else { }
            if (MathR.IsInteger(t)) t = MathR.Factorial((int)t);
            else t = factorial((double)t);
            return t;
          }
        case "π": return MathR.PI(20);
        default: return NewRational.Parse(list[a]);
      }
    }

    static double gamma1(double z)
    {
      return Math.Sqrt(2 * Math.PI / z) * Math.Pow((1 / Math.E) * (z + 1 / (12 * z - 1 / (10 * z))), z);
    }
    static double gamma(double z)
    {
      if (z < 0.5) return Math.PI / (Math.Sin(Math.PI * z) * gamma(1 - z));
      else
      {
        const int g = 7;
        double[] a = new double[] { 0.99999999999980993, 676.5203681218851, -1259.1392167224028, 771.32342877765313, -176.61502916214059, 12.507343278686905, -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 };
        z -= 1;
        var x = a[0];
        for (var i = 1; i < g + 2; i++) x += a[i] / (z + i);
        var t = z + g + 0.5;
        return Math.Sqrt(2 * Math.PI) * Math.Pow(t, (z + 0.5)) * Math.Exp(-t) * x;
      }
    }
    static double factorial(double n)
    {
      return gamma(n + 1);
    }

    void button11_Click(object sender, EventArgs e)
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
          if (s.Contains('.')) return; s += NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];// c;
          break;
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
          if (state == 3) { s = "0"; list.Clear(); state = kl = 0; }
          if (state == 0 && s != "0") { list.Add(s); state = 2; }
          if (state == 2) list.Add("*");
          list.Add("π"); state = 2;
          break;
        case '℮': break;
        case '!':
        case '√':
        case '²':
          {
            var f = c == '!' ? "fact" : c == '√' ? "√" : "sqr";
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
      if (state != 0 && kl == 0 && (state == 2 || list.Count >= 3))
      {
        try
        {
          var r = parse(0, state == 2 ? list.Count - 1 : list.Count - 2);
          s = r.ToString();
        }
        catch (Exception) { s = "NaN"; }
      }
      label1.Text = string.Join(' ', list);
      textBox1.Text = s;
    }
  }
}
