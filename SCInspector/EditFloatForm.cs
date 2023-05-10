using SCInspector.Unreal;

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
            floatVal.Select(0, floatVal.Value.ToString().Length);
        }

        private void writeButton_Click(object sender, EventArgs e)
        {
            ExitForm(true);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            ExitForm();
        }

        private void FloatVal_KeyPress(object sender, KeyPressEventArgs e)
        {
            // On pressing enter
            if (e.KeyChar == (char)Keys.Enter)
                ExitForm(true);
        }

        private void ExitForm(bool setValue = false)
        {
            if (setValue)
                pd.value = (float)floatVal.Value;
            this.Close();
        }
    }
}
