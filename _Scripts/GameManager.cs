using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using PN = Photon.Pun.PhotonNetwork;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager S;

    public PlayersTop Top;
    public GameObject PlayerPrefabs;
    public Text text;
    public Color color;

    private float time;
    private bool IsTimeUp;
    private bool IsEnd;
    private List<Player> players = new List<Player>();
    private string Name;
    private double birth;
    private GameObject go;

    private void Awake()
    {
        if (S == null)
            S = this;
    }

    void Start()
    {
        Name = PN.CurrentRoom.Name;


        //If is a server, set a timer
        if (PN.IsMasterClient)
        {
            time = (float)PN.CurrentRoom.CustomProperties[Name];
            ExitGames.Client.Photon.Hashtable cust = new ExitGames.Client.Photon.Hashtable();
            cust.Add(Name, time);
            PN.MasterClient.SetCustomProperties(cust);
        }
        //else just get a time
        if (!PN.IsMasterClient)
            time = (float)PN.MasterClient.CustomProperties[Name];

        //Instantiate player
        Vector3 pos = new Vector3(Random.Range(-90f, 90f), Random.Range(-90f, 90f));
        color = LobbyManager.S.color;
        go = PN.Instantiate(PlayerPrefabs.name, pos, Quaternion.identity);
        go.GetComponent<Player>().SetColor(color);

        //This is need to sent your color to another player
        PhotonPeer.RegisterType(typeof(Color), 240, SerializeColorToInt, DeserializeColorToInt);
    }

    private void Update()
    {
        if (PN.NetworkClientState == Photon.Realtime.ClientState.Leaving)
            return;

        //timer and top 5 players
        Top.SetText(players);
        UpdateTimeText(time);

        //room timer
        if (PN.IsMasterClient)
        {
            if (time > 0)
            {
                time -= Time.deltaTime;
                PN.MasterClient.CustomProperties.Remove(0);
                ExitGames.Client.Photon.Hashtable cust = new ExitGames.Client.Photon.Hashtable();
                cust.Add(Name, time);
                PN.MasterClient.SetCustomProperties(cust);
            }
        }

        if (!PN.IsMasterClient)
        {
            if (time > 0.1)
            {
                try
                {
                    time = (float)PN.MasterClient.CustomProperties[Name];
                }
                catch (Exception ex)
                {
                    print(ex);
                    time = 0;
                }
            }
            else
                time = 0;
        }

        if (time <= 0.01 && !IsTimeUp)
        {
            time = 0;
            IsTimeUp = true;
            birth = PN.Time;
            Time.timeScale = 0;
        }
        //take a 3 seconds pause to continue
        if (IsTimeUp && PN.Time > birth + 3f && !IsEnd)
        {
            go.GetComponent<Player>().Restart();
            Time.timeScale = 1;
            IsEnd = true;
        }
    }
    public void AddPlayer(Player player)
    {
        players.Add(player);
    }
    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
    private void UpdateTimeText(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        text.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    //Check if auth key is not in use
    //auth key is needed to pair father and doughter split
    public bool CheckAuth(int auth)
    {
        foreach (var player in players)
        {
            if (player.auth == auth)
                return false;
        }
        return true;
    }

    public void Leave()
    {
        PN.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.LogFormat($"Player {0} entered room", newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.LogFormat($"Player {0} left room", otherPlayer.NickName);
    }

    public static object DeserializeColorToInt(byte[] data)
    {
        Color res = new Color();

        res.r = BitConverter.ToInt32(data, 0);
        res.g = BitConverter.ToInt32(data, 4);
        res.b = BitConverter.ToInt32(data, 8);
        res.a = 1f;

        return res;
    }
    public static byte[] SerializeColorToInt(object obj)
    {
        Color color = (Color)obj;
        byte[] res = new byte[12];

        BitConverter.GetBytes((int)color.r).CopyTo(res, 0);
        BitConverter.GetBytes((int)color.g).CopyTo(res, 4);
        BitConverter.GetBytes((int)color.b).CopyTo(res, 8);

        return res;
    }
}
