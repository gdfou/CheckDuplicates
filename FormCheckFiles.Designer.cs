namespace CheckDuplicates
{
	partial class FormCheckFiles
	{
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCheckFiles));
            this.panelTools = new System.Windows.Forms.Panel();
            this.buttonStop = new System.Windows.Forms.Button();
            this.imageListButtons = new System.Windows.Forms.ImageList(this.components);
            this.buttonGo = new System.Windows.Forms.Button();
            this.buttonScan = new System.Windows.Forms.Button();
            this.buttonAddFolder = new System.Windows.Forms.Button();
            this.contextMenuStripFolder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuImporterFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExporterFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuClearFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuOuvrirDossier = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOuvrirFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDeleteFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuNoTestFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTestFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuExporterFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.menuImporterFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.purgerTOUTLesDoublonsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageListView = new System.Windows.Forms.ImageList(this.components);
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusInfo1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusInfo2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusInfo3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.listViewFiles = new CheckDuplicates.MyListView();
            this.columnHeaderFileFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileMD5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewFolders = new CheckDuplicates.MyListView();
            this.columnHeaderFolderFolder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFolderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelTools.SuspendLayout();
            this.contextMenuStripFolder.SuspendLayout();
            this.contextMenuStripFiles.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTools
            // 
            this.panelTools.Controls.Add(this.buttonStop);
            this.panelTools.Controls.Add(this.buttonGo);
            this.panelTools.Controls.Add(this.buttonScan);
            this.panelTools.Controls.Add(this.buttonAddFolder);
            this.panelTools.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTools.Location = new System.Drawing.Point(0, 0);
            this.panelTools.Name = "panelTools";
            this.panelTools.Size = new System.Drawing.Size(1140, 62);
            this.panelTools.TabIndex = 0;
            // 
            // buttonStop
            // 
            this.buttonStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonStop.Enabled = false;
            this.buttonStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStop.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonStop.ImageIndex = 3;
            this.buttonStop.ImageList = this.imageListButtons;
            this.buttonStop.Location = new System.Drawing.Point(537, 12);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.buttonStop.Size = new System.Drawing.Size(121, 41);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.Text = "STOP";
            this.buttonStop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // imageListButtons
            // 
            this.imageListButtons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListButtons.ImageStream")));
            this.imageListButtons.TransparentColor = System.Drawing.Color.Black;
            this.imageListButtons.Images.SetKeyName(0, "Folder.png");
            this.imageListButtons.Images.SetKeyName(1, "Plus.png");
            this.imageListButtons.Images.SetKeyName(2, "Process3.png");
            this.imageListButtons.Images.SetKeyName(3, "Stop.png");
            // 
            // buttonGo
            // 
            this.buttonGo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonGo.Enabled = false;
            this.buttonGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonGo.ImageIndex = 2;
            this.buttonGo.ImageList = this.imageListButtons;
            this.buttonGo.Location = new System.Drawing.Point(362, 11);
            this.buttonGo.Name = "buttonGo";
            this.buttonGo.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.buttonGo.Size = new System.Drawing.Size(169, 41);
            this.buttonGo.TabIndex = 2;
            this.buttonGo.Text = "Démarrer le test";
            this.buttonGo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonGo.UseVisualStyleBackColor = true;
            this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
            // 
            // buttonScan
            // 
            this.buttonScan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonScan.Enabled = false;
            this.buttonScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonScan.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonScan.ImageIndex = 0;
            this.buttonScan.ImageList = this.imageListButtons;
            this.buttonScan.Location = new System.Drawing.Point(187, 11);
            this.buttonScan.Name = "buttonScan";
            this.buttonScan.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.buttonScan.Size = new System.Drawing.Size(169, 41);
            this.buttonScan.TabIndex = 1;
            this.buttonScan.Text = "Lister les fichiers";
            this.buttonScan.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonScan.UseVisualStyleBackColor = true;
            this.buttonScan.Click += new System.EventHandler(this.buttonScan_Click);
            // 
            // buttonAddFolder
            // 
            this.buttonAddFolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonAddFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddFolder.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonAddFolder.ImageIndex = 1;
            this.buttonAddFolder.ImageList = this.imageListButtons;
            this.buttonAddFolder.Location = new System.Drawing.Point(12, 11);
            this.buttonAddFolder.Name = "buttonAddFolder";
            this.buttonAddFolder.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.buttonAddFolder.Size = new System.Drawing.Size(169, 41);
            this.buttonAddFolder.TabIndex = 0;
            this.buttonAddFolder.Text = "Ajouter un dossier";
            this.buttonAddFolder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonAddFolder.UseVisualStyleBackColor = true;
            this.buttonAddFolder.Click += new System.EventHandler(this.buttonAddFolder_Click);
            // 
            // contextMenuStripFolder
            // 
            this.contextMenuStripFolder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuImporterFolder,
            this.menuExporterFolder,
            this.toolStripMenuItem1,
            this.menuClearFolder});
            this.contextMenuStripFolder.Name = "contextMenuStripFolder";
            this.contextMenuStripFolder.Size = new System.Drawing.Size(229, 76);
            this.contextMenuStripFolder.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripFolder_Opening);
            // 
            // menuImporterFolder
            // 
            this.menuImporterFolder.Name = "menuImporterFolder";
            this.menuImporterFolder.Size = new System.Drawing.Size(228, 22);
            this.menuImporterFolder.Text = "&Importer une liste de dossiers";
            this.menuImporterFolder.Click += new System.EventHandler(this.menuImporterFolder_Click);
            // 
            // menuExporterFolder
            // 
            this.menuExporterFolder.Name = "menuExporterFolder";
            this.menuExporterFolder.Size = new System.Drawing.Size(228, 22);
            this.menuExporterFolder.Text = "&Exporter une liste de dossiers";
            this.menuExporterFolder.Click += new System.EventHandler(this.menuExporterFolder_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(225, 6);
            // 
            // menuClearFolder
            // 
            this.menuClearFolder.Name = "menuClearFolder";
            this.menuClearFolder.Size = new System.Drawing.Size(228, 22);
            this.menuClearFolder.Text = "Effacer la &Liste";
            this.menuClearFolder.Click += new System.EventHandler(this.menuClearFolder_Click);
            // 
            // contextMenuStripFiles
            // 
            this.contextMenuStripFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuOuvrirDossier,
            this.menuOuvrirFile,
            this.menuDeleteFile,
            this.toolStripMenuItem2,
            this.menuNoTestFile,
            this.menuTestFile,
            this.toolStripMenuItem3,
            this.menuExporterFiles,
            this.menuImporterFiles,
            this.purgerTOUTLesDoublonsToolStripMenuItem});
            this.contextMenuStripFiles.Name = "contextMenuStripFiles";
            this.contextMenuStripFiles.Size = new System.Drawing.Size(265, 192);
            this.contextMenuStripFiles.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripFiles_Opening);
            // 
            // menuOuvrirDossier
            // 
            this.menuOuvrirDossier.Name = "menuOuvrirDossier";
            this.menuOuvrirDossier.Size = new System.Drawing.Size(264, 22);
            this.menuOuvrirDossier.Text = "Ouvrir le dossier &contenant le fichier";
            this.menuOuvrirDossier.Click += new System.EventHandler(this.menuOuvrirDossier_Click);
            // 
            // menuOuvrirFile
            // 
            this.menuOuvrirFile.Name = "menuOuvrirFile";
            this.menuOuvrirFile.Size = new System.Drawing.Size(264, 22);
            this.menuOuvrirFile.Text = "&Ouvrir le fichier";
            this.menuOuvrirFile.Click += new System.EventHandler(this.menuOuvrirFile_Click);
            // 
            // menuDeleteFile
            // 
            this.menuDeleteFile.Name = "menuDeleteFile";
            this.menuDeleteFile.Size = new System.Drawing.Size(264, 22);
            this.menuDeleteFile.Text = "&Supprimer le(s) fichier(s)";
            this.menuDeleteFile.Click += new System.EventHandler(this.menuDeleteFile_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(261, 6);
            // 
            // menuNoTestFile
            // 
            this.menuNoTestFile.Name = "menuNoTestFile";
            this.menuNoTestFile.Size = new System.Drawing.Size(264, 22);
            this.menuNoTestFile.Text = "Ne pas &tester le fichier";
            this.menuNoTestFile.Click += new System.EventHandler(this.menuNoTestFile_Click);
            // 
            // menuTestFile
            // 
            this.menuTestFile.Name = "menuTestFile";
            this.menuTestFile.Size = new System.Drawing.Size(264, 22);
            this.menuTestFile.Text = "&Tester le fichier";
            this.menuTestFile.Click += new System.EventHandler(this.menuTestFile_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(261, 6);
            // 
            // menuExporterFiles
            // 
            this.menuExporterFiles.Name = "menuExporterFiles";
            this.menuExporterFiles.Size = new System.Drawing.Size(264, 22);
            this.menuExporterFiles.Text = "&Exporter le résultat";
            this.menuExporterFiles.Click += new System.EventHandler(this.menuExporterFiles_Click);
            // 
            // menuImporterFiles
            // 
            this.menuImporterFiles.Name = "menuImporterFiles";
            this.menuImporterFiles.Size = new System.Drawing.Size(264, 22);
            this.menuImporterFiles.Text = "&Importer un fichier résultat";
            this.menuImporterFiles.Click += new System.EventHandler(this.menuImporterFiles_Click);
            // 
            // purgerTOUTLesDoublonsToolStripMenuItem
            // 
            this.purgerTOUTLesDoublonsToolStripMenuItem.Name = "purgerTOUTLesDoublonsToolStripMenuItem";
            this.purgerTOUTLesDoublonsToolStripMenuItem.Size = new System.Drawing.Size(264, 22);
            this.purgerTOUTLesDoublonsToolStripMenuItem.Text = "Purger TOUT les doublons";
            this.purgerTOUTLesDoublonsToolStripMenuItem.Click += new System.EventHandler(this.purgerTOUTLesDoublonsToolStripMenuItem_Click);
            // 
            // imageListView
            // 
            this.imageListView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListView.ImageStream")));
            this.imageListView.TransparentColor = System.Drawing.Color.Olive;
            this.imageListView.Images.SetKeyName(0, "empty.bmp");
            this.imageListView.Images.SetKeyName(1, "led1on.bmp");
            this.imageListView.Images.SetKeyName(2, "led2on.bmp");
            this.imageListView.Images.SetKeyName(3, "led3on.bmp");
            this.imageListView.Images.SetKeyName(4, "led4on.bmp");
            this.imageListView.Images.SetKeyName(5, "PetiteFlecheBas.bmp");
            this.imageListView.Images.SetKeyName(6, "PetiteFlecheHaut.bmp");
            this.imageListView.Images.SetKeyName(7, "PetiteFleche.bmp");
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 257);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1140, 3);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusInfo1,
            this.toolStripStatusInfo2,
            this.toolStripStatusInfo3});
            this.statusStrip1.Location = new System.Drawing.Point(0, 557);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1140, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusInfo1
            // 
            this.toolStripStatusInfo1.AutoSize = false;
            this.toolStripStatusInfo1.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusInfo1.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.toolStripStatusInfo1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusInfo1.Name = "toolStripStatusInfo1";
            this.toolStripStatusInfo1.Size = new System.Drawing.Size(150, 17);
            this.toolStripStatusInfo1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusInfo2
            // 
            this.toolStripStatusInfo2.AutoSize = false;
            this.toolStripStatusInfo2.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusInfo2.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.toolStripStatusInfo2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusInfo2.Name = "toolStripStatusInfo2";
            this.toolStripStatusInfo2.Size = new System.Drawing.Size(150, 17);
            this.toolStripStatusInfo2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusInfo3
            // 
            this.toolStripStatusInfo3.AutoSize = false;
            this.toolStripStatusInfo3.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusInfo3.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.toolStripStatusInfo3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusInfo3.Name = "toolStripStatusInfo3";
            this.toolStripStatusInfo3.Size = new System.Drawing.Size(825, 17);
            this.toolStripStatusInfo3.Spring = true;
            this.toolStripStatusInfo3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
            // 
            // listViewFiles
            // 
            this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFileFile,
            this.columnHeaderFileSize,
            this.columnHeaderFileType,
            this.columnHeaderFileDate,
            this.columnHeaderFileMD5,
            this.columnHeaderFileStatus});
            this.listViewFiles.ContextMenuStrip = this.contextMenuStripFiles;
            this.listViewFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewFiles.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewFiles.FullRowSelect = true;
            this.listViewFiles.GridLines = true;
            this.listViewFiles.HideSelection = false;
            this.listViewFiles.Location = new System.Drawing.Point(0, 260);
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.Size = new System.Drawing.Size(1140, 297);
            this.listViewFiles.SmallImageList = this.imageListView;
            this.listViewFiles.TabIndex = 2;
            this.listViewFiles.UseCompatibleStateImageBehavior = false;
            this.listViewFiles.View = System.Windows.Forms.View.Details;
            this.listViewFiles.VirtualMode = true;
            this.listViewFiles.CacheVirtualItems += new System.Windows.Forms.CacheVirtualItemsEventHandler(this.listViewFiles_CacheVirtualItems);
            this.listViewFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewFiles_ColumnClick);
            this.listViewFiles.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listViewFiles_RetrieveVirtualItem);
            this.listViewFiles.SearchForVirtualItem += new System.Windows.Forms.SearchForVirtualItemEventHandler(this.listViewFiles_SearchForVirtualItem);
            this.listViewFiles.DoubleClick += new System.EventHandler(this.menuOuvrirDossier_Click);
            // 
            // columnHeaderFileFile
            // 
            this.columnHeaderFileFile.Text = "Fichier";
            this.columnHeaderFileFile.Width = 400;
            // 
            // columnHeaderFileSize
            // 
            this.columnHeaderFileSize.Text = "Taille";
            this.columnHeaderFileSize.Width = 95;
            // 
            // columnHeaderFileType
            // 
            this.columnHeaderFileType.Text = "Type";
            this.columnHeaderFileType.Width = 85;
            // 
            // columnHeaderFileDate
            // 
            this.columnHeaderFileDate.Text = "Date/Time";
            this.columnHeaderFileDate.Width = 170;
            // 
            // columnHeaderFileMD5
            // 
            this.columnHeaderFileMD5.Text = "MD5";
            this.columnHeaderFileMD5.Width = 240;
            // 
            // columnHeaderFileStatus
            // 
            this.columnHeaderFileStatus.Text = "Status";
            this.columnHeaderFileStatus.Width = 120;
            // 
            // listViewFolders
            // 
            this.listViewFolders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFolderFolder,
            this.columnHeaderFolderStatus});
            this.listViewFolders.ContextMenuStrip = this.contextMenuStripFolder;
            this.listViewFolders.Dock = System.Windows.Forms.DockStyle.Top;
            this.listViewFolders.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewFolders.FullRowSelect = true;
            this.listViewFolders.GridLines = true;
            this.listViewFolders.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewFolders.HideSelection = false;
            this.listViewFolders.Location = new System.Drawing.Point(0, 62);
            this.listViewFolders.Name = "listViewFolders";
            this.listViewFolders.Size = new System.Drawing.Size(1140, 195);
            this.listViewFolders.TabIndex = 1;
            this.listViewFolders.UseCompatibleStateImageBehavior = false;
            this.listViewFolders.View = System.Windows.Forms.View.Details;
            this.listViewFolders.VirtualMode = true;
            this.listViewFolders.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listViewFolders_RetrieveVirtualItem);
            this.listViewFolders.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewFolders_KeyDown);
            // 
            // columnHeaderFolderFolder
            // 
            this.columnHeaderFolderFolder.Text = "Dossier";
            this.columnHeaderFolderFolder.Width = 834;
            // 
            // columnHeaderFolderStatus
            // 
            this.columnHeaderFolderStatus.Text = "Status";
            this.columnHeaderFolderStatus.Width = 121;
            // 
            // FormCheckFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 579);
            this.Controls.Add(this.listViewFiles);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.listViewFolders);
            this.Controls.Add(this.panelTools);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormCheckFiles";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Check Files";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCheckFiles_FormClosing);
            this.Load += new System.EventHandler(this.FormCheckFiles_Load);
            this.panelTools.ResumeLayout(false);
            this.contextMenuStripFolder.ResumeLayout(false);
            this.contextMenuStripFiles.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panelTools;
		private MyListView listViewFolders;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Button buttonAddFolder;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusInfo1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusInfo2;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusInfo3;
		private System.Windows.Forms.ImageList imageListButtons;
		private System.Windows.Forms.Button buttonStop;
		private System.Windows.Forms.Button buttonGo;
		private System.Windows.Forms.Button buttonScan;
		private System.Windows.Forms.ColumnHeader columnHeaderFolderFolder;
		private System.Windows.Forms.ColumnHeader columnHeaderFolderStatus;
		private System.Windows.Forms.ColumnHeader columnHeaderFileFile;
		private System.Windows.Forms.ColumnHeader columnHeaderFileSize;
		private System.Windows.Forms.ColumnHeader columnHeaderFileMD5;
		private System.Windows.Forms.ColumnHeader columnHeaderFileType;
		private System.Windows.Forms.ColumnHeader columnHeaderFileStatus;
		private System.Windows.Forms.ImageList imageListView;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripFolder;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripFiles;
		private System.Windows.Forms.ColumnHeader columnHeaderFileDate;
		private System.Windows.Forms.ToolStripMenuItem menuImporterFolder;
		private System.Windows.Forms.ToolStripMenuItem menuExporterFolder;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem menuClearFolder;
		private System.Windows.Forms.ToolStripMenuItem menuOuvrirDossier;
		private System.Windows.Forms.ToolStripMenuItem menuOuvrirFile;
		private System.Windows.Forms.ToolStripMenuItem menuDeleteFile;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem menuNoTestFile;
		private System.Windows.Forms.ToolStripMenuItem menuTestFile;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem menuExporterFiles;
		private System.Windows.Forms.ToolStripMenuItem menuImporterFiles;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private MyListView listViewFiles;
		private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.Windows.Forms.ToolStripMenuItem purgerTOUTLesDoublonsToolStripMenuItem;
    }
}

