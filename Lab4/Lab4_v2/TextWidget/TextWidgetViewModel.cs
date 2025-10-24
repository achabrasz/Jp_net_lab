using System.ComponentModel.Composition;
using Contracts;
using Prism.Events;

namespace TextWidget;

[Export(typeof(IWidget))]
public class TextWidgetViewModel : IWidget
{
    public string Name => "Analizator Tekstu";
    public View View => _view;
    private readonly TextWidgetView _view = new();

    [ImportingConstructor]
    public TextWidgetViewModel(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<DataSubmittedEvent>().Subscribe(OnDataReceived);
    }

    private void OnDataReceived(string data)
    {
        MainThread.BeginInvokeOnMainThread(() => _view.Update(data));
    }
}