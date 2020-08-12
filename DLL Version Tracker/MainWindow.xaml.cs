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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filePath = @"C:\";

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
            
            filePath = filePathTextBox.Text;
            List<string> dllFiles = Directory.GetFiles(filePath, "*.dll", SearchOption.AllDirectories).ToList();
            List<string> exeFiles = Directory.GetFiles(filePath, "*.exe", SearchOption.AllDirectories).ToList();
            List<string> thirdPartyPackages = new List<string>() { "System" };

            thirdPartyPackages = getThirdPartyNames();

            foreach (string file in dllFiles)
                {
                    var info = new FileInfo(file);
                    string fileName = info.Name;

                    bool isThirdParty = checkIsThirdPartyLibrary(fileName, thirdPartyPackages);
                    
                    if (!isThirdParty)
                    {
                        var assemblyVersion = AssemblyName.GetAssemblyName(info.FullName).Version;
                        string fileNameAndAssembly = fileName + "  -  DLL Version: " + assemblyVersion.ToString();

                        outputListView.Items.Add(fileNameAndAssembly);
                    }
                }
                foreach (string file in exeFiles)
                {
                    var info = new FileInfo(file);
                    string fileName = info.Name;
                    var assemblyVersion = AssemblyName.GetAssemblyName(info.FullName).Version;
                    string fileNameAndAssembly = fileName + "  -  EXE Version: " + assemblyVersion.ToString();

                    outputListView.Items.Add(fileNameAndAssembly);
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
                MessageBox.Show("Error!" + e.Message.ToString());
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
                    MessageBox.Show("Third Party Library found and excluding from list: " + filename);
                }
            }
            return isThirdParty;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
