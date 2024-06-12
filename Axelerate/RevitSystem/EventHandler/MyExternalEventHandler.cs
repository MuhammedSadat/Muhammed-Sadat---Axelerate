using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axelerate.RevitSystem.EventHandler.RequestHandler;

namespace Axelerate.RevitSystem.EventHandler
{
    public class MyExternalEventHandler : IExternalEventHandler
    {
        public RequestHandler RequestHandler { get; set; }

        public void Execute(UIApplication app)
        {
            RequestId request = RequestHandler.TakeRequest();
            switch (request)
            {
                // i will work with Show dialog in this Plugin // Muhammed Sadat
                case RequestId.None:
                    break;
            }
        }

        public string GetName()
        {
            return "My Custom Event Handler";
        }


    }
}
