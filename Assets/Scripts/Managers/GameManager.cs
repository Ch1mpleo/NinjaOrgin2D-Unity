using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Player player;

    public Player Player => player;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.ResetPlayer();
        }
    }

    public void AddPlayerExp(float expAmount)
    {
        PlayerExp playerExp = player.GetComponent<PlayerExp>();
        playerExp.AddExp(expAmount);
    }
}