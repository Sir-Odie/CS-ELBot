// Eternal Lands Bot
// Copyright (C) 2006  Artem Makhutov
// artem@makhutov.org
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

using System;

namespace TCPClient
{
	/// <summary>
	/// description of TCPClient.
	/// </summary>
    public class TCPClient
    {
        // OnGotDisconnected
        public delegate void GotDisconnectedEventHandler(object sender, EventArgs e);
        public event GotDisconnectedEventHandler GotDisconnected;

        public void OnGotDisconnected(EventArgs e)
        {
            if (GotDisconnected != null)
                GotDisconnected(this, e);
        }

        // OnGotConnected
        public delegate void GotConnectedEventHandler(object sender, EventArgs e);
        public event GotConnectedEventHandler GotConnected;

        protected void OnGotConnected(EventArgs e)
        {
            if (GotConnected != null)
                GotConnected(this, e);
        }

        // OnGotData
        public delegate void GotDataEventHandler(object sender, GotDataEventArgs e);
        public event GotDataEventHandler GotData;

        public class GotDataEventArgs : EventArgs
        {
            public readonly byte[] DataBuffer;
            public GotDataEventArgs(byte[] Buffer)
            {
                DataBuffer = Buffer;
            }
        }

        private void OnGotData(GotDataEventArgs e)
        {
            if (GotData != null)
                GotData(this, e);
        }

        System.Threading.Thread ReaderThread = null;
        System.Net.Sockets.Socket TCPSocket = null;

        public TCPClient()
        {

        }

        public bool Connect(string Hostname, int Port)
        {
            return this.Connect(System.Net.Dns.GetHostEntry(Hostname).AddressList[0], Port);
        }
        public bool Connect(System.Net.IPAddress IPAddress, int Port)
        {
            if (this.TCPSocket != null)
            {
                if (this.TCPSocket.Connected == true)
                {
                    Console.WriteLine("socket already connected, returning true....");
                    return true;
                }
            }

            try
            {
                //new code
                if (this.ReaderThread != null)
                {
                    if (this.ReaderThread.IsAlive)
                    {
                        Console.WriteLine("Thread still alive.... Trying to kill it...");
                        this.ReaderThread.Abort();
                        this.ReaderThread.Join();
                    }
                    else
                    {
                        this.ReaderThread = null;
                    }
                    return false;
                }
                else
                {
                    Console.WriteLine("Connecting to game and starting a new reader....");
                    this.TCPSocket = null;
                    this.TCPSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                    this.TCPSocket.ReceiveTimeout = 1000;
                    this.TCPSocket.SendTimeout = 1000;
                    this.TCPSocket.Connect(new System.Net.IPEndPoint(IPAddress, Port));

                    // Start the thread to pool received data
                    this.ReaderThread = new System.Threading.Thread(this.ReaderThreadLoop);
                    this.ReaderThread.Start();
                    this.OnGotConnected(new System.EventArgs());
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.TargetSite.Name);
                System.Console.WriteLine(ex.StackTrace);
                System.Console.WriteLine(ex.Source);
                System.Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void Disconnect()
        {
            //if (this.ReaderThread != null)
            //{
            //    this.ReaderThread.Abort();
            //    this.ReaderThread.Join();
            //    this.ReaderThread = null;
            //}

            if (this.TCPSocket != null)
            {
                if (this.TCPSocket.Connected == true)
                {
                    this.TCPSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                }
                this.TCPSocket.Close();
                this.TCPSocket = null;
            }


            this.OnGotDisconnected(new System.EventArgs());
            if (this.ReaderThread != null)
            {
                if (this.ReaderThread.IsAlive)
                {
                    //this.ReaderThread.Abort();
                    this.ReaderThread.Join();
                }
                this.ReaderThread = null;
            }
        }

        public int Send(byte[] Buffer)
        {
            if (this.TCPSocket == null)
            {
                this.Disconnect();
                return 0;
            }

            if (this.TCPSocket.Connected == false)
            {
                this.Disconnect();
                return 0;
            }

            int DataSent = 0;
            int TotalDataSent = 0;
            int DataLeft = Buffer.Length;

            // Loop while we have send all data
            while (DataLeft > 0 && !(this.TCPSocket == null || this.TCPSocket.Connected == false))
            {
                if (this.TCPSocket == null)
                {
                    this.Disconnect();
                    return TotalDataSent;
                }

                if (this.TCPSocket.Connected == false)
                {
                    this.Disconnect();
                    return TotalDataSent;
                }

                DataSent = this.TCPSocket.Send(Buffer, TotalDataSent, DataLeft, System.Net.Sockets.SocketFlags.None);
                TotalDataSent += DataSent;
                DataLeft -= DataSent;
            }

            return TotalDataSent;
        }

        private void ReaderThreadLoop()
        {
            ReaderThread.Name = "TCPClient.ReaderThreadLoop";
            try
            {
                while (this.TCPSocket != null)
                {
                    if (!this.TCPSocket.Connected)
                    {
                        this.TCPSocket = null;
                        break;
                    }
                    int RxLength = 0;
                    int PacketLength = 0;
                    int BytesAvailable = 0;
                    byte[] DataBufferA = new byte[3];
                    byte[] DataBufferB = new byte[0];

                    // We need the first 3 bytes to get the packet length

                    while (RxLength < 3)
                    {
                        // Do a loop until we receive the first 3 bytes of data
                        // reader loop here here

                        while (this.TCPSocket != null)
                        {
                            if (!this.TCPSocket.Connected)
                            {
                                this.TCPSocket = null;
                                break;
                            }
                            BytesAvailable = this.TCPSocket.Available;
                            //right here here
                            if (BytesAvailable < 3)
                            {
                                System.Threading.Thread.Sleep(30);//was 20ms, increased to see if it will fix an infrequent error
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (this.TCPSocket == null)
                        {
                            break;
                        }
                        else if (!this.TCPSocket.Connected)
                        {
                            this.TCPSocket = null;
                            break;
                        }
                        RxLength += this.TCPSocket.Receive(DataBufferA, 3, System.Net.Sockets.SocketFlags.None);
                    }

                    if (this.TCPSocket == null)
                    {
                        break;
                    }
                    else if (!this.TCPSocket.Connected)
                    {
                        this.TCPSocket = null;
                        break;
                    }
                    // Get the packet length
                    PacketLength = System.BitConverter.ToUInt16(DataBufferA, 1) + 2;

                    // Check if we need to fetch more data to complete the packet
                    if (PacketLength > 3)
                    {
                        DataBufferB = new byte[PacketLength - 3];

                        while (RxLength < PacketLength)
                        {
                            if (this.TCPSocket == null)
                            {
                                break;
                            }
                            else if (!this.TCPSocket.Connected)
                            {
                                this.TCPSocket = null;
                                break;
                            }
                            // Do a loop until we receive enough data to fill the packet
                            BytesAvailable = this.TCPSocket.Available;
                            while (BytesAvailable < PacketLength - 3)
                            {
                                System.Threading.Thread.Sleep(30);
                                if (this.TCPSocket == null)
                                {
                                    break;
                                }
                                else if (!this.TCPSocket.Connected)
                                {
                                    this.TCPSocket = null;
                                    break;
                                }
                                BytesAvailable = this.TCPSocket.Available;
                            }

                            //							RxLength += this.TCPSocket.Receive(DataBufferB,PacketLength-3,System.Net.Sockets.SocketFlags.None);

                            //Check if we can get all data with one go, if not get only a part of it

                            if ((RxLength + BytesAvailable) < PacketLength)
                            {
                                if (this.TCPSocket == null)
                                {
                                    break;
                                }
                                else if (!this.TCPSocket.Connected)
                                {
                                    this.TCPSocket = null;
                                    break;
                                }
                                RxLength += this.TCPSocket.Receive(DataBufferB, RxLength - 3, BytesAvailable, System.Net.Sockets.SocketFlags.None);
                            }
                            else
                            {
                                if (this.TCPSocket == null)
                                {
                                    break;
                                }
                                else if (!this.TCPSocket.Connected)
                                {
                                    this.TCPSocket = null;
                                    break;
                                }
                                RxLength += this.TCPSocket.Receive(DataBufferB, RxLength - 3, PacketLength - RxLength, System.Net.Sockets.SocketFlags.None);
                                //RxLength += this.TCPSocket.Receive(DataBufferB, RxLength - 3, BytesAvailable, System.Net.Sockets.SocketFlags.None);
                            }
                        }
                    }
                    if (this.TCPSocket == null)
                    {
                        break;
                    }
                    else if (!this.TCPSocket.Connected)
                    {
                        break;
                    }
                    // Put the two byte arrays together
                    System.IO.MemoryStream OutputBuffer = new System.IO.MemoryStream();

                    OutputBuffer.Write(DataBufferA, 0, DataBufferA.Length);
                    OutputBuffer.Write(DataBufferB, 0, DataBufferB.Length);

                    this.OnGotData(new GotDataEventArgs(OutputBuffer.ToArray()));
                }
                Console.WriteLine("Allowing thread to die on its own, like it should!!!!");
                this.ReaderThread = null;
                //this.Disconnect();
            }
            catch (System.Threading.ThreadAbortException ex)
            {
                Console.WriteLine("Getting to thread abort");
                System.Console.WriteLine(ex.TargetSite.Name);
                System.Console.WriteLine(ex.StackTrace);
                System.Console.WriteLine(ex.Source);
                System.Console.WriteLine(ex.Message);
                this.ReaderThread = null;
            }

            catch (System.Net.Sockets.SocketException ex)
            {
                System.Console.WriteLine(ex.TargetSite.Name);
                System.Console.WriteLine(ex.StackTrace);
                System.Console.WriteLine(ex.Source);
                System.Console.WriteLine(ex.Message);
                this.ReaderThread = null;
                this.Disconnect();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.TargetSite.Name);
                System.Console.WriteLine(ex.StackTrace);
                System.Console.WriteLine(ex.Source);
                System.Console.WriteLine(ex.Message);
                this.ReaderThread = null;
                //this.Disconnect();
            }
        }
    }
}
