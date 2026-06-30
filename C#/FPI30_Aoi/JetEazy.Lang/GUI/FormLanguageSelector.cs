using JetEazy.Lang;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace JetEazy.Lang.GUI
{
    public partial class FormLanguageSelector : Form
    {
        #region PRIVATE_DATA
        QxLang _langPack;
        #endregion

        public FormLanguageSelector(QxLang lang)
        {
            InitializeComponent();
            _langPack = lang;

            #region GUI_CONTROLS_EVENTS
            this.Load += new EventHandler(Form_Load);
            btnOK.Click += new EventHandler(btnOK_Click);
            btnCancel.Click += new EventHandler(btnCancel_Click);
            #endregion

            _updateList();
        }

        #region WINDOW_EVENT_HANDLERS
        void Form_Load(object sender, EventArgs e)
        {
            _autoLayout();
        }
        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            var idx = listBox1.SelectedIndex;
            if (idx >= 0)
            {
                _langPack.LanguageID = idx;
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void _updateList()
        {
            listBox1.Items.Clear();
            //var values = Enum.GetValues(typeof(Language));
            //foreach(Language value in values)
            //{
            //    var text = JetEazy.QxNums.GetEnumDescription(value);
            //    listBox1.Items.Add(text);
            //}
            var availableLangs = _langPack.GetAvailableLanguages();
            foreach (string lang in availableLangs)
            {
                listBox1.Items.Add(lang);
            }
            try
            {
                listBox1.SelectedIndex = (int)_langPack.LanguageID;
            }
            catch
            {

            }
        }
        private void _autoLayout()
        {
            Rectangle rc = ClientRectangle;
            if (rc.Height < btnCancel.Bottom + 8)
            {
                int dy = btnCancel.Bottom + 8 - rc.Bottom;
                this.Height += dy;
            }
        }
        #endregion
    }
}