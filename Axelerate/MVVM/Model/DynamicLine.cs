using System.Windows.Media;
using Autodesk.Revit.DB;

namespace Axelerate.MVVM.ViewModel
{
    #region DynamicLine Class
    /// <summary>
    /// Represents a dynamic line with properties for coordinates and appearance.
    /// </summary>
    public class DynamicLine
    {
        #region Public Properties
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public Brush Stroke { get; set; } = Brushes.White;
        public double StrokeThickness { get; set; } = 2;
        #endregion

        #region Public Methods

        #region Validate Line
        /// <summary>
        /// Validates if the line has non-zero coordinates.
        /// </summary>
        /// <returns>True if valid, otherwise false.</returns>
        public bool IsValid()
        {
            // Step 1: Check if all coordinates (X1, Y1, X2, Y2) are non-zero
            return !(X1 == 0 && Y1 == 0 && X2 == 0 && Y2 == 0);
        }
        #endregion

        #region Convert to Revit Curve
        /// <summary>
        /// Converts the dynamic line to a Revit curve.
        /// </summary>
        /// <returns>Revit Line object.</returns>
        public Autodesk.Revit.DB.Line ToRevitCurve()
        {
            // Step 1: Create start and end points using the X1, Y1, X2, Y2 coordinates
            var startPoint = new Autodesk.Revit.DB.XYZ(X1, Y1, 0);
            var endPoint = new Autodesk.Revit.DB.XYZ(X2, Y2, 0);
            // Step 2: Use the Revit API to create a Revit Line between the start and end points
            return Autodesk.Revit.DB.Line.CreateBound(startPoint, endPoint);
        }
        #endregion

        #endregion
    }
    #endregion
}
