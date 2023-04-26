using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCInspector
{
    public partial class ClassViewerForm : Form
    {
        private GameData gameData;
        private int index;
        private GameObject gameObject;

        public ClassViewerForm(int _index, GameData _gameData)
        {
            InitializeComponent();
            gameData = _gameData;
            index = _index;
            if (gameData.objects.ContainsKey(index))
                gameObject = gameData.objects[index];
        }

        private void ClassViewerForm_Load(object sender, EventArgs e)
        {
            if (gameObject.type == ObjectType.GameObject)
            {
                objectTypeLabel.Text = "GameObject";

                if (gameData.objects.ContainsKey(gameObject.classIndex))
                    classLabel.Text = String.Format("Class: {0}", gameData.objects[gameObject.classIndex].fullPath);

                if (gameData.objects.ContainsKey(gameObject.inheritedClassIndex))
                    inheritsLabel.Text = String.Format("Inherits from: {0}", gameData.objects[gameObject.inheritedClassIndex].fullPath);
            }
            else
            {
                objectTypeLabel.Text = "Instance";

                if (gameData.objects.ContainsKey(gameObject.classIndex))
                    classLabel.Text = String.Format("Class: {0}", gameData.objects[gameObject.classIndex].fullPath);

                inheritsLabel.Text = "";
            }
        }
    }
}
