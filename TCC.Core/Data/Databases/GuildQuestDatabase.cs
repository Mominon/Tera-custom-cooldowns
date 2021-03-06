﻿using System;
using System.Collections.Generic;
using System.IO;

namespace TCC.Data.Databases
{
    public static class GuildQuestDatabase
    {
        public static Dictionary<uint, GuildQuest> GuildQuests;
        public static void Load()
        {
            GuildQuests = new Dictionary<uint, GuildQuest>();
            var f = File.OpenText(Environment.CurrentDirectory + "/resources/data/guild-quests.tsv");
            while (true)
            {
                var line = f.ReadLine();
                if (line == null) break;
                var s = line.Split('\t');
                var id = uint.Parse(s[0]);
                var str = s[1];
                //var zId = uint.Parse(s[2]);
                GuildQuests.Add(id, new GuildQuest(id, str));
            }
        }
    }
    public class GuildQuest
    {
        public uint Id { get; private set; }
        public string Title { get; private set; }
        public uint ZoneId { get; private set; }

        public GuildQuest(uint id, string s)
        {
            Id = id;
            Title = s;
            //ZoneId = zId;
        }
    }

}
