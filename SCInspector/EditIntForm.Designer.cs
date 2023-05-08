namespace SCInspector
{
    partial class EditIntForm
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
            integerVal = new NumericUpDown();
            valueLabel = new Label();
            writeButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)integerVal).BeginInit();
            SuspendLayout();
            // 
            // integerVal
            // 
            integerVal.Location = new Point(60, 68);
            integerVal.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            integerVal.Minimum = new decimal(new int[] { int.MinValue, 0, 0, int.MinValue });
            integerVal.Name = "integerVal";
            integerVal.Size = new Size(162, 23);
            integerVal.TabIndex = 0;
            integerVal.KeyPress += integerVal_KeyPress;
            // 
            // valueLabel
            // 
            valueLabel.AutoSize = true;
            valueLabel.Location = new Point(60, 50);
            valueLabel.Name = "valueLabel";
            valueLabel.Size = new Size(35, 15);
            valueLabel.TabIndex = 1;
            valueLabel.Text = "value";
            // 
            // writeButton
            // 
            writeButton.Location = new Point(32, 116);
            writeButton.Name = "writeButton";
            writeButton.Size = new Size(107, 33);
            writeButton.TabIndex = 2;
            writeButton.Text = "Write";
            writeButton.UseVisualStyleBackColor = true;
            writeButton.Click += writeButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(145, 116);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(107, 33);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // EditIntForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 161);
            Controls.Add(cancelButton);
            Controls.Add(writeButton);
            Controls.Add(valueLabel);
            Controls.Add(integerVal);
            Name = "EditIntForm";
            Text = "Edit IntProperty";
            ((System.ComponentModel.ISupportInitialize)integerVal).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NumericUpDown integerVal;
        private Label valueLabel;
        private Button writeButton;
        private Button cancelButton;
    }
}