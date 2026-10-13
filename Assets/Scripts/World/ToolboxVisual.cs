using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class ToolboxVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer closed, body, lid, fuse;
        [SerializeField] private GameObject cavity;
        [SerializeField] private float openingDuration = .45f;
        private Vector3 lidRest;
        private float elapsed;
        public bool IsOpening { get; private set; }
        private void Awake() { lidRest = lid.transform.localPosition; }
        private void Start() { Apply(RetenMission.Session.ToolboxOpened ? 1 : 0); }
        public void BeginOpening() { if (IsOpening) return; IsOpening = true; elapsed = 0; Apply(0); }
        private void Update()
        {
            if (IsOpening)
            {
                elapsed += Time.deltaTime;
                float amount = Mathf.SmoothStep(0, 1, elapsed / openingDuration);
                if (elapsed >= openingDuration) IsOpening = false;
                Apply(amount);
            }
            else Apply(RetenMission.Session.ToolboxOpened ? 1 : 0);
        }
        private void Apply(float amount)
        {
            bool open = RetenMission.Session.ToolboxOpened;
            closed.enabled = !open;
            body.enabled = lid.enabled = open;
            cavity.SetActive(open);
            lid.transform.localPosition = lidRest + Vector3.up * (.46f * amount);
            lid.transform.localScale = new Vector3(1, Mathf.Lerp(1, .62f, amount), 1);
            fuse.enabled = open && !IsOpening && !RetenMission.Session.FuseCollected;
        }
    }
}
