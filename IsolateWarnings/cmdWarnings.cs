using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IsolateWarnings;

[Transaction(TransactionMode.Manual)]
public class cmdWarnings : IExternalCommand
{
    private UIApplication _uiapp;
    private UIDocument _uidoc;
    private Document _doc;

    private IList<FailureMessage> _warnings;
    private List<ElementId> _elementIds = new List<ElementId>();

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        _uiapp = commandData.Application;
        _uidoc = _uiapp.ActiveUIDocument;
        _doc = _uidoc.Document;

        ShowWarningElements();

        // Must return some code
        return Result.Succeeded;
    }

    private void ShowWarningElements()
    {
        try
        { 
            _warnings = _doc.GetWarnings();

            foreach (FailureMessage warning in _warnings)
            {
                _elementIds.AddRange(warning.GetFailingElements());
            }

            if (_elementIds.Count != 0)
            {
                Create3DView(ref _doc, "IsolateWarningElements");
                Create3DView(ref _doc, "OverrideWarningElements");
                var isolateView = new FilteredElementCollector(_doc).OfClass(typeof(View)).Cast<View>().Where(q => q.Name == "IsolateWarningElements").FirstOrDefault();
                var overrideView = new FilteredElementCollector(_doc).OfClass(typeof(View)).Cast<View>().Where(q => q.Name == "OverrideWarningElements").FirstOrDefault();

                // open the views
                var tmpvw = _uidoc.ActiveView;
                _uidoc.ActiveView = isolateView;
                _uidoc.ActiveView = overrideView;
                _uidoc.ActiveView = tmpvw;
                foreach (var vw in _uidoc.GetOpenUIViews())
                {
                    if (vw.ViewId == isolateView.Id | vw.ViewId == overrideView.Id)
                    {
                        vw.ZoomToFit();
                    }
                    // do nothing
                    else
                    {
                        //vw.Close();
                    }
                }

                //foreach (var vw in _uidoc.GetOpenUIViews())
                //{
                //    vw.ZoomToFit();
                //}


                var ogsClear = new OverrideGraphicSettings();
                var ogs = new OverrideGraphicSettings();
                var red = new Color(255, 0, 0);
                ogs.SetProjectionLineColor(red);
                ogs.SetProjectionLineWeight(8);
                var fillPatternElements = new FilteredElementCollector(_doc).OfClass(typeof(FillPatternElement)).OfType<FillPatternElement>().OrderBy(fp => fp.Name).ToList();
                var fillPatterns = fillPatternElements.Select(fpe => fpe.GetFillPattern());
                string SolidName = "Solid";
                foreach (FillPattern fp in fillPatterns)
                {
                    if (fp.IsSolidFill == true)
                    {
                        // we have the solid fill
                        SolidName = fp.Name;
                    }
                }

                var solidFill = new FilteredElementCollector(_doc).OfClass(typeof(FillPatternElement)).Where(q => q.Name.Contains(SolidName)).First();


#if REVIT2018
                ogs.SetProjectionFillPatternId(solidFill.Id);
                ogs.SetProjectionFillColor(new Color(0, 255, 0));
#else
                ogs.SetSurfaceForegroundPatternId(solidFill.Id);
                ogs.SetSurfaceForegroundPatternColor(new Color(0, 255, 0));
#endif

                using (var t = new Transaction(_doc, "Unisolate/Override Warnings"))
                {
                    t.Start();
                    // get all the elements in the view so we can be sure they are not hidden or graphically hidden
                    var viewElems = new FilteredElementCollector(_doc, overrideView.Id).WhereElementIsNotElementType();
                    IList<ElementId> idsall = new List<ElementId>();
                    foreach (var elem in viewElems)
                    {
                        idsall.Add(elem.Id);
                        overrideView.SetElementOverrides(elem.Id, ogsClear);
                    }

                    isolateView.UnhideElements(idsall);
                    isolateView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                    t.Commit();
                }

                using (var t = new Transaction(_doc, "Isolate/Override Warnings"))
                {
                    t.Start();
                    isolateView.IsolateElementsTemporary(_elementIds);
                    foreach (ElementId id in _elementIds)
                    {
                        overrideView.SetElementOverrides(id, ogs);
                    }

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
            TaskDialog.Show("Isolate Warnings", ex.Message + Environment.NewLine + ex.Source + Environment.NewLine + ex.HelpLink + Environment.NewLine + ex.StackTrace, TaskDialogCommonButtons.Ok);
        }

    }

    private void Create3DView(ref Document document, string viewname)
    {
        if (ViewExists(viewname) == true)
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

        using (var t = new Transaction(_doc, "Create 3D View"))
        {
            t.Start();
            // Create a new View3D
            var view3D = View3D.CreateIsometric(document, viewFamilyTypes.First().Id);
            if (view3D is object)
            {
                view3D.Name = viewname;
            }

            t.Commit();
        }
    }

    private bool ViewExists(string ViewName)
    {
        bool retval = false;
        IList<View3D> viewList = new FilteredElementCollector(_doc).OfClass(typeof(View3D)).Cast<View3D>().Where(v => v.IsTemplate == false).ToList();
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
