using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
//using System.Reactive.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using PgnToFenCore;
using PgnToFenCore.Conversion;


namespace Anti_Cheating_software
{
    class Program
    {

		public String pgntofenexe = 
			"C:\\Users\\vzsto\\source\\repos\\PgnToFen\\PgnToFen\\bin" +
			"\\Debug\\netcoreapp3.0\\PgnToFen.exe";

		static void Main(string[] args)
        {

			try
			{
				PGN_File_Handler fileHandler = new PGN_File_Handler();

				string dir = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\PGN-format\\TEST\\";
				fileHandler.readPGN(dir);

				string Fn = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\Stockfish\\stockfish_20090216_x64.exe";
		 
				//StockfishCall sf = new StockfishCall();
				//sf.RunStockfish(Fn);
				//sf.RunStockfish(Fn, "isready uci ");
				//sf.RunStockfish(Fn, "uci");
				//sf.RunStockfish(Fn, "position startpos");
				//sf.RunStockfish(Fn, "position startpos moves e2e4");
				//sf.RunStockfish(Fn, "eval");
				Console.ReadKey();
			}catch(FileNotFoundException e)
			{
				Console.WriteLine(e.ToString()+e.FileName);
			}

		}
	}

	public class PGN_File_Handler
	{
		SqlConnection conn = new SqlConnection(@"Initial Catalog=ChessGamesDatabase");

		public String pgntofenexe = 
			"C:\\Users\\vzsto\\source\\repos\\PgnToFen" +
			"\\PgnToFen\\bin\\Debug\\netcoreapp3.0" +
			"\\PgnToFen.exe";


		string _connectionString = "";
		public PGN_File_Handler()
		{

		}

		public void readPGNs(string dir)
		{

			try
			{
				string qry = "";
				conn.Open();



				conn.Close();



				/*using (SqliteConnection cxn = new SqliteConnection(_connectionString))
				{
					cxn.Open();
					SqliteCommand cmd = new SqliteCommand(qry, cxn);
					cmd.CommandType = CommandType.Text;




				}
				*/

			}
			catch (Exception e)
			{ Console.WriteLine(e.Message); }
		}

		public List<Game> readPGN(string dir)
		{
			List<Game> Result = new List<Game>();

 
			if (Directory.Exists(dir))
			{
				// Process the list of files found in the directory.
				string[] fileEntries = Directory.GetFiles(dir);
				foreach (string fileName in fileEntries)
				{
					if (!fileName.EndsWith(".pgn"))
						continue;

					try
					{

						//var converter = new PgnToFenConverter(); 
						//var conversionStrategy = new SaveFensToListStrategy();
						//					Console.WriteLine(fileName);
						//					Console.ReadKey();
						//					converter.Convert(conversionStrategy, fileName);
					}
					catch (FileNotFoundException ee)
					{
						Console.WriteLine(ee.ToString()+ee.FileName);
					}

					Game G = new Game();
					string GMoves = "";

					string[] lines = System.IO.File.ReadAllLines(fileName);
					foreach (string line in lines)
					{
						//Console.WriteLine(line); 
						if (line.Equals(""))
						{
							if (!GMoves.Equals(""))
							{
								Result.Add(G);


								//process GMOVES into a sequence of moves
								string[] Moves = GMoves.Split(' ');
								foreach (string m in Moves)
								{
									if (m.Equals(""))
										continue;
									else if (m.EndsWith("."))
										continue;
									else
										
										G.GameMoves.Add(m.Trim());
								}

								G.GameMoves.RemoveAt(G.GameMoves.Count - 1);
								Console.WriteLine(G.GetGameInfo());
								Console.WriteLine(String.Join(" ", G.GameMoves.ToArray<string>()));

								String gamepgn = fileName
									+ "_" + (Result.Count - 1) + "_S.pgn";
								String gamefen = fileName
									+ "_" + (Result.Count - 1) + "_S.fen";

								using (System.IO.StreamWriter file =
									new System.IO.StreamWriter(gamepgn, true))
								{
									file.WriteLine(G.GetGamePNGInfo() 
										+ "\n" 
										+ String.Join(" ", G.GameMoves.ToArray<string>()));
								}


								var procpgn = new Process
								{
									StartInfo = new ProcessStartInfo
									{
										FileName = pgntofenexe,
										Arguments = " --pgnfile "
											+ gamepgn
											+ " --output "
											+ gamefen,
										UseShellExecute = false,
										RedirectStandardOutput = true,
										CreateNoWindow = true
									}
								};

								procpgn.Start();

								while (!procpgn.StandardOutput.EndOfStream)
								{
									Console.WriteLine(procpgn.StandardOutput.ReadLine());
								}

								procpgn.WaitForExit();
								procpgn.Close();


								//Starting a new game and a new set of moves
								G = new Game();

								GMoves = "";
							} 
							
							

							continue;
						}
						if (line.StartsWith("[") && line.EndsWith("]"))
						{
							string l = line.Substring(1, line.Length - 2);
							string key = l.Substring(0, l.IndexOf(" "));
							string Value = l.Substring(l.IndexOf(" ") + 1);

							if (Value.StartsWith("\"") && Value.EndsWith("\""))
								Value = Value.Substring(1, Value.Length - 2);
							switch (key)
							{

								case "Event":
									G.Event = Value;
									break;
								case "EventDate":
									G.EventDate = Value;
									break;
								case "Round":
									G.Round = Value;
									break;
								case "Result":
									G.Result = Value;
									break;
								case "White":
									if (Value.Contains(","))
									{
										G.WForename = Value.Substring(Value.IndexOf(",") + 1).Trim();
										G.WSurname = Value.Substring(0, Value.IndexOf(",")).Trim();
									}
									break;

								case "Black":
									if (Value.Contains(","))
									{
										G.BForename = Value.Substring(Value.IndexOf(",") + 1).Trim();
										G.BSurname = Value.Substring(0, Value.IndexOf(",")).Trim();
									}
									break;

								case "WhiteElo":
									G.WhiteElo = Value;
									break;

								case "BlackElo":
									G.BlackElo = Value;
									break;

								case "WhiteFideId":
									G.WhiteFideId = Value;
									break;

								case "BlackFideId":
									G.BlackFideId = Value;
									break;

								default:
									Console.WriteLine( "Unrecognised " + key);
									break;


							}


						}
						else GMoves += " " + line;
					
					}
				
				}


			}
			else
			{
				Console.WriteLine("{0} is not a valid file or directory.", dir);
			}

			return Result;


		}
}


public class StockfishCall
{
		private static StringBuilder output = null;
		private static int outputLines = 0;

		private static void OutputHandler(object sendingProcess,
			DataReceivedEventArgs outLine)
		{
			// Collect the sort command output.
			if (!String.IsNullOrEmpty(outLine.Data))
			{
				outputLines++;

				// Add the text to the collected output.
				output.Append(Environment.NewLine +
					$"[{outputLines}] - {outLine.Data}");
			}
		}

		public void RunStockfish(string Fn)
		{
			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Fn, 
					Arguments = "",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

			output = new StringBuilder();
			proc.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);

			// Redirect standard input as well.  This stream
			// is used synchronously.
			proc.StartInfo.RedirectStandardInput = true;
			proc.Start();

			// Setting up a stream writer which will be fed to stockfish and processed.
			StreamWriter streamWriter = proc.StandardInput;

			// Start the asynchronous read of the sort output stream.
			proc.BeginOutputReadLine();

			/*
			String inputText;
			int numInputLines = 0;
			do
			{
				Console.WriteLine("Enter a text line (or press the Enter key to stop):");

				inputText = Console.ReadLine();
				if (!String.IsNullOrEmpty(inputText))
				{
					numInputLines++;
					streamWriter.WriteLine(inputText);
				}
			} while (!String.IsNullOrEmpty(inputText) && (numInputLines < 2));

			Console.WriteLine("<end of input stream>");
			Console.WriteLine();
			*/

			// End the input stream to the sort command.

			streamWriter.WriteLine("isready");
			streamWriter.WriteLine("uci");
			streamWriter.WriteLine("position startpos");
			streamWriter.WriteLine("d");
			streamWriter.WriteLine("eval");


			streamWriter.Close();

			// Wait for the sort process to write the sorted text lines.
			proc.WaitForExit();

			if (outputLines > 0)
			{
				// Write the formatted and sorted output to the console.
				Console.WriteLine($" Sort results = {outputLines} sorted text line(s) ");
				Console.WriteLine("----------");
				Console.WriteLine(output);
			}
			else
			{
				Console.WriteLine(" No input lines were sorted.");
			}

			
			proc.Close();

		}

		}

	public class Game
	{
		public string Event;
		public string EventDate;
		public string Round;
		public string WSurname;
		public string WForename;
		public string White;
		public string Black;
		public string BSurname;
		public string BForename;
		public string WhiteElo;
		public string BlackElo;
		public string WhiteFideId;
		public string BlackFideId;
		public string Result;
		public List<string> GameMoves = new List<string>();

		public string GetGameInfo()
		{
			string result = "";


			result += "Event:" + Event + "\n"
		+ "EventDate:" + EventDate + "\n"
		+ "Round:" + Round + "\n"
		+ "WSurname:" + WSurname + "\n"
		+ "WForename:" + WForename + "\n"
		+ "BSurname:" + BSurname + "\n"
		+ "BForename:" + BForename + "\n"
		+ "Welo:" + WhiteElo + "\n"
		+ "Belo:" + BlackElo + "\n"
		+ "WFideID:" + WhiteFideId + "\n"
		+ "BFideID:" + BlackFideId + "\n"
		+ "Result:" + Result + "\n";


			return result;
		}
		public string GetGamePNGInfo()
		{
			string result = "";
 
			result += "[Event \"" + Event + "\"]\n"
		+ "[EventDate \"" + EventDate + "\"]\n"
		+ "[Round \"" + Round + "\"]\n"
		+ "[White \"" + WSurname+", "+ WForename + "\"]\n"
		+ "[Black \"" + BSurname+", " + BForename + "\"]\n"
		+ "[WhiteElo \"" + WhiteElo + "\"]\n"
		+ "[BlackElo \"" + BlackElo + "\"]\n"
		+ "[WhiteFideId \"" + WhiteFideId + "\"]\n"
		+ "[BlackFideId \"" + BlackFideId + "\"]\n"
		+ "[Result \"" + Result + "\"]\n";


			return result;
		}
	}





}
