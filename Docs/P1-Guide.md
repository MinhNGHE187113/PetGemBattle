# Pet Arena Classic P1

## Mở và chơi

Unity 6000.3.27f1. Chọn Tools > Pet Gem Battle > Open P1 Home, rồi Play. Entry scene: Assets/Scenes/P1Home.unity. Scene PetGemBattle cũ vẫn giữ để thử P0. Không đổi package hoặc Input System.

Khởi đầu có Kim Miêu bậc C hệ Kim. Vào Đảo Boss, chọn ải 1, xem SPD/tương khắc và bắt đầu. Kim khắc Mộc +25%. Thắng lần đầu bắt Pet 100%, mở ải sau và nhận đá. Chọn Pet Mộc đã bắt trong Kho Pet để dùng đá Mộc nâng cấp. Pet Kim khởi đầu chưa có nguồn đá nâng trong P1.

Kho Pet có bộ lọc hệ, bậc, trạng thái đang xuất chiến và sắp xếp Lv/bậc/HP/ATK/SPD. Sách Pet chỉ hiển thị silhouette khi chưa sở hữu. Kho đồ hiển thị 35 định danh đá; bấm cấp đá để xem công dụng/nguồn rơi. Đá Lv1 chỉ sưu tầm; từ Lv1 lên Lv2 cần một đá cùng hệ Lv2. Mỗi lần xác nhận lên đúng một cấp, tối đa Lv7.

Mỗi lượt thu toàn bộ cascade trước, đóng băng kế hoạch combat trên bản sao state, ẩn bàn rồi chạy hỗ trợ → hút → từng nhóm kiếm. Toàn bộ Nộ kiếm được nạp trước các đòn, từng special trừ riêng 60. Sát thương cập nhật tại impact, không roll RNG lại trong renderer. Nút x1/x2 thay tốc độ trình diễn; đồng hồ quyết định vẫn dùng giây thực. Về đảo hủy phiên hiện tại; trận đang bỏ dở không có thưởng.

## Cấu hình

- Assets/PetGemBattle/Resources/P1Catalog.asset: 8 loài (7 boss + starter), 6 bậc tham chiếu, 7 ải, vòng khắc, tăng chỉ số theo Lv và drop. Dữ liệu có thể chỉnh Inspector, không nằm trong UI.
- Assets/PetGemBattle/Resources/P1BattleRules.asset: các công thức P0, cinematicTurns, thời gian hide/show, đòn thường/đặc biệt/evolution, timer, giới hạn vòng.
- Công thức cấp: HP +10%, ATK +9%, DEF +7% trên mỗi cấp thêm tính từ Base; SPD +2/cấp. HP/ATK/DEF làm tròn midpoint ra xa 0. Tiến hóa trận không ghi ngược vào loài/cấp vĩnh viễn.
- Kim → Mộc → Thổ → Thủy → Hỏa → Kim. Thuận ×1,25; ngược ×0,85; còn lại ×1. Chỉ kiếm chịu hệ số, sau crit và trước round cuối cùng cùng Sudden Death. Hút/hồi/giáp không chịu khắc hệ.
- SPD cao đi trước, hòa người chơi đi trước; mỗi hai lượt hoàn chỉnh mới tính một vòng, kể cả boss đi trước.
- Boss HP buff 1,00 đến 1,15 theo ải; Pet bắt được luôn dùng base loài, Lv1.

## Lưu và tính toàn vẹn

JSON schemaVersion 1: Application.persistentDataPath/p1-save.json. Trên Windows mặc định: %USERPROFILE%/AppData/LocalLow/DefaultCompany/PetGemBattle/p1-save.json. Có file .bak và ghi qua .tmp, flush trước thay file. File chính hỏng thử backup; dữ liệu không hợp lệ bị từ chối. Migration version 0 bổ sung danh sách giao dịch/lịch sử.

Thưởng có battleId; gọi lại cùng ID không thưởng nữa. Mỗi tái đấu tạo battleId mới. Nâng cấp yêu cầu transaction ID và expectedLevel; double-click cũ không nâng lần hai. Tạo bản sao save, cập nhật đá/cấp/thu phục cùng nhau, ghi thành công mới thay dữ liệu đang dùng. Lỗi ghi không trừ vật phẩm trong bộ nhớ. Thưởng được commit ngay khi kết thúc model, trước animation thu phục.

## Mỹ thuật và animation

P1 dùng rig tạm nguyên bản được dựng từ các sprite hình học trong P1Art: đầu, thân, mắt, tai/lá, tay/chân, đuôi, nấm/hoa/gai/sừng/tán cây. Các bộ phận chuyển động riêng và mắt chớp; có idle, attack, special, hit, evolve, victory, defeat. Evolve thêm vương miện và tăng silhouette. Cổ Thụ có chiêu Rừng Trói Buộc bằng rễ lá tại mục tiêu. Đây là rig tạm nguyên bản, chưa phải bộ art/sprite sheet chuyên nghiệp. Có Animator Controller P1PetAnimator với 7 clip/state dùng chung, curve điều khiển phase của rig, OnAttackImpact ở frame 40% và OnAnimationComplete ở cuối clip. Clip/Controller nằm trong Resources/P1Animations và Resources/P1PetAnimator.controller; ngoại hình/bộ phận riêng được dựng theo AnimationProfile rig ID của từng loài. Director chỉ trình diễn plan có sẵn; nếu event không tới thì có timeout để tránh treo trận.

Bảy hình đá thật được xuất PNG tại Resources/P1Stones: giọt sương, mầm đôi, lục giác, hoa bốn cánh, gai tinh thể, tim có khung, vương miện. Runtime dùng cùng mask, đổi màu theo hệ. Ảnh tham khảo trong Docs chỉ để đối chiếu bố cục; không dùng làm asset game.

## Kiểm thử và build

Tools > Pet Gem Battle > Run Core Tests / Run Gameplay Tests. Tests P1 nằm tại Tests/Editor/P1Tests.cs và Tests/PlayMode/P1FlowTests.cs. Kết quả batch: Logs/P1-EditMode.xml, Logs/P1-PlayMode.xml. Mô phỏng riêng P1: Logs/P1Simulation.md. Các trận mô phỏng dùng Pet/level khác nhau theo ải; không phải đo thời gian chơi người thật.

Build Windows qua Tools > Pet Gem Battle > Build Windows. Bản xuất ở Builds/Windows/PetArenaClassic.exe; giữ nguyên Data và DLL bên cạnh khi sao chép. Scene Home được xếp đầu build, scene Battle cũ vẫn có trong project.

## Phạm vi

Có offline PvE, sảnh, 7 boss Mộc, kho Pet/đá, Lv1–Lv7, thu phục, save, thẻ, cinematic combat. PvP, bang hội/bạn bè, cửa hàng/triệu hồi, nhiệm vụ/xếp hạng, đảo mới và ghép đá là Coming Soon bị khóa thật. Không thêm server, database, gacha hay tiền thật. Chưa có art chuyên nghiệp, replay editor đầy đủ hoặc hỗ trợ touch chuyên biệt.

Bản mốc trước P1: Backups/Before-P1.zip. Project chưa có Git nên dùng snapshot zip bảo toàn Assets, ProjectSettings, Packages thay cho branch trên repo không tồn tại.

Kết quả kiểm chứng: **89/89 EditMode + 24/24 PlayMode đạt**. Có test tích hợp thực từ Home → Battle → thắng → thu phục/thưởng → nạp lại save → Home. 210 mô phỏng cinematic P1 không kẹt, cùng bộ 1.000 trận hồi quy P0. Ảnh minh chứng: Docs/P1-Home.png, P1-Island.png, P1-Stones.png, P1-Combat.png.
