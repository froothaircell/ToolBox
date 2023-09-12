#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ToolBox.Editor
{
#if !DISABLESTEAMWORKS && !(PLATFORM_STANDALONE_OSX || UNITY_STANDALONE_OSX)
    class BuildPostprocessor : IPostprocessBuildWithReport
    {
        // Add tehse files 
        #region File Names
        //private const string BUILD_VERSION = "BuildVersion.json";
        //private const string META_DATA = "Metadata.txt";
        //private const string STEAM_API = "steam_api64.dll";
        //private const string STEAM_APP_ID = "steam_appid.txt";
        #endregion

        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildReport report)
        {
            // Sample code. Won't run without requisite files in FileName placed in base output path
            //string dataOutputPath = Path.Combine(Path.GetDirectoryName(report.summary.outputPath), Application.productName + "_Data");
            //string baseOutputPath = Path.GetDirectoryName(report.summary.outputPath);
            //string buildVersionInputPath = Path.Combine(Application.dataPath, BUILD_VERSION);
            //string metadataInputPath = Path.Combine(Application.dataPath, META_DATA);

            //File.Copy(buildVersionInputPath, Path.Combine(dataOutputPath, BUILD_VERSION));
            //File.Copy(metadataInputPath, Path.Combine(dataOutputPath, META_DATA));


            //string steamAPIInputPath = Path.Combine(Application.dataPath, "Plugins", "x86_64", STEAM_API);
            //string steamAppIDInputPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), STEAM_APP_ID);

            //File.Copy(steamAPIInputPath, Path.Combine(baseOutputPath, STEAM_API));
            //File.Copy(steamAppIDInputPath, Path.Combine(baseOutputPath, STEAM_APP_ID));
        }
    }

// Adjustments for Mac
#elif !DISABLESTEAMWORKS && (PLATFORM_STANDALONE_OSX || UNITY_STANDALONE_OSX)                                                                        
    class BuildPostprocessor : IPostprocessBuildWithReport
    {
        //private const string BUILD_VERSION = "BuildVersion.json";
        //private const string META_DATA = "Metadata.txt";
        //private const string STEAM_API = "steam_api64.dll";
        //private const string STEAM_APP_ID = "steam_appid.txt";

        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildReport report)
        {         
            //string baseOutputPath = report.summary.outputPath;
            //string dataOutputPath = Path.Combine(baseOutputPath, "Contents");
            
            //string buildVersionInputPath = Path.Combine(Application.dataPath, BUILD_VERSION);
            //string metadataInputPath = Path.Combine(Application.dataPath, META_DATA);
            //File.Copy(buildVersionInputPath, Path.Combine(dataOutputPath, BUILD_VERSION));
            //File.Copy(metadataInputPath, Path.Combine(dataOutputPath, META_DATA));


            //string steamAPIInputPath = Path.Combine(Application.dataPath, "Plugins", "x86_64", STEAM_API);
            //string preservePreviousPath = Path.GetDirectoryName(Application.dataPath) + @"\";
            //string steamAppIDInputPath = Path.Combine(Path.GetDirectoryName(preservePreviousPath), STEAM_APP_ID);
            //File.Copy(steamAPIInputPath, Path.Combine(baseOutputPath, "Contents", "MacOS", STEAM_API));
            //File.Copy(steamAppIDInputPath, Path.Combine(baseOutputPath, "Contents", "MacOS", STEAM_APP_ID));

        }
    }

// Adjustments for PS5
#elif PLATFORM_PS5 || UNITY_PS5
    class BuildPostprocessor : IPostProcessPS5
    {
        //private const string BUILD_VERSION = "BuildVersion.json";
        //private const string META_DATA = "Metadata.txt";

        public int callbackOrder => 1;

        public void OnPostProcessPS5(string projectFolder, string outputFolder)
        {
            //string dataOutputPath = Path.Combine(outputFolder, "Data");
            //string buildVersionInputPath = Path.Combine(projectFolder, "Assets", BUILD_VERSION);
            //string metadataInputPath = Path.Combine(projectFolder, "Assets", META_DATA);

            //File.Copy(buildVersionInputPath, Path.Combine(dataOutputPath, BUILD_VERSION));
            //File.Copy(metadataInputPath, Path.Combine(dataOutputPath, META_DATA));
        }
    }
#endif
}
