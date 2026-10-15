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
using UnityEngine.UI;

[DefaultExecutionOrder(-500)]
public sealed class PauseValidationRuntime : MonoBehaviour
{
    readonly List<string> log = new List<string>();
    Keyboard keyboard; Mouse mouse; Key[] keys = Array.Empty<Key>();
    Vector2 pointer; ushort buttons; int passed, failed, bottleNoise; float started;
    bool finished;
    string reports;
    LeoMovement Leo => FindAnyObjectByType<LeoMovement>();
    SurvivalPlayer Player => Leo.GetComponent<SurvivalPlayer>();
    PauseMenuController Pause => Leo.GetComponent<PauseMenuController>();
    NarrativeDirector Director => FindAnyObjectByType<NarrativeDirector>();
    void Awake()
    {
        DontDestroyOnLoad(gameObject); Application.runInBackground = true;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
        foreach (var d in InputSystem.devices.ToArray()) if (d != keyboard && d != mouse) InputSystem.DisableDevice(d);
        pointer = new Vector2(Screen.width * .85f, Screen.height * .5f);
        reports = Path.GetFullPath(Path.Combine(Application.dataPath, "../../PausaValidacion"));
        started = Time.realtimeSinceStartup; Application.logMessageReceived += ConsoleLog;
        InputSystem.onBeforeUpdate += Inject;
    }
    void Inject()
    {
        if (InputState.currentUpdateType != InputUpdateType.Dynamic) return;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer, buttons = buttons });
        keyboard.MakeCurrent(); mouse.MakeCurrent();
    }
    void Update() { if (!finished && Time.realtimeSinceStartup - started > 240) Finish("TIMEOUT"); }
    void ConsoleLog(string m, string s, LogType t)
    {
        if (t != LogType.Error && t != LogType.Exception) return;
        if (s.Contains("UnityEditor.Search") || s.Contains("Unity.AI.Toolkit") || s.Contains("TutorialModalWindow")) { log.Add("EDITOR_INFRA " + m); return; }
        failed++; log.Add("CONSOLE " + m + "\n" + s);
    }
    void Check(bool ok, string text)
    {
        if (ok) passed++; else failed++;
        var line = (ok ? "PASS " : "FAIL ") + text; log.Add(line);
        Directory.CreateDirectory(reports); File.AppendAllText(Path.Combine(reports, "ProgresoPausa.txt"), line + "\n");
    }
    void Finish(string why)
    {
        if (finished) return; finished = true;
        Time.timeScale = 1; AudioListener.pause = false;
        log.Add("RESULT passed=" + passed + " failed=" + failed + " reason=" + why);
        Directory.CreateDirectory(reports); File.WriteAllLines(Path.Combine(reports, "PruebasPausa.txt"), log);
        EditorApplication.Exit(failed == 0 ? 0 : 1);
    }
    IEnumerator Wait(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
    IEnumerator Press(Key key, float duration = .09f) { keys = new[] { key }; yield return Wait(duration); keys = Array.Empty<Key>(); yield return Wait(.12f); }
    IEnumerator Load(string name) { keys = Array.Empty<Key>(); buttons = 0; yield return SceneManager.LoadSceneAsync(name); yield return Wait(.4f); }
    IEnumerator Until(Func<bool> f, float seconds, string label) { float end = Time.realtimeSinceStartup + seconds; while (!f() && Time.realtimeSinceStartup < end) yield return null; Check(f(), label); }
    T Field<T>(object o, string name) => (T)o.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(o);
    Button Button(string name) => Pause.GetComponentsInChildren<Button>().First(b => b.name == name);
    IEnumerator Click(Button button)
    {
        pointer = RectTransformUtility.WorldToScreenPoint(null, ((RectTransform)button.transform).position);
        yield return Wait(.08f); buttons = 1; yield return Wait(.1f); buttons = 0; yield return Wait(.15f);
        pointer = new Vector2(Screen.width * .85f, Screen.height * .5f);
    }
    void Warp(Vector2 point)
    {
        Leo.GetComponent<Rigidbody2D>().position = point; Leo.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        Leo.transform.position = point; Physics2D.SyncTransforms();
    }
    IEnumerator CloseModal()
    {
        for (int i = 0; i < 30 && Director.IsOpen; i++) yield return Press(Director.Document != null ? Key.Escape : Key.Enter, .03f);
        Check(!Director.IsOpen, "Cierre modal conserva control");
    }
    void Noise(NoiseEvent e) { if (e.Kind == NoiseKind.Bottle) bottleNoise++; }
    IEnumerator Start() { yield return Run(); Finish("complete"); }
    IEnumerator Run()
    {
        string savedPreference = Path.Combine(reports, "PreferenciaEsperada.txt");
        if (File.Exists(savedPreference)) Check(Mathf.Abs(MenuPreferences.EffectsVolume - float.Parse(File.ReadAllText(savedPreference), System.Globalization.CultureInfo.InvariantCulture)) < .001f,
            "PlayerPrefs conserva volumen tras reiniciar el proceso Unity");
        RetenMission.ResetSession(); RetenTravel.Reset(); HudVisibility.ResetSession();
        MenuPreferences.SetEffects(.6f); MenuPreferences.SetMusic(.4f);
        yield return Load("N1_ExteriorReten");
        Check(Pause != null && FindObjectsByType<PauseMenuController>().Length == 1, "Un controlador automático en gameplay");
        Check(Pause.CanOpen && !GameplayPause.IsPaused, "Gameplay inicial admite pausa");
        for (int i = 0; i < 3; i++)
        {
            yield return Press(Key.Escape); Check(Pause.IsOpen && Time.timeScale == 0 && AudioListener.pause, "ESC pausa física y audio ciclo " + i);
            yield return Press(Key.Escape); Check(!Pause.IsOpen && Time.timeScale == 1 && !AudioListener.pause && Player.CanAct, "ESC restaura control ciclo " + i);
        }
        foreach (bool running in new[] { false, true })
        {
            // Use the verified clear path; the arrival spawn is beside the solid truck.
            Warp(new Vector2(-11, 2)); yield return Wait(.1f);
            keys = running ? new[] { Key.D, Key.LeftShift } : new[] { Key.D }; yield return Wait(.16f);
            Check(Leo.GetComponent<Rigidbody2D>().linearVelocity.x > 0 && Leo.IsRunning == running, "Movimiento real antes de pausa correr=" + running);
            keys = running ? new[] { Key.D, Key.LeftShift, Key.Escape } : new[] { Key.D, Key.Escape }; yield return Wait(.1f);
            keys = Array.Empty<Key>(); var pos = Leo.transform.position; float scaled = Time.time;
            float charge = Player.GetComponentInChildren<FlashlightBattery>().Percent;
            yield return Wait(.35f);
            Check(Pause.IsOpen && Leo.transform.position == pos && Time.time == scaled, "Posición y temporizador congelados correr=" + running);
            Check(Mathf.Approximately(charge, Player.GetComponentInChildren<FlashlightBattery>().Percent), "Batería no drena en pausa");
            yield return Click(Button("CONTINUAR")); Check(!Pause.IsOpen && Player.CanAct, "CONTINUAR funciona con ratón");
        }
        var inventory = RetenMission.Session.Survival;
        inventory.Collect("test-bottle", SupplyKind.Bottle); inventory.Collect("test-bottle2", SupplyKind.Bottle);
        inventory.Collect("test-medkit", SupplyKind.Medkit); inventory.Damage(1);
        NoiseEvents.Emitted += Noise;
        yield return Press(Key.Q); Check(Player.Aiming && Leo.IsBlocked, "Q activa apuntado y bloqueo");
        int stock = inventory.Bottles;
        buttons = 3; yield return Wait(.08f); buttons = 0; yield return Wait(.12f);
        Check(!Player.Aiming && !Leo.IsBlocked && inventory.Bottles == stock && bottleNoise == 0 && FindObjectsByType<BottleProjectile>().Length == 0,
            "Clic derecho tiene prioridad sobre izquierdo; cancela sin consumo ni ruido");
        buttons = 2; yield return Wait(.08f); buttons = 0; yield return Wait(.12f);
        Check(!Player.Aiming && inventory.Bottles == stock, "Clic derecho fuera de apuntado no hace nada");
        keys = new[] { Key.Q }; buttons = 2; yield return Wait(.08f); keys = Array.Empty<Key>(); buttons = 0; yield return Wait(.12f);
        Check(!Player.Aiming, "Q y clic derecho simultáneos no dejan apuntado activo");
        yield return Press(Key.Q); yield return Press(Key.Escape);
        Check(Pause.IsOpen && Player.Aiming, "ESC pausa durante apuntado sin perder estado");
        int medkits = inventory.Medkits; bool light = Player.GetComponentInChildren<FlashlightController>().IsOn;
        keys = new[] { Key.H, Key.E, Key.F, Key.R, Key.Tab }; yield return Wait(.12f);
        Check(inventory.Medkits == medkits && Player.GetComponentInChildren<FlashlightController>().IsOn == light && !Director.IsOpen, "Entradas de gameplay bloqueadas en pausa");
        yield return Click(Button("CONTINUAR"));
        Check(!Pause.IsOpen && GameplayPause.InputsBlocked && inventory.Bottles == stock && inventory.Medkits == medkits && Player.Aiming,
            "Clic de continuar y teclas sostenidas no lanzan, curan ni interactúan");
        keys = Array.Empty<Key>(); yield return Wait(.14f);
        Check(!GameplayPause.InputsBlocked && Player.Aiming && inventory.Bottles == stock, "Liberación de entradas conserva apuntado");
        buttons = 2; yield return Wait(.08f); buttons = 0; yield return Wait(.12f);
        Check(Player.CanAct && !Leo.IsBlocked, "Cancelación correcta tras reanudar");
        yield return Press(Key.Escape); keys = new[] { Key.H, Key.E };
        yield return Wait(.08f); yield return Click(Button("CONTINUAR"));
        Check(GameplayPause.InputsBlocked && inventory.Medkits == medkits && !Director.IsOpen, "H/E sostenidas al reanudar sin apuntado no consumen ni interactúan");
        keys = Array.Empty<Key>(); yield return Wait(.15f);
        Check(inventory.Medkits == medkits && Player.CanAct, "Soltar H/E no reproduce acciones aplazadas");
        yield return Press(Key.H); Check(inventory.Medkits == medkits - 1 && inventory.Health == 3, "Nuevo H intencional cura después de pausa");
        yield return Press(Key.Q);
        pointer = RectTransformUtility.WorldToScreenPoint(Camera.main, Leo.transform.position + Vector3.right * 1.5f);
        buttons = 1; yield return Wait(.08f); buttons = 0; yield return Wait(.4f);
        Check(inventory.Bottles == stock - 1 && bottleNoise == 1, "Clic izquierdo sigue lanzando e impactando una botella");
        yield return Press(Key.Escape); yield return Click(Button("OPCIONES"));
        Check(Pause.CurrentPage == PauseMenuController.Page.Options && Time.timeScale == 0, "Opciones abre sin reanudar");
        var effectSlider = Pause.GetComponentsInChildren<Slider>().First(s => s.name == "EFECTOS Slider");
        pointer = RectTransformUtility.WorldToScreenPoint(null, effectSlider.transform.TransformPoint(Vector3.left * 160));
        buttons = 1; yield return Wait(.1f); buttons = 0; yield return Wait(.1f);
        Check(MenuPreferences.EffectsVolume < .3f, "Slider cambia preferencia mediante clic real");
        float preference = MenuPreferences.EffectsVolume;
        File.WriteAllText(savedPreference, preference.ToString(System.Globalization.CultureInfo.InvariantCulture));
        yield return Click(Button("VOLVER")); yield return Click(Button("CONTROLES"));
        string controls = string.Join("\n", Pause.GetComponentsInChildren<Text>().Select(t => t.text));
        Check(Pause.CurrentPage == PauseMenuController.Page.Controls && controls.Contains("CLIC DERECHO") && controls.Contains("WASD / FLECHAS") && controls.Contains("H     Usar botiquín"), "Pantalla controles refleja bindings reales");
        yield return Click(Button("VOLVER")); yield return Click(Button("CONTINUAR"));
        Check(Mathf.Approximately(MenuPreferences.EffectsVolume, preference) && !GameplayPause.IsPaused, "Preferencia persiste al reanudar");
        var audio = Leo.GetComponent<LeoFootstepAudio>();
        keys = new[] { Key.A }; yield return Wait(.7f); keys = Array.Empty<Key>();
        Check(audio.PlayedCount > 0 && Leo.GetComponent<AudioSource>().volume <= preference, "Volumen real de pasos respeta preferencias");
        var audioProbe = new GameObject("Audio de prueba", typeof(AudioSource));
        var probeSource = audioProbe.GetComponent<AudioSource>(); probeSource.clip = audio.LastClip; probeSource.loop = true; probeSource.Play();
        yield return Wait(.12f); yield return Press(Key.Escape);
        int cursor = probeSource.timeSamples; double dsp = AudioSettings.dspTime;
        yield return Wait(.3f);
        Check(probeSource.timeSamples == cursor && AudioSettings.dspTime == dsp, "Cursor de AudioSource y reloj DSP congelados realmente");
        yield return Press(Key.Escape); yield return Wait(.15f);
        Check(probeSource.isPlaying && AudioSettings.dspTime > dsp, "Audio reanuda desde el punto pausado"); Destroy(audioProbe);
        SurvivalPlayer.Notify("test notice"); float beforeClock = GameplayPause.PresentationTime;
        yield return Press(Key.Escape); yield return Wait(.35f);
        Check(GameplayPause.PresentationTime - beforeClock < .12f, "Avisos con tiempo no escalado permanecen congelados");
        yield return Press(Key.Escape);
        Player.Cinematic = true; yield return Press(Key.Escape); Check(!Pause.IsOpen, "Cinemática rechaza pausa"); Player.Cinematic = false;
        Director.OpenTarget(RetenInteractionKind.Map); yield return Wait(.1f);
        Check(Director.IsOpen && !Pause.CanOpen, "Documento modal rechaza pausa"); yield return CloseModal();
        var fade = GeneratorGateFade.Create(); yield return Press(Key.Escape); Check(!Pause.IsOpen, "Fundido de transición rechaza pausa"); Destroy(fade.gameObject); yield return Wait(.1f);
        // Transition uses the game's real loader, snapshot and arrival point.
        Check(RetenTravel.Begin("Assets/Scenes/Nivel01/N1_InteriorReten.unity", "interior_puerta", Player.GetComponentInChildren<FlashlightBattery>(), Player.GetComponentInChildren<FlashlightController>()), "RetenTravel inicia tras pausa");
        yield return Until(() => SceneManager.GetActiveScene().name == "N1_InteriorReten" && !RetenTravel.IsLoading, 10, "Transición interior termina"); yield return Wait(.3f);
        Check(Pause != null && Player.CanAct && inventory == RetenMission.Session.Survival && inventory.Bottles == stock - 1, "Viaje conserva inventario y crea pausa funcional");
        yield return Press(Key.Escape); Check(Pause.IsOpen, "Pausa funciona después de viajar"); yield return Press(Key.Escape);
        RetenMission.Session.FinishEncounter(); yield return Load("N1_BosqueAbandonado");
        var enemy = FindAnyObjectByType<Escarbador>();
        enemy.Hear(new NoiseEvent(enemy.transform.position + Vector3.left * 2, 1, 16, NoiseKind.Bottle));
        yield return Wait(.9f); Check(enemy.State != EscarbadorState.Hidden, "IA activada por ruido existente");
        yield return Press(Key.Escape); var enemyPos = enemy.transform.position; var state = enemy.State; var heard = enemy.LastHeard;
        enemy.Hear(new NoiseEvent(enemy.transform.position + Vector3.right, 1, 16, NoiseKind.Bottle));
        yield return Wait(.5f); Check(enemy.transform.position == enemyPos && enemy.State == state && enemy.LastHeard == heard, "Escarbador inmóvil y sin nuevas decisiones en pausa");
        yield return Press(Key.Escape); Check(!enemy.Frozen && Player.CanAct, "IA y control se restauran sin alterar Frozen");
        enemy.ResetAtHideout(); inventory.Damage(3); yield return Wait(1.3f); yield return Press(Key.Escape);
        Check(Player.Dead && !Pause.IsOpen, "Muerte rechaza pausa");
        yield return Press(Key.Enter); yield return Wait(1);
        Check(!Player.Dead && Player.CanAct && Time.timeScale == 1 && !AudioListener.pause, "Reintento posterior a pausa restaura control");
        yield return Press(Key.Escape); yield return Click(Button("VOLVER AL MENÚ PRINCIPAL"));
        Check(Pause.CurrentPage == PauseMenuController.Page.ConfirmExit && SceneManager.GetActiveScene().name == "N1_BosqueAbandonado", "Salir requiere confirmación");
        yield return Click(Button("CANCELAR")); Check(Pause.CurrentPage == PauseMenuController.Page.Pause && Pause.IsOpen, "Cancelar salida mantiene partida pausada");
        yield return Click(Button("VOLVER AL MENÚ PRINCIPAL")); yield return Click(Button("VOLVER AL MENÚ PRINCIPAL"));
        yield return Until(() => SceneManager.GetActiveScene().name == "MainMenu", 10, "Salida confirmada carga MainMenu"); yield return Wait(1.3f);
        Check(Time.timeScale == 1 && !AudioListener.pause && !GameplayPause.IsPaused && FindObjectsByType<PauseMenuController>().Length == 0, "Menú sin estado global de pausa ni controlador residual");
        var menu = FindAnyObjectByType<MainMenuController>();
        Check(menu.GetComponent<AudioSource>().isPlaying && Mathf.Approximately(MenuPreferences.EffectsVolume, preference), "Menú recupera música y preferencias");
        menu.newGame.onClick.Invoke(); yield return Until(() => SceneManager.GetActiveScene().name == "IntroHistoria", 10, "Nueva partida carga introducción");
        Check(RetenMission.Session.Survival.Bottles == 0 && !RetenMission.Session.TreeFallen && Time.timeScale == 1 && !AudioListener.pause, "Reinicio existente de sesión sigue intacto");
        yield return Press(Key.Space, 1.3f); yield return Until(() => SceneManager.GetActiveScene().name == "N1_ExteriorReten", 12, "Omitir intro lleva al exterior"); yield return Wait(.5f);
        Check(Pause != null && Player.CanAct && !GameplayPause.InputsBlocked, "Nueva partida no empieza congelada");
        Time.timeScale = .5f; yield return Press(Key.Escape); yield return Press(Key.Escape);
        Check(Time.timeScale == .5f, "Pausa restaura escala previa sin imponer una nueva"); Time.timeScale = 1;
        yield return Press(Key.Escape); Pause.enabled = false; yield return Wait(.2f);
        Check(Time.timeScale == 1 && !AudioListener.pause && !GameplayPause.IsPaused && HudVisibility.IsVisible && Player.CanAct, "Desactivar propietario libera todos los estados globales y la entrada"); Pause.enabled = true;
        yield return Wait(.2f); yield return Press(Key.Escape);
        foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(960, 720) })
        {
            // Runtime assembly cannot reference Assembly-CSharp-Editor directly.
            AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("PauseValidationEditor")).First(t => t != null)
                .GetMethod("SetSize").Invoke(null, new object[] { size.x, size.y }); yield return Wait(.4f);
            Check(Screen.width == size.x && Screen.height == size.y, "Render real en " + size.x + "x" + size.y);
            foreach (PauseMenuController.Page page in Enum.GetValues(typeof(PauseMenuController.Page)))
            {
                Pause.ShowPage(page); yield return Wait(.2f);
                bool inside = true;
                foreach (var label in Pause.GetComponentsInChildren<Text>())
                {
                    var corners = new Vector3[4]; label.rectTransform.GetWorldCorners(corners);
                    foreach (var corner in corners) { var point = RectTransformUtility.WorldToScreenPoint(null, corner); inside &= point.x >= -1 && point.y >= -1 && point.x <= Screen.width + 1 && point.y <= Screen.height + 1; }
                }
                Check(inside, "Todos los textos y botones visibles: " + page + " " + size.x + "x" + size.y);
                ScreenCapture.CaptureScreenshot(Path.Combine(reports, page + "_" + size.x + "x" + size.y + ".png")); yield return Wait(.15f);
            }
        }
        Pause.Resume();
    }
}
#endif
