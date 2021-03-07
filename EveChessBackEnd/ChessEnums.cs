using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveChessBackEnd
{
    public static class ChessEnums
    {

        public enum MessageTags
        {
            ConnectedPlayers = 0,
            PlayerReady = 1,
            SetColor = 2,
            MovePiece = 3,
            PieceEaten =4,
            TimerTick =5
        }

        public enum Colors
        {
            White = 0,
            Black = 1
        }
    }
}
