using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class IntroParallaxLayer : MonoBehaviour
    {
        [Min(0)] public float speedRatio = 1;
        [Min(1)] public float repeatWidth = 40;
        private Vector3[] origins;

        private void Awake()
        {
            origins = new Vector3[transform.childCount];
            for (int i = 0; i < origins.Length; i++) origins[i] = transform.GetChild(i).localPosition;
        }

        public void SetDistance(float distance)
        {
            if (origins == null) return;
            for (int i = 0; i < origins.Length; i++)
            {
                Vector3 position = origins[i];
                position.x = Mathf.Repeat(position.x - distance * speedRatio + repeatWidth / 2,
                    repeatWidth) - repeatWidth / 2;
                transform.GetChild(i).localPosition = position;
            }
        }
    }
}
