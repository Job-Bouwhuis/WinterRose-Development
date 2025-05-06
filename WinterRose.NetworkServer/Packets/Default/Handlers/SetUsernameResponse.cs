using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Packets.Default.Responses
{
    class SetUsernameResponse : PacketHandler
    {
        private static List<Func<string, bool>> validations = [];
        public static void AddNameValidation(Func<string, bool> validator) => validations.Add(validator);

        public override string Type => "SETUSERNAME";

        public override void Handle(Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            Validate(((StringContent)packet.Content).Content);
        }

        public override void HandleResponsePacket(ReplyPacket replyPacket, Packet packet, NetworkConnection self, NetworkConnection sender)
        {
            string name = ((StringContent)packet.Content).Content;
            if (Validate(name))
            {
                logger.LogInformation($"User {sender.Username} ({sender.Identifier}) changed their username to {name}");
                sender.Username = name;

                sender.Send(replyPacket.Reply(new OkPacket(), self));
            }
            else
                sender.Send(replyPacket.Reply(new NoPacket(), self));
        }

        private bool Validate(string name)
        {
            foreach(var val in validations)
                if (!val(name))
                    return false;
            return true;
        }
    }
}
