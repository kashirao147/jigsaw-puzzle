using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingBar : MonoBehaviour
{
    public Slider loadingSlider;  // Assign in the Inspector
    public float fillTime = 4f;   // Total time to fill the slider

    void Start()
    {
        StartCoroutine(FillSlider());
    }

    IEnumerator FillSlider()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fillTime)
        {
            elapsedTime += Time.deltaTime;
            loadingSlider.value = Mathf.Clamp01(elapsedTime / fillTime);
            yield return null;
        }

        loadingSlider.value = 0.8f; // Ensure it's fully filled
    }
}
