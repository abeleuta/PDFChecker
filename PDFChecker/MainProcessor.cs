using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFChecker {
    class MainProcessor {

        private PDFTextExtractor extractor = new PDFTextExtractor();

        private PDFTextProcessor processor = new PDFTextProcessor();

        public string processPdf(string pdfPath) {

            using (PdfReader reader = new PdfReader(pdfPath)) {
                var pagesText = extractor.ExtractText(reader);
                var processingResult = processor.ProcessPDF(pagesText);
                var missingLots = processor.GetMissingLotNumbers(pdfPath, reader, pagesText, processingResult);
                if (missingLots.Count == 0) {
                    //this.parsingResultsTextBox.Text = "None";
                    return "None";
                } else {

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < missingLots.Count; i++) {
                        sb.Append(missingLots[i]).Append("\r\n");
                    }

                    //this.parsingResultsTextBox.Text = sb.ToString();
                    return sb.ToString();

                }
            }
        }

    }
}
