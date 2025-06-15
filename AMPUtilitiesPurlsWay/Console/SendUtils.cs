using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.API.Managers;
using Torch.Commands;

namespace AMPUtilitiesPurlsWay.Console
{
    public class SendUtils
    {
        public void SendChatMessage(string author, string message)
        {
            var chat = Torch.Managers?.GetManager<IChatManagerServer>();
            chat?.SendMessageAsOther(author, message, "White");
        }

        public void RunCommand(string commandText)
        {
            var cmdMan = Torch.Managers.GetManager<CommandManager>();
            var command = cmdMan?.Commands?.GetCommand(commandText, out string argsText);

            if (command != null)
            {
                var argsList = argsText.Split(' ').ToList();
                var context = new CommandContext(Torch, Torch, 0, argsText, argsList);
                command.TryInvoke(context);
            }
        }
    }
}
