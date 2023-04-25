namespace SCInspector
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            gamesDropdown = new ComboBox();
            progressBar = new ProgressBar();
            hookGameButton = new Button();
            progressMsgLabel = new Label();
            refreshButton = new Button();
            nameSearchBox = new TextBox();
            objectsListView = new ListView();
            Path = new ColumnHeader();
            ObjIndex = new ColumnHeader();
            ObjAddress = new ColumnHeader();
            ObjClass = new ColumnHeader();
            PropOffset = new ColumnHeader();
            searchNameLabel = new Label();
            resultsCountLabel = new Label();
            mainFormRightClickContext = new ContextMenuStrip(components);
            rightClickCopy = new ToolStripMenuItem();
            classSearchBox = new TextBox();
            searchClassLabel = new Label();
            addressSearchBox = new TextBox();
            searchAddressLabel = new Label();
            mainFormRightClickContext.SuspendLayout();
            SuspendLayout();
            // 
            // gamesDropdown
            // 
            gamesDropdown.DropDownStyle = ComboBoxStyle.DropDownList;
            gamesDropdown.FormattingEnabled = true;
            gamesDropdown.Location = new Point(630, 12);
            gamesDropdown.Name = "gamesDropdown";
            gamesDropdown.Size = new Size(158, 23);
            gamesDropdown.TabIndex = 0;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 410);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(776, 23);
            progressBar.Step = 1;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 1;
            // 
            // hookGameButton
            // 
            hookGameButton.Location = new Point(630, 41);
            hookGameButton.Name = "hookGameButton";
            hookGameButton.Size = new Size(158, 30);
            hookGameButton.TabIndex = 2;
            hookGameButton.Text = "Hook Game";
            hookGameButton.UseVisualStyleBackColor = true;
            hookGameButton.Click += hookGameButton_Click;
            // 
            // progressMsgLabel
            // 
            progressMsgLabel.Dock = DockStyle.Bottom;
            progressMsgLabel.ImageAlign = ContentAlignment.MiddleLeft;
            progressMsgLabel.Location = new Point(0, 435);
            progressMsgLabel.Name = "progressMsgLabel";
            progressMsgLabel.Size = new Size(800, 15);
            progressMsgLabel.TabIndex = 3;
            progressMsgLabel.Text = "Ready";
            progressMsgLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // refreshButton
            // 
            refreshButton.Location = new Point(630, 77);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(158, 30);
            refreshButton.TabIndex = 4;
            refreshButton.Text = "Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += refreshButton_Click;
            // 
            // nameSearchBox
            // 
            nameSearchBox.Location = new Point(67, 28);
            nameSearchBox.Name = "nameSearchBox";
            nameSearchBox.Size = new Size(276, 23);
            nameSearchBox.TabIndex = 5;
            nameSearchBox.TextChanged += searchBox_TextChanged;
            // 
            // objectsListView
            // 
            objectsListView.Anchor = AnchorStyles.Bottom;
            objectsListView.Columns.AddRange(new ColumnHeader[] { Path, ObjIndex, ObjAddress, ObjClass, PropOffset });
            objectsListView.FullRowSelect = true;
            objectsListView.Location = new Point(12, 123);
            objectsListView.Name = "objectsListView";
            objectsListView.Size = new Size(776, 281);
            objectsListView.TabIndex = 6;
            objectsListView.UseCompatibleStateImageBehavior = false;
            objectsListView.View = View.Details;
            objectsListView.SelectedIndexChanged += objectsListView_SelectedIndexChanged;
            objectsListView.MouseClick += objectsListView_MouseClick;
            objectsListView.MouseDoubleClick += objectsListView_MouseDoubleClick;
            // 
            // Path
            // 
            Path.Text = "Name";
            Path.Width = 300;
            // 
            // ObjIndex
            // 
            ObjIndex.Text = "Index";
            // 
            // ObjAddress
            // 
            ObjAddress.Text = "Address";
            ObjAddress.Width = 120;
            // 
            // ObjClass
            // 
            ObjClass.Text = "Class";
            ObjClass.Width = 180;
            // 
            // PropOffset
            // 
            PropOffset.Text = "Offset";
            PropOffset.Width = 80;
            // 
            // searchNameLabel
            // 
            searchNameLabel.AutoSize = true;
            searchNameLabel.Location = new Point(22, 31);
            searchNameLabel.Name = "searchNameLabel";
            searchNameLabel.Size = new Size(39, 15);
            searchNameLabel.TabIndex = 7;
            searchNameLabel.Text = "Name";
            searchNameLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // resultsCountLabel
            // 
            resultsCountLabel.AutoSize = true;
            resultsCountLabel.Location = new Point(698, 436);
            resultsCountLabel.Name = "resultsCountLabel";
            resultsCountLabel.Size = new Size(50, 15);
            resultsCountLabel.TabIndex = 8;
            resultsCountLabel.Text = "Results: ";
            resultsCountLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // mainFormRightClickContext
            // 
            mainFormRightClickContext.Items.AddRange(new ToolStripItem[] { rightClickCopy });
            mainFormRightClickContext.Name = "contextMenuStrip1";
            mainFormRightClickContext.Size = new Size(148, 26);
            // 
            // rightClickCopy
            // 
            rightClickCopy.Name = "rightClickCopy";
            rightClickCopy.Size = new Size(147, 22);
            rightClickCopy.Text = "Copy Address";
            rightClickCopy.Click += RightClickCopy_Click;
            // 
            // classSearchBox
            // 
            classSearchBox.Location = new Point(67, 57);
            classSearchBox.Name = "classSearchBox";
            classSearchBox.Size = new Size(276, 23);
            classSearchBox.TabIndex = 9;
            classSearchBox.TextChanged += classSearchBox_TextChanged;
            // 
            // searchClassLabel
            // 
            searchClassLabel.AutoSize = true;
            searchClassLabel.Location = new Point(27, 60);
            searchClassLabel.Name = "searchClassLabel";
            searchClassLabel.Size = new Size(34, 15);
            searchClassLabel.TabIndex = 10;
            searchClassLabel.Text = "Class";
            searchClassLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // addressSearchBox
            // 
            addressSearchBox.Location = new Point(67, 86);
            addressSearchBox.Name = "addressSearchBox";
            addressSearchBox.Size = new Size(276, 23);
            addressSearchBox.TabIndex = 11;
            addressSearchBox.TextChanged += addressSearchBox_TextChanged;
            // 
            // searchAddressLabel
            // 
            searchAddressLabel.AutoSize = true;
            searchAddressLabel.Location = new Point(12, 89);
            searchAddressLabel.Name = "searchAddressLabel";
            searchAddressLabel.Size = new Size(49, 15);
            searchAddressLabel.TabIndex = 12;
            searchAddressLabel.Text = "Address";
            searchAddressLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(800, 450);
            Controls.Add(searchAddressLabel);
            Controls.Add(addressSearchBox);
            Controls.Add(searchClassLabel);
            Controls.Add(classSearchBox);
            Controls.Add(resultsCountLabel);
            Controls.Add(searchNameLabel);
            Controls.Add(objectsListView);
            Controls.Add(nameSearchBox);
            Controls.Add(refreshButton);
            Controls.Add(progressMsgLabel);
            Controls.Add(hookGameButton);
            Controls.Add(progressBar);
            Controls.Add(gamesDropdown);
            Name = "MainForm";
            Text = "SC Inspector";
            Load += MainForm_Load;
            mainFormRightClickContext.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox gamesDropdown;
        public ProgressBar progressBar;
        private Button hookGameButton;
        private Label progressMsgLabel;
        private Button refreshButton;
        private TextBox nameSearchBox;
        private ListView objectsListView;
        private ColumnHeader Path;
        private ColumnHeader ObjIndex;
        private ColumnHeader ObjAddress;
        private ColumnHeader ObjClass;
        private Label searchNameLabel;
        private Label resultsCountLabel;
        private ContextMenuStrip mainFormRightClickContext;
        private ToolStripMenuItem rightClickCopy;
        private ColumnHeader PropOffset;
        private TextBox classSearchBox;
        private Label searchClassLabel;
        private TextBox addressSearchBox;
        private Label searchAddressLabel;
    }
}