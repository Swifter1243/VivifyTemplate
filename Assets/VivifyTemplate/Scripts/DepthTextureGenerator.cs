using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class DepthTextureGenerator : MonoBehaviour
{
    // Use this variable to toggle depth texture generation
    public bool generateDepthTexture = true;

    void OnEnable()
    {
        // Enable depth texture generation initially
        UpdateDepthTextureMode(GetComponent<Camera>());

#if UNITY_EDITOR
        // Subscribe to the onPreCull event for both the Scene view camera and the game camera
        Camera.onPreCull += OnCameraPreCull;
#endif
    }

#if UNITY_EDITOR
    void OnDisable()
    {
        // Unsubscribe from the onPreCull event when the script is disabled
        Camera.onPreCull -= OnCameraPreCull;
    }

    void OnCameraPreCull(Camera camera)
    {
        // Update depth texture mode for the cameras
        UpdateDepthTextureMode(camera);
    }
#endif

    void UpdateDepthTextureMode(Camera cameraToUpdate)
    {
        // Enable or disable depth texture generation
        cameraToUpdate.depthTextureMode = generateDepthTexture ? DepthTextureMode.Depth : DepthTextureMode.None;
    }
}
