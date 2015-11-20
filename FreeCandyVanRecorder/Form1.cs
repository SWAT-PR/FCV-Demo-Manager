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

        public Form1(){InitializeComponent();doIt();}
       
        public static List<LinkItem> cleanCode(string html){List<LinkItem> list = new List<LinkItem>(); MatchCollection a = Regex.Matches(html, @"(<a.*?>.*?</a>)",RegexOptions.Singleline); foreach (Match m in a){string value = m.Groups[1].Value;LinkItem i = new LinkItem(); Match b = Regex.Match(value, @"href=\""(.*?)\""",RegexOptions.Singleline);if (b.Success){i.Href = b.Groups[1].Value; } string t = Regex.Replace(value, @"\s*<.*?>\s*", "",RegexOptions.Singleline);i.Text = t;list.Add(i);}return list; }


        public struct LinkItem
        {
            public string Href; public string Text; public string CopyText;
            public override string ToString()
            {
                Text = Text.Replace("auto_", "");Text = Text.Replace(".bf2demo", ""); char[] c = Text.ToCharArray();

                /*Get the current year*/
                char[] annee = { '0', '0', '0', '0' };
                int i = 0; while (i <= 3) { annee[i] = c[i]; i += 1; }
                string a = new string(annee);

                /*Get the current day*/
                char[] jour = { '0', '0' };
                i = 8; int k = 0; while (i <= 9) { jour[k] = c[i]; k += 1; i += 1; }
                string j = new string(jour);

                /*Get the current month and change it to a name*/
                string[] m_mois = new string[] { "none", "Jan", "Feb", "Mar", "May", "Apr", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};
                char[] mois = { '0', '0' };
                i = 5; k = 0; while (i <= 6) { mois[k] = c[i]; i += 1; k += 1; }
                string x = new string(mois); int y = int.Parse(x); string m = m_mois[y];

                /*Get the current time*/
                char[] heure = { '0', '0' }; char[] minute = { '0', '0' }; char[] secondes = { '0', '0' };
                heure[0] = c[11]; heure[1] = c[12]; minute[0] = c[14]; minute[1] = c[15]; secondes[0] = c[17]; secondes[1] = c[18];
                string h = new string(heure); string min = new string(minute); string sec = new string(secondes);
                string time = h + ":" + min + ":" + sec;
                string output = j + " " + m + " " + a + " " + time; Text = output;
             
                if (File.Exists(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ProjectReality\\Profiles\\Default\\demos\\" + Href)){Text = Text + "   (Saved)";}
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

        public void doIt() // Loads all the demos and sets their attributes : saved or empty(=not saved)
        {
            try
            {   
                using (WebClient client = new WebClient())
                {
                    string htmlCode = client.DownloadString("http://142.4.195.170/logs/demos/");
                    textBox1.Text = htmlCode;
                }
                var res = cleanCode(textBox1.Text);
                tab = res;
                var i = 5;  //skip the tab headers to avoid them being counted as an href here
                var totalFiles = res.Count - 7;
                string s = new StringBuilder().Append(totalFiles).ToString();
                label2.Text = s + " files online";
                while (i < res.Count - 1) // avoid 	changeperm.sh to be counted
                {
                    listBox1.Items.Add(res[i]);
                    i += 1;
                }
                listBox1.SelectedIndex = totalFiles; //totalFiles
                timer1.Start();
            }
            catch
            {
                MessageBox.Show("Impossible to contact the server. (Server down or firewall settings do not allow it)", "Connection problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(1);
            }
        }

   
        public void clearIt() { listBox1.Items.Clear(); } //We need to delete all the items
        public void resetIt() { clearIt(); doIt(); } //Reloads everything as if we had restarted the program

        private void timer1_Tick(object sender, EventArgs e)
        {
            string lel = new StringBuilder().Append(listBox1.SelectedIndex).ToString();
            nb.Text = lel;
            int j = listBox1.SelectedIndex + 5; //Get selected demo index
            if (getDemoByName(tab[j].Href)) { saveItem.Enabled = false; deleteItem.Enabled = true; } //Check if demo exists in demo directory
            if (getDemoByName(tab[j].Href) == false) { saveItem.Enabled = true; deleteItem.Enabled = false; }
        }

        private void menuItem6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("> Coded in C# by SWAT-" + Environment.NewLine + "> Version 0.2" + Environment.NewLine +Environment.NewLine + "Dont forget to check the forum for updates!", "About", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void deleteItem_Click(object sender, EventArgs e)
        {          
            try
            {
                int j = listBox1.SelectedIndex + 5; //Get selected demo index
                File.Delete(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ProjectReality\\Profiles\\Default\\demos\\" + tab[j].Href);
                resetIt();
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
                    saveItem.Enabled = false;
                    deleteItem.Enabled = false;
                    timer1.Stop();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
	                 client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    int j = listBox1.SelectedIndex + 5; //Get selected demo index
                   // client.DownloadFile("http://142.4.195.170/logs/demos/" + tab[j].Href, System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ProjectReality\\Profiles\\Default\\demos\\" + tab[j].Href);
                    Form1.ActiveForm.Size = new System.Drawing.Size(284, 386);
                    client.DownloadFileAsync(new Uri("http://142.4.195.170/logs/demos/" + tab[j].Href), System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ProjectReality\\Profiles\\Default\\demos\\" + tab[j].Href);
                }
            }
            catch
            {
                MessageBox.Show("Impossible to download the file. (File not found on server or insufficient privileges)", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            downloadStatus.Text = Math.Round(percentage) + " % saved";
            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            resetIt(); // Now that our file is downloaded, let's refresh the list
            timer1.Start();
            deleteItem.Enabled = true;
            downloadStatus.Text = "Downloaded";
            Form1.ActiveForm.Size = new System.Drawing.Size(284,358);
        }

        private void Form1_Load(object sender, EventArgs e){}

        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        /*public void minimalist()
        {
            while (listBox1.Items.Count > 10)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                listBox1.SetSelected(0, true);
            }
            listBox1.SetSelected(listBox1.Items.Count-1, true);
        }*/
          }

    }  
