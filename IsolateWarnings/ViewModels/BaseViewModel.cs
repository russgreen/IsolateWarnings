using CommunityToolkit.Mvvm.ComponentModel;

namespace IsolateWarnings.ViewModels;

public class BaseViewModel : ObservableValidator
{
    public event EventHandler ClosingRequest;

    protected void OnClosingRequest()
    {
        if (this.ClosingRequest != null)
        {
            ClosingRequest(this, EventArgs.Empty);
        }
    }

}
