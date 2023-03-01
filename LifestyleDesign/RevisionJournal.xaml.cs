using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.IO;

namespace LifestyleDesign
{
    /// <summary>
    /// Interaction logic for RevisionJournal.xaml
    /// </summary>
    public partial class RevisionJournal : Window
    {
        string folderPath = "";
        string searchPath1 = @"S:\Shared Folders\-Job Log-\01-Current Jobs\";
        string searchPath2 = @"S:\Shared Folders\-Job log-\02-Completed Jobs\";
       
        public RevisionJournal(string filePath)
        {
            InitializeComponent();

            string[] directory1 = Directory.GetDirectories(searchPath1);
            string[] directory2 = Directory.GetDirectories(searchPath2);

            // search path 1

            foreach (string dir in directory1)
            {
                if(dir.Contains(Globals.JobNumber))
                    folderPath = dir;
            }

            // search path 2

            if(folderPath == "")
            {
                foreach(string dir in directory2)
                {
                    if (dir.Contains(Globals.JobNumber))
                        folderPath = dir;
                }
            }

            if(folderPath == "")
            {
                folderPath = searchPath1;
            }

            tbFileNmae.Text = System.IO.Path.GetFileName(filePath);

            string curFileName = Globals.JobNumber + "_Log.txt";

            todoFilePath = folderPath + @"\" + curFileName;

            ReadToDoFile();
        }

        private void ReadToDoFile()
        {
            if (File.Exists(todoFilePath))
            {
                int counter = 0;
                string[] strings = File.ReadAllLines(todoFilePath);

                foreach (string line in strings)
                {
                    string[] todoData = JournalData.ParseDsiplayString(line);

                    JournalData curToDo = new JournalData(counter + 1, todoData[0], todoData[1]);
                    todoDataList.Add(curToDo);
                    counter++;
                }
            }

            ShowData();
        }

        private void ShowData()
        {
            lbxTasks.ItemsSource = null;
            lbxTasks.ItemsSource = todoDataList;
            lbxTasks.DisplayMemberPath = "Display";
        }

        private void btnAddEdit_Click(object sender, RoutedEventArgs e)
        {

            if (curEdit == null)
            {
                AddToDoItem(tbxItem.Text);
            }
            else
            {
                CompleteEditingItem();
            }

            tbxItem.Text = "";
        }

        private void AddToDoItem(string todoText)
        {
            JournalData curToDo = new JournalData(todoDataList.Count + 1, todoText, "To Do");
            todoDataList.Add(curToDo);

            WriteToDoFile();
        }

        private void WriteToDoFile()
        {
            using (StreamWriter writer = File.CreateText(todoFilePath))
            {
                foreach (JournalData curToDo in lbxTasks.Items)
                {
                    curToDo.UpdateDisplayString();
                    writer.WriteLine(curToDo.Display);
                }
            }

            ShowData();
        }

        private void CompleteEditingItem()
        {
            foreach (JournalData todo in todoDataList)
            {
                if (todo == curEdit)
                    todo.Text = tbxItem.Text;
            }

            curEdit = null;
            tbkAddEdit.Text = "Add Item";
            btnAddEdit.Content = "Add Item";

            WriteToDoFile();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lbxTasks.SelectedItems != null)
            {
                JournalData curToDo = lbxTasks.SelectedItem as JournalData;
                StartEditingItem(curToDo);
            }
        }

        private void StartEditingItem(JournalData curToDo)
        {
            curEdit = curToDo;

            tbkAddEdit.Text = "Update Item";
            btnAddEdit.Content = "Update Item";
            tbxItem.Text = curToDo.Text;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbxTasks.SelectedItems != null)
            {
                JournalData curToDo = lbxTasks.SelectedItem as JournalData;
                RemoveItem(curToDo);
            }
        }

        private void RemoveItem(JournalData curToDo)
        {
            todoDataList.Remove(curToDo);
            ReOrderToDoItems();

            WriteToDoFile();
        }

        private void ReOrderToDoItems()
        {
            for (int i = 0; i < todoDataList.Count; i++)
            {
                todoDataList[i].PositionNumber = i + 1;
                todoDataList[i].UpdateDisplayString();
            }
            WriteToDoFile();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
