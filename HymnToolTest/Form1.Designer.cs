namespace HymnToolTest
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
            button1 = new Button();
            tbHymnNumber = new TextBox();
            label1 = new Label();
            tbInfo = new TextBox();
            lbAltTunes = new ListBox();
            label2 = new Label();
            tbText = new TextBox();
            button2 = new Button();
            pbSrc = new PictureBox();
            pbAlt = new PictureBox();
            pbTest = new PictureBox();
            button3 = new Button();
            pbDefault = new PictureBox();
            pbCV = new PictureBox();
            tbfile = new TextBox();
            ((System.ComponentModel.ISupportInitialize)pbSrc).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbAlt).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbTest).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbDefault).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbCV).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(355, 17);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 0;
            button1.Text = "Search";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // tbHymnNumber
            // 
            tbHymnNumber.Location = new Point(174, 17);
            tbHymnNumber.Name = "tbHymnNumber";
            tbHymnNumber.Size = new Size(150, 31);
            tbHymnNumber.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 20);
            label1.Name = "label1";
            label1.Size = new Size(159, 25);
            label1.TabIndex = 2;
            label1.Text = "Src Hymn Number";
            // 
            // tbInfo
            // 
            tbInfo.Location = new Point(29, 79);
            tbInfo.Multiline = true;
            tbInfo.Name = "tbInfo";
            tbInfo.Size = new Size(520, 246);
            tbInfo.TabIndex = 3;
            // 
            // lbAltTunes
            // 
            lbAltTunes.FormattingEnabled = true;
            lbAltTunes.ItemHeight = 25;
            lbAltTunes.Location = new Point(599, 79);
            lbAltTunes.Name = "lbAltTunes";
            lbAltTunes.Size = new Size(520, 404);
            lbAltTunes.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(599, 23);
            label2.Name = "label2";
            label2.Size = new Size(85, 25);
            label2.TabIndex = 5;
            label2.Text = "Alt Tunes";
            // 
            // tbText
            // 
            tbText.Location = new Point(29, 331);
            tbText.Multiline = true;
            tbText.Name = "tbText";
            tbText.Size = new Size(520, 228);
            tbText.TabIndex = 6;
            // 
            // button2
            // 
            button2.Location = new Point(1007, 512);
            button2.Name = "button2";
            button2.Size = new Size(112, 34);
            button2.TabIndex = 8;
            button2.Text = "Search";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // pbSrc
            // 
            pbSrc.Location = new Point(29, 565);
            pbSrc.Name = "pbSrc";
            pbSrc.Size = new Size(520, 456);
            pbSrc.TabIndex = 9;
            pbSrc.TabStop = false;
            // 
            // pbAlt
            // 
            pbAlt.Location = new Point(599, 565);
            pbAlt.Name = "pbAlt";
            pbAlt.Size = new Size(520, 456);
            pbAlt.TabIndex = 10;
            pbAlt.TabStop = false;
            // 
            // pbTest
            // 
            pbTest.Location = new Point(1125, 565);
            pbTest.Name = "pbTest";
            pbTest.Size = new Size(520, 456);
            pbTest.TabIndex = 11;
            pbTest.TabStop = false;
            // 
            // button3
            // 
            button3.Location = new Point(1139, 20);
            button3.Name = "button3";
            button3.Size = new Size(112, 34);
            button3.TabIndex = 12;
            button3.Text = "TestSearch";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // pbDefault
            // 
            pbDefault.Location = new Point(1139, 81);
            pbDefault.Name = "pbDefault";
            pbDefault.Size = new Size(613, 152);
            pbDefault.TabIndex = 13;
            pbDefault.TabStop = false;
            // 
            // pbCV
            // 
            pbCV.Location = new Point(1139, 249);
            pbCV.Name = "pbCV";
            pbCV.Size = new Size(613, 152);
            pbCV.TabIndex = 14;
            pbCV.TabStop = false;
            // 
            // textBox1
            // 
            tbfile.Location = new Point(1257, 23);
            tbfile.Name = "textBox1";
            tbfile.Size = new Size(495, 31);
            tbfile.TabIndex = 15;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1907, 1043);
            Controls.Add(tbfile);
            Controls.Add(pbCV);
            Controls.Add(pbDefault);
            Controls.Add(button3);
            Controls.Add(pbTest);
            Controls.Add(pbAlt);
            Controls.Add(pbSrc);
            Controls.Add(button2);
            Controls.Add(tbText);
            Controls.Add(label2);
            Controls.Add(lbAltTunes);
            Controls.Add(tbInfo);
            Controls.Add(label1);
            Controls.Add(tbHymnNumber);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pbSrc).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbAlt).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbTest).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbDefault).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbCV).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox tbHymnNumber;
        private Label label1;
        private TextBox tbInfo;
        private ListBox lbAltTunes;
        private Label label2;
        private TextBox tbText;
        private Button button2;
        private PictureBox pbSrc;
        private PictureBox pbAlt;
        private PictureBox pbTest;
        private Button button3;
        private PictureBox pbDefault;
        private PictureBox pbCV;
        private TextBox tbfile;
    }
}