using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EveChessBackEnd
{
    class ChessTimer
    {
        IClient client1, client2;

        Timer timerWhites;
        Timer timerBlacks;

        public ChessTimer(IClient client1,IClient client2)
        {
            this.client1 = client1;
            this.client2 = client2;
            timerWhites = new Timer();
            timerBlacks = new Timer();
            timerWhites.Interval = 1000;
            timerBlacks.Interval = 1000;
            timerWhites.Elapsed += TimerElapsed;
            timerBlacks.Elapsed += TimerElapsed;
        }

        public void TimerElapsed(object sender, ElapsedEventArgs args)
        {
            if (((Timer)sender).Equals(timerWhites))
            {
                using (DarkRiftWriter messageWriter = DarkRiftWriter.Create())
                {
                    messageWriter.Write(true);
                    using (Message tickMessage = Message.Create((ushort)ChessEnums.MessageTags.TimerTick, messageWriter))
                    {
                        client1.SendMessage(tickMessage, SendMode.Unreliable);
                        client2.SendMessage(tickMessage, SendMode.Unreliable);
                    }
                }
            }
            else
            {
                using (DarkRiftWriter messageWriter = DarkRiftWriter.Create())
                {
                    messageWriter.Write(false);
                    using (Message tickMessage = Message.Create((ushort)ChessEnums.MessageTags.TimerTick, messageWriter))
                    {
                        client1.SendMessage(tickMessage, SendMode.Unreliable);
                        client2.SendMessage(tickMessage, SendMode.Unreliable);
                    }
                }
            }
        }


        public void WhiteMoved()
        {
            timerWhites.Stop();
            timerBlacks.Start();
        }

        public void BlackMoved()
        {
            timerBlacks.Stop();
            timerWhites.Start();
        }
    }
}
