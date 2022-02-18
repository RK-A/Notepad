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
        private Font mainFont;
        private Encoding encoding;

        
        
        public FormMain()
        {
            InitializeComponent();
            this.KeyPreview = true;
            richTextBox1.Height = statusStrip1.Location.Y - statusStrip1.Height;
            //кодировка
            encoding = Encoding.UTF8;   
            //название файла по умолчанию
            fileName = "Undetected";

            // Настройка шрифта используемого по умолчанию 
            mainFont = new Font(Form.DefaultFont.FontFamily, 10F);


            // Минимальные размеры формы
            this.MinimumSize = new Size(500,300);
            // Другие настройки 

            changed = false;
            richTextBox1.ReadOnly = true;

            numericUpDown1.Value = (decimal)richTextBox1.Font.Size;
            numericUpDown1.Minimum = 8;
            numericUpDown1.Maximum = 72;
        }


        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(richTextBox1.Text))
            {
                MessageBox.Show("Пустая печать");
            }
            else
            {
                OpenPrintDialog(); 
            }
        }

        private void RichTextBoxAviable()
        {
            richTextBox1.ReadOnly = false;
            richTextBox1.Font = mainFont;
            numericUpDown1.Value = (decimal)richTextBox1.Font.Size;
        }
        private void OpenPrintDialog()
        {
            using (PrintDialog printDialog = new PrintDialog())
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = fileName;
                printDocument.PrintPage += PrintPageHandler;
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
        }
        private void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            Font font = richTextBox1.Font;
            int charactersOnPage = 0;
            int linesPerPage = 0;
            string stringToPrint = richTextBox1.Text;
            e.Graphics.MeasureString(stringToPrint, font,
                e.MarginBounds.Size, StringFormat.GenericTypographic,
                out charactersOnPage, out linesPerPage);

            e.Graphics.DrawString(stringToPrint, font, Brushes.Black,
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

                    changed = true;
                    richTextBox1.Text = fileContent;
                    changed = false;
                    this.Text = fileName + " - " + Text.Split('-').Last().Trim();
                    RichTextBoxAviable();
                }
            }
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
                RichTextBoxAviable();
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

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            int position = richTextBox1.SelectionStart;
            int line = richTextBox1.GetLineFromCharIndex(position);
            int firstIndex = richTextBox1.GetFirstCharIndexFromLine(line);
            toolStripStatusLabel1.Text = $"Строка-{line+1} стлб-{position-firstIndex}";
        }

        private void richTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int position = richTextBox1.SelectionStart;
            int line = richTextBox1.GetLineFromCharIndex(position);
            int firstIndex = richTextBox1.GetFirstCharIndexFromLine(line);
            toolStripStatusLabel1.Text = $"Строка-{line + 1} стлб-{position - firstIndex}";
        }

        private void FormMain_SizeChanged(object sender, EventArgs e)
        {

            richTextBox1.Height = statusStrip1.Location.Y - statusStrip1.Height;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog()== DialogResult.OK)
            {
                richTextBox1.SelectionColor = colorDialog1.Color;
            }
        }

        private void iToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Font defaultF = richTextBox1.Font;
            if (richTextBox1.SelectionFont.Style == (FontStyle.Italic | FontStyle.Bold))
            {
                richTextBox1.SelectionFont = new Font(defaultF.FontFamily, defaultF.Size, FontStyle.Bold);
                return;
            }
            if (richTextBox1.SelectionFont.Style == FontStyle.Italic)
            {
                richTextBox1.SelectionFont = new Font(defaultF.FontFamily, defaultF.Size, FontStyle.Regular);
                return;
            }
            if (richTextBox1.SelectionFont.Style == FontStyle.Bold)
            {
                richTextBox1.SelectionFont = new Font(defaultF.FontFamily, defaultF.Size, FontStyle.Bold | FontStyle.Italic);
                return;
            }
            richTextBox1.SelectionFont = new Font(defaultF.FontFamily, defaultF.Size, FontStyle.Italic);
        }

        private void bToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Font defaultF = richTextBox1.Font;
            if (richTextBox1.SelectionFont.Style == (FontStyle.Italic | FontStyle.Bold))
            {
                richTextBox1.SelectionFont = new Font(defaultF.FontFamily, defaultF.Size, FontStyle.Italic);
                return;
            }
            if (richTextBox1.SelectionFont.Style == FontStyle.Bold)
            {
                richTextBox1.SelectionFont = new Font(defaultF.FontFamily, defaultF.Size, FontStyle.Regular);
                return;
            }
            if (richTextBox1.SelectionFont.Style == FontStyle.Italic)
            {
                richTextBox1.SelectionFont = new Font(defaultF.FontFamily, defaultF.Size, FontStyle.Bold | FontStyle.Italic);
                return;
            }
            richTextBox1.SelectionFont = new Font(defaultF.FontFamily, defaultF.Size, FontStyle.Bold);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionFont = new Font(richTextBox1.Font.FontFamily, (float)numericUpDown1.Value);

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(richTextBox1.SelectedText))
                Clipboard.SetDataObject(richTextBox1.SelectedText);

            
        }
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();

            if (iData.GetDataPresent(DataFormats.Text))
            {
                richTextBox1.SelectedText = (string)iData.GetData(DataFormats.Text);
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(richTextBox1.SelectedText))
                Clipboard.SetDataObject(richTextBox1.SelectedText);
                richTextBox1.SelectedText = string.Empty;
        }
    }
}
