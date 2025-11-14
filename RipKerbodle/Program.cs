using System;
using System.IO;

namespace RipKerboodle
{
    class Program
    {
        static KerboodleDownloader kdown = new KerboodleDownloader();
        static PDFProcessor pdfman = new PDFProcessor();
        static PDFMerger pdfmerge = new PDFMerger();
        static URLDownloader udown = new URLDownloader();
        static string kerboodleTempPagePath = null;
        static string ext = null;

        static void Main(string[] args)
        {
            Console.WriteLine("RIP Kerboodle!");

            // Command line mode: args = [option, folder_path, output_name]
            if (args.Length >= 3)
            {
                int mis = int.Parse(args[0]);
                if (mis == 8)
                {
                    kerboodleTempPagePath = args[1];
                    pdfmerge.percentageUpdate += percentager;
                    var pth = args[2];
                    pdfmerge.mergePDFs(kerboodleTempPagePath, pth + ".pdf", ".pdf");
                    pdfmerge.percentageUpdate -= percentager;
                    Console.WriteLine("Merge completed. Output: " + pth + ".pdf");
                    Environment.Exit(0);
                }
            }

            var mis2 = getMenuItem();
            if (mis2 == 0) Environment.Exit(0);
            if (mis2 == 1)
            {
                getKerboodleLink();
                ext = kdown.extension;
                kdown.percentageUpdate += percentager;
                kerboodleTempPagePath = getKerboodleDownloadPath();
                if (!Directory.Exists(kerboodleTempPagePath)) Directory.CreateDirectory(kerboodleTempPagePath);
                kdown.downloadTo(kerboodleTempPagePath);
                kdown.percentageUpdate -= percentager;
            }
            if (mis2 == 2)
            {
                if (object.ReferenceEquals(kerboodleTempPagePath, null)) kerboodleTempPagePath = getKerboodleDownloadPath(true);
                if (object.ReferenceEquals(ext, null)) ext = getImageExtension();
                pdfman.percentageUpdate += percentager;
                var pth = getPDFSavePath();
                insertPDFImages();
                pdfman.save(pth + ".pdf", 0.0F);
                pdfman.percentageUpdate -= percentager;
            }
            if (mis2 == 4)
            {
                udown.load(getURLStorePath());
                ext = udown.extension;
                udown.percentageUpdate += percentager;
                kerboodleTempPagePath = getKerboodleDownloadPath();
                if (!Directory.Exists(kerboodleTempPagePath)) Directory.CreateDirectory(kerboodleTempPagePath);
                udown.downloadTo(kerboodleTempPagePath);
                udown.percentageUpdate -= percentager;
            }
            if (mis2 == 8)
            {
                if (object.ReferenceEquals(kerboodleTempPagePath, null)) kerboodleTempPagePath = getKerboodleDownloadPath(true);
                pdfmerge.percentageUpdate += percentager;
                var pth = getPDFSavePath();
                pdfmerge.mergePDFs(kerboodleTempPagePath, pth + ".pdf", ".pdf");
                pdfmerge.percentageUpdate -= percentager;
            }
            Console.WriteLine("Press Any Key To Continue...");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        public static int getMenuItem()
        {
            Console.WriteLine("MENU:");
            Console.WriteLine("1: Download from Kerboodle.");
            Console.WriteLine("2: Create PDF From Downloaded Images.");
            Console.WriteLine("4: Download from URL Store.");
            Console.WriteLine("8: Create PDF From Downloaded PDFs.");
            Console.WriteLine("INVALID: Exit.");
            Console.Write("Option: ");
            var ck = Console.ReadKey(true);
            Console.WriteLine(ck.KeyChar);
            if (ck.Key == ConsoleKey.D1 || ck.Key == ConsoleKey.NumPad1) return 1;
            if (ck.Key == ConsoleKey.D2 || ck.Key == ConsoleKey.NumPad2) return 2;
            if (ck.Key == ConsoleKey.D4 || ck.Key == ConsoleKey.NumPad4) return 4;
            if (ck.Key == ConsoleKey.D8 || ck.Key == ConsoleKey.NumPad8) return 8;
            return 0;
        }

        public static void getKerboodleLink()
        {
            Console.WriteLine("Example of image link: " + kdown.kerboodleLink);
            Console.WriteLine("Where '#NUMBER#' will be replaced by the page number.");
            Console.WriteLine("Enter the image link:");
            kdown.kerboodleLink = Console.ReadLine();
            Console.WriteLine("Image Link Set To: " + kdown.kerboodleLink);
        }

        public static string getKerboodleDownloadPath(bool shouldExist = false)
        {
            Console.WriteLine("Enter the download folder path of the kerboodle images:");
            var toret = Console.ReadLine();
            return (shouldExist && (!Directory.Exists(toret))) ? "" : toret;
        }

        public static string getPDFSavePath()
        {
            Console.WriteLine("Enter the name for the pdf to be saved as:");
            return Console.ReadLine();
        }

        public static string getImageExtension()
        {
            Console.WriteLine("Enter the file extension of the saved images (Including the leading .):");
            return Console.ReadLine();
        }

        public static string getURLStorePath()
        {
            Console.WriteLine("Enter the load path of the URL Store:");
            return Console.ReadLine();
        }

        public static void insertPDFImages(int startPage = 1, int endPage = int.MaxValue)
        {
            if (startPage > endPage) return;
            Console.WriteLine("Inserting PDF Images...");
            for (int i = startPage; i <= endPage; i++)
            {
                try
                {
                    using (var imageStream = new FileStream(kerboodleTempPagePath + "\\" + i.ToString() + ext, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        pdfman.add(iTextSharp.text.Image.GetInstance(imageStream));
                    }
                    Console.WriteLine("Inserting Image: " + i.ToString() + "/" + (endPage - startPage + 1).ToString());
                }
                catch { break; }
            }
        }

        static void percentager(object sender, int per)
        {
            Console.WriteLine(((object.ReferenceEquals(sender, null)) ? "%UNKNOWN%" : sender.GetType().Name) + " : " + per.ToString() + "%");
        }
    }
}
