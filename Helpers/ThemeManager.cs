using Microsoft.Maui.Controls;

namespace AuthApp.Helpers;

public static class ThemeManager
{
    public const string DefaultTheme = "Midnight";
    public const string EmeraldTheme = "Emerald";
    public const string SunsetTheme = "Sunset";
    public const string CyberpunkTheme = "Cyberpunk";

    public static void InitializeTheme(Application app)
    {
        app.RequestedThemeChanged += (s, e) =>
        {
            ApplyTheme(app.Resources, Preferences.Get("selected_theme", DefaultTheme), e.RequestedTheme);
        };
        ApplyTheme(app.Resources, Preferences.Get("selected_theme", DefaultTheme), app.RequestedTheme);
    }

    public static void ApplyTheme(string themeName)
    {
        var app = Application.Current;
        if (app != null)
        {
            ApplyTheme(app.Resources, themeName, app.RequestedTheme);
        }
    }

    public static void ApplyTheme(ResourceDictionary resources, string themeName, AppTheme currentTheme)
    {
        Preferences.Set("selected_theme", themeName);
        if (resources == null) return;

        Color primary, primaryDark, primaryLight, secondary, accent, bgDark, bgDarker, surfaceDark;

        switch (themeName)
        {
            case EmeraldTheme:
                primary = Color.FromArgb("#10B981"); // Emerald
                primaryDark = Color.FromArgb("#059669");
                primaryLight = Color.FromArgb("#34D399");
                secondary = Color.FromArgb("#F59E0B"); // Amber
                accent = Color.FromArgb("#3B82F6"); // Blue
                bgDark = Color.FromArgb("#064E3B"); // Dark forest green
                bgDarker = Color.FromArgb("#022C22");
                surfaceDark = Color.FromArgb("#0F766E");
                break;
            case SunsetTheme:
                primary = Color.FromArgb("#EC4899"); // Pink/Rose
                primaryDark = Color.FromArgb("#DB2777");
                primaryLight = Color.FromArgb("#F472B6");
                secondary = Color.FromArgb("#F59E0B"); // Amber
                accent = Color.FromArgb("#8B5CF6"); // Purple
                bgDark = Color.FromArgb("#4C1D95"); // Dark violet
                bgDarker = Color.FromArgb("#2E1065");
                surfaceDark = Color.FromArgb("#6D28D9");
                break;
            case CyberpunkTheme:
                primary = Color.FromArgb("#06B6D4"); // Cyan
                primaryDark = Color.FromArgb("#0891B2");
                primaryLight = Color.FromArgb("#22D3EE");
                secondary = Color.FromArgb("#D946EF"); // Neon Fuchsia
                accent = Color.FromArgb("#F43F5E"); // Rose
                bgDark = Color.FromArgb("#0B0F19"); // Very dark slate
                bgDarker = Color.FromArgb("#020617"); // Darker slate
                surfaceDark = Color.FromArgb("#1E1B4B"); // Deep Indigo
                break;
            case DefaultTheme:
            default:
                primary = Color.FromArgb("#6366F1"); // Electric Indigo
                primaryDark = Color.FromArgb("#4F46E5");
                primaryLight = Color.FromArgb("#818CF8");
                secondary = Color.FromArgb("#F43F5E"); // Rose/Pink
                accent = Color.FromArgb("#10B981"); // Emerald
                bgDark = Color.FromArgb("#0F172A"); // Slate-800
                bgDarker = Color.FromArgb("#020617"); // Slate-950
                surfaceDark = Color.FromArgb("#1E293B");
                break;
        }

        // Apply core colors to resource dictionary
        resources["Primary"] = primary;
        resources["PrimaryDark"] = primaryDark;
        resources["PrimaryLight"] = primaryLight;
        resources["Secondary"] = secondary;
        resources["Accent"] = accent;
        resources["BgDark"] = bgDark;
        resources["BgDarker"] = bgDarker;
        resources["SurfaceDark"] = surfaceDark;

        // Recreate brushes dynamically so that XAML binds correctly
        resources["PrimaryBrush"] = new SolidColorBrush(primary);
        resources["PrimaryDarkBrush"] = new SolidColorBrush(primaryDark);
        resources["SurfaceDarkBrush"] = new SolidColorBrush(surfaceDark);
        resources["SecondaryBrush"] = new SolidColorBrush(secondary);

        // Determine if system theme is light or dark
        bool isDark = currentTheme == AppTheme.Dark || currentTheme == AppTheme.Unspecified;

        if (isDark)
        {
            resources["PageBackground"] = bgDark;
            resources["CardBackground"] = surfaceDark;
            resources["CardBorder"] = Color.FromArgb("#334155"); // BorderDark
            resources["TextPrimaryColor"] = Color.FromArgb("#F8FAFC");
            resources["TextSecondaryColor"] = Color.FromArgb("#94A3B8");

            resources["PageGradient"] = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = bgDark, Offset = 0.0f },
                    new GradientStop { Color = bgDarker, Offset = 1.0f }
                }
            };
        }
        else
        {
            resources["PageBackground"] = Color.FromArgb("#F1F5F9"); // BgLight
            resources["CardBackground"] = Color.FromArgb("#FFFFFF"); // SurfaceLight
            resources["CardBorder"] = Color.FromArgb("#E2E8F0"); // BorderLight
            resources["TextPrimaryColor"] = Color.FromArgb("#0F172A"); // TextPrimaryLight
            resources["TextSecondaryColor"] = Color.FromArgb("#64748B"); // TextSecondaryLight

            resources["PageGradient"] = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Color.FromArgb("#F8FAFC"), Offset = 0.0f },
                    new GradientStop { Color = Color.FromArgb("#E2E8F0"), Offset = 1.0f }
                }
            };
        }

        resources["AccentGradient"] = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            GradientStops = new GradientStopCollection
            {
                new GradientStop { Color = primary, Offset = 0.0f },
                new GradientStop { Color = secondary, Offset = 1.0f }
            }
        };
    }
}
