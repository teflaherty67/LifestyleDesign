using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifestyleDesign.Classes
{
    internal class clsJournalData
    {
        public int PositionNumber { get; set; }
        public string Text { get; set; }
        public string Display { get; set; }

        public clsJournalData(int positionNumber, string text)
        {
            PositionNumber = positionNumber;
            Text = text;

            UpdateDisplayString();
        }

        public void UpdateDisplayString()
        {
            Display = Text;
        }
    }
}
