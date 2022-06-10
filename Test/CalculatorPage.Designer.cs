namespace Test
{
  partial class CalculatorPage
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
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.panel_hist = new System.Windows.Forms.Panel();
      this.basket = new System.Windows.Forms.Button();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.calculatorPage1 = new Test.CalculatorView();
      this.label3 = new System.Windows.Forms.Label();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(706, 3);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(269, 595);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.panel_hist);
      this.tabPage1.Controls.Add(this.basket);
      this.tabPage1.Location = new System.Drawing.Point(4, 29);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(261, 562);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "History";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // panel_hist
      // 
      this.panel_hist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel_hist.AutoScroll = true;
      this.panel_hist.Location = new System.Drawing.Point(6, 6);
      this.panel_hist.Name = "panel_hist";
      this.panel_hist.Size = new System.Drawing.Size(249, 511);
      this.panel_hist.TabIndex = 5;
      // 
      // basket
      // 
      this.basket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.basket.BackColor = System.Drawing.SystemColors.InactiveBorder;
      this.basket.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.basket.Location = new System.Drawing.Point(215, 519);
      this.basket.Name = "basket";
      this.basket.Size = new System.Drawing.Size(40, 37);
      this.basket.TabIndex = 4;
      this.basket.Text = "";
      this.basket.UseVisualStyleBackColor = false;
      this.basket.Click += new System.EventHandler(this.basket_Click);
      // 
      // tabPage2
      // 
      this.tabPage2.Location = new System.Drawing.Point(4, 29);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(283, 535);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Memory";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // calculatorPage1
      // 
      this.calculatorPage1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.calculatorPage1.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.calculatorPage1.Location = new System.Drawing.Point(0, 44);
      this.calculatorPage1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
      this.calculatorPage1.Name = "calculatorPage1";
      this.calculatorPage1.Size = new System.Drawing.Size(702, 516);
      this.calculatorPage1.TabIndex = 1;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label3.Location = new System.Drawing.Point(11, 3);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(309, 38);
      this.label3.TabIndex = 3;
      this.label3.Text = "BigRational Calculator";
      // 
      // CalculatorPage
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.label3);
      this.Controls.Add(this.calculatorPage1);
      this.Controls.Add(this.tabControl1);
      this.Name = "CalculatorPage";
      this.Size = new System.Drawing.Size(975, 598);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private CalculatorView calculatorPage1;
    private Label label3;
    private Button basket;
    internal Panel panel_hist;
  }
}
