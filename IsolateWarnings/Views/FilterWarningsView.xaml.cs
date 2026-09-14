using Syncfusion.UI.Xaml.Grid;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace IsolateWarnings.Views;
/// <summary>
/// Interaction logic for FilterWarnings.xaml
/// </summary>
public partial class FilterWarningsView : Window
{
    private readonly ViewModels.FilterWarningsViewModel _viewModel;

    public FilterWarningsView()
    {
        InitializeComponent();

        var _ = new Microsoft.Xaml.Behaviors.DefaultTriggerAttribute(typeof(Trigger), typeof(Microsoft.Xaml.Behaviors.TriggerBase), null);


        _viewModel = new ViewModels.FilterWarningsViewModel();
        DataContext = _viewModel;
        _viewModel.ClosingRequest += (sender, e) => this.Close();

        BuildDataGrid();
    }

    private void BuildDataGrid()
    {
        var grid = new SfDataGrid
        {
            AutoGenerateColumns = false,
            AllowGrouping = true,
            AllowFiltering = true,
            AllowSorting = true,
            NavigationMode = NavigationMode.Row,
            SelectionMode = GridSelectionMode.Extended,
            RowSelectionBrush = new SolidColorBrush(Color.FromArgb(153, 147, 184, 217)),
            ColumnSizer = GridLengthUnitType.AutoWithLastColumnFill,
        };

        grid.SetBinding(SfDataGrid.ItemsSourceProperty, new Binding(nameof(_viewModel.FilteredFailures)));
        grid.SetBinding(SfDataGrid.SelectedItemsProperty, new Binding(nameof(_viewModel.SelectedFailures)) { Mode = BindingMode.TwoWay });

        grid.SortColumnDescriptions.Add(new SortColumnDescription { ColumnName = "IsCritical", SortDirection = ListSortDirection.Descending });
        grid.SortColumnDescriptions.Add(new SortColumnDescription { ColumnName = "Severity", SortDirection = ListSortDirection.Ascending });
        grid.SortColumnDescriptions.Add(new SortColumnDescription { ColumnName = "Description", SortDirection = ListSortDirection.Ascending });
        grid.GroupColumnDescriptions.Add(new GroupColumnDescription { ColumnName = "Description" });

        grid.Columns.Add(new GridCheckBoxSelectorColumn { MappingName = "SelectorColumn", HeaderText = string.Empty, Width = 34 });
        grid.Columns.Add(new GridTextColumn { MappingName = "Severity", HeaderText = "Severity", Width = 100, AllowEditing = false });
        grid.Columns.Add(new GridTextColumn { MappingName = "Description", HeaderText = "Description", AllowEditing = false });
        grid.Columns.Add(new GridTextColumn { MappingName = "ResolutionCaption", HeaderText = "Resolution", Width = 300, AllowEditing = false });

        var detailGrid = new SfDataGrid
        {
            AutoGenerateColumns = false,
            ColumnSizer = GridLengthUnitType.AutoWithLastColumnFill,
        };
        detailGrid.Columns.Add(new GridTextColumn { MappingName = "Id", HeaderText = "ID", Width = 100, AllowEditing = false });
        detailGrid.Columns.Add(new GridTextColumn { MappingName = "CategoryName", HeaderText = "Category", Width = 250, AllowEditing = false });
        detailGrid.Columns.Add(new GridTextColumn { MappingName = "Name", HeaderText = "Name", AllowEditing = false });

        var detailsView = new GridViewDefinition { RelationalColumn = "FailingElements", DataGrid = detailGrid };
        grid.DetailsViewDefinition.Add(detailsView);

        sfDataGridFailuresHost.Content = grid;
    }

}
