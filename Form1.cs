using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using iTunesLib;

namespace iTunesToFoobar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void notFoundiTunes(IITFileOrCDTrack ft) {
            txtLog.Text = "Could not find file: " + ft.Artist + ";" + ft.Album + ";" + ft.Name + "=\r\n" + txtLog.Text;
        }

        private void notFoundM3U(string line)
        {
            txtLog.Text = "Could not find file in iTunes with location: " + line + "\r\n" + txtLog.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, IITFileOrCDTrack> maps = new Dictionary<string,IITFileOrCDTrack>();

            string m3uLoc;
            openFileDialog1.Filter = "M3U Playlists|*.m3u";
            openFileDialog1.CheckFileExists = true;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m3uLoc = openFileDialog1.FileName;
            }
            else
            {
                MessageBox.Show("Please select a file!");
                return;
            }
            
            iTunesApp iT = new iTunesApp();
            IITTrackCollection tCol = iT.LibraryPlaylist.Tracks;
            prog.Maximum = tCol.Count;
            for(int i = 1; i <= tCol.Count; i++) {
                IITTrack t = iT.LibraryPlaylist.Tracks[i];
                if (t.Kind == ITTrackKind.ITTrackKindFile)
                {
                    IITFileOrCDTrack ft = (IITFileOrCDTrack)t;
                    string location = ft.Location;
                    if(location == null || location == "" ){
                        notFoundiTunes(ft);
                    } else {
                        location = location.ToLower();
                        maps[location] = ft;
                    }

                }
                prog.Value = i;
                prog.Refresh();
            }

            // and now iterate through them and print them
            string[] lines = System.IO.File.ReadAllLines(m3uLoc);
            prog.Maximum = lines.Length;
            prog.Value = 0;
            StringBuilder sb = new StringBuilder();
            foreach (string l in lines)
            {
                string line = l.ToLower();
                if(maps.ContainsKey(line)) {
                    sb.AppendLine(getOutput(maps[line]));
                } else {
                    sb.AppendLine(getDefOutput());
                    notFoundM3U(line);
                }
                prog.Value += 1;
            }

            txtOut.Text = sb.ToString();
        }


        private string getDefOutput()
        {
            // default output to use when we can't find the track in iTunes
            return "0|0||0||";
        }

        private string getDateString(DateTime dt)
        {
            //"YYYY-MM-DD hh:mm:ss" is the format used by foobar2000
            return dt.ToString("yyyy'-'MM'-'dd HH':'mm':'ss");
        }

        private string getOutput(IITFileOrCDTrack ft)
        {
            // format to use in mass tagger
            // %rating%|%play_count%|%last_played%|%skip_count%|%last_skipped%|%added%

            string added = "";
            added = (ft.Rating / 20).ToString();
            added += "|" + ft.PlayedCount;
            added += "|" + getDateString(ft.PlayedDate) + "|" + ft.SkippedCount + "|" + getDateString(ft.SkippedDate) + "|" + getDateString(ft.DateAdded);
            return added;
        }
    }
}
