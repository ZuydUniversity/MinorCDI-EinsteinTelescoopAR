using System.Net;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    /// <summary>
    /// SettingsMenu to be opened or closed
    /// </summary>
    public GameObject Menu;
    /// <summary>
    /// XROrgigin to be moved
    /// </summary>
    public XROrigin xROrigin;
    /// <summary>
    /// Opens SettingMenu
    /// </summary>
    public void OpenMenu()
    {
        Menu.SetActive(true);
    }
    /// <summary>
    /// Closes SettingMenu
    /// </summary>
    public void CloseMenu()
    {
        Menu.SetActive(false);
    }
    /// <summary>
    /// Restarts game if needed. 
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene("MainScene");
    }
    /// <summary>
    /// Returns player to lift (use when player gets lost)
    /// </summary>
    public void ReturnToLift()
    {
        Vector3 offset = Vector3.zero - Camera.main.transform.position;
        offset.y = 0;
        Vector3 targetPosition = xROrigin.transform.position + offset;
        xROrigin.transform.position = targetPosition;
    }
}
