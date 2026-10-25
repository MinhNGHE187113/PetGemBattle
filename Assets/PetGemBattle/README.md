# Pet Arena Classic P1

Bản hiện hành P1: mở **Tools > Pet Gem Battle > Open P1 Home**, nhấn Play. Windows khởi động vào sảnh. Hướng dẫn đầy đủ: [P1 Guide](../../Docs/P1-Guide.md). Bàn giao/test: [P1 Acceptance](../../Docs/P1-Acceptance.md).

Có sảnh, 7 boss Mộc/thu phục, kho Pet/đá, Lv1–Lv7, Ngũ Hành, SPD, save và cinematic combat. **89/89 EditMode + 24/24 PlayMode đạt**. Art/rig P1 là bộ tạm có bộ phận chuyển động và Animator events, cần trau chuốt trước phát hành.

Thông tin bên dưới là ghi chú P0, dùng cho scene Battle cũ; thông số/phạm vi P1 lấy theo tài liệu P1 bên trên.

# Pet Arena Classic — PvE Unity 2D

## Chạy trong Unity

1. Mở project `D:\UnityProjects\PetGemBattle` bằng Unity **6000.3.27f1**.
2. Chọn **Tools > Pet Gem Battle > Open Battle Scene**, hoặc mở `Assets/Scenes/PetGemBattle.unity`.
3. Nhấn **Play**, chọn tab **Game**. Chọn tỷ lệ **16:9**, nhấp đúp tab Game để xem lớn.
4. Kéo ngọc sang ô liền kề, hoặc nhấp lần lượt hai ô. Nước không tạo match được hoàn tác và không mất lượt.
5. Mỗi lượt được dùng một thẻ trước khi đổi ngọc: **Tiếp Mana +20** (2 lần/trận), **Khơi Nộ +20** (2 lần/trận), **Tiến Hóa** (80 Mana, 1 lần/trận).
6. Hết 30 giây sẽ tự chọn nước hợp lệ. Đồng hồ dừng trong lúc giải quyết hoạt ảnh/combo. Sau nước đi, AI thực hiện lượt rồi trả lượt.
7. Nút chơi lại hủy hoạt ảnh đang chạy và khởi tạo trận mới; màn hình kết quả cũng có nút chơi lại.

UI được tạo lúc Play nên Scene trước Play chỉ có camera và controller. Không cần kéo sprite hoặc gán prefab thủ công.

## Đã triển khai

- Bàn 8×8, sáu ngọc: kiếm vàng, hồi HP xanh lá, Mana xanh dương, Nộ đỏ, giáp tím, hút xám. Match giao nhau không đếm trùng; rơi, sinh bù, combo, xáo bàn hết nước đi.
- Bố cục fantasy, Canvas tham chiếu 1920×1080: nền đền cổ, hai Pet lớn, HUD HP/Mana/Nộ/Giáp, timer trung tâm, thẻ dưới bàn, khung vàng/ngọc, hào quang, số nổi, thanh chỉ số nội suy.
- Hỏa Long Chiến Thần và Lôi Băng Thần Thú, mỗi Pet có ảnh thường/tiến hóa. Idle, đánh thường, đặc biệt, trúng đòn, tiến hóa, thắng và thua dùng chuyển động lập trình cùng hiệu ứng. Âm thanh tổng hợp riêng cho hai Pet và hai loại đòn.
- Chỉ số khởi đầu 300 HP, 25 ATK, 8 DEF; Mana/Nộ 0/100; giáp tối đa 20% MaxHP.
- Ngọc kiếm cộng Nộ trước khi xét đặc biệt: +10 cho 3 viên, +4 mỗi viên thêm. Đủ 60 dùng đòn đặc biệt, trừ đúng 60, giữ phần dư. Ngọc đỏ chỉ cộng Nộ.
- Tiến hóa: MaxHP ×1.4, ATK ×1.35, DEF ×1.2; hồi 40% MaxHP cũ; trừ 80 Mana; giữ Nộ và giáp. Không tự tiến hóa và không tiến hóa lần hai.
- AI chọn thẻ, tiến hóa và nước đi dựa trên chỉ số hiện tại; ưu tiên kết liễu chắc chắn. Không đọc trước ngọc ngẫu nhiên sẽ sinh.
- Thắng/thua/hòa, khóa tương tác khi xử lý, chơi lại giữa hoạt ảnh, RNG riêng cho bàn cờ và xác suất hút.

## Công thức và thứ tự

`n` là số ngọc duy nhất trong nhóm; `C = 1 + 0.25 × (đợt combo - 1)`.

| Ngọc | Hiệu ứng |
| --- | --- |
| Kiếm | Power = ATK × (1.8 + 0.35 × (n - 3)) × C; thường = max(1, round(Power - DEF × 0.5)); đặc biệt = round(thường × 1.75); chí mạng = round(sát thương đã tính × 1.7) |
| HP | round(MaxHP × (0.04 + 0.015 × (n - 3)) × C), tối đa 10% MaxHP mỗi nhóm |
| Mana | round(8 × n × C) |
| Nộ | round(7 × n × C) |
| Giáp | round(MaxHP × (0.03 + 0.01 × (n - 3)) × C), chặn ở 20% MaxHP |
| Hút HP | Xác suất min(0.55, 0.30 + 0.05 × (n - 3)); lượng round(HP hiện tại đối thủ × 0.08 × C), tối đa 12% MaxHP đối thủ |
| Hút Nộ | Xác suất min(0.50, 0.25 + 0.05 × (n - 3)); lượng round(Nộ đối thủ × 0.25 × C) |
| Hút giáp | Xác suất như hút Nộ; lượng round(Giáp đối thủ × 0.30 × C) |

Mỗi nhóm hút dùng ba lần thử độc lập. Giáp hấp thụ sát thương trước HP; hút HP chỉ hồi lượng HP thực sự mất. Không hút quá tài nguyên còn lại; bên nhận vẫn chịu giới hạn. Trong một nhóm hút, xử lý HP → Nộ → giáp, nên lượng giáp hút tính sau khi giáp đã chặn hút HP.

Trong một đợt match, xử lý nhóm tài nguyên → hút → kiếm, cùng loại theo vị trí trên bàn. Nhóm đỏ cùng đợt có thể giúp kiếm kích hoạt đặc biệt. Kiểm tra tử trận sau mỗi nhóm, ngừng các nhóm còn lại. Làm tròn midpoint ra xa 0; chuẩn hóa sai số float nhỏ trước khi làm tròn số nguyên. Giới hạn 100 đợt combo để tránh vòng lặp; khi vượt giới hạn sinh bàn ổn định và chuyển lượt.

## File chính

Tất cả đường dẫn dưới đây tính từ `Assets/PetGemBattle/`.

| File | Vai trò |
| --- | --- |
| `Scripts/Core/BattleRules.cs`, `Data/ClassicBattleRules.asset` | Cấu hình công thức, chỉ số, giới hạn và thời gian |
| `Scripts/Core/BattleCombat.cs` | PetState, công thức, đặc biệt, hút và kết quả |
| `Scripts/Core/BattleRandom.cs` | RNG có seed và quy tắc làm tròn |
| `Scripts/Core/CardSystem.cs` | Thẻ bài, số lần dùng, khóa theo lượt, tiến hóa |
| `Scripts/Core/BattleAI.cs` | Chấm điểm nước và chọn thẻ |
| `Scripts/Core/MatchBoard.cs`, `Scripts/Runtime/GemInput.cs` | Tái sử dụng bàn cờ và chuột/drag đang hoạt động |
| `Scripts/Runtime/BattleController.cs` | Lượt, timer, combo, thẻ, AI, restart |
| `Scripts/Runtime/BattleView.cs` | HUD, thẻ, bàn, số nổi, hiệu ứng và kết quả |
| `Scripts/Runtime/PetPresentation.cs` | Hai hình thái và chuyển động Pet |
| `Scripts/Runtime/BattleAudio.cs` | Âm thanh tổng hợp tại runtime |
| `Scripts/Runtime/PlaceholderArt.cs` | Ngọc, biểu tượng, vòng hào quang và nạp ảnh Pet/nền |
| `Resources/BattleArt/` | 4 PNG Pet trong suốt và 1 PNG nền, tạo bằng image generation tích hợp |
| `Editor/BattleArtImporter.cs` | Thiết lập import alpha, clamp, không mipmap |
| `Editor/BattleTestRunner.cs` | Menu chạy test và lưu XML |
| `Tests/Editor/BattleCoreTests.cs` | Kiểm thử công thức, RNG, thẻ, bàn và trận mô phỏng |
| `Tests/PlayMode/BattleFlowTests.cs` | Kiểm thử luồng UI, tiến hóa, timer, AI và restart |

Scene và asset cũ được giữ GUID để không mất tham chiếu. Cấu trúc skill cũ còn ở model cho loadout tương lai; ba thẻ mới là giao diện hiện hành theo yêu cầu nâng cấp.

## Kiểm thử và phạm vi

Khi không Play, chọn **Tools > Pet Gem Battle > Run Core Tests** hoặc **Run Gameplay Tests**. Menu lưu XML trong `Logs/PetGemBattle-TestResults.xml` và `Logs/PetGemBattle-PlayModeResults.xml`. Bộ kiểm thử cân bằng mới nằm trong `Tests/Editor/BattleBalanceTests.cs`; báo cáo mô phỏng 1.000 trận lưu ở `Logs/BalanceSimulation.md`.

Animation hiện là chuyển động/biến dạng sprite và hiệu ứng lập trình, chưa có skeletal rig hoặc sprite sheet vẽ từng khung. Âm thanh là âm tổng hợp prototype, có 12 SFX và nhạc ambient tổng hợp nguyên bản; chưa có thu âm chuyên nghiệp. Bản Windows đã build và mở chơi thử. Chưa hỗ trợ mobile/touch/màn hình dọc. Chưa có lưu tiến trình, đổi đội, khắc chế nguyên tố hoặc online/backend; metadata hệ vẫn được giữ. Cần playtest thêm để cân bằng độ dài trận.


## Bản cân bằng và chí mạng — 25/10/2026

Mỗi nhóm kiếm tung đúng một lần RNG để xét chí mạng (không theo chu kỳ). Xác suất = min(40%, 15% + 5% × (n - 3) + 5% nếu tiến hóa). Dùng chung RNG trận có seed với ngọc hút; AI chấm điểm bằng sát thương chắc chắn và kỳ vọng chí mạng, không tiêu thụ RNG thực tế khi suy nghĩ.

Thứ tự tính: cộng Nộ → xét ngưỡng 60 → round sát thương thường sau DEF ×0.5 → nếu đặc biệt round ×1.75 → nếu chí mạng round ×1.7 → round hệ số Sudden Death → trừ giáp rồi HP → tiêu hao đúng 60 Nộ nếu đặc biệt. Model cập nhật nguyên tử trước hoạt ảnh; BattleReport mang kết quả để hiển thị đồng bộ. Với 25 ATK, đối thủ 8 DEF và 3 kiếm: thường 41, thường chí mạng 70, đặc biệt 72, đặc biệt chí mạng 122. Nộ đỏ vẫn không tự đánh.

Một vòng hoàn thành sau cả lượt người chơi và lượt AI. Cuối vòng 15 thông báo Sudden Death; vòng 16–17 +10%, vòng 18–19 +20%, 20–21 +30%, … vòng 26–30 +60%. Hệ số áp dụng cuối công thức kiếm (bao gồm đặc biệt/chí mạng), không tăng hồi/giáp. Ngọc hút giữ nguyên công thức và trần cũ để không tăng hồi gián tiếp. Cuối vòng 30, nếu chưa hạ HP, so sánh chính xác tỷ lệ HP bằng nhân chéo số nguyên; giáp không tham gia, tỷ lệ bằng nhau thì hòa. Restart xóa toàn bộ tiến trình vòng và trạng thái kết quả.

Hiệu ứng bổ sung: chém thường màu trắng/vàng, chữ CRITICAL! và số lớn vàng cam, aura riêng cho đặc biệt, kết hợp đặc biệt chí mạng; ngọc hút dùng biểu tượng xoáy xám. HUD hiển thị vòng hiện tại/30 và mức tăng Sudden Death; màn kết quả ghi tỷ lệ HP và lý do giới hạn vòng.

Các hệ số chỉnh được trong `Data/ClassicBattleRules.asset`: attackMultiplier, swordExtraMultiplier, defenseMultiplier, specialMultiplier, criticalChance/criticalExtraChance/evolvedCriticalBonus/criticalChanceCap/criticalMultiplier, healBasePercent/healExtraPercent/maxHealPercent, shieldBasePercent/shieldExtraPercent/maxShieldPercent, suddenDeathAfterRounds/suddenDeathStepRounds/suddenDeathStep/suddenDeathMaxBonus/maxRounds.

## Cập nhật theo đặc tả đầy đủ ngày 25/10/2026

- Mọi nhóm 4 viên thưởng thêm 5 Nộ; 5+ viên thưởng 10 Nộ, một lần/nhóm, không nhân combo. Thưởng cộng trước hiệu ứng của nhóm. Hai dãy match cùng màu chạm cạnh thuộc cùng nhóm.
- Hút Nộ/giáp giới hạn cả tài nguyên nguồn lẫn chỗ trống bên nhận. Số trừ đối thủ bằng số thực nhận.
- Tiến hóa giữ ATK 33,75 và DEF 9,6; chỉ HP/giáp/sát thương làm tròn số nguyên.
- Tạm dừng khóa thao tác, timer, AI và hoạt ảnh; Tiếp tục hoặc Chơi lại khôi phục thời gian. Âm thanh bật/tắt được lưu bằng PlayerPrefs.
- BattleEvent chứa snapshot trước/sau mỗi nhóm, số ngọc, combo và roll chí mạng. BattleJournal lưu seed, cấu hình, swap/thẻ, các roll combat và snapshot bàn. Khi kết thúc trận, JSON được ghi vào `Application.persistentDataPath/BattleLogs/latest-battle.json`, giữ `.bak` và ghi qua file tạm. Đây là nhật ký chẩn đoán, chưa phải trình phát replay hoặc save tiến trình.
- AI hòa điểm chọn bằng RNG có seed riêng; không tiêu thụ RNG chiến đấu khi chấm điểm.
- Config được kiểm tra trước khi bắt đầu; giá trị NaN, vô hạn, ngoài giới hạn hoặc chi phí không thể đạt bị từ chối.

Báo cáo trước cập nhật: `Docs/Balance-Before.md`. Báo cáo hiện tại: `Logs/BalanceSimulation.md`. 1.000 seed: trung bình 14,36 vòng, trung vị 15; 100% hạ HP trước vòng 30; bên đi trước thắng 56,8%. Ước tính 4,18 phút với 12 giây suy nghĩ/lượt người chơi, chưa phải thời gian đo ở người thật.

## Mở rộng dữ liệu

Chỉnh `Data/ClassicBattleRules.asset` trong Inspector để đổi công thức, thời gian và loadout hai bên. PetDefinition hiện là dữ liệu serializable nằm trong BattleRules; không phải asset Pet độc lập. Đổi `player`/`enemy`, đặt `art` trùng tên PNG trong `Resources/BattleArt`, cung cấp ảnh `<art>Evolved.png` để thêm ngoại hình. Hệ nguyên tố chỉ là metadata.

Ba loại thẻ dùng CardKind + CardSystem và hệ số trong BattleRules. Thẻ có hành vi mới cần bổ sung resolver, AI, UI và test; ngọc mới cần cập nhật GemKind, số loại, resolver, biểu tượng và test board. Không tuyên bố có hệ thống plugin thẻ/ngọc không cần code.

Phạm vi đợt này là P0 theo mục 10.3 của đặc tả. Sảnh, ba ải, XP/Gold, thu phục, save tiến trình và PvP vẫn thuộc P1–P3. Giữ cấu trúc Assets/PetGemBattle để bảo toàn GUID/tham chiếu hiện có. Logic board/combat không phụ thuộc GameObject; BattleRules vẫn là ScriptableObject, chưa đóng gói thành thư viện server độc lập.

## Build Windows

Thoát Play rồi chọn **Tools > Pet Gem Battle > Build Windows**. Đầu ra: `Builds/Windows/PetArenaClassic.exe`. Khi sao chép game, giữ toàn bộ thư mục Windows, gồm thư mục Data và DLL cạnh exe. Báo cáo build: `Logs/WindowsBuild.txt`.

Kiểm chứng đợt đặc tả: **59/59 EditMode + 15/15 PlayMode đạt**. Windows build thành công, 0 lỗi; 485 cảnh báo chủ yếu shader ConvGeneric thuộc Unity Inference. Đã mở bản độc lập, kiểm tra HUD/thẻ/pause/restart và swap bằng hai lần click, xem bố cục ở Full HD, 1366×768 và 1600×900. Drag đạt test PointerEventData; chưa xác nhận thao tác kéo thực bằng công cụ điều khiển chuột.
