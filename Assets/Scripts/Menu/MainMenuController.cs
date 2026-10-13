using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
    public sealed class MainMenuController : MonoBehaviour
    {
        public const string GameplayScene = "N1_ExteriorReten";
        public const string IntroScene = "IntroHistoria";
        public AudioClip menuMusic;
        public CanvasGroup mainPage, optionsPage, creditsPage;
        public CanvasGroup fade;
        public Button newGame, options, credits, quit, optionsBack, creditsBack;
        public Slider musicVolume, effectsVolume;
        public Text musicPercent, effectsPercent;
        public Toggle fullscreen;
        public Dropdown resolution;
        private AudioSource music;
        private List<Vector2Int> resolutions;
        private CanvasGroup current;
        private bool busy;
        private float musicFade;
        public bool IsTransitioning => busy;

        private void Awake()
        {
            music = GetComponent<AudioSource>();
            music.playOnAwake = false;
            music.loop = true;
            music.spatialBlend = 0;
            music.clip = menuMusic;
            music.volume = 0;
            MenuPreferences.ApplyDisplay();
            musicVolume.SetValueWithoutNotify(MenuPreferences.MusicVolume);
            effectsVolume.SetValueWithoutNotify(MenuPreferences.EffectsVolume);
            RefreshAudio();
            resolutions = MenuPreferences.CompatibleResolutions();
            resolution.ClearOptions();
            var labels = new List<string>();
            int selected = 0;
            for (int i = 0; i < resolutions.Count; i++)
            {
                Vector2Int size = resolutions[i];
                labels.Add(size.x + " × " + size.y);
                if (size.x == PlayerPrefs.GetInt(MenuPreferences.WidthKey, Screen.width) &&
                    size.y == PlayerPrefs.GetInt(MenuPreferences.HeightKey, Screen.height)) selected = i;
            }
            resolution.AddOptions(labels);
            resolution.SetValueWithoutNotify(selected);
            fullscreen.SetIsOnWithoutNotify(PlayerPrefs.GetInt(MenuPreferences.FullscreenKey, Screen.fullScreen ? 1 : 0) == 1);
            newGame.onClick.AddListener(StartNewGame);
            options.onClick.AddListener(() => Show(optionsPage, musicVolume.gameObject));
            credits.onClick.AddListener(() => Show(creditsPage, creditsBack.gameObject));
            optionsBack.onClick.AddListener(ReturnToMain);
            creditsBack.onClick.AddListener(ReturnToMain);
            quit.onClick.AddListener(Quit);
            musicVolume.onValueChanged.AddListener(MenuPreferences.SetMusic);
            effectsVolume.onValueChanged.AddListener(MenuPreferences.SetEffects);
            fullscreen.onValueChanged.AddListener(_ => SaveDisplay());
            resolution.onValueChanged.AddListener(_ => SaveDisplay());
            mainPage.gameObject.SetActive(true);
            optionsPage.gameObject.SetActive(false);
            creditsPage.gameObject.SetActive(false);
            current = mainPage;
            fade.alpha = 1;
            fade.blocksRaycasts = true;
            MenuPreferences.AudioChanged += RefreshAudio;
        }
        private void Start()
        {
            if (menuMusic != null) music.Play();
            StartCoroutine(Enter());
        }
        private void OnDestroy() { MenuPreferences.AudioChanged -= RefreshAudio; PlayerPrefs.Save(); }
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame &&
                current != mainPage && !busy) ReturnToMain();
        }
        private void RefreshAudio()
        {
            music.volume = MenuPreferences.MusicVolume * musicFade;
            musicPercent.text = Mathf.RoundToInt(MenuPreferences.MusicVolume * 100) + " %";
            effectsPercent.text = Mathf.RoundToInt(MenuPreferences.EffectsVolume * 100) + " %";
        }
        private void SaveDisplay()
        {
            if (resolutions.Count > 0) MenuPreferences.SetDisplay(resolutions[resolution.value], fullscreen.isOn);
        }
        private IEnumerator Enter()
        {
            busy = true;
            float time = 0;
            while (time < 1.2f)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0, 1, time / 1.2f);
                fade.alpha = 1 - t;
                musicFade = t;
                RefreshAudio();
                yield return null;
            }
            fade.alpha = 0;
            fade.blocksRaycasts = false;
            busy = false;
            newGame.Select();
        }
        private void Show(CanvasGroup page, GameObject selection)
        {
            if (!busy && current != page) StartCoroutine(ChangePage(page, selection));
        }
        private void ReturnToMain()
        {
            PlayerPrefs.Save();
            Show(mainPage, options.gameObject);
        }
        private IEnumerator ChangePage(CanvasGroup page, GameObject selection)
        {
            busy = true;
            current.interactable = false;
            EventSystem.current.SetSelectedGameObject(null);
            yield return FadePage(current, 0);
            current.gameObject.SetActive(false);
            current = page;
            page.alpha = 0;
            page.gameObject.SetActive(true);
            page.interactable = false;
            yield return FadePage(page, 1);
            page.interactable = true;
            busy = false;
            EventSystem.current.SetSelectedGameObject(selection);
        }
        private static IEnumerator FadePage(CanvasGroup page, float target)
        {
            float from = page.alpha;
            for (float t = 0; t < .22f; t += Time.unscaledDeltaTime)
            {
                page.alpha = Mathf.Lerp(from, target, Mathf.SmoothStep(0, 1, t / .22f));
                yield return null;
            }
            page.alpha = target;
        }
        public void StartNewGame()
        {
            if (busy) return;
            if (!Application.CanStreamedLevelBeLoaded(IntroScene) ||
                !Application.CanStreamedLevelBeLoaded(GameplayScene))
            {
                Debug.LogError("MainMenu: faltan IntroHistoria o N1_ExteriorReten en la lista de escenas del Build.");
                return;
            }
            StartCoroutine(NewGame());
        }
        private IEnumerator NewGame()
        {
            busy = true;
            current.interactable = false;
            fade.blocksRaycasts = true;
            for (float t = 0; t < .8f; t += Time.unscaledDeltaTime)
            {
                float value = Mathf.SmoothStep(0, 1, t / .8f);
                fade.alpha = value;
                musicFade = 1 - value;
                RefreshAudio();
                yield return null;
            }
            fade.alpha = 1;
            music.Stop();
            music.volume = 0;
            PlayerPrefs.Save();
            RetenMission.ResetSession();
            RetenTravel.Reset();
            HudVisibility.ResetSession();
            Time.timeScale = 1;
            // Single unloads this scene and its AudioSource; no persistent copy of menu music.
            yield return SceneManager.LoadSceneAsync(IntroScene, LoadSceneMode.Single);
        }
        public void Quit()
        {
            if (busy) return;
            PlayerPrefs.Save();
            music.Stop();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
