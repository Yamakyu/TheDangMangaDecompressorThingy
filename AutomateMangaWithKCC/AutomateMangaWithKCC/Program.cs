using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutomateMangaWithKCC
{
    class Program
    {
        //[System.AttributeUsage(System.AttributeTargets.Method)]
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Ca va jamais marcher du premier coup mdr");
            string testFolder = PickWorkingFolder();
            Console.WriteLine(testFolder);
            Console.ReadLine();

        }
        private static string PickWorkingFolder()
        {
            OpenFileDialog folderBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            // Always default to Folder Selection.
            folderBrowser.FileName = "Folder Selection.";
            string folderPath = "didn't work";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                folderPath = Path.GetDirectoryName(folderBrowser.FileName);
                // ...
            }
            return folderPath;
            // https://stackoverflow.com/a/50263779
        }
    }
}
