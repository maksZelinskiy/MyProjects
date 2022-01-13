using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour, IPunObservable
{
    [Header("Set In Inspector")]
    public GameObject split;
    public Camera cam;
    public TextMeshPro NicknameText;

    [Header("Set Dynamicly")]
    public Transform parent;
    public PlayersTop Top;
    public PhotonView photonView;
    public Quad currQuad;
    public int auth = 0;
    public float mass = 10;
    public bool IsSplited = false;
    public bool IsDead;
    public int impulse = 11;

    private SpriteRenderer spriteRenderer;
    private Vector2 mousePosition;
    private Vector2 mousePositionForSplit;
    private Vector2 randVec;
    private Vector2 parentPos;
    private Vector3 vecScale;
    private Vector2 setVector2;
    private Vector2 setScale;
    private Color color;
    private float quotient;
    private float delta;
    private float lastSplit;
    private float splitDuration = 10f;
    private float camSize = 8f;
    private float massCoin = 10f;
    private float keepIn = 100f;
    private bool GoToParent = false;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position.x);
            stream.SendNext(transform.position.y);

            stream.SendNext(transform.localScale.x);
            stream.SendNext(transform.localScale.y);

            stream.SendNext(spriteRenderer.color.r);
            stream.SendNext(spriteRenderer.color.g);
            stream.SendNext(spriteRenderer.color.b);
        }
        if (stream.IsReading)
        {
            float x = (float)stream.ReceiveNext();
            float y = (float)stream.ReceiveNext();
            setVector2 = new Vector2(x, y);

            float _x = (float)stream.ReceiveNext();
            float _y = (float)stream.ReceiveNext();
            setScale = new Vector2(_x, _y);

            float r = (float)stream.ReceiveNext();
            float g = (float)stream.ReceiveNext();
            float b = (float)stream.ReceiveNext();
            color = new Color(r, g, b, 1f);
        }
    }

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (IsSplited)
            return;

        NicknameText.SetText(photonView.Owner.NickName);
        FindObjectOfType<GameManager>().AddPlayer(this);

        if (!photonView.IsMine)
        {
            //turn off enemy camera
            photonView.transform.GetChild(0).gameObject.SetActive(false);
            NicknameText.color = Color.green;
        }
    }

    private void Update()
    {
        if (IsDead)
            return;

        if (photonView.IsMine)
        {
            //If player wants to split
            if (Input.GetKeyDown(KeyCode.Space) && !IsSplited && PhotonNetwork.Time > lastSplit + splitDuration)
            {
                lastSplit = (float)PhotonNetwork.Time;
                transform.localScale /= 2;
                mass /= 2;

                GameObject go = PhotonNetwork.Instantiate(split.name, this.transform.position, Quaternion.identity);
                go.transform.localScale = transform.localScale;
                go.GetComponent<Player>().parent = transform;
                go.GetComponent<Player>().NicknameText.SetText(NicknameText.text);
                go.GetComponent<Player>().IsSplited = true;
                go.GetComponent<Player>().mass = mass;
                go.GetComponent<SpriteRenderer>().color = spriteRenderer.color;

                //Instantiate impulse
                mousePositionForSplit = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePositionForSplit -= (Vector2)transform.position;
                go.GetComponent<Rigidbody2D>().AddForce(mousePositionForSplit.normalized * impulse, ForceMode2D.Impulse);

                //check if alredy exist a key
                int authCheck = Random.Range(2, 2347592);
                while (!GameManager.S.CheckAuth(authCheck))
                    authCheck = Random.Range(2, 2347592);
                auth = authCheck;
                go.GetComponent<Player>().auth = auth;
            }

            KeepInGame();

            //get scale of circle including mass
            vecScale.Set((mass / 200 + 0.95f), (mass / 200 + 0.95f), 1f);
            transform.localScale = vecScale;

            //if it is a player part
            if (IsSplited)
            {
                //this is for move to parent
                if (GoToParent)
                {
                    parentPos = parent.position;
                    parentPos -= (Vector2)transform.position;
                    float q = Mathf.Sqrt(parentPos.x * parentPos.x + parentPos.y * parentPos.y) / 8;
                    parentPos /= q;
                    transform.Translate(parentPos * Time.deltaTime);
                    return;
                }

                //Check if splited part is too far
                if (Vector2.Distance(transform.position, parent.position) >= 8)
                {
                    //to stop impule
                    GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                    GoToParent = true;
                }
                return;
            }

            //if it is real player - move by mouse pos
            delta = 8 * Mathf.Pow(20, -Mathf.Log(2, 0.1f)) * Mathf.Pow(mass, Mathf.Log(2, 0.1f));
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition -= (Vector2)transform.position;
            quotient = Mathf.Sqrt(mousePosition.x * mousePosition.x + mousePosition.y * mousePosition.y) / delta;
            mousePosition /= quotient;
            transform.Translate(mousePosition * Time.deltaTime);

            //if player is big get camera further
            if (cam.orthographicSize > camSize)
            {
                if (cam.orthographicSize - 1 > camSize)
                    cam.orthographicSize = camSize;
                else
                    cam.orthographicSize -= 0.0001f;
            }
            else if (cam.orthographicSize < camSize)
            {
                if (cam.orthographicSize + 1 < camSize)
                    cam.orthographicSize = camSize;
                else
                    cam.orthographicSize += 0.0001f;
            }
        }

        //Get enemy scale, pos and color
        if (!photonView.IsMine)
        {
            if (photonView.GetComponent<Player>().currQuad != null)
                //if they are around
                if (photonView.GetComponent<Player>().currQuad.GetQuads().Contains(this.currQuad))
                {
                    print("curr" + currQuad.name);

                    foreach (var item in photonView.GetComponent<Player>().currQuad.GetQuads())
                    {
                        print(item.name);
                    }
                    transform.position = setVector2;
                    transform.localScale = setScale;
                    spriteRenderer.color = color;
                }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //check if leaving is too long
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
            return;

        //Eat the food
        if (collision.gameObject.CompareTag("food"))
        {
            mass += massCoin;
            //get random pos of next food
            randVec.Set(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            collision.gameObject.transform.position = randVec;
            camSize += 0.002f * massCoin;
        }

        //if you bigger than enemy
        if (collision.gameObject.CompareTag("player"))
        {
            if (mass > collision.GetComponent<Player>().mass)
            {
                Vector3 pos = transform.position;
                pos.z = 2;
                transform.position = pos;
            }
            else
            {
                Vector3 pos = transform.position;
                pos.z = 0;
                transform.position = pos;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
            return;

        if (photonView.IsMine)
        {
            if (collision.gameObject.CompareTag("player"))
            {
                //Get reference for player
                Player newPlayer = collision.gameObject.GetComponent<Player>();
                //authenfication for parent and children
                if (auth == newPlayer.auth && !newPlayer.GoToParent)
                    if (auth != 0 && newPlayer.auth != 0)
                        return;

                if (IsDead || newPlayer.IsDead)
                    return;

                //If it is a part
                if (newPlayer.IsSplited && newPlayer.GoToParent)
                {
                    if (auth == newPlayer.auth)
                    {
                        if (Vector2.Distance(newPlayer.transform.position, transform.position) <= 2)
                        {
                            mass += newPlayer.mass;
                            for (int i = 0; i < newPlayer.mass / massCoin + 1; i++)
                                camSize += 0.002f * massCoin;

                            newPlayer.Leave(newPlayer.GetComponent<PhotonView>());
                            auth = 0;
                        }
                        return;
                    }
                }

                //If player bigger than enemy
                if (mass > newPlayer.mass)
                {
                    if (Vector2.Distance(transform.position, newPlayer.transform.position) <= transform.localScale.x)
                    {
                        mass += newPlayer.mass;
                        for (int i = 0; i < newPlayer.mass / massCoin + 1; i++)
                            camSize += 0.002f * massCoin;
                        newPlayer.IsDead = true;
                    }
                }
                //else die
                if (mass < newPlayer.mass)
                {
                    if (Vector2.Distance(transform.position, newPlayer.transform.position) <= newPlayer.transform.localScale.x)
                    {
                        IsDead = true;
                        Leave();
                    }
                }
            }
        }
    }
    //Called from Game Manager to set player color
    public void SetColor(Color color)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }
    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }
    //If Splited
    public void Leave(PhotonView photonView)
    {
        PhotonNetwork.Destroy(photonView);
        IsDead = true;
    }
    //To keep player in map
    void KeepInGame()
    {
        if (transform.position.x >= keepIn)
            transform.position = new Vector3(keepIn, transform.position.y, -1f);
        if (transform.position.x <= -keepIn)
            transform.position = new Vector3(-keepIn, transform.position.y, -1f);
        if (transform.position.y >= keepIn)
            transform.position = new Vector3(transform.position.x, keepIn, -1f);
        if (transform.position.y <= -keepIn)
            transform.position = new Vector3(transform.position.x, -keepIn, -1f);
    }

}
