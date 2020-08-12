using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;

namespace DLL_Version_Tracker
{
    public partial class MainWindow : Window
    {
        private string filePath = @"";

        public MainWindow()
        {
            InitializeComponent();
            filePathTextBox.Text = filePath;
        }

        private void checkButton_Click(object sender, RoutedEventArgs e)
        {
            loadDllFiles();
        }

        public void loadDllFiles()
        {
            try
            {
                outputListView.Items.Add("START FILE SEARCH: ");
                outputListView.Items.Add("-------------------");
                filePath = filePathTextBox.Text;
                List<string> dllFiles = Directory.GetFiles(filePath, "*.dll", SearchOption.AllDirectories).ToList();
                List<string> exeFiles = Directory.GetFiles(filePath, "*.exe", SearchOption.AllDirectories).ToList();
                List<string> thirdPartyPackages = new List<string>() { "System" };
                List<string> assemblyVersions = new List<string>();
                thirdPartyPackages = getThirdPartyNames();

                foreach (string file in dllFiles)
                {
                    var info = new FileInfo(file);
                    string fileName = info.Name;

                    bool isThirdParty = checkIsThirdPartyLibrary(fileName, thirdPartyPackages);

                    if (!isThirdParty)
                    {
                        var assemblyVersion = AssemblyName.GetAssemblyName(info.FullName).Version;
                        assemblyVersions.Add(assemblyVersion.ToString());
                        string fileNameAndAssembly = fileName + "  -  DLL Version: " + assemblyVersion.ToString();
                        outputListView.Items.Add(fileNameAndAssembly);
                    }
                }
                foreach (string file in exeFiles)
                {
                    var info = new FileInfo(file);
                    string fileName = info.Name;
                    var assemblyVersion = AssemblyName.GetAssemblyName(info.FullName).Version;
                    assemblyVersions.Add(assemblyVersion.ToString());
                    string fileNameAndAssembly = fileName + "  -  EXE Version: " + assemblyVersion.ToString();

                    outputListView.Items.Add(fileNameAndAssembly);
                }
                string averageVersion = getAverageAssemblyVersion(assemblyVersions);

                outputListView.Items.Add("-------------------");
                outputListView.Items.Add("END FILE SEARCH");
                outputListView.Items.Add("");
                outputListView.Items.Add("**AVERAGE VERSION:**");
                outputListView.Items.Add(averageVersion);
                outputListView.Items.Add("");
                outputListView.Items.Add(@"!!! POTENTIALLY OUTDATED OR ERRONEOUS LIBRARIES !!!");
                foreach (string file in dllFiles)
                {
                    var info = new FileInfo(file);
                    string fileName = info.Name;

                    bool isThirdParty = checkIsThirdPartyLibrary(fileName, thirdPartyPackages);

                    if (!isThirdParty)
                    {
                        var assemblyVersion = AssemblyName.GetAssemblyName(info.FullName).Version;
                        assemblyVersions.Add(assemblyVersion.ToString());
                        string fileNameAndAssembly = fileName + "  -  DLL Version: " + assemblyVersion.ToString();
                        if (assemblyVersion.ToString() != averageVersion)
                        {
                            outputListView.Items.Add(fileNameAndAssembly);
                        }

                    }
                }
                foreach (string file in exeFiles)
                {
                    var info = new FileInfo(file);
                    string fileName = info.Name;
                    var assemblyVersion = AssemblyName.GetAssemblyName(info.FullName).Version;
                    assemblyVersions.Add(assemblyVersion.ToString());
                    string fileNameAndAssembly = fileName + "  -  EXE Version: " + assemblyVersion.ToString();
                    if (assemblyVersion.ToString() != averageVersion)
                    {
                        outputListView.Items.Add(fileNameAndAssembly);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR! - " + e.Message.ToString());
                outputListView.Items.Clear();
            }
        }

        private List<string> getThirdPartyNames()
        {
            List<string> thirdPartyPackages = new List<string>() { "System" };
            try
            {
                DirectoryInfo parentDir0 = Directory.GetParent(filePath);
                DirectoryInfo parentDir1 = Directory.GetParent(parentDir0.FullName);
                using (XmlReader reader = XmlReader.Create(parentDir1.FullName + @"\packages.config"))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name.ToString())
                            {
                                case "package":
                                    thirdPartyPackages.Add(reader.GetAttribute(0));
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR! - " + e.Message.ToString());
                outputListView.Items.Clear();
            }
            return thirdPartyPackages;
        }

        private bool checkIsThirdPartyLibrary(string filename, List<string> thirdPartyList)
        {
            string thirdParty = "";
            bool isThirdParty = false;

            for (int i = 0; i < thirdPartyList.Count; i++)
            {
                if (thirdPartyList[i].Contains('.'))
                {
                    thirdParty = thirdPartyList[i].Substring(0, thirdPartyList[i].IndexOf('.'));
                }
                else
                {
                    thirdParty = thirdPartyList[i].Substring(0, thirdPartyList[i].Length);
                }
                string pattern = $@"\b{thirdParty}\w*\b";
                Match m = Regex.Match(filename, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    isThirdParty = true;
                }
            }
            return isThirdParty;
        }

        private string getAverageAssemblyVersion(List<string> versions)
        {
            string averageVersion;
            var freqs = GetFrequencies(versions);
            averageVersion = DisplaySortedFrequencies(freqs);

            return averageVersion;
        }

        private Dictionary<string,int> GetFrequencies(List<string> versions)
        {
            var result = new Dictionary<string, int>();
            foreach (string value in versions)
            {
                if (result.TryGetValue(value, out int count))
                {
                    result[value] = count + 1;
                }
                else
                {
                    result.Add(value, 1);
                }
            }
            return result;
        }

        private string DisplaySortedFrequencies(Dictionary<string, int> frequencies)
        {
            var sorted = from pair in frequencies
                         orderby pair.Value descending
                         select pair;

            return sorted.ElementAt(0).Key;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Hyperlink_RequestNavigate(object sender,
                                       System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
