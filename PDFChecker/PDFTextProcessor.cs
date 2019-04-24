using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PDFChecker {
    class PDFTextProcessor {

        private static char[] NEW_LINE_DELIMITER = new char[] { '\n' };

        private static string[] IGNORED_WORDS = new string[] {
            "Manufacture", "Document", "Quantity", "Shipment", "Number"
        };

        private PdfImageProcessor pdfImageProcessor = new PdfImageProcessor();

        public ProcessingResult processPDF(PageText[] pagesText) {

            int numPages = pagesText.Length;
            int numPagesWithLots = getNumPagesWithLots(pagesText[0].Text);

            if (numPagesWithLots == 0) {
                throw new ParserException("Could not find number of pages with lots!");
            }

            List<LotData> pkgListLotNumbers = new List<LotData>();
            for (int i = 0; i < numPagesWithLots; i++) {
                addLotNumbersfromPkgList(pagesText[i], pkgListLotNumbers, i);
            }

            return new ProcessingResult() {
                LotNumbers = pkgListLotNumbers,
                NumPagesWithLotNumbers = numPagesWithLots
            };
        }

        private static char[] SPACE_DELIMTERS = new char[] { ' ' };

        private void addLotNumbersFromAssembledDoc(string pageText, IDictionary<string, int> docLotNumbers) {
            int idx = pageText.IndexOf("Lot");
            if (idx >= 0) {
                var words = pageText.Split(SPACE_DELIMTERS);
                int numWords = words.Length;
                for(int i=1;i<numWords;i++) {
                    string word = words[i];
                    long num;
                    if (long.TryParse(word, out num)) {
                        if (words[i - 1].IndexOf("Lot", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                            words[i - 1].IndexOf("Number", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                            docLotNumbers[word] = 1;
                            break;
                        } else {
                            break;
                        }
                    }
                }

            }
        }

        public List<string> getMissingLotNumbers(string pdfPath, PdfReader reader, PageText []pagesText, ProcessingResult processingResults) {
            List<string> missingLots = new List<string>();

            int numPages = pagesText.Length;
            int numPagesWithLots = processingResults.NumPagesWithLotNumbers;

            IDictionary<string, int> docLotNumbers = new Dictionary<string, int>();
            for (int i = numPagesWithLots; i < numPages; i++) {
                addLotNumbersFromAssembledDoc(pagesText[i].Text, docLotNumbers);
            }

            int pageNum = -1;
            int pageNumToSkip = -1;
            foreach (var lotData in processingResults.LotNumbers) {
                if (lotData.PageNumber == pageNumToSkip) {
                    //skip current page
                    continue;
                }
                if (pageNum == -1) {
                    pageNum = lotData.PageNumber;
                } else if (pageNum != lotData.PageNumber) {
                    pageNum = lotData.PageNumber;
                }

                if (!docLotNumbers.ContainsKey(lotData.LotNumber)) {
                    //if (lotData.LotNumber == "1901101626") {
                    //    int a = 5;
                    //}
                    pageNumToSkip = pageNum;
                    if (!doSecondProcess(pdfPath, reader, pageNum + 1, pagesText[pageNum], missingLots, docLotNumbers)) {
                        if (!missingLots.Contains(lotData.LotNumber)) {
                            missingLots.Add(lotData.LotNumber);
                        }
                    }
                }
                
            }

            return missingLots;
        }

        private bool doSecondProcess(string pdfPath, PdfReader reader, int pageNo, PageText pageText, List<string> missingLots,
            IDictionary<string, int> docLotNumbers) {

            //first check if Line Num order is natural
            int numLines = pageText.LineNumbers.Length;
            int prevNumber;

            bool isNaturalOrder = true;
            if (int.TryParse(pageText.LineNumbers[0], out prevNumber)) {
                for (int i = 1; i < numLines; i++) {
                    int lineNo;
                    if (int.TryParse(pageText.LineNumbers[i], out lineNo)) {
                        if (lineNo != prevNumber + 1) {
                            bool startNextNumbersOrder = false;
                            if (i < numLines - 1) {
                                int nextLineNo;
                                if (int.TryParse(pageText.LineNumbers[i + 1], out nextLineNo)) {
                                    if (nextLineNo == lineNo + 1) {
                                        startNextNumbersOrder = true;
                                    }
                                }
                            }
                            if (!startNextNumbersOrder) {
                                isNaturalOrder = false;
                                break;
                            }
                        }
                        prevNumber = lineNo;
                    }
                }
            }

            if (isNaturalOrder) {
                //line nos are in natural order, but Lot nr not found
                return false;
            }

            var points = pdfImageProcessor.processPage(pdfPath, pageNo);
            List<string> groups = new List<string>();

            if (points != null) {
                var pageSize = reader.GetPageSize(1);
                foreach (var point in points) {
                    Rectangle size = new Rectangle(pageSize);
                    size.Top = pageSize.Height - pageSize.Height * point.X;
                    size.Bottom = pageSize.Height - pageSize.Height * point.Y;

                    RenderFilter[] groupFilter = { new RegionTextRenderFilter(size) };
                        //new Rectangle(0, pageSize.Height * point.Y, pageSize.Width, pageSize.Height * point.X)) };
                    //new Rectangle(0, pageSize.Height * point.X, pageSize.Width, pageSize.Height * point.Y)) };
                    string groupedLotsText = PdfTextExtractor.GetTextFromPage(reader, pageNo,
                        new FilteredTextRenderListener(new LocationTextExtractionStrategy(), groupFilter));

                    groups.Add(groupedLotsText);
                    //System.Diagnostics.Debug.Fail(groupedLotsText);
                }

                string lineNo = pageText.LineNumbers[0];
                int lineNoInt;
                int lineNoIndex = 0;
                int.TryParse(lineNo, out lineNoInt);

                string lotNumber = pageText.LotNumbers[0];
                string lotNumberToCheck = lotNumber;
                int lotNrIndex = 0;

                int prevLineNo = -1;

                List<string> lotNumbersToCheck = new List<string>();

                foreach(var txt in groups) {
                    string []lines = txt.Split(NEW_LINE_DELIMITER);
                    foreach(var line in lines) {
                        if (line.StartsWith(lineNo) && lotNrIndex == lineNoIndex) {
                            if (lineNoIndex < pageText.LineNumbers.Length - 1) {
                                lineNo = pageText.LineNumbers[++lineNoIndex];
                                int.TryParse(lineNo, out lineNoInt);
                                if (prevLineNo != -1 && prevLineNo != lineNoInt - 1) {
                                    //ignore lotNumber
                                    lotNumberToCheck = null;
                                } else {
                                    if (lotNumberToCheck != null) {
                                        lotNumbersToCheck.Add(lotNumberToCheck);
                                    }
                                }
                                prevLineNo = lineNoInt;
                            }
                        }
                        if (line.EndsWith(lotNumber)) {
                            if (lotNrIndex < pageText.LotNumbers.Length - 1) {
                                lotNumber = pageText.LotNumbers[++lotNrIndex];
                                lotNumberToCheck = lotNumber;
                                //if (lotNumber == "1901101626") {
                                //    int a = 5;
                                //}
                            }
                        }
                    }
                    //System.Diagnostics.Debug.Print(txt);
                    //System.Diagnostics.Debug.Print("--------------------------------");
                }

                foreach(var lotNr in lotNumbersToCheck) {
                    if (!docLotNumbers.ContainsKey(lotNr)) {
                        if (!missingLots.Contains(lotNr)) {
                            missingLots.Add(lotNr);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        private void addLotNumbersfromPkgList(PageText pageText, List<LotData> lotNumbers, int pageNo) {
            if (pageText.LineNumbers.Length == 0) {
                //there are no line numbers, so ignore the page
                return;
            }
            string[] lines = pageText.Text.Split(NEW_LINE_DELIMITER);

            int numLines = lines.Length;
            int i = 0;

            bool tableHeaderFound = false;
            bool firstLineIgnored = false;

            char[] wordsSeparators = new char[] { ' ', '\t' };

            int lotNumberIndex = 0;
            int lineNumIndex = 0;
            string lineNum = pageText.LineNumbers[0].ToString();

            while (i < numLines) {
                string line = lines[i];
                int numIgnoredWords = 0;
                foreach (string ignoredWord in IGNORED_WORDS) {
                    if (line.Contains(ignoredWord)) {
                        numIgnoredWords++;
                        if (numIgnoredWords >= 2) {
                            tableHeaderFound = true;
                            break;
                        }
                    }
                }

                if (numIgnoredWords == 0 && tableHeaderFound) {
                    //try to check if there is a line number in this row
                    if (line.StartsWith(lineNum)) {
                        char nextChar = line[lineNum.Length];
                        if (nextChar == ' ' || nextChar == '\t') {
                            lineNumIndex++;
                            if (lineNumIndex < pageText.LineNumbers.Length - 2) {
                                if (pageText.LineNumbers[lineNumIndex - 1] == pageText.LineNumbers[lineNumIndex]) {
                                    i++;
                                    continue;
                                }
                            }
                        }
                    }
                    //ignore white spaces
                    int index = line.Length - 1;
                    char ch = line[index];
                    while (index > 0 && (ch == ' ' || ch == '\t')) {
                        ch = line[--index];
                    }
                    if (index > 0) {
                        //lot number should be digits only, so check for that
                        bool lotNrValid = true;
                        while (index > 0) {
                            if (ch < '0' || ch > '9') {
                                if (ch != ' ') {
                                    lotNrValid = false;
                                }
                                break;
                            }
                            ch = line[--index];
                        }

                        if (lotNrValid) {
                            if (pageNo == 0 && !firstLineIgnored) {
                                firstLineIgnored = true;
                            } else {
                                //ignore lot number that have Quantity Ordered set
                                int numNumbers = 0;
                                string []words  = line.Split(wordsSeparators);
                                for(int j=0;j<words.Length;j++) {
                                    int number;
                                    if (int.TryParse(words[j], out number) && number < 100) {
                                        numNumbers++;
                                    } else {
                                        break;
                                    }
                                }

                                string lotNumber = line.Substring(index).Trim();
                                if (numNumbers == 1) {
                                    if (lotNumber.Length > 2) {
                                        //if (lotNumberIndex >= pageText.LotNumbers.Length) {
                                        //    int a = 7;
                                        //}
                                        if (lotNumber == pageText.LotNumbers[lotNumberIndex]) {
                                            lotNumbers.Add(new LotData() {
                                                PageNumber = pageNo,
                                                LotNumber = lotNumber
                                            });
                                            lotNumberIndex++;
                                        //} else {
                                            //lotNumbers.Add(new LotData() {
                                            //    LotNumber = lotNumber + " <=> " + pageText.LotNumbers[lotNumberIndex]
                                            //});
                                        }
                                    }
                                } else if (lotNumber == pageText.LotNumbers[lotNumberIndex]) {
                                    lotNumberIndex++;
                                }
                            }
                        }
                    }
                }

                i++;
            }
        }

        private int getNumPagesWithLots(string firstPageText) {

            Regex regex = new Regex(@"\d+\((\d+)\)");

            string []lines = firstPageText.Split(NEW_LINE_DELIMITER);
            int n = lines.Length;
            for (int i = 0;i<n;i++) {
                var match = regex.Match(lines[i].Replace(" ", ""));
                if (match.Success) {
                    string numPages = match.Groups[1].Value;
                    return int.Parse(numPages);
                }
            }

            return 0;
        }

    }
}
