using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Marker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ZXing;

namespace Core.QrCodeScaner
{
    public sealed class QRCodeScanerAsynHandler : IQRCodeScanerHandler
    {
        public enum TextureType
        {
            Platform,
            YCbCr,
            BGRA
        }

        IARSession aRSession;
        private QRCodeScannerSettings settings;
        public IQRCodeParser IQRCodeParser { get; private set; }
        public QRCodeScannerStatus status;
        private IQRCodeResult result;
        private TextureType textureType;
        private bool decodeInterrupted;
        private Thread codeScannerThread;
        private bool parserPixelAvailable;
        private IARCamera arCamera;
        public byte[] rawPixels;
        public byte[] rawPixelsY;
        public int rawWidth;
        public int rawHeight;
        public Color32[] imgColor;

        public QRCodeScannerStatus Status
        {
            get
            {
               
                return status;
            }
            private set
            {
                if (value == status)
                    return;

                status = value;
                Debug.Log(Status);
                var handler = StatusChanged;
                if (handler != null)
                {
                    var args = new ARQRCodeStatusArgs(value);
                    handler(args);
                }
            }
        }



        public QRCodeScanerAsynHandler(IARSession _aRSession,QRCodeScannerSettings _settings = null, IQRCodeParser _qRCodeParser = null)
        {
            aRSession = _aRSession;
            InitializeFrameSettings();

            settings = _settings ?? new QRCodeScannerSettings();
            IQRCodeParser = _qRCodeParser ?? new ZXingQRCodDecoder(settings);

            Status = QRCodeScannerStatus.Initialized;
        }

        public event ArdkEventHandler<ARFrameMarkerScannerReadyArgs> Ready;
        public event ArdkEventHandler<ARQRCodeStatusArgs> StatusChanged;
        public event ArdkEventHandler<ARQRCodeScannerGotResultArgs> GotResult;

        public void Scan()
        {
            if (Status == QRCodeScannerStatus.Running)
            {
                Debug.Log("This ARFrameMarkerScanner is already running.");
                return;
            }
            if (settings.ScannerBackgroundThread)
            {

                Debug.Log("Start Thread");
                decodeInterrupted = false;
                codeScannerThread = new Thread(ThreadTryToParse);
                codeScannerThread.Start();
            }


            Debug.Log("ARFrameMarkerScanner started.");
            Status = QRCodeScannerStatus.Running;
            aRSession.FrameUpdated += Update;
        }

        private void ThreadTryToParse()
        {

            while (decodeInterrupted == false  && result == null)
            {
                 
                if (Status != QRCodeScannerStatus.Running || !parserPixelAvailable)
                {
                    Thread.Sleep(Mathf.FloorToInt(settings.ScannerDecodeInterval * 1000));
                    continue;
                }
                try
                {
                    ConvertTextureAndDecode();

                    if (result == null)
                        continue;

                    Thread.Sleep(Mathf.FloorToInt(settings.ScannerDecodeInterval * 1000));
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
           // Debug.Log("Texture Size: -> " + rawWidth + " " + rawHeight);
            var rawIndex = 0;
            for (var idx = 0; idx < rawWidth * rawHeight; idx++)
            {
                if (textureType == TextureType.YCbCr)
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
            imgColor = pixels;
            TryToDecodePixels(pixels);
        }

        private void TryToDecodePixels(Color32[] pixels)
        {
            IQRCodeResult qRCodeResult;
            var readerSuccess = IQRCodeParser.Decode(pixels, rawWidth, rawHeight, out qRCodeResult);

            if (readerSuccess)
            {

                qRCodeResult.Width = rawWidth;
                qRCodeResult.Height = rawHeight;
                result = qRCodeResult;

            }
            else
            {
                Debug.Log("No QR Code");
            }

            parserPixelAvailable = false;
        }

        public void Stop()
        {
            Debug.Log("ARFrameMarkerScanner stopped.");

            if (codeScannerThread != null)
            {
                decodeInterrupted = true;
                codeScannerThread.Join();
                codeScannerThread = null;

            }

            Status = QRCodeScannerStatus.Paused;
            aRSession.FrameUpdated -= Update;
        }

        public void Update()
        {
            //var args = new FrameUpdatedArgs(aRSession.CurrentFrame);
            //Update(args);
        }

        private void Update(FrameUpdatedArgs updateArgs) 
        {
            var readyHandler = Ready;
            if (readyHandler !=  null)
            {
                Ready = null;

                var readyArgs = new ARFrameMarkerScannerReadyArgs();
                readyHandler(readyArgs);
            }

            if (Status == QRCodeScannerStatus.Running)
            {
                if (result != null)
                {
                    var gotResultHandler = GotResult;
                    if (gotResultHandler != null)
                    {
                        var args = new ARQRCodeScannerGotResultArgs(result);
                        gotResultHandler(args);
                    }

                    result = null;
                    parserPixelAvailable = false;
                    return;
                }

                if (!parserPixelAvailable)
                {
                    IARFrame frame = updateArgs.Frame;

                    if (frame.CapturedImageBuffer == null)
                    {
                        Debug.Log("I don't have data");
                        return;
                    }


                    arCamera = frame.Camera;

                    Debug.Log(" Format: " + frame.CapturedImageBuffer.Format + "Planes: " + frame.CapturedImageBuffer.Planes.Count);
                    rawPixels = frame.CapturedImageBuffer.Planes[0].Data.ToArray();

                    //rawPixelsY = frame.CapturedImageBuffer.Planes[1].Data.ToArray();
                    //Debug.Log( "RawNormal: " + rawPixels.Length + " RawY: " + rawPixelsY.Length);
                    rawWidth = frame.Camera.CPUImageResolution.width;
                    rawHeight = frame.Camera.CPUImageResolution.height;

                    //Debug.Log("Width: " + rawWidth + " " + "Height: " + rawHeight + "Content: " + rawPixels.Length);

                    parserPixelAvailable = true;
                }
            }
        }


        private void InitializeFrameSettings()
        {
#if AR_NATIVE_SUPPORT && UNITY_ANDROID
                  textureType = TextureType.BGRA;
#else
            textureType = TextureType.YCbCr;
#endif

#if UNITY_EDITOR
            textureType = TextureType.BGRA;
#endif
            Debug.Log("Texture: - > " + textureType);

            
        }

        
    }

    public class ZXingQRCodDecoder : IQRCodeParser
    {
        private readonly BarcodeReader reader;

        public ZXingQRCodDecoder() : this(new QRCodeScannerSettings())
        {

        }

        public ZXingQRCodDecoder(QRCodeScannerSettings settings)
        {
            reader = new BarcodeReader();
            reader.AutoRotate = settings.ParserAutoRotate;
            reader.TryInverted = settings.ParserTryInverted;
            reader.Options.TryHarder = settings.ParserTryHarder;
        }
        public bool Decode(Color32[] pixels, int width, int height, out IQRCodeResult qrCodeResult)
        {
            qrCodeResult = new QRCodeResult();
            if (pixels == null || pixels.Length == 0 || width == 0 || height == 0)
                return false;

            bool success = false;
            try
            {
                Result result = reader.Decode(pixels, width, height);

                if (result != null && result.ResultPoints.Length ==4)
                {
                    qrCodeResult.Content = result.Text;
                    Vector2[] vectorPoints = new Vector2[4];
                    qrCodeResult.DetectedPoints = vectorPoints;

                    Vector2[] resultPoints = CleanPoints(result.ResultPoints, width, height);

                    for (var i = 0; i < vectorPoints.Length; ++i)
                    {
                        var point = result.ResultPoints[i];
                        vectorPoints[i] = new Vector2(point.X, point.Y);
                    }

                    Vector2 qRCodeCenter = GetCenter(resultPoints[2], resultPoints[0], resultPoints[1], resultPoints[3]);
                    qrCodeResult.Center = qRCodeCenter;
                    qrCodeResult.Angle = FindAligment(qRCodeCenter, resultPoints[3]);
                    success = true;

                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return success;
        }

        private Vector2[] CleanPoints(ResultPoint[] resultPoints, int width, int height) 
        {

            Vector2 sizeImg = new Vector2(width, height);
            Vector2 pTR = new Vector2(resultPoints[0].X, sizeImg.y - resultPoints[0].Y);
            Vector2 pTL = new Vector2(resultPoints[1].X, sizeImg.y - resultPoints[1].Y);
            Vector2 pBL = new Vector2(resultPoints[2].X, sizeImg.y - resultPoints[2].Y);
            Vector2 pAP = new Vector2(resultPoints[3].X, sizeImg.y - resultPoints[3].Y);
            Vector2[] newResultPoint = { pTR, pTL, pBL, pAP };
            return newResultPoint;
        }
        private Vector2 GetCenter(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            var dx1 = p1.x - p0.x;
            var m1 = (p1.y - p0.y) / dx1;
            var b1 = p0.y - (m1 * p0.x);


            var dx2 = p3.x - p2.x;
            var m2 = (p3.y - p2.y) / dx2;
            var b2 = p2.y - (m2 * p2.x);

            var x = (b1 - b2) / (m2 - m1);

            var y = m2 * ((b1 - b2) / (m2 - m1)) + b2;

            var QRCenter = new Vector2(x, y);

            return QRCenter;
        }

        private double FindAligment(Vector2 qRCodeCenter, Vector2 pAP)
        {
  
            if (pAP.x > qRCodeCenter.x && pAP.y < qRCodeCenter.y)
            {

                return GetAngle(qRCodeCenter, pAP, 90, -1);

            }
            else if (pAP.x > qRCodeCenter.x && pAP.y > qRCodeCenter.y)
            {

                return GetAngle(qRCodeCenter, pAP, 90, -1);

            }
            else if (pAP.x < qRCodeCenter.x && pAP.y > qRCodeCenter.y)
            {

                return GetAngle(qRCodeCenter, pAP, 270);

            }
            else if (pAP.x < qRCodeCenter.x && pAP.y < qRCodeCenter.y)
            {

                return GetAngle(qRCodeCenter, pAP, 270);
            }
            else
            {
                Debug.Log("Ninguno");
                return 0;
            }
        }


        private double GetAngle(Vector2 qRCenter, Vector2 pAP, float angle = 0.0f, int factor = 1)
        {
            var radius = Vector2.Distance(qRCenter, pAP);
            var yComp = qRCenter.y - pAP.y;
            var absoluteAngle = Math.Asin(yComp / radius) * (180 / Math.PI);
            var localAngle = angle + factor * (float)absoluteAngle;
            return 45 - localAngle;
        }
    }
    public class QRCodeResult : IQRCodeResult
    {
        public string Content { get; set; }
        public Vector2[] DetectedPoints { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Angle { get; set; }
        public Vector2 Center { get; set; }
    }

    public class QRCodeScannerSettings
    {
        public bool ScannerBackgroundThread { get; set; }
        public int ScannerDelayFrameMin { get; set; }

        // This is in seconds
        public float ScannerDecodeInterval { get; set; }

        // Parser Options
        public bool ParserAutoRotate { get; set; }
        public bool ParserTryInverted { get; set; }
        public bool ParserTryHarder { get; set; }

        public QRCodeScannerSettings()
        {
            ScannerBackgroundThread = true;
            ScannerDelayFrameMin = 3;
            ScannerDecodeInterval = 0.1f;

            ParserAutoRotate = true;
            ParserTryInverted = true;
            ParserTryHarder = false;


#if UNITY_STANDALONE || UNITY_EDITOR
            ParserTryHarder = true;
#endif
        }
    }


    public interface IQRCodeScanerHandler
    {
        event ArdkEventHandler<ARFrameMarkerScannerReadyArgs> Ready;
        event ArdkEventHandler<ARQRCodeStatusArgs> StatusChanged;

        QRCodeScannerStatus Status { get; }

        IQRCodeParser IQRCodeParser { get; }

        void Scan();
        void Stop();
        void Update();
    }

    public interface IQRCodeParser
    {
        bool Decode(Color32[] pixels, int width, int height, out IQRCodeResult qrCodeResult);
    }

    public interface IQRCodeResult
    {
        string Content { get; set; }
        Vector2[] DetectedPoints { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double Angle { get; set; }
        Vector2 Center { get; set; }
    }

    public struct ARQRCodeScannerGotResultArgs : IArdkEventArgs 
    {
        public readonly IQRCodeResult QRCodeResult;

        public ARQRCodeScannerGotResultArgs(IQRCodeResult qRCodeResult) 
        {
            QRCodeResult = qRCodeResult;
        }
    }

    public struct ARQRCodeStatusArgs : IArdkEventArgs 
    {
        public readonly QRCodeScannerStatus QRCodeScannerStatus;

        public ARQRCodeStatusArgs(QRCodeScannerStatus qrCodeScannerStatus) 
        {
            QRCodeScannerStatus = qrCodeScannerStatus;
        } 
    }

    public enum QRCodeScannerStatus
    {
        Initialized,
        Running,
        Paused
    }

}






