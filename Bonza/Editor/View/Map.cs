// BonzaEditor, clss BonzaView.MapClass
// Internal view class for better encapsulation, manages mapping between WordPosition and WordCanvas
//
// 2017-07-29   PV  First version, extracted from view

// ToDo: Replace this class by a combination of WordPosition and WordCanvas


using System.Collections.Generic;
using System.Windows;
using Bonza.Generator;
using Bonza.Editor.Support;

namespace Bonza.Editor.View
{
    public partial class BonzaView : Window
    {
        // Mapping WordPositon <--> WordCanvas
        internal class MapClass
        {
            // HitTest selects a WordCanvas, this dictionary maps it to associated WordPosition
            private readonly Dictionary<WordCanvas, WordPosition> canvasToWordPositionDictionary = new Dictionary<WordCanvas, WordPosition>();


            internal void Add(WordPosition wp, WordCanvas wc)
            {
                canvasToWordPositionDictionary.Add(wc, wp);
            }

            // Helper, mapping WordCanvas -> WordPosition or null
            internal WordPosition GetWordPositionFromWordCanvas(WordCanvas wc)
            {
                return canvasToWordPositionDictionary.TryGetValue(wc, out WordPosition wp) ? wp : null;
            }

            // Helper, mapping WordPosition -> WordCanvas or null
            internal WordCanvas GetWordCanvasFromWordPosition(WordPosition wp)
            {
                foreach (var kv in canvasToWordPositionDictionary)
                    if (kv.Value == wp)
                        return kv.Key;
                return null;
            }
        }

    }
}
