using System.Collections.Generic;

namespace PDFChecker {
    class ProcessingResult {

        public int NumPagesWithLotNumbers { set; get; }

        public List<LotData> LotNumbers { set; get; }

    }
}
