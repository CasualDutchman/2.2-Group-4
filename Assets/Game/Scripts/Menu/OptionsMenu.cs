using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Author: Pieter
public class OptionsMenu : MonoBehaviour {

    //Set an instance, so other scripts can call from it, without a need of a reference of this class
    public static OptionsMenu instance;
    void Awake() { instance = this; }

    //different screen in the options menu
    public GameObject graphical, keys;

    public Dropdown resolutionDropdown, qualityDropdown;

    Resolution[] resolutions;

    //changeable mouse speed in the controls panel
    public float mouseSpeed = 4;

	void Start () {
        resolutions = Screen.resolutions;

        //resolution options // Set the options for the resolutions automatically
        resolutionDropdown.ClearOptions();

        List<string> resolutionOptions = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++) {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            resolutionOptions.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        //quality options // Set the options for the qualities automatically
        qualityDropdown.ClearOptions();
        List<string> qualityOptions = new List<string>();
        int currentQualityIndex = 0;

        for (int i = 0; i < QualitySettings.names.Length; i++) {
            qualityOptions.Add(QualitySettings.names[i]);

            if (QualitySettings.GetQualityLevel() == i)
                currentQualityIndex = i;
        }
        qualityDropdown.AddOptions(qualityOptions);
        qualityDropdown.value = currentQualityIndex;
        qualityDropdown.RefreshShownValue();
    }
	
    //Change the quality to something else
    public void SetQuality(int qualityIndex) {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    //Change the resolution to something else
    public void SetResolution(int resolutionIndex) {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    //Toggle fullscreen on or off
    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
    }


    //Change the speed of the mouse to a new value
    public void SetMouseSpeed(float value) {
        mouseSpeed = value;
    }

    //Change view to the graphical screen
    public void ViewGraphical() {
        if (!graphical.activeSelf) {
            graphical.SetActive(true);
            keys.SetActive(false);
        }
    }

    //Change view to the controls screen
    public void ViewKeyBinding() {
        if (!keys.activeSelf) {
            keys.SetActive(true);
            graphical.SetActive(false);
        }
    }
}
