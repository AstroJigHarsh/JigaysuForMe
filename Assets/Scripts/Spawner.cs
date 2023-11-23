using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
public class Spawner : MonoBehaviourPunCallbacks
{
    public GameObject Jigyasu;
    public GameObject Venti;
    public GameObject SciFi;

    public Canvas HealthSysGameOver;
    public Canvas HealthSysHealthCanvas;
    public Text HealthSysHealthText;
    string Char;
    GameObject Instance;
    // Start is called before the first frame update
    public void Start()
    {
        GetUserData();
    }

    public void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved,
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });
    }

    public void OnDataRecieved(GetUserDataResult result)
    {
        if (result.Data.ContainsKey("Player"))
        {
            Char = result.Data["Player"].Value;
            if (Char == "Jigyasu")
            {
                if (Jigyasu != null)
                {
                    Instance = PhotonNetwork.Instantiate(this.Jigyasu.name, new Vector3(600f, 30f, 300f), Quaternion.identity, 0);
                    Debug.Log("Instantiated");
                    Instance.GetComponent<NetworkedHealthSystem>().gameOverCanvas= HealthSysGameOver;
                    Instance.GetComponent<NetworkedHealthSystem>().healthCanvas= HealthSysHealthCanvas;
                    Instance.GetComponent<NetworkedHealthSystem>().healthText = HealthSysHealthText;
                }
            }
            if (Char == "Lyney")
            {
                if (Venti != null)
                {
                    Instance = PhotonNetwork.Instantiate(this.Venti.name, new Vector3(600f, 30f, 300f), Quaternion.identity, 0);
                    Debug.Log("Instantiated");
                    Instance.GetComponent<NetworkedHealthSystem>().gameOverCanvas = HealthSysGameOver;
                    Instance.GetComponent<NetworkedHealthSystem>().healthCanvas= HealthSysHealthCanvas;
                    Instance.GetComponent<NetworkedHealthSystem>().healthText = HealthSysHealthText;
                }
            }
            if (Char == "SciFi")
            {
                if(SciFi != null)
                {
                    Instance = PhotonNetwork.Instantiate(this.SciFi.name, new Vector3(600, 30, 300), Quaternion.identity, 0);
                    Debug.Log("Instantiated");
                    Instance.GetComponent<NetworkedHealthSystem>().gameOverCanvas = HealthSysGameOver;
                    Instance.GetComponent<NetworkedHealthSystem>().healthCanvas= HealthSysHealthCanvas;
                    Instance.GetComponent<NetworkedHealthSystem>().healthText = HealthSysHealthText;
                }
            }
        }
    }

    public void LeaveRoomAndDestroy()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Destroy(Instance);
    }
}
