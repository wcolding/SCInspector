﻿namespace SCInspector.Unreal
{
    using SCInspector;
    using GameObjectEntry = KeyValuePair<IntPtr, GameObject>;

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
            int curEntryIndex;

            byte[] stringBuffer = new byte[256];
            string curString;

            IntPtr[] namePtrs = Memory.ReadTArrayPtrs(gNamesArray);

            foreach (IntPtr curEntry in namePtrs)
            {
                ProgressUpdate.progressType = ProgressType.Names;
                ProgressUpdate.max = (int)gNamesArray.count;
                //ProgressUpdate.index = i;

                curEntryIndex = (int)Memory.ReadUInt32(curEntry);
                if (!names.ContainsKey(curEntryIndex))
                {
                    curString = Memory.ReadString(curEntry + stringOffset, unicode);
                    names.Add(curEntryIndex, curString);
                }
            }
        }

        protected virtual void GetObjects(TArray gObjectsArray)
        {
            if (names.Count() == 0)
                return;

            objects.Clear();

            int offset = 0;

            int curEntryIndex;
            int curNameIndex;
            int linkerLoadValue = 0;

            IntPtr[] objPtrs = Memory.ReadTArrayPtrs(gObjectsArray);

            GameObject curObject;

            foreach (IntPtr curEntry in objPtrs)
            {
                ProgressUpdate.progressType = ProgressType.Objects;
                ProgressUpdate.max = (int)gObjectsArray.count;
                //ProgressUpdate.index = i;

                curEntryIndex = (int)Memory.ReadUInt32(curEntry + indexOffset);
                if (!objects.ContainsKey(curEntryIndex))
                {
                    curNameIndex = (int)Memory.ReadUInt32(curEntry + nameOffset);
                    if (curNameIndex > -1 && curNameIndex < gNamesArray.count)
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

                        curObject.index = offset / 4;
                        objects.Add(curEntry, curObject);
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
            if (Memory.ProcessHandle != IntPtr.Zero && Memory.ModuleBase != IntPtr.Zero)
            {
                gNamesArray = Memory.ReadStructure<TArray>(Memory.ModuleBase + info.gNamesOffset);
                gObjectsArray = Memory.ReadStructure<TArray>(Memory.ModuleBase + info.gObjectsOffset);

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
                    case "ArrayProperty":
                        {
                            ArrayPropertyData pd = new ArrayPropertyData();
                            pd.type = PropertyType.Array;
                            pd.offset = (int)Memory.ReadUInt32(curEntryPtr + propertyOffset);
                            return pd;
                        }
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

            if (gameObject.outerAddress != IntPtr.Zero && objects.ContainsKey(gameObject.outerAddress))
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
                newObj.fullPath = string.Format("{0}.{1}", gameObject.fullPath, newObj.name);
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

            if (recursive && gameObject.inheritedAddress != IntPtr.Zero)
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
                case PropertyType.Array:
                    {
                        newObj.propertyData = new ArrayPropertyData();
                        ArrayPropertyData original = (ArrayPropertyData)gameObject.propertyData;
                        newObj.propertyData.offset = original.offset;
                        newObj.propertyData.calculated = IntPtr.Zero;
                        newObj.propertyData.type = original.type;
                        break;
                    }
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