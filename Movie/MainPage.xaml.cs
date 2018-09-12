using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Timers;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Movie
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        Timer timer;
        DispatcherTimer timevideo;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new Timer();
            timer.Interval = 5000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            timevideo = new DispatcherTimer();
            timevideo.Interval = TimeSpan.FromMilliseconds(100);
            timevideo.Tick += Timevideo_Tick;
            volumeslider.Value = player.Volume * 100;
        }

        private void Timevideo_Tick(object sender, object e)
        {
            long current = player.Position.Ticks;
            videoprogress.Value = current;
            from.Text = Milliseconds_to_Minute((long)player.Position.TotalMilliseconds);
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                if ((title.Visibility == Visibility.Visible || controlpanel.Visibility == Visibility.Visible) && player.CurrentState != MediaElementState.Closed)
                {
                    title.Visibility = Visibility.Collapsed;
                    controlpanel.Visibility = Visibility.Collapsed;
                }
                timer.Enabled = false;
            });
            
        }

        private void last_Click(object sender, RoutedEventArgs e)
        {
            if (player.PlaybackRate >= 0)
            {
                player.PlaybackRate = player.PlaybackRate - 0.25;
                if (player.PlaybackRate == 1)
                {
                    playrate.Text = "";
                }
            }
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            if(player.CurrentState == MediaElementState.Playing)
            {
                (play.Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Assets/play.png"));
                player.Pause();
                timevideo.Stop();
            }
            else if(player.CurrentState == MediaElementState.Paused || player.CurrentState == MediaElementState.Stopped)
            {
                (play.Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Assets/pause.png"));
                player.Play();
                timevideo.Start();
            }
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            if (player.PlaybackRate <= 4)
            {
                player.PlaybackRate = player.PlaybackRate + 0.25;
                if (player.PlaybackRate == 1)
                {
                    playrate.Text = "";
                }
            }
        }

        private void Win_Tapped(object sender, TappedRoutedEventArgs e)
        {
            timer.Enabled = true;
            if (title.Visibility == Visibility.Collapsed || controlpanel.Visibility == Visibility.Collapsed)
            {
                title.Visibility = Visibility.Visible;
                controlpanel.Visibility = Visibility.Visible;
            }
            else
            {
                title.Visibility = Visibility.Collapsed;
                controlpanel.Visibility = Visibility.Collapsed;
            }
        }


        private void fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
            else
            {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            }
        }

        private void CheckPlayer(string flag)
        {
            if (flag == "P")
            {
                if (player.CurrentState == MediaElementState.Playing)
                {
                    (play.Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Assets/play.png"));
                    player.Pause();
                    timevideo.Stop();
                }
            }
            else if (flag == "S")
            {
                if (player.CurrentState == MediaElementState.Playing || player.CurrentState == MediaElementState.Paused)
                {
                    (play.Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Assets/play.png"));
                    player.Stop();
                    timevideo.Stop();
                }
            }
        }

        private async void Fromfile_Click(object sender, RoutedEventArgs e)
        {
            CheckPlayer("P");
            var picker = new FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".flv");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add("*");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                filename.Text = file.Name;
                var f = await file.OpenAsync(FileAccessMode.Read);
                player.SetSource(f, file.ContentType);
            }
        }

        private async void Fromnet_Click(object sender, RoutedEventArgs e)
        {
            CheckPlayer("P");
            var condia = new ContentDialog();
            var text = new TextBox();
            text.Height = 32;
            text.AcceptsReturn = false;
            condia.Title = "Input Url";
            condia.Content = text;
            condia.PrimaryButtonText = "OK";
            condia.SecondaryButtonText = "Cancel";
            if(await condia.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    player.Source = new Uri(text.Text);
                    loadvideo.IsActive = true;
                }
                catch
                {
                    if (text.Text.Trim() == "")
                    {
                        await new MessageDialog("请输入网址!").ShowAsync();
                    }
                    else
                    {
                        await new MessageDialog("未知错误!请稍后重试").ShowAsync();
                    }
                }
            }
            else
            {
                loadvideo.IsActive = false;
                return;
            }
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            (play.Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Assets/pause.png"));
            videoprogress.IsEnabled = true;
            videoprogress.Maximum = player.NaturalDuration.TimeSpan.Ticks;
            total.Text = Milliseconds_to_Minute((long)player.NaturalDuration.TimeSpan.TotalMilliseconds);
            loadvideo.IsActive = false;
            timevideo.Start();
        }

        private void videoprogress_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            player.Position = TimeSpan.FromTicks((long)videoprogress.Value);
        }


        private void volumeslider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            player.Volume = volumeslider.Value / 100;
        }

        private void repeat_Click(object sender, RoutedEventArgs e)
        {
            if (player.IsLooping)
            {
                repeat.Text = "开启重复播放";
                player.IsLooping = false;
            }
            else
            {
                repeat.Text = "关闭重复播放";
                player.IsLooping = true;
            }
        }

        private void autoplay_Click(object sender, RoutedEventArgs e)
        {
            if(player.AutoPlay)
            {
                autoplay.Text = "关闭自动播放";
                player.AutoPlay = false;
            }
            else
            {
                autoplay.Text = "开启自动播放";
                player.AutoPlay = true;
            }
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            (play.Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Assets/play.png"));
        }

        private string Milliseconds_to_Minute(long milliseconds)
        {
            int minute = (int)TimeSpan.FromMilliseconds(milliseconds).TotalMinutes;
            int seconds = (int)TimeSpan.FromMilliseconds(milliseconds).TotalSeconds;
            int hours = minute / 60;
            if (hours > 0)
            {
                return (hours + " : " + (minute - 60 * hours) + " : " + (seconds - minute * 60));
            }
            else
            {
                return (minute + " : " + (seconds - minute * 60));
            }
        }

        private void player_RateChanged(object sender, RateChangedRoutedEventArgs e)
        {
            if(player.PlaybackRate != player.DefaultPlaybackRate)
            {
                playrate.Text = player.PlaybackRate.ToString();
            }
        }

        private void controlpanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            timer.Enabled = false;
        }

        private void controlpanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            timer.Enabled = true;
        }

    }
}
