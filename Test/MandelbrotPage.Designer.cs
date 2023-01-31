namespace Test
{
  partial class MandelbrotPage
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MandelbrotPage));
      splitContainer1 = new SplitContainer();
      label4 = new Label();
      mandelbrotView1 = new MandelbrotView();
      label3 = new Label();
      labelState1 = new Label();
      label1 = new Label();
      label6 = new Label();
      checkBoxActive2 = new CheckBox();
      mandelbrotView2 = new MandelbrotView();
      labelState2 = new Label();
      label2 = new Label();
      textBoxScaling = new TextBox();
      labelRelation = new Label();
      textBoxCenterX = new TextBox();
      label5 = new Label();
      label7 = new Label();
      label8 = new Label();
      label9 = new Label();
      textBoxCenterY = new TextBox();
      numericUpDownIter = new NumericUpDown();
      buttonReset = new Button();
      numericUpDownRound = new NumericUpDown();
      label10 = new Label();
      toolTip1 = new ToolTip(components);
      cbBigRat = new CheckBox();
      ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
      splitContainer1.Panel1.SuspendLayout();
      splitContainer1.Panel2.SuspendLayout();
      splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)numericUpDownIter).BeginInit();
      ((System.ComponentModel.ISupportInitialize)numericUpDownRound).BeginInit();
      SuspendLayout();
      // 
      // splitContainer1
      // 
      splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      splitContainer1.IsSplitterFixed = true;
      splitContainer1.Location = new Point(0, 3);
      splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      splitContainer1.Panel1.Controls.Add(label4);
      splitContainer1.Panel1.Controls.Add(mandelbrotView1);
      splitContainer1.Panel1.Controls.Add(label3);
      splitContainer1.Panel1.Controls.Add(labelState1);
      splitContainer1.Panel1.Controls.Add(label1);
      splitContainer1.Panel1.Padding = new Padding(8, 0, 4, 8);
      // 
      // splitContainer1.Panel2
      // 
      splitContainer1.Panel2.Controls.Add(label6);
      splitContainer1.Panel2.Controls.Add(checkBoxActive2);
      splitContainer1.Panel2.Controls.Add(mandelbrotView2);
      splitContainer1.Panel2.Controls.Add(labelState2);
      splitContainer1.Panel2.Controls.Add(label2);
      splitContainer1.Panel2.Padding = new Padding(4, 0, 8, 8);
      splitContainer1.Size = new Size(975, 474);
      splitContainer1.SplitterDistance = 486;
      splitContainer1.SplitterWidth = 5;
      splitContainer1.TabIndex = 0;
      splitContainer1.TabStop = false;
      // 
      // label4
      // 
      label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      label4.Location = new Point(232, 14);
      label4.Name = "label4";
      label4.Size = new Size(240, 20);
      label4.TabIndex = 7;
      label4.Text = "Numeric based on BigRational";
      label4.TextAlign = ContentAlignment.TopRight;
      // 
      // mandelbrotView1
      // 
      mandelbrotView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      mandelbrotView1.CenterX = (rat)resources.GetObject("mandelbrotView1.CenterX");
      mandelbrotView1.CenterY = (rat)resources.GetObject("mandelbrotView1.CenterY");
      mandelbrotView1.Driver = MandelbrotView.MandelDriver.BigRational;
      mandelbrotView1.Iterations = 32;
      mandelbrotView1.Lim = 64;
      mandelbrotView1.Location = new Point(11, 45);
      mandelbrotView1.Manual = false;
      mandelbrotView1.Name = "mandelbrotView1";
      mandelbrotView1.PropChanged = null;
      mandelbrotView1.Scaling = (rat)resources.GetObject("mandelbrotView1.Scaling");
      mandelbrotView1.Size = new Size(468, 391);
      mandelbrotView1.StateChanged = null;
      mandelbrotView1.TabIndex = 3;
      mandelbrotView1.TabStop = false;
      // 
      // label3
      // 
      label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      label3.Location = new Point(232, 438);
      label3.Name = "label3";
      label3.Size = new Size(247, 23);
      label3.TabIndex = 2;
      label3.Text = "(Wheel to zoom, click to move)";
      label3.TextAlign = ContentAlignment.TopRight;
      // 
      // labelState1
      // 
      labelState1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      labelState1.Location = new Point(11, 438);
      labelState1.Name = "labelState1";
      labelState1.Size = new Size(240, 28);
      labelState1.TabIndex = 2;
      labelState1.Text = "status";
      toolTip1.SetToolTip(labelState1, "Calculation tíme in milliseconds\r\nGC memory allocs (including System allocs)\r\nImage size in pixel");
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold, GraphicsUnit.Point);
      label1.Location = new Point(11, 0);
      label1.Name = "label1";
      label1.Size = new Size(169, 38);
      label1.TabIndex = 1;
      label1.Text = "BigRational";
      // 
      // label6
      // 
      label6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      label6.Location = new Point(147, 14);
      label6.Name = "label6";
      label6.Size = new Size(304, 24);
      label6.TabIndex = 7;
      label6.Text = "Numeric based on BigInteger";
      label6.TextAlign = ContentAlignment.TopRight;
      toolTip1.SetToolTip(label6, "Calculation based on a conventional BigRational implementation\r\nusing one BigInteger each for the numerator and denominator.");
      // 
      // checkBoxActive2
      // 
      checkBoxActive2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      checkBoxActive2.CheckAlign = ContentAlignment.MiddleRight;
      checkBoxActive2.Checked = true;
      checkBoxActive2.CheckState = CheckState.Checked;
      checkBoxActive2.Location = new Point(226, 437);
      checkBoxActive2.Name = "checkBoxActive2";
      checkBoxActive2.Size = new Size(225, 24);
      checkBoxActive2.TabIndex = 7;
      checkBoxActive2.Text = "on/off";
      checkBoxActive2.TextAlign = ContentAlignment.MiddleRight;
      checkBoxActive2.UseVisualStyleBackColor = true;
      checkBoxActive2.CheckedChanged += checkBoxActive2_CheckedChanged;
      // 
      // mandelbrotView2
      // 
      mandelbrotView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      mandelbrotView2.CenterX = (rat)resources.GetObject("mandelbrotView2.CenterX");
      mandelbrotView2.CenterY = (rat)resources.GetObject("mandelbrotView2.CenterY");
      mandelbrotView2.Driver = MandelbrotView.MandelDriver.BigInteger;
      mandelbrotView2.Enabled = false;
      mandelbrotView2.Iterations = 32;
      mandelbrotView2.Lim = 64;
      mandelbrotView2.Location = new Point(7, 45);
      mandelbrotView2.Manual = true;
      mandelbrotView2.Name = "mandelbrotView2";
      mandelbrotView2.PropChanged = null;
      mandelbrotView2.Scaling = (rat)resources.GetObject("mandelbrotView2.Scaling");
      mandelbrotView2.Size = new Size(446, 391);
      mandelbrotView2.StateChanged = null;
      mandelbrotView2.TabIndex = 3;
      mandelbrotView2.TabStop = false;
      // 
      // labelState2
      // 
      labelState2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      labelState2.Location = new Point(7, 439);
      labelState2.Name = "labelState2";
      labelState2.Size = new Size(346, 27);
      labelState2.TabIndex = 2;
      labelState2.Text = "status";
      toolTip1.SetToolTip(labelState2, "Calculation tíme in milliseconds\r\nGC memory allocs (including System allocs)\r\nImage size in pixel");
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold, GraphicsUnit.Point);
      label2.Location = new Point(7, 0);
      label2.Name = "label2";
      label2.Size = new Size(155, 38);
      label2.TabIndex = 1;
      label2.Text = "BigInteger";
      // 
      // textBoxScaling
      // 
      textBoxScaling.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      textBoxScaling.CausesValidation = false;
      textBoxScaling.Location = new Point(412, 528);
      textBoxScaling.Name = "textBoxScaling";
      textBoxScaling.ShortcutsEnabled = false;
      textBoxScaling.Size = new Size(196, 27);
      textBoxScaling.TabIndex = 3;
      textBoxScaling.WordWrap = false;
      textBoxScaling.KeyPress += textBox_KeyPress;
      textBoxScaling.Leave += textBox_Leave;
      // 
      // labelRelation
      // 
      labelRelation.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      labelRelation.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold, GraphicsUnit.Point);
      labelRelation.Location = new Point(322, 462);
      labelRelation.Name = "labelRelation";
      labelRelation.Size = new Size(335, 40);
      labelRelation.TabIndex = 2;
      labelRelation.Text = "___ : ___";
      labelRelation.TextAlign = ContentAlignment.TopCenter;
      toolTip1.SetToolTip(labelRelation, "Performance ratio");
      // 
      // textBoxCenterX
      // 
      textBoxCenterX.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      textBoxCenterX.CausesValidation = false;
      textBoxCenterX.Location = new Point(87, 528);
      textBoxCenterX.Name = "textBoxCenterX";
      textBoxCenterX.ShortcutsEnabled = false;
      textBoxCenterX.Size = new Size(225, 27);
      textBoxCenterX.TabIndex = 1;
      textBoxCenterX.WordWrap = false;
      textBoxCenterX.KeyPress += textBox_KeyPress;
      textBoxCenterX.Leave += textBox_Leave;
      // 
      // label5
      // 
      label5.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label5.Location = new Point(3, 531);
      label5.Name = "label5";
      label5.Size = new Size(78, 20);
      label5.TabIndex = 4;
      label5.Text = "Center X:";
      label5.TextAlign = ContentAlignment.TopRight;
      // 
      // label7
      // 
      label7.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label7.Location = new Point(318, 531);
      label7.Name = "label7";
      label7.Size = new Size(88, 24);
      label7.TabIndex = 4;
      label7.Text = "Scaling:";
      label7.TextAlign = ContentAlignment.TopRight;
      // 
      // label8
      // 
      label8.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label8.Location = new Point(318, 563);
      label8.Name = "label8";
      label8.Size = new Size(88, 20);
      label8.TabIndex = 4;
      label8.Text = "Iterations:";
      label8.TextAlign = ContentAlignment.TopRight;
      // 
      // label9
      // 
      label9.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label9.Location = new Point(3, 563);
      label9.Name = "label9";
      label9.Size = new Size(77, 20);
      label9.TabIndex = 4;
      label9.Text = "Center Y:";
      label9.TextAlign = ContentAlignment.TopRight;
      // 
      // textBoxCenterY
      // 
      textBoxCenterY.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      textBoxCenterY.CausesValidation = false;
      textBoxCenterY.Location = new Point(87, 560);
      textBoxCenterY.Name = "textBoxCenterY";
      textBoxCenterY.ShortcutsEnabled = false;
      textBoxCenterY.Size = new Size(225, 27);
      textBoxCenterY.TabIndex = 2;
      textBoxCenterY.WordWrap = false;
      textBoxCenterY.KeyPress += textBox_KeyPress;
      textBoxCenterY.Leave += textBox_Leave;
      // 
      // numericUpDownIter
      // 
      numericUpDownIter.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      numericUpDownIter.BorderStyle = BorderStyle.FixedSingle;
      numericUpDownIter.Location = new Point(412, 561);
      numericUpDownIter.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
      numericUpDownIter.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
      numericUpDownIter.Name = "numericUpDownIter";
      numericUpDownIter.Size = new Size(92, 27);
      numericUpDownIter.TabIndex = 4;
      numericUpDownIter.Value = new decimal(new int[] { 32, 0, 0, 0 });
      numericUpDownIter.ValueChanged += itervaluechanged;
      numericUpDownIter.KeyDown += numericUpDownIter_KeyDown;
      // 
      // buttonReset
      // 
      buttonReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      buttonReset.Location = new Point(872, 563);
      buttonReset.Name = "buttonReset";
      buttonReset.Size = new Size(94, 29);
      buttonReset.TabIndex = 6;
      buttonReset.Text = "Reset";
      buttonReset.UseVisualStyleBackColor = true;
      buttonReset.Click += buttonReset_Click;
      // 
      // numericUpDownRound
      // 
      numericUpDownRound.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      numericUpDownRound.BorderStyle = BorderStyle.FixedSingle;
      numericUpDownRound.Location = new Point(550, 561);
      numericUpDownRound.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
      numericUpDownRound.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
      numericUpDownRound.Name = "numericUpDownRound";
      numericUpDownRound.Size = new Size(56, 27);
      numericUpDownRound.TabIndex = 5;
      toolTip1.SetToolTip(numericUpDownRound, "Precision limitation.\r\nRoughly equivalent to an imaginary floating-point type\r\nwith such mantissa bit count.");
      numericUpDownRound.Value = new decimal(new int[] { 32, 0, 0, 0 });
      numericUpDownRound.ValueChanged += itervaluechanged;
      numericUpDownRound.KeyDown += numericUpDownIter_KeyDown;
      // 
      // label10
      // 
      label10.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      label10.Location = new Point(514, 563);
      label10.Name = "label10";
      label10.Size = new Size(35, 21);
      label10.TabIndex = 4;
      label10.Text = "lim:";
      label10.TextAlign = ContentAlignment.TopRight;
      // 
      // cbBigRat
      // 
      cbBigRat.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      cbBigRat.AutoSize = true;
      cbBigRat.Location = new Point(11, 483);
      cbBigRat.Name = "cbBigRat";
      cbBigRat.Size = new Size(176, 24);
      cbBigRat.TabIndex = 7;
      cbBigRat.Text = "BigRat (Experimental)";
      toolTip1.SetToolTip(cbBigRat, "If enabled, BigRat is used instead of BigRational in the left view.");
      cbBigRat.UseVisualStyleBackColor = true;
      cbBigRat.CheckedChanged += cbBigRat_CheckedChanged;
      // 
      // MandelbrotPage
      // 
      Controls.Add(cbBigRat);
      Controls.Add(buttonReset);
      Controls.Add(numericUpDownRound);
      Controls.Add(numericUpDownIter);
      Controls.Add(textBoxCenterY);
      Controls.Add(textBoxCenterX);
      Controls.Add(label10);
      Controls.Add(label8);
      Controls.Add(label9);
      Controls.Add(label7);
      Controls.Add(label5);
      Controls.Add(textBoxScaling);
      Controls.Add(labelRelation);
      Controls.Add(splitContainer1);
      Name = "MandelbrotPage";
      Padding = new Padding(0, 0, 0, 10);
      Size = new Size(975, 598);
      splitContainer1.Panel1.ResumeLayout(false);
      splitContainer1.Panel1.PerformLayout();
      splitContainer1.Panel2.ResumeLayout(false);
      splitContainer1.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
      splitContainer1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)numericUpDownIter).EndInit();
      ((System.ComponentModel.ISupportInitialize)numericUpDownRound).EndInit();
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private SplitContainer splitContainer1;
    private Label labelState1;
    private Label label1;
    private Label label2;
    private MandelbrotView mandelbrotView1;
    private MandelbrotView mandelbrotView2;
    private TextBox textBoxScaling;
    private Label labelRelation;
    private TextBox textBoxCenterX;
    private Label label5;
    private Label label7;
    private Label label8;
    private Label label9;
    private TextBox textBoxCenterY;
    private Label label4;
    private NumericUpDown numericUpDownIter;
    private CheckBox checkBoxActive2;
    private Label labelState2;
    private Button buttonReset;
    private Label label3;
    private Label label6;
    private NumericUpDown numericUpDownRound;
    private Label label10;
    private ToolTip toolTip1;
    private CheckBox cbBigRat;
  }
}
