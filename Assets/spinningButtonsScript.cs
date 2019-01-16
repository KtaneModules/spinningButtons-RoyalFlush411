using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class spinningButtonsScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] buttons;
    public TextMesh[] buttonText;
    private String[] textOptions = new String[6] {"f","l","q","w","y","d"};
    private List<int> selectedIndices = new List<int>();
    public Renderer[] buttonRend;
    public Material[] buttonMatOptions;
    public String[] buttonNameOptions = new String[6] {"red","purple","orange","grey","green","blue"};
    public Renderer[] indicatorLights;
    public Material[] indicatorMaterials;
    public Animator anim;

    public int[] buttonLocations = new int[4];
    public int[] orderOfButtons = new int[4];

    public ButtonOrder[] buttonInfo;
    private int expectedValue = 0;
    private int stage = 0;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { ButtonPress(pressedButton); return false; };
        }
    }

    void Start()
    {
        foreach(Renderer indic in indicatorLights)
        {
            indic.material = indicatorMaterials[0];
        }
        AssignNumbers();
        AssignMaterials();
        CalculateButtonValue();
        OrderButtons();
        Log();
    }

    void AssignNumbers()
    {
        for(int i = 0; i <= 3; i++)
        {
            int index = UnityEngine.Random.Range(0,6);
            while(selectedIndices.Contains(index))
            {
                index = UnityEngine.Random.Range(0,6);
            }
            selectedIndices.Add(index);
            buttonText[i].text = textOptions[index];
            buttonInfo[i].numberLabel = index;
        }
        selectedIndices.Clear();
    }

    void AssignMaterials()
    {
        for(int i = 0; i <= 3; i++)
        {
            int index = UnityEngine.Random.Range(0,6);
            while(selectedIndices.Contains(index))
            {
                index = UnityEngine.Random.Range(0,6);
            }
            selectedIndices.Add(index);
            buttonRend[i].material = buttonMatOptions[index];
            buttonInfo[i].colourValue = index;
            buttonInfo[i].colourName = buttonNameOptions[index];
        }
        selectedIndices.Clear();
    }

    void CalculateButtonValue()
    {
        for(int i = 0; i <= 3; i++)
        {
            buttonInfo[i].buttonValue = buttonInfo[i].colourValue + buttonInfo[i].numberLabel;
        }
    }

    void OrderButtons()
    {
        for(int i = 0; i <=3; i++)
        {
            buttonLocations[i] = i+1;
            orderOfButtons[i] = buttonInfo[i].buttonValue;
        }
        Array.Sort(orderOfButtons, buttonLocations);
        expectedValue = orderOfButtons[stage];
    }

    void Log()
    {
        for(int i = 0; i <= 3; i++)
        {
            Debug.LogFormat("[Spinning Buttons #{0}] Button #{1} is {2} and says {3}. Its value is {4}.", moduleId, buttonInfo[i].buttonLocation.ToString(), buttonInfo[i].colourName, textOptions[buttonInfo[i].numberLabel].ToString(), buttonInfo[i].buttonValue.ToString());
            //Debug.LogFormat("[Spinning Buttons #{0}] Button #{1} is {2} and says {3}.", moduleId, buttonInfo[i].buttonLocation.ToString(), buttonInfo[i].colourName, textOptions[buttonInfo[i].numberLabel-1]);
        }

    }

    void ButtonPress(KMSelectable pressedButton)
    {
        if(moduleSolved || pressedButton.GetComponent<ButtonOrder>().pressed)
        {
            return;
        }
        pressedButton.AddInteractionPunch();
        if(pressedButton.GetComponent<ButtonOrder>().buttonValue == expectedValue)
        {
            Audio.PlaySoundAtTransform("beep", transform);
            Debug.LogFormat("[Spinning Buttons #{0}] You pressed the {1} button that says {2}. That is correct.", moduleId, pressedButton.GetComponent<ButtonOrder>().colourName, textOptions[pressedButton.GetComponent<ButtonOrder>().numberLabel]);
            pressedButton.GetComponent<ButtonOrder>().pressed = true;
            indicatorLights[pressedButton.GetComponent<ButtonOrder>().buttonLocation-1].material = indicatorMaterials[1];
            stage++;
            if(stage < 4)
            {
                expectedValue = orderOfButtons[stage];
            }
            else
            {
                anim.SetTrigger("solved");
                GetComponent<KMBombModule>().HandlePass();
                Debug.LogFormat("[Spinning Buttons #{0}] Module disarmed.", moduleId);
                moduleSolved = true;
                StartCoroutine(PassAlarm());
            }
        }
        else
        {
            for(int i = 0; i <= 3; i++)
            {
                buttonInfo[i].pressed = false;
            }
            Audio.PlaySoundAtTransform("wrong", transform);
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Spinning Buttons #{0}] Strike! You pressed the {1} button that says {2}. That is incorrect.", moduleId, pressedButton.GetComponent<ButtonOrder>().colourName, textOptions[pressedButton.GetComponent<ButtonOrder>().numberLabel]);
            stage = 0;
            Start();
        }
    }

    IEnumerator PassAlarm()
    {
        yield return new WaitForSeconds(0.25f);
        Audio.PlaySoundAtTransform("wrong", transform);
        yield return new WaitForSeconds(0.2f);
        Audio.PlaySoundAtTransform("beep", transform);
        yield return new WaitForSeconds(0.15f);
        Audio.PlaySoundAtTransform("wrong", transform);
        yield return new WaitForSeconds(0.1f);
        Audio.PlaySoundAtTransform("beep", transform);
        yield return new WaitForSeconds(0.05f);
        Audio.PlaySoundAtTransform("wrong", transform);
        yield return new WaitForSeconds(0.01f);
        Audio.PlaySoundAtTransform("beep", transform);
    }
}
