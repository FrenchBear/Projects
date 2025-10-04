// UniView CS WPF and UWP
// TextSample record
// Basic helper class
//
// 2020-12-20   PV      Basic version for C#8 (no record!).
// 2023-01-23   PV      C#11

namespace UniView_WinUI3;

public class TextSample
{
    public string Name { get; }
    public string Text { get; }

    public TextSample(string name, string text) => (Name, Text) = (name, text);

    public override string ToString() => Name;
}