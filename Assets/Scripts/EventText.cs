using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EventText : MonoBehaviour
{
    private List<string> textList = new List<string>();
    private List<float> timeList = new List<float>();
    private Text theText;

    public static EventText instance;

    void Start()
    {
        theText = GetComponent<Text>();
        instance = this;
    }

    public void AddWhiteText(string text)
    {
        textList.Add("<color=#FFFFFF|>" + text + "</color>");
        timeList.Add(0);
        ResetText();
    }

    public void AddRedText(string text)
    {
        textList.Add("<color=#FF0000|>" + text + "</color>");
        timeList.Add(0);
        ResetText();
    }

    void ResetText()
    {
        theText.text = "";
        for (int i = 0; i < textList.Count; i++)
        {
            string opac;
            // Get Opacity In Hex based on how long the text has been there.
            int opacAmount = GetOpacity(timeList[i]);
            if (opacAmount < 0x10)
                opac = "0" + opacAmount.ToString("X");
            else
                opac = opacAmount.ToString("X");

            theText.text += textList[i].Replace("|", opac) + System.Environment.NewLine;
        }
    }

    void Update()
    {
        if (timeList.Count > 0)
        {
            for (int i = 0; i < timeList.Count; i++)
            {
                timeList[i] += Time.deltaTime;
                if (timeList[i] >= 8.9f)
                {
                    textList.RemoveAt(i);
                    timeList.RemoveAt(i);
                }
            }
            ResetText();
        }
    }

    int GetOpacity(float time)
    {
        return Mathf.CeilToInt(280 / (time - 10) + 255);
    }
}
