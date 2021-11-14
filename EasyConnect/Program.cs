using System;
using SimpleWifi;
using NativeWifi;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace EasyConnect
{
    class wifiSo
    {
        public WIFISSID ssid;               //wifi ssid
        public string key;                 //wifi密码
        public List<WIFISSID> ssids = new List<WIFISSID>();

        public wifiSo()
        {
            ssids.Clear();
        }

        public wifiSo(WIFISSID ssid, string key)
        {
            ssids.Clear();
            this.ssid = ssid;
            this.key = key;
        }

        //寻找当前连接的网络：
        public static string GetCurrentConnection()
        {
            WlanClient client = new WlanClient();
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {
                    if (wlanIface.InterfaceState == Wlan.WlanInterfaceState.Connected && wlanIface.CurrentConnection.isState == Wlan.WlanInterfaceState.Connected)
                    {
                        return wlanIface.CurrentConnection.profileName;
                    }
                }
            }

            return string.Empty;
        }
        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.UTF8.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }
        /// <summary>
        /// 枚举所有无线设备接收到的SSID
        /// </summary>
        public void ScanSSID()
        {
            WlanClient client = new WlanClient();
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                // Lists all networks with WEP security
                Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {
                    WIFISSID targetSSID = new WIFISSID();

                    targetSSID.wlanInterface = wlanIface;
                    targetSSID.wlanSignalQuality = (int)network.wlanSignalQuality;
                    targetSSID.SSID = GetStringForSSID(network.dot11Ssid);
                    //targetSSID.SSID = Encoding.Default.GetString(network.dot11Ssid.SSID, 0, (int)network.dot11Ssid.SSIDLength);
                    targetSSID.dot11DefaultAuthAlgorithm = network.dot11DefaultAuthAlgorithm.ToString();
                    targetSSID.dot11DefaultCipherAlgorithm = network.dot11DefaultCipherAlgorithm.ToString();
                    ssids.Add(targetSSID);
                }
            }
        }

        // 字符串转Hex
        public static string StringToHex(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.Default.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString().ToUpper());

        }

        // 连接到无线网络
        public void ConnectToSSID()
        {
            try
            {
                String auth = string.Empty;
                String cipher = string.Empty;
                bool isNoKey = false;
                String keytype = string.Empty;
                //Console.WriteLine("》》》《《" + ssid.dot11DefaultAuthAlgorithm + "》》对比《《" + "Wlan.Dot11AuthAlgorithm.RSNA_PSK》》");
                switch (ssid.dot11DefaultAuthAlgorithm)
                {
                    case "IEEE80211_Open":
                        auth = "open"; break;
                    case "RSNA":
                        auth = "WPA2PSK"; break;
                    case "RSNA_PSK":
                        //Console.WriteLine("电子设计wifi：》》》");
                        auth = "WPA2PSK"; break;
                    case "WPA":
                        auth = "WPAPSK"; break;
                    case "WPA_None":
                        auth = "WPAPSK"; break;
                    case "WPA_PSK":
                        auth = "WPAPSK"; break;
                }
                switch (ssid.dot11DefaultCipherAlgorithm)
                {
                    case "CCMP":
                        cipher = "AES";
                        keytype = "passPhrase";
                        break;
                    case "TKIP":
                        cipher = "TKIP";
                        keytype = "passPhrase";
                        break;
                    case "None":
                        cipher = "none"; keytype = "";
                        isNoKey = true;
                        break;
                    case "WWEP":
                        cipher = "WEP";
                        keytype = "networkKey";
                        break;
                    case "WEP40":
                        cipher = "WEP";
                        keytype = "networkKey";
                        break;
                    case "WEP104":
                        cipher = "WEP";
                        keytype = "networkKey";
                        break;
                }

                if (isNoKey && !string.IsNullOrEmpty(key))
                {

                    Console.WriteLine(">>>>>>>>>>>>>>>>>无法连接网络！");
                    return;
                }
                else if (!isNoKey && string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("无法连接网络！");
                    return;
                }
                else
                {
                    //string profileName = ssid.profileNames; // this is also the SSID 
                    string profileName = ssid.SSID;
                    string mac = StringToHex(profileName);
                    string profileXml = string.Empty;
                    if (!string.IsNullOrEmpty(key))
                    {
                        profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>auto</connectionMode><autoSwitch>false</autoSwitch><MSM><security><authEncryption><authentication>{2}</authentication><encryption>{3}</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>{4}</keyType><protected>false</protected><keyMaterial>{5}</keyMaterial></sharedKey><keyIndex>0</keyIndex></security></MSM></WLANProfile>",
                            profileName, mac, auth, cipher, keytype, key);
                    }
                    else
                    {
                        profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>auto</connectionMode><autoSwitch>false</autoSwitch><MSM><security><authEncryption><authentication>{2}</authentication><encryption>{3}</encryption><useOneX>false</useOneX></authEncryption></security></MSM></WLANProfile>",
                            profileName, mac, auth, cipher, keytype);
                    }

                    ssid.wlanInterface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);

                    bool success = ssid.wlanInterface.ConnectSynchronously(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName, 15000);
                    if (!success)
                    {
                        Console.WriteLine("");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }
        //当连接的连接状态进行通知 面是简单的通知事件的实现，根据通知的内容在界面上显示提示信息：
        private void WlanInterface_WlanConnectionNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            try
            {
                if (notifyData.notificationSource == Wlan.WlanNotificationSource.ACM)
                {
                    int notificationCode = (int)notifyData.NotificationCode;
                    switch (notificationCode)
                    {
                        case (int)Wlan.WlanNotificationCodeAcm.ConnectionStart:

                            Console.WriteLine("开始连接无线网络.......");
                            break;
                        case (int)Wlan.WlanNotificationCodeAcm.ConnectionComplete:

                            break;
                        case (int)Wlan.WlanNotificationCodeAcm.Disconnecting:

                            Console.WriteLine("正在断开无线网络连接.......");
                            break;
                        case (int)Wlan.WlanNotificationCodeAcm.Disconnected:
                            Console.WriteLine("已经断开无线网络连接.......");
                            break;
                    }
                }
                //}));
            }
            catch (Exception e)
            {
                //Loger.WriteLog(e.Message);
            }
        }
    }
    class MyWifi
    {
        public List<WIFISSID> ssids = new List<WIFISSID>();

        public MyWifi()
        {
            ssids.Clear();
        }


        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.UTF8.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        /// <summary>
        /// 枚举所有无线设备接收到的SSID
        /// </summary>
        public void ScanSSID()
        {
            WlanClient client = new WlanClient();
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                // Lists all networks with WEP security
                Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                foreach (Wlan.WlanAvailableNetwork network in networks)
                {
                    WIFISSID targetSSID = new WIFISSID();

                    targetSSID.wlanInterface = wlanIface;
                    targetSSID.wlanSignalQuality = (int)network.wlanSignalQuality;
                    targetSSID.SSID = GetStringForSSID(network.dot11Ssid);
                    //targetSSID.SSID = Encoding.Default.GetString(network.dot11Ssid.SSID, 0, (int)network.dot11Ssid.SSIDLength);
                    targetSSID.dot11DefaultAuthAlgorithm = network.dot11DefaultAuthAlgorithm.ToString();
                    targetSSID.dot11DefaultCipherAlgorithm = network.dot11DefaultCipherAlgorithm.ToString();
                    ssids.Add(targetSSID);

                    //if ( network.dot11DefaultCipherAlgorithm == Wlan.Dot11CipherAlgorithm.WEP )
                    //{
                    //    Console.WriteLine( "Found WEP network with SSID {0}.", GetStringForSSID(network.dot11Ssid));
                    //}
                    //Console.WriteLine("Found network with SSID {0}.", GetStringForSSID(network.dot11Ssid));
                    //Console.WriteLine("dot11BssType:{0}.", network.dot11BssType.ToString());
                    //Console.WriteLine("dot11DefaultAuthAlgorithm:{0}.", network.dot11DefaultAuthAlgorithm.ToString());
                    //Console.WriteLine("dot11DefaultCipherAlgorithm:{0}.", network.dot11DefaultCipherAlgorithm.ToString());
                    //Console.WriteLine("dot11Ssid:{0}.", network.dot11Ssid.ToString());

                    //Console.WriteLine("flags:{0}.", network.flags.ToString());
                    //Console.WriteLine("morePhyTypes:{0}.", network.morePhyTypes.ToString());
                    //Console.WriteLine("networkConnectable:{0}.", network.networkConnectable.ToString());
                    //Console.WriteLine("numberOfBssids:{0}.", network.numberOfBssids.ToString());
                    //Console.WriteLine("profileName:{0}.", network.profileName.ToString());
                    //Console.WriteLine("wlanNotConnectableReason:{0}.", network.wlanNotConnectableReason.ToString());
                    //Console.WriteLine("wlanSignalQuality:{0}.", network.wlanSignalQuality.ToString());
                    //Console.WriteLine("-----------------------------------");
                    // Console.WriteLine(network.ToString());
                }
            }
        } // EnumSSID


        /// <summary>
        /// 连接到未加密的SSID
        /// </summary>
        /// <param name="ssid"></param>
        public void ConnectToSSID(WIFISSID ssid)
        {
            // Connects to a known network with WEP security
            string profileName = ssid.SSID; // this is also the SSID

            string mac = StringToHex(profileName); // 

            //string key = "";
            //string profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>New{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><MSM><security><authEncryption><authentication>open</authentication><encryption>none</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>networkKey</keyType><protected>false</protected><keyMaterial>{2}</keyMaterial></sharedKey><keyIndex>0</keyIndex></security></MSM></WLANProfile>", profileName, mac, key);
            //string profileXml2 = "<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>Hacker SSID</name><SSIDConfig><SSID><hex>54502D4C494E4B5F506F636B657441505F433844323632</hex><name>TP-LINK_PocketAP_C8D262</name></SSID>        </SSIDConfig>        <connectionType>ESS</connectionType><connectionMode>manual</connectionMode><MSM> <security><authEncryption><authentication>open</authentication><encryption>none</encryption><useOneX>false</useOneX></authEncryption></security></MSM></WLANProfile>";
            //wlanIface.SetProfile( Wlan.WlanProfileFlags.AllUser, profileXml2, true );
            //wlanIface.Connect( Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName );
            string myProfileXML = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>manual</connectionMode><MSM><security><authEncryption><authentication>open</authentication><encryption>none</encryption><useOneX>false</useOneX></authEncryption></security></MSM></WLANProfile>", profileName, mac);
            
            ssid.wlanInterface.SetProfile(Wlan.WlanProfileFlags.AllUser, myProfileXML, true);
            ssid.wlanInterface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
            //Console.ReadKey();
        }

        /// <summary>
        /// 字符串转Hex
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToHex(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.Default.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString().ToUpper());
        }
    }

    class WIFISSID
    {
        public string SSID = "NONE";
        public string dot11DefaultAuthAlgorithm = "";
        public string dot11DefaultCipherAlgorithm = "";
        public bool networkConnectable = true;
        public string wlanNotConnectableReason = "";
        public int wlanSignalQuality = 0;
        public WlanClient.WlanInterface wlanInterface = null;
    }

    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length==0)
            {
                Console.WriteLine("不支持的操作.\n使用--Connect <SSID> <密码>连接WiFi\n使用--ShowList显示WiFi列表");
            }
            if (args.Length == 1)
            {
                string type = args[0];
                if (type == "--ShowList")
                {
                    Wifi wifi = new Wifi();
                    var WifiList = wifi.GetAccessPoints();
                    foreach(var i in WifiList)
                    {
                        Console.WriteLine(i.Name);
                    }
                }
                else
                {
                    Console.WriteLine("不支持的操作.\n使用--Connect <SSID> <密码>连接WiFi\n使用--ShowList显示WiFi列表");
                }
            }
            
            if (args.Length==2)
            {
                string type = args[0];
                Console.WriteLine(type);
                if (type=="--Connect")
                {
                    string SSID = args[1];
                    WlanClient client = new WlanClient();
                    foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                    {
                        // Lists all networks with WEP security
                        Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                        foreach (Wlan.WlanAvailableNetwork network in networks)
                        {
                            WIFISSID targetSSID = new WIFISSID();

                            targetSSID.wlanInterface = wlanIface;
                            targetSSID.wlanSignalQuality = (int)network.wlanSignalQuality;
                            targetSSID.SSID = GetStringForSSID(network.dot11Ssid);
                            //targetSSID.SSID = Encoding.Default.GetString(network.dot11Ssid.SSID, 0, (int)network.dot11Ssid.SSIDLength);
                            targetSSID.dot11DefaultAuthAlgorithm = network.dot11DefaultAuthAlgorithm.ToString();
                            targetSSID.dot11DefaultCipherAlgorithm = network.dot11DefaultCipherAlgorithm.ToString();
                            if (GetStringForSSID(network.dot11Ssid).Equals(SSID))
                            {
                                var obj = new wifiSo(targetSSID,"");
                                Thread wificonnect = new Thread(obj.ConnectToSSID);
                                wificonnect.Start();
                                Console.WriteLine("开始连接...");
                            }

                        }
                    }
                }
                else
                {
                    Console.WriteLine("不支持的操作.\n使用--Connect <SSID> <密码>连接WiFi\n使用--ShowList显示WiFi列表");
                }
            }
            if (args.Length == 3)
            {
                string type = args[0];
                Console.WriteLine(type);
                if (type == "--Connect")
                {
                    string SSID = args[1];
                    string Password = args[2];
                    WlanClient client = new WlanClient();
                    foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                    {
                        // Lists all networks with WEP security
                        Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                        foreach (Wlan.WlanAvailableNetwork network in networks)
                        {
                            WIFISSID targetSSID = new WIFISSID();

                            targetSSID.wlanInterface = wlanIface;
                            targetSSID.wlanSignalQuality = (int)network.wlanSignalQuality;
                            targetSSID.SSID = GetStringForSSID(network.dot11Ssid);
                            //targetSSID.SSID = Encoding.Default.GetString(network.dot11Ssid.SSID, 0, (int)network.dot11Ssid.SSIDLength);
                            targetSSID.dot11DefaultAuthAlgorithm = network.dot11DefaultAuthAlgorithm.ToString();
                            targetSSID.dot11DefaultCipherAlgorithm = network.dot11DefaultCipherAlgorithm.ToString();
                            if (GetStringForSSID(network.dot11Ssid).Equals(SSID))
                            {
                                var obj = new wifiSo(targetSSID, Password);
                                Thread wificonnect = new Thread(obj.ConnectToSSID);
                                wificonnect.Start();
                                Console.WriteLine("开始连接...");

                            }

                        }
                    }
                }
                else
                {
                    Console.WriteLine("不支持的操作.\n使用--Connect <SSID> <密码>连接WiFi\n使用--ShowList显示WiFi列表");
                }

                if (args.Length>=4)
                {
                    Console.WriteLine("不支持的操作.\n使用--Connect <SSID> <密码>连接WiFi\n使用--ShowList显示WiFi列表");
                }
            }
        }

        public static string StringToHex(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.Default.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)  
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString().ToUpper());
        }
        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.UTF8.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }
    }
}
