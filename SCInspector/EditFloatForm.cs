namespace SCInspector
{
    public partial class EditFloatForm : Form
    {
        FloatPropertyData pd;

        public EditFloatForm(FloatPropertyData _pd, string propertyName)
        {
            InitializeComponent();
            valueLabel.Text = propertyName;
            pd = _pd;
            floatVal.Value = (decimal)pd.value;
        }

        private void writeButton_Click(object sender, EventArgs e)
        {
            pd.value = (float)floatVal.Value;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
