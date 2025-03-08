using System.Windows;

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


        _viewModel = (ViewModels.FilterWarningsViewModel)this.DataContext;
        _viewModel.ClosingRequest += (sender, e) => this.Close();

    }

}
