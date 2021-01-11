using System;

namespace Anti_Cheating_software
{
	public class PGN_File_Handler
	{
		SqlConnection conn = new SqlConnection(@"SQLCONNECTION1");
		public PGN_File_Handler()
		{

		}

		public void ReadPGN(string dir)
		{
			string qry = "";
			using (SQLiteConnection cxn = new SQLiteConnection(_connectionString))
			{
				cxn.Open();
				SQLiteCommand cmd = new SQLiteCommand(qry, cxn);
				cmd.CommandType = CommandType.Text;

				SQLiteAdapter ad = new SQLiteAdapter(cmd);





			}



		}
	}
}