using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SingDictionaryWPF
{

    [SerializableAttribute()]
    [XmlTypeAttribute]
    public class Word : IComparable<Word>
    {
        #region fields
        private string nameField;

        private string firstLetterField;        

        private Video[] videosField;
        
        [XmlElementAttribute]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [XmlElementAttribute]
        public string Category { get; set; }

        [XmlElementAttribute]
        public string Subcategory { get; set; }


        [XmlElementAttribute]
        public string FirstLetter
        {
            get
            {
                return this.firstLetterField;
            }
            set
            {
                this.firstLetterField = value;
            }
        }

        [XmlArrayAttribute]
        [XmlArrayItemAttribute("Video", typeof(Video))]
        public Video[] Videos
        {
            get
            {
                return this.videosField;
            }
            set
            {
                this.videosField = value;
            }
        }
        #endregion

        //dict['front'] and dict['side']
        private Dictionary<string, Dictionary<string, Video>> videoDict;
        [XmlIgnore]
        public Dictionary<string, Dictionary<string, Video>> VideoDict
        {
            get { return videoDict; }
            set { videoDict = value; }
        }

      


        #region equals,comparable
        //overidden
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }




        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Word return false.
            Word w = obj as Word;
            if ((System.Object)w == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.Name == w.Name);
        }



        public override string ToString()
        {
            return Name + (videoDict.Count > 1 ? " ("+ videoDict.Count +")" : "");
        }


        public int CompareTo(Word other)
        {
            return Name.CompareTo(other.Name);
        }
        #endregion
    }


    [SerializableAttribute()]
    [XmlTypeAttribute]
    public class Video
    {

        private const String DATA_FOLDER_NAME = "data/videos/";
        private const String DATA_FOLDER_NAME_ABS = "C:/videos/videos_avi/";

        private string orientationField;

        private string versionField;

        private string pathField;

        private Uri uri;

        [XmlAttributeAttribute()]
        public string orientation{get{return this.orientationField;}set{this.orientationField = value;}}

        [XmlAttributeAttribute()]
        public string version { get { return this.versionField; } set { this.versionField = value; } }

        [XmlAttributeAttribute()]
        public string path { get { return this.pathField; } set { this.pathField = value; } }

        public string FullPath
        {
            get
            {
                //return DATA_FOLDER_NAME_ABS + path;
                return DATA_FOLDER_NAME+ path;
            }
        }
        [XmlIgnore]
        public Uri Uri { get { return uri;} set{ uri = value;}  }



        
    }
}
