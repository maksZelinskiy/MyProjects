using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpawnFood : MonoBehaviour
{
    [Header("Set In Inspector")]
    public GameObject food;
    public GameObject quad;
    public Transform parent;
    public Quad[,] posQuad = new Quad[10, 10];

    private Vector2 randomPos;
    private readonly int MaxFood = 5000;

    private void Awake()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameObject go = Instantiate(quad, new Vector2(-90f + j * 20, -90f + i * 20), Quaternion.identity, parent);
                go.name = i + " " + j;
                posQuad[i, j] = go.GetComponent<Quad>();
                posQuad[i, j].i = i;
                posQuad[i, j].j = j;
            }
        }
    }

    //food spawn once per room, cause only one GameObject has its script
    private void Start()
    {
        for (int i = 0; i < MaxFood; i++)
        {
            randomPos.Set(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            Instantiate(food, randomPos, Quaternion.identity, parent);
        }
    }
}
