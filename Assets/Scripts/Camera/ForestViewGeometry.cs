using System;

namespace FrecuenciaCero
{
    // Mirrors the installed URP Pixel Perfect viewport sizing without changing its zoom.
    public static class ForestViewGeometry
    {
        public static void PixelPerfectHalfSize(int width, int height, int referenceWidth,
            int referenceHeight, int pixelsPerUnit, bool cropX, bool cropY, bool upscale,
            out double halfX, out double halfY)
        {
            int zoom = Math.Max(1, Math.Min(width / Math.Max(1, referenceWidth),
                height / Math.Max(1, referenceHeight)));
            double pixelWidth = cropX ? referenceWidth : width / (double)zoom;
            double pixelHeight = cropY ? referenceHeight : height / (double)zoom;
            if (upscale && zoom > 1)
            {
                if (!cropX) pixelWidth = width / zoom / 2 * 2;
                if (!cropY) pixelHeight = height / zoom / 2 * 2;
            }
            halfX = pixelWidth / (2 * Math.Max(1, pixelsPerUnit));
            halfY = pixelHeight / (2 * Math.Max(1, pixelsPerUnit));
        }

        public static double ClampCenter(double center, double min, double max, double halfSize)
        {
            return max - min <= halfSize * 2 ? (min + max) * .5 :
                Math.Max(min + halfSize, Math.Min(max - halfSize, center));
        }
    }
}
