using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QRScanner : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// The scan line running over the screen.
    /// </summary>
    public Image scanLine;
    /// <summary>
    /// The box frame in which the QR code needs to be.
    /// </summary>
    public RawImage boxFrame;
    /// <summary>
    /// The checkmark to show on successfull scan.
    /// </summary>
    public RawImage checkmark;

    [Header("Animation settings")]
    /// <summary>
    /// The speed at which the scan line moves up and down.
    /// </summary>
    public float scanSpeed = 0.5f;
    /// <summary>
    /// How long to show the checkmark.
    /// </summary>
    public float checkmarkDisplayTime = 1.5f;
    /// <summary>
    /// The time it takes for the checkmark to fade out.
    /// </summary>
    public float fadeDuration = 0.5f;

    /// <summary>
    /// When enabled it resets all elements used by the scanner.
    /// </summary>
    void OnEnable() {
        boxFrame.gameObject.SetActive(true);
        scanLine.gameObject.SetActive(true);
        checkmark.gameObject.SetActive(false);
    }

    /// <summary>
    /// Moves scan line up and down.
    /// </summary>
    void Update()
    {
        RectTransform canvasRect = gameObject.GetComponent<RectTransform>();    
        float scanPosition = Mathf.PingPong(Time.time * scanSpeed, 1f);
        scanLine.transform.position = new Vector2(0, Mathf.Lerp(0, canvasRect.rect.height, scanPosition));
    }

    /// <summary>
    /// When scan succeeds show checkmark.
    /// </summary>
    public void OnScanSuccess() {
        StartCoroutine(ShowCheckmark());
    }

    /// <summary>
    /// Temporary shows checkmark.
    /// </summary>
    private IEnumerator ShowCheckmark() {
        boxFrame.gameObject.SetActive(false);
        scanLine.gameObject.SetActive(false);
        checkmark.gameObject.SetActive(true);

        yield return new WaitForSeconds(checkmarkDisplayTime);

        float time = 0;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            Color checkmarkColor = checkmark.color;
            checkmarkColor.a = Mathf.Lerp(1, 0, time / fadeDuration);
            checkmark.color = checkmarkColor;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
