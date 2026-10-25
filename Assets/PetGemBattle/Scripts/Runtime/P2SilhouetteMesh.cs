using UnityEngine;
using UnityEngine.UI;
namespace PetGemBattle
{
    // Use the saved silhouette with Unity UI without mutating imported Sprite render data.
    [RequireComponent(typeof(Image))]
    public sealed class P2SilhouetteMesh : BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper vh)
        {
            if(!IsActive())return;var image=graphic as Image;var sprite=image.sprite;
            var mesh=P2ArtLibrary.Geometry(sprite);if(mesh==null || mesh.vertices==null || mesh.vertices.Length<3)return;
            var rect=image.GetPixelAdjustedRect();var source=sprite.rect;var pivot=image.rectTransform.pivot;
            if(image.preserveAspect)
            {
                float aspect=source.width/source.height;
                if(rect.width/rect.height>aspect){float w=rect.height*aspect;rect.x+=(rect.width-w)*pivot.x;rect.width=w;}
                else{float h=rect.width/aspect;rect.y+=(rect.height-h)*pivot.y;rect.height=h;}
            }
            vh.Clear();
            foreach(var point in mesh.vertices)
            {
                Vector2 pixel=point*sprite.pixelsPerUnit+sprite.pivot;
                var position=new Vector3(rect.x+pixel.x/source.width*rect.width,rect.y+pixel.y/source.height*rect.height);
                var uv=new Vector2((source.x+pixel.x)/sprite.texture.width,(source.y+pixel.y)/sprite.texture.height);
                vh.AddVert(position,image.color,uv);
            }
            for(int i=0;i<mesh.triangles.Length;i+=3)vh.AddTriangle(mesh.triangles[i],mesh.triangles[i+1],mesh.triangles[i+2]);
        }
    }
}
