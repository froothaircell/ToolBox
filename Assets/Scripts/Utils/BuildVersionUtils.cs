#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ToolBox.Utils
{
    public class BuildVersionUtils : ScriptableWizard
    {
        private const string fileDirectory = "/BuildVersion.json";

        [MenuItem("Tools/Custom Tools/Generate Build Version")]
        public static void GenerateBuildItem()
        {
            var currentDir = Application.dataPath + fileDirectory;

            if (File.Exists(currentDir))
            {
                Debug.Log("File exists");
            }
            else
            {
                Debug.Log("File does not exist, creating");
            }

            FileStream fs = new FileStream(@currentDir, FileMode.Create, FileAccess.Write, FileShare.Write);

            var version = "V_" + TimeUtils.GetCurrentTimeWithoutFormatting() + '_' + TimeUtils.GetCurrentDayNumber() + TimeUtils.GetCurrentMonthNumber() + TimeUtils.GetCurrentYearNumber();

            Debug.Log($"Version: {version}");

            version = "{\n" + "\t\"CurrentVersion\": \"" + version + "\"" + "\n}";
            byte[] bytes = Encoding.ASCII.GetBytes(version);

            fs.Write(bytes);

            fs.Close();
            Debug.Log("Set the build version successfully");
        }
    }
}
#endif