namespace SCInspector.Unreal
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct TArray
    {
        public IntPtr contents;
        public uint count;
        public uint max;
    }

    public enum PropertyType
    {
        None,
        Array,
        Bool,
        Byte,
        Float,
        Int,
        Map,
        Name,
        Object,
        Pointer,
        Struct,
        String
    }

    public enum ObjectType
    {
        GameObject,
        Instance
    }

    public struct GameOffsets
    {
        public IntPtr String;
        public IntPtr InternalIndex;
        public IntPtr LinkerLoad;
        public IntPtr Outer;
        public IntPtr FName;
        public IntPtr Class;
        public IntPtr DName;
        public IntPtr SuperField;
        public IntPtr Size;
        public IntPtr PropertyOffset;
        public IntPtr Bitmask;
        public IntPtr Struct;
    }

    public struct GameObject
    {
        public string name;
        public string fullPath;
        public int index;
        public IntPtr classAddress;
        public IntPtr outerAddress;
        public IntPtr inheritedAddress;
        public ObjectType type;
        public PropertyData propertyData;
    }
}
