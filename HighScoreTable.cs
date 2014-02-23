using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Oodles
{
    public class HighScoreTable
    {
        private List<HighScoreEntry> entries;

        private int capacity;
        private string filename;

        public HighScoreTable(string filename, int capacity)
        {
            this.filename = filename;
            this.capacity = capacity;

            this.entries = new List<HighScoreEntry>(capacity);
        }

        public void Load()
        {
            if (File.Exists(filename))
            {
                try
                {
                    // Initialize the reader
                    XmlTextReader tr = new XmlTextReader(filename);

                    // Begin reading the content elements
                    while (tr.Read())
                    {
                        if (tr.NodeType == XmlNodeType.Element)
                        {
                            if (tr.Name == "Score")
                            {
                                if (entries.Count < capacity)
                                {
                                    string name = tr.GetAttribute("Name");
                                    string score = tr.ReadString();

                                    if (name.Length > 0 && score.Length > 0)
                                    {
                                        entries.Add(new HighScoreEntry(double.Parse(score), name));
                                    }
                                }
                            }
                        }
                    }

                    tr.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                for (int i = 0; i < capacity; i++)
                {
                    Add("John Doe", 0);
                }

                Save();
            }
        }

        public void Save()
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            try
            {
                XmlTextWriter tw = new XmlTextWriter(filename, Encoding.UTF8);

                tw.WriteStartDocument();
                tw.WriteStartElement("Scores");

                for (int i = 0; i < entries.Count; i++)
                {
                    tw.WriteStartElement("Score");

                    // Attribute: Name
                    tw.WriteStartAttribute("Name");
                    tw.WriteString(entries[i].Name);
                    tw.WriteEndAttribute();

                    // Content: Score
                    tw.WriteString(entries[i].Score.ToString());

                    tw.WriteEndElement();
                }

                tw.WriteEndDocument();
                tw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Add(string name, double score)
        {
            if (entries.Count == capacity)
            {
                entries.RemoveAt(capacity - 1);
            }

            entries.Add(new HighScoreEntry(score, name));

            Sort();
        }

        private void Sort()
        {
            entries.Sort();
            entries.Reverse();
        }

        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        public List<HighScoreEntry> Entries
        {
            get { return entries; }
        }
    }
}
