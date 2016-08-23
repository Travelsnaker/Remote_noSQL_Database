///////////////////////////////////////////////////////////////
// WriteClient.cs - define WriteClient                       //
// Ver 1.2                                                   //
// Application: Demonstration for CSE687-OOD, Project#4      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    lenovo Y470, Core-i3, Windows 7              //
// Author:      Wei Sun, Syracuse University                 //
//              wsun13@syr.edu                               //
///////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package defines WriteClient class. This class sends insert, delete, edit, persist,
* and restore request to server.
* 
* public interface:
* ===================
* WriteClient writeClient=new WriteClient(localUrl, remoteUrl);
* public void processCommandLine(string[]args);   //Process command line argument.
* public bool readXmlFile(string XmlFile);        //read a XML file which defines a number of messages.
* public void sendMessages();
* public string toString(XElement elem);          
* public void shutDown();                                                                         
*
*
* Maintance:
* -------------------
* Required Files: WriteClient.cs, ICommService.cs, Receiver.cs, Sender.cs, Utilities.cs
* 
* Build process: devenv CommPrototype.sln /Rebuild debug
*                 Run from Developer Command Prompt
*                 To find: search for developer
*
* Maintenance History:
* -------------------- 
* ver 1.0 : 15 Nov 19
* - first release
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static System.Console;
using System.Diagnostics;

namespace Project4Starter
{
    public class WriteClient
    {
        string localUrl;
        string remoteUrl;
        Sender sndr;
        Receiver rcvr;
        string option=null;
        int numMsgs = 0;
        List<Message> messages = null;
        public WriteClient(string LocalUrl,string ToUrl)
        {
            localUrl = LocalUrl;
            remoteUrl = ToUrl;
            rcvr = new Receiver(Utilities.urlPort(localUrl), Utilities.urlAddress(localUrl));
            if(rcvr.StartService())
            {
                rcvr.doService(rcvr.defaultServiceAction());
            }
            sndr = new Sender(localUrl);           
        }
        public void processCommandLine(string[]args)
        {
            if (args.Length > 0)
                option = args[0];
        }
        public bool readXmlFile(string XmlFile)
        {
            XDocument doc = null;
            try
            {
                doc = XDocument.Load(XmlFile);
            }
            catch
            {
                Write("\n The XML file does not exist");
                return false;
            }
            XDocument xml = XDocument.Parse(doc.ToString());
            XElement numMsg = xml.Descendants("numMsg").ElementAt(0);
            numMsgs = int.Parse(numMsg.Value);
            IEnumerable<XElement> msgs = xml.Descendants("Msg");
            messages = new List<Message>();
            foreach (XElement msg in msgs)
            {
               Message message = new Message();
               message.fromUrl = localUrl;
               message.toUrl = remoteUrl;
               message.content = toString(msg);
               messages.Add(message);
            }
            return true;
        }       
        public void sendMessages()
        {
            if (messages == null)
                return;
            Stopwatch time = new Stopwatch();
            time.Start();
            int count = (messages.Count())*numMsgs;
            foreach (Message msg in messages)
                for (int i = 0; i < numMsgs; ++i)
                {
                    if (option == "showMessage")
                        showMsg(msg);
                    sndr.sendMessage(msg);
                }
            time.Stop();
            sendTimeInfo(time, count);                 
        }
        // send the performance information
        void sendTimeInfo(Stopwatch time,int count)
        {            
            Message timeMsg = new Message();
            timeMsg.fromUrl = localUrl;
            timeMsg.toUrl = remoteUrl;
            StringBuilder Content = new StringBuilder(string.Format("<Msg><Command>writeClientPerf</Command><numMsg>{0}</numMsg><timeinfo>{1}</timeinfo></Msg>", count, time.ElapsedMilliseconds.ToString()));
            timeMsg.content = Content.ToString();
            sndr.sendMessage(timeMsg);
            count = 0;
        }
        // log to the console the message which is sent. 
        void showMsg(Message msg)
        {
            Console.Write("\n Sending message");
            Console.Write("\n LocalUrl:{0}", msg.fromUrl);
            Console.Write("\n RemoteUrl: {0}", msg.toUrl);
            Console.Write("\n Message content \n {0}\n", msg.content);
        }
        // convert XElement to string
        public string toString(XElement elem)
        {
            XDocument doc = XDocument.Parse(elem.ToString());
            System.IO.StringWriter sw = new System.IO.StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            doc.WriteTo(tx);
            string str = sw.ToString();
            return str;
        }
        public void shutDown()
        {
            rcvr.shutDown();
            sndr.shutdown();
        }
        //args[0] is an option switch
        //when it is "showMessage", messages are logged to the console.
        //args[1] is used to define the number of write client. 
        static void Main(string[]args)
        {
            int port = 8082;
            if(args.Length>1)
                port = int.Parse(args[1]);
            string remoteUrl = "http://localhost:8080/CommService";
            string localUrl =string.Format("http://localhost:{0}/CommService",port);            
            WriteClient writeClient1 = new WriteClient(localUrl,remoteUrl);
            writeClient1.processCommandLine(args);
            if(writeClient1.readXmlFile("XMLfile.xml"))
                writeClient1.sendMessages();
            Utilities.waitForUser();
            writeClient1.shutDown();
            Console.Write("\n\n");
        }
    }  
}
