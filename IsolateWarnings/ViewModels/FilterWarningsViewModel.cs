using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECETools.Services;
using IsolateWarnings.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsolateWarnings.ViewModels;
internal partial class FilterWarningsViewModel : BaseViewModel
{

    private IList<FailureMessage> _warnings = [];

    [ObservableProperty]
    private IList<FailureModel> _failures = [];

    [ObservableProperty]
    private IList<FailureModel> _filteredFailures = [];

    [ObservableProperty]
    private ObservableCollection<object> _selectedFailures = new();

    private List<ElementId> _elementIds = new();

    [ObservableProperty]
    private System.Windows.Visibility _isWindowVisible;

    [ObservableProperty]
    private string _buttonText = "Isolate All Warnings";

    [ObservableProperty]
    private bool _isCommandEnabled = false;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private int _warningCount;

    [ObservableProperty]
    private int _criticalCount;

    [ObservableProperty]
    private bool _showCriticalOnly = false;

    [ObservableProperty]
    private IList<WarningGroupSummary> _topWarningGroups = [];

    public FilterWarningsViewModel()
    {
        LoadDocumentWarnings();

        SelectedFailures = new();
        SelectedFailures.CollectionChanged += SelectedFailures_CollectionChanged;
    }

    partial void OnShowCriticalOnlyChanged(bool value)
    {
        ApplyFilter(value);
    }

    private void ApplyFilter(bool criticalOnly)
    {
        FilteredFailures = criticalOnly
            ? Failures.Where(f => f.IsCritical).ToList()
            : Failures.ToList();
    }

    private void LoadDocumentWarnings()
    {
        _warnings = App.RevitDocument.GetWarnings();
        Failures = new List<FailureModel>();

        foreach (var warning in _warnings)
        {
            string resolution;
            try
            {
                resolution = warning.GetDefaultResolutionCaption();
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
            {
                //_logger.LogDebug(ex, "Failed to get default resolution caption for warning: {description}", warning.GetDescriptionText());
                resolution = "No resolution available";
            }

            var guid = warning.GetFailureDefinitionId().Guid;
            var isCritical = CriticalWarningsHelper.IsCritical(guid);

            var failure = new FailureModel
            {
                Description = warning.GetDescriptionText(),
                ResolutionCaption = resolution,
                Severity = warning.GetSeverity(),
                FailureDefinitionGuid = guid,
                IsCritical = isCritical,
                CriticalReason = CriticalWarningsHelper.GetCriticalReason(guid)
            };

            failure.FailingElements.AddRange(
                warning.GetFailingElements()
                       .Select(elementId => App.RevitDocument.GetElement(elementId))
                       .Where(element => element != null)
                       .Select(element => new WarningElementModel
                       {
                           Id = element!.Id,
                           CategoryName = element.Category?.Name ?? string.Empty,
                           Name = element.Name
                       })
            );
            Failures.Add(failure);
        }

        if(Failures.Count > 0)
        {
            IsCommandEnabled = true;
        }

        TotalCount = Failures.Count;
        ErrorCount = Failures.Count(f => f.Severity == FailureSeverity.Error);
        WarningCount = Failures.Count(f => f.Severity == FailureSeverity.Warning);
        CriticalCount = Failures.Count(f => f.IsCritical);

        var groups = Failures
            .GroupBy(f => f.Description)
            .Select(g => new WarningGroupSummary
            {
                Description = g.Key,
                Count = g.Count(),
                Severity = g.Max(f => f.Severity),
                IsCritical = g.Any(f => f.IsCritical)
            })
            .OrderByDescending(g => g.IsCritical)
            .ThenByDescending(g => g.Count)
            .Take(10)
            .ToList();

        var maxCount = groups.Count > 0 ? groups.Max(g => g.Count) : 1;
        if (groups.Count > 0)
        {
            foreach (var group in groups)
            {
                group.BarWidthFraction = (double)group.Count / maxCount;
            }
        }

        TopWarningGroups = groups;
        ApplyFilter(ShowCriticalOnly);
    }

    private void SelectedFailures_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ButtonText = SelectedFailures.Count > 0 ? "Isolate Selected Warnings" : "Isolate All Warnings";
    }

    [RelayCommand]
    private void IsolateWarnings()
    {
        _elementIds.Clear();

        var failuresToProcess = SelectedFailures.Count > 0 ? SelectedFailures.Cast<FailureModel>() : Failures;

        foreach (var failure in failuresToProcess)
        {
            foreach (var element in failure.FailingElements)
            {
                _elementIds.Add(element.Id);
            }
        }

        IsolateWarningsElements();

        this.OnClosingRequest();
        return;
    }

    private void IsolateWarningsElements()
    {
        try
        {
            if (_elementIds.Count != 0)
            {
                using (var t = new Transaction(App.RevitDocument, "Create 3D View"))
                {
                    t.Start();

                    Create3DView(ref App.RevitDocument, "IsolateWarningElements");

                    t.Commit();
                }

                var isolateView = new FilteredElementCollector(App.RevitDocument)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(q => q.Name == "IsolateWarningElements")
                    .FirstOrDefault();

                // open the view
                if (isolateView == null)
                {
                    return;
                }

                App.CachedUiApp.ActiveUIDocument.ActiveView = isolateView;

                foreach (var vw in App.CachedUiApp.ActiveUIDocument.GetOpenUIViews())
                {
                    if (vw.ViewId == isolateView.Id)
                    {
                        vw.ZoomToFit();
                    }
                }

                using (var t = new Transaction(App.RevitDocument, "Isolate/Override Warnings"))
                {
                    t.Start();
                    isolateView.TemporaryViewModes.DeactivateAllModes();
                    isolateView.IsolateElementsTemporary(_elementIds);
                    t.Commit();
                }
            }
            else
            {
                TaskDialog.Show("Warnings", "No warnings found in the current model", TaskDialogCommonButtons.Ok);
            }


        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error isolating warnings");
            TaskDialog.Show("Isolate Warnings", ex.Message + Environment.NewLine + ex.Source + Environment.NewLine + ex.HelpLink + Environment.NewLine + ex.StackTrace, TaskDialogCommonButtons.Ok);
        }

    }

    private void Create3DView(ref Document document, string viewName)
    {
        if (ViewExists(document, viewName) == true)
        {
            return;
        }

        // Find a 3D view type
        var collector1 = new FilteredElementCollector(document);
        collector1 = collector1.OfClass(typeof(ViewFamilyType));
        IEnumerable<ViewFamilyType> viewFamilyTypes;
        viewFamilyTypes = from elem in collector1
                          let vftype = elem as ViewFamilyType
                          where vftype.ViewFamily == ViewFamily.ThreeDimensional
                          select vftype;

        var view3D = View3D.CreateIsometric(document, viewFamilyTypes.First().Id);
        if (view3D is not null)
        {
            view3D.Name = viewName;
        }
    }

    private bool ViewExists(Document document, string ViewName)
    {
        bool retval = false;
        IList<View3D> viewList = new FilteredElementCollector(document)
            .OfClass(typeof(View3D))
            .Cast<View3D>()
            .Where(v => v.IsTemplate == false)
            .ToList();

        foreach (View3D view in viewList)
        {
            if ((view.Name ?? "") == (ViewName ?? ""))
            {
                retval = true;
            }
        }

        return retval;
    }
}
