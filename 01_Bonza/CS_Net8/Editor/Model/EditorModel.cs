// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// Editor Model, access to Generator class
//
// 2017-07-22   PV      First version
// 2021-11-13   PV      Net6 C#10

using Bonza.Editor.ViewModel;
using Bonza.Generator;
using System.Collections.Generic;
using System.Linq;

namespace Bonza.Editor.Model;

internal sealed class EditorModel
{
    private readonly EditorViewModel viewModel;
    private Grille grille;
    public WordPositionLayout Layout => grille?.Layout;

    public EditorModel(EditorViewModel viewModel)
    {
        this.viewModel = viewModel;
        NewGrille();
    }

    internal void NewGrille() => grille = new Grille();

    internal void LoadGrille(string wordsFile)
    {
        //grille = new Grille();
        //viewModel.ClearLayout();
        int i;
        for (i = 0; i < 5; i++)
            if (grille.AddWordsFromFile(wordsFile))
                break;
        if (i == 5)
            throw new BonzaException("Impossible de placer les mots malgré 5 tentatives");

        viewModel.AddCanvasForWordPositionList(Layout.WordPositionList);
    }

    internal void ResetLayout()
    {
        viewModel.ClearLayout();
        int i;
        for (i = 0; i < 5; i++)
            if (grille.PlaceWordsAgain())
                break;
        if (i == 5)
            throw new BonzaException("Impossible de générer un nouveau layout malgré 5 tentatives");
    }

    // Build a copy of Layout without a specific list of WordPosition to validate placement
    internal WordPositionLayout GetLayoutExcludingWordPositionList(IEnumerable<WordPosition> movedWordPositionList)
    {
        var layout = new WordPositionLayout();
        foreach (var wp in Layout.WordPositionList)
            // ReSharper disable once PossibleMultipleEnumeration
            if (!movedWordPositionList.Contains(wp))
                layout.AddWordPositionNoCheck(wp);
        return layout;
    }

    // Check if a WordPosition can be placed at specific location in given layout
    internal static PlaceWordStatus CanPlaceWordAtPositionInLayout(WordPositionLayout layout, WordPosition wordPosition, PositionOrientation position)
    {
        var testWordPosition = new WordPosition(wordPosition.Word, wordPosition.OriginalWord, position);
        return layout.CanPlaceWord(testWordPosition, true);
    }

    // Removes a word from current Layout
    internal void RemoveWordPosition(WordPosition wordPosition) => Layout.RemoveWordPosition(wordPosition);

    internal PlaceWordStatus AddWordPosition(WordPosition wordPosition) => Layout.AddWordPosition(wordPosition);

    public string CheckWordsList(List<string> wordsList) => grille.CheckWordsList(wordsList);

    public IEnumerable<WordPosition> AddWordsList(List<string> wordsList) =>
        // Use backup option to avoid half updating layout
        grille.PlaceWordsList(wordsList, true);
}
