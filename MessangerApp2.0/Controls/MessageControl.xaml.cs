using System;
using System.Windows;
using System.Windows.Controls;

namespace MessangerApp2._0
{
    /// <summary>
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        public struct Data
        {
            public string Text;
            public DateTime DateTime;
            public string Sender;
            public bool IsRightSide;
            public Data(string Text, DateTime DateTime, string Sender, bool IsRightSide)
            {
                this.Text = Text;
                this.DateTime = DateTime;
                this.Sender = Sender;
                this.IsRightSide = IsRightSide;
            }
        }
        public Data data;
        public MessageControl(MessageControl control)
        {
            InitializeComponent();
            Initialize(control.data.Text, control.data.DateTime, control.data.Sender, control.data.IsRightSide);
        }
        
        public MessageControl(string text, DateTime dateTime, string sender, bool isRightSide)
        {
            InitializeComponent();
            Initialize(text,dateTime,sender,isRightSide);

        }
        public void Initialize(string text, DateTime dateTime, string sender, bool isRightSide)
        {
            Grid.SetColumn(this, isRightSide ? 1 : 0);
            data = new Data(text, dateTime, sender, isRightSide);
            Margin = new Thickness(10, 10, 10, 10);
            if (isRightSide)
            {
                DateText.HorizontalAlignment = HorizontalAlignment.Right;
                DateText.HorizontalContentAlignment = HorizontalAlignment.Right;
                MessageText.HorizontalAlignment = HorizontalAlignment.Right;
                MessageText.TextAlignment = TextAlignment.Right;
                Border.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                DateText.HorizontalAlignment = HorizontalAlignment.Left;
                MessageText.HorizontalAlignment = HorizontalAlignment.Left;
                DateText.HorizontalContentAlignment = HorizontalAlignment.Left;
                MessageText.TextAlignment = TextAlignment.Left;
                Border.HorizontalAlignment = HorizontalAlignment.Left;
            }
            DateText.Content = dateTime.ToShortTimeString();
            MessageText.Text = text;
        }
        private void MessageText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Border.Width = Math.Max(MessageText.ActualWidth + 20, DateText.ActualWidth + 20);
            Border.Height = MessageText.ActualHeight + 45;
            this.Height = MessageText.ActualHeight + 45;
        }

    }
}
