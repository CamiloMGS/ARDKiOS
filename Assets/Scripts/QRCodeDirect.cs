using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
public class QRCodeDirect : MonoBehaviour
{
    [SerializeField] private RawImage img;
    // Start is called before the first frame update
    void Start()
    {
        ReadQRCode();
    }
    void Update()
    {

    }

    private void ReadQRCode() 
    {
        StartCoroutine(ScrenShot());
        Invoke("ReadQRCode", 0.5f) ;
    }

    IEnumerator ScrenShot() 
    {

        yield return new WaitForEndOfFrame();
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        Scan(ss);
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

}
