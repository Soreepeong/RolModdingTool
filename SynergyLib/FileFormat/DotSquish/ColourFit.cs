using System;

namespace SynergyLib.FileFormat.DotSquish {
    // From DotSquish
    internal abstract class ColorFit {
        public readonly ColorSet Colors;
        public readonly SquishOptions Flags;

        protected ColorFit(ColorSet colors, SquishOptions flags) {
            Colors = colors;
            Flags = flags;
        }

        public void Compress(Span<byte> block) {
            if (Flags.Method == SquishMethod.Dxt1) {
                Compress3(block);
                if (!Colors.IsTransparent)
                    Compress4(block);
            } else
                Compress4(block);
        }

        protected abstract void Compress3(Span<byte> block);
        protected abstract void Compress4(Span<byte> block);
    }
}
