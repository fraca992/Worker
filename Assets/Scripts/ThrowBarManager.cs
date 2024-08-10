using UnityEngine;
using UnityEngine.UI;

public class ThrowBarManager : MonoBehaviour
{
    private Slider ThrowSlider;
    private Image bkgImage;
    private Image fllImage;

    private void Awake()
    {
        ThrowSlider = this.GetComponent<Slider>();
        bkgImage = this.transform.Find("Background").GetComponent<Image>();
        fllImage = this.transform.Find("Fill").GetComponent<Image>();

        bkgImage.enabled = false;
        fllImage.enabled = false;
        ThrowSlider.value = 0f;
    }

    private void Update()
    {
        if (ThrowSlider.value != 0)
        {
            bkgImage.enabled = true;
            fllImage.enabled = true;
        }
        else
        {
            bkgImage.enabled = false;
            fllImage.enabled = false;
        }
    }

    public void SetValuePercentage(float percentage)
    {
        ThrowSlider.value = Mathf.Clamp(percentage,0,1);
    }
}
