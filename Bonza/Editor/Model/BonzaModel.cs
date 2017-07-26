// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM Model
// 2017-07-22   PV  First version

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonza.Generator;


namespace Bonza.Editor
{
    class BonzaModel
    {
        private BonzaViewModel viewModel;
        private Grille grille;
        public WordPositionLayout Layout => grille?.GetLayout();
        public WordPositionLayout MoveTestLayout;

        public void SetViewModel(BonzaViewModel vm)
        {
            viewModel = vm;
        }



        internal void LoadGrille(string wordsFile)
        {
            grille = new Grille();
            for (; !grille.PlaceWords(wordsFile);)
            {
            }
            viewModel.NewLayout();
        }

        internal void ResetLayout()
        {
            grille.PlaceWordsAgain();
            viewModel.NewLayout();
        }



        // Build a copy of Layout without a specific WordPosition to validate placement
        internal void BuildMoveTestLayout(List<WordPosition> movedWordPositionList)
        {
            MoveTestLayout = new WordPositionLayout();
            foreach (var wp in Layout.WordPositionList)
                if (!movedWordPositionList.Contains(wp))
                    MoveTestLayout.AddWordPositionAndSquares(wp);
        }

        internal bool CanPlaceWordInMoveTestLayout(WordPosition hitWordPosition, (int left, int top) position)
        {
            WordPosition testWordPosition = new WordPosition
            {
                Word = hitWordPosition.Word,
                IsVertical = hitWordPosition.IsVertical,
                StartColumn = position.left,
                StartRow = position.top
            };
            return MoveTestLayout.CanPlaceWord(testWordPosition);
        }


        // Update a word in current Layout
        internal void UpdateWordPositionLocation(WordPosition word, int column, int row)
        {
            word.StartColumn = column;
            word.StartRow = row;

            // Need to recompute number of words not connected
            viewModel.WordsNotConnected = Layout.GetWordsNotConnected();
        }

    }
}
