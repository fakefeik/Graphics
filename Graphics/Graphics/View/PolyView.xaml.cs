using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Graphics.ViewModel;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace Graphics.View
{
    /// <summary>
    /// Interaction logic for PolyView.xaml
    /// </summary>
    public partial class PolyView : UserControl
    {
        public PolyView()
        {
            InitializeComponent();
        }

        private void Grid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid.Focus();
        }
    }
}
