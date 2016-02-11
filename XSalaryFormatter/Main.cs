using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace XSalaryFormatter
{
    public partial class Main : Form
    {
        private Salary _salary;
        public Main()
        {
            InitializeComponent();
        }

        private void OpenFile(string strFileName)
        {
            btnConvert.Enabled = btnSave.Enabled = false;
            try
            {
                _salary = Salary.ReadFromFile(strFileName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "打开文件出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Render();
            btnConvert.Enabled = true;
        }

        private void Render()
        {
            lblSalarySummary.Visible = true;
            if (string.IsNullOrEmpty(_salary.Header) || _salary.Rows.Count == 0)
            {
                richTextBox1.Clear();
                richTextBox1.AppendText(_salary.RawText);
                lblSalarySummary.Text = string.Format("文件：{0}。", _salary.FileName);
                return;
            }

            
            lblSalarySummary.Text = string.Format("文件：{0}, 包含{1}名职工的工资。", _salary.FileName, _salary.Rows.Count);
            richTextBox1.Clear();
            richTextBox1.AppendText(_salary.Header + Environment.NewLine);
            foreach (string row in _salary.Rows)
            {
                richTextBox1.AppendText(row + Environment.NewLine);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenFile(openFileDialog1.FileName);
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (_salary == null)
            {
                return;
            }
            _salary.Header = null;
            _salary.Rows.Clear();

            using (Stream stream = GenerateStreamFromString(_salary.RawText))
            {
                TextFieldParser parser = new TextFieldParser(stream);
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(",");

                string[] fields;
                int counter = 0;
                while (!parser.EndOfData)
                {
                    fields = parser.ReadFields();
                    if (fields == null || fields.Length == 0)
                        continue;
                    if (counter == 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < fields.Length; i++)
                        {
                            string field = fields[i].Trim().Replace(" ", "").Replace("　", "").Replace("\n", "").Replace("\r", "");
                            field = field.Replace("\"", "\"\"");
                            if (i != fields.Length - 1)
                            {
                                sb.AppendFormat("\"{0}\",", field);
                            }
                            else
                            {
                                sb.AppendFormat("\"{0}\"", field);
                            }
                        }
                        _salary.Header = sb.ToString();
                        counter++;
                    }
                    else
                    {
                        string name = fields[0].Trim().Replace(" ", "").Replace("　", "").Replace("\n", "");
                        if (name.StartsWith("\"") && name.EndsWith("\""))
                        {
                        }
                        else
                        {
                            name = "\"" + name.Replace("\"", "\"\"") + "\"";
                        }
                        fields[0] = name;
                        _salary.Rows.Add(string.Join(",", fields));
                    }
                }
            }

            Render();
            btnSave.Enabled = true;
        }

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_salary == null) {
                return;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open(_salary.FileName, FileMode.Truncate), Encoding.GetEncoding("gb2312")))
                {
                    sw.WriteLine(_salary.Header);
                    foreach (string row in _salary.Rows)
                    {
                        sw.WriteLine(row);
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "保存文件出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("保存成功，请关闭本软件并在OA系统中上传工资表。");
        }
    }

    class Salary
    {
        public static Salary ReadFromFile(string fileName)
        {
            //line = line.Trim().Replace(" ", "").Replace("　", "").Replace("\n", "");
            return new Salary(fileName) { RawText = File.ReadAllText(fileName, Encoding.GetEncoding("gb2312")) };
        }
        public Salary(string fileName)
        {
            FileName = fileName;
            Rows = new List<string>();
        }
        public string FileName { get; set; }
        public string Header { get; set; }
        public List<string> Rows { get; private set; }
        public string RawText { get; set; }
    }
}
