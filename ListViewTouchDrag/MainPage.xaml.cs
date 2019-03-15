using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ListViewTouchDrag
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PointerPoint pointer;
        private bool isDragging;
        private PointerPoint startingPointer;
        private ScrollViewer scrollviewer;

        private const double RadiansThreshold = Math.PI * 2 * 80 / 360;  // 20 degree angle cone in both vertical directions (160 degrees on each side)
        private const double DistanceThreshold = 50;

        public List<string> Items { get; }
        public MainPage()
        {
            this.InitializeComponent();

            Items = new List<string>();
            for (var i = 1; i < 51; i++)
                Items.Add(i.ToString());

            listView.ChoosingItemContainer += ListView_ChoosingItemContainer;
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            scrollviewer = GetVisualChild<ScrollViewer>(listView);
            scrollviewer.PointerPressed += Scrollviewer_PointerPressed;
        }

        private void Scrollviewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Scrollviewer pressed");
        }

        private void ListView_ChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            Debug.WriteLine(args.ItemContainer);
            var container = new ListViewItem();
            //container.PointerPressed += Container_PointerPressed;
            //container.PointerMoved += Container_PointerMoved;
            //container.PointerReleased += Container_PointerReleased;
           
            container.DragStarting += Container_DragStarting;
            container.DropCompleted += Container_DropCompleted;
            args.ItemContainer = container;

        }

        //private void Container_ManipulationStarted1(object sender, ManipulationStartedRoutedEventArgs e)
        //{
        //    Debug.WriteLine("Manipulation started");
        //}

        private void Container_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            isDragging = false;
            startingPointer = null;
            Debug.WriteLine("PointerReleased");
        }

        private void Container_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            isDragging = false;
            startingPointer = null;
            Debug.WriteLine("DropCompleted");
        }


        private void Container_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            var deferral = args.GetDeferral();
            var container = (ListViewItem)sender;
            args.Data.SetData("number", container.Content);
            deferral.Complete();
        }

        private async void Container_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!isDragging)
            {
                if (startingPointer == null)
                {
                    Debug.WriteLine("Starting set");
                    startingPointer = e.GetCurrentPoint(this);
                    return;
                }
                
                var movedPoint = e.GetCurrentPoint(this);

                //detect whether still scrolling or moving vertical and want to tear off item
                if (IsDraggingVertically(startingPointer, movedPoint))
                {
                    Debug.WriteLine("Passed distance check... dragging!");
                    isDragging = true;
                    var container = (ListViewItem)sender;
                    var package = await container.StartDragAsync(movedPoint);
                }
            }
        }

        private void Container_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Pressed!");
            startingPointer = e.GetCurrentPoint(this);
        }

        private void SwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            Debug.WriteLine("Hey we should start dragging.");
        }

        public bool IsDraggingVertically(PointerPoint origin, PointerPoint moved)
        {
            var x = moved.Position.X - origin.Position.X;
            var y = moved.Position.Y - origin.Position.Y;

            var radians = Math.Atan2(y, x);
            Debug.WriteLine(radians);
            if (radians > RadiansThreshold || radians < -RadiansThreshold)
            {
                Debug.WriteLine("Passed radians check");
                //check distance now
                var distance = Math.Sqrt((x * x) + (y * y));
                Debug.WriteLine(distance);
                if (distance > DistanceThreshold)
                    return true;
                else return false;
            }
            else
                return false;
        }

        public T GetVisualChild<T>(DependencyObject o) where T: DependencyObject
        {
            if (o is T)
            {
                return (T)o;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);
                var result = GetVisualChild<T>(child);
                if (result == null)
                    continue;
                else
                    return result;
            }
            return null;
        }

        private async void Container_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var container = (ListViewItem)sender;
            //detect whether still scrolling or moving vertical and want to tear off item
            
        }

        

        //private void Container_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        //{
        //    var container = (ListViewItem)sender;
        //    var pointerPoint = PointerPoint.GetCurrentPoint()
        //    container.StartDragAsync(e.Pivot.Center)
        //}
    }
}
