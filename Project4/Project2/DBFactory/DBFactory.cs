///////////////////////////////////////////////////////////////
// DBFactory.cs - Define DBFactory for noSQL database        //
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
* This package is used to support the creation 
* of a new immutable database constructed from the result of any query that 
* returns a collection of keys. It implements class immutableDB<Key,Value> 
* where Value is DBElement type. 
* 
* public interface:
* ===================
* immutableDB<Key,Value>immu=new immutableDB<Key,Value>(DBEngine<Key, Value> db, List<Key> keys);
* DBEngine<Key, Value> ImmutableDB { get { return imDB; } }
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
using static System.Console;

namespace Project2
{
    public class immutableDB<Key,Value>
    {
        private DBEngine<Key, Value> imDB;
        // the contructor of immutableDB, it takes a reference to database,
        // and takes a list of key indentify the key/value pair that should 
        // be stored in immutable database.
        public immutableDB(DBEngine<Key, Value> db, List<Key> keys)
        {
            imDB = new DBEngine<Key, Value>();
            foreach (var key in keys)
            {
                Value val;
                if (db.getValue(key, out val))
                    imDB.insert(key, val);
            }
        }
        // Return the immutable database, it is read only.
        public DBEngine<Key, Value> ImmutableDB { get { return imDB; } }
    }

#if (TEST_DBFACTORY)
    
    class TestDBFactory
    {
        static void Main(string[] args)
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
            elem2.timeStamp = new DateTime(2015, 9, 10, 12, 30, 1);
            elem2.payload = "The Empire strikes back!";

            WriteLine();

            var elem3 = new DBElement<int, string>("Luke Skywalker", "Young HotShot");
            elem3.name = "fight";
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
            db.show<int, DBElement<int,string>,string>();
            WriteLine();

            Write("\n --- Test creation of immutable database ---");
            Write("\n --- create database for elements which contain \"element\" in metadata ---");
            QueryEngine<int, string> query = new QueryEngine<int, string>(db);
            Func<int, bool> defineQuery = query.defineStringQuery("element");
            List<int> keyCollection;
            bool result = query.processQuery(defineQuery, out keyCollection);
            immutableDB<int,DBElement<int,string>> imdb = new immutableDB<int, DBElement<int, string>>(db,keyCollection);
            imdb.ImmutableDB.showDB();
            WriteLine();
        }
    }
#endif
}
