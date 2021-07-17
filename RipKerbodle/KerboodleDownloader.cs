using System;
using System.IO;
using System.Net;

namespace RipKerboodle
{
    class KerboodleDownloader
    {
        private string _klnk = "https://assets-runtime-production-oxed-oup.avallain.net/ebooks/33fc849f13124f366e9f1e2a/images/#NUMBER#.jpg";
        private string _ext = ".jpg";
        private decimal per = 0;
        private decimal peradd = 0;
        private object slock = new object();

        public event EventHandler<int> percentageUpdate;

        public string kerboodleLink
        {
            get
            {
                lock (slock) return _klnk;                
            }
            set
            {
                lock (slock)
                {
                    _klnk = value;
                    if (!_klnk.Contains("#NUMBER#"))
                    {
                        if (_klnk.EndsWith("/")) _klnk += "#NUMBER#.jpg"; else _klnk += "/#NUMBER#.jpg";
                    }
                    _ext = _klnk.Substring(_klnk.LastIndexOf("."));
                }
            }
        }

        public string extension
        {
            get
            {
                return _ext;
            }
        }

        public void downloadTo(string folderPath, int startPage = 1, int endPage = int.MaxValue)
        {
            if (startPage > endPage) return;
            lock (slock)
            {
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
                            var lurl = _klnk.Replace("#NUMBER#", i.ToString());
                            var request = (HttpWebRequest)WebRequest.Create(lurl);
                            webClient.DownloadFile(lurl, folderPath + "\\" + i.ToString() + _ext);
                            per = per + peradd;
                            ev = percentageUpdate;
                            if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, (int)per);
                        }
                        catch {
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
    }
}
