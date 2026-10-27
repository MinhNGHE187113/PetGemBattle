using UnityEngine;

namespace PetGemBattle
{
    // Replaces the missing P3Art/elements atlas with a single, reusable set of
    // transparent elemental emblems. They are drawn once and cached by P3ArtLibrary.
    internal static class ElementEmblems
    {
        private const int Size = 128;
        private static readonly Color Gold = new Color(.98f, .77f, .34f);
        private static readonly Color Light = new Color(.96f, .98f, 1f);
        private static readonly Color Dark = new Color(.025f, .10f, .19f);

        public static Sprite Create(Element element)
        {
            var pixels = new Color[Size * Size];
            var tint = P1Catalog.ElementColor(element);
            Badge(pixels, tint);
            switch (element)
            {
                case Element.Metal:
                    Gem(pixels, tint);
                    break;
                case Element.Wood:
                    Leaf(pixels, tint);
                    break;
                case Element.Water:
                    Drop(pixels, tint);
                    break;
                case Element.Fire:
                    Flame(pixels, tint);
                    break;
                case Element.Earth:
                    Mountain(pixels, tint);
                    break;
            }

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.name = "ElementEmblem_" + element;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, Size, Size),
                new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
        }

        private static void Badge(Color[] pixels, Color tint)
        {
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float px = (x + .5f) * 2f / Size - 1f;
                float py = (y + .5f) * 2f / Size - 1f;
                float radius = Mathf.Sqrt(px * px + py * py);
                Color color;
                if (radius < .77f) color = Color.Lerp(Dark, tint, .18f + .1f * (py + 1f));
                else if (radius < .82f) color = tint;
                else if (radius < .91f) color = Gold;
                else if (radius < .95f) color = Light;
                else continue;
                color.a = Mathf.Clamp01((.95f - radius) * Size / 2f);
                pixels[y * Size + x] = color;
            }
        }

        private static void Gem(Color[] pixels, Color tint)
        {
            var outline = new[] { V(0,.72f), V(.55f,.23f), V(.41f,-.42f), V(0,-.72f), V(-.41f,-.42f), V(-.55f,.23f) };
            Fill(pixels, outline, Gold);
            Fill(pixels, Scale(outline, .86f), tint);
            Fill(pixels, new[] { V(0,.58f), V(.38f,.20f), V(0,.26f), V(-.38f,.20f) }, Light);
            Line(pixels, V(0,.22f), V(0,-.56f), Light, .025f);
            Line(pixels, V(-.37f,.18f), V(0,-.56f), Light, .018f);
            Line(pixels, V(.37f,.18f), V(0,-.56f), Light, .018f);
        }

        private static void Leaf(Color[] pixels, Color tint)
        {
            var left = new[] { V(-.02f,-.25f), V(-.50f,-.07f), V(-.55f,.24f), V(-.31f,.53f), V(.02f,.35f) };
            var right = new[] { V(.02f,-.25f), V(.50f,-.07f), V(.55f,.24f), V(.31f,.53f), V(-.02f,.35f) };
            Fill(pixels, left, Gold); Fill(pixels, Scale(left, .87f), tint);
            Fill(pixels, right, Gold); Fill(pixels, Scale(right, .87f), tint);
            var top = new[] { V(0,-.12f), V(-.25f,.29f), V(0,.69f), V(.25f,.29f) };
            Fill(pixels, top, Gold); Fill(pixels, Scale(top, .82f), new Color(.50f, 1f, .62f));
            Line(pixels, V(0,-.58f), V(0,.42f), Light, .035f);
            Line(pixels, V(0,-.23f), V(-.39f,.26f), Light, .023f);
            Line(pixels, V(0,-.23f), V(.39f,.26f), Light, .023f);
        }

        private static void Drop(Color[] pixels, Color tint)
        {
            var outer = new[] { V(0,.75f), V(-.17f,.43f), V(-.43f,.08f), V(-.51f,-.26f),
                V(-.38f,-.57f), V(-.16f,-.72f), V(.16f,-.72f), V(.38f,-.57f),
                V(.51f,-.26f), V(.43f,.08f), V(.17f,.43f) };
            Fill(pixels, outer, Gold);
            Fill(pixels, Scale(outer, .88f), tint);
            Fill(pixels, new[] { V(-.05f,.45f), V(-.30f,.02f), V(-.36f,-.26f),
                V(-.23f,-.48f), V(-.05f,-.39f), V(-.18f,-.10f) }, Light);
            Line(pixels, V(-.02f,-.53f), V(.30f,-.44f), Light, .025f);
        }

        private static void Flame(Color[] pixels, Color tint)
        {
            var outer = new[] { V(-.06f,.75f), V(.24f,.37f), V(.45f,.51f), V(.39f,.12f),
                V(.54f,-.20f), V(.40f,-.56f), V(.14f,-.74f), V(-.18f,-.73f),
                V(-.46f,-.51f), V(-.53f,-.18f), V(-.33f,.13f), V(-.29f,.49f) };
            Fill(pixels, outer, Gold);
            Fill(pixels, Scale(outer, .87f), tint);
            Fill(pixels, new[] { V(.03f,.43f), V(.20f,.06f), V(.09f,-.14f), V(.25f,-.31f),
                V(.15f,-.59f), V(-.14f,-.60f), V(-.28f,-.30f), V(-.08f,-.10f) },
                new Color(1f, .92f, .47f));
        }

        private static void Mountain(Color[] pixels, Color tint)
        {
            var left = new[] { V(-.63f,-.55f), V(-.22f,.38f), V(.09f,-.28f), V(.25f,-.55f) };
            var right = new[] { V(-.17f,-.55f), V(.20f,.69f), V(.66f,-.55f) };
            Fill(pixels, left, Gold); Fill(pixels, Scale(left, .88f), tint);
            Fill(pixels, right, Gold); Fill(pixels, Scale(right, .88f), tint);
            Fill(pixels, new[] { V(.20f,.61f), V(.04f,.12f), V(.20f,.23f), V(.34f,.12f) }, Light);
            Fill(pixels, new[] { V(-.22f,.29f), V(-.38f,-.07f), V(-.21f,.03f), V(-.10f,-.04f) }, Light);
            Line(pixels, V(-.61f,-.59f), V(.65f,-.59f), Light, .025f);
        }

        private static Vector2 V(float x, float y) { return new Vector2(x, y); }

        private static Vector2[] Scale(Vector2[] points, float factor)
        {
            var result = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++) result[i] = points[i] * factor;
            return result;
        }

        private static void Fill(Color[] pixels, Vector2[] points, Color color)
        {
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var p = V((x + .5f) * 2f / Size - 1f, (y + .5f) * 2f / Size - 1f);
                bool inside = false;
                for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                    if ((points[i].y > p.y) != (points[j].y > p.y) &&
                        p.x < (points[j].x - points[i].x) * (p.y - points[i].y) / (points[j].y - points[i].y) + points[i].x)
                        inside = !inside;
                if (inside) pixels[y * Size + x] = color;
            }
        }

        private static void Line(Color[] pixels, Vector2 start, Vector2 end, Color color, float width)
        {
            var direction = end - start;
            float lengthSquared = direction.sqrMagnitude;
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var p = V((x + .5f) * 2f / Size - 1f, (y + .5f) * 2f / Size - 1f);
                float t = Mathf.Clamp01(Vector2.Dot(p - start, direction) / lengthSquared);
                if ((p - (start + direction * t)).sqrMagnitude < width * width)
                    pixels[y * Size + x] = color;
            }
        }
    }
}
