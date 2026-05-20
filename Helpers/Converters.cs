using System.Globalization;

namespace AuthApp.Helpers;

public class ProgressToSecondsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress)
        {
            return Math.Max(0, (int)Math.Round(progress * 30)).ToString();
        }
        return "0";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ProgressToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color normalColor = Color.FromArgb("#6366F1"); // Default Electric Indigo
        Color warningColor = Color.FromArgb("#EF4444"); // Default Error Red

        if (Application.Current != null)
        {
            if (Application.Current.Resources.TryGetValue("Primary", out var pColor) && pColor is Color c1)
                normalColor = c1;
            if (Application.Current.Resources.TryGetValue("Error", out var eColor) && eColor is Color c2)
                warningColor = c2;
        }

        if (value is double progress)
        {
            // If less than 5 seconds remaining (5/30 = 0.166), turn red
            return progress < 0.17 ? warningColor : normalColor;
        }
        return normalColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
