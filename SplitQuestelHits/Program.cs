using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using SplitQuestelHits.Services;

namespace SplitQuestelHits
{
    class Program
    {
        static void Main(string[] args)
        {
            SplitService SplS = new SplitService();

            string path = "C:\\Mafell\\";
            string[] fileEntries;


            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (Directory.Exists(path))
            {
                // This path is a directory
                fileEntries = ProcessDirectory(path);


                foreach (string fileName in fileEntries)
                {
                    string Akte = "";
                    if (!String.IsNullOrEmpty(fileName))
                    {
                        Akte = fileName.Substring(fileName.IndexOf('-') + 1, fileName.IndexOf(')') - fileName.IndexOf('-'));
                    }

                    if (!String.IsNullOrEmpty(Akte) && Directory.Exists(path + Akte))
                    {
                        DeleteDirectory(path + Akte);
                        Directory.CreateDirectory(path + Akte);

                    }
                    if (!String.IsNullOrEmpty(Akte) && !Directory.Exists(path + Akte))
                    {
                        Directory.CreateDirectory(path + Akte);
                    }


                    //string fileName = "C:\\Questel\\Transformed\\Bookmarks\\Überwachungsmitteilung-109(69)-KW25-2020.pdf";

                    Dictionary<int, int> dict = SplS.GetBookmarkList(fileName);

                    SplS.SplitPDFByBookMark(fileName, dict, Akte);
                }
            }

        }


        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static string[] ProcessDirectory(string targetDirectory)
        {
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                return fileEntries;
        }

    }
    
}
