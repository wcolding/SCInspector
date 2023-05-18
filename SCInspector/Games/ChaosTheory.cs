using SCInspector.Unreal;

namespace SCInspector.ChaosTheory
{
    public class SC3GameData : GameData
    {
        public SC3GameData(GameEntry _info) : base(_info) { }

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
