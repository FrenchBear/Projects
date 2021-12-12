﻿namespace LSystemTest;

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
            this.picOut = new System.Windows.Forms.PictureBox();
            this.PrintButton = new System.Windows.Forms.Button();
            this.PrintingGroupBox = new System.Windows.Forms.GroupBox();
            this.PrintersList = new System.Windows.Forms.ComboBox();
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.RulesTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.AxiomTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.AngleTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.CommentTextBox = new System.Windows.Forms.TextBox();
            this.LSystemsComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.DrawLSystemButton = new System.Windows.Forms.Button();
            this.DrawTestButton = new System.Windows.Forms.Button();
            this.SmoothingGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.SmoothingMethodsComboBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.IterationsUpDown = new System.Windows.Forms.NumericUpDown();
            this.IterationsTrackBar = new System.Windows.Forms.TrackBar();
            this.TensionUpDown = new System.Windows.Forms.NumericUpDown();
            this.TensionTrackBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.DepthUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.picOut)).BeginInit();
            this.PrintingGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SmoothingGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IterationsUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IterationsTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TensionUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TensionTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DepthUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // picOut
            // 
            this.picOut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picOut.Location = new System.Drawing.Point(0, 398);
            this.picOut.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.picOut.Name = "picOut";
            this.picOut.Size = new System.Drawing.Size(1924, 1198);
            this.picOut.TabIndex = 4;
            this.picOut.TabStop = false;
            // 
            // PrintButton
            // 
            this.PrintButton.Location = new System.Drawing.Point(6, 69);
            this.PrintButton.Name = "PrintButton";
            this.PrintButton.Size = new System.Drawing.Size(150, 33);
            this.PrintButton.TabIndex = 6;
            this.PrintButton.Text = "&Print";
            this.PrintButton.UseVisualStyleBackColor = true;
            this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
            // 
            // PrintingGroupBox
            // 
            this.PrintingGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PrintingGroupBox.Controls.Add(this.PrintersList);
            this.PrintingGroupBox.Controls.Add(this.PrintButton);
            this.PrintingGroupBox.Location = new System.Drawing.Point(0, 177);
            this.PrintingGroupBox.Name = "PrintingGroupBox";
            this.PrintingGroupBox.Size = new System.Drawing.Size(758, 114);
            this.PrintingGroupBox.TabIndex = 9;
            this.PrintingGroupBox.TabStop = false;
            this.PrintingGroupBox.Text = "Printing";
            // 
            // PrintersList
            // 
            this.PrintersList.AllowDrop = true;
            this.PrintersList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PrintersList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PrintersList.FormattingEnabled = true;
            this.PrintersList.Location = new System.Drawing.Point(6, 30);
            this.PrintersList.Name = "PrintersList";
            this.PrintersList.Size = new System.Drawing.Size(749, 33);
            this.PrintersList.TabIndex = 8;
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Cursor = System.Windows.Forms.Cursors.Default;
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.groupBox1);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.DepthUpDown);
            this.MainSplitContainer.Panel2.Controls.Add(this.label1);
            this.MainSplitContainer.Panel2.Controls.Add(this.DrawLSystemButton);
            this.MainSplitContainer.Panel2.Controls.Add(this.DrawTestButton);
            this.MainSplitContainer.Panel2.Controls.Add(this.SmoothingGroupBox);
            this.MainSplitContainer.Panel2.Controls.Add(this.PrintingGroupBox);
            this.MainSplitContainer.Size = new System.Drawing.Size(1929, 399);
            this.MainSplitContainer.SplitterDistance = 933;
            this.MainSplitContainer.TabIndex = 11;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(933, 380);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Smoothing";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel3.Controls.Add(this.RulesTextBox, 1, 5);
            this.tableLayoutPanel3.Controls.Add(this.label6, 0, 5);
            this.tableLayoutPanel3.Controls.Add(this.AxiomTextBox, 1, 4);
            this.tableLayoutPanel3.Controls.Add(this.label4, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.AngleTextBox, 1, 3);
            this.tableLayoutPanel3.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.NameTextBox, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.CommentTextBox, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.LSystemsComboBox, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label10, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.label11, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label12, 0, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 30);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 6;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(921, 350);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // RulesTextBox
            // 
            this.RulesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RulesTextBox.Location = new System.Drawing.Point(123, 263);
            this.RulesTextBox.Multiline = true;
            this.RulesTextBox.Name = "RulesTextBox";
            this.RulesTextBox.Size = new System.Drawing.Size(795, 84);
            this.RulesTextBox.TabIndex = 23;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 292);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 25);
            this.label6.TabIndex = 22;
            this.label6.Text = "Rules";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AxiomTextBox
            // 
            this.AxiomTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.AxiomTextBox.Location = new System.Drawing.Point(123, 224);
            this.AxiomTextBox.Name = "AxiomTextBox";
            this.AxiomTextBox.Size = new System.Drawing.Size(795, 31);
            this.AxiomTextBox.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 227);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 25);
            this.label4.TabIndex = 20;
            this.label4.Text = "Axiom";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AngleTextBox
            // 
            this.AngleTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.AngleTextBox.Location = new System.Drawing.Point(123, 184);
            this.AngleTextBox.Name = "AngleTextBox";
            this.AngleTextBox.Size = new System.Drawing.Size(94, 31);
            this.AngleTextBox.TabIndex = 19;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 187);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 25);
            this.label3.TabIndex = 18;
            this.label3.Text = "Angle";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NameTextBox
            // 
            this.NameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.NameTextBox.Location = new System.Drawing.Point(123, 144);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(795, 31);
            this.NameTextBox.TabIndex = 17;
            // 
            // CommentTextBox
            // 
            this.CommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommentTextBox.Location = new System.Drawing.Point(123, 43);
            this.CommentTextBox.Multiline = true;
            this.CommentTextBox.Name = "CommentTextBox";
            this.CommentTextBox.Size = new System.Drawing.Size(795, 94);
            this.CommentTextBox.TabIndex = 16;
            // 
            // LSystemsComboBox
            // 
            this.LSystemsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.LSystemsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LSystemsComboBox.FormattingEnabled = true;
            this.LSystemsComboBox.Location = new System.Drawing.Point(123, 3);
            this.LSystemsComboBox.Name = "LSystemsComboBox";
            this.LSystemsComboBox.Size = new System.Drawing.Size(795, 33);
            this.LSystemsComboBox.TabIndex = 12;
            this.LSystemsComboBox.SelectedIndexChanged += new System.EventHandler(this.LSystemsComboBox_SelectedIndexChanged);
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 147);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 25);
            this.label10.TabIndex = 11;
            this.label10.Text = "Name";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 77);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(91, 25);
            this.label11.TabIndex = 10;
            this.label11.Text = "Comment";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 7);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(97, 25);
            this.label12.TabIndex = 9;
            this.label12.Text = "Predefined";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DrawLSystemButton
            // 
            this.DrawLSystemButton.Location = new System.Drawing.Point(340, 317);
            this.DrawLSystemButton.Name = "DrawLSystemButton";
            this.DrawLSystemButton.Size = new System.Drawing.Size(150, 34);
            this.DrawLSystemButton.TabIndex = 13;
            this.DrawLSystemButton.Text = "Draw LSystem";
            this.DrawLSystemButton.UseVisualStyleBackColor = true;
            this.DrawLSystemButton.Click += new System.EventHandler(this.DrawLSystemButton_Click);
            // 
            // DrawTestButton
            // 
            this.DrawTestButton.Location = new System.Drawing.Point(6, 319);
            this.DrawTestButton.Name = "DrawTestButton";
            this.DrawTestButton.Size = new System.Drawing.Size(150, 34);
            this.DrawTestButton.TabIndex = 12;
            this.DrawTestButton.Text = "Draw Test";
            this.DrawTestButton.UseVisualStyleBackColor = true;
            this.DrawTestButton.Click += new System.EventHandler(this.DrawTestButton_Click);
            // 
            // SmoothingGroupBox
            // 
            this.SmoothingGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SmoothingGroupBox.Controls.Add(this.tableLayoutPanel2);
            this.SmoothingGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.SmoothingGroupBox.Location = new System.Drawing.Point(0, 0);
            this.SmoothingGroupBox.Name = "SmoothingGroupBox";
            this.SmoothingGroupBox.Size = new System.Drawing.Size(992, 167);
            this.SmoothingGroupBox.TabIndex = 11;
            this.SmoothingGroupBox.TabStop = false;
            this.SmoothingGroupBox.Text = "Smoothing";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel2.Controls.Add(this.SmoothingMethodsComboBox, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label9, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label8, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.IterationsUpDown, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.IterationsTrackBar, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.TensionUpDown, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.TensionTrackBar, 2, 2);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 30);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(980, 121);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // SmoothingMethodsComboBox
            // 
            this.SmoothingMethodsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.SetColumnSpan(this.SmoothingMethodsComboBox, 2);
            this.SmoothingMethodsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SmoothingMethodsComboBox.FormattingEnabled = true;
            this.SmoothingMethodsComboBox.Location = new System.Drawing.Point(123, 3);
            this.SmoothingMethodsComboBox.Name = "SmoothingMethodsComboBox";
            this.SmoothingMethodsComboBox.Size = new System.Drawing.Size(854, 33);
            this.SmoothingMethodsComboBox.TabIndex = 12;
            this.SmoothingMethodsComboBox.SelectedIndexChanged += new System.EventHandler(this.SmoothingMethodsComboBox_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 87);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(71, 25);
            this.label9.TabIndex = 11;
            this.label9.Text = "Tension";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 47);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(86, 25);
            this.label8.TabIndex = 10;
            this.label8.Text = "Iterations";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(75, 25);
            this.label7.TabIndex = 9;
            this.label7.Text = "Method";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // IterationsUpDown
            // 
            this.IterationsUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.IterationsUpDown.Location = new System.Drawing.Point(123, 44);
            this.IterationsUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.IterationsUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.IterationsUpDown.Name = "IterationsUpDown";
            this.IterationsUpDown.Size = new System.Drawing.Size(97, 31);
            this.IterationsUpDown.TabIndex = 13;
            this.IterationsUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.IterationsUpDown.ValueChanged += new System.EventHandler(this.IterationsUpDown_ValueChanged);
            // 
            // IterationsTrackBar
            // 
            this.IterationsTrackBar.Location = new System.Drawing.Point(243, 43);
            this.IterationsTrackBar.Minimum = 1;
            this.IterationsTrackBar.Name = "IterationsTrackBar";
            this.IterationsTrackBar.Size = new System.Drawing.Size(352, 34);
            this.IterationsTrackBar.TabIndex = 14;
            this.IterationsTrackBar.Value = 2;
            this.IterationsTrackBar.Scroll += new System.EventHandler(this.IterationsTrackBar_Scroll);
            // 
            // TensionUpDown
            // 
            this.TensionUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.TensionUpDown.DecimalPlaces = 2;
            this.TensionUpDown.Location = new System.Drawing.Point(123, 84);
            this.TensionUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.TensionUpDown.Name = "TensionUpDown";
            this.TensionUpDown.Size = new System.Drawing.Size(97, 31);
            this.TensionUpDown.TabIndex = 15;
            this.TensionUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.TensionUpDown.ValueChanged += new System.EventHandler(this.TensionUpDown_ValueChanged);
            // 
            // TensionTrackBar
            // 
            this.TensionTrackBar.Location = new System.Drawing.Point(243, 83);
            this.TensionTrackBar.Maximum = 100;
            this.TensionTrackBar.Name = "TensionTrackBar";
            this.TensionTrackBar.Size = new System.Drawing.Size(352, 34);
            this.TensionTrackBar.TabIndex = 16;
            this.TensionTrackBar.Value = 50;
            this.TensionTrackBar.Scroll += new System.EventHandler(this.TensionTrackBar_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(513, 322);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 25);
            this.label1.TabIndex = 14;
            this.label1.Text = "Depth";
            // 
            // DepthUpDown
            // 
            this.DepthUpDown.Location = new System.Drawing.Point(580, 320);
            this.DepthUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.DepthUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.DepthUpDown.Name = "DepthUpDown";
            this.DepthUpDown.Size = new System.Drawing.Size(100, 31);
            this.DepthUpDown.TabIndex = 15;
            this.DepthUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // LSystemForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1929, 1611);
            this.Controls.Add(this.MainSplitContainer);
            this.Controls.Add(this.picOut);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "LSystemForm";
            this.Text = "L-System Test Form";
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picOut)).EndInit();
            this.PrintingGroupBox.ResumeLayout(false);
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            this.MainSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.SmoothingGroupBox.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IterationsUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IterationsTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TensionUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TensionTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DepthUpDown)).EndInit();
            this.ResumeLayout(false);

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
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.TextBox CommentTextBox;
    private System.Windows.Forms.TextBox RulesTextBox;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox AxiomTextBox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox AngleTextBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox NameTextBox;
    private System.Windows.Forms.Button DrawLSystemButton;
    private System.Windows.Forms.Button DrawTestButton;
    private System.Windows.Forms.NumericUpDown DepthUpDown;
    private System.Windows.Forms.Label label1;
}
