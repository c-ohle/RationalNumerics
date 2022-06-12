namespace Test
{
  partial class PolyhedronPage
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
      System.Windows.Forms.ToolStripSeparator _1;
      Test.MenuItem redo_back;
      Test.MenuItem undo_back;
      Test.MenuItem props;
      Test.MenuItem undo_z;
      Test.MenuItem redo_y;
      System.Windows.Forms.ToolStripSeparator _2;
      Test.MenuItem cut;
      Test.MenuItem copy;
      Test.MenuItem paste;
      Test.MenuItem del;
      Test.MenuItem undo2_z;
      Test.MenuItem redo2_y;
      Test.MenuItem undo2_back;
      Test.MenuItem redo2_back;
      this.label1 = new System.Windows.Forms.Label();
      this.contextMenuView = new Test.ContextMenu(this.components);
      this._model = new Test.MenuItem();
      this._ggroup = new Test.MenuItem();
      this._ungroup = new Test.MenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this._band = new Test.MenuItem();
      this._bdiiff = new Test.MenuItem();
      this._bint = new Test.MenuItem();
      this._bhalf = new Test.MenuItem();
      this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
      this._conv = new Test.MenuItem();
      this.menuItem5 = new Test.MenuItem();
      this._setings = new Test.MenuItem();
      this._selstyle = new Test.MenuItem();
      this._selbox = new Test.MenuItem();
      this._selpiv = new Test.MenuItem();
      this._selwire = new Test.MenuItem();
      this._selnorm = new Test.MenuItem();
      this._drv = new Test.MenuItem();
      this.menuItem7 = new Test.MenuItem();
      this._samples = new Test.MenuItem();
      this.menuItem8 = new Test.MenuItem();
      this._3 = new System.Windows.Forms.ToolStripSeparator();
      this.label2 = new System.Windows.Forms.Label();
      this.modelView = new Test.DX11ModelCtrl();
      this.propsView = new Test.DX11PropsCtrl();
      this.contextMenuPropsView = new Test.ContextMenu(this.components);
      this.panel1 = new System.Windows.Forms.Panel();
      this.label3 = new System.Windows.Forms.Label();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      _1 = new System.Windows.Forms.ToolStripSeparator();
      redo_back = new Test.MenuItem();
      undo_back = new Test.MenuItem();
      props = new Test.MenuItem();
      undo_z = new Test.MenuItem();
      redo_y = new Test.MenuItem();
      _2 = new System.Windows.Forms.ToolStripSeparator();
      cut = new Test.MenuItem();
      copy = new Test.MenuItem();
      paste = new Test.MenuItem();
      del = new Test.MenuItem();
      undo2_z = new Test.MenuItem();
      redo2_y = new Test.MenuItem();
      undo2_back = new Test.MenuItem();
      redo2_back = new Test.MenuItem();
      this.contextMenuView.SuspendLayout();
      this.contextMenuPropsView.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // _1
      // 
      _1.Name = "_1";
      _1.Size = new System.Drawing.Size(214, 6);
      // 
      // redo_back
      // 
      redo_back.Id = 2001;
      redo_back.Name = "redo_back";
      redo_back.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Back)));
      redo_back.Size = new System.Drawing.Size(217, 24);
      redo_back.Text = "Redo";
      redo_back.Visible = false;
      // 
      // undo_back
      // 
      undo_back.Id = 2000;
      undo_back.Name = "undo_back";
      undo_back.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Back)));
      undo_back.Size = new System.Drawing.Size(217, 24);
      undo_back.Text = "Undo";
      undo_back.Visible = false;
      // 
      // props
      // 
      props.Id = 2008;
      props.Name = "props";
      props.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Return)));
      props.Size = new System.Drawing.Size(217, 24);
      props.Text = "Properties";
      // 
      // undo_z
      // 
      undo_z.Id = 2000;
      undo_z.Name = "undo_z";
      undo_z.ShortcutKeyDisplayString = "";
      undo_z.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
      undo_z.Size = new System.Drawing.Size(217, 24);
      undo_z.Text = "Undo";
      // 
      // redo_y
      // 
      redo_y.Id = 2001;
      redo_y.Name = "redo_y";
      redo_y.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
      redo_y.Size = new System.Drawing.Size(217, 24);
      redo_y.Text = "Redo";
      // 
      // _2
      // 
      _2.Name = "_2";
      _2.Size = new System.Drawing.Size(214, 6);
      // 
      // cut
      // 
      cut.Id = 2002;
      cut.Name = "cut";
      cut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
      cut.Size = new System.Drawing.Size(217, 24);
      cut.Text = "Cut";
      // 
      // copy
      // 
      copy.Id = 2003;
      copy.Name = "copy";
      copy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
      copy.Size = new System.Drawing.Size(217, 24);
      copy.Text = "Copy";
      // 
      // paste
      // 
      paste.Id = 2004;
      paste.Name = "paste";
      paste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
      paste.Size = new System.Drawing.Size(217, 24);
      paste.Text = "Paste";
      // 
      // del
      // 
      del.Id = 2005;
      del.Name = "del";
      del.ShortcutKeys = System.Windows.Forms.Keys.Delete;
      del.Size = new System.Drawing.Size(217, 24);
      del.Text = "Delete";
      del.Visible = false;
      // 
      // undo2_z
      // 
      undo2_z.Id = 2000;
      undo2_z.Name = "undo2_z";
      undo2_z.ShortcutKeyDisplayString = "";
      undo2_z.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
      undo2_z.Size = new System.Drawing.Size(186, 24);
      undo2_z.Text = "Undo";
      // 
      // redo2_y
      // 
      redo2_y.Id = 2001;
      redo2_y.Name = "redo2_y";
      redo2_y.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
      redo2_y.Size = new System.Drawing.Size(186, 24);
      redo2_y.Text = "Redo";
      // 
      // undo2_back
      // 
      undo2_back.Id = 2000;
      undo2_back.Name = "undo2_back";
      undo2_back.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Back)));
      undo2_back.Size = new System.Drawing.Size(186, 24);
      undo2_back.Text = "Undo";
      undo2_back.Visible = false;
      // 
      // redo2_back
      // 
      redo2_back.Id = 2001;
      redo2_back.Name = "redo2_back";
      redo2_back.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Back)));
      redo2_back.Size = new System.Drawing.Size(186, 24);
      redo2_back.Text = "Redo";
      redo2_back.Visible = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label1.Location = new System.Drawing.Point(11, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(327, 38);
      this.label1.TabIndex = 0;
      this.label1.Text = "BigRational Polyhedron";
      // 
      // contextMenuView
      // 
      this.contextMenuView.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.contextMenuView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            undo_z,
            redo_y,
            undo_back,
            redo_back,
            _1,
            cut,
            copy,
            paste,
            del,
            this._model,
            _2,
            this._setings,
            this._3,
            props});
      this.contextMenuView.Name = "contextMenu1";
      this.contextMenuView.Size = new System.Drawing.Size(218, 286);
      // 
      // _model
      // 
      this._model.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._ggroup,
            this._ungroup,
            this.toolStripSeparator1,
            this._band,
            this._bdiiff,
            this._bint,
            this._bhalf,
            this.toolStripSeparator3,
            this._conv});
      this._model.Id = 0;
      this._model.Name = "_model";
      this._model.Size = new System.Drawing.Size(217, 24);
      this._model.Text = "Model";
      // 
      // _ggroup
      // 
      this._ggroup.Id = 2006;
      this._ggroup.Name = "_ggroup";
      this._ggroup.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
      this._ggroup.Size = new System.Drawing.Size(253, 26);
      this._ggroup.Text = "Group";
      // 
      // _ungroup
      // 
      this._ungroup.Id = 2007;
      this._ungroup.Name = "_ungroup";
      this._ungroup.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
      this._ungroup.Size = new System.Drawing.Size(253, 26);
      this._ungroup.Text = "Ungroup";
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(250, 6);
      // 
      // _band
      // 
      this._band.Id = 2050;
      this._band.Name = "_band";
      this._band.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U)));
      this._band.Size = new System.Drawing.Size(253, 26);
      this._band.Text = "Union A Ս B";
      // 
      // _bdiiff
      // 
      this._bdiiff.Id = 2051;
      this._bdiiff.Name = "_bdiiff";
      this._bdiiff.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D)));
      this._bdiiff.Size = new System.Drawing.Size(253, 26);
      this._bdiiff.Text = "Difference A / B";
      // 
      // _bint
      // 
      this._bint.Id = 2052;
      this._bint.Name = "_bint";
      this._bint.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.I)));
      this._bint.Size = new System.Drawing.Size(253, 26);
      this._bint.Text = "Intersection A Ո B";
      // 
      // _bhalf
      // 
      this._bhalf.Id = 2053;
      this._bhalf.Name = "_bhalf";
      this._bhalf.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H)));
      this._bhalf.Size = new System.Drawing.Size(253, 26);
      this._bhalf.Text = "Halfspace";
      // 
      // toolStripSeparator3
      // 
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new System.Drawing.Size(250, 6);
      // 
      // _conv
      // 
      this._conv.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem5});
      this._conv.Id = 0;
      this._conv.Name = "_conv";
      this._conv.Size = new System.Drawing.Size(253, 26);
      this._conv.Text = "Convert";
      // 
      // menuItem5
      // 
      this.menuItem5.Id = 2055;
      this.menuItem5.Name = "menuItem5";
      this.menuItem5.Size = new System.Drawing.Size(187, 26);
      this.menuItem5.Text = "No conversion";
      // 
      // _setings
      // 
      this._setings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._selstyle,
            this._drv,
            this._samples});
      this._setings.Id = 0;
      this._setings.Name = "_setings";
      this._setings.Size = new System.Drawing.Size(217, 24);
      this._setings.Text = "Settings";
      // 
      // _selstyle
      // 
      this._selstyle.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._selbox,
            this._selpiv,
            this._selwire,
            this.toolStripSeparator2,
            this._selnorm});
      this._selstyle.Id = 0;
      this._selstyle.Name = "_selstyle";
      this._selstyle.Size = new System.Drawing.Size(224, 26);
      this._selstyle.Text = "Select Style";
      // 
      // _selbox
      // 
      this._selbox.Id = 2100;
      this._selbox.Name = "_selbox";
      this._selbox.Size = new System.Drawing.Size(185, 26);
      this._selbox.Text = "Bounding Box";
      // 
      // _selpiv
      // 
      this._selpiv.Id = 2101;
      this._selpiv.Name = "_selpiv";
      this._selpiv.ShortcutKeyDisplayString = "";
      this._selpiv.Size = new System.Drawing.Size(185, 26);
      this._selpiv.Text = "Pivot";
      // 
      // _selwire
      // 
      this._selwire.Id = 2102;
      this._selwire.Name = "_selwire";
      this._selwire.ShortcutKeyDisplayString = "";
      this._selwire.Size = new System.Drawing.Size(185, 26);
      this._selwire.Text = "Wireframe";
      // 
      // _selnorm
      // 
      this._selnorm.Id = 2103;
      this._selnorm.Name = "_selnorm";
      this._selnorm.ShortcutKeyDisplayString = "";
      this._selnorm.Size = new System.Drawing.Size(185, 26);
      this._selnorm.Text = "Normals";
      // 
      // _drv
      // 
      this._drv.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem7});
      this._drv.Id = 0;
      this._drv.Name = "_drv";
      this._drv.Size = new System.Drawing.Size(224, 26);
      this._drv.Text = "Graphics Driver";
      // 
      // menuItem7
      // 
      this.menuItem7.Id = 3015;
      this.menuItem7.Name = "menuItem7";
      this.menuItem7.Size = new System.Drawing.Size(132, 26);
      this.menuItem7.Text = "Driver";
      // 
      // _samples
      // 
      this._samples.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem8});
      this._samples.Id = 0;
      this._samples.Name = "_samples";
      this._samples.Size = new System.Drawing.Size(224, 26);
      this._samples.Text = "Multi-Samples";
      // 
      // menuItem8
      // 
      this.menuItem8.Id = 3016;
      this.menuItem8.Name = "menuItem8";
      this.menuItem8.Size = new System.Drawing.Size(148, 26);
      this.menuItem8.Text = "Samples";
      // 
      // _3
      // 
      this._3.Name = "_3";
      this._3.Size = new System.Drawing.Size(214, 6);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(344, 17);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(142, 20);
      this.label2.TabIndex = 0;
      this.label2.Text = "(under construction)";
      // 
      // modelView
      // 
      this.modelView.AllowDrop = true;
      this.modelView.BackColor = System.Drawing.Color.White;
      this.modelView.ContextMenuStrip = this.contextMenuView;
      this.modelView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.modelView.IsModified = false;
      this.modelView.Location = new System.Drawing.Point(0, 0);
      this.modelView.Name = "modelView";
      this.modelView.Scene = null;
      this.modelView.Size = new System.Drawing.Size(675, 499);
      this.modelView.TabIndex = 1;
      // 
      // propsView
      // 
      this.propsView.ContextMenuStrip = this.contextMenuPropsView;
      this.propsView.Dock = System.Windows.Forms.DockStyle.Right;
      this.propsView.Location = new System.Drawing.Point(675, 0);
      this.propsView.Name = "propsView";
      this.propsView.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
      this.propsView.Size = new System.Drawing.Size(286, 499);
      this.propsView.TabIndex = 2;
      this.propsView.Target = this.modelView;
      this.propsView.Visible = false;
      // 
      // contextMenuPropsView
      // 
      this.contextMenuPropsView.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.contextMenuPropsView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            undo2_z,
            redo2_y,
            undo2_back,
            redo2_back});
      this.contextMenuPropsView.Name = "contextMenu4";
      this.contextMenuPropsView.Size = new System.Drawing.Size(187, 100);
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel1.Controls.Add(this.modelView);
      this.panel1.Controls.Add(this.propsView);
      this.panel1.Location = new System.Drawing.Point(11, 55);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(961, 499);
      this.panel1.TabIndex = 1;
      this.panel1.TabStop = true;
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(11, 569);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(142, 20);
      this.label3.TabIndex = 0;
      this.label3.Text = "(under construction)";
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(182, 6);
      // 
      // PolyhedronPage
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "PolyhedronPage";
      this.Size = new System.Drawing.Size(975, 598);
      this.contextMenuView.ResumeLayout(false);
      this.contextMenuPropsView.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Label label1;
    private ContextMenu contextMenuView;
    private Label label2;
    private DX11ModelCtrl modelView;
    private DX11PropsCtrl propsView;
    private ContextMenu contextMenuPropsView;
    private Panel panel1;
    private Label label3;
    private MenuItem _ggroup;
    private MenuItem _ungroup;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripSeparator _3;
    private MenuItem _setings;
    private MenuItem _selstyle;
    private MenuItem _selbox;
    private MenuItem _selpiv;
    private MenuItem _selwire;
    private MenuItem _selnorm;
    private MenuItem _drv;
    private MenuItem menuItem7;
    private MenuItem _samples;
    private MenuItem menuItem8;
    private MenuItem _model;
    private MenuItem _conv;
    private MenuItem menuItem5;
    private MenuItem _band;
    private MenuItem _bdiiff;
    private MenuItem _bint;
    private MenuItem _bhalf;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripSeparator toolStripSeparator2;
  }
}
