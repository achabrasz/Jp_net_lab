using Microsoft.Maui.Controls;

namespace ChartsWidget;

public class ChartsWidgetView : VerticalStackLayout
{
    private readonly VerticalStackLayout _bars = new() { Spacing = 12 };

    public ChartsWidgetView()
    {
        Children.Add(new Label { Text = "Wykres liczb", FontAttributes = FontAttributes.Bold });
        Children.Add(_bars);
    }

    public void Update(string input)
    {
        _bars.Children.Clear();
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int index = 1;
        foreach (var part in parts)
        {
            if (int.TryParse(part, out var value) && value > 0)
            {
                var barStack = new HorizontalStackLayout() { Spacing = 2, HorizontalOptions = LayoutOptions.Center };
                for (int i = 0; i < value; i++)
                    barStack.Children.Add(new BoxView
                    {
                        WidthRequest = value*2,
                        HeightRequest = 20,
                        Color = Colors.Blue
                    });

                var row = new HorizontalStackLayout() { Spacing = 8, HorizontalOptions = LayoutOptions.Center };
                row.Children.Add(new Label { Text = $"{value}", FontSize = 14, HorizontalTextAlignment = TextAlignment.Start });
                row.Children.Add(barStack);

                _bars.Children.Add(row);
                index++;
            }
        }
    }
}