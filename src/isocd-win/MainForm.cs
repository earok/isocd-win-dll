using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using isocd_builder;
using System.Media;
using System.Drawing;
using System.Diagnostics;

namespace isocd_win {
    public partial class ISOCDWin : Form {
        const string COULD_NOT_FIND_TM_FILES = "Could not find valid trademark files. Would you like to download them?";
        const string COULD_NOT_DOWNLOAD_TM_FILES = "Could not download trademark files - aborting!";
        const string ABORT_BUILD_WARNING_MESSAGE = "Are you sure you want to abort the build process?";
        const string OVERWRITE_ISO_WARNING_MESSAGE = "Are you sure you want to overwrite the ISO file '{0}'?";
        const string OVERWRITE_LOG_WARNING_MESSAGE = "Are you sure you want to overwrite the LOG file '{0}'?";
        const string MUST_SELECT_SOURCE_AND_IMAGE_ERROR_MESSAGE = "You must select both a source folder and image file.";
        const string SOURCE_MUST_BE_VALID_ERROR_MESSAGE = "Source must be a valid folder.";
        const string IMAGE_MUST_BE_VALID_ERROR_MESSAGE = "Image must be a valid file.";
        const string BROWSE_FOR_ISO_FILE_INFO_MESSAGE = "Browse for ISO file";
        const string BROWSE_FOR_TEMPLATE_FILE_INFO_MESSAGE = "Browse for template file";
        const string BUILD_ISO_AND_SAVE_TEMPLATE_BUTTON = "Build ISO and save template";
        const string ABORT_BUTTON = "Abort";
        const string SCANNING_DIRECTORIES_STATUS_LABEL = "Scanning directories...";
        const string DONE_STATUS_LABEL = "Done!";
        const string ABORTED_STATUS_LABEL = "Aborted!";
        const string DOWNLOADING_TRADEMARK_FILES_STATUS_LABEL = "Downloading trademark files, please wait...";
        const string ERROR_MESSAGE_BOX = "Error";
        const string WARNING_MESSAGE_BOX = "Warning";

        AppStates appState;

        readonly ConfigManager configManager = new ConfigManager();

#pragma warning disable IDE0069 // These are disposed in the FormClosed event handler, but is missed by IntelliSense
        private SoundPlayer successSound;
        private SoundPlayer errorSound;
#pragma warning restore IDE0069

        readonly BuildIsoWorker worker;

        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        // Consts to allow window dragging using ReleaseCapture() and SendMessage()
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x02;

        public ISOCDWin() {
            InitializeComponent();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Text += $" v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            targetSystemComboBox.DataSource = Enum.GetValues(typeof(TargetSystemType));

            loadSetting("");

            try {
                worker = new BuildIsoWorker();
                worker.WorkerUpdateEvent += WorkerUpdate;
                worker.WorkerCompletedEvent += WorkerCompleted;
            }
            catch(Exception ex) {
                ShowError(ex.Message);
            }
        }

        void SetSystemLogo() {
            switch(targetSystemComboBox.SelectedIndex) {
                case (int)TargetSystemType.CD32:
                    targetSystemPictureBox.Image = Resources.amiga_cd32;
                    break;
                case (int)TargetSystemType.CDTV:
                    targetSystemPictureBox.Image = Resources.commodore_cdtv;
                    break;
                default:
                    targetSystemPictureBox.Image = Resources.amiga;
                    break;
            }
        }

        async void StartBuildButton_Click(object sender, EventArgs e) {

            configManager.Options.GenerateOrderFile = false;

            statusLabel.ForeColor = Color.Blue;

            if(appState == AppStates.BuildingIso) {
                if(ShowWarning(ABORT_BUILD_WARNING_MESSAGE) == DialogResult.Yes) {
                    statusLabel.Text = "Aborting...";
                    worker.StopWork();
                }
                return;
            }

            if (!checkEnteredParameters())
                return;

            if(File.Exists(imgTextBox.Text)) {
                if(ShowWarning(string.Format(OVERWRITE_ISO_WARNING_MESSAGE, imgTextBox.Text)) == DialogResult.No) {
                    return;
                }
            }

            SetAppState(AppStates.BuildingIso);
            UpdateConfigFromForm();

            await worker.StartWorkAsync(configManager.Options);

            SetAppState(AppStates.Idle);
        }

        void SrcBrowseButton_Click(object sender, EventArgs e) {
            using(var frm = new OpenFolderDialog()) {
                frm.InitialFolder = srcTextBox.Text;

                if(frm.ShowDialog(this) == DialogResult.OK) {
                    srcTextBox.Text = frm.Folder;
                }
            }
        }

        void ImgBrowseButton_Click(object sender, EventArgs e) {
            var path = !string.IsNullOrWhiteSpace(imgTextBox.Text) ?
                Path.GetDirectoryName(imgTextBox.Text) :
                Directory.GetCurrentDirectory();

            string file;

            if(!string.IsNullOrWhiteSpace(imgTextBox.Text)) {
                file = Path.GetFileName(imgTextBox.Text);
            }
            else {
                switch (targetSystemComboBox.SelectedIndex)
                {
                    case (int)TargetSystemType.CD32:
                        file = "cd32.iso";
                        break;
                    case (int)TargetSystemType.CDTV:
                        file = "cdtv.iso";
                        break;
                    default:
                        file = "amiga.iso";
                        break;
                }
            }

            using(var fdlg = new OpenFileDialog {
                Title = BROWSE_FOR_ISO_FILE_INFO_MESSAGE,
                Filter = "ISO files (*.iso)|*.iso",
                FilterIndex = 1,
                RestoreDirectory = true,
                CheckFileExists = false,
                CheckPathExists = true,
                InitialDirectory = path,
                FileName = file
            }) {
                if(fdlg.ShowDialog() == DialogResult.OK) {
                    imgTextBox.Text = fdlg.FileName;
                }
            }
        }

        private void settingsBrowseButton_Click(object sender, EventArgs e)
        {
            var path = !string.IsNullOrWhiteSpace(settingsTextBox.Text) ?
                Path.GetDirectoryName(settingsTextBox.Text) :
                Directory.GetCurrentDirectory();

            string file;

            if (!string.IsNullOrWhiteSpace(settingsTextBox.Text))
            {
                file = Path.GetFileName(settingsTextBox.Text);
            }
            else
            {
                file = $"{Path.GetFileNameWithoutExtension(Application.ExecutablePath)}.xml";
            }

            using (var fdlg = new OpenFileDialog
            {
                Title = BROWSE_FOR_TEMPLATE_FILE_INFO_MESSAGE,
                Filter = "Template files (*.xml)|*.xml",
                FilterIndex = 1,
                RestoreDirectory = true,
                CheckFileExists = false,
                CheckPathExists = true,
                InitialDirectory = path,
                FileName = file
            })
            {
                if (fdlg.ShowDialog() == DialogResult.OK)
                {
                    settingsTextBox.Text = fdlg.FileName;
                }
            }
            if (File.Exists(settingsTextBox.Text))
                loadSetting(settingsTextBox.Text);
        }

        void OptionsButton_Click(object sender, EventArgs e) {
            using(var optionsForm = new optionsForm(configManager.Options)) {
                optionsForm.ShowDialog();
            }
        }

        void UpdateConfigFromForm() {
            configManager.Options.InputFolder = srcTextBox.Text;
            configManager.Options.OutputFile = imgTextBox.Text;
            configManager.Options.TargetSystem = (TargetSystemType)targetSystemComboBox.SelectedIndex;

            switch (targetSystemComboBox.SelectedIndex)
            {
                case (int)TargetSystemType.CD32:
                    configManager.Options.Trademark = true;
                    configManager.Options.TrademarkFile = Path.Combine(isocd_builder_constants.ISOCDWIN_PUBLIC_DOCUMENTS_PATH, isocd_builder_constants.CD32_TRADEMARK_FILE);
                    break;
                case (int)TargetSystemType.CDTV:
                    configManager.Options.Trademark = true;
                    configManager.Options.TrademarkFile = Path.Combine(isocd_builder_constants.ISOCDWIN_PUBLIC_DOCUMENTS_PATH, isocd_builder_constants.CDTV_TRADEMARK_FILE);
                    break;
                default:
                    configManager.Options.Trademark = false;
                    configManager.Options.TrademarkFile = "";
                    break;
            }

            configManager.SaveConfig(settingsTextBox.Text);
        }

        void ISOCDWin_MouseDown(object sender, MouseEventArgs e) {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        DialogResult ShowWarning(string message) {
            return MessageBox.Show(null, message, WARNING_MESSAGE_BOX, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        void ShowError(string message) {
            MessageBox.Show(message, ERROR_MESSAGE_BOX, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        void ISOCDWin_FormClosing(object sender, FormClosingEventArgs e) {
            if(appState == AppStates.BuildingIso && ShowWarning(ABORT_BUILD_WARNING_MESSAGE) == DialogResult.No) {
                e.Cancel = true;
                return;
            }

            UpdateConfigFromForm();
        }

        void ISOCDWin_FormClosed(object sender, FormClosedEventArgs e) {
            successSound.Dispose();
            errorSound.Dispose();
        }

        void SetAppState(AppStates newState) {
            appState = newState;
            SetControlState(newState == AppStates.Idle);
        }

        void SetControlState(bool enabled) {
            if(enabled) {
                startBuildButton.Text = BUILD_ISO_AND_SAVE_TEMPLATE_BUTTON;
            }
            else {
                statusLabel.Text = SCANNING_DIRECTORIES_STATUS_LABEL;
                startBuildButton.Text = ABORT_BUTTON;
            }

            progressBar.Value = 0;
            progressBar.SetDrawing(enabled == false);

            srcTextBox.Enabled = enabled;
            srcBrowseButton.Enabled = enabled;
            imgTextBox.Enabled = enabled;
            imgBrowseButton.Enabled = enabled;
            optionsButton.Enabled = enabled;
        }

        void WorkerUpdate(object sender, WorkerUpdateEventArgs e) {
            if(!string.IsNullOrWhiteSpace(e.State.StatusMessage)) {
                statusLabel.Text = e.State.StatusMessage;
            }
            else {
                progressBar.Value = e.State.Progress;
            }
        }

        void WorkerCompleted(object sender, WorkerCompletedEventArgs e) {
            switch(e.Status) {
                case WorkerCompletedStatus.Success:
                    statusLabel.ForeColor = Color.DarkGreen;
                    statusLabel.Text = DONE_STATUS_LABEL;

                    if(configManager.Options.PlaySounds) {
                        successSound.Play();
                    }

                    if(configManager.Options.WinUAETest && configManager.Options.WinUAEPath.Contains("winuae") && configManager.Options.WinUAEPath.Contains(".exe") && configManager.Options.WinUAEConfigPath != "")
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = configManager.Options.WinUAEPath,
                            Arguments = "-G -config=" + configManager.Options.WinUAEConfigPath + " -cdimage=" + configManager.Options.OutputFile,
                            UseShellExecute = false
                        };

                        Process.Start(psi);
                    }

                    break;

                case WorkerCompletedStatus.Error:
                    statusLabel.ForeColor = Color.Red;
                    statusLabel.Text = e.Exception.Message;

                    if(configManager.Options.PlaySounds) {
                        errorSound.Play();
                    }

                    break;

                case WorkerCompletedStatus.Cancelled:
                    statusLabel.ForeColor = Color.Blue;
                    statusLabel.Text = ABORTED_STATUS_LABEL;
                    break;
            }

            SetAppState(AppStates.Idle);
        }

        void ISOCDWin_Shown(object sender, EventArgs e) {
            // Make sure everything is setup ready to use the isocd-win-builder library
            var builderSetupResult = BuilderSetupHelper.Setup();

            if(!builderSetupResult.HaveTmFiles) {
                var response = ShowWarning(COULD_NOT_FIND_TM_FILES);

                if(response == DialogResult.Yes) {
                    statusLabel.Text = DOWNLOADING_TRADEMARK_FILES_STATUS_LABEL;
                    builderSetupResult.HaveTmFiles = TmFileHelper.DownloadTmFiles();
                    statusLabel.Text = "";
                }

                if(!builderSetupResult.HaveTmFiles) {
                    if(response == DialogResult.Yes) {
                        ShowError(COULD_NOT_DOWNLOAD_TM_FILES);
                    }

                    Application.Exit();
                }
            }
        }

        private void useTmFileCheckBox_CheckedChanged(object sender, EventArgs e) {
            useTmFileCheckBox.Enabled = useTmFileCheckBox.Checked;

            if(!useTmFileCheckBox.Checked) {
                targetSystemComboBox.SelectedIndex = (int)TargetSystemType.Amiga;
            }

            SetSystemLogo();
        }

        private void targetSystemComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            useTmFileCheckBox.Checked = useTmFileCheckBox.Enabled = targetSystemComboBox.SelectedIndex != (int)TargetSystemType.Amiga;
            configManager.Options.TargetSystem = configManager.Options.TargetSystem = (TargetSystemType)targetSystemComboBox.SelectedIndex;
            SetSystemLogo();
        }

        private void srcTextBox_TextChanged(object sender, EventArgs e)
        {
            configManager.Options.InputFolder = srcTextBox.Text;
        }

        private void imgTextBox_TextChanged(object sender, EventArgs e)
        {
            configManager.Options.OutputFile = imgTextBox.Text;
        }

        void Any_Characters_check(object sender, KeyPressEventArgs e)
        {

            e.Handled = true;

            base.OnKeyPress(e);
        }

        async private void GenerateOrderFileButton_Click(object sender, EventArgs e)
        {
            if (!checkEnteredParameters())
                return;

            configManager.Options.GenerateOrderFile = true;
            configManager.Options.UseOrderFile = false;

            if (File.Exists(srcTextBox.Text + '\\' + "ISOCD_" + configManager.Options.VolumeId + ".log"))
            {
                if (ShowWarning(string.Format(OVERWRITE_LOG_WARNING_MESSAGE, srcTextBox.Text + '\\' + "ISOCD_" + configManager.Options.VolumeId + ".log")) == DialogResult.No)
                {
                    return;
                }
            }

            SetAppState(AppStates.BuildingIso);
            UpdateConfigFromForm();

            await worker.StartWorkAsync(configManager.Options);

            SetAppState(AppStates.Idle);
        }

        private bool checkEnteredParameters()
        {
            if (string.IsNullOrWhiteSpace(srcTextBox.Text) || string.IsNullOrWhiteSpace(imgTextBox.Text))
            {
                ShowError(MUST_SELECT_SOURCE_AND_IMAGE_ERROR_MESSAGE);
                return false;
            }

            if (!Directory.Exists(srcTextBox.Text))
            {
                ShowError(SOURCE_MUST_BE_VALID_ERROR_MESSAGE);
                return false;
            }

            if (!Directory.Exists(Path.GetDirectoryName(imgTextBox.Text)))
            {
                ShowError(IMAGE_MUST_BE_VALID_ERROR_MESSAGE);
                return false;
            }

            if (!configManager.Options.IsValid())
            {
                MessageBox.Show(configManager.Options.ValidationResult().Message, ERROR_MESSAGE_BOX, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

             return true;
        }

        private void loadSetting(string _stettings)
        {
            try
            {
                var loadedConfig = configManager.LoadConfig(_stettings);

                if (!loadedConfig)
                {
                    // Default to CD32 on first run
                    configManager.Options.TargetSystem = TargetSystemType.CD32;
                    configManager.Options.TrademarkFile = Path.Combine(isocd_builder_constants.ISOCDWIN_PUBLIC_DOCUMENTS_PATH, isocd_builder_constants.CD32_TRADEMARK_FILE);
                    SetSystemLogo();
                }

                srcTextBox.Text = configManager.Options.InputFolder;
                imgTextBox.Text = configManager.Options.OutputFile;
                settingsTextBox.Text = configManager.m_sConfigFileName;
                useTmFileCheckBox.Checked = configManager.Options.Trademark;
                targetSystemComboBox.SelectedItem = configManager.Options.TargetSystem;

                useTmFileCheckBox_CheckedChanged(null, null);

                if (File.Exists(configManager.Options.TrademarkFile))
                {
                    SetSystemLogo();
                }

                successSound = new SoundPlayer(Resources.success);
                successSound.Load();
                errorSound = new SoundPlayer(Resources.error);
                errorSound.Load();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void progressBar_Click(object sender, EventArgs e)
        {

        }

        private void statusLabel_Click(object sender, EventArgs e)
        {

        }

        private void targetSystemPictureBox_Click(object sender, EventArgs e)
        {

        }

        private void targetSystemSettingsLabel_Click(object sender, EventArgs e)
        {

        }

        private void ISOCDWin_Load(object sender, EventArgs e)
        {

        }

        private void targetSystemGroupBox_Paint(object sender, PaintEventArgs e)
        {

        }

        private void buildStatusLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
