using Niantic.ARDK.AR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Anchors;
using System;
using Niantic.ARDKExamples.Common.Helpers;

public class AnchorManager : MonoBehaviour
{
    private IARSession Session = null;
    private Dictionary<Guid, IARAnchor> addedAnchors = new Dictionary<Guid, IARAnchor>();
    private Dictionary<Guid, GameObject> placedObjects = new Dictionary<Guid, GameObject>();
    [SerializeField] private GameObject aRObject;
    [SerializeField] private Quaternion rot;
    [SerializeField] private GameObject cubePref;
    // Start is called before the first frame update
    void Start()
    {
        ARSessionFactory.SessionInitialized += ARSessionFactory_SessionInitialized;
    }

    public void AddAnchorInSpace(Vector3 pos, Quaternion rot)
    {
        this.rot = rot;
        Matrix4x4 anchorMatrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
        IARAnchor anchor = Session.AddAnchor(anchorMatrix);
        addedAnchors.Add(anchor.Identifier, anchor);

        Debug.LogFormat("Created anchor (id: {0}, position: {1} ", anchor.Identifier, pos.ToString("F4"));
    }
    private void ARSessionFactory_SessionInitialized(AnyARSessionInitializedArgs args)
    {
        Session = args.Session;
        Session.AnchorsAdded += AnchorsAdded;
    }

    private void AnchorsAdded(AnchorsArgs args)
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

            var cube = Instantiate  (cubePref, new Vector3(0, 0, 0), Quaternion.identity);

            var attachment = cube.AddComponent<ARAnchorAttachment>();
            attachment.AttachedAnchor = anchor;
            var cubeYOffset = cubePref.transform.localScale.y / 2;
            attachment.Offset = Matrix4x4.Translate(new Vector3(0, cubeYOffset, 0));



            /*            GameObject GO = Instantiate(aRObject, Vector3.zero, Quaternion.identity);
                        GO.SetActive(true);
                        GO.transform.GetChild(0).transform.localRotation = rot;
                        var attachment = GO.AddComponent<ARAnchorAttachment>();
                        attachment.AttachedAnchor = anchor;
                        attachment.Offset = Matrix4x4.Translate(new Vector3(0, 0, 0));
                        placedObjects.Add(anchor.Identifier, GO);*/
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
