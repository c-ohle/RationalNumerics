namespace Test
{
  partial class MainFrame
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabPageMandel = new System.Windows.Forms.TabPage();
      this.mandelbrotPage1 = new Test.MandelbrotPage();
      this.tabPageBench = new System.Windows.Forms.TabPage();
      this.tesselatorPage1 = new Test.TesselatorPage();
      this.tabPagePolyh = new System.Windows.Forms.TabPage();
      this.label2 = new System.Windows.Forms.Label();
      this.tabPageCalc = new System.Windows.Forms.TabPage();
      this.label3 = new System.Windows.Forms.Label();
      this.calculatorPage1 = new Test.CalculatorPage();
      this.tabPageTess = new System.Windows.Forms.TabPage();
      this.label1 = new System.Windows.Forms.Label();
      this.labelDebug = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.tabControl.SuspendLayout();
      this.tabPageMandel.SuspendLayout();
      this.tabPageBench.SuspendLayout();
      this.tabPagePolyh.SuspendLayout();
      this.tabPageCalc.SuspendLayout();
      this.tabPageTess.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      this.tabControl.Controls.Add(this.tabPageMandel);
      this.tabControl.Controls.Add(this.tabPageBench);
      this.tabControl.Controls.Add(this.tabPagePolyh);
      this.tabControl.Controls.Add(this.tabPageCalc);
      this.tabControl.Controls.Add(this.tabPageTess);
      this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl.Location = new System.Drawing.Point(0, 32);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(1018, 589);
      this.tabControl.TabIndex = 0;
      // 
      // tabPageMandel
      // 
      this.tabPageMandel.Controls.Add(this.mandelbrotPage1);
      this.tabPageMandel.Location = new System.Drawing.Point(4, 29);
      this.tabPageMandel.Name = "tabPageMandel";
      this.tabPageMandel.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageMandel.Size = new System.Drawing.Size(1010, 556);
      this.tabPageMandel.TabIndex = 0;
      this.tabPageMandel.Text = "Mandelbrot";
      this.tabPageMandel.UseVisualStyleBackColor = true;
      // 
      // mandelbrotPage1
      // 
      this.mandelbrotPage1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mandelbrotPage1.Location = new System.Drawing.Point(3, 3);
      this.mandelbrotPage1.Name = "mandelbrotPage1";
      this.mandelbrotPage1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
      this.mandelbrotPage1.Size = new System.Drawing.Size(1004, 550);
      this.mandelbrotPage1.TabIndex = 0;
      // 
      // tabPageBench
      // 
      this.tabPageBench.Controls.Add(this.tesselatorPage1);
      this.tabPageBench.Location = new System.Drawing.Point(4, 29);
      this.tabPageBench.Name = "tabPageBench";
      this.tabPageBench.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageBench.Size = new System.Drawing.Size(1010, 556);
      this.tabPageBench.TabIndex = 2;
      this.tabPageBench.Text = "Tesselation";
      this.tabPageBench.UseVisualStyleBackColor = true;
      // 
      // tesselatorPage1
      // 
      this.tesselatorPage1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tesselatorPage1.Location = new System.Drawing.Point(3, 3);
      this.tesselatorPage1.Name = "tesselatorPage1";
      this.tesselatorPage1.Size = new System.Drawing.Size(1004, 550);
      this.tesselatorPage1.TabIndex = 0;
      // 
      // tabPagePolyh
      // 
      this.tabPagePolyh.Controls.Add(this.label2);
      this.tabPagePolyh.Location = new System.Drawing.Point(4, 29);
      this.tabPagePolyh.Name = "tabPagePolyh";
      this.tabPagePolyh.Padding = new System.Windows.Forms.Padding(3);
      this.tabPagePolyh.Size = new System.Drawing.Size(1010, 556);
      this.tabPagePolyh.TabIndex = 4;
      this.tabPagePolyh.Text = "Polyhedron";
      this.tabPagePolyh.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label2.Location = new System.Drawing.Point(14, 7);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(269, 38);
      this.label2.TabIndex = 1;
      this.label2.Text = "Under construction";
      // 
      // tabPageCalc
      // 
      this.tabPageCalc.Controls.Add(this.label4);
      this.tabPageCalc.Controls.Add(this.label3);
      this.tabPageCalc.Controls.Add(this.calculatorPage1);
      this.tabPageCalc.Location = new System.Drawing.Point(4, 29);
      this.tabPageCalc.Name = "tabPageCalc";
      this.tabPageCalc.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageCalc.Size = new System.Drawing.Size(1010, 556);
      this.tabPageCalc.TabIndex = 3;
      this.tabPageCalc.Text = "Calculator";
      this.tabPageCalc.UseVisualStyleBackColor = true;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label3.Location = new System.Drawing.Point(14, 7);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(325, 38);
      this.label3.TabIndex = 2;
      this.label3.Text = "NewRational Calculator";
      // 
      // calculatorPage1
      // 
      this.calculatorPage1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.calculatorPage1.Location = new System.Drawing.Point(14, 52);
      this.calculatorPage1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.calculatorPage1.Name = "calculatorPage1";
      this.calculatorPage1.Size = new System.Drawing.Size(729, 504);
      this.calculatorPage1.TabIndex = 0;
      // 
      // tabPageTess
      // 
      this.tabPageTess.Controls.Add(this.label1);
      this.tabPageTess.Location = new System.Drawing.Point(4, 29);
      this.tabPageTess.Name = "tabPageTess";
      this.tabPageTess.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTess.Size = new System.Drawing.Size(1010, 556);
      this.tabPageTess.TabIndex = 1;
      this.tabPageTess.Text = "Benchmarks";
      this.tabPageTess.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label1.Location = new System.Drawing.Point(14, 7);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(269, 38);
      this.label1.TabIndex = 0;
      this.label1.Text = "Under construction";
      // 
      // labelDebug
      // 
      this.labelDebug.BackColor = System.Drawing.SystemColors.Info;
      this.labelDebug.Dock = System.Windows.Forms.DockStyle.Top;
      this.labelDebug.ForeColor = System.Drawing.SystemColors.InfoText;
      this.labelDebug.Location = new System.Drawing.Point(0, 0);
      this.labelDebug.Name = "labelDebug";
      this.labelDebug.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
      this.labelDebug.Size = new System.Drawing.Size(1018, 32);
      this.labelDebug.TabIndex = 1;
      this.labelDebug.Text = "Application is in debug or a debugger is attached - benchmarks and comparisons ar" +
    "e not realistic !";
      this.labelDebug.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(345, 21);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(144, 20);
      this.label4.TabIndex = 3;
      this.label4.Text = "(Under construction)";
      // 
      // MainFrame
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(1018, 621);
      this.Controls.Add(this.tabControl);
      this.Controls.Add(this.labelDebug);
      this.MinimumSize = new System.Drawing.Size(800, 600);
      this.Name = "MainFrame";
      this.Text = "NewRational Test";
      this.tabControl.ResumeLayout(false);
      this.tabPageMandel.ResumeLayout(false);
      this.tabPageBench.ResumeLayout(false);
      this.tabPagePolyh.ResumeLayout(false);
      this.tabPagePolyh.PerformLayout();
      this.tabPageCalc.ResumeLayout(false);
      this.tabPageCalc.PerformLayout();
      this.tabPageTess.ResumeLayout(false);
      this.tabPageTess.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private TabControl tabControl;
    private TabPage tabPageMandel;
    private TabPage tabPageTess;
    private TabPage tabPagePolyh;
    private TabPage tabPageCalc;
    private TabPage tabPageBench;
    private MandelbrotPage mandelbrotPage1;
    private Label labelDebug;
    private Label label1;
    private TesselatorPage tesselatorPage1;
    private Label label2;
    private Label label3;
    private CalculatorPage calculatorPage1;
    private Label label4;
  }
}