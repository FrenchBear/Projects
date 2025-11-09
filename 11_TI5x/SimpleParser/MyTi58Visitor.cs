using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace SimpleParser;

// Inherit from the generated base visitor.
// The 'object' type means your visit methods can return any type.
public class MyTi58Visitor : ti58BaseVisitor<object>
{
    // This method is called for every single token in the tree.
    // It's a good way to see the visitor in action.
    public override object VisitTerminal(ITerminalNode node)
    {
        System.Console.WriteLine($"  -> Visiting terminal: {node.GetText()}");
        return base.VisitTerminal(node);
    }

    public override object VisitMemory_or_flag_instruction([NotNull] ti58Parser.Memory_or_flag_instructionContext context)
    {
        var inst = context.children[0];

        return base.VisitMemory_or_flag_instruction(context);
    }

    // --- THIS IS WHERE YOU ADD YOUR LOGIC ---
    //
    // You need to override the 'Visit' methods for the rules from
    // your 'ti58.g4' grammar file that you care about.
    //
    // For example, if your grammar had a rule like:
    //
    //     assignment : ID '=' NUMBER;
    //
    // You would add a method like this:
    //
    // public override object VisitAssignment(ti58Parser.AssignmentContext context)
    // {
    //     string varName = context.ID().GetText();
    //     string varValue = context.NUMBER().GetText();
    //
    //     Console.WriteLine($"Found assignment: {varName} = {varValue}");
    //
    //     // You must visit the children if you want the visitor to continue
    //     // walking down the tree from this node.
    //     return base.VisitAssignment(context);
    // }
}