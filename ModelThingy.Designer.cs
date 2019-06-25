namespace LM2L
{
    partial class ModelThingy
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.glViewport = new OpenTK.GLControl();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.meshSetList = new System.Windows.Forms.ListBox();
            this.modelList = new System.Windows.Forms.ListBox();
            this.texturePathBox = new System.Windows.Forms.TextBox();
            this.modelInfo = new System.Windows.Forms.Label();
            this.checkBoxEnableWireframe = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableTextures = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.texturePathBox);
            this.splitContainer1.Panel2.Controls.Add(this.modelInfo);
            this.splitContainer1.Panel2.Controls.Add(this.checkBoxEnableWireframe);
            this.splitContainer1.Panel2.Controls.Add(this.checkBoxEnableTextures);
            this.splitContainer1.Size = new System.Drawing.Size(1053, 628);
            this.splitContainer1.SplitterDistance = 788;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.glViewport);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(788, 628);
            this.splitContainer2.SplitterDistance = 418;
            this.splitContainer2.TabIndex = 0;
            // 
            // glViewport
            // 
            this.glViewport.BackColor = System.Drawing.Color.Transparent;
            this.glViewport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glViewport.ForeColor = System.Drawing.Color.Transparent;
            this.glViewport.Location = new System.Drawing.Point(0, 0);
            this.glViewport.Name = "glViewport";
            this.glViewport.Size = new System.Drawing.Size(788, 418);
            this.glViewport.TabIndex = 0;
            this.glViewport.VSync = true;
            this.glViewport.Load += new System.EventHandler(this.GlViewport_Load);
            this.glViewport.Paint += new System.Windows.Forms.PaintEventHandler(this.GlViewport_Paint);
            this.glViewport.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GlViewport_MouseDown);
            this.glViewport.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GlViewport_MouseMove);
            this.glViewport.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.GlViewport_MouseWheel);
            this.glViewport.Resize += new System.EventHandler(this.GlViewport_Resize);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.meshSetList);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.modelList);
            this.splitContainer3.Size = new System.Drawing.Size(788, 206);
            this.splitContainer3.SplitterDistance = 388;
            this.splitContainer3.TabIndex = 0;
            // 
            // meshSetList
            // 
            this.meshSetList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.meshSetList.Enabled = false;
            this.meshSetList.FormattingEnabled = true;
            this.meshSetList.Location = new System.Drawing.Point(0, 0);
            this.meshSetList.Name = "meshSetList";
            this.meshSetList.Size = new System.Drawing.Size(388, 206);
            this.meshSetList.TabIndex = 0;
            // 
            // modelList
            // 
            this.modelList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelList.Enabled = false;
            this.modelList.FormattingEnabled = true;
            this.modelList.Location = new System.Drawing.Point(0, 0);
            this.modelList.Name = "modelList";
            this.modelList.Size = new System.Drawing.Size(396, 206);
            this.modelList.TabIndex = 0;
            // 
            // texturePathBox
            // 
            this.texturePathBox.Location = new System.Drawing.Point(9, 35);
            this.texturePathBox.Name = "texturePathBox";
            this.texturePathBox.Size = new System.Drawing.Size(240, 20);
            this.texturePathBox.TabIndex = 3;
            // 
            // modelInfo
            // 
            this.modelInfo.AutoSize = true;
            this.modelInfo.Location = new System.Drawing.Point(6, 81);
            this.modelInfo.Name = "modelInfo";
            this.modelInfo.Size = new System.Drawing.Size(96, 91);
            this.modelInfo.TabIndex = 2;
            this.modelInfo.Text = "HashID: none\r\nVertices: 0\r\nFaces: 0\r\nVertex fmt: none\r\nIndex fmt: none\r\nVertex o" +
    "ffset: none\r\nIndex offset: none";
            // 
            // checkBoxEnableWireframe
            // 
            this.checkBoxEnableWireframe.AutoSize = true;
            this.checkBoxEnableWireframe.Location = new System.Drawing.Point(9, 61);
            this.checkBoxEnableWireframe.Name = "checkBoxEnableWireframe";
            this.checkBoxEnableWireframe.Size = new System.Drawing.Size(110, 17);
            this.checkBoxEnableWireframe.TabIndex = 1;
            this.checkBoxEnableWireframe.Text = "Enable Wireframe";
            this.checkBoxEnableWireframe.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnableTextures
            // 
            this.checkBoxEnableTextures.AutoSize = true;
            this.checkBoxEnableTextures.Location = new System.Drawing.Point(9, 12);
            this.checkBoxEnableTextures.Name = "checkBoxEnableTextures";
            this.checkBoxEnableTextures.Size = new System.Drawing.Size(103, 17);
            this.checkBoxEnableTextures.TabIndex = 0;
            this.checkBoxEnableTextures.Text = "Enable Textures";
            this.checkBoxEnableTextures.UseVisualStyleBackColor = true;
            // 
            // ModelThingy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1053, 628);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ModelThingy";
            this.Text = "ModelThingy";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private OpenTK.GLControl glViewport;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.ListBox meshSetList;
        private System.Windows.Forms.ListBox modelList;
        private System.Windows.Forms.CheckBox checkBoxEnableWireframe;
        private System.Windows.Forms.CheckBox checkBoxEnableTextures;
        private System.Windows.Forms.TextBox texturePathBox;
        private System.Windows.Forms.Label modelInfo;
    }
}