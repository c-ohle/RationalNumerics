using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
  public partial class MandelbrotPage : UserControl
  {
    public MandelbrotPage()
    {
      InitializeComponent();
      update();
      mandelbrotView1.PropChanged += update;
      mandelbrotView1.StateChanged += statechaged;
      mandelbrotView2.StateChanged += statechaged;
    }
    void statechaged(object? p, EventArgs? e)
    {
      var v1 = mandelbrotView1; var t1 = v1.RenderTime;
      var v2 = mandelbrotView2; var t2 = v2.RenderTime; var bug = mandelbrotView2.Enabled; if (bug) t2 = 0; //crashed (BigInteger bugs)
      static string ms(long n) { var i = 0; for (; n >= 1024 && i < 4; n >>= 10, i++) ; return $"{n} {" KMGT"[i]}B"; }
      labelState1.Text = t1 != 0 ?
        $"{t1} ms {ms(v1.gcnews)} ({v1.Width}×{v1.Height})" : "calculates...";
      labelState2.Text = t2 != 0 ?
        $"{t2} ms {ms(v2.gcnews)} ({v2.Width}×{v2.Height})" : !checkBoxActive2.Checked ? "" : !bug ? "calculates..." : "exception";
      labelRelation.Text = t1 != 0 && t2 != 0 ?
        $"{(t1 <= t2 ? 1 : t1 / t2)} : {(t1 <= t2 ? t2 / t1 : 1)}" :
        "___ : ___";
      if (p == mandelbrotView1)
      {
        mandelbrotView2.Stop();
        if (t1 != 0)
        {
          mandelbrotView2.CenterX = mandelbrotView1.CenterX;
          mandelbrotView2.CenterY = mandelbrotView1.CenterY;
          mandelbrotView2.Scaling = mandelbrotView1.Scaling;
          mandelbrotView2.Iterations = mandelbrotView1.Iterations;
          mandelbrotView2.Lim = mandelbrotView1.Lim;
          if (checkBoxActive2.Checked && !bug) // !crashed (BigInteger bugs)
            mandelbrotView2.Start();
        }
      }
    }
    void update(object? p = null, EventArgs? e = null)
    {
      textBoxScaling.Text = mandelbrotView1.Scaling.ToString();
      textBoxCenterX.Text = mandelbrotView1.CenterX.ToString();
      textBoxCenterY.Text = mandelbrotView1.CenterY.ToString();
      numericUpDownIter.Value = mandelbrotView1.Iterations;
      numericUpDownRound.Value = mandelbrotView1.Lim;
    }
    void textBox_Leave(object sender, EventArgs e)
    {
      if (sender == textBoxScaling)
      {
        var v = BigRational.Parse(textBoxScaling.Text);
        mandelbrotView1.Scaling = v; update();
      }
      else if (sender == textBoxCenterX)
      {
        var v = BigRational.Parse(textBoxCenterX.Text);
        mandelbrotView1.CenterX = v; update();
      }
      else if (sender == textBoxCenterY)
      {
        var v = BigRational.Parse(textBoxCenterY.Text);
        mandelbrotView1.CenterY = v; update();
      }

    }
    void textBox_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar == 13)
      {
        e.Handled = true; textBox_Leave(sender, e);
      }
    }
    void itervaluechanged(object sender, EventArgs e)
    {
      if (sender == numericUpDownIter)
        mandelbrotView1.Iterations = (int)numericUpDownIter.Value;
      else if (sender == numericUpDownRound)
      {
        mandelbrotView1.Lim = (int)numericUpDownRound.Value;
        update();
      }
    }
    void buttonReset_Click(object sender, EventArgs e)
    {
      mandelbrotView1.Reset(); update();
    }
    void checkBoxActive2_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxActive2.Checked)
      {
        if (mandelbrotView1.RenderTime != 0)
        {
          mandelbrotView2.Driver = ModifierKeys == Keys.Alt ?
            MandelbrotView.MandelDriver.Double :
            MandelbrotView.MandelDriver.BigInteger;
          mandelbrotView2.Start();
        }
      }
      else
      {
        mandelbrotView2.Clear();
      }
    }
    void numericUpDownIter_KeyDown(object sender, KeyEventArgs e)
    {
      e.SuppressKeyPress = e.KeyCode == Keys.Enter;
    }
    void cbBigRat_CheckedChanged(object sender, EventArgs e)
    {
      mandelbrotView1.Driver = cbBigRat.Checked ?
        MandelbrotView.MandelDriver.BigRat :
        MandelbrotView.MandelDriver.BigRational;
      label1.Text = cbBigRat.Checked ? "BigRat" : "BigRational";
      label4.Text = cbBigRat.Checked ? "Numeric based on BigRat" : "Numeric based on BigRational";
    }

  }
}

