using System;

namespace SynergyLib.FileFormat.DotSquish {
    // From DotSquish
    internal abstract class ColorFit {
        public readonly SquishOptions Options;
        public readonly ColorSet Colors;

        protected ColorFit(ColorSet colors, SquishOptions options) {
            Colors = colors;
            Options = options;
        }

        public void Compress(Span<byte> block) {
            Reset();
            if (Options.Method == SquishMethod.Dxt1) {
                Compress3(block);
                if (!Colors.IsTransparent)
                    Compress4(block);
            } else
                Compress4(block);
        }

        protected abstract void Reset();
        protected abstract void Compress3(Span<byte> block);
        protected abstract void Compress4(Span<byte> block);
    }
}
