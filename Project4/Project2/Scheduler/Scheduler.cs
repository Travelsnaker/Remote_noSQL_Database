///////////////////////////////////////////////////////////////
// Scheduler.cs - Define Scheduler for noSQL database        //
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
* This package is used to accept a positive time interval or number of 
* writes after which the database contents are persisted. It sheduled 
* "save" process until cancelled.
* 
* public interface:
* ===================
* void autoSave(PersistWapper<Key, Value> persist, double interval,string XmlFile);
* void cancel();
*
* Maintance:
* -------------------
* Required Files: Scheduler.cs, DBElement.cs, DBEngine.cs, PersistEngine.cs
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
using System.Timers;
using static System.Console;

namespace Project2
{
    public class Schedular<Key, Value>
    {
        private Timer Schedule = new Timer();
        // provide function to set the time interval and the file it should persist
        public void autoSave(PersistWapper<Key, Value> persist, double interval,string XmlFile)
        {
            Schedule.AutoReset = true;
            Schedule.Interval = interval;
            Schedule.Enabled = true;
            if (persist.persistDB.Keys().Count() > 0)
            {
                Schedule.Elapsed += (object source, ElapsedEventArgs e) =>
                {
                    Console.Write("\n  persist occurred at {0}", e.SignalTime);
                    persist.writeToXML(XmlFile);

                };
            }
        }
        // Cancel the scheduler
        public void cancel()
        {
            Schedule.Enabled = false;
        }
    }

#if (TEST_SCHEDULER)
    class TestScheduler
    {
        static void Main(string[] args)
        {
            
            DBElement<int, string> elem1 = new DBElement<int, string>();
            elem1.descr = "payload desxription";
            elem1.name = "element 1";
            elem1.timeStamp = DateTime.Now;
            elem1.payload = "a payload";
            DBElement<int, string> elem2 = new DBElement<int, string>("Darth Vader", "Evil Overlord");
            elem2.descr = "star war 2";
            elem2.name = "element 2";
            elem2.timeStamp = new DateTime(2015, 9, 10, 12, 30, 1);
            elem2.payload = "The Empire strikes back!";
            var elem3 = new DBElement<int, string>("Luke Skywalker", "Young HotShot");
            elem3.name = "element 3";
            elem3.descr = "star war 3";
            elem3.timeStamp = new DateTime(2015, 10, 2, 8, 0, 0);
            elem3.children = new List<int> { 1, 2, 3 };
            elem3.payload = "X-Wing fighter in swamp - Oh oh!";

            
            int key = 0;
            Func<int> keyGen = () => { ++key; return key; };  // anonymous function to generate keys

            DBEngine<int, DBElement<int, string>> db = new DBEngine<int, DBElement<int, string>>();
            bool p1 = db.insert(keyGen(), elem1);
            bool p2 = db.insert(keyGen(), elem2);
            bool p3 = db.insert(keyGen(), elem3);
            WriteLine("--- Test Scheduler ---");
            PersistWapper<int, DBElement<int, string>> persist = new IntAndString(db);           
            Schedular<int, DBElement<int,string>> test = new Schedular<int, DBElement<int, string>>();
            test.autoSave(persist, 1000,"DatabaseContent.xml");
            ReadKey();
        }
    }
#endif
}
