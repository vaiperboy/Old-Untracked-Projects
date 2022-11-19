namespace ikout_assister {
    partial class MainForm {
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
            this.mainButton = new System.Windows.Forms.Button();
            this.output = new System.Windows.Forms.RichTextBox();
            this.displayCardList = new System.Windows.Forms.CheckBox();
            this.displayCardCount = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // mainButton
            // 
            this.mainButton.Location = new System.Drawing.Point(22, 12);
            this.mainButton.Name = "mainButton";
            this.mainButton.Size = new System.Drawing.Size(178, 76);
            this.mainButton.TabIndex = 0;
            this.mainButton.Text = "Start Cheat";
            this.mainButton.UseVisualStyleBackColor = true;
            this.mainButton.Click += new System.EventHandler(this.mainButton_Click);
            // 
            // output
            // 
            this.output.Location = new System.Drawing.Point(22, 118);
            this.output.Name = "output";
            this.output.ReadOnly = true;
            this.output.Size = new System.Drawing.Size(271, 262);
            this.output.TabIndex = 1;
            this.output.Text = "";
            // 
            // displayCardList
            // 
            this.displayCardList.AutoSize = true;
            this.displayCardList.Enabled = false;
            this.displayCardList.Location = new System.Drawing.Point(206, 71);
            this.displayCardList.Name = "displayCardList";
            this.displayCardList.Size = new System.Drawing.Size(75, 17);
            this.displayCardList.TabIndex = 2;
            this.displayCardList.Text = "Display list";
            this.displayCardList.UseVisualStyleBackColor = true;
            this.displayCardList.CheckedChanged += new System.EventHandler(this.displayCardList_CheckedChanged);
            // 
            // displayCardCount
            // 
            this.displayCardCount.AutoSize = true;
            this.displayCardCount.Checked = true;
            this.displayCardCount.CheckState = System.Windows.Forms.CheckState.Checked;
            this.displayCardCount.Enabled = false;
            this.displayCardCount.Location = new System.Drawing.Point(206, 12);
            this.displayCardCount.Name = "displayCardCount";
            this.displayCardCount.Size = new System.Drawing.Size(84, 30);
            this.displayCardCount.TabIndex = 3;
            this.displayCardCount.Text = "Display card\r\n count";
            this.displayCardCount.UseVisualStyleBackColor = true;
            this.displayCardCount.CheckedChanged += new System.EventHandler(this.displayCardCount_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(305, 427);
            this.Controls.Add(this.displayCardCount);
            this.Controls.Add(this.displayCardList);
            this.Controls.Add(this.output);
            this.Controls.Add(this.mainButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button mainButton;
        private System.Windows.Forms.RichTextBox output;
        private System.Windows.Forms.CheckBox displayCardList;
        private System.Windows.Forms.CheckBox displayCardCount;
    }
}