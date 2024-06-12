using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Axelerate.MVVM.View;
using System;

namespace Axelerate.RevitSystem
{
    [Transaction(TransactionMode.Manual)]
    public class ExecuteAddin : IExternalCommand
    {
        #region Private Fields
        public static UIApplication uiApp;
        public static Document doc;
        #endregion

        #region Execute Method
        #region Summary
        /// <summary>
        /// The Execute method is the entry point for the command. It is called when the command is invoked.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="message">A message that can be displayed if the command fails.</param>
        /// <param name="elements">A set of elements to operate on.</param>
        /// <returns>A Result indicating whether the command succeeded or failed.</returns>
        #endregion
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                #region Step 1: Initialize static fields
                // Step 1.1: Set the uiApp field to the current UIApplication
                uiApp = commandData.Application;
                // Step 1.2: Set the doc field to the current document
                doc = uiApp.ActiveUIDocument.Document;
                #endregion

                #region Step 2: Show the main UI window
                // Step 2.1: Invoke the method to show the main UI window
                ShowMainWindow(commandData);
                #endregion

                // Step 3: Return success
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                #region Step 4: Handle any errors
                // Step 4.1: Set the message parameter to the exception message
                message = ex.Message;
                // Step 4.2: Return failure
                return Result.Failed;
                #endregion
            }
        }
        #endregion

        #region Private Methods
        #region Summary
        /// <summary>
        /// Shows the main UI window.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        #endregion
        private void ShowMainWindow(ExternalCommandData commandData)
        {
            #region Step 1: Instantiate the MainUi class
            // Step 1.1: Create an instance of the MainUi class, passing the command data
            MainUi mainUi = new MainUi(commandData);
            #endregion

            #region Step 2: Show the main UI window
            // Step 2.1: Display the main UI window modally
            mainUi.ShowDialog();
            #endregion
        }
        #endregion
    }
}
