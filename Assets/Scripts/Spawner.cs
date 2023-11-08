using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
public class Spawner : MonoBehaviour
{
    public GameObject jigaysuPrefab;
    // Start is called before the first frame update
    public void Start()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved,
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });
    }

    public void OnDataRecieved(GetUserDataResult result)
    {
        if (jigaysuPrefab == null && result.Data.ContainsKey("Player"))
        {
            if (result.Data["Player"].Value == "Jigyasu")
            {
                Debug.LogFormat("We are Instantiating LocalJigyasuPlayer from {0}", Application.loadedLevelName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.jigaysuPrefab.name, new Vector3(600f, 30f, 300f), Quaternion.identity, 0);
            }
        }
    }




   


}
