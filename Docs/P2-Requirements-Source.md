# PET ARENA CLASSIC P2 YÊU CẦU CẬP NHẬT GIAO DIỆN PET VÀ CHIẾN ĐẤU

**Ngày đặc tả:** 25/10/2026  
**Dành cho:** Codex triển khai trực tiếp trong project Unity 2D C# hiện tại  
**Tài liệu kế thừa:** `Pet_Arena_Classic_Full_Design_Specification.docx`, `Pet_Arena_P1_Codex_Requirements.md`. Nếu quy tắc P2 dưới đây khác P1, **P2 là bản mới ưu tiên áp dụng**.  
**Ảnh tham chiếu do người dùng cung cấp:** `image(20261025-131538).png` là ví dụ độ dễ thương của Pet và đường đi vượt ải; `image(20261025-132144).png` là ví dụ mức độ chi tiết của dạng mạnh hơn. Hai ảnh chỉ dùng để hiểu kỳ vọng chất lượng, tuyệt đối không sao chép nét nhận diện, nhân vật, tư thế đặc trưng hoặc hiệu ứng độc quyền.

## 1 Mục tiêu bản cập nhật P2

Hãy **nâng cấp bản game hiện tại** từ nguyên mẫu Match-3 thành một game 2D fantasy có sảnh chính, luồng điều hướng đầy đủ, màn Khiêu chiến năm đảo, kho Pet đầy đủ cho thử nghiệm offline, hệ thống nâng cấp và tiến hóa có thay đổi ngoại hình, chỉ số boss tăng dần hợp lý, tuyệt kỹ dùng **cả Nộ lẫn Mana** và animation giàu chuyển động.

Yêu cầu Codex **thực sự tạo và kết nối giao diện, Scene, Prefab, Animator, sprite/icon, dữ liệu, công thức và logic**. Không dừng ở mô tả, mockup, ảnh chụp hoặc button chỉ có tên mà không hoạt động. Kiểm tra code, các Scene và asset đang có trước khi chỉnh sửa; tái sử dụng hệ thống Match-3, thẻ, lưu dữ liệu và chiến đấu đã hoạt động. Không tự ý tạo project thứ hai.

Mục tiêu nghiệm thu quan trọng nhất: từ lúc vào game người chơi thấy một **sảnh đẹp, có Pet sống động**. Người chơi bấm `Khiêu chiến` mới thấy các đảo; bấm `Mộc Tinh` mới thấy 7 ải; bấm một ải mới vào trận. Trong trận có đủ cảnh nhặt hết ngọc, ẩn bàn, Pet tung chiêu và boss tự biến hình khi xuống dưới 30% HP.

## 2 Phong cách mỹ thuật và quy tắc bản quyền

### 2.1 Định hướng đồ họa thống nhất

- Chọn art direction **2D fantasy creature collector** với nhân vật chibi cao cấp: phần đầu khá lớn, biểu cảm rõ, cơ thể nhỏ vừa phải, dáng chiến đấu năng động; phối hợp vật liệu lá, gỗ, đá, kim loại, vảy, pha lê. Pet phải có nhiều chi tiết hơn bản hiện tại, không chỉ là khối tròn, hình vuông hoặc vài miếng hình học lắp với nhau.
- Bậc C và B ưu tiên dễ thương, gần gũi, silhouette rõ và có một nét nhận diện độc đáo. Bậc A bắt đầu có giáp/sừng/khí chất thủ lĩnh. Bậc S, SS, SSS ngày càng hoành tráng, nhiều lớp trang phục/giáp, aura và hoạt cảnh riêng; vẫn giữ tính thống nhất thế giới.
- Dùng bảng màu mềm, dễ nhìn: nền tối vừa phải tại chiến đấu để Pet nổi bật; sảnh dùng màu ngọc lam, xanh rêu, gỗ ấm, vàng nhạt; điểm nhấn có màu theo đảo. Tránh mọi khung, icon và nền cùng phát sáng làm rối mắt.
- Ánh sáng nhất quán từ một hướng, có độ đậm nhạt, bóng tiếp đất và đường viền mềm. Pet phải đọc được hình ở kích thước nhỏ trong kho và đủ chi tiết khi phóng lớn trong màn chiến đấu.
- **Không sao chép Pokiwar hoặc nhân vật trong ảnh**; không trace sprite, không dùng lại logo, tên nhân vật, trang phục, mảng màu hoặc silhouette đặc trưng từ hình tham khảo. Tạo tên, cấu trúc cơ thể, hình dáng mắt/sừng/giáp/vũ khí và pose riêng. Không sử dụng tài nguyên không rõ quyền khai thác.

### 2.2 Màu chủ đạo theo Ngũ Hành

| Đảo và hệ | Bảng màu gợi ý | Chi tiết môi trường và Pet |
| --- | --- | --- |
| Mộc Tinh | Xanh lá rừng, xanh ngọc, vàng lá | Rễ, dây leo, lá, nấm, hoa, gỗ sống |
| Hỏa Tinh | Đỏ son, cam rực, vàng hổ phách | Đá núi lửa, than hồng, vảy lửa, hỏa văn |
| Kim Tinh | Trắng bạc, vàng kim, xanh thép | Tinh thể kim loại, giáp nhỏ, lưỡi gươm, chim máy |
| Thủy Tinh | Xanh biển, xanh lam, xanh ngọc lạnh | Bọt nước, san hô, giọt nước, cá/rồng nước |
| Thổ Tinh | Nâu ấm, vàng đất, xanh rêu nhạt | Cát, đá, mai rùa, khoáng vật, tượng cổ |

Không tô Pet bằng một màu duy nhất: màu hệ chiếm khoảng 50–70%, màu phụ và mắt/giáp tạo tương phản. Các Pet cùng đảo phải khác silhouette, chất liệu, biểu cảm và cách di chuyển. Không tạo bốn Pet giống nhau rồi chỉ thay màu.

### 2.3 Bộ asset Codex phải bàn giao

- Sảnh chính: 1 background nhiều lớp có parallax nhẹ, nền đặt Pet, các icon chức năng, nút và trạng thái hover/chọn/khóa.
- Màn Khiêu chiến: 5 card đảo riêng biệt; mỗi đảo có background màu/chất liệu riêng, tên, mô tả, biểu tượng hệ và trạng thái.
- Đảo Mộc Tinh: bản đồ đường đi 7 ải với mỗi điểm có icon/ảnh bóng boss; màn boss info và màn kết quả thưởng.
- Ít nhất **11 thiết kế Pet gốc**: 7 Pet Mộc của mục 5 và 1 Pet đại diện mới cho mỗi hệ còn lại (Hỏa, Kim, Thủy, Thổ). Bốn Pet hệ còn lại được dùng để kiểm tra hình ảnh/kho và giới thiệu đảo; boss/ải khác chưa cần lập trình đầy đủ ở P2.
- Tạo các hình dạng tiến hóa của những Pet được chỉ định ở mục 6; không phải Pet nào cũng có tiến hóa ngoại hình.
- Đá nâng cấp: 7 silhouette cấp đá khác nhau; mỗi hệ dùng lõi/hoa văn/màu riêng; icon chất lượng ở kích thước nhỏ và ảnh lớn trong màn nâng cấp.
- Bộ VFX: thu ngọc theo màu; hồi HP, Mana, Nộ, giáp; số sát thương thường/chí mạng/khắc hệ; đòn thường, tuyệt kỹ, boss biến hình, Pet lên cấp, nhận thưởng.

Mỗi sprite/prefab phải là **asset sử dụng được trong Unity**, không chỉ ảnh phối cảnh. Ưu tiên PNG nền trong suốt, layer tách bộ phận để rig 2D hoặc sprite sheet chuẩn; gợi ý xuất concept 1024×1024 và sprite game 256–512 px/nhân vật. Nếu môi trường Codex không có công cụ tạo tranh chất lượng, hãy thông báo rõ asset nào chưa thể tạo, tạo bộ placeholder có phong cách nhất quán để kiểm thử nhưng **không được tuyên bố art hoàn thiện** khi Pet vẫn là hình học thô. Không lấy ảnh tham khảo trực tiếp làm sprite game.

## 3 Giao diện sảnh và hệ thống màn hình

### 3.1 Sảnh chính là điểm bắt đầu duy nhất

Khi bấm Play và sau màn loading ngắn, vào **Home Lobby**. Không tự động nhảy thẳng vào bàn xếp ngọc. Sảnh có bố cục phù hợp màn hình landscape 16:9, 16:10, scale UI an toàn:

- Trên cùng: logo game riêng, avatar/tên người chơi thử nghiệm, Pet đang xuất chiến, tổng đá hoặc tài nguyên, cài đặt âm thanh.
- Chính giữa: Pet đang chọn **hiển thị cỡ lớn và có animation Idle**, đứng trên bệ/đất có bóng và VFX hệ rất nhẹ; thông tin bên cạnh gồm tên, cấp, bậc, hệ, chỉ số ngắn.
- Nút nổi bật nhất: `KHIÊU CHIẾN`, dẫn đến màn **Chọn Đảo**. Không nhảy trực tiếp vào trận, không tải thẳng ải Mộc.
- Nút chức năng thứ cấp: `Kho Pet`, `Kho đồ`, `Nâng cấp`, `Sách Pet`, `Đấu thử` và `Thẻ phép` (hướng dẫn).
- Các ô `PvP`, `Cửa hàng`, `Bang hội`, `Nhiệm vụ`, `Xếp hạng`, `Triệu hồi` có nhãn `Coming Soon`, khóa click hoặc chỉ hiển thị tooltip. Không tạo màn trắng hay popup rỗng.
- Khi đổi Pet xuất chiến trong kho, sảnh tự cập nhật portrait, mô hình, biểu tượng hệ và chỉ số. Nút chức năng có âm thanh nhấp và chuyển cảnh ngắn, không lạm dụng nhấp nháy.

### 3.2 Danh sách màn hình yêu cầu

| Màn hình | Yêu cầu giao diện | Trạng thái P2 |
| --- | --- | --- |
| Loading / Home Lobby | Logo và sảnh fantasy, Pet ở giữa, các chức năng điều hướng | Hoạt động |
| Challenge Island Select | 5 card Mộc/Hỏa/Kim/Thủy/Thổ, hiệu ứng hover, tương khắc, trạng thái | Hoạt động |
| Mộc Tinh Stage Map | 7 ải trên đường đi; chi tiết boss; chọn ải; tiến trình, thưởng | Chơi được |
| Hỏa/Kim/Thủy/Thổ Stage Preview | Nền, sơ đồ ải mẫu và Pet đại diện riêng | Giao diện đẹp; vào boss ghi Coming Soon |
| Boss Preview / Team Select | Pet địch vs Pet xuất chiến, HP, ATK, DEF, SPD, hệ khắc | Hoạt động với Mộc |
| Match 3 Battle | Bàn 8×8, HUD, thẻ, animation chuyển sang Combat | Hoạt động với Mộc và Đấu thử |
| Combat Presentation | Ẩn bàn, Pet lớn, VFX tấn công, hồi máu/năng lượng, boss hóa hình | Hoạt động |
| Victory / Defeat | Thưởng đá, tiến trình, nút tiếp tục/đánh lại/về đảo | Hoạt động |
| Pet Storage | Lưới Pet, phân bậc và lọc hệ, chọn xuất chiến | Hoạt động |
| Pet Detail / Evolution View | Xem Pet thường/tiến hóa, stat và ngưỡng Lv | Hoạt động |
| Item Storage | Đá theo hệ và 7 cấp, số lượng, tooltip nguồn rơi | Hoạt động |
| Upgrade Pet | Pet 3D-like 2D portrait, before/after, đá cần, xác nhận | Hoạt động |
| Card Guide / Settings / Bestiary | Xem thẻ, âm thanh/VFX, Sách Pet; Back/Home | Hoạt động mức cơ bản |
| PvP / Shop / Guild / Gacha | Nhãn Coming Soon | Khóa thật |

Mọi màn có nút Back, Home, âm thanh/transition nhất quán; không bấm Back bị mất vật phẩm. Tách `ScreenManager`, UI prefab và navigation stack; không để Scene phụ giữ bản sao dữ liệu Pet riêng.

### 3.3 Luồng Khiêu chiến nhiều đảo

**Home → Khiêu chiến → Chọn đảo → Mộc Tinh → Bản đồ 7 ải → Chọn boss → Trận đấu → Kết quả → Quay lại đảo hoặc sảnh.**

Các card đảo hiển thị nhãn cấp độ đề xuất, chủng hệ, khắc hệ và artwork đại diện. Ở bản P2, **chỉ Đảo Mộc Tinh có 7 boss chơi được**, bốn đảo còn lại có giao diện giới thiệu đầy đủ và nút `Coming Soon` cho boss. Khi chọn đảo chưa có nội dung, vẫn mở được trang preview đẹp nhưng không cho đánh trận giả/đánh Pet Mộc đội lốt hệ khác. Cấu trúc dữ liệu phải cho phép thêm 7 ải mỗi đảo về sau mà không viết lại Navigation.

## 4 Kho Pet đầy đủ trong chế độ thử nghiệm offline

Hiện chưa có database. Để tập trung làm hình ảnh và thử nghiệm, **mặc định ở bản phát triển P2 người chơi sở hữu tất cả Pet đã được định nghĩa trong game**, ít nhất 11 Pet của đợt này, ở Lv1. Hiển thị đủ trong kho ngay lần chạy đầu; người chơi được chọn bất kỳ Pet nào để xuất chiến. Không giả lập danh sách bằng UI tĩnh không có dữ liệu.

- Có `GameModeConfig.DemoFullCollection = true` cho bản thử nghiệm. Lần đầu vào game, seed một `PetInstance` duy nhất cho mỗi `PetDefinition`, không lặp sau restart; save vào JSON `Application.persistentDataPath`. Có cách reset save demo trong Settings với xác nhận.
- Trong demo, đánh bại boss lần đầu vẫn ghi nhận hoàn thành ải, phát animation thu phục và nhận thưởng, nhưng vì Pet **đã có sẵn trong kho**, popup ghi `Đã chinh phục [Tên]` và không thêm Pet trùng. Sau này khi chuyển `DemoFullCollection = false`, áp dụng lại luật P1: thắng boss lần đầu mới mở sở hữu/thu phục 100%.
- Không dùng database, Redis hoặc yêu cầu đăng nhập cho P2. Thiết kế model ổn định để sau này chuyển từ JSON sang API mà không đụng công thức chiến đấu.
- Pet có UI `Owned`, `Equipped`, `Level`, `BaseForm/CurrentForm`, `Element`, `Rarity`, chỉ số real-time. Kho hiển thị lọc theo hệ, bậc, cấp, đã tiến hóa, sắp xếp theo ATK/HP/SPD.
- Nút `Xuất chiến` đổi Pet ở sảnh và đội hình trận kế; không thay Pet giữa lượt combat.

## 5 Danh sách Pet cần vẽ mới

### 5.1 Bảy Pet đảo Mộc Tinh

Giữ thông tin game logic P1 gồm tên, hệ Mộc và bậc; **vẽ lại toàn bộ** theo phong cách chất lượng cao hơn, có silhouette độc nhất. Bậc C/B vẫn nhỏ nhắn, đầu to, mắt rõ; bậc A mạnh mẽ hơn nhưng không biến thành boss SS/SSS.

| Ải | Tên và bậc | Thiết kế tạo hình gốc cần có | Chi tiết nhận diện |
| --- | --- | --- | --- |
| 1 | Mầm Miu C | Linh miêu mầm cây xanh lá, thân tròn, đuôi lá cuộn | Hai lá lệch trên tai, mắt hổ phách |
| 2 | Lá Lùn C | Ếch lá với áo choàng lá và chân ngắn | Giọt sương lơ lửng và đôi má chấm nắng |
| 3 | Nấm Nấp C | Nấm con tinh nghịch, mũ bè, găng rêu | Túi bào tử sau lưng, mắt tinh quái |
| 4 | Dây Dẻo B | Thằn lằn dây leo nhỏ, giáp cành và đuôi rễ | Rễ quấn chân, mảng lá tạo găng tay |
| 5 | Hoa Gai B | Nhím hoa rừng, lưng có gai cánh hoa bán trong suốt | Nụ hoa rực giữa trán, gai chuyển động |
| 6 | Linh Hươu B | Hươu linh trẻ, áo cổ lá, móng như gỗ quý | Sừng mầm và hạt sáng xanh dọc cổ |
| 7 | Cổ Thụ Vệ Thần A | Thần cây chibi cứng cáp với giáp rễ nhiều lớp | Mắt vàng ngọc, sừng tán cây, quyền trượng rễ nguyên bản |

Mỗi Pet cần một concept thiết kế nhìn chính diện/3/4, sprite chiến đấu có cùng tỉ lệ, portrait cho kho, ảnh icon nhỏ và ít nhất 7 trạng thái animation cơ bản (Idle, Attack, SpecialAttack, Hit, Evolution, Victory, Defeat). Mỗi con có **một kỹ năng đặc biệt nguyên tố Mộc riêng về hình ảnh**, không chỉ đổi màu cùng một chùm tia.

### 5.2 Pet đại diện cho bốn hệ còn lại

| Hệ | Pet đại diện mới gợi ý | Phẩm cấp | Hình ảnh chủ đạo |
| --- | --- | --- | --- |
| Hỏa | Hỏa Lân Nhí | B | Sư tử nhỏ có sừng tro, bờm lửa cam, giáp than |
| Kim | Thiết Điểu | B | Chim ưng tí hon, cánh kim loại lắp ghép như lông vũ |
| Thủy | Thủy Ly | B | Rái cá linh nước đeo vòng san hô, vây pha lê xanh |
| Thổ | Sa Quy | B | Rùa sa mạc nhỏ có mai tinh thạch cát, sừng đá mịn |

Bốn Pet này phải thực sự xuất hiện trong kho và có bản sprite/Idle; để dùng Đấu thử và kiểm tra khắc hệ, nên có cả Normal Attack, Special Attack, Hit. Có thể ưu tiên 7 Pet Mộc đủ animation trước, sau đó hoàn thiện 4 Pet preview. Tất cả phải là nhân vật gốc.

## 6 Tiến hóa thay đổi ngoại hình khi đạt cấp

### 6.1 Tiến hóa vĩnh viễn theo cấp Pet

**Không phải Pet nào cũng bắt buộc có dạng tiến hóa.** Mỗi loài có `EvolutionMilestones` từ 0 đến 2 mốc là Lv4 và/hoặc Lv7. Khi nâng cấp đến mốc, sprite/portrait/animation và chỉ số phải đổi theo form; không chỉ thay màu hoặc số lượng HP. Đây là **tiến hóa vĩnh viễn**, khác với thẻ Tiến hóa tạm thời trong trận.

| Pet | Mốc đổi dạng | Mô tả dạng mới |
| --- | --- | --- |
| Mầm Miu C | Không có | Vẫn đáng yêu; mạnh lên bằng chỉ số Lv nhưng giữ hình gốc |
| Lá Lùn C | Lv4 | Ếch Lá Hiệp Sĩ, áo choàng lá xếp lớp và vòng sương |
| Nấm Nấp C | Lv7 | Nấm Ảo Ảnh, mũ pha lê rêu và quầng bào tử nhỏ |
| Dây Dẻo B | Lv4 | Long Đằng Non, chân giáp dây leo và đuôi gai |
| Hoa Gai B | Lv7 | Hoa Giáp Vệ, giáp cánh hoa và gai nở sáng |
| Linh Hươu B | Lv4 | Hươu Linh Thủ Hộ, cặp sừng cành phát sáng |
| Cổ Thụ Vệ Thần A | Lv4 và Lv7 | Lv4 thêm giáp thân cổ thụ; Lv7 tán lá vương miện và quyền trượng rễ |
| Bốn Pet preview | Chưa bắt buộc | Cấu hình dữ liệu để mở rộng, không tự tạo form từ ảnh của Pet khác |

Mỗi form được khai báo bằng `EvolutionFormDefinition` với `MinLevel`, `Portrait`, `BattleSpriteOrRig`, `AnimatorOverride`, `StatBonus`, `VFXProfile`, `DisplayName`. Lên cấp tự chuyển form đúng một lần, lưu và khôi phục sau restart; xuống mức menu khác không làm mất form. Sảnh, kho, chi tiết Pet và combat **đều dùng form theo cấp hiện tại**.

### 6.2 Công thức sức mạnh các dạng

Vẫn tính stat Lv1–Lv7 theo P1 trước; sau đó nhân bonus form và làm tròn một lần:

- Form mở ở Lv4 hoặc Lv7 lần đầu: `MaxHP ×1.12`, `ATK ×1.18`, `DEF ×1.10`, `SPD +2` so với stat cấp tương ứng.
- Chỉ Cổ Thụ Vệ Thần có **form thứ hai** tại Lv7: nhân tiếp `MaxHP ×1.10`, `ATK ×1.12`, `DEF ×1.08`, `SPD +2` trên chỉ số đã áp dụng form Lv4. Không nhân lặp bonus form mỗi lần mở sảnh.
- Các Pet không có form **vẫn tăng chỉ số theo level** của P1; không được yếu đến mức không dùng được. Độ hiếm **C/B/A/S/SS/SSS không thay đổi** vì tiến hóa.
- Animation tiến hóa vĩnh viễn chạy **khi upgrade đạt mốc**; popup so sánh trước/sau, hiển thị tên form mới. Nếu mở Pet Lv7 đã lưu, hiển thị đúng form nhưng không phát lại animation unlock liên tục.

### 6.3 Thẻ tiến hóa tạm thời trong trận vẫn tồn tại

Thẻ P1 tốn **80 Mana, tối đa một lần/trận**, hoạt động ngay cả khi Pet không có tiến hóa vĩnh viễn. Nó bật trạng thái `BattleAwakened` tạm thời, thêm aura/hoa văn, thay pose hoặc rig overlay, giữ mức tăng MaxHP ×1.40, ATK ×1.35, DEF ×1.20 như P1; chỉ có hiệu lực trong trận, hết trận trở về đúng form cấp của Pet. Không cộng nhầm stat Lv/form permanent hai lần. Khi Pet đã Lv7, thẻ vẫn dùng được, nhưng không tăng thêm một mốc vĩnh viễn.

## 7 Điều chỉnh boss Mộc Tinh và cân bằng HP sát thương

### 7.1 Phân biệt Pet sở hữu và boss gặp trong ải

Giữ **BaseStats gốc khi thu phục** từ P1, nhưng khi lên sàn boss phải dùng `BossEncounterProfile` riêng. Người chơi muốn boss có HP nhiều hơn và tăng dần từng ải; **không sao chép HP boss khổng lồ thành HP Pet thu phục**.

| Ải | Boss Mộc | HP boss khi chiến đấu | ATK boss | DEF boss | Tỉ lệ damage boss gợi ý |
| --- | --- | ---: | ---: | ---: | ---: |
| 1 | Mầm Miu C | 440 | 22 | 6 | ×0.52 |
| 2 | Lá Lùn C | 530 | 24 | 7 | ×0.54 |
| 3 | Nấm Nấp C | 630 | 26 | 8 | ×0.56 |
| 4 | Dây Dẻo B | 760 | 28 | 9 | ×0.58 |
| 5 | Hoa Gai B | 900 | 31 | 10 | ×0.60 |
| 6 | Linh Hươu B | 1070 | 32 | 11 | ×0.62 |
| 7 | Cổ Thụ Vệ Thần A | 1270 | 38 | 14 | ×0.65 |

HP boss tăng **đều qua bảy ải**, boss 7 vẫn **hạng A**, không đột ngột xuất hiện S. Các hệ số là mặc định cân bằng P2, phải khai báo trong `BossEncounterProfile` để chỉnh dễ dàng. Đề xuất cấp Pet người chơi cho ải 1→7 lần lượt `Lv1, Lv1, Lv2, Lv3, Lv4, Lv5, Lv6+`; hiển thị khuyến nghị ở màn boss, không khóa cứng người chơi.

### 7.2 Sát thương người chơi đủ lớn và boss đánh vừa phải

Vì HP boss đã cao hơn P1, tăng hệ số **đòn kiếm vàng** để tránh một trận kéo dài quá lâu. Trong PvE P2 dùng công thức chính:

1. `AttackPower = CurrentATK × [2.30 + 0.40 × (SwordCount − 3)] × ComboMultiplier`.
2. `PreDamage = max(1, round(AttackPower − 0.50 × TargetDEF))`.
3. Nếu đủ tài nguyên cho tuyệt kỹ, nhân `SpecialMultiplier = 1.75`; nếu không, dùng đòn thường `×1.00`.
4. Nếu chí mạng, nhân `CritMultiplier = 1.70` theo xác suất hiện hành P1; sau đó nhân hệ số khắc hệ `1.25 / 0.85 / 1.00`, cộng hệ số chống kéo dài của trận, cuối cùng mới làm tròn và trừ giáp trước HP.
5. Với **boss đánh người chơi** trong đảo Mộc: tính theo công thức trên rồi nhân `BossOutgoingScale` ở bảng; hạn chế sát thương mỗi **đòn thường** không vượt `24% MaxHP` của người chơi, **tuyệt kỹ** không vượt `30% MaxHP` trong ải đầu. Tuyệt đối không có boss hạ Pet đủ máu bằng một đòn ở đầu đảo khi cấp phù hợp.

Ví dụ Pet B Lv1 ATK29 đánh boss DEF9 bằng **3 kiếm**, không crit, trung tính: `29×2.30−0.5×9 = 62.2`, thành **62 damage**, mạnh hơn mức cũ. Một boss Lv1 ATK22 đánh Pet HP260, DEF6: sát thương cơ sở `22×2.30−3 = 47.6`; sau hệ số boss ải1 `×0.52` ≈ **25 damage**, khoảng 9.6% HP nếu trung tính.

HP boss tăng dần, nhưng người chơi có **Lv, form tiến hóa, kiếm 4–5 viên, combo, chí mạng, tuyệt kỹ, tương khắc hệ** để tăng sát thương. Mục tiêu tuning: ải 1–3 khoảng 5–10 lượt có kiếm của người chơi; ải 7 khoảng 10–16 lượt có kiếm khi Pet đạt cấp khuyến nghị. Chạy mô phỏng AI-AI trước/sau tuning, ghi thời lượng trung vị và tỷ lệ thắng, tránh ép người chơi đánh 30 vòng mới xử xong.

### 7.3 Boss tự biến hình khi còn dưới 30 phần trăm HP

**Tất cả boss trong Mộc Tinh đều có trạng thái `BossAwakened` dùng một lần/trận**, độc lập với form vĩnh viễn của Pet sở hữu. Sau khi kết thúc việc nhận sát thương của một đòn, nếu `0 < CurrentHP ≤ 0.30×MaxHP` và chưa hóa hình, tạm giữ state chiến đấu, kích hoạt animation biến đổi 0.9–1.5 giây, rồi tiếp tục đúng lượt. Nếu bị hạ từ trên 30% xuống 0 HP trong cùng đòn thì **chết luôn, không hóa hình**.

- Form boss thức tỉnh có thay đổi ngoại hình rõ: mắt sáng hơn, lá/gai/giáp mở rộng, aura theo cấp. Đây là **dạng boss PvE riêng**, không tự ghi vào kho Pet và không đổi rarity.
- Buff mặc định khi thức tỉnh: `ATK ×1.18`, `DEF ×1.08`, nhận **giáp bằng 8% MaxHP boss** một lần (không hồi HP); giới hạn giáp chung theo ruleset nếu cần. Không được cộng buff lặp lại khi HP tiếp tục giảm.
- Boss không được hành động thêm một lượt ngay lúc biến hình; sau animation mới xét lượt thông thường. Thanh HP vẫn giữ số HP còn lại, ngưỡng 30% không tự hồi về đầy.
- Dạng thức tỉnh khiến tuyệt kỹ có VFX mới, nhưng **vẫn phải đủ cả Mana và Nộ mới được sử dụng tuyệt kỹ**. Auto-evolve dưới 30% không tốn Mana/Nộ vì là cơ chế boss riêng.
- HUD hiện cảnh báo `Boss thức tỉnh` và hiệu ứng đếm nhịp ngắn; người chơi vẫn nhìn thấy HP và giáp, không bị giật camera quá mạnh.

## 8 Tuyệt kỹ phải cần cả Mana và Nộ

**Đây là thay đổi bắt buộc so với P1.** Trước đây chỉ cần Nộ 60 là dùng tuyệt kỹ; giờ để tránh Mana chỉ dành cho thẻ tiến hóa, mỗi đòn kiếm khi xét tuyệt kỹ **phải có đồng thời**:

| Điều kiện | Mốc yêu cầu | Khi kích hoạt |
| --- | ---: | --- |
| Nộ | Ít nhất 60 trên tối đa 100 | Trừ đúng 60 Nộ |
| Mana | Ít nhất 30 trên tối đa 100 | Trừ đúng 30 Mana |
| Có nhóm kiếm vàng hợp lệ | Ít nhất 3 viên trong một match | Phát animation SpecialAttack và sát thương ×1.75 |

Nếu chỉ đủ Nộ hoặc chỉ đủ Mana: **đánh thường**, không trừ cả hai tài nguyên; giữ tài nguyên để dùng lượt sau. Đòn thường và chí mạng vẫn hoạt động như cũ. Ví dụ Nộ=72, Mana=29 → đánh thường, Nộ/Mana không bị trừ vì dùng chiêu; Nộ=72, Mana=35 → tuyệt kỹ và còn Nộ=12, Mana=5.

Thứ tự xử lý một lượt:

1. Match-3 thu **toàn bộ** xanh lá, xanh nước biển, đỏ, tím, xám và vàng qua các combo/cascade. Không đánh ngay trong lúc ngọc còn rơi.
2. Cộng Mana từ xanh nước biển, cộng Nộ từ đỏ và từ kiếm, giải quyết các buff/hồi phục/đánh cắp theo snapshot sự kiện.
3. Ẩn bàn cờ; phát animation hồi HP (xanh lá), Mana (xanh nước biển), Nộ (đỏ), tạo giáp (tím), đánh cắp (xám) **có số liệu hiện ở HUD**.
4. Lần lượt với từng nhóm kiếm vàng: kiểm tra **Nộ≥60 và Mana≥30** ở thời điểm đó; nếu đủ cả hai trừ cả hai, phát SpecialAttack; nếu không phát NormalAttack. Chí mạng và hệ khắc cũng tính mỗi đòn. Trừ HP tại đúng frame trúng đòn.
5. Nếu boss qua ngưỡng 30% sau đòn mà còn sống, chen `BossAwakened` vào hàng đợi trình diễn; nếu thắng/thua thì chạy kết quả. Chỉ hiện bàn lại khi toàn bộ animation đã kết thúc và trận chưa kết thúc.

Nguồn Mana tiếp tục là ngọc xanh nước biển và thẻ `+20 Mana` (tối đa 2 lần/trận); Nộ có ngọc đỏ, kiếm và thẻ `+20 Nộ`. Thẻ `BattleAwakened` tốn 80 Mana một lần/trận tạo quyết định chiến thuật **giữ Mana để dùng tuyệt kỹ nhiều lần hoặc dồn 80 Mana để thức tỉnh**. Phải hiển thị rõ `Đủ Mana / Thiếu Mana`, `Đủ Nộ / Thiếu Nộ` trên HUD và tooltip thẻ.

## 9 Animation toàn bộ màn chơi

### 9.1 Những động tác bắt buộc trên Pet

| Trạng thái | Chuyển động bắt buộc | Gợi ý |
| --- | --- | --- |
| Idle | Thở nhẹ, mắt chớp, tai/lá/sừng hoặc đuôi chuyển động riêng | Loop 8–12 FPS, không chỉ bob toàn sprite |
| Move / Prepare | Chùng gối, lấy đà hoặc tích năng lượng | 0.15–0.35 giây |
| NormalAttack | Lao tới/quật/gõ/vung, vệt sáng theo bộ phận, lùi về | 0.65–0.95 giây |
| SpecialAttack | Pose rõ, tích Mana/Nộ và phát hiệu ứng hệ, rung nhẹ khi impact | 1.1–1.7 giây |
| Hit | Đẩy lùi 5–15px, flash ngắn, biểu cảm đau | 0.2–0.4 giây |
| Heal HP | Hạt xanh lá bay quanh thân, `+HP` và thanh xanh tăng mượt | 0.4–0.7 giây |
| Gain Mana | Hạt xanh nước biển đi vào thanh Mana; số `+Mana` | 0.35–0.65 giây |
| Gain Rage | Sóng lửa đỏ nhỏ, `+Nộ`; thanh Nộ đổi màu | 0.35–0.65 giây |
| Gain Shield | Lục giác tím ôm Pet, xuất hiện trị số giáp | 0.4–0.7 giây |
| Gray Steal | Sợi sáng xám từ boss về Pet, nêu tài nguyên hút được | 0.45–0.8 giây |
| Permanent Evolution | Layer phát sáng, bộ phận giáp mọc/mở, sprite/rig đổi form | 1.3–2.0 giây |
| Boss Awakened | Vòng sức mạnh xoáy, lá/vảy/giáp mở lớn, aura bùng rồi dịu | 0.9–1.5 giây |
| Victory / Defeat | Pose thắng hân hoan và thua khuỵu người | 0.7–1.4 giây |

Không sử dụng một cú `transform.localScale` duy nhất thay cho tất cả animation. Pet phải cử động ít nhất hai bộ phận riêng (mắt/tai/đuôi/tay/vảy/lá...) và có anticipation → impact → recovery. Dùng sprite atlas/2D rig hoặc sprite sheet; Animator Controller/state tags và Animation Events để đồng bộ trừ máu tại impact. Có `2× Animation Speed` chỉ thay đổi thời lượng biểu diễn, không đổi kết quả tính toán.

### 9.2 Bàn ngọc và màn combat phải chuyển cảnh mượt

- Khi xếp ngọc, màn 8×8 ở giữa, Pet ở hai bên, HUD ở trên và thẻ phép dưới; animation ngọc hoán đổi/rơi/tan có easing và hạt cùng màu.
- Sau **toàn bộ** cascade, bàn fade/slide ra trong 0.2–0.35 giây; camera zoom/đổi layout để Pet ở trung tâm hai phía giống nhịp trận mẫu người dùng đã gửi. Trong pha Pet đánh **không hiển thị bàn ngọc che nhân vật**.
- Mọi `GemEffectEvent` được tính một lần ở model rồi xếp vào `CombatAnimationQueue`; không được generate random mới khi VFX chạy, không được gọi damage hai lần qua `OnAnimationEvent` và coroutine.
- Với ngọc xanh/hồi, Mana, Nộ, giáp hay hút máu: trình diễn **số tăng giảm nhìn thấy được** và cập nhật thanh HUD theo tween. Kiếm tấn công phát đòn vật lý/cận chiến hoặc phép phù hợp Pet; tuyệt kỹ phải phân biệt rõ về chuyển động và VFX.
- Nếu boss đạt ngưỡng thức tỉnh dưới 30%, animation thức tỉnh phải xen đúng sau đòn gây ngưỡng. Nếu đòn đó kết liễu boss, bỏ thức tỉnh và vào Victory.
- Sau tất cả sự kiện mà trận vẫn còn, đưa bàn đã refill trở lại 0.2–0.35 giây và mở lượt đối phương. Trong quá trình diễn, **khóa mọi nút/drag board** trừ nút tăng tốc.

## 10 Kho đồ và nâng cấp đá cần làm đẹp lại

Giữ 5 hệ × 7 cấp đá và luật nâng đúng hệ của P1. Vẽ lại **7 hình dạng đá khác nhau**, phát triển dần từ mầm thạch đến tinh thể huyền thoại, có chiều sâu và chất liệu. Cấp cao cần ấn tượng hơn, nhưng icon Lv1–Lv7 vẫn cùng họ thiết kế. Mỗi hệ dùng biến thể lõi/khắc văn riêng, không chỉ tô một mảng phẳng.

- Màn kho đồ là lưới icon chất lượng cao, group theo hệ, nhãn cấp và số lượng; hover/click hiện tên `Đá Mộc Lv4`, nguồn rơi, công dụng, hình phóng to.
- Màn nâng cấp chia hai cột: Pet động bên trái, vòng 7 mốc cấp hoặc progress dọc; bên phải là stat trước/sau với số tăng màu xanh, slot đá bắt buộc, số lượng đang có và nút `Nâng cấp`.
- Từ Lv1→Lv2 dùng 1 đá cùng hệ Lv2; cứ thế đến Lv6→Lv7 dùng đá cùng hệ Lv7. Đá Lv1 là vật phẩm nhập môn/bộ sưu tập, chưa tiêu vào nâng cấp ở P2. Không đủ đá thì khóa nút kèm lời giải thích.
- Nếu nâng đến mốc evolution, xem trước **form mới bên cạnh form cũ** và animation đổi dạng sau xác nhận. Chỉ trừ đá và tăng Lv **một lần** kể cả double-click; lưu dữ liệu ngay sau giao dịch.
- Mặc định demo full collection nhưng **không tự nâng Pet lên Lv7**. Có thể seed một ít đá để thử UI trong chế độ Editor/Debug; khi reset dữ liệu dùng lại seed rõ ràng, không cộng chồng đá mỗi lần vào sảnh.

## 11 Thiết kế dữ liệu kiến trúc Unity và ưu tiên code

Ưu tiên tái sử dụng kiến trúc đã có. Mẫu cấu trúc dữ liệu phải có ít nhất:

- `PetDefinition`: id, displayName, element, rarity, BaseStats, crit, sprite/profile, animation profile, evolution profile.
- `PetInstance`: definitionId, level, owned, equipped, unlockedMilestones; current form **derived from level** để tránh data bất nhất.
- `EvolutionFormDefinition`: threshold Level, texture/rig, battle animation, portrait, relative stat modifiers; riêng `BossAwakened` đặt trong `BossEncounterProfile`, không lưu như permanent form.
- `ChallengeIslandDefinition`: 5 đảo, theme colors/background, id, currentStatus, stageIds, element, previewPetId; `BossStageDefinition` và `BossEncounterProfile` chứa HP, ATK, DEF, outgoing scale, threshold, reward.
- `BattleBalanceConfig`: base attack 2.30, extra sword 0.40, Mana special cost30, Rage special cost60, crit/special/element multipliers, boss caps và chống stalemate.
- `GameModeConfig`: `DemoFullCollection`, seed stones/editor test, island availability; `PlayerSaveData` versioned JSON giữ pet, đá, stage progression, activePet.
- `UIScreenRouter`, `LobbyController`, `ChallengeIslandController`, `PetCollectionController`, `UpgradeController`, `EvolutionDisplayController`, `BattleTurnStateMachine`, `CombatAnimationDirector`, `BattleMath`, `RewardService`, `SaveGameService` tách trách nhiệm. Tên file có thể khác nếu project đã có tên tương đương.

Các biến thiết kế/giá trị P2 phải đặt trong ScriptableObject/serialized config để chỉnh cân bằng mà không sửa mã cứng. Hạn chế package không cần; đừng dùng Texture2D.LoadImage mỗi frame hay tạo mới GameObject liên tục gây giật. Có pooling VFX, preload sprite/atlas vừa phải, tự động reset animation state khi chuyển cảnh.

## 12 Kiểm thử bắt buộc và thứ tự triển khai

### 12.1 Danh sách nghiệm thu có thể kiểm tra

| Mã | Thử nghiệm | Kết quả đúng |
| --- | --- | --- |
| P2-01 | Khởi chạy Play từ Scene chính | Vào sảnh, Pet đang xuất chiến Idle; không vào thẳng bàn |
| P2-02 | Sảnh bấm Khiêu chiến | Thấy đủ 5 đảo cùng hình riêng |
| P2-03 | Chọn Mộc Tinh | Hiện bản đồ đủ 7 ải, boss và trạng thái |
| P2-04 | Chọn Hỏa/Kim/Thủy/Thổ | Có trang preview; boss chưa làm ghi Coming Soon |
| P2-05 | Bấm chức năng PvP/Shop | Nút bị khóa/tooltip, không hiện Scene rỗng |
| P2-06 | Mở kho Pet lần đầu | Có đủ ít nhất 11 Pet thuộc 5 hệ, không có duplicate |
| P2-07 | Thoát/vào game lại | Full collection demo không cộng dồn, active Pet giữ nguyên |
| P2-08 | Chọn Pet xuất chiến | Sảnh, màn boss và trận dùng cùng Pet/form |
| P2-09 | Pet Mầm Miu Lv7 | Giữ form cũ, stat level tăng |
| P2-10 | Lá Lùn lên Lv4 | Đổi form, nhận đúng bonus, save và UI thay |
| P2-11 | Cổ Thụ Lv4 rồi Lv7 | Có đúng 2 mốc form, không nhân lại bonus qua restart |
| P2-12 | Đá khác hệ có cùng Lv | Không nâng cấp được, không tiêu đá |
| P2-13 | Bấm nâng cấp liên tục | Chỉ trừ đúng 1 đá cho một xác nhận |
| P2-14 | Kiểm tra boss Mộc 1→7 | HP lần lượt 440, 530, 630, 760, 900, 1070, 1270 |
| P2-15 | Đòn vàng 3 viên Pet B ATK29 vs DEF9 | Ra 62 damage trung tính, không crit/buff |
| P2-16 | Boss ải1 đánh Pet HP260 | Trung bình đòn thường không quá mạnh, gần mức ~25 ở ví dụ |
| P2-17 | Nộ=72, Mana=29, có 3 kiếm | Chỉ đánh thường, không tiêu Mana/Nộ do tuyệt kỹ |
| P2-18 | Nộ=72, Mana=35, có 3 kiếm | Dùng tuyệt kỹ, trừ 60 Nộ và 30 Mana |
| P2-19 | Hai nhóm kiếm cùng lượt | Xét điều kiện mỗi đòn, không dùng quá tài nguyên hiện có |
| P2-20 | Boss HP từ 35% xuống 28% | Sau đòn đánh, hóa hình đúng 1 lần, giữ HP thật |
| P2-21 | Boss HP từ 35% xuống 0 | Không hóa hình, vào Victory |
| P2-22 | Boss đã hóa hình tiếp tục bị đánh | Không nhận buff/enrage lần thứ hai |
| P2-23 | Nhặt combo xanh, đỏ, vàng | Ăn xong toàn bộ → ẩn bàn → VFX buff → đòn kiếm |
| P2-24 | Hồi HP, Mana, Nộ, giáp và trộm | Có animation, số tăng/giảm và HUD đúng model |
| P2-25 | Animation 2× | Chỉ nhanh hơn, không đổi sát thương hay random |
| P2-26 | Thử 7 đá Mộc | Có 7 silhouette thật khác nhau và icon đủ nét |
| P2-27 | Boss 7 sau khi thu phục ở demo | Không tạo Pet bản sao; vẫn nhận đá/progression |
| P2-28 | Nâng Pet đạt Lv7, thoát game | Lv7, hình tiến hóa và tài nguyên còn lại khớp |
| P2-29 | Dùng thẻ tiến hóa tốn 80 Mana | Buff chỉ trong trận và trở lại đúng permanent form |
| P2-30 | Kiểm tra hiệu năng khi cascade dài | Không double event, không treo lượt, animation vẫn hoàn tất |

### 12.2 Thứ tự triển khai Codex

**Bước A: Audit và chốt art direction.** Liệt kê Scenes/Prefabs/scripts hiện có, kiểm tra xung đột P1→P2, xây UI style tokens (màu, font, icon, panel, nút), tạo thiết kế gốc cho 7 Pet Mộc + 4 Pet đại diện và 5 theme đảo. Hoàn thiện bố cục sảnh trước khi làm những màn con.

**Bước B: Dựng toàn bộ màn hình.** Home Lobby → Challenge Island Select → 5 Island Preview → Mộc 7 ải → Boss Preview → Battle → Result, rồi Pet Storage → Pet Detail/Evolution → Item Storage → Upgrade → Settings/Bestiary. Link tất cả Back/Home, Coming Soon thật.

**Bước C: Hệ Pet và tiến hóa.** Seed đầy kho thử nghiệm từ PetDefinition, nâng Lv, form Lv4/Lv7 tùy loài, portrait/Animator override, nâng cấp đá theo hệ, save migration, test.

**Bước D: Gameplay và boss.** Áp dụng boss HP/scale P2, cần đủ Mana+Nộ cho tuyệt kỹ, tự hóa hình dưới 30% HP, tạo sprite/Animator riêng cho boss. Giữ ngọc màu và loop Match-3 đúng như P1.

**Bước E: VFX animation và polish.** Làm lại chuyển cảnh thu hết ngọc → ẩn bàn → effect/battle show → hiện bàn; hồi HP/Mana/Nộ/giáp, crit, special, enrage, victory, âm thanh. Chạy đủ 30 test và mô phỏng cân bằng. Thu thập screenshot/video Play Mode nếu môi trường cho phép.

### 12.3 Bàn giao và định nghĩa hoàn thành

Codex phải giao **project Unity chạy được**, gồm source C#, Scene/Prefab, Sprite/Atlas, Animator/AnimationClip, ScriptableObject config, test và hướng dẫn bấm Play/Build. Có bảng `Done / Partial / Coming Soon`, ghi rõ asset nào thực sự vẽ xong, asset nào đang là placeholder, cơ chế nào đã Play-test. Không đánh dấu hoàn tất một tính năng khi chỉ có UI tĩnh, một sprite không động, hoặc nút không xử lý.

**Ưu tiên đánh giá thành công:** (1) sảnh thực sự đẹp và dẫn đến các chức năng đã có; (2) Pet gốc có chi tiết và animation thật; (3) năm đảo hiện đúng, Mộc 7 ải chơi được; (4) tiến hóa đúng mốc Lv, boss tự thức tỉnh dưới 30%; (5) tuyệt kỹ cần cả Mana và Nộ; (6) boss HP nhiều hơn nhưng damage vừa phải, đánh có nhịp và kết thúc được; (7) mọi UI/tiến trình có thể kiểm tra và lưu được.
