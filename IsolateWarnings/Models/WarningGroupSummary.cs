using Autodesk.Revit.DB;

namespace IsolateWarnings.Models;

internal class WarningGroupSummary
{
    public string Description { get; set; } = string.Empty;

    public int Count { get; set; }

    public FailureSeverity Severity { get; set; }

    public double BarWidthFraction { get; set; }

    /// <summary>
    /// Indicates whether any warning in this group is considered critical.
    /// </summary>
    public bool IsCritical { get; set; }
}
