namespace PDFChecker {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.button1 = new System.Windows.Forms.Button();
            this.parsingResultsTextBox = new System.Windows.Forms.TextBox();
            this.pdfFileTextBox = new System.Windows.Forms.TextBox();
            this.selPDFButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(187, 61);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Check";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // parsingResultsTextBox
            // 
            this.parsingResultsTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.parsingResultsTextBox.Location = new System.Drawing.Point(0, 153);
            this.parsingResultsTextBox.Multiline = true;
            this.parsingResultsTextBox.Name = "parsingResultsTextBox";
            this.parsingResultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.parsingResultsTextBox.Size = new System.Drawing.Size(450, 235);
            this.parsingResultsTextBox.TabIndex = 1;
            // 
            // pdfFileTextBox
            // 
            this.pdfFileTextBox.Location = new System.Drawing.Point(13, 13);
            this.pdfFileTextBox.Name = "pdfFileTextBox";
            this.pdfFileTextBox.ReadOnly = true;
            this.pdfFileTextBox.Size = new System.Drawing.Size(323, 20);
            this.pdfFileTextBox.TabIndex = 2;
            // 
            // selPDFButton
            // 
            this.selPDFButton.Location = new System.Drawing.Point(352, 9);
            this.selPDFButton.Name = "selPDFButton";
            this.selPDFButton.Size = new System.Drawing.Size(75, 23);
            this.selPDFButton.TabIndex = 3;
            this.selPDFButton.Text = "Select  PDF";
            this.selPDFButton.UseVisualStyleBackColor = true;
            this.selPDFButton.Click += new System.EventHandler(this.selPDFButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 134);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(226, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Documentation missing for these Lot Numbers:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 388);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selPDFButton);
            this.Controls.Add(this.pdfFileTextBox);
            this.Controls.Add(this.parsingResultsTextBox);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "PDFChecker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox parsingResultsTextBox;
        private System.Windows.Forms.TextBox pdfFileTextBox;
        private System.Windows.Forms.Button selPDFButton;
        private System.Windows.Forms.Label label1;
    }
}

