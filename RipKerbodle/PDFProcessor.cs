using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf; 

namespace RipKerboodle
{
    class PDFProcessor
    {
        private List<Image> _lst = new List<Image>();
        private Document doc = new Document(PageSize.A4);
        private float mw = 0.0F;
        private float mh = 0.0F;
        private decimal per = 0;
        private decimal peradd = 0;
        private object slock = new object();

        public event EventHandler<int> percentageUpdate;

        public PDFProcessor()
        {
            lock (slock)
            {
                mw = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
                mh = doc.PageSize.Height - doc.TopMargin - doc.BottomMargin;
            }
        }

        public void add(Image imgIn, bool rescale = true)
        {
            if (rescale && (imgIn.Height > mh || imgIn.Width > mw)) imgIn.ScaleToFit(mw, mh);
            lock (slock) _lst.Add(imgIn);
        }

        public int percentage
        {
            get
            {
                return (int)per;
            }
        }

        public bool save(string filepath, float margin) {
            try
            {
                lock (slock)
                {
                    per = 0;
                    var ev = percentageUpdate;
                    if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, (int)per);
                    peradd = (decimal)100 / _lst.Count;
                    using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        PdfWriter.GetInstance(doc, stream);
                        doc.Open();
                        doc.SetMargins(margin, margin, margin, margin);
                        foreach (var c in _lst)
                        {
                            doc.Add(c);
                            per = per + peradd;
                            ev = percentageUpdate;
                            if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, (int)per);
                        }
                        doc.Close();
                    }
                    return true;
                }
            }
            catch
            {
                var ev = percentageUpdate;
                if (!object.ReferenceEquals(ev, null)) ev.Invoke(this, (int)per);
                return false;
            }
        }

        public void clear()
        {
            _lst.Clear();
        }
    }
}
