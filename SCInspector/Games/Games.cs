namespace SCInspector
{
    using Target = KeyValuePair<string, GameInfo>;
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

    public struct GameInfo
    {
        public Game game;
        public string windowName;
        public string moduleName;
        public IntPtr gNamesOffset;
        public IntPtr gObjectsOffset;
    }

    public static class Games
    { 
        public static Dictionary<string, GameInfo> GetTargets()
        {
            Dictionary<string, GameInfo> targets = new Dictionary<string, GameInfo>();
            foreach (Target t in SplinterCell.Data.Targets) { targets.Add(t.Key, t.Value); };
            foreach (Target t in PandoraTomorrow.Data.Targets) { targets.Add(t.Key, t.Value); };
            foreach (Target t in ChaosTheory.Data.Targets) { targets.Add(t.Key, t.Value); };
            foreach (Target t in DoubleAgent.Data.Targets) { targets.Add(t.Key, t.Value); };
            foreach (Target t in Conviction.Data.Targets) { targets.Add(t.Key, t.Value); };
            return targets;
        }
    }
}
