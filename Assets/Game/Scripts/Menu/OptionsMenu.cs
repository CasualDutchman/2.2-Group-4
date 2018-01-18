using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {

    public static OptionsMenu instance;

    void Awake() { instance = this; }

    public GameObject graphical, keys;

    public Dropdown resolutionDropdown, qualityDropdown;

    Resolution[] resolutions;

    public float mouseSpeed = 4;

	void Start () {
        resolutions = Screen.resolutions;

        //resolution options
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

        //quality options
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
	
    public void SetQuality(int qualityIndex) {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetResolution(int resolutionIndex) {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
    }

    public void SetMouseSpeed(float value) {
        mouseSpeed = value;
    }

    public void ViewGraphical() {
        if (!graphical.activeSelf) {
            graphical.SetActive(true);
            keys.SetActive(false);
        }
    }
    public void ViewKeyBinding() {
        if (!keys.activeSelf) {
            keys.SetActive(true);
            graphical.SetActive(false);
        }
    }
}
