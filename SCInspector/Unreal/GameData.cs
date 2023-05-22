namespace SCInspector.Unreal
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

        public bool LEADEngine { get; protected set; } = false;

        protected void GetNames(TArray gNamesArray)
        {
            names.Clear();

            string curString;
            Dictionary<int, IntPtr> nameEntries = Memory.ReadTArray(gNamesArray);

            foreach (KeyValuePair<int, IntPtr> curEntry in nameEntries)
            {
                ProgressUpdate.progressType = ProgressType.Names;
                ProgressUpdate.max = (int)gNamesArray.count;
                //ProgressUpdate.index = i;

                if (!names.ContainsKey(curEntry.Key))
                {
                    curString = Memory.ReadString(curEntry.Value + Offsets.String, unicode);
                    names.Add(curEntry.Key, curString);
                }
            }
        }

        protected void GetObjects(TArray gObjectsArray)
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

                curEntryIndex = (int)Memory.ReadUInt32(curEntry + Offsets.InternalIndex);
                if (!objects.ContainsKey(curEntry))
                {
                    curNameIndex = (int)Memory.ReadUInt32(curEntry + Offsets.FName);
                    if (curNameIndex > -1)
                    {
                        curObject = new GameObject();

                        curObject.outerAddress = (IntPtr)Memory.ReadUInt32(curEntry + Offsets.Outer);
                        curObject.classAddress = (IntPtr)Memory.ReadUInt32(curEntry + Offsets.Class);
                        curObject.inheritedAddress = (IntPtr)Memory.ReadUInt32(curEntry + Offsets.SuperField);

                        curObject.propertyData = SetPropertyData(curObject, curEntry);

                        if (names.ContainsKey(curNameIndex))
                            curObject.name = names[curNameIndex];
                        else if (LEADEngine)
                        {
                            int classNameIndex = curNameIndex & 0x7FFFF;
                            int instanceNum = (curNameIndex >> 0x13) - 1;
                            if (names.ContainsKey(classNameIndex))
                                curObject.name = String.Format("{0}_{1}", names[classNameIndex], instanceNum.ToString());
                            else
                                curObject.name = curNameIndex.ToString();
                        }
                        else
                            curObject.name = curNameIndex.ToString();

                        curObject.type = ObjectType.GameObject;
                        linkerLoadValue = (int)Memory.ReadUInt32(curEntry + Offsets.LinkerLoad);
                        if (linkerLoadValue == 0)
                            curObject.type = ObjectType.Instance;

                        curObject.index = curEntryIndex;
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

        protected GameOffsets Offsets;

        private TArray gNamesArray;
        private TArray gObjectsArray;
        private GameInfo info;

        public GameData(GameInfo _info)
        {
            names = new Dictionary<int, string>();
            objects = new Dictionary<IntPtr, GameObject>();
            info = _info;
            RefreshObjects();
        }

        public virtual void RefreshObjects()
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
            if (propertyGameObject.propertyData.Type == PropertyType.None)
                return false;

            if (objects.ContainsKey(propertyGameObject.outerAddress))
            {
                if (objects[propertyGameObject.outerAddress].propertyData.Type == PropertyType.Struct)
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
                        pd.SetData(curEntryPtr, Offsets, LEADEngine);
                        return pd;
                    }
                    case "BoolProperty":
                    {
                        BoolPropertyData pd = new BoolPropertyData();
                        pd.SetData(curEntryPtr, Offsets, LEADEngine);
                        return pd;
                    }
                    case "ByteProperty":
                    {
                        BytePropertyData pd = new BytePropertyData();
                        pd.SetData(curEntryPtr, Offsets, LEADEngine);
                        return pd;
                    }
                    case "IntProperty":
                    {
                        IntPropertyData pd = new IntPropertyData();
                        pd.SetData(curEntryPtr, Offsets, LEADEngine);
                        return pd;
                    }
                    case "FloatProperty":
                    {
                        FloatPropertyData pd = new FloatPropertyData();
                        pd.SetData(curEntryPtr, Offsets, LEADEngine);
                        return pd;
                    }
                    case "ObjectProperty":
                    {
                        ObjectPropertyData pd = new ObjectPropertyData();
                        pd.SetData(curEntryPtr, Offsets, LEADEngine);
                        return pd;
                    }
                    case "StrProperty":
                    {
                        StrPropertyData pd = new StrPropertyData();
                        pd.SetData(curEntryPtr, Offsets, LEADEngine);
                        return pd;
                    }
                    case "NameProperty":
                    {
                        NamePropertyData pd = new NamePropertyData();
                        pd.SetData(curEntryPtr, Offsets, LEADEngine);
                        return pd;
                    }
                    case "StructProperty":
                    {
                        StructPropertyData pd = new StructPropertyData();
                        pd.SetData(curEntryPtr, Offsets, LEADEngine);

                        return pd;
                    }
                    default:
                        return new PropertyData();
                        
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

            if (gameObject.propertyData.Type != PropertyType.Struct)
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
            newObj.propertyData = gameObject.propertyData.GetCopy();

            return newObj;

        }
    }
}
