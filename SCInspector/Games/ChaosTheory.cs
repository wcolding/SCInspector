using SCInspector.Unreal;

namespace SCInspector.ChaosTheory
{
    public static class Data
    {
        public static Dictionary<string, GameInfo> Targets = new Dictionary<string, GameInfo>()
        {
            {
                "Chaos Theory",
                new GameInfo()
                {
                    game = Game.ChaosTheory,
                    windowName = "Tom Clancy's Splinter Cell Chaos Theory",
                    moduleName = "splintercell3.exe",
                    gNamesOffset = 0xA0DFC0,
                    gObjectsOffset = 0xA12084
                }
            }
        };

        public static readonly GameOffsets Offsets = new GameOffsets()
        {
            String = 0x0C,
            InternalIndex = 0x04,
            LinkerLoad = 0x10,
            Outer = 0x18,
            FName = 0x20,
            Class = 0x24,
            SuperField = 0x28,
            Size = 0x36,
            PropertyOffset = 0x44,
            Bitmask = 0x78,
            Struct = 0x54
        };
    }

    public class SC3GameData : GameData
    {
        public SC3GameData(GameInfo _info) : base(_info) { }
        
        public override void RefreshObjects()
        {
            Offsets = Data.Offsets;
            base.RefreshObjects();
        }
    }
}
