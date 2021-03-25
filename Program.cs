using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Reactive.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;

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

                //creating handlers from each class:

                //Used for converting pgn files to fen files
                PGN_File_Handler fileHandler = new PGN_File_Handler();
                //Used for creating and populating the database
                DatabaseHandler DBHandler = new DatabaseHandler();
                //Used to perform statistical analysis on both 
                // the large data set and the test data
                DataAnalysis DataHandler = new DataAnalysis();


                string dir = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\FEN-format\\";
                //fileHandler.readPGN(dir);

                //The location of the Stockfish .exe file for use in functions,
                //to analyse each FEN file. 
                string Fnexe = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\Stockfish\\stockfish_20090216_x64.exe";

                

                

                //fileHandler.readPGN(dir);

                //Creating an object to call Stockfish
                StockfishCall sf = new StockfishCall();


                //sf.RunStockfishAll(Fnexe, dir);
                //sf.TestStatOnFiles(dir);
                //DBHandler.ProcessGames(dir);
                //DataHandler.StatisticsCollection();

                //Test set processing
                //defining the directories of each of the test sets, ready to be processed.
                string TestDir1 = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\Test-1\\";
                string TestDir2 = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\Test-2\\";
                string TestDir3 = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\Test-3\\";
                string TestDir4 = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\Test-4\\";
                string TestDir5 = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\Test-5\\";
                string TestDir6 = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\Test-6\\";

                //Creating a table which will contain the summary statistics for each
                //of the rating ranges
                string inFn3 = "C:\\Users\\vzsto\\Documents\\COMPSCI_Project\\Final_Data.tsv";

                TestingGames GameTester = new TestingGames(inFn3);


                //1.
                fileHandler.PgnFenConversion(TestDir6);
                //2.
                sf.RunStockfishAll(Fnexe, TestDir6);
                //3.
                sf.TestStatOnFiles(TestDir6);
                //4.
                Console.WriteLine(GameTester.EvaluateGames(TestDir6, "Player, Test"));

                //DataHandler.StatisticsCollection(inFn3);

               

                string outFn4 = "C:\\Users\\vzsto\\Documents\\" +
                    "COMPSCI_Project\\Data_2100.tsv";

                //DataHandler.getStatisticsForRange(2100, outFn4);



                Console.ReadKey();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.ToString() + e.FileName);
            }



        }
    }
    public class DatabaseHandler
    {
        public void ProcessGames(string dir)
        {

            string RelatedPGNfile = "";
            try
            {
                // Creating a SQLite connection so we can populate the databases
                SqliteConnection conn = new SqliteConnection(
                    "Data Source=C:\\Users\\vzsto\\source\\repos" +
                    "\\Anti-Cheating_software\\ChessGamesDatabase.db");

                conn.Open();

                if (Directory.Exists(dir))
                {
                    int ctr = 0;
                    // Processing the list of files found in the directory.
                    string[] fileEntries = Directory.GetFiles(dir);
                    foreach (string fileName in fileEntries)
                    {
                        if (!fileName.EndsWith("_calc.tsv"))
                            continue;

                        //Finds the corresponding pgn file, so that it can take 
                        //its metadata and put it in the database.
                        RelatedPGNfile = fileName.Replace(".fen_Eval.tsv_calc.tsv", ".pgn");

                        if (!File.Exists(RelatedPGNfile)) 
                            continue;

                        //Loading a new game from the pgn file.
                        Game G = new Game(RelatedPGNfile);
 
                        //Ignoring games which contain players without ELO ratings
                        if (G.WhiteElo.Equals("")) continue;
                        
                        //Generating a unique primary key (PlayerID) for this player in the table Players
                        string WhiteKey = G.WSurname + "_" + G.WForename + "_" + G.WhiteElo;
                        
                        //Performing SQL commands so that primary keys are not duplicated.
                        string CheckCommandTextWhite = "SELECT * FROM Players WHERE PlayerID = " + "'" + WhiteKey + "'";
                        SqliteCommand CheckCommandWhite = new SqliteCommand(CheckCommandTextWhite, conn);
                        SqliteDataReader CheckReaderWhite = CheckCommandWhite.ExecuteReader();

                        // Populating the table if the player is not already in it.
                        if (!CheckReaderWhite.Read())
                        {
                            string CommandText = "INSERT INTO Players" +
                                "(PlayerID,Surname,Forename,ELO) " +
                            "VALUES ("
                            + "'" + WhiteKey + "'" + ", "
                            + "'" + G.WSurname + "'" + ", "
                            + "'" + G.WForename + "'" + ", "
                            + G.WhiteElo
                            + ");";
                            Console.WriteLine(CommandText);
                            SqliteCommand cmd1 = new SqliteCommand(CommandText, conn);
                            cmd1.ExecuteNonQuery();
                            Console.WriteLine(RelatedPGNfile);
                            Console.WriteLine(CommandText);
                            
                        }

                        //Same sequence of procedures for black as outlined above for white.
                        if (G.BlackElo.Equals("")) continue;
                        string BlackKey = G.BSurname + "_" + G.BForename + "_" + G.BlackElo;
                        string CheckCommandTextBlack = "SELECT * FROM Players WHERE PlayerID = " + "'" + BlackKey + "'";
                        SqliteCommand CheckCommandBlack = new SqliteCommand(CheckCommandTextBlack, conn);
                        SqliteDataReader CheckReaderBlack = CheckCommandBlack.ExecuteReader();

                        if (!CheckReaderBlack.Read())
                        {
                            string CommandText = "INSERT INTO Players " +
                                "(PlayerID, Surname, Forename, ELO) " +
                            "VALUES ("
                            + "'" + BlackKey + "'" + ", "
                            + "'" + G.BSurname + "'" + ", "
                            + "'" + G.BForename + "'" + ", "
                            + G.BlackElo
                            + ");";
                            Console.WriteLine(CommandText);
                            SqliteCommand cmd2 = new SqliteCommand(CommandText, conn);
                            cmd2.ExecuteNonQuery();
                            Console.WriteLine(RelatedPGNfile);
                           
                        }

                        //initialising the test statistics for white and black.
                        string WhiteCalcs = "";
                        string BlackCalcs = "";

                        int LineNumber = 0;

                        string[] CalcLines = System.IO.File.ReadAllLines(fileName);

                        //For odd numbered lines, adding the test statistic to WhiteCalcs 
                        //and for even numbered lines, adding it to BlackCalcs
                        foreach (string line in CalcLines)
                        {
                            LineNumber++;
                            if (line.Equals("")) continue;
                            if (LineNumber % 2 == 1)
                                WhiteCalcs += line + " ";
                            else if (LineNumber % 2 == 0)
                                BlackCalcs += line + " ";
                        }


                            //Populating the Games database with all of the data
                            //obtained from the pgn file
                            string GamesCommandText = "INSERT INTO Games " +
                            "(GameID, FileName, Event, EventDate, Round," +
                            " White, Black, WhiteElo, BlackElo, Result, Moves, WhiteStats, BlackStats) " +
                        "VALUES ("
                        + (ctr++) + ", "
                        + "'" + fileName + "'" + ", "
                        + "'" + G.Event + "'" + ", "
                        + "'" + G.EventDate + "'" + ", "
                        + "'" + G.Round + "'" + ", "
                        + "'" + WhiteKey + "'" + ", "
                        + "'" + BlackKey + "'" + ", "
                        + "'" + G.WhiteElo + "'" + ", "
                        + "'" + G.BlackElo + "'" + ", "
                        + "'" + G.Result + "'" + ", "
                        + "'" + G.GMoves + "'" + ", "
                        + "'" + WhiteCalcs + "'" + ","
                        + "'" + BlackCalcs + "'"
                        + ");";

                        Console.WriteLine(GamesCommandText);
                        SqliteCommand cmd3 = new SqliteCommand(GamesCommandText, conn);
                        cmd3.ExecuteNonQuery();
                        Console.WriteLine(RelatedPGNfile);
                        

                    }


                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", dir);
                }

                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(RelatedPGNfile);
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }

            


        }
    }
    public class PGN_File_Handler
    {
        //Defining the path of the pgn to fen conversion executable.
        public string pgntofenexe =
            "C:\\Users\\vzsto\\source\\repos\\PgnToFen" +
            "\\PgnToFen\\bin\\Debug\\netcoreapp3.0" +
            "\\PgnToFen.exe";





        public List<Game> PgnFenConversion(string dir)
        {
            List<Game> Result = new List<Game>();

            try
            {

                if (Directory.Exists(dir))
                {
                    // Processes the list of files found in the directory - converts all pgn's to fen's
                    string[] fileEntries = Directory.GetFiles(dir);
                    foreach (string fileName in fileEntries)
                    {
                        if (!fileName.EndsWith(".pgn"))
                            continue;

                        
                        //Creating a new object Game 
                        // and filling in the details from the file.
                        Game G = new Game();
                        
                        string[] lines = System.IO.File.ReadAllLines(fileName);
                        foreach (string line in lines)
                        {
                            if (line.Equals(""))
                            {
                                if (!G.GMoves.Equals(""))
                                {
                                    Result.Add(G);

                                   

                                    //Transforms GMoves into a sequence of moves
                                    //by splitting it up into the separate moves.
                                    string[] Moves = G.GMoves.Split(' ');
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
                                    Console.WriteLine(string.Join(" ", G.GameMoves.ToArray<string>()));

                                    string gamepgn = fileName
                                        + "_" + (Result.Count - 1) + "_S.pgn";
                                    string gamefen = fileName
                                        + "_" + (Result.Count - 1) + "_S.fen";

                                    //Separates a pgn file containing multiple games into 
                                    //its constituent games
                                    using (System.IO.StreamWriter file =
                                        new System.IO.StreamWriter(gamepgn, true))
                                    {
                                        file.WriteLine(G.GetGamePGNInfo()
                                            + "\n"
                                            // Outputs the moves as a string.
                                            + String.Join(" ", G.GameMoves.ToArray<string>()));
                                    }

                                    
                                    if (File.Exists(gamefen))
                                    {
                                        File.Delete(gamefen);
                                    }


                                    //Calling the external program defined by the path pgntofenexe.
                                    try
                                    {

                                        //Creates a process to handle communication with the external program 
                                        //at the location pgntofenexe
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
                                            Console.ReadKey();
                                        }



                                        procpgn.WaitForExit();
                                        procpgn.Close();

                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                        Console.ReadKey();
                                    }
                                    //Starting a new game and a new set of moves,
                                    //ready for the next loop
                                    G = new Game();

                                    
                                }



                                continue;
                            }

                            //Reads and processes the information which is then used to create
                            //the individual games with their game data and moves
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
                                        Console.WriteLine("Unrecognised " + key);
                                        break;


                                }


                            }
                            else G.GMoves += " " + line;

                        }

                    }


                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", dir);
                }

            }
            catch (Exception e)
            { Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }

            return Result;


        }
    }

    public class DataAnalysis
    {
        //Creating an object with which functions in the class DataProcessor can be executed.
        DataProcessor dp = new DataProcessor();

        //Defining dictionaries which are used to store all of the collected data
        Dictionary<int, List<double>> TestStatDict = new Dictionary<int, List<double>>();
        Dictionary<int, List<double>> FinalTestDict = new Dictionary<int, List<double>>();

        //Collects the statistics and will output them to the file OutputFile
        public void StatisticsCollection(string OutputFile)
        {
            //Creating a stream writer which is used to fill the file OutputFile
            StreamWriter OutputFileWriter = new StreamWriter(OutputFile);

            //Establishing a connection with the database so that it can be read from
            //and the elo ratings taken, using the foreign keys from Players which are present
            //in Games.
            SqliteConnection conn = new SqliteConnection(
                    "Data Source=C:\\Users\\vzsto\\source\\repos" +
                    "\\Anti-Cheating_software\\ChessGamesDatabase.db");

            conn.Open();

            //Looping through all of the lower bounds on the ratings.
            for (int i = 1600; i < 2900; i = i + 100)
            {
                //Adding the keys to the dictionary
                TestStatDict.Add(i, new List<double>());
                FinalTestDict.Add(i, new List<double>());

                //Extracting the records which are inside the relevant rating range.
                string RecordExtraction = "SELECT PlayerID FROM Players WHERE ELO BETWEEN " + i + " AND " + (i + 100);
                SqliteCommand GetPlayersCommand = new SqliteCommand(RecordExtraction, conn);
                SqliteDataReader GetPlayersReader = GetPlayersCommand.ExecuteReader();

                //Going through the output, as long as there are still records being read, the 
                //stats are taken for each player.
                while (GetPlayersReader.Read())
                {


                    string name = GetPlayersReader.GetString(0);
                    Console.WriteLine(name);
                    string TestStatExtractionWhite = "SELECT WhiteStats FROM Games WHERE White = '" + name + "'";
                    SqliteCommand TestStatExtractionCommandWhite = new SqliteCommand(TestStatExtractionWhite, conn);
                    SqliteDataReader TestStatWhiteReader = TestStatExtractionCommandWhite.ExecuteReader();


                    while (TestStatWhiteReader.Read())
                    {
                        //Reads the first (0th) element in each row (the second being the FEN)
                        string Stats = TestStatWhiteReader.GetString(0);

                        //Calculates the mean of all the test statistics
                        double Mean = dp.MeanCalculator(Stats);

                        //Skips places where the mean is less than zero so that
                        //it doesn't mess up the final data..
                        if (Mean < 0) continue;
                        Console.WriteLine(Mean);
                        TestStatDict[i].Add(Mean);

                    }

                    //A similar set of procedures is carried out for games where the player played with
                    //the black pieces, as outlined above for their white games.
                    string TestStatExtractionBlack = "SELECT BlackStats FROM Games WHERE Black = '" + name + "'";
                    SqliteCommand TestStatExtractionCommandBlack = new SqliteCommand(TestStatExtractionBlack, conn);
                    SqliteDataReader TestStatBlackReader = TestStatExtractionCommandBlack.ExecuteReader();


                    while (TestStatBlackReader.Read())
                    {
                        string Stats = TestStatBlackReader.GetString(0);
                        //Console.WriteLine(Stats);
                        double Mean = dp.MeanCalculator(Stats);
                        if (Mean < 0) continue;
                        Console.WriteLine(Mean);
                        TestStatDict[i].Add(Mean);
                    }




                }


                if (TestStatDict[i].Count() > 0)
                {
                    double FinalSum = 0.0f;
                    foreach (double Mean in TestStatDict[i])
                    {
                        FinalSum += Mean;
                    }
                    //Calculating the mean test statistic for each rating range
                    //and storing it in the dictionary
                    double FinalMean = FinalSum / TestStatDict[i].Count();
                    FinalTestDict[i].Add(FinalMean);

                    //Calculating the standard deviation of the test statistics for each rating range
                    //and putting that into the dictionary
                    double FinalSquareSum = 0.0f;
                    foreach (double Mean in TestStatDict[i])
                    {
                        FinalSquareSum += Math.Pow(Mean, 2);
                    }
                    double StandardDeviation = Math.Sqrt(FinalSquareSum / TestStatDict[i].Count() - Math.Pow(FinalMean, 2));
                    FinalTestDict[i].Add(StandardDeviation);

                    //Writing the rating lower bound, the mean and the standard deviation into the file
                    //using the stream writer
                    OutputFileWriter.WriteLine(i + "\t" + Convert.ToString(FinalMean)
                        + "\t" + Convert.ToString(StandardDeviation));

                    OutputFileWriter.Flush();

                }
            }


            conn.Close();
            OutputFileWriter.Close();



        }



        //Outputs the statistics for a single ELO range group.
        public void getStatisticsForRange(int i, string OutputFile)
        {
            //Creating a stream writer which is used to fill the file OutputFile
            StreamWriter OutputFileWriter = new StreamWriter(OutputFile);

            //Establishing a connection with the database so that it can be read from
            //and the elo ratings taken, using the foreign keys from Players which are present
            //in Games.
            SqliteConnection conn = new SqliteConnection(
                    "Data Source=C:\\Users\\vzsto\\source\\repos" +
                    "\\Anti-Cheating_software\\ChessGamesDatabase.db");

            conn.Open();

            
                 //Adding the keys to the dictionary
                TestStatDict.Add(i, new List<double>());
                FinalTestDict.Add(i, new List<double>());

                //Extracting the records which are inside the relevant rating range.
                string RecordExtraction = "SELECT PlayerID FROM Players WHERE ELO BETWEEN " + i + " AND " + (i + 100);
                SqliteCommand GetPlayersCommand = new SqliteCommand(RecordExtraction, conn);
                SqliteDataReader GetPlayersReader = GetPlayersCommand.ExecuteReader();

                //Going through the output, as long as there are still records being read, the 
                //stats are taken for each player.
                while (GetPlayersReader.Read())
                {


                    string name = GetPlayersReader.GetString(0);
                    Console.WriteLine(name);
                    string TestStatExtractionWhite = "SELECT WhiteStats FROM Games WHERE White = '" + name + "'";
                    SqliteCommand TestStatExtractionCommandWhite = new SqliteCommand(TestStatExtractionWhite, conn);
                    SqliteDataReader TestStatWhiteReader = TestStatExtractionCommandWhite.ExecuteReader();


                    while (TestStatWhiteReader.Read())
                    {
                        //Reads the first (0th) element in each row (the second being the FEN)
                        string Stats = TestStatWhiteReader.GetString(0);

                        //Calculates the mean of all the test statistics
                        double Mean = dp.MeanCalculator(Stats);

                        //Skips places where the mean is less than zero so that
                        //it doesn't mess up the final data..
                        if (Mean < 0) continue;

                    OutputFileWriter.WriteLine(Mean);

                    OutputFileWriter.Flush();

                }

                    //A similar set of procedures is carried out for games where the player played with
                    //the black pieces, as outlined above for their white games.
                    string TestStatExtractionBlack = "SELECT BlackStats FROM Games WHERE Black = '" + name + "'";
                    SqliteCommand TestStatExtractionCommandBlack = new SqliteCommand(TestStatExtractionBlack, conn);
                    SqliteDataReader TestStatBlackReader = TestStatExtractionCommandBlack.ExecuteReader();


                    while (TestStatBlackReader.Read())
                    {
                        string Stats = TestStatBlackReader.GetString(0);
                        //Console.WriteLine(Stats);
                        double Mean = dp.MeanCalculator(Stats);
                        if (Mean < 0) continue;

                        OutputFileWriter.WriteLine(Mean);

                        OutputFileWriter.Flush();
                    }




            }


            conn.Close();
            OutputFileWriter.Close();



        }


    }
    }


    public class DataProcessor{

        //A function which creates the mean test statistic for a single game
        public double MeanCalculator(string GameStats)
        {
            double SUM = 0;
            string[] GameMoveStats = GameStats.Split(' ');


            if (GameMoveStats.Count() == 0)
                return -1;

            foreach (string num in GameMoveStats)
            {
                try
                {
                    double value = Convert.ToDouble(num);
                    SUM += value;
                }
                catch (Exception e)
                { }

            }
            double Mean = SUM / GameMoveStats.Count();
            return Mean;
        }

        
    
    
    }

    public class TestingGames
    {
        Dictionary<int, List<double>> TestGamesStatsDict = new Dictionary<int, List<double>>();

        //Defining a constructor to get the data for evaluation.
        public TestingGames(string DatafileName) 
        {
            try
            {
                //Loading data file into dictionary
                string[] lines = System.IO.File.ReadAllLines(DatafileName);
                foreach (string line in lines)
                {
                    
                    //Returns the file contents to the dictionary.
                    string[] Data = line.Split('\t');
                    int LowerBoundary = Convert.ToInt32(Data[0]);
                    TestGamesStatsDict.Add(LowerBoundary, new List<double>());

                    TestGamesStatsDict[LowerBoundary].Add(Convert.ToDouble(Data[1]));
                    TestGamesStatsDict[LowerBoundary].Add(Convert.ToDouble(Data[2]));
                }
            }
            catch(Exception e)
            { Console.WriteLine(e.Message); }
        }

        public string EvaluateGames(string dir, string PlayerName)
        {
            string result = "";

            
            double total = 0.0f;
            //Console.WriteLine("Analysing player" + PlayerName + "'s games...");

            //Creating a object of type DataProcessor which is used to call functions
            //from the DataProcessor class
            DataProcessor dp = new DataProcessor();
            int elo = 0;
            try
            {
                string[] fileEntries = Directory.GetFiles(dir);
                foreach (string fileName in fileEntries)
                {

                    if (!fileName.EndsWith("_calc.tsv"))
                        continue;
                    //Finds the corresponding pgn file.
                    string RelatedPGNfile = fileName.Replace(".fen_Eval.tsv_calc.tsv", ".pgn");

                    if (!File.Exists(RelatedPGNfile))
                        continue;

                    //Loading the associated pgn file into G.
                    Game G = new Game(RelatedPGNfile);

                    string PlayerCalcs = "";

                    int LineNumber = 0;
                    string[] CalcLines = System.IO.File.ReadAllLines(fileName);


                    //Taking the appropriate elo rating and test statistics, depending on whether the
                    //test player played white or black.
                    if (PlayerName.Equals(G.WSurname + ", " + G.WForename))
                    {
                        elo = G.getELOWhite();

                        
                        foreach (string line in CalcLines)
                        {
                            LineNumber++;
                            if (line.Equals("")) continue;
                            if (LineNumber % 2 == 1)
                                PlayerCalcs += line + " ";

                        }

                    }
                    if (PlayerName.Equals(G.BSurname + ", " + G.BForename))
                    {
                        elo = G.getELOBlack();

                        foreach (string line in CalcLines)
                        {
                            LineNumber++;
                            if (line.Equals("")) continue;
                            if (LineNumber % 2 == 0)
                                PlayerCalcs += line + " ";
                        }

                    }
                    Console.WriteLine(elo);
                    Console.WriteLine(PlayerCalcs);

                if (elo == 0) continue;

                    //Calculating the mean test statistic per move for the game in question.
                    total += dp.MeanCalculator(PlayerCalcs);

                }

                //Calculating the mean test statistic per move for the entire set of games for the player.
                total = total / fileEntries.Count();

                int eloLowerBound = (elo / 100) * 100;

                Console.WriteLine(eloLowerBound);
                Console.WriteLine("Elo of player: " + elo);
                Console.ReadKey();

                //Comparing the mean test statistic per move for the test player to the expected one
                //in the dictionary, so as to make a conclusion on the legitimacy of the player's play.
                if (TestGamesStatsDict[eloLowerBound][0] - total < TestGamesStatsDict[eloLowerBound][0] - 0.5 * TestGamesStatsDict[eloLowerBound][1])
                    result = "Potential flag - Another such result from this player will require human intervention";
                if (TestGamesStatsDict[eloLowerBound][0] - total < TestGamesStatsDict[eloLowerBound][0] - TestGamesStatsDict[eloLowerBound][1])
                    result = "Flag - Requires urgent human intervention";
                else result = "Legitimate games";
            }
            catch (Exception e)
            { 
                
                
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
            return result;
            
        }
    }


    public class StockfishCall
    {
        private static StringBuilder output = null;
        private static int outputLines = 0;

        //A function which handles the output given from stockfish
        private static void OutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                outputLines++;

                //Adds the text to the collected output.
                output.Append(Environment.NewLine +
                    $"[{outputLines}] - {outLine.Data}");
            }
        }



        

        


        //Function to run stockfish on all fen files in a given directory
        public void RunStockfishAll(string Fnexe, string dir)
        {
            if (Directory.Exists(dir))
            {
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(dir);
                foreach (string fileName in fileEntries)
                {
                    if (!fileName.EndsWith(".fen")) continue;

                    try
                    {

                        //Runs Stockfish on every file individually.
                        RunStockfish(Fnexe, fileName);
  
                       

                    }
                    catch (FileNotFoundException ee)
                    {
                        Console.WriteLine(ee.ToString() + ee.FileName);
                        Console.WriteLine(ee.StackTrace);
                        Console.ReadKey();
                    }


                }



            }

        }

        //Function which runs stockfish on a single game.
        public void RunStockfish(string Fn, string inFn)
        {
            try
            {
                //Creats a file into which the evaluations of all of the FENs will be written
                string outFile = inFn + "_Eval.tsv";
                if (File.Exists(outFile))
                    return;


                ArrayList FENs = new ArrayList();
                ArrayList Evals = new ArrayList();

                //Creating a process with its parameters
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

                proc.StartInfo.RedirectStandardInput = true;

                proc.Start();

                // Setting up a stream writer which will be fed to stockfish
                StreamWriter streamWriter = proc.StandardInput;


                streamWriter.AutoFlush = true;


                // Start the reading of the output stream.
                StreamReader streamReader = proc.StandardOutput;

                //Initialising Stockfish
                streamWriter.WriteLine("isready");
                streamWriter.WriteLine("uci");
                streamWriter.Flush();


                //reads the file line by line
                string[] lines = System.IO.File.ReadAllLines(inFn);

                //Removing the first 12 moves for white and black (total of 24 moves)
                //We need to read one move back so we can feed it as previousEval in the 
                //test statistic calculator function.
                for (int k = 23; k < lines.Length; k++)
                {

                    string line = lines[k];
                    Console.WriteLine(line);

                    if (string.IsNullOrEmpty(line)) continue;


                    var task = Task.Run(() =>
                    {
                        //Writes onto the Command line so that Stockfish can be used.
                        streamWriter.WriteLine("position fen " + line);
                        FENs.Add(line);

                        streamWriter.WriteLine("eval");
                        streamWriter.Flush();
                    });


                    TimeSpan ts = TimeSpan.FromMilliseconds(5000);
                    if (!task.Wait(ts))
                    {
                        Console.WriteLine("The timeout interval elapsed.");
                    }

                }


                streamWriter.Flush();

           //     Console.ReadKey();


                string evaltype = "Final evaluation:";

                while (streamReader.Peek() != -1)
                {
                    string L = streamReader.ReadLine();

                    
                    if (L.StartsWith(evaltype))
                    {

                        int diff = L.Length - evaltype.Length - 13;
                        string eS = L.Substring(evaltype.Length + 1, diff);

                        
                        //If Stockfish processes the move incorrectly, a large value is put in instead
                        // In order to be noticeable later and can be removed.
                        if (eS.StartsWith("non")) 
                            Evals.Add(-1999);
                        else Evals.Add(Convert.ToDouble(eS));
                    }

                    long milliseconds = DateTime.Now.Ticks;
                    while (DateTime.Now.Ticks - milliseconds < 5000) ;


                }






                FileStream fileStream = new FileStream(outFile, FileMode.OpenOrCreate);
                StreamWriter fileWriter = new StreamWriter(fileStream);

                for (int j = 0; j < Evals.Count; j++)
                {
                    fileWriter.WriteLine(Evals[j] + "\t" + FENs[j]);
                    fileWriter.Flush();
                }

           

               

                streamWriter.Flush();
                streamWriter.Close();


                streamReader.Close();
                fileWriter.Flush();
                fileWriter.Close();
                fileStream.Close();

                // Wait for the process
                proc.WaitForExit();
                proc.Close();
                proc.Dispose();


            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }



        }



        //Function which calculates test statistics for individual moves
        public double TestStatisticCalculator(double EvalBest, double EvalPlayed)
        {
            //Making the evaluations containing errors more noticable so they can be removed.
            if (EvalPlayed == -1999 || EvalPlayed == 1999 || EvalBest == -1999 || EvalBest == 1999)
                return -1000000;

            else if (EvalPlayed > EvalBest)
            {
                EvalPlayed = EvalBest;
                return 0;
            }
            else
            {
                //Dealing with cases where the boundary of discontinuity is reached or crossed.
                if (EvalPlayed < -1 && EvalBest > -1)
                {
                    double TestStatistic = Math.Log((EvalBest + 1) / (-0.99 + 1)) + Math.Abs(Math.Log(Math.Abs((-1.01 + 1) / (EvalPlayed + 1))));

                    return TestStatistic;
                }

                else if (EvalPlayed == -1 && EvalBest > -1)
                {
                    EvalPlayed = -0.99;
                    double TestStatCase1 = Math.Log((EvalBest + 1) / (EvalPlayed + 1));
                    return TestStatCase1;
                }
                else if (EvalBest == -1 && EvalPlayed < -1)
                {
                    EvalBest = -1.01;
                    double TestStatCase2 = Math.Log((-EvalPlayed + 1) / (-EvalBest - 1));
                    return TestStatCase2;
                }
                else if (EvalBest > -1 && EvalPlayed > -1)
                {
                    double LogArgument = Math.Abs((EvalBest + 1) / (EvalPlayed + 1));

                    double TestStatCase3 = Math.Log(LogArgument);

                    return TestStatCase3;
                }
                else if (EvalBest < -1 && EvalPlayed < -1)
                {
                    double TestStatCase4 = Math.Log((-EvalPlayed + 1) / (-EvalBest - 1));

                    return TestStatCase4;
                }
                else
                    return -1000000;
            }
        }

        //Calculating the test statistics on data in files and storing them in a new 'calc' file.
        public void TestStatOnFiles(string dir)
        {

            string[] fileEntries = Directory.GetFiles(dir);
            foreach (string fileName in fileEntries)
            {
                if (!fileName.EndsWith("_Eval.tsv")) continue;

                try
                {
                    //Creating a new calc file, which will store all of the test statistics for a give game.
                    StreamWriter TSOutput = new StreamWriter
                        (new FileStream(fileName + "_calc.tsv", FileMode.OpenOrCreate));
                    Console.WriteLine(fileName);
                    string[] lines = System.IO.File.ReadAllLines(fileName);
                    double previousEval = -1995;
                    int lineNumber = 1;
                    foreach (string line in lines)
                    {
                        Console.WriteLine(line);
                        double eval = Convert.ToDouble(line.Split('\t')[0]);
                        double testStat = -1000000;
                        if (Math.Abs(previousEval) < 1000 && Math.Abs(eval) < 1000)
                        {
                            // Dealing with the case where white is to move and calculating the 
                            // test statistic and writing it to the file.
                            if (lineNumber % 2 == 0)
                            {
                                testStat = TestStatisticCalculator(previousEval, eval);
                                TSOutput.WriteLine(testStat);
                                TSOutput.Flush();
                            }
                            //Similar to above, but for when black is to move
                            if (lineNumber % 2 == 1)
                            {
                                testStat = TestStatisticCalculator(-previousEval, -eval);
                                TSOutput.WriteLine(testStat);
                                TSOutput.Flush();
                            }
                        }


                        Console.WriteLine(testStat);
                        previousEval = eval;
                        lineNumber += 1;
                    }
                    TSOutput.Flush();
                    TSOutput.Close();

                }
                catch (FileNotFoundException ee)
                {
                    Console.WriteLine(ee.ToString() + ee.FileName);
                    Console.WriteLine(ee.StackTrace);
                    Console.ReadKey();
                }


            }

        }

       
    }
    //Class to describe a single game.
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
        public string GMoves = "";
        public List<string> GameMoves = new List<string>();


        //Creating a game from a PGN file.
        public Game(string InputPGN)
        {
            readGameFromPGN(InputPGN);
        }

        //Alternatively, creating an empty game.
        public Game( )
        {
            
        }

        //Reading the game from the PGN file, element by element
        public void readGameFromPGN(String fileName)
        {
             
            string[] lines = System.IO.File.ReadAllLines(fileName);
            foreach (string line in lines)
            {

                if (line.Equals("")) continue;

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
                            Event = Value.Replace("'", "");
                            break;
                        case "EventDate":
                            EventDate = Value;
                            break;
                        case "Round":
                            Round = Value;
                            break;
                        case "Result":
                            Result = Value;
                            break;
                        case "White":
                            if (Value.Contains(","))
                            {
                                WForename = Value.Substring(Value.IndexOf(",") + 1).Trim().Replace("'", "");
                                WSurname = Value.Substring(0, Value.IndexOf(",")).Trim().Replace("'", "");
                            }
                            break;

                        case "Black":
                            if (Value.Contains(","))
                            {
                                BForename = Value.Substring(Value.IndexOf(",") + 1).Trim().Replace("'", "");
                                BSurname = Value.Substring(0, Value.IndexOf(",")).Trim().Replace("'", "");
                            }
                            break;

                        case "WhiteElo":
                            WhiteElo = Value;
                            break;

                        case "BlackElo":
                            BlackElo = Value;
                            break;

                        case "WhiteFideId":
                            WhiteFideId = Value;
                            break;

                        case "BlackFideId":
                            BlackFideId = Value;
                            break;

                        default:
                            Console.WriteLine("Unrecognised " + key);
                            break;


                    }


                }
                else
                {
                    GMoves += line + " ";
                }



            }
             
        }

        public int getELOWhite()
        {
            return Convert.ToInt32(WhiteElo);
        }

        public int getELOBlack()
        {
            return Convert.ToInt32(BlackElo);
        }


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

        //Getting the game data.
        public string GetGamePGNInfo()
        {
            string result = "";

            result += "[Event \"" + Event + "\"]\n"
        + "[EventDate \"" + EventDate + "\"]\n"
        + "[Round \"" + Round + "\"]\n"
        + "[White \"" + WSurname + ", " + WForename + "\"]\n"
        + "[Black \"" + BSurname + ", " + BForename + "\"]\n"
        + "[WhiteElo \"" + WhiteElo + "\"]\n"
        + "[BlackElo \"" + BlackElo + "\"]\n"
        + "[WhiteFideId \"" + WhiteFideId + "\"]\n"
        + "[BlackFideId \"" + BlackFideId + "\"]\n"
        + "[Result \"" + Result + "\"]\n";


            return result;
        }
    }




