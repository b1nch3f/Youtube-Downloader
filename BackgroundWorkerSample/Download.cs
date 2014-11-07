using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using YoutubeExtractor;

namespace YouDownloader
{
    public partial class YouDownload : Form
    {

        BackgroundWorker m_oWorker;

        public YouDownload()
        {
            InitializeComponent();
            m_oWorker = new BackgroundWorker();
            m_oWorker.DoWork += new DoWorkEventHandler(m_oWorker_DoWork);
            m_oWorker.ProgressChanged += new ProgressChangedEventHandler(m_oWorker_ProgressChanged);
            m_oWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_oWorker_RunWorkerCompleted);
            m_oWorker.WorkerReportsProgress = true;
            m_oWorker.WorkerSupportsCancellation = true;
        }

        void m_oWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //If it was cancelled midway
            if (e.Cancelled)
            {
                lblStatus.Text = "Download Cancelled.";
            }
            else if (e.Error != null)
            {
                lblStatus.Text = "Error while performing background operation.";
            }
            else
            {
                lblStatus.Text = "Download Completed...";
            }
            btnStartAsyncOperation.Enabled = true;
            btnCancel.Enabled = false;
        }

        void m_oWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Here you play with the main UI thread
            progressBar1.Value = e.ProgressPercentage;
            lblStatus.Text = "Downloading......" + progressBar1.Value.ToString() + "%";
        }

        void m_oWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Our youtube link
            string videoUrl = e.Argument as string;

            string link = videoUrl;

            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            /*
             * Select the .mp4 video with 720p resolution
             */
            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 720);

            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */
            string dekstopPath = @"C:\Users\";
            string username = System.Environment.UserName;
            string filePath = dekstopPath + username + @"\Desktop\video.mp4";

            VideoDownloader videoDownloader = new VideoDownloader(video, filePath);

            // Register the ProgressChanged event and print the current progress
            videoDownloader.DownloadProgressChanged += (sender2, args2) => m_oWorker.ReportProgress(Convert.ToInt32(args2.ProgressPercentage));

            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously, unless used backgroundworker
             */

            videoDownloader.Execute();

            //Report 100% completion on operation completed
            m_oWorker.ReportProgress(100);
        }

        private void btnStartAsyncOperation_Click(object sender, EventArgs e)
        {
            btnStartAsyncOperation.Enabled  = false;
            btnCancel.Enabled               = true;
            //Start the async operation here
            string url = inputUrl.Text;
            m_oWorker.RunWorkerAsync(url);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
