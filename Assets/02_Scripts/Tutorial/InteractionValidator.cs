public enum TutorialLockType { None, HouseExit, Spaceship, ShopEnter, Bed, ExploreNextMap, WalkToTown }
public enum TimeLockType { None, MorningOnly, DayOnly, EveningOnly, NightOnly, NotNight }
public enum AccessLockType { None, NoPermission, UnderConstruction }

public static class InteractionValidator
{
    public static bool CanInteract(TutorialLockType tutoLock, out string rejectMessage)
    {
        return CanInteract(tutoLock, TimeLockType.None, AccessLockType.None, out rejectMessage);
    }

    public static bool CanInteract(TutorialLockType tutoLock, TimeLockType timeLock, AccessLockType accessLock, out string rejectMessage)
    {
        if (!CheckTutorial(tutoLock, out rejectMessage)) return false;
        if (!CheckTime(timeLock, out rejectMessage)) return false;
        if (!CheckAccess(accessLock, out rejectMessage)) return false;

        return true;
    }

    private static bool CheckTutorial(TutorialLockType lockType, out string msg)
    {
        msg = string.Empty;
        if (lockType == TutorialLockType.None) return true;

        if (GameSaveManager.Instance != null)
        {
            if (GameSaveManager.Instance.IsTutorialCompleted()) return true;
        }

        switch (lockType)
        {
            case TutorialLockType.HouseExit:
                if (QuestManager.Instance.GetActiveQuest(QuestID.Tuto_02) == null &&
                    !QuestManager.Instance.completedQuestIDs.Contains(QuestID.Tuto_02))
                {
                    msg = LocalizationHelper.Main("NOTI_NEED_TALK_PARENT");
                    return false;
                }
                break;

            case TutorialLockType.Spaceship:
                if (QuestManager.Instance.GetActiveQuest(QuestID.Tuto_06) == null &&
                    !QuestManager.Instance.completedQuestIDs.Contains(QuestID.Tuto_06))
                {
                    msg = LocalizationHelper.Main("NOTI_NEED_PREPARE_EXPLORE");
                    return false;
                }
                break;

            case TutorialLockType.ShopEnter:
                if (QuestManager.Instance.GetActiveQuest(QuestID.Tuto_09) == null &&
                    !QuestManager.Instance.completedQuestIDs.Contains(QuestID.Tuto_09))
                {
                    msg = LocalizationHelper.Main("NOTI_SHOP_NOT_READY");
                    return false;
                }
                break;

            case TutorialLockType.Bed:
                Quest tuto10 = QuestManager.Instance.GetActiveQuest(QuestID.Tuto_10);
                if (tuto10 == null)
                {
                    msg = LocalizationHelper.Main("NOTI_TUTO_NOT_DONE");
                    return false;
                }
                break;

            case TutorialLockType.ExploreNextMap:
                msg = LocalizationHelper.Main("NOTI_EXPLORE_NEXT_LOCKED");
                return false;

            case TutorialLockType.WalkToTown:
                msg = LocalizationHelper.Main("NOTI_WALK_TOWN_LOCKED");
                return false;
        }
        return true;
    }

    private static bool CheckTime(TimeLockType lockType, out string msg)
    {
        msg = string.Empty;
        if (lockType == TimeLockType.None) return true;

        GAME_TIME currentTime = GameManager.Instance.currentTime;

        switch (lockType)
        {
            case TimeLockType.MorningOnly:
                if (currentTime != GAME_TIME.Morning)
                {
                    msg = LocalizationHelper.Main("NOTI_SHOP_MORNING_ONLY");
                    return false;
                }
                break;

            case TimeLockType.DayOnly:
                if (currentTime != GAME_TIME.Day)
                {
                    msg = LocalizationHelper.Main("NOTI_SHOP_DAY_ONLY");
                    return false;
                }
                break;

            case TimeLockType.EveningOnly:
                if (currentTime != GAME_TIME.Evening)
                {
                    msg = LocalizationHelper.Main("NOTI_SHOP_EVENING_ONLY");
                    return false;
                }
                break;

            case TimeLockType.NightOnly:
                if (currentTime != GAME_TIME.Night)
                {
                    msg = LocalizationHelper.Main("NOTI_SHOP_NIGHT_ONLY");
                    return false;
                }
                break;

            case TimeLockType.NotNight:
                if (currentTime == GAME_TIME.Night)
                {
                    msg = LocalizationHelper.Main("NOTI_CLOSED_AT_NIGHT");
                    return false;
                }
                break;
        }
        return true;
    }

    private static bool CheckAccess(AccessLockType lockType, out string msg)
    {
        msg = string.Empty;
        if (lockType == AccessLockType.None) return true;

        if (lockType == AccessLockType.NoPermission)
        {
            msg = LocalizationHelper.Main("NOTI_NO_PERMISSION");
            return false;
        }

        if (lockType == AccessLockType.UnderConstruction)
        {
            msg = LocalizationHelper.Main("NOTI_UNDER_CONSTRUCTION");
            return false;
        }

        return true;
    }
}