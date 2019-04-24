using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PDFChecker {
    public partial class Form1 : Form {

        private MainProcessor mainProcessor = new MainProcessor();

        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {

            string pdfPath = pdfFileTextBox.Text;
            if (pdfPath == "") {
                MessageBox.Show("Please select file to be processed!");
                return;
            }

            if (!File.Exists(pdfPath)) {
                MessageBox.Show("File does not exist!");
                return;
            }

            try {
                this.parsingResultsTextBox.Text = "";
                button1.Enabled = false;
                this.parsingResultsTextBox.Text = mainProcessor.processPdf(pdfPath);
                //var pagesText = extractor.extractText(pdfPath);
                //var processingResult = processor.processPDF(pagesText);
                //var missingLots = processor.getMissingLotNumbers(pdfPath, pagesText, processingResult);
                //if (missingLots.Count == 0) {
                //    this.parsingResultsTextBox.Text = "None";
                //} else {

                //    StringBuilder sb = new StringBuilder();

                //    for (int i = 0; i < missingLots.Count; i++) {
                //        sb.Append(missingLots[i]).Append("\r\n");
                //    }

                //    this.parsingResultsTextBox.Text = sb.ToString();

                //}

                //this.parsingResultsTextBox.Text = pagesText[4].Text;
                //new PdfImageProcessor().processPage(pdfPath, 4);

            } catch(ParserException ex) {
                parsingResultsTextBox.Text = ex.Message;
            } catch(Exception ex) {
                MessageBox.Show("Failed to process PDF file!");
            } finally {
                button1.Enabled = true;
            }
        }

        private void selPDFButton_Click(object sender, EventArgs e) {
            var fileDialog = new OpenFileDialog() {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false,
                RestoreDirectory = true
            };

            if (fileDialog.ShowDialog() == DialogResult.OK) {
                pdfFileTextBox.Text = fileDialog.FileName;
            }
        }
    }
}
