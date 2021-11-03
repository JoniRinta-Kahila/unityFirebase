using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardController : MonoBehaviour
{
    [Header("Player score")]
    [SerializeField] private TextMeshProUGUI PlayerScore;

    [Header("Prefabs")]
    [SerializeField] private GameObject ScoreRowPrefab;

    [Header("Scoreboards")]
    [SerializeField] private GameObject TodayScores;
    [SerializeField] private GameObject AllTimeScores;

    //[Header("Activityindicators")]
    //[SerializeField] private GameObject WaitAnimationAllTime;
    //[SerializeField] private GameObject WaitAnimationToday;

    [Header("Endpoint Settings")]
    [SerializeField] private string ApplyScoreEndpoint;
    [SerializeField] private string TodaysBestEndpoint;
    [SerializeField] private string AllTimeBestEndpoint;


    public static LeaderboardController Instance { get; set; }

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null) throw new Exception("Only one LeaderboardController allowed");
        Instance = this;
    }

    // Update is called once per frame
    void Start()
    {
        if (!ScoreRowPrefab) throw new Exception("ScoreRowPrefab is missing");
        if (!TodayScores) throw new Exception("Scoreboard TodayScores is missing");
        if (!AllTimeScores) throw new Exception("Scoreboard AllTimeScores is missing");

        if (string.IsNullOrWhiteSpace(ApplyScoreEndpoint)) throw new Exception("ApplyScoreEndpoint is null or whiteSpace");
        if (string.IsNullOrWhiteSpace(TodaysBestEndpoint)) throw new Exception("TodaysBestEndpoint is null or whiteSpace");
        if (string.IsNullOrWhiteSpace(AllTimeBestEndpoint)) throw new Exception("AllTimeBestEndpoint is null or whiteSpace");

        Debug.Log("LeaderboardController running...");

        UpdateList();
    }

    private void UpdateList()
    {
        if (AllTimeScores != null)
        {
            Debug.Log("Updating All time scoreboards");
            StartCoroutine(GetAllTimeHigh());
        }

        if (TodayScores != null)
        {
            Debug.Log("Updating Today scoreboards");
            StartCoroutine(GetTodayHigh());
        }
    }

    public void BtnSubmit_OnClick()
    {
        StartCoroutine(ApplyScore());
    }

    private void InstantiateScoreRows(List<ScoreData> response, GameObject scoreboard)
    {
        foreach (var t in response)
        {
            var scoreRowPrefab = Instantiate(ScoreRowPrefab, scoreboard.transform);

            var prefTexts = scoreRowPrefab.GetComponentsInChildren<TextMeshProUGUI>();
            var nameField = prefTexts.FirstOrDefault(x => x.CompareTag("ScoresName"));
            var scoreField = prefTexts.FirstOrDefault(x => x.CompareTag("ScoresScore"));

            if (nameField != null) nameField.text = t.Name;
            if (scoreField != null) scoreField.text = t.Score.ToString();
        }
    }

    private IEnumerator ApplyScore()
    { 
        if (string.IsNullOrWhiteSpace(PlayerScore.text) ||  // if player have no score OR
            string.IsNullOrWhiteSpace(GameManager.Instance.PlayerNameInput.GetComponent<TMP_InputField>().text)) // player have no name, BREAK!
            yield break;

        var playerName = GameManager.Instance.PlayerNameInput.GetComponent<TMP_InputField>().text;
        var uriData = Uri.EscapeUriString($"?name={playerName}&score={PlayerScore.text}"); // generate data string.

        foreach (Transform child in AllTimeScores.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in TodayScores.transform)
        {
            Destroy(child.gameObject);
        }

        // this is good place to trigger an activityindicator animation to play.

        using (var webRequest = UnityWebRequest.Get(ApplyScoreEndpoint + uriData))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode == 200)
            {
                Debug.Log("ApplyScore success");
                UpdateList();
            } else
            {
                Debug.LogError($"Cannot apply score, Firebase respond with code {webRequest.responseCode}");
            }
        }

    }

    private IEnumerator GetAllTimeHigh()
    {
        using (var webRequest = UnityWebRequest.Get(AllTimeBestEndpoint))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode == 200)
            {
                var res = JsonConvert.DeserializeObject<List<ScoreData>>(webRequest.downloadHandler.text);
                InstantiateScoreRows(res, AllTimeScores);
            }
        }
    }

    private IEnumerator GetTodayHigh()
    {
        using (var webRequest = UnityWebRequest.Get(TodaysBestEndpoint))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode == 200)
            {
                var res = JsonConvert.DeserializeObject<List<ScoreData>>(webRequest.downloadHandler.text);

                // TODO for reason x, firebase doesn't sort the list of todays scores..
                var sortedList = res.OrderBy(x => x.Score).Reverse().ToList<ScoreData>();
                InstantiateScoreRows(sortedList, TodayScores);
            }
        }
    }
}

