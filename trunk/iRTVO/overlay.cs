﻿using System;
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

namespace iRTVO
{
    public partial class Overlay : Window
    {

        Canvas driver;
        Label driverPosLabel;
        Label driverNameLabel;
        Label driverDiffLabel;
        Label driverInfoLabel;

        Canvas sidepanel;
        Label[] sidepanelPosLabel;
        Label[] sidepanelNameLabel; 
        Label[] sidepanelDiffLabel;

        Canvas results;
        Label resultsHeader;
        Label resultsSubHeader;
        Label[] resultsPosLabel;
        Label[] resultsNameLabel;
        Label[] resultsDiffLabel;

        //Canvas ticker;
        StackPanel tickerStackPanel;
        Label[] tickerPosLabel;
        Label[] tickerNameLabel;
        Label[] tickerDiffLabel;

        //Canvas sessionstate;
        Label sessionstateText;

        // ligths
        TimeSpan timer;

        // flags
        int[] flags = new int[4] { 
            (int)Theme.overlayTypes.flaggreen,
            (int)Theme.overlayTypes.flagyellow,
            (int)Theme.overlayTypes.flagwhite,
            (int)Theme.overlayTypes.flagcheckered
        };

        private void overlayUpdate(object sender, EventArgs e)
        {

            if (SharedData.requestRefresh == true)
            {
                loadTheme(Properties.Settings.Default.theme);

                overlay.Left = Properties.Settings.Default.OverlayLocationX;
                overlay.Top = Properties.Settings.Default.OverlayLocationY;
                overlay.Width = Properties.Settings.Default.OverlayWidth;
                overlay.Height = Properties.Settings.Default.OverlayHeight;

                resizeOverlay(overlay.Width, overlay.Height);
                SharedData.requestRefresh = false;
            }

            if (SharedData.runOverlay)
            {

                // wait
                SharedData.driversMutex.WaitOne(5);
                SharedData.standingMutex.WaitOne(5);
                SharedData.sessionsMutex.WaitOne(5);

                // hide/show objects
                // driver
                if (SharedData.visible[(int)SharedData.overlayObjects.driver])
                {
                    if (themeImages[(int)Theme.overlayTypes.driver].Visibility == System.Windows.Visibility.Hidden)
                    {
                        if (themeImages[(int)Theme.overlayTypes.driver] != null)
                            themeImages[(int)Theme.overlayTypes.driver].Visibility = System.Windows.Visibility.Visible;
                        driver.Visibility = System.Windows.Visibility.Visible;
                        //SharedData.standingsUpdated = true;
                    }
                }
                else
                {
                    if (themeImages[(int)Theme.overlayTypes.driver].Visibility == System.Windows.Visibility.Visible)
                    {
                        if (themeImages[(int)Theme.overlayTypes.driver] != null)
                            themeImages[(int)Theme.overlayTypes.driver].Visibility = System.Windows.Visibility.Hidden;
                        driver.Visibility = System.Windows.Visibility.Hidden;
                    }
                }

                // sidepanel
                if (SharedData.visible[(int)SharedData.overlayObjects.sidepanel])
                {
                    if (themeImages[(int)Theme.overlayTypes.sidepanel].Visibility == System.Windows.Visibility.Hidden)
                    {
                        if (themeImages[(int)Theme.overlayTypes.sidepanel] != null)
                            themeImages[(int)Theme.overlayTypes.sidepanel].Visibility = System.Windows.Visibility.Visible;
                        sidepanel.Visibility = System.Windows.Visibility.Visible;
                        //SharedData.standingsUpdated = true;
                    }
                }
                else
                {
                    if (themeImages[(int)Theme.overlayTypes.sidepanel].Visibility == System.Windows.Visibility.Visible)
                    {
                        if (themeImages[(int)Theme.overlayTypes.sidepanel] != null)
                            themeImages[(int)Theme.overlayTypes.sidepanel].Visibility = System.Windows.Visibility.Hidden;
                        sidepanel.Visibility = System.Windows.Visibility.Hidden;
                    }
                }

                // replay
                if (SharedData.visible[(int)SharedData.overlayObjects.replay])
                {
                    if (themeImages[(int)Theme.overlayTypes.replay].Visibility == System.Windows.Visibility.Hidden)
                        themeImages[(int)Theme.overlayTypes.replay].Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    if (themeImages[(int)Theme.overlayTypes.replay].Visibility == System.Windows.Visibility.Visible)
                        themeImages[(int)Theme.overlayTypes.replay].Visibility = System.Windows.Visibility.Hidden;
                }

                // results
                if (SharedData.visible[(int)SharedData.overlayObjects.results])
                {
                    if (themeImages[(int)Theme.overlayTypes.results].Visibility == System.Windows.Visibility.Hidden)
                    {
                        if (themeImages[(int)Theme.overlayTypes.results] != null)
                            themeImages[(int)Theme.overlayTypes.results].Visibility = System.Windows.Visibility.Visible;
                        results.Visibility = System.Windows.Visibility.Visible;
                        //SharedData.standingsUpdated = true;
                    }
                }
                else
                {
                    if (themeImages[(int)Theme.overlayTypes.results].Visibility == System.Windows.Visibility.Visible)
                    {
                        if (themeImages[(int)Theme.overlayTypes.results] != null)
                            themeImages[(int)Theme.overlayTypes.results].Visibility = System.Windows.Visibility.Hidden;
                        results.Visibility = System.Windows.Visibility.Hidden;
                    }
                }

                // session state
                if (SharedData.visible[(int)SharedData.overlayObjects.sessionstate])
                {
                    if (themeImages[(int)Theme.overlayTypes.sessionstate].Visibility == System.Windows.Visibility.Hidden)
                    {
                        if (themeImages[(int)Theme.overlayTypes.sessionstate] != null)
                            themeImages[(int)Theme.overlayTypes.sessionstate].Visibility = System.Windows.Visibility.Visible;
                        sessionstateText.Visibility = System.Windows.Visibility.Visible;
                       // SharedData.sessionsUpdated = true;
                    }
                }
                else
                {
                    if (themeImages[(int)Theme.overlayTypes.sessionstate].Visibility == System.Windows.Visibility.Visible)
                    {
                        if (themeImages[(int)Theme.overlayTypes.sessionstate] != null)
                            themeImages[(int)Theme.overlayTypes.sessionstate].Visibility = System.Windows.Visibility.Hidden;
                        sessionstateText.Visibility = System.Windows.Visibility.Hidden;
                    }
                }

                // start lights
                if (!SharedData.visible[(int)SharedData.overlayObjects.startlights])
                {
                    if (themeImages[(int)Theme.overlayTypes.lightsgreen] != null)
                        themeImages[(int)Theme.overlayTypes.lightsgreen].Visibility = System.Windows.Visibility.Hidden;
                    if (themeImages[(int)Theme.overlayTypes.lightsoff] != null)
                        themeImages[(int)Theme.overlayTypes.lightsoff].Visibility = System.Windows.Visibility.Hidden;
                    if (themeImages[(int)Theme.overlayTypes.lightsred] != null)
                        themeImages[(int)Theme.overlayTypes.lightsred].Visibility = System.Windows.Visibility.Hidden;
                }

                // ticker
                if (SharedData.visible[(int)SharedData.overlayObjects.ticker])
                {
                    //if (themeImages[(int)Theme.overlayTypes.ticker].Visibility == System.Windows.Visibility.Hidden)
                    //{
                        //if (themeImages[(int)Theme.overlayTypes.ticker] != null)
                        //    themeImages[(int)Theme.overlayTypes.ticker].Visibility = System.Windows.Visibility.Visible;
                    if (tickerStackPanel.Visibility == System.Windows.Visibility.Hidden)
                        tickerStackPanel.Visibility = System.Windows.Visibility.Visible;
                    //}
                }
                else
                {
                    /*
                    if (themeImages[(int)Theme.overlayTypes.sidepanel].Visibility == System.Windows.Visibility.Visible)
                    {
                        if (themeImages[(int)Theme.overlayTypes.sidepanel] != null)
                            themeImages[(int)Theme.overlayTypes.sidepanel].Visibility = System.Windows.Visibility.Hidden;
                        sidepanel.Visibility = System.Windows.Visibility.Hidden;
                    }
                    */
                    if (tickerStackPanel.Visibility == System.Windows.Visibility.Visible)
                        tickerStackPanel.Visibility = System.Windows.Visibility.Hidden;

                    if (tickerStackPanel.Margin.Left != theme.width)
                    {
                        // move ticker for new scroll
                        Thickness scroller = tickerStackPanel.Margin;
                        scroller.Left = tickerStackPanel.ActualWidth - 1;
                        tickerStackPanel.Margin = scroller;
                    }
                }

                // do we allow retirement
                Boolean allowRetire = true;

                if (SharedData.resultSession >= 0)
                {
                    if (SharedData.sessions[SharedData.resultSession].lapsRemaining <= 0 || 
                            SharedData.sessions[SharedData.resultSession].state == iRacingTelem.eSessionState.kSessionStateCheckered)
                        allowRetire = false;
                    else
                        allowRetire = true;
                }
                else if (SharedData.sessions[SharedData.currentSession].lapsRemaining <= 0 || 
                        SharedData.sessions[SharedData.currentSession].state == iRacingTelem.eSessionState.kSessionStateCheckered)
                    allowRetire = false;
                else
                    allowRetire = true;


                //  driver
                if (SharedData.visible[(int)SharedData.overlayObjects.driver]) // && SharedData.standingsUpdated)
                {
                    Boolean noLapsDriver = true;
                    driverNameLabel.Content = String.Format(theme.driver.Name.text, theme.getFormats(SharedData.drivers[SharedData.sessions[SharedData.currentSession].driverFollowed]));
                    driverInfoLabel.Content = String.Format(theme.driver.Info.text, theme.getFormats(SharedData.drivers[SharedData.sessions[SharedData.currentSession].driverFollowed]));
                    if (SharedData.standing[SharedData.currentSession] != null)
                    {
                        for (int i = 0; i < SharedData.standing[SharedData.currentSession].Length; i++)
                        {
                            // update  driver
                            if (SharedData.standing[SharedData.currentSession][i].id == SharedData.sessions[SharedData.currentSession].driverFollowed)
                            {
                                noLapsDriver = false;
                                driverPosLabel.Content = (i + 1).ToString() + ".";

                                // race
                                if (SharedData.sessions[SharedData.currentSession].type == iRacingTelem.eSessionType.kSessionTypeRace)
                                {
                                    if (SharedData.drivers[SharedData.standing[SharedData.currentSession][i].id].onTrack == false && allowRetire) // out
                                    {
                                        if ((DateTime.Now - SharedData.drivers[SharedData.standing[SharedData.currentSession][i].id].offTrackSince).TotalMilliseconds > 1000)
                                            driverDiffLabel.Content = theme.translation["out"];
                                    }
                                    else if (SharedData.standing[SharedData.currentSession][i].lapDiff > 0) // lapped
                                    {
                                        driverDiffLabel.Content = theme.translation["behind"] + SharedData.standing[SharedData.currentSession][i].lapDiff + theme.translation["lap"];
                                    }
                                    else // not lapped
                                    {
                                        if (SharedData.standing[SharedData.currentSession][i].diff > 0)
                                        { // in same lap
                                            if (SharedData.sidepanelType == SharedData.sidepanelTypes.fastlap)
                                                driverDiffLabel.Content = floatTime2String(SharedData.standing[SharedData.currentSession][i].fastLap, true, false);
                                            else
                                                driverDiffLabel.Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].diff, true, false);
                                        }
                                        else // leader
                                            driverDiffLabel.Content = "-.--";
                                    }
                                }
                                // prac/qual
                                else
                                {
                                    if (i == 0)
                                        driverDiffLabel.Content = floatTime2String(SharedData.standing[SharedData.currentSession][0].fastLap, true, false);
                                    else if (SharedData.standing[SharedData.currentSession][i].diff > 0)
                                    {
                                        if (SharedData.sidepanelType == SharedData.sidepanelTypes.fastlap)
                                            driverDiffLabel.Content = floatTime2String(SharedData.standing[SharedData.currentSession][i].fastLap, true, false);
                                        else
                                            driverDiffLabel.Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].diff - SharedData.standing[SharedData.currentSession][0].diff, true, false);
                                    }
                                    else
                                        driverDiffLabel.Content = "-.--";
                                }
                            }
                        }
                        if (noLapsDriver)
                        {
                            driverPosLabel.Content = SharedData.standing[SharedData.currentSession].Length + ".";
                            driverDiffLabel.Content = "-.--";
                        }
                    }
                }

                // oSidepanel
                if (SharedData.standing[SharedData.currentSession] != null && SharedData.visible[(int)SharedData.overlayObjects.sidepanel]) // && SharedData.standingsUpdated)
                {
                    int sidepanelCount = 0;
                    for (int i = 0; i < SharedData.standing[SharedData.currentSession].Length; i++)
                    {
                        // diff to followed
                        if (SharedData.standing[SharedData.currentSession][i].id == SharedData.sessions[SharedData.currentSession].driverFollowed && SharedData.sidepanelType == SharedData.sidepanelTypes.followed)
                        {
                            int k = i - (theme.sidepanel.size / 2);
                            while (k < 0)
                                k++;
                            while ((k + theme.sidepanel.size) > SharedData.standing[SharedData.currentSession].Length && k > 0)
                            {
                                if (k > 0)
                                    k--;
                            }

                            for (int j = 0; j < theme.sidepanel.size; j++)
                            {
                                if (k < SharedData.standing[SharedData.currentSession].Length)
                                {
                                    sidepanelPosLabel[j].Content = (k + 1).ToString();
                                    sidepanelNameLabel[j].Content = String.Format(theme.sidepanel.Name.text, theme.getFormats(SharedData.drivers[SharedData.standing[SharedData.currentSession][k].id]));

                                    if (i != k)
                                    {
                                        if (k < i)
                                        {
                                            if (SharedData.sessions[SharedData.currentSession].type == iRacingTelem.eSessionType.kSessionTypeRace) // race
                                            {
                                                if (SharedData.drivers[SharedData.standing[SharedData.currentSession][k].id].onTrack == false && allowRetire) // out
                                                    sidepanelDiffLabel[j].Content = theme.translation["out"];
                                                else if (SharedData.standing[SharedData.currentSession][k].lapDiff == SharedData.standing[SharedData.currentSession][i].lapDiff) // same lap
                                                    sidepanelDiffLabel[j].Content = theme.translation["ahead"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].diff - SharedData.standing[SharedData.currentSession][k].diff, true, false);
                                                else // lapped
                                                    sidepanelDiffLabel[j].Content = theme.translation["ahead"] + Math.Abs(SharedData.standing[SharedData.currentSession][k].lapDiff - SharedData.standing[SharedData.currentSession][i].lapDiff) + theme.translation["lap"];
                                            }
                                            else // prac / qual
                                                sidepanelDiffLabel[j].Content = theme.translation["ahead"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].fastLap - SharedData.standing[SharedData.currentSession][k].fastLap, true, false);
                                        }
                                        else
                                        {
                                            if (SharedData.sessions[SharedData.currentSession].type == iRacingTelem.eSessionType.kSessionTypeRace) // race
                                            {
                                                if (SharedData.drivers[SharedData.standing[SharedData.currentSession][k].id].onTrack == false && allowRetire) // out
                                                    sidepanelDiffLabel[j].Content = theme.translation["out"];
                                                else if (SharedData.standing[SharedData.currentSession][k].lapDiff == SharedData.standing[SharedData.currentSession][i].lapDiff) // same lap
                                                    sidepanelDiffLabel[j].Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].diff - SharedData.standing[SharedData.currentSession][k].diff, true, false);
                                                else // lapped
                                                    sidepanelDiffLabel[j].Content = theme.translation["behind"] + Math.Abs(SharedData.standing[SharedData.currentSession][i].lapDiff - SharedData.standing[SharedData.currentSession][k].lapDiff) + theme.translation["lap"];
                                            }
                                            else // prac / qual
                                                sidepanelDiffLabel[j].Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].fastLap - SharedData.standing[SharedData.currentSession][k].fastLap, true, false);
                                        }

                                    }
                                    else
                                    {
                                        sidepanelDiffLabel[j].Content = "-.--";
                                    }
                                    k++;
                                    sidepanelCount++;
                                }
                            }
                        }
                        // diff to leader
                        if (SharedData.sidepanelType == SharedData.sidepanelTypes.leader && i < theme.sidepanel.size)
                        {
                            sidepanelPosLabel[i].Content = (i + 1).ToString();
                            sidepanelNameLabel[i].Content = String.Format(theme.sidepanel.Name.text, theme.getFormats(SharedData.drivers[SharedData.standing[SharedData.currentSession][i].id]));
                            if (i > 0)
                            {
                                if (SharedData.sessions[SharedData.currentSession].type == iRacingTelem.eSessionType.kSessionTypeRace)
                                {
                                    if (SharedData.drivers[SharedData.standing[SharedData.currentSession][i].id].onTrack == false && allowRetire) // out
                                        sidepanelDiffLabel[i].Content = theme.translation["out"];
                                    else if (SharedData.standing[SharedData.currentSession][i].lapDiff > 0)
                                        sidepanelDiffLabel[i].Content = theme.translation["behind"] + SharedData.standing[SharedData.currentSession][i].lapDiff + theme.translation["lap"]; // lapped
                                    else
                                        sidepanelDiffLabel[i].Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].diff, true, false); // normal
                                }
                                else // prac/qual
                                    sidepanelDiffLabel[i].Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].fastLap - SharedData.standing[SharedData.currentSession][0].fastLap, true, false);
                            }
                            else // leader
                            {
                                if (SharedData.sessions[SharedData.currentSession].type == iRacingTelem.eSessionType.kSessionTypeRace)
                                {
                                    if (SharedData.drivers[SharedData.standing[SharedData.currentSession][0].id].onTrack == false && allowRetire) // out
                                        sidepanelDiffLabel[0].Content = theme.translation["out"];
                                    else // normal
                                        sidepanelDiffLabel[0].Content = null;
                                        //sidepanelDiffLabel[0].Content = floatTime2String((SharedData.sessions[SharedData.currentSession].time - SharedData.sessions[SharedData.currentSession].timeRemaining), false, true);
                                }
                                else // prac/qual
                                    sidepanelDiffLabel[0].Content = floatTime2String(SharedData.standing[SharedData.currentSession][0].fastLap, true, true);

                            }
                            sidepanelCount++;
                        }
                        // fastest lap
                        if (SharedData.sidepanelType == SharedData.sidepanelTypes.fastlap && i < theme.sidepanel.size)
                        {
                            sidepanelPosLabel[i].Content = (i + 1).ToString();
                            sidepanelNameLabel[i].Content = String.Format(theme.sidepanel.Name.text, theme.getFormats(SharedData.drivers[SharedData.standing[SharedData.currentSession][i].id]));
                            sidepanelDiffLabel[i].Content = floatTime2String(SharedData.standing[SharedData.currentSession][i].fastLap, true, false);
                            sidepanelCount++;
                        }
                    }
                }

                // results update
                if (SharedData.resultSession >= 0 && SharedData.standing[SharedData.resultSession] != null) // && SharedData.standingsUpdated)
                {
                    // header
                    if (SharedData.sessions[SharedData.resultSession].type == iRacingTelem.eSessionType.kSessionTypeRace)
                        resultsHeader.Content = String.Format(theme.resultsHeader.text, theme.translation["race"]);
                    else if (SharedData.sessions[SharedData.resultSession].type == iRacingTelem.eSessionType.kSessionTypeQualifyLone ||
                             SharedData.sessions[SharedData.resultSession].type == iRacingTelem.eSessionType.kSessionTypeQualifyOpen)
                        resultsHeader.Content = String.Format(theme.resultsHeader.text, theme.translation["qualify"]);
                    else if (SharedData.sessions[SharedData.resultSession].type == iRacingTelem.eSessionType.kSessionTypePractice ||
                             SharedData.sessions[SharedData.resultSession].type == iRacingTelem.eSessionType.kSessionTypePracticeLone ||
                             SharedData.sessions[SharedData.resultSession].type == iRacingTelem.eSessionType.kSessionTypeTesting)
                        resultsHeader.Content = String.Format(theme.resultsHeader.text, theme.translation["practice"]);

                    if (SharedData.sessions[SharedData.resultSession].laps == iRacingTelem.LAPS_UNLIMITED)
                        resultsSubHeader.Content = String.Format(theme.resultsSubHeader.text, Math.Floor((SharedData.sessions[SharedData.resultSession].time - SharedData.sessions[SharedData.resultSession].timeRemaining) / 60), theme.translation["minutes"]);
                    else
                        resultsSubHeader.Content = String.Format(theme.resultsSubHeader.text, SharedData.sessions[SharedData.resultSession].laps - SharedData.sessions[SharedData.resultSession].lapsRemaining, theme.translation["laps"]);

                    for (int i = theme.results.size * SharedData.resultPage; i <= ((theme.results.size * (SharedData.resultPage + 1)) - 1); i++)
                    {
                        int j;
                        if (SharedData.resultPage > 0)
                            j = i % (theme.results.size * SharedData.resultPage);
                        else
                            j = i;

                        if (i < SharedData.standing[SharedData.currentSession].Length)
                        {
                            resultsPosLabel[j].Content = (i + 1).ToString();
                            resultsNameLabel[j].Content = String.Format(theme.results.Name.text, theme.getFormats(SharedData.drivers[SharedData.standing[SharedData.resultSession][i].id]));

                            if (SharedData.sessions[SharedData.resultSession].type == iRacingTelem.eSessionType.kSessionTypeRace)
                            {
                                if (SharedData.drivers[SharedData.standing[SharedData.resultSession][i].id].onTrack == false && allowRetire) // out
                                    resultsDiffLabel[j].Content = theme.translation["out"];
                                else if (i == 0)
                                    if (SharedData.sidepanelType == SharedData.sidepanelTypes.fastlap)
                                        resultsDiffLabel[j].Content = floatTime2String(SharedData.standing[SharedData.resultSession][0].fastLap, true, false);
                                    else
                                        resultsDiffLabel[j].Content = Math.Floor(SharedData.standing[SharedData.resultSession][0].completedLaps) + " " + theme.translation["laps"];
                                else if (SharedData.standing[SharedData.resultSession][i].lapDiff > 0) // lapped
                                    resultsDiffLabel[j].Content = theme.translation["behind"] + SharedData.standing[SharedData.resultSession][i].lapDiff + theme.translation["lap"];
                                else
                                { // normal
                                    if (SharedData.sidepanelType == SharedData.sidepanelTypes.fastlap)
                                        resultsDiffLabel[j].Content = floatTime2String(SharedData.standing[SharedData.resultSession][i].fastLap, true, false);
                                    else
                                        resultsDiffLabel[j].Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.resultSession][i].diff - SharedData.standing[SharedData.resultSession][0].diff, true, false);
                                }
                            }
                            else if (i == 0)
                                resultsDiffLabel[j].Content = floatTime2String(SharedData.standing[SharedData.resultSession][0].fastLap, true, false);
                            else
                            {
                                if (SharedData.sidepanelType == SharedData.sidepanelTypes.fastlap)
                                    resultsDiffLabel[j].Content = floatTime2String(SharedData.standing[SharedData.resultSession][i].fastLap, true, false);
                                else
                                    resultsDiffLabel[j].Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.resultSession][i].fastLap - SharedData.standing[SharedData.resultSession][0].fastLap, true, false);
                            }

                            if (resultsPosLabel[j].Visibility == System.Windows.Visibility.Hidden)
                            {
                                resultsPosLabel[j].Visibility = System.Windows.Visibility.Visible;
                                resultsNameLabel[j].Visibility = System.Windows.Visibility.Visible;
                                resultsDiffLabel[j].Visibility = System.Windows.Visibility.Visible;
                            }

                        }
                        else
                        {
                            resultsPosLabel[j].Visibility = System.Windows.Visibility.Hidden;
                            resultsNameLabel[j].Visibility = System.Windows.Visibility.Hidden;
                            resultsDiffLabel[j].Visibility = System.Windows.Visibility.Hidden;
                        }

                        if (i == (SharedData.standing[SharedData.currentSession].Length - 1))
                            SharedData.resultLastPage = true;
                    }
                }

                // session state
                if (SharedData.visible[(int)SharedData.overlayObjects.sessionstate]) // && SharedData.sessionsUpdated)
                {
                    if (SharedData.sessions[SharedData.currentSession].laps == iRacingTelem.LAPS_UNLIMITED)
                    {
                        if(SharedData.sessions[SharedData.currentSession].state == iRacingTelem.eSessionState.kSessionStateCheckered) // session ending
                            sessionstateText.Content = theme.translation["finishing"];
                        else // normal
                            sessionstateText.Content = floatTime2String(SharedData.sessions[SharedData.currentSession].timeRemaining, false, true);
                    }
                    else if (SharedData.sessions[SharedData.currentSession].state == iRacingTelem.eSessionState.kSessionStateGetInCar)
                    {
                        sessionstateText.Content = theme.translation["gridding"];
                    }
                    else if (SharedData.sessions[SharedData.currentSession].state == iRacingTelem.eSessionState.kSessionStateParadeLaps)
                    {
                        sessionstateText.Content = theme.translation["pacelap"];
                    }
                    else
                    {
                        int currentlap = (SharedData.sessions[SharedData.currentSession].laps - SharedData.sessions[SharedData.currentSession].lapsRemaining);
                        if (SharedData.sessions[SharedData.currentSession].lapsRemaining < 1)
                        {
                            sessionstateText.Content = theme.translation["finishing"];
                        }
                        else if (SharedData.sessions[SharedData.currentSession].lapsRemaining == 1)
                        {
                            sessionstateText.Content = theme.translation["finallap"];
                        }
                        else if (SharedData.sessions[SharedData.currentSession].lapsRemaining <= Properties.Settings.Default.countdownThreshold) // x laps remaining
                            sessionstateText.Content = String.Format("{0} {1} {2}",
                                SharedData.sessions[SharedData.currentSession].lapsRemaining,
                                theme.translation["laps"],
                                theme.translation["remaining"]
                            ); 
                        else // normal behavior
                        {
                            sessionstateText.Content = String.Format("{0} {1} {2} {3}",
                                theme.translation["lap"],
                                currentlap,
                                theme.translation["of"],
                                SharedData.sessions[SharedData.currentSession].laps
                            );

                        }
                    }
                }

                if (SharedData.visible[(int)SharedData.overlayObjects.startlights])
                {
                    if (SharedData.sessions[SharedData.currentSession].state == iRacingTelem.eSessionState.kSessionStateWarmup ||
                        SharedData.sessions[SharedData.currentSession].state == iRacingTelem.eSessionState.kSessionStateRacing)
                    {
                        timer = (DateTime.Now - SharedData.startlights);

                        if ((SharedData.sessions[SharedData.currentSession].laps - SharedData.sessions[SharedData.currentSession].lapsRemaining) < 0)
                        {
                            if (timer.TotalMinutes > 1) // reset
                                SharedData.startlights = DateTime.Now;
                        }

                        if (timer.TotalSeconds < 5 || timer.TotalMinutes > 1)
                        {
                            themeImages[(int)Theme.overlayTypes.lightsoff].Visibility = System.Windows.Visibility.Visible;
                            themeImages[(int)Theme.overlayTypes.lightsred].Visibility = System.Windows.Visibility.Hidden;
                            themeImages[(int)Theme.overlayTypes.lightsgreen].Visibility = System.Windows.Visibility.Hidden;
                        }
                        else if (timer.TotalSeconds < 9)
                        {
                            themeImages[(int)Theme.overlayTypes.lightsoff].Visibility = System.Windows.Visibility.Hidden;
                            themeImages[(int)Theme.overlayTypes.lightsred].Visibility = System.Windows.Visibility.Visible;
                            themeImages[(int)Theme.overlayTypes.lightsgreen].Visibility = System.Windows.Visibility.Hidden;
                        }
                        else
                        {
                            themeImages[(int)Theme.overlayTypes.lightsoff].Visibility = System.Windows.Visibility.Hidden;
                            themeImages[(int)Theme.overlayTypes.lightsred].Visibility = System.Windows.Visibility.Hidden;
                            themeImages[(int)Theme.overlayTypes.lightsgreen].Visibility = System.Windows.Visibility.Visible;
                        }
                    }
                    //sessionstateText.Content = timer.TotalSeconds.ToString() + SharedData.sessions[SharedData.currentSession].state.ToString();
                }

                //if (SharedData.sessionsUpdated)
                //{
                    // flags
                    foreach (int flag in flags)
                        if (themeImages[flag] != null)
                            themeImages[flag].Visibility = System.Windows.Visibility.Hidden; // reset

                    if (SharedData.visible[(int)SharedData.overlayObjects.flag])
                    {
                        if (SharedData.sessions[SharedData.currentSession].state == iRacingTelem.eSessionState.kSessionStateRacing)
                        {
                            if (SharedData.sessions[SharedData.currentSession].flag == iRacingTelem.eSessionFlag.kFlagYellow)
                                themeImages[(int)Theme.overlayTypes.flagyellow].Visibility = System.Windows.Visibility.Visible;
                            else if (SharedData.sessions[SharedData.currentSession].lapsRemaining == 1)
                                themeImages[(int)Theme.overlayTypes.flagwhite].Visibility = System.Windows.Visibility.Visible;
                            else if (SharedData.sessions[SharedData.currentSession].lapsRemaining <= 0)
                                themeImages[(int)Theme.overlayTypes.flagcheckered].Visibility = System.Windows.Visibility.Visible;
                            else
                                themeImages[(int)Theme.overlayTypes.flaggreen].Visibility = System.Windows.Visibility.Visible;
                        }
                        else if (SharedData.sessions[SharedData.currentSession].state == iRacingTelem.eSessionState.kSessionStateCheckered ||
                            SharedData.sessions[SharedData.currentSession].state == iRacingTelem.eSessionState.kSessionStateCoolDown)
                            themeImages[(int)Theme.overlayTypes.flagcheckered].Visibility = System.Windows.Visibility.Visible;
                    }
                //}

                if (SharedData.visible[(int)SharedData.overlayObjects.ticker])
                {
                    Thickness scroller;

                    if (tickerStackPanel.Margin.Left + tickerStackPanel.ActualWidth <= 0) // ticker is hidden
                    {
                        int itemcount = SharedData.standing[SharedData.currentSession].Length;
                        if (itemcount != (tickerStackPanel.Children.Count / 3))
                        {
                            tickerStackPanel.Children.Clear();

                            tickerPosLabel = new Label[itemcount];
                            tickerNameLabel = new Label[itemcount];
                            tickerDiffLabel = new Label[itemcount];

                            for (int i = 0; i < itemcount; i++)
                            {
                                tickerPosLabel[i] = DrawLabel(theme.ticker.Num);
                                tickerNameLabel[i] = DrawLabel(theme.ticker.Name);
                                tickerDiffLabel[i] = DrawLabel(theme.ticker.Diff);

                                tickerPosLabel[i].Width = Double.NaN;
                                tickerNameLabel[i].Width = Double.NaN;
                                tickerDiffLabel[i].Width = Double.NaN;

                                tickerStackPanel.Children.Add(tickerPosLabel[i]);
                                tickerStackPanel.Children.Add(tickerNameLabel[i]);
                                tickerStackPanel.Children.Add(tickerDiffLabel[i]);
                                
                                // initial data
                                tickerPosLabel[i].Content = (i + 1).ToString();
                                tickerNameLabel[i].Content = SharedData.drivers[SharedData.standing[SharedData.currentSession][i].id].name;
                                tickerDiffLabel[i].Content = SharedData.standing[SharedData.currentSession][i].diff;

                                // fixed widths to prevent changes
                                //tickerPosLabel[i].Width = tickerPosLabel[i].ActualWidth;
                                //tickerNameLabel[i].Width = tickerNameLabel[i].ActualWidth;
                                //tickerDiffLabel[i].Width = tickerDiffLabel[i].ActualWidth;
                            }

                        }

                        // move ticker for new scroll
                        scroller = tickerStackPanel.Margin;
                        scroller.Left = theme.width;
                        tickerStackPanel.Margin = scroller;
                    }
                    else // ticker visible
                    {
                        for (int i = 0; i < (tickerStackPanel.Children.Count / 3); i++) // update data
                        {
                            /*
                            tickerPosLabel[i].Content = (i + 1).ToString();
                            tickerNameLabel[i].Content = SharedData.drivers[SharedData.standing[SharedData.currentSession][i].id].name;
                            tickerDiffLabel[i].Content = SharedData.standing[SharedData.currentSession][i].diff;
                            */

                            tickerPosLabel[i].Content = (i + 1).ToString();
                            tickerNameLabel[i].Content = String.Format(theme.ticker.Name.text, theme.getFormats(SharedData.drivers[SharedData.standing[SharedData.currentSession][i].id]));

                            if (SharedData.sessions[SharedData.currentSession].type == iRacingTelem.eSessionType.kSessionTypeRace)
                            {
                                if (SharedData.drivers[SharedData.standing[SharedData.currentSession][i].id].onTrack == false && allowRetire) // out
                                    tickerDiffLabel[i].Content = theme.translation["out"];
                                else if (i == 0)
                                    if (SharedData.sidepanelType == SharedData.sidepanelTypes.fastlap)
                                        tickerDiffLabel[i].Content = floatTime2String(SharedData.standing[SharedData.currentSession][0].fastLap, true, false);
                                    else
                                        tickerDiffLabel[i].Content = Math.Floor(SharedData.standing[SharedData.currentSession][0].completedLaps) + " " + theme.translation["laps"];
                                else if (SharedData.standing[SharedData.currentSession][i].lapDiff > 0) // lapped
                                    tickerDiffLabel[i].Content = theme.translation["behind"] + SharedData.standing[SharedData.currentSession][i].lapDiff + theme.translation["lap"];
                                else
                                { // normal
                                    if (SharedData.sidepanelType == SharedData.sidepanelTypes.fastlap)
                                        tickerDiffLabel[i].Content = floatTime2String(SharedData.standing[SharedData.currentSession][i].fastLap, true, false);
                                    else
                                        tickerDiffLabel[i].Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].diff - SharedData.standing[SharedData.currentSession][0].diff, true, false);
                                }
                            }
                            else if (i == 0)
                                tickerDiffLabel[i].Content = floatTime2String(SharedData.standing[SharedData.currentSession][0].fastLap, true, false);
                            else
                            {
                                if (SharedData.sidepanelType == SharedData.sidepanelTypes.fastlap)
                                    tickerDiffLabel[i].Content = floatTime2String(SharedData.standing[SharedData.currentSession][i].fastLap, true, false);
                                else
                                    tickerDiffLabel[i].Content = theme.translation["behind"] + floatTime2String(SharedData.standing[SharedData.currentSession][i].fastLap - SharedData.standing[SharedData.currentSession][0].fastLap, true, false);
                            }
                        }

                        // scroll
                        scroller = tickerStackPanel.Margin;
                        scroller.Left -= Properties.Settings.Default.TickerSpeed;
                        tickerStackPanel.Margin = scroller;
                    }
                }

                /*
                // reset
                SharedData.standingsUpdated = false;
                SharedData.sessionsUpdated = false;
                SharedData.driversUpdated = false;
                SharedData.refreshOverlay = false;
                */
            }
        }

        public static string floatTime2String(float time, Boolean showMilli, Boolean showMinutes)
        {
            time = Math.Abs(time);

            int hours = (int)Math.Floor(time / 3600);
            int minutes = (int)Math.Floor((time - (hours * 3600)) / 60);
            int seconds = (int)Math.Floor(time % 60);
            int microseconds = (int)Math.Round(time * 1000 % 1000, 3);
            string output;

            if (hours > 0)
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
        
        private Label DrawLabel(Canvas canvas, Theme.LabelProperties prop)
        {
            Label label = new Label();
            label.Width = prop.width;
            label.Height = prop.height;
            label.Foreground = prop.fontColor;
            label.Margin = new Thickness(prop.left, prop.top, 0, 0);
            label.FontSize = prop.fontSize;
            label.FontFamily = prop.font;
            label.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;
            
            label.FontWeight = prop.FontBold;
            label.FontStyle = prop.FontItalic;

            label.HorizontalContentAlignment = prop.TextAlign;

            Canvas.SetZIndex(label, 100);

            return label;
        }

        private Label DrawLabel(Theme.LabelProperties prop)
        {
            Label label = new Label();
            label.Width = prop.width;
            label.Height = prop.height;
            label.Foreground = prop.fontColor;
            label.Margin = new Thickness(prop.left, prop.top, 0, 0);
            label.FontSize = prop.fontSize;
            label.FontFamily = prop.font;
            label.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;

            label.FontWeight = prop.FontBold;
            label.FontStyle = prop.FontItalic;

            label.HorizontalContentAlignment = prop.TextAlign;

            //stackpanel.SetZIndex(label, 100);

            return label;
        }
    }
}