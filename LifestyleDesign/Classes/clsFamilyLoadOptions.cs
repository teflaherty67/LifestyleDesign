using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifestyleDesign.Classes
{
    internal class clsFamilyLoadOptions : IFamilyLoadOptions
    {

        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            if (!familyInUse)
            {
                TaskDialog.Show("SampleFamilyLoadOptions", "The family has not been in use and will keep loading.");

                overwriteParameterValues = true;
                return true;
            }
            else
            {
                TaskDialog.Show("SampleFamilyLoadOptions", "The family has been in use but will still be loaded with existing parameters overwritten.");

                overwriteParameterValues = true;
                return true;
            }
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            if (!familyInUse)
            {
                TaskDialog.Show("SampleFamilyLoadOptions", "The shared family has not been in use and will keep loading.");

                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
            else
            {
                TaskDialog.Show("SampleFamilyLoadOptions", "The shared family has been in use but will still be loaded from the FamilySource with existing parameters overwritten.");

                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }
    }
}

