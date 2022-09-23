using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Survivor.Core;

public class PermissionWindowUI : MonoBehaviour
{
    public TextMeshProUGUI permissionText;
    public Button onGrantedButton;
    public Button onDeniedButton;

    private void Awake() 
    {
        UIPromptManager.IsPermissionPromptOpen = true;
    }

    private void OnDestroy() 
    {
        UIPromptManager.IsPermissionPromptOpen = false;
    }
}
