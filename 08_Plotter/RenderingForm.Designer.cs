namespace Plotter;

partial class RenderingForm
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
            this.PrintersList = new System.Windows.Forms.ComboBox();
            this.PrintButton = new System.Windows.Forms.Button();
            this.picOut = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picOut)).BeginInit();
            this.SuspendLayout();
            // 
            // PrintersList
            // 
            this.PrintersList.AllowDrop = true;
            this.PrintersList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PrintersList.FormattingEnabled = true;
            this.PrintersList.Location = new System.Drawing.Point(231, 22);
            this.PrintersList.Margin = new System.Windows.Forms.Padding(4);
            this.PrintersList.Name = "PrintersList";
            this.PrintersList.Size = new System.Drawing.Size(824, 45);
            this.PrintersList.TabIndex = 5;
            // 
            // PrintButton
            // 
            this.PrintButton.Location = new System.Drawing.Point(17, 22);
            this.PrintButton.Margin = new System.Windows.Forms.Padding(4);
            this.PrintButton.Name = "PrintButton";
            this.PrintButton.Size = new System.Drawing.Size(170, 49);
            this.PrintButton.TabIndex = 4;
            this.PrintButton.Text = "&Print";
            this.PrintButton.UseVisualStyleBackColor = true;
            this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
            // 
            // picOut
            // 
            this.picOut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picOut.Location = new System.Drawing.Point(17, 96);
            this.picOut.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.picOut.Name = "picOut";
            this.picOut.Size = new System.Drawing.Size(1791, 1057);
            this.picOut.TabIndex = 3;
            this.picOut.TabStop = false;
            // 
            // RenderingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 37F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1825, 1171);
            this.Controls.Add(this.PrintersList);
            this.Controls.Add(this.PrintButton);
            this.Controls.Add(this.picOut);
            this.Name = "RenderingForm";
            this.Text = "Plotter Rendering";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RenderingForm_FormClosed);
            this.Resize += new System.EventHandler(this.RenderingForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picOut)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox PrintersList;
    private System.Windows.Forms.Button PrintButton;
    private System.Windows.Forms.PictureBox picOut;
}
