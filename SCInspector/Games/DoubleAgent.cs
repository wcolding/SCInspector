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
    }
    public class SC4GameData : GameData
    {
        public SC4GameData(GameInfo _info) : base(_info) { }

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
            superOffset = 0x2C;
            propertyOffset = 0x48;
            structPropertySizeOffset = 0x3A;
            structTypeOffset = 0x58;
            structNextPropertyOffset = 0x4C;
            bitmaskOffset = 0x78;
            base.GetObjects(gObjectsArray);
        }
    }
}
