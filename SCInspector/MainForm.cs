using SCInspector.Unreal;

namespace SCInspector
{
    using GameObjectEntry = KeyValuePair<IntPtr, GameObject>;
    using Target = KeyValuePair<string, GameInfo>;
    using static InputUtils;

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            debouncedFilterListView = Debounce(500, () => Invoke(FilterListView));
        }

        public GameData gameData;
        private List<ListViewItem> listViewCache = new List<ListViewItem>();
        private Action debouncedFilterListView;
        private Dictionary<string, GameInfo> targets;

        private void MainForm_Load(object sender, EventArgs e)
        {
            ProgressUpdate.form = this;

            SetMsgLabel("Ready");

            gamesDropdown.Enabled = true;
            hookGameButton.Enabled = true;
            refreshButton.Enabled = false;
            viewNamesButton.Enabled = false;

            gamesDropdown.Items.Clear();

            targets = Games.GetTargets();
            foreach (Target t in targets)
                gamesDropdown.Items.Add(t.Key);

            gamesDropdown.SelectedIndex = 0;
        }

        private void hookGameButton_Click(object sender, EventArgs e)
        {
            GameInfo selectedGame = targets[gamesDropdown.Text];
            Memory.OpenGame(selectedGame.windowName, selectedGame.moduleName);
            if (Memory.ModuleBase == IntPtr.Zero)
            {
                SetMsgLabel("Unable to hook game");
                return;
            }

            SetMsgLabel("Hooking...");

            gamesDropdown.Enabled = false;
            hookGameButton.Enabled = false;

            switch (selectedGame.game)
            {
                case Game.SplinterCell:
                    gameData = new SplinterCell.SC1GameData(selectedGame);
                    break;
                case Game.PandoraTomorrow:
                    gameData = new PandoraTomorrow.SC2GameData(selectedGame);
                    break;
                case Game.ChaosTheory:
                    gameData = new ChaosTheory.SC3GameData(selectedGame);
                    break;
                case Game.DoubleAgent:
                    gameData = new DoubleAgent.SC4GameData(selectedGame);
                    break;
                case Game.ConvictionSteam:
                    gameData = new Conviction.SC5GameData(selectedGame);
                    break;
                case Game.ConvictionUbi:
                    gameData = new Conviction.SC5GameData(selectedGame);
                    break;
                default:
                    break;
            }

            LockFilters();
            UpdateListView();
            refreshButton.Enabled = true;
            viewNamesButton.Enabled = true;
        }

        public void SetMsgLabel(string msg)
        {
            progressMsgLabel.Text = msg;
            progressMsgLabel.Update();
        }

        private void UpdateResultsLabel()
        {
            resultsCountLabel.Text = String.Format("Results: {0}", objectsListView.Items.Count.ToString());
            resultsCountLabel.Refresh();
        }

        private void LockFilters()
        {
            nameSearchBox.Text = string.Empty;
            classSearchBox.Text = string.Empty;
            addressSearchBox.Text = string.Empty;

            nameSearchBox.Enabled = false;
            classSearchBox.Enabled = false;
            addressSearchBox.Enabled = false;

        }

        private void UnlockFilters()
        {
            nameSearchBox.Enabled = true;
            classSearchBox.Enabled = true;
            addressSearchBox.Enabled = true;
        }


        private void refreshButton_Click(object sender, EventArgs e)
        {
            refreshButton.Enabled = false; 
            viewNamesButton.Enabled = false;
            LockFilters();
            gameData.RefreshObjects();
            UpdateListView();
            refreshButton.Enabled = true; 
            viewNamesButton.Enabled = true;
        }

        private void UpdateListView()
        {
            objectsListView.Items.Clear();
            listViewCache.Clear();

            ProgressUpdate.progressType = ProgressType.LoadTable;
            ProgressUpdate.index = 0;
            ProgressUpdate.max = gameData.objects.Count();

            foreach (GameObjectEntry obj in gameData.objects)
            {
                ListViewItem currentItem = new ListViewItem();
                currentItem.Text = obj.Value.fullPath;
                currentItem.SubItems.Add(obj.Value.index.ToString());
                currentItem.SubItems.Add(obj.Key.ToString("X8"));
                currentItem.SubItems.Add(gameData.GetClassName(obj.Value));
                if (obj.Value.propertyData.offset >= 0)
                    currentItem.SubItems.Add(obj.Value.propertyData.offset.ToString("X2"));
                else
                    currentItem.SubItems.Add(string.Empty);
                objectsListView.Items.Add(currentItem);
                listViewCache.Add(currentItem);
                ProgressUpdate.index++;
            }

            ProgressUpdate.progressType = ProgressType.None;
            objectsListView.Refresh();
            UpdateResultsLabel();
            UnlockFilters();
        }

        private bool NameFilter(ListViewItem item)
        {
            ProgressUpdate.index++;
            if (nameSearchBox.Text == string.Empty)
                return true;

            if (item.Text.ToLower().Contains(nameSearchBox.Text.ToLower().Trim()))
                return true;

            return false;
        }

        private bool ClassFilter(ListViewItem item)
        {
            ProgressUpdate.index++;
            if (classSearchBox.Text == string.Empty)
                return true;

            if (item.SubItems[3].Text.ToLower().Contains(classSearchBox.Text.ToLower().Trim()))
                return true;

            return false;
        }

        private bool AddressFilter(ListViewItem item)
        {
            ProgressUpdate.index++;
            if (addressSearchBox.Text == string.Empty)
                return true;

            if (item.SubItems[2].Text.ToLower().Contains(addressSearchBox.Text.ToLower().Trim()))
                return true;

            return false;
        }

        private void FilterListView()
        {
            objectsListView.Items.Clear();

            if ((nameSearchBox.Text == string.Empty) && (classSearchBox.Text == string.Empty) && (addressSearchBox.Text == string.Empty))
            {
                objectsListView.Items.AddRange(listViewCache.ToArray());
                return;
            }

            ProgressUpdate.index = 0;
            ProgressUpdate.progressType = ProgressType.Filter;
            ProgressUpdate.max = listViewCache.Count;

            objectsListView.Items.AddRange(listViewCache.FindAll(NameFilter).FindAll(ClassFilter).FindAll(AddressFilter).ToArray());
            UpdateResultsLabel();

            ProgressUpdate.progressType = ProgressType.None;
        }

        private void objectsListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void objectsListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (objectsListView.SelectedItems.Count > 0)
                    mainFormRightClickContext.Show(Cursor.Position);
            }
        }

        private void objectsListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = 0;
            if (int.TryParse(objectsListView.SelectedItems[0].SubItems[1].Text, out index))
            {
                foreach (GameObjectEntry obj in gameData.objects)
                {
                    if (obj.Value.index == index)
                    {
                        ClassViewerForm cvForm = new ClassViewerForm(obj.Key, gameData);
                        cvForm.Show();
                        return;
                    }
                }
            }

        }

        private void RightClickCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(objectsListView.SelectedItems[0].SubItems[2].Text);
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            debouncedFilterListView();
        }

        private void classSearchBox_TextChanged(object sender, EventArgs e)
        {
            debouncedFilterListView();
        }

        private void addressSearchBox_TextChanged(object sender, EventArgs e)
        {
            debouncedFilterListView();
        }

        private void viewNamesButton_Click(object sender, EventArgs e)
        {
            NamesViewerForm namesViewer = new NamesViewerForm(gameData);
            namesViewer.Show();
        }
    }

    public enum ProgressType
    {
        None,
        Names,
        Objects,
        LoadTable,
        Filter
    }

    public static class ProgressUpdate
    {
        public static ProgressType progressType
        {
            get { return _progressType; }
            set
            {
                _progressType = value;
                updateDisplay();
            }
        }

        public static int index
        {
            get { return _index; }
            set
            {
                _index = value;
                updateDisplay();
            }
        }
        public static int max
        {
            get { return _index; }
            set
            {
                _max = value;
                updateDisplay();
            }
        }

        public static MainForm form;

        private static ProgressType _progressType;
        private static int _index;
        private static int _max;

        private static void updateDisplay()
        {
            switch (_progressType)
            {
                case ProgressType.Names:
                    form.SetMsgLabel(String.Format("Getting Names ({0}/{1})", _index, _max));
                    updateBar();
                    break;
                case ProgressType.Objects:
                    form.SetMsgLabel(String.Format("Getting Objects ({0}/{1})", _index, _max));
                    updateBar();
                    break;
                case ProgressType.Filter:
                    form.SetMsgLabel("Filtering list");
                    updateBar();
                    break;
                case ProgressType.LoadTable:
                    form.SetMsgLabel("Loading table");
                    updateBar();
                    break;
                default:
                    form.SetMsgLabel("Ready");
                    form.progressBar.Value = 0;
                    break;
            }
        }

        private static void updateBar()
        {
            form.progressBar.Minimum = 0;
            form.progressBar.Maximum = _max;
            form.progressBar.Value = Math.Min(_index, _max);
        }

    }
}