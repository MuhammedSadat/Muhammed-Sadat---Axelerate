using Autodesk.Revit.UI;
using System.Windows.Input;
using Axelerate.MVVM.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using System.Diagnostics;
using System;
using Axelerate.MVVM.ViewModel;

namespace Axelerate.MVVM.ViewModel
{

    #region Summary
    /// <summary>
    /// Main UI ViewModel class for managing dynamic lines and Revit operations.
    /// </summary>
    #endregion
    public class MainUiViewModel : ViewModelBase
    {
        #region Algorithm

        // Initialize MainUiViewModel Class:
        // 1. Set private fields for command data, background color, scale, loop status message, and a constant _sixteenth.
        // 2. Define public properties for Lines, Colors, SelectedBackgroundColor, Scale, LoopStatusMessage, IsCurvesContiguous, and commands CheckLoopCommand and CreateFloorCommand.
        // 3. Implement the constructor to initialize the commands.

        // Check Loop Closure Method:
        // Step 1: Validate lines
        // 1.1: Retrieve valid lines from the Lines collection.
        // 1.2: If less than three valid lines, set LoopStatusMessage and IsCurvesContiguous to false and return.
        // Step 2: Convert lines to Revit curves
        // 2.1: Convert valid lines to Revit curves.
        // Step 3: Sort curves to be contiguous
        // 3.1: Ensure the curves are contiguous by calling SortCurvesContiguous.
        // Step 4: Check if the loop is closed
        // 4.1: Initialize a loop closure flag.
        // 4.2: Iterate through curves to check continuity.
        // 4.2.1: Get current and next curve.
        // 4.2.2: Compare the end point of the current curve to the start point of the next curve.
        // 4.2.3: If not contiguous, set flag to false and break.
        // 4.3: Set LoopStatusMessage and IsCurvesContiguous based on loop closure status.
        // Handle any exceptions and update LoopStatusMessage.

        // Create Floor Method:
        // Step 1: Validate lines
        // 1.1: Retrieve valid lines from the Lines collection.
        // 1.2: If less than three valid lines, set LoopStatusMessage and return.
        // 1.3: If curves do not form a closed loop, set LoopStatusMessage and return.
        // Step 2: Create curve array from valid lines
        // 2.1: Create a new curve array from the valid lines.
        // Step 3: Retrieve the floor type
        // 3.1: Use a FilteredElementCollector to find the specified floor type.
        // 3.2: If the floor type is not found, set LoopStatusMessage and roll back the transaction.
        // Step 4: Retrieve the level
        // 4.1: Retrieve the level from the active view.
        // 4.2: If the level is not found, set LoopStatusMessage and roll back the transaction.
        // Step 5: Create the floor
        // 5.1: Create a new floor with the curve array, floor type, and level.
        // 5.2: Set LoopStatusMessage and commit the transaction.
        // Handle any exceptions and update LoopStatusMessage.

        // Sort Curves Contiguous Method:
        // Step 1: Initialize variables
        // 1.1: Initialize a variable n to the count of curves.
        // Step 2: Iterate through curves
        // 2.1: For each curve, get the current curve and its end point.
        // 2.2: Initialize a found flag.
        // 2.3: Iterate through the remaining curves to find the next contiguous curve.
        // 2.4: Check if the start point of the curve matches the end point of the current curve.
        // 2.5: If matching, swap the curves if not already in position.
        // 2.6: If not found, throw an exception for non-contiguous input curves.

        // Create Reversed Curve Method:
        // Step 1: Use the Revit API to create and return a reversed curve.

        #endregion

        #region Private Fields
        private readonly ExternalCommandData _commandData;
        private string _selectedBackgroundColor = "Black";
        private double _scale = 1.0;
        private string _loopStatusMessage = "";
        private static readonly double _sixteenth = 1.0 / 16.0;
        #endregion

        #region Public Properties
        public ObservableCollection<DynamicLine> Lines { get; private set; } = new ObservableCollection<DynamicLine>();
        public ObservableCollection<string> Colors { get; } = new ObservableCollection<string> { "Black", "Red", "Green", "Blue" };

        public string SelectedBackgroundColor
        {
            get => _selectedBackgroundColor;
            set
            {
                if (_selectedBackgroundColor != value)
                {
                    _selectedBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LoopStatusMessage
        {
            get => _loopStatusMessage;
            set
            {
                if (_loopStatusMessage != value)
                {
                    _loopStatusMessage = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool IsCurvesContiguous { get; set; }
        public ICommand CheckLoopCommand { get; }
        public ICommand CreateFloorCommand { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainUiViewModel class.
        /// </summary>
        /// <param name="commandData">External command data provided by Revit.</param>
        public MainUiViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            CheckLoopCommand = new RelayCommand(CheckLoopClosure);
            CreateFloorCommand = new RelayCommand(CreateFloor);
        }
        #endregion

        #region Public Methods

        #region Check Loop Closure
        /// <summary>
        /// Checks if the dynamic lines form a contiguous closed loop.
        /// </summary>
        public void CheckLoopClosure()
        {
            #region Step 1: Validate lines
            // Step 1.1: Retrieve valid lines from the Lines collection
            var validLines = Lines.Where(line => line.IsValid()).ToList();
            // Step 1.2: If less than three valid lines, set LoopStatusMessage and IsCurvesContiguous
            if (validLines.Count < 3)
            {
                LoopStatusMessage = "At least three valid lines are required to form a loop.";
                IsCurvesContiguous = false;
                return;
            }
            #endregion

            #region Step 2: Convert lines to Revit curves
            // Step 2.1: Convert valid lines to Revit curves
            IList<Curve> curves = validLines.Select(line => line.ToRevitCurve() as Curve).ToList();
            #endregion

            try
            {
                #region Step 3: Sort curves to be contiguous
                // Step 3.1: Ensure the curves are contiguous
                var sortedCurves = SortCurvesContiguous(curves);
                #endregion

                #region Step 4: Check if the loop is closed
                // Step 4.1: Initialize loop closure flag
                bool isLoopClosed = true;
                // Step 4.2: Iterate through curves to check continuity
                for (int i = 0; i < sortedCurves.Count; i++)
                {
                    #region Step 4.2.1: Get current and next curve
                    // Get current curve
                    Curve current = sortedCurves[i];
                    // Get next curve, wrapping around to the first curve
                    Curve next = sortedCurves[(i + 1) % sortedCurves.Count];
                    #endregion

                    #region Step 4.2.2: Compare end and start points
                    // Compare end point of current curve to start point of next curve
                    if (!current.GetEndPoint(1).IsAlmostEqualTo(next.GetEndPoint(0)))
                    {
                        // If not contiguous, set flag to false and break
                        isLoopClosed = false;
                        break;
                    }
                    #endregion
                }

                // Step 4.3: Set LoopStatusMessage and IsCurvesContiguous based on loop closure status
                LoopStatusMessage = isLoopClosed ? "Successfully formed a loop." : "Curves do not form a closed loop.";
                IsCurvesContiguous = isLoopClosed;
                #endregion
            }
            catch (Exception ex)
            {
                // Step 4.4: Handle any exceptions and update LoopStatusMessage
                LoopStatusMessage = $"Error checking loop closure: {ex.Message}";
                IsCurvesContiguous = false;
            }
        }
        #endregion

        #region Create Floor
        /// <summary>
        /// Creates a floor in Revit based on the dynamic lines if they form a closed loop.
        /// </summary>
        public void CreateFloor()
        {
            #region Step 1: Validate lines
            // Step 1.1: Retrieve valid lines from the Lines collection
            var validLines = Lines.Where(line => line.IsValid()).ToList();
            // Step 1.2: If less than three valid lines, set LoopStatusMessage and return
            if (validLines.Count < 3)
            {
                LoopStatusMessage = "Cannot create floor: at least three valid lines are required to form a loop.";
                return;
            }

            // Step 1.3: If curves do not form a closed loop, set LoopStatusMessage and return
            if (!IsCurvesContiguous)
            {
                LoopStatusMessage = "Curves do not form a closed loop.";
                return;
            }
            #endregion

            try
            {
                // Step 1.4: Get the current Revit document and active view
                Document doc = _commandData.Application.ActiveUIDocument.Document;
                Autodesk.Revit.DB.View activeView = _commandData.Application.ActiveUIDocument.ActiveView;

                // Step 1.5: Start a new transaction
                using (Transaction tx = new Transaction(doc, "Create Floor"))
                {
                    tx.Start();

                    #region Step 2: Create curve array from valid lines
                    // Step 2.1: Ensure the curves are contiguous and get sorted curves
                    IList<Curve> sortedCurves = SortCurvesContiguous(validLines.Select(line => line.ToRevitCurve() as Curve).ToList());

                    // Step 2.2: Create a new curve array from the sorted curves
                    CurveArray curveArray = new CurveArray();
                    foreach (var curve in sortedCurves)
                    {
                        curveArray.Append(curve);
                    }
                    #endregion

                    #region Step 3: Retrieve the floor type
                    // Step 3.1: Use a FilteredElementCollector to find the specified floor type
                    FloorType floorType = new FilteredElementCollector(doc)
                        .OfClass(typeof(FloorType))
                        .FirstOrDefault(e => e.Name == "Floor-Grnd-Susp_65Scr-80Ins-100Blk-75PC") as FloorType;

                    // Step 3.2: If the floor type is not found, set LoopStatusMessage and roll back transaction
                    if (floorType == null)
                    {
                        LoopStatusMessage = "Failed to find the specified floor type.";
                        tx.RollBack();
                        return;
                    }
                    #endregion

                    #region Step 4: Retrieve the level
                    // Step 4.1: Retrieve the level from the active view
                    Level level = activeView.GenLevel;
                    // Step 4.2: If the level is not found, set LoopStatusMessage and roll back transaction
                    if (level == null)
                    {
                        LoopStatusMessage = "Failed to find a valid level.";
                        tx.RollBack();
                        return;
                    }
                    #endregion

                    #region Step 5: Create the floor
                    // Step 5.1: Create a new floor with the curve array, floor type, and level
                    doc.Create.NewFloor(curveArray, floorType, level, false);
                    // Step 5.2: Set LoopStatusMessage and commit transaction
                    LoopStatusMessage = "Floor created successfully.";
                    tx.Commit();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                // Step 5.3: Handle any exceptions and update LoopStatusMessage
                LoopStatusMessage = $"Error creating floor: {ex.Message}";
            }
        }
        #endregion

        #endregion

        #region Private Methods

        #region Sort Curves Contiguous
        /// <summary>
        /// Sorts curves to ensure they are contiguous.
        /// </summary>
        /// <param name="curves">List of curves to sort.</param>
        /// <param name="debugOutput">Flag indicating whether to output debug information.</param>
        /// <returns>List of sorted curves</returns>
        public static IList<Curve> SortCurvesContiguous(IList<Curve> curves, bool debugOutput = false)
        {
            // Step 1: Initialize variables
            int n = curves.Count;
            for (int i = 0; i < n; ++i)
            {
                // Step 1.1: Get current curve and its end point
                Curve curve = curves[i];
                XYZ endPoint = curve.GetEndPoint(1);

                if (debugOutput)
                {
                    Debug.Print("{0} endPoint {1}", i, endPoint.ToString());
                }

                // Step 1.2: Initialize found flag
                bool found = (i + 1 >= n);

                // Step 1.3: Iterate through remaining curves to find the next contiguous curve
                for (int j = i + 1; j < n; ++j)
                {
                    XYZ p = curves[j].GetEndPoint(0);

                    // Step 1.4: Check if start point of curve matches end point of current curve
                    if (_sixteenth > p.DistanceTo(endPoint))
                    {
                        if (debugOutput)
                        {
                            Debug.Print("{0} start point, swap with {1}", j, i + 1);
                        }

                        // Step 1.5: Swap curves if not already in position
                        if (i + 1 != j)
                        {
                            Curve tmp = curves[i + 1];
                            curves[i + 1] = curves[j];
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }

                    p = curves[j].GetEndPoint(1);

                    // Step 1.6: Check if end point of curve matches end point of current curve
                    if (_sixteenth > p.DistanceTo(endPoint))
                    {
                        if (i + 1 == j)
                        {
                            if (debugOutput)
                            {
                                Debug.Print("{0} end point, reverse {1}", j, i + 1);
                            }

                            // Step 1.7: Reverse the curve if necessary
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                        }
                        else
                        {
                            if (debugOutput)
                            {
                                Debug.Print("{0} end point, swap with reverse {1}", j, i + 1);
                            }

                            Curve tmp = curves[i + 1];
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }
                }
                // Step 1.8: If no matching curve found, throw exception
                if (!found)
                {
                    throw new Exception("SortCurvesContiguous: non-contiguous input curves");
                }
            }

            return curves;
        }
        #endregion

        #region Create Reversed Curve
        /// <summary>
        /// Creates a reversed curve.
        /// </summary>
        /// <param name="curve">Curve to reverse.</param>
        /// <returns>Reversed curve.</returns>
        public static Curve CreateReversedCurve(Curve curve)
        {
            // Step 1: Use the Revit API to create and return a reversed curve
            return curve.CreateReversed();
        }
        #endregion

        #endregion

    }
}

