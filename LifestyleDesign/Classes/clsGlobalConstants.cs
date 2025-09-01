using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifestyleDesign.Classes
{
    internal static class ProjectConstants
    {
        // Level filtering
        public static readonly string[] ExcludedLevelNames = { "First Floor", "Main Level" };
        public static readonly string[] MultiStoryIndicators = { "Second Floor", "Upper Level" };

        // Plate adjustments
        public static readonly double StandardPlateAdjustment = 1.0; // 12 inches

        // View names
        public static readonly string FrontElevationViewName = "Front Elevation";

        // Other constants that multiple commands might use...
    }
}
