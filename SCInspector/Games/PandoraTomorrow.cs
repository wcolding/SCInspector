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
    }

    public class SC2GameData : GameData
    {
        public SC2GameData(GameInfo _info) : base(_info) { }

        protected override void GetNames(TArray gNamesArray)
        {
            stringOffset = 0x0C;
            unicode = true;
            base.GetNames(gNamesArray);
        }

        protected override void GetObjects(TArray gObjectsArray)
        {
            indexOffset = 0x04;
            linkerLoadOffset = 0x10;
            outerOffset = 0x18;
            nameOffset = 0x20;
            classOffset = 0x24;
            superOffset = 0x2C;
            propertyOffset = 0x44;
            structPropertySizeOffset = 0x3A;
            structTypeOffset = 0x54;
            structNextPropertyOffset = 0x48;
            bitmaskOffset = 0x54;
            base.GetObjects(gObjectsArray);
        }
    }
}
