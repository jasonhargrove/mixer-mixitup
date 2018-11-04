﻿using Mixer.Base.Model.Chat;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Themes;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatMessageControl.xaml
    /// </summary>
    public partial class ChatMessageControl : UserControl
    {
        public ChatMessageViewModel Message { get; private set; }

        private ChatMessageHeaderControl messageHeader;
        private List<TextBlock> textBlocks = new List<TextBlock>();

        public ChatMessageControl(ChatMessageViewModel message)
        {
            this.Loaded += ChatMessageControl_Loaded;

            InitializeComponent();

            this.DataContext = this.Message = message;
            this.messageHeader = new ChatMessageHeaderControl(this.Message);
        }

        private void ChatMessageControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.MessageWrapPanel.Children.Clear();

            if (!this.Message.IsAlertMessage)
            {
                this.MessageWrapPanel.Children.Add(this.messageHeader);
            }

            foreach (ChatMessageDataModel messageData in this.Message.MessageComponents)
            {
                EmoticonImage emoticon = ChannelSession.GetEmoticonForMessage(messageData);
                if (emoticon != null)
                {
                    EmoticonControl image = new EmoticonControl(emoticon);
                    this.MessageWrapPanel.Children.Add(image);
                }
                else
                {
                    foreach (string word in messageData.text.Split(new string[] { " " }, StringSplitOptions.None))
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = word + " ";
                        textBlock.VerticalAlignment = VerticalAlignment.Center;
                        if (this.Message.IsAlertMessage)
                        {
                            textBlock.FontWeight = FontWeights.Bold;
                            if (!string.IsNullOrEmpty(this.Message.AlertMessageBrush) && !this.Message.AlertMessageBrush.Equals(ColorSchemes.DefaultColorScheme))
                            {
                                textBlock.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(this.Message.AlertMessageBrush));
                            }
                            else
                            {
                                textBlock.Foreground = (App.AppSettings.IsDarkBackground) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                            }
                        }
                        if (this.Message.IsWhisper || (messageData.type == "tag" && word.Equals("@" + ChannelSession.User.username, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            textBlock.Background = (Brush)FindResource("PrimaryHueLightBrush");
                            textBlock.Foreground = (Brush)FindResource("PrimaryHueLightForegroundBrush");
                        }
                        this.textBlocks.Add(textBlock);
                        this.MessageWrapPanel.Children.Add(textBlock);
                    }
                }
            }

            this.UpdateSizing();

            if (!string.IsNullOrEmpty(this.Message.ModerationReason))
            {
                this.DeleteMessage();
            }
        }

        public void DeleteMessage(string deletedBy = null)
        {
            this.Dispatcher.Invoke(() =>
            {
                TextBlock textBlock = new TextBlock();
                textBlock.VerticalAlignment = VerticalAlignment.Center;

                this.messageHeader.DeleteMessage();

                if (!string.IsNullOrEmpty(deletedBy))
                {
                    string text = " (Deleted By: " + deletedBy + ")";
                    textBlock.Text += text;
                    this.Message.AddToMessage(text);
                    this.Message.DeletedBy = deletedBy;
                }

                if (this.Message.IsAlertMessage)
                {
                    textBlock.FontWeight = FontWeights.Bold;
                    if (!string.IsNullOrEmpty(this.Message.AlertMessageBrush) && !this.Message.AlertMessageBrush.Equals(ColorSchemes.DefaultColorScheme))
                    {
                        textBlock.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(this.Message.AlertMessageBrush));
                    }
                    else
                    {
                        textBlock.Foreground = (App.AppSettings.IsDarkBackground) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                    }
                }

                this.Message.IsDeleted = true;
                foreach (TextBlock tb in this.textBlocks)
                {
                    tb.TextDecorations = TextDecorations.Strikethrough;
                }

                this.textBlocks.Add(textBlock);
                this.MessageWrapPanel.Children.Add(textBlock);

                if (!string.IsNullOrEmpty(this.Message.ModerationReason))
                {
                    textBlock = new TextBlock();
                    textBlock.VerticalAlignment = VerticalAlignment.Center;

                    string text = " (Auto-Moderated: " + this.Message.ModerationReason + ")";
                    textBlock.Text += text;
                    this.Message.AddToMessage(text);

                    this.textBlocks.Add(textBlock);
                    this.MessageWrapPanel.Children.Add(textBlock);
                }
            });
        }

        public void UpdateSizing()
        {
            this.messageHeader.UpdateSizing();
            foreach (var item in this.MessageWrapPanel.Children)
            {
                if (item is TextBlock)
                {
                    TextBlock textBlock = (TextBlock)item;
                    textBlock.FontSize = ChannelSession.Settings.ChatFontSize;
                }
                else if (item is EmoticonControl)
                {
                    EmoticonControl emoticon = (EmoticonControl)item;
                    emoticon.Height = emoticon.Width = ChannelSession.Settings.ChatFontSize + 2;
                }
            }
        }
    }
}
