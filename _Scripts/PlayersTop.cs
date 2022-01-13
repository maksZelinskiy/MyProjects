using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayersTop : MonoBehaviour
{
    private void Start()
    {
        foreach (var text in GetComponentsInChildren<Text>())
        {
            text.text = "";
        }
    }

    public void SetText(List<Player> players)
    {
        Player[] top = players
            //If you want to see only alive players uncomment next line
            //.Where(p => !p.IsDead)
            .OrderByDescending(p => p.mass)
            .Take(5)
            .ToArray();

        for (int i = 0; i < top.Length; i++)
        {
            transform.GetChild(i).GetComponent<Text>().text =
                (i + 1) + ". " + top[i].photonView.Owner.NickName + "  Score: " + top[i].mass / 10;
        }
    }
}
