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
using System.Configuration;
using System.Reflection;
using Newtonsoft.Json;
using static WebBrowser_HTML_File_CS.VSVshopHelper;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Net.Http;

namespace WebBrowser_HTML_File_CS
{
    public partial class Form1 : Form
    {
        List<string> FilteredVideos = new List<string>();
        FolderBrowserDialog br = new FolderBrowserDialog();
        int CurrentFile = 0;
        private readonly pubsEntities2 pubsEntities = new pubsEntities2();
        HttpClient httpClient = new HttpClient();
        Random rnd = new Random();
        List<VSVshopHelper.VSVShopVideos> VideosList = new List<VSVshopHelper.VSVShopVideos>();

        List<int> PlayedEntertaimentVideosList = new List<int>();
        List<int> PlayedIntrosVideosList = new List<int>();

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadDirectory(object sender, EventArgs e)
        {
            MediaPlayer.Ctlcontrols.stop();
            if (FilteredVideos.Count > 1)
            {
                FilteredVideos.Clear();
                FilteredVideos = null;

                ListOfVideos.Items.Clear();
                CurrentFile = 0;
            }

            DialogResult result = br.ShowDialog();
            if (result == DialogResult.OK)
            {
                FilteredVideos = Directory.GetFiles(br.SelectedPath, "*.*").Where(files => files.ToLower().EndsWith("mp4")).ToList();
            }

            LoadPlayList();

        }
        public static string userid = "";
        public static int AccountId = 0;
        private void Hardcoded()
        {
            FilteredVideos = Directory.GetFiles("C:/Users/Dev/Videos/Video").Where(files => files.ToLower().EndsWith("mp4")).ToList();
            LoadPlayList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string FilePath = ConfigurationManager.AppSettings["LogPath"];
            //string fileName = @"C:\Temp\SmSpVimeo\Vimeo\dontDelete3.txt";
            var uid = "";
            uid = File.ReadAllText(FilePath);
            char[] separators = new char[] { ' ', '$' };

            string[] subs = uid.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            userid = subs[0];
            var accid = subs[1].ToString();
            accid = accid.Substring(0, accid.Length - 2);
            AccountId = Int16.Parse(accid);
            Hardcoded();
        }
        private void LoadPlayList()
        {
            MediaPlayer.currentPlaylist = MediaPlayer.newPlaylist("Playlist", "");

            foreach (string videos in FilteredVideos)
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

        public int GetRandomNumber(int maxLength)
        {
            Random rnd = new Random();
            int random = rnd.Next(maxLength - 1);
            return random;
        }
        public int GetRandomNumber(int minValue, int maxLength)
        {
            Random rnd = new Random();
            int random = rnd.Next(minValue, maxLength - 1);
            return random;
        }
        public string GetVimeoVideoId(string url)
        {
            string vimeoVideoRegex = "(.*)(.com/)(.*)";
            string resp = Regex.Replace(url, vimeoVideoRegex, "$3");
            return resp.Replace("/", "?h=");
            //return resp;
        }

        public string SavePlaysVideos(List<VSVShopVideos> VideosList)// public string SavePlaysVideos( [FromBody]List<VSVShopVideos> VideosList)
        {
            try
            {
                foreach (var resp in VideosList)
                {
                    if (resp.VSVAccountID != null)
                    {
                        var vsvPlayObj = new VSVPlay
                        {
                            VSVAccountID = (int)resp.VSVAccountID,
                            VidID = (int)resp.VidId,
                            PlayTimeStamp = resp.PlayTimeStamp,
                            VSVLocationID = 0,
                        };
                        pubsEntities.VSVPlays.Add(vsvPlayObj);
                        pubsEntities.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return "";
        }

        [HttpPost]
        public string GetEntertainmentVideos(int loopIndex)
        {
            VideosList = new List<VSVShopVideos>();
            int accountId = AccountId;
            try
            {
                // VsvShop Entertainment video //
                var countEntertainment = pubsEntities.VSVEntertainmentVideos.Count();
                if (countEntertainment > 0)
                {
                    var VsventertainmentVideo = pubsEntities.VSVEntertainmentVideos.ToList().ElementAt(GetRandomNumber(countEntertainment));
                    if (VsventertainmentVideo != null)
                    {
                        var VsventertainmentVideosObj = new VSVshopHelper.VSVShopVideos
                        {
                            Url = VsventertainmentVideo.URL,
                            VideoId = GetVimeoVideoId(VsventertainmentVideo.URL)
                        };
                        VideosList.Add(VsventertainmentVideosObj);
                    }
                }
                // VsvShop Intro video //
                var countIntros = pubsEntities.VSVShopIntroes.Where(x => x.VSVAccountID == accountId).Count();
                if (countIntros > 0)
                {
                    int respRandomNumIntro = GetRandomNumber(countIntros);
                    var VsvshopIntro = pubsEntities.VSVShopIntroes.Where(x => x.VSVAccountID == accountId).ToList().ElementAt(respRandomNumIntro);
                    if (VsvshopIntro != null)
                    {
                        var VsvshopIntrosvideoId = GetVimeoVideoId(VsvshopIntro.IntroURL);
                        var VsvshopIntrosObj = new VSVshopHelper.VSVShopVideos
                        {
                            Url = VsvshopIntro.IntroURL,
                            VideoId = VsvshopIntrosvideoId
                        };
                        VideosList.Add(VsvshopIntrosObj);
                        PlayedIntrosVideosList.Add(respRandomNumIntro);
                    }
                }
                // VsvShop prod video //
                var countProdvideo = pubsEntities.VSVaccountprodvideos.Where(x => x.VSVAccountID == accountId).Count();
                if (countProdvideo > 0)
                {
                    int respRandomNumProdFirst = GetRandomNumber(1, countProdvideo / 3);
                    var prodVideo1 = GetProdVideos(accountId, respRandomNumProdFirst);
                    if (prodVideo1 != null)
                        VideosList.Add(prodVideo1);
                    int respRandomNumProdSecond = GetRandomNumber((countProdvideo / 3) + 1, countProdvideo / 2);
                    var prodVideo2 = GetProdVideos(accountId, respRandomNumProdSecond);
                    if (prodVideo2 != null)
                        VideosList.Add(prodVideo2);
                    int respRandomNumProdThird = GetRandomNumber((countProdvideo / 2) + 1, countProdvideo);
                    var prodVideo3 = GetProdVideos(accountId, respRandomNumProdThird);
                    if (prodVideo3 != null)
                        VideosList.Add(prodVideo3);
                }
                // VsvShop outro video //
                var countOutros = pubsEntities.VSVShopOutroes.Where(x => x.VSVAccountID == accountId).Count();
                if (countOutros > 0)
                {
                    int respRandomNumOutro = GetRandomNumber(countOutros);
                    var VsvshopOutros = pubsEntities.VSVShopOutroes.Where(x => x.VSVAccountID == accountId).ToList().ElementAt(respRandomNumOutro);
                    if (VsvshopOutros != null)
                    {
                        var VsvshopOutrosObj = new VSVshopHelper.VSVShopVideos
                        {
                            Url = VsvshopOutros.OutroURL,
                            VideoId = GetVimeoVideoId(VsvshopOutros.OutroURL)
                        };
                        VideosList.Add(VsvshopOutrosObj);
                    }
                }
                var objDTO = new VSVshopHelper.VSVShopVideosDTO
                {
                    VideosLists = VideosList,
                    LoopEnded = false
                };

                return JsonConvert.SerializeObject(objDTO);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public VSVshopHelper.VSVShopVideos GetProdVideos(int accountIdd, int randomNum)
        {

            var Vsvaccountprodvideos = pubsEntities.VSVaccountprodvideos.Where(x => x.VSVAccountID == accountIdd).ToList().ElementAt(randomNum);
            if (Vsvaccountprodvideos != null)
            {
                var VsvaccountprodvideosvideoId = GetVimeoVideoId(Vsvaccountprodvideos.URL);
                var VsvaccountprodvideosObj = new VSVshopHelper.VSVShopVideos
                {
                    VidId = Vsvaccountprodvideos.VidID,
                    VSVAccountID = Vsvaccountprodvideos.VSVAccountID,
                    PlayTimeStamp = DateTime.Now,
                    Url = Vsvaccountprodvideos.URL,
                    VideoId = VsvaccountprodvideosvideoId
                };
                return VsvaccountprodvideosObj;
            }
            return null;
        }
        public string getVideoIds()
        {
            var countEntertainment = pubsEntities.VSVEntertainmentVideos.ToList();
            //var constring = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=dbVimeo;User ID=sa;Password=sa";
            var constring = ConfigurationManager.ConnectionStrings["vimeocs"].ConnectionString;
            SqlConnection con = new SqlConnection(constring);
            SqlCommand selectvideoids = new SqlCommand("Select URL from VSVProducts", con);
            DataTable dt = new DataTable();
            con.Open();
            SqlDataReader selectedVids = selectvideoids.ExecuteReader();
            dt.Load(selectedVids);
            con.Close();
            var ids = new List<string>();
            //var ids = new object();
            foreach (DataRow row in dt.Rows)
            {
                ids.Add(Convert.ToString(row["URL"]));
            }
            List<string> videoids = ids.ConvertAll<string>(x => x.ToString());
            //String.Join(", ", videoids);
            var idssss = String.Join(", ", videoids);
            //Console.WriteLine(String.Join(", ", videoids));
            //var videoids = dt.Rows;
            //string[] videoids = new string[] { "590305554", "36319428", "95212995", "67423133", "131484417" };
            //int[] videoids = { 590305554, 36319428, 95212995, 67423133, 131484417 };
            //string test = "1323";
            return idssss;
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
                    if (CurrentFile >= FilteredVideos.Count - 1)
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
