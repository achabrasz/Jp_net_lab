using Microsoft.Maui.Controls;

namespace TextWidget;

public class TextWidgetView : VerticalStackLayout
{
    private readonly Label _chars = new() { Text = "Znaki: 0" };
    private readonly Label _words = new() { Text = "Słowa: 0" };

    public TextWidgetView()
    {
        Children.Add(new Label { Text = "Analizator Tekstu", FontAttributes = FontAttributes.Bold });
        Children.Add(_chars);
        Children.Add(_words);
    }

    public void Update(string text)
    {
        var chars = text.Length;
        var words = string.IsNullOrWhiteSpace(text) ? 0 : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        _chars.Text = $"Znaki: {chars}";
        _words.Text = $"Słowa: {words}";
    }
}