using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifestyleDesign
{
    internal class JournalData
    {
        public int PositionNumber { get; set; }
        public string Text { get; set; }
        public string Display { get; set; }

        public JournalData(int positionNumber, string text)
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
