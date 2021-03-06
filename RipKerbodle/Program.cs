using System;
using System.IO;

namespace RipKerboodle
{
    class Program
    {
        static KerboodleDownloader kdown = new KerboodleDownloader();
        static PDFProcessor pdfman = new PDFProcessor();
        static URLDownloader udown = new URLDownloader();
        static string fld = null;
        static string ext = null;

        static void Main(string[] args)
        {
            Console.WriteLine("RIP Kerboodle!");
            var mis = getMenuItem();
            if (mis == 0) Environment.Exit(0);
            if (mis == 1 || (mis - 2) == 1 || (mis - 4) == 1 || (mis - 6) == 1)
            {
                getKerboodleLink();
                ext = kdown.extension;
                kdown.percentageUpdate += percentager;
                fld = getKerboodleDownloadPath();
                if (!Directory.Exists(fld)) Directory.CreateDirectory(fld);
                kdown.downloadTo(fld);
                kdown.percentageUpdate -= percentager;
            }
            if (mis == 4 || (mis - 1) == 4 || (mis - 2) == 4 || (mis - 3) == 4)
            {
                udown.load(getURLStorePath());
                ext = udown.extension;
                udown.percentageUpdate += percentager;
                fld = getKerboodleDownloadPath();
                if (!Directory.Exists(fld)) Directory.CreateDirectory(fld);
                udown.downloadTo(fld);
                udown.percentageUpdate -= percentager;
            }
            if (mis == 2 || (mis - 1) == 2 || (mis - 4) == 2 || (mis - 5) == 2)
            {
                if (object.ReferenceEquals(fld, null)) fld = getKerboodleDownloadPath(true);
                if (object.ReferenceEquals(ext, null)) ext = getImageExtension();
                pdfman.percentageUpdate += percentager;
                var pth = getPDFSavePath();
                insertPDFImages();
                pdfman.save(pth, 0.0F);
                pdfman.percentageUpdate -= percentager;
            }
            Console.WriteLine("Press Any Key To Continue...");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        public static int getMenuItem()
        {
            Console.WriteLine("MENU:");
            Console.WriteLine("Add the numbers of tasks to do those tasks.");
            Console.WriteLine("1: Download from Kerboodle.");
            Console.WriteLine("2: Create PDF From download.");
            Console.WriteLine("4: Download from URL Store.");
            Console.WriteLine("INVALID: Exit.");
            Console.Write("Option: ");
            var ck = Console.ReadKey(true);
            Console.WriteLine(ck.KeyChar);
            if (ck.Key == ConsoleKey.D1 || ck.Key == ConsoleKey.NumPad1) return 1;
            if (ck.Key == ConsoleKey.D2 || ck.Key == ConsoleKey.NumPad2) return 2;
            if (ck.Key == ConsoleKey.D3 || ck.Key == ConsoleKey.NumPad3) return 3;
            if (ck.Key == ConsoleKey.D4 || ck.Key == ConsoleKey.NumPad4) return 4;
            if (ck.Key == ConsoleKey.D5 || ck.Key == ConsoleKey.NumPad5) return 5;
            if (ck.Key == ConsoleKey.D6 || ck.Key == ConsoleKey.NumPad6) return 6;
            if (ck.Key == ConsoleKey.D7 || ck.Key == ConsoleKey.NumPad7) return 7;
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
            Console.WriteLine("Enter the save path of the PDF:");
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
                    using (var imageStream = new FileStream(fld + "\\" + i.ToString() + ext, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
