namespace SCInspector
{
    public partial class EditIntForm : Form
    {
        IntPropertyData pd;

        public EditIntForm(IntPropertyData _pd, string propertyName)
        {
            InitializeComponent();
            valueLabel.Text = propertyName;
            pd = _pd;
            integerVal.Value = pd.value;
        }

        private void writeButton_Click(object sender, EventArgs e)
        {
            pd.value = (int)integerVal.Value;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
