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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MandelbrotPage));
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.label4 = new System.Windows.Forms.Label();
      this.mandelbrotView1 = new Test.MandelbrotView();
      this.label3 = new System.Windows.Forms.Label();
      this.labelState1 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.checkBoxActive2 = new System.Windows.Forms.CheckBox();
      this.mandelbrotView2 = new Test.MandelbrotView();
      this.labelState2 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.textBoxScaling = new System.Windows.Forms.TextBox();
      this.labelRelation = new System.Windows.Forms.Label();
      this.textBoxCenterX = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.textBoxCenterY = new System.Windows.Forms.TextBox();
      this.numericUpDownIter = new System.Windows.Forms.NumericUpDown();
      this.buttonReset = new System.Windows.Forms.Button();
      this.numericUpDownRound = new System.Windows.Forms.NumericUpDown();
      this.label10 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIter)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRound)).BeginInit();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 3);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.label4);
      this.splitContainer1.Panel1.Controls.Add(this.mandelbrotView1);
      this.splitContainer1.Panel1.Controls.Add(this.label3);
      this.splitContainer1.Panel1.Controls.Add(this.labelState1);
      this.splitContainer1.Panel1.Controls.Add(this.label1);
      this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(8, 0, 4, 8);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.label6);
      this.splitContainer1.Panel2.Controls.Add(this.checkBoxActive2);
      this.splitContainer1.Panel2.Controls.Add(this.mandelbrotView2);
      this.splitContainer1.Panel2.Controls.Add(this.labelState2);
      this.splitContainer1.Panel2.Controls.Add(this.label2);
      this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(4, 0, 8, 8);
      this.splitContainer1.Size = new System.Drawing.Size(975, 474);
      this.splitContainer1.SplitterDistance = 486;
      this.splitContainer1.SplitterWidth = 5;
      this.splitContainer1.TabIndex = 0;
      this.splitContainer1.TabStop = false;
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label4.Location = new System.Drawing.Point(239, 14);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(240, 20);
      this.label4.TabIndex = 7;
      this.label4.Text = "Numeric based on NewRational";
      this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // mandelbrotView1
      // 
      this.mandelbrotView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mandelbrotView1.CenterX = ((System.Numerics.Rational.NewRational)(resources.GetObject("mandelbrotView1.CenterX")));
      this.mandelbrotView1.CenterY = ((System.Numerics.Rational.NewRational)(resources.GetObject("mandelbrotView1.CenterY")));
      this.mandelbrotView1.Driver = Test.MandelbrotView.MandelDriver.NewRational;
      this.mandelbrotView1.Iterations = 32;
      this.mandelbrotView1.Lim = 64;
      this.mandelbrotView1.Location = new System.Drawing.Point(11, 45);
      this.mandelbrotView1.Manual = false;
      this.mandelbrotView1.Name = "mandelbrotView1";
      this.mandelbrotView1.PropChanged = null;
      this.mandelbrotView1.Scaling = ((System.Numerics.Rational.NewRational)(resources.GetObject("mandelbrotView1.Scaling")));
      this.mandelbrotView1.Size = new System.Drawing.Size(468, 391);
      this.mandelbrotView1.StateChanged = null;
      this.mandelbrotView1.TabIndex = 3;
      this.mandelbrotView1.TabStop = false;
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Location = new System.Drawing.Point(263, 438);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(216, 20);
      this.label3.TabIndex = 2;
      this.label3.Text = "(Wheel to zoom, click to move)";
      // 
      // labelState1
      // 
      this.labelState1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelState1.AutoSize = true;
      this.labelState1.Location = new System.Drawing.Point(11, 438);
      this.labelState1.Name = "labelState1";
      this.labelState1.Size = new System.Drawing.Size(47, 20);
      this.labelState1.TabIndex = 2;
      this.labelState1.Text = "status";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label1.Location = new System.Drawing.Point(11, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(185, 38);
      this.label1.TabIndex = 1;
      this.label1.Text = "NewRational";
      // 
      // label6
      // 
      this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label6.Location = new System.Drawing.Point(239, 14);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(219, 20);
      this.label6.TabIndex = 7;
      this.label6.Text = "Numeric based on BigInteger";
      this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // checkBoxActive2
      // 
      this.checkBoxActive2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxActive2.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.checkBoxActive2.Checked = true;
      this.checkBoxActive2.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxActive2.Location = new System.Drawing.Point(233, 437);
      this.checkBoxActive2.Name = "checkBoxActive2";
      this.checkBoxActive2.Size = new System.Drawing.Size(225, 24);
      this.checkBoxActive2.TabIndex = 7;
      this.checkBoxActive2.Text = "on/off";
      this.checkBoxActive2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.checkBoxActive2.UseVisualStyleBackColor = true;
      this.checkBoxActive2.CheckedChanged += new System.EventHandler(this.checkBoxActive2_CheckedChanged);
      // 
      // mandelbrotView2
      // 
      this.mandelbrotView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mandelbrotView2.CenterX = ((System.Numerics.Rational.NewRational)(resources.GetObject("mandelbrotView2.CenterX")));
      this.mandelbrotView2.CenterY = ((System.Numerics.Rational.NewRational)(resources.GetObject("mandelbrotView2.CenterY")));
      this.mandelbrotView2.Driver = Test.MandelbrotView.MandelDriver.BigInteger;
      this.mandelbrotView2.Enabled = false;
      this.mandelbrotView2.Iterations = 32;
      this.mandelbrotView2.Lim = 64;
      this.mandelbrotView2.Location = new System.Drawing.Point(7, 45);
      this.mandelbrotView2.Manual = true;
      this.mandelbrotView2.Name = "mandelbrotView2";
      this.mandelbrotView2.PropChanged = null;
      this.mandelbrotView2.Scaling = ((System.Numerics.Rational.NewRational)(resources.GetObject("mandelbrotView2.Scaling")));
      this.mandelbrotView2.Size = new System.Drawing.Size(451, 391);
      this.mandelbrotView2.StateChanged = null;
      this.mandelbrotView2.TabIndex = 3;
      this.mandelbrotView2.TabStop = false;
      // 
      // labelState2
      // 
      this.labelState2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelState2.AutoSize = true;
      this.labelState2.Location = new System.Drawing.Point(7, 439);
      this.labelState2.Name = "labelState2";
      this.labelState2.Size = new System.Drawing.Size(47, 20);
      this.labelState2.TabIndex = 2;
      this.labelState2.Text = "status";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label2.Location = new System.Drawing.Point(7, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(169, 38);
      this.label2.TabIndex = 1;
      this.label2.Text = "BigRational";
      // 
      // textBoxScaling
      // 
      this.textBoxScaling.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.textBoxScaling.CausesValidation = false;
      this.textBoxScaling.Location = new System.Drawing.Point(412, 528);
      this.textBoxScaling.Name = "textBoxScaling";
      this.textBoxScaling.ShortcutsEnabled = false;
      this.textBoxScaling.Size = new System.Drawing.Size(196, 27);
      this.textBoxScaling.TabIndex = 3;
      this.textBoxScaling.WordWrap = false;
      this.textBoxScaling.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
      this.textBoxScaling.Leave += new System.EventHandler(this.textBox_Leave);
      // 
      // labelRelation
      // 
      this.labelRelation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.labelRelation.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.labelRelation.Location = new System.Drawing.Point(322, 462);
      this.labelRelation.Name = "labelRelation";
      this.labelRelation.Size = new System.Drawing.Size(335, 40);
      this.labelRelation.TabIndex = 2;
      this.labelRelation.Text = "___ : ___";
      this.labelRelation.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // textBoxCenterX
      // 
      this.textBoxCenterX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.textBoxCenterX.CausesValidation = false;
      this.textBoxCenterX.Location = new System.Drawing.Point(75, 528);
      this.textBoxCenterX.Name = "textBoxCenterX";
      this.textBoxCenterX.ShortcutsEnabled = false;
      this.textBoxCenterX.Size = new System.Drawing.Size(225, 27);
      this.textBoxCenterX.TabIndex = 1;
      this.textBoxCenterX.WordWrap = false;
      this.textBoxCenterX.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
      this.textBoxCenterX.Leave += new System.EventHandler(this.textBox_Leave);
      // 
      // label5
      // 
      this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label5.Location = new System.Drawing.Point(3, 531);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(68, 20);
      this.label5.TabIndex = 4;
      this.label5.Text = "Center X:";
      this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // label7
      // 
      this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label7.Location = new System.Drawing.Point(332, 531);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(74, 24);
      this.label7.TabIndex = 4;
      this.label7.Text = "Scaling:";
      this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label8.Location = new System.Drawing.Point(332, 563);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(74, 20);
      this.label8.TabIndex = 4;
      this.label8.Text = "Iterations:";
      this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // label9
      // 
      this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label9.Location = new System.Drawing.Point(3, 563);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(67, 20);
      this.label9.TabIndex = 4;
      this.label9.Text = "Center Y:";
      this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // textBoxCenterY
      // 
      this.textBoxCenterY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.textBoxCenterY.CausesValidation = false;
      this.textBoxCenterY.Location = new System.Drawing.Point(75, 560);
      this.textBoxCenterY.Name = "textBoxCenterY";
      this.textBoxCenterY.ShortcutsEnabled = false;
      this.textBoxCenterY.Size = new System.Drawing.Size(225, 27);
      this.textBoxCenterY.TabIndex = 2;
      this.textBoxCenterY.WordWrap = false;
      this.textBoxCenterY.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
      this.textBoxCenterY.Leave += new System.EventHandler(this.textBox_Leave);
      // 
      // numericUpDownIter
      // 
      this.numericUpDownIter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.numericUpDownIter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.numericUpDownIter.Location = new System.Drawing.Point(412, 561);
      this.numericUpDownIter.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
      this.numericUpDownIter.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
      this.numericUpDownIter.Name = "numericUpDownIter";
      this.numericUpDownIter.Size = new System.Drawing.Size(92, 27);
      this.numericUpDownIter.TabIndex = 4;
      this.numericUpDownIter.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
      this.numericUpDownIter.ValueChanged += new System.EventHandler(this.itervaluechanged);
      this.numericUpDownIter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.numericUpDownIter_KeyDown);
      // 
      // buttonReset
      // 
      this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonReset.Location = new System.Drawing.Point(870, 563);
      this.buttonReset.Name = "buttonReset";
      this.buttonReset.Size = new System.Drawing.Size(94, 29);
      this.buttonReset.TabIndex = 6;
      this.buttonReset.Text = "Reset";
      this.buttonReset.UseVisualStyleBackColor = true;
      this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
      // 
      // numericUpDownRound
      // 
      this.numericUpDownRound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.numericUpDownRound.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.numericUpDownRound.Location = new System.Drawing.Point(550, 561);
      this.numericUpDownRound.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.numericUpDownRound.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
      this.numericUpDownRound.Name = "numericUpDownRound";
      this.numericUpDownRound.Size = new System.Drawing.Size(56, 27);
      this.numericUpDownRound.TabIndex = 5;
      this.numericUpDownRound.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
      this.numericUpDownRound.ValueChanged += new System.EventHandler(this.itervaluechanged);
      this.numericUpDownRound.KeyDown += new System.Windows.Forms.KeyEventHandler(this.numericUpDownIter_KeyDown);
      // 
      // label10
      // 
      this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label10.Location = new System.Drawing.Point(514, 563);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(35, 21);
      this.label10.TabIndex = 4;
      this.label10.Text = "lim:";
      this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // MandelbrotPage
      // 
      this.Controls.Add(this.buttonReset);
      this.Controls.Add(this.numericUpDownRound);
      this.Controls.Add(this.numericUpDownIter);
      this.Controls.Add(this.textBoxCenterY);
      this.Controls.Add(this.textBoxCenterX);
      this.Controls.Add(this.label10);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.label9);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.textBoxScaling);
      this.Controls.Add(this.labelRelation);
      this.Controls.Add(this.splitContainer1);
      this.Name = "MandelbrotPage";
      this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
      this.Size = new System.Drawing.Size(975, 598);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel1.PerformLayout();
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIter)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRound)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

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
  }
}
