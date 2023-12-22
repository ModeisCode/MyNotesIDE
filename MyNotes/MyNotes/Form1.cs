using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MyNotes
{
    public partial class Form1 : Form
    {

        private string WARNING = "[WARNING]:";

        public DirectoryInfo directory = new DirectoryInfo("Notes");
        public List<DirectoryInfo> directoryInfos = new List<DirectoryInfo>();
        public List<FileInfo> fileInfos = new List<FileInfo>();
        public List<TreeNode> dirNodes = new List<TreeNode>();

        public string selectedItem = string.Empty;
        public FileInfo selectedFileInfo = new FileInfo("Notes\\log.txt");

//        public Regex regex;
        public string configAdditionalRegex = "class";
        public string defaultRegex = @"\b(for|int|float|let|mut|impl|ADD|GET|def|method|File|)\b";
        public string keywordPattern = @"\b(for|int|float|let|mut|impl|ADD|GET|def|method|File|)\b";

        private int p1 = 0;
        private int p2 = 0;

        private string selectedTextInListBox = "";

        public Form1()
        {
            InitializeComponent();
            p1 = panel1.Width;
            p2 = panel2.Height;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            isDirExists();
            GetConfigFile();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            isDirExists();
        }

        private void isDirExists() 
        {
            DirectoryInfo directory = new DirectoryInfo("Notes");
            if (!directory.Exists)
            {
                directory.Create();
                richTextBox1.AppendText(WARNING + "[Notes_Folder_does_not_exists]:[*]: It has been added to your workspace.");
                RefreshTreeViewField();
            }
            else 
            {
                RefreshTreeViewField();
            }
        }

        private void RefreshTreeViewField() 
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                node.Remove();
            }

            directoryInfos.Clear();
            fileInfos.Clear();
            dirNodes.Clear();
            // İlk olarak, klasörü bulmak için bir File.DirectoryInfo nesnesi oluşturuyoruz.
            TreeNode root = new TreeNode("Directories");
            treeView1.Nodes.Add(root);

            // Ardından, tüm klasörleri ve dosyaları bulmak için Directory.EnumerateDirectories() ve Directory.EnumerateFiles() metodlarını kullanıyoruz.
            foreach (DirectoryInfo childDir in directory.EnumerateDirectories())
            {
                // Tüm klasörleri Child olarak yeniden adlandırıyoruz.
                richTextBox1.AppendText("Dir:" + childDir.Name + "\n");
                directoryInfos.Add(childDir);
                TreeNode child1 = new TreeNode(childDir.Name);
                dirNodes.Add(child1);
                root.Nodes.Add(child1);

            }

            int c = 0;
            if (dirNodes.Count == directoryInfos.Count)
            {
                foreach (DirectoryInfo dir in directoryInfos)
                {

                    List<string> filenames = new List<string>();

                    foreach (FileInfo file in dir.EnumerateFiles())
                    {
                        // Tüm txt dosyalarını leaf olarak yeniden adlandırıyoruz.
                        if (file.Extension == ".txt")
                        {
                            richTextBox1.AppendText("File:" + file.Name + "\n");
                            fileInfos.Add(file);
                            filenames.Add(file.Name);

                        }
                    }

                    foreach (string name in filenames)
                    {
                        dirNodes[c].Nodes.Add(name);
                    }
                    c++;
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (colorDialog1.ShowDialog() == DialogResult.OK) 
            {
                richTextBox2.BackColor = colorDialog1.Color;
            }

        }

        private void ChgFntBtn_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK) 
            { 
                richTextBox2.Font = fontDialog1.Font;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            richTextBox2.Text = "";
                    foreach (FileInfo file in fileInfos)
                    {
                        richTextBox1.AppendText(file.FullName + "\n" + treeView1.SelectedNode.Text + "\n");
                        if (file.Name == selectedItem)
                        {                       
                            richTextBox1.AppendText("File named" + file.Name + " is reading..\n");
                            if (file.Exists) 
                            {
                                try
                                {
                                    StreamReader sr = new StreamReader(file.FullName);
                                    selectedFileInfo = file;
                                    richTextBox2.Text = sr.ReadToEnd();
                                    sr.Close();
                                    richTextBox1.AppendText("File read successfully.");
                                }
                                catch 
                                {
                                    richTextBox1.AppendText(WARNING + "[COULD_NOT_READ]:[*]: File might does not exists.");
                                }   
                            }
                        }
                    }

                    setColorfulText();
                    

        }

       

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {


            richTextBox1.AppendText( treeView1.SelectedNode.Text + " has selected \n");
            selectedItem = treeView1.SelectedNode.Text;
            label1.Text = selectedItem;

            foreach (FileInfo file in fileInfos)
            {
                if (file.Name == selectedItem)
                {
                    if (file.Exists)
                    {
                        selectedFileInfo = file;
                    }
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBox2.ForeColor = colorDialog1.Color;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (selectedFileInfo.Exists && selectedFileInfo != null)
            {

                    StreamWriter sr = new StreamWriter(selectedFileInfo.FullName);
                    sr.WriteLine(richTextBox2.Text);
                    sr.Close();

            }
            else 
            {
                MessageBox.Show("File does not exists!","ERROR",MessageBoxButtons.OK);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sr = new StreamWriter(saveFileDialog1.FileName);
                sr.WriteLine(richTextBox2.Text);
                sr.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            setColorfulText();
        }

        private void setColorfulText()
        {

            try
            {
                GetRegex(keywordPattern);
            }
            catch (ArgumentException are)
            {
                GetRegex(defaultRegex);
            }


        }

        private void GetRegex(string pattern) 
        {

            // Aramak istediğiniz anahtar kelimeleri buraya ekleyin
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase); // Büyük/küçük harf duyarlılığı olmadan arama yapmak için RegexOptions.IgnoreCase kullanılır

            string text = richTextBox2.Text;
            Color c = richTextBox2.ForeColor;
            int i = richTextBox2.SelectionStart;
            richTextBox2.SuspendLayout(); // Geçici olarak RichTextBox güncellemelerini askıya alın

            int originalSelectionStart = richTextBox2.SelectionStart; // İlk seçim başlangıç noktasını kaydedin
            int originalSelectionLength = richTextBox2.SelectionLength; // İlk seçim uzunluğunu kaydedin
            richTextBox2.SelectAll(); // Tüm metni seçin
            richTextBox2.SelectionColor = c; // Tüm metni siyah renge ayarlayın
            richTextBox2.Select(originalSelectionStart, originalSelectionLength); // İlk seçimi geri yükleyin

            MatchCollection matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                richTextBox2.Select(match.Index, match.Length);
                richTextBox2.SelectionColor = Color.Red;
                //richTextBox2.SelectionFont = new Font(richTextBox2.Font, FontStyle.Bold);
            }

            richTextBox2.ResumeLayout(); // RichTextBox güncellemelerini yeniden etkinleştirin
            richTextBox2.Select(i, 1);
            richTextBox2.SelectionColor = c;


        }

        private void button6_Click(object sender, EventArgs e)
        {
            
            if (!Directory.Exists("Notes\\" + textBox1.Text) && textBox1.Text != string.Empty)
            {
                try
                {
                    Directory.CreateDirectory("Notes\\" + textBox1.Text.Trim());
                    RefreshTreeViewField();
                    richTextBox1.AppendText("!");
                }
                catch (Exception exc)
                {
                    richTextBox1.AppendText(exc.Message);
                }
            }

        }



        private void button7_Click(object sender, EventArgs e)
        {
            GetConfigFile();
        }

        private void GetConfigFile() 
        {
            FileInfo configFile = new FileInfo("settings.config");
            if (configFile.Exists)
            {
                StreamReader sr = new StreamReader(configFile.FullName);
                string allLines = sr.ReadToEnd();
                richTextBox1.AppendText(allLines + "\n");
                foreach (string item in allLines.Split('\n'))
                {
                    try
                    {
                        string itemKey = item.Split(':')[0];
                        string itemvalue = item.Split(':')[1];
                        if (itemKey == "addRegex") { configAdditionalRegex = itemvalue; }
                        keywordPattern = @"\b(for|int|float|let|mut|impl|ADD|GET|def|method|File|" +  configAdditionalRegex.TrimEnd() + @")\b";
                        richTextBox1.AppendText("NEW REGEX > " + configAdditionalRegex +  "," + keywordPattern + "\n");
                        addItemsFromConfigFile("for|int|float|let|mut|impl|ADD|GET|def|method|File|" +  configAdditionalRegex.TrimEnd());
                    }
                    catch { continue; }
                }
                sr.Close();
            }
            else {
                StreamWriter sw = new StreamWriter("settings.config");
                sw.WriteLine("addRegex:NEW|MYFILE|");
                sw.Close();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                richTextBox2.ReadOnly = true;
            }
            else 
            {
                richTextBox2.ReadOnly = false;
            }
        }

        private void addItemsFromConfigFile(string parse_string) 
        {
            listBox1.Items.Clear();
            try
            {
                string[] parsed = parse_string.Split('|');
                foreach (var item in parsed)
                {
                    listBox1.Items.Add(item);
                }
            }
            catch (Exception exc)
            {
                richTextBox1.AppendText(WARNING + "CODE PANEL ADDING ITEMS ERROR" + exc.Message);
            }

        }

        

        private void hideOrOpenLeftPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (panel1.Width != 0)
            {
                panel1.Width = 0;
            }
            else
            {
                panel1.Width = p1;
            }
        }

        private void hideOrOpenBottomPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (panel2.Height != 0)
            {
                panel2.Height = 0;
            }
            else
            {
                panel2.Height = p2;
            }
        }

        private void versionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Version 1.0.0");
        }

        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Space)
            {
                SetVisibleListBox();
                listBox1.Focus();
            }
        }

        private void SetVisibleListBox() 
        {
            listBox1.Visible = true;
            listBox1.Location = PointToClient(Cursor.Position);
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                listBox1.Visible = false;
                richTextBox2.Focus();
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshTreeViewField();
        }
    }
}
