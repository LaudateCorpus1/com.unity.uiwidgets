﻿using System;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class Color : IEquatable<Color> {
        public Color(long value) {
            this.value = value & 0xFFFFFFFF;
        }

        public static readonly Color clear = new Color(0x00000000);

        public static readonly Color black = new Color(0xFF000000);

        public static readonly Color white = new Color(0xFFFFFFFF);

        public static Color fromARGB(int a, int r, int g, int b) {
            return new Color(
                (((a & 0xff) << 24) |
                 ((r & 0xff) << 16) |
                 ((g & 0xff) << 8) |
                 ((b & 0xff) << 0)) & 0xFFFFFFFF);
        }

        public static Color fromRGBO(int r, int g, int b, double opacity) {
            return new Color(
                ((((int) (opacity * 0xff) & 0xff) << 24) |
                 ((r & 0xff) << 16) |
                 ((g & 0xff) << 8) |
                 ((b & 0xff) << 0)) & 0xFFFFFFFF);
        }

        public readonly long value;

        public int alpha {
            get { return (int) ((0xff000000 & this.value) >> 24); }
        }

        public double opacity {
            get { return this.alpha / 255.0; }
        }

        public int red {
            get { return (int) ((0x00ff0000 & this.value) >> 16); }
        }

        public int green {
            get { return (int) ((0x0000ff00 & this.value) >> 8); }
        }

        public int blue {
            get { return (int) ((0x000000ff & this.value) >> 0); }
        }

        public Color withAlpha(int a) {
            return fromARGB(a, this.red, this.green, this.blue);
        }

        public Color withOpacity(double opacity) {
            return this.withAlpha((int) (opacity * 255));
        }

        public Color withRed(int r) {
            return fromARGB(this.alpha, r, this.green, this.blue);
        }

        public Color withGreen(int g) {
            return fromARGB(this.alpha, this.red, g, this.blue);
        }

        public Color withBlue(int b) {
            return fromARGB(this.alpha, this.red, this.green, b);
        }

        static double _linearizeColorComponent(double component) {
            if (component <= 0.03928) {
                return component / 12.92;
            }

            return Math.Pow((component + 0.055) / 1.055, 2.4);
        }

        public double computeLuminance() {
            double R = _linearizeColorComponent(this.red / 0xFF);
            double G = _linearizeColorComponent(this.green / 0xFF);
            double B = _linearizeColorComponent(this.blue / 0xFF);
            return 0.2126 * R + 0.7152 * G + 0.0722 * B;
        }

        public static Color lerp(Color a, Color b, double t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b._scaleAlpha(t);
            }

            if (b == null) {
                return a._scaleAlpha(1.0 - t);
            }

            return fromARGB(
                ((int) MathUtils.lerpDouble(a.alpha, b.alpha, t)).clamp(0, 255),
                ((int) MathUtils.lerpDouble(a.red, b.red, t)).clamp(0, 255),
                ((int) MathUtils.lerpDouble(a.green, b.green, t)).clamp(0, 255),
                ((int) MathUtils.lerpDouble(a.blue, b.blue, t)).clamp(0, 255)
            );
        }

        public bool Equals(Color other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.value == other.value;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((Color) obj);
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public static bool operator ==(Color a, Color b) {
            return ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);
        }

        public static bool operator !=(Color a, Color b) {
            return !(a == b);
        }

        public override string ToString() {
            return $"Color(0x{this.value:X8})";
        }
    }

    public enum Clip {
        none,
        hardEdge,
        antiAlias,
        antiAliasWithSaveLayer,
    }

    public enum PaintingStyle {
        fill,
        stroke,
    }

    public enum StrokeCap {
        butt,
        round,
        square,
    }

    public enum StrokeJoin {
        miter,
        round,
        bevel,
    }

    public enum BlurStyle {
        normal, // only normal for now.
        solid,
        outer,
        inner,
    }

    public class MaskFilter : IEquatable<MaskFilter> {
        MaskFilter(BlurStyle style, double sigma) {
            this.style = style;
            this.sigma = sigma;
        }

        public static MaskFilter blur(BlurStyle style, double sigma) {
            return new MaskFilter(style, sigma);
        }

        public readonly BlurStyle style;
        public readonly double sigma;

        public bool Equals(MaskFilter other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.style == other.style && this.sigma.Equals(other.sigma);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((MaskFilter) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) this.style * 397) ^ this.sigma.GetHashCode();
            }
        }

        public static bool operator ==(MaskFilter left, MaskFilter right) {
            return Equals(left, right);
        }

        public static bool operator !=(MaskFilter left, MaskFilter right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"MaskFilter.blur(${this.style}, ${this.sigma:F1})";
        }
    }

    public class ColorFilter : IEquatable<ColorFilter> {
        ColorFilter(Color color, BlendMode blendMode) {
            D.assert(color != null);
            this.color = color;
            this.blendMode = blendMode;
        }

        public static ColorFilter mode(Color color, BlendMode blendMode) {
            return new ColorFilter(color, blendMode);
        }

        public readonly Color color;

        public readonly BlendMode blendMode;

        public bool Equals(ColorFilter other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.color, other.color) && this.blendMode == other.blendMode;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((ColorFilter) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.color != null ? this.color.GetHashCode() : 0) * 397) ^ (int) this.blendMode;
            }
        }

        public static bool operator ==(ColorFilter left, ColorFilter right) {
            return Equals(left, right);
        }

        public static bool operator !=(ColorFilter left, ColorFilter right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"ColorFilter({this.color}, {this.blendMode})";
        }
    }

    public enum TileMode {
        // todo: implement repeated, mirror.
        clamp,
        repeated,
        mirror
    }

    public abstract class PaintShader {
    }

    public class Gradient : PaintShader {
        internal float[] invXform;
        internal float[] extent;
        internal float radius;
        internal float feather;
        internal Color innerColor;
        internal Color outerColor;

        public static Gradient linear(
            Offset from, Offset to,
            Color color0, Color color1, TileMode tileMode = TileMode.clamp) {
            const float large = 1e5f;

            var dir = to - from;
            var dx = (float) dir.dx;
            var dy = (float) dir.dy;
            var d = (float) dir.distance;
            if (d > 0.0001f) {
                dx /= d;
                dy /= d;
            }
            else {
                dx = 0;
                dy = 1;
            }

            var xform = new[] {dy, -dx, dx, dy, (float) from.dx - dx * large, (float) from.dy - dy * large};
            var invXform = new float[6];
            XformUtils.transformInverse(invXform, xform);

            return new Gradient {
                invXform = invXform,
                extent = new[] {large, large + d * 0.5f},
                radius = 0.0f,
                feather = Mathf.Max(1.0f, d),
                innerColor = color0,
                outerColor = color1
            };
        }

        public static Gradient radial(
            Offset center, double radius0, double radius1,
            Color color0, Color color1, TileMode tileMode = TileMode.clamp) {
            float r = (float) (radius0 + radius1) * 0.5f;
            float f = (float) (radius1 - radius0);

            var xform = new[] {1, 0, 0, 1, (float) center.dx, (float) center.dy};
            var invXform = new float[6];
            XformUtils.transformInverse(invXform, xform);

            return new Gradient {
                invXform = invXform,
                extent = new[] {r, r},
                radius = r,
                feather = Mathf.Max(1.0f, f),
                innerColor = color0,
                outerColor = color1
            };
        }

        public static Gradient box(
            Rect rect, double radius, double feather,
            Color color0, Color color1, TileMode tileMode = TileMode.clamp) {
            var ext0 = (float) rect.width * 0.5f;
            var ext1 = (float) rect.height * 0.5f;

            var xform = new[] {1, 0, 0, 1, (float) rect.left + ext0, (float) rect.top + ext1};
            var invXform = new float[6];
            XformUtils.transformInverse(invXform, xform);

            return new Gradient {
                invXform = invXform,
                extent = new[] {ext0, ext1},
                radius = (float) radius,
                feather = Mathf.Max(1.0f, (float) feather),
                innerColor = color0,
                outerColor = color1
            };
        }
    }

    public class Paint {
        static readonly Color _kColorDefault = new Color(0xFFFFFFFF);

        public Color color = _kColorDefault;

        public BlendMode blendMode = BlendMode.srcOver;

        public PaintingStyle style = PaintingStyle.fill;

        public double strokeWidth = 0;

        public StrokeCap strokeCap = StrokeCap.butt;

        public StrokeJoin strokeJoin = StrokeJoin.miter;

        public double strokeMiterLimit = 4.0;

        public FilterMode filterMode = FilterMode.Point;

        public ColorFilter colorFilter = null;

        public MaskFilter maskFilter = null;

        public PaintShader shader = null;

        public bool invertColors;

        public Paint() {
        }

        public Paint(Paint paint) {
            D.assert(paint != null);

            this.color = paint.color;
            this.blendMode = paint.blendMode;
            this.style = paint.style;
            this.strokeWidth = paint.strokeWidth;
            this.strokeCap = paint.strokeCap;
            this.strokeJoin = paint.strokeJoin;
            this.strokeMiterLimit = paint.strokeMiterLimit;
            this.filterMode = paint.filterMode;
            this.colorFilter = paint.colorFilter;
            this.maskFilter = paint.maskFilter;
            this.shader = paint.shader;
            this.invertColors = paint.invertColors;
        }

        public static Paint shapeOnly(Paint paint) {
            return new Paint {
                style = paint.style,
                strokeWidth = paint.strokeWidth,
                strokeCap = paint.strokeCap,
                strokeJoin = paint.strokeJoin,
                strokeMiterLimit = paint.strokeMiterLimit,
            };
        }
    }

    public static class Conversions {
        public static UnityEngine.Color toColor(this Color color) {
            return new UnityEngine.Color(
                color.red / 255f, color.green / 255f, color.blue / 255f, color.alpha / 255f);
        }

        public static Color32 toColor32(this Color color) {
            return new Color32(
                (byte) color.red, (byte) color.green, (byte) color.blue, (byte) color.alpha);
        }

        public static Vector2 toVector(this Offset offset) {
            return new Vector2((float) offset.dx, (float) offset.dy);
        }

        public static UnityEngine.Rect toRect(this Rect rect) {
            return new UnityEngine.Rect((float) rect.left, (float) rect.top, (float) rect.width, (float) rect.height);
        }

        public static float alignToPixel(this float v, float devicePixelRatio) {
            return Mathf.Round(v * devicePixelRatio) / devicePixelRatio;
        }

        internal static Color _scaleAlpha(this Color a, double factor) {
            return a.withAlpha((a.alpha * factor).round().clamp(0, 255));
        }
    }

    public enum BlendMode {
        clear,
        src,
        dst,
        srcOver,
        dstOver,
        srcIn,
        dstIn,
        srcOut,
        dstOut,
        srcATop,
        dstATop,
        xor,
        plus,

        // REF: https://www.w3.org/TR/compositing-1/#blendingseparable
        modulate,
        screen,
        overlay,
        darken,
        lighten,
        colorDodge,
        colorBurn,
        hardLight,
        softLight,
        difference,
        exclusion,
        multiply,

        // REF: https://www.w3.org/TR/compositing-1/#blendingnonseparable
        hue,
        saturation,
        color,
        luminosity,
    }
}