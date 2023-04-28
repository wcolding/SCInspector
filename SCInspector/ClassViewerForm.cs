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
    public partial class ClassViewerForm : Form
    {
        private GameData gameData;
        private int index;
        private GameObject gameObject;
        private List<GameObject> properties;
        private List<GameObject> selectedProperties;
        private GameObject[] instances;

        public ClassViewerForm(int _index, GameData _gameData)
        {
            InitializeComponent();
            gameData = _gameData;
            index = _index;
            if (gameData.objects.ContainsKey(index))
                gameObject = gameData.objects[index];

            properties = new List<GameObject>();
            selectedProperties = new List<GameObject>();
        }

        private void ClassViewerForm_Load(object sender, EventArgs e)
        {
            int preferredInstanceIndex = -1;
            if (gameObject.type == ObjectType.Instance)
            {
                if (gameData.objects.ContainsKey(gameObject.classIndex))
                {
                    preferredInstanceIndex = gameObject.index;
                    gameObject = gameData.objects[gameObject.classIndex];
                }
            }

            this.Text = String.Format("Viewing {0}", gameObject.fullPath);

            UpdateInstances();
            if ((preferredInstanceIndex >= 0) && gameData.objects.ContainsKey(preferredInstanceIndex))
                instanceSelection.SelectedIndex = instanceSelection.Items.IndexOf(gameData.objects[preferredInstanceIndex].fullPath);

            properties.AddRange(gameData.GetClassProperties(gameObject));

            foreach (GameObject obj in properties)
            {
                ListViewItem currentItem = new ListViewItem();
                currentItem.Text = obj.fullPath;
                currentItem.SubItems.Add(gameData.GetClassName(obj));
                currentItem.SubItems.Add(obj.index.ToString());
                fullPropertiesListView.Items.Add(currentItem);
            }
        }

        private void FullPropertiesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem selected = fullPropertiesListView.SelectedItems[0];
            foreach (GameObject property in properties)
            {
                if (property.fullPath == selected.Text)
                {
                    fullPropertiesListView.Items.Remove(selected);
                    selectedProperties.Add(property);
                    RecalculateInstanceProperties();
                }
            }
        }

        private void SelPropertiesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            GameObject selectedProperty = selectedProperties[selPropertiesListView.SelectedItems[0].Index];
            PropertyType selectedType = selectedProperty.propertyData.type;

            switch (selectedType)
            {
                case PropertyType.Bool:
                    {
                        DialogResult result = MessageBox.Show(
                            String.Format("Setting BoolProperty '{0}'", selectedProperty.name),
                            String.Format("Yes = True, No = False", selectedProperty.name),
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        BoolPropertyData asBool = (BoolPropertyData)selectedProperty.propertyData;

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
                default:
                    break;
            }
            //ListViewItem selected = selPropertiesListView.SelectedItems[0];
            //selPropertiesListView.Items.Remove(selected);
            //fullPropertiesListView.Items.Insert(0, selected);
        }

        private void UpdateInstances()
        {
            instances = gameData.GetInstances(gameObject);
            instanceSelection.Items.Clear();
            foreach (GameObject instance in instances)
                instanceSelection.Items.Add(String.Format("{0}: {1}", instance.address.ToString("X8"), instance.fullPath));
        }

        private void RecalculateInstanceProperties()
        {
            if (instanceSelection.SelectedItem != null)
            {
                foreach (GameObject property in selectedProperties)
                    property.propertyData.calculated = instances[instanceSelection.SelectedIndex].address + property.propertyData.offset;
            }

            PopulateSelPropertiesListView();
        }

        private void PopulateSelPropertiesListView()
        {
            selPropertiesListView.Items.Clear();
            foreach (GameObject property in selectedProperties)
            {
                ListViewItem currentItem = new ListViewItem();
                currentItem.Text = property.fullPath;
                currentItem.SubItems.Add(gameData.GetClassName(property));
                currentItem.SubItems.Add(property.propertyData.offset.ToString("X2")); //offset
                currentItem.SubItems.Add(property.propertyData.calculated.ToString("X8")); //address
                switch (property.propertyData.type) //value
                {
                    case PropertyType.Int:
                        IntPropertyData asInt = (IntPropertyData)property.propertyData;
                        currentItem.SubItems.Add(asInt.value.ToString());
                        break;
                    case PropertyType.Bool:
                        BoolPropertyData asBool = (BoolPropertyData)property.propertyData;
                        currentItem.SubItems.Add(asBool.value.ToString());
                        break;
                    case PropertyType.Byte:
                        BytePropertyData asByte = (BytePropertyData)property.propertyData;
                        currentItem.SubItems.Add(String.Format("0x{0}", asByte.value.ToString("X2")));
                        break;
                    case PropertyType.Float:
                        FloatPropertyData asFloat = (FloatPropertyData)property.propertyData;
                        currentItem.SubItems.Add(String.Format("{0}f", asFloat.value.ToString("0.0")));
                        break;
                    case PropertyType.Object:
                        ObjectPropertyData asObject = (ObjectPropertyData)property.propertyData;
                        int objectPtr = asObject.value;
                        if (objectPtr > 0)
                        {
                            int objectIndex = gameData.GetObjectIndexFromPtr((IntPtr)objectPtr);
                            if (objectIndex != -1)
                                currentItem.SubItems.Add(gameData.objects[objectIndex].name);
                            else
                                currentItem.SubItems.Add("NULL");
                        }
                        else
                        {
                            currentItem.SubItems.Add("NULL");
                        }
                        break;
                    case PropertyType.String:
                        StrPropertyData asString = (StrPropertyData)property.propertyData;
                        string value = Memory.ReadString(asString.value.contents, true); // FStrings always unicode?
                        currentItem.SubItems.Add(value);
                        break;
                    case PropertyType.Name:
                        NamePropertyData asName = (NamePropertyData)property.propertyData;
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

        private void nameSearchBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void classSearchBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
