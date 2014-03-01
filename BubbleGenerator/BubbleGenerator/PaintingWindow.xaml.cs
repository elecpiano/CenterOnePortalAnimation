using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BubbleGenerator
{
    public partial class PaintingWindow : Window
    {
        private Random _Random = new Random();

        private int Boundary_Left = 100;
        private int Boundary_Top = 60;
        private int Boundary_Right = 1266;
        private int Boundary_Bottom = 700;
        private int Radius_Min = 12;
        private int Radius_Max = 120;

        private double IntersectionFactor = 0.8;
        private Dictionary<RectArea, SolidColorBrush> _BubbleQueue = new Dictionary<RectArea, SolidColorBrush>();

        private List<string> ColorSet = new List<string>();
        private List<SolidColorBrush> ColorSetBrush = new List<SolidColorBrush>();

        private Ellipse EllipseUnderMouse = null;

        public PaintingWindow()
        {
            InitializeComponent();
            PopulateColorSet();
        }

        private void UpdateFile(string param1)
        {
            var file = File.Open("bubbles.txt", FileMode.Create);
            file.Close();
            File.WriteAllText(@"bubbles.txt", param1);

            Clipboard.SetText(param1);
        }

        private void PopulateColorSet()
        {
            ColorSet.Add("#0171c5");//, blue
            ColorSet.Add("#692079");//, purple
            ColorSet.Add("#da1658");//, pink
            ColorSet.Add("#00bbf2");//, light blue
            ColorSet.Add("#bad709");//, light green
            ColorSet.Add("#009e49");//, green
            ColorSet.Add("#ffb700");//, orange
            //ColorSet.Add("#fff001");//, yellow

            ColorSetBrush.Add(new SolidColorBrush(Color.FromRgb(1, 113, 197)));
            ColorSetBrush.Add(new SolidColorBrush(Color.FromRgb(105, 32, 121)));
            ColorSetBrush.Add(new SolidColorBrush(Color.FromRgb(218, 22, 88)));
            ColorSetBrush.Add(new SolidColorBrush(Color.FromRgb(0, 187, 242)));
            ColorSetBrush.Add(new SolidColorBrush(Color.FromRgb(186, 215, 9)));
            ColorSetBrush.Add(new SolidColorBrush(Color.FromRgb(0, 158, 73)));
            ColorSetBrush.Add(new SolidColorBrush(Color.FromRgb(255, 183, 0)));
            //ColorSetBrush.Add(new SolidColorBrush(Color.FromRgb()));
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(canvas);
            int diameter = _Random.Next(Radius_Min, Radius_Max);
            var brush = ColorSetBrush[GetRandomBrushIndex(0, ColorSet.Count)];
            RectArea rect = new RectArea() { CenterX = point.X, CenterY = point.Y, Width = diameter, Height = diameter };
            _BubbleQueue.Add(rect, brush);

            Ellipse ellipse = new Ellipse() { Fill = brush, Width = diameter, Height = diameter };
            canvas.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, point.X - diameter * 0.5d);
            Canvas.SetTop(ellipse, point.Y - diameter * 0.5d);
            ellipse.Tag = rect;
            ellipse.MouseRightButtonDown += ellipse_MouseRightButtonDown;
            ellipse.MouseWheel += ellipse_MouseWheel;
            ellipse.MouseEnter += ellipse_MouseEnter;
            ellipse.MouseLeave += ellipse_MouseLeave;
            UpdateBubbleCount();
        }

        void ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            EllipseUnderMouse = null;
        }

        void ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            EllipseUnderMouse = sender as Ellipse;
        }

        void ellipse_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            RectArea rect = (RectArea)ellipse.Tag;
            int diameter = (int)rect.Width;
            Point centerPoint = new Point(rect.CenterX, rect.CenterY);

            int newDiameter = diameter + (int)(e.Delta * 0.1d);
            if (newDiameter > Radius_Min && newDiameter < Radius_Max)
            {
                diameter = newDiameter;
            }

            Canvas.SetLeft(ellipse, centerPoint.X - diameter * 0.5d);
            Canvas.SetTop(ellipse, centerPoint.Y - diameter * 0.5d);
            ellipse.Width = diameter;
            ellipse.Height = diameter;

            rect.Width = diameter;
            rect.Height = diameter;
        }

        void ellipse_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            RectArea rect = (RectArea)ellipse.Tag;
            var brush = ColorSetBrush[GetRandomBrushIndex(0, ColorSet.Count)];
            ellipse.Fill = brush;
            _BubbleQueue[rect] = brush;
        }

        int _PreviousBrushIndex = -1;
        private int GetRandomBrushIndex(int min, int max)
        {
            int result = -1;
            bool shouldGenerateAgain = true;
            while (shouldGenerateAgain)
            {
                result = _Random.Next(min, max);
                if (result == _PreviousBrushIndex)
                {
                    shouldGenerateAgain = true;
                }
                else
                {
                    shouldGenerateAgain = false;
                }
            }

            _PreviousBrushIndex = result;
            return result;
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb1 = new StringBuilder();

            int diameter = 0;
            int left = 0;
            int top = 0;
            int j = 0;

            sb1.Append("var bubblePositions = [");
            foreach (var rect in _BubbleQueue.Keys)
            {
                if (j > 0)
                {
                    sb1.Append(",");
                }

                diameter = (int)rect.Width;
                left = (int)(rect.CenterX - diameter * 0.5d);
                top = (int)(rect.CenterY - diameter * 0.5d);

                sb1.Append("[")
                    .Append(left.ToString()).Append(",")
                    .Append(top.ToString()).Append(",")
                    .Append(diameter.ToString()).Append(",")
                    .Append("'").Append(ColorSet[ColorSetBrush.IndexOf(_BubbleQueue[rect])]).Append("'")
                    .Append("]");

                j++;
            }

            sb1.Append("];");
            sb1.Append(System.Environment.NewLine);

            UpdateFile(sb1.ToString());

            MessageBox.Show("Done!");
        }


        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            _BubbleQueue.Clear();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (EllipseUnderMouse == null)
                {
                    return;
                }
                canvas.Children.Remove(EllipseUnderMouse);
                _BubbleQueue.Remove((RectArea)EllipseUnderMouse.Tag);
                UpdateBubbleCount();
            }
        }

        private void UpdateBubbleCount()
        {
            progressTextblock.Text = _BubbleQueue.Count.ToString();
        }

        private void SetImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strImage = "";
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "所有文本文件(*.jpg;*.gif;*.png)|*.jpg;*.gif;*.png";
                if (dlg.ShowDialog() == true)
                {
                    strImage = dlg.FileName;
                }
                if (strImage != "")
                {
                    BitmapImage image = new BitmapImage(new Uri(strImage, UriKind.Absolute));
                    backgroundImage.Source = image;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
