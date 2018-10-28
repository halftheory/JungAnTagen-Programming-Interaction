/* adapted from OSC.cs in http://thomasfredericks.github.io/UnityOSC/ */
using System;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;

/// <summary>
/// UdpPacket provides packetIO over UDP
/// </summary>
public class UDPPacketIO {
	private UdpClient Sender;
	private UdpClient Receiver;
	private bool socketsOpen;
	private string remoteHostName;
	private int remotePort;
	private int localPort;
	
	public UDPPacketIO(string hostIP, int remotePort, int localPort){
		RemoteHostName = hostIP;
		RemotePort = remotePort;
		LocalPort = localPort;
		socketsOpen = false;
	}
	
	~UDPPacketIO() {
		// latest time for this socket to be closed
		if (IsOpen()) {
			Debug.Log("closing udpclient listener on port " + localPort);
			Close();
		}
	}
	
	/// <summary>
	/// Open a UDP socket and create a UDP sender.
	/// </summary>
	/// <returns>True on success, false on failure.</returns>
	public bool Open() {
		try {
			Sender = new UdpClient();
			Debug.Log("Opening OSC listener on port " + localPort);
			IPEndPoint listenerIp = new IPEndPoint(IPAddress.Any, localPort);
			Receiver = new UdpClient(listenerIp);
			socketsOpen = true;
			return true;
		}
		catch (Exception e) {
			Debug.LogWarning("cannot open udp client interface at port "+localPort);
			Debug.LogWarning(e);
		}
		return false;
	}
	
	/// <summary>
	/// Close the socket currently listening, and destroy the UDP sender device.
	/// </summary>
	public void Close() {    
		if(Sender != null) {
			Sender.Close();
		}		
		if (Receiver != null) {
			Receiver.Close();
		}
		Receiver = null;
		socketsOpen = false;		
	}
	
	public void OnDisable() {
		Close();
	}
	
	/// <summary>
	/// Query the open state of the UDP socket.
	/// </summary>
	/// <returns>True if open, false if closed.</returns>
	public bool IsOpen() {
		return socketsOpen;
	}
	
	/// <summary>
	/// Send a packet of bytes out via UDP.
	/// </summary>
	/// <param name="packet">The packet of bytes to be sent.</param>
	/// <param name="length">The length of the packet of bytes to be sent.</param>
	public void SendPacket(byte[] packet, int length) {   
		if (!IsOpen()) {
			Open();
		}
		if (!IsOpen()) {
			return;
		}
		Sender.Send(packet, length, remoteHostName, remotePort);
	}
	
	/// <summary>
	/// Receive a packet of bytes over UDP.
	/// </summary>
	/// <param name="buffer">The buffer to be read into.</param>
	/// <returns>The number of bytes read, or 0 on failure.</returns>
	public int ReceivePacket(byte[] buffer) {
		if (!IsOpen()) {
			Open();
		}
		if (!IsOpen()) {
			return 0;
		}
		IPEndPoint iep = new IPEndPoint(IPAddress.Any, localPort);
		byte[] incoming = Receiver.Receive( ref iep );
		int count = Math.Min(buffer.Length, incoming.Length);
		System.Array.Copy(incoming, buffer, count);
		return count;		
	}
	
	/// <summary>
	/// The address of the board that you're sending to.
	/// </summary>
	public string RemoteHostName {
		get { 
			return remoteHostName; 
		}
		set { 
			remoteHostName = value; 
		}
	}
	
	/// <summary>
	/// The remote port that you're sending to.
	/// </summary>
	public int RemotePort {
		get { 
			return remotePort; 
		}
		set { 
			remotePort = value; 
		}
	}
	
	/// <summary>
	/// The local port you're listening on.
	/// </summary>
	public int LocalPort {
		get {
			return localPort; 
		}
		set { 
			localPort = value; 
		}
	}
} //public class UDPPacketIO

/// <summary>
/// The OscMessage class is a data structure that represents
/// an OSC address and an arbitrary number of values to be sent to that address.
/// </summary>
public class OscMessage {
	/// <summary>
	/// The OSC address of the message as a string.
	/// </summary>
	public string address;
	/// <summary>
	/// The list of values to be delivered to the Address.
	/// </summary>
	public ArrayList values;

	public OscMessage() {
	  values = new ArrayList();
	}

	public override string ToString() {
		StringBuilder s = new StringBuilder();
		s.Append(address);
		foreach( object o in values ) {
			s.Append(" ");
			s.Append(o.ToString());
		}
		return s.ToString();
	}

	//halftheory - new function
	public string GetString() {
		StringBuilder s = new StringBuilder();
		int i = 0;
		foreach(object o in values) {
			if (i > 0) {
				s.Append(" ");
			}
			s.Append(o.ToString());
			i++;
		}
		return s.ToString();
	}

	//halftheory - new function
	public bool GetBool(int index) {
		if (values[index].GetType() == typeof(int) ) {
	        int data = (int)values[index];
	        if (Double.IsNaN(data)) return false;
	        return data > 0;
	    }
	    else if (values[index].GetType() == typeof(float)) {
	        float data = (float)values[index];
	        if (Double.IsNaN(data)) return false;
	        return data > 0f;
	    }
	    else {
			Debug.Log("Wrong type");
			return false;
		}
	}

	public int GetInt(int index) {
		if (values[index].GetType() == typeof(int) ) {
	        int data = (int)values[index];
	        if (Double.IsNaN(data)) return 0;
	        return data;
	    }
	    else if (values[index].GetType() == typeof(float)) {
	        int data = (int)((float)values[index]);
	        if (Double.IsNaN(data)) return 0;
	        return data;
	    }
	    else {
			Debug.Log("Wrong type");
			return 0;
		}
	}

	public float GetFloat(int index) {
		if (values[index].GetType() == typeof(int)) {
			float data = (int)values[index];
	        if (Double.IsNaN(data)) return 0f;
	        return data;
		}
		else if (values[index].GetType() == typeof(float)) {
	        float data = (float)values[index];
	        if (Double.IsNaN(data)) return 0f;
	        return data;
		}
		else {
			Debug.Log("Wrong type");
			return 0f;
		}
	}
} //public class OscMessage

public delegate void OscMessageHandler( OscMessage oscM );

public class OSC : MonoBehaviour {
	public int inPort  = 8888;
	public string outIP = "127.0.0.1";
	public int outPort  = 9999;

	public UDPPacketIO OscPacketIO;
	Thread ReadThread;
	private bool ReaderRunning;
	private OscMessageHandler AllMessageHandler;

	Hashtable AddressTable;
	ArrayList messagesReceived;
	private object ReadThreadLock = new object();
	byte[] buffer;
	private int byteLength = 8000;

	bool paused = false;

	#if UNITY_EDITOR    
    private void HandleOnPlayModeChanged(UnityEditor.PlayModeStateChange state) {
		//FIX FOR UNITY POST 2017
		// This method is run whenever the playmode state is changed.
		paused = UnityEditor.EditorApplication.isPaused;
	}
	#endif

	private bool useRepeater = true;

    void Awake() {
		OscPacketIO = new UDPPacketIO(outIP, outPort, inPort);
		AddressTable = new Hashtable();
		messagesReceived = new ArrayList();
		buffer = new byte[byteLength];

		ReadThread = new Thread(Read);
		ReaderRunning = true;
		ReadThread.IsBackground = true;      
		ReadThread.Start();

		#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
		#endif

        //halftheory - Update is disabled, use InvokeRepeating instead
        if (useRepeater) {
			float repeatInterval = 1f / 60f;
			InvokeRepeating("UpdateRepeater", repeatInterval, repeatInterval);
		}
    }

	void OnDestroy() {
		Close();
	}

	/// <summary>
	/// Set the method to call back on when a message with the specified
	/// address is received.  The method needs to have the OscMessageHandler signature - i.e. 
	/// void amh( OscMessage oscM )
	/// </summary>
	/// <param name="key">Address string to be matched</param>   
	/// <param name="ah">The method to call back on.</param>   
	public void SetAddressHandler(string key, OscMessageHandler ah) {
		ArrayList al = (ArrayList)Hashtable.Synchronized(AddressTable)[key];
		if ( al == null) {
			al = new ArrayList();
			al.Add(ah);
			Hashtable.Synchronized(AddressTable).Add(key, al);
		}
		else {
			al.Add(ah);
		}
	}

	void OnApplicationPause(bool pauseStatus) {
		#if !UNITY_EDITOR
		paused = pauseStatus;
		print ("Application paused : " + pauseStatus);
		#endif
	}

	void Update() {
		if (useRepeater) {
			return;
		}
		if (messagesReceived != null && messagesReceived.Count > 0) { //halftheory - null check
			//Debug.Log("received " + messagesReceived.Count + " messages");
			lock(ReadThreadLock) {
				foreach (OscMessage om in messagesReceived) {
					if (AllMessageHandler != null) {
						AllMessageHandler(om);
					}
					ArrayList al = (ArrayList)Hashtable.Synchronized(AddressTable)[om.address];
					if ( al != null) {
						foreach (OscMessageHandler h in al) {
							h(om);
						}
					}
				}
				messagesReceived.Clear();
			}
		}
	}

	void UpdateRepeater() {
		if (messagesReceived != null && messagesReceived.Count > 0) {
			lock(ReadThreadLock) {
				foreach (OscMessage om in messagesReceived) {
					if (AllMessageHandler != null) {
						AllMessageHandler(om);
					}
					else {
						ArrayList al = (ArrayList)Hashtable.Synchronized(AddressTable)[om.address];
						if ( al != null) {
							foreach (OscMessageHandler h in al) {
								h(om);
							}
						}
					}
				}
				messagesReceived.Clear();
			}
		}
	}
	
    /// <summary>
    /// Make sure the PacketExchange is closed.
    /// </summary>
    public void Close() {
        if (ReaderRunning) {
            ReaderRunning = false;
            ReadThread.Abort();
        }
        if (OscPacketIO != null && OscPacketIO.IsOpen()) {
            OscPacketIO.Close();
            OscPacketIO = null;
			print("Closed OSC listener");
        }
    }

	/// <summary>
	/// Read Thread.  Loops waiting for packets.  When a packet is received, it is 
	/// dispatched to any waiting All Message Handler.  Also, the address is looked up and
	/// any matching handler is called.
	/// </summary>
	private void Read() {
		try {
			while (ReaderRunning) {
				int length = OscPacketIO.ReceivePacket(buffer);
				if (length > 0) {
					lock(ReadThreadLock) {
						if ( paused == false ) {
							ArrayList newMessages = OSC.PacketToOscMessages(buffer, length);
							messagesReceived.AddRange(newMessages);
						}
					}
				}
				else {
					Thread.Sleep(5);
				}
			}
		}
		catch (Exception e) {
			Debug.Log("ThreadAbortException"+e);
		}
		finally {
		}
	}

    /// <summary>
    /// Send an individual OSC message.  Internally takes the OscMessage object and 
    /// serializes it into a byte[] suitable for sending to the PacketIO.
    /// </summary>
    /// <param name="oscMessage">The OSC Message to send.</param>   
    public void Send( OscMessage oscMessage ) {
      byte[] packet = new byte[byteLength];
      int length = OSC.OscMessageToPacket( oscMessage, packet, byteLength );
      OscPacketIO.SendPacket( packet, length);
    }

    /// <summary>
    /// Sends a list of OSC Messages.  Internally takes the OscMessage objects and 
    /// serializes them into a byte[] suitable for sending to the PacketExchange.
    /// </summary>
    /// <param name="oms">The OSC Message to send.</param>   
    public void Send(ArrayList oms) {
      byte[] packet = new byte[byteLength];
		int length = OSC.OscMessagesToPacket(oms, packet, byteLength);
      OscPacketIO.SendPacket(packet, length);
    }

    /// <summary>
    /// Set the method to call back on when any message is received.
    /// The method needs to have the OscMessageHandler signature - i.e. void amh( OscMessage oscM )
    /// </summary>
    /// <param name="amh">The method to call back on.</param>   
    public void SetAllMessageHandler(OscMessageHandler amh) {
      AllMessageHandler = amh;
    }    

    /// <summary>
    /// Creates an OscMessage from a string - extracts the address and determines each of the values. 
    /// </summary>
    /// <param name="message">The string to be turned into an OscMessage</param>
    /// <returns>The OscMessage.</returns>
    public static OscMessage StringToOscMessage(string message) {
      OscMessage oM = new OscMessage();
      Console.WriteLine("Splitting " + message);
      string[] ss = message.Split(new char[] { ' ' });
      IEnumerator sE = ss.GetEnumerator();
      if (sE.MoveNext())
        oM.address = (string)sE.Current;
      while ( sE.MoveNext() )
      {
        string s = (string)sE.Current;
        // Console.WriteLine("  <" + s + ">");
        if (s.StartsWith("\""))
        {
          StringBuilder quoted = new StringBuilder();
          bool looped = false;
          if (s.Length > 1)
            quoted.Append(s.Substring(1));
          else
            looped = true;
          while (sE.MoveNext())
          {
            string a = (string)sE.Current;
            // Console.WriteLine("    q:<" + a + ">");
            if (looped)
              quoted.Append(" ");
            if (a.EndsWith("\""))
            {
              quoted.Append(a.Substring(0, a.Length - 1));
              break;
            }
            else
            {
              if (a.Length == 0)
                quoted.Append(" ");
              else
                quoted.Append(a);
            }
            looped = true;
          }
          oM.values.Add(quoted.ToString());
        }
        else
        {
          if (s.Length > 0)
          {
            try
            {
              int i = int.Parse(s);
              // Console.WriteLine("  i:" + i);
              oM.values.Add(i);
            }
            catch
            {
              try
              {
                float f = float.Parse(s);
                // Console.WriteLine("  f:" + f);
                oM.values.Add(f);
              }
              catch
              {
                // Console.WriteLine("  s:" + s);
                oM.values.Add(s);
              }
            }

          }
        }
      }
      return oM;
    }

    /// <summary>
    /// Takes a packet (byte[]) and turns it into a list of OscMessages.
    /// </summary>
    /// <param name="packet">The packet to be parsed.</param>
    /// <param name="length">The length of the packet.</param>
    /// <returns>An ArrayList of OscMessages.</returns>
    public static ArrayList PacketToOscMessages(byte[] packet, int length) {
      ArrayList messages = new ArrayList();
      ExtractMessages(messages, packet, 0, length);
      return messages;
    }

    /// <summary>
    /// Puts an array of OscMessages into a packet (byte[]).
    /// </summary>
    /// <param name="messages">An ArrayList of OscMessages.</param>
    /// <param name="packet">An array of bytes to be populated with the OscMessages.</param>
    /// <param name="length">The size of the array of bytes.</param>
    /// <returns>The length of the packet</returns>
    public static int OscMessagesToPacket(ArrayList messages, byte[] packet, int length) {
      int index = 0;
      if (messages.Count == 1)
        index = OscMessageToPacket((OscMessage)messages[0], packet, 0, length);
      else
      {
        // Write the first bundle bit
        index = InsertString("#bundle", packet, index, length);
        // Write a null timestamp (another 8bytes)
        int c = 8;
        while (( c-- )>0)
          packet[index++]++;
        // Now, put each message preceded by it's length
        foreach (OscMessage oscM in messages)
        {
          int lengthIndex = index;
          index += 4;
          int packetStart = index;
          index = OscMessageToPacket(oscM, packet, index, length);
          int packetSize = index - packetStart;
          packet[lengthIndex++] = (byte)((packetSize >> 24) & 0xFF);
          packet[lengthIndex++] = (byte)((packetSize >> 16) & 0xFF);
          packet[lengthIndex++] = (byte)((packetSize >> 8) & 0xFF);
          packet[lengthIndex++] = (byte)((packetSize) & 0xFF);
        }
      }
      return index;
    }

    /// <summary>
    /// Creates a packet (an array of bytes) from a single OscMessage.
    /// </summary>
    /// <remarks>A convenience method, not requiring a start index.</remarks>
    /// <param name="oscM">The OscMessage to be returned as a packet.</param>
    /// <param name="packet">The packet to be populated with the OscMessage.</param>
    /// <param name="length">The usable size of the array of bytes.</param>
    /// <returns>The length of the packet</returns>
    public static int OscMessageToPacket(OscMessage oscM, byte[] packet, int length) {
      return OscMessageToPacket(oscM, packet, 0, length);
    }

    /// <summary>
    /// Creates an array of bytes from a single OscMessage.  Used internally.
    /// </summary>
    /// <remarks>Can specify where in the array of bytes the OscMessage should be put.</remarks>
    /// <param name="oscM">The OscMessage to be turned into an array of bytes.</param>
    /// <param name="packet">The array of bytes to be populated with the OscMessage.</param>
    /// <param name="start">The start index in the packet where the OscMessage should be put.</param>
    /// <param name="length">The length of the array of bytes.</param>
    /// <returns>The index into the packet after the last OscMessage.</returns>
    private static int OscMessageToPacket(OscMessage oscM, byte[] packet, int start, int length) {
      int index = start;
      index = InsertString(oscM.address, packet, index, length);
      //if (oscM.values.Count > 0)
      {
        StringBuilder tag = new StringBuilder();
        tag.Append(",");
        int tagIndex = index;
        index += PadSize(2 + oscM.values.Count);

        foreach (object o in oscM.values)
        {
          if (o is int)
          {
            int i = (int)o;
            tag.Append("i");
            packet[index++] = (byte)((i >> 24) & 0xFF);
            packet[index++] = (byte)((i >> 16) & 0xFF);
            packet[index++] = (byte)((i >> 8) & 0xFF);
            packet[index++] = (byte)((i) & 0xFF);
          }
          else
          {
            if (o is float)
            {
              float f = (float)o;
              tag.Append("f");
              byte[] buffer = new byte[4];
              MemoryStream ms = new MemoryStream(buffer);
              BinaryWriter bw = new BinaryWriter(ms);
              bw.Write(f);
              packet[index++] = buffer[3];
              packet[index++] = buffer[2];
              packet[index++] = buffer[1];
              packet[index++] = buffer[0];
            }
            else
            {
              if (o is string)
              {
                tag.Append("s");
                index = InsertString(o.ToString(), packet, index, length);
              }
              else
              {
                tag.Append("?");
              }
            }
          }
        }
        InsertString(tag.ToString(), packet, tagIndex, length);
      }
      return index;
    }

    /// <summary>
    /// Receive a raw packet of bytes and extract OscMessages from it.  Used internally.
    /// </summary>
    /// <remarks>The packet may contain a OSC message or a bundle of messages.</remarks>
    /// <param name="messages">An ArrayList to be populated with the OscMessages.</param>
    /// <param name="packet">The packet of bytes to be parsed.</param>
    /// <param name="start">The index of where to start looking in the packet.</param>
    /// <param name="length">The length of the packet.</param>
    /// <returns>The index after the last OscMessage read.</returns>
    private static int ExtractMessages(ArrayList messages, byte[] packet, int start, int length) {
      int index = start;
      switch ( (char)packet[ start ] )
      {
        case '/':
          index = ExtractMessage( messages, packet, index, length );
          break;
        case '#':
          string bundleString = ExtractString(packet, start, length);
          if ( bundleString == "#bundle" )
          {
            // skip the "bundle" and the timestamp
            index+=16;
            while ( index < length )
            {
              int messageSize = ( packet[index++] << 24 ) + ( packet[index++] << 16 ) + ( packet[index++] << 8 ) + packet[index++];
              /*int newIndex = */ExtractMessages( messages, packet, index, length ); 
              index += messageSize;
            }            
          }
          break;
      }
      return index;
    }

    /// <summary>
    /// Extracts a messages from a packet.
    /// </summary>
    /// <param name="messages">An ArrayList to be populated with the OscMessage.</param>
    /// <param name="packet">The packet of bytes to be parsed.</param>
    /// <param name="start">The index of where to start looking in the packet.</param>
    /// <param name="length">The length of the packet.</param>
    /// <returns>The index after the OscMessage is read.</returns>
    private static int ExtractMessage(ArrayList messages, byte[] packet, int start, int length) {
      OscMessage oscM = new OscMessage();
      oscM.address = ExtractString(packet, start, length);
      int index = start + PadSize(oscM.address.Length+1);
      string typeTag = ExtractString(packet, index, length);
      index += PadSize(typeTag.Length + 1);
      //oscM.values.Add(typeTag);
      foreach (char c in typeTag)
      {
        switch (c)
        {
          case ',':
            break;
          case 's':
            {
              string s = ExtractString(packet, index, length);
              index += PadSize(s.Length + 1);
              oscM.values.Add(s);
              break;
            }
          case 'i':
            {
              int i = ( packet[index++] << 24 ) + ( packet[index++] << 16 ) + ( packet[index++] << 8 ) + packet[index++];
              oscM.values.Add(i);
              break;
            }
          case 'f':
            {
              byte[] buffer = new byte[4];
              buffer[3] = packet[index++];
              buffer[2] = packet[index++];
              buffer[1] = packet[index++];
              buffer[0] = packet[index++];
              MemoryStream ms = new MemoryStream(buffer);
              BinaryReader br = new BinaryReader(ms);
              float f = br.ReadSingle();
              oscM.values.Add(f);
              break;
            }
        }
      }
      messages.Add( oscM );
      return index;
    }

    /// <summary>
    /// Removes a string from a packet.  Used internally.
    /// </summary>
    /// <param name="packet">The packet of bytes to be parsed.</param>
    /// <param name="start">The index of where to start looking in the packet.</param>
    /// <param name="length">The length of the packet.</param>
    /// <returns>The string</returns>
    private static string ExtractString(byte[] packet, int start, int length) {
      StringBuilder sb = new StringBuilder();
      int index = start;
      while (packet[index] != 0 && index < length) {
        sb.Append((char)packet[index++]);
      }
      return sb.ToString();
    }

    private static string Dump(byte[] packet, int start, int length) {
      StringBuilder sb = new StringBuilder();
      int index = start;
      while (index < length) {
        sb.Append(packet[index++]+"|");
      }
      return sb.ToString();
    }

    /// <summary>
    /// Inserts a string, correctly padded into a packet.  Used internally.
    /// </summary>
    /// <param name="string">The string to be inserted</param>
    /// <param name="packet">The packet of bytes to be parsed.</param>
    /// <param name="start">The index of where to start looking in the packet.</param>
    /// <param name="length">The length of the packet.</param>
    /// <returns>An index to the next byte in the packet after the padded string.</returns>
    private static int InsertString(string s, byte[] packet, int start, int length) {
      int index = start;
      foreach (char c in s)
      {
        packet[index++] = (byte)c;
        if (index == length)
          return index;
      }
      packet[index++] = 0;
      int pad = (s.Length+1) % 4;
      if (pad != 0)
      {
        pad = 4 - pad;
        while (pad-- > 0)
          packet[index++] = 0;
      }
      return index;
    }

    /// <summary>
    /// Takes a length and returns what it would be if padded to the nearest 4 bytes.
    /// </summary>
    /// <param name="rawSize">Original size</param>
    /// <returns>padded size</returns>
    private static int PadSize(int rawSize) {
      int pad = rawSize % 4;
      if (pad == 0)
        return rawSize;
      else
        return rawSize + (4 - pad);
    }
} //public class OSC