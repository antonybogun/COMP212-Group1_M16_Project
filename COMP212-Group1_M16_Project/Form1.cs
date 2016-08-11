using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;


// There are some methods to support individually create a dictionary and show it up
// The way to show your dictionary up in DataGridView
// 1. Create your own dictionary e.g. publishLetterDic() and add it into 'dicList'
// 2. Call method requestTableByDic(idx) - idx means the order of column name, and please follow the order
// By Jacob 2016/08/03


namespace COMP212_Group1_M16_Project
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> huffman_CodeDic = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog();
        }

        private void encryptBtn_Click(object sender, EventArgs e)
        {
            double sizeCipheredText = 0, sizeClearText = 0;
            using (StreamReader r = new StreamReader(txtPath.Text))
            {
                using (StreamWriter outputFile = new StreamWriter("Huffman_ciphered.txt"))
                {
                    char c;
                    while (r.Peek() != -1)
                    {
                        c = Char.ToUpper((char)r.Read());

                        if (c == ' ' || c == ',' || c == '.' || c == ':')
                        {
                            outputFile.Write(huffman_CodeDic["Space"]);
                            sizeCipheredText += huffman_CodeDic["Space"].Length;
                            sizeClearText++;
                        }
                        else if (c >= 'A' && c <= 'Z')
                        {
                            outputFile.Write(huffman_CodeDic[c.ToString()]);
                            sizeCipheredText += huffman_CodeDic[c.ToString()].Length;
                            sizeClearText++;
                        }
                        else if (c == '\n')
                        {
                            outputFile.WriteLine();
                        }
                    }
                }
            }
            sizeClearText *= 8;
            Console.WriteLine("Total2: " + sizeClearText);
            sizeCipheredTextBox.Text = Math.Round(sizeCipheredText, 0).ToString("#,#");
            sizeClearTextBox.Text = Math.Round(sizeClearText, 0).ToString("#,#");
            compressionRatioTextBox.Text = Math.Round((100 - (100 * (sizeCipheredText / sizeClearText))), 2).ToString() + " %";
            Process.Start("notepad.exe", "Huffman_ciphered.txt");
        }

        private void decryptBtn_Click(object sender, EventArgs e)
        {
            using (StreamReader r = new StreamReader("Huffman_ciphered.txt"))
            {
                String s;
                int i;
                using (StreamWriter outputFile = new StreamWriter("Huffman_decoded.txt"))
                {
                    while (r.Peek() != -1)
                    {
                        s = r.ReadLine();
                        i = 0;
                        while (true)
                        {
                            if (s.Length == i)
                            {
                                outputFile.WriteLine();
                                break;
                            }

                            foreach (var a in huffman_CodeDic)
                            {
                                if (s.IndexOf(a.Value, i) == i)
                                {
                                    outputFile.Write(a.Key == "Space" ? " " : a.Key);
                                    i += a.Value.Length;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            Process.Start("notepad.exe", "Huffman_decoded.txt");

        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Open file dialog and read char by char
        private void openFileDialog()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Open Text File";
            openFileDialog1.Filter = "TXT files|*.txt";
            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    txtPath.Text = openFileDialog1.FileName;
                    publishOccurenceDic(2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error happens while opening the file: " + ex.Message);
                }
            }
        }

        private void publishOccurenceDic(int idx)
        {
            if (txtPath.Text != "")
                using (StreamReader r = new StreamReader(txtPath.Text))
                {
                    char c;
                    ulong totalOccurence = 0;
                    string[] huffArray = new string[] { "100", "0010", "0011", "1111","1110",
                        "1100", "1011", "1010", "0110", "0101", "11011", "01111", "01001", "01000", "00011",
                        "00010", "00001", "00000", "110101", "011101", "011100", "1101001", "110100011",
                        "110100001","110100000", "1101000101","11010001000" };

                    Dictionary<string, uint> asciiDic = new Dictionary<string, uint>();
                    Dictionary<string, ulong> occurenceDic = new Dictionary<string, ulong>();
                    Dictionary<string, double> frequencyDic = new Dictionary<string, double>();
                    Dictionary<string, double> ordered_FrequencyDic = new Dictionary<string, double>();
                    
                    //ASCII Dictionary
                    asciiDic.Add("Space", Convert.ToUInt32(' '));
                    for (c = 'A'; c <= 'Z'; c++)
                    {
                        asciiDic.Add(c.ToString(), Convert.ToUInt32(Char.ToLower(c)));
                    }

                    //Occurence Dictionary
                    occurenceDic.Add("Space", (ulong)0);
                    for (c = 'A'; c <= 'Z'; c++)
                        occurenceDic.Add(c.ToString(), (ulong)0);

                    while (r.Peek() != -1)
                    {
                        c = Char.ToUpper((char)r.Read());
                        if (c == ' ' || c == ',' || c == '.' || c == ':')
                        {
                            occurenceDic["Space"] = (ulong)occurenceDic["Space"] + 1;
                            totalOccurence++;
                        }
                        else if (c >= 'A' && c <= 'Z')
                        {
                            occurenceDic[c.ToString()] = (ulong)occurenceDic[c.ToString()] + 1;
                            totalOccurence++;
                        }
                    }

                    //Frequency Dictionary
                    frequencyDic.Add("Space", (double)Math.Round((Convert.ToDouble(occurenceDic["Space"]) / totalOccurence), 6));
                    for (c = 'A'; c <= 'Z'; c++)
                        frequencyDic.Add(c.ToString(), (double)Math.Round((Convert.ToDouble(occurenceDic[c.ToString()]) / totalOccurence), 6));
                    
                    //Ordered_frequency Dictionary
                    var a = from entry in frequencyDic orderby entry.Value descending select entry;
                    foreach (var b in a)
                    {
                        ordered_FrequencyDic.Add(b.Key, b.Value);
                    }

                    //Huffman_code Dictionary
                    int i = 0;
                    foreach (var entry in ordered_FrequencyDic)
                        huffman_CodeDic.Add(entry.Key, huffArray[i++]);

                    //DataTable source for gridView
                    DataTable table = new DataTable();
                    table.Columns.Add("Letter", typeof(string));
                    table.Columns.Add("ASCII", asciiDic["A"].GetType());
                    table.Columns.Add("Occurence", occurenceDic["A"].GetType());
                    table.Columns.Add("Frequency", frequencyDic["A"].GetType());
                    table.Columns.Add("Huffman_Code", huffman_CodeDic["A"].GetType());
                    foreach (var e in huffman_CodeDic)
                        table.Rows.Add(new Object[] { e.Key, asciiDic[e.Key], occurenceDic[e.Key], frequencyDic[e.Key], e.Value, });
                    dataGridView1.DataSource = table;
                    dataGridView1.RowHeadersVisible = false;
                    dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
                }
        }
    }
}
