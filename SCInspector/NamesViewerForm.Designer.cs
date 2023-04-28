namespace SCInspector
{
    partial class NamesViewerForm
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
            namesListView = new ListView();
            indexCol = new ColumnHeader();
            nameCol = new ColumnHeader();
            SuspendLayout();
            // 
            // namesListView
            // 
            namesListView.Columns.AddRange(new ColumnHeader[] { indexCol, nameCol });
            namesListView.FullRowSelect = true;
            namesListView.Location = new Point(12, 12);
            namesListView.Name = "namesListView";
            namesListView.Size = new Size(565, 686);
            namesListView.TabIndex = 0;
            namesListView.UseCompatibleStateImageBehavior = false;
            namesListView.View = View.Details;
            // 
            // indexCol
            // 
            indexCol.Text = "Index";
            indexCol.Width = 80;
            // 
            // nameCol
            // 
            nameCol.Text = "Name";
            nameCol.Width = 480;
            // 
            // NamesViewerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(589, 710);
            Controls.Add(namesListView);
            Name = "NamesViewerForm";
            Text = "Names Viewer";
            Load += NamesViewerForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListView namesListView;
        private ColumnHeader indexCol;
        private ColumnHeader nameCol;
    }
}