namespace PlotterLibrary;

internal partial class RenderingForm
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
        this.PrintersList.Location = new System.Drawing.Point(154, 15);
        this.PrintersList.Name = "PrintersList";
        this.PrintersList.Size = new System.Drawing.Size(551, 33);
        this.PrintersList.TabIndex = 5;
        // 
        // PrintButton
        // 
        this.PrintButton.Location = new System.Drawing.Point(11, 15);
        this.PrintButton.Name = "PrintButton";
        this.PrintButton.Size = new System.Drawing.Size(113, 33);
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
        this.picOut.Location = new System.Drawing.Point(14, 57);
        this.picOut.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
        this.picOut.Name = "picOut";
        this.picOut.Size = new System.Drawing.Size(1189, 719);
        this.picOut.TabIndex = 6;
        this.picOut.TabStop = false;
        // 
        // RenderingForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1217, 791);
        this.Controls.Add(this.picOut);
        this.Controls.Add(this.PrintersList);
        this.Controls.Add(this.PrintButton);
        this.Margin = new System.Windows.Forms.Padding(2);
        this.Name = "RenderingForm";
        this.Text = "Plotter Rendering";
        this.Resize += new System.EventHandler(this.RenderingForm_Resize);
        ((System.ComponentModel.ISupportInitialize)(this.picOut)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox PrintersList;
    private System.Windows.Forms.Button PrintButton;
    public System.Windows.Forms.PictureBox picOut;
}
