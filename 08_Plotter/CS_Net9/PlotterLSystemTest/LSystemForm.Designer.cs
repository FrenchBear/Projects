namespace LSystemTest;

partial class LSystemForm
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
        picOut = new System.Windows.Forms.PictureBox();
        PrintButton = new System.Windows.Forms.Button();
        PrintingGroupBox = new System.Windows.Forms.GroupBox();
        PrintersList = new System.Windows.Forms.ComboBox();
        MainSplitContainer = new System.Windows.Forms.SplitContainer();
        groupBox1 = new System.Windows.Forms.GroupBox();
        tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
        label2 = new System.Windows.Forms.Label();
        SourcesComboBox = new System.Windows.Forms.ComboBox();
        RulesTextBox = new System.Windows.Forms.TextBox();
        label6 = new System.Windows.Forms.Label();
        AxiomTextBox = new System.Windows.Forms.TextBox();
        label4 = new System.Windows.Forms.Label();
        AngleTextBox = new System.Windows.Forms.TextBox();
        label3 = new System.Windows.Forms.Label();
        CommentTextBox = new System.Windows.Forms.TextBox();
        LSystemsComboBox = new System.Windows.Forms.ComboBox();
        label11 = new System.Windows.Forms.Label();
        label12 = new System.Windows.Forms.Label();
        ForceMonochromeCheckBox = new System.Windows.Forms.CheckBox();
        ColorsComboBox = new System.Windows.Forms.ComboBox();
        label5 = new System.Windows.Forms.Label();
        DepthUpDown = new System.Windows.Forms.NumericUpDown();
        label1 = new System.Windows.Forms.Label();
        SmoothingGroupBox = new System.Windows.Forms.GroupBox();
        tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
        SmoothingMethodsComboBox = new System.Windows.Forms.ComboBox();
        label9 = new System.Windows.Forms.Label();
        label8 = new System.Windows.Forms.Label();
        label7 = new System.Windows.Forms.Label();
        IterationsUpDown = new System.Windows.Forms.NumericUpDown();
        IterationsTrackBar = new System.Windows.Forms.TrackBar();
        TensionUpDown = new System.Windows.Forms.NumericUpDown();
        TensionTrackBar = new System.Windows.Forms.TrackBar();
        ((System.ComponentModel.ISupportInitialize)picOut).BeginInit();
        PrintingGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)MainSplitContainer).BeginInit();
        MainSplitContainer.Panel1.SuspendLayout();
        MainSplitContainer.Panel2.SuspendLayout();
        MainSplitContainer.SuspendLayout();
        groupBox1.SuspendLayout();
        tableLayoutPanel3.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)DepthUpDown).BeginInit();
        SmoothingGroupBox.SuspendLayout();
        tableLayoutPanel2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)IterationsUpDown).BeginInit();
        ((System.ComponentModel.ISupportInitialize)IterationsTrackBar).BeginInit();
        ((System.ComponentModel.ISupportInitialize)TensionUpDown).BeginInit();
        ((System.ComponentModel.ISupportInitialize)TensionTrackBar).BeginInit();
        SuspendLayout();
        // 
        // picOut
        // 
        picOut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        picOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        picOut.Location = new System.Drawing.Point(0, 262);
        picOut.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        picOut.Name = "picOut";
        picOut.Size = new System.Drawing.Size(1250, 598);
        picOut.TabIndex = 4;
        picOut.TabStop = false;
        // 
        // PrintButton
        // 
        PrintButton.Location = new System.Drawing.Point(4, 41);
        PrintButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        PrintButton.Name = "PrintButton";
        PrintButton.Size = new System.Drawing.Size(105, 20);
        PrintButton.TabIndex = 6;
        PrintButton.Text = "&Print";
        PrintButton.UseVisualStyleBackColor = true;
        PrintButton.Click += PrintButton_Click;
        // 
        // PrintingGroupBox
        // 
        PrintingGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        PrintingGroupBox.Controls.Add(PrintersList);
        PrintingGroupBox.Controls.Add(PrintButton);
        PrintingGroupBox.Location = new System.Drawing.Point(0, 106);
        PrintingGroupBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        PrintingGroupBox.Name = "PrintingGroupBox";
        PrintingGroupBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
        PrintingGroupBox.Size = new System.Drawing.Size(531, 68);
        PrintingGroupBox.TabIndex = 9;
        PrintingGroupBox.TabStop = false;
        PrintingGroupBox.Text = "Printing";
        // 
        // PrintersList
        // 
        PrintersList.AllowDrop = true;
        PrintersList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        PrintersList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        PrintersList.FormattingEnabled = true;
        PrintersList.Location = new System.Drawing.Point(4, 18);
        PrintersList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        PrintersList.Name = "PrintersList";
        PrintersList.Size = new System.Drawing.Size(526, 23);
        PrintersList.TabIndex = 8;
        // 
        // MainSplitContainer
        // 
        MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Top;
        MainSplitContainer.Location = new System.Drawing.Point(0, 0);
        MainSplitContainer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        MainSplitContainer.Name = "MainSplitContainer";
        // 
        // MainSplitContainer.Panel1
        // 
        MainSplitContainer.Panel1.Controls.Add(groupBox1);
        // 
        // MainSplitContainer.Panel2
        // 
        MainSplitContainer.Panel2.Controls.Add(ForceMonochromeCheckBox);
        MainSplitContainer.Panel2.Controls.Add(ColorsComboBox);
        MainSplitContainer.Panel2.Controls.Add(label5);
        MainSplitContainer.Panel2.Controls.Add(DepthUpDown);
        MainSplitContainer.Panel2.Controls.Add(label1);
        MainSplitContainer.Panel2.Controls.Add(SmoothingGroupBox);
        MainSplitContainer.Panel2.Controls.Add(PrintingGroupBox);
        MainSplitContainer.Size = new System.Drawing.Size(1253, 256);
        MainSplitContainer.SplitterDistance = 604;
        MainSplitContainer.SplitterWidth = 3;
        MainSplitContainer.TabIndex = 11;
        // 
        // groupBox1
        // 
        groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        groupBox1.Controls.Add(tableLayoutPanel3);
        groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
        groupBox1.Location = new System.Drawing.Point(0, 0);
        groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        groupBox1.Name = "groupBox1";
        groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
        groupBox1.Size = new System.Drawing.Size(604, 254);
        groupBox1.TabIndex = 12;
        groupBox1.TabStop = false;
        groupBox1.Text = "L-System";
        // 
        // tableLayoutPanel3
        // 
        tableLayoutPanel3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tableLayoutPanel3.ColumnCount = 2;
        tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 84F));
        tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 512F));
        tableLayoutPanel3.Controls.Add(label2, 0, 0);
        tableLayoutPanel3.Controls.Add(SourcesComboBox, 1, 0);
        tableLayoutPanel3.Controls.Add(RulesTextBox, 1, 5);
        tableLayoutPanel3.Controls.Add(label6, 0, 5);
        tableLayoutPanel3.Controls.Add(AxiomTextBox, 1, 4);
        tableLayoutPanel3.Controls.Add(label4, 0, 4);
        tableLayoutPanel3.Controls.Add(AngleTextBox, 1, 3);
        tableLayoutPanel3.Controls.Add(label3, 0, 3);
        tableLayoutPanel3.Controls.Add(CommentTextBox, 1, 2);
        tableLayoutPanel3.Controls.Add(LSystemsComboBox, 1, 1);
        tableLayoutPanel3.Controls.Add(label11, 0, 2);
        tableLayoutPanel3.Controls.Add(label12, 0, 1);
        tableLayoutPanel3.Location = new System.Drawing.Point(4, 18);
        tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        tableLayoutPanel3.Name = "tableLayoutPanel3";
        tableLayoutPanel3.RowCount = 6;
        tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
        tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
        tableLayoutPanel3.Size = new System.Drawing.Size(595, 236);
        tableLayoutPanel3.TabIndex = 1;
        // 
        // label2
        // 
        label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(2, 4);
        label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(43, 15);
        label2.TabIndex = 25;
        label2.Text = "Source";
        label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // SourcesComboBox
        // 
        SourcesComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        SourcesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        SourcesComboBox.FormattingEnabled = true;
        SourcesComboBox.Location = new System.Drawing.Point(86, 2);
        SourcesComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        SourcesComboBox.Name = "SourcesComboBox";
        SourcesComboBox.Size = new System.Drawing.Size(508, 23);
        SourcesComboBox.TabIndex = 24;
        SourcesComboBox.SelectedIndexChanged += SourcesComboBox_SelectedIndexChanged;
        // 
        // RulesTextBox
        // 
        RulesTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        RulesTextBox.Location = new System.Drawing.Point(86, 158);
        RulesTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        RulesTextBox.Multiline = true;
        RulesTextBox.Name = "RulesTextBox";
        RulesTextBox.Size = new System.Drawing.Size(508, 76);
        RulesTextBox.TabIndex = 23;
        RulesTextBox.TextChanged += RulesTextBox_TextChanged;
        // 
        // label6
        // 
        label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label6.AutoSize = true;
        label6.Location = new System.Drawing.Point(2, 188);
        label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label6.Name = "label6";
        label6.Size = new System.Drawing.Size(35, 15);
        label6.TabIndex = 22;
        label6.Text = "Rules";
        label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // AxiomTextBox
        // 
        AxiomTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        AxiomTextBox.Location = new System.Drawing.Point(86, 134);
        AxiomTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        AxiomTextBox.Name = "AxiomTextBox";
        AxiomTextBox.Size = new System.Drawing.Size(508, 23);
        AxiomTextBox.TabIndex = 21;
        AxiomTextBox.TextChanged += AxiomTextBox_TextChanged;
        // 
        // label4
        // 
        label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label4.AutoSize = true;
        label4.Location = new System.Drawing.Point(2, 136);
        label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(42, 15);
        label4.TabIndex = 20;
        label4.Text = "Axiom";
        label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // AngleTextBox
        // 
        AngleTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
        AngleTextBox.Location = new System.Drawing.Point(86, 110);
        AngleTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        AngleTextBox.Name = "AngleTextBox";
        AngleTextBox.Size = new System.Drawing.Size(67, 23);
        AngleTextBox.TabIndex = 19;
        AngleTextBox.TextChanged += AngleTextBox_TextChanged;
        // 
        // label3
        // 
        label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label3.AutoSize = true;
        label3.Location = new System.Drawing.Point(2, 112);
        label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(38, 15);
        label3.TabIndex = 18;
        label3.Text = "Angle";
        label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // CommentTextBox
        // 
        CommentTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        CommentTextBox.Location = new System.Drawing.Point(86, 50);
        CommentTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        CommentTextBox.Multiline = true;
        CommentTextBox.Name = "CommentTextBox";
        CommentTextBox.Size = new System.Drawing.Size(508, 56);
        CommentTextBox.TabIndex = 16;
        // 
        // LSystemsComboBox
        // 
        LSystemsComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        LSystemsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        LSystemsComboBox.FormattingEnabled = true;
        LSystemsComboBox.Location = new System.Drawing.Point(86, 26);
        LSystemsComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        LSystemsComboBox.Name = "LSystemsComboBox";
        LSystemsComboBox.Size = new System.Drawing.Size(508, 23);
        LSystemsComboBox.TabIndex = 12;
        LSystemsComboBox.SelectedIndexChanged += LSystemsComboBox_SelectedIndexChanged;
        // 
        // label11
        // 
        label11.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label11.AutoSize = true;
        label11.Location = new System.Drawing.Point(2, 70);
        label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label11.Name = "label11";
        label11.Size = new System.Drawing.Size(61, 15);
        label11.TabIndex = 10;
        label11.Text = "Comment";
        label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // label12
        // 
        label12.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label12.AutoSize = true;
        label12.Location = new System.Drawing.Point(2, 28);
        label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label12.Name = "label12";
        label12.Size = new System.Drawing.Size(64, 15);
        label12.TabIndex = 9;
        label12.Text = "Predefined";
        label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // ForceMonochromeCheckBox
        // 
        ForceMonochromeCheckBox.AutoSize = true;
        ForceMonochromeCheckBox.Location = new System.Drawing.Point(422, 212);
        ForceMonochromeCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        ForceMonochromeCheckBox.Name = "ForceMonochromeCheckBox";
        ForceMonochromeCheckBox.Size = new System.Drawing.Size(131, 19);
        ForceMonochromeCheckBox.TabIndex = 18;
        ForceMonochromeCheckBox.Text = "Force monochrome";
        ForceMonochromeCheckBox.UseVisualStyleBackColor = true;
        ForceMonochromeCheckBox.CheckedChanged += ForceMonochromeCheckBox_CheckedChanged;
        // 
        // ColorsComboBox
        // 
        ColorsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        ColorsComboBox.FormattingEnabled = true;
        ColorsComboBox.Location = new System.Drawing.Point(90, 211);
        ColorsComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        ColorsComboBox.Name = "ColorsComboBox";
        ColorsComboBox.Size = new System.Drawing.Size(298, 23);
        ColorsComboBox.TabIndex = 17;
        ColorsComboBox.SelectedIndexChanged += ColorsComboBox_SelectedIndexChanged;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new System.Drawing.Point(6, 212);
        label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label5.Name = "label5";
        label5.Size = new System.Drawing.Size(75, 15);
        label5.TabIndex = 16;
        label5.Text = "Default color";
        // 
        // DepthUpDown
        // 
        DepthUpDown.Location = new System.Drawing.Point(90, 187);
        DepthUpDown.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        DepthUpDown.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
        DepthUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        DepthUpDown.Name = "DepthUpDown";
        DepthUpDown.Size = new System.Drawing.Size(56, 23);
        DepthUpDown.TabIndex = 15;
        DepthUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
        DepthUpDown.ValueChanged += DepthUpDown_ValueChanged;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(6, 191);
        label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(39, 15);
        label1.TabIndex = 14;
        label1.Text = "Depth";
        // 
        // SmoothingGroupBox
        // 
        SmoothingGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        SmoothingGroupBox.Controls.Add(tableLayoutPanel2);
        SmoothingGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
        SmoothingGroupBox.Location = new System.Drawing.Point(0, 0);
        SmoothingGroupBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        SmoothingGroupBox.Name = "SmoothingGroupBox";
        SmoothingGroupBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
        SmoothingGroupBox.Size = new System.Drawing.Size(646, 100);
        SmoothingGroupBox.TabIndex = 11;
        SmoothingGroupBox.TabStop = false;
        SmoothingGroupBox.Text = "Smoothing";
        // 
        // tableLayoutPanel2
        // 
        tableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        tableLayoutPanel2.ColumnCount = 3;
        tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 84F));
        tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 84F));
        tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 469F));
        tableLayoutPanel2.Controls.Add(SmoothingMethodsComboBox, 0, 0);
        tableLayoutPanel2.Controls.Add(label9, 0, 2);
        tableLayoutPanel2.Controls.Add(label8, 0, 1);
        tableLayoutPanel2.Controls.Add(label7, 0, 0);
        tableLayoutPanel2.Controls.Add(IterationsUpDown, 1, 1);
        tableLayoutPanel2.Controls.Add(IterationsTrackBar, 2, 1);
        tableLayoutPanel2.Controls.Add(TensionUpDown, 1, 2);
        tableLayoutPanel2.Controls.Add(TensionTrackBar, 2, 2);
        tableLayoutPanel2.Location = new System.Drawing.Point(4, 18);
        tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        tableLayoutPanel2.Name = "tableLayoutPanel2";
        tableLayoutPanel2.RowCount = 4;
        tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
        tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
        tableLayoutPanel2.Size = new System.Drawing.Size(638, 73);
        tableLayoutPanel2.TabIndex = 1;
        // 
        // SmoothingMethodsComboBox
        // 
        SmoothingMethodsComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        tableLayoutPanel2.SetColumnSpan(SmoothingMethodsComboBox, 2);
        SmoothingMethodsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        SmoothingMethodsComboBox.FormattingEnabled = true;
        SmoothingMethodsComboBox.Location = new System.Drawing.Point(86, 2);
        SmoothingMethodsComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        SmoothingMethodsComboBox.Name = "SmoothingMethodsComboBox";
        SmoothingMethodsComboBox.Size = new System.Drawing.Size(550, 23);
        SmoothingMethodsComboBox.TabIndex = 12;
        SmoothingMethodsComboBox.SelectedIndexChanged += SmoothingMethodsComboBox_SelectedIndexChanged;
        // 
        // label9
        // 
        label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label9.AutoSize = true;
        label9.Location = new System.Drawing.Point(2, 52);
        label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label9.Name = "label9";
        label9.Size = new System.Drawing.Size(47, 15);
        label9.TabIndex = 11;
        label9.Text = "Tension";
        label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // label8
        // 
        label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label8.AutoSize = true;
        label8.Location = new System.Drawing.Point(2, 28);
        label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label8.Name = "label8";
        label8.Size = new System.Drawing.Size(56, 15);
        label8.TabIndex = 10;
        label8.Text = "Iterations";
        label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // label7
        // 
        label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label7.AutoSize = true;
        label7.Location = new System.Drawing.Point(2, 4);
        label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
        label7.Name = "label7";
        label7.Size = new System.Drawing.Size(49, 15);
        label7.TabIndex = 9;
        label7.Text = "Method";
        label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // IterationsUpDown
        // 
        IterationsUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
        IterationsUpDown.Location = new System.Drawing.Point(86, 26);
        IterationsUpDown.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        IterationsUpDown.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        IterationsUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        IterationsUpDown.Name = "IterationsUpDown";
        IterationsUpDown.Size = new System.Drawing.Size(68, 23);
        IterationsUpDown.TabIndex = 13;
        IterationsUpDown.Value = new decimal(new int[] { 2, 0, 0, 0 });
        IterationsUpDown.ValueChanged += IterationsUpDown_ValueChanged;
        // 
        // IterationsTrackBar
        // 
        IterationsTrackBar.Location = new System.Drawing.Point(170, 26);
        IterationsTrackBar.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        IterationsTrackBar.Minimum = 1;
        IterationsTrackBar.Name = "IterationsTrackBar";
        IterationsTrackBar.Size = new System.Drawing.Size(246, 20);
        IterationsTrackBar.TabIndex = 14;
        IterationsTrackBar.Value = 2;
        IterationsTrackBar.Scroll += IterationsTrackBar_Scroll;
        // 
        // TensionUpDown
        // 
        TensionUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
        TensionUpDown.DecimalPlaces = 2;
        TensionUpDown.Location = new System.Drawing.Point(86, 50);
        TensionUpDown.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        TensionUpDown.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
        TensionUpDown.Name = "TensionUpDown";
        TensionUpDown.Size = new System.Drawing.Size(68, 23);
        TensionUpDown.TabIndex = 15;
        TensionUpDown.Value = new decimal(new int[] { 5, 0, 0, 65536 });
        TensionUpDown.ValueChanged += TensionUpDown_ValueChanged;
        // 
        // TensionTrackBar
        // 
        TensionTrackBar.Location = new System.Drawing.Point(170, 50);
        TensionTrackBar.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        TensionTrackBar.Maximum = 100;
        TensionTrackBar.Name = "TensionTrackBar";
        TensionTrackBar.Size = new System.Drawing.Size(246, 20);
        TensionTrackBar.TabIndex = 16;
        TensionTrackBar.Value = 50;
        TensionTrackBar.Scroll += TensionTrackBar_Scroll;
        // 
        // LSystemForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1253, 868);
        Controls.Add(MainSplitContainer);
        Controls.Add(picOut);
        Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
        Name = "LSystemForm";
        Text = "L-System Test Form";
        Resize += MainForm_Resize;
        ((System.ComponentModel.ISupportInitialize)picOut).EndInit();
        PrintingGroupBox.ResumeLayout(false);
        MainSplitContainer.Panel1.ResumeLayout(false);
        MainSplitContainer.Panel2.ResumeLayout(false);
        MainSplitContainer.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)MainSplitContainer).EndInit();
        MainSplitContainer.ResumeLayout(false);
        groupBox1.ResumeLayout(false);
        tableLayoutPanel3.ResumeLayout(false);
        tableLayoutPanel3.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)DepthUpDown).EndInit();
        SmoothingGroupBox.ResumeLayout(false);
        tableLayoutPanel2.ResumeLayout(false);
        tableLayoutPanel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)IterationsUpDown).EndInit();
        ((System.ComponentModel.ISupportInitialize)IterationsTrackBar).EndInit();
        ((System.ComponentModel.ISupportInitialize)TensionUpDown).EndInit();
        ((System.ComponentModel.ISupportInitialize)TensionTrackBar).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.PictureBox picOut;
    private System.Windows.Forms.Button PrintButton;
    private System.Windows.Forms.GroupBox PrintingGroupBox;
    private System.Windows.Forms.ComboBox PrintersList;
    private System.Windows.Forms.SplitContainer MainSplitContainer;
    private System.Windows.Forms.GroupBox SmoothingGroupBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.ComboBox SmoothingMethodsComboBox;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.NumericUpDown IterationsUpDown;
    private System.Windows.Forms.TrackBar IterationsTrackBar;
    private System.Windows.Forms.NumericUpDown TensionUpDown;
    private System.Windows.Forms.TrackBar TensionTrackBar;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    private System.Windows.Forms.ComboBox LSystemsComboBox;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.TextBox CommentTextBox;
    private System.Windows.Forms.TextBox RulesTextBox;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox AxiomTextBox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox AngleTextBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.NumericUpDown DepthUpDown;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox SourcesComboBox;
    private System.Windows.Forms.CheckBox ForceMonochromeCheckBox;
    private System.Windows.Forms.ComboBox ColorsComboBox;
    private System.Windows.Forms.Label label5;
}
