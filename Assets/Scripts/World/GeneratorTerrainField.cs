using System;

namespace FrecuenciaCero
{
    // Continuous world-space terrain, independent of a tile grid; also used by the asset baker.
    public static class GeneratorTerrainField
    {
        public const int Width = 2304, Height = 1792, PPU = 32;
        private static readonly double[,] Road = {{-15,-7},{-11,-5.5},{-7,-4},{-3,-2.5},{1,-.5},{4,2},{5,4.5}};
        private static readonly double[,] Storage = {{-3,-2.5},{-5,0},{-6.5,2.5},{-7,5.5}};
        private static double Clamp(double n) { return Math.Max(0, Math.Min(1, n)); }
        private static double Smooth(double n) { n = Clamp(n); return n*n*(3-2*n); }
        private static double Mix(double a,double b,double t) { return a+(b-a)*t; }
        public static double Hash(int x,int y) { return ForestFogField.Hash(x+901,y-243); }
        private static double Noise(double x,double y)
        {
            int ix=(int)Math.Floor(x),iy=(int)Math.Floor(y);double a=Smooth(x-ix),b=Smooth(y-iy);
            return Mix(Mix(Hash(ix,iy),Hash(ix+1,iy),a),Mix(Hash(ix,iy+1),Hash(ix+1,iy+1),a),b);
        }
        private static double Distance(double x,double y,double[,] route)
        {
            double best=double.MaxValue;
            for(int i=1;i<route.GetLength(0);i++)
            {
                double dx=route[i,0]-route[i-1,0],dy=route[i,1]-route[i-1,1];
                double t=Clamp(((x-route[i-1,0])*dx+(y-route[i-1,1])*dy)/(dx*dx+dy*dy));
                double a=x-route[i-1,0]-t*dx,b=y-route[i-1,1]-t*dy;
                best=Math.Min(best,a*a+b*b);
            }
            return Math.Sqrt(best);
        }
        public static double PathDistance(double x,double y) { return Math.Min(Distance(x,y,Road),Distance(x,y,Storage)); }
        public static uint Pixel(int px,int py)
        {
            double x=-36+(px+.5)/PPU,y=-28+(py+.5)/PPU;
            double grain=Hash(px,py)-.5,broad=Noise(x*.25,y*.25);
            double edge=(Noise(x*1.6,y*1.6)-.5)*.4+(Noise(x*5,y*5)-.5)*.09;
            double path=Smooth((1.1+edge-PathDistance(x,y))/.48);
            double r=Mix(36+7*broad,64+5*broad,path),g=Mix(45+8*broad,56+4*broad,path),b=Mix(38+6*broad,43+4*broad,path);
            double mottling=(Noise(x*2.5,y*2.5)-.5)*5+grain*4;
            r+=mottling;g+=mottling;b+=mottling;
            int cx=px/9,cy=py/9;double seed=Hash(cx,cy),lx=px-cx*9-1-Hash(cx+31,cy)*6,ly=py-cy*9-1-Hash(cx,cy+97)*6;
            if(lx*lx/4+ly*ly/1.8<1 && seed>.81){r=Mix(r,seed>.95?73:65,.4);g=Mix(g,seed>.95?74:56,.4);b=Mix(b,seed>.95?64:35,.4);}
            return (uint)((int)r | ((int)g<<8) | ((int)b<<16));
        }
        public static byte FogAlpha(int px,int py)
        {
            double x=-36+(px+.5)/16,y=-28+(py+.5)/16;
            double edge=Smooth(Math.Min((36-Math.Abs(x))/6,(28-Math.Abs(y))/6));
            double density=.35+.65*Smooth((PathDistance(x,y)-.4)/1.7);
            return (byte)(255*Clamp((.2+.4*Noise(x*.17,y*.2)+.15*Noise(x*.4+51,y*.4))*density*edge));
        }
    }
}
