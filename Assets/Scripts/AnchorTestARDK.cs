using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Utilities.Input.Legacy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.ARDK.AR.Anchors;
using Niantic.ARDK.Utilities;

public class AnchorTestARDK : MonoBehaviour
{

    [SerializeField] private GameObject aRObjectPrefab;
    private Camera aRCamera;
    bool shoulPlaceARObject = true;
    private IARSession session;
    private IARAnchor anchor;

    void Start()
    {
        ARSessionFactory.SessionInitialized += OnARSessionInitialized;
        aRCamera = Camera.main;
    }

    void Update()
    {
        if (session == null)
            return;

        if (anchor != null && aRObjectPrefab != null)
        {
            aRObjectPrefab.transform.position = anchor.Transform.ToPosition();
            aRObjectPrefab.transform.rotation = anchor.Transform.ToRotation();
        }

        if (PlatformAgnosticInput.touchCount <= 0)
            return;

        Touch touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        if (shoulPlaceARObject)
            HitToPlaceAnchor();
    }

    private void OnARSessionInitialized(AnyARSessionInitializedArgs args)
    {
        session = args.Session;
        session.AnchorsAdded += Session_AnchorsAdded;
        session.AnchorsUpdated += Session_AnchorsUpdated;
    }

    private void Session_AnchorsUpdated(AnchorsArgs args)
    {
        foreach (var anchor in args.Anchors)
        {
            Debug.Log("Update Anchor Position: " + anchor.Transform.ToPosition().ToString("F4"));
        }
    }

    private void Session_AnchorsAdded(AnchorsArgs args)
    {
        foreach (var anchor in args.Anchors)
        {

            if (aRObjectPrefab == null)
            {
                aRObjectPrefab = CreateARObject();
            }

            aRObjectPrefab.transform.position = anchor.Transform.ToPosition();
            aRObjectPrefab.transform.rotation = anchor.Transform.ToRotation();
        }
    }



    private void HitToPlaceAnchor()
    {
        Vector3 position = aRCamera.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, 0.15f));
        Matrix4x4 tMAnchor = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
        anchor = session.AddAnchor(tMAnchor);

        Debug.LogFormat("Created anchor (id: {0}, position: {1} ", anchor.Identifier, position.ToString("F4"));
        Debug.Log("Anchor initial position: " + position.ToString("F4"));
    }

    public GameObject CreateARObject() 
    {
        GameObject root = new GameObject("Root");
        GameObject aRContent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        aRContent.name = "ARContent";
        aRContent.transform.position = new Vector3(0.24f, 0, 0);
        aRContent.transform.localScale = new Vector3(0.48f, 0.045f, 0.01f);
        aRContent.transform.parent = root.transform;
        shoulPlaceARObject = false;
        return root;
    }
}
