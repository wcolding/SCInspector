using SCInspector.Unreal;

namespace SCInspector.Conviction
{
    public class SC5GameData : GameData
    {
        public SC5GameData(GameEntry _info) : base(_info) { }

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
