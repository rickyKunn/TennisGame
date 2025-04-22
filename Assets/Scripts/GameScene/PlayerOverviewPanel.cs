using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class PlayerOverviewPanel : MonoBehaviour
{
    

    [SerializeField] private TextMeshProUGUI scoreText = null;

    private ServiceManager servicemanager;
    private bool deuce;
    private BallMove ballmove;
    private Dictionary<PlayerRef, TextMeshProUGUI>
        _playerListEntries = new Dictionary<PlayerRef, TextMeshProUGUI>();

    private TextMeshProUGUI newScores;

    public string nick1,nick2;
    private Dictionary<PlayerRef, int> _playerScores = new Dictionary<PlayerRef, int>();
    private Dictionary<PlayerRef, int> _playerScore1 = new Dictionary<PlayerRef, int>();
    private Dictionary<PlayerRef, int> _playerScore2 = new Dictionary<PlayerRef, int>();

    // Creates a new Overview Entry
    private void Start()
    {
        servicemanager = GameObject.Find("service").GetComponent<ServiceManager>();
        var playerdata = FindObjectOfType<PlayerData>();
        nick1 = playerdata.name1;
        nick2 = playerdata.name2;
    }

    private void Update()
    {
    }

    public void AddEntry(PlayerRef playerRef, PlayerNetworkedChange playerDataNetworked)
    {
        if (_playerListEntries.ContainsKey(playerRef)) return;
        if (playerDataNetworked == null) return;
        var ScoresEntry = Instantiate(scoreText,this.transform);
        print("a");
        //var P1Entry = Instantiate(P1Text,this.transform);
        //var P2Entry = Instantiate(P2Text,this.transform);
        ScoresEntry.transform.localScale = Vector3.one;
        //P1Entry.transform.localScale = Vector3.one;
        //P2Entry.transform.localScale = Vector3.one;

        string nickName = String.Empty;

        int score = 0;
        _playerScore1.Add(playerRef, score);
        _playerScore2.Add(playerRef,score);
        //newP1 = P1Entry;
        //newP2 = P2Entry;
        newScores = ScoresEntry;
        UpdateScore(playerRef, 0,"0");
    }

    public void RemoveEntry(PlayerRef playerRef)
    {
        if (newScores != null)
        {
            Destroy(newScores.gameObject);
            //Destroy(newP1.gameObject);
            //Destroy(newP2.gameObject);

        }

        _playerScore1.Remove(playerRef);
        _playerScore2.Remove(playerRef);
        _playerListEntries.Remove(playerRef);
    }

    public void UpdateScore(PlayerRef player, int score1,int score2)
    {
        //if (_playerListEntries.TryGetValue(player, out var entry) == false) return;

        _playerScore1[player] = score1;
        _playerScore2[player] = score2;
        UpdateScore(player,0, "0" );
    }

    private void UpdateScore(PlayerRef player, int  id, String name)
    {
        GameObject ball = GameObject.Find("Ball");
        if (ball == true)
        {
            ballmove = ball.GetComponent<BallMove>();
        }
        
        var score1 = _playerScore1[player];
        var score2 = _playerScore2[player];
        string adPlayer = null;
        int countDif = 0;
        bool gameSet = false;
        bool deuce = false;
        if (score1 != 0 || score2 != 0)
        {
            ballmove.rec_scoreChanged = true;

            if (score1 >= 3 && score2 >= 3)
            {
                ballmove.deuce = true;
                deuce = true;

            }

            if (deuce == true) //デュースになった時
            {
                if(score1 > score2)
                {
                    adPlayer = nick1;
                    countDif = score1 - score2;
                    newScores.text = $"Advantage :{nick1}";

                }
                else if(score2 > score1)
                {
                    adPlayer = nick2;
                    countDif = score2 - score1;
                    newScores.text = $"Advantage :{nick2}";
                }
                else //デュース
                {
                    adPlayer = "";
                    countDif = 0;
                    newScores.text = "Deuce";
                }
                if (countDif == 2)
                {
                    gameSet = true;
                }
            }
            if (score1 == 4 || score2 == 4 ) //デュースじゃない時
            {
                if (deuce == false) gameSet = true;
            }

            if (gameSet == true)
            {
                newScores.text = "Game Set!";
                servicemanager.serve_changed = true;
                servicemanager.ServiceChange();
                print("changed!!");
                score1 = 0;
                score2 = 0;
            }
            ballmove.res_p1Score = score1;
            ballmove.res_p2Score = score2;
        }

        if (nick2 == "")
        {
            GameObject.Find("wall1").GetComponent<BoxCollider>().enabled = true;
            nick2 = "Wall";
        }
        if (deuce == false && gameSet == false) newScores.text = $"{nick1}:{score1} - {score2}:{nick2}";
    }
}
