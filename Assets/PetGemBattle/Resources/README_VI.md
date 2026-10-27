# BỘ PET REMASTER V2 — GAME 2D COMBAT KIM CƯƠNG

## Nội dung bàn giao

- **30 loài pet trong `P1Catalog.asset`** đã có artwork mới theo phong cách *2D anime fantasy creature / clean cel-shading* tham khảo từ chim trắng–xanh–vàng.
- **2 pet chiến đấu trong `BattleArt`**: Rồng Lửa và Sói Băng Sét, đều có hai hình thái.
- **32 ảnh master** trong `Pets/`, mỗi ảnh thể hiện trọn một loài (một hình thái hoặc hai hình thái đặt cạnh nhau).
- **44 PNG tách hình thái** trong `Ordered_Sprites/`, tên có số thứ tự và `form-1` / `form-2`; tất cả có kênh alpha.
- **2 ảnh tổng quan** trong `Preview/` để rà nhanh bộ nhân vật.
- **`UnityPatch/P1Catalog.asset`** là bản sao catalog đã rút chuỗi ba hình thái còn hai. Chỉ thay đổi hai chuỗi nêu dưới.

## Tiêu chuẩn mỹ thuật

- Viền tối sắc, mảng màu rõ, hard-edge/cel shading, giáp khắc góc cạnh và pha lê làm điểm nhấn.
- Giữ hệ nguyên tố nhưng **không chỉ đổi màu để tạo pet mới**: mỗi loài phải có dáng và bộ phận nhận diện riêng.
- Pet nhỏ có hình khối dễ đọc; cấp S/SS tăng độ tinh xảo theo cấp.
- Không có chữ, UI, phông nền hoặc đổ bóng nền trong PNG nhân vật.

## Tiến hóa: ba hình thái thành hai hình thái

| Pet | Trước | Sau | Hình thái bị bỏ |
|---|---|---|---|
| `wood-7` — Cổ Thụ Vệ Thần | Lv1 → Lv4 → Lv7 | **Lv1 → Lv7** | Lv4, tức `wood-7-form1` |
| `fire-lion-s` — Viêm Tinh Sư | Lv1 → Lv4 → Lv7 | **Lv1 → Lv7** | Lv4, tức `fire-lion-s-form1` |

Trong file catalog chỉnh sửa, multiplier tại Lv7 đã được gộp từ hai lần tiến hóa cũ để **giữ tổng hệ số cuối** (HP 1.232, ATK 1.3216, DEF 1.188). Các chuỗi vốn có một hoặc hai hình thái không bị thay đổi số cấp.

## Cấu trúc và cách dùng

```text
Pets/
  01_Wood/                 7 loài hệ Mộc
  02_Fire/                 sư tử lửa cơ bản
  03_Metal/                pet Kim cơ bản
  04_Water/                rái cá nước
  05_Earth/                rùa đất
  06_Legendary/            S/SS và thần thú
  07_Zodiac/               linh thú độc lập, 8 loài
  08_Battle/               hai pet chiến đấu độc lập
Ordered_Sprites/           001_... đến 032_..., từng hình thái độc lập
Preview/                   bảng xem nhanh
UnityPatch/P1Catalog.asset file catalog đề xuất
```

### Lưu ý kỹ thuật khi đưa vào Unity

**Đây là gói artwork và patch dữ liệu tiến hóa, chưa phải bản cập nhật có thể tự động thay thế trực tiếp toàn bộ Unity project.** ZIP gốc chứa một số atlas P3 và prefab tham chiếu tới `artSheet`/`sheet` cũ. Vì không có mã nguồn loader/sprite-slicing đi kèm nên không thể xác nhận cơ chế nạp trực tiếp các PNG mới.

1. Giữ bản sao dự án trước khi tích hợp.
2. Nhập `Ordered_Sprites` vào `Assets/Resources/PetRemaster/` (hoặc thư mục asset mới trong dự án).
3. Đặt Texture Type = **Sprite (2D and UI)**, Sprite Mode = **Single**, Alpha Is Transparency = **On**; thử Compression = None trên PC rồi tối ưu cho máy đích.
4. Dùng `DANH_SACH_PET.csv` để nối tên `petId` hiện tại với file `NNN_petId_form-N.png` tương ứng. Giữ nguyên ID, chỉ thay art lookup.
5. Kiểm thử prefab, hướng nhìn, tỉ lệ sprite, animation và hiệu ứng trước khi thay atlas cũ. Các sprite này là minh họa tĩnh, **không phải frame animation đã được rig**.
6. Nếu dùng patch catalog, thay nội dung `P1Catalog.asset` trong project nhưng **giữ file .meta/GUID gốc của project**. `artSheet` trong patch vẫn là tên cũ; cần remap tới `Ordered_Sprites` trong loader.

## Danh sách và thứ tự

Thứ tự của 30 pet đầu tiên bám sát danh sách `pets:` trong `P1Catalog.asset`. Hai cặp trong `BattleArt` được thêm cuối, số 031–032. File `DANH_SACH_PET.csv` ghi rõ danh tính, nhóm và số hình thái. Có thể dùng `Pets/` làm tranh tổng hợp hoặc `Ordered_Sprites/` để tích hợp từng hình thái.

## Ghi chú chất lượng

Art style thống nhất theo ảnh chim mẫu nhưng điều chỉnh giải phẫu theo từng loài: ếch bè và chân có màng, nấm có tán, nhím mang gai hoa, cua thân ngang/càng lớn, cá đèn lồng có mồi sáng, cá đuối có cánh vây dẹt, Kirin dạng long mã, Chimera dạng quái thú núi lửa và các loài chim khác nhau ở beak/wings/tail.

Artwork được tạo bằng AI và đã được xem lại ở kích thước tổng quan. Trước khi sản xuất chính thức nên kiểm tra thêm viền alpha/chi tiết nhỏ ở kích thước nhân vật thực tế trong màn combat.
