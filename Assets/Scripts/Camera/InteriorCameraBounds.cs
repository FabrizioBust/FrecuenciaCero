using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FrecuenciaCero
{
    // Runs after SmoothCameraFollow and before flashlight aiming. Exterior follow is unchanged.
    [DefaultExecutionOrder(50), RequireComponent(typeof(Camera))]
    public sealed class InteriorCameraBounds : MonoBehaviour
    {
        [SerializeField] private Rect roomBounds = new Rect(-5.5f, -4.5f, 11f, 9f);
        private Camera worldCamera;
        private PixelPerfectCamera pixelPerfect;
        private void Awake()
        {
            worldCamera = GetComponent<Camera>();
            pixelPerfect = GetComponent<PixelPerfectCamera>();
        }
        private void LateUpdate()
        {
            float halfY = worldCamera.orthographicSize;
            float aspect = worldCamera.aspect;
            if (pixelPerfect != null && pixelPerfect.enabled &&
                (pixelPerfect.cropFrame == PixelPerfectCamera.CropFrame.Windowbox ||
                 pixelPerfect.cropFrame == PixelPerfectCamera.CropFrame.StretchFill))
                aspect = pixelPerfect.refResolutionX / (float)pixelPerfect.refResolutionY;
            float halfX = halfY * aspect;
            Vector3 p = transform.position;
            p.x = ClampCenter(p.x, roomBounds.xMin, roomBounds.xMax, halfX);
            p.y = ClampCenter(p.y, roomBounds.yMin, roomBounds.yMax, halfY);
            p.z = -10f;
            transform.position = p;
        }
        private static float ClampCenter(float value, float min, float max, float half)
        {
            return max - min <= half * 2f ? (min + max) * .5f : Mathf.Clamp(value, min + half, max - half);
        }
    }
}
