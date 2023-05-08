namespace SCInspector
{
    public class SC5GameData : GameData
    {
        public SC5GameData(GameEntry _info) : base(_info) { }

        protected override void GetNames(TArray gNamesArray)
        {
            LEADGame = true;
            stringOffset = 0x08;
            base.GetNames(gNamesArray);
        }

        protected override void GetObjects(TArray gObjectsArray)
        {
            indexOffset = 0x04;
            //linkerLoadOffset = 0x10;
            outerOffset = 0x18;
            nameOffset = 0x20; //0xE8 for class name on instances?
            classOffset = 0x24;
            superOffset = 0x28;
            propertyOffset = 0x3E; // 16 bit
            //structTypeOffset = 0x54;
            //structNextPropertyOffset = 0x48;
            bitmaskOffset = 0x9C;
            base.GetObjects(gObjectsArray);
        }
    }
}
