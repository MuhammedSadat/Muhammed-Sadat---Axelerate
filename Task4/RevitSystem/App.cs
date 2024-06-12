using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Task4.RevitSystem
{
    [Transaction(TransactionMode.Manual)]
    public class App : IExternalApplication
    {
        #region Global Variables
        public static double offsetNum = 1;
        public static bool IfWpfOpened = false;

        private static ExternalEvent _externalEvent;
        private static AdjustSectionBoxDepthHandler _handler;
        private static SectionViewUpdater _updater;
        #endregion

        #region OnStartup Method
        /// <summary>
        /// Method called when the application starts up.
        /// </summary>
        /// <param name="application">The UI controlled application.</param>
        /// <returns>Result indicating success or failure.</returns>
        public Result OnStartup(UIControlledApplication application)
        {
            // Step 1: Initialize handler and external event
            _handler = new AdjustSectionBoxDepthHandler();
            _externalEvent = ExternalEvent.Create(_handler);

            // Step 2: Initialize and register the section view updater
            _updater = new SectionViewUpdater(_externalEvent, _handler);
            UpdaterRegistry.RegisterUpdater(_updater, true);

            // Step 3: Add trigger for section view updater
            ElementClassFilter viewSectionFilter = new ElementClassFilter(typeof(ViewSection));
            UpdaterRegistry.AddTrigger(_updater.GetUpdaterId(), viewSectionFilter, Element.GetChangeTypeElementAddition());

            // Step 4: Display a task dialog to indicate startup
            TaskDialog.Show("Startup Task4", "Muhammed Sadat: Ready to adjust section box depths based on the activated floor plan.");
            return Result.Succeeded;
        }
        #endregion

        #region OnShutdown Method
        /// <summary>
        /// Method called when the application shuts down.
        /// </summary>
        /// <param name="application">The UI controlled application.</param>
        /// <returns>Result indicating success or failure.</returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            // Step 1: Unregister the section view updater
            UpdaterRegistry.UnregisterUpdater(_updater.GetUpdaterId());
            return Result.Succeeded;
        }
        #endregion
    }
}
