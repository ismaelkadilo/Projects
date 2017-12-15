// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Net.HttpStatusCode;
using System.Diagnostics;
using System.Dynamic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Boggle
{
    /// <summary>
    /// Provides a way to start and stop the IIS web server from within the test
    /// cases.  If something prevents the test cases from stopping the web server,
    /// subsequent tests may not work properly until the stray process is killed
    /// manually.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class IISAgent {
        // Reference to the running process
        private static Process process = null;

        /// <summary>
        /// Starts IIS
        /// </summary>
        public static void Start(string arguments) {
            if (process == null) {
                ProcessStartInfo info = new ProcessStartInfo(Properties.Resources.IIS_EXECUTABLE, arguments);
                info.WindowStyle = ProcessWindowStyle.Minimized;
                info.UseShellExecute = false;
                process = Process.Start(info);
            }
        }

        /// <summary>
        ///  Stops IIS
        /// </summary>
        public static void Stop() {
            if (process != null) {
                process.Kill();
            }
        }
    }

    [TestClass, ExcludeFromCodeCoverage]
    public class BoggleTests {
        /// <summary>
        /// This is automatically run prior to all the tests to start the server
        /// </summary>
        [ClassInitialize()]
        public static void StartIIS(TestContext testContext) {
            IISAgent.Start(@"/site:""BoggleService"" /apppool:""Clr4IntegratedAppPool"" /config:""..\..\..\.vs\config\applicationhost.config""");
        }

        /// <summary>
        /// This is automatically run when all tests have completed to stop the server
        /// </summary>
        [ClassCleanup()]
        public static void StopIIS() {
            IISAgent.Stop();
        }

        private RestTestClient client = new RestTestClient("http://localhost:60000/BoggleService.svc/");

        /// <summary>
        /// Misc Test
        /// </summary>
        [TestMethod]

        public void TestMethod1() {
            Response r = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", "-5").Result;
            Assert.AreEqual(BadRequest, r.Status);

            dynamic user = new ExpandoObject();
            user.Nickname = "test";
            r = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user).Result;
            Assert.AreEqual(Created, r.Status);
        }

        /// <summary>
        /// Misc Test
        /// </summary>
        [TestMethod]
        public void TestMethod2() {
            dynamic user = new ExpandoObject();
            user.Nickname = "ismael";
            Response r = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user).Result;
            Assert.AreEqual(Created, r.Status);
            string gameID = r.Data.ToString();
        }

        /// <summary>
        /// Misc Test
        /// </summary>
        [TestMethod]
        public void TestMethod3() {
            dynamic user = new ExpandoObject();
            user.Nickname = "ismael";
            Response r = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user).Result;
            Assert.AreEqual(Created, r.Status);
            string gameID = r.Data.ToString();

            dynamic gamestatusthing = new ExpandoObject();
            gamestatusthing.GameID = gameID;
            gamestatusthing.StartTime = "30";
            gamestatusthing.Completed = false;
            r = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", gamestatusthing).Result;
        }

        /// <summary>
        /// Misc Test
        /// </summary>
        [TestMethod]
        public void TestMethod4() {
            dynamic user = new ExpandoObject();
            user.Nickname = "ismael";
            Response r = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user).Result;
            Assert.AreEqual(Created, r.Status);
            string gameID = r.Data.ToString();

            dynamic gamestatusthing = new ExpandoObject();
            gamestatusthing.GameID = gameID;
            gamestatusthing.StartTime = "30";
            gamestatusthing.Completed = false;
            r = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", gamestatusthing).Result;

            dynamic gamestatusthings = new ExpandoObject();
            gamestatusthings.GameID = gameID;
            gamestatusthings.StartTime = "25";
            gamestatusthing.TimeLimit = "0";
            r = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", gamestatusthings).Result;

        }

        /// <summary>
        /// Creates two players and joins a match
        /// </summary>
        [TestMethod]
        public void TestMethod5() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Test 2";
            Response r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user2).Result;
            Assert.AreEqual(Created, r2.Status);
            string userID2 = r2.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            dynamic JoinRequestInfo2 = new ExpandoObject();
            JoinRequestInfo2.UserToken = userID2;
            JoinRequestInfo2.TimeLimit = 30;
            r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo2).Result;
            Assert.AreEqual(Created, r2.Status);
        }

        /// <summary>
        /// Creates two players and joins a match after canceling a match
        /// </summary>
        [TestMethod]
        public void TestMethod6()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Test 2";
            Response r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user2).Result;
            Assert.AreEqual(Created, r2.Status);
            string userID2 = r2.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            dynamic CancelJoinRequestInfo = new ExpandoObject();
            CancelJoinRequestInfo.UserToken = userID1;
            r1 = client.DoPutAsync(CancelJoinRequestInfo, "http://localhost:60000/BoggleService.svc/games").Result;
            Assert.AreEqual(OK, r1.Status);

            JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            dynamic JoinRequestInfo2 = new ExpandoObject();
            JoinRequestInfo2.UserToken = userID2;
            JoinRequestInfo2.TimeLimit = 30;
            r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo2).Result;
            Assert.AreEqual(Created, r2.Status);
        }

        /// <summary>
        /// Creates player using @ to make the server wait 10 seconds
        /// </summary>
        [TestMethod]
        public void TestMethod7()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "@";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
        }

        /// <summary>
        /// Creates player using # to make the server return forbidden
        /// </summary>
        [TestMethod]
        public void TestMethod8()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "#";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        /// Creates player using invalid names to make the server return forbidden
        /// </summary>
        [TestMethod]
        public void TestMethod9()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = null;
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Forbidden, r1.Status);

            user1 = new ExpandoObject();
            user1.Nickname = " ";
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Forbidden, r1.Status);

            user1 = new ExpandoObject();
            user1.Nickname = "";
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        /// Misc Test
        /// </summary>
        [TestMethod]
        public void TestMethod10()
        {
            dynamic todogamething = new ExpandoObject();
            todogamething.TimeLimit = "";
        }

        /// <summary>
        /// Misc Test
        /// </summary>
        [TestMethod]
        public void TestMethod11()
        {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Test 2";
            Response r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user2).Result;
            Assert.AreEqual(Created, r2.Status);
            string userID2 = r2.Data.UserToken;

            dynamic wordthing = new ExpandoObject();
            wordthing.UserToken = userID1;
            wordthing.Word = "ismael";
            Response r3 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", wordthing).Result;
            Assert.AreEqual(Created, r2.Status);
           
        }
        /// <summary>
        /// Scorething 
        /// </summary>
        [TestMethod]
        public void TestMethod12()
        {
            dynamic scorething = new ExpandoObject();
            scorething.Score = 6;
            Response r = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", scorething).Result;
            Assert.AreEqual(Forbidden, r.Status);
          //  string gameID = r.Data.ToString();
        }

        /// <summary>
        /// Joins game using invalid times and user token to make the server return forbidden 
        /// </summary>
        [TestMethod]
        public void TestMethod10_2() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 0;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Forbidden, r1.Status);

            JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 300;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Forbidden, r1.Status);

            JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = "Apple";
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        /// Joins game while in a pending game 
        /// </summary>
        [TestMethod]
        public void TestMethod11_2() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Conflict, r1.Status);
        }

        /// <summary>
        /// Cancel Join Request with invalid parameters 
        /// </summary>
        [TestMethod]
        public void TestMethod12_2() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;

            dynamic CancelJoinRequestInfo1 = new ExpandoObject();
            CancelJoinRequestInfo1.UserToken = "Test";
            r1 = client.DoPutAsync(CancelJoinRequestInfo1, "http://localhost:60000/BoggleService.svc/games").Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        /// Cancel Join Request with invalid parameters 
        /// </summary>
        [TestMethod]
        public void TestMethod13() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic CancelJoinRequestInfo1 = new ExpandoObject();
            CancelJoinRequestInfo1.UserToken = userID1;
            r1 = client.DoPutAsync(CancelJoinRequestInfo1, "http://localhost:60000/BoggleService.svc/games").Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        /// Cancel Join Request with invalid parameters 
        /// </summary>
        [TestMethod]
        public void TestMethod14() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Test 2";
            Response r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user2).Result;
            Assert.AreEqual(Created, r2.Status);
            string userID2 = r2.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;

            dynamic JoinRequestInfo2 = new ExpandoObject();
            JoinRequestInfo2.UserToken = userID2;
            JoinRequestInfo2.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo2).Result;

            dynamic CancelJoinRequestInfo1 = new ExpandoObject();
            CancelJoinRequestInfo1.UserToken = userID1;
            r1 = client.DoPutAsync(CancelJoinRequestInfo1, "http://localhost:60000/BoggleService.svc/games").Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        /// Get Status with no game 
        /// </summary>
        [TestMethod]
        public void TestMethod15() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            r1 = client.DoGetAsync("http://localhost:60000/BoggleService.svc/games/" + "100000", new string[] { "" }).Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        /// Get Status normal 
        /// </summary>
        [TestMethod]
        public void TestMethod16() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            string gameID = r1.Data.GameID;

            r1 = client.DoGetAsync("http://localhost:60000/BoggleService.svc/games/" + gameID, new string[] { "" }).Result;
            Assert.AreEqual(OK, r1.Status);
        }

        /// <summary>
        /// Get Status brief active game 
        /// </summary>
        [TestMethod]
        public void TestMethod17() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Created, r1.Status);

            string gameID = r1.Data.GameID;

            r1 = client.DoGetAsync("http://localhost:60000/BoggleService.svc/games/" + gameID + "?Brief=yes", new string[] { "" }).Result;
            Assert.AreEqual(OK, r1.Status);
        }

        /// <summary>
        /// Get Status active game   
        /// </summary>
        [TestMethod]
        public void TestMethod18() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Test";
            Response r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user2).Result;
            Assert.AreEqual(Created, r2.Status);
            string userID2 = r2.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            dynamic JoinRequestInfo2 = new ExpandoObject();
            JoinRequestInfo2.UserToken = userID2;
            JoinRequestInfo2.TimeLimit = 30;
            r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo2).Result;
            Assert.AreEqual(Created, r2.Status);

            string gameID = r2.Data.GameID;

            r1 = client.DoGetAsync("http://localhost:60000/BoggleService.svc/games/" + gameID, new string[] { "" }).Result;
            Assert.AreEqual(OK, r1.Status);
        }

        /// <summary>
        /// Get Status after game and play word after active 
        /// </summary>
        [TestMethod]
        public void TestMethod19() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Test";
            Response r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user2).Result;
            Assert.AreEqual(Created, r2.Status);
            string userID2 = r2.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 5;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            dynamic JoinRequestInfo2 = new ExpandoObject();
            JoinRequestInfo2.UserToken = userID2;
            JoinRequestInfo2.TimeLimit = 5;
            r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo2).Result;
            Assert.AreEqual(Created, r2.Status);

            string gameID = r2.Data.GameID;

            Thread.Sleep(6000);

            r1 = client.DoGetAsync("http://localhost:60000/BoggleService.svc/games/" + gameID, new string[] { "" }).Result;
            Assert.AreEqual(OK, r1.Status);

            dynamic wordData = new ExpandoObject();
            wordData.UserToken = userID1;
            wordData.Word = "Test";
            r1 = client.DoPutAsync(wordData, "http://localhost:60000/BoggleService.svc/games/" + gameID).Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        ///Play invalid word 
        /// </summary>
        [TestMethod]
        public void TestMethod20() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Test";
            Response r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user2).Result;
            Assert.AreEqual(Created, r2.Status);
            string userID2 = r2.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            dynamic JoinRequestInfo2 = new ExpandoObject();
            JoinRequestInfo2.UserToken = userID2;
            JoinRequestInfo2.TimeLimit = 30;
            r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo2).Result;
            Assert.AreEqual(Created, r2.Status);

            string gameID = r2.Data.GameID;

            dynamic wordData = new ExpandoObject();
            wordData.UserToken = userID1;
            wordData.Word = null;
            r1 = client.DoPutAsync(wordData, "http://localhost:60000/BoggleService.svc/games/" + gameID).Result;
            Assert.AreEqual(Forbidden, r1.Status);

            wordData = new ExpandoObject();
            wordData.UserToken = userID1;
            wordData.Word = " ";
            r1 = client.DoPutAsync(wordData, "http://localhost:60000/BoggleService.svc/games/" + gameID).Result;
            Assert.AreEqual(Forbidden, r1.Status);

            wordData = new ExpandoObject();
            wordData.UserToken = "Test";
            wordData.Word = "test";
            r1 = client.DoPutAsync(wordData, "http://localhost:60000/BoggleService.svc/games/" + gameID).Result;
            Assert.AreEqual(Forbidden, r1.Status);
        }

        /// <summary>
        ///Play valid words 
        /// </summary>
        [TestMethod]
        public void TestMethod21() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Test";
            Response r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user2).Result;
            Assert.AreEqual(Created, r2.Status);
            string userID2 = r2.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 30;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            dynamic JoinRequestInfo2 = new ExpandoObject();
            JoinRequestInfo2.UserToken = userID2;
            JoinRequestInfo2.TimeLimit = 30;
            r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo2).Result;
            Assert.AreEqual(Created, r2.Status);

            string gameID = r2.Data.GameID;

            dynamic wordData = new ExpandoObject();
            wordData.UserToken = userID1;
            wordData.Word = "Test";
            r1 = client.DoPutAsync(wordData, "http://localhost:60000/BoggleService.svc/games/" + gameID).Result;
            Assert.AreEqual(OK, r1.Status);

            wordData = new ExpandoObject();
            wordData.UserToken = userID2;
            wordData.Word = "Test";
            r1 = client.DoPutAsync(wordData, "http://localhost:60000/BoggleService.svc/games/" + gameID).Result;
            Assert.AreEqual(OK, r1.Status);
        }

        /// <summary>
        ///Play valid word as game is ending 
        /// </summary>
        [TestMethod]
        public void TestMethod22() {
            dynamic user1 = new ExpandoObject();
            user1.Nickname = "Test";
            Response r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user1).Result;
            Assert.AreEqual(Created, r1.Status);
            string userID1 = r1.Data.UserToken;

            dynamic user2 = new ExpandoObject();
            user2.Nickname = "Test";
            Response r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/users", user2).Result;
            Assert.AreEqual(Created, r2.Status);
            string userID2 = r2.Data.UserToken;

            dynamic JoinRequestInfo1 = new ExpandoObject();
            JoinRequestInfo1.UserToken = userID1;
            JoinRequestInfo1.TimeLimit = 5;
            r1 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo1).Result;
            Assert.AreEqual(Accepted, r1.Status);

            dynamic JoinRequestInfo2 = new ExpandoObject();
            JoinRequestInfo2.UserToken = userID2;
            JoinRequestInfo2.TimeLimit = 5;
            r2 = client.DoPostAsync("http://localhost:60000/BoggleService.svc/games", JoinRequestInfo2).Result;
            Assert.AreEqual(Created, r2.Status);

            string gameID = r2.Data.GameID;

            Thread.Sleep(4900);

            dynamic wordData = new ExpandoObject();
            wordData.UserToken = userID1;
            wordData.Word = "Test";
            r1 = client.DoPutAsync(wordData, "http://localhost:60000/BoggleService.svc/games/" + gameID).Result;
            Assert.AreEqual(Conflict, r1.Status);
        }
    }
}
