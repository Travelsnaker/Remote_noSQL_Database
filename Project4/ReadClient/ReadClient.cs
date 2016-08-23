///////////////////////////////////////////////////////////////
// ReadClient.cs - define ReadClient                         //
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
* This package defines two classes. 
* ReadClient class sends different kinds of query requests,
* and gets replys from the server. ClientExtension class defines
* different actions which determine what happens to received messages
* 
* public interface of ReadClient:
* ===================
* ReadClient readClient=new ReadClient(localUrl, remoteUrl);
* public bool readXmlFile(string XmlFile);
* public void sendMessages();
* public string toString(XElement elem);                                                                        
* public void shutDown();
* public bool addAct(string command, Func<Message, string> act);
* public bool removeAct(string command);
* Action serviceAction();
* 
* public interface of ClientExtension:
* ===================
* public Func<Message, string> keyQuery();
* public Func<Message,string>keysQuery();
*
* Maintance:
* -------------------
* Required Files: ReadClient.cs, ICommService.cs, Receiver.cs, Sender.cs, Utilities.cs
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
using System.Diagnostics;

namespace Project4Starter
{
    public class ReadClient
    {
        string localUrl;
        Stopwatch time;
        string remoteUrl;
        Sender sndr;
        Receiver rcvr;
        int numMsgs = 0;
        int totalNumMsg = 0;
        int count = 0;
        List<Message> messages = null;
        Dictionary<string, Func<Message, string>> acts;
        public ReadClient(string LocalUrl, string ToUrl)
        {
            acts = new Dictionary<string, Func<Message, string>>();
            localUrl = LocalUrl;
            remoteUrl = ToUrl;
            rcvr = new Receiver(Utilities.urlPort(localUrl), Utilities.urlAddress(localUrl));
            if (rcvr.StartService())
            {
                rcvr.doService(serviceAction());
            }
            sndr = new Sender(localUrl);
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
                Console.Write("\n The XML file does not exist");
                return false;
            }
            XDocument xml = XDocument.Parse(doc.ToString());
            XElement numMsg = xml.Descendants("numMsg").ElementAt(0);
            numMsgs = int.Parse(numMsg.Value);
            IEnumerable<XElement> msgs = xml.Descendants("Msg");
            totalNumMsg=numMsgs*(msgs.Count());
            count = totalNumMsg;
            messages = new List<Message>();
            foreach (XElement msg in msgs)
            {
                Message message = new Message();
                message.fromUrl = localUrl;
                message.toUrl = remoteUrl;
                message.content =msg.ToString();
                messages.Add(message);
            }
            return true;
        }
        public void sendMessages()
        {
            if (messages == null)
                return;
            time = new Stopwatch();
            time.Start();
            foreach (Message msg in messages)
                for (int i = 0; i < numMsgs; ++i)
                    sndr.sendMessage(msg);
        }
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
        public bool addAct(string command, Func<Message, string> act)
        {
            if (acts.ContainsKey(command))
                return false;
            acts[command] = act;
            return true;
        }
        public bool removeAct(string command)
        {
            if (acts.ContainsKey(command))
            {
                acts.Remove(command);
                return true;
            }
            return false;
        }
        Action serviceAction()
        {
            Action ServiceAction = () =>
              {
                  Message msg = null;
                  while (true)
                  {
                      msg = rcvr.getMessage();                     
                      if (msg.content == "connection start message")
                          continue;
                      if (msg.content == "done")
                          continue;
                      if (msg.content == "closeReceiver")
                          break;                      
                      --totalNumMsg;
                      if (totalNumMsg == 0)
                          sendTimeInfo();
                      XDocument xml = new XDocument();
                      xml = XDocument.Parse(msg.content);
                      string reply = xml.Descendants("Msg").Descendants("Reply").ElementAt(0).Value;
                      string DBName = xml.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                      if (acts.ContainsKey(reply))
                      {
                          Console.Write("\n\n ---{0} result in DB {1}--- ", reply,DBName);
                          Console.Write(acts[reply].Invoke(msg));
                      }
                      else
                          Console.Write("\n Invaild reply \n");
                  }
              };
            return ServiceAction;
        }
        void sendTimeInfo()
        {
            time.Stop();
            Message timeMsg = new Message();
            timeMsg.fromUrl = localUrl;
            timeMsg.toUrl = remoteUrl;
            StringBuilder Content = new StringBuilder(string.Format("<Msg><Command>readClientPerf</Command><numMsg>{0}</numMsg><timeinfo>{1}</timeinfo></Msg>", count, time.ElapsedMilliseconds.ToString()));
            timeMsg.content = Content.ToString();
            sndr.sendMessage(timeMsg);
            count = 0;
        }
        static void Main(string[] args)
        {
            int port = 8083;
            if (args.Length > 0)
                port = int.Parse(args[0]);
            string localUrl= string.Format("http://localhost:{0}/CommService",port);
            string remoteUrl= "http://localhost:8080/CommService";
            ReadClient readClient = new ReadClient(localUrl, remoteUrl);
            ClientExtension fun = new ClientExtension();
            readClient.addAct("keyQuery", fun.keyQuery());
            readClient.addAct("childrenQuery", fun.keysQuery());
            readClient.addAct("timeQuery", fun.keysQuery());
            readClient.addAct("specPatternQuery", fun.keysQuery());
            readClient.addAct("stringQuery", fun.keysQuery());
            if(readClient.readXmlFile("XMLFile1.xml"))
                readClient.sendMessages();
            Utilities.waitForUser();           
            readClient.shutDown();
            Console.Write("\n\n");
        }
    }
    // Determine what happens to received messages
    public class ClientExtension
    {
        // show the query result for a specified key.
        public Func<Message, string> keyQuery()
        {
            Func<Message, string> keyQueryReply = (msg) =>
              {
                  XDocument doc = null;
                  try
                  {
                      doc = XDocument.Parse(msg.content);
                  }
                  catch
                  {
                      return "Invalid message";
                  }
                  return xmlToString(doc);
              };
            return keyQueryReply;
        }
        // show the query result that returns a list of keys.
        public Func<Message,string>keysQuery()
        {
            Func<Message, string> KeysQuery = (msg) =>
                {
                    XDocument doc = null;
                    try
                    {
                        doc = XDocument.Parse(msg.content);
                    }
                    catch
                    {
                        return "Invalid message";
                    }
                    IEnumerable<XElement> keys = doc.Descendants("Msg").Descendants("Data").Descendants("key");
                    StringBuilder result = new StringBuilder();
                    foreach (XElement key in keys)
                    {
                        result.Append(string.Format("\n key: {0}", key.Value));
                    }
                    return result.ToString();
                };
            return KeysQuery;
        }
        // convert XDocument to DBElement
        string xmlToString(XDocument doc)
        {
            StringBuilder result = new StringBuilder();
            IEnumerable<XElement> elems = doc.Descendants("Data");
            if(elems.ElementAt(0).Value== "This key does not exist")
            {
                result.Append("\n This key does not exist");
                return result.ToString();
            }
            foreach (var elem in elems)
            {
                bool firstChild = true, firstItem = true;
                string key = elem.Descendants("key").ElementAt(0).Value;
                string name = elem.Descendants("value").Descendants("name").ElementAt(0).Value;
                string descr = elem.Descendants("value").Descendants("descr").ElementAt(0).Value;
                string time = elem.Descendants("value").Descendants("timestamp").ElementAt(0).Value;
                result.Append(string.Format("\n ---key = {0}---", key));
                result.Append(string.Format("\n name: {0}", name));
                result.Append(string.Format("\n desc: {0}", descr));
                result.Append(string.Format("\n time: {0}", time));
                IEnumerable<XElement> children = elem.Descendants("children");
                IEnumerable<XElement> items = elem.Descendants("payload");
                foreach (var child in children)
                {
                    string Key = child.Descendants("key").ElementAt(0).Value;
                    if (firstChild)
                    {
                        firstChild = false;
                        result.Append(string.Format("\n Children: {0}", Key));
                    }
                    else
                        result.Append(string.Format(", {0}", Key));
                }
                if (items.Count() == 1)
                {
                    result.Append(string.Format("\n payload: \n {0}", items.ElementAt(0).Value));
                    return result.ToString();
                }
                foreach (var item in items)
                {
                    string Key = item.Descendants("item").ElementAt(0).Value;
                    if (firstItem)
                    {
                        firstItem = false;
                        result.Append(string.Format("\n payload: \n {0}", Key));
                    }
                    else
                        result.Append(string.Format(", {0}", Key));
                }
            }
            return result.ToString();
        }
    }
}
