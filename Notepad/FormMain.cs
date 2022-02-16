using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notepad
{
    public partial class FormMain : Form
    {
        private string filePath;
        private string fileName;
        private bool changed;
        //private Font font;
        private Encoding encoding;

        
        
        public FormMain()
        {
            InitializeComponent();
            this.KeyPreview = true;
            encoding = Encoding.UTF8;
            fileName = "Undetected";
            changed = false;
        }


        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Печать без файла");
            }
            else
            {
                OpenPrintDialog(); 
            }
        }

        private void OpenPrintDialog()
        {
            using (PrintDialog printDialog = new PrintDialog())
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = filePath;
                printDocument.PrintPage += PrintPageHandler;
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
        }
        private void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            int charactersOnPage = 0;
            int linesPerPage = 0;
            string stringToPrint = richTextBox1.Text;
            e.Graphics.MeasureString(stringToPrint, this.Font,
                e.MarginBounds.Size, StringFormat.GenericTypographic,
                out charactersOnPage, out linesPerPage);

            e.Graphics.DrawString(stringToPrint, this.Font, Brushes.Black,
                e.MarginBounds, StringFormat.GenericTypographic);

            stringToPrint = stringToPrint.Substring(charactersOnPage);

            e.HasMorePages = (stringToPrint.Length > 0);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileContent = "";
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                //openFileDialog.
                if (openFileDialog.ShowDialog()==DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    fileName = openFileDialog.FileName.Split('\\').Last();
                    var fileStream = openFileDialog.OpenFile();
                    
                    using (StreamReader reader =new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }
            changed = true;
            richTextBox1.Text = fileContent;
            changed = false;
            this.Text = fileName + " - " + Text.Split('-').Last().Trim();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(richTextBox1.Text))
            {
                var result = MessageBox.Show($"Хотите сохранить файл {fileName}",
                    "Блокнот",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1
                    );
                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem.PerformClick();
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
            this.Close();
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.Font = richTextBox1.Font;
                fontDialog.Color = richTextBox1.ForeColor;
                fontDialog.ShowColor = true;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox1.Font = fontDialog.Font;
                    richTextBox1.ForeColor = fontDialog.Color;
                }
            }
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.Control && e.KeyCode ==Keys.S)
            {
                saveToolStripMenuItem.PerformClick();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                saveAsToolStripMenuItem.PerformClick();
                return;
            }
            if (!changed)
            {
                return;
            }
            using (StreamWriter streamWriter = new StreamWriter(filePath, false, encoding))
            {
                foreach (var line in richTextBox1.Text.Split('\n'))
                {
                    streamWriter.WriteLine(line);
                }
            }
            changed = false;
            this.Text = string.Join("*", this.Text.Split('*').Skip(1).ToList());

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!changed)
            {
                return;
            }
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save file";
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FileName = fileName;
                //saveFileDialog.FilterIndex = 2;
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, richTextBox1.Text);
                }
            }
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!changed)
            {
                fileName = "Undetected";
                this.Text = fileName +" - " + Text.Split('-').Last().Trim();
                richTextBox1.Text = string.Empty;
                return;
            }
            if (!string.IsNullOrEmpty(filePath))
            {
                var result = MessageBox.Show($"Вы хотите сохранить изменения в файле" +
                    $" \r\n {filePath}\\{fileName}.txt",
                    "Блокнот",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1
                    );
                if (result==DialogResult.Yes)
                {
                    saveToolStripMenuItem.PerformClick();
                }
                else if (result == DialogResult.No)
                {
                    richTextBox1.Text = string.Empty;
                }
                return;
            }
            if (!string.IsNullOrEmpty(richTextBox1.Text))
            {
                var result =MessageBox.Show($"Хотите сохранить файл {fileName}",
                    "Блокнот",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1
                    );
                if (result == DialogResult.Yes)
                {
                    saveAsToolStripMenuItem.PerformClick();
                }
                else if (result == DialogResult.No)
                {
                    richTextBox1.Text = string.Empty;
                }
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!changed)
            {
                this.Text = "*" + this.Text;
            }
            changed = true;
            
        }
    }
}
