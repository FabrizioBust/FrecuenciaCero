using System;

namespace FrecuenciaCero
{
    // World-space pixel field: tile boundaries never participate in color selection.
    // Shared by the editor baker and the river collider generator.
    public static class OrganicTerrainField
    {
        public const int PixelsPerUnit = 32, Width = 1344, Height = 896;
        public const int MinX = -21, MinY = -14, ChunkSize = 4, Columns = 11, Rows = 7;
        private static readonly double[] RouteX = { -18, -13, -9, -6, -3, 0, 4, 7, 10, 11 };
        private static readonly double[] RouteY = { 4, 4, 3, .5, -2.6, -3, -3, -.5, 2.8, 3.8 };
        private static readonly double[,] Route = MakeRoute();

        private static double[,] MakeRoute()
        {
            var result = new double[73, 2];
            for (int i = 0; i < 73; i++)
            {
                int s = Math.Min(i / 8, 8);
                double t = (i - s * 8) / 8.0;
                result[i, 0] = Curve(RouteX, s, t);
                result[i, 1] = Curve(RouteY, s, t);
            }
            return result;
        }
        private static double Curve(double[] a, int s, double t)
        {
            double p = a[Math.Max(0, s - 1)], q = a[s], r = a[s + 1], u = a[Math.Min(9, s + 2)];
            return .5 * (2*q + (-p+r)*t + (2*p-5*q+4*r-u)*t*t + (-p+3*q-3*r+u)*t*t*t);
        }
        private static double Hash(int x, int y)
        {
            unchecked
            {
                uint n = (uint)(x * 374761393 + y * 668265263 + 19391);
                n = (n ^ (n >> 13)) * 1274126177u;
                return (n ^ (n >> 16)) / 4294967295.0;
            }
        }
        private static double Smooth(double t) { return t*t*(3-2*t); }
        private static double Mix(double a, double b, double t) { return a+(b-a)*t; }
        private static double Clamp(double v) { return Math.Max(0, Math.Min(1, v)); }
        private static double Noise(double x, double y)
        {
            int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y);
            double u = Smooth(x-ix), v = Smooth(y-iy);
            return Mix(Mix(Hash(ix,iy),Hash(ix+1,iy),u),Mix(Hash(ix,iy+1),Hash(ix+1,iy+1),u),v);
        }
        public static double RiverCenter(double y) { return .5 + Math.Sin((y+3)*.24); }
        public static double WaterHalfWidth(double y)
        {
            return .82 + .055*Math.Sin(y*2.3) + .06*(Noise(y*1.7,53)-.5);
        }
        public static double CollisionHalfWidth(double y) { return WaterHalfWidth(y)+.13; }
        public static double PathDistance(double x, double y)
        {
            double best = double.MaxValue;
            for (int i=1; i<73; i++)
            {
                double dx=Route[i,0]-Route[i-1,0], dy=Route[i,1]-Route[i-1,1];
                double t=Clamp(((x-Route[i-1,0])*dx+(y-Route[i-1,1])*dy)/(dx*dx+dy*dy));
                double ex=x-Route[i-1,0]-t*dx, ey=y-Route[i-1,1]-t*dy;
                best=Math.Min(best,ex*ex+ey*ey);
            }
            return Math.Sqrt(best);
        }
        // RGB packed in the low 24 bits. Alpha is always opaque.
        public static uint Pixel(int px, int py)
        {
            double x=MinX+(px+.5)/32, y=MinY+(py+.5)/32;
            double broad=Noise(x*.3,y*.3), grain=Hash(px,py)-.5;
            double edge=(Noise(x*1.7,y*1.7)-.5)*.34+(Noise(x*5,y*5)-.5)*.08;
            double path=Smooth(Clamp((1.12+edge-PathDistance(x,y))/.42));
            double r=Mix(39+7*broad,65+5*broad,path), g=Mix(48+8*broad,58+4*broad,path), b=Mix(40+5*broad,44+4*broad,path);
            double mottling=(Noise(x*2.6,y*2.6)-.5)*5+grain*4;
            r+=mottling; g+=mottling; b+=mottling;
            double mud=Clamp((Noise(x*.9+36,y*.9)-.61)*3)*.65;
            r=Mix(r,42,mud); g=Mix(g,42,mud); b=Mix(b,34,mud);
            double bank=Math.Abs(x-RiverCenter(y))-WaterHalfWidth(y);
            double shore=Smooth(Clamp((.48+edge*.5-bank)/.48));
            r=Mix(r,49+grain*4,shore);g=Mix(g,49+grain*4,shore);b=Mix(b,39+grain*4,shore);
            // Sparse irregular pixel clusters: stones, leaves and root fragments.
            int cx=(int)Math.Floor(px/9.0),cy=(int)Math.Floor(py/9.0);
            double seed=Hash(cx+872,cy-91);
            double lx=px-cx*9-1-Hash(cx,cy)*6, ly=py-cy*9-1-Hash(cx+91,cy)*6;
            bool detail=lx*lx/4+ly*ly/1.8<1;
            if(detail && seed>.77)
            {
                double strength=.45;
                if(seed>.93) { r=Mix(r,74,strength);g=Mix(g,75,strength);b=Mix(b,64,strength); }
                else { r=Mix(r,70,strength);g=Mix(g,59,strength);b=Mix(b,37,strength); }
            }
            if(seed<.045 && Math.Abs(ly-Math.Sin(lx*.75)*.65)<.45 && Math.Abs(lx)<3)
            { r-=7;g-=7;b-=6; }
            if(bank<0)
            {
                double ripple=Noise(x*3,y*6)*3+grain*2;
                r=22+ripple;g=37+ripple;b=43+ripple;
                double glint=Hash((int)Math.Floor(px/14.0),(int)Math.Floor(py/5.0));
                if(glint>.8 && py%5==0 && px%14<5) { r+=3;g+=5;b+=5; }
            }
            return 0xff000000u | ((uint)Math.Max(0,Math.Min(255,r))<<16) | ((uint)Math.Max(0,Math.Min(255,g))<<8) | (uint)Math.Max(0,Math.Min(255,b));
        }
    }
}
