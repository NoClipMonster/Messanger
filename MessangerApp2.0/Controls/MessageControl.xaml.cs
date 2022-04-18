using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            public string FileId = "";
            public Data(string Text, DateTime DateTime, string Sender, bool IsRightSide, string FileId = "")
            {
                this.Text = Text;
                this.DateTime = DateTime;
                this.Sender = Sender;
                this.IsRightSide = IsRightSide;
                this.FileId = FileId;
            }
        }
        public delegate void FileOpen(string FileId);
        public event FileOpen OnFileOpenClick;
        public Data data;
        public MessageControl(MessageControl control)
        {
            InitializeComponent();
            Initialize(control.data.Text, control.data.DateTime, control.data.Sender, control.data.IsRightSide,control.data.FileId);
        }
        
        public MessageControl(string text, DateTime dateTime, string sender, bool isRightSide, string FileId = "")
        {
            InitializeComponent();
            Initialize(text,dateTime,sender,isRightSide,FileId);

        }
        public void Initialize(string text, DateTime dateTime, string sender, bool isRightSide, string FileId = "")
        {
            Grid.SetColumn(this, isRightSide ? 1 : 0);
            data = new Data(text, dateTime, sender, isRightSide);
            Margin = new Thickness(10, 10, 10, 10);
            if (FileId != "")
            {
                MsGrid.Cursor = Cursors.Hand;
                MsGrid.MouseLeftButtonUp += MsGrid_MouseLeftButtonUp;
                Border.Background = new SolidColorBrush(new Color() { R = 55, G = 70, B = 100, A = 255 });
                data.FileId = FileId;
            }
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
            DateText.Content = sender+" "+ dateTime.ToShortTimeString();
            MessageText.Text = text;
        }

        private void MessageText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Border.Width = Math.Max(MessageText.ActualWidth + 10, DateText.ActualWidth + 10);
            Border.Height = MessageText.ActualHeight + 45;
            this.Height = MessageText.ActualHeight + 45;
        }

        private void MsGrid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnFileOpenClick(data.FileId);
        }
    }
}
