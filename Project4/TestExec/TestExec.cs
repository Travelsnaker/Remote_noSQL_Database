//////////////////////////////////////////////////////////////////////
// TestExec.cs - defines entry points to run server and client      //
// Ver 1.2                                                          //
// Application: Demonstration for CSE687-OOD, Project#4             //
// Language:    C#, ver 6.0, Visual Studio 2015                     //
// Platform:    lenovo Y470, Core-i3, Windows 7                     //
// Author:      Wei Sun, Syracuse University                        //
//              wsun13@syr.edu                                      //
//////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package defines the entry point for read client, write client
* and server. Users can define the number of read client and write 
* client through command line.
* 
* public interface of ClientExtension:
* ===================
* public bool startProcess(string process, string argument = null);
*
*                                                                       
* Maintance:
* -------------------
* Required Files: TestExec.cs
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
using System.Diagnostics;
using System.IO;

namespace TestExec
{
    public class ProcessStarter
    {
        // start a process, pass the argument.
        public bool startProcess(string process, string argument = null)
        {
            process = Path.GetFullPath(process);
            Console.Write("\n  fileSpec - \"{0}\"", process);
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = process,
                Arguments = argument,
                UseShellExecute = true
            };
            try
            {
                Process p = Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
        }
    }
    class TestExec
    {
        //args[0] is relative path, it is used to allow the .bat file to locate the .exe.
        //args[1] is the number of read client. 
        //arg[2] is the number of write client.
        //arg[3] determines whether messages are logged to the console in write client.
        //arg[3]="showMessage", then show sending message on the console.    
        static void Main(string[] args)
        {
            int numOfReadClient=1, numOfWriteClient=1;
            string writeClientShow=null,path="../../../";
            if (args.Length> 0)
                path = args[0];
            if (args.Length>1)
                numOfReadClient = int.Parse(args[1]);
            if (args.Length >2)
                numOfWriteClient = int.Parse(args[2]);
            if (args.Length > 3)
                writeClientShow = args[3];                      
            ProcessStarter serverDB = new ProcessStarter();
            serverDB.startProcess(string.Format("{0}DBServer/bin/debug/DBServer.exe",path));
            int count = 8082;
            for (int i=0;i<numOfWriteClient;++i)
            {                
                StringBuilder arg = new StringBuilder(string.Format("{0}{1}{2}",writeClientShow," ",count));
                ProcessStarter writeClient = new ProcessStarter();
                writeClient.startProcess(string.Format("{0}WriteClient/bin/debug/WriteClient.exe",path), arg.ToString());
                count += 2;
            }
            int Count = 8083;
            for(int i=0;i< numOfReadClient;++i)
            {
                StringBuilder arg = new StringBuilder(string.Format("{0}", Count));
                ProcessStarter readClient = new ProcessStarter();
                readClient.startProcess(string.Format("{0}ReadClient/bin/debug/ReadClient.exe",path),arg.ToString());
                Count += 2;
            }
            ProcessStarter wpf = new ProcessStarter();
            wpf.startProcess(string.Format("{0}Project3UI/Project3UI/bin/debug/Project3UI.exe",path));
            Console.Write("\n  press key to exit: ");
            Console.ReadKey();
         }
     }
}

