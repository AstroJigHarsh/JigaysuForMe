using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    public TMP_Dropdown resDropDown;
    public GameObject Sett;
    public GameObject Lobby;
    Resolution[] resolutions;
    void Start()
    {
        resolutions = Screen.resolutions;

        resDropDown.ClearOptions();

        List<string> Opti = new List<string>();

        int CurrentResInd = 0;
        
        for(int i = 0; i < resolutions.Length; i++)
        {
            string Opt = resolutions[i].width + "x" + resolutions[i].height;
            Opti.Add(Opt);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                CurrentResInd = i;
            }
        
        }
        resDropDown.AddOptions(Opti);
        resDropDown.value = CurrentResInd;
        resDropDown.RefreshShownValue();
    }

    public void Open()
    {
        Sett.SetActive(true);
        Lobby.SetActive(false);
    }

    public void Close()
    {
        Sett.SetActive(false);
        Lobby.SetActive(true);
    }

    public void ResChabge(int ResIndx)
    {
        Resolution resolution = resolutions[ResIndx];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void FullScreenToggle(bool Toggle)
    {
        Screen.fullScreen = Toggle;
    }

    public void Quality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
    }
}
