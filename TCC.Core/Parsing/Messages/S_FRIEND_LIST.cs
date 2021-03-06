﻿using System;
using System.Collections.Generic;
using TCC.Data;
using Tera.Game;
using Tera.Game.Messages;

namespace TCC.Parsing.Messages
{
    public class S_FRIEND_LIST : ParsedMessage
    {
        public List<SimpleUser> Friends;
        public S_FRIEND_LIST(TeraMessageReader reader) : base(reader)
        {
            Friends = new List<SimpleUser>();
            var count = reader.ReadUInt16();
            reader.Skip(4);
            reader.ReadTeraString();
            for (int i = 0; i < count; i++)
            {
                try
                {
                    Friends.Add(ParseFriend(reader));

                }
                catch (Exception)
                {

                }
            }
        }

        private SimpleUser ParseFriend(TeraMessageReader reader)
        {
            reader.Skip(4);
            var nameOffset = reader.ReadUInt16();
            reader.Skip(4);
            var id = reader.ReadUInt32();
            reader.BaseStream.Position = nameOffset - 4;
            var name = reader.ReadTeraString();
            reader.ReadTeraString();
            reader.ReadTeraString();
            return new SimpleUser(id, name);
        }
    }
}
