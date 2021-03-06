﻿using TCC.Data;
using TCC.Data.Databases;

namespace TCC.ViewModels
{
    public class GunnerBarManager : ClassManager
    {
        private static GunnerBarManager _instance;
        public static GunnerBarManager Instance => _instance ?? (_instance = new GunnerBarManager());

        public DurationCooldownIndicator BurstFire { get; set; }
        public DurationCooldownIndicator Balder { get; set; }
        public DurationCooldownIndicator Bombardment { get; set; }
        public GunnerBarManager() : base()
        {
            _instance = this;
            CurrentClassManager = this;
            LoadSpecialSkills();
            ST.PropertyChanged += FlashBfIfFullWp;
        }

        private void FlashBfIfFullWp(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ST.Maxed))
            {
                BurstFire.Cooldown.ForceAvailable(ST.Maxed);
                Balder.Cooldown.FlashOnAvailable = ST.Maxed;
                Bombardment.Cooldown.FlashOnAvailable = ST.Maxed;
            }
        }

        protected override void LoadSpecialSkills()
        {
            SkillsDatabase.TryGetSkill(51000, Class.Engineer, out Skill bfire);
            SkillsDatabase.TryGetSkill(130200, Class.Engineer, out Skill balder);
            SkillsDatabase.TryGetSkill(20700, Class.Engineer, out Skill bombard);

            BurstFire = new DurationCooldownIndicator(_dispatcher);
            Balder = new DurationCooldownIndicator(_dispatcher);
            Bombardment = new DurationCooldownIndicator(_dispatcher);

            BurstFire.Buff = new FixedSkillCooldown(bfire, CooldownType.Skill, _dispatcher, false);
            Balder.Buff = new FixedSkillCooldown(balder, CooldownType.Skill, _dispatcher, false);
            Bombardment.Buff = new FixedSkillCooldown(bombard, CooldownType.Skill, _dispatcher, false);
            BurstFire.Cooldown = new FixedSkillCooldown(bfire, CooldownType.Skill, _dispatcher, true);
            Balder.Cooldown = new FixedSkillCooldown(balder, CooldownType.Skill, _dispatcher, false);
            Bombardment.Cooldown = new FixedSkillCooldown(bombard, CooldownType.Skill, _dispatcher, false);

        }

        public override bool StartSpecialSkill(SkillCooldown sk)
        {
            if (sk.Skill.IconName == Balder.Cooldown.Skill.IconName)
            {
                Balder.Cooldown.Start(sk.Cooldown);
                return true;
            }
            if (sk.Skill.IconName == Bombardment.Cooldown.Skill.IconName)
            {
                Bombardment.Cooldown.Start(sk.Cooldown);
                return true;
            }
            return false;
        }
    }
}
