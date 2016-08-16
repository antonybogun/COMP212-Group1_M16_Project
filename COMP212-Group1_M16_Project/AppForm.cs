using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace COMP212_Group1_M16_Project
{
    public partial class AppForm : Form
    {
        Dictionary<string, string> huffman_CodeDic = new Dictionary<string, string>();
        Dictionary<string, string> asciiDic = new Dictionary<string, string>();
        Dictionary<string, ulong> occurenceDic = new Dictionary<string, ulong>();
        Dictionary<string, double> frequencyDic = new Dictionary<string, double>();
        Dictionary<string, double> ordered_FrequencyDic = new Dictionary<string, double>();

        string workingDir;

        public AppForm()
        {
            // Form Constructor
            InitializeComponent();
            encryptBtn.Enabled = false;
            decryptBtn.Enabled = false;
            txtPath.ReadOnly = true;
            // Creation ASCII Dictionary
            asciiDic.Add("Space", Convert.ToString(Convert.ToUInt32(' '), 2).PadLeft(8, '0') + " [" + Convert.ToUInt32(' ') + "]");
            for (char c = 'A'; c <= 'Z'; c++)
                asciiDic.Add(c.ToString(),  Convert.ToString(Convert.ToUInt32(Char.ToLower(c)),2).PadLeft(8,'0') + " [" + Convert.ToUInt32(Char.ToLower(c)) + "]");

            // Creation table as a source of dataGridView
            DataTable table = createTable();

            // Populating table with the ASCII Dictionary
            foreach (var e in asciiDic)
                table.Rows.Add(new Object[] { e.Key, asciiDic[e.Key] });

            // Binding dataGridView to table and configuring appearance
            dataGridView.DataSource = table;
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
        }

        // Opening a file dialog and saving working directory
        private void openFileDialog()
        {
            OpenFileDialog openFileDialogWindow = new OpenFileDialog();
            openFileDialogWindow.Title = "Open Text File";
            openFileDialogWindow.Filter = "TXT files|*.txt";
            openFileDialogWindow.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (openFileDialogWindow.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    txtPath.Text = openFileDialogWindow.FileName;
                    workingDir = openFileDialogWindow.FileName.Substring(0, openFileDialogWindow.FileName.LastIndexOf('\\')) + '\\';
                    publishDictionaries();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error happened while opening and processing the file: " + ex.Message);
                }
            }
        }

        //Generating and publishing all the dictionaries
        private void publishDictionaries()
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

                    // Creation Occurence Dictionary
                    occurenceDic.Add("Space", 0);
                    for (c = 'A'; c <= 'Z'; c++)
                        occurenceDic.Add(c.ToString(), 0);
                    while (r.Peek() != -1)
                    {
                        c = Char.ToUpper((char)r.Read());
                        if (c == ' ' || c == ',' || c == ';' || c == ':')
                        {
                            occurenceDic["Space"]++;
                            totalOccurence++;
                        }
                        else if (c >= 'A' && c <= 'Z')
                        {
                            occurenceDic[c.ToString()]++;
                            totalOccurence++;
                        }
                    }

                    // Creation Frequency Dictionary
                    frequencyDic.Add("Space", Math.Round((Convert.ToDouble(occurenceDic["Space"]) / totalOccurence), 6));
                    for (c = 'A'; c <= 'Z'; c++)
                        frequencyDic.Add(c.ToString(), Math.Round((Convert.ToDouble(occurenceDic[c.ToString()]) / totalOccurence), 6));

                    //Creation Ordered_frequency Dictionary
                    var sorted = from entry in frequencyDic orderby entry.Value descending select entry;
                    foreach (var entry in sorted)
                        ordered_FrequencyDic.Add(entry.Key, entry.Value);

                    // Creation Huffman_code Dictionary
                    int i = 0;
                    foreach (var entry in ordered_FrequencyDic)
                        huffman_CodeDic.Add(entry.Key, huffArray[i++]);

                    // Creation a table as a source of dataGridView;
                    DataTable table = createTable();

                    // Populating table with the ASCII Dictionary
                    foreach (var e in huffman_CodeDic)
                        table.Rows.Add(new Object[] { e.Key, asciiDic[e.Key], occurenceDic[e.Key], frequencyDic[e.Key], e.Value });

                    // Binding dataGridView to table and configuring appearance
                    dataGridView.DataSource = table;
                    dataGridView.RowHeadersVisible = false;
                    dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

                    MessageBox.Show("File is processed successfully. Now you can encrypt it");
                    encryptBtn.Enabled = true;
                }
        }

        // Utilities
        private DataTable createTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Letter", typeof(string));
            table.Columns.Add("ASCII", (asciiDic.GetType().GetGenericArguments())[1]);
            table.Columns.Add("Occurence", (occurenceDic.GetType().GetGenericArguments())[1]);
            table.Columns.Add("Frequency", (frequencyDic.GetType().GetGenericArguments())[1]);
            table.Columns.Add("Huffman_Code", (huffman_CodeDic.GetType().GetGenericArguments())[1]);
            return table;
        }

        // Event handlers
        private void encryptBtn_Click(object sender, EventArgs e)
        {
            ulong sizeCipheredText = 0, sizeClearText = 0;

            // Reading each letter from a source file and writing correspondence Huffman value to Huffman_ciphered.txt
            try
            {
                using (StreamReader r = new StreamReader(txtPath.Text))
                {
                    using (StreamWriter outputFile = new StreamWriter(workingDir + "Huffman_ciphered.txt"))
                    {
                        char c;
                        
                        // Checking if there is a character in the file 
                        while (r.Peek() != -1)
                        {
                            // Read a character from a source file and increment occurance if applicable
                            c = Char.ToUpper((char)r.Read());
                            if (c == ' ' || c == ',' || c == ';' || c == ':')
                            {
                                outputFile.Write(huffman_CodeDic["Space"]);
                                // Increase a sizeCipheredText by the number of digits of correspondence Huffman code value (length of huffman_CodeDic value)
                                sizeCipheredText += (ulong)huffman_CodeDic["Space"].Length;
                                sizeClearText++;
                            }
                            else if (c >= 'A' && c <= 'Z')
                            {
                                outputFile.Write(huffman_CodeDic[c.ToString()]);
                                sizeCipheredText += (ulong)huffman_CodeDic[c.ToString()].Length;
                                sizeClearText++;
                            }
                            // If the character is the end of the line, put the end of the line in Huffman_ciphered.txt
                            // For allignment purposes
                            else if (c == '\n')
                                outputFile.WriteLine();
                        }
                    }
                }
                
                // Calculate compression ratio

                //Size of text in bites (each character is 8 bites)
                sizeClearText *= 8;

                sizeCipheredTextBox.Text = sizeCipheredText.ToString("#,#");
                sizeClearTextBox.Text = sizeClearText.ToString("#,#");
                compressionRatioTextBox.Text = Math.Round((double)(100 - (100 * ((double)sizeCipheredText / sizeClearText))), 2).ToString() + " %";

                MessageBox.Show("File is encrypted successfully. Now you can decrypt it");
                decryptBtn.Enabled = true;
                Process.Start("notepad.exe", workingDir+"Huffman_ciphered.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error happens while encrypting the file: " + ex.Message);
            }
        }

        private void decryptBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader r = new StreamReader(workingDir+"Huffman_ciphered.txt"))
                {
                    using (StreamWriter outputFile = new StreamWriter(workingDir+"Huffman_decoded.txt"))
                    {
                        string s;
                        int i;

                        // Checking if there is a character in the file
                        while (r.Peek() != -1)
                        {
                            // Read line from the Huffman_ciphered.txt file
                            s = r.ReadLine();
                            i = 0;
                            while (true)
                            {
                                // If there is no more character in the line, finish processing it and end line in the Huffman_decoded.txt
                                if (s.Length == i)
                                {
                                    outputFile.WriteLine();
                                    break;
                                }

                                // Processing of which Huffman code value is in the current position of the string 
                                foreach (var a in huffman_CodeDic)
                                    if (s.IndexOf(a.Value, i) == i)
                                    {
                                        // Writing a correspondence character in Huffman_decoded.txt
                                        outputFile.Write(a.Key == "Space" ? " " : a.Key);
                                        // Moving index to next Huffman code value in the Huffman_ciphered.txt
                                        i += a.Value.Length;
                                        break;
                                    }
                            }
                        }
                    }
                }
                MessageBox.Show("File is decrypted succesfully!");
                Process.Start("notepad.exe", workingDir+"Huffman_decoded.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error happened while decrypting the file: " + ex.Message);
            }        
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog();
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }       
    }
}
