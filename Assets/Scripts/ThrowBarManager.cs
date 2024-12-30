using UnityEngine;
using UnityEngine.UI;

public class ThrowBarManager : MonoBehaviour
{
    private Slider throwSlider;
    private Image backgroundImage;
    private Image fillImage;

    private void Awake()
    {
        throwSlider = this.GetComponent<Slider>();
        backgroundImage = this.transform.Find("Background").GetComponent<Image>();
        fillImage = this.transform.Find("Fill").GetComponent<Image>();

        backgroundImage.enabled = false;
        fillImage.enabled = false;
        throwSlider.value = 0f;
    }

    private void Update()
    {
        if (throwSlider.value != 0)
        {
            backgroundImage.enabled = true;
            fillImage.enabled = true;
        }
        else
        {
            backgroundImage.enabled = false;
            fillImage.enabled = false;
        }
    }

    public void SetValuePercentage(float percentage)
    {
        throwSlider.value = Mathf.Clamp(percentage,0,1);
    }
}
