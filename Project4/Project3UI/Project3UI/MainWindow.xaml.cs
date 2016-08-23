///////////////////////////////////////////////////////////////
// Project3UI.cs - define UI that shows performance info     //
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
* This package defines UI that shows performance information.
* 
*                                                                        
* Maintance:
* -------------------
* Required Files: MainWindow.xaml.cs, ICommService.cs, Receiver.cs, Sender.cs, Utilities.cs
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Project4Starter;

namespace Project3UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Receiver rcvr = null;
        string localUrl = "http://localhost:8081/CommService";
        Dictionary<string, Action<Message>> acts = new Dictionary<string, Action<Message>>();
        public MainWindow()
        {
            InitializeComponent();
            acts.Add("readClientPerf", perfClient(textBlock2, readClientBlock));
            acts.Add("writeClientPerf", perfClient(textBlock4, textBlock3));
            acts.Add("serverPerf", perfClient(textBlock3_Copy1, textBlock3_Copy));
            setChannel();
        }
        //get Receiver and Sender running
        void setChannel()
        {
            rcvr = new Receiver(Utilities.urlPort(localUrl), Utilities.urlAddress(localUrl));
            Action serviceAction = () =>
              {
                  try
                  {
                      Message msg = null;
                      while (true)
                      {
                          msg = rcvr.getMessage();
                          if (msg.content == "connection start message")
                              continue;
                          if (msg.content == "done")
                              continue;
                          if (msg.content == "closeServer")
                              break;
                          XDocument doc = XDocument.Parse(msg.content);
                          string command = doc.Descendants("Msg").Descendants("Command").ElementAt(0).Value;
                          acts[command].Invoke(msg);
                      }
                  }
                  catch
                  {
                      return;
                  }
              };
            if (rcvr.StartService())
                rcvr.doService(serviceAction);
        }
        // show the time information in the UI
        Action<Message> perfClient(TextBlock Count,TextBlock Time)
        {
            Action<Message> ReadClient = (msg) =>
               {
                   XDocument doc = XDocument.Parse(msg.content);
                   string time = doc.Descendants("Msg").Descendants("timeinfo").ElementAt(0).Value;
                   string count = doc.Descendants("Msg").Descendants("numMsg").ElementAt(0).Value;
                   Action actCount = () => { Count.Text = count; };
                   Action actTime = () => { Time.Text = time; };
                   Dispatcher.Invoke(actCount);
                   Dispatcher.Invoke(actTime);
               };
            return ReadClient;
        }
    }
}
