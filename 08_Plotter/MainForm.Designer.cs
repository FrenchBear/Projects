namespace Plotter;

partial class MainForm
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
            this.picOut = new System.Windows.Forms.PictureBox();
            this.TestComboBox = new System.Windows.Forms.ComboBox();
            this.PrintersList = new System.Windows.Forms.ComboBox();
            this.PrintButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picOut)).BeginInit();
            this.SuspendLayout();
            // 
            // picOut
            // 
            this.picOut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picOut.Location = new System.Drawing.Point(14, 99);
            this.picOut.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.picOut.Name = "picOut";
            this.picOut.Size = new System.Drawing.Size(1110, 1097);
            this.picOut.TabIndex = 4;
            this.picOut.TabStop = false;
            // 
            // TestComboBox
            // 
            this.TestComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TestComboBox.FormattingEnabled = true;
            this.TestComboBox.Location = new System.Drawing.Point(14, 12);
            this.TestComboBox.Name = "TestComboBox";
            this.TestComboBox.Size = new System.Drawing.Size(519, 33);
            this.TestComboBox.TabIndex = 5;
            this.TestComboBox.SelectedIndexChanged += new System.EventHandler(this.TestComboBox_SelectedIndexChanged);
            // 
            // PrintersList
            // 
            this.PrintersList.AllowDrop = true;
            this.PrintersList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PrintersList.FormattingEnabled = true;
            this.PrintersList.Location = new System.Drawing.Point(12, 51);
            this.PrintersList.Name = "PrintersList";
            this.PrintersList.Size = new System.Drawing.Size(521, 33);
            this.PrintersList.TabIndex = 7;
            // 
            // PrintButton
            // 
            this.PrintButton.Location = new System.Drawing.Point(551, 50);
            this.PrintButton.Name = "PrintButton";
            this.PrintButton.Size = new System.Drawing.Size(113, 33);
            this.PrintButton.TabIndex = 6;
            this.PrintButton.Text = "&Print";
            this.PrintButton.UseVisualStyleBackColor = true;
            this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1138, 1211);
            this.Controls.Add(this.PrintersList);
            this.Controls.Add(this.PrintButton);
            this.Controls.Add(this.TestComboBox);
            this.Controls.Add(this.picOut);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Plotter Test Form";
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picOut)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox picOut;
    private System.Windows.Forms.ComboBox TestComboBox;
    private System.Windows.Forms.ComboBox PrintersList;
    private System.Windows.Forms.Button PrintButton;
}
