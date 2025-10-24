using System.ComponentModel.Composition;
using Contracts;
using Prism.Events;

namespace ChartsWidget;

[Export(typeof(IWidget))]
public class ChartsWidgetViewModel : IWidget
{
    public string Name => "Wykres liczb";
    public View View => _view;
    private readonly ChartsWidgetView _view = new();

    [ImportingConstructor]
    public ChartsWidgetViewModel(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<DataSubmittedEvent>().Subscribe(OnDataReceived);
    }

    private void OnDataReceived(string data)
    {
        MainThread.BeginInvokeOnMainThread(() => _view.Update(data));
    }
}