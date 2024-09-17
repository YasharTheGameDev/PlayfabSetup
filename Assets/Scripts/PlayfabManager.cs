using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using System;

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager instance;
    // name
    public GameObject nameWindow;
    public GameObject leaderboardWindow;

    string playerName;

    GameObject rowPrefab;
    Transform rowParent;
    // name


    string Email;
    string Password;

    int A_Score;
    float B_Score;
    string C_Score;

    string TitleData_String;
    int TitleData_Int;

    public class Data_Json
    {
        int A_Json;
        float B_Json;
        string C_Json;
    }
    Data_Json[] _Data_Json;

    string LoggedInPlayfabId;

    string output;

    string coin_string;
    string rubies_string;
    string energy_string;

    string energyRechargeTime_string;
    float energyRechargeTime_float = 1;

    #region FriendList
    public InputField FriendName;
    #endregion

    private void Awake()
    {
        instance = this;
    }
    #region Login-Simple
    //SimpleLogin
    void Login() 
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            //For name on leaderboard
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams 
            {
                GetPlayerProfile = true
            }
            //For name on leaderboard
        };
        PlayFabClientAPI.LoginWithCustomID(request,OnSuccess,OnError);
    }
    #endregion
    #region Login-Register
    public void RegisterButton() 
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = Email,
            Password = Password,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess,OnError);
    }
    void OnRegisterSuccess(RegisterPlayFabUserResult result) 
    {
        Debug.Log("Registerd!");
    }
    public void LoginButton() 
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = Email,
            Password = Password,
            //For name on leaderboard
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
            //For name on leaderboard
        };
        PlayFabClientAPI.LoginWithEmailAddress(request,OnSuccess,OnError);
    }
    public void ResetPasswordButton() 
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = Email,
            TitleId = "5ED99"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request,OnPasswordReset,OnError);
    }
    void OnPasswordReset(SendAccountRecoveryEmailResult result) 
    {
        Debug.Log("Pass reset mail sent!");
    }
    #endregion
    //Logged-in
    void OnSuccess(LoginResult result) 
    {
        LoggedInPlayfabId = result.PlayFabId;
        Debug.Log("Loggedin!");
        string name = null;
        if (result.InfoResultPayload.PlayerProfile != null) 
        {
            name = result.InfoResultPayload.PlayerProfile.DisplayName;
        }
        if (name == null)
        {
            nameWindow.SetActive(true);
        }
        else 
        {
            leaderboardWindow.SetActive(true);
        }
        GetVirtualCurrencies();
    }
    //Error
    void OnError(PlayFabError error) 
    {
        Debug.Log(error.GenerateErrorReport());
    }
    #region Name
    public void SubmitNameButton()
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = playerName
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request,OnDisplayNameUpdate,OnError);
    }
    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result) 
    {
        leaderboardWindow.SetActive(true);
    }
    #endregion
    #region LeaderBoard
    //Send score to LeaderBoard-01 
    public void SendLeaderboard(int score) 
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "LeaderBoard-01",
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request,OnLeaderboardUpdate,OnError);
    }
    //Sent score to LeaderBoard-01 
    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {

    }
    //Get scores from LeaderBoard-01
    public void GetLeaderboard() 
    {
        var request = new GetLeaderboardRequest 
        {
            StatisticName = "LeaderBoard-01",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request,OnLeaderboardGet,OnError);
    }
    //UpdateLeaderBoard on LeaderBoard-01
    void OnLeaderboardGet(GetLeaderboardResult result) 
    {
        /*
        foreach (var item in result.Leaderboard) 
        {
            Debug.Log(item.Position + " " + item.PlayFabId + " " + item.StatValue);
        }
        */
        foreach (Transform item in rowParent) 
        {
            Destroy(item.gameObject);
        }
        foreach (var item in result.Leaderboard) 
        {
            GameObject newGo = Instantiate(rowPrefab,rowParent);
            Text[] texts = newGo.GetComponentsInChildren<Text>();
            texts[0].text = (item.Position + 1).ToString();
            texts[1].text = item.DisplayName;
            texts[2].text = item.StatValue.ToString();

            Debug.Log(string.Format("PLACE: {0} | ID: {1} | VALUE: {2}",
                item.Position,item.PlayFabId,item.StatValue));
        }
    }
    // Leader Board Around Player
    public void GetLeaderboardAroundPlayer() 
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "LeaderBoard-01",
            MaxResultsCount = 9
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request,OnLeaderboardAroundPlayerGet,OnError);
    }
    void OnLeaderboardAroundPlayerGet(GetLeaderboardAroundPlayerResult result) 
    {
        foreach (Transform item in rowParent)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in result.Leaderboard)
        {
            GameObject newGo = Instantiate(rowPrefab, rowParent);
            Text[] texts = newGo.GetComponentsInChildren<Text>();
            texts[0].text = (item.Position + 1).ToString();
            texts[1].text = item.DisplayName;
            texts[2].text = item.StatValue.ToString();

            if (item.PlayFabId == LoggedInPlayfabId) 
            {
                texts[0].color = Color.cyan;
                texts[1].color = Color.cyan;
                texts[2].color = Color.cyan;
            }

            Debug.Log(string.Format("PLACE: {0} | ID: {1} | VALUE: {2}",
                item.Position, item.PlayFabId, item.StatValue));
        }
    }
    #endregion
    #region Data-Simple
    //Get data from server
    public void GetAppearance() 
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),OnDataRecieved,OnError);
    }
    //Get data from server !Comp
    void OnDataRecieved(GetUserDataResult result) 
    {
        if (result.Data != null && result.Data.ContainsKey("A")
            && result.Data.ContainsKey("B") && result.Data.ContainsKey("C"))
        {
            A_Score = int.Parse(result.Data["A"].Value);
            B_Score = float.Parse(result.Data["B"].Value);
            C_Score = (result.Data["C"].Value);
        }
        else 
        {
        }
    }
    //Send data to Server
    public void SaveAppearance() 
    {
        var request = new UpdateUserDataRequest 
        {
            Data = new Dictionary<string, string> 
            {
                {"A", A_Score.ToString() },
                {"B", B_Score.ToString() },
                {"C", C_Score.ToString() }
            }
        };
        PlayFabClientAPI.UpdateUserData(request,OnDataSend,OnError);
    }
    //Send data to server !Comp
    #endregion
    #region Data-Json
    public void SaveJson() 
    {
        List<Data_Json> data_json = new List<Data_Json>();
        foreach (var item in _Data_Json) 
        {
            data_json.Add(item);
        }
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> 
            {
                {"Data-Json",JsonConvert.SerializeObject(data_json)}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend,OnError);
    }
    public void GetJson() 
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJsonDataRecieved,OnError);
    }
    void OnJsonDataRecieved(GetUserDataResult result) 
    {
        if (result.Data != null && result.Data.ContainsKey("Data-Json")) 
        {
            List<Data_Json> data_json = JsonConvert.DeserializeObject<List<Data_Json>>(result.Data["Data-Json"].Value);
            for (int i = 0; i < _Data_Json.Length; i++) 
            {
                _Data_Json[i] = data_json[i];
            }
        }
    }
    #endregion
    #region Currency
    public void GetVirtualCurrencies() 
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),OnGetUserInventorySuccess,OnError);
    }
    void OnGetUserInventorySuccess(GetUserInventoryResult result) 
    {
        int coins = result.VirtualCurrency["CN"];
        coin_string = coins.ToString();

        int rubies = result.VirtualCurrency["RB"];
        rubies_string = rubies.ToString();

        int energy = result.VirtualCurrency["EN"];
        energy_string = energy.ToString();
        energyRechargeTime_float = result.VirtualCurrencyRechargeTimes["EN"].SecondsToRecharge;
    }
    #endregion
    #region Currency-Grant
    public void GrantVirtualCurrency() 
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = "CN",
            Amount = 50
        };
        PlayFabClientAPI.AddUserVirtualCurrency(request,OnGrantVirtualCurrencySuccess,OnError);
    }
    void OnGrantVirtualCurrencySuccess(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log("Currency granted");
    }
    #endregion
    #region CloudScript
    public void ExecuteButton() 
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "hello",
            FunctionParameter = new
            {
                name = LoggedInPlayfabId
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request,OnExecuteSuccess,OnError);
    }
    void OnExecuteSuccess(ExecuteCloudScriptResult result) 
    {
        output = result.FunctionResult.ToString();
    }
    #endregion
    void OnDataSend(UpdateUserDataResult result)
    {
        Debug.Log("Data-Simple Saved!");
    }
    #region TitleData
    //Ask TitleData
    void GetTitleData() 
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnTitleDataRecieved, OnError);
    }
    //Asked TitleData
    void OnTitleDataRecieved(GetTitleDataResult result) 
    {
        if (result.Data == null 
            || result.Data.ContainsKey("Message") == false
            || result.Data.ContainsKey("Multiplier") == false)
        {
            return;
        }
        TitleData_String = result.Data["Message"];
        TitleData_Int = int.Parse(result.Data["Multiplier"]);
    }
    #endregion
    private void Update()
    {
        energyRechargeTime_float -= Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(energyRechargeTime_float);
        energyRechargeTime_string = time.ToString("mm':'ss");
        if (energyRechargeTime_float < 0) 
        {
            GetVirtualCurrencies();
        }
    }
    #region Currency-Buy <EN>
    public void BuyItem()
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = "EN",
            Amount = 5
        };
        PlayFabClientAPI.SubtractUserVirtualCurrency(request, OnSubtractCoinsSuccess, OnError);
    }
    void OnSubtractCoinsSuccess(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log("Bought item! ");
        instance.GetVirtualCurrencies();
    }
    #endregion
    #region FriendList
    // Add Friend
    public void AddFriend() 
    {
        var request = new AddFriendRequest 
        {
            FriendTitleDisplayName = FriendName.text,
            FriendPlayFabId = FriendName.text,
            FriendEmail = FriendName.text,
            FriendUsername = FriendName.text
        };
        PlayFabClientAPI.AddFriend(request,OnAddFriend,OnError);
    }
    void OnAddFriend(AddFriendResult result) 
    {
        Debug.Log("Friend Added!");
    }
    // Remove Friend
    public void RemoveFriend() 
    {
        var request = new RemoveFriendRequest
        {
            FriendPlayFabId = FriendName.text
        };
        PlayFabClientAPI.RemoveFriend(request,OnRemoveFriend,OnError);
    }
    void OnRemoveFriend(RemoveFriendResult result) 
    {
        Debug.Log("Friend removed!");
    }
    // Get Friend List
    public void GetFriendList() 
    {
        var request = new GetFriendsListRequest 
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false
        };
        PlayFabClientAPI.GetFriendsList(request,OnFriendList,OnError);
    }
    void OnFriendList(GetFriendsListResult result) 
    {
        if (result.Friends != null) 
        {
            foreach (var friend in result.Friends)
            {
                Debug.Log(friend.TitleDisplayName);
            }
        }
    }
    #endregion
    #region Data-Simple <other user>
    public void GetAppearance_Other() 
    {
        //PlayFabClientAPI.GetSharedGroupData(new GetUserDataRequest,,OnError);
    }
    #endregion
}
