using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    static class AnimatedIconUtils {
        public static T _interpolate<T>(T[] values, float progress, _Interpolator<T> interpolator) {
            D.assert(progress <= 1.0f);
            D.assert(progress >= 0.0f);
            if (values.Length == 1) {
                return values[0];
            }

            float targetIdx = MathUtils.lerpNullableFloat(0, values.Length - 1, progress);
            int lowIdx = targetIdx.floor();
            int highIdx = targetIdx.ceil();
            float t = targetIdx - lowIdx;
            return interpolator(values[lowIdx], values[highIdx], t);
        }
    }

    public class AnimatedIcon : StatelessWidget {
        public AnimatedIcon(
            Key key = null,
            AnimatedIconData icon = null,
            Animation<float> progress = null,
            Color color = null,
            float? size = null
        ) : base(key: key) {
            D.assert(progress != null);
            D.assert(icon != null);
            this.progress = progress;
            this.color = color;
            this.size = size;
            this.icon = icon;
        }

        public readonly Animation<float> progress;

        public readonly Color color;

        public readonly float? size;

        public readonly AnimatedIconData icon;

        public static readonly _UiPathFactory _pathFactory = () => new Path();

        public override Widget build(BuildContext context) {
            _AnimatedIconData iconData = (_AnimatedIconData) icon;
            IconThemeData iconTheme = IconTheme.of(context);
            float iconSize = size ?? iconTheme.size ?? 0.0f;
            float? iconOpacity = iconTheme.opacity;
            Color iconColor = color ?? iconTheme.color;
            if (iconOpacity != 1.0f) {
                iconColor = iconColor.withOpacity(iconColor.opacity * (iconOpacity ?? 1.0f));
            }

            return new CustomPaint(
                size: new Size(iconSize, iconSize),
                painter: new _AnimatedIconPainter(
                    paths: iconData.paths,
                    progress: progress,
                    color: iconColor,
                    scale: iconSize / iconData.size.width,
                    uiPathFactory: _pathFactory
                )
            );
        }
    }

    public delegate Path _UiPathFactory();

    class _AnimatedIconPainter : AbstractCustomPainter {
        public _AnimatedIconPainter(
            _PathFrames[] paths = null,
            Animation<float> progress = null,
            Color color = null,
            float? scale = null,
            bool? shouldMirror = null,
            _UiPathFactory uiPathFactory = null
        ) : base(repaint: progress) {
            this.paths = paths;
            this.progress = progress;
            this.color = color;
            this.scale = scale;
            this.shouldMirror = shouldMirror;
            this.uiPathFactory = uiPathFactory;
        }

        public readonly _PathFrames[] paths;
        public readonly Animation<float> progress;
        public readonly Color color;
        public readonly float? scale;
        public readonly bool? shouldMirror;
        public readonly _UiPathFactory uiPathFactory;

        public override void paint(Canvas canvas, Size size) {
            canvas.scale(scale ?? 1.0f, scale ?? 1.0f);
            if (shouldMirror == true) {
                canvas.rotate(Mathf.PI);
                canvas.translate(-size.width, -size.height);
            }

            float clampedProgress = progress.value.clamp(0.0f, 1.0f);
            foreach (_PathFrames path in paths) {
                path.paint(canvas, color, uiPathFactory, clampedProgress);
            }
        }


        public override bool shouldRepaint(CustomPainter _oldDelegate) {
            _AnimatedIconPainter oldDelegate = _oldDelegate as _AnimatedIconPainter;
            return oldDelegate.progress.value != progress.value
                   || oldDelegate.color != color
                   || oldDelegate.paths != paths
                   || oldDelegate.scale != scale
                   || oldDelegate.uiPathFactory != uiPathFactory;
        }

        public override bool? hitTest(Offset position) {
            return null;
        }
    }

    class _PathFrames {
        public _PathFrames(
            _PathCommand[] commands,
            float[] opacities
        ) {
            this.commands = commands;
            this.opacities = opacities;
        }

        public readonly _PathCommand[] commands;
        public readonly float[] opacities;

        public void paint(Canvas canvas, Color color, _UiPathFactory uiPathFactory, float progress) {
            float opacity = AnimatedIconUtils._interpolate<float>(opacities, progress, MathUtils.lerpNullableFloat);
            Paint paint = new Paint();
            paint.style = PaintingStyle.fill;
            paint.color = color.withOpacity(color.opacity * opacity);
            Path path = uiPathFactory();
            foreach (_PathCommand command in commands) {
                command.apply(path, progress);
            }

            canvas.drawPath(path, paint);
        }
    }

    abstract class _PathCommand {
        public _PathCommand() {
        }

        public abstract void apply(Path path, float progress);
    }

    class _PathMoveTo : _PathCommand {
        public _PathMoveTo(Offset[] points) {
            this.points = points;
        }

        public readonly Offset[] points;

        public override void apply(Path path, float progress) {
            Offset offset = AnimatedIconUtils._interpolate<Offset>(points, progress, Offset.lerp);
            path.moveTo(offset.dx, offset.dy);
        }
    }

    class _PathCubicTo : _PathCommand {
        public _PathCubicTo(Offset[] controlPoints1, Offset[] controlPoints2, Offset[] targetPoints) {
            this.controlPoints1 = controlPoints1;
            this.controlPoints2 = controlPoints2;
            this.targetPoints = targetPoints;
        }

        public readonly Offset[] controlPoints2;
        public readonly Offset[] controlPoints1;
        public readonly Offset[] targetPoints;

        public override void apply(Path path, float progress) {
            Offset controlPoint1 = AnimatedIconUtils._interpolate<Offset>(controlPoints1, progress, Offset.lerp);
            Offset controlPoint2 = AnimatedIconUtils._interpolate<Offset>(controlPoints2, progress, Offset.lerp);
            Offset targetPoint = AnimatedIconUtils._interpolate<Offset>(targetPoints, progress, Offset.lerp);
            path.cubicTo(
                controlPoint1.dx, controlPoint1.dy,
                controlPoint2.dx, controlPoint2.dy,
                targetPoint.dx, targetPoint.dy
            );
        }
    }

    class _PathLineTo : _PathCommand {
        public _PathLineTo(Offset[] points) {
            this.points = points;
        }

        Offset[] points;

        public override void apply(Path path, float progress) {
            Offset point = AnimatedIconUtils._interpolate<Offset>(points, progress, Offset.lerp);
            path.lineTo(point.dx, point.dy);
        }
    }

    class _PathClose : _PathCommand {
        public _PathClose() {
        }

        public override void apply(Path path, float progress) {
            path.close();
        }
    }

    public delegate T _Interpolator<T>(T a, T b, float progress);
}