using System;

namespace FrecuenciaCero
{
    // The original tilemaps and river field remain untouched. Extend their exact world pixel grid.
    public static class ForestBorderField
    {
        public const int MinX = -40, MinY = -30, PixelsPerUnit = 32, Width = 2560, Height = 1920;

        public static uint Pixel(int px, int py)
        {
            uint color = OrganicTerrainField.Pixel(px + (MinX - OrganicTerrainField.MinX) * PixelsPerUnit,
                py + (MinY - OrganicTerrainField.MinY) * PixelsPerUnit);
            double x = MinX + (px + .5) / PixelsPerUnit;
            double y = MinY + (py + .5) / PixelsPerUnit;
            double depth = Math.Max((Math.Abs(x) - 21) / 9, (Math.Abs(y) - 14) / 8);
            double shade = 1 - .24 * ForestFogField.Smooth(depth);
            return 0xff000000u | ((uint)(((color >> 16) & 255) * shade) << 16) |
                ((uint)(((color >> 8) & 255) * shade) << 8) | (uint)((color & 255) * shade);
        }

        public static bool CanPlace(double x, double y)
        {
            if (Math.Abs(x) < 17.5 && Math.Abs(y) < 10.8) return false;
            if (OrganicTerrainField.PathDistance(x, y) < 2.7) return false;
            if (Math.Abs(x - OrganicTerrainField.RiverCenter(y)) < OrganicTerrainField.CollisionHalfWidth(y) + .9)
                return false;
            if (x > -17.8 && x < -9 && y > .5 && y < 8.8) return false;
            if (x > 6 && x < 16.7 && y > -3 && y < 9.5) return false;
            return true;
        }
    }
}
