using Microsoft.Maui.Graphics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Clock;

public partial class MainPage : ContentPage
{
    private Timer _timer;
    private TimeGraphicsDrawable _drawable;

    public MainPage()
    {
        InitializeComponent();

        _drawable = new TimeGraphicsDrawable();
        ClockView.Drawable = _drawable;

        _timer = new Timer(1000);
        _timer.Elapsed += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _drawable.CurrentTime = DateTime.Now;
                ClockView.Invalidate();
            });
        };
        _timer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _timer?.Start();
    }
}

public class TimeGraphicsDrawable : IDrawable
{
    public DateTime CurrentTime { get; set; } = DateTime.Now;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.Black;
        canvas.FillRectangle(dirtyRect);

        canvas.StrokeColor = Colors.Lime;
        canvas.StrokeSize = 5;

        DrawDigitalTime(canvas, CurrentTime, dirtyRect);
    }

    private void DrawDigitalTime(ICanvas canvas, DateTime time, RectF rect)
    {
        string timeString = time.ToString("HH:mm:ss");

        float X = 50;
        float Y = 50;
        float segmentLength = 60;
        float segmentSpacing = 20;

        foreach (char c in timeString)
        {
            if (c == ':')
            {
                DrawColon(canvas, X, Y, segmentLength);
                X += segmentSpacing + 10;
            }
            else
            {
                DrawDigit(canvas, c - '0', X, Y, segmentLength);
                X += segmentLength + segmentSpacing;
            }
        }
    }

    private void DrawDigit(ICanvas canvas, int digit, float x, float y, float l)
    {
        float halfL = l / 2;

        var segments = new[]
        {
            (x, y, x + l, y),
            (x + l, y, x + l, y + halfL),
            (x + l, y + halfL, x + l, y + l),
            (x, y + l, x + l, y + l),
            (x, y + halfL, x, y + l),
            (x, y, x, y + halfL),
            (x, y + halfL, x + l, y + halfL),
        };

        bool[] segmentMap = digit switch
        {
            0 => new[] { true, true, true, true, true, true, false },
            1 => new[] { false, true, true, false, false, false, false },
            2 => new[] { true, true, false, true, true, false, true },
            3 => new[] { true, true, true, true, false, false, true },
            4 => new[] { false, true, true, false, false, true, true },
            5 => new[] { true, false, true, true, false, true, true },
            6 => new[] { true, false, true, true, true, true, true },
            7 => new[] { true, true, true, false, false, false, false },
            8 => new[] { true, true, true, true, true, true, true },
            9 => new[] { true, true, true, true, false, true, true },
            _ => new[] { false, false, false, false, false, false, false },
        };

        for (int i = 0; i < segments.Length; i++)
        {
            if (segmentMap[i])
            {
                var (x1, y1, x2, y2) = segments[i];
                canvas.DrawLine(x1, y1, x2, y2);
            }
        }
    }

    private void DrawColon(ICanvas canvas, float x, float y, float l)
    {
        float ellipseWidth = 5;
        float ellipseHeight = 5;
        float spacing = l / 4;  

        float upperEllipseY = y + spacing;
        float lowerEllipseY = y + 3 * spacing;

        canvas.DrawEllipse(x, upperEllipseY, ellipseWidth, ellipseHeight);

        canvas.DrawEllipse(x, lowerEllipseY, ellipseWidth, ellipseHeight);
    }


}
