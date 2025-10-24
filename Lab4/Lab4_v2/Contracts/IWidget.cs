using Microsoft.Maui.Controls;

namespace Contracts;

public interface IWidget
{
    string Name { get; }
    View View { get; }
}