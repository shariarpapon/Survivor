using UnityEngine;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI headerText;
    [SerializeField] private TMPro.TextMeshProUGUI contentText;
    private string _header;
    private string _content;

    public void Create(string header, string content, TextOptions textOptions) 
    {
        _header = header;
        _content = content;
        SetText(textOptions);
    }

    private void SetText(TextOptions textOptions)
    {
        if (textOptions != null)
        {
            headerText.color = textOptions.headerColor;
            contentText.color = textOptions.contentColor;
        }

        TooltipManager.headers.Add(_header);
        TooltipManager.contents.Add(_content);

        if (!string.IsNullOrEmpty(_header)) headerText.text = _header;
        if (!string.IsNullOrEmpty(_content)) contentText.text = _content;

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        StartCoroutine(GameUtility.TweenScaleBell(gameObject, 1, 1.15f, 20));
    }

    private void OnDestroy() 
    {
        TooltipManager.headers.Remove(_header);
        TooltipManager.contents.Remove(_content);
    }
}

public class TextOptions
{
    public readonly Color headerColor;
    public readonly Color contentColor;
    public TextOptions(Color headerColor, Color contentColor)
    {
        this.headerColor = headerColor;
        this.contentColor = contentColor;
    }

    public static TextOptions Regular { get { return new TextOptions(Color.white, Color.grey); } }
    public static TextOptions YellowContent { get { return new TextOptions(Color.white, Color.yellow); } }
    public static TextOptions RedContent { get { return new TextOptions(Color.white, Color.red); } }
}
