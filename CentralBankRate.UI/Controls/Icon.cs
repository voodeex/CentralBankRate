using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace CentralBankRate.UI.Controls;

public class Icon : TemplatedControl
{
    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<Icon, Geometry?>(nameof(Data));

    public Geometry? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
}
