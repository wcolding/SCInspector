namespace SCInspector
{
    partial class EditFloatForm
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
            floatVal = new NumericUpDown();
            valueLabel = new Label();
            writeButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)floatVal).BeginInit();
            SuspendLayout();
            // 
            // floatVal
            // 
            floatVal.DecimalPlaces = 2;
            floatVal.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            floatVal.Location = new Point(60, 68);
            floatVal.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            floatVal.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            floatVal.Name = "floatVal";
            floatVal.Size = new Size(162, 23);
            floatVal.TabIndex = 0;
            floatVal.KeyPress += FloatVal_KeyPress;
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
            // EditFloatForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 161);
            Controls.Add(cancelButton);
            Controls.Add(writeButton);
            Controls.Add(valueLabel);
            Controls.Add(floatVal);
            Name = "EditFloatForm";
            Text = "Edit FloatProperty";
            ((System.ComponentModel.ISupportInitialize)floatVal).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NumericUpDown floatVal;
        private Label valueLabel;
        private Button writeButton;
        private Button cancelButton;
    }
}