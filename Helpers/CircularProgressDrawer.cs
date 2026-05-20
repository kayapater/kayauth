namespace AuthApp.Helpers;

public class CircularProgressDrawer : IDrawable
{
    public double Progress { get; set; }
    public Color ProgressColor { get; set; } = Color.FromArgb("#6366F1");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float size = Math.Min(dirtyRect.Width, dirtyRect.Height) - 4;
        float x = (dirtyRect.Width - size) / 2;
        float y = (dirtyRect.Height - size) / 2;

        // Background Circle (Semi-transparent)
        canvas.StrokeColor = Color.FromRgba(0, 0, 0, 0.05);
        canvas.StrokeSize = 3;
        canvas.DrawEllipse(x, y, size, size);

        // Progress Arc
        canvas.StrokeColor = ProgressColor;
        canvas.StrokeSize = 3;
        canvas.StrokeLineCap = LineCap.Round;
        
        float startAngle = 90;
        float sweepAngle = (float)(360 * Progress);
        float endAngle = startAngle - sweepAngle;
        
        canvas.DrawArc(x, y, size, size, startAngle, endAngle, true, false);
    }
}
