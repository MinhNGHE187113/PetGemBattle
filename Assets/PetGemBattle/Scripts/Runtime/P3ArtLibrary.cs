using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PetGemBattle
{
    public static class P3ArtLibrary
    {
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private static readonly HashSet<string> singleSprites = new HashSet<string>{"wood-1","starter-metal","metal-bird","fire-lion","water-otter","earth-turtle","legend-fire-bird","legend-gold-bird"};
        private static readonly Dictionary<string, int> diversePets = new Dictionary<string, int>
        {
            {"sun-rooster",0},{"moon-rabbit",1},{"coral-crab",2},{"jade-serpent",3},
            {"fire-monkey",4},{"star-pig",5},{"storm-tiger",6},{"abyss-angler",7}
        };
        private static readonly Dictionary<string, int> legendaryPets = new Dictionary<string, int>
        {
            {"legend-metal-rhino",0},{"legend-fire-chimera",1},{"legend-water-manta",2}
        };
        private static readonly Dictionary<string, string> artAliases = new Dictionary<string, string>
        {
            {"metal-eagle-s","metal-ss"},{"fire-lion-s","fire-ss"},{"water-bird-s","water-ss"}
        };
        // The three drawings intentionally grow in width as they evolve. Their
        // separators therefore differ from the equal thirds of the source PNG.
        private static readonly Dictionary<string, Vector2> stageSeparators = new Dictionary<string, Vector2>
        {
            {"wood-2",new Vector2(438f/1900,935f/1900)},
            {"wood-3",new Vector2(452f/1881,1017f/1881)},
            {"wood-4",new Vector2(385f/1881,981f/1881)},
            {"wood-5",new Vector2(462f/2172,1221f/2172)},
            {"wood-6",new Vector2(437f/1774,937f/1774)},
            {"wood-7",new Vector2(382f/1932,1032f/1932)},
            {"wood-s",new Vector2(387f/2172,1209f/2172)},
            {"water-s",new Vector2(436f/2172,1139f/2172)},
            {"earth-s",new Vector2(505f/2172,1195f/2172)},
            {"metal-ss",new Vector2(482f/2048,1203f/2048)},
            {"fire-ss",new Vector2(393f/1942,1076f/1942)},
            {"water-ss",new Vector2(387f/1942,1023f/1942)}
        };
        private static readonly Dictionary<string, Rect[]> stageBounds = new Dictionary<string, Rect[]>();
        public static Sprite NavIcon(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string key = "nav-icon-" + id;
            if (sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            var icon = Resources.Load<Sprite>("P3Art/NavIcons/" + id);
            if (icon == null)
            {
                var texture = Resources.Load<Texture2D>("P3Art/NavIcons/" + id);
                if (texture != null) icon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            }
            if (icon != null) sprites[key] = icon;
            return icon;
        }
        public static Sprite LobbyIcon(int index)
        {
            if (index < 0 || index > 7) return null;
            string key = "lobby-icon-" + index;
            if (sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            var texture = Resources.Load<Texture2D>("P3Art/lobby-icons");
            if (texture == null) return null;
            float cellWidth = texture.width / 4f, cellHeight = texture.height / 2f;
            var sprite = Sprite.Create(texture, new Rect(index % 4 * cellWidth, (1 - index / 4) * cellHeight, cellWidth, cellHeight), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            sprite.name = key;
            sprites[key] = sprite;
            return sprite;
        }
        public static Sprite ShortcutIcon(int index)
        {
            if (index < 0 || index > 3) return null;
            string key = "shortcut-icon-" + index;
            if (sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            var texture = Resources.Load<Texture2D>("P3Art/shortcut-icons");
            if (texture == null) return null;
            float width = texture.width / 4f;
            var sprite = Sprite.Create(texture, new Rect(index * width, 0, width, texture.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            sprite.name = key;sprites[key] = sprite;return sprite;
        }
        public static Sprite Element(Element element)
        {
            string key = "element-" + (int)element;
            if (sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            var texture = Resources.Load<Texture2D>("P3Art/elements");
            Sprite sprite;
            if (texture != null)
            {
                float width = texture.width / 5f;
                sprite = Sprite.Create(texture, new Rect((int)element * width, texture.height * .12f, width, texture.height * .78f), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            }
            else
            {
                // The old P3 atlas is absent from this project. Keep one shared
                // elemental emblem source for collection, island and battle UI.
                sprite = ElementEmblems.Create(element);
            }
            sprite.name = key;
            sprites[key] = sprite;
            return sprite;
        }
        public static Sprite Get(string key)
        {
            if (string.IsNullOrEmpty(key) || !key.StartsWith("p3-")) return null;
            if (sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            if (key == "p3-lobby-map")
            {
                var map = Resources.Load<Texture2D>("P3Art/lobby-map");
                if (map == null) return null;
                var full = Sprite.Create(map, new Rect(0, 0, map.width, map.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
                sprites[key] = full;
                return full;
            }
            string species = key.Substring(3);
            // Remaster sprites are separate transparent single-form PNGs. The
            // legacy artSheet convention uses base, -form1, -form2; map its first
            // evolution to the second authored form and never load a combined sheet.
            string remasterSpecies = species;
            int remasterForm = 1;
            if (remasterSpecies.EndsWith("-form1"))
            {
                remasterSpecies = remasterSpecies.Substring(0, remasterSpecies.Length - 6);
                remasterForm = 2;
            }
            else if (remasterSpecies.EndsWith("-form2")) return null;
            if (remasterSpecies == "legend-metal-rhino") remasterSpecies = "metal-ss";
            else if (remasterSpecies == "legend-fire-chimera") remasterSpecies = "fire-ss";
            else if (remasterSpecies == "legend-water-manta") remasterSpecies = "water-ss";
            else if (remasterSpecies == "legend-fire-bird") remasterSpecies = "water-bird-s";
            else if (remasterSpecies == "legend-gold-bird") remasterSpecies = "golden-wing-ss";
            var remaster = Resources.Load<Sprite>($"PetRemaster/{remasterSpecies}-form-{remasterForm}");
            if (remaster != null)
            {
                remaster.name = key;
                sprites[key] = remaster;
                return remaster;
            }
            if (legendaryPets.TryGetValue(species, out int legendaryIndex))
            {
                var atlas = Resources.Load<Texture2D>("P3Art/legendary-diversity");
                if (atlas == null) return null;
                float width = atlas.width / 3f;
                var portrait = Sprite.Create(atlas, new Rect(legendaryIndex * width, 0, width, atlas.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
                portrait.name = key;sprites[key] = portrait;return portrait;
            }
            if (diversePets.TryGetValue(species, out int atlasIndex))
            {
                var atlas = Resources.Load<Texture2D>("P3Art/zodiac-diversity");
                if (atlas == null) return null;
                float width = atlas.width / 4f, height = atlas.height / 2f;
                var portrait = Sprite.Create(atlas, new Rect(atlasIndex % 4 * width, (1 - atlasIndex / 4) * height, width, height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
                portrait.name = key;
                sprites[key] = portrait;
                return portrait;
            }
            int stage = 0;
            if (species.EndsWith("-form1")) { species = species.Substring(0, species.Length - 6); stage = 1; }
            else if (species.EndsWith("-form2")) { species = species.Substring(0, species.Length - 6); stage = 2; }
            string artSpecies=artAliases.TryGetValue(species,out var source) ? source : species;
            var texture = Resources.Load<Texture2D>("P3Art/" + artSpecies);
            if (texture == null) return null;
            if (singleSprites.Contains(artSpecies))
            {
                var single = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
                single.name = key;
                sprites[key] = single;
                return single;
            }
            if (!stageBounds.TryGetValue(artSpecies, out var bounds))
            {
                bounds = FindStageBounds(artSpecies, texture);
                stageBounds[artSpecies] = bounds;
            }
            var sprite = Sprite.Create(texture, bounds[stage], new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            sprite.name = key;
            sprites[key] = sprite;
            return sprite;
        }
        public static string NextFormSheet(string sheet)
        {
            if (string.IsNullOrEmpty(sheet) || !sheet.StartsWith("p3-")) return null;
            string next = sheet.EndsWith("-form2") ? null : sheet.EndsWith("-form1") ? sheet.Substring(0, sheet.Length - 6) + "-form2" : sheet + "-form1";
            return next != null && Get(next) != null ? next : null;
        }
        private static Rect[] FindStageBounds(string species, Texture2D texture)
        {
            var split = stageSeparators.TryGetValue(species, out var known) ? known : new Vector2(1f / 3, 2f / 3);
            int[] edges = {0, Mathf.RoundToInt(texture.width * split.x), Mathf.RoundToInt(texture.width * split.y), texture.width};
            var result = new Rect[3];
            Color32[] pixels;
            try { pixels = texture.GetPixels32(); }
            catch (UnityException)
            {
                for (int stage = 0; stage < 3; stage++) result[stage] = new Rect(edges[stage], 0, edges[stage + 1] - edges[stage], texture.height);
                return result;
            }
            for (int stage = 0; stage < 3; stage++)
            {
                int minX = edges[stage + 1], maxX = edges[stage], minY = texture.height, maxY = 0;
                for (int y = 0; y < texture.height; y++)
                {
                    int row = y * texture.width;
                    for (int x = edges[stage]; x < edges[stage + 1]; x++)
                        if (pixels[row + x].a > 32)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                }
                if (minX > maxX) result[stage] = new Rect(edges[stage], 0, edges[stage + 1] - edges[stage], texture.height);
                else
                {
                    const int padding = 6;
                    int left = Mathf.Max(edges[stage], minX - padding), bottom = Mathf.Max(0, minY - padding);
                    int right = Mathf.Min(edges[stage + 1], maxX + padding + 1), top = Mathf.Min(texture.height, maxY + padding + 1);
                    result[stage] = new Rect(left, bottom, right - left, top - bottom);
                }
            }
            return result;
        }
    }

    public sealed class P3PetRig : MonoBehaviour
    {
        private RectTransform pivot, figure, aura;
        private string baseSheet, nextSheet;
        private float baseSize, evolvedSize;
        private bool formChanged;
        private P3SoftMotion softMotion;
        private PetPose pose;
        private float elapsed, duration = 1;
        private bool left;
        private PetAnimationEvents events;
        private Color accent;
        public void Build(string sheet, bool facingLeft, Color tint, PetAnimationEvents frameEvents)
        {
            left = facingLeft; events = frameEvents; accent = tint; baseSheet = sheet;
            pivot = new GameObject("P3OriginalPet", typeof(RectTransform)).GetComponent<RectTransform>();
            pivot.SetParent(transform, false);
            var shadow = P1Art.Part(transform, "PetShadow", P1Art.Circle, new Vector2(0, -142), new Vector2(270, 24), new Color(.01f, .05f, .08f, .38f));
            shadow.transform.SetAsFirstSibling();
            aura = P1Art.Part(pivot, "LegendaryAura", P1Art.Circle, Vector2.zero, new Vector2(305, 305), new Color(tint.r, tint.g, tint.b, .13f)).rectTransform;
            aura.gameObject.SetActive(IsLegendarySprite(sheet));
            int size = IsLegendarySprite(sheet) ? 365 : IsGrandEvolution(sheet) || sheet.EndsWith("-form2") ? 375 : sheet.EndsWith("-form1") ? 315 : 250;
            baseSize = evolvedSize = size;
            var art = P1Art.Part(pivot, "OriginalSprite", P3ArtLibrary.Get(sheet), new Vector2(0, (size - 250) * .48f), new Vector2(size, size), Color.white);
            art.preserveAspect = true;
            figure = art.rectTransform;softMotion = art.gameObject.AddComponent<P3SoftMotion>();softMotion.Configure(facingLeft, sheet);
        }
        public void SetPose(PetPose next, float seconds)
        {
            pose = next; elapsed = 0; duration = Mathf.Max(.01f, seconds);
            if (next == PetPose.Evolution)
            {
                nextSheet = P3ArtLibrary.NextFormSheet(baseSheet);formChanged = false;
                evolvedSize = nextSheet == null ? baseSize : IsGrandEvolution(nextSheet) || nextSheet.EndsWith("-form2") ? 375 : 315;
            }
            softMotion?.SetPose(next, duration);
        }
        private static bool IsLegendarySprite(string sheet) => sheet.Contains("-ss") || sheet.Contains("p3-legend-") || sheet == "p3-metal-eagle-s" || sheet == "p3-water-bird-s";
        private static bool IsGrandEvolution(string sheet) => sheet.EndsWith("-form1") && (sheet.Contains("wood-7") || sheet.Contains("fire-lion-s"));
        public void BossAwaken() { SetPose(PetPose.Evolution, 1.2f); }
        public void ResetForm()
        {
            if (figure != null) figure.GetComponent<Image>().sprite = P3ArtLibrary.Get(baseSheet);
            nextSheet = null;formChanged = false;evolvedSize = baseSize;SetPose(PetPose.Idle, 1);
        }
        private void Update()
        {
            if (pivot == null) return;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(events != null && pose != PetPose.Idle ? events.phase : elapsed / duration);
            if (pose == PetPose.Evolution && !formChanged && t >= .5f && nextSheet != null)
            {
                figure.GetComponent<Image>().sprite = P3ArtLibrary.Get(nextSheet);
                formChanged = true;
            }
            float size = formChanged ? pose == PetPose.Evolution ? Mathf.Lerp(baseSize,evolvedSize,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,1,t))) : evolvedSize : baseSize;
            figure.sizeDelta = new Vector2(size,size);
            figure.anchoredPosition = new Vector2(0,(size-250)*.48f);
            float wave = Mathf.Sin(Time.time * 2.3f), strike = Mathf.Sin(t * Mathf.PI);
            bool attacking = pose == PetPose.Attack || pose == PetPose.Special;
            float lunge = attacking ? strike * (pose == PetPose.Special ? 115 : 70) : 0;
            float recoil=pose==PetPose.Hit ? Mathf.Sin(t*7*Mathf.PI)*(1-t)*19 : 0;
            pivot.anchoredPosition = new Vector2((left ? 1 : -1) * (lunge-recoil), wave * 7 + (pose == PetPose.Victory ? Mathf.Abs(wave) * 22 : 0) - (pose == PetPose.Defeat ? t * 65 : 0));
            pivot.localScale = new Vector3((left ? 1 : -1) * (1 + wave * .018f + (pose == PetPose.Special ? strike * .16f : 0)), 1 - wave * .018f + (pose == PetPose.Evolution ? strike * .12f : 0), 1);
            pivot.localRotation = Quaternion.Euler(0, 0, attacking ? -strike * 11 : pose == PetPose.Defeat ? -t * 55 : wave * 1.5f);
            figure.GetComponent<Image>().color = pose == PetPose.Hit && elapsed < .25f ? new Color(1, .52f, .45f) : pose == PetPose.Defeat ? Color.Lerp(Color.white, new Color(.45f, .5f, .56f, .65f), t) : Color.white;
            if (aura != null) { aura.localScale = Vector3.one * (1 + Mathf.Sin(Time.time * 3) * .08f + (pose == PetPose.Special ? strike * .5f : 0)); aura.GetComponent<Image>().color = new Color(accent.r, accent.g, accent.b, pose == PetPose.Special ? .34f : .13f); }
            if (t >= 1 && pose != PetPose.Idle && pose != PetPose.Victory && pose != PetPose.Defeat) SetPose(PetPose.Idle, 1);
        }
    }

    // The source art is a flat illustration. A small grid lets the head, sides and
    // feet move at different times without cutting the character into fake limbs.
    public sealed class P3SoftMotion : BaseMeshEffect
    {
        private enum Profile { Grounded, Bird, Serpent, Aquatic, Crab, Agile }
        private Profile profile;
        private bool facesLeft;
        private float phase, poseStart, poseSeconds = 1;
        private PetPose pose;
        private Image image;
        public void Configure(bool left, string species)
        {
            facesLeft = left;phase = Mathf.Abs((species ?? "").GetHashCode() % 1000) * .006f;
            string id=species??"";
            profile=id.Contains("serpent") || id.Contains("wood-4") || id.Contains("wood-s") ? Profile.Serpent :
                id.Contains("manta") || id.Contains("angler") || id.Contains("water-s") || id.Contains("otter") ? Profile.Aquatic :
                id.Contains("crab") ? Profile.Crab :
                id.Contains("bird") || id.Contains("eagle") || id.Contains("golden-wing") || id.Contains("rooster") || id.Contains("metal-ss") || id.Contains("water-ss") ? Profile.Bird :
                id.Contains("monkey") || id.Contains("wood-3") || id.Contains("wood-5") ? Profile.Agile : Profile.Grounded;
            image = GetComponent<Image>();
        }
        public void SetPose(PetPose value, float seconds) { pose = value; poseStart = Time.time; poseSeconds = Mathf.Max(.01f, seconds); }
        private void Update() { if (image != null && image.isActiveAndEnabled) image.SetVerticesDirty(); }
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount < 4) return;
            var vertex = new UIVertex();
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            float minU = float.MaxValue, maxU = float.MinValue, minV = float.MaxValue, maxV = float.MinValue;
            Color32 tint = Color.white;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);tint = vertex.color;
                minX = Mathf.Min(minX, vertex.position.x); maxX = Mathf.Max(maxX, vertex.position.x);
                minY = Mathf.Min(minY, vertex.position.y); maxY = Mathf.Max(maxY, vertex.position.y);
                minU = Mathf.Min(minU, vertex.uv0.x); maxU = Mathf.Max(maxU, vertex.uv0.x);
                minV = Mathf.Min(minV, vertex.uv0.y); maxV = Mathf.Max(maxV, vertex.uv0.y);
            }
            const int steps = 8;
            float breath = Mathf.Sin(Time.time * (2.1f + phase * .07f) + phase);
            float gait = Mathf.Sin(Time.time * (3.3f + phase * .03f) + phase);
            float action = pose == PetPose.Attack || pose == PetPose.Special ? Mathf.Sin(Mathf.Clamp01((Time.time - poseStart) / poseSeconds) * Mathf.PI) : 0;
            float facing = facesLeft ? 1 : -1;
            vh.Clear();
            for (int y = 0; y <= steps; y++) for (int x = 0; x <= steps; x++)
            {
                float u = x / (float)steps, v = y / (float)steps;
                float head = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.43f, .9f, v));
                float foot = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.05f, .43f, v));
                float side = (u - .5f) * 2;
                float dx = head * (breath * 4.5f + gait * 2f) + side * (breath * 4f + action * 11f) + facing * action * (8f + head * 8f);
                float dy = head * breath * 4f + foot * gait * side * 5f - action * foot * 6f;
                if(profile==Profile.Serpent)
                {
                    dx+=Mathf.Sin(Time.time*2.4f+v*7+phase)*7*(.3f+v*.7f);
                    dy+=Mathf.Cos(Time.time*2.1f+v*5+phase)*3;
                }
                else if(profile==Profile.Aquatic)
                {
                    dy+=Mathf.Abs(side)*Mathf.Sin(Time.time*2.8f+phase+v*3)*7;
                    dx+=Mathf.Sin(Time.time*1.9f+v*5+phase)*4;
                }
                else if(profile==Profile.Bird)
                {
                    dy+=Mathf.Abs(side)*head*Mathf.Sin(Time.time*4.2f+phase)*8;
                }
                else if(profile==Profile.Crab)
                {
                    dx+=side*Mathf.Sin(Time.time*3.6f+phase)*7*(1-foot*.4f);
                }
                else if(profile==Profile.Agile)
                {
                    dx+=side*Mathf.Sin(Time.time*3.7f+phase)*5*(1-foot);
                    dy+=Mathf.Sin(Time.time*3.1f+phase)*2.5f;
                }
                var item = UIVertex.simpleVert;item.color = tint;
                item.position = new Vector3(Mathf.Lerp(minX, maxX, u) + dx, Mathf.Lerp(minY, maxY, v) + dy);
                item.uv0 = new Vector2(Mathf.Lerp(minU, maxU, u), Mathf.Lerp(minV, maxV, v));
                vh.AddVert(item);
            }
            for (int y = 0; y < steps; y++) for (int x = 0; x < steps; x++)
            {
                int a = y * (steps + 1) + x, b = a + 1, c = a + steps + 1, d = c + 1;
                vh.AddTriangle(a, c, b);vh.AddTriangle(b, c, d);
            }
        }
    }
}
