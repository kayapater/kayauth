using CommunityToolkit.Maui.Views;
using AuthApp.ViewModels;

namespace AuthApp.Views;

public partial class AddAccountPopup : Popup
{
	private bool _isClosing;

	public AddAccountPopup(AddAccountViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;

		// Close popup when requested by ViewModel
		viewModel.OnRequestClose += async (s, e) => await CloseWithAnimationAsync();
	}

	private async void OnPopupLoaded(object sender, EventArgs e)
	{
		if (SheetRoot == null)
			return;

		SheetRoot.Opacity = 0;
		SheetRoot.TranslationY = 30;

		await Task.WhenAll(
			SheetRoot.FadeTo(1, 220, Easing.CubicOut),
			SheetRoot.TranslateTo(0, 0, 220, Easing.CubicOut));
	}

	private async Task CloseWithAnimationAsync()
	{
		if (_isClosing)
			return;

		_isClosing = true;

		if (SheetRoot != null)
		{
			await Task.WhenAll(
				SheetRoot.FadeTo(0, 180, Easing.CubicIn),
				SheetRoot.TranslateTo(0, 30, 180, Easing.CubicIn));
		}

		Close();
	}
}
