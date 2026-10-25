# Đối chiếu đặc tả Pet Arena Classic ngày 25/10/2026

Phạm vi triển khai theo mục 10.3: P0, các pha A–F. Không thêm tài khoản, database hoặc server. P1–P3 là lộ trình tương lai.

## Kiểm chứng tự động

Unity 6000.3.27f1, không đổi package hoặc Input System. EditMode: 59/59 đạt. PlayMode: 15/15 đạt. XML gốc ở Logs/PetGemBattle-TestResults.xml và Logs/PetGemBattle-PlayModeResults.xml.

| Đặc tả | Bằng chứng test |
| --- | --- |
| TC01–02 | InitialBoardsHaveSixTypesNoMatchesAndLegalMoves; InvalidSwapsAndNonNeighborsDoNotMutateBoard; InvalidMoveReturnsToSameTurnWithoutResettingTimer |
| TC03–04 | NormalSwordFormulaAndRageGain; CrossMergesIntoFiveUniqueCells; AdjacentMatchedRunsMergeEvenWithoutSharedCell; GroupBonusIsOncePerGroupIndependentOfCombo |
| TC05–06 | GravityPreservesSurvivorOrderAndRefillsAllCells; DragAndComboLockInputAndAIHandsBackControl; DeadBoardShuffleRestoresStablePlayability |
| TC07–11 | HeartActuallyHealsWoundedPet; NormalSpecialCriticalRoundInSpecifiedOrder; CriticalSeedReproducesActualRollsAndFrequency |
| TC12–14 | StableGroupOrderAvoidsDuplicateCellsAndUsesRedRageBeforeSwords; ComboAppliesToAllResourceFormulasAndShieldsAbsorbFirst; CriticalHitsShieldBeforeHpAndUsesOneRollForLargeGroup |
| TC15–16 | ActualDrainTrialsMatchIndependentProbabilities; DrainRollsAreIndependentAndUsePostDamageShield; HpDrainHealsOnlyActualHpLostAndCapsAtTwelvePercent; FullReceiverDoesNotDestroyVictimResources |
| TC17–20 | CardsEnforceChargesAndOneCardPerTurn; EvolutionValidatesCostOnceAndUpdatesStatsWithoutMutatingDefinition; EvolutionCardChangesStatsAndSpriteAndLocksOtherCards |
| TC21 | AISelectsLegalMovesWithoutMutationAndEvolvesWhenReady; AiTieRandomIsSeededAndNeverConsumesCombatRandom; TurnTimeoutAutomaticallyMakesALegalMove |
| TC22–23 | SuddenDeathUsesCompletedRoundsAndCaps; SuddenDeathAnnouncesAfterCompletedRoundAndKeepsCardsWorking; RoundLimitWaitsForBothTurnsThenStopsAndRestartClearsRoundState |
| TC24 | RestartDuringSwapCancelsPreviousResolution; RestartDuringEvolutionResetsFormResourcesAndCharges; CriticalSpecialPresentationMatchesReportAndRestartClearsEffects |
| TC25 | ThousandSeededAiBattlesAlwaysConcludeWithinThirtyRounds: 1.000 seed, mọi trận kết thúc; trung vị 15 vòng, 100% trước vòng 30 |
| Bổ sung | PauseFreezesTimerRejectsActionsAndRestartResumes; PauseDuringResolutionResumesWithoutDuplicatingEvents; CanvasUsesFullHdReferenceAndJournalRoundTrips; InvalidConfigAndUnknownCardsAreRejected |

## Những quyết định cụ thể

- Thưởng 4/5+ áp dụng cho mọi loại ngọc, cộng trước hiệu ứng của nhóm; không nhân cascade.
- Trong cùng đợt: tài nguyên → hút → kiếm. Trong hút: HP → Nộ → giáp.
- Sudden Death: vòng 16–17 +10%, 18–19 +20%, 20–21 +30%, 22–23 +40%, 24–25 +50%, 26–30 +60%. Chỉ tăng damage kiếm; hút giữ công thức phần trăm và trần riêng.
- ATK/DEF tiến hóa giữ số thực. 25×1,35 = 33,75 và 8×1,2 = 9,6.
- Giữ cấu trúc Assets/PetGemBattle hiện có thay vì đổi tên hàng loạt. BattleRules là GameBalanceConfig của project này; PetDefinition và tham số thẻ vẫn nằm trong asset chung. UI được dựng runtime, không yêu cầu prefab thủ công.
- Nhật ký JSON là chẩn đoán tại máy người chơi, không phải lưu tiến trình hay replay hoàn chỉnh. Có seed bàn, seed combat suy ra từ seed trận, seed AI riêng, cấu hình, snapshot và random roll combat.
- Hình Pet là 4 ảnh gốc có chuyển động lập trình, chưa phải bộ sprite atlas 6 trạng thái vẽ riêng hoặc rig. Âm thanh và nhạc là tổng hợp nguyên bản, chất lượng prototype.

## Cân bằng

Báo cáo trước: Docs/Balance-Before.md; sau: Logs/BalanceSimulation.md. Trung bình 14,36 vòng; P90 21; bên đi trước thắng 56,8%. Ước tính trung bình 4,18 phút khi giả định 12 giây suy nghĩ/lượt người chơi. Chưa đo playtest người thật; lợi thế đi trước cần theo dõi.

## Lộ trình chưa triển khai

P1: chọn Pet, ba ải, XP/cấp, Gold, phần thưởng chống nhận lặp, lưu tiến độ. P2: thu phục, bộ sưu tập, nhiệm vụ/boss. P3: server authoritative, matchmaking, reconnect, state hash và replay. Chưa hỗ trợ mobile/touch hoặc khắc hệ.

## Windows và kiểm tra trực quan

Build thành công: 0 lỗi, 485 warnings, khoảng 153,4 MiB. Cảnh báo ConvGeneric từ Unity Inference; không sửa hoặc gỡ package sẵn có. Bản exe mở được, thẻ Mana cộng đúng +20, pause/continue và restart hoạt động, ghép ba Mana bằng click cộng +24. Kiểm tra bố cục Full HD, 1366×768 và 1600×900 không thấy phần tử tràn. Test drag bằng PointerEventData đạt; thao tác kéo qua công cụ chuột chưa được xác nhận. Log standalone có cảnh báo cleanup ComputeBuffer lúc thoát và lỗi kết nối Curl từ runtime; chưa thấy exception của gameplay.
