using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class PlayFabLoginManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject LoginPanel;
    public GameObject Lobby;

    [Header("Login Screen")]
    public TMP_InputField LoginEmailField;
    public TMP_InputField LoginPasswordField;
    public TMP_InputField LoginDisplayNameField;
    public Button LoginBtn;


    

    
    public void OpenLobbyPanel()
    {
        
        LoginPanel.SetActive(false);

        Lobby.SetActive(true);
    }
    


    public void OnTryLogin()
    {
        string email = LoginEmailField.text;
        string password = LoginPasswordField.text;

        LoginBtn.interactable = false;

        var req = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
        };

        PlayFabClientAPI.LoginWithEmailAddress(req,
        res =>
        {
            Debug.Log("Login Success");
            PhotonNetwork.NickName = LoginDisplayNameField.text;
            OpenLobbyPanel();
        },
        err =>
        {
            Debug.Log("Error: " + err.ErrorMessage);
            LoginBtn.interactable = true;
        });


    }
    
}