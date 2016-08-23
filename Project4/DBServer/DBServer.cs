///////////////////////////////////////////////////////////////
// DBSever.cs - define server which hosts a remote database  //
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
* This package contains three classes. 
* DBServer defines the server that hosts the noSQL database. It receives the message from clients, 
* processes each message, performs different operations on database and send back the reply. 
* class PL_StringExt defines operations performs on DBEngine<string,DBElement<string,List<string>>.
* class Int_StringExt defines operations performs on DBEngine<int,DBElement<int,string>.
* 
* public interface of DBServer:
* ===================
* DBServer server = new DBServer(localUrl);
* public bool doService();
* public bool addAct(string command, Action<Message> act);   //Add a new action on DBServer
* public bool removeAct(string command);                     //Remove an action on DBServer
* public Action ServiceAction();
*
* public interface of PL_StringExt:
* ===================
* public Action<Message>insert(DBEngine<string, DBElement<string, List<string>>> db);
* public Action<Message>delete(DBEngine<string, DBElement<string, List<string>>> db);
* public Action<Message>edit(DBEngine<string, DBElement<string, List<string>>> db);
* public Action<Message>keyQuery(DBEngine<string, DBElement<string, List<string>>> db);
* public Action<Message>childrenQuery(DBEngine<string, DBElement<string, List<string>>> db);
* public Action<Message>timeQuery(DBEngine<string,DBElement<string,List<string>>>db);
* public Action<Message>specPatternQuery(DBEngine<string, DBElement<string, List<string>>> db);
* public Action<Message>stringQuery(DBEngine<string, DBElement<string, List<string>>> db);
* public Action<Message>persistDB(DBEngine<string, DBElement<string, List<string>>> db);
* public Action<Message> restoreDB(DBEngine<string, DBElement<string, List<string>>> db);
* 
* public interface of Int_StringExt:
* ===================
* public Action<Message>insert(DBEngine<int,DBElement<int,string>>db);
* public Action<Message>delete(DBEngine<int, DBElement<int, string>> db);
* public Action<Message>edit(DBEngine<int, DBElement<int, string>> db);
* public Action<Message> keyQuery(DBEngine<int, DBElement<int, string>> db);
* public Action<Message> childrenQuery(DBEngine<int, DBElement<int, string>> db);
* public Action<Message> timeQuery(DBEngine<int, DBElement<int, string>> db);
* public Action<Message> specPatternQuery(DBEngine<int, DBElement<int, string>> db);
* public Action<Message> stringQuery(DBEngine<int, DBElement<int, string>> db);
* public Action<Message> persistDB(DBEngine<int, DBElement<int, string>> db);
* public Action<Message> restoreDB(DBEngine<int, DBElement<int, string>> db);
* 
*
* Maintance:
* -------------------
* Required Files: DBServer.cs, DBElement.cs, DBEngine.cs, PersistEngine.cs, QueryEngine.cs
* DBExtensions.cs, Display.cs, UtilityExtensions.cs
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
using Project2;
using System.Threading;
using System.Diagnostics;

namespace Project4Starter
{ 
    public class DBServer
    {
        string localUrl;
        Sender sndr;
        Receiver rcvr;
        // Dictionary used to store actions for different request.
        Dictionary<string, Action<Message>> acts;
        int numMsg = 0;
        // Constructor of DBServer
        public DBServer(string LocalUrl)
        {
            localUrl = LocalUrl;
            acts = new Dictionary<string, Action<Message>>();
            sndr = new Sender(localUrl);
            rcvr = new Receiver(Utilities.urlPort(localUrl), Utilities.urlAddress(localUrl));
        }
        // start service of server
        public bool doService()
        {
            if (rcvr.StartService())
            {
                rcvr.doService(ServiceAction());
                return true;
            }
            return false;
        }
        // Add a new action.
        public bool addAct(string command, Action<Message> act)
        {
            if (acts.ContainsKey(command))
                return false;
            acts[command] = act;
            return true;
        }
        // Remove an  action.
        public bool removeAct(string command)
        {
            if (acts.ContainsKey(command))
            {
                acts.Remove(command);
                return true;
            }
            return false;
        }
        // Determine what happens to received messages
        public Action ServiceAction()
        {
            Action serviceAction = () =>
              {
                  int count = 0;
                  Stopwatch time = new Stopwatch();                  
                  Message msg = null;
                  time.Start();
                  while (true)
                  {
                      if (numMsg != 0 && numMsg == count)
                      {
                          time.Stop();
                          sendTimeInfo(time, count);
                      }                          
                      msg = rcvr.getMessage();
                      if (msg.content == "connection start message")
                          continue;
                      if (msg.content == "done")
                          continue;
                      if (msg.content == "closeServer")
                          break;
                      XDocument xml = new XDocument();
                      xml = XDocument.Parse(msg.content);
                      string order = xml.Descendants("Msg").ElementAt(0).Descendants("Command").ElementAt(0).Value;
                      if (order == "writeClientPerf")
                      {
                          numMsg += ClientPerf(msg);
                          sndr.sendMessage(msg);
                          continue;
                      }
                      if (order == "readClientPerf")
                      {
                          numMsg += ClientPerf(msg);
                          sndr.sendMessage(msg);
                          continue;
                      }
                      string DBName= xml.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;                                         
                      string command = DBName + order;                  
                      if (acts.ContainsKey(command))
                      {
                          acts[command].Invoke(msg);
                          sndr.sendMessage(msg);
                      }
                      else
                          Write("\n command {0} {1} doesn't exist \n", order, DBName);
                      ++count;
                  }                  
              };
            return serviceAction;
        }
        // Get the total number of message the server receives.
        int ClientPerf(Message msg)
        {
            XDocument xml = XDocument.Parse(msg.content);
            int count = int.Parse(xml.Descendants("Msg").Descendants("numMsg").ElementAt(0).Value);
            msg.toUrl= "http://localhost:8081/CommService";
            return count;          
        }
        // Send performance information to Project3UI
        void sendTimeInfo(Stopwatch time, int count)
        {
            Message timeMsg = new Message();
            timeMsg.fromUrl = localUrl;
            timeMsg.toUrl = "http://localhost:8081/CommService";
            StringBuilder Content = new StringBuilder(string.Format("<Msg><Command>serverPerf</Command><numMsg>{0}</numMsg><timeinfo>{1}</timeinfo></Msg>", count, time.ElapsedMilliseconds.ToString()));
            timeMsg.content = Content.ToString();
            sndr.sendMessage(timeMsg);
            count = 0;
        }
    }  
    // Define action on requests for DBEngine<string, DBElement<string,List<string>> 
    public class PL_StringExt
    {
        public Action<Message> insert(DBEngine<string, DBElement<string, List<string>>> db)
        {
            Action<Message> Insert =(msg)=>
            {
                XDocument xml = XDocument.Parse(msg.content);                
                XElement element = xml.Descendants("Msg").ElementAt(0).Descendants("Data").ElementAt(0);
                string key = element.Descendants("key").ElementAt(0).Value;
                DBElement<string, List<string>> elem = new DBElement<string, List<string>>();
                elem.name = element.Descendants("value").Descendants("name").ElementAt(0).Value;
                elem.descr= element.Descendants("value").Descendants("descr").ElementAt(0).Value;
                elem.timeStamp = (DateTime)element.Descendants("value").Descendants("timestamp").ElementAt(0);
                IEnumerable<XElement> children = element.Descendants("value").Descendants("children").Descendants("key");
                foreach(var child in children)
                    elem.children.Add(child.Value);
                IEnumerable<XElement> items = element.Descendants("value").Descendants("payload").Descendants("item");
                elem.payload = new List<string>();
                foreach (var item in items)    
                    elem.payload.Add(item.Value);                
                if (!db.insert(key, elem))
                    msg.content = "Insert fail";
                else
                {
                    msg.content = "Insert success";
                    Console.Write("\n\n --- insert a DBElement<string,List<string>>---");
                    elem.showEnumerableElement();
                }                   
                Utilities.swapUrls(ref msg);                
            };
            return Insert;
        }
        public Action<Message>delete(DBEngine<string, DBElement<string, List<string>>> db)
        {
            Action<Message> Delete = (msg) =>
               {
                   XDocument doc = XDocument.Parse(msg.content.ToString());
                   string key = doc.Descendants("Msg").Descendants("Data").Descendants("key").ElementAt(0).Value;
                   Console.Write("\n\n --- delete DBElement<string,List<string>> which key = {0}---", key);
                   Write("\n\n --- Database before delete ---");
                   db.showEnumerableDB();
                   if (db.delete(key))
                   {
                       msg.content = "Delete success";
                       Console.Write("\n\n --- Database after delete---", key);
                       db.showEnumerableDB();
                   }                      
                   else
                       msg.content = "Delete fail";
                   Utilities.swapUrls(ref msg);
               };
            return Delete;
        }
        public Action<Message>edit(DBEngine<string, DBElement<string, List<string>>> db)
        {
                Action<Message> Edit = (msg) =>
                   {
                       XDocument xml = XDocument.Parse(msg.content.ToString());
                       XElement element = xml.Descendants("Msg").ElementAt(0).Descendants("Data").ElementAt(0);
                       string key = element.Descendants("key").ElementAt(0).Value;
                       DBElement<string, List<string>> elem = new DBElement<string, List<string>>();
                       Console.Write("\n\n --- edit a DBElement<string,List<string>> which key={0}---",key);
                       Write("\n\n ---Database before edit---");
                       db.showEnumerableDB();
                       if (!db.getValue(key, out elem))
                       {
                           msg.content = "edit fail";
                           return;
                       }
                       elem.name = element.Descendants("value").Descendants("name").ElementAt(0).Value;
                       elem.descr = element.Descendants("value").Descendants("descr").ElementAt(0).Value;
                       elem.timeStamp = (DateTime)element.Descendants("value").Descendants("timestamp").ElementAt(0);
                       IEnumerable<XElement> children = element.Descendants("value").Descendants("children").Descendants("key");
                       elem.children = new List<string>();
                       foreach (var child in children)
                           elem.children.Add(child.Value);
                       IEnumerable<XElement> items = element.Descendants("value").Descendants("payload").Descendants("item");
                       elem.payload = new List<string>();
                       foreach (var item in items)
                           elem.payload.Add(item.Value);
                       msg.content = "edit success";
                       Utilities.swapUrls(ref msg);
                       Write("\n\n ---Database after edit---");
                       db.showEnumerableDB();
                   };
                return Edit;
        }
        public Action<Message> keyQuery(DBEngine<string, DBElement<string, List<string>>> db)
        {
            Action<Message> KeyQuery = (msg) =>
            {
                XDocument doc = XDocument.Parse(msg.content);
                string key = doc.Descendants("Msg").Descendants("Data").Descendants("key").ElementAt(0).Value;
                string name = doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                DBElement<string, List<string>> elem;
                QueryEngine<string, List<string>> qe = new QueryEngine<string, List<string>>(db);
                StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>keyQuery</Reply><DBName>{0}</DBName><Data>",name));
                if (qe.KeyQuery(key, out elem))
                {
                    result.Append(string.Format("<key>{0}</key><value>", key.ToString()));
                    result.Append(string.Format("<name>{0}</name><descr>{1}</descr><timestamp>{2}</timestamp>", elem.name.ToString(),
                        elem.descr.ToString(), elem.timeStamp.ToString()));
                    if (elem.children.Count() > 0)
                    {
                        result.Append("<children>");
                        foreach (var child in elem.children)
                            result.Append(string.Format("<key>{0}</key>", child.ToString()));
                        result.Append("</children>");
                    }
                    if (elem.payload.Count() > 0)
                    {
                        result.Append("<payload>");
                        foreach (var item in elem.payload)
                            result.Append(string.Format("<item>{0}</item>", item.ToString()));
                        result.Append("</payload>");
                    }
                    result.Append("</value>");
                }
                else
                    result.Append("This key does not exist");
                result.Append("</Data></Msg>");
                msg.content = result.ToString();
                Utilities.swapUrls(ref msg);
            };
            return KeyQuery;
        }
            public Action<Message>childrenQuery(DBEngine<string, DBElement<string, List<string>>> db)
            {
                Action<Message> ChildrenQuery = (msg) =>
                   {
                       QueryEngine<string, List<string>> qe = new QueryEngine<string, List<string>>(db);
                       XDocument doc = XDocument.Parse(msg.content.ToString());
                       string key = doc.Descendants("Msg").Descendants("Data").Descendants("key").ElementAt(0).Value;
                       string name = doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                       List<string> children;
                       StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>childrenQuery</Reply><DBName>{0}</DBName><Data>", name));
                       if (qe.queryChildren(key, out children))
                       {
                           foreach (var child in children)
                               result.Append(string.Format("<key>{0}</key>", child.ToString()));
                       }
                       else
                           result.Append("This element does not have children");
                       result.Append("</Data></Msg>");
                       msg.content = result.ToString();
                       Utilities.swapUrls(ref msg);
                   };
                return ChildrenQuery;
            }
            public Action<Message>timeQuery(DBEngine<string,DBElement<string,List<string>>>db)
            {
                Action<Message> TimeQuery = (msg) =>
                   {
                       QueryEngine<string, List<string>> qe = new QueryEngine<string, List<string>>(db);
                       XDocument doc = XDocument.Parse(msg.content);
                       string FirstTime = doc.Descendants("Msg").Descendants("Data").Descendants("firstTime").ElementAt(0).Value;
                       string SecondTime = doc.Descendants("Msg").Descendants("Data").Descendants("secondTime").ElementAt(0).Value;
                       DateTime firstTime = Convert.ToDateTime(FirstTime);
                       DateTime secondTime = Convert.ToDateTime(SecondTime);
                       Func<string, bool> timequery = qe.defineTimeQuery(firstTime, secondTime);
                       string name = doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                       List<string> keys;
                       StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>timeQuery</Reply><DBName>{0}</DBName><Data>",name));
                       if (qe.processQuery(timequery, out keys))
                       {
                           foreach (var key in keys)
                               result.Append(string.Format("<key>{0}</key>", key.ToString()));
                       }
                       else
                           result.Append("There is no element written in a specified time-date interval");
                       result.Append("</Data></Msg>");
                       msg.content = result.ToString();
                       Utilities.swapUrls(ref msg);
                   };
                return TimeQuery;
            }
            public Action<Message>specPatternQuery(DBEngine<string, DBElement<string, List<string>>> db)
            {
                Action<Message> SpecPatternQuery = (msg) =>
                   {
                       QueryEngine<string, List<string>> qe = new QueryEngine<string, List<string>>(db);
                       XDocument doc = XDocument.Parse(msg.content);
                       string name = doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                       Func<string, bool> f = qe.defineSpecPatternQuery();
                       List<string> keys;
                       StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>specPatternQuery</Reply><DBName>{0}</DBName><Data>",name));
                       if (qe.processQuery(f, out keys))
                       {
                           foreach (var key in keys)
                               result.Append(string.Format("<key>{0}</key>", key.ToString()));
                       }
                       else
                           result.Append("There is no key that matching a specified pattern");
                       result.Append("</Data></Msg>");
                       msg.content = result.ToString();
                       Utilities.swapUrls(ref msg);
                   };
                return SpecPatternQuery;
            }
            public Action<Message>stringQuery(DBEngine<string, DBElement<string, List<string>>> db)
            {
                Action<Message> StringQuery = (msg) =>
                   {
                       QueryEngine<string, List<string>> qe = new QueryEngine<string, List<string>>(db);
                       XDocument doc = XDocument.Parse(msg.content);
                       string search = doc.Descendants("Msg").Descendants("Data").Descendants("string").ElementAt(0).Value;
                       string name = doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                       Func<string, bool> f = qe.defineStringQuery(search);
                       List<string> keys;
                       StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>stringQuery</Reply><DBName>{0}</DBName><Data>",name));
                       if (qe.processQuery(f, out keys))
                       {
                           foreach (var key in keys)
                               result.Append(string.Format("<key>{0}</key>", key.ToString()));
                       }
                       else
                           result.Append("There is no key that matching a specified pattern");
                       result.Append("</Data></Msg>");
                       msg.content = result.ToString();
                       Utilities.swapUrls(ref msg);
                   };
                return StringQuery;
            }
        public Action<Message>persistDB(DBEngine<string, DBElement<string, List<string>>> db)
        {
            Action<Message> PersistDB = (msg) =>
               {
                   StringAndStringList pe = new StringAndStringList(db);
                   XDocument doc = XDocument.Parse(msg.content);
                   string XmlFile = doc.Descendants("Msg").Descendants("Data").Descendants("FileName").ElementAt(0).Value;
                   pe.writeToXML(XmlFile);
                   msg.content = "Persist success";
                   Console.Write("\n\n The database has been persisted");
                   Utilities.swapUrls(ref msg);
               };
            return PersistDB;
        }
        public Action<Message> restoreDB(DBEngine<string, DBElement<string, List<string>>> db)
        {
            Action<Message> RestoreDB = (msg) =>
            {
                StringAndStringList pe = new StringAndStringList(db);
                XDocument doc = XDocument.Parse(msg.content);
                string XmlFile = doc.Descendants("Msg").Descendants("Data").Descendants("FileName").ElementAt(0).Value;
                pe.writeToXML(XmlFile);
                Console.Write("\n\n ---The database has been restored ---");
                pe.persistDB.showEnumerableDB();
                msg.content = "Restore success";
                Utilities.swapUrls(ref msg);
            };
            return RestoreDB;
        }
    }

    //Define action on request for DBEngine<int,DBElement<int,string>>
    public class Int_StringExt
    {
        public Action<Message>insert(DBEngine<int,DBElement<int,string>>db)
        {
            Action<Message> Insert = (msg) =>
               {
                   XDocument doc = XDocument.Parse(msg.content);
                   int key = int.Parse(doc.Descendants("Msg").Descendants("Data").Descendants("key").ElementAt(0).Value);
                   DBElement<int, string> elem = new DBElement<int, string>();
                   elem.name = doc.Descendants("Msg").Descendants("Data").Descendants("value").Descendants("name").ElementAt(0).Value;
                   elem.descr = doc.Descendants("Msg").Descendants("Data").Descendants("value").Descendants("descr").ElementAt(0).Value;
                   elem.timeStamp = (DateTime)doc.Descendants("Msg").Descendants("Data").Descendants("value").Descendants("timestamp").ElementAt(0);
                   IEnumerable<XElement> children = doc.Descendants("Msg").Descendants("Data").Descendants("value").Descendants("children").Descendants("key");
                   foreach (var child in children)
                       elem.children.Add(int.Parse(child.Value));
                   IEnumerable<XElement> items = doc.Descendants("Msg").Descendants("Data").Descendants("value").Descendants("payload");
                   if (items.Count() == 1)
                       elem.payload = items.ElementAt(0).Value;
                   if (!db.insert(key, elem))
                       msg.content = "Insert fail";
                   else
                   {
                       msg.content = "Insert success";
                       Console.Write("\n\n --- insert a DBElement<int,string>---");
                       elem.showElement();
                   }
                   Utilities.swapUrls(ref msg);
               };
            return Insert;
        }
        public Action<Message>delete(DBEngine<int, DBElement<int, string>> db)
        {
            Action<Message> Delete = (msg) =>
               {
                   XDocument doc = XDocument.Parse(msg.content.ToString());
                   int key = int.Parse(doc.Descendants("Msg").Descendants("Data").Descendants("key").ElementAt(0).Value);
                   Console.Write("\n\n delete an element which key = {0}", key);
                   Write("\n\n --- Database before delete ---");
                   db.showDB();
                   if (db.delete(key))
                   {
                       msg.content = "Delete success";
                       Console.Write("\n\n --- Database after delete---", key);
                       db.showDB();
                   }                       
                   else
                       msg.content = "Delete fail";
                   Utilities.swapUrls(ref msg);                  
               };
            return Delete;
        }
        public Action<Message>edit(DBEngine<int, DBElement<int, string>> db)
        {
            Action<Message> Edit = (msg) =>
              {
                  XDocument xml = XDocument.Parse(msg.content.ToString());
                  XElement element = xml.Descendants("Msg").ElementAt(0).Descendants("Data").ElementAt(0);
                  int key = int.Parse(element.Descendants("key").ElementAt(0).Value);
                  DBElement<int, string> elem = new DBElement<int, string>();
                  Console.Write("\n\n --- edit a element which key ={0}--- ",key);
                  Write("\n\n ---Database before edit---");
                  db.showDB();
                  if (!db.getValue(key, out elem))
                  {
                      msg.content = "edit fail";
                      return;
                  }
                  elem.name = element.Descendants("value").Descendants("name").ElementAt(0).Value;
                  elem.descr = element.Descendants("value").Descendants("descr").ElementAt(0).Value;
                  elem.timeStamp = (DateTime)element.Descendants("value").Descendants("timestamp").ElementAt(0);
                  IEnumerable<XElement> children = element.Descendants("value").Descendants("children").Descendants("key");
                  elem.children = new List<int>();
                  foreach (var child in children)
                      elem.children.Add(int.Parse(child.Value));
                  IEnumerable<XElement> items = element.Descendants("value").Descendants("payload");
                  if (items.Count() == 1)
                      elem.payload = items.ElementAt(0).Value;
                  msg.content = "edit success";
                  Utilities.swapUrls(ref msg);
                  Console.Write("\n\n --- Database after edit ---");
                  db.showDB();
              };
            return Edit;
        }
        public Action<Message> keyQuery(DBEngine<int, DBElement<int, string>> db)
        {
            Action<Message> KeyQuery = (msg) =>
            {
                XDocument doc = XDocument.Parse(msg.content);
                int key = int.Parse(doc.Descendants("Msg").Descendants("Data").Descendants("key").ElementAt(0).Value);
                DBElement<int, string> elem;
                QueryEngine<int, string> qe = new QueryEngine<int, string>(db);
                string name = doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>keyQuery</Reply><DBName>{0}</DBName><Data>",name));
                if (qe.KeyQuery(key, out elem))
                {
                    result.Append(string.Format("<key>{0}</key><value>", key.ToString()));
                    result.Append(string.Format("<name>{0}</name><descr>{1}</descr><timestamp>{2}</timestamp>", elem.name.ToString(),
                        elem.descr.ToString(), elem.timeStamp.ToString()));
                    if (elem.children.Count() > 0)
                    {
                        result.Append("<children>");
                        foreach (var child in elem.children)
                            result.Append(string.Format("<key>{0}</key>", child.ToString()));
                        result.Append("</children>");
                    }
                    if (elem.payload.Count()==1)
                        result.Append(string.Format("<payload>{0}</payload>", elem.payload));                        
                    result.Append("</value>");
                }
                else
                    result.Append("This key does not exist");
                result.Append("</Data></Msg>");
                msg.content = result.ToString();
                Utilities.swapUrls(ref msg);
            };
            return KeyQuery;
        }
        public Action<Message> childrenQuery(DBEngine<int, DBElement<int, string>> db)
        {
            Action<Message> ChildrenQuery = (msg) =>
            {
                QueryEngine<int, string> qe = new QueryEngine<int,string>(db);
                XDocument doc = XDocument.Parse(msg.content.ToString());
                int key = int.Parse(doc.Descendants("Msg").Descendants("Data").Descendants("key").ElementAt(0).Value);
                string name = doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                List<int> children;
                StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>childrenQuery</Reply><DBName>{0}</DBName><Data>",name));
                if (qe.queryChildren(key, out children))
                {
                    foreach (var child in children)
                        result.Append(string.Format("<key>{0}</key>", child.ToString()));
                }
                else
                    result.Append("This element does not have children");
                result.Append("</Data></Msg>");
                msg.content = result.ToString();
                Utilities.swapUrls(ref msg);
            };
            return ChildrenQuery;
        }
        public Action<Message> timeQuery(DBEngine<int, DBElement<int, string>> db)
        {
            Action<Message> TimeQuery = (msg) =>
            {
                QueryEngine<int, string> qe = new QueryEngine<int, string>(db);
                XDocument doc = XDocument.Parse(msg.content);
                string FirstTime = doc.Descendants("Msg").Descendants("Data").Descendants("firstTime").ElementAt(0).Value;
                string SecondTime = doc.Descendants("Msg").Descendants("Data").Descendants("secondTime").ElementAt(0).Value;
                DateTime firstTime = Convert.ToDateTime(FirstTime);
                DateTime secondTime = Convert.ToDateTime(SecondTime);
                string name = doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                Func<int, bool> timequery = qe.defineTimeQuery(firstTime, secondTime);
                List<int> keys;
                StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>timeQuery</Reply><DBName>{0}</DBName><Data>",name));
                if (qe.processQuery(timequery, out keys))
                {
                    foreach (var key in keys)
                        result.Append(string.Format("<key>{0}</key>", key.ToString()));
                }
                else
                    result.Append("There is no element written in a specified time-date interval");
                result.Append("</Data></Msg>");
                msg.content = result.ToString();
                Utilities.swapUrls(ref msg);
            }; 
            return TimeQuery;
        }
        public Action<Message> specPatternQuery(DBEngine<int, DBElement<int, string>> db)
        {
            Action<Message> SpecPatternQuery = (msg) =>
            {
                QueryEngine<int, string> qe = new QueryEngine<int, string>(db);
                XDocument doc = XDocument.Parse(msg.content);
                Func<int, bool> f = qe.defineSpecPatternQuery();
                string name=doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                List<int> keys;
                StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>specPatternQuery</Reply><DBName>{0}</DBName><Data>",name));
                if (qe.processQuery(f, out keys))
                {
                    foreach (var key in keys)
                        result.Append(string.Format("<key>{0}</key>", key.ToString()));
                }
                else
                    result.Append("There is no key that matching a specified pattern");
                result.Append("</Data></Msg>");
                msg.content = result.ToString();
                Utilities.swapUrls(ref msg);
            };
            return SpecPatternQuery;
        }
        public Action<Message> stringQuery(DBEngine<int, DBElement<int, string>> db)
        {
            Action<Message> StringQuery = (msg) =>
            {
                QueryEngine<int, string> qe = new QueryEngine<int, string>(db);
                XDocument doc = XDocument.Parse(msg.content);
                string search = doc.Descendants("Msg").Descendants("Data").Descendants("string").ElementAt(0).Value;
                Func<int, bool> f = qe.defineStringQuery(search);
                List<int> keys;
                string name = doc.Descendants("Msg").Descendants("DBName").ElementAt(0).Value;
                StringBuilder result = new StringBuilder(string.Format("<Msg><Reply>stringQuery</Reply><DBName>{0}</DBName><Data>",name));
                if (qe.processQuery(f, out keys))
                {
                    foreach (var key in keys)
                        result.Append(string.Format("<key>{0}</key>", key.ToString()));
                }
                else
                    result.Append("There is no key that matching a specified pattern");
                result.Append("</Data></Msg>"); 
                msg.content = result.ToString();
                Utilities.swapUrls(ref msg);
            };
            return StringQuery;
        }
        public Action<Message> persistDB(DBEngine<int, DBElement<int, string>> db)
        {
            Action<Message> PersistDB = (msg) =>
            {
                IntAndString pe = new IntAndString(db);
                XDocument doc = XDocument.Parse(msg.content);
                string XmlFile = doc.Descendants("Msg").Descendants("Data").Descendants("FileName").ElementAt(0).Value;
                pe.writeToXML(XmlFile);
                msg.content = "Persist success";
                Utilities.swapUrls(ref msg);
                Console.Write("\n\n The database has been persisted");
            };
            return PersistDB;
        }
        public Action<Message> restoreDB(DBEngine<int, DBElement<int, string>> db)
        {
            Action<Message> RestoreDB = (msg) =>
            {
                IntAndString pe = new IntAndString(db);
                XDocument doc = XDocument.Parse(msg.content);
                string XmlFile = doc.Descendants("Msg").Descendants("Data").Descendants("FileName").ElementAt(0).Value;
                pe.writeToXML(XmlFile);
                Console.Write("\n\n --- The database has been restored ---");
                pe.persistDB.showDB();
                msg.content = "Restore success";
                Utilities.swapUrls(ref msg);
            };
            return RestoreDB;
        }
    }
    public class DBServerTest
    {
#if (TEST_SERVERDB)
        static void Main(string[] args)
        {
            DBEngine<string, DBElement<string, List<string>>> db = new DBEngine<string, DBElement<string, List<string>>>();
            DBEngine<int, DBElement<int, string>> db2 = new DBEngine<int, DBElement<int, string>>();
            PL_StringExt fun = new PL_StringExt();
            Int_StringExt fun2 = new Int_StringExt();
            DBServer server = new DBServer("http://localhost:8080/CommService");
            string DBName = "String_ListOfString";
            server.addAct(DBName+"insert", fun.insert(db));
            server.addAct(DBName + "keyQuery", fun.keyQuery(db));
            server.addAct(DBName + "delete", fun.delete(db));
            server.addAct(DBName + "edit", fun.edit(db));
            server.addAct(DBName + "childrenQuery", fun.childrenQuery(db));
            server.addAct(DBName + "timeQuery", fun.timeQuery(db));
            server.addAct(DBName + "specPatternQuery", fun.specPatternQuery(db));
            server.addAct(DBName + "stringQuery", fun.stringQuery(db));
            server.addAct(DBName + "persistDB", fun.persistDB(db));
            server.addAct(DBName + "restoreDB", fun.restoreDB(db));
            DBName = "Int_String";
            server.addAct(DBName + "insert", fun2.insert(db2));
            server.addAct(DBName + "keyQuery", fun2.keyQuery(db2));
            server.addAct(DBName + "delete", fun2.delete(db2));
            server.addAct(DBName + "edit", fun2.edit(db2));
            server.addAct(DBName + "childrenQuery", fun2.childrenQuery(db2));
            server.addAct(DBName + "timeQuery", fun2.timeQuery(db2));
            server.addAct(DBName + "specPatternQuery", fun2.specPatternQuery(db2));
            server.addAct(DBName + "stringQuery", fun2.stringQuery(db2));
            server.addAct(DBName + "persistDB", fun2.persistDB(db2));
            server.addAct(DBName + "restoreDB", fun2.restoreDB(db2));
            server.doService();            
            Utilities.waitForUser();
        }
    }
#endif
}


