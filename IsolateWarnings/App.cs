using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace IsolateWarnings;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
class App : IExternalApplication
{
    public static UIControlledApplication cachedUiCtrApp;

    private readonly string _tabName = "RG Tools";

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

    public Result OnStartup(UIControlledApplication application)
    {
        cachedUiCtrApp = application;
        var ribbonPanel = CreateRibbonPanel();

        return Result.Succeeded;
    }

    private RibbonPanel CreateRibbonPanel()
    {
        RibbonPanel panel;

        //Check if "Archisoft Tools" already exists and use if its there
        try
        {
            panel = cachedUiCtrApp.CreateRibbonPanel(_tabName, Guid.NewGuid().ToString());
            panel.Name = "ARBG_IsolateWarnings_ExtApp";
            panel.Title = "Isolate Warnings";
        }
        catch
        {
            var archisoftPanel = false;
            var pluginPath = @"C:\ProgramData\Autodesk\ApplicationPlugins";
            if (System.IO.Directory.Exists(pluginPath) == true)
            {
                foreach (var folder in System.IO.Directory.GetDirectories(pluginPath))
                {
                    if (folder.ToLower().Contains("archisoft") == true & folder.ToLower().Contains("archisoft isolate warnings") == false)
                    {
                        archisoftPanel = true;
                        break;
                    }

                    if (folder.ToLower().Contains("rg") == true & folder.ToLower().Contains("rg isolate warnings") == false)
                    {
                        archisoftPanel = true;
                        break;
                    }
                }
            }

            if (archisoftPanel == true)
            {
                cachedUiCtrApp.CreateRibbonTab(_tabName);
                panel = cachedUiCtrApp.CreateRibbonPanel(_tabName, Guid.NewGuid().ToString());
                panel.Name = "ARBG_IsolateWarnings_ExtApp";
                panel.Title = "Isolate Warnings";
            }
            else
            {
                panel = cachedUiCtrApp.CreateRibbonPanel("Isolate Warnings");
            }
        }

        PushButtonData pbData = new PushButtonData("Isolate Warnings", "Isolate Warnings", Assembly.GetExecutingAssembly().Location, "IsolateWarnings.cmdWarnings");
        PushButton pb = (PushButton)panel.AddItem(pbData);
        pb.ToolTip = "Isolate elements with warnings in Revit";
        pb.LargeImage = PngImageSource("IsolateWarnings.Images.Warnings32.png");


        ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, @"https://github.com/russgreen/IsolateWarnings/wiki");
        pb.SetContextualHelp(contextHelp);

        return panel;
    }

    private System.Windows.Media.ImageSource PngImageSource(string embeddedPath)
    {
        var stream = GetType().Assembly.GetManifestResourceStream(embeddedPath);
        System.Windows.Media.ImageSource imageSource;
        try
        {
            imageSource = BitmapFrame.Create(stream);
        }
        catch
        {
            imageSource = null;
        }

        return imageSource;
    }
}
