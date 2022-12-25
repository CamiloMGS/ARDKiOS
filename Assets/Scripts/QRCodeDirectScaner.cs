using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using TMPro;
using UnityEngine.UI;
public class QRCodeDirectScaner : MonoBehaviour
{
    [SerializeField] private RawImage imgBkg;
    [SerializeField] private AspectRatioFitter ratioFitter;
    [SerializeField] private TextMeshProUGUI textResult;
    [SerializeField] private RectTransform scaneZone;

    private bool isCamAvaible;
    private WebCamTexture cameraTexture;
    Camera aRCamera;
    // Start is called before the first frame update
    void Start()
    {
        //SetUpCamera();
        GetTexture();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ReadQRCode()
    {
        StartCoroutine(GetTexture());
        Invoke("ReadQRCode", 0.5f);
    }

    IEnumerator GetTexture() 
    {
        yield return new WaitForEndOfFrame();
        var rTex = aRCamera.activeTexture;
        var t2D = ToTexture2D(rTex);
        Scan(t2D);
    }

    Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    private void Scan(Texture2D texture)
    {
        Debug.Log("Scaning");
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);
            if (result != null)
            {
                Debug.Log(result.Text);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    private void SetUpCamera() 
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            isCamAvaible = false;
            return;
        }

        for (int i = 0; i <devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                cameraTexture = new WebCamTexture(devices[i].name, (int)scaneZone.rect.width, (int)scaneZone.rect.height);
            }
        }

        cameraTexture.Play();
        imgBkg.texture = cameraTexture;
        isCamAvaible = true;
    }
}
