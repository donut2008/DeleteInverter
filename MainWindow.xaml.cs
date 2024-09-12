using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeleteInverter
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void HelpClick(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Select a folder in which to delete files using the \"Browse for folder button\"." +
				" Select a file list containing the list of files which are NOT to be deleted. An empty list works too," +
				" with the effect that the contents of the folder are deleted without the folder itself." +
				" The file names in the list should be separated by a | (press 'Shift' + '\\' to get it on most keyboards)." +
				"\r\nFILE NAMES ARE CASE SENSITIVE! \"myFile.txt\" will NOT be recognised as \"myfile.txt\", \"Myfile.txt\", or as any other combinations!" +
				"\r\nWhy? Because C# cannot check for equality without ignoring the case, for some reason. Will be implemented soon.",
				"Help",
				MessageBoxButton.OK,
				MessageBoxImage.Information);
		}

		private void BrowseFolder_Click(object sender, RoutedEventArgs e)
		{

		}

		private void BrowseFileList_Click(object sender, RoutedEventArgs e)
		{

		}

		private void CheckBeforeDelete(object sender, RoutedEventArgs e)
		{

		}
	}
}
