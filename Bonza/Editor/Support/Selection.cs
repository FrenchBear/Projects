// BonzaEditor, class EditorView.Selection
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
        private readonly EditorViewModel viewModel;

        public Selection(EditorViewModel viewModel)
        {
            this.viewModel = viewModel;
        }


        public ReadOnlyCollection<WordAndCanvas> WordAndCanvasList => m_WordAndCanvasList?.AsReadOnly();


        public void Clear()
        {
            if (m_WordAndCanvasList == null)
                return;

            // Remove highlight
            var sauveHighlight = m_WordAndCanvasList;
            m_WordAndCanvasList = null;
            viewModel.RecolorizeWordAndCanvasList(sauveHighlight);

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
