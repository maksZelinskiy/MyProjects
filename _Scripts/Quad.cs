using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad : MonoBehaviour
{
    public List<Quad> quadsAround;
    public SpawnFood sf;
    public int i;
    public int j;
    private int _i = 3;
    private int _j = 3;

    private void Awake()
    {
        sf = transform.parent.GetComponent<SpawnFood>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player"))
        {
            collision.GetComponent<Player>().currQuad = this;
        }
    }
    public List<Quad> GetQuads()
    {
        quadsAround = new List<Quad>();

        if (i == 0)
            _i = 2;
        if (i != 0)
            _i = 3;
        if (j == 0)
            _j = 2;
        if (j != 0)
            _j = 3;

        for (int g = i + 2 - _i; g < i + 2 && g < 10; g++)
        {
            for (int r = j + 2 - _j; r < j + 2 && r < 10; r++)
            {
                quadsAround.Add(sf.posQuad[g, r]);
            }
        }

        return quadsAround;
    }
}
