using System;

namespace FrecuenciaCero
{
    // Procedural, world-space alpha fields. Independent of physics and the camera.
    public static class ForestFogField
    {
        public const int AmbientWidth = 1536, AmbientHeight = 1152, AmbientPPU = 16;
        public const int BankWidth = 256, BankHeight = 128, BankPPU = 32;
        public static double Hash(int x, int y)
        {
            unchecked
            {
                uint n = (uint)(x * 374761393 + y * 668265263 + 6199);
                n = (n ^ (n >> 13)) * 1274126177u;
                return (n ^ (n >> 16)) / 4294967295.0;
            }
        }
        public static double Clamp(double n) { return Math.Max(0, Math.Min(1, n)); }
        public static double Smooth(double n) { n = Clamp(n); return n * n * (3 - 2 * n); }
        private static double Mix(double a, double b, double t) { return a + (b - a) * t; }
        private static double Noise(double x, double y)
        {
            int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y);
            double a = Smooth(x - ix), b = Smooth(y - iy);
            return Mix(Mix(Hash(ix, iy), Hash(ix + 1, iy), a), Mix(Hash(ix, iy + 1), Hash(ix + 1, iy + 1), a), b);
        }
        public static double ZoneDensity(double x, double y)
        {
            double path = Smooth((OrganicTerrainField.PathDistance(x, y) - .5) / 1.8);
            double riverDistance = Math.Abs(x - OrganicTerrainField.RiverCenter(y));
            double river = Math.Exp(-riverDistance * riverDistance / 9);
            double cabin = Math.Exp(-((x - 10.5) * (x - 10.5) + (y - 3.5) * (y - 3.5)) / 12);
            double truck = Math.Exp(-((x + 13) * (x + 13) + (y - 4) * (y - 4)) / 16);
            // Forest is denser than clearings; the crossing remains comparatively transparent.
            return (.3 + .7 * path) * (1 + .55 * river) * (1 - .4 * cabin) * (1 - .3 * truck);
        }
        public static double AmbientAlpha(int px, int py)
        {
            double x = -48 + (px + .5) / AmbientPPU, y = -36 + (py + .5) / AmbientPPU;
            double edge = Smooth(Math.Min((48 - Math.Abs(x)) / 8, (36 - Math.Abs(y)) / 8));
            double cloud = .16 + .43 * Noise(x * .16, y * .18) + .2 * Noise(x * .37 + 41, y * .42 - 19);
            return Clamp(cloud * ZoneDensity(x, y) * edge);
        }
        public static double BankAlpha(int px, int py)
        {
            double x = (px + .5) / BankWidth * 2 - 1, y = (py + .5) / BankHeight * 2 - 1;
            double radius = Math.Sqrt(x * x + y * y);
            double outline = .07 * (Noise(x * 7 + 61, y * 8 + 18) - .5);
            double edge = Smooth((1 - radius + outline) / .52);
            double cloud = .35 + .35 * Noise(x * 4 + 23, y * 5 - 7) + .12 * Noise(x * 11, y * 13);
            // Entire rectangular border is zero alpha, including the middle of every side.
            double border = Smooth(Math.Min(1 - Math.Abs(x), 1 - Math.Abs(y)) / .12);
            return Clamp(edge * cloud * border);
        }
        public static double LifeFade(double normalizedAge)
        {
            return Smooth(normalizedAge / .23) * Smooth((1 - normalizedAge) / .27);
        }
    }
}
