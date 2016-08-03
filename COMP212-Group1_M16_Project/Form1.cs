using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        // Include DataGridView column names
        List<string> columnName = new List<string>();
        // Include all tables
        List<DataTable> tableList = new List<DataTable>();
        // Include all dictionaries
        List<Dictionary<string, object>> dicList = new List<Dictionary<string, object>>();
        // Each table's primary key
        readonly string TABLE_PK = "No";

        public Form1()
        {
            InitializeComponent();

            // Add DataGridView column names
            columnName.Add("Letter");
            columnName.Add("ASCII");
            columnName.Add("Occurence");
            columnName.Add("Frequency");
            columnName.Add("Ordered_Frequency");
            columnName.Add("Huffman_Code");

            /*************** CREATE A DICTIONARY AND REQUEST A TABLE BY THE DICTIONARY *************/
            // Create Letter Dictionary for 1st column 
            publishLetterDic(0);
            // Create a table for 1st column 
            requestTableByDic(0);
            // Create ASCII Dictionary for 2nd column 
            publishAsciiDic(1);
            // Create a table for 2nd column 
            requestTableByDic(1);

        
            
            /*************** AFTER ALL DICTIONARIES AND TABLES CREATE ******************************/
            // Merge all tables generated above (GUYS IT IS OKAY TO IGNORE DETAIL ABOUT MergeTables Method)
            DataTable mergedTable = MergeTables(tableList, "No");
            dataGridView1.DataSource = mergedTable;
            dataGridView1.Columns[0].Visible = false;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog();
        }

        private void encryptBtn_Click(object sender, EventArgs e)
        {

        }

        private void decryptBtn_Click(object sender, EventArgs e)
        {

        }

        private void exitBtn_Click(object sender, EventArgs e)
        {

        }

        // Open file dialog and read char by char
        private void openFileDialog()
        {

            StreamReader reader;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Open Text File";
            openFileDialog1.Filter = "TXT files|*.txt";
            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (reader = new StreamReader(openFileDialog1.FileName))
                    {
                        while (reader.Peek() >= 0)
                        {
                            // Read char by char
                            Console.Write((char)reader.Read());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error happens while opening the file: " + ex.Message);
                }
            }
        }
        // Create letter dictionary - 1st column
        private void publishLetterDic(int idx)
        {
            Dictionary<string, object> letterDic = new Dictionary<string, object>();
            letterDic.Add("space", "space");
            for (var letter = 'A'; letter <= 'Z'; letter++)
            {
                Console.WriteLine(letter);
                letterDic.Add(letter.ToString(), letter);
            }

            // Add letter dictioinary into dictionary list
            dicList.Insert(idx, letterDic);

        }
        // Create letter dictionary - 2nd column
        private void publishAsciiDic(int idx)
        {
            var str = " abcdefghijklmnopqrstuvwxyz";
            Dictionary<string, object> asciiDic = new Dictionary<string, object>();
            foreach (char b in str.ToCharArray())
                asciiDic.Add((int)b == 32 ? "space" : b.ToString(), (int)b);

            //alphaASCII.ToList().ForEach(x => Console.WriteLine(x.Key));
            //alphaASCII.ToList().ForEach(x => Console.WriteLine(x.Value));

            // Add ASCII dictioinary into dictionary list
            dicList.Insert(idx, asciiDic);
        }
        
        // Create Occurence dictionary - 3rd column
        private void publishOccurenceDic(int idx)
        {
            Dictionary<string, object> occurenceDic = new Dictionary<string, object>();
            dicList.Insert(idx, occurenceDic);
        }
        // Create Frequency dictionary - 4th column
        private void publishFrequencyDic(int idx)
        {
            Dictionary<string, object> frequencyDic = new Dictionary<string, object>();
            dicList.Insert(idx, frequencyDic);
        }
        // Create Ordered Frequency dictionary - 5th column 
        private void publishOrFrequencyDic(int idx)
        {
            Dictionary<string, object> oFrequencyDic = new Dictionary<string, object>();
            dicList.Insert(idx, oFrequencyDic);
        }
        // Create Ordered Huffman_Code dictionary - 6th column 
        private void publishHufmanCodeDic(int idx)
        {
            Dictionary<string, object> hufmanCodeDic = new Dictionary<string, object>();
            dicList.Insert(idx, hufmanCodeDic);
        }

        // Create DataTable with data of a dictionary 
        private void requestTableByDic(int idx)
        {
            // Create a table each a dictionary
            DataTable table = new DataTable();
            
            // 1st column in a table - primary key and invisible
            table.Columns.Add(TABLE_PK);
            // 2nd column in a table - predefined one of column names
            table.Columns.Add(columnName[idx]);

            // Iterate to get data from a dictionary and put it to a table
            var cnt = 1;
            foreach (var pair in dicList[idx])
            {
                //Console.WriteLine("{0}, {1}", pair.Key, pair.Value);
                table.Rows.Add(cnt, pair.Value);
                cnt++;
            }

            // Add a table into table list
            tableList.Insert(idx, table);
        }

        // Merge all tables and return a final table
        public static DataTable MergeTables(IList<DataTable> tables, String primaryKeyColumn)
        {
            if (tables.Count == 1)
                return tables[0];

            DataTable table = new DataTable("FinalTable");
            table.BeginLoadData();
            foreach (DataTable t in tables)
            {
                table.Merge(t);
            }
            table.EndLoadData();

            if (primaryKeyColumn != null)
            {

                var pkGroups = table.AsEnumerable()
                    .GroupBy(r => r[primaryKeyColumn]);
                var dupGroups = pkGroups.Where(g => g.Count() > 1);
                foreach (var grpDup in dupGroups)
                {
                    // Use first row and modify it
                    DataRow firstRow = grpDup.First();
                    foreach (DataColumn c in table.Columns)
                    {
                        if (firstRow.IsNull(c))
                        {
                            DataRow firstNotNullRow = grpDup.Skip(1).FirstOrDefault(r => !r.IsNull(c));
                            if (firstNotNullRow != null)
                                firstRow[c] = firstNotNullRow[c];
                        }
                    }
                    // Remove all but first row
                    var rowsToRemove = grpDup.Skip(1);
                    foreach (DataRow rowToRemove in rowsToRemove)
                        table.Rows.Remove(rowToRemove);
                }
            }
            return table;
        }
    }
}
