using DatabaseAPI.Account;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Text userIdText;
    public Text hightScoreText;
    public Text scoreText;
    public Text levelText;
    public Text moneyText;

    private int hightScore;
    private int currentScore;
    private int level;
    private int money;

    public GameObject SignInSection;
    public GameObject LoadingScreen;
    public GameObject SignUpSection;

    void Start()
    {
        SignInSection.SetActive(true);
        SignUpSection.SetActive(false);
        StartCoroutine(UpdateAllStats());
    }

    IEnumerator UpdateAllStats()
    {
        yield return new WaitForSeconds(1); // Data refresh frequency

        if (AccountController.controller.connectedToAnAccount && !AccountController.controller.getStatsFinished)
        {
            LoadingScreen.SetActive(true);
        }
        else
        {
            LoadingScreen.SetActive(false);
        }

        // Get the statictic in the player data dictionnary
        AccountController.playerData.TryGetValue("HightScore", out hightScore);
        AccountController.playerData.TryGetValue("Level", out level);
        AccountController.playerData.TryGetValue("Money", out money);


        if(AccountController.userID == null)
        {
            userIdText.text = "Connected anonymously..."; // Display this if the user doesn't have an id, generally with an anonymous connection on mobile devices
        }
        else
        {
            userIdText.text = "Connected to user " + AccountController.userID; // display the user ID in playfab servers; you can also type userName instead of userID to get the username
        }

        hightScoreText.text = "Hight Score: " + hightScore.ToString();
        scoreText.text = currentScore.ToString() + " points";
        levelText.text = "Level " + level.ToString();
        moneyText.text = money.ToString() + " credits";

        StartCoroutine(UpdateAllStats());
    }

    #region Button Section
    public void UpdateScore(int value)
    {
        currentScore += value;

        if (currentScore > hightScore)
            AccountController.controller.SetStat("HightScore", currentScore);
    }
    public void UpdateLevel(int value)
    {
        AccountController.controller.SetStat("Level", level + value);
    }
    public void UpdateMoney(int value)
    {
        AccountController.controller.SetStat("Money", money + value);
    }

    public void GetUserEmail(string value)
    {
        AccountController.controller.GET_USER_EMAIL(value);
    }
    public void GetUserName(string value)
    {
        AccountController.controller.GET_USER_USERNAME(value);
    }
    public void GetUserPassword(string value)
    {
        AccountController.controller.GET_USER_PASSWORD(value);
    }

    public void ActivateSignInOrOutPanel()
    {
        SignInSection.SetActive(SignInSection.activeSelf ? false : true);
        SignUpSection.SetActive(SignUpSection.activeSelf ? false : true);
    }

    public void SignInOrUp()
    {
        int toDo = SignInSection.activeSelf ? 0 : 1;

        if(toDo == 0)
            AccountController.controller.LOGIN_ACTION();
        else if(toDo == 1)
            AccountController.controller.ON_CLIC_CREATE_ACCOUNT();
    }
    public void SignOut()
    {
        AccountController.controller.LOG_OUT();
    }

#if UNITY_ANDROID || UNITY_IOS
    #region For Anonymous connection /!\ ONLY FOR MOBILE DEVICES /!\
    public void OpenCloseRecoverySection()
    {
        AccountController.controller.OPEN_RECOVERY_PANEL();
    }
    public void CreateRecoveryAccount()
    {
        AccountController.controller.ON_CLIC_ADD_RECOVERY();
    }
    #endregion
#endif
#endregion
}
