/***************************************************************
 * ***** Defining MessageEventArgs ****************************
 * *************************************************************/

using System;

namespace NetWebSocketTest {

    public class MessageEventArgs : EventArgs {

        public byte[] RawData;

        public MessageEventArgs(byte[] rawData) {

            RawData = rawData;

        }

    }

}