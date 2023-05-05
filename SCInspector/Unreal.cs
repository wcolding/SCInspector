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

    public class StructPropertyData : PropertyData
    {
        public IntPtr structClassPtr;
        public short size = -1;
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
                    curEntryIndex = (int)Memory.ReadUInt32(curEntry);
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
                        if ((curNameIndex > -1) && (curNameIndex < gNamesArray.count))
                        {
                            curObject = new GameObject();

                            curObject.outerAddress = (IntPtr)Memory.ReadUInt32(curEntry + outerOffset);
                            curObject.classAddress = (IntPtr)Memory.ReadUInt32(curEntry + classOffset);
                            curObject.inheritedAddress = (IntPtr)Memory.ReadUInt32(curEntry + superOffset);

                            curObject.propertyData = SetPropertyData(curObject, curEntry);

                            if (names.ContainsKey(curNameIndex))
                                curObject.name = names[curNameIndex];
                            else
                                curObject.name = curNameIndex.ToString();

                            curObject.type = ObjectType.GameObject;
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
        protected int structTypeOffset = -1;
        protected int structPropertySizeOffset = -1;
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

        private bool isPropertyInStruct(GameObject propertyGameObject)
        {
            if (propertyGameObject.propertyData.type == PropertyType.None)
                return false;

            if (objects.ContainsKey(propertyGameObject.outerAddress))
            {
                if (objects[propertyGameObject.outerAddress].propertyData.type == PropertyType.Struct)
                    return true;
            }

            return false;
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
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        pd.bitmask = Memory.ReadUInt32(curEntryPtr + bitmaskOffset);
                        return pd;
                    }
                    case "ByteProperty":
                    {
                        BytePropertyData pd = new BytePropertyData();
                        pd.type = PropertyType.Byte;
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                    case "IntProperty":
                    {
                        IntPropertyData pd = new IntPropertyData();
                        pd.type = PropertyType.Int;
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                    case "FloatProperty":
                    {
                        FloatPropertyData pd = new FloatPropertyData();
                        pd.type = PropertyType.Float;
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                    case "ObjectProperty":
                    {
                        ObjectPropertyData pd = new ObjectPropertyData();
                        pd.type = PropertyType.Object;
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                    case "StrProperty":
                    {
                        StrPropertyData pd = new StrPropertyData();
                        pd.type = PropertyType.String;
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                    case "NameProperty":
                    {
                        NamePropertyData pd = new NamePropertyData();
                        pd.type = PropertyType.Name;
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                    case "StructProperty":
                    {
                        StructPropertyData pd = new StructPropertyData();
                        pd.type = PropertyType.Struct;
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        if (structPropertySizeOffset != -1)
                            pd.size = (short)Memory.ReadUInt16(curEntryPtr + structPropertySizeOffset);

                        if (structTypeOffset != -1)
                            pd.structClassPtr = (IntPtr)Memory.ReadUInt32(curEntryPtr + structTypeOffset);

                        return pd;
                    }
                    default:
                    {
                        PropertyData pd = new PropertyData();
                        pd.type = PropertyType.None;
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
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

        public GameObjectEntry[] GetStructProperties(GameObject gameObject)
        {
            List<GameObjectEntry> properties = new List<GameObjectEntry>();

            if (gameObject.propertyData.type != PropertyType.Struct)
                return properties.ToArray();

            StructPropertyData asStruct = (StructPropertyData)gameObject.propertyData;

            if (objects.ContainsKey(asStruct.structClassPtr))
            {
                properties.AddRange(GetProperties(objects[asStruct.structClassPtr]));
            }

            List<GameObjectEntry> newProperties = new List<GameObjectEntry>();
            foreach (GameObjectEntry property in properties) 
            {
                GameObject newObj = CopyGameObject(property.Value);
                newObj.fullPath = String.Format("{0}.{1}", gameObject.fullPath, newObj.name);
                newObj.propertyData.offset += asStruct.offset;
                newProperties.Add(new GameObjectEntry(property.Key, newObj));
            }

            return newProperties.ToArray();
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

        private GameObject CopyGameObject(GameObject gameObject)
        {
            GameObject newObj = new GameObject();
            newObj.name = gameObject.name;
            newObj.fullPath = gameObject.fullPath;
            newObj.index = gameObject.index;
            newObj.classAddress = gameObject.classAddress;
            newObj.outerAddress = gameObject.outerAddress;
            newObj.inheritedAddress = gameObject.inheritedAddress;
            newObj.type = gameObject.type;

            switch (gameObject.propertyData.type)
            {
                case PropertyType.Int:
                {
                    newObj.propertyData = new IntPropertyData();
                    IntPropertyData original = (IntPropertyData)gameObject.propertyData;
                    newObj.propertyData.offset = original.offset;
                    newObj.propertyData.calculated = IntPtr.Zero;
                    newObj.propertyData.type = original.type;
                    break;
                }
                case PropertyType.Bool:
                {
                    newObj.propertyData = new BoolPropertyData();
                    BoolPropertyData original = (BoolPropertyData)gameObject.propertyData;
                    newObj.propertyData.offset = original.offset;
                    newObj.propertyData.calculated = IntPtr.Zero;
                    newObj.propertyData.type = original.type;
                    break;
                    }
                case PropertyType.Byte:
                {
                    newObj.propertyData = new BytePropertyData();
                    BytePropertyData original = (BytePropertyData)gameObject.propertyData;
                    newObj.propertyData.offset = original.offset;
                    newObj.propertyData.calculated = IntPtr.Zero;
                    newObj.propertyData.type = original.type;
                    break;
                }
                case PropertyType.Float:
                {
                    newObj.propertyData = new FloatPropertyData();
                    FloatPropertyData original = (FloatPropertyData)gameObject.propertyData;
                    newObj.propertyData.offset = original.offset;
                    newObj.propertyData.calculated = IntPtr.Zero;
                    newObj.propertyData.type = original.type;
                    break;
                }
                case PropertyType.Name:
                {
                    newObj.propertyData = new NamePropertyData();
                    NamePropertyData original = (NamePropertyData)gameObject.propertyData;
                    newObj.propertyData.offset = original.offset;
                    newObj.propertyData.calculated = IntPtr.Zero;
                    newObj.propertyData.type = original.type;
                    break;
                }
                case PropertyType.Object:
                {
                    newObj.propertyData = new ObjectPropertyData();
                    ObjectPropertyData original = (ObjectPropertyData)gameObject.propertyData;
                    newObj.propertyData.offset = original.offset;
                    newObj.propertyData.calculated = IntPtr.Zero;
                    newObj.propertyData.type = original.type;
                    break;
                }
                case PropertyType.String:
                {
                    newObj.propertyData = new StrPropertyData();
                    StrPropertyData original = (StrPropertyData)gameObject.propertyData;
                    newObj.propertyData.offset = original.offset;
                    newObj.propertyData.calculated = IntPtr.Zero;
                    newObj.propertyData.type = original.type;
                    break;
                }
                case PropertyType.Struct:
                {
                    newObj.propertyData = new StructPropertyData();
                    StructPropertyData newPD = (StructPropertyData)newObj.propertyData;
                    StructPropertyData original = (StructPropertyData)gameObject.propertyData;
                    newPD.offset = original.offset;
                    newPD.calculated = IntPtr.Zero;
                    newPD.type = original.type;
                    newPD.structClassPtr = original.structClassPtr;
                    newPD.size = original.size;
                    newObj.propertyData = newPD;
                    break;
                }
                default:
                    newObj.propertyData = new PropertyData();
                    break;
            }

            return newObj;

        }
}
}
