

using LT.Logger;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;

namespace LT_Nemesis
{
    public class NemesisTextManager
    {

        bool _debug = false;

        public static Dictionary<string, string> VoiceLines = new();

        public static NemesisTextManager Instance { get; set; }

        public NemesisTextManager()
        {
            Instance = this;
        }


        public void Initialize()
        {

            VoiceLines.Clear();

            // read voiceLines from files and put into Dictionary

            // get current working mod folder
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string parentFolder = Directory.GetParent(assemblyFolder)?.Parent?.FullName;
            string soundsFolder = System.IO.Path.Combine(parentFolder, "ModuleSounds\\");

            string[] subFolders = Directory.GetDirectories(soundsFolder);

            foreach (string subFolder in subFolders)
            {
                string folderName = Path.GetFileName(subFolder);
               

                string gender = "";
                string voiceNumber = "";

                if (folderName.Contains("Female"))
                {
                    gender = "f";
                    voiceNumber = folderName.Replace("Female", "");
                } else if (folderName.Contains("Male"))
                {
                    gender = "m";
                    voiceNumber = folderName.Replace("Male", "");
                } else
                {
                    continue; // wrong named folder, skipping
                }

                if (! int.TryParse(voiceNumber, out int number))
                {
                    continue; // voiceNumber not found
                }

                if (_debug) LTLogger.IMGrey(folderName + " gender: " + gender);

                string[] subSubFolders = Directory.GetDirectories(subFolder);
                foreach (string subSubFolder in subSubFolders)
                {
                    string subSubFolderName = Path.GetFileName(subSubFolder);
                    if (_debug) LTLogger.IMGrey("  " + subSubFolderName);

                    bool voiceLineTXTOK = false;
                    string voiceLineTXTFile = System.IO.Path.Combine(subSubFolder, "VoiceLines.txt");
                    if (File.Exists(voiceLineTXTFile))
                    {
                        //LTLogger.IMTAGreen("    VoiceLines.txt PRESENT");

                        // Check if the file is readable
                        try
                        {
                            using (FileStream fileStream = File.OpenRead(voiceLineTXTFile))
                            {
                                // File is readable
                                voiceLineTXTOK = true;
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            LTLogger.IMRed("LT_Nemesis ERROR: " + folderName + "\\" + subSubFolderName + "\\VoiceLines.txt is not readable due to lack of permissions");
                        }
                        catch (IOException)
                        {
                            LTLogger.IMRed("LT_Nemesis ERROR: " + folderName + "\\" + subSubFolderName + "\\VoiceLines.txt is not readable due to other IO errors");
                        }
                    } else
                    {
                        LTLogger.IMRed("LT_Nemesis ERROR: " + folderName + "\\" + subSubFolderName + "\\VoiceLines.txt NOT PRESENT");
                    }

                    if (voiceLineTXTOK)
                    {
                        if (_debug) LTLogger.IMTAGreen("    VoiceLines.txt PRESENT and Readable");

                        // finally we can read the file
                        string[] lines = File.ReadAllLines(voiceLineTXTFile);

                        int i = 1;                       
                        foreach (string line in lines)
                        {
                            
                            string voiceName = gender + voiceNumber + "_" + subSubFolderName.ToLower() + "_" + i.ToString() ;

                            if (_debug) LTLogger.IMGrey(voiceName + " - " + line);

                            if (!VoiceLines.ContainsKey(voiceName))
                            {
                                VoiceLines.Add(voiceName, line);
                            }

                            i++;
                        }

                    }
                    
                }
            }

            if (_debug) LTLogger.IMTAGreen("NemesisTextManager initialized. Assembly Folder: " + soundsFolder);
        }

        public string GetVoiceLineTextByVoiceName(string key)
        {

            string voiceName = "!!!";

            // remove Pitch modification
            int indexOfP = key.IndexOf("-p");
            if (indexOfP != -1)
            {
                key = key.Substring(0, indexOfP);
            }

            if (_debug) voiceName = key;

            if (VoiceLines.ContainsKey(key))
            {
                voiceName = VoiceLines[key];
            }

            return voiceName;
        }

    }
}