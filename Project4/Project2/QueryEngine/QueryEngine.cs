///////////////////////////////////////////////////////////////
// QueryEngine.cs - Define query engine for noSQL database   //
// Ver 1.0                                                   //
// Application: CSE681   Project#2                           //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    lenovo Y470, Core-i3, Windows 7              //
// Author:      Wei Sun, Syracuse University                 //
//              wsun13@syr.edu                               //
///////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package is used to support query. It implements class QueryEngine<Key, Data> where Data
* is the payload type. 
* 
* public interface:
* ===================
* QueryEngine<Key,Data> query=new QueryEngine<Key,Data>(DBEngine<Key, DBElement<Key, Data>> dbEngine);
* bool KeyQuery(Key key,out DBElement<Key,Data>elem);
* bool queryChildren(Key key,out List<Key>children);
* Func<Key,bool>defineSpecPatternQuery();
* Func<Key, bool> defineTimeQuery(DateTime time1, DateTime time2); //default time value is DateTime.Now
* Func<Key,bool>defineStringQuery(string search);
* bool processQuery(Func<Key,bool>queryPredicate,out List<Key>keyCollection);
*
* Maintance:
* -------------------
* Required Files: QueryEngine.cs, DBElement.cs, DBEngine.cs, DBExtensions.cs
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
using static System.Console;


namespace Project2
{
    public class QueryEngine<Key, Data>
    {
        private DBEngine<Key, DBElement<Key, Data>> db;
        public QueryEngine(DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            db = dbEngine;
        }
        // Query for the value of specific key
        public bool KeyQuery(Key key,out DBElement<Key,Data>elem)
        {
            if (db.getValue(key,out elem))
                return true;
            return false;
        }
        // Query for the children of a specific key
        public bool queryChildren(Key key,out List<Key>children)
        {
            children = new List<Key>();
            DBElement<Key, Data> elem;
            if (db.getValue(key,out elem))
                foreach (var child in elem.children)                
                    children.Add(child);
            if (children.Count() > 0)
                return true;
            return false;                       
        }

        // Return a functor to check if a key contains string "key"
        public Func<Key,bool>defineSpecPatternQuery()
        {
            Func<Key, bool> queryPredicate = (Key key) =>
                {
                    if (key.ToString().Contains("key"))
                        return true;
                    else
                        return false;
                };
            return queryPredicate;
        }
        // Return Func<Key,bool> to check if the key/value is written within a specific time interval
        public Func<Key, bool> defineTimeQuery(DateTime time1, DateTime time2)
        {
            Func<Key, bool> queryPredicate = (Key key) =>
            {
                DBElement<Key, Data> elem;
                if (db.getValue(key, out elem))
                {
                    if (time1 == default(DateTime))
                        time1 = DateTime.Now;
                    if (time2 == default(DateTime))
                        time2 = DateTime.Now;
                    int result = DateTime.Compare(time1, time2);
                    DateTime From = result <= 0 ? time1 : time2;
                    DateTime End = result >= 0 ? time1 : time2;
                    int compToFrom = DateTime.Compare(elem.timeStamp, From);
                    int compToEnd = DateTime.Compare(elem.timeStamp, End);
                    if (compToFrom >= 0 && compToEnd <= 0)
                        return true;
                }
                return false;
            };
            return queryPredicate;
        }
        // Return Func<Key,bool> to check if it contains a specific string in metadata
        public Func<Key,bool>defineStringQuery(string search)
        {
            Func<Key, bool> queryPredicate = (Key key) =>
            {
                DBElement<Key, Data> elem;
                if (db.getValue(key, out elem))
                {
                    if (elem.name.Contains(search))
                        return true;
                    else if (elem.descr.Contains(search))
                        return true;
                    else
                    {
                        foreach (var child in elem.children)
                            if (child.ToString().Contains(search))
                                return true;
                    }
                }
                return false;
            };
            return queryPredicate;
        }
        // process the query according to the Func 
        public bool processQuery(Func<Key,bool>queryPredicate,out List<Key>keyCollection)
        {
            keyCollection = new List<Key>();
            foreach(var key in db.Keys())
            {
                if(queryPredicate(key))
                    keyCollection.Add(key);
            }
            if (keyCollection.Count() > 0)
                return true;
            return false;
        }   
    }
    
   

#if (TEST_QUERY_ENGINE)
    class TestQueryEngine
    {
        static void Main(string[]args)
        {
            Write("\n --- Test specific key query ---");
            Write("\n ---Database contents");
            
            DBElement<int, string> elem1 = new DBElement<int, string>();
            elem1.descr = "payload desxription";
            elem1.name = "element 1";
            elem1.timeStamp = DateTime.Now;
            elem1.payload = "a payload";
            
            WriteLine();

            DBElement<int, string> elem2 = new DBElement<int, string>("Darth Vader", "Evil Overlord");
            elem2.descr = "star war 2";
            elem2.name = "element 2";
            elem2.timeStamp = new DateTime(2015,9,10,12,30,1);    
            elem2.payload = "The Empire strikes back!";
            
            WriteLine();

            var elem3 = new DBElement<int, string>("Luke Skywalker", "Young HotShot");
            elem3.name = "element 3";
            elem3.descr = "star war 3";
            elem3.timeStamp = new DateTime(2015,10,2,8,0,0);
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
            QueryEngine<int, string> QETest = new QueryEngine<int, string>(db);
            WriteLine();

            Write("\n --- Query element which key=2");
            QueryEngine<int, string> query = new QueryEngine<int, string>(db);
            DBElement<int, string> elem;
            bool valQuery = query.KeyQuery(2,out elem);
            if (valQuery)
                Write("\n  This element exist");
            Write(elem.showElement<int,string>());
            WriteLine();

            Write("\n --- Test queries for children of a specified key ---");
            Write("\n --- Query for children of element which key=3 ---");
            List<int> children;
            bool childQuery = query.queryChildren(3, out children);
            if (childQuery)
                Write("\n  This element has child");
            foreach (var child in children)
                Write("\n Key of child: {0}", child.ToString());
            WriteLine();

            Write("\n --- Test all keys that contain a specified string in their metadata section---");
            Write("\n --- query for \"star war\" in metadata, return keys ---");
            
            Func<int, bool> stringTest = QETest.defineStringQuery("star war");
            List<int> keyCollection;
            bool stringQuery = QETest.processQuery(stringTest, out keyCollection);
            foreach(var k in keyCollection)
                Write("\n Key: {0}", k.ToString());
            WriteLine();

            WriteLine("\n --- Test query according to a specified time-date interval ---");
            DateTime time1 = new DateTime(2015, 10, 1, 0, 0, 0);
            DateTime time2 = new DateTime(2015, 10, 2, 11, 0, 0);
            List<int> timeCollection;
            Func<int,bool>timeTest = QETest.defineTimeQuery(time1, time2);
            bool timeResult = QETest.processQuery(timeTest, out timeCollection);
            foreach (var k in timeCollection)
                WriteLine("key in specific time interval: {0}", k.ToString());
            WriteLine();
            WriteLine("--- When time is not provided ---");
            DateTime time3 = new DateTime();

            timeTest = QETest.defineTimeQuery(time1, time3);
            bool timeQuery2 = QETest.processQuery(timeTest, out timeCollection);

            foreach (var k in timeCollection) 
                WriteLine("key in specific time interval: {0}", k.ToString());
            WriteLine();
        }
    }
#endif
}
