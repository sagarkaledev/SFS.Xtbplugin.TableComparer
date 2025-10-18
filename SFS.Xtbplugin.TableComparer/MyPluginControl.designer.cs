namespace SFS.Xtbplugin.TableComparer
{
    partial class MyPluginControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbLabelTable1 = new System.Windows.Forms.ToolStripLabel();
            this.tsbTable1 = new System.Windows.Forms.ToolStripComboBox();
            this.tsbLabelTable2 = new System.Windows.Forms.ToolStripLabel();
            this.tsbTable2 = new System.Windows.Forms.ToolStripComboBox();
            this.chkHideOOTB = new System.Windows.Forms.CheckBox();
            this.tsbHideOOTB = new System.Windows.Forms.ToolStripControlHost(this.chkHideOOTB);
            this.tsbCompare = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbCloneSelectedColumn = new System.Windows.Forms.ToolStripButton();
            this.dgvComparison = new System.Windows.Forms.DataGridView();
            this.toolStripMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComparison)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tssSeparator1,
            this.tsbLabelTable1,
            this.tsbTable1,
            this.tsbLabelTable2,
            this.tsbTable2,
            this.tsbHideOOTB,
            this.tsbCompare,
            this.tssSeparator2,
            this.tsbCloneSelectedColumn});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1015, 31);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tsbClose
            // 
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbClose.Image = global::SFS.Xtbplugin.TableComparer.Properties.Resources.circle_xmark_solid_full;
            this.tsbClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(28, 28);
            this.tsbClose.Text = "Close";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbLabelTable1
            // 
            this.tsbLabelTable1.Name = "tsbLabelTable1";
            this.tsbLabelTable1.Size = new System.Drawing.Size(47, 28);
            this.tsbLabelTable1.Text = "Table 1:";
            // 
            // tsbTable1
            // 
            this.tsbTable1.Name = "tsbTable1";
            this.tsbTable1.Size = new System.Drawing.Size(201, 31);
            // 
            // tsbLabelTable2
            // 
            this.tsbLabelTable2.Name = "tsbLabelTable2";
            this.tsbLabelTable2.Size = new System.Drawing.Size(47, 28);
            this.tsbLabelTable2.Text = "Table 2:";
            // 
            // tsbTable2
            // 
            this.tsbTable2.Name = "tsbTable2";
            this.tsbTable2.Size = new System.Drawing.Size(201, 31);
            // 
            // chkHideOOTB
            // 
            this.chkHideOOTB.AutoSize = true;
            this.chkHideOOTB.Name = "chkHideOOTB";
            this.chkHideOOTB.Size = new System.Drawing.Size(118, 24);
            this.chkHideOOTB.Text = "Hide OOTB Fields";
            this.chkHideOOTB.UseVisualStyleBackColor = true;
            this.chkHideOOTB.CheckedChanged += new System.EventHandler(this.chkHideOOTB_CheckedChanged);
            // 
            // tsbHideOOTB
            // 
            this.tsbHideOOTB.Name = "tsbHideOOTB";
            this.tsbHideOOTB.Size = new System.Drawing.Size(118, 28);
            // 
            // tsbCompare
            // 
            this.tsbCompare.Image = global::SFS.Xtbplugin.TableComparer.Properties.Resources.circle_check_solid;
            this.tsbCompare.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCompare.Name = "tsbCompare";
            this.tsbCompare.Size = new System.Drawing.Size(84, 28);
            this.tsbCompare.Text = "Compare";
            this.tsbCompare.Click += new System.EventHandler(this.tsbCompare_Click);
            // 
            // tssSeparator2
            // 
            this.tssSeparator2.Name = "tssSeparator2";
            this.tssSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbCloneSelectedColumn
            // 
            this.tsbCloneSelectedColumn.Enabled = false;
            this.tsbCloneSelectedColumn.Image = global::SFS.Xtbplugin.TableComparer.Properties.Resources.circle_right_solid;
            this.tsbCloneSelectedColumn.Name = "tsbCloneSelectedColumn";
            this.tsbCloneSelectedColumn.Size = new System.Drawing.Size(158, 28);
            this.tsbCloneSelectedColumn.Text = "Clone selected Column";
            this.tsbCloneSelectedColumn.Click += new System.EventHandler(this.tsbCloneSelectedColumn_Click);
            // 
            // dgvComparison
            // 
            this.dgvComparison.AllowUserToAddRows = false;
            this.dgvComparison.AllowUserToDeleteRows = false;
            this.dgvComparison.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvComparison.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvComparison.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvComparison.Location = new System.Drawing.Point(0, 31);
            this.dgvComparison.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvComparison.Name = "dgvComparison";
            this.dgvComparison.ReadOnly = true;
            this.dgvComparison.Size = new System.Drawing.Size(1015, 489);
            this.dgvComparison.TabIndex = 15;
            // 
            // MyPluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvComparison);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "MyPluginControl";
            this.Size = new System.Drawing.Size(1015, 520);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComparison)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ToolStripLabel tsbLabelTable1;
        private System.Windows.Forms.ToolStripComboBox tsbTable1;
        private System.Windows.Forms.ToolStripLabel tsbLabelTable2;
        private System.Windows.Forms.ToolStripComboBox tsbTable2;
        private System.Windows.Forms.ToolStripButton tsbCompare;
        private System.Windows.Forms.CheckBox chkHideOOTB;
        private System.Windows.Forms.ToolStripControlHost tsbHideOOTB;
        private System.Windows.Forms.DataGridView dgvComparison;
        private System.Windows.Forms.ToolStripSeparator tssSeparator2;
        private System.Windows.Forms.ToolStripButton tsbCloneSelectedColumn;
    }
}
