///////////////////////////////////////////////////////////////
// TestExec.cs - Test Requirements for Project #2            //
// Ver 1.2                                                   //
// Application: Demonstration for CSE687-OOD, Project#2      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    lenovo Y470, windows 7                       //
// Author:      Wei Sun, Syracuse University                 //
//              wsun13@syr.edu                               //
///////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package begins the demonstration of meeting requirements.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   TestExec.cs,  DBElement.cs, DBEngine, Display, 
 *   DBExtensions.cs, UtilityExtensions.cs, DBFactory.cs,
 *   QueryEngine.cs, PersistEngine.cs, Scheduler.cs, TestExec
 *
 * Build Process:  devenv Project2.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.1 : 24 Sep 15
 * ver 1.0 : 18 Sep 15
 * - first release
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Project2
{
    class TestExec
    {
        private DBEngine<int, DBElement<int, string>> dbInt = new DBEngine<int, DBElement<int, string>>();
        private DBEngine<string, DBElement<string, List<string>>> packages = new DBEngine<string, DBElement<string, List<string>>>();
        private DBEngine<string, DBElement<string, List<string>>> DBListOfString = new DBEngine<string, DBElement<string, List<string>>>();
        void TestR2()
        {
            "Demonstrating Requirement #2".title();
            Write("\n --- When the instance is string --- \n");
            DBElement<int, string> elem1 = new DBElement<int, string>();
            elem1.showElement();
            dbInt.insert(1, elem1);
            dbInt.showDB();
            WriteLine();

            Write("\n --- When the instance is a list of string ---");
            DBElement<string, List<string>> elem2 = new DBElement<string, List<string>>();
            elem2.name = "elementLos1";
            elem2.descr = "element with ListofStrings payload";
            elem2.timeStamp = new DateTime(2015, 9, 29, 12, 0, 0);           
            elem2.payload=new List<string> { "one", "two", "three","four","five" };
            elem2.showElement();
            DBListOfString.insert("key15", elem2);
            DBElement<string, List<string>> elem3 = new DBElement<string, List<string>>();
            elem3.name = "elemLos2";
            elem3.descr = "element with ListOfString payload";
            elem3.timeStamp = new DateTime(2015, 10, 2, 10, 0, 0);
            elem3.children = new List<string> { "key1", "key2", "key3" };
            elem3.payload = new List<string> { "alpha", "beta", "gamma", "delta", "epsilon" };
            elem3.showElement();
            DBListOfString.insert("key16", elem3);
            DBListOfString.showEnumerableDB();
            WriteLine();
        }
        void TestR3()
        {
            "Demonstrating Requirement #3".title();
            Write("\n --- show orginal database ---");            
            DBElement<int, string> elem2 = new DBElement<int, string>("Darth Vader", "element");
            elem2.payload = "The Empire strikes back!";
            dbInt.insert(2, elem2);
            dbInt.showDB();
            WriteLine();
            Write("\n --- add a new key/value pair ---");
            DBElement<int, string> elem3 = new DBElement<int, string>("Luke Skywalker", "element");
            elem3.children.AddRange(new List<int> { 1, 5, 23 });
            elem3.payload = "X-Wing fighter in swamp - Oh oh!";
            dbInt.insert(3, elem3);
            dbInt.showDB();
            WriteLine();
            Write("\n --- delete key/value pair where key=1");
            dbInt.delete(1);
            dbInt.showDB();
            WriteLine();
        }
        void TestR4()
        {
            "Demonstrating Requirement #4".title();
            Write("\n --- show orginal database ---");
            dbInt.showDB();
            WriteLine();
            Write("\n ---Add a child key 15 for key=3 ---");
            DBElement<int, string> elem;
            dbInt.getValue(3, out elem);
            bool childadd=elem.addChild(15);
            if (childadd)
                Write("\n the new child added successfully");
            else
                Write("\n the child has already existed");
            dbInt.showDB();
            WriteLine();
            Write("\n --- Remove child key=15 from element key=3 ---");
            bool childRemove=elem.removeChild(15);
            if (childRemove)
                Write("\n the child has been removed");
            else
                Write("\n the child does not exist");
            dbInt.showDB();
            WriteLine();

            Write("\n --- Replace an instance with new instance ---");
            elem.payload = "The empire is failed";
            dbInt.showDB();
            WriteLine();
        }
        void TestR5()
        {
            "Demonstrating Requirement #5".title();
            Write("\n --- Test persist database contents to XML ---");
            PersistWapper<string, DBElement<string, List<string>>> persist = new StringAndStringList(DBListOfString);
            persist.writeToXML("DatabaseContent.xml");
            Write("\n --XML file has been created in ./databaseContent.xml--");
            WriteLine();
            Write("\n -- Restore an existing xml file ---");
            persist.restore("databaseContent.xml");
            persist.persistDB.showEnumerableDB();
            WriteLine();
        }
        void TestR6()
        {
            "Demonstrating Requirement #6".title();
            Write("\n --- Test sheduler---");
            dbInt.showDB();
            PersistWapper<int, DBElement<int, string>> persist = new IntAndString(dbInt);
            Schedular<int, DBElement<int, string>> test = new Schedular<int, DBElement<int, string>>();
            Write("\n persist database in ./DatabaseContent2.xml");
            test.autoSave(persist, 1000, "DatabaseContent2.xml");
            ReadKey();
            WriteLine();
        }
        void TestR7()
        {
            "Demonstrating Requirement #7".title();
            Write("\n --- Test query ---");            
            DBElement<int, string> elem1 = new DBElement<int, string>();
            elem1.timeStamp = DateTime.Now;
            elem1.payload = "default payload";
            dbInt.insert(1, elem1);
            Write("\n --show orginal database--");
            dbInt.showDB();
            WriteLine();
            Write("\n --- Query for the value of a specified key=2 ---");
            QueryEngine<int, string> query = new QueryEngine<int, string>(dbInt);
            DBElement<int, string> elem;
            query.KeyQuery(2, out elem);
            elem.showElement();
            WriteLine();
            Write("\n --- Query for all children of a specified key---");
            List<int> children;
            bool child=query.queryChildren(3, out children);
            if (child)
                foreach (var c in children)
                    Write("\n child is {0}", c.ToString());
            else
                Write("\n This element doesn't contain key");
            WriteLine();           
        }
        void TestR7_2()
        {
            Write("\n ---Query for keys matching a specific pattern ---");
            Write("\n return all keys which contain string \"key\"");
            QueryEngine<string, List<string>> query2 = new QueryEngine<string, List<string>>(DBListOfString);
            List<string> result;
            Func<string,bool>defineQuery=query2.defineSpecPatternQuery();
            query2.processQuery(defineQuery, out result);
            foreach (var r in result)
                Write("\n key: {0}", r.ToString());
            WriteLine();
            Write("\n ---Query keys contains a specific string in metadata ---");
            Write("\n return all keys that contains \"elemLos2\" in its metadata");
            defineQuery = query2.defineStringQuery("elemLos2");
            query2.processQuery(defineQuery, out result);
            foreach (var r in result)
                Write("\n key: {0}", r.ToString());
            WriteLine();
            Write("\n ---Query for values written within a specific time interval---");
            Write("\n Query for value written between 10/1/2015 00:00:00 and 10/2/2015 11:00:00");
            DateTime time1 = new DateTime(2015, 10, 1, 0, 0, 0);
            DateTime time2 = new DateTime(2015, 10, 2, 11, 0, 0);
            defineQuery = query2.defineTimeQuery(time1, time2);
            query2.processQuery(defineQuery, out result);
            foreach (var r in result)
                Write("\n key: {0}", r.ToString());
        }
        void TestR8()
        {
            "Demonstrating Requirement #8".title();
            Write("\n Create a new immutable database");
            List<int> result;
            QueryEngine<int, string> query = new QueryEngine<int, string>(dbInt);
            Func<int, bool> defineQuery = query.defineStringQuery("element");
            query.processQuery(defineQuery, out result);
            Write("\n Query for \"element\" in metadata first, return a list of key");
            Write("\n Use the list of key to build immutable database");
            immutableDB<int, DBElement<int, string>> immdb = new immutableDB<int, DBElement<int, string>>(dbInt, result);
            Write("\n Show the immutable database");
            immdb.ImmutableDB.showDB();
            WriteLine();
        }
        void TestR9()
        {
            "Demonstrating Requirement #9".title();
            DBElement<string, List<string>> pack1 = new DBElement<string, List<string>>("DBElement.cs", "package structure");
            packages.insert("DBElement", pack1);
            DBElement<string, List<string>> pack2 = new DBElement<string, List<string>>("DBEngine.cs", "package structure");           
            packages.insert("DBEngine", pack2);
            DBElement<string, List<string>> pack3 = new DBElement<string, List<string>>("DBFactory.cs", "package structure");
            pack3.children.AddRange(new List<string>{ "DBEngine" });            
            packages.insert("DBFactory", pack3);
            DBElement<string, List<string>> pack4 = new DBElement<string, List<string>>("Dispaly.cs","package structure");
            pack4.children.AddRange(new List<string> { "DBElement", "DBEngine", "DBExtensions", "UtilityExtentions" });           
            packages.insert("Display", pack4);
            DBElement<string, List<string>> pack5 = new DBElement<string, List<string>>("PersistEngine.cs", "package structure");
            pack5.children.AddRange(new List<string> { "DBElement", "DBEngine" });           
            packages.insert("PersistEngine", pack5);
            DBElement<string, List<string>> pack6 = new DBElement<string, List<string>>("QueryEngine.cs", "package structure");
            pack6.children.AddRange(new List<string> { "DBElement", "DBEngine" });            
            packages.insert("QueryEngine", pack6);           
            DBElement<string, List<string>> pack7 = new DBElement<string, List<string>>("Scheduler.cs", "package structure");
            pack7.children.AddRange(new List<string> { "DBElement", "DBEngine", "PersistEngine" });           
            packages.insert("Scheduler", pack7);
            DBElement<string, List<string>> pack8 = new DBElement<string, List<string>>("TestExec.cs", "package structure");
            pack8.children.AddRange(new List<string> { "DBElement", "DBEngine", "PersistEngine", "DBExtensions" ,"DBFactory","Display","QueryEngine","Scheduler","UtiltiyExtension"});           
            packages.insert("TestExec", pack8);
            foreach (var key in packages.Keys())
            {
                DBElement<string, List<string>> elem;
                packages.getValue(key, out elem);
                elem.payload = new List<string>();
                elem.payload.AddRange(new List<string>{ "content" });
            }
            PersistWapper<string, DBElement<string,List<string>>> persist = new StringAndStringList(packages);
            persist.writeToXML("project2.xml");
            Write("\n write the package structure into XML file ./project2.xml");
            WriteLine();
        }
        static void Main(string[] args)
        {
            TestExec exec = new TestExec();
            exec.TestR2();
            exec.TestR3();
            exec.TestR4();
            exec.TestR5();           
            exec.TestR7();
            exec.TestR7_2();
            exec.TestR8();
            exec.TestR9();
            exec.TestR6();
            Write("\n\n");
        }
    }
}
