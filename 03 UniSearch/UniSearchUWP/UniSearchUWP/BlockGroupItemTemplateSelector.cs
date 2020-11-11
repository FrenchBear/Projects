// UniSearchUWP
// Unicode Character Search Tool, UWP version
// Dynamically select DataTemplate for TreeView item
//
// 2018-09-18   PV
// 2020-11-11   PV      nullable enable


using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable


namespace UniSearchUWPNS
{
    public class BlockGroupItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? BlockTemplate { get; set; } 
        public DataTemplate? GroupL1Template { get; set; }
        public DataTemplate? GroupTemplate { get; set; }

        // Used by binding
        protected override DataTemplate? SelectTemplateCore(object item)
        {
            if (item is BlockNode blockItem)
            {
                if (blockItem.Level == 0) return BlockTemplate;
                if (blockItem.Level == 1) return GroupL1Template;
            }
            return GroupTemplate;
        }
    }


}
