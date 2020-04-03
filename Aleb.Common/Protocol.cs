using System;
using System.Net;

namespace Aleb.Common {
    public static class Protocol {
        public static readonly IPAddress Localhost = new IPAddress(new byte[] {127, 0, 0, 1});
        public const int Version = 0;
    }
}
