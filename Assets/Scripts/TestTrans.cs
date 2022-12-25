using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTrans : MonoBehaviour
{
    [SerializeField] private GameObject obj1;
    [SerializeField] private GameObject obj2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = obj1.transform.GetChild(0).transform.TransformPoint(new Vector3(0.5f, 0, 0));
        obj2.transform.position = pos;
    }
}
