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
        private static readonly string continuePrompt = "\n--- Appuyez sur entrée pour continuer";

        //[System.AttributeUsage(System.AttributeTargets.Method)]
        [STAThread]
        static void Main(string[] args)
        {
            string selectedFolder = PromptSelectFolder();
            Console.WriteLine(String.Format("Le programme va fonctionner dans {0} \nPour traiter d'autres archives, il faudra relancer le programme et choisir un autre dossier {1}", selectedFolder, continuePrompt));
            Console.ReadLine();

        }
        private static string PromptSelectFolder()
        {
            bool isFolderSelected = false;
            string folder = null;
            do
            {
                Console.WriteLine("\nSélection du dossier où se trouvent les archives .cbz ou .zip" + continuePrompt);
                Console.ReadLine();
                folder = PickWorkingFolder();
                Console.WriteLine(String.Format("Le dossier sélectionné est : {0} \n→ Est-ce que ça convient ? Tapez 'n' pour choisir un nouveau dossier, sinon, appuyez sur entrée", folder));
                string userInput = Console.ReadLine();
                isFolderSelected = userInput == "n" || userInput == "N" ? false : true;
            } while (!isFolderSelected);

            return folder;
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
            // Merci https://stackoverflow.com/a/50263779
        }
    }
}
