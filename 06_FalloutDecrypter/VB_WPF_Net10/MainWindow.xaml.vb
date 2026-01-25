' 325 VB WPF Fallout 3 Decrypter
'
' 2012-02-25	PV      VS2010
' 2021-09-22    PV      VS2022; Net6
' 2023-01-10	PV		Net7; Much better interface using a grid; Partial check
' 2023-11-18	PV		Net8
' 2024-11-15	PV		Net9
' 2026-01-19	PV		Net10
' 2026-01-25	PV		Moved to Projects

Option Explicit On
Option Compare Text

Partial Public Class MainWindow

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub AddButton_Click(sender As Object, e As RoutedEventArgs) Handles AddButton.Click
        If WordTextBox.Text <> "" And IsNumeric(PlacedTextBox.Text) Then
            If Word1TextBox.Text = "" Then
                Word1TextBox.Text = WordTextBox.Text
                Placed1TextBox.Text = PlacedTextBox.Text
            Else
                If WordTextBox.Text.Length <> Word1TextBox.Text.Length Then
                    MsgBox("Length mismatch", MsgBoxStyle.Exclamation)
                    Exit Sub
                End If
                If Word2TextBox.Text = "" Then
                    Word2TextBox.Text = WordTextBox.Text
                    Placed2TextBox.Text = PlacedTextBox.Text
                ElseIf Word3TextBox.Text = "" Then
                    Word3TextBox.Text = WordTextBox.Text
                    Placed3TextBox.Text = PlacedTextBox.Text
                ElseIf Word4TextBox.Text = "" Then
                    Word4TextBox.Text = WordTextBox.Text
                    Placed4TextBox.Text = PlacedTextBox.Text
                Else
                    MsgBox("No more choices", MsgBoxStyle.Exclamation)
                    Exit Sub
                End If
            End If
            WordTextBox.Text = ""
            PlacedTextBox.Text = ""
            WordTextBox.Focus()
        End If
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ClearAll()
    End Sub

    Private Sub ClearButton_Click(sender As Object, e As RoutedEventArgs) Handles ClearButton.Click
        ClearAll()
    End Sub

    Private Sub ClearAll()
        WordTextBox.Text = ""
        Word1TextBox.Text = ""
        Word2TextBox.Text = ""
        Word3TextBox.Text = ""
        Word4TextBox.Text = ""
        PlacedTextBox.Text = ""
        Placed1TextBox.Text = ""
        Placed2TextBox.Text = ""
        Placed3TextBox.Text = ""
        Placed4TextBox.Text = ""
        ClearStatuses()
        WordTextBox.Focus()
    End Sub

    Private Sub ClearStatuses()
        Status1Label.Content = ""
        Status2Label.Content = ""
        Status3Label.Content = ""
        Status4Label.Content = ""
    End Sub

    Private Sub WordTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles WordTextBox.TextChanged
        If WordTextBox.Text = "" Then
            AnalysisLabel.Content = ""
            ClearStatuses()
        ElseIf Placed1TextBox.Text = "" Then
            AnalysisLabel.Content = "Enter first word"
            ClearStatuses()
        ElseIf WordTextBox.Text.Length <> Word1TextBox.Text.Length Then
            AnalysisLabel.Content = "Enter next word"
            CompareWord(WordTextBox.Text, Word1TextBox.Text, Val(Placed1TextBox.Text), Status1Label)
            If Placed2TextBox.Text <> "" Then CompareWord(WordTextBox.Text, Word2TextBox.Text, Val(Placed2TextBox.Text), Status2Label)
            If Placed3TextBox.Text <> "" Then CompareWord(WordTextBox.Text, Word3TextBox.Text, Val(Placed3TextBox.Text), Status3Label)
            If Placed4TextBox.Text <> "" Then CompareWord(WordTextBox.Text, Word4TextBox.Text, Val(Placed4TextBox.Text), Status4Label)
        Else
            CompareWord(WordTextBox.Text, Word1TextBox.Text, Val(Placed1TextBox.Text), Status1Label)
            If Placed2TextBox.Text <> "" Then CompareWord(WordTextBox.Text, Word2TextBox.Text, Val(Placed2TextBox.Text), Status2Label)
            If Placed3TextBox.Text <> "" Then CompareWord(WordTextBox.Text, Word3TextBox.Text, Val(Placed3TextBox.Text), Status3Label)
            If Placed4TextBox.Text <> "" Then CompareWord(WordTextBox.Text, Word4TextBox.Text, Val(Placed4TextBox.Text), Status4Label)
            If Status1Label.Content = "OK" And (Status2Label.Content = "" Or Status2Label.Content = "OK") And (Status3Label.Content = "" Or Status3Label.Content = "OK") And (Status4Label.Content = "" Or Status4Label.Content = "OK") Then
                AnalysisLabel.Content = "Possible"
                PlacedTextBox.Focus()
            Else
                AnalysisLabel.Content = "Nope"
            End If
            WordTextBox.SelectAll()
        End If
    End Sub

    Private Shared Sub CompareWord(Word As String, Guessed As String, Placed As Integer, lblStatus As Label)
        Dim bPartialCheck = Word.Length <> Guessed.Length

        Dim Match As Integer = 0
        For i As Integer = 0 To Word.Length - 1
            If Word.Chars(i) = Guessed.Chars(i) Then Match += 1
        Next
        If bPartialCheck Then
            If Match > Placed Then
                lblStatus.Content = "NO"
                lblStatus.Foreground = Brushes.Red
            Else
                lblStatus.Content = ""
            End If
        Else
            If Match = Placed Then
                lblStatus.Content = "OK"
                lblStatus.Foreground = Brushes.DarkGreen
            Else
                lblStatus.Content = "NO"
                lblStatus.Foreground = Brushes.Red
            End If
        End If
    End Sub

End Class
