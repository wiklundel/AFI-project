namespace HitsterApp.Models
{
    public class Turn
    {
        public string Id { get; set; }

        public int TurnNumber { get; set; }

        public string PlayerId { get; set; }

        public string CardId { get; set; }
    }
}