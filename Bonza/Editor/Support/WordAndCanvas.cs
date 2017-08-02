// Bonza Editor - WordAndCanvas class
// Combination of a WordPositon and a WordCanvas, used by View and Selection

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonza.Generator;


namespace Bonza.Editor.Support
{
    internal class WordAndCanvas
    {
        private readonly WordPosition m_WordPosition;
        private readonly WordCanvas m_WordCanvas;


        internal WordPosition WordPosition => m_WordPosition;

        internal WordCanvas WordCanvas => m_WordCanvas;


        internal WordAndCanvas(WordPosition wp, WordCanvas wc)
        {
            this.m_WordPosition = wp;
            this.m_WordCanvas = wc;
        }
    }
}
