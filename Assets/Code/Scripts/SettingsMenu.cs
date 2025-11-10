using System.Net;
using System.Linq;
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
    /// The name of the movable point to reset to.
    /// </summary>
    public string moveablePointName = "InsideElevator";
    /// <summary>
    /// The movable point to reset to.
    /// </summary>
    private MovablePoint movablePoint;

    /// <summary>
    /// Toggles the menu.
    /// </summary>
    public void ToggleMenu() 
    {
        Menu.SetActive(!Menu.activeSelf);
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
        if (movablePoint == null) 
        {
            movablePoint = FindObjectsOfType<MovablePoint>().FirstOrDefault(obj => obj.name == moveablePointName);
        }

        if (movablePoint != null)
        {
            Vector3 offset = movablePoint.transform.position - Camera.main.transform.position;
            offset.y = 0;

            Vector3 targetPosition = xROrigin.transform.position + offset;
            xROrigin.transform.position = targetPosition;
        }
    }
}
