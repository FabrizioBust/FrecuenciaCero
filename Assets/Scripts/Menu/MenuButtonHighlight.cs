using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    [RequireComponent(typeof(Button))]
    public sealed class MenuButtonHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        public Text label;
        public Text marker;
        public Image border;
        private Button button;
        private Image background;
        private bool hovered, selected;
        private float amount;
        private static readonly Color Red = new Color(.95f, .16f, .18f);
        private void Awake() { button = GetComponent<Button>(); background = GetComponent<Image>(); }
        private void OnDisable() { hovered = selected = false; amount = 0; }
        public void OnSelect(BaseEventData data) { selected = true; }
        public void OnDeselect(BaseEventData data) { selected = false; }
        public void OnPointerEnter(PointerEventData data)
        {
            hovered = true;
            if (button.IsInteractable()) button.Select();
        }
        public void OnPointerExit(PointerEventData data) { hovered = false; }
        private void Update()
        {
            amount = Mathf.MoveTowards(amount, button.IsInteractable() && (hovered || selected) ? 1 : 0,
                Time.unscaledDeltaTime * 7f);
            if (label != null) label.color = Color.Lerp(new Color(.8f, .79f, .75f), Red, amount);
            if (marker != null) marker.color = new Color(Red.r, Red.g, Red.b, amount);
            if (border != null) border.color = Color.Lerp(new Color(.55f, .55f, .52f, .4f), Red, amount);
            background.color = Color.Lerp(new Color(0, 0, 0, .58f), new Color(.35f, .02f, .025f, .75f), amount);
        }
    }
}
