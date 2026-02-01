namespace MonitorInstaller
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tb_key = new System.Windows.Forms.TextBox();
            this.btn_install = new System.Windows.Forms.Button();
            this.btn_uninstall = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.reset = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(782, 460);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // tb_key
            // 
            this.tb_key.Location = new System.Drawing.Point(92, 490);
            this.tb_key.Name = "tb_key";
            this.tb_key.Size = new System.Drawing.Size(702, 25);
            this.tb_key.TabIndex = 1;
            // 
            // btn_install
            // 
            this.btn_install.Location = new System.Drawing.Point(173, 538);
            this.btn_install.Name = "btn_install";
            this.btn_install.Size = new System.Drawing.Size(100, 40);
            this.btn_install.TabIndex = 2;
            this.btn_install.Text = "安装";
            this.btn_install.UseVisualStyleBackColor = true;
            this.btn_install.Click += new System.EventHandler(this.btn_install_Click);
            // 
            // btn_uninstall
            // 
            this.btn_uninstall.Location = new System.Drawing.Point(479, 538);
            this.btn_uninstall.Name = "btn_uninstall";
            this.btn_uninstall.Size = new System.Drawing.Size(100, 40);
            this.btn_uninstall.TabIndex = 3;
            this.btn_uninstall.Text = "卸载";
            this.btn_uninstall.UseVisualStyleBackColor = true;
            this.btn_uninstall.Click += new System.EventHandler(this.btn_uninstall_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 493);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "API密钥";
            // 
            // reset
            // 
            this.reset.Location = new System.Drawing.Point(326, 538);
            this.reset.Name = "reset";
            this.reset.Size = new System.Drawing.Size(100, 40);
            this.reset.TabIndex = 2;
            this.reset.Text = "重设密钥";
            this.reset.UseVisualStyleBackColor = true;
            this.reset.Click += new System.EventHandler(this.btn_reset_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 599);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_uninstall);
            this.Controls.Add(this.reset);
            this.Controls.Add(this.btn_install);
            this.Controls.Add(this.tb_key);
            this.Controls.Add(this.pictureBox1);
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "微信/QQ聊天记录安装器";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox tb_key;
        private System.Windows.Forms.Button btn_install;
        private System.Windows.Forms.Button btn_uninstall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button reset;
    }
}

