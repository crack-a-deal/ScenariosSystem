
using UnityEngine;

public class CameraSwitcher
{
    private readonly Camera _mainCamera;

    public CameraSwitcher(Camera mainCamera)
    {
        _mainCamera = mainCamera;
    }

    public void Activate(Camera camera)
    {
        SetCameraEnabled(_mainCamera, false);
        SetCameraEnabled(camera, true);
    }

    public void RestoreMainCamera()
    {
        SetCameraEnabled(_mainCamera, true);
    }

    private static void SetCameraEnabled(Camera camera, bool enabled)
    {
        if (camera == null)
            return;

        if (camera.CompareTag("PreviewCamera"))
            return;

        camera.enabled = enabled;

        AudioListener listener = camera.GetComponent<AudioListener>();

        if (listener != null)
            listener.enabled = enabled;
    }
}
