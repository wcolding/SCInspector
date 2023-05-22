using SCInspector.Unreal;

namespace SCInspector.DoubleAgent
{
    public static class Data
    {
        public static Dictionary<string, GameInfo> Targets = new Dictionary<string, GameInfo>()
        {
            {
                "Double Agent",
                new GameInfo()
                {
                    game = Game.DoubleAgent,
                    windowName = "Tom Clancy's SplinterCell 4",
                    moduleName = "Core.dll",
                    gNamesOffset = 0x2D3A24,
                    gObjectsOffset = 0X2D7AC4
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
            SuperField = 0x2C,
            Size = 0x3A,
            PropertyOffset = 0x48,
            Bitmask = 0x78,
            Struct = 0x58
        };
    }
    public class SC4GameData : GameData
    {
        public SC4GameData(GameInfo _info) : base(_info) { }

        public override void RefreshObjects()
        {
            Offsets = Data.Offsets;
            base.RefreshObjects();
        }
    }
}
