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
            integerVal.Select(0, integerVal.Value.ToString().Length);
        }

        private void writeButton_Click(object sender, EventArgs e)
        {
            ExitForm(true);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            ExitForm();
        }

        private void integerVal_KeyPress(object sender, KeyPressEventArgs e)
        {
            // On pressing enter
            if (e.KeyChar == (char)Keys.Enter)
                ExitForm(true);
        }

        private void ExitForm(bool setValue = false)
        {
            if (setValue)
                pd.value = (int)integerVal.Value;
            this.Close();
        }
    }
}
