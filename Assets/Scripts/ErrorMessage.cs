using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : MonoBehaviour
{
    private float displayTime;
    private string message;
    private Text theText;
    public static ErrorMessage instance;
    
    void Start ()
    {
        theText = GetComponent<Text>();
        instance = this;
	}

    public void NewMessage(string message)
    {
        this.message = message;
        displayTime = 0;
    }
	
	void FixedUpdate ()
    {
        displayTime += Time.fixedDeltaTime;
        if (displayTime >= 2)
            message = "";

        string opac;
        // Get Opacity In Hex based on how long the text has been there.
        int opacAmount = GetOpacity(displayTime);
        if (opacAmount < 16)
            opac = "0" + opacAmount.ToString("X");
        else
            opac = opacAmount.ToString("X");

        theText.text = "<color=#FF0000" + opac + ">" + message + "</color>";
    }

    int GetOpacity(float time)
    {
        return Mathf.CeilToInt(-125 * time + 255);
    }
}