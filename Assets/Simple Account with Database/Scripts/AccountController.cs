using System.Collections.Generic;
using PlayFab.ClientModels;
using System.Collections;
using UnityEditor;
using UnityEngine;
using PlayFab;

namespace DatabaseAPI.Account
{
    public class AccountController : MonoBehaviour
    {
        public static AccountController controller;

        [Header("Login Section")]
#if UNITY_ANDROID || UNITY_IOS
        [HideInInspector] public bool AnonymeLogin;
        [HideInInspector] public GameObject addLoginPanel;
        [HideInInspector] public GameObject recoveryButton;
#endif
        private string userEmail;
        private string userPassword;
        private string username;

        [HideInInspector] public bool connectedToAnAccount;

        [HideInInspector] public GameObject loginPanel;

        public static string DebugMessage;

        [Header("User Info Section")]
        public static string userID;
        public static string userName;

        [Header("User Data Section")]
        [HideInInspector] public bool getStatsFinished;
        public static Dictionary<string, int> playerData = new Dictionary<string, int>();

        private void OnEnable()
        {
            if (AccountController.controller == null) AccountController.controller = this;
            else if (AccountController.controller != this) Destroy(this.gameObject);
            DontDestroyOnLoad(this.gameObject);
        }

        public void Start()
        {
            getStatsFinished = false;
            loginPanel.SetActive(true);
            //Note: Setting title Id here can be skipped if you have set the value in Editor Extensions already.
            if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
            {
                PlayFabSettings.TitleId = "C9C63"; // Please change this value to your own titleId from Account Manager Window
            }

            // Auto Login Condition
            if (PlayerPrefs.HasKey("PLAYFAB_USER_EMAIL"))
            {
                userEmail = PlayerPrefs.GetString("PLAYFAB_USER_EMAIL");
                userPassword = PlayerPrefs.GetString("PLAYFAB_USER_PASSWORD");
                var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
                PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
            }
#if UNITY_ANDROID || UNITY_IOS
            else
            {
                if (AnonymeLogin)
                {
                    addLoginPanel.SetActive(false);
                    recoveryButton.SetActive(true);
#if UNITY_ANDROID
                var requestAndroid = new LoginWithAndroidDeviceIDRequest {AndroidDeviceId = ReturnMobileID(), CreateAccount = true};
                PlayFabClientAPI.LoginWithAndroidDeviceID(requestAndroid, OnLoginMobileSuccess, OnLoginMobileFailure);
#endif
#if UNITY_IOS
                    var requestIOS = new LoginWithIOSDeviceIDRequest { DeviceId = ReturnMobileID(), CreateAccount = true };
                    PlayFabClientAPI.LoginWithIOSDeviceID(requestIOS, OnLoginMobileSuccess, OnLoginMobileFailure);
#endif
                }
                else
                {
                    addLoginPanel.SetActive(false);
                    recoveryButton.SetActive(false);
                }
            }
#endif
        }

        #region Login/Register Section
        #region Private Callbacks
        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Congratulations, you're now connected!");
            PlayerPrefs.SetString("PLAYFAB_USER_EMAIL", userEmail);
            PlayerPrefs.SetString("PLAYFAB_USER_PASSWORD", userPassword);
            DebugMessage = "you're now connected!";
            loginPanel.SetActive(false);
#if UNITY_ANDROID || UNITY_IOS
            recoveryButton.SetActive(false);
#endif
            connectedToAnAccount = true;
            GetStats();
            GetAccountInfo();
        }
        private void OnLoginMobileSuccess(LoginResult result)
        {
            Debug.Log("Congratulations, you're now connected with mobile ID!");
            DebugMessage = "you're now connected with mobile ID!";
            loginPanel.SetActive(false);
            connectedToAnAccount = true;
            GetStats();
        }
        private void OnLoginFailure(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
            DebugMessage = error.GenerateErrorReport();
            connectedToAnAccount = false;
        }
#if UNITY_ANDROID || UNITY_IOS
        private void OnLoginMobileFailure(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
            DebugMessage = error.GenerateErrorReport();
            connectedToAnAccount = false;
        }
#endif
        private void OnRegisterSuccess(RegisterPlayFabUserResult result)
        {
            PlayerPrefs.SetString("PLAYFAB_USER_EMAIL", userEmail);
            PlayerPrefs.SetString("PLAYFAB_USER_PASSWORD", userPassword);
            PlayerPrefs.SetString("PLAYFAB_USER_USERNAM", username);
            DebugMessage = "you're now connected!";
            loginPanel.SetActive(false);
            connectedToAnAccount = true;
            GetStats();
            GetAccountInfo();
        }
        private void OnRegisterFailed(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());
            DebugMessage = error.GenerateErrorReport();
            connectedToAnAccount = false;
        }
#if UNITY_ANDROID || UNITY_IOS
        private void OnAddLoginSuccess(AddUsernamePasswordResult result)
        {
            Debug.Log("Congratulations, you're now connected!");
            DebugMessage = "you're now connected!";
            PlayerPrefs.SetString("PLAYFAB_USER_EMAIL", userEmail);
            PlayerPrefs.SetString("PLAYFAB_USER_PASSWORD", userPassword);
            addLoginPanel.SetActive(false);
            recoveryButton.SetActive(false);
            connectedToAnAccount = true;
            GetStats();
        }
        private void OnAddRegisterFailed(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
            DebugMessage = error.GenerateErrorReport();
            LOGIN_ACTION();
        }
#endif
        #endregion

        #region Public Callbacks
        /// <summary>
        /// Call this to connect the player into playfab account. 
        /// </summary>
        public void LOGIN_ACTION()
        {
            var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }
#if UNITY_ANDROID || UNITY_IOS
        /// <summary>
        /// Call this to open your recovery panel that you just set in the editor window. 
        /// </summary>
        public void OPEN_RECOVERY_PANEL()
        {
            addLoginPanel.SetActive(addLoginPanel.activeSelf ? false : true);
        }
        /// <summary>
        /// Call this to connect or create an account for the anonym player. 
        /// </summary>
        public void ON_CLIC_ADD_RECOVERY()
        {
            var AddLoginRequest = new AddUsernamePasswordRequest { Email = userEmail, Password = userPassword, Username = username };
            PlayFabClientAPI.AddUsernamePassword(AddLoginRequest, OnAddLoginSuccess, OnAddRegisterFailed);
        }
#endif
        /// <summary>
        /// Call this to create an account and connect the player into it. 
        /// </summary>
        public void ON_CLIC_CREATE_ACCOUNT()
        {
            var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username };
            PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailed);
        }

        public void GET_USER_EMAIL(string emailIn)
        {
            userEmail = emailIn;
        }
        public void GET_USER_PASSWORD(string passwordIn)
        {
            userPassword = passwordIn;
        }
        public void GET_USER_USERNAME(string usernameIn)
        {
            username = usernameIn;
        }

        /// <summary>
        /// Call this to logout the player from playfab account. 
        /// </summary>
        public void LOG_OUT()
        {
            PlayerPrefs.DeleteKey("PLAYFAB_USER_EMAIL");
            Debug.Log("Now Disconnected.");
            connectedToAnAccount = false;
            Application.Quit();
        }

        public static string ReturnMobileID()
        {
            string deviceID = SystemInfo.deviceUniqueIdentifier;
            return deviceID;
        }
        #endregion
        #endregion

        #region User Data Section
        void GetAccountInfo()
        {
            GetAccountInfoRequest request = new GetAccountInfoRequest();
            PlayFabClientAPI.GetAccountInfo(request, Successs, fail);
        }
        void Successs(GetAccountInfoResult result)
        {
            userID = result.AccountInfo.PlayFabId;
            userName = result.AccountInfo.Username;
        }
        void fail(PlayFabError error)
        {

            Debug.LogError(error.GenerateErrorReport());
        }

        public void SetStat(string statistcName, int statiscticValue)
        {
            PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate> {
        new StatisticUpdate { StatisticName = statistcName, Value = statiscticValue },
            }
            },
            result =>
            {
                // update or add this data to the dictionary
                try { playerData[statistcName] = statiscticValue; }
                catch { playerData.Add(statistcName, statiscticValue); }
            },
            error => { Debug.LogError(error.GenerateErrorReport()); DebugMessage = error.GenerateErrorReport(); });
        }

        void GetStats()
        {
            PlayFabClientAPI.GetPlayerStatistics(
                new GetPlayerStatisticsRequest(),
                OnGetStats,
                error => { Debug.LogError(error.GenerateErrorReport()); DebugMessage = error.GenerateErrorReport(); }
            );
        }
        void OnGetStats(GetPlayerStatisticsResult result)
        {
            Debug.Log("Received the following Statistics:");
            playerData.Clear();
            foreach (var eachStat in result.Statistics)
            {
                playerData.Add(eachStat.StatisticName, eachStat.Value);
            }
            foreach (var data in playerData)
            {
                Debug.Log(data.Key + ": " + data.Value);
            }
            getStatsFinished = true;
        }
        #endregion
    }
}