using SCInspector.Unreal;

namespace SCInspector.Conviction
{
    public static class Data
    {
        public static Dictionary<string, GameInfo> Targets = new Dictionary<string, GameInfo>()
        {
            //{
            //    "Conviction (Steam)",
            //    new GameInfo()
            //    {
            //        game = Game.ConvictionSteam,
            //        windowName = "Conviction",
            //        moduleName = "conviction_game.exe",
            //        gNamesOffset = 0x101907C,
            //        gObjectsOffset = 0x1008FC8
            //    }
            //},
            //{
            //    "Conviction (Ubisoft)",
            //    new GameInfo()
            //    {
            //        game = Game.ConvictionUbi,
            //        windowName = "Conviction",
            //        moduleName = "Conviction_game.exe", // capital C for ubi
            //        gNamesOffset = (IntPtr)0x101913C,
            //        gObjectsOffset = (IntPtr)0x1009088
            //    }
            //}
        };
    }

    public class SC5GameData : GameData
    {
        public SC5GameData(GameInfo _info) : base(_info) { }

        protected override void GetNames(TArray gNamesArray)
        {
            stringOffset = 0x08;
            base.GetNames(gNamesArray);
        }

        protected override void GetObjects(TArray gObjectsArray)
        {
            indexOffset = 0x04;
            outerOffset = 0xF0;
            nameOffset = 0xE8;
            classOffset = 0x0C;
            propertyOffset = 0x50; // todo: find this
            base.GetObjects(gObjectsArray);
        }
    }
}
