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
    /// Everything because lazy
    /// </summary>
    /// 
    /// PLAN:
    /// Keep track of which .FNT are modified, at save export 'global supported' .met/.txd overwriting originals, warn if ex mets found
    /// Search also filters by subtitles rather than just FNT parent
    /// Add audio mix support w/ drop down (bonus: preview based on AFS? (require pointing to AFS? / infer based on font folder?))
    ///

    public partial class MainWindow : Window {
        List<FNT> openedFnts;
        string originalPath;
        List<int> filteredIndicies;

        public MainWindow() {
            InitializeComponent();
            openedFnts = new List<FNT>();
            filteredIndicies = new List<int>();
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
            originalPath = dialog.SelectedPath;
            string[] foundFnts = Directory.GetFiles(originalPath, "*_EN.fnt", SearchOption.AllDirectories);
            for (int i = 0; i < foundFnts.Length; i++) {
                byte[] readFile = File.ReadAllBytes(foundFnts[i]);
                FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile);
                openedFnts.Add(newFnt);
                ListBox_OpenedFNTS.Items.Add(foundFnts[i].Split(originalPath + '\\')[1]);
                filteredIndicies.Add(i);
            }
        }

        private void clearData() {
            openedFnts = new List<FNT>();
            ListBox_OpenedFNTS.Items.Clear();
            ListBox_CurrentFNTOpened.Items.Clear();

        }

        private void ListBox_OpenedFNTS_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListBox_CurrentFNTOpened.Items.Clear();
            if (filteredIndicies.Count == 0 || ListBox_OpenedFNTS.SelectedIndex == -1)
                return;
            foreach (String subtitle in openedFnts[filteredIndicies[ListBox_OpenedFNTS.SelectedIndex]].subtitleList) {
                ListBox_CurrentFNTOpened.Items.Add(subtitle);
            }
        }

        private void ListBox_CurrentFNTOpened_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ListBox_CurrentFNTOpened.SelectedItem == null)
                return;
            TextBox_EditSubtitle.Text = ListBox_CurrentFNTOpened.SelectedItem.ToString();
        }

        private void TextBox_SearchText_TextChanged(object sender, TextChangedEventArgs e) {
            ListBox_OpenedFNTS.Items.Clear();
            filteredIndicies = new List<int>();
            for (int font = 0; font < openedFnts.Count; font++) {
                FNT curfnt = openedFnts[font];
                foreach (String subtitle in curfnt.subtitleList) {
                    if (subtitle.Contains(TextBox_SearchText.Text)) {
                        ListBox_OpenedFNTS.Items.Add(curfnt.fileName.Split(originalPath + '\\')[1]);
                        filteredIndicies.Add(font);
                        break;
                    }
                }
            }

        }

        private void Button_SaveCurrentSubtitle_Click(object sender, RoutedEventArgs e) {
            int selectedSubtitle = ListBox_CurrentFNTOpened.SelectedIndex;
            if (selectedSubtitle == -1)
                return;
            openedFnts[filteredIndicies[ListBox_OpenedFNTS.SelectedIndex]].updateSubtitle(selectedSubtitle, TextBox_EditSubtitle.Text);
            ListBox_CurrentFNTOpened.Items[selectedSubtitle] = TextBox_EditSubtitle.Text;
            TextBox_EditSubtitle.Clear();
            //ListBox_CurrentFNTOpened.SelectedItem =;
            
        }
    }
}
