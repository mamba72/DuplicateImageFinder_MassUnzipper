using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

namespace DuplicateImageFinder_MassUnzipper
{
	public class UnzipperThread
	{

		int threadNum = -1;
		LinkedList<string> ZipPaths;
		string extractToPath;
		//this is just a tracker to allow us to rename the zips
		int unzippedNumber = 0;

		/// <summary>
		/// The constructor for the unzipping object
		/// </summary>
		/// <param name="threadNum"></param>
		/// <param name="pathsToUnzip"></param>
		/// <param name="dirToWriteTo"></param>
		public UnzipperThread(int threadNum, List<string> pathsToUnzip,string dirToWriteTo)
		{
			this.threadNum = threadNum;
			this.ZipPaths = new LinkedList<string>(pathsToUnzip);
			this.extractToPath = dirToWriteTo + "__" + threadNum;
			unzippedNumber = threadNum;

		}

		public string UnzipFile(string zipPath, string extractToPath)
		{
			//Console.WriteLine("\tZipPath: " + zipPath + "\n\tExtractPath: " + extractToPath);
			try
			{
				System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractToPath, true);
			}catch(Exception error)
			{
				Console.WriteLine(error.Message);
			}
			
			++unzippedNumber;
			return extractToPath;
		}

		public int GetRemainingZips()
		{
			return ZipPaths.Count;
		}

		/// <summary>
		/// This function is going to be one of the worker functions that actually does the unzipping
		/// </summary>
		public void RunUnzipper()
		{
			Console.WriteLine("Unzipper " + threadNum + " has Started.");
			

			//create the directory
			DirectoryInfo dirInfo = Directory.CreateDirectory(extractToPath);
			//iterate through the zips, extracting them and ensuring we correct the path
			int zipCount = ZipPaths.Count;
			for(int i = 0; i < zipCount; ++i)
			{
				string curPath = ZipPaths.First();
				string fileName = Path.GetFileNameWithoutExtension(curPath);

				string outputPath = Path.Combine(extractToPath, fileName + "_" + unzippedNumber);

				

				//Console.ForegroundColor = ConsoleColor.Yellow;
				//Console.WriteLine("Thread " + threadNum + " processing File with path " + curPath);
				//Console.ForegroundColor = ConsoleColor.White;

				UnzipFile(curPath, outputPath);


				//Console.ForegroundColor = ConsoleColor.Yellow;
				//Console.WriteLine("Thread " + threadNum + " Writing File to path " + outputPath);
				//Console.ForegroundColor = ConsoleColor.White;

				//Thread.Sleep(250);
				ZipPaths.RemoveFirst();

				

				//return;//this is for testing a single unzip file.
			}





			// finished
			Console.WriteLine("Unzipper " + threadNum + " has finished.");

			
		}


	}
}
