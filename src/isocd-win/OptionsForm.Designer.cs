namespace isocd_win {
    partial class optionsForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(optionsForm));
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.volumeIdLabel = new System.Windows.Forms.Label();
            this.volumeIdTextBox = new System.Windows.Forms.TextBox();
            this.dataCacheLabel = new System.Windows.Forms.Label();
            this.dataCacheSectorsLabel = new System.Windows.Forms.Label();
            this.dirCacheSectorsLabel = new System.Windows.Forms.Label();
            this.dirCacheLabel = new System.Windows.Forms.Label();
            this.fileLockNodesLabel = new System.Windows.Forms.Label();
            this.fileLockLlabel = new System.Windows.Forms.Label();
            this.fileHandleNodesLabel = new System.Windows.Forms.Label();
            this.fileHandleLabel = new System.Windows.Forms.Label();
            this.retriesLabel = new System.Windows.Forms.Label();
            this.directReadCheckBox = new System.Windows.Forms.CheckBox();
            this.fastSearchCheckBox = new System.Windows.Forms.CheckBox();
            this.speedIndCheckBox = new System.Windows.Forms.CheckBox();
            this.restoreDefaultsButton = new System.Windows.Forms.Button();
            this.playSoundsCheckBox = new System.Windows.Forms.CheckBox();
            this.volSetIdLabel = new System.Windows.Forms.Label();
            this.volumeSetIdTextBox = new System.Windows.Forms.TextBox();
            this.publisherIdLabel = new System.Windows.Forms.Label();
            this.publisherIdTextBox = new System.Windows.Forms.TextBox();
            this.dataPreparerIdLabel = new System.Windows.Forms.Label();
            this.dataPreparerIdTextBox = new System.Windows.Forms.TextBox();
            this.applicationIdLabel = new System.Windows.Forms.Label();
            this.applicationIdTextBox = new System.Windows.Forms.TextBox();
            this.imagePaddingComboBox = new System.Windows.Forms.ComboBox();
            this.imagePaddingLabel = new System.Windows.Forms.Label();
            this.WinUAETestCheckBox = new System.Windows.Forms.CheckBox();
            this.cdfsGroupBox = new System.Windows.Forms.GroupBox();
            this.isoGroupBox = new System.Windows.Forms.GroupBox();
            this.generalGroupBox = new System.Windows.Forms.GroupBox();
            this.useOrderFileCheckBox = new System.Windows.Forms.CheckBox();
            this.winUAEBrowseButton = new System.Windows.Forms.Button();
            this.winUAEPathLabel = new System.Windows.Forms.Label();
            this.winUAEPathTextBox = new System.Windows.Forms.TextBox();
            this.HelpTextBox = new System.Windows.Forms.TextBox();
            this.dirCacheTextBox = new isocd_win.ValueBox();
            this.dataCacheTextBox = new isocd_win.ValueBox();
            this.fileLockTextBox = new isocd_win.ValueBox();
            this.fileHandleTextBox = new isocd_win.ValueBox();
            this.retriesTextBox = new isocd_win.ValueBox();
            this.cdfsGroupBox.SuspendLayout();
            this.isoGroupBox.SuspendLayout();
            this.generalGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(230, 394);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 28);
            this.okButton.TabIndex = 42;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(335, 394);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 28);
            this.cancelButton.TabIndex = 43;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // volumeIdLabel
            // 
            this.volumeIdLabel.AutoSize = true;
            this.volumeIdLabel.Location = new System.Drawing.Point(56, 25);
            this.volumeIdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.volumeIdLabel.Name = "volumeIdLabel";
            this.volumeIdLabel.Size = new System.Drawing.Size(72, 17);
            this.volumeIdLabel.TabIndex = 20;
            this.volumeIdLabel.Text = "Volume ID";
            // 
            // volumeIdTextBox
            // 
            this.volumeIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.volumeIdTextBox.Location = new System.Drawing.Point(132, 22);
            this.volumeIdTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.volumeIdTextBox.MaxLength = 32;
            this.volumeIdTextBox.Name = "volumeIdTextBox";
            this.volumeIdTextBox.Size = new System.Drawing.Size(345, 22);
            this.volumeIdTextBox.TabIndex = 21;
            this.volumeIdTextBox.Tag = "This field shall specify an identification of the volume. Allowed 32 d-characters" +
    ": A B C D E F G H I J K L M N O P Q R S T U V W X Y Z 0 1 2 3 4 5 6 7 8 9 _";
            this.volumeIdTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            this.volumeIdTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.D_Characters_check);
            // 
            // dataCacheLabel
            // 
            this.dataCacheLabel.AutoSize = true;
            this.dataCacheLabel.Location = new System.Drawing.Point(33, 25);
            this.dataCacheLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.dataCacheLabel.Name = "dataCacheLabel";
            this.dataCacheLabel.Size = new System.Drawing.Size(82, 17);
            this.dataCacheLabel.TabIndex = 2;
            this.dataCacheLabel.Tag = "CDFS allocates a read-ahead cache to improve performance. The size of this cache " +
    "is specified in CD sectors (2K bytes each). The default size is 8 blocks, min.1 " +
    "- max.127.";
            this.dataCacheLabel.Text = "Data Cache";
            // 
            // dataCacheSectorsLabel
            // 
            this.dataCacheSectorsLabel.AutoSize = true;
            this.dataCacheSectorsLabel.Location = new System.Drawing.Point(264, 25);
            this.dataCacheSectorsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.dataCacheSectorsLabel.Name = "dataCacheSectorsLabel";
            this.dataCacheSectorsLabel.Size = new System.Drawing.Size(56, 17);
            this.dataCacheSectorsLabel.TabIndex = 4;
            this.dataCacheSectorsLabel.Text = "Sectors";
            // 
            // dirCacheSectorsLabel
            // 
            this.dirCacheSectorsLabel.AutoSize = true;
            this.dirCacheSectorsLabel.Location = new System.Drawing.Point(264, 71);
            this.dirCacheSectorsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.dirCacheSectorsLabel.Name = "dirCacheSectorsLabel";
            this.dirCacheSectorsLabel.Size = new System.Drawing.Size(56, 17);
            this.dirCacheSectorsLabel.TabIndex = 7;
            this.dirCacheSectorsLabel.Text = "Sectors";
            // 
            // dirCacheLabel
            // 
            this.dirCacheLabel.AutoSize = true;
            this.dirCacheLabel.Location = new System.Drawing.Point(47, 71);
            this.dirCacheLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.dirCacheLabel.Name = "dirCacheLabel";
            this.dirCacheLabel.Size = new System.Drawing.Size(70, 17);
            this.dirCacheLabel.TabIndex = 5;
            this.dirCacheLabel.Text = "Dir Cache";
            // 
            // fileLockNodesLabel
            // 
            this.fileLockNodesLabel.AutoSize = true;
            this.fileLockNodesLabel.Location = new System.Drawing.Point(264, 116);
            this.fileLockNodesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fileLockNodesLabel.Name = "fileLockNodesLabel";
            this.fileLockNodesLabel.Size = new System.Drawing.Size(49, 17);
            this.fileLockNodesLabel.TabIndex = 10;
            this.fileLockNodesLabel.Text = "Nodes";
            // 
            // fileLockLlabel
            // 
            this.fileLockLlabel.AutoSize = true;
            this.fileLockLlabel.Location = new System.Drawing.Point(52, 116);
            this.fileLockLlabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fileLockLlabel.Name = "fileLockLlabel";
            this.fileLockLlabel.Size = new System.Drawing.Size(64, 17);
            this.fileLockLlabel.TabIndex = 8;
            this.fileLockLlabel.Text = "File Lock";
            // 
            // fileHandleNodesLabel
            // 
            this.fileHandleNodesLabel.AutoSize = true;
            this.fileHandleNodesLabel.Location = new System.Drawing.Point(264, 162);
            this.fileHandleNodesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fileHandleNodesLabel.Name = "fileHandleNodesLabel";
            this.fileHandleNodesLabel.Size = new System.Drawing.Size(49, 17);
            this.fileHandleNodesLabel.TabIndex = 13;
            this.fileHandleNodesLabel.Text = "Nodes";
            // 
            // fileHandleLabel
            // 
            this.fileHandleLabel.AutoSize = true;
            this.fileHandleLabel.Location = new System.Drawing.Point(39, 162);
            this.fileHandleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fileHandleLabel.Name = "fileHandleLabel";
            this.fileHandleLabel.Size = new System.Drawing.Size(79, 17);
            this.fileHandleLabel.TabIndex = 11;
            this.fileHandleLabel.Text = "File Handle";
            // 
            // retriesLabel
            // 
            this.retriesLabel.AutoSize = true;
            this.retriesLabel.Location = new System.Drawing.Point(64, 207);
            this.retriesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.retriesLabel.Name = "retriesLabel";
            this.retriesLabel.Size = new System.Drawing.Size(53, 17);
            this.retriesLabel.TabIndex = 14;
            this.retriesLabel.Text = "Retries";
            // 
            // directReadCheckBox
            // 
            this.directReadCheckBox.AutoSize = true;
            this.directReadCheckBox.Location = new System.Drawing.Point(44, 252);
            this.directReadCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.directReadCheckBox.Name = "directReadCheckBox";
            this.directReadCheckBox.Size = new System.Drawing.Size(105, 21);
            this.directReadCheckBox.TabIndex = 16;
            this.directReadCheckBox.Tag = resources.GetString("directReadCheckBox.Tag");
            this.directReadCheckBox.Text = "Direct Read";
            this.directReadCheckBox.UseVisualStyleBackColor = true;
            this.directReadCheckBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // fastSearchCheckBox
            // 
            this.fastSearchCheckBox.AutoSize = true;
            this.fastSearchCheckBox.Location = new System.Drawing.Point(157, 252);
            this.fastSearchCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.fastSearchCheckBox.Name = "fastSearchCheckBox";
            this.fastSearchCheckBox.Size = new System.Drawing.Size(106, 21);
            this.fastSearchCheckBox.TabIndex = 17;
            this.fastSearchCheckBox.Tag = resources.GetString("fastSearchCheckBox.Tag");
            this.fastSearchCheckBox.Text = "Fast Search";
            this.fastSearchCheckBox.UseVisualStyleBackColor = true;
            this.fastSearchCheckBox.CheckedChanged += new System.EventHandler(this.fastSearchCheckBox_CheckedChanged);
            this.fastSearchCheckBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // speedIndCheckBox
            // 
            this.speedIndCheckBox.AutoSize = true;
            this.speedIndCheckBox.Location = new System.Drawing.Point(271, 252);
            this.speedIndCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.speedIndCheckBox.Name = "speedIndCheckBox";
            this.speedIndCheckBox.Size = new System.Drawing.Size(98, 21);
            this.speedIndCheckBox.TabIndex = 18;
            this.speedIndCheckBox.Tag = resources.GetString("speedIndCheckBox.Tag");
            this.speedIndCheckBox.Text = "Speed Ind.";
            this.speedIndCheckBox.UseVisualStyleBackColor = true;
            this.speedIndCheckBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // restoreDefaultsButton
            // 
            this.restoreDefaultsButton.Location = new System.Drawing.Point(16, 394);
            this.restoreDefaultsButton.Margin = new System.Windows.Forms.Padding(4);
            this.restoreDefaultsButton.Name = "restoreDefaultsButton";
            this.restoreDefaultsButton.Size = new System.Drawing.Size(141, 28);
            this.restoreDefaultsButton.TabIndex = 41;
            this.restoreDefaultsButton.Text = "Restore Defaults";
            this.restoreDefaultsButton.UseVisualStyleBackColor = true;
            this.restoreDefaultsButton.Click += new System.EventHandler(this.RestoreDefaultsButton_Click);
            // 
            // playSoundsCheckBox
            // 
            this.playSoundsCheckBox.AutoSize = true;
            this.playSoundsCheckBox.Location = new System.Drawing.Point(8, 72);
            this.playSoundsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.playSoundsCheckBox.Name = "playSoundsCheckBox";
            this.playSoundsCheckBox.Size = new System.Drawing.Size(109, 21);
            this.playSoundsCheckBox.TabIndex = 34;
            this.playSoundsCheckBox.Tag = "Play sounds after the event";
            this.playSoundsCheckBox.Text = "Play Sounds";
            this.playSoundsCheckBox.UseVisualStyleBackColor = true;
            this.playSoundsCheckBox.CheckedChanged += new System.EventHandler(this.playSoundsCheckBox_CheckedChanged);
            this.playSoundsCheckBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // volSetIdLabel
            // 
            this.volSetIdLabel.AutoSize = true;
            this.volSetIdLabel.Location = new System.Drawing.Point(31, 71);
            this.volSetIdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.volSetIdLabel.Name = "volSetIdLabel";
            this.volSetIdLabel.Size = new System.Drawing.Size(97, 17);
            this.volSetIdLabel.TabIndex = 22;
            this.volSetIdLabel.Text = "Volume Set ID";
            // 
            // volumeSetIdTextBox
            // 
            this.volumeSetIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.volumeSetIdTextBox.Location = new System.Drawing.Point(131, 67);
            this.volumeSetIdTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.volumeSetIdTextBox.MaxLength = 128;
            this.volumeSetIdTextBox.Name = "volumeSetIdTextBox";
            this.volumeSetIdTextBox.Size = new System.Drawing.Size(345, 22);
            this.volumeSetIdTextBox.TabIndex = 23;
            this.volumeSetIdTextBox.Tag = "This field shall specify an identification of the volume set of which the volume " +
    "is a member. Allowed 128 d-characters: A B C D E F G H I J K L M N O P Q R S T U" +
    " V W X Y Z 0 1 2 3 4 5 6 7 8 9 _";
            this.volumeSetIdTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            this.volumeSetIdTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.D_Characters_check);
            // 
            // publisherIdLabel
            // 
            this.publisherIdLabel.AutoSize = true;
            this.publisherIdLabel.Location = new System.Drawing.Point(45, 116);
            this.publisherIdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.publisherIdLabel.Name = "publisherIdLabel";
            this.publisherIdLabel.Size = new System.Drawing.Size(84, 17);
            this.publisherIdLabel.TabIndex = 24;
            this.publisherIdLabel.Text = "Publisher ID";
            // 
            // publisherIdTextBox
            // 
            this.publisherIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.publisherIdTextBox.Location = new System.Drawing.Point(131, 113);
            this.publisherIdTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.publisherIdTextBox.MaxLength = 128;
            this.publisherIdTextBox.Name = "publisherIdTextBox";
            this.publisherIdTextBox.Size = new System.Drawing.Size(345, 22);
            this.publisherIdTextBox.TabIndex = 25;
            this.publisherIdTextBox.Tag = resources.GetString("publisherIdTextBox.Tag");
            this.publisherIdTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            this.publisherIdTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.A_Characters_check);
            // 
            // dataPreparerIdLabel
            // 
            this.dataPreparerIdLabel.AutoSize = true;
            this.dataPreparerIdLabel.Location = new System.Drawing.Point(15, 162);
            this.dataPreparerIdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.dataPreparerIdLabel.Name = "dataPreparerIdLabel";
            this.dataPreparerIdLabel.Size = new System.Drawing.Size(115, 17);
            this.dataPreparerIdLabel.TabIndex = 26;
            this.dataPreparerIdLabel.Text = "Data Preparer ID";
            // 
            // dataPreparerIdTextBox
            // 
            this.dataPreparerIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.dataPreparerIdTextBox.Location = new System.Drawing.Point(131, 158);
            this.dataPreparerIdTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.dataPreparerIdTextBox.MaxLength = 128;
            this.dataPreparerIdTextBox.Name = "dataPreparerIdTextBox";
            this.dataPreparerIdTextBox.Size = new System.Drawing.Size(345, 22);
            this.dataPreparerIdTextBox.TabIndex = 27;
            this.dataPreparerIdTextBox.Tag = resources.GetString("dataPreparerIdTextBox.Tag");
            this.dataPreparerIdTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            this.dataPreparerIdTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.A_Characters_check);
            // 
            // applicationIdLabel
            // 
            this.applicationIdLabel.AutoSize = true;
            this.applicationIdLabel.Location = new System.Drawing.Point(33, 207);
            this.applicationIdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.applicationIdLabel.Name = "applicationIdLabel";
            this.applicationIdLabel.Size = new System.Drawing.Size(94, 17);
            this.applicationIdLabel.TabIndex = 28;
            this.applicationIdLabel.Text = "Application ID";
            // 
            // applicationIdTextBox
            // 
            this.applicationIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.applicationIdTextBox.Location = new System.Drawing.Point(131, 204);
            this.applicationIdTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.applicationIdTextBox.MaxLength = 128;
            this.applicationIdTextBox.Name = "applicationIdTextBox";
            this.applicationIdTextBox.Size = new System.Drawing.Size(345, 22);
            this.applicationIdTextBox.TabIndex = 29;
            this.applicationIdTextBox.Tag = resources.GetString("applicationIdTextBox.Tag");
            this.applicationIdTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            this.applicationIdTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.A_Characters_check);
            // 
            // imagePaddingComboBox
            // 
            this.imagePaddingComboBox.FormattingEnabled = true;
            this.imagePaddingComboBox.ItemHeight = 16;
            this.imagePaddingComboBox.Location = new System.Drawing.Point(131, 249);
            this.imagePaddingComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.imagePaddingComboBox.Name = "imagePaddingComboBox";
            this.imagePaddingComboBox.Size = new System.Drawing.Size(108, 24);
            this.imagePaddingComboBox.TabIndex = 31;
            this.imagePaddingComboBox.Tag = "Gap from the inner edge of the disk. Recommended One minute, Ten minutes or None." +
    "";
            this.imagePaddingComboBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            this.imagePaddingComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Any_Characters_check);
            // 
            // imagePaddingLabel
            // 
            this.imagePaddingLabel.AutoSize = true;
            this.imagePaddingLabel.Location = new System.Drawing.Point(27, 253);
            this.imagePaddingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.imagePaddingLabel.Name = "imagePaddingLabel";
            this.imagePaddingLabel.Size = new System.Drawing.Size(102, 17);
            this.imagePaddingLabel.TabIndex = 30;
            this.imagePaddingLabel.Text = "Image Padding";
            // 
            // WinUAETestCheckBox
            // 
            this.WinUAETestCheckBox.AutoSize = true;
            this.WinUAETestCheckBox.Location = new System.Drawing.Point(8, 100);
            this.WinUAETestCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.WinUAETestCheckBox.Name = "WinUAETestCheckBox";
            this.WinUAETestCheckBox.Size = new System.Drawing.Size(18, 17);
            this.WinUAETestCheckBox.TabIndex = 33;
            this.WinUAETestCheckBox.Tag = "Test ISO with WinUAE";
            this.WinUAETestCheckBox.UseVisualStyleBackColor = true;
            this.WinUAETestCheckBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // cdfsGroupBox
            // 
            this.cdfsGroupBox.Controls.Add(this.dirCacheTextBox);
            this.cdfsGroupBox.Controls.Add(this.dataCacheTextBox);
            this.cdfsGroupBox.Controls.Add(this.dataCacheLabel);
            this.cdfsGroupBox.Controls.Add(this.dataCacheSectorsLabel);
            this.cdfsGroupBox.Controls.Add(this.dirCacheLabel);
            this.cdfsGroupBox.Controls.Add(this.dirCacheSectorsLabel);
            this.cdfsGroupBox.Controls.Add(this.fileLockTextBox);
            this.cdfsGroupBox.Controls.Add(this.fileLockLlabel);
            this.cdfsGroupBox.Controls.Add(this.fileLockNodesLabel);
            this.cdfsGroupBox.Controls.Add(this.fileHandleTextBox);
            this.cdfsGroupBox.Controls.Add(this.fileHandleLabel);
            this.cdfsGroupBox.Controls.Add(this.fileHandleNodesLabel);
            this.cdfsGroupBox.Controls.Add(this.retriesTextBox);
            this.cdfsGroupBox.Controls.Add(this.retriesLabel);
            this.cdfsGroupBox.Controls.Add(this.speedIndCheckBox);
            this.cdfsGroupBox.Controls.Add(this.directReadCheckBox);
            this.cdfsGroupBox.Controls.Add(this.fastSearchCheckBox);
            this.cdfsGroupBox.Location = new System.Drawing.Point(16, 4);
            this.cdfsGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.cdfsGroupBox.Name = "cdfsGroupBox";
            this.cdfsGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.cdfsGroupBox.Size = new System.Drawing.Size(419, 287);
            this.cdfsGroupBox.TabIndex = 1;
            this.cdfsGroupBox.TabStop = false;
            this.cdfsGroupBox.Text = "CDFS";
            // 
            // isoGroupBox
            // 
            this.isoGroupBox.Controls.Add(this.volumeSetIdTextBox);
            this.isoGroupBox.Controls.Add(this.volumeIdTextBox);
            this.isoGroupBox.Controls.Add(this.volumeIdLabel);
            this.isoGroupBox.Controls.Add(this.imagePaddingLabel);
            this.isoGroupBox.Controls.Add(this.imagePaddingComboBox);
            this.isoGroupBox.Controls.Add(this.volSetIdLabel);
            this.isoGroupBox.Controls.Add(this.publisherIdTextBox);
            this.isoGroupBox.Controls.Add(this.applicationIdLabel);
            this.isoGroupBox.Controls.Add(this.publisherIdLabel);
            this.isoGroupBox.Controls.Add(this.applicationIdTextBox);
            this.isoGroupBox.Controls.Add(this.dataPreparerIdTextBox);
            this.isoGroupBox.Controls.Add(this.dataPreparerIdLabel);
            this.isoGroupBox.Location = new System.Drawing.Point(443, 4);
            this.isoGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.isoGroupBox.Name = "isoGroupBox";
            this.isoGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.isoGroupBox.Size = new System.Drawing.Size(497, 287);
            this.isoGroupBox.TabIndex = 19;
            this.isoGroupBox.TabStop = false;
            this.isoGroupBox.Text = "ISO";
            // 
            // generalGroupBox
            // 
            this.generalGroupBox.Controls.Add(this.useOrderFileCheckBox);
            this.generalGroupBox.Controls.Add(this.winUAEBrowseButton);
            this.generalGroupBox.Controls.Add(this.winUAEPathLabel);
            this.generalGroupBox.Controls.Add(this.winUAEPathTextBox);
            this.generalGroupBox.Controls.Add(this.WinUAETestCheckBox);
            this.generalGroupBox.Controls.Add(this.playSoundsCheckBox);
            this.generalGroupBox.Location = new System.Drawing.Point(443, 294);
            this.generalGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.generalGroupBox.Name = "generalGroupBox";
            this.generalGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.generalGroupBox.Size = new System.Drawing.Size(497, 128);
            this.generalGroupBox.TabIndex = 32;
            this.generalGroupBox.TabStop = false;
            this.generalGroupBox.Text = "General";
            // 
            // useOrderFileCheckBox
            // 
            this.useOrderFileCheckBox.AutoSize = true;
            this.useOrderFileCheckBox.Location = new System.Drawing.Point(8, 47);
            this.useOrderFileCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.useOrderFileCheckBox.Name = "useOrderFileCheckBox";
            this.useOrderFileCheckBox.Size = new System.Drawing.Size(115, 21);
            this.useOrderFileCheckBox.TabIndex = 33;
            this.useOrderFileCheckBox.Tag = resources.GetString("useOrderFileCheckBox.Tag");
            this.useOrderFileCheckBox.Text = "Use order file";
            this.useOrderFileCheckBox.UseVisualStyleBackColor = true;
            this.useOrderFileCheckBox.CheckedChanged += new System.EventHandler(this.useOrderFileCheckBox_CheckedChanged);
            this.useOrderFileCheckBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // winUAEBrowseButton
            // 
            this.winUAEBrowseButton.Location = new System.Drawing.Point(389, 95);
            this.winUAEBrowseButton.Margin = new System.Windows.Forms.Padding(4);
            this.winUAEBrowseButton.Name = "winUAEBrowseButton";
            this.winUAEBrowseButton.Size = new System.Drawing.Size(100, 28);
            this.winUAEBrowseButton.TabIndex = 37;
            this.winUAEBrowseButton.Tag = "Browse for WinUAE executable";
            this.winUAEBrowseButton.Text = "Browse";
            this.winUAEBrowseButton.UseVisualStyleBackColor = true;
            this.winUAEBrowseButton.Click += new System.EventHandler(this.WinUAEBrowseButton_Click);
            this.winUAEBrowseButton.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // winUAEPathLabel
            // 
            this.winUAEPathLabel.AutoSize = true;
            this.winUAEPathLabel.Location = new System.Drawing.Point(29, 99);
            this.winUAEPathLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.winUAEPathLabel.Name = "winUAEPathLabel";
            this.winUAEPathLabel.Size = new System.Drawing.Size(93, 17);
            this.winUAEPathLabel.TabIndex = 35;
            this.winUAEPathLabel.Text = "WinUAE Path";
            // 
            // winUAEPathTextBox
            // 
            this.winUAEPathTextBox.Location = new System.Drawing.Point(130, 98);
            this.winUAEPathTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.winUAEPathTextBox.Name = "winUAEPathTextBox";
            this.winUAEPathTextBox.Size = new System.Drawing.Size(254, 22);
            this.winUAEPathTextBox.TabIndex = 36;
            this.winUAEPathTextBox.Tag = "WinUAE executable path";
            this.winUAEPathTextBox.TextChanged += new System.EventHandler(this.winUAEPathTextBox_TextChanged);
            this.winUAEPathTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // HelpTextBox
            // 
            this.HelpTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.HelpTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.HelpTextBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.HelpTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.HelpTextBox.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.HelpTextBox.Location = new System.Drawing.Point(16, 294);
            this.HelpTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.HelpTextBox.Multiline = true;
            this.HelpTextBox.Name = "HelpTextBox";
            this.HelpTextBox.ReadOnly = true;
            this.HelpTextBox.Size = new System.Drawing.Size(419, 99);
            this.HelpTextBox.TabIndex = 41;
            this.HelpTextBox.TabStop = false;
            // 
            // dirCacheTextBox
            // 
            this.dirCacheTextBox.Location = new System.Drawing.Point(120, 67);
            this.dirCacheTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.dirCacheTextBox.MaxLength = 32;
            this.dirCacheTextBox.MaxValue = 127;
            this.dirCacheTextBox.MinValue = 2;
            this.dirCacheTextBox.Name = "dirCacheTextBox";
            this.dirCacheTextBox.Size = new System.Drawing.Size(140, 22);
            this.dirCacheTextBox.TabIndex = 6;
            this.dirCacheTextBox.Tag = resources.GetString("dirCacheTextBox.Tag");
            this.dirCacheTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // dataCacheTextBox
            // 
            this.dataCacheTextBox.Location = new System.Drawing.Point(120, 22);
            this.dataCacheTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.dataCacheTextBox.MaxLength = 32;
            this.dataCacheTextBox.MaxValue = 127;
            this.dataCacheTextBox.MinValue = 1;
            this.dataCacheTextBox.Name = "dataCacheTextBox";
            this.dataCacheTextBox.Size = new System.Drawing.Size(140, 22);
            this.dataCacheTextBox.TabIndex = 3;
            this.dataCacheTextBox.Tag = "CDFS allocates a read-ahead cache to improve performance. The size of this cache " +
    "is specified in CD sectors (2K bytes each). The default size is 8 blocks, min.1 " +
    "- max.127.";
            this.dataCacheTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // fileLockTextBox
            // 
            this.fileLockTextBox.Location = new System.Drawing.Point(120, 113);
            this.fileLockTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.fileLockTextBox.MaxLength = 32;
            this.fileLockTextBox.MaxValue = 9999;
            this.fileLockTextBox.MinValue = 1;
            this.fileLockTextBox.Name = "fileLockTextBox";
            this.fileLockTextBox.Size = new System.Drawing.Size(140, 22);
            this.fileLockTextBox.TabIndex = 9;
            this.fileLockTextBox.Tag = resources.GetString("fileLockTextBox.Tag");
            this.fileLockTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // fileHandleTextBox
            // 
            this.fileHandleTextBox.Location = new System.Drawing.Point(120, 158);
            this.fileHandleTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.fileHandleTextBox.MaxLength = 32;
            this.fileHandleTextBox.MaxValue = 9999;
            this.fileHandleTextBox.MinValue = 1;
            this.fileHandleTextBox.Name = "fileHandleTextBox";
            this.fileHandleTextBox.Size = new System.Drawing.Size(140, 22);
            this.fileHandleTextBox.TabIndex = 12;
            this.fileHandleTextBox.Tag = "As with Filelocks, CDFS pre-allocates a pool of Filehandle structures. The size o" +
    "f the pool is specified as the number of Filehandles to pre-allocate. The defaul" +
    "t value is 16, min.1 - max.9999.";
            this.fileHandleTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // retriesTextBox
            // 
            this.retriesTextBox.Location = new System.Drawing.Point(120, 204);
            this.retriesTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.retriesTextBox.MaxLength = 32;
            this.retriesTextBox.MaxValue = 9999;
            this.retriesTextBox.MinValue = 0;
            this.retriesTextBox.Name = "retriesTextBox";
            this.retriesTextBox.Size = new System.Drawing.Size(140, 22);
            this.retriesTextBox.TabIndex = 15;
            this.retriesTextBox.Tag = "You can specify from 0 to 9999 retry attempts on read failures. The default value" +
    " is 32.";
            this.retriesTextBox.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.textBox_HelpRequested);
            // 
            // optionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(955, 427);
            this.Controls.Add(this.HelpTextBox);
            this.Controls.Add(this.generalGroupBox);
            this.Controls.Add(this.isoGroupBox);
            this.Controls.Add(this.cdfsGroupBox);
            this.Controls.Add(this.restoreDefaultsButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "optionsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Options";
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            this.cdfsGroupBox.ResumeLayout(false);
            this.cdfsGroupBox.PerformLayout();
            this.isoGroupBox.ResumeLayout(false);
            this.isoGroupBox.PerformLayout();
            this.generalGroupBox.ResumeLayout(false);
            this.generalGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        System.Windows.Forms.Button okButton;
        System.Windows.Forms.Button cancelButton;
        System.Windows.Forms.Label volumeIdLabel;
        System.Windows.Forms.TextBox volumeIdTextBox;
        System.Windows.Forms.Label dataCacheLabel;
        System.Windows.Forms.Label dataCacheSectorsLabel;
        System.Windows.Forms.Label dirCacheSectorsLabel;
        System.Windows.Forms.Label dirCacheLabel;
        System.Windows.Forms.Label fileLockNodesLabel;
        System.Windows.Forms.Label fileLockLlabel;
        System.Windows.Forms.Label fileHandleNodesLabel;
        System.Windows.Forms.Label fileHandleLabel;
        System.Windows.Forms.Label retriesLabel;
        System.Windows.Forms.CheckBox directReadCheckBox;
        System.Windows.Forms.CheckBox fastSearchCheckBox;
        System.Windows.Forms.CheckBox speedIndCheckBox;
        System.Windows.Forms.Button restoreDefaultsButton;
        ValueBox retriesTextBox;
        ValueBox fileHandleTextBox;
        ValueBox fileLockTextBox;
        ValueBox dirCacheTextBox;
        ValueBox dataCacheTextBox;
        System.Windows.Forms.CheckBox playSoundsCheckBox;
        System.Windows.Forms.Label volSetIdLabel;
        System.Windows.Forms.TextBox volumeSetIdTextBox;
        System.Windows.Forms.Label publisherIdLabel;
        System.Windows.Forms.TextBox publisherIdTextBox;
        System.Windows.Forms.Label dataPreparerIdLabel;
        System.Windows.Forms.TextBox dataPreparerIdTextBox;
        System.Windows.Forms.Label applicationIdLabel;
        System.Windows.Forms.TextBox applicationIdTextBox;
        System.Windows.Forms.ComboBox imagePaddingComboBox;
        System.Windows.Forms.Label imagePaddingLabel;
        System.Windows.Forms.CheckBox WinUAETestCheckBox;
        System.Windows.Forms.GroupBox cdfsGroupBox;
        System.Windows.Forms.GroupBox isoGroupBox;
        System.Windows.Forms.GroupBox generalGroupBox;
        System.Windows.Forms.Label winUAEPathLabel;
        System.Windows.Forms.TextBox winUAEPathTextBox;
        System.Windows.Forms.Button winUAEBrowseButton;
        private System.Windows.Forms.TextBox HelpTextBox;
        private System.Windows.Forms.CheckBox useOrderFileCheckBox;
    }
}