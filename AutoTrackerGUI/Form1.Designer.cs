namespace AutoTrackerGUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.tb_tgtX = new System.Windows.Forms.TextBox();
            this.tb_tgtY = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_mY = new System.Windows.Forms.TextBox();
            this.tb_mX = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(182, 33);
            this.comboBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(200, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(184, 34);
            this.button1.TabIndex = 1;
            this.button1.Text = "Open Device";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(390, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(184, 34);
            this.button2.TabIndex = 2;
            this.button2.Text = "Close Device";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(926, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(112, 34);
            this.button3.TabIndex = 3;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(1044, 12);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(112, 34);
            this.button4.TabIndex = 4;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // tb_tgtX
            // 
            this.tb_tgtX.Location = new System.Drawing.Point(1327, 18);
            this.tb_tgtX.Name = "tb_tgtX";
            this.tb_tgtX.Size = new System.Drawing.Size(150, 31);
            this.tb_tgtX.TabIndex = 5;
            this.tb_tgtX.Text = "700";
            // 
            // tb_tgtY
            // 
            this.tb_tgtY.Location = new System.Drawing.Point(1327, 55);
            this.tb_tgtY.Name = "tb_tgtY";
            this.tb_tgtY.Size = new System.Drawing.Size(150, 31);
            this.tb_tgtY.TabIndex = 6;
            this.tb_tgtY.Text = "400";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1251, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 25);
            this.label1.TabIndex = 7;
            this.label1.Text = "tgt X";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1251, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 25);
            this.label2.TabIndex = 8;
            this.label2.Text = "tgt Y";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1251, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 25);
            this.label3.TabIndex = 12;
            this.label3.Text = "marg Y";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1251, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 25);
            this.label4.TabIndex = 11;
            this.label4.Text = "marg X";
            // 
            // tb_mY
            // 
            this.tb_mY.Location = new System.Drawing.Point(1327, 129);
            this.tb_mY.Name = "tb_mY";
            this.tb_mY.Size = new System.Drawing.Size(150, 31);
            this.tb_mY.TabIndex = 10;
            this.tb_mY.Text = "50";
            // 
            // tb_mX
            // 
            this.tb_mX.Location = new System.Drawing.Point(1327, 92);
            this.tb_mX.Name = "tb_mX";
            this.tb_mX.Size = new System.Drawing.Size(150, 31);
            this.tb_mX.TabIndex = 9;
            this.tb_mX.Text = "50";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(1319, 166);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(158, 61);
            this.button5.TabIndex = 13;
            this.button5.Text = "Track";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(1319, 233);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(158, 61);
            this.button6.TabIndex = 14;
            this.button6.Text = "Stop";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(1483, 18);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(112, 34);
            this.button7.TabIndex = 15;
            this.button7.Text = "Set";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1773, 1148);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_mY);
            this.Controls.Add(this.tb_mX);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_tgtY);
            this.Controls.Add(this.tb_tgtX);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox comboBox1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private TextBox tb_tgtX;
        private TextBox tb_tgtY;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox tb_mY;
        private TextBox tb_mX;
        private Button button5;
        private Button button6;
        private Button button7;
    }
}