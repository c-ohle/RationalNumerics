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
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mandelbrotPage1 = new Test.MandelbrotPage();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.labelDebug = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.tabPage3 = new System.Windows.Forms.TabPage();
      this.label2 = new System.Windows.Forms.Label();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 24);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(1018, 597);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mandelbrotPage1);
      this.tabPage1.Location = new System.Drawing.Point(4, 29);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(1010, 564);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Mandelbrot";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mandelbrotPage1
      // 
      this.mandelbrotPage1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mandelbrotPage1.Location = new System.Drawing.Point(3, 3);
      this.mandelbrotPage1.Name = "mandelbrotPage1";
      this.mandelbrotPage1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
      this.mandelbrotPage1.Size = new System.Drawing.Size(1004, 558);
      this.mandelbrotPage1.TabIndex = 0;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.label1);
      this.tabPage2.Location = new System.Drawing.Point(4, 29);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(1010, 564);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Benchmarks";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // labelDebug
      // 
      this.labelDebug.BackColor = System.Drawing.SystemColors.Info;
      this.labelDebug.Dock = System.Windows.Forms.DockStyle.Top;
      this.labelDebug.ForeColor = System.Drawing.SystemColors.InfoText;
      this.labelDebug.Location = new System.Drawing.Point(0, 0);
      this.labelDebug.Name = "labelDebug";
      this.labelDebug.Size = new System.Drawing.Size(1018, 24);
      this.labelDebug.TabIndex = 1;
      this.labelDebug.Text = "Application is in debug or a debugger is attached - benchmarks and comparisons ar" +
    "e not realistic !";
      this.labelDebug.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label1.Location = new System.Drawing.Point(30, 29);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(269, 38);
      this.label1.TabIndex = 0;
      this.label1.Text = "Under construction";
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.label2);
      this.tabPage3.Location = new System.Drawing.Point(4, 29);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage3.Size = new System.Drawing.Size(1010, 564);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Tesselation";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label2.Location = new System.Drawing.Point(30, 29);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(269, 38);
      this.label2.TabIndex = 1;
      this.label2.Text = "Under construction";
      // 
      // MainFrame
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1018, 621);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.labelDebug);
      this.MinimumSize = new System.Drawing.Size(800, 600);
      this.Name = "MainFrame";
      this.Text = "NewRational Test";
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage2.PerformLayout();
      this.tabPage3.ResumeLayout(false);
      this.tabPage3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private MandelbrotPage mandelbrotPage1;
    private Label labelDebug;
    private Label label1;
    private TabPage tabPage3;
    private Label label2;
  }
}