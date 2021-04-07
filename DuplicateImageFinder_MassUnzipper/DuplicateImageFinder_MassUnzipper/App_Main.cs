using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace DuplicateImageFinder_MassUnzipper
{
	class App_Main
	{

		public static int cpuThreads = 8;
		public static string mainFolder;
		public static string extractedFolder;
		public static bool isUnzip = false;
		public static bool isDups = false;
		public static int numUnzippers = 6;

		public static List<string> AllZips;

		public static UnzipperThread[] unzippers;
		public static Thread[] unzipThreads;

		static void Main(string[] args)
		{
			Console.WriteLine("Hello!");

			Console.WriteLine("Welcome to my unzipper and image finder.");

			//for testing:
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("arg Length = " + args.Length);
			for(int i = 0; i < args.Length; ++i)
			{
				string arg = args[i];
				Console.WriteLine("arg[" + i + "] = " + arg);
			}
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine();
			//end debugging

			//get all the arguments the user wants to input and stuff
			ParseArguments();

			// get the arguments
			if (args.Length >= 1)
			{
				
				
				
				extractedFolder = Path.Combine(mainFolder, "EXTRACTED_ZIPS");



				//do the unzipping
				if (isUnzip)
					DoTheUnzipping();

			}

		}



		

		static void ListHelpMessage()
		{
			Console.WriteLine("usage: App_Main.exe DIR_TO_SEARCH");
			Console.WriteLine("Required Options (choose at least one):");
			Console.WriteLine("[--z UNZIP]");
			Console.WriteLine("[--d DUPLICATES]");
		}

		static List<string> GetAllZipPaths(string dirToSearch)
		{
			List<string> paths = new List<string>();
			try
			{
				foreach(string f in Directory.GetFiles(dirToSearch))
				{
					//only add .zip files
					if(f.EndsWith(".zip"))
						paths.Add(f);
				}

				foreach(string d in Directory.GetDirectories(dirToSearch))
				{
					paths.AddRange(GetAllZipPaths(d));
				}
			}catch(Exception error)
			{
				Console.WriteLine(error.Message);
				return null;
			}

			return paths;
		}


		static void StartUnzippers()
		{
			unzippers = new UnzipperThread[numUnzippers];
			unzipThreads = new Thread[numUnzippers];

			int zipsPerThread = AllZips.Count / numUnzippers;

			int starterIndex = 0;
			for (int i = 0; i < numUnzippers; ++i)
			{
				//get the unzippers list of paths
				List<string> zipperZipList;
				if (i == numUnzippers-1)
				{
					int endIndex = AllZips.Count - starterIndex;//should move this to the next line
					zipperZipList = AllZips.GetRange(starterIndex, endIndex);
					starterIndex += endIndex;
				}else
				{
					zipperZipList = AllZips.GetRange(starterIndex, zipsPerThread);
					starterIndex += zipsPerThread;
				}
				
				UnzipperThread unzipper = new UnzipperThread(i, zipperZipList, extractedFolder);
				unzippers[i] = unzipper;

				ThreadStart childRef = new ThreadStart(unzipper.RunUnzipper);
				Thread newThread = new Thread(childRef);
				unzipThreads[i] = newThread;
				//start the unzipper thread
				newThread.Start();
			}

		}

		static int ZipsRemaining()
		{
			int zipsLeft = 0;
			foreach(UnzipperThread unzipper in unzippers)
			{
				zipsLeft += unzipper.GetRemainingZips();
			}

			return zipsLeft;
		}

		static bool ParseArguments()
		{
			List<string> args = Environment.GetCommandLineArgs().ToList<string>();
			args.RemoveAt(0);

			//make sure the user actually input an argument.
			if(args.Count >= 2)
			{

				mainFolder = args[0].Trim();
				Console.WriteLine("Main Folder Arg: " + mainFolder);
				//check to make sure the directory exists
				bool dirExists = Directory.Exists(mainFolder);
				if (dirExists == false)
				{
					Console.WriteLine("That directory does not exist. Please enter a valid directoy.");
					return false;
				}

				bool changedSomething = false;
				//check if they want unzip or dupes
				if (args.Contains("--z"))
                {
					isUnzip = true;
					changedSomething = true;
				}
				if (args.Contains("--d"))
                {
					isDups = true;
					changedSomething = true;
				}


				if(changedSomething == false)
                {
					Console.WriteLine("You didn't have one of the required options. Exiting program.");
					return false;
                }
					

			}
			else//else for if the args is 0
			{
				ListHelpMessage();
				return false;
			}


			return true;
		}

		static void DoTheUnzipping()
        {
			//NOTE: do the unzipping (should put this in if statement to know if user wants to do this from command line)

			Console.WriteLine("Searching File System for zips. Could take a minute...");

			List<string> zips = GetAllZipPaths(mainFolder);
			//get all the zip paths into the linked list
			AllZips = new List<string>(zips);


			Console.WriteLine("Searching Completed! Found " + zips.Count + " zip files.");

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			StartUnzippers();
			Console.WriteLine("All unzip Threads are started.");

			Console.WriteLine("Waiting for all unzipper threads to finish... This could take a very long time.");

			//wait for all the threads to be done.
			//NOTE: should switch this out for delegates and a more elegant solution, but whatever for now.
			while (ZipsRemaining() != 0)
			{
				Console.WriteLine("Progress: " + (AllZips.Count - ZipsRemaining()) + "/" + AllZips.Count + " , "
					+ ((float)((AllZips.Count - ZipsRemaining()) / (float)AllZips.Count)) * 100 + "%");
				Thread.Sleep(2500);
			}

			stopwatch.Stop();
			TimeSpan ts = stopwatch.Elapsed;

			// Format and display the TimeSpan value.
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
				ts.Hours, ts.Minutes, ts.Seconds,
				ts.Milliseconds / 10);
			Console.WriteLine("RunTime " + elapsedTime);

			Console.WriteLine("All unzip threads are done. Ending program");
		}

		
	}
}
