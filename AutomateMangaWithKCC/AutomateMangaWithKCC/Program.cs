﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

// Phind https://www.phind.com/search/cm9g47dm200002a6how64gd6z
// https://www.webdevtutor.net/blog/c-sharp-zip-file-with-7zip

namespace AutomateMangaWithKCC
{
    class Program
    {
        //E:\Hakuneko_Biblio\_dossierDeTravail\for_script_testing
        private static readonly string continuePrompt = "\n--------- Appuyez sur entrée pour continuer";
        private static readonly string path7zExe = "C:/Program Files/7-Zip/7z.exe";

        private static string pathKCCExe = "C:/Users/Jabar/SOFTWARE/KCC_c2e_7.3.3.exe";
        private static string defaultPagePrefix = "yk_cXxX_p00";

        private static string pagePrefix = defaultPagePrefix;
        private static string coverArt;
        private static string finalArchiveName;
        private static string archiveWorkingFolder;

        private static bool hasCoverArt = false;

        //[System.AttributeUsage(System.AttributeTargets.Method)]
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string selectedFolder = AskUserToSelectFolder();

                Console.WriteLine($"Vérifier l'emplacement de KCC. Emplacement actuel : \n{pathKCCExe}\nSi emplacement incorrect, tapez 'n' pour corriger");
                if (Console.ReadLine().ToLower() == "n")
                {
                    pathKCCExe = AskUserToSelectFile();
                }

                Console.WriteLine($"Le programme va fonctionner dans {selectedFolder} \nPour traiter d'autres archives, il faudra relancer le programme et choisir un autre dossier" + continuePrompt);
                Console.ReadLine();

                Console.WriteLine("Comment doit s'appeler l'archive .cbz finale ? Ce ne sera pas le nom du volume sur la liseuse");

                finalArchiveName = AskUserToInputText(isMangaTitle: true);
                archiveWorkingFolder = selectedFolder + "\\" + finalArchiveName;
                string CBZarchive = archiveWorkingFolder + ".cbz";

                Console.WriteLine($"L'archive finale sera {CBZarchive}");
                Directory.CreateDirectory(archiveWorkingFolder);

                Console.WriteLine("S'il est nécessaire d'inclure une première de couverture au KEPUB, mais qui n'est inclue dans aucune des archives, tapez 'y' pour pouvoir la sélectionner");
                if (Console.ReadLine().ToLower() == "y")
                {
                    coverArt = AskUserToSelectFile();
                    hasCoverArt = true;
                }

                Console.WriteLine($"Par défaut, les pages seront nommées {pagePrefix}01.jpg/png, {pagePrefix}02.jpg/png, etc...\n→ Voulez-vous utiliser un 'préfixe' différent ? Si oui, tapez 'y'");
                if (Console.ReadLine().ToLower() == "y")
                {
                    Console.WriteLine("Entrez le préfixe de votre choix. Vous pouvez aussi ne rien taper pour laisser le préfixe par défaut");
                    pagePrefix = AskUserToInputText(isMangaTitle: false);
                }

                string[] cbzFiles = Directory.GetFiles(selectedFolder, "*.cbz", SearchOption.TopDirectoryOnly);
                string renamingDir = selectedFolder + "\\renameDir";
                Directory.CreateDirectory(renamingDir);

                int count = 0;
                string fullPrefix = pagePrefix;     //Sera changé dans le foreach

                if (hasCoverArt)
                {
                    // move cover art
                    FileInfo coverArtFile = new FileInfo(coverArt);
                    // File.Move(coverArtFile.FullName, archiveWorkingFolder + "\\" + fullPrefix.Replace("XxX", "0") + "00" + coverArtFile.Extension);
                    File.Copy(coverArtFile.FullName, archiveWorkingFolder + "\\" + fullPrefix.Replace("XxX", "0") + "00" + coverArtFile.Extension);
                }

                foreach (string archive in cbzFiles)
                {
                    count++;
                    DecompressFiles(archive, renamingDir);

                    if (pagePrefix == defaultPagePrefix)
                    {
                        fullPrefix = pagePrefix.Replace("XxX", count.ToString());
                        Console.WriteLine("New prefix = " + fullPrefix);
                    }
                    else
                    {
                        fullPrefix = pagePrefix + "c" + count.ToString() + "_p00";
                    }

                    RenameAndMoveFiles(renamingDir, count, fullPrefix);
                }

                Console.WriteLine($"Préparation de la nouvelle archive {finalArchiveName}.cbz");
                CompressToCBZ(archiveWorkingFolder);

                Console.WriteLine("\nAlmost done.....");
                ConvertWithKCC(CBZarchive);

                Console.WriteLine("\n\nFINITO");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Console.Write(e);
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
        private static string AskUserToInputText(bool isMangaTitle = false)
        {
            //Si c'est pas un titre de manga, c'est un prefixe.

            string userInput = isMangaTitle ? "" : pagePrefix;

            bool isInputValidAndConfirmed = false;
            do
            {
                userInput = Console.ReadLine();
                bool isInputEmpty = userInput.Length == 0;

                char[] pagePrefixChars = userInput.ToCharArray();
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
                        userInput = "";
                    }
                }

                if (!isMangaTitle)  //Si c'est un prefixe
                {
                    if (isInputEmpty)
                    {
                        Console.WriteLine("Vous n'avez rien écrit, voulez-vous utiliser le préfixe par défaut ? Tapez 'n' pour entrer un préfixe de votre choix");
                        if (isUserHappyWithInput())
                        {
                            // On tombe dans ce cas si l'utilisateur a tapé autre chose que 'n' quand on lui a demandé s'il voulait utiliser un préfixe
                            userInput = pagePrefix;
                            isInputValidAndConfirmed = true;
                            Console.WriteLine($"Le préfixe par défaut sera utilisé.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Le préfixe utilisé sera {userInput}. Les pages seront donc nommées selon le modèle {userInput}01.jpg, {userInput}02.jpg,.... \n→ Si cela ne convient pas tapez 'n' pour en choisir un nouveau, sinon appuyez sur entrée");
                        isInputValidAndConfirmed = isUserHappyWithInput();
                    }
                }
                else
                {
                    if (isInputEmpty)
                    {
                        Console.WriteLine("Vous n'avez rien écrit, recommencez");
                    }
                    else
                    {
                        Console.WriteLine($"Le nom de l'archive finale sera {userInput}.cbz \n→ Si cela ne convient pas tapez 'n' pour recommencer, sinon appuyez sur entrée");
                        isInputValidAndConfirmed = isUserHappyWithInput();
                    }
                }
            } while (!isInputValidAndConfirmed);

            return userInput;
        }
        private static void DecompressFiles(string archiveFilePath, string extractPath)
        {
            try
            {
                Console.WriteLine($"Extraction : \n{archiveFilePath}");
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = path7zExe,
                    Arguments = $"x \"{archiveFilePath}\" -o\"{extractPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(processInfo))
                {
                    process.WaitForExit();

                    if (process.HasExited)
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Console.Write(e);
                Console.ReadLine();
            }
        }
        private static void CompressToCBZ(string folderToCompress)
        {
            try
            {
                Console.WriteLine($"Compression : \n{folderToCompress}");
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = path7zExe,
                    Arguments = $"a -tzip -mx=9 -mmt=10 \"{folderToCompress}.cbz\" \"{folderToCompress}\\*\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(processInfo))
                {
                    process.WaitForExit();

                    if (process.HasExited)
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Console.Write(e);
                Console.ReadLine();
            }
        }
        private static void ConvertWithKCC(string theCBZ)
        {
            try
            {
                Console.WriteLine("\n\nVous allez entrer les informations liées au manga");

                Console.WriteLine("\nNom du manga (ne pas inclure le numéro du tome)");
                string title = Console.ReadLine();

                bool isValidNumber = false;
                string volume;
                do
                {
                    Console.WriteLine("\nQuel tome ? Entrez un nombre valide");
                    volume = Console.ReadLine();
                    isValidNumber = int.TryParse(volume, out int ignoreOutput);
                } while (!isValidNumber);

                volume = volume.Length == 1 ? "0" + volume : volume;

                Console.WriteLine("\nAuteur(s) ? S'il y en a plusieurs, séparez avec un plus '+' (PAS D'ESPACE ENTRE CHAQUE)");
                string author = Console.ReadLine();
                bool hasMultipleAuthors = author.Contains("+") || author.Contains(",");

                Console.WriteLine("\nDessinateur(s) ? S'il y en a plusieurs, séparez avec un plus '+' (PAS D'ESPACE ENTRE CHAQUE)");
                string artist = Console.ReadLine();
                bool hasMultipleArtists = artist.Contains("+") || artist.Contains(",");

                List<string> authors = new List<string>();
                List<string> artists = new List<string>();

                if (hasMultipleAuthors)
                {
                    string[] authsArray;
                    if (author.Contains("+"))
                    {
                        authsArray = author.Split('+');
                    }
                    else
                    {
                        authsArray = author.Split(',');
                    }

                    foreach (string auth in authsArray)
                    {
                        authors.Add(auth);
                    }
                }
                else
                {
                    authors.Add(author);
                }

                if (hasMultipleArtists)
                {
                    string[] artistsArray;
                    if (artist.Contains("+"))
                    {
                        artistsArray = artist.Split('+');
                    }
                    else
                    {
                        artistsArray = artist.Split(',');
                    }

                    foreach (string art in artistsArray)
                    {
                        authors.Add(art);
                    }
                }
                else
                {
                    artists.Add(artist);
                }

                //Concaténation vraiment déguelasse 😩
                string authorInfo = "";
                if (hasMultipleAuthors)
                {
                    int count = 0;
                    foreach (string aut in authors)
                    {
                        count++;
                        authorInfo += aut + " & ";
                    }
                }
                else
                {
                    authorInfo += authors[0] + " & ";
                }

                if (hasMultipleArtists)
                {
                    int count = 0;
                    foreach (string art in artists)
                    {
                        count++;
                        authorInfo += art + " &";
                    }
                    authorInfo = authorInfo.Remove(authorInfo.Length - 2);
                }
                else
                {
                    authorInfo += artists[0];
                }

                Console.WriteLine($"Voici le(s) auteurs : \n{authorInfo}");

                Console.WriteLine($"KCC : \n{theCBZ}");

                string thePath = Path.GetFullPath(theCBZ);
                string theQuotedPath = $"\"{thePath}\"";

                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = pathKCCExe,
                    Arguments = $"-m -s --profile KoC --title \"{title} V{volume}\" --format EPUB --author  \"{authorInfo}\" {theQuotedPath}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                Console.WriteLine("KCCProcess :");
                Console.WriteLine($"\n{processInfo.FileName} {processInfo.Arguments}\n");

                Process KCCprocess = Process.Start(processInfo);
                string KCCoutput = KCCprocess.StandardOutput.ReadToEnd();
                string KCCerrors = KCCprocess.StandardError.ReadToEnd();
                KCCprocess.WaitForExit();

                Console.WriteLine($"\nOutput : {KCCoutput}");

                if (KCCprocess.ExitCode != 0)
                {
                    Console.WriteLine($"\nErrors : {KCCerrors}");
                }

                KCCprocess.Dispose();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Console.Write(e);
                Console.ReadLine();
            }
        }
        private static void RenameAndMoveFiles(string folder, int chapterCount, string prefixWithChapter)
        {
            DirectoryInfo d = new DirectoryInfo(folder);
            FileInfo[] individualPages = d.GetFiles("*", SearchOption.TopDirectoryOnly);

            int pageCount = 0;
            foreach (FileInfo page in individualPages)
            {
                if (page.Name.ToLower().EndsWith(".jpg") || page.Name.ToLower().EndsWith(".png"))
                {
                    File.Move(page.FullName, archiveWorkingFolder + "\\" + prefixWithChapter + page.Name);
                    pageCount++;
                }
                else
                {
                    File.Delete(page.FullName);
                }
            }
            Console.WriteLine($"Sur ce chapitre, {pageCount} ont été renomées et déplacées.");
        }

    }
}
