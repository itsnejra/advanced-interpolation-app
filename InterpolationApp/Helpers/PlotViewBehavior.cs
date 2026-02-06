using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using OxyPlot.Wpf;

namespace InterpolationApp.Helpers
{
    /// <summary>
    /// Behavior koji omogućava da tooltip ostane vidljiv duže vrijeme kada prevedeš mišem preko grafikona
    /// </summary>
    public class PersistentTrackerBehavior : Behavior<PlotView>
    {
        private bool _isMouseOver = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseLeave += OnMouseLeave;
            AssociatedObject.MouseMove += OnMouseMove;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseEnter -= OnMouseEnter;
            AssociatedObject.MouseLeave -= OnMouseLeave;
            AssociatedObject.MouseMove -= OnMouseMove;
            base.OnDetaching();
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOver = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseOver = false;
            // Sakrij tracker kada miš napusti PlotView
            AssociatedObject.HideTracker();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseOver)
            {
                // OxyPlot automatski pokazuje tracker na MouseMove
                // Ovdje možemo dodati dodatnu logiku ako je potrebno
            }
        }
    }
}
