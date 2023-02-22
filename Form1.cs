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

namespace Media_Player
{
    public partial class Form1 : Form
    {
        List <string> FilteredVideos = new List<string>();
        FolderBrowserDialog br = new FolderBrowserDialog();
        int CurrentFile = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void LoadDirectory(object sender, EventArgs e)
        {
            MediaPlayer.Ctlcontrols.stop();
            if(FilteredVideos.Count>1)
            {
                FilteredVideos.Clear();
                FilteredVideos = null;

                ListOfVideos.Items.Clear();
                CurrentFile = 0;
            }

            DialogResult result = br.ShowDialog();
            if(result == DialogResult.OK)
            {
                FilteredVideos = Directory.GetFiles(br.SelectedPath, "*.*").Where(files => files.ToLower().EndsWith("mp4")).ToList();
            }

            LoadPlayList();
  
        }

        private void Hardcoded()
        {
            FilteredVideos = Directory.GetFiles("Directory").Where(files => files.ToLower().EndsWith("mp4")).ToList();
            LoadPlayList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Hardcoded();
        }
        private void LoadPlayList()
        {
            MediaPlayer.currentPlaylist = MediaPlayer.newPlaylist("Playlist", "");

            foreach(string videos in FilteredVideos)
            {
                MediaPlayer.currentPlaylist.appendItem(MediaPlayer.newMedia(videos));
                ListOfVideos.Items.Add(videos);
            }

            
        }

        private void PlayFile(string url)
        {
            MediaPlayer.URL = url;
        }

        private void ShowLabel(Label name)
        {
            string file = Path.GetFileName(ListOfVideos.SelectedItem.ToString());
            name.Text = "Current Playing: " + file;
        }



        private void StateChangeEvent(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            switch (e.newState)
            {
                case 0:    // Undefined
                    duration.Text = "Undefined";
                    break;

                case 1:    // Stopped
                    duration.Text = "Stopped";
                    //MediaPlayer.fullScreen = true;
                    break;

                case 2:    // Paused
                    duration.Text = "Paused";

                    break;

                case 3:    // Playing
                    duration.Text = "Playing";
                    MediaPlayer.fullScreen = true;
                    break;

                case 4:    // ScanForward
                    duration.Text = "ScanForward";
                    //MediaPlayer.fullScreen = true;
                    break;

                case 5:    // ScanReverse
                    duration.Text = "ScanReverse";
                    break;

                case 6:    // Buffering
                    duration.Text = "Buffering";
                    break;

                case 7:    // Waiting
                    duration.Text = "Waiting";
                    break;

                case 8:    // MediaEnded
                    duration.Text = "MediaEnded";
                    if (CurrentFile >= FilteredVideos.Count-1)
                    {
                        CurrentFile = 0;
                    }
                    else
                    {
                        CurrentFile += 1;
                    }

                    ListOfVideos.SelectedIndex = CurrentFile;
                    ShowLabel(FileName);
                    break;

                case 9:    // Transitioning
                    duration.Text = "Transitioning";
                    break;

                case 10:   // Ready
                    duration.Text = "Ready";
                    //MediaPlayer.fullScreen = true;
                    timer1.Start();
                    break;

                case 11:   // Reconnecting
                    duration.Text = "Reconnecting";
                    break;

                case 12:   // Last
                    duration.Text = "Last";
                    break;

                default:
                    duration.Text = ("Unknown State: " + e.newState.ToString());
                    break;
            }
        }

        private void MediaPlayer_Enter(object sender, EventArgs e)
        {

        }

        private void PlayList(object sender, EventArgs e)
        {
            CurrentFile = ListOfVideos.SelectedIndex;
            PlayFile(ListOfVideos.SelectedItem.ToString());
            ShowLabel(FileName);
        }

    

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void TimerEvent(object sender, EventArgs e)
        {
            MediaPlayer.Ctlcontrols.play();
            timer1.Stop();
        }

        private void Exit(object sender, EventArgs e)
        {
            Application.Exit();
        }


       
    }

}
