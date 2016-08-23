///////////////////////////////////////////////////////////////////
// PersistEngine.cs - Define persist function for noSQL database //
// Ver 1.0                                                       //
// Application: CSE681   Project#2                               //
// Language:    C#, ver 6.0, Visual Studio 2015                  //
// Platform:    lenovo Y470, Core-i3, Windows 7                  //
// Author:      Wei Sun, Syracuse University                     //
//              wsun13@syr.edu                                   //
///////////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package is used to support persist database content to a xml file, and restore 
* database content document from xml file. It defines an abstract class PersistWrapper<Key,Value>, 
* and two subclasses IntAndString and StringAndStringList. IntandString is used to 
* persist and restore DBEngine<int,DBElement<int,string>>, and StringAndStringList
* is used to persist and restore DBEngine<string,DBElement<string,List<string>>.
* 
* public interface:
* ===================
* DBEngine<Key, Value> persistDB { get; set; };
* void writeToXML(string XmlFile);
* bool restore(string XmlFile);
*
* Maintance:
* -------------------
* Required Files: DBFactory.cs, DBElement.cs, DBEngine.cs, DBExtensions.cs, Display.cs, QueryEngine.cs
* 
* Build process: devenv Project2.sln /Rebuild debug
*                 Run from Developer Command Prompt
*                 To find: search for developer
*
* Maintenance History:
* -------------------- 
* ver 1.0 : 15 Oct 6
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


namespace Project2
{
    public abstract class PersistWapper<Key, Value>
    {
        public DBEngine<Key, Value> persistDB { get; set; } = new DBEngine<Key, Value>();
        // Write database content to xml file, XmlFile is the name of xml file to be created
        public abstract void writeToXML(string XmlFile);
        // Read the xml file to database.
        public abstract bool restore(string XmlFile);
    }
    // Used for DBEngine<int, DBElement<int,string>>
    public class IntAndString : PersistWapper<int, DBElement<int,string>>
    {
        public IntAndString(DBEngine<int, DBElement<int, string>> db) { persistDB = db; }       
        public override void writeToXML(string XmlFile)
        {
            XDocument xml = new XDocument();
            xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            XComment comment = new XComment("Convert database content to XML file");
            xml.Add(comment);
            XElement root = new XElement("NoSQLdb");
            xml.Add(root);
            foreach (var key in persistDB.Keys())
            {
                XElement keyElem = new XElement("key", key.ToString());
                XElement element = new XElement("element");
                XElement val = new XElement("value");
                DBElement<int, string> elem;
                persistDB.getValue(key, out elem);
                XElement nameElem = new XElement("name", elem.name.ToString());
                XElement descrElem = new XElement("descr", elem.descr.ToString());
                XElement timeElem = new XElement("timestamp", elem.timeStamp.ToString());
                element.Add(keyElem);
                val.Add(nameElem, descrElem, timeElem);
                if (elem.children.Count() > 0)
                {
                    XElement childElem = new XElement("children");
                    foreach (var child in elem.children)                      
                        childElem.Add(new XElement("key", child.ToString()));
                    val.Add(childElem);
                }
                XElement valueElem = new XElement("payload", elem.payload.ToString());
                val.Add(valueElem);
                element.Add(val);
                root.Add(element);
            }
            xml.Save(XmlFile);
        }
        
        public override bool restore(string XmlFile)
        {
            XDocument doc = XDocument.Load(XmlFile);
            persistDB = new DBEngine<int, DBElement<int, string>>();           
            if (doc == null)
                return false; 
            XDocument xml = XDocument.Parse(doc.ToString());
            IEnumerable<XElement> allElem = xml.Descendants("element");
            foreach (var elem in allElem)
            {
                IEnumerable<XElement> keys = elem.Descendants("key");
                IEnumerable<XElement> names = elem.Descendants("name");
                IEnumerable<XElement> descrs = elem.Descendants("descr");
                IEnumerable<XElement> timestamps = elem.Descendants("timestamp");
                IEnumerable<XElement> children = elem.Descendants("children");
                IEnumerable<XElement> payloads = elem.Descendants("payload");
                DBElement<int, string> element = new DBElement<int, string>();
                if (keys.Count() != 1)
                    return false;
                int key = int.Parse(keys.ElementAt(0).Value);
                foreach (var name_ in names)
                    element.name = name_.Value;
                foreach (var descr_ in descrs)
                    element.descr = descr_.Value;
                foreach (var timestamp_ in timestamps)
                    element.timeStamp = (DateTime)timestamp_;
                foreach (var child in children)
                    element.children.Add(int.Parse(child.Value));
                foreach (var payload_ in payloads)
                    element.payload = payload_.Value;
                persistDB.insert(key, element);
            }            
            return true;
        }
    }
    // Used for DBEngine<string,DBElement<string,List<string>>>
    public class StringAndStringList : PersistWapper<string, DBElement<string, List<string>>>
    {
        public StringAndStringList(DBEngine<string, DBElement<string, List<string>>> db) { persistDB = db; }
        public override void writeToXML(string XmlFile)
        {
            XDocument xml = new XDocument();
            xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            xml.Add(new XComment("Convert database content to XML file"));
            XElement root = new XElement("NoSQLdb");
            xml.Add(root);
            foreach (var key in persistDB.Keys())
            {
                DBElement<string, List<string>> elem;
                persistDB.getValue(key, out elem);
                XElement element = new XElement("element");
                XElement val = new XElement("value");
                val.Add (new XElement("name", elem.name.ToString()), new XElement("descr", elem.descr.ToString()), new XElement("timestamp", elem.timeStamp.ToString()));
                element.Add(new XElement("key", key.ToString()),val);
                if (elem.children.Count() > 0)
                {
                    XElement childElem = new XElement("children");
                    foreach (var child in elem.children)
                        childElem.Add(new XElement("key", child.ToString()));
                    val.Add(childElem);
                }             
                XElement itemElem = new XElement("payload");
                foreach(var item in elem.payload)
                    itemElem.Add(new XElement("item", item.ToString()));                
                val.Add(itemElem);
                root.Add(element);
            }
            xml.Save(XmlFile);
        }

        public override bool restore(string XmlFile)
        {
            XDocument doc = XDocument.Load(XmlFile);
            persistDB = new DBEngine<string, DBElement<string, List<string>>>();
            if (doc == null)
                return false;
            XDocument xml = XDocument.Parse(doc.ToString());
            IEnumerable<XElement> allElem = xml.Descendants("element");
            foreach (var elem in allElem)
            {
                IEnumerable<XElement> keys = elem.Descendants("key");
                IEnumerable<XElement> names = elem.Descendants("name");
                IEnumerable<XElement> descrs = elem.Descendants("descr");
                IEnumerable<XElement> timestamps = elem.Descendants("timestamp");
                IEnumerable<XElement> children = elem.Descendants("children").Descendants("key");
                IEnumerable<XElement> payloads = elem.Descendants("payload");
                DBElement<string, List<string>> element = new DBElement<string, List<string>>();
                string key = keys.ElementAt(0).Value;
                foreach (var name_ in names)
                    element.name = name_.Value;
                foreach (var descr_ in descrs)
                    element.descr = descr_.Value;
                foreach (var timestamp_ in timestamps)
                    element.timeStamp = (DateTime)timestamp_;
                foreach (var child in children)
                    element.children.Add(child.Value);
                element.payload = new List<string>();
                foreach (var payload_ in payloads)
                    foreach (var item in payload_.Descendants("item"))
                        element.payload.Add(item.Value);
                persistDB.insert(key, element);
            }
            return true;
        }
    }


#if (TEST_PERSISTENGINE)
    class TestPersistEngine
    {
        // test write DBEngine<int,DBElement<int,string>> to xml
        static void TestWriteToXML()
        {
            Write("\n ---convert database content to XML ---");
            DBElement<int, string> elem1 = new DBElement<int, string>();
            elem1.descr = "payload desxription";
            elem1.name = "element 1";
            elem1.timeStamp = DateTime.Now;
            elem1.payload = "a payload";

            WriteLine();

            DBElement<int, string> elem2 = new DBElement<int, string>("Darth Vader", "Evil Overlord");
            elem2.descr = "star war 2";
            elem2.name = "element 2";
            elem2.timeStamp = new DateTime(2015, 9, 10, 12, 30, 1);
            elem2.payload = "The Empire strikes back!";

            WriteLine();

            var elem3 = new DBElement<int, string>("Luke Skywalker", "Young HotShot");
            elem3.name = "element 3";
            elem3.descr = "star war 3";
            elem3.timeStamp = new DateTime(2015, 10, 2, 8, 0, 0);
            elem3.children = new List<int> { 1, 2, 3 };
            elem3.payload = "X-Wing fighter in swamp - Oh oh!";

            WriteLine();
            int key = 0;
            Func<int> keyGen = () => { ++key; return key; };  // anonymous function to generate keys

            DBEngine<int, DBElement<int, string>> db = new DBEngine<int, DBElement<int, string>>();
            bool p1 = db.insert(keyGen(), elem1);
            bool p2 = db.insert(keyGen(), elem2);
            bool p3 = db.insert(keyGen(), elem3);
            db.show<int,DBElement<int,string>,string>();
            WriteLine();
            IntAndString test = new IntAndString(db);
            test.writeToXML("DatabaseContent.xml");
            Write("\n XML file has been created in ./bin/Debug");
        }
        
        static void Main(string[] args)
        {
            TestWriteToXML();
            Write("\n --- Test persist <string,<List<string>> to XML ---");
            DBElement<string, List<string>> elem1 = new DBElement<string, List<string>>();
            elem1.name = "elemLos1";
            elem1.descr = "element with ListOfString payload";
            elem1.timeStamp = new DateTime(2015, 10, 2, 11, 0, 0);
            elem1.payload = new List<string> { "one", "two", "three", "four", "five" };

            DBElement<string, List<string>> elem2 = new DBElement<string, List<string>>();
            elem2.name = "elemLos2";
            elem2.descr = "element with ListOfString payload";
            elem2.timeStamp = new DateTime(2015, 10, 2, 10, 0, 0);
            elem2.children = new List<string> { "key1", "key2", "key3" };
            elem2.payload = new List<string> { "alpha", "beta", "gamma", "delta", "epsilon" };

            DBEngine<string, DBElement<string,List<string>>> db = new DBEngine<string, DBElement<string, List<string>>>();
            bool p1=db.insert("key15", elem1);
            bool p2=db.insert("key16", elem2);
            if (p1 && p2)
                Write("\n  all inserts succeeded");
            else
                Write("\n  at least one insert failed");
            PersistWapper<string, DBElement<string, List<string>>> persist2 = new StringAndStringList(db);
           
            persist2.writeToXML("DatabaseContent.xml");

            persist2.persistDB = new DBEngine<string, DBElement<string, List<string>>>();
            if(persist2.restore("DatabaseContent.xml"))
            persist2.persistDB.show<string,DBElement<string,List<string>>,List<string>,string>();
            WriteLine();
        }
    }
#endif
}
