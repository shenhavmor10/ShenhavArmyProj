﻿// A C# Program for Server 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class Program
    {
        //globals
        static Mutex mutex=new Mutex();
        static ArrayList threadsData = new ArrayList();
        static ArrayList closeConnections = new ArrayList();
        static ArrayList errorList = new ArrayList();
        static int threadNumber=0;
        public static ArrayList GetThreadsData()
        {
            return threadsData;
        }
        /// Function - CloseConnection
        /// <summary>
        /// Close the connection for the client number - "index" and send it the error - "error"
        /// </summary>
        /// <param name="index"> client number</param>
        /// <param name="error"> error type</param>
        public static void CloseConnection(int index,string error)
        {
            //mutex.
            mutex.WaitOne();
            closeConnections[index] = 1;
            errorList[index] = error;
            mutex.ReleaseMutex();
        }
        /// <summary>
        /// Starts connection with the clientSocket
        /// </summary>
        /// <param name="clientSocket"> the socket of the client.</param>
        public static void StartConnection(Socket clientSocket)
        {
            bool stopWhile = false;
            while (true&&!stopWhile)
            {
                // Data buffer 
                byte[] bytes = new Byte[1024];

                string data;
                int numByte = clientSocket.Receive(bytes);

                data = Encoding.ASCII.GetString(bytes,
                                            0, numByte);

                if (data.IndexOf("<EOF>") > -1)
                    break;
                //add to the closeConnection ArrayList 0 in the index of the client. (0 means dont stop 1 means stop).
                closeConnections.Add(0);
                //just create the error for the client in case it has an error.
                errorList.Add("");
                //the number of the client is the current thread number.
                int currentThread = threadNumber;
                threadNumber++;
                //adds the data it got fromt he client to the threads data. so the MainProgram can take the data and use it for itself.
                threadsData.Add(data);
                //mutex for closeConnections.
                mutex.WaitOne();
                while ((int)closeConnections[currentThread] == 0)
                {
                    mutex.ReleaseMutex();
                    Thread.Sleep(1000);
                    mutex.WaitOne();

                    // Send a message to Client  
                    // using Send() method 
                }
                mutex.ReleaseMutex();
                //take the error for the client and send it back.

                //right here will be an if for close connections if it will be 1 it will take the error of the client if it will 
                //be 2 it will take the path for the new file (will be created).
                byte[] message = Encoding.ASCII.GetBytes("Error - "+(string)errorList[currentThread]);
                clientSocket.Send(message);
                
                // Close client Socket using the 
                // Close() method. After closing, 
                // we can use the closed Socket  
                // for a new Client Connection 
                clientSocket.Close();
                stopWhile = true;
            }
            
        }
        public static void ExecuteServer()
        {
            // Establish the local endpoint  
            // for the socket. Dns.GetHostName 
            // returns the name of the host  
            // running the application. 
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);
            Socket listener = new Socket(ipAddr.AddressFamily,
                                     SocketType.Stream, ProtocolType.Tcp);
            // Using Bind() method we associate a 
            // network address to the Server Socket 
            // All client that will connect to this  
            // Server Socket must know this network 
            // Address 
            listener.Bind(localEndPoint);

            // Using Listen() method we create  
            // the Client list that will want 
            // to connect to Server 
            // Creation TCP/IP Socket using  
            // Socket Class Costructor 
            
            while (true)
            {
                listener.Listen(10);
                Console.WriteLine("Waiting connection ... ");
                // Suspend while waiting for 
                // incoming connection Using  
                // Accept() method the server  
                // will accept connection of client 
                Socket clientSocket = listener.Accept();
                Console.WriteLine("Accepted");
                
                /*try
                {
                    clientSocket = listener.Accept();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }*/
                Thread serverThread;
                serverThread = new Thread(() => StartConnection(clientSocket));
                serverThread.Start();

            }
            

            
        }
        // Main Method 
        public static void Main(string[] args)
        {
            ExecuteServer();
        }
    }
}