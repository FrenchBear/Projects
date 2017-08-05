// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// Editor Model, access to Generator class
//
// 2017-07-22   PV  First version


using System.Collections.Generic;
using System.Linq;
using Bonza.Editor.ViewModel;
using Bonza.Generator;

namespace Bonza.Editor.Model
{
    internal class EditorModel
    {
        private readonly EditorViewModel viewModel;
        private Grille grille;
        public WordPositionLayout Layout => grille?.Layout;

        public EditorModel(EditorViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        internal void NewGrille()
        {
            grille = new Grille();
        }

        internal void LoadGrille(string wordsFile)
        {
            grille = new Grille();
            viewModel.ClearLayout();
            int i;
            for (i = 0; i < 5; i++)
                if (grille.AddWordsFromFile(wordsFile)) break;
            if (i == 5)
                throw new BonzaException("Impossible de placer les mots malgré 5 tentatives");
            viewModel.AddCanvasForWordPositionList(Layout.WordPositionList);
        }

        internal void ResetLayout()
        {
            grille.NewLayout();
            viewModel.ClearLayout();
            int i;
            for (i = 0; i < 5 && !grille.PlaceWordsAgain(); i++) { }
            if (i == 5)
                throw new BonzaException("Impossible de générer un nouveau layout malgré 5 tentatives");
            viewModel.AddCanvasForWordPositionList(Layout.WordPositionList);
        }


        // Build a copy of Layout without a specific list of WordPosition to validate placement
        internal WordPositionLayout GetLayoutExcludingWordPositionList(IEnumerable<WordPosition> movedWordPositionList)
        {
            var layout = new WordPositionLayout();
            foreach (var wp in Layout.WordPositionList)
                // ReSharper disable once PossibleMultipleEnumeration
                if (!movedWordPositionList.Contains(wp))
                    layout.AddWordPositionAndSquaresNoCheck(wp);
            return layout;
        }


        // Check if a WordPosition can be placed at specific location in given layout
        internal PlaceWordStatus CanPlaceWordAtPositionInLayout(WordPositionLayout layout, WordPosition wordPosition, PositionOrientation position)
        {
            WordPosition testWordPosition = new WordPosition(wordPosition.Word, wordPosition.OriginalWord, position);
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

        public string CheckWordsList(List<string> wordsList)
        {
            return grille.CheckWordsList(wordsList);
        }

        public List<WordPosition> AddWordsList(List<string> wordsList)
        {
            return grille.AddWordsList(wordsList);
        }
    }
}
