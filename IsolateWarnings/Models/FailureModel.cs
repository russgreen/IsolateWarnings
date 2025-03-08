using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IsolateWarnings.Models;
internal partial class FailureModel : ObservableObject
{
    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _resolutionCaption;

    [ObservableProperty]
    private FailureSeverity _severity;

    [ObservableProperty]
    private List<Element> _failingElements = new();

}
