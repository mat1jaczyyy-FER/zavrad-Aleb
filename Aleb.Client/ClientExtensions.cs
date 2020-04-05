using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

using Aleb.Common;

namespace Aleb.Client {
    public enum ConnectStatus {
        Success, Failed, VersionMismatch
    }

    public static class ClientExtensions {
        public static async Task<(ConnectStatus, AlebClient)> ConnectToServer(string host) {
            TcpClient tcp = null;

            try {
                tcp = new TcpClient();
                await tcp.ConnectAsync(host?? Protocol.Localhost, Protocol.Port);

            } catch {
                tcp?.Dispose();
                return (ConnectStatus.Failed, null);
            }

            AlebClient client = new AlebClient(tcp);

            if (await client.ReadMessage(i => Convert.ToInt32(i.Args[0]), -1, "Version") != Protocol.Version) {
                client.Dispose();
                return (ConnectStatus.VersionMismatch, null);
            }
            
            return (ConnectStatus.Success, client);
        }

        public static async Task<bool> Login(this AlebClient client, string name, string password) {
            if (!Validation.ValidateUsername(name)) return false;
            if (!Validation.ValidatePassword(password)) return false;

            client.Send("Login", name, password);

            return await client.ReadMessage(i => Convert.ToBoolean(i.Args[0]), false, "LoginResult");
        }

        public static async Task<List<Room>> GetRoomList(this AlebClient client) {
            client.Send("GetRoomList");

            return await client.ReadMessage(i => i.Args.Select(i => new Room(i)).ToList(), new List<Room>(), "RoomList");
        }

        public static async Task<Room> CreateRoom(this AlebClient client, string name) {
            if (!Validation.ValidateRoomName(name)) return null;

            client.Send("CreateRoom", name);

            return await client.ReadMessage(i => i.Command == "RoomCreated"? new Room(i.Args[0]) : null, null, "RoomCreated", "RoomFailed");
        }
    }
}
