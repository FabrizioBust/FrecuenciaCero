#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FrecuenciaCero;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-500)]
public sealed class ChapterAuditRuntime : MonoBehaviour
{
    private readonly List<string> log=new List<string>();
    private Keyboard keyboard; private Mouse mouse; private Key[] keys=Array.Empty<Key>();
    private int passed,failed; private bool finished; private float started; private Vector2 pointer;
    private string reports; private bool leftClick;
    void Awake(){DontDestroyOnLoad(gameObject);Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();foreach(var device in InputSystem.devices.ToArray())if(device!=keyboard&&device!=mouse)InputSystem.DisableDevice(device);pointer=new Vector2(Screen.width*.85f,Screen.height*.5f);started=Time.realtimeSinceStartup;reports=Path.GetFullPath(Path.Combine(Application.dataPath,"../../AuditoriaCapitulo1"));Application.logMessageReceived+=ConsoleLog;InputSystem.onBeforeUpdate+=Inject;}
    void Inject(){if(InputState.currentUpdateType!=InputUpdateType.Dynamic)return;InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.QueueStateEvent(mouse,new MouseState{position=pointer,buttons=(ushort)(leftClick?1:0)});keyboard.MakeCurrent();mouse.MakeCurrent();}
    void Update(){if(!finished&&Time.realtimeSinceStartup-started>450)Finish("TIMEOUT");}
    void ConsoleLog(string message,string stack,LogType kind){if((kind==LogType.Exception||kind==LogType.Error)&&(stack.Contains("UnityEditor.Search")||stack.Contains("Unity.AI.Toolkit")||stack.Contains("TutorialModalWindow"))){log.Add("EDITOR_INFRA "+message);return;}if(kind==LogType.Exception||kind==LogType.Error){failed++;log.Add("CONSOLE "+kind+" "+message+"\n"+stack);}}
    void Check(bool condition,string description){if(condition){passed++;log.Add("PASS "+description);}else{failed++;log.Add("FAIL "+description);}Directory.CreateDirectory(reports);File.AppendAllText(Path.Combine(reports,"ProgresoPruebas.txt"),log[log.Count-1]+"\n");}
    void Note(string description){log.Add("NOTE "+description);}
    void Finish(string reason){if(finished)return;finished=true;log.Add("RESULT passed="+passed+" failed="+failed+" reason="+reason);Directory.CreateDirectory(reports);File.WriteAllLines(Path.Combine(reports,"PruebasPlayMode.txt"),log);EditorApplication.Exit(failed==0?0:1);}
    IEnumerator Start(){yield return Run();Finish("complete");}
    IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    IEnumerator Until(Func<bool> condition,float seconds,string label){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
    IEnumerator Press(Key key,float seconds=.08f){keys=new[]{key};yield return Wait(seconds);keys=Array.Empty<Key>();yield return Wait(.08f);}
    IEnumerator Load(string name){keys=Array.Empty<Key>();yield return SceneManager.LoadSceneAsync(name);yield return Wait(.25f);}
    LeoMovement Leo=>FindAnyObjectByType<LeoMovement>();
    NarrativeDirector Director=>FindAnyObjectByType<NarrativeDirector>();
    void Warp(Vector2 point){var leo=Leo;var body=leo.GetComponent<Rigidbody2D>();body.position=point;body.linearVelocity=Vector2.zero;leo.transform.position=new Vector3(point.x,point.y,0);Physics2D.SyncTransforms();}
    T Get<T>(object obj,string field)=>(T)obj.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(obj);
    bool Free(Vector2 point){var own=Leo.GetComponent<Collider2D>();foreach(var c in Physics2D.OverlapCapsuleAll(point+Vector2.up*.1f,new Vector2(.45f,.28f),CapsuleDirection2D.Horizontal,0))if(c!=own&&!c.isTrigger&&c.enabled&&!c.transform.IsChildOf(Leo.transform))return false;return true;}
    Vector2? Reach(WorldInteraction target){var original=(Vector2)Leo.transform.position;var probe=new InteractionProbe(Leo.transform);Vector2? result=null;for(float y=-1.8f;y<=1.8f&&!result.HasValue;y+=.15f)for(float x=-1.8f;x<=1.8f;x+=.15f){Vector2 p=target.InteractionPoint+new Vector2(x,y);if(!Free(p))continue;Warp(p);if(target.IsAvailable(probe)&&FindObjectsByType<WorldInteraction>().Where(t=>t.isActiveAndEnabled&&t.IsAvailable(probe)).OrderBy(t=>(probe.Position-t.InteractionPoint).sqrMagnitude).FirstOrDefault()==target){result=p;break;}}Warp(original);return result;}
    IEnumerator CloseDialog(){float end=Time.realtimeSinceStartup+20;while(Director!=null&&Director.IsOpen&&Time.realtimeSinceStartup<end)yield return Press(Director.Document!=null?Key.Escape:Key.Enter,.03f);Check(Director!=null&&!Director.IsOpen,"Diálogo/documento cerrado y control liberado");}
    IEnumerator Interact(WorldInteraction target,string label){var point=Reach(target);Check(point.HasValue,"Punto físico accesible: "+label);if(!point.HasValue)yield break;Warp(point.Value);yield return Wait(.12f);var interaction=FindAnyObjectByType<PlayerInteraction>();Check(interaction.FocusTarget==target,"Selección E: "+label);yield return Press(Key.E);}
    IEnumerator Travel(WorldInteraction target,string destination,string label){yield return Interact(target,label);yield return Until(()=>SceneManager.GetActiveScene().name==destination&&!RetenTravel.IsLoading,12,"Transición "+label+" -> "+destination);yield return Wait(.25f);Check(Leo!=null&&Leo.enabled&&!Leo.IsBlocked,"Control disponible tras "+label);Check(Leo!=null&&Free(Leo.transform.position),"Aparición sin solapamiento sólido tras "+label);}
    IEnumerator Run()
    {
        yield return Interruptions();
        MenuPreferences.SetEffects(.6f);MenuPreferences.SetMusic(.4f);RetenMission.ResetSession();RetenTravel.Reset();HudVisibility.ResetSession();
        yield return Load("MainMenu");yield return Wait(1.5f);
        var menu=FindAnyObjectByType<MainMenuController>();Check(menu!=null&&!menu.IsTransitioning,"Menú principal operativo");Check(menu.GetComponent<AudioSource>().isPlaying&&menu.GetComponent<AudioSource>().loop,"Música sólo en menú");
        menu.newGame.Select();Check(EventSystem.current.currentSelectedGameObject==menu.newGame.gameObject,"Nueva partida seleccionada");yield return Press(Key.Enter);
        yield return Until(()=>SceneManager.GetActiveScene().name=="IntroHistoria",10,"Nueva partida carga introducción");
        yield return Press(Key.Space,1.7f);yield return Until(()=>SceneManager.GetActiveScene().name=="N1_ExteriorReten",10,"Omitir intro manteniendo Espacio");yield return Wait(.4f);
        Check(!RetenMission.Session.TreeFallen&&!RetenMission.Session.GeneratorRepaired,"Nueva partida reinicia misión");Check(Mathf.Abs(MenuPreferences.EffectsVolume-.6f)<.001f,"Nueva partida conserva preferencias");
        Check(FindObjectsByType<AudioListener>().Count(a=>a.enabled)==1,"Un AudioListener");Check(FindObjectsByType<Camera>().Count(c=>c.enabled)==1,"Una cámara activa");
        Warp(new Vector2(-11,2));var body=Leo.GetComponent<Rigidbody2D>();float speed=Leo.Speed;
        foreach(var key in new[]{Key.D,Key.A,Key.W,Key.S}){keys=new[]{key};yield return Wait(.15f);Check(Mathf.Abs(body.linearVelocity.magnitude-speed)<.12f,"Movimiento "+key+" velocidad "+body.linearVelocity.magnitude);keys=Array.Empty<Key>();yield return Wait(.08f);}
        keys=new[]{Key.D,Key.W};yield return Wait(.12f);Check(Mathf.Abs(body.linearVelocity.magnitude-speed)<.12f,"Diagonal conserva velocidad");keys=new[]{Key.D,Key.LeftShift};yield return Wait(.12f);Check(Mathf.Abs(body.linearVelocity.magnitude-speed*1.65f)<.12f,"Correr velocidad original");keys=Array.Empty<Key>();yield return Wait(.15f);Check(body.linearVelocity==Vector2.zero,"Quieto: velocidad cero");
        var footsteps=Leo.GetComponent<LeoFootstepAudio>();Check(footsteps.PlayedCount>0&&footsteps.LastClip!=null&&!footsteps.GetComponent<AudioSource>().loop,"Pisadas individuales reproducidas al moverse");int before=footsteps.PlayedCount;yield return Wait(.6f);Check(footsteps.PlayedCount==before&&!footsteps.GetComponent<AudioSource>().isPlaying,"Quieto: sin pasos");
        foreach(var c in FindObjectsByType<Collider2D>().Where(c=>!c.isTrigger&&c!=Leo.GetComponent<Collider2D>()).Take(1))Note("Collider sólido presente: "+c.name);
        var snapshot=RetenMission.Session;var inventory=snapshot.Survival;
        inventory.Collect("audit-battery",SupplyKind.Battery);inventory.Collect("audit-bottle",SupplyKind.Bottle);inventory.Collect("audit-medkit",SupplyKind.Medkit);inventory.Damage(1);
        var battery=Leo.GetComponentInChildren<FlashlightBattery>();var light=Leo.GetComponentInChildren<FlashlightController>();battery.RestoreCharge(64);light.SetOn(false);
        yield return Travel(FindAnyObjectByType<RetenDoor>(),"N1_InteriorReten","Entrar al retén");
        Check(ReferenceEquals(snapshot,RetenMission.Session)&&inventory.Health==2&&inventory.Batteries==1&&inventory.Medkits==1&&inventory.Bottles==1,"Inventario y salud persisten en interior");
        Check(Mathf.Abs(Leo.GetComponentInChildren<FlashlightBattery>().Percent-64)<.01f&&!Leo.GetComponentInChildren<FlashlightController>().IsOn,"Carga/estado de linterna persisten");
        Check(Leo.GetComponent<LeoFootstepAudio>()!=null,"Audio de pasos en retén");
        var radio=FindObjectsByType<NarrativeTarget>().First(t=>Get<RetenInteractionKind>(t,"kind")==RetenInteractionKind.Radio);
        yield return Interact(radio,"Radio supervisor");Check(Director.IsOpen&&!Leo.enabled,"Radio bloquea movimiento durante diálogo");yield return CloseDialog();
        yield return Until(()=>RetenMission.Session.TreeFallen,8,"Accidente del árbol tras conversación");yield return Wait(.5f);if(Director.IsOpen)yield return CloseDialog();Check(Leo.enabled&&!Leo.IsBlocked,"Accidente libera control");
        var surface=FindAnyObjectByType<FootstepSurfaceMap>();Check(surface.At(Leo.GetComponent<Collider2D>().bounds.center)==FootstepSurface.RetenWood,"V2 asignado al retén");
        var logbook=FindObjectsByType<NarrativeTarget>().First(t=>Get<RetenInteractionKind>(t,"kind")==RetenInteractionKind.Logbook);yield return Interact(logbook,"Cuaderno");yield return CloseDialog();Check(snapshot.LogbookRead,"Lectura de cuaderno registrada");
        var exit=FindAnyObjectByType<RetenDoor>();yield return Travel(exit,"N1_ExteriorReten","Salir del retén");
        var accident=FindAnyObjectByType<TruckAccident>();var discovery=Get<Transform>(accident,"discoveryPoint");Vector2? spot=null;var probe=new InteractionProbe(Leo.transform);
        for(float y=-1.5f;y<=1.5f&&!spot.HasValue;y+=.2f)for(float x=-1.5f;x<=1.5f;x+=.2f){var p=(Vector2)discovery.position+new Vector2(x,y);if(!Free(p))continue;Warp(p);if(probe.CanReach(discovery.position,1.8f,probe.Position-(Vector2)discovery.position,1.8f)){spot=p;break;}}
        Check(spot.HasValue,"Camioneta destruida examinable");if(spot.HasValue)Warp(spot.Value);yield return Wait(.2f);if(Director.IsOpen)yield return CloseDialog();Check(snapshot.WreckDiscovered&&snapshot.GeneratorAvailable,"Objetivo habilita generador");
        yield return Travel(FindAnyObjectByType<GeneratorAccessGate>(),"N1_Generador","Reja generador");
        var generator=FindAnyObjectByType<GeneratorInteraction>();yield return Interact(generator,"Examinar generador");yield return CloseDialog();Check(snapshot.GeneratorExamined,"Generador examinado");
        var fuse=FindAnyObjectByType<FusePickup>();yield return Interact(fuse,"Examinar caja");yield return CloseDialog();yield return Interact(fuse,"Abrir caja");yield return Wait(.7f);yield return Interact(fuse,"Recoger fusible");Check(snapshot.HasFuse,"Fusible recogido");
        yield return Interact(generator,"Instalar fusible");yield return CloseDialog();Check(snapshot.GeneratorRepaired&&!snapshot.HasFuse,"Generador reparado y fusible consumido");
        yield return Wait(2.3f);if(Director.IsOpen)yield return CloseDialog();Check(snapshot.GeneratorAnomalyCompleted&&snapshot.ForestAvailable,"Anomalía libera ruta al bosque");
        yield return Travel(FindAnyObjectByType<GeneratorAccessGate>(),"N1_ExteriorReten","Regresar del generador");
        yield return Travel(FindAnyObjectByType<GeneratorAccessGate>(),"N1_Generador","Volver al camino de servicio");
        yield return Travel(FindAnyObjectByType<ForestPassage>(),"N1_BosqueAbandonado","Sendero al bosque");
        var roots=FindAnyObjectByType<RootsEncounter>();var creature=FindAnyObjectByType<Escarbador>();var ending=FindAnyObjectByType<ChapterOneEnding>();
        Warp(Get<Vector2>(roots,"encounterPoint"));yield return Until(()=>roots.IsPlaying,3,"Encuentro inicia");yield return Wait(7.7f);
        float tutorialEnd=Time.realtimeSinceStartup+15;while(roots.IsPlaying&&Time.realtimeSinceStartup<tutorialEnd)yield return Press(Key.Enter);Check(snapshot.EncounterCompleted&&!Leo.IsBlocked&&Leo.enabled,"Tutorial termina y libera controles");
        yield return Until(()=>creature.State==EscarbadorState.Hidden&&snapshot.EscapedEscarbador,35,"Escarbador busca y se retira al guardar silencio");
        var originalPosition=(Vector2)Leo.transform.position;
        var aim=Leo.GetComponent<BottleAim>();pointer=Camera.main.WorldToScreenPoint(Leo.GetComponent<Collider2D>().bounds.center+Vector3.left*2);
        yield return Press(Key.Q);Check(aim.IsAiming&&Leo.IsBlocked,"Q bloquea caminar y permite apuntar");
        yield return Press(Key.Escape);Check(!aim.IsAiming&&!Leo.IsBlocked,"Escape cancela apuntado y devuelve control");
        yield return Press(Key.Q);int bottlesBefore=inventory.Bottles;leftClick=true;yield return Wait(.1f);leftClick=false;
        yield return Until(()=>inventory.Bottles==bottlesBefore-1&&creature.LastKind==NoiseKind.Bottle,4,"Botella real consume inventario y genera ruido en su impacto");
        Check(!aim.IsAiming,"Lanzar termina apuntado");
        creature.ResetAtHideout();NoiseEvents.Emit(new NoiseEvent(creature.transform.position,1,16,NoiseKind.Bottle));yield return Wait(.1f);Check(creature.LastKind==NoiseKind.Bottle&&creature.State==EscarbadorState.Alert,"Escarbador detecta botella");
        NoiseEvents.Emit(new NoiseEvent(creature.transform.position+Vector3.right,1,16,NoiseKind.Run));yield return Wait(.05f);NoiseEvents.Emit(new NoiseEvent(creature.transform.position+Vector3.right,1,16,NoiseKind.Run));yield return Wait(.05f);Check(creature.State==EscarbadorState.Chase,"Ruido continuo provoca persecucion");
        creature.ResetAtHideout();Warp(originalPosition);var player=Leo.GetComponent<SurvivalPlayer>();yield return Wait(.15f);
        Note("ANTES H health="+inventory.Health+" kits="+inventory.Medkits+" canAct="+player.CanAct+" blocked="+Leo.IsBlocked+" cinematic="+player.Cinematic);
        // Independent controlled healing case: battle damage is not assumed to be exactly one heart.
        inventory.RetryHealth();inventory.Damage(1);int kitsBefore=inventory.Medkits;
        yield return Press(Key.H);Check(inventory.Health==3&&inventory.Medkits==kitsBefore-1,"H recupera un corazon y consume un botiquin");
        yield return Wait(1.1f);
        player.TakeDamage(3);Check(player.Dead&&Leo.IsBlocked,"Muerte bloquea control");player.Retry();yield return Wait(1.1f);Check(!player.Dead&&Leo.enabled&&!Leo.IsBlocked&&inventory.Health==3,"Reintento devuelve control y salud");Check(ReferenceEquals(snapshot,RetenMission.Session)&&snapshot.GeneratorRepaired&&snapshot.EncounterCompleted,"Reintento conserva progreso");Check(Free(Leo.transform.position),"Checkpoint libre de colisiones");
        Warp(Get<Vector2>(roots,"safeExit"));yield return Until(()=>ending.IsPlaying,3,"Salida inicia cierre del capítulo");yield return Until(()=>ending.CardReady,25,"Cinemática completa y tarjeta final habilitada");
        Check(snapshot.LevelOneCompleted&&ending.ContinueButton.interactable,"CONTINUAR habilitado");Check(EventSystem.current.currentSelectedGameObject==ending.ContinueButton.gameObject,"CONTINUAR seleccionado para teclado");
        yield return Press(Key.Enter);
        yield return Until(()=>SceneManager.GetActiveScene().name=="N2_SenderoCabana"&&!RetenTravel.IsLoading,15,"CONTINUAR por teclado carga capítulo II");yield return Wait(3.5f);
        Check(ReferenceEquals(snapshot,RetenMission.Session)&&snapshot.Chapters.TwoStarted&&snapshot.Chapters.ArrivalFinished,"Capítulo II conserva sesión y termina llegada");Check(Leo.enabled&&!Leo.IsBlocked&&!Leo.GetComponent<SurvivalPlayer>().Cinematic,"Capítulo II libera controles");Check(FindAnyObjectByType<ChapterTransitionOverlay>()==null,"Overlay de transición destruido");
        yield return AuditReachability();
        yield return Interruptions();
    }
    IEnumerator AuditReachability()
    {
        foreach(string scene in new[]{"N1_ExteriorReten","N1_InteriorReten","N1_Generador","N1_BosqueAbandonado"})
        {
            yield return Load(scene);var player=Leo.GetComponent<SurvivalPlayer>();player.Cinematic=true;var creature=FindAnyObjectByType<Escarbador>();if(creature!=null)creature.Frozen=true;
            var targets=FindObjectsByType<WorldInteraction>();foreach(var target in targets){if(target is FusePickup||target is SupplyPickup&&RetenMission.Session.Survival.WasCollected(((SupplyPickup)target).UniqueId))continue;var p=Reach(target);Note("INTERACTION "+scene+" "+target.name+" accessible="+p.HasValue);}
            Check(FindObjectsByType<AudioListener>().Count(a=>a.enabled)==1,"Listener único "+scene);Check(FindObjectsByType<Camera>().Count(a=>a.enabled)==1,"Cámara única "+scene);
        }
    }
    IEnumerator Interruptions()
    {
        RetenMission.ResetSession();RetenTravel.Reset();yield return Load("N1_BosqueAbandonado");var roots=FindAnyObjectByType<RootsEncounter>();var creature=FindAnyObjectByType<Escarbador>();Warp(Get<Vector2>(roots,"encounterPoint"));yield return Wait(.4f);Check(roots.IsPlaying,"Prueba de interrupción: encuentro activo");
        var follow=Camera.main.GetComponent<SmoothCameraFollow>();roots.gameObject.SetActive(false);yield return Wait(.1f);
        Check(follow.enabled&&!creature.Frozen&&!Leo.GetComponent<SurvivalPlayer>().Cinematic,"Interrumpir encuentro restaura cámara, criatura y control");
        roots.gameObject.SetActive(true);yield return Wait(.1f);Check(!roots.IsPlaying,"Reactivar encuentro no conserva coroutine bloqueada");
    }
}
#endif
