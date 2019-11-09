namespace ShadowTH_Text_Editor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.subtitleTextBox = new System.Windows.Forms.TextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.subtitleTable_ListBox = new System.Windows.Forms.ListBox();
            this.aboutButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // subtitleTextBox
            // 
            this.subtitleTextBox.BackColor = System.Drawing.Color.Black;
            this.subtitleTextBox.ForeColor = System.Drawing.Color.Yellow;
            this.subtitleTextBox.Location = new System.Drawing.Point(165, 23);
            this.subtitleTextBox.Multiline = true;
            this.subtitleTextBox.Name = "subtitleTextBox";
            this.subtitleTextBox.Size = new System.Drawing.Size(269, 290);
            this.subtitleTextBox.TabIndex = 0;
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.Black;
            this.buttonSave.ForeColor = System.Drawing.Color.Yellow;
            this.buttonSave.Location = new System.Drawing.Point(307, 323);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(113, 42);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save File";
            this.buttonSave.UseVisualStyleBackColor = false;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonOpen
            // 
            this.buttonOpen.BackColor = System.Drawing.Color.Black;
            this.buttonOpen.ForeColor = System.Drawing.Color.Yellow;
            this.buttonOpen.Location = new System.Drawing.Point(180, 323);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(102, 42);
            this.buttonOpen.TabIndex = 2;
            this.buttonOpen.Text = "Open File";
            this.buttonOpen.UseVisualStyleBackColor = false;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // subtitleTable_ListBox
            // 
            this.subtitleTable_ListBox.BackColor = System.Drawing.Color.Black;
            this.subtitleTable_ListBox.ForeColor = System.Drawing.Color.Yellow;
            this.subtitleTable_ListBox.FormattingEnabled = true;
            this.subtitleTable_ListBox.Location = new System.Drawing.Point(12, 23);
            this.subtitleTable_ListBox.Name = "subtitleTable_ListBox";
            this.subtitleTable_ListBox.Size = new System.Drawing.Size(134, 290);
            this.subtitleTable_ListBox.TabIndex = 3;
            this.subtitleTable_ListBox.SelectedIndexChanged += new System.EventHandler(this.subtitleTable_ListBox_SelectedIndexChanged);
            // 
            // aboutButton
            // 
            this.aboutButton.BackColor = System.Drawing.Color.Black;
            this.aboutButton.ForeColor = System.Drawing.Color.Yellow;
            this.aboutButton.Location = new System.Drawing.Point(26, 323);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(102, 42);
            this.aboutButton.TabIndex = 4;
            this.aboutButton.Text = "About";
            this.aboutButton.UseVisualStyleBackColor = false;
            this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "*.fnt";
            this.openFileDialog.Filter = "Shadow FNT files|*.fnt|All files|*.*";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "fnt";
            this.saveFileDialog.FileName = "*.fnt";
            this.saveFileDialog.Filter = "Shadow FNT files|*.fnt|All files|*.*";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(446, 377);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.subtitleTable_ListBox);
            this.Controls.Add(this.buttonOpen);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.subtitleTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "ShadowTH Text Editor Prototype Edition";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox subtitleTextBox;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.ListBox subtitleTable_ListBox;
        private System.Windows.Forms.Button aboutButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}

