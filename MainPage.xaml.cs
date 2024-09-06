using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DeleteInverter
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private StorageFolder folder = null;
		private StorageFile file = null;

		private bool IsFolderSelected = false, IsFileListPresent = false;
		private bool[] bools = { };

		private string[] FileList = [], FileNameList = [], DeletionList = [], ExclusionList = [], DeletionPaths = [];

		public MainPage()
		{
			this.InitializeComponent();
			ProgressBox.Text = "No task.";
		}

		private async void BrowseFolder_Click(object sender, RoutedEventArgs e)
		{
			var folderPicker = new FolderPicker
			{
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary
			};
			folderPicker.FileTypeFilter.Add("*");

			folder = await folderPicker.PickSingleFolderAsync();
			if (folder != null)
			{
				DirPath.Text = folder.Path;
				IsFolderSelected = true;
			}
			else
				IsFolderSelected = false;
		}

		private async void BrowseFileList_Click(object sender, RoutedEventArgs e)
		{
			var filePicker = new FileOpenPicker
			{
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary
			};
			filePicker.FileTypeFilter.Add(".txt");

			file = await filePicker.PickSingleFileAsync();

			if (file != null)
			{
				ListPath.Text = file.Path;
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
			if (IsFolderSelected && (file == null))
			{
				WarnText.Text = $"Are you sure you want to delete ALL files in the folder \"{folder.Path}\"? This cannot be undone!";
				await DeleteAllFilesWarning.ShowAsync();
			}
			else if (!IsFolderSelected)
			{
				await NoFolder.ShowAsync();
			}
		}

		private async void StartDeletion()
		{
			ProgressBox.Text = "";
			Print("Getting file list...");

			DirectoryInfo dInfo = new(folder.Path);
			FileList = Directory.GetFiles(dInfo.FullName);

			bools = new bool[FileList.Length];
			List<string> fnList = new(), delList = new();
			int i, n;
			foreach (string str in FileList)
			{
				i = str.LastIndexOf('\\');
				fnList.Add(str.Substring(i + 1));
			}
			FileNameList = [.. fnList];

			Print("Preparing list of files to delete...");
			foreach (var str in FileNameList)
			{
				bool t = ExclusionList.Contains(str);
				if (t) continue;
				else
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
			DeletionList = [.. delList];

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

		private void ConfirmAllFilesDeletion(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			StartDeletion();
		}

		private void Print(string message)
		{
			ProgressBox.Text += $"{message}\n";
		}

		private Task FileListTask()
		{
			if (File.Exists(@file.Path))
				ExclusionList = File.ReadAllText(file.Path).Split('|');
			return Task.CompletedTask;
		}
	}
}
