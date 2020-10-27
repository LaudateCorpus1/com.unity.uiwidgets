using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class PageController : ScrollController {
        public PageController(
            int initialPage = 0,
            bool keepPage = true,
            float viewportFraction = 1.0f
        ) {
            this.initialPage = initialPage;
            this.keepPage = keepPage;
            this.viewportFraction = viewportFraction;
            D.assert(viewportFraction > 0.0);
        }

        public readonly int initialPage;

        public readonly bool keepPage;

        public readonly float viewportFraction;


        public virtual float page {
            get {
                D.assert(positions.isNotEmpty(),
                    () => "PageController.page cannot be accessed before a PageView is built with it."
                );
                D.assert(positions.Count == 1,
                    () => "The page property cannot be read when multiple PageViews are attached to " +
                          "the same PageController."
                );
                _PagePosition position = (_PagePosition) this.position;
                return position.page;
            }
        }

        public Future animateToPage(int page, TimeSpan duration, Curve curve) {
            _PagePosition position = (_PagePosition) this.position;
            return position.animateTo(
                position.getPixelsFromPage(page),
                duration,
                curve
            );
        }

        public void jumpToPage(int page) {
            _PagePosition position = (_PagePosition) this.position;
            position.jumpTo(position.getPixelsFromPage(page));
        }

        public Future nextPage(TimeSpan duration, Curve curve) {
            return animateToPage(page.round() + 1, duration: duration, curve: curve);
        }

        public Future previousPage(TimeSpan duration, Curve curve) {
            return animateToPage(page.round() - 1, duration: duration, curve: curve);
        }

        public override ScrollPosition createScrollPosition(ScrollPhysics physics, ScrollContext context,
            ScrollPosition oldPosition) {
            return new _PagePosition(
                physics: physics,
                context: context,
                initialPage: initialPage,
                keepPage: keepPage,
                viewportFraction: viewportFraction,
                oldPosition: oldPosition
            );
        }

        public override void attach(ScrollPosition position) {
            base.attach(position);
            _PagePosition pagePosition = (_PagePosition) position;
            pagePosition.viewportFraction = viewportFraction;
        }
    }

    public interface IPageMetrics : ScrollMetrics {
        float page { get; }
        float viewportFraction { get; }
    }

    public class PageMetrics : FixedScrollMetrics, IPageMetrics {
        public PageMetrics(
            float minScrollExtent = 0.0f,
            float maxScrollExtent = 0.0f,
            float pixels = 0.0f,
            float viewportDimension = 0.0f,
            AxisDirection axisDirection = AxisDirection.down,
            float viewportFraction = 0.0f
        ) : base(
            minScrollExtent: minScrollExtent,
            maxScrollExtent: maxScrollExtent,
            pixels: pixels,
            viewportDimension: viewportDimension,
            axisDirection: axisDirection
        ) {
            _viewportFraction = viewportFraction;
        }

        public readonly float _viewportFraction;

        public float page {
            get {
                return (Mathf.Max(0.0f, pixels.clamp(minScrollExtent, maxScrollExtent)) /
                        Mathf.Max(1.0f, viewportDimension * viewportFraction));
            }
        }

        public float viewportFraction {
            get { return _viewportFraction; }
        }
    }

    class _PagePosition : ScrollPositionWithSingleContext, IPageMetrics {
        internal _PagePosition(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            int initialPage = 0,
            bool keepPage = true,
            float viewportFraction = 1.0f,
            ScrollPosition oldPosition = null
        ) :
            base(
                physics: physics,
                context: context,
                initialPixels: null,
                keepScrollOffset: keepPage,
                oldPosition: oldPosition
            ) {
            D.assert(viewportFraction > 0.0);
            this.initialPage = initialPage;
            _viewportFraction = viewportFraction;
            _pageToUseOnStartup = initialPage;
        }

        public readonly int initialPage;
        float _pageToUseOnStartup;

        public float viewportFraction {
            get { return _viewportFraction; }
            set {
                if (_viewportFraction == value) {
                    return;
                }

                float oldPage = page;
                _viewportFraction = value;
                forcePixels(getPixelsFromPage(oldPage));
            }
        }

        float _viewportFraction;

        public float getPageFromPixels(float pixels, float viewportDimension) {
            return (Mathf.Max(0.0f, pixels) / Mathf.Max(1.0f, viewportDimension * viewportFraction));
        }

        public float getPixelsFromPage(float page) {
            return page * viewportDimension * viewportFraction;
        }

        public float page {
            get {
                return getPageFromPixels(pixels.clamp(minScrollExtent, maxScrollExtent),
                    viewportDimension);
            }
        }

        protected override void saveScrollOffset() {
            PageStorage.of(context.storageContext)?.writeState(context.storageContext,
                getPageFromPixels(pixels, viewportDimension));
        }

        protected override void restoreScrollOffset() {
            object value = PageStorage.of(context.storageContext)?.readState(context.storageContext);
            if (value != null) {
                _pageToUseOnStartup = (float) value;
            }
        }

        public override bool applyViewportDimension(float viewportDimension) {
            float oldViewportDimensions = 0.0f;
            if (haveDimensions) {
                oldViewportDimensions = this.viewportDimension;
            }

            bool result = base.applyViewportDimension(viewportDimension);
            float? oldPixels = null;
            if (havePixels) {
                oldPixels = pixels;
            }

            float page = (oldPixels == null || oldViewportDimensions == 0.0f)
                ? _pageToUseOnStartup
                : getPageFromPixels(oldPixels.Value, oldViewportDimensions);
            float newPixels = getPixelsFromPage(page);
            if (newPixels != oldPixels) {
                correctPixels(newPixels);
                return false;
            }

            return result;
        }
    }

    public class PageScrollPhysics : ScrollPhysics {
        public PageScrollPhysics(ScrollPhysics parent = null) : base(parent: parent) { }

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new PageScrollPhysics(parent: buildParent(ancestor));
        }

        float _getPage(ScrollPosition position) {
            if (position is _PagePosition) {
                return ((_PagePosition) position).page;
            }

            return position.pixels / position.viewportDimension;
        }

        float _getPixels(ScrollPosition position, float page) {
            if (position is _PagePosition) {
                return ((_PagePosition) position).getPixelsFromPage(page);
            }

            return page * position.viewportDimension;
        }

        float _getTargetPixels(ScrollPosition position, Tolerance tolerance, float velocity) {
            float page = _getPage(position);
            if (velocity < -tolerance.velocity) {
                page -= 0.5f;
            }
            else if (velocity > tolerance.velocity) {
                page += 0.5f;
            }

            return _getPixels(position, page.round());
        }

        public override Simulation createBallisticSimulation(ScrollMetrics position, float velocity) {
            if ((velocity <= 0.0 && position.pixels <= position.minScrollExtent) ||
                (velocity >= 0.0 && position.pixels >= position.maxScrollExtent)) {
                return base.createBallisticSimulation(position, velocity);
            }

            Tolerance tolerance = this.tolerance;
            float target = _getTargetPixels((ScrollPosition) position, tolerance, velocity);
            if (target != position.pixels) {
                return new ScrollSpringSimulation(spring, position.pixels, target, velocity, tolerance: tolerance);
            }

            return null;
        }

        public override bool allowImplicitScrolling {
            get { return false; }
        }
    }

    public static class PageViewUtils {
        internal static PageController _defaultPageController = new PageController();
        internal static PageScrollPhysics _kPagePhysics = new PageScrollPhysics();
    }


    public class PageView : StatefulWidget {
        public PageView(
            Key key = null,
            Axis scrollDirection = Axis.horizontal,
            bool reverse = false,
            PageController controller = null,
            ScrollPhysics physics = null,
            bool pageSnapping = true,
            ValueChanged<int> onPageChanged = null,
            List<Widget> children = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            IndexedWidgetBuilder itemBuilder = null,
            SliverChildDelegate childDelegate = null,
            int itemCount = 0
        ) : base(key: key) {
            this.scrollDirection = scrollDirection;
            this.reverse = reverse;
            this.physics = physics;
            this.pageSnapping = pageSnapping;
            this.onPageChanged = onPageChanged;
            this.dragStartBehavior = dragStartBehavior;
            this.controller = controller ?? PageViewUtils._defaultPageController;
            if (itemBuilder != null) {
                childrenDelegate = new SliverChildBuilderDelegate(itemBuilder, childCount: itemCount);
            }
            else if (childDelegate != null) {
                childrenDelegate = childDelegate;
            }
            else {
                childrenDelegate = new SliverChildListDelegate(children ?? new List<Widget>());
            }
        }
        
        public static PageView builder(
            IndexedWidgetBuilder itemBuilder,
            Key key = null,
            Axis scrollDirection = Axis.horizontal,
            bool reverse = false,
            PageController controller = null,
            ScrollPhysics physics = null,
            bool pageSnapping = true,
            ValueChanged<int> onPageChanged = null,
            int itemCount = 0,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) {
            return new PageView(
                itemBuilder: itemBuilder,
                key: key,
                scrollDirection: scrollDirection,
                reverse: reverse,
                controller: controller,
                physics: physics,
                pageSnapping: pageSnapping,
                onPageChanged: onPageChanged,
                itemCount: itemCount,
                dragStartBehavior: dragStartBehavior
            );
        }

        // TODO: PageView.custom

        public readonly Axis scrollDirection;

        public readonly bool reverse;

        public readonly PageController controller;

        public readonly ScrollPhysics physics;

        public readonly bool pageSnapping;

        public readonly ValueChanged<int> onPageChanged;

        public readonly SliverChildDelegate childrenDelegate;

        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _PageViewState();
        }
    }

    class _PageViewState : State<PageView> {
        int _lastReportedPage = 0;

        public override void initState() {
            base.initState();
            _lastReportedPage = widget.controller.initialPage;
        }

        AxisDirection _getDirection(BuildContext context) {
            switch (widget.scrollDirection) {
                case Axis.horizontal:
                    D.assert(WidgetsD.debugCheckHasDirectionality(context));
                    TextDirection textDirection = Directionality.of(context);
                    AxisDirection axisDirection = AxisUtils.textDirectionToAxisDirection(textDirection);
                    return widget.reverse ? AxisUtils.flipAxisDirection(axisDirection) : axisDirection;
                case Axis.vertical:
                    return widget.reverse ? AxisDirection.up : AxisDirection.down;
            }

            throw new UIWidgetsError("fail to get axis direction");
        }

        public override Widget build(BuildContext context) {
            AxisDirection axisDirection = _getDirection(context);
            ScrollPhysics physics = widget.pageSnapping
                ? PageViewUtils._kPagePhysics.applyTo(widget.physics)
                : widget.physics;

            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                    if (notification.depth == 0 && widget.onPageChanged != null &&
                        notification is ScrollUpdateNotification) {
                        IPageMetrics metrics = (IPageMetrics) notification.metrics;
                        int currentPage = metrics.page.round();
                        if (currentPage != _lastReportedPage) {
                            _lastReportedPage = currentPage;
                            widget.onPageChanged(currentPage);
                        }
                    }

                    return false;
                },
                child: new Scrollable(
                    dragStartBehavior: widget.dragStartBehavior,
                    axisDirection: axisDirection,
                    controller: widget.controller,
                    physics: physics,
                    viewportBuilder: (BuildContext _context, ViewportOffset position) => {
                        return new Viewport(
                            cacheExtent: 0.0f,
                            axisDirection: axisDirection,
                            offset: position,
                            slivers: new List<Widget> {
                                new SliverFillViewport(
                                    viewportFraction: widget.controller.viewportFraction,
                                    del: widget.childrenDelegate
                                )
                            }
                        );
                    }
                )
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new EnumProperty<Axis>("scrollDirection", widget.scrollDirection));
            description.add(new FlagProperty("reverse", value: widget.reverse, ifTrue: "reversed"));
            description.add(
                new DiagnosticsProperty<PageController>("controller", widget.controller, showName: false));
            description.add(new DiagnosticsProperty<ScrollPhysics>("physics", widget.physics, showName: false));
            description.add(new FlagProperty("pageSnapping", value: widget.pageSnapping,
                ifFalse: "snapping disabled"));
        }
    }
}