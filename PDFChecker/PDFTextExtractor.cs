using System;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Collections.Generic;

namespace PDFChecker {
    class PDFTextExtractor {

        private static char[] NEW_LINE_DELIMITER = new char[] { '\n' };

        private static string[] LINE_START_WORDS = new string[] {
            "Ship",
            "Line",
            "Num"
        };

        public PageText[] ExtractText(PdfReader reader) {

            int numPages = reader.NumberOfPages;

            PageText[] pagesText = new PageText[numPages];

            var pageSize = reader.GetPageSize(1);

            pageSize.Left = pageSize.Right * 0.8315f;
            RenderFilter[] lotNumbersFilter = { new RegionTextRenderFilter(pageSize) };

            pageSize.Right /= 10;
            pageSize.Left = 0;
            RenderFilter[] lineNumbersFilter = { new RegionTextRenderFilter(pageSize) };

            for (int i = 1; i <= numPages; i++) {
                string lotNumbersText = PdfTextExtractor.GetTextFromPage(reader, i,
                    new FilteredTextRenderListener(new LocationTextExtractionStrategy(), lotNumbersFilter));

                string lineNumbersText = PdfTextExtractor.GetTextFromPage(reader, i,
                    new FilteredTextRenderListener(new LocationTextExtractionStrategy(), lineNumbersFilter));

                pagesText[i - 1] = new PageText() {
                    Text = PdfTextExtractor.GetTextFromPage(reader, i),
                    LotNumbers = GetLotNumbers(lotNumbersText, i == 1),
                    LineNumbers = GetLineNumbers(lineNumbersText)
                };
            }

            return pagesText;
        }

        private string[] GetLotNumbers(string lotNumbersText, bool isFirstPage) {
            string[] lines = lotNumbersText.Split(NEW_LINE_DELIMITER);
            List<string> lotNumbers = new List<string>();

            int numHeaderWordsFound = 0;
            foreach (string line in lines) {
                if (line.IndexOf("Document", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                    line.IndexOf("Control", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                    line.IndexOf("Number", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                    numHeaderWordsFound++;
                } else if (numHeaderWordsFound >= 2) {
                    string trimmedLine = line.Trim();
                    int n = trimmedLine.Length;
                    bool lotNumberValid = true;
                    for(int i=0;i<n;i++) {
                        char c = trimmedLine[i];
                        if (c < '0' || c > '9') {
                            lotNumberValid = false;
                            break;
                        }
                    }
                    if (lotNumberValid) {
                        if (isFirstPage) {
                            isFirstPage = false;
                        } else {
                            //skip first lot number on the first page
                            lotNumbers.Add(trimmedLine);
                        }
                    } else {
                        break;
                    }
                }
            }

            return lotNumbers.ToArray();
        }

        private string[] GetLineNumbers(string lineNumbersText) {
            string[] lines = lineNumbersText.Split(NEW_LINE_DELIMITER);
            List<string> lotNumbers = new List<string>();

            int numHeaderWordsFound = 0;
            foreach (string line in lines) {
                if (ContainsWords(line, LINE_START_WORDS)) {
                    numHeaderWordsFound++;
                } else if (numHeaderWordsFound >= 2) {
                    int num;
                    string trimmedLine = line.Trim();
                    if (int.TryParse(trimmedLine, out num)) {
                        lotNumbers.Add(trimmedLine);
                    } else {
                        break;
                    }
                }
            }

            return lotNumbers.ToArray();
        }

        private static bool ContainsWords(string line, string []words) {

            foreach (var word in words) {
                if (ContainsWord(line, word)) {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsWord(string line, string word) {
            int index = line.IndexOf(word, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1) {
                if (index > 0 && line[index - 1] != ' ') {
                    return false;
                }

                if (index + word.Length < line.Length) {
                    return line[index + word.Length] == ' ';
                }

                return true;
            }

            return false;
        }
    }
}
