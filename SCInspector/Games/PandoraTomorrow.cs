using SCInspector.Unreal;

namespace SCInspector.PandoraTomorrow
{
    public static class Data
    {
        public static Dictionary<string, GameInfo> Targets = new Dictionary<string, GameInfo>()
        {
            { 
                "Pandora Tomorrow",
                new GameInfo()
                {
                    game = Game.PandoraTomorrow,
                    windowName = "Tom Clancy's Splinter Cell: Pandora Tomorrow",
                    moduleName = "Core.dll",
                    gNamesOffset = 0x1A9384,
                    gObjectsOffset = 0x1AE0EC
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
            DName = 0x28,
            SuperField = 0x2C,
            Size = 0x3A,
            PropertyOffset = 0x44,
            Bitmask = 0x54,
            Struct = 0x54
        };
    }

    public class SC2GameData : GameData
    {
        public SC2GameData(GameInfo _info) : base(_info) { }
        
        public override void RefreshObjects()
        {
            Offsets = Data.Offsets;
            unicode = true;
            base.RefreshObjects();
        }
    }
}
