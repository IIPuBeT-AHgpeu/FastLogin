using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace FastLoginSettings
{
    public partial class FastLoginSettingsForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int ALTI_HOTKEY_ID = 1;
        const int ALTO_HOTKEY_ID = 2;
        const int ALTP_HOTKEY_ID = 3;

        static readonly string MY_DOCS_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public Model model = new Model(MY_DOCS_PATH);

        public NotifyIcon notifyIcon;

        public FormWindowState OldFormState;

        public FastLoginSettingsForm()
        {
            InitializeComponent();

            //Если приложение уже запущено, то не даем запустить еще один процесс
            if (Process.GetProcesses().Count(x => x.ProcessName == "FastLoginSettings") > 1)
                Process.GetCurrentProcess().Kill();

            OldFormState = FormWindowState.Normal;

            RegisterHotKey(Handle, ALTI_HOTKEY_ID, 1, (int)Keys.I);
            RegisterHotKey(Handle, ALTO_HOTKEY_ID, 1, (int)Keys.O);
            RegisterHotKey(Handle, ALTP_HOTKEY_ID, 1, (int)Keys.P);

            notifyIconInitialization();

            model.loadData(MY_DOCS_PATH);
        }

        private void AcceptBtn_Click(object sender, EventArgs e)
        {
            showApplyDialog();
        }

        private void FastLoginSettingsForm_Load(object sender, EventArgs e)
        {
            setInputsAndSettings();

            enableChangesButtons(false);
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            setInputsAndSettings();

            enableChangesButtons(false);
        }

        private void AltIInput_TextChanged(object sender, EventArgs e)
        {
            enableChangesButtons(checkInputsAndSettingsChanges());
        }

        private void AltOInput_TextChanged(object sender, EventArgs e)
        {
            enableChangesButtons(checkInputsAndSettingsChanges());
        }

        private void AltPInput_TextChanged(object sender, EventArgs e)
        {
            enableChangesButtons(checkInputsAndSettingsChanges());
        }

        private void AutorunCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            enableChangesButtons(checkInputsAndSettingsChanges());
        }

        //Заполняет все поля формы данными из модели.
        private void setInputsAndSettings()
        {
            AltIInput.Text = model.data.FirstString;
            AltOInput.Text = model.data.SecondString;
            AltPInput.Text = model.data.ThirdString;

            AutorunCheckBox.Checked = model.data.Autorun;
        }

        //Сравнивает значения полей из формы со значениями в модели.
        private bool checkInputsAndSettingsChanges()
        {
            if (AltIInput.Text == model.data.FirstString && AltOInput.Text == model.data.SecondString &&
                AltPInput.Text == model.data.ThirdString && AutorunCheckBox.Checked == model.data.Autorun)
                return false;
            else return true;
        }

        //Включает/отключает кнопки принятия и отмены изменений.
        private void enableChangesButtons(bool enable)
        {
            CancelBtn.Enabled = enable;
            AcceptBtn.Enabled = enable;
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            showApplyDialog();

            DialogResult exitDialog = MessageBox.Show("Do you want to close FastLogin?", "Exit FastLogin",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (exitDialog == DialogResult.OK)
            {
                notifyIcon.Dispose();
                Process.GetCurrentProcess().Kill();
            }
        }

        private void HideBtn_Click(object sender, EventArgs e)
        {
            if (!showApplyDialog()) setInputsAndSettings();
            hideWindow();
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                switch(m.WParam.ToInt32())
                {
                    case ALTI_HOTKEY_ID:
                        {
                            if (!(model.data.FirstString == ""))
                            {
                                //toast maybe
                                Clipboard.Clear();
                                Clipboard.SetText(model.data.FirstString);
                            }
                            break;
                        }
                    case ALTO_HOTKEY_ID:
                        {
                            if (!(model.data.SecondString == ""))
                            {
                                //toast maybe
                                Clipboard.Clear();
                                Clipboard.SetText(model.data.SecondString);
                            }
                            break;
                        }
                    case ALTP_HOTKEY_ID:
                        {
                            if (!(model.data.ThirdString == ""))
                            {
                                //toast maybe
                                Clipboard.Clear();
                                Clipboard.SetText(model.data.ThirdString);
                            }
                            break;
                        }
                }
            }
            base.WndProc(ref m);
        }

        //Инициализирует элемент в трее.
        private void notifyIconInitialization()
        {
            notifyIcon = new NotifyIcon();
            //path
            notifyIcon.Icon = new Icon("C:\\VS_Projects\\FastLoginSettings\\FastLoginSettings\\pictures\\FastLogin.ico");
            notifyIcon.DoubleClick += (snr, args) =>
            {
                showWindow();
            };
            notifyIcon.Text = "FastLogin";
            notifyIcon.Visible = false;
            notifyIcon.ContextMenuStrip = new ContextMenuStrip();

            notifyIcon.ContextMenuStrip.Items.Add("Open", 
                Image.FromFile("C:\\VS_Projects\\FastLoginSettings\\FastLoginSettings\\pictures\\context-open-icon.png"), 
                this.showWindowContext);

            notifyIcon.ContextMenuStrip.Items.Add("Exit", 
                Image.FromFile("C:\\VS_Projects\\FastLoginSettings\\FastLoginSettings\\pictures\\context-exit-icon.png"),
                this.ExitBtn_Click);

        }

        private void showWindowContext(object sender, EventArgs e)
        {
            showWindow();
        }

        //Сворачивает окно в трей.
        private void hideWindow()
        {
            Hide();
            OldFormState = WindowState;
            notifyIcon.Visible = true;
        }

        //Разворачивает окно из трея.
        private void showWindow()
        {
            Show();
            WindowState = OldFormState;
            notifyIcon.Visible = false;
        }

        //При нажатии на крестик, сворачиваем окно в трей
        private void FastLoginSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            //Если данные не были сохранены, то отменяем изменения на формах
            if (!showApplyDialog()) setInputsAndSettings();
            
            hideWindow();
        }

        //Вызывает диалог с предложением сохранения изменений, если таковые имеются
        //возвращает булевый результат, было ли выполнено сохранение
        private bool showApplyDialog()
        {
            if(checkInputsAndSettingsChanges())
            {
                DialogResult applyDialog = MessageBox.Show("Do you want to apply the changes?", "Apply changes",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if (applyDialog == DialogResult.OK)
                {
                    if (!(SetAutorunValue(!model.data.Autorun))) 
                        throw new Exception("Can not to add/delete the app in autorun");
                    
                    model.setData(AutorunCheckBox.Checked, AltIInput.Text,
                        AltOInput.Text, AltPInput.Text);
                    model.saveData(MY_DOCS_PATH);

                    enableChangesButtons(false);

                    return true;
                }
                else return false;
            }
            return false;
        }

        public bool SetAutorunValue(bool autorun)
        {
            string ExePath = Application.ExecutablePath;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                if (autorun)
                    reg.SetValue("FastLogin", ExePath);
                else
                    reg.DeleteValue("FastLogin");

                reg.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void FastLoginSettingsForm_Shown(object sender, EventArgs e)
        {
            if (!model.data.FirstStart) hideWindow();
        }
    }
}
