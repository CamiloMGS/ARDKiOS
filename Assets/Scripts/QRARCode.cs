using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class QRARCode : MonoBehaviour
{
    WebCamTexture webcamTexture;
    string QrCode = string.Empty;
    //public AudioSource beepSound;

    void Start()
    {
        //var renderer = GetComponent<RawImage>();
        WebCamDevice[] devices = WebCamTexture.devices;
        Debug.Log(devices.Length);
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                Debug.Log(devices[i].name);
                webcamTexture = new WebCamTexture(devices[i].name,512, 512);
            }
        }

        //renderer.material.mainTexture = webcamTexture;
        webcamTexture.Play();
        StartCoroutine(GetQRCode());
    }

    IEnumerator GetQRCode()
    {
        IBarcodeReader barCodeReader = new BarcodeReader();

        var snap = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);
        while (string.IsNullOrEmpty(QrCode))
        {
            try
            {
                snap.SetPixels32(webcamTexture.GetPixels32());
                var Result = barCodeReader.Decode(snap.GetRawTextureData(), webcamTexture.width, webcamTexture.height, RGBLuminanceSource.BitmapFormat.ARGB32);
                if (Result != null)
                {
                    QrCode = Result.Text;
                    if (!string.IsNullOrEmpty(QrCode))
                    {
                        Debug.Log("DECODED TEXT FROM QR: " + QrCode);
                        break;
                    }
                }
            }
            catch (Exception ex)
            { 
                Debug.LogWarning(ex.Message); 
            }
            yield return null;
        }
        webcamTexture.Stop();
    }
}
