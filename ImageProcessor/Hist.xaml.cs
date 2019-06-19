using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageProcessor
{
    /// <summary>
    /// Interaction logic for Hist.xaml
    /// </summary>
    public partial class Hist : Window
    {
        public Hist(double[] hists, Brush brush)
        {
            InitializeComponent();
            var zoom = 1 / hists.Max();
            Title += $" (x{zoom})";
            for (var i = 0; i < hists.Length; i++)
            {
                HistCanvas.Children.Add(new Line
                {
                    StrokeThickness = 1,
                    Stroke = brush,
                    X1 = i,
                    X2 = i,
                    Y1 = 400 - hists[i] * 400 * zoom,
                    Y2 = 400
                });
            }
        }
    }
}
