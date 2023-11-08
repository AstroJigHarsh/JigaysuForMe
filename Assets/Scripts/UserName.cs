using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;
using System;

public class UserName : MonoBehaviour
{
    [Header("Screens")]
    public GameObject LoginPanel;
    public GameObject Lobby;
    public GameObject Leaderboard;
    public GameObject ChoosePlayer;

    [Header("Login Screen")]
    public TMP_InputField LoginEmailField;
    public TMP_InputField LoginPasswordField;
    public TMP_InputField LoginDisplayNameField;
    public Button LoginBtn;

    [Header("Mischevious")]
    public GameObject LeaderListPrefab;
    public Transform LeaderBoardSpawner;

    public void Start()
    {
        if (PhotonNetwork.IsConnected != true)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
        else OpenLoginPanel();
    }
    public void OpenLobbyPanel()
    {

        LoginPanel.SetActive(false);
        Leaderboard.SetActive(false);
        Lobby.SetActive(true);
        ChoosePlayer.SetActive(false);
    }
    public void OpenLoginPanel()
    {

        LoginPanel.SetActive(true);
        Leaderboard.SetActive(false);
        Lobby.SetActive(false);
        ChoosePlayer.SetActive(false);
    }
    public void OpenLeaderboardPanel()
    {

        LoginPanel.SetActive(false);
        Leaderboard.SetActive(false);
        Lobby.SetActive(true);
        ChoosePlayer.SetActive(false);
    }
    public void OpenPlayerChoosePanel()
    {
        LoginPanel.SetActive(false);
        Leaderboard.SetActive(false);
        Lobby.SetActive(false);
        ChoosePlayer.SetActive(true);
    }


    public void OnTryLogin()
    {
        string email = LoginEmailField.text;
        string password = LoginPasswordField.text;

        LoginBtn.interactable = false;

        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetPlayerProfile = true }
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, 
      
        err =>
        {
            Debug.Log("Error: " + err.ErrorMessage);
            LoginBtn.interactable = true;
        });

        


    }

    private void OnLoginSuccess(LoginResult result)
    {
        if (result.InfoResultPayload.PlayerProfile != null)
            PhotonNetwork.NickName = result.InfoResultPayload.PlayerProfile.DisplayName;


        Debug.Log("Login Success");
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved,
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });

    }

    

    public void SendLeaderboard()
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "BarrelPoints",
                    Value = 10
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request,
         res =>
         {
             Debug.Log("Successfull leaderboard sent");
         },
        err =>
        {
            Debug.Log("Error: " + err.ErrorMessage);
        });
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "BarrelPoints",
            StartPosition = 1,
            MaxResultsCount = 10

        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet,
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });

    }

    private void OnLeaderboardGet(GetLeaderboardResult result)
    {

        foreach(Transform item in LeaderBoardSpawner)
        {
            Destroy(item.gameObject);
        }
        foreach(var item in result.Leaderboard)
        {

            GameObject NewGo = Instantiate(LeaderListPrefab, LeaderBoardSpawner);
            Text[] texts = NewGo.GetComponentsInChildren<Text>();
            texts[0].text = item.Position.ToString();
            texts[1].text = item.DisplayName;
            texts[2].text = item.StatValue.ToString();
            Debug.Log(string.Format( "Place : {0} | UserName : {1} | Score : {2}", 
                item.Position, item.DisplayName ,item.StatValue));
            OpenLeaderboardPanel();
        }

    }


    private void OnDataRecieved(GetUserDataResult result)
    {
        if (result != null && result.Data.ContainsKey("Player"))
        {
            OpenLobbyPanel();
        }
        else
        {
            OpenPlayerChoosePanel();
        }
    }

    public void SendPlayerJigyasu()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"Player", "Jigyasu" }
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend,
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });
    }

    private void OnDataSend(UpdateUserDataResult result)
    {
        Debug.Log("Completed");
        OpenLobbyPanel();
    }


}