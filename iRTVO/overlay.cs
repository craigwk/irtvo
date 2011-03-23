﻿/*
 * overlay.cs
 * 
 * The actual labels and canvases that are shown on the overlay are defined here.
 * 
 * overlayUpdate() updates the data on the overlay.
 * 
 * floatTime2string() converts float seconds to more familiar minute:second form.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
// additional
using System.Diagnostics;
using System.IO;

namespace iRTVO
{
    public partial class Overlay : Window
    {

        private Dictionary<System.Windows.Visibility, Boolean> visibility2boolean = new Dictionary<System.Windows.Visibility, Boolean>(){
            {System.Windows.Visibility.Visible, true},
            {System.Windows.Visibility.Hidden, false},
            {System.Windows.Visibility.Collapsed, false}
        };

        private Dictionary<Boolean, System.Windows.Visibility> boolean2visibility = new Dictionary<Boolean, System.Windows.Visibility>(){
            {true, System.Windows.Visibility.Visible},
            {false, System.Windows.Visibility.Hidden}
        };

        // fps counter
        Stopwatch stopwatch = Stopwatch.StartNew();
        DateTime drawBegun = DateTime.Now;

        // start lights
        TimeSpan timer;

        public void showReplayScreen()
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (SharedData.theme.images[i].replay == true)
                    images[i].Visibility = boolean2visibility[SharedData.theme.images[i].visible];
            }
        }

        private void overlayUpdate(object sender, EventArgs e)
        {

            if (SharedData.refreshTheme == true)
            {
                loadTheme(Properties.Settings.Default.theme);

                overlay.Left = Properties.Settings.Default.OverlayLocationX;
                overlay.Top = Properties.Settings.Default.OverlayLocationY;
                overlay.Width = Properties.Settings.Default.OverlayWidth;
                overlay.Height = Properties.Settings.Default.OverlayHeight;

                resizeOverlay(overlay.Width, overlay.Height);
                SharedData.refreshTheme = false;
            }

            if (SharedData.runOverlay)
            {

                // wait
                SharedData.driversMutex.WaitOne(updateMs);
                SharedData.standingMutex.WaitOne(updateMs);
                SharedData.sessionsMutex.WaitOne(updateMs);

                // fps counter
                stopwatch.Restart();
                SharedData.overlayFPSstack.Push((float)(DateTime.Now - drawBegun).TotalMilliseconds);
                drawBegun = DateTime.Now;

                // do we allow retirement
                SharedData.allowRetire = true;

                if (SharedData.sessions[SharedData.overlaySession].state == iRacingTelem.eSessionState.kSessionStateRacing &&
                        (SharedData.sessions[SharedData.overlaySession].lapsRemaining > 0 && 
                        (SharedData.sessions[SharedData.overlaySession].laps - SharedData.sessions[SharedData.overlaySession].lapsRemaining) > 1)
                    )
                {
                    SharedData.allowRetire = true;
                }
                else
                {
                    SharedData.allowRetire = false;
                }

                // images
                for (int i = 0; i < images.Length; i++)
                {
                    if (SharedData.theme.images[i].visible != visibility2boolean[images[i].Visibility] || SharedData.theme.images[i].dynamic == true)
                    {
                        if (SharedData.theme.images[i].dynamic == true)
                            loadImage(images[i], SharedData.theme.images[i]);
                        
                        images[i].Visibility = boolean2visibility[SharedData.theme.images[i].visible];
                        
                    }
                }

                // objects
                for (int i = 0; i < SharedData.theme.objects.Length; i++)
                {
                    if (SharedData.theme.objects[i].visible != visibility2boolean[objects[i].Visibility])
                        objects[i].Visibility = boolean2visibility[SharedData.theme.objects[i].visible];

                    if (objects[i].Visibility == System.Windows.Visibility.Visible)
                    {
                        switch (SharedData.theme.objects[i].dataset)
                        {
                            case Theme.dataset.standing:
                                for (int j = 0; j < SharedData.theme.objects[i].labels.Length; j++) // items
                                {
                                    for (int k = 0; k < SharedData.theme.objects[i].itemCount; k++) // drivers
                                    {
                                        if (SharedData.theme.objects[i].itemCount * (SharedData.theme.objects[i].page + 1) >= SharedData.standing[SharedData.overlaySession].Length)
                                        {
                                            SharedData.lastPage[i] = true;
                                        }

                                        if ((k + (SharedData.theme.objects[i].itemCount * SharedData.theme.objects[i].page)) < SharedData.standing[SharedData.overlaySession].Length)
                                        {
                                            labels[i][(j * SharedData.theme.objects[i].itemCount) + k].Content = SharedData.theme.formatFollowedText(
                                                SharedData.theme.objects[i].labels[j],
                                                SharedData.standing[SharedData.overlaySession][k + (SharedData.theme.objects[i].itemCount * SharedData.theme.objects[i].page)].id,
                                                SharedData.overlaySession);
                                        }
                                        else
                                        {
                                            labels[i][(j * SharedData.theme.objects[i].itemCount) + k].Content = null;
                                        }
                                    }
                                }
                                break;
                            case Theme.dataset.sessionstate:
                                for (int j = 0; j < SharedData.theme.objects[i].labels.Length; j++)
                                {
                                    labels[i][j].Content = SharedData.theme.formatSessionstateText(
                                         SharedData.theme.objects[i].labels[j],
                                         SharedData.overlaySession);
                                }
                                break;
                            default:
                            case Theme.dataset.followed:
                                for (int j = 0; j < SharedData.theme.objects[i].labels.Length; j++)
                                {
                                    labels[i][j].Content = SharedData.theme.formatFollowedText(
                                        SharedData.theme.objects[i].labels[j],
                                        SharedData.sessions[SharedData.overlaySession].driverFollowed,
                                        SharedData.overlaySession);
                                }
                                break;
                        }
                    }
                    else
                    {
                        SharedData.theme.objects[i].page = -1;
                        SharedData.lastPage[i] = false;
                    }
                }

                // tickers
                for (int i = 0; i < SharedData.theme.tickers.Length; i++)
                {
                    if (SharedData.theme.tickers[i].visible != visibility2boolean[tickers[i].Visibility])
                        tickers[i].Visibility = boolean2visibility[SharedData.theme.tickers[i].visible];

                    if (tickers[i].Visibility == System.Windows.Visibility.Visible)
                    {
                        switch (SharedData.theme.tickers[i].dataset)
                        {
                            case Theme.dataset.standing:
                                if (tickerStackpanels[i].Margin.Left + tickerStackpanels[i].ActualWidth <= 0)
                                {
                                    //tickerScrolls[i].Children.Clear();
                                    tickerStackpanels[i].Children.Clear();

                                    tickerStackpanels[i] = new StackPanel();
                                    tickerStackpanels[i].Margin = new Thickness(SharedData.theme.tickers[i].width, 0, 0, 0);
                                    tickerStackpanels[i].Orientation = Orientation.Horizontal;

                                    if (SharedData.theme.tickers[i].fillVertical)
                                        tickerRowpanels[i] = new StackPanel[SharedData.standing[SharedData.overlaySession].Length];

                                    tickers[i].Children.Add(tickerStackpanels[i]);
                                    //tickerScrolls[i].Children.Add(tickerStackpanels[i]);
                                    tickerLabels[i] = new Label[SharedData.standing[SharedData.overlaySession].Length * SharedData.theme.tickers[i].labels.Length];

                                    for (int j = 0; j < SharedData.standing[SharedData.overlaySession].Length; j++) // drivers
                                    {
                                        if (SharedData.theme.tickers[i].fillVertical)
                                        {
                                            tickerRowpanels[i][j] = new StackPanel();
                                            tickerStackpanels[i].Children.Add(tickerRowpanels[i][j]);
                                        }

                                        for (int k = 0; k < SharedData.theme.tickers[i].labels.Length; k++) // labels
                                        {
                                            tickerLabels[i][(j * SharedData.theme.tickers[i].labels.Length) + k] = DrawLabel(SharedData.theme.tickers[i].labels[k]);
                                            tickerLabels[i][(j * SharedData.theme.tickers[i].labels.Length) + k].Content = SharedData.theme.formatFollowedText(
                                                SharedData.theme.tickers[i].labels[k],
                                                SharedData.standing[SharedData.overlaySession][j].id,
                                                SharedData.overlaySession);
                                            if(SharedData.theme.tickers[i].labels[k].width == 0)
                                                tickerLabels[i][(j * SharedData.theme.tickers[i].labels.Length) + k].Width = Double.NaN;

                                            if (SharedData.theme.tickers[i].fillVertical)
                                                tickerRowpanels[i][j].Children.Add(tickerLabels[i][(j * SharedData.theme.tickers[i].labels.Length) + k]);
                                            else
                                                tickerStackpanels[i].Children.Add(tickerLabels[i][(j * SharedData.theme.tickers[i].labels.Length) + k]);
                                        }
                                    }

                                    /*
                                    if(this.FindName("tickerScroll" + i) == null)
                                        this.RegisterName("tickerScroll" + i, tickerStackpanels[i]);

                                    Storyboard.SetTargetName(tickerAnimations[i], "tickerScroll" + i);
                                    Storyboard.SetTargetProperty(tickerAnimations[i], new PropertyPath(StackPanel.MarginProperty));
                                    tickerAnimations[i].From = new Thickness(SharedData.theme.tickers[i].width + tickerStackpanels[i].ActualWidth, 0, 0, 0);
                                    tickerAnimations[i].To = new Thickness(0);
                                    tickerAnimations[i].RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;

                                    tickerStoryboards[i].Children.Clear();
                                    tickerStoryboards[i].Children.Add(tickerAnimations[i]);

                                    tickerScrolls[i].Margin = new Thickness(0, 0, 0, 0);
                                    */

                                }
                                else {
                                    /*
                                    if (tickerScrolls[i].Margin.Left == 0)
                                    {
                                        tickerScrolls[i].Margin = new Thickness(0 - tickerStackpanels[i].ActualWidth, 0, 0, 0);
                                        tickerAnimations[i].From = new Thickness(SharedData.theme.tickers[i].width + tickerStackpanels[i].ActualWidth, 0, 0, 0);
                                        tickerAnimations[i].Duration = TimeSpan.FromSeconds(tickerAnimations[i].From.Value.Left / 120);
                                        tickerStoryboards[i].Begin(this);
                                    }
                                    */
                                    // update data
                                    Double margin = tickerStackpanels[i].Margin.Left; // +tickerScrolls[i].Margin.Left;
                                    for (int j = 0; j < SharedData.standing[SharedData.overlaySession].Length; j++) // drivers
                                    {
                                        for (int k = 0; k < SharedData.theme.tickers[i].labels.Length; k++) // labels
                                        {
                                            if ((j * SharedData.theme.tickers[i].labels.Length) + k < tickerLabels[i].Length)
                                            {
                                                if (margin > (0 - tickerLabels[i][(j * SharedData.theme.tickers[i].labels.Length) + k].DesiredSize.Width) && margin < SharedData.theme.tickers[i].width)
                                                {
                                                    tickerLabels[i][(j * SharedData.theme.tickers[i].labels.Length) + k].Content = SharedData.theme.formatFollowedText(
                                                        SharedData.theme.tickers[i].labels[k],
                                                        SharedData.standing[SharedData.overlaySession][j].id,
                                                        SharedData.overlaySession);
                                                }

                                                if (SharedData.theme.tickers[i].fillVertical == false)
                                                {
                                                    margin += tickerLabels[i][(j * SharedData.theme.tickers[i].labels.Length) + k].DesiredSize.Width;
                                                }
                                            }
                                        }

                                        if (SharedData.theme.tickers[i].fillVertical == true && j < tickerRowpanels[i].Length)
                                        {
                                            margin += tickerRowpanels[i][j].DesiredSize.Width;
                                        }
                                    }

                                    // old scroll
                                    Thickness scroller = tickerStackpanels[i].Margin;
                                    scroller.Left -= Properties.Settings.Default.TickerSpeed;
                                    tickerStackpanels[i].Margin = scroller;
                                }
                                break;
                            case Theme.dataset.sessionstate:
                                if (/*tickerScrolls[i].Margin.Left +*/ tickerStackpanels[i].ActualWidth + tickerStackpanels[i].Margin.Left <= 0)
                                {
                                    //tickerScrolls[i].Children.Clear();
                                    tickerStackpanels[i].Children.Clear();

                                    tickerStackpanels[i] = new StackPanel();
                                    tickerStackpanels[i].Margin = new Thickness(SharedData.theme.tickers[i].width, 0, 0, 0);

                                    if (SharedData.theme.tickers[i].fillVertical)
                                        tickerStackpanels[i].Orientation = Orientation.Vertical;
                                    else
                                        tickerStackpanels[i].Orientation = Orientation.Horizontal;

                                    //tickerScrolls[i].Children.Add(tickerStackpanels[i]);
                                    tickers[i].Children.Add(tickerStackpanels[i]);
                                    tickerLabels[i] = new Label[SharedData.theme.tickers[i].labels.Length];

                                    for (int j = 0; j < SharedData.theme.tickers[i].labels.Length; j++) // drivers
                                    {
                                        tickerLabels[i][j] = DrawLabel(SharedData.theme.tickers[i].labels[j]);
                                        tickerLabels[i][j].Content = SharedData.theme.formatSessionstateText(
                                            SharedData.theme.tickers[i].labels[j],
                                            SharedData.overlaySession);
                                        if (SharedData.theme.tickers[i].labels[j].width == 0)
                                            tickerLabels[i][j].Width = Double.NaN;
                                        
                                        tickerStackpanels[i].Children.Add(tickerLabels[i][j]);

                                    }
                                    /*
                                    if (this.FindName("tickerScroll" + i) == null)
                                        this.RegisterName("tickerScroll" + i, tickerStackpanels[i]);

                                    Storyboard.SetTargetName(tickerAnimations[i], "tickerScroll" + i);
                                    Storyboard.SetTargetProperty(tickerAnimations[i], new PropertyPath(StackPanel.MarginProperty));
                                    tickerAnimations[i].From = new Thickness(SharedData.theme.tickers[i].width + tickerStackpanels[i].ActualWidth, 0, 0, 0);
                                    tickerAnimations[i].To = new Thickness(0);
                                    tickerAnimations[i].RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;

                                    tickerStoryboards[i].Children.Clear();
                                    tickerStoryboards[i].Children.Add(tickerAnimations[i]);

                                    tickerScrolls[i].Margin = new Thickness(0, 0, 0, 0);
                                    */

                                }
                                else
                                {
                                    /*
                                    if (tickerScrolls[i].Margin.Left == 0)
                                    {
                                        tickerScrolls[i].Margin = new Thickness(0 - tickerStackpanels[i].ActualWidth, 0, 0, 0);
                                        tickerAnimations[i].From = new Thickness(SharedData.theme.tickers[i].width + tickerStackpanels[i].ActualWidth, 0, 0, 0);
                                        tickerAnimations[i].Duration = TimeSpan.FromSeconds(tickerAnimations[i].From.Value.Left / 120);
                                        tickerStoryboards[i].Begin(this);
                                    }
                                    */

                                    // update data
                                    Double margin = tickerStackpanels[i].Margin.Left; // + tickerScrolls[i].Margin.Left;
                                    for (int k = 0; k < SharedData.theme.tickers[i].labels.Length; k++) // labels
                                    {
                                        if (k < tickerLabels[i].Length)
                                        {
                                            if (margin > (0 - tickerLabels[i][k].DesiredSize.Width) && margin < SharedData.theme.tickers[i].width)
                                            {
                                                tickerLabels[i][k].Content = SharedData.theme.formatSessionstateText(
                                                    SharedData.theme.tickers[i].labels[k],
                                                    SharedData.overlaySession);
                                            }
                                            margin += tickerLabels[i][k].DesiredSize.Width;
                                        }
                                    }

                                    // old scroll
                                    Thickness scroller = tickerStackpanels[i].Margin;
                                    scroller.Left -= Properties.Settings.Default.TickerSpeed;
                                    tickerStackpanels[i].Margin = scroller;
                                }
                                break;
                            default:
                            case Theme.dataset.followed:
                                if (tickerStackpanels[i].ActualWidth + tickerStackpanels[i].Margin.Left <= 0)
                                {
                                    //tickerScrolls[i].Children.Clear();
                                    tickerStackpanels[i].Children.Clear();

                                    tickerStackpanels[i] = new StackPanel();
                                    tickerStackpanels[i].Margin = new Thickness(SharedData.theme.tickers[i].width, 0, 0, 0);

                                    if (SharedData.theme.tickers[i].fillVertical)
                                        tickerStackpanels[i].Orientation = Orientation.Vertical;
                                    else
                                        tickerStackpanels[i].Orientation = Orientation.Horizontal;

                                    //tickerScrolls[i].Children.Add(tickerStackpanels[i]);
                                    tickers[i].Children.Add(tickerStackpanels[i]);

                                    tickerLabels[i] = new Label[SharedData.theme.tickers[i].labels.Length];

                                    for (int j = 0; j < SharedData.theme.tickers[i].labels.Length; j++) // drivers
                                    {
                                        tickerLabels[i][j] = DrawLabel(SharedData.theme.tickers[i].labels[j]);
                                        if (SharedData.theme.tickers[i].labels[j].width == 0)
                                            tickerLabels[i][j].Width = Double.NaN;

                                        tickerStackpanels[i].Children.Add(tickerLabels[i][j]);
                                    }

                                    /*
                                    if (this.FindName("tickerScroll" + i) == null)
                                        this.RegisterName("tickerScroll" + i, tickerStackpanels[i]);

                                    Storyboard.SetTargetName(tickerAnimations[i], "tickerScroll" + i);
                                    Storyboard.SetTargetProperty(tickerAnimations[i], new PropertyPath(StackPanel.MarginProperty));
                                    
                                    tickerAnimations[i].From = new Thickness(SharedData.theme.tickers[i].width, 0, 0, 0);
                                    tickerStackpanels[i].Margin = new Thickness(SharedData.theme.tickers[i].width, 0, 0, 0);
                                    tickerAnimations[i].To = new Thickness(0);
                                    tickerAnimations[i].RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
                                    tickerAnimations[i].Duration = TimeSpan.FromSeconds(tickerAnimations[i].From.Value.Left / 120);
                                    
                                    tickerStoryboards[i].Children.Clear();
                                    tickerStoryboards[i].Children.Add(tickerAnimations[i]);
                                    //tickerStoryboards[i].Begin(this);

                                    tickerScrolls[i].Margin = new Thickness(0, 0, 0, 0);
                                     * */
                                }
                                else
                                {
                                    /*
                                    if (tickerScrolls[i].Margin.Left == 0)
                                    {
                                        
                                        tickerScrolls[i].Margin = new Thickness(0 - tickerStackpanels[i].ActualWidth, 0, 0, 0);
                                        tickerAnimations[i].From = new Thickness(SharedData.theme.tickers[i].width + tickerStackpanels[i].ActualWidth, 0, 0, 0);
                                        //tickerAnimations[i].To = new Thickness(0);
                                        //tickerAnimations[i].RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
                                        //tickerAnimations[i].Duration = TimeSpan.FromSeconds(tickerAnimations[i].From.Value.Left / 120);
                                        //tickerStackpanels[i].Margin = new Thickness(SharedData.theme.tickers[i].width + (tickerStackpanels[i].ActualWidth * 2), 0, 0, 0);
                                        //tickerStoryboards[i].Begin(this);
                                        //tickerStoryboards[i].Resume(this);

                                    }
                                    */
                                    // update data
                                    for (int k = 0; k < SharedData.theme.tickers[i].labels.Length; k++) // labels
                                    {
                                        if (k < tickerLabels[i].Length)
                                        {
                                            tickerLabels[i][k].Content = SharedData.theme.formatFollowedText(
                                                SharedData.theme.tickers[i].labels[k],
                                                SharedData.sessions[SharedData.overlaySession].driverFollowed,
                                                SharedData.overlaySession);
                                        }
                                    }

                                    // old scroll
                                    Thickness scroller = tickerStackpanels[i].Margin;
                                    scroller.Left -= Properties.Settings.Default.TickerSpeed;
                                    tickerStackpanels[i].Margin = scroller;
                                }
                                break;
                        }
                    }
                }

                // start lights
                for (int i = 0; i < SharedData.theme.images.Length; i++)
                {
                    if (SharedData.theme.images[i].light != Theme.lights.none && SharedData.theme.images[i].visible == true)
                    {
                        if (SharedData.sessions[SharedData.overlaySession].state == iRacingTelem.eSessionState.kSessionStateWarmup ||
                        SharedData.sessions[SharedData.overlaySession].state == iRacingTelem.eSessionState.kSessionStateRacing)
                        {
                            timer = (DateTime.Now - SharedData.startlights);

                            if ((SharedData.sessions[SharedData.overlaySession].laps - SharedData.sessions[SharedData.overlaySession].lapsRemaining) < 0)
                            {
                                if (timer.TotalMinutes > 1) // reset
                                    SharedData.startlights = DateTime.Now;
                            }

                            if (timer.TotalSeconds < 5 || timer.TotalMinutes > 1)
                            {
                                if(SharedData.theme.images[i].light == Theme.lights.off)
                                    images[i].Visibility = System.Windows.Visibility.Visible;
                                else
                                    images[i].Visibility = System.Windows.Visibility.Hidden;
                            }
                            else if (SharedData.sessions[SharedData.overlaySession].state == iRacingTelem.eSessionState.kSessionStateRacing)
                            {
                                if (SharedData.theme.images[i].light == Theme.lights.green)
                                    images[i].Visibility = System.Windows.Visibility.Visible;
                                else
                                    images[i].Visibility = System.Windows.Visibility.Hidden;
                            }
                            else
                            {
                                if (SharedData.theme.images[i].light == Theme.lights.red)
                                    images[i].Visibility = System.Windows.Visibility.Visible;
                                else
                                    images[i].Visibility = System.Windows.Visibility.Hidden;
                            }
                        }
                    }
                }

                
                for (int i = 0; i < SharedData.theme.images.Length; i++)
                {
                    // flags
                    if (SharedData.theme.images[i].flag != Theme.flags.none && SharedData.theme.images[i].visible == true) 
                    {
                        // race
                        if (SharedData.sessions[SharedData.overlaySession].state == iRacingTelem.eSessionState.kSessionStateRacing)
                        {
                            // yellow
                            if (SharedData.sessions[SharedData.overlaySession].flag == iRacingTelem.eSessionFlag.kFlagYellow)
                            {
                                if (SharedData.theme.images[i].flag == Theme.flags.yellow)
                                    images[i].Visibility = System.Windows.Visibility.Visible;
                                else
                                    images[i].Visibility = System.Windows.Visibility.Hidden;
                            }

                            // white
                            else if (SharedData.sessions[SharedData.overlaySession].lapsRemaining == 1) 
                            {
                                if(SharedData.theme.images[i].flag == Theme.flags.white)
                                    images[i].Visibility = System.Windows.Visibility.Visible;
                                else
                                    images[i].Visibility = System.Windows.Visibility.Hidden;
                            }

                            // checkered
                            else if (SharedData.sessions[SharedData.overlaySession].lapsRemaining <= 0)
                            {
                                if (SharedData.theme.images[i].flag == Theme.flags.checkered)
                                    images[i].Visibility = System.Windows.Visibility.Visible;
                                else
                                    images[i].Visibility = System.Windows.Visibility.Hidden;
                            }
                            // green
                            else
                            {
                                if (SharedData.theme.images[i].flag == Theme.flags.green)
                                    images[i].Visibility = System.Windows.Visibility.Visible;
                                else
                                    images[i].Visibility = System.Windows.Visibility.Hidden;
                            }

                        }
                        // finishing
                        else if (SharedData.sessions[SharedData.overlaySession].state == iRacingTelem.eSessionState.kSessionStateCheckered ||
                            SharedData.sessions[SharedData.overlaySession].state == iRacingTelem.eSessionState.kSessionStateCoolDown)
                        {
                            if (SharedData.theme.images[i].flag == Theme.flags.checkered)
                                images[i].Visibility = System.Windows.Visibility.Visible;
                            else
                                images[i].Visibility = System.Windows.Visibility.Hidden;
                        }
                        // gridding & pace lap
                        else if (SharedData.sessions[SharedData.overlaySession].state != iRacingTelem.eSessionState.kSessionStateGetInCar ||
                            SharedData.sessions[SharedData.overlaySession].state != iRacingTelem.eSessionState.kSessionStateParadeLaps ||
                            SharedData.sessions[SharedData.overlaySession].state != iRacingTelem.eSessionState.kSessionStateWarmup)
                        {
                            if (SharedData.theme.images[i].flag == Theme.flags.yellow)
                                images[i].Visibility = System.Windows.Visibility.Visible;
                            else
                                images[i].Visibility = System.Windows.Visibility.Hidden;
                        }
                        else
                            images[i].Visibility = System.Windows.Visibility.Hidden;
                    }
                    
                    // replay transition
                    if (SharedData.theme.images[i].replay == true)
                    {
                        if (SharedData.replayInProgress)
                            images[i].Visibility = System.Windows.Visibility.Visible;
                        else
                            images[i].Visibility = System.Windows.Visibility.Hidden;
                    }
                }

                for (int i = 0; i < videos.Length; i++)
                {
                    if (SharedData.theme.videos[i].visible != visibility2boolean[videos[i].Visibility])
                        videos[i].Visibility = boolean2visibility[SharedData.theme.videos[i].visible];

                    if(videos[i].Visibility == System.Windows.Visibility.Visible && SharedData.theme.videos[i].playing == false) 
                    {
                        videos[i].Play();
                        SharedData.theme.videos[i].playing = true;
                    }

                    if (SharedData.theme.videos[i].replay == true)
                    {
                        if (SharedData.replayInProgress)
                        {
                            if (videoBoxes[i].Visibility == System.Windows.Visibility.Hidden)
                            {
                                MediaElement video = new MediaElement();
                                if (File.Exists(Directory.GetCurrentDirectory() + "\\" + SharedData.theme.path + "\\" + SharedData.theme.videos[i].filename))
                                {
                                    video.Source = new Uri(Directory.GetCurrentDirectory() + "\\" + SharedData.theme.path + "\\" + SharedData.theme.videos[i].filename);
                                    video.IsMuted = true;
                                    video.LoadedBehavior = MediaState.Manual;
                                    
                                    video.MediaOpened += new RoutedEventHandler(video_MediaOpened);
                                    video.MediaEnded += new RoutedEventHandler(video_MediaEnded); 

                                    VisualBrush myVisualBrush = new VisualBrush();
                                    myVisualBrush.Visual = video;

                                    videoBoxes[i].Fill = myVisualBrush;
                                    
                                    video.Play();
                                }
                            }
                            videoBoxes[i].Visibility = System.Windows.Visibility.Visible;
                        }

                        else
                        {
                            videoBoxes[i].Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                }

                stopwatch.Stop();
                SharedData.overlayEffectiveFPSstack.Push((float)stopwatch.Elapsed.TotalMilliseconds);
            }

            

        }

        void video_MediaOpened(object sender, RoutedEventArgs e)
        {
            SharedData.replayReady.Set();
        }

        private void video_MediaEnded(object sender, EventArgs e)
        {
            MediaElement me;
            me = (MediaElement)sender;
            me.Stop();
            me.Position = new TimeSpan(0);
            me.Play();
        }

        public static string floatTime2String(float time, Boolean showMilli, Boolean showMinutes)
        {
            time = Math.Abs(time);

            int hours = (int)Math.Floor(time / 3600);
            int minutes = (int)Math.Floor((time - (hours * 3600)) / 60);
            int seconds = (int)Math.Floor(time % 60);
            int microseconds = (int)Math.Round(time * 1000 % 1000, 3);
            string output;

            if (time == 0.0)
                output = "-.--";
            else if (hours > 0)
            {
                output = String.Format("{0}:{1:d2}:{2:d2}", hours, minutes, seconds);
            }
            else if (minutes > 0 || showMinutes)
            {
                if (showMilli)
                    output = String.Format("{0}:{1:d2}.{2:d3}", minutes, seconds, microseconds);
                else
                    output = String.Format("{0}:{1:d2}", minutes, seconds);
            }

            else
            {
                if (showMilli)
                    output = String.Format("{0}.{1:d3}", seconds, microseconds);
                else
                    output = String.Format("{0}", seconds);
            }

            return output;
        }
        
    }
}