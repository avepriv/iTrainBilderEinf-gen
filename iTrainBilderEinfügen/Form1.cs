using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Xml;
using InitListsNameSpace;

namespace iTrainBilderEinfügen
{
    public partial class Form1 : Form
    {

        #region Variables
        private String basispfad = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\iTrain";
        List<int> memberNums = new List<int>();
        List<String> manufacturers = new List<string>();
        Dictionary<int, String> images = new Dictionary<int, string>();
        String tdcName;
        Logger logger = new Logger();
        InitLists initList = null;
        

        public static String listsName;
        const String layoutdatei = "Layout-Datei";
        const String imagespath = "Bilder-Pfad";
        const String mitglieder = "Mitglieder";
        const String hersteller = "Hersteller";
        #endregion Variables

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(Object sender, EventArgs e)
        {
            listsName = basispfad + "\\Mitglieder_Hersteller.txt";
            // read the member- and manufacturer list
            if(!ReadMemberAndManuLists()) {
                Cursor = Cursors.Default;
                logger.show();
                return;
            }
        }

        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            // update the paths
            initList.set(layoutdatei, iTrainLayoutTB.Text, true);
            initList.set(imagespath, imageFldrTB.Text, true);
            // write the init-list (paths, members, and manufacturers
            initList.write();
        }

        private void layoutBtn_Click(Object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.InitialDirectory = repBasispfad(iTrainLayoutTB.Text);
                ofd.Filter = "iTrain files|*.tcdz";
                if(ofd.ShowDialog() == DialogResult.OK) {
                    iTrainLayoutTB.Text = repBasispfad(ofd.FileName);
                }
            }
        }

        private void ImgPathBtn_Click(Object sender, EventArgs e)
        {
            using(FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                fbd.SelectedPath = repBasispfad(imageFldrTB.Text);
                if(fbd.ShowDialog() == DialogResult.OK) {
                    imageFldrTB.Text = repBasispfad(fbd.SelectedPath);
                }
            }
        }

        private void StartBtn_Click(Object sender, EventArgs e)
        {
            XmlDocument xDoc = null;
            int totalLocCntr = 0;
            int totalWagonCntr = 0;
            int adaptedLocCntr = 0;
            int adaptedWagonCntr = 0;

            Cursor = Cursors.WaitCursor;
            logger.clear();
            Status("Lese Bild-Ordner ...");

            // check layout-file and images folder for existence, and load images into the list
            String iTrainLayoutFile = repBasispfad(iTrainLayoutTB.Text, false);
            if(!File.Exists(iTrainLayoutFile)) {
                ErrMsg($"Layout '{iTrainLayoutFile}' konnte nicht gefunden werden!", "Datei nicht gefunden");
                Cursor = Cursors.Default;
                return;
            }

            if(!loadImageList()) {
                ErrMsg("Fehler beim Lesen der Bilder - Siehe Bericht", "Fehler bei Bildern");
                Cursor = Cursors.Default;
                logger.show();
                return;
            }

            // load the file
            xDoc = openTCDZ(iTrainLayoutFile);
            XmlNode ctrlItems = xDoc["train-control"]["control-items"];

            // Check and update the locomotives
            if(ctrlItems["locomotives"] != null) {
                foreach(XmlNode loc in ctrlItems["locomotives"].ChildNodes) {
                    if(loc.Name == "locomotive") {
                        Status($"Bearbeite Locomotive '{getValue(loc, "name")}' ...");
                        ++totalLocCntr;
                        if(checkVehicle(loc)) {
                            ++adaptedLocCntr;
                        }
                    }
                }
            }

            // Check and update the wagons
            if(ctrlItems["wagons"] != null) {
                foreach(XmlNode wagon in ctrlItems["wagons"].ChildNodes) {
                    if(wagon.Name == "wagon") {
                        Status($"Bearbeite Wagon '{getValue(wagon, "name")}' ...");
                        ++totalWagonCntr;
                        if(checkVehicle(wagon)) {
                            ++adaptedWagonCntr;
                        }
                    }
                }
            }

            if(!logger.hasCritical) {
                // rename the original and save the new one
                String ext = Path.GetExtension(iTrainLayoutFile);
                String oldName = iTrainLayoutFile.Replace(ext, "_alt" + ext);

                if(File.Exists(oldName)) {
                    File.Delete(oldName);
                }

                File.Move(iTrainLayoutFile, oldName);
                saveNewArchive(xDoc, iTrainLayoutFile, tdcName);
            }

            logger.log(null, ""); //empty line
            logger.log(null, $"{totalLocCntr} Lokomotiven und {totalWagonCntr} Wagons bearbeitet, \r\n" 
                    + $"         davon {adaptedLocCntr} Lokomotiven und {adaptedWagonCntr} Wagons angepasst");
            logger.show();
            Status("");
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Update the status-bar
        /// </summary>
        /// <param name="msg"></param>
        private void Status(String msg)
        {
            statusLbl.Text = msg;
            Application.DoEvents(); 
        }

        #region list and check images
        private bool loadImageList()
        {
            int total = 0;
            int loaded = 0;
            images.Clear();
            String imgFldr = repBasispfad(imageFldrTB.Text);
            if(!Directory.Exists(imgFldr)) {
                logger.critical(null, "Ordner für Bilder ('{imgFldr}') nicht gefunden");
                return false;
            }
            foreach(String filename in Directory.EnumerateFiles(imgFldr)) {
                ++total;
                int imgNum = getInt(Path.GetFileName(filename), -1);
                if(imgNum != -1) {
                    if(images.ContainsKey(imgNum)) {
                        logger.critical(null, "Es gibt mehr als ein Bild mit der Nummer {imgNum}");
                    }
                    images[imgNum] = filename;
                    ++loaded;
                }
            }
            logger.log(null, $"{loaded} von {total} Bildern erfasst");
            return true;
        }
        #endregion list and check images

        #region check description

        /// <summary>
        /// Read the lists of members and manufacturers.
        /// Format: 
        /// Mitglieder:
        /// -num- (optional space and text)
        /// ...
        /// Hersteller:
        /// -initials- (optional space and text)
        /// </summary>
        /// <returns></returns>
        private bool ReadMemberAndManuLists()
        {
            if(!File.Exists(listsName)) {
                ErrMsg($"Mitglieder- und Hersteller Listen ({listsName}) konnten nicht gefunden werden!", 
                        "Datei nicht gefunden");
                return false;
            }
            initList = new InitLists(listsName);
            memberNums.Clear();
            manufacturers.Clear();

            // read the layout file, if available
            iTrainLayoutTB.Text = initList.getFirst(layoutdatei, iTrainLayoutTB.Text);

            // read the image folder path , if available
            imageFldrTB.Text = initList.getFirst(imagespath, imageFldrTB.Text);

            // read the members
            memberNums = initList.getUniqueNumList(mitglieder);

            // read the manufacturers
            manufacturers = initList.getUniqueList_lc(hersteller, ' ');
            return true;
        }

        /// <summary>
        /// Check the format of the description field.
        /// Should be "imageNumber-MemberNumber-Manufacturer-value"
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns>If valid returns the image number, otherwise -1</returns>
        int CheckDescriptionFields(XmlNode vehicle)
        {
            String descr = getValue(vehicle, "description", "");
            if (descr == "") {
                logger.error(vehicle, "Beschreibung fehlt");
                return -1;
            }
            String[] descrParts = descr.Split('-');
            if (descrParts.Length != 4) {
                logger.error(vehicle, $"Beschreibung enthält {descrParts.Length} Teile, 4 erwartet");
                return -1;
            }
            // check member
            if (!memberNums.Contains(getInt(descrParts[1], -1))) {
                logger.error(vehicle, $"Ungültige Mitgliedsnummer '{descrParts[1].Trim()}'");
                return -1;
            }

            // check manufacturer
            if(!manufacturers.Contains(descrParts[2].Trim().ToLower())) {
                logger.error(vehicle, $"Ungültige Herstellerkennung '{descrParts[2].Trim()}'");
                return -1;
            }

            // check that value is a number
            for(int i = 0; i < descrParts[3].Trim().Length; ++i) {
                if ("0123456789,".IndexOf(descrParts[3].Trim()[i]) < 0) {
                    logger.error(vehicle, $"Ungültige Wertangabe '{descrParts[3].Trim()}' (nur Ziffern und Komma erlaubt)");
                    return -1;
                }
            }

            // check image-number
            int imageNum = getInt(descrParts[0].Trim(), -1);
            if(imageNum == -1) {
                logger.error(vehicle, "Bildnummer ist keine Zahl");
                return -1;
            }

            // check whether the referenced image exists 
            if(!images.ContainsKey(imageNum)) {
                logger.error(vehicle, $"Bilddatei mit Nummer {imageNum} nicht gefunden");
                return -1;
            }
            return imageNum;
        }

        /// <summary>
        /// check the format of the description field, and if ok update the image-entry
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        bool checkVehicle(XmlNode vehicle)
        {
            int imgNum = CheckDescriptionFields(vehicle);
            if (imgNum == -1) {
                return false;
            }
            if(UpdateNodeAttr(vehicle, "image", "file", images[imgNum].Replace(basispfad, "iTrain").Replace("\\", "/"))) {
                logger.log(vehicle, "Bild angepasst");
                return true;
            }
            return false;
        }
        #endregion check description

        #region iTrain access
        /// <summary>
        ///  open the zip-file (tcdz-file) with the given path 
        ///  - save it to the global varialbe archive, 
        ///  - open the xml-document 
        ///  - read the image index
        ///  - fill the listbox with its items
        /// </summary>
        /// <param name="zipPath"></param>
        private XmlDocument openTCDZ(String archivePath)
        {
            XmlDocument xmlDoc = null;

            using(ZipArchive archive = ZipFile.OpenRead(archivePath)) {
                foreach(ZipArchiveEntry entry in archive.Entries) {
                    // load the tcd-xml file into a document.
                    if(entry.FullName.ToLower().EndsWith(".tcd")) {
                        tdcName = entry.FullName;
                        xmlDoc = new XmlDocument();
                        xmlDoc.PreserveWhitespace = true;
                        using(Stream fstrm = entry.Open()) {
                            xmlDoc.Load(fstrm);
                        }
                        break;
                    }
                }
                return xmlDoc;
            }
        }

        public static String getAttr(XmlNode node, String attrName, String defval = "-")
        {
            String res = node?.Attributes?[attrName]?.InnerText;
            if(res == null) {
                return defval;
            }
            return res;
        }

        String getSubAttr(XmlNode node, String subNode, String attrName, String defval = "-")
        {
            XmlNode sub = node?[subNode];
            return getAttr(sub, attrName, defval);
        }

        public static String getValue(XmlNode node, String subNode, String defVal = "-")
        {
            XmlNode sub = node?[subNode];
            if(sub == null) {
                return defVal;
            }
            return sub.InnerText;
        }

        /// <summary>
        /// convert the German special chars to HTML-encoding
        /// </summary>
        /// <param name="orgStr"></param>
        /// <returns></returns>
        String htmlEncode(String orgStr)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<char, String> transl = new Dictionary<char, string> {
                { 'Ä', "&#196;"}, {'ä', "&#228;"}, {'Ö', "&#214;"}, {'ö', "&#246;"},
                { 'Ü', "&#220;"}, {'ü', "&#252;"}, {'ß', "&#223;"}};
            foreach(char c in orgStr) {
                String outstr;
                if(transl.TryGetValue(c, out outstr)) {
                    sb.Append(outstr);
                } else {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private XmlNode AddNode(XmlNode parent, String name, String inner = "")
        {

            // get the whitespace text from this node, or if not found from parent node
            XmlNode exwhsp = parent["#whitespace"];
            String whiteInner = "\r\n  ";
            if(exwhsp != null) {
                whiteInner = exwhsp.InnerText;
            } else if(parent.ParentNode != null && parent.ParentNode["#whitespace"] != null) {
                whiteInner = parent.ParentNode["#whitespace"].InnerText + "  ";
            }

            // add the whitespace node, the actual node and the trailing whitespace-node
            // if the last child is a whitespace, replace its text to whiteInner, otherwise add a node
            if(parent.LastChild != null && parent.LastChild.Name == "#whitespace") {
                parent.LastChild.InnerText = whiteInner;
            } else {
                _SimpleAddNode(parent, "#whitespace", whiteInner);
            }
            XmlNode theNode = _SimpleAddNode(parent, name, inner);
            _SimpleAddNode(parent, "#whitespace", "\r\n");

            return theNode;

            XmlNode _SimpleAddNode(XmlNode _parent, String _name, String _inner)
            {
                XmlNode newNode;
                if(_name == "#whitespace") {
                    newNode = _parent.OwnerDocument.CreateWhitespace(_inner);
                } else {
                    newNode = _parent.OwnerDocument.CreateElement(_name);
                }
                if(_inner != "") {
                    newNode.InnerText = _inner;
                }
                parent.AppendChild(newNode);
                return newNode;
            }

        }

        /// <summary>
        /// Set the value for an Attribute. Create it if it does not exist
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attrName"></param>
        /// <param name="attrValue"></param>
        /// <returns>true, if the attribute was changed</returns>
        private bool UpdateAttr(XmlNode node, String attrName, String attrValue)
        {
            bool changed = true;
            XmlAttribute attr = node?.Attributes?[attrName];
            if (attr != null) {
                if(attr.Value != attrValue) {
                    node.Attributes[attrName].Value = attrValue;
                } else {
                    changed = false;
                }
            } else {
                attr = node.OwnerDocument.CreateAttribute(attrName);
                attr.Value = attrValue;
                node.Attributes.Append(attr);
            }
            return changed;
        }

        /// <summary>
        /// Update inner text, if the node exists, otherwise add the node
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="inner"></param>
        private void UpdateNode(XmlNode parent, String name, String inner = "")
        {
            if(parent[name] != null) {
                parent[name].InnerText = inner;
            } else {
                AddNode(parent, name, inner);
            }
        }

        /// <summary>
        /// Update an attribute, Create it, if it does not exist, create the node, if it does not exist
        /// </summary>
        /// <param name="xmlNode"></param>
        private bool UpdateNodeAttr(XmlNode parent, String nodeName, String attrName, String attrValue)
        {
            XmlNode theNode = parent[nodeName];
            if (theNode == null) {
                theNode = AddNode(parent, nodeName, "");
            }
            return UpdateAttr(theNode, attrName, attrValue);
        }

        /// <summary>
        /// save the corrected xml (tcd), 
        /// </summary>
        private void saveNewArchive(XmlDocument xmlDoc, String tcdzFileName, String archiveName)
        {
            // create a new archive with the same name and extension_new
            if(File.Exists(tcdzFileName)) {
                File.Delete(tcdzFileName); // delete the new zip, if it already exists
            }
            using(ZipArchive newArchive = ZipFile.Open(tcdzFileName, ZipArchiveMode.Create)) {

                // save the tcd (xml) file
                ZipArchiveEntry newTcdEntry = newArchive.CreateEntry(archiveName);
                // convert the xml-doc as string and remove the extra space
                using(StreamWriter sw = new StreamWriter(newTcdEntry.Open(), Encoding.ASCII)) {
                    sw.Write(htmlEncode(xmlDoc.OuterXml).Replace(" />", "/>"));
                }
            }
        }

        #endregion iTrain access

        #region helpers

        /// <summary>
        /// show an error message
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private DialogResult ErrMsg(String msg, String title)
        {
            logger.error(null, msg);
            return MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            if (indx == 0) {
                return defVal;
            } else {
                theStr = theStr.Substring(0, indx);
                if(!Int32.TryParse(theStr, out res)) {
                    res = defVal;
                }
                return res;
            }
        }

        /// <summary>
        ///  if the path begins with "Basispfad", replace that with basispfad and vice versa
        /// </summary>
        /// <param name="path"></param>
        /// <param name="allowBack">Replace basispfad with "Basispfad" only if true and </param>
        /// <returns></returns>
        private String repBasispfad(String path, bool allowBack = true)
        {
            if(path.StartsWith("Basispfad")) {
                return path.Replace("Basispfad", basispfad);
            } else if(allowBack) {
                return path.Replace(basispfad, "Basispfad");
            } else {
                return path;
            }
        }

        private String getFullPath(String path, String name)
        {
            String fullPath = repBasispfad(path, false);
            if(!fullPath.EndsWith("\\")) {
                fullPath += "\\";        
            }
            return fullPath + name;
        }

        #endregion helpers

    }

    public class Logger
    {
        private StringBuilder logTxt = new StringBuilder();
        private StringBuilder errorTxt = new StringBuilder();
        private logWin win = new logWin();
        public bool hasCritical { get; set; } = false;

        public Logger() {
            win.Hide();
        }

        public void clear()
        {
            logTxt.Clear();
            errorTxt.Clear();
        }

        public void log(XmlNode vehicle, String msg)
        {
            if(vehicle == null && msg == "") {
                logTxt.Append("\r\n"); // just add an empty line
                return;
            }
            logTxt.Append($"{Form1.getAttr(vehicle, "name", "General")}: {msg}\r\n");
        }

        public void error(XmlNode vehicle, String msg)
        {
            errorTxt.Append($"{Form1.getAttr(vehicle, "name", "General")}: {msg}\r\n");
        }

        public void critical(XmlNode vehicle, String msg)
        {
            errorTxt.Append($"{Form1.getAttr(vehicle, "name", "General")}: {msg}\r\n");
            hasCritical = true;
        }

        public bool hasErrors { get { return errorTxt.Length > 0; } }
        public void show()
        {
            win.clear();
            win.write($"Benutzte Hersteller- und Mitglieder-Datei:\r\n{Form1.listsName}\r\n\r\n");
            win.write("Fehlerprotokoll:\r\n");
            win.write("----------------\r\n");
            if(errorTxt.Length == 0) {
                win.write("Keine Fehler!\r\n");
            } else {
                win.write(errorTxt.ToString());
            }

            String headline = $"Bilder Einfügen Logs {DateTime.Now}";
            win.write($"\r\n{headline}\r\n");
            win.write("------------------------------------------".Substring(0, headline.Length) + "\r\n");
            win.write(logTxt.ToString());

            win.Show();
        }
    }
}
