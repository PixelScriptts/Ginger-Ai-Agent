using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace AiAgent.Pages;

public class BaseAnimatedPage : ContentPage
{
    private const uint EnterDuration = 300;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = RunEnterAnimationAsync();
    }

    private async Task RunEnterAnimationAsync()
    {
        try
        {
            this.Opacity = 0;
            this.TranslationY = 12;
            await Task.WhenAll(this.FadeTo(1, EnterDuration, Easing.CubicOut), this.TranslateTo(0, 0, EnterDuration, Easing.CubicOut));
        }
        catch { }
    }
}
