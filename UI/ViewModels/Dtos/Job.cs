namespace UI.ViewModels.Dtos
{
    public class Job : Calculable
    {
        public int Id { get; set; }
        public int DocumentCount { get; set; }
        public int Dimensions { get; set; }
        public Engine.Contracts.JobStatus Status { get; set; }
    }
}