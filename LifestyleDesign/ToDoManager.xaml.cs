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
    /// Interaction logic for ToDoManager.xaml
    /// </summary>
    public partial class ToDoManager : Window
    {
        string todoFilePath = "";
        BindingList<ToDoData> todoDataList = new BindingList<ToDoData>();
        ToDoData curEdit;

        public ToDoManager(string filePath)
        {
            InitializeComponent();

            tbFileNmae.Text = Path.GetFileName(filePath);

            string curPath = Path.GetDirectoryName(filePath);
            string curFileName = Path.GetFileNameWithoutExtension(filePath);

            todoFilePath = curPath + @"\" + curFileName;

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
                    string[] todoData = ToDoData.ParseDsiplayString(line);

                    ToDoData curToDo = new ToDoData(counter + 1, todoData[0], todoData[1]);
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

        private void AddToDoItem(string todoText)
        {
            ToDoData curToDo = new ToDoData(todoDataList.Count + 1, todoText, "To Do");
            todoDataList.Add(curToDo);

            WriteToDoFile();
        }

        private void RemoveItem(ToDoData curToDo)
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

        private void WriteToDoFile()
        {
            using (StreamWriter writer = File.CreateText(todoFilePath))
            {
                foreach (ToDoData curToDo in lbxTasks.Items)
                {
                    curToDo.UpdateDisplayString();
                    writer.WriteLine(curToDo.Display);
                }
            }

            ShowData();
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

        private void CompleteEditingItem()
        {
            foreach (ToDoData todo in todoDataList)
            {
                if (todo == curEdit)
                    todo.Text = tbxItem.Text;
            }

            curEdit = null;
            lblAddEdit.Text = "Add Item";
            btnAddEdit.Text = "Add Item";

            WriteToDoFile();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            TaskDialog.Show("Edit", "Clicked the Edit button");
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            TaskDialog.Show("Delete", "Clicked the Delete button");
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            TaskDialog.Show("Up", "Clicked the Up button");
        }

        private void btnDn_Click(object sender, RoutedEventArgs e)
        {
            if(lbxTasks.SelectedItems != null)
            {

            }
        }
    }
}
