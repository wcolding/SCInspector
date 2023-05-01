using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SCInspector
{
    using GameObjectEntry = KeyValuePair<IntPtr, GameObject>;

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

    public class PropertyData
    {
        public int offset = -1;
        public IntPtr calculated = IntPtr.Zero;
        public PropertyType type;
    }

    public class BytePropertyData : PropertyData
    {
        public byte value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return 0;

                return (byte)Memory.ReadUInt8(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteUInt8(calculated, value);
            }
        }
    }

    public class BoolPropertyData : PropertyData
    {
        public uint bitmask;
        public bool value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return false;

                uint bitfield = Memory.ReadUInt32(calculated);
                if ((bitfield & bitmask) != 0)
                    return true;

                return false;
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                uint bitfield = Memory.ReadUInt32(calculated);
                uint newVal = bitfield | bitmask;
                if (!value)
                    newVal ^= bitmask;

                Memory.WriteUInt32(calculated, newVal);
            }
        }
    }

    public class IntPropertyData : PropertyData
    {
        public int value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return -1;

                return (int)Memory.ReadUInt32(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteUInt32(calculated, (uint)value);
            }
        }
    }

    public class NamePropertyData : IntPropertyData { }

    public class StrPropertyData : PropertyData
    {
        public TArray value
        {
            get
            {
                TArray nullTArray = new TArray() { contents = IntPtr.Zero, count = 0, max = 0 };

                if (calculated == IntPtr.Zero)
                    return nullTArray;

                byte[] fstringBuffer = new byte[12];
                Memory.ReadProcessMemory(Memory.ProcessHandle, calculated, fstringBuffer, 12, out Memory.outputPtr);
                if (Memory.outputPtr != IntPtr.Zero)
                {
                    TArray output = new TArray();
                    output.contents = (IntPtr)BitConverter.ToUInt32(fstringBuffer, 0);
                    output.count = BitConverter.ToUInt32(fstringBuffer, 4);
                    output.max = BitConverter.ToUInt32(fstringBuffer, 8);
                    return output;
                }

                return nullTArray;
            }

            // set?
        }
    }

    public class FloatPropertyData : PropertyData
    {
        public float value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return -1;

                return Memory.ReadFloat(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteFloat(calculated, value);
            }
        }
    }

    public class ObjectPropertyData : PropertyData
    {
        public IntPtr value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return IntPtr.Zero;

                return (IntPtr)Memory.ReadUInt32(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteUInt32(calculated, (uint)value);
            }
        }
    }
        

    public class GameData
    {
        public Dictionary<int, string> names;
        public Dictionary<IntPtr, GameObject> objects;

        public bool isUnicode
        {
            get { return unicode; }
        }

        protected bool unicode = false;


        public bool isLEADGame
        {
            get { return LEADGame; }
        }

        protected bool LEADGame = false;

        protected virtual void GetNames(TArray gNamesArray)
        {
            names.Clear();

            int offset = 0;
            IntPtr curEntry;
            int curEntryIndex;

            byte[] stringBuffer = new byte[256];
            string curString;

            for (int i = 0; i < gNamesArray.count; i++)
            {
                ProgressUpdate.progressType = ProgressType.Names;
                ProgressUpdate.max = (int)gNamesArray.count;
                ProgressUpdate.index = i;

                curEntry = (IntPtr)Memory.ReadUInt32(gNamesArray.contents + offset);
                if (curEntry != IntPtr.Zero)
                {
                    if (!LEADGame)
                        curEntryIndex = (int)Memory.ReadUInt32(curEntry);
                    else
                        curEntryIndex = i;
                    if (!names.ContainsKey(curEntryIndex))
                    {
                        curString = Memory.ReadString(curEntry + stringOffset, unicode);
                        names.Add(curEntryIndex, curString);
                    }
                }

                offset += 4;
            }
        }

        protected virtual void GetObjects(TArray gObjectsArray)
        {
            if (names.Count() == 0)
                return;

            objects.Clear();

            int offset = 0;

            IntPtr curEntry;
            int curEntryIndex;
            int curNameIndex;
            int linkerLoadValue = 0;

            GameObject curObject;

            for (int i = 0; i < gObjectsArray.count; i++)
            {
                ProgressUpdate.progressType = ProgressType.Objects;
                ProgressUpdate.max = (int)gObjectsArray.count;
                ProgressUpdate.index = i;

                curEntry = (IntPtr)Memory.ReadUInt32(gObjectsArray.contents + offset);
                if (curEntry != IntPtr.Zero)
                {
                    curEntryIndex = (int)Memory.ReadUInt32(curEntry + indexOffset);
                    if (!objects.ContainsKey(curEntryIndex))
                    {
                        curNameIndex = (int)Memory.ReadUInt32(curEntry + nameOffset);
                        if (curNameIndex > -1)
                        {
                            curObject = new GameObject();

                            curObject.outerAddress = (IntPtr)Memory.ReadUInt32(curEntry + outerOffset);
                            curObject.classAddress = (IntPtr)Memory.ReadUInt32(curEntry + classOffset);
                            curObject.inheritedAddress = (IntPtr)Memory.ReadUInt32(curEntry + superOffset);

                            curObject.propertyData = SetPropertyData(curObject, curEntry);

                            curObject.type = ObjectType.GameObject;

                            if (names.ContainsKey(curNameIndex))
                                curObject.name = names[curNameIndex];
                            else if (LEADGame)
                            {
                                curObject.name = GetClassName(curObject);
                                curObject.type = ObjectType.Instance;
                            }
                            else
                                curObject.name = curNameIndex.ToString();
;
                            linkerLoadValue = (int)Memory.ReadUInt32(curEntry + linkerLoadOffset);
                            if (linkerLoadValue == 0)
                                curObject.type = ObjectType.Instance;

                            curObject.index = (int)(offset / 4);
                            objects.Add(curEntry, curObject);
                        }
                    }
                }

                offset += 4;
            }

            ResolvePaths();
        }

        protected void ResolvePaths()
        {
            Dictionary<IntPtr, GameObject> newObjects = new Dictionary<IntPtr, GameObject>();
            GameObject curObj;

            foreach (GameObjectEntry obj in objects)
            {
                curObj = obj.Value;
                curObj.fullPath = GetFullPath(curObj);
                newObjects.Add(obj.Key, curObj);
            }

            objects = newObjects;
        }

        private TArray gNamesArray;
        private TArray gObjectsArray;
        private GameEntry info;

        #region Game-Specified Offsets
        protected int stringOffset;
        protected int indexOffset;
        protected int linkerLoadOffset;  // Distinguishes classes from their instances; null if instance
        protected int outerOffset;       // Parent object
        protected int nameOffset;
        protected int classOffset;
        protected int superOffset;       // Class this inherits from
        protected int propertyOffset;
        protected int structTypeOffset;
        protected int structNextPropertyOffset;
        protected int bitmaskOffset;
        #endregion

        public GameData(GameEntry _info)
        {
            names = new Dictionary<int, string>();
            objects = new Dictionary<IntPtr, GameObject>();
            info = _info;
            RefreshObjects();
        }

        public void RefreshObjects()
        {
            if ((Memory.ProcessHandle != IntPtr.Zero) && (Memory.ModuleBase != IntPtr.Zero))
            {
                gNamesArray = new TArray();
                gNamesArray.contents = (IntPtr)Memory.ReadUInt32(Memory.ModuleBase + info.gNamesOffset);
                gNamesArray.count = Memory.ReadUInt32(Memory.ModuleBase + info.gNamesOffset + 4);
                gNamesArray.max = Memory.ReadUInt32(Memory.ModuleBase + info.gNamesOffset + 8);

                gObjectsArray = new TArray();
                gObjectsArray.contents = (IntPtr)Memory.ReadUInt32(Memory.ModuleBase + info.gObjectsOffset);
                gObjectsArray.count = Memory.ReadUInt32(Memory.ModuleBase + info.gObjectsOffset + 4);
                gObjectsArray.max = Memory.ReadUInt32(Memory.ModuleBase + info.gObjectsOffset + 8);

                GetNames(gNamesArray);
                GetObjects(gObjectsArray);
                ProgressUpdate.progressType = ProgressType.None;
            }
        }

        public IntPtr[] GetObjectAddresses(string objectName)
        {
            List<IntPtr> addresses = new List<IntPtr>();
            if (objects.Count == 0)
                return addresses.ToArray();

            foreach (GameObjectEntry obj in objects)
            {
                if (obj.Value.name == objectName)
                    addresses.Add(obj.Key);
            }

            return addresses.ToArray();
        }

        public GameObject GetObject(string objectName)
        {
            GameObject temp = new GameObject();

            foreach (GameObjectEntry obj in objects)
            {
                if (obj.Value.name == objectName)
                    return obj.Value;
            }

            return temp;
        }

        public string GetClassName(GameObject gameObject) 
        {
            if (gameObject.classAddress == IntPtr.Zero)
                return string.Empty;

            if (objects.ContainsKey(gameObject.classAddress))
                return objects[gameObject.classAddress].name;

            return string.Empty;
        }

        private bool isProperty(GameObject gameObject)
        {
            if (GetClassName(gameObject).Contains("Property"))
                return true;
            return false;
        }

        private void GetPropertyOffset(PropertyData pd, IntPtr curEntryPtr)
        {
            if (!LEADGame)
                pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
            else
                pd.offset = (int)Memory.ReadUInt16(curEntryPtr + propertyOffset);
        }

        private PropertyData SetPropertyData(GameObject gameObject, IntPtr curEntryPtr)
        {
            string className = GetClassName(gameObject);
            if (className.Contains("Property"))
            {
                switch (className)
                {
                    case "BoolProperty":
                    {
                        BoolPropertyData pd = new BoolPropertyData();
                        pd.type = PropertyType.Bool;
                        GetPropertyOffset(pd, curEntryPtr);
                        pd.bitmask = Memory.ReadUInt32(curEntryPtr + bitmaskOffset);
                        return pd;
                    }
                    case "ByteProperty":
                    {
                        BytePropertyData pd = new BytePropertyData();
                        pd.type = PropertyType.Byte;
                        GetPropertyOffset(pd, curEntryPtr);
                        return pd;
                    }
                    case "IntProperty":
                    {
                        IntPropertyData pd = new IntPropertyData();
                        pd.type = PropertyType.Int;
                        GetPropertyOffset(pd, curEntryPtr);
                        return pd;
                    }
                    case "FloatProperty":
                    {
                        FloatPropertyData pd = new FloatPropertyData();
                        pd.type = PropertyType.Float;
                        GetPropertyOffset(pd, curEntryPtr);
                        return pd;
                    }
                    case "ObjectProperty":
                    {
                        ObjectPropertyData pd = new ObjectPropertyData();
                        pd.type = PropertyType.Object;
                        GetPropertyOffset(pd, curEntryPtr);
                        return pd;
                    }
                    case "StrProperty":
                    {
                        StrPropertyData pd = new StrPropertyData();
                        pd.type = PropertyType.String;
                        GetPropertyOffset(pd, curEntryPtr);
                        return pd;
                    }
                    case "NameProperty":
                    {
                        NamePropertyData pd = new NamePropertyData();
                        pd.type = PropertyType.Name;
                        GetPropertyOffset(pd, curEntryPtr);
                        return pd;
                    }
                    default:
                    {
                        PropertyData pd = new PropertyData();
                        pd.type = PropertyType.None;
                        GetPropertyOffset(pd, curEntryPtr);
                        return pd;
                    }
                }
            }

            return new PropertyData();
        }

        protected string GetObjectName(IntPtr address)
        {
            if (objects.ContainsKey(address))
                return objects[address].name;
            
            return "null";
        }

        protected string GetFullPath(GameObject gameObject)
        {
            string path = gameObject.name;

            if ((gameObject.outerAddress != IntPtr.Zero) && objects.ContainsKey(gameObject.outerAddress))
            {
                path = string.Format("{0}.{1}", GetFullPath(objects[gameObject.outerAddress]), path);
            }

            return path;
        }

        public GameObjectEntry[] GetInstances(string className)
        {
            List<GameObjectEntry> instances = new List<GameObjectEntry>();

            foreach (GameObjectEntry obj in objects)
            {
                if (GetObjectName(obj.Value.classAddress) == className)
                    instances.Add(obj);
            }

            return instances.ToArray();
        }

        public GameObjectEntry[] GetInstances(GameObject classObject)
        {
            List<GameObjectEntry> instances = new List<GameObjectEntry>();

            foreach (GameObjectEntry obj in objects)
            {
                if (objects.ContainsKey(obj.Value.classAddress))
                    if (objects[obj.Value.classAddress].index == classObject.index)
                        instances.Add(obj);
            }

            return instances.ToArray();
        }

        public GameObjectEntry[] GetProperties(GameObject gameObject)
        {
            List<GameObjectEntry> properties = new List<GameObjectEntry>();

            foreach (GameObjectEntry obj in objects)
            {
                if (objects.ContainsKey(obj.Value.outerAddress))
                {
                    if (objects[obj.Value.outerAddress].index == gameObject.index)
                        properties.Add(obj);
                }
            }

            return properties.ToArray();
        }

        public GameObjectEntry[] GetClassProperties(GameObject gameObject, bool recursive = true)
        {
            List<GameObjectEntry> properties = new List<GameObjectEntry>();

            // Get the first layer of children of this object
            foreach (GameObjectEntry obj in objects)
            {
                if (objects.ContainsKey(obj.Value.outerAddress))
                {
                    if (objects[obj.Value.outerAddress].index == gameObject.index)
                    {
                        properties.Add(obj);
                    }
                }
            }

            // Get inherited class properties
            GameObject inheritedClassObject;

            if (recursive && (gameObject.inheritedAddress != IntPtr.Zero))
            {
                if (objects.ContainsKey(gameObject.inheritedAddress))
                {
                    inheritedClassObject = objects[gameObject.inheritedAddress];
                    GameObjectEntry[] parentProperties = GetClassProperties(inheritedClassObject, true);
                    foreach (GameObjectEntry pObj in parentProperties)
                        properties.Add(pObj);
                }
            }

            return properties.ToArray();
        }
    }
}
