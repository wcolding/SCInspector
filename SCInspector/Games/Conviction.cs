using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            outerOffset = 0xF0;
            nameOffset = 0xE8;
            classOffset = 0x24;
            //superOffset = 0x28;
            propertyOffset = 0x50; // todo: find this
            //structTypeOffset = 0x54;
            //structNextPropertyOffset = 0x48;
            //bitmaskOffset = 0x78;
            base.GetObjects(gObjectsArray);
        }
    }
}
