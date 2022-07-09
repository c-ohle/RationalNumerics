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
      System.Windows.Forms.Button button_save;
      this.label1 = new System.Windows.Forms.Label();
      this.panel_webview = new System.Windows.Forms.Panel();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.labelDebug = new System.Windows.Forms.Label();
      button_add = new System.Windows.Forms.Button();
      button_save = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // button_add
      // 
      button_add.Location = new System.Drawing.Point(202, 11);
      button_add.Name = "button_add";
      button_add.Size = new System.Drawing.Size(94, 29);
      button_add.TabIndex = 3;
      button_add.Text = "Run";
      button_add.UseVisualStyleBackColor = true;
      button_add.Click += new System.EventHandler(this.button_run_Click);
      // 
      // button_save
      // 
      button_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      button_save.Location = new System.Drawing.Point(878, 569);
      button_save.Name = "button_save";
      button_save.Size = new System.Drawing.Size(94, 29);
      button_save.TabIndex = 3;
      button_save.Text = "Save...";
      button_save.UseVisualStyleBackColor = true;
      button_save.Click += new System.EventHandler(this.button_save_Click);
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
      this.panel_webview.Location = new System.Drawing.Point(11, 58);
      this.panel_webview.Name = "panel_webview";
      this.panel_webview.Size = new System.Drawing.Size(861, 540);
      this.panel_webview.TabIndex = 2;
      // 
      // textBox1
      // 
      this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.textBox1.Location = new System.Drawing.Point(878, 58);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(94, 27);
      this.textBox1.TabIndex = 7;
      this.textBox1.Visible = false;
      this.textBox1.Leave += new System.EventHandler(this.textBox1_Leave);
      // 
      // labelDebug
      // 
      this.labelDebug.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.labelDebug.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(220)))));
      this.labelDebug.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.labelDebug.ForeColor = System.Drawing.SystemColors.InfoText;
      this.labelDebug.Location = new System.Drawing.Point(323, 11);
      this.labelDebug.Name = "labelDebug";
      this.labelDebug.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
      this.labelDebug.Size = new System.Drawing.Size(549, 29);
      this.labelDebug.TabIndex = 9;
      this.labelDebug.Text = "Under construction";
      this.labelDebug.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // BenchmarksPage
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.labelDebug);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(button_save);
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
    private TextBox textBox1;
    private Label labelDebug;
  }
}
