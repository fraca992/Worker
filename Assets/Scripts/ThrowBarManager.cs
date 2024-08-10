using UnityEngine;
using UnityEngine.UI;

public class ThrowBarManager : MonoBehaviour
{
    private Slider ThrowSlider;

    private void Awake()
    {
        ThrowSlider = this.GetComponent<Slider>();
    }

    public void SetFuelLvl(int fuel)
    {
        ThrowSlider.value = fuel;
    }
    public void SetBarMax(int maxLvl)
    {
        ThrowSlider.maxValue = maxLvl;
        ThrowSlider.value = maxLvl;
    }
}
