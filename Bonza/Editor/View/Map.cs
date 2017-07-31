// BonzaEditor, clss BonzaView.Map
// Internal view class for better encapsulation, manages mapping between WordPosition and WordCanvas
//
// 2017-07-29   PV  First version, extracted from view

using System.Collections.Generic;
using System.Windows;
using Bonza.Generator;


namespace Bonza.Editor
{
    public partial class BonzaView : Window
    {
        // Mapping WordPositon <--> WordCanvas
        internal class Map
        {
            // HitTest selects a WordCanvas, this dictionary maps it to associated WordPosition
            private readonly Dictionary<WordCanvas, WordPosition> CanvasToWordPositionDictionary = new Dictionary<WordCanvas, WordPosition>();


            internal void Add(WordPosition wp, WordCanvas wc)
            {
                CanvasToWordPositionDictionary.Add(wc, wp);
            }

            // Helper, mapping WordCanvas -> WordPosition or null
            internal WordPosition GetWordPositionFromCanvas(WordCanvas wc)
            {
                return CanvasToWordPositionDictionary.TryGetValue(wc, out WordPosition wp) ? wp : null;
            }

            // Helper, mapping WordPosition -> WordCanvas or null
            internal WordCanvas GetCanvasFromWordPosition(WordPosition wp)
            {
                foreach (var kv in CanvasToWordPositionDictionary)
                    if (kv.Value == wp)
                        return kv.Key;
                return null;
            }
        }

    }
}
