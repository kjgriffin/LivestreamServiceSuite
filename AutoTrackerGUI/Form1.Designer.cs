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
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbCamPort = new System.Windows.Forms.TextBox();
            this.tbCamIP = new System.Windows.Forms.TextBox();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tbValidY = new System.Windows.Forms.TextBox();
            this.tbValidX = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.tbRangeY = new System.Windows.Forms.TextBox();
            this.tbRangeX = new System.Windows.Forms.TextBox();
            this.button12 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(8, 7);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(129, 23);
            this.comboBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(140, 7);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(129, 20);
            this.button1.TabIndex = 1;
            this.button1.Text = "Open Device";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(273, 7);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(129, 20);
            this.button2.TabIndex = 2;
            this.button2.Text = "Close Device";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(715, 10);
            this.button3.Margin = new System.Windows.Forms.Padding(2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(78, 20);
            this.button3.TabIndex = 3;
            this.button3.Text = "Connect";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(108, 91);
            this.button4.Margin = new System.Windows.Forms.Padding(2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(78, 20);
            this.button4.TabIndex = 4;
            this.button4.Text = "EMG STOP";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // tb_tgtX
            // 
            this.tb_tgtX.Location = new System.Drawing.Point(929, 11);
            this.tb_tgtX.Margin = new System.Windows.Forms.Padding(2);
            this.tb_tgtX.Name = "tb_tgtX";
            this.tb_tgtX.Size = new System.Drawing.Size(106, 23);
            this.tb_tgtX.TabIndex = 5;
            this.tb_tgtX.Text = "700";
            // 
            // tb_tgtY
            // 
            this.tb_tgtY.Location = new System.Drawing.Point(929, 33);
            this.tb_tgtY.Margin = new System.Windows.Forms.Padding(2);
            this.tb_tgtY.Name = "tb_tgtY";
            this.tb_tgtY.Size = new System.Drawing.Size(106, 23);
            this.tb_tgtY.TabIndex = 6;
            this.tb_tgtY.Text = "400";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(876, 13);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "tgt X";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(876, 33);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "tgt Y";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(876, 77);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "marg Y";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(876, 57);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 15);
            this.label4.TabIndex = 11;
            this.label4.Text = "marg X";
            // 
            // tb_mY
            // 
            this.tb_mY.Location = new System.Drawing.Point(929, 77);
            this.tb_mY.Margin = new System.Windows.Forms.Padding(2);
            this.tb_mY.Name = "tb_mY";
            this.tb_mY.Size = new System.Drawing.Size(106, 23);
            this.tb_mY.TabIndex = 10;
            this.tb_mY.Text = "50";
            // 
            // tb_mX
            // 
            this.tb_mX.Location = new System.Drawing.Point(929, 55);
            this.tb_mX.Margin = new System.Windows.Forms.Padding(2);
            this.tb_mX.Name = "tb_mX";
            this.tb_mX.Size = new System.Drawing.Size(106, 23);
            this.tb_mX.TabIndex = 9;
            this.tb_mX.Text = "50";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(703, 84);
            this.button5.Margin = new System.Windows.Forms.Padding(2);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(111, 37);
            this.button5.TabIndex = 13;
            this.button5.Text = "Track";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(703, 124);
            this.button6.Margin = new System.Windows.Forms.Padding(2);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(111, 37);
            this.button6.TabIndex = 14;
            this.button6.Text = "Stop";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(957, 104);
            this.button7.Margin = new System.Windows.Forms.Padding(2);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(78, 20);
            this.button7.TabIndex = 15;
            this.button7.Text = "Set";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(658, 55);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 15);
            this.label5.TabIndex = 19;
            this.label5.Text = "Port";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(653, 36);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 15);
            this.label6.TabIndex = 18;
            this.label6.Text = "IP Addr";
            // 
            // tbCamPort
            // 
            this.tbCamPort.Location = new System.Drawing.Point(703, 52);
            this.tbCamPort.Margin = new System.Windows.Forms.Padding(2);
            this.tbCamPort.Name = "tbCamPort";
            this.tbCamPort.Size = new System.Drawing.Size(106, 23);
            this.tbCamPort.TabIndex = 17;
            this.tbCamPort.Text = "5002";
            // 
            // tbCamIP
            // 
            this.tbCamIP.Location = new System.Drawing.Point(703, 30);
            this.tbCamIP.Margin = new System.Windows.Forms.Padding(2);
            this.tbCamIP.Name = "tbCamIP";
            this.tbCamIP.Size = new System.Drawing.Size(106, 23);
            this.tbCamIP.TabIndex = 16;
            this.tbCamIP.Text = "127.0.0.1";
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(109, 57);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.TabIndex = 20;
            this.button8.Text = "Up";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(109, 122);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 23);
            this.button9.TabIndex = 21;
            this.button9.Text = "Down";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(190, 91);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.TabIndex = 22;
            this.button10.Text = "Right";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(28, 91);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(75, 23);
            this.button11.TabIndex = 23;
            this.button11.Text = "Left";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(418, 80);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 15);
            this.label7.TabIndex = 27;
            this.label7.Text = "valid Y";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(418, 60);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 15);
            this.label8.TabIndex = 26;
            this.label8.Text = "valid X";
            // 
            // tbValidY
            // 
            this.tbValidY.Location = new System.Drawing.Point(471, 80);
            this.tbValidY.Margin = new System.Windows.Forms.Padding(2);
            this.tbValidY.Name = "tbValidY";
            this.tbValidY.Size = new System.Drawing.Size(106, 23);
            this.tbValidY.TabIndex = 25;
            this.tbValidY.Text = "50";
            // 
            // tbValidX
            // 
            this.tbValidX.Location = new System.Drawing.Point(471, 58);
            this.tbValidX.Margin = new System.Windows.Forms.Padding(2);
            this.tbValidX.Name = "tbValidX";
            this.tbValidX.Size = new System.Drawing.Size(106, 23);
            this.tbValidX.TabIndex = 24;
            this.tbValidX.Text = "50";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(418, 129);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 15);
            this.label9.TabIndex = 31;
            this.label9.Text = "range Y";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(418, 109);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 15);
            this.label10.TabIndex = 30;
            this.label10.Text = "range X";
            // 
            // tbRangeY
            // 
            this.tbRangeY.Location = new System.Drawing.Point(471, 129);
            this.tbRangeY.Margin = new System.Windows.Forms.Padding(2);
            this.tbRangeY.Name = "tbRangeY";
            this.tbRangeY.Size = new System.Drawing.Size(106, 23);
            this.tbRangeY.TabIndex = 29;
            this.tbRangeY.Text = "50";
            // 
            // tbRangeX
            // 
            this.tbRangeX.Location = new System.Drawing.Point(471, 107);
            this.tbRangeX.Margin = new System.Windows.Forms.Padding(2);
            this.tbRangeX.Name = "tbRangeX";
            this.tbRangeX.Size = new System.Drawing.Size(106, 23);
            this.tbRangeX.TabIndex = 28;
            this.tbRangeX.Text = "50";
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(499, 34);
            this.button12.Margin = new System.Windows.Forms.Padding(2);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(78, 20);
            this.button12.TabIndex = 32;
            this.button12.Text = "Set";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1135, 637);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.tbRangeY);
            this.Controls.Add(this.tbRangeX);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbValidY);
            this.Controls.Add(this.tbValidX);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbCamPort);
            this.Controls.Add(this.tbCamIP);
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
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
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
        private Label label5;
        private Label label6;
        private TextBox tbCamPort;
        private TextBox tbCamIP;
        private Button button8;
        private Button button9;
        private Button button10;
        private Button button11;
        private Label label7;
        private Label label8;
        private TextBox tbValidY;
        private TextBox tbValidX;
        private Label label9;
        private Label label10;
        private TextBox tbRangeY;
        private TextBox tbRangeX;
        private Button button12;
    }
}