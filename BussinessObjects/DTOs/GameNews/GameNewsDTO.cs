namespace BussinessObjects.DTOs.GameNews
{
    public class GameNewsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string BannerPath { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }

    public class CreateGameNewsRequest
    {
        public string Title { get; set; }
        public string BannerPath { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }

    public class UpdateGameNewsRequest
    {
        public string Title { get; set; }
        public string BannerPath { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }
}
