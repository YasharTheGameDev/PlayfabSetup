using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class ItemToBuy : MonoBehaviour
{
    public int coinsPrice;
    public string itemName;
    public void BuyItem() 
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = "CN",
            Amount = coinsPrice
        };
        PlayFabClientAPI.SubtractUserVirtualCurrency(request,OnSubtractCoinsSuccess,OnError);
    }
    void OnSubtractCoinsSuccess(ModifyUserVirtualCurrencyResult result) 
    {
        Debug.Log("Bought item! "+itemName);
        PlayfabManager.instance.GetVirtualCurrencies();
    }
    void OnError(PlayFabError error) 
    {
        Debug.Log("Error: "+error.ErrorMessage);
    }
}
