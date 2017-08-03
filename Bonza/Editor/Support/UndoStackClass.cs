// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// Support for Undo
//
// 2017-07-22   PV  First version
// 2017-08-03   PV  Isolated in a separate file, UndoAction(s)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Bonza.Generator;


namespace Bonza.Editor.Support
{
    internal enum UndoActions
    {
        Move,
        Delete
    };

    internal class UndoAction
    {
        internal readonly UndoActions Action;

        internal IList<WordAndCanvas> WordAndCanvasList;
        internal IList<PositionOrientation> PositionOrientationList;


        internal UndoAction(UndoActions action)
        {
            Action = action;
        }
    }


    // Undo support
    // Encapsulated support in a specific class for easier debugging (and also because it's a good practice...)
    internal class UndoStackClass
    {
        // Can't memorize original position in List<WordPosition> since referenced WordPosition will change position during future edit
        // So List<PositionOrientation> represents the position the List<WordPositon> must return to in case on undo
        private Stack<UndoAction> undoStack;

        internal void Clear()
        {
            undoStack = null;
        }


        // Memorize current position of a list of WordPosition during a Move operation
        internal void MemorizeMove(IList<WordAndCanvas> wordAndCanvasList)
        {
            if (wordAndCanvasList == null) throw new ArgumentNullException(nameof(wordAndCanvasList));
            Debug.Assert(wordAndCanvasList.Count >= 1);

            // Memorize position in a separate list since WordPosition objects position will change
            List<PositionOrientation> topLeftList = wordAndCanvasList.Select(wac => new PositionOrientation { StartRow = wac.WordPosition.StartRow, StartColumn = wac.WordPosition.StartColumn, IsVertical = wac.WordPosition.IsVertical }).ToList();

            if (undoStack == null)
                undoStack = new Stack<UndoAction>();

            UndoAction a = new UndoAction(UndoActions.Move)
            {
                // Since wordPositionList is a list belonging to view, we need to clone it
                WordAndCanvasList = new List<WordAndCanvas>(wordAndCanvasList),
                PositionOrientationList = topLeftList
            };
            undoStack.Push(a);
        }

        // Memorize a list of Words about to be deleted
        internal void MemorizeDelete(IList<WordAndCanvas> wordAndCanvasList)
        {
            if (wordAndCanvasList == null) throw new ArgumentNullException(nameof(wordAndCanvasList));
            Debug.Assert(wordAndCanvasList.Count >= 1);

            if (undoStack == null)
                undoStack = new Stack<UndoAction>();

            UndoAction a = new UndoAction(UndoActions.Delete)
            {
                WordAndCanvasList = new List<WordAndCanvas>(wordAndCanvasList),
                PositionOrientationList = null   // No need to memorize original position, sice words are about to be deleted and won't move
            };
            undoStack.Push(a);
        }


        internal bool CanUndo => undoStack != null && undoStack.Count > 0;

        internal UndoAction Pop()
        {
            Debug.Assert(CanUndo);
            return undoStack.Pop();
        }

    }

}
