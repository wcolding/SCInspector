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
    }

    public class SC3GameData : GameData
    {
        public SC3GameData(GameInfo _info) : base(_info) { }

        protected override void GetNames(TArray gNamesArray)
        {
            stringOffset = 0x0C;
            base.GetNames(gNamesArray);
        }

        protected override void GetObjects(TArray gObjectsArray)
        {
            indexOffset = 0x04;
            linkerLoadOffset = 0x10;
            outerOffset = 0x18;
            nameOffset = 0x20;
            classOffset = 0x24;
            superOffset = 0x28;
            propertyOffset = 0x44;
            structPropertySizeOffset = 0x36;
            structTypeOffset = 0x54;
            structNextPropertyOffset = 0x48;
            bitmaskOffset = 0x78;
            base.GetObjects(gObjectsArray);
        }
    }
}
