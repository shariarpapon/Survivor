using UnityEngine;

namespace Survivor.Core 
{
    public class UIPromptManager : MonoBehaviour
    {
        public static UIPromptManager Instance { get; private set; }
        public static bool IsPermissionPromptOpen = false;

        [SerializeField] private GameObject permissionWindowUI;

        private void Awake() 
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else 
            {
                Destroy(gameObject);
                return;
            }
        }

        public void PermissionPrompt(string msg, System.Action onGranted, System.Action onDenied) 
        {
            AudioManager.Instance.PlayNotificationAlert();
            PermissionWindowUI permissionWindow = Instantiate(permissionWindowUI, transform).GetComponent<PermissionWindowUI>();

            permissionWindow.permissionText.text = msg;

            permissionWindow.onGrantedButton.onClick.AddListener(
                delegate { onGranted?.Invoke(); Destroy(permissionWindow.gameObject); });

            permissionWindow.onDeniedButton.onClick.AddListener(
                delegate { onDenied?.Invoke(); Destroy(permissionWindow.gameObject); });
        }

        public void PermissionPrompt(string msg, System.Action onGranted) 
        {
            AudioManager.Instance.PlayNotificationAlert();
            PermissionWindowUI permissionWindow = Instantiate(permissionWindowUI, transform).GetComponent<PermissionWindowUI>();

            permissionWindow.permissionText.text = msg;

            permissionWindow.onGrantedButton.onClick.AddListener(
                delegate { onGranted?.Invoke(); Destroy(permissionWindow.gameObject); });

            permissionWindow.onDeniedButton.onClick.AddListener(
                delegate { Destroy(permissionWindow.gameObject); });
        }
    }
}
