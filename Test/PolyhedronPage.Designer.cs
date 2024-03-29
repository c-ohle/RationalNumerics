﻿namespace Test
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
      Test.MenuItem _file;
      Test.MenuItem _file_new;
      Test.MenuItem _file_open;
      Test.MenuItem _file_save;
      Test.MenuItem _file_saveas;
      System.Windows.Forms.ToolStripLabel toolStripLabel1;
      Test.MenuItem _group1;
      Test.MenuItem _ungroup2;
      System.Windows.Forms.ToolStripSeparator _5;
      Test.MenuItem _groupcsg;
      Test.MenuItem _center2;
      System.Windows.Forms.Button button1;
      System.Windows.Forms.Button button2;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PolyhedronPage));
      this.label1 = new System.Windows.Forms.Label();
      this.contextMenuView = new Test.ContextMenu(this.components);
      this._3 = new System.Windows.Forms.ToolStripSeparator();
      this._4 = new System.Windows.Forms.ToolStripSeparator();
      this.modelView = new Test.DX11ModelCtrl();
      this.propsView = new Test.DX11PropsCtrl();
      this.contextMenuPropsView = new Test.ContextMenu(this.components);
      this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panelStory = new System.Windows.Forms.Panel();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.btn_back = new System.Windows.Forms.ToolStripButton();
      this.btn_back_ = new System.Windows.Forms.ToolStripButton();
      this.btn_play = new System.Windows.Forms.ToolStripButton();
      this.btn_forw_ = new System.Windows.Forms.ToolStripButton();
      this.btn_forw = new System.Windows.Forms.ToolStripButton();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.btn_record = new System.Windows.Forms.ToolStripButton();
      this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
      this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
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
      _file = new Test.MenuItem();
      _file_new = new Test.MenuItem();
      _file_open = new Test.MenuItem();
      _file_save = new Test.MenuItem();
      _file_saveas = new Test.MenuItem();
      toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
      _group1 = new Test.MenuItem();
      _ungroup2 = new Test.MenuItem();
      _5 = new System.Windows.Forms.ToolStripSeparator();
      _groupcsg = new Test.MenuItem();
      _center2 = new Test.MenuItem();
      button1 = new System.Windows.Forms.Button();
      button2 = new System.Windows.Forms.Button();
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
      btn_run.Text = "CSG Tests";
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
      // _file
      // 
      _file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            _file_new,
            _file_open,
            _file_save,
            _file_saveas});
      _file.Id = 0;
      _file.Name = "_file";
      _file.Size = new System.Drawing.Size(231, 24);
      _file.Text = "File";
      // 
      // _file_new
      // 
      _file_new.Id = 1000;
      _file_new.Name = "_file_new";
      _file_new.Size = new System.Drawing.Size(190, 26);
      _file_new.Text = "New";
      // 
      // _file_open
      // 
      _file_open.Id = 1001;
      _file_open.Name = "_file_open";
      _file_open.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      _file_open.Size = new System.Drawing.Size(190, 26);
      _file_open.Text = "Open...";
      // 
      // _file_save
      // 
      _file_save.Id = 1002;
      _file_save.Name = "_file_save";
      _file_save.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
      _file_save.Size = new System.Drawing.Size(190, 26);
      _file_save.Text = "Save";
      // 
      // _file_saveas
      // 
      _file_saveas.Id = 1003;
      _file_saveas.Name = "_file_saveas";
      _file_saveas.Size = new System.Drawing.Size(190, 26);
      _file_saveas.Text = "Save as...";
      // 
      // toolStripLabel1
      // 
      toolStripLabel1.Name = "toolStripLabel1";
      toolStripLabel1.Size = new System.Drawing.Size(100, 24);
      toolStripLabel1.Text = " Storyboard";
      // 
      // _group1
      // 
      _group1.Id = 2006;
      _group1.Name = "_group1";
      _group1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
      _group1.Size = new System.Drawing.Size(231, 24);
      _group1.Text = "Group";
      // 
      // _ungroup2
      // 
      _ungroup2.Id = 2007;
      _ungroup2.Name = "_ungroup2";
      _ungroup2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
      _ungroup2.Size = new System.Drawing.Size(231, 24);
      _ungroup2.Text = "Ungroup";
      // 
      // _5
      // 
      _5.Name = "_5";
      _5.Size = new System.Drawing.Size(228, 6);
      // 
      // _groupcsg
      // 
      _groupcsg.Id = 2009;
      _groupcsg.Name = "_groupcsg";
      _groupcsg.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.G)));
      _groupcsg.Size = new System.Drawing.Size(231, 24);
      _groupcsg.Text = "Group CSG";
      // 
      // _center2
      // 
      _center2.Id = 2056;
      _center2.Name = "_center2";
      _center2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.C)));
      _center2.Size = new System.Drawing.Size(231, 24);
      _center2.Text = "Center";
      // 
      // button1
      // 
      button1.Enabled = false;
      button1.Location = new System.Drawing.Point(444, 12);
      button1.Name = "button1";
      button1.Size = new System.Drawing.Size(94, 29);
      button1.TabIndex = 2;
      button1.Text = "Skeleton";
      button1.UseVisualStyleBackColor = true;
      button1.Click += new System.EventHandler(this.btn_run_Click);
      // 
      // button2
      // 
      button2.Enabled = false;
      button2.Location = new System.Drawing.Point(544, 12);
      button2.Name = "button2";
      button2.Size = new System.Drawing.Size(94, 29);
      button2.TabIndex = 2;
      button2.Text = "BSP Tests";
      button2.UseVisualStyleBackColor = true;
      button2.Click += new System.EventHandler(this.btn_run_Click);
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
            _group1,
            _groupcsg,
            _ungroup2,
            _center2,
            _5,
            _file,
            this._3,
            _storyboard,
            this._4,
            props});
      this.contextMenuView.Name = "contextMenu1";
      this.contextMenuView.Size = new System.Drawing.Size(232, 418);
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
      this.propsView.Visible = false;
      this.propsView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel_MouseDown);
      this.propsView.MouseLeave += new System.EventHandler(this.panel_MouseLeave);
      this.propsView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_MouseMove);
      this.propsView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_MouseUp);
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
      this.panelStory.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel_MouseDown);
      this.panelStory.MouseLeave += new System.EventHandler(this.panel_MouseLeave);
      this.panelStory.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_MouseMove);
      this.panelStory.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_MouseUp);
      // 
      // toolStrip1
      // 
      this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripLabel1,
            this.btn_back,
            this.btn_back_,
            this.btn_play,
            this.btn_forw_,
            this.btn_forw,
            this.toolStripSeparator2,
            this.btn_record,
            this.toolStripButton1,
            this.toolStripButton2});
      this.toolStrip1.Location = new System.Drawing.Point(0, 6);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
      this.toolStrip1.Size = new System.Drawing.Size(670, 27);
      this.toolStrip1.TabIndex = 0;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // btn_back
      // 
      this.btn_back.AccessibleRole = System.Windows.Forms.AccessibleRole.RadioButton;
      this.btn_back.AutoToolTip = false;
      this.btn_back.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_back.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
      this.btn_back.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_back.Name = "btn_back";
      this.btn_back.Size = new System.Drawing.Size(29, 24);
      this.btn_back.Text = "";
      this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
      // 
      // btn_back_
      // 
      this.btn_back_.AutoToolTip = false;
      this.btn_back_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_back_.Font = new System.Drawing.Font("Segoe UI Symbol", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.btn_back_.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_back_.Name = "btn_back_";
      this.btn_back_.Size = new System.Drawing.Size(29, 24);
      this.btn_back_.Text = "⏪";
      this.btn_back_.Click += new System.EventHandler(this.btn_back__Click);
      // 
      // btn_play
      // 
      this.btn_play.AccessibleRole = System.Windows.Forms.AccessibleRole.RadioButton;
      this.btn_play.AutoToolTip = false;
      this.btn_play.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_play.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_play.Name = "btn_play";
      this.btn_play.Size = new System.Drawing.Size(29, 24);
      this.btn_play.Text = "";
      this.btn_play.Click += new System.EventHandler(this.btn_play_Click);
      // 
      // btn_forw_
      // 
      this.btn_forw_.AutoToolTip = false;
      this.btn_forw_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_forw_.Font = new System.Drawing.Font("Segoe UI Symbol", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.btn_forw_.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_forw_.Name = "btn_forw_";
      this.btn_forw_.Size = new System.Drawing.Size(29, 24);
      this.btn_forw_.Text = "⏩";
      this.btn_forw_.Click += new System.EventHandler(this.btn_forw__Click);
      // 
      // btn_forw
      // 
      this.btn_forw.AccessibleRole = System.Windows.Forms.AccessibleRole.RadioButton;
      this.btn_forw.AutoToolTip = false;
      this.btn_forw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_forw.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_forw.Name = "btn_forw";
      this.btn_forw.Size = new System.Drawing.Size(29, 24);
      this.btn_forw.Text = "";
      this.btn_forw.Click += new System.EventHandler(this.btn_forw_Click);
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
      // 
      // btn_record
      // 
      this.btn_record.AccessibleRole = System.Windows.Forms.AccessibleRole.RadioButton;
      this.btn_record.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.btn_record.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.btn_record.ForeColor = System.Drawing.Color.Red;
      this.btn_record.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btn_record.Name = "btn_record";
      this.btn_record.Size = new System.Drawing.Size(29, 24);
      this.btn_record.Text = "⦁";
      this.btn_record.ToolTipText = "Record";
      this.btn_record.Click += new System.EventHandler(this.btn_record_Click);
      // 
      // toolStripButton1
      // 
      this.toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.toolStripButton1.AutoToolTip = false;
      this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButton1.Name = "toolStripButton1";
      this.toolStripButton1.Size = new System.Drawing.Size(29, 24);
      this.toolStripButton1.Text = "⨯";
      this.toolStripButton1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      this.toolStripButton1.Click += new System.EventHandler(this.btn_close_Click);
      // 
      // toolStripButton2
      // 
      this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripButton2.Font = new System.Drawing.Font("Segoe UI Symbol", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
      this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButton2.Name = "toolStripButton2";
      this.toolStripButton2.Size = new System.Drawing.Size(29, 24);
      this.toolStripButton2.Text = "⚒";
      this.toolStripButton2.Visible = false;
      // 
      // PolyhedronPage
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(button2);
      this.Controls.Add(button1);
      this.Controls.Add(btn_run);
      this.Controls.Add(this.panel1);
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
    private ToolStripSeparator _3;
    private ToolStripSeparator _4;
    private Panel panelStory;
    private ToolStrip toolStrip1;
    private ToolStripButton btn_play;
    private ToolStripButton btn_back;
    private ToolStripButton btn_forw;
    private ToolStripButton btn_record;
    private ToolStripSeparator toolStripSeparator6;
    private ToolStripButton btn_back_;
    private ToolStripButton btn_forw_;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripButton toolStripButton1;
    private ToolStripButton toolStripButton2;
  }
}
