using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using Meadow.Update;
using System.Threading.Tasks;

namespace F7FeatherDemo;

public class MeadowApp : App<F7FeatherV2>
{
    private Color color = Color.Green;

    private RgbPwmLed onboardLed;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize.....");

        onboardLed = new RgbPwmLed(
            redPwmPin: Device.Pins.OnboardLedRed,
            greenPwmPin: Device.Pins.OnboardLedGreen,
            bluePwmPin: Device.Pins.OnboardLedBlue,
            CommonType.CommonAnode);

        onboardLed.SetColor(color);

        return Task.CompletedTask;
    }

    private void OnUpdateStateChanged(object sender, UpdateState e)
    {
        Resolver.Log.Info($"UpdateService: {e}");
    }

    private void OnUpdateProgress(IUpdateService updateService, UpdateInfo info)
    {
        short percentage = (short)(((double)info.DownloadProgress / info.FileSize) * 100);

        Resolver.Log.Info($"Download Progress: {percentage}%");
    }

    private async void OnUpdateAvailable(IUpdateService updateService, UpdateInfo info)
    {
        _ = onboardLed.StartBlink(Color.Magenta);
        Resolver.Log.Info("Update available!");

        await Task.Delay(5000);
        updateService.RetrieveUpdate(info);
    }

    private async void OnUpdateRetrieved(IUpdateService updateService, UpdateInfo info)
    {
        _ = onboardLed.StartBlink(Color.Cyan);
        Resolver.Log.Info("Update retrieved!");

        await Task.Delay(5000);
        updateService.ApplyUpdate(info);
    }

    private void OnCloudStateChanged(object sender, CloudConnectionState e)
    {
        Resolver.Log.Info($"MeadowCloudService: {e}");
    }

    public override Task Run()
    {
        Resolver.Log.Info("Run...");

        var updateService = Resolver.UpdateService;

        updateService.ClearUpdates(); // uncomment to clear persisted info

        updateService.StateChanged += OnUpdateStateChanged;

        updateService.RetrieveProgress += OnUpdateProgress;

        updateService.UpdateAvailable += OnUpdateAvailable;

        updateService.UpdateRetrieved += OnUpdateRetrieved;

        var cloudService = Resolver.MeadowCloudService;

        cloudService.ConnectionStateChanged += OnCloudStateChanged;

        return Task.CompletedTask;
    }
}