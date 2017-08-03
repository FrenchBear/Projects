// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// MVVM Model
// 2017-07-22   PV  First version


using System;
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
        public WordPositionLayout Layout => grille?.GetLayout();
        public WordPositionLayout MoveTestLayout;

        public BonzaModel(BonzaViewModel viewModel)
        {
            this.viewModel = viewModel;
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


        //// Update a word in current Layout
        //internal void UpdateWordPositionLocation(WordPosition word, PositionOrientation po)
        //{
        //    Layout.UpdateWordPositionLocation(word, po);

        //    // Need to recompute number of words not connected
        //    viewModel.WordsNotConnected = Layout.GetWordsNotConnected();
        //}

        // Removes a word from current Layout
        internal void RemoveWordPosition(WordPosition wordPosition)
        {
            Layout.RemoveWordPositionAndSquares(wordPosition);
        }

        internal void AddWordPosition(WordPosition wordPosition)
        {
            Layout.AddWordPositionAndSquares(wordPosition);
        }
    }
}
