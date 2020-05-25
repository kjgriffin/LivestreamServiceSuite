namespace LSBgenerator
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbinput = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button7 = new System.Windows.Forms.Button();
            this.nWidth = new System.Windows.Forms.NumericUpDown();
            this.nHeight = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.pbTypeset = new System.Windows.Forms.PictureBox();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nPaddingLeft = new System.Windows.Forms.NumericUpDown();
            this.nPaddingRight = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lbSlides = new System.Windows.Forms.ListBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.nPaddingCol = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.nPaddingBottom = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.nPaddingTop = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nHeight)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTypeset)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingRight)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingCol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingTop)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(6, 19);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(480, 270);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 387);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Export Luma Key";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Height %";
            // 
            // tbinput
            // 
            this.tbinput.Location = new System.Drawing.Point(6, 19);
            this.tbinput.Multiline = true;
            this.tbinput.Name = "tbinput";
            this.tbinput.Size = new System.Drawing.Size(480, 472);
            this.tbinput.TabIndex = 8;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(497, 416);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Layout";
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(160, 78);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 12;
            this.button7.Text = "Set";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // nWidth
            // 
            this.nWidth.Location = new System.Drawing.Point(68, 51);
            this.nWidth.Name = "nWidth";
            this.nWidth.Size = new System.Drawing.Size(94, 20);
            this.nWidth.TabIndex = 11;
            this.nWidth.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // nHeight
            // 
            this.nHeight.Location = new System.Drawing.Point(68, 23);
            this.nHeight.Name = "nHeight";
            this.nHeight.Size = new System.Drawing.Size(94, 20);
            this.nHeight.TabIndex = 10;
            this.nHeight.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Width %";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(434, 377);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(52, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "View";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbinput);
            this.groupBox2.Location = new System.Drawing.Point(0, 422);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(497, 497);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Program";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(1009, 546);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 14;
            this.button6.Text = "View";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(118, 71);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(104, 23);
            this.button5.TabIndex = 13;
            this.button5.Text = "Change Font";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(118, 19);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 12;
            // 
            // pbTypeset
            // 
            this.pbTypeset.BackColor = System.Drawing.Color.White;
            this.pbTypeset.Location = new System.Drawing.Point(503, 0);
            this.pbTypeset.Name = "pbTypeset";
            this.pbTypeset.Size = new System.Drawing.Size(960, 540);
            this.pbTypeset.TabIndex = 10;
            this.pbTypeset.TabStop = false;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(159, 131);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 9;
            this.button3.Text = "Typeset";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.nWidth);
            this.groupBox3.Controls.Add(this.nHeight);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(6, 299);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(174, 77);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Textbox Position";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.nPaddingBottom);
            this.groupBox4.Controls.Add(this.button7);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.nPaddingTop);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.nPaddingCol);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.nPaddingRight);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.nPaddingLeft);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Location = new System.Drawing.Point(186, 299);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(242, 111);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Padding";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Left";
            // 
            // nPaddingLeft
            // 
            this.nPaddingLeft.Location = new System.Drawing.Point(44, 23);
            this.nPaddingLeft.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nPaddingLeft.Name = "nPaddingLeft";
            this.nPaddingLeft.Size = new System.Drawing.Size(69, 20);
            this.nPaddingLeft.TabIndex = 11;
            this.nPaddingLeft.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // nPaddingRight
            // 
            this.nPaddingRight.Location = new System.Drawing.Point(44, 51);
            this.nPaddingRight.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nPaddingRight.Name = "nPaddingRight";
            this.nPaddingRight.Size = new System.Drawing.Size(69, 20);
            this.nPaddingRight.TabIndex = 13;
            this.nPaddingRight.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Right";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.groupBox6);
            this.groupBox5.Controls.Add(this.button3);
            this.groupBox5.Location = new System.Drawing.Point(503, 546);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(241, 164);
            this.groupBox5.TabIndex = 15;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Typeset";
            // 
            // lbSlides
            // 
            this.lbSlides.FormattingEnabled = true;
            this.lbSlides.Location = new System.Drawing.Point(750, 546);
            this.lbSlides.Name = "lbSlides";
            this.lbSlides.Size = new System.Drawing.Size(253, 225);
            this.lbSlides.TabIndex = 17;
            this.lbSlides.SelectedValueChanged += new System.EventHandler(this.lbSlides_SelectedValueChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.textBox2);
            this.groupBox6.Controls.Add(this.textBox1);
            this.groupBox6.Controls.Add(this.button5);
            this.groupBox6.Location = new System.Drawing.Point(6, 20);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(228, 105);
            this.groupBox6.TabIndex = 14;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Font";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(118, 45);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 14;
            // 
            // nPaddingCol
            // 
            this.nPaddingCol.Location = new System.Drawing.Point(44, 76);
            this.nPaddingCol.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nPaddingCol.Name = "nPaddingCol";
            this.nPaddingCol.Size = new System.Drawing.Size(69, 20);
            this.nPaddingCol.TabIndex = 15;
            this.nPaddingCol.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Col";
            // 
            // nPaddingBottom
            // 
            this.nPaddingBottom.Location = new System.Drawing.Point(166, 51);
            this.nPaddingBottom.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nPaddingBottom.Name = "nPaddingBottom";
            this.nPaddingBottom.Size = new System.Drawing.Size(69, 20);
            this.nPaddingBottom.TabIndex = 19;
            this.nPaddingBottom.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(120, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Bottom";
            // 
            // nPaddingTop
            // 
            this.nPaddingTop.Location = new System.Drawing.Point(166, 23);
            this.nPaddingTop.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nPaddingTop.Name = "nPaddingTop";
            this.nPaddingTop.Size = new System.Drawing.Size(69, 20);
            this.nPaddingTop.TabIndex = 17;
            this.nPaddingTop.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(120, 25);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(26, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Top";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1469, 919);
            this.Controls.Add(this.lbSlides);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pbTypeset);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nHeight)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTypeset)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingRight)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingCol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPaddingTop)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbinput;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nWidth;
        private System.Windows.Forms.NumericUpDown nHeight;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.PictureBox pbTypeset;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.NumericUpDown nPaddingRight;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nPaddingLeft;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ListBox lbSlides;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.NumericUpDown nPaddingCol;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nPaddingBottom;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nPaddingTop;
        private System.Windows.Forms.Label label7;
    }
}

