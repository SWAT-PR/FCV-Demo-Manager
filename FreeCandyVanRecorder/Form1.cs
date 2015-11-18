using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;



namespace FreeCandyVanRecorder
{
    public partial class Form1 : Form
    {

        List<LinkItem> tab = new List<LinkItem>() { }; //global list used in local areas
        public Form1(string[] args)
        {
            try
            {
                InitializeComponent();
                using (WebClient client = new WebClient())
                {
                    if (args.Length > 1)
                        string htmlCode = client.DownloadString(args[1]);
                    else
                        string htmlCode = client.DownloadString("http://142.4.195.170/logs/demos/");
                    textBox1.Text = htmlCode;
                }

                var res = cleanCode(textBox1.Text);
                tab = res;
                var i = 5; // skip the tab headers to avoid them being counted as an href here
                var totalFiles = res.Count - 7;
                string s = new StringBuilder().Append(totalFiles).ToString();
                label2.Text = s + " files online";
                while (i < res.Count - 1) // avoid 	changeperm.sh to be counted
                {

                    listBox1.Items.Add(res[i]);
                    i += 1;
                }
                listBox1.SelectedIndex = totalFiles;
                timer1.Start();

            }
            catch
            {
                MessageBox.Show("Impossible to contact the server. (Server down or firewall settings do not allow it)", "Connection problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(1);
            }
        }
       
        public static List<LinkItem> cleanCode(string html)
        {
            List<LinkItem> list = new List<LinkItem>();
            MatchCollection a = Regex.Matches(html, @"(<a.*?>.*?</a>)",RegexOptions.Singleline);
            foreach (Match m in a)
            {
                string value = m.Groups[1].Value;
                LinkItem i = new LinkItem();
                Match b = Regex.Match(value, @"href=\""(.*?)\""",RegexOptions.Singleline);
                if (b.Success)
                {
                    i.Href = b.Groups[1].Value;
                }
                string t = Regex.Replace(value, @"\s*<.*?>\s*", "",RegexOptions.Singleline);
                i.Text = t;
                list.Add(i);               
            }
            return list;
        }


        public struct LinkItem
        {
            public string Href;
            public string Text;

            public override string ToString()
            {
                Text = Text.Replace("auto_", "");
                Text = Text.Replace(".bf2demo", "");
                /* Function : Replace the 3 first _ by - and the last _ by : */
                char[] array = Text.ToCharArray();
                array[4] = '-';
                array[7] = '-';
                array[10] = ' ';  
                array[13] = ':';
                array[16] = ':';
                Text = new string(array);
                /* End of Function */
                return Text;              
            }
        }

        public bool getDemoByName(string file)
        {
            bool s = false;
            if (File.Exists(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ProjectReality\\Profiles\\Default\\demos\\" + file))
            {
                s = true;
            }
            return s;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int j = listBox1.SelectedIndex + 5; //Get selected demo index
            if (getDemoByName(tab[j].Href)) { saveItem.Enabled = false; deleteItem.Enabled = true; } //Check if demo exists in demo directory
            if (getDemoByName(tab[j].Href) == false) { saveItem.Enabled = true; deleteItem.Enabled = false; }
        }

        private void menuItem6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("> Coded in C# by SWAT-" + Environment.NewLine + "> Version 0.1" + Environment.NewLine +Environment.NewLine + "Dont forget to check the forum for updates!", "About", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void deleteItem_Click(object sender, EventArgs e)
        {          
            try
            {
                int j = listBox1.SelectedIndex + 5; //Get selected demo index
                File.Delete(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ProjectReality\\Profiles\\Default\\demos\\" + tab[j].Href);
            }
            catch
            {
                MessageBox.Show("Impossible to delete the file. (File not found or insufficient privileges)","", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
         

        }

        private void saveItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var client = new WebClient())
                {
                    int j = listBox1.SelectedIndex + 5; //Get selected demo index
                    client.DownloadFile("http://142.4.195.170/logs/demos/" + tab[j].Href, System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ProjectReality\\Profiles\\Default\\demos\\" + tab[j].Href);
                }
            }
            catch
            {
                MessageBox.Show("Impossible to download the file. (File not found on server or insufficient privileges)", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


          }

    }  
        
        
    

