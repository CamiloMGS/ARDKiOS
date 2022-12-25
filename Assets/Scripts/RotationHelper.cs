using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RotationHelper : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMeshPro;
    [SerializeField] private GameObject aRCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.text = aRCamera.transform.localRotation.eulerAngles.ToString();
    }
}
