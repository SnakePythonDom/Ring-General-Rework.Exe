namespace RingGeneral.Core.Import;

public sealed class BakiQuantileMap
{
    private readonly double[] _binEdges;
    private readonly double[] _cumulative;

    public BakiQuantileMap(double[] binEdges, double[] cumulative)
    {
        _binEdges = binEdges;
        _cumulative = cumulative;
    }

    public double PercentileFor(double value)
    {
        if (_binEdges.Length == 0)
        {
            return 0;
        }

        var index = Array.BinarySearch(_binEdges, value);
        if (index < 0)
        {
            index = ~index;
        }

        if (index <= 0)
        {
            return 0;
        }

        if (index >= _cumulative.Length)
        {
            return 1;
        }

        return _cumulative[index - 1];
    }
}

public sealed class BakiQuantileHistogram
{
    private readonly int[] _bins;
    private readonly double _min;
    private readonly double _max;

    public BakiQuantileHistogram(int binCount, double min, double max)
    {
        _bins = new int[binCount];
        _min = min;
        _max = max;
    }

    public void Add(double value)
    {
        var clamped = Math.Min(Math.Max(value, _min), _max);
        var index = (int)Math.Floor((clamped - _min) / (_max - _min) * _bins.Length);
        index = Math.Min(Math.Max(index, 0), _bins.Length - 1);
        _bins[index]++;
    }

    public BakiQuantileMap BuildMap()
    {
        var total = _bins.Sum();
        if (total == 0)
        {
            return new BakiQuantileMap(Array.Empty<double>(), Array.Empty<double>());
        }

        var cumulative = new double[_bins.Length];
        var running = 0;
        for (var i = 0; i < _bins.Length; i++)
        {
            running += _bins[i];
            cumulative[i] = running / (double)total;
        }

        var edges = new double[_bins.Length];
        for (var i = 0; i < _bins.Length; i++)
        {
            edges[i] = _min + ((_max - _min) * (i + 1) / _bins.Length);
        }

        return new BakiQuantileMap(edges, cumulative);
    }
}
