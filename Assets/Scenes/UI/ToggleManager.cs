using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ToggleManager : MonoBehaviour
{
    [SerializeField] private Button[] buttons = new Button[2];

    private void Start()
    {
        buttons[0].onClick.AddListener(delegate { ClickOnToggleButton(0); }); 
        //buttons[0].onClick.AddListener(GameManager.Instance.ToogleAR); 

        buttons[1].onClick.AddListener(delegate { ClickOnToggleButton(1); });
        //buttons[1].onClick.AddListener(GameManager.Instance.ToogleAR);

        Initialize();
    }

    private void Initialize() 
    {
        buttons[0].onClick.Invoke();
    }
    private void ClickOnToggleButton(int buttonOn) 
    {
        int buttonOff = buttonOn == 0 ? 1 : 0;

        buttons[buttonOn].enabled = false;
        buttons[buttonOn].GetComponent<Image>().color = new Color(0, 0.475f, 1, 1);
        buttons[buttonOn].transform.GetChild(0).GetComponent<Image>().color = Color.white;
        buttons[buttonOn].transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;

        buttons[buttonOff].enabled = true;
        buttons[buttonOff].GetComponent<Image>().color = new Color(1.0f,1.0f,1.0f,0.25f);
        buttons[buttonOff].transform.GetChild(0).GetComponent<Image>().color = Color.black;
        buttons[buttonOff].transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.black;
    }
}
