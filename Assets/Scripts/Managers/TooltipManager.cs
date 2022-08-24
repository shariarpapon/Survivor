using System.Collections.Generic;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] private GameObject textBox;
    [SerializeField] private Transform tooltipWindow;

    public static readonly HashSet<string> headers = new HashSet<string>();
    public static readonly HashSet<string> contents = new HashSet<string>();

    private void Awake() 
    {
        if (Instance == null) Instance = this;
        else return;
    }

    public void TimedPopup(string header, string content, float lifetime, TextOptions textOptions = null)
    {
        Tooltip tooltip = Popup(header, content, textOptions);
        if (tooltip) Destroy(tooltip.gameObject, lifetime);
    }

    public void TimedPopup(string header, string content, float lifetime, Vector3 position, TextOptions textOptions = null)
    {
        Tooltip tooltip = Popup(header, content, position, textOptions);
        if (tooltip) Destroy(tooltip.gameObject, lifetime);
    }

    public Tooltip Popup(string header, string content, TextOptions textOptions = null) 
    {
        if (AlreadyExists(header, content)) return null;

        Tooltip tooltip = Instantiate(textBox, tooltipWindow).GetComponent<Tooltip>();
        tooltip.Create(header, content, textOptions);
        return tooltip;
    }

    public Tooltip Popup(string header, string content, Vector3 position, TextOptions textOptions = null)
    {
        if (AlreadyExists(header, content)) return null;

        Tooltip tooltip = Instantiate(textBox,position, Quaternion.identity, tooltipWindow.root ).GetComponent<Tooltip>();
        tooltip.Create(header, content, textOptions);
        return tooltip;
    }

    private bool AlreadyExists(string header, string content) 
    {
        return headers.Contains(header) && contents.Contains(content);
    }
}
