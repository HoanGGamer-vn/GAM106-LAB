namespace GAM106_LAB.DTO
{
    public class UpdateUserInformationDTO
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public int RegionId { get; set; }  
        public IFormFile Avatar { get; set; }
    }
}
