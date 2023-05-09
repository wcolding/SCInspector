namespace SCInspector
{
    public enum Game
    {
        SplinterCell,
        PandoraTomorrow,
        ChaosTheory,
        DoubleAgent,
        ConvictionSteam,
        ConvictionUbi,
        Blacklist,
        BlacklistDX11
    }

    public struct GameEntry
    {
        public Game game;
        public string displayName;
        public string windowName;
        public string moduleName;
        public IntPtr gNamesOffset;
        public IntPtr gObjectsOffset;
    }

    public static class Games
    { 
        public static GameEntry[] GameInfo = new GameEntry[4]
        {
            new GameEntry() 
            { 
                game = Game.SplinterCell,
                displayName = "Splinter Cell",
                windowName = "Tom Clancy's Splinter Cell",
                moduleName = "Core.dll",
                gNamesOffset = (IntPtr)0x18DD7C,
                gObjectsOffset = (IntPtr)0x19518C
            },

            new GameEntry()
            {
                game = Game.PandoraTomorrow,
                displayName = "Pandora Tomorrow",
                windowName = "Tom Clancy's Splinter Cell: Pandora Tomorrow",
                moduleName = "Core.dll",
                gNamesOffset = (IntPtr)0x1A9384,
                gObjectsOffset = (IntPtr)0x1AE0EC
            },

            new GameEntry() 
            { 
                game = Game.ChaosTheory,
                displayName = "Chaos Theory",
                windowName = "Tom Clancy's Splinter Cell Chaos Theory",
                moduleName = "splintercell3.exe",
                gNamesOffset = (IntPtr)0xA0DFC0,
                gObjectsOffset = (IntPtr)0xA12084
            },

            new GameEntry()
            {
                game = Game.DoubleAgent,
                displayName = "Double Agent v1",
                windowName = "Tom Clancy's SplinterCell 4",
                moduleName = "Core.dll",
                gNamesOffset = (IntPtr)0x2D3A24,
                gObjectsOffset = (IntPtr)0X2D7AC4
            },

            //new GameEntry()
            //{
            //    game = Game.ConvictionSteam,
            //    displayName = "Conviction (Steam)",
            //    windowName = "Conviction",
            //    moduleName = "conviction_game.exe",
            //    gNamesOffset = (IntPtr)0x101907C,
            //    gObjectsOffset = (IntPtr)0x1008FC8 // no
            //},

            //new GameEntry()
            //{
            //    game = Game.ConvictionUbi,
            //    displayName = "Conviction (Ubisoft)",
            //    windowName = "Conviction",
            //    moduleName = "Conviction_game.exe", // capital C for ubi
            //    gNamesOffset = (IntPtr)0x101913C,
            //    gObjectsOffset = (IntPtr)0x1009088
            //},

            //new GameEntry() 
            //{ 
            //    game = Game.Blacklist,
            //    displayName = "Blacklist",
            //    windowName = "Blacklist_game.exe",
            //    moduleName = "Blacklist_game.exe",
            //    gNamesOffset = IntPtr.Zero,
            //    gObjectsOffset = IntPtr.Zero 
            //},

            //new GameEntry() 
            //{ 
            //    game = Game.BlacklistDX11,
            //    displayName = "Blacklist (DX11)",
            //    windowName = "Blacklist_DX11_game.exe",
            //    moduleName = "Blacklist_DX11_game.exe",
            //    gNamesOffset = IntPtr.Zero,
            //    gObjectsOffset = IntPtr.Zero
            //}
        };
    }
}
