using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace RipKerboodle
{
    class URLDownloader
    {
        private List<string> urls = new List<string>();
        private string _ext = ".jpg";
        private decimal per = 0;
        private decimal peradd = 0;
        private object slock = new object();

        public event EventHandler<int> percentageUpdate;

        public void load(string pth)
        {
            lock (slock)
            {
                urls.Clear();
                urls.AddRange(File.ReadAllLines(pth));
                if (urls.Count > 0) _ext = urls[0].Substring(urls[0].LastIndexOf("."));
            }
        }

        public void downloadTo(string folderPath, int startPage = 1, int endPage = int.MaxValue)
        {
            if (startPage > endPage) return;
            lock (slock)
            {
                if (endPage > urls.Count) endPage = urls.Count;
                per = 0;
                var ev = percentageUpdate;
                if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, (int)per);
                peradd = (decimal)100 / (endPage - startPage + 1);
                using (WebClient webClient = new WebClient())
                {
                    for (int i = startPage; i <= endPage; i++)
                    {
                        try
                        {
                            var request = (HttpWebRequest)WebRequest.Create(urls[i - 1]);
                            webClient.DownloadFile(urls[i - 1], folderPath + "\\" + i.ToString() + _ext);
                            per = per + peradd;
                            ev = percentageUpdate;
                            if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, (int)per);
                        }
                        catch
                        {
                            per = per + peradd;
                            ev = percentageUpdate;
                            if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, (int)per);
                            break;
                        }
                    }
                }
            }
        }

        public int percentage
        {
            get
            {
                return (int)per;
            }
        }

        public string extension
        {
            get
            {
                return _ext;
            }
        }
    }
}
