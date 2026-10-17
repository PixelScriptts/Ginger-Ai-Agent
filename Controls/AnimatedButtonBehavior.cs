using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace AiAgent.Controls;

public class AnimatedButtonBehavior : Behavior<Button>
{
    private const uint AnimationDuration = 80;
    private Button? _button;

    protected override void OnAttachedTo(Button bindable)
    {
        base.OnAttachedTo(bindable);
        _button = bindable;
        bindable.Pressed += OnPressed;
        bindable.Released += OnReleased;
    }

    protected override void OnDetachingFrom(Button bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.Pressed -= OnPressed;
        bindable.Released -= OnReleased;
        _button = null;
    }

    private async void OnPressed(object? sender, System.EventArgs e) => await ScaleTo(0.96);
    private async void OnReleased(object? sender, System.EventArgs e) => await ScaleTo(1.0);

    private async Task ScaleTo(double scale)
    {
        if (_button == null) return;
        try
        {
            await _button.ScaleTo(scale, AnimationDuration, Easing.CubicOut);
        }
        catch { }
    }
}
