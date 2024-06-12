using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Data;
using System.Windows.Shapes;
using Task4.View;

namespace Task4.RevitSystem
{
    public class AdjustSectionBoxDepthHandler : IExternalEventHandler
    {
        #region Algorithm

        // ************************************************************ IMPORTANT ***********************************************************************
        // * In the Revit API, you cannot directly modify the bounding box of a 2D Section view.                                                        *
        // * There are two common approaches to handle this limitation:                                                                                 *
        // * 1. Create a temporary 3D view, copy your 2D section elements into it, adjust the bounding box as needed, and then convert it back to a 2D  *
        // *    section with the desired bounding box.                                                                                                  *
        // * 2. Copy the section view, delete the original, and recreate it with the desired bounding box dimensions.                                   *
        // ************************************************************ IMPORTANT ***********************************************************************

        // SetParameters Method:
        // Step 1: Set the parameters for the handler.
        // 1.1: Set the document parameter.
        // 1.2: Set the view ID parameter.

        // Execute Method:
        // Step 2: Store and delete original section view.
        // 2.1: Store original section view parameters.
        // 2.2: Delete original section view.
        // 2.3: Handle error if active view is not a section view or is null.
        // Step 3: Create a new section view.
        // 3.1: Get the level of the active floor plan.
        // 3.2: Adjust the bounding box Y coordinates to be 10 feet above and below the level elevation.
        // 3.3: Use the original transform directly with level modification.
        // 3.4: Create a new section view using the original parameters.
        // 3.5: Set the adjusted crop box for the new section view.
        // Step 4: Create a reference section.
        // 4.1: Transform coordinates for the reference section.
        // 4.2: Create reference section.
        // 4.3: Handle exceptions.

        // GetOffsetValueFromWpf Method:
        // Step 5: Retrieve the offset value from the WPF interface.
        // 5.1: Check if WPF window is already opened.
        // 5.2: Show the offset WPF window.

        // GetName Method:
        // Step 6: Return the name of the handler.
        // 6.1: Return handler name.

        // GetLevel Method:
        // Step 7: Retrieve the level from the specified floor plan view.
        // 7.1: Get level from the floor plan view.

        #endregion

        #region Private Fields
        private Document _doc;
        private ElementId _viewId;
        private static List<ElementId> createdElementIds = new List<ElementId>();
        #endregion

        #region Public Methods

        #region SetParameters
        /// <summary>
        /// Sets the parameters for the handler.
        /// </summary>
        /// <param name="doc">The Revit document.</param>
        /// <param name="viewId">The view ID.</param>
        public void SetParameters(Document doc, ElementId viewId)
        {
            // Step 1.1: Set the document parameter
            _doc = doc;

            // Step 1.2: Set the view ID parameter
            _viewId = viewId;
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the handler to adjust section box depth.
        /// </summary>
        /// <param name="app">The UIApplication instance.</param>
        public void Execute(UIApplication app)
        {
            try
            {
                BoundingBoxXYZ cropBox = null;
                ElementId viewTypeId = null;
                string originalViewName = null;
                Transform originalTransform = null;
                XYZ originalMin = null;
                XYZ originalMax = null;

                using (TransactionGroup txGroup = new TransactionGroup(_doc, "Adjust Section Box Depth"))
                {
                    txGroup.Start();

                    #region Step 2: Store and delete original section view
                    using (Transaction txDelete = new Transaction(_doc, "Delete Original Section View"))
                    {
                        txDelete.Start();

                        ViewSection originalView = _doc.GetElement(_viewId) as ViewSection;
                        if (originalView != null && originalView.ViewType == ViewType.Section)
                        {
                            if (createdElementIds.Contains(originalView.Id))
                                return;

                            // Step 2.1: Store original section view parameters
                            cropBox = originalView.CropBox;
                            viewTypeId = originalView.GetTypeId();
                            originalViewName = originalView.Name;
                            originalTransform = cropBox.Transform;
                            originalMin = cropBox.Min;
                            originalMax = cropBox.Max;

                            // Step 2.2: Delete original section view
                            _doc.Delete(originalView.Id);
                            txDelete.Commit();
                        }
                        else
                        {
                            // Step 2.3: Handle error if active view is not a section view or is null
                            TaskDialog.Show("Error", "The active view is not a section view or is null.");
                            txDelete.RollBack();
                            return;
                        }
                    }
                    #endregion

                    #region Step 3: Create a new section view
                    ViewSection newSectionView = null;
                    using (Transaction txCreate = new Transaction(_doc, "Create Copy with Adjusted Bounding Box"))
                    {
                        txCreate.Start();

                        if (cropBox != null)
                        {
                            // Step 3.1: Get the level of the active floor plan
                            ViewPlan floorPlanView = _doc.ActiveView as ViewPlan;
                            Level level = GetLevel(floorPlanView);
                            double levelElevation = level.Elevation;

                            GetOffsetValueFromWpf();

                            // Step 3.2: Adjust the bounding box Y coordinates to be 10 feet above and below the level elevation
                            double offset = App.offsetNum;
                            BoundingBoxXYZ newBox = new BoundingBoxXYZ
                            {
                                Min = new XYZ(cropBox.Min.X, -offset, -1),
                                Max = new XYZ(cropBox.Max.X, offset, 1)
                            };

                            // Step 3.3: Use the original transform directly with level modification
                            XYZ newOrigin = new XYZ(originalTransform.Origin.X, 0, offset + levelElevation);
                            originalTransform.Origin = newOrigin;
                            newBox.Transform = originalTransform;

                            // Step 3.4: Create a new section view using the original parameters
                            newSectionView = ViewSection.CreateSection(_doc, viewTypeId, newBox);
                            newSectionView.Name = originalViewName;

                            // Step 3.5: Set the adjusted crop box for the new section view
                            newSectionView.CropBox = newBox;
                            newSectionView.CropBoxActive = true;
                            newSectionView.CropBoxVisible = true;

                            createdElementIds.Add(newSectionView.Id);
                            txCreate.Commit();
                        }
                    }
                    #endregion

                    #region Step 4: Create a reference section
                    if (newSectionView != null)
                    {
                        using (Transaction txReference = new Transaction(_doc, "Create Reference Section"))
                        {
                            txReference.Start();
                            ViewPlan floorPlanView = _doc.ActiveView as ViewPlan;

                            // Step 4.1: Transform coordinates for the reference section
                            Transform referenceTransform = cropBox.Transform;
                            XYZ originalHeadPoint = referenceTransform.OfPoint(new XYZ(originalMin.X, originalMin.Y, 0));
                            XYZ originalTailPoint = referenceTransform.OfPoint(new XYZ(originalMax.X, originalMax.Y, 0));

                            // Step 4.2: Create reference section
                            ViewSection.CreateReferenceSection(_doc, floorPlanView.Id, newSectionView.Id, originalHeadPoint, originalTailPoint);
                            txReference.Commit();
                        }
                    }
                    #endregion

                    txGroup.Assimilate();
                }
            }
            catch (Exception ex)
            {
                // Step 4.3: Handle exceptions
                TaskDialog.Show("Error", $"Failed to complete the operation: {ex.Message}");
            }
        }
        #endregion

        #region GetOffsetValueFromWpf
        /// <summary>
        /// Retrieves the offset value from the WPF interface.
        /// </summary>
        public void GetOffsetValueFromWpf()
        {
            // Step 5.1: Check if WPF window is already opened
            if (App.IfWpfOpened)
                return;

            // Step 5.2: Show the offset WPF window
            offset offsetUi = new offset();
            offsetUi.ShowDialog(); // This makes the window modeless
        }
        #endregion

        #region GetName
        /// <summary>
        /// Returns the name of the handler.
        /// </summary>
        /// <returns>Handler name.</returns>
        public string GetName()
        {
            // Step 6.1: Return handler name
            return "Adjust Section Box Depth Handler";
        }
        #endregion

        #endregion

        #region Private Methods

        #region GetLevel
        /// <summary>
        /// Retrieves the level from the specified floor plan view.
        /// </summary>
        /// <param name="floorPlanView">The floor plan view.</param>
        /// <returns>The associated level.</returns>
        private Level GetLevel(ViewPlan floorPlanView)
        {
            // Step 7.1: Get level from the floor plan view
            return _doc.GetElement(floorPlanView.GenLevel.Id) as Level;
        }
        #endregion

        #endregion
    }
}
