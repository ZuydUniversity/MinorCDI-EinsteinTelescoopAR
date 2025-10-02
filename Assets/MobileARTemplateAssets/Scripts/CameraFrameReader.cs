using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARCameraManager))]
public class CameraFrameReader : MonoBehaviour
{
    private ARCameraManager cameraManager;
    public Texture2D cameraTexture;

    void Awake()
    {
        cameraManager = GetComponent<ARCameraManager>();
    }

    void Update()
    {
        if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // Set up conversion parameters
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width, image.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            // Create or resize texture if needed
            if (cameraTexture == null || cameraTexture.width != image.width || cameraTexture.height != image.height)
            {
                cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
            }

            // Convert and apply
            var rawData = cameraTexture.GetRawTextureData<byte>();
            image.Convert(conversionParams, rawData);
            cameraTexture.Apply();

            image.Dispose();
        }
    }
}
