using SCInspector.Unreal;

namespace SCInspector
{
    public partial class NamesViewerForm : Form
    {
        GameData gameData;

        public NamesViewerForm(GameData _gameData)
        {
            InitializeComponent();
            this.gameData = _gameData;
        }

        private void NamesViewerForm_Load(object sender, EventArgs e)
        {
            namesListView.Items.Clear();

            foreach(KeyValuePair<int, string> name in gameData.names)
            {
                ListViewItem curItem = new ListViewItem();
                curItem.Text = name.Key.ToString();
                curItem.SubItems.Add(name.Value);
                namesListView.Items.Add(curItem);
            }
        }
    }
}
