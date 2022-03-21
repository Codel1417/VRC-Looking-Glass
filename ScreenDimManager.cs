using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class ScreenDimManager : MonoBehaviour
{
    public Image screenOverlay;
    public float fadeSpeed = 1f;
    private Color targetColor;
    private Color currentColor;
    private Color previousColor;
    private float fadeAmount;
    [UsedImplicitly] public bool isFading;
    // Start is called before the first frame update
    void Start()
    {
        currentColor = screenOverlay.color;
        targetColor = currentColor;
        previousColor = currentColor;
        fadeAmount = 0f;
        isFading = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isFading)
        {
            fadeAmount += fadeSpeed * Time.deltaTime;
            currentColor = Color.Lerp(previousColor, targetColor, fadeAmount);
            isFading =  fadeAmount < 1f;
            screenOverlay.color = currentColor;
        }
        
        if (!isFading)
        {
            currentColor = targetColor;
            
        }
    }
    public void FadeTo(Color newColor)
    {
        if (newColor == targetColor)
        {
            return;
        }
        targetColor = newColor;
        previousColor = currentColor;
        fadeAmount = 0f;
        isFading = true;
    }

}
