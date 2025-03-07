using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Windows.Threading;


using VideoLibrary;

namespace VideoExtractor.Views
{
    public partial class HomeView : System.Windows.Controls.UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void FileGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                HandleFile(files[0]);
            }
        }
        private void Open_Explorer(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Media files (*.mp4)|*.mp4|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            HandleFile(openFileDialog.FileName);
        }
        private void HandleFile(string file)
        {
            string fileExtension = Path.GetExtension(file);
            if (fileExtension == ".mp4")
            {
                VideoSourceUri.Text = file;
            }
            else
            {
                MessageBox.Show("Unable to use " + file + System.Environment.NewLine + "Only .mp4 files are allowed.", "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ValidFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), String.Empty);
            }

            return fileName;
        }

        public void ButtonDownloadAudio(object sender, RoutedEventArgs e)
        {
            youtubeURLMenu.Visibility = Visibility.Hidden;
            youtubeVideoDownloadMenu.Visibility = Visibility.Visible;

            BackgroundWorker worker = new()
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += DownloadAudio;
            worker.ProgressChanged += Worker_ProgressChanged;

            worker.RunWorkerAsync(urlTextBox.Text);
        }
        public void ButtonDownload(object sender, RoutedEventArgs e)
        {
            youtubeURLMenu.Visibility = Visibility.Hidden;
            youtubeVideoDownloadMenu.Visibility = Visibility.Visible;

            BackgroundWorker worker = new()
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += Download;
            worker.ProgressChanged += Worker_ProgressChanged;

            worker.RunWorkerAsync(urlTextBox.Text);
        }
        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;

            if (e.UserState != null)
            {
                string[] param = (string[])e.UserState;

                pbInfos.Text = param[0];
                if (param[1] == "t")
                {
                    pbStatus.IsIndeterminate = true;
                    pbStatusText.Visibility = Visibility.Hidden;
                }
                else
                {
                    pbStatus.IsIndeterminate = false;
                    pbStatusText.Visibility = Visibility.Visible;
                }
            }
        }

        void DownloadAudio(object sender, DoWorkEventArgs e)
        {
            try
            {
                (sender as BackgroundWorker).ReportProgress(0, new string[] { "Searching Best Audio Format...", "t" });

                var videos = YouTube.Default.GetAllVideos(e.Argument as string);

                int hightaudio = 1;

                YouTubeVideo audioItem = null;

                foreach (var item in videos)
                {
                    if (item.AdaptiveKind.ToString() == "Audio" && item.AudioBitrate > hightaudio)
                    {
                        hightaudio = item.AudioBitrate;
                        audioItem = item;
                    }
                }

                (sender as BackgroundWorker).ReportProgress(0, new string[] {"Downloading audio with bitrate "
                            + audioItem.AudioBitrate.ToString()
                            + " and size " + Math.Round((double)audioItem.ContentLength / 1000000, 2).ToString()
                            + "MB", "" });

                DownloadBest(audioItem, Directory.GetParent(Directory.GetCurrentDirectory()) + "\\Downloads\\" + ValidFileName(audioItem.Title) + ".mp3", sender);

                (sender as BackgroundWorker).ReportProgress(0, new string[] { "", "" });

                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
                {
                    youtubeURLMenu.Visibility = Visibility.Visible;
                    youtubeVideoDownloadMenu.Visibility = Visibility.Hidden;
                    MessageBox.Show("Audio downloaded at " + Directory.GetParent(Directory.GetCurrentDirectory()) + "\\Downloads\\", "Audio Download Complete!");
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
                {
                    youtubeURLMenu.Visibility = Visibility.Visible;
                    youtubeVideoDownloadMenu.Visibility = Visibility.Hidden;
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
        void Download(object sender, DoWorkEventArgs e)
        {
            try
            {
                (sender as BackgroundWorker).ReportProgress(0, new string[] { "Searching Best Audio and Video Formats...", "t" });

                var videos = YouTube.Default.GetAllVideos(e.Argument as string);
                int hightaudio = 1;
                int hightvideo = 1;

                YouTubeVideo audioItem = null;
                YouTubeVideo videoItem = null;

                foreach (var item in videos)
                {
                    if (item.AdaptiveKind.ToString() == "Audio" && item.AudioBitrate > hightaudio)
                    {
                        hightaudio = item.AudioBitrate;
                        audioItem = item;
                    }
                    if (item.Resolution > hightvideo)
                    {
                        hightvideo = item.Resolution;
                        videoItem = item;
                    }
                }

                    (sender as BackgroundWorker).ReportProgress(0, new string[] { "Downloading audio with bitrate "
                                + audioItem.AudioBitrate.ToString()
                                + " and size " + Math.Round((double)audioItem.ContentLength / 1000000, 2).ToString()
                                + "MB", "" });
                DownloadBest(audioItem, Directory.GetCurrentDirectory() + "\\youtubeAudio.mp3", sender);

                (sender as BackgroundWorker).ReportProgress(0, new string[] {"Downloading video with resolution "
                                + videoItem.Resolution.ToString()
                                + "p and size " + Math.Round((double)videoItem.ContentLength / 1000000, 2).ToString()
                                + "MB", "" });
                DownloadBest(videoItem, Directory.GetCurrentDirectory() + "\\youtubeVideo.mp4", sender);

                (sender as BackgroundWorker).ReportProgress(0, new string[] { "Merging Audio and Video...", "t" });

                Combine(ValidFileName(videoItem.Title));

                (sender as BackgroundWorker).ReportProgress(0, new string[] { "", "" });

                File.Delete(Directory.GetCurrentDirectory() + "\\youtubeAudio.mp3");
                File.Delete(Directory.GetCurrentDirectory() + "\\youtubeVideo.mp4");

                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
                {
                    youtubeURLMenu.Visibility = Visibility.Visible;
                    youtubeVideoDownloadMenu.Visibility = Visibility.Hidden;

                    VideoSourceUri.Text = Directory.GetParent(Directory.GetCurrentDirectory()) + "\\Downloads\\" + ValidFileName(videoItem.Title) + ".mp4";

                    MessageBox.Show("Video downloaded at " + Directory.GetParent(Directory.GetCurrentDirectory()) + "\\Downloads\\", "Video Download Complete!");
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
                {
                    youtubeURLMenu.Visibility = Visibility.Visible;
                    youtubeVideoDownloadMenu.Visibility = Visibility.Hidden;
                    MessageBox.Show(ex.Message, "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public void DownloadBest(YouTubeVideo y, string patch, object sender)
        {
            int total = 0;
            FileStream fs = null;
            Stream streamweb = null;
            WebResponse w_response = null;
            try
            {
                WebRequest w_request = WebRequest.Create(y.Uri);
                if (w_request != null)
                {
                    w_response = w_request.GetResponse();
                    if (w_response != null)
                    {
                        fs = new FileStream(patch, FileMode.Create);
                        byte[] buffer = new byte[128 * 1024];
                        int bytesRead = 0;
                        streamweb = w_response.GetResponseStream();

                        do
                        {
                            bytesRead = streamweb.Read(buffer, 0, buffer.Length);
                            fs.Write(buffer, 0, bytesRead);
                            total += bytesRead;
                            double percent = Math.Round((double)total / (int)y.ContentLength * 100, 2);
                            (sender as BackgroundWorker).ReportProgress((int) percent);
                        }
                        while (bytesRead > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
                {
                    MessageBox.Show(ex.Message, "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                w_response?.Close();
                fs?.Close();
                streamweb?.Close();
            }
        }

        public void Combine(string title)
        {
            Process p = new();
            p.StartInfo.FileName = "ffmpeg\\bin\\ffmpeg.exe";
            p.StartInfo.Arguments = "-i \"" + 
                Directory.GetCurrentDirectory() + "\\youtubeVideo.mp4\" -i \"" + 
                Directory.GetCurrentDirectory() + "\\youtubeAudio.mp3\" -preset veryfast  \"" +
                Directory.GetParent(Directory.GetCurrentDirectory()) + "\\Downloads\\" + title + ".mp4\"";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            p.Start();
            p.WaitForExit();
        }
    }
}