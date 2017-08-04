// BonzaEditor, clss BonzaView.Selection
// Internal view class for better encapsulation, manages selection at view level
//
// 2017-07-29   PV  First version, extracted from view


using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bonza.Editor.ViewModel;
using System.Diagnostics;

namespace Bonza.Editor.Support
{

    internal class Selection
    {
        private List<WordAndCanvas> m_WordAndCanvasList;
        private readonly BonzaViewModel viewModel;

        public Selection(BonzaViewModel viewModel)
        {
            this.viewModel = viewModel;
        }


        public ReadOnlyCollection<WordAndCanvas> WordAndCanvasList => m_WordAndCanvasList?.AsReadOnly();


        public void Clear()
        {
            if (m_WordAndCanvasList == null)
                return;

            // Before clearing selection, remove highlight
            m_WordAndCanvasList?.ForEach(wac => wac.SetColor(App.NormalValidForeground, App.NormalValidBackground));

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
                wac.SetColor(App.SelectedValidForeground, App.SelectedValidBackground);

                viewModel.SelectedWordCount++;
            }
        }

        // Overload helper
        internal void Add(IEnumerable<WordAndCanvas> wordAndCanvasList)
        {
            foreach (WordAndCanvas wac in wordAndCanvasList)
                Add(wac);
        }


        // Swaps a single word between orizontal and vertical orientation
        internal void SwapOrientation()
        {
            Debug.Assert(m_WordAndCanvasList != null && m_WordAndCanvasList.Count == 1);

            // ToDo: Make sure that caller check that position is valid (recycle snail re-placement pattern)
            // ToDo: Caller must support undo
            // ToDo: Redraw background grid if needed (done at the end of position check/snail re-placement?)

            // wc.SwapOrientation only relocate visually letters and update 
            foreach (WordAndCanvas wac in m_WordAndCanvasList)
            {
                wac.WordPosition.IsVertical = !wac.WordPosition.IsVertical;
                wac.RebuildCanvasAfterOrientationSwap();
            }
        }

        // Delete selection
        internal void Delete(WordAndCanvas wac)
        {
            Debug.Assert(wac != null);

            if (m_WordAndCanvasList != null && m_WordAndCanvasList.Contains(wac))
            {
                m_WordAndCanvasList.Remove(wac);
                viewModel.SelectedWordCount = m_WordAndCanvasList.Count;
            }
        }

    }

}
