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
        PermissionManager.isPromptOpen = true;
    }

    private void OnDestroy() 
    {
        PermissionManager.isPromptOpen = false;
    }
}
