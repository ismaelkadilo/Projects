// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Boggle
{
    [ServiceContract]
    public interface IBoggleService
    {
        /// <summary>
        /// Sends back index.html as the response body.
        /// </summary>
        [WebGet(UriTemplate = "/api")]
        Stream API();

        /// <summary>
        /// Creates a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [WebInvoke(Method = "POST", UriTemplate = "/users")]
        UserID CreateUser(UserInfo user);

        /// <summary>
        /// Joins a game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [WebInvoke(Method = "POST", UriTemplate = "/games")]
        GameResponse JoinGame(GameThing info);

        /// <summary>
        /// Cancels a join request
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [WebInvoke(Method = "PUT", UriTemplate = "/games")]
        void CancelJoinRequest(UserID user);

        /// <summary>
        /// Plays a word
        /// </summary>
        /// <param name="word"></param>
        /// <param name="gameID"></param>
        /// <returns></returns>
        [WebInvoke(Method = "PUT", UriTemplate = "/games/{gameID}")]
        ScoreThing PlayWord(WordThing word, string gameID);

        /// <summary>
        /// Gets the game status
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        [WebInvoke(Method = "GET", UriTemplate = "/games/{gameID}?Brief={BRIEF}")]
        GameStatusThing GameStatus(string gameID, string Brief = "no");
    }
}
