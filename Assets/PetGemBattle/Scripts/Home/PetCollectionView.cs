using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PetGemBattle
{
    public sealed partial class HomeController
    {
        private int collectionTab;
        private string collectionSearch = "";
        private string collectionSort = "Cấp ↓";
        private RectTransform collectionCards, collectionDetail, collectionPagination;
        private Text collectionCount, collectionResultCount;
        private bool collectionCompact;
        private int collectionScreenWidth;

        private int CollectionViewportWidth()
        {
            var canvas = root == null ? null : root.GetComponent<Canvas>();
            return canvas == null ? Screen.width : Mathf.RoundToInt(canvas.pixelRect.width);
        }

        private void Update()
        {
            if (CurrentScreen == "Pets" && root != null && collectionScreenWidth != CollectionViewportWidth())
                Render();
        }

        private Image CollectionFrame(Transform parent, string name, Vector2 at, Vector2 size, float opacity = 1f)
        {
            var frame = Panel(parent, name, at, size, new Color(1, 1, 1, opacity));
            frame.sprite = LobbyNavbarSkin.Bar;
            frame.type = Image.Type.Sliced;
            return frame;
        }

        private Text CollectionText(Transform parent, string value, Vector2 at, Vector2 size, int fontSize, Color color)
        {
            var label = Text(parent, value, at, size, fontSize, color);
            label.fontStyle = FontStyle.Bold;
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(.005f, .025f, .075f, .95f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return label;
        }

        private Button CollectionButton(Transform parent, string label, Vector2 at, Vector2 size, Action action,
                                        bool enabled = true, Color? accent = null)
        {
            var graphic = CollectionFrame(parent, "Button_" + label, at, size, enabled ? .96f : .55f);
            graphic.raycastTarget = true;
            var color = accent ?? new Color(.42f, .85f, 1f);
            var outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(color.r, color.g, color.b, enabled ? .78f : .25f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            var button = graphic.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            button.interactable = enabled;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => { if (!button.interactable) return; P2Click(); Safe(action); });
            var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
            CollectionText(graphic.transform, label, Vector2.zero, size - new Vector2(12, 6),
                           size.y > 46 ? 18 : 15, enabled ? Color.white : new Color(.6f, .71f, .77f));
            return button;
        }

        private void DrawPetCollection()
        {
            collectionScreenWidth = CollectionViewportWidth();
            collectionCompact = collectionScreenWidth < 1400;
            if (collectionSort != "Cấp ↑" && collectionSort != "Cấp ↓" && collectionSort != "Bậc" && collectionSort != "Tên A–Z")
                collectionSort = "Cấp ↓";

            var header = CollectionFrame(root, "PetCollectionHeader", new Vector2(0, 392), new Vector2(1530, 110), .92f);
            CollectionText(header.transform, "KHO PET", new Vector2(0, 19), new Vector2(580, 54), 41, new Color(1, .88f, .57f));
            CollectionText(header.transform, "THIÊN CẢNH NGŨ HÀNH", new Vector2(0, -20), new Vector2(500, 30), 18, new Color(.78f, .93f, 1));
            collectionCount = CollectionText(header.transform, "", new Vector2(0, -45), new Vector2(520, 25), 16, Color.white);
            CollectionButton(header.transform, "‹ TRỞ LẠI", new Vector2(-638, 1), new Vector2(210, 53), Back);
            CollectionButton(header.transform, "⚙", new Vector2(686, 1), new Vector2(62, 62), () => Navigate("Settings"));

            CollectionFrame(root, "PetCollectionSurface", new Vector2(0, -41), new Vector2(1530, 730), .82f);
            CollectionFrame(root, "PetFilterRail", new Vector2(-267, 260), new Vector2(1000, 113), .83f);
            DrawCollectionSearch();
            string[] tabs = { "TẤT CẢ", "ĐÃ SỞ HỮU", "XUẤT CHIẾN", "CHƯA MỞ" };
            for (int i = 0; i < tabs.Length; i++)
            {
                int chosen = i;
                CollectionButton(root, tabs[i], new Vector2(-445 + i * 163, 288), new Vector2(151, 43),
                    () => { collectionTab = chosen; petPage = 0; Render(); }, true,
                    i == collectionTab ? new Color(.28f, .94f, 1) : new Color(.48f, .65f, .77f));
            }

            string[] elements = { "Tất cả", "Kim", "Mộc", "Thủy", "Hỏa", "Thổ" };
            string[] rarities = { "Tất cả", "C", "B", "A", "S", "SS", "SSS" };
            string[] levels = { "Tất cả", "1", "2", "3", "4", "5", "6", "7" };
            string[] forms = { "Tất cả", "Gốc", "Đã tiến hóa" };
            string[] sorts = { "Cấp ↑", "Cấp ↓", "Bậc", "Tên A–Z" };
            CollectionDropdown("HỆ", elements, elementFilter + 1, new Vector2(-657, 233), i => elementFilter = i - 1);
            CollectionDropdown("BẬC", rarities, rarityFilter + 1, new Vector2(-497, 233), i => rarityFilter = i - 1);
            CollectionDropdown("LV", levels, levelFilter, new Vector2(-337, 233), i => levelFilter = i);
            CollectionDropdown("FORM", forms, formFilter, new Vector2(-177, 233), i => formFilter = i);
            CollectionDropdown("SẮP XẾP", sorts, Array.IndexOf(sorts, collectionSort), new Vector2(-17, 233), i => collectionSort = sorts[i]);
            CollectionButton(root, "XÓA LỌC", new Vector2(145, 233), new Vector2(145, 42), () =>
            {
                collectionSearch = ""; collectionTab = 0; elementFilter = rarityFilter = -1;
                levelFilter = formFilter = petPage = 0; collectionSort = "Cấp ↓"; Render();
            });

            collectionCards = Rect(root, "PetCards", Vector2.zero, new Vector2(1500, 900));
            collectionDetail = Rect(root, "PetDetail", Vector2.zero, new Vector2(1500, 900));
            collectionPagination = Rect(root, "PetPagination", Vector2.zero, new Vector2(1500, 900));
            collectionResultCount = CollectionText(root, "", new Vector2(-565, -376), new Vector2(370, 28), 17, Color.white);
            RefreshPetCollection();
        }

        private void DrawCollectionSearch()
        {
            var background = CollectionFrame(root, "PetSearch", new Vector2(-640, 288), new Vector2(242, 43), .97f);
            background.raycastTarget = true;
            var value = Text(background.transform, "", new Vector2(9, 0), new Vector2(210, 36), 16, Color.white);
            value.alignment = TextAnchor.MiddleLeft;
            var placeholder = Text(background.transform, "⌕  Tìm tên pet...", new Vector2(9, 0), new Vector2(210, 36), 16, new Color(.67f, .79f, .89f));
            placeholder.alignment = TextAnchor.MiddleLeft;
            var input = background.gameObject.AddComponent<InputField>();
            input.targetGraphic = background;
            input.textComponent = value;
            input.placeholder = placeholder;
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = 50;
            input.SetTextWithoutNotify(collectionSearch);
            input.onValueChanged.AddListener(text =>
            {
                collectionSearch = text;
                petPage = 0;
                RefreshPetCollection();
            });
        }

        private void CollectionDropdown(string label, string[] options, int selected, Vector2 at, Action<int> choose)
        {
            selected = Mathf.Clamp(selected, 0, options.Length - 1);
            CollectionButton(root, label + ": " + options[selected] + "  ▾", at, new Vector2(150, 42),
                () => ShowCollectionDropdown(label, options, at, choose));
        }

        private void ShowCollectionDropdown(string label, string[] options, Vector2 at, Action<int> choose)
        {
            var blocker = Panel(root, "CollectionDropdownBlocker", Vector2.zero, new Vector2(1600, 900), new Color(.01f, .04f, .09f, .20f));
            blocker.raycastTarget = true;
            var close = blocker.gameObject.AddComponent<Button>(); close.targetGraphic = blocker;
            close.onClick.AddListener(() => Destroy(blocker.gameObject));
            var popup = CollectionFrame(root, "Dropdown_" + label,
                new Vector2(at.x, at.y - 31 - options.Length * 19),
                new Vector2(180, options.Length * 38 + 12), 1f);
            popup.transform.SetAsLastSibling();
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                CollectionButton(popup.transform, options[i],
                    new Vector2(0, (options.Length - 1) * 19 - i * 38), new Vector2(164, 35),
                    () => { choose(index); petPage = 0; Render(); });
            }
        }

        private static string SearchKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            string decomposed = value.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (char c in decomposed)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) builder.Append(c);
            return builder.ToString();
        }

        private List<PetSpecies> CollectionResults()
        {
            string query = SearchKey(collectionSearch);
            IEnumerable<PetSpecies> results = catalog.pets.Where(p =>
            {
                var owned = Progress.Owned(p.id);
                if (collectionTab == 1 && owned == null) return false;
                if (collectionTab == 2 && p.id != Progress.Data.activePetId) return false;
                if (collectionTab == 3 && owned != null) return false;
                if (elementFilter >= 0 && (int)p.element != elementFilter) return false;
                if (rarityFilter >= 0 && (int)p.rarity != rarityFilter) return false;
                int level = owned?.level ?? 1;
                if (levelFilter > 0 && level != levelFilter) return false;
                if (formFilter > 0 && (catalog.Forms(p.id, level).Length > 0) != (formFilter == 2)) return false;
                return query.Length == 0 || SearchKey(p.title).Contains(query) || SearchKey(catalog.Stats(p.id, level).name).Contains(query);
            });
            switch (collectionSort)
            {
                case "Cấp ↑": results = results.OrderBy(p => Progress.Owned(p.id)?.level ?? 1).ThenBy(p => p.title); break;
                case "Bậc": results = results.OrderByDescending(p => (int)p.rarity).ThenBy(p => p.title); break;
                case "Tên A–Z": results = results.OrderBy(p => SearchKey(p.title)); break;
                default: results = results.OrderByDescending(p => Progress.Owned(p.id)?.level ?? 1).ThenBy(p => p.title); break;
            }
            return results.ToList();
        }

        private static void ClearCollection(Transform parent)
        {
            if (parent == null) return;
            foreach (Transform child in parent) { child.gameObject.SetActive(false); UnityEngine.Object.Destroy(child.gameObject); }
        }

        private void RefreshPetCollection()
        {
            if (collectionCards == null || collectionDetail == null || collectionPagination == null) return;
            ClearCollection(collectionCards);
            ClearCollection(collectionDetail);
            ClearCollection(collectionPagination);
            var results = CollectionResults();
            int ownedCount = Progress.Data.pets.Count(p => catalog.Pet(p.id) != null);
            collectionCount.text = "Đã sở hữu " + ownedCount + "/" + catalog.pets.Length + "  •  Bộ sưu tập " + catalog.pets.Length;
            collectionResultCount.text = results.Count + " pet phù hợp";
            int pageSize = collectionCompact ? 4 : 6;
            int pageCount = Math.Max(1, (results.Count + pageSize - 1) / pageSize);
            petPage = Mathf.Clamp(petPage, 0, pageCount - 1);
            var page = results.Skip(petPage * pageSize).Take(pageSize).ToList();
            if (page.Count == 0)
            {
                CollectionText(collectionCards, "KHÔNG CÓ PET PHÙ HỢP\nThử tên khác hoặc xóa bộ lọc.",
                    new Vector2(-268, -35), new Vector2(900, 170), 26, new Color(.82f, .92f, 1));
                DrawCollectionEmptyDetail();
            }
            else
            {
                if (!page.Any(p => p.id == selectedPet)) selectedPet = page[0].id;
                int columns = collectionCompact ? 2 : 3;
                for (int i = 0; i < page.Count; i++)
                    DrawCollectionCard(page[i], new Vector2((collectionCompact ? -505 : -590) + (i % columns) * (collectionCompact ? 490 : 320), 58 - (i / columns) * 273));
                DrawCollectionDetail(catalog.Pet(selectedPet));
            }
            DrawCollectionPages(pageCount);
        }

        private static Color RarityColor(PetRarity rarity)
        {
            switch (rarity)
            {
                case PetRarity.SSS: return new Color(1, .45f, .92f);
                case PetRarity.SS: return new Color(1, .72f, .23f);
                case PetRarity.S: return new Color(1, .88f, .43f);
                case PetRarity.A: return new Color(.82f, .61f, 1);
                case PetRarity.B: return new Color(.55f, .81f, 1);
                default: return new Color(.8f, .87f, .96f);
            }
        }

        private void DrawCollectionCard(PetSpecies species, Vector2 at)
        {
            var owned = Progress.Owned(species.id);
            int level = owned?.level ?? 1;
            var stats = catalog.Stats(species.id, level);
            bool active = owned != null && species.id == Progress.Data.activePetId;
            bool chosen = species.id == selectedPet;
            Color accent = active ? new Color(.4f, 1, .83f) : RarityColor(species.rarity);
            var card = CollectionFrame(collectionCards, "PetCard_" + species.id, at,
                new Vector2(collectionCompact ? 440 : 305, 255), owned != null ? .96f : .78f);
            card.raycastTarget = true;
            var outline = card.gameObject.AddComponent<Outline>();
            outline.effectColor = accent * (chosen ? 1f : .69f);
            outline.effectDistance = chosen ? new Vector2(3, -3) : new Vector2(1.5f, -1.5f);
            if (chosen || species.rarity >= PetRarity.SS)
            {
                var glow = card.gameObject.AddComponent<Shadow>(); glow.effectColor = new Color(accent.r, accent.g, accent.b, chosen ? .72f : .36f);
                glow.effectDistance = new Vector2(0, -3);
            }
            var petSprite = P3ArtLibrary.Get(catalog.Sheet(species.id, level));
            Image portrait = null;
            if (petSprite != null)
            {
                portrait = P1Art.Part(card.transform, "PetPortrait", petSprite, new Vector2(0, 13),
                    new Vector2(242, 177), owned != null ? Color.white : new Color(.19f, .28f, .39f, .94f));
                portrait.preserveAspect = true; portrait.raycastTarget = false;
            }
            else CollectionText(card.transform, "CHƯA CÓ ẢNH", new Vector2(0, 13), new Vector2(220, 80), 18, Color.white);
            var emblem = P3ArtLibrary.Element(species.element);
            if (emblem != null)
            {
                var badge = P1Art.Part(card.transform, "Element", emblem, new Vector2(-119, 92), new Vector2(42, 42), Color.white);
                badge.preserveAspect = true; badge.raycastTarget = false;
            }
            CollectionText(card.transform, species.rarity.ToString(), new Vector2(119, 98), new Vector2(52, 42), 25, accent);
            CollectionText(card.transform, "Lv " + level, new Vector2(-113, 61), new Vector2(64, 30), 17, Color.white);
            if (active) CollectionText(card.transform, "XUẤT CHIẾN", new Vector2(4, 99), new Vector2(140, 30), 17, new Color(.94f, .88f, .54f));
            if (owned == null) CollectionText(card.transform, "🔒", new Vector2(0, 4), new Vector2(74, 70), 39, new Color(1, .85f, .45f));
            CollectionText(card.transform, stats.name, new Vector2(0, -86), new Vector2(collectionCompact ? 405 : 277, 38), 21, new Color(1, .9f, .67f));
            if (active || owned == null)
                CollectionText(card.transform, active ? "ĐANG XUẤT CHIẾN" : "CHƯA SỞ HỮU",
                    new Vector2(0, -115), new Vector2(250, 22), 14,
                    active ? new Color(.58f, 1, .85f) : new Color(.79f, .92f, 1));
            var button = card.gameObject.AddComponent<Button>(); button.targetGraphic = card;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => { P2Click(); selectedPet = species.id; RefreshPetCollection(); });
            var nav = button.navigation; nav.mode = Navigation.Mode.None; button.navigation = nav;
            card.gameObject.AddComponent<PetCollectionCardMotion>().Configure(card.rectTransform, portrait != null ? portrait.rectTransform : null, outline, accent, chosen);
        }

        private void DrawCollectionDetail(PetSpecies species)
        {
            if (species == null) { DrawCollectionEmptyDetail(); return; }
            var owned = Progress.Owned(species.id);
            int level = owned?.level ?? 1;
            var stats = catalog.Stats(species.id, level);
            bool active = owned != null && species.id == Progress.Data.activePetId;
            var frame = CollectionFrame(collectionDetail, "SelectedPetDetail", new Vector2(515, -49), new Vector2(480, 674), .98f);
            var outline = frame.gameObject.AddComponent<Outline>(); outline.effectColor = RarityColor(species.rarity);
            outline.effectDistance = new Vector2(2, -2);
            CollectionText(frame.transform, stats.name, new Vector2(0, 282), new Vector2(448, 58), 26, new Color(1, .9f, .65f));
            CollectionText(frame.transform, species.rarity + "  •  " + P1Catalog.ElementName(species.element),
                new Vector2(0, 239), new Vector2(310, 39), 20, RarityColor(species.rarity));
            var elemental = P3ArtLibrary.Element(species.element);
            if (elemental != null)
            {
                var elementImage = P1Art.Part(frame.transform, "ElementSymbol", elemental,
                    new Vector2(-197, 243), new Vector2(48, 48), Color.white);
                elementImage.preserveAspect = true; elementImage.raycastTarget = false;
            }
            var sprite = P3ArtLibrary.Get(catalog.Sheet(species.id, level));
            if (sprite != null)
            {
                var portrait = P1Art.Part(frame.transform, "SelectedPortrait", sprite,
                    new Vector2(0, 68), new Vector2(410, 305), owned != null ? Color.white : new Color(.18f, .28f, .38f));
                portrait.preserveAspect = true; portrait.raycastTarget = false;
            }
            else CollectionText(frame.transform, "CHƯA CÓ ẢNH", new Vector2(0, 55), new Vector2(300, 80), 20, Color.white);
            string formLabel = species.forms == null || species.forms.Length == 0 ? "Dạng nguyên bản" :
                               stats.permanentForm == 0 ? "Hình thái gốc" : "Hình thái tiến hóa";
            CollectionText(frame.transform, "Lv " + level + "  •  " + formLabel, new Vector2(0, -111),
                new Vector2(424, 35), 19, new Color(.9f, .95f, 1));
            CollectionText(frame.transform, "♥ HP " + stats.maxHp + "      ⚔ ATK " + stats.attack + "      ◆ DEF " + stats.defense,
                new Vector2(0, -153), new Vector2(448, 37), 17, Color.white);
            CollectionButton(frame.transform, "CHI TIẾT", new Vector2(-112, -219), new Vector2(205, 57),
                () => { previewEvolution = false; Navigate("Detail"); }, owned != null);
            CollectionButton(frame.transform, species.forms != null && species.forms.Length > 0 ? "TIẾN HÓA" : "NÂNG CẤP",
                new Vector2(112, -219), new Vector2(205, 57), () => Navigate("Upgrade"), owned != null,
                new Color(1, .76f, .38f));
            CollectionButton(frame.transform, owned == null ? "CHƯA SỞ HỮU" : active ? "ĐANG XUẤT CHIẾN" : "XUẤT CHIẾN",
                new Vector2(0, -293), new Vector2(330, 51),
                () => { if (Progress.Select(species.id)) { message = "Đã chọn " + species.title + " xuất chiến."; RefreshPetCollection(); } },
                owned != null && !active, new Color(.48f, 1, .8f));
            if (owned == null) CollectionText(frame.transform, "Điều kiện thu phục chưa được công bố.",
                new Vector2(0, -328), new Vector2(435, 22), 14, new Color(.7f, .79f, .86f));
        }

        private void DrawCollectionEmptyDetail()
        {
            var frame = CollectionFrame(collectionDetail, "EmptyPetDetail", new Vector2(515, -49), new Vector2(480, 674), .97f);
            CollectionText(frame.transform, "CHỌN MỘT PET", new Vector2(0, 40), new Vector2(430, 65), 27, new Color(1, .9f, .65f));
            CollectionText(frame.transform, "Khung này sẽ hiển thị hình thái, chỉ số\nvà các thao tác của pet đã chọn.",
                new Vector2(0, -25), new Vector2(425, 100), 18, Color.white);
        }

        private void DrawCollectionPages(int pageCount)
        {
            CollectionButton(collectionPagination, "‹", new Vector2(-86, -376), new Vector2(43, 40),
                () => { petPage--; RefreshPetCollection(); }, petPage > 0);
            int first = Mathf.Clamp(petPage - 2, 0, Math.Max(0, pageCount - 5));
            for (int i = first; i < Math.Min(pageCount, first + 5); i++)
            {
                int page = i;
                CollectionButton(collectionPagination, (i + 1).ToString(),
                    new Vector2(-31 + (i - first) * 48, -376), new Vector2(43, 40),
                    () => { petPage = page; RefreshPetCollection(); }, true,
                    i == petPage ? new Color(.3f, .96f, 1) : new Color(.49f, .66f, .78f));
            }
            CollectionButton(collectionPagination, "›", new Vector2(238, -376), new Vector2(43, 40),
                () => { petPage++; RefreshPetCollection(); }, petPage + 1 < pageCount);
        }
    }

    public sealed class PetCollectionCardMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform rect, portrait;
        private Outline outline;
        private Color accent;
        private Vector2 original;
        private bool selected, hovered;
        private float hoverAmount;

        public void Configure(RectTransform card, RectTransform petImage, Outline border, Color color, bool isSelected)
        {
            rect = card; portrait = petImage; outline = border; accent = color; selected = isSelected;
            original = card.anchoredPosition;
        }
        public void OnPointerEnter(PointerEventData data) { hovered = true; }
        public void OnPointerExit(PointerEventData data) { hovered = false; }
        private void Update()
        {
            if (rect == null) return;
            hoverAmount = Mathf.MoveTowards(hoverAmount, hovered ? 1f : 0f, Time.unscaledDeltaTime / .19f);
            float eased = hoverAmount * hoverAmount * (3f - 2f * hoverAmount);
            rect.anchoredPosition = original + new Vector2(0, 5f * eased);
            if (portrait != null) portrait.localScale = Vector3.one * (1f + .045f * eased);
            if (outline != null)
            {
                float glow = selected ? 1f : Mathf.Lerp(.65f, .96f, eased);
                outline.effectColor = new Color(accent.r, accent.g, accent.b, glow);
            }
        }
    }
}
