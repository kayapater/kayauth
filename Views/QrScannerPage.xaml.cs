using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AuthApp.Views;

public partial class QrScannerPage : ContentPage
{
    private bool _isProcessed = false;

    public QrScannerPage()
    {
        InitializeComponent();
        
        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessed) return;

        var first = e.Results.FirstOrDefault();
        if (first == null) return;

        _isProcessed = true; // Stop further processing

        Dispatcher.Dispatch(async () =>
        {
            // Turn off scanning to save battery/resources
            barcodeReader.IsDetecting = false;
            
            // Navigate back with the data
            await Shell.Current.GoToAsync($"..?qrCode={Uri.EscapeDataString(first.Value)}");
        });
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
