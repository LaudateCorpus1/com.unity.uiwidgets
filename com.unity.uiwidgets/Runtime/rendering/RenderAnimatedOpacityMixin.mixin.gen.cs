using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
namespace Unity.UIWidgets.rendering {

     public abstract class RenderAnimatedOpacityMixinRenderSliver<ChildType> : RenderProxySliver,  RenderAnimatedOpacityMixin<ChildType> where ChildType : RenderObject {
         
         
         public int _alpha { get; set; }

         public new bool alwaysNeedsCompositing {
            get { return child != null && _currentlyNeedsCompositing;}
        }

         public bool _currentlyNeedsCompositing { get; set; }

        

        public Animation<float> opacity {
            get { return _opacity; }
            set {
                D.assert(value != null);
                if (_opacity == value)
                    return;
                if (attached && _opacity != null)
                    _opacity.removeListener(_updateOpacity);
                _opacity = value;
                if (attached)
                    _opacity.addListener(_updateOpacity);
                _updateOpacity();
            }
        }

        public Animation<float> _opacity { get; set; }
        public bool alwaysIncludeSemantics {
            get { return _alwaysIncludeSemantics; }
            set { 
                if (value == _alwaysIncludeSemantics)
                    return;
                _alwaysIncludeSemantics = value;
                //markNeedsSemanticsUpdate();
                
            }
        }

        public bool _alwaysIncludeSemantics { get; set; }

        public override void attach(object owner) {
            owner = (PipelineOwner) owner;
            base.attach(owner);
            _opacity.addListener(_updateOpacity);
            _updateOpacity(); 
        }

        public void attach(PipelineOwner owner) {
            base.attach(owner);
            _opacity.addListener(_updateOpacity);
            _updateOpacity(); 
        }

        public override void detach() {
            _opacity.removeListener(_updateOpacity);
            base.detach();
        }
        public void _updateOpacity() {
            int oldAlpha = _alpha; 
            _alpha = ui.Color.getAlphaFromOpacity((float)_opacity.value); 
            if (oldAlpha != _alpha) { 
                bool didNeedCompositing = _currentlyNeedsCompositing; 
                _currentlyNeedsCompositing = _alpha > 0 && _alpha < 255; 
                if (child != null && didNeedCompositing != _currentlyNeedsCompositing) 
                    markNeedsCompositingBitsUpdate(); 
                markNeedsPaint(); 
                //if (oldAlpha == 0 || _alpha == 0) 
                //    markNeedsSemanticsUpdate();
            }
        }
        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                if (_alpha == 0) {
                    layer = null;
                    return;
                } 
                if (_alpha == 255) {
                    layer = null;
                    context.paintChild(child, offset);
                    return;
                } 
                D.assert(needsCompositing);
                layer = context.pushOpacity(offset, _alpha, base.paint, oldLayer: layer as OpacityLayer);
            }
        }

       
        public  void visitChildrenForSemantics(RenderObjectVisitor visitor) {
            if (child != null && (_alpha != 0 || alwaysIncludeSemantics))
                visitor(child);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Animation<float>>("opacity", opacity));
            properties.add(new FlagProperty("alwaysIncludeSemantics", value: alwaysIncludeSemantics, ifTrue: "alwaysIncludeSemantics"));
        }


        public ChildType child { get; set; }
     }


}


