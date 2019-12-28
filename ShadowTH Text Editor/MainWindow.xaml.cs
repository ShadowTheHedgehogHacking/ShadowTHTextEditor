using ShadowFNT.Structures;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ShadowTH_Text_Editor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        List<FNT> openedFnts;

        public MainWindow() {
            InitializeComponent();
            openedFnts = new List<FNT>();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == false) {
                return;
            }
            if (!dialog.SelectedPath.EndsWith("fonts")) {
                MessageBox.Show("Pick the 'fonts' folder extracted from Shadow The Hedgehog", "Try Again");
                return;
            }
            clearData();
            string originalPath = dialog.SelectedPath;
            string[] foundFnts = Directory.GetFiles(dialog.SelectedPath, "*_EN.fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++) {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile);
                openedFnts.Add(newFnt);
                ListBox_OpenedFNTS.Items.Add(foundFnts[i].Split(dialog.SelectedPath + '\\')[1]);
            }
        }

        private void clearData() {
            openedFnts = new List<FNT>();
            ListBox_OpenedFNTS.Items.Clear();
            ListBox_CurrentFNTOpened.Items.Clear();

        }

        private void ListBox_OpenedFNTS_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListBox_CurrentFNTOpened.Items.Clear();
            foreach (String subtitle in openedFnts[ListBox_OpenedFNTS.SelectedIndex].subtitleList) {
                ListBox_CurrentFNTOpened.Items.Add(subtitle);
            }
        }

        private void ListBox_CurrentFNTOpened_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            TextBox_EditSubtitle.Text = ListBox_CurrentFNTOpened.SelectedItem.ToString();
        }
    }
}
