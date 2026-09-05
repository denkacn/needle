using Avalonia.Controls;

namespace Needle.Avalonia.Views;

internal sealed class StartupOverlayController
{
    private const int InitialDelayMilliseconds = 220;
    private const int FadeStepDelayMilliseconds = 16;
    private const int FadeSteps = 14;

    private readonly Control _overlay;

    public StartupOverlayController(Control overlay)
    {
        _overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
    }

    public async Task FadeOutAsync()
    {
        await Task.Delay(InitialDelayMilliseconds);

        for (var step = 0; step <= FadeSteps; step++)
        {
            _overlay.Opacity = 1d - step / (double)FadeSteps;
            await Task.Delay(FadeStepDelayMilliseconds);
        }

        _overlay.IsVisible = false;
    }
}
