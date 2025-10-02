using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Text;
using System.Collections.Generic;

public class HandTrackingClient : MonoBehaviour
{
    public CameraFrameReader cameraReader;
    public string serverURL = "http://192.168.178.149:5000/handpose";
    public RectTransform uiRoot;
    public GameObject jointUIPrefab;
    public GameObject handSpherePrefab;

    private List<RectTransform> uiJoints = new List<RectTransform>();
    private GameObject handSphere;

    void Start()
    {
        for (int i = 0; i < 21; i++)
        {
            GameObject dot = Instantiate(jointUIPrefab, uiRoot);
            var rect = dot.GetComponent<RectTransform>();
            uiJoints.Add(rect);
            dot.SetActive(false);
        }

        if (handSpherePrefab != null)
        {
            handSphere = Instantiate(handSpherePrefab);
            handSphere.SetActive(false);
        }
    }

    void Update()
    {
        if (cameraReader.cameraTexture != null)
        {
            StartCoroutine(SendCameraFrame(cameraReader.cameraTexture));
        }
    }

    IEnumerator SendCameraFrame(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToJPG(25);
        string base64Img = Convert.ToBase64String(bytes);
        string json = "{\"image\":\"" + base64Img + "\"}";

        using (UnityWebRequest www = new UnityWebRequest(serverURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            www.disposeCertificateHandlerOnDispose = true;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("WebRequest error: " + www.error);
            }
            else
            {
                string response = www.downloadHandler.text;
                ProcessLandmarks(response);
            }
        }
    }

    void ProcessLandmarks(string json)
    {
        if (string.IsNullOrEmpty(json) || json.Contains("error")) 
        { 
            return;
        }

        json = json.Trim();

        json = json.TrimStart('[').TrimEnd(']');

        string[] landmarkStrings = json.Split(new string[] { "],[" }, StringSplitOptions.None);

        List<float[]> landmarks = new List<float[]>();
        foreach (string lmStr in landmarkStrings)
        {
            string clean = lmStr.Replace("[", "").Replace("]", "");
            string[] parts = clean.Split(',');
            float[] coords = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                coords[i] = float.Parse(parts[i], System.Globalization.CultureInfo.InvariantCulture);
            landmarks.Add(coords);
        }

        for (int i = 0; i < uiJoints.Count; i++)
        {
            uiJoints[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < landmarks.Count && i < uiJoints.Count; i++)
        {
            float posX = (landmarks[i][0] * uiRoot.rect.width) - (uiRoot.rect.width * 0.5f);
            float posY = (landmarks[i][1] * uiRoot.rect.height) - (uiRoot.rect.height * 0.5f);

            uiJoints[i].anchoredPosition = new Vector2(posY, posX);
            uiJoints[i].gameObject.SetActive(true);
        }

        if (handSphere != null && landmarks.Count > 0)
        {
            Vector3 palm = new Vector3(landmarks[5][0], landmarks[5][1], landmarks[5][2]);

            Vector3 screenPoint = new Vector3(
                palm[1] * Screen.width,
                palm[0] * Screen.height,
                1.0f + palm[2] * 15.0f
            );

            handSphere.transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
            handSphere.SetActive(true);
        }
    }
}
