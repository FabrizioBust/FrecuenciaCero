using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FrecuenciaCero
{
    // After smooth follow, before flashlight aiming and URP's pixel snap. Exterior only.
    [DefaultExecutionOrder(50), DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class ForestCameraBounds : MonoBehaviour
    {
        [SerializeField, Tooltip("Rectangle fully covered by ground and forest decoration, in world units.")]
        private Rect decoratedBounds = new Rect(-35f, -23f, 70f, 46f);
        [SerializeField, Min(0f), Tooltip("Extra inset for pixel rounding at the viewport edges.")]
        private float edgePadding = .125f;
        private Camera worldCamera;
        private PixelPerfectCamera pixelPerfect;

        private void Awake()
        {
            worldCamera = GetComponent<Camera>();
            pixelPerfect = GetComponent<PixelPerfectCamera>();
        }

        private void LateUpdate()
        {
            double halfY = worldCamera.orthographicSize;
            double halfX = halfY * worldCamera.aspect;
            if (pixelPerfect != null && pixelPerfect.isActiveAndEnabled)
            {
                int width = worldCamera.targetTexture != null ? worldCamera.targetTexture.width : Screen.width;
                int height = worldCamera.targetTexture != null ? worldCamera.targetTexture.height : Screen.height;
                PixelPerfectCamera.CropFrame crop = pixelPerfect.cropFrame;
                bool cropX = crop == PixelPerfectCamera.CropFrame.Pillarbox ||
                    crop == PixelPerfectCamera.CropFrame.Windowbox || crop == PixelPerfectCamera.CropFrame.StretchFill;
                bool cropY = crop == PixelPerfectCamera.CropFrame.Letterbox ||
                    crop == PixelPerfectCamera.CropFrame.Windowbox || crop == PixelPerfectCamera.CropFrame.StretchFill;
                ForestViewGeometry.PixelPerfectHalfSize(width, height, pixelPerfect.refResolutionX,
                    pixelPerfect.refResolutionY, pixelPerfect.assetsPPU, cropX, cropY,
                    pixelPerfect.gridSnapping == PixelPerfectCamera.GridSnapping.UpscaleRenderTexture,
                    out halfX, out halfY);
            }
            Vector3 position = transform.position;
            position.x = (float)ForestViewGeometry.ClampCenter(position.x,
                decoratedBounds.xMin, decoratedBounds.xMax, halfX + edgePadding);
            position.y = (float)ForestViewGeometry.ClampCenter(position.y,
                decoratedBounds.yMin, decoratedBounds.yMax, halfY + edgePadding);
            transform.position = position;
        }
    }
}
