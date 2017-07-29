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

        internal void NewGrille()
        {
            grille = new Grille();
            viewModel.ClearLayout();
        }

        internal void LoadGrille(string wordsFile)
        {
            grille = new Grille();
            viewModel.ClearLayout();
            int i;
            for (i = 0; i < 5 && !grille.PlaceWords(wordsFile); i++) { }
            if (i == 5)
                throw new BonzaException("Impossible de placer les mots malgré 5 tentatives");
            viewModel.InitialLayoutDisplay();
        }

        internal void ResetLayout()
        {
            viewModel.ClearLayout();
            int i;
            for (i = 0; i < 5 && !grille.PlaceWordsAgain(); i++) { }
            if (i == 5)
                throw new BonzaException("Impossible de générer un nouveau layout malgré 5 tentatives");
            viewModel.InitialLayoutDisplay();
        }



        // Build a copy of Layout without a specific WordPosition to validate placement
        internal void BuildMoveTestLayout(IEnumerable<WordPosition> movedWordPositionList)
        {
            MoveTestLayout = new WordPositionLayout();
            foreach (var wp in Layout.WordPositionList)
                if (!movedWordPositionList.Contains(wp))
                    MoveTestLayout.AddWordPositionAndSquares(wp);
        }

        internal bool CanPlaceWordInMoveTestLayout(WordPosition hitWordPosition, PositionOrientation position)
        {
            WordPosition testWordPosition = new WordPosition
            {
                Word = hitWordPosition.Word,
                IsVertical = position.IsVertical,
                StartRow = position.StartRow,
                StartColumn = position.StartColumn
            };
            return MoveTestLayout.CanPlaceWord(testWordPosition);
        }


        // Update a word in current Layout
        internal void UpdateWordPositionLocation(WordPosition word, PositionOrientation po)
        {
            word.StartRow = po.StartRow;
            word.StartColumn = po.StartColumn;
            if (word.IsVertical = po.IsVertical)
            {
                // ToDo: Need to have a word redraw if orientation changed!!!
            }

            // Need to recompute number of words not connected
            viewModel.WordsNotConnected = Layout.GetWordsNotConnected();
        }


    }
}
