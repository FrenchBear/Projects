// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM View
// 2017-07-22   PV  First version

// ToDo: Esc cancel a move operation, clean properly
// ToDo: Let user change orientation of a word
// ToDo: Contextual menu on selection or grid
// ToDo: Delete selection command

using System.Collections.Generic;
using System.Windows;
using Bonza.Generator;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bonza.Editor
{
    public partial class BonzaView : Window
    {
        // --------------------------------------------------------------------------------------------------------
        // Current selection management

        class Selection
        {
            private List<WordPosition> m_WordPositionList;
            private readonly BonzaView view;

            public Selection(BonzaView editorWindow)
            {
                view = editorWindow;
            }


            public ReadOnlyCollection<WordPosition> WordPositionList => m_WordPositionList?.AsReadOnly();

            public void Clear()
            {
                if (m_WordPositionList == null)
                    return;

                m_WordPositionList?.Select(wp => view.map.GetCanvasFromWordPosition(wp)).ForEach(ca => SetWordCanvasColor(ca, NormalForegroundBrush, NormalBackgroundBrush));

                // Optimization, by convention null means no selection
                m_WordPositionList = null;

                view.viewModel.SelectedWordCount = 0;
            }

            // ToDo: update ViewModel SelectionCount
            public void Add(WordPosition wp)
            {
                if (m_WordPositionList == null)
                    m_WordPositionList = new List<WordPosition>();

                if (!m_WordPositionList.Contains(wp))
                {
                    m_WordPositionList.Add(wp);
                    WordCanvas ca = view.map.GetCanvasFromWordPosition(wp);
                    SetWordCanvasColor(ca, SelectedForegroundBrush, SelectedBackgroundBrush);

                    view.viewModel.SelectedWordCount++;
                }
            }
        }

    }
}
