using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Stream.Wall.Services
{
    public static class ScrollExtensions
    {
        public static ScrollViewer GetScrollViewer(this DependencyObject element)
        {
            if (element is ScrollViewer)
                return (ScrollViewer) element;
            for (int childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(element); ++childIndex)
            {
                ScrollViewer scrollViewer = VisualTreeHelper.GetChild(element, childIndex).GetScrollViewer();
                if (scrollViewer != null)
                    return scrollViewer;
            }
            return (ScrollViewer) null;
        }

        public static double DistanceFromTop(this ScrollViewer scrollViewer, UIElement element) => element.TransformToVisual((UIElement) scrollViewer).TransformPoint(new Point(0.0, 0.0)).Y;

        public static bool IsItemVisible(this FrameworkElement container, FrameworkElement element)
        {
            Rect rect1 = element.TransformToVisual((UIElement) container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect2 = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect1.Top < rect2.Bottom && rect1.Bottom > rect2.Top;
        }
    }
}