using System;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Oodles
{
    public class HighScoreEntry : IComparable<HighScoreEntry>
    {
        private double score;
        private string name;

        public HighScoreEntry(double score, string name)
        {
            this.score = score;
            this.name = name;
        }

        public double Score
        {
            get { return score; }
        }

        public string Name
        {
            get { return name; }
        }

        #region IComparable<HighScoreEntry> Members

        public int CompareTo(HighScoreEntry other)
        {
            return this.score.CompareTo(other.Score);
        }

        #endregion
    }
}
