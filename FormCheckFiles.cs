using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Collections;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using Cryptography;
using System.Numerics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace CheckDuplicates
{
	public partial class FormCheckFiles : Form
	{
		// TODO: ajouter drag & drop depuis l'explorateur de fichier
		// TODO: ajouter la possibilité de mettre des fichiers dans listviewFolders
		//---------------------------------------------------------------------------
		static readonly Color CUSTOM_JAUNE = Color.FromArgb(255, 255, 240);
		static readonly Color CUSTOM_VERT = Color.FromArgb(224, 255, 224);
		static readonly Color COLOR_SAUMON_N = Color.FromArgb(238, 132, 132);    // Saumon Normal
		static readonly Color COLOR_SAUMON_C = Color.FromArgb(245, 184, 184);    // Saumon Clair

		//---------------------------------------------------------------------------
		const int BLOC_SIZE = 1 * 1024 * 1024;  // 1Mo

		//---------------------------------------------------------------------------
		int						m_Read_Error;
		eBW_CMD					m_BackgroundWorkerCmd;
		ListViewItemComparer	m_FilesListComparer;
		List<TFolderInfo>		m_FolderList;
		List<TFileInfo>			m_FilesList;
        string                  m_ImporterFilesPath;
        string                  m_ImporterFolderPath;
        BackgroundWorker        m_CheckDuplicatesBW;

        /// <summary>
        /// Status d'un fichier
        /// </summary>
        protected enum eFILE_STATUS
		{
			fsNONE,             // Ne rien faire
			fsCHECKED,          // Check OK
			fsDOUBLON,          // Check OK -> Doublon
			fsERROR,            // Erreur de lecture du fichier
			fsDELETED,          // Fichier supprimer (n'éxiste plus)
			fsNOTEST,           // Ne pas tester le fichier
			fsTOPROCESS,		// Fichier à traiter
			fsTO_DELETE,        // SPECIAL -> état intermédiaire pour la suppression en groupe
		}
		/// <summary>
		/// Liste correspondant aux images de imageListView
		/// </summary>
		protected enum eIMGLIST
		{
			EMPTY,
			LED_ROUGE,
			LED_VERTE,
			LED_ORANGE,
			LED_BLEU,
			FLECHE_HAUT,
			FLECHE_BAS,
			PETITE_FLECHE,
		}
		/// <summary>
		/// Code de colonne: doit être dans l'ordre de déclaration des colonnes
		/// </summary>
		protected enum eCOLUMNS
		{
			ecFICHIER,
			ecTAILLE,
			ecTYPE,
			ecDATETIME,
			ecMD5,
			ecSTATUS
		}
		/// <summary>
		/// Commande pour le background worker
		/// </summary>
		protected enum eBW_CMD
		{
			NO_CMD,
			BUILD_FILES_LIST,
			CHECK_DUPLICATES,
			IMPORT_FILES_LIST,
		}
		class TBackgroundWorkerCmd
		{
			public eBW_CMD			cmd;
			public string			filename;
			public TBackgroundWorkerCmd(eBW_CMD cmdarg)
			{
				cmd = cmdarg;
			}
			public TBackgroundWorkerCmd(eBW_CMD cmdarg, string filenamearg)
			{
				cmd = cmdarg;
				filename = filenamearg;
			}
		}

		//---------------------------------------------------------------------------
		protected class TFolderInfo
		{
			public string			fullFolderName; // Nom complet du dossier
			public string			statusText;
			public int				index;
			public int				status;

			public TFolderInfo(string folder, int idx)
			{
				fullFolderName = folder;
				status = 0;
				statusText = "";
				index = idx;
			}
		}

		//---------------------------------------------------------------------------
		protected class TFileInfo
		{
			public string			fullFileName;   // Nom complet du fichier
			public string			folder;			// Dossier de base (si import syno)
			public string			type;			// Type / extension

			public Int64			fileSize;		// Taille du fichier
			public DateTime			dateTime;		// Last Write Date / Time

			public eFILE_STATUS		eStatus;		// 0 = non-scanner, 1 = ok, 2 = erreur

			public byte[]			md5;			// MD5 du fichier

			public int				ctrDbl;			// Compteur de doublons

			public bool				fInProgress;	// en cours de traitement ...
//			public bool				process;		// pour l'algo duplicate

			public TFileInfo()
			{
				fileSize = 0;
				ctrDbl = 0;
				eStatus = eFILE_STATUS.fsNONE;
				md5 = null;
				fInProgress = false;
			}

			public void Reset()
			{
				eStatus = eFILE_STATUS.fsNONE;
				md5 = null;
			}
		};

		/// <summary>
		/// Convert from "AABBCCDDEEFF" to 0xAA,0xBB,0xCC,0xDD,0xEE,0xFF  
		/// </summary>
		/// <param name="str">texte d'entrée</param>
		/// <returns>tableau d'octets</returns>
		byte[] ConvertFromString(string str)
		{
			int length = (str.Length + 1) / 2;
			byte[] tab = new byte[length];
			for (int i = 0; i < length; i++) tab[i] = Convert.ToByte(str.Substring(2 * i, 2), 16);
			return tab;
		}

		/// <summary>
		/// Convertie la taille d'un fichier en texte
		/// </summary>
		/// <param name="size"></param>
		/// <returns>texte</returns>
		string FileSizeToString(Int64 size)
		{
			const double iBase = 1024.0;    // 1000 if decimal size, 1024 if hexa size
			const double iKo = iBase;
			const double iMo = iBase * iKo;
			const double iGo = iBase * iMo;

			if (size > iGo)
			{
				return string.Format("{0:0.00} Go", size / iGo);
			}
			else if (size > iMo)
			{
				return string.Format("{0:0.00} Mo", size / iMo);
			}
			else if (size > (10 * iKo))
			{
				return string.Format("{0:0.} Ko", size / iKo);
			}
			else
			{
				return string.Format("{0:0.} o", size);
			}
		}

		int Pourcentage(int _value, int _max)
		{
			return (100 * _value) / _max;
		}

		float Pourcentage_FLOAT(int _value, int _max)
		{
			return (100 * (float)_value) / _max;
		}

		int Pourcentage(long value, long max)
		{
			return (int)((100 * value) / max);
		}

		// Code copié sur le code de C++Buidler
		void CutFirstDirectory(ref string s)
		{
			bool root;
			int p;
			if (s == @"\")
			{
				s = "";
			}
			else
			{
				if (s[0] == '\\')
				{
					root = true;
					s = s.Remove(0, 1);
				}
				else
				{
					root = false;
				}

				if (s[0] == '.')
				{
					s = s.Remove(0, 4);
				}
				p = s.IndexOf('\\');
				if (p >= 0)
				{
					s = s.Remove(0, 1+p);
					s = @"...\" + s;
				}
				else
				{
					s = "";
				}
				if (root)
				{
					s = @"\" + s;
				}
			}
		}

		/// <summary>
		/// Retourne une chaine de caractère de type filename tronqué 'proprement'
		/// </summary>
		/// <param name="graphics">objet de dessin</param>
		/// <param name="font">police de caractère</param>
		/// <param name="maxlen">longeur max en pixel</param>
		/// <param name="filename">nom de fichier complet</param>
		/// <returns></returns>
		string MinimizeName(Graphics graphics, Font font, int maxlen, string filename)
		{
			string result = filename;
			string drive;
			string dir = Path.GetDirectoryName(result);
			string name = Path.GetFileName(result);

			if ((dir.Length >= 2) && (dir[1] == ':'))
			{
				drive = dir.Substring(0,2);
				dir = dir.Remove(0,2);
			}
			else
			{
				drive = "";
			}

			while (((dir.Length > 0) || (drive.Length > 0)) && 
				(graphics.MeasureString(result,font).Width > maxlen) )
			{
				if (dir == @"\...\")
				{
					drive = "";
					dir = @"...\";
				}
				else if (dir.Length == 0)
				{
					drive = "";
				}
				else
				{
					CutFirstDirectory(ref dir);
				}
				result = drive + dir + name;
			}

			return result;
		}

		public FormCheckFiles()
		{
			m_FolderList = new List<TFolderInfo>();
			m_FilesList = new List<TFileInfo>();
			m_FilesListComparer = new ListViewItemComparer();

			InitializeComponent();
			EnabledControls();
		}
		private void FormCheckFiles_Load(object sender, EventArgs e)
		{
			foreach (ColumnHeader column in listViewFiles.Columns) column.ImageIndex = (int)eIMGLIST.EMPTY;

            m_CheckDuplicatesBW = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            m_CheckDuplicatesBW.DoWork += CheckDuplicatesBW_DoWork;
        }

        /// <summary>
        /// Gestion des 'enabled' des boutons en fonction des listes et des process en cours
        /// </summary>
        void EnabledControls()
		{
			buttonScan.Enabled = (m_FolderList.Count > 0) && (buttonStop.Enabled == false);
			buttonAddFolder.Enabled = (buttonStop.Enabled == false);

			// Scan l'état des fichiers (recherche les fichiers non-traités)
			/*int iCtrStatusNone = 0;
			int iCtrStatusCheck = 0;
			foreach (var item in m_FilesList)
			{
				if (item.eStatus == eFILE_STATUS.fsNONE)
				{
					iCtrStatusNone++;
				}
				else
				{
					iCtrStatusCheck++;
				}
			}*/

			buttonGo.Enabled = (m_FilesList.Count > 0) && (buttonStop.Enabled == false);

			/*if (iCtrStatusCheck > 0)
			{
				buttonGo.Text = "Reprendre le test";
				buttonGo.Tag = 1;
			}
			else*/
			if (buttonGo.Tag == null)
			{
				buttonGo.Text = "Démarrer le test";				
			}
			else
			{
				buttonGo.Text = "Reprendre le test";
			}
		}

		/// <summary>
		/// Construit la liste des disques externe (USB) 
		/// </summary>
		/// <param name="usbdrives">liste des disques USB</param>
		void FindUSBExternalDrivers(List<string> usbdrives)
		{
			// Browse all USB WMI physical disks
			foreach (ManagementObject drive in new ManagementObjectSearcher("select DeviceID, MediaType,InterfaceType from Win32_DiskDrive").Get())
			{
				string interfaceType = drive["InterfaceType"].ToString().ToLowerInvariant();
				if (interfaceType == "usb")
				{
					// Associate physical disks with partitions
					ManagementObjectCollection partitionCollection = new ManagementObjectSearcher(string.Format(
						"associators of {{Win32_DiskDrive.DeviceID='{0}'}} " + "where AssocClass = Win32_DiskDriveToDiskPartition", 
						drive["DeviceID"])).Get();
					foreach (ManagementObject partition in partitionCollection)
					{
						if (partition != null)
						{
							// Associate partitions with logical disks (drive letter volumes)
							ManagementObjectCollection logicalCollection = new ManagementObjectSearcher(string.Format(
								"associators of {{Win32_DiskPartition.DeviceID='{0}'}} " + "where AssocClass= Win32_LogicalDiskToPartition", 
								partition["DeviceID"])).Get();
							foreach (ManagementObject logical in logicalCollection)
							{
								if (logical != null)
								{
									// Finally find the logical disk entry
									ManagementObjectCollection.ManagementObjectEnumerator volumeEnumerator = new ManagementObjectSearcher(string.Format(
										"select DeviceID from Win32_LogicalDisk " + "where Name='{0}'", logical["Name"])).Get().GetEnumerator();
									volumeEnumerator.MoveNext();
									ManagementObject volume = (ManagementObject)volumeEnumerator.Current;
									usbdrives.Add(volume["DeviceID"].ToString());
								}
							}
						}
					}

				}
			}
		}

		/// <summary>
		/// Process de la liste des dossiers pour chercher les doublons ou sous-dossiers
		/// </summary>
		void CheckFolderList()
		{
			// TODO: détecter les alias de chemin genre 'I:\' == '\\Diskstation\Data'
			// Init
			foreach (TFolderInfo item in m_FolderList)
			{
				item.status = 0;
				item.statusText = "";
			}

			List<string> usbdrives = new List<string>();
			FindUSBExternalDrivers(usbdrives);

			// Verification des dossiers
			HashSet<string> hostlist = new HashSet<string>();
			foreach (TFolderInfo item in m_FolderList)
			{
				string folder = item.fullFolderName;
				if (Directory.Exists(folder))
				{
					// Attention DriveInfo ne supporte pas le chemin de type UNC (network)
					if (folder.StartsWith(@"\\") == false)
					{
						DriveInfo driveInfo = new DriveInfo(folder);
						if (driveInfo.DriveType == DriveType.Network)
						{
							// Convert from drive path to drive letter (R:\\ => R:)
							string driveletter = driveInfo.Name.Replace(Path.DirectorySeparatorChar.ToString(), "");
							// Query WMI if the drive letter is a network drive, and if so the UNC path for it
							using (ManagementObject mo = new ManagementObject())
							{
								mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", driveletter));
								// Extract computer name from provider
								var toto = mo["ProviderName"];
								Uri uri = new Uri(Convert.ToString(mo["ProviderName"]));
								hostlist.Add(uri.Host);
							}
						}
					}
					else
					{
						Uri uri = new Uri(folder);
						hostlist.Add(uri.Host);
					}

					// Verif si doublon ou sous-dossier
					// => est un opérateur lambda
					// Retourne une liste des dossiers contenant 'folder' et dont l'index est différent du dossier en cours de test
					List<TFolderInfo> flist = m_FolderList.FindAll(x => ((x.fullFolderName.IndexOf(folder) >= 0)&&(item.index!=x.index)));
					if (flist != null)
					{
						foreach (var fitem in flist)
						{
							fitem.statusText = (folder == fitem.fullFolderName) ? "Doublon" : "Sous-dossier";
							fitem.status = 1;
						}
					}
				}
				else
				{
					item.status = 1;
					item.statusText = "N'existe pas !";
				}
			}
			listViewFolders.Refresh();
		}

		/// <summary>
		/// Importe un fichier de la liste des dossiers au format texte de CheckFiles V1
		/// </summary>
		/// <param name="FileName"></param>
		private void ImporterFolderList(string FileName)
		{
			if (File.Exists(FileName))
			{
				// Là on rencontre un problème très con, le lecteur ne sait pas lire les accents si fichier ASCII (UTF-8 par défaut) !
				// Il faut donc forcer le jeu de caractère en latin-1 c'est-à-dire iso-8859-1
				// TODO: détecter le format du fichier ou utiliser un autre type de fichier (XML) ?
				var logFile = File.ReadAllLines(FileName, Encoding.GetEncoding("iso-8859-1"));
				foreach (var line in logFile)
				{
					m_FolderList.Add(new TFolderInfo(line, m_FolderList.Count));
				}
				listViewFolders.VirtualListSize = m_FolderList.Count;
				CheckFolderList();
				EnabledControls();
				ResumeLayout(false);    // Resume layout of the form
			}
		}

		/// <summary>
		/// Exporte la liste des dossiers au format texte de CheckFiles V1
		/// </summary>
		/// <param name="FileName"></param>
		private void ExporterFolderList(string FileName)
		{
			// On force le jeu de caractère en latin-1 c'est-à-dire iso-8859-1
			using (StreamWriter file = new StreamWriter(FileName, false, Encoding.GetEncoding("iso-8859-1")))
			{
				foreach (TFolderInfo item in m_FolderList)
				{
					file.WriteLine(item.fullFolderName);
				}
			}
		}

		/// <summary>
		/// Exporte de la liste des fichiers au format texte de CheckFiles V1
		/// </summary>
		/// <param name="FileName"></param>
		private void ExporterFileList(string FileName)
		{
			// TODO: exporter la liste de fichier en tache de fond
			if (File.Exists(FileName))
			{
				File.Delete(FileName);
			}

			// On force le jeu de caractère en latin-1 c'est-à-dire iso-8859-1
			using (StreamWriter file = new StreamWriter(FileName, false, Encoding.GetEncoding("iso-8859-1")))
			{
				// Options courantes
				file.WriteLine("MD5=1");

				// Liste de dossier
				file.WriteLine("Dossiers={0}", m_FolderList.Count);
				foreach (TFolderInfo item in m_FolderList)
				{
					file.WriteLine(item.fullFolderName);
				}

				// Liste des fichiers et résultat
				file.WriteLine("Fichiers={0}", m_FilesList.Count);
				foreach (TFileInfo item in m_FilesList)
				{
					// format: <FullFileName>=<Taille>|<MD5>?<STATUS>
					// bloc MD5                        <MD5>
					// bloc STATUS                          ?<STATUS>
					StringBuilder str = new StringBuilder(384);

					str.AppendFormat("{0}={1}|", item.fullFileName, item.fileSize);

					str.AppendFormat("{0:X08}", 0);

					if (item.md5 != null)
					{
						string str_md5 = BitConverter.ToString(item.md5).Replace("-", "").ToUpperInvariant();
						str.AppendFormat("*{0}", str_md5);
					}

					str.AppendFormat("?{0}", Convert.ToInt32(item.eStatus));

					file.WriteLine(str);
				}
			}
		}

		/// <summary>
		/// Décodage <name>=<value> de type int
		/// </summary>
		/// <param name="file">object de type StreamReader</param>
		/// <param name="name">name</param>
		/// <param name="vdefault">valeur par défaut</param>
		/// <returns>valeur int</returns>
		private int FileReadInt(StreamReader file, string name, int vdefault )
		{
			int ret = vdefault;
			if (file != null)
			{
				string str = file.ReadLine();
				string[] val = str.Split('=');
				if ((val != null) && (val.Length == 2) && (val[0] == name))
				{
					ret = Convert.ToInt32(val[1]);
				}
			}
			return ret;
		}

		/// <summary>
		/// Décodage <name>=<value> de type bool
		/// </summary>
		/// <param name="file">object de type StreamReader</param>
		/// <param name="name">name</param>
		/// <param name="vdefault">valeur par défaut</param>
		/// <returns>valeur bool</returns>
		private bool FileReadBool(StreamReader file, string name, bool vdefault )
		{
			return Convert.ToBoolean( FileReadInt( file, name, (vdefault)?1:0 ) );
		}

		/// <summary>
		/// Décodage <name>=<value> de type string
		/// </summary>
		/// <param name="file">object de type StreamReader</param>
		/// <returns>valeur string ou null</returns>
		private string[] FileReadString(StreamReader file)
		{
			if (file != null)
			{
				string str = file.ReadLine();
				string[] result = str.Split('=');	// attention si il y a des '=' dans le nom du fichier !
				if (result.Length == 2)
				{
					return result;
				}
				else // Plusieurs '=' -> reconstruire et garder la dernière chaine comme 'value'
				{
					str = string.Join("",result,0, result.Length-1);
					string[] ret = new string[2];
					ret[0] = str;
					ret[1] = result[result.Length - 1];
					return ret;
				}
			}
			return null;
		}

		/// <summary>
		/// Importe un fichier de la liste des fichiers au format texte de CheckFiles V1
		/// </summary>
		/// <param name="fileName"></param>
		private long ImporterFileList(BackgroundWorker worker, string fileName)
		{
			long totalFileSize = 0;

			m_FolderList.Clear();
			m_FilesList.Clear();

			// Check extension
			string ext = Path.GetExtension(fileName);
			if (ext == ".dat")
			{
				// Il faut donc forcer le jeu de caractère en latin-1 c'est-à-dire iso-8859-1
				// Attention ce code ne fonctionne pas si fichier en UTF-89 sans BOM !
				using (StreamReader file = new StreamReader(fileName, Encoding.GetEncoding("iso-8859-1")))
				{
					// Lecture options
					FileReadBool(file, "MD5", false);

					// Liste de dossier
					int folderCount = FileReadInt(file, "Dossiers", 0);
					for (int i = 0; i < folderCount; i++)
					{
						m_FolderList.Add(new TFolderInfo(file.ReadLine(), m_FolderList.Count));
					}

					if (worker != null)
					{
						worker.ReportProgress(0, new TBWScanReport(eBW_REPORT_MODE.CHECK_FOLDER_LIST));
					}
					else
					{
						listViewFolders.VirtualListSize = m_FolderList.Count;
						CheckFolderList();
					}

					// Liste des fichiers et résultats
					int fileCount = FileReadInt(file, "Fichiers", 0);
					for (int f = 0; f < fileCount; f++)
					{
						// format: <FullFileName>=<Taille>|<MD5>?<STATUS>
						// bloc MD5                        <MD5>
						// bloc STATUS                          ?<STATUS>
						TFileInfo fi = new TFileInfo();

						// Décodage infos
						string[] keyValue = FileReadString(file);
						if ((keyValue != null) && (keyValue.Length == 2))
						{
							string name = keyValue[0];
							string value = keyValue[1];

							// Ajout dans la liste des fichiers
							fi.fullFileName = name;

							if (worker != null)
							{
								worker.ReportProgress(Pourcentage(f + 1, fileCount), new TBWScanReport("Load"));
							}
							else
							{
								Application.DoEvents();
								if ((f % 100) == 0)
								{
									toolStripStatusInfo2.Text = string.Format("Load {0,3}%", Pourcentage(f + 1, fileCount));
									statusStrip1.Refresh();
								}
							}

							// INFOS
							// 328456|1F5FDC58*F7784C590FB32498D9897005F6B11B8C?2
							//       |        *                                ?
							// 907543*FFF3973B3B93CF24451CF16CA275C827?1
							//       *                                ?
							// 2456|?0
							//     |?
							// 3730472960?5
							//           ?
							int sizeTAILLE = 0;
							int posCRC32 = value.IndexOf("|");      // optional
							int posMD5 = value.IndexOf("*");     // optional
							int posSTATUS = value.IndexOf("?");     // obligatoire

							string str;
							if (posCRC32 > 0)
							{
								// debut = |, fin = * ou ?
								int length = ((posMD5 > 0) ? (posMD5 - posCRC32 - 1) : (posSTATUS - posCRC32 - 1));
								str = value.Substring(posCRC32 + 1, length);
								//fi.crc32 = (length > 0) ? uint.Parse(str, NumberStyles.HexNumber) : 0;
								sizeTAILLE = posCRC32;
							}

							if (posMD5 > 0)
							{
								string str_md5 = value.Substring(posMD5 + 1, 32);
								// Convert from AABBCCDDEEFF to 0xAA,0xBB,0xCC,0xDD,0xEE,0xFF 
								fi.md5 = ConvertFromString(str_md5);
								if (sizeTAILLE == 0)
								{
									sizeTAILLE = posMD5 - 1;
								}
							}

							if (sizeTAILLE == 0)
							{
								sizeTAILLE = posSTATUS;
							}

							str = value.Substring(posSTATUS + 1, 1);
							fi.eStatus = (eFILE_STATUS)Convert.ToInt32(str);

							str = value.Substring(0, sizeTAILLE);
							fi.fileSize = Convert.ToInt64(str);

							// Complément d'info: type
							fi.type = Path.GetExtension(fi.fullFileName).Replace(".", "");

							m_FilesList.Add(fi);
						}
					}
				}
			}
			else if (ext == ".zip")
			{
				using (ZipArchive archive = ZipFile.OpenRead(fileName))
				{
					foreach (ZipArchiveEntry entry in archive.Entries)
					{
						using (var stream = entry.Open())
						{
							using (var reader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-1")))
							{
								string line;
								while ((line = reader.ReadLine()) != null)
								{
									Thread.Sleep(0);
								}
							}
						}
					}
				}
			}
			else if (ext == ".csv")
			{
				//Format:
				//Groupe<tab>Dossier partagé<tab>Fichier<tab>Taille(Byte)<tab>Heure de modification
				//1<tab>"Dev"<tab>"/volume1/Dev/Dev/Web/php5213/php5213.rar"<tab>14891316<tab>"2012/11/29 18:08:00"
				var logFileTab = File.ReadAllLines(fileName, Encoding.GetEncoding("iso-8859-1"));
				var logFile = new List<string>(logFileTab);
				logFile.RemoveAt(0);
				foreach (var line in logFile)
				{
					string[] dec = line.Split('\t');
					Thread.Sleep(0);

					TFileInfo fi = new TFileInfo();

					fi.folder = dec[1].Trim('"');
					fi.fullFileName = dec[2].Trim('"');
					fi.fileSize = Convert.ToInt64(dec[3]);
					fi.dateTime = DateTime.Parse(dec[4].Trim('"'));
					fi.type = Path.GetExtension(fi.fullFileName).Replace(".", "");

					fi.eStatus = eFILE_STATUS.fsNONE;

					m_FilesList.Add(fi);
				}
			}
			// TODO => construire la liste des dossiers et créer un système d'alias pour pouvoir
			// associer le champs 'Dossier partagé' à un chemin accessible par le PC

			// Calcul la taille total
			foreach (var item in m_FilesList)
			{
				totalFileSize += item.fileSize;
			}

			if (worker == null)
			{
				buttonGo.Tag = 1; // Mode reprise

				listViewFiles.VirtualListSize = m_FilesList.Count;

				toolStripStatusInfo1.Text = "Load OK";
				toolStripStatusInfo2.Text = "100%";
				toolStripStatusInfo3.Text = string.Format("{0} fichiers / {1}", m_FilesList.Count, FileSizeToString(totalFileSize));

				buttonStop.Enabled = false;
				CheckDuplicates(null);
				EnabledControls();
			}
			return totalFileSize;
		}

		/// <summary>
		/// Background Worder: Compte le nombre de dossier sur les n premiers niveaux de dossier 
		/// Attention: ne pas accéder directement à l'interface graphique
		/// </summary>
		/// <param name="dir_info">objet DirectoryInfo</param>
		/// <param name="nbdirs">nombre de dossiers</param>
		/// <param name="maxlevels">nombre de niveau à scanner (0=root)</param>
		/// <param name="level">niveau courant</param>
		void BW_CountDirectories(BackgroundWorker worker, DirectoryInfo dir_info, ref int nbdirs, int maxlevels, int level=0)
		{
			try
			{
				if (level < maxlevels)
				{
					// Création d'une requête LINQ pour filtrer les dossiers qui ne sont pas cachés
					var dirs = from dir in dir_info.EnumerateDirectories()                  // Source de données
							   where ((dir.Attributes & FileAttributes.Hidden) == 0)		// Filtrage
							   select dir;													// Résultat
					// Execution de la requête
					foreach (var di in dirs)
					{
						nbdirs++;
						BW_CountDirectories(worker, di, ref nbdirs, maxlevels, level+1);
					}
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Mode pour TBWScanReport
		/// </summary>
		protected enum eBW_REPORT_MODE
		{
			FILENAME_PROGRESS,	// Compute progress info (filename + %)
			GLOBAL_PROCESS,		// Global progress info (files + size + filename + %)
			LISTVIEW,           // Listview progress info (current_file_index)
			FILENAME,			// Mode affichage folder/filename name
			MESSAGE,			// Mode affichage message + poucentage
			CHECK_FOLDER_LIST,	// Appel de CheckFolderList()
		}
		/// <summary>
		/// Classe pour le report dans un Background Process en mode scan
		/// </summary>
		class TBWScanReport
		{
			public eBW_REPORT_MODE mode;
			public int current_file_index;
			public string fullfilename;
			public string message;

			public TBWScanReport(eBW_REPORT_MODE mode)
			{
				this.mode = mode;
			}
			public TBWScanReport(eBW_REPORT_MODE mode, string filename)
			{
				this.mode = mode;
				fullfilename = filename;
			}
			public TBWScanReport(string message)
			{
				mode = eBW_REPORT_MODE.MESSAGE;
				this.message = message;
			}
		}

		/// <summary>
		/// Background Worder: Scan toute l'arborescence d'un chemin et construit la liste des fichiers
		/// Attention: ne pas accéder directement à l'interface graphique si worker est valide
		/// </summary>
		/// <param name="worker">Background Worder object</param>
		/// <param name="dir_info">objet DirectoryInfo (chemin de base)</param>
		/// <param name="file_list">liste des fichiers TFileInfo</param>
		/// <param name="lasttime">temps de scan (pour l'affichage de la progression)</param>
		/// <param name="maxlevels">nombre de niveau à compter pour le calcul de la progression</param>
		/// <param name="level">niveau courant</param>
		int m_nbdirs, m_ctrdir;
		void BW_SearchDirectory(BackgroundWorker worker, DirectoryInfo dir_info, List<TFileInfo> file_list, ref int lasttime, int maxlevels, int level=0)
		{
			if (lasttime == 0) // start process
			{
				m_ctrdir = 0;
				m_nbdirs = 0;
				lasttime = Environment.TickCount;
				// Compte le nombre de dossiers sur 2 niveaux de sous-dossier (0=root)
				BW_CountDirectories(worker, dir_info, ref m_nbdirs, maxlevels);
			}
			try
			{
				// Scan files sans les fichiers cachés ni les fichiers de taille nulle 
				var files = from file in dir_info.GetFiles()
							where (((file.Attributes & FileAttributes.Hidden) == 0) && (file.Length!=0))
							select file;
				foreach (FileInfo file_info in files)
				{
					TFileInfo item = new TFileInfo();
					item.fullFileName = file_info.FullName;
					item.fileSize = file_info.Length;
					try
					{
						item.dateTime = file_info.LastWriteTime;
					}
					catch
					{
						item.dateTime = file_info.LastAccessTime;
					}
					item.type = Path.GetExtension(item.fullFileName).Replace(".", "");
					file_list.Add(item);

					// Affichage info de suivi toutes les 300 ms
					if ((Environment.TickCount - lasttime) > 300)
					{
						lasttime = Environment.TickCount;
						if (worker != null)
						{
							if (worker.CancellationPending)
							{
								break;
							}
							worker.ReportProgress(0, new TBWScanReport(eBW_REPORT_MODE.FILENAME, file_info.FullName));
						}
						else
						{
							toolStripStatusInfo3.Text = MinimizeName(statusStrip1.CreateGraphics(), toolStripStatusInfo3.Font,
								toolStripStatusInfo3.Width, file_info.FullName);
							Application.DoEvents();
						}
					}
				}

				// Scan folders
				// Création d'une requête LINQ pour filtrer les dossiers qui ne sont pas cachés
				var dirs = from dir in dir_info.EnumerateDirectories()                  // Source de données
						   where ((dir.Attributes & FileAttributes.Hidden) == 0)        // Filtrage
						   select dir;                                                  // Résultat
				// Execution de la requête
				foreach (var subdir_info in dirs)
				{
					if (level < maxlevels)
					{
						m_ctrdir++;
					}
					BW_SearchDirectory(worker,subdir_info, file_list, ref lasttime, maxlevels, level + 1);

					if (worker != null)
					{
						if (worker.CancellationPending)
						{
							break;
						}
						worker.ReportProgress(Pourcentage(m_ctrdir, m_nbdirs), new TBWScanReport("SCAN"));
					}
					else
					{
						toolStripStatusInfo2.Text = string.Format("SCAN {0}%", Pourcentage(m_ctrdir, m_nbdirs));
					}
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				Debug.WriteLine("Folder or file access error {0}", ex.ToString());
			}
			catch
			{
			}
		}

		/// <summary>
		/// Background Worder: ScanFiles
		/// Attention: ne pas accéder directement à l'interface graphique si worker est valide
		/// </summary>
		/// <param name="worker">Background Worder object</param>
		/// <param name="path">chemin de base</param>
		/// <param name="list">liste des fichiers TFileInfo</param>
		void BW_ScanFiles(BackgroundWorker worker, string path, List<TFileInfo> list)
		{
			// Scan le dossier et les sous-dossiers
			try
			{
				Debug.WriteLine("Scan path : {0}", path);
				DirectoryInfo diTop = new DirectoryInfo(path);
				int lasttime = 0;
				BW_SearchDirectory(worker, diTop, list, ref lasttime, 2);
			}
			catch (DirectoryNotFoundException DirNotFound)
			{
				Debug.WriteLine("{0}", DirNotFound.Message);
			}
			catch (UnauthorizedAccessException UnAuthDir)
			{
				Debug.WriteLine("UnAuthDir: {0}", UnAuthDir.Message);
			}
			catch (PathTooLongException LongPath)
			{
				Debug.WriteLine("{0}", LongPath.Message);
			}
		}

		/// <summary>
		/// Classe pour l'implémentation du trie de colonnes
		/// </summary>
		class ListViewItemComparer : IComparer<TFileInfo>
		{
			private eCOLUMNS iLastCmd, iCmd;
			private bool fInv;  // si true on inverse la comparaison

			public ListViewItemComparer()
			{
				iLastCmd = eCOLUMNS.ecFICHIER;
				iCmd = eCOLUMNS.ecFICHIER;
				fInv = false;
			}
			public bool Inverse { get { return fInv; } set { fInv = value; } }
			public void SetCmd(eCOLUMNS cmd)
			{
				iLastCmd = iCmd;
				iCmd = cmd;
			}
			public void CheckInverse()
			{
				fInv = (iCmd == iLastCmd) ? !fInv : false;
			}
			// Return 0 if equal, -1 if fi1 less than fi2, +1 if greater
			// Attention la comparaison doit être cohérente sinon l'algo se plante !
			public int Compare(TFileInfo fi1, TFileInfo fi2)
			{
				int compare = 0;
				switch (iCmd)
				{
					case eCOLUMNS.ecFICHIER: // FullFileName
						{
							compare = fi1.fullFileName.CompareTo(fi2.fullFileName);
						}
						break;

					case eCOLUMNS.ecTAILLE:  // Taille
						{
							if (fi1.fileSize == fi2.fileSize)
							{
								compare = fi1.fullFileName.CompareTo(fi2.fullFileName);
							}
							else
							{
								compare = fi1.fileSize.CompareTo(fi2.fileSize);
							}
						}
						break;

					case eCOLUMNS.ecTYPE:  // Type
						{
							if (fi1.type == fi2.type)
							{
								compare = fi1.fullFileName.CompareTo(fi2.fullFileName);
							}
							else
							{
								compare = fi1.type.CompareTo(fi2.type);
							}
						}
						break;

					case eCOLUMNS.ecDATETIME:   // Date/Time
						{
							if (fi1.dateTime == fi2.dateTime)
							{
								compare = fi1.fullFileName.CompareTo(fi2.fullFileName);
							}
							else
							{
								compare = fi1.dateTime.CompareTo(fi2.dateTime);
							}
						}
						break;

					case eCOLUMNS.ecMD5: // MD5
						{
							byte[] md1 = (fi1.md5 != null) ? fi1.md5 : new byte[16];
							byte[] md2 = (fi2.md5 != null) ? fi2.md5 : new byte[16];
							if (StructuralComparisons.StructuralEqualityComparer.Equals(md1, md2))
							{
								compare = fi1.fullFileName.CompareTo(fi2.fullFileName);
							}
							else
							{
								compare = StructuralComparisons.StructuralComparer.Compare(md1, md2);
							}
						}
						break;

					case eCOLUMNS.ecSTATUS:  // Status
						{
							if (fi1.eStatus == fi2.eStatus)
							{
								compare = fi1.fullFileName.CompareTo(fi2.fullFileName);
							}
							else
							{
								compare = fi1.eStatus.CompareTo(fi2.eStatus);
							}
						}
						break;
				}
				if (fInv)
				{
					compare = (compare > 0) ? -1 : +1;
				}
				return compare;
			}
		}

		/// <summary>
		/// Trie la liste de fichier selon colindex et force_ascendent
		/// Compatible Background Worker
		/// </summary>
		/// <param name="worker"></param>
		/// <param name="index"></param>
		/// <param name="force_ascendent"></param>
		private void BW_SortVirtualList(BackgroundWorker worker, eCOLUMNS column_code, bool force_ascendent)
		{
			List<string> memoSelect = null;
			if (worker == null)
			{
				// Mémorise les sélections
				memoSelect = new List<string>();
				ListView.SelectedIndexCollection indexes = listViewFiles.SelectedIndices;
				foreach (Int32 idx in indexes)
				{
					memoSelect.Add(m_FilesList[idx].fullFileName);
				}
				listViewFiles.SelectedIndices.Clear();
				listViewFiles.BeginUpdate();
			}
			else
			{
				TBWScanReport report = new TBWScanReport(eBW_REPORT_MODE.FILENAME_PROGRESS, "Sort files list");
				worker.ReportProgress(0,report);
			}
			m_FilesListComparer.SetCmd(column_code);
			if (force_ascendent)
				m_FilesListComparer.Inverse = false;
			else
				m_FilesListComparer.CheckInverse();                    // On inverse le sens si clic sur la même colonne

			if (worker == null)
			{
				// On efface les images de trie des colonnes (le -1 ne fonctionne pas si une liste existe !)
				foreach (ColumnHeader column in listViewFiles.Columns) column.ImageIndex = (int)eIMGLIST.EMPTY;
				listViewFiles.Columns[(int)column_code].ImageIndex = (int)((m_FilesListComparer.Inverse) ? eIMGLIST.FLECHE_HAUT : eIMGLIST.FLECHE_BAS);
			}
			// sort en fct de la colonne
			try
			{
				m_FilesList.Sort(m_FilesListComparer);
			}
			catch
			{
				Debug.WriteLine("m_FilesList.Sort Error");
			}

			if (worker == null)
			{
				listViewFiles.EndUpdate();

				// Repositionne les selections
				if ((memoSelect != null) && (memoSelect.Count > 0))
				{
					ListViewItem focusfirst = null;
					foreach (var itemsel in memoSelect)
					{
						ListViewItem listitem = listViewFiles.FindItemWithText(itemsel, false, 0, false);
						if (listitem != null)
						{
							listitem.Selected = true;
							if (focusfirst == null)
							{
								focusfirst = listitem;
							}
						}
					}
					// Focus first
					if (focusfirst != null)
					{
						focusfirst.Focused = true;
						focusfirst.EnsureVisible();
					}
				}
			}
			else
			{
				worker.ReportProgress(100);
			}
		}

		/// <summary>
		/// Retourne false si erreur de lecture du fichier, sinon true 
		/// Attention: ne pas accéder directement à l'interface graphique si worker est valide
		/// </summary>
		/// <param name="worker">Background Worder object</param>
		/// <param name="fi">Item de fichier TFileInfo</param>
		/// <returns></returns>
		private bool BW_ReadFileAndProcess(BackgroundWorker worker, TFileInfo fi, int fileidx)
		{
			bool ret = false;
			bool fReadError = false;
			// Affichage progression uniquement si taille > 50 Mo
			bool fProcessPourcentage = (fi.fileSize > (50 * 1024 * 1024)) && (worker != null);
			if (fProcessPourcentage)
			{
				// Create listview progress info (state 2)
				TBWScanReport report = new TBWScanReport(eBW_REPORT_MODE.LISTVIEW);
				report.current_file_index = fileidx;
				worker.ReportProgress(0, report);
			}
			if (fi.fileSize > 0)
			{
				byte[] buffer = new byte[BLOC_SIZE];
				try
				{

					using (FileStream fs = new FileStream(fi.fullFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						int prc, old_prc = -1;
						Int64 file_size = fi.fileSize;
						Int64 file_read = 0;

						/*FileInfo file_info = new FileInfo(fi.fullFileName);
						try
						{
							fi.dateTime = file_info.LastWriteTime;
						}
						catch
						{
							fi.dateTime = file_info.LastAccessTime;
						}*/

						// On calcul le MD5
						MD5 md5 = MD5.Create();
						md5.Initialize();

						do
						{
							int bloc_size = (file_size > BLOC_SIZE) ? BLOC_SIZE : (int)file_size;
							int bloc_read = fs.Read(buffer, 0, bloc_size);
							if (bloc_size != bloc_read)
							{
								fReadError = true;
								ret = false;
								break;
							}
							else
							{
								file_read += bloc_read;
								md5.TransformBlock(buffer, 0, bloc_size, null, 0);

								if (fProcessPourcentage)
								{
									prc = Pourcentage(file_read, fi.fileSize);
									if (prc != old_prc)
									{
										old_prc = prc;
										// Create report type 0 => compute md5 progress
										TBWScanReport report = new TBWScanReport(eBW_REPORT_MODE.FILENAME_PROGRESS, fi.fullFileName);
										if (worker.CancellationPending)
										{
											break;
										}
										worker.ReportProgress(prc, report);
									}
								}

								file_size -= BLOC_SIZE;
							}
						}
						while (file_size > 0);

						md5.TransformFinalBlock(new byte[0], 0, 0);
						fi.md5 = md5.Hash;
					}   // using FileStream
				}
				catch
				{
					Thread.Sleep(0);
				}

				fi.eStatus = (fReadError) ? eFILE_STATUS.fsERROR : eFILE_STATUS.fsCHECKED;
				ret = !fReadError;
			}
			else // fi.fileSize == 0
			{
				ret = true;
				fi.md5 = null;
				fi.eStatus = eFILE_STATUS.fsCHECKED;
			}
			return ret;
		}

		/// <summary>
		/// Fonction de comparaison des MD5 de 2 éléments de type TFileInfo
		/// </summary>
		/// <param name="e1"></param>
		/// <param name="e2"></param>
		/// <returns></returns>
		private bool CompareMD5(TFileInfo e1, TFileInfo e2)
		{
			bool ret = false;
			if( (e1.eStatus == eFILE_STATUS.fsCHECKED) || (e1.eStatus == eFILE_STATUS.fsDOUBLON) )
			{
				if ( (e1.md5 != null) && (e2.md5 != null) )
				{
					BigInteger md1 = new BigInteger(e1.md5);
					BigInteger md2 = new BigInteger(e2.md5);
					ret = (md2.Equals(md1));
				}
			}
			return ret;
		}

		/// <summary>
		/// Compteur de doublons pour la fonction 'CheckDuplicates'
		/// </summary>
		int m_nbDoublons; 

		/// <summary>
		/// Verif des doublons
		/// </summary>
		/// <param name="worker">Background worker</param>
		/// <returns></returns>
		private int CheckDuplicates(BackgroundWorker worker)
		{
			m_nbDoublons = 0;

			// Check Result
			// On efface le status DOUBLON avant recherche
			foreach (var it in m_FilesList)
			{
				if (it.eStatus == eFILE_STATUS.fsDOUBLON)
				{
					it.eStatus = eFILE_STATUS.fsCHECKED;
				}
			}

			foreach (var it in m_FilesList)
			{
				if (it.eStatus == eFILE_STATUS.fsCHECKED)
				{
					int ctr_dbl = 0;
					foreach (var fd in m_FilesList)
					{
						if (fd != it)
						{
							if (CompareMD5(it, fd))
							{
								if (fd.eStatus == eFILE_STATUS.fsCHECKED)
								{
									m_nbDoublons++;
									ctr_dbl++;
									fd.eStatus = eFILE_STATUS.fsDOUBLON;
								}

								if (fd.eStatus != eFILE_STATUS.fsDELETED)
								{
									if (it.eStatus == eFILE_STATUS.fsCHECKED)
									{
										m_nbDoublons++;
										ctr_dbl++;
										it.eStatus = eFILE_STATUS.fsDOUBLON;
									}
								}
								else
								{
									it.eStatus = eFILE_STATUS.fsCHECKED;    // on annule le status Doublon si deleted !
								}
							}
						}
						if ((worker != null) && (worker.CancellationPending))
						{
							break;
						}
					}
					if ((worker != null) && (worker.CancellationPending))
					{
						break;
					}
					it.ctrDbl = ctr_dbl;
				}
			}
			if (worker == null) listViewFiles.Refresh();

			// fin
			string msg = "";
			if ((m_nbDoublons > 0) || (m_Read_Error > 0))
			{
				msg += "(";
			}
			if (m_nbDoublons > 0)
			{
				msg += string.Format("{0} doublon{1}", m_nbDoublons, (m_nbDoublons > 1) ? 's' : ' ');
			}
			if (m_Read_Error > 0)
			{
				if (m_nbDoublons > 0) msg += ",";
				msg += string.Format("{0} erreur{1}", m_Read_Error, (m_Read_Error > 1) ? 's' : ' ');
			}
			if ((m_nbDoublons > 0) || (m_Read_Error > 0))
			{
				msg += ")";
			}

			if (worker == null) toolStripStatusInfo2.Text = msg;

			return m_nbDoublons;
		}

		/// <summary>
		/// Ouvre un dossier dans l'explorateur de Windows
		/// </summary>
		/// <param name="fullfilename"></param>
		private void OpenFolder(string fullfilename)
		{
			string path = Path.GetDirectoryName(fullfilename);
			if (string.IsNullOrEmpty(path) == false)
			{
				Process.Start(path);
			}
		}


		/// <summary>
		/// Event Click pour le bouton AddFolder
		/// Ouvre une boite de dialogue de Windows pour ajouter un dossier dans la liste des dosiers
		/// </summary>
		/// <param name="sender">bouton</param>
		/// <param name="e">argument</param>
		private void buttonAddFolder_Click(object sender, EventArgs e)
		{
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				string selectedDir = folderBrowserDialog1.SelectedPath;
				m_FolderList.Add(new TFolderInfo(selectedDir, m_FolderList.Count));
				listViewFolders.VirtualListSize = m_FolderList.Count;
				CheckFolderList();
			}
			EnabledControls();
		}

		/// <summary>
		/// Event KeyDown pour la liste listViewFolders
		/// Gestion de la touche DELETE pour supprimer un dossier de la liste des dossiers
		/// </summary>
		/// <param name="sender">liste</param>
		/// <param name="e">argument</param>
		private void listViewFolders_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				ListView.SelectedIndexCollection indexes = listViewFolders.SelectedIndices;
				listViewFolders.BeginUpdate();
				foreach (Int32 idx in indexes)
				{
					m_FolderList[idx].status = 2;	// marque comme deleted
				}
				loop:
				foreach (var item in m_FolderList)
				{
					if (item.status == 2)
					{
						m_FolderList.Remove(item);
						goto loop;
					}
				}
				listViewFolders.VirtualListSize = m_FolderList.Count;
				listViewFolders.EndUpdate();
				CheckFolderList();
				EnabledControls();
			}
		}

		/// <summary>
		/// Event Click pour le bouton buttonScan
		/// Lance la construction de la liste des fichiers à l'aide du Background Worder
		/// </summary>
		/// <param name="sender">bouton</param>
		/// <param name="e">argument</param>
		private void buttonScan_Click(object sender, EventArgs e)
		{
			// Process en tache de fond => BackgroundWorker
			if (backgroundWorker.IsBusy != true)
			{
				buttonStop.Enabled = true;
				buttonStop.Focus();
				EnabledControls();

				toolStripStatusInfo1.Text = "Scan Folders & Files ...";
				toolStripStatusInfo2.Text = "";

				// Clear listViewFiles
				listViewFiles.VirtualListSize = 0;

				// Lance la tache de fond avec le mode construction de la liste des fichiers
				backgroundWorker.RunWorkerAsync(new TBackgroundWorkerCmd(eBW_CMD.BUILD_FILES_LIST));
			}
			else
			{
				// Le Background Worder tourne déjà !
			}
		}

		/// <summary>
		/// Event Click pour le bouton buttonGo
		/// Lance la vérification des doublons en tache de fond (Background Worker)
		/// </summary>
		/// <param name="sender">bouton</param>
		/// <param name="e">argument</param>
		private void buttonGo_Click(object sender, EventArgs e)
		{
            if (m_CheckDuplicatesBW.IsBusy)
            {
                m_CheckDuplicatesBW.CancelAsync();
                while (m_CheckDuplicatesBW.IsBusy)
                    Application.DoEvents();
            }
            // Process en tache de fond => BackgroundWorker
            if (!backgroundWorker.IsBusy)
			{
				buttonStop.Enabled = true;
				buttonStop.Focus();
				EnabledControls();

				toolStripStatusInfo1.Text = "Calcul MD5 ...";
				toolStripStatusInfo2.Text = "";
				toolStripStatusInfo3.Text = "";
				listViewFiles.Focus();

				// Lock listview files pendant le début de l'opération (délock en cours de process)
				listViewFiles.SelectedIndices.Clear();
				listViewFiles.BeginUpdate();

				// Lance la tache de fond avec le mode construction check duplicates
				backgroundWorker.RunWorkerAsync(new TBackgroundWorkerCmd(eBW_CMD.CHECK_DUPLICATES));
			}
			else
			{
				// Le Background Worder tourne déjà !
			}
		}

		/// <summary>
		/// Event Click pour le bouton buttonStop
		/// Arrete le process en cours
		/// </summary>
		/// <param name="sender">bouton</param>
		/// <param name="e">argument</param>
		private void buttonStop_Click(object sender, EventArgs e)
		{
			buttonGo.Tag = 1; // Mode reprise

			// Cancel the asynchronous operation.
			backgroundWorker.CancelAsync();
		}

		/// <summary>
		/// Event RetrieveVirtualItem de la liste listViewFolders
		/// Construit l'élément de la liste virtuelle correspondant à 'e.ItemIndex' dans la liste des dossiers
		/// </summary>
		/// <param name="sender">liste</param>
		/// <param name="e">argument</param>
		private void listViewFolders_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			ListView listview = (ListView)sender;
			TFolderInfo info = m_FolderList[e.ItemIndex];
			ListViewItem lvi = new ListViewItem();

			// Folder
			lvi.Text = info.fullFolderName;

			// Status
			lvi.SubItems.Add(info.statusText);
			
			// Background color
			if (info.status != 0)
			{
				lvi.BackColor = ((e.ItemIndex % 2) != 0) ? COLOR_SAUMON_C : COLOR_SAUMON_N;
			}
			else
			{
				lvi.BackColor = ((e.ItemIndex % 2) != 0) ? CUSTOM_JAUNE : CUSTOM_VERT;
			}
			e.Item = lvi;
		}

		/// <summary>
		/// Event CacheVirtualItems de la liste listViewFiles
		/// </summary>
		/// <param name="sender">liste</param>
		/// <param name="e">argument</param>
		private void listViewFiles_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
		{
			// TODO: coder le cache
		}

		/// <summary>
		/// Event RetrieveVirtualItem de la liste listViewFiles
		/// Construit l'élément de la liste virtuelle correspondant à 'e.ItemIndex' dans la liste des fichiers
		/// </summary>
		/// <param name="sender">liste</param>
		/// <param name="e">argument</param>
		private void listViewFiles_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			ListView listview = (ListView)sender;
			TFileInfo info = m_FilesList[e.ItemIndex];
			ListViewItem lvi = new ListViewItem();

			// Fichier
			int cw = listview.Columns[0].Width - listview.SmallImageList.ImageSize.Width - 20;
			lvi.Text = MinimizeName(listview.CreateGraphics(), listview.Font, cw, info.fullFileName);

			// Taille
			lvi.SubItems.Add(FileSizeToString(info.fileSize));

			// Type (extension)
			lvi.SubItems.Add(info.type);

			// Date
			if (info.dateTime.ToBinary() != 0)
			{
				lvi.SubItems.Add(info.dateTime.ToString());
			}
			else
			{
				lvi.SubItems.Add("");
			}

			// MD5
			if (info.md5 != null)
			{
				string str_md5 = BitConverter.ToString(info.md5).Replace("-", "").ToUpperInvariant();
				lvi.SubItems.Add(str_md5);
			}
			else
			{
				lvi.SubItems.Add("");
			}

			// STATUS
			string str = "";
			switch (info.eStatus)
			{
				case eFILE_STATUS.fsNONE: break;
				case eFILE_STATUS.fsCHECKED: str = "Checked"; lvi.ImageIndex = (int)eIMGLIST.LED_VERTE; break;
				case eFILE_STATUS.fsDOUBLON:
					{
						if (info.ctrDbl > 2)
						{
							str = string.Format("Doublons (x{0})", info.ctrDbl);
						}
						else
						{
							str = "Doublon";
						}
						lvi.ImageIndex = (int)eIMGLIST.LED_BLEU;
					}
					break;
				case eFILE_STATUS.fsERROR: str = "Erreur !"; lvi.ImageIndex = (int)eIMGLIST.LED_ROUGE; break;
				case eFILE_STATUS.fsDELETED: str = "Supprimer !"; lvi.ImageIndex = (int)eIMGLIST.LED_ROUGE; break;
				case eFILE_STATUS.fsNOTEST: str = "Ne pas tester !"; lvi.ImageIndex = (int)eIMGLIST.LED_ORANGE; break;
                case eFILE_STATUS.fsTO_DELETE: str = "A supprimer !"; lvi.ImageIndex = (int)eIMGLIST.LED_ROUGE; break;
                default: str = "?"; break;
			}
			lvi.SubItems.Add(str);

			// Image
			if (info.fInProgress)
			{
				lvi.ImageIndex = (int)eIMGLIST.PETITE_FLECHE;
			}

			// Background color
			if ((info.eStatus == eFILE_STATUS.fsDOUBLON) || (info.eStatus == eFILE_STATUS.fsERROR))
			{
				lvi.BackColor = ((e.ItemIndex % 2) != 0) ? COLOR_SAUMON_C : COLOR_SAUMON_N;
			}
			else
			{
				lvi.BackColor = ((e.ItemIndex % 2) != 0) ? CUSTOM_JAUNE : CUSTOM_VERT;
			}
			e.Item = lvi;
		}

		/// <summary>
		/// Event SearchForVirtualItem de la liste listViewFiles
		/// Permet de rechercher un élément dans la liste virtuelle
		/// </summary>
		/// <param name="sender">liste</param>
		/// <param name="e">argument</param>
		private void listViewFiles_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
		{
			if ((e.IsTextSearch) && (e.IsPrefixSearch == false))
			{
				for (int i = e.StartIndex; i < m_FilesList.Count; i++)
				{
					if (e.Text == m_FilesList[i].fullFileName)
					{
						e.Index = i;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Event Opening du menu popup contextMenuStripFolder
		/// Gestion des enabled des items de ce menu
		/// </summary>
		/// <param name="sender">menu</param>
		/// <param name="e">argument</param>
		private void contextMenuStripFolder_Opening(object sender, CancelEventArgs e)
		{
			if (buttonStop.Enabled)
			{
				e.Cancel = true;
			}
			else
			{
				menuExporterFolder.Enabled = (m_FolderList.Count > 0);
				menuClearFolder.Enabled = menuExporterFolder.Enabled;
			}
		}

		/// <summary>
		/// Event Click du menu menuImporterFolder
		/// </summary>
		/// <param name="sender">menu</param>
		/// <param name="e">argument</param>
		private void menuImporterFolder_Click(object sender, EventArgs e)
		{
			openFileDialog1.Title = "Importer une liste de dossiers";
			openFileDialog1.Filter = "Liste de dossier (*.txt)|*.txt";
			openFileDialog1.DefaultExt = "txt";
            openFileDialog1.InitialDirectory = m_ImporterFolderPath;
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
                m_ImporterFolderPath = Path.GetDirectoryName(openFileDialog1.FileName);
                ImporterFolderList(openFileDialog1.FileName);
			}
		}

		/// <summary>
		/// Event Click du menu menuExporterFolder
		/// </summary>
		/// <param name="sender">menu</param>
		/// <param name="e">argument</param>
		private void menuExporterFolder_Click(object sender, EventArgs e)
		{
			saveFileDialog1.Title = "Exporter une liste de dossiers";
			saveFileDialog1.Filter = "Liste de dossier (*.txt)|*.txt";
			saveFileDialog1.DefaultExt = "txt";
			saveFileDialog1.InitialDirectory = m_ImporterFolderPath;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
                m_ImporterFolderPath = Path.GetDirectoryName(saveFileDialog1.FileName);
                ExporterFolderList(saveFileDialog1.FileName);
			}
		}

		/// <summary>
		/// Event Click du menu menuClearFolder
		/// </summary>
		/// <param name="sender">menu</param>
		/// <param name="e">argument</param>
		private void menuClearFolder_Click(object sender, EventArgs e)
		{
			listViewFolders.VirtualListSize = 0;
			m_FolderList.Clear();

			listViewFiles.VirtualListSize = 0;
			m_FilesList.Clear();
			listViewFiles.Refresh();

			EnabledControls();
		}

		/// <summary>
		/// Event ColumnClick de la liste listViewFiles
		/// Lancer un trie selon la colonne sélectionnée
		/// </summary>
		/// <param name="sender">liste</param>
		/// <param name="e">argument</param>
		private void listViewFiles_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			BW_SortVirtualList(null,(eCOLUMNS)e.Column,false);
		}

		/// <summary>
		/// Event Opening du menu popup contextMenuStripFiles
		/// Gestion des enabled des items de ce menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">argument</param>
		private void contextMenuStripFiles_Opening(object sender, CancelEventArgs e)
		{
			if (buttonStop.Enabled)
			{
				e.Cancel = true;
			}
			else
			{
				menuExporterFiles.Enabled = (listViewFiles.VirtualListSize > 0);

				menuNoTestFile.Enabled = false;
				menuTestFile.Enabled = false;

				bool fFileOK = false;
				bool fFileDeleteOK = false;
				if (listViewFiles.SelectedIndices.Count == 1)
				{
					TFileInfo it = m_FilesList[listViewFiles.SelectedIndices[0]];
					if (it != null)
					{
						if (it.eStatus != eFILE_STATUS.fsDELETED)
						{
							fFileOK = true;
							fFileDeleteOK = true;
						}

						if (it.eStatus == eFILE_STATUS.fsNOTEST)
						{
							menuTestFile.Enabled = fFileOK;
						}
						else
						{
							menuNoTestFile.Enabled = fFileOK;
						}
					}
				}
				else if (listViewFiles.SelectedIndices.Count > 1)
				{
					fFileDeleteOK = true;
					menuNoTestFile.Enabled = true;
					menuTestFile.Enabled = true;
				}
				menuOuvrirDossier.Enabled = fFileOK;
				menuOuvrirFile.Enabled = fFileOK;
				menuDeleteFile.Enabled = fFileDeleteOK;
			}
		}

		/// <summary>
		/// Event Click du menu menuOuvrirDossier
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">argument</param>
		private void menuOuvrirDossier_Click(object sender, EventArgs e)
		{
			ListView.SelectedIndexCollection indexes = listViewFiles.SelectedIndices;
			if (indexes.Count > 0)
			{
				OpenFolder(m_FilesList[indexes[0]].fullFileName);
			}
		}

		/// <summary>
		/// Event Click du menu menuOuvrirFile
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">argument</param>
		private void menuOuvrirFile_Click(object sender, EventArgs e)
		{
			ListView.SelectedIndexCollection indexes = listViewFiles.SelectedIndices;
			if (indexes.Count > 0)
			{
				Process.Start(m_FilesList[indexes[0]].fullFileName);
			}
		}

		/// <summary>
		/// Event Click du menu menuDeleteFile
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">argument</param>
		private void menuDeleteFile_Click(object sender, EventArgs e)
		{
			bool shift_key = ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
			ListView.SelectedIndexCollection indexes = listViewFiles.SelectedIndices;
			foreach (Int32 idx in indexes)
			{
				if (shift_key)
				{
					// Delete direct sans passer par la poubelle  !
					File.Delete(m_FilesList[idx].fullFileName);
					m_FilesList[idx].eStatus = eFILE_STATUS.fsDELETED;
				}
				else
				{
					// Delete dans la poubelle
					try
					{
						FileSystem.DeleteFile(m_FilesList[idx].fullFileName, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
						m_FilesList[idx].eStatus = eFILE_STATUS.fsDELETED;
					}
					catch (System.OperationCanceledException)
					{
					}
				}
			}
			listViewFiles.Refresh();
            //CheckDuplicates(null);
            if (!m_CheckDuplicatesBW.IsBusy)
                m_CheckDuplicatesBW.RunWorkerAsync();
        }

		/// <summary>
		/// Event Click du menu menuNoTestFile
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">argument</param>
		private void menuNoTestFile_Click(object sender, EventArgs e)
		{
			ListView.SelectedIndexCollection indexes = listViewFiles.SelectedIndices;
			foreach (Int32 idx in indexes)
			{
				m_FilesList[idx].eStatus = eFILE_STATUS.fsNOTEST;
			}
			listViewFiles.Refresh();
		}

		/// <summary>
		/// Event Click du menu menuTestFile
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">argument</param>
		private void menuTestFile_Click(object sender, EventArgs e)
		{
			ListView.SelectedIndexCollection indexes = listViewFiles.SelectedIndices;
			foreach (Int32 idx in indexes)
			{
				m_FilesList[idx].Reset();
			}
			listViewFiles.Refresh();
		}

		/// <summary>
		/// Event Click du menu menuExporterFiles
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">argument</param>
		private void menuExporterFiles_Click(object sender, EventArgs e)
		{
			saveFileDialog1.Title = "Exporter le résultat";
			saveFileDialog1.Filter = "Fichier de résultat (*.dat)|*.dat";
			saveFileDialog1.DefaultExt = "dat";
			saveFileDialog1.InitialDirectory = m_ImporterFilesPath;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
                m_ImporterFilesPath = Path.GetDirectoryName(saveFileDialog1.FileName);
                ExporterFileList(saveFileDialog1.FileName);
			}
		}

		/// <summary>
		/// Event Click du menu menuImporterFiles
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">argument</param>
		private void menuImporterFiles_Click(object sender, EventArgs e)
		{
			openFileDialog1.Title = "Importer un fichier de résultat";
			openFileDialog1.Filter = "All Reports Files|*.csv;*.zip;*.dat|Synology Files Report (*.csv;*.zip)|*.csv;*.zip|Fichier de résultat (*.dat)|*.dat";
			openFileDialog1.DefaultExt = "csv";
			openFileDialog1.InitialDirectory = m_ImporterFilesPath;
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string filename = openFileDialog1.FileName;
				if (File.Exists(filename))
				{
                    m_ImporterFilesPath = Path.GetDirectoryName(filename);
                    // On efface les listes et bloque l'interface
                    toolStripStatusInfo3.Text = "Charge le fichier ...";
					listViewFolders.VirtualListSize = 0;
					m_FolderList.Clear();
					listViewFiles.VirtualListSize = 0;
					m_FilesList.Clear();
					buttonStop.Enabled = true;
					buttonStop.Focus();
					EnabledControls();

					// Lance la tache de fond avec le mode construction de la liste des fichiers
					backgroundWorker.RunWorkerAsync(new TBackgroundWorkerCmd(eBW_CMD.IMPORT_FILES_LIST, filename));
				}
			}
		}

		class TScanFolderResult
		{
			public Int64 totalFileSize;
			public int   elapseTime;
		}

		private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = sender as BackgroundWorker;
			if (e.Argument != null)
			{
				TBackgroundWorkerCmd cmd = (TBackgroundWorkerCmd)e.Argument;
				m_BackgroundWorkerCmd = cmd.cmd;
				switch (cmd.cmd)
				{
					case eBW_CMD.BUILD_FILES_LIST:
						{
							TScanFolderResult result = new TScanFolderResult();
							int startTime = Environment.TickCount;

							// Scan des dossiers et sous-dossiers
							m_FilesList.Clear();
							foreach (TFolderInfo item in m_FolderList)
							{
								if ((item != null) && ((int)item.status == 0))
								{
									BW_ScanFiles(worker, item.fullFolderName, m_FilesList);
								}
							}

							if (worker.CancellationPending)
							{
								e.Cancel = true;
							}

							// Calcul la taille total
							result.totalFileSize = 0;
							foreach (var item in m_FilesList)
							{
								result.totalFileSize += item.fileSize;
							}

							int stopTime = Environment.TickCount;
							result.elapseTime = stopTime - startTime;
							e.Result = result;
						}
						break;

					case eBW_CMD.CHECK_DUPLICATES:
						{
							// On trie par taille puis on élimine les fichiers de taille unique ou nulle
							BW_SortVirtualList(worker, eCOLUMNS.ecTAILLE, true);

							// Efface flags de test (ne pas faire si reprise)
							if (buttonGo.Tag == null)
							{
								foreach (var item in m_FilesList)
								{
									// Reset flag
									if ((item.eStatus == eFILE_STATUS.fsCHECKED) || (item.eStatus == eFILE_STATUS.fsDOUBLON))
									{
										item.eStatus = eFILE_STATUS.fsNONE;
									}
									//item.process = false;
								}

								// Elimination des fichiers de taille unique et de taille nulle
								TFileInfo last = null;
								foreach (var item in m_FilesList)
								{
									// Si taille identique alors status = toprocess
									// Si == alors test sauf si no_test
									if (item.eStatus == eFILE_STATUS.fsNONE)
									{
										if ((last != null) && (item.fileSize == last.fileSize) && (item.fileSize > 0))
										{
											//last.process = true;
											//item.process = true;
											item.eStatus = eFILE_STATUS.fsTOPROCESS;
											if (last.eStatus == eFILE_STATUS.fsNONE)
											{
												last.eStatus = eFILE_STATUS.fsTOPROCESS;
											}
										}
									}
									last = item;
								}
							}

							// Comptabilise la taille total des fichiers à traiter (si process = true)
							Int64 totalFilesSize = 0;
							var files = from file in m_FilesList        // Source de données
																		//									where file.process              // Filtrage
										where (file.eStatus == eFILE_STATUS.fsTOPROCESS)    // Filtrage
										select file;                    // Résultat
							foreach (var file in files)
								totalFilesSize += file.fileSize;

#if true
							// Process
							m_Read_Error = 0;
							int lasttime = Environment.TickCount;
							Int64 currentFilesSize = 0;
							for (int i = 0; i < m_FilesList.Count; i++)
							{
								TFileInfo fileitem = m_FilesList[i];
								//if (fileitem.process)
								if (fileitem.eStatus == eFILE_STATUS.fsTOPROCESS)
								{
									fileitem.fInProgress = true;

									if (BW_ReadFileAndProcess(worker, fileitem, i) == false)
									{
										m_Read_Error++;
									}
									currentFilesSize += fileitem.fileSize;

									fileitem.fInProgress = false;
									//fileitem.process = false;

									// Affichage info de suivi toutes les 300 ms
									if ((Environment.TickCount - lasttime) > 300)
									{
										lasttime = Environment.TickCount;

										// Create global progress info (state 1)
										TBWScanReport report = new TBWScanReport(eBW_REPORT_MODE.GLOBAL_PROCESS);
										report.current_file_index = 1 + i;
										report.fullfilename = fileitem.fullFileName;
										worker.ReportProgress(Pourcentage(currentFilesSize, totalFilesSize), report);
									}
								}
								if (worker.CancellationPending)
								{
									e.Cancel = true;
									break;
								}
							}
#endif
							CheckDuplicates(worker);
						}
						break;

					case eBW_CMD.IMPORT_FILES_LIST:
						{
							TScanFolderResult result = new TScanFolderResult();
							result.totalFileSize = ImporterFileList(worker, cmd.filename);
							e.Result = result;
						}
						break;
				}
			}
		}

		private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.UserState != null)
			{
				TBWScanReport report = e.UserState as TBWScanReport;
				switch (report.mode)
				{
					case eBW_REPORT_MODE.FILENAME_PROGRESS:	// Compute progress info (filename + %)
						{
							toolStripStatusInfo3.Text = string.Format("{0} {1,3}%", report.fullfilename, e.ProgressPercentage);
						}
						break;

					case eBW_REPORT_MODE.GLOBAL_PROCESS:	// Global progress info (files + size + filename + %)
						{
							toolStripStatusInfo1.Text = string.Format("Files {0}/{1}", report.current_file_index, m_FilesList.Count);
							toolStripStatusInfo2.Text = string.Format("Size {0,5:0.00}%", e.ProgressPercentage);
							toolStripStatusInfo3.Text = report.fullfilename;

							ListViewItem listitem = listViewFiles.Items[report.current_file_index];
							if (listitem != null)
							{
								listitem.Focused = true;
								listitem.EnsureVisible();
							}
							listViewFiles.Refresh();
						}
						break;

					case eBW_REPORT_MODE.LISTVIEW: // Listview progress info (current_file_index)
						{
							ListViewItem listitem = listViewFiles.Items[ report.current_file_index ];
							if (listitem != null)
							{
								listitem.Focused = true;
								listitem.EnsureVisible();
							}
						}
						break;

					case eBW_REPORT_MODE.FILENAME: // mode affichage folder/filename name
						{
							toolStripStatusInfo3.Text = MinimizeName(statusStrip1.CreateGraphics(), toolStripStatusInfo3.Font,
							toolStripStatusInfo3.Width, report.fullfilename);
						}
						break;

					case eBW_REPORT_MODE.MESSAGE: // mode affichage message + poucentage
						{
							toolStripStatusInfo2.Text = string.Format("{0} {1,3}%", report.message, e.ProgressPercentage);
						}
						break;

					case eBW_REPORT_MODE.CHECK_FOLDER_LIST:
						{
							listViewFolders.VirtualListSize = m_FolderList.Count;
							CheckFolderList();
						}
						break;
				}
			}
			else // pour sort files
			{
				toolStripStatusInfo3.Text = string.Format("Sort files list {0,3}%", e.ProgressPercentage);
				listViewFiles.EndUpdate();
			}
		}

		private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			buttonStop.Enabled = false;
			EnabledControls();

			if (e.Cancelled == true)
			{
				toolStripStatusInfo1.Text = "Canceled !";
				toolStripStatusInfo2.Text = string.Format("{0} fichiers", m_FilesList.Count);
				toolStripStatusInfo3.Text = "";
			}
			else if (e.Error != null)
			{
				toolStripStatusInfo1.Text = "";
				toolStripStatusInfo2.Text = string.Format("{0} fichiers", m_FilesList.Count);
				toolStripStatusInfo3.Text = "Error " + e.Error.Message;
			}
			else 
			{
				// Fin selon commande
				if (m_BackgroundWorkerCmd == eBW_CMD.BUILD_FILES_LIST)
				{
					if (e.Result != null)
					{
						if (e.Result.GetType() == typeof(TScanFolderResult))
						{
							TScanFolderResult result = e.Result as TScanFolderResult;
							toolStripStatusInfo1.Text = "Scan OK";
							toolStripStatusInfo2.Text = string.Format("{0} fichiers / {1}", m_FilesList.Count, FileSizeToString(result.totalFileSize));
							toolStripStatusInfo3.Text = string.Format("Temps de process : {0} ms", result.elapseTime);
						}
					}
				}
				else if (m_BackgroundWorkerCmd == eBW_CMD.CHECK_DUPLICATES)
				{
					buttonGo.Tag = null; // Mode test

					toolStripStatusInfo1.Text = "Compute MD5 OK";
					// Se positionne sur le premier doublon
					if (m_nbDoublons > 0)
					{
						foreach (var item in m_FilesList)
						{
							if (item.eStatus == eFILE_STATUS.fsDOUBLON)
							{
								ListViewItem fitem = listViewFiles.FindItemWithText(item.fullFileName, false, 0, false);
								if (fitem != null)
								{
									fitem.Focused = true;
									fitem.EnsureVisible();
								}
								break;
							}
						}
					}
				}
				else if (m_BackgroundWorkerCmd == eBW_CMD.IMPORT_FILES_LIST)
				{
					buttonGo.Tag = 1; // Mode reprise

					listViewFolders.VirtualListSize = m_FolderList.Count;
					CheckFolderList();

					listViewFiles.VirtualListSize = m_FilesList.Count;

					toolStripStatusInfo1.Text = "Load OK";
					toolStripStatusInfo2.Text = "100%";
					if ((e.Result != null) && (e.Result.GetType() == typeof(TScanFolderResult)))
					{
						TScanFolderResult result = e.Result as TScanFolderResult;
						toolStripStatusInfo3.Text = string.Format("{0} fichiers / {1}", m_FilesList.Count, FileSizeToString(result.totalFileSize));
					}

					buttonStop.Enabled = false;
                    //CheckDuplicates(null);
                    m_CheckDuplicatesBW.RunWorkerAsync();
                    EnabledControls();
				}
			}
			m_BackgroundWorkerCmd = eBW_CMD.NO_CMD;

			listViewFiles.VirtualListSize = m_FilesList.Count;
			listViewFiles.Refresh();
		}

        private void CheckDuplicatesBW_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckDuplicates(sender as BackgroundWorker);
        }

        private void purgerTOUTLesDoublonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment purger TOUT les doublons ?",
                                "CheckFiles",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
#if true
                // on trie par MD5
                toolStripStatusInfo1.Text = "Purge";
                BW_SortVirtualList(null, eCOLUMNS.ecMD5, true);
                // Delete direct sans passer par la poubelle  !
                // On check et on ne garde que le premier de la liste
                byte[] last_md5 = null;
                foreach (var item in m_FilesList)
                {
                    if (item.eStatus == eFILE_STATUS.fsDOUBLON)
                    {
                        bool compare_md5 = StructuralComparisons.StructuralEqualityComparer.Equals(last_md5, item.md5);
                        if (compare_md5)
                        {
                            item.eStatus = eFILE_STATUS.fsTO_DELETE;
                        }
                        else
                        {
                            // on garde le premier !
                            last_md5 = item.md5;
                        }
                    }
                    else
                    {
                        last_md5 = null;
                    }
                }
                int i = 0;
                int last_prc = -1;
                foreach (var item in m_FilesList)
                {
                    if (item.eStatus == eFILE_STATUS.fsTO_DELETE)
                    {
                        item.eStatus = eFILE_STATUS.fsDELETED;
                        File.Delete(item.fullFileName);
                    }
                    int prc = Pourcentage(i, m_FilesList.Count);
                    if (last_prc != prc)
                    {
                        last_prc = prc;
                        toolStripStatusInfo1.Text = string.Format("Files {0}/{1}", i, m_FilesList.Count);
                        toolStripStatusInfo2.Text = string.Format("Size {0,5:0.00}%", Pourcentage(i, m_FilesList.Count));
                        Application.DoEvents();
                    }
                    i++;
                }
                toolStripStatusInfo1.Text = "";
                toolStripStatusInfo2.Text = "";
                toolStripStatusInfo3.Text = "";
                listViewFiles.Refresh();
                if(!m_CheckDuplicatesBW.IsBusy)
                    m_CheckDuplicatesBW.RunWorkerAsync();
#endif
            }
        }

        /// <summary>
        /// Event FormClosing de la fenêtre principale
        /// </summary>
        /// <param name="sender">Form</param>
        /// <param name="e">arguement</param>
        private void FormCheckFiles_FormClosing(object sender, FormClosingEventArgs e)
		{
            if ((int)m_FilesList.Count > 0)
			{
				if (MessageBox.Show("Voulez-vous vraiment quittez le logiciel ?",
									"CheckFiles",
									MessageBoxButtons.YesNo,
									MessageBoxIcon.Question,
									MessageBoxDefaultButton.Button2) == DialogResult.No)
				{
					e.Cancel = true;
				}
				else // dialog result = yes
				{
                    if (m_CheckDuplicatesBW.IsBusy)
                        m_CheckDuplicatesBW.CancelAsync();
                    if (backgroundWorker.IsBusy)
					{
						// Cancel the asynchronous operation.
						backgroundWorker.CancelAsync();
					}
					while (backgroundWorker.IsBusy && m_CheckDuplicatesBW.IsBusy)
						Application.DoEvents();
				}
			}
		}
	}

	// Héritage d'une ListView pour activer le mode Double Buffered !
	public class MyListView : ListView
	{
		public MyListView()
			: base()
		{
			DoubleBuffered = true;
		}
	}
}
