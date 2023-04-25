﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCInspector
{
    public class SC1GameData : GameData
    {
        public SC1GameData(GameEntry _info) : base(_info) { }

        protected override void GetNames(TArray gNamesArray, bool isUnicode = false)
        {
            stringOffset = 0x0C;
            base.GetNames(gNamesArray, true);
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
            bitmaskOffset = 0x54;
            base.GetObjects(gObjectsArray);
        }
    }
}
