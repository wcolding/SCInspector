using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCInspector
{
    public class SC1GameData : GameData
    {
        public SC1GameData(GameEntry _info) : base(_info) { }

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
            structPropertySizeOffset = 0x3C;
            structTypeOffset = 0x54;
            structNextPropertyOffset = 0x48;
            bitmaskOffset = 0x54;
            base.GetObjects(gObjectsArray);
        }
    }
}
