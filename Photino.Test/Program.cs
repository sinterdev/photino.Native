﻿using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace PhotinoNET
{
    class Program
    {
        private static readonly bool _logEvents = true;
        private static int _windowNumber = 1;

        private static PhotinoWindow mainWindow;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                //FluentStyle();
                PropertyInitStyle();
            }
            catch (Exception ex)
            {
                Log(null, ex.Message);
                Console.ReadKey();
            }
        }


        private static void FluentStyle()
        {
            mainWindow = new PhotinoWindow()
                .SetIconFile("wwwroot/photino-logo.ico")
                .SetTitle($"My Photino Window {_windowNumber++}")

                //.Load(new Uri("https://google.com"))
                .Load("wwwroot/main.html")
                //.LoadRawString("<h1>Hello Photino!</h1>")

                .SetChromeless(true)
                //.SetFullScreen(true)
                //.SetMaximized(true)
                //.SetMinimized(true)
                //.SetResizable(false)
                //.SetTopMost(true)
                .SetUseOsDefaultLocation(false)
                .SetUseOsDefaultSize(false)
                //.SetZoom(150)

                //.SetContextMenuEnabled(false)
                //.SetDevToolsEnabled(false)
                //.SetGrantBrowserPermissions(false)

                //.Center()
                //.SetSize(new Size(800, 600))
                .SetHeight(600)
                .SetWidth(800)
                //.SetLocation(new Point(50, 50))
                //.SetTop(50)
                //.SetLeft(50)
                //.MoveTo(new Point(10, 10))
                //.MoveTo(20, 20)
                //.Offset(new Point(150, 150))
                //.Offset(250, 250)

                .RegisterCustomSchemeHandler("app", AppCustomSchemeUsed)

                .RegisterWindowCreatingHandler(WindowCreating)
                .RegisterWindowCreatedHandler(WindowCreated)
                .RegisterLocationChangedHandler(WindowLocationChanged)
                .RegisterSizeChangedHandler(WindowSizeChanged)
                .RegisterWebMessageReceivedHandler(MessageReceivedFromWindow)
                .RegisterWindowClosingHandler(WindowIsClosing)

                //.SetTemporaryFilesPath(@"C:\Temp")

                .SetLogVerbosity(_logEvents ? 2 : 0);


            mainWindow.WaitForClose();

            mainWindow.Center(); //will never happen - this is blocking.
        }

        private static void PropertyInitStyle()
        {
            mainWindow = new PhotinoWindow
            {
                IconFile = "wwwroot/photino-logo.png",
                Title = $"My Photino Window {_windowNumber++}",

                StartUrl = "wwwroot/main.html",
                //StartString = "<h1>Hello Photino!</h1>",

                Centered = true,
                //Chromeless = true,
                //FullScreen = true,
                //Maximized = true,
                //Minimized = true,
                //Resizable = false,
                //TopMost = true,
                //UseOsDefaultLocation = false,
                UseOsDefaultSize = false,
                //Zoom = 300,

                //ContextMenuEnabled = false,
                //DevToolsEnabled = false,
                //GrantBrowserPermissions = false,

                //CenterOnInitialize = true,
                //Size = new Size(800, 600),
                Height = 600,
                Width = 800,
                //Location = new Point(50, 50),
                //Top = 50,
                //Left = 50,

                WindowCreatingHandler = WindowCreating,
                WindowCreatedHandler = WindowCreated,
                WindowLocationChangedHandler = WindowLocationChanged,
                WindowSizeChangedHandler = WindowSizeChanged,
                WebMessageReceivedHandler = MessageReceivedFromWindow,
                WindowClosingHandler = WindowIsClosing,

                //TemporaryFilesPath = @"C:\Temp",

                LogVerbosity = _logEvents ? 2 : 0,
            };

            //Can this be done with a property? 
            mainWindow.RegisterCustomSchemeHandler("app", AppCustomSchemeUsed);

            mainWindow.WaitForClose();

            Console.WriteLine("Done Blocking!");
        }



        //These are the event handlers I'm hooking up
        private static Stream AppCustomSchemeUsed(object sender, string scheme, string url, out string contentType)
        {
            Log(sender, $"Custom scheme '{scheme}' was used.");
            var currentWindow = sender as PhotinoWindow;

            contentType = "text/javascript";

            var js =
@"
(() =>{
    window.setTimeout(() => {
        const title = document.getElementById('Title');
        const lineage = document.getElementById('Lineage');
        title.innerHTML = "

            + $"'{currentWindow.Title}';" + "\n"

            + $"        lineage.innerHTML = `PhotinoWindow Id: {currentWindow.Id} <br>`;" + "\n"; 

            //show lineage of this window
            var p = currentWindow.Parent;
            while (p != null)
            {
                js += $"        lineage.innerHTML += `Parent Id: {p.Id} <br>`;" + "\n";
                p = p.Parent;
            }

            js +=
@"        alert(`🎉 Dynamically inserted JavaScript.`);
    }, 1000);
})();
";

            return new MemoryStream(Encoding.UTF8.GetBytes(js));
        }

        private static void MessageReceivedFromWindow(object sender, string message)
        {
            Log(sender, $"MessageRecievedFromWindow Callback Fired.");

            var currentWindow = sender as PhotinoWindow;
            if (string.Compare(message, "child-window", true) == 0)
            {
                var x = new PhotinoWindow(currentWindow)
                    .SetTitle($"Child Window {_windowNumber++}")
                    .Load("wwwroot/main.html")

                    .SetUseOsDefaultLocation(true)
                    .SetHeight(600)
                    .SetWidth(800)

                    .RegisterWindowCreatingHandler(WindowCreating)
                    .RegisterWindowCreatedHandler(WindowCreated)
                    .RegisterLocationChangedHandler(WindowLocationChanged)
                    .RegisterSizeChangedHandler(WindowSizeChanged)
                    .RegisterWebMessageReceivedHandler(MessageReceivedFromWindow)
                    .RegisterWindowClosingHandler(WindowIsClosing)

                    .RegisterCustomSchemeHandler("app", AppCustomSchemeUsed)

                    .SetTemporaryFilesPath(currentWindow.TemporaryFilesPath)
                    .SetLogVerbosity(_logEvents ? 2 : 0);

                x.WaitForClose();

                //x.Center();           //WaitForClose() is non-blocking for child windows
                //x.SetHeight(800);
                //x.Close();
            }
            else if (string.Compare(message, "zoom-in", true) == 0)
            {
                currentWindow.Zoom += 5;
                Log(sender, $"Zoom: {currentWindow.Zoom}");
            }
            else if (string.Compare(message, "zoom-out", true) == 0)
            {
                currentWindow.Zoom -= 5;
                Log(sender, $"Zoom: {currentWindow.Zoom}");
            }
            else if (string.Compare(message, "center", true) == 0)
            {
                currentWindow.Center();
            }
            else if (string.Compare(message, "close", true) == 0)
            {
                currentWindow.Close();
            }
            else
                throw new Exception($"Unknown message '{message}'");
        }

        private static void WindowCreating(object sender, EventArgs e)
        {
            Log(sender, "WindowCreating Callback Fired.");
        }

        private static void WindowCreated(object sender, EventArgs e)
        {
            Log(sender, "WindowCreated Callback Fired.");
        }

        private static void WindowLocationChanged(object sender, Point location)
        {
            Log(sender, $"WindowLocationChanged Callback Fired.  Left: {location.X}  Top: {location.Y}");
        }

        private static void WindowSizeChanged(object sender, Size size)
        {
            Log(sender, $"WindowSizeChanged Callback Fired.  Height: {size.Height}  Width: {size.Width}");
        }

        private static bool WindowIsClosing(object sender, EventArgs e)
        {
            Log(sender, "WindowIsClosing Callback Fired.");
            return false;   //return true to block closing of the window
        }




        private static void Log(object sender, string message)
        {
            if (!_logEvents) return;
            var currentWindow = sender as PhotinoWindow;
            var windowTitle = currentWindow == null ? string.Empty : currentWindow.Title;
            Console.WriteLine($"-Client App: \"{windowTitle ?? "title?"}\" {message}");
        }
    }
}
