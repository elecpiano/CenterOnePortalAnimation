using System;
using System.Collections.Generic;
using System.IO;
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

namespace BubbleGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random _Random = new Random();

        private int _BubbleCount = 50;

        private int Boundary_Left = 100;
        private int Boundary_Top = 60;
        private int Boundary_Right = 1266;
        private int Boundary_Bottom = 700;
        private int Radius_Min = 12;
        private int Radius_Max = 80;

        private double IntersectionFactor = 0.8;

        private List<string> ColorSet = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            PopulateColorSet();
        }

        private void UpdateFile(string param1, string param2)
        {
            var jsCode = File.ReadAllText(@"C:\Jason\Projects\Visitor Center\Testwork\Portal\BubbleColorHTML\TEMPLATE.js");
            jsCode = jsCode.Replace("###Param1", param1);
            jsCode = jsCode.Replace("###Param2", param2);
            File.WriteAllText(@"C:\Jason\Projects\Visitor Center\Testwork\Portal\BubbleColorHTML\ColorBubbleHTML_edge.js", jsCode);
        }

        private void PopulateColorSet()
        {
            ColorSet.Add("1,113,197,");//0171c5, blue
            ColorSet.Add("105,32,121,");//692079, purple
            ColorSet.Add("218,22,88,");//da1658, pink
            ColorSet.Add("0,187,242,");//00bbf2, light blue
            ColorSet.Add("186,215,9,");//bad709, light green
            ColorSet.Add("0,158,73,");//009e49, green
            ColorSet.Add("255,183,0,");//ffb700, orange
            //ColorSet.Add("255,240,1,");//fff001, yellow
        }

        private void LoadBubbleCount()
        {
            int bubbleCount = 100;
            if (int.TryParse(bubbleCountTextbox.Text, out bubbleCount))
            {
                _BubbleCount = bubbleCount;
            }
            else
            {
                bubbleCount = 100;
            }
        }

        List<Rect> _Bubbles = new List<Rect>();
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            progressTextblock.Text = "0";
            LoadBubbleCount();

            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            for (int i = 0; i < _BubbleCount; i++)
            {
                if (i > 0)
                {
                    sb1.Append(",");
                    sb1.AppendLine();
                }
                sb1.Append(@"{ id:'Ellipse")
                    .Append(i.ToString())
                    .Append(@"', type:'ellipse', rect:['0px','0px','0px','0px','auto','auto'], borderRadius:[""50%"",""50%"",""50%"",""50%""], fill:[""rgba(218,22,88,1.00)""], stroke:[0,""rgb(0, 0, 0)"",""none""]}");
            }

            _Bubbles.Clear();
            int diameter = 0;
            int left = 0;
            int top = 0;
            Rect newRect = new Rect();
            for (int i = 0; i < _BubbleCount; i++)
            {
                if (i > 0)
                {
                    sb2.Append(",");
                    sb2.AppendLine();
                }

                //int diameter = _Random.Next(Radius_Min, Radius_Max);
                //int left = _Random.Next(Boundary_Left, Boundary_Right);
                //int top = _Random.Next(Boundary_Top, Boundary_Bottom);
                //Rect newRect = new Rect() { X = left, Y = top, Width = diameter, Height = diameter };

        
                bool shouldGenerateNewRect = true;

                while (shouldGenerateNewRect)
                {
                    diameter = _Random.Next(Radius_Min, Radius_Max);
                    left = _Random.Next(Boundary_Left, Boundary_Right);
                    top = _Random.Next(Boundary_Top, Boundary_Bottom);
                    newRect = new Rect() { X = left, Y = top, Width = diameter, Height = diameter };

                    if (Conflicts(newRect))
                    {
                        shouldGenerateNewRect = true;
                    }
                    else
                    {
                        shouldGenerateNewRect = false;
                    }
                }

                sb2.Append(@"""${_Ellipse")
                    .Append(i.ToString())
                    .Append(@"}"": [[""color"", ""background-color"", 'rgba(")
                    //.Append(_Random.Next(1, 255).ToString()).Append(@",")
                    //.Append(_Random.Next(1, 255).ToString()).Append(@",")
                    //.Append(_Random.Next(1, 255).ToString()).Append(@",")
                    .Append(ColorSet[_Random.Next(0, ColorSet.Count)])
                    .Append(@"1.00)'], [""style"", ""width"", '")
                    .Append(diameter.ToString())
                    .Append(@"px'], [""style"", ""height"", '")
                    .Append(diameter.ToString())
                    .Append(@"px'], [""style"", ""left"", '")
                    .Append(left.ToString())
                    .Append(@"px'], [""style"", ""top"", '")
                    .Append(top.ToString())
                    .Append(@"px']]");

                _Bubbles.Add(newRect);

                progressTextblock.Text = (i+1).ToString();
                this.UpdateLayout();
            }

            UpdateFile(sb1.ToString(), sb2.ToString());
        }

        private bool Conflicts(Rect newRect)
        {
            int idx = 0;
            while (idx < _Bubbles.Count)
            {
                Rect existingRect = _Bubbles[idx];
                existingRect = new Rect() 
                { 
                    X = existingRect.X + existingRect.Width * 0.5d * (1 - IntersectionFactor),
                    Y = existingRect.Y + existingRect.Height * 0.5d * (1 - IntersectionFactor),  
                    Width = existingRect.Width * IntersectionFactor,
                    Height = existingRect.Height * IntersectionFactor
                };

                newRect = new Rect()
                {
                    X = newRect.X + newRect.Width * 0.5d * (1 - IntersectionFactor),
                    Y = newRect.Y + newRect.Height * 0.5d * (1 - IntersectionFactor),
                    Width = newRect.Width * IntersectionFactor,
                    Height = newRect.Height * IntersectionFactor
                };

                if (existingRect.IntersectsWith(newRect))
                {
                    return true;
                }
                idx++;
            }
            return false;
        }
    }
}
