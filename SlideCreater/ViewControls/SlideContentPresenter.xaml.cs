﻿using CommonGraphics;

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace SlideCreater
{

    public delegate void SlideClickedEvent(object sender, RenderedSlide slide);
    /// <summary>
    /// Interaction logic for SlideContentPresenter.xaml
    /// </summary>
    public partial class SlideContentPresenter : UserControl
    {

        public RenderedSlide Slide { get; set; }

        public SlideContentPresenter()
        {
            InitializeComponent();
        }

        private bool keydisplay = false;
        public void ShowSlide(bool showkey)
        {
            keydisplay = showkey;
            background.Visibility = Visibility.Hidden;
            if (showkey)
            {
                if (Slide?.MediaType == MediaType.Video_KeyedVideo)
                {
                    ImgDisplay.Visibility = Visibility.Hidden;
                    textDisplay.Visibility = Visibility.Hidden;
                    VideoDisplay.Visibility = Visibility.Visible;
                    VideoDisplay.Source = new Uri(Slide.KeyAssetPath);
                    VideoDisplay.MediaEnded += VideoDisplay_MediaEnded;
                    VideoDisplay.Volume = 0;
                    VideoDisplay.Play();
                }
                else if (Slide.KeyBitmap != null)
                {
                    VideoDisplay.Visibility = Visibility.Hidden;
                    textDisplay.Visibility = Visibility.Hidden;
                    ImgDisplay.Visibility = Visibility.Visible;
                    //ImgDisplay.Source = Slide.KeyBitmap.ToBitmapImage();
                    ImgDisplay.Source = Slide.KeyPNGMS?.ToBitmapImage();
                }


                if (Slide.MediaType == MediaType.Text)
                {
                    VideoDisplay.Visibility = Visibility.Hidden;
                    textDisplay.Text = "NO KEY";
                    // handle action slides being auto loaded with black keys
                    if (Slide.RenderedAs == "Action")
                    {
                        background.Fill = Brushes.Black;
                        background.Visibility = Visibility.Visible;
                        textDisplay.Text = "AUTO BLACK KEY\r\n(for action)";

                        if (Regex.Match(Slide.Text, "!keysrc='.*\\.png';").Success)
                        {
                            background.Visibility = Visibility.Hidden;
                            textDisplay.Visibility = Visibility.Hidden;
                            ImgDisplay.Visibility = Visibility.Visible;
                            ImgDisplay.Source = Slide.KeyPNGMS?.ToBitmapImage();
                        }
                    }
                    else
                    {
                        textDisplay.Visibility = Visibility.Visible;
                        ImgDisplay.Visibility = Visibility.Hidden;
                    }
                }
            }
            else
            {
                ImgDisplay.Source = null;
                VideoDisplay.Source = null;
                if (Slide?.MediaType == MediaType.Image)
                {
                    VideoDisplay.Visibility = Visibility.Hidden;
                    textDisplay.Visibility = Visibility.Hidden;
                    ImgDisplay.Visibility = Visibility.Visible;
                    ImgDisplay.Source = Slide.BitmapPNGMS?.ToBitmapImage();
                }
                if (Slide?.MediaType == MediaType.Video || Slide?.MediaType == MediaType.Video_KeyedVideo)
                {
                    ImgDisplay.Visibility = Visibility.Hidden;
                    textDisplay.Visibility = Visibility.Hidden;
                    VideoDisplay.Visibility = Visibility.Visible;
                    VideoDisplay.Source = new Uri(Slide.AssetPath);
                    VideoDisplay.MediaEnded += VideoDisplay_MediaEnded;
                    VideoDisplay.Volume = 0;
                    VideoDisplay.Play();
                }
                if (Slide?.MediaType == MediaType.Audio)
                {
                    VideoDisplay.Visibility = Visibility.Hidden;
                    textDisplay.Visibility = Visibility.Visible;
                    ImgDisplay.Visibility = Visibility.Visible;
                    ImgDisplay.Source = new BitmapImage(new Uri("pack://application:,,,/ViewControls/Images/musicnote.png"));
                    textDisplay.Text = Slide.Name + Slide.CopyExtension;
                }
                if (Slide?.MediaType == MediaType.Text)
                {
                    VideoDisplay.Visibility = Visibility.Hidden;
                    ImgDisplay.Visibility = Visibility.Hidden;
                    textDisplay.Visibility = Visibility.Hidden;
                    //textDisplay.Visibility = Visibility.Visible;
                    //textDisplay.Text = Slide.Text;

                    if (Slide.RenderedAs == "Action")
                    {
                        background.Fill = Brushes.Black;
                        background.Visibility = Visibility.Visible;

                        if (Regex.Match(Slide.Text, "!displaysrc='.*\\.png';").Success)
                        {
                            background.Visibility = Visibility.Hidden;
                            ImgDisplay.Visibility = Visibility.Visible;
                            ImgDisplay.Source = Slide.BitmapPNGMS?.ToBitmapImage();
                        }
                        else if (Regex.Match(Slide.Text, "!displaysrc='.*\\.mp4';").Success)
                        {
                            ImgDisplay.Visibility = Visibility.Hidden;
                            textDisplay.Visibility = Visibility.Hidden;
                            VideoDisplay.Visibility = Visibility.Visible;
                            VideoDisplay.Source = new Uri(Slide.AssetPath);
                            VideoDisplay.MediaEnded += VideoDisplay_MediaEnded;
                            VideoDisplay.Volume = 0;
                            VideoDisplay.Play();
                        }
                    }

                }
            }
        }

        private void VideoDisplay_MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoDisplay.Stop();
            VideoDisplay.Volume = 0;
            VideoDisplay.Play();
        }

        public void PlaySlide()
        {
            if (Slide?.MediaType == MediaType.Video && !keydisplay)
            {
                VideoDisplay.Play();
            }
        }

        public void Clear()
        {
            keydisplay = false;
            Slide = null;
            textDisplay.Text = "Empty Slide";
            textDisplay.Visibility = Visibility.Visible;
            background.Visibility = Visibility.Hidden;
            ImgDisplay.Visibility = Visibility.Hidden;
            VideoDisplay.Visibility = Visibility.Hidden;
            ShowSlide(keydisplay);
        }

    }
}
