using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace RipKerboodle
{
    class PDFMerger
    {
        private decimal per = 0;
        private decimal peradd = 0;
        private object slock = new object();

        public event EventHandler<int> percentageUpdate;

        public int percentage
        {
            get
            {
                return (int)per;
            }
        }

        public bool mergePDFs(string folderPath, string outputPath, string fileExtension, int startPage = 1, int endPage = int.MaxValue)
        {
            lock (slock)
            {
                per = 0;
                var ev = percentageUpdate;
                if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, (int)per);

                // Count how many files exist and build input list
                List<string> inputFiles = new List<string>();
                for (int i = startPage; i <= endPage; i++)
                {
                    string filePath = Path.Combine(folderPath, i.ToString() + fileExtension);
                    if (!File.Exists(filePath)) break;
                    inputFiles.Add(filePath);
                }

                if (inputFiles.Count == 0)
                {
                    Console.WriteLine("No input files found.");
                    return false;
                }

                peradd = (decimal)100 / inputFiles.Count;

                Console.WriteLine($"Merging {inputFiles.Count} PDF files using iTextSharp...");

                // Use iTextSharp with lenient mode for malformed PDFs
                using (Document document = new Document())
                {
                    using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                    {
                        using (PdfCopy copy = new PdfCopy(document, fs))
                        {
                            document.Open();

                            for (int i = 0; i < inputFiles.Count; i++)
                            {
                                try
                                {
                                    // Read the file and check/fix header if needed
                                    byte[] fileBytes = File.ReadAllBytes(inputFiles[i]);

                                    // Fix malformed PDF header (%ADF -> %PDF) if present
                                    if (fileBytes.Length > 4 && fileBytes[0] == '%' && fileBytes[1] == 'A' && fileBytes[2] == 'D' && fileBytes[3] == 'F')
                                    {
                                        fileBytes[1] = (byte)'P';
                                        fileBytes[2] = (byte)'D';
                                        fileBytes[3] = (byte)'F';
                                    }

                                    // Read PDF with lenient parsing
                                    PdfReader reader = new PdfReader(fileBytes);
                                    reader.ConsolidateNamedDestinations();

                                    // Import all pages
                                    for (int pageNum = 1; pageNum <= reader.NumberOfPages; pageNum++)
                                    {
                                        try
                                        {
                                            PdfImportedPage page = copy.GetImportedPage(reader, pageNum);
                                            copy.AddPage(page);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error importing page {pageNum} from file {i + 1}: {ex.Message}");
                                        }
                                    }

                                    reader.Close();

                                    // Update progress
                                    per = (decimal)((i + 1) * 100) / inputFiles.Count;
                                    ev = percentageUpdate;
                                    if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, (int)per);

                                    if ((i + 1) % 10 == 0)
                                    {
                                        Console.WriteLine($"Processed {i + 1}/{inputFiles.Count} files...");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error processing file {i + 1} ({inputFiles[i]}): {ex.Message}");
                                }
                            }

                            document.Close();
                        }
                    }
                }

                Console.WriteLine("PDF merge completed successfully.");
                return true;
            }
        }
    }
}
