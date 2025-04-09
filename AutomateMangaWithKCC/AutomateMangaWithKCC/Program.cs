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
                string selectedFolder = AskUserToSelectFolder();

                Console.WriteLine($"Le programme va fonctionner dans {selectedFolder} \nPour traiter d'autres archives, il faudra relancer le programme et choisir un autre dossier" + continuePrompt);
                Console.ReadLine();

                Console.WriteLine("S'il est nécessaire d'inclure une première de couverture au KEPUB, mais qui n'est inclue dans aucune des archives, tapez 'y' pour pouvoir la sélectionner");
                if (Console.ReadLine().ToLower() == "y")
                {
                    coverArt = AskUserToSelectFile();
                    hasCoverArt = true;
                }

                Console.WriteLine($"Par défaut, les pages seront nommées {pagePrefix}01.jpg/png, {pagePrefix}02.jpg/png, etc...\n→ Voulez-vous utiliser un 'préfixe' différent ? Si oui, tapez 'y'");
                if (Console.ReadLine().ToLower() == "y")
                {
                    pagePrefix = AskUserToInputPagePrefix();
                }

                Console.WriteLine($"Alrighty : {pagePrefix}");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Console.ReadLine();
            }
        }
        private static string AskUserToSelectFolder()
        {
            bool isFolderSelected;
            string folder;
            do
            {
                Console.WriteLine("\nVous allez sélectionner le dossier où se trouvent les archives .cbz ou .zip" + continuePrompt);
                Console.ReadLine();
                folder = PromptUserSelect();
                Console.WriteLine($"Le dossier sélectionné est : {folder} \n→ Si ça ne convient pas tapez 'n' pour choisir un nouveau dossier, sinon appuyez sur entrée");
                isFolderSelected = isUserHappyWithInput();
            } while (!isFolderSelected);

            return folder;
        }
        private static string AskUserToSelectFile()
        {
            bool isFileSelected;
            string filePath;
            do
            {
                Console.WriteLine("\nVous allez sélectionner le fichier." + continuePrompt);
                Console.ReadLine();
                filePath = PromptUserSelect(false);
                Console.WriteLine($"Le fichier sélectionné est : {filePath} \n→ Si ça ne convient pas tapez 'n' pour choisir un nouveau fichier, sinon appuyez sur entrée");
                isFileSelected = isUserHappyWithInput();
            } while (!isFileSelected);

            return filePath;
        }
        private static bool isUserHappyWithInput(string toCheck = "n")
        {
            //Méthode pour véridier que l'utilisateur est content de ce qu'il a entré.
            //Si l'utilisateur n'est pas satisfait de son entrée, il tape "n" ou "N" (par défaut, mais on peut changer le check)

            string userInput = Console.ReadLine();
            bool isUserConfirmsInput = userInput.ToLower() == toCheck ? false : true;
            return isUserConfirmsInput;
        }
        private static string PromptUserSelect(bool isFolder = true)
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
        private static string AskUserToInputPagePrefix()
        {
            string chosenPagePrefix = pagePrefix;

            bool isCustomPrefixSet = false;
            do
            {
                Console.WriteLine("Entrez le préfixe de votre choix. Dans tous les cas, il est conseillé de faire terminer le préfixe par '00'. Vous pouvez aussi ne rien taper pour laisser le préfixe par défaut");
                chosenPagePrefix = Console.ReadLine();
                bool isInputEmpty = chosenPagePrefix.Length == 0;

                char[] pagePrefixChars = chosenPagePrefix.ToCharArray();
                Array.Reverse(pagePrefixChars);

                foreach (char character in pagePrefixChars)
                {
                    //Tant qu'on trouve des caractères vides, on continue de considérer l'input comme une suite d'espace. Dés qu'on trouve un vrai caractère, on arrête de checker
                    if (!char.IsWhiteSpace(character))
                    {
                        isInputEmpty = false;
                        break;
                    }
                    else
                    {
                        isInputEmpty = true;
                        chosenPagePrefix = "";
                    }
                }

                if (isInputEmpty)
                {
                    Console.WriteLine("Vous n'avez rien écrit, voulez-vous utiliser le préfixe par défaut ? Tapez 'n' pour entrer un préfixe de votre choix");
                    if (isUserHappyWithInput())
                    {
                        // On tombe dans ce cas si l'utilisateur a tapé autre chose que 'n' quand on lui a demandé s'il voulait utiliser un préfixe
                        chosenPagePrefix = pagePrefix;
                        isCustomPrefixSet = true;
                        Console.WriteLine($"Le préfixe par défaut ser utilisé.");
                    }
                }
                else
                {
                    Console.WriteLine($"Le préfixe utilisé sera {chosenPagePrefix}. Les pages seront donc nommées selon le modèle {chosenPagePrefix}01.jpg, {chosenPagePrefix}02.jpg,.... \n→ Si cela ne convient pas tapez 'n' pour en choisir un nouveau, sinon appuyez sur entrée");
                    isCustomPrefixSet = isUserHappyWithInput();
                }

            } while (!isCustomPrefixSet);

            return chosenPagePrefix;
        }
    }
}
