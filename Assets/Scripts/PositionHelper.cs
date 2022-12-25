using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PositionHelper : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;
    [SerializeField] private GameObject obj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textUI.text = string.Format("Local Position: {0}\nWorld Position:{1}", obj.transform.localPosition.ToString("F4"), obj.transform.position.ToString("F4"));
    }
}
