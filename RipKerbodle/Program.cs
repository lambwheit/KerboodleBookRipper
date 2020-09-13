using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf; 

namespace RipKerboodle
{
    class Program
    {
        static void Main(string[] args)
        {
            int init = 1;
            Console.WriteLine("Example of image link: https://assets-runtime-production-oxed-oup.avallain.net/ebooks/33fc849f13124f366e9f1e2a/images/");
            Console.Write("Enter in image link: ");
            string link = Console.ReadLine();
            Console.WriteLine("Instructions for page name: ");
            Console.WriteLine("Get the file name");
            Console.WriteLine("https://assets-runtime-production-oxed-oup.avallain.net/ebooks/dd094244a18d3730296d98c0/images/page-5.jpg");
            Console.WriteLine("In the above it's page-5.jpg");
            Console.WriteLine("Once u have the page name relace the page number with the word number");
            Console.WriteLine("page-number.jpg");
            Console.WriteLine("That will be your page name");
            Console.Write("Enter in page name: ");
            string pagename = Console.ReadLine();
            Console.Write("Folder name: ");
            string foldername = Console.ReadLine();
            Console.Write("Pdf Name: ");
            string pdfname = Console.ReadLine();
            System.IO.Directory.CreateDirectory(foldername);
            Console.Clear();
	        /*Console.Title = "Downloading pages into: "+foldername ;
            while (true)
            {
                try
                {
                    string pagename2 = pagename.Replace("number", init.ToString());
                    string url = link + pagename2;
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.DownloadFile(url, foldername+"\\"+ init + ".jpg");
                    }
                    Console.WriteLine("Ripped Page Number: "+init);
                }
                catch 
                {
                    Console.WriteLine("Rip Finished, "+(init-1).ToString()+" Pages Ripped");
                    break;
                }
                init++;
            }
	        Console.Clear();*/
	        Console.Title = "Adding pages into:" + pdfname+".pdf";
            Document document = new Document();
            using (var stream = new FileStream(pdfname + ".pdf", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                PdfWriter.GetInstance(document, stream);
                document.Open();
                for (int i = 1;i< init; i++)
                {
                    Console.WriteLine("Adding page "+i+" into pdf");
                    using (var imageStream = new FileStream(foldername+"\\"+i+".jpg", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var image = Image.GetInstance(imageStream);
                        float maxWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                        float maxHeight = document.PageSize.Height - document.TopMargin - document.BottomMargin;
                        if (image.Height > maxHeight || image.Width > maxWidth) image.ScaleToFit(maxWidth, maxHeight);
                        document.Add(image);
                    }
                }
                document.Close();
            }
            Console.Clear();
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
