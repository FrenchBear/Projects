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

namespace Bonza
{
    class BonzaPuzzle
    {
        public List<WordPosition> Layout;


        public BonzaPuzzle()
        {
            // Demo
            ReadLayout("fruits.layout");
        }

        // Load layout from a .json file
        internal void ReadLayout(string inFile)
        {
            Debug.Assert(Layout == null);
            string text = File.ReadAllText(inFile);

            Layout = JsonConvert.DeserializeObject<List<WordPosition>>(text);
        }

    }
}
