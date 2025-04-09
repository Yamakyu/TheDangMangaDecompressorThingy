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
        //E:\Hakuneko_Biblio\_dossierDeTravail\for_script_testing
        private static readonly string continuePrompt = "\n--------- Appuyez sur entrée pour continuer";

        private static string pagePrefix = "yk_cXxX_p00";
        private static string coverArt;

        private static bool hasCoverArt = false;

        //[System.AttributeUsage(System.AttributeTargets.Method)]
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string selectedFolder = PromptSelectFolder();
                Console.WriteLine($"Le programme va fonctionner dans {selectedFolder} \nPour traiter d'autres archives, il faudra relancer le programme et choisir un autre dossier {1}" + continuePrompt);
                Console.ReadLine();

                Console.WriteLine("S'il est nécessaire d'inclure une première de couverture au KEPUB, mais qui n'est inclue dans aucune des archives, tapez 'y' pour pouvoir la sélectionner");
                if (Console.ReadLine().ToLower() == "y")
                {
                    coverArt = PromptFile();
                    hasCoverArt = true;
                }

                Console.WriteLine($"Alrighty : {coverArt}");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Console.ReadLine();
            }


        }
        private static string PromptSelectFolder()
        {
            bool isFolderSelected;
            string folder;
            do
            {
                Console.WriteLine("\nVous allez sélectionner le dossier où se trouvent les archives .cbz ou .zip" + continuePrompt);
                Console.ReadLine();
                folder = PromptUserToSelect();
                Console.WriteLine($"Le dossier sélectionné est : {folder} \n→ Si ça ne convient pas tapez 'n' pour choisir un nouveau dossier, sinon appuyez sur entrée");
                isFolderSelected = isUserInputValid();
            } while (!isFolderSelected);

            return folder;
        }
        private static string PromptFile()
        {
            bool isFileSelected;
            string filePath;
            do
            {
                Console.WriteLine("\nVous allez sélectionner le fichier." + continuePrompt);
                Console.ReadLine();
                filePath = PromptUserToSelect(false);
                Console.WriteLine($"Le fichier sélectionné est : {filePath} \n→ Si ça ne convient pas tapez 'n' pour choisir un nouveau fichier, sinon appuyez sur entrée");
                isFileSelected = isUserInputValid();
            } while (!isFileSelected);

            return filePath;
        }
        private static bool isUserInputValid(string toCheck = "n")
        {
            //Méthode pour véridier que l'utilisateur est content de ce qu'il a entré.
            //Si l'utilisateur n'est pas satisfait de son entrée, il tape "n" ou "N" (par défaut, mais on peut changer le check)
            string userInput = Console.ReadLine();
            bool isUserConfirmsInput = userInput.ToLower() == toCheck ? false : true;
            return isUserConfirmsInput;
        }
        private static string PromptUserToSelect(bool isFolder = true)
        {
            //Sous entendu : si ce n'est pas un dossier, c'est un fichier

            OpenFileDialog openBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            //openBrowser.ValidateNames = false;
            //openBrowser.CheckFileExists = false;
            //openBrowser.CheckPathExists = true;
            
            openBrowser.ValidateNames = !isFolder;
            openBrowser.CheckFileExists = !isFolder;
            openBrowser.CheckPathExists = isFolder;
            // Always default to Folder Selection.
            openBrowser.FileName = "Selection.";
            string folderPath = "didn't work";
            if (openBrowser.ShowDialog() == DialogResult.OK)
            {
                folderPath = isFolder ? Path.GetDirectoryName(openBrowser.FileName) : Path.GetFullPath(openBrowser.FileName);
            }
            return folderPath;
            // Merci https://stackoverflow.com/a/50263779
        }
    }
}
