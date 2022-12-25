using Niantic.ARDK.AR;
using Niantic.ARDK.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.ARDK.AR.ARSessionEventArgs;
using System;
using ZXing;
using System.Threading;
using TMPro;
using UnityEngine.UI;
using QRDetection.Location;
using TMPro;
public class QRCodeAnalizer : MonoBehaviour
{

    [SerializeField] private Camera aRCamera;

    private byte[] rawPixels;
    private int rawWidth;
    private int rawHeight;
    private TextureType textureType;
    private BarcodeReader reader;
    private bool shouldConvert = true;
    private Thread codeScannerThread;

    private float angleRot;
    private float depthAverageQRCode;
    private QRCodeDepth qRCodeDepth = new QRCodeDepth();
    private GameObject tempRootARContent;

    private Vector3 posQRCode;
    private Quaternion rotQRCode;

    [SerializeField] GameObject QRCodeRepresentation;
    [SerializeField] GameObject exanger;
    [SerializeField] private AnchorManager anchorManager;
    [SerializeField] private TextMeshProUGUI textPosition;
    [SerializeField] private TextMeshProUGUI textAnchor;

    private float movement;
    public IARSession Session;
    // Start is called before the first frame update
    void Start()
    {
        ARSessionFactory.SessionInitialized += ARSessionFactory_SessionInitialized;

        StartCoroutine(StartScan());
        reader = new BarcodeReader();
    }

    private void Update()
    {
        QRCodeRepresentation.transform.SetPositionAndRotation(posQRCode, rotQRCode);
        movement =  Input.acceleration.magnitude;
    }
    private void ARSessionFactory_SessionInitialized(AnyARSessionInitializedArgs args)
    {
        InitializeFrameSettings();
        Session = args.Session;
        args.Session.FrameUpdated += ARSession_FrameUpdated;
    }

    private void ARSession_FrameUpdated(FrameUpdatedArgs args)
    {
        IARFrame frame = args.Frame;

        if (frame.CapturedImageBuffer == null)
        {

            Debug.Log("I don't have data");
            return;
        }

        rawPixels = frame.CapturedImageBuffer.Planes[0].Data.ToArray();
        rawWidth = frame.Camera.CPUImageResolution.width;
        rawHeight = frame.Camera.CPUImageResolution.height;
    }
    private void InitializeFrameSettings()
    {
#if AR_NATIVE_SUPPORT && UNITY_ANDROID
      textureType = TextureType.BGRA;
#else
        textureType = TextureType.YCbCr;
#endif
    }
    private IEnumerator StartScan() 
    {
        Debug.Log("Wait ForScan");
        yield return new WaitForSeconds(5);
        codeScannerThread = new Thread(ThreadTryToParse);
        codeScannerThread.Start();
    }


    private void ThreadTryToParse()
    {
        while (true)
        {

            try
            {
                if (shouldConvert)
                {
                    shouldConvert = false;
                    ConvertTextureAndDecode();
                }

                Thread.Sleep(800);
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    private void ConvertTextureAndDecode()
    {
        var pixels = new Color32[rawWidth * rawHeight];

        for (var idx = 0; idx < rawWidth * rawHeight; idx++)
        {
            if (textureType == TextureType.YCbCr)
            {

                var val = rawPixels[idx];
                pixels[idx] = new Color32(val, val, val, 255);
            }
        }


        TryToDecodePixels(pixels);
    }
    private void TryToDecodePixels(Color32[] pixels)
    {
        var result = reader.Decode(pixels, rawWidth, rawHeight);

        if (result == null)
        {
            Debug.Log("Re Try");
            shouldConvert = true;
        }
        if (result != null)
        {
            ResultPoint[] resultPoints = result.ResultPoints;

            ResultPoint TRP = resultPoints[0]; // Top Right            //P1
            ResultPoint TLP = resultPoints[1]; // TopLeft              //P0
            ResultPoint BLP = resultPoints[2]; // Button Left //       //P2 
            ResultPoint APP = resultPoints[3]; // Alignment Pattern    //P3

            Point topRight = new Point(TRP.X, TRP.Y);
            Point topLeft = new Point(TLP.X, TLP.Y);
            Point bottomLeft = new Point(BLP.X, BLP.Y);
            Point alignmentPattern = new Point(APP.X, APP.Y);

            QRLocation ARCodeLoation = new QRLocation(topLeft, topRight, alignmentPattern, bottomLeft);
            ARCodeLoation.Initialize();

            angleRot = ARCodeLoation.Angle;
            qRCodeDepth.GenerateArtificialDepthEstimate("20CeriumAR");
            depthAverageQRCode = qRCodeDepth.DepthEstimation;

            Vector3 centerPost = aRCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, depthAverageQRCode));
            posQRCode = centerPost;
            Quaternion rotationGO = Quaternion.Euler(0, 0, angleRot);

            exanger.transform.parent = aRCamera.transform;
            exanger.transform.localRotation = rotationGO;
            exanger.transform.position = centerPost;
            exanger.transform.parent = null;

            posQRCode = exanger.transform.position;
            rotQRCode = exanger.transform.rotation;
            //anchorManager.AddAnchorInSpace(posQRCode, rotQRCode);
            shouldConvert = true;

            textPosition.text = "Cam Pos: " + aRCamera.transform.position;
            textAnchor.text = "Anchor Pos: " + posQRCode;
        }
    }



    private void OnApplicationQuit()
    {
        codeScannerThread.Abort();
    }

}

public enum TextureType
{
    /// Texture format is determined by platform (YCbCr for iOS, BGRA for Android or in-editor).
    Platform,

    /// YCbCr format. This value is only valid on iOS devices.
    YCbCr,

    /// BGRA format. This value is valid on Android devices and in-editor.
    BGRA
}
