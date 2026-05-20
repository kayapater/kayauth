using AuthApp.ViewModels;
using AuthApp.Views;
using AuthApp.Models;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;

namespace AuthApp;

public partial class MainPage : ContentPage
{
	private readonly MainViewModel _viewModel;
	private readonly IServiceProvider _serviceProvider;
	private readonly HashSet<Guid> _animatedAccountIds = new();
	private bool _didAnimate;
	private bool _isAnimatingList;

	public MainPage(MainViewModel viewModel, IServiceProvider serviceProvider)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
		_serviceProvider = serviceProvider;

		_viewModel.OnRequestAddAccount += OnRequestAddAccount;
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	private async void OnRequestAddAccount(object? sender, EventArgs e)
	{
		var addAccountViewModel = _serviceProvider.GetRequiredService<AddAccountViewModel>();
		
		// If there is a scanned QR code, pass it to the popup VM
		if (!string.IsNullOrEmpty(_viewModel.QrCode))
		{
			addAccountViewModel.QrCodeResult = _viewModel.QrCode;
			_viewModel.QrCode = null; // Clear it so it doesn't persist
		}

		var popup = new AddAccountPopup(addAccountViewModel);
		await this.ShowPopupAsync(popup);
		
		// Refresh list after popup closes
		await _viewModel.InitializeAsync();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.InitializeAsync();
		await AnimateEntranceAsync();
	}

	private async Task AnimateEntranceAsync()
	{
		if (_didAnimate)
			return;

		_didAnimate = true;

		if (HeaderSection != null)
		{
			await Task.WhenAll(
				HeaderSection.FadeTo(1, 280, Easing.CubicOut),
				HeaderSection.TranslateTo(0, 0, 280, Easing.CubicOut));
		}

		if (SearchSection != null)
		{
			await Task.WhenAll(
				SearchSection.FadeTo(1, 260, Easing.CubicOut),
				SearchSection.TranslateTo(0, 0, 260, Easing.CubicOut));
		}

		if (ListSection != null)
		{
			await Task.WhenAll(
				ListSection.FadeTo(1, 240, Easing.CubicOut),
				ListSection.TranslateTo(0, 0, 240, Easing.CubicOut));
		}
	}

	private async void OnAccountItemLoaded(object sender, EventArgs e)
	{
		if (sender is not VisualElement element)
			return;

		if (element.BindingContext is not AuthenticatorAccount account)
			return;

		if (_animatedAccountIds.Contains(account.Id))
		{
			element.Opacity = 1;
			element.TranslationY = 0;
			return;
		}

		_animatedAccountIds.Add(account.Id);
		element.Opacity = 0;
		element.TranslationY = 12;

		await Task.WhenAll(
			element.FadeTo(1, 240, Easing.CubicOut),
			element.TranslateTo(0, 0, 240, Easing.CubicOut));
	}

	private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(MainViewModel.IsCompactMode))
		{
			await AnimateListSwitchAsync();
		}
	}

	private async Task AnimateListSwitchAsync()
	{
		if (_isAnimatingList || AccountsList == null)
			return;

		_isAnimatingList = true;
		await AccountsList.FadeTo(0.5, 120, Easing.CubicOut);
		await AccountsList.FadeTo(1, 180, Easing.CubicIn);
		_isAnimatingList = false;
	}

	private async void OnDeleteAccountTapped(object? sender, TappedEventArgs e)
	{
		AuthenticatorAccount? account = e.Parameter as AuthenticatorAccount;

		if (account is null && sender is BindableObject bindable)
		{
			account = bindable.BindingContext as AuthenticatorAccount;
		}

		if (account is null && sender is TapGestureRecognizer tap && tap.Parent is BindableObject parent)
		{
			account = parent.BindingContext as AuthenticatorAccount;
		}

		if (account is null)
			return;

		var answer = await DisplayAlert(
			"Sil",
			$"{account.Issuer} hesabını silmek istediğinize emin misiniz?",
			"Evet",
			"Hayır");

		if (!answer)
			return;

		await _viewModel.DeleteAccountAsync(account);
	}
}
