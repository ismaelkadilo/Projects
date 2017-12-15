// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Boggle
{
    /// <summary>
    /// Input for Create User  
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserInfo
    {
        public string Nickname { get; set; }

        public string GameID { get; set; }

    }

    /// <summary>
    /// Response for Create User  
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserID
    {
        public string UserToken { get; set; }
    }

    /// <summary>
    /// Input for Join Game  
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GameThing
    {
        public string UserToken { get; set; }
        public int TimeLimit { get; set; }
    }

    /// <summary>
    /// Response for Join Game  
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GameResponse
    {
        public string GameID { get; set; }
    }

    /// <summary>
    /// Input for Play Word
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WordThing
    {
        public string UserToken { get; set; }
        public string Word { get; set; }
    }

    /// <summary>
    /// Words that are played  
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Words
    {
        public string Word { get; set; }
        public int Score { get; set; }
    }

    /// <summary>
    /// Output for Play Word   
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ScoreThing
    {
        public int Score { get; set; }
    }

    /// <summary>
    /// Output for Game Status  
    /// </summary>
    [DataContract, ExcludeFromCodeCoverage]
    public class GameStatusThing {
        [DataMember]
        public string GameState { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Board { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int TimeLimit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? TimeLeft { get; set; } = null;
        [DataMember(EmitDefaultValue = false)]
        public PlayerThing Player1 = null;
        [DataMember(EmitDefaultValue = false)]
        public PlayerThing Player2 = null;

        // The following data is hidden  
        public string GameIDHidden { get; set; }
        public DateTime StartTime { get; set; }
        public int TimeLimitHidden { get; set; }
        public BoggleBoard BoggleBoard { get; set; }
        public PlayerThing Player1Hidden = new PlayerThing();

    }

    /// <summary>
    /// For use in GameStatusThing  
    /// </summary>
    [DataContract, ExcludeFromCodeCoverage]
    public class PlayerThing {
        [DataMember(EmitDefaultValue = false)]
        public string Nickname { get; set; }

        [DataMember]
        public int Score { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Words> WordsPlayed { get; set; }

        // The following data is hidden  
        public string UserToken { get; set; }
    }
}


