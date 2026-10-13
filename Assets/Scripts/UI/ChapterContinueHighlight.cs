using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    [RequireComponent(typeof(Button))]
    public sealed class ChapterContinueHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Text label, marker;
        public Image border;
        private Button button;
        private Image background;
        private bool selected, hovered;
        private float amount;
        private static readonly Color Accent = new Color(.80f, .74f, .62f);
        private void Awake() { button = GetComponent<Button>(); background = GetComponent<Image>(); }
        public void OnSelect(BaseEventData e) => selected = true;
        public void OnDeselect(BaseEventData e) => selected = false;
        public void OnPointerEnter(PointerEventData e) { hovered = true; if (button.IsInteractable()) button.Select(); }
        public void OnPointerExit(PointerEventData e) => hovered = false;
        private void OnDisable() { hovered = selected = false; amount = 0; }
        private void Update()
        {
            amount = Mathf.MoveTowards(amount, button.IsInteractable() && (selected || hovered) ? 1 : 0, Time.unscaledDeltaTime * 5);
            label.color = Color.Lerp(new Color(.67f, .67f, .62f), Accent, amount);
            marker.color = new Color(Accent.r, Accent.g, Accent.b, amount);
            border.color = Color.Lerp(new Color(.45f, .46f, .43f, .3f), Accent, amount);
            background.color = Color.Lerp(new Color(0, 0, 0, .35f), new Color(.13f, .12f, .10f, .65f), amount);
        }
    }
}
