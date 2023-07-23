using System.Numerics;
using System.Threading;

namespace SynergyLib.FileFormat.DotSquish; 

public class SquishOptions {
    public CancellationToken CancellationToken;
    public Vector3? Weights = null;
    public SquishMethod Method = SquishMethod.Dxt1;
    public SquishFit Fit = SquishFit.ColorClusterFit;
    public bool WeightColorByAlpha = false;
    public int Threads = 0;
}

public enum SquishMethod {
    /// <summary>
    /// Use DXT1 compression.
    /// </summary>
    Dxt1,
    /// <summary>
    /// Use DXT3 compression.
    /// </summary>
    Dxt3,
    /// <summary>
    /// Use DXT5 compression.
    /// </summary>
    Dxt5,
}

public enum SquishFit {
    /// <summary>
    /// Use a very slow but very high quality Color compressor.
    /// </summary>
    ColorIterativeClusterFit,

    /// <summary>
    /// Use a slow but high quality Color compressor (default).
    /// </summary>
    ColorClusterFit,

    /// <summary>
    /// Use a fast but low quality Color compressor.
    /// </summary>
    ColorRangeFit,
}