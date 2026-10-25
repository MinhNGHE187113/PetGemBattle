# Pet Arena Classic — P2

Unity 2D offline, Unity 6000.3.27f1, Windows x64.

**Chạy ngay:** mở `Builds/Windows/PetArenaClassic.exe`.

**Trong Unity:** Tools → Pet Gem Battle → Open P2 Home → Play.

Sảnh → Khiêu chiến → Mộc Tinh → chọn một trong 7 ải. Bốn đảo còn lại có preview riêng và boss Coming Soon. Demo mở 12 Pet Lv1; Cài đặt có cấp đá thử nghiệm một lần/save.

- [Hướng dẫn P2](Docs/P2-Guide.md)
- [Nghiệm thu và giới hạn](Docs/P2-Acceptance.md)
- [Nguồn asset/prompt](Docs/ART-PROMPTS-P2.md)
- [Mô phỏng 700 trận](Logs/P2BalanceSimulation.md)

Snapshot trước P2: `Backups/Before-P2.zip`. Scene chính giữ tên `P1Home.unity` để tương thích project cũ.

## Source và Git

Repo lưu Assets (kèm .meta), Packages, ProjectSettings và Docs.
Library, Temp, Logs, UserSettings, Builds, Backups và file solution/project IDE tự sinh được bỏ qua.
Sau khi clone, mở thư mục project bằng Unity Hub với Unity 6000.3.27f1 để Unity khôi phục packages và cache.
Bản .exe không nằm trong source Git; tạo lại qua **Tools → Pet Gem Battle → Build Windows**.

Code C# nằm trong Assets/PetGemBattle/Scripts, công cụ Unity trong Editor và kiểm thử trong Tests.
Để mở bằng Visual Studio hoặc VS Code: **Edit → Preferences → External Tools → External Script Editor**,
chọn editor đã cài rồi nhấp đúp một file .cs trong cửa sổ Project của Unity.
