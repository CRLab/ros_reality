using System;

//not useful now, safed for later use


namespace NetWebSocketTest {

    public class MessageEventArgs : EventArgs {

        public byte[] RawData;

        public MessageEventArgs(byte[] rawData) {

            RawData = rawData;

        }

    }

}