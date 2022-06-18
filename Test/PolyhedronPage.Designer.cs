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
      Test.MenuItem _storyboard;
      Test.MenuItem _selall;
      System.Windows.Forms.Button btn_run;
      Test.MenuItem _copy;
      Test.MenuItem _pros;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PolyhedronPage));
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
      this._center = new Test.MenuItem();
      this._setings = new Test.MenuItem();
      this._selstyle = new Test.MenuItem();
      this._selbox = new Test.MenuItem();
      this._selpiv = new Test.MenuItem();
      this._selwire = new Test.MenuItem();
      this._selpoints = new Test.MenuItem();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this._selnorm = new Test.MenuItem();
      this._drv = new Test.MenuItem();
      this.menuItem7 = new Test.MenuItem();
      this._samples = new Test.MenuItem();
      this.menuItem8 = new Test.MenuItem();
      this._3 = new System.Windows.Forms.ToolStripSeparator();
      this._4 = new System.Windows.Forms.ToolStripSeparator();
      this.modelView = new Test.DX11ModelCtrl();
      this.propsView = new Test.DX11PropsCtrl();
      this.contextMenuPropsView = new Test.ContextMenu(this.components);
      this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panelStory = new System.Windows.Forms.Panel();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
      this.btn_back = new System.Windows.Forms.ToolStripButton();
      this.btn_back_ = new System.Windows.Forms.ToolStripButton();
      this.btn_record = new System.Windows.Forms.ToolStripButton();
      this.btn_play = new System.Windows.Forms.ToolStripButton();
      this.btn_stop = new System.Windows.Forms.ToolStripButton();
      this.btn_forw_ = new System.Windows.Forms.ToolStripButton();
      this.btn_forw = new System.Windows.Forms.ToolStripButton();
      this.label3 = new System.Windows.Forms.Label();
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
      _storyboard = new Test.MenuItem();
      _selall = new Test.MenuItem();
      btn_run = new System.Windows.Forms.Button();
      _copy = new Test.MenuItem();
      _pros = new Test.MenuItem();
      this.contextMenuView.SuspendLayout();
      this.contextMenuPropsView.SuspendLayout();
      this.panel1.SuspendLayout();
      this.panelStory.SuspendLayout();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // _1
      // 
      _1.Name = "_1";
      _1.Size = new System.Drawing.Size(228, 6);
      // 
      // redo_back
      // 
      redo_back.Id = 2001;
      redo_back.Name = "redo_back";
      redo_back.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Back)));
      redo_back.Size = new System.Drawing.Size(231, 24);
      redo_back.Text = "Redo";
      redo_back.Visible = false;
      // 
      // undo_back
      // 
      undo_back.Id = 2000;
      undo_back.Name = "undo_back";
      undo_back.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Back)));
      undo_back.Size = new System.Drawing.Size(231, 24);
      undo_back.Text = "Undo";
      undo_back.Visible = false;
      // 
      // props
      // 
      props.Id = 2008;
      props.Name = "props";
      props.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Return)));
      props.Size = new System.Drawing.Size(231, 24);
      props.Text = "Properties";
      // 
      // undo_z
      // 
      undo_z.Id = 2000;
      undo_z.Name = "undo_z";
      undo_z.ShortcutKeyDisplayString = "";
      undo_z.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
      undo_z.Size = new System.Drawing.Size(231, 24);
      undo_z.Text = "Undo";
      // 
      // redo_y
      // 
      redo_y.Id = 2001;
      redo_y.Name = "redo_y";
      redo_y.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
      redo_y.Size = new System.Drawing.Size(231, 24);
      redo_y.Text = "Redo";
      // 
      // _2
      // 
      _2.Name = "_2";
      _2.Size = new System.Drawing.Size(228, 6);
      // 
      // cut
      // 
      cut.Id = 2002;
      cut.Name = "cut";
      cut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
      cut.Size = new System.Drawing.Size(231, 24);
      cut.Text = "Cut";
      // 
      // copy
      // 
      copy.Id = 2003;
      copy.Name = "copy";
      copy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
      copy.Size = new System.Drawing.Size(231, 24);
      copy.Text = "Copy";
      // 
      // paste
      // 
      paste.Id = 2004;
      paste.Name = "paste";
      paste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
      paste.Size = new System.Drawing.Size(231, 24);
      paste.Text = "Paste";
      // 
      // del
      // 
      del.Id = 2005;
      del.Name = "del";
      del.ShortcutKeys = System.Windows.Forms.Keys.Delete;
      del.Size = new System.Drawing.Size(231, 24);
      del.Text = "Delete";
      del.Visible = false;
      // 
      // undo2_z
      // 
      undo2_z.Id = 2000;
      undo2_z.Name = "undo2_z";
      undo2_z.ShortcutKeyDisplayString = "";
      undo2_z.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
      undo2_z.ShowShortcutKeys = false;
      undo2_z.Size = new System.Drawing.Size(136, 24);
      undo2_z.Text = "Undo";
      // 
      // redo2_y
      // 
      redo2_y.Id = 2001;
      redo2_y.Name = "redo2_y";
      redo2_y.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
      redo2_y.ShowShortcutKeys = false;
      redo2_y.Size = new System.Drawing.Size(136, 24);
      redo2_y.Text = "Redo";
      // 
      // undo2_back
      // 
      undo2_back.Id = 2000;
      undo2_back.Name = "undo2_back";
      undo2_back.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Back)));
      undo2_back.ShowShortcutKeys = false;
      undo2_back.Size = new System.Drawing.Size(136, 24);
      undo2_back.Text = "Undo";
      undo2_back.Visible = false;
      // 
      // redo2_back
      // 
      redo2_back.Id = 2001;
      redo2_back.Name = "redo2_back";
      redo2_back.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Back)));
      redo2_back.ShowShortcutKeys = false;
      redo2_back.Size = new System.Drawing.Size(136, 24);
      redo2_back.Text = "Redo";
      redo2_back.Visible = false;
      // 
      // _storyboard
      // 
      _storyboard.Id = 2010;
      _storyboard.Name = "_storyboard";
      _storyboard.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.S)));
      _storyboard.Size = new System.Drawing.Size(231, 24);
      _storyboard.Text = "Storyboard";
      // 
      // _selall
      // 
      _selall.Id = 2057;
      _selall.Name = "_selall";
      _selall.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
      _selall.Size = new System.Drawing.Size(231, 24);
      _selall.Text = "Select All";
      // 
      // btn_run
      // 
      btn_run.Location = new System.Drawing.Point(344, 12);
      btn_run.Name = "btn_run";
      btn_run.Size = new System.Drawing.Size(94, 29);
      btn_run.TabIndex = 2;
      btn_run.Text = "Run Test";
      btn_run.UseVisualStyleBackColor = true;
      btn_run.Click += new System.EventHandler(this.btn_run_Click);
      // 
      // _copy
      // 
      _copy.Id = 2003;
      _copy.Name = "_copy";
      _copy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
      _copy.ShowShortcutKeys = false;
      _copy.Size = new System.Drawing.Size(136, 24);
      _copy.Text = "Copy";
      // 
      // _pros
      // 
      _pros.Id = 2008;
      _pros.Name = "_pros";
      _pros.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Return)));
      _pros.ShowShortcutKeys = false;
      _pros.Size = new System.Drawing.Size(136, 24);
      _pros.Text = "Properties";
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
            _selall,
            _2,
            this._model,
            this._setings,
            this._3,
            _storyboard,
            this._4,
            props});
      this.contextMenuView.Name = "contextMenu1";
      this.contextMenuView.Size = new System.Drawing.Size(232, 340);
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
            this._center});
      this._model.Id = 0;
      this._model.Name = "_model";
      this._model.Size = new System.Drawing.Size(231, 24);
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
      // _center
      // 
      this._center.Id = 2056;
      this._center.Name = "_center";
      this._center.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
      this._center.Size = new System.Drawing.Size(253, 26);
      this._center.Text = "Center";
      // 
      // _setings
      // 
      this._setings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._selstyle,
            this._drv,
            this._samples});
      this._setings.Id = 0;
      this._setings.Name = "_setings";
      this._setings.Size = new System.Drawing.Size(231, 24);
      this._setings.Text = "Settings";
      // 
      // _selstyle
      // 
      this._selstyle.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._selbox,
            this._selpiv,
            this._selwire,
            this._selpoints,
            this.toolStripSeparator2,
            this._selnorm});
      this._selstyle.Id = 0;
      this._selstyle.Name = "_selstyle";
      this._selstyle.Size = new System.Drawing.Size(193, 26);
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
      // _selpoints
      // 
      this._selpoints.Id = 2104;
      this._selpoints.Name = "_selpoints";
      this._selpoints.Size = new System.Drawing.Size(185, 26);
      this._selpoints.Text = "Points";
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(182, 6);
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
      this._drv.Size = new System.Drawing.Size(193, 26);
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
      this._samples.Size = new System.Drawing.Size(193, 26);
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
      this._3.Size = new System.Drawing.Size(228, 6);
      // 
      // _4
      // 
      this._4.Name = "_4";
      this._4.Size = new System.Drawing.Size(228, 6);
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
      this.modelView.Size = new System.Drawing.Size(670, 380);
      this.modelView.TabIndex = 1;
      // 
      // propsView
      // 
      this.propsView.ContextMenuStrip = this.contextMenuPropsView;
      this.propsView.Dock = System.Windows.Forms.DockStyle.Right;
      this.propsView.Location = new System.Drawing.Point(670, 0);
      this.propsView.Name = "propsView";
      this.propsView.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
      this.propsView.Size = new System.Drawing.Size(286, 511);
      this.propsView.TabIndex = 2;
      this.propsView.Target = this.modelView;
      this.propsView.Visible = false;
      this.propsView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelStory_MouseDown);
      this.propsView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelStory_MouseMove);
      this.propsView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelStory_MouseUp);
      // 
      // contextMenuPropsView
      // 
      this.contextMenuPropsView.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.contextMenuPropsView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            undo2_z,
            redo2_y,
            undo2_back,
            redo2_back,
            this.toolStripSeparator6,
            _copy,
            _pros});
      this.contextMenuPropsView.Name = "contextMenu4";
      this.contextMenuPropsView.Size = new System.Drawing.Size(137, 154);
      // 
      // toolStripSeparator6
      // 
      this.toolStripSeparator6.Name = "toolStripSeparator6";
      this.toolStripSeparator6.Size = new System.Drawing.Size(133, 6);
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel1.Controls.Add(this.modelView);
      this.panel1.Controls.Add(this.panelStory);
      this.panel1.Controls.Add(this.propsView);
      this.panel1.Location = new System.Drawing.Point(16, 47);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(956, 511);
      this.panel1.TabIndex = 1;
      this.panel1.TabStop = true;
      // 
      // panelStory
      // 
      this.panelStory.Controls.Add(this.toolStrip1);
      this.panelStory.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panelStory.Location = new System.Drawing.Point(0, 380);
      this.panelStory.Name = "panelStory";
      this.panelStory.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
      this.panelStory.Size = new System.Drawing.Size(670, 131);
      this.panelStory.TabIndex = 2;
      this.panelStory.Visible = false;
      this.panelStory.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelStory_MouseDown);
      this.panelStory.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelStory_MouseMove);
      this.panelStory.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelStory_MouseUp);
      // 
      // toolStrip1
      // 
      this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.btn_back,
            this.btn_back_,
            this.btn_record,
            this.btn_play,
            this.btn_stop,
            this.btn_forw_,
            this.btn_forw});
      this.toolStrip1.Location = new System.Drawing.Point(0, 6);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
      this.toolStrip1.ShowItemToolTips = false;
      this.toolStrip1.Size = new System.Drawing.Size(670, 27);
      this.toolStrip1.TabIndex = 0;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // toolStripLabel1
      // 
      this.toolStripLabel1.Name = "toolStripLabel1";
      this.toolStripLabel1.Size = new System.Drawing.Size(83, 24);
      this.toolStripLabel1.Text = "Storyboard";
      // 
      // btn_back
      // 
      this.btn_back.AccessibleRole = System.Windows.Forms.AccessibleRole.RadioButton;
      this.btn_back.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_back.Enabled = false;
      this.btn_back.Image = ((System.Drawing.Image)(resources.GetObject("btn_back.Image")));
      this.btn_back.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_back.Name = "btn_back";
      this.btn_back.Size = new System.Drawing.Size(29, 24);
      this.btn_back.Text = "";
      this.btn_back.ToolTipText = "Start";
      this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
      // 
      // btn_back_
      // 
      this.btn_back_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_back_.Enabled = false;
      this.btn_back_.Font = new System.Drawing.Font("Segoe UI Symbol", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.btn_back_.Image = ((System.Drawing.Image)(resources.GetObject("btn_back_.Image")));
      this.btn_back_.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_back_.Name = "btn_back_";
      this.btn_back_.Size = new System.Drawing.Size(29, 24);
      this.btn_back_.Text = "⏪";
      this.btn_back_.Click += new System.EventHandler(this.btn_back__Click);
      // 
      // btn_record
      // 
      this.btn_record.AccessibleRole = System.Windows.Forms.AccessibleRole.RadioButton;
      this.btn_record.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_record.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.btn_record.ForeColor = System.Drawing.Color.Red;
      this.btn_record.Image = ((System.Drawing.Image)(resources.GetObject("btn_record.Image")));
      this.btn_record.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_record.Name = "btn_record";
      this.btn_record.Size = new System.Drawing.Size(29, 24);
      this.btn_record.Text = "";
      this.btn_record.ToolTipText = "Record";
      this.btn_record.Click += new System.EventHandler(this.btn_record_Click);
      // 
      // btn_play
      // 
      this.btn_play.AccessibleRole = System.Windows.Forms.AccessibleRole.RadioButton;
      this.btn_play.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_play.Enabled = false;
      this.btn_play.Image = ((System.Drawing.Image)(resources.GetObject("btn_play.Image")));
      this.btn_play.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_play.Name = "btn_play";
      this.btn_play.Size = new System.Drawing.Size(29, 24);
      this.btn_play.Text = "";
      this.btn_play.ToolTipText = "Play";
      this.btn_play.Click += new System.EventHandler(this.btn_play_Click);
      // 
      // btn_stop
      // 
      this.btn_stop.AccessibleRole = System.Windows.Forms.AccessibleRole.RadioButton;
      this.btn_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_stop.Enabled = false;
      this.btn_stop.Image = ((System.Drawing.Image)(resources.GetObject("btn_stop.Image")));
      this.btn_stop.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_stop.Name = "btn_stop";
      this.btn_stop.Size = new System.Drawing.Size(29, 24);
      this.btn_stop.Text = "";
      this.btn_stop.ToolTipText = "Stop";
      this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
      // 
      // btn_forw_
      // 
      this.btn_forw_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_forw_.Enabled = false;
      this.btn_forw_.Font = new System.Drawing.Font("Segoe UI Symbol", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.btn_forw_.Image = ((System.Drawing.Image)(resources.GetObject("btn_forw_.Image")));
      this.btn_forw_.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_forw_.Name = "btn_forw_";
      this.btn_forw_.Size = new System.Drawing.Size(29, 24);
      this.btn_forw_.Text = "⏩";
      this.btn_forw_.Click += new System.EventHandler(this.btn_forw__Click);
      // 
      // btn_forw
      // 
      this.btn_forw.AccessibleRole = System.Windows.Forms.AccessibleRole.RadioButton;
      this.btn_forw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_forw.Enabled = false;
      this.btn_forw.Image = ((System.Drawing.Image)(resources.GetObject("btn_forw.Image")));
      this.btn_forw.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_forw.Name = "btn_forw";
      this.btn_forw.Size = new System.Drawing.Size(29, 24);
      this.btn_forw.Text = "";
      this.btn_forw.ToolTipText = "End";
      this.btn_forw.Click += new System.EventHandler(this.btn_forw_Click);
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
      // PolyhedronPage
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(btn_run);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label1);
      this.Name = "PolyhedronPage";
      this.Size = new System.Drawing.Size(975, 598);
      this.contextMenuView.ResumeLayout(false);
      this.contextMenuPropsView.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panelStory.ResumeLayout(false);
      this.panelStory.PerformLayout();
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Label label1;
    private ContextMenu contextMenuView;
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
    private MenuItem _band;
    private MenuItem _bdiiff;
    private MenuItem _bint;
    private MenuItem _bhalf;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripSeparator toolStripSeparator2;
    private MenuItem _center;
    private ToolStripSeparator _4;
    private Panel panelStory;
    private ToolStrip toolStrip1;
    private ToolStripLabel toolStripLabel1;
    private ToolStripButton btn_stop;
    private ToolStripButton btn_play;
    private ToolStripButton btn_back;
    private ToolStripButton btn_forw;
    private ToolStripButton btn_record;
    private MenuItem _selpoints;
    private ToolStripSeparator toolStripSeparator6;
    private ToolStripButton btn_back_;
    private ToolStripButton btn_forw_;
  }
}
