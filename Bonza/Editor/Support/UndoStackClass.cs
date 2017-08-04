﻿// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
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
    // Undo support
    // Encapsulated support in a specific class for easier debugging (and also because it's a good practice...)
    internal class UndoStackClass
    {
        internal enum UndoActions
        {
            Move,
            Delete,
            Add,
            SwapOrientation
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



        // Can't memorize original position in List<WordPosition> since referenced WordPosition will change position during future edit
        // So List<PositionOrientation> represents the position the List<WordPositon> must return to in case on undo
        private Stack<UndoAction> undoStack;

        internal void Clear()
        {
            undoStack = null;
        }


        private void MemorizeUndoableAction(UndoActions action, IList<WordAndCanvas> wordAndCanvasList, IList<PositionOrientation> topLeftList)
        {
            if (wordAndCanvasList == null) throw new ArgumentNullException(nameof(wordAndCanvasList));
            Debug.Assert(wordAndCanvasList.Count >= 1);

            if (undoStack == null)
                undoStack = new Stack<UndoAction>();

            UndoAction a = new UndoAction(action)
            {
                WordAndCanvasList = new List<WordAndCanvas>(wordAndCanvasList),
                PositionOrientationList = topLeftList
            };
            undoStack.Push(a);
        }

        // Memorize current position of a list of WordPosition during a Move operation
        internal void MemorizeMove(IList<WordAndCanvas> wordAndCanvasList)
        {
            if (wordAndCanvasList == null) throw new ArgumentNullException(nameof(wordAndCanvasList));
            // Memorize position in a separate list since WordPosition objects position will change
            List<PositionOrientation> topLeftList = wordAndCanvasList.Select(wac => new PositionOrientation { StartRow = wac.WordPosition.StartRow, StartColumn = wac.WordPosition.StartColumn, IsVertical = wac.WordPosition.IsVertical }).ToList();

            MemorizeUndoableAction(UndoActions.Move, wordAndCanvasList, topLeftList);
        }

        // Memorize a list of Words about to be deleted
        internal void MemorizeDelete(IList<WordAndCanvas> wordAndCanvasList)
        {
            MemorizeUndoableAction(UndoActions.Delete, wordAndCanvasList, null);
        }

        public void MemorizeAdd(IList<WordAndCanvas> wordAndCanvasList)
        {
            MemorizeUndoableAction(UndoActions.Add, wordAndCanvasList, null);
        }

        internal void MemorizeSwapOrientation(IList<WordAndCanvas> wordAndCanvasList)
        {
            // Memorize position in a separate list since WordPosition objects position will change
            List<PositionOrientation> topLeftList = wordAndCanvasList.Select(wac => new PositionOrientation { StartRow = wac.WordPosition.StartRow, StartColumn = wac.WordPosition.StartColumn, IsVertical = wac.WordPosition.IsVertical }).ToList();
            MemorizeUndoableAction(UndoActions.SwapOrientation, wordAndCanvasList, topLeftList);
        }


        internal bool CanUndo => undoStack != null && undoStack.Count > 0;

        internal UndoAction Pop()
        {
            Debug.Assert(CanUndo);
            return undoStack.Pop();
        }

    }

}
