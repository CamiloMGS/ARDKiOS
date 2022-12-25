using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.QrCodeScaner;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using ZXing;
using QRDetection.Location;
using Niantic.ARDK.AR.Anchors;
using System;
using Niantic.ARDKExamples.Common.Helpers;
using TMPro;
using Niantic.ARDK.Utilities;
using UnityEngine.UI;

public class QRCodeAR : MonoBehaviour
{
    private QRCodeScanerAsynHandler qRCodeScanerAsynHandler;
    [SerializeField] private Camera cameraAR;
    [SerializeField] private GameObject exanger;
    [SerializeField] private GameObject QRCode;
    [SerializeField] private Vector2 Center;

    private QRCodeDepth qRCodeDepth = new QRCodeDepth();
    [SerializeField] private float angleRot;
    [SerializeField] private float angleQRCode;

    private float depthAverageQRCode;
    private Vector3 posQRCode;
    private Quaternion rotQRCode;

    [SerializeField] private float minDistance = 100;
    int numScaners =3;
    [SerializeField] private GameObject aRPrefab;

    private IARSession Session = null;

    public List<IQRCodeResult> VerifiedScannings = new List<IQRCodeResult>();
    private Dictionary<Guid, IARAnchor> addedAnchors = new Dictionary<Guid, IARAnchor>();

    [SerializeField] private TextMeshProUGUI textAnchor;
    [SerializeField] private TextMeshProUGUI textUpdate;
    [SerializeField] private TextMeshProUGUI textInitial;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject anchorOneGO;
    [SerializeField] private GameObject anchorTwoGO;
    private IARAnchor anchorOne;
    private IARAnchor anchorTwo;

    private float disAnchorOne;
    private float disAnchorTwo;


    [SerializeField] private RawImage image;
    [SerializeField] private Texture2D texture;
    void Start()
    {
        ARSessionFactory.SessionInitialized += ARSessionFactory_SessionInitialized;
    }

    private void ARSessionFactory_SessionInitialized(AnyARSessionInitializedArgs args)
    {
        qRCodeScanerAsynHandler = new QRCodeScanerAsynHandler(args.Session);
        qRCodeScanerAsynHandler.Scan();
        qRCodeScanerAsynHandler.GotResult += QRCodeScanerAsynHandlerGotResult;
        args.Session.AnchorsAdded += SessionAnchorsAdded;
        args.Session.AnchorsUpdated += SessionAnchorsUpdated;
        Session = args.Session;

        //Debug.Log("Camera Propieties: " + camera.CPUIntrinsics.FocalLength + "  --  " + camera.CPUIntrinsics.PrincipalPoint);
        //Invoke("SetTextureImg", 2.0f);
    }


    private void SetTextureImg() 
    {
        texture = new Texture2D(qRCodeScanerAsynHandler.rawWidth, qRCodeScanerAsynHandler.rawHeight);
        texture.SetPixels32(qRCodeScanerAsynHandler.imgColor);
        texture.Apply();
        //image.uvRect = new Rect(300, -450, qRCodeScanerAsynHandler.rawWidth * 0.5f, qRCodeScanerAsynHandler.rawWidth * 0.5f);
        image.texture = texture;

        Invoke("SetTextureImg", 0.5f);
    }
    private void Update()
    {


        if (anchorOne == null  || anchorTwo == null)
        {
            return;
        }

        anchorOneGO.transform.position = anchorOne.Transform.ToPosition();
        anchorOneGO.transform.rotation = anchorOne.Transform.ToRotation();

        anchorTwoGO.transform.position = anchorTwo.Transform.ToPosition();
        anchorTwoGO.transform.rotation = anchorTwo.Transform.ToRotation();

        disAnchorOne = Vector3.Distance(cameraAR.transform.position, anchorOne.Transform.ToPosition());
        disAnchorTwo = Vector3.Distance(cameraAR.transform.position, anchorTwo.Transform.ToPosition());
        string textTemp = string.Empty;
        if (disAnchorOne <= disAnchorTwo)
        {
            aRPrefab.transform.parent = anchorOneGO.transform.GetChild(0).transform;
            aRPrefab.transform.localPosition = new Vector3(0, 0, 0);
            aRPrefab.transform.localRotation = Quaternion.identity;
            textTemp = "I'm in One";
        }
        else
        {
            aRPrefab.transform.parent = anchorTwoGO.transform.GetChild(0).transform;
            aRPrefab.transform.localPosition = new Vector3(-0.485f, 0, 0);
            aRPrefab.transform.localRotation = Quaternion.identity;
            textTemp = "I'm in Two";
        }

        textUpdate.text = string.Format("Distance One: {0} ---\nDistance Two: {1} --- \nState: {2}", disAnchorOne, disAnchorTwo, textTemp);


    }

    private void SessionAnchorsUpdated(AnchorsArgs args)
    {
        Debug.Log("Update Anchor-> " + args.Anchors.Count);
        foreach (var anchor in args.Anchors)
        {
            textAnchor.text ="In Update: " +anchor.Transform.ToPosition().ToString();
        }
    }

    private void SessionAnchorsAdded(AnchorsArgs args)
    {
        foreach (IARAnchor anchor in args.Anchors)
        {
            if (!addedAnchors.ContainsKey(anchor.Identifier))
            {
                Debug.LogWarningFormat
                (
                  "Found anchor (id: {0}) not added by this class. This should not happen.",
                  anchor.Identifier
                );

                continue;
            }

            /*            aRPrefab.transform.GetChild(0).transform.localRotation = rotQRCode;
                        content = aRPrefab;

                        textInitial.text = "Initial: " + anchor.Transform.ToPosition().ToString();

                        anchorOne = anchor;*/

/*            anchorOneGO.transform.GetChild(0).transform.localRotation = rotQRCode;
            anchorTwoGO.transform.GetChild(0).transform.localRotation = rotQRCode;*/
            Debug.Log("Anchor: " + anchor.Identifier);

        }
    }

    private void QRCodeScanerAsynHandlerGotResult(ARQRCodeScannerGotResultArgs args)
    {
        IQRCodeResult qRCodeResult = args.QRCodeResult;
        var disToCenter = Vector2.Distance(qRCodeResult.Center, new Vector2((float)qRCodeResult.Width / 2, (float)qRCodeResult.Height / 2));
        if (disToCenter > minDistance)
        {
            Debug.Log("Far");
            return;
        }
        if (VerifiedScannings.Count == numScaners)
        {
            Debug.Log("Finish");
            qRCodeScanerAsynHandler.GotResult -= QRCodeScanerAsynHandlerGotResult;
            qRCodeScanerAsynHandler.Stop();
            PlaceARObject();
            return;
        }
        Debug.Log("Added");
        VerifiedScannings.Add(qRCodeResult);
    }

    private void PlaceARObject() 
    {
        var lastReslt = VerifiedScannings[numScaners - 1];
        angleRot = (float)lastReslt.Angle;
        qRCodeDepth.GenerateArtificialDepthEstimate("20CeriumAR");
        depthAverageQRCode = qRCodeDepth.DepthEstimation;


        Vector3 centerPost = cameraAR.ScreenToWorldPoint(new Vector3(Screen.width / 2, (Screen.height / 2), depthAverageQRCode));
        posQRCode = centerPost;

        Quaternion rotationGO = Quaternion.Euler(0, 0, angleRot);

        exanger.transform.parent = cameraAR.transform;
        exanger.transform.localRotation = rotationGO;
        exanger.transform.position = centerPost;
        exanger.transform.parent = null;

        posQRCode = exanger.transform.position;
        rotQRCode = exanger.transform.rotation;

        AddAnchorInSpace(posQRCode);
    }

    private void AddAnchorInSpace(Vector3 pos) 
    {
        Matrix4x4 anchorOneMatrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
        IARAnchor _anchorOne = Session.AddAnchor(anchorOneMatrix);
        addedAnchors.Add(_anchorOne.Identifier, _anchorOne);
        anchorOne = _anchorOne;

        Debug.LogFormat("Created anchor (id: {0}, position: {1} ", _anchorOne.Identifier, pos.ToString("F4"));

        anchorOneGO.transform.localPosition = anchorOne.Transform.ToPosition();
        anchorOneGO.transform.GetChild(0).transform.localRotation = rotQRCode;
        var pos2 = anchorOneGO.transform.GetChild(0).transform.TransformPoint(new Vector3(0.485f, 0, 0));

        Matrix4x4 anchorTwoMatrix = Matrix4x4.TRS(pos2, Quaternion.identity, Vector3.one);
        IARAnchor _anchorTwo = Session.AddAnchor(anchorTwoMatrix);
        addedAnchors.Add(_anchorTwo.Identifier, _anchorTwo);
        anchorTwo = _anchorTwo;
        anchorTwoGO.transform.localPosition = anchorTwo.Transform.ToPosition();
        anchorTwoGO.transform.GetChild(0).transform.localRotation = rotQRCode;

        Debug.LogFormat("Created anchor (id: {0}, position: {1} ", _anchorOne.Identifier, pos.ToString("F4"));
    }

    private void OnApplicationQuit()
    {
        qRCodeScanerAsynHandler.Stop();
    }
}

