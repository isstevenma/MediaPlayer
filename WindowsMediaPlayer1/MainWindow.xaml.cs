using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace WindowsMediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer timer = new DispatcherTimer();

        DispatcherTimer fullscreenMouseTimer = new DispatcherTimer();

        bool isPosSliderDrag = false;
        String trackPath = "";
        bool fullscreen = false;
        bool newMedia = false;
        bool pause = false;




        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += new EventHandler(TimerTick);
        

            fullscreenMouseTimer.Interval = new TimeSpan(0, 0, 0, 2);
            fullscreenMouseTimer.Tick += new EventHandler(fullscreenMouseTimerTick);
            fullscreenMouseTimer.Stop();

            PositionSlider.IsEnabled = false;


        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (!isPosSliderDrag)
            {
                PositionSlider.Value = MediaEle.Position.TotalMilliseconds;
                getDuration();
                getCurrentPosition();
            }
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            PauseAndUnpause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            MediaEle.Stop();
            timer.Stop();
        }



        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaEle.Volume = VolumeSlider.Value;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaEle.SpeedRatio = SpeedSlider.Value;
        }


        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaEle.Position = TimeSpan.FromMilliseconds(PositionSlider.Value);
        }

        private void MediaEle_MediaOpened(object sender, RoutedEventArgs e)
        {
            PositionSlider.Maximum = MediaEle.NaturalDuration.TimeSpan.TotalMilliseconds;
            SpeedSlider.Value = 1;
        }


        private void PositionSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isPosSliderDrag = true;
            MediaEle.Stop();

        }

        private void PositionSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isPosSliderDrag = false;
            MediaEle.Play();

        }

        private void OpenFile_Click(Object sender, RoutedEventArgs e)
        {
            Nullable<bool> result;
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.FileName = "";
            fileDialog.DefaultExt = ".mp3";
            fileDialog.Filter = ".mp3|*.mp3|.mp4|*.mp4|.mpg|*.mpg|.wmv|*.wmv|All files (*.*)|*.*";
            fileDialog.CheckFileExists = true;
            result = fileDialog.ShowDialog();
            if (result == true)
            {
                Playlist.Items.Clear();
                Playlist.Visibility = Visibility.Hidden;
                trackPath = fileDialog.FileName;
                newMedia = true;

                PlayTrack();
                getDuration();
            }

        }

        private void OpenFolder_Click(Object sender, RoutedEventArgs e)
        {
            String folderPath = "";
            string[] files;
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = fbd.SelectedPath;
            }
            if (folderPath != "")
            {
                Playlist.Items.Clear();
                Playlist.Visibility = Visibility.Visible;
                files = Directory.GetFiles(folderPath, "*.mp3");
                foreach (String fileName in files)
                {
                    Playlist.Items.Add(fileName);
                }
                Playlist.SelectedIndex = 0;
            }

            newMedia = true;

        }

        private void PlayTrack()
        {
            bool check = true;
            FileInfo fileInfo = null;
            Uri source;
            try
            {
                fileInfo = new FileInfo(trackPath);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                check = false;
            }
            if (check)
            {
                if (!fileInfo.Exists)
                {
                    System.Windows.MessageBox.Show("Cannot find " + trackPath);
                }
                else
                {
                    source = new Uri(trackPath);
                    PositionSlider.IsEnabled = true;
                    MediaEle.Source = source;
                    MediaEle.SpeedRatio = SpeedSlider.Value;
                    MediaEle.Volume = VolumeSlider.Value;
                    MediaEle.Play();
                    timer.Start();

                    
                    

                   
                    

                }
            }
        }

        private void PlayPlaylist()
        {
            int selectedItemIndex = -1;
            if (!Playlist.Items.IsEmpty)
            {
                selectedItemIndex = Playlist.SelectedIndex;
                if (selectedItemIndex > -1)
                {
                    trackPath = Playlist.Items[selectedItemIndex].ToString();
                    PlayTrack();
                    getDuration();


                }

            }
        }

        private void MediaEle_MediaEnded(object sender, RoutedEventArgs e)
        {
            int nextTrackIndex = -1;
            int numberOfTracks = -1;
            MediaEle.Stop();
            numberOfTracks = Playlist.Items.Count;
            if (numberOfTracks > 0)
            {
                nextTrackIndex = Playlist.SelectedIndex + 1;
                if (nextTrackIndex >= numberOfTracks)
                {
                    nextTrackIndex = 0;
                }
                Playlist.SelectedIndex = nextTrackIndex;
                PlayPlaylist();
                
            }
        }

        private void MediaEle_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Unable to play " + trackPath + " [" + e.ErrorException.Message + "]");
        }

        private void MediaEle_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] filePaths = e.Data.GetData(System.Windows.DataFormats.FileDrop) as String[];
            if (filePaths.Length > 1)
            {
                foreach (String s in filePaths)
                {
                    if (isAudioTrack(s))
                    {
                        Playlist.Items.Add(s);
                    }
                }




                Playlist.Visibility = Visibility.Visible;
                Playlist.SelectedIndex = 0;
                trackPath = filePaths[0];
                MediaEle.Source = new Uri(trackPath);
                newMedia = true;

            }
            else
            {
                if (isMediaFile(filePaths[0]))
                {
                    if (MediaEle.IsLoaded)
                    {
                        MediaEle.Stop();
                    }



                    trackPath = filePaths[0];
                    MediaEle.Source = new Uri(trackPath);
                    newMedia = true;
                }
            }

        }

        private bool isAudioTrack(String track)
        {
            return (track.EndsWith(".mp3"));
        }

        private bool isMediaFile(String track)
        {
            string[] mediaMarkers = { ".mp3", ".mp4", ".wmv", ".avi", ".flv" };
            bool checker = false;
            foreach (String s in mediaMarkers)
            {
                if (track.EndsWith(s))
                {
                    checker = true;
                }
            }
            return checker;
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (!fullscreen)
            {
                Menu1.Visibility = Visibility.Collapsed;
                Menu.Visibility = Visibility.Collapsed;
                ToolsGrid.Visibility = Visibility.Collapsed;








                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;





                fullscreen = true;




            }
            else
            {
                ToolsGrid.Visibility = Visibility.Visible;



                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;





                fullscreen = false;



            }


        }



        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            PositionSlider.Value += 5000;
            MediaEle.Position = TimeSpan.FromMilliseconds(PositionSlider.Value);
        }



        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (fullscreen)
            {
                fullscreenMouseTimer.Start();
                ToolsGrid.Visibility = Visibility.Visible;
            }



        }

        private void fullscreenMouseTimerTick(object sender, EventArgs e)
        {

            if (fullscreen)
            {

                ToolsGrid.Visibility = Visibility.Collapsed;
                fullscreenMouseTimer.Stop();
            }
        }

        private void getDuration()
        {

            if (MediaEle.NaturalDuration.HasTimeSpan)
            {
                TimeSpan totalDuration = MediaEle.NaturalDuration.TimeSpan;


                String totalLengthOfMedia;


                totalLengthOfMedia = getGivenPosition(totalDuration);

                DurationLabel.Content = totalLengthOfMedia;

                getCurrentPosition();
            }




        }



        private void getCurrentPosition()
        {
            TimeSpan currentDuration = MediaEle.Position;

            String currentPosition;

            currentPosition = getGivenPosition(currentDuration);

            PositionLabel.Content = currentPosition;



        }

        private string getGivenPosition(TimeSpan timeSpan)

        {
            String lengthOfGivenTimeSpan;

            if (timeSpan.TotalMinutes < 1.0)
            {
                lengthOfGivenTimeSpan = String.Format("0:{0}", timeSpan.Seconds);
            }

            else if (timeSpan.TotalHours < 1.0)
            {
                lengthOfGivenTimeSpan = String.Format("{0}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            }

            else
            {
                lengthOfGivenTimeSpan = String.Format("{0}:{1:D2}:{2:D2}", timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
            }

            return lengthOfGivenTimeSpan;

        }

        private void PrimaryWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == (Key.Space))
            {
                PauseAndUnpause();


            }

            else if (e.Key == (Key.Right))
            {
                e.Handled = true;
                PositionSlider.Value += 5000;
                MediaEle.Position = TimeSpan.FromMilliseconds(PositionSlider.Value);
            }

            else if (e.Key == (Key.Left))
            {

                e.Handled = true;
                PositionSlider.Value -= 5000;
                MediaEle.Position = TimeSpan.FromMilliseconds(PositionSlider.Value);

            }


        }

        private void PauseAndUnpause()
        {
            if (!pause && newMedia)
            {
                if (!Playlist.Items.IsEmpty)
                {
                    PlayPlaylist();
                    
                }
                else
                {
                    PlayTrack();
                    getDuration();
                }

                newMedia = false;
            }

            else if (!pause)
            {
                pause = true;
                MediaEle.Pause();
                timer.Stop();

            }


            else
            {
                pause = false;
                MediaEle.Play();
                timer.Start();
            }
        }

        private void BackwardButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            PositionSlider.Value -= 5000;
            MediaEle.Position = TimeSpan.FromMilliseconds(PositionSlider.Value);
        }
    }



    }
    

