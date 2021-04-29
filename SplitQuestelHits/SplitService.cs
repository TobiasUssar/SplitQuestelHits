using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using System.Drawing;
using iTextSharp.text.pdf.parser;

namespace SplitQuestelHits.Services
{
    public class SplitService
    {
        public string GetFileName()
        {
            string result = System.IO.Path.GetFileName(@"C:\Mafell\");

                return result;
        }

        public Dictionary<int, int> GetBookmarkList(string fileName)
        {
            Dictionary<int, int> bookmarks = new Dictionary<int, int>();
            string sInFile = fileName;
            iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(sInFile);
            try
            {

                StringWriter firstPageOutput = new StringWriter();

                firstPageOutput.WriteLine(PdfTextExtractor.GetTextFromPage(pdfReader, 1, new SimpleTextExtractionStrategy()));

                string stringout = firstPageOutput.ToString();

                int hitsPos = stringout.IndexOf("\n1/");
                string substr = stringout.Substring(hitsPos + 1, 5);
                substr = substr.TrimEnd(' ');
                short numberOfHits;
                Int16.TryParse(substr.Substring(2, substr.Length-2), out numberOfHits);


                for (int hit = 1; hit <= numberOfHits; hit++)
                {
                    string searchstring = hit.ToString() + "/" + numberOfHits.ToString() + " ";
                    for(int page = hit; page <= pdfReader.NumberOfPages ; page++ )
                    { 
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                        currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(
                        Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                        if (currentText.Contains(searchstring))
                        {
                            bookmarks.Add(hit, page);
                            break;
                        }
                    }
                }
            }

            catch (DocumentException dex)
            {
                throw dex;
            }

            catch (IOException ioex)
            {
                throw ioex;
            }

            finally
            {

            }
            return bookmarks;
        }

    


        public bool SplitPDFByBookMark(string fileName, Dictionary <int, int> bookmarks, string subdirectory)
        {
            string sInFile = fileName;
            iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(sInFile);
            try
            {
                int totalhitsCount = bookmarks.Count;
                int lastPageValue = 0;
                int key = 0;

                foreach (KeyValuePair<int, int> kvp in bookmarks)
                {
                    key = kvp.Key;

                    if (key == 1)
                    {
                        lastPageValue = kvp.Value;

                        if (key == totalhitsCount)
                        {
                            SplitByBookmark(pdfReader, lastPageValue, lastPageValue, kvp.Key.ToString() + ".pdf", fileName, subdirectory);
                        }
                    }
                     

                    if (key > 1 && key < totalhitsCount)
                    {

                        SplitByBookmark(pdfReader, lastPageValue, kvp.Value - 1, (kvp.Key - 1).ToString() + ".pdf", fileName, subdirectory);
                        lastPageValue = kvp.Value;
                    }

                    if (key == totalhitsCount)
                    {
                        //vorletzte PDF
                        SplitByBookmark(pdfReader, lastPageValue, kvp.Value - 1, (kvp.Key - 1).ToString() + ".pdf", fileName, subdirectory);
                        //letzte PDF
                        SplitByBookmark(pdfReader, kvp.Value,  pdfReader.NumberOfPages , kvp.Key.ToString() + ".pdf", fileName, subdirectory);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
                return false;
            }
            return true;
        }

        private void SplitByBookmark(iTextSharp.text.pdf.PdfReader reader, int pageFrom, int PageTo, string outPutName, string inPutFileName, string subdirectory)
        {
            Document document = new Document();
            FileStream fs = new System.IO.FileStream(System.IO.Path.GetDirectoryName(inPutFileName) + '\\' + subdirectory + '\\' + outPutName, System.IO.FileMode.Create);

            try
            {

                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                //holds pdf data
                PdfImportedPage page;
                if (pageFrom == PageTo && pageFrom == 1)
                {
                    document.NewPage();
                    page = writer.GetImportedPage(reader, pageFrom);
                    cb.AddTemplate(page, 0, 0);
                    pageFrom++;
                    fs.Flush();
                    document.Close();
                    fs.Close();

                }
                else
                {
                    while (pageFrom <= PageTo)
                    {
                        document.NewPage();
                        page = writer.GetImportedPage(reader, pageFrom);
                        cb.AddTemplate(page, 0, 0);
                        pageFrom++;
                        fs.Flush();
                        //document.Close();
                        //fs.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (document.IsOpen())
                    document.Close();
                if (fs != null)
                    fs.Close();
            }

        }
    }
}
