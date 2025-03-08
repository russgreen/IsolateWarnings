using Autodesk.Revit.ApplicationServices;
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
    public static UIControlledApplication CachedUiCtrApp;
    public static UIApplication CachedUiApp;
    public static ControlledApplication CtrApp;

    public static Autodesk.Revit.DB.Document RevitDocument;

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

    public Result OnStartup(UIControlledApplication application)
    {
        CachedUiCtrApp = application;
        CtrApp = application.ControlledApplication;

        var ribbonPanel = CreateRibbonPanel();

        return Result.Succeeded;
    }

    private RibbonPanel CreateRibbonPanel()
    {
        RibbonPanel panel;
        panel = CachedUiCtrApp.CreateRibbonPanel(nameof(IsolateWarnings));
        panel.Title = "Warnings";
 
        PushButton pushButton = (PushButton)panel.AddItem(
            new PushButtonData("Isolate Warnings", 
            $"Isolate{System.Environment.NewLine}Warnings", 
            Assembly.GetExecutingAssembly().Location,
            $"{nameof(IsolateWarnings)}.{nameof(IsolateWarnings.CommandWarnings)}"));
        pushButton.ToolTip = "Isolate elements with warnings in Revit";
        pushButton.LargeImage = PngImageSource("IsolateWarnings.Images.Warnings32.png");


        ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, @"https://github.com/russgreen/IsolateWarnings/wiki");
        pushButton.SetContextualHelp(contextHelp);

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
