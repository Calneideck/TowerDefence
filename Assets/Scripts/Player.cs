public static class Player
{
    private static int gold = 200;
    private static int lives = 30;

    public static int Gold
    {
        get { return gold; }
        set { gold = value; }
    }

    public static int Lives
    {
        get { return lives; }
        set { lives = value; }
    }
}
