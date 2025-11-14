using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RipKerboodle
{
    class URLDownloader
    {
        private List<string> urls = new List<string>();
        private string _ext = ".jpg";
        private int completedCount = 0;
        private int totalCount = 0;
        private object slock = new object();
        private int maxConcurrentDownloads = 5;

        public event EventHandler<int> percentageUpdate;

        public int MaxConcurrentDownloads
        {
            get { return maxConcurrentDownloads; }
            set { maxConcurrentDownloads = value > 0 ? value : 1; }
        }

        public void load(string pth)
        {
            lock (slock)
            {
                urls.Clear();
                var allLines = File.ReadAllLines(pth);
                foreach (var line in allLines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var trimmedLine = line.Trim();
                        // Check if the URL ends with a valid file extension (pdf, jpg, jpeg, png)
                        if (trimmedLine.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                            trimmedLine.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            trimmedLine.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                            trimmedLine.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            urls.Add(trimmedLine);
                        }
                    }
                }
                if (urls.Count > 0) _ext = urls[0].Substring(urls[0].LastIndexOf("."));
            }
        }

        public void downloadTo(string folderPath, int startPage = 1, int endPage = int.MaxValue)
        {
            if (startPage > endPage) return;

            if (endPage > urls.Count) endPage = urls.Count;

            completedCount = 0;
            totalCount = endPage - startPage + 1;

            var ev = percentageUpdate;
            if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, 0);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = maxConcurrentDownloads * 2;

            // Create a semaphore to limit concurrent downloads
            using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrentDownloads))
            {
                List<Task> tasks = new List<Task>();

                for (int i = startPage; i <= endPage; i++)
                {
                    int pageNum = i;
                    string url = urls[i - 1];

                    Task task = Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            using (WebClient webClient = new WebClient())
                            {
                                string filePath = folderPath + "\\" + pageNum.ToString() + _ext;

                                // Download to memory first
                                byte[] fileBytes = await webClient.DownloadDataTaskAsync(new Uri(url));

                                // Fix malformed PDF header (%ADF -> %PDF) if this is a PDF
                                if (_ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (fileBytes.Length > 4 && fileBytes[0] == '%' && fileBytes[1] == 'A' && fileBytes[2] == 'D' && fileBytes[3] == 'F')
                                    {
                                        fileBytes[1] = (byte)'P';
                                        fileBytes[2] = (byte)'D';
                                        fileBytes[3] = (byte)'F';
                                    }
                                }

                                // Write the corrected file
                                File.WriteAllBytes(filePath, fileBytes);
                            }
                        }
                        catch
                        {
                            // Silently fail for individual downloads
                        }
                        finally
                        {
                            semaphore.Release();

                            lock (slock)
                            {
                                completedCount++;
                                int percent = (int)((completedCount * 100) / totalCount);
                                ev = percentageUpdate;
                                if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, percent);
                            }
                        }
                    });

                    tasks.Add(task);
                }

                // Wait for all downloads to complete
                Task.WaitAll(tasks.ToArray());
            }
        }

        public int percentage
        {
            get
            {
                lock (slock)
                {
                    if (totalCount == 0) return 0;
                    return (int)((completedCount * 100) / totalCount);
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
    }
}
