using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class Texture : LeafRenderObjectWidget {
        public Texture( 
            Key key = null, 
            int? textureId = null) : base(key: key) {
            D.assert(textureId != null);
            this.textureId = textureId.Value;
        }

        public readonly int textureId;

        public override RenderObject createRenderObject(BuildContext context) {
            return new TextureBox(textureId: textureId);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((TextureBox) renderObject).textureId = textureId;
        }
    }
}