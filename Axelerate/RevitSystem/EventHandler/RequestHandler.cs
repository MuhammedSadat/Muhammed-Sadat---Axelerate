using System;
using System.Collections.Generic;

namespace Axelerate.RevitSystem.EventHandler
{
    #region Summary
    /// <summary>
    /// Handles requests for Revit operations in a thread-safe manner.
    /// </summary>
    #endregion
    public class RequestHandler
    {
        #region Private Fields
        private Queue<RequestId> requests = new Queue<RequestId>();
        #endregion

        #region Public Methods

        #region Make Request
        /// <summary>
        /// Enqueues a request for Revit operation.
        /// </summary>
        /// <param name="request">The request to enqueue.</param>
        public void MakeRequest(RequestId request)
        {
            lock (this)
            {
                #region Step 1: Enqueue request
                // Step 1.1: Add the request to the queue
                requests.Enqueue(request);
                #endregion
            }
        }
        #endregion

        #region Take Request
        /// <summary>
        /// Dequeues a request for Revit operation.
        /// </summary>
        /// <returns>The dequeued request or RequestId.None if the queue is empty.</returns>
        public RequestId TakeRequest()
        {
            lock (this)
            {
                #region Step 1: Check if queue is not empty
                // Step 1.1: If there are requests in the queue, dequeue and return the request
                if (requests.Count > 0)
                {
                    return requests.Dequeue();
                }
                #endregion

                #region Step 2: Return None if queue is empty
                // Step 2.1: Return None if there are no requests in the queue
                return RequestId.None;
                #endregion
            }
        }
        #endregion

        #region Is Request Pending
        /// <summary>
        /// Checks if there are any pending requests.
        /// </summary>
        /// <returns>True if there are pending requests, otherwise false.</returns>
        public bool IsRequestPending()
        {
            lock (this)
            {
                #region Step 1: Check if queue has requests
                // Step 1.1: Return true if there are requests in the queue, otherwise false
                return requests.Count > 0;
                #endregion
            }
        }
        #endregion

        #endregion

        #region Enum Definitions
        /// <summary>
        /// Identifies different types of requests for Revit operations.
        /// </summary>
        public enum RequestId
        {
            // i Will work With Show Dialog On this Plugin :D
            None
        }
        #endregion
    }
}
