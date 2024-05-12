namespace iTrainBilderEinfügen
{
    partial class Form1
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
            if(disposing && (components != null)) {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.iTrainLayoutTB = new System.Windows.Forms.TextBox();
            this.layoutBtn = new System.Windows.Forms.Button();
            this.statusLbl = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.imageFldrTB = new System.Windows.Forms.TextBox();
            this.ImgPathBtn = new System.Windows.Forms.Button();
            this.StartBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(309, 195);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 247);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "iTrain Layout auswählen:";
            // 
            // iTrainLayoutTB
            // 
            this.iTrainLayoutTB.Location = new System.Drawing.Point(175, 244);
            this.iTrainLayoutTB.Name = "iTrainLayoutTB";
            this.iTrainLayoutTB.Size = new System.Drawing.Size(192, 20);
            this.iTrainLayoutTB.TabIndex = 2;
            this.iTrainLayoutTB.Text = "Basispfad\\layouts";
            // 
            // layoutBtn
            // 
            this.layoutBtn.Location = new System.Drawing.Point(373, 242);
            this.layoutBtn.Name = "layoutBtn";
            this.layoutBtn.Size = new System.Drawing.Size(47, 23);
            this.layoutBtn.TabIndex = 3;
            this.layoutBtn.Text = "...";
            this.layoutBtn.UseVisualStyleBackColor = true;
            this.layoutBtn.Click += new System.EventHandler(this.layoutBtn_Click);
            // 
            // statusLbl
            // 
            this.statusLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusLbl.AutoSize = true;
            this.statusLbl.Location = new System.Drawing.Point(3, 385);
            this.statusLbl.Name = "statusLbl";
            this.statusLbl.Size = new System.Drawing.Size(0, 13);
            this.statusLbl.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 276);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Ordner mit skalierten Bildern:";
            // 
            // imageFldrTB
            // 
            this.imageFldrTB.Location = new System.Drawing.Point(175, 273);
            this.imageFldrTB.Name = "imageFldrTB";
            this.imageFldrTB.Size = new System.Drawing.Size(192, 20);
            this.imageFldrTB.TabIndex = 2;
            this.imageFldrTB.Text = "Basispfad\\images";
            // 
            // ImgPathBtn
            // 
            this.ImgPathBtn.Location = new System.Drawing.Point(373, 271);
            this.ImgPathBtn.Name = "ImgPathBtn";
            this.ImgPathBtn.Size = new System.Drawing.Size(47, 23);
            this.ImgPathBtn.TabIndex = 3;
            this.ImgPathBtn.Text = "...";
            this.ImgPathBtn.UseVisualStyleBackColor = true;
            this.ImgPathBtn.Click += new System.EventHandler(this.ImgPathBtn_Click);
            // 
            // StartBtn
            // 
            this.StartBtn.Location = new System.Drawing.Point(34, 323);
            this.StartBtn.Name = "StartBtn";
            this.StartBtn.Size = new System.Drawing.Size(75, 23);
            this.StartBtn.TabIndex = 5;
            this.StartBtn.Text = "Start";
            this.StartBtn.UseVisualStyleBackColor = true;
            this.StartBtn.Click += new System.EventHandler(this.StartBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 407);
            this.Controls.Add(this.StartBtn);
            this.Controls.Add(this.statusLbl);
            this.Controls.Add(this.ImgPathBtn);
            this.Controls.Add(this.imageFldrTB);
            this.Controls.Add(this.layoutBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.iTrainLayoutTB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "iTrain Bilder einfügen";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox iTrainLayoutTB;
        private System.Windows.Forms.Button layoutBtn;
        private System.Windows.Forms.Label statusLbl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox imageFldrTB;
        private System.Windows.Forms.Button ImgPathBtn;
        private System.Windows.Forms.Button StartBtn;
    }
}

