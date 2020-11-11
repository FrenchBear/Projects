// BlockNode class
// Element of BlocsRecords TreeView
//
// 2018-12-09   PV
// 2020-11-11   PV      nullable enable

using System.Collections.Generic;
using System.Linq;
using UniDataNS;
using Windows.UI.Xaml.Controls;

#nullable enable


namespace UniSearchUWPNS
{
    internal class BlockNode : TreeViewNode
    {
        public BlockNode(string name, int level, BlockRecord? block = null)
        {
            Name = name;
            Level = level;
            Block = block;
            Content = name;
            IsExpanded = true;
        }

        public int Level { get; set; }
        public string Name { get; set; }
        public BlockRecord? Block { get; set; }


        public IEnumerable<BlockNode> BlockNodeChildren => Children.Cast<BlockNode>();
    }
}
