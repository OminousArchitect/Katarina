using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Achievements;
using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace SurvivorTemplate
{
    public static class Achievements
    {
        internal static UnlockableDef masteryUnlock;
        public static void RegisterUnlockables()
        {
            //masteryUnlock = NewUnlockable<Achievements.MasteryUnlockable>("MASTERYUNLOCKABLE", ICON, "SURVIVOR: Mastery", "As SURVIVOR, beat the game or obliterate on Monsoon.");
        }
        static UnlockableDef NewUnlockable<T>(string AchievementIdentifier, Sprite Icon, string Title, string Description) where T : BaseAchievement
        {
            string IDKey = "ACHIEVEMENT_SURVIVOR_";
            var unlock = ScriptableObject.CreateInstance<UnlockableDef>();
            string langName = IDKey + AchievementIdentifier + "_NAME";
            string langDesc = IDKey + AchievementIdentifier + "_DESCRIPTION";
            LanguageAPI.Add(langName, Title);
            LanguageAPI.Add(langDesc, Description);
            var s = new Func<string>(() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
            {
                Language.GetString(langName),
                Language.GetString(langDesc)
            }));
            Type type = typeof(T);

            unlock.cachedName = IDKey + AchievementIdentifier + "_UNLOCKABLE_ID";
            unlock.getHowToUnlockString = s;
            unlock.getUnlockedString = s;
            unlock.achievementIcon = Icon;
            unlock.sortScore = 200;
            unlock.hidden = false;
            ContentAddition.AddUnlockableDef(unlock);
            return unlock;
        }
        [RegisterAchievement("SURVIVOR_MASTERYUNLOCKABLE", "ACHIEVEMENT_SURVIVOR_MASTERYUNLOCKABLE_UNLOCKABLE_ID", null, null)]
        public class MasteryUnlockable : BasePerSurvivorClearGameMonsoonAchievement
        {
            public override BodyIndex LookUpRequiredBodyIndex()
            {
                return BodyCatalog.FindBodyIndex("AatroxBody");
            }
        }
    }
}
