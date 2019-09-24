using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class XFileReader {

    public static string[] ReadLines(string filename) {
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            StreamReader reader = new StreamReader(new FileStream(Application.streamingAssetsPath + "/" + filename, FileMode.Open));
            List<string> lines = new List<string>();
            while (true) {
                string line = reader.ReadLine();
                if (line == null) break;
                lines.Add(line);
            }
            reader.Close();
            return lines.ToArray();
        } else if (Application.platform == RuntimePlatform.Android) {
            string url = Application.streamingAssetsPath + "/" + filename;
            WWW www = new WWW(url);
            while (!www.isDone) { }
            return www.text.Split('\n');
        }
        return new string[0];
    }
}