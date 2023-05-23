using System.ComponentModel;
using SCInspector.Unreal;

namespace SCInspector
{
    using GameObjectEntry = KeyValuePair<IntPtr, GameObject>;

    public partial class ClassViewerForm : Form
    {
        private GameData gameData;
        private int index;
        private IntPtr address;
        private GameObject gameObject;
        private List<GameObjectEntry> properties;
        private List<GameObjectEntry> selectedProperties;
        private GameObjectEntry[] instances;
        private List<ListViewItem> propertiesListViewCache = new List<ListViewItem>();
        private bool isClosable = false;

        public ClassViewerForm(IntPtr _address, GameData _gameData)
        {
            InitializeComponent();
            gameData = _gameData;
            address = _address;
            if (gameData.objects.ContainsKey(address))
                gameObject = gameData.objects[address];

            properties = new List<GameObjectEntry>();
            selectedProperties = new List<GameObjectEntry>();
        }

        private void ClassViewerForm_Load(object sender, EventArgs e)
        {
            IntPtr preferredInstancePtr = IntPtr.Zero;

            if (gameObject.type == ObjectType.Instance)
            {
                if (gameData.objects.ContainsKey(gameObject.classAddress))
                {
                    preferredInstancePtr = address;
                    gameObject = gameData.objects[gameObject.classAddress];
                }
            }

            this.Text = String.Format("Viewing {0}", gameObject.fullPath);

            UpdateInstances();
            if ((preferredInstancePtr != IntPtr.Zero) && gameData.objects.ContainsKey(preferredInstancePtr))
            {
                GameObject instance = gameData.objects[preferredInstancePtr];
                instanceSelection.SelectedIndex = instanceSelection.Items.IndexOf(String.Format("{0}: {1}", preferredInstancePtr.ToString("X8"), instance.fullPath));
            }

            GameObjectEntry[] children = gameData.GetClassProperties(gameObject);
            foreach (GameObjectEntry child in children)
            {
                if (child.Value.propertyData.Type != PropertyType.None)
                        properties.Add(child);
            }

            foreach (GameObjectEntry obj in properties)
            {
                ListViewItem currentItem = new ListViewItem();
                currentItem.Text = obj.Value.fullPath;
                currentItem.SubItems.Add(gameData.GetClassName(obj.Value));
                currentItem.SubItems.Add(obj.Value.index.ToString());
                propertiesListViewCache.Add(currentItem);
                fullPropertiesListView.Items.Add(currentItem);
            }

            isClosable = false;
            autoRefreshWorker.WorkerReportsProgress = true;
            autoRefreshWorker.WorkerSupportsCancellation = true;
            autoRefreshWorker.RunWorkerAsync();
        }

        private void FullPropertiesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (fullPropertiesListView.SelectedItems.Count == 0)
                return;

            ListViewItem selected = fullPropertiesListView.SelectedItems[0];
            foreach (GameObjectEntry property in properties)
            {
                if (property.Value.fullPath == selected.Text)
                {
                    fullPropertiesListView.Items.Remove(selected);
                    propertiesListViewCache.Remove(selected);
                    selectedProperties.Add(property);
                    if (property.Value.propertyData.Type == PropertyType.Struct)
                        selectedProperties.AddRange(gameData.GetStructProperties(property.Value));
                    RecalculateInstanceProperties();
                }
            }
        }

        private void SelPropertiesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (selPropertiesListView.SelectedItems.Count < 1)
                return;

            foreach (GameObjectEntry property in selectedProperties)
            {
                if (property.Value.fullPath == selPropertiesListView.SelectedItems[0].Text)
                {
                    PropertyType selectedType = property.Value.propertyData.Type;
                    switch (selectedType)
                    {
                        case PropertyType.Bool:
                            {
                                DialogResult result = MessageBox.Show(
                                    String.Format("Setting BoolProperty '{0}'", property.Value.name),
                                    "Yes = True, No = False",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question);

                                BoolPropertyData asBool = (BoolPropertyData)property.Value.propertyData;

                                switch (result)
                                {
                                    case DialogResult.Yes:
                                        asBool.value = true;
                                        RecalculateInstanceProperties();
                                        break;
                                    case DialogResult.No:
                                        asBool.value = false;
                                        RecalculateInstanceProperties();
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            }
                        case PropertyType.Int:
                            {
                                IntPropertyData asInt = (IntPropertyData)property.Value.propertyData;
                                EditIntForm form = new EditIntForm(asInt, property.Value.fullPath);
                                form.Show();
                                RecalculateInstanceProperties();
                                break;
                            }
                        case PropertyType.Float:
                            {
                                FloatPropertyData asFloat = (FloatPropertyData)property.Value.propertyData;
                                EditFloatForm form = new EditFloatForm(asFloat, property.Value.fullPath);
                                form.Show();
                                RecalculateInstanceProperties();
                                break;
                            }
                        case PropertyType.Object:
                            {
                                ObjectPropertyData asObj = (ObjectPropertyData)property.Value.propertyData;
                                if (asObj.value != IntPtr.Zero)
                                {
                                    ClassViewerForm form = new ClassViewerForm(asObj.value, this.gameData);
                                    form.Show();
                                }
                                break;
                            }
                        default:
                            break;
                    }

                    return;
                }
            }            
        }

        private void UpdateInstances()
        {
            instances = gameData.GetInstances(gameObject);
            instanceSelection.Items.Clear();
            foreach (GameObjectEntry instance in instances)
                instanceSelection.Items.Add(String.Format("{0}: {1}", instance.Key.ToString("X8"), instance.Value.fullPath));
        }

        private void RecalculateInstanceProperties()
        {
            if (instanceSelection.SelectedItem != null)
            {
                foreach (GameObjectEntry property in selectedProperties)
                    property.Value.propertyData.calculated = instances[instanceSelection.SelectedIndex].Key + property.Value.propertyData.offset;
            }

            PopulateSelPropertiesListView();
        }

        private void RefreshInstanceValues()
        {
            if (selPropertiesListView.Items.Count < 1)
                return;

            if (instanceSelection.SelectedItem != null)
            {
                foreach (GameObjectEntry property in selectedProperties)
                    property.Value.propertyData.calculated = instances[instanceSelection.SelectedIndex].Key + property.Value.propertyData.offset;

                for (int i = 0; i < selectedProperties.Count; i++)
                {
                    switch (selectedProperties[i].Value.propertyData.Type)
                    {
                        case PropertyType.Array:
                            ArrayPropertyData asArray = (ArrayPropertyData)selectedProperties[i].Value.propertyData;
                            selPropertiesListView.Items[i].SubItems[4].Text = String.Format("0x{0}", asArray.value.contents.ToString("X8"));
                            break;
                        case PropertyType.Int:
                            IntPropertyData asInt = (IntPropertyData)selectedProperties[i].Value.propertyData;
                            selPropertiesListView.Items[i].SubItems[4].Text = asInt.value.ToString();
                            break;
                        case PropertyType.Bool:
                            BoolPropertyData asBool = (BoolPropertyData)selectedProperties[i].Value.propertyData;
                            selPropertiesListView.Items[i].SubItems[4].Text = asBool.value.ToString();
                            break;
                        case PropertyType.Byte:
                            BytePropertyData asByte = (BytePropertyData)selectedProperties[i].Value.propertyData;
                            selPropertiesListView.Items[i].SubItems[4].Text = String.Format("0x{0}", asByte.value.ToString("X2"));
                            break;
                        case PropertyType.Float:
                            FloatPropertyData asFloat = (FloatPropertyData)selectedProperties[i].Value.propertyData;
                            selPropertiesListView.Items[i].SubItems[4].Text = String.Format("{0}f", asFloat.value.ToString("0.0"));
                            break;
                        case PropertyType.Object:
                            ObjectPropertyData asObject = (ObjectPropertyData)selectedProperties[i].Value.propertyData;
                            IntPtr objectPtr = (IntPtr)asObject.value;
                            if ((objectPtr != IntPtr.Zero) && (gameData.objects.ContainsKey(objectPtr)))
                            {
                                selPropertiesListView.Items[i].SubItems[4].Text = gameData.objects[objectPtr].fullPath;
                            }
                            else
                            {
                                selPropertiesListView.Items[i].SubItems[4].Text = "null";
                            }
                            break;
                        case PropertyType.String:
                            StrPropertyData asString = (StrPropertyData)selectedProperties[i].Value.propertyData;
                            string value = Memory.ReadString(asString.value.contents, true); // FStrings always unicode?
                            selPropertiesListView.Items[i].SubItems[4].Text = value;
                            break;
                        case PropertyType.Name:
                            NamePropertyData asName = (NamePropertyData)selectedProperties[i].Value.propertyData;
                            selPropertiesListView.Items[i].SubItems[4].Text = gameData.names[asName.value];
                            break;
                        case PropertyType.Struct:
                            StructPropertyData asStruct = (StructPropertyData)selectedProperties[i].Value.propertyData;
                            selPropertiesListView.Items[i].Font = new Font(selPropertiesListView.Items[i].Font, selPropertiesListView.Items[i].Font.Style | FontStyle.Bold);
                            selPropertiesListView.Items[i].SubItems[4].Text = String.Format("Struct size: {0}", asStruct.size.ToString());
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void PopulateSelPropertiesListView()
        {
            selPropertiesListView.Items.Clear();
            foreach (GameObjectEntry property in selectedProperties)
            {
                ListViewItem currentItem = new ListViewItem();
                currentItem.Text = property.Value.fullPath;
                currentItem.SubItems.Add(gameData.GetClassName(property.Value));
                currentItem.SubItems.Add(property.Value.propertyData.offset.ToString("X2")); //offset
                currentItem.SubItems.Add(property.Value.propertyData.calculated.ToString("X8")); //address
                currentItem.SubItems.Add("Value");                
                selPropertiesListView.Items.Add(currentItem);
            }
            
            RefreshInstanceValues();
        }

        private void selPropertiesListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void instanceSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecalculateInstanceProperties();
        }

        private void refreshInstanceButton_Click(object sender, EventArgs e)
        {
            object curSelected = instanceSelection.SelectedItem;

            UpdateInstances();

            if ((curSelected != null) && instanceSelection.Items.Contains(curSelected))
            {
                instanceSelection.SelectedItem = curSelected;
            }

            RecalculateInstanceProperties();
        }

        private bool NameFilter(ListViewItem item)
        {
            if (nameSearchBox.Text == string.Empty)
                return true;

            if (item.Text.ToLower().Contains(nameSearchBox.Text.ToLower().Trim()))
                return true;

            return false;
        }

        private bool ClassFilter(ListViewItem item)
        {
            if (classSearchBox.Text == string.Empty)
                return true;

            if (item.SubItems[1].Text.ToLower().Contains(classSearchBox.Text.ToLower().Trim()))
                return true;

            return false;
        }

        private void FilterListView()
        {
            fullPropertiesListView.Items.Clear();

            if ((nameSearchBox.Text == string.Empty) && (classSearchBox.Text == string.Empty))
            {
                fullPropertiesListView.Items.AddRange(propertiesListViewCache.ToArray());
                return;
            }

            fullPropertiesListView.Items.AddRange(propertiesListViewCache.FindAll(NameFilter).FindAll(ClassFilter).ToArray());

            ProgressUpdate.progressType = ProgressType.None;
        }

        private void nameSearchBox_TextChanged(object sender, EventArgs e)
        {
            FilterListView();
        }

        private void classSearchBox_TextChanged(object sender, EventArgs e)
        {
            FilterListView();
        }

        private void autoRefreshWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!autoRefreshWorker.CancellationPending)
            {
                Thread.Sleep(300);
                Invoke(() =>
                {
                    RefreshInstanceValues();
                });
            }

            Invoke(() => 
            { 
                isClosable = true; 
                this.Close();
            });
        }

        private void ClassViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isClosable)
            {
                autoRefreshWorker.CancelAsync();
                e.Cancel = true;
                this.Enabled = false;
            }
        }
    }
}
