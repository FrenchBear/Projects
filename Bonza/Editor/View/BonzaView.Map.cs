// BonzaEditor - WordPositionWordCanvasMap
// Internal view class for better encapsulation, manages mapping between WordPosition and WordCanvas
// 2017-07-29   PV  First version, extracted from view

using System.Collections.Generic;
using System.Windows;
using Bonza.Generator;


namespace Bonza.Editor
{
    public partial class BonzaView : Window
    {
        // Mapping WordPositon <--> WordCanvas
        class Map
        {
            // HitTest selects a WordCanvas, this dictionary maps it to associated WordPosition
            private readonly Dictionary<WordCanvas, WordPosition> CanvasToWordPositionDictionary = new Dictionary<WordCanvas, WordPosition>();


            public void Add(WordPosition wp, WordCanvas wc)
            {
                CanvasToWordPositionDictionary.Add(wc, wp);
            }

            // Helper, mapping WordCanvas -> WordPosition or null
            public WordPosition GetWordPositionFromCanvas(WordCanvas wc)
            {
                return CanvasToWordPositionDictionary.TryGetValue(wc, out WordPosition wp) ? wp : null;
            }

            // Helper, mapping WordPosition -> WordCanvas or null
            public WordCanvas GetCanvasFromWordPosition(WordPosition wp)
            {
                foreach (var kv in CanvasToWordPositionDictionary)
                    if (kv.Value == wp)
                        return kv.Key;
                return null;
            }
        }

    }
}
