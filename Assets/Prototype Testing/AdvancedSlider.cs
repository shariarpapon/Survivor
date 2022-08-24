using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdvancedSlider : MonoBehaviour
{
    public Slider slider;
    public TMP_InputField input;
    public UserPreferenceHandler handler;
    
    private void Awake() 
    {
        input.onEndEdit.AddListener( delegate { UpdateSliderValue();} );
        slider.onValueChanged.AddListener( delegate { UpdateInputText(); } );
    }

    private void UpdateSliderValue() 
    {
        if (float.TryParse(input.text, out float f))
        {
            f = Mathf.Clamp01(f);
            input.text = f.ToString();
            slider.value = f;
        }
        handler.WriteToUserPreference();
    }

    private void UpdateInputText() 
    {
        input.text = slider.value.ToString();
        handler.WriteToUserPreference();
    }
}
