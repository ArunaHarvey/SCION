using Microsoft.Extensions.Options;

namespace Hplc.Controller.Api.Services;

public class ChromatogramService
{
    private readonly ChromatogramSimulationOptions _options;
    private readonly Random _rand = new();

    public ChromatogramService(IOptions<ChromatogramSimulationOptions> options)
    {
        _options = options.Value;
    }

    public List<ChromPoint> Load()
    {
        int totalPoints =
            (int)(_options.TotalSeconds * _options.PointsPerSecond);

        var data = new List<ChromPoint>(totalPoints);

        int peakCount = _rand.Next(_options.MinPeaks, _options.MaxPeaks + 1);

        var peaks = new List<GaussianPeak>();

        for (int i = 0; i < peakCount; i++)
        {
            peaks.Add(new GaussianPeak
            {
                Center = _rand.NextDouble() * _options.TotalSeconds,
                Height = _rand.NextDouble() *
                         (_options.MaxPeakHeight - _options.MinPeakHeight)
                         + _options.MinPeakHeight,
                Width = _rand.NextDouble() *
                        (_options.MaxPeakWidth - _options.MinPeakWidth)
                        + _options.MinPeakWidth
            });
        }

        for (int i = 0; i < totalPoints; i++)
        {
            double t = i / (double)_options.PointsPerSecond;

            double intensity =
                _options.Baseline +
                (_rand.NextDouble() - 0.5) * _options.NoiseAmplitude;

            foreach (var p in peaks)
                intensity += p.ValueAt(t);

            intensity = Math.Max(intensity, 0);

            data.Add(new ChromPoint(t, intensity));
        }

        return data;
    }

    private class GaussianPeak
    {
        public double Center;
        public double Height;
        public double Width;

        public double ValueAt(double x)
        {
            double z = (x - Center) / Width;
            return Height * Math.Exp(-z * z);
        }
    }
}

public record ChromPoint(double Time, double Intensity);

public class ChromatogramSimulationOptions
{
    public double TotalSeconds { get; set; } = 20;
    public int PointsPerSecond { get; set; } = 20;

    public int MinPeaks { get; set; } = 2;
    public int MaxPeaks { get; set; } = 4;

    public double MinPeakHeight { get; set; } = 50;
    public double MaxPeakHeight { get; set; } = 200;

    public double MinPeakWidth { get; set; } = 0.3;
    public double MaxPeakWidth { get; set; } = 0.8;

    public double Baseline { get; set; } = 20;
    public double NoiseAmplitude { get; set; } = 10;
}
