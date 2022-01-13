using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager S;
    public static int roomID;

    public InputField NicknameInput;
    public Image input;

    public Color color = new Color(0f, 0f, 0f);

    private void Awake()
    {
        if (S == null)
            S = this;
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
        //idk why its show a mistake
        if (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.ConnectingToMasterServer)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
    }

    public void CreateRoom()
    {
        //Check for valid nickname
        if (NicknameInput.text.Length < 3)
        {
            input.color = Color.red;
            return;
        }
        //Check if color is selected
        if (color == new Color(0f, 0f, 0f, 0f))
            return;

        PhotonNetwork.NickName = NicknameInput.text;
        roomID++;
        ExitGames.Client.Photon.Hashtable custom = new ExitGames.Client.Photon.Hashtable();
        //Add timer to each room
        custom.Add(roomID.ToString(), 300f);

        PhotonNetwork.CreateRoom(roomID.ToString(), new Photon.Realtime.RoomOptions { MaxPlayers = 10, CleanupCacheOnLeave = true, CustomRoomProperties = custom });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print(message);
    }

    public void JoinRoom()
    {
        //Same as Create Room
        if (NicknameInput.text.Length < 3)
        {
            input.color = Color.red;
            return;
        }
        if (color == new Color(0f, 0f, 0f, 0f))
            return;

        PhotonNetwork.NickName = NicknameInput.text;
        PhotonNetwork.JoinRandomRoom();
    }

    //Call back
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined the room");
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        Debug.Log("Please try again");
    }
    //Check valid nickName
    public void OnValueChanged(string value)
    {
        if (NicknameInput.text.Length < 3)
            input.color = Color.red;
        if (NicknameInput.text.Length >= 3)
            input.color = Color.white;
    }
}
