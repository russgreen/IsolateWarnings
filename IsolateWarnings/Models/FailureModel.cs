using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsolateWarnings.Models;
internal partial class FailureModel : ObservableObject
{
    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _resolutionCaption = string.Empty;

    [ObservableProperty]
    private FailureSeverity _severity;

    [ObservableProperty]
    private List<WarningElementModel> _failingElements = new();

    /// <summary>
    /// The Revit failure-definition GUID for this warning, used to identify critical warnings.
    /// </summary>
    public Guid FailureDefinitionGuid { get; set; }

    /// <summary>
    /// Indicates whether this warning is considered critical based on the critical warnings list.
    /// </summary>
    [ObservableProperty]
    private bool _isCritical;

    /// <summary>
    /// A short explanation of why this warning is flagged as critical, or null if it is not critical.
    /// </summary>
    [ObservableProperty]
    private string _criticalReason;
}
