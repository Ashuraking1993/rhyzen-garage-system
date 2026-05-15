namespace Car_Rental.Models
{
    public class CarImage
    {
        public int Id { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; }

        public string ImagePath { get; set; }

        public bool IsMain { get; set; } = false;
       
    }
}
