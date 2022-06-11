namespace Test
{
  partial class BenchmarksPage
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
      System.Windows.Forms.Button button_add;
      System.Windows.Forms.Button button_add2;
      System.Windows.Forms.Button button_clear;
      System.Windows.Forms.Button button_save;
      System.Windows.Forms.Button button_add3;
      this.label1 = new System.Windows.Forms.Label();
      this.panel_webview = new System.Windows.Forms.Panel();
      this.label2 = new System.Windows.Forms.Label();
      button_add = new System.Windows.Forms.Button();
      button_add2 = new System.Windows.Forms.Button();
      button_clear = new System.Windows.Forms.Button();
      button_save = new System.Windows.Forms.Button();
      button_add3 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // button_add
      // 
      button_add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      button_add.Location = new System.Drawing.Point(878, 65);
      button_add.Name = "button_add";
      button_add.Size = new System.Drawing.Size(94, 29);
      button_add.TabIndex = 3;
      button_add.Text = "test1";
      button_add.UseVisualStyleBackColor = true;
      button_add.Click += new System.EventHandler(this.button_add_Click);
      // 
      // button_add2
      // 
      button_add2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      button_add2.Location = new System.Drawing.Point(878, 100);
      button_add2.Name = "button_add2";
      button_add2.Size = new System.Drawing.Size(94, 29);
      button_add2.TabIndex = 3;
      button_add2.Text = "test2";
      button_add2.UseVisualStyleBackColor = true;
      button_add2.Click += new System.EventHandler(this.button_add2_Click);
      // 
      // button_clear
      // 
      button_clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      button_clear.Location = new System.Drawing.Point(878, 566);
      button_clear.Name = "button_clear";
      button_clear.Size = new System.Drawing.Size(94, 29);
      button_clear.TabIndex = 3;
      button_clear.Text = "Reset";
      button_clear.UseVisualStyleBackColor = true;
      button_clear.Click += new System.EventHandler(this.button_clear_Click);
      // 
      // button_save
      // 
      button_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      button_save.Location = new System.Drawing.Point(878, 531);
      button_save.Name = "button_save";
      button_save.Size = new System.Drawing.Size(94, 29);
      button_save.TabIndex = 3;
      button_save.Text = "Save...";
      button_save.UseVisualStyleBackColor = true;
      button_save.Click += new System.EventHandler(this.button_save_Click);
      // 
      // button_add3
      // 
      button_add3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      button_add3.Location = new System.Drawing.Point(878, 135);
      button_add3.Name = "button_add3";
      button_add3.Size = new System.Drawing.Size(94, 29);
      button_add3.TabIndex = 3;
      button_add3.Text = "test3";
      button_add3.UseVisualStyleBackColor = true;
      button_add3.Click += new System.EventHandler(this.button_add3_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label1.Location = new System.Drawing.Point(11, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(177, 38);
      this.label1.TabIndex = 1;
      this.label1.Text = "Benchmarks";
      // 
      // panel_webview
      // 
      this.panel_webview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel_webview.Location = new System.Drawing.Point(11, 65);
      this.panel_webview.Name = "panel_webview";
      this.panel_webview.Size = new System.Drawing.Size(861, 533);
      this.panel_webview.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(194, 17);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(142, 20);
      this.label2.TabIndex = 6;
      this.label2.Text = "(under construction)";
      // 
      // BenchmarksPage
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.label2);
      this.Controls.Add(button_clear);
      this.Controls.Add(button_save);
      this.Controls.Add(button_add3);
      this.Controls.Add(button_add2);
      this.Controls.Add(button_add);
      this.Controls.Add(this.panel_webview);
      this.Controls.Add(this.label1);
      this.Name = "BenchmarksPage";
      this.Size = new System.Drawing.Size(975, 598);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Label label1;
    private Panel panel_webview;
    private Label label2;
  }
}
