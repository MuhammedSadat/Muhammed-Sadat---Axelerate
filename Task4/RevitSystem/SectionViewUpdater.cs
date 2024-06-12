using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace Task4.RevitSystem
{
    public class SectionViewUpdater : IUpdater
    {
        #region Private Fields
        private readonly ExternalEvent _externalEvent;
        private readonly AdjustSectionBoxDepthHandler _handler;

        private static AddInId appId = new AddInId(new Guid("D5011608-AA4C-4E00-9673-08C70812C0CA"));
        private static UpdaterId updaterId = new UpdaterId(appId, new Guid("C3C95E4D-C169-4510-B01C-9D07EFDE7E86"));
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the SectionViewUpdater class.
        /// </summary>
        /// <param name="externalEvent">The external event to trigger.</param>
        /// <param name="handler">The handler for adjusting section box depth.</param>
        public SectionViewUpdater(ExternalEvent externalEvent, AdjustSectionBoxDepthHandler handler)
        {
            // Step 1.1: Initialize the external event
            _externalEvent = externalEvent;

            // Step 1.2: Initialize the handler
            _handler = handler;
        }
        #endregion

        #region Public Methods

        #region Execute
        /// <summary>
        /// Executes the updater when an element is added.
        /// </summary>
        /// <param name="updateData">The data related to the update event.</param>
        public void Execute(UpdaterData updateData)
        {
            // Step 2.1: Get the document from the update data
            Document doc = updateData.GetDocument();

            // Step 2.2: Process each added element
            foreach (ElementId eid in updateData.GetAddedElementIds())
            {
                // Step 2.3: Check if the added element is a ViewSection
                ViewSection viewSection = doc.GetElement(eid) as ViewSection;
                if (viewSection != null)
                {
                    // Step 2.4: Set parameters and raise the external event
                    _handler.SetParameters(doc, eid);
                    _externalEvent.Raise();
                }
            }
        }
        #endregion

        #region GetAdditionalInformation
        /// <summary>
        /// Provides additional information about the updater.
        /// </summary>
        /// <returns>String containing additional information.</returns>
        public string GetAdditionalInformation()
        {
            // Step 3.1: Return additional information string
            return "Automatically adjusts the bounding box when a section view is created.";
        }
        #endregion

        #region GetChangePriority
        /// <summary>
        /// Returns the priority of changes handled by this updater.
        /// </summary>
        /// <returns>The change priority.</returns>
        public ChangePriority GetChangePriority()
        {
            // Step 4.1: Return the priority of views
            return ChangePriority.Views;
        }
        #endregion

        #region GetUpdaterId
        /// <summary>
        /// Returns the updater ID.
        /// </summary>
        /// <returns>The updater ID.</returns>
        public UpdaterId GetUpdaterId()
        {
            // Step 5.1: Return the updater ID
            return updaterId;
        }
        #endregion

        #region GetUpdaterName
        /// <summary>
        /// Returns the name of the updater.
        /// </summary>
        /// <returns>The updater name.</returns>
        public string GetUpdaterName()
        {
            // Step 6.1: Return the updater name
            return "View Bounding Box Updater";
        }
        #endregion

        #endregion
    }
}
