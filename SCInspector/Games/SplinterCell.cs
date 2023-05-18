using SCInspector.Unreal;
﻿using System.Runtime.InteropServices;

namespace SCInspector.SplinterCell
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
            structPropertySizeOffset = 0x3A;
            structTypeOffset = 0x54;
            structNextPropertyOffset = 0x48;
            bitmaskOffset = 0x54;
            base.GetObjects(gObjectsArray);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe class UObject
    {
        [FieldOffset(0)]
        public IntPtr vTable;
        [FieldOffset(0x04)]
        public int internalIndex;
        [FieldOffset(0x10)]
        public int linkerLoad;
        [FieldOffset(0x18)]
        public IntPtr outer;
        [FieldOffset(0x1C)]
        public int objectFlags;
        [FieldOffset(0x20)]
        public int nameIndex;
        [FieldOffset(0x24)]
        public IntPtr classPtr;
        [FieldOffset(0x28)]
        public IntPtr dName;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe class UField : UObject
    {
        [FieldOffset(0x2C)]
        public int superField;
        [FieldOffset(0x30)]
        public int next;
        [FieldOffset(0x34)]
        public int hashNext;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe class UProperty : UField
    {
        [FieldOffset(0x3A)]
        public short size;
        [FieldOffset(0x44)]
        public IntPtr propertyOffset;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe class UBoolProperty : UProperty
    {
        [FieldOffset(0x54)]
        public int bitmask;
    }

}
