using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCInspector
{
    public class SC2GameData : GameData
    {
        public SC2GameData(GameEntry _info) : base(_info) { }

        protected override void GetNames(TArray gNamesArray)
        {
            stringOffset = 0x0C;
            unicode = true;
            base.GetNames(gNamesArray);
        }

        protected override void GetObjects(TArray gObjectsArray)
        {
            indexOffset = 0x04;
            outerOffset = 0x18;
            nameOffset = 0x20;
            classOffset = 0x24;
            propertyOffset = 0x44;
            base.GetObjects(gObjectsArray);
        }
    }
}
