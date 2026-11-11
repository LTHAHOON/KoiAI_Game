namespace KoiAI.Utilities
{
    public enum GameTagName
    {
        Player,
        Monster,
        Ground,
        Ditherable,
    }

    public static class GameTags 
    {
        public const string Player = "Player";
        public const string Monster = "Monster";
        public const string Ground = "Ground";
        public const string Ditherable = "Ditherable";

        public static string GetGameTag(GameTagName gameTagName)
        {
            return gameTagName switch
            {
                GameTagName.Player => Player,
                GameTagName.Monster => Monster,
                GameTagName.Ground => Ground,
                GameTagName.Ditherable => Ditherable,
                _ => "",
            };
        }
    }
}