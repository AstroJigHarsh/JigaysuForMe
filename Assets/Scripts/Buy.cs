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
using Unity.VisualScripting;

public class Buy : MonoBehaviour
{
    public int CoinsPrice;
    public string ItemName;
    public string Type;
    public GameObject BuyB;
    public GameObject Equip;
    public GameObject LessCurrency;

    public void Start()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                Debug.Log("Gotcha");
            },
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });
        GetUserInventory();
    }

    public void GetUserInventory()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnDataRecieved,
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });
    }

    private void OnDataRecieved(GetUserInventoryResult result)
    {
        foreach (ItemInstance item in result.Inventory)
        {
            if (item.ItemId == ItemName)
            {
                Equip.SetActive(true);
                BuyB.SetActive(false);
            }
            


        }
    }


    public void EquipIt()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {Type, ItemName}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend,
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });
    }


    public void ButIt()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result =>
            {
                if (result.VirtualCurrency["AP"] < CoinsPrice)
                {
                    Error();
                }
                else
                    Grant();
            },
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });
     }

    public void Grant()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GiveItem",
            FunctionParameter = new
            {
                Item = ItemName,
                Amt = CoinsPrice
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                SubtractAP();
            },
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });
    }

    public void SubtractAP()
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = "AP",
            Amount = CoinsPrice
        };
        PlayFabClientAPI.SubtractUserVirtualCurrency(request, OnSuccess,
            err =>
            {
                Debug.Log("Error: " + err.ErrorMessage);
            });
    }

    private void OnSuccess(ModifyUserVirtualCurrencyResult result)
    {
        GetUserInventory();
    }


    private void OnDataSend(UpdateUserDataResult result)
    {
        Debug.Log("Done");
        UserName.instance.GetVirtualCurrency();
    }

    private void Error()
    {
        LessCurrency.SetActive(true);
    }

    public void Close()
    {
        LessCurrency.SetActive(false);
    }

}