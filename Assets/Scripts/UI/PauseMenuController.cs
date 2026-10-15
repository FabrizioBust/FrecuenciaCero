using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    [DefaultExecutionOrder(-1000), DisallowMultipleComponent]
    public sealed class PauseMenuController : MonoBehaviour
    {
        public enum Page { Pause, Options, Controls, ConfirmExit }
        public Page CurrentPage { get; private set; }
        public bool IsOpen => ownsPause;
        private SurvivalPlayer player;
        private Canvas canvas;
        private CanvasGroup group;
        private RectTransform[] pages;
        private Button[] firstButtons;
        private Slider music, effects;
        private Text musicPercent, effectsPercent, displayLabel;
        private Toggle fullscreen;
        private System.Collections.Generic.List<Vector2Int> resolutions;
        private int resolutionIndex;
        private Font font;
        private bool ownsPause, leaving;
        private float previousScale;
        private bool previousAudioPause;
        private IDisposable hiddenHud;
        private GameObject ownEventSystem;
        private GameObject previousSelection;
        private static readonly Color Ink = new Color(.018f, .025f, .032f, .96f);
        private static readonly Color Paper = new Color(.83f, .83f, .78f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= Attach;
            SceneManager.sceneLoaded += Attach;
        }
        private static void Attach(Scene scene, LoadSceneMode mode)
        {
            // Scenes retain their serialized cameras, colliders and narrative components.
            var leo = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<SurvivalPlayer>())
                .FirstOrDefault(p => p.isActiveAndEnabled);
            if (leo == null || leo.GetComponent<PauseMenuController>() != null) return;
            leo.gameObject.AddComponent<PauseMenuController>();
        }
        private void Awake()
        {
            player = GetComponent<SurvivalPlayer>();
            BuildUI();
        }
        public bool CanOpen
        {
            get
            {
                if (leaving || Time.timeScale <= 0 || GameplayPause.InputsBlocked || player == null || !player.CanAct) return false;
                // This short narrative event uses realtime waits and must finish uninterrupted.
                var tree = FindAnyObjectByType<TreeFallEvent>();
                return (tree == null || !tree.IsRunning) && FindAnyObjectByType<GeneratorGateFade>() == null &&
                    FindAnyObjectByType<ChapterTransitionOverlay>() == null;
            }
        }
        private void Update()
        {
            GameplayPause.UpdateReleaseGate();
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (ownsPause) Resume();
                else if (CanOpen) Open();
            }
            if (ownsPause) group.alpha = Mathf.MoveTowards(group.alpha, 1, Time.unscaledDeltaTime * 9);
        }
        public void Open()
        {
            if (ownsPause || !CanOpen) return;
            previousScale = Time.timeScale;
            previousAudioPause = AudioListener.pause;
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            ownsPause = true;
            GameplayPause.Begin();
            Time.timeScale = 0;
            AudioListener.pause = true;
            hiddenHud = HudVisibility.HideForCinematic();
            canvas.gameObject.SetActive(true);
            if (EventSystem.current == null)
            {
                if (ownEventSystem == null)
                {
                    ownEventSystem = new GameObject("Pausa EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                    ownEventSystem.transform.SetParent(transform, false);
                    ownEventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                }
                ownEventSystem.SetActive(true);
            }
            group.alpha = 0;
            ShowPage(Page.Pause);
        }
        public void Resume()
        {
            if (!ownsPause || leaving) return;
            RestorePause();
            PlayerPrefs.Save();
        }
        private void RestorePause()
        {
            if (!ownsPause) return;
            ownsPause = false;
            GameplayPause.End();
            Time.timeScale = previousScale;
            AudioListener.pause = previousAudioPause;
            hiddenHud?.Dispose(); hiddenHud = null;
            if (canvas != null) canvas.gameObject.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(previousSelection);
            if (ownEventSystem != null) ownEventSystem.SetActive(false);
        }
        public void ShowPage(Page page)
        {
            if (!ownsPause) return;
            CurrentPage = page;
            foreach (var p in pages) p.gameObject.SetActive(false);
            pages[(int)page].gameObject.SetActive(true);
            if (page == Page.Options)
            {
                music.SetValueWithoutNotify(MenuPreferences.MusicVolume);
                effects.SetValueWithoutNotify(MenuPreferences.EffectsVolume);
                fullscreen.SetIsOnWithoutNotify(Screen.fullScreen);
                resolutions = MenuPreferences.CompatibleResolutions();
                resolutionIndex = resolutions.FindIndex(s => s.x == Screen.width && s.y == Screen.height);
                resolutionIndex = Mathf.Max(0, resolutionIndex);
                RefreshDisplay(); RefreshAudio();
            }
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(firstButtons[(int)page].gameObject);
        }
        public void ReturnToMainMenu()
        {
            if (!ownsPause || CurrentPage != Page.ConfirmExit || leaving) return;
            if (!Application.CanStreamedLevelBeLoaded("MainMenu")) { Debug.LogError("MainMenu falta en la lista de escenas del Build."); return; }
            leaving = true;
            RestorePause();
            PlayerPrefs.Save();
            // MainMenu.NewGame owns the existing reset of mission, survival, HUD and travel.
            // Complete the menu switch before another gameplay Update can consume input.
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
        private void OnDisable() { RestorePause(); }
        private void OnDestroy() { if (canvas != null) Destroy(canvas.gameObject); }
        private void RefreshAudio()
        {
            musicPercent.text = Mathf.RoundToInt(MenuPreferences.MusicVolume * 100) + " %";
            effectsPercent.text = Mathf.RoundToInt(MenuPreferences.EffectsVolume * 100) + " %";
        }
        private void RefreshDisplay()
        {
            var size = resolutions[resolutionIndex];
            displayLabel.text = "RESOLUCIÓN: " + size.x + " × " + size.y;
        }
        private void ApplyDisplay() { MenuPreferences.SetDisplay(resolutions[resolutionIndex], fullscreen.isOn); }
        private void BuildUI()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("Menú de pausa", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            group = root.GetComponent<CanvasGroup>();
            var dim = new GameObject("Escenario oscurecido", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(root.transform, false);
            var dimRect = (RectTransform)dim.transform; dimRect.anchorMin = Vector2.zero; dimRect.anchorMax = Vector2.one; dimRect.offsetMin = dimRect.offsetMax = Vector2.zero;
            dim.GetComponent<Image>().color = new Color(0, .005f, .015f, .72f);
            pages = new RectTransform[4]; firstButtons = new Button[4];
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i] = Rect(root.transform, ((Page)i).ToString(), 0, 0, 650, 620);
                pages[i].gameObject.AddComponent<Image>().color = Ink;
                var border = pages[i].gameObject.AddComponent<Outline>(); border.effectDistance = new Vector2(1, -1); border.effectColor = new Color(.38f, .39f, .35f);
            }
            var home = pages[0];
            Label(home, "FRECUENCIA CERO", 245, 570, 21);
            Label(home, "PAUSA", 180, 570, 38);
            firstButtons[0] = Button(home, "CONTINUAR", 70, Resume);
            Button(home, "OPCIONES", 0, () => ShowPage(Page.Options));
            Button(home, "CONTROLES", -70, () => ShowPage(Page.Controls));
            Button(home, "VOLVER AL MENÚ PRINCIPAL", -160, () => ShowPage(Page.ConfirmExit));
            var opts = pages[1]; Label(opts, "OPCIONES", 245, 570, 34);
            music = Volume(opts, "MÚSICA", 150, out musicPercent);
            effects = Volume(opts, "EFECTOS", 55, out effectsPercent);
            music.onValueChanged.AddListener(v => { MenuPreferences.SetMusic(v); RefreshAudio(); });
            effects.onValueChanged.AddListener(v => { MenuPreferences.SetEffects(v); RefreshAudio(); });
            var toggleRect = Rect(opts, "Pantalla completa", 0, -70, 500, 44);
            toggleRect.gameObject.AddComponent<Image>().color = Color.clear;
            fullscreen = toggleRect.gameObject.AddComponent<Toggle>();
            var box = Rect(toggleRect, "Casilla", -210, 0, 30, 30).gameObject.AddComponent<Image>(); box.color = new Color(.25f, .27f, .26f);
            var check = Rect(box.transform, "Marca", 0, 0, 20, 20).gameObject.AddComponent<Image>(); check.color = new Color(.85f, .18f, .19f);
            fullscreen.targetGraphic = box; fullscreen.graphic = check;
            var caption = Label(toggleRect, "PANTALLA COMPLETA", 0, 380, 21); caption.rectTransform.anchoredPosition = new Vector2(25, 0);
            fullscreen.onValueChanged.AddListener(_ => ApplyDisplay());
            var display = Button(opts, "RESOLUCIÓN", -130, () => { resolutionIndex = (resolutionIndex + 1) % resolutions.Count; RefreshDisplay(); ApplyDisplay(); });
            displayLabel = display.GetComponent<MenuButtonHighlight>().label;
            firstButtons[1] = Button(opts, "VOLVER", -245, () => { PlayerPrefs.Save(); ShowPage(Page.Pause); });
            var controls = pages[2]; Label(controls, "CONTROLES", 245, 570, 34);
            Label(controls, "WASD / FLECHAS     Mover a Leo\nSHIFT     Correr\nE     Interactuar\nF     Encender / apagar linterna\nRATÓN     Orientar la linterna\nQ     Apuntar botella\nCLIC IZQUIERDO     Lanzar botella\nCLIC DERECHO     Cancelar apuntado\nH     Usar botiquín\nR     Recargar batería\nTAB     Mostrar / ocultar HUD\nESC     Pausar / reanudar", 10, 560, 21, 405, TextAnchor.MiddleLeft);
            Label(controls, "Diálogos: E / Enter · Documentos: E / ESC\nReintentar después de morir: Enter", -185, 560, 16, 42);
            firstButtons[2] = Button(controls, "VOLVER", -245, () => ShowPage(Page.Pause));
            var confirm = pages[3]; Label(confirm, "VOLVER AL MENÚ", 180, 570, 32);
            Label(confirm, "¿Quieres abandonar la partida?\nNo existe guardado de sesión.\nNueva partida reiniciará el progreso actual.", 45, 550, 23, 150);
            firstButtons[3] = Button(confirm, "CANCELAR", -95, () => ShowPage(Page.Pause));
            Button(confirm, "VOLVER AL MENÚ PRINCIPAL", -180, ReturnToMainMenu);
            root.SetActive(false);
        }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var rect = (RectTransform)obj.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height); return rect;
        }
        private Text Label(Transform parent, string text, float y, float width, int size, float height = 48, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var label = Rect(parent, text, 0, y, width, height).gameObject.AddComponent<Text>();
            label.font = font; label.fontSize = size; label.text = text; label.color = Paper;
            label.alignment = align; label.raycastTarget = false; label.supportRichText = false; return label;
        }
        private Button Button(Transform parent, string text, float y, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(parent, text, 0, y, 550, 52);
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0, 0, 0, .6f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action);
            var highlight = rect.gameObject.AddComponent<MenuButtonHighlight>();
            highlight.label = Label(rect, text, 0, 470, 23);
            highlight.label.rectTransform.anchoredPosition = new Vector2(20, 0);
            highlight.marker = Label(rect, "▶", 0, 25, 18); highlight.marker.rectTransform.anchoredPosition = new Vector2(-250, 0);
            return button;
        }
        private Slider Volume(Transform parent, string title, float y, out Text percent)
        {
            Label(parent, title, y + 27, 400, 21).rectTransform.anchoredPosition = new Vector2(-55, y + 27);
            percent = Label(parent, "0 %", y + 27, 110, 21); percent.rectTransform.anchoredPosition = new Vector2(235, y + 27);
            var rect = Rect(parent, title + " Slider", 0, y - 18, 500, 30);
            var background = rect.gameObject.AddComponent<Image>(); background.color = new Color(.14f, .17f, .18f);
            var slider = rect.gameObject.AddComponent<Slider>(); slider.minValue = 0; slider.maxValue = 1;
            var area = Rect(rect, "Área", 0, 0, 480, 30);
            var fill = Rect(area, "Nivel", 0, 0, 480, 8).gameObject.AddComponent<Image>(); fill.color = new Color(.64f, .12f, .15f);
            // Slider drives the horizontal anchors; a fixed width would spill out of its track.
            fill.rectTransform.anchorMin = new Vector2(0, .5f);
            fill.rectTransform.anchorMax = new Vector2(1, .5f);
            fill.rectTransform.sizeDelta = Vector2.zero;
            var handle = Rect(area, "Control", 0, 0, 16, 30).gameObject.AddComponent<Image>(); handle.color = Paper;
            slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
            return slider;
        }
    }
}
