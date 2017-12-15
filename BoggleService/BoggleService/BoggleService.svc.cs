// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using static System.Net.HttpStatusCode;
using System.Diagnostics.CodeAnalysis;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;


namespace Boggle
{
    public class BoggleService : IBoggleService
    {
        private static string BoggleDB;

        private static readonly object sync = new object();

        /// <summary>
        /// Static Constructor
        /// </summary>
        static BoggleService()
        {
            BoggleDB = ConfigurationManager.ConnectionStrings["BoggleDB"].ConnectionString;
        }

        /// <summary>
        /// The most recent call to SetStatus determines the response code used when
        /// an http response is sent.
        /// </summary>
        /// <param name="status"></param>
        private static void SetStatus(HttpStatusCode status)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = status;
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public UserID CreateUser(UserInfo user)
        {
            if (user.Nickname == null || user.Nickname.Trim().Length == 0 || user.Nickname.ToCharArray()[0] == '#')
            {
                SetStatus(Forbidden);
                return null;
            }

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                // Connections must be opened
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO Users (UserID, Nickname) VALUES(@UserID, @Nickname)", conn, trans))
                    {

                        string userID = Guid.NewGuid().ToString();
                        UserID id = new UserID();
                        id.UserToken = userID;

                        // This is where the placeholders are replaced.
                        command.Parameters.AddWithValue("@UserID", userID);
                        command.Parameters.AddWithValue("@Nickname", user.Nickname.Trim());

                        // This executes the command within the transaction over the connection.  The number of rows
                        // that were modified is returned.  Perhaps I should check and make sure that 1 is returned
                        // as expected.
                        int tmp = command.ExecuteNonQuery();
                        SetStatus(Created);

                        // Immediately before each return that appears within the scope of a transaction, it is
                        // important to commit the transaction.  Otherwise, the transaction will be aborted and
                        // rolled back as soon as control leaves the scope of the transaction. 
                        trans.Commit();
                        return id;
                    }
                }
            }
        }


        /// <summary>
        /// Joins a game
        /// </summary>
        /// <param name="user"></param>
        /// <param name="timeLimit"></param>
        /// <returns></returns>
        public GameResponse JoinGame(GameThing info)
        {
            UserInfo user = GetUserInfo(info.UserToken);

            if (info.TimeLimit > 120 || info.TimeLimit < 5 || user.Nickname == null)
            {
                SetStatus(Forbidden);
                return null;
            }

            GameResponse response = new GameResponse();

            lock (sync)
            {
                bool pendingGame = false;
                string gameID = "";

                pendingGame = GamePending();
                if (pendingGame)
                {
                    gameID = GamesCount() + "";
                }

                if (pendingGame && user.GameID != gameID)
                {
                    GameStatusThing tmp = GetGameStatus(gameID);
                    //DeleteGame(tmp.GameIDHidden);

                    tmp.GameState = "active";
                    tmp.Board = tmp.BoggleBoard.ToString();
                    tmp.TimeLimit = (tmp.TimeLimit + info.TimeLimit) / 2; 
                    tmp.StartTime = DateTime.Now;
                    tmp.TimeLeft = (tmp.TimeLimit - (DateTime.Now - tmp.StartTime).Seconds);

                    tmp.Player1 = new PlayerThing();
                    tmp.Player1.Nickname = tmp.Player1Hidden.Nickname;
                    tmp.Player1.Score = 0;
                    tmp.Player1.WordsPlayed = new List<Words>();
                    tmp.Player1.UserToken = tmp.Player1Hidden.UserToken;

                    tmp.Player1Hidden = new PlayerThing();
                    tmp.Player1Hidden.Nickname = tmp.Player1Hidden.Nickname;
                    tmp.Player1Hidden.Score = 0;
                    tmp.Player1Hidden.WordsPlayed = new List<Words>();
                    tmp.Player1Hidden.UserToken = tmp.Player1Hidden.UserToken;

                    tmp.Player2 = new PlayerThing();
                    tmp.Player2.Nickname = user.Nickname;
                    tmp.Player2.Score = 0;
                    tmp.Player2.WordsPlayed = new List<Words>();
                    tmp.Player2.UserToken = info.UserToken;

                    SetUserGameID(info.UserToken, tmp.GameIDHidden);

                    AddGameToDB(tmp);

                    response.GameID = tmp.GameIDHidden;

                    SetStatus(Created);

                    return response;
                }
                else if (user.GameID != gameID)
                {
                    GameStatusThing tmp = new GameStatusThing();

                    tmp.GameState = "pending";
                    tmp.BoggleBoard = new BoggleBoard();
                    tmp.TimeLimitHidden = info.TimeLimit;

                    tmp.Player1Hidden.Nickname = user.Nickname;
                    tmp.Player1Hidden.UserToken = info.UserToken;

                    tmp.GameIDHidden = (GamesCount() + 1) + "";

                    SetUserGameID(info.UserToken, tmp.GameIDHidden);

                    AddGameToDB(tmp);

                    response.GameID = tmp.GameIDHidden;

                    SetStatus(Accepted);

                    return response;
                }
                else
                {
                    SetStatus(Conflict);
                    return null;
                }
            }
        }

        /// <summary>
        /// Cancels a join request
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public void CancelJoinRequest(UserID user)
        {
            UserInfo info = GetUserInfo(user.UserToken);
            if (info.Nickname == null)
            {
                SetStatus(Forbidden);
                return;
            }

            bool valid = false;

            if (info.GameID == null)
            {
                SetStatus(Forbidden);
                return;
            }

            lock (sync)
            {
                if (ContainsGame(info.GameID))
                {
                    GameStatusThing tmp = GetGameStatus(info.GameID);
                    if (tmp.GameState == "pending")
                    {
                        valid = tmp.Player1Hidden.UserToken == user.UserToken;
                        if (valid)
                        {
                            DeleteGame(info.GameID);
                            SetUserGameID(user.UserToken, null);
                        }
                    }
                }
            }

            if (valid)
            {
                SetStatus(OK);
            }
            else
            {
                SetStatus(Forbidden);
            }
        }

        /// <summary>
        /// Plays a word
        /// </summary>
        /// <param name="word"></param>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public ScoreThing PlayWord(WordThing word, string gameID)
        {
            UserInfo user = GetUserInfo(word.UserToken);
            if (word.Word == null || word.Word.Trim() == "" || user.Nickname == null || user.GameID != gameID)
            {
                SetStatus(Forbidden);
                return null;
            }

            if (user.GameID == null)
            {
                SetStatus(Conflict);
                return null;
            }

            GameStatusThing game = GetGameStatus(user.GameID);

            UpdateGameTimeLeft(game);

            if (game.GameState != "active")
            {
                SetStatus(Conflict);
                return null;
            }

            if (word.UserToken == game.Player1.UserToken)
            {
                Words wordPlayed = new Words();
                wordPlayed.Word = word.Word;
                wordPlayed.Score = calculateScore(word.Word.Trim(), game.Player1.WordsPlayed, game.BoggleBoard);
                AddWord(wordPlayed.Word, game.Player1.UserToken, game.GameIDHidden, wordPlayed.Score);
                ScoreThing score = new ScoreThing();
                score.Score = wordPlayed.Score;
                SetStatus(OK);
                return score;
            }
            else
            {
                Words wordPlayed = new Words();
                wordPlayed.Word = word.Word;
                wordPlayed.Score = calculateScore(word.Word.Trim(), game.Player2.WordsPlayed, game.BoggleBoard);
                AddWord(wordPlayed.Word, game.Player2.UserToken, game.GameIDHidden, wordPlayed.Score);
                ScoreThing score = new ScoreThing();
                score.Score = wordPlayed.Score;
                SetStatus(OK);
                return score;
            }
        }

        /// <summary>
        /// Gets the game status
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public GameStatusThing GameStatus(string gameID, string Brief = "no")
        {
            if (!ContainsGame(gameID))
            {
                SetStatus(Forbidden);
                return null;
            }
            else
            {
                GameStatusThing response = GetGameStatus(gameID);
                if (response.TimeLimit != 0) {
                    UpdateGameTimeLeft(response);
                }
                SetStatus(OK);
                if(response.GameState == "pending") {
                    GameStatusThing tmp = new GameStatusThing();
                    tmp.GameState = "pending";
                    return tmp;
                }

                if (Brief == "yes" && response.GameState != "pending")
                {
                    GameStatusThing tmp = new GameStatusThing();
                    tmp.GameState = response.GameState;
                    tmp.TimeLeft = response.TimeLeft;
                    tmp.Player1 = new PlayerThing();
                    tmp.Player1.Score = response.Player1.Score;
                    tmp.Player2 = new PlayerThing();
                    tmp.Player2.Score = response.Player2.Score;
                    return tmp;
                }
                if (response.GameState == "active")
                {
                    GameStatusThing tmp = new GameStatusThing();
                    tmp.GameState = response.GameState;
                    tmp.Board = response.Board;
                    tmp.TimeLimit = response.TimeLimit;
                    tmp.TimeLeft = response.TimeLeft;
                    tmp.Player1 = new PlayerThing();
                    tmp.Player1.Nickname = response.Player1.Nickname;
                    tmp.Player1.Score = response.Player1.Score;
                    tmp.Player2 = new PlayerThing();
                    tmp.Player2.Nickname = response.Player2.Nickname;
                    tmp.Player2.Score = response.Player2.Score;
                    return tmp;
                }
                return response;
            }
        }

        /// <summary>
        /// Returns a Stream version of index.html.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public Stream API()
        {
            SetStatus(OK);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "index.html");
        }

        /// <summary>
        /// Helper Method to evaluate word score value
        /// </summary>
        /// <param name="word"></param>
        /// <param name="words"></param>
        /// <param name="board"></param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        private int calculateScore(string word, List<Words> words, BoggleBoard board)
        {
            if (word.Length < 3)
            {
                return 0;
            }
            if (words != null && words.Count > 0)
            {
                foreach (Words wordThing in words)
                {
                    if (wordThing.Word.Equals(word))
                    {
                        return 0;
                    }
                }
            }

            if (!board.CanBeFormed(word))
            {
                return -1;
            }

            switch (word.Length)
            {
                case 3:
                case 4:
                    return 1;
                case 5:
                    return 2;
                case 6:
                    return 3;
                case 7:
                    return 5;
                default:
                    return 11;
            }
        }

        /// <summary>
        /// Helper Method to check if a game has completed
        /// </summary>
        /// <param name="game"></param>
        private void UpdateGameTimeLeft(GameStatusThing game)
        {
            UserInfo p1 = GetUserInfo(game.Player1.UserToken);
            UserInfo p2;
            if (game.Player2 != null) {
                p2 = GetUserInfo(game.Player2.UserToken);
            } else {
                p2 = new UserInfo();
                p2.Nickname = null;
                p2.GameID = null;
            }
            game.TimeLeft = (int)(game.TimeLimit - (DateTime.Now - game.StartTime).Duration().TotalSeconds) > 0 ? (int)(game.TimeLimit - (DateTime.Now - game.StartTime).Duration().TotalSeconds) : 0;
            if (game.TimeLeft == 0 && game.StartTime != new DateTime())
            {
                game.GameState = "completed";
                SetGameState(game.GameIDHidden, game.GameState);
                if (p1.Nickname != null && p1.GameID == game.GameIDHidden)
                {
                    SetUserGameID(game.Player1.UserToken, null);
                }
                if (p2.Nickname != null && p2.GameID == game.GameIDHidden)
                {
                    SetUserGameID(game.Player2.UserToken, null);
                }
            }
        }

        /// <summary>
        /// Helper method to get UserInfo (Nickname and GameID
        /// </summary>
        /// <param name="UserToken"></param>
        /// <returns></returns>
        private UserInfo GetUserInfo(string UserToken)
        {
            UserInfo user = new UserInfo();
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("SELECT * FROM Users WHERE UserID LIKE \'" + UserToken + "\'", conn, trans))
                    {
                        SqlDataReader reader = command.ExecuteReaderAsync().Result;
                        reader.Read();
                        try
                        {
                            user.Nickname = reader.GetFieldValue<string>(1);
                            user.GameID = reader.GetFieldValue<string>(2);
                        }
                        catch
                        {
                            // Intentionally empty
                        }
                        reader.Close();
                        trans.Commit();
                    }

                }
                conn.Close();
            }
            return user;
        }

        /// <summary>
        /// Helper Method to check the database for a pending game
        /// </summary>
        /// <returns></returns>
        private bool GamePending()
        {
            bool pending;
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("SELECT * FROM Games WHERE GameState LIKE \'pending\'", conn, trans))
                    {
                        SqlDataReader reader = command.ExecuteReaderAsync().Result;
                        reader.Read();
                        try
                        {
                            if (reader.GetValue(6).Equals("pending"))
                            {
                                pending = true;
                            }else {
                                pending = false;
                            }
                        }
                        catch
                        {
                            pending = false;
                        }
                        reader.Close();
                        trans.Commit();
                    }

                }
                conn.Close();
            }
            return pending;
        }

        /// <summary>
        /// Helper Method to get the number of games
        /// </summary>
        /// <returns></returns>
        private int GamesCount()
        {
            int count;
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("SELECT MAX(GameID) AS GamesCount FROM Games", conn, trans))
                    {
                        SqlDataReader reader = command.ExecuteReaderAsync().Result;
                        reader.Read();
                        try
                        {
                            count = reader.GetFieldValue<int>(0);
                        }
                        catch
                        {
                            count = 0;
                        }
                        reader.Close();
                        trans.Commit();
                    }
                }
                conn.Close();
            }
            return count;
        }

        /// <summary>
        /// Helper Method to update User game id
        /// </summary>
        /// <param name="UserToken"></param>
        /// <param name="GameID"></param>
        private void SetUserGameID(string UserToken, string GameID)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    if (GameID == "" || GameID == null) {
                        using (SqlCommand command = new SqlCommand("UPDATE Users SET GameID = null WHERE UserID = \'" + UserToken + "\'", conn, trans)) {
                            command.ExecuteNonQuery();
                            trans.Commit();
                        }
                    } else {
                        using (SqlCommand command = new SqlCommand("UPDATE Users SET GameID = \'" + GameID + "\' WHERE UserID = \'" + UserToken + "\'", conn, trans)) {
                            command.ExecuteNonQuery();
                            trans.Commit();
                        }
                    }
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Helper Method to add game status to database
        /// </summary>
        /// <param name="game"></param>
        private void AddGameToDB(GameStatusThing game)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction()) {
                    if (game.Player2 == null) {
                        using (SqlCommand command = new SqlCommand("INSERT INTO Games(Player1, Player2, Board, TimeLimit, StartTime, GameState) VALUES(\'" + game.Player1Hidden.UserToken + "\', null, \'" + game.BoggleBoard.ToString() + "\', " + game.TimeLimitHidden + ", null, \'pending\')", conn, trans)) {
                            command.ExecuteNonQuery();
                        }
                    } else {
                        using (SqlCommand command = new SqlCommand("UPDATE Games SET Player2 = \'" + game.Player2.UserToken + "\', TimeLimit = " + game.TimeLimit + ", StartTime = \'" + game.StartTime + "\', GameState = \'active\' WHERE GameID = " + int.Parse(game.GameIDHidden), conn, trans)) {
                            command.ExecuteNonQuery(); 
                        }
                    }
                    trans.Commit();
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Helper Method to get GameStatusThing from database
        /// </summary>
        /// <param name="GameID"></param>
        /// <returns></returns>
        private GameStatusThing GetGameStatus(string GameID)
        {
            GameStatusThing gameStatus = new GameStatusThing();
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("SELECT * FROM Games WHERE GameID = " + GameID, conn, trans))
                    {
                        SqlDataReader reader = command.ExecuteReaderAsync().Result;
                        reader.Read();
                        try
                        {
                            gameStatus.GameIDHidden = reader.GetFieldValue<int>(0) + "";

                            gameStatus.Board = reader.GetFieldValue<string>(3);
                            gameStatus.BoggleBoard = new BoggleBoard(gameStatus.Board);

                            gameStatus.TimeLimit = reader.GetFieldValue<int>(4);

                            try {
                                gameStatus.StartTime = reader.GetFieldValue<DateTime>(5);
                            } catch {
                            }

                            gameStatus.GameState = reader.GetFieldValue<string>(6);

                            gameStatus.Player1 = new PlayerThing();
                            gameStatus.Player1.UserToken = reader.GetFieldValue<string>(1);
                            gameStatus.Player1.Nickname = GetUserInfo(gameStatus.Player1.UserToken).Nickname;
                            try {
                                gameStatus.Player1.WordsPlayed = GetWordsPlayed(gameStatus.Player1.UserToken, gameStatus.GameIDHidden, gameStatus.BoggleBoard);
                            } catch {
                            }
                            gameStatus.Player1.Score = CalculateTotalScore(gameStatus.Player1.WordsPlayed);

                            gameStatus.Player1Hidden = new PlayerThing();
                            gameStatus.Player1Hidden.UserToken = reader.GetFieldValue<string>(1);
                            gameStatus.Player1Hidden.Nickname = GetUserInfo(gameStatus.Player1Hidden.UserToken).Nickname;

                            gameStatus.Player2 = new PlayerThing();
                            gameStatus.Player2.UserToken = reader.GetFieldValue<string>(2);
                            if (gameStatus.Player2.UserToken != null)
                            {
                                gameStatus.Player2.Nickname = GetUserInfo(gameStatus.Player2.UserToken).Nickname;
                                gameStatus.Player2.WordsPlayed = GetWordsPlayed(gameStatus.Player2.UserToken, gameStatus.GameIDHidden, gameStatus.BoggleBoard);
                                gameStatus.Player2.Score = CalculateTotalScore(gameStatus.Player2.WordsPlayed);
                            }
                            else
                            {
                                gameStatus.Player2 = null;
                            }

                        }
                        catch
                        {
                            // Intentionally empty
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return gameStatus;
        }

        /// <summary>
        /// Helper method that gets all the words that a player has played in a given game
        /// </summary>
        /// <param name="UserToken"></param>
        /// <returns></returns>
        private List<Words> GetWordsPlayed(string UserToken, string GameID, BoggleBoard board)
        {
            lock (sync) {
                List<Words> wordList = new List<Words>();
                using (SqlConnection conn = new SqlConnection(BoggleDB)) {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction()) {
                        using (SqlCommand command = new SqlCommand("SELECT * FROM Words WHERE GameID = " + GameID + "AND Player = \'" + UserToken + "\'", conn, trans)) {
                            SqlDataReader reader = command.ExecuteReaderAsync().Result;
                            while (reader.Read()) {
                                try {
                                    Words word = new Words();
                                    word.Word = reader.GetFieldValue<string>(1);
                                    word.Score = reader.GetFieldValue<int>(4);
                                    wordList.Add(word);
                                } catch {
                                    // Intentionally empty
                                }
                            }
                            reader.Close();
                            trans.Commit();
                        }
                    }
                }
                return wordList;
            }
        }

        /// <summary>
        /// Helper Method that calculate the total score from a list of Words
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        private int CalculateTotalScore(List<Words> words)
        {
            if(words == null) {
                return 0;
            }

            int total = 0;
            foreach (Words word in words)
            {
                total += word.Score;
            }
            return total;
        }

        /// <summary>
        /// Helper method that checks to see if the database has a game
        /// </summary>
        /// <param name="GameID"></param>
        /// <returns></returns>
        private bool ContainsGame(string GameID)
        {
            Regex tmp = new Regex("^[0-9]*$");
            if (!tmp.IsMatch(GameID)) {
                return false;
            }
            bool result = false;
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("SELECT * FROM Games WHERE GameID = " + GameID, conn, trans))
                    {
                        SqlDataReader reader = command.ExecuteReaderAsync().Result;
                        reader.Read();
                        result = reader.HasRows;
                        reader.Close();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Helper method that deletes a game from the database
        /// </summary>
        /// <param name="GameID"></param>
        private void DeleteGame(string GameID)
        {
            SetGameState(GameID, "completed");
        }

        /// <summary>
        /// Helper method that sets the game state on the database
        /// </summary>
        /// <param name="GameID"></param>
        private void SetGameState(string GameID, string state)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("UPDATE Games SET GameState = \'" + state + "\' FROM Games WHERE GameID = " + GameID, conn, trans))
                    {
                        command.ExecuteNonQuery();
                        trans.Commit();
                    }
                } 
            }
        }

        /// <summary>
        /// Helper Method to add a word to the database
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="GameID"></param>
        private void AddWord(string Word, string UserID, string GameID, int Score) {
            using (SqlConnection conn = new SqlConnection(BoggleDB)) {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction()) {
                    using (SqlCommand command = new SqlCommand("INSERT INTO words(Word, GameID, Player, Score) VALUES(@Word, @GameID, @Player, @Score)", conn, trans)) {
                        command.Parameters.AddWithValue("@Word", Word);
                        command.Parameters.AddWithValue("@GameID", int.Parse(GameID));
                        command.Parameters.AddWithValue("@Player", UserID);
                        command.Parameters.AddWithValue("@Score", Score);
                        command.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
            }
        }

    }
}
