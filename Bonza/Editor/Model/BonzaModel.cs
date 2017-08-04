// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM Model
// 2017-07-22   PV  First version


using System.Collections.Generic;
using System.Linq;
using Bonza.Editor.ViewModel;
using Bonza.Generator;

namespace Bonza.Editor.Model
{
    internal class BonzaModel
    {
        private readonly BonzaViewModel viewModel;
        private Grille grille;
        public WordPositionLayout Layout => grille?.Layout;

        public BonzaModel(BonzaViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        internal void NewGrille()
        {
            grille = new Grille();
            grille.NewLayout();
        }

        internal void LoadGrille(string wordsFile)
        {
            grille = new Grille();
            grille.NewLayout();
            viewModel.ClearLayout();
            int i;
            for (i = 0; i < 5 && !grille.PlaceWords(wordsFile); i++) { }
            if (i == 5)
                throw new BonzaException("Impossible de placer les mots malgré 5 tentatives");
            viewModel.InitialLayoutDisplay();
        }

        internal void ResetLayout()
        {
            grille.NewLayout();
            viewModel.ClearLayout();
            int i;
            for (i = 0; i < 5 && !grille.PlaceWordsAgain(); i++) { }
            if (i == 5)
                throw new BonzaException("Impossible de générer un nouveau layout malgré 5 tentatives");
            viewModel.InitialLayoutDisplay();
        }



        // Build a copy of Layout without a specific list of WordPosition to validate placement
        internal WordPositionLayout GetLayoutExcludingWordPositionList(IEnumerable<WordPosition> movedWordPositionList)
        {
            var layout = new WordPositionLayout();
            foreach (var wp in Layout.WordPositionList)
                if (!movedWordPositionList.Contains(wp))
                    layout.AddWordPositionAndSquaresNoCheck(wp);
            return layout;
        }


        internal PlaceWordStatus CanPlaceWordAtPositionInLayout(WordPositionLayout layout, WordPosition hitWordPosition, PositionOrientation position)
        {
            WordPosition testWordPosition = new WordPosition
            {
                Word = hitWordPosition.Word,
                IsVertical = position.IsVertical,
                StartRow = position.StartRow,
                StartColumn = position.StartColumn
            };
            return layout.CanPlaceWord(testWordPosition);
        }


        // Removes a word from current Layout
        internal void RemoveWordPosition(WordPosition wordPosition)
        {
            Layout.RemoveWordPositionAndSquares(wordPosition);
        }

        internal PlaceWordStatus AddWordPosition(WordPosition wordPosition)
        {
            return Layout.AddWordPositionAndSquares(wordPosition);
        }
    }
}
