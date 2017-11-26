using com.shephertz.app42.paas.sdk.csharp;
using com.shephertz.app42.paas.sdk.csharp.game;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardScore {
    public String   name  = "";
    public int      score = 0;
    public DateTime date;
}

/**
* App42 Cloud API: FREE
* http://api.shephertz.com/pricing.php
* http://api.shephertz.com/app42-dev/unity3d-backend-apis.php
* http://api.shephertz.com/app42-docs/leaderboard-service/?sdk=unity
*/
public static class Leaderboard {

    private static LeaderboardCallback _leaderboardCB = new LeaderboardCallback();
    private static StatsCallback       _statsCB       = new StatsCallback();
    private static String              _leaderboard   = "AnimalBattleLeaderboard";
    private static String              _stats         = "AnimalBattleWinStats";
    private static ServiceAPI          _serviceAPI    = new ServiceAPI("8038413da68b9f1677e5672775ae1ec294c8986213e34ae158b75b752ea295f7", "86d5c35b8e5fa2ce30475a22462587bca54bb5156ebfd7a3ac5193bfb779913c");
    private static ScoreBoardService   _scoreBoard    = Leaderboard._serviceAPI.BuildScoreBoardService();

    public static void SaveScore(string user, double score) {
        if (Leaderboard._scoreBoard != null)
            Leaderboard._scoreBoard.SaveUserScore(Leaderboard._leaderboard, user, score, Leaderboard._leaderboardCB);
    }

    public static void GetLeaderboard(int max) {
        if (Leaderboard._scoreBoard != null)
            Leaderboard._scoreBoard.GetTopNRankings(Leaderboard._leaderboard, max, Leaderboard._leaderboardCB);
    }

    public static void SaveStats(string animal) {
        if (Leaderboard._scoreBoard != null)
            Leaderboard._scoreBoard.SaveUserScore(Leaderboard._stats, animal, 1.0, Leaderboard._statsCB);
    }

    public static void GetStats(string animal) {
        if (Leaderboard._scoreBoard != null)
            Leaderboard._scoreBoard.GetScoresByUser(Leaderboard._stats, animal, Leaderboard._statsCB);
    }
}

public class LeaderboardCallback : App42CallBack {

    public void OnException(Exception e) {
        this.updateHUD(new List<LeaderboardScore>());
    }

    public void OnSuccess(object obj) {
        if ((obj == null) || !(obj is Game)) {
            this.OnException(null);
            return;
        }

        Game              game   = (Game)obj;
        IList<Game.Score> scores = game.GetScoreList();

        if (scores == null) {
            this.OnException(null);
            return;
        }

        List<LeaderboardScore> scoreList = new List<LeaderboardScore>();

        foreach (var score in scores) {
            LeaderboardScore score2 = new LeaderboardScore();

            score2.name  = score.GetUserName();
            score2.score = (int)score.GetValue();
            score2.date  = score.GetCreatedOn();

            scoreList.Add(score2);
        }

        this.updateHUD(scoreList);
    }

    private void updateHUD(List<LeaderboardScore> scoreList) {
        GameObject lobbyCanvas = GameObject.Find("LobbyCanvas");
        LobbyHUD   lobbyHUD    = null;

        if (lobbyCanvas != null)
            lobbyHUD = lobbyCanvas.GetComponent<LobbyHUD>();

        if (lobbyHUD != null)
            lobbyHUD.UpdateLeaderboard(scoreList);
    }
}

public class StatsCallback : App42CallBack {

    public void OnException(Exception e) {
        //
    }

    public void OnSuccess(object obj) {
        if ((obj == null) || !(obj is Game))
            return;

        Game              game   = (Game)obj;
        IList<Game.Score> scores = game.GetScoreList();
        int               count  = 0;

        if (scores == null)
            return;

        foreach (var score in scores)
            count++;

        if (count > 0)
            Debug.Log("LeaderboardScore::STATS: " + scores[0].GetUserName() + "=" + count);
    }
}
