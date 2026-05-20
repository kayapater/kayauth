using AuthApp.Helpers;

namespace AuthApp.Controls;

public class CircularProgressBar : GraphicsView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(CircularProgressBar), 0.0, 
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                var control = (CircularProgressBar)bindable;
                control.Drawer.Progress = (double)newVal;
                control.Invalidate();
            });

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public static readonly BindableProperty ProgressColorProperty =
        BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(CircularProgressBar), Color.FromArgb("#6366F1"),
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                var control = (CircularProgressBar)bindable;
                control.Drawer.ProgressColor = (Color)newVal;
                control.Invalidate();
            });

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    private CircularProgressDrawer Drawer => (CircularProgressDrawer)Drawable;

    public CircularProgressBar()
    {
        Drawable = new CircularProgressDrawer();
    }
}
