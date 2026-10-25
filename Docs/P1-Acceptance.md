# Bàn giao Pet Arena Classic P1

Bản nâng cấp trực tiếp trên project hiện tại, không thay phiên bản Unity hoặc package. Backup trước cập nhật: Backups/Before-P1.zip. Entry Windows: P1Home. Scene Battle cũ được giữ.

## Nội dung và hệ thống

- Sảnh, đảo 7 boss Mộc, chuẩn bị trận, kho Pet, chi tiết/xuất chiến, Sách Pet silhouette, kho 35 loại đá, nâng cấp Lv1–Lv7, đấu thử và hướng dẫn thẻ.
- 6 bậc C/B/A/S/SS/SSS, 5 hệ, vòng tương khắc cấu hình; SPD quyết định đi trước và không tạo lượt phụ.
- Thu phục 100% first clear, farm đá không thêm bản sao, mở ải tuần tự. Thắng/thua có luồng quay lại, đánh lại và ải tiếp.
- Save JSON versioned + backup + ghi qua file tạm. Thưởng/upgrades có ID giao dịch; không thay state trước khi lưu thành công.
- Combat gom hết cascade, chuẩn bị plan trên state copy, ẩn bàn, phát từng hiệu ứng/đòn nối tiếp, cập nhật HP tại impact, hiện lại bàn khi còn sống. x2 giữ nguyên RNG/damage và thời gian quyết định.
- 8 rig Pet có bộ phận chuyển động độc lập; 7 trạng thái với Animator Controller/clip/event thật. Evolve có crown/silhouette riêng; Cổ Thụ có rễ trói.
- 7 mask đá và PNG khác nhau; màu theo hệ. Coming Soon khóa thao tác và có giải thích khi hover.

## Test P1 T01 đến T25

| Mã | Bằng chứng |
| --- | --- |
| T01–T03 | P1_ElementPairs: kiểm tra thuận/ngược/trung tính và đủ 5 cạnh khắc |
| T04 | P1_T04_AllSixRaritiesAndSpeciesValidate |
| T05–T08 | WrongElementCannotUpgrade; UpgradeConsumesCorrectStoneAndSaves; LevelSevenNeverConsumes; RestartPreservesSelectionAndLevel |
| T09–T12 | FirstClearCapturesAndUnlocks; FarmDoesNotDuplicatePet; LossDoesNotReward; FinalBossIsAAndBuffNotCaptured |
| T13 | FasterEnemyStartsAndAlternates, có test runtime |
| T14–T16 | CollectionHasNoDamageAndBoardHidesBeforeImpact; TwoAttacksSpendRageSeparately; ImpactCannotApplyTwiceOrReroll |
| T17 | SpecialCritElementOrder: 122 ×1,25 làm tròn ra 153 trước Shield |
| T18 | LethalImpactNeverShowsBoardAgain |
| T19 | HomeNavigationAndComingSoonAreReal |
| T20 | PresentationSpeedCannotAffectPreparedDamage; SpeedDoesNotAccelerateDecisionTimer |
| T21 | SevenDistinctStoneSilhouettes |
| T22 | UnloadDuringCombatResetsTimeAndNewSession |
| T23 | DoubleClickAndDuplicateRewardAreIdempotent |
| T24 | FilterOwnedWoodC |
| T25 | TwoHundredTenCinematicBattlesConclude, seeds 0–29 ở 7 ải |
| Bổ sung | CorruptSaveRestoresBackup; FailedSaveDoesNotConsumeStoneOrLevel; InvalidActivePetIsRejected; RareDropRollsOnceAndIsSeedReproducible; FullHomeBattleRewardAndReload; AnimatorEmitsImpactAndCompletionFrames |

89/89 EditMode và 24/24 PlayMode đạt. XML: Logs/P1-EditMode.xml, Logs/P1-PlayMode.xml. Logs/P1Simulation.md ghi kết quả riêng từng ải; tỉ lệ thắng tùy Pet/Lv dùng trong mô phỏng, không phải bảo đảm thắng cho người thật. Bộ hồi quy P0 vẫn chạy 1.000 trận có giới hạn vòng.

## Kiểm tra bản Windows

Đã mở P1 trên bản Windows 1600×900: sảnh, đảo, màn chuẩn bị đúng stat/khắc hệ, vào/ra trận, pause/resume, kho đá, swap và popup chí mạng/khắc chế. Có ảnh tại Docs/P1-Home.png, P1-Island.png, P1-Stones.png, P1-Combat.png. Luồng nhận thưởng và nạp lại save có test PlayMode tích hợp độc lập, không chỉnh file save người chơi để tạo ảnh thưởng giả.

## Phần cần tiếp tục trau chuốt

Rig và stone art hiện là bộ tạm nguyên bản bằng sprite hình học, chưa có bộ tranh/texture/rig chuyên nghiệp riêng từng loài. Bản đồ đang dùng thẻ ải trên nền rừng, chưa có đường đi uốn/parallax riêng. Animation Controller dùng chung state/curve, từng loài có bộ phận và hình dáng riêng; chưa có sprite sheet vẽ từng khung. Chưa có replay editor thao tác riêng; dùng PlayMode tests để chạy lại các tình huống thường/crit/evolve/hide/show.

PvP, bang hội, bạn bè, cửa hàng, triệu hồi, nhiệm vụ/xếp hạng, đảo khác và ghép đá vẫn Coming Soon đúng phạm vi. Chưa triển khai backend, mua bán, gacha hoặc nhiều Pet chiến đấu đồng thời.

Hướng dẫn chi tiết chạy/config/save: Docs/P1-Guide.md. Build: Builds/Windows/PetArenaClassic.exe; giữ nguyên toàn bộ thư mục Windows khi copy game.

Build cuối có Animator: Logs/P1-Build-Animator.log và Logs/WindowsBuild.txt — Succeeded, 0 errors, 485 cảnh báo shader từ package Unity Inference/ConvGeneric đã có trong project; dung lượng báo cáo 161.094.444 byte. Đã mở lại bản cuối tại sảnh, không thấy exception gameplay hoặc cảnh báo thiếu Animator trong Logs/P1-Standalone-Final.log.
