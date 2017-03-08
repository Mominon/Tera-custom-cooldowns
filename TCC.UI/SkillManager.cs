﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCC.Messages;

namespace TCC
{
    public delegate void SkillAddedEventHandler(object sender, EventArgs e, SkillCooldown s);
    public delegate void SkillOverEventHandler(object sender, EventArgs e, SkillCooldown s);
    public delegate void SkillCooldownChangedEventHandler(object sender, EventArgs e, SkillCooldown s);

    public static class SkillManager
    {
        public const int LongSkillTreshold = 40000;
        public const int Ending = 250;
        public const uint HurricaneId = 60010;

        public static SkillQueue NormalSkillsQueue = new SkillQueue();
        public static SkillQueue LongSkillsQueue = new SkillQueue();

        public static SkillListener NormalSkillsQueueListener = new SkillListener(NormalSkillsQueue);
        public static SkillListener LongSkillsQueueListener = new SkillListener(LongSkillsQueue);

        public static List<string> LastSkills = new List<string>();


        public static event SkillCooldownChangedEventHandler Changed;

        static bool Filter(string name)
        {
            if (name != "Unknown" &&
                !name.Contains("Summon:") &&
                !name.Contains("Flight:") &&
                !name.Contains("Super Rocket Jump")&&
                !name.Contains("greeting") ||
                name == "Summon: Party") return true;
            else return false;

        }
        public static void AddSkill(SkillCooldown sk)
        {
            switch (sk.Type)
            {
                case CooldownType.Skill:
                    string skillName = SkillsDatabase.SkillIdToName(sk.Id, PacketParser.CurrentClass);
                    if (sk.Cooldown == 0)
                    {
                        ResetSkill(sk.Id);
                    }
                    else if (Filter(skillName) && !LastSkills.Contains(skillName))
                    {
                        if (sk.Cooldown >= LongSkillTreshold)
                        {
                            LongSkillsQueue.Add(sk);
                        }
                        else
                        {
                            NormalSkillsQueue.Add(sk);
                        }
                        LastSkills.Add(skillName);
                        //Console.WriteLine("{0} added.", skillName);

                    }
                    break;

                case CooldownType.Item:
                    if (BroochesDatabase.GetBrooch(sk.Id) != null)
                    {
                        var name = BroochesDatabase.GetBrooch(sk.Id).Name;
                        if (!LastSkills.Contains(name))
                        {
                            LongSkillsQueue.Add(sk);
                            LastSkills.Add(name);
                            //Console.WriteLine("{0} added.", name);
                        }
                    }
                    break;

                default:
                    break;
            }

        }
        public static void ResetSkill(uint id)
        {
            if (LongSkillsQueue.Where(x => x.Id == id).Count() > 0)
            {
                LongSkillsQueue.Where(x => x.Id == id).Single().Timer.Stop();
                LongSkillsQueue.Remove(LongSkillsQueue.Where(x => x.Id == id).Single());
                CooldownsBarWindow.RemoveLongSkill(new SkillCooldown(id, 0, CooldownType.Skill));
            }
            else if (NormalSkillsQueue.Where(x => x.Id == id).Count() > 0)
            {
                NormalSkillsQueue.Where(x => x.Id == id).Single().Timer.Stop();
                NormalSkillsQueue.Remove(NormalSkillsQueue.Where(x => x.Id == id).Single());
                CooldownsBarWindow.RemoveNormalSkill(new SkillCooldown(id, 0, CooldownType.Skill));

            }

            try
            {
                var name = SkillsDatabase.GetSkill(id, PacketParser.CurrentClass).Name;
                LastSkills.Remove(name);
                //Console.WriteLine(name + " reset.");
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot reset skill.");
            }
        }
        public static void ChangeSkillCooldown(SkillCooldown sk)
        {
            if (sk.Cooldown > SkillManager.LongSkillTreshold)
            {
                if (LongSkillsQueue.Where(X => X.Id == sk.Id).Count() > 0)
                {
                    if (sk.Cooldown == 0)
                    {
                        ResetSkill(sk.Id);
                    }
                    else
                    {
                        LongSkillsQueue.Where(x => x.Id == sk.Id).Single().Timer.Interval = sk.Cooldown;

                        Changed?.Invoke(null, null, sk);
                    }
                }
            }
            else
            {
                if (NormalSkillsQueue.Where(X => X.Id == sk.Id).Count() > 0)
                {
                    if (sk.Cooldown == 0)
                    {
                        ResetSkill(sk.Id);
                    }
                    else
                    {
                        NormalSkillsQueue.Where(x => x.Id == sk.Id).Single().Timer.Interval = sk.Cooldown;

                        Changed?.Invoke(null, null, sk);
                    }
                }

            }

            try
            {
                //Console.WriteLine("{0} cooldown reduced.", SkillsDatabase.GetSkill(sk.Id, PacketParser.CurrentClass).Name);

            }
            catch (Exception)
            {

                Console.WriteLine("Unknown's skill cooldown reduced.");
            }
        }

        public static void Clear()
        {
            NormalSkillsQueue.Clear();
            LongSkillsQueue.Clear();
            PacketParser.CurrentClass = Class.Common;
            PacketParser.CurrentCharId = 0;
            CooldownsBarWindow.ClearSkills();
            WindowManager.HideEdgeGauge();
            LastSkills.Clear();

            Console.WriteLine("Manager cleared.");
        }
    }
}
