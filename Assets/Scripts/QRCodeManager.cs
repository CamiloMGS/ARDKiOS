using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Marker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Networking.Clock;
using ZXing;
using QRDetection.Location;
using UnityEngine.UI;
public class QRCodeManager : MonoBehaviour
{
    public enum CamTextureType 
    {
        Platfrom,
        YCbCr,
        BGRA
    }

    public event ArdkEventHandler<ARFrameMarkerScannerReadyArgs> Ready;
    public event ArdkEventHandler<ARFrameMarkerScannerStatusChangedArgs> StatusChanged;

    private IARSession aRSession;
    private CamTextureType textureType;

    private IARCamera aRCamera;
    private byte[] rawPixels;
    private int rawWidth;
    private int rawHeight;

    private bool parserPixelAvailable = false;
    private bool decodeInterrupted = true;

    private Thread codeScannerThread;
    private bool hasResult;
    private float scannerDecodeInterval = 0.2f;
    private BarcodeReader reader;

    private QRCodeDepth qRCodeDepth = new QRCodeDepth();

    [SerializeField] private Camera cameraAR;
    private bool shouldWait = true;

    event Action OnTargetFound;
    public MarkerScannerStatus Status 
    {
        get 
        {
            return status;
        }
        private set 
        {
            if (value == status)
            {
                return;
            }

            status = value;

            var handler = StatusChanged;
            if (handler != null)
            {
                var arg = new ARFrameMarkerScannerStatusChangedArgs(value);
                handler(arg);
            }
        }
    }

    private MarkerScannerStatus status;
    private float angleRot;
    private float depthAverageQRCode;
    private Vector3 posQRCode;
    private Quaternion rotQRCode;
    [SerializeField] private GameObject exanger;
    [SerializeField] private GameObject QRCodeRepresentation;
    void Start()
    {
        ARSessionFactory.SessionInitialized += ARSessionFactory_SessionInitialized;
        OnTargetFound += CalculatePosition;
    }
    private void Update()
    {
        CalculatePosition();
        QRCodeRepresentation.transform.SetPositionAndRotation(posQRCode, rotQRCode);
        
    }

    private void ARSessionFactory_SessionInitialized(AnyARSessionInitializedArgs args)
    {
        aRSession = args.Session;
        Status = MarkerScannerStatus.Initialized;
        reader = new BarcodeReader();
        InitializeFrameSettings();
        Scan();
    }

    public void Scan() 
    {
        if (Status == MarkerScannerStatus.Running)
        {
            Debug.Log("This ARFrameMarkerScanner is already running.");
            return;
        }

        if (codeScannerThread != null)
            //Stop();

        Debug.Log("Start Thread");
        decodeInterrupted = false;
        codeScannerThread = new Thread(ThreadTryToParse);
        codeScannerThread.Start();
        Status = MarkerScannerStatus.Running;
        aRSession.FrameUpdated += UpdateMarker;
    }

    private void Stop ()
    {
        Status = MarkerScannerStatus.Paused;
        aRSession.FrameUpdated -= UpdateMarker;
    }

    private void UpdateMarker(FrameUpdatedArgs args)
    {
        var readyHandler = Ready;
        if (readyHandler != null)
        {
            Ready = null;

            var readyArgs = new ARFrameMarkerScannerReadyArgs();
            readyHandler(readyArgs);
        }

        if (status == MarkerScannerStatus.Running)
        {
            if (hasResult)
            {
                parserPixelAvailable = false;
                Stop();
                return;
            }
            if (!parserPixelAvailable)
            {
                IARFrame frame = args.Frame;
                if (frame.CapturedImageBuffer == null)
                {

                    Debug.Log("I don't have data");
                    return;
                }

                aRCamera = frame.Camera;

                rawPixels = frame.CapturedImageBuffer.Planes[0].Data.ToArray();
                rawWidth = frame.Camera.CPUImageResolution.width;
                rawHeight = frame.Camera.CPUImageResolution.height;

                parserPixelAvailable = true;
                shouldWait = true;
            }

/*            bool shouldParse = mainThreadLastDecode < Time.realtimeSinceStartup - scannerDecodeInterval;

            if (shouldParse)
            {
                TryToParse();
                mainThreadLastDecode = Time.realtimeSinceStartup;
            }*/
        }
    }

    private void TryToParse()
    {
        if (Status != MarkerScannerStatus.Running || !parserPixelAvailable)
            return;

        try
        {

            ConvertTextureAndDecode();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }



    private void ThreadTryToParse()
    {
        while (decodeInterrupted == false && !hasResult)
        {
            if (Status != MarkerScannerStatus.Running || !parserPixelAvailable)
            {
                Thread.Sleep(Mathf.FloorToInt(scannerDecodeInterval * 1000));
                continue;
            }
            try
            {
                ConvertTextureAndDecode();

                if (!hasResult)
                {
                    continue;
                }

                Thread.Sleep(Mathf.FloorToInt(scannerDecodeInterval * 1000));
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
        var rawIndex = 0;

        for (var idx = 0; idx < rawWidth * rawHeight; idx++)
        {
            if (textureType == CamTextureType.YCbCr)
            {

                var val = rawPixels[idx];
                pixels[idx] = new Color32(val, val, val, 255);
            }
            else
            {
                pixels[idx] =
                  new Color32
                  (
                    rawPixels[rawIndex + 2],
                    rawPixels[rawIndex + 1],
                    rawPixels[rawIndex],
                    rawPixels[rawIndex + 3]
                  );

                rawIndex += 4;
                
            }
        }

        TryToDecodePixels(pixels);
    }
    private void TryToDecodePixels(Color32[] pixels)
    {
        shouldWait = false;
        var result = reader.Decode(pixels, rawWidth, rawHeight);
        if (result != null)
        {
            Debug.Log("-----------> " + result.Text);
            CalculatePosition(result.ResultPoints);
        }
        else
        {
            Debug.Log("No result");
        }

        parserPixelAvailable = false;
    }

    private void CalculatePosition(ResultPoint[] resultPoints) 
    {
        if (resultPoints.Length <4)
        {
            return;
        }

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

    }

    private void CalculatePosition() 
    {
        Vector3 centerPost = cameraAR.ScreenToWorldPoint(new Vector3(Screen.width / 2, (Screen.height / 2) + 70 *3, depthAverageQRCode));
        posQRCode = centerPost;
        Quaternion rotationGO = Quaternion.Euler(0, 0, -angleRot);

        exanger.transform.parent = cameraAR.transform;
        exanger.transform.localRotation = rotationGO;
        exanger.transform.position = centerPost;
        exanger.transform.parent = null;

        posQRCode = exanger.transform.position;
        rotQRCode = exanger.transform.rotation;
    }

    private void InitializeFrameSettings()
    {
#if AR_NATIVE_SUPPORT && UNITY_ANDROID
      textureType = TextureType.BGRA;
#else
        textureType = CamTextureType.YCbCr;
#endif
#if UNITY_EDITOR
        textureType = CamTextureType.BGRA;
#endif

        Debug.Log("Texture: - > " + textureType);
    }

    private void OnApplicationQuit()
    {
        codeScannerThread.Abort();
    }
}
