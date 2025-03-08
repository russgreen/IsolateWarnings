using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace IsolateWarnings;

[Transaction(TransactionMode.Manual)]
public class CommandWarnings : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        App.RevitDocument = commandData.Application.ActiveUIDocument.Document;
        App.CachedUiApp = commandData.Application;

        var newView = new Views.FilterWarningsView();
        newView.ShowDialog();

        // Must return some code
        return Result.Succeeded;
    }
}
