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


namespace iTrainBilderEinfügen
{
    public partial class Form1 : Form
    {

        #region Variables
        private String basispfad = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\iTrain";
        #endregion Variables

        public Form1()
        {
            InitializeComponent();
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
            } else if (allowBack) {
                return path.Replace(basispfad, "Basispfad");
            } else {
                return path;
            }
        }

        private void layoutBtn_Click(Object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.InitialDirectory = repBasispfad(iTrainLayoutTB.Text);
                ofd.Filter = "iTrain files (tcdz)|*.tcdz";
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
            Cursor = Cursors.WaitCursor;
            Status("");

            // check layout-file and images folder for existence
            String iTrainLayoutFile = repBasispfad(iTrainLayoutTB.Text, false);
            String imageFolder = repBasispfad(imageFldrTB.Text, false);
            if(!File.Exists(iTrainLayoutFile)) {
                MessageBox.Show($"Layout '{iTrainLayoutFile}' konnte nicht gefunden werden!", "Datei nicht gefunden", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Cursor = Cursors.Default;
                return;
            }
            if(!Directory.Exists(imageFolder)) {
                MessageBox.Show($"Bild-Ordner '{imageFolder}' konnte nicht gefunden werden!", "Ordner nicht gefunden", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Cursor = Cursors.Default;
                return;
            }


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
            ZipArchive archive = ZipFile.OpenRead(archivePath);
            XmlDocument xmlDoc = null;
            foreach(ZipArchiveEntry entry in archive.Entries) {
                // load the tcd-xml file into a document.
                if(entry.FullName.ToLower().EndsWith(".tcd")) {
                    ZipArchiveEntry tcdEntry = entry;
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

        String getAttr(XmlNode node, String attrName, String defval = "-")
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

        String getValue(XmlNode node, String subNode, String defVal = "-")
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

        static void AddNode(XmlNode parent, String name, String inner = "")
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
            _SimpleAddNode(parent, name, inner);
            _SimpleAddNode(parent, "#whitespace", "\r\n");

            void _SimpleAddNode(XmlNode _parent, String _name, String _inner)
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
            }

        }

        #endregion iTrain access
    }
}
