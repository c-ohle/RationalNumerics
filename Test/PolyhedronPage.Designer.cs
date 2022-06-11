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
      System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      Test.MenuItem _redo2;
      Test.MenuItem _undo2;
      Test.MenuItem _props;
      Test.MenuItem _undo1;
      Test.MenuItem _redo1;
      System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
      Test.MenuItem _cut;
      Test.MenuItem _copy;
      Test.MenuItem _paste;
      Test.MenuItem _del;
      this.label1 = new System.Windows.Forms.Label();
      this.dX11CtrlModel1 = new Test.DX11ModelCtrl();
      this.contextMenu1 = new Test.ContextMenu(this.components);
      this.label2 = new System.Windows.Forms.Label();
      toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      _redo2 = new Test.MenuItem();
      _undo2 = new Test.MenuItem();
      _props = new Test.MenuItem();
      _undo1 = new Test.MenuItem();
      _redo1 = new Test.MenuItem();
      toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      _cut = new Test.MenuItem();
      _copy = new Test.MenuItem();
      _paste = new Test.MenuItem();
      _del = new Test.MenuItem();
      this.contextMenu1.SuspendLayout();
      this.SuspendLayout();
      // 
      // toolStripSeparator1
      // 
      toolStripSeparator1.Name = "toolStripSeparator1";
      toolStripSeparator1.Size = new System.Drawing.Size(214, 6);
      // 
      // _redo2
      // 
      _redo2.Enabled = false;
      _redo2.Id = 2001;
      _redo2.Name = "_redo2";
      _redo2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Back)));
      _redo2.Size = new System.Drawing.Size(217, 24);
      _redo2.Text = "Redo";
      _redo2.Visible = false;
      // 
      // _undo2
      // 
      _undo2.Enabled = false;
      _undo2.Id = 2000;
      _undo2.Name = "_undo2";
      _undo2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Back)));
      _undo2.Size = new System.Drawing.Size(217, 24);
      _undo2.Text = "Undo";
      _undo2.Visible = false;
      // 
      // _props
      // 
      _props.Id = 2008;
      _props.Name = "_props";
      _props.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Return)));
      _props.Size = new System.Drawing.Size(217, 24);
      _props.Text = "Properties";
      // 
      // _undo1
      // 
      _undo1.Enabled = false;
      _undo1.Id = 2000;
      _undo1.Name = "_undo1";
      _undo1.ShortcutKeyDisplayString = "";
      _undo1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
      _undo1.Size = new System.Drawing.Size(217, 24);
      _undo1.Text = "Undo";
      // 
      // _redo1
      // 
      _redo1.Enabled = false;
      _redo1.Id = 2001;
      _redo1.Name = "_redo1";
      _redo1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
      _redo1.Size = new System.Drawing.Size(217, 24);
      _redo1.Text = "Redo";
      // 
      // toolStripSeparator2
      // 
      toolStripSeparator2.Name = "toolStripSeparator2";
      toolStripSeparator2.Size = new System.Drawing.Size(214, 6);
      // 
      // _cut
      // 
      _cut.Enabled = false;
      _cut.Id = 2002;
      _cut.Name = "_cut";
      _cut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
      _cut.Size = new System.Drawing.Size(217, 24);
      _cut.Text = "Cut";
      // 
      // _copy
      // 
      _copy.Enabled = false;
      _copy.Id = 2003;
      _copy.Name = "_copy";
      _copy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
      _copy.Size = new System.Drawing.Size(217, 24);
      _copy.Text = "Copy";
      // 
      // _paste
      // 
      _paste.Enabled = false;
      _paste.Id = 2004;
      _paste.Name = "_paste";
      _paste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
      _paste.Size = new System.Drawing.Size(217, 24);
      _paste.Text = "Paste";
      // 
      // _del
      // 
      _del.Enabled = false;
      _del.Id = 2005;
      _del.Name = "_del";
      _del.ShortcutKeys = System.Windows.Forms.Keys.Delete;
      _del.Size = new System.Drawing.Size(217, 24);
      _del.Text = "Delete";
      _del.Visible = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label1.Location = new System.Drawing.Point(11, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(327, 38);
      this.label1.TabIndex = 2;
      this.label1.Text = "BigRational Polyhedron";
      // 
      // dX11CtrlModel1
      // 
      this.dX11CtrlModel1.AllowDrop = true;
      this.dX11CtrlModel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dX11CtrlModel1.BackColor = System.Drawing.Color.White;
      this.dX11CtrlModel1.ContextMenuStrip = this.contextMenu1;
      this.dX11CtrlModel1.IsModified = false;
      this.dX11CtrlModel1.Location = new System.Drawing.Point(11, 44);
      this.dX11CtrlModel1.Name = "dX11CtrlModel1";
      this.dX11CtrlModel1.Scene = null;
      this.dX11CtrlModel1.Size = new System.Drawing.Size(772, 489);
      this.dX11CtrlModel1.TabIndex = 3;
      // 
      // contextMenu1
      // 
      this.contextMenu1.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            _undo1,
            _redo1,
            _undo2,
            _redo2,
            toolStripSeparator1,
            _cut,
            _copy,
            _paste,
            _del,
            toolStripSeparator2,
            _props});
      this.contextMenu1.Name = "contextMenu1";
      this.contextMenu1.Size = new System.Drawing.Size(218, 232);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(344, 17);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(142, 20);
      this.label2.TabIndex = 4;
      this.label2.Text = "(under construction)";
      // 
      // PolyhedronPage
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.label2);
      this.Controls.Add(this.dX11CtrlModel1);
      this.Controls.Add(this.label1);
      this.Name = "PolyhedronPage";
      this.Size = new System.Drawing.Size(975, 598);
      this.contextMenu1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Label label1;
    private DX11ModelCtrl dX11CtrlModel1;
    private ContextMenu contextMenu1;
    private Label label2;
  }
}
