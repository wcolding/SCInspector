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
        Struct
    }

    public enum ObjectType
    {
        GameObject,
        Instance
    }

    public struct GameObject
    {
        public IntPtr address;
        public string name;
        public string fullPath;
        public int index;
        public int classIndex;
        public int outerIndex;
        public int inheritedClassIndex;
        public ObjectType type;
        public PropertyData propertyData;
    }

    public class PropertyData
    {
        public int offset = -1;
        public IntPtr calculated = IntPtr.Zero;
    }

    public class BytePropertyData : PropertyData
    {
        public byte value;
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

    public class FloatPropertyData : PropertyData
    {
        public float value
        {
            get
            {
                if (calculated == IntPtr.Zero)
                    return -1;

                return (float)Memory.ReadUInt32(calculated);
            }

            set
            {
                if (calculated == IntPtr.Zero)
                    return;

                Memory.WriteUInt32(calculated, (uint)value);
            }
        }
    }

    public class GameObjectInspector
    {
        public GameObject gameObject;
        public List<GameObject> attachedObjects;
        public List<GameObject> properties;

        public GameObjectInspector(GameData _gameData, GameObject _gameObject) 
        {
            gameData = _gameData;
            gameObject = _gameObject;
            attachedObjects = new List<GameObject>();
            properties = new List<GameObject>();
        }

        public void RecalculateProperties()
        {

        }

        private GameData gameData;

    }

    public class GameData
    {
        public Dictionary<int, string> names;
        public Dictionary<int, GameObject> objects;
        
        protected virtual void GetNames(TArray gNamesArray, bool isUnicode = false)
        {
            names.Clear();

            int offset = 0;
            IntPtr curEntry;
            int curEntryIndex;

            byte[] stringBuffer = new byte[256];
            string curString;
            int nullIndex;

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
                        Memory.ReadProcessMemory(Memory.ProcessHandle, (IntPtr)(curEntry + stringOffset), stringBuffer, stringBuffer.Length, out Memory.outputPtr);
                        if (isUnicode)
                            curString = Encoding.Unicode.GetString(stringBuffer);
                        else
                            curString = Encoding.UTF8.GetString(stringBuffer);

                        nullIndex = curString.IndexOf('\0');
                        if (nullIndex > -1)
                            curString = curString.Remove(nullIndex);

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
            IntPtr classAddress;
            IntPtr parentAddress;
            IntPtr inheritAddress;
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
                            curObject.address = curEntry;

                            parentAddress = (IntPtr)Memory.ReadUInt32(curEntry + outerOffset);
                            curObject.outerIndex = GetObjectIndexFromPtr(parentAddress);

                            classAddress = (IntPtr)Memory.ReadUInt32(curEntry + classOffset);
                            curObject.classIndex = GetObjectIndexFromPtr(classAddress);

                            inheritAddress = (IntPtr)Memory.ReadUInt32(curEntry + superOffset);
                            curObject.inheritedClassIndex = GetObjectIndexFromPtr(inheritAddress);

                            curObject.propertyData = SetPropertyData(curObject, curEntry);

                            if (names.ContainsKey(curNameIndex))
                                curObject.name = names[curNameIndex];
                            else
                                curObject.name = curNameIndex.ToString();

                            curObject.fullPath = GetFullPath(curObject);

                            curObject.type = ObjectType.GameObject;
                            linkerLoadValue = (int)Memory.ReadUInt32(curEntry + linkerLoadOffset);
                            if (linkerLoadValue == 0)
                                curObject.type = ObjectType.Instance;

                            curObject.index = (int)(offset / 4);
                            objects.Add(curEntryIndex, curObject);
                        }
                    }
                }

                offset += 4;
            }
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
        protected int bitmaskOffset;
        #endregion

        public GameData(GameEntry _info)
        {
            names = new Dictionary<int, string>();
            objects = new Dictionary<int, GameObject>();
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

            foreach (KeyValuePair<int, GameObject> obj in objects)
            {
                if (obj.Value.name == objectName)
                    addresses.Add(obj.Value.address);
            }

            return addresses.ToArray();
        }

        public GameObject GetObject(string objectName)
        {
            GameObject temp = new GameObject();

            foreach (KeyValuePair<int, GameObject> obj in objects)
            {
                if (obj.Value.name == objectName)
                    return obj.Value;
            }

            return temp;
        }

        public string GetClassName(GameObject gameObject) 
        {
            if (gameObject.classIndex == -1)
                return string.Empty;

            if (objects.ContainsKey(gameObject.classIndex))
                return objects[gameObject.classIndex].name;

            return string.Empty;
        }

        private bool isProperty(GameObject gameObject)
        {
            if (GetClassName(gameObject).Contains("Property"))
                return true;
            return false;
        }

        private PropertyData SetPropertyData(GameObject gameObject, IntPtr curEntryPtr)
        {
            string className = GetClassName(gameObject);
            if (className.Contains("Property"))
            {
                switch (className)
                {
                    case "ArrayProperty":
                        break;
                    case "BoolProperty":
                    {
                        BoolPropertyData pd = new BoolPropertyData();
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        pd.bitmask = Memory.ReadUInt32(curEntryPtr + bitmaskOffset);
                        return pd;
                    }
                    case "ByteProperty":
                    {
                        BytePropertyData pd = new BytePropertyData();
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                    case "IntProperty":
                    {
                        IntPropertyData pd = new IntPropertyData();
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                    case "FloatProperty":
                    {
                        FloatPropertyData pd = new FloatPropertyData();
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                    default:
                    {
                        PropertyData pd = new PropertyData();
                        pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                        return pd;
                    }
                }
            }

            return new PropertyData();
        }

        protected int GetObjectIndexFromPtr(IntPtr ptr)
        {
            foreach (KeyValuePair<int, GameObject> obj in objects)
            {
                if (obj.Value.address == ptr)
                    return obj.Key;
            }

            return -1;
        }

        protected string GetObjectName(int index)
        {
            if (objects.ContainsKey(index))
                return objects[index].name;
            
            return "null";
        }

        protected string GetFullPath(GameObject gameObject)
        {
            string path = gameObject.name;

            if ((gameObject.outerIndex > 0) && objects.ContainsKey(gameObject.outerIndex))
            {
                path = string.Format("{0}.{1}", GetFullPath(objects[gameObject.outerIndex]), path);
            }

            return path;
        }

        public GameObject[] GetInstances(string className)
        {
            List<GameObject> instances = new List<GameObject>();

            foreach (KeyValuePair<int, GameObject> obj in objects)
            {
                if (GetObjectName(obj.Value.classIndex) == className)
                    instances.Add(obj.Value);
            }

            return instances.ToArray();
        }

        public GameObject[] GetProperties(GameObject gameObject)
        {
            List<GameObject> properties = new List<GameObject>();

            foreach (KeyValuePair<int, GameObject> obj in objects)
            {
                if (objects.ContainsKey(obj.Value.outerIndex) && objects.ContainsKey(gameObject.index))
                {
                    if (objects[obj.Value.outerIndex].address == objects[gameObject.classIndex].address)
                        properties.Add(obj.Value);
                }
            }

            return properties.ToArray();
        }

        public GameObject[] GetClassProperties(GameObject gameObject, bool recursive = true)
        {
            List<GameObject> properties = new List<GameObject>();

            // Get the class object
            GameObject classObject;

            if (objects.ContainsKey(gameObject.classIndex))
                classObject = objects[gameObject.classIndex];
            else
                return properties.ToArray();

            // Get the first layer of children of the class object
            foreach (KeyValuePair<int, GameObject> obj in objects)
            {
                if (obj.Value.outerIndex == gameObject.classIndex)
                {
                    properties.Add(obj.Value);
                }
            }


            // Get class properties
            if (recursive && (gameObject.classIndex != 0))
            {
                GameObject[] parentProperties = GetClassProperties(classObject, true);
                foreach (GameObject pObj in parentProperties)
                    properties.Add(pObj);
            }

            return properties.ToArray();
        }



    }
}
