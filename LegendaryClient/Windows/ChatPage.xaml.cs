﻿using jabber.protocol.client;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ChatPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        private static System.Timers.Timer UpdateTimer;
        private LargeChatPlayer PlayerItem;
        private ChatPlayerItem LastPlayerItem;

        public ChatPage()
        {
            InitializeComponent();
            if (Properties.Settings.Default.StatusMsg != "Set your status message")
                StatusBox.Text = Properties.Settings.Default.StatusMsg;
            UpdateTimer = new System.Timers.Timer(1000);
            UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateChat);
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
        }

        private void PresenceChanger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresenceChanger.SelectedIndex != -1)
            {
                switch ((string)PresenceChanger.SelectedValue)
                {
                    case "Online":
                        Client.CurrentPresence = PresenceType.available;
                        break;

                    case "Invisible":
                        Client.CurrentPresence = PresenceType.invisible;
                        break;
                }
            }
        }

        private void UpdateChat(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (Client.CurrentStatus != StatusBox.Text && StatusBox.Text != "Set your status message")
                {
                    Client.CurrentStatus = StatusBox.Text;
                }
                else if (StatusBox.Text == "Set your status message")
                {
                    Client.CurrentStatus = "Online";
                }

                Properties.Settings.Default.StatusMsg = StatusBox.Text;
                Properties.Settings.Default.Save();

                if (Client.UpdatePlayers)
                {
                    Client.UpdatePlayers = false;

                    ChatListView.Items.Clear();
                    foreach (KeyValuePair<string, ChatPlayerItem> ChatPlayerPair in Client.AllPlayers.ToArray())
                    {
                        if (ChatPlayerPair.Value.Level != 0)
                        {
                            ChatPlayer player = new ChatPlayer();
                            player.Width = 250;
                            player.Tag = ChatPlayerPair.Value;
                            player.PlayerName.Content = ChatPlayerPair.Value.Username;
                            player.LevelLabel.Content = ChatPlayerPair.Value.Level;
                            player.PlayerStatus.Content = ChatPlayerPair.Value.Status;
                            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ChatPlayerPair.Value.ProfileIcon + ".png"), UriKind.RelativeOrAbsolute);
                            player.ProfileImage.Source = new BitmapImage(uriSource);
                            player.ContextMenu = (ContextMenu)Resources["PlayerChatMenu"];
                            player.MouseMove += ChatPlayerMouseOver;
                            player.MouseLeave += player_MouseLeave;
                            ChatListView.Items.Add(player);
                        }
                    }
                }
            }));
        }

        private void player_MouseLeave(object sender, MouseEventArgs e)
        {
            if (PlayerItem != null)
            {
                Client.MainGrid.Children.Remove(PlayerItem);
                PlayerItem = null;
            }
        }

        private void ChatPlayerMouseOver(object sender, MouseEventArgs e)
        {
            ChatPlayer item = (ChatPlayer)sender;
            ChatPlayerItem playerItem = (ChatPlayerItem)item.Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(PlayerItem);
                PlayerItem.Tag = playerItem;
                PlayerItem.PlayerName.Content = playerItem.Username;
                PlayerItem.PlayerLeague.Content = playerItem.LeagueTier + " " + playerItem.LeagueDivision;
                if (playerItem.RankedWins == 0)
                    PlayerItem.PlayerWins.Content = playerItem.Wins + " Normal Wins";
                else
                    PlayerItem.PlayerWins.Content = playerItem.RankedWins + " Ranked Wins";
                PlayerItem.LevelLabel.Content = playerItem.Level;
                PlayerItem.UsingLegendary.Visibility = playerItem.UsingLegendary ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", playerItem.ProfileIcon + ".png"), UriKind.RelativeOrAbsolute);
                PlayerItem.ProfileImage.Source = new BitmapImage(uriSource);
                if (playerItem.Status != null)
                {
                    PlayerItem.PlayerStatus.Text = playerItem.Status.Replace("∟", "");
                }
                else
                {
                    PlayerItem.PlayerStatus.Text = "";
                }
                PlayerItem.Width = 250;
                PlayerItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                PlayerItem.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }

            Point MouseLocation = e.GetPosition(Client.MainGrid);
            double YMargin = MouseLocation.Y;
            if (YMargin + 155 > Client.MainGrid.ActualHeight)
                YMargin = Client.MainGrid.ActualHeight - 155;
            PlayerItem.Margin = new Thickness(0, YMargin, 250, 0);
        }

        private void ChatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatListView.SelectedIndex != -1)
            {
                ChatPlayer player = (ChatPlayer)ChatListView.SelectedItem;
                ChatListView.SelectedIndex = -1;
                ChatPlayerItem playerItem = (ChatPlayerItem)player.Tag;
                LastPlayerItem = playerItem;
                foreach (NotificationChatPlayer x in Client.ChatListView.Items)
                {
                    if ((string)x.PlayerLabelName.Content == playerItem.Username)
                        return;
                }
                NotificationChatPlayer ChatPlayer = new NotificationChatPlayer();
                ChatPlayer.Tag = playerItem;
                ChatPlayer.Margin = new Thickness(1, 0, 1, 0);
                ChatPlayer.PlayerLabelName.Content = playerItem.Username;
                Client.ChatListView.Items.Add(ChatPlayer);
            }
        }

        private void ProfileItem_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new ProfilePage(LastPlayerItem.Username));
        }
    }
}