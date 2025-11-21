using System;

namespace GAM106_LAB.DTO
{
    public class SaveResultDTO
    {
        public string UserId { get; set; }
        public int LevelId { get; set; }
        public int Score { get; set; }
        public DateTime CompletionDate { get; set; }
    }
}
