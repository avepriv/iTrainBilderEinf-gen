using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace InitListsNameSpace
// namespace iTrainBilderEinfügen
{
    /// <summary>
    /// This class handles an "ini-file" of the form
    /// section1:
    /// entry1
    /// entry2
    /// 
    /// section2:
    /// entry1
    /// entry1
    /// It offers a dict section -> list(entry)
    /// </summary>
    public class InitLists
    {
        #region variables
        private List<String> sections = new List<string>();
        private List<String> sections_lc = new List<string>();
        public Dictionary<String, List<String>> entries = new Dictionary<String, List<String>>();
        private String filename = "";
        #endregion variables
        public InitLists(String filename)
        {
            this.filename = filename;
            read();
        }

        /// <summary>
        /// read the ini-file, if it exists, else start an empty file
        /// </summary>
        public void read()
        {
            entries.Clear();
            if(!File.Exists(filename)) {
                return;
            }
            String section = "";
            using(StreamReader sr = new StreamReader(filename)) {
                while(!sr.EndOfStream) {
                    String line = sr.ReadLine().Trim();
                    if(line.EndsWith(":")) {
                        section = line.Substring(0, line.Length - 1).Trim();
                        if(section.Length > 0) {
                            // save the new section
                            if(!sections_lc.Contains(section.ToLower())) {
                                sections.Add(section);
                                section = section.ToLower();
                                sections_lc.Add(section);
                                entries[section] = new List<string>();
                            }
                        }
                    } else if (line.Length > 0 && section != "") {
                        if(!entries[section].Contains(line)) {
                            entries[section].Add(line);
                        }
                    }
                }
            }
        }

        private bool checkSecAndSecLC()
        {
            if (sections.Count != sections_lc.Count) {
                System.Windows.Forms.MessageBox.Show("Sections and Sections_lc have different count!");
                return false;
            }
            foreach(String sec in sections) {
                if(!sections_lc.Contains(sec.ToLower())){
                    System.Windows.Forms.MessageBox.Show($"Could not fine section '{sec}' in sections_lc");
                    return false;
                }
            }
            return true;
        }

        public void write()
        {
            // correct the possibly changed dict
            if(!checkSecAndSecLC()) {
                return;
            }

            // find sections which are not in sections
            foreach(String sec in entries.Keys) {
                if (!sections_lc.Contains(sec)) {
                    if(sections_lc.Contains(sec.ToLower())) {
                        List<String> entriesToMove = entries[sec];
                        String sec_lc = sec.ToLower();
                        foreach(String entry in entriesToMove) {
                            if(!entries[sec_lc].Contains(entry)) {
                                entries[sec_lc].Add(entry);
                            }
                            entries.Remove(sec);
                        }
                    } else {
                        // it seems to be a new section, so add it, and set the key to lower
                        sections.Add(sec);
                        sections_lc.Add(sec.ToLower());
                        entries[sec.ToLower()] = entries[sec];
                        entries.Remove(sec);
                    }
                }
            }

            // create a backup if possible
            if(File.Exists(filename)) {
                String ext = Path.GetExtension(filename);
                String bakName = filename.Replace(ext, "_bak" + ext);
                if(File.Exists(bakName)) {
                    File.Delete(bakName);
                }
                File.Move(filename, bakName);
            }
            // write the init-file
            using(StreamWriter sw = new StreamWriter(filename)) {
                foreach(String sec in sections) {
                    String sec_lc = sec.ToLower();
                    if(!entries.ContainsKey(sec_lc)) {
                        continue;
                    }
                    sw.WriteLine(sec + ":");
                    foreach(String entry in entries[sec_lc]) {
                        sw.WriteLine(entry);
                    }
                    sw.WriteLine("");
                }
            }
        }

        public String getFirst(String section, String defres = "")
        {
            String sec_lc = section.ToLower();
            if(!entries.ContainsKey(sec_lc) || entries[sec_lc].Count == 0) {
                return defres;
            }
            return entries[sec_lc][0];
        }

        public List<String> getUniqueList(String section)
        {
            List<String> res = new List<string>();
            if(entries.ContainsKey(section.ToLower()))
                foreach(String entry in entries[section.ToLower()]) {
                    if(!res.Contains(entry)) {
                        res.Add(entry);
                    }
                }
            return res;
        }

        public List<String> getUniqueList(String section, char commentChar)
        {
            List<String> res = new List<string>();
            if(entries.ContainsKey(section.ToLower()))
                foreach(String entry in entries[section.ToLower()]) {
                    String candidate = strip_comment(entry, commentChar);
                    if(!res.Contains(candidate)) {
                        res.Add(candidate);
                    }
                }
            return res;
        }

        public List<String> getUniqueList_lc(String section)
        {
            List<String> res = new List<string>();
            if(entries.ContainsKey(section.ToLower()))
                foreach(String entry in entries[section.ToLower()]) {
                    if(!res.Contains(entry.ToLower())) {
                        res.Add(entry.ToLower());
                    }
                }
            return res;
        }

        public List<String> getUniqueList_lc(String section, char commentChar)
        {
            List<String> res = new List<string>();
            if(entries.ContainsKey(section.ToLower()))
                foreach(String entry in entries[section.ToLower()]) {
                    String candidate = strip_comment(entry.ToLower(), commentChar);
                    if(!res.Contains(candidate)) {
                        res.Add(candidate);
                    }
                }
            return res;
        }

        public List<int> getUniqueNumList(String section)
        {
            List<int> res = new List<int>();
            if(entries.ContainsKey(section.ToLower()))
                foreach(String entry in entries[section.ToLower()]) {
                    int num = getInt(entry, -1);
                    if(num != -1 && !res.Contains(num)) {
                        res.Add(num);
                    }
                }
            return res;
        }

        /// <summary>
        /// set a new value, optionally clear the entry before adding
        /// </summary>
        /// <param name="section"></param>
        /// <param name="value"></param>
        public void set(String section, String value, bool clearExistingList)
        {
            String section_lc = section.ToLower();
            if(!entries.ContainsKey(section_lc)) {
                sections.Add(section);
                sections_lc.Add(section_lc);
                entries[section_lc] = new List<string>();
            } else if(clearExistingList) {
                entries[section_lc].Clear();
            }
            entries[section_lc].Add(value);
        }

        public static String strip_comment(String line, char commChar = ' ')
        {
            return line.Split(commChar)[0].Trim();
        }

        /// <summary>
        /// get an integer from the given string. The string has to start with
        /// a number, and may be followed by other text
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        private int getInt(String str, int defVal)
        {
            int res;
            String theStr = str.Trim();

            // get the number-part
            int indx = 0;
            while(indx < theStr.Length && "0123456789".IndexOf(theStr[indx]) >= 0) {
                ++indx;
            }
            if(indx == 0) {
                return defVal;
            } else {
                theStr = theStr.Substring(0, indx);
                if(!Int32.TryParse(theStr, out res)) {
                    res = defVal;
                }
                return res;
            }
        }
    }
}
