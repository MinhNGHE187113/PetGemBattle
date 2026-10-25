# Pet Arena Classic P2 — Hướng dẫn

Cập nhật trong project hiện tại theo cả hai tài liệu P2 ngày 25/10/2026. Không tạo project khác. Snapshot trước cập nhật: `Backups/Before-P2.zip`.

## Chạy game

- Bản Windows: mở `Builds/Windows/PetArenaClassic.exe`; giữ nguyên thư mục `PetArenaClassic_Data` và DLL bên cạnh.
- Unity 6000.3.27f1: mở project, chọn **Tools → Pet Gem Battle → Open P2 Home**, rồi Play. Scene chính vẫn là `Assets/Scenes/P1Home.unity` để giữ liên kết P1.
- Build lại: **Tools → Pet Gem Battle → Build Windows**. Setup chạy trước build, kiểm tra catalog, sprite, prefab và rules.

## Luồng chơi

Sảnh → **KHIÊU CHIẾN** → năm đảo → **Mộc Tinh** → bảy ải → xem boss → bắt đầu → kết quả/thưởng → về đảo/sảnh. Bốn đảo Hỏa/Kim/Thủy/Thổ có tranh, Pet và trang giới thiệu riêng; nút boss khóa Coming Soon.

Sảnh có Kho Pet, Kho đồ, Nâng cấp, Sách Pet, Đấu thử, Thẻ phép, Cài đặt. Đấu thử không nhận thưởng. PvP, cửa hàng, bang hội, nhiệm vụ, xếp hạng, triệu hồi chưa mở.

## Demo và dữ liệu

`Resources/GameModeConfig.asset`: DemoFullCollection=true, DemoTools=true, DemoStoneAmount=2. Demo mở **12 Pet Lv1** (11 thiết kế yêu cầu + Kim Miêu P1), chỉ bổ sung Pet còn thiếu; không nhân bản mỗi lần vào sảnh. Cấp, xuất chiến và tiến trình cũ được giữ.

Vào **Cài đặt → Cấp đá thử nghiệm → Xác nhận** để nhận 2 viên mỗi loại trong 5×7 loại đá, một lần/save. Không tự cộng mỗi lần vào game. Reset có xác nhận riêng và trở về Lv1. Đá Lv1 chỉ sưu tầm; Lv1→2 dùng một đá đúng hệ Lv2, tương tự đến Lv7.

JSON v2 lưu tại `%USERPROFILE%/AppData/LocalLow/DefaultCompany/PetGemBattle/p1-save.json`, có file dự phòng và giao dịch nâng cấp/thưởng chống lặp. Tên file P1 được giữ để migration. Muốn thử chế độ thu phục bình thường: tắt DemoFullCollection trong config, dùng save mới hoặc reset; config không tự xóa những Pet đã sở hữu trong save cũ.

## Tiến hóa

Mầm Miu giữ hình gốc. Lá Lùn/Dây Dẻo/Linh Hươu đổi hình Lv4; Nấm Nấp/Hoa Gai Lv7. Cổ Thụ có hai hình Lv4 và Lv7. Xem trước trong chi tiết/nâng cấp không thay đổi dữ liệu.

Chỉ số P1 theo cấp được làm tròn trước, sau đó nhân tích bonus các form đã mở và làm tròn một lần. Đọc save/mở màn hình không cộng lại bonus. Ví dụ Lá Lùn Lv4 = HP379/ATK35/DEF9/SPD25; Cổ Thụ Lv7 = HP887/ATK78/DEF24/SPD43.

## Chiến đấu P2

- Kiếm: `ATK × (2.30 + 0.40 × (số kiếm−3)) × combo`, trừ DEF×0.50 rồi làm tròn PreDamage. Sau đó nhân special/crit/hệ/Sudden Death/boss scale và làm tròn cuối.
- Tuyệt kỹ tự động trên từng nhóm kiếm cần **60 Nộ + 30 Mana**, trừ đúng cả hai; thiếu một điều kiện thì đánh thường, không tiêu chi phí tuyệt kỹ.
- Thu hết cascade → chuẩn bị model/RNG một lần → ẩn bàn → tài nguyên/hút/kiếm → cập nhật HP ở impact → hiện bàn nếu trận còn tiếp. 2× chỉ tăng tốc trình diễn, đồng hồ chọn nước theo thời gian thực.
- Boss HP: 440/530/630/760/900/1070/1270. Scale sát thương: .52/.54/.56/.58/.60/.62/.65; cap đòn thường 24%, special 30% MaxHP người chơi.
- Boss sống và HP≤30% sau đòn: thức tỉnh một lần, ATK×1.18, DEF×1.08, giáp8% MaxHP, không hồi HP/không thêm lượt. Đòn kết liễu bỏ qua thức tỉnh.
- Thẻ 80 Mana là buff tạm trong trận, riêng với form theo cấp và boss thức tỉnh.

## Asset và mã nguồn

`Resources/P2Art`: 26 PNG mới, gồm 19 sheet Pet/form, 5 đảo, 1 sảnh và 1 sheet 7 đá. `P2ArtLibrary.asset` chứa sprite đã tách và contour mesh lưu riêng; `P2SilhouetteMesh` dùng contour khi vẽ UI, loại mảnh ảnh lân cận mà không sửa PNG; `Resources/P2Pets` chứa prefab tái sử dụng. Pet có head/torso/accessory độc lập, Animator và event impact dùng lại hệ P1.

Đá có bảy silhouette vẽ riêng; năm material đổi lõi và khắc văn theo hệ, giữ màu mount vàng và shading. `P2StoneCore.shader` chỉ tác động vùng tinh thể xanh.

Nguồn thiết kế/prompt: `Docs/ART-PROMPTS-P2.md`. Báo cáo nghiệm thu: `Docs/P2-Acceptance.md`. Mô phỏng cân bằng: `Logs/P2BalanceSimulation.md` (700 trận, thời gian là ước lượng, không phải người chơi thật).

Ảnh kiểm tra `Docs/P2-*.png` được render từ UI thật bằng `Editor/P2VisualQA.cs` ở 1600×900, dùng save QA riêng trong Logs. Ảnh nâng cấp/trận dùng Cổ Thụ Lv7 để kiểm tra form; save người chơi vẫn Lv1 mặc định. Build Windows đã mở trực tiếp để kiểm tra sảnh; các ca queue, boss và điều hướng được chạy trong PlayMode.
