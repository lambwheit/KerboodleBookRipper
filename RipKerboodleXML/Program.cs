using System;
using System.Collections.Generic;
using System.IO;

namespace RipKerboodleXML
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Kerboodle XML Ripper!");
            Console.WriteLine("Obtain the XML from the request for data.js making sure the XML saved into the file to be loaded is valid.");
            Console.WriteLine("Enter XML File Name:");
            var fnom = Console.ReadLine();
            var ret = urlextractor(File.ReadAllText(fnom));
            Console.WriteLine("Enter Store File Name:");
            var snom = Console.ReadLine();
            Console.WriteLine("Storing URLs...");
            File.WriteAllLines(snom, ret);
            Console.WriteLine("Press Any Key To Continue...");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        static string[] urlextractor(string xmlIn)
        {
            Console.WriteLine("Extracting URLs...");
            var toret = new List<string>();
            var inurl = false;
            var readin = "";
            for (int i = 0; i < xmlIn.Length; i++)
            {
                var cchar = xmlIn[i];
                if (inurl)
                {
                    if (cchar != '"') { readin += cchar; } else { inurl = false; toret.Add("https:" + readin); Console.WriteLine("Extracted URL " + toret.Count + "."); readin = ""; }
                }
                else
                {
                    if (cchar == '"' && readin.EndsWith("url="))
                    {
                        inurl = true;
                        readin = "";
                    }
                    else
                    {
                        readin += cchar;
                    }
                }
            }
            return toret.ToArray();
        }
    }
}
