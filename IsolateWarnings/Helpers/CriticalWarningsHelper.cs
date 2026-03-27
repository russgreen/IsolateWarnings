using System;
using System.Collections.Generic;

namespace ECETools.Services;

/// <summary>
/// Provides information about which Revit warnings are considered critical.
/// The list of critical warning GUIDs is sourced from:
/// https://github.com/pyrevitlabs/pyRevit/blob/develop/extensions/pyRevitTools.extension/pyRevit.tab/Project.panel/Preflight%20Checks.pushbutton/critical_warnings.csv
/// </summary>
public static class CriticalWarningsHelper
{
    // Maps failure-definition GUIDs to a short explanation of why the warning is critical.
    // Source: pyRevit critical_warnings.csv
    private static readonly Dictionary<Guid, string> _criticalWarnings = new()
    {
        // Elements have duplicate "Number" / "Type Mark" / "Mark" values
        [new Guid("6e1efefe-c8e0-483d-8482-150b9f1da21a")] =
            "Duplicate parameter values can cause data-export and scheduling errors.",

        // There are identical instances in the same place
        [new Guid("b4176cef-6086-45a8-a066-c3fd424c9412")] =
            "Identical instances in the same place cause incorrect quantities and model bloat.",

        // Room Tag is outside of its Room
        [new Guid("4f0bba25-e17f-480a-a763-d97d184be18a")] =
            "A room tag outside its room produces incorrect area schedules and room data.",

        // One element is completely inside another
        [new Guid("505d84a1-67e4-4987-8287-21ad1792ffe9")] =
            "An element completely inside another may indicate duplicated geometry or modelling errors.",

        // Highlighted floors overlap
        [new Guid("8695a52f-2a88-4ca2-bedc-3676d5857af6")] =
            "Overlapping floors cause incorrect area calculations and structural analysis errors.",

        // Room is not in a properly enclosed region
        [new Guid("ce3275c6-1c51-402e-8de3-df3a3d566f5c")] =
            "An unenclosed room will report incorrect area and volume data.",

        // Multiple Rooms are in the same enclosed region
        [new Guid("83d4a67c-818c-4291-adaf-f2d33064fea8")] =
            "Multiple rooms in the same region produce duplicate area data in schedules.",

        // Area is not in a properly enclosed region
        [new Guid("e4d98f16-24ac-4cbe-9d83-80245cf41f0a")] =
            "An unenclosed area boundary will produce incorrect area calculations.",

        // Room separation line is slightly off axis and may cause inaccuracies
        [new Guid("f657364a-e0b7-46aa-8c17-edd8e59683b9")] =
            "An off-axis room separation line can lead to boundary closure failures and inaccurate areas.",
    };

    /// <inheritdoc/>
    public static bool IsCritical(Guid failureDefinitionGuid) =>
        _criticalWarnings.ContainsKey(failureDefinitionGuid);

    /// <inheritdoc/>
    public static string? GetCriticalReason(Guid failureDefinitionGuid) =>
        _criticalWarnings.TryGetValue(failureDefinitionGuid, out var reason) ? reason : null;
}
