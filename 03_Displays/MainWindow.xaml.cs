namespace Displays;

public partial class MainWindow: Window
{
    public MainWindow() => InitializeComponent();

    private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox { Text: { Length: > 0 } } textBox)
        {
            MyDotMatrix.Digit = textBox.Text[0];
        }
        else
        {
            MyDotMatrix.Digit = ' ';
        }
    }
}
