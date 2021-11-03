using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{
    public GameObject Btn_Start;
    public GameObject Btn_Claim;
    public GameObject PlayerNameInput;
    public GameObject ScoreView;

    public static GameManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null) throw new Exception("Only one GameManager allowed");
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // trigger leaderboard update on game start.
        LeaderboardController.Instance.BtnSubmit_OnClick();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Btn_Start_OnClick()
    {
        var playerName = PlayerNameInput.GetComponent<TMP_InputField>().text;
        if (string.IsNullOrWhiteSpace(playerName) || playerName.Length < 3) return;

        Btn_Start.SetActive(false);
        PlayerNameInput.SetActive(false);
        Btn_Claim.SetActive(true);
        ScoreView.SetActive(true);
    }

    public void Btn_Claim_OnClick()
    {
        Btn_Start.SetActive(true);
        Btn_Claim.SetActive(false);
        PlayerNameInput.SetActive(true);
        ScoreView.SetActive(false);
        LeaderboardController.Instance.BtnSubmit_OnClick();
        PlayerNameInput.GetComponent<TMP_InputField>().text = "";
    }
}
