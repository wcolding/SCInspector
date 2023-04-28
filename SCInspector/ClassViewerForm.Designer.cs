namespace SCInspector
{
    partial class ClassViewerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            NameFilterLabel = new Label();
            classFilterLabel = new Label();
            instanceLabel = new Label();
            fullPropertiesListView = new ListView();
            path = new ColumnHeader();
            classCol = new ColumnHeader();
            splitContainer = new SplitContainer();
            selPropertiesListView = new ListView();
            nameCol = new ColumnHeader();
            selClassCol = new ColumnHeader();
            offsetCol = new ColumnHeader();
            addressCol = new ColumnHeader();
            valueCol = new ColumnHeader();
            instanceSelection = new ComboBox();
            refreshInstanceButton = new Button();
            nameSearchBox = new TextBox();
            classSearchBox = new TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // NameFilterLabel
            // 
            NameFilterLabel.AutoSize = true;
            NameFilterLabel.Location = new Point(12, 11);
            NameFilterLabel.Name = "NameFilterLabel";
            NameFilterLabel.Size = new Size(39, 15);
            NameFilterLabel.TabIndex = 0;
            NameFilterLabel.Text = "Name";
            // 
            // classFilterLabel
            // 
            classFilterLabel.AutoSize = true;
            classFilterLabel.Location = new Point(12, 41);
            classFilterLabel.Name = "classFilterLabel";
            classFilterLabel.Size = new Size(34, 15);
            classFilterLabel.TabIndex = 1;
            classFilterLabel.Text = "Class";
            // 
            // instanceLabel
            // 
            instanceLabel.AutoSize = true;
            instanceLabel.Location = new Point(411, 40);
            instanceLabel.Name = "instanceLabel";
            instanceLabel.Size = new Size(98, 15);
            instanceLabel.TabIndex = 2;
            instanceLabel.Text = "Selected Instance";
            // 
            // fullPropertiesListView
            // 
            fullPropertiesListView.Columns.AddRange(new ColumnHeader[] { path, classCol });
            fullPropertiesListView.FullRowSelect = true;
            fullPropertiesListView.Location = new Point(0, 0);
            fullPropertiesListView.MultiSelect = false;
            fullPropertiesListView.Name = "fullPropertiesListView";
            fullPropertiesListView.Size = new Size(377, 372);
            fullPropertiesListView.TabIndex = 3;
            fullPropertiesListView.UseCompatibleStateImageBehavior = false;
            fullPropertiesListView.View = View.Details;
            fullPropertiesListView.MouseDoubleClick += FullPropertiesListView_MouseDoubleClick;
            // 
            // path
            // 
            path.Text = "Name";
            path.Width = 232;
            // 
            // classCol
            // 
            classCol.Text = "Class";
            classCol.Width = 140;
            // 
            // splitContainer
            // 
            splitContainer.Location = new Point(12, 66);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(fullPropertiesListView);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(selPropertiesListView);
            splitContainer.Size = new Size(1047, 372);
            splitContainer.SplitterDistance = 381;
            splitContainer.SplitterWidth = 16;
            splitContainer.TabIndex = 4;
            // 
            // selPropertiesListView
            // 
            selPropertiesListView.Columns.AddRange(new ColumnHeader[] { nameCol, selClassCol, offsetCol, addressCol, valueCol });
            selPropertiesListView.FullRowSelect = true;
            selPropertiesListView.Location = new Point(2, 3);
            selPropertiesListView.MultiSelect = false;
            selPropertiesListView.Name = "selPropertiesListView";
            selPropertiesListView.Size = new Size(645, 369);
            selPropertiesListView.TabIndex = 4;
            selPropertiesListView.UseCompatibleStateImageBehavior = false;
            selPropertiesListView.View = View.Details;
            selPropertiesListView.SelectedIndexChanged += selPropertiesListView_SelectedIndexChanged;
            selPropertiesListView.MouseDoubleClick += SelPropertiesListView_MouseDoubleClick;
            // 
            // nameCol
            // 
            nameCol.Text = "Name";
            nameCol.Width = 232;
            // 
            // selClassCol
            // 
            selClassCol.Text = "Class";
            selClassCol.Width = 140;
            // 
            // offsetCol
            // 
            offsetCol.Text = "Offset";
            // 
            // addressCol
            // 
            addressCol.Text = "Address";
            addressCol.Width = 100;
            // 
            // valueCol
            // 
            valueCol.Text = "Value";
            valueCol.Width = 108;
            // 
            // instanceSelection
            // 
            instanceSelection.DropDownStyle = ComboBoxStyle.DropDownList;
            instanceSelection.FormattingEnabled = true;
            instanceSelection.Location = new Point(515, 37);
            instanceSelection.Name = "instanceSelection";
            instanceSelection.Size = new Size(360, 23);
            instanceSelection.TabIndex = 5;
            instanceSelection.SelectedIndexChanged += instanceSelection_SelectedIndexChanged;
            // 
            // refreshInstanceButton
            // 
            refreshInstanceButton.Location = new Point(881, 36);
            refreshInstanceButton.Name = "refreshInstanceButton";
            refreshInstanceButton.Size = new Size(75, 23);
            refreshInstanceButton.TabIndex = 6;
            refreshInstanceButton.Text = "Refresh";
            refreshInstanceButton.UseVisualStyleBackColor = true;
            refreshInstanceButton.Click += refreshInstanceButton_Click;
            // 
            // nameSearchBox
            // 
            nameSearchBox.Location = new Point(58, 8);
            nameSearchBox.Name = "nameSearchBox";
            nameSearchBox.Size = new Size(199, 23);
            nameSearchBox.TabIndex = 7;
            nameSearchBox.TextChanged += nameSearchBox_TextChanged;
            // 
            // classSearchBox
            // 
            classSearchBox.Location = new Point(58, 37);
            classSearchBox.Name = "classSearchBox";
            classSearchBox.Size = new Size(199, 23);
            classSearchBox.TabIndex = 8;
            classSearchBox.TextChanged += classSearchBox_TextChanged;
            // 
            // ClassViewerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1072, 450);
            Controls.Add(classSearchBox);
            Controls.Add(nameSearchBox);
            Controls.Add(refreshInstanceButton);
            Controls.Add(instanceSelection);
            Controls.Add(splitContainer);
            Controls.Add(instanceLabel);
            Controls.Add(classFilterLabel);
            Controls.Add(NameFilterLabel);
            Name = "ClassViewerForm";
            Text = "ClassViewerForm";
            Load += ClassViewerForm_Load;
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label NameFilterLabel;
        private Label classFilterLabel;
        private Label instanceLabel;
        private ListView fullPropertiesListView;
        private ColumnHeader path;
        private SplitContainer splitContainer;
        private ColumnHeader classCol;
        private ListView selPropertiesListView;
        private ColumnHeader nameCol;
        private ColumnHeader selClassCol;
        private ComboBox instanceSelection;
        private ColumnHeader offsetCol;
        private ColumnHeader addressCol;
        private ColumnHeader valueCol;
        private Button refreshInstanceButton;
        private TextBox nameSearchBox;
        private TextBox classSearchBox;
    }
}