using UnityEngine;
using UnityEngine.UI;

namespace PetGemBattle
{
    // The toggle stays outside the animated bar, so a closed menu never blocks island clicks.
    public sealed class LobbyNavbarMotion : MonoBehaviour
    {
        private const float Duration = .28f;
        private RectTransform bar;
        private CanvasGroup group;
        private RectTransform arrow;
        private float openness;
        private bool expanded;
        private float fullHeight;

        public void Configure(RectTransform navbar, CanvasGroup content, RectTransform arrowTransform, bool initiallyExpanded)
        {
            bar = navbar;
            group = content;
            arrow = arrowTransform;
            fullHeight = navbar.sizeDelta.y;
            expanded = initiallyExpanded;
            openness = expanded ? 1f : 0f;
            Apply();
        }

        public void SetExpanded(bool value)
        {
            expanded = value;
            if (expanded && bar != null) bar.gameObject.SetActive(true);
            if (group != null)
            {
                group.blocksRaycasts = false;
                group.interactable = false;
            }
        }

        private void Update()
        {
            float target = expanded ? 1f : 0f;
            if (Mathf.Approximately(openness, target)) return;
            openness = Mathf.MoveTowards(openness, target, Time.unscaledDeltaTime / Duration);
            Apply();
        }

        private void Apply()
        {
            if (bar == null || group == null || arrow == null) return;
            float eased = openness * openness * (3f - 2f * openness);
            if (openness > 0f && !bar.gameObject.activeSelf) bar.gameObject.SetActive(true);
            bar.anchoredPosition = new Vector2(0, 391f + (1f - eased) * 130f);
            bar.sizeDelta = new Vector2(bar.sizeDelta.x, fullHeight * eased);
            group.alpha = eased;
            group.blocksRaycasts = expanded && openness >= 1f;
            group.interactable = group.blocksRaycasts;
            arrow.localEulerAngles = new Vector3(0, 0, (1f - eased) * 180f);
            if (openness <= 0f) bar.gameObject.SetActive(false);
        }
    }

    public static class LobbyNavbarSkin
    {
        private static Sprite bar, toggle, arrow;
        public static Sprite Bar => bar != null ? bar : bar = Rounded(96, 21f, 3.5f, true);
        public static Sprite Toggle => toggle != null ? toggle : toggle = Rounded(96, 47f, 6f, false);
        public static Sprite Arrow => arrow != null ? arrow : arrow = Chevron();

        private static Sprite Rounded(int size, float radius, float border, bool sliced)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = sliced ? "SapphireNavbarSkin" : "SapphireToggleSkin";
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            float half = (size - 1) * .5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - half) - (half - radius);
                float dy = Mathf.Abs(y - half) - (half - radius);
                float distance = Mathf.Min(Mathf.Max(dx, dy), 0f) +
                                 Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f)) - radius;
                float coverage = Mathf.Clamp01(.5f - distance);
                float vertical = y / (float)(size - 1);
                Color sapphire = Color.Lerp(new Color(.015f, .065f, .18f, .94f),
                                            new Color(.025f, .18f, .38f, .94f), vertical);
                Color gold = Color.Lerp(new Color(.60f, .34f, .09f, 1f),
                                        new Color(1f, .83f, .37f, 1f), vertical);
                Color color = distance > -border ? gold : sapphire;
                color.a *= coverage;
                pixels[y * size + x] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            var borderPixels = sliced ? new Vector4(25, 25, 25, 25) : Vector4.zero;
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0,
                                       SpriteMeshType.FullRect, borderPixels);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static Sprite Chevron()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "GoldNavbarChevron";
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            var point = new Vector2(20, 31.5f);
            var upper = new Vector2(43, 52);
            var lower = new Vector2(43, 11);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x, y);
                float distance = Mathf.Min(DistanceToSegment(p, point, upper), DistanceToSegment(p, point, lower));
                float alpha = Mathf.Clamp01(4f - distance);
                pixels[y * size + x] = new Color(1f, .91f, .62f, alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f));
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + t * ab);
        }
    }
}
