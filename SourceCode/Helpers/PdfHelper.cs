using System.Collections.Generic;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;

namespace SDK.Helpers
{
    /// <summary>
    /// PDF utility helper for SAP Business One Add-ons
    /// </summary>
    public static class PdfHelper
    {
        /// <summary>
        /// Merges multiple PDF files into a single PDF
        /// </summary>
        /// <param name="pdfFiles">List of PDF file paths</param>
        /// <param name="outputPath">Output PDF path</param>
        /// <returns>Merged PDF file path</returns>
        public static string MergePdfFiles(
            List<string> pdfFiles,
            string outputPath)
        {
            if (pdfFiles == null || pdfFiles.Count == 0)
                return string.Empty;

            using (PdfDocument destinationPdf =
                   new PdfDocument(new PdfWriter(outputPath)))
            {
                PdfMerger merger = new PdfMerger(destinationPdf);

                foreach (string file in pdfFiles)
                {
                    if (!File.Exists(file))
                        continue;

                    using (PdfDocument sourcePdf =
                           new PdfDocument(new PdfReader(file)))
                    {
                        merger.Merge(
                            sourcePdf,
                            1,
                            sourcePdf.GetNumberOfPages());
                    }
                }
            }

            return outputPath;
        }
    }
}

