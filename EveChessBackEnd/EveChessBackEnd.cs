using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;

namespace EveChessBackEnd
{
    public class ChessBackEnd : Plugin
    {
        public override bool ThreadSafe => false;

        public override Version Version => new Version(1, 0, 0);


        private List<ushort> readyPlayers = new List<ushort>();
        private List<ushort> ingamePlayers = new List<ushort>();


        public ChessBackEnd(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
            
        }


        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            SendAllPlayers();
            e.Client.MessageReceived += ClientMessageReceived;
        }


        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            SendAllPlayers();

            if (readyPlayers.Contains(e.Client.ID))
                readyPlayers.Remove(e.Client.ID);

            if (ingamePlayers.Contains(e.Client.ID))
                ingamePlayers.Remove(e.Client.ID);
        }

        private void SendAllPlayers()
        {

            foreach (IClient clienT in ClientManager.GetAllClients())
            {
                using (DarkRiftWriter listOfPlayersWriter = DarkRiftWriter.Create())
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                    {
                        listOfPlayersWriter.Write(client.ID);
                    }
                    using (Message listOfPlayersMessage = Message.Create((ushort)ChessEnums.MessageTags.ConnectedPlayers, listOfPlayersWriter))
                    {  
                        clienT.SendMessage(listOfPlayersMessage, SendMode.Reliable);
                    }
                }
            }
        }


        private void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                if (message.Tag == (ushort)ChessEnums.MessageTags.PlayerReady)
                {
                    if (reader.Length % 2 != 0)
                    {
                        Logger.Error("Received malformed ready packet.");
                        return;
                    }
                    ushort id = reader.ReadUInt16();
                    Logger.Log("Player Ready: " + id, LogType.Info);
                    readyPlayers.Add(id);
                    if (readyPlayers.Count == 2)
                    {
                        DeliverColorsAndStartGame();
                    }
                }
                else if (message.Tag == (ushort)ChessEnums.MessageTags.MovePiece)
                {
                    char originColumn = reader.ReadChar();
                    char originRow = reader.ReadChar();
                    char targetColumn = reader.ReadChar();
                    char targetRow = reader.ReadChar();
                    SendMovementToClient(originColumn,originRow,targetColumn,targetRow,ClientManager.GetClient(ingamePlayers.Find(x=> x != e.Client.ID)).ID);
                }
                else if (message.Tag == (ushort)ChessEnums.MessageTags.PieceEaten)
                {
                    char column = reader.ReadChar();
                    char row = reader.ReadChar();
                    Logger.Log("Eaten: " + column + row, LogType.Info);
                    SendEatenPieceToClient(column, row, ClientManager.GetClient(ingamePlayers.Find(x => x != e.Client.ID)).ID);
                }

            }
        }


        private void DeliverColorsAndStartGame()
        {
            Random random = new Random();
            if (random.Next(0, 1) == 0)
            {
                AssignColorToPlayer(ChessEnums.Colors.White,readyPlayers[0]);
                AssignColorToPlayer(ChessEnums.Colors.Black, readyPlayers[1]);
            }
            else
            {
                AssignColorToPlayer(ChessEnums.Colors.Black, readyPlayers[0]);
                AssignColorToPlayer(ChessEnums.Colors.White, readyPlayers[1]);
            }
            ingamePlayers.Add(readyPlayers[0]);
            ingamePlayers.Add(readyPlayers[1]);
            readyPlayers.Clear();
        }


        private void AssignColorToPlayer(ChessEnums.Colors color, ushort id)
        {
            using (DarkRiftWriter messageWriter = DarkRiftWriter.Create())
            {
               
                messageWriter.Write(color == ChessEnums.Colors.White ? true:false);
                using (Message colorMessage = Message.Create((ushort)ChessEnums.MessageTags.SetColor, messageWriter))
                {
                    ClientManager.GetClient(id).SendMessage(colorMessage, SendMode.Reliable);
                }
            }
        }

        private void SendMovementToClient(char originColumn, char originRow, char targetColumn, char targetRow, ushort clientId)
        {
            using (DarkRiftWriter messageWriter = DarkRiftWriter.Create())
            {

                messageWriter.Write(originColumn);
                messageWriter.Write(originRow);
                messageWriter.Write(targetColumn);
                messageWriter.Write(targetRow);
                using (Message movementMessage = Message.Create((ushort)ChessEnums.MessageTags.MovePiece, messageWriter))
                {
                    ClientManager.GetClient(clientId).SendMessage(movementMessage, SendMode.Reliable);
                }
            }
        }

        private void SendEatenPieceToClient(char column, char row, ushort clientId)
        {
            using (DarkRiftWriter messageWriter = DarkRiftWriter.Create())
            {

                messageWriter.Write(column);
                messageWriter.Write(row);

                using (Message movementMessage = Message.Create((ushort)ChessEnums.MessageTags.PieceEaten, messageWriter))
                {
                    ClientManager.GetClient(clientId).SendMessage(movementMessage, SendMode.Reliable);
                }
            }
        }



    }
}
