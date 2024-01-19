using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BaseUISupport.Helpers;

public static class ScrollViewerHelper
{
    // USE SHIFT TO SCROLL HORIZONTALLY WITH MOUSE WHEEL

    public static readonly DependencyProperty ShiftWheelScrollsHorizontallyProperty = DependencyProperty.RegisterAttached("ShiftWheelScrollsHorizontally", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(false, ShiftWheelScrollsHorizontallyChangedCallback));
    public static void SetShiftWheelScrollsHorizontally(UIElement element, bool value) => element.SetValue(ShiftWheelScrollsHorizontallyProperty, value);
    public static bool GetShiftWheelScrollsHorizontally(UIElement element) => (bool)element.GetValue(ShiftWheelScrollsHorizontallyProperty);

    private static void ShiftWheelScrollsHorizontallyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) throw new Exception("Attached property must be used with UIElement.");

        if ((bool)e.NewValue) element.PreviewMouseWheel += OnPreviewMouseWheel;
        else element.PreviewMouseWheel -= OnPreviewMouseWheel;
    }

    private static void OnPreviewMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        try
        {
            ScrollViewer? scrollViewer = e.Source switch
            {
                ScrollViewer s => s,
                UIElement u => u.FindParent<ScrollViewer>(),
                _ => null,
            };

            if (scrollViewer is not null)
            {
                if (scrollViewer.ScrollableWidth > 0)
                {
                    if ((scrollViewer.ScrollableHeight > 0 && Keyboard.Modifiers == ModifierKeys.Shift)
                        || scrollViewer.ScrollableHeight == 0)
                    {
                        double scroll = Math.Round(10 * scrollViewer.PanningRatio);
                        var lines = 0;

                        if (e.Delta < 0)
                        {
                            while (lines < scroll)
                            {
                                scrollViewer.LineRight();
                                lines++;
                            }
                        }
                        else
                        {
                            while (lines < scroll)
                            {
                                scrollViewer.LineLeft();
                                lines++;
                            }
                        }
                        e.Handled = true;
                        return;
                    }
                }
            }

            e.Handled = false;
            return;
        }
        catch (Exception) { }
    }


    /*
     * 
     * 
     *      THIS BREAKS EVERYTHING:
     *      NICE, BUT GLITCHY AS HELL
     * 
     * 
     * 

    // FADE SCROLLABLE EDGES

    public static readonly DependencyProperty FadeScrollableEdgesProperty = DependencyProperty.RegisterAttached("FadeScrollableEdges", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(false, FadeScrollableEdgesChangedCallback));

    private static void FadeScrollableEdgesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) throw new Exception("Attached property must be used with UIElement.");

        if ((bool)e.NewValue)
        {
            if (element is ScrollViewer scroll && !Tracked.ContainsKey(scroll))
            {
                Tracked.Add(scroll, new ScrollFade(scroll));
            }
        }
        else
        {
            if(element is ScrollViewer scroll && Tracked.ContainsKey(scroll))
            {
                Tracked[scroll].Dispose();
                Tracked.Remove(scroll);
            }
        }
    }
    public static void SetFadeScrollableEdges(UIElement element, bool value) => element.SetValue(FadeScrollableEdgesProperty, value);
    public static bool GetFadeScrollableEdges(UIElement element) => (bool)element.GetValue(FadeScrollableEdgesProperty);

    private static Dictionary<ScrollViewer, ScrollFade> Tracked = new Dictionary<ScrollViewer, ScrollFade>();


    class ScrollFade : IDisposable
    {
        private const string PART_SCROLL_PRESENTER_CONTAINER_NAME = "PART_FadeableContainer";

        public double FadedEdgeThickness;
        public double FadedEdgeFalloffSpeed;
        public double FadedEdgeOpacity;

        private BlurEffect InnerFadedBorderEffect;
        private Border InnerFadedBorder;
        private Border OuterFadedBorder;
        private ScrollViewer Scroll;



        public ScrollFade(ScrollViewer scroll)
        {
            FadedEdgeThickness = 20;
            FadedEdgeFalloffSpeed = 4.0;
            FadedEdgeOpacity = 0.0;
            Scroll = scroll;
            Scroll.ScrollChanged += Scroll_ScrollChanged;
            Scroll.SizeChanged += Scroll_SizeChanged;
            if(Scroll.IsLoaded)
            {
                Init();
            }
            else Scroll.Loaded += Scroll_Loaded;
        }

        public void Dispose()
        {
            RemoveOpacityMaskOfScrollContainer();
            Scroll.ScrollChanged -= Scroll_ScrollChanged;
            Scroll.SizeChanged -= Scroll_SizeChanged;
        }

        private void Scroll_Loaded(object? sender, RoutedEventArgs e)
        {
            Scroll.Loaded -= Scroll_Loaded;
            Init();
        }

        private void Init()
        {
            InnerFadedBorderEffect = new BlurEffect() { RenderingBias = RenderingBias.Performance }; InnerFadedBorder = new Border()
            {
                Background = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Effect = InnerFadedBorderEffect
            };
            byte opacity = (byte)(FadedEdgeOpacity * 255);
            OuterFadedBorder = new Border()
            {
                Background = new SolidColorBrush(Color.FromArgb(opacity, 0, 0, 0)),
                ClipToBounds = true,
                Child = InnerFadedBorder
            };
            SetOpacityMaskOfScrollContainer();
        }

        private void Scroll_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (InnerFadedBorder == null) return;
            var topOffset = CalculateNewMarginBasedOnOffsetFromEdge(Scroll.VerticalOffset); ;
            var bottomOffset = CalculateNewMarginBasedOnOffsetFromEdge(Scroll.ScrollableHeight - Scroll.VerticalOffset);
            var leftOffset = CalculateNewMarginBasedOnOffsetFromEdge(Scroll.HorizontalOffset);
            var rightOffset = CalculateNewMarginBasedOnOffsetFromEdge(Scroll.ScrollableWidth - Scroll.HorizontalOffset);
            InnerFadedBorder.Margin = new Thickness(leftOffset, topOffset, rightOffset, bottomOffset);
        }

        private double CalculateNewMarginBasedOnOffsetFromEdge(double edgeOffset)
        {
            var innerFadedBorderBaseMarginThickness = FadedEdgeThickness / 2.0;
            var calculatedOffset = (innerFadedBorderBaseMarginThickness) - (1.5 * (FadedEdgeThickness - (edgeOffset / FadedEdgeFalloffSpeed)));
            return Math.Min(innerFadedBorderBaseMarginThickness, calculatedOffset);
        }

        private void Scroll_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (OuterFadedBorder == null || InnerFadedBorder == null || InnerFadedBorderEffect == null) return;
            OuterFadedBorder.Width = e.NewSize.Width;
            OuterFadedBorder.Height = e.NewSize.Height;
            double thickness = FadedEdgeThickness / 2.0;
            InnerFadedBorder.Margin = new Thickness(thickness);
            InnerFadedBorderEffect.Radius = FadedEdgeThickness;
        }

        private void SetOpacityMaskOfScrollContainer()
        {
            if (Scroll.Template.FindName(PART_SCROLL_PRESENTER_CONTAINER_NAME, Scroll) is UIElement scroll_container)
            {
                scroll_container.OpacityMask = new VisualBrush() { Visual = OuterFadedBorder };
                Debug.WriteLine("Success");
            }
        }

        private void RemoveOpacityMaskOfScrollContainer()
        {
            if(Scroll.Template.FindName(PART_SCROLL_PRESENTER_CONTAINER_NAME, Scroll) is UIElement scroll_container)
            {
                scroll_container.OpacityMask = null;
            }
        }
    }
    */

    /*
    public static readonly DependencyProperty FadedEdgeThicknessProperty = DependencyProperty.RegisterAttached("FadedEdgeThickness", typeof(double), typeof(ScrollViewerHelper), new PropertyMetadata(20.0d, OnFadedEdgeThicknessChanged));
    public static void SetFadedEdgeThickness(ScrollViewer s, double value) => s.SetValue(FadedEdgeThicknessProperty, value);
    public static double GetFadedEdgeThickness(ScrollViewer s) => (double)s.GetValue(FadedEdgeThicknessProperty);

    public static readonly DependencyProperty FadedEdgeFalloffSpeedProperty = DependencyProperty.RegisterAttached("FadedEdgeFalloffSpeed", typeof(double), typeof(ScrollViewerHelper), new PropertyMetadata(4.0d, OnFadedEdgeFalloffSpeedChanged));
    public static void SetFadedEdgeFalloffSpeed(ScrollViewer s, double value) => s.SetValue(FadedEdgeFalloffSpeedProperty, value);
    public static double GetFadedEdgeFalloffSpeed(ScrollViewer s) => (double)s.GetValue(FadedEdgeFalloffSpeedProperty);

    public static readonly DependencyProperty FadedEdgeOpacityProperty = DependencyProperty.RegisterAttached("FadedEdgeOpacity", typeof(double), typeof(ScrollViewerHelper), new PropertyMetadata(0.0d, OnFadedEdgeOpacityChanged));
    public static void SetFadedEdgeOpacity(ScrollViewer s, double value) => s.SetValue(FadedEdgeOpacityProperty, value);
    public static double GetFadedEdgeOpacity(ScrollViewer s) => (double)s.GetValue(FadedEdgeOpacityProperty);


    private const string PART_SCROLL_PRESENTER_CONTAINER_NAME = "PART_ScrollContentPresenterContainer";

    private static Dictionary<ScrollViewer, FadeSettings> Settings = new Dictionary<ScrollViewer, FadeSettings>();

    public static void OnFadedEdgeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer scrollViewer) return;
        double edgeThickness = (double)e.NewValue;

        scrollViewer.ScrollChanged += FadingScrollViewer_ScrollChanged;
        scrollViewer.SizeChanged += FadingScrollViewer_SizeChanged;
        if (!Settings.ContainsKey(scrollViewer)) Settings.Add(scrollViewer, new FadeSettings());
        Settings[scrollViewer].FadedEdgeThickness = edgeThickness;
    }


    public static void OnFadedEdgeFalloffSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer scrollViewer) return;
        double edgeFalloffSpeed = (double)e.NewValue;
        if (!Settings.ContainsKey(scrollViewer)) Settings.Add(scrollViewer, new FadeSettings());
        Settings[scrollViewer].FadedEdgeFalloffSpeed = edgeFalloffSpeed;
    }


    public static void OnFadedEdgeOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer scrollViewer) return;
        double edgeOpacity = (double)e.NewValue;
        if (!Settings.ContainsKey(scrollViewer)) Settings.Add(scrollViewer, new FadeSettings());
        Settings[scrollViewer].FadedEdgeOpacity = edgeOpacity;
    }


    private static void FadingScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        ScrollViewer scrollViewer = sender as ScrollViewer;
        FadeSettings settings = Settings[scrollViewer];
        if (settings.InnerFadedBorder == null) return;

        var topOffset = CalculateNewMarginBasedOnOffsetFromEdge(scrollViewer, scrollViewer.VerticalOffset); ;
        var bottomOffset = CalculateNewMarginBasedOnOffsetFromEdge(scrollViewer, scrollViewer.ScrollableHeight - scrollViewer.VerticalOffset);
        var leftOffset = CalculateNewMarginBasedOnOffsetFromEdge(scrollViewer, scrollViewer.HorizontalOffset);
        var rightOffset = CalculateNewMarginBasedOnOffsetFromEdge(scrollViewer, scrollViewer.ScrollableWidth - scrollViewer.HorizontalOffset);

        settings.InnerFadedBorder.Margin = new Thickness(leftOffset, topOffset, rightOffset, bottomOffset);
    }

    private static void FadingScrollViewer_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ScrollViewer scrollViewer = sender as ScrollViewer;
        FadeSettings settings = Settings[scrollViewer];

        if (!settings.Initialized)
        {
            OnApplyTemplate(scrollViewer);
            settings.Initialized = true;
        }

        if (settings.OuterFadedBorder == null || settings.InnerFadedBorder == null || settings.InnerFadedBorderEffect == null) return;

        settings.OuterFadedBorder.Width = e.NewSize.Width;
        settings.OuterFadedBorder.Height = e.NewSize.Height;

        double innerFadedBorderBaseMarginThickness = settings.FadedEdgeThickness / 2.0;
        settings.InnerFadedBorder.Margin = new Thickness(innerFadedBorderBaseMarginThickness);
        settings.InnerFadedBorderEffect.Radius = settings.FadedEdgeThickness;
    }

    private static double CalculateNewMarginBasedOnOffsetFromEdge(ScrollViewer scrollViewer, double edgeOffset)
    {
        FadeSettings settings = Settings[scrollViewer];
        var innerFadedBorderBaseMarginThickness = settings.FadedEdgeThickness / 2.0;
        double calculatedOffset;
        if (edgeOffset == 0) calculatedOffset = -innerFadedBorderBaseMarginThickness;
        else calculatedOffset = (edgeOffset * settings.FadedEdgeFalloffSpeed) - innerFadedBorderBaseMarginThickness;
        return Math.Min(innerFadedBorderBaseMarginThickness, calculatedOffset);
    }

    public static void OnApplyTemplate(ScrollViewer scrollViewer)
    {
        BuildInnerFadedBorderEffectForOpacityMask(scrollViewer);
        BuildInnerFadedBorderForOpacityMask(scrollViewer);
        BuildOuterFadedBorderForOpacityMask(scrollViewer);
        SetOpacityMaskOfScrollContainer(scrollViewer);
    }

    private static void BuildInnerFadedBorderEffectForOpacityMask(ScrollViewer scrollViewer)
    {
        FadeSettings settings = Settings[scrollViewer];
        settings.InnerFadedBorderEffect = new BlurEffect()
        {
            RenderingBias = RenderingBias.Performance,
        };
    }

    private static void BuildInnerFadedBorderForOpacityMask(ScrollViewer scrollViewer)
    {
        FadeSettings settings = Settings[scrollViewer];
        settings.InnerFadedBorder = new Border()
        {
            Background = Brushes.Black,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
            Effect = settings.InnerFadedBorderEffect,
        };
    }

    private static void BuildOuterFadedBorderForOpacityMask(ScrollViewer scrollViewer)
    {
        FadeSettings settings = Settings[scrollViewer];
        byte fadedEdgeByteOpacity = (byte)(settings.FadedEdgeOpacity * 255);
        settings.OuterFadedBorder = new Border()
        {
            Background = new SolidColorBrush(Color.FromArgb(fadedEdgeByteOpacity, 0, 0, 0)),
            ClipToBounds = true,
            Child = settings.InnerFadedBorder,
        };
    }

    private static void SetOpacityMaskOfScrollContainer(ScrollViewer scrollViewer)
    {
        FadeSettings settings = Settings[scrollViewer];
        var opacityMaskBrush = new VisualBrush()
        {
            Visual = settings.OuterFadedBorder,
            Stretch = Stretch.None
        };
        var scrollContentPresentationContainer = scrollViewer.Template.FindName(PART_SCROLL_PRESENTER_CONTAINER_NAME, scrollViewer) as UIElement;
        if (scrollContentPresentationContainer == null) return;
        scrollContentPresentationContainer.OpacityMask = opacityMaskBrush;
    }


    protected class FadeSettings
    {
        public BlurEffect InnerFadedBorderEffect { get; set; }
        public Border InnerFadedBorder { get; set; }
        public Border OuterFadedBorder { get; set; }

        public double FadedEdgeThickness { get; set; }
        public double FadedEdgeFalloffSpeed { get; set; }
        public double FadedEdgeOpacity { get; set; }

        public bool Initialized { get; set; }

        public FadeSettings()
        {
            FadedEdgeThickness = 20.0d;
            FadedEdgeFalloffSpeed = 4.0d;
            FadedEdgeOpacity = 0.0d;
        }
    }*/
}
