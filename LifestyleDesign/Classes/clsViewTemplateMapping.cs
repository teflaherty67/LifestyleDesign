using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifestyleDesign.Classes
{
    internal class clsViewTemplateMapping
    {
        public string OldTemplateName { get; set; }
        public string NewTemplateName { get; set; }

        public clsViewTemplateMapping(string oldVTName, string newVTName)
        {
            OldTemplateName = oldVTName;
            NewTemplateName = newVTName;
        }
    }
}
