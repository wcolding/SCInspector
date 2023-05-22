using SCInspector.Unreal;

namespace SCInspector.Conviction
{
    public static class Data
    {
        public static Dictionary<string, GameInfo> Targets = new Dictionary<string, GameInfo>()
        {
            {
                "Conviction (Steam)",
                new GameInfo()
                {
                    game = Game.ConvictionSteam,
                    windowName = "Conviction",
                    moduleName = "conviction_game.exe",
                    gNamesOffset = 0x101907C,
                    gObjectsOffset = 0x1008FC8
                }
            },
            {
                "Conviction (Ubisoft)",
                new GameInfo()
                {
                    game = Game.ConvictionUbi,
                    windowName = "Conviction",
                    moduleName = "Conviction_game.exe", // capital C for ubi
                    gNamesOffset = 0x101913C,
                    gObjectsOffset = 0x1009088
                }
            }
        };

        public static readonly GameOffsets Offsets = new GameOffsets()
        {
            String = 0x08,
            InternalIndex = 0x04,
            LinkerLoad = 0x10,
            Outer = 0x18,
            FName = 0x20,
            Class = 0x24,
            SuperField = 0x28,
            Size = 0x3A,
            PropertyOffset = 0x3E,
            DName = 0xE8,
            Bitmask = 0x9C,
            Struct = 0x9C
        };
    }

    public class SC5GameData : GameData
    {
        public SC5GameData(GameInfo _info) : base(_info) { }

        public override void RefreshObjects()
        {
            Offsets = Data.Offsets;
            LEADEngine = true;
            base.RefreshObjects();
        }
    }
}
