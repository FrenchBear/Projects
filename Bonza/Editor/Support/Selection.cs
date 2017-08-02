// BonzaEditor, clss BonzaView.Selection
// Internal view class for better encapsulation, manages selection at view level
// Note: Maybe this would be better in ViewModel...
//
// 2017-07-29   PV  First version, extracted from view


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bonza.Editor.ViewModel;
using Bonza.Generator;
using Bonza.Editor.View;

namespace Bonza.Editor.Support
{

    class Selection
    {
        private List<WordAndCanvas> m_WordAndCanvasList;
        private readonly BonzaView view;
        private readonly BonzaViewModel viewModel;

        public Selection(BonzaView view, BonzaViewModel viewModel)
        {
            this.view = view;
            this.viewModel = viewModel;
        }


        // ToDo: Remove this
        //public IEnumerable<WordPosition> WordPositionList => m_WordAndCanvasList?.Select(wac => wac.WordPosition);

        public ReadOnlyCollection<WordAndCanvas> WordAndCanvasList => m_WordAndCanvasList?.AsReadOnly();


        public void Clear()
        {
            if (m_WordAndCanvasList == null)
                return;

            // Before clearing selection, remove highlight
            m_WordAndCanvasList?.ForEach(wac => wac.WordCanvas.SetColor(App.NormalForegroundBrush, App.NormalBackgroundBrush));

            // Optimization, by convention null means no selection
            m_WordAndCanvasList = null;

            viewModel.SelectedWordCount = 0;
        }

        public void Add(WordAndCanvas wac)
        {
            if (m_WordAndCanvasList == null)
                m_WordAndCanvasList = new List<WordAndCanvas>();

            if (!m_WordAndCanvasList.Contains(wac))
            {
                m_WordAndCanvasList.Add(wac);
                wac.WordCanvas.SetColor(App.SelectedForegroundBrush, App.SelectedBackgroundBrush);

                viewModel.SelectedWordCount++;
            }
        }

        internal void SwapOrientation()
        {
            if (m_WordAndCanvasList == null)
                return;

            // ToDo: Make sure that caller check that position is valid (recycle snail re-placement pattern)
            // ToDo: Caller must support undo
            // ToDo: Redraw background grid if needed (done at the end of position check/snail re-placement?)

            // wc.SwapOrientation only relocate visually letters and update 
            foreach (WordAndCanvas wac in m_WordAndCanvasList)
            {
                wac.WordPosition.IsVertical = !wac.WordPosition.IsVertical;
                wac.WordCanvas.RebuildCanvasAfterOrientationSwap();
            }
        }
    }

}
