using System;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class camera
    {
        string path = @"C:\Program Files\Weighing System\Camera";
        string xmlTagName = "IPAddress";
        string filePath = @"C:\\Program Files\\Weighing System\\Camera\\68d5fd42-5595-41e2-81c9-639793ab870f.config";
        string ipAddress = string.Empty;
        Dictionary<string, string> ds = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            camera camera = new camera();

            string[] fileEntries = Directory.GetFiles(camera.path);
            foreach (string fileName in fileEntries)
            {
                if (fileName.EndsWith(".config"))
                {
                    if (camera.macAddressVerifier(fileName) == false)
                    {
                        camera.ipAddress = camera.ipRetriever(fileName);
                    }
                }
                else
                    Console.WriteLine("No device configuration exists in specified XML Document.");
            }

            camera.configFilesReader(camera.path, camera.xmlTagName);
            foreach (KeyValuePair<string, string> kvp in camera.ds)
            {
                Console.WriteLine("Key =  {0}, Value = {1}", kvp.Key, kvp.Value);
                if (camera.macAddressVerifier(camera.filePath) == false)
                    if (kvp.Value == camera.ipAddress)
                        camera.xmlWriter(camera.filePath, "Camera", kvp.Key);
            }

            Console.Read();
        }

        //-->Code will read all config files in the given path 
        public void configFilesReader(string path, string tagName)
        {
            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                if (fileName.EndsWith(".config"))
                {
                    Console.WriteLine(getMacAddress(xmlReader(fileName, tagName)));

                    //                    Console.WriteLine(xmlReader(fileName, tagName));
                }
                else
                    Console.WriteLine("No device configuration exists in specified XML Document.");
            }

        }
        //-->Code will read all config files in the given path 

        //-->Code for calling arp -a and retrieving macaddress 
        public string getMacAddress(string ipAddress)
        {
            string cmdLine;
            string macAddress = string.Empty;

            ProcessStartInfo PingNetwork = new ProcessStartInfo();
            PingNetwork.FileName = "cmd.exe";
            PingNetwork.Arguments = "/C for /l %i in (1,1,254) do @ping 192.168.1.%i -n 1 -w 100 -l 1";
            PingNetwork.UseShellExecute = true;
            PingNetwork.CreateNoWindow = true;
            Process Pn = Process.Start(PingNetwork);
            Pn.WaitForExit();

            // if (!Pn.HasExited) { }
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/C arp -a > net_info.txt";
            psi.UseShellExecute = true;
            Process tmp = Process.Start(psi);

            using (StreamReader sr = new StreamReader(Environment.CurrentDirectory + "\\net_info.txt"))
            {
                string line = string.Empty;
                while (sr.EndOfStream == false)
                {
                    line = sr.ReadLine();
                    // Dictionary<string, string> ds = new Dictionary<string, string>();

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] address = line.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        if (!ds.ContainsKey(address[1])) //Example 255.255.255.255 is the phsyical layer broadcast address while 192.168.1.255 would be considered the network layer broadcast address both hold the same **MacAddress
                            ds.Add(address[1], address[0]);
                    }
                }
                //
                //
            }
            return macAddress;
        }
        //-->Code for calling arp -a and grabbing mac address 

        //-->Code for reading xml
        public string xmlReader(string fileName, string tagName)
        {
            string innerText = string.Empty;
            string deviceType = path.Split('/').Last();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            XmlNodeList root = xmlDoc.GetElementsByTagName("DeviceConfiguration");
            if (root[0] != null)
                foreach (XmlNode node in root[0].ChildNodes)
                {
                    if (node.Name == "Camera")
                    {
                        XmlNodeList cameraNodes = node.ChildNodes;
                        foreach (XmlNode cameraNode in cameraNodes)
                            if (cameraNode.Name == tagName)
                            {
                                if (cameraNode.InnerText.Contains(":"))
                                    innerText = cameraNode.InnerText.Substring(0, cameraNode.InnerText.IndexOf(':')).Trim();
                                else
                                    innerText = cameraNode.InnerText.Trim();
                            }
                    }
                }
            return innerText;
        }
        //-->Code for reading xml

        //-->Code for writing in the config xml
        public void xmlWriter(string fileName, string tagName, string keyValue)
        {
            string innerText = string.Empty;
            string deviceType = path.Split('/').Last();


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            //XmlElement elem = xmlDoc.CreateElement("MACAddress");


            XmlNodeList root = xmlDoc.GetElementsByTagName("DeviceConfiguration");
            if (root[0] != null)
            {
                foreach (XmlNode node in root[0].ChildNodes)
                {
                    if (node.Name == tagName)
                    {
                        //Create a new node.
                        // XmlElement elem = xmlDoc.CreateElement("MACAddress");
                        XmlElement elem = xmlDoc.CreateElement("MACAddress", "http://tempuri.org/DeviceConfiguration.xsd");
                        elem.InnerText = keyValue;

                        //Add the node to the document.
                        // xmlDoc.DocumentElement.AppendChild(elem);
                        node.InsertAfter(elem, node.FirstChild);
                        xmlDoc.Save(filePath);


                    }
                }
            }
        }
        //-->Code for writing in the config xml

        //->Method to verify the existence of a macaddress
        public bool macAddressVerifier(string filepath)
        {
            bool macAddress = false;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            XmlNodeList List = xmlDoc.GetElementsByTagName("Camera");


            if (List[0] != null)
            {
                foreach (XmlNode node in List[0].ChildNodes)
                    if (node.Name == "MACAddress")
                        macAddress = true;

            }

            return macAddress;
        }
        //->Method to verify the existence of a macaddress

        //->Method to retrieve ipaddress from configfile 
        public string ipRetriever(string filepath)
        {
            string ipAddress = string.Empty;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            XmlNodeList List = xmlDoc.GetElementsByTagName("Camera");

            if (List[0] != null)
            {
                foreach (XmlNode node in List[0].ChildNodes)
                    if (node.Name == "IPAddress")
                        ipAddress = node.InnerText;
            }

            return ipAddress;
        }
        //->Method to retrieve ipaddress from configfile 
    }
}

