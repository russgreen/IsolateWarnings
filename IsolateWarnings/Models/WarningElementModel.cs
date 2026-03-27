using Autodesk.Revit.DB;

namespace IsolateWarnings.Models;

internal class WarningElementModel
{
    public ElementId Id { get; set; } = ElementId.InvalidElementId;

    public string CategoryName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
