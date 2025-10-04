// BlockNode class
// Element of BlocsRecords TreeView
//
// 2018-12-09   PV
// 2020-11-11   PV      nullable enable
// 2024-09-27   PV      WinUI3 version

using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using UniDataWinUI3;

namespace UniSearchWinUI3;

internal sealed class BlockNode: TreeViewNode
{
    public BlockNode(string name, int level, BlockRecord? block = null)
    {
        Level = level;
        Block = block;
        Content = name;
        IsExpanded = true;
    }

    public int Level { get; set; }
    public string Name => (string)Content;
    public BlockRecord? Block { get; set; }

    public IEnumerable<BlockNode> BlockNodeChildren => Children.Cast<BlockNode>();
}
