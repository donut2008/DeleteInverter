using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using Windows.Storage.Pickers;
using ui = ModernWpf.Controls;


namespace DeleteInverter
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string file = null, folder = null;

		private bool IsFolderSelected = false, IsFileListPresent = false;
		private bool[] bools = { };

		private string[] FileList, FileNameList, DeletionList, ExclusionList;

		public MainWindow()
		{
			InitializeComponent();
			ProgressBox.Text = "No task.";
		}

		private void BrowseFolder_Click(object sender, RoutedEventArgs e)
		{
			var folderPicker = new FolderBrowserDialog
			{
				Description = "Select folder",
				UseDescriptionForTitle = true,
				SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar,
				ShowNewFolderButton = false
			};

			if (folderPicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				folder = DirPath.Text = folderPicker.SelectedPath;
				IsFolderSelected = true;
			}
			else
				IsFolderSelected = false;
		}

		private async void BrowseFileList_Click(object sender, RoutedEventArgs e)
		{
			var filePicker = new OpenFileDialog
			{
				FileName = "exclusions",
				DefaultExt = ".txt",
				Filter = "Text documents (.txt)|*.txt"
			};

			if (filePicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				file = ListPath.Text = filePicker.FileName;
				IsFileListPresent = true;
				await Task.Run(() => FileListTask());
				ProgressBox.Text = "";
				Print("Files to exclude:");
				foreach (string str in ExclusionList)
					Print(str);
			}
			else IsFileListPresent = false;
		}

		private async void HelpClick(object sender, RoutedEventArgs e)
		{
			await HelpDialog.ShowAsync();
		}

		private async void CheckBeforeDelete(object sender, RoutedEventArgs e)
		{
			if (IsFolderSelected && !IsFileListPresent)
			{
				WarnText.Text = $"Are you sure you want to delete ALL files in the folder \"{folder}\"? This cannot be undone!";
				await DeleteAllFilesWarning.ShowAsync();
			}
			else if (!IsFolderSelected)
				await NoFolder.ShowAsync();
			else
				StartDeletion();
		}

		private async void StartDeletion()
		{
			ProgressBox.Text = "";
			Print("Getting file list...");

			DirectoryInfo dInfo = new DirectoryInfo(folder);
			FileList = Directory.GetFiles(dInfo.FullName);

			bools = new bool[FileList.Length];
			List<string> fnList = new List<string>(), delList = new List<string>();
			int i, n;
			foreach (string str in FileList)
			{
				i = str.LastIndexOf('\\');
				fnList.Add(str.Substring(i + 1));
			}
			FileNameList = fnList.ToArray();

			Print("Preparing list of files to delete...");
			foreach (var str in FileNameList)
			{
				if (!string.IsNullOrEmpty(str))
					if (!ExclusionList.Contains(str))
					{
						n = Array.IndexOf(FileNameList, str);
						bools[n] = true;
					}
			}

			for (i = 0; i < FileNameList.Length; i++)
			{
				if (bools[i])
					delList.Add(FileList[i]);
			}
			DeletionList = delList.ToArray();

			Print("Deleting files...");
			n = 0;
			foreach (string str in DeletionList)
			{
				Print($"Deleting {str}");
				await Task.Run(() => File.Delete(@str));
				bools[n++] = false;
			}

			Print("Delete success.");
		}

		private void ConfirmAllFilesDeletion(ui.ContentDialog sender, ui.ContentDialogButtonClickEventArgs args)
		{
			StartDeletion();
		}

		private void Print(string message)
		{
			ProgressBox.Text += $"{message}\n";
		}

		private Task FileListTask()
		{
			if (File.Exists(@file))
				ExclusionList = File.ReadAllText(file).Split("\r\n");
			return Task.CompletedTask;
		}
	}
}
