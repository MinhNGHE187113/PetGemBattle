# P2 — Nghiệm thu và giới hạn

Đối chiếu cả `Pet_Arena_P2_Visual_Gameplay_Codex_Requirements.md` và `Pet_Arena_Classic_P2_Visual_Gameplay_Update.docx`. P2 ghi đè quy tắc P1 tại các mục đã thay đổi. Tái sử dụng project/Scene/battle/save đang có.

## Done

| Hạng mục | Triển khai / bằng chứng |
|---|---|
| Luồng Home → 5 đảo → Mộc 7 ải → boss → trận → kết quả | HomeController + P2Screens, stack Back/Home; P2FlowTests |
| Preview 4 đảo còn lại | Artwork và Pet riêng, boss khóa thật; P2FlowTests |
| Bộ sưu tập demo | 12 Pet/5 hệ Lv1, cấp đá một lần/save, reset xác nhận; P2Tests + P2FlowTests |
| Art mới dùng trong Unity | 26 PNG, 89 sprite subasset, 19 prefab Pet/form, 5 material đá; không dùng ảnh tham khảo làm sprite |
| Rig / animation | Head, torso, accessory chuyển động độc lập; Animator và event impact; idle/attack/special/hit/evolve/victory/defeat |
| Form vĩnh viễn | Các mốc Lv4/Lv7 theo đặc tả; lấy từ level, không cộng lặp; Cổ Thụ Lv7 887/78/24/43 |
| Nâng cấp | Đúng hệ/cấp đá, before/after và form preview; xác nhận expectedLevel + transaction; không trừ đá lặp |
| Boss P2 | HP/scale/cap theo bảng, chỉ boss mới dùng profile; legacy rules được bảo vệ khỏi inline serialization |
| Mana + Nộ | Từng nhóm kiếm cần cả 60 Nộ và 30 Mana; model chuẩn bị một lần; kiểm thử 72/29 và 72/35 |
| Boss ≤30% | Buff/giáp một lần, không heal/extra turn, skip khi lethal; animation xen đúng lượt |
| Combat presentation | Thu hết cascade, ẩn bàn, cập nhật HUD tại impact, hiện lại sau queue; 2× không đổi decision timer |
| Save | JSON v2, migration P1, backup, giữ active/level, giao dịch chống lặp |
| Đá | 7 silhouette sơn riêng; shader lõi/khắc văn 5 hệ; chi tiết nguồn rơi/số lượng |
| Hiệu năng cơ bản | Reuse 64 tile, pool image VFX, cache sprite; PlayMode kiểm tra identity tile/reset queue |

## Partial — cần đánh giá/tinh chỉnh thêm

- **Cân bằng ải cuối:** 700 trận AI–AI dùng Thiết Điểu ở cấp khuyến nghị. P2 thắng 49/50 ở ải1 và 28/50 ở ải7; trung vị 15→26 vòng, 5→8 lượt người chơi có kiếm. Có 12/50 trận ải7 chạm giới hạn 30 vòng. Chưa đạt mục tiêu nhịp 10–16 lượt kiếm ở ải7; cần playtest người thật và tuning thêm. Không tự ý đổi các hệ số/HP P2 đã yêu cầu. Thời gian 4.65→7.86 phút là **ước lượng với 12 giây suy nghĩ**, không phải đo thời gian người chơi.
- **Animation/art polish:** asset là tranh gốc có rig cutout, không phải hoạt họa vẽ tay từng frame hay skeleton Spine. Các bộ phận dùng profile vị trí theo loài; nét nối, mắt chớp và pose còn cần art pass thủ công trên nhiều kích thước. Không tuyên bố chất lượng phát hành thương mại.
- **Kiến trúc UI:** các màn hoạt động và dùng stack điều hướng nhưng UI vẫn được dựng runtime trong HomeController/P2Screens; chưa tách mọi màn thành một UI prefab/controller độc lập. Pet/form đã có prefab dùng được.
- **Đá:** 7 tranh silhouette gốc + 5 shader engraving/material, không phải 35 bức tranh vẽ riêng.
- **Hiệu năng:** có pooling và kiểm thử dài ở model; chưa có capture Profiler FPS/memory trên máy cấu hình thấp hoặc thiết bị di động. Text nổi vẫn tạo theo sự kiện.
- Âm thanh UI/chiêu tiếp tục dùng âm tổng hợp cục bộ; chưa có nhạc nền/motion soundtrack sản xuất riêng.

## Coming Soon

Boss/7 ải thực tế của Hỏa, Kim, Thủy, Thổ; PvP, cửa hàng, bang hội, nhiệm vụ, xếp hạng, triệu hồi, ghép/đổi đá. Các nút đã khóa, không giả trận khác hệ.

## Kiểm thử tự động

- EditMode: **122/122 đạt**, `Logs/P2-EditMode.xml` (bao gồm 700 trận so sánh P1/P2 và kiểm thử cũ).
- PlayMode: **37/37 đạt**, `Logs/P2-PlayMode.xml` (13 ca tích hợp P2 + 24 ca trước).
- Các ca P2-01..30 được gom trong P2Tests/P2FlowTests/P2BalanceTests: điều hướng, đầy đủ Pet, reload, form, đúng đá, double-click, boss HP/caps, mana/rage mỗi đòn, thức tỉnh/sát thương kết liễu, trình tự queue, HUD, 2×, asset đá, reward, persistence, buff tạm và reuse tile.
- Ca thức tỉnh PlayMode dùng hit xác định 154→131 HP để tránh random crit/cascade kết liễu boss trước khi qua ngưỡng.

## Kiểm tra hình ảnh

Sảnh đã được mở trong Windows standalone. Bộ ảnh tiếp theo được render bằng Unity từ các màn UI thật, với save QA riêng và camera 1600×900: Home, Islands, WoodMap, FirePreview, Collection, FireStones, Upgrade, Battle, Combat. Đây là ảnh engine render, không phải mockup hoặc ảnh giao diện được vẽ lại.

Đã sửa các lỗi tìm thấy qua QA: mảnh ảnh lân cận lọt vào Sprite Rect, mắt chớp đặt trên trán, Pet che tiêu đề nâng cấp, bộ lọc Sách Pet không áp dụng form/level, preview boss bị lẫn form Pet sở hữu. HUD nay ghi rõ form vĩnh viễn và buff thẻ/boss riêng.

- [Sảnh](P2-Home.png)
- [Năm đảo](P2-Islands.png)
- [Bản đồ Mộc](P2-WoodMap.png)
- [Preview Hỏa](P2-FirePreview.png)
- [Bộ sưu tập](P2-Collection.png)
- [Đá Hỏa](P2-FireStones.png)
- [Cổ Thụ Lv7](P2-Upgrade.png)
- [Bàn 8×8](P2-Battle.png)
- [Combat ẩn bàn](P2-Combat.png)

Màn loading riêng và transition giữa toàn bộ menu chưa có art/animation riêng; hiện dùng splash Unity và đổi menu runtime. Transition ẩn/hiện bàn combat đã hoạt động.

Kết quả build Windows cuối được lưu tại `Logs/WindowsBuild.txt`; không tính một build có shader error là đạt dù Unity ghi Succeeded. Build script hiện dừng khi totalErrors>0.


Build cuối: **Succeeded, 0 errors, 485 warnings, 314,684,464 bytes**. Các cảnh báo còn lại thuộc shader/package Unity Inference (đã có từ P1); không có shader error của P2 trong log build cuối. Binary và data/Managed mới nằm trong `Builds/Windows`.
