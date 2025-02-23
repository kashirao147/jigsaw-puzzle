using UnityEngine;

public class AutoResizeSprite : MonoBehaviour
{
    private void Start()
    {
        AdjustSpriteSize();
    }

    public void AdjustSpriteSize()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on the GameObject.");
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found.");
            return;
        }

        // Get the world size of the camera
        float cameraHeight = mainCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Get the sprite's size
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // Calculate scale factors
        float scaleX = cameraWidth / spriteSize.x;
        float scaleY = cameraHeight / spriteSize.y;

        // Apply the scale to fit the camera
        transform.localScale = new Vector3(scaleX, scaleY, 1);

        // Center the sprite
        transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, transform.position.z);
    }

    private void Update()
    {
        // Optional: Adjust the sprite size dynamically when screen size changes
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            AdjustSpriteSize();
        }
    }

    private int lastScreenWidth, lastScreenHeight;
}
