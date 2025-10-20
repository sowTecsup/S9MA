using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Lobbies;
using Sirenix.OdinInspector;

using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System.Collections.Generic;
using System;

public class TestLobby : MonoBehaviour
{
    public Lobby HostLobby;
    public Lobby JoinedLobby;

    private float heartBeatTimer;
    private float lobbyUpdateTimer;
    private async void Start()
    {
        //await UnityServices.InitializeAsync();
    }
    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
    }
    public async void HandleLobbyHeartBeat()
    {
        if (HostLobby != null)
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0)
            {
                float heartbeatTimerMax = 10;
                heartBeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
                Debug.Log("HeartBeat");
            }
        }
    }
    public async void HandleLobbyPollForUpdates()
    {
        if (JoinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0)
            {
                float LobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = LobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                JoinedLobby = lobby;
            }
        }
    }
    
    [Button]
    public async void CreateLobby(string lobbyName, int maxPlayers =4, bool isPrivate = false, string gameMode = "CTF", string map = "Peru")
    {
        try
        {
            //Player player = await GetPlayer();
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                IsPrivate = isPrivate,

                Player = await GetPlayer(),

                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public,gameMode) },
                    {"Map", new DataObject(DataObject.VisibilityOptions.Public,map) },
                }

            };


            Lobby lobby =  await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers , options);

            HostLobby = lobby;
            JoinedLobby = HostLobby;
            Debug.Log("Partida creada" + lobby.Name + "Max player : " + lobby.MaxPlayers + "Joincode: " + lobby.LobbyCode);


        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        } 
    }
    [Button]
    public async void ListLobbies()
    {

        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            Debug.Log("Se encontraron los lobbies: " + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Lobby Name: " + lobby.Name + "Max player : " + lobby.MaxPlayers + "Joincode: " + lobby.LobbyCode);

            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }


    }
    [Button]
    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            //Player player = await GetPlayer();
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions()
            {
                Player = await GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            JoinedLobby = lobby;
            Debug.Log("Te uniste al lobby!!!   " + lobby.Name);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    [Button]
    public void PrintPlayer()
    {
        PrintPlayers(JoinedLobby);
    }

    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Lobby Name: " + lobby.Name + "Max player : " + lobby.MaxPlayers + "Joincode: ");
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }
    [Button]
    public async Task<Player> GetPlayer()
    {
        string nickName = await AuthenticationService.Instance.GetPlayerNameAsync();

        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
        {
            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nickName) }
        }
        };
    }
   /* [Button]
    public async Task<String> GetPlayerName()
    {
        try
        {
            string nickName = await AuthenticationService.Instance.GetPlayerNameAsync();
            Debug.Log(nickName);
            return nickName;
        }
        catch (AuthenticationException ex)
        {
            Debug.Log("Testo" + ex);
        }
        return "";
    }*/
  
}
