namespace Test
{
  partial class TesselatorPage
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
      this.components = new System.ComponentModel.Container();
      this.label1 = new System.Windows.Forms.Label();
      this.tesselatorView1 = new Test.TesselatorView();
      this.comboBoxWinding = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.checkBoxDrawSurface = new System.Windows.Forms.CheckBox();
      this.checkBoxDrawMesh = new System.Windows.Forms.CheckBox();
      this.checkBoxDrawOutlines = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.checkBoxPolygons = new System.Windows.Forms.CheckBox();
      this.checkBoxPoints = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.checkBox2 = new System.Windows.Forms.CheckBox();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.checkBoxDelaunay = new System.Windows.Forms.CheckBox();
      this.labelStatus = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.buttonReset = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label1.Location = new System.Drawing.Point(11, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(322, 38);
      this.label1.TabIndex = 0;
      this.label1.Text = "NewRational Tesselator";
      // 
      // tesselatorView1
      // 
      this.tesselatorView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tesselatorView1.Delaunay = true;
      this.tesselatorView1.DrawMesh = true;
      this.tesselatorView1.DrawOutlines = true;
      this.tesselatorView1.DrawPoints = false;
      this.tesselatorView1.DrawPolygons = true;
      this.tesselatorView1.DrawSurface = true;
      this.tesselatorView1.Location = new System.Drawing.Point(16, 47);
      this.tesselatorView1.Name = "tesselatorView1";
      this.tesselatorView1.Size = new System.Drawing.Size(728, 511);
      this.tesselatorView1.TabIndex = 0;
      this.tesselatorView1.TabStop = false;
      this.tesselatorView1.Winding = Test.Winding.EvenOdd;
      // 
      // comboBoxWinding
      // 
      this.comboBoxWinding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxWinding.FormattingEnabled = true;
      this.comboBoxWinding.Items.AddRange(new object[] {
            "EvenOdd",
            "NonZero",
            "Positive",
            "Negative",
            "AbsGeqTwo",
            "AbsGeqThree"});
      this.comboBoxWinding.Location = new System.Drawing.Point(16, 56);
      this.comboBoxWinding.Name = "comboBoxWinding";
      this.comboBoxWinding.Size = new System.Drawing.Size(162, 28);
      this.comboBoxWinding.TabIndex = 3;
      this.toolTip1.SetToolTip(this.comboBoxWinding, "The polygon winding mode for the tessellation.");
      this.comboBoxWinding.SelectedIndexChanged += new System.EventHandler(this.comboBoxWinding_SelectedIndexChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 33);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(68, 20);
      this.label2.TabIndex = 0;
      this.label2.Text = "Winding:";
      // 
      // checkBoxDrawSurface
      // 
      this.checkBoxDrawSurface.AutoSize = true;
      this.checkBoxDrawSurface.Checked = true;
      this.checkBoxDrawSurface.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxDrawSurface.Location = new System.Drawing.Point(16, 34);
      this.checkBoxDrawSurface.Name = "checkBoxDrawSurface";
      this.checkBoxDrawSurface.Size = new System.Drawing.Size(80, 24);
      this.checkBoxDrawSurface.TabIndex = 7;
      this.checkBoxDrawSurface.Text = "Surface";
      this.toolTip1.SetToolTip(this.checkBoxDrawSurface, "Fills the result mesh polygons with yellow.");
      this.checkBoxDrawSurface.UseVisualStyleBackColor = true;
      this.checkBoxDrawSurface.CheckedChanged += new System.EventHandler(this.checkBoxDraw_CheckedChanged);
      // 
      // checkBoxDrawMesh
      // 
      this.checkBoxDrawMesh.AutoSize = true;
      this.checkBoxDrawMesh.Location = new System.Drawing.Point(16, 64);
      this.checkBoxDrawMesh.Name = "checkBoxDrawMesh";
      this.checkBoxDrawMesh.Size = new System.Drawing.Size(66, 24);
      this.checkBoxDrawMesh.TabIndex = 8;
      this.checkBoxDrawMesh.Text = "Mesh";
      this.toolTip1.SetToolTip(this.checkBoxDrawMesh, "Draws the result mesh as gray lines.");
      this.checkBoxDrawMesh.UseVisualStyleBackColor = true;
      this.checkBoxDrawMesh.CheckedChanged += new System.EventHandler(this.checkBoxDraw_CheckedChanged);
      // 
      // checkBoxDrawOutlines
      // 
      this.checkBoxDrawOutlines.AutoSize = true;
      this.checkBoxDrawOutlines.Checked = true;
      this.checkBoxDrawOutlines.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxDrawOutlines.Location = new System.Drawing.Point(16, 94);
      this.checkBoxDrawOutlines.Name = "checkBoxDrawOutlines";
      this.checkBoxDrawOutlines.Size = new System.Drawing.Size(79, 24);
      this.checkBoxDrawOutlines.TabIndex = 9;
      this.checkBoxDrawOutlines.Text = "Outline";
      this.toolTip1.SetToolTip(this.checkBoxDrawOutlines, "Draws the result outlines as black lines");
      this.checkBoxDrawOutlines.UseVisualStyleBackColor = true;
      this.checkBoxDrawOutlines.CheckedChanged += new System.EventHandler(this.checkBoxDraw_CheckedChanged);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.checkBoxPolygons);
      this.groupBox1.Controls.Add(this.checkBoxPoints);
      this.groupBox1.Location = new System.Drawing.Point(761, 47);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(196, 103);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Display Input ";
      // 
      // checkBoxPolygons
      // 
      this.checkBoxPolygons.AutoSize = true;
      this.checkBoxPolygons.Checked = true;
      this.checkBoxPolygons.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxPolygons.Location = new System.Drawing.Point(16, 36);
      this.checkBoxPolygons.Name = "checkBoxPolygons";
      this.checkBoxPolygons.Size = new System.Drawing.Size(98, 24);
      this.checkBoxPolygons.TabIndex = 1;
      this.checkBoxPolygons.Text = "Polygones";
      this.toolTip1.SetToolTip(this.checkBoxPolygons, "Draws the input polygons as blue lines.");
      this.checkBoxPolygons.UseVisualStyleBackColor = true;
      this.checkBoxPolygons.CheckedChanged += new System.EventHandler(this.checkBoxDraw_CheckedChanged);
      // 
      // checkBoxPoints
      // 
      this.checkBoxPoints.AutoSize = true;
      this.checkBoxPoints.Location = new System.Drawing.Point(16, 66);
      this.checkBoxPoints.Name = "checkBoxPoints";
      this.checkBoxPoints.Size = new System.Drawing.Size(70, 24);
      this.checkBoxPoints.TabIndex = 2;
      this.checkBoxPoints.Text = "Points";
      this.toolTip1.SetToolTip(this.checkBoxPoints, "Draws the input polygon points.");
      this.checkBoxPoints.UseVisualStyleBackColor = true;
      this.checkBoxPoints.CheckedChanged += new System.EventHandler(this.checkBoxDraw_CheckedChanged);
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.checkBoxDrawSurface);
      this.groupBox2.Controls.Add(this.checkBoxDrawMesh);
      this.groupBox2.Controls.Add(this.checkBoxDrawOutlines);
      this.groupBox2.Location = new System.Drawing.Point(761, 359);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(196, 135);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Display Output";
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.checkBox2);
      this.groupBox3.Controls.Add(this.checkBox1);
      this.groupBox3.Controls.Add(this.checkBoxDelaunay);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.Controls.Add(this.comboBoxWinding);
      this.groupBox3.Location = new System.Drawing.Point(761, 156);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(196, 198);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Tesselation";
      // 
      // checkBox2
      // 
      this.checkBox2.AutoSize = true;
      this.checkBox2.Checked = true;
      this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBox2.Location = new System.Drawing.Point(16, 123);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(109, 24);
      this.checkBox2.TabIndex = 5;
      this.checkBox2.Text = "Outline opt.";
      this.toolTip1.SetToolTip(this.checkBox2, "Tessellator ensures that the outlines are always positively oriented in case of s" +
        "elf intersections.");
      this.checkBox2.UseVisualStyleBackColor = true;
      this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBoxDraw_CheckedChanged);
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.Checked = true;
      this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBox1.Location = new System.Drawing.Point(16, 153);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(60, 24);
      this.checkBox1.TabIndex = 6;
      this.checkBox1.Text = "Trim";
      this.toolTip1.SetToolTip(this.checkBox1, "Tesselator eliminates unnecessary output vertices.\r\n");
      this.checkBox1.UseVisualStyleBackColor = true;
      this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBoxDraw_CheckedChanged);
      // 
      // checkBoxDelaunay
      // 
      this.checkBoxDelaunay.AutoSize = true;
      this.checkBoxDelaunay.Checked = true;
      this.checkBoxDelaunay.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxDelaunay.Location = new System.Drawing.Point(16, 93);
      this.checkBoxDelaunay.Name = "checkBoxDelaunay";
      this.checkBoxDelaunay.Size = new System.Drawing.Size(123, 24);
      this.checkBoxDelaunay.TabIndex = 4;
      this.checkBoxDelaunay.Text = "Delaunay opt.";
      this.toolTip1.SetToolTip(this.checkBoxDelaunay, "Tesselation with Delaunay mesh optimization.");
      this.checkBoxDelaunay.UseVisualStyleBackColor = true;
      this.checkBoxDelaunay.CheckedChanged += new System.EventHandler(this.checkBoxDraw_CheckedChanged);
      // 
      // labelStatus
      // 
      this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelStatus.AutoSize = true;
      this.labelStatus.Location = new System.Drawing.Point(11, 565);
      this.labelStatus.Name = "labelStatus";
      this.labelStatus.Size = new System.Drawing.Size(214, 20);
      this.labelStatus.TabIndex = 0;
      this.labelStatus.Text = "(Wheel to zoom, click to scroll)";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(338, 18);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(380, 20);
      this.label3.TabIndex = 0;
      this.label3.Text = "High-precision tessellation based on rational arithmetic.";
      // 
      // buttonReset
      // 
      this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonReset.Location = new System.Drawing.Point(863, 561);
      this.buttonReset.Name = "buttonReset";
      this.buttonReset.Size = new System.Drawing.Size(94, 29);
      this.buttonReset.TabIndex = 4;
      this.buttonReset.Text = "Reset";
      this.buttonReset.UseVisualStyleBackColor = true;
      this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
      // 
      // TesselatorPage
      // 
      //this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.buttonReset);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.labelStatus);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.tesselatorView1);
      this.Controls.Add(this.label1);
      this.Name = "TesselatorPage";
      this.Size = new System.Drawing.Size(975, 598);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Label label1;
    private TesselatorView tesselatorView1;
    private ComboBox comboBoxWinding;
    private Label label2;
    private CheckBox checkBoxDrawSurface;
    private CheckBox checkBoxDrawMesh;
    private CheckBox checkBoxDrawOutlines;
    private GroupBox groupBox1;
    private CheckBox checkBoxPolygons;
    private CheckBox checkBoxPoints;
    private GroupBox groupBox2;
    private GroupBox groupBox3;
    private CheckBox checkBoxDelaunay;
    private Label labelStatus;
    private Label label3;
    private CheckBox checkBox2;
    private CheckBox checkBox1;
    private ToolTip toolTip1;
    private Button buttonReset;
  }
}
