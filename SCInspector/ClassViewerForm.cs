using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            properties.AddRange(gameData.GetClassProperties(gameObject));

            foreach (GameObjectEntry obj in properties)
            {
                ListViewItem currentItem = new ListViewItem();
                currentItem.Text = obj.Value.fullPath;
                currentItem.SubItems.Add(gameData.GetClassName(obj.Value));
                currentItem.SubItems.Add(obj.Value.index.ToString());
                propertiesListViewCache.Add(currentItem);
                fullPropertiesListView.Items.Add(currentItem);
            }
        }

        private void FullPropertiesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem selected = fullPropertiesListView.SelectedItems[0];
            foreach (GameObjectEntry property in properties)
            {
                if (property.Value.fullPath == selected.Text)
                {
                    fullPropertiesListView.Items.Remove(selected);
                    propertiesListViewCache.Remove(selected);
                    selectedProperties.Add(property);
                    RecalculateInstanceProperties();
                }
            }
        }

        private void SelPropertiesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            GameObjectEntry selectedProperty = selectedProperties[selPropertiesListView.SelectedItems[0].Index];
            PropertyType selectedType = selectedProperty.Value.propertyData.type;

            switch (selectedType)
            {
                case PropertyType.Bool:
                {
                    DialogResult result = MessageBox.Show(
                        String.Format("Setting BoolProperty '{0}'", selectedProperty.Value.name),
                        "Yes = True, No = False",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    BoolPropertyData asBool = (BoolPropertyData)selectedProperty.Value.propertyData;

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
                    IntPropertyData asInt = (IntPropertyData)selectedProperty.Value.propertyData;
                    EditIntForm form = new EditIntForm(asInt, selectedProperty.Value.fullPath);
                    form.Show();
                    RecalculateInstanceProperties();
                    break;
                }
                case PropertyType.Float:
                {
                    FloatPropertyData asFloat = (FloatPropertyData)selectedProperty.Value.propertyData;
                    EditFloatForm form = new EditFloatForm(asFloat, selectedProperty.Value.fullPath);
                    form.Show();
                    RecalculateInstanceProperties();
                    break;
                }
                default:
                    break;
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
                switch (property.Value.propertyData.type) //value
                {
                    case PropertyType.Int:
                        IntPropertyData asInt = (IntPropertyData)property.Value.propertyData;
                        currentItem.SubItems.Add(asInt.value.ToString());
                        break;
                    case PropertyType.Bool:
                        BoolPropertyData asBool = (BoolPropertyData)property.Value.propertyData;
                        currentItem.SubItems.Add(asBool.value.ToString());
                        break;
                    case PropertyType.Byte:
                        BytePropertyData asByte = (BytePropertyData)property.Value.propertyData;
                        currentItem.SubItems.Add(String.Format("0x{0}", asByte.value.ToString("X2")));
                        break;
                    case PropertyType.Float:
                        FloatPropertyData asFloat = (FloatPropertyData)property.Value.propertyData;
                        currentItem.SubItems.Add(String.Format("{0}f", asFloat.value.ToString("0.0")));
                        break;
                    case PropertyType.Object:
                        ObjectPropertyData asObject = (ObjectPropertyData)property.Value.propertyData;
                        IntPtr objectPtr = (IntPtr)asObject.value;
                        if (objectPtr != IntPtr.Zero)
                        {
                            currentItem.SubItems.Add(gameData.objects[objectPtr].name);
                        }
                        else
                        {
                            currentItem.SubItems.Add("NULL");
                        }
                        break;
                    case PropertyType.String:
                        StrPropertyData asString = (StrPropertyData)property.Value.propertyData;
                        string value = Memory.ReadString(asString.value.contents, true); // FStrings always unicode?
                        currentItem.SubItems.Add(value);
                        break;
                    case PropertyType.Name:
                        NamePropertyData asName = (NamePropertyData)property.Value.propertyData;
                        currentItem.SubItems.Add(gameData.names[asName.value]);
                        break;
                    default:
                        currentItem.SubItems.Add("temp");
                        break;
                }
                selPropertiesListView.Items.Add(currentItem);
            }
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
    }
}
