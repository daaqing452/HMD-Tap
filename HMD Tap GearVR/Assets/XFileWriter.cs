using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class XFileWriter {

    string fileName;
    bool buffered, containTimeTag, isAppend;
    List<String> buffer;

    const float BUFFER_REFRESH_TICK = 10000000;
    //const float BUFFER_REFRESH_TICK = 990000000;
    long buffer_lastTick = 0;

    public XFileWriter(string fileNamePrefix, bool buffered = true, bool containTimeTag = true, bool isAppend = true) {
        fileName = fileNamePrefix + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
        this.buffered = buffered;
        this.containTimeTag = containTimeTag;
        this.isAppend = isAppend;
        buffer = new List<String>();
    }
    
    ~XFileWriter() {
        Flush();
    }

    public void WriteLine(string s) {
        long nowTick = DateTime.Now.Ticks;
        if (containTimeTag) s = (nowTick / 10000000.0) + " " + s;
        buffer.Add(s);
        if (!buffered || nowTick - buffer_lastTick > BUFFER_REFRESH_TICK) {
            Flush();
            buffer_lastTick = nowTick;
        }
    }

    public void Flush() {
        if (buffer.Count == 0) return;
        StreamWriter writer;
        FileMode fileMode = isAppend ? FileMode.Append : FileMode.Create;
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            writer = new StreamWriter(new FileStream(Application.dataPath + "//" + fileName, fileMode));
        } else if (Application.platform == RuntimePlatform.Android) {
            writer = new StreamWriter(new FileStream(Application.persistentDataPath + "//" + fileName, fileMode));
        } else {
            writer = null;
        }
        foreach (string bufferedString in buffer) {
            writer.WriteLine(bufferedString);
        }
        writer.Flush();
        writer.Close();
        buffer.Clear();
    }
}