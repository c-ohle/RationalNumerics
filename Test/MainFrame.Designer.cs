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
      Test.MandelbrotPage mandelbrotPage1;
      Test.TesselatorPage tesselatorPage1;
      Test.CalculatorPage calculatorPage21;
      Test.BenchmarksPage benchmarksPage1;
      Test.PolyhedronPage polyhedronPage1;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrame));
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabPageMandel = new System.Windows.Forms.TabPage();
      this.tabPageBench = new System.Windows.Forms.TabPage();
      this.tabPagePolyh = new System.Windows.Forms.TabPage();
      this.tabPageCalc = new System.Windows.Forms.TabPage();
      this.tabPageTess = new System.Windows.Forms.TabPage();
      this.labelDebug = new System.Windows.Forms.Label();
      mandelbrotPage1 = new Test.MandelbrotPage();
      tesselatorPage1 = new Test.TesselatorPage();
      calculatorPage21 = new Test.CalculatorPage();
      benchmarksPage1 = new Test.BenchmarksPage();
      polyhedronPage1 = new Test.PolyhedronPage();
      this.tabControl.SuspendLayout();
      this.tabPageMandel.SuspendLayout();
      this.tabPageBench.SuspendLayout();
      this.tabPagePolyh.SuspendLayout();
      this.tabPageCalc.SuspendLayout();
      this.tabPageTess.SuspendLayout();
      this.SuspendLayout();
      // 
      // mandelbrotPage1
      // 
      mandelbrotPage1.Dock = System.Windows.Forms.DockStyle.Fill;
      mandelbrotPage1.Location = new System.Drawing.Point(3, 3);
      mandelbrotPage1.Name = "mandelbrotPage1";
      mandelbrotPage1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
      mandelbrotPage1.Size = new System.Drawing.Size(1004, 550);
      mandelbrotPage1.TabIndex = 0;
      // 
      // tesselatorPage1
      // 
      tesselatorPage1.Dock = System.Windows.Forms.DockStyle.Fill;
      tesselatorPage1.Location = new System.Drawing.Point(3, 3);
      tesselatorPage1.Name = "tesselatorPage1";
      tesselatorPage1.Size = new System.Drawing.Size(1004, 550);
      tesselatorPage1.TabIndex = 0;
      // 
      // calculatorPage21
      // 
      calculatorPage21.AutoSize = true;
      calculatorPage21.Dock = System.Windows.Forms.DockStyle.Fill;
      calculatorPage21.Location = new System.Drawing.Point(3, 3);
      calculatorPage21.Name = "calculatorPage21";
      calculatorPage21.Size = new System.Drawing.Size(1004, 550);
      calculatorPage21.TabIndex = 0;
      // 
      // benchmarksPage1
      // 
      benchmarksPage1.Dock = System.Windows.Forms.DockStyle.Fill;
      benchmarksPage1.Location = new System.Drawing.Point(3, 3);
      benchmarksPage1.Name = "benchmarksPage1";
      benchmarksPage1.Size = new System.Drawing.Size(1004, 550);
      benchmarksPage1.TabIndex = 0;
      // 
      // polyhedronPage1
      // 
      polyhedronPage1.Dock = System.Windows.Forms.DockStyle.Fill;
      polyhedronPage1.Location = new System.Drawing.Point(3, 3);
      polyhedronPage1.Name = "polyhedronPage1";
      polyhedronPage1.Size = new System.Drawing.Size(1004, 550);
      polyhedronPage1.TabIndex = 0;
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
      this.tabPageMandel.Controls.Add(mandelbrotPage1);
      this.tabPageMandel.Location = new System.Drawing.Point(4, 29);
      this.tabPageMandel.Name = "tabPageMandel";
      this.tabPageMandel.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageMandel.Size = new System.Drawing.Size(1010, 556);
      this.tabPageMandel.TabIndex = 0;
      this.tabPageMandel.Text = "Mandelbrot";
      this.tabPageMandel.UseVisualStyleBackColor = true;
      // 
      // tabPageBench
      // 
      this.tabPageBench.Controls.Add(tesselatorPage1);
      this.tabPageBench.Location = new System.Drawing.Point(4, 29);
      this.tabPageBench.Name = "tabPageBench";
      this.tabPageBench.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageBench.Size = new System.Drawing.Size(1010, 556);
      this.tabPageBench.TabIndex = 2;
      this.tabPageBench.Text = "Tesselation";
      this.tabPageBench.UseVisualStyleBackColor = true;
      // 
      // tabPagePolyh
      // 
      this.tabPagePolyh.Controls.Add(polyhedronPage1);
      this.tabPagePolyh.Location = new System.Drawing.Point(4, 29);
      this.tabPagePolyh.Name = "tabPagePolyh";
      this.tabPagePolyh.Padding = new System.Windows.Forms.Padding(3);
      this.tabPagePolyh.Size = new System.Drawing.Size(1010, 556);
      this.tabPagePolyh.TabIndex = 4;
      this.tabPagePolyh.Text = "Polyhedron";
      this.tabPagePolyh.UseVisualStyleBackColor = true;
      // 
      // tabPageCalc
      // 
      this.tabPageCalc.Controls.Add(calculatorPage21);
      this.tabPageCalc.Location = new System.Drawing.Point(4, 29);
      this.tabPageCalc.Name = "tabPageCalc";
      this.tabPageCalc.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageCalc.Size = new System.Drawing.Size(1010, 556);
      this.tabPageCalc.TabIndex = 3;
      this.tabPageCalc.Text = "Calculator";
      this.tabPageCalc.UseVisualStyleBackColor = true;
      // 
      // tabPageTess
      // 
      this.tabPageTess.Controls.Add(benchmarksPage1);
      this.tabPageTess.Location = new System.Drawing.Point(4, 29);
      this.tabPageTess.Name = "tabPageTess";
      this.tabPageTess.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTess.Size = new System.Drawing.Size(1010, 556);
      this.tabPageTess.TabIndex = 1;
      this.tabPageTess.Text = "Benchmarks";
      this.tabPageTess.UseVisualStyleBackColor = true;
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
      // MainFrame
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(1018, 621);
      this.Controls.Add(this.tabControl);
      this.Controls.Add(this.labelDebug);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimumSize = new System.Drawing.Size(800, 600);
      this.Name = "MainFrame";
      this.Text = "BigRational Test";
      this.tabControl.ResumeLayout(false);
      this.tabPageMandel.ResumeLayout(false);
      this.tabPageBench.ResumeLayout(false);
      this.tabPagePolyh.ResumeLayout(false);
      this.tabPageCalc.ResumeLayout(false);
      this.tabPageCalc.PerformLayout();
      this.tabPageTess.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private TabControl tabControl;
    private TabPage tabPageMandel;
    private TabPage tabPageTess;
    private TabPage tabPagePolyh;
    private TabPage tabPageCalc;
    private TabPage tabPageBench;
    private Label labelDebug;
  }
}