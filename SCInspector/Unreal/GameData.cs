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

        protected void GetNames(TArray gNamesArray)
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
                    curString = Memory.ReadString(curEntry + Offsets.String, unicode);
                    names.Add(curEntryIndex, curString);
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
                if (!objects.ContainsKey(curEntryIndex))
                {
                    curNameIndex = (int)Memory.ReadUInt32(curEntry + Offsets.FName);
                    if (curNameIndex > -1 && curNameIndex < gNamesArray.count)
                    {
                        curObject = new GameObject();

                        curObject.outerAddress = (IntPtr)Memory.ReadUInt32(curEntry + Offsets.Outer);
                        curObject.classAddress = (IntPtr)Memory.ReadUInt32(curEntry + Offsets.Class);
                        curObject.inheritedAddress = (IntPtr)Memory.ReadUInt32(curEntry + Offsets.SuperField);

                        curObject.propertyData = SetPropertyData(curObject, curEntry);

                        if (names.ContainsKey(curNameIndex))
                            curObject.name = names[curNameIndex];
                        else
                            curObject.name = curNameIndex.ToString();

                        curObject.type = ObjectType.GameObject;
                        linkerLoadValue = (int)Memory.ReadUInt32(curEntry + Offsets.LinkerLoad);
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
                            pd.Type = PropertyType.Array;
                            pd.SetData(curEntryPtr, Offsets);
                            return pd;
                        }
                    case "BoolProperty":
                        {
                            BoolPropertyData pd = new BoolPropertyData();
                            pd.Type = PropertyType.Bool;
                            pd.SetData(curEntryPtr, Offsets);
                            return pd;
                        }
                    case "ByteProperty":
                        {
                            BytePropertyData pd = new BytePropertyData();
                            pd.Type = PropertyType.Byte;
                            pd.SetData(curEntryPtr, Offsets);
                            return pd;
                        }
                    case "IntProperty":
                        {
                            IntPropertyData pd = new IntPropertyData();
                            pd.Type = PropertyType.Int;
                            pd.SetData(curEntryPtr, Offsets);
                            return pd;
                        }
                    case "FloatProperty":
                        {
                            FloatPropertyData pd = new FloatPropertyData();
                            pd.Type = PropertyType.Float;
                            pd.SetData(curEntryPtr, Offsets);
                            return pd;
                        }
                    case "ObjectProperty":
                        {
                            ObjectPropertyData pd = new ObjectPropertyData();
                            pd.Type = PropertyType.Object;
                            pd.SetData(curEntryPtr, Offsets);
                            return pd;
                        }
                    case "StrProperty":
                        {
                            StrPropertyData pd = new StrPropertyData();
                            pd.Type = PropertyType.String;
                            pd.SetData(curEntryPtr, Offsets);
                            return pd;
                        }
                    case "NameProperty":
                        {
                            NamePropertyData pd = new NamePropertyData();
                            pd.Type = PropertyType.Name;
                            pd.SetData(curEntryPtr, Offsets);
                            return pd;
                        }
                    case "StructProperty":
                        {
                            StructPropertyData pd = new StructPropertyData();
                            pd.Type = PropertyType.Struct;
                            pd.SetData(curEntryPtr, Offsets);

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

            switch (gameObject.propertyData.Type)
            {
                case PropertyType.Array:
                    {
                        newObj.propertyData = new ArrayPropertyData();
                        ArrayPropertyData original = (ArrayPropertyData)gameObject.propertyData;
                        newObj.propertyData.offset = original.offset;
                        newObj.propertyData.calculated = IntPtr.Zero;
                        newObj.propertyData.Type = original.Type;
                        break;
                    }
                case PropertyType.Int:
                    {
                        newObj.propertyData = new IntPropertyData();
                        IntPropertyData original = (IntPropertyData)gameObject.propertyData;
                        newObj.propertyData.offset = original.offset;
                        newObj.propertyData.calculated = IntPtr.Zero;
                        newObj.propertyData.Type = original.Type;
                        break;
                    }
                case PropertyType.Bool:
                    {
                        newObj.propertyData = new BoolPropertyData();
                        BoolPropertyData original = (BoolPropertyData)gameObject.propertyData;
                        newObj.propertyData.offset = original.offset;
                        newObj.propertyData.calculated = IntPtr.Zero;
                        newObj.propertyData.Type = original.Type;
                        break;
                    }
                case PropertyType.Byte:
                    {
                        newObj.propertyData = new BytePropertyData();
                        BytePropertyData original = (BytePropertyData)gameObject.propertyData;
                        newObj.propertyData.offset = original.offset;
                        newObj.propertyData.calculated = IntPtr.Zero;
                        newObj.propertyData.Type = original.Type;
                        break;
                    }
                case PropertyType.Float:
                    {
                        newObj.propertyData = new FloatPropertyData();
                        FloatPropertyData original = (FloatPropertyData)gameObject.propertyData;
                        newObj.propertyData.offset = original.offset;
                        newObj.propertyData.calculated = IntPtr.Zero;
                        newObj.propertyData.Type = original.Type;
                        break;
                    }
                case PropertyType.Name:
                    {
                        newObj.propertyData = new NamePropertyData();
                        NamePropertyData original = (NamePropertyData)gameObject.propertyData;
                        newObj.propertyData.offset = original.offset;
                        newObj.propertyData.calculated = IntPtr.Zero;
                        newObj.propertyData.Type = original.Type;
                        break;
                    }
                case PropertyType.Object:
                    {
                        newObj.propertyData = new ObjectPropertyData();
                        ObjectPropertyData original = (ObjectPropertyData)gameObject.propertyData;
                        newObj.propertyData.offset = original.offset;
                        newObj.propertyData.calculated = IntPtr.Zero;
                        newObj.propertyData.Type = original.Type;
                        break;
                    }
                case PropertyType.String:
                    {
                        newObj.propertyData = new StrPropertyData();
                        StrPropertyData original = (StrPropertyData)gameObject.propertyData;
                        newObj.propertyData.offset = original.offset;
                        newObj.propertyData.calculated = IntPtr.Zero;
                        newObj.propertyData.Type = original.Type;
                        break;
                    }
                case PropertyType.Struct:
                    {
                        newObj.propertyData = new StructPropertyData();
                        StructPropertyData newPD = (StructPropertyData)newObj.propertyData;
                        StructPropertyData original = (StructPropertyData)gameObject.propertyData;
                        newPD.offset = original.offset;
                        newPD.calculated = IntPtr.Zero;
                        newPD.Type = original.Type;
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
