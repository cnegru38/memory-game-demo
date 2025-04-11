using System;

namespace Memory
{
    public class User
    {
        public string Username { get; set; }

        // Store only the Image ID (not the full object) for serialization
        public int ProfileImageId { get; set; }

        // These will be serialized
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }

        // This will be set at runtime based on ProfileImageId
        [System.Text.Json.Serialization.JsonIgnore]
        public Image ProfileImage { get; set; }

        public User() { }

        public User(string username, Image profileImage)
        {
            Username = username;
            ProfileImage = profileImage;
            ProfileImageId = profileImage?.Id ?? 0;
            GamesPlayed = 0;
            GamesWon = 0;
        }

        public override string ToString()
        {
            return $"Username: {Username}, Profile Image ID: {ProfileImageId}, Path: {ProfileImage?.FilePath}, Games Played: {GamesPlayed}, Games Won: {GamesWon}";
        }
    }

}

