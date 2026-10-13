using UnityEngine;

namespace FrecuenciaCero
{
    public sealed class SurvivalHUD : MonoBehaviour
    {
        [SerializeField] private SurvivalPlayer player;
        private MovementNoise noise; private BottleAim aim; private NarrativeDirector director;
        private GUIStyle style, centered;
        private Texture2D heart, medkit;
        private void Start(){noise=player.GetComponent<MovementNoise>();aim=player.GetComponent<BottleAim>();director=FindAnyObjectByType<NarrativeDirector>();}
        private void OnGUI()
        {
            if(player==null)return;
            if(style==null)
            {
                style=new GUIStyle(GUI.skin.label){fontSize=13,wordWrap=true,padding=new RectOffset(0,0,0,0)};
                style.normal.textColor=new Color(.87f,.9f,.86f);centered=new GUIStyle(style){alignment=TextAnchor.MiddleCenter};
                string[] pixels={"01100110","11111111","11111111","01111110","00111100","00011000"};
                heart=new Texture2D(8,6){filterMode=FilterMode.Point};var data=new Color[48];
                for(int y=0;y<6;y++)for(int x=0;x<8;x++)data[(5-y)*8+x]=pixels[y][x]=='1'?Color.white:Color.clear;
                heart.SetPixels(data);heart.Apply();
                medkit=new Texture2D(12,10){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
                var kitPixels=new Color[120];
                for(int y=0;y<10;y++)for(int x=0;x<12;x++)
                {
                    bool box=x>=1&&x<=10&&y>=1&&y<=7, handle=x>=4&&x<=7&&y==8;
                    bool cross=box&&(x>=5&&x<=6&&y>=2&&y<=6||x>=3&&x<=8&&y>=3&&y<=4);
                    kitPixels[y*12+x]=cross?new Color(.92f,.91f,.83f):box?new Color(.65f,.16f,.18f):handle?new Color(.4f,.35f,.3f):Color.clear;
                }
                medkit.SetPixels(kitPixels);medkit.Apply();
            }
            var matrix=GUI.matrix;var color=GUI.color;int depth=GUI.depth;
            float scale=HudLayout.Scale,width=Screen.width/scale,height=Screen.height/scale;
            GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);GUI.depth=-30;
            if(HudVisibility.IsVisible&&!player.Dead)
            {
                for(int i=0;i<3;i++)
                {
                    bool filled=i<RetenMission.Session.Survival.Health;
                    GUI.color=filled?new Color(.82f,.18f,.2f):new Color(.25f,.25f,.27f);
                    if(filled&&i==RetenMission.Session.Survival.Health-1)GUI.color=Color.Lerp(GUI.color,new Color(.6f,1f,.72f),player.HealPulse);
                    GUI.DrawTexture(new Rect(16+i*28,16,24,18),heart);
                }
                GUI.color=Color.white;
                GUI.Label(new Rect(16,122,232,19),$"Baterías: {RetenMission.Session.Survival.Batteries}/5   [R] recargar",style);
                GUI.Label(new Rect(16,143,232,19),$"Botellas: {RetenMission.Session.Survival.Bottles}/10   [Q] apuntar",style);
                GUI.DrawTexture(new Rect(16,167,18,15),medkit);
                GUI.Label(new Rect(40,164,232,19),$"Botiquines: {RetenMission.Session.Survival.Medkits}/{RetenMission.Session.Survival.MaximumMedkits}   [H] usar",style);
                if(RetenMission.Session.NoiseHudUnlocked && (director==null||!director.IsOpen))
                {
                    float x=width-184,y=height-53;
                    Rect(new Rect(x-10,y-8,178,44),new Color(.025f,.04f,.04f,.85f));
                    GUI.color=Color.white;GUI.Label(new Rect(x,y-5,75,18),"RUIDO",style);
                    Rect(new Rect(x,y+16,6,6),new Color(.8f,.85f,.8f));Rect(new Rect(x+6,y+12,6,14),new Color(.8f,.85f,.8f));
                    Rect(new Rect(x+14,y+14,2,2),new Color(.8f,.85f,.8f));Rect(new Rect(x+16,y+16,2,6),new Color(.8f,.85f,.8f));Rect(new Rect(x+14,y+22,2,2),new Color(.8f,.85f,.8f));
                    float level=noise!=null?noise.Level:0;
                    for(int i=0;i<8;i++)Rect(new Rect(x+25+i*16,y+15,12,9),i<Mathf.CeilToInt(level*8)?Color.Lerp(new Color(.8f,.73f,.48f),new Color(.85f,.22f,.2f),level):new Color(.15f,.2f,.18f));
                }
                if(Time.unscaledTime<SurvivalPlayer.NoticeUntil && (director==null||!director.IsOpen))
                {GUI.color=Color.white;GUI.Label(new Rect(width*.2f,height-112,width*.6f,40),SurvivalPlayer.Notice,centered);}
            }
            if(aim!=null&&aim.IsAiming&&Camera.main!=null)
            {
                Vector3 screen=Camera.main.WorldToScreenPoint(aim.PredictedImpact);
                float x=screen.x/scale,y=(Screen.height-screen.y)/scale;
                var tint=aim.InRange?new Color(.87f,.94f,.83f):new Color(.95f,.2f,.2f);
                Rect(new Rect(x-10,y-1,6,2),tint);Rect(new Rect(x+4,y-1,6,2),tint);
                Rect(new Rect(x-1,y-10,2,6),tint);Rect(new Rect(x-1,y+4,2,6),tint);
                GUI.color=Color.white;GUI.Label(new Rect(width/2-200,height-84,400,25),"Clic: lanzar   ·   Clic derecho / Esc: cancelar",centered);
            }
            if(player.Dead)
            {
                GUI.depth=-500;Rect(new Rect(0,0,width,height),new Color(0,0,0,player.DeathFade));GUI.color=Color.white;
                GUI.Label(new Rect(width/2-200,height/2-55,400,40),"LEO HA CAÍDO",new GUIStyle(centered){fontSize=28});
                GUI.Label(new Rect(width/2-220,height/2+15,440,40),"ENTER — Reintentar desde el campamento",centered);
            }
            GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;
        }
        private static void Rect(Rect rect,Color color){GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);}
        private void OnDestroy(){if(heart!=null)Destroy(heart);if(medkit!=null)Destroy(medkit);}
    }
}
