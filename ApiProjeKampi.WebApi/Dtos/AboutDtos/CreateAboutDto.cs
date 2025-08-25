namespace ApiProjeKampi.WebApi.Dtos.AboutDtos
{
    public class CreateAboutDto
    {
        public string Title { get; set; }
        public string ImageURL { get; set; }
        public string VideoCoverImageURL { get; set; }
        public string VideoURL { get; set; }
        public string Description { get; set; }
        public string ReservationNumber { get; set; }
    }
}
