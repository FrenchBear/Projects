// UniView CS WPF Core
// TextSample record
// A pretext to test C# records and init-only properties!
//
// 2020-12-18   PV      1st version
// 2020-12-19   PV      Positional record, more compact without "public string Name { get; init; }   public string Text { get; init; }", but forces the use of a constructor or add public TextSample(): this("","") { }

#nullable enable

namespace UniViewNS
{
    public record TextSample(string Name, string Text)
    {
        public override string ToString() => Name;
    }
}