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
        private List<WordPosition> m_WordPositionList;
        private readonly BonzaView view;
        private readonly BonzaViewModel viewModel;

        public Selection(BonzaView view, BonzaViewModel viewModel)
        {
            this.view = view;
            this.viewModel = viewModel;
        }


        public ReadOnlyCollection<WordPosition> WordPositionList => m_WordPositionList?.AsReadOnly();

        public void Clear()
        {
            if (m_WordPositionList == null)
                return;

            // Before clearing selection, remove highlight
            m_WordPositionList?.Select(wp => view.Map.GetWordCanvasFromWordPosition(wp)).ForEach(ca => ca.SetColor(App.NormalForegroundBrush, App.NormalBackgroundBrush));

            // Optimization, by convention null means no selection
            m_WordPositionList = null;

            viewModel.SelectedWordCount = 0;
        }

        public void Add(WordPosition wp)
        {
            if (m_WordPositionList == null)
                m_WordPositionList = new List<WordPosition>();

            if (!m_WordPositionList.Contains(wp))
            {
                m_WordPositionList.Add(wp);
                WordCanvas wc = view.Map.GetWordCanvasFromWordPosition(wp);
                wc.SetColor(App.SelectedForegroundBrush, App.SelectedBackgroundBrush);

                viewModel.SelectedWordCount++;
            }
        }

        internal void SwapOrientation()
        {
            if (m_WordPositionList == null)
                return;

            // ToDo: Make sure that caller check that position is valid (recycle snail re-placement pattern)
            // ToDo: Caller must support undo
            // ToDo: Redraw background grid if needed (done at the end of position check/snail re-placement?)

            // wc.SwapOrientation only relocate visually letters and update 
            foreach (WordPosition wp in m_WordPositionList)
            {
                WordCanvas wc = view.Map.GetWordCanvasFromWordPosition(wp);
                wp.IsVertical = !wp.IsVertical;
                wc.RebuildCanvasAfterOrientationSwap();
            }
        }
    }

}
