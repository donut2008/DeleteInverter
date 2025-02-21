using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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

		private string[] FileList = [], FileNameList = [], DeletionList = [], ExclusionList = [];

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
			IsFolderSelected = folder != null;

			DirPath.Text = IsFolderSelected ? folder.Path : "";
		}

		private async void BrowseFileList_Click(object sender, RoutedEventArgs e)
		{
			var filePicker = new FileOpenPicker
			{
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary
			};
			filePicker.FileTypeFilter.Add(".txt");


			file = await filePicker.PickSingleFileAsync();
			IsFileListPresent = file != null;

			if (IsFileListPresent)
			{
				ExclusionList = (await FileIO.ReadTextAsync(file)).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
				ListPath.Text = file.Path;

				Print("Files to exclude:");
				foreach (string str in ExclusionList)
					Print(str);
			}

			if (ExclusionList.Length == 0 || ExclusionList == null)
				await FSAccessUnauthorized.ShowAsync();
		}

		private async void HelpClick(object sender, RoutedEventArgs e)
		{
			await HelpDialog.ShowAsync();
		}

		private async void CheckBeforeDelete(object sender, RoutedEventArgs e)
		{
			if (IsFolderSelected && !IsFileListPresent)
			{
				WarnText.Text = $"Are you sure you want to delete ALL files in the folder \"{folder.Path}\"? This cannot be undone!";
				await DeleteAllFilesWarning.ShowAsync();
			}
			else if (!IsFolderSelected)
			{
				await NoFolder.ShowAsync();
			}
			else
				StartDeletion();
		}

		private async void LaunchFSPrivacySettings(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
			if (!result)
				App.Current.Exit();
		}

		private void NoFSAccess_QuitApp(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			App.Current.Exit();
		}

		private async void StartDeletion()
		{
			ProgressBox.Text = "";
			Print("Getting file list...");

			DirectoryInfo dInfo = new(folder.Path);
			try
			{
				FileList = Directory.GetFiles(dInfo.FullName);

				bools = new bool[FileList.Length];
				List<string> fnList = [], delList = new();

				foreach (string str in FileList)
					fnList.Add(str.Substring(str.LastIndexOf('\\') + 1));

				FileNameList = [.. fnList];

				Print("Preparing list of files to delete...");
				foreach (var str in FileNameList)
				{
					if (!ExclusionList.Contains(str))
						bools[Array.IndexOf(FileNameList, str)] = true;
				}

				for (int i = 0; i < FileNameList.Length; i++)
					if (bools[i]) delList.Add(FileList[i]);
				DeletionList = [.. delList];

				Print("Deleting files...");
				foreach (string str in DeletionList)
				{
					Print($"Deleting {str}");
					await Task.Run(() => File.Delete(@str));
					bools[Array.IndexOf(DeletionList, str)] = false;
				}

				Print("Delete success.");
			}
			catch (UnauthorizedAccessException)
			{
				await FSAccessUnauthorized.ShowAsync();
			}
		}

		private void ConfirmAllFilesDeletion(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			StartDeletion();
		}

		private void Print(string message)
		{
			ProgressBox.Text += $"{message}\n";
		}
	}
}
