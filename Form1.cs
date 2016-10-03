using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace ChatApp
{
    public partial class Form1 : Form
    {
        Socket sck;
        EndPoint epLocal, epRemote;
        bool connected = false;
        //used for receiving/sending messages
        byte[] buffer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //set up socket
            //sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //get user ID;
            textLocalIp.Text = GetLocalIP();
            textRemoteIp.Text = GetLocalIP();
            buttonSend.Enabled = false;
        }

        private string GetLocalIP()
        {
            IPHostEntry host;

            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return "127.0.0.1";
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            //bind the socket
            if (connected == false)
            {
            try{
                sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIp.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);
                //Connecting to Remote IP
                epRemote = new IPEndPoint(IPAddress.Parse(textRemoteIp.Text), Convert.ToInt32(textRemotePort.Text));
                sck.Connect(epRemote);
                //Listen for specific port:
                buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                //disable connect button until new IP or Host is entered.
                textBoxState(false);
                //set the IPs and disable the button
                buttonConnect.Text = "End";
                buttonSend.Enabled = true;
                connected = true;
                }
                catch(Exception ex)
                {
                   MessageBox.Show(ex.ToString());
                }
            }
            else
            {
               // sck.EndConnect();
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                byte[] disconnectMsg = new byte[1500];
                disconnectMsg = aEncoding.GetBytes("Friend has disconnected");
                sck.Send(disconnectMsg);
                sck.Shutdown(SocketShutdown.Receive);
                sck.Close();

                textBoxState(true);
                buttonConnect.Text = "Connect";
                buttonSend.Enabled = false;
                connected = false;
                listMessage.Items.Add("You have disconnected from the chat.");
                //sck.EndReceiveFrom(buffer , ref epRemote);
            }
        }

        //recieves messages from the friend
        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                if (sck.Connected == true)
                {
                    bool cont = false;
                    byte[] receivedData = new byte[1500];
                    receivedData = (byte[])aResult.AsyncState;
                    
                    foreach (byte i in receivedData)
                    {
                        if (i != 0)
                        {
                            cont = true;
                        }
                    }

                    if (cont == true)
                    {
                        //Convert byte[] to string
                        ASCIIEncoding aEncoding = new ASCIIEncoding();
                        string receivedMessage = aEncoding.GetString(receivedData);
                        //Add this message into the ListBox
                        //sck.EndReceiveFrom(aResult, ref epRemote);
                        listMessage.Items.Add("Friend: " + receivedMessage);
                    }
                        buffer = new byte[1500];
                        sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            //convert string message to byte[]
            ASCIIEncoding aEncoding = new ASCIIEncoding();
            byte[] sendingMessage = new byte[1500];
            sendingMessage = aEncoding.GetBytes(textMessage.Text);
            //Send the Encoded message
            sck.Send(sendingMessage);
            //add message to list Message
            listMessage.Items.Add("Me: " + textMessage.Text);
            textMessage.Text = "";

        }

        private void textBoxState(bool state)
        {
            textLocalIp.Enabled = state;
            textLocalPort.Enabled = state;
            textRemoteIp.Enabled = state;
            textRemotePort.Enabled = state;
        }

    }
}
