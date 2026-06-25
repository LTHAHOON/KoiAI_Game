using UnityEngine;

public enum GameTagName
{
    Player,
    Monster,
    Ground,
}

public static class GameTags 
{
    public const string Player = "Player";
    public const string Monster = "Monster";
    public const string Ground = "Ground";

    public static string GetGameTag(GameTagName gameTagName)
    {
        return gameTagName switch
        {
            GameTagName.Player => Player,
            GameTagName.Monster => Monster,
            GameTagName.Ground => Ground,
            _ => "",
        };
    }
}

